# Insurance Platform Harness

Harness de validacao para build, testes, Docker, health checks e fluxo funcional principal do projeto.

## Pre-requisitos

- WSL/Linux com Bash
- .NET SDK compativel com a solucao
- Docker
- Docker Compose (`docker compose` ou `docker-compose`)
- `curl`
- `jq`

No Ubuntu/WSL, instale `jq` com:

```bash
sudo apt-get update && sudo apt-get install -y jq
```

## Execucao

Execute a partir da raiz do repositorio:

```bash
chmod +x harness/run-harness.sh
./harness/run-harness.sh
```

O script executa:

1. `dotnet restore`
2. `dotnet build`
3. `dotnet test` filtrando testes com `Category!=Integration`
4. `docker compose up --build -d`
5. Health checks em `http://localhost:5001/health` e `http://localhost:5002/health`
6. `POST /api/proposals`
7. Extracao de `proposalId`
8. `PATCH /api/proposals/{id}/approve`
9. `POST /api/contracts`
10. Extracao de `contractId`
11. `GET /api/contracts/{id}`
12. Relatorio final no terminal
13. `docker compose down --remove-orphans` ao finalizar

## Variaveis

As variaveis abaixo podem ser sobrescritas:

```bash
PROPOSAL_API_URL=http://localhost:5001
CONTRACTING_API_URL=http://localhost:5002
SOLUTION_FILE=insurance-platform.sln
HEALTH_TIMEOUT_SECONDS=90
```

Exemplo:

```bash
HEALTH_TIMEOUT_SECONDS=120 ./harness/run-harness.sh
```

## Requisicoes manuais

Use `harness/insurance-platform.http` com uma extensao de REST Client para executar o fluxo manualmente.
