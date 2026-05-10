#!/usr/bin/env bash
set -euo pipefail

base_dir=".ai-temp/scene-tests/runs"
target_dir=""

usage() {
    cat >&2 <<'USAGE'
Usage:
  analyze-godot-scene-logs.sh [--run-dir <path>] [--base-dir <path>]

Reads the latest run under .ai-temp/scene-tests/runs by default and prints
a compact PASS/FAIL/error summary. New structured runs use index.json;
legacy flat run directories are still summarized for compatibility.
USAGE
}

while [ "$#" -gt 0 ]; do
    case "$1" in
        --run-dir)
            target_dir="${2:-}"
            if [ -z "$target_dir" ]; then
                echo "--run-dir requires a path." >&2
                exit 2
            fi
            shift 2
            ;;
        --base-dir)
            base_dir="${2:-}"
            if [ -z "$base_dir" ]; then
                echo "--base-dir requires a path." >&2
                exit 2
            fi
            shift 2
            ;;
        -h|--help)
            usage
            exit 0
            ;;
        *)
            echo "Unknown option: $1" >&2
            usage
            exit 2
            ;;
    esac
done

if [ -z "$target_dir" ]; then
    if [ ! -d "$base_dir" ]; then
        echo "No scene test log directory found: $base_dir" >&2
        exit 1
    fi

    target_dir="$(find "$base_dir" -mindepth 2 -maxdepth 2 -type d | sort | tail -n 1)"
    if [ -z "$target_dir" ]; then
        echo "No scene test runs found under: $base_dir" >&2
        exit 1
    fi
fi

if [ ! -d "$target_dir" ]; then
    echo "Run directory not found: $target_dir" >&2
    exit 1
fi

echo "Scene test run: $target_dir"

normalize_status() {
    local value="$1"
    printf '%s\n' "$value" | tr '[:upper:]' '[:lower:]'
}

artifact_status_for_result() {
    local result_file="$1"
    jq -r '
      (.artifacts.files // [])
      | map(select(endswith(".json") and (endswith("scene-log.jsonl") | not)))
      | .[]
    ' "$result_file" 2>/dev/null | while IFS= read -r artifact_file; do
        if [ -f "$artifact_file" ]; then
            jq -r '.status // empty' "$artifact_file" 2>/dev/null || true
        fi
    done | head -n 1
}

marker_status_for_combined() {
    local combined_file="$1"
    if [ ! -f "$combined_file" ]; then
        printf 'unknown\n'
        return
    fi

    if rg -q "\\[FAIL\\]| playable slice FAIL| validation FAIL|GameOS smoke FAIL|BrotatoLike GameOS smoke FAIL" "$combined_file"; then
        printf 'fail\n'
    elif rg -q "\\[PASS\\]| playable slice PASS| validation PASS|BrotatoLike GameOS smoke PASS" "$combined_file"; then
        printf 'pass\n'
    else
        printf 'unknown\n'
    fi
}

error_marker_for_combined() {
    local combined_file="$1"
    if [ ! -f "$combined_file" ]; then
        return
    fi

    rg -n -m 1 "ERROR:|\\[ERROR\\]|\\[FAIL\\]|FAIL:|Exception|Cannot instantiate|Failed to load|scene not found" "$combined_file" || true
}

if [ -f "$target_dir/index.json" ]; then
    echo "Format: structured index.json"
    echo

    failed=0
    while IFS= read -r entry; do
        scene="$(jq -r '.scene' <<<"$entry")"
        attempt="$(jq -r '.attempt' <<<"$entry")"
        result_file="$(jq -r '.logFiles.result // empty' <<<"$entry")"
        combined_file="$(jq -r '.logFiles.combined // empty' <<<"$entry")"
        exit_code="$(jq -r '.exitCode // empty' <<<"$entry")"
        first_error="$(jq -r '.firstError // empty' <<<"$entry")"
        failure_reason="$(jq -r '.failureReason // empty' <<<"$entry")"
        artifact_dir="$(jq -r '.artifactDirs.artifacts // empty' <<<"$entry")"
        artifact_status=""
        jsonl_count=0

        if [ -n "$result_file" ] && [ -f "$result_file" ]; then
            artifact_status="$(artifact_status_for_result "$result_file" | head -n 1)"
        fi

        marker_status="$(marker_status_for_combined "$combined_file")"
        error_marker="$(error_marker_for_combined "$combined_file")"

        if [ -n "$artifact_dir" ] && [ -f "$artifact_dir/logs/scene-log.jsonl" ]; then
            jsonl_count="$(wc -l < "$artifact_dir/logs/scene-log.jsonl" | tr -d ' ')"
        fi

        if [ -n "$exit_code" ] && [ "$exit_code" != "0" ]; then
            status="fail"
            [ -n "$failure_reason" ] || failure_reason="ExitCodeNonZero:$exit_code"
        elif [ -n "$artifact_status" ] && [ "$(normalize_status "$artifact_status")" = "fail" ]; then
            status="fail"
            [ -n "$failure_reason" ] || failure_reason="ArtifactStatusFail"
        elif [ -n "$artifact_status" ] && [ "$(normalize_status "$artifact_status")" = "pass" ]; then
            status="pass"
        elif [ "$marker_status" = "fail" ]; then
            status="fail"
            [ -n "$failure_reason" ] || failure_reason="ExplicitFailMarker"
        elif [ "$marker_status" = "pass" ]; then
            status="pass"
        elif [ -n "$error_marker" ]; then
            status="needs_review"
            [ -n "$failure_reason" ] || failure_reason="ErrorMarker"
        else
            status="unknown"
        fi

        if [ "$status" != "pass" ]; then
            failed=1
        fi

        echo "Scene: $scene (attempt $attempt)"
        echo "  status: $status"
        echo "  exitCode: ${exit_code:-unknown}"
        echo "  failureReason: ${failure_reason:-none}"
        echo "  firstError: ${first_error:-${error_marker:-none}}"
        echo "  combinedLog: ${combined_file:-none}"
        echo "  jsonlLogCount: $jsonl_count"

        if [ -n "$result_file" ] && [ -f "$result_file" ]; then
            artifacts="$(jq -r '(.artifacts.files // [])[]' "$result_file" 2>/dev/null || true)"
            if [ -n "$artifacts" ]; then
                echo "  artifacts:"
                printf '%s\n' "$artifacts" | sed 's/^/    /'
            else
                echo "  artifacts: none"
            fi
        else
            echo "  artifacts: result.json not found"
        fi
        echo
    done < <(jq -c '.entries[]' "$target_dir/index.json")

    if [ "$failed" -ne 0 ]; then
        exit 1
    fi
    exit 0
fi

stdout_file="$target_dir/stdout.log"
stderr_file="$target_dir/stderr.log"
acceptance_file="$target_dir/artifacts/scene-acceptance.json"
artifacts_dir="$target_dir/artifacts"

echo "Format: legacy flat run directory"

if [ -f "$stdout_file" ] && rg -q "BrotatoLike playable slice PASS" "$stdout_file"; then
    echo "Status: playable slice PASS marker found"
elif [ -f "$stdout_file" ] && rg -q "BrotatoLike playable slice FAIL" "$stdout_file"; then
    echo "Status: playable slice FAIL marker found"
elif [ -f "$stdout_file" ] && rg -q "BrotatoLike GameOS smoke PASS|PASS|\\[PASS\\]" "$stdout_file"; then
    echo "Status: PASS marker found"
else
    echo "Status: PASS marker not found"
fi

if [ -f "$acceptance_file" ]; then
    artifact_status="$(jq -r '.status // "unknown"' "$acceptance_file" 2>/dev/null || printf 'unreadable')"
    echo "Acceptance artifact: $acceptance_file ($artifact_status)"
elif [ -d "$artifacts_dir" ] && find "$artifacts_dir" -maxdepth 1 -name '*.json' -type f | rg -q .; then
    echo "Artifacts:"
    while IFS= read -r artifact_file; do
        artifact_status="$(jq -r '.status // "unknown"' "$artifact_file" 2>/dev/null || printf 'unreadable')"
        artifact_logs="$(jq -r '(.logs // []) | length' "$artifact_file" 2>/dev/null || printf '0')"
        artifact_failures="$(jq -r '(.failureReasons // .failure_reasons // []) | length' "$artifact_file" 2>/dev/null || printf '0')"
        echo "  $artifact_file ($artifact_status, logs=$artifact_logs, failures=$artifact_failures)"
    done < <(find "$artifacts_dir" -maxdepth 1 -name '*.json' -type f | sort)
else
    echo "Artifacts: not found"
fi

matches="$(rg -n -m 40 "ERROR:|\\[ERROR\\]|\\[FAIL\\]|FAIL:|Exception|Cannot instantiate|Failed to load|scene not found" "$target_dir" || true)"
if [ -n "$matches" ]; then
    echo
    echo "First error markers:"
    printf '%s\n' "$matches"
else
    echo "Error markers: none"
fi
