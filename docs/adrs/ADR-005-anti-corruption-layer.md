# ADR-005 — Anti-Corruption Layer (ACL)

| Campo | Valor |
|-------|-------|
| **Status** | Aceito |
| **Data** | 2026-06-10 |
| **Contexto** | Comunicação entre ProposalService e ContractingService |
| **Decisores** | Equipe de arquitetura |

---

## Contexto

O `ContractingService` precisa verificar o status de uma proposta antes de criar um contrato — regra de negócio RN005: "apenas propostas com status `Approved` podem ser contratadas".

A solução mais simples seria o `ContractingService` referenciar diretamente o `ProposalService`: compartilhar um projeto de domínio comum, reutilizar a entidade `Proposal` ou importar um DTO do serviço externo. Porém, isso criaria acoplamento direto entre bounded contexts — qualquer alteração no modelo do `ProposalService` afetaria o `ContractingService`.

---

## Decisão

A comunicação entre contextos ocorre exclusivamente através de uma **Anti-Corruption Layer (ACL)** composta por:

### 1. Interface na camada Application do ContractingService

```
ContractingService/Application/Ports/IProposalServiceGateway.cs

IProposalServiceGateway
  Task<ProposalSnapshot?> GetProposalByIdAsync(Guid proposalId, CancellationToken ct)
```

Esta interface é um **Secondary Port** do `ContractingService`. O `CreateContractCommandHandler` depende dela, nunca da implementação concreta.

### 2. DTO de travessia interno

```
ContractingService/Application/Ports/ProposalSnapshot.cs

record ProposalSnapshot(Guid Id, string Status)
```

`ProposalSnapshot` é um **tipo interno do ContractingService**. Não é a entidade `Proposal`, não é um DTO do `ProposalService`. É a representação mínima necessária para o `ContractingService` tomar decisões de negócio.

### 3. Implementação na camada Infrastructure do ContractingService

```
ContractingService/Infrastructure/Gateways/ProposalServiceGateway.cs

ProposalServiceGateway : IProposalServiceGateway
  → GET http://proposal-service/api/proposals/{id}
  → HTTP 404 → retorna null
  → HTTP 200 → deserializa para ProposalSnapshot
  → Outros erros → propaga exceção de infraestrutura
```

A tradução do modelo externo (resposta JSON do `ProposalService`) para o modelo interno (`ProposalSnapshot`) ocorre exclusivamente aqui.

### Regra de isolamento

O `ContractingService` **nunca**:
- Importa, herda ou referencia tipos do namespace `InsurancePlatform.ProposalService.*`
- Utiliza a entidade `Proposal` ou qualquer outro tipo de domínio do `ProposalService`
- Conhece a estrutura interna do banco de dados do `ProposalService`

---

## Fluxo da ACL no CreateContract

```
CreateContractCommandHandler
        │
        │  IProposalServiceGateway.GetProposalByIdAsync(proposalId)
        ▼
ProposalServiceGateway (Infrastructure)
        │  GET /api/proposals/{id}
        ▼
ProposalService (HTTP)
        │  retorna JSON { "id": "...", "status": "Approved", ... }
        ▼
ProposalServiceGateway
        │  deserializa → ProposalSnapshot { Id, Status }
        ▼
CreateContractCommandHandler
        │  verifica snapshot.Status == "Approved"
        │  prossegue com a criação do contrato
```

---

## Alternativas Consideradas

### Projeto compartilhado de contratos (shared kernel)

Criar um projeto `InsurancePlatform.Shared` com DTOs comuns entre os serviços.

**Rejeitado porque:**
- Acoplamento de deploy: alteração no `Shared` afeta ambos os serviços simultaneamente
- Viola o princípio de independência de microsserviços
- Qualquer mudança no contrato externo do `ProposalService` deve ser absorvida apenas pelo `ProposalServiceGateway`, não propagada a todos os consumidores do projeto compartilhado

### Referência direta ao projeto ProposalService.Application

Referenciar o projeto do `ProposalService` no `ContractingService`.

**Rejeitado porque:**
- Acoplamento direto entre microsserviços no nível de código
- Impede deploy independente
- Qualquer alteração em `ProposalService.Application` poderia quebrar o build do `ContractingService`

---

## Consequências

**Positivas:**
- Bounded contexts evoluem de forma totalmente independente
- Alterações no modelo do `ProposalService` afetam apenas `ProposalServiceGateway`
- `IProposalServiceGateway` é trivialmente mockável nos testes unitários
- A ACL pode absorver mudanças no contrato externo sem impacto nos casos de uso

**Negativas (aceitas):**
- Chamada HTTP síncrona introduz latência e ponto de falha
- Sem resiliência configurada (retry, circuit breaker) nesta versão — candidato a melhoria futura via Polly
