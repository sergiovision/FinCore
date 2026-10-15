#!/usr/bin/env bash
# Restores NuGet and npm packages.
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet restore FinCore.sln
(cd FinCore/ClientApp && npm ci)
