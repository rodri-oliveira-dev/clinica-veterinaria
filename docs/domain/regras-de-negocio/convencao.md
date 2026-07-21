# Convencao do catalogo de regras

## Nomenclatura

Use identificadores estaveis por area:

- `BR-TUT-XXX`: Tutores.
- `BR-ANI-XXX`: Animais.
- `BR-REL-XXX`: Relacionamento Tutor-Animal.
- `BR-TEN-XXX`: Multitenancy aplicado a regras e operacoes tenant-owned.

`BR` significa business rule no catalogo, mesmo quando a classificacao da entrada for decisao arquitetural multitenant. A classificacao deve deixar claro se a entrada e regra de negocio, decisao de produto, decisao arquitetural, politica configuravel ou hipotese.

## Ciclo de vida

Status permitidos:

- `Vigente`: regra aprovada, implementada ou aceita como decisao atual.
- `Proposta`: regra em discussao, sem implementacao como comportamento vigente.
- `Adiada`: descoberta registrada, mas fora da entrega atual.
- `Substituida`: regra historica mantida para rastreabilidade.

Hipoteses e questoes em aberto ficam no backlog central `docs/domain/discovery-backlog.md`, nao no catalogo vigente. O arquivo `lacunas-hipoteses-politicas.md` resume riscos da Entrega 1 e aponta para os itens `DISC-*`.

Mapeamento com a taxonomia de discovery do SDD 28:

| Catalogo | Discovery | Uso |
| --- | --- | --- |
| `Vigente` | `Vigente` | Pode orientar implementacao quando houver evidencia e contexto. |
| `Proposta` | `Hipótese` ou `Questão aberta` | Nao deve orientar codigo, contrato, banco ou autorizacao sem promocao. |
| `Adiada` | `Decisão adiada` | Deve possuir gatilho de retomada e limites da solucao temporaria. |
| `Substituida` | `Descartada` quando a hipotese for rejeitada | Preservar historico e motivo. |

## Alteracao

Ao alterar uma regra:

1. Preserve o identificador.
2. Registre a nova data de validacao.
3. Atualize implementacao, evidencia automatizada e matriz.
4. Atualize lacunas se a regra nao tiver teste ou fonte.
5. Nao transforme validacao tecnica, constraint acidental ou comportamento incidental em regra de negocio.

## Substituicao

Quando uma regra for substituida:

1. Mantenha a regra antiga no catalogo.
2. Mude o status para `Substituida`.
3. Indique a regra sucessora no campo `Reavaliacao` ou em nota historica.
4. Crie novo identificador para a regra sucessora.
5. Nao reutilize identificador removido.

## Revisao

Revise o catalogo quando:

- ADR nova alterar multitenancy, ownership, ciclo de vida ou fronteira.
- Novo caso de uso de Tutores, Animais ou relacionamento for implementado.
- Migration alterar tabelas `tutores`, `animais` ou `historico_transferencias_animais`.
- Teste revelar comportamento divergente de regra vigente.
- Produto validar pessoa juridica, multiplos responsaveis, responsavel financeiro, identificadores externos ou Agenda.
- Algum item `DISC-*` for promovido, adiado, descartado ou virar regra vigente.

## Responsabilidade de manutencao

Quem alterar codigo ou contrato relacionado a uma regra vigente deve atualizar o catalogo, a matriz e o registro de lacunas no mesmo SDD/commit da mudanca.
