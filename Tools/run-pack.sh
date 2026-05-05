#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

dotnet pack GameOS/SkilmeAI.GameOS.csproj \
  --configuration Debug \
  --output Packages/LocalNuGet

