#!/bin/bash
# ============================================================================
# CRM Solution Database Deployment Script
# Version: 1.0
# Date: 2026-01-23
# Description: Deploys database schema and seed data for both prod and demo
# ============================================================================

set -e

# Configuration
DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-3306}"
DB_ROOT_USER="${DB_ROOT_USER:-root}"
DB_ROOT_PASS="${DB_ROOT_PASS:-RootPass@Dev2024}"
DB_USER="${DB_USER:-crm_user}"
DB_USER_PASS="${DB_USER_PASS:-CrmPass@Dev2024}"
PROD_DB="${PROD_DB:-crm_db}"
DEMO_DB="${DEMO_DB:-crm_demodb}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SCHEMA_DIR="$SCRIPT_DIR/schema"
SEED_DIR="$SCRIPT_DIR/seed"
MASTER_DATA_DIR="$SCRIPT_DIR/master_data"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_header() {
    echo -e "\n${BLUE}============================================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}============================================================${NC}"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

# Function to execute SQL file
execute_sql_file() {
    local database=$1
    local sql_file=$2
    local description=$3
    
    if [[ -f "$sql_file" ]]; then
        echo -n "  Executing $description... "
        if mysql -h "$DB_HOST" -P "$DB_PORT" -u "$DB_ROOT_USER" -p"$DB_ROOT_PASS" "$database" < "$sql_file" 2>/dev/null; then
            print_success "Done"
            return 0
        else
            print_error "Failed"
            return 1
        fi
    else
        print_warning "File not found: $sql_file"
        return 1
    fi
}

# Function to execute SQL command
execute_sql() {
    local database=$1
    local sql=$2
    
    mysql -h "$DB_HOST" -P "$DB_PORT" -u "$DB_ROOT_USER" -p"$DB_ROOT_PASS" "$database" -e "$sql" 2>/dev/null
}

# Function to create database if not exists
create_database() {
    local database=$1
    echo -n "  Creating database $database if not exists... "
    if mysql -h "$DB_HOST" -P "$DB_PORT" -u "$DB_ROOT_USER" -p"$DB_ROOT_PASS" -e "CREATE DATABASE IF NOT EXISTS \`$database\` CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;" 2>/dev/null; then
        print_success "Done"
    else
        print_error "Failed"
        return 1
    fi
}

# Function to grant permissions
grant_permissions() {
    local database=$1
    echo -n "  Granting permissions on $database... "
    if mysql -h "$DB_HOST" -P "$DB_PORT" -u "$DB_ROOT_USER" -p"$DB_ROOT_PASS" -e "GRANT ALL PRIVILEGES ON \`$database\`.* TO '$DB_USER'@'%' IDENTIFIED BY '$DB_USER_PASS'; FLUSH PRIVILEGES;" 2>/dev/null; then
        print_success "Done"
    else
        print_error "Failed"
        return 1
    fi
}

# Function to deploy schema
deploy_schema() {
    local database=$1
    echo ""
    echo "  Deploying schema to $database..."
    
    for schema_file in "$SCHEMA_DIR"/*.sql; do
        if [[ -f "$schema_file" ]]; then
            local filename=$(basename "$schema_file")
            execute_sql_file "$database" "$schema_file" "$filename"
        fi
    done
}

# Function to deploy seed data
deploy_seed_data() {
    local database=$1
    echo ""
    echo "  Deploying seed data to $database..."
    
    for seed_file in "$SEED_DIR"/*.sql; do
        if [[ -f "$seed_file" ]]; then
            local filename=$(basename "$seed_file")
            execute_sql_file "$database" "$seed_file" "$filename"
        fi
    done
}

# Function to deploy master data (ZIP codes, etc.)
deploy_master_data() {
    local database=$1
    echo ""
    echo "  Deploying master data to $database..."
    
    if [[ -d "$MASTER_DATA_DIR" ]]; then
        for data_file in "$MASTER_DATA_DIR"/*.sql; do
            if [[ -f "$data_file" ]]; then
                local filename=$(basename "$data_file")
                execute_sql_file "$database" "$data_file" "$filename"
            fi
        done
    else
        print_warning "Master data directory not found"
    fi
}

# Main deployment function
main() {
    print_header "CRM Solution Database Deployment"
    
    echo ""
    echo "Configuration:"
    echo "  Host: $DB_HOST:$DB_PORT"
    echo "  Production DB: $PROD_DB"
    echo "  Demo DB: $DEMO_DB"
    echo "  DB User: $DB_USER"
    
    # Check connection
    echo ""
    echo -n "Testing database connection... "
    if mysql -h "$DB_HOST" -P "$DB_PORT" -u "$DB_ROOT_USER" -p"$DB_ROOT_PASS" -e "SELECT 1;" >/dev/null 2>&1; then
        print_success "Connected"
    else
        print_error "Failed to connect to database"
        exit 1
    fi
    
    # Deploy to Production Database
    print_header "Deploying to Production Database: $PROD_DB"
    
    create_database "$PROD_DB"
    grant_permissions "$PROD_DB"
    deploy_schema "$PROD_DB"
    deploy_seed_data "$PROD_DB"
    deploy_master_data "$PROD_DB"
    
    # Deploy to Demo Database
    print_header "Deploying to Demo Database: $DEMO_DB"
    
    create_database "$DEMO_DB"
    grant_permissions "$DEMO_DB"
    deploy_schema "$DEMO_DB"
    deploy_seed_data "$DEMO_DB"
    deploy_master_data "$DEMO_DB"
    
    # Verify deployment
    print_header "Verifying Deployment"
    
    echo ""
    echo "  Production Database Tables:"
    local prod_count=$(mysql -h "$DB_HOST" -P "$DB_PORT" -u "$DB_ROOT_USER" -p"$DB_ROOT_PASS" -N -e "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '$PROD_DB';" 2>/dev/null)
    echo "    Total tables: $prod_count"
    
    echo ""
    echo "  Demo Database Tables:"
    local demo_count=$(mysql -h "$DB_HOST" -P "$DB_PORT" -u "$DB_ROOT_USER" -p"$DB_ROOT_PASS" -N -e "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '$DEMO_DB';" 2>/dev/null)
    echo "    Total tables: $demo_count"
    
    print_header "Deployment Complete!"
    
    echo ""
    print_success "Database deployment completed successfully"
    echo ""
}

# Parse command line arguments
case "${1:-}" in
    --schema-only)
        print_header "Deploying Schema Only"
        deploy_schema "$PROD_DB"
        deploy_schema "$DEMO_DB"
        ;;
    --seed-only)
        print_header "Deploying Seed Data Only"
        deploy_seed_data "$PROD_DB"
        deploy_seed_data "$DEMO_DB"
        ;;
    --master-data-only)
        print_header "Deploying Master Data Only"
        deploy_master_data "$PROD_DB"
        deploy_master_data "$DEMO_DB"
        ;;
    --prod-only)
        print_header "Deploying to Production Only"
        create_database "$PROD_DB"
        deploy_schema "$PROD_DB"
        deploy_seed_data "$PROD_DB"
        deploy_master_data "$PROD_DB"
        ;;
    --demo-only)
        print_header "Deploying to Demo Only"
        create_database "$DEMO_DB"
        deploy_schema "$DEMO_DB"
        deploy_seed_data "$DEMO_DB"
        deploy_master_data "$DEMO_DB"
        ;;
    --help|-h)
        echo "Usage: $0 [option]"
        echo ""
        echo "Options:"
        echo "  (no option)      Deploy everything to both databases"
        echo "  --schema-only    Deploy only schema files"
        echo "  --seed-only      Deploy only seed data"
        echo "  --master-data-only Deploy only master data (ZIP codes, etc.)"
        echo "  --prod-only      Deploy to production database only"
        echo "  --demo-only      Deploy to demo database only"
        echo "  --help, -h       Show this help message"
        echo ""
        echo "Environment variables:"
        echo "  DB_HOST          Database host (default: localhost)"
        echo "  DB_PORT          Database port (default: 3306)"
        echo "  DB_ROOT_USER     Root user (default: root)"
        echo "  DB_ROOT_PASS     Root password"
        echo "  DB_USER          Application user (default: crm_user)"
        echo "  DB_USER_PASS     Application user password"
        echo "  PROD_DB          Production database name (default: crm_db)"
        echo "  DEMO_DB          Demo database name (default: crm_demodb)"
        ;;
    *)
        main
        ;;
esac
