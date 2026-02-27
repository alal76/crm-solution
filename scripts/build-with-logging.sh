#!/bin/bash
# Build script with centralized logging
# Saves build logs to docs/buildlogs/ and removes old logs before each build

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
LOGS_DIR="$SCRIPT_DIR/docs/buildlogs"
API_LOG="$LOGS_DIR/api-build.log"
FRONTEND_LOG="$LOGS_DIR/frontend-build.log"

# Create logs directory
mkdir -p "$LOGS_DIR"

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}================================${NC}"
echo -e "${BLUE}CRM Solution Build with Logging${NC}"
echo -e "${BLUE}================================${NC}"
echo
echo -e "${YELLOW}Logs directory: $LOGS_DIR${NC}"

# Function to clean old logs
clean_logs() {
    local log_file=$1
    if [[ -f "$log_file" ]]; then
        rm -f "$log_file"
        echo -e "${YELLOW}Removed old log: $(basename $log_file)${NC}"
    fi
}

# Function to build API
build_api() {
    echo
    echo -e "${BLUE}>>> Building Backend API${NC}"
    clean_logs "$API_LOG"
    
    echo "Started: $(date)" > "$API_LOG"
    echo "Build command: docker buildx build --no-cache --platform linux/amd64 -t crm-api:latest -f docker/Dockerfile.backend ." >> "$API_LOG"
    echo "---" >> "$API_LOG"
    
    if cd "$SCRIPT_DIR" && docker buildx build --no-cache --platform linux/amd64 -t crm-api:latest -f docker/Dockerfile.backend . >> "$API_LOG" 2>&1; then
        echo "Completed: $(date)" >> "$API_LOG"
        echo -e "${GREEN}✓ API build successful${NC}"
        echo "Log: $API_LOG"
        return 0
    else
        echo "Completed: $(date) - FAILED" >> "$API_LOG"
        echo -e "${RED}✗ API build failed${NC}"
        echo "Log: $API_LOG"
        tail -30 "$API_LOG"
        return 1
    fi
}

# Function to build Frontend
build_frontend() {
    echo
    echo -e "${BLUE}>>> Building Frontend${NC}"
    clean_logs "$FRONTEND_LOG"
    
    echo "Started: $(date)" > "$FRONTEND_LOG"
    echo "Build command: docker buildx build --no-cache --platform linux/amd64 -t crm-frontend:latest -f docker/Dockerfile.frontend ." >> "$FRONTEND_LOG"
    echo "---" >> "$FRONTEND_LOG"
    
    if cd "$SCRIPT_DIR" && docker buildx build --no-cache --platform linux/amd64 -t crm-frontend:latest -f docker/Dockerfile.frontend . >> "$FRONTEND_LOG" 2>&1; then
        echo "Completed: $(date)" >> "$FRONTEND_LOG"
        echo -e "${GREEN}✓ Frontend build successful${NC}"
        echo "Log: $FRONTEND_LOG"
        return 0
    else
        echo "Completed: $(date) - FAILED" >> "$FRONTEND_LOG"
        echo -e "${RED}✗ Frontend build failed${NC}"
        echo "Log: $FRONTEND_LOG"
        tail -30 "$FRONTEND_LOG"
        return 1
    fi
}

# Parse arguments
BUILD_API=0
BUILD_FRONTEND=0
TAIL_LOGS=0

if [[ $# -eq 0 ]]; then
    BUILD_API=1
    BUILD_FRONTEND=1
else
    while [[ $# -gt 0 ]]; do
        case $1 in
            api)
                BUILD_API=1
                shift
                ;;
            frontend)
                BUILD_FRONTEND=1
                shift
                ;;
            all)
                BUILD_API=1
                BUILD_FRONTEND=1
                shift
                ;;
            tail)
                TAIL_LOGS=1
                shift
                ;;
            *)
                echo "Unknown option: $1"
                echo "Usage: $0 [api|frontend|all] [tail]"
                exit 1
                ;;
        esac
    done
fi

# Execute builds
BUILD_FAILED=0

if [[ $BUILD_API -eq 1 ]]; then
    build_api || BUILD_FAILED=1
fi

if [[ $BUILD_FRONTEND -eq 1 ]]; then
    build_frontend || BUILD_FAILED=1
fi

# Summary
echo
echo -e "${BLUE}================================${NC}"
echo -e "${BLUE}Build Summary${NC}"
echo -e "${BLUE}================================${NC}"

if [[ -f "$API_LOG" ]]; then
    status=$(tail -1 "$API_LOG" | grep -q FAILED && echo "FAILED" || echo "OK")
    echo -e "API Log:      $API_LOG ${status}"
fi

if [[ -f "$FRONTEND_LOG" ]]; then
    status=$(tail -1 "$FRONTEND_LOG" | grep -q FAILED && echo "FAILED" || echo "OK")
    echo -e "Frontend Log: $FRONTEND_LOG ${status}"
fi

# Tail logs if requested
if [[ $TAIL_LOGS -eq 1 ]]; then
    echo
    echo -e "${BLUE}Latest log entries:${NC}"
    [ -f "$API_LOG" ] && echo "=== API ===" && tail -20 "$API_LOG"
    [ -f "$FRONTEND_LOG" ] && echo "=== Frontend ===" && tail -20 "$FRONTEND_LOG"
fi

exit $BUILD_FAILED
