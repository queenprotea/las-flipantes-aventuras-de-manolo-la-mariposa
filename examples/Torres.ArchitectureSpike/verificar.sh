#!/usr/bin/env bash
set -u

SPIKE_DIR="$(cd "$(dirname "$0")" && pwd)"
SERVER_PROJECT="$SPIKE_DIR/Torres.ArchitectureSpike.Server/Torres.ArchitectureSpike.Server.csproj"
CLIENT_PROJECT="$SPIKE_DIR/Torres.ArchitectureSpike.Client/Torres.ArchitectureSpike.Client.csproj"
PROBE_PROJECT="$SPIKE_DIR/Torres.ArchitectureSpike.DomainAccessProbe/Torres.ArchitectureSpike.DomainAccessProbe.csproj"
SERVER_OUTPUT="$SPIKE_DIR/Torres.ArchitectureSpike.Server/bin/Debug/net10.0"
CLIENT_OUTPUT="$SPIKE_DIR/Torres.ArchitectureSpike.Client/bin/Debug/net10.0"
SERVER_PORT="$(grep -o '"NetTcpPort": *[0-9]*' "$SPIKE_DIR/Torres.ArchitectureSpike.Server/appsettings.json" | grep -o '[0-9]*$')"
SERVER_LOG="$(mktemp)"
FAILED_STEPS=0

echo "== Compilando servidor y cliente"
dotnet build "$SERVER_PROJECT" --disable-build-servers -v q -nologo || exit 1
dotnet build "$CLIENT_PROJECT" --disable-build-servers -v q -nologo || exit 1

echo "== Arrancando el servidor en el puerto $SERVER_PORT"
(cd "$SERVER_OUTPUT" && exec dotnet Torres.ArchitectureSpike.Server.dll) > "$SERVER_LOG" 2>&1 &
SERVER_PID=$!
for attempt in $(seq 1 60); do
    nc -z localhost "$SERVER_PORT" 2>/dev/null && break
    sleep 0.5
done

echo "== Ejecutando las comprobaciones del cliente"
(cd "$CLIENT_OUTPUT" && dotnet Torres.ArchitectureSpike.Client.dll)
CLIENT_EXIT=$?
[ "$CLIENT_EXIT" -eq 0 ] || FAILED_STEPS=$((FAILED_STEPS + 1))

kill "$SERVER_PID" 2>/dev/null
wait "$SERVER_PID" 2>/dev/null
echo "== Registro del servidor"
grep -v "discarded: no live session" "$SERVER_LOG"
rm -f "$SERVER_LOG"

echo "== Sonda: un proyecto que solo referencia Match no debe ver Domain"
PROBE_OUTPUT="$(dotnet build "$PROBE_PROJECT" --disable-build-servers -v q -nologo 2>&1)"
if echo "$PROBE_OUTPUT" | grep -qE "error CS0234|error CS0246"; then
    echo "[PASS] La sonda no compila: Domain no es visible a traves de Match"
else
    echo "[FAIL] La sonda compilo o fallo por otra causa:"
    echo "$PROBE_OUTPUT" | tail -5
    FAILED_STEPS=$((FAILED_STEPS + 1))
fi

echo "== Pasos fallidos: $FAILED_STEPS"
exit "$FAILED_STEPS"
