---
name: privacy-audit-logging
description: Use esta skill ao criar ou revisar logs, traces, métricas, eventos de auditoria, acessos administrativos, suporte, exportações, downloads, alterações críticas ou evidências de incidentes que possam envolver dados pessoais. Separe observabilidade técnica de auditoria funcional, minimize conteúdo, evite dados pessoais e alta cardinalidade, preserve tenant e correlação e aplique retenção proporcional. Não use logs comuns como prontuário, armazenamento de documentos ou prova jurídica definitiva.
---

# Auditoria e logging com proteção de dados

## Objetivo

Produzir evidência suficiente para segurança, investigação, rastreabilidade e suporte sem transformar a plataforma de observabilidade em uma cópia descontrolada dos dados pessoais e clínicos da aplicação.

Esta skill complementa:

- `lgpd-backend-dotnet`;
- `multitenancy-dotnet`;
- `configuring-opentelemetry-dotnet`.

## Distinções obrigatórias

### Log técnico

Usado para diagnóstico operacional, erros e desempenho. Deve ser minimizado, estruturado e possuir retenção curta ou proporcional.

### Trace

Usado para acompanhar uma operação distribuída. Deve carregar contexto técnico e correlação, não conteúdo pessoal ou clínico.

### Métrica

Usada para agregação e alertas. Não deve conter identificadores de alta cardinalidade ou dados pessoais como labels.

### Auditoria funcional

Registra quem executou uma ação crítica, sobre qual recurso, quando, com qual resultado e justificativa. Deve ser persistida e protegida de maneira apropriada ao risco.

### Registro clínico ou documento

É dado de domínio e não deve ser armazenado em logs. Prontuário, prescrição, laudo, consentimento e anexos precisam de persistência própria, autoria, integridade e regras de acesso.

## Quando usar

Use esta skill em:

- middleware de logging;
- filtros de exceção;
- OpenTelemetry;
- logs de EF Core e SQL;
- auditoria de prontuário e documentos;
- alterações de permissões;
- acesso de suporte ou impersonation;
- exportação e download;
- exclusão, anonimização ou bloqueio;
- autenticação e segurança;
- integrações externas;
- resposta a incidentes;
- dashboards e alertas.

## Processo obrigatório

### 1. Classifique o evento

Antes de registrar, determine:

- É diagnóstico técnico ou auditoria funcional?
- A informação já existe em persistência de domínio?
- Qual decisão ou investigação depende desse registro?
- Qual é o menor conjunto de atributos necessário?
- Qual é a retenção esperada?
- Quem pode consultar?
- O evento pertence a um tenant?

Não duplique bodies, documentos ou entidades inteiras “para facilitar debug”.

### 2. Defina uma allowlist de atributos

Prefira atributos explicitamente aprovados.

Exemplos aceitáveis conforme o contexto:

```text
correlation_id
trace_id
span_id
operation
http_method
route_template
status_code
outcome
duration_ms
error_code
resource_type
resource_id técnico
action
actor_user_id técnico
tenant_id quando necessário e protegido
occurred_at
```

Evite serialização automática de DTOs, exceptions enriquecidas com input ou scopes contendo objetos completos.

### 3. Nunca registre conteúdo proibido por padrão

Não registre em logs, traces ou métricas comuns:

- senhas, hashes de senha ou respostas de recuperação;
- access tokens, refresh tokens, cookies ou API keys;
- CPF, RG, endereço ou documentos completos;
- dados completos de cartão ou conta bancária;
- bodies integrais de requests e responses;
- prontuários, prescrições, diagnósticos, laudos ou evolução;
- anexos, imagens ou documentos codificados;
- conteúdo integral de e-mail, SMS ou WhatsApp;
- termos de consentimento completos;
- texto livre inserido pelo usuário;
- query strings com dados pessoais;
- cabeçalhos de autorização;
- connection strings ou secrets.

Quando um erro depende de valor inválido, registre código, tipo, tamanho, formato ou hash não reversível quando justificável, nunca o valor bruto por conveniência.

### 4. Use rotas parametrizadas

Registre o template:

```text
/api/customers/{customerId}
```

Evite registrar a URL completa quando ela contiver identificadores ou dados de busca.

Sanitize query string. Termos de busca podem conter nome, CPF, telefone ou conteúdo clínico.

### 5. Proteja multitenancy

- O `tenant_id` usado em auditoria deve vir do contexto autenticado ou da unidade de trabalho confiável.
- Não aceite tenant de input público como atributo de autoridade.
- Jobs e consumidores devem propagar tenant explicitamente.
- Pesquisas e consultas de auditoria devem ser autorizadas e limitadas ao tenant.
- Operações cross-tenant precisam de papel administrativo específico e trilha reforçada.
- Falhas de isolamento e tentativas de acesso devem ser registradas sem revelar a existência do recurso ao cliente.

`tenant_id` pode ser útil em logs restritos, mas não deve ser label de métrica de alta cardinalidade.

### 6. Modele auditoria funcional

Estrutura mínima sugerida:

```csharp
public sealed class AuditEvent
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid? ActorUserId { get; init; }
    public string Action { get; init; } = null!;
    public string ResourceType { get; init; } = null!;
    public string ResourceId { get; init; } = null!;
    public AuditOutcome Outcome { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public string? CorrelationId { get; init; }
    public string? ReasonCode { get; init; }
}
```

Use códigos ou referências para justificativas. Não armazene prontuário, parecer ou documento completo no evento.

### 7. Audite ações críticas

Avalie auditoria para:

- visualização de prontuário ou documento restrito;
- criação, correção, assinatura ou complementação de registro clínico;
- emissão e download de documento;
- exportação de dados;
- alteração de permissões;
- acesso administrativo cross-tenant;
- início e término de sessão de suporte;
- alteração de preferências e consentimentos;
- anonimização, bloqueio e exclusão;
- cancelamento, estorno e ações financeiras críticas;
- revogação de credenciais;
- falhas repetidas de autenticação;
- associação rejeitada entre tenants.

Não audite toda leitura trivial sem análise. Alto volume pode reduzir utilidade, elevar custo e criar mais superfície de exposição.

### 8. Registre autoria real em suporte

Em impersonation ou suporte:

- preserve o usuário real do operador;
- preserve o usuário ou contexto representado, quando existir;
- registre justificativa ou ticket;
- registre início, término e expiração;
- aplique MFA e permissão específica;
- não permita conta global compartilhada;
- não esconda a identidade real atrás do usuário impersonado.

Exemplo conceitual:

```text
actor_user_id = suporte real
represented_user_id = usuário do tenant
support_session_id = sessão autorizada
action = clinical-record.read
reason_code = SUPPORT_TICKET
```

### 9. Separe atributos de erro

Use códigos estáveis:

```text
CUSTOMER_NOT_FOUND
TENANT_CONTEXT_MISSING
CROSS_TENANT_ASSOCIATION_REJECTED
EXPORT_EXPIRED
ATTACHMENT_MALWARE_DETECTED
```

Não dependa da mensagem completa de exception como contrato de dashboard ou auditoria.

Exceptions inesperadas devem ser capturadas sem ecoar request completo, headers ou SQL com parâmetros pessoais.

### 10. Configure EF Core e banco com cautela

- Não habilite sensitive data logging em produção.
- Revise logs de comandos SQL e parâmetros.
- Não registre connection strings.
- Evite dumps automáticos de entidades em falhas de persistência.
- Trate deadlocks e conflitos por código e contexto técnico.
- Proteja logs de migrations e ferramentas administrativas.

### 11. Projete métricas agregadas

Exemplos apropriados:

```text
http.server.request.duration
privacy.requests.completed
privacy.exports.generated
support.sessions.active
cross_tenant_access.rejected
attachments.malware_detected
```

Labels adequadas tendem a ser de baixa cardinalidade:

```text
operation
status_code
outcome
request_type
channel
service_name
environment
```

Não use:

```text
tenant_id
customer_id
person_id
email
cpf
phone
pet_name
correlation_id
```

como labels de métrica.

### 12. Defina retenção e acesso

Para cada destino de observabilidade ou auditoria, documente:

- finalidade;
- atributos permitidos;
- retenção;
- acesso por papel;
- região de armazenamento;
- fornecedor;
- exportação e exclusão;
- resposta a incidente;
- integridade e backup.

Logs técnicos e auditoria funcional podem ter retenções distintas. Não escolha retenção infinita “porque pode ser útil”.

### 13. Preserve evidências de incidente

O sistema deve permitir:

- localizar eventos por correlação, tempo, serviço e operação;
- identificar o tenant afetado de forma restrita;
- reconstruir linha do tempo;
- distinguir falha, abuso, acesso legítimo e acesso administrativo;
- preservar evidências relevantes sob legal hold ou investigação;
- limitar acesso à investigação;
- registrar quem exportou evidências.

Evite ampliar o incidente copiando grandes volumes de logs com dados pessoais para canais de chat, tickets públicos ou estações locais.

## Checklist por componente

### Middleware HTTP

- Registra template de rota, não URL sensível?
- Remove query string ou aplica allowlist?
- Exclui headers de autenticação e cookies?
- Limita tamanho das mensagens?
- Não captura body por padrão?
- Mantém correlation ID e trace context?

### OpenTelemetry

- Spans não carregam dados pessoais?
- Baggage não possui dados pessoais ou secrets?
- Exporter e collector estão protegidos?
- Sampling preserva investigação sem coletar payload?
- Atributos seguem convenções estáveis?

### Auditoria

- Evento é append-only ou protegido contra alteração indevida?
- Actor, tenant, ação, recurso, resultado e horário estão presentes?
- O conteúdo é mínimo?
- Consulta exige permissão específica?
- Existe política de retenção?

### Métricas

- Labels têm baixa cardinalidade?
- Nenhum identificador pessoal ou tenant aparece como label?
- Dashboards usam agregação?
- Alertas não incluem conteúdo sensível na mensagem?

## Testes mínimos

1. Token e cookie nunca aparecem em logs de request.
2. Request com CPF ou telefone não ecoa o valor em erro.
3. Prontuário não aparece em log, trace ou exception.
4. Tenant B não consulta auditoria do Tenant A.
5. Evento cross-tenant rejeitado preserva evidência sem revelar recurso.
6. Impersonation registra operador real e sessão de suporte.
7. Métricas não possuem labels de alta cardinalidade proibidos.
8. Sensitive data logging do EF Core permanece desabilitado em produção.
9. Exportação e download geram auditoria.
10. Falha do exporter não interrompe silenciosamente a operação de negócio nem provoca log recursivo contendo payload.

## Sinais de risco

Interrompa e revise quando encontrar:

- `LogInformation("{@Request}", request)` em DTO público;
- logging de body habilitado globalmente;
- `EnableSensitiveDataLogging()` em produção;
- CPF, e-mail ou `tenant_id` como label Prometheus;
- prontuário serializado em exception;
- conta compartilhada de suporte;
- auditoria sem actor ou tenant;
- URL permanente para download de logs ou exportações;
- retenção infinita sem justificativa;
- dashboards ou alertas enviando dados pessoais para canais externos.

## Saída esperada

Ao concluir, informe:

- eventos técnicos e funcionais criados ou alterados;
- atributos permitidos e removidos;
- tratamento de tenant e correlação;
- ações críticas auditadas;
- labels de métricas revisadas;
- retenção e acesso definidos;
- testes executados;
- risco residual e decisões pendentes.

## Referências oficiais e técnicas

Data de consulta da baseline: 20 de julho de 2026.

- LGPD consolidada, especialmente arts. 6, 37, 46 a 49: https://www.planalto.gov.br/ccivil_03/_ato2015-2018/2018/lei/l13709compilado.htm
- ANPD — segurança para agentes de pequeno porte: https://www.gov.br/anpd/pt-br/centrais-de-conteudo/materiais-educativos-e-publicacoes/guia-orientativo-sobre-seguranca-da-informacao-para-agentes-de-tratamento-de-pequeno-porte
- OWASP Logging Cheat Sheet: https://cheatsheetseries.owasp.org/cheatsheets/Logging_Cheat_Sheet.html
- OpenTelemetry semantic conventions: https://opentelemetry.io/docs/specs/semconv/
