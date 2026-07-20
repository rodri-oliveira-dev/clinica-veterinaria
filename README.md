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
  Modules/Tutores/PetShop.Tutores/
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
- `PetShop.Tutores`: modulo Cadastro de Tutores e Animais, carregado pela API por `AddModuloTutores`, `MapModuloTutores` e pela extensao de persistencia do modulo. Possui os aggregates `Tutor` e `Animal` persistidos em PostgreSQL, com endpoints HTTP para cadastro, consulta, atualizacao, pesquisa, inativacao e transferencia explicita de responsabilidade do animal. Ainda nao ha repository generico.

## Decisoes preservadas

- O tenant autenticado vem exclusivamente da claim validada `tenant_id`.
- `TenantId` e um identificador forte baseado em `Guid`, com representacao futura PostgreSQL `uuid`.
- Nao existe tenant padrao, fallback ou tenant informado pelo frontend como autoridade.
- O Domain nao depende de ASP.NET Core, `HttpContext`, JWT ou claims.
- `CorrelationId` e independente de `TraceId`.
- HTTP usa `X-Correlation-Id`.
- `PetShop.Observability` nao depende de ASP.NET Core.
- O AppHost e apenas composicao local; ele sobe PostgreSQL e Keycloak para desenvolvimento, incluindo realm e client locais, sem definir broker, cache ou gateway.
- A primeira fatia de negocio documentada e `Cadastro de Tutores e Animais`, mantendo tutor, animal e vinculo no mesmo Bounded Context inicial e materializada inicialmente em um unico assembly de modulo. As tabelas `tutores` e `animais` pertencem a esse modulo.

As decisoes completas estao em:

- `docs/adrs/0001-multitenancy-claim-e-isolamento-por-linha.md`
- `docs/adrs/0002-library-propagacao-observabilidade.md`
- `docs/adrs/0003-fronteira-cadastro-tutores-animais.md`

## Requisitos

- .NET SDK 10
- VS Code, quando usar a experiencia de desenvolvimento versionada do repositorio
- Docker Desktop, Podman ou runtime OCI compativel para os containers locais do Aspire
- Docker Compose v2, quando usar a alternativa containerizada
- Acesso ao NuGet.org para restore

## Experiencia no VS Code

Abra o workspace versionado na raiz do repositorio:

```bash
code ./clinica-veterinaria.code-workspace
```

O workspace abre a pasta raiz do repositorio. As preferencias compartilhadas, incluindo a solution padrao `ClinicaVeterinaria.slnx`, ficam em `.vscode/settings.json` para evitar configuracao duplicada.

Extensoes recomendadas em `.vscode/extensions.json`:

- C# Dev Kit, C#, runtime .NET e extensao oficial Aspire para editar, testar e depurar os projetos .NET e o AppHost.
- Docker/Container Tools, YAML e GitHub Actions para `Dockerfile`, `compose.yaml` e workflows.
- Markdown, markdownlint e Mermaid para documentacao e modelos de ameaca.
- EditorConfig, Coverage Gutters, SonarLint em modo local e Trivy para manter sinais de qualidade e seguranca no editor.

Tasks principais disponiveis pelo comando `Tasks: Run Task`:

- `dotnet: tool restore`;
- `dotnet: restore solution`;
- `dotnet: build solution`;
- `dotnet: test solution`;
- `local: start aspire`;
- `local: start compose`;
- `local: stop compose`;
- `local: reset compose`;
- `local: logs compose`;
- `docker: compose config`;
- `docker: build api image`.

Configuracoes de debug disponiveis:

- `PetShop.AppHost (Aspire)`: experiencia principal local. Inicia o AppHost, que compoe API, PostgreSQL e Keycloak e integra com o Aspire Dashboard.
- `PetShop.Api (host, local dependencies required)`: executa apenas a API no host em `Development`. Use quando PostgreSQL e Keycloak ja estiverem disponiveis por Compose ou outro ambiente local compativel. O launch usa a connection string local descartavel do `.env.example`; se alterar porta, usuario, senha ou banco em `.env`, ajuste tambem a variavel `ConnectionStrings__petshop` da configuracao de debug.

Aspire e Compose tem papeis diferentes:

- Aspire e o caminho padrao de desenvolvimento local, com composicao declarativa, injecao de configuracao e dashboard.
- Docker Compose e a alternativa explicita para validar a imagem da API e executar API, PostgreSQL e Keycloak totalmente em containers. Ele nao substitui o AppHost e nao adiciona observabilidade externa por conta propria.

## Comandos

```bash
dotnet tool restore
dotnet restore ./ClinicaVeterinaria.slnx
dotnet build ./ClinicaVeterinaria.slnx --configuration Release --no-restore
dotnet test ./ClinicaVeterinaria.slnx --configuration Release --no-build --no-restore --settings ./coverlet.runsettings
```

Gates locais equivalentes ao CI:

```bash
dotnet test ./tests/PetShop.UnitTests/PetShop.UnitTests.csproj --configuration Release --no-build --no-restore --settings ./coverlet.runsettings --collect "XPlat Code Coverage"
dotnet test ./tests/BuildingBlocks/PetShop.Observability.Tests/PetShop.Observability.Tests.csproj --configuration Release --no-build --no-restore --settings ./coverlet.runsettings --collect "XPlat Code Coverage"
dotnet test ./tests/PetShop.ArchitectureTests/PetShop.ArchitectureTests.csproj --configuration Release --no-build --no-restore --settings ./coverlet.runsettings --collect "XPlat Code Coverage"
dotnet test ./tests/PetShop.IntegrationTests/PetShop.IntegrationTests.csproj --configuration Release --no-build --no-restore --settings ./coverlet.runsettings --collect "XPlat Code Coverage"
dotnet test ./tests/PetShop.AppHost.Tests/PetShop.AppHost.Tests.csproj --configuration Release --no-build --no-restore --settings ./coverlet.runsettings --collect "XPlat Code Coverage"
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

A API registra um `PetShopDbContext` tecnico em `src/Apps/PetShop.Api/Infrastructure/Persistence/`. Ele centraliza a migration do banco compartilhado do monolito e carrega o mapeamento do modulo `PetShop.Tutores` por uma extensao publica de composicao, sem expor entidades de dominio como contrato entre modulos.

Configuracao:

- A connection string se chama `petshop`.
- No Aspire local, `PetShop.AppHost` injeta essa connection string na API por `WithReference(petshopDatabase)`.
- Fora do Aspire, use configuracao padrao do ASP.NET Core, como variavel de ambiente `ConnectionStrings__petshop`, user-secrets ou arquivo local nao versionado.
- O provider EF Core e `Npgsql.EntityFrameworkCore.PostgreSQL`.
- A convencao relacional usa `snake_case` e a tabela de historico de migrations se chama `__ef_migrations_history`.
- O endpoint `/health` inclui o check `postgresql`, baseado no `PetShopDbContext`.

Tabela funcional introduzida:

- `tutores`: possui `id`, `tenant_id NOT NULL`, `nome`, `documento`, `email`, `telefone`, `situacao`, `criado_em`, `atualizado_em` e `inativado_em`.
- `documento` armazena CPF normalizado quando informado.
- A unicidade de CPF e local ao tenant pelo indice unico `(tenant_id, documento)`, permitindo o mesmo CPF em tenants diferentes.
- Constraints no banco impedem `id` e `tenant_id` vazios, situacao fora do dominio conhecido, documento/telefone com tamanho invalido e tutor sem contato operacional.
- `animais`: possui `id`, `tenant_id NOT NULL`, `nome`, `especie`, `raca`, `sexo`, `data_de_nascimento`, `cor_ou_pelagem`, `observacao_cadastral`, `situacao`, `tutor_responsavel_id`, `versao`, `criado_em`, `atualizado_em` e `inativado_em`.
- O vinculo de animal com tutor usa foreign key composta `(tenant_id, tutor_responsavel_id)` para `tutores (tenant_id, id)`, porque os dois aggregates pertencem ao mesmo modulo owner nesta fase.
- Constraints no banco impedem `id`, `tenant_id` e `tutor_responsavel_id` vazios, textos obrigatorios em branco, sexo/situacao fora do dominio conhecido e vinculo com tutor inexistente ou de outro tenant.
- `historico_transferencias_animais`: possui `id`, `tenant_id NOT NULL`, `animal_id`, `tutor_anterior_id`, `tutor_novo_id`, `realizada_em`, `subject` e `motivo`. A tabela registra a trilha minima da transferencia de responsabilidade, sem token, claims completas, CPF, e-mail ou telefone.

Estrategia multitenant da persistencia de tutores:

- O tenant continua vindo exclusivamente do `ITenantContext` resolvido da claim `tenant_id` autenticada.
- O `PetShopDbContext` aplica query filter parametrizado por contexto para `Tutor`; sem tenant resolvido, consultas comuns nao retornam tutores.
- `SaveChanges` bloqueia inclusao, alteracao ou exclusao de tutores quando nao houver tenant resolvido ou quando o tenant da entidade divergir do tenant autenticado.
- A estrategia combina filtro de leitura, guarda de escrita e constraints de banco. O trade-off e que o `PetShopDbContext` tecnico conhece a extensao de persistencia do modulo; em troca, a entidade `Tutor` permanece interna ao modulo e o Domain nao referencia EF Core.

Estrategia multitenant da persistencia de animais:

- O tenant continua vindo exclusivamente do `ITenantContext` resolvido da claim `tenant_id` autenticada.
- O `PetShopDbContext` aplica query filter parametrizado por contexto para `Animal`; sem tenant resolvido, consultas comuns nao retornam animais.
- `SaveChanges` bloqueia inclusao, alteracao ou exclusao de animais quando nao houver tenant resolvido ou quando o tenant da entidade divergir do tenant autenticado.
- A Application valida o tutor responsavel por consulta filtrada no tenant atual; tutor de outro tenant se comporta como inexistente.
- A FK composta no banco protege o mesmo limite caso uma escrita tente associar animal a tutor inexistente ou de outro tenant.
- A transferencia de responsabilidade usa endpoint explicito, exige `versao` do animal para proteger contra lost update e grava historico append-only limitado ao tenant atual.

Comandos de migrations:

```bash
dotnet tool restore

ConnectionStrings__petshop="Host=localhost;Port=5432;Database=petshop;Username=petshop;Password=<senha>" \
dotnet ef migrations add NomeDaMigration \
  --project ./src/Apps/PetShop.Api/PetShop.Api.csproj \
  --startup-project ./src/Apps/PetShop.Api/PetShop.Api.csproj \
  --output-dir Infrastructure/Persistence/Migrations \
  --context PetShopDbContext

ConnectionStrings__petshop="Host=localhost;Port=5432;Database=petshop;Username=petshop;Password=<senha>" \
dotnet ef database update \
  --project ./src/Apps/PetShop.Api/PetShop.Api.csproj \
  --startup-project ./src/Apps/PetShop.Api/PetShop.Api.csproj \
  --context PetShopDbContext
```

Politica de migrations:

- Em desenvolvimento, novas alteracoes de schema devem gerar migrations versionadas junto com a mudanca de codigo.
- A API nao aplica migrations automaticamente no startup.
- Em producao, gere script idempotente e aplique pelo processo de release do ambiente:

```bash
dotnet ef migrations script --idempotent \
  --project ./src/Apps/PetShop.Api/PetShop.Api.csproj \
  --startup-project ./src/Apps/PetShop.Api/PetShop.Api.csproj \
  --context PetShopDbContext \
  --output ./artifacts/sql/petshop-migrations.sql
```

Novas tabelas de negocio devem possuir `tenant_id` obrigatorio conforme a ADR-0001. Row-Level Security permanece fora desta fundacao ate existir uma decisao especifica.

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

Endpoints funcionais de Tutores:

| Metodo | Rota | Uso |
| --- | --- | --- |
| `POST` | `/tutores` | Cadastra tutor e retorna `201 Created` com `Location`. |
| `GET` | `/tutores/{tutorId}` | Consulta tutor visivel no tenant atual. |
| `PUT` | `/tutores/{tutorId}` | Atualiza cadastro do tutor pela rota, sem aceitar `tenant_id` ou `id` no body como autoridade. |
| `GET` | `/tutores` | Pesquisa tutores com `pagina`, `tamanhoPagina`, `nome`, `cpf`, `situacao`, `ordenarPor` e `direcao`. |
| `POST` | `/tutores/{tutorId}/inativacao` | Inativa tutor sem hard delete. |

Todos exigem JWT Bearer com `tenant_id` valido e a role minima `petshop.access`. Dados de outro tenant retornam `404` nos fluxos por identificador. CPF e aceito como filtro normalizado, mas as respostas expõem apenas `cpfMascarado`.

Endpoints funcionais de Animais:

| Metodo | Rota | Uso |
| --- | --- | --- |
| `POST` | `/animais` | Cadastra animal vinculado a tutor responsavel do tenant atual e retorna `201 Created` com `Location`. |
| `GET` | `/animais/{animalId}` | Consulta animal visivel no tenant atual. |
| `PUT` | `/animais/{animalId}` | Atualiza cadastro do animal pela rota, sem trocar tutor responsavel nem aceitar `tenant_id` ou `id` no body como autoridade. |
| `GET` | `/animais` | Pesquisa animais com `pagina`, `tamanhoPagina`, `nome`, `tutorResponsavelId`, `especie`, `situacao`, `ordenarPor` e `direcao`. |
| `POST` | `/animais/{animalId}/transferencias-de-responsabilidade` | Transfere explicitamente a responsabilidade do animal para outro tutor ativo do tenant atual, usando `novoTutorId`, `versao` e motivo opcional. |
| `POST` | `/animais/{animalId}/inativacao` | Inativa animal sem hard delete. |

Todos exigem JWT Bearer com `tenant_id` valido e a role minima `petshop.access`. Tutor responsavel inexistente ou pertencente a outro tenant retorna `404`. Dados de outro tenant retornam `404` nos fluxos por identificador. As respostas de animais retornam `tutorResponsavelId` e `versao`, sem duplicar dados pessoais do tutor.

## Entrega 1 - Cadastro de Tutores e Animais

A Entrega 1 consolida a primeira fatia vertical funcional do monolito: o fluxo Tutor -> Animal dentro do modulo `PetShop.Tutores`, que representa a capacidade **Cadastro de Tutores e Animais**. A linguagem de negocio fica em portugues para conceitos e casos de uso (`Tutor`, `Animal`, `CadastrarTutor`, `CadastrarAnimal`, `TransferirResponsabilidadeDoAnimal`), enquanto termos tecnicos consolidados permanecem em ingles (`Application`, `Infrastructure`, `DbContext`, `Repository`, `Request`, `Response`).

Escopo funcional validado:

- cadastrar, consultar, atualizar, pesquisar e inativar tutores;
- cadastrar, consultar, atualizar, pesquisar e inativar animais;
- vincular animal a tutor responsavel do mesmo tenant;
- transferir explicitamente a responsabilidade do animal para outro tutor ativo do mesmo tenant;
- tratar dados de outro tenant como inexistentes nos fluxos comuns;
- retornar Problem Details padronizado, com `correlationId`, para erros de entrada, autenticacao, autorizacao, recurso inexistente e conflitos.

Modulo e ownership:

- `src/Modules/Tutores/PetShop.Tutores/` contem Domain, Application, Infrastructure e API do modulo.
- O modulo e owner das tabelas `tutores`, `animais` e `historico_transferencias_animais`.
- Outros modulos nao acessam entidades, mappings, repositories ou tabelas desse modulo.
- A superficie publica do assembly continua limitada a `AddModuloTutores`, `MapModuloTutores` e `ConfigurePersistenciaDoModuloTutores`.
- Nao foram introduzidos `Shared`, `Common`, `Core`, repository generico, MediatR, AutoMapper, FluentValidation, Integration Events, broker, Redis, API Gateway, microsservico ou banco separado.

Regras multitenant e seguranca:

- O tenant vem exclusivamente da claim validada `tenant_id`.
- Nenhum request aceita `tenant_id` como autoridade.
- Todas as tabelas funcionais possuem `tenant_id NOT NULL`.
- Unicidade de CPF e local ao tenant.
- FKs compostas com `tenant_id` impedem associacao de animal ou historico de transferencia com tutor/animal de outro tenant.
- O OpenAPI descreve JWT Bearer e nao publica `tenantId` como parametro ou propriedade de contrato.
- CPF, e-mail, telefone, tokens e claims completas nao sao registrados em historico de transferencia nem retornados em listagens; CPF sai mascarado.

Requests principais:

- `POST /tutores`: `nome`, `cpf`, `email`, `telefone`; exige ao menos um contato.
- `PUT /tutores/{tutorId}`: mesmos campos de cadastro; o identificador vem da rota.
- `GET /tutores`: `pagina`, `tamanhoPagina`, `nome`, `cpf`, `situacao`, `ordenarPor`, `direcao`.
- `POST /animais`: `tutorResponsavelId`, `nome`, `especie`, `raca`, `sexo`, `dataDeNascimento`, `corOuPelagem`, `observacaoCadastral`.
- `PUT /animais/{animalId}`: dados cadastrais do animal; nao troca tutor responsavel.
- `GET /animais`: `pagina`, `tamanhoPagina`, `nome`, `tutorResponsavelId`, `especie`, `situacao`, `ordenarPor`, `direcao`.
- `POST /animais/{animalId}/transferencias-de-responsabilidade`: `novoTutorId`, `versao`, `motivo`.

Paginacao:

- `pagina` padrao: `1`.
- `tamanhoPagina` padrao: `20`.
- `tamanhoPagina` maximo: `100`.
- Ordenacao aceita `nome` ou `criadoEm`, com `asc` ou `desc`.

Migrations e execucao local:

- As quatro migrations versionadas recriam o banco desde zero: fundacao inicial, tutores, animais e historico de transferencias.
- A API nao aplica migrations automaticamente no startup; aplique com `dotnet ef database update`.
- Aspire continua sendo a experiencia principal para desenvolvimento local com API, PostgreSQL, Keycloak e Dashboard.
- Docker Compose valida a imagem da API e a stack containerizada com PostgreSQL e Keycloak.
- Smoke tests existentes validam autenticacao local do Keycloak, `X-Correlation-Id` e tenant autenticado.

Limitacoes e proximos passos:

- Busca textual e ordenacao por nomes usam Value Objects persistidos por conversao EF Core. A paginacao e o isolamento sao testados, mas a otimizacao para alto volume deve ser revisada antes de carga real, possivelmente com colunas normalizadas, indices funcionais ou projecoes de leitura deliberadas.
- Row-Level Security, suporte administrativo cross-tenant, direitos do titular, retencao, exportacao, auditoria funcional ampla e prontuario veterinario continuam fora desta entrega.
- Agenda, atendimento, faturamento, notificacoes, catalogo de servicos e profissionais devem entrar como capacidades proprias, sem acessar diretamente tabelas ou entidades de Tutores.

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

## Ambiente local Docker Compose

O Aspire continua sendo a experiencia principal de desenvolvimento local porque compoe recursos, injeta configuracao e integra com o Aspire Dashboard. O Docker Compose e uma alternativa explicita para validar a imagem da API, executar API/PostgreSQL/Keycloak totalmente em containers e rodar smoke tests em ambientes onde o AppHost nao sera usado.

Arquivos:

- `Dockerfile`: imagem da API em multi-stage build, runtime ASP.NET Core .NET 10, publish Release, usuario nao-root e healthcheck em `/health/ready`.
- `compose.yaml`: stack local com API, PostgreSQL e Keycloak usando o realm versionado em `src/AppHost/PetShop.AppHost/keycloak/realms/petshop-local-realm.json`.
- `.env.example`: valores locais descartaveis. Copie para `.env` somente se quiser sobrescrever portas, usuario ou senhas locais. Nao versione `.env`.

Portas e credenciais locais padrao:

| Recurso | URL/porta | Credencial local |
| --- | --- | --- |
| API | `http://localhost:5000` | JWT Bearer do Keycloak |
| PostgreSQL | `localhost:5432` | `petshop` / `petshop-local-password` |
| Keycloak | `http://localhost:8080` | admin `admin` / `admin-local-password` |
| Usuario local | Keycloak realm `petshop-local` | `local.petshop.user` / `local-dev-password` |

Comandos:

```bash
docker build -t petshop-api:local .
docker compose config
docker compose build
docker compose up -d
docker compose ps
docker compose logs --no-color
docker compose down
docker compose down -v
```

Use `docker compose down -v` para remover os volumes nomeados e recriar PostgreSQL e Keycloak do zero. Sem `-v`, os volumes `petshop-postgres-data` e `keycloak-data` preservam dados locais entre reinicios.

Migrations na stack Compose:

- A API nao aplica migrations automaticamente no startup.
- Para esta stack local, aplique migrations de forma explicita a partir do host com o SDK .NET:

```bash
dotnet tool restore

ConnectionStrings__petshop="Host=localhost;Port=5432;Database=petshop;Username=petshop;Password=petshop-local-password" \
dotnet ef database update \
  --project ./src/Apps/PetShop.Api/PetShop.Api.csproj \
  --startup-project ./src/Apps/PetShop.Api/PetShop.Api.csproj \
  --context PetShopDbContext
```

Em PowerShell:

```powershell
$env:ConnectionStrings__petshop = "Host=localhost;Port=5432;Database=petshop;Username=petshop;Password=petshop-local-password"
dotnet ef database update `
  --project ./src/Apps/PetShop.Api/PetShop.Api.csproj `
  --startup-project ./src/Apps/PetShop.Api/PetShop.Api.csproj `
  --context PetShopDbContext
```

Token local pela stack Compose:

```bash
curl --request POST \
  --url http://localhost:8080/realms/petshop-local/protocol/openid-connect/token \
  --header "Content-Type: application/x-www-form-urlencoded" \
  --data "grant_type=password" \
  --data "client_id=petshop-api" \
  --data "username=local.petshop.user" \
  --data "password=local-dev-password"
```

Smoke test:

```powershell
./scripts/smoke-keycloak-auth.ps1 -ApiBaseUrl "http://localhost:5000" -KeycloakBaseUrl "http://localhost:8080"
```

Issuer interno/externo:

- Clientes no host obtem tokens em `http://localhost:8080`.
- A API dentro da rede Docker acessa o Keycloak por `http://keycloak:8080`.
- O Keycloak do Compose usa `--hostname=http://localhost:8080` e `--hostname-backchannel-dynamic=true`.
- A API valida `Authentication:Authority=http://localhost:8080/realms/petshop-local`, que corresponde ao `iss` do token, e busca metadados/JWKS por `Authentication:MetadataAddress=http://keycloak:8080/realms/petshop-local/.well-known/openid-configuration`.
- A validacao de issuer continua habilitada; o endereco interno serve apenas para discovery/backchannel dentro do Compose.

Observabilidade no Compose:

- A API inicia sem collector OTLP.
- Configure `OTEL_EXPORTER_OTLP_ENDPOINT` ou `OpenTelemetry__Otlp__Endpoint` quando houver collector externo.
- O Compose nao sobe dashboard, collector ou backend APM.

Troubleshooting:

- Se a API ficar unhealthy, confira `docker compose logs --no-color api` e valide se migrations foram aplicadas quando a mudanca de schema exigir.
- Se tokens forem rejeitados com `401`, confira se `iss` do token e `Authentication:Authority` continuam iguais a `http://localhost:8080/realms/petshop-local`.
- Se o realm nao refletir alteracoes no JSON, execute `docker compose down -v` para remover o volume persistido do Keycloak e importar novamente.
- Se portas `5000`, `5432` ou `8080` estiverem ocupadas, copie `.env.example` para `.env` e altere `API_PORT`, `POSTGRES_PORT` ou `KEYCLOAK_PORT`.

## Observabilidade

A API registra OpenTelemetry para traces, metricas e logs com `service.name` estavel `PetShop.Api`.

Instrumentacoes habilitadas:

- ASP.NET Core;
- `HttpClient`;
- metricas de runtime .NET.

O exporter OTLP e habilitado somente quando houver endpoint configurado por `OpenTelemetry:Otlp:Endpoint`, `OpenTelemetry__Otlp__Endpoint` ou pela variavel padrao `OTEL_EXPORTER_OTLP_ENDPOINT`. Ao executar pelo AppHost Aspire, o endpoint OTLP do dashboard e injetado no processo da API e logs, traces e metricas passam a aparecer no Aspire Dashboard.

O middleware de entrada aceita `X-Correlation-Id` valido ou cria um novo GUID, devolve o mesmo header na resposta e adiciona `correlation_id` e `tenant_id` como tags da Activity e scope de log. Esses identificadores nao sao adicionados ao W3C baggage; mensagens e jobs usam os headers canonicos `correlation_id`, `tenant_id`, `traceparent`, `tracestate` e `baggage` pelo building block `PetShop.Observability`.

## Qualidade, testes e CI

As Entregas 0 e 1 fecham os gates automatizados minimos para a fundacao tecnica e para a primeira fatia vertical, sem criar modulos vazios ou infraestrutura futura.

Suites:

- `PetShop.UnitTests`: tipos, factories e validacoes puras da API.
- `PetShop.Observability.Tests`: propagacao, correlation, HTTP de saida e adapter ASP.NET Core.
- `PetShop.ArchitectureTests`: regras de referencia entre projetos, ausencia de dependencia ASP.NET Core em `PetShop.Observability`, API sem dependencia do AppHost, producao sem dependencia de testes, ausencia de ciclos e fronteiras do modulo `PetShop.Tutores`.
- `PetShop.IntegrationTests`: WebApplicationFactory, JWT Bearer defensivo, Problem Details, health/readiness, OpenAPI, migrations, isolamento com dois tenants e fluxos HTTP de Tutores/Animais em PostgreSQL real com Testcontainers.
- `PetShop.AppHost.Tests`: smoke tests leves da composicao Aspire e do realm local do Keycloak.

O workflow `.github/workflows/dotnet.yml` executa checkout, setup do .NET pelo `global.json`, restore com auditoria NuGet, build Release, gates separados de testes, verificacao de Docker para Testcontainers, coleta de cobertura via Coverlet e publicacao de TRX/cobertura como artifacts. Os workflows adicionais cobrem Gitleaks, CodeQL, Dependency Review e SonarCloud opt-in quando as variaveis e secrets do repositorio estiverem configurados.

Cobertura e usada como sinal de risco. Nao ha threshold artificial nesta entrega; os pontos de maior risco ja cobertos sao autenticacao/autorizacao, tenant autenticado, correlation, health/readiness, OpenAPI, migrations, fronteiras arquiteturais, persistencia tenant-owned, contratos de Tutores/Animais, concorrencia de transferencia e bloqueio cross-tenant.

## Escopo ainda nao implementado

- Row-Level Security.
- Outros modulos de negocio como agenda, atendimento ou cobranca.
- Broker, Redis, API Gateway, microsservicos ou multiplos bancos.
