#!/usr/bin/env bash
# ==========================================================================
# CRM Solution — Local CI Test Runner
#
# Mirrors the CI/CD pipeline (ci-cd.yml) and runs all test suites locally
# with parallel execution where possible. Results are logged to:
#   logs/ci-test-run-<timestamp>.log
#
# Usage:
#   ./scripts/run-all-ci-tests.sh
# ==========================================================================
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
LOG_DIR="$REPO_ROOT/logs"
LOG_FILE="$LOG_DIR/ci-test-run-${TIMESTAMP}.log"
SUMMARY_FILE="$LOG_DIR/ci-test-summary-${TIMESTAMP}.log"
ERRORS_FILE="$LOG_DIR/ci-test-errors-${TIMESTAMP}.log"

mkdir -p "$LOG_DIR"

# Temp dirs for per-suite logs
TMPDIR_LOGS=$(mktemp -d)
trap 'rm -rf "$TMPDIR_LOGS"' EXIT

# --------------------------------------------------------------------------
# Helper: timestamped log
# --------------------------------------------------------------------------
ts() { echo "[$(date '+%H:%M:%S')]"; }

log() {
  echo "$(ts) $*" | tee -a "$LOG_FILE"
}

# --------------------------------------------------------------------------
# Phase 0 — Build everything first (sequential, required by all tests)
# --------------------------------------------------------------------------
log "=========================================================="
log "PHASE 0: BUILD"
log "=========================================================="

# Backend build
log "Building .NET backend (Release)..."
BACKEND_BUILD_LOG="$TMPDIR_LOGS/backend-build.log"
(
  cd "$REPO_ROOT/CRM.Backend"
  dotnet restore CRM.sln 2>&1
  dotnet build CRM.sln -c Release --no-restore 2>&1
) > "$BACKEND_BUILD_LOG" 2>&1
BACKEND_BUILD_RC=$?

if [ $BACKEND_BUILD_RC -ne 0 ]; then
  log "❌ Backend build FAILED (exit $BACKEND_BUILD_RC)"
  cat "$BACKEND_BUILD_LOG" >> "$LOG_FILE"
  echo "BACKEND BUILD FAILED — see $LOG_FILE" > "$SUMMARY_FILE"
  exit 1
fi
log "✅ Backend build succeeded"

# Frontend install
log "Installing frontend dependencies..."
FRONTEND_INSTALL_LOG="$TMPDIR_LOGS/frontend-install.log"
(
  cd "$REPO_ROOT/CRM.Frontend"
  npm ci --legacy-peer-deps 2>&1
) > "$FRONTEND_INSTALL_LOG" 2>&1
FRONTEND_INSTALL_RC=$?

if [ $FRONTEND_INSTALL_RC -ne 0 ]; then
  log "❌ Frontend npm ci FAILED (exit $FRONTEND_INSTALL_RC)"
  cat "$FRONTEND_INSTALL_LOG" >> "$LOG_FILE"
  echo "FRONTEND INSTALL FAILED — see $LOG_FILE" > "$SUMMARY_FILE"
  exit 1
fi
log "✅ Frontend dependencies installed"

# --------------------------------------------------------------------------
# Phase 1 — Run all test suites in PARALLEL
#
# Parallel groups:
#   Backend:  4 dotnet test processes (independent projects)
#   Frontend: 1 jest process (TypeScript check + unit tests)
# --------------------------------------------------------------------------
log "=========================================================="
log "PHASE 1: RUNNING ALL TESTS IN PARALLEL"
log "=========================================================="

declare -A PIDS
declare -A SUITE_NAMES
declare -A SUITE_LOGS
declare -A SUITE_RC

# --- Backend suite 1: CRM.Tests.Unit.Core ---
SUITE="backend-core-unit"
SUITE_NAMES[$SUITE]="Backend: Core Unit Tests (CRM.Tests.Unit.Core)"
SUITE_LOGS[$SUITE]="$TMPDIR_LOGS/${SUITE}.log"
(
  cd "$REPO_ROOT/CRM.Backend"
  dotnet test tests/Unit/Core/CRM.Tests.Unit.Core.csproj \
    -c Release --no-build \
    --logger "trx;LogFileName=core-unit-tests.trx" \
    --logger "console;verbosity=detailed" \
    --results-directory tests/TestResults/ 2>&1
) > "${SUITE_LOGS[$SUITE]}" 2>&1 &
PIDS[$SUITE]=$!

# --- Backend suite 2: CRM.Tests (root — main test suite) ---
SUITE="backend-main"
SUITE_NAMES[$SUITE]="Backend: Main Test Suite (tests/CRM.Tests.csproj)"
SUITE_LOGS[$SUITE]="$TMPDIR_LOGS/${SUITE}.log"
(
  cd "$REPO_ROOT/CRM.Backend"
  dotnet test tests/CRM.Tests.csproj \
    -c Release --no-build \
    --logger "trx;LogFileName=main-tests.trx" \
    --logger "console;verbosity=detailed" \
    --results-directory tests/TestResults/ 2>&1
) > "${SUITE_LOGS[$SUITE]}" 2>&1 &
PIDS[$SUITE]=$!

# --- Backend suite 3: CRM.Tests (subfolder — service/integration) ---
SUITE="backend-service-integration"
SUITE_NAMES[$SUITE]="Backend: Service & Integration Tests (tests/CRM.Tests/)"
SUITE_LOGS[$SUITE]="$TMPDIR_LOGS/${SUITE}.log"
(
  cd "$REPO_ROOT/CRM.Backend"
  dotnet test tests/CRM.Tests/CRM.Tests.csproj \
    -c Release --no-build \
    --logger "trx;LogFileName=service-integration-tests.trx" \
    --logger "console;verbosity=detailed" \
    --results-directory tests/TestResults/ 2>&1
) > "${SUITE_LOGS[$SUITE]}" 2>&1 &
PIDS[$SUITE]=$!

# --- Backend suite 4: CRM.SystemModule.Tests ---
SUITE="backend-system-module"
SUITE_NAMES[$SUITE]="Backend: System Module Tests (CRM.SystemModule.Tests)"
SUITE_LOGS[$SUITE]="$TMPDIR_LOGS/${SUITE}.log"
(
  cd "$REPO_ROOT/CRM.Backend"
  dotnet test tests/CRM.SystemModule.Tests/CRM.SystemModule.Tests.csproj \
    -c Release --no-build \
    --logger "trx;LogFileName=system-module-tests.trx" \
    --logger "console;verbosity=detailed" \
    --results-directory tests/TestResults/ 2>&1
) > "${SUITE_LOGS[$SUITE]}" 2>&1 &
PIDS[$SUITE]=$!

# --- Frontend: TypeScript check + unit tests ---
SUITE="frontend-tests"
SUITE_NAMES[$SUITE]="Frontend: TypeScript Check + Unit Tests"
SUITE_LOGS[$SUITE]="$TMPDIR_LOGS/${SUITE}.log"
(
  cd "$REPO_ROOT/CRM.Frontend"
  echo "=== TypeScript Check ==="
  npx tsc --noEmit 2>&1
  TSC_RC=$?
  echo ""
  echo "=== TypeScript Check exit code: $TSC_RC ==="
  echo ""
  echo "=== Unit Tests ==="
  CI=true npm run test:ci 2>&1
  TEST_RC=$?
  echo ""
  echo "=== Unit Tests exit code: $TEST_RC ==="
  # Fail if either failed
  if [ $TSC_RC -ne 0 ] || [ $TEST_RC -ne 0 ]; then
    exit 1
  fi
) > "${SUITE_LOGS[$SUITE]}" 2>&1 &
PIDS[$SUITE]=$!

# --------------------------------------------------------------------------
# Wait for all suites, collect exit codes
# --------------------------------------------------------------------------
log "Waiting for ${#PIDS[@]} parallel suites to complete..."
log ""

TOTAL_PASS=0
TOTAL_FAIL=0
FAILED_SUITES=()

for SUITE in "${!PIDS[@]}"; do
  PID=${PIDS[$SUITE]}
  wait "$PID" || true
  RC=$?
  SUITE_RC[$SUITE]=$RC

  # Append suite log to master log
  echo "" >> "$LOG_FILE"
  echo "==========================================================" >> "$LOG_FILE"
  echo "SUITE: ${SUITE_NAMES[$SUITE]}" >> "$LOG_FILE"
  echo "EXIT CODE: $RC" >> "$LOG_FILE"
  echo "==========================================================" >> "$LOG_FILE"
  cat "${SUITE_LOGS[$SUITE]}" >> "$LOG_FILE"
  echo "" >> "$LOG_FILE"

  if [ $RC -eq 0 ]; then
    log "  ✅ ${SUITE_NAMES[$SUITE]} — PASSED"
    TOTAL_PASS=$((TOTAL_PASS + 1))
  else
    log "  ❌ ${SUITE_NAMES[$SUITE]} — FAILED (exit $RC)"
    TOTAL_FAIL=$((TOTAL_FAIL + 1))
    FAILED_SUITES+=("$SUITE")
  fi
done

# --------------------------------------------------------------------------
# Phase 2 — Extract errors from failed suites
# --------------------------------------------------------------------------
log ""
log "=========================================================="
log "PHASE 2: ERROR EXTRACTION"
log "=========================================================="

> "$ERRORS_FILE"

for SUITE in "${FAILED_SUITES[@]}"; do
  echo "==========================================================" >> "$ERRORS_FILE"
  echo "FAILED SUITE: ${SUITE_NAMES[$SUITE]}" >> "$ERRORS_FILE"
  echo "==========================================================" >> "$ERRORS_FILE"

  SLOG="${SUITE_LOGS[$SUITE]}"

  if [[ "$SUITE" == frontend-* ]]; then
    # Extract Jest failures
    grep -A 20 "FAIL " "$SLOG" >> "$ERRORS_FILE" 2>/dev/null || true
    grep -B 2 -A 10 "● " "$SLOG" >> "$ERRORS_FILE" 2>/dev/null || true
    grep -B 2 -A 10 "Error:" "$SLOG" >> "$ERRORS_FILE" 2>/dev/null || true
    # TypeScript errors
    grep -E "error TS[0-9]+" "$SLOG" >> "$ERRORS_FILE" 2>/dev/null || true
  else
    # Extract dotnet test failures
    grep -B 2 -A 15 "Failed " "$SLOG" >> "$ERRORS_FILE" 2>/dev/null || true
    grep -B 1 -A 10 "Error Message:" "$SLOG" >> "$ERRORS_FILE" 2>/dev/null || true
    grep -B 1 -A 5 "Stack Trace:" "$SLOG" >> "$ERRORS_FILE" 2>/dev/null || true
    # Also grab the summary line
    grep -E "(Passed|Failed|Skipped)!" "$SLOG" >> "$ERRORS_FILE" 2>/dev/null || true
  fi

  echo "" >> "$ERRORS_FILE"
done

# --------------------------------------------------------------------------
# Summary
# --------------------------------------------------------------------------
log ""
log "=========================================================="
log "SUMMARY"
log "=========================================================="
log "Total suites: ${#PIDS[@]}"
log "Passed:       $TOTAL_PASS"
log "Failed:       $TOTAL_FAIL"
log ""
log "Full log:     $LOG_FILE"
log "Summary:      $SUMMARY_FILE"
if [ $TOTAL_FAIL -gt 0 ]; then
  log "Errors file:  $ERRORS_FILE"
fi

# Write machine-readable summary
{
  echo "TIMESTAMP=$TIMESTAMP"
  echo "TOTAL_SUITES=${#PIDS[@]}"
  echo "PASSED=$TOTAL_PASS"
  echo "FAILED=$TOTAL_FAIL"
  echo ""
  for SUITE in "${!SUITE_RC[@]}"; do
    RC=${SUITE_RC[$SUITE]}
    STATUS="PASS"
    [ $RC -ne 0 ] && STATUS="FAIL"
    echo "SUITE=$SUITE STATUS=$STATUS RC=$RC NAME=${SUITE_NAMES[$SUITE]}"
  done
  echo ""
  echo "LOG_FILE=$LOG_FILE"
  echo "ERRORS_FILE=$ERRORS_FILE"
} > "$SUMMARY_FILE"

log ""
if [ $TOTAL_FAIL -gt 0 ]; then
  log "❌ $TOTAL_FAIL suite(s) failed — see $ERRORS_FILE for details"
  exit 1
else
  log "✅ All suites passed!"
  exit 0
fi
