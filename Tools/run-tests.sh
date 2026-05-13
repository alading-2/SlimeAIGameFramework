#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."
Tools/run-dataos-validate.sh
dotnet run --project Tests/SlimeAI.GameOS.Tests/SlimeAI.GameOS.Tests.csproj
