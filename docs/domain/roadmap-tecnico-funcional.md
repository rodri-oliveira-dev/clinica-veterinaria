# Roadmap tecnico-funcional

- **Data:** 2026-07-21
- **Escopo:** proximos SDDs apos o checkpoint SDD 26

## Principios

- Entregar fatias verticais demonstraveis.
- Evitar modulos vazios e infraestrutura sem consumidor.
- Preservar tenant da claim validada.
- Criar contracts internos somente quando houver consumidor real.
- Reavaliar fronteiras quando uma regra exigir consistencia em mais de uma capacidade.

## Sequencia recomendada

| Ordem | SDD proposto | Objetivo | Dependencias | Risco | Resultado demonstravel |
| --- | --- | --- | --- | --- | --- |
| 1 | Catalogo de servicos simples | Criar definicao tenant-owned de servico ativo/inativo com duracao padrao e consulta | Nenhuma nova capacidade; usa fundacao multitenant | Baixo/medio: evitar preco e requisitos excessivos | Tenant cadastra e consulta servicos ativos |
| 2 | Profissionais simples | Cadastrar profissional tenant-owned e status operacional | Catalogo pode existir, mas nao precisa acoplamento forte | Medio: nao confundir usuario Keycloak com profissional | Tenant cadastra profissional ativo/inativo |
| 3 | Disponibilidade basica | Registrar disponibilidade ou bloqueio minimo para profissional/unidade | Profissionais | Alto: owner ainda incerto entre Workforce e Agenda | Consulta de disponibilidade base por periodo |
| 4 | Agendamento | Criar/remarcar/cancelar agendamento usando animal, servico e profissional aptos | Catalogo, Profissionais, decisao de disponibilidade | Alto: concorrencia e conflito de slots | Agendamento criado sem conflito no tenant |
| 5 | Consulta de agenda | Expor visao operacional paginada/filtrada da agenda | Agendamento | Medio: projection versus consulta direta | Tela/API consulta agenda por periodo, unidade/profissional |
| 6 | Check-in | Confirmar chegada/transicao inicial do agendamento | Agendamento | Medio: fronteira com Atendimento | Agendamento muda para estado de chegada/confirmacao |
| 7 | Atendimento operacional | Registrar execucao basica do servico e conclusao operacional | Check-in/Agenda, Profissionais, Catalogo | Alto: evitar prontuario e cobranca dentro do aggregate | Atendimento iniciado e concluido com servico realizado |
| 8 | Cobranca basica | Gerar cobranca a partir de servico contratado/realizado com responsavel financeiro explicito | Atendimento operacional; discovery financeiro | Alto: valores, descontos e pagador | Cobranca criada e marcada como paga/cancelada em fluxo simples |

## Ajustes na sequencia original

A sequencia sugerida pelo SDD foi mantida em essencia. O motivo e que Agenda precisa aprender com Catalogo e Profissionais antes de reservar horario, e Cobranca precisa distinguir preco padrao, servico contratado e servico realizado.

Disponibilidade fica antes de Agendamento, mas com criterio de reavaliacao forte: se a disponibilidade inicial for apenas conflito de reservas, ela pode nascer dentro de Agenda. Se envolver escala, ferias e bloqueios mantidos por gestores, deve ser modelada com Workforce ou como capacidade separada.

## Descobertas obrigatorias antes de cada SDD

Cada descoberta abaixo deve referenciar um item `DISC-*` quando houver dependencia semantica. Ausencia de resposta nao autoriza transformar uma hipotese em regra; o SDD deve reduzir escopo ou registrar explicitamente o bloqueio.

### Catalogo de servicos

- Quem cria e altera servicos no tenant?
- Duracao padrao e obrigatoria?
- Preco padrao e apenas referencia ou regra financeira?
- Servico pode exigir profissional, recurso ou unidade?
- Item de discovery: DISC-011.

### Profissionais

- Profissional pode existir sem usuario?
- Quais credenciais sao exigidas?
- Profissional atua em uma ou muitas unidades?
- Quais papeis sao permissao tecnica e quais sao dominio?
- Item de discovery: DISC-010.

### Disponibilidade

- Quem altera disponibilidade?
- A escala e do profissional, da unidade ou da agenda?
- Bloqueios e ferias precisam aprovacao?
- Recursos fisicos entram no mesmo conflito?
- Item de discovery: DISC-009.

### Agenda

- Qual unidade minima de reserva?
- Quais estados existem antes do atendimento?
- Qual conflito gera `409`?
- Remarcacao exige versao?
- Quem pode solicitar, confirmar, cancelar, remarcar, levar ou buscar o animal?
- Item de discovery: DISC-001.

### Atendimento

- Atendimento nasce no check-in ou ao iniciar execucao?
- O que e servico realizado?
- Quem registra responsavel presente?
- O que deve virar prontuario e o que e apenas operacional?
- Consentimento ou autorizacao clinica sao necessarios?
- Item de discovery: DISC-006.

### Cobranca

- Quando uma cobranca nasce?
- Quem e responsavel financeiro?
- Orcamento e cobranca sao o mesmo aggregate?
- O valor vem do catalogo, do contrato, do atendimento ou de ajuste manual?
- Quem e pagador e quem recebe a cobranca?
- Item de discovery: DISC-005.

## Prontidao por discovery

| Capacidade | Prontidao | Bloqueio de discovery | Criterio de prontidao |
| --- | --- | --- | --- |
| Cadastro de Tutores e Animais | Pronta para manutencao incremental | Nenhum para Entrega 1 | Manter BR-* vigentes e ADRs atuais. |
| Catalogo de Servicos | Parcialmente compreendida | DISC-011 | Campos minimos e ownership de preco/duracao definidos. |
| Profissionais / Workforce | Parcialmente compreendida | DISC-010 | Separacao entre usuario, profissional, credencial e autoria. |
| Disponibilidade | Bloqueada por discovery | DISC-009 | Owner e invariantes de escala/bloqueio/slot decididos. |
| Agenda | Bloqueada por discovery | DISC-001, DISC-009 | Atores, estados, cancelamento, conflitos e contratos upstream definidos. |
| Atendimento | Parcialmente compreendida | DISC-006, DISC-010 | Fronteira com Agenda/Prontuario e consentimento quando aplicavel. |
| Prontuario | Fora do MVP atual | DISC-002, DISC-006, DISC-007 | Acesso, autoria, retencao, correcao e auditoria validados. |
| Cobranca | Bloqueada por discovery | DISC-005, DISC-011 | Papeis financeiros, origem de valor e autorizacao definidos. |
| Notifications | Fora do MVP atual | DISC-012 | Finalidade, canal, preferencias, payload minimo e retencao definidos. |

## Gates tecnicos esperados

- Novo modulo persistente deve ter `tenant_id NOT NULL`.
- Novo relacionamento tenant-owned deve impedir associacao cross-tenant.
- Novo fluxo persistente deve ter teste com dois tenants.
- Novo limite importante deve ter teste arquitetural proporcional.
- Novo contrato HTTP nao deve aceitar `tenant_id` como autoridade.
- Nova decisao de fronteira ou consistencia deve virar ADR.
