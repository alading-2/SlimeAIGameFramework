#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."
Tools/run-dataos-validate.sh
dotnet run --project Tests/SkilmeAI.GameOS.Tests/SkilmeAI.GameOS.Tests.csproj
