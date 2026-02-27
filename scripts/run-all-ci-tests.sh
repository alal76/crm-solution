#!/usr/bin/env bash
# ==========================================================================
# CRM Solution — Local CI Test Runner
#
# Mirrors the CI/CD pipeline (ci-cd.yml) and runs all test suites locally
# with parallel execution where possible. Results are logged to:
#   logs/ci-test-run-<timestamp>.log
#
# Compatible with bash 3.2+ (macOS default).
#
# Usage:
#   ./scripts/run-all-ci-tests.sh
# ==========================================================================

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
LOG_DIR="$REPO_ROOT/logs"
LOG_FILE="$LOG_DIR/ci-test-run-${TIMESTAMP}.log"
SUMMARY_FILE="$LOG_DIR/ci-test-summary-${TIMESTAMP}.log"
ERRORS_FILE="$LOG_DIR/ci-test-errors-${TIMESTAMP}.log"

mkdir -p "$LOG_DIR"

# Temp dirs for per-suite logs
TMPDIR_LOGS=$(mktemp -d)
cleanup() { rm -rf "$TMPDIR_LOGS"; }
trap cleanup EXIT

# --------------------------------------------------------------------------
# Suite definitions (indexed arrays — bash 3.2 compatible)
# --------------------------------------------------------------------------
SUITE_IDS=(   "backend-core-unit"   "backend-main"   "backend-service-integration"   "backend-system-module"   "frontend-tests")
SUITE_NAMES=( "Backend: Core Unit Tests (CRM.Tests.Unit.Core)" \
              "Backend: Main Test Suite (tests/CRM.Tests.csproj)" \
              "Backend: Service & Integration (tests/CRM.Tests/)" \
              "Backend: System Module Tests (CRM.SystemModule.Tests)" \
              "Frontend: TypeScript Check + Unit Tests")
NUM_SUITES=${#SUITE_IDS[@]}

# Will be filled during execution
SUITE_PIDS=()
SUITE_RCS=()

# --------------------------------------------------------------------------
# Helper: timestamped log
# --------------------------------------------------------------------------
log() {
  local msg="[$(date '+%H:%M:%S')] $*"
  echo "$msg"
  echo "$msg" >> "$LOG_FILE"
}

# --------------------------------------------------------------------------
# Phase 0 — Build everything (sequential, required by all tests)
# --------------------------------------------------------------------------
log "=========================================================="
log "PHASE 0: BUILD"
log "=========================================================="

# Backend build
log "Building .NET backend (Release)..."
BACKEND_BUILD_LOG="$TMPDIR_LOGS/backend-build.log"
BACKEND_BUILD_RC=0
(cd "$REPO_ROOT/CRM.Backend" && dotnet restore CRM.sln 2>&1 && dotnet build CRM.sln -c Release --no-restore 2>&1) > "$BACKEND_BUILD_LOG" 2>&1 || BACKEND_BUILD_RC=$?

if [[ $BACKEND_BUILD_RC -ne 0 ]]; then
  log "❌ Backend build FAILED (exit $BACKEND_BUILD_RC)"
  cat "$BACKEND_BUILD_LOG" >> "$LOG_FILE"
  echo "BACKEND BUILD FAILED" > "$SUMMARY_FILE"
  exit 1
fi
log "✅ Backend build succeeded"

# Frontend install
log "Installing frontend dependencies..."
FRONTEND_INSTALL_LOG="$TMPDIR_LOGS/frontend-install.log"
FRONTEND_INSTALL_RC=0
(cd "$REPO_ROOT/CRM.Frontend" && npm ci --legacy-peer-deps 2>&1) > "$FRONTEND_INSTALL_LOG" 2>&1 || FRONTEND_INSTALL_RC=$?

if [[ $FRONTEND_INSTALL_RC -ne 0 ]]; then
  log "❌ Frontend npm ci FAILED (exit $FRONTEND_INSTALL_RC)"
  cat "$FRONTEND_INSTALL_LOG" >> "$LOG_FILE"
  echo "FRONTEND INSTALL FAILED" > "$SUMMARY_FILE"
  exit 1
fi
log "✅ Frontend dependencies installed"

# --------------------------------------------------------------------------
# Phase 1 — Run all test suites in PARALLEL
# --------------------------------------------------------------------------
log "=========================================================="
log "PHASE 1: RUNNING ALL TESTS IN PARALLEL ($NUM_SUITES suites)"
log "=========================================================="

# Suite 0: Backend Core Unit
(
  cd "$REPO_ROOT/CRM.Backend"
  dotnet test tests/Unit/Core/CRM.Tests.Unit.Core.csproj \
    -c Release --no-build \
    --logger "trx;LogFileName=core-unit-tests.trx" \
    --logger "console;verbosity=detailed" \
    --results-directory tests/TestResults/ 2>&1
) > "$TMPDIR_LOGS/backend-core-unit.log" 2>&1 &
SUITE_PIDS[0]=$!
log "  Started ${SUITE_NAMES[0]} (PID ${SUITE_PIDS[0]})"

# Suite 1: Backend Main
(
  cd "$REPO_ROOT/CRM.Backend"
  dotnet test tests/CRM.Tests.csproj \
    -c Release --no-build \
    --logger "trx;LogFileName=main-tests.trx" \
    --logger "console;verbosity=detailed" \
    --results-directory tests/TestResults/ 2>&1
) > "$TMPDIR_LOGS/backend-main.log" 2>&1 &
SUITE_PIDS[1]=$!
log "  Started ${SUITE_NAMES[1]} (PID ${SUITE_PIDS[1]})"

# Suite 2: Backend Service & Integration
(
  cd "$REPO_ROOT/CRM.Backend"
  dotnet test tests/CRM.Tests/CRM.Tests.csproj \
    -c Release --no-build \
    --logger "trx;LogFileName=service-integration-tests.trx" \
    --logger "console;verbosity=detailed" \
    --results-directory tests/TestResults/ 2>&1
) > "$TMPDIR_LOGS/backend-service-integration.log" 2>&1 &
SUITE_PIDS[2]=$!
log "  Started ${SUITE_NAMES[2]} (PID ${SUITE_PIDS[2]})"

# Suite 3: Backend System Module
(
  cd "$REPO_ROOT/CRM.Backend"
  dotnet test tests/CRM.SystemModule.Tests/CRM.SystemModule.Tests.csproj \
    -c Release --no-build \
    --logger "trx;LogFileName=system-module-tests.trx" \
    --logger "console;verbosity=detailed" \
    --results-directory tests/TestResults/ 2>&1
) > "$TMPDIR_LOGS/backend-system-module.log" 2>&1 &
SUITE_PIDS[3]=$!
log "  Started ${SUITE_NAMES[3]} (PID ${SUITE_PIDS[3]})"

# Suite 4: Frontend Tests
(
  cd "$REPO_ROOT/CRM.Frontend"
  echo "=== TypeScript Check ==="
  TSC_RC=0
  npx tsc --noEmit 2>&1 || TSC_RC=$?
  echo ""
  echo "=== TypeScript Check exit code: $TSC_RC ==="
  echo ""
  echo "=== Unit Tests ==="
  TEST_RC=0
  CI=true npm run test:ci 2>&1 || TEST_RC=$?
  echo ""
  echo "=== Unit Tests exit code: $TEST_RC ==="
  if [[ $TSC_RC -ne 0 ]] || [[ $TEST_RC -ne 0 ]]; then exit 1; fi
) > "$TMPDIR_LOGS/frontend-tests.log" 2>&1 &
SUITE_PIDS[4]=$!
log "  Started ${SUITE_NAMES[4]} (PID ${SUITE_PIDS[4]})"

log ""
log "All $NUM_SUITES suites launched. Waiting for completion..."
log ""

# --------------------------------------------------------------------------
# Wait for all suites, collect exit codes
# --------------------------------------------------------------------------
TOTAL_PASS=0
TOTAL_FAIL=0
FAILED_INDICES=()

for i in $(seq 0 $((NUM_SUITES - 1))); do
  PID=${SUITE_PIDS[$i]}
  SID=${SUITE_IDS[$i]}
  SNAME=${SUITE_NAMES[$i]}
  SLOG="$TMPDIR_LOGS/${SID}.log"
  RC=0
  wait "$PID" || RC=$?
  SUITE_RCS[$i]=$RC

  # Append suite log to master log
  {
    echo ""
    echo "=========================================================="
    echo "SUITE: $SNAME"
    echo "EXIT CODE: $RC"
    echo "=========================================================="
    cat "$SLOG"
    echo ""
  } >> "$LOG_FILE"

  # Extract pass/fail counts from log
  PASS_LINE=$(grep -oE 'Passed:\s*[0-9]+' "$SLOG" 2>/dev/null | tail -1 || true)
  FAIL_LINE=$(grep -oE 'Failed:\s*[0-9]+' "$SLOG" 2>/dev/null | tail -1 || true)
  TOTAL_LINE=$(grep -oE 'Total tests:\s*[0-9]+' "$SLOG" 2>/dev/null | tail -1 || true)
  # Frontend uses different format
  FE_SUITES=$(grep -oE 'Test Suites:.*total' "$SLOG" 2>/dev/null | tail -1 || true)
  FE_TESTS=$(grep -oE 'Tests:.*total' "$SLOG" 2>/dev/null | tail -1 || true)

  COUNTS=""
  if [[ -n "$TOTAL_LINE" ]]; then
    COUNTS="[$TOTAL_LINE]"
  elif [[ -n "$FE_TESTS" ]]; then
    COUNTS="[$FE_SUITES | $FE_TESTS]"
  fi

  if [[ $RC -eq 0 ]]; then
    log "  ✅ $SNAME — PASSED $COUNTS"
    TOTAL_PASS=$((TOTAL_PASS + 1))
  else
    log "  ❌ $SNAME — FAILED (exit $RC) $COUNTS"
    TOTAL_FAIL=$((TOTAL_FAIL + 1))
    FAILED_INDICES+=($i)
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

if [[ ${#FAILED_INDICES[@]} -eq 0 ]]; then
  log "No failures — skipping error extraction."
else
  for i in "${FAILED_INDICES[@]}"; do
    SID=${SUITE_IDS[$i]}
    SNAME=${SUITE_NAMES[$i]}
    SLOG="$TMPDIR_LOGS/${SID}.log"

    {
      echo "=========================================================="
      echo "FAILED SUITE: $SNAME"
      echo "=========================================================="
    } >> "$ERRORS_FILE"

    if [[ "$SID" == frontend-* ]]; then
      # Jest failures
      grep -A 20 "FAIL " "$SLOG" >> "$ERRORS_FILE" 2>/dev/null || true
      grep -B 2 -A 10 "● " "$SLOG" >> "$ERRORS_FILE" 2>/dev/null || true
      grep -B 2 -A 10 "Error:" "$SLOG" >> "$ERRORS_FILE" 2>/dev/null || true
      grep -E "error TS[0-9]+" "$SLOG" >> "$ERRORS_FILE" 2>/dev/null || true
    else
      # dotnet test failures
      grep -B 2 -A 20 "Failed " "$SLOG" >> "$ERRORS_FILE" 2>/dev/null || true
      grep -B 1 -A 15 "Error Message:" "$SLOG" >> "$ERRORS_FILE" 2>/dev/null || true
      grep -B 1 -A 10 "Stack Trace:" "$SLOG" >> "$ERRORS_FILE" 2>/dev/null || true
      grep -E "Passed\!|Failed\!" "$SLOG" >> "$ERRORS_FILE" 2>/dev/null || true
    fi
    echo "" >> "$ERRORS_FILE"
  done
  log "Errors extracted to: $ERRORS_FILE"
fi

# --------------------------------------------------------------------------
# Summary
# --------------------------------------------------------------------------
log ""
log "=========================================================="
log "SUMMARY"
log "=========================================================="
log "Total suites: $NUM_SUITES"
log "Passed:       $TOTAL_PASS"
log "Failed:       $TOTAL_FAIL"
log ""
log "Full log:     $LOG_FILE"
log "Summary:      $SUMMARY_FILE"
if [[ $TOTAL_FAIL -gt 0 ]]; then
  log "Errors file:  $ERRORS_FILE"
fi

# Write machine-readable summary
{
  echo "TIMESTAMP=$TIMESTAMP"
  echo "TOTAL_SUITES=$NUM_SUITES"
  echo "PASSED=$TOTAL_PASS"
  echo "FAILED=$TOTAL_FAIL"
  echo ""
  for i in $(seq 0 $((NUM_SUITES - 1))); do
    RC=${SUITE_RCS[$i]}
    STATUS="PASS"
    [ "$RC" != "0" ] && STATUS="FAIL"
    echo "SUITE=${SUITE_IDS[$i]} STATUS=$STATUS RC=$RC NAME=${SUITE_NAMES[$i]}"
  done
  echo ""
  echo "LOG_FILE=$LOG_FILE"
  echo "ERRORS_FILE=$ERRORS_FILE"
} > "$SUMMARY_FILE"

log ""
if [[ $TOTAL_FAIL -gt 0 ]]; then
  log "❌ $TOTAL_FAIL suite(s) failed — see $ERRORS_FILE for details"
  exit 1
else
  log "✅ All suites passed!"
  exit 0
fi
