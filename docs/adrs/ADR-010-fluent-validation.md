# ADR-010 — Validação de Input com FluentValidation

| Campo | Valor |
|-------|-------|
| **Status** | Aceito |
| **Data** | 2026-06-10 |
| **Contexto** | Validação de Commands e Queries na camada Application |
| **Decisores** | Equipe de arquitetura |

---

## Contexto

A validação de dados de entrada possui duas naturezas distintas:

1. **Validação de formato/entrada** — `CustomerName` não pode ser vazio, `PageNumber` deve ser maior que zero, `ProposalId` não pode ser `Guid.Empty`. Essas regras pertencem à borda do sistema e devem ser verificadas antes de qualquer lógica de domínio.

2. **Validação de invariante de domínio** — `CoverageAmount` deve ser maior que zero (regra de negócio), uma proposta já aprovada não pode ser aprovada novamente (regra de transição). Essas regras pertencem ao Domain.

Misturar os dois tipos na mesma camada compromete a separação de responsabilidades.

---

## Decisão

**FluentValidation** é utilizado na camada `Application` para validar o formato e os pré-requisitos de entrada dos `Commands` e `Queries`, antes da execução dos handlers.

### Separação clara de responsabilidades

| Tipo de validação | Onde fica | Ferramenta |
|-------------------|-----------|------------|
| Formato de entrada (required, range, pattern) | Application | FluentValidation |
| Invariante de negócio | Domain | `DomainException` no agregado |

### Exemplos de validações na Application

```
CreateProposalCommand:
  - CustomerName: NotEmpty
  - InsuranceType: NotEmpty
  - CoverageAmount: GreaterThan(0)

GetAllProposalsQuery:
  - PageNumber: GreaterThanOrEqualTo(1)
  - PageSize: InclusiveBetween(1, 100)

CreateContractCommand:
  - ProposalId: NotEmpty
```

### Localização dos validators

Cada caso de uso com validações possui seu próprio `Validator<T>` na mesma pasta:

```
Application/UseCases/CreateProposal/
├── CreateProposalCommand.cs
├── CreateProposalCommandHandler.cs
├── CreateProposalCommandValidator.cs   ← validator aqui
└── CreateProposalResponse.cs
```

---

## Alternativas Consideradas

### Data Annotations (`[Required]`, `[Range]`, etc.)

Aplicar atributos de validação diretamente nas classes `Command`.

**Não adotado como solução principal porque:**
- Regras complexas (ex: `InsuranceType` deve ser um dos valores aceitos) não são expressáveis de forma limpa com Data Annotations
- Regras de validação ficam acopladas à estrutura da classe (dificulta herança e composição)
- FluentValidation é mais expressivo e testável isoladamente

### Validação apenas no Domain (Value Objects)

Deixar que os Value Objects rejeitem dados inválidos e propagar a `DomainException` como erro 422.

**Parcialmente adotado, mas insuficiente como única camada:**
- Um `CustomerName` vazio lançaria `DomainException` (422) em vez de `ValidationException` (400)
- A distinção entre "formato inválido de entrada" (400) e "violação de regra de negócio" (422) seria perdida
- Validação de parâmetros de query (`PageNumber`, `PageSize`) não tem representação no Domain

---

## Consequências

**Positivas:**
- Domain protegido de input malformado — só recebe dados válidos em formato
- HTTP 400 para erros de entrada, HTTP 422 para violações de negócio — semântica HTTP correta
- Validators testáveis isoladamente sem instanciar o handler
- Mensagens de validação descritivas e localizáveis

**Negativas (aceitas):**
- Uma camada adicional de validação que cobre casos que os Value Objects já cobririam parcialmente
- Risco de duplicação de regras simples (ex: `CoverageAmount > 0` aparece tanto no validator quanto no `CoverageAmount` VO)
- Risco mitigado: as duas validações são intencionalmente redundantes — a do VO garante o domínio mesmo que o validator seja contornado
