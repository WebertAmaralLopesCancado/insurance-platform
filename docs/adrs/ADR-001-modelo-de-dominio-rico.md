# ADR-001 — Modelo de Domínio Rico

| Campo | Valor |
|-------|-------|
| **Status** | Aceito |
| **Data** | 2026-06-10 |
| **Contexto** | Modelagem do agregado `Proposal` e `Contract` |
| **Decisores** | Equipe de arquitetura |

---

## Contexto

Modelos anêmicos — entidades compostas apenas por propriedades públicas com setters — violam o encapsulamento e permitem que qualquer camada da aplicação modifique o estado interno de um agregado sem passar pelas regras de negócio do domínio.

Em um modelo anêmico, a lógica de transição de status poderia ser escrita assim:

```csharp
// Modelo Anêmico — estado atribuído diretamente
proposal.Status = ProposalStatus.Approved;
```

Isso não garante que a transição é válida (ex: proposta já rejeitada sendo aprovada), permitindo estados inconsistentes que só seriam detectados em tempo de execução ou em testes manuais.

---

## Decisão

Todas as entidades do domínio possuem comportamentos explícitos. Nenhuma propriedade expõe setter público. O estado interno de um agregado só pode ser modificado por métodos do próprio agregado.

Os agregados são instanciados exclusivamente via **factory methods estáticos** que aplicam todas as validações antes de construir o objeto:

```
Proposal.Create(customerName, insuranceType, coverageAmount)
Proposal.Approve()
Proposal.Reject()

Contract.Create(proposalId)
```

Os construtores são privados. O EF Core utiliza um construtor sem parâmetros também privado para materialização de entidades do banco.

### Guardas de transição em `Proposal`

| Método | Pré-condição | Exceção lançada |
|--------|-------------|-----------------|
| `Create()` | Todos os parâmetros não nulos | `DomainException` |
| `Approve()` | `Status == UnderAnalysis` | `DomainException` |
| `Reject()` | `Status == UnderAnalysis` | `DomainException` |

### Guardas em `Contract`

| Método | Pré-condição | Exceção lançada |
|--------|-------------|-----------------|
| `Create()` | `proposalId != Guid.Empty` | `DomainException` |

---

## Alternativas Consideradas

### Modelo Anêmico com validação no Application

Manter entidades como simples contêineres de dados e colocar todas as validações nos handlers da camada `Application`.

**Rejeitado porque:**
- Lógica de negócio dispersa em múltiplos handlers
- Risco de validação omitida ao adicionar novos casos de uso
- Testes de domínio tornam-se testes de Application, com dependências de mocks desnecessárias
- Viola o princípio fundamental do DDD: o Domain deve ser a fonte de verdade das regras de negócio

---

## Consequências

**Positivas:**
- Invariantes garantidas em qualquer ponto de entrada — API, testes, scripts de seed
- Lógica de negócio centralizada e descobrível pelo nome dos métodos
- Testes unitários de domínio sem mocks, sem banco, sem HTTP — testes puros e rápidos
- Adição de novos comportamentos via novos métodos no agregado, sem risco de quebrar comportamentos existentes

**Negativas (aceitas):**
- Requer atenção na configuração do EF Core para ignorar `DomainEvents` e mapear propriedades sem setter
- O construtor privado exige configuração explícita no `IEntityTypeConfiguration<T>`
