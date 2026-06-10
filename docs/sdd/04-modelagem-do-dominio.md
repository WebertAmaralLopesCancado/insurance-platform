# Modelagem do Domínio

## Proposal

Representa uma proposta de seguro.

### Propriedades

- Id
- CustomerName
- InsuranceType
- CoverageAmount
- Status
- CreatedAt

### Regras

- Toda proposta inicia como UnderAnalysis.
- Status não pode ser nulo.
- CoverageAmount deve ser maior que zero.

---

## Contract

Representa uma contratação efetivada.

### Propriedades

- Id
- ProposalId
- ContractedAt

### Regras

- Deve possuir ProposalId válido.
- Deve possuir data de contratação.
- Não pode existir mais de uma contratação para a mesma proposta.

---

## ProposalStatus

Enum responsável pelo ciclo de vida da proposta.

Valores:

- UnderAnalysis
- Approved
- Rejected

---

## Aggregate Roots

### Proposal

Aggregate Root do Proposal Context.

### Contract

Aggregate Root do Contracting Context.

---

## Casos de Uso

### Proposal Service

- CreateProposal
- GetProposal
- GetAllProposals
- UpdateProposalStatus

### Contracting Service

- CreateContract
- GetContract

---

## Invariantes

Uma proposta:

- Sempre nasce UnderAnalysis.

Uma contratação:

- Só pode existir para proposta Approved.
- Só pode existir uma contratação por proposta.