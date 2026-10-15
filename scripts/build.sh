#!/usr/bin/env bash
# Builds the Angular client and publishes the server into ./bin
# (same output as scripts/build.bat). Requires Node 18 and the .NET 10 SDK.
set -euo pipefail
cd "$(dirname "$0")/.."

if [ ! -d FinCore/ClientApp/node_modules ]; then
  (cd FinCore/ClientApp && npm ci)
fi
# Release publish also runs `npm run build` for the client (see FinCore.csproj).
dotnet publish FinCore/FinCore.csproj -c Release -o ./bin
