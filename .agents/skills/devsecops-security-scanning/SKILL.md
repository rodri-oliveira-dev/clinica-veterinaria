---
name: devsecops-security-scanning
description: Use esta skill para evoluir os controles de segurança no CI/CD, incluindo Gitleaks, auditoria NuGet, dependency review, CodeQL, SonarCloud, Trivy e OWASP ZAP. Adicione ferramentas por fase e evite scanners redundantes ou workflows que dependam de infraestrutura inexistente.
license: Apache-2.0
origin:
  repository: mukul975/Anthropic-Cybersecurity-Skills
  commit: 673da1f3b0b7be34ffc9624ef3858fe45f1c3bed
  path: skills/implementing-devsecops-security-scanning/SKILL.md
modified: true
---

# Objetivo

Manter um pipeline DevSecOps proporcional ao estágio do projeto, com gates claros, baixo privilégio e feedback reproduzível.

Esta adaptação substitui a combinação genérica Semgrep/Trivy/ZAP/Gitleaks por uma estratégia específica para o repositório: CodeQL e SonarCloud para análise estática, auditoria NuGet e dependency review para dependências, Gitleaks para segredos, e inclusão posterior de Trivy e ZAP somente quando existirem imagem e ambiente executável.

## Controles atuais

| Controle | Arquivo | Estado |
|---|---|---|
| Restore, auditoria NuGet, build e testes | `.github/workflows/dotnet.yml` | ativo |
| Dependency review | `.github/workflows/dependency-review.yml` | ativo |
| CodeQL C# | `.github/workflows/codeql.yml` | ativo |
| Gitleaks | `.github/workflows/gitleaks.yml` | ativo |
| SonarCloud | `.github/workflows/sonarcloud.yml` | opt-in por configuração |

Não adicione Semgrep apenas para duplicar CodeQL e SonarCloud. Uma ferramenta adicional exige gap demonstrável, regra específica ou cobertura de linguagem que os controles atuais não forneçam.

## Fases

### Fase 1 — proteção do código-fonte

Obrigatória desde o início:

- Gitleaks;
- restore com auditoria NuGet;
- dependency review em pull requests;
- build e testes;
- CodeQL;
- SonarCloud após cadastrar o projeto e o token.

### Fase 2 — frontend e supply chain ampliada

Quando React/TypeScript forem introduzidos:

- lockfile obrigatório;
- auditoria do gerenciador de pacotes;
- dependency review cobrindo npm/pnpm;
- análise TypeScript no SonarCloud ou CodeQL;
- geração de SBOM por release quando houver artefato definido.

### Fase 3 — containers

Somente quando existir `Dockerfile` e imagem oficial:

- Trivy em filesystem durante PR;
- Trivy na imagem final;
- scan de configuração do container;
- SBOM CycloneDX do artefato;
- gate para vulnerabilidades críticas, com política documentada para altas.

Não crie job que tenta construir uma imagem inexistente.

### Fase 4 — DAST

Somente quando houver ambiente de teste estável, URL conhecida e autorização:

- ZAP baseline em PR ou pós-deploy;
- full scan agendado, fora do caminho crítico;
- regras revisadas, sem ignorar alertas apenas para fazer o job passar;
- autenticação de teste e dados sintéticos quando necessário.

DAST não substitui testes de autorização multitenant nem revisão de lógica de negócio.

## SonarCloud

O workflow é deliberadamente opt-in para não falhar antes do provisionamento.

Configure no GitHub:

### Repository variables

```text
SONARCLOUD_ENABLED=true
SONAR_PROJECT_KEY=<project-key>
SONAR_ORGANIZATION=<organization-key>
SONAR_HOST_URL=https://sonarcloud.io
```

`SONAR_HOST_URL` pode ser ajustada conforme a região escolhida no SonarQube Cloud.

### Repository secret

```text
SONAR_TOKEN=<token restrito ao projeto ou organização>
```

O workflow:

- ignora PRs de forks, porque secrets não são disponibilizados com segurança;
- usa SonarScanner for .NET com versão explícita;
- executa restore, build e testes dentro do bloco de análise;
- importa relatórios OpenCover existentes em `TestResults`;
- espera o Quality Gate;
- não faz deploy nem publica artefatos.

## Estratégia de gates

| Achado | Comportamento inicial |
|---|---|
| Segredo | bloqueante; revogar/rotacionar imediatamente |
| Dependência crítica ou alta conhecida | bloqueante quando houver correção viável; exceção formal se inevitável |
| Dependency review moderada ou superior | segue o gate atual do repositório |
| CodeQL alto/crítico | bloqueante após triagem |
| Sonar Quality Gate | bloqueante quando SonarCloud estiver habilitado |
| Vulnerabilidade de container crítica | bloqueante quando scan de imagem existir |
| DAST | inicialmente informativo; tornar bloqueante por regra validada |

Não reduza severidade, threshold ou cobertura silenciosamente para liberar merge.

## Processo de alteração do pipeline

1. Leia `AGENTS.md` e `.agents/skills/ci-release-governance/SKILL.md`.
2. Identifique o risco que o novo controle deve detectar.
3. Verifique se um controle atual já cobre o cenário.
4. Defina trigger, permissões, timeout, custo e comportamento em forks.
5. Fixe actions externas por commit SHA.
6. Use secrets somente em eventos seguros.
7. Evite `pull_request_target` com checkout de código do PR.
8. Faça o comando local equivaler ao CI quando possível.
9. Diferencie análise informativa de gate bloqueante.
10. Documente pré-requisitos e procedimento de exceção.

## Resultados e artifacts

- Não publique código-fonte ou configuração sensível como artifact.
- Relatórios devem ocultar segredos e PII.
- Use SARIF apenas quando a integração representar corretamente o risco.
- Findings devem indicar ferramenta, regra, severidade, arquivo e ação esperada.
- Não publique tokens, payloads clínicos, connection strings ou conteúdo de documentos.

## Exceções

Uma exceção de segurança deve registrar:

- finding e ferramenta;
- justificativa técnica;
- impacto e exposição;
- compensating controls;
- owner;
- prazo de revisão;
- issue ou decisão associada.

Suppressions no código devem ser específicas e comentadas. Exceções globais ou por diretório exigem revisão mais rigorosa.

## Checklist

- A ferramenta cobre um gap real?
- O workflow tem permissões mínimas?
- Actions estão fixadas por SHA?
- PR de fork não recebe secret?
- O job funciona sem infraestrutura ainda inexistente?
- O Quality Gate está claro?
- Falsos positivos possuem processo de triagem?
- Findings evitam dados pessoais e segredos?
- Há comando local ou documentação reproduzível?
- Branch protection será atualizada quando o check estiver estável?

## Restrições

- Não adicionar scanners redundantes sem justificativa.
- Não executar DAST contra produção por padrão.
- Não criar container scan antes da imagem oficial.
- Não expor secrets a código não confiável.
- Não transformar todo warning em gate sem período de calibração.
- Não remover CodeQL, dependency review ou auditoria NuGet ao habilitar SonarCloud.
