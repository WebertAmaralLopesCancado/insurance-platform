#!/usr/bin/env bash

set -euo pipefail

PROPOSAL_API_URL="${PROPOSAL_API_URL:-http://localhost:5001}"
CONTRACTING_API_URL="${CONTRACTING_API_URL:-http://localhost:5002}"
SOLUTION_FILE="${SOLUTION_FILE:-insurance-platform.sln}"
HEALTH_TIMEOUT_SECONDS="${HEALTH_TIMEOUT_SECONDS:-90}"

COMPOSE_CMD=()
STARTED_COMPOSE=0

log() {
  printf '\n[%s] %s\n' "$(date '+%H:%M:%S')" "$*"
}

fail() {
  printf '\n[ERROR] %s\n' "$*" >&2
  exit 1
}

require_command() {
  command -v "$1" >/dev/null 2>&1 || fail "Comando obrigatorio nao encontrado: $1"
}

detect_compose() {
  if docker compose version >/dev/null 2>&1; then
    COMPOSE_CMD=(docker compose)
    return
  fi

  if command -v docker-compose >/dev/null 2>&1; then
    COMPOSE_CMD=(docker-compose)
    return
  fi

  fail "Docker Compose nao encontrado. Instale o plugin 'docker compose' ou o binario 'docker-compose'."
}

cleanup() {
  if [[ "${STARTED_COMPOSE}" -eq 1 ]]; then
    log "Derrubando docker-compose"
    "${COMPOSE_CMD[@]}" down --remove-orphans
  fi
}

wait_for_health() {
  local name="$1"
  local url="$2"
  local deadline

  deadline=$((SECONDS + HEALTH_TIMEOUT_SECONDS))

  log "Aguardando ${name} em ${url}/health"
  until curl --silent --fail "${url}/health" >/dev/null; do
    if (( SECONDS >= deadline )); then
      fail "${name} nao ficou healthy dentro de ${HEALTH_TIMEOUT_SECONDS}s"
    fi
    sleep 2
  done
}

request_json() {
  local method="$1"
  local url="$2"
  local payload="${3:-}"
  local expected_status="$4"
  local response_file status

  response_file="$(mktemp)"

  if [[ -n "${payload}" ]]; then
    status="$(curl --silent --show-error \
      --output "${response_file}" \
      --write-out '%{http_code}' \
      --request "${method}" \
      --header 'Content-Type: application/json' \
      --data "${payload}" \
      "${url}")"
  else
    status="$(curl --silent --show-error \
      --output "${response_file}" \
      --write-out '%{http_code}' \
      --request "${method}" \
      "${url}")"
  fi

  if [[ "${status}" != "${expected_status}" ]]; then
    printf '\n[ERROR] %s %s retornou HTTP %s, esperado %s\n' "${method}" "${url}" "${status}" "${expected_status}" >&2
    printf '[ERROR] Resposta:\n' >&2
    cat "${response_file}" >&2
    printf '\n' >&2
    rm -f "${response_file}"
    exit 1
  fi

  cat "${response_file}"
  rm -f "${response_file}"
}

request_no_content() {
  local method="$1"
  local url="$2"
  local expected_status="$3"
  local response_file status

  response_file="$(mktemp)"
  status="$(curl --silent --show-error \
    --output "${response_file}" \
    --write-out '%{http_code}' \
    --request "${method}" \
    "${url}")"

  if [[ "${status}" != "${expected_status}" ]]; then
    printf '\n[ERROR] %s %s retornou HTTP %s, esperado %s\n' "${method}" "${url}" "${status}" "${expected_status}" >&2
    printf '[ERROR] Resposta:\n' >&2
    cat "${response_file}" >&2
    printf '\n' >&2
    rm -f "${response_file}"
    exit 1
  fi

  rm -f "${response_file}"
}

main() {
  require_command dotnet
  require_command docker
  require_command curl

  if ! command -v jq >/dev/null 2>&1; then
    fail "jq nao encontrado. Instale com 'sudo apt-get update && sudo apt-get install -y jq' no Ubuntu/WSL."
  fi

  detect_compose
  trap cleanup EXIT

  log "Executando dotnet restore"
  dotnet restore "${SOLUTION_FILE}"

  log "Executando dotnet build"
  dotnet build "${SOLUTION_FILE}" --no-restore

  log "Executando testes unitarios"
  dotnet test "${SOLUTION_FILE}" --no-build --filter "Category!=Integration"

  log "Subindo docker-compose"
  "${COMPOSE_CMD[@]}" up --build -d
  STARTED_COMPOSE=1

  wait_for_health "Proposal API" "${PROPOSAL_API_URL}"
  wait_for_health "Contracting API" "${CONTRACTING_API_URL}"

  log "Criando proposta"
  proposal_payload='{
    "customerName": "Maria Silva",
    "insuranceType": "Auto",
    "coverageAmount": 15000
  }'
  proposal_response="$(request_json POST "${PROPOSAL_API_URL}/api/proposals" "${proposal_payload}" 201)"
  proposal_id="$(printf '%s' "${proposal_response}" | jq -r '.id')"

  [[ -n "${proposal_id}" && "${proposal_id}" != "null" ]] || fail "Nao foi possivel extrair proposalId da resposta: ${proposal_response}"

  log "Aprovando proposta ${proposal_id}"
  request_no_content PATCH "${PROPOSAL_API_URL}/api/proposals/${proposal_id}/approve" 204

  log "Criando contrato para proposta ${proposal_id}"
  contract_payload="$(jq -n --arg proposalId "${proposal_id}" '{proposalId: $proposalId}')"
  contract_response="$(request_json POST "${CONTRACTING_API_URL}/api/contracts" "${contract_payload}" 201)"
  contract_id="$(printf '%s' "${contract_response}" | jq -r '.id')"

  [[ -n "${contract_id}" && "${contract_id}" != "null" ]] || fail "Nao foi possivel extrair contractId da resposta: ${contract_response}"

  log "Consultando contrato ${contract_id}"
  contract_lookup_response="$(request_json GET "${CONTRACTING_API_URL}/api/contracts/${contract_id}" "" 200)"

  log "Relatorio final"
  cat <<REPORT
Harness executado com sucesso.

Build: OK
Testes unitarios: OK
Docker Compose: OK
Health Proposal API: OK (${PROPOSAL_API_URL}/health)
Health Contracting API: OK (${CONTRACTING_API_URL}/health)
Proposta criada: ${proposal_id}
Proposta aprovada: OK
Contrato criado: ${contract_id}
Contrato consultado:
$(printf '%s' "${contract_lookup_response}" | jq .)
REPORT
}

main "$@"
