#!/bin/bash
# =============================================================================
# CRM Solution - Environment Setup Script
# =============================================================================
# This script creates a .env file from .env.example template
# It auto-generates secure secrets or prompts for user input
#
# Usage:
#   ./scripts/setup-env.sh              # Interactive mode
#   ./scripts/setup-env.sh --auto       # Auto-generate all secrets
#   ./scripts/setup-env.sh --dev        # Development mode with defaults
# =============================================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
ENV_EXAMPLE="$ROOT_DIR/.env.example"
ENV_FILE="$ROOT_DIR/.env"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

# =============================================================================
# HELPER FUNCTIONS
# =============================================================================

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Generate a random secure string
generate_secret() {
    local length=${1:-32}
    openssl rand -base64 $length | tr -d '/+=' | head -c $length
}

# Generate a random password with special chars
generate_password() {
    local length=${1:-16}
    # Generate password with letters, numbers, and @ symbol
    local pass=$(openssl rand -base64 $length | tr -d '/+=' | head -c $((length-2)))
    echo "${pass}@1"
}

# =============================================================================
# MAIN LOGIC
# =============================================================================

MODE="interactive"
case "${1:-}" in
    --auto)
        MODE="auto"
        ;;
    --dev|--development)
        MODE="dev"
        ;;
    --help|-h)
        cat << 'EOF'
CRM Solution - Environment Setup Script

Usage:
  ./scripts/setup-env.sh              Interactive mode (prompts for values)
  ./scripts/setup-env.sh --auto       Auto-generate all secrets
  ./scripts/setup-env.sh --dev        Development mode with defaults

This script creates a .env file from .env.example with:
  - Auto-generated secure JWT keys
  - Random database passwords
  - Admin user credentials

EOF
        exit 0
        ;;
esac

echo ""
echo "=============================================="
echo "  CRM Solution - Environment Setup"
echo "=============================================="
echo ""

# Check if .env already exists
if [[ -f "$ENV_FILE" ]]; then
    log_warning ".env file already exists!"
    read -p "Do you want to overwrite it? (y/N): " confirm
    if [[ ! "$confirm" =~ ^[Yy]$ ]]; then
        log_info "Keeping existing .env file"
        exit 0
    fi
    # Backup existing file
    cp "$ENV_FILE" "$ENV_FILE.backup.$(date +%Y%m%d_%H%M%S)"
    log_info "Backed up existing .env file"
fi

# Check if .env.example exists
if [[ ! -f "$ENV_EXAMPLE" ]]; then
    log_error ".env.example not found at $ENV_EXAMPLE"
    exit 1
fi

# Copy template
cp "$ENV_EXAMPLE" "$ENV_FILE"
log_info "Created .env from template"

# =============================================================================
# GENERATE/SET VALUES
# =============================================================================

if [[ "$MODE" = "dev" ]]; then
    log_info "Setting up for DEVELOPMENT mode..."
    
    # Use development defaults
    sed -i.bak "s/ASPNETCORE_ENVIRONMENT=Production/ASPNETCORE_ENVIRONMENT=Development/" "$ENV_FILE"
    sed -i.bak "s/JWT_KEY=CHANGE_ME_TO_SECURE_KEY_AT_LEAST_32_CHARS/JWT_KEY=DevJwtKey2024SecureAtLeast32Chars!/" "$ENV_FILE"
    sed -i.bak "s/DB_PASSWORD=CHANGE_ME_DB_PASSWORD/DB_PASSWORD=DevDbPass@2024/" "$ENV_FILE"
    sed -i.bak "s/DB_ROOT_PASSWORD=CHANGE_ME_ROOT_PASSWORD/DB_ROOT_PASSWORD=DevRootPass@2024/" "$ENV_FILE"
    sed -i.bak "s/ADMIN_PASSWORD=CHANGE_ME_ADMIN_PASSWORD/ADMIN_PASSWORD=Admin@123/" "$ENV_FILE"
    sed -i.bak "s/ADMIN_EMAIL=/ADMIN_EMAIL=admin@localhost/" "$ENV_FILE"
    
    rm -f "$ENV_FILE.bak"
    log_success "Development environment configured!"
    
elif [[ "$MODE" = "auto" ]]; then
    log_info "Auto-generating secure secrets..."
    
    JWT_KEY=$(generate_secret 48)
    DB_PASSWORD=$(generate_password 16)
    DB_ROOT_PASSWORD=$(generate_password 20)
    ADMIN_PASSWORD=$(generate_password 14)
    
    sed -i.bak "s/JWT_KEY=CHANGE_ME_TO_SECURE_KEY_AT_LEAST_32_CHARS/JWT_KEY=$JWT_KEY/" "$ENV_FILE"
    sed -i.bak "s/DB_PASSWORD=CHANGE_ME_DB_PASSWORD/DB_PASSWORD=$DB_PASSWORD/" "$ENV_FILE"
    sed -i.bak "s/DB_ROOT_PASSWORD=CHANGE_ME_ROOT_PASSWORD/DB_ROOT_PASSWORD=$DB_ROOT_PASSWORD/" "$ENV_FILE"
    sed -i.bak "s/ADMIN_PASSWORD=CHANGE_ME_ADMIN_PASSWORD/ADMIN_PASSWORD=$ADMIN_PASSWORD/" "$ENV_FILE"
    
    rm -f "$ENV_FILE.bak"
    
    echo ""
    log_success "Secrets generated successfully!"
    echo ""
    echo "================================================"
    echo "  GENERATED CREDENTIALS - SAVE THESE SECURELY!"
    echo "================================================"
    echo ""
    echo "  Database Root Password: $DB_ROOT_PASSWORD"
    echo "  Database User Password: $DB_PASSWORD"
    echo "  Admin Login Password:   $ADMIN_PASSWORD"
    echo ""
    echo "  Admin Username: admin"
    echo "  Admin Email:    admin@crm.local (default)"
    echo ""
    echo "================================================"
    echo ""
    
else
    # Interactive mode
    log_info "Interactive mode - please provide values..."
    echo ""
    
    # Environment
    read -p "Environment [Production/Development] (Production): " env_mode
    env_mode=${env_mode:-Production}
    sed -i.bak "s/ASPNETCORE_ENVIRONMENT=Production/ASPNETCORE_ENVIRONMENT=$env_mode/" "$ENV_FILE"
    
    # JWT Key
    echo ""
    read -p "Generate JWT key automatically? (Y/n): " gen_jwt
    if [[ ! "$gen_jwt" =~ ^[Nn]$ ]]; then
        JWT_KEY=$(generate_secret 48)
        log_info "Generated JWT key: ${JWT_KEY:0:20}..."
    else
        read -p "Enter JWT key (min 32 chars): " JWT_KEY
    fi
    sed -i.bak "s/JWT_KEY=CHANGE_ME_TO_SECURE_KEY_AT_LEAST_32_CHARS/JWT_KEY=$JWT_KEY/" "$ENV_FILE"
    
    # Database passwords
    echo ""
    read -p "Generate database passwords automatically? (Y/n): " gen_db
    if [[ ! "$gen_db" =~ ^[Nn]$ ]]; then
        DB_PASSWORD=$(generate_password 16)
        DB_ROOT_PASSWORD=$(generate_password 20)
        log_info "Generated database passwords"
    else
        read -p "Enter database user password: " DB_PASSWORD
        read -p "Enter database root password: " DB_ROOT_PASSWORD
    fi
    sed -i.bak "s/DB_PASSWORD=CHANGE_ME_DB_PASSWORD/DB_PASSWORD=$DB_PASSWORD/" "$ENV_FILE"
    sed -i.bak "s/DB_ROOT_PASSWORD=CHANGE_ME_ROOT_PASSWORD/DB_ROOT_PASSWORD=$DB_ROOT_PASSWORD/" "$ENV_FILE"
    
    # Admin user
    echo ""
    read -p "Admin username (admin): " admin_user
    admin_user=${admin_user:-admin}
    sed -i.bak "s/ADMIN_USERNAME=admin/ADMIN_USERNAME=$admin_user/" "$ENV_FILE"
    
    read -p "Admin email (leave empty for admin@crm.local): " admin_email
    if [[ -n "$admin_email" ]]; then
        sed -i.bak "s/ADMIN_EMAIL=/ADMIN_EMAIL=$admin_email/" "$ENV_FILE"
    fi
    
    read -p "Generate admin password automatically? (Y/n): " gen_admin
    if [[ ! "$gen_admin" =~ ^[Nn]$ ]]; then
        ADMIN_PASSWORD=$(generate_password 14)
        log_info "Generated admin password"
    else
        read -p "Enter admin password: " ADMIN_PASSWORD
    fi
    sed -i.bak "s/ADMIN_PASSWORD=CHANGE_ME_ADMIN_PASSWORD/ADMIN_PASSWORD=$ADMIN_PASSWORD/" "$ENV_FILE"
    
    # Deployment host (optional)
    echo ""
    read -p "Remote deployment host (leave empty for local only): " deploy_host
    if [[ -n "$deploy_host" ]]; then
        sed -i.bak "s/DEPLOY_HOST=/DEPLOY_HOST=$deploy_host/" "$ENV_FILE"
    fi
    
    rm -f "$ENV_FILE.bak"
    
    echo ""
    log_success "Environment configured successfully!"
    echo ""
    echo "================================================"
    echo "  CREDENTIALS SUMMARY"
    echo "================================================"
    echo ""
    echo "  Admin Username:         $admin_user"
    echo "  Admin Email:            ${admin_email:-admin@crm.local}"
    echo "  Admin Password:         $ADMIN_PASSWORD"
    echo ""
    echo "  Database User Password: $DB_PASSWORD"
    echo "  Database Root Password: $DB_ROOT_PASSWORD"
    echo ""
    echo "================================================"
fi

echo ""
log_info "Next steps:"
echo "  1. Review the .env file: cat $ENV_FILE"
echo "  2. Build and deploy: ./build.sh deploy"
echo ""
log_success "Environment setup complete!"
