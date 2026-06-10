# ADR-004 — Ports and Adapters Explícitos

| Campo | Valor |
|-------|-------|
| **Status** | Aceito |
| **Data** | 2026-06-10 |
| **Contexto** | Definição das fronteiras da Arquitetura Hexagonal |
| **Decisores** | Equipe de arquitetura |

---

## Contexto

A Arquitetura Hexagonal exige que as fronteiras entre o núcleo da aplicação (Domain + Application) e o mundo externo (banco de dados, HTTP, mensageria) sejam **explícitas via interfaces**. Sem essa explicitação, a arquitetura torna-se apenas uma convenção de pastas sem verificabilidade real.

O risco sem Ports e Adapters explícitos é que a Application use diretamente classes concretas de Infrastructure (ex: `ProposalRepository`, `HttpClient`), criando acoplamento que torna os testes unitários inviáveis e a substituição de implementações custosa.

---

## Decisão

Todos os Ports são interfaces nomeadas, localizadas em camadas específicas, com contratos explícitos.

### Primary Ports (Input Ports) — Application/Common

Definem como o mundo externo aciona os casos de uso:

| Interface | Uso |
|-----------|-----|
| `ICommandHandler<TCommand, TResponse>` | Operações de escrita com retorno |
| `IQueryHandler<TQuery, TResponse>` | Operações de leitura com retorno |

Os Controllers recebem handlers via injeção de dependência pela interface, nunca pela classe concreta.

### Secondary Ports — Domain/Repositories

Definem como o domínio persiste e consulta seus agregados:

| Interface | Localização | Implementação |
|-----------|-------------|---------------|
| `IProposalRepository` | `ProposalService.Domain/Repositories/` | `ProposalRepository` (Infrastructure) |
| `IContractRepository` | `ContractingService.Domain/Repositories/` | `ContractRepository` (Infrastructure) |

### Secondary Port — Application/Ports (ACL)

Define como o `ContractingService` consulta dados de outro contexto:

| Interface | Localização | Implementação |
|-----------|-------------|---------------|
| `IProposalServiceGateway` | `ContractingService.Application/Ports/` | `ProposalServiceGateway` (Infrastructure) |

### Primary Adapters — Api/Controllers

Traduzem requisições HTTP em chamadas aos Primary Ports:

| Adapter | Microsserviço |
|---------|---------------|
| `ProposalsController` | ProposalService |
| `ContractsController` | ContractingService |

### Secondary Adapters — Infrastructure

Implementam os Secondary Ports com tecnologias concretas:

| Adapter | Porta implementada | Tecnologia |
|---------|--------------------|------------|
| `ProposalRepository` | `IProposalRepository` | EF Core + PostgreSQL |
| `ContractRepository` | `IContractRepository` | EF Core + PostgreSQL |
| `ProposalServiceGateway` | `IProposalServiceGateway` | `HttpClient` |

### Mapa completo de localização

| Artefato | Tipo | Camada |
|----------|------|--------|
| `ICommandHandler<,>` | Primary Port | Application |
| `IQueryHandler<,>` | Primary Port | Application |
| `IProposalRepository` | Secondary Port | Domain |
| `IContractRepository` | Secondary Port | Domain |
| `IProposalServiceGateway` | Secondary Port (ACL) | Application |
| `ProposalsController` | Primary Adapter | Api |
| `ContractsController` | Primary Adapter | Api |
| `ProposalRepository` | Secondary Adapter | Infrastructure |
| `ContractRepository` | Secondary Adapter | Infrastructure |
| `ProposalServiceGateway` | Secondary Adapter | Infrastructure |

---

## Regra de Dependência entre Projetos

```
*.Domain          ← zero referências externas
*.Application     ← referencia *.Domain
*.Infrastructure  ← referencia *.Application + *.Domain
*.Api             ← referencia *.Application + *.Infrastructure
```

A `Api` referencia `Infrastructure` exclusivamente para registrar implementações no container de DI. Nenhuma classe de negócio na `Api` depende diretamente de classes de `Infrastructure`.

---

## Alternativas Consideradas

### Repositório genérico `IRepository<T>`

Uma única interface `IRepository<T>` para todos os agregados.

**Rejeitado porque:**
- Interface única expõe métodos não relevantes para cada agregado
- Viola o Interface Segregation Principle (ISP)
- Contratos específicos (`GetByProposalIdAsync`, `GetPagedAsync`) não são expressáveis genericamente sem comprometer o design

---

## Consequências

**Positivas:**
- Substituição de implementações sem impacto no domínio ou na aplicação
- Testes unitários com mocks triviais baseados em interfaces
- Direção de dependência verificável pelas referências de projeto no `.csproj`
- Cada interface pode ser evoluída independentemente

**Negativas (aceitas):**
- Mais interfaces e arquivos que em um design sem separação explícita
- Registro de DI mais verboso (cada interface mapeada para sua implementação)
