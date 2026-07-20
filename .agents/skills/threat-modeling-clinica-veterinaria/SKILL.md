---
name: threat-modeling-clinica-veterinaria
description: Use esta skill para criar ou revisar modelos de ameaça de funcionalidades, integrações e mudanças arquiteturais da plataforma. Aplica STRIDE e, quando houver dados pessoais, LINDDUN, com foco obrigatório em isolamento multitenant. Não use para executar pentest ativo.
license: Apache-2.0
origin:
  repository: mukul975/Anthropic-Cybersecurity-Skills
  commit: 673da1f3b0b7be34ffc9624ef3858fe45f1c3bed
  path: skills/performing-threat-modeling-with-owasp-threat-dragon/SKILL.md
modified: true
---

# Objetivo

Produzir modelos de ameaça pequenos, versionáveis e acionáveis para o monólito modular, sem tornar o OWASP Threat Dragon uma dependência obrigatória.

Esta é uma adaptação da skill de origem. Foram alterados escopo, linguagem, ferramentas, exemplos e critérios para refletir ASP.NET Core, Keycloak, PostgreSQL, React e o isolamento multitenant deste projeto.

## Quando usar

- Nova jornada com dados pessoais, clínicos, financeiros ou documentos.
- Nova integração externa, upload, exportação, notificação ou processamento assíncrono.
- Mudança em autenticação, autorização, Keycloak, permissões ou suporte administrativo.
- Alteração no limite de tenant, unidade, módulo ou trust boundary.
- Funcionalidade de alto risco, como prontuário, prescrição, cobrança, estorno ou acesso cross-tenant.
- Revisão arquitetural antes de implementar uma fatia relevante.

## Fontes obrigatórias

Leia somente o necessário:

1. `AGENTS.md`;
2. ADRs relacionadas;
3. contratos, módulos e fluxos afetados;
4. `.agents/skills/multitenancy-dotnet/SKILL.md`;
5. requisitos funcionais e regulatórios relacionados.

## Formato da análise

O artefato deve ser criado em:

```text
docs/security/threat-models/<capacidade-ou-fluxo>.md
```

Use Markdown e Mermaid por padrão. Use o formato JSON do OWASP Threat Dragon somente quando a equipe já estiver utilizando a ferramenta. Não instale nem execute Threat Dragon sem necessidade explícita.

## Processo

### 1. Defina o escopo

Registre:

- objetivo da funcionalidade;
- atores humanos e sistemas externos;
- dados e documentos processados;
- operações críticas;
- componentes e módulos envolvidos;
- dependências externas;
- o que está fora do modelo.

### 2. Identifique ativos

Considere ao menos:

- identidade e sessão;
- claim validada `tenant_id`;
- dados de tutores e responsáveis;
- dados e documentos do animal;
- prontuário e autoria profissional;
- agenda e disponibilidade;
- pagamentos e estornos;
- anexos e exportações;
- logs, traces e auditoria;
- segredos, tokens e configurações.

### 3. Desenhe fluxos e fronteiras de confiança

Inclua, quando existirem:

- navegador ou aplicativo;
- frontend React;
- API ASP.NET Core;
- Keycloak;
- módulos internos;
- PostgreSQL;
- storage de documentos;
- provedores de mensagem, pagamento ou comunicação;
- usuários administrativos e suporte.

Diferencie claramente fronteiras entre:

- internet e aplicação;
- Keycloak e API;
- tenant autenticado e dados persistidos;
- usuário comum e operação administrativa cross-tenant;
- módulo proprietário do dado e consumidores internos;
- aplicação e fornecedor externo.

### 4. Aplique STRIDE

Avalie por elemento e fluxo:

| Categoria | Pergunta mínima |
|---|---|
| Spoofing | Uma identidade, sessão, tenant, profissional ou integração pode ser falsificada? |
| Tampering | Dados, claims, documentos, estados ou valores financeiros podem ser alterados indevidamente? |
| Repudiation | A autoria e a sequência das ações críticas podem ser demonstradas? |
| Information Disclosure | Há retorno excessivo, enumeração de recursos, log sensível ou vazamento cross-tenant? |
| Denial of Service | Uploads, consultas, relatórios ou integrações podem esgotar recursos? |
| Elevation of Privilege | Um papel pode executar ato reservado, acessar outro tenant ou ampliar permissões? |

### 5. Aplique LINDDUN quando houver dados pessoais

Avalie linkabilidade, identificabilidade, detectabilidade, divulgação, falta de transparência e não conformidade. Considere especialmente exportações, campanhas, observabilidade, suporte e integrações.

### 6. Execute o checklist multitenant obrigatório

- O tenant vem exclusivamente da identidade autenticada?
- Body, rota, query e headers não são tratados como autoridade de tenant?
- IDs externos podem ser usados para consultar ou associar registros de outro tenant?
- Listagens, busca, paginação, contagens e erros evitam confirmar a existência de dados externos?
- Índices únicos e relacionamentos respeitam o limite do tenant?
- Jobs, cache, idempotência, exportações e notificações preservam tenant?
- Operações cross-tenant têm autorização e auditoria próprias?
- Existem testes com tenants A e B?

### 7. Classifique e trate ameaças

Cada ameaça deve registrar:

```markdown
## THR-000 — Título

- Categoria: STRIDE/LINDDUN
- Cenário:
- Ativo afetado:
- Pré-condições:
- Impacto:
- Probabilidade: baixa | média | alta
- Severidade: baixa | média | alta | crítica
- Controles existentes:
- Mitigação proposta:
- Evidência/teste esperado:
- Owner:
- Estado: aberta | mitigada | aceita | não aplicável
```

Não marque uma ameaça como mitigada apenas porque existe autenticação. Exija evidência verificável.

## Ameaças prioritárias deste projeto

Sempre considere:

- BOLA/IDOR entre tenants;
- spoofing de `tenant_id` em body, query, rota ou header;
- associação de registros pertencentes a tenants diferentes;
- autorização baseada apenas em visibilidade da interface;
- mass assignment de tenant, papel, autoria, status ou valor financeiro;
- retorno excessivo de entidades e propriedades internas;
- exposição de tokens, PII ou documentos em logs e traces;
- upload malicioso e acesso indevido a anexos;
- alteração ou exclusão silenciosa de histórico clínico;
- notificações enviadas ao destinatário ou tenant incorreto;
- suporte administrativo sem escopo, prazo ou auditoria;
- confiança indevida em roles ou audiences do Keycloak.

## Critério de conclusão

Um modelo está pronto quando:

- o fluxo e as trust boundaries estão compreensíveis;
- ameaças relevantes possuem classificação e owner;
- mitigações estão ligadas a requisitos ou testes;
- riscos aceitos possuem justificativa explícita;
- questões não respondidas estão registradas;
- o documento pode ser revisado sem depender de ferramenta proprietária.

## Restrições

- Não executar ataques contra ambientes sem autorização explícita.
- Não introduzir microsserviços, gateway, broker ou WAF como mitigação automática.
- Não transformar checklist em prova de segurança.
- Não registrar tokens, segredos ou dados pessoais reais no modelo.
- Não assumir que Threat Dragon, STRIDE ou LINDDUN substituem revisão especializada.
