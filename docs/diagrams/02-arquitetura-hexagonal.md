# Diagrama de Arquitetura Hexagonal

Representação visual da Arquitetura Hexagonal (Ports and Adapters) aplicada em ambos os microsserviços.

---

## Visão do Hexágono — Estrutura Geral

```mermaid
flowchart TB
    subgraph PRIMARY_ADAPTERS["Primary Adapters (Entrada)"]
        CTRL["ProposalsController\nContractsController\n(Api/Controllers)"]
    end

    subgraph HEXAGON["Hexágono — Núcleo da Aplicação"]
        direction TB

        subgraph APPLICATION["Application Layer"]
            PP["Primary Ports\nICommandHandler‹T,R›\nIQueryHandler‹T,R›"]
            UC["Use Cases\nCommands · Queries · Handlers"]
            SP_APP["Secondary Ports (ACL)\nIProposalServiceGateway"]
        end

        subgraph DOMAIN["Domain Layer"]
            AGG["Aggregates\nProposal · Contract"]
            VO["Value Objects\nCustomerName · InsuranceType\nCoverageAmount"]
            DE["Domain Events\nProposalCreatedEvent\nContractCreatedEvent"]
            SP_DOM["Secondary Ports\nIProposalRepository\nIContractRepository"]
        end
    end

    subgraph SECONDARY_ADAPTERS["Secondary Adapters (Saída)"]
        REPO["ProposalRepository\nContractRepository\n(Infrastructure/Persistence)"]
        GW["ProposalServiceGateway\n(Infrastructure/Gateways)"]
    end

    subgraph EXTERNAL["Externos"]
        PG[("PostgreSQL")]
        HTTP["ProposalService\n(HTTP REST)"]
    end

    CTRL -->|"mapeia request\npara Command/Query"| PP
    PP --> UC
    UC --> AGG
    AGG --> VO
    AGG --> DE
    UC --> SP_DOM
    UC --> SP_APP
    SP_DOM -->|"implementado por"| REPO
    SP_APP -->|"implementado por"| GW
    REPO --- PG
    GW --- HTTP

    style PRIMARY_ADAPTERS fill:#dbeafe,stroke:#2563eb,color:#1e3a5f
    style HEXAGON fill:#f0fdf4,stroke:#16a34a
    style APPLICATION fill:#dcfce7,stroke:#15803d
    style DOMAIN fill:#bbf7d0,stroke:#166534
    style SECONDARY_ADAPTERS fill:#fef3c7,stroke:#d97706,color:#451a03
    style EXTERNAL fill:#f3f4f6,stroke:#9ca3af
```

---

## Direção de Dependências entre Projetos

```mermaid
flowchart LR
    API["*.Api"]
    APP["*.Application"]
    INF["*.Infrastructure"]
    DOM["*.Domain"]

    API -->|"referencia"| APP
    API -->|"referencia (só DI)"| INF
    APP -->|"referencia"| DOM
    INF -->|"referencia"| APP
    INF -->|"referencia"| DOM

    style DOM fill:#bbf7d0,stroke:#166534
    style APP fill:#dcfce7,stroke:#15803d
    style INF fill:#fef3c7,stroke:#d97706
    style API fill:#dbeafe,stroke:#2563eb
```

> A seta indica "depende de". O `Domain` não possui setas de saída — zero dependências externas.

---

## Mapa de Ports e Adapters

```mermaid
flowchart LR
    subgraph "Primary Side"
        HTTP_IN["HTTP Request"]
        CTRL_P["ProposalsController\n(Primary Adapter)"]
        CTRL_C["ContractsController\n(Primary Adapter)"]
    end

    subgraph "Primary Ports"
        ICMD["ICommandHandler‹T,R›"]
        IQRY["IQueryHandler‹T,R›"]
    end

    subgraph "Core"
        HANDLERS["Command Handlers\nQuery Handlers"]
        DOMAIN_CORE["Domain\nAggregates · VOs · Events"]
    end

    subgraph "Secondary Ports"
        IREPO_P["IProposalRepository\n(Domain)"]
        IREPO_C["IContractRepository\n(Domain)"]
        IGWY["IProposalServiceGateway\n(Application)"]
    end

    subgraph "Secondary Side"
        REPO_P["ProposalRepository\n(Secondary Adapter)"]
        REPO_C["ContractRepository\n(Secondary Adapter)"]
        GWY["ProposalServiceGateway\n(Secondary Adapter)"]
    end

    subgraph "External"
        DB_P[("Proposal DB")]
        DB_C[("Contracting DB")]
        SVC["ProposalService API"]
    end

    HTTP_IN --> CTRL_P & CTRL_C
    CTRL_P -->|"usa"| ICMD & IQRY
    CTRL_C -->|"usa"| ICMD & IQRY
    ICMD & IQRY --> HANDLERS
    HANDLERS --> DOMAIN_CORE
    HANDLERS -->|"usa"| IREPO_P & IREPO_C & IGWY
    IREPO_P -->|"implementado por"| REPO_P
    IREPO_C -->|"implementado por"| REPO_C
    IGWY -->|"implementado por"| GWY
    REPO_P --> DB_P
    REPO_C --> DB_C
    GWY --> SVC
```

---

## Referência de Localização dos Artefatos

| Artefato | Tipo | Camada | Localização no Projeto |
|----------|------|--------|----------------------|
| `ICommandHandler<,>` | Primary Port | Application | `Application/Common/` |
| `IQueryHandler<,>` | Primary Port | Application | `Application/Common/` |
| `IProposalRepository` | Secondary Port | Domain | `Domain/Repositories/` |
| `IContractRepository` | Secondary Port | Domain | `Domain/Repositories/` |
| `IProposalServiceGateway` | Secondary Port (ACL) | Application | `Application/Ports/` |
| `ProposalsController` | Primary Adapter | Api | `Api/Controllers/` |
| `ContractsController` | Primary Adapter | Api | `Api/Controllers/` |
| `ProposalRepository` | Secondary Adapter | Infrastructure | `Infrastructure/Persistence/Repositories/` |
| `ContractRepository` | Secondary Adapter | Infrastructure | `Infrastructure/Persistence/Repositories/` |
| `ProposalServiceGateway` | Secondary Adapter (ACL) | Infrastructure | `Infrastructure/Gateways/` |
