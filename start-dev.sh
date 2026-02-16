#!/bin/bash
# Start CRM API + Frontend locally (parallel)

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Color codes
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${BLUE}================================${NC}"
echo -e "${BLUE}CRM Solution - Local Dev Setup${NC}"
echo -e "${BLUE}================================${NC}"
echo ""

# Check dependencies
echo -e "${YELLOW}Checking dependencies...${NC}"

if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}✗ dotnet not found. Install .NET SDK first.${NC}"
    exit 1
fi
echo -e "${GREEN}✓ dotnet ${NC}"

if ! command -v node &> /dev/null; then
    echo -e "${RED}✗ node not found. Install Node.js first.${NC}"
    exit 1
fi
echo -e "${GREEN}✓ node $(node -v)${NC}"

if ! command -v npm &> /dev/null; then
    echo -e "${RED}✗ npm not found.${NC}"
    exit 1
fi
echo -e "${GREEN}✓ npm $(npm -v)${NC}"

echo ""
echo -e "${BLUE}Configuration:${NC}"
echo -e "  Database: ${GREEN}192.168.0.9:3306/crm_db${NC}"
echo -e "  API URL: ${GREEN}http://localhost:5000${NC}"
echo -e "  Frontend URL: ${GREEN}http://localhost:3000${NC}"
echo ""

# Start API
echo -e "${BLUE}Starting Backend API...${NC}"
bash "$SCRIPT_DIR/start-api.sh" &
API_PID=$!
echo -e "${GREEN}API started (PID: $API_PID)${NC}"

# Give API time to start
sleep 3

# Start Frontend
echo -e "${BLUE}Starting Frontend...${NC}"
bash "$SCRIPT_DIR/start-frontend.sh" &
FRONTEND_PID=$!
echo -e "${GREEN}Frontend started (PID: $FRONTEND_PID)${NC}"

echo ""
echo -e "${GREEN}================================${NC}"
echo -e "${GREEN}✓ Both services started!${NC}"
echo -e "${GREEN}================================${NC}"
echo ""
echo -e "🌐 Open in browser: ${BLUE}http://localhost:3000${NC}"
echo -e "📚 API Swagger: ${BLUE}http://localhost:5000/swagger${NC}"
echo ""
echo "Press Ctrl+C to stop all services"
echo ""

# Wait for both processes
wait $API_PID $FRONTEND_PID
