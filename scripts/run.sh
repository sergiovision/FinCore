#!/usr/bin/env bash
# Runs the server locally on http://localhost:2020 (serves FinCore/ClientApp/dist).
# Build the client first: scripts/build.sh or `npm run build` in FinCore/ClientApp.
set -euo pipefail
cd "$(dirname "$0")/../FinCore"
dotnet run -c Release
