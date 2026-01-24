#!/bin/bash

# Test Infrastructure Validation Script
# Verifies all test files and configurations are in place

set -e

echo "╔════════════════════════════════════════════════════════════════════╗"
echo "║        CRM Solution - Testing Infrastructure Validation            ║"
echo "╚════════════════════════════════════════════════════════════════════╝"
echo ""

SOLUTION_DIR="/Users/alal/Code/Git CRM Solution/crm-solution"
FRONTEND_DIR="$SOLUTION_DIR/CRM.Frontend"
BACKEND_DIR="$SOLUTION_DIR/CRM.Backend"

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

FAILED=0
PASSED=0

# Function to check if file exists
check_file() {
  local file=$1
  local description=$2
  
  if [ -f "$file" ]; then
    echo -e "${GREEN}✓${NC} $description"
    ((PASSED++))
  else
    echo -e "${RED}✗${NC} $description (NOT FOUND)"
    ((FAILED++))
  fi
}

# Function to check if directory exists
check_dir() {
  local dir=$1
  local description=$2
  
  if [ -d "$dir" ]; then
    echo -e "${GREEN}✓${NC} $description"
    ((PASSED++))
  else
    echo -e "${RED}✗${NC} $description (NOT FOUND)"
    ((FAILED++))
  fi
}

# ============================================================================
# 1. FRONTEND TEST INFRASTRUCTURE
# ============================================================================
echo -e "${BLUE}1. FRONTEND TEST INFRASTRUCTURE${NC}"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

check_dir "$FRONTEND_DIR/src/__tests__" "Frontend tests directory"
check_file "$FRONTEND_DIR/src/__tests__/LoginPage.test.tsx" "LoginPage test file"
check_file "$FRONTEND_DIR/src/__tests__/CustomersPage.test.tsx" "CustomersPage test file"
check_file "$FRONTEND_DIR/src/__tests__/apiClient.test.ts" "API client test file"
check_file "$FRONTEND_DIR/src/setupTests.ts" "Jest setup configuration"
check_file "$FRONTEND_DIR/jest.config.json" "Jest configuration file"
check_file "$FRONTEND_DIR/package.json" "Frontend package.json"

echo ""

# ============================================================================
# 2. BACKEND TEST INFRASTRUCTURE
# ============================================================================
echo -e "${BLUE}2. BACKEND TEST INFRASTRUCTURE${NC}"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

check_dir "$BACKEND_DIR/tests" "Backend tests directory"
check_file "$BACKEND_DIR/tests/CRM.Tests.csproj" "Test project file"
check_file "$BACKEND_DIR/tests/Controllers/DepartmentsControllerTests.cs" "Departments controller tests"
check_file "$BACKEND_DIR/tests/Controllers/CustomersControllerTests.cs" "Customers controller tests"
check_file "$BACKEND_DIR/tests/CRM.Tests/EntityTests.cs" "Entity tests"
check_file "$BACKEND_DIR/tests/CRM.Tests/UserEntityTests.cs" "User entity tests"
check_file "$BACKEND_DIR/CRM.sln" "Solution file"

echo ""

# ============================================================================
# 3. BUILD & TEST SCRIPTS
# ============================================================================
echo -e "${BLUE}3. BUILD & TEST SCRIPTS${NC}"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

check_file "$SOLUTION_DIR/scripts/run-tests.sh" "Test runner script"
check_file "$SOLUTION_DIR/scripts/verify-build.sh" "Build verification script"
check_file "$SOLUTION_DIR/.github/workflows/ci-cd.yml" "GitHub Actions workflow"

echo ""

# ============================================================================
# 4. DOCUMENTATION
# ============================================================================
echo -e "${BLUE}4. DOCUMENTATION${NC}"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

check_file "$SOLUTION_DIR/TESTING_GUIDE.md" "Testing guide documentation"
check_file "$SOLUTION_DIR/TESTING_STATUS.md" "Testing status document"

echo ""

# ============================================================================
# 5. FILE STATISTICS
# ============================================================================
echo -e "${BLUE}5. FILE STATISTICS${NC}"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

# Count test files
FRONTEND_TEST_FILES=$(find "$FRONTEND_DIR/src/__tests__" -name "*.test.ts*" 2>/dev/null | wc -l)
BACKEND_TEST_FILES=$(find "$BACKEND_DIR/tests" -name "*Tests.cs" 2>/dev/null | wc -l)

echo -e "${GREEN}Frontend test files:${NC} $FRONTEND_TEST_FILES"
echo -e "${GREEN}Backend test files:${NC} $BACKEND_TEST_FILES"

# Count test cases (approximate)
if command -v grep &> /dev/null; then
  FRONTEND_TEST_CASES=$(grep -r "it('\\|it(\"" "$FRONTEND_DIR/src/__tests__" 2>/dev/null | wc -l)
  BACKEND_TEST_CASES=$(grep -r "\[Fact\]\|\[Theory\]" "$BACKEND_DIR/tests" 2>/dev/null | wc -l)
  
  echo -e "${GREEN}Frontend test cases:${NC} ~$FRONTEND_TEST_CASES"
  echo -e "${GREEN}Backend test cases:${NC} ~$BACKEND_TEST_CASES"
fi

echo ""

# ============================================================================
# 6. VALIDATION SUMMARY
# ============================================================================
echo -e "${BLUE}6. VALIDATION SUMMARY${NC}"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

TOTAL=$((PASSED + FAILED))

echo ""
echo "Results:"
echo -e "  ${GREEN}Passed:${NC} $PASSED"
echo -e "  ${RED}Failed:${NC} $FAILED"
echo -e "  ${BLUE}Total:${NC}  $TOTAL"
echo ""

if [ $FAILED -eq 0 ]; then
  echo -e "${GREEN}✅ All test infrastructure files are in place!${NC}"
  echo ""
  echo "Next steps:"
  echo "  1. Run tests: ./scripts/run-tests.sh"
  echo "  2. Verify build: ./scripts/verify-build.sh"
  echo "  3. Push to GitHub to trigger CI/CD pipeline"
  echo ""
  exit 0
else
  echo -e "${RED}❌ Some test infrastructure files are missing!${NC}"
  echo ""
  exit 1
fi
