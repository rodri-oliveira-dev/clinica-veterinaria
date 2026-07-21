# Catalogo de regras de negocio - Entrega 1

- **Modulo:** `PetShop.Tutores`
- **Bounded Context:** Cadastro de Tutores e Animais
- **Data de validacao:** 2026-07-21
- **Escopo analisado:** SDDs 12 a 24, ADRs 0001 a 0006, dominio, Application, Infrastructure, API, testes e migrations.

# BR-TUT-001 - Tutor pertence a exatamente um tenant

## Regra
Todo tutor cadastrado pertence a exatamente um tenant, e esse tenant nao pode ser alterado por fluxos comuns.

## Motivacao
Proteger isolamento entre organizacoes e impedir troca acidental ou maliciosa de ownership.

## Classificacao
Regra de negocio com decisao arquitetural multitenant associada.

## Escopo
Tenant / Tutor.

## Gatilho
Cadastro, leitura, atualizacao, pesquisa, inativacao e persistencia de tutor.

## Resultado esperado
Operacoes comuns materializam e alteram somente tutores do tenant autenticado.

## Violacao
Leitura fora do tenant retorna recurso inexistente; escrita sem tenant ou com tenant divergente e rejeitada.

## Excecoes
Nenhuma conhecida para fluxos comuns. Operacao administrativa cross-tenant exige decisao propria.

## Fonte
ADR-0001; ADR-0003; `docs/domain/tutores-e-animais.md`.

## Data de validacao
2026-07-21.

## Implementacao
`Tutor.TenantId`; `TutoresApplicationService`; `TutorEntityTypeConfiguration`; `ModuloTutoresPersistenceExtensions`; `PetShopDbContext`.

## Evidencia automatizada
`TutorTests.Equals_ComMesmoIdEmTenantDiferente_NaoConsideraMesmoTutor`; `TutoresPersistenceTests.LeituraEmOutroTenant_TrataTutorComoInexistente`; `TutoresPersistenceTests.AlteracaoCrossTenant_EhBloqueadaAntesDeSalvar`; `TutoresApiTests.ConsultarTutor_DeOutroTenant_RetornaNotFound`.

## Status
Vigente.

## Reavaliacao
Reavaliar se houver suporte administrativo cross-tenant ou mudanca na estrategia de isolamento.

# BR-TUT-002 - Nome do tutor e obrigatorio

## Regra
Tutor deve possuir nome informado; espacos externos sao removidos.

## Motivacao
Permitir identificacao operacional minima do tutor no cadastro do tenant.

## Classificacao
Regra de negocio.

## Escopo
Tutor.

## Gatilho
Cadastro e atualizacao cadastral de tutor.

## Resultado esperado
Nome nao vazio e persistido normalizado sem espacos externos.

## Violacao
Entrada invalida no contrato HTTP ou excecao de dominio em criacao direta do Value Object.

## Excecoes
Nenhuma conhecida.

## Fonte
`docs/domain/tutores-e-animais.md`; `NomeDoTutor`.

## Data de validacao
2026-07-21.

## Implementacao
`NomeDoTutor`; `Tutor.Cadastrar`; `Tutor.AlterarCadastro`; `TutoresApplicationService`.

## Evidencia automatizada
`ValueObjectTests.NomeDoTutor_Criar_NormalizaEspacosExternos`; `ValueObjectTests.NomeDoTutor_Criar_ComValorVazio_Rejeita`; `TutoresApiTests.CadastrarTutor_ComEntradaInvalida_RetornaValidationProblemDetails`.

## Status
Vigente.

## Reavaliacao
Reavaliar se houver cadastro de pessoa juridica, abrigo ou organizacao com nomenclatura distinta.

# BR-TUT-003 - CPF do tutor e opcional, valido quando informado e unico por tenant

## Regra
CPF nao e obrigatorio. Quando informado, deve ser valido, normalizado e unico somente dentro do tenant atual.

## Motivacao
Permitir cadastro operacional sem documento em cenarios iniciais, mas impedir duplicidade documental dentro da mesma organizacao.

## Classificacao
Regra de negocio.

## Escopo
Tenant / Tutor.

## Gatilho
Cadastro, atualizacao, pesquisa por CPF e persistencia.

## Resultado esperado
CPF ausente e aceito; CPF valido fica normalizado; mesmo CPF em tenants diferentes e permitido; duplicidade no mesmo tenant e conflito.

## Violacao
CPF invalido retorna entrada invalida; duplicidade no tenant atual retorna conflito; constraint do banco rejeita duplicidade direta.

## Excecoes
Nenhuma conhecida.

## Fonte
`docs/domain/tutores-e-animais.md`; README; ADR-0001.

## Data de validacao
2026-07-21.

## Implementacao
`Cpf`; `TutoresApplicationService`; `TutoresRepository.ExisteDocumentoAsync`; indice `ix_tutores_tenant_id_documento`.

## Evidencia automatizada
`TutorTests.Cadastrar_SemDocumento_AceitaCpfOpcional`; `ValueObjectTests.Cpf_Criar_ComCpfValido_NormalizaDigitos`; `ValueObjectTests.Cpf_Criar_ComCpfInvalido_Rejeita`; `TutoresPersistenceTests.CpfIgual_EhPermitidoEmTenantsDiferentes`; `TutoresPersistenceTests.CpfIgual_NoMesmoTenant_EhRejeitadoPeloBanco`; `TutoresApiTests.CadastrarTutor_ComCpfDuplicado_RespeitaEscopoDoTenant`.

## Status
Vigente.

## Reavaliacao
Reavaliar se produto exigir CPF obrigatorio, pessoa juridica, documentos estrangeiros ou deduplicacao assistida.

# BR-TUT-004 - Tutor deve possuir contato operacional

## Regra
Tutor deve possuir ao menos e-mail ou telefone. Cada contato e opcional individualmente.

## Motivacao
Garantir canal minimo de relacionamento operacional sem exigir ambos os canais.

## Classificacao
Regra de negocio.

## Escopo
Tutor.

## Gatilho
Cadastro, atualizacao cadastral e persistencia.

## Resultado esperado
Cadastro com e-mail, telefone ou ambos e aceito.

## Violacao
Cadastro ou atualizacao sem ambos os contatos e rejeitado; banco tambem impede linha sem contato.

## Excecoes
Nenhuma conhecida.

## Fonte
`docs/domain/tutores-e-animais.md`.

## Data de validacao
2026-07-21.

## Implementacao
`Tutor.ValidarContato`; `Email`; `Telefone`; `TutoresApplicationService`; check constraint `ck_tutores_contato_obrigatorio`.

## Evidencia automatizada
`TutorTests.Cadastrar_ComAoMenosUmContato_AceitaCamposDeContatoOpcionais`; `TutorTests.Cadastrar_SemContato_RejeitaTutor`; `TutorTests.AlterarCadastro_SemContato_RejeitaAlteracao`; `ValueObjectTests.Email_Criar_ComEmailValido_NormalizaValor`; `ValueObjectTests.Telefone_Criar_ComTelefoneValido_NormalizaDigitos`; `TutoresApiTests.CadastrarTutor_ComEntradaInvalida_RetornaValidationProblemDetails`.

## Status
Vigente.

## Reavaliacao
Reavaliar se houver contatos adicionais, cadastro presencial sem contato, responsavel financeiro separado ou politica por tenant.

# BR-TUT-005 - Inativacao de tutor nao remove cadastro

## Regra
Inativar tutor muda a situacao para inativo, registra `InativadoEm` e nao executa hard delete.

## Motivacao
Preservar historico operacional e referencias futuras de atendimento, faturamento ou auditoria.

## Classificacao
Regra de negocio.

## Escopo
Tutor / operacao.

## Gatilho
`POST /tutores/{tutorId}/inativacao`.

## Resultado esperado
Tutor permanece consultavel no tenant atual com situacao `inativo`.

## Violacao
Nova inativacao de tutor ja inativo retorna conflito.

## Excecoes
Nenhuma conhecida.

## Fonte
`docs/domain/tutores-e-animais.md`; ADR-0006.

## Data de validacao
2026-07-21.

## Implementacao
`Tutor.Inativar`; `TutoresApplicationService.InativarAsync`; endpoint `InativarTutor`.

## Evidencia automatizada
`TutorTests.Inativar_TutorAtivo_MarcaInativoEAtualizaTimestamp`; `TutorTests.Inativar_TutorJaInativo_RejeitaNovaInativacao`; `TutoresPersistenceTests.Inativacao_PersisteSituacaoETimestamp`; `TutoresApiTests.InativarTutor_MarcaInativoSemHardDelete`.

## Status
Vigente.

## Reavaliacao
Reavaliar com requisitos de retencao, eliminacao, bloqueio ou direitos do titular.

# BR-TUT-006 - Tutor com animal ativo vinculado nao pode ser inativado

## Regra
Tutor responsavel por pelo menos um animal ativo no tenant atual nao pode ser inativado antes de transferencia de responsabilidade ou inativacao do animal.

## Motivacao
Evitar animal ativo com responsavel operacional sem aptidao vigente.

## Classificacao
Regra de negocio.

## Escopo
Tutor / Animal / vinculo / operacao.

## Gatilho
Inativacao de tutor.

## Resultado esperado
Tutor sem animais ativos vinculados pode ser inativado.

## Violacao
Operacao retorna conflito e preserva tutor ativo.

## Excecoes
Animais inativos ou falecidos podem preservar referencia historica ao tutor.

## Fonte
ADR-0006; `docs/domain/tutores-e-animais.md`.

## Data de validacao
2026-07-21.

## Implementacao
`TutoresApplicationService.InativarAsync`; `ITutoresRepository.ExisteAnimalAtivoVinculadoAsync`; `TutoresRepository`.

## Evidencia automatizada
`TutoresApiTests.InativarTutor_ComAnimalAtivoVinculado_RetornaConflictEPreservaTutor`.

## Status
Vigente.

## Reavaliacao
Reavaliar se houver transferencia em lote, multiplos responsaveis ou workflow administrativo de encerramento.

# BR-TUT-007 - Atualizacao de tutor preserva identidade e tenant

## Regra
Atualizacao cadastral de tutor altera dados cadastrais permitidos, mas preserva `TutorId`, `TenantId` e `CriadoEm`.

## Motivacao
Separar manutencao cadastral de mudanca de identidade ou ownership.

## Classificacao
Regra de negocio com decisao arquitetural multitenant associada.

## Escopo
Tutor / operacao.

## Gatilho
Atualizacao cadastral de tutor.

## Resultado esperado
Nome, CPF e contatos podem ser atualizados quando validos; `AtualizadoEm` muda.

## Violacao
Tentativa de alterar tenant ou registro cross-tenant e bloqueada pela persistencia; `id` e `tenant_id` no body nao sao autoridade.

## Excecoes
Nenhuma conhecida.

## Fonte
ADR-0001; `docs/domain/tutores-e-animais.md`.

## Data de validacao
2026-07-21.

## Implementacao
`Tutor.AlterarCadastro`; `TutoresApplicationService.AtualizarAsync`; `AtualizarTutorRequest`.

## Evidencia automatizada
`TutorTests.AlterarCadastro_ComDadosValidos_AtualizaCamposETimestamp`; `TutoresApiTests.AtualizarTutor_AlteraCadastroSemAceitarIdOuTenantNoBody`; `TutoresPersistenceTests.AlteracaoCrossTenant_EhBloqueadaAntesDeSalvar`.

## Status
Vigente.

## Reavaliacao
Reavaliar se houver merge de duplicidades ou transferencia administrativa de cadastro entre tenants.

# BR-ANI-001 - Animal pertence a exatamente um tenant

## Regra
Todo animal pertence a exatamente um tenant, e esse tenant nao pode ser alterado por fluxos comuns.

## Motivacao
Preservar isolamento dos dados do paciente animal entre organizacoes.

## Classificacao
Regra de negocio com decisao arquitetural multitenant associada.

## Escopo
Tenant / Animal.

## Gatilho
Cadastro, leitura, atualizacao, pesquisa, inativacao, falecimento, transferencia e persistencia.

## Resultado esperado
Operacoes comuns materializam e alteram somente animais do tenant autenticado.

## Violacao
Dados de outro tenant retornam recurso inexistente; escrita sem tenant ou com tenant divergente e rejeitada.

## Excecoes
Nenhuma conhecida para fluxos comuns.

## Fonte
ADR-0001; ADR-0003; ADR-0006.

## Data de validacao
2026-07-21.

## Implementacao
`Animal.TenantId`; `AnimaisApplicationService`; `AnimalEntityTypeConfiguration`; `ModuloTutoresPersistenceExtensions`.

## Evidencia automatizada
`AnimalTests.Equals_ComMesmoIdEmTenantDiferente_NaoConsideraMesmoAnimal`; `AnimaisPersistenceTests.LeituraEmOutroTenant_TrataAnimalComoInexistente`; `AnimaisPersistenceTests.AlteracaoCrossTenant_EhBloqueadaAntesDeSalvar`; `AnimaisApiTests.ConsultarAnimal_DeOutroTenant_RetornaNotFound`.

## Status
Vigente.

## Reavaliacao
Reavaliar se houver operacao administrativa cross-tenant ou reorganizacao de tenants.

# BR-ANI-002 - Animal deve possuir nome e especie

## Regra
Animal deve possuir nome e especie informados. Ambos sao textuais e normalizados por remocao de espacos externos.

## Motivacao
Garantir identificacao operacional minima do animal sem criar catalogos prematuros.

## Classificacao
Regra de negocio.

## Escopo
Animal.

## Gatilho
Cadastro e atualizacao cadastral de animal.

## Resultado esperado
Nome e especie nao vazios sao aceitos.

## Violacao
Entrada invalida no HTTP ou excecao de dominio; banco rejeita textos obrigatorios em branco.

## Excecoes
Nenhuma conhecida.

## Fonte
`docs/domain/tutores-e-animais.md`; `docs/domain/refinamento-ciclo-de-vida-animal.md`.

## Data de validacao
2026-07-21.

## Implementacao
`NomeDoAnimal`; `Especie`; `Animal.Cadastrar`; `Animal.AlterarCadastro`; `AnimaisApplicationService`; constraints `ck_animais_nome_not_blank` e `ck_animais_especie_not_blank`.

## Evidencia automatizada
`AnimalValueObjectTests.NomeDoAnimal_Criar_ComValorVazio_Rejeita`; `AnimalValueObjectTests.Especie_Criar_ComValorVazio_Rejeita`; `AnimaisApiTests.CadastrarAnimal_ComEntradaInvalida_RetornaValidationProblemDetails`.

## Status
Vigente.

## Reavaliacao
Reavaliar se especie virar catalogo ou se houver importacao com dados incompletos.

# BR-ANI-003 - Raca e dados cadastrais complementares sao opcionais

## Regra
Raca, cor ou pelagem e observacao cadastral sao opcionais; quando informadas, nao podem ser vazias apos normalizacao.

## Motivacao
Permitir cadastro simples sem inventar dados ou exigir completude ainda nao validada.

## Classificacao
Regra de negocio.

## Escopo
Animal.

## Gatilho
Cadastro, atualizacao cadastral e persistencia.

## Resultado esperado
Campos ausentes sao aceitos; campos presentes sao persistidos normalizados.

## Violacao
Valor presente e vazio e rejeitado pelo dominio ou pelo banco.

## Excecoes
Nenhuma conhecida.

## Fonte
`docs/domain/tutores-e-animais.md`; `docs/domain/refinamento-ciclo-de-vida-animal.md`.

## Data de validacao
2026-07-21.

## Implementacao
`Raca`; `CorOuPelagem`; `ObservacaoCadastral`; `AnimalEntityTypeConfiguration`.

## Evidencia automatizada
`AnimalTests.Cadastrar_ComCamposOpcionaisAusentes_CriaAnimal`; `AnimalTests.AlterarCadastro_ComCamposOpcionaisAusentes_RemoveCamposOpcionais`; `AnimalValueObjectTests.Raca_Criar_ComValorVazio_Rejeita`; `AnimalValueObjectTests.CorOuPelagem_Criar_ComValorVazio_Rejeita`; `AnimalValueObjectTests.ObservacaoCadastral_Criar_ComValorVazio_Rejeita`; `AnimaisPersistenceTests.ValueObjects_SaoPersistidosNormalizados`.

## Status
Vigente.

## Reavaliacao
Reavaliar se houver catalogo de racas, alertas clinicos ou prontuario.

# BR-ANI-004 - Data de nascimento e opcional e nao pode estar no futuro

## Regra
Data de nascimento do animal pode ser desconhecida. Quando informada, deve ser uma data exata nao futura.

## Motivacao
Evitar falsa precisao e impedir cadastro de datas impossiveis.

## Classificacao
Regra de negocio.

## Escopo
Animal.

## Gatilho
Cadastro e atualizacao cadastral de animal.

## Resultado esperado
Ausencia de data representa desconhecimento; data informada e aceita se nao estiver no futuro.

## Violacao
Data futura retorna entrada invalida.

## Excecoes
Nenhuma conhecida.

## Fonte
`docs/domain/refinamento-ciclo-de-vida-animal.md`.

## Data de validacao
2026-07-21.

## Implementacao
`DataDeNascimento`; `AnimaisApplicationService.ValidarDadosDoAnimal`.

## Evidencia automatizada
`AnimalValueObjectTests.DataDeNascimento_Criar_ComDataHoje_Aceita`; `AnimalValueObjectTests.DataDeNascimento_Criar_ComDataFutura_Rejeita`; `AnimalTests.Cadastrar_ComCamposOpcionaisAusentes_CriaAnimal`.

## Status
Vigente.

## Reavaliacao
Reavaliar quando houver requisito de idade estimada, mes/ano ou fonte da informacao.

# BR-ANI-005 - Animal possui ciclo de vida operacional minimo

## Regra
Animal nasce `ativo` e pode assumir somente as situacoes `ativo`, `inativo` ou `falecido`.

## Motivacao
Diferenciar disponibilidade operacional, retirada administrativa e falecimento sem antecipar uma maquina de estados completa.

## Classificacao
Regra de negocio / decisao de produto.

## Escopo
Animal / operacao.

## Gatilho
Cadastro, pesquisa, inativacao, falecimento, atualizacao, transferencia e persistencia.

## Resultado esperado
Situacoes conhecidas sao aceitas e expostas nos contratos HTTP.

## Violacao
Situacao fora do dominio e rejeitada pelo banco ou pela aplicacao.

## Excecoes
Nenhuma conhecida.

## Fonte
ADR-0005; `docs/domain/refinamento-ciclo-de-vida-animal.md`.

## Data de validacao
2026-07-21.

## Implementacao
`SituacaoDoAnimal`; `Animal`; `AnimalEntityTypeConfiguration`.

## Evidencia automatizada
`AnimalTests.Cadastrar_ComDadosValidos_CriaAnimalAtivo`; `AnimaisPersistenceTests.SexoForaDoDominio_EhRejeitadoPeloBanco` cobre dominio fisico de enumeracoes; `AnimaisApiTests.PesquisarAnimais_AplicaFiltrosPaginacaoOrdenacaoEIsolamento`.

## Status
Vigente.

## Reavaliacao
Reavaliar se `desaparecido`, `duplicado` ou `arquivado` tiverem fluxo confirmado.

# BR-ANI-006 - Falecimento e transicao explicita com data obrigatoria

## Regra
Falecimento do animal deve ser registrado por operacao explicita, com `DataDoFalecimento` obrigatoria e nao futura.

## Motivacao
Separar falecimento de inativacao administrativa e preparar bloqueios futuros de Agenda e Atendimento.

## Classificacao
Regra de negocio / decisao de produto.

## Escopo
Animal / operacao.

## Gatilho
`POST /animais/{animalId}/falecimento` e persistencia.

## Resultado esperado
Animal passa para `falecido`, registra data de falecimento e incrementa versao.

## Violacao
Data ausente ou futura retorna entrada invalida; novo registro de falecimento retorna conflito; banco rejeita estado incoerente entre situacao e data.

## Excecoes
Nenhuma conhecida.

## Fonte
ADR-0005; `docs/domain/refinamento-ciclo-de-vida-animal.md`.

## Data de validacao
2026-07-21.

## Implementacao
`DataDoFalecimento`; `Animal.RegistrarFalecimento`; `AnimaisApplicationService.RegistrarFalecimentoAsync`; endpoint `RegistrarFalecimentoDoAnimal`; constraint `ck_animais_data_falecimento_situacao`.

## Evidencia automatizada
`AnimalValueObjectTests.DataDoFalecimento_Criar_ComDataFutura_Rejeita`; `AnimalTests.RegistrarFalecimento_AnimalAtivo_MarcaFalecidoEAtualizaTimestamp`; `AnimalTests.RegistrarFalecimento_AnimalJaFalecido_RejeitaEPreservaEstado`; `AnimaisPersistenceTests.Falecimento_PersisteSituacaoEData`; `AnimaisPersistenceTests.FalecimentoSemData_EhRejeitadoPeloBanco`; `AnimaisPersistenceTests.DataDoFalecimentoSemSituacaoFalecido_EhRejeitadaPeloBanco`; `AnimaisApiTests.RegistrarFalecimento_ComDadosValidos_MarcaFalecidoEBloqueiaFluxosIncompativeis`; `AnimaisApiTests.RegistrarFalecimento_ComDataFutura_RetornaValidationProblemDetails`.

## Status
Vigente.

## Reavaliacao
Reavaliar se correcoes de falecimento exigirem auditoria funcional ou documento de obito.

# BR-ANI-007 - Animal falecido bloqueia fluxos comuns incompativeis

## Regra
Animal falecido nao pode sofrer atualizacao cadastral comum, inativacao ou transferencia de responsabilidade.

## Motivacao
Evitar estados contraditorios e proteger fluxos futuros que dependam da situacao operacional do animal.

## Classificacao
Regra de negocio.

## Escopo
Animal / operacao.

## Gatilho
Atualizacao, inativacao e transferencia de animal.

## Resultado esperado
Operacoes incompativeis sobre animal falecido retornam conflito.

## Violacao
Operacao e recusada e o estado anterior e preservado.

## Excecoes
Nenhuma conhecida.

## Fonte
ADR-0005; `docs/domain/refinamento-ciclo-de-vida-animal.md`.

## Data de validacao
2026-07-21.

## Implementacao
`Animal.AlterarCadastro`; `Animal.Inativar`; `Animal.TransferirResponsabilidade`; `AnimaisApplicationService`.

## Evidencia automatizada
`AnimalTests.AlterarCadastro_AnimalFalecido_RejeitaEPreservaEstado`; `AnimalTests.Inativar_AnimalFalecido_Rejeita`; `AnimalTests.TransferirResponsabilidade_ComAnimalFalecido_RejeitaEPreservaEstado`; `AnimaisApiTests.RegistrarFalecimento_ComDadosValidos_MarcaFalecidoEBloqueiaFluxosIncompativeis`.

## Status
Vigente.

## Reavaliacao
Reavaliar se houver fluxo auditado de correcao de falecimento.

# BR-ANI-008 - Atualizacao cadastral de animal preserva tutor responsavel

## Regra
Atualizacao cadastral comum de animal nao altera `AnimalId`, `TenantId`, `TutorResponsavelId` nem `CriadoEm`.

## Motivacao
Separar manutencao cadastral da operacao de transferencia de responsabilidade.

## Classificacao
Regra de negocio.

## Escopo
Animal / vinculo / operacao.

## Gatilho
`PUT /animais/{animalId}`.

## Resultado esperado
Dados cadastrais mudam e versao incrementa; tutor responsavel permanece.

## Violacao
Troca de responsavel no body nao e aceita como autoridade; transferencia deve usar endpoint explicito.

## Excecoes
Nenhuma conhecida.

## Fonte
ADR-0004; `docs/domain/tutores-e-animais.md`.

## Data de validacao
2026-07-21.

## Implementacao
`Animal.AlterarCadastro`; `AtualizarAnimalRequest`; endpoint `AtualizarAnimal`.

## Evidencia automatizada
`AnimalTests.AlterarCadastro_ComDadosValidos_AtualizaCamposETimestamp`; `AnimaisApiTests.AtualizarAnimal_AlteraCadastroSemAceitarTutorOuTenantNoBody`.

## Status
Vigente.

## Reavaliacao
Reavaliar se houver entidade propria de vinculo ou multiplos responsaveis.

# BR-ANI-009 - Inativacao de animal nao remove cadastro nem vinculo

## Regra
Inativar animal muda situacao para `inativo`, registra `InativadoEm`, incrementa versao e preserva o tutor responsavel como referencia historica minima.

## Motivacao
Retirar o animal dos fluxos comuns sem apagar dados ou quebrar referencias.

## Classificacao
Regra de negocio.

## Escopo
Animal / vinculo / operacao.

## Gatilho
`POST /animais/{animalId}/inativacao`.

## Resultado esperado
Animal permanece consultavel no tenant atual com situacao `inativo`.

## Violacao
Nova inativacao retorna conflito; animal falecido retorna conflito especifico.

## Excecoes
Nenhuma conhecida.

## Fonte
ADR-0005; ADR-0006; `docs/domain/tutores-e-animais.md`.

## Data de validacao
2026-07-21.

## Implementacao
`Animal.Inativar`; `AnimaisApplicationService.InativarAsync`; endpoint `InativarAnimal`.

## Evidencia automatizada
`AnimalTests.Inativar_AnimalAtivo_MarcaInativoEAtualizaTimestamp`; `AnimalTests.Inativar_AnimalJaInativo_RejeitaNovaInativacao`; `AnimaisPersistenceTests.Inativacao_PersisteSituacaoETimestamp`; `AnimaisApiTests.InativarAnimal_MarcaInativoSemHardDelete`.

## Status
Vigente.

## Reavaliacao
Reavaliar se houver arquivamento, deduplicacao ou exclusao logica com retencao.

# BR-REL-001 - Animal possui exatamente um tutor responsavel vigente

## Regra
Animal possui exatamente um `TutorResponsavel` vigente em todos os estados atuais.

## Motivacao
Representar a responsabilidade operacional minima necessaria para cadastrar e manter o animal.

## Classificacao
Regra de negocio / decisao de produto.

## Escopo
Animal / vinculo.

## Gatilho
Cadastro, consulta, persistencia, inativacao, falecimento e transferencia.

## Resultado esperado
Animal sempre possui `tutorResponsavelId` nao vazio.

## Violacao
Cadastro sem tutor responsavel e rejeitado; banco rejeita `tutor_responsavel_id` vazio.

## Excecoes
Nenhuma conhecida.

## Fonte
ADR-0004; ADR-0006.

## Data de validacao
2026-07-21.

## Implementacao
`TutorResponsavel`; `Animal.TutorResponsavelId`; `AnimalEntityTypeConfiguration`.

## Evidencia automatizada
`AnimalValueObjectTests.TutorResponsavel_Criar_ComIdentificadorVazio_Rejeita`; `AnimaisPersistenceTests.CriacaoELeituraNoMesmoTenant_PersisteAnimalEVinculo`; `AnimaisApiTests.CadastrarAnimal_ComDadosValidos_RetornaCreatedComLocationEContratoMinimo`.

## Status
Vigente.

## Reavaliacao
Reavaliar se houver multiplos responsaveis, papeis, vigencia ou entidade propria de vinculo.

# BR-REL-002 - Tutor pode ser responsavel por varios animais

## Regra
Um tutor pode ser responsavel por zero, um ou muitos animais dentro do mesmo tenant.

## Motivacao
Refletir a operacao comum de uma pessoa manter varios animais no cadastro.

## Classificacao
Regra de negocio / decisao de produto.

## Escopo
Tutor / Animal / vinculo.

## Gatilho
Cadastro de animais e persistencia do vinculo.

## Resultado esperado
Mais de um animal pode apontar para o mesmo tutor no mesmo tenant.

## Violacao
Nao ha violacao conhecida nessa cardinalidade; o banco nao possui unicidade sobre `(tenant_id, tutor_responsavel_id)`.

## Excecoes
Nenhuma conhecida.

## Fonte
ADR-0006.

## Data de validacao
2026-07-21.

## Implementacao
Ausencia deliberada de indice unico em `(tenant_id, tutor_responsavel_id)`; `AnimalEntityTypeConfiguration`.

## Evidencia automatizada
`AnimaisPersistenceTests.MesmoTutorPodeSerResponsavelPorMultiplosAnimaisNoTenant`.

## Status
Vigente.

## Reavaliacao
Reavaliar se produto criar limites por tutor, planos comerciais ou regras de capacidade.

# BR-REL-003 - Cadastro de animal exige tutor responsavel ativo do mesmo tenant

## Regra
Cadastrar animal exige tutor responsavel existente, ativo e visivel no tenant autenticado.

## Motivacao
Impedir animal novo sem responsavel operacional apto e impedir associacao cross-tenant.

## Classificacao
Regra de negocio.

## Escopo
Tenant / Animal / Tutor / vinculo / operacao.

## Gatilho
Cadastro de animal.

## Resultado esperado
Animal e criado vinculado ao tutor responsavel ativo do mesmo tenant.

## Violacao
Tutor inexistente ou de outro tenant retorna `404`; tutor inativo retorna `409`; FK composta protege escrita direta inconsistente.

## Excecoes
Nenhuma conhecida.

## Fonte
ADR-0004; ADR-0006; `docs/domain/refinamento-responsabilidades-tutores-animais.md`.

## Data de validacao
2026-07-21.

## Implementacao
`AnimaisApplicationService.CadastrarAsync`; `AnimaisRepository.ObterTutorResponsavelPorIdAsync`; FK `fk_animais_tutores_tenant_id_tutor_responsavel_id`.

## Evidencia automatizada
`AnimaisPersistenceTests.CadastrarAnimal_ComTutorDoMesmoTenant_PersistePelaApplication`; `AnimaisPersistenceTests.CadastrarAnimal_ComTutorDeOutroTenant_TrataTutorComoInexistente`; `AnimaisPersistenceTests.CadastrarAnimal_ComTutorInativo_RejeitaResponsavelOperacional`; `AnimaisPersistenceTests.TutorInexistente_EhRejeitadoPelaForeignKeyComposta`; `AnimaisPersistenceTests.TutorDeOutroTenant_EhRejeitadoPelaForeignKeyComposta`; `AnimaisApiTests.CadastrarAnimal_ComTutorInexistente_RetornaNotFound`; `AnimaisApiTests.CadastrarAnimal_ComTutorDeOutroTenant_RetornaNotFound`; `AnimaisApiTests.CadastrarAnimal_ComTutorInativo_RetornaConflict`.

## Status
Vigente.

## Reavaliacao
Reavaliar se houver cadastro de animal sem responsavel, acolhimento por abrigo ou workflow pendente de vinculacao.

# BR-REL-004 - Transferencia de responsabilidade e operacao explicita

## Regra
Transferencia de responsabilidade nao ocorre por atualizacao cadastral. Ela exige animal ativo, novo tutor ativo do mesmo tenant, novo tutor diferente do atual e versao atual do animal.

## Motivacao
Proteger uma mudanca de responsabilidade operacional contra updates genericos, concorrencia perdida e associacao invalida.

## Classificacao
Regra de negocio.

## Escopo
Animal / Tutor / vinculo / operacao.

## Gatilho
`POST /animais/{animalId}/transferencias-de-responsabilidade`.

## Resultado esperado
Tutor responsavel do animal e alterado, versao incrementa e historico minimo e registrado.

## Violacao
Animal inativo ou falecido, mesmo tutor, tutor inativo, tutor inexistente/cross-tenant ou versao desatualizada retornam conflito ou inexistencia conforme o caso.

## Excecoes
Nenhuma conhecida.

## Fonte
ADR-0004; ADR-0006.

## Data de validacao
2026-07-21.

## Implementacao
`Animal.TransferirResponsabilidade`; `AnimaisApplicationService.TransferirResponsabilidadeAsync`; endpoint `TransferirResponsabilidadeDoAnimal`; `versao` como concurrency token.

## Evidencia automatizada
`AnimalTests.TransferirResponsabilidade_ComNovoTutor_AtualizaTutorTimestampEVersao`; `AnimalTests.TransferirResponsabilidade_ComMesmoTutor_Rejeita`; `AnimalTests.TransferirResponsabilidade_ComAnimalInativo_RejeitaEPreservaEstado`; `AnimalTests.TransferirResponsabilidade_ComAnimalFalecido_RejeitaEPreservaEstado`; `AnimaisApiTests.TransferirResponsabilidade_ComDadosValidos_AtualizaTutorEVersaoERegistraHistorico`; `AnimaisApiTests.TransferirResponsabilidade_ComTutorInativo_RetornaConflict`; `AnimaisApiTests.TransferirResponsabilidade_ComMesmoTutor_RetornaConflict`; `AnimaisApiTests.TransferirResponsabilidade_ComDadosDeOutroTenant_RetornaNotFound`; `AnimaisApiTests.TransferirResponsabilidade_ComVersaoDesatualizada_RetornaConflict`; `AnimaisApiTests.TransferirResponsabilidade_ComAnimalInativo_RetornaConflictEPreservaTutor`.

## Status
Vigente.

## Reavaliacao
Reavaliar se houver multiplos responsaveis, aprovacao dupla, disputa ou transferencia em lote.

# BR-REL-005 - Historico de transferencia preserva trilha minima

## Regra
Toda transferencia de responsabilidade registra historico minimo append-only com tenant, animal, tutor anterior, tutor novo, data, subject autenticado e motivo opcional normalizado.

## Motivacao
Preservar a trilha operacional da mudanca sem armazenar dados pessoais ou claims completas.

## Classificacao
Regra de negocio com decisao de privacidade minima associada.

## Escopo
Tenant / Animal / Tutor / vinculo / operacao.

## Gatilho
Transferencia de responsabilidade.

## Resultado esperado
Registro e criado no mesmo tenant e na mesma transacao local da transferencia.

## Violacao
Subject ausente retorna proibido; historico com IDs vazios, tutores iguais ou relacionamento cross-tenant e rejeitado pelo dominio/banco; tentativa de alterar ou excluir historico e bloqueada pela guarda de persistencia.

## Excecoes
Nenhuma conhecida para fluxos comuns.

## Fonte
ADR-0004; ADR-0006; README.

## Data de validacao
2026-07-21.

## Implementacao
`TransferenciaDeResponsabilidadeDoAnimal`; `TransferenciaDeResponsabilidadeDoAnimalEntityTypeConfiguration`; `ModuloTutoresPersistenceExtensions.ValidarAlteracoesTenantOwnedDoModuloTutores`.

## Evidencia automatizada
`AnimaisApiTests.TransferirResponsabilidade_ComDadosValidos_AtualizaTutorEVersaoERegistraHistorico`. Lacuna: nao ha teste dedicado para bloquear update/delete manual do historico.

## Status
Vigente.

## Reavaliacao
Reavaliar se historico minimo deixar de ser suficiente para auditoria, vigencia completa ou requisitos legais.

# BR-TEN-001 - Tenant vem exclusivamente da autenticacao

## Regra
Fluxos comuns obtem o tenant exclusivamente da claim validada `tenant_id` do access token autenticado.

## Motivacao
Impedir spoofing de tenant por body, rota, query ou header.

## Classificacao
Decisao arquitetural multitenant.

## Escopo
Tenant / operacao.

## Gatilho
Toda requisicao autenticada que acessa dados de negocio.

## Resultado esperado
Tenant resolvido fica disponivel para Application e persistencia.

## Violacao
Claim ausente, vazia, duplicada ou invalida retorna `403` com `identity.tenant_required`.

## Excecoes
Nenhuma conhecida para fluxos comuns.

## Fonte
ADR-0001.

## Data de validacao
2026-07-21.

## Implementacao
`TenantIdParser`; `HttpTenantContextMiddleware`; `ITenantContext`; `ModuloTutoresServiceCollectionExtensions`.

## Evidencia automatizada
`TenantIdParserTests`; `JwtBearerDiagnosticsAuthorizationTests`; `TutoresApiTests.Tutores_ExigeAutenticacaoRoleETenantValido`; `AnimaisApiTests.Animais_ExigeAutenticacaoRoleETenantValido`.

## Status
Vigente.

## Reavaliacao
Reavaliar se houver impersonation, suporte administrativo ou outro provedor de identidade.

# BR-TEN-002 - Contratos HTTP comuns nao aceitam tenant como autoridade

## Regra
Requests de Tutores e Animais nao expoem nem aceitam `tenant_id` como propriedade ou parametro de autoridade.

## Motivacao
Manter a borda autenticada como unica fonte confiavel do tenant.

## Classificacao
Decisao arquitetural multitenant.

## Escopo
Tenant / endpoint / operacao.

## Gatilho
Contratos HTTP de Tutores e Animais.

## Resultado esperado
`tenantId` enviado no body nao altera o escopo da operacao; respostas nao publicam tenant.

## Violacao
Membro nao mapeado ou tentativa de usar tenant externo nao muda o tenant autenticado.

## Excecoes
Nenhuma conhecida para fluxos comuns.

## Fonte
ADR-0001; README; `docs/domain/tutores-e-animais.md`.

## Data de validacao
2026-07-21.

## Implementacao
Requests privados em `ModuloTutoresEndpointRouteBuilderExtensions`; configuracao JSON fechada na API.

## Evidencia automatizada
`TutoresApiTests.AtualizarTutor_AlteraCadastroSemAceitarIdOuTenantNoBody`; `AnimaisApiTests.AtualizarAnimal_AlteraCadastroSemAceitarTutorOuTenantNoBody`; `OpenApiContractTests` valida contrato OpenAPI sem publicacao indevida de tenant nos contratos funcionais.

## Status
Vigente.

## Reavaliacao
Reavaliar apenas com fluxo administrativo cross-tenant aprovado.

# BR-TEN-003 - Dados tenant-owned sao filtrados e protegidos na escrita

## Regra
Consultas comuns usam filtro por tenant atual, e `SaveChanges` rejeita alteracoes tenant-owned quando nao ha tenant resolvido ou quando o registro pertence a outro tenant.

## Motivacao
Combinar protecao de leitura e escrita para evitar vazamento ou corrupcao cross-tenant.

## Classificacao
Decisao arquitetural multitenant.

## Escopo
Tenant / persistencia.

## Gatilho
Leitura e escrita de `tutores`, `animais` e `historico_transferencias_animais`.

## Resultado esperado
Sem tenant resolvido, consultas comuns nao retornam dados e escritas sao rejeitadas.

## Violacao
Alteracao sem tenant ou cross-tenant gera excecao antes de salvar.

## Excecoes
Tabelas tecnicas, como historico de migrations, nao sao dados de negocio.

## Fonte
ADR-0001; ADR-0003; README.

## Data de validacao
2026-07-21.

## Implementacao
Query filters em `TutorEntityTypeConfiguration`, `AnimalEntityTypeConfiguration` e `TransferenciaDeResponsabilidadeDoAnimalEntityTypeConfiguration`; guarda em `ModuloTutoresPersistenceExtensions`.

## Evidencia automatizada
`TutoresPersistenceTests.LeituraEmOutroTenant_TrataTutorComoInexistente`; `TutoresPersistenceTests.TenantObrigatorio_EhRejeitadoAoAlterarEntidadeTenantOwned`; `AnimaisPersistenceTests.LeituraEmOutroTenant_TrataAnimalComoInexistente`; `AnimaisPersistenceTests.TenantObrigatorio_EhRejeitadoAoAlterarEntidadeTenantOwned`.

## Status
Vigente.

## Reavaliacao
Reavaliar se ADR futura aprovar Row-Level Security, interceptors ou outro enforcement.

# BR-TEN-004 - Relacionamentos cross-tenant sao proibidos

## Regra
Relacionamentos entre dados tenant-owned devem incluir tenant no limite de integridade e impedir associacao entre tenants.

## Motivacao
Evitar que um animal ou historico de transferencia de um tenant aponte para tutor ou animal de outro tenant.

## Classificacao
Decisao arquitetural multitenant com regra de negocio aplicada ao vinculo.

## Escopo
Tenant / Animal / Tutor / vinculo / historico.

## Gatilho
Cadastro de animal, transferencia de responsabilidade e persistencia direta.

## Resultado esperado
Somente entidades do mesmo tenant podem ser associadas.

## Violacao
Application trata registro cross-tenant como inexistente; FK composta rejeita tentativa direta no banco.

## Excecoes
Nenhuma conhecida.

## Fonte
ADR-0001; ADR-0004; ADR-0006.

## Data de validacao
2026-07-21.

## Implementacao
FK `fk_animais_tutores_tenant_id_tutor_responsavel_id`; FKs `fk_hist_transf_animais_*`; repositories filtrados pelo tenant.

## Evidencia automatizada
`AnimaisPersistenceTests.TutorDeOutroTenant_EhRejeitadoPelaForeignKeyComposta`; `AnimaisApiTests.CadastrarAnimal_ComTutorDeOutroTenant_RetornaNotFound`; `AnimaisApiTests.TransferirResponsabilidade_ComDadosDeOutroTenant_RetornaNotFound`.

## Status
Vigente.

## Reavaliacao
Reavaliar apenas com fluxo administrativo cross-tenant formal.

# BR-TEN-005 - Unicidade local ao tenant inclui tenant no indice

## Regra
Unicidade que pertence ao tenant deve incluir `tenant_id` no indice ou constraint correspondente.

## Motivacao
Permitir que tenants diferentes tenham seus proprios cadastros sem colisao global indevida.

## Classificacao
Decisao arquitetural multitenant aplicada a regra de negocio.

## Escopo
Tenant / Tutor.

## Gatilho
Persistencia de atributos com unicidade local ao tenant.

## Resultado esperado
CPF duplicado dentro do tenant e rejeitado; mesmo CPF em tenants diferentes e aceito.

## Violacao
Indice unico rejeita duplicidade dentro do tenant.

## Excecoes
Nenhuma conhecida.

## Fonte
ADR-0001; `docs/domain/tutores-e-animais.md`.

## Data de validacao
2026-07-21.

## Implementacao
Indice `ix_tutores_tenant_id_documento`.

## Evidencia automatizada
`TutoresPersistenceTests.CpfIgual_EhPermitidoEmTenantsDiferentes`; `TutoresPersistenceTests.CpfIgual_NoMesmoTenant_EhRejeitadoPeloBanco`; `TutoresApiTests.CadastrarTutor_ComCpfDuplicado_RespeitaEscopoDoTenant`.

## Status
Vigente.

## Reavaliacao
Reavaliar quando houver identificadores externos, microchip ou outros campos unicos.

# BR-TEN-006 - Fluxos persistentes devem comprovar isolamento com dois tenants

## Regra
Funcionalidades persistentes de Tutores e Animais devem possuir evidencia automatizada com pelo menos dois tenants quando houver risco de acesso cross-tenant.

## Motivacao
Transformar isolamento multitenant em propriedade verificavel, nao apenas declaracao arquitetural.

## Classificacao
Decisao arquitetural multitenant.

## Escopo
Tenant / testes.

## Gatilho
Criacao ou alteracao de fluxo persistente tenant-owned.

## Resultado esperado
Testes exercitam tenant A e tenant B em leitura, escrita, filtros, duplicidade ou associacao.

## Violacao
Mudanca com risco multitenant sem teste deve ser bloqueada ou registrada como lacuna explicita.

## Excecoes
Nenhuma conhecida para dados de negocio persistidos.

## Fonte
ADR-0001; AGENTS.md.

## Data de validacao
2026-07-21.

## Implementacao
Suites `PetShop.IntegrationTests` e `PetShop.UnitTests` conforme tipo de regra.

## Evidencia automatizada
`TutoresPersistenceTests`; `AnimaisPersistenceTests`; `TutoresApiTests`; `AnimaisApiTests`; `JwtBearerDiagnosticsAuthorizationTests`.

## Status
Vigente.

## Reavaliacao
Reavaliar com jobs, eventos, cache, importacao, exportacao ou modulos novos.
