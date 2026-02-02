# CRM Solution - Azure Deployment Guide

## Overview

This guide explains how to deploy the CRM Solution to Microsoft Azure using Azure DevOps for CI/CD.

## Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        Azure Cloud                                       │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │                    Resource Group                                │    │
│  │                                                                  │    │
│  │  ┌──────────────┐    ┌──────────────┐    ┌──────────────────┐   │    │
│  │  │   Azure      │    │   App        │    │   Azure Database │   │    │
│  │  │   Container  │───▶│   Service    │───▶│   for MySQL      │   │    │
│  │  │   Registry   │    │   (Backend)  │    │   Flexible       │   │    │
│  │  └──────────────┘    └──────────────┘    └──────────────────┘   │    │
│  │         │                                                        │    │
│  │         │            ┌──────────────┐    ┌──────────────────┐   │    │
│  │         └───────────▶│   App        │    │   Azure Key      │   │    │
│  │                      │   Service    │    │   Vault          │   │    │
│  │                      │   (Frontend) │    │   (Secrets)      │   │    │
│  │                      └──────────────┘    └──────────────────┘   │    │
│  │                                                                  │    │
│  │  ┌──────────────┐    ┌──────────────┐                           │    │
│  │  │   App        │    │   Log        │                           │    │
│  │  │   Insights   │    │   Analytics  │                           │    │
│  │  │   (APM)      │    │   Workspace  │                           │    │
│  │  └──────────────┘    └──────────────┘                           │    │
│  └─────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
```

## Prerequisites

1. **Azure Subscription** - Active Azure subscription
2. **Azure CLI** - Version 2.50+ installed locally
3. **Azure DevOps Account** - With permissions to create pipelines
4. **Docker** - For building container images locally
5. **Git** - For repository management

## Quick Start

### Step 1: Azure CLI Login

```bash
# Login to Azure
az login

# Set subscription (if you have multiple)
az account set --subscription "Your Subscription Name"
```

### Step 2: Deploy Infrastructure

```bash
# Make script executable
chmod +x azure/deploy.sh

# Deploy to dev environment
./azure/deploy.sh -e dev

# Deploy to production
./azure/deploy.sh -e prod
```

### Step 3: Configure Azure DevOps

1. **Create a new project** in Azure DevOps
2. **Import the repository** or connect your Git repo
3. **Create Service Connection**:
   - Go to Project Settings → Service connections
   - New service connection → Azure Resource Manager
   - Select your subscription and resource group
   - Name it: `Azure-CRM-ServiceConnection`

4. **Create Variable Groups**:
   - Go to Pipelines → Library
   - Create variable group: `crm-secrets`
   - Add variables:
     - `MYSQL_PASSWORD` (secret)
     - `JWT_SECRET` (secret)
     - `API_URL`

5. **Create Pipeline**:
   - Go to Pipelines → New Pipeline
   - Select your repository
   - Select "Existing Azure Pipelines YAML file"
   - Choose `azure-pipelines.yml`

## Azure Resources Created

| Resource | Purpose | SKU (Dev) | SKU (Prod) |
|----------|---------|-----------|------------|
| Container Registry | Docker images | Basic | Standard |
| App Service Plan | Compute | B1 | P1v3 |
| App Service (API) | Backend | Basic | Premium |
| App Service (Web) | Frontend | Basic | Premium |
| MySQL Flexible | Database | B1ms | D2ds_v4 |
| Key Vault | Secrets | Standard | Standard |
| Application Insights | Monitoring | - | - |
| Log Analytics | Logging | PerGB | PerGB |

## Environment Configuration

### Development
- URL: `https://api-crm-dev.azurewebsites.net`
- Auto-deploy on push to `dev` branch
- Basic tier resources

### Staging
- URL: `https://api-crm-staging.azurewebsites.net`
- Deploy after successful dev
- Manual approval gate

### Production
- URL: `https://api-crm-prod.azurewebsites.net`
- Deploy after staging approval
- Premium tier resources
- Zone-redundant database

## CI/CD Pipeline Stages

```
┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐
│  Build  │───▶│  Test   │───▶│ Docker  │───▶│ Deploy  │
│  & Test │    │ Results │    │  Push   │    │ Staging │
└─────────┘    └─────────┘    └─────────┘    └────┬────┘
                                                   │
                                                   ▼
                                             ┌─────────┐
                                             │ Deploy  │
                                             │  Prod   │
                                             └─────────┘
```

## Database Migration

### Initial Setup
```bash
# Connect to Azure MySQL
mysql -h mysql-crm-dev.mysql.database.azure.com \
      -u crmadmin@mysql-crm-dev \
      -p crm_db

# Run baseline schema
source database/schema/000_baseline_schema.sql

# Run seed data
source database/seed/001_color_palettes.sql
source database/seed/002_module_ui_configs.sql
source database/seed/003_system_settings.sql
source database/seed/004_service_request_types.sql
```

### Using Azure Cloud Shell
```bash
# From Azure Portal, open Cloud Shell
az mysql flexible-server execute \
    --name mysql-crm-dev \
    --resource-group crm-solution-rg-dev \
    --admin-user crmadmin \
    --admin-password <password> \
    --file-path database/schema/000_baseline_schema.sql
```

## Monitoring & Logging

### Application Insights
- Real-time metrics
- Request tracing
- Exception tracking
- Custom events

### Log Analytics Queries
```kusto
// API Errors in last 24 hours
exceptions
| where timestamp > ago(24h)
| summarize count() by problemId
| order by count_ desc

// Request performance
requests
| where timestamp > ago(1h)
| summarize avg(duration) by bin(timestamp, 5m)
| render timechart
```

## Security Best Practices

1. **Secrets Management**
   - All secrets stored in Azure Key Vault
   - Managed Identity for App Service access
   - No secrets in source code

2. **Network Security**
   - HTTPS only (enforced)
   - MySQL firewall rules
   - VNet integration (recommended for prod)

3. **Authentication**
   - JWT tokens with secure secrets
   - Token expiration policies
   - CORS configuration

## Cost Estimation

| Environment | Monthly Cost (Est.) |
|-------------|---------------------|
| Development | ~$50-80 |
| Staging | ~$50-80 |
| Production | ~$200-400 |

*Costs vary based on usage and region*

## Troubleshooting

### Common Issues

1. **Deployment fails with permission error**
   - Verify service connection has Contributor role
   - Check resource group permissions

2. **App Service won't start**
   - Check Application Insights for startup errors
   - Verify connection strings in App Settings
   - Check container logs: `az webapp log tail --name <app-name> --resource-group <rg>`

3. **Database connection timeout**
   - Verify firewall rules allow Azure services
   - Check connection string format
   - Ensure SSL mode is enabled

### Useful Commands

```bash
# View App Service logs
az webapp log tail --name api-crm-dev --resource-group crm-solution-rg-dev

# Restart App Service
az webapp restart --name api-crm-dev --resource-group crm-solution-rg-dev

# View container settings
az webapp config container show --name api-crm-dev --resource-group crm-solution-rg-dev

# Scale App Service
az appservice plan update --name asp-crm-dev --resource-group crm-solution-rg-dev --sku P1v2
```

## Next Steps

1. Configure custom domain with SSL
2. Set up Azure Front Door for global load balancing
3. Enable Azure AD authentication
4. Configure backup policies
5. Set up alerts and action groups
