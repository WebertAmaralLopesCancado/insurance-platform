# Arquitetura Hexagonal (Ports and Adapters)

## Objetivo

Manter o domínio completamente isolado de detalhes externos como banco de dados, APIs, frameworks, mensageria e infraestrutura.

A regra central é: **nenhuma dependência aponta para dentro do núcleo (Domain). Todas as dependências externas dependem do núcleo, nunca o contrário.**

---

## Princípio de Dependência

```
Api  →  Application  →  Domain
Infrastructure  →  Application
Infrastructure  →  Domain
```

```
Domain          — não referencia nenhuma camada
Application     — referencia apenas Domain
Infrastructure  — referencia Application e Domain
Api             — referencia Application e Infrastructure (apenas para DI)
```

---

## Visão do Hexágono

```
                    ┌─────────────────────────────────────────┐
                    │              Primary Adapters            │
                    │         (Controllers, REST, CLI)         │
                    └──────────────────┬──────────────────────┘
                                       │
                                       ▼  Primary Ports (Input)
                    ┌─────────────────────────────────────────┐
                    │              Application                 │
                    │  ┌───────────────────────────────────┐  │
                    │  │  Commands / Queries / Handlers     │  │
                    │  └───────────────────────────────────┘  │
                    │                   │                      │
                    │                   ▼                      │
                    │  ┌───────────────────────────────────┐  │
                    │  │              Domain                │  │
                    │  │  Aggregates / ValueObjects /       │  │
                    │  │  Domain Events / Repositories(I)   │  │
                    │  └───────────────────────────────────┘  │
                    │                   │                      │
                    │                   ▼  Secondary Ports (Output)
                    └──────────────────┬──────────────────────┘
                                       │
                                       ▼
                    ┌─────────────────────────────────────────┐
                    │             Secondary Adapters           │
                    │    (EF Core, HTTP Client, PostgreSQL)    │
                    └─────────────────────────────────────────┘
```

---

## Ports

### Primary Ports (Input Ports)

Definem o contrato de entrada da aplicação. São as interfaces que expõem os casos de uso para o mundo externo.

**Localização:** `Application/Common/`

| Interface | Propósito |
|-----------|-----------|
| `ICommandHandler<TCommand>` | Contrato base para handlers de Commands (sem retorno) |
| `ICommandHandler<TCommand, TResponse>` | Contrato base para handlers de Commands (com retorno) |
| `IQueryHandler<TQuery, TResponse>` | Contrato base para handlers de Queries |

Os casos de uso concretos (ex: `CreateProposalCommandHandler`) implementam essas interfaces. O `Controller` recebe o handler injetado via interface.

---

### Secondary Ports — Domain Layer (Output Ports de Persistência)

Definem os contratos de acesso a dados. São interfaces definidas no `Domain` e implementadas na `Infrastructure`.

**Localização:** `Domain/Repositories/`

| Interface | Microsserviço | Responsabilidade |
|-----------|---------------|------------------|
| `IProposalRepository` | ProposalService | Persistência e consulta de `Proposal` |
| `IContractRepository` | ContractingService | Persistência e consulta de `Contract` |

**Métodos esperados em `IProposalRepository`:**

```
Task<Proposal?> GetByIdAsync(Guid id)
Task<IReadOnlyList<Proposal>> GetAllAsync(int pageNumber, int pageSize)
Task<int> CountAsync()
Task AddAsync(Proposal proposal)
Task UpdateAsync(Proposal proposal)
```

**Métodos esperados em `IContractRepository`:**

```
Task<Contract?> GetByIdAsync(Guid id)
Task<Contract?> GetByProposalIdAsync(Guid proposalId)
Task AddAsync(Contract contract)
```

---

### Secondary Port — Application Layer (Output Port de Integração)

Define o contrato de comunicação com serviço externo (ACL).

**Localização:** `ContractingService/Application/Ports/`

| Interface | Microsserviço | Responsabilidade |
|-----------|---------------|------------------|
| `IProposalServiceGateway` | ContractingService | Consulta status de proposta no ProposalService via HTTP |

**Contrato:**

```
Task<ProposalSnapshot?> GetProposalAsync(Guid proposalId)
```

**DTO de travessia (Application/Ports):**

```
ProposalSnapshot
  Guid   Id
  string Status
```

---

## Adapters

### Primary Adapters (Adaptadores de Entrada)

Recebem requisições externas e as traduzem para chamadas nos casos de uso via Primary Ports.

**Localização:** `Api/Controllers/`

| Adapter | Microsserviço | Descrição |
|---------|---------------|-----------|
| `ProposalsController` | ProposalService | Endpoints REST de proposta |
| `ContractsController` | ContractingService | Endpoints REST de contratação |

**Responsabilidades dos Controllers:**
- Receber o request HTTP
- Mapear o DTO de entrada para um `Command` ou `Query`
- Delegar ao handler via interface injetada
- Mapear o resultado para resposta HTTP com status code correto

**O Controller não contém lógica de negócio.**

---

### Secondary Adapters (Adaptadores de Saída)

Implementam os Secondary Ports. Lidam com detalhes externos como banco de dados e HTTP.

**Localização:** `Infrastructure/`

#### Adapters de Persistência

| Adapter | Interface implementada | Localização |
|---------|-----------------------|-------------|
| `ProposalRepository` | `IProposalRepository` | `ProposalService/Infrastructure/Persistence/Repositories/` |
| `ContractRepository` | `IContractRepository` | `ContractingService/Infrastructure/Persistence/Repositories/` |

- Utiliza EF Core com `DbContext` próprio por microsserviço
- Mapeamentos via Fluent API em classes separadas (`IEntityTypeConfiguration<T>`)
- Migrations versionadas por serviço

#### Adapter de Integração (ACL)

| Adapter | Interface implementada | Localização |
|---------|-----------------------|-------------|
| `ProposalServiceGateway` | `IProposalServiceGateway` | `ContractingService/Infrastructure/Gateways/` |

- Realiza chamada `GET /proposals/{id}` ao ProposalService
- Utiliza `HttpClient` com `IHttpClientFactory`
- Deserializa a resposta para `ProposalSnapshot` (DTO interno ao ContractingService)
- Retorna `null` quando proposta não encontrada (HTTP 404)
- Lança exceção de infraestrutura em falhas de comunicação

---

## Fluxo Completo — CreateContract

```
HTTP POST /contracts
    │
    ▼
ContractsController (Primary Adapter)
    │  mapeia request para CreateContractCommand
    ▼
CreateContractCommandHandler (Application — Primary Port)
    │  consulta IProposalServiceGateway.GetProposalAsync(proposalId)
    │         │
    │         ▼
    │  ProposalServiceGateway (Secondary Adapter)
    │         │  GET http://proposal-service/proposals/{id}
    │         ▼
    │  ProposalService retorna JSON
    │         │  deserializa para ProposalSnapshot
    │         ◄──────────────────────────────────────
    │
    │  verifica ProposalSnapshot.Status == "Approved"
    │  consulta IContractRepository.GetByProposalIdAsync(proposalId)
    │         │
    │         ▼
    │  ContractRepository (Secondary Adapter)
    │         │  SELECT ... FROM contracts WHERE proposal_id = ...
    │         ◄──────────────────────────────────────
    │
    │  valida que não existe contrato duplicado
    │  chama Contract.Create(proposalId, DateTime.UtcNow)
    │  publica ContractCreatedEvent
    │  salva via IContractRepository.AddAsync(contract)
    │
    ▼
ContractsController
    │  retorna HTTP 201 Created com ContractResponse
    ▼
HTTP Response
```

---

## Fluxo Completo — CreateProposal

```
HTTP POST /proposals
    │
    ▼
ProposalsController (Primary Adapter)
    │  mapeia request para CreateProposalCommand
    ▼
CreateProposalCommandHandler (Application — Primary Port)
    │  chama Proposal.Create(customerName, insuranceType, coverageAmount)
    │  publica ProposalCreatedEvent
    │  salva via IProposalRepository.AddAsync(proposal)
    ▼
ProposalsController
    │  retorna HTTP 201 Created com ProposalResponse
    ▼
HTTP Response
```

---

## Mapa de Localização: Interfaces e Implementações

| Artefato | Tipo | Camada | Localização |
|----------|------|--------|-------------|
| `ICommandHandler<TCommand>` | Primary Port | Application | `Application/Common/` |
| `ICommandHandler<TCommand, TResponse>` | Primary Port | Application | `Application/Common/` |
| `IQueryHandler<TQuery, TResponse>` | Primary Port | Application | `Application/Common/` |
| `IProposalRepository` | Secondary Port | Domain | `Domain/Repositories/` |
| `IContractRepository` | Secondary Port | Domain | `Domain/Repositories/` |
| `IProposalServiceGateway` | Secondary Port (ACL) | Application | `Application/Ports/` |
| `ProposalsController` | Primary Adapter | Api | `Api/Controllers/` |
| `ContractsController` | Primary Adapter | Api | `Api/Controllers/` |
| `ProposalRepository` | Secondary Adapter | Infrastructure | `Infrastructure/Persistence/Repositories/` |
| `ContractRepository` | Secondary Adapter | Infrastructure | `Infrastructure/Persistence/Repositories/` |
| `ProposalServiceGateway` | Secondary Adapter (ACL) | Infrastructure | `Infrastructure/Gateways/` |
| `ProposalDbContext` | Secondary Adapter | Infrastructure | `Infrastructure/Persistence/` |
| `ContractingDbContext` | Secondary Adapter | Infrastructure | `Infrastructure/Persistence/` |

---

## Regras de Dependência entre Projetos

| Projeto | Referencia |
|---------|-----------|
| `*.Domain` | _(nenhum projeto interno)_ |
| `*.Application` | `*.Domain` |
| `*.Infrastructure` | `*.Application` + `*.Domain` |
| `*.Api` | `*.Application` + `*.Infrastructure` |

> `*.Api` referencia `Infrastructure` **exclusivamente** para registrar as implementações no container de DI (`builder.Services.AddInfrastructure()`). Nenhuma classe de negócio na `Api` depende diretamente de classes de `Infrastructure`.
