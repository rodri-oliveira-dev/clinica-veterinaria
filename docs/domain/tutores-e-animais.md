# Tutores e Animais

## Objetivo

Registrar a linguagem ubiqua, as responsabilidades e a fronteira inicial da primeira fatia de negocio da plataforma: cadastro e manutencao de tutores, animais e seus vinculos dentro de uma clinica veterinaria.

Este documento orienta a Entrega 1. Ele nao define entidades, tabelas, endpoints, contratos HTTP ou estrutura fisica de projetos.

## Decisao de fronteira

Tutores e Animais pertencem ao mesmo Bounded Context inicial: **Cadastro de Tutores e Animais**.

Na implementacao inicial, a recomendacao e materializar essa capacidade como um unico modulo de negocio, com linguagem interna em portugues e ownership unico dos dados de tutores, animais, contatos e vinculos. Nao ha evidencia suficiente para criar dois Bounded Contexts, dois DbContexts, dois schemas, dois repositories independentes ou contratos formais entre Tutores e Animais nesta fase.

Entre as alternativas da SDD, a separacao em Bounded Contexts distintos esta rejeitada. A possibilidade de dois modulos internos no mesmo Bounded Context fica tratada como decisao fisica futura, nao como requisito inicial.

A separacao futura em modulos ou Bounded Contexts distintos fica adiada ate existir evidencia de linguagem, regras, ownership, ciclo de vida, caracteristicas de seguranca ou ritmo de mudanca realmente divergentes.

## Glossario

| Termo | Significado inicial |
| --- | --- |
| Tutor | Pessoa responsavel pelo animal no relacionamento operacional com a clinica. |
| Animal | Paciente atendido pela clinica. |
| Vinculo | Relacao entre um tutor e um animal dentro do tenant da clinica. |
| Responsavel principal | Tutor destacado para comunicacoes ou decisoes operacionais quando houver mais de um tutor vinculado ao animal. Deve ser modelado somente se a regra aparecer na Entrega 1. |
| Contato | Canal de comunicacao do tutor, como telefone, e-mail ou outro meio aceito pela clinica. |
| Situacao | Estado operacional de um tutor ou animal, inicialmente ativo ou inativo se houver necessidade do fluxo. |
| Transferencia de responsabilidade | Mudanca confirmada do tutor responsavel por um animal. Nao e uma alteracao implicita nem silenciosa. |

## Termos aceitos e evitados

Termos aceitos na linguagem de dominio:

- `Tutor`;
- `Animal`;
- `Vinculo`;
- `Contato`;
- `Responsavel principal`, quando a regra exigir;
- `Situacao`, quando a regra exigir;
- `CadastrarTutor`;
- `CadastrarAnimal`;
- `VincularAnimalAoTutor`;
- `TransferirResponsabilidadeDoAnimal`.

Termos evitados:

- `Customer`, `Client`, `PetOwner` ou `Owner` para representar tutor;
- `Pet`, quando o conceito do dominio for paciente animal;
- `CreateTutorCommand`, `UpdateAnimalHandler` ou nomes mistos em ingles para casos de uso de dominio;
- `Responsavel financeiro`, sem requisito especifico;
- `Proprietario legal`, como sinonimo automatico de tutor;
- `Shared`, `Common` ou `Core` para compartilhar conceitos de dominio por conveniencia.

Termos tecnicos consolidados podem permanecer em ingles quando representarem infraestrutura ou padroes de codigo, como `Domain`, `Application`, `Infrastructure`, `Controller`, `DbContext`, `Repository`, `Handler`, `Request` e `Response`.

## Responsabilidades

O Bounded Context Cadastro de Tutores e Animais e responsavel por:

- cadastrar, consultar, pesquisar, atualizar e inativar tutores;
- registrar e manter contatos de tutores conforme necessidade do fluxo;
- cadastrar, consultar, pesquisar, atualizar e inativar animais;
- manter o vinculo entre tutor e animal;
- transferir a responsabilidade por um animal somente mediante confirmacao explicita;
- garantir que tutor, animal e vinculo pertencam ao tenant autenticado;
- tratar dados de outro tenant como inexistentes nos fluxos comuns;
- expor contratos publicos futuros orientados a casos de uso, sem expor entidades de dominio ou persistencia.

Nao pertencem a esta fronteira nesta etapa:

- prontuario;
- atendimento;
- vacinacao;
- exames;
- medicamentos;
- agenda;
- faturamento;
- estoque;
- convenio;
- guarda compartilhada;
- pedigree;
- historico clinico.

## Ownership dos dados

O modulo Cadastro de Tutores e Animais sera o owner dos dados funcionais que representem:

- tutores;
- contatos de tutores;
- animais;
- vinculos entre tutores e animais;
- situacao operacional de tutor ou animal, se introduzida.

Quando esses dados forem persistidos, todas as tabelas funcionais devem possuir `tenant_id NOT NULL`, conforme a ADR-0001. Unicidades locais ao tenant devem incluir `tenant_id`, e relacionamentos entre tutor, animal e vinculo devem impedir associacao cruzada entre tenants.

Outros modulos nao devem consultar diretamente tabelas, entidades EF Core, `DbContext` ou repositories desse modulo. Necessidades futuras de agenda, atendimento, faturamento ou notificacao devem usar contratos deliberados, projecoes locais ou workflows definidos quando a funcionalidade existir.

## Fundacao tecnica inicial

O SDD 13 materializa a fronteira como um unico assembly de modulo:

```text
src/Modules/Tutores/PetShop.Tutores/
```

Essa fundacao usa pastas conceituais `Domain`, `Application`, `Infrastructure` e `Api`, mas preserva superficie publica minima. A API carrega o modulo somente pelos pontos de composicao `AddModuloTutores` e `MapModuloTutores`.

O SDD 15 persiste `Tutor` em PostgreSQL usando o `PetShopDbContext` tecnico da API como contexto de migration do monolito e mapeamento localizado no modulo `PetShop.Tutores`.

O SDD 16 adiciona os casos de uso e contratos HTTP de Tutores no mesmo assembly do modulo. A API continua carregando o modulo pelos pontos de composicao `AddModuloTutores` e `MapModuloTutores`, informando ao modulo o `DbContext` tecnico e o tenant autenticado resolvido na borda.

O SDD 17 introduz o modelo de dominio inicial de `Animal`, ainda sem persistencia, endpoints, repository, consulta direta a `Tutor`, vinculos completos ou eventos de integracao.

O SDD 18 persiste `Animal` em PostgreSQL no mesmo modulo e cria o vinculo inicial com `Tutor` por `TutorResponsavel`.

## Invariantes conhecidas

- Tutor, animal e vinculo sempre pertencem a exatamente um tenant autenticado.
- O tenant nao pode ser informado pelo cliente como autoridade em body, rota, query string ou header.
- Dados de outro tenant devem se comportar como inexistentes para operacoes comuns.
- Um animal nao deve ser vinculado a tutor de outro tenant.
- A transferencia de responsabilidade de um animal exige confirmacao explicita.
- Inativar tutor ou animal nao deve apagar historico futuro de atendimento ou faturamento, quando esses contextos existirem.
- O conceito de tutor nao presume propriedade legal do animal.
- Responsavel financeiro nao deve ser separado do tutor sem requisito de negocio.

## Modelo inicial de Tutor

O SDD 14 introduz `Tutor` como Aggregate Root inicial do modulo Cadastro de Tutores e Animais. O modelo permanece somente no Domain, sem EF Core, endpoints, DTOs HTTP, repositories ou eventos de dominio.

Identidade e tenant sao imutaveis por instancia. O `TenantId` do dominio e um identificador forte local ao modulo; a conversao a partir da claim autenticada continua sendo responsabilidade futura da borda/Application, conforme a ADR-0001.

Campos e regras iniciais:

- `TutorId` e `TenantId` sao obrigatorios e baseados em `Guid`;
- `NomeDoTutor` e obrigatorio e remove espacos externos;
- `Cpf` e opcional, mas deve ter digitos verificadores validos quando informado;
- `Email` e `Telefone` sao opcionais individualmente, mas o tutor deve possuir ao menos um contato operacional;
- `Telefone` aceita DDD brasileiro com 10 ou 11 digitos, com formatacao comum e opcionalmente `+55`;
- tutor nasce com `SituacaoDoTutor.Ativo`;
- inativacao muda a situacao para inativo, registra `InativadoEm` e atualiza `AtualizadoEm`;
- alteracoes de cadastro preservam identidade, tenant e `CriadoEm`, e atualizam `AtualizadoEm`.

CPF, e-mail e telefone sao dados pessoais do tutor. O modelo nao registra logs nem eventos com documento completo nesta etapa. Finalidade, retencao, mascaramento em contratos HTTP e fluxos de direitos do titular continuam pendentes para os SDDs que criarem persistencia, API ou exportacao desses dados.

## Modelo inicial de Animal

O SDD 17 introduz `Animal` como Aggregate Root inicial para o paciente atendido pela clinica dentro do mesmo Bounded Context Cadastro de Tutores e Animais.

O modelo permanece somente no Domain, sem EF Core, endpoints, DTOs HTTP, repositories, eventos de dominio ou contratos entre modulos. `Animal` nao carrega nem consulta o aggregate `Tutor`; ele guarda a referencia operacional pelo Value Object `TutorResponsavel`, baseado no identificador do tutor responsavel. A validacao de existencia do tutor e de associacao no mesmo tenant pertence aos casos de uso futuros que tiverem acesso a persistencia.

Identidade e tenant sao imutaveis por instancia. O `TenantId` permanece o identificador forte local ao modulo e continua vindo da borda autenticada quando houver Application/API.

Campos e regras iniciais:

- `AnimalId`, `TenantId` e `TutorResponsavel` sao obrigatorios e baseados em `Guid`;
- `NomeDoAnimal` e obrigatorio e remove espacos externos;
- `Especie` e obrigatoria e modelada como Value Object textual simples, nao como catalogo;
- `Raca` e opcional e tambem textual, evitando catalogo completo sem requisito;
- `SexoDoAnimal` aceita `NaoInformado`, `Macho` ou `Femea`;
- `DataDeNascimento` e opcional, mas nao pode estar no futuro quando informada;
- `CorOuPelagem` e `ObservacaoCadastral` sao opcionais e removem espacos externos quando usadas;
- animal nasce com `SituacaoDoAnimal.Ativo`;
- inativacao muda a situacao para inativo, registra `InativadoEm` e atualiza `AtualizadoEm`;
- alteracoes de cadastro preservam identidade, tenant, tutor responsavel e `CriadoEm`, e atualizam `AtualizadoEm`.

A decisao por `Especie` e `Raca` textuais reduz complexidade inicial e evita criar catalogos de especies ou racas antes de existir regra de negocio, curadoria ou ownership claro para esses dados.

## Persistencia inicial de Tutor

O SDD 15 introduz a tabela funcional `tutores`, owned pelo modulo Cadastro de Tutores e Animais.

Colunas:

- `id`;
- `tenant_id NOT NULL`;
- `nome`;
- `documento`;
- `email`;
- `telefone`;
- `situacao`;
- `criado_em`;
- `atualizado_em`;
- `inativado_em`.

Decisoes:

- CPF e persistido normalizado em `documento`.
- CPF e unico somente dentro do tenant por `(tenant_id, documento)`.
- O mesmo CPF pode existir em tenants diferentes.
- `TutorId`, `TenantId`, `NomeDoTutor`, `Cpf`, `Email`, `Telefone` e `SituacaoDoTutor` usam conversoes EF Core no mapeamento do modulo.
- Consultas comuns usam query filter por tenant atual.
- Escritas usam guarda em `SaveChanges` para exigir tenant resolvido e impedir alteracao de tutor pertencente a outro tenant.
- Sem tenant resolvido, dados de tutores nao devem ser materializados por consultas comuns.

Trade-off registrado:

- Foi mantido um unico `PetShopDbContext` tecnico para migrations do banco compartilhado do monolito, carregando uma extensao publica de persistencia do modulo. Isso evita criar outro contexto/migration history antes de haver necessidade, mas exige que a API conheca o ponto de composicao EF do modulo. A entidade `Tutor` permanece interna ao modulo e o Domain continua sem dependencia de EF Core, ASP.NET Core, JWT, claims ou `HttpContext`.

Privacidade:

- CPF, e-mail e telefone sao dados pessoais. Nesta etapa eles sao persistidos para finalidade operacional de cadastro e contato do tutor pela clinica dentro do tenant.
- A API de Tutores retorna CPF somente como `cpfMascarado`; pesquisa por CPF aceita valor formatado ou normalizado, mas nao devolve o documento completo.
- Listagens minimizam dados e retornam somente `tutorId`, `nome`, `cpfMascarado` e `situacao`.
- Ainda nao foram definidos retencao, exportacao, eliminacao, bloqueio ou fluxos de direitos do titular, pois nao ha API publica para esses processos nesta fatia.

## Persistencia inicial de Animal

O SDD 18 introduz a tabela funcional `animais`, owned pelo mesmo modulo Cadastro de Tutores e Animais.

Colunas:

- `id`;
- `tenant_id NOT NULL`;
- `nome`;
- `especie`;
- `raca`;
- `sexo`;
- `data_de_nascimento`;
- `cor_ou_pelagem`;
- `observacao_cadastral`;
- `situacao`;
- `tutor_responsavel_id`;
- `criado_em`;
- `atualizado_em`;
- `inativado_em`.

Decisoes:

- `Animal` permanece no mesmo Bounded Context e no mesmo ownership de `Tutor`, conforme ADR-0003.
- O vinculo inicial usa referencia por identidade (`TutorResponsavel`) e e persistido em `tutor_responsavel_id`.
- Foi adotada foreign key fisica composta de `animais (tenant_id, tutor_responsavel_id)` para `tutores (tenant_id, id)`, pois Tutor e Animal pertencem ao mesmo modulo owner nesta fase.
- Essa FK composta impede associacao cross-tenant no banco. Um tutor de outro tenant nao satisfaz a constraint e, na Application, a consulta filtrada por tenant o trata como inexistente.
- A tabela `tutores` recebeu a alternate key `(tenant_id, id)` apenas para suportar a FK composta; o ownership continua no mesmo modulo.
- `AnimalId`, `TenantId`, `TutorId`, `NomeDoAnimal`, `Especie`, `Raca`, `DataDeNascimento`, `CorOuPelagem`, `ObservacaoCadastral`, `SexoDoAnimal` e `SituacaoDoAnimal` usam conversoes EF Core no mapeamento do modulo.
- Consultas comuns usam query filter por tenant atual.
- Escritas usam guarda de `SaveChanges` para exigir tenant resolvido e impedir alteracao de animal pertencente a outro tenant.
- Nao foram criados endpoints de animais, transferencia de responsabilidade, cache, broker, duplicacao de dados completos do tutor nem contrato entre modulos.

## API de Tutor

Rotas implementadas no SDD 16:

| Metodo | Rota | Uso |
| --- | --- | --- |
| `POST` | `/tutores` | `CadastrarTutor`; retorna `201 Created` com `Location`. |
| `GET` | `/tutores/{tutorId}` | `ConsultarTutorPorId`; outro tenant recebe `404`. |
| `PUT` | `/tutores/{tutorId}` | `AtualizarTutor`; `tutorId` vem da rota e o tenant vem da claim validada. |
| `GET` | `/tutores` | `PesquisarTutores`; suporta pagina limitada, nome, CPF normalizado, situacao e ordenacao estavel. |
| `POST` | `/tutores/{tutorId}/inativacao` | `InativarTutor`; nao realiza hard delete. |

Contratos HTTP sao separados do Domain. Requests nao aceitam `tenant_id` nem `id` como autoridade; membros JSON nao mapeados sao rejeitados pelo contrato fechado da API. Todos os endpoints exigem JWT Bearer com `tenant_id` valido e role minima `petshop.access`.

Pesquisa:

- `pagina`: padrao `1`;
- `tamanhoPagina`: padrao `20`, maximo `100`;
- `nome`: filtro textual;
- `cpf`: filtro por CPF normalizado;
- `situacao`: `ativo` ou `inativo`;
- `ordenarPor`: `nome` ou `criadoEm`;
- `direcao`: `asc` ou `desc`.

## API de Animal

Rotas implementadas no SDD 19:

| Metodo | Rota | Uso |
| --- | --- | --- |
| `POST` | `/animais` | `CadastrarAnimal`; valida tutor responsavel no tenant atual e retorna `201 Created` com `Location`. |
| `GET` | `/animais/{animalId}` | `ConsultarAnimalPorId`; outro tenant recebe `404`. |
| `PUT` | `/animais/{animalId}` | `AtualizarAnimal`; `animalId` vem da rota, o tenant vem da claim validada e o tutor responsavel nao e alterado. |
| `GET` | `/animais` | `PesquisarAnimais`; suporta pagina limitada, nome, tutor responsavel, especie, situacao e ordenacao estavel. |
| `POST` | `/animais/{animalId}/inativacao` | `InativarAnimal`; nao realiza hard delete. |

Contratos HTTP sao separados do Domain e nao expoem entidades de dominio ou persistencia. Requests de animais nao aceitam `tenant_id`, `id` nem troca de `tutorResponsavelId` na atualizacao como autoridade. Todos os endpoints exigem JWT Bearer com `tenant_id` valido e role minima `petshop.access`.

Cadastro valida o tutor responsavel por consulta filtrada no tenant atual. Tutor inexistente ou pertencente a outro tenant retorna `404`, sem revelar existencia cross-tenant. Respostas de animais retornam `tutorResponsavelId`, mas nao duplicam nome, CPF, e-mail, telefone ou outros dados pessoais do tutor.

Pesquisa:

- `pagina`: padrao `1`;
- `tamanhoPagina`: padrao `20`, maximo `100`;
- `nome`: filtro textual;
- `tutorResponsavelId`: filtro por tutor responsavel no tenant atual;
- `especie`: filtro textual normalizado pelo Value Object `Especie`;
- `situacao`: `ativo` ou `inativo`;
- `ordenarPor`: `nome` ou `criadoEm`;
- `direcao`: `asc` ou `desc`.

## Fluxos da Entrega 1

Fluxos minimos de tutor:

- cadastrar tutor;
- consultar tutor;
- atualizar tutor;
- pesquisar tutores;
- inativar tutor.

Fluxos minimos de animal:

- cadastrar animal;
- consultar animal;
- atualizar animal;
- pesquisar animais;
- inativar animal.

Fluxos de vinculo:

- vincular animal a tutor;
- transferir responsabilidade do animal somente apos confirmacao explicita.

Todos os fluxos persistentes da Entrega 1 devem validar isolamento com pelo menos dois tenants.

## Diagrama

```mermaid
flowchart LR
    Tenant[Tenant autenticado]
    Modulo[Cadastro de Tutores e Animais]
    Tutor[Tutor]
    Contato[Contato]
    Animal[Animal]
    Vinculo[Vinculo]

    Tenant --> Modulo
    Modulo --> Tutor
    Tutor --> Contato
    Modulo --> Animal
    Tutor --> Vinculo
    Animal --> Vinculo
```

## Decisoes adiadas

- Se `Responsavel principal` sera necessario na Entrega 1.
- Se `Situacao` sera modelada como estado explicito ou derivada de regras simples.
- Quais campos de tutor, contato e animal serao obrigatorios nos contratos HTTP.
- Quais regras de unicidade local ao tenant serao exigidas.
- Se a persistencia futura de animais e vinculos continuara no `PetShopDbContext` tecnico ou exigira um `DbContext` especifico do modulo.
- Se outros modulos precisarao de contratos de leitura ou projecoes locais sobre tutores e animais.

## Criterios para revisao da fronteira

Revisar a decisao se surgirem evidencias de que tutores e animais:

- usam linguagens conflitantes em fluxos diferentes;
- mudam por motivos frequentes e independentes;
- exigem ownership de dados por times ou capacidades distintas;
- possuem regras transacionais que causam acoplamento excessivo;
- precisam de caracteristicas de seguranca, compliance, disponibilidade ou escala diferentes;
- viram passagem obrigatoria para fluxos de agenda, atendimento ou faturamento sem pertencer a eles.
