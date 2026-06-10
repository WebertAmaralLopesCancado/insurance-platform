# ADR-003 — Domain Events

| Campo | Valor |
|-------|-------|
| **Status** | Aceito |
| **Data** | 2026-06-10 |
| **Contexto** | Notificação de mudanças de estado nos agregados |
| **Decisores** | Equipe de arquitetura |

---

## Contexto

Operações de domínio frequentemente possuem efeitos colaterais: notificar outros serviços, atualizar projeções, acionar integrações. Se esses efeitos forem codificados diretamente nos handlers da camada `Application`, o código de negócio fica acoplado a detalhes de infraestrutura e cada novo efeito colateral exige modificação do handler existente.

Além disso, em uma arquitetura de microsserviços, é comum que eventos de domínio precisem ser publicados em um barramento de mensagens no futuro. Sem a estrutura de Domain Events, essa evolução exigiria refatoração significativa.

---

## Decisão

Os agregados registram **Domain Events** internamente após cada operação de mudança de estado. Os eventos representam **o que aconteceu** no domínio, não o que deve ser feito a seguir.

### Infraestrutura de eventos (SeedWork)

```
IDomainEvent
  DateTime OccurredOnUtc

AggregateRoot<TId>
  IReadOnlyCollection<IDomainEvent> DomainEvents
  void AddDomainEvent(IDomainEvent)
  void ClearDomainEvents()
```

### Eventos definidos

| Evento | Contexto | Disparado por | Propriedades |
|--------|----------|---------------|--------------|
| `ProposalCreatedEvent` | ProposalService | `Proposal.Create()` | `ProposalId`, `CustomerName`, `InsuranceType`, `CoverageAmount`, `OccurredOnUtc` |
| `ProposalApprovedEvent` | ProposalService | `Proposal.Approve()` | `ProposalId`, `OccurredOnUtc` |
| `ProposalRejectedEvent` | ProposalService | `Proposal.Reject()` | `ProposalId`, `OccurredOnUtc` |
| `ContractCreatedEvent` | ContractingService | `Contract.Create()` | `ContractId`, `ProposalId`, `ContractedAt`, `OccurredOnUtc` |

Todos os eventos são implementados como `record` imutável que implementa `IDomainEvent`.

### Ciclo de vida dos eventos

```
1. Agregado executa operação (Approve, Create...)
2. Agregado chama AddDomainEvent(new XxxEvent(...))
3. Evento fica em DomainEvents até o commit
4. Repository salva a entidade e chama SaveChangesAsync()
5. Após o commit, eventos podem ser publicados
6. ClearDomainEvents() é chamado após publicação
```

### Escopo atual

Nesta versão, os Domain Events são **estruturais** — a infraestrutura de coleta e publicação está preparada, mas não há handlers de eventos ativos. Os eventos registram o histórico de intenções do domínio e estão prontos para integração com MediatR ou barramento de mensagens.

---

## Alternativas Consideradas

### Sem Domain Events — efeitos colaterais diretos no handler

Executar todos os efeitos colaterais diretamente no `CommandHandler`.

**Rejeitado porque:**
- Handler com múltiplas responsabilidades (SRP violado)
- Adição de novos efeitos colaterais exige modificação do handler existente (OCP violado)
- Testabilidade reduzida — cada efeito colateral precisa ser mockado no teste do handler

### Eventos via Outbox Pattern (banco de dados)

Persistir eventos em tabela de outbox dentro da mesma transação e publicá-los de forma assíncrona.

**Considerado como melhoria futura (ver Melhorias Futuras no README):**
- Garante entrega exactly-once mesmo em caso de falha após o commit
- Requer tabela adicional e processo de relay de eventos
- Não é necessário no escopo atual da avaliação

---

## Consequências

**Positivas:**
- Separação entre o que aconteceu (evento) e o que fazer com isso (handler de evento)
- Extensibilidade: novos efeitos colaterais via novos handlers sem modificar o agregado ou o caso de uso
- Rastreabilidade: histórico de operações de domínio disponível para auditoria
- Base para migração futura para arquitetura event-driven (RabbitMQ, Kafka)

**Negativas (aceitas):**
- `DomainEvents` precisa ser explicitamente ignorado no mapeamento EF Core (`builder.Ignore(x => x.DomainEvents)`)
- Sem handlers ativos, os eventos são coletados mas não processados nesta versão
