# Limites semanticos do vinculo entre pessoa e animal

- **Data:** 2026-07-21
- **Escopo:** SDD 27
- **Modulo analisado:** `PetShop.Tutores`

## Inspecao realizada

Foram inspecionados os documentos e pontos executaveis relacionados ao vinculo entre pessoa e animal:

- `README.md`;
- `docs/adrs/0003-fronteira-cadastro-tutores-animais.md`;
- `docs/adrs/0004-relacao-tutores-animais-responsabilidade.md`;
- `docs/adrs/0006-ownership-relacionamento-tutores-animais.md`;
- `docs/adrs/0007-revisao-bounded-contexts-modulos-aggregates.md`;
- `docs/domain/tutores-e-animais.md`;
- `docs/domain/refinamento-responsabilidades-tutores-animais.md`;
- `docs/domain/context-map.md`;
- `docs/domain/matriz-responsabilidades.md`;
- `docs/domain/aggregates.md`;
- `docs/domain/regras-de-negocio/glossario.md`;
- `docs/domain/regras-de-negocio/lacunas-hipoteses-politicas.md`;
- `src/Modules/Tutores/PetShop.Tutores/Domain/Animal.cs`;
- `src/Modules/Tutores/PetShop.Tutores/Domain/TutorResponsavel.cs`;
- `src/Modules/Tutores/PetShop.Tutores/Application/AnimaisApplicationModels.cs`;
- `src/Modules/Tutores/PetShop.Tutores/Application/AnimaisApplicationService.cs`;
- `src/Modules/Tutores/PetShop.Tutores/Api/ModuloTutoresEndpointRouteBuilderExtensions.cs`;
- `src/Modules/Tutores/PetShop.Tutores/Infrastructure/AnimalEntityTypeConfiguration.cs`;
- `src/Modules/Tutores/PetShop.Tutores/Infrastructure/AnimaisRepository.cs`;
- `src/Apps/PetShop.Api/Authentication/SecurityServiceCollectionExtensions.cs`;
- `tests/PetShop.UnitTests/Tutores/Domain/AnimalTests.cs`;
- `tests/PetShop.IntegrationTests/AnimaisApiTests.cs`;
- `tests/PetShop.IntegrationTests/AnimaisPersistenceTests.cs`;
- `tests/PetShop.IntegrationTests/OpenApiContractTests.cs`;
- `tests/PetShop.ArchitectureTests/TutoresModuleBoundaryTests.cs`.

Tambem foram buscadas referencias a `TutorResponsavelId`, `TutorId`, `ResponsavelId`, `ResponsavelPrincipalId`, `ResponsavelAnimalId`, `OwnerId`, `GuardianId`, `PetOwnerId`, `PrimaryTutorId`, `CustomerId`, `ContactId` e equivalentes em portugues ou ingles.

## Modulos encontrados

| Area solicitada | Evidencia no repositorio | Situacao |
| --- | --- | --- |
| Tutores | `PetShop.Tutores`, aggregate `Tutor`, endpoints `/tutores` | Implementado |
| Responsaveis | `TutorResponsavel` como vinculo operacional vigente no `Animal` | Implementado apenas na semantica operacional atual |
| Animais | aggregate `Animal`, endpoints `/animais` | Implementado |
| Agenda | docs de roadmap/context map | Candidato, sem codigo executavel |
| Atendimento | docs de roadmap/context map | Candidato, sem codigo executavel |
| Prontuario | docs de roadmap/context map | Adiado/candidato, sem codigo executavel |
| Faturamento/Cobranca | docs de roadmap/context map | Candidato, sem codigo executavel |
| Identidade/Autorizacao | Keycloak/JWT, policies `DiagnosticsAccess` e `TutoresAccess` | Suporte tecnico implementado |
| Auditoria | historico minimo de transferencia; auditoria funcional ampla documentada como adiada | Parcial e especifico |
| Privacidade/LGPD | minimizacao em contratos atuais; direitos do titular documentados como lacuna | Adiado |

## Semantica atual do vinculo

O vinculo atual representa a responsabilidade operacional cadastral vigente de um tutor sobre um animal dentro do tenant autenticado.

Ele e expresso por:

- `Animal.TutorResponsavel`;
- `Animal.TutorResponsavelId`;
- `TutorResponsavel` como Value Object;
- coluna `animais.tutor_responsavel_id`;
- indice e FK composta com `tenant_id`;
- historico minimo em `historico_transferencias_animais` quando ocorre transferencia.

Operacoes que podem usar esse vinculo hoje:

- cadastrar animal com tutor responsavel ativo do mesmo tenant;
- consultar animal e exibir `tutorResponsavelId`;
- pesquisar animais filtrando por `tutorResponsavelId`;
- preservar o vinculo em atualizacao cadastral;
- transferir explicitamente a responsabilidade operacional;
- bloquear inativacao de tutor quando houver animal ativo vinculado.

## O que o vinculo nao representa

O vinculo cadastral ou operacional entre uma pessoa e um animal nao concede, por si so:

- autorizacao clinica;
- consentimento para procedimentos;
- acesso irrestrito a prontuario ou informacoes clinicas;
- poder de decisao sobre tratamentos;
- representacao legal de outra pessoa;
- responsabilidade financeira;
- condicao automatica de pagador;
- direito automatico de exercer direitos relacionados a dados pessoais;
- qualquer operacao apenas por possuir vinculo cadastral com o animal.

Cada uma dessas capacidades exige regra, contrato, politica e evidencia propria quando for implementada.

## Matriz de capacidades

| Capacidade | Pode ser inferida do vinculo atual? | Contexto responsavel | Situacao |
| --- | ---: | --- | --- |
| Responsabilidade operacional cadastral vigente | Sim, dentro das regras vigentes | Cadastro de Tutores e Animais (`PetShop.Tutores`) | Implementada |
| Contato operacional minimo do tutor | Parcialmente, apenas como dados do tutor cadastrado | Cadastro de Tutores e Animais (`PetShop.Tutores`) | Implementado para e-mail/telefone do tutor, sem multiplos contatos |
| Solicitacao de agenda | Nao automaticamente | Agenda | Pendente de discovery |
| Acompanhante em atendimento | Nao automaticamente | Atendimento | Pendente de discovery |
| Consentimento clinico | Nao | Atendimento/Prontuario a definir | Pendente de discovery |
| Autorizacao de procedimento | Nao | Atendimento/Prontuario a definir | Pendente de discovery |
| Acesso ao prontuario | Nao | Prontuario/Privacidade a definir | Pendente de discovery |
| Responsabilidade financeira | Nao | Cobranca/Faturamento | Pendente de discovery |
| Pagamento ou condicao de pagador | Nao | Cobranca/Faturamento | Pendente de discovery |
| Representacao legal | Nao | A definir | Pendente de discovery |
| Direitos do titular de dados pessoais | Nao | Privacidade/Compliance a definir | Pendente de discovery |
| Auditoria funcional ampla | Nao | Auditoria/Compliance a definir | Adiada |

## Inferencias indevidas encontradas

Nao foram encontradas inferencias executaveis de consentimento clinico, acesso a prontuario, autorizacao de procedimentos, responsabilidade financeira, pagador, representante legal ou direitos do titular baseadas apenas em `TutorResponsavelId` ou `TutorId`.

Tambem nao foram encontrados handlers, policies ou endpoints fazendo comparacao equivalente a `animal.TutorResponsavelId == currentUser.PersonId` para conceder capacidade protegida. As policies atuais exigem usuario autenticado, role `petshop.access` e claim `tenant_id` valida; elas nao interpretam vinculo com animal.

## Ambiguidades e nomes revisados

`TutorResponsavelId` permanece amplo, mas a semantica operacional esta validada nos ADRs e no codigo atual. A renomeacao para `ResponsavelOperacionalId` ou `ContatoPrincipalId` foi evitada porque:

- exigiria alteracao de contrato HTTP e possivel migration;
- nao ha novo consumidor real que precise de outro contrato;
- a semantica atual ja e operacional e esta sendo reforcada por documentacao, OpenAPI e testes;
- nomes como `ContatoPrincipalId` poderiam reduzir indevidamente a semantica validada a contato.

`ResponsavelPrincipal` permanece termo reservado e nao deve virar campo enquanto nao houver multiplos responsaveis simultaneos.

## Contratos e guardrails

Contratos atuais revisados:

- `POST /animais` recebe `tutorResponsavelId` somente para vinculo operacional inicial.
- `GET /animais` aceita `tutorResponsavelId` somente como filtro de vinculo operacional.
- `POST /animais/{animalId}/transferencias-de-responsabilidade` altera somente o tutor responsavel operacional vigente.
- Responses de animais retornam `tutorResponsavelId` sem duplicar dados pessoais do tutor.

Guardrails adicionados:

- comentarios no Value Object `TutorResponsavel` e na propriedade `Animal.TutorResponsavel`;
- descricoes OpenAPI nos endpoints de animais que expoem ou alteram o vinculo;
- teste de arquitetura para impedir que `PetShop.Tutores` defina contratos de capacidades futuras;
- teste OpenAPI para impedir publicacao de nomes de contrato de consentimento, prontuario, pagador, responsavel financeiro, representante legal e correlatos.

## Orientacao para modulos futuros

Modulos futuros devem solicitar uma capacidade explicita, em vez de interpretar um vinculo generico.

Quando Agenda, Atendimento, Prontuario, Cobranca, Notifications, Auditoria ou Privacidade forem implementados, cada SDD deve responder:

- qual pergunta de negocio esta sendo feita;
- qual modulo e owner da resposta;
- qual regra autoriza a operacao;
- qual evidencia sera registrada;
- como o tenant autenticado sera aplicado;
- quais dados pessoais sao estritamente necessarios;
- qual contrato sera exposto;
- quais testes impedem acesso cross-tenant e inferencia indevida.

Nenhum modulo deve acessar diretamente tabelas, entidades EF Core, repositories ou `DbContext` de `PetShop.Tutores` para obter capacidades que pertencem a outro contexto.
