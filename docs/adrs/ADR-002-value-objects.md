# ADR-002 — Value Objects

| Campo | Valor |
|-------|-------|
| **Status** | Aceito |
| **Data** | 2026-06-10 |
| **Contexto** | Representação de atributos do agregado `Proposal` |
| **Decisores** | Equipe de arquitetura |

---

## Contexto

Atributos como nome do cliente, tipo de seguro e valor de cobertura possuem regras de validação e semântica de negócio próprias. Representá-los como tipos primitivos (`string`, `decimal`) transfere a responsabilidade de validação para o código consumidor e permite a construção de estados inválidos sem nenhuma proteção.

Exemplo do problema com primitivos:

```csharp
// Qual string é válida para InsuranceType? Qualquer uma?
var proposal = new Proposal("João", "TipoInexistente", -100m);
```

---

## Decisão

Serão criados **Value Objects** para representar conceitos do domínio que possuem regras e semântica próprias. Value Objects são imutáveis e implementam igualdade por valor, não por referência.

### Value Objects definidos no ProposalService

| Value Object | Tipo base | Regras de validação |
|--------------|-----------|---------------------|
| `CustomerName` | `string` | Não nulo, não vazio após trim; máximo 200 caracteres; trim automático |
| `InsuranceType` | `string` | Apenas `Life`, `Auto`, `Property`, `Health`; case insensitive na validação |
| `CoverageAmount` | `decimal` | Deve ser maior que zero |

### Classe base `ValueObject`

Todos os Value Objects herdam de `ValueObject` (SeedWork), que implementa:
- `Equals()` por componentes (`GetEqualityComponents()`)
- `GetHashCode()` consistente com `Equals()`
- Operadores `==` e `!=`

### Mapeamento no EF Core

Value Objects são mapeados como **owned entities** usando `OwnsOne()` na Fluent API. O valor é persistido diretamente na tabela do agregado, sem tabela separada:

```
proposals
  id            uuid
  customer_name varchar(200)   ← CustomerName.Value
  insurance_type varchar(100)  ← InsuranceType.Value
  coverage_amount decimal(18,2) ← CoverageAmount.Value
  status        varchar(50)
  created_at    timestamp
```

---

## Alternativas Consideradas

### Validação via Data Annotations

Aplicar `[Required]`, `[MaxLength]`, `[Range]` nos DTOs de request.

**Rejeitado porque:**
- Validações ficam na camada de entrada (API), não no domínio
- A entidade pode ser construída com valores inválidos internamente
- Regras como "InsuranceType deve ser um valor da lista" não são expressáveis com Data Annotations simples

### Validação via FluentValidation no Application

Usar `AbstractValidator<T>` nos Commands.

**Aceito como complemento, não substituto:**
- FluentValidation valida o formato do input na camada Application (ADR-010)
- Value Objects validam invariantes de negócio na camada Domain
- As duas camadas têm responsabilidades distintas e não se substituem

---

## Consequências

**Positivas:**
- Construção de objetos inválidos impossível por design (fail-fast no construtor)
- Testes de Value Objects são simples, isolados e sem dependências
- Semântica de negócio expressa no tipo (`CustomerName` em vez de `string`)
- Igualdade por valor funciona corretamente sem implementação manual em cada entidade

**Negativas (aceitas):**
- Pequena verbosidade extra ao instanciar: `new CustomerName("João")` em vez de `"João"`
- Configuração de `OwnsOne` no EF Core necessária para cada Value Object
