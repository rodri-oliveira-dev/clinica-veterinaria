# Checklists de prontidão, conclusão e revisão

## Definition of Ready

Uma tarefa está pronta para desenvolvimento quando, na profundidade adequada ao risco:

- o problema e o resultado esperado estão claros;
- o papel envolvido está identificado;
- as regras conhecidas estão documentadas;
- hipóteses estão marcadas como hipóteses;
- fluxo principal e exceções relevantes estão descritos;
- estados e transições estão claros quando aplicáveis;
- permissões estão definidas;
- ownership e escopo do tenant estão claros;
- critérios de aceite são testáveis;
- conflitos e erros esperados foram mapeados;
- documentos e auditoria foram avaliados;
- dependências estão identificadas;
- risco clínico, regulatório, financeiro e de privacidade foi classificado;
- validações especializadas foram concluídas ou registradas como bloqueio;
- UX possui contexto suficiente;
- arquitetura recebeu as invariantes relevantes;
- fora de escopo está explícito.

Não exija documentação excessiva para tarefas simples. A profundidade deve ser proporcional à incerteza e ao impacto.

## Definition of Done de produto

A conclusão técnica não encerra automaticamente o item. Verifique:

- critérios de aceite demonstrados;
- isolamento entre pelo menos dois tenants quando houver persistência;
- permissões verificadas;
- erros e conflitos tratados de forma compreensível;
- cancelamento, repetição e concorrência avaliados quando relevantes;
- auditoria crítica disponível;
- documentação funcional atualizada;
- textos e termos revisados;
- impacto regulatório validado quando aplicável;
- métricas definidas quando necessárias;
- suporte e operação informados sobre mudanças relevantes;
- feedback da demonstração registrado;
- pendências transformadas em itens explícitos.

## Checklist final de especificação

- O problema está claro?
- O ator correto foi identificado?
- A linguagem do domínio está consistente?
- Existe hipótese disfarçada de fato?
- O fluxo principal está completo?
- As exceções mais importantes foram consideradas?
- Estados e transições são explícitos?
- Há conflito de agenda, recurso, permissão ou cobrança?
- O ownership funcional está compreendido?
- Tenant e unidade estão claros?
- Existe cenário com dois tenants?
- Há dados pessoais?
- Há informação clínica ou confidencial?
- Alguma ação crítica exige auditoria?
- Existe documento, consentimento ou assinatura?
- A funcionalidade toca ato restrito ao médico-veterinário?
- Há impacto de CDC, LGPD, CFMV/CRMV, MAPA, Anvisa ou regra local?
- A fonte regulatória foi registrada com data?
- É necessária validação especializada?
- O item pode ser entregue de forma menor?
- O fora de escopo está explícito?
- Os critérios de aceite são testáveis?

## Revisão de limites semânticos

Ao lidar com pessoas e animais, confirme:

- tutor não foi tratado automaticamente como proprietário legal;
- contato não foi tratado automaticamente como autorizador clínico;
- solicitante não foi tratado automaticamente como pagador;
- responsável operacional não foi tratado automaticamente como responsável financeiro;
- vínculo cadastral não concedeu acesso automático ao prontuário;
- direitos relacionados a dados não foram inferidos do vínculo com o animal;
- cada capacidade possui linguagem e regra próprias ou foi marcada como pendente.

## Revisão de discovery

- Todos os itens possuem estado explícito?
- Regras vigentes possuem fonte e evidência?
- Hipóteses possuem pergunta de validação?
- Questões abertas possuem próxima ação?
- Decisões adiadas possuem gatilho de retomada?
- Hipóteses descartadas possuem justificativa?
- Existem identificadores duplicados?
- Catálogo, glossário e backlog são consistentes?
- O roadmap diferencia discovery de implementação?
- Temas pendentes aparecem indevidamente como requisitos confirmados?

## Revisão de risco alto

Para prontuário, consentimento, prescrição, controlados, internação, telemedicina, LGPD, cobrança ou operação cross-tenant, confirme:

- especialista necessário identificado;
- fonte oficial verificada e datada;
- autoria, integridade e auditoria avaliadas;
- permissões e segregação de funções definidas;
- retenção, correção e histórico analisados;
- comportamento de emergência ou exceção descrito;
- testes adicionais identificados;
- fora de escopo e solução temporária registrados.

## Qualidade da resposta da PO

A resposta final deve:

- ser clara, profissional e em português brasileiro;
- separar fato, hipótese, interpretação e recomendação;
- evitar jargão técnico desnecessário para o público de produto;
- não declarar conformidade jurídica definitiva;
- não prescrever tratamento ou decisão clínica;
- não inventar regra, fonte, contrato ou arquitetura;
- preferir a menor fatia útil;
- deixar lacunas importantes visíveis;
- indicar o próximo passo de produto ou discovery.