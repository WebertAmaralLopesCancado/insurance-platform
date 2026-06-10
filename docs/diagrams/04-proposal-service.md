# Diagrama — Proposal Service

Visão interna completa do ProposalService: estrutura de camadas, agregado, casos de uso e fluxos.

---

## Estrutura de Camadas

```mermaid
flowchart TB
    subgraph API["Api Layer — InsurancePlatform.ProposalService.Api"]
        CTRL["ProposalsController\n(Primary Adapter)"]
        MDW["ExceptionHandlerMiddleware"]
        REQ["CreateProposalRequest\nPaginationRequest"]
    end

    subgraph APP["Application Layer — InsurancePlatform.ProposalService.Application"]
        subgraph COMMON["Common"]
            ICMD["ICommandHandler‹T,R›"]
            IQRY["IQueryHandler‹T,R›"]
            RESULT["Result"]
            PAGED["PagedResponse‹T›"]
        end
        subgraph USECASES["Use Cases"]
            UC1["CreateProposal\nCommand + Handler + Response"]
            UC2["GetProposal\nQuery + Handler + Response"]
            UC3["GetAllProposals\nQuery + Handler + Response"]
            UC4["ApproveProposal\nCommand + Handler"]
            UC5["RejectProposal\nCommand + Handler"]
        end
        EXC["Exceptions\nNotFoundException\nConflictException\nValidationException"]
    end

    subgraph DOM["Domain Layer — InsurancePlatform.ProposalService.Domain"]
        subgraph SEED["SeedWork"]
            ENT["Entity‹TId›"]
            AGR["AggregateRoot‹TId›"]
            VO_BASE["ValueObject"]
            IDE["IDomainEvent"]
        end
        subgraph AGG["Aggregates/Proposals"]
            PROP["Proposal\n(Aggregate Root)"]
            STATUS["ProposalStatus\n(Enum)"]
            subgraph EVENTS["Events"]
                EVT1["ProposalCreatedEvent"]
                EVT2["ProposalApprovedEvent"]
                EVT3["ProposalRejectedEvent"]
            end
        end
        subgraph VOS["ValueObjects"]
            CN["CustomerName"]
            IT["InsuranceType"]
            CA["CoverageAmount"]
        end
        subgraph REPOS["Repositories"]
            IREPO["IProposalRepository\n(Secondary Port)"]
        end
        DEXC["DomainException"]
    end

    subgraph INF["Infrastructure Layer — InsurancePlatform.ProposalService.Infrastructure"]
        subgraph PERSIST["Persistence"]
            CTX["ProposalDbContext"]
            MAP["ProposalMapping\n(IEntityTypeConfiguration)"]
            REPO["ProposalRepository\n(Secondary Adapter)"]
            MIG["Migrations/"]
        end
        DI["InfrastructureServiceCollectionExtensions"]
    end

    DB[("PostgreSQL\nproposals table")]

    CTRL --> ICMD & IQRY
    ICMD & IQRY --> UC1 & UC2 & UC3 & UC4 & UC5
    UC1 & UC4 & UC5 --> PROP
    PROP --> AGR
    AGR --> ENT
    PROP --> VOS
    PROP --> EVENTS
    UC1 & UC2 & UC3 & UC4 & UC5 --> IREPO
    IREPO -->|"implementado por"| REPO
    REPO --> CTX
    CTX --> MAP
    CTX --> DB

    style API fill:#dbeafe,stroke:#2563eb
    style APP fill:#dcfce7,stroke:#16a34a
    style DOM fill:#bbf7d0,stroke:#166534
    style INF fill:#fef3c7,stroke:#d97706
```

---

## Modelo do Agregado Proposal

```mermaid
classDiagram
    class AggregateRoot~Guid~ {
        +IReadOnlyCollection~IDomainEvent~ DomainEvents
        #AddDomainEvent(IDomainEvent)
        +ClearDomainEvents()
    }

    class Proposal {
        -Guid id
        -CustomerName customerName
        -InsuranceType insuranceType
        -CoverageAmount coverageAmount
        -ProposalStatus status
        -DateTime createdAt
        +Guid Id
        +CustomerName CustomerName
        +InsuranceType InsuranceType
        +CoverageAmount CoverageAmount
        +ProposalStatus Status
        +DateTime CreatedAt
        +Create(CustomerName, InsuranceType, CoverageAmount)$ Proposal
        +Approve() void
        +Reject() void
        -EnsureCanChangeStatus(string) void
    }

    class ProposalStatus {
        <<enumeration>>
        UnderAnalysis
        Approved
        Rejected
    }

    class CustomerName {
        +string Value
        +CustomerName(string value)
        #GetEqualityComponents() IEnumerable
    }

    class InsuranceType {
        +string Value
        +InsuranceType(string value)
        #GetEqualityComponents() IEnumerable
    }

    class CoverageAmount {
        +decimal Value
        +CoverageAmount(decimal value)
        #GetEqualityComponents() IEnumerable
    }

    class ValueObject {
        <<abstract>>
        #GetEqualityComponents()* IEnumerable
        +Equals(object?) bool
        +GetHashCode() int
    }

    AggregateRoot~Guid~ <|-- Proposal
    Proposal --> ProposalStatus
    Proposal --> CustomerName
    Proposal --> InsuranceType
    Proposal --> CoverageAmount
    ValueObject <|-- CustomerName
    ValueObject <|-- InsuranceType
    ValueObject <|-- CoverageAmount
```

---

## Endpoints REST

```mermaid
flowchart LR
    subgraph "ProposalService API :5001"
        direction TB
        EP1["POST /api/proposals\n→ 201 Created"]
        EP2["GET /api/proposals/{id}\n→ 200 OK | 404 Not Found"]
        EP3["GET /api/proposals?pageNumber&pageSize\n→ 200 OK (paginado)"]
        EP4["PATCH /api/proposals/{id}/approve\n→ 204 | 404 | 422"]
        EP5["PATCH /api/proposals/{id}/reject\n→ 204 | 404 | 422"]
        EP6["GET /health → 200 OK"]
        EP7["GET /swagger → Docs"]
    end
```

---

## Fluxo Interno — CreateProposal

```mermaid
sequenceDiagram
    participant C as ProposalsController
    participant H as CreateProposalCommandHandler
    participant P as Proposal (Domain)
    participant R as IProposalRepository
    participant DB as PostgreSQL

    C->>H: HandleAsync(CreateProposalCommand)
    H->>P: Proposal.Create(CustomerName, InsuranceType, CoverageAmount)
    note right of P: Valida VOs\nStatus = UnderAnalysis\nRegistra ProposalCreatedEvent
    P-->>H: proposal
    H->>R: AddAsync(proposal)
    R->>DB: INSERT INTO proposals
    DB-->>R: OK
    R-->>H: OK
    H-->>C: CreateProposalResponse { id, status, ... }
    C-->>C: 201 Created
```

---

## Fluxo Interno — ApproveProposal

```mermaid
sequenceDiagram
    participant C as ProposalsController
    participant H as ApproveProposalCommandHandler
    participant P as Proposal (Domain)
    participant R as IProposalRepository
    participant DB as PostgreSQL

    C->>H: HandleAsync(ApproveProposalCommand { id })
    H->>R: GetByIdAsync(id)
    R->>DB: SELECT * FROM proposals WHERE id = ?
    DB-->>R: proposal row
    R-->>H: Proposal

    alt Proposal não encontrada
        H-->>C: throws NotFoundException
        C-->>C: 404 Not Found
    else Status != UnderAnalysis
        H->>P: proposal.Approve()
        P-->>H: throws DomainException
        C-->>C: 422 Unprocessable Entity
    else Caminho feliz
        H->>P: proposal.Approve()
        note right of P: Status = Approved\nRegistra ProposalApprovedEvent
        P-->>H: OK
        H->>R: UpdateAsync(proposal)
        R->>DB: UPDATE proposals SET status = 'Approved'
        DB-->>R: OK
        R-->>H: OK
        H-->>C: Result.Success()
        C-->>C: 204 No Content
    end
```
