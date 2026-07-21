# ADR-0009: Governanca de hipoteses e discovery de dominio

- **Status:** Aceita
- **Data:** 2026-07-21
- **Decisao:** adotar backlog central `DISC-*` e taxonomia explicita para separar regras vigentes de hipoteses, questoes abertas, decisoes adiadas e hipoteses descartadas

## Contexto

Os SDDs anteriores consolidaram a fundacao tecnica, a Entrega 1 de Cadastro de Tutores e Animais, o catalogo de regras de negocio, o context map e os limites semanticos do vinculo Tutor-Animal.

Ao mesmo tempo, varios temas seguem sem validacao suficiente: Agenda, direitos dos titulares, pessoa juridica, abrigos, ONGs, multiplos responsaveis, responsavel financeiro, pagador, consentimento clinico, acesso ao prontuario, representacao legal e historico temporal completo de vinculos.

Sem uma governanca explicita, essas hipoteses podem aparecer em documentos, contratos ou codigo como se fossem requisitos confirmados.

## Decisao

Criar e manter `docs/domain/discovery-backlog.md` como backlog central de discovery de dominio.

Cada item relevante deve possuir identificador estavel no formato:

```text
DISC-001
DISC-002
DISC-003
```

Estados principais permitidos:

- `Vigente`;
- `Hipótese`;
- `Questão aberta`;
- `Decisão adiada`;
- `Descartada`.

O catalogo de regras de negocio continua usando `BR-*` para regras vigentes e rastreaveis. Hipoteses nao devem ser copiadas para o catalogo como regras. O arquivo `docs/domain/regras-de-negocio/lacunas-hipoteses-politicas.md` permanece como resumo de lacunas da Entrega 1 e passa a apontar para o backlog `DISC-*`.

## Processo de promocao

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

Ao promover um item, o SDD responsavel deve preservar o `DISC-*`, registrar a evidencia, criar ou referenciar a regra `BR-*`, atualizar glossario, matriz de rastreabilidade, roadmap e ADR quando a decisao afetar arquitetura, dominio, privacidade, seguranca ou ownership.

## Decisoes adiadas e descartadas

Decisoes adiadas devem registrar:

- motivo do adiamento;
- gatilho de retomada;
- riscos do adiamento;
- solucao temporaria, quando existir;
- limites da solucao temporaria.

Hipoteses descartadas devem permanecer registradas quando sua remocao puder gerar rediscussao futura. O registro deve conter motivo, evidencia, data ou marco da decisao e impacto.

## Relacao com bounded contexts

Itens de discovery podem listar contextos potencialmente afetados, mas isso nao define ownership definitivo.

Contextos candidatos como Agenda, Atendimento, Cobranca, Prontuario, Notifications, Workforce e Disponibilidade so devem virar modulos fisicos quando houver fatia vertical concreta e decisao de ownership.

## Relacao com roadmap

O roadmap deve distinguir:

- capacidade pronta para implementacao;
- capacidade bloqueada por discovery;
- capacidade parcialmente compreendida;
- capacidade fora do MVP;
- risco que exige validacao antecipada.

Datas ficticias nao devem ser usadas. Quando nao houver estimativa confiavel, registrar dependencias e criterios de prontidao.

## Alternativas consideradas

### Manter hipoteses apenas no arquivo de lacunas da Entrega 1

Rejeitada. O arquivo existente era util para o catalogo do SDD 25, mas agregava varios temas amplos em poucos itens `HYP-*` e nao tinha estrutura suficiente para priorizacao, ownership potencial, impactos e criterio de conclusao.

### Criar regras `BR-*` em estado hipotetico

Rejeitada. Misturar regras vigentes e hipoteses no catalogo aumenta o risco de implementacao especulativa.

### Criar documentos separados por tema

Rejeitada neste momento. O backlog central reduz duplicidade e fornece uma visao priorizavel. Documentos especificos podem nascer em futuros SDDs quando houver discovery real de cada tema.

### Tratar comportamento implementado como fonte suficiente

Rejeitada. Codigo existente pode demonstrar comportamento atual, mas nao transforma sozinho uma hipotese de negocio em regra vigente, especialmente para privacidade, autorizacao, consentimento e financeiro.

## Consequencias positivas

- Evita que suposicoes virem schema, contrato ou politica de autorizacao.
- Facilita priorizar discovery antes de SDDs tecnicos.
- Mantem rastreabilidade entre ADRs, regras, glossario, roadmap e bounded contexts.
- Preserva historico de hipoteses descartadas.
- Reduz risco regulatorio, financeiro, de seguranca e de privacidade por inferencias indevidas.

## Custos de manutencao

- Cada SDD com impacto de dominio precisa atualizar o backlog quando promover, adiar ou descartar uma hipotese.
- O glossario e o catalogo precisam ser revisados junto com decisoes de dominio.
- Itens `DISC-*` podem envelhecer se nao forem revisitados nos gatilhos registrados.

## Riscos evitados

- Tratar `TutorResponsavelId` como pagador, autorizador clinico, representante legal ou titular apto a exercer direitos.
- Criar pessoa juridica, multiplos responsaveis, consentimento, prontuario ou historico temporal sem evidencia.
- Definir constraints de banco e contratos publicos para conceitos ainda ambiguos.
- Implementar Agenda, Cobranca ou Prontuario com ownership semantico errado.

## Relacao com documentacao

- Backlog central: `docs/domain/discovery-backlog.md`
- Catalogo de regras: `docs/domain/regras-de-negocio/catalogo-entrega-1.md`
- Lacunas e hipoteses historicas: `docs/domain/regras-de-negocio/lacunas-hipoteses-politicas.md`
- Glossario: `docs/domain/regras-de-negocio/glossario.md`
- Roadmap: `docs/domain/roadmap-tecnico-funcional.md`
- Context map: `docs/domain/context-map.md`

Documentacao de discovery nao substitui validacao com stakeholders, especialistas de dominio, juridico, seguranca ou privacidade.
