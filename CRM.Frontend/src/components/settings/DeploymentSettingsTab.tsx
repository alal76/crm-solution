import { useState } from 'react';
import {
  Box, Typography, Card, CardContent, Grid, Button, TextField,
  FormControl, InputLabel, Select, MenuItem, Chip, Stepper, Step,
  StepLabel, StepContent, Alert, CircularProgress, Paper, Divider,
  List, ListItem, ListItemIcon, ListItemText, IconButton, Tooltip,
  Dialog, DialogTitle, DialogContent, DialogActions, Switch, FormControlLabel
} from '@mui/material';
import {
  Cloud as CloudIcon, Storage as StorageIcon, Api as ApiIcon,
  Web as WebIcon, CheckCircle as CheckIcon, Error as ErrorIcon,
  PlayArrow as PlayIcon, Settings as SettingsIcon, Refresh as RefreshIcon,
  CloudQueue as AzureIcon, Code as AwsIcon, Computer as GoogleIcon,
  Dns as KubernetesIcon, Storage as DatabaseIcon, Security as SecurityIcon,
  ArrowForward as ArrowIcon, ContentCopy as CopyIcon
} from '@mui/icons-material';

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

  return (
    <Box>
      <Typography variant="h5" sx={{ fontWeight: 700, mb: 1 }}>
        Deployment & Hosting
      </Typography>
      <Typography color="textSecondary" sx={{ mb: 3 }}>
        Configure and deploy your CRM to cloud services or on-premises infrastructure
      </Typography>

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
    </Box>
  );
}

export default DeploymentSettingsTab;
