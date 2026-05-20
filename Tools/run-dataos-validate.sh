#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
tmp_db="$(mktemp)"
tmp_dir="$(mktemp -d)"
trap 'rm -f "$tmp_db"; rm -rf "$tmp_dir"' EXIT

while IFS= read -r migration; do
    sqlite3 "$tmp_db" ".read $migration"
done < <(find "$repo_root/DataOS/Migrations" -maxdepth 1 -name '*.sql' | sort)
sqlite3 "$tmp_db" ".read $repo_root/DataOS/Authoring/Framework.seed.sql"
DATAOS_REPORT_PATH="$tmp_dir/validation-report.json" "$repo_root/DataOS/Validation/validate-dataos.sh" "$tmp_db"
DATAOS_VALIDATION_REPORT_PATH="$tmp_dir/generator-validation-report.json" \
    "$repo_root/DataOS/Generators/generate-runtime-snapshot.sh" "$tmp_db" "$tmp_dir/runtime_snapshot.json" >/dev/null

jq -e '.manifest and .descriptors and .records and .resources' "$tmp_dir/runtime_snapshot.json" >/dev/null
jq -e '.summary.errorCount == 0' "$tmp_dir/validation-report.json" >/dev/null
