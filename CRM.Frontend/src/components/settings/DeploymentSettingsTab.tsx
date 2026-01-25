import { useState, useEffect, useCallback } from 'react';
import {
  Box, Typography, Card, CardContent, Grid, Button, TextField,
  FormControl, InputLabel, Select, MenuItem, Chip, Stepper, Step,
  StepLabel, StepContent, Alert, CircularProgress, Paper, Divider,
  List, ListItem, ListItemIcon, ListItemText, IconButton, Tooltip,
  Dialog, DialogTitle, DialogContent, DialogActions, Switch, FormControlLabel,
  Tabs, Tab, Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  LinearProgress, Accordion, AccordionSummary, AccordionDetails, InputAdornment,
  Badge
} from '@mui/material';
import {
  Cloud as CloudIcon, Storage as StorageIcon, Api as ApiIcon,
  Web as WebIcon, CheckCircle as CheckIcon, Error as ErrorIcon,
  PlayArrow as PlayIcon, Settings as SettingsIcon, Refresh as RefreshIcon,
  CloudQueue as AzureIcon, Code as AwsIcon, Computer as GoogleIcon,
  Dns as KubernetesIcon, Storage as DatabaseIcon, Security as SecurityIcon,
  ArrowForward as ArrowIcon, ContentCopy as CopyIcon, Download as DownloadIcon,
  Upload as UploadIcon, Sync as SyncIcon, ExpandMore as ExpandMoreIcon,
  Visibility as VisibilityIcon, VisibilityOff as VisibilityOffIcon,
  Terminal as TerminalIcon, Description as ScriptIcon, DataObject as DataIcon,
  CheckCircleOutline as ValidatedIcon, WarningAmber as WarningIcon,
  Info as InfoIcon, Delete as DeleteIcon, Add as AddIcon,
  History as HistoryIcon, HealthAndSafety as HealthIcon, Dashboard as DashboardIcon,
  Stop as StopIcon, Replay as RestartIcon, Tune as ScaleIcon
} from '@mui/icons-material';
import cloudDeploymentService, {
  CloudProvider as ApiCloudProvider,
  CloudDeployment as ApiCloudDeployment,
  DeploymentAttempt,
  HealthCheck,
  DeploymentDashboard,
  HealthCheckResult
} from '../../services/cloudDeploymentService';

// Types for hosting status
interface CurrentHosting {
  environment: 'docker' | 'kubernetes' | 'paas' | 'vm' | 'local';
  provider: 'azure' | 'aws' | 'gcp' | 'onprem' | 'local';
  status: 'healthy' | 'degraded' | 'offline' | 'unknown';
  services: {
    name: string;
    status: 'running' | 'stopped' | 'error';
    url?: string;
    version?: string;
  }[];
  database: {
    type: string;
    host: string;
    status: 'connected' | 'disconnected';
    size?: string;
  };
  lastCheck: string;
}

// Types for provider credentials
interface ProviderCredentials {
  azure?: {
    subscriptionId: string;
    tenantId: string;
    clientId: string;
    clientSecret: string;
    resourceGroup: string;
  };
  aws?: {
    accessKeyId: string;
    secretAccessKey: string;
    region: string;
    accountId: string;
  };
  gcp?: {
    projectId: string;
    serviceAccountKey: string;
    region: string;
  };
  onprem?: {
    serverHost: string;
    sshUser: string;
    sshKeyPath: string;
    dockerHost?: string;
  };
}

// Types for data replication
interface ReplicationConfig {
  replicateDatabase: boolean;
  replicateSettings: boolean;
  replicateUsers: boolean;
  replicateCustomizations: boolean;
  fullBackup: boolean;
  incrementalSync: boolean;
}

interface DeploymentTarget {
  id: string;
  name: string;
  provider: 'azure' | 'aws' | 'gcp' | 'onprem';
  type: 'container' | 'kubernetes' | 'paas' | 'vm';
  status: 'configured' | 'not_configured' | 'deploying' | 'deployed' | 'error';
  endpoint?: string;
  lastDeployed?: string;
}

interface DeploymentConfig {
  provider: string;
  region: string;
  databaseType: string;
  databaseTier: string;
  apiReplicas: number;
  frontendReplicas: number;
  useKubernetes: boolean;
  kubernetesNamespace: string;
  enableSsl: boolean;
  enableAutoScaling: boolean;
  minReplicas: number;
  maxReplicas: number;
}

const CLOUD_PROVIDERS = [
  { id: 'azure', name: 'Microsoft Azure', icon: <AzureIcon sx={{ color: '#0078d4' }} />, color: '#0078d4' },
  { id: 'aws', name: 'Amazon Web Services', icon: <AwsIcon sx={{ color: '#ff9900' }} />, color: '#ff9900' },
  { id: 'gcp', name: 'Google Cloud Platform', icon: <GoogleIcon sx={{ color: '#4285f4' }} />, color: '#4285f4' },
  { id: 'onprem', name: 'On-Premises / Self-Hosted', icon: <StorageIcon sx={{ color: '#6750A4' }} />, color: '#6750A4' },
];

const AZURE_REGIONS = [
  'East US', 'West US', 'Central US', 'North Europe', 'West Europe', 
  'Southeast Asia', 'Australia East', 'UK South', 'Japan East'
];

const AWS_REGIONS = [
  'us-east-1', 'us-west-2', 'eu-west-1', 'eu-central-1', 
  'ap-southeast-1', 'ap-northeast-1', 'ap-south-1'
];

const GCP_REGIONS = [
  'us-central1', 'us-east1', 'europe-west1', 'asia-east1', 
  'australia-southeast1', 'southamerica-east1'
];

interface DatabaseOption {
  id: string;
  name: string;
  description?: string;
  tier?: string;
}

const DATABASE_OPTIONS: DatabaseOption[] = [
  { id: 'mariadb', name: 'MariaDB', description: 'Recommended - Full compatibility' },
  { id: 'mysql', name: 'MySQL', description: 'Compatible with most features' },
  { id: 'postgres', name: 'PostgreSQL', description: 'Enterprise-grade database' },
  { id: 'sqlserver', name: 'SQL Server', description: 'Microsoft SQL Server' },
  { id: 'sqlite', name: 'SQLite', description: 'Lightweight, development only' },
];

const PAAS_DATABASE_OPTIONS: Record<string, DatabaseOption[]> = {
  azure: [
    { id: 'azure-mysql', name: 'Azure Database for MySQL', tier: 'PaaS' },
    { id: 'azure-mariadb', name: 'Azure Database for MariaDB', tier: 'PaaS' },
    { id: 'azure-postgres', name: 'Azure Database for PostgreSQL', tier: 'PaaS' },
    { id: 'azure-sql', name: 'Azure SQL Database', tier: 'PaaS' },
  ],
  aws: [
    { id: 'aws-rds-mysql', name: 'Amazon RDS for MySQL', tier: 'PaaS' },
    { id: 'aws-rds-mariadb', name: 'Amazon RDS for MariaDB', tier: 'PaaS' },
    { id: 'aws-rds-postgres', name: 'Amazon RDS for PostgreSQL', tier: 'PaaS' },
    { id: 'aws-aurora', name: 'Amazon Aurora', tier: 'PaaS' },
  ],
  gcp: [
    { id: 'gcp-cloudsql-mysql', name: 'Cloud SQL for MySQL', tier: 'PaaS' },
    { id: 'gcp-cloudsql-postgres', name: 'Cloud SQL for PostgreSQL', tier: 'PaaS' },
    { id: 'gcp-spanner', name: 'Cloud Spanner', tier: 'PaaS' },
  ],
  onprem: DATABASE_OPTIONS,
};

function DeploymentSettingsTab() {
  const [activeStep, setActiveStep] = useState(0);
  const [selectedProvider, setSelectedProvider] = useState<string>('');
  const [deploymentConfig, setDeploymentConfig] = useState<DeploymentConfig>({
    provider: '',
    region: '',
    databaseType: '',
    databaseTier: 'basic',
    apiReplicas: 2,
    frontendReplicas: 2,
    useKubernetes: false,
    kubernetesNamespace: 'crm-system',
    enableSsl: true,
    enableAutoScaling: false,
    minReplicas: 1,
    maxReplicas: 5,
  });
  const [deploying, setDeploying] = useState(false);
  const [deploymentStatus, setDeploymentStatus] = useState<string | null>(null);
  const [deploymentLogs, setDeploymentLogs] = useState<string[]>([]);
  const [showLogsDialog, setShowLogsDialog] = useState(false);
  const [targets, setTargets] = useState<DeploymentTarget[]>([
    { id: '1', name: 'Production Database', provider: 'azure', type: 'paas', status: 'not_configured' },
    { id: '2', name: 'API Layer', provider: 'azure', type: 'container', status: 'not_configured' },
    { id: '3', name: 'Frontend', provider: 'azure', type: 'container', status: 'not_configured' },
  ]);

  // New state for current hosting, credentials, scripts, and replication
  const [mainTab, setMainTab] = useState(0);
  const [loadingHostingStatus, setLoadingHostingStatus] = useState(false);
  const [currentHosting, setCurrentHosting] = useState<CurrentHosting>({
    environment: 'docker',
    provider: 'local',
    status: 'healthy',
    services: [
      { name: 'CRM API', status: 'running', url: 'http://localhost:5000', version: '1.0.0' },
      { name: 'CRM Frontend', status: 'running', url: 'http://localhost:3000', version: '1.0.0' },
      { name: 'Background Services', status: 'running', version: '1.0.0' },
    ],
    database: {
      type: 'MariaDB',
      host: 'localhost:3306',
      status: 'connected',
      size: 'Loading...',
    },
    lastCheck: new Date().toISOString(),
  });

  // Fetch actual database status on mount
  useEffect(() => {
    const fetchDatabaseStatus = async () => {
      setLoadingHostingStatus(true);
      try {
        const token = localStorage.getItem('token');
        const response = await fetch('/api/database/status', {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
          },
        });
        if (response.ok) {
          const data = await response.json();
          setCurrentHosting(prev => ({
            ...prev,
            database: {
              type: data.databaseEngine || data.currentProvider || 'MariaDB',
              host: `${data.serverHost || 'localhost'}:${data.serverPort || '3306'}`,
              status: data.connectionStatus === 'Connected' ? 'connected' : 'disconnected',
              size: data.databaseSize || 'Unknown',
            },
            lastCheck: new Date().toISOString(),
          }));
        }
      } catch (error) {
        console.error('Error fetching database status:', error);
      } finally {
        setLoadingHostingStatus(false);
      }
    };
    fetchDatabaseStatus();
  }, []);
  
  const [credentials, setCredentials] = useState<ProviderCredentials>({
    azure: { subscriptionId: '', tenantId: '', clientId: '', clientSecret: '', resourceGroup: 'crm-resources' },
    aws: { accessKeyId: '', secretAccessKey: '', region: 'us-east-1', accountId: '' },
    gcp: { projectId: '', serviceAccountKey: '', region: 'us-central1' },
    onprem: { serverHost: '', sshUser: 'root', sshKeyPath: '~/.ssh/id_rsa', dockerHost: '' },
  });
  
  const [showCredential, setShowCredential] = useState<Record<string, boolean>>({});
  const [credentialsValidated, setCredentialsValidated] = useState<Record<string, boolean>>({});
  const [validatingCredentials, setValidatingCredentials] = useState(false);
  
  const [replicationConfig, setReplicationConfig] = useState<ReplicationConfig>({
    replicateDatabase: true,
    replicateSettings: true,
    replicateUsers: true,
    replicateCustomizations: true,
    fullBackup: true,
    incrementalSync: false,
  });
  
  const [replicating, setReplicating] = useState(false);
  const [replicationProgress, setReplicationProgress] = useState(0);
  const [replicationLog, setReplicationLog] = useState<string[]>([]);
  
  const [scriptDialogOpen, setScriptDialogOpen] = useState(false);
  const [generatedScript, setGeneratedScript] = useState('');
  const [scriptType, setScriptType] = useState<'docker' | 'kubernetes' | 'terraform' | 'shell'>('docker');
  const [generatingScript, setGeneratingScript] = useState(false);

  // API-integrated state for deployment management
  const [apiProviders, setApiProviders] = useState<ApiCloudProvider[]>([]);
  const [apiDeployments, setApiDeployments] = useState<ApiCloudDeployment[]>([]);
  const [deploymentAttempts, setDeploymentAttempts] = useState<DeploymentAttempt[]>([]);
  const [healthChecks, setHealthChecks] = useState<HealthCheck[]>([]);
  const [dashboard, setDashboard] = useState<DeploymentDashboard | null>(null);
  const [loadingApi, setLoadingApi] = useState(false);
  const [selectedDeployment, setSelectedDeployment] = useState<ApiCloudDeployment | null>(null);
  const [healthCheckResult, setHealthCheckResult] = useState<HealthCheckResult | null>(null);
  const [runningHealthCheck, setRunningHealthCheck] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);
  const [showAttemptLogsDialog, setShowAttemptLogsDialog] = useState(false);
  const [attemptLogs, setAttemptLogs] = useState<string>('');
  const [triggeringDeployment, setTriggeringDeployment] = useState(false);

  // Load API data
  const loadApiData = useCallback(async () => {
    setLoadingApi(true);
    setApiError(null);
    try {
      const [providersData, deploymentsData, dashboardData] = await Promise.all([
        cloudDeploymentService.getProviders(),
        cloudDeploymentService.getDeployments(),
        cloudDeploymentService.getDashboard()
      ]);
      setApiProviders(providersData);
      setApiDeployments(deploymentsData);
      setDashboard(dashboardData);
    } catch (error: unknown) {
      console.error('Error loading deployment data:', error);
      setApiError('Failed to load deployment data. The deployment API may not be configured yet.');
    } finally {
      setLoadingApi(false);
    }
  }, []);

  // Load deployment attempts for selected deployment
  const loadDeploymentAttempts = useCallback(async (deploymentId: number) => {
    try {
      const attempts = await cloudDeploymentService.getDeploymentAttempts(deploymentId);
      setDeploymentAttempts(attempts);
    } catch (error) {
      console.error('Error loading deployment attempts:', error);
    }
  }, []);

  // Load health check history
  const loadHealthCheckHistory = useCallback(async (deploymentId: number) => {
    try {
      const checks = await cloudDeploymentService.getHealthCheckHistory(deploymentId, 20);
      setHealthChecks(checks);
    } catch (error) {
      console.error('Error loading health checks:', error);
    }
  }, []);

  // Run health check
  const runHealthCheck = async (deploymentId: number) => {
    setRunningHealthCheck(true);
    try {
      const result = await cloudDeploymentService.runHealthCheck(deploymentId);
      setHealthCheckResult(result);
      await loadHealthCheckHistory(deploymentId);
      await loadApiData();
    } catch (error) {
      console.error('Error running health check:', error);
    } finally {
      setRunningHealthCheck(false);
    }
  };

  // Trigger deployment
  const handleTriggerDeployment = async (deploymentId: number) => {
    setTriggeringDeployment(true);
    try {
      const result = await cloudDeploymentService.triggerDeployment(deploymentId, {
        deploymentId,
        forceBuild: false
      });
      if (result.success) {
        setDeploymentLogs([result.message, result.deployLog || '']);
        setShowLogsDialog(true);
      }
      await loadApiData();
      await loadDeploymentAttempts(deploymentId);
    } catch (error) {
      console.error('Error triggering deployment:', error);
    } finally {
      setTriggeringDeployment(false);
    }
  };

  // View attempt logs
  const viewAttemptLogs = async (attemptId: number) => {
    try {
      const logs = await cloudDeploymentService.getDeploymentAttemptLogs(attemptId);
      setAttemptLogs(logs);
      setShowAttemptLogsDialog(true);
    } catch (error) {
      console.error('Error loading attempt logs:', error);
    }
  };

  // Stop deployment
  const handleStopDeployment = async (deploymentId: number) => {
    try {
      await cloudDeploymentService.stopDeployment(deploymentId);
      await loadApiData();
    } catch (error) {
      console.error('Error stopping deployment:', error);
    }
  };

  // Restart deployment
  const handleRestartDeployment = async (deploymentId: number) => {
    try {
      await cloudDeploymentService.restartDeployment(deploymentId);
      await loadApiData();
    } catch (error) {
      console.error('Error restarting deployment:', error);
    }
  };

  // Load API data on mount
  useEffect(() => {
    loadApiData();
  }, [loadApiData]);

  // Refresh hosting status
  const refreshHostingStatus = async () => {
    setLoadingHostingStatus(true);
    try {
      const token = localStorage.getItem('token');
      const response = await fetch('/api/database/status', {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });
      if (response.ok) {
        const data = await response.json();
        setCurrentHosting(prev => ({
          ...prev,
          database: {
            type: data.databaseEngine || data.currentProvider || 'MariaDB',
            host: `${data.serverHost || 'localhost'}:${data.serverPort || '3306'}`,
            status: data.connectionStatus === 'Connected' ? 'connected' : 'disconnected',
            size: data.databaseSize || 'Unknown',
          },
          lastCheck: new Date().toISOString(),
        }));
      }
    } catch (error) {
      console.error('Error refreshing hosting status:', error);
    } finally {
      setLoadingHostingStatus(false);
    }
  };

  // Validate provider credentials
  const validateCredentials = async (provider: string) => {
    setValidatingCredentials(true);
    // Simulate validation
    await new Promise(resolve => setTimeout(resolve, 2000));
    setCredentialsValidated(prev => ({ ...prev, [provider]: true }));
    setValidatingCredentials(false);
  };

  // Generate deployment scripts
  const generateDeploymentScript = (type: 'docker' | 'kubernetes' | 'terraform' | 'shell') => {
    setScriptType(type);
    setGeneratingScript(true);
    
    let script = '';
    
    if (type === 'docker') {
      script = generateDockerComposeScript();
    } else if (type === 'kubernetes') {
      script = generateKubernetesScript();
    } else if (type === 'terraform') {
      script = generateTerraformScript();
    } else if (type === 'shell') {
      script = generateShellScript();
    }
    
    setGeneratedScript(script);
    setGeneratingScript(false);
    setScriptDialogOpen(true);
  };

  const generateDockerComposeScript = () => {
    const provider = CLOUD_PROVIDERS.find(p => p.id === selectedProvider);
    return `# Docker Compose Deployment Script for ${provider?.name || 'CRM Solution'}
# Generated: ${new Date().toLocaleString()}
# Region: ${deploymentConfig.region || 'Default'}

version: '3.8'

services:
  crm-api:
    image: crm-api:latest
    build:
      context: ./CRM.Backend
      dockerfile: Dockerfile.backend
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=${currentHosting.database.host};Database=crm_db;User=crm_user;Password=\${DB_PASSWORD}
      - JWT_SECRET=\${JWT_SECRET}
${deploymentConfig.enableSsl ? '      - ASPNETCORE_URLS=https://+:5000' : '      - ASPNETCORE_URLS=http://+:5000'}
    depends_on:
      - crm-database
    restart: always
    deploy:
      replicas: ${deploymentConfig.apiReplicas}
${deploymentConfig.enableAutoScaling ? `      resources:
        limits:
          cpus: '1.0'
          memory: 1G` : ''}

  crm-frontend:
    image: crm-frontend:latest
    build:
      context: ./CRM.Frontend
      dockerfile: Dockerfile.frontend
    ports:
      - "3000:80"
    environment:
      - VITE_API_URL=http://crm-api:5000/api
    depends_on:
      - crm-api
    restart: always
    deploy:
      replicas: ${deploymentConfig.frontendReplicas}

  crm-database:
    image: mariadb:10.11
    environment:
      - MYSQL_ROOT_PASSWORD=\${DB_ROOT_PASSWORD}
      - MYSQL_DATABASE=crm_db
      - MYSQL_USER=crm_user
      - MYSQL_PASSWORD=\${DB_PASSWORD}
    volumes:
      - db_data:/var/lib/mysql
    ports:
      - "3306:3306"
    restart: always

volumes:
  db_data:

networks:
  default:
    name: crm-network
`;
  };

  const generateKubernetesScript = () => {
    const provider = CLOUD_PROVIDERS.find(p => p.id === selectedProvider);
    return `# Kubernetes Deployment Manifest for ${provider?.name || 'CRM Solution'}
# Generated: ${new Date().toLocaleString()}
# Namespace: ${deploymentConfig.kubernetesNamespace}
# Region: ${deploymentConfig.region || 'Default'}

---
apiVersion: v1
kind: Namespace
metadata:
  name: ${deploymentConfig.kubernetesNamespace}

---
apiVersion: v1
kind: Secret
metadata:
  name: crm-secrets
  namespace: ${deploymentConfig.kubernetesNamespace}
type: Opaque
stringData:
  db-password: "\${DB_PASSWORD}"
  jwt-secret: "\${JWT_SECRET}"

---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: crm-api
  namespace: ${deploymentConfig.kubernetesNamespace}
spec:
  replicas: ${deploymentConfig.apiReplicas}
  selector:
    matchLabels:
      app: crm-api
  template:
    metadata:
      labels:
        app: crm-api
    spec:
      containers:
      - name: crm-api
        image: crm-api:latest
        ports:
        - containerPort: 5000
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          value: "Server=crm-database;Database=crm_db;User=crm_user;Password=$(DB_PASSWORD)"
        envFrom:
        - secretRef:
            name: crm-secrets
        resources:
          limits:
            memory: "1Gi"
            cpu: "500m"
          requests:
            memory: "256Mi"
            cpu: "100m"
${deploymentConfig.enableAutoScaling ? `
---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: crm-api-hpa
  namespace: ${deploymentConfig.kubernetesNamespace}
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: crm-api
  minReplicas: ${deploymentConfig.minReplicas}
  maxReplicas: ${deploymentConfig.maxReplicas}
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70` : ''}

---
apiVersion: v1
kind: Service
metadata:
  name: crm-api
  namespace: ${deploymentConfig.kubernetesNamespace}
spec:
  selector:
    app: crm-api
  ports:
  - port: 5000
    targetPort: 5000
  type: ClusterIP

---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: crm-frontend
  namespace: ${deploymentConfig.kubernetesNamespace}
spec:
  replicas: ${deploymentConfig.frontendReplicas}
  selector:
    matchLabels:
      app: crm-frontend
  template:
    metadata:
      labels:
        app: crm-frontend
    spec:
      containers:
      - name: crm-frontend
        image: crm-frontend:latest
        ports:
        - containerPort: 80
        env:
        - name: VITE_API_URL
          value: "http://crm-api:5000/api"

---
apiVersion: v1
kind: Service
metadata:
  name: crm-frontend
  namespace: ${deploymentConfig.kubernetesNamespace}
spec:
  selector:
    app: crm-frontend
  ports:
  - port: 80
    targetPort: 80
  type: LoadBalancer
`;
  };

  const generateTerraformScript = () => {
    const provider = CLOUD_PROVIDERS.find(p => p.id === selectedProvider);
    
    if (selectedProvider === 'azure') {
      return `# Terraform Configuration for Azure Deployment
# Generated: ${new Date().toLocaleString()}
# Provider: ${provider?.name}

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~>3.0"
    }
  }
}

provider "azurerm" {
  features {}
  subscription_id = "${credentials.azure?.subscriptionId || '<SUBSCRIPTION_ID>'}"
  tenant_id       = "${credentials.azure?.tenantId || '<TENANT_ID>'}"
  client_id       = "${credentials.azure?.clientId || '<CLIENT_ID>'}"
  client_secret   = "${credentials.azure?.clientSecret || '<CLIENT_SECRET>'}"
}

resource "azurerm_resource_group" "crm" {
  name     = "${credentials.azure?.resourceGroup || 'crm-resources'}"
  location = "${deploymentConfig.region || 'East US'}"
}

resource "azurerm_container_registry" "crm" {
  name                = "crmcontainerregistry"
  resource_group_name = azurerm_resource_group.crm.name
  location            = azurerm_resource_group.crm.location
  sku                 = "Standard"
  admin_enabled       = true
}

resource "azurerm_mysql_flexible_server" "crm" {
  name                   = "crm-database-server"
  resource_group_name    = azurerm_resource_group.crm.name
  location               = azurerm_resource_group.crm.location
  administrator_login    = "crmadmin"
  administrator_password = var.db_password
  sku_name               = "${deploymentConfig.databaseTier === 'basic' ? 'B_Standard_B1s' : deploymentConfig.databaseTier === 'standard' ? 'GP_Standard_D2ds_v4' : 'MO_Standard_E4ds_v4'}"
  version                = "8.0.21"
}

${deploymentConfig.useKubernetes ? `
resource "azurerm_kubernetes_cluster" "crm" {
  name                = "crm-aks-cluster"
  location            = azurerm_resource_group.crm.location
  resource_group_name = azurerm_resource_group.crm.name
  dns_prefix          = "crm-k8s"

  default_node_pool {
    name       = "default"
    node_count = ${deploymentConfig.apiReplicas}
    vm_size    = "Standard_D2_v2"
${deploymentConfig.enableAutoScaling ? `    enable_auto_scaling = true
    min_count           = ${deploymentConfig.minReplicas}
    max_count           = ${deploymentConfig.maxReplicas}` : ''}
  }

  identity {
    type = "SystemAssigned"
  }
}
` : `
resource "azurerm_container_group" "crm_api" {
  name                = "crm-api"
  location            = azurerm_resource_group.crm.location
  resource_group_name = azurerm_resource_group.crm.name
  os_type             = "Linux"

  container {
    name   = "crm-api"
    image  = "\${azurerm_container_registry.crm.login_server}/crm-api:latest"
    cpu    = "0.5"
    memory = "1.0"

    ports {
      port     = 5000
      protocol = "TCP"
    }
  }
}
`}

variable "db_password" {
  type      = string
  sensitive = true
}

output "container_registry_url" {
  value = azurerm_container_registry.crm.login_server
}
`;
    } else if (selectedProvider === 'aws') {
      return `# Terraform Configuration for AWS Deployment
# Generated: ${new Date().toLocaleString()}
# Provider: ${provider?.name}

terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~>5.0"
    }
  }
}

provider "aws" {
  region     = "${credentials.aws?.region || deploymentConfig.region || 'us-east-1'}"
  access_key = "${credentials.aws?.accessKeyId || '<ACCESS_KEY_ID>'}"
  secret_key = "${credentials.aws?.secretAccessKey || '<SECRET_ACCESS_KEY>'}"
}

resource "aws_ecr_repository" "crm_api" {
  name                 = "crm-api"
  image_tag_mutability = "MUTABLE"
}

resource "aws_ecr_repository" "crm_frontend" {
  name                 = "crm-frontend"
  image_tag_mutability = "MUTABLE"
}

resource "aws_db_instance" "crm" {
  identifier           = "crm-database"
  allocated_storage    = 20
  engine              = "mariadb"
  engine_version      = "10.11"
  instance_class      = "${deploymentConfig.databaseTier === 'basic' ? 'db.t3.micro' : deploymentConfig.databaseTier === 'standard' ? 'db.t3.medium' : 'db.r5.large'}"
  db_name             = "crm_db"
  username            = "crmadmin"
  password            = var.db_password
  skip_final_snapshot = true
}

${deploymentConfig.useKubernetes ? `
resource "aws_eks_cluster" "crm" {
  name     = "crm-eks-cluster"
  role_arn = aws_iam_role.eks_cluster.arn

  vpc_config {
    subnet_ids = aws_subnet.crm[*].id
  }
}

resource "aws_eks_node_group" "crm" {
  cluster_name    = aws_eks_cluster.crm.name
  node_group_name = "crm-nodes"
  node_role_arn   = aws_iam_role.eks_nodes.arn
  subnet_ids      = aws_subnet.crm[*].id

  scaling_config {
    desired_size = ${deploymentConfig.apiReplicas}
    max_size     = ${deploymentConfig.maxReplicas}
    min_size     = ${deploymentConfig.minReplicas}
  }

  instance_types = ["t3.medium"]
}
` : `
resource "aws_ecs_cluster" "crm" {
  name = "crm-cluster"
}

resource "aws_ecs_task_definition" "crm_api" {
  family                   = "crm-api"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = "512"
  memory                   = "1024"

  container_definitions = jsonencode([
    {
      name  = "crm-api"
      image = "\${aws_ecr_repository.crm_api.repository_url}:latest"
      portMappings = [
        {
          containerPort = 5000
          hostPort      = 5000
        }
      ]
    }
  ])
}
`}

variable "db_password" {
  type      = string
  sensitive = true
}

output "ecr_repository_url" {
  value = aws_ecr_repository.crm_api.repository_url
}
`;
    } else if (selectedProvider === 'gcp') {
      return `# Terraform Configuration for Google Cloud Platform
# Generated: ${new Date().toLocaleString()}
# Provider: ${provider?.name}

terraform {
  required_providers {
    google = {
      source  = "hashicorp/google"
      version = "~>4.0"
    }
  }
}

provider "google" {
  project = "${credentials.gcp?.projectId || '<PROJECT_ID>'}"
  region  = "${credentials.gcp?.region || deploymentConfig.region || 'us-central1'}"
}

resource "google_artifact_registry_repository" "crm" {
  location      = "${credentials.gcp?.region || 'us-central1'}"
  repository_id = "crm-repository"
  format        = "DOCKER"
}

resource "google_sql_database_instance" "crm" {
  name             = "crm-database-instance"
  database_version = "MYSQL_8_0"
  region           = "${credentials.gcp?.region || 'us-central1'}"

  settings {
    tier = "${deploymentConfig.databaseTier === 'basic' ? 'db-f1-micro' : deploymentConfig.databaseTier === 'standard' ? 'db-n1-standard-1' : 'db-n1-highmem-2'}"
  }
}

${deploymentConfig.useKubernetes ? `
resource "google_container_cluster" "crm" {
  name     = "crm-gke-cluster"
  location = "${credentials.gcp?.region || 'us-central1'}"

  initial_node_count = ${deploymentConfig.apiReplicas}

  node_config {
    machine_type = "e2-medium"
  }

${deploymentConfig.enableAutoScaling ? `  cluster_autoscaling {
    enabled = true
    resource_limits {
      resource_type = "cpu"
      minimum       = ${deploymentConfig.minReplicas}
      maximum       = ${deploymentConfig.maxReplicas}
    }
  }` : ''}
}
` : `
resource "google_cloud_run_service" "crm_api" {
  name     = "crm-api"
  location = "${credentials.gcp?.region || 'us-central1'}"

  template {
    spec {
      containers {
        image = "gcr.io/\${var.project_id}/crm-api:latest"
        ports {
          container_port = 5000
        }
      }
    }
  }
}
`}

variable "db_password" {
  type      = string
  sensitive = true
}
`;
    }
    
    return `# Terraform configuration for ${provider?.name || 'On-Premises'}
# Please configure manually for on-premises deployment
`;
  };

  const generateShellScript = () => {
    const provider = CLOUD_PROVIDERS.find(p => p.id === selectedProvider);
    return `#!/bin/bash
# Deployment Script for ${provider?.name || 'CRM Solution'}
# Generated: ${new Date().toLocaleString()}
# Region: ${deploymentConfig.region || 'Default'}

set -e

echo "=== CRM Solution Deployment Script ==="
echo "Provider: ${provider?.name || 'On-Premises'}"
echo "Region: ${deploymentConfig.region || 'Local'}"
echo ""

# Check prerequisites
echo "Checking prerequisites..."
command -v docker >/dev/null 2>&1 || { echo "Docker is required but not installed. Aborting."; exit 1; }
${deploymentConfig.useKubernetes ? 'command -v kubectl >/dev/null 2>&1 || { echo "kubectl is required but not installed. Aborting."; exit 1; }' : ''}

# Set environment variables
export DB_PASSWORD="\${DB_PASSWORD:-$(openssl rand -base64 32)}"
export JWT_SECRET="\${JWT_SECRET:-$(openssl rand -base64 64)}"

# Build Docker images
echo ""
echo "Building Docker images..."
docker build -t crm-api:latest -f Dockerfile.backend ./CRM.Backend
docker build -t crm-frontend:latest -f Dockerfile.frontend ./CRM.Frontend

${selectedProvider === 'azure' ? `
# Azure-specific deployment
echo ""
echo "Logging into Azure..."
az login --service-principal \\
  --username "${credentials.azure?.clientId || '<CLIENT_ID>'}" \\
  --password "${credentials.azure?.clientSecret || '<CLIENT_SECRET>'}" \\
  --tenant "${credentials.azure?.tenantId || '<TENANT_ID>'}"

az account set --subscription "${credentials.azure?.subscriptionId || '<SUBSCRIPTION_ID>'}"

# Push to Azure Container Registry
echo "Pushing images to ACR..."
az acr login --name crmcontainerregistry
docker tag crm-api:latest crmcontainerregistry.azurecr.io/crm-api:latest
docker tag crm-frontend:latest crmcontainerregistry.azurecr.io/crm-frontend:latest
docker push crmcontainerregistry.azurecr.io/crm-api:latest
docker push crmcontainerregistry.azurecr.io/crm-frontend:latest
` : selectedProvider === 'aws' ? `
# AWS-specific deployment
echo ""
echo "Configuring AWS CLI..."
aws configure set aws_access_key_id "${credentials.aws?.accessKeyId || '<ACCESS_KEY_ID>'}"
aws configure set aws_secret_access_key "${credentials.aws?.secretAccessKey || '<SECRET_ACCESS_KEY>'}"
aws configure set region "${credentials.aws?.region || deploymentConfig.region || 'us-east-1'}"

# Push to ECR
echo "Pushing images to ECR..."
aws ecr get-login-password | docker login --username AWS --password-stdin ${credentials.aws?.accountId || '<ACCOUNT_ID>'}.dkr.ecr.${credentials.aws?.region || 'us-east-1'}.amazonaws.com
docker tag crm-api:latest ${credentials.aws?.accountId || '<ACCOUNT_ID>'}.dkr.ecr.${credentials.aws?.region || 'us-east-1'}.amazonaws.com/crm-api:latest
docker push ${credentials.aws?.accountId || '<ACCOUNT_ID>'}.dkr.ecr.${credentials.aws?.region || 'us-east-1'}.amazonaws.com/crm-api:latest
` : selectedProvider === 'gcp' ? `
# GCP-specific deployment
echo ""
echo "Authenticating with GCP..."
gcloud auth activate-service-account --key-file="${credentials.gcp?.serviceAccountKey || '<SERVICE_ACCOUNT_KEY_FILE>'}"
gcloud config set project "${credentials.gcp?.projectId || '<PROJECT_ID>'}"
gcloud config set compute/region "${credentials.gcp?.region || 'us-central1'}"

# Push to GCR
echo "Pushing images to GCR..."
gcloud auth configure-docker
docker tag crm-api:latest gcr.io/${credentials.gcp?.projectId || '<PROJECT_ID>'}/crm-api:latest
docker push gcr.io/${credentials.gcp?.projectId || '<PROJECT_ID>'}/crm-api:latest
` : `
# On-premises deployment
echo ""
echo "Deploying to on-premises server..."
${credentials.onprem?.serverHost ? `ssh ${credentials.onprem.sshUser}@${credentials.onprem.serverHost} "mkdir -p ~/crm-deployment"
scp docker-compose.yml ${credentials.onprem.sshUser}@${credentials.onprem.serverHost}:~/crm-deployment/` : '# Configure server details in credentials'}
`}

${deploymentConfig.useKubernetes ? `
# Deploy to Kubernetes
echo ""
echo "Deploying to Kubernetes..."
kubectl apply -f kubernetes/00-namespace-config.yaml
kubectl apply -f kubernetes/01-database-tier.yaml
kubectl apply -f kubernetes/02-application-tier.yaml

echo "Waiting for pods to be ready..."
kubectl wait --for=condition=ready pod -l app=crm-api -n ${deploymentConfig.kubernetesNamespace} --timeout=300s
` : `
# Deploy with Docker Compose
echo ""
echo "Starting Docker Compose..."
docker-compose up -d
`}

echo ""
echo "=== Deployment Complete ==="
echo "API URL: ${deploymentConfig.enableSsl ? 'https' : 'http'}://localhost:5000"
echo "Frontend URL: http://localhost:3000"
`;
  };

  // Handle replication
  const startReplication = async () => {
    setReplicating(true);
    setReplicationProgress(0);
    setReplicationLog([]);
    
    const steps = [
      { message: 'Preparing replication...', progress: 5 },
      { message: 'Connecting to target environment...', progress: 10 },
      ...(replicationConfig.replicateDatabase ? [
        { message: 'Creating database backup...', progress: 20 },
        { message: 'Transferring database (256 MB)...', progress: 40 },
        { message: 'Restoring database on target...', progress: 55 },
      ] : []),
      ...(replicationConfig.replicateSettings ? [
        { message: 'Exporting system settings...', progress: 60 },
        { message: 'Applying settings to target...', progress: 65 },
      ] : []),
      ...(replicationConfig.replicateUsers ? [
        { message: 'Migrating user accounts...', progress: 75 },
        { message: 'Syncing permissions...', progress: 80 },
      ] : []),
      ...(replicationConfig.replicateCustomizations ? [
        { message: 'Transferring UI customizations...', progress: 85 },
        { message: 'Syncing module configurations...', progress: 90 },
      ] : []),
      { message: 'Verifying replication...', progress: 95 },
      { message: 'Replication completed successfully!', progress: 100 },
    ];
    
    for (const step of steps) {
      await new Promise(resolve => setTimeout(resolve, 1000));
      setReplicationProgress(step.progress);
      setReplicationLog(prev => [...prev, `[${new Date().toLocaleTimeString()}] ${step.message}`]);
    }
    
    setReplicating(false);
  };

  const getRegions = () => {
    switch (selectedProvider) {
      case 'azure': return AZURE_REGIONS;
      case 'aws': return AWS_REGIONS;
      case 'gcp': return GCP_REGIONS;
      default: return ['Local Network'];
    }
  };

  const getDatabaseOptions = (): DatabaseOption[] => {
    return PAAS_DATABASE_OPTIONS[selectedProvider] || DATABASE_OPTIONS;
  };

  const handleProviderSelect = (providerId: string) => {
    setSelectedProvider(providerId);
    setDeploymentConfig(prev => ({ ...prev, provider: providerId, region: '', databaseType: '' }));
    setActiveStep(1);
  };

  const handleConfigChange = (field: keyof DeploymentConfig, value: any) => {
    setDeploymentConfig(prev => ({ ...prev, [field]: value }));
  };

  const simulateDeployment = async () => {
    setDeploying(true);
    setDeploymentStatus('deploying');
    setDeploymentLogs([]);

    const steps = [
      'Initializing deployment...',
      'Validating configuration...',
      `Connecting to ${selectedProvider.toUpperCase()}...`,
      'Provisioning database resources...',
      'Creating container registry...',
      'Building Docker images...',
      'Pushing images to registry...',
      deploymentConfig.useKubernetes ? 'Creating Kubernetes cluster...' : 'Provisioning container instances...',
      deploymentConfig.useKubernetes ? 'Deploying to Kubernetes...' : 'Starting containers...',
      'Configuring networking...',
      'Setting up SSL certificates...',
      'Running health checks...',
      'Deployment complete!',
    ];

    for (const step of steps) {
      await new Promise(resolve => setTimeout(resolve, 1500));
      setDeploymentLogs(prev => [...prev, `[${new Date().toLocaleTimeString()}] ${step}`]);
    }

    setDeploying(false);
    setDeploymentStatus('success');
    
    // Update targets to deployed
    setTargets(prev => prev.map(t => ({ ...t, status: 'deployed' as const, lastDeployed: new Date().toISOString() })));
  };

  const getProviderColor = (providerId: string) => {
    return CLOUD_PROVIDERS.find(p => p.id === providerId)?.color || '#6750A4';
  };

  const renderProviderSelection = () => (
    <Box>
      <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
        Select Cloud Provider
      </Typography>
      <Grid container spacing={2}>
        {CLOUD_PROVIDERS.map((provider) => (
          <Grid item xs={12} sm={6} key={provider.id}>
            <Card
              sx={{
                cursor: 'pointer',
                border: selectedProvider === provider.id ? `2px solid ${provider.color}` : '1px solid #e0e0e0',
                transition: 'all 0.2s',
                '&:hover': { borderColor: provider.color, transform: 'translateY(-2px)' },
              }}
              onClick={() => handleProviderSelect(provider.id)}
            >
              <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                {provider.icon}
                <Box>
                  <Typography variant="subtitle1" fontWeight={600}>
                    {provider.name}
                  </Typography>
                  {selectedProvider === provider.id && (
                    <Chip label="Selected" size="small" sx={{ bgcolor: provider.color, color: 'white', mt: 0.5 }} />
                  )}
                </Box>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Box>
  );

  const renderDatabaseConfig = () => (
    <Box>
      <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
        Database Configuration
      </Typography>
      <Grid container spacing={2}>
        <Grid item xs={12} md={6}>
          <FormControl fullWidth>
            <InputLabel>Region</InputLabel>
            <Select
              value={deploymentConfig.region}
              label="Region"
              onChange={(e) => handleConfigChange('region', e.target.value)}
            >
              {getRegions().map((region) => (
                <MenuItem key={region} value={region}>{region}</MenuItem>
              ))}
            </Select>
          </FormControl>
        </Grid>
        <Grid item xs={12} md={6}>
          <FormControl fullWidth>
            <InputLabel>Database Type</InputLabel>
            <Select
              value={deploymentConfig.databaseType}
              label="Database Type"
              onChange={(e) => handleConfigChange('databaseType', e.target.value)}
            >
              {getDatabaseOptions().map((db) => (
                <MenuItem key={db.id} value={db.id}>
                  <Box>
                    <Typography variant="body2">{db.name}</Typography>
                    {db.description && (
                      <Typography variant="caption" color="textSecondary">{db.description}</Typography>
                    )}
                  </Box>
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Grid>
        <Grid item xs={12}>
          <FormControl fullWidth>
            <InputLabel>Database Tier</InputLabel>
            <Select
              value={deploymentConfig.databaseTier}
              label="Database Tier"
              onChange={(e) => handleConfigChange('databaseTier', e.target.value)}
            >
              <MenuItem value="basic">Basic (Development)</MenuItem>
              <MenuItem value="standard">Standard (Production)</MenuItem>
              <MenuItem value="premium">Premium (High Availability)</MenuItem>
              <MenuItem value="enterprise">Enterprise (Mission Critical)</MenuItem>
            </Select>
          </FormControl>
        </Grid>
      </Grid>
    </Box>
  );

  const renderApplicationConfig = () => (
    <Box>
      <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
        Application Deployment
      </Typography>
      <Grid container spacing={2}>
        <Grid item xs={12}>
          <FormControlLabel
            control={
              <Switch
                checked={deploymentConfig.useKubernetes}
                onChange={(e) => handleConfigChange('useKubernetes', e.target.checked)}
              />
            }
            label="Deploy as Kubernetes Cluster"
          />
        </Grid>
        {deploymentConfig.useKubernetes && (
          <Grid item xs={12}>
            <TextField
              fullWidth
              label="Kubernetes Namespace"
              value={deploymentConfig.kubernetesNamespace}
              onChange={(e) => handleConfigChange('kubernetesNamespace', e.target.value)}
            />
          </Grid>
        )}
        <Grid item xs={12} md={6}>
          <TextField
            fullWidth
            label="API Replicas"
            type="number"
            value={deploymentConfig.apiReplicas}
            onChange={(e) => handleConfigChange('apiReplicas', parseInt(e.target.value))}
            inputProps={{ min: 1, max: 10 }}
          />
        </Grid>
        <Grid item xs={12} md={6}>
          <TextField
            fullWidth
            label="Frontend Replicas"
            type="number"
            value={deploymentConfig.frontendReplicas}
            onChange={(e) => handleConfigChange('frontendReplicas', parseInt(e.target.value))}
            inputProps={{ min: 1, max: 10 }}
          />
        </Grid>
        <Grid item xs={12}>
          <Divider sx={{ my: 2 }} />
        </Grid>
        <Grid item xs={12}>
          <FormControlLabel
            control={
              <Switch
                checked={deploymentConfig.enableSsl}
                onChange={(e) => handleConfigChange('enableSsl', e.target.checked)}
              />
            }
            label="Enable SSL/TLS"
          />
        </Grid>
        <Grid item xs={12}>
          <FormControlLabel
            control={
              <Switch
                checked={deploymentConfig.enableAutoScaling}
                onChange={(e) => handleConfigChange('enableAutoScaling', e.target.checked)}
              />
            }
            label="Enable Auto Scaling"
          />
        </Grid>
        {deploymentConfig.enableAutoScaling && (
          <>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Min Replicas"
                type="number"
                value={deploymentConfig.minReplicas}
                onChange={(e) => handleConfigChange('minReplicas', parseInt(e.target.value))}
                inputProps={{ min: 1, max: 10 }}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Max Replicas"
                type="number"
                value={deploymentConfig.maxReplicas}
                onChange={(e) => handleConfigChange('maxReplicas', parseInt(e.target.value))}
                inputProps={{ min: 1, max: 50 }}
              />
            </Grid>
          </>
        )}
      </Grid>
    </Box>
  );

  const renderReview = () => (
    <Box>
      <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
        Review & Deploy
      </Typography>
      <Paper sx={{ p: 2, mb: 3, bgcolor: '#F5EFF7', borderRadius: 2 }}>
        <Grid container spacing={2}>
          <Grid item xs={12} md={6}>
            <Typography variant="subtitle2" color="textSecondary">Cloud Provider</Typography>
            <Typography variant="body1" fontWeight={500}>
              {CLOUD_PROVIDERS.find(p => p.id === selectedProvider)?.name}
            </Typography>
          </Grid>
          <Grid item xs={12} md={6}>
            <Typography variant="subtitle2" color="textSecondary">Region</Typography>
            <Typography variant="body1" fontWeight={500}>{deploymentConfig.region}</Typography>
          </Grid>
          <Grid item xs={12} md={6}>
            <Typography variant="subtitle2" color="textSecondary">Database</Typography>
            <Typography variant="body1" fontWeight={500}>
              {getDatabaseOptions().find((d) => d.id === deploymentConfig.databaseType)?.name} ({deploymentConfig.databaseTier})
            </Typography>
          </Grid>
          <Grid item xs={12} md={6}>
            <Typography variant="subtitle2" color="textSecondary">Deployment Type</Typography>
            <Typography variant="body1" fontWeight={500}>
              {deploymentConfig.useKubernetes ? 'Kubernetes Cluster' : 'Container Instances'}
            </Typography>
          </Grid>
          <Grid item xs={12} md={6}>
            <Typography variant="subtitle2" color="textSecondary">API Replicas</Typography>
            <Typography variant="body1" fontWeight={500}>{deploymentConfig.apiReplicas}</Typography>
          </Grid>
          <Grid item xs={12} md={6}>
            <Typography variant="subtitle2" color="textSecondary">Frontend Replicas</Typography>
            <Typography variant="body1" fontWeight={500}>{deploymentConfig.frontendReplicas}</Typography>
          </Grid>
          <Grid item xs={12}>
            <Divider sx={{ my: 1 }} />
          </Grid>
          <Grid item xs={12}>
            <Box sx={{ display: 'flex', gap: 2 }}>
              {deploymentConfig.enableSsl && <Chip label="SSL Enabled" size="small" color="success" icon={<SecurityIcon />} />}
              {deploymentConfig.enableAutoScaling && <Chip label="Auto Scaling" size="small" color="primary" icon={<RefreshIcon />} />}
              {deploymentConfig.useKubernetes && <Chip label="Kubernetes" size="small" color="secondary" icon={<KubernetesIcon />} />}
            </Box>
          </Grid>
        </Grid>
      </Paper>

      {/* Deployment Targets */}
      <Typography variant="subtitle1" sx={{ mb: 2, fontWeight: 600 }}>
        Deployment Targets
      </Typography>
      <List>
        {targets.map((target) => (
          <ListItem key={target.id} sx={{ bgcolor: '#fafafa', mb: 1, borderRadius: 2 }}>
            <ListItemIcon>
              {target.type === 'paas' ? <DatabaseIcon /> : target.type === 'kubernetes' ? <KubernetesIcon /> : <CloudIcon />}
            </ListItemIcon>
            <ListItemText
              primary={target.name}
              secondary={target.lastDeployed ? `Last deployed: ${new Date(target.lastDeployed).toLocaleString()}` : 'Not deployed'}
            />
            <Chip
              label={target.status.replace('_', ' ')}
              size="small"
              color={target.status === 'deployed' ? 'success' : target.status === 'error' ? 'error' : 'default'}
            />
          </ListItem>
        ))}
      </List>

      {/* Deploy Button */}
      <Box sx={{ mt: 3, display: 'flex', gap: 2, justifyContent: 'center' }}>
        <Button
          variant="contained"
          size="large"
          startIcon={deploying ? <CircularProgress size={20} color="inherit" /> : <PlayIcon />}
          onClick={simulateDeployment}
          disabled={deploying}
          sx={{ backgroundColor: getProviderColor(selectedProvider), minWidth: 200 }}
        >
          {deploying ? 'Deploying...' : 'Start Deployment'}
        </Button>
        <Button
          variant="outlined"
          onClick={() => setShowLogsDialog(true)}
          disabled={deploymentLogs.length === 0}
        >
          View Logs
        </Button>
      </Box>

      {deploymentStatus === 'success' && (
        <Alert severity="success" sx={{ mt: 3 }}>
          Deployment completed successfully! All services are now running.
        </Alert>
      )}
    </Box>
  );

  const steps = [
    { label: 'Select Provider', content: renderProviderSelection() },
    { label: 'Database Setup', content: renderDatabaseConfig() },
    { label: 'Application Setup', content: renderApplicationConfig() },
    { label: 'Review & Deploy', content: renderReview() },
  ];

  // Render Current Hosting Status
  const renderCurrentHosting = () => (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h6" sx={{ fontWeight: 600 }}>
          Current Hosting Status
        </Typography>
        <Button
          variant="outlined"
          startIcon={loadingHostingStatus ? <CircularProgress size={16} /> : <RefreshIcon />}
          onClick={refreshHostingStatus}
          size="small"
          disabled={loadingHostingStatus}
        >
          {loadingHostingStatus ? 'Refreshing...' : 'Refresh'}
        </Button>
      </Box>

      {/* Environment Overview */}
      <Paper sx={{ p: 3, mb: 3, bgcolor: '#f8f9fa', borderRadius: 2 }}>
        <Grid container spacing={3}>
          <Grid item xs={12} md={4}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <Box sx={{ 
                p: 1.5, 
                borderRadius: 2, 
                bgcolor: currentHosting.status === 'healthy' ? '#e8f5e9' : '#fff3e0'
              }}>
                {currentHosting.environment === 'docker' ? <CloudIcon color="primary" /> :
                 currentHosting.environment === 'kubernetes' ? <KubernetesIcon color="secondary" /> :
                 <StorageIcon color="action" />}
              </Box>
              <Box>
                <Typography variant="caption" color="textSecondary">Environment</Typography>
                <Typography variant="h6" sx={{ textTransform: 'capitalize' }}>
                  {currentHosting.environment}
                </Typography>
              </Box>
            </Box>
          </Grid>
          <Grid item xs={12} md={4}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <Box sx={{ 
                p: 1.5, 
                borderRadius: 2, 
                bgcolor: currentHosting.status === 'healthy' ? '#e8f5e9' : '#ffebee'
              }}>
                {currentHosting.status === 'healthy' ? <CheckIcon color="success" /> :
                 currentHosting.status === 'degraded' ? <WarningIcon color="warning" /> :
                 <ErrorIcon color="error" />}
              </Box>
              <Box>
                <Typography variant="caption" color="textSecondary">Status</Typography>
                <Typography variant="h6" sx={{ textTransform: 'capitalize' }}>
                  {currentHosting.status}
                </Typography>
              </Box>
            </Box>
          </Grid>
          <Grid item xs={12} md={4}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <Box sx={{ p: 1.5, borderRadius: 2, bgcolor: '#e3f2fd' }}>
                {CLOUD_PROVIDERS.find(p => p.id === currentHosting.provider)?.icon || <StorageIcon />}
              </Box>
              <Box>
                <Typography variant="caption" color="textSecondary">Provider</Typography>
                <Typography variant="h6">
                  {CLOUD_PROVIDERS.find(p => p.id === currentHosting.provider)?.name || 'Local Development'}
                </Typography>
              </Box>
            </Box>
          </Grid>
        </Grid>
      </Paper>

      {/* Services */}
      <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 2 }}>
        Running Services
      </Typography>
      <TableContainer component={Paper} sx={{ mb: 3 }}>
        <Table size="small">
          <TableHead>
            <TableRow sx={{ bgcolor: '#f5f5f5' }}>
              <TableCell><strong>Service</strong></TableCell>
              <TableCell><strong>Status</strong></TableCell>
              <TableCell><strong>URL</strong></TableCell>
              <TableCell><strong>Version</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {currentHosting.services.map((service, index) => (
              <TableRow key={index}>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    {service.name.includes('API') ? <ApiIcon fontSize="small" color="primary" /> :
                     service.name.includes('Frontend') ? <WebIcon fontSize="small" color="secondary" /> :
                     <SettingsIcon fontSize="small" color="action" />}
                    {service.name}
                  </Box>
                </TableCell>
                <TableCell>
                  <Chip
                    label={service.status}
                    size="small"
                    color={service.status === 'running' ? 'success' : service.status === 'stopped' ? 'default' : 'error'}
                  />
                </TableCell>
                <TableCell>
                  {service.url ? (
                    <Button
                      size="small"
                      href={service.url}
                      target="_blank"
                      sx={{ textTransform: 'none' }}
                    >
                      {service.url}
                    </Button>
                  ) : '-'}
                </TableCell>
                <TableCell>{service.version || '-'}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Database */}
      <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 2 }}>
        Database
      </Typography>
      <Paper sx={{ p: 2 }}>
        <Grid container spacing={2}>
          <Grid item xs={6} md={3}>
            <Typography variant="caption" color="textSecondary">Type</Typography>
            <Typography variant="body1">{currentHosting.database.type}</Typography>
          </Grid>
          <Grid item xs={6} md={3}>
            <Typography variant="caption" color="textSecondary">Host</Typography>
            <Typography variant="body1">{currentHosting.database.host}</Typography>
          </Grid>
          <Grid item xs={6} md={3}>
            <Typography variant="caption" color="textSecondary">Status</Typography>
            <Chip
              label={currentHosting.database.status}
              size="small"
              color={currentHosting.database.status === 'connected' ? 'success' : 'error'}
            />
          </Grid>
          <Grid item xs={6} md={3}>
            <Typography variant="caption" color="textSecondary">Size</Typography>
            <Typography variant="body1">{currentHosting.database.size || '-'}</Typography>
          </Grid>
        </Grid>
      </Paper>

      <Typography variant="caption" color="textSecondary" sx={{ display: 'block', mt: 2 }}>
        Last checked: {new Date(currentHosting.lastCheck).toLocaleString()}
      </Typography>
    </Box>
  );

  // Render Provider Credentials
  const renderCredentials = () => (
    <Box>
      <Typography variant="h6" sx={{ fontWeight: 600, mb: 1 }}>
        Provider Credentials
      </Typography>
      <Typography color="textSecondary" sx={{ mb: 3 }}>
        Configure authentication credentials for each cloud provider. Credentials are stored securely and encrypted.
      </Typography>

      {CLOUD_PROVIDERS.map((provider) => (
        <Accordion key={provider.id} sx={{ mb: 1 }}>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, width: '100%' }}>
              {provider.icon}
              <Typography sx={{ flexGrow: 1 }}>{provider.name}</Typography>
              {credentialsValidated[provider.id] && (
                <Chip
                  icon={<ValidatedIcon />}
                  label="Validated"
                  size="small"
                  color="success"
                  sx={{ mr: 2 }}
                />
              )}
            </Box>
          </AccordionSummary>
          <AccordionDetails>
            <Grid container spacing={2}>
              {provider.id === 'azure' && (
                <>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Subscription ID"
                      value={credentials.azure?.subscriptionId || ''}
                      onChange={(e) => setCredentials(prev => ({
                        ...prev,
                        azure: { ...prev.azure!, subscriptionId: e.target.value }
                      }))}
                      placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Tenant ID"
                      value={credentials.azure?.tenantId || ''}
                      onChange={(e) => setCredentials(prev => ({
                        ...prev,
                        azure: { ...prev.azure!, tenantId: e.target.value }
                      }))}
                      placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Client ID (App ID)"
                      value={credentials.azure?.clientId || ''}
                      onChange={(e) => setCredentials(prev => ({
                        ...prev,
                        azure: { ...prev.azure!, clientId: e.target.value }
                      }))}
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Client Secret"
                      type={showCredential['azure-secret'] ? 'text' : 'password'}
                      value={credentials.azure?.clientSecret || ''}
                      onChange={(e) => setCredentials(prev => ({
                        ...prev,
                        azure: { ...prev.azure!, clientSecret: e.target.value }
                      }))}
                      InputProps={{
                        endAdornment: (
                          <InputAdornment position="end">
                            <IconButton
                              onClick={() => setShowCredential(prev => ({ ...prev, 'azure-secret': !prev['azure-secret'] }))}
                              edge="end"
                            >
                              {showCredential['azure-secret'] ? <VisibilityOffIcon /> : <VisibilityIcon />}
                            </IconButton>
                          </InputAdornment>
                        ),
                      }}
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      label="Resource Group"
                      value={credentials.azure?.resourceGroup || ''}
                      onChange={(e) => setCredentials(prev => ({
                        ...prev,
                        azure: { ...prev.azure!, resourceGroup: e.target.value }
                      }))}
                    />
                  </Grid>
                </>
              )}

              {provider.id === 'aws' && (
                <>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Access Key ID"
                      value={credentials.aws?.accessKeyId || ''}
                      onChange={(e) => setCredentials(prev => ({
                        ...prev,
                        aws: { ...prev.aws!, accessKeyId: e.target.value }
                      }))}
                      placeholder="AKIAXXXXXXXXXXXXXXXX"
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Secret Access Key"
                      type={showCredential['aws-secret'] ? 'text' : 'password'}
                      value={credentials.aws?.secretAccessKey || ''}
                      onChange={(e) => setCredentials(prev => ({
                        ...prev,
                        aws: { ...prev.aws!, secretAccessKey: e.target.value }
                      }))}
                      InputProps={{
                        endAdornment: (
                          <InputAdornment position="end">
                            <IconButton
                              onClick={() => setShowCredential(prev => ({ ...prev, 'aws-secret': !prev['aws-secret'] }))}
                              edge="end"
                            >
                              {showCredential['aws-secret'] ? <VisibilityOffIcon /> : <VisibilityIcon />}
                            </IconButton>
                          </InputAdornment>
                        ),
                      }}
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <FormControl fullWidth>
                      <InputLabel>Region</InputLabel>
                      <Select
                        value={credentials.aws?.region || 'us-east-1'}
                        label="Region"
                        onChange={(e) => setCredentials(prev => ({
                          ...prev,
                          aws: { ...prev.aws!, region: e.target.value }
                        }))}
                      >
                        {AWS_REGIONS.map(r => (
                          <MenuItem key={r} value={r}>{r}</MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Account ID"
                      value={credentials.aws?.accountId || ''}
                      onChange={(e) => setCredentials(prev => ({
                        ...prev,
                        aws: { ...prev.aws!, accountId: e.target.value }
                      }))}
                      placeholder="123456789012"
                    />
                  </Grid>
                </>
              )}

              {provider.id === 'gcp' && (
                <>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Project ID"
                      value={credentials.gcp?.projectId || ''}
                      onChange={(e) => setCredentials(prev => ({
                        ...prev,
                        gcp: { ...prev.gcp!, projectId: e.target.value }
                      }))}
                      placeholder="my-project-123456"
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <FormControl fullWidth>
                      <InputLabel>Region</InputLabel>
                      <Select
                        value={credentials.gcp?.region || 'us-central1'}
                        label="Region"
                        onChange={(e) => setCredentials(prev => ({
                          ...prev,
                          gcp: { ...prev.gcp!, region: e.target.value }
                        }))}
                      >
                        {GCP_REGIONS.map(r => (
                          <MenuItem key={r} value={r}>{r}</MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Grid>
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      label="Service Account Key (JSON)"
                      multiline
                      rows={4}
                      value={credentials.gcp?.serviceAccountKey || ''}
                      onChange={(e) => setCredentials(prev => ({
                        ...prev,
                        gcp: { ...prev.gcp!, serviceAccountKey: e.target.value }
                      }))}
                      placeholder='{"type": "service_account", ...}'
                    />
                  </Grid>
                </>
              )}

              {provider.id === 'onprem' && (
                <>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Server Host"
                      value={credentials.onprem?.serverHost || ''}
                      onChange={(e) => setCredentials(prev => ({
                        ...prev,
                        onprem: { ...prev.onprem!, serverHost: e.target.value }
                      }))}
                      placeholder="192.168.1.100 or server.example.com"
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="SSH User"
                      value={credentials.onprem?.sshUser || 'root'}
                      onChange={(e) => setCredentials(prev => ({
                        ...prev,
                        onprem: { ...prev.onprem!, sshUser: e.target.value }
                      }))}
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="SSH Key Path"
                      value={credentials.onprem?.sshKeyPath || ''}
                      onChange={(e) => setCredentials(prev => ({
                        ...prev,
                        onprem: { ...prev.onprem!, sshKeyPath: e.target.value }
                      }))}
                      placeholder="~/.ssh/id_rsa"
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Docker Host (optional)"
                      value={credentials.onprem?.dockerHost || ''}
                      onChange={(e) => setCredentials(prev => ({
                        ...prev,
                        onprem: { ...prev.onprem!, dockerHost: e.target.value }
                      }))}
                      placeholder="tcp://192.168.1.100:2376"
                    />
                  </Grid>
                </>
              )}

              <Grid item xs={12}>
                <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
                  <Button
                    variant="outlined"
                    onClick={() => validateCredentials(provider.id)}
                    disabled={validatingCredentials}
                    startIcon={validatingCredentials ? <CircularProgress size={16} /> : <SecurityIcon />}
                  >
                    Validate Credentials
                  </Button>
                  <Button
                    variant="contained"
                    sx={{ bgcolor: provider.color }}
                  >
                    Save
                  </Button>
                </Box>
              </Grid>
            </Grid>
          </AccordionDetails>
        </Accordion>
      ))}
    </Box>
  );

  // Render Script Generation
  const renderScriptGeneration = () => (
    <Box>
      <Typography variant="h6" sx={{ fontWeight: 600, mb: 1 }}>
        Generate Deployment Scripts
      </Typography>
      <Typography color="textSecondary" sx={{ mb: 3 }}>
        Generate deployment scripts customized for your selected provider and configuration.
      </Typography>

      {!selectedProvider && (
        <Alert severity="info" sx={{ mb: 3 }}>
          Please select a cloud provider in the "Deploy New" tab first to generate provider-specific scripts.
        </Alert>
      )}

      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                <CloudIcon color="primary" sx={{ fontSize: 40 }} />
                <Typography variant="h6">Docker Compose</Typography>
              </Box>
              <Typography color="textSecondary" sx={{ mb: 2 }}>
                Generate a docker-compose.yml file for container deployment. Includes all services, networking, and volume configurations.
              </Typography>
              <Button
                variant="contained"
                startIcon={<ScriptIcon />}
                onClick={() => generateDeploymentScript('docker')}
                fullWidth
              >
                Generate Docker Compose
              </Button>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={6}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                <KubernetesIcon color="secondary" sx={{ fontSize: 40 }} />
                <Typography variant="h6">Kubernetes</Typography>
              </Box>
              <Typography color="textSecondary" sx={{ mb: 2 }}>
                Generate Kubernetes manifests including Deployments, Services, ConfigMaps, and HPA configurations.
              </Typography>
              <Button
                variant="contained"
                color="secondary"
                startIcon={<ScriptIcon />}
                onClick={() => generateDeploymentScript('kubernetes')}
                fullWidth
              >
                Generate K8s Manifests
              </Button>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={6}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                <DataIcon sx={{ fontSize: 40, color: '#7B42BC' }} />
                <Typography variant="h6">Terraform</Typography>
              </Box>
              <Typography color="textSecondary" sx={{ mb: 2 }}>
                Generate Terraform configuration for Infrastructure as Code. Includes provider-specific resources.
              </Typography>
              <Button
                variant="contained"
                startIcon={<ScriptIcon />}
                onClick={() => generateDeploymentScript('terraform')}
                fullWidth
                sx={{ bgcolor: '#7B42BC' }}
                disabled={!selectedProvider}
              >
                Generate Terraform
              </Button>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={6}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                <TerminalIcon sx={{ fontSize: 40, color: '#333' }} />
                <Typography variant="h6">Shell Script</Typography>
              </Box>
              <Typography color="textSecondary" sx={{ mb: 2 }}>
                Generate a comprehensive deployment shell script with all commands needed for deployment.
              </Typography>
              <Button
                variant="contained"
                startIcon={<ScriptIcon />}
                onClick={() => generateDeploymentScript('shell')}
                fullWidth
                sx={{ bgcolor: '#333' }}
              >
                Generate Shell Script
              </Button>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );

  // Render Data Replication
  const renderDataReplication = () => (
    <Box>
      <Typography variant="h6" sx={{ fontWeight: 600, mb: 1 }}>
        Data & Settings Replication
      </Typography>
      <Typography color="textSecondary" sx={{ mb: 3 }}>
        Replicate your current CRM data and settings to a new deployment target.
      </Typography>

      {!selectedProvider && (
        <Alert severity="warning" sx={{ mb: 3 }}>
          Please configure a deployment target in the "Deploy New" tab first.
        </Alert>
      )}

      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 2 }}>
          Replication Options
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={12} md={6}>
            <FormControlLabel
              control={
                <Switch
                  checked={replicationConfig.replicateDatabase}
                  onChange={(e) => setReplicationConfig(prev => ({ ...prev, replicateDatabase: e.target.checked }))}
                />
              }
              label={
                <Box>
                  <Typography>Replicate Database</Typography>
                  <Typography variant="caption" color="textSecondary">
                    Transfer all database tables and data ({currentHosting.database.size})
                  </Typography>
                </Box>
              }
            />
          </Grid>
          <Grid item xs={12} md={6}>
            <FormControlLabel
              control={
                <Switch
                  checked={replicationConfig.replicateSettings}
                  onChange={(e) => setReplicationConfig(prev => ({ ...prev, replicateSettings: e.target.checked }))}
                />
              }
              label={
                <Box>
                  <Typography>Replicate System Settings</Typography>
                  <Typography variant="caption" color="textSecondary">
                    SMTP, security, integrations, and other configurations
                  </Typography>
                </Box>
              }
            />
          </Grid>
          <Grid item xs={12} md={6}>
            <FormControlLabel
              control={
                <Switch
                  checked={replicationConfig.replicateUsers}
                  onChange={(e) => setReplicationConfig(prev => ({ ...prev, replicateUsers: e.target.checked }))}
                />
              }
              label={
                <Box>
                  <Typography>Replicate User Accounts</Typography>
                  <Typography variant="caption" color="textSecondary">
                    Users, roles, and permissions (passwords will be reset)
                  </Typography>
                </Box>
              }
            />
          </Grid>
          <Grid item xs={12} md={6}>
            <FormControlLabel
              control={
                <Switch
                  checked={replicationConfig.replicateCustomizations}
                  onChange={(e) => setReplicationConfig(prev => ({ ...prev, replicateCustomizations: e.target.checked }))}
                />
              }
              label={
                <Box>
                  <Typography>Replicate Customizations</Typography>
                  <Typography variant="caption" color="textSecondary">
                    UI customizations, module configs, and workflows
                  </Typography>
                </Box>
              }
            />
          </Grid>
        </Grid>

        <Divider sx={{ my: 3 }} />

        <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 2 }}>
          Replication Method
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={12} md={6}>
            <FormControlLabel
              control={
                <Switch
                  checked={replicationConfig.fullBackup}
                  onChange={(e) => setReplicationConfig(prev => ({ 
                    ...prev, 
                    fullBackup: e.target.checked,
                    incrementalSync: !e.target.checked 
                  }))}
                />
              }
              label={
                <Box>
                  <Typography>Full Backup & Restore</Typography>
                  <Typography variant="caption" color="textSecondary">
                    Complete backup transferred to target (slower but complete)
                  </Typography>
                </Box>
              }
            />
          </Grid>
          <Grid item xs={12} md={6}>
            <FormControlLabel
              control={
                <Switch
                  checked={replicationConfig.incrementalSync}
                  onChange={(e) => setReplicationConfig(prev => ({ 
                    ...prev, 
                    incrementalSync: e.target.checked,
                    fullBackup: !e.target.checked
                  }))}
                />
              }
              label={
                <Box>
                  <Typography>Incremental Sync</Typography>
                  <Typography variant="caption" color="textSecondary">
                    Only sync changes since last replication (faster)
                  </Typography>
                </Box>
              }
            />
          </Grid>
        </Grid>
      </Paper>

      {/* Replication Progress */}
      {replicating && (
        <Paper sx={{ p: 3, mb: 3 }}>
          <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 2 }}>
            Replication Progress
          </Typography>
          <LinearProgress variant="determinate" value={replicationProgress} sx={{ mb: 2, height: 10, borderRadius: 5 }} />
          <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
            {replicationProgress}% complete
          </Typography>
          <Paper sx={{ p: 2, bgcolor: '#1e1e1e', borderRadius: 2, maxHeight: 200, overflow: 'auto' }}>
            {replicationLog.map((log, index) => (
              <Typography key={index} variant="body2" sx={{ color: '#00ff00', fontFamily: 'monospace', mb: 0.5 }}>
                {log}
              </Typography>
            ))}
          </Paper>
        </Paper>
      )}

      {replicationProgress === 100 && !replicating && (
        <Alert severity="success" sx={{ mb: 3 }}>
          Replication completed successfully! Your data and settings have been transferred to the target environment.
        </Alert>
      )}

      <Box sx={{ display: 'flex', gap: 2 }}>
        <Button
          variant="contained"
          size="large"
          startIcon={replicating ? <CircularProgress size={20} color="inherit" /> : <SyncIcon />}
          onClick={startReplication}
          disabled={replicating || !selectedProvider}
          sx={{ bgcolor: '#6750A4' }}
        >
          {replicating ? 'Replicating...' : 'Start Replication'}
        </Button>
        {replicationLog.length > 0 && (
          <Button
            variant="outlined"
            startIcon={<DownloadIcon />}
            onClick={() => {
              const blob = new Blob([replicationLog.join('\n')], { type: 'text/plain' });
              const url = URL.createObjectURL(blob);
              const a = document.createElement('a');
              a.href = url;
              a.download = `replication-log-${new Date().toISOString()}.txt`;
              a.click();
            }}
          >
            Download Log
          </Button>
        )}
      </Box>
    </Box>
  );

  // Render Deploy New (existing wizard)
  const renderDeployNew = () => (
    <Grid container spacing={3}>
      {/* Stepper */}
      <Grid item xs={12} md={3}>
        <Paper sx={{ p: 2, borderRadius: 2 }}>
          <Stepper activeStep={activeStep} orientation="vertical">
            {steps.map((step, index) => (
              <Step key={step.label} completed={index < activeStep}>
                <StepLabel
                  onClick={() => index <= activeStep && setActiveStep(index)}
                  sx={{ cursor: index <= activeStep ? 'pointer' : 'default' }}
                >
                  {step.label}
                </StepLabel>
              </Step>
            ))}
          </Stepper>
        </Paper>
      </Grid>

      {/* Step Content */}
      <Grid item xs={12} md={9}>
        <Card sx={{ borderRadius: 3 }}>
          <CardContent>
            {steps[activeStep]?.content}

            {/* Navigation Buttons */}
            <Box sx={{ mt: 4, display: 'flex', justifyContent: 'space-between' }}>
              <Button
                disabled={activeStep === 0}
                onClick={() => setActiveStep(prev => prev - 1)}
              >
                Back
              </Button>
              {activeStep < steps.length - 1 && (
                <Button
                  variant="contained"
                  onClick={() => setActiveStep(prev => prev + 1)}
                  endIcon={<ArrowIcon />}
                  disabled={
                    (activeStep === 0 && !selectedProvider) ||
                    (activeStep === 1 && (!deploymentConfig.region || !deploymentConfig.databaseType))
                  }
                  sx={{ backgroundColor: '#6750A4' }}
                >
                  Next
                </Button>
              )}
            </Box>
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  );

  // Helper function to get status color
  const getStatusColor = (status: string) => {
    switch (status?.toLowerCase()) {
      case 'running': case 'healthy': case 'success': return 'success';
      case 'building': case 'deploying': case 'provisioning': case 'degraded': return 'warning';
      case 'failed': case 'error': case 'unhealthy': case 'offline': return 'error';
      case 'stopped': case 'terminated': return 'default';
      default: return 'info';
    }
  };

  // Render Dashboard
  const renderDashboard = () => (
    <Box>
      {loadingApi ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
          <CircularProgress />
        </Box>
      ) : apiError ? (
        <Alert severity="warning" sx={{ mb: 2 }}>
          {apiError}
        </Alert>
      ) : dashboard ? (
        <Grid container spacing={3}>
          {/* Summary Cards */}
          <Grid item xs={12} sm={6} md={3}>
            <Card sx={{ borderRadius: 2 }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                  <CloudIcon color="primary" />
                  <Typography variant="subtitle2" color="textSecondary">Providers</Typography>
                </Box>
                <Typography variant="h4">{dashboard.totalProviders}</Typography>
                <Typography variant="body2" color="success.main">
                  {dashboard.activeProviders} active
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card sx={{ borderRadius: 2 }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                  <StorageIcon color="secondary" />
                  <Typography variant="subtitle2" color="textSecondary">Deployments</Typography>
                </Box>
                <Typography variant="h4">{dashboard.totalDeployments}</Typography>
                <Typography variant="body2" color="success.main">
                  {dashboard.runningDeployments} running
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card sx={{ borderRadius: 2 }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                  <CheckIcon color="success" />
                  <Typography variant="subtitle2" color="textSecondary">Healthy</Typography>
                </Box>
                <Typography variant="h4">{dashboard.healthyDeployments}</Typography>
                <Typography variant="body2" color="textSecondary">
                  of {dashboard.totalDeployments} total
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card sx={{ borderRadius: 2 }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                  <ErrorIcon color="error" />
                  <Typography variant="subtitle2" color="textSecondary">Failed</Typography>
                </Box>
                <Typography variant="h4">{dashboard.failedDeployments}</Typography>
                <Typography variant="body2" color="error.main">
                  need attention
                </Typography>
              </CardContent>
            </Card>
          </Grid>

          {/* Deployments Table */}
          <Grid item xs={12}>
            <Card sx={{ borderRadius: 2 }}>
              <CardContent>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                  <Typography variant="h6">Active Deployments</Typography>
                  <Button startIcon={<RefreshIcon />} onClick={loadApiData} size="small">
                    Refresh
                  </Button>
                </Box>
                <TableContainer>
                  <Table>
                    <TableHead>
                      <TableRow>
                        <TableCell>Name</TableCell>
                        <TableCell>Provider</TableCell>
                        <TableCell>Status</TableCell>
                        <TableCell>Health</TableCell>
                        <TableCell>Frontend URL</TableCell>
                        <TableCell>Last Deployed</TableCell>
                        <TableCell align="right">Actions</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {apiDeployments.length === 0 ? (
                        <TableRow>
                          <TableCell colSpan={7} align="center">
                            <Typography color="textSecondary">No deployments configured</Typography>
                          </TableCell>
                        </TableRow>
                      ) : apiDeployments.map((deployment) => (
                        <TableRow key={deployment.id}>
                          <TableCell>
                            <Typography fontWeight={500}>{deployment.name}</Typography>
                            <Typography variant="caption" color="textSecondary">
                              v{deployment.backendVersion || 'N/A'} / v{deployment.frontendVersion || 'N/A'}
                            </Typography>
                          </TableCell>
                          <TableCell>
                            <Chip label={deployment.providerType} size="small" variant="outlined" />
                          </TableCell>
                          <TableCell>
                            <Chip 
                              label={deployment.status} 
                              size="small" 
                              color={getStatusColor(deployment.status) as any}
                            />
                          </TableCell>
                          <TableCell>
                            <Chip 
                              label={deployment.healthStatus} 
                              size="small" 
                              color={getStatusColor(deployment.healthStatus) as any}
                            />
                          </TableCell>
                          <TableCell>
                            {deployment.frontendUrl ? (
                              <a href={deployment.frontendUrl} target="_blank" rel="noopener noreferrer">
                                {deployment.frontendUrl}
                              </a>
                            ) : '-'}
                          </TableCell>
                          <TableCell>
                            {deployment.deployedAt ? new Date(deployment.deployedAt).toLocaleString() : 'Never'}
                          </TableCell>
                          <TableCell align="right">
                            <Tooltip title="Run Health Check">
                              <IconButton 
                                size="small" 
                                onClick={() => runHealthCheck(deployment.id)}
                                disabled={runningHealthCheck}
                              >
                                <HealthIcon />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Deploy">
                              <IconButton 
                                size="small" 
                                onClick={() => handleTriggerDeployment(deployment.id)}
                                disabled={triggeringDeployment}
                              >
                                <PlayIcon />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Stop">
                              <IconButton 
                                size="small" 
                                onClick={() => handleStopDeployment(deployment.id)}
                              >
                                <StopIcon />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Restart">
                              <IconButton 
                                size="small" 
                                onClick={() => handleRestartDeployment(deployment.id)}
                              >
                                <RestartIcon />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="View Attempts">
                              <IconButton 
                                size="small" 
                                onClick={() => {
                                  setSelectedDeployment(deployment);
                                  loadDeploymentAttempts(deployment.id);
                                  setMainTab(3);
                                }}
                              >
                                <HistoryIcon />
                              </IconButton>
                            </Tooltip>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      ) : (
        <Alert severity="info">
          No deployment data available. Configure your first cloud provider to get started.
        </Alert>
      )}
    </Box>
  );

  // Render Build Attempts
  const renderBuildAttempts = () => (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h6">Build & Deployment Attempts</Typography>
        <FormControl size="small" sx={{ minWidth: 200 }}>
          <InputLabel>Filter by Deployment</InputLabel>
          <Select
            value={selectedDeployment?.id || ''}
            onChange={(e) => {
              const deployment = apiDeployments.find(d => d.id === e.target.value);
              if (deployment) {
                setSelectedDeployment(deployment);
                loadDeploymentAttempts(deployment.id);
              } else {
                setSelectedDeployment(null);
                setDeploymentAttempts([]);
              }
            }}
            label="Filter by Deployment"
          >
            <MenuItem value="">All Deployments</MenuItem>
            {apiDeployments.map((d) => (
              <MenuItem key={d.id} value={d.id}>{d.name}</MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>

      <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Attempt #</TableCell>
              <TableCell>Deployment</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Branch</TableCell>
              <TableCell>Backend Tag</TableCell>
              <TableCell>Frontend Tag</TableCell>
              <TableCell>Started</TableCell>
              <TableCell>Duration</TableCell>
              <TableCell>Trigger</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {(selectedDeployment ? deploymentAttempts : dashboard?.recentAttempts || []).length === 0 ? (
              <TableRow>
                <TableCell colSpan={10} align="center">
                  <Typography color="textSecondary">
                    {selectedDeployment ? 'No attempts for this deployment' : 'No recent build attempts'}
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (selectedDeployment ? deploymentAttempts : dashboard?.recentAttempts || []).map((attempt: DeploymentAttempt | any) => (
              <TableRow key={attempt.id}>
                <TableCell>
                  <Chip label={`#${attempt.attemptNumber}`} size="small" variant="outlined" />
                </TableCell>
                <TableCell>{attempt.deploymentName || selectedDeployment?.name || '-'}</TableCell>
                <TableCell>
                  <Chip 
                    label={attempt.status} 
                    size="small" 
                    color={getStatusColor(attempt.status) as any}
                  />
                </TableCell>
                <TableCell>{attempt.gitBranch || '-'}</TableCell>
                <TableCell>
                  <Chip label={attempt.backendImageTag || 'N/A'} size="small" variant="outlined" />
                </TableCell>
                <TableCell>
                  <Chip label={attempt.frontendImageTag || 'N/A'} size="small" variant="outlined" />
                </TableCell>
                <TableCell>{new Date(attempt.startedAt).toLocaleString()}</TableCell>
                <TableCell>
                  {attempt.durationSeconds ? `${attempt.durationSeconds}s` : 'In progress...'}
                </TableCell>
                <TableCell>
                  <Chip label={attempt.triggerType || 'Manual'} size="small" />
                </TableCell>
                <TableCell align="right">
                  <Tooltip title="View Logs">
                    <IconButton size="small" onClick={() => viewAttemptLogs(attempt.id)}>
                      <TerminalIcon />
                    </IconButton>
                  </Tooltip>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Error messages */}
      {(selectedDeployment ? deploymentAttempts : dashboard?.recentAttempts || [])
        .filter((a: any) => a.errorMessage)
        .map((attempt: any) => (
          <Alert key={attempt.id} severity="error" sx={{ mt: 2 }}>
            <Typography variant="subtitle2">Attempt #{attempt.attemptNumber} Error:</Typography>
            <Typography variant="body2">{attempt.errorMessage}</Typography>
          </Alert>
        ))}
    </Box>
  );

  // Render Health Checks
  const renderHealthChecks = () => (
    <Box>
      <Grid container spacing={3}>
        {/* Health Check Controls */}
        <Grid item xs={12}>
          <Card sx={{ borderRadius: 2 }}>
            <CardContent>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Typography variant="h6">Run Health Check</Typography>
              </Box>
              <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel>Select Deployment</InputLabel>
                <Select
                  value={selectedDeployment?.id || ''}
                  onChange={(e) => {
                    const deployment = apiDeployments.find(d => d.id === e.target.value);
                    if (deployment) {
                      setSelectedDeployment(deployment);
                      loadHealthCheckHistory(deployment.id);
                    }
                  }}
                  label="Select Deployment"
                >
                  {apiDeployments.map((d) => (
                    <MenuItem key={d.id} value={d.id}>{d.name}</MenuItem>
                  ))}
                </Select>
              </FormControl>
              <Button
                variant="contained"
                startIcon={runningHealthCheck ? <CircularProgress size={20} /> : <HealthIcon />}
                disabled={!selectedDeployment || runningHealthCheck}
                onClick={() => selectedDeployment && runHealthCheck(selectedDeployment.id)}
                sx={{ backgroundColor: '#6750A4' }}
              >
                {runningHealthCheck ? 'Checking...' : 'Run Health Check Now'}
              </Button>
            </CardContent>
          </Card>
        </Grid>

        {/* Latest Health Check Result */}
        {healthCheckResult && (
          <Grid item xs={12}>
            <Card sx={{ borderRadius: 2, border: `2px solid`, borderColor: getStatusColor(healthCheckResult.overallStatus) + '.main' }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                  <Typography variant="h6">Latest Health Check Result</Typography>
                  <Chip 
                    label={healthCheckResult.overallStatus} 
                    color={getStatusColor(healthCheckResult.overallStatus) as any}
                  />
                </Box>
                <Typography variant="caption" color="textSecondary" sx={{ display: 'block', mb: 2 }}>
                  Checked at: {new Date(healthCheckResult.checkedAt).toLocaleString()}
                </Typography>
                
                <Grid container spacing={2}>
                  <Grid item xs={12} md={4}>
                    <Paper sx={{ p: 2, textAlign: 'center' }}>
                      <ApiIcon sx={{ fontSize: 40, color: healthCheckResult.api.healthy ? 'success.main' : 'error.main' }} />
                      <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>API</Typography>
                      <Chip 
                        label={healthCheckResult.api.healthy ? 'Healthy' : 'Unhealthy'} 
                        size="small"
                        color={healthCheckResult.api.healthy ? 'success' : 'error'}
                      />
                      <Typography variant="body2" sx={{ mt: 1 }}>
                        Response: {healthCheckResult.api.responseTimeMs}ms
                      </Typography>
                    </Paper>
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <Paper sx={{ p: 2, textAlign: 'center' }}>
                      <WebIcon sx={{ fontSize: 40, color: healthCheckResult.frontend.healthy ? 'success.main' : 'error.main' }} />
                      <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>Frontend</Typography>
                      <Chip 
                        label={healthCheckResult.frontend.healthy ? 'Healthy' : 'Unhealthy'} 
                        size="small"
                        color={healthCheckResult.frontend.healthy ? 'success' : 'error'}
                      />
                      <Typography variant="body2" sx={{ mt: 1 }}>
                        Response: {healthCheckResult.frontend.responseTimeMs}ms
                      </Typography>
                    </Paper>
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <Paper sx={{ p: 2, textAlign: 'center' }}>
                      <DatabaseIcon sx={{ fontSize: 40, color: healthCheckResult.database.healthy ? 'success.main' : 'error.main' }} />
                      <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>Database</Typography>
                      <Chip 
                        label={healthCheckResult.database.healthy ? 'Healthy' : 'Unhealthy'} 
                        size="small"
                        color={healthCheckResult.database.healthy ? 'success' : 'error'}
                      />
                      <Typography variant="body2" sx={{ mt: 1 }}>
                        Response: {healthCheckResult.database.responseTimeMs}ms
                      </Typography>
                    </Paper>
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Grid>
        )}

        {/* Health Check History */}
        <Grid item xs={12}>
          <Card sx={{ borderRadius: 2 }}>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2 }}>Health Check History</Typography>
              <TableContainer>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Checked At</TableCell>
                      <TableCell>Deployment</TableCell>
                      <TableCell>Overall Status</TableCell>
                      <TableCell>API</TableCell>
                      <TableCell>Frontend</TableCell>
                      <TableCell>Database</TableCell>
                      <TableCell>Response Times</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {healthChecks.length === 0 ? (
                      <TableRow>
                        <TableCell colSpan={7} align="center">
                          <Typography color="textSecondary">
                            {selectedDeployment ? 'No health checks recorded' : 'Select a deployment to view history'}
                          </Typography>
                        </TableCell>
                      </TableRow>
                    ) : healthChecks.map((check) => (
                      <TableRow key={check.id}>
                        <TableCell>{new Date(check.checkedAt).toLocaleString()}</TableCell>
                        <TableCell>{check.deploymentName}</TableCell>
                        <TableCell>
                          <Chip 
                            label={check.status} 
                            size="small" 
                            color={getStatusColor(check.status) as any}
                          />
                        </TableCell>
                        <TableCell>
                          {check.apiHealthy !== undefined && (
                            check.apiHealthy ? <CheckIcon color="success" /> : <ErrorIcon color="error" />
                          )}
                        </TableCell>
                        <TableCell>
                          {check.frontendHealthy !== undefined && (
                            check.frontendHealthy ? <CheckIcon color="success" /> : <ErrorIcon color="error" />
                          )}
                        </TableCell>
                        <TableCell>
                          {check.databaseHealthy !== undefined && (
                            check.databaseHealthy ? <CheckIcon color="success" /> : <ErrorIcon color="error" />
                          )}
                        </TableCell>
                        <TableCell>
                          <Typography variant="caption">
                            API: {check.apiResponseTimeMs || '-'}ms | 
                            FE: {check.frontendResponseTimeMs || '-'}ms | 
                            DB: {check.databaseResponseTimeMs || '-'}ms
                          </Typography>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );

  return (
    <Box>
      <Typography variant="h5" sx={{ fontWeight: 700, mb: 1 }}>
        Deployment & Hosting
      </Typography>
      <Typography color="textSecondary" sx={{ mb: 3 }}>
        Configure and deploy your CRM to cloud services or on-premises infrastructure
      </Typography>

      {/* Main Tabs */}
      <Paper sx={{ mb: 3, borderRadius: 2 }}>
        <Tabs
          value={mainTab}
          onChange={(_, v) => setMainTab(v)}
          variant="scrollable"
          scrollButtons="auto"
          sx={{ borderBottom: 1, borderColor: 'divider' }}
        >
          <Tab icon={<DashboardIcon />} iconPosition="start" label="Dashboard" />
          <Tab icon={<InfoIcon />} iconPosition="start" label="Current Hosting" />
          <Tab icon={<CloudIcon />} iconPosition="start" label="Deploy New" />
          <Tab icon={<HistoryIcon />} iconPosition="start" label={
            <Badge badgeContent={dashboard?.recentAttempts?.length || 0} color="primary" max={99}>
              <span>Build Attempts</span>
            </Badge>
          } />
          <Tab icon={<HealthIcon />} iconPosition="start" label="Health Checks" />
          <Tab icon={<SecurityIcon />} iconPosition="start" label="Credentials" />
          <Tab icon={<ScriptIcon />} iconPosition="start" label="Scripts" />
          <Tab icon={<SyncIcon />} iconPosition="start" label="Replicate" />
        </Tabs>
      </Paper>

      {/* Tab Content */}
      <Box sx={{ mt: 3 }}>
        {mainTab === 0 && renderDashboard()}
        {mainTab === 1 && renderCurrentHosting()}
        {mainTab === 2 && renderDeployNew()}
        {mainTab === 3 && renderBuildAttempts()}
        {mainTab === 4 && renderHealthChecks()}
        {mainTab === 5 && renderCredentials()}
        {mainTab === 6 && renderScriptGeneration()}
        {mainTab === 7 && renderDataReplication()}
      </Box>

      {/* Script Dialog */}
      <Dialog open={scriptDialogOpen} onClose={() => setScriptDialogOpen(false)} maxWidth="lg" fullWidth>
        <DialogTitle>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            {scriptType === 'docker' && <CloudIcon color="primary" />}
            {scriptType === 'kubernetes' && <KubernetesIcon color="secondary" />}
            {scriptType === 'terraform' && <DataIcon sx={{ color: '#7B42BC' }} />}
            {scriptType === 'shell' && <TerminalIcon />}
            {scriptType === 'docker' ? 'Docker Compose Configuration' :
             scriptType === 'kubernetes' ? 'Kubernetes Manifests' :
             scriptType === 'terraform' ? 'Terraform Configuration' :
             'Deployment Shell Script'}
          </Box>
        </DialogTitle>
        <DialogContent>
          <Paper 
            sx={{ 
              p: 2, 
              bgcolor: '#1e1e1e', 
              borderRadius: 2, 
              maxHeight: 500, 
              overflow: 'auto',
              fontFamily: 'monospace'
            }}
          >
            <pre style={{ margin: 0, color: '#d4d4d4', whiteSpace: 'pre-wrap', fontSize: '13px' }}>
              {generatedScript}
            </pre>
          </Paper>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setScriptDialogOpen(false)}>Close</Button>
          <Button
            startIcon={<CopyIcon />}
            onClick={() => {
              navigator.clipboard.writeText(generatedScript);
            }}
          >
            Copy to Clipboard
          </Button>
          <Button
            variant="contained"
            startIcon={<DownloadIcon />}
            onClick={() => {
              const extension = scriptType === 'docker' ? 'yml' : 
                               scriptType === 'kubernetes' ? 'yaml' :
                               scriptType === 'terraform' ? 'tf' : 'sh';
              const filename = scriptType === 'docker' ? 'docker-compose' :
                              scriptType === 'kubernetes' ? 'crm-deployment' :
                              scriptType === 'terraform' ? 'main' : 'deploy';
              const blob = new Blob([generatedScript], { type: 'text/plain' });
              const url = URL.createObjectURL(blob);
              const a = document.createElement('a');
              a.href = url;
              a.download = `${filename}.${extension}`;
              a.click();
            }}
          >
            Download
          </Button>
        </DialogActions>
      </Dialog>

      {/* Logs Dialog */}
      <Dialog open={showLogsDialog} onClose={() => setShowLogsDialog(false)} maxWidth="md" fullWidth>
        <DialogTitle>Deployment Logs</DialogTitle>
        <DialogContent>
          <Paper sx={{ p: 2, bgcolor: '#1e1e1e', borderRadius: 2, maxHeight: 400, overflow: 'auto' }}>
            {deploymentLogs.map((log, index) => (
              <Typography key={index} variant="body2" sx={{ color: '#00ff00', fontFamily: 'monospace', mb: 0.5 }}>
                {log}
              </Typography>
            ))}
          </Paper>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setShowLogsDialog(false)}>Close</Button>
          <Button
            startIcon={<CopyIcon />}
            onClick={() => navigator.clipboard.writeText(deploymentLogs.join('\n'))}
          >
            Copy Logs
          </Button>
        </DialogActions>
      </Dialog>

      {/* Attempt Logs Dialog */}
      <Dialog open={showAttemptLogsDialog} onClose={() => setShowAttemptLogsDialog(false)} maxWidth="lg" fullWidth>
        <DialogTitle>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <TerminalIcon />
            Build & Deploy Logs
          </Box>
        </DialogTitle>
        <DialogContent>
          <Paper 
            sx={{ 
              p: 2, 
              bgcolor: '#1e1e1e', 
              borderRadius: 2, 
              maxHeight: 500, 
              overflow: 'auto',
              fontFamily: 'monospace'
            }}
          >
            <pre style={{ margin: 0, color: '#00ff00', whiteSpace: 'pre-wrap', fontSize: '13px' }}>
              {attemptLogs || 'No logs available'}
            </pre>
          </Paper>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setShowAttemptLogsDialog(false)}>Close</Button>
          <Button
            startIcon={<CopyIcon />}
            onClick={() => navigator.clipboard.writeText(attemptLogs)}
          >
            Copy Logs
          </Button>
          <Button
            variant="contained"
            startIcon={<DownloadIcon />}
            onClick={() => {
              const blob = new Blob([attemptLogs], { type: 'text/plain' });
              const url = URL.createObjectURL(blob);
              const a = document.createElement('a');
              a.href = url;
              a.download = `deployment-logs-${new Date().toISOString()}.txt`;
              a.click();
            }}
          >
            Download
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default DeploymentSettingsTab;
