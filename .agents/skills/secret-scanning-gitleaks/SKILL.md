---
name: secret-scanning-gitleaks
description: Use esta skill para configurar, executar ou revisar detecção de segredos com Gitleaks no repositório e no GitHub Actions. Não use para armazenar, rotacionar ou recuperar credenciais.
license: Apache-2.0
origin:
  repository: mukul975/Anthropic-Cybersecurity-Skills
  commit: 673da1f3b0b7be34ffc9624ef3858fe45f1c3bed
  path: skills/implementing-secret-scanning-with-gitleaks/SKILL.md
modified: true
---

# Objetivo

Impedir que tokens, senhas, chaves privadas, connection strings e credenciais de fornecedores entrem no histórico Git.

Esta adaptação atualiza os comandos para a CLI moderna do Gitleaks, usa o padrão de GitHub Actions deste repositório, evita baseline automática e considera futuros segredos de Keycloak, PostgreSQL, storage e provedores de comunicação.

## Implementação deste repositório

- Configuração: `.gitleaks.toml`.
- CI: `.github/workflows/gitleaks.yml`.
- O workflow varre o histórico completo em pull requests, pushes na `main`, execução manual e agenda semanal.
- O repositório pertence a uma conta pessoal; a action v2 não exige `GITLEAKS_LICENSE` nesse cenário.

## Quando usar

- Ao alterar workflows, arquivos de configuração ou setup local.
- Antes de introduzir Keycloak, banco, storage, e-mail, SMS, WhatsApp ou pagamentos.
- Ao investigar credencial exposta.
- Ao criar exemplos, fixtures ou documentação que se pareçam com segredos.
- Ao revisar allowlists ou falso positivo.

## Execução local

Instale uma versão estável atual do Gitleaks e execute na raiz:

```bash
gitleaks git --redact --config .gitleaks.toml .
```

Para verificar somente os arquivos do diretório, sem histórico:

```bash
gitleaks dir --redact --config .gitleaks.toml .
```

Para mudanças staged, use o hook oficial ou uma integração local aprovada. Não adicione um hook que dependa silenciosamente de uma ferramenta ausente.

## Política de findings

Todo finding deve ser tratado como potencialmente real até verificação.

1. Não publique o valor encontrado em issue, PR, log ou chat.
2. Identifique o provedor e o ambiente.
3. Revogue ou rotacione a credencial antes de discutir limpeza do histórico.
4. Atualize os consumidores para usar secret store ou variáveis protegidas.
5. Avalie impacto e acessos realizados.
6. Remova o segredo do estado atual.
7. Reescreva histórico somente com coordenação explícita da equipe.
8. Adicione allowlist apenas quando houver evidência de falso positivo.

Remover o texto em um commit posterior não invalida uma credencial já publicada no histórico.

## Baseline

Não crie baseline por padrão neste projeto novo.

Uma baseline pode ser considerada apenas quando:

- existe histórico legado relevante;
- todos os findings foram triados;
- credenciais ativas foram rotacionadas;
- cada exceção possui owner e justificativa;
- há plano para reduzir a baseline.

Nunca use baseline para aceitar segredo ativo.

## Allowlist

- Prefira allowlist por fingerprint em `.gitleaksignore` para um finding específico.
- Evite excluir diretórios inteiros de testes ou documentação.
- Exemplos devem usar valores claramente inválidos e curtos, sem formato de credencial real.
- Não permita padrões genéricos como `password`, `token` ou `secret` globalmente.
- Revise qualquer exclusão em pull request.

## Segredos esperados fora do Git

Quando introduzidos, devem vir de configuração segura:

- `SONAR_TOKEN`;
- credenciais administrativas e client secrets do Keycloak;
- senha e connection string do PostgreSQL;
- chaves de assinatura ou certificados;
- credenciais de storage;
- tokens de provedores de e-mail, SMS ou WhatsApp;
- chaves de pagamento;
- tokens de observabilidade.

Valores públicos, como Authority do Keycloak, project key do SonarCloud ou nomes de realm/client, não são segredos por si só, mas não devem conter credenciais embutidas.

## GitHub Actions

- Use `fetch-depth: 0` para examinar o histórico relevante.
- Fixe actions externas por SHA.
- Use permissões mínimas.
- Não imprima findings sem redaction.
- Não execute código arbitrário de PR com credenciais privilegiadas.
- O gate deve falhar quando houver finding novo.

## Teste da configuração

Não faça commit de uma credencial com formato real, mesmo para teste.

Para validar localmente, use o comando oficial de testes da configuração quando disponível ou um repositório temporário descartável com valores documentados pelo próprio Gitleaks como amostras inválidas. Apague o repositório temporário depois.

## Saída esperada

Ao executar esta skill, informe:

- escopo examinado;
- comando ou workflow usado;
- quantidade de findings, sem valores sensíveis;
- classificação de cada finding;
- ações de revogação/rotação necessárias;
- alterações de configuração;
- validações realizadas.

## Restrições

- Não revelar o segredo detectado.
- Não assumir que segredo histórico está inativo.
- Não adicionar allowlist ampla para fazer o CI passar.
- Não reescrever histórico ou force-push sem autorização explícita.
- Não substituir um secret manager por Gitleaks.
