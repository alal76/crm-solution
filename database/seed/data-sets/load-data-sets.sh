#!/bin/bash
# ============================================================================
# CRM Database Seed Data Loader
# Copyright (C) 2024-2026 Abhishek Lal
# Licensed under the GNU Affero General Public License v3.0
# ============================================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Database configuration (override with environment variables)
DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_NAME="${DB_NAME:-crm_db}"
DB_USER="${DB_USER:-crm_user}"
DB_PASSWORD="${DB_PASSWORD:-}"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_header() {
    echo -e "${BLUE}"
    echo "╔════════════════════════════════════════════════════════════╗"
    echo "║            CRM Database Seed Data Loader                   ║"
    echo "╚════════════════════════════════════════════════════════════╝"
    echo -e "${NC}"
}

print_usage() {
    echo "Usage: $0 [level]"
    echo ""
    echo "Levels:"
    echo "  essential  - Load only essential data (system won't function without it)"
    echo "  basic      - Load essential + sample entries"
    echo "  demo       - Load all demo data (default)"
    echo ""
    echo "Environment Variables:"
    echo "  DB_HOST      - Database host (default: localhost)"
    echo "  DB_PORT      - Database port (default: 5432)"
    echo "  DB_NAME      - Database name (default: crm_db)"
    echo "  DB_USER      - Database user (default: crm_user)"
    echo "  DB_PASSWORD  - Database password"
    echo ""
    echo "Examples:"
    echo "  $0 essential"
    echo "  $0 demo"
    echo "  DB_HOST=192.168.0.9 DB_PASSWORD=secret $0 basic"
}

run_sql_file() {
    local file="$1"
    local description="$2"
    
    if [[ ! -f "$file" ]]; then
        echo -e "${RED}Error: File not found: $file${NC}"
        exit 1
    fi
    
    echo -e "${YELLOW}Loading: $description${NC}"
    echo "  File: $(basename "$file")"
    
    if [[ -n "$DB_PASSWORD" ]]; then
        PGPASSWORD="$DB_PASSWORD" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -f "$file" -q
    else
        psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -f "$file" -q
    fi
    
    if [[ $? -eq 0 ]]; then
        echo -e "  ${GREEN}✓ Success${NC}"
    else
        echo -e "  ${RED}✗ Failed${NC}"
        exit 1
    fi
    echo ""
}

# Main
print_header

# Parse arguments
DATA_LEVEL="${1:-demo}"

case $DATA_LEVEL in
    -h|--help)
        print_usage
        exit 0
        ;;
    essential|basic|demo)
        echo "Data Level: $DATA_LEVEL"
        ;;
    *)
        echo -e "${RED}Error: Invalid level '$DATA_LEVEL'${NC}"
        print_usage
        exit 1
        ;;
esac

echo "Database: $DB_USER@$DB_HOST:$DB_PORT/$DB_NAME"
echo ""

# Check if psql is available
if ! command -v psql &> /dev/null; then
    echo -e "${RED}Error: psql is not installed or not in PATH${NC}"
    echo "Install PostgreSQL client tools and try again."
    exit 1
fi

# Load data based on level
case $DATA_LEVEL in
    essential)
        run_sql_file "$SCRIPT_DIR/01_essential_data.sql" "Essential Data (required for system operation)"
        ;;
    basic)
        run_sql_file "$SCRIPT_DIR/01_essential_data.sql" "Essential Data (required for system operation)"
        run_sql_file "$SCRIPT_DIR/02_basic_data.sql" "Basic Sample Data"
        ;;
    demo)
        run_sql_file "$SCRIPT_DIR/01_essential_data.sql" "Essential Data (required for system operation)"
        run_sql_file "$SCRIPT_DIR/02_basic_data.sql" "Basic Sample Data"
        run_sql_file "$SCRIPT_DIR/03_demo_data.sql" "Full Demo Data"
        ;;
esac

echo -e "${GREEN}╔════════════════════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║              Data loading complete!                        ║${NC}"
echo -e "${GREEN}╚════════════════════════════════════════════════════════════╝${NC}"
echo ""
echo "Default login:"
echo "  Username: admin"
echo "  Password: Admin@123"
echo ""
