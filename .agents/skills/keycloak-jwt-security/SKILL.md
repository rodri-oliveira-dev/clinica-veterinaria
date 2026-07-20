---
name: keycloak-jwt-security
description: Use esta skill para configurar ou revisar autenticação e autorização JWT/OIDC entre Keycloak e APIs ASP.NET Core, incluindo issuer, audience, JWKS, roles, tenant_id e testes defensivos. Não use para brute force, token forgery ou pentest sem autorização.
license: Apache-2.0
origin:
  repository: mukul975/Anthropic-Cybersecurity-Skills
  commit: 673da1f3b0b7be34ffc9624ef3858fe45f1c3bed
  path: skills/testing-jwt-token-security/SKILL.md
modified: true
---

# Objetivo

Garantir que a API confie somente em access tokens emitidos pelo realm esperado do Keycloak, destinados à API correta, dentro da validade e com claims autorizadas, sem permitir que o cliente controle o tenant.

Esta adaptação converte uma skill ofensiva de teste de JWT em um guia defensivo para Keycloak e ASP.NET Core. Foram removidos brute force, hashcat, Burp, `jwt_tool`, ataques ativos e exemplos de falsificação de tokens.

## Quando usar

- Introdução ou atualização do Keycloak.
- Configuração de `AddJwtBearer`.
- Mudança de realm, client, audience, roles, scopes ou protocol mappers.
- Inclusão da claim `tenant_id`.
- Criação de políticas de autorização.
- Testes de autenticação, autorização e isolamento multitenant.
- Rotação de chaves ou mudança de URL pública do Keycloak.

## Modelo de confiança

- O Keycloak é o Authorization Server/OpenID Provider.
- A API é um resource server e valida access tokens.
- O frontend não é autoridade para roles, permissions ou tenant.
- A assinatura é validada pelas chaves publicadas no JWKS do realm.
- `iss`, `aud`, expiração e assinatura devem ser validados.
- ID token não deve ser aceito como access token da API.
- A API não emite nem re-assina tokens do usuário.

## Configuração esperada no Keycloak

### Realm e URLs

- Use um realm dedicado ao contexto da plataforma ou uma decisão equivalente documentada.
- Configure hostname e proxy corretamente para que o `iss` emitido corresponda à URL pública canônica.
- Não aceite múltiplos issuers de ambientes diferentes por conveniência.
- Desenvolvimento local pode usar HTTP somente em ambiente isolado e configuração explícita.

### Client da API

- Represente a API por um client/resource identificável.
- Configure audience explícita no access token, normalmente pelo Audience protocol mapper.
- Desabilite `Full Scope Allowed` em produção quando não houver justificativa.
- Limite role scope mappings ao necessário para a API.
- Não coloque client secret de client confidencial no frontend.

### Claim `tenant_id`

A claim deve:

- existir no access token usado pela API;
- possuir nome exato `tenant_id`;
- conter um identificador válido no formato definido pelo domínio;
- representar o tenant autorizado para a sessão/operação comum;
- ser emitida por mapper ou fluxo confiável do Keycloak;
- não ser derivada de body, query, rota, header customizado ou escolha livre do frontend.

A origem organizacional da claim precisa ser decidida antes da implementação, por exemplo atributo gerenciado, membership validada ou seleção controlada de contexto. Não permita que o usuário edite diretamente o valor que concede acesso ao tenant.

Para usuários com acesso a vários tenants, crie um fluxo explícito de seleção/troca de contexto e emissão de token correspondente. Não coloque uma lista irrestrita de tenants no token e deixe a API escolher informalmente.

### Roles e permissions

O Keycloak inclui realm roles em `realm_access` e client roles em `resource_access` por padrão. A aplicação deve escolher conscientemente uma estratégia:

1. emitir claims planas e específicas para a API por protocol mapper; ou
2. centralizar o parsing das claims aninhadas em um único componente testado.

Prefira permissões ou roles específicas da API. Não trate toda realm role como autorização automática dentro da plataforma.

## Configuração ASP.NET Core

Use `Microsoft.AspNetCore.Authentication.JwtBearer` e configuração externa:

```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = configuration["Authentication:Authority"];
        options.Audience = configuration["Authentication:Audience"];
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = !environment.IsDevelopment();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "preferred_username"
        };
    });
```

O exemplo é direcional. Use os nomes reais de configuração e não copie sem validar o token emitido pelo realm.

### Regras obrigatórias

- Não desabilite `ValidateAudience` para contornar configuração do Keycloak.
- Não defina `ValidateIssuerSigningKey = false`.
- Não aceite algoritmo `none` ou HMAC quando a confiança foi desenhada para chaves assimétricas do Keycloak.
- Não faça download de chave a partir de `jku` ou `x5u` arbitrário do token.
- Use metadata/JWKS da Authority configurada.
- Mantenha clock skew pequeno e explícito.
- Não registre access token ou refresh token.
- Não use claims não validadas antes da autenticação concluir.

## Resolução do tenant

Após autenticação bem-sucedida:

1. leia exclusivamente a claim `tenant_id` da principal validada;
2. valide presença e formato;
3. crie o contexto de tenant na borda;
4. propague o identificador explicitamente para Application e Infrastructure;
5. rejeite a requisição quando a claim estiver ausente ou inválida;
6. não use fallback ou tenant padrão.

O Domain não deve depender de `HttpContext`, JWT, claims ou Keycloak.

## Autenticação versus autorização

- Token válido comprova identidade e claims emitidas; não comprova acesso a todo recurso.
- Policies devem verificar role/permission apropriada.
- O caso de uso deve verificar ownership, tenant, estado e regras de negócio.
- IDs em rota não substituem autorização.
- Operações administrativas cross-tenant exigem esquema, policy e auditoria próprios.
- Um usuário com role administrativa no Keycloak não deve ganhar acesso cross-tenant sem uma decisão explícita da aplicação.

## Testes defensivos obrigatórios

### Validação do token

- sem token resulta em `401`;
- token malformado resulta em `401`;
- assinatura inválida resulta em `401`;
- issuer diferente resulta em `401`;
- audience ausente ou diferente resulta em `401`;
- token expirado resulta em `401`;
- token ainda não válido resulta em `401`;
- token de outro realm ou ambiente resulta em `401`;
- ID token usado como bearer é rejeitado;
- `kid` desconhecido é rejeitado e rotação válida de chave continua funcionando via JWKS.

### Claims e autorização

- `tenant_id` ausente ou inválido impede o caso de uso;
- role/permission ausente resulta em `403` quando o usuário está autenticado;
- role de outro client não concede permissão por acidente;
- alteração de `tenant_id`, roles ou audience sem assinatura válida é rejeitada;
- tenant informado no body, query, rota ou header não substitui a claim;
- token do tenant B não lê, altera, exclui nem associa dados do tenant A;
- erros não confirmam indevidamente a existência do recurso de outro tenant.

### Ciclo de vida

- teste o comportamento durante rotação de chave;
- teste indisponibilidade temporária do Keycloak e cache de metadata/JWKS;
- defina a expectativa para logout, revogação e troca de senha;
- não assuma que um access token JWT autocontido é invalidado instantaneamente antes de expirar;
- use tempos de vida coerentes com o risco e o fluxo de refresh.

## Ambiente de testes

Quando testes de integração de identidade forem necessários:

- use uma instância isolada do Keycloak, container ou ambiente dedicado;
- importe configuração mínima e versionada sem secrets reais;
- crie usuários, tenants e roles sintéticos;
- gere tokens pelo fluxo OIDC real em vez de assinar JWT manualmente para todos os cenários;
- use doubles apenas em testes unitários fora do boundary de autenticação;
- não faça testes destrutivos contra o realm compartilhado.

## Observabilidade e auditoria

Pode registrar, com proteção adequada:

- resultado de autenticação sem token;
- issuer esperado versus categoria de falha, sem conteúdo do token;
- subject ou identificador interno quando necessário e permitido;
- tenant resolvido como contexto estruturado de log, nunca como label de métrica;
- policy negada;
- operação administrativa cross-tenant.

Não registre:

- token completo;
- refresh token;
- client secret;
- authorization code;
- conteúdo integral das claims;
- PII desnecessária.

## Checklist

- Authority corresponde exatamente ao `iss` público?
- Audience está presente no access token e validada?
- API rejeita token de outro client/realm/ambiente?
- `tenant_id` vem de mapper confiável?
- Não existe fallback de tenant?
- Roles/permissions usadas pertencem à API?
- Policies e ownership são testados separadamente?
- Key rotation e JWKS foram considerados?
- Secrets estão fora do repositório?
- Logs não contêm tokens?

## Restrições

- Não executar ataques de falsificação, confusion ou brute force.
- Não colar token real em ferramentas online.
- Não desabilitar validação para fazer integração funcionar.
- Não confiar em claims do ID token para autorizar a API.
- Não modelar tenant como parâmetro livre do cliente.
- Não usar Keycloak como substituto das regras de autorização do domínio.
