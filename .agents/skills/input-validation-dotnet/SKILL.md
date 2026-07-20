---
name: input-validation-dotnet
description: Use esta skill para implementar ou revisar validação de entrada em APIs ASP.NET Core. Tipos, tamanhos e limites semânticos devem derivar das entidades, Value Objects e invariantes do domínio, sem expor entidades como contratos HTTP. Não use para inventar limites ausentes no modelo.
license: Apache-2.0
origin:
  repository: mukul975/Anthropic-Cybersecurity-Skills
  commit: 673da1f3b0b7be34ffc9624ef3858fe45f1c3bed
  path: skills/implementing-api-schema-validation-security/SKILL.md
modified: true
---

# Objetivo

Garantir que toda entrada seja validada na borda e novamente nas camadas responsáveis pelas invariantes, evitando overposting, mass assignment, ambiguidades de tipo, payloads inesperados e divergência entre contrato, domínio e persistência.

Esta adaptação remove exemplos de API Gateway e Python, elimina filtros frágeis de SQL/XSS por expressão regular e direciona a implementação para .NET 10, ASP.NET Core, System.Text.Json, contratos explícitos e domínio como fonte dos limites semânticos.

## Regra central

**Tipos, tamanhos e limites semânticos devem ser baseados nas entidades, Value Objects e invariantes do domínio.**

Isso não significa receber ou devolver entidades diretamente pela API.

A ordem de decisão é:

1. o domínio define o conceito e sua invariante;
2. a entidade ou Value Object expõe uma fábrica, método ou constante estável para a regra;
3. o contrato HTTP usa um tipo apropriado e valida limites compatíveis;
4. a aplicação cria ou altera o domínio por métodos intencionais;
5. o mapping do EF Core e o banco refletem a mesma restrição.

Quando a entidade ainda não definir um tamanho ou tipo, não invente um valor apenas para completar o validator. Registre a lacuna e resolva a decisão no domínio. Limites de transporte, como tamanho máximo do body ou upload, são controles separados e podem existir independentemente.

## Quando usar

- Criação ou alteração de endpoints.
- Inclusão de DTOs, commands, requests ou formulários.
- Mudança de entidade, Value Object, enum, nullable ou tamanho de coluna.
- Geração ou revisão de OpenAPI.
- Correção de overposting, mass assignment ou retorno excessivo.
- Validação de IDs relacionados e associações multitenant.

## Separação obrigatória de modelos

- Entidades de domínio não são contratos HTTP.
- Entidades EF Core não devem ser usadas como parâmetros de endpoint.
- Requests de criação, atualização total e atualização parcial devem ser tipos distintos quando suas regras diferirem.
- Responses devem listar explicitamente os campos públicos.
- Campos internos, como `tenant_id`, autoria técnica, flags administrativas, timestamps internos e dados de auditoria, não devem ser graváveis pelo cliente.

## Fontes de validação

Para cada campo, localize a fonte real:

| Restrição | Fonte preferencial |
|---|---|
| Tipo semântico | Value Object, entidade, enum ou assinatura do caso de uso |
| Tamanho mínimo/máximo | constante ou regra da entidade/Value Object |
| Obrigatoriedade | regra do caso de uso e estado da entidade |
| Intervalo numérico | invariante de domínio |
| Precisão monetária | tipo de valor e regra financeira |
| Formato de identificador | tipo forte, como `Guid` ou ID dedicado |
| Relação permitida | caso de uso, ownership e tenant |
| Tamanho máximo do request | política de transporte/infraestrutura |

Não derive regra de negócio apenas do tamanho atual de uma coluna. O mapping deve refletir o domínio, não ser sua única documentação.

## Tipos

- Use `Guid`, `DateOnly`, `TimeOnly`, `DateTimeOffset`, `decimal`, `bool` e enums quando esses forem os tipos do conceito.
- Não aceite string genérica para converter tardiamente em um tipo conhecido.
- Não use `double` para valores monetários.
- Diferencie ausência, valor vazio e valor nulo conforme o caso de uso.
- Não transforme enum inválido em valor padrão.
- Coleções devem ter limite explícito quando houver risco de abuso ou invariante conhecida.

## Validação em camadas

### 1. Transporte e desserialização

Avalie:

- content type permitido;
- tamanho máximo do body e uploads;
- profundidade JSON e coleções excessivas;
- tipos incompatíveis;
- propriedades desconhecidas;
- parâmetros obrigatórios ausentes.

Para contratos fechados, configure System.Text.Json para rejeitar membros não mapeados:

```csharp
using System.Text.Json.Serialization;

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
    options.SerializerOptions.RespectRequiredConstructorParameters = true;
});
```

Em MVC controllers, aplique a configuração equivalente em `AddJsonOptions`.

Antes de habilitar globalmente, execute testes de compatibilidade dos contratos existentes. Quando a API precisar aceitar extensões deliberadas, documente e isole essa exceção.

### 2. Contrato do caso de uso

Valide no request/command:

- campos obrigatórios;
- limites de texto e coleção;
- intervalos;
- combinações sintaticamente incompatíveis;
- formato de e-mail, telefone ou identificadores quando necessário;
- presença de campos proibidos.

Validators devem reutilizar constantes ou APIs do domínio, evitando duplicar números literais.

Exemplo conceitual:

```csharp
public sealed record CreatePetRequest(
    string Name,
    DateOnly? BirthDate);

public sealed class CreatePetRequestValidator : AbstractValidator<CreatePetRequest>
{
    public CreatePetRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(PetName.MaxLength);
    }
}
```

O exemplo pressupõe que `PetName.MaxLength` exista no domínio. Não crie a constante na camada HTTP apenas para satisfazer o validator.

### 3. Aplicação e domínio

A validação da borda não substitui:

- invariantes da entidade;
- autorização;
- ownership do dado;
- conflito de agenda;
- transições de estado;
- consistência entre registros;
- regras dependentes do estado atual.

A entidade ou Value Object deve continuar rejeitando estado inválido mesmo quando chamada fora da API.

### 4. Persistência

Use constraints, tipos e índices do banco para reforçar invariantes relevantes. Trate violações esperadas de forma controlada e não exponha detalhes SQL ao cliente.

## Segurança multitenant

- Não aceite `tenant_id` em contratos comuns de criação ou alteração.
- Ignore ou rejeite tentativas de enviar tenant por body, query, rota ou header conforme o contrato.
- Todo ID relacionado deve ser carregado no tenant autenticado antes da associação.
- Não use `AnyAsync(id)` sem filtro de tenant para validar existência.
- Evite mensagens que revelem que um recurso de outro tenant existe.
- Atualizações devem usar allowlist de propriedades e métodos de domínio, nunca binding automático sobre a entidade.

## Mass assignment e overposting

Rejeite padrões como:

```csharp
public Task Update(Pet entityFromBody)
```

ou mapeamento reflexivo que copie todas as propriedades com o mesmo nome.

Prefira requests específicos e aplicação explícita:

```csharp
pet.Rename(validatedName);
pet.UpdateBirthDate(request.BirthDate);
```

Campos de tenant, papel, autoria, status, preço aprovado, desconto, assinatura ou auditoria exigem casos de uso próprios quando forem alteráveis.

## SQL injection, XSS e conteúdo livre

- Não tente detectar SQL injection procurando palavras como `select`, aspas ou ponto e vírgula em texto legítimo.
- Use queries parametrizadas e APIs seguras do EF Core.
- Não remova caracteres válidos de nomes, observações clínicas ou endereços.
- Faça encoding no contexto de saída.
- HTML deve ser proibido por contrato ou sanitizado por biblioteca apropriada quando houver requisito real de rich text.
- Não confunda validação de formato com sanitização e autorização.

## Respostas de erro

- Use `ValidationProblemDetails` ou `HttpValidationProblemDetails`.
- Retorne nomes de campos e códigos estáveis quando necessário ao frontend.
- Não exponha stack trace, SQL, nomes internos de tabelas, entidades ou regras confidenciais.
- Diferencie erro de contrato, regra de negócio, conflito e autorização.

## OpenAPI

- Gere o schema a partir dos contratos reais sempre que possível.
- Tipos, required, formatos, enums, limites e nulabilidade devem corresponder ao comportamento executado.
- Não use OpenAPI como única validação em runtime.
- Adicione teste de contrato para impedir divergência relevante entre API e documentação.

## Testes obrigatórios

Para cada request relevante, cubra proporcionalmente:

- valor válido no limite mínimo e máximo;
- valor imediatamente abaixo e acima do limite;
- nulo, ausente, vazio e whitespace;
- tipo JSON incorreto;
- enum desconhecido;
- propriedade desconhecida;
- coleção vazia e acima do limite;
- campo proibido, incluindo `tenant_id`;
- identificador inexistente;
- identificador pertencente a outro tenant;
- associação entre tenants;
- resposta em Problem Details sem vazamento interno.

## Checklist de revisão

- O request é separado da entidade?
- O tipo representa o conceito real?
- Tamanhos vêm do domínio ou de decisão registrada?
- Create e update possuem regras próprias?
- Propriedades desconhecidas são tratadas conscientemente?
- Existe allowlist de campos alteráveis?
- O tenant não é controlado pelo payload?
- IDs relacionados são validados dentro do tenant?
- O domínio continua protegendo suas invariantes?
- OpenAPI, validator, mapping e testes estão coerentes?

## Restrições

- Não inventar tamanho de campo sem fonte.
- Não mover invariantes para controllers ou validators HTTP.
- Não expor entidades de domínio ou persistência.
- Não usar regex genérica como defesa contra SQL injection ou XSS.
- Não aceitar alteração parcial por reflexão sem allowlist.
- Não confiar na validação do frontend.
