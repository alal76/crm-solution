// =============================================================================
// CRM Solution - Azure Infrastructure with AKS, MySQL, and LLM VM
// Version: 2.0.0
// Description: Complete Azure infrastructure for CRM Solution
//              - AKS for container orchestration
//              - Azure MySQL Flexible Server
//              - GPU VM for LLM (Ollama)
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

@description('SSH public key for LLM VM')
param sshPublicKey string = ''

@description('LLM VM admin username')
param llmVmAdminUsername string = 'azureuser'

// Resource naming
var resourceSuffix = '${baseName}-${environment}'
var acrName = replace('${baseName}acr${environment}', '-', '')
var keyVaultName = 'kv-${resourceSuffix}'
var appInsightsName = 'ai-${resourceSuffix}'
var logAnalyticsName = 'log-${resourceSuffix}'
var aksName = 'aks-${resourceSuffix}'
var mysqlServerName = 'mysql-${resourceSuffix}'
var mysqlDbName = 'crm_db'
var llmVmName = 'vm-llm-${resourceSuffix}'
var vnetName = 'vnet-${resourceSuffix}'

// VM sizes based on environment - using smallest VMs for limited quota subscriptions
var llmVmSize = environment == 'prod' ? 'Standard_NC6s_v3' : 'Standard_B2s' // 2 vCPUs for dev
var aksNodeSize = environment == 'prod' ? 'Standard_D4ds_v5' : 'Standard_B2s' // 2 vCPUs for dev
var aksNodeCount = environment == 'prod' ? 3 : 1 // 1 node for dev to save quota

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
      {
        name: 'llm-subnet'
        properties: {
          addressPrefix: '10.0.2.0/24'
        }
      }
      {
        name: 'mysql-subnet'
        properties: {
          addressPrefix: '10.0.3.0/24'
          delegations: [
            {
              name: 'mysql-delegation'
              properties: {
                serviceName: 'Microsoft.DBforMySQL/flexibleServers'
              }
            }
          ]
        }
      }
    ]
  }
}

// =============================================================================
// Log Analytics Workspace
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

// =============================================================================
// Application Insights
// =============================================================================
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
// Azure Container Registry
// =============================================================================
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: acrName
  location: location
  sku: {
    name: environment == 'prod' ? 'Standard' : 'Basic'
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
    enableRbacAuthorization: true
  }
}

// Store secrets
resource jwtSecretKv 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'JwtSecret'
  properties: {
    value: jwtSecret
  }
}

resource mysqlPasswordKv 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'MySqlPassword'
  properties: {
    value: mysqlAdminPassword
  }
}

// =============================================================================
// Azure Database for MySQL Flexible Server
// =============================================================================
resource mysqlServer 'Microsoft.DBforMySQL/flexibleServers@2023-06-30' = {
  name: mysqlServerName
  location: location
  sku: {
    name: environment == 'prod' ? 'Standard_D4ds_v4' : 'Standard_B1ms'
    tier: environment == 'prod' ? 'GeneralPurpose' : 'Burstable'
  }
  properties: {
    version: '8.0.21'
    administratorLogin: mysqlAdminUsername
    administratorLoginPassword: mysqlAdminPassword
    storage: {
      storageSizeGB: environment == 'prod' ? 128 : 32
      autoGrow: 'Enabled'
    }
    backup: {
      backupRetentionDays: environment == 'prod' ? 35 : 7
      geoRedundantBackup: environment == 'prod' ? 'Enabled' : 'Disabled'
    }
    highAvailability: {
      mode: environment == 'prod' ? 'ZoneRedundant' : 'Disabled'
    }
    network: {
      delegatedSubnetResourceId: vnet.properties.subnets[2].id
    }
  }
}

// MySQL Database
resource mysqlDatabase 'Microsoft.DBforMySQL/flexibleServers/databases@2023-06-30' = {
  parent: mysqlServer
  name: mysqlDbName
  properties: {
    charset: 'utf8mb4'
    collation: 'utf8mb4_unicode_ci'
  }
}

// =============================================================================
// AKS Cluster
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
        enableAutoScaling: environment == 'prod'
        minCount: environment == 'prod' ? 2 : null
        maxCount: environment == 'prod' ? 5 : null
      }
    ]
    networkProfile: {
      networkPlugin: 'azure'
      networkPolicy: 'azure'
      serviceCidr: '10.1.0.0/16'
      dnsServiceIP: '10.1.0.10'
    }
    addonProfiles: {
      omsagent: {
        enabled: true
        config: {
          logAnalyticsWorkspaceResourceID: logAnalytics.id
        }
      }
    }
  }
}

// AKS role assignment for ACR
resource aksAcrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(aksCluster.id, containerRegistry.id, 'AcrPull')
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d') // AcrPull
    principalId: aksCluster.properties.identityProfile.kubeletidentity.objectId
    principalType: 'ServicePrincipal'
  }
}

// =============================================================================
// LLM VM (for Ollama)
// =============================================================================
resource llmNic 'Microsoft.Network/networkInterfaces@2023-05-01' = {
  name: '${llmVmName}-nic'
  location: location
  properties: {
    ipConfigurations: [
      {
        name: 'ipconfig1'
        properties: {
          subnet: {
            id: vnet.properties.subnets[1].id
          }
          privateIPAllocationMethod: 'Dynamic'
          publicIPAddress: {
            id: llmPublicIp.id
          }
        }
      }
    ]
  }
}

resource llmPublicIp 'Microsoft.Network/publicIPAddresses@2023-05-01' = {
  name: '${llmVmName}-pip'
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
    publicIPAllocationMethod: 'Static'
    dnsSettings: {
      domainNameLabel: llmVmName
    }
  }
}

resource llmNsg 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
  name: '${llmVmName}-nsg'
  location: location
  properties: {
    securityRules: [
      {
        name: 'SSH'
        properties: {
          priority: 1000
          protocol: 'Tcp'
          access: 'Allow'
          direction: 'Inbound'
          sourceAddressPrefix: '*'
          sourcePortRange: '*'
          destinationAddressPrefix: '*'
          destinationPortRange: '22'
        }
      }
      {
        name: 'Ollama'
        properties: {
          priority: 1010
          protocol: 'Tcp'
          access: 'Allow'
          direction: 'Inbound'
          sourceAddressPrefix: '10.0.0.0/16'
          sourcePortRange: '*'
          destinationAddressPrefix: '*'
          destinationPortRange: '11434'
        }
      }
    ]
  }
}

resource llmVm 'Microsoft.Compute/virtualMachines@2023-09-01' = {
  name: llmVmName
  location: location
  properties: {
    hardwareProfile: {
      vmSize: llmVmSize
    }
    osProfile: {
      computerName: llmVmName
      adminUsername: llmVmAdminUsername
      linuxConfiguration: {
        disablePasswordAuthentication: true
        ssh: {
          publicKeys: [
            {
              path: '/home/${llmVmAdminUsername}/.ssh/authorized_keys'
              keyData: sshPublicKey != '' ? sshPublicKey : 'ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQC7...' // Placeholder
            }
          ]
        }
      }
    }
    storageProfile: {
      imageReference: {
        publisher: 'Canonical'
        offer: '0001-com-ubuntu-server-jammy'
        sku: '22_04-lts-gen2'
        version: 'latest'
      }
      osDisk: {
        createOption: 'FromImage'
        diskSizeGB: 128
        managedDisk: {
          storageAccountType: 'Premium_LRS'
        }
      }
    }
    networkProfile: {
      networkInterfaces: [
        {
          id: llmNic.id
        }
      ]
    }
  }
}

// LLM VM Extension - Install Ollama
resource llmVmExtension 'Microsoft.Compute/virtualMachines/extensions@2023-09-01' = {
  parent: llmVm
  name: 'install-ollama'
  location: location
  properties: {
    publisher: 'Microsoft.Azure.Extensions'
    type: 'CustomScript'
    typeHandlerVersion: '2.1'
    autoUpgradeMinorVersion: true
    settings: {
      script: base64('''
#!/bin/bash
# Install Ollama
curl -fsSL https://ollama.com/install.sh | sh

# Configure Ollama to listen on all interfaces
mkdir -p /etc/systemd/system/ollama.service.d
cat > /etc/systemd/system/ollama.service.d/override.conf << EOF
[Service]
Environment="OLLAMA_HOST=0.0.0.0"
EOF

systemctl daemon-reload
systemctl enable ollama
systemctl start ollama

# Pull default model
sleep 10
ollama pull llama2
ollama pull olmo2:7b
''')
    }
  }
}

// =============================================================================
// Outputs
// =============================================================================
output acrLoginServer string = containerRegistry.properties.loginServer
output aksName string = aksCluster.name
output aksFqdn string = aksCluster.properties.fqdn
output mysqlServer string = mysqlServer.properties.fullyQualifiedDomainName
output mysqlDatabase string = mysqlDbName
output llmVmPublicIp string = llmPublicIp.properties.ipAddress
output llmVmFqdn string = llmPublicIp.properties.dnsSettings.fqdn
output appInsightsKey string = appInsights.properties.InstrumentationKey
output keyVaultUri string = keyVault.properties.vaultUri
output vnetId string = vnet.id
