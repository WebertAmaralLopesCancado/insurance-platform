# ADR-009 — Estratégia de Testes Automatizados

| Campo | Valor |
|-------|-------|
| **Status** | Aceito |
| **Data** | 2026-06-10 |
| **Contexto** | Definição dos níveis e ferramentas de testes para os dois microsserviços |
| **Decisores** | Equipe de arquitetura |

---

## Contexto

Testes automatizados são critério explícito da avaliação técnica. A estratégia deve cobrir domínio, aplicação e API com qualidade, velocidade e clareza. Uma pirâmide de testes invertida (muitos testes de integração lentos e poucos testes de domínio rápidos) resulta em suítes frágeis e de manutenção custosa.

---

## Decisão

Três níveis de testes por microsserviço, seguindo a **pirâmide de testes**:

```
         ┌─────────────┐
         │ Integração  │  ← poucos, lentos, realistas (banco real)
         ├─────────────┤
         │ Application │  ← médios, rápidos, com mocks
         ├─────────────┤
         │   Domain    │  ← muitos, ultrarrápidos, sem dependências
         └─────────────┘
```

### Nível 1 — Domain Unit Tests

**Projetos:**
- `InsurancePlatform.ProposalService.Domain.UnitTests`
- `InsurancePlatform.ContractingService.Domain.UnitTests`

**Ferramentas:** `xUnit`, `FluentAssertions`

**Mocks:** nenhum — testa lógica pura do domínio

**O que cobrir:**
- Factory methods dos agregados em cenários válidos
- Factory methods em cenários inválidos (invariante quebrada)
- Transições de status (`Approve`, `Reject`)
- Registro de Domain Events após operações
- Value Objects: construção válida e inválida
- Guardas de estado (ex: aprovar proposta já aprovada)

**Exemplo de estrutura de testes:**

```
Domain.UnitTests/
├── Aggregates/
│   └── Proposals/
│       └── ProposalTests.cs
└── ValueObjects/
    ├── CustomerNameTests.cs
    ├── InsuranceTypeTests.cs
    └── CoverageAmountTests.cs
```

### Nível 2 — Application Unit Tests

**Projetos:**
- `InsurancePlatform.ProposalService.Application.UnitTests`
- `InsurancePlatform.ContractingService.Application.UnitTests`

**Ferramentas:** `xUnit`, `FluentAssertions`, `Moq`

**Mocks:** `IProposalRepository`, `IContractRepository`, `IProposalServiceGateway`

**O que cobrir:**
- Todos os command handlers: cenário de sucesso e cenários de falha
- Todos os query handlers: encontrado e não encontrado
- Verificação de chamadas a repositórios e gateways (via `Mock.Verify`)
- Propagação correta de exceções de aplicação

**Exemplo de estrutura de testes:**

```
Application.UnitTests/
└── UseCases/
    ├── CreateProposal/
    │   └── CreateProposalCommandHandlerTests.cs
    ├── ApproveProposal/
    │   └── ApproveProposalCommandHandlerTests.cs
    ├── RejectProposal/
    │   └── RejectProposalCommandHandlerTests.cs
    ├── GetProposal/
    │   └── GetProposalQueryHandlerTests.cs
    └── GetAllProposals/
        └── GetAllProposalsQueryHandlerTests.cs
```

### Nível 3 — API Integration Tests

**Projetos:**
- `InsurancePlatform.ProposalService.Api.IntegrationTests`
- `InsurancePlatform.ContractingService.Api.IntegrationTests`

**Ferramentas:** `xUnit`, `FluentAssertions`, `Microsoft.AspNetCore.Mvc.Testing`, `Testcontainers.PostgreSql`

**Banco:** PostgreSQL real provisionado via Testcontainers em tempo de execução

**Infraestrutura:**
- `WebApplicationFactory<Program>` para hospedar a API em memória
- Connection string substituída pela do container Testcontainers
- Migrations aplicadas automaticamente no `IAsyncLifetime.InitializeAsync()`
- Container destruído no `IAsyncLifetime.DisposeAsync()`

**O que cobrir:**
- Fluxo HTTP completo (request → middleware → controller → handler → banco → response)
- Status codes corretos para cenários de sucesso e erro
- Corpo da resposta com campos esperados
- Persistência verificada após operações de escrita

**Requisito de execução:**
- Docker ativo na máquina
- Recomendado: WSL (Windows Subsystem for Linux) com integração Docker ativa

### Resumo de cobertura

| Nível | Projetos | Testes |
|-------|----------|--------|
| Domain Unit Tests | 2 | 22 |
| Application Unit Tests | 2 | 26 |
| API Integration Tests | 2 | 11 |
| **Total** | **6** | **59** |

---

## Alternativas Consideradas

### Testes de integração com banco em memória (`InMemory`)

Usar `options.UseInMemoryDatabase()` do EF Core nos testes de integração.

**Rejeitado porque:**
- Banco em memória não respeita constraints de banco de dados (ex: `UNIQUE INDEX` no `proposal_id`)
- Comportamento de SQL (ordenação, tipos, conversões) pode divergir do PostgreSQL real
- Falhas que ocorrem em produção podem não ser reproduzíveis com banco em memória
- Testcontainers é a abordagem moderna e confiável para este caso

### Testes de integração contra banco de desenvolvimento

Usar uma instância PostgreSQL real pré-configurada na máquina do desenvolvedor.

**Rejeitado porque:**
- Requer configuração manual de cada máquina
- Estado do banco pode afetar resultados dos testes (dados residuais)
- Não funciona em CI sem configuração adicional de infraestrutura

---

## Consequências

**Positivas:**
- Pirâmide de testes respeitada: testes rápidos na base, lentos no topo
- Testes de domínio executam em milissegundos, sem dependências
- Testcontainers garante ambiente limpo e idempotente em cada execução
- Cobertura real do banco — constraints, índices e migrações são validados

**Negativas (aceitas):**
- Testes de integração requerem Docker ativo (não executam sem daemon Docker)
- Tempo de execução dos testes de integração inclui provisionamento do container
- Recomendação de uso de WSL no Windows para melhor compatibilidade com Docker
