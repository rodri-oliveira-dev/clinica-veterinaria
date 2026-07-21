# Glossario de Tutores, Animais e relacionamentos

Status segue a taxonomia do backlog de discovery: `Vigente`, `Hipótese`, `Questão aberta`, `Decisão adiada` e `Descartada`. Termos aceitos ou confirmados antes do SDD 28 foram mapeados para `Vigente`; termos ambiguos permanecem explicitamente marcados para nao virarem contratos por acidente.

| Termo | Status | Uso no catalogo |
| --- | --- | --- |
| Tutor | Vigente | Pessoa cadastrada pela clinica para relacionamento operacional sobre animais dentro do tenant. Nao implica propriedade legal, responsabilidade financeira ou consentimento clinico amplo. |
| Responsavel | Questão aberta | Usar somente com qualificacao. O termo isolado nao deve virar contrato ou regra. Relacionado a DISC-004. |
| Tutor responsavel | Vigente | Tutor ativo, existente e visivel no tenant atual que representa a responsabilidade operacional vigente pelo animal. Nao concede autorizacao clinica, prontuario, cobranca, representacao legal ou direitos de dados. |
| Responsavel operacional | Hipótese | Definicao candidata para contrato futuro mais explicito que `TutorResponsavelId`; nao renomear contrato atual sem consumidor real. Relacionado a DISC-001 e DISC-004. |
| Responsavel principal | Questão aberta | Nao existe como dado separado. Na fatia atual equivale ao tutor responsavel vigente apenas porque nao ha multiplos responsaveis. Relacionado a DISC-004. |
| Animal | Vigente | Paciente animal mantido no cadastro operacional do tenant. |
| Vinculo | Vigente | Relacao operacional vigente entre animal e tutor responsavel. Hoje e referencia por identidade no aggregate `Animal`, nao entidade propria. |
| Transferencia | Vigente | Operacao explicita que altera o tutor responsavel vigente e registra historico minimo. |
| Inativacao | Vigente | Retirada de uso comum sem hard delete. Aplica-se a tutor e animal com regras diferentes. |
| Falecimento | Vigente | Transicao explicita de animal para `Falecido`, com data obrigatoria e bloqueio de fluxos comuns incompativeis. |
| Proprietario | Descartada | Nao usar como sinonimo automatico de Tutor. |
| Proprietario declarado | Hipótese | Pode virar conceito futuro, mas nao existe na Entrega 1. |
| Cliente | Questão aberta / evitado | Pode significar tenant, tutor, pagador ou consumidor de API. Nao usar como termo de dominio de Tutores e Animais. |
| Paciente | Hipótese | Pode ser usado em contexto clinico futuro; na Entrega 1 o termo de cadastro e `Animal`. Relacionado a DISC-007. |
| Pet | Descartada | Usar `Animal` quando o conceito for o paciente/cadastro animal no dominio. |
| Solicitante | Questão aberta | Pessoa ou papel que pede agendamento ou atendimento. Nao inferir de `TutorResponsavelId`. Relacionado a DISC-001. |
| Acompanhante | Questão aberta | Pessoa que leva, acompanha ou busca o animal em atendimento. Nao inferir de `TutorResponsavelId`. Relacionado a DISC-001 e DISC-006. |
| Contato operacional | Hipótese | Pessoa/canal usado para comunicacao operacional. Hoje ha contato minimo do tutor, mas nao multiplos contatos por animal. Relacionado a DISC-001 e DISC-012. |
| Responsavel financeiro | Questão aberta | Reservado para Cobranca/Faturamento; nao inferir a partir de `TutorResponsavelId`. Relacionado a DISC-005. |
| Pagador | Questão aberta | Reservado para Cobranca/Faturamento; nao inferir a partir de `TutorResponsavelId`. Relacionado a DISC-005. |
| Destinatario da cobranca | Questão aberta | Pessoa ou organizacao que recebe documento/cobranca; pode ser diferente de pagador e tutor. Relacionado a DISC-005. |
| Autorizador clinico | Questão aberta | Reservado para discovery de Atendimento/Prontuario; nao inferir a partir de `TutorResponsavelId`. Relacionado a DISC-006. |
| Consentimento | Questão aberta | Registro de concordancia para finalidade ou procedimento especifico; definicao clinica/juridica pendente. Relacionado a DISC-006 e DISC-012. |
| Autorizacao | Questão aberta | Permissao para ato operacional, clinico ou acesso; deve ser qualificada por contexto. Relacionado a DISC-001, DISC-006 e DISC-007. |
| Representante legal | Questão aberta | Reservado para discovery proprio; nao inferir a partir de `TutorResponsavelId`. Relacionado a DISC-003, DISC-004 e DISC-006. |
| Titular | Questão aberta | Pessoa natural a quem se referem dados pessoais; fluxos LGPD pendem de validacao. Relacionado a DISC-002. |
| Direitos do titular | Questão aberta | Reservado para fluxos de Privacidade/Compliance; nao inferir a partir de vinculo cadastral com animal. Relacionado a DISC-002. |
| Pessoa | Descartada | Nao criar entidade generica sem regras compartilhadas confirmadas. Relacionado a DISC-013. |
| Organizacao | Questão aberta | Pessoa juridica, abrigo, ONG, protetor institucional ou unidade organizacional ainda nao modelada. Relacionado a DISC-003. |
| Abrigo | Questão aberta | Possivel organizacao responsavel por muitos animais; sem regra vigente. Relacionado a DISC-003. |
| ONG | Questão aberta | Possivel organizacao responsavel por muitos animais; sem regra vigente. Relacionado a DISC-003. |
| Prontuario | Questão aberta | Registro clinico futuro com acesso, autoria, correcao e retencao proprios; nao inferir acesso de vinculo. Relacionado a DISC-007. |
| Microchip | Hipótese | Identificador externo futuro, sem regra atual de unicidade. |
| Idade estimada | Hipótese | Nao persistida; data exata opcional e a regra vigente. |
