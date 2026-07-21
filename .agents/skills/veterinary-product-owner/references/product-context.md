# Contexto do produto e limites arquiteturais

## Visão do produto

A plataforma atende clínicas veterinárias, hospitais, consultórios, petshops e serviços para pets no Brasil. O objetivo é apoiar operações reais com segurança, rastreabilidade e evolução incremental, sem antecipar complexidade técnica ou regras ainda não descobertas.

A Product Owner deve compreender o problema antes de especificar a solução e preservar a diferença entre necessidade de negócio e decisão técnica.

## Direção arquitetural atual

- Backend em .NET 10 e ASP.NET Core.
- PostgreSQL e Entity Framework Core.
- Monólito modular com um único deploy.
- Autenticação e identidade com Keycloak.
- OpenTelemetry para observabilidade.
- Aspire como experiência principal de composição local.
- Docker Compose como alternativa explícita para execução containerizada.
- Testcontainers para testes de integração.
- Frontend provável em React, TypeScript e Vite quando entrar no escopo.

A arquitetura deve permanecer simples até que regras, volume ou operação justifiquem mecanismos adicionais.

## Limites que a PO deve respeitar

A PO deve:

- descrever capacidades, responsabilidades, jornadas, regras e resultados;
- expor invariantes e necessidades de consistência;
- identificar ownership funcional dos dados;
- apontar riscos, dependências e validações especializadas;
- preparar fatias verticais pequenas e demonstráveis;
- colaborar com arquitetura sem prescrever a implementação.

A PO não deve:

- propor microsserviços, filas, brokers, API Gateway, cache distribuído ou bancos separados sem requisito real;
- transformar nomes de entidades em módulos definitivos;
- definir tabelas, projetos, `DbContext`, frameworks ou padrões como requisitos de produto;
- exigir integração assíncrona apenas porque uma ação pode acontecer depois;
- criar capacidades vazias para uma arquitetura futura hipotética.

## Bounded contexts e módulos

As capacidades candidatas incluem:

- organização, clínicas e unidades;
- tutores, responsáveis e animais;
- agenda;
- atendimento;
- prontuário e documentos;
- catálogo de serviços;
- profissionais e força de trabalho;
- faturamento e cobrança;
- estoque;
- comunicação;
- assinaturas SaaS;
- auditoria e privacidade.

Esses nomes não são decisões encerradas. Fronteiras devem surgir de linguagem, ownership, invariantes, transações e ritmos de mudança.

## Estado atual do repositório

A primeira fatia funcional é o módulo `PetShop.Tutores`, que representa a capacidade inicial de cadastro de tutores e animais.

Decisões atuais:

- `Tutor` e `Animal` permanecem no mesmo bounded context inicial;
- o módulo possui ownership de suas entidades, mappings, repositórios e tabelas;
- outros módulos não acessam diretamente seu banco ou tipos internos;
- o vínculo vigente entre tutor e animal representa responsabilidade operacional cadastral;
- Agenda, Atendimento, Prontuário e Cobrança ainda não devem ampliar a semântica desse vínculo;
- contratos entre módulos devem transportar somente a informação necessária para o caso de uso.

## Guardrail semântico do vínculo

`TutorResponsavelId` não significa automaticamente:

- autorizador clínico;
- pessoa com acesso ao prontuário;
- representante legal;
- responsável financeiro;
- pagador;
- titular de todos os direitos relacionados aos dados.

Módulos futuros devem modelar cada capacidade de forma deliberada. Quando a regra ainda não estiver descoberta, registre hipótese ou questão aberta em vez de reutilizar o vínculo existente.

## Divisão de responsabilidade

### Produto e domínio

Esclarecem:

- problema e resultado esperado;
- papéis e jornadas;
- regras, exceções, estados e transições;
- dados e documentos necessários;
- permissões e auditoria;
- riscos regulatórios e operacionais;
- políticas que variam por tenant.

### Arquitetura e engenharia

Decidem:

- fronteiras compiláveis;
- contratos técnicos;
- persistência e transações;
- integração e consistência;
- observabilidade técnica;
- padrões de implementação;
- topologia de execução e evolução operacional.

## Linguagem

- Use português brasileiro nos conceitos e casos de uso do domínio.
- Preserve termos técnicos consolidados quando melhorarem a clareza.
- Não use `paciente`, `cliente`, `responsável`, `tutor` e `proprietário` como sinônimos automáticos.
- Registre no glossário quando uma palavra variar entre clínicas ou possuir significado jurídico e operacional diferente.

## Estratégia de entrega

Prefira uma sequência de fatias que permita validar o produto:

1. cadastrar pessoa responsável;
2. cadastrar animal e vínculo operacional;
3. cadastrar serviço simples;
4. cadastrar profissional e disponibilidade básica;
5. criar agendamento válido;
6. consultar agenda diária;
7. realizar check-in;
8. encerrar atendimento operacional simples;
9. registrar cobrança básica.

Prontuário clínico completo, prescrição, internação, controlados e consentimento exigem discovery específico antes da implementação.