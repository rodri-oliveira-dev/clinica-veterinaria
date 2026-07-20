# Kit reutilizável para o novo projeto

Esta pasta reúne artefatos portáveis da POC atual para iniciar o backend do novo sistema de atendimento de petshop.

O conteúdo foi revisado para não depender dos bounded contexts, nomes de soluções, tópicos, provedores ou fluxos financeiros da POC anterior. A intenção é copiar **o conteúdo interno desta pasta** para a raiz do novo repositório depois que ele for criado.

## O que está incluído

- `AGENTS.md`: regras globais para agentes de código, incluindo multitenancy e propagação de observabilidade obrigatórios.
- `.agents/skills/`: skills reutilizáveis para DDD, .NET, EF Core, testes, cobertura, observabilidade, containers, CI, multitenancy e segurança de aplicação.
- `docs/adrs/`: decisões arquiteturais iniciais de multitenancy e propagação distribuída.
- `docs/security/`: registro de migração das skills de cybersecurity e template versionável de threat model.
- `src/BuildingBlocks/PetShop.Observability/`: núcleo agnóstico para correlation, contexto W3C, mensageria, Outbox e HTTP de saída.
- `src/BuildingBlocks/PetShop.Observability.AspNetCore/`: middleware de correlation e tenant para APIs.
- `tests/BuildingBlocks/PetShop.Observability.Tests/`: testes do contrato de propagação.
- `.githooks/`: Conventional Commits, restore após merge e validação antes do push.
- `scripts/setup/`: configuração segura de `core.hooksPath` para Bash e PowerShell.
- `ClinicaVeterinaria.slnx`: solution inicial com os building blocks e testes de observabilidade.
- `.github/actions/setup-dotnet/`: action composta para SDK e cache NuGet.
- `.github/workflows/`: CI .NET, CodeQL, dependency review, Gitleaks e SonarCloud opt-in.
- `.gitleaks.toml`: configuração conservadora de secret scanning.
- `third-party/licenses/`: licenças de materiais externos adaptados.
- `.editorconfig`, `Directory.Build.props`, `Directory.Packages.props`, `global.json` e `coverlet.runsettings`: baseline de desenvolvimento .NET.
- `.gitignore`, `.gitattributes` e `.dockerignore`: baseline de repositório e containers.

## Decisão multitenant obrigatória

O projeto nasce multitenant.

- O tenant autenticado deve ser obtido da claim validada `tenant_id` do access token.
- Todas as tabelas de negócio devem possuir a coluna obrigatória `tenant_id`.
- Não existe tenant padrão nem fallback silencioso.
- Funcionalidades persistentes devem possuir testes com pelo menos dois tenants.

Fontes de verdade:

- `docs/adrs/0001-multitenancy-claim-e-isolamento-por-linha.md`;
- `.agents/skills/multitenancy-dotnet/SKILL.md`;
- seção `Multitenancy` do `AGENTS.md`.

## Decisão de observabilidade obrigatória

Correlação e tracing distribuído devem permanecer contínuos quando o sistema evoluir para múltiplos serviços, jobs e mensageria.

Headers canônicos:

- HTTP: `X-Correlation-Id`;
- mensagens e jobs: `correlation_id`, `tenant_id`, `traceparent`, `tracestate` e `baggage`.

Os building blocks incluídos oferecem:

- middleware ASP.NET Core;
- `DelegatingHandler` para correlation em `HttpClient`;
- snapshot persistível para Outbox;
- injeção e extração agnósticas de broker;
- Activities `Producer` e `Consumer`;
- contexto ambiente seguro para processamento assíncrono.

Fontes de verdade:

- `docs/adrs/0002-library-propagacao-observabilidade.md`;
- `src/BuildingBlocks/PetShop.Observability/README.md`;
- `.agents/skills/configuring-opentelemetry-dotnet/SKILL.md`;
- seção `Observabilidade e propagação` do `AGENTS.md`.

## Baseline de segurança

As skills e automações iniciais cobrem:

- threat modeling com STRIDE/LINDDUN e foco multitenant;
- validação de entrada ASP.NET Core orientada pelas entidades, Value Objects e invariantes;
- secret scanning com Gitleaks;
- CodeQL, dependency review e auditoria NuGet;
- SonarCloud opt-in;
- configuração defensiva de JWT/OIDC para futura integração com Keycloak.

Fontes de verdade:

- `docs/security/cybersecurity-skills-migration.md`;
- `.agents/skills/threat-modeling-clinica-veterinaria/SKILL.md`;
- `.agents/skills/input-validation-dotnet/SKILL.md`;
- `.agents/skills/secret-scanning-gitleaks/SKILL.md`;
- `.agents/skills/devsecops-security-scanning/SKILL.md`;
- `.agents/skills/keycloak-jwt-security/SKILL.md`.

O workflow do SonarCloud permanece ignorado até a configuração das repository variables e do secret descritos no documento de migração.

## O que foi deixado de fora

Estes itens da POC anterior não foram copiados porque dependem de decisões ainda não tomadas no novo projeto:

- GCP, Terraform, Cloud Run e Cloud SQL;
- Nginx e topologia de borda;
- implementação concreta de Kafka, Pub/Sub, Outbox, DLQ e contratos de eventos;
- configuração de collector, exporter, backend APM, dashboards e alertas;
- mutation testing, k6 e OWASP ZAP;
- scanning de imagem e SBOM antes de existir artefato oficial;
- arquitetura C4/LikeC4 específica da POC;
- scripts e configurações ligados aos serviços financeiros existentes.

A library está preparada para esses fluxos, mas não antecipa qual broker, vendor APM ou infraestrutura será adotado.

Também permanecem pendentes decisões específicas de multitenancy, como tipo concreto de `TenantId`, provedor de identidade, Global Query Filters, interceptors e PostgreSQL Row-Level Security. A ADR inicial não inventa essas escolhas.

## Como usar

1. Crie o novo repositório.
2. Copie todos os arquivos e diretórios dentro de `novo-projeto/` para a raiz dele.
3. Renomeie a solution e acrescente os primeiros projetos do produto quando o novo repositório for iniciado.
4. Preserve as decisões ADR-0001 e ADR-0002.
5. Ajuste o nome e a descrição do projeto no `AGENTS.md`, sem remover as regras de isolamento ou propagação.
6. Acrescente novas versões ao `Directory.Packages.props` somente quando outros pacotes forem introduzidos.
7. Configure os hooks:

```bash
./scripts/setup/configure-git-hooks.sh
```

No PowerShell:

```powershell
./scripts/setup/configure-git-hooks.ps1
```

8. Confirme a configuração:

```bash
./scripts/setup/configure-git-hooks.sh --check
```

9. Configure SonarCloud quando o projeto estiver cadastrado, seguindo `docs/security/cybersecurity-skills-migration.md`.

## Premissas iniciais

O baseline assume:

- .NET 10;
- ASP.NET Core;
- nullable reference types;
- Central Package Management;
- backend inicialmente organizado como monólito modular;
- sistema multitenant desde a primeira funcionalidade persistida;
- tenant obtido da claim validada `tenant_id`;
- coluna `tenant_id` obrigatória em todas as tabelas de negócio;
- correlation ID independente do trace ID;
- propagação W3C para futuros serviços e mensageria;
- DDD aplicado somente onde houver linguagem, invariantes e ciclo de vida relevantes;
- PostgreSQL e EF Core como direção provável, sem obrigar sua adoção antes da modelagem;
- REST/OpenAPI como integração inicial com o frontend;
- Keycloak como direção para identidade, ainda dependente de configuração de realm, clients e claims;
- extração de microsserviços somente quando houver motivo de negócio, escala ou autonomia.

## Próximos passos sugeridos

Depois de preparar o repositório final, a primeira evolução deve ser uma fatia vertical pequena, por exemplo:

1. cadastro de tutor;
2. cadastro de pet;
3. cadastro de serviço;
4. disponibilidade de profissional;
5. criação de agendamento;
6. consulta da agenda diária.

Desde a primeira fatia:

- crie dados para pelo menos dois tenants;
- valide que não existe leitura ou alteração cruzada;
- configure OpenTelemetry no executável;
- registre o middleware de contexto;
- use a library em qualquer chamada HTTP ou processo assíncrono;
- derive tipos e limites de entrada das entidades e Value Objects;
- mantenha o Gitleaks ativo;
- habilite o SonarCloud quando as credenciais estiverem configuradas.

Evite instalar toda a infraestrutura da POC anterior antes de existir um problema concreto que a justifique.
