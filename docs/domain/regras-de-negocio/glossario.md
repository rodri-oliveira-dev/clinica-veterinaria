# Glossario de Tutores, Animais e relacionamentos

| Termo | Status | Uso no catalogo |
| --- | --- | --- |
| Tutor | Aceito / fato confirmado | Pessoa cadastrada pela clinica para relacionamento operacional sobre animais dentro do tenant. Nao implica propriedade legal, responsabilidade financeira ou consentimento clinico amplo. |
| Responsavel | Ambiguo | Usar somente com qualificacao. O termo isolado nao deve virar contrato ou regra. |
| Tutor responsavel | Aceito / decisao vigente | Tutor ativo, existente e visivel no tenant atual que representa a responsabilidade operacional vigente pelo animal. |
| Responsavel principal | Reservado | Nao existe como dado separado. Na fatia atual equivale ao tutor responsavel vigente apenas porque nao ha multiplos responsaveis. |
| Animal | Aceito / fato confirmado | Paciente animal mantido no cadastro operacional do tenant. |
| Vinculo | Aceito | Relacao operacional vigente entre animal e tutor responsavel. Hoje e referencia por identidade no aggregate `Animal`, nao entidade propria. |
| Transferencia | Aceito | Operacao explicita que altera o tutor responsavel vigente e registra historico minimo. |
| Inativacao | Aceito | Retirada de uso comum sem hard delete. Aplica-se a tutor e animal com regras diferentes. |
| Falecimento | Aceito | Transicao explicita de animal para `Falecido`, com data obrigatoria e bloqueio de fluxos comuns incompativeis. |
| Proprietario | Proibido como sinonimo de Tutor | Nao usar para representar automaticamente tutor. |
| Proprietario declarado | Hipotese | Pode virar conceito futuro, mas nao existe na Entrega 1. |
| Cliente | Ambiguo / evitado | Pode significar tenant, tutor, pagador ou consumidor de API. Nao usar como termo de dominio de Tutores e Animais. |
| Paciente | Dependente de descoberta futura | Pode ser usado em contexto clinico futuro; na Entrega 1 o termo de cadastro e `Animal`. |
| Pet | Evitado | Usar `Animal` quando o conceito for o paciente/cadastro animal no dominio. |
| Responsavel financeiro | Hipotese | Reservado para Billing; nao inferir a partir de `TutorResponsavelId`. |
| Pessoa | Reservado | Nao criar entidade generica sem regras compartilhadas confirmadas. |
| Microchip | Hipotese | Identificador externo futuro, sem regra atual de unicidade. |
| Idade estimada | Hipotese | Nao persistida; data exata opcional e a regra vigente. |
