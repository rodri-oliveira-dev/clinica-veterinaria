# Matriz de rastreabilidade - Entrega 1

| Regra | Aggregate | Caso de uso | Endpoint | Teste | ADR | Status |
| --- | --- | --- | --- | --- | --- | --- |
| BR-TUT-001 | `Tutor` | Todos de Tutores | `/tutores*` | `TutoresPersistenceTests`, `TutoresApiTests` | ADR-0001, ADR-0003 | Vigente |
| BR-TUT-002 | `Tutor` | `CadastrarTutor`, `AtualizarTutor` | `POST /tutores`, `PUT /tutores/{tutorId}` | `ValueObjectTests`, `TutoresApiTests` | ADR-0003 | Vigente |
| BR-TUT-003 | `Tutor` | `CadastrarTutor`, `AtualizarTutor`, `PesquisarTutores` | `POST /tutores`, `PUT /tutores/{tutorId}`, `GET /tutores` | `TutorTests`, `ValueObjectTests`, `TutoresPersistenceTests`, `TutoresApiTests` | ADR-0001, ADR-0003 | Vigente |
| BR-TUT-004 | `Tutor` | `CadastrarTutor`, `AtualizarTutor` | `POST /tutores`, `PUT /tutores/{tutorId}` | `TutorTests`, `ValueObjectTests`, `TutoresApiTests` | ADR-0003 | Vigente |
| BR-TUT-005 | `Tutor` | `InativarTutor` | `POST /tutores/{tutorId}/inativacao` | `TutorTests`, `TutoresPersistenceTests`, `TutoresApiTests` | ADR-0006 | Vigente |
| BR-TUT-006 | `Tutor`, `Animal` | `InativarTutor` | `POST /tutores/{tutorId}/inativacao` | `TutoresApiTests.InativarTutor_ComAnimalAtivoVinculado_RetornaConflictEPreservaTutor` | ADR-0006 | Vigente |
| BR-TUT-007 | `Tutor` | `AtualizarTutor` | `PUT /tutores/{tutorId}` | `TutorTests`, `TutoresApiTests`, `TutoresPersistenceTests` | ADR-0001, ADR-0003 | Vigente |
| BR-ANI-001 | `Animal` | Todos de Animais | `/animais*` | `AnimalTests`, `AnimaisPersistenceTests`, `AnimaisApiTests` | ADR-0001, ADR-0003 | Vigente |
| BR-ANI-002 | `Animal` | `CadastrarAnimal`, `AtualizarAnimal` | `POST /animais`, `PUT /animais/{animalId}` | `AnimalValueObjectTests`, `AnimaisApiTests` | ADR-0003 | Vigente |
| BR-ANI-003 | `Animal` | `CadastrarAnimal`, `AtualizarAnimal` | `POST /animais`, `PUT /animais/{animalId}` | `AnimalTests`, `AnimalValueObjectTests`, `AnimaisPersistenceTests` | ADR-0005 | Vigente |
| BR-ANI-004 | `Animal` | `CadastrarAnimal`, `AtualizarAnimal` | `POST /animais`, `PUT /animais/{animalId}` | `AnimalValueObjectTests`, `AnimalTests` | ADR-0005 | Vigente |
| BR-ANI-005 | `Animal` | Todos de Animais | `/animais*` | `AnimalTests`, `AnimaisPersistenceTests`, `AnimaisApiTests` | ADR-0005 | Vigente |
| BR-ANI-006 | `Animal` | `RegistrarFalecimentoDoAnimal` | `POST /animais/{animalId}/falecimento` | `AnimalValueObjectTests`, `AnimalTests`, `AnimaisPersistenceTests`, `AnimaisApiTests` | ADR-0005 | Vigente |
| BR-ANI-007 | `Animal` | `AtualizarAnimal`, `InativarAnimal`, `TransferirResponsabilidadeDoAnimal` | `PUT /animais/{animalId}`, `POST /animais/{animalId}/inativacao`, `POST /animais/{animalId}/transferencias-de-responsabilidade` | `AnimalTests`, `AnimaisApiTests` | ADR-0005 | Vigente |
| BR-ANI-008 | `Animal` | `AtualizarAnimal` | `PUT /animais/{animalId}` | `AnimalTests`, `AnimaisApiTests` | ADR-0004 | Vigente |
| BR-ANI-009 | `Animal` | `InativarAnimal` | `POST /animais/{animalId}/inativacao` | `AnimalTests`, `AnimaisPersistenceTests`, `AnimaisApiTests` | ADR-0005, ADR-0006 | Vigente |
| BR-REL-001 | `Animal` | `CadastrarAnimal`, `TransferirResponsabilidadeDoAnimal` | `POST /animais`, `POST /animais/{animalId}/transferencias-de-responsabilidade` | `AnimalValueObjectTests`, `AnimaisPersistenceTests`, `AnimaisApiTests` | ADR-0004, ADR-0006 | Vigente |
| BR-REL-002 | `Animal` | `CadastrarAnimal` | `POST /animais` | `AnimaisPersistenceTests.MesmoTutorPodeSerResponsavelPorMultiplosAnimaisNoTenant` | ADR-0006 | Vigente |
| BR-REL-003 | `Animal`, `Tutor` | `CadastrarAnimal` | `POST /animais` | `AnimaisPersistenceTests`, `AnimaisApiTests` | ADR-0004, ADR-0006 | Vigente |
| BR-REL-004 | `Animal`, `TransferenciaDeResponsabilidadeDoAnimal` | `TransferirResponsabilidadeDoAnimal` | `POST /animais/{animalId}/transferencias-de-responsabilidade` | `AnimalTests`, `AnimaisApiTests` | ADR-0004, ADR-0006 | Vigente |
| BR-REL-005 | `TransferenciaDeResponsabilidadeDoAnimal` | `TransferirResponsabilidadeDoAnimal` | `POST /animais/{animalId}/transferencias-de-responsabilidade` | `AnimaisApiTests.TransferirResponsabilidade_ComDadosValidos_AtualizaTutorEVersaoERegistraHistorico` | ADR-0004, ADR-0006 | Vigente com lacuna de teste |
| BR-TEN-001 | N/A | Todos tenant-owned | `/tutores*`, `/animais*` | `TenantIdParserTests`, `JwtBearerDiagnosticsAuthorizationTests`, `TutoresApiTests`, `AnimaisApiTests` | ADR-0001 | Vigente |
| BR-TEN-002 | N/A | Todos tenant-owned | `/tutores*`, `/animais*` | `TutoresApiTests`, `AnimaisApiTests`, `OpenApiContractTests` | ADR-0001 | Vigente |
| BR-TEN-003 | `Tutor`, `Animal`, `TransferenciaDeResponsabilidadeDoAnimal` | Todos persistentes | N/A | `TutoresPersistenceTests`, `AnimaisPersistenceTests` | ADR-0001 | Vigente |
| BR-TEN-004 | `Animal`, `TransferenciaDeResponsabilidadeDoAnimal` | `CadastrarAnimal`, `TransferirResponsabilidadeDoAnimal` | `POST /animais`, `POST /animais/{animalId}/transferencias-de-responsabilidade` | `AnimaisPersistenceTests`, `AnimaisApiTests` | ADR-0001, ADR-0004, ADR-0006 | Vigente |
| BR-TEN-005 | `Tutor` | `CadastrarTutor`, `AtualizarTutor` | `POST /tutores`, `PUT /tutores/{tutorId}` | `TutoresPersistenceTests`, `TutoresApiTests` | ADR-0001 | Vigente |
| BR-TEN-006 | N/A | Todos persistentes | N/A | Suites de integracao com Tenant A e Tenant B | ADR-0001 | Vigente |
