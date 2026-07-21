# Backlog de discovery de dominio

- **Data:** 2026-07-21
- **Escopo:** SDD 28
- **Natureza:** governanca de conhecimento, sem implementacao de codigo

## Objetivo

Preservar a diferenca entre regras vigentes, hipoteses, questoes abertas, decisoes adiadas e hipoteses descartadas durante a evolucao da plataforma veterinaria.

Este backlog nao cria requisitos, contratos, entidades, tabelas, autorizacoes ou invariantes. Ele registra o que ainda precisa de evidencia antes de orientar implementacao.

## Documentos inspecionados

Foram efetivamente inspecionados:

- `README.md`;
- `docs/adrs/README.md`;
- `docs/adrs/0001-multitenancy-claim-e-isolamento-por-linha.md`;
- `docs/adrs/0003-fronteira-cadastro-tutores-animais.md`;
- `docs/adrs/0004-relacao-tutores-animais-responsabilidade.md`;
- `docs/adrs/0005-ciclo-de-vida-animal.md`;
- `docs/adrs/0006-ownership-relacionamento-tutores-animais.md`;
- `docs/adrs/0007-revisao-bounded-contexts-modulos-aggregates.md`;
- `docs/adrs/0008-limites-semanticos-vinculo-tutor-animal.md`;
- `docs/domain/tutores-e-animais.md`;
- `docs/domain/refinamento-responsabilidades-tutores-animais.md`;
- `docs/domain/refinamento-ciclo-de-vida-animal.md`;
- `docs/domain/limites-semanticos-vinculo-animal.md`;
- `docs/domain/revisao-estrategica-sdd-26.md`;
- `docs/domain/context-map.md`;
- `docs/domain/matriz-responsabilidades.md`;
- `docs/domain/aggregates.md`;
- `docs/domain/roadmap-tecnico-funcional.md`;
- `docs/domain/regras-de-negocio/README.md`;
- `docs/domain/regras-de-negocio/convencao.md`;
- `docs/domain/regras-de-negocio/catalogo-entrega-1.md`;
- `docs/domain/regras-de-negocio/matriz-rastreabilidade.md`;
- `docs/domain/regras-de-negocio/lacunas-hipoteses-politicas.md`;
- `docs/domain/regras-de-negocio/glossario.md`;
- `docs/security/cybersecurity-skills-migration.md`;
- `docs/security/threat-models/_template.md`.

Tambem foram buscados termos relacionados a hipotese, regra, requisito, decisao, pendencia, questao aberta, discovery, LGPD, consentimento, responsavel financeiro, pagador, abrigo, ONG, pessoa juridica, multiplos responsaveis, historico de vinculo, prontuario e autorizacao.

## Taxonomia

Estados principais permitidos neste backlog:

| Estado | Definicao | Pode orientar implementacao? |
| --- | --- | --- |
| Vigente | Regra, decisao ou conhecimento validado por fonte registrada, com contexto de aplicacao definido e criterio verificavel. | Sim, quando tambem houver regra ou ADR relacionada. |
| Hipótese | Possivel necessidade, comportamento ou modelo ainda nao confirmado. | Nao. |
| Questão aberta | Pergunta objetiva cuja resposta e necessaria para avancar a modelagem. | Nao. |
| Decisão adiada | Tema conhecido postergado deliberadamente, com gatilho de retomada e limite da solucao temporaria. | Somente dentro do limite temporario registrado. |
| Descartada | Hipotese analisada e rejeitada, mantida para evitar rediscussao sem nova evidencia. | Nao. |

Mapeamento com termos ja existentes:

| Termo existente | Mapeamento neste backlog | Observacao |
| --- | --- | --- |
| `Vigente`, `Aceito`, `Fato confirmado`, `Decisao vigente`, `Confirmado` | Vigente | Usar quando houver fonte e contexto definidos. |
| `Hipotese`, `Candidato`, `Exige descoberta`, `Termo reservado`, `Dependente de descoberta futura` | Hipótese ou Questão aberta | O item deve ter pergunta de validacao quando bloquear modelagem. |
| `Adiada`, `Fora da entrega`, `Fora do MVP` | Decisão adiada | Registrar gatilho de retomada e risco do adiamento. |
| `Rejeitada`, `Descartada`, `Proibido como sinonimo` | Descartada | Preservar motivo e evidencia. |

Termos como `provavel`, `futuro`, `pendente`, `provisorio`, `em avaliacao` e `a confirmar` podem aparecer como comentario, mas nao substituem o estado principal.

## Prioridade

Escala simples:

| Prioridade | Criterio |
| --- | --- |
| Alta | Bloqueia MVP proximo, envolve risco regulatorio, seguranca, financeiro, vazamento de dados ou alto custo de mudanca se decidido tarde. |
| Media | Afeta uma capacidade candidata relevante, mas pode ser contornada por escopo reduzido sem criar regra falsa. |
| Baixa | Nao bloqueia o MVP atual e pode ser retomada quando houver demanda concreta. |

## Criterios para promover hipotese

Uma hipotese so pode virar regra vigente quando houver:

1. pergunta de negocio respondida;
2. fonte da resposta identificada;
3. contexto de aplicacao definido;
4. excecoes conhecidas registradas;
5. impacto em outros modulos analisado;
6. impacto multitenant analisado;
7. impacto em autorizacao analisado;
8. impacto regulatorio analisado, quando aplicavel;
9. terminologia alinhada ao glossario;
10. contradicoes resolvidas;
11. criterio verificavel;
12. responsavel pela validacao identificado.

Ao promover uma hipotese, preserve o item `DISC-*`, atualize o estado, registre a evidencia, crie ou referencie a regra `BR-*`, a ADR, a implementacao e os testes quando existirem.

## Mapa de rastreabilidade

| Item de discovery | Estado | Regra relacionada | Modulo potencial | ADR | Impacto no MVP | Proxima acao |
| --- | --- | --- | --- | --- | --- | --- |
| DISC-001 Agenda e politicas operacionais | Questão aberta | Nenhuma regra vigente; relacionado a BR-ANI-005, BR-ANI-007, BR-REL-001 | Agenda, Catalogo, Workforce, Atendimento, Notifications | ADR-0007, ADR-0008 | Bloqueia Agenda do MVP operacional | Event storming com operacao e produto antes de SDD de Agenda |
| DISC-002 LGPD e direitos dos titulares | Questão aberta | Nenhuma regra vigente; relacionado a BR-TEN-001..006 e regras de minimizacao atuais | Privacidade/Compliance, Auditoria, Tutores, Prontuario, Cobranca | ADR-0001, ADR-0008 | Bloqueia fluxos de direitos e acesso/exportacao de dados | Discovery juridico e privacidade; nao produzir parecer sem especialista |
| DISC-003 Pessoa juridica, abrigo, ONG e protetor | Questão aberta | Nenhuma regra vigente; relacionado a HYP-TUT-002 | Tutores/Animais, Organizacoes e Unidades, Cobranca, Identidade | ADR-0004, ADR-0006, ADR-0008 | Nao bloqueia cadastro PF atual; bloqueia cenarios institucionais | Entrevistar operacao sobre abrigos/ONGs e faturamento institucional |
| DISC-004 Multiplos responsaveis por animal | Questão aberta | BR-REL-001 continua vigente ate nova evidencia | Tutores/Animais, Agenda, Atendimento, Prontuario, Cobranca | ADR-0004, ADR-0006, ADR-0008 | Nao bloqueia Entrega 1; bloqueia fluxos com guarda compartilhada | Modelar papeis, conflitos, vigencia e autorizacoes antes de schema |
| DISC-005 Responsavel financeiro, pagador e destinatario da cobranca | Questão aberta | Nenhuma regra vigente; nao inferir de BR-REL-001 | Cobranca, Atendimento, Tutores/Animais, Notifications | ADR-0006, ADR-0008 | Bloqueia Cobranca basica confiavel | Discovery financeiro antes de qualquer contrato de cobranca |
| DISC-006 Consentimento e autorizacao clinica | Questão aberta | Nenhuma regra vigente; nao inferir de TutorResponsavel | Atendimento, Prontuario, Identidade e Acesso, Auditoria | ADR-0008 | Bloqueia procedimentos com consentimento | Validar tipos de procedimento, evidencias, urgencia e autorizadores |
| DISC-007 Acesso ao prontuario | Questão aberta | Nenhuma regra vigente; nao inferir de TutorResponsavel | Prontuario, Privacidade/Compliance, Identidade e Acesso, Auditoria | ADR-0008 | Bloqueia Prontuario com acesso externo | Threat modeling e discovery clinico/privacidade antes do modulo |
| DISC-008 Historico temporal dos vinculos pessoa-animal | Decisão adiada | BR-REL-005 cobre apenas trilha minima de transferencia | Tutores/Animais, Atendimento, Cobranca, Prontuario, Auditoria | ADR-0004, ADR-0005, ADR-0006 | Nao bloqueia Entrega 1; bloqueia reconstrucoes historicas completas | Retomar quando houver multiplos responsaveis, disputa ou auditoria completa |
| DISC-009 Ownership de disponibilidade | Questão aberta | Nenhuma regra vigente | Workforce, Agenda, Organizacoes e Unidades | ADR-0007 | Bloqueia disponibilidade antes de Agenda | Decidir se escala/bloqueio pertence a Workforce, Agenda ou contexto proprio |
| DISC-010 Profissional, usuario e autoria | Questão aberta | Nenhuma regra vigente alem de autenticacao tecnica | Workforce, Identidade e Acesso, Atendimento, Prontuario | ADR-0007, ADR-0008 | Bloqueia autoria clinica e aptidao profissional | Separar login, permissao tecnica, profissional executor e autoria clinica |
| DISC-011 Catalogo de servicos, duracao e preco de referencia | Hipótese | Nenhuma regra vigente | Catalogo, Agenda, Cobranca, Atendimento | ADR-0007 | Pode bloquear Agenda se duracao nao for validada | Validar se duracao e obrigatoria e se preco pertence ao Catalogo ou Cobranca |
| DISC-012 Notifications, preferencias e consentimento de comunicacao | Hipótese | Nenhuma regra vigente | Notifications, Agenda, Cobranca, Privacidade/Compliance | ADR-0002, ADR-0007, ADR-0008 | Nao bloqueia MVP sem notificacao; bloqueia comunicacao automatica | Definir canal, finalidade, opt-in/opt-out e minimizacao de payload |
| DISC-013 Modelo generico de Pessoa/Party para antecipar papeis | Descartada | Nao aplicavel | Tutores/Animais, Workforce, Cobranca, Organizacoes | ADR-0003, ADR-0004, ADR-0006, ADR-0008 | Evita complexidade prematura | Reabrir somente com regras comuns reais entre pessoas, profissionais, organizacoes e pagadores |

## Itens de discovery

### DISC-001 Agenda e politicas operacionais

- **Estado:** Questão aberta
- **Prioridade:** Alta
- **Contexto:** Agenda ainda nao existe como codigo. Roadmap indica Agenda apos Catalogo, Profissionais e decisao de Disponibilidade.
- **Pergunta principal:** Quem pode solicitar, confirmar, cancelar, remarcar ou comparecer a um agendamento dentro do tenant?
- **Perguntas secundarias:** ha vinculo previo com animal; regras de no-show, encaixe, recorrencia, unidade, multiplos profissionais, multiplos recursos, restricoes por especie/servico, contato operacional, responsavel por levar/buscar.
- **Hipotese inicial:** Agenda precisara consultar animal, servico, profissional e disponibilidade por contratos especificos.
- **Por que nao e regra vigente:** nao ha stakeholder, fluxo validado, contrato, modulo ou teste de Agenda.
- **Evidencia disponivel:** ADR-0007 e roadmap registram Agenda como candidata; ADR-0008 impede inferir solicitante/pagador/autorizador de `TutorResponsavelId`.
- **Evidencia necessaria:** event storming com operacao, politica de cancelamento/no-show, concorrencia de slots, unidades e recursos.
- **Fonte esperada:** produto, operacao de clinica, discovery de processos.
- **Stakeholders:** recepcao, gestores de unidade, veterinarios, produto.
- **Modulos/Aggregates potencialmente afetados:** Agenda/`Agendamento`, Catalogo/`Servico`, Workforce/`Profissional`, Disponibilidade, Atendimento, Notifications.
- **Impactos:** autorizacao alta; multitenancy alto; financeiro medio; dados pessoais medio; regulatorio baixo/medio conforme atendimento.
- **Risco de decidir cedo:** criar reserva sem regra de conflito, solicitante ou cancelamento; acoplar Agenda ao cadastro de Tutores por tabela.
- **Risco de nao decidir:** atrasar SDD de Agenda e forcar suposicoes em contratos.
- **Criterio de conclusao:** estados e comandos de Agenda validados, conflitos definidos, atores autorizados e contratos upstream descritos.
- **Gatilho:** inicio de qualquer SDD de Agenda, disponibilidade ou check-in.

### DISC-002 LGPD e direitos dos titulares

- **Estado:** Questão aberta
- **Prioridade:** Alta
- **Contexto:** dados pessoais de tutores ja existem, mas direitos dos titulares, retencao, exportacao, bloqueio, eliminacao e portabilidade estao fora da Entrega 1.
- **Pergunta principal:** Quais titulares, dados, bases legais, fluxos de atendimento e evidencias devem existir para cumprir as obrigacoes de privacidade?
- **Perguntas secundarias:** controlador/operador, dados de menores, prontuario, compartilhamento entre unidades/profissionais, anonimização, auditoria, consentimentos quando aplicavel.
- **Hipotese inicial:** Privacidade/Compliance podera exigir workflows proprios e auditoria funcional separada de logs tecnicos.
- **Por que nao e regra vigente:** conclusoes juridicas exigem validacao especializada; o SDD 28 nao produz parecer juridico.
- **Evidencia disponivel:** README, ADR-0001, catalogo e lacunas registram minimizacao atual e ausencia de fluxos LGPD.
- **Evidencia necessaria:** mapeamento de dados pessoais, bases legais validadas, prazos de retencao, fluxo DSAR, verificacao de identidade e excecoes legais/regulatorias.
- **Fonte esperada:** juridico/privacidade, seguranca, produto, controlador do dado.
- **Stakeholders:** responsavel por privacidade, juridico, seguranca, suporte, produto.
- **Modulos/Aggregates potencialmente afetados:** Privacidade/Compliance, Auditoria, Tutores, Prontuario, Cobranca, Notifications.
- **Impactos:** regulatorio alto; seguranca alto; dados pessoais alto; multitenancy alto.
- **Risco de decidir cedo:** eliminar dado que deve ser retido, expor prontuario indevidamente ou aceitar solicitacao sem identidade verificada.
- **Risco de nao decidir:** criar novos contratos com dados pessoais sem retencao, auditoria ou finalidade clara.
- **Criterio de conclusao:** fluxo validado por profissional qualificado, matriz de dados e titulares, regra de autorizacao, auditoria e testes de isolamento definidos.
- **Gatilho:** qualquer SDD de exportacao, eliminacao, retencao, prontuario, suporte administrativo ou compartilhamento de dados.

### DISC-003 Pessoa juridica, abrigo, ONG e protetor

- **Estado:** Questão aberta
- **Prioridade:** Media
- **Contexto:** `Tutor` atual representa pessoa operacional cadastrada, sem modelo de organizacao, CNPJ, representante ou colaboradores.
- **Pergunta principal:** Uma organizacao pode assumir responsabilidade operacional, financeira ou legal por animais dentro de um tenant?
- **Perguntas secundarias:** distinguir organizacao e contato individual; representante; colaboradores; faturamento PJ; muitos animais; troca de representante; auditoria; impacto multitenant.
- **Hipotese inicial:** abrigos, ONGs e protetores podem exigir modelo de organizacao, mas nao necessariamente uma hierarquia generica de partes.
- **Por que nao e regra vigente:** nao ha fluxo confirmado nem contrato que exija pessoa juridica.
- **Evidencia disponivel:** lacunas e ADR-0008 registram pessoa juridica/abrigo/ONG como hipotese.
- **Evidencia necessaria:** cenarios reais de cadastro institucional, faturamento, responsabilidade por animais e acesso por colaboradores.
- **Fonte esperada:** produto, operacao, financeiro, juridico.
- **Stakeholders:** clinicas parceiras, abrigos/ONGs, financeiro, suporte.
- **Modulos/Aggregates potencialmente afetados:** Tutores/Animais, Organizacoes e Unidades, Cobranca, Identidade e Acesso.
- **Impactos:** multitenancy alto; financeiro medio/alto; dados pessoais medio; autorizacao medio.
- **Risco de decidir cedo:** transformar CPF em regra universal ou criar `Pessoa` generica sem invariantes.
- **Risco de nao decidir:** bloquear clientes institucionais ou improvisar campos em Tutor.
- **Criterio de conclusao:** tipos de organizacao, representantes, permissao, cobranca e vinculos com animais validados.
- **Gatilho:** pedido de cadastro de PJ, abrigo, ONG, protetor institucional ou faturamento CNPJ.

### DISC-004 Multiplos responsaveis por animal

- **Estado:** Questão aberta
- **Prioridade:** Alta
- **Contexto:** regra vigente BR-REL-001 mantem exatamente um tutor responsavel operacional vigente por animal.
- **Pergunta principal:** Um animal pode ter mais de uma pessoa ou organizacao vinculada simultaneamente, com papeis diferentes?
- **Perguntas secundarias:** quantidade, responsavel principal, prioridade de contato, divergencia, permissoes individuais, vigencia, remocao, transferencia, falecimento, disputa, restricao legal, visibilidade.
- **Hipotese inicial:** multiplos responsaveis podem exigir entidade tenant-owned de vinculo com papel e vigencia.
- **Por que nao e regra vigente:** ADR-0004/0006 rejeitaram implementacao sem fluxo confirmado.
- **Evidencia disponivel:** historico minimo de transferencia; documentos de SDD 22, 24 e 27.
- **Evidencia necessaria:** processos de guarda compartilhada, disputa, autorizacao e contato operacional.
- **Fonte esperada:** operacao, juridico quando houver disputa/restricao, produto.
- **Stakeholders:** recepcao, veterinarios, suporte, juridico, tutores.
- **Modulos/Aggregates potencialmente afetados:** Tutores/Animais, Agenda, Atendimento, Prontuario, Cobranca, Auditoria.
- **Impactos:** autorizacao alto; seguranca alto; multitenancy alto; dados pessoais alto; custo de mudanca alto.
- **Risco de decidir cedo:** criar colecao generica de papeis sem regras claras ou quebrar contrato atual.
- **Risco de nao decidir:** novos modulos tratarem `TutorResponsavelId` como permissao ampla.
- **Criterio de conclusao:** papeis, vigencia, conflitos, autorizacoes, migracao e testes definidos.
- **Gatilho:** requisito de guarda compartilhada, restricao legal, responsaveis simultaneos ou historico completo.

### DISC-005 Responsavel financeiro, pagador e destinatario da cobranca

- **Estado:** Questão aberta
- **Prioridade:** Alta
- **Contexto:** Cobranca e candidata futura; `TutorResponsavelId` nao representa pagador.
- **Pergunta principal:** Quem contrata, quem recebe cobranca, quem paga e quem responde por inadimplencia?
- **Perguntas secundarias:** multiplos pagadores, divisao, PF/PJ, emergencia, alteracao apos atendimento, estorno, reembolso, convenios e planos.
- **Hipotese inicial:** Cobranca precisara separar contratante, destinatario, responsavel financeiro e pagamento efetivo.
- **Por que nao e regra vigente:** nao existe modulo de Cobranca, contrato financeiro ou regra validada.
- **Evidencia disponivel:** ADR-0006, ADR-0007, ADR-0008 e roadmap indicam que tutor operacional nao e pagador.
- **Evidencia necessaria:** processo financeiro, regras de inadimplencia, notas/recibos, planos e autorizacao de alteracao.
- **Fonte esperada:** financeiro, produto, operacao, juridico/contabil quando aplicavel.
- **Stakeholders:** caixa/financeiro, gestores, tutores/pagadores, produto.
- **Modulos/Aggregates potencialmente afetados:** Cobranca/`Cobranca` ou `ContaAReceber`, Atendimento, Catalogo, Tutores/Animais, Notifications.
- **Impactos:** financeiro alto; dados pessoais medio; autorizacao medio; multitenancy alto.
- **Risco de decidir cedo:** cobrar tutor errado, expor cobranca a pessoa indevida ou modelar valor no contexto errado.
- **Risco de nao decidir:** Cobranca basica ficar bloqueada ou nascer com divida semantica cara.
- **Criterio de conclusao:** papeis financeiros definidos, eventos de criacao/alteracao, autorizacao e evidencias de pagamento.
- **Gatilho:** SDD de Cobranca, pagamento, recibo, inadimplencia, convenio ou plano.

### DISC-006 Consentimento e autorizacao clinica

- **Estado:** Questão aberta
- **Prioridade:** Alta
- **Contexto:** consentimento clinico nao existe e nao pode ser inferido do vinculo operacional.
- **Pergunta principal:** Quais procedimentos exigem consentimento ou autorizacao, e quem pode conceder?
- **Perguntas secundarias:** forma de registro, validade, revogacao, consentimento verbal, emergencia, recusa, alteracao de escopo, assinatura, evidencias, auditoria e acesso posterior.
- **Hipotese inicial:** consentimento pode pertencer a Atendimento, Prontuario ou capacidade propria conforme regra clinica.
- **Por que nao e regra vigente:** falta validacao clinica, juridica e operacional.
- **Evidencia disponivel:** ADR-0008 e documento de limites semanticos registram explicitamente que o vinculo nao concede consentimento.
- **Evidencia necessaria:** lista de procedimentos, regras de urgencia, forma de assinatura/evidencia e poderes do autorizador.
- **Fonte esperada:** veterinarios, juridico, privacidade, produto.
- **Stakeholders:** veterinarios, recepcao, responsaveis pelo animal, juridico.
- **Modulos/Aggregates potencialmente afetados:** Atendimento, Prontuario, Identidade e Acesso, Auditoria, Tutores/Animais.
- **Impactos:** regulatorio medio/alto; seguranca alto; dados pessoais alto; autorizacao alto.
- **Risco de decidir cedo:** aceitar consentimento invalido ou impedir atendimento emergencial necessario.
- **Risco de nao decidir:** bloquear Prontuario/Atendimento clinico com procedimentos sensiveis.
- **Criterio de conclusao:** matriz procedimento x autorizador x evidencia x validade x excecao validada.
- **Gatilho:** SDD de Atendimento clinico, Prontuario, procedimento, cirurgia, anestesia ou termo de consentimento.

### DISC-007 Acesso ao prontuario

- **Estado:** Questão aberta
- **Prioridade:** Alta
- **Contexto:** Prontuario e candidato forte, mas adiado; acesso a informacao clinica tem risco de privacidade.
- **Pergunta principal:** Quem pode visualizar, alterar, corrigir, exportar ou compartilhar registros clinicos?
- **Perguntas secundarias:** profissionais com acesso, tutores/responsaveis, compartilhamento, exportacao, restricoes, retencao, auditoria, informacoes sigilosas, dados de outros responsaveis.
- **Hipotese inicial:** Prontuario exigira regras proprias de autoria, acesso, correcao, retencao e auditoria.
- **Por que nao e regra vigente:** nao ha Prontuario, regra clinica ou decisao de privacidade validada.
- **Evidencia disponivel:** ADR-0007 classifica Prontuario como adiado/candidato forte; ADR-0008 impede derivar acesso de `TutorResponsavelId`.
- **Evidencia necessaria:** politica de acesso, classificacao de dados, retencao, compartilhamento e trilha de auditoria.
- **Fonte esperada:** veterinarios, privacidade, seguranca, juridico, produto.
- **Stakeholders:** veterinarios, suporte, responsaveis pelo animal, privacidade.
- **Modulos/Aggregates potencialmente afetados:** Prontuario, Atendimento, Identidade e Acesso, Auditoria, Privacidade/Compliance.
- **Impactos:** seguranca alto; dados pessoais alto; regulatorio alto; multitenancy alto.
- **Risco de decidir cedo:** vazamento de dado clinico ou autorizacao excessiva por vinculo cadastral.
- **Risco de nao decidir:** impedir modulo de Prontuario ou criar acesso sem guardrails.
- **Criterio de conclusao:** matriz de permissoes, auditoria, isolamento, excecoes e testes definida.
- **Gatilho:** qualquer SDD que crie Prontuario, anexo clinico, evolucao, exportacao ou acesso externo.

### DISC-008 Historico temporal dos vinculos pessoa-animal

- **Estado:** Decisão adiada
- **Prioridade:** Media
- **Contexto:** BR-REL-005 registra somente historico minimo append-only de transferencias.
- **Pergunta principal:** O dominio precisa reconstruir vinculos pessoa-animal por vigencia temporal completa?
- **Perguntas secundarias:** data inicial/final, motivo de encerramento, transferencia, responsavel principal, preservacao historica, efeitos sobre atendimentos/cobrancas, auditoria, consultas historicas, correcoes retroativas.
- **Hipotese inicial:** uma entidade principal de vinculo pode ser necessaria se houver papeis, vigencia ou disputa.
- **Por que foi adiada:** nao ha requisito de historico completo; criar temporal tables, event sourcing ou modelo bitemporal agora seria prematuro.
- **Solucao temporaria:** manter `animais.tutor_responsavel_id` como vinculo vigente e `historico_transferencias_animais` como trilha minima de transferencia.
- **Limites da solucao temporaria:** nao representa periodos completos, multiplos papeis simultaneos, suspensao, disputa ou correcoes retroativas.
- **Evidencia disponivel:** ADR-0004, ADR-0005, ADR-0006 e BR-REL-005.
- **Evidencia necessaria:** necessidade de auditoria funcional, consulta historica, disputa, faturamento retroativo ou prontuario com snapshots formais.
- **Fonte esperada:** operacao, juridico, financeiro, privacidade, produto.
- **Stakeholders:** suporte, juridico, financeiro, veterinarios.
- **Modulos/Aggregates potencialmente afetados:** Tutores/Animais, Atendimento, Cobranca, Prontuario, Auditoria.
- **Impactos:** dados pessoais alto; financeiro medio; regulatorio medio; custo de mudanca alto.
- **Risco de decidir cedo:** introduzir modelo temporal complexo sem consulta real.
- **Risco de nao decidir:** perder rastreabilidade exigida por atendimento, cobranca ou disputa futura.
- **Criterio de conclusao:** casos de consulta historica e retencao definidos, com decisao de modelagem e migracao.
- **Gatilho de retomada:** multiplos responsaveis, disputa, auditoria completa, prontuario, cobranca retroativa ou exigencia regulatoria.

### DISC-009 Ownership de disponibilidade

- **Estado:** Questão aberta
- **Prioridade:** Alta
- **Contexto:** Disponibilidade aparece como hipotese aberta no context map e antes de Agenda no roadmap.
- **Pergunta principal:** Disponibilidade pertence a Workforce, Agenda ou a um contexto proprio?
- **Perguntas secundarias:** quem altera escala; ferias/bloqueios exigem aprovacao; recursos fisicos entram no conflito; disponibilidade e regra de trabalho ou apenas reserva de slots.
- **Hipotese inicial:** se for escala de trabalho, tende a Workforce; se for conflito de reserva, tende a Agenda.
- **Por que nao e regra vigente:** owner e invariantes ainda nao foram validados.
- **Evidencia disponivel:** ADR-0007, context map e roadmap registram a incerteza.
- **Evidencia necessaria:** processo de escala, tipos de bloqueio, unidade, profissional, recurso e concorrencia.
- **Fonte esperada:** operacao, gestores de unidade, produto.
- **Stakeholders:** gestores, recepcao, profissionais, produto.
- **Modulos/Aggregates potencialmente afetados:** Workforce, Agenda, Organizacoes e Unidades.
- **Impactos:** multitenancy alto; seguranca medio; custo de mudanca alto.
- **Risco de decidir cedo:** acoplar escala de pessoas a reserva de agenda ou duplicar conflitos.
- **Risco de nao decidir:** atrasar Agenda ou permitir conflitos de slot.
- **Criterio de conclusao:** owner, comandos, invariantes, unidade/recurso e estrategia de conflito definidos.
- **Gatilho:** SDD de Profissionais, Disponibilidade ou Agenda.

### DISC-010 Profissional, usuario e autoria

- **Estado:** Questão aberta
- **Prioridade:** Media
- **Contexto:** Keycloak autentica usuario, mas profissional executor e autoria clinica sao conceitos de dominio candidatos.
- **Pergunta principal:** Como separar usuario autenticado, permissao tecnica, profissional executor e autoria clinica?
- **Perguntas secundarias:** profissional existe sem usuario; credenciais exigidas; atua em varias unidades; quem assina registro clinico; role tecnica equivale a aptidao de dominio?
- **Hipotese inicial:** Workforce deve modelar profissional; Identidade e Acesso continua suporte tecnico; Prontuario define autoria clinica.
- **Por que nao e regra vigente:** nao existe modulo Workforce nem Prontuario.
- **Evidencia disponivel:** ADR-0007 e revisao estrategica SDD 26.
- **Evidencia necessaria:** processos de cadastro profissional, credenciais, login, assinatura e delegacao.
- **Fonte esperada:** operacao, veterinarios, seguranca, produto.
- **Stakeholders:** profissionais, gestores, seguranca, produto.
- **Modulos/Aggregates potencialmente afetados:** Workforce/`Profissional`, Identidade e Acesso, Atendimento, Prontuario.
- **Impactos:** autorizacao alto; seguranca alto; regulatorio medio; dados pessoais medio.
- **Risco de decidir cedo:** usar role Keycloak como credencial profissional ou autoria clinica indevida.
- **Risco de nao decidir:** bloquear Atendimento/Prontuario e Agenda com aptidao profissional.
- **Criterio de conclusao:** matriz usuario x profissional x permissao x autoria x unidade validada.
- **Gatilho:** SDD de Profissionais, Atendimento, Prontuario ou autorizacao granular.

### DISC-011 Catalogo de servicos, duracao e preco de referencia

- **Estado:** Hipótese
- **Prioridade:** Media
- **Contexto:** Catalogo de Servicos e primeiro SDD recomendado no roadmap, mas preco e requisitos ainda possuem ownership incerto.
- **Pergunta principal:** O Catalogo deve conter apenas definicao operacional e duracao, ou tambem preco de referencia e requisitos?
- **Perguntas secundarias:** quem cria servico; duracao e obrigatoria; preco e referencia ou regra financeira; servico exige profissional, recurso ou unidade.
- **Hipotese inicial:** duracao padrao pertence ao Catalogo; valor efetivamente cobrado pertence a Cobranca.
- **Por que nao e regra vigente:** nao ha modulo, contrato ou validacao de produto para servicos.
- **Evidencia disponivel:** ADR-0007, aggregates candidatos e roadmap.
- **Evidencia necessaria:** lista de servicos, variacao por tenant/unidade, precificacao e uso em Agenda/Cobranca.
- **Fonte esperada:** produto, operacao, financeiro.
- **Stakeholders:** gestores, recepcao, financeiro, produto.
- **Modulos/Aggregates potencialmente afetados:** Catalogo/`Servico`, Agenda, Cobranca, Atendimento.
- **Impactos:** financeiro medio; multitenancy alto; custo de mudanca medio.
- **Risco de decidir cedo:** colocar regra financeira no Catalogo ou deixar Agenda sem duracao confiavel.
- **Risco de nao decidir:** atrasar Agenda e Cobranca.
- **Criterio de conclusao:** campos minimos, ownership de preco, regras de inativacao e contratos consumidores definidos.
- **Gatilho:** SDD de Catalogo de Servicos.

### DISC-012 Notifications, preferencias e consentimento de comunicacao

- **Estado:** Hipótese
- **Prioridade:** Baixa
- **Contexto:** Notifications aparece como capacidade que exige descoberta; ainda nao ha envio automatico.
- **Pergunta principal:** Notifications sera apenas adapter tecnico de mensagens operacionais ou bounded context com preferencias, templates e consentimento?
- **Perguntas secundarias:** canais, opt-in/opt-out, finalidade, template por tenant, historico de entrega, marketing, minimizacao de payload e tenant em mensagens.
- **Hipotese inicial:** envio operacional simples pode iniciar como adapter; preferencias e consentimentos podem exigir dominio proprio.
- **Por que nao e regra vigente:** nao ha consumidor real nem regra de comunicacao.
- **Evidencia disponivel:** ADR-0002 define propagacao futura; ADR-0007 classifica Notifications como discovery.
- **Evidencia necessaria:** canais, finalidade, consentimento/preferencia, templates, retencao e auditoria de entrega.
- **Fonte esperada:** produto, privacidade, operacao.
- **Stakeholders:** recepcao, marketing/produto, privacidade, suporte.
- **Modulos/Aggregates potencialmente afetados:** Notifications, Agenda, Atendimento, Cobranca, Privacidade/Compliance.
- **Impactos:** dados pessoais medio/alto; regulatorio medio; multitenancy alto.
- **Risco de decidir cedo:** enviar PII demais ou criar opt-in/marketing sem regra.
- **Risco de nao decidir:** adiar notificacoes sem afetar MVP operacional inicial.
- **Criterio de conclusao:** finalidade por canal, preferencias, payload minimo e eventos consumidores definidos.
- **Gatilho:** SDD de notificacao automatica, lembrete de agenda, cobranca por mensagem ou marketing.

### DISC-013 Modelo generico de Pessoa/Party para antecipar papeis

- **Estado:** Descartada
- **Prioridade:** Baixa
- **Contexto:** ADRs anteriores consideraram e rejeitaram `Pessoa`/`Party` generica sem regras compartilhadas confirmadas.
- **Pergunta principal:** Deve existir um modelo generico de Pessoa para tutor, profissional, pagador, representante, abrigo e organizacao antes de fluxos concretos?
- **Motivo da rejeicao:** criaria acoplamento e Shared Kernel de dominio sem evidencias de linguagem comum, invariantes ou ciclo de vida compartilhado.
- **Evidencia:** ADR-0003, ADR-0004, ADR-0006, ADR-0008 e documentos de Tutores e Animais.
- **Impactos:** preserva simplicidade e evita migration/contratos especulativos.
- **Gatilho de reavaliacao:** surgirem regras comuns reais entre pessoas fisicas, profissionais, organizacoes, representantes e pagadores que justifiquem ownership compartilhado ou ACL explicita.

## Contradicoes e afirmacoes prematuras

Busca textual nao encontrou as frases absolutas exemplificadas no SDD, como "todo animal possui um unico tutor", "o tutor e responsavel pelo pagamento", "o tutor pode acessar o prontuario" ou "o solicitante do agendamento e o pagador".

Achados:

| Achado | Tipo | Tratamento |
| --- | --- | --- |
| `docs/domain/aggregates.md` usava "Invariantes conhecidas" tambem para aggregates candidatos. | Risco de leitura prematura | Mantido o conteudo, mas reclassificado como "Invariantes candidatas, nao vigentes" nos candidatos. |
| `docs/domain/matriz-responsabilidades.md` listava invariantes de capacidades candidatas sem nota de estado. | Risco de leitura prematura | Adicionada nota de que, para capacidades nao confirmadas, sao hipoteses de consistencia. |
| `docs/domain/regras-de-negocio/lacunas-hipoteses-politicas.md` agregava varios temas em `HYP-REL-006`. | Granularidade insuficiente para discovery | Backlog `DISC-*` separa consentimento, prontuario, LGPD, financeiro e vinculos. |

Nao foram encontradas contradicoes executaveis no codigo durante este SDD documental.

## Bloqueios de MVP

Bloqueiam ou condicionam proximas fatias do MVP:

- DISC-001 para Agenda;
- DISC-005 para Cobranca basica;
- DISC-006 para procedimentos que exijam consentimento;
- DISC-007 para Prontuario;
- DISC-009 para Disponibilidade antes de Agenda;
- DISC-010 para autoria clinica e aptidao profissional.

Nao bloqueiam a Entrega 1 ja implementada:

- DISC-003;
- DISC-004 enquanto houver apenas um tutor responsavel operacional vigente;
- DISC-008 enquanto a trilha minima de transferencia for suficiente;
- DISC-011 se o proximo SDD limitar Catalogo a definicao operacional validada;
- DISC-012;
- DISC-013 permanece descartada.

## Temas fora do MVP atual

Continuam fora do MVP atual sem nova evidencia:

- parecer juridico LGPD completo;
- prontuario veterinario;
- consentimento clinico;
- pessoa juridica/abrigo/ONG/protetor institucional;
- multiplos responsaveis simultaneos;
- responsavel financeiro, pagador e destinatario da cobranca;
- historico temporal completo, temporal tables, event sourcing ou modelo bitemporal;
- Notifications com marketing/preferencias;
- modelo generico de Pessoa/Party.

## Orientacao para futuros SDDs

Um futuro SDD nao deve implementar uma hipotese como regra vigente sem:

- referenciar o item `DISC-*`;
- apresentar a evidencia obtida;
- registrar a decisao;
- atualizar o catalogo de regras e a matriz;
- atualizar o glossario quando a terminologia mudar;
- avaliar impactos em multitenancy, autorizacao, dados pessoais, regulatorio e financeiro;
- criar ou atualizar ADR quando houver decisao arquitetural ou de dominio relevante;
- adicionar testes que expressem a regra validada.

Quando um prompt futuro depender de uma questao aberta, ele deve interromper somente a parte dependente, preservar o restante do escopo possivel, registrar a limitacao e nao preencher a lacuna com suposicao silenciosa.
