// =============================================================================
// CRM Solution - Azure Infrastructure (Minimal - No LLM VM)
// Version: 2.0.1
// Description: Minimal Azure infrastructure for CRM Solution
//              - AKS for container orchestration (single node)
//              - Azure MySQL Flexible Server
//              - Designed for subscriptions with limited vCPU quota (4 vCPUs)
// =============================================================================

@description('Environment name (dev, staging, prod)')
@allowed(['dev', 'staging', 'prod'])
param environment string = 'dev'

@description('Azure region for all resources')
param location string = resourceGroup().location

@description('Base name for all resources')
param baseName string = 'crm'

@description('MySQL admin username')
@secure()
param mysqlAdminUsername string

@description('MySQL admin password')
@secure()
param mysqlAdminPassword string

@description('JWT Secret for API authentication')
@secure()
param jwtSecret string

// Resource naming
var resourceSuffix = '${baseName}-${environment}'
var acrName = replace('${baseName}acr${environment}', '-', '')
var keyVaultName = 'kv-${resourceSuffix}'
var appInsightsName = 'ai-${resourceSuffix}'
var logAnalyticsName = 'log-${resourceSuffix}'
var aksName = 'aks-${resourceSuffix}'
var mysqlServerName = 'mysql-${resourceSuffix}'
var mysqlDbName = 'crm_db'
var vnetName = 'vnet-${resourceSuffix}'

// VM sizes - minimal for limited quota (total 4 vCPUs available)
var aksNodeSize = 'Standard_D2s_v3' // 2 vCPUs per node - available in eastus2
var aksNodeCount = 1 // 1 node × 2 vCPUs = 2 vCPUs - leaving room for quota limits

// =============================================================================
// Virtual Network
// =============================================================================
resource vnet 'Microsoft.Network/virtualNetworks@2023-05-01' = {
  name: vnetName
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: ['10.0.0.0/16']
    }
    subnets: [
      {
        name: 'aks-subnet'
        properties: {
          addressPrefix: '10.0.1.0/24'
          serviceEndpoints: [
            { service: 'Microsoft.Sql' }
          ]
        }
      }
    ]
  }
}

// =============================================================================
// Logging and Monitoring
// =============================================================================
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

// =============================================================================
// Container Registry
// =============================================================================
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: acrName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

// =============================================================================
// Key Vault
// =============================================================================
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enabledForDeployment: true
    enabledForTemplateDeployment: true
    enabledForDiskEncryption: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    accessPolicies: []
  }
}

// Store secrets in Key Vault
resource secretMysqlPassword 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'mysql-admin-password'
  properties: {
    value: mysqlAdminPassword
  }
}

resource secretJwt 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'jwt-secret'
  properties: {
    value: jwtSecret
  }
}

// =============================================================================
// Azure Kubernetes Service (AKS)
// =============================================================================
resource aksCluster 'Microsoft.ContainerService/managedClusters@2024-01-01' = {
  name: aksName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    dnsPrefix: '${aksName}-dns'
    kubernetesVersion: '1.33'
    agentPoolProfiles: [
      {
        name: 'system'
        count: aksNodeCount
        vmSize: aksNodeSize
        mode: 'System'
        osType: 'Linux'
        osSKU: 'Ubuntu'
        vnetSubnetID: vnet.properties.subnets[0].id
        enableAutoScaling: false
      }
    ]
    networkProfile: {
      networkPlugin: 'azure'
      networkPolicy: 'azure'
      serviceCidr: '10.1.0.0/16'
      dnsServiceIP: '10.1.0.10'
    }
    addonProfiles: {}
  }
}

// Grant AKS access to ACR
resource aksAcrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistry.id, aksCluster.id, 'acrpull')
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d') // AcrPull
    principalId: aksCluster.properties.identityProfile.kubeletidentity.objectId
    principalType: 'ServicePrincipal'
  }
}

// =============================================================================
// Azure MySQL Flexible Server
// =============================================================================
resource mysqlServer 'Microsoft.DBforMySQL/flexibleServers@2023-06-30' = {
  name: mysqlServerName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '8.0.21'
    administratorLogin: mysqlAdminUsername
    administratorLoginPassword: mysqlAdminPassword
    storage: {
      storageSizeGB: 20
      autoGrow: 'Enabled'
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    // Public access mode - no VNet integration
  }
}

resource mysqlDatabase 'Microsoft.DBforMySQL/flexibleServers/databases@2023-06-30' = {
  parent: mysqlServer
  name: mysqlDbName
  properties: {
    charset: 'utf8mb4'
    collation: 'utf8mb4_unicode_ci'
  }
}

// Allow Azure services to access MySQL
resource mysqlFirewall 'Microsoft.DBforMySQL/flexibleServers/firewallRules@2023-06-30' = {
  parent: mysqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// =============================================================================
// Outputs
// =============================================================================
output acrLoginServer string = containerRegistry.properties.loginServer
output aksName string = aksCluster.name
output mysqlServerFqdn string = mysqlServer.properties.fullyQualifiedDomainName
output mysqlDatabaseName string = mysqlDbName
output keyVaultName string = keyVault.name
output appInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey
output resourceGroupName string = resourceGroup().name
output subscriptionId string = subscription().subscriptionId
