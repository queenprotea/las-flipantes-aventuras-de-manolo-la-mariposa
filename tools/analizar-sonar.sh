#!/usr/bin/env bash
# Analiza Torres con el SonarQube local.
#
# Uso:
#   bash tools/analizar-sonar.sh            # analiza el cliente (proyecto torres-client)
#   bash tools/analizar-sonar.sh servidor   # analiza el servidor (proyecto torres-server)
#   bash tools/analizar-sonar.sh todo       # cliente y servidor juntos (proyecto torres)
#
# El token se toma, en este orden, de la variable SONAR_TOKEN o del archivo
# ~/.config/sonar/torres-token. Si no hay ninguno, lo pide y ofrece guardarlo.
# Nunca se guarda dentro del repositorio.

set -euo pipefail

SONAR_URL="${SONAR_URL:-http://localhost:9000}"
TOKEN_FILE="${SONAR_TOKEN_FILE:-$HOME/.config/sonar/torres-token}"
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TARGET="${1:-cliente}"
WAIT_SECONDS=240

case "$TARGET" in
  cliente)
    PROJECT_KEY="torres-client"
    SOLUTIONS=("Torres.Client.sln")
    ;;
  servidor)
    PROJECT_KEY="torres-server"
    SOLUTIONS=("Torres.Server.sln")
    ;;
  todo)
    PROJECT_KEY="torres"
    SOLUTIONS=("Torres.Server.sln" "Torres.Client.sln")
    ;;
  *)
    echo "Uso: bash tools/analizar-sonar.sh [cliente|servidor|todo]" >&2
    exit 1
    ;;
esac

step() { printf '\n\033[1;35m==> %s\033[0m\n' "$1"; }
fail() { printf '\033[1;31mError:\033[0m %s\n' "$1" >&2; exit 1; }

sonar_status() {
  curl -s --max-time 3 "$SONAR_URL/api/system/status" 2>/dev/null \
    | grep -o '"status":"[A-Z_]*"' | cut -d'"' -f4 || true
}

docker_cmd() {
  if docker info >/dev/null 2>&1; then
    docker "$@"
  else
    sudo docker "$@"
  fi
}

# --- 1. Herramientas ---------------------------------------------------------
step "Revisando herramientas"
export PATH="$PATH:$HOME/.dotnet/tools"
command -v dotnet >/dev/null || fail "no se encontró dotnet."
command -v dotnet-sonarscanner >/dev/null \
  || fail "no se encontró dotnet-sonarscanner. Instálalo con: dotnet tool install --global dotnet-sonarscanner"
command -v curl >/dev/null || fail "no se encontró curl."
echo "dotnet y dotnet-sonarscanner listos."

# --- 2. SonarQube ------------------------------------------------------------
step "Revisando SonarQube en $SONAR_URL"
if [ "$(sonar_status)" != "UP" ]; then
  command -v docker >/dev/null || fail "SonarQube no responde y no se encontró docker para arrancarlo."

  map_count="$(cat /proc/sys/vm/max_map_count)"
  if [ "$map_count" -lt 262144 ]; then
    echo "vm.max_map_count está en $map_count; SonarQube necesita 262144. Se ajusta con sudo."
    sudo sysctl -w vm.max_map_count=262144 >/dev/null
  fi

  if ! systemctl is-active --quiet docker 2>/dev/null; then
    echo "Docker no está corriendo; se arranca con sudo."
    sudo systemctl start docker
  fi

  container="$(docker_cmd ps -a --format '{{.Names}} {{.Image}}' | awk '$2 ~ /sonarqube/ {print $1; exit}')"
  [ -n "$container" ] || fail "no hay ningún contenedor de SonarQube. Revisa con: docker ps -a"
  echo "Arrancando el contenedor '$container'…"
  docker_cmd start "$container" >/dev/null

  waited=0
  until [ "$(sonar_status)" = "UP" ]; do
    [ "$waited" -lt "$WAIT_SECONDS" ] \
      || fail "SonarQube no quedó listo en $WAIT_SECONDS s. Revisa con: docker logs $container"
    printf '.'
    sleep 5
    waited=$((waited + 5))
  done
  echo
fi
echo "SonarQube está arriba."

# --- 3. Token ----------------------------------------------------------------
step "Leyendo el token"
if [ -n "${SONAR_TOKEN:-}" ]; then
  token="$SONAR_TOKEN"
elif [ -f "$TOKEN_FILE" ]; then
  token="$(<"$TOKEN_FILE")"
else
  echo "Genera uno en $SONAR_URL/account/security si no lo tienes."
  read -rsp "Token de SonarQube: " token
  echo
  read -rp "¿Guardarlo en $TOKEN_FILE para la próxima vez? [s/N] " save_token
  if [[ "$save_token" =~ ^[sS]$ ]]; then
    mkdir -p "$(dirname "$TOKEN_FILE")"
    (umask 077 && printf '%s' "$token" > "$TOKEN_FILE")
    echo "Guardado (solo tu usuario puede leerlo)."
  fi
fi

validation="$(curl -s --max-time 5 -u "$token:" "$SONAR_URL/api/authentication/validate" || true)"
[[ "$validation" == *'"valid":true'* ]] \
  || fail "SonarQube rechazó el token. Genera uno nuevo en $SONAR_URL/account/security (y borra $TOKEN_FILE si lo guardaste)."
echo "Token válido."

# --- 4. Análisis y compilación ----------------------------------------------
cd "$REPO_ROOT"

# El prototipo HTML de docs/ y los spikes de examples/ no son parte del juego.
EXCLUSIONS="docs/**,examples/**,reportes-sonar/**,tools/**"
# Fuera de .sonarqube/, porque "dotnet-sonarscanner begin" borra esa carpeta.
COVERAGE_DIR="$(mktemp -d -t torres-cobertura-XXXXXX)"
trap 'rm -rf "$COVERAGE_DIR"' EXIT

step "Iniciando el análisis de '$PROJECT_KEY'"
dotnet-sonarscanner begin /k:"$PROJECT_KEY" /d:sonar.host.url="$SONAR_URL" /d:sonar.token="$token" \
  /d:sonar.exclusions="$EXCLUSIONS" \
  /d:sonar.cs.opencover.reportsPaths="$COVERAGE_DIR/**/coverage.opencover.xml"

for solution in "${SOLUTIONS[@]}"; do
  step "Compilando $solution"
  dotnet build "$solution" --no-incremental
done

# --- 5. Pruebas y cobertura --------------------------------------------------
# Las soluciones sin proyectos de prueba (hoy, el cliente) no generan cobertura.
# Si alguna prueba falla se avisa, pero el análisis se envía de todos modos.
for solution in "${SOLUTIONS[@]}"; do
  step "Corriendo las pruebas de $solution"
  dotnet test "$solution" --no-build \
    --collect "XPlat Code Coverage" \
    --settings "$REPO_ROOT/tools/cobertura.runsettings" \
    --results-directory "$COVERAGE_DIR" \
    || printf '\033[1;33mAviso:\033[0m fallaron pruebas de %s; la cobertura puede quedar incompleta.\n' "$solution"
done

coverage_files="$(find "$COVERAGE_DIR" -name coverage.opencover.xml 2>/dev/null | wc -l || true)"
echo "Reportes de cobertura generados: $coverage_files"

# --- 6. Envío ----------------------------------------------------------------
step "Enviando los resultados"
dotnet-sonarscanner end /d:sonar.token="$token"

step "Listo"
echo "Resultados: $SONAR_URL/dashboard?id=$PROJECT_KEY"
echo "(SonarQube tarda unos segundos en procesarlos antes de mostrarlos.)"
