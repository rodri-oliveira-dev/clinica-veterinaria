# Clinica Veterinaria

Backend inicial de uma plataforma SaaS multitenant para clinicas veterinarias, petshops e servicos para pets.

O projeto nasce como um monolito modular em .NET 10 com um unico deploy. A entrega atual cria a fundacao tecnica minima: API HTTP, AppHost Aspire para composicao local com PostgreSQL e Keycloak, EF Core com provider PostgreSQL, building blocks de observabilidade e testes de baseline. Modulos de negocio, autenticacao JWT e multitenancy tecnico serao adicionados apenas quando houver uma fatia funcional concreta.

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

- `PetShop.Api`: ASP.NET Core API com endpoints minimos `/health` e `/diagnostics`, `PetShopDbContext` tecnico minimo e health check de PostgreSQL.
- `PetShop.AppHost`: composicao local Aspire contendo API, PostgreSQL e Keycloak para desenvolvimento.
- `PetShop.Observability`: building block agnostico de ASP.NET Core para correlation, contexto W3C, HTTP de saida e mensageria futura.
- `PetShop.Observability.AspNetCore`: adapter web para middleware de correlation e contexto de execucao.

## Decisoes preservadas

- O tenant autenticado vem exclusivamente da claim validada `tenant_id`.
- Nao existe tenant padrao, fallback ou tenant informado pelo frontend como autoridade.
- O Domain nao depende de ASP.NET Core, `HttpContext`, JWT ou claims.
- `CorrelationId` e independente de `TraceId`.
- HTTP usa `X-Correlation-Id`.
- `PetShop.Observability` nao depende de ASP.NET Core.
- O AppHost e apenas composicao local; ele sobe PostgreSQL e Keycloak para desenvolvimento, mas nao define EF Core, realm, client, JWT, broker, cache ou gateway.

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

## Ambiente local Aspire

Recursos locais:

- `petshop-api`: API ASP.NET Core do monolito.
- `postgres`: servidor PostgreSQL em container, com credenciais geradas pelo Aspire.
- `petshop`: banco logico criado no PostgreSQL local e referenciado pela API.
- `keycloak`: Keycloak em container, exposto em porta estavel `8080` para evitar instabilidade de cookies OIDC durante o desenvolvimento.

O AppHost usa `WaitFor` para aguardar PostgreSQL e Keycloak antes de iniciar a API, e `WithReference` para disponibilizar as informacoes dos recursos para a API. Nesta entrega a API consome o PostgreSQL via EF Core, mas ainda nao autentica via Keycloak; realm, client e JWT permanecem fora do escopo.

Volumes persistentes:

- PostgreSQL usa volume Docker gerenciado pelo Aspire para preservar dados do servidor local.
- Keycloak usa volume Docker gerenciado pelo Aspire para preservar dados e credenciais administrativas locais.
- Nao versione nem copie os segredos gerados pelo Aspire. Eles ficam no secret store local do AppHost.

Reset do ambiente local:

1. Pare o AppHost.
2. Remova os volumes do PostgreSQL e Keycloak pelo Docker Desktop/Podman ou pela CLI do runtime local.
3. Execute novamente `dotnet run --project ./src/AppHost/PetShop.AppHost/PetShop.AppHost.csproj`.

Faca esse reset tambem se os logs mostrarem falha de autenticacao no PostgreSQL ou Keycloak depois de uma interrupcao forcada ou regeneracao de secrets locais, pois os volumes preservam credenciais internas do container.

Separacao local/producao:

- Aspire e usado somente para composicao e observabilidade do ambiente local.
- Producao nao deve depender do AppHost como runtime ou IaC obrigatoria.
- PostgreSQL e Keycloak produtivos devem ser provisionados pela estrategia de infraestrutura propria do ambiente.

## Escopo ainda nao implementado

- Realm, client, JWT, autenticacao e autorizacao.
- Implementacao tecnica de multitenancy.
- Query filters, interceptors de tenant e Row-Level Security.
- OpenTelemetry completo com exporter OTLP.
- Modulos de negocio como cadastro, pets, agenda, atendimento ou cobranca.
- Broker, Redis, API Gateway, microsservicos ou multiplos bancos.
