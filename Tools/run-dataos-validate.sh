#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
tmp_db="$(mktemp)"
trap 'rm -f "$tmp_db"' EXIT

sqlite3 "$tmp_db" ".read $repo_root/DataOS/Migrations/001_initial.sql"
"$repo_root/DataOS/Validation/validate-dataos.sh" "$tmp_db"
