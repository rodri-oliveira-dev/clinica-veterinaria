# Architecture Decision Records

Este diretório registra decisões arquiteturais relevantes para o novo projeto.

ADRs são históricas. Depois que uma decisão for aceita, não reescreva o arquivo como se a decisão original nunca tivesse existido. Quando a arquitetura mudar, crie uma nova ADR que substitua ou complemente a anterior.

## Decisões

| ADR | Status | Decisão |
| --- | --- | --- |
| [ADR-0001](0001-multitenancy-claim-e-isolamento-por-linha.md) | Aceita | Resolver o tenant pela claim `tenant_id` do token e exigir a coluna `tenant_id` em todas as tabelas de negócio |
| [ADR-0002](0002-library-propagacao-observabilidade.md) | Aceita | Padronizar correlação HTTP e propagação W3C/multitenant em building blocks agnósticos de mensageira |
| [ADR-0003](0003-fronteira-cadastro-tutores-animais.md) | Aceita | Manter tutores e animais no mesmo Bounded Context inicial |
| [ADR-0004](0004-relacao-tutores-animais-responsabilidade.md) | Aceita | Manter um tutor responsavel operacional vigente por animal, sem pessoa generica ou multiplos responsaveis nesta etapa |
| [ADR-0005](0005-ciclo-de-vida-animal.md) | Aceita | Usar situacao operacional explicita minima para o ciclo de vida do Animal |
| [ADR-0006](0006-ownership-relacionamento-tutores-animais.md) | Aceita | Manter ownership unico do relacionamento Tutor-Animal e bloquear inativacao de tutor com animal ativo vinculado |
| [ADR-0007](0007-revisao-bounded-contexts-modulos-aggregates.md) | Aceita | Confirmar Cadastro de Tutores e Animais e manter demais capacidades como candidatas ate discovery e fatia vertical |
| [ADR-0008](0008-limites-semanticos-vinculo-tutor-animal.md) | Aceita | Nao inferir capacidades clinicas, financeiras, legais ou de privacidade a partir do vinculo operacional Tutor-Animal |
| [ADR-0009](0009-governanca-hipoteses-discovery-dominio.md) | Aceita | Adotar backlog central DISC-* e taxonomia explicita para governanca de hipoteses e discovery de dominio |

## Convenção

Use nomes no formato:

```text
NNNN-titulo-curto-em-kebab-case.md
```

Cada ADR deve registrar:

- contexto;
- decisão;
- consequências;
- alternativas consideradas;
- pontos ainda pendentes;
- relação com código, testes ou documentação operacional.
