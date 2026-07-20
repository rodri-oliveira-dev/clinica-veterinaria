# Migração das skills de cybersecurity

## Objetivo

Registrar a origem, a versão e as adaptações realizadas ao migrar skills do repositório `mukul975/Anthropic-Cybersecurity-Skills` para este projeto.

## Origem fixada

- Repositório: `https://github.com/mukul975/Anthropic-Cybersecurity-Skills`
- Commit de origem: `673da1f3b0b7be34ffc9624ef3858fe45f1c3bed`
- Licença declarada: Apache License 2.0
- Cópia da licença: `third-party/licenses/Anthropic-Cybersecurity-Skills-APACHE-2.0.txt`

As skills locais são trabalhos modificados. Não são cópias oficiais nem recebem atualizações automáticas do repositório de origem.

## Mapeamento

| Origem | Adaptação local | Principais mudanças |
|---|---|---|
| `skills/performing-threat-modeling-with-owasp-threat-dragon/SKILL.md` | `.agents/skills/threat-modeling-clinica-veterinaria/SKILL.md` | Threat Dragon tornou-se opcional; Markdown/Mermaid é o padrão; foram adicionados monólito modular, STRIDE/LINDDUN, LGPD e ameaças multitenant obrigatórias. |
| `skills/implementing-api-schema-validation-security/SKILL.md` | `.agents/skills/input-validation-dotnet/SKILL.md` | Exemplos de Python e API Gateway foram removidos; validação foi direcionada a .NET 10/ASP.NET Core; tipos e tamanhos derivam de entidades, Value Objects e invariantes; entidades não são contratos HTTP. |
| `skills/implementing-secret-scanning-with-gitleaks/SKILL.md` | `.agents/skills/secret-scanning-gitleaks/SKILL.md` | Comandos atualizados para a CLI moderna; baseline deixou de ser padrão; configuração e workflow reais foram adicionados. |
| `skills/implementing-devsecops-security-scanning/SKILL.md` | `.agents/skills/devsecops-security-scanning/SKILL.md` | Semgrep não foi incluído por sobreposição com CodeQL/SonarCloud; Trivy e ZAP ficaram condicionados à existência de imagem e staging; SonarCloud foi integrado como opt-in. |
| `skills/testing-jwt-token-security/SKILL.md` | `.agents/skills/keycloak-jwt-security/SKILL.md` | Conteúdo ofensivo, brute force e falsificação foram removidos; a skill passou a tratar configuração defensiva de Keycloak, JwtBearer, audience, issuer, JWKS, roles e `tenant_id`. |

## Arquivos operacionais adicionados

### Gitleaks

- `.gitleaks.toml`
- `.github/workflows/gitleaks.yml`

O workflow está ativo para pull requests contra `main`, pushes em `main`, execução manual e agenda semanal. A action está fixada pelo commit correspondente à versão 2.3.9.

O repositório é de uma conta pessoal. A action v2 não requer `GITLEAKS_LICENSE` nesse cenário. Se o repositório for transferido para uma organização, revise a exigência de licença da action antes da transferência.

### SonarCloud

- `.github/workflows/sonarcloud.yml`

O workflow é opt-in. Sem a variável `SONARCLOUD_ENABLED=true`, o job permanece ignorado e não quebra o CI.

Configure as repository variables:

```text
SONARCLOUD_ENABLED=true
SONAR_HOST_URL=https://sonarcloud.io
```

`SONAR_PROJECT_KEY` e `SONAR_ORGANIZATION` possuem defaults versionados no workflow para este repositório:

```text
SONAR_PROJECT_KEY=rodri-oliveira-dev_clinica-veterinaria
SONAR_ORGANIZATION=rodri-oliveira-dev
```

Use repository variables com esses nomes somente se o projeto for renomeado, transferido ou precisar sobrescrever os defaults.

Configure o repository secret:

```text
SONAR_TOKEN=<token restrito>
```

O host deve ser alterado quando o projeto estiver em outra região do SonarQube Cloud.

O workflow usa SonarScanner for .NET 11.2.0, executa build e testes dentro da análise, importa cobertura OpenCover e aguarda o Quality Gate. Pull requests originados de forks são ignorados para não expor secrets.

## Decisões específicas

### Validação de entrada

- Entidades e Value Objects são fontes dos tipos e limites semânticos.
- Requests e responses continuam separados do domínio e da persistência.
- O validator deve reutilizar constantes ou fábricas do domínio; não deve duplicar números arbitrários.
- Quando o domínio ainda não definiu um limite, a skill orienta registrar a lacuna em vez de inventar tamanho.
- Create, update e patch podem possuir obrigatoriedade diferente mesmo quando usam o mesmo conceito.
- Propriedades JSON desconhecidas devem ser rejeitadas conscientemente em contratos fechados.
- `tenant_id` não é um campo gravável pelo cliente em operações comuns.

### SonarCloud junto com CodeQL

CodeQL permanece ativo. SonarCloud adiciona análise de qualidade, bugs, vulnerabilidades, hotspots e Quality Gate. Semgrep não foi incluído agora porque criaria sobreposição sem gap demonstrado.

### Keycloak

Esta migração não provisiona realm, client, usuário ou secret do Keycloak. Esses valores dependem da topologia de identidade ainda não definida.

A skill exige, quando o Keycloak for implementado:

- Authority correspondente ao `iss` público;
- audience explícita da API;
- assinatura via metadata/JWKS do realm;
- `tenant_id` emitido por mapper confiável;
- roles ou permissions específicas da API;
- testes de token inválido e isolamento com tenants A e B.

### Threat modeling

O OWASP Threat Dragon não foi instalado. O formato padrão local é Markdown/Mermaid, que evita dependência de ferramenta. Um template inicial está em `docs/security/threat-models/_template.md`.

## Controles que permanecem futuros

- Trivy de filesystem e imagem: adicionar quando existir imagem oficial.
- SBOM CycloneDX: adicionar quando houver artefato/release definido.
- OWASP ZAP: adicionar quando existir ambiente de staging estável e autorizado.
- Testes reais de integração com Keycloak: adicionar quando realm e clients estiverem configurados.

## Atualizações da origem

Para incorporar uma nova versão:

1. compare a skill local com o novo commit da origem;
2. revise scripts e comandos como conteúdo não confiável;
3. preserve regras arquiteturais e multitenant locais;
4. registre o novo commit neste documento e no frontmatter da skill;
5. mantenha o aviso de arquivo modificado;
6. execute revisão de segurança antes do merge.

Não atualize automaticamente as skills externas.
