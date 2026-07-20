---
name: privacy-rights-workflow
description: Use esta skill ao criar ou revisar fluxos para solicitações de titulares de dados pessoais, incluindo confirmação, acesso, correção, informação de compartilhamento, revogação de consentimento, oposição, anonimização, bloqueio, eliminação, portabilidade e revisão de decisão automatizada. Aplique verificação de identidade, isolamento multitenant, análise de retenção, auditoria e execução segura. Não use como endpoint público de exclusão irrestrita nem para prometer atendimento automático sem revisão das exceções legais e regulatórias.
---

# Workflow de direitos dos titulares

## Objetivo

Implementar solicitações de titulares como casos de uso rastreáveis, seguros e revisáveis, evitando exportação indevida, exclusão de registros obrigatórios ou confirmação da existência de dados de outro tenant.

A clínica normalmente será a controladora dos dados de tutores e atendimentos. A plataforma tende a atuar como operadora nesse contexto e deve fornecer mecanismos para que a clínica execute sua decisão. O papel concreto deve ser validado para cada tratamento.

## Quando usar

Use esta skill para:

- portal ou API de privacidade;
- atendimento manual pelo encarregado ou suporte autorizado;
- exportação de dados pessoais;
- correção de cadastro;
- revogação de consentimento e preferências;
- anonimização, bloqueio ou eliminação;
- informação sobre compartilhamentos;
- portabilidade;
- revisão de decisões automatizadas;
- encerramento de tenant com exportação ou descarte.

## Princípios obrigatórios

- Uma solicitação não autoriza acesso indiscriminado a todos os dados do tenant.
- A identidade do solicitante deve ser verificada de forma proporcional ao risco.
- O usuário autenticado não pode escolher o tenant da solicitação.
- A existência de dados de outro tenant não deve ser revelada.
- Exclusão não é sinônimo de hard delete.
- Obrigações de guarda, defesa de direitos, auditoria e documentação profissional precisam ser avaliadas antes da disposição.
- Toda decisão deve registrar responsável, fundamento, data e evidência.

## Modelo de estados

Use estados equivalentes a:

```text
Received
IdentityVerificationPending
ScopeAnalysis
RetentionReview
Approved
PartiallyApproved
Rejected
ExecutionPending
Completed
Cancelled
```

Não exponha nomes internos diretamente ao titular quando isso revelar investigação, retenção legal ou controles de segurança.

## Processo obrigatório

### 1. Receba a solicitação

Registre no mínimo:

- tenant responsável;
- canal de origem;
- tipo de solicitação;
- data e hora;
- dados de contato fornecidos;
- identificador de correlação;
- texto original, protegido e com acesso restrito;
- prazo operacional calculado fora de regra fixa de domínio quando depender de norma vigente.

Não registre documentos de identidade em logs comuns.

### 2. Verifique identidade e autoridade

A verificação deve ser proporcional ao risco do resultado.

Considere:

- sessão autenticada existente;
- confirmação por canal previamente cadastrado;
- desafio adicional para exportação ou alteração sensível;
- representante legal ou procurador;
- relação entre tutor, responsável financeiro, pessoa autorizada e animal;
- conflito entre múltiplos responsáveis.

Não solicite documento excessivo quando uma verificação menos invasiva for suficiente.

### 3. Determine o controlador e o tenant

- Resolva o tenant por contexto confiável, nunca por body, rota, query ou header recebido do solicitante.
- Confirme se a plataforma deve executar diretamente, encaminhar à clínica ou apenas disponibilizar ferramentas.
- Para solicitações recebidas pela plataforma sobre dados controlados pela clínica, registre o encaminhamento e preserve o SLA contratual.
- Operação administrativa cross-tenant exige autorização específica e auditoria reforçada.

### 4. Levante o escopo

Mapeie dados em:

- cadastro de pessoa e vínculos com animais;
- agendamentos e comunicações;
- atendimento e documentos;
- cobrança e pagamentos;
- anexos e arquivos;
- preferências e consentimentos;
- usuários e profissionais;
- integrações e fornecedores;
- logs de auditoria relevantes;
- backups conforme política documentada.

Não confunda busca textual parcial com inventário completo. O escopo deve ser definido por ownership e catálogo de dados.

### 5. Analise retenção e restrições

Classifique cada conjunto encontrado:

- pode ser corrigido;
- pode ser exportado;
- pode ser anonimizável;
- pode ser eliminado;
- deve ser bloqueado;
- deve ser preservado por obrigação;
- deve ser preservado para exercício de direitos;
- contém dados de terceiros que precisam de proteção ou redação.

Prontuários e documentos veterinários podem exigir preservação de autoria, integridade, cronologia e correções rastreáveis. Não apague silenciosamente conteúdo clínico ou assinatura.

### 6. Registre a decisão

A decisão deve indicar:

- escopo aprovado;
- itens parcialmente atendidos;
- restrições e fundamentos;
- aprovador autorizado;
- data;
- execução planejada;
- comunicação a fornecedores ou suboperadores;
- risco residual.

Não grave parecer jurídico extenso em logs técnicos. Mantenha o documento de decisão em armazenamento apropriado.

### 7. Execute de forma segura

#### Confirmação e acesso

- Retorne apenas dados do titular verificado e do tenant correto.
- Proteja dados de terceiros.
- Diferencie resposta simplificada de exportação completa quando aplicável.

#### Correção

- Preserve histórico quando o registro exigir rastreabilidade.
- Não permita alterar autoria, tenant ou documento finalizado como atualização comum.
- Propague correções a projeções e integrações quando necessário.

#### Revogação de consentimento

- Registre finalidade, canal, versão, data e origem.
- Interrompa apenas tratamentos dependentes daquele consentimento.
- Não interrompa automaticamente comunicações necessárias à execução do serviço quando possuírem outro fundamento validado.
- Preserve evidência da concessão e da revogação conforme política de retenção.

#### Anonimização

- Remova ou transforme identificadores diretos e indiretos suficientes para impedir reidentificação razoável.
- Considere relações, anexos, texto livre, histórico e índices de busca.
- Substituir nome por `ANONIMIZADO` mantendo CPF, telefone ou vínculos não é anonimização.

#### Bloqueio

- Impeça tratamentos não autorizados, mantendo os dados preservados pelo motivo documentado.
- Garanta que jobs, relatórios e campanhas também respeitem o bloqueio.

#### Eliminação

- Use hard delete somente quando a categoria permitir e não houver obrigação de preservação.
- Remova cópias derivadas, arquivos temporários, caches e índices conforme política.
- Documente o comportamento de backups e restaurações.

#### Portabilidade

- Use formato estruturado e interoperável quando aplicável.
- Não inclua segredos comerciais, dados de terceiros ou informações fora do escopo regulamentado.
- Evite prometer formato definitivo antes da regulamentação aplicável.

#### Decisão automatizada

- Registre o sistema, a decisão e os critérios explicáveis disponíveis.
- Permita encaminhamento para revisão humana autorizada.
- Preserve segurança e segredo comercial sem inviabilizar transparência adequada.

### 8. Gere exportação protegida

A exportação deve:

- exigir permissão específica;
- executar no tenant correto;
- usar processamento assíncrono quando o volume justificar;
- gerar arquivo com nome sem dados pessoais;
- ser armazenada fora de área pública;
- utilizar link temporário ou entrega autenticada;
- possuir expiração curta;
- ser criptografada quando o risco justificar;
- registrar geração, acesso, download e expiração;
- impedir enumeração e descoberta por outro tenant.

Não envie exportações completas por e-mail aberto sem avaliação de risco.

### 9. Trate fornecedores

Quando dados tiverem sido compartilhados:

- identifique destinatários relevantes;
- registre solicitações de correção, bloqueio ou eliminação enviadas;
- acompanhe retorno e falhas;
- evite excluir dados que o fornecedor precisa reter por obrigação própria sem análise;
- preserve evidência de comunicação.

### 10. Comunique e encerre

A resposta deve informar de maneira clara:

- o que foi atendido;
- o que foi atendido parcialmente;
- o que não pôde ser executado e por quê, em linguagem apropriada;
- como acessar eventual exportação;
- canal para contestação ou complemento.

Registre a conclusão sem expor detalhes de segurança internos.

## Modelo mínimo sugerido

```csharp
public sealed class DataSubjectRequest
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid? PersonId { get; init; }
    public DataSubjectRequestType Type { get; init; }
    public DataSubjectRequestStatus Status { get; private set; }
    public DateTimeOffset ReceivedAt { get; init; }
    public DateTimeOffset? IdentityVerifiedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string? DecisionCode { get; private set; }
    public string? RetentionRestrictionCode { get; private set; }
}
```

O modelo é ilustrativo. Não armazene documentos, pareceres ou justificativas extensas diretamente em campos de log ou status.

## Permissões sugeridas

Use capacidades separadas, por exemplo:

```text
privacy.requests.create
privacy.requests.read
privacy.requests.verify-identity
privacy.requests.review-retention
privacy.requests.approve
privacy.requests.execute
privacy.exports.generate
privacy.exports.download
privacy.requests.audit
```

Não concentre tudo em uma permissão administrativa genérica.

## Testes mínimos

1. Titular verificado acessa somente seus dados no tenant correto.
2. Tenant B não descobre solicitação ou exportação do Tenant A.
3. Identificador informado pelo cliente não substitui o tenant autenticado.
4. Exportação expirada não pode ser baixada.
5. Usuário sem permissão não aprova nem executa solicitação.
6. Dados de terceiros são omitidos ou protegidos.
7. Retenção obrigatória impede eliminação e produz decisão auditável.
8. Revogação afeta apenas a finalidade correspondente.
9. Anonimização remove identificadores diretos e indiretos previstos.
10. Retry não gera exportações duplicadas ou ações destrutivas repetidas.

## Sinais de risco

- endpoint `DELETE /person/{id}` como atendimento integral da LGPD;
- exportação síncrona com URL permanente;
- solicitação resolvida apenas por e-mail sem verificação;
- uso do `tenant_id` recebido no formulário;
- exclusão de prontuário ou auditoria;
- anonimização apenas do nome;
- exportação contendo dados de outros responsáveis;
- consentimento revogado sem finalidade identificada;
- suporte podendo aprovar e executar sem segregação;
- prazo regulatório codificado sem governança de atualização.

## Saída esperada

Ao concluir, informe:

- tipo de solicitação e identidade verificada;
- controlador e tenant responsáveis;
- sistemas e categorias de dados abrangidos;
- retenções e restrições encontradas;
- operação executada;
- exportações e fornecedores envolvidos;
- trilha de auditoria criada;
- testes de autorização e isolamento;
- validações jurídicas ou regulatórias pendentes.

## Referências oficiais

Data de consulta da baseline: 20 de julho de 2026.

- LGPD consolidada, especialmente arts. 17 a 22 e 37 a 40: https://www.planalto.gov.br/ccivil_03/_ato2015-2018/2018/lei/l13709compilado.htm
- ANPD — petição de titular: https://www.gov.br/anpd/pt-br/canais_atendimento/cidadao-titular-de-dados/peticao-de-titular-contra-controlador-de-dados
- ANPD — regulamentações: https://www.gov.br/anpd/pt-br/acesso-a-informacao/institucional/atos-normativos/regulamentacoes_anpd
- Documentação veterinária e retenção devem ser validadas com as normas vigentes do CFMV e com o contexto registrado em `PO_Virtual_Plataforma_Clinica_Veterinaria.md`.
