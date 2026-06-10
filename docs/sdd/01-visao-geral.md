# Visão Geral

## Objetivo

Desenvolver uma plataforma de seguros composta por microsserviços independentes responsáveis pelo gerenciamento de propostas e contratações.

A solução demonstrará domínio de Arquitetura Hexagonal (Ports and Adapters), DDD com modelo rico, princípios SOLID, Clean Code, separação de responsabilidades e alta coesão com baixo acoplamento.

---

## Contexto de Negócio

A plataforma permite que clientes realizem propostas de seguro.

Uma proposta percorre o seguinte ciclo de vida:

```
UnderAnalysis → Approved
             → Rejected
```

Somente propostas com status `Approved` poderão ser contratadas.
Uma proposta aprovada só pode gerar uma única contratação.

---

## Microsserviços

### Proposal Service

Responsável pelo ciclo de vida completo das propostas de seguro.

Casos de uso:

- `CreateProposal` — Criar nova proposta
- `GetProposal` — Consultar proposta por ID
- `GetAllProposals` — Listar propostas com paginação
- `ApproveProposal` — Aprovar proposta em análise
- `RejectProposal` — Rejeitar proposta em análise

### Contracting Service

Responsável pela contratação de propostas aprovadas.

Casos de uso:

- `CreateContract` — Contratar proposta aprovada
- `GetContract` — Consultar contratação por ID

---

## Regras de Negócio

| Código | Regra |
|--------|-------|
| RN001 | Toda proposta criada inicia com status `UnderAnalysis` |
| RN002 | Uma proposta pode ser alterada para `Approved` ou `Rejected` |
| RN003 | Somente propostas com status `Approved` podem ser contratadas |
| RN004 | Uma proposta não pode ser contratada mais de uma vez |
| RN005 | Toda contratação deve armazenar data e hora da efetivação |
| RN006 | O valor de cobertura deve ser maior que zero |
| RN007 | Uma proposta rejeitada não pode ser aprovada |
| RN008 | Uma proposta já aprovada não pode ser aprovada novamente |

---

## Requisitos Não Funcionais

| Requisito | Descrição |
|-----------|-----------|
| Arquitetura | Hexagonal (Ports and Adapters) |
| Design | DDD com modelo rico, SOLID, Clean Code |
| Comunicação | REST HTTP entre microsserviços |
| Persistência | PostgreSQL com EF Core e migrations versionadas |
| Paginação | Listagens com `PageNumber`, `PageSize`, `TotalItems`, `TotalPages` |
| Testes | Unitários (Domain e Application) e Integração (API com Testcontainers) |
| Containerização | Docker com `docker-compose` funcional |
| Documentação | Swagger/OpenAPI em cada serviço |
| Erros | Middleware global com mapeamento de exceções para HTTP |

---

## Restrições Arquiteturais

- O `Domain` não referencia nenhuma camada externa
- O `Application` referencia apenas o `Domain`
- O `Infrastructure` referencia `Application` e `Domain`
- A `Api` referencia `Application` e `Infrastructure` (exclusivamente para registro de DI)
- O `ContractingService` não conhece entidades internas do `ProposalService` (ACL via gateway)
