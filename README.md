# Clinica Veterinaria

Backend inicial de uma plataforma SaaS multitenant para clinicas veterinarias, petshops e servicos para pets.

O projeto nasce como um monolito modular em .NET 10 com um unico deploy. A entrega atual cria somente a fundacao tecnica minima: API HTTP, AppHost Aspire para composicao local, building blocks de observabilidade e testes de baseline. Modulos de negocio, PostgreSQL, Keycloak, autenticacao e multitenancy tecnico serao adicionados apenas quando houver uma fatia funcional concreta.

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

- `PetShop.Api`: ASP.NET Core API com endpoints minimos `/health` e `/diagnostics`.
- `PetShop.AppHost`: composicao local Aspire contendo apenas a API.
- `PetShop.Observability`: building block agnostico de ASP.NET Core para correlation, contexto W3C, HTTP de saida e mensageria futura.
- `PetShop.Observability.AspNetCore`: adapter web para middleware de correlation e contexto de execucao.

## Decisoes preservadas

- O tenant autenticado vem exclusivamente da claim validada `tenant_id`.
- Nao existe tenant padrao, fallback ou tenant informado pelo frontend como autoridade.
- O Domain nao depende de ASP.NET Core, `HttpContext`, JWT ou claims.
- `CorrelationId` e independente de `TraceId`.
- HTTP usa `X-Correlation-Id`.
- `PetShop.Observability` nao depende de ASP.NET Core.
- O AppHost e apenas composicao local; ele nao define banco, identidade, broker, cache ou gateway.

As decisoes completas estao em:

- `docs/adrs/0001-multitenancy-claim-e-isolamento-por-linha.md`
- `docs/adrs/0002-library-propagacao-observabilidade.md`

## Requisitos

- .NET SDK 10
- Acesso ao NuGet.org para restore

## Comandos

```bash
dotnet tool restore
dotnet restore ./ClinicaVeterinaria.slnx
dotnet build ./ClinicaVeterinaria.slnx --configuration Release --no-restore
dotnet test ./ClinicaVeterinaria.slnx --configuration Release --no-build --no-restore --settings ./coverlet.runsettings
```

Para executar a API diretamente:

```bash
dotnet run --project ./src/Apps/PetShop.Api/PetShop.Api.csproj
```

Para executar a composicao local Aspire:

```bash
dotnet run --project ./src/AppHost/PetShop.AppHost/PetShop.AppHost.csproj
```

## Escopo ainda nao implementado

- PostgreSQL e migrations.
- Keycloak, JWT, autenticacao e autorizacao.
- Implementacao tecnica de multitenancy.
- OpenTelemetry completo com exporter OTLP.
- Modulos de negocio como cadastro, pets, agenda, atendimento ou cobranca.
- Broker, Redis, API Gateway, microsservicos ou multiplos bancos.
