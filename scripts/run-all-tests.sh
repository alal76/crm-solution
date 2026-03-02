#!/bin/bash
# =============================================================================
# CRM Solution - Master Test Runner Script
# =============================================================================
# This script runs all tests and generates a comprehensive test report
# with version tracking for each component.
#
# Usage:
#   ./scripts/run-all-tests.sh              # Run all tests
#   ./scripts/run-all-tests.sh --backend    # Run backend tests only
#   ./scripts/run-all-tests.sh --frontend   # Run frontend tests only
#   ./scripts/run-all-tests.sh --e2e        # Run E2E tests only
#   ./scripts/run-all-tests.sh --bvt        # Run BVT tests only
#   ./scripts/run-all-tests.sh --quick      # Run quick smoke tests
# =============================================================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# Configuration
REPORT_DIR="${PROJECT_ROOT}/test-reports"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
REPORT_FILE="${REPORT_DIR}/test-report-${TIMESTAMP}.md"

# Default settings
RUN_BACKEND=true
RUN_FRONTEND=true
RUN_E2E=true
RUN_BVT=true
QUICK_MODE=false

# Test URLs
API_URL="${API_URL:-http://192.168.0.9:5000}"
FRONTEND_URL="${FRONTEND_URL:-http://192.168.0.9}"

# Counters
TOTAL_PASSED=0
TOTAL_FAILED=0
TOTAL_SKIPPED=0

# =============================================================================
# Helper Functions
# =============================================================================

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[PASS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[FAIL]${NC} $1"
}

# Get version from version.json
get_backend_version() {
    if [[ -f "${PROJECT_ROOT}/version.json" ]]; then
        cat "${PROJECT_ROOT}/version.json" | grep '"version"' | head -1 | sed 's/.*"version"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/'
    else
        echo "unknown"
    fi
}

# Get frontend version from package.json
get_frontend_version() {
    if [[ -f "${PROJECT_ROOT}/CRM.Frontend/package.json" ]]; then
        cat "${PROJECT_ROOT}/CRM.Frontend/package.json" | grep '"version"' | head -1 | sed 's/.*"version"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/'
    else
        echo "unknown"
    fi
}

# Get git commit hash
get_git_hash() {
    if command -v git &> /dev/null && [[ -d "${PROJECT_ROOT}/.git" ]]; then
        git -C "${PROJECT_ROOT}" rev-parse --short HEAD 2>/dev/null || echo "unknown"
    else
        echo "unknown"
    fi
}

# Get git branch
get_git_branch() {
    if command -v git &> /dev/null && [[ -d "${PROJECT_ROOT}/.git" ]]; then
        git -C "${PROJECT_ROOT}" rev-parse --abbrev-ref HEAD 2>/dev/null || echo "unknown"
    else
        echo "unknown"
    fi
}

# Parse command line arguments
parse_args() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            --backend)
                RUN_BACKEND=true
                RUN_FRONTEND=false
                RUN_E2E=false
                RUN_BVT=false
                shift
                ;;
            --frontend)
                RUN_BACKEND=false
                RUN_FRONTEND=true
                RUN_E2E=false
                RUN_BVT=false
                shift
                ;;
            --e2e)
                RUN_BACKEND=false
                RUN_FRONTEND=false
                RUN_E2E=true
                RUN_BVT=false
                shift
                ;;
            --bvt)
                RUN_BACKEND=false
                RUN_FRONTEND=false
                RUN_E2E=false
                RUN_BVT=true
                shift
                ;;
            --quick)
                QUICK_MODE=true
                shift
                ;;
            --api-url)
                API_URL="$2"
                shift 2
                ;;
            --help)
                echo "Usage: $0 [OPTIONS]"
                echo ""
                echo "Options:"
                echo "  --backend     Run backend tests only"
                echo "  --frontend    Run frontend tests only"
                echo "  --e2e         Run E2E tests only"
                echo "  --bvt         Run BVT tests only"
                echo "  --quick       Run quick smoke tests"
                echo "  --api-url     Set API URL (default: http://192.168.0.9:5000)"
                echo "  --help        Show this help message"
                exit 0
                ;;
            *)
                log_error "Unknown option: $1"
                exit 1
                ;;
        esac
    done
}

# =============================================================================
# Test Functions
# =============================================================================

run_backend_tests() {
    log_info "Running Backend Unit Tests..."
    
    cd "${PROJECT_ROOT}/CRM.Backend/tests"
    
    local result_file="${REPORT_DIR}/backend-results-${TIMESTAMP}.trx"
    
    if dotnet test --logger "trx;LogFileName=${result_file}" --no-build 2>&1 | tee "${REPORT_DIR}/backend-output.log"; then
        local passed=$(grep -c "outcome=\"Passed\"" "${result_file}" 2>/dev/null || echo "0")
        local failed=$(grep -c "outcome=\"Failed\"" "${result_file}" 2>/dev/null || echo "0")
        
        TOTAL_PASSED=$((TOTAL_PASSED + passed))
        TOTAL_FAILED=$((TOTAL_FAILED + failed))
        
        log_success "Backend tests: ${passed} passed, ${failed} failed"
        return 0
    else
        log_error "Backend tests failed"
        return 1
    fi
}

run_frontend_tests() {
    log_info "Running Frontend Unit Tests..."
    
    cd "${PROJECT_ROOT}/CRM.Frontend"
    
    if npm test -- --coverage --watchAll=false --passWithNoTests 2>&1 | tee "${REPORT_DIR}/frontend-output.log"; then
        # Parse Jest output for results
        local output=$(cat "${REPORT_DIR}/frontend-output.log")
        local passed=$(echo "$output" | grep -oP '\d+(?= passed)' | tail -1 || echo "0")
        local failed=$(echo "$output" | grep -oP '\d+(?= failed)' | tail -1 || echo "0")
        
        TOTAL_PASSED=$((TOTAL_PASSED + ${passed:-0}))
        TOTAL_FAILED=$((TOTAL_FAILED + ${failed:-0}))
        
        log_success "Frontend tests: ${passed:-0} passed, ${failed:-0} failed"
        return 0
    else
        log_error "Frontend tests failed"
        return 1
    fi
}

run_e2e_tests() {
    log_info "Running E2E Tests..."
    
    cd "${PROJECT_ROOT}/e2e-tests"
    
    # Install dependencies if needed
    if [[ ! -d "node_modules" ]]; then
        npm install
        npx playwright install --with-deps chromium
    fi
    
    local test_args="--project=chromium"
    if [[ "$QUICK_MODE" = true ]]; then
        test_args="$test_args tests/bvt/"
    fi
    
    if BASE_URL="${FRONTEND_URL}" npx playwright test $test_args --reporter=json 2>&1 | tee "${REPORT_DIR}/e2e-output.log"; then
        # Parse Playwright output
        local output=$(cat "${REPORT_DIR}/e2e-output.log")
        
        log_success "E2E tests completed"
        return 0
    else
        log_warning "Some E2E tests failed"
        return 1
    fi
}

run_bvt_tests() {
    log_info "Running BVT API Tests..."
    
    cd "${PROJECT_ROOT}/e2e-tests"
    
    # Install dependencies if needed
    if [[ ! -d "node_modules" ]]; then
        npm install
    fi
    
    if BASE_URL="${FRONTEND_URL}" npx playwright test tests/bvt/api-bvt.spec.ts --reporter=json 2>&1 | tee "${REPORT_DIR}/bvt-output.log"; then
        log_success "BVT tests completed"
        return 0
    else
        log_warning "Some BVT tests failed"
        return 1
    fi
}

# =============================================================================
# Report Generation
# =============================================================================

generate_report() {
    log_info "Generating test report..."
    
    local backend_version=$(get_backend_version)
    local frontend_version=$(get_frontend_version)
    local git_hash=$(get_git_hash)
    local git_branch=$(get_git_branch)
    
    cat > "${REPORT_FILE}" << EOF
# CRM Solution - Test Execution Report

**Generated:** $(date "+%Y-%m-%d %H:%M:%S")  
**Report ID:** ${TIMESTAMP}

---

## Version Information

| Component | Version | Details |
|-----------|---------|---------|
| Backend API | ${backend_version} | .NET 8.0 |
| Frontend | ${frontend_version} | React 18 |
| Git Commit | ${git_hash} | Branch: ${git_branch} |
| Test Runner | 1.0.0 | Bash script |

---

## Test Environment

| Setting | Value |
|---------|-------|
| API URL | ${API_URL} |
| Frontend URL | ${FRONTEND_URL} |
| Test Server | $(hostname) |
| OS | $(uname -s) |

---

## Test Results Summary

| Metric | Value |
|--------|-------|
| **Total Passed** | ${TOTAL_PASSED} |
| **Total Failed** | ${TOTAL_FAILED} |
| **Total Skipped** | ${TOTAL_SKIPPED} |
| **Pass Rate** | $(if [[ $((TOTAL_PASSED + TOTAL_FAILED)) -gt 0 ]]; then echo "scale=1; ${TOTAL_PASSED} * 100 / (${TOTAL_PASSED} + ${TOTAL_FAILED})" | bc; else echo "0"; fi)% |

---

## Test Suites Executed

EOF

    if [[ "$RUN_BACKEND" = true ]]; then
        echo "### Backend Unit Tests" >> "${REPORT_FILE}"
        echo "" >> "${REPORT_FILE}"
        if [[ -f "${REPORT_DIR}/backend-output.log" ]]; then
            echo "- Status: $(grep -q "Test Run Successful" "${REPORT_DIR}/backend-output.log" && echo "✅ Passed" || echo "❌ Failed")" >> "${REPORT_FILE}"
        fi
        echo "" >> "${REPORT_FILE}"
    fi

    if [[ "$RUN_FRONTEND" = true ]]; then
        echo "### Frontend Unit Tests" >> "${REPORT_FILE}"
        echo "" >> "${REPORT_FILE}"
        if [[ -f "${REPORT_DIR}/frontend-output.log" ]]; then
            echo "- Status: $(grep -q "Tests:.*passed" "${REPORT_DIR}/frontend-output.log" && echo "✅ Passed" || echo "❌ Failed")" >> "${REPORT_FILE}"
        fi
        echo "" >> "${REPORT_FILE}"
    fi

    if [[ "$RUN_E2E" = true ]]; then
        echo "### E2E Tests" >> "${REPORT_FILE}"
        echo "" >> "${REPORT_FILE}"
        if [[ -f "${REPORT_DIR}/e2e-output.log" ]]; then
            echo "- Status: $(grep -q "passed" "${REPORT_DIR}/e2e-output.log" && echo "✅ Completed" || echo "⚠️ Check logs")" >> "${REPORT_FILE}"
        fi
        echo "" >> "${REPORT_FILE}"
    fi

    if [[ "$RUN_BVT" = true ]]; then
        echo "### BVT Tests" >> "${REPORT_FILE}"
        echo "" >> "${REPORT_FILE}"
        if [[ -f "${REPORT_DIR}/bvt-output.log" ]]; then
            echo "- Status: $(grep -q "passed" "${REPORT_DIR}/bvt-output.log" && echo "✅ Completed" || echo "⚠️ Check logs")" >> "${REPORT_FILE}"
        fi
        echo "" >> "${REPORT_FILE}"
    fi

    cat >> "${REPORT_FILE}" << EOF

---

## Artifacts

- Backend results: \`test-reports/backend-results-${TIMESTAMP}.trx\`
- Frontend results: \`test-reports/frontend-output.log\`
- E2E results: \`test-reports/e2e-output.log\`
- BVT results: \`test-reports/bvt-output.log\`

---

## Next Steps

1. Review failed tests in detail
2. Fix any regressions
3. Update test coverage for gaps

---

*Report generated by run-all-tests.sh*
EOF

    log_success "Report generated: ${REPORT_FILE}"
}

# =============================================================================
# Main Execution
# =============================================================================

main() {
    parse_args "$@"
    
    echo ""
    echo "=============================================="
    echo "   CRM Solution - Test Runner"
    echo "=============================================="
    echo ""
    
    # Create report directory
    mkdir -p "${REPORT_DIR}"
    
    # Get versions
    log_info "Backend Version: $(get_backend_version)"
    log_info "Frontend Version: $(get_frontend_version)"
    log_info "Git Commit: $(get_git_hash)"
    log_info "Git Branch: $(get_git_branch)"
    echo ""
    
    # Run tests
    local exit_code=0
    
    if [[ "$RUN_BACKEND" = true ]]; then
        run_backend_tests || exit_code=1
    fi
    
    if [[ "$RUN_FRONTEND" = true ]]; then
        run_frontend_tests || exit_code=1
    fi
    
    if [[ "$RUN_BVT" = true ]]; then
        run_bvt_tests || exit_code=1
    fi
    
    if [[ "$RUN_E2E" = true ]]; then
        run_e2e_tests || exit_code=1
    fi
    
    # Generate report
    generate_report
    
    echo ""
    echo "=============================================="
    echo "   Test Execution Complete"
    echo "=============================================="
    echo ""
    echo "Total Passed: ${TOTAL_PASSED}"
    echo "Total Failed: ${TOTAL_FAILED}"
    echo "Report: ${REPORT_FILE}"
    echo ""
    
    exit $exit_code
}

main "$@"
