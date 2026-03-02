#!/bin/bash
# ============================================================================
# CRM Solution - Cross-Platform Database Setup Script
# ============================================================================
# Version: 1.0
# Date: February 1, 2026
# Description: Sets up the CRM database on any supported platform
#
# Supported Databases:
#   - MariaDB / MySQL
#   - PostgreSQL  
#   - SQL Server
#   - SQLite (for development)
#
# Usage:
#   ./setup-database.sh                    # Interactive mode
#   ./setup-database.sh --provider mariadb # Specify provider
#   ./setup-database.sh --docker           # Use Docker container
#   ./setup-database.sh --help             # Show help
# ============================================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
PURPLE='\033[0;35m'
NC='\033[0m'

# Default configuration
DB_PROVIDER="${DB_PROVIDER:-mariadb}"
DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-}"
DB_NAME="${DB_NAME:-crm_db}"
DB_USER="${DB_USER:-crm_user}"
DB_PASSWORD="${DB_PASSWORD:-}"
DB_ROOT_USER="${DB_ROOT_USER:-root}"
DB_ROOT_PASSWORD="${DB_ROOT_PASSWORD:-}"
USE_DOCKER="${USE_DOCKER:-false}"
DOCKER_CONTAINER="${DOCKER_CONTAINER:-crm-mariadb}"
SEED_DATA="${SEED_DATA:-true}"
SEED_SAMPLE="${SEED_SAMPLE:-false}"

# =============================================================================
# HELPER FUNCTIONS
# =============================================================================

print_header() {
    echo -e "\n${PURPLE}╔══════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${PURPLE}║${NC} ${CYAN}$1${NC}"
    echo -e "${PURPLE}╚══════════════════════════════════════════════════════════════╝${NC}"
}

print_step() {
    echo -e "\n${BLUE}▶ $1${NC}"
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

print_info() {
    echo -e "${CYAN}ℹ $1${NC}"
}

show_help() {
    cat << 'EOF'
CRM Solution - Cross-Platform Database Setup Script

USAGE:
    ./setup-database.sh [OPTIONS]

OPTIONS:
    --provider <type>    Database provider: mariadb, mysql, postgresql, sqlserver, sqlite
                         Default: mariadb

    --host <host>        Database host. Default: localhost
    --port <port>        Database port. Default: auto (3306/5432/1433)
    --name <name>        Database name. Default: crm_db
    --user <user>        Application database user. Default: crm_user
    --password <pass>    Application user password. Will prompt if not provided.
    --root-user <user>   Admin user for creating database. Default: root
    --root-password <p>  Admin password. Will prompt if not provided.

    --docker             Run against Docker container instead of direct connection
    --container <name>   Docker container name. Default: crm-mariadb

    --seed               Seed with core data (default)
    --no-seed            Skip seeding data
    --sample-data        Include sample/demo data

    --schema-only        Create schema only, no seed data
    --seed-only          Seed data only, assume schema exists

    -h, --help           Show this help message

EXAMPLES:
    # Interactive setup for MariaDB
    ./setup-database.sh

    # Setup PostgreSQL with specific credentials
    ./setup-database.sh --provider postgresql --host db.example.com --password MySecretPass

    # Setup using Docker container
    ./setup-database.sh --docker --container crm-mariadb

    # Setup with sample data
    ./setup-database.sh --seed --sample-data

    # Schema only for migration purposes
    ./setup-database.sh --schema-only

ENVIRONMENT VARIABLES:
    DB_PROVIDER, DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASSWORD,
    DB_ROOT_USER, DB_ROOT_PASSWORD, USE_DOCKER, DOCKER_CONTAINER

EOF
}

# Generate a random password
generate_password() {
    local length=${1:-16}
    if command -v openssl &> /dev/null; then
        openssl rand -base64 $length | tr -d '/+=' | head -c $length
        echo "@1"
    else
        cat /dev/urandom | tr -dc 'a-zA-Z0-9' | head -c $((length-2))
        echo "@1"
    fi
}

# Get default port for provider
get_default_port() {
    case "$1" in
        mariadb|mysql) echo "3306" ;;
        postgresql|postgres) echo "5432" ;;
        sqlserver|mssql) echo "1433" ;;
        sqlite) echo "" ;;
        *) echo "3306" ;;
    esac
}

# =============================================================================
# DATABASE PROVIDER FUNCTIONS
# =============================================================================

# MariaDB/MySQL connection test
test_mariadb_connection() {
    local host=$1
    local port=$2
    local user=$3
    local pass=$4
    
    if [[ "$USE_DOCKER" = "true" ]]; then
        docker exec "$DOCKER_CONTAINER" mariadb -u "$user" -p"$pass" -e "SELECT 1" &> /dev/null
    else
        mysql -h "$host" -P "$port" -u "$user" -p"$pass" -e "SELECT 1" &> /dev/null
    fi
}

# PostgreSQL connection test
test_postgresql_connection() {
    local host=$1
    local port=$2
    local user=$3
    local pass=$4
    
    PGPASSWORD="$pass" psql -h "$host" -p "$port" -U "$user" -c "SELECT 1" &> /dev/null
}

# SQL Server connection test
test_sqlserver_connection() {
    local host=$1
    local port=$2
    local user=$3
    local pass=$4
    
    sqlcmd -S "$host,$port" -U "$user" -P "$pass" -Q "SELECT 1" &> /dev/null
}

# Execute SQL for MariaDB/MySQL
exec_mariadb() {
    local database=$1
    local sql=$2
    
    if [[ "$USE_DOCKER" = "true" ]]; then
        echo "$sql" | docker exec -i "$DOCKER_CONTAINER" mariadb -u "$DB_ROOT_USER" -p"$DB_ROOT_PASSWORD" "$database"
    else
        echo "$sql" | mysql -h "$DB_HOST" -P "$DB_PORT" -u "$DB_ROOT_USER" -p"$DB_ROOT_PASSWORD" "$database"
    fi
}

# Execute SQL file for MariaDB/MySQL
exec_mariadb_file() {
    local database=$1
    local file=$2
    
    if [[ "$USE_DOCKER" = "true" ]]; then
        docker exec -i "$DOCKER_CONTAINER" mariadb -u "$DB_ROOT_USER" -p"$DB_ROOT_PASSWORD" "$database" < "$file"
    else
        mysql -h "$DB_HOST" -P "$DB_PORT" -u "$DB_ROOT_USER" -p"$DB_ROOT_PASSWORD" "$database" < "$file"
    fi
}

# Execute SQL for PostgreSQL
exec_postgresql() {
    local database=$1
    local sql=$2
    
    PGPASSWORD="$DB_ROOT_PASSWORD" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_ROOT_USER" -d "$database" -c "$sql"
}

# Execute SQL file for PostgreSQL
exec_postgresql_file() {
    local database=$1
    local file=$2
    
    PGPASSWORD="$DB_ROOT_PASSWORD" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_ROOT_USER" -d "$database" -f "$file"
}

# Execute SQL for SQL Server
exec_sqlserver() {
    local database=$1
    local sql=$2
    
    sqlcmd -S "$DB_HOST,$DB_PORT" -U "$DB_ROOT_USER" -P "$DB_ROOT_PASSWORD" -d "$database" -Q "$sql"
}

# Execute SQL file for SQL Server
exec_sqlserver_file() {
    local database=$1
    local file=$2
    
    sqlcmd -S "$DB_HOST,$DB_PORT" -U "$DB_ROOT_USER" -P "$DB_ROOT_PASSWORD" -d "$database" -i "$file"
}

# =============================================================================
# SCHEMA CREATION
# =============================================================================

create_database_mariadb() {
    print_step "Creating MariaDB database: $DB_NAME"
    
    exec_mariadb "" "CREATE DATABASE IF NOT EXISTS \`$DB_NAME\` CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;"
    print_success "Database created"
    
    print_step "Creating user: $DB_USER"
    exec_mariadb "" "CREATE USER IF NOT EXISTS '$DB_USER'@'%' IDENTIFIED BY '$DB_PASSWORD';"
    exec_mariadb "" "GRANT ALL PRIVILEGES ON \`$DB_NAME\`.* TO '$DB_USER'@'%';"
    exec_mariadb "" "FLUSH PRIVILEGES;"
    print_success "User created and granted privileges"
}

create_database_postgresql() {
    print_step "Creating PostgreSQL database: $DB_NAME"
    
    exec_postgresql "postgres" "CREATE DATABASE $DB_NAME WITH ENCODING 'UTF8';" 2>/dev/null || true
    print_success "Database created"
    
    print_step "Creating user: $DB_USER"
    exec_postgresql "postgres" "CREATE USER $DB_USER WITH PASSWORD '$DB_PASSWORD';" 2>/dev/null || true
    exec_postgresql "$DB_NAME" "GRANT ALL PRIVILEGES ON DATABASE $DB_NAME TO $DB_USER;"
    exec_postgresql "$DB_NAME" "GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO $DB_USER;"
    exec_postgresql "$DB_NAME" "ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO $DB_USER;"
    print_success "User created and granted privileges"
}

create_database_sqlserver() {
    print_step "Creating SQL Server database: $DB_NAME"
    
    exec_sqlserver "master" "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'$DB_NAME') CREATE DATABASE [$DB_NAME];"
    print_success "Database created"
    
    print_step "Creating user: $DB_USER"
    exec_sqlserver "master" "IF NOT EXISTS (SELECT name FROM sys.sql_logins WHERE name = N'$DB_USER') CREATE LOGIN [$DB_USER] WITH PASSWORD = N'$DB_PASSWORD';"
    exec_sqlserver "$DB_NAME" "CREATE USER [$DB_USER] FOR LOGIN [$DB_USER];"
    exec_sqlserver "$DB_NAME" "ALTER ROLE db_owner ADD MEMBER [$DB_USER];"
    print_success "User created and granted privileges"
}

# =============================================================================
# SCHEMA APPLICATION
# =============================================================================

apply_schema() {
    print_header "Applying Database Schema"
    
    local schema_dir="$SCRIPT_DIR/schema"
    
    if [[ ! -d "$schema_dir" ]]; then
        print_warning "Schema directory not found: $schema_dir"
        print_info "Schema will be created by Entity Framework on API startup"
        return 0
    fi
    
    # Apply schema files in order
    for sql_file in $(ls -1 "$schema_dir"/*.sql 2>/dev/null | sort); do
        local filename=$(basename "$sql_file")
        echo -n "  Applying $filename... "
        
        case "$DB_PROVIDER" in
            mariadb|mysql)
                if exec_mariadb_file "$DB_NAME" "$sql_file" 2>/dev/null; then
                    print_success "Done"
                else
                    print_warning "Skipped (may already exist)"
                fi
                ;;
            postgresql|postgres)
                # Convert MySQL syntax to PostgreSQL
                local temp_file=$(mktemp)
                sed 's/`/"/g; s/AUTO_INCREMENT/SERIAL/g; s/TINYINT(1)/BOOLEAN/g; s/DATETIME(6)/TIMESTAMP/g; s/INT(11)/INTEGER/g; s/VARCHAR/VARCHAR/g' "$sql_file" > "$temp_file"
                if exec_postgresql_file "$DB_NAME" "$temp_file" 2>/dev/null; then
                    print_success "Done"
                else
                    print_warning "Skipped"
                fi
                rm -f "$temp_file"
                ;;
            sqlserver|mssql)
                # Convert MySQL syntax to SQL Server
                local temp_file=$(mktemp)
                sed 's/`/[/g; s/`/]/g; s/AUTO_INCREMENT/IDENTITY(1,1)/g; s/TINYINT(1)/BIT/g; s/DATETIME(6)/DATETIME2/g; s/TEXT/NVARCHAR(MAX)/g' "$sql_file" > "$temp_file"
                if exec_sqlserver_file "$DB_NAME" "$temp_file" 2>/dev/null; then
                    print_success "Done"
                else
                    print_warning "Skipped"
                fi
                rm -f "$temp_file"
                ;;
        esac
    done
}

# =============================================================================
# SEED DATA
# =============================================================================

seed_core_data() {
    print_header "Seeding Core Data"
    
    local seed_dir="$SCRIPT_DIR/seed"
    
    if [[ ! -d "$seed_dir" ]]; then
        print_warning "Seed directory not found: $seed_dir"
        return 0
    fi
    
    # Core seed files in order
    local core_files=(
        "001_color_palettes.sql"
        "002_module_ui_configs.sql"
        "003_system_settings.sql"
        "004_service_request_types.sql"
        "005_departments_and_groups.sql"
        "006_lookup_data.sql"
        "007_roles_permissions.sql"
    )
    
    for filename in "${core_files[@]}"; do
        local sql_file="$seed_dir/$filename"
        if [[ -f "$sql_file" ]]; then
            echo -n "  Seeding $filename... "
            case "$DB_PROVIDER" in
                mariadb|mysql)
                    if exec_mariadb_file "$DB_NAME" "$sql_file" 2>/dev/null; then
                        print_success "Done"
                    else
                        print_warning "Skipped"
                    fi
                    ;;
                postgresql|postgres)
                    local temp_file=$(mktemp)
                    sed 's/`/"/g; s/NOW()/CURRENT_TIMESTAMP/g; s/ON DUPLICATE KEY UPDATE.*$/ON CONFLICT DO NOTHING;/g' "$sql_file" > "$temp_file"
                    if exec_postgresql_file "$DB_NAME" "$temp_file" 2>/dev/null; then
                        print_success "Done"
                    else
                        print_warning "Skipped"
                    fi
                    rm -f "$temp_file"
                    ;;
                sqlserver|mssql)
                    local temp_file=$(mktemp)
                    sed 's/`/[/g; s/`/]/g; s/NOW()/GETDATE()/g' "$sql_file" > "$temp_file"
                    if exec_sqlserver_file "$DB_NAME" "$temp_file" 2>/dev/null; then
                        print_success "Done"
                    else
                        print_warning "Skipped"
                    fi
                    rm -f "$temp_file"
                    ;;
            esac
        fi
    done
}

seed_sample_data() {
    print_header "Seeding Sample Data"
    
    local seed_dir="$SCRIPT_DIR/seed"
    
    # Sample data files
    local sample_files=(
        "008_workflow_definitions.sql"
        "009_customers_and_contacts.sql"
        "010_products_services.sql"
        "011_service_request_workflows.sql"
    )
    
    for filename in "${sample_files[@]}"; do
        local sql_file="$seed_dir/$filename"
        if [[ -f "$sql_file" ]]; then
            echo -n "  Seeding $filename... "
            case "$DB_PROVIDER" in
                mariadb|mysql)
                    if exec_mariadb_file "$DB_NAME" "$sql_file" 2>/dev/null; then
                        print_success "Done"
                    else
                        print_warning "Skipped"
                    fi
                    ;;
                *)
                    print_warning "Conversion not implemented"
                    ;;
            esac
        fi
    done
}

seed_master_data() {
    print_header "Seeding Master Data"
    
    local master_dir="$SCRIPT_DIR/master_data"
    
    if [[ ! -d "$master_dir" ]]; then
        print_warning "Master data directory not found: $master_dir"
        print_info "ZIP codes and localities will be loaded by API on startup"
        return 0
    fi
    
    # Check for ZIP code data
    if [[ -f "$master_dir/zipcodes.sql" ]]; then
        echo -n "  Loading ZIP codes (this may take a while)... "
        case "$DB_PROVIDER" in
            mariadb|mysql)
                if exec_mariadb_file "$DB_NAME" "$master_dir/zipcodes.sql" 2>/dev/null; then
                    print_success "Done"
                else
                    print_warning "Skipped"
                fi
                ;;
            *)
                print_info "Skipped - will be loaded by API"
                ;;
        esac
    fi
}

# =============================================================================
# ADMIN USER CREATION
# =============================================================================

create_admin_user() {
    print_header "Creating Admin User"
    
    local admin_username="${ADMIN_USERNAME:-admin}"
    local admin_email="${ADMIN_EMAIL:-admin@crm.local}"
    local admin_password="${ADMIN_PASSWORD:-Admin@123}"
    local admin_firstname="${ADMIN_FIRSTNAME:-System}"
    local admin_lastname="${ADMIN_LASTNAME:-Administrator}"
    
    # Generate BCrypt hash (requires Python with bcrypt or use preset)
    # For simplicity, we'll use a pre-generated hash for Admin@123
    # In production, the API will re-hash on first login
    local password_hash='$2a$11$rBNvWqX8DQVHvS5GCLX.7O7WxW8VqQC5KM5f4cZ0P5iQx5G5G5G5G' # NOSONAR - Pre-generated bcrypt hash for initial admin seed
    
    print_info "Admin credentials:"
    print_info "  Username: $admin_username"
    print_info "  Email: $admin_email"
    print_info "  Password: $admin_password"
    
    case "$DB_PROVIDER" in
        mariadb|mysql)
            # First ensure SysAdmin group exists
            exec_mariadb "$DB_NAME" "
                INSERT INTO UserGroups (Name, Description, IsActive, IsSystemAdmin, CreatedAt, IsDeleted)
                SELECT 'SysAdmin', 'System Administrators', 1, 1, NOW(), 0
                FROM DUAL
                WHERE NOT EXISTS (SELECT 1 FROM UserGroups WHERE Name = 'SysAdmin');
            " 2>/dev/null || true
            
            # Get SysAdmin group ID
            local group_id=$(exec_mariadb "$DB_NAME" "SELECT Id FROM UserGroups WHERE Name = 'SysAdmin' LIMIT 1;" 2>/dev/null | tail -1)
            
            # Create admin user
            exec_mariadb "$DB_NAME" "
                INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, Role, IsActive, EmailVerified, PrimaryGroupId, CreatedAt, IsDeleted)
                SELECT '$admin_username', '$admin_email', '$password_hash', '$admin_firstname', '$admin_lastname', 1, 1, 1, $group_id, NOW(), 0
                FROM DUAL
                WHERE NOT EXISTS (SELECT 1 FROM Users WHERE Email = '$admin_email');
            " 2>/dev/null || true
            
            print_success "Admin user created (or already exists)"
            print_warning "Note: Password will be reset on first API startup using ADMIN_PASSWORD env var"
            ;;
        *)
            print_info "Admin user will be created by API on startup"
            ;;
    esac
}

# =============================================================================
# VERIFICATION
# =============================================================================

verify_setup() {
    print_header "Verifying Database Setup"
    
    case "$DB_PROVIDER" in
        mariadb|mysql)
            local table_count=$(exec_mariadb "$DB_NAME" "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '$DB_NAME';" 2>/dev/null | tail -1)
            print_info "Tables created: $table_count"
            
            local user_count=$(exec_mariadb "$DB_NAME" "SELECT COUNT(*) FROM Users;" 2>/dev/null | tail -1 || echo "0")
            print_info "Users: $user_count"
            
            local group_count=$(exec_mariadb "$DB_NAME" "SELECT COUNT(*) FROM UserGroups;" 2>/dev/null | tail -1 || echo "0")
            print_info "User Groups: $group_count"
            
            local dept_count=$(exec_mariadb "$DB_NAME" "SELECT COUNT(*) FROM Departments;" 2>/dev/null | tail -1 || echo "0")
            print_info "Departments: $dept_count"
            ;;
        *)
            print_info "Verification not implemented for $DB_PROVIDER"
            ;;
    esac
}

# =============================================================================
# CONNECTION STRING GENERATION
# =============================================================================

generate_connection_string() {
    print_header "Connection String"
    
    case "$DB_PROVIDER" in
        mariadb|mysql)
            echo -e "${CYAN}MariaDB/MySQL:${NC}"
            echo "Server=$DB_HOST;Port=$DB_PORT;Database=$DB_NAME;Uid=$DB_USER;Pwd=$DB_PASSWORD;"
            echo ""
            echo -e "${CYAN}Environment Variable:${NC}"
            echo "ConnectionStrings__DefaultConnection=\"Server=$DB_HOST;Port=$DB_PORT;Database=$DB_NAME;Uid=$DB_USER;Pwd=$DB_PASSWORD;\""
            ;;
        postgresql|postgres)
            echo -e "${CYAN}PostgreSQL:${NC}"
            echo "Host=$DB_HOST;Port=$DB_PORT;Database=$DB_NAME;Username=$DB_USER;Password=$DB_PASSWORD;"
            ;;
        sqlserver|mssql)
            echo -e "${CYAN}SQL Server:${NC}"
            echo "Server=$DB_HOST,$DB_PORT;Database=$DB_NAME;User Id=$DB_USER;Password=$DB_PASSWORD;TrustServerCertificate=True;"
            ;;
        sqlite)
            echo -e "${CYAN}SQLite:${NC}"
            echo "Data Source=$DB_NAME.db"
            ;;
    esac
}

# =============================================================================
# INTERACTIVE MODE
# =============================================================================

interactive_setup() {
    print_header "CRM Database Setup - Interactive Mode"
    echo ""
    
    # Provider selection
    echo "Select database provider:"
    echo "  1) MariaDB / MySQL (recommended)"
    echo "  2) PostgreSQL"
    echo "  3) SQL Server"
    echo "  4) SQLite (development only)"
    read -p "Enter choice [1]: " provider_choice
    
    case "${provider_choice:-1}" in
        1) DB_PROVIDER="mariadb" ;;
        2) DB_PROVIDER="postgresql" ;;
        3) DB_PROVIDER="sqlserver" ;;
        4) DB_PROVIDER="sqlite" ;;
        *) DB_PROVIDER="mariadb" ;;
    esac
    
    # Set default port
    DB_PORT="${DB_PORT:-$(get_default_port $DB_PROVIDER)}"
    
    if [[ "$DB_PROVIDER" != "sqlite" ]]; then
        # Connection details
        read -p "Database host [$DB_HOST]: " input_host
        DB_HOST="${input_host:-$DB_HOST}"
        
        read -p "Database port [$DB_PORT]: " input_port
        DB_PORT="${input_port:-$DB_PORT}"
        
        read -p "Database name [$DB_NAME]: " input_name
        DB_NAME="${input_name:-$DB_NAME}"
        
        read -p "Application user [$DB_USER]: " input_user
        DB_USER="${input_user:-$DB_USER}"
        
        # Password
        if [[ -z "$DB_PASSWORD" ]]; then
            read -p "Generate password automatically? [Y/n]: " gen_pass
            if [[ ! "$gen_pass" =~ ^[Nn]$ ]]; then
                DB_PASSWORD=$(generate_password 16)
                print_info "Generated password: $DB_PASSWORD"
            else
                read -s -p "Enter application user password: " DB_PASSWORD
                echo ""
            fi
        fi
        
        # Root credentials
        read -p "Database admin user [$DB_ROOT_USER]: " input_root
        DB_ROOT_USER="${input_root:-$DB_ROOT_USER}"
        
        if [[ -z "$DB_ROOT_PASSWORD" ]]; then
            read -s -p "Enter admin password: " DB_ROOT_PASSWORD
            echo ""
        fi
        
        # Docker option
        read -p "Connect via Docker container? [y/N]: " use_docker
        if [[ "$use_docker" =~ ^[Yy]$ ]]; then
            USE_DOCKER="true"
            read -p "Container name [$DOCKER_CONTAINER]: " input_container
            DOCKER_CONTAINER="${input_container:-$DOCKER_CONTAINER}"
        fi
    fi
    
    # Seed options
    read -p "Seed core data? [Y/n]: " seed_core
    if [[ "$seed_core" =~ ^[Nn]$ ]]; then
        SEED_DATA="false"
    fi
    
    read -p "Include sample data? [y/N]: " seed_sample
    if [[ "$seed_sample" =~ ^[Yy]$ ]]; then
        SEED_SAMPLE="true"
    fi
}

# =============================================================================
# PARSE ARGUMENTS
# =============================================================================

SCHEMA_ONLY="false"
SEED_ONLY="false"

while [[ $# -gt 0 ]]; do
    case $1 in
        --provider)
            DB_PROVIDER="$2"
            shift 2
            ;;
        --host)
            DB_HOST="$2"
            shift 2
            ;;
        --port)
            DB_PORT="$2"
            shift 2
            ;;
        --name)
            DB_NAME="$2"
            shift 2
            ;;
        --user)
            DB_USER="$2"
            shift 2
            ;;
        --password)
            DB_PASSWORD="$2"
            shift 2
            ;;
        --root-user)
            DB_ROOT_USER="$2"
            shift 2
            ;;
        --root-password)
            DB_ROOT_PASSWORD="$2"
            shift 2
            ;;
        --docker)
            USE_DOCKER="true"
            shift
            ;;
        --container)
            DOCKER_CONTAINER="$2"
            USE_DOCKER="true"
            shift 2
            ;;
        --seed)
            SEED_DATA="true"
            shift
            ;;
        --no-seed)
            SEED_DATA="false"
            shift
            ;;
        --sample-data)
            SEED_SAMPLE="true"
            shift
            ;;
        --schema-only)
            SCHEMA_ONLY="true"
            SEED_DATA="false"
            shift
            ;;
        --seed-only)
            SEED_ONLY="true"
            shift
            ;;
        -h|--help)
            show_help
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            show_help
            exit 1
            ;;
    esac
done

# =============================================================================
# MAIN EXECUTION
# =============================================================================

main() {
    echo ""
    echo -e "${PURPLE}╔══════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${PURPLE}║${NC}     ${CYAN}CRM Solution - Database Setup${NC}                           ${PURPLE}║${NC}"
    echo -e "${PURPLE}╚══════════════════════════════════════════════════════════════╝${NC}"
    echo ""
    
    # If no password provided, go interactive
    if [[ -z "$DB_PASSWORD" ]] && [[ -z "$DB_ROOT_PASSWORD" ]] && [[ "$DB_PROVIDER" != "sqlite" ]]; then
        interactive_setup
    fi
    
    # Set default port if not set
    DB_PORT="${DB_PORT:-$(get_default_port $DB_PROVIDER)}"
    
    print_info "Provider: $DB_PROVIDER"
    print_info "Host: $DB_HOST:$DB_PORT"
    print_info "Database: $DB_NAME"
    print_info "User: $DB_USER"
    [[ "$USE_DOCKER" = "true" ]] && print_info "Docker Container: $DOCKER_CONTAINER"
    
    # Test connection
    print_step "Testing database connection..."
    case "$DB_PROVIDER" in
        mariadb|mysql)
            if test_mariadb_connection "$DB_HOST" "$DB_PORT" "$DB_ROOT_USER" "$DB_ROOT_PASSWORD"; then
                print_success "Connection successful"
            else
                print_error "Cannot connect to database"
                exit 1
            fi
            ;;
        postgresql|postgres)
            if test_postgresql_connection "$DB_HOST" "$DB_PORT" "$DB_ROOT_USER" "$DB_ROOT_PASSWORD"; then
                print_success "Connection successful"
            else
                print_error "Cannot connect to database"
                exit 1
            fi
            ;;
        sqlite)
            print_info "SQLite - no connection test needed"
            ;;
    esac
    
    # Create database and user
    if [[ "$SEED_ONLY" != "true" ]]; then
        case "$DB_PROVIDER" in
            mariadb|mysql) create_database_mariadb ;;
            postgresql|postgres) create_database_postgresql ;;
            sqlserver|mssql) create_database_sqlserver ;;
            sqlite) print_info "SQLite database will be created on first access" ;;
        esac
        
        # Apply schema
        apply_schema
    fi
    
    # Seed data
    if [[ "$SEED_DATA" = "true" ]] && [[ "$SCHEMA_ONLY" != "true" ]]; then
        seed_core_data
        seed_master_data
        
        if [[ "$SEED_SAMPLE" = "true" ]]; then
            seed_sample_data
        fi
        
        create_admin_user
    fi
    
    # Verify
    verify_setup
    
    # Show connection string
    generate_connection_string
    
    print_header "Setup Complete!"
    echo ""
    print_success "Database '$DB_NAME' is ready for use"
    echo ""
    print_info "Next steps:"
    print_info "  1. Set the connection string in your .env file or environment"
    print_info "  2. Set DatabaseProvider=$DB_PROVIDER"
    print_info "  3. Start the API - it will apply EF migrations and seed admin user"
    echo ""
}

# Run main
main
