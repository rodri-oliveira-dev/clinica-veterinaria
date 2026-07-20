# ADR-0003: Fronteira inicial para cadastro de tutores e animais

- **Status:** Aceita
- **Data:** 2026-07-20
- **Decisao:** manter tutores e animais no mesmo Bounded Context inicial

## Contexto

A primeira fatia de negocio da plataforma envolve tutores, animais e o vinculo entre eles. A linguagem inicial diferencia `Tutor` como pessoa responsavel pelo animal no relacionamento com a clinica e `Animal` como paciente atendido.

A Entrega 0 criou somente a fundacao tecnica do monolito modular. Ainda nao existem modulos de negocio, entidades persistidas, tabelas funcionais, endpoints de cadastro, contratos entre modulos ou regras implementadas para tutores e animais.

O projeto deve evitar tratar cada substantivo como um Bounded Context. A separacao deve ser guiada por linguagem, ownership, invariantes, ciclo de vida, acoplamento e necessidade real de contratos entre modulos.

## Decisao

Tutores e Animais pertencem ao mesmo Bounded Context inicial: **Cadastro de Tutores e Animais**.

Para a primeira implementacao funcional, a orientacao e iniciar com um unico modulo de negocio para essa capacidade. Ele sera owner dos dados de tutores, contatos, animais e vinculos, todos tenant-owned quando persistidos.

Entre as alternativas da SDD, a opcao de Bounded Contexts distintos esta rejeitada. A opcao de dois modulos dentro do mesmo Bounded Context fica adiada como estrutura fisica possivel, condicionada a evidencia da implementacao.

Nao serao criados agora:

- Bounded Context separado para Tutores;
- Bounded Context separado para Animais;
- contrato de integracao entre Tutores e Animais;
- repository generico;
- projeto `Shared`, `Common` ou `Core` com conceitos de dominio;
- eventos de integracao para sincronizar tutor e animal;
- microsservico, broker, Redis, API Gateway ou banco separado.

## Justificativa

Os fluxos minimos conhecidos mudam juntos e dependem da relacao tutor-animal:

- cadastrar, consultar, atualizar, pesquisar e inativar tutores;
- cadastrar, consultar, atualizar, pesquisar e inativar animais;
- vincular animal a tutor;
- transferir responsabilidade do animal somente se confirmado.

Separar Tutores e Animais em Bounded Contexts distintos neste momento introduziria contratos e possivel coordenacao transacional antes de existir uma regra que justifique esse custo. A invariante de tenant e a associacao entre tutor, animal e vinculo sao mais simples e verificaveis dentro de uma fronteira coesa.

## Consequencias positivas

- Evita fragmentacao prematura da primeira fatia de negocio.
- Mantem a linguagem de dominio em portugues e centrada no fluxo real da clinica.
- Reduz acoplamento artificial entre modulos que ainda nao possuem autonomia.
- Facilita validar isolamento multitenant entre tutor, animal e vinculo.
- Mantem caminho claro para evolucao futura por evidencia.

## Consequencias negativas e custos

- O modulo inicial pode crescer se novas responsabilidades forem adicionadas sem revisao de fronteira.
- Fluxos futuros de agenda, atendimento e faturamento precisarao de contratos deliberados para nao acessar dados internos diretamente.
- Sera necessario criar fitness functions quando a fronteira virar codigo, para proteger dependencias e ownership.

## Alternativas consideradas

### Dois Bounded Contexts distintos

Rejeitada por enquanto. A linguagem e os fluxos conhecidos ainda sao fortemente acoplados pelo vinculo tutor-animal, e a separacao exigiria contratos sem beneficio claro.

### Dois modulos dentro do mesmo Bounded Context

Adiada como estrutura fisica. Pode ser reconsiderada se a implementacao mostrar que Tutores e Animais possuem ciclos de mudanca internos suficientemente diferentes, mas isso nao deve ser antecipado como projetos ou contratos separados.

### Um modulo tecnico generico de cadastro

Rejeitada. A fronteira nao deve virar um deposito generico de cadastros. Ela cobre somente a capacidade Cadastro de Tutores e Animais; outros cadastros, como profissionais ou catalogo de servicos, precisam de avaliacao propria.

## Decisoes ainda pendentes

- Contratos HTTP, requests e responses.
- Entidades, aggregates, Value Objects e invariantes detalhadas.
- Persistencia futura de animais e vinculos.
- Necessidade futura de `DbContext` especifico do modulo caso o contexto tecnico compartilhado deixe de ser suficiente.
- Necessidade de `Responsavel principal` ou `Situacao` como conceitos explicitos.
- Contratos futuros para agenda, atendimento, faturamento ou notificacoes.

## Desdobramento de implementacao

No SDD 13, a fundacao tecnica inicial foi materializada como um unico assembly de modulo:

```text
src/Modules/Tutores/PetShop.Tutores/
```

O assembly representa a capacidade Cadastro de Tutores e Animais, com pastas conceituais `Domain`, `Application`, `Infrastructure` e `Api`. A superficie publica fica limitada aos pontos de composicao `AddModuloTutores` e `MapModuloTutores`, usados pela API do monolito.

Essa implementacao nao cria entidade completa, tabela funcional, migration, repository, contrato HTTP de caso de uso, endpoint funcional ou evento de integracao. As proximas decisoes de persistencia, contratos e invariantes continuam pendentes.

No SDD 15, o aggregate `Tutor` passou a ser persistido em PostgreSQL pela tabela `tutores`, owned por este modulo. A estrategia inicial usa o `PetShopDbContext` tecnico da API como contexto de migrations do monolito e uma extensao publica de persistencia do modulo para aplicar o mapeamento EF Core. O isolamento multitenant combina `tenant_id NOT NULL`, indice unico `(tenant_id, documento)`, query filter parametrizado pelo tenant atual e guarda de `SaveChanges` contra escrita sem tenant resolvido ou com tenant divergente. Essa decisao nao cria endpoint funcional, repository generico, tabela de animais, eventos de integracao, RLS ou contexto de banco separado.

## Relacao com codigo, testes e documentacao

Esta ADR nao criou codigo de producao, migrations, endpoints ou testes quando foi aceita. O SDD 13 adicionou a fundacao de codigo e os testes arquiteturais da fronteira sem implementar funcionalidade de negocio.

Documentacao relacionada:

- `docs/domain/tutores-e-animais.md`;
- `docs/adrs/0001-multitenancy-claim-e-isolamento-por-linha.md`;
- `docs/adrs/0002-library-propagacao-observabilidade.md`.
