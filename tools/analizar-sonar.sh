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

# Código que solo se ve corriendo el juego (pantallas, widgets de Myra, dibujo 3D,
# proxies WCF y arranque). Se sigue analizando, pero no cuenta para la cobertura.
CLIENT_COVERAGE_EXCLUSIONS="src/Torres.Client/Screens/**,src/Torres.Client/Widgets/**,src/Torres.Client/UI/**,\
src/Torres.Client/Contracts/**,\
src/Torres.Client/Scene/*.cs,src/Torres.Client/Scene/ActionPoints/**,src/Torres.Client/Scene/Butterflies/**,\
src/Torres.Client/Scene/Cards/**,src/Torres.Client/Scene/Caterpillars/**,src/Torres.Client/Scene/Flowers/**,\
src/Torres.Client/Scene/Icons/**,src/Torres.Client/Scene/Sprout/**,src/Torres.Client/Scene/Table/**,\
src/Torres.Client/Scene/Board/*Shapes.cs,src/Torres.Client/Scene/Board/CellMarkers.cs,src/Torres.Client/Scene/Board/GardenMeshes.cs,\
src/Torres.Client/Scene/Rendering/GardenPalette.cs,src/Torres.Client/Scene/Rendering/MeshBuilder.cs,\
src/Torres.Client/Scene/Rendering/MeshPlacement.cs,src/Torres.Client/Scene/Rendering/SceneLighting.cs,\
src/Torres.Client/Scene/Rendering/SceneMeshes.cs,src/Torres.Client/Scene/Rendering/StaticMesh.cs,\
src/Torres.Client/Scene/Rendering/SurfacePoint.cs,src/Torres.Client/Scene/Rendering/VertexPositionNormalColor.cs,\
src/Torres.Client/Connection/AccountConnection.cs,src/Torres.Client/Connection/GameSessionConnection.cs,\
src/Torres.Client/Connection/RankingConnection.cs,src/Torres.Client/Connection/ServerStatusConnection.cs,\
src/Torres.Client/Avatars/AvatarCache.cs,src/Torres.Client/Avatars/AvatarView.cs,\
src/Torres.Client/Avatars/AvatarViewSet.cs,src/Torres.Client/Avatars/DefaultAvatarArt.cs,\
src/Torres.Client/Localization/ComputedLabel.cs,src/Torres.Client/Localization/LabeledText.cs,\
src/Torres.Client/Localization/LabeledTextBox.cs,src/Torres.Client/Localization/Localized*.cs,\
src/Torres.Client/Program.cs,src/Torres.Client/TorresGame.cs,src/Torres.Client/SettingsFailureGame.cs,\
src/Torres.Client/DoomLauncher.cs"
# El arranque del servidor, los repositorios de EF Core (necesitan PostgreSQL) y
# el canal que lee el OperationContext de CoreWCF (solo existe dentro de una llamada real).
SERVER_COVERAGE_EXCLUSIONS="src/Game.Server/**,src/Game.Persistence/**,src/Game.Services/Session/CallerChannel.cs"

case "$TARGET" in
  cliente)
    PROJECT_KEY="torres-client"
    SOLUTIONS=("Torres.Client.sln")
    COVERAGE_EXCLUSIONS="$CLIENT_COVERAGE_EXCLUSIONS"
    ;;
  servidor)
    PROJECT_KEY="torres-server"
    SOLUTIONS=("Torres.Server.sln")
    COVERAGE_EXCLUSIONS="$SERVER_COVERAGE_EXCLUSIONS"
    ;;
  todo)
    PROJECT_KEY="torres"
    SOLUTIONS=("Torres.Server.sln" "Torres.Client.sln")
    COVERAGE_EXCLUSIONS="$SERVER_COVERAGE_EXCLUSIONS,$CLIENT_COVERAGE_EXCLUSIONS"
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
  /d:sonar.coverage.exclusions="$COVERAGE_EXCLUSIONS" \
  /d:sonar.cs.opencover.reportsPaths="$COVERAGE_DIR/**/coverage.opencover.xml"

for solution in "${SOLUTIONS[@]}"; do
  step "Compilando $solution"
  dotnet build "$solution" --no-incremental
done

# --- 5. Pruebas y cobertura --------------------------------------------------
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
