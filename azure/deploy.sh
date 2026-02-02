#!/bin/bash
# =============================================================================
# CRM Solution - Azure Deployment Script
# Version: 1.0.0
# Description: Deploy CRM Solution to Azure
# =============================================================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default values
ENVIRONMENT="dev"
LOCATION="eastus"
RESOURCE_GROUP="crm-solution-rg"
BASE_NAME="crm"

# Function: Print colored message
print_msg() {
    local color=$1
    local msg=$2
    echo -e "${color}${msg}${NC}"
}

# Function: Show usage
show_usage() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  -e, --environment    Environment (dev, staging, prod) [default: dev]"
    echo "  -l, --location       Azure region [default: eastus]"
    echo "  -g, --resource-group Resource group name [default: crm-solution-rg]"
    echo "  -n, --name           Base name for resources [default: crm]"
    echo "  --infrastructure     Deploy infrastructure only"
    echo "  --app                Deploy application only"
    echo "  --database           Run database migrations only"
    echo "  -h, --help           Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 -e dev                    # Deploy to dev environment"
    echo "  $0 -e prod --infrastructure  # Deploy prod infrastructure only"
    echo "  $0 -e staging --app          # Deploy app to staging"
}

# Parse arguments
DEPLOY_INFRA=true
DEPLOY_APP=true
DEPLOY_DB=true

while [[ $# -gt 0 ]]; do
    case $1 in
        -e|--environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -l|--location)
            LOCATION="$2"
            shift 2
            ;;
        -g|--resource-group)
            RESOURCE_GROUP="$2"
            shift 2
            ;;
        -n|--name)
            BASE_NAME="$2"
            shift 2
            ;;
        --infrastructure)
            DEPLOY_APP=false
            DEPLOY_DB=false
            shift
            ;;
        --app)
            DEPLOY_INFRA=false
            DEPLOY_DB=false
            shift
            ;;
        --database)
            DEPLOY_INFRA=false
            DEPLOY_APP=false
            shift
            ;;
        -h|--help)
            show_usage
            exit 0
            ;;
        *)
            print_msg $RED "Unknown option: $1"
            show_usage
            exit 1
            ;;
    esac
done

print_msg $BLUE "╔══════════════════════════════════════════════════════════════╗"
print_msg $BLUE "║          CRM Solution - Azure Deployment                     ║"
print_msg $BLUE "╠══════════════════════════════════════════════════════════════╣"
print_msg $BLUE "║  Environment:     $ENVIRONMENT"
print_msg $BLUE "║  Location:        $LOCATION"
print_msg $BLUE "║  Resource Group:  $RESOURCE_GROUP"
print_msg $BLUE "║  Base Name:       $BASE_NAME"
print_msg $BLUE "╚══════════════════════════════════════════════════════════════╝"
echo ""

# Check Azure CLI is installed
if ! command -v az &> /dev/null; then
    print_msg $RED "Azure CLI is not installed. Please install it first."
    print_msg $YELLOW "Visit: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi

# Check if logged in
print_msg $YELLOW "Checking Azure login status..."
if ! az account show &> /dev/null; then
    print_msg $YELLOW "Please log in to Azure..."
    az login
fi

SUBSCRIPTION=$(az account show --query name -o tsv)
print_msg $GREEN "✓ Logged in to Azure subscription: $SUBSCRIPTION"

# =============================================================================
# STEP 1: Create Resource Group
# =============================================================================
if $DEPLOY_INFRA; then
    print_msg $YELLOW "\n[1/4] Creating resource group..."
    az group create \
        --name "$RESOURCE_GROUP-$ENVIRONMENT" \
        --location "$LOCATION" \
        --output none
    print_msg $GREEN "✓ Resource group created: $RESOURCE_GROUP-$ENVIRONMENT"

    # =============================================================================
    # STEP 2: Deploy Infrastructure with Bicep
    # =============================================================================
    print_msg $YELLOW "\n[2/4] Deploying infrastructure..."
    
    # Generate secrets if not provided
    JWT_SECRET=$(openssl rand -base64 64 | tr -d '\n')
    MYSQL_PASSWORD=$(openssl rand -base64 32 | tr -d '\n' | head -c 24)
    
    DEPLOYMENT_OUTPUT=$(az deployment group create \
        --name "crm-deployment-$(date +%Y%m%d%H%M%S)" \
        --resource-group "$RESOURCE_GROUP-$ENVIRONMENT" \
        --template-file azure/main.bicep \
        --parameters environment="$ENVIRONMENT" \
        --parameters location="$LOCATION" \
        --parameters baseName="$BASE_NAME" \
        --parameters mysqlAdminUsername="crmadmin" \
        --parameters mysqlAdminPassword="$MYSQL_PASSWORD" \
        --parameters jwtSecret="$JWT_SECRET" \
        --query properties.outputs -o json)
    
    print_msg $GREEN "✓ Infrastructure deployed successfully"
    
    # Extract outputs
    ACR_SERVER=$(echo $DEPLOYMENT_OUTPUT | jq -r '.acrLoginServer.value')
    API_URL=$(echo $DEPLOYMENT_OUTPUT | jq -r '.apiUrl.value')
    FRONTEND_URL=$(echo $DEPLOYMENT_OUTPUT | jq -r '.frontendUrl.value')
    MYSQL_SERVER=$(echo $DEPLOYMENT_OUTPUT | jq -r '.mysqlServer.value')
    
    print_msg $BLUE "\nDeployment Outputs:"
    print_msg $BLUE "  ACR Server:   $ACR_SERVER"
    print_msg $BLUE "  API URL:      $API_URL"
    print_msg $BLUE "  Frontend URL: $FRONTEND_URL"
    print_msg $BLUE "  MySQL Server: $MYSQL_SERVER"
fi

# =============================================================================
# STEP 3: Build and Push Docker Images
# =============================================================================
if $DEPLOY_APP; then
    print_msg $YELLOW "\n[3/4] Building and pushing Docker images..."
    
    # Get ACR credentials
    ACR_NAME="${BASE_NAME}acr${ENVIRONMENT//[-]/}"
    
    print_msg $YELLOW "Logging into Azure Container Registry..."
    az acr login --name "$ACR_NAME"
    
    ACR_SERVER=$(az acr show --name "$ACR_NAME" --query loginServer -o tsv)
    
    # Build and push backend
    print_msg $YELLOW "Building backend image..."
    docker build -t "$ACR_SERVER/crm-api:latest" -f docker/Dockerfile.backend .
    docker push "$ACR_SERVER/crm-api:latest"
    print_msg $GREEN "✓ Backend image pushed"
    
    # Build and push frontend
    print_msg $YELLOW "Building frontend image..."
    docker build -t "$ACR_SERVER/crm-frontend:latest" -f docker/Dockerfile.frontend .
    docker push "$ACR_SERVER/crm-frontend:latest"
    print_msg $GREEN "✓ Frontend image pushed"
    
    # Restart App Services to pull new images
    print_msg $YELLOW "Restarting App Services..."
    az webapp restart --name "api-${BASE_NAME}-${ENVIRONMENT}" --resource-group "$RESOURCE_GROUP-$ENVIRONMENT"
    az webapp restart --name "web-${BASE_NAME}-${ENVIRONMENT}" --resource-group "$RESOURCE_GROUP-$ENVIRONMENT"
    print_msg $GREEN "✓ App Services restarted"
fi

# =============================================================================
# STEP 4: Database Migration
# =============================================================================
if $DEPLOY_DB; then
    print_msg $YELLOW "\n[4/4] Running database migrations..."
    
    # Get MySQL connection info
    MYSQL_SERVER="${BASE_NAME}-mysql-${ENVIRONMENT}.mysql.database.azure.com"
    
    print_msg $YELLOW "To run database migrations, execute:"
    print_msg $BLUE "mysql -h $MYSQL_SERVER -u crmadmin -p crm_db < database/schema/000_baseline_schema.sql"
    
    print_msg $GREEN "✓ Database migration instructions provided"
fi

# =============================================================================
# Deployment Complete
# =============================================================================
print_msg $GREEN "\n╔══════════════════════════════════════════════════════════════╗"
print_msg $GREEN "║           Deployment Complete!                               ║"
print_msg $GREEN "╚══════════════════════════════════════════════════════════════╝"

if $DEPLOY_INFRA; then
    print_msg $BLUE "\nAccess your application:"
    print_msg $BLUE "  Frontend: $FRONTEND_URL"
    print_msg $BLUE "  API:      $API_URL"
    print_msg $BLUE "  Swagger:  $API_URL/swagger"
fi

print_msg $YELLOW "\nNext Steps:"
print_msg $YELLOW "1. Configure Azure DevOps pipeline with the service connection"
print_msg $YELLOW "2. Add secrets to Azure DevOps variable group"
print_msg $YELLOW "3. Run the database migration script"
print_msg $YELLOW "4. Configure custom domain and SSL certificate"
