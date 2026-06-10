# DDD e Bounded Contexts

## Domínio

**Plataforma de Seguros**

O sistema é responsável por gerenciar propostas de seguro e suas respectivas contratações.

---

## Bounded Contexts

Foram identificados dois contextos delimitados independentes:

```
┌─────────────────────────────────┐     HTTP/ACL      ┌──────────────────────────────────┐
│        Proposal Context         │ ◄──────────────── │      Contracting Context          │
│                                 │                    │                                  │
│  Aggregate Root: Proposal       │                    │  Aggregate Root: Contract        │
└─────────────────────────────────┘                    └──────────────────────────────────┘
```

A comunicação entre contextos ocorre exclusivamente via `IProposalServiceGateway`, que abstrai o contrato HTTP entre os dois serviços. O `ContractingContext` nunca referencia diretamente entidades do `ProposalContext`.

---

## Proposal Context

### Responsabilidades

- Criar proposta
- Consultar proposta
- Listar propostas com paginação
- Aprovar proposta
- Rejeitar proposta

### Aggregate Root: Proposal

Controla o ciclo de vida da proposta e protege todas as invariantes.

**Comportamentos (modelo rico):**

| Método | Descrição | Guarda |
|--------|-----------|--------|
| `Proposal.Create(customerName, insuranceType, coverageAmount)` | Cria nova proposta com status `UnderAnalysis` | Valida todos os parâmetros |
| `Proposal.Approve()` | Transiciona status para `Approved` | Rejeita se status != `UnderAnalysis` |
| `Proposal.Reject()` | Transiciona status para `Rejected` | Rejeita se status != `UnderAnalysis` |

**Propriedades:**

| Propriedade | Tipo | Observação |
|-------------|------|------------|
| `Id` | `Guid` | Gerado no construtor |
| `CustomerName` | `CustomerName` (VO) | Não nulo, não vazio |
| `InsuranceType` | `InsuranceType` (VO) | Valor válido do enum |
| `CoverageAmount` | `CoverageAmount` (VO) | Maior que zero |
| `Status` | `ProposalStatus` | Inicia como `UnderAnalysis` |
| `CreatedAt` | `DateTime` (UTC) | Gerado no construtor |

### Value Objects

#### CustomerName

Representa o nome do cliente de forma segura.

- Não pode ser nulo ou vazio
- Máximo de 200 caracteres
- Trimming automático aplicado na criação

#### InsuranceType

Representa o tipo de seguro contratado.

Valores permitidos:

| Valor | Descrição |
|-------|-----------|
| `Life` | Seguro de Vida |
| `Auto` | Seguro Automotivo |
| `Property` | Seguro Patrimonial |
| `Health` | Seguro Saúde |

- Não aceita valores fora da lista acima
- Armazenado como `string` no banco para legibilidade

#### CoverageAmount

Representa o valor de cobertura do seguro.

- Deve ser maior que zero
- Tipo base: `decimal`
- Imutável após criação

### Domain Events

Eventos publicados pelo agregado `Proposal` após cada operação de estado:

| Evento | Disparado em |
|--------|--------------|
| `ProposalCreatedEvent` | `Proposal.Create()` |
| `ProposalApprovedEvent` | `Proposal.Approve()` |
| `ProposalRejectedEvent` | `Proposal.Reject()` |

### Enum: ProposalStatus

```
UnderAnalysis   — estado inicial
Approved        — proposta aprovada para contratação
Rejected        — proposta recusada
```

### Casos de Uso — Proposal Service

| Caso de Uso | Tipo | Descrição |
|-------------|------|-----------|
| `CreateProposal` | Command | Cria nova proposta |
| `GetProposal` | Query | Retorna proposta por ID |
| `GetAllProposals` | Query | Lista propostas com paginação |
| `ApproveProposal` | Command | Aprova proposta em análise |
| `RejectProposal` | Command | Rejeita proposta em análise |

---

## Contracting Context

### Responsabilidades

- Contratar proposta aprovada
- Registrar e consultar contratação
- Validar status da proposta via gateway (ACL)

### Aggregate Root: Contract

Representa uma contratação efetivada e protege as invariantes de criação.

**Comportamentos (modelo rico):**

| Método | Descrição | Guarda |
|--------|-----------|--------|
| `Contract.Create(proposalId, contractedAt)` | Cria contratação para proposta aprovada | Valida `proposalId` não nulo, `contractedAt` válido |

**Propriedades:**

| Propriedade | Tipo | Observação |
|-------------|------|------------|
| `Id` | `Guid` | Gerado no construtor |
| `ProposalId` | `Guid` | Referência opaca ao contexto externo |
| `ContractedAt` | `DateTime` (UTC) | Data e hora da contratação |

> `ProposalId` é tratado como referência opaca entre bounded contexts. O `ContractingService` não carrega nem conhece a entidade `Proposal`. Consulta o status via `IProposalServiceGateway`.

### Domain Events

| Evento | Disparado em |
|--------|--------------|
| `ContractCreatedEvent` | `Contract.Create()` |

### Anti-Corruption Layer (ACL)

A validação do status da proposta no `ContractingService` ocorre por meio de uma interface definida na camada `Application`:

**Interface (Application/Ports):**

```
IProposalServiceGateway
  Task<ProposalSnapshot?> GetProposalAsync(Guid proposalId)
```

**DTO de retorno (Application/Ports):**

```
ProposalSnapshot
  Guid   Id
  string Status   ← "Approved", "Rejected", "UnderAnalysis"
```

`ProposalSnapshot` é um DTO do `ContractingService`. Não é a entidade `Proposal` do `ProposalService`. Essa separação garante o isolamento entre bounded contexts.

**Implementação (Infrastructure/Gateways):**

```
ProposalServiceGateway : IProposalServiceGateway
  → Realiza chamada HTTP ao ProposalService
  → Deserializa a resposta para ProposalSnapshot
  → Retorna null quando proposta não encontrada
```

### Casos de Uso — Contracting Service

| Caso de Uso | Tipo | Descrição |
|-------------|------|-----------|
| `CreateContract` | Command | Valida proposta via gateway e cria contratação |
| `GetContract` | Query | Retorna contratação por ID |

---

## Repositórios (Secondary Ports — Domain Layer)

Os contratos de persistência são definidos no `Domain` de cada serviço:

| Interface | Localização | Microsserviço |
|-----------|-------------|---------------|
| `IProposalRepository` | `Domain/Repositories` | ProposalService |
| `IContractRepository` | `Domain/Repositories` | ContractingService |

As implementações concretas residem na `Infrastructure` de cada serviço.

---

## SeedWork

Cada microsserviço possui sua própria `SeedWork` na camada `Domain`:

| Classe/Interface | Descrição |
|------------------|-----------|
| `Entity<TId>` | Classe base com `Id` e suporte a domain events |
| `AggregateRoot<TId>` | Estende `Entity<TId>`, gerencia coleção de eventos |
| `ValueObject` | Classe base com igualdade por valor |
| `IDomainEvent` | Marcador de domain events |

---

## Regras de Negócio Consolidadas

| Código | Contexto | Regra |
|--------|----------|-------|
| RN001 | Proposal | Toda proposta inicia como `UnderAnalysis` |
| RN002 | Proposal | Apenas propostas `UnderAnalysis` podem ser aprovadas ou rejeitadas |
| RN003 | Proposal | Uma proposta `Rejected` não pode ser aprovada |
| RN004 | Proposal | `CoverageAmount` deve ser maior que zero |
| RN005 | Contracting | Apenas propostas `Approved` podem ser contratadas |
| RN006 | Contracting | Uma proposta não pode ter mais de uma contratação |
| RN007 | Contracting | Toda contratação registra data e hora UTC da efetivação |
