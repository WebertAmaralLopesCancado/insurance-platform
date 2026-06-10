# Relatório de Revisão Arquitetural

**Data:** 2026-06-10
**Responsável:** Arquiteto de Software Sênior
**Fase:** Revisão e consolidação de documentação — pré-implementação

---

## Arquivos Alterados

| Arquivo | Tipo de Alteração |
|---------|------------------|
| `docs/sdd/01-visao-geral.md` | Atualizado |
| `docs/sdd/02-ddd-e-bounded-contexts.md` | Reescrito |
| `docs/sdd/03-arquitetura-hexagonal.md` | Reescrito (estava incompleto) |
| `docs/sdd/04-modelagem-do-dominio.md` | Reescrito |
| `docs/sdd/05-decisoes-arquiteturais.md` | Expandido com 11 ADRs detalhados |
| `docs/sdd/00-relatorio-revisao-arquitetural.md` | Criado (este arquivo) |

---

## Pontos Corrigidos

### 01-visao-geral.md

| # | Correção |
|---|----------|
| 1 | Separação de `UpdateProposalStatus` em `ApproveProposal` e `RejectProposal` |
| 2 | Menção explícita a paginação em `GetAllProposals` |
| 3 | Tabela de regras de negócio expandida (RN006–RN008 adicionadas) |
| 4 | Requisitos não funcionais reformatados em tabela com cobertura completa |
| 5 | Seção de restrições arquiteturais com as 5 regras de dependência |

### 02-ddd-e-bounded-contexts.md

| # | Correção |
|---|----------|
| 1 | Diagrama ASCII dos bounded contexts com relação de comunicação via ACL |
| 2 | Comportamentos do agregado `Proposal` documentados com pré e pós-condições |
| 3 | Comportamentos do agregado `Contract` documentados |
| 4 | Seção completa de Value Objects (`CustomerName`, `InsuranceType`, `CoverageAmount`) |
| 5 | Seção completa de Domain Events por contexto |
| 6 | Separação de `UpdateProposalStatus` em `ApproveProposal` e `RejectProposal` |
| 7 | Seção completa de ACL com `IProposalServiceGateway` e `ProposalSnapshot` |
| 8 | `ProposalId` no `Contract` documentado como referência opaca |
| 9 | Interfaces de repositório como Secondary Ports no Domain |
| 10 | SeedWork (`Entity`, `AggregateRoot`, `ValueObject`, `IDomainEvent`) documentado |
| 11 | Tabela unificada de regras de negócio com códigos por contexto |
| 12 | Separação da tabela de casos de uso por tipo (Command/Query) |

### 03-arquitetura-hexagonal.md

| # | Correção |
|---|----------|
| 1 | Documento reescrito do zero (anterior terminava na linha 31) |
| 2 | Diagrama do hexágono com Primary/Secondary Ports e Adapters |
| 3 | Primary Ports definidos com interfaces genéricas em `Application/Common/` |
| 4 | Secondary Ports de persistência definidos no `Domain/Repositories/` |
| 5 | Secondary Port de ACL definido em `Application/Ports/` |
| 6 | Primary Adapters documentados com responsabilidades explícitas |
| 7 | Secondary Adapters de persistência (EF Core) documentados |
| 8 | Secondary Adapter de integração (`ProposalServiceGateway`) documentado |
| 9 | Fluxo completo de `CreateContract` com diagrama passo a passo |
| 10 | Fluxo completo de `CreateProposal` com diagrama passo a passo |
| 11 | Mapa completo de localização de cada interface e implementação |
| 12 | Tabela de dependências entre projetos com nota sobre uso de Infrastructure na Api |

### 04-modelagem-do-dominio.md

| # | Correção |
|---|----------|
| 1 | SeedWork documentado com contratos de `Entity<TId>`, `AggregateRoot<TId>`, `ValueObject`, `IDomainEvent` |
| 2 | `Proposal` reestruturado como modelo rico com construtor privado e factory method |
| 3 | Pré e pós-condições documentadas para `Create()`, `Approve()`, `Reject()` |
| 4 | Invariantes do agregado `Proposal` explicitadas |
| 5 | Value Objects com regras de validação detalhadas |
| 6 | `InsuranceType` com valores aceitos e normalização documentada |
| 7 | Domain Events com estrutura completa de propriedades |
| 8 | `IProposalRepository` com assinatura completa (paginação incluída) |
| 9 | `Contract` reestruturado como modelo rico |
| 10 | Nota sobre responsabilidade do handler vs. invariante do agregado em `Contract.Create()` |
| 11 | `IContractRepository` com método `GetByProposalIdAsync` para validação de duplicidade |
| 12 | Casos de uso de `Application` documentados com contratos de entrada/saída completos |
| 13 | `PagedResponse<T>` documentado com todos os campos incluindo `TotalPages` |
| 14 | `ApproveProposal` e `RejectProposal` com fluxo do handler detalhado |
| 15 | `CreateContract` com fluxo do handler em 9 passos |
| 16 | Hierarquia de exceções completa |
| 17 | Tabela de mapeamento HTTP por exceção |
| 18 | Middleware de erro com formato `ProblemDetails` documentado |

### 05-decisoes-arquiteturais.md

| # | Correção |
|---|----------|
| 1 | ADRs renumerados e expandidos para padrão Architecture Decision Record |
| 2 | Cada ADR inclui Status, Contexto, Decisão, Consequências |
| 3 | ADR-007 expandido com contrato completo de `PagedResponse<T>` |
| 4 | ADR-008 expandido com hierarquia de exceções e formato `ProblemDetails` |
| 5 | ADR-009 expandido com 3 níveis de testes, ferramentas e exemplos de casos |
| 6 | ADR-010 adicionado (FluentValidation) |
| 7 | ADR-011 adicionado (SeedWork por microsserviço com justificativa) |

---

## Consistência entre Documentos

| Ponto verificado | Status |
|-----------------|--------|
| Casos de uso iguais em 01, 02, 04 e 05 | Consistente |
| Regras de negócio com mesmos códigos em 01 e 02 | Consistente |
| Value Objects mencionados em 02, 04 e 05 | Consistente |
| Domain Events mencionados em 02, 04 e 05 | Consistente |
| ACL descrita em 02, 03, 04 e 05 | Consistente |
| Paginação em 01, 04 e 05 | Consistente |
| Estratégia de erros em 04 e 05 | Consistente |
| Estratégia de testes em 04 e 05 | Consistente |
| Direção de dependências em 01, 03 e 05 | Consistente |
| Separação Approve/Reject em 01, 02, 04 e 05 | Consistente |

---

## Pendências Restantes

### Documentação (baixa prioridade — podem ser criados durante implementação)

| # | Pendência | Prioridade |
|---|-----------|------------|
| 1 | Criar ADRs individuais em `docs/adrs/` (ex: `ADR-001-modelo-rico.md`) | Baixa |
| 2 | Criar diagramas de sequência UML para `CreateContract` e `CreateProposal` | Baixa |
| 3 | Documentar `docker-compose.yml` com os serviços esperados | Baixa |
| 4 | Definir contrato OpenAPI/Swagger (endpoints, métodos HTTP, payloads) | Baixa |

### Decisões técnicas em aberto (resolver durante implementação)

| # | Pendência | Impacto |
|---|-----------|---------|
| 1 | Estratégia de publicação de Domain Events (dispatch pós-save vs. outbox pattern) | Médio |
| 2 | Estratégia de resiliência no `ProposalServiceGateway` (timeout, retry via Polly) | Médio |
| 3 | Autenticação/autorização (não mencionado no enunciado — confirmar se escopo da vaga) | Baixo |
| 4 | Configuração de `CancellationToken` propagation no pipeline HTTP | Baixo |

> **Nota sobre Domain Events:** Para o escopo desta avaliação técnica, os Domain Events serão registrados no agregado e podem ser publicados via `IMediator` (MediatR) ou simplesmente ignorados após save (apenas estruturais, sem handler). A decisão deve ser tomada antes da implementação da Infrastructure.

---

## Recomendação: Podemos Iniciar a Criação da Solution .NET?

### Sim. A documentação está pronta para iniciar a implementação.

**Justificativa:**

Todos os contratos arquiteturais críticos estão definidos:

- Estrutura de projetos e namespaces
- Direção de dependências entre camadas
- Interfaces (Ports) com assinaturas completas
- Implementações (Adapters) com responsabilidades definidas
- Agregados com comportamentos, invariantes e domain events
- Value Objects com regras de validação
- Casos de uso com contratos de entrada/saída
- Estratégia de erros com mapeamento HTTP
- Estratégia de testes por nível

**Sequência recomendada de implementação:**

```
1. Criação da Solution (.sln) e estrutura de projetos
2. SeedWork (Domain de cada serviço)
3. Domain do ProposalService
   - Value Objects
   - Agregado Proposal (rico)
   - Domain Events
   - IProposalRepository
4. Domain do ContractingService
   - Agregado Contract (rico)
   - Domain Events
   - IContractRepository
5. Application do ProposalService
   - Common (ICommandHandler, IQueryHandler)
   - Todos os use cases (Commands, Queries, Handlers)
   - Exceções de aplicação
6. Application do ContractingService
   - Ports (IProposalServiceGateway, ProposalSnapshot)
   - Todos os use cases
   - Exceções de aplicação
7. Infrastructure do ProposalService
   - ProposalDbContext + Mappings
   - ProposalRepository
   - DI Extensions
8. Infrastructure do ContractingService
   - ContractingDbContext + Mappings
   - ContractRepository
   - ProposalServiceGateway
   - DI Extensions
9. Api do ProposalService
   - ProposalsController
   - ExceptionHandlerMiddleware
   - Program.cs + Swagger
10. Api do ContractingService
    - ContractsController
    - ExceptionHandlerMiddleware
    - Program.cs + Swagger
11. Testes unitários de Domain (ambos os serviços)
12. Testes unitários de Application (ambos os serviços)
13. Testes de integração de API (ambos os serviços)
14. docker-compose funcional
```
