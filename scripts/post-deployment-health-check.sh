#!/bin/bash
# =============================================================================
# CRM Solution - Post-Deployment Health Check Module
# =============================================================================
# This script performs comprehensive health checks after deployment to verify:
# 1. Schema completion - all expected tables exist
# 2. Connectivity - database, Redis, and network accessibility
# 3. API Endpoints - all required endpoints are responding
# 4. Pluggable Providers - all configured providers are healthy
# 5. Initial Configuration - admin user, seed data, etc.
#
# Usage:
#   ./post-deployment-health-check.sh [OPTIONS]
#
# Options:
#   -h, --host HOST         Target host (default: localhost)
#   -p, --port PORT         API port (default: 5000)
#   -s, --ssh USER@HOST     Run checks via SSH on remote host
#   --db-host HOST          Database host (default: crm-mariadb)
#   --db-user USER          Database user (default: crm_user)
#   --db-pass PASS          Database password
#   --db-name NAME          Database name (default: crm_db)
#   --skip-schema           Skip schema validation
#   --skip-providers        Skip provider health checks
#   --json                  Output results as JSON
#   --verbose               Verbose output
#   --help                  Show this help
# =============================================================================

set -e

# =============================================================================
# Configuration Defaults
# =============================================================================
API_HOST="localhost"
API_PORT="5000"
SSH_TARGET=""
DB_HOST="crm-mariadb"
DB_USER="crm_user"
DB_PASS="CrmPass@Dev2024"
DB_NAME="crm_db"
REDIS_HOST="crm-redis"
REDIS_PORT="6379"
VERBOSE=false
JSON_OUTPUT=false
SKIP_SCHEMA=false
SKIP_PROVIDERS=false

# Colors for terminal output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Result tracking
TOTAL_CHECKS=0
PASSED_CHECKS=0
FAILED_CHECKS=0
WARNINGS=0
declare -a RESULTS=()
declare -a FAILED_ITEMS=()

# =============================================================================
# Helper Functions
# =============================================================================

print_header() {
    if [[ "$JSON_OUTPUT" = false ]]; then
        echo ""
        echo -e "${BLUE}═══════════════════════════════════════════════════════════════════${NC}"
        echo -e "${BLUE}  $1${NC}"
        echo -e "${BLUE}═══════════════════════════════════════════════════════════════════${NC}"
    fi
}

print_section() {
    if [[ "$JSON_OUTPUT" = false ]]; then
        echo ""
        echo -e "${CYAN}▶ $1${NC}"
        echo -e "${CYAN}───────────────────────────────────────────────────────────────────${NC}"
    fi
}

log_check() {
    local status="$1"
    local message="$2"
    local details="$3"
    TOTAL_CHECKS=$((TOTAL_CHECKS + 1))
    
    if [[ "$status" = "PASS" ]]; then
        PASSED_CHECKS=$((PASSED_CHECKS + 1))
        if [[ "$JSON_OUTPUT" = false ]]; then
            echo -e "  ${GREEN}✓${NC} $message"
            [[ "$VERBOSE" = true ]] && [[ -n "$details" ]] && echo -e "    ${CYAN}$details${NC}"
        fi
        RESULTS+=("{\"check\":\"$message\",\"status\":\"pass\",\"details\":\"$details\"}")
    elif [[ "$status" = "FAIL" ]]; then
        FAILED_CHECKS=$((FAILED_CHECKS + 1))
        if [[ "$JSON_OUTPUT" = false ]]; then
            echo -e "  ${RED}✗${NC} $message"
            [ -n "$details" ] && echo -e "    ${RED}$details${NC}"
        fi
        RESULTS+=("{\"check\":\"$message\",\"status\":\"fail\",\"details\":\"$details\"}")
        FAILED_ITEMS+=("$message: $details")
    elif [[ "$status" = "WARN" ]]; then
        WARNINGS=$((WARNINGS + 1))
        if [[ "$JSON_OUTPUT" = false ]]; then
            echo -e "  ${YELLOW}⚠${NC} $message"
            [ -n "$details" ] && echo -e "    ${YELLOW}$details${NC}"
        fi
        RESULTS+=("{\"check\":\"$message\",\"status\":\"warning\",\"details\":\"$details\"}")
    elif [[ "$status" = "SKIP" ]]; then
        if [[ "$JSON_OUTPUT" = false ]]; then
            echo -e "  ${YELLOW}○${NC} $message (skipped)"
        fi
        RESULTS+=("{\"check\":\"$message\",\"status\":\"skipped\",\"details\":\"$details\"}")
    fi
}

run_cmd() {
    if [[ -n "$SSH_TARGET" ]]; then
        ssh "$SSH_TARGET" "$1"
    else
        eval "$1"
    fi
}

run_docker_cmd() {
    local container="$1"
    local cmd="$2"
    run_cmd "docker exec $container $cmd"
}

# =============================================================================
# Parse Arguments
# =============================================================================
parse_args() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            -h|--host)
                API_HOST="$2"
                shift 2
                ;;
            -p|--port)
                API_PORT="$2"
                shift 2
                ;;
            -s|--ssh)
                SSH_TARGET="$2"
                shift 2
                ;;
            --db-host)
                DB_HOST="$2"
                shift 2
                ;;
            --db-user)
                DB_USER="$2"
                shift 2
                ;;
            --db-pass)
                DB_PASS="$2"
                shift 2
                ;;
            --db-name)
                DB_NAME="$2"
                shift 2
                ;;
            --skip-schema)
                SKIP_SCHEMA=true
                shift
                ;;
            --skip-providers)
                SKIP_PROVIDERS=true
                shift
                ;;
            --json)
                JSON_OUTPUT=true
                shift
                ;;
            --verbose)
                VERBOSE=true
                shift
                ;;
            --help)
                echo "Usage: $0 [OPTIONS]"
                echo ""
                echo "Options:"
                echo "  -h, --host HOST         Target host (default: localhost)"
                echo "  -p, --port PORT         API port (default: 5000)"
                echo "  -s, --ssh USER@HOST     Run checks via SSH on remote host"
                echo "  --db-host HOST          Database host (default: crm-mariadb)"
                echo "  --db-user USER          Database user (default: crm_user)"
                echo "  --db-pass PASS          Database password"
                echo "  --db-name NAME          Database name (default: crm_db)"
                echo "  --skip-schema           Skip schema validation"
                echo "  --skip-providers        Skip provider health checks"
                echo "  --json                  Output results as JSON"
                echo "  --verbose               Verbose output"
                echo "  --help                  Show this help"
                exit 0
                ;;
            *)
                echo "Unknown option: $1"
                exit 1
                ;;
        esac
    done
}

# =============================================================================
# Check: Docker Container Status
# =============================================================================
check_containers() {
    print_section "Docker Container Status"
    
    local containers=("crm-api" "crm-mariadb" "crm-redis" "crm-frontend")
    
    for container in "${containers[@]}"; do
        local status
        status=$(run_cmd "docker inspect -f '{{.State.Status}}' $container 2>/dev/null" || echo "not_found")
        
        if [[ "$status" = "running" ]]; then
            local health
            health=$(run_cmd "docker inspect -f '{{.State.Health.Status}}' $container 2>/dev/null" || echo "none")
            if [[ "$health" = "healthy" ]] || [[ "$health" = "none" ]]; then
                log_check "PASS" "Container $container" "Status: $status, Health: $health"
            else
                log_check "WARN" "Container $container" "Status: $status, Health: $health"
            fi
        elif [[ "$status" = "not_found" ]]; then
            log_check "FAIL" "Container $container" "Container not found"
        else
            log_check "FAIL" "Container $container" "Status: $status"
        fi
    done
}

# =============================================================================
# Check: Database Connectivity
# =============================================================================
check_database_connectivity() {
    print_section "Database Connectivity"
    
    # Check if we can connect to the database
    local result
    result=$(run_cmd "docker exec crm-mariadb mariadb -u $DB_USER -p'$DB_PASS' $DB_NAME -e 'SELECT 1' 2>&1" || echo "FAILED")
    
    if [[ "$result" == *"1"* ]]; then
        log_check "PASS" "Database connection" "Successfully connected to $DB_NAME"
    else
        log_check "FAIL" "Database connection" "Failed to connect: $result"
        return 1
    fi
    
    # Check database version
    local version
    version=$(run_cmd "docker exec crm-mariadb mariadb -u $DB_USER -p'$DB_PASS' -e 'SELECT VERSION()' -s -N 2>/dev/null" || echo "unknown")
    log_check "PASS" "Database version" "$version"
    
    return 0
}

# =============================================================================
# Check: Redis Connectivity
# =============================================================================
check_redis_connectivity() {
    print_section "Redis Connectivity"
    
    local result
    result=$(run_cmd "docker exec crm-redis redis-cli PING 2>&1" || echo "FAILED")
    
    if [[ "$result" == *"PONG"* ]]; then
        log_check "PASS" "Redis connection" "Redis is responding"
    else
        log_check "FAIL" "Redis connection" "Failed: $result"
        return 1
    fi
    
    # Check Redis info
    local keys
    keys=$(run_cmd "docker exec crm-redis redis-cli DBSIZE 2>/dev/null" || echo "unknown")
    log_check "PASS" "Redis status" "$keys"
    
    return 0
}

# =============================================================================
# Check: Schema Completion
# =============================================================================
check_schema_completion() {
    if [[ "$SKIP_SCHEMA" = true ]]; then
        print_section "Schema Validation (SKIPPED)"
        log_check "SKIP" "Schema validation" "Skipped by user request"
        return 0
    fi
    
    print_section "Schema Validation"
    
    # Expected core tables (EF Core table names)
    # These are the minimum required tables for a functional CRM deployment
    local expected_tables=(
        # Core Authentication & Users
        "Users"
        "UserGroups"
        "UserGroupMembers"
        "UserProfiles"
        "Departments"
        
        # CRM Core Entities
        "Accounts"
        "Contacts"
        "AccountContacts"
        "Leads"
        "Opportunities"
        "OpportunityProducts"
        "Products"
        
        # Sales
        "Quotes"
        "QuoteLineItems"
        "Orders"
        "OrderLineItems"
        "Invoices"
        "InvoiceLineItems"
        
        # Marketing
        "MarketingCampaigns"
        "EmailTemplates"
        "EmailSequences"
        
        # Service Desk
        "ServiceRequests"
        "ServiceRequestCategories"
        "KnowledgeArticles"
        
        # Workflow
        "WorkflowDefinitions"
        "WorkflowNodes"
        "SLAPolicies"
        
        # System
        "SystemSettings"
        "Addresses"
        "PhoneNumbers"
        "EmailAddresses"
        "Interactions"
        "Tasks"
        "Notes"
        "ActivityLogs"
        
        # AI
        "AIAgents"
        
        # Commissions
        "CommissionPlans"
        
        # EF Core
        "__EFMigrationsHistory"
    )
    
    # Get all tables from database
    local db_tables
    db_tables=$(run_cmd "docker exec crm-mariadb mariadb -u $DB_USER -p'$DB_PASS' $DB_NAME -e 'SHOW TABLES' -s -N 2>/dev/null" || echo "")
    
    if [[ -z "$db_tables" ]]; then
        log_check "FAIL" "Schema tables" "Could not retrieve table list"
        return 1
    fi
    
    # Count tables
    local table_count
    table_count=$(echo "$db_tables" | wc -l | tr -d ' ')
    log_check "PASS" "Total tables in database" "$table_count tables found"
    
    # Check for expected tables
    local missing_tables=()
    for table in "${expected_tables[@]}"; do
        if echo "$db_tables" | grep -q "^${table}$"; then
            [ "$VERBOSE" = true ] && log_check "PASS" "Table: $table" "exists"
        else
            missing_tables+=("$table")
        fi
    done
    
    if [[ ${#missing_tables[@]} -eq 0 ]]; then
        log_check "PASS" "Core tables present" "All ${#expected_tables[@]} required tables exist"
    else
        log_check "FAIL" "Missing tables" "${missing_tables[*]}"
    fi
    
    # Check migration history
    local migrations
    migrations=$(run_cmd "docker exec crm-mariadb mariadb -u $DB_USER -p'$DB_PASS' $DB_NAME -e 'SELECT COUNT(*) FROM __EFMigrationsHistory' -s -N 2>/dev/null" || echo "0")
    
    if [[ "$migrations" -gt 0 ]]; then
        log_check "PASS" "EF Migrations applied" "$migrations migration(s) recorded"
    else
        log_check "WARN" "EF Migrations" "No migrations recorded in __EFMigrationsHistory"
    fi
    
    return 0
}

# =============================================================================
# Check: API Health Endpoints
# =============================================================================
check_api_endpoints() {
    print_section "API Health Endpoints"
    
    local base_url="http://${API_HOST}:${API_PORT}"
    
    # Use curl directly for SSH targets
    if [[ -n "$SSH_TARGET" ]]; then
        base_url="http://localhost:${API_PORT}"
    fi
    
    # Core health endpoints
    local endpoints=(
        "/health:Basic Health"
        "/health/ready:Readiness Probe"
        "/health/live:Liveness Probe"
        "/api/health/providers:Provider Health"
    )
    
    for endpoint_entry in "${endpoints[@]}"; do
        local endpoint="${endpoint_entry%%:*}"
        local name="${endpoint_entry##*:}"
        local url="${base_url}${endpoint}"
        
        local response
        local http_code
        
        if [[ -n "$SSH_TARGET" ]]; then
            http_code=$(ssh "$SSH_TARGET" "curl -s -o /dev/null -w '%{http_code}' '$url' 2>/dev/null" || echo "000")
        else
            http_code=$(curl -s -o /dev/null -w '%{http_code}' "$url" 2>/dev/null || echo "000")
        fi
        
        if [[ "$http_code" = "200" ]]; then
            log_check "PASS" "$name" "HTTP $http_code"
        elif [[ "$http_code" = "503" ]]; then
            log_check "WARN" "$name" "HTTP $http_code (degraded)"
        elif [[ "$http_code" = "000" ]]; then
            log_check "FAIL" "$name" "Connection failed"
        else
            log_check "FAIL" "$name" "HTTP $http_code"
        fi
    done
    
    return 0
}

# =============================================================================
# Check: Required API Endpoints
# =============================================================================
check_api_crud_endpoints() {
    print_section "API CRUD Endpoints"
    
    local base_url="http://${API_HOST}:${API_PORT}"
    
    if [[ -n "$SSH_TARGET" ]]; then
        base_url="http://localhost:${API_PORT}"
    fi
    
    # Endpoints that should respond (may require auth, but should not 404)
    local crud_endpoints=(
        "/api/accounts:Accounts API"
        "/api/contacts:Contacts API"
        "/api/leads:Leads API"
        "/api/opportunities:Opportunities API"
        "/api/products:Products API"
        "/api/campaigns:Campaigns API"
        "/api/servicerequests:Service Requests API"
        "/api/users:Users API"
        "/api/usergroups:User Groups API"
        "/api/settings:Settings API"
        "/api/dashboard:Dashboard API"
    )
    
    for endpoint_entry in "${crud_endpoints[@]}"; do
        local endpoint="${endpoint_entry%%:*}"
        local name="${endpoint_entry##*:}"
        local url="${base_url}${endpoint}"
        
        local http_code
        
        if [[ -n "$SSH_TARGET" ]]; then
            http_code=$(ssh "$SSH_TARGET" "curl -s -o /dev/null -w '%{http_code}' '$url' 2>/dev/null" || echo "000")
        else
            http_code=$(curl -s -o /dev/null -w '%{http_code}' "$url" 2>/dev/null || echo "000")
        fi
        
        # 401 Unauthorized is acceptable - means endpoint exists but requires auth
        if [[ "$http_code" = "200" ]] || [[ "$http_code" = "401" ]]; then
            log_check "PASS" "$name" "HTTP $http_code"
        elif [[ "$http_code" = "404" ]]; then
            log_check "FAIL" "$name" "HTTP $http_code (not found)"
        elif [[ "$http_code" = "000" ]]; then
            log_check "FAIL" "$name" "Connection failed"
        else
            log_check "WARN" "$name" "HTTP $http_code"
        fi
    done
    
    return 0
}

# =============================================================================
# Check: Provider Health
# =============================================================================
check_provider_health() {
    if [[ "$SKIP_PROVIDERS" = true ]]; then
        print_section "Provider Health (SKIPPED)"
        log_check "SKIP" "Provider health" "Skipped by user request"
        return 0
    fi
    
    print_section "Provider Health"
    
    local base_url="http://${API_HOST}:${API_PORT}"
    
    if [[ -n "$SSH_TARGET" ]]; then
        base_url="http://localhost:${API_PORT}"
    fi
    
    local response
    if [[ -n "$SSH_TARGET" ]]; then
        response=$(ssh "$SSH_TARGET" "curl -s '${base_url}/api/health/providers' 2>/dev/null" || echo "{}")
    else
        response=$(curl -s "${base_url}/api/health/providers" 2>/dev/null || echo "{}")
    fi
    
    # Parse provider health using Python (cross-platform JSON parsing)
    local providers_status
    providers_status=$(echo "$response" | python3 -c "
import sys
import json
try:
    data = json.load(sys.stdin)
    if 'providers' in data:
        for name, info in data['providers'].items():
            status = 'healthy' if info.get('isHealthy', False) else 'unhealthy'
            provider = info.get('providerName', 'Unknown')
            print(f'{name}:{status}:{provider}')
    else:
        print('ERROR:parse:No providers data')
except Exception as e:
    print(f'ERROR:parse:{str(e)[:50]}')
" 2>/dev/null || echo "ERROR:parse:Python parsing failed")
    
    while IFS=':' read -r name status provider; do
        if [[ "$name" = "ERROR" ]]; then
            log_check "WARN" "Provider health parsing" "$provider"
            continue
        fi
        
        if [[ "$status" = "healthy" ]]; then
            log_check "PASS" "Provider: $name" "$provider"
        else
            log_check "FAIL" "Provider: $name" "$provider - $status"
        fi
    done <<< "$providers_status"
    
    # Check overall health
    local overall
    overall=$(echo "$response" | python3 -c "
import sys, json
try:
    data = json.load(sys.stdin)
    print('healthy' if data.get('overallHealthy', False) else 'unhealthy')
except:
    print('unknown')
" 2>/dev/null || echo "unknown")
    
    if [[ "$overall" = "healthy" ]]; then
        log_check "PASS" "Overall provider health" "All providers healthy"
    elif [[ "$overall" = "unhealthy" ]]; then
        log_check "WARN" "Overall provider health" "Some providers unhealthy"
    else
        log_check "WARN" "Overall provider health" "Unable to determine"
    fi
    
    return 0
}

# =============================================================================
# Check: Initial Configuration / Seed Data
# =============================================================================
check_seed_data() {
    print_section "Initial Configuration & Seed Data"
    
    # Check for admin user
    local admin_count
    admin_count=$(run_cmd "docker exec crm-mariadb mariadb -u $DB_USER -p'$DB_PASS' $DB_NAME -e \"SELECT COUNT(*) FROM Users WHERE Role = 0 AND IsDeleted = 0\" -s -N 2>/dev/null" || echo "0")
    
    if [[ "$admin_count" -gt 0 ]]; then
        log_check "PASS" "Admin user exists" "$admin_count admin user(s) found"
    else
        log_check "FAIL" "Admin user" "No admin user found - seeding may be required"
    fi
    
    # Check for SysAdmin group
    local sysadmin_count
    sysadmin_count=$(run_cmd "docker exec crm-mariadb mariadb -u $DB_USER -p'$DB_PASS' $DB_NAME -e \"SELECT COUNT(*) FROM UserGroups WHERE IsSystemAdmin = 1 AND IsDeleted = 0\" -s -N 2>/dev/null" || echo "0")
    
    if [[ "$sysadmin_count" -gt 0 ]]; then
        log_check "PASS" "SysAdmin group exists" "$sysadmin_count SysAdmin group(s) found"
    else
        log_check "FAIL" "SysAdmin group" "No SysAdmin group found - seeding may be required"
    fi
    
    # Check for system settings
    local settings_count
    settings_count=$(run_cmd "docker exec crm-mariadb mariadb -u $DB_USER -p'$DB_PASS' $DB_NAME -e \"SELECT COUNT(*) FROM SystemSettings WHERE IsDeleted = 0\" -s -N 2>/dev/null" || echo "0")
    
    if [[ "$settings_count" -gt 0 ]]; then
        log_check "PASS" "System settings" "$settings_count setting(s) configured"
    else
        log_check "WARN" "System settings" "No system settings found"
    fi
    
    # Check for service request categories
    local category_count
    category_count=$(run_cmd "docker exec crm-mariadb mariadb -u $DB_USER -p'$DB_PASS' $DB_NAME -e \"SELECT COUNT(*) FROM ServiceRequestCategories WHERE IsDeleted = 0\" -s -N 2>/dev/null" || echo "0")
    
    if [[ "$category_count" -gt 0 ]]; then
        log_check "PASS" "Service request categories" "$category_count category(ies) defined"
    else
        log_check "WARN" "Service request categories" "No categories found"
    fi
    
    return 0
}

# =============================================================================
# Check: Authentication Flow
# =============================================================================
check_authentication() {
    print_section "Authentication Flow"
    
    local base_url="http://${API_HOST}:${API_PORT}"
    
    if [[ -n "$SSH_TARGET" ]]; then
        base_url="http://localhost:${API_PORT}"
    fi
    
    # Test login endpoint availability
    local login_url="${base_url}/api/auth/login"
    local response
    
    if [[ -n "$SSH_TARGET" ]]; then
        response=$(ssh "$SSH_TARGET" "curl -s -X POST '$login_url' -H 'Content-Type: application/json' -d '{\"email\":\"test@test.com\",\"password\":\"test\"}' 2>/dev/null" || echo "{}")
    else
        response=$(curl -s -X POST "$login_url" -H 'Content-Type: application/json' -d '{"email":"test@test.com","password":"test"}' 2>/dev/null || echo "{}")
    fi
    
    # Check if we get a proper response (not 500 error)
    if echo "$response" | grep -qi "invalid\|password\|email"; then
        log_check "PASS" "Login endpoint" "Responds correctly to auth requests"
    elif echo "$response" | grep -qi "error\|exception\|500"; then
        log_check "FAIL" "Login endpoint" "Returns server error"
    else
        log_check "WARN" "Login endpoint" "Unexpected response format"
    fi
    
    # Test with actual admin credentials (optional)
    if [[ -n "$SSH_TARGET" ]]; then
        response=$(ssh "$SSH_TARGET" "curl -s -X POST '$login_url' -H 'Content-Type: application/json' -d '{\"email\":\"admin@crm.local\",\"password\":\"Admin@123\"}' 2>/dev/null" || echo "{}")
    else
        response=$(curl -s -X POST "$login_url" -H 'Content-Type: application/json' -d '{"email":"admin@crm.local","password":"Admin@123"}' 2>/dev/null || echo "{}")
    fi
    
    if echo "$response" | grep -qi "accessToken"; then
        log_check "PASS" "Admin login" "Successfully authenticated with default credentials"
    else
        log_check "WARN" "Admin login" "Default credentials not working (may have been changed)"
    fi
    
    return 0
}

# =============================================================================
# Check: Network Connectivity Between Containers
# =============================================================================
check_network_connectivity() {
    print_section "Network Connectivity"
    
    # Check if API can reach database
    local db_ping
    db_ping=$(run_cmd "docker exec crm-api ping -c 1 crm-mariadb 2>&1" || echo "FAILED")
    
    if echo "$db_ping" | grep -q "1 packets transmitted"; then
        log_check "PASS" "API → Database network" "Ping successful"
    else
        log_check "WARN" "API → Database network" "Ping utility may not be installed"
    fi
    
    # Check if API can reach Redis
    local redis_ping
    redis_ping=$(run_cmd "docker exec crm-api ping -c 1 crm-redis 2>&1" || echo "FAILED")
    
    if echo "$redis_ping" | grep -q "1 packets transmitted"; then
        log_check "PASS" "API → Redis network" "Ping successful"
    else
        log_check "WARN" "API → Redis network" "Ping utility may not be installed"
    fi
    
    # Check Docker network
    local network_info
    network_info=$(run_cmd "docker network ls | grep -E 'crm.*network'" || echo "")
    
    if [[ -n "$network_info" ]]; then
        log_check "PASS" "Docker network" "CRM network exists"
    else
        log_check "WARN" "Docker network" "CRM network not found by name pattern"
    fi
    
    return 0
}

# =============================================================================
# Check: Swagger / API Documentation
# =============================================================================
check_api_documentation() {
    print_section "API Documentation"
    
    local base_url="http://${API_HOST}:${API_PORT}"
    
    if [[ -n "$SSH_TARGET" ]]; then
        base_url="http://localhost:${API_PORT}"
    fi
    
    local swagger_url="${base_url}/swagger/index.html"
    local http_code
    
    if [[ -n "$SSH_TARGET" ]]; then
        http_code=$(ssh "$SSH_TARGET" "curl -s -o /dev/null -w '%{http_code}' '$swagger_url' 2>/dev/null" || echo "000")
    else
        http_code=$(curl -s -o /dev/null -w '%{http_code}' "$swagger_url" 2>/dev/null || echo "000")
    fi
    
    if [[ "$http_code" = "200" ]]; then
        log_check "PASS" "Swagger UI" "Available at /swagger"
    else
        log_check "WARN" "Swagger UI" "HTTP $http_code (may be disabled in production)"
    fi
    
    # Check OpenAPI spec
    local openapi_url="${base_url}/swagger/v1/swagger.json"
    
    if [[ -n "$SSH_TARGET" ]]; then
        http_code=$(ssh "$SSH_TARGET" "curl -s -o /dev/null -w '%{http_code}' '$openapi_url' 2>/dev/null" || echo "000")
    else
        http_code=$(curl -s -o /dev/null -w '%{http_code}' "$openapi_url" 2>/dev/null || echo "000")
    fi
    
    if [[ "$http_code" = "200" ]]; then
        log_check "PASS" "OpenAPI spec" "Available at /swagger/v1/swagger.json"
    else
        log_check "WARN" "OpenAPI spec" "HTTP $http_code"
    fi
    
    return 0
}

# =============================================================================
# Generate Summary Report
# =============================================================================
generate_report() {
    if [[ "$JSON_OUTPUT" = true ]]; then
        echo "{"
        echo "  \"timestamp\": \"$(date -u +"%Y-%m-%dT%H:%M:%SZ")\","
        echo "  \"target\": \"${SSH_TARGET:-$API_HOST:$API_PORT}\","
        echo "  \"summary\": {"
        echo "    \"total\": $TOTAL_CHECKS,"
        echo "    \"passed\": $PASSED_CHECKS,"
        echo "    \"failed\": $FAILED_CHECKS,"
        echo "    \"warnings\": $WARNINGS"
        echo "  },"
        echo "  \"overallHealthy\": $([ $FAILED_CHECKS -eq 0 ] && echo "true" || echo "false"),"
        echo "  \"results\": ["
        local first=true
        for result in "${RESULTS[@]}"; do
            if [[ "$first" = true ]]; then
                first=false
            else
                echo ","
            fi
            echo -n "    $result"
        done
        echo ""
        echo "  ]"
        echo "}"
    else
        print_header "Health Check Summary"
        echo ""
        echo -e "  ${CYAN}Target:${NC} ${SSH_TARGET:-$API_HOST:$API_PORT}"
        echo -e "  ${CYAN}Time:${NC} $(date)"
        echo ""
        echo -e "  ${GREEN}Passed:${NC}   $PASSED_CHECKS"
        echo -e "  ${RED}Failed:${NC}   $FAILED_CHECKS"
        echo -e "  ${YELLOW}Warnings:${NC} $WARNINGS"
        echo -e "  ${BLUE}Total:${NC}    $TOTAL_CHECKS"
        echo ""
        
        if [[ $FAILED_CHECKS -eq 0 ]]; then
            echo -e "  ${GREEN}═══════════════════════════════════════════════════════════════════${NC}"
            echo -e "  ${GREEN}  ✓ ALL HEALTH CHECKS PASSED - Deployment is healthy!${NC}"
            echo -e "  ${GREEN}═══════════════════════════════════════════════════════════════════${NC}"
        else
            echo -e "  ${RED}═══════════════════════════════════════════════════════════════════${NC}"
            echo -e "  ${RED}  ✗ DEPLOYMENT HAS ISSUES - $FAILED_CHECKS check(s) failed${NC}"
            echo -e "  ${RED}═══════════════════════════════════════════════════════════════════${NC}"
            echo ""
            echo -e "  ${RED}Failed Checks:${NC}"
            for item in "${FAILED_ITEMS[@]}"; do
                echo -e "    ${RED}•${NC} $item"
            done
        fi
        echo ""
    fi
}

# =============================================================================
# Main Execution
# =============================================================================
main() {
    parse_args "$@"
    
    if [[ "$JSON_OUTPUT" = false ]]; then
        print_header "CRM Solution - Post-Deployment Health Check"
        echo ""
        echo -e "  ${CYAN}Target:${NC} ${SSH_TARGET:-$API_HOST:$API_PORT}"
        echo -e "  ${CYAN}Database:${NC} $DB_HOST / $DB_NAME"
        echo -e "  ${CYAN}Started:${NC} $(date)"
    fi
    
    # Run all checks
    check_containers
    check_database_connectivity
    check_redis_connectivity
    check_network_connectivity
    check_schema_completion
    check_api_endpoints
    check_api_crud_endpoints
    check_provider_health
    check_seed_data
    check_authentication
    check_api_documentation
    
    # Generate final report
    generate_report
    
    # Exit with appropriate code
    if [[ $FAILED_CHECKS -eq 0 ]]; then
        exit 0
    else
        exit 1
    fi
}

# Run main function
main "$@"
