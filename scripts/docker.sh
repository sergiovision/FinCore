#!/usr/bin/env bash
# Builds the FinCore docker image (docker-compose.yml at repo root).
set -euo pipefail
cd "$(dirname "$0")/.."
docker compose build "$@"
