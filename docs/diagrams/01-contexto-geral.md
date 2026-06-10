# Diagrama de Contexto Geral

Visão de alto nível da plataforma de seguros, seus atores e a relação entre os microsserviços.

---

## Contexto do Sistema

```mermaid
C4Context
    title Contexto Geral — Insurance Platform

    Person(cliente, "Cliente", "Realiza propostas de seguro e efetiva contratações via API REST")

    System_Boundary(platform, "Insurance Platform") {
        System(proposalService, "Proposal Service", "Gerencia o ciclo de vida das propostas: criação, consulta, aprovação e rejeição")
        System(contractingService, "Contracting Service", "Efetiva contratações de propostas aprovadas e consulta contratos")
    }

    SystemDb(proposalDb, "Proposal Database", "PostgreSQL — armazena propostas e seus status")
    SystemDb(contractingDb, "Contracting Database", "PostgreSQL — armazena contratos efetivados")

    Rel(cliente, proposalService, "POST /proposals, PATCH /approve, PATCH /reject, GET /proposals", "HTTP REST")
    Rel(cliente, contractingService, "POST /contracts, GET /contracts", "HTTP REST")
    Rel(contractingService, proposalService, "GET /proposals/{id}", "HTTP REST (ACL)")
    Rel(proposalService, proposalDb, "Leitura e escrita de propostas", "EF Core")
    Rel(contractingService, contractingDb, "Leitura e escrita de contratos", "EF Core")
```

---

## Descrição dos Componentes

| Componente | Tipo | Responsabilidade |
|------------|------|-----------------|
| **Cliente** | Ator externo | Consome os endpoints REST de ambos os serviços |
| **Proposal Service** | Microsserviço | Ciclo de vida completo das propostas de seguro |
| **Contracting Service** | Microsserviço | Contratação de propostas aprovadas |
| **Proposal Database** | Banco de dados | Persistência das propostas (PostgreSQL) |
| **Contracting Database** | Banco de dados | Persistência dos contratos (PostgreSQL) |

---

## Diagrama Simplificado de Contexto

```mermaid
flowchart TB
    subgraph Atores
        CLI([Cliente])
    end

    subgraph "Insurance Platform"
        direction LR
        PS["Proposal Service\n:5001"]
        CS["Contracting Service\n:5002"]
    end

    subgraph "Persistência"
        PDB[(Proposal DB\nPostgreSQL)]
        CDB[(Contracting DB\nPostgreSQL)]
    end

    CLI -->|"HTTP REST"| PS
    CLI -->|"HTTP REST"| CS
    CS -->|"GET /proposals/{id}\n(ACL)"| PS
    PS --- PDB
    CS --- CDB

    style PS fill:#dbeafe,stroke:#2563eb
    style CS fill:#dcfce7,stroke:#16a34a
    style PDB fill:#fef9c3,stroke:#ca8a04
    style CDB fill:#fef9c3,stroke:#ca8a04
```

---

## Regras de Comunicação

- O **Cliente** se comunica diretamente com ambos os serviços via HTTP REST
- O **Contracting Service** consulta o **Proposal Service** via ACL (`IProposalServiceGateway`) para verificar o status de uma proposta antes de criar um contrato
- Os dois serviços possuem **bancos de dados independentes** — não há acesso cruzado entre os bancos
- O `ContractingService` nunca acessa diretamente o banco do `ProposalService`
