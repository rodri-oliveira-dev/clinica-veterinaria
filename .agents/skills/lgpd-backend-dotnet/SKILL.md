---
name: lgpd-backend-dotnet
description: Use esta skill ao criar ou revisar funcionalidades de backend .NET que tratem dados pessoais, dados vinculados a tutores, profissionais, usuários, pagamentos, documentos, anexos, notificações, integrações, exportações, retenção, suporte ou incidentes. Aplique privacy by design, minimização, autorização, isolamento multitenant e rastreabilidade. Não use para emitir parecer jurídico nem para escolher uma base legal sem validação do controlador ou responsável por privacidade.
---

# LGPD aplicada ao backend .NET

## Objetivo

Aplicar requisitos de proteção de dados pessoais desde a concepção das funcionalidades do backend da plataforma de clínicas veterinárias.

Esta skill traduz riscos de privacidade em decisões verificáveis de produto, arquitetura, implementação e testes. Ela não substitui avaliação jurídica, DPO, encarregado, responsável técnico ou validação regulatória.

## Escopo de dados

A LGPD protege dados de pessoas naturais. No produto, considere especialmente:

- tutores, responsáveis financeiros e contatos de emergência;
- médicos-veterinários, funcionários, prestadores e usuários;
- representantes de associações, abrigos ou empresas;
- dados de pagamento, comunicação, autenticação e auditoria;
- documentos e anexos que identifiquem ou permitam identificar uma pessoa.

Dados clínicos do animal não são automaticamente dados pessoais sensíveis da LGPD. Ainda assim, quando vinculados a tutor, cobrança, endereço, documento ou prontuário, devem receber proteção compatível com confidencialidade, segurança, contrato e normas veterinárias.

## Quando usar

Use esta skill ao trabalhar com:

- novos endpoints, commands, queries ou DTOs;
- entidades e tabelas contendo dados de pessoas;
- cadastro de tutor, responsável, profissional ou usuário;
- prontuários, documentos, consentimentos e anexos;
- notificações operacionais ou campanhas;
- integrações com e-mail, SMS, WhatsApp, pagamento, analytics ou suporte;
- importação, exportação, relatórios ou compartilhamento;
- retenção, anonimização, exclusão ou bloqueio;
- suporte administrativo e acesso cross-tenant;
- logs, traces, métricas, auditoria e resposta a incidentes.

## Quando não usar

- Mudança puramente visual sem tratamento de dados.
- Código técnico sem dados funcionais, identificadores pessoais ou efeito sobre acesso.
- Para declarar conformidade jurídica definitiva.
- Para definir automaticamente base legal, prazo regulatório ou hipótese de retenção sem validação adequada.

## Processo obrigatório

### 1. Identifique o tratamento

Antes de implementar, responda:

- Qual dado pessoal será coletado, consultado, alterado, compartilhado ou removido?
- Quem é o titular?
- Qual é a finalidade concreta?
- O dado é necessário para essa finalidade?
- A clínica atua como controladora e a plataforma como operadora neste fluxo?
- A plataforma possui finalidade própria que a torna controladora neste tratamento?
- Há fornecedor ou suboperador envolvido?
- Existe transferência internacional?
- Qual critério de retenção se aplica?

Quando finalidade, base legal ou retenção não estiverem definidas, registre a lacuna. Não invente consentimento para completar o fluxo.

### 2. Minimize coleta e exposição

- Não use entidade de persistência como request ou response público.
- Não colete campos para possível uso futuro.
- Não retorne CPF, endereço, telefone, conteúdo clínico ou documento completo em listagens quando dados mascarados forem suficientes.
- Restrinja campos livres, pois podem receber dados pessoais ou clínicos inesperados.
- Evite duplicar dados pessoais entre módulos.
- Prefira identificadores técnicos a dados pessoais em URLs, eventos e integrações.

### 3. Separe finalidades

Não modele um consentimento genérico para toda comunicação.

Diferencie, quando aplicável:

- confirmação de agendamento;
- instrução de preparo;
- resultado disponível;
- atualização de internação;
- cobrança;
- lembrete de vacina ou retorno;
- campanha promocional;
- perfilamento ou analytics.

Comunicação operacional não deve ser tratada automaticamente como autorização para marketing.

### 4. Autorize no backend

A autenticação não substitui autorização.

Considere:

- tenant;
- unidade;
- papel e permissão;
- finalidade da operação;
- vínculo com o registro;
- estado do documento ou prontuário;
- segregação entre dados clínicos, administrativos e financeiros.

Permissões amplas como `admin` não devem permitir silenciosamente leitura de todo prontuário, exportação massiva ou acesso cross-tenant.

### 5. Preserve isolamento multitenant

Aplique também a skill `multitenancy-dotnet`.

- O tenant vem exclusivamente da claim validada `tenant_id`.
- Toda consulta e mutação tenant-owned deve aplicar o tenant antes de materializar dados.
- Arquivos, caches, jobs, eventos, exportações e índices de busca também precisam de escopo de tenant.
- Não revele a existência de registros de outro tenant.
- Suporte cross-tenant exige fluxo, autorização, justificativa, duração e auditoria específicos.
- Todo fluxo persistente relevante deve possuir testes com pelo menos dois tenants.

Falha de isolamento pode constituir incidente de segurança envolvendo dados pessoais.

### 6. Proteja persistência e transporte

- Use TLS em todos os ambientes.
- Armazene segredos fora do código e repositório.
- Criptografe backups e armazenamento quando suportado pela infraestrutura.
- Aplique privilégios mínimos ao banco, storage e serviços externos.
- Não copie dados reais para desenvolvimento ou testes sem processo excepcional aprovado.
- Use provedor especializado para cartão; evite armazenar dados completos de cartão.
- Para anexos, valide tamanho, tipo real, autorização de download e expiração de links assinados.

Criptografia de campo não é requisito automático. Use somente quando o modelo de ameaça justificar e houver gestão de chaves adequada.

### 7. Evite vazamento por observabilidade

Aplique também a skill `privacy-audit-logging`.

Não registre automaticamente:

- bodies integrais de request ou response;
- CPF, endereço, telefone ou e-mail completo;
- tokens, cookies, credenciais ou secrets;
- prontuários, prescrições, laudos ou anexos;
- conteúdo integral de mensagens;
- dados bancários ou de cartão.

Não use dados pessoais, `tenant_id`, `person_id` ou `customer_id` como labels de métricas de alta cardinalidade.

### 8. Defina retenção e disposição

Classifique cada categoria como:

- apagável;
- anonimizável;
- retida por obrigação;
- retida para exercício de direitos;
- bloqueada por disputa ou investigação;
- registro histórico que exige preservação e correção rastreável.

Não implemente hard delete universal.

Prontuários, documentos veterinários, registros fiscais, auditoria e evidências de consentimento podem possuir requisitos próprios. Registre a justificativa e valide o prazo antes de automatizar o descarte.

Backups não devem se tornar arquivo permanente. Documente como exclusões e anonimizações se comportam durante restauração.

### 9. Prepare direitos do titular

Quando a funcionalidade cria ou altera dados pessoais, verifique se o backend consegue apoiar:

- confirmação e acesso;
- correção;
- informação sobre compartilhamento;
- revogação de consentimento;
- oposição, quando aplicável;
- anonimização, bloqueio ou eliminação quando cabível;
- portabilidade conforme regulamentação aplicável;
- revisão de decisão automatizada, se existir.

Use a skill `privacy-rights-workflow` para implementar solicitações de titulares.

### 10. Revise integrações

Para cada fornecedor registre:

- dados enviados;
- finalidade;
- país ou região de processamento;
- retenção;
- subprocessadores;
- exclusão e exportação;
- procedimento de incidente;
- credenciais e escopo de acesso.

Envie apenas a informação mínima. Em notificações, prefira “há um resultado disponível” a incluir conteúdo clínico desnecessário.

### 11. Trate incidentes

A implementação deve permitir:

- detectar comportamento anormal;
- identificar tenants, titulares e categorias de dados afetados;
- preservar evidências sem ampliar a exposição;
- revogar credenciais e sessões;
- bloquear integrações comprometidas;
- registrar linha do tempo e medidas de contenção;
- informar rapidamente o controlador quando a plataforma atuar como operadora.

Não codifique prazo de comunicação como regra imutável. Mantenha-o configurável e validado conforme regulamentação vigente da ANPD.

## Checklist de mudança

### Novo endpoint

- Há dados pessoais no input ou output?
- Todos os campos são necessários?
- O output usa mascaramento quando possível?
- A autorização considera tenant, papel e finalidade?
- Logs e traces omitem conteúdo pessoal?
- Há teste de acesso indevido e cross-tenant?

### Nova entidade ou tabela

- Quais campos são dados pessoais?
- A finalidade está documentada?
- Existe critério de retenção?
- O tenant é obrigatório quando o dado for tenant-owned?
- Exclusão, anonimização e auditoria foram avaliadas?
- Índices e buscas expõem dados desnecessários?

### Nova integração

- Quais dados deixam a plataforma?
- O fornecedor precisa realmente recebê-los?
- Existe suboperador ou transferência internacional?
- Falhas e retries podem duplicar dados?
- Há contrato, retenção e processo de incidente?

### Nova exportação

- Exige permissão específica?
- O escopo está limitado ao tenant e ao titular correto?
- O arquivo é protegido e expira?
- Geração e download são auditados?
- O nome do arquivo evita dados pessoais?

## Sinais de risco

Interrompa e revise quando encontrar:

- `Consent = true` sem finalidade, versão e evidência;
- retorno de entidade completa em API;
- logs com body ou prontuário;
- exportação acessível por URL permanente;
- suporte global compartilhado;
- hard delete de prontuário ou auditoria;
- analytics recebendo CPF, telefone, e-mail ou texto clínico;
- job sem tenant explícito;
- armazenamento de cartão completo;
- dados reais em ambiente não produtivo;
- base legal ou retenção escolhida apenas por conveniência técnica.

## Saída esperada

Ao concluir, informe:

- dados e titulares envolvidos;
- finalidade e papel provável dos agentes de tratamento;
- campos minimizados ou mascarados;
- controles de autorização e multitenancy;
- impacto em logs, auditoria, retenção e direitos do titular;
- integrações ou transferências afetadas;
- testes executados;
- decisões que ainda exigem jurídico, encarregado ou responsável técnico.

## Referências oficiais

Data de consulta da baseline: 20 de julho de 2026.

- LGPD consolidada: https://www.planalto.gov.br/ccivil_03/_ato2015-2018/2018/lei/l13709compilado.htm
- ANPD — agentes de tratamento e encarregado: https://www.gov.br/anpd/pt-br/centrais-de-conteudo/materiais-educativos-e-publicacoes/guia-orientativo-para-definicoes-dos-agentes-de-tratamento-de-dados-pessoais-e-do-encarregado
- ANPD — segurança para agentes de pequeno porte: https://www.gov.br/anpd/pt-br/centrais-de-conteudo/materiais-educativos-e-publicacoes/guia-orientativo-sobre-seguranca-da-informacao-para-agentes-de-tratamento-de-pequeno-porte
- ANPD — regulamentações: https://www.gov.br/anpd/pt-br/acesso-a-informacao/institucional/atos-normativos/regulamentacoes_anpd
- Contexto regulatório veterinário do projeto: `PO_Virtual_Plataforma_Clinica_Veterinaria.md`
