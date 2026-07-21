# Refinamento de responsabilidades entre Tutores e Animais

- **Data:** 2026-07-21
- **Escopo:** SDD 22
- **Modulo analisado:** `PetShop.Tutores`

## Objetivo

Registrar o checkpoint de dominio posterior a Entrega 1 sobre o significado de `Tutor`, a relacao com `Animal` e a evolucao futura para responsabilidades distintas, sem introduzir abstracoes ainda nao exigidas pelo produto.

## Inspecao realizada

### Fato confirmado

- A branch atual contem os commits da Entrega 1 posteriores ao SDD 12, incluindo fronteira de dominio, fundacao do modulo, dominio/persistencia/API de tutores, dominio/persistencia/API de animais, transferencia de responsabilidade e validacao da fatia vertical.
- O modulo `src/Modules/Tutores/PetShop.Tutores/` contem Domain, Application, Infrastructure e API em um unico assembly.
- `Tutor` e `Animal` sao aggregates internos ao modulo.
- `Animal` referencia o tutor operacional atual por identidade, via `TutorResponsavel`, persistido em `animais.tutor_responsavel_id`.
- A relacao usa FK composta `(tenant_id, tutor_responsavel_id)` para `tutores (tenant_id, id)`.
- A transferencia de responsabilidade existe, e grava historico minimo em `historico_transferencias_animais`.
- Todas as tabelas funcionais atuais possuem `tenant_id NOT NULL`.
- Os endpoints publicos nao aceitam `tenant_id` como autoridade.

### Decisao vigente

- Tutores e Animais permanecem no Bounded Context inicial **Cadastro de Tutores e Animais**.
- O modulo `PetShop.Tutores` e owner de `tutores`, `animais` e `historico_transferencias_animais`.
- O vinculo atualmente suportado e exatamente um tutor responsavel operacional vigente por animal.
- `Tutor` nao significa proprietario legal, responsavel financeiro, contato de emergencia, pessoa autorizada a retirar animal ou representante de pessoa juridica.
- `Responsavel principal` e um termo reservado para quando houver multiplos responsaveis por animal; na fatia atual, o tutor responsavel vigente cumpre a necessidade operacional minima.
- A transferencia altera explicitamente a responsabilidade operacional vigente do animal e registra a trilha minima da mudanca.

### Hipotese preservada

- Futuramente um animal pode ter multiplas pessoas relacionadas, cada uma com papel, autorizacao, vigencia e nivel de acesso distinto.
- Agenda, Atendimento, Cobranca e Prontuario podem precisar consultar dados resumidos de tutor e animal por contratos ou projecoes, mas ainda nao ha contrato confirmado.
- `Responsavel financeiro`, `proprietario declarado`, `contato de emergencia` e `pessoa autorizada` podem virar conceitos reais, mas permanecem fora do codigo ate existir fluxo validado.

### Questao em aberto

- Qual pessoa pode consentir procedimento clinico, contratar servico, receber documento, acessar prontuario, retirar o animal ou assumir cobranca em cenarios com multiplos envolvidos?
- Se houver disputa entre responsaveis, qual modulo coordenara decisao, auditoria e bloqueio operacional?
- O historico de vinculos exigira vigencia completa, motivo estruturado, aprovacao dupla ou evidencias documentais?

### Regra de negocio consolidada

- O cadastro de animal deve escolher um tutor responsavel existente, ativo e visivel no tenant autenticado.
- A transferencia de responsabilidade exige animal ativo, novo tutor ativo, novo tutor diferente do atual, `versao` atual do animal e tenant autenticado.
- Tutor, animal e historico de transferencia nao podem cruzar tenants.
- Atualizacao cadastral de animal nao troca tutor responsavel.
- Transferencia nao e atualizacao generica de chave estrangeira.

### Possibilidade futura

- Uma entidade explicita de vinculo pode substituir ou complementar `animais.tutor_responsavel_id` quando houver multiplos responsaveis, papeis simultaneos, vigencia, autorizacoes ou historico completo de relacoes.
- Uma abstracao de pessoa pode ser reavaliada se houver regras comuns reais entre tutor, profissional, usuario, fornecedor, pessoa juridica, abrigo ou orgao publico.

## Respostas objetivas

### Conceito de Tutor

`Tutor` representa hoje uma pessoa cadastrada pela clinica para o relacionamento operacional sobre animais dentro de um tenant.

Ele e uma pessoa no cadastro operacional do modulo, nao uma entidade generica global de pessoas e nao uma relacao por si so. A responsabilidade sobre um animal aparece no `Animal` por `TutorResponsavel`.

O modelo atual poderia ser lido como sinonimo de proprietario ou responsavel financeiro se a documentacao nao for explicita. A decisao deste SDD corrige essa ambiguidade pela linguagem: tutor operacional nao implica propriedade legal, responsabilidade financeira, autorizacao clinica ampla ou retirada do animal.

Nao ha razao real para introduzir `Pessoa`, `Party`, `Customer`, `Contact` ou similar agora. Isso anteciparia complexidade sem invariantes concretas, e criaria um centro compartilhado de dominio antes de existir evidencia de linguagem comum.

### Relacao entre Tutor e Animal

O vinculo atual e persistido como uma FK composta no `Animal`, mas nao e apenas detalhe de banco: ele representa a responsabilidade operacional vigente usada pela clinica para cadastrar e manter o animal.

Na fatia atual, o vinculo nao possui identidade propria completa nem varios papeis simultaneos. Seu comportamento relevante e:

- criacao valida com tutor ativo do mesmo tenant;
- preservacao durante atualizacao cadastral;
- transferencia explicita com concorrencia;
- historico minimo da transferencia.

O modelo atual permite um tutor relacionado a varios animais. Ele nao implementa varios responsaveis por animal. A evolucao futura e preservada porque o conceito esta nomeado como `TutorResponsavel` e a decisao de uma entidade explicita de vinculo esta documentada como proxima etapa possivel.

O modulo Cadastro de Tutores e Animais e responsavel por criar, alterar e encerrar a responsabilidade operacional vigente enquanto esse fluxo permanecer na mesma fronteira.

### Responsavel principal

Para a fatia atual, existe apenas o tutor responsavel operacional vigente do animal. Nao foram implementados responsavel financeiro, proprietario declarado ou varias pessoas autorizadas.

`Responsavel principal` nao deve ser criado agora como coluna, entidade ou enum porque so ganha significado quando houver mais de um responsavel simultaneo por animal.

### Transferencia de responsabilidade

A transferencia implementada representa uma operacao de dominio, nao um `PUT` generico de `tutor_responsavel_id`.

Ela altera o `Animal`, registra historico append-only e usa `versao` para proteger contra lost update. A consistencia fica em uma transacao do mesmo DbContext e modulo. O banco usa FKs compostas com `tenant_id`, e a Application consulta sempre no tenant atual.

O refinamento deste SDD acrescentou duas protecoes de dominio:

- tutor inativo nao pode ser escolhido como responsavel ao cadastrar animal;
- animal inativo nao pode ter responsabilidade transferida.

### Ownership e fronteiras

O modulo `PetShop.Tutores` e owner dos dados de tutor, animal e historico de transferencia.

Como Tutor e Animal estao no mesmo modulo nesta fase, a referencia por FK composta e aceitavel. Isso nao autoriza outros modulos a acessar tabelas, `DbContext`, entities EF Core, repositories ou tipos internos do modulo.

Agenda, Atendimento, Cobranca e Prontuario devem depender de contratos deliberados quando surgirem, por exemplo:

- consulta local para validar existencia e situacao resumida de animal;
- leitura resumida de tutor responsavel para exibicao operacional;
- projection local quando houver consultas frequentes;
- workflow explicito quando uma regra futura exigir coordenacao transacional.

## Inconsistencias identificadas

- O cadastro de animal validava existencia do tutor, mas nao a situacao ativa do tutor responsavel.
- A transferencia podia ser invocada sobre animal inativo se a versao atual fosse informada.
- O glossario documentava termos futuros, mas ainda nao separava de forma suficiente fato confirmado, decisao vigente, hipotese e possibilidade futura.

## Decisoes tomadas

- Manter `Animal` contendo `TutorResponsavel` como referencia por identidade.
- Nao criar entidade explicita de vinculo nesta etapa.
- Nao criar modelo generico de pessoa nesta etapa.
- Rejeitar cadastro de animal com tutor responsavel inativo.
- Rejeitar transferencia de responsabilidade de animal inativo.
- Documentar `Tutor` como pessoa operacional cadastrada, distinta de proprietario declarado e responsavel financeiro.

## Alternativas descartadas

- Criar `Pessoa`: descartada por falta de regras comuns confirmadas.
- Criar tabela de vinculos agora: descartada porque ainda ha somente um responsavel vigente e nao existem papeis simultaneos.
- Implementar multiplos responsaveis: descartada por ser possibilidade futura sem fluxo da Entrega 1.
- Renomear todos os endpoints para `responsaveis`: descartada porque quebraria contratos sem ganho proporcional nesta fatia.

## Impacto sobre modulos futuros

Agenda nao deve assumir que `TutorResponsavelId` autoriza consentimento clinico; pode usa-lo somente como contato/responsavel operacional atual ate nova decisao.

Atendimento e Prontuario devem tratar acesso a informacao clinica e consentimento como decisoes proprias, possivelmente com nova revisao de responsabilidades.

Cobranca nao deve presumir que tutor responsavel operacional e devedor financeiro. O responsavel financeiro fica reservado para discovery de Billing.

## Condicoes para nova revisao

Reabrir esta decisao se aparecer qualquer uma destas evidencias:

- mais de uma pessoa precisa estar vinculada ao mesmo animal ao mesmo tempo;
- papeis distintos precisam coexistir, como financeiro, autorizacao de retirada e contato de emergencia;
- uma relacao precisa de vigencia, suspensao, aprovacao, evidencias ou auditoria propria;
- outro modulo precisar consultar responsaveis frequentemente e `TutorResponsavelId` deixar de ser suficiente;
- a transferencia precisar preservar historico completo de vinculos, nao apenas eventos de mudanca;
- pessoa juridica, abrigo, associacao ou orgao publico entrar no fluxo operacional.
