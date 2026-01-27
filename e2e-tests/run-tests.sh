#!/bin/bash

# CRM Solution - E2E Test Runner Script
# This script builds and runs the E2E test container

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Default values
BASE_URL="${BASE_URL:-http://192.168.0.9}"
TEST_USERNAME="${TEST_USERNAME:-admin}"
TEST_PASSWORD="${TEST_PASSWORD:-admin}"

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}CRM Solution - E2E Test Suite${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "${YELLOW}Base URL: ${BASE_URL}${NC}"
echo -e "${YELLOW}Test User: ${TEST_USERNAME}${NC}"
echo ""

# Change to e2e-tests directory
cd "$(dirname "$0")"

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo -e "${RED}Error: Docker is not running. Please start Docker and try again.${NC}"
    exit 1
fi

# Parse command line arguments
TEST_SUITE="all"
HEADED=false
DEBUG=false

while [[ "$#" -gt 0 ]]; do
    case $1 in
        --auth) TEST_SUITE="auth" ;;
        --customers) TEST_SUITE="customers" ;;
        --contacts) TEST_SUITE="contacts" ;;
        --opportunities) TEST_SUITE="opportunities" ;;
        --leads) TEST_SUITE="leads" ;;
        --service-requests) TEST_SUITE="service-requests" ;;
        --campaigns) TEST_SUITE="campaigns" ;;
        --workflows) TEST_SUITE="workflows" ;;
        --admin) TEST_SUITE="admin" ;;
        --dashboard) TEST_SUITE="dashboard" ;;
        --headed) HEADED=true ;;
        --debug) DEBUG=true ;;
        --url) BASE_URL="$2"; shift ;;
        --help)
            echo "Usage: $0 [options]"
            echo ""
            echo "Options:"
            echo "  --auth              Run authentication tests only"
            echo "  --customers         Run customer tests only"
            echo "  --contacts          Run contact tests only"
            echo "  --opportunities     Run opportunity tests only"
            echo "  --leads             Run lead tests only"
            echo "  --service-requests  Run service request tests only"
            echo "  --campaigns         Run campaign tests only"
            echo "  --workflows         Run workflow tests only"
            echo "  --admin             Run admin tests only"
            echo "  --dashboard         Run dashboard tests only"
            echo "  --headed            Run tests in headed mode (not in container)"
            echo "  --debug             Run tests in debug mode"
            echo "  --url <url>         Set base URL (default: http://192.168.0.9)"
            echo "  --help              Show this help message"
            exit 0
            ;;
        *) echo "Unknown option: $1"; exit 1 ;;
    esac
    shift
done

# Create output directories
mkdir -p test-results playwright-report test-logs

# Define CRM source root for copying logs
CRM_SOURCE_ROOT="$(dirname "$(pwd)")"

if [ "$HEADED" = true ]; then
    echo -e "${YELLOW}Running tests in headed mode (locally)...${NC}"
    npm install
    npx playwright install
    
    if [ "$TEST_SUITE" = "all" ]; then
        npm run test:headed
    else
        npx playwright test tests/$TEST_SUITE --headed
    fi
else
    # Build the Docker image
    echo -e "${YELLOW}Building E2E test container...${NC}"
    docker build -t crm-e2e-tests .

    # Run the tests
    echo -e "${YELLOW}Running E2E tests...${NC}"
    echo ""

    if [ "$DEBUG" = true ]; then
        docker run --rm \
            -e CI=true \
            -e BASE_URL="$BASE_URL" \
            -e TEST_USERNAME="$TEST_USERNAME" \
            -e TEST_PASSWORD="$TEST_PASSWORD" \
            -e DEBUG=pw:api \
            -v "$(pwd)/test-results:/app/e2e-tests/test-results" \
            -v "$(pwd)/playwright-report:/app/e2e-tests/playwright-report" \
            -v "$(pwd)/test-logs:/app/e2e-tests/test-logs" \
            --network host \
            crm-e2e-tests npm run test:debug
    elif [ "$TEST_SUITE" = "all" ]; then
        docker run --rm \
            -e CI=true \
            -e BASE_URL="$BASE_URL" \
            -e TEST_USERNAME="$TEST_USERNAME" \
            -e TEST_PASSWORD="$TEST_PASSWORD" \
            -v "$(pwd)/test-results:/app/e2e-tests/test-results" \
            -v "$(pwd)/playwright-report:/app/e2e-tests/playwright-report" \
            -v "$(pwd)/test-logs:/app/e2e-tests/test-logs" \
            --network host \
            crm-e2e-tests
    else
        docker run --rm \
            -e CI=true \
            -e BASE_URL="$BASE_URL" \
            -e TEST_USERNAME="$TEST_USERNAME" \
            -e TEST_PASSWORD="$TEST_PASSWORD" \
            -v "$(pwd)/test-results:/app/e2e-tests/test-results" \
            -v "$(pwd)/playwright-report:/app/e2e-tests/playwright-report" \
            -v "$(pwd)/test-logs:/app/e2e-tests/test-logs" \
            --network host \
            crm-e2e-tests npx playwright test "tests/$TEST_SUITE"
    fi
fi

# Copy logs to CRM source root
echo ""
echo -e "${YELLOW}Copying test logs to CRM source root...${NC}"
if [ -d "test-logs" ] && [ "$(ls -A test-logs 2>/dev/null)" ]; then
    mkdir -p "${CRM_SOURCE_ROOT}/test-logs"
    cp -r test-logs/* "${CRM_SOURCE_ROOT}/test-logs/"
    echo -e "${GREEN}Test logs copied to: ${CRM_SOURCE_ROOT}/test-logs/${NC}"
fi

# Check exit code
EXIT_CODE=$?

echo ""
if [ $EXIT_CODE -eq 0 ]; then
    echo -e "${GREEN}========================================${NC}"
    echo -e "${GREEN}All tests passed!${NC}"
    echo -e "${GREEN}========================================${NC}"
else
    echo -e "${RED}========================================${NC}"
    echo -e "${RED}Some tests failed!${NC}"
    echo -e "${RED}========================================${NC}"
    echo ""
    echo -e "${YELLOW}Check the test results in:${NC}"
    echo "  - test-results/ (screenshots, videos)"
    echo "  - playwright-report/ (HTML report)"
    echo "  - test-logs/ (human-readable logs)"
fi

echo ""
echo -e "${YELLOW}Test Logs Location:${NC}"
echo "  - Container logs: ./test-logs/"
echo "  - CRM Source Root: ${CRM_SOURCE_ROOT}/test-logs/"
echo ""
echo -e "${YELLOW}To view the HTML report:${NC}"
echo "  npx playwright show-report playwright-report"

exit $EXIT_CODE
