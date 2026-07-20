# Clinica Veterinaria

Backend inicial de uma plataforma SaaS multitenant para clinicas veterinarias, petshops e servicos para pets.

O projeto nasce como um monolito modular em .NET 10 com um unico deploy. A entrega atual cria a fundacao tecnica minima: API HTTP protegida por JWT Bearer do Keycloak, contexto multitenant autenticado, AppHost Aspire para composicao local com PostgreSQL e Keycloak, EF Core com provider PostgreSQL, building blocks de observabilidade e testes de baseline. Modulos de negocio serao adicionados apenas quando houver uma fatia funcional concreta.

## Estrutura

```text
src/
  AppHost/PetShop.AppHost/
  Apps/PetShop.Api/
  BuildingBlocks/PetShop.Observability/
  BuildingBlocks/PetShop.Observability.AspNetCore/
tests/
  PetShop.UnitTests/
  PetShop.ArchitectureTests/
  PetShop.IntegrationTests/
  PetShop.AppHost.Tests/
  BuildingBlocks/PetShop.Observability.Tests/
```

## Projetos

- `PetShop.Api`: ASP.NET Core API com endpoint publico `/health`, endpoint protegido `/diagnostics`, contexto scoped de tenant autenticado, `PetShopDbContext` tecnico minimo e health check de PostgreSQL.
- `PetShop.AppHost`: composicao local Aspire contendo API, PostgreSQL e Keycloak declarativo para desenvolvimento.
- `PetShop.Observability`: building block agnostico de ASP.NET Core para correlation, contexto W3C, HTTP de saida e mensageria futura.
- `PetShop.Observability.AspNetCore`: adapter web para middleware de correlation e contexto de execucao.

## Decisoes preservadas

- O tenant autenticado vem exclusivamente da claim validada `tenant_id`.
- `TenantId` e um identificador forte baseado em `Guid`, com representacao futura PostgreSQL `uuid`.
- Nao existe tenant padrao, fallback ou tenant informado pelo frontend como autoridade.
- O Domain nao depende de ASP.NET Core, `HttpContext`, JWT ou claims.
- `CorrelationId` e independente de `TraceId`.
- HTTP usa `X-Correlation-Id`.
- `PetShop.Observability` nao depende de ASP.NET Core.
- O AppHost e apenas composicao local; ele sobe PostgreSQL e Keycloak para desenvolvimento, incluindo realm e client locais, sem definir broker, cache ou gateway.

As decisoes completas estao em:

- `docs/adrs/0001-multitenancy-claim-e-isolamento-por-linha.md`
- `docs/adrs/0002-library-propagacao-observabilidade.md`

## Requisitos

- .NET SDK 10
- Docker Desktop, Podman ou runtime OCI compativel para os containers locais do Aspire
- Acesso ao NuGet.org para restore

## Comandos

```bash
dotnet tool restore
dotnet restore ./ClinicaVeterinaria.slnx
dotnet build ./ClinicaVeterinaria.slnx --configuration Release --no-restore
dotnet test ./ClinicaVeterinaria.slnx --configuration Release --no-build --no-restore --settings ./coverlet.runsettings
```

Para executar a API diretamente, informe a connection string convencional `ConnectionStrings:petshop`:

```bash
ConnectionStrings__petshop="Host=localhost;Port=5432;Database=petshop;Username=petshop;Password=<senha>"
dotnet run --project ./src/Apps/PetShop.Api/PetShop.Api.csproj
```

Em `Development`, a API usa a configuracao local versionada de JWT:

- `Authentication:Authority`: `http://localhost:8080/realms/petshop-local`;
- `Authentication:Audience`: `petshop-api`;
- `Authentication:RoleClientId`: `petshop-api`;
- `Authentication:RequiredRole`: `petshop.access`;
- `Authentication:RequireHttpsMetadata`: `false`.

Fora de `Development`, configure esses valores pelo ambiente ou secret store do runtime. Em producao, a autoridade deve usar HTTPS e a validacao de issuer, audience, assinatura e lifetime permanece habilitada.

Para executar a composicao local Aspire:

```bash
dotnet run --project ./src/AppHost/PetShop.AppHost/PetShop.AppHost.csproj
```

Esse comando inicia a API, o PostgreSQL e o Keycloak e disponibiliza o Aspire Dashboard. O endereco do dashboard aparece no console do AppHost durante a inicializacao.

## Persistencia PostgreSQL e EF Core

A API registra um `PetShopDbContext` tecnico minimo em `src/Apps/PetShop.Api/Infrastructure/Persistence/`. Ele nao possui `DbSet`, entidades de negocio, repositories genericos ou Unit of Work customizado.

Configuracao:

- A connection string se chama `petshop`.
- No Aspire local, `PetShop.AppHost` injeta essa connection string na API por `WithReference(petshopDatabase)`.
- Fora do Aspire, use configuracao padrao do ASP.NET Core, como variavel de ambiente `ConnectionStrings__petshop`, user-secrets ou arquivo local nao versionado.
- O provider EF Core e `Npgsql.EntityFrameworkCore.PostgreSQL`.
- A convencao relacional usa `snake_case` e a tabela de historico de migrations se chama `__ef_migrations_history`.
- O endpoint `/health` inclui o check `postgresql`, baseado no `PetShopDbContext`.

Comandos de migrations:

```bash
dotnet tool restore

ConnectionStrings__petshop="Host=localhost;Port=5432;Database=petshop;Username=petshop;Password=<senha>" \
dotnet dotnet-ef migrations add NomeDaMigration \
  --project ./src/Apps/PetShop.Api/PetShop.Api.csproj \
  --startup-project ./src/Apps/PetShop.Api/PetShop.Api.csproj \
  --output-dir Infrastructure/Persistence/Migrations \
  --context PetShopDbContext

ConnectionStrings__petshop="Host=localhost;Port=5432;Database=petshop;Username=petshop;Password=<senha>" \
dotnet dotnet-ef database update \
  --project ./src/Apps/PetShop.Api/PetShop.Api.csproj \
  --startup-project ./src/Apps/PetShop.Api/PetShop.Api.csproj \
  --context PetShopDbContext
```

Politica de migrations:

- Em desenvolvimento, novas alteracoes de schema devem gerar migrations versionadas junto com a mudanca de codigo.
- A API nao aplica migrations automaticamente no startup.
- Em producao, gere script idempotente e aplique pelo processo de release do ambiente:

```bash
dotnet dotnet-ef migrations script --idempotent \
  --project ./src/Apps/PetShop.Api/PetShop.Api.csproj \
  --startup-project ./src/Apps/PetShop.Api/PetShop.Api.csproj \
  --context PetShopDbContext \
  --output ./artifacts/sql/petshop-migrations.sql
```

Ao introduzir a primeira tabela de negocio, ela deve possuir `tenant_id` obrigatorio conforme a ADR-0001. Query filters, interceptors de tenant e Row-Level Security permanecem fora desta fundacao ate existir uma decisao especifica.

## Contratos HTTP

A API padroniza respostas de erro com `application/problem+json`. Quando aplicavel, o corpo contem:

- `type`;
- `title`;
- `status`;
- `detail`;
- `instance`;
- `code`;
- `correlationId`.

O backend nunca deve expor stack trace, SQL, connection string, token, segredo ou dump de claims em respostas de erro. O `correlationId` usa o `X-Correlation-Id` recebido quando ele for um GUID valido; caso contrario, a API gera um novo valor e o devolve no header de resposta.

Codigos estaveis documentados:

| HTTP | `code` | Semantica |
| --- | --- | --- |
| 400 | `request.invalid` | Entrada invalida, payload malformado ou contrato HTTP nao processavel. |
| 401 | `auth.unauthenticated` | Bearer token ausente, malformado, expirado, com issuer, audience ou assinatura invalidos. |
| 403 | `auth.forbidden` | Principal autenticado sem permissao para o recurso. |
| 403 | `identity.tenant_required` | Token autenticado sem exatamente uma claim `tenant_id` valida. |
| 404 | `resource.not_found` | Recurso inexistente ou nao visivel no escopo atual. |
| 409 | `resource.conflict` | Conflito de estado futuro, como concorrencia de agenda. |
| 500 | `server.unexpected` | Falha inesperada, sem detalhes internos no contrato publico. |

Em `Development`, o documento OpenAPI fica disponivel em:

```text
/openapi/v1.json
```

O documento descreve JWT Bearer, o header opcional `X-Correlation-Id` e respostas `application/problem+json`.

Health checks:

- `/health/live`: liveness local, nao depende de PostgreSQL nem de servicos externos;
- `/health/ready`: readiness, considera o PostgreSQL;
- `/health`: alias de readiness preservado para compatibilidade local.

## Ambiente local Aspire

Recursos locais:

- `petshop-api`: API ASP.NET Core do monolito.
- `postgres`: servidor PostgreSQL em container, com credenciais geradas pelo Aspire.
- `petshop`: banco logico criado no PostgreSQL local e referenciado pela API.
- `keycloak`: Keycloak em container, exposto em porta estavel `8080` para evitar instabilidade de cookies OIDC durante o desenvolvimento.

O AppHost usa `WaitFor` para aguardar PostgreSQL e Keycloak antes de iniciar a API, e `WithReference` para disponibilizar as informacoes dos recursos para a API. Nesta entrega a API consome o PostgreSQL via EF Core e valida JWT Bearer emitido pelo realm local do Keycloak.

Configuracao declarativa do Keycloak:

- Realm: `petshop-local`.
- Issuer local esperado nos tokens: `http://localhost:8080/realms/petshop-local`.
- Client da API: `petshop-api`, publico e habilitado para direct access grants apenas para desenvolvimento local.
- Audience emitida no access token: `petshop-api`.
- Role minima: client role `petshop.access` em `resource_access.petshop-api.roles`.
- Usuario descartavel: `local.petshop.user`.
- Senha descartavel local: `local-dev-password`.
- Tenant local estavel: `11111111-1111-4111-8111-111111111111`.
- Claim emitida no access token: `tenant_id` no formato `Guid`.

O realm versionado fica em `src/AppHost/PetShop.AppHost/keycloak/realms/petshop-local-realm.json` e e importado automaticamente pelo AppHost com `WithRealmImport("keycloak/realms")`. Essa configuracao e apenas local, nao representa senha real nem politica produtiva de identidade.

Para obter um access token local depois que o AppHost estiver rodando:

```bash
curl --request POST \
  --url http://localhost:8080/realms/petshop-local/protocol/openid-connect/token \
  --header "Content-Type: application/x-www-form-urlencoded" \
  --data "grant_type=password" \
  --data "client_id=petshop-api" \
  --data "username=local.petshop.user" \
  --data "password=local-dev-password"
```

Para inspecionar as claims sem colar o token em servicos externos, decodifique localmente o segundo segmento do JWT. O access token deve conter:

- `iss` igual a `http://localhost:8080/realms/petshop-local`;
- `aud` contendo `petshop-api`;
- `resource_access.petshop-api.roles` contendo `petshop.access`;
- `tenant_id` igual a `11111111-1111-4111-8111-111111111111`.

A API valida tokens Bearer emitidos pelo realm local, exige audience `petshop-api`, lifetime valido, assinatura via metadata/JWKS do Keycloak e a policy explicita `PetShop.DiagnosticsAccess` para o endpoint `/diagnostics`. Depois de `UseAuthentication`, a API resolve um `ITenantContext` scoped exclusivamente da claim `tenant_id`; claim ausente, vazia, duplicada ou invalida resulta em `403 Forbidden`, sem tenant padrao ou fallback. A policy exige usuario autenticado, tenant valido e client role `petshop.access` em `resource_access.petshop-api.roles`. O endpoint retorna o `tenantId` autenticado e nao retorna token nem dump de claims.

Jobs, workers e processamentos futuros sem `HttpContext` nao devem tentar acessar claims. O item de trabalho tenant-owned deve carregar o `tenant_id` previamente validado e abrir a execucao com um contexto explicito, como `ExplicitTenantContext`, dentro do escopo daquele tenant.

Smoke test local, depois que a API e o Keycloak estiverem rodando:

```powershell
./scripts/smoke-keycloak-auth.ps1 -ApiBaseUrl "http://localhost:5000"
```

Se a API estiver em outra URL, informe o endereco exibido pelo AppHost em `-ApiBaseUrl`. O smoke test valida `401` sem token, `200` com um access token local autorizado, preservacao de `X-Correlation-Id` e tenant autenticado.

Volumes persistentes:

- PostgreSQL usa volume Docker gerenciado pelo Aspire para preservar dados do servidor local.
- Keycloak usa volume Docker gerenciado pelo Aspire para preservar dados e credenciais administrativas locais.
- Nao versione nem copie os segredos gerados pelo Aspire. Eles ficam no secret store local do AppHost.

Reset do ambiente local:

1. Pare o AppHost.
2. Remova os volumes do PostgreSQL e Keycloak pelo Docker Desktop/Podman ou pela CLI do runtime local.
3. Execute novamente `dotnet run --project ./src/AppHost/PetShop.AppHost/PetShop.AppHost.csproj`.

Faca esse reset tambem se os logs mostrarem falha de autenticacao no PostgreSQL ou Keycloak depois de uma interrupcao forcada, regeneracao de secrets locais ou alteracao do JSON do realm, pois os volumes preservam credenciais internas do container e realms ja importados.

Separacao local/producao:

- Aspire e usado somente para composicao e observabilidade do ambiente local.
- Producao nao deve depender do AppHost como runtime ou IaC obrigatoria.
- PostgreSQL e Keycloak produtivos devem ser provisionados pela estrategia de infraestrutura propria do ambiente.

## Observabilidade

A API registra OpenTelemetry para traces, metricas e logs com `service.name` estavel `PetShop.Api`.

Instrumentacoes habilitadas:

- ASP.NET Core;
- `HttpClient`;
- metricas de runtime .NET.

O exporter OTLP e habilitado somente quando houver endpoint configurado por `OpenTelemetry:Otlp:Endpoint`, `OpenTelemetry__Otlp__Endpoint` ou pela variavel padrao `OTEL_EXPORTER_OTLP_ENDPOINT`. Ao executar pelo AppHost Aspire, o endpoint OTLP do dashboard e injetado no processo da API e logs, traces e metricas passam a aparecer no Aspire Dashboard.

O middleware de entrada aceita `X-Correlation-Id` valido ou cria um novo GUID, devolve o mesmo header na resposta e adiciona `correlation_id` e `tenant_id` como tags da Activity e scope de log. Esses identificadores nao sao adicionados ao W3C baggage; mensagens e jobs usam os headers canonicos `correlation_id`, `tenant_id`, `traceparent`, `tracestate` e `baggage` pelo building block `PetShop.Observability`.

## Escopo ainda nao implementado

- Entidades e modulos de negocio tenant-owned.
- Query filters, interceptors de tenant, enforcement persistente e Row-Level Security.
- Modulos de negocio como cadastro, pets, agenda, atendimento ou cobranca.
- Broker, Redis, API Gateway, microsservicos ou multiplos bancos.
