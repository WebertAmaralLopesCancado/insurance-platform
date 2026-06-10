# ADR-007 — Paginação em Listagens

| Campo | Valor |
|-------|-------|
| **Status** | Aceito |
| **Data** | 2026-06-10 |
| **Contexto** | Endpoint `GET /api/proposals` no ProposalService |
| **Decisores** | Equipe de arquitetura |

---

## Contexto

O caso de uso `GetAllProposals` precisa retornar propostas cadastradas no sistema. Retornar todos os registros em uma única resposta é inviável em produção — a tabela pode conter milhares de propostas, tornando a resposta impraticável em termos de memória, rede e tempo de resposta.

---

## Decisão

O endpoint `GET /api/proposals` suporta paginação via query parameters, retornando uma resposta paginada tipada com metadados completos.

### Parâmetros de entrada

| Parâmetro | Tipo | Padrão | Restrição |
|-----------|------|--------|-----------|
| `pageNumber` | `int` | `1` | Mínimo: 1 |
| `pageSize` | `int` | `10` | Mínimo: 1 |

### Contrato de saída — `PagedResponse<T>`

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `items` | `IReadOnlyCollection<T>` | Itens da página atual |
| `pageNumber` | `int` | Número da página atual |
| `pageSize` | `int` | Tamanho da página solicitada |
| `totalItems` | `int` | Total de registros no banco |
| `totalPages` | `int` | `ceil(totalItems / pageSize)` |

### Implementação na Infrastructure

A paginação é aplicada diretamente na query SQL via EF Core:

```
SELECT * FROM proposals
ORDER BY created_at DESC, id ASC
OFFSET (pageNumber - 1) * pageSize ROWS
FETCH NEXT pageSize ROWS ONLY
```

A contagem total (`totalItems`) é obtida com uma query `COUNT(*)` separada, antes do `SKIP/TAKE`. O `AsNoTracking()` é aplicado para consultas de leitura.

### Ordenação padrão

Os resultados são ordenados por `CreatedAt` decrescente (mais recentes primeiro), com desempate por `Id` crescente para garantir ordenação determinística.

---

## Alternativas Consideradas

### Retornar todos os registros (`IReadOnlyList<ProposalResponse>`)

Buscar todas as propostas sem limitação.

**Rejeitado porque:**
- Inviável em produção com volume de dados real
- Risco de timeout e estouro de memória
- Sem metadados de paginação, o cliente não sabe quantos registros existem

### Cursor-based pagination

Usar um cursor opaco (ex: `after: "base64encodedId"`) em vez de número de página.

**Não adotado nesta versão porque:**
- Maior complexidade de implementação
- Adequado para feeds em tempo real e grandes volumes
- O modelo atual com `pageNumber` e `pageSize` atende o escopo da avaliação
- Pode ser evoluído para cursor-based sem breaking changes no contrato (adicionando campo opcional)

---

## Consequências

**Positivas:**
- Desempenho previsível independente do volume de dados
- Resposta com metadados completos (`totalPages`, `totalItems`) sem chamadas adicionais do cliente
- Query SQL eficiente com `SKIP/TAKE` e `COUNT` — dois round-trips ao banco por listagem
- Padrão amplamente reconhecido e integrável com qualquer cliente (web, mobile, CLI)

**Negativas (aceitas):**
- Dois round-trips ao banco por requisição de listagem (COUNT + SELECT)
- `pageNumber`-based pagination pode apresentar inconsistências em inserções concorrentes (registro aparece em duas páginas ou é pulado) — comportamento aceitável para este domínio
