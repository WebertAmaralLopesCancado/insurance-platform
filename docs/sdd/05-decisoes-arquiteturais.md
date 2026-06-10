# Decisões Arquiteturais

## ADR-001 — Modelo de Domínio Rico

**Status:** Aceito

**Contexto:**
Modelos anêmicos (apenas propriedades públicas com setters) violam os princípios de encapsulamento do DDD e facilitam a fuga de regras de negócio para camadas externas.

**Decisão:**
Todas as entidades do domínio possuirão comportamentos explícitos. Nenhuma propriedade terá setter público. O estado interno do agregado só pode ser modificado por métodos do próprio agregado.

**Exemplos obrigatórios:**

| Agregado | Métodos |
|----------|---------|
| `Proposal` | `Create()`, `Approve()`, `Reject()` |
| `Contract` | `Create()` |

**Consequências:**
- Invariantes garantidas em tempo de compilação e execução
- Lógica de negócio centralizada no domínio
- Testes unitários de domínio sem dependências externas

---

## ADR-002 — Value Objects

**Status:** Aceito

**Contexto:**
Primitivos não expressam o significado do negócio e não validam seu próprio conteúdo. `string` para `CustomerName` aceita qualquer valor, incluindo inválidos.

**Decisão:**
Serão criados Value Objects para conceitos com semântica e regras próprias.

**Value Objects definidos:**

| Value Object | Contexto | Regras |
|--------------|----------|--------|
| `CustomerName` | ProposalService | Não vazio, max 200 chars, trim automático |
| `InsuranceType` | ProposalService | Apenas "Life", "Auto", "Property", "Health" |
| `CoverageAmount` | ProposalService | Maior que zero |

**Consequências:**
- Validações de valor centralizadas e testáveis isoladamente
- Construção inválida impossível por design (fail-fast)
- Igualdade semântica por valor, não por referência

---

## ADR-003 — Domain Events

**Status:** Aceito

**Contexto:**
Efeitos colaterais de operações de domínio (ex: notificações, integrações futuras) não devem estar acoplados ao fluxo principal do caso de uso.

**Decisão:**
O agregado registrará Domain Events internamente. Os eventos serão publicados pela Infrastructure após a persistência bem-sucedida (padrão outbox ou dispatch pós-save).

**Eventos definidos:**

| Evento | Contexto | Disparado por |
|--------|----------|---------------|
| `ProposalCreatedEvent` | ProposalService | `Proposal.Create()` |
| `ProposalApprovedEvent` | ProposalService | `Proposal.Approve()` |
| `ProposalRejectedEvent` | ProposalService | `Proposal.Reject()` |
| `ContractCreatedEvent` | ContractingService | `Contract.Create()` |

**Consequências:**
- Extensibilidade futura sem modificação dos casos de uso
- Separação entre o que aconteceu (evento) e o que fazer com isso (handler)

---

## ADR-004 — Ports and Adapters Explícitos

**Status:** Aceito

**Contexto:**
A arquitetura hexagonal exige que as fronteiras entre o núcleo e o exterior sejam explícitas via interfaces.

**Decisão:**
Todos os Ports serão interfaces nomeadas e localizadas em camadas específicas.

**Primary Ports (Application/Common):**
- `ICommandHandler<TCommand>`
- `ICommandHandler<TCommand, TResponse>`
- `IQueryHandler<TQuery, TResponse>`

**Secondary Ports — Domain:**
- `IProposalRepository`
- `IContractRepository`

**Secondary Ports — Application (ACL):**
- `IProposalServiceGateway`

**Primary Adapters (Api/Controllers):**
- `ProposalsController`
- `ContractsController`

**Secondary Adapters (Infrastructure):**
- `ProposalRepository`, `ContractRepository` (EF Core)
- `ProposalServiceGateway` (HTTP Client)

**Consequências:**
- Substituição de implementações sem impacto no domínio ou aplicação
- Testes com mocks simples baseados em interfaces
- Direção de dependência sempre preservada

---

## ADR-005 — Anti-Corruption Layer (ACL)

**Status:** Aceito

**Contexto:**
O `ContractingService` precisa verificar o status de uma proposta antes de criar um contrato. Se referenciasse diretamente entidades do `ProposalService`, os bounded contexts estariam acoplados.

**Decisão:**
A comunicação entre contextos ocorre exclusivamente via `IProposalServiceGateway` (Application) e `ProposalSnapshot` (DTO interno do ContractingService).

**Regras:**
- O `ContractingService` não importa, herda nem referencia nenhum tipo do `ProposalService`
- `ProposalSnapshot` é um DTO definido dentro do `ContractingService`
- A tradução do modelo externo para `ProposalSnapshot` ocorre em `ProposalServiceGateway`

**Consequências:**
- Bounded contexts isolados e evolutivos de forma independente
- Mudanças na API do `ProposalService` só afetam `ProposalServiceGateway`
- Testabilidade: `IProposalServiceGateway` é facilmente mockável

---

## ADR-006 — Separação de Casos de Uso por Intenção

**Status:** Aceito

**Contexto:**
Um caso de uso genérico `UpdateProposalStatus` aceita qualquer status como parâmetro, mistura responsabilidades e não é expressivo.

**Decisão:**
Casos de uso separados por intenção explícita:
- `ApproveProposal` — aprova uma proposta em análise
- `RejectProposal` — rejeita uma proposta em análise

**Consequências:**
- Cada handler tem uma única responsabilidade (SRP)
- Facilidade de adicionar validações distintas por transição no futuro
- Endpoints REST semanticamente corretos (`POST /proposals/{id}/approve`)

---

## ADR-007 — Paginação em GetAllProposals

**Status:** Aceito

**Contexto:**
Retornar todas as propostas sem paginação é inviável em produção e sinaliza falta de experiência com sistemas reais.

**Decisão:**
`GetAllProposals` retorna uma resposta paginada.

**Contrato de entrada:**

| Campo | Tipo | Padrão | Restrição |
|-------|------|--------|-----------|
| `PageNumber` | `int` | 1 | Mínimo: 1 |
| `PageSize` | `int` | 10 | Mínimo: 1, Máximo: 100 |

**Contrato de saída (`PagedResponse<T>`):**

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `Items` | `IReadOnlyList<T>` | Itens da página atual |
| `PageNumber` | `int` | Página atual |
| `PageSize` | `int` | Tamanho da página |
| `TotalItems` | `int` | Total de registros no banco |
| `TotalPages` | `int` | `ceil(TotalItems / PageSize)` |

**Consequências:**
- Desempenho previsível independente do volume de dados
- Contrato evolutivo sem breaking changes

---

## ADR-008 — Estratégia de Tratamento de Erros

**Status:** Aceito

**Contexto:**
Sem uma estratégia centralizada, cada controller trata erros de forma diferente, gerando inconsistências nas respostas HTTP.

**Decisão:**
Middleware global de exceções em cada Api (`ExceptionHandlerMiddleware`) com mapeamento para `ProblemDetails` (RFC 7807).

**Hierarquia de exceções:**

```
DomainException
  ProposalStatusTransitionException

ApplicationException (base interna — não System.ApplicationException)
  ProposalNotFoundException          → 404
  ContractNotFoundException           → 404
  ProposalNotApprovedException        → 422
  ProposalAlreadyContractedException  → 409
```

**Mapeamento HTTP:**

| Exceção | Status HTTP |
|---------|-------------|
| `NotFoundException` (base) | 404 Not Found |
| `ProposalStatusTransitionException` | 422 Unprocessable Entity |
| `ProposalNotApprovedException` | 422 Unprocessable Entity |
| `ProposalAlreadyContractedException` | 409 Conflict |
| `DomainException` | 422 Unprocessable Entity |
| `ValidationException` (FluentValidation) | 400 Bad Request |
| `Exception` (não mapeada) | 500 Internal Server Error |

**Formato da resposta de erro:**

```json
{
  "type": "string (URI de referência do erro)",
  "title": "string (descrição curta)",
  "status": 0,
  "detail": "string (mensagem legível)",
  "instance": "string (caminho da requisição)"
}
```

**Consequências:**
- Respostas de erro consistentes entre todos os endpoints
- Controllers sem blocos try/catch
- Facilidade de adicionar novos tipos de erro sem modificar controllers

---

## ADR-009 — Estratégia de Testes Automatizados

**Status:** Aceito

**Contexto:**
Testes são critério de avaliação da vaga. A estratégia deve cobrir domínio, aplicação e API com qualidade e clareza.

**Decisão:**
Três níveis de testes por microsserviço:

### Nível 1: Domain Unit Tests

**Projeto:** `*.Domain.UnitTests`
**Ferramentas:** xUnit, FluentAssertions
**Mocks:** nenhum (domínio puro)

Cobertura obrigatória:
- Factory methods dos agregados (cenários válidos e inválidos)
- Transições de status (`Approve`, `Reject`)
- Value Objects (cenários válidos e inválidos)
- Domain Events (verificar que foram registrados)
- Invariantes (verificar que lançam `DomainException`)

Exemplo de casos de teste:
```
✓ Proposal.Create() deve iniciar com status UnderAnalysis
✓ Proposal.Create() deve registrar ProposalCreatedEvent
✓ Proposal.Approve() deve mudar status para Approved
✓ Proposal.Approve() quando status != UnderAnalysis deve lançar ProposalStatusTransitionException
✓ Proposal.Reject() deve mudar status para Rejected
✓ CoverageAmount com valor zero deve lançar DomainException
✓ CustomerName vazio deve lançar DomainException
✓ InsuranceType inválido deve lançar DomainException
```

### Nível 2: Application Unit Tests

**Projeto:** `*.Application.UnitTests`
**Ferramentas:** xUnit, FluentAssertions, Moq
**Mocks:** repositórios e gateways via interfaces

Cobertura obrigatória:
- Todos os command handlers
- Todos os query handlers
- Cenários de sucesso e falha de cada caso de uso

Exemplo de casos de teste:
```
✓ CreateProposalHandler deve persistir e retornar resposta com Id
✓ GetProposalHandler quando proposta inexistente deve lançar ProposalNotFoundException
✓ ApproveProposalHandler deve chamar Approve() e UpdateAsync()
✓ CreateContractHandler deve consultar gateway antes de criar contrato
✓ CreateContractHandler quando proposta não aprovada deve lançar ProposalNotApprovedException
✓ CreateContractHandler quando proposta já contratada deve lançar ProposalAlreadyContractedException
```

### Nível 3: API Integration Tests

**Projeto:** `*.Api.IntegrationTests`
**Ferramentas:** xUnit, FluentAssertions, `Microsoft.AspNetCore.Mvc.Testing`, `Testcontainers.PostgreSql`
**Banco:** PostgreSQL real via Testcontainers (container Docker em memória durante o teste)

Cobertura obrigatória:
- Fluxo HTTP completo para cada endpoint
- Cenários de sucesso (2xx)
- Cenários de erro (4xx, 5xx)
- Verificação do banco de dados após operações

Exemplo de casos de teste:
```
✓ POST /proposals → 201 Created com corpo correto
✓ POST /proposals com payload inválido → 400 Bad Request
✓ GET /proposals/{id} existente → 200 OK
✓ GET /proposals/{id} inexistente → 404 Not Found
✓ POST /proposals/{id}/approve → 204 No Content
✓ POST /proposals/{id}/approve quando já aprovada → 422 Unprocessable Entity
✓ GET /proposals?pageNumber=1&pageSize=10 → 200 OK com paginação
✓ POST /contracts → 201 Created
✓ POST /contracts quando proposta não aprovada → 422 Unprocessable Entity
✓ POST /contracts quando proposta já contratada → 409 Conflict
✓ GET /contracts/{id} → 200 OK
✓ GET /contracts/{id} inexistente → 404 Not Found
```

**Consequências:**
- Cobertura dos três níveis da pirâmide de testes
- Testes de domínio rápidos e sem dependências
- Testes de integração com banco real (sem risco de divergência mock/produção)
- Testcontainers elimina necessidade de banco pré-configurado no CI

---

## ADR-010 — Validação de Input com FluentValidation

**Status:** Aceito

**Contexto:**
A validação de DTOs de entrada (format, range, required) não é responsabilidade do domínio. O domínio valida invariantes. A API valida o formato do input.

**Decisão:**
FluentValidation será usado para validar Commands e Queries antes da execução do handler.

**Localização:** `Application/UseCases/{UseCase}/`

Cada command ou query com validações terá um `Validator<T>` correspondente.

**Consequências:**
- Domain protegido de input malformado
- Mensagens de validação descritivas retornadas como 400 Bad Request
- Separação clara entre validação de formato (Application) e invariante (Domain)

---

## ADR-011 — SeedWork por Microsserviço

**Status:** Aceito

**Contexto:**
Classes base de domínio (`Entity`, `AggregateRoot`, `ValueObject`, `IDomainEvent`) são necessárias em ambos os microsserviços.

**Decisão:**
Cada microsserviço tem sua própria `SeedWork` dentro do seu projeto `Domain`. Não há projeto compartilhado entre microsserviços.

**Justificativa:**
- Microsserviços devem ser deployáveis e evolutivos de forma independente
- Um projeto compartilhado cria acoplamento entre serviços
- A duplicação de ~50 linhas de código base é aceitável para manter o isolamento

**Consequências:**
- Total independência entre os microsserviços
- Cada serviço pode evoluir sua base de domínio sem afetar o outro
