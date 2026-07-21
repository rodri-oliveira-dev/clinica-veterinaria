# Matriz de responsabilidades

- **Data:** 2026-07-21
- **Escopo:** SDD 26

Para capacidades com status diferente de `Confirmado`, a coluna `Invariantes` registra hipoteses de consistencia para discovery, nao regras vigentes. Antes de implementar schema, contrato, validacao ou autorizacao, relacione a capacidade ao backlog `docs/domain/discovery-backlog.md`.

| Capacidade | Dados proprios | Operacoes | Invariantes | Consumidores | Status |
| --- | --- | --- | --- | --- | --- |
| Tutor | `tutores`, contatos, situacao, CPF mascarado em respostas | cadastrar, consultar, pesquisar, atualizar, inativar | tenant obrigatorio; CPF unico por tenant quando informado; ao menos um contato; tutor com animal ativo vinculado nao pode ser inativado | API atual; Agenda/Atendimento/Cobranca futuros por contrato | Confirmado |
| Animal | `animais`, dados cadastrais, situacao, tutor responsavel vigente, versao | cadastrar, consultar, pesquisar, atualizar, inativar, registrar falecimento | tenant obrigatorio; nome/especie obrigatorios; falecido exige data; falecido bloqueia fluxos comuns; tutor responsavel do mesmo tenant | API atual; Agenda/Atendimento/Prontuario futuros por contrato | Confirmado |
| Vinculo Tutor-Animal | `animais.tutor_responsavel_id`; `historico_transferencias_animais` como trilha minima | vincular no cadastro; transferir responsabilidade | exatamente um tutor responsavel vigente; tutor responsavel ativo no cadastro/transferencia; FKs compostas com tenant; transferencia exige versao; nao concede consentimento clinico, prontuario, cobranca, pagador, representacao legal ou direitos de dados | API atual; futuros modulos por contrato especifico | Confirmado |
| Servico | Definicao operacional do servico, status, duracao padrao, requisitos simples | cadastrar/editar/inativar servico; consultar definicao | nome/codigo por tenant; servico ativo para novos agendamentos; duracao padrao valida | Agenda, Atendimento, Cobranca | Candidato |
| Profissional | cadastro profissional, credenciais de dominio, unidades de atuacao, aptidoes | cadastrar, habilitar/desabilitar, associar a unidade/servico | profissional pertence ao tenant; credencial valida quando exigida; nao e usuario Keycloak por definicao | Agenda, Atendimento, Prontuario | Candidato |
| Disponibilidade | escalas, bloqueios, ferias, disponibilidade por unidade/profissional/recurso | abrir/fechar horarios, bloquear periodos, consultar slots base | intervalos validos; sem sobreposicao indevida conforme owner; tenant e unidade respeitados | Agenda | Exige descoberta |
| Agendamento | reservas, horario, estado, animal, servico, profissional/recurso quando aplicavel | criar, remarcar, cancelar, confirmar, registrar no-show, talvez check-in | sem conflito de horario/recurso; animal apto; servico ativo; tenant consistente; concorrencia explicita | API de Agenda, Atendimento, Notifications, Cobranca futura | Candidato |
| Atendimento | execucao operacional, check-in se separado, servico realizado, responsavel presente | iniciar, registrar execucao, concluir, cancelar operacao | nasce de agendamento ou demanda avulsa; nao contem prontuario inteiro; nao decide regras financeiras | Prontuario, Cobranca, Notifications | Candidato |
| Prontuario | registros clinicos, evolucao, documentos, consentimentos, autoria | registrar, assinar, corrigir, anexar, finalizar | autoria clinica obrigatoria; correcao auditada; retencao e privacidade fortes | Atendimento, relatorios clinicos futuros | Adiado |
| Cobranca | orcamentos, itens cobrados, valores, descontos, responsavel financeiro, pagamentos | gerar cobranca, ajustar, receber, cancelar, emitir recibo | valores owned por Cobranca; pagador nao e inferido de tutor operacional; consistencia com itens realizados conforme regra | API de Cobranca, Notifications, relatorios | Candidato |
| Notificacao | templates, preferencia/consentimento, mensagem, tentativa de entrega, canal | enviar, reagendar, registrar falha, respeitar preferencia | nao transportar PII desnecessaria; consentimento quando aplicavel; tenant em mensagens | Agenda, Atendimento, Cobranca, Marketing futuro | Exige descoberta |
| Identidade e acesso | usuarios/autenticacao no IdP, roles, claims, tenant_id | autenticar, autorizar, emitir claims | tenant vem da claim validada; usuario autenticado nao e profissional automaticamente | Todas as APIs | Confirmado como suporte tecnico |
| Auditoria | eventos auditaveis, ator, alvo, tempo, tenant, correlacao | registrar alteracoes criticas, suporte, exportacoes | minimizacao; tenant e correlation; nao confundir log tecnico com auditoria funcional | Compliance, suporte, seguranca | Adiado |
| Assinaturas SaaS | plano, assinatura, tenant contratante, limites comerciais da plataforma | contratar, renovar, suspender, cobrar assinatura | separado da cobranca operacional do petshop; possiveis operacoes cross-tenant explicitas | Administracao SaaS | Adiado |

## Ownership multitenant

Por padrao, todas as capacidades operacionais sao tenant-owned. Dados por unidade continuam dentro de um tenant. Dados globais ficam adiados ate haver justificativa explicita.

| Capacidade | Tenant owner | Dados por unidade | Dados globais | Risco cross-tenant | Estrategia de teste |
| --- | --- | --- | --- | --- | --- |
| Tutor | `tenant_id` em `tutores` | Ainda nao existe unidade | Nenhum | Expor dados pessoais ou associar animal de outro tenant | Testes API e persistencia com dois tenants |
| Animal | `tenant_id` em `animais` | Ainda nao existe unidade | Nenhum | Associar tutor/animal entre tenants | FK composta e testes com dois tenants |
| Servico | Tenant do catalogo | Servico pode variar por unidade | Catalogo global nao aprovado | Reutilizar catalogo global indevido | Testes de unicidade e consulta por tenant |
| Profissional | Tenant empregador/contratante | Atuacao por unidade provavel | Credenciais externas podem ser referencia, nao ownership | Profissional associado a unidade de outro tenant | Testes de associacao por tenant e unidade |
| Disponibilidade | Tenant operacional | Forte relacao com unidade | Nenhum previsto | Bloqueio/escala aplicada ao tenant errado | Testes de intervalos com dois tenants |
| Agendamento | Tenant que agenda | Forte relacao com unidade | Nenhum | Agendar animal/profissional/recurso cross-tenant | Testes de conflito e IDs externos ao tenant |
| Atendimento | Tenant do atendimento | Unidade de execucao | Nenhum | Iniciar atendimento com agendamento/animal de outro tenant | Testes de transicao por tenant |
| Prontuario | Tenant clinico | Unidade pode ser contexto | Nenhum | Vazamento de dado clinico | Testes de isolamento e autorizacao |
| Cobranca | Tenant que cobra | Unidade/caixa pode existir | Nenhum | Cobrar pessoa/atendimento de outro tenant | Testes de itens e pagador por tenant |
| Notificacao | Tenant dono da mensagem | Unidade pode influenciar template | Templates globais so como produto SaaS, se aprovado | Enviar mensagem a contato de outro tenant | Testes de payload e destinatario por tenant |
