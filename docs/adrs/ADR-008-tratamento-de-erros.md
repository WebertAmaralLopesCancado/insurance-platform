# ADR-008 — Estratégia de Tratamento de Erros

| Campo | Valor |
|-------|-------|
| **Status** | Aceito |
| **Data** | 2026-06-10 |
| **Contexto** | Padronização de respostas de erro em todos os endpoints |
| **Decisores** | Equipe de arquitetura |

---

## Contexto

Sem uma estratégia centralizada de tratamento de erros, cada controller precisaria de blocos `try/catch` individuais, produzindo respostas inconsistentes — alguns endpoints retornariam `string` de mensagem, outros `ProblemDetails`, outros nenhum corpo. O resultado é uma API imprevisível para o consumidor.

---

## Decisão

Um **middleware global** (`ExceptionHandlerMiddleware`) em cada serviço intercepta todas as exceções não tratadas e as mapeia para respostas HTTP padronizadas no formato `ProblemDetails` (RFC 7807).

### Hierarquia de exceções

```
Exception (sistema)
│
├── DomainException                       ← violações de invariante de domínio
│
└── ApplicationException (base interna)  ← erros de fluxo de aplicação
    ├── NotFoundException
    ├── ConflictException
    └── ValidationException
        └── ProposalNotApprovedException
        └── ProposalAlreadyContractedException
```

`DomainException` reside no `Domain` de cada microsserviço.
As exceções de aplicação residem no `Application/Exceptions/` de cada microsserviço.

### Mapeamento para HTTP

| Exceção | Status HTTP | Cenário típico |
|---------|-------------|---------------|
| `NotFoundException` | `404 Not Found` | Proposta ou contrato inexistente |
| `DomainException` | `422 Unprocessable Entity` | Transição de status inválida, invariante quebrada |
| `ProposalNotApprovedException` | `422 Unprocessable Entity` | Tentativa de contratar proposta não aprovada |
| `ProposalAlreadyContractedException` | `409 Conflict` | Proposta já possui contrato |
| `ValidationException` | `400 Bad Request` | Payload de entrada com formato inválido |
| `Exception` (não mapeada) | `500 Internal Server Error` | Erros inesperados de infraestrutura |

### Formato da resposta de erro — ProblemDetails (RFC 7807)

```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "Proposal with id 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx' was not found.",
  "instance": "/api/proposals/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
}
```

### Comportamento de logging

- Erros `500` são logados como `LogError` com stack trace completa
- Erros `4xx` não expõem detalhes internos de infraestrutura no corpo da resposta
- O `Content-Type` da resposta de erro é sempre `application/problem+json`

### Posição no pipeline

O middleware é registrado antes dos controllers para capturar qualquer exceção não tratada:

```csharp
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.MapControllers();
```

---

## Alternativas Consideradas

### `UseExceptionHandler` do ASP.NET Core

Usar o middleware nativo `app.UseExceptionHandler("/error")` com um endpoint dedicado.

**Não adotado porque:**
- Requer endpoint separado (`/error`) que aparece no Swagger
- Menos controle sobre o mapeamento de tipos de exceção específicos
- Middleware customizado é mais explícito e testável

### `try/catch` em cada controller

Cada action method trata suas próprias exceções.

**Rejeitado porque:**
- Código repetitivo em todos os controllers
- Inconsistência garantida ao longo do tempo
- Viola o DRY (Don't Repeat Yourself)

---

## Consequências

**Positivas:**
- Respostas de erro consistentes em todos os endpoints
- Controllers sem blocos `try/catch` — focados apenas no caminho feliz
- Novos tipos de exceção adicionados ao middleware sem modificar controllers
- `ProblemDetails` (RFC 7807) é um padrão reconhecido por ferramentas e clientes

**Negativas (aceitas):**
- Exceções de domínio lançadas profundamente no stack percorrem todas as camadas antes de serem capturadas
- O middleware precisa ser atualizado quando novos tipos de exceção forem adicionados
