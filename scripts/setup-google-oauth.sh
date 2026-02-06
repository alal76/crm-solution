#!/bin/bash
# ============================================================================
# Setup Google OAuth for Local Development
# ============================================================================
# This script helps configure Google OAuth for local CRM deployments.
#
# Prerequisites:
# 1. Google Cloud project with OAuth 2.0 credentials
# 2. CRM Frontend running locally or on a dev server
# 3. Access to the CRM database (Docker or direct)
#
# Usage:
#   ./scripts/setup-google-oauth.sh
#
# The script will:
# 1. Prompt for your Google OAuth credentials
# 2. Update the frontend .env file
# 3. Update the backend SystemSettings in the database
# ============================================================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

echo -e "${CYAN}============================================${NC}"
echo -e "${CYAN}  Google OAuth Setup for CRM Solution${NC}"
echo -e "${CYAN}============================================${NC}"
echo ""

# Check if config file exists with credentials
CONFIG_FILE="$PROJECT_ROOT/config/google-oauth.local.env"
if [[ -f "$CONFIG_FILE" ]]; then
    source "$CONFIG_FILE"
    if [[ "$GOOGLE_CLIENT_ID" != "your-client-id.apps.googleusercontent.com" && -n "$GOOGLE_CLIENT_ID" ]]; then
        echo -e "${GREEN}Found existing credentials in config/google-oauth.local.env${NC}"
        echo "Client ID: ${GOOGLE_CLIENT_ID:0:30}..."
        echo ""
        read -p "Use these credentials? (y/n): " USE_EXISTING
        if [[ "$USE_EXISTING" != "y" && "$USE_EXISTING" != "Y" ]]; then
            unset GOOGLE_CLIENT_ID
            unset GOOGLE_CLIENT_SECRET
        fi
    fi
fi

# Prompt for credentials if not set
if [[ -z "$GOOGLE_CLIENT_ID" || "$GOOGLE_CLIENT_ID" == "your-client-id.apps.googleusercontent.com" ]]; then
    echo ""
    echo -e "${YELLOW}Google OAuth Credentials Setup${NC}"
    echo ""
    echo "To get these credentials:"
    echo "1. Go to https://console.cloud.google.com/"
    echo "2. Create/select a project"
    echo "3. Go to APIs & Services > Credentials"
    echo "4. Create OAuth 2.0 Client ID (Web Application)"
    echo "5. Add authorized JavaScript origins:"
    echo "   - http://localhost:3000"
    echo "   - http://localhost:5000"  
    echo "   - http://192.168.0.9 (your dev server)"
    echo ""
    
    read -p "Enter Google Client ID: " GOOGLE_CLIENT_ID
    read -p "Enter Google Client Secret: " GOOGLE_CLIENT_SECRET
    
    if [[ -z "$GOOGLE_CLIENT_ID" || -z "$GOOGLE_CLIENT_SECRET" ]]; then
        echo -e "${RED}Error: Both Client ID and Client Secret are required${NC}"
        exit 1
    fi
    
    # Save to config file
    echo "Saving credentials to config/google-oauth.local.env..."
    cat > "$CONFIG_FILE" << EOF
# ============================================================================
# LOCAL GOOGLE OAUTH CONFIGURATION
# ============================================================================
# Generated on: $(date)
# DO NOT COMMIT THIS FILE TO SOURCE CONTROL
# ============================================================================

GOOGLE_CLIENT_ID=${GOOGLE_CLIENT_ID}
GOOGLE_CLIENT_SECRET=${GOOGLE_CLIENT_SECRET}
EOF
    
    echo -e "${GREEN}✓ Credentials saved to config/google-oauth.local.env${NC}"
fi

echo ""
echo -e "${YELLOW}Choose where to apply these credentials:${NC}"
echo "1. Frontend only (.env file)"
echo "2. Backend only (database SystemSettings)"
echo "3. Both frontend and backend"
echo "4. Remote server (192.168.0.9)"
echo ""
read -p "Enter choice (1-4): " CHOICE

case $CHOICE in
    1)
        # Frontend only
        FRONTEND_ENV="$PROJECT_ROOT/CRM.Frontend/.env"
        if [[ -f "$FRONTEND_ENV" ]]; then
            # Update existing .env
            if grep -q "REACT_APP_GOOGLE_CLIENT_ID" "$FRONTEND_ENV"; then
                sed -i.bak "s|^REACT_APP_GOOGLE_CLIENT_ID=.*|REACT_APP_GOOGLE_CLIENT_ID=${GOOGLE_CLIENT_ID}|" "$FRONTEND_ENV"
            else
                echo "" >> "$FRONTEND_ENV"
                echo "# Google OAuth" >> "$FRONTEND_ENV"
                echo "REACT_APP_GOOGLE_CLIENT_ID=${GOOGLE_CLIENT_ID}" >> "$FRONTEND_ENV"
            fi
        else
            # Create new .env
            cat > "$FRONTEND_ENV" << EOF
REACT_APP_API_URL=http://localhost:5000/api
NODE_ENV=development
REACT_APP_GOOGLE_CLIENT_ID=${GOOGLE_CLIENT_ID}
EOF
        fi
        echo -e "${GREEN}✓ Frontend .env updated${NC}"
        echo ""
        echo "Next step: Restart the frontend dev server"
        echo "  cd CRM.Frontend && npm start"
        ;;
        
    2)
        # Backend only (database)
        echo ""
        echo "Updating database SystemSettings..."
        SQL="UPDATE SystemSettings SET GoogleAuthEnabled = 1, GoogleClientId = '${GOOGLE_CLIENT_ID}', GoogleClientSecret = '${GOOGLE_CLIENT_SECRET}', UpdatedAt = NOW() WHERE Id = 1; SELECT GoogleAuthEnabled, GoogleClientId FROM SystemSettings WHERE Id = 1;"
        
        if command -v docker &> /dev/null; then
            docker exec -i crm-mariadb mariadb -u root -pRootPass@Dev2024 crm_db -e "$SQL"
            echo -e "${GREEN}✓ Database SystemSettings updated${NC}"
        else
            echo -e "${YELLOW}Docker not found. Run this SQL manually:${NC}"
            echo "$SQL"
        fi
        ;;
        
    3)
        # Both frontend and backend
        FRONTEND_ENV="$PROJECT_ROOT/CRM.Frontend/.env"
        if [[ -f "$FRONTEND_ENV" ]]; then
            if grep -q "REACT_APP_GOOGLE_CLIENT_ID" "$FRONTEND_ENV"; then
                sed -i.bak "s|^REACT_APP_GOOGLE_CLIENT_ID=.*|REACT_APP_GOOGLE_CLIENT_ID=${GOOGLE_CLIENT_ID}|" "$FRONTEND_ENV"
            else
                echo "" >> "$FRONTEND_ENV"
                echo "# Google OAuth" >> "$FRONTEND_ENV"
                echo "REACT_APP_GOOGLE_CLIENT_ID=${GOOGLE_CLIENT_ID}" >> "$FRONTEND_ENV"
            fi
        else
            cat > "$FRONTEND_ENV" << EOF
REACT_APP_API_URL=http://localhost:5000/api
NODE_ENV=development
REACT_APP_GOOGLE_CLIENT_ID=${GOOGLE_CLIENT_ID}
EOF
        fi
        echo -e "${GREEN}✓ Frontend .env updated${NC}"
        
        SQL="UPDATE SystemSettings SET GoogleAuthEnabled = 1, GoogleClientId = '${GOOGLE_CLIENT_ID}', GoogleClientSecret = '${GOOGLE_CLIENT_SECRET}', UpdatedAt = NOW() WHERE Id = 1;"
        if command -v docker &> /dev/null; then
            docker exec -i crm-mariadb mariadb -u root -pRootPass@Dev2024 crm_db -e "$SQL"
            echo -e "${GREEN}✓ Database SystemSettings updated${NC}"
        else
            echo -e "${YELLOW}Docker not found locally. Trying local mysql...${NC}"
            mysql -h localhost -u crm_user -p crm_db -e "$SQL" 2>/dev/null || echo "Run SQL manually on backend database"
        fi
        ;;
        
    4)
        # Remote server
        REMOTE_HOST="192.168.0.9"
        echo ""
        echo "Updating Google OAuth on remote server ${REMOTE_HOST}..."
        
        SQL="UPDATE SystemSettings SET GoogleAuthEnabled = 1, GoogleClientId = '${GOOGLE_CLIENT_ID}', GoogleClientSecret = '${GOOGLE_CLIENT_SECRET}', UpdatedAt = NOW() WHERE Id = 1; SELECT GoogleAuthEnabled, LEFT(GoogleClientId, 30) as ClientIdPrefix FROM SystemSettings WHERE Id = 1;"
        
        ssh "root@${REMOTE_HOST}" "docker exec -i crm-mariadb mariadb -u root -pRootPass@Dev2024 crm_db -e \"${SQL}\""
        
        echo -e "${GREEN}✓ Remote database updated${NC}"
        echo ""
        echo -e "${YELLOW}Note: You also need to update the frontend .env on the remote server.${NC}"
        echo "If using prebuilt frontend, you may need to rebuild with REACT_APP_GOOGLE_CLIENT_ID set."
        ;;
        
    *)
        echo -e "${RED}Invalid choice${NC}"
        exit 1
        ;;
esac

echo ""
echo -e "${GREEN}============================================${NC}"
echo -e "${GREEN}  Google OAuth Setup Complete!${NC}"
echo -e "${GREEN}============================================${NC}"
echo ""
echo "Important reminders:"
echo "1. Make sure your Google Cloud Console authorized origins include:"
echo "   - http://localhost:3000"
echo "   - http://localhost:5000"
echo "   - http://192.168.0.9"
echo ""
echo "2. The frontend needs REACT_APP_GOOGLE_CLIENT_ID in .env"
echo "3. The backend needs GoogleAuthEnabled=true in SystemSettings"
echo ""
echo -e "${YELLOW}Files NOT committed to git:${NC}"
echo "  - config/google-oauth.local.env"
echo "  - CRM.Frontend/.env"
echo ""
