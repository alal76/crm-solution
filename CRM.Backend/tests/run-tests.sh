#!/usr/bin/env bash
# =============================================================================
# CRM Test Batch Runner
# Executes all 3 test projects sequentially with per-project timeouts.
# Usage:  ./run-tests.sh [--timeout 300] [--verbosity normal] [--filter "FullyQualifiedName~Service"]
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
TESTS_DIR="$SCRIPT_DIR"

# Defaults
TIMEOUT_SECS=300          # 5 minutes per project
VERBOSITY="minimal"
FILTER=""
STOP_ON_FAIL=false
NO_BUILD=false

# Parse args
while [[ $# -gt 0 ]]; do
  case "$1" in
    --timeout)     TIMEOUT_SECS="$2"; shift 2 ;;
    --verbosity)   VERBOSITY="$2";    shift 2 ;;
    --filter)      FILTER="$2";       shift 2 ;;
    --stop-on-fail) STOP_ON_FAIL=true; shift ;;
    --no-build)    NO_BUILD=true;     shift ;;
    -h|--help)
      echo "Usage: $0 [--timeout SECS] [--verbosity LEVEL] [--filter EXPR] [--stop-on-fail] [--no-build]"
      exit 0 ;;
    *) echo "Unknown arg: $1"; exit 1 ;;
  esac
done

# ---------------------------------------------------------------------------
# Test project batches — ordered by dependency / size
# ---------------------------------------------------------------------------
declare -a PROJECTS=(
  "Unit/Core/CRM.Tests.Unit.Core.csproj"
  "CRM.Tests/CRM.Tests.csproj"
  "CRM.Tests.csproj"
)

declare -a PROJECT_LABELS=(
  "Batch 1/3 — Unit.Core (entity & DTO tests)"
  "Batch 2/3 — CRM.Tests.Services (service tests)"
  "Batch 3/3 — CRM.Tests (controllers, providers, integration)"
)

TOTAL=${#PROJECTS[@]}
PASSED=0
FAILED=0
TIMED_OUT=0
RESULTS=()

echo "╔══════════════════════════════════════════════════════════════╗"
echo "║              CRM Test Batch Runner                          ║"
echo "╠══════════════════════════════════════════════════════════════╣"
echo "║  Projects : $TOTAL                                            ║"
echo "║  Timeout  : ${TIMEOUT_SECS}s per project                              ║"
echo "║  Filter   : ${FILTER:-<none>}"
echo "╚══════════════════════════════════════════════════════════════╝"
echo ""

for i in "${!PROJECTS[@]}"; do
  PROJECT="${PROJECTS[$i]}"
  LABEL="${PROJECT_LABELS[$i]}"
  PROJ_PATH="$TESTS_DIR/$PROJECT"

  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  echo "▶ $LABEL"
  echo "  $PROJECT"
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

  if [[ ! -f "$PROJ_PATH" ]]; then
    echo "  ⚠️  Project file not found — skipping"
    RESULTS+=("⚠️  SKIP  $LABEL")
    continue
  fi

  CMD=(dotnet test "$PROJ_PATH" --verbosity "$VERBOSITY" --logger "console;verbosity=minimal")

  if $NO_BUILD; then
    CMD+=(--no-build)
  fi

  if [[ -n "$FILTER" ]]; then
    CMD+=(--filter "$FILTER")
  fi

  START_TIME=$SECONDS

  set +e
  timeout "${TIMEOUT_SECS}s" "${CMD[@]}" 2>&1
  EXIT_CODE=$?
  set -e

  ELAPSED=$(( SECONDS - START_TIME ))

  if [[ $EXIT_CODE -eq 124 ]]; then
    echo ""
    echo "  ⏱️  TIMED OUT after ${TIMEOUT_SECS}s"
    TIMED_OUT=$((TIMED_OUT + 1))
    RESULTS+=("⏱️  TIMEOUT  $LABEL  (${ELAPSED}s)")
  elif [[ $EXIT_CODE -ne 0 ]]; then
    echo ""
    echo "  ❌ FAILED (exit code $EXIT_CODE, ${ELAPSED}s)"
    FAILED=$((FAILED + 1))
    RESULTS+=("❌ FAIL     $LABEL  (${ELAPSED}s)")
    if $STOP_ON_FAIL; then
      echo "  --stop-on-fail set — aborting remaining batches"
      break
    fi
  else
    echo ""
    echo "  ✅ PASSED (${ELAPSED}s)"
    PASSED=$((PASSED + 1))
    RESULTS+=("✅ PASS     $LABEL  (${ELAPSED}s)")
  fi

  echo ""
done

# Summary
echo "╔══════════════════════════════════════════════════════════════╗"
echo "║                      BATCH SUMMARY                         ║"
echo "╠══════════════════════════════════════════════════════════════╣"
for R in "${RESULTS[@]}"; do
  printf "║  %-56s  ║\n" "$R"
done
echo "╠══════════════════════════════════════════════════════════════╣"
printf "║  Passed: %-3d  Failed: %-3d  Timed Out: %-3d  Total: %-3d    ║\n" "$PASSED" "$FAILED" "$TIMED_OUT" "$TOTAL"
echo "╚══════════════════════════════════════════════════════════════╝"

# Exit with failure if anything failed
if [[ $FAILED -gt 0 || $TIMED_OUT -gt 0 ]]; then
  exit 1
fi
