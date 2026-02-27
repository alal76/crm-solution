#!/bin/bash
# =============================================================================
# CRM Solution - Azure AKS Deployment Script
# Version: 2.0.0
# Description: Deploy CRM Solution to Azure with AKS, MySQL, and LLM VM
# =============================================================================

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

# Default values
ENVIRONMENT="dev"
LOCATION="eastus"
RESOURCE_GROUP="crm-solution-rg"
BASE_NAME="crm"
SSH_KEY_PATH="$HOME/.ssh/id_rsa.pub"

print_msg() { echo -e "${1}${2}${NC}"; }
print_header() {
    echo ""
    print_msg $CYAN "╔════════════════════════════════════════════════════════════════╗"
    print_msg $CYAN "║  $1"
    print_msg $CYAN "╚════════════════════════════════════════════════════════════════╝"
}

show_usage() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  -e, --environment    Environment (dev, staging, prod) [default: dev]"
    echo "  -l, --location       Azure region [default: eastus]"
    echo "  -g, --resource-group Resource group base name [default: crm-solution-rg]"
    echo "  --ssh-key            Path to SSH public key [default: ~/.ssh/id_rsa.pub]"
    echo "  --infrastructure     Deploy infrastructure only"
    echo "  --app                Deploy application only"
    echo "  --database           Initialize database only"
    echo "  -h, --help           Show this help"
}

# Parse arguments
DEPLOY_INFRA=true
DEPLOY_APP=true
DEPLOY_DB=true

while [[ $# -gt 0 ]]; do
    case $1 in
        -e|--environment) ENVIRONMENT="$2"; shift 2 ;;
        -l|--location) LOCATION="$2"; shift 2 ;;
        -g|--resource-group) RESOURCE_GROUP="$2"; shift 2 ;;
        --ssh-key) SSH_KEY_PATH="$2"; shift 2 ;;
        --infrastructure) DEPLOY_APP=false; DEPLOY_DB=false; shift ;;
        --app) DEPLOY_INFRA=false; DEPLOY_DB=false; shift ;;
        --database) DEPLOY_INFRA=false; DEPLOY_APP=false; shift ;;
        -h|--help) show_usage; exit 0 ;;
        *) print_msg $RED "Unknown option: $1"; show_usage; exit 1 ;;
    esac
done

RG_NAME="${RESOURCE_GROUP}-${ENVIRONMENT}"
ACR_NAME="${BASE_NAME}acr${ENVIRONMENT}"
AKS_NAME="aks-${BASE_NAME}-${ENVIRONMENT}"

print_header "CRM Solution - Azure AKS Deployment"
print_msg $BLUE "Environment:     $ENVIRONMENT"
print_msg $BLUE "Location:        $LOCATION"
print_msg $BLUE "Resource Group:  $RG_NAME"
print_msg $BLUE "AKS Cluster:     $AKS_NAME"
echo ""

# Check prerequisites
command -v az >/dev/null 2>&1 || { print_msg $RED "Azure CLI not installed"; exit 1; }
command -v kubectl >/dev/null 2>&1 || { print_msg $RED "kubectl not installed"; exit 1; }
command -v docker >/dev/null 2>&1 || { print_msg $RED "Docker not installed"; exit 1; }

# Check Azure login
if ! az account show &> /dev/null; then
    print_msg $YELLOW "Please log in to Azure..."
    az login
fi

SUBSCRIPTION=$(az account show --query name -o tsv)
print_msg $GREEN "✓ Logged in: $SUBSCRIPTION"

# =============================================================================
# STEP 1: Create Resource Group
# =============================================================================
if $DEPLOY_INFRA; then
    print_header "Step 1: Create Resource Group"
    az group create --name "$RG_NAME" --location "$LOCATION" --output none
    print_msg $GREEN "✓ Resource group: $RG_NAME"

    # =============================================================================
    # STEP 2: Generate SSH Key if needed
    # =============================================================================
    print_header "Step 2: Check SSH Key"
    if [[ ! -f "$SSH_KEY_PATH" ]]; then
        print_msg $YELLOW "Generating SSH key..."
        ssh-keygen -t rsa -b 4096 -f "${SSH_KEY_PATH%.pub}" -N ""
    fi
    SSH_PUBLIC_KEY=$(cat "$SSH_KEY_PATH")
    print_msg $GREEN "✓ SSH key ready"

    # =============================================================================
    # STEP 3: Generate Secrets
    # =============================================================================
    print_header "Step 3: Generate Secrets"
    JWT_SECRET=$(openssl rand -base64 64 | tr -d '\n')
    MYSQL_PASSWORD="CRM${ENVIRONMENT^}$(openssl rand -base64 12 | tr -d '/+=' | head -c 12)!"
    print_msg $GREEN "✓ Secrets generated"

    # =============================================================================
    # STEP 4: Deploy Infrastructure with Bicep
    # =============================================================================
    print_header "Step 4: Deploy Infrastructure (this takes 10-15 minutes)"
    
    DEPLOYMENT_OUTPUT=$(az deployment group create \
        --name "crm-aks-deployment" \
        --resource-group "$RG_NAME" \
        --template-file azure/main-aks.bicep \
        --parameters environment="$ENVIRONMENT" \
        --parameters location="$LOCATION" \
        --parameters baseName="$BASE_NAME" \
        --parameters mysqlAdminUsername="crmadmin" \
        --parameters mysqlAdminPassword="$MYSQL_PASSWORD" \
        --parameters jwtSecret="$JWT_SECRET" \
        --parameters sshPublicKey="$SSH_PUBLIC_KEY" \
        --query properties.outputs -o json)
    
    # Extract outputs
    ACR_SERVER=$(echo $DEPLOYMENT_OUTPUT | jq -r '.acrLoginServer.value')
    AKS_FQDN=$(echo $DEPLOYMENT_OUTPUT | jq -r '.aksFqdn.value')
    MYSQL_SERVER=$(echo $DEPLOYMENT_OUTPUT | jq -r '.mysqlServer.value')
    LLM_VM_IP=$(echo $DEPLOYMENT_OUTPUT | jq -r '.llmVmPublicIp.value')
    LLM_VM_FQDN=$(echo $DEPLOYMENT_OUTPUT | jq -r '.llmVmFqdn.value')
    
    print_msg $GREEN "✓ Infrastructure deployed"
    print_msg $BLUE "  ACR: $ACR_SERVER"
    print_msg $BLUE "  AKS: $AKS_FQDN"
    print_msg $BLUE "  MySQL: $MYSQL_SERVER"
    print_msg $BLUE "  LLM VM: $LLM_VM_IP"
    
    # Save deployment info
    cat > .azure-deployment-$ENVIRONMENT.json << EOF
{
    "environment": "$ENVIRONMENT",
    "resourceGroup": "$RG_NAME",
    "acrServer": "$ACR_SERVER",
    "aksName": "$AKS_NAME",
    "aksFqdn": "$AKS_FQDN",
    "mysqlServer": "$MYSQL_SERVER",
    "mysqlPassword": "$MYSQL_PASSWORD",
    "llmVmIp": "$LLM_VM_IP",
    "llmVmFqdn": "$LLM_VM_FQDN",
    "jwtSecret": "$JWT_SECRET"
}
EOF
    chmod 600 .azure-deployment-$ENVIRONMENT.json
    print_msg $GREEN "✓ Deployment info saved to .azure-deployment-$ENVIRONMENT.json"
fi

# =============================================================================
# STEP 5: Build and Push Docker Images
# =============================================================================
if $DEPLOY_APP; then
    print_header "Step 5: Build and Push Docker Images"
    
    # Load deployment info if not from infra step
    if [[ -f ".azure-deployment-$ENVIRONMENT.json" ]]; then
        ACR_SERVER=$(jq -r '.acrServer' .azure-deployment-$ENVIRONMENT.json)
        LLM_VM_IP=$(jq -r '.llmVmIp' .azure-deployment-$ENVIRONMENT.json)
        MYSQL_SERVER=$(jq -r '.mysqlServer' .azure-deployment-$ENVIRONMENT.json)
        MYSQL_PASSWORD=$(jq -r '.mysqlPassword' .azure-deployment-$ENVIRONMENT.json)
        JWT_SECRET=$(jq -r '.jwtSecret' .azure-deployment-$ENVIRONMENT.json)
    fi
    
    # Login to ACR
    print_msg $YELLOW "Logging into ACR..."
    az acr login --name "$ACR_NAME"
    
    # Build and push backend
    print_msg $YELLOW "Building backend image..."
    docker build -t "$ACR_SERVER/crm-api:latest" -t "$ACR_SERVER/crm-api:$ENVIRONMENT" -f docker/Dockerfile.backend .
    docker push "$ACR_SERVER/crm-api:latest"
    docker push "$ACR_SERVER/crm-api:$ENVIRONMENT"
    print_msg $GREEN "✓ Backend image pushed"
    
    # Build and push frontend
    print_msg $YELLOW "Building frontend image..."
    docker build -t "$ACR_SERVER/crm-frontend:latest" -t "$ACR_SERVER/crm-frontend:$ENVIRONMENT" -f docker/Dockerfile.frontend .
    docker push "$ACR_SERVER/crm-frontend:latest"
    docker push "$ACR_SERVER/crm-frontend:$ENVIRONMENT"
    print_msg $GREEN "✓ Frontend image pushed"

    # =============================================================================
    # STEP 6: Deploy to AKS
    # =============================================================================
    print_header "Step 6: Deploy to AKS"
    
    # Get AKS credentials
    print_msg $YELLOW "Getting AKS credentials..."
    az aks get-credentials --resource-group "$RG_NAME" --name "$AKS_NAME" --overwrite-existing
    
    # Create namespace
    kubectl apply -f azure/k8s/01-namespace-config.yaml
    
    # Create ACR pull secret
    ACR_PASSWORD=$(az acr credential show --name "$ACR_NAME" --query "passwords[0].value" -o tsv)
    kubectl create secret docker-registry acr-secret \
        --namespace crm \
        --docker-server="$ACR_SERVER" \
        --docker-username="$ACR_NAME" \
        --docker-password="$ACR_PASSWORD" \
        --dry-run=client -o yaml | kubectl apply -f -
    
    # Create secrets
    CONNECTION_STRING="Server=$MYSQL_SERVER;Database=crm_db;User=crmadmin;Password=$MYSQL_PASSWORD;SslMode=Required;"
    kubectl create secret generic crm-secrets \
        --namespace crm \
        --from-literal="ConnectionStrings__DefaultConnection=$CONNECTION_STRING" \
        --from-literal="Jwt__Secret=$JWT_SECRET" \
        --from-literal="Jwt__Issuer=CRM-Solution" \
        --from-literal="Jwt__Audience=CRM-Users" \
        --from-literal="Jwt__ExpirationMinutes=60" \
        --dry-run=client -o yaml | kubectl apply -f -
    
    # Deploy with variable substitution
    export ACR_LOGIN_SERVER=$ACR_SERVER
    export IMAGE_TAG=$ENVIRONMENT
    export LLM_VM_IP=$LLM_VM_IP
    export API_HOSTNAME="api-crm-${ENVIRONMENT}.${LOCATION}.cloudapp.azure.com"
    export DOMAIN_NAME="crm-${ENVIRONMENT}.${LOCATION}.cloudapp.azure.com"
    
    envsubst < azure/k8s/02-backend-deployment.yaml | kubectl apply -f -
    envsubst < azure/k8s/03-frontend-deployment.yaml | kubectl apply -f -
    envsubst < azure/k8s/04-ingress.yaml | kubectl apply -f -
    
    if [[ "$ENVIRONMENT" == "prod" ]]; then
        kubectl apply -f azure/k8s/05-autoscaling.yaml
    fi
    
    print_msg $GREEN "✓ Application deployed to AKS"
    
    # Wait for deployments
    print_msg $YELLOW "Waiting for deployments to be ready..."
    kubectl rollout status deployment/crm-api -n crm --timeout=300s
    kubectl rollout status deployment/crm-frontend -n crm --timeout=300s
    print_msg $GREEN "✓ All deployments ready"
fi

# =============================================================================
# STEP 7: Initialize Database
# =============================================================================
if $DEPLOY_DB; then
    print_header "Step 7: Database Initialization"
    
    if [[ -f ".azure-deployment-$ENVIRONMENT.json" ]]; then
        MYSQL_SERVER=$(jq -r '.mysqlServer' .azure-deployment-$ENVIRONMENT.json)
        MYSQL_PASSWORD=$(jq -r '.mysqlPassword' .azure-deployment-$ENVIRONMENT.json)
    fi
    
    print_msg $YELLOW "To initialize the database, run:"
    print_msg $BLUE "mysql -h $MYSQL_SERVER -u crmadmin -p'$MYSQL_PASSWORD' crm_db < database/schema/000_baseline_schema.sql"
    print_msg $YELLOW ""
    print_msg $YELLOW "Or use Azure Cloud Shell / MySQL Workbench to connect and run the schema."
fi

# =============================================================================
# Summary
# =============================================================================
print_header "Deployment Complete!"

if [[ -f ".azure-deployment-$ENVIRONMENT.json" ]]; then
    print_msg $GREEN "Access URLs:"
    
    # Get LoadBalancer IPs
    API_IP=$(kubectl get svc crm-api-lb -n crm -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "pending")
    FRONTEND_IP=$(kubectl get svc crm-frontend-lb -n crm -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "pending")
    
    print_msg $BLUE "  Frontend: http://$FRONTEND_IP"
    print_msg $BLUE "  API:      http://$API_IP"
    print_msg $BLUE "  Swagger:  http://$API_IP/swagger"
    print_msg $BLUE "  LLM VM:   http://$LLM_VM_IP:11434"
    print_msg $BLUE ""
    print_msg $YELLOW "Note: LoadBalancer IPs may take a few minutes to provision."
    print_msg $YELLOW "Run: kubectl get svc -n crm"
fi

print_msg $CYAN ""
print_msg $CYAN "Next Steps:"
print_msg $CYAN "1. Initialize the database with the schema"
print_msg $CYAN "2. SSH to LLM VM and verify Ollama: ssh azureuser@$LLM_VM_IP"
print_msg $CYAN "3. Configure Azure DevOps pipeline"
print_msg $CYAN "4. Set up custom domain and SSL"
