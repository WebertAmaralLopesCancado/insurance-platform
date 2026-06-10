# Modelagem do Domínio

## Princípio: Modelo Rico

O domínio não é um conjunto de DTOs com getters/setters. Cada agregado encapsula seu comportamento e protege suas próprias invariantes. Nenhuma lógica de negócio reside fora do domínio.

---

## SeedWork

Infraestrutura base do domínio, comum a ambos os microsserviços.

**Localização:** `Domain/SeedWork/`

### Entity\<TId\>

Classe base para todas as entidades do domínio.

```
Entity<TId>
  TId Id                          — identificador único
  IReadOnlyList<IDomainEvent> DomainEvents  — eventos pendentes de publicação
  void RaiseDomainEvent(IDomainEvent)       — registra evento na coleção
  void ClearDomainEvents()                  — limpa após publicação
```

### AggregateRoot\<TId\>

Estende `Entity<TId>`. Marca o ponto de entrada do agregado. Toda modificação de estado passa pela raiz do agregado.

### ValueObject

Classe base para objetos de valor. Implementa igualdade por valor (não por referência).

```
ValueObject
  abstract IEnumerable<object> GetEqualityComponents()
  override bool Equals(object?)
  override int GetHashCode()
  operators == e !=
```

### IDomainEvent

Interface marcadora para domain events.

```
IDomainEvent
  DateTime OccurredAt    — data e hora UTC do evento
```

---

## Proposal Context

### Agregado: Proposal

**Localização:** `ProposalService/Domain/Aggregates/Proposals/`

#### Propriedades

| Propriedade | Tipo | Visibilidade | Observação |
|-------------|------|--------------|------------|
| `Id` | `Guid` | público, get | Gerado no factory method |
| `CustomerName` | `CustomerName` | público, get | Value Object |
| `InsuranceType` | `InsuranceType` | público, get | Value Object |
| `CoverageAmount` | `CoverageAmount` | público, get | Value Object |
| `Status` | `ProposalStatus` | público, get | Somente atualizado por métodos do agregado |
| `CreatedAt` | `DateTime` | público, get | UTC, gerado no factory method |

#### Construtor

O construtor é privado. A criação ocorre exclusivamente via factory method `Create()`.
O EF Core instancia via construtor sem parâmetros (privado), necessário para materialização.

#### Comportamentos

**`Proposal.Create(customerName, insuranceType, coverageAmount)`**

```
Pré-condições:
  - customerName não nulo (validado pelo VO)
  - insuranceType válido (validado pelo VO)
  - coverageAmount > 0 (validado pelo VO)

Pós-condições:
  - Status == UnderAnalysis
  - CreatedAt == DateTime.UtcNow
  - Evento ProposalCreatedEvent registrado
```

**`Proposal.Approve()`**

```
Pré-condição:
  - Status == UnderAnalysis
    → caso contrário: lança ProposalStatusTransitionException

Pós-condição:
  - Status == Approved
  - Evento ProposalApprovedEvent registrado
```

**`Proposal.Reject()`**

```
Pré-condição:
  - Status == UnderAnalysis
    → caso contrário: lança ProposalStatusTransitionException

Pós-condição:
  - Status == Rejected
  - Evento ProposalRejectedEvent registrado
```

#### Invariantes do Agregado

- A proposta nasce sempre `UnderAnalysis`
- Apenas propostas `UnderAnalysis` mudam de estado
- Nenhuma propriedade pública tem setter
- `Status` nunca é atribuído diretamente de fora do agregado

---

### Value Objects — Proposal Context

**Localização:** `ProposalService/Domain/ValueObjects/`

#### CustomerName

Representa o nome do cliente de forma segura.

```
CustomerName
  string Value

Validações:
  - Não nulo e não vazio após trim → lança DomainException
  - Máximo de 200 caracteres → lança DomainException
  - Trim automático aplicado no construtor
```

#### InsuranceType

Representa o tipo de seguro.

```
InsuranceType
  string Value

Valores aceitos: "Life", "Auto", "Property", "Health"
  - Qualquer outro valor → lança DomainException
  - Case insensitive na validação, armazenado normalizado (Pascal Case)
```

#### CoverageAmount

Representa o valor monetário de cobertura.

```
CoverageAmount
  decimal Value

Validações:
  - Deve ser maior que zero → lança DomainException
```

---

### Domain Events — Proposal Context

**Localização:** `ProposalService/Domain/Aggregates/Proposals/Events/`

#### ProposalCreatedEvent

```
ProposalCreatedEvent : IDomainEvent
  Guid     ProposalId
  string   CustomerName
  string   InsuranceType
  decimal  CoverageAmount
  DateTime OccurredAt
```

#### ProposalApprovedEvent

```
ProposalApprovedEvent : IDomainEvent
  Guid     ProposalId
  DateTime OccurredAt
```

#### ProposalRejectedEvent

```
ProposalRejectedEvent : IDomainEvent
  Guid     ProposalId
  DateTime OccurredAt
```

---

### Enum: ProposalStatus

**Localização:** `ProposalService/Domain/Aggregates/Proposals/`

```
ProposalStatus
  UnderAnalysis = 0
  Approved      = 1
  Rejected      = 2
```

Armazenado no banco como `integer`. Mapeamento explícito via EF Core Fluent API.

---

### Repositório: IProposalRepository

**Localização:** `ProposalService/Domain/Repositories/`

```
IProposalRepository
  Task<Proposal?> GetByIdAsync(Guid id, CancellationToken ct = default)
  Task<IReadOnlyList<Proposal>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
  Task<int> CountAsync(CancellationToken ct = default)
  Task AddAsync(Proposal proposal, CancellationToken ct = default)
  Task UpdateAsync(Proposal proposal, CancellationToken ct = default)
```

---

## Contracting Context

### Agregado: Contract

**Localização:** `ContractingService/Domain/Aggregates/Contracts/`

#### Propriedades

| Propriedade | Tipo | Visibilidade | Observação |
|-------------|------|--------------|------------|
| `Id` | `Guid` | público, get | Gerado no factory method |
| `ProposalId` | `Guid` | público, get | Referência opaca ao contexto externo |
| `ContractedAt` | `DateTime` | público, get | UTC, gerado no factory method |

#### Construtor

Privado. Criação exclusiva via factory method `Create()`.

#### Comportamentos

**`Contract.Create(proposalId)`**

```
Pré-condições:
  - proposalId != Guid.Empty → lança DomainException

Pós-condições:
  - ContractedAt == DateTime.UtcNow
  - Evento ContractCreatedEvent registrado
```

> A validação de negócio (proposta aprovada, contrato duplicado) ocorre no `CreateContractCommandHandler` (Application), antes de chamar `Contract.Create()`. O agregado protege apenas suas invariantes internas.

#### Invariantes do Agregado

- `ProposalId` não pode ser `Guid.Empty`
- `ContractedAt` é sempre UTC e não é nulo

---

### Domain Events — Contracting Context

**Localização:** `ContractingService/Domain/Aggregates/Contracts/Events/`

#### ContractCreatedEvent

```
ContractCreatedEvent : IDomainEvent
  Guid     ContractId
  Guid     ProposalId
  DateTime ContractedAt
  DateTime OccurredAt
```

---

### Repositório: IContractRepository

**Localização:** `ContractingService/Domain/Repositories/`

```
IContractRepository
  Task<Contract?> GetByIdAsync(Guid id, CancellationToken ct = default)
  Task<Contract?> GetByProposalIdAsync(Guid proposalId, CancellationToken ct = default)
  Task AddAsync(Contract contract, CancellationToken ct = default)
```

---

## Application Layer — Casos de Uso

### Proposal Service

**Localização:** `ProposalService/Application/UseCases/`

#### CreateProposal

```
CreateProposalCommand
  string  CustomerName
  string  InsuranceType
  decimal CoverageAmount

CreateProposalResponse
  Guid     Id
  string   CustomerName
  string   InsuranceType
  decimal  CoverageAmount
  string   Status
  DateTime CreatedAt
```

Fluxo do handler:
1. Valida o Command via FluentValidation
2. Chama `Proposal.Create(...)`
3. Persiste via `IProposalRepository.AddAsync()`
4. Retorna `CreateProposalResponse`

#### GetProposal

```
GetProposalQuery
  Guid Id

ProposalResponse
  Guid     Id
  string   CustomerName
  string   InsuranceType
  decimal  CoverageAmount
  string   Status
  DateTime CreatedAt
```

Fluxo do handler:
1. Busca via `IProposalRepository.GetByIdAsync()`
2. Lança `ProposalNotFoundException` se não encontrado
3. Mapeia para `ProposalResponse`

#### GetAllProposals

```
GetAllProposalsQuery
  int PageNumber    — mínimo: 1
  int PageSize      — mínimo: 1, máximo: 100

PagedResponse<ProposalResponse>
  IReadOnlyList<ProposalResponse> Items
  int PageNumber
  int PageSize
  int TotalItems
  int TotalPages    — calculado: ceil(TotalItems / PageSize)
```

#### ApproveProposal

```
ApproveProposalCommand
  Guid Id
```

Fluxo do handler:
1. Busca via `IProposalRepository.GetByIdAsync()`
2. Lança `ProposalNotFoundException` se não encontrado
3. Chama `proposal.Approve()`
4. Persiste via `IProposalRepository.UpdateAsync()`

#### RejectProposal

```
RejectProposalCommand
  Guid Id
```

Fluxo do handler: idêntico ao `ApproveProposal`, chamando `proposal.Reject()`.

---

### Contracting Service

**Localização:** `ContractingService/Application/UseCases/`

#### CreateContract

```
CreateContractCommand
  Guid ProposalId

CreateContractResponse
  Guid     Id
  Guid     ProposalId
  DateTime ContractedAt
```

Fluxo do handler:
1. Valida o Command via FluentValidation
2. Consulta `IProposalServiceGateway.GetProposalAsync(proposalId)`
3. Lança `ProposalNotFoundException` se `ProposalSnapshot` for null
4. Lança `ProposalNotApprovedException` se `Status != "Approved"`
5. Consulta `IContractRepository.GetByProposalIdAsync(proposalId)`
6. Lança `ProposalAlreadyContractedException` se já existe contrato
7. Chama `Contract.Create(proposalId)`
8. Persiste via `IContractRepository.AddAsync()`
9. Retorna `CreateContractResponse`

#### GetContract

```
GetContractQuery
  Guid Id

ContractResponse
  Guid     Id
  Guid     ProposalId
  DateTime ContractedAt
```

Fluxo do handler:
1. Busca via `IContractRepository.GetByIdAsync()`
2. Lança `ContractNotFoundException` se não encontrado
3. Mapeia para `ContractResponse`

---

## Estratégia de Erros

### Hierarquia de Exceções

```
DomainException                    ← base para erros do domínio
  ProposalStatusTransitionException  ← status inválido para a transição
  ContractDomainException            ← violação de invariante de contrato

ApplicationException (base)        ← base para erros de aplicação
  ProposalNotFoundException
  ContractNotFoundException
  ProposalNotApprovedException
  ProposalAlreadyContractedException
```

### Localização das Exceções

| Exceção | Camada | Localização |
|---------|--------|-------------|
| `DomainException` | Domain | `Domain/Exceptions/` |
| `ProposalStatusTransitionException` | Domain | `Domain/Exceptions/` |
| `ProposalNotFoundException` | Application | `Application/Exceptions/` |
| `ContractNotFoundException` | Application | `Application/Exceptions/` |
| `ProposalNotApprovedException` | Application | `Application/Exceptions/` |
| `ProposalAlreadyContractedException` | Application | `Application/Exceptions/` |

### Mapeamento HTTP

| Exceção | HTTP Status | Cenário |
|---------|-------------|---------|
| `ProposalNotFoundException` | 404 Not Found | Proposta não existe |
| `ContractNotFoundException` | 404 Not Found | Contratação não existe |
| `ProposalStatusTransitionException` | 422 Unprocessable Entity | Transição de status inválida |
| `ProposalNotApprovedException` | 422 Unprocessable Entity | Proposta não está aprovada |
| `ProposalAlreadyContractedException` | 409 Conflict | Proposta já contratada |
| `DomainException` | 422 Unprocessable Entity | Violação de invariante de domínio |
| `ValidationException` (FluentValidation) | 400 Bad Request | Payload de entrada inválido |
| `Exception` (não mapeada) | 500 Internal Server Error | Erros inesperados |

### Middleware Global

**Localização:** `Api/Middlewares/ExceptionHandlerMiddleware.cs`

- Captura todas as exceções não tratadas
- Mapeia para o status HTTP correto
- Retorna `ProblemDetails` (RFC 7807) padronizado:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Proposal with ID 'xxxxxxxx' was not found.",
  "instance": "/proposals/xxxxxxxx"
}
```

- Erros 500 não expõem detalhes internos em produção
- Todos os erros são logados antes de retornar ao cliente
