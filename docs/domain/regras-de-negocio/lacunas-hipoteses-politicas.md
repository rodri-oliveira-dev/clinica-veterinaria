# Lacunas, hipoteses e politicas candidatas - Entrega 1

## Resumo

- Regras vigentes catalogadas: 27.
- Hipoteses separadas de regras vigentes: 12.
- Politicas configuraveis candidatas: 5.
- Contradicoes encontradas: 2 documentais, corrigidas neste SDD.
- Contradicoes executaveis encontradas no codigo: nenhuma.

## Contradicoes e duplicidades

| Item | Classificacao | Evidencia | Tratamento |
| --- | --- | --- | --- |
| README listava ADRs completas ate a ADR-0005, mas a ADR-0006 ja existe e esta aceita. | Contradicao documental | `README.md`; `docs/adrs/README.md` | Corrigido no README. |
| `docs/domain/tutores-e-animais.md` dizia que secoes posteriores documentavam decisoes introduzidas pelos SDDs 13 a 20, mas o mesmo documento ja consolida SDDs 21 a 24. | Contradicao documental | `docs/domain/tutores-e-animais.md`; ADR-0006 | Corrigido para SDDs 13 a 24. |
| Algumas regras multitenant aparecem tanto em README quanto em ADRs e documentos de dominio. | Duplicidade controlada | README, ADR-0001, `docs/domain/tutores-e-animais.md` | O catalogo referencia uma unica regra por comportamento e aponta fontes relacionadas. |

## Lacunas registradas

| Lacuna | Classificacao | Impacto | Acao neste SDD | Reavaliacao |
| --- | --- | --- | --- | --- |
| Nao ha teste dedicado para bloquear update/delete manual de `historico_transferencias_animais`; ha guarda implementada e teste que comprova gravacao do historico. | Regra vigente sem cobertura completa | Medio para auditoria operacional futura. | Registrada explicitamente em BR-REL-005 e na matriz. | Adicionar teste se o historico ganhar auditoria, retencao ou APIs administrativas. |
| Fonte de produto/stakeholder das regras e indireta, via SDDs e ADRs versionados; nao ha registro nominal de stakeholder. | Regra com fonte documental, sem stakeholder nominal | Baixo agora; pode dificultar priorizacao futura. | Mantida como fonte documental, sem inventar stakeholder. | Revisar em discovery da proxima entrega. |
| Direitos do titular, retencao, exportacao, bloqueio e eliminacao de dados pessoais permanecem fora da Entrega 1. | Validacao especializada necessaria | Alto quando houver fluxos LGPD. | Registrada como fora de escopo; nenhuma regra vigente criada. | Usar skill LGPD quando esses fluxos forem especificados. |
| Agenda ainda nao existe; elegibilidade para novos agendamentos nao e regra executavel atual. | Hipotese / risco para proxima entrega | Alto para Entrega de Agenda. | Mantida como risco, sem regra vigente. | Ao criar Agenda, validar situacao `Ativo` e impacto de `Inativo`/`Falecido`. |
| Pessoa juridica, abrigo, associacao ou orgao publico nao possuem modelo nem regra vigente. | Questao em aberto | Medio para cadastros reais. | Mantido fora do catalogo vigente. | Reabrir quando produto pedir cadastro nao pessoa fisica. |

## Hipoteses e questoes em aberto

| ID | Informacao | Categoria | Estado atual | Risco |
| --- | --- | --- | --- | --- |
| HYP-TUT-001 | CPF pode virar obrigatorio por tenant ou por tipo de atendimento. | Hipotese | Nao implementado; CPF vigente e opcional. | Quebrar cadastro operacional se virar requisito sem migration e UX. |
| HYP-TUT-002 | Cadastro de pessoa juridica, abrigo, associacao ou orgao publico. | Hipotese | Nao implementado; `Tutor` representa pessoa operacional cadastrada. | Usar CPF como regra universal indevida. |
| HYP-TUT-003 | Deduplicacao assistida de tutores. | Hipotese | Nao implementado. | Duplicidades operacionais podem exigir merge auditado. |
| HYP-ANI-001 | Idade estimada, mes/ano ou data aproximada. | Hipotese | Nao implementado; data exata opcional. | Fabricar precisao ou bloquear cadastros incompletos. |
| HYP-ANI-002 | Microchip ou identificadores externos. | Hipotese | Nao implementado. | Definir unicidade errada sem discovery. |
| HYP-ANI-003 | Estado `desaparecido`, `duplicado` ou `arquivado`. | Hipotese | Nao implementado; estados vigentes sao `ativo`, `inativo`, `falecido`. | Agenda e Atendimento podem exigir semantica diferente. |
| HYP-ANI-004 | Catalogos de especie e raca. | Hipotese | Nao implementado; ambos sao textuais. | Curadoria e manutencao por tenant podem surgir. |
| HYP-REL-001 | Multiplos responsaveis simultaneos por animal. | Hipotese | Nao implementado; um tutor responsavel vigente. | `TutorResponsavelId` pode virar insuficiente. |
| HYP-REL-002 | Responsavel principal como conceito separado. | Termo reservado | Nao existe como dado separado. | Criar coluna antes de multiplos responsaveis geraria duplicidade sem valor. |
| HYP-REL-003 | Responsavel financeiro ou pagador. | Hipotese | Fora de Cobranca atual. | Cobrar tutor operacional indevidamente. |
| HYP-REL-004 | Proprietario declarado ou proprietario legal. | Hipotese | Nao inferido de Tutor. | Confundir operacao cadastral com propriedade legal. |
| HYP-REL-005 | Historico completo de vigencia de vinculos. | Hipotese | Historico atual registra eventos minimos de transferencia. | Auditoria futura pode exigir tabela principal de vinculo. |
| HYP-REL-006 | Consentimento clinico, acesso ao prontuario, autorizador clinico, representante legal e direitos do titular. | Hipotese / discovery obrigatorio | Nao implementado; o vinculo operacional nao concede essas capacidades. | Vazamento clinico, consentimento invalido, representacao indevida ou tratamento de dados sem regra propria. |

## Politicas configuraveis candidatas

Estas politicas podem variar por tenant no futuro, mas nao foram implementadas porque nao existe infraestrutura de configuracao nem decisao aprovada.

| Politica | Comportamento padrao atual | Motivo da possivel variacao | Risco de configuracao | Quem poderia alterar | Reavaliacao |
| --- | --- | --- | --- | --- | --- |
| CPF obrigatorio ou opcional | CPF opcional; unico por tenant quando informado. | Clinicas podem ter politicas diferentes de identificacao. | Bloquear cadastro legitimo ou aceitar cadastro sem documento onde seja obrigatorio. | Administrador do tenant com permissao especifica, se existir. | Quando houver configuracao por tenant ou requisitos regulatorios/contratuais. |
| Contato minimo do tutor | Ao menos e-mail ou telefone. | Algumas operacoes podem exigir telefone, e-mail ou multiplos contatos. | Reduzir contactabilidade ou impedir fluxo presencial. | Administrador do tenant, se existir. | Quando surgirem notificacoes, confirmacoes ou cobranca. |
| Catalogo de especie/raca | Texto livre validado como nao vazio. | Clinicas podem querer catalogo proprio ou padronizacao. | Criar divergencia entre tenants e dificultar relatorios. | Administrador do tenant ou curador operacional. | Antes de importacao em massa ou analiticos por especie/raca. |
| Bloqueio de agenda por situacao do animal | Agenda nao implementada; documentado que `Falecido` e `Inativo` devem ser considerados. | Alguns tenants podem permitir agendamento administrativo para inativos. | Permitir atendimento indevido para falecido ou bloquear follow-up administrativo necessario. | Produto/operacao; nao deve ser configuracao livre sem guardrails. | Ao implementar Agenda. |
| Campos complementares obrigatorios do animal | Raca, cor/pelagem e observacao sao opcionais. | Tenants podem exigir completude maior. | Inflar cadastro obrigatorio e piorar dados com preenchimento ficticio. | Administrador do tenant, se houver configuracao validada. | Quando houver onboarding por segmento ou importacao legada. |

## Validacoes especializadas necessarias

- Privacidade/LGPD para retencao, exportacao, eliminacao, bloqueio e minimizacao em novos contratos.
- Discovery de dominio para Agenda antes de transformar situacao do animal em regra de agendamento.
- Discovery de Billing antes de usar tutor operacional como responsavel financeiro.
- Revisao de ownership se outro modulo precisar de leitura frequente de tutor/animal.
- Modelagem de vinculos se houver multiplos responsaveis, vigencia, suspensao, autorizacao ou disputa.
