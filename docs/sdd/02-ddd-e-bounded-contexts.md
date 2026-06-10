# DDD e Bounded Contexts

## Domínio

Plataforma de Seguros

O sistema é responsável por gerenciar propostas de seguro e suas respectivas contratações.

---

# Bounded Contexts

Foram identificados dois contextos principais:

## Proposal Context

Responsável pelo ciclo de vida das propostas.

### Responsabilidades

- Criar proposta
- Consultar proposta
- Listar propostas
- Alterar status

---

## Contracting Context

Responsável pela contratação de propostas aprovadas.

### Responsabilidades

- Contratar proposta
- Registrar contratação
- Consultar contratação
- Validar status da proposta

---

# Entidades

## Proposal

Representa uma proposta de seguro.

### Atributos

- Id
- CustomerName
- InsuranceType
- CoverageAmount
- Status
- CreatedAt

---

## Contract

Representa uma contratação efetivada.

### Atributos

- Id
- ProposalId
- ContractedAt

---

# Enums

## ProposalStatus

- UnderAnalysis
- Approved
- Rejected

---

# Casos de Uso

## Proposal Service

### CreateProposal

Criar nova proposta.

### GetProposal

Consultar proposta.

### GetAllProposals

Listar propostas.

### UpdateProposalStatus

Alterar status da proposta.

---

## Contracting Service

### CreateContract

Contratar proposta aprovada.

### GetContract

Consultar contratação.

---

# Regras de Negócio

RN001
Toda proposta inicia como UnderAnalysis.

RN002
Somente propostas Approved podem ser contratadas.

RN003
Uma proposta só pode possuir uma contratação.

RN004
Toda contratação deve registrar data e hora da efetivação.