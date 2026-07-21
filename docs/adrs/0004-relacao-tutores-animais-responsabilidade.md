# ADR-0004: Relacao entre tutores, animais e responsabilidade operacional

- **Status:** Aceita
- **Data:** 2026-07-21
- **Decisao:** manter um tutor responsavel operacional vigente no `Animal`, com historico de transferencia e ponto explicito de evolucao

## Contexto

A Entrega 1 implementou o cadastro de tutores, o cadastro de animais, o vinculo inicial entre eles e a transferencia explicita de responsabilidade. O modulo `PetShop.Tutores` representa o Bounded Context inicial **Cadastro de Tutores e Animais**, conforme ADR-0003.

A palavra `Tutor` pode ser confundida com proprietario legal, responsavel financeiro, contato de emergencia ou pessoa autorizada. O produto ainda nao validou esses papeis como funcionalidades distintas.

## Problema

O modelo precisa responder qual relacao atual autoriza a clinica a operar o cadastro do animal sem tratar automaticamente todo tutor como proprietario, pagador ou pessoa autorizada para qualquer acao futura.

Tambem precisa preservar evolucao para multiplos responsaveis sem criar agora entidades genericas ou historicos completos que nao possuem fluxo confirmado.

## Forcas arquiteturais

- Simplicidade e entrega vertical ja validada.
- Isolamento multitenant obrigatorio.
- Evitar modelo anemico e updates genericos de FK para operacoes de dominio.
- Evitar abstracoes prematuras como `Pessoa` ou `Party`.
- Preservar caminho para Agenda, Atendimento, Cobranca e Prontuario sem acoplamento direto a tabelas internas.
- Manter migrations seguras e evitar alteracao destrutiva do banco.

## Alternativas consideradas

### 1. `Animal` contendo diretamente `TutorId`

Mantem o schema e os casos de uso simples. E adequado quando existe exatamente um tutor operacional vigente por animal.

Risco: se nomeado apenas como `TutorId`, pode parecer propriedade legal ou relacao sem semantica. A mitigacao e manter a linguagem `TutorResponsavel` e documentar limites.

### 2. Entidade explicita de vinculo entre Tutor e Animal

Permite identidade propria, vigencia, papeis, historico e multiplos responsaveis.

Rejeitada agora porque a fatia atual nao possui multiplos papeis simultaneos nem regras de ciclo de vida do vinculo que justifiquem nova tabela principal. A transferencia ja registra historico minimo.

### 3. Modelo generico de Pessoa com papeis

Poderia unificar tutor, proprietario, responsavel financeiro, representante e outros participantes.

Rejeitada agora porque nao ha regras concretas compartilhadas entre esses conceitos. A opcao aumentaria acoplamento e risco de um modulo generico compartilhado de dominio.

### 4. Multiplos responsaveis implementados imediatamente

Tornaria explicitos responsavel principal, financeiro, proprietario declarado e autorizados.

Rejeitada porque esses papeis ainda sao possibilidades futuras. Implementa-los agora criaria contratos, migrations e telas implicitas sem validacao de produto.

### 5. Modelo atual simples com ponto explicito para evolucao

Mantem `Animal` com `TutorResponsavel`, exige tutor ativo do mesmo tenant, impede transferencia em animal inativo, preserva historico de transferencias e documenta quando uma entidade de vinculo devera ser criada.

Aceita.

## Decisao

O modelo da Entrega 1 permanece com um unico tutor responsavel operacional vigente por animal:

- `Tutor` e a pessoa cadastrada operacionalmente pela clinica dentro do tenant.
- `Animal` guarda `TutorResponsavel` por identidade.
- O banco persiste a relacao em `animais.tutor_responsavel_id`.
- A FK composta `(tenant_id, tutor_responsavel_id)` impede relacao cross-tenant.
- Cadastro de animal exige tutor responsavel existente, ativo e visivel no tenant atual.
- Atualizacao cadastral do animal nao altera responsabilidade.
- Transferencia de responsabilidade e operacao explicita, exige animal ativo, novo tutor ativo, novo tutor diferente do atual e versao atual.
- `historico_transferencias_animais` continua append-only para trilha minima da transferencia.

Nao serao criados neste SDD:

- entidade `Pessoa`;
- entidade principal de vinculo;
- multiplos responsaveis;
- responsavel financeiro;
- proprietario declarado;
- pessoa autorizada;
- contrato entre modulos;
- modulo compartilhado generico.

## Consequencias positivas

- Mantem o modelo coerente com a fatia funcional entregue.
- Evita tratar tutor como sinonimo de proprietario ou pagador.
- Protege o fluxo contra tutor responsavel inativo e animal inativo.
- Preserva isolamento multitenant em Application e banco.
- Mantem baixo custo de manutencao e migracao.
- Deixa claro quando a entidade de vinculo devera ser introduzida.

## Consequencias negativas

- O historico completo de relacoes ainda nao existe.
- Nao ha suporte a mais de uma pessoa relacionada ao animal.
- Modulos futuros nao podem inferir consentimento, retirada ou cobranca apenas pelo `TutorResponsavelId`.
- Uma futura evolucao para multiplos responsaveis exigira migration aditiva e adaptacao de contratos.

## Riscos

- Consumidores futuros podem usar `TutorResponsavelId` como autorizacao ampla se a fronteira nao for reforcada por contratos.
- O modulo pode crescer demais se Agenda, Atendimento ou Cobranca passarem a pedir dados diretamente.
- A tabela de historico atual registra transferencia, mas nao substitui uma modelagem completa de vigencia de vinculos.

## Estrategia de evolucao

Quando houver regra confirmada de multiplos responsaveis, criar uma entidade/tabela tenant-owned de vinculo com migration aditiva, preservando `animais.tutor_responsavel_id` durante uma fase de compatibilidade.

Uma estrategia provavel:

- criar tabela de vinculos com `tenant_id NOT NULL`;
- backfill deterministico a partir de `animais.tutor_responsavel_id`;
- manter constraint para um responsavel principal vigente por animal;
- expor contratos especificos por caso de uso;
- somente depois considerar remover ou depreciar a coluna antiga.

## Condicoes de reavaliacao

Reavaliar esta ADR se:

- houver multiplos responsaveis simultaneos por animal;
- papeis como financeiro, proprietario declarado ou autorizacao de retirada tiverem regras confirmadas;
- Prontuario ou Atendimento exigirem consentimento formal;
- Cobranca precisar separar pagador de tutor operacional;
- Agenda precisar compor dados de tutor/animal em alta frequencia;
- uma relacao precisar de vigencia, suspensao, disputa ou auditoria propria.

## Relacao com codigo, testes e documentacao

- Codigo: `src/Modules/Tutores/PetShop.Tutores/Domain/Animal.cs`
- Codigo: `src/Modules/Tutores/PetShop.Tutores/Application/AnimaisApplicationService.cs`
- Persistencia: `src/Modules/Tutores/PetShop.Tutores/Infrastructure/AnimalEntityTypeConfiguration.cs`
- Testes: `tests/PetShop.UnitTests/Tutores/Domain/AnimalTests.cs`
- Testes: `tests/PetShop.IntegrationTests/AnimaisPersistenceTests.cs`
- Testes: `tests/PetShop.IntegrationTests/AnimaisApiTests.cs`
- Documento de analise: `docs/domain/refinamento-responsabilidades-tutores-animais.md`
