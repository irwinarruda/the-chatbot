#!/usr/bin/env sh
set -e

echo "[DockerRun] Starting entrypoint script"
if [ -z "${APPSETTINGS_PRODUCTION:-}" ]; then
  echo "[DockerRun][HINT] APPSETTINGS_PRODUCTION not set. Running without appsettings.Production.json" >&2
else
  echo "[DockerRun] APPSETTINGS_PRODUCTION length: $(printf '%s' "$APPSETTINGS_PRODUCTION" | wc -c) bytes"
  echo "[DockerRun] First 80 chars: $(printf '%s' "$APPSETTINGS_PRODUCTION" | head -c 30)"
  printf '%s' "$APPSETTINGS_PRODUCTION" > /App/appsettings.Production.json
  mkdir -p /Mcp
  printf '%s' "$APPSETTINGS_PRODUCTION" > /Mcp/appsettings.Production.json
  echo "[DockerRun] Wrote /App/appsettings.Production.json"
  echo "[DockerRun] Wrote /Mcp/appsettings.Production.json"
fi

exec make run-prod
