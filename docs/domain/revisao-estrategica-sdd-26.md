# Revisao estrategica SDD 26 - Bounded Contexts, modulos e Aggregates

- **Data:** 2026-07-21
- **Escopo:** checkpoint anterior as entregas de Catalogo de Servicos, Profissionais, Disponibilidade, Agenda, Atendimento e Cobranca
- **Natureza:** analise e documentacao

## Objetivo

Revisar a arquitetura de dominio do monolito modular antes de ampliar a plataforma alem da primeira fatia vertical de Cadastro de Tutores e Animais.

Este checkpoint avalia bounded contexts candidatos, modulos atuais, ownership, contracts internos, persistencia, dependencias, consistencia, linguagem ubiqua, riscos de acoplamento e sequencia recomendada para os proximos SDDs.

## Inspecao realizada

Foram revisados:

- `README.md`, `.editorconfig`, `Directory.Build.props`, `Directory.Packages.props`, `global.json`, `coverlet.runsettings` e `ClinicaVeterinaria.slnx`;
- ADRs `0001` a `0006`;
- documentos de dominio e regras derivados dos SDDs 12 a 25, incluindo `docs/domain/tutores-e-animais.md`, refinamentos do animal e relacionamento tutor-animal, catalogo de regras e matriz de rastreabilidade;
- historico recente de commits desde `docs: define tutors and animals domain boundaries` ate `docs: consolidate domain business rules`;
- projetos `.csproj`, referencias de compilacao, composition root da API, AppHost, building blocks, modulo `PetShop.Tutores` e suites de testes;
- entidades de dominio, application services, repositories internos, endpoints, mappings EF Core, migrations, snapshot do modelo e testes arquiteturais.

Nao foram encontrados documentos especificos nomeados como SDD 25. A evidencia local mostra SDDs 12 a 24 consolidados nos ADRs e documentos de dominio, e o commit mais recente consolida o catalogo de regras da Entrega 1. Este SDD assume essa base como concluida.

## Estado atual

A plataforma e um monolito modular .NET com um unico deploy, uma API HTTP, um AppHost Aspire local, PostgreSQL, Keycloak local, building blocks de observabilidade e um unico modulo de negocio implementado.

Modulo de negocio atual:

- `PetShop.Tutores`: representa o bounded context inicial **Cadastro de Tutores e Animais**.

Dados funcionais persistidos:

- `tutores`;
- `animais`;
- `historico_transferencias_animais`.

Aggregates implementados:

- `Tutor`;
- `Animal`.

Registro historico interno:

- `TransferenciaDeResponsabilidadeDoAnimal`, append-only, owned pelo mesmo modulo, sem ser Aggregate Root nesta etapa.

Superficie publica do modulo:

- `AddModuloTutores`;
- `MapModuloTutores`;
- `ConfigurePersistenciaDoModuloTutores`.

Persistencia:

- `PetShopDbContext` tecnico fica na API e centraliza migrations do banco compartilhado do monolito;
- o modulo fornece mapeamentos EF Core por extensao publica;
- todas as tabelas de negocio atuais possuem `tenant_id NOT NULL`;
- FKs entre `animais`, `tutores` e historico incluem `tenant_id`, porque pertencem ao mesmo modulo owner;
- query filters e guardas de `SaveChanges` protegem dados tenant-owned.

Dependencias principais:

- `PetShop.AppHost` referencia `PetShop.Api`;
- `PetShop.Api` referencia `PetShop.Tutores`, `PetShop.Observability` e `PetShop.Observability.AspNetCore`;
- `PetShop.Observability.AspNetCore` referencia `PetShop.Observability`;
- `PetShop.Tutores` nao referencia outros modulos de negocio.

## Diagnostico

O desenho atual esta coerente com um monolito modular inicial. A separacao fisica ainda e pequena, mas intencional: um unico assembly por modulo de negocio evita fragmentacao precoce e permite enforcement basico por testes arquiteturais.

Nao ha evidencia para separar `Tutor` e `Animal` em bounded contexts distintos. A linguagem e as invariantes atuais giram em torno da responsabilidade operacional vigente pelo animal dentro do mesmo tenant. O vinculo pertence ao aggregate `Animal` e ao modulo `PetShop.Tutores`, enquanto nao houver multiplos responsaveis, papeis, vigencia ou consentimento formal.

Tambem nao ha evidencia para criar agora modulos vazios de Catalogo, Workforce, Scheduling, Attendance, Billing ou Notifications. Esses nomes continuam hipoteses de capacidades. Eles devem nascer somente quando houver uma fatia vertical concreta.

## Problemas e lacunas

Nao foram encontradas violacoes claras que exijam correcao de codigo neste SDD:

- nao ha dependencia circular de projeto;
- nao ha outro modulo acessando tabelas, `DbContext`, entities EF Core ou repositories de `PetShop.Tutores`;
- nao ha `IRepository<T>` ou `IService<T>` genericos;
- nao ha projeto `Shared`, `Common` ou `Core` com conceitos de dominio;
- contratos HTTP nao expoem entidades de dominio ou persistencia;
- `IQueryable` nao cruza a fronteira publica do modulo.

Lacunas relevantes:

- o historico de transferencia possui guarda append-only implementada, mas ainda nao tem teste dedicado para update/delete manual; a lacuna ja esta registrada em `BR-REL-005`;
- pesquisas de tutores e animais materializam o conjunto do tenant antes de filtrar por alguns Value Objects; isso e aceitavel para a fatia inicial, mas e hotspot antes de volume real;
- ainda nao existe contrato publico interno para futuros consumidores consultarem dados minimos de tutor e animal;
- nao ha decisao sobre direitos do titular, retencao, exportacao, eliminacao ou suporte administrativo cross-tenant;
- a fonte de produto das regras e documental, via SDDs e ADRs, sem stakeholder nominal registrado.

## Decisoes confirmadas

- Manter **Cadastro de Tutores e Animais** como bounded context confirmado.
- Manter `PetShop.Tutores` como unico modulo fisico para Tutor, Animal, vinculo vigente e historico minimo de transferencia.
- Manter `Tutor` e `Animal` como Aggregate Roots separados dentro do mesmo bounded context.
- Manter o vinculo vigente no aggregate `Animal`, sem Aggregate de vinculo nesta etapa.
- Manter `PetShopDbContext` tecnico unico para migrations do monolito enquanto houver apenas um modulo persistido.
- Manter integridade fisica por FK composta com `tenant_id` apenas dentro do mesmo modulo owner.
- Nao criar Shared Kernel de dominio.
- Nao criar microsservico, broker, API Gateway, cache distribuido, banco por modulo, saga, CQRS completo ou event sourcing.

## Hipoteses de bounded contexts candidatos

| Contexto candidato | Status | Justificativa |
| --- | --- | --- |
| Cadastro de Tutores e Animais | Confirmado | Linguagem, dados, endpoints, aggregates, invariantes e testes ja implementados. |
| Catalogo de Servicos | Candidato | Provavel owner da definicao do servico, duracao padrao e requisitos operacionais, mas ainda sem codigo. |
| Workforce / Profissionais | Candidato | Cadastro profissional, unidades de atuacao, credenciais e papeis operacionais possuem linguagem propria. |
| Disponibilidade | Exige descoberta | Pode pertencer a Workforce, Agenda ou virar capacidade distinta conforme owner das alteracoes e invariantes de calendario. |
| Agenda | Candidato | Reserva de horario, conflitos, recursos e ciclo de agendamento indicam limite de consistencia proprio. |
| Atendimento | Candidato | Check-in, execucao operacional, registro de servico realizado e transicao para cobranca precisam discovery. |
| Prontuario | Adiado / candidato forte | Autoria clinica, evolucao, documentos e correcoes sugerem contexto distinto, mas nao deve ser implementado agora. |
| Cobranca | Candidato | Valores, itens cobrados, orcamento, responsavel financeiro e pagamento nao devem morar em Atendimento. |
| Notifications | Exige descoberta | Pode ser capacidade de comunicacao ou adapter tecnico; consentimento, templates e marketing podem mudar a decisao. |
| Identidade e Acesso | Confirmado como capacidade tecnica externa/interna | Keycloak autentica identidade; usuario/plataforma/permissao nao sao sinonimos de profissional. |
| Auditoria | Adiado | Ainda ha trilhas pontuais; auditoria funcional ampla exige requisitos de compliance e retencao. |
| Assinaturas SaaS | Adiado | Billing da plataforma fornecedora e diferente de cobranca operacional do petshop. |

## Respostas estrategicas

### Tutores e Animais

Permanecem no mesmo bounded context. A separacao atual em modulos distintos nao agregaria clareza; geraria contratos artificiais para uma invariante local: animal deve possuir tutor responsavel operacional ativo do mesmo tenant ao ser cadastrado, e tutor com animal ativo vinculado nao pode ser inativado.

O vinculo pertence ao aggregate `Animal`, representado por `TutorResponsavel`. O historico de transferencia registra fatos minimos, mas nao e entidade de vigencia. Contratos futuros devem ser especificos, por exemplo validar animal ativo, obter resumo de tutor ou obter responsavel operacional atual.

### Catalogo de Servicos

Catalogo e uma capacidade autonoma candidata, desde que comece pequeno. O owner inicial deve ser a definicao operacional do servico oferecido pelo tenant: nome, status, duracao padrao, talvez categoria e requisitos minimos para execucao.

Preco pode nascer como preco padrao informativo no Catalogo quando necessario para cotacao simples, mas ownership financeiro definitivo pertence a Cobranca. Valor efetivamente cobrado, descontos, impostos, pacotes, formas de pagamento e negociacao nao devem entrar no Aggregate `Servico`.

Duracao padrao pertence ao Catalogo como referencia de planejamento. A disponibilidade real do slot e conflito de agenda pertencem a Agenda. Requisitos de profissional ou recurso podem ser referencias leves no Catalogo, mas alocacao concreta pertence a Agenda.

Evitar um Aggregate `Servico` grande mantendo somente definicao e regras estaveis no Catalogo. Variacoes de preco, execucao, escalas e recursos alocados devem ser outros modelos quando houver caso de uso real.

### Profissionais, disponibilidade e Agenda

Profissional como pessoa que executa servico nao e usuario do Keycloak. Identidade autenticada, usuario da plataforma, papel/permissao e profissional do dominio devem continuar separados.

Disponibilidade ainda exige descoberta. A tendencia inicial e tratar disponibilidade operacional basica como parte de Agenda quando a regra principal for "quais slots podem ser reservados", e manter Workforce como owner do cadastro profissional, unidades de atuacao e aptidoes. Se disponibilidade virar escala de trabalho mantida por RH/gestao, com ferias, bloqueios, regras trabalhistas e alteracoes frequentes fora da reserva, ela pode pertencer a Workforce ou a uma capacidade propria.

Recursos fisicos devem ser avaliados junto com Agenda se participarem de conflito de reserva; se forem apenas cadastro patrimonial, nao devem inflar Agenda.

### Agenda e Atendimento

Agendamento termina quando a reserva deixa de ser promessa de horario: cancelamento, nao comparecimento ou transicao confirmada para atendimento/check-in. Check-in pode ser o ponto de transicao entre Agenda e Atendimento, mas no MVP pode permanecer em Agenda se apenas confirmar chegada e ocupar o slot.

Atendimento deve nascer como modulo proprio somente quando houver ciclo operacional distinto: executar servico, registrar responsavel presente, registrar resultado, encaminhar para prontuario ou gerar itens realizados. O MVP pode manter check-in no contexto de Agenda e adiar Atendimento ate haver regra de execucao.

Evitar um Aggregate que represente toda a jornada. `Agendamento` nao deve conter prontuario, cobranca, documentos clinicos ou pagamento.

### Atendimento e Prontuario

Atendimento operacional e Prontuario indicam contextos distintos. Atendimento organiza a execucao do servico e estados operacionais. Prontuario registra informacao clinica, autoria, evolucao, documentos, consentimentos, correcao e finalizacao clinica.

Ainda faltam evidencias para modelar Prontuario: quais profissionais podem assinar, como corrigir registros, quais documentos existem, quais consentimentos sao exigidos e quais retencoes se aplicam.

### Atendimento e Cobranca

Cobranca deve nascer quando houver servico contratado, servico realizado ou ordem de cobranca confirmada. Itens cobrados podem derivar do agendamento/atendimento, mas os valores, descontos, responsabilidade financeira, estado de pagamento e conciliacao pertencem a Cobranca.

Atendimento pode publicar ou expor resultado operacional; Cobranca decide se isso vira item cobrado, orçamento, ajuste ou isencao. Nao colocar regras financeiras dentro de Atendimento.

### Notificacoes

Notifications ainda nao deve ser bounded context confirmado. Pode comecar como adapter tecnico para envio operacional quando houver consumidor. Se surgirem consentimento, preferencias por canal, templates por tenant, marketing, opt-in/opt-out, campanhas e historico de entrega, entao vira capacidade de comunicacao com regras proprias.

### Identidade, usuario e profissional

Keycloak autentica identidade e emite claims. Usuario da plataforma representa acesso ao SaaS. Papel/permissao autoriza operacoes. Pessoa e profissional sao conceitos de dominio. Medico-veterinario e um tipo de profissional com credenciais. Profissional executor de servico pode ou nao ter login. Autoria clinica pertence ao fluxo clinico e nao deve ser inferida apenas por role tecnica.

## Multitenancy por capacidade

Todos os dados operacionais de petshop devem ser tenant-owned por padrao. Dados por unidade devem tambem pertencer ao tenant. Dados globais so devem existir com justificativa explicita e sem vazar configuracao entre organizacoes.

Riscos principais:

- associar animal de um tenant a agendamento, atendimento ou cobranca de outro tenant;
- tratar catalogos como globais sem curadoria e ownership;
- aceitar `tenant_id` de payload em novos modulos;
- usar `tenant_id` como label de metrica;
- criar job, notificacao, outbox ou idempotencia sem tenant no item de trabalho.

Estrategia de teste:

- cada novo fluxo persistente deve exercitar pelo menos dois tenants;
- relacoes entre tabelas tenant-owned devem incluir tenant em constraints ou ser validadas por contrato do modulo owner;
- operacoes administrativas cross-tenant devem ter autorizacao e auditoria explicitas antes de existirem.

## Dependencias e contratos

A regra atual deve permanecer:

- modulos nao acessam `DbContext`, entities EF Core, repositories ou tabelas de outro modulo;
- contratos internos devem ser orientados a caso de uso;
- consultas frequentes entre contextos devem avaliar projection local deliberada;
- eventos internos so devem aparecer quando houver fato de dominio e consistencia eventual aceitavel;
- ACL so deve existir quando houver diferenca semantica real entre modelos.

Contratos provaveis de `PetShop.Tutores` para Agenda/Atendimento:

- validar se animal esta apto para novo fluxo;
- obter snapshot minimo de animal para exibicao;
- obter responsavel operacional vigente;
- validar tutor visivel e ativo para contato operacional.

Esses contratos nao devem expor CPF completo, entidade de dominio, `IQueryable`, `DbContext` ou DTO de Infrastructure.

## Modularizacao fisica

A estrutura fisica atual e adequada para a fase:

- um assembly de modulo de negocio reduz boilerplate;
- pastas `Domain`, `Application`, `Infrastructure` e `Api` sao suficientes;
- testes arquiteturais verificam superficie publica minima, ausencia de ciclos e ausencia de acesso a internals;
- `PetShop.Observability` e building block tecnico, nao Shared Kernel de dominio.

Nao ha beneficio objetivo em reorganizar o repositorio agora.

## Persistencia e transacoes

O `PetShopDbContext` unico continua adequado enquanto houver um modulo persistido. Ele centraliza migrations do monolito e evita custo de varios contexts antes de necessidade real.

Pontos de atencao para proximas entregas:

- FKs fisicas cross-module devem ser evitadas por padrao;
- se Agenda precisar validar Animal e Tutor, usar contrato do modulo owner antes de criar FK entre modulos;
- se uma invariante exigir escrita atomica em varios modulos, reavaliar fronteira antes de aceitar transacao cross-module;
- relatorios futuros podem usar endpoints/projections de leitura, sem integrar por tabela como contrato.

## Decisoes adiadas

- Separar Tutores e Animais fisicamente.
- Criar entidade principal de vinculo entre pessoas e animais.
- Criar Pessoa/Party generica.
- Criar modulo de Prontuario.
- Definir responsavel financeiro.
- Definir disponibilidade como Workforce, Agenda ou contexto proprio.
- Definir Notifications como bounded context ou adapter.
- Criar DbContext por modulo.
- Criar contratos internos executaveis sem consumidor real.
- Criar qualquer infraestrutura assincrona.

## Recomendacao

Prosseguir com uma sequencia incremental:

1. Catalogo de servicos simples;
2. Profissionais;
3. Disponibilidade basica;
4. Agendamento;
5. Consulta de agenda;
6. Check-in;
7. Atendimento operacional;
8. Cobranca basica.

A ordem preserva aprendizado e evita que Agenda seja modelada antes de conhecer servicos e profissionais. Cobranca fica depois de Atendimento operacional minimo para nao confundir preco padrao, contratado, executado e cobrado.

## Alteracoes de codigo

Nenhuma alteracao de producao foi realizada neste SDD. A revisao encontrou dividas documentadas, mas nao uma violacao clara que justificasse mudar codigo, migrations ou testes nesta entrega.
