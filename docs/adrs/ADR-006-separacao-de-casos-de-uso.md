# ADR-006 — Separação de Casos de Uso por Intenção

| Campo | Valor |
|-------|-------|
| **Status** | Aceito |
| **Data** | 2026-06-10 |
| **Contexto** | Modelagem dos casos de uso de transição de status do `ProposalService` |
| **Decisores** | Equipe de arquitetura |

---

## Contexto

A operação de alterar o status de uma proposta poderia ser modelada como um único caso de uso genérico:

```
UpdateProposalStatus(proposalId, newStatus)
```

Essa abordagem agrupa duas operações com naturezas distintas — aprovação e rejeição — em um único handler que aceita o novo status como parâmetro. Embora mais compacta, essa modelagem traz problemas de design.

---

## Decisão

Casos de uso separados por intenção explícita, cada um com seu próprio `Command`, `Handler` e endpoint REST:

| Caso de Uso | Command | Endpoint |
|-------------|---------|----------|
| Aprovar proposta | `ApproveProposalCommand` | `PATCH /api/proposals/{id}/approve` |
| Rejeitar proposta | `RejectProposalCommand` | `PATCH /api/proposals/{id}/reject` |

Cada handler tem uma única responsabilidade e não recebe parâmetro de status — a intenção está expressa no nome do comando e do endpoint.

### Estrutura de pastas resultante

```
Application/UseCases/
├── ApproveProposal/
│   ├── ApproveProposalCommand.cs
│   └── ApproveProposalCommandHandler.cs
└── RejectProposal/
    ├── RejectProposalCommand.cs
    └── RejectProposalCommandHandler.cs
```

---

## Alternativas Consideradas

### `UpdateProposalStatus(proposalId, newStatus)` genérico

Um único handler que recebe o novo status como parâmetro.

**Rejeitado porque:**

1. **SRP violado:** um handler com duas responsabilidades (aprovar e rejeitar)
2. **Endpoint semanticamente fraco:** `PATCH /proposals/{id}` com `{ "status": "Approved" }` não comunica intenção de negócio
3. **Escalabilidade de regras comprometida:** se aprovação e rejeição ganharem pré-condições distintas no futuro (ex: aprovação requer análise de crédito, rejeição requer motivo), o handler genérico precisaria de bifurcações internas
4. **Risco de estados inválidos por input:** aceitar `status = "UnderAnalysis"` via API seria tecnicamente possível com um handler genérico, exigindo validação adicional para barrar esse caso
5. **Testabilidade:** testes precisariam cobrir todas as combinações de parâmetros em um único handler

### PATCH com payload `{ "status": "..." }`

Manter um único endpoint mas com payload contendo o novo status.

**Rejeitado pelo mesmo motivo:** o endpoint não comunica intenção de negócio e aceita valores arbitrários de status como input.

---

## Consequências

**Positivas:**
- Cada handler tem responsabilidade única e bem definida (SRP)
- Endpoints REST semanticamente corretos e auto-documentados
- Regras de aprovação e rejeição podem evoluir de forma independente
- Testes unitários focados: cada teste cobre exatamente um comportamento
- Swagger exibe intenções de negócio claras: `/approve` e `/reject`

**Negativas (aceitas):**
- Dois handlers em vez de um — mais arquivos, porém mais coesos
- Dois endpoints em vez de um — necessário documentar ambos no Swagger
