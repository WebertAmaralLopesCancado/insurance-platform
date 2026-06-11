# Harness Report

## Escopo

Este harness valida automaticamente o caminho principal da insurance-platform sem alterar codigo da aplicacao:

- restore da solucao
- build da solucao
- testes unitarios
- subida do ambiente com Docker Compose
- health checks das APIs
- criacao e aprovacao de proposta
- criacao e consulta de contrato
- teardown do Docker Compose ao final

## Artefatos

- `harness/run-harness.sh`: script Bash compativel com WSL/Linux.
- `harness/insurance-platform.http`: colecao HTTP para execucao manual do fluxo.
- `harness/README.md`: instrucoes de uso, pre-requisitos e variaveis.

## Fluxo Validado

1. `dotnet restore insurance-platform.sln`
2. `dotnet build insurance-platform.sln --no-restore`
3. `dotnet test insurance-platform.sln --no-build --filter "Category!=Integration"`
4. `docker compose up --build -d`
5. `GET http://localhost:5001/health`
6. `GET http://localhost:5002/health`
7. `POST http://localhost:5001/api/proposals`
8. `PATCH http://localhost:5001/api/proposals/{proposalId}/approve`
9. `POST http://localhost:5002/api/contracts`
10. `GET http://localhost:5002/api/contracts/{contractId}`
11. `docker compose down --remove-orphans`

## Dependencias

O harness usa `curl` para chamadas HTTP e `jq` para extrair `proposalId` e `contractId`.

Instalacao do `jq` no Ubuntu/WSL:

```bash
sudo apt-get update && sudo apt-get install -y jq
```

## Resultado Esperado

Ao final, o terminal deve exibir:

- Build: OK
- Testes unitarios: OK
- Docker Compose: OK
- Health Proposal API: OK
- Health Contracting API: OK
- Proposta criada
- Proposta aprovada
- Contrato criado
- Contrato consultado

O script sempre tenta derrubar o Docker Compose no encerramento por meio de `trap`.
