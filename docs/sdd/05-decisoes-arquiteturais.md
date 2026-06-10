# Decisões Arquiteturais

## Decisão 001 - Modelo de Domínio Rico

As entidades do domínio deverão possuir comportamento e proteger suas próprias invariantes.

Exemplos:

- Proposal.Create()
- Proposal.Approve()
- Proposal.Reject()
- Contract.Create()

Não será utilizado modelo anêmico baseado apenas em propriedades públicas.

## Decisão 002 - Value Objects

Serão utilizados Value Objects para representar conceitos relevantes do domínio.

Proposal Service:

- CustomerName
- InsuranceType
- CoverageAmount

## Decisão 003 - Domain Events

Serão utilizados Domain Events para representar eventos importantes do domínio.

Eventos previstos:

- ProposalCreatedEvent
- ProposalApprovedEvent
- ProposalRejectedEvent
- ContractCreatedEvent

## Decisão 004 - Ports and Adapters

As interfaces de entrada e saída serão explícitas.

Input Ports:

- CreateProposal
- GetProposal
- GetAllProposals
- ApproveProposal
- RejectProposal
- CreateContract
- GetContract

Output Ports:

- IProposalRepository
- IContractRepository
- IProposalServiceGateway

## Decisão 005 - Anti-Corruption Layer

O ContractingService não conhecerá diretamente o modelo interno do ProposalService.

A comunicação será feita por meio de uma interface:

- IProposalServiceGateway

A implementação concreta ficará na Infrastructure:

- ProposalServiceGateway

## Decisão 006 - Separação de Casos de Uso

Não será utilizado um caso de uso genérico UpdateProposalStatus.

Serão criados casos de uso explícitos:

- ApproveProposal
- RejectProposal

## Decisão 007 - Paginação

A listagem de propostas deverá possuir paginação.

Parâmetros:

- PageNumber
- PageSize

## Decisão 008 - Tratamento de Erros

Serão diferenciados:

- Erros de domínio
- Erros de aplicação
- Erros de infraestrutura
- Erros HTTP

A API terá middleware global de tratamento de exceções.

## Decisão 009 - Testes

Serão criados testes unitários para Domain e Application.

Também serão criados testes de integração para APIs, preferencialmente usando banco real com Testcontainers.