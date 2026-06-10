# ADR-011 — SeedWork por Microsserviço

| Campo | Valor |
|-------|-------|
| **Status** | Aceito |
| **Data** | 2026-06-10 |
| **Contexto** | Compartilhamento de classes base de domínio entre microsserviços |
| **Decisores** | Equipe de arquitetura |

---

## Contexto

Ambos os microsserviços (`ProposalService` e `ContractingService`) precisam das mesmas classes base de domínio:

- `Entity<TId>` — classe base com identidade e igualdade por ID
- `AggregateRoot<TId>` — estende `Entity<TId>`, gerencia coleção de Domain Events
- `ValueObject` — implementa igualdade por valor via `GetEqualityComponents()`
- `IDomainEvent` — interface marcadora com `OccurredOnUtc`

A questão é: essas classes devem residir em um projeto compartilhado ou ser duplicadas em cada microsserviço?

---

## Decisão

Cada microsserviço possui sua própria `SeedWork` dentro do seu projeto `Domain`. **Não existe projeto compartilhado** entre os microsserviços.

### Estrutura resultante

```
ProposalService.Domain/
└── SeedWork/
    ├── Entity.cs
    ├── AggregateRoot.cs
    ├── ValueObject.cs
    ├── IDomainEvent.cs
    └── PagedResult.cs

ContractingService.Domain/
└── SeedWork/
    ├── Entity.cs
    ├── AggregateRoot.cs
    ├── ValueObject.cs
    └── IDomainEvent.cs
```

O código duplicado é mínimo — aproximadamente 100–150 linhas no total entre os dois serviços.

---

## Alternativas Consideradas

### Projeto compartilhado `InsurancePlatform.Shared` ou `InsurancePlatform.Domain.Shared`

Um único projeto NuGet/projeto de solução com as classes base, referenciado por ambos os microsserviços.

**Rejeitado porque:**

1. **Acoplamento de deploy:** qualquer alteração no projeto compartilhado exige rebuild e redeploy de ambos os serviços — viola o princípio de deploy independente de microsserviços

2. **Versionamento problemático:** se `Entity<TId>` precisar de uma mudança que afeta apenas o `ContractingService`, o `ProposalService` seria arrastado para a mesma versão

3. **Primeiro passo para o "distributed monolith":** projetos compartilhados crescem — o que começa com `Entity` acaba incluindo DTOs de integração, constantes de negócio e utilitários comuns, criando acoplamento progressivo

4. **Não é um shared kernel DDD legítimo:** as classes base são infraestrutura técnica, não conceitos de negócio compartilhados. Um shared kernel DDD compartilha tipos de domínio com significado de negócio explicitamente acordado entre contextos

### NuGet package interno

Publicar as classes base como pacote NuGet interno em um feed privado.

**Não adotado nesta versão porque:**
- Requer infraestrutura de feed privado (Azure Artifacts, GitHub Packages, etc.)
- Aumenta a complexidade de setup para a avaliação técnica
- O volume de código duplicado (< 150 linhas) não justifica a infraestrutura adicional

---

## Consequências

**Positivas:**
- Total independência entre os microsserviços — um não conhece o código do outro
- Cada serviço pode evoluir sua SeedWork de acordo com suas necessidades específicas
- Deploy completamente independente sem risco de quebrar o outro serviço
- Setup simplificado: clonar o repositório é suficiente para trabalhar em qualquer serviço

**Negativas (aceitas):**
- Duplicação de ~150 linhas de código entre os dois projetos `Domain`
- Correção de bug em `Entity<TId>` ou `ValueObject` precisa ser aplicada nos dois projetos
- Mitigação: o código base é estável e raramente muda após definido
