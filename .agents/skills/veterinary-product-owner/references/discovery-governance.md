# Governança de hipóteses, regras e discovery

## Objetivo

Preservar a diferença entre conhecimento validado e suposições, evitando que incertezas virem código, contrato, banco ou autorização por acidente.

## Estados do conhecimento

### Vigente

Regra ou decisão validada, com contexto, fonte e critério verificável.

### Hipótese

Possível necessidade ou comportamento ainda não confirmado. Não deve orientar automaticamente código, banco, validações, contratos ou permissões.

### Questão aberta

Pergunta objetiva cuja resposta é necessária para avançar.

### Decisão adiada

Tema conhecido e deliberadamente postergado. Deve possuir motivo, risco, solução temporária e gatilho de retomada.

### Descartada

Hipótese analisada e rejeitada. Preserve motivo e evidência quando a discussão puder se repetir.

## Evidência, interpretação e decisão

- **Evidência:** informação obtida de stakeholder, norma, processo observado, sistema legado, pesquisa ou decisão formal.
- **Interpretação:** conclusão derivada da evidência.
- **Hipótese:** explicação ou regra possível ainda não validada.
- **Decisão:** escolha formal entre alternativas.

Nunca apresente interpretação como evidência.

## Backlog de discovery

Use identificadores estáveis, como `DISC-001`.

Cada item deve conter, conforme o risco:

- identificador e título;
- estado;
- contexto e descrição;
- pergunta principal;
- perguntas secundárias;
- hipótese inicial;
- motivo pelo qual não é regra vigente;
- evidência disponível e necessária;
- fonte esperada da resposta;
- stakeholders envolvidos;
- módulos e Aggregates potencialmente afetados;
- impacto em autorização e multitenancy;
- impacto regulatório, de dados e financeiro;
- dependências e prioridade;
- risco de decidir prematuramente;
- risco de não decidir;
- critério de conclusão;
- gatilho de reavaliação;
- decisão resultante;
- links para regra, ADR, história ou SDD.

Não renumere itens publicados.

## Critérios para promover hipótese a regra vigente

Exija:

1. pergunta respondida;
2. fonte legítima identificada;
3. contexto de aplicação definido;
4. exceções conhecidas;
5. impactos entre módulos analisados;
6. impacto multitenant analisado;
7. impacto de autorização analisado;
8. impacto regulatório analisado quando aplicável;
9. linguagem alinhada ao glossário;
10. contradições resolvidas;
11. comportamento verificável;
12. responsável pela validação identificado.

Se os critérios não forem atendidos, mantenha hipótese ou questão aberta.

## Temas prioritários de discovery

Mantenha explícitos quando ainda não validados:

- políticas de Agenda;
- consentimento e autorização clínica;
- acesso ao prontuário;
- direitos dos titulares e operação LGPD;
- pessoa jurídica, abrigo, ONG e protetor;
- múltiplos responsáveis e permissões diferentes;
- responsável financeiro, pagador e destinatário;
- representação legal;
- histórico temporal completo dos vínculos;
- regras locais, fiscais e municipais.

## Integração com catálogo de regras

O catálogo deve:

- separar regras vigentes de hipóteses;
- apontar questões abertas para o backlog de discovery;
- registrar fonte, evidência, data e escopo;
- relacionar regras com histórias, testes e ADRs;
- preservar decisões descartadas quando útil;
- destacar contradições em vez de ocultá-las.

## Integração com glossário

Para termos ambíguos:

- registre definições candidatas;
- marque o estado;
- informe contexto e diferenças entre áreas;
- não escolha uma definição definitiva sem evidência.

Termos que normalmente exigem cuidado:

- tutor;
- responsável;
- proprietário;
- contato;
- solicitante;
- acompanhante;
- autorizador clínico;
- representante legal;
- responsável financeiro;
- pagador;
- titular;
- paciente;
- cliente;
- prontuário;
- consentimento e autorização.

## Priorização do discovery

Considere:

- bloqueio do MVP;
- risco clínico, regulatório, financeiro ou de segurança;
- risco de exposição ou perda de dados;
- quantidade de módulos afetados;
- custo de mudança futura;
- dívida semântica;
- proximidade da implementação;
- dependência de especialista externo.

Use escala simples. Não crie pontuação complexa sem benefício real.

## Fluxo de discovery recomendado

1. definir contexto e segmento de tenant;
2. identificar papéis e stakeholders;
3. mapear a jornada atual, inclusive exceções;
4. levantar regras, estados, permissões, documentos e riscos;
5. separar fatos de hipóteses;
6. selecionar a menor fatia vertical;
7. validar com fontes legítimas;
8. registrar decisões e atualizar catálogo, glossário e roadmap.

## Orientação para futuros SDDs

Um SDD não deve implementar hipótese como regra vigente sem:

- referenciar o item de discovery;
- apresentar evidência;
- registrar decisão;
- atualizar catálogo e glossário;
- avaliar impactos;
- criar ou atualizar ADR quando necessário;
- adicionar testes que expressem a regra.

Quando uma questão aberta bloquear apenas parte do escopo, implemente somente o que estiver validado e registre a limitação. Não preencha a lacuna com suposição silenciosa.