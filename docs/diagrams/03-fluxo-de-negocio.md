# Diagrama de Fluxo de Negócio

Representação dos fluxos de negócio da plataforma, do ciclo de vida da proposta até a efetivação do contrato.

---

## Ciclo de Vida da Proposta

```mermaid
stateDiagram-v2
    [*] --> UnderAnalysis : Proposal.Create()

    UnderAnalysis --> Approved : Proposal.Approve()\n[status == UnderAnalysis]
    UnderAnalysis --> Rejected : Proposal.Reject()\n[status == UnderAnalysis]

    Approved --> [*] : proposta pode ser contratada
    Rejected --> [*] : proposta encerrada

    Approved --> Contracted : Contract.Create()\n(via ContractingService)

    note right of UnderAnalysis
        Estado inicial de toda proposta.
        Única transição de saída permitida.
    end note

    note right of Approved
        Proposta elegível para contratação.
        Apenas uma contratação por proposta.
    end note

    note right of Rejected
        Estado terminal.
        Proposta não pode ser reativada.
    end note
```

---

## Fluxo Completo de Contratação

```mermaid
sequenceDiagram
    actor Cliente

    rect rgb(219, 234, 254)
        note over Cliente, ProposalService: Etapa 1 — Proposal Context
        Cliente->>+ProposalService: POST /api/proposals
        note right of ProposalService: Proposal.Create()\nStatus = UnderAnalysis\nProposalCreatedEvent registrado
        ProposalService-->>-Cliente: 201 Created\n{ id, status: "UnderAnalysis" }
    end

    rect rgb(220, 252, 231)
        note over Cliente, ProposalService: Etapa 2 — Aprovação
        Cliente->>+ProposalService: PATCH /api/proposals/{id}/approve
        note right of ProposalService: proposal.Approve()\nStatus = Approved\nProposalApprovedEvent registrado
        ProposalService-->>-Cliente: 204 No Content
    end

    rect rgb(254, 249, 195)
        note over Cliente, ContractingService: Etapa 3 — Contracting Context (via ACL)
        Cliente->>+ContractingService: POST /api/contracts\n{ proposalId }

        ContractingService->>+ProposalService: GET /api/proposals/{id}
        note right of ProposalService: consulta via IProposalServiceGateway\n(Anti-Corruption Layer)
        ProposalService-->>-ContractingService: 200 OK { status: "Approved" }

        note right of ContractingService: Valida: status == "Approved"\nValida: sem contrato duplicado\nContract.Create(proposalId)\nContractCreatedEvent registrado
        ContractingService-->>-Cliente: 201 Created\n{ id, proposalId, contractedAt }
    end
```

---

## Cenários de Erro no Fluxo de Contratação

```mermaid
flowchart TD
    START([Cliente: POST /api/contracts]) --> Q1{Proposta\nexiste?}

    Q1 -->|Não| E1["404 Not Found\nProposal not found"]
    Q1 -->|Sim| Q2{Status ==\nApproved?}

    Q2 -->|"UnderAnalysis\nou Rejected"| E2["422 Unprocessable Entity\nProposal is not approved"]
    Q2 -->|Sim| Q3{Já possui\ncontrato?}

    Q3 -->|Sim| E3["409 Conflict\nProposal already contracted"]
    Q3 -->|Não| SUCCESS["201 Created\n{ id, proposalId, contractedAt }"]

    style E1 fill:#fee2e2,stroke:#dc2626
    style E2 fill:#fef3c7,stroke:#d97706
    style E3 fill:#fce7f3,stroke:#db2777
    style SUCCESS fill:#dcfce7,stroke:#16a34a
    style START fill:#dbeafe,stroke:#2563eb
```

---

## Cenários de Erro na Transição de Status

```mermaid
flowchart TD
    APPROVE([PATCH /proposals/{id}/approve]) --> Q1{Proposta\nexiste?}
    REJECT([PATCH /proposals/{id}/reject]) --> Q1

    Q1 -->|Não| E1["404 Not Found"]
    Q1 -->|Sim| Q2{Status ==\nUnderAnalysis?}

    Q2 -->|"Approved\nou Rejected"| E2["422 Unprocessable Entity\nDomainException:\nCannot approve/reject a proposal\nwith status 'Approved'"]
    Q2 -->|Sim| OK["204 No Content"]

    style E1 fill:#fee2e2,stroke:#dc2626
    style E2 fill:#fef3c7,stroke:#d97706
    style OK fill:#dcfce7,stroke:#16a34a
```

---

## Regras de Negócio por Transição

| Operação | Pré-condição | Pós-condição | Evento registrado |
|----------|-------------|--------------|-------------------|
| `Proposal.Create()` | Parâmetros válidos (VO) | Status = `UnderAnalysis` | `ProposalCreatedEvent` |
| `Proposal.Approve()` | `Status == UnderAnalysis` | Status = `Approved` | `ProposalApprovedEvent` |
| `Proposal.Reject()` | `Status == UnderAnalysis` | Status = `Rejected` | `ProposalRejectedEvent` |
| `Contract.Create()` | Proposta `Approved`, sem contrato duplicado | Contrato criado com data UTC | `ContractCreatedEvent` |
