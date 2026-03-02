#!/bin/bash
# ============================================================================
# Reset Admin Password Script
# ============================================================================
# This script resets the admin user (admin@crm.local) to require a fresh
# password setup on next login.
#
# It sets PasswordNeverSet=true and clears the password hash, which will
# trigger the password setup flow when the user logs in.
#
# Usage:
#   ./scripts/reset-admin-password.sh [options]
#
# Options:
#   --docker          Run against the Docker MariaDB container (crm-mariadb)
#   --host HOST       Database host (default: localhost)
#   --port PORT       Database port (default: 3306)
#   --user USER       Database user (default: crm_user)
#   --password PASS   Database password (default: prompt)
#   --database DB     Database name (default: crm_db)
#   --remote HOST     Run on remote server via SSH
#   --help            Show this help message
#
# Examples:
#   # Run against local Docker container
#   ./scripts/reset-admin-password.sh --docker
#
#   # Run against remote server 192.168.0.9
#   ./scripts/reset-admin-password.sh --remote 192.168.0.9
#
#   # Run against custom database
#   ./scripts/reset-admin-password.sh --host db.example.com --password mypass
# ============================================================================

set -e

# Default values
DB_HOST="localhost"
DB_PORT="3306"
DB_USER="crm_user"
DB_PASSWORD=""
DB_NAME="crm_db"
ADMIN_EMAIL="admin@crm.local"
USE_DOCKER=false
REMOTE_HOST=""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --docker)
            USE_DOCKER=true
            shift
            ;;
        --host)
            DB_HOST="$2"
            shift 2
            ;;
        --port)
            DB_PORT="$2"
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
        --database)
            DB_NAME="$2"
            shift 2
            ;;
        --remote)
            REMOTE_HOST="$2"
            shift 2
            ;;
        --help)
            head -40 "$0" | tail -35
            exit 0
            ;;
        *)
            echo -e "${RED}Unknown option: $1${NC}"
            exit 1
            ;;
    esac
done

# SQL to reset admin password
SQL_COMMAND="
UPDATE Users 
SET 
    PasswordHash = '',
    PasswordNeverSet = 1,
    MustResetPassword = 0,
    PasswordLastChangedAt = NULL,
    UpdatedAt = NOW()
WHERE Email = '${ADMIN_EMAIL}';

SELECT 
    Id, 
    Username, 
    Email, 
    PasswordNeverSet, 
    MustResetPassword,
    IsActive
FROM Users 
WHERE Email = '${ADMIN_EMAIL}';
"

echo -e "${YELLOW}=== Reset Admin Password Script ===${NC}"
echo "This will reset admin@crm.local to require password setup on next login."
echo ""

if [[ -n "$REMOTE_HOST" ]]; then
    # Run on remote server via SSH
    echo -e "${YELLOW}Running on remote server: ${REMOTE_HOST}${NC}"
    
    ssh "root@${REMOTE_HOST}" << EOF
docker exec -i crm-mariadb mariadb -u root -p${DB_ROOT_PASSWORD:?Set DB_ROOT_PASSWORD} crm_db << 'EOSQL'
${SQL_COMMAND}
EOSQL
EOF
    
    echo ""
    echo -e "${GREEN}✓ Admin password reset on remote server ${REMOTE_HOST}${NC}"
    
elif [[ "$USE_DOCKER" == "true" ]]; then
    # Run against Docker container
    echo -e "${YELLOW}Running against Docker container: crm-mariadb${NC}"
    
    docker exec -i crm-mariadb mariadb -u root -p"${DB_ROOT_PASSWORD:?Set DB_ROOT_PASSWORD}" crm_db << EOF
${SQL_COMMAND}
EOF
    
    echo ""
    echo -e "${GREEN}✓ Admin password reset in Docker container${NC}"
    
else
    # Run against direct database connection
    echo -e "${YELLOW}Running against database: ${DB_HOST}:${DB_PORT}/${DB_NAME}${NC}"
    
    if [[ -z "$DB_PASSWORD" ]]; then
        echo -n "Enter database password: "
        read -s DB_PASSWORD
        echo ""
    fi
    
    mysql -h "$DB_HOST" -P "$DB_PORT" -u "$DB_USER" -p"$DB_PASSWORD" "$DB_NAME" << EOF
${SQL_COMMAND}
EOF
    
    echo ""
    echo -e "${GREEN}✓ Admin password reset${NC}"
fi

echo ""
echo -e "${GREEN}The admin user (admin@crm.local) will now be prompted to set a${NC}"
echo -e "${GREEN}new password on next login.${NC}"
echo ""
echo "Next steps:"
echo "  1. Navigate to the CRM login page"
echo "  2. Enter email: admin@crm.local"
echo "  3. You will be redirected to set a new password"
echo ""
