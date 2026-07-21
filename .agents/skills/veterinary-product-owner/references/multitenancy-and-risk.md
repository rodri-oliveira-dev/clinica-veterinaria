# Multitenancy, permissões, auditoria, risco e priorização

## Multitenancy como requisito de produto

O tenant representa a clínica ou organização usuária da plataforma. Toda funcionalidade persistente deve declarar ownership e isolamento.

Para cada item de backlog, responda:

- quem é o tenant owner da informação;
- se a informação pertence ao tenant inteiro ou a uma unidade;
- se existe compartilhamento legítimo entre unidades;
- se há algum cenário cross-tenant permitido;
- quais perfis podem executar cada operação;
- como impedir associação de dados de tenants diferentes;
- o que aparece em relatórios, notificações, exportações e auditoria;
- como jobs e processamentos preservam o tenant.

## Regras arquiteturais vigentes

- O tenant autenticado vem exclusivamente da claim validada `tenant_id`.
- Body, rota, query string ou header não são autoridade de tenant em operações comuns.
- Não existe tenant padrão nem fallback silencioso.
- Tabelas funcionais persistentes possuem `tenant_id NOT NULL`.
- Índices únicos locais incluem o tenant.
- Relações tenant-owned não podem cruzar tenants.
- O tenant de um registro não é alterado como atualização comum.
- Operações administrativas cross-tenant exigem autorização e auditoria específicas.
- Funcionalidades persistentes devem possuir testes com pelo menos dois tenants.

A PO deve transformar essas regras em critérios de aceite funcionais, sem prescrever o mecanismo técnico.

## Cenários mínimos de isolamento

```gherkin
Cenário: usuário não visualiza registros de outro tenant
  Dado que existe um registro pertencente ao tenant A
  E o usuário autenticado pertence ao tenant B
  Quando consultar a coleção ou tentar acessar o registro diretamente
  Então o registro do tenant A não deve ser retornado
  E nenhuma informação que confirme sua existência deve ser exposta indevidamente

Cenário: usuário não altera registros de outro tenant
  Dado que existe um registro pertencente ao tenant A
  E o usuário autenticado pertence ao tenant B
  Quando tentar alterar ou excluir o registro
  Então a operação deve ser rejeitada de forma segura
  E o registro original deve permanecer inalterado

Cenário: associação entre tenants é impedida
  Dado que o usuário pertence ao tenant A
  E informa o identificador de um registro pertencente ao tenant B
  Quando tentar criar uma relação entre os registros
  Então a operação deve ser rejeitada
```

O código HTTP e a estratégia de não revelar existência são decisões de arquitetura e segurança.

## Permissões

Para cada operação, diferencie:

- quem pode visualizar;
- quem pode criar;
- quem pode alterar;
- quem pode cancelar, inativar ou excluir;
- quem pode autorizar exceções;
- quem pode exportar;
- quem pode acessar dados clínicos ou financeiros;
- qual unidade ou escopo organizacional se aplica.

Não use papéis de sistema para autorizar atos profissionais que exigem credencial, responsabilidade técnica ou validação clínica.

## Auditoria

Avalie auditoria quando houver:

- alteração de autoria ou histórico;
- consentimento, recusa ou autorização;
- acesso a prontuário;
- alteração ou exportação de dados pessoais;
- cobrança, estorno, desconto e cancelamento;
- prescrição e substância controlada;
- operação administrativa cross-tenant;
- acesso temporário de suporte;
- ações irreversíveis ou de alto impacto.

Registre quem, quando, em qual tenant, qual operação e qual resultado. Não copie tokens, claims completas, documentos pessoais ou conteúdo sensível sem necessidade.

## Matriz de risco

Classifique a funcionalidade como baixo, médio ou alto risco nos eixos:

- clínico;
- privacidade;
- financeiro;
- regulatório;
- segurança;
- reputação;
- continuidade operacional;
- isolamento multitenant.

Exemplos normalmente de maior risco:

- prontuário;
- prescrição;
- substâncias controladas;
- cirurgia e anestesia;
- internação;
- consentimento;
- acesso cross-tenant;
- exportação e exclusão de dados;
- cobrança e estorno;
- telemedicina;
- alteração de autoria ou histórico clínico.

Funcionalidades de alto risco exigem discovery mais profundo, revisão especializada e critérios de aceite adicionais.

## Priorização

Priorize por combinação de:

1. valor operacional;
2. valor para o usuário;
3. risco reduzido ou evitado;
4. aprendizado gerado;
5. frequência do problema;
6. abrangência de papéis e tenants;
7. dependências desbloqueadas;
8. esforço e complexidade avaliados com o time.

Classificação sugerida:

- **P0 — Segurança ou obrigação crítica:** isolamento, perda de dados, acesso indevido ou obrigação bloqueante.
- **P1 — Jornada central:** necessária para executar a proposta principal do produto.
- **P2 — Eficiência operacional:** reduz trabalho, erros ou retrabalho.
- **P3 — Otimização:** melhora conveniência sem bloquear a operação.

Compliance não implica automaticamente P0. Avalie aplicabilidade, prazo, risco e alternativas operacionais.

## Observabilidade funcional

Quando útil, defina indicadores como:

- taxa de conclusão da jornada;
- cancelamentos e no-show;
- conflitos e rejeições;
- tempo até atendimento;
- retrabalho e correções;
- falhas de comunicação;
- estornos e divergências financeiras;
- tentativas de acesso negadas;
- hipóteses validadas ou refutadas.

Não use dados pessoais nem identificadores de alta cardinalidade como labels de métricas.