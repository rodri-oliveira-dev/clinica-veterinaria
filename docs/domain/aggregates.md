# Catalogo de Aggregates

- **Data:** 2026-07-21
- **Escopo:** SDD 26

## Aggregates existentes

### Tutor

| Item | Descricao |
| --- | --- |
| Bounded context | Cadastro de Tutores e Animais |
| Modulo | `PetShop.Tutores` |
| Aggregate Root | `Tutor` |
| Entidades internas | Nenhuma |
| Value Objects | `TutorId`, `TenantId`, `NomeDoTutor`, `Cpf`, `Email`, `Telefone`, `SituacaoDoTutor` |
| Invariantes | tenant e id obrigatorios; nome obrigatorio; CPF valido quando informado; ao menos um contato; situacao nasce ativa; inativacao nao apaga cadastro; tutor com animal ativo vinculado nao pode ser inativado pela Application |
| Operacoes | `Cadastrar`, `AlterarCadastro`, `Inativar` |
| Consistencia | Local ao aggregate para cadastro/alteracao; consulta ao repository interno para bloquear inativacao com animal ativo |
| Tamanho esperado | Pequeno; cadastro operacional e contatos minimos |
| Referencias externas | Nenhuma no Domain; Application recebe tenant resolvido da borda |
| Eventos relevantes | Nenhum evento implementado; fatos candidatos futuros: tutor cadastrado, tutor inativado |
| Nao deve conter | animais, prontuario, cobrancas, permissoes Keycloak, responsavel financeiro, propriedade legal, consentimentos clinicos |

### Animal

| Item | Descricao |
| --- | --- |
| Bounded context | Cadastro de Tutores e Animais |
| Modulo | `PetShop.Tutores` |
| Aggregate Root | `Animal` |
| Entidades internas | Nenhuma |
| Value Objects | `AnimalId`, `TenantId`, `NomeDoAnimal`, `Especie`, `Raca`, `SexoDoAnimal`, `DataDeNascimento`, `DataDoFalecimento`, `CorOuPelagem`, `ObservacaoCadastral`, `SituacaoDoAnimal`, `TutorResponsavel`, `TutorId` |
| Invariantes | tenant e id obrigatorios; tutor responsavel obrigatorio; nome e especie obrigatorios; data de nascimento nao futura; falecimento exige data nao futura; falecido bloqueia atualizacao, inativacao e transferencia; transferencia exige animal ativo e novo tutor diferente |
| Operacoes | `Cadastrar`, `AlterarCadastro`, `Inativar`, `RegistrarFalecimento`, `TransferirResponsabilidade` |
| Consistencia | Local para estado do animal e versao; Application valida existencia/situacao do tutor no tenant; FK composta protege associacao cross-tenant |
| Tamanho esperado | Medio, limitado a cadastro operacional e ciclo de vida basico |
| Referencias externas | Tutor por identidade (`TutorResponsavelId`), sem navegacao de dominio |
| Eventos relevantes | Nenhum evento implementado; fatos candidatos futuros: animal cadastrado, responsabilidade transferida, falecimento registrado |
| Nao deve conter | prontuario, diagnosticos, vacinas, documentos clinicos, atendimento, cobranca, recursos de agenda, microchip sem requisito, catalogos de especie/raca sem owner |

## Registro historico existente

### TransferenciaDeResponsabilidadeDoAnimal

| Item | Descricao |
| --- | --- |
| Bounded context | Cadastro de Tutores e Animais |
| Modulo | `PetShop.Tutores` |
| Aggregate Root | Nao e Aggregate Root nesta etapa |
| Papel | Registro append-only de trilha minima da transferencia |
| Entidades internas | Nenhuma |
| Value Objects | `TenantId`, `AnimalId`, `TutorId` |
| Invariantes | id, tenant, animal, tutor anterior, tutor novo e subject obrigatorios; tutores diferentes; motivo opcional normalizado; update/delete bloqueados pela guarda de persistencia |
| Operacoes | `Registrar` |
| Consistencia | Gravado na mesma transacao local da transferencia do `Animal` |
| Tamanho esperado | Pequeno e append-only |
| Referencias externas | Animal e tutores por identidade, com FKs compostas no mesmo modulo |
| Eventos relevantes | O proprio registro representa fato historico interno; nao e integration event |
| Nao deve conter | CPF, email, telefone, token, claims completas, documento clinico, consentimento, pagamento |

## Aggregates candidatos

### Servico

| Item | Descricao |
| --- | --- |
| Bounded context | Catalogo de Servicos candidato |
| Aggregate Root | `Servico` |
| Entidades internas | Evitar no MVP; categorias e requisitos so se tiverem regra propria |
| Value Objects candidatos | `ServicoId`, `TenantId`, `NomeDoServico`, `CodigoDoServico`, `DuracaoPadrao`, `SituacaoDoServico`, talvez `PrecoPadrao` como referencia |
| Invariantes conhecidas | tenant obrigatorio; nome/codigo valido; duracao positiva; servico inativo nao deve ser usado em novos agendamentos comuns |
| Operacoes candidatas | cadastrar, atualizar definicao, ativar/inativar |
| Consistencia | Local ao Catalogo; Agenda consulta definicao ativa por contrato |
| Tamanho esperado | Pequeno; definicao operacional, nao execucao inteira |
| Referencias externas | Profissional/recurso por requisitos leves somente se confirmado |
| Eventos relevantes | Servico cadastrado/alterado/inativado como candidatos, sem implementar agora |
| Nao deve conter | agenda real, disponibilidade, atendimento realizado, regras de pagamento, desconto, impostos, pacote, prontuario |

### Profissional

| Item | Descricao |
| --- | --- |
| Bounded context | Workforce candidato |
| Aggregate Root | `Profissional` |
| Entidades internas | Credenciais, unidades de atuacao ou aptidoes podem virar entidades internas se tiverem ciclo proprio |
| Value Objects candidatos | `ProfissionalId`, `TenantId`, `NomeDoProfissional`, `RegistroProfissional`, `SituacaoDoProfissional` |
| Invariantes conhecidas | tenant obrigatorio; profissional ativo para execucao/agendamento; credencial obrigatoria para funcoes reguladas, se confirmado |
| Operacoes candidatas | cadastrar, habilitar, inativar, associar unidade, registrar aptidao |
| Consistencia | Local para cadastro; Agenda/Atendimento validam aptidao por contrato |
| Tamanho esperado | Medio; nao deve virar usuario/permissao generico |
| Referencias externas | Identidade autenticada opcional, por id de usuario, se houver login |
| Eventos relevantes | Profissional habilitado/inativado como candidatos |
| Nao deve conter | senha, token, roles Keycloak como dominio, agenda completa, prontuario, pagamento |

### Disponibilidade

| Item | Descricao |
| --- | --- |
| Bounded context | Hipotese: Workforce, Agenda ou contexto proprio |
| Aggregate Root | Em aberto: `DisponibilidadeDoProfissional`, `Escala`, `BloqueioDeAgenda` ou similar |
| Entidades internas | Intervalos, excecoes, ferias, bloqueios |
| Value Objects candidatos | `IntervaloDeTempo`, `DiaDaSemana`, `Periodo`, `UnidadeId` |
| Invariantes conhecidas | intervalos validos; sem sobreposicao proibida; tenant e unidade consistentes |
| Operacoes candidatas | definir escala, bloquear periodo, liberar periodo, consultar disponibilidade base |
| Consistencia | Depende se a regra e de escala (Workforce) ou reserva (Agenda) |
| Tamanho esperado | Ainda incerto |
| Referencias externas | Profissional, unidade, recursos |
| Eventos relevantes | Bloqueio criado, escala alterada como candidatos |
| Nao deve conter | agendamento confirmado inteiro, atendimento, cobranca |

### Agendamento

| Item | Descricao |
| --- | --- |
| Bounded context | Agenda candidato |
| Aggregate Root | `Agendamento` |
| Entidades internas | Talvez itens/servicos agendados, recursos reservados, historico de remarcacao |
| Value Objects candidatos | `AgendamentoId`, `TenantId`, `JanelaDeHorario`, `SituacaoDoAgendamento`, `Origem`, `Versao` |
| Invariantes conhecidas | sem conflito de horario para profissional/recurso quando aplicavel; animal apto; servico ativo; tenant consistente; remarcacao com concorrencia |
| Operacoes candidatas | agendar, remarcar, cancelar, confirmar, no-show, check-in se permanecer em Agenda |
| Consistencia | Forte dentro da agenda para conflito de reserva |
| Tamanho esperado | Medio; nao representar toda jornada clinica |
| Referencias externas | Animal, tutor, servico, profissional e recurso por identidade/snapshot minimo |
| Eventos relevantes | Agendamento criado/remarcado/cancelado/check-in realizado como candidatos |
| Nao deve conter | prontuario, pagamento, nota fiscal, diagnostico, execucao detalhada do atendimento |

### Atendimento

| Item | Descricao |
| --- | --- |
| Bounded context | Atendimento candidato |
| Aggregate Root | `Atendimento` |
| Entidades internas | Servicos realizados, responsavel presente, observacoes operacionais |
| Value Objects candidatos | `AtendimentoId`, `TenantId`, `SituacaoDoAtendimento`, `SnapshotDoAnimal`, `SnapshotDoResponsavel` |
| Invariantes conhecidas | atendimento iniciado a partir de agendamento ou demanda avulsa confirmada; conclusao exige estado operacional coerente |
| Operacoes candidatas | iniciar, registrar execucao, concluir, cancelar |
| Consistencia | Local ao fluxo operacional; Cobranca e Prontuario devem integrar por contrato ou evento futuro |
| Tamanho esperado | Medio; separado de prontuario quando houver conteudo clinico |
| Referencias externas | Agendamento, animal, profissional, servico |
| Eventos relevantes | Atendimento iniciado/concluido/cancelado como candidatos |
| Nao deve conter | evolucao clinica completa, documento clinico, pagamento, ledger financeiro |

### Prontuario

| Item | Descricao |
| --- | --- |
| Bounded context | Prontuario adiado / candidato forte |
| Aggregate Root | Em aberto: `RegistroClinico`, `ProntuarioDoAnimal` ou `EvolucaoClinica` |
| Entidades internas | Evolucoes, anexos, consentimentos, correcoes |
| Value Objects candidatos | `AutoriaClinica`, `ConteudoClinico`, `Assinatura`, `Retencao` |
| Invariantes conhecidas | autoria clinica; correcao auditada; privacidade; tenant consistente |
| Operacoes candidatas | registrar evolucao, anexar documento, corrigir, assinar/finalizar |
| Consistencia | Forte para autoria e correcoes; retencao/compliance exigem discovery |
| Tamanho esperado | Potencialmente grande; deve ser separado de Atendimento se regras clinicas se confirmarem |
| Referencias externas | Animal e atendimento por snapshot/identidade |
| Eventos relevantes | Registro clinico assinado/corrigido como candidatos |
| Nao deve conter | agenda, cobranca, permissao tecnica, marketing |

### Cobranca

| Item | Descricao |
| --- | --- |
| Bounded context | Cobranca candidato |
| Aggregate Root | `Cobranca` ou `ContaAReceber` |
| Entidades internas | Itens, pagamentos, ajustes, responsavel financeiro |
| Value Objects candidatos | `CobrancaId`, `Money`, `ItemCobrado`, `SituacaoDaCobranca`, `ResponsavelFinanceiro` |
| Invariantes conhecidas | valores owned por Cobranca; item cobrado deve ter origem rastreavel; pagador nao e automaticamente tutor responsavel |
| Operacoes candidatas | gerar, adicionar item, aplicar desconto, receber pagamento, cancelar |
| Consistencia | Financeira local; integracao com Atendimento/Catalogo por snapshot ou contrato |
| Tamanho esperado | Medio/grande conforme pagamentos e fiscal |
| Referencias externas | Atendimento, servico, tutor/responsavel financeiro por snapshot |
| Eventos relevantes | Cobranca gerada/paga/cancelada como candidatos |
| Nao deve conter | execucao clinica, prontuario, disponibilidade, dados pessoais desnecessarios |
