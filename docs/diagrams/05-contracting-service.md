# Diagrama — Contracting Service

Visão interna completa do ContractingService: estrutura de camadas, agregado, ACL, casos de uso e fluxos.

---

## Estrutura de Camadas

```mermaid
flowchart TB
    subgraph API["Api Layer — InsurancePlatform.ContractingService.Api"]
        CTRL["ContractsController\n(Primary Adapter)"]
        MDW["ExceptionHandlerMiddleware"]
        REQ["CreateContractRequest"]
    end

    subgraph APP["Application Layer — InsurancePlatform.ContractingService.Application"]
        subgraph COMMON["Common"]
            ICMD["ICommandHandler‹T,R›"]
            IQRY["IQueryHandler‹T,R›"]
            RESULT["Result"]
            PAGED["PagedResponse‹T›"]
        end
        subgraph USECASES["Use Cases"]
            UC1["CreateContract\nCommand + Handler + Response"]
            UC2["GetContract\nQuery + Handler + Response"]
            UC3["GetAllContracts\nQuery + Handler + Response"]
        end
        subgraph ACL_PORT["Ports (ACL)"]
            IGWY["IProposalServiceGateway\n(Secondary Port — ACL)"]
            SNAP["ProposalSnapshot\n(DTO interno)"]
        end
        EXC["Exceptions\nNotFoundException\nConflictException\nValidationException"]
    end

    subgraph DOM["Domain Layer — InsurancePlatform.ContractingService.Domain"]
        subgraph SEED["SeedWork"]
            ENT["Entity‹TId›"]
            AGR["AggregateRoot‹TId›"]
            VO_BASE["ValueObject"]
            IDE["IDomainEvent"]
        end
        subgraph AGG["Aggregates/Contracts"]
            CONT["Contract\n(Aggregate Root)"]
            subgraph EVENTS["Events"]
                EVT1["ContractCreatedEvent"]
            end
        end
        subgraph REPOS["Repositories"]
            IREPO["IContractRepository\n(Secondary Port)"]
        end
        DEXC["DomainException"]
    end

    subgraph INF["Infrastructure Layer — InsurancePlatform.ContractingService.Infrastructure"]
        subgraph PERSIST["Persistence"]
            CTX["ContractDbContext"]
            MAP["ContractMapping\n(IEntityTypeConfiguration)"]
            REPO["ContractRepository\n(Secondary Adapter)"]
            MIG["Migrations/"]
        end
        subgraph GW["Gateways"]
            GWY["ProposalServiceGateway\n(Secondary Adapter — ACL)"]
            HTTP_CLI["HttpClient\n(ProposalService API)"]
        end
        DI["InfrastructureServiceCollectionExtensions"]
    end

    DB[("PostgreSQL\ncontracts table")]
    EXT_SVC["ProposalService\n:5001"]

    CTRL --> ICMD & IQRY
    ICMD & IQRY --> UC1 & UC2 & UC3
    UC1 --> CONT
    UC1 --> IGWY
    CONT --> AGR
    AGR --> ENT
    CONT --> EVENTS
    UC1 & UC2 & UC3 --> IREPO
    IREPO -->|"implementado por"| REPO
    IGWY -->|"implementado por"| GWY
    REPO --> CTX
    CTX --> MAP
    CTX --> DB
    GWY --> HTTP_CLI
    HTTP_CLI --> EXT_SVC

    style API fill:#dbeafe,stroke:#2563eb
    style APP fill:#dcfce7,stroke:#16a34a
    style DOM fill:#bbf7d0,stroke:#166534
    style INF fill:#fef3c7,stroke:#d97706
```

---

## Modelo do Agregado Contract

```mermaid
classDiagram
    class AggregateRoot~Guid~ {
        +IReadOnlyCollection~IDomainEvent~ DomainEvents
        #AddDomainEvent(IDomainEvent)
        +ClearDomainEvents()
    }

    class Contract {
        -Guid id
        -Guid proposalId
        -DateTime contractedAt
        +Guid Id
        +Guid ProposalId
        +DateTime ContractedAt
        +Create(Guid proposalId)$ Contract
    }

    class ContractCreatedEvent {
        +Guid ContractId
        +Guid ProposalId
        +DateTime OccurredOnUtc
    }

    class IDomainEvent {
        <<interface>>
        +DateTime OccurredOnUtc
    }

    AggregateRoot~Guid~ <|-- Contract
    IDomainEvent <|.. ContractCreatedEvent
    Contract --> ContractCreatedEvent : registra
```

---

## Anti-Corruption Layer (ACL)

```mermaid
flowchart LR
    subgraph ContractingService
        HC["CreateContractCommandHandler"]
        PORT["IProposalServiceGateway\n(Application/Ports)"]
        SNAP["ProposalSnapshot\n{ Id, Status }"]
        GWY["ProposalServiceGateway\n(Infrastructure/Gateways)"]
    end

    subgraph ProposalService
        PAPI["GET /api/proposals/{id}"]
        PRESP["ProposalResponse\n{ id, customerName, insuranceType,\ncoverageAmount, status, createdAt }"]
    end

    HC -->|"usa porta"| PORT
    PORT -->|"implementado por"| GWY
    GWY -->|"HTTP GET"| PAPI
    PAPI -->|"200 OK JSON"| PRESP
    PRESP -->|"mapeia para"| SNAP
    SNAP -->|"retorna para"| HC

    note1["A ACL isola o ContractingService\ndo modelo interno do ProposalService.\nSó ProposalSnapshot cruza a fronteira."]

    style PORT fill:#dcfce7,stroke:#16a34a
    style SNAP fill:#fef3c7,stroke:#d97706
    style GWY fill:#fef3c7,stroke:#d97706
```

---

## Endpoints REST

```mermaid
flowchart LR
    subgraph "ContractingService API :5002"
        direction TB
        EP1["POST /api/contracts\n{ proposalId }\n→ 201 Created | 404 | 409 | 422"]
        EP2["GET /api/contracts/{id}\n→ 200 OK | 404 Not Found"]
        EP3["GET /api/contracts?pageNumber&pageSize\n→ 200 OK (paginado)"]
        EP4["GET /health → 200 OK"]
        EP5["GET /swagger → Docs"]
    end
```

---

## Fluxo Interno — CreateContract

```mermaid
sequenceDiagram
    participant C as ContractsController
    participant H as CreateContractCommandHandler
    participant GW as IProposalServiceGateway
    participant PS as ProposalService API
    participant R as IContractRepository
    participant CON as Contract (Domain)
    participant DB as PostgreSQL

    C->>H: HandleAsync(CreateContractCommand { proposalId })

    H->>GW: GetByIdAsync(proposalId)
    GW->>PS: GET /api/proposals/{proposalId}

    alt Proposta não encontrada (404)
        PS-->>GW: 404 Not Found
        GW-->>H: null (ProposalSnapshot)
        H-->>C: throws NotFoundException
        C-->>C: 404 Not Found
    else Proposta encontrada
        PS-->>GW: 200 OK { status: "Approved" }
        GW-->>H: ProposalSnapshot { id, status: "Approved" }
    end

    alt Status != "Approved"
        H-->>C: throws DomainException
        C-->>C: 422 Unprocessable Entity
    end

    H->>R: ExistsForProposalAsync(proposalId)
    R->>DB: SELECT 1 FROM contracts WHERE proposal_id = ?
    DB-->>R: existe / não existe

    alt Já existe contrato para esta proposta
        H-->>C: throws ConflictException
        C-->>C: 409 Conflict
    end

    H->>CON: Contract.Create(proposalId)
    note right of CON: contractedAt = DateTime.UtcNow\nRegistra ContractCreatedEvent
    CON-->>H: contract

    H->>R: AddAsync(contract)
    R->>DB: INSERT INTO contracts
    DB-->>R: OK
    R-->>H: OK

    H-->>C: CreateContractResponse { id, proposalId, contractedAt }
    C-->>C: 201 Created
```

---

## Validações no CreateContract

```mermaid
flowchart TD
    START([POST /api/contracts]) --> V1{proposalId\nválido?}

    V1 -->|Não| ERR1["400 Bad Request\nCampo obrigatório"]
    V1 -->|Sim| V2{Proposta\nexiste?}

    V2 -->|Não| ERR2["404 Not Found\nProposal not found"]
    V2 -->|Sim| V3{Status ==\n'Approved'?}

    V3 -->|Não| ERR3["422 Unprocessable Entity\nProposal is not approved"]
    V3 -->|Sim| V4{Contrato\njá existe?}

    V4 -->|Sim| ERR4["409 Conflict\nProposal already contracted"]
    V4 -->|Não| SUCCESS["201 Created\n{ id, proposalId, contractedAt }"]

    style ERR1 fill:#fee2e2,stroke:#dc2626
    style ERR2 fill:#fee2e2,stroke:#dc2626
    style ERR3 fill:#fef3c7,stroke:#d97706
    style ERR4 fill:#fce7f3,stroke:#db2777
    style SUCCESS fill:#dcfce7,stroke:#16a34a
    style START fill:#dbeafe,stroke:#2563eb
```
