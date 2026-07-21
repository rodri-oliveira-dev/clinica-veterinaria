# ADR-0007: Revisao estrategica de bounded contexts, modulos e Aggregates

- **Status:** Aceita
- **Data:** 2026-07-21
- **Decisao:** confirmar Cadastro de Tutores e Animais como bounded context inicial e manter demais capacidades como candidatas ate discovery e fatia vertical

## Contexto

Os SDDs 12 a 25 consolidaram a fundacao tecnica e a primeira fatia vertical de negocio. O repositorio contem um monolito modular .NET, multitenant, com observabilidade distribuida, PostgreSQL, Keycloak local, `PetShopDbContext` tecnico e um modulo de negocio implementado: `PetShop.Tutores`.

Antes de implementar Catalogo de Servicos, Profissionais, Disponibilidade, Agenda, Atendimento, Prontuario, Cobranca ou Notifications, era necessario revisar se os nomes candidatos representam bounded contexts reais, modulos fisicos, adapters ou apenas hipoteses.

## Decisao

Confirmar o bounded context **Cadastro de Tutores e Animais** como limite vigente da primeira entrega.

Manter `PetShop.Tutores` como modulo fisico unico para:

- cadastro de tutores;
- cadastro de animais;
- vinculo operacional vigente entre animal e tutor responsavel;
- historico minimo de transferencia de responsabilidade.

Manter `Tutor` e `Animal` como Aggregate Roots separados dentro do mesmo bounded context.

Manter o vinculo vigente owned pelo aggregate `Animal`, sem criar Aggregate de vinculo, entidade principal de relacionamento, `Pessoa` generica, Shared Kernel de dominio ou contratos internos sem consumidor real.

Classificar como candidatos, nao confirmados:

- Catalogo de Servicos;
- Profissionais / Workforce;
- Agenda;
- Atendimento;
- Cobranca.

Classificar como decisoes adiadas ou exigindo descoberta:

- Disponibilidade como parte de Workforce, Agenda ou contexto proprio;
- Prontuario como possivel bounded context clinico separado;
- Notifications como adapter tecnico, capacidade de comunicacao ou bounded context;
- Auditoria funcional ampla;
- Assinaturas SaaS da plataforma.

## Decisoes de nao separar agora

Tutores e Animais nao serao separados agora porque:

- a linguagem atual e coesa;
- o vinculo operacional vigente e uma invariante local;
- as tabelas pertencem ao mesmo owner;
- a FK composta com `tenant_id` reforca integridade dentro do mesmo modulo;
- separar exigiria contratos artificiais sem autonomia real.

Catalogo, Workforce, Agenda, Atendimento, Prontuario, Cobranca e Notifications nao serao criados como projetos vazios. Cada capacidade deve nascer somente com uma fatia vertical concreta e uma decisao de ownership.

## Relacoes

Relacoes confirmadas:

- API compoe `PetShop.Tutores` por `AddModuloTutores`, `MapModuloTutores` e `ConfigurePersistenciaDoModuloTutores`;
- `PetShop.Tutores` usa o `PetShopDbContext` tecnico do monolito por composicao;
- `Animal` referencia `Tutor` por identidade dentro do mesmo modulo owner;
- observabilidade e tenancy sao building blocks/suporte tecnico, nao bounded contexts de negocio.

Relacoes hipoteticas:

- Agenda consumira contratos de Cadastro de Tutores e Animais para validar animal/responsavel;
- Agenda consumira Catalogo para duracao e servico ativo;
- Agenda consumira Workforce/Disponibilidade para profissional e slots;
- Atendimento podera nascer de Agenda;
- Cobranca consumira Atendimento e Catalogo por snapshot/contrato;
- Notifications podera consumir fatos de Agenda, Atendimento ou Cobranca quando houver decisao.

## Consequencias positivas

- Evita fragmentacao prematura por substantivos do dominio.
- Mantem o monolito modular simples e verificavel.
- Preserva baixo custo de mudanca para a proxima entrega.
- Deixa claro que `TutorResponsavel` nao significa pagador, proprietario legal ou autorizador clinico.
- Orienta novos modulos a dependerem de contratos e nao de tabelas internas.

## Consequencias negativas e custos

- A API ainda conhece pontos de composicao de persistencia do modulo.
- O `PetShopDbContext` central pode virar acoplamento se muitos modulos persistidos surgirem sem reavaliacao.
- Contratos internos de consulta ainda nao existem e precisarao ser criados quando Agenda ou Atendimento precisarem deles.
- Algumas decisoes importantes, como owner de disponibilidade e separacao de prontuario, permanecem abertas.

## Criterios de reavaliacao

Reavaliar esta ADR se:

- outro modulo precisar ler frequentemente Tutor/Animal com SLA proprio;
- surgir transacao recorrente entre modulos;
- disponibilidade tiver owner e invariantes diferentes de Agenda;
- Atendimento e Prontuario passarem a misturar regras operacionais e clinicas;
- Cobranca precisar definir responsavel financeiro separado de tutor operacional;
- Notifications ganhar consentimento, templates, preferencias e marketing;
- o `PetShopDbContext` tecnico deixar de ser suficiente para preservar ownership.

## Validacao

Documentacao criada:

- `docs/domain/revisao-estrategica-sdd-26.md`;
- `docs/domain/context-map.md`;
- `docs/domain/matriz-responsabilidades.md`;
- `docs/domain/aggregates.md`;
- `docs/domain/roadmap-tecnico-funcional.md`.

Nao foram realizadas alteracoes de producao, migrations ou testes neste SDD.
