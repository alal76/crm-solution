#!/bin/bash
# CRM Data Loader Runner
# Copyright (C) 2024-2026 Abhishek Lal
# Licensed under the GNU Affero General Public License v3.0

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PYTHON_SCRIPT="$SCRIPT_DIR/load_data.py"

# Default values
API_BASE="${API_BASE:-http://192.168.0.9:5000/api}"
ADMIN_USER="${ADMIN_USER:-admin@crm.local}"
ADMIN_PASS="${ADMIN_PASS:-Admin@123}"
DATA_LEVEL="${1:-demo}"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}╔════════════════════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║              CRM Data Loader - Shell Runner                ║${NC}"
echo -e "${GREEN}╚════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Check Python version
if ! command -v python3 &> /dev/null; then
    echo -e "${RED}Error: Python 3 is required but not installed.${NC}"
    exit 1
fi

PYTHON_VERSION=$(python3 --version 2>&1)
echo -e "  Python: ${PYTHON_VERSION}"

# Help
if [[ "$1" == "-h" || "$1" == "--help" ]]; then
    echo ""
    echo "Usage: $0 [level] [options]"
    echo ""
    echo "Levels:"
    echo "  essential  - Load minimum required data (roles, lookups)"
    echo "  basic      - Load essential + sample entries"
    echo "  demo       - Load all demo data (default)"
    echo ""
    echo "Environment Variables:"
    echo "  API_BASE    - API base URL (default: http://192.168.0.9:5000/api)"
    echo "  ADMIN_USER  - Admin login (email or username) (default: admin@crm.local)"
    echo "  ADMIN_PASS  - Admin password (default: Admin@123)"
    echo ""
    echo "Examples:"
    echo "  $0 essential"
    echo "  $0 demo"
    echo "  API_BASE=http://localhost:5000/api $0 basic"
    exit 0
fi

# Validate level
case $DATA_LEVEL in
    essential|basic|demo)
        echo -e "  Level: ${DATA_LEVEL}"
        ;;
    *)
        echo -e "${RED}Error: Invalid level '${DATA_LEVEL}'. Use essential, basic, or demo.${NC}"
        exit 1
        ;;
esac

echo -e "  API: ${API_BASE}"
echo ""

# Run the Python script
python3 "$PYTHON_SCRIPT" \
    --api-base="$API_BASE" \
    --username="$ADMIN_USER" \
    --password="$ADMIN_PASS" \
    "$DATA_LEVEL"

exit_code=$?

if [[ $exit_code -eq 0 ]]; then
    echo -e "${GREEN}✓ Data loading completed successfully${NC}"
else
    echo -e "${RED}✗ Data loading failed with exit code $exit_code${NC}"
fi

exit $exit_code
