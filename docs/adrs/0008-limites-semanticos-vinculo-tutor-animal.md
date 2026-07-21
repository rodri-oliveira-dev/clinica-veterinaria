# ADR-0008: Limites semanticos do vinculo Tutor-Animal

- **Status:** Aceita
- **Data:** 2026-07-21
- **Decisao:** nao inferir capacidades clinicas, financeiras, legais ou de privacidade a partir do vinculo operacional entre tutor e animal

## Contexto

O modulo implementado `PetShop.Tutores` mantem o bounded context **Cadastro de Tutores e Animais**. Nele, `Animal` possui um `TutorResponsavel` vigente, persistido em `animais.tutor_responsavel_id`, e a transferencia desse vinculo e uma operacao explicita do proprio modulo.

Os SDDs anteriores ja registravam que `TutorResponsavel` nao equivale a proprietario legal, pagador ou autorizador clinico. O SDD 27 reforca essa decisao porque os futuros candidatos Agenda, Atendimento, Prontuario, Cobranca, Notifications, Auditoria e Privacidade podem precisar de perguntas parecidas, mas semanticamente diferentes.

## Problema

Um unico identificador cadastral pode ser reutilizado indevidamente por modulos futuros como se respondesse a perguntas distintas:

- quem pode ser contatado sobre o animal;
- quem solicitou um agendamento;
- quem acompanhou o animal;
- quem pode consentir um procedimento;
- quem autorizou atendimento ou acesso clinico;
- quem recebera cobranca;
- quem e pagador ou responsavel financeiro;
- quem representa legalmente outra pessoa;
- quem exerce direitos relacionados a dados pessoais.

Essas capacidades podem coincidir em alguns casos reais, mas a coincidencia nao e uma regra do modelo atual.

## Decisao vigente

O vinculo atual entre pessoa e animal representa somente a responsabilidade operacional cadastral vigente dentro do modulo `PetShop.Tutores`.

Ele permite, na fatia atual:

- cadastrar animal com tutor responsavel ativo e visivel no tenant autenticado;
- consultar e pesquisar animais pelo tutor responsavel operacional;
- preservar o tutor responsavel em atualizacoes cadastrais do animal;
- transferir explicitamente a responsabilidade operacional de animal ativo para outro tutor ativo do mesmo tenant;
- impedir inativacao de tutor que ainda possui animal ativo vinculado;
- preservar uma trilha minima de transferencia.

O vinculo cadastral ou operacional entre uma pessoa e um animal nao concede, por si so, autorizacao clinica, consentimento para procedimentos, acesso irrestrito ao prontuario, representacao legal, responsabilidade financeira, condicao de pagador ou exercicio automatico de direitos relacionados aos dados pessoais.

Cada uma dessas capacidades devera possuir regra, contrato, politica e evidencia proprias quando for efetivamente implementada.

## Principio de capacidade explicita

Nenhum modulo deve responder a perguntas distintas utilizando apenas `TutorResponsavelId`, `TutorId`, `ResponsavelId` ou identificador equivalente.

Modulos futuros devem solicitar ou publicar uma capacidade explicita. Exemplos:

- Agenda pode precisar de solicitante, acompanhante ou contato operacional.
- Atendimento pode precisar registrar acompanhante, autorizador do atendimento ou pessoa que recebeu orientacoes.
- Prontuario pode precisar de regras de autoria, acesso, correcao, consentimento e retencao.
- Cobranca pode precisar de contratante, destinatario da cobranca, responsavel financeiro e pagador.
- Privacidade/Compliance pode precisar de verificacao de identidade e fluxo proprio para direitos do titular.

Esses contratos nao devem expor entidades de dominio, entidades EF Core, `DbContext`, repositories ou `IQueryable` de `PetShop.Tutores`.

## Hipoteses e limitacoes atuais

Permanecem hipoteses, nao regras vigentes:

- multiplos responsaveis simultaneos por animal;
- responsavel principal como conceito separado;
- proprietario declarado ou proprietario legal;
- consentimento clinico formal;
- acesso ao prontuario por tutor ou terceiros;
- responsavel financeiro, pagador ou devedor;
- representante legal;
- direitos do titular de dados pessoais;
- pessoa juridica, abrigo, ONG ou associacao;
- historico temporal completo de vinculos.

## Alternativas consideradas

### Renomear `TutorResponsavelId` agora

Rejeitada neste SDD. A semantica operacional do campo ja esta validada e documentada. Renomear contrato HTTP, propriedade de dominio e coluna de banco sem novo comportamento exigiria migration e quebra de compatibilidade com baixo ganho proporcional.

Essa decisao deve ser reavaliada se um novo consumidor real precisar de um contrato interno mais preciso, como `ResponsavelOperacionalAtualId` ou `ContatoOperacionalId`, ou se houver mudanca de contrato planejada.

### Criar modelo completo de capacidades

Rejeitada. Criar entidades para consentimento, prontuario, responsavel financeiro, pagador, representante legal ou direitos LGPD sem fluxo validado transformaria hipoteses em regras vigentes.

### Usar `TutorResponsavelId` como autorizacao ampla ate discovery

Rejeitada por risco de seguranca, privacidade, responsabilidade financeira indevida e acoplamento semantico entre modulos.

## Consequencias positivas

- Reduz risco de inferencia indevida por modulos futuros.
- Mantem simples a fatia validada de Cadastro de Tutores e Animais.
- Preserva isolamento multitenant e ownership do modulo atual.
- Evita criar conceitos futuros sem discovery.
- Torna a superficie HTTP mais clara por descricao contratual.
- Adiciona guardrails automatizados contra publicacao acidental de capacidades futuras no modulo atual.

## Custos e limitacoes

- `TutorResponsavelId` continua sendo um nome amplo no contrato atual e exige leitura da documentacao de semantica.
- Guardrails automatizados nao provam regras futuras que ainda nao existem.
- Modulos futuros terao custo inicial maior porque precisarao definir contratos proprios.
- A matriz de capacidades devera ser atualizada quando Agenda, Atendimento, Prontuario, Cobranca ou Privacidade forem implementados.

## Temas encaminhados para discovery

- Quem pode solicitar, remarcar ou cancelar agendamento.
- Quem pode acompanhar animal em atendimento.
- Quais procedimentos exigem consentimento, forma de evidencia e vigencia.
- Quem pode acessar informacoes clinicas ou prontuario.
- Quem e contratante, responsavel financeiro, destinatario da cobranca e pagador.
- Como verificar representante legal ou procuracao.
- Como executar direitos relacionados a dados pessoais com verificacao de identidade, autorizacao e auditoria.

## Impacto sobre codigo e testes

- O Value Object `TutorResponsavel` documenta explicitamente a ausencia de capacidades clinicas, financeiras, legais e de prontuario.
- Os endpoints de animais que expoem `TutorResponsavelId` possuem descricao OpenAPI com a semantica limitada.
- Testes de arquitetura impedem que `PetShop.Tutores` defina contratos de capacidades futuras como consentimento, prontuario, pagador, responsavel financeiro ou representante legal.
- Testes de contrato OpenAPI impedem que a API publique nomes de entrada/operacao para essas capacidades sem decisao propria.

## Relacao com documentacao

- Documento de dominio: `docs/domain/limites-semanticos-vinculo-animal.md`
- Documento de dominio consolidado: `docs/domain/tutores-e-animais.md`
- ADR complementada: `docs/adrs/0004-relacao-tutores-animais-responsabilidade.md`
- ADR complementada: `docs/adrs/0006-ownership-relacionamento-tutores-animais.md`
- ADR complementada: `docs/adrs/0007-revisao-bounded-contexts-modulos-aggregates.md`
