#!/usr/bin/env bash
# Runs the same checks as CI: client lint/build/audit, server build and unit tests.
set -euo pipefail
cd "$(dirname "$0")/.."

(cd FinCore/ClientApp && npm run lint && npm run build && npm audit --audit-level=critical)
dotnet build FinCore.sln -c Release
dotnet test FinCore.sln -c Release --no-build
