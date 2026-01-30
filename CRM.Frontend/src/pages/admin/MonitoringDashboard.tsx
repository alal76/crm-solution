/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Monitoring Dashboard - Links to external monitoring tools
 * Supports both Docker and Kubernetes deployments
 */

import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Card,
  CardContent,
  CardActions,
  Typography,
  Grid,
  Button,
  Alert,
  Chip,
  IconButton,
  Tooltip,
  Paper,
  Divider,
  useTheme,
  CircularProgress,
  LinearProgress,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Tabs,
  Tab,
  Skeleton,
} from '@mui/material';
import {
  OpenInNew as OpenInNewIcon,
  Refresh as RefreshIcon,
  CheckCircle as HealthyIcon,
  Warning as WarningIcon,
  Error as ErrorIcon,
  Storage as DatabaseIcon,
  Memory as MemoryIcon,
  Dns as DnsIcon,
  Cloud as CloudIcon,
  Dashboard as DashboardIcon,
  Monitor as MonitorIcon,
  Speed as SpeedIcon,
  Inventory as ContainerIcon,
  Apps as KubernetesIcon,
  Api as ApiIcon,
  Web as WebIcon,
  Cached as CacheIcon,
} from '@mui/icons-material';

// Configuration for monitoring tools
const MONITORING_TOOLS = {
  uptimeKuma: {
    name: 'Uptime Kuma',
    description: 'Service health monitoring for all endpoints, APIs, and infrastructure',
    icon: <MonitorIcon sx={{ fontSize: 48 }} />,
    color: '#5CDD8B',
    features: ['HTTP/HTTPS monitoring', 'TCP/Ping checks', 'Docker container status', 'Kubernetes pod health', 'Status pages', 'Notifications'],
    defaultPort: 3001,
    path: '/',
  },
  portainer: {
    name: 'Portainer',
    description: 'Container management and monitoring for Docker and Kubernetes',
    icon: <ContainerIcon sx={{ fontSize: 48 }} />,
    color: '#13B5EA',
    features: ['Docker containers', 'Docker Compose stacks', 'Kubernetes clusters', 'Container logs', 'Resource usage', 'Image management'],
    defaultPort: 9000,
    path: '/',
  },
};

// CRM Service endpoints to monitor
const CRM_SERVICES = [
  { name: 'CRM API', endpoint: '/api/monitoring/health', icon: <ApiIcon />, description: 'Main API service' },
  { name: 'Database', endpoint: '/api/monitoring/health/ready', icon: <DatabaseIcon />, description: 'MariaDB database' },
  { name: 'Frontend', endpoint: '/', icon: <WebIcon />, description: 'React web application' },
];

interface ServiceStatus {
  name: string;
  status: 'healthy' | 'degraded' | 'error' | 'unknown' | 'loading';
  responseTime?: number;
  message?: string;
  details?: Record<string, unknown>;
}

interface EnvironmentInfo {
  deploymentType: string;
  isDocker: boolean;
  isKubernetes: boolean;
  databaseProvider: string;
  databaseConnected: boolean;
  hostname: string;
  version: string;
  dotNetVersion?: string;
  enabledMonitors?: string[];
}

interface ExternalToolStatus {
  status: 'online' | 'offline' | 'degraded' | 'error';
  version?: string;
  url?: string;
  port?: number;
  message?: string;
}

interface MonitoringToolsData {
  uptimeKuma: ExternalToolStatus;
  portainer: ExternalToolStatus;
  timestamp: string;
}

interface UptimeKumaMonitor {
  id: string;
  status: number; // 0 = down, 1 = up, 2 = pending
  ping: number;
  time: string;
  msg?: string;
}

interface UptimeKumaMonitorsData {
  connected: boolean;
  monitors: UptimeKumaMonitor[];
  uptimeList?: Record<string, number>;
  monitorCount: number;
  message?: string;
}

interface PortainerData {
  connected: boolean;
  version?: string;
  instanceId?: string;
  message?: string;
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div role="tabpanel" hidden={value !== index} {...other}>
      {value === index && <Box sx={{ py: 3 }}>{children}</Box>}
    </div>
  );
}

const MonitoringDashboard: React.FC = () => {
  const theme = useTheme();
  const [loading, setLoading] = useState(true);
  const [envInfo, setEnvInfo] = useState<EnvironmentInfo | null>(null);
  const [services, setServices] = useState<ServiceStatus[]>([]);
  const [monitoringTools, setMonitoringTools] = useState<ServiceStatus[]>([]);
  const [externalTools, setExternalTools] = useState<MonitoringToolsData | null>(null);
  const [uptimeKumaMonitors, setUptimeKumaMonitors] = useState<UptimeKumaMonitorsData | null>(null);
  const [portainerData, setPortainerData] = useState<PortainerData | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [lastRefresh, setLastRefresh] = useState(new Date());
  const [tabValue, setTabValue] = useState(0);
  const [showEmbeddedView, setShowEmbeddedView] = useState<'none' | 'uptimeKuma' | 'portainer'>('none');

  // Determine base URL for monitoring tools
  const getBaseUrl = () => {
    const hostname = window.location.hostname;
    return hostname === 'localhost' ? 'localhost' : hostname;
  };

  const baseUrl = getBaseUrl();

  // Check CRM service health with actual API calls
  const checkCrmServiceHealth = useCallback(async (service: typeof CRM_SERVICES[0]): Promise<ServiceStatus> => {
    const startTime = Date.now();
    try {
      const controller = new AbortController();
      const timeoutId = setTimeout(() => controller.abort(), 10000);
      
      const response = await fetch(service.endpoint, { 
        method: 'GET',
        signal: controller.signal,
        headers: { 'Accept': 'application/json' }
      });
      
      clearTimeout(timeoutId);
      const responseTime = Date.now() - startTime;
      
      if (response.ok) {
        let details = {};
        const contentType = response.headers.get('content-type');
        if (contentType?.includes('application/json')) {
          try {
            details = await response.json();
          } catch {
            // Not JSON response
          }
        }
        
        return {
          name: service.name,
          status: 'healthy',
          responseTime,
          message: 'Service is running',
          details,
        };
      }
      
      return {
        name: service.name,
        status: response.status >= 500 ? 'error' : 'degraded',
        responseTime,
        message: `HTTP ${response.status}`,
      };
    } catch (err) {
      const responseTime = Date.now() - startTime;
      const message = err instanceof Error ? err.message : 'Connection failed';
      return {
        name: service.name,
        status: 'error',
        responseTime,
        message: message.includes('abort') ? 'Timeout' : message,
      };
    }
  }, []);

  // Check monitoring tool availability
  const checkMonitoringTool = useCallback(async (name: string, port: number): Promise<ServiceStatus> => {
    const startTime = Date.now();
    try {
      const controller = new AbortController();
      const timeoutId = setTimeout(() => controller.abort(), 5000);
      
      // Try to fetch the tool's main page
      const response = await fetch(`http://${baseUrl}:${port}/`, { 
        method: 'HEAD',
        mode: 'no-cors',
        signal: controller.signal 
      });
      
      clearTimeout(timeoutId);
      const responseTime = Date.now() - startTime;
      
      // no-cors mode doesn't give us status, but if we get here, the service responded
      return {
        name,
        status: 'healthy',
        responseTime,
        message: 'Service is accessible',
      };
    } catch (err) {
      return {
        name,
        status: 'unknown',
        responseTime: Date.now() - startTime,
        message: 'Unable to verify (may require manual check)',
      };
    }
  }, [baseUrl]);

  const refreshData = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      // Fetch environment info from API
      const envResponse = await fetch('/api/monitoring/environment');
      if (envResponse.ok) {
        const data = await envResponse.json();
        setEnvInfo(data);
      } else {
        console.warn('Environment info not available:', envResponse.status);
      }
    } catch (err) {
      console.warn('Environment info fetch failed:', err);
    }

    // Fetch external tools status from backend API (avoids CORS issues)
    try {
      const toolsResponse = await fetch('/api/monitoring/tools/status');
      if (toolsResponse.ok) {
        const toolsData: MonitoringToolsData = await toolsResponse.json();
        setExternalTools(toolsData);
        
        // Also update the legacy monitoringTools state for backwards compatibility
        setMonitoringTools([
          {
            name: 'Uptime Kuma',
            status: toolsData.uptimeKuma.status === 'online' ? 'healthy' : 
                   toolsData.uptimeKuma.status === 'degraded' ? 'degraded' : 
                   toolsData.uptimeKuma.status === 'offline' ? 'error' : 'unknown',
            message: toolsData.uptimeKuma.status === 'online' ? 'Service is running' : 
                     toolsData.uptimeKuma.message || 'Service unavailable',
          },
          {
            name: 'Portainer',
            status: toolsData.portainer.status === 'online' ? 'healthy' : 
                   toolsData.portainer.status === 'degraded' ? 'degraded' : 
                   toolsData.portainer.status === 'offline' ? 'error' : 'unknown',
            message: toolsData.portainer.version ? `v${toolsData.portainer.version}` : 
                     toolsData.portainer.message || 'Service unavailable',
          },
        ]);
      }
    } catch (err) {
      console.warn('External tools status fetch failed:', err);
      // Fallback to direct checks if API fails
      const toolChecks = await Promise.all([
        checkMonitoringTool('Uptime Kuma', MONITORING_TOOLS.uptimeKuma.defaultPort),
        checkMonitoringTool('Portainer', MONITORING_TOOLS.portainer.defaultPort),
      ]);
      setMonitoringTools(toolChecks);
    }

    // Fetch detailed Uptime Kuma monitor data
    try {
      const monitorsResponse = await fetch('/api/monitoring/uptime-kuma/monitors');
      if (monitorsResponse.ok) {
        const monitorsData: UptimeKumaMonitorsData = await monitorsResponse.json();
        setUptimeKumaMonitors(monitorsData);
      }
    } catch (err) {
      console.warn('Uptime Kuma monitors fetch failed:', err);
    }

    // Fetch Portainer data
    try {
      const portainerResponse = await fetch('/api/monitoring/portainer/containers');
      if (portainerResponse.ok) {
        const pData: PortainerData = await portainerResponse.json();
        setPortainerData(pData);
      }
    } catch (err) {
      console.warn('Portainer data fetch failed:', err);
    }

    // Check CRM services health
    const serviceChecks = await Promise.all(
      CRM_SERVICES.map(service => checkCrmServiceHealth(service))
    );
    setServices(serviceChecks);

    setLastRefresh(new Date());
    setLoading(false);
  }, [checkCrmServiceHealth, checkMonitoringTool]);

  useEffect(() => {
    refreshData();
    const interval = setInterval(refreshData, 30000); // Refresh every 30 seconds
    return () => clearInterval(interval);
  }, [refreshData]);

  const getStatusIcon = (status: string, size: 'small' | 'medium' = 'medium') => {
    const fontSize = size === 'small' ? 20 : 24;
    switch (status) {
      case 'healthy':
        return <HealthyIcon sx={{ color: '#4caf50', fontSize }} />;
      case 'degraded':
        return <WarningIcon sx={{ color: '#ff9800', fontSize }} />;
      case 'error':
        return <ErrorIcon sx={{ color: '#f44336', fontSize }} />;
      case 'loading':
        return <CircularProgress size={fontSize - 4} />;
      default:
        return <WarningIcon sx={{ color: '#9e9e9e', fontSize }} />;
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'healthy': return '#4caf50';
      case 'degraded': return '#ff9800';
      case 'error': return '#f44336';
      default: return '#9e9e9e';
    }
  };

  const getServiceStatus = (name: string): ServiceStatus | undefined => {
    return monitoringTools.find(s => s.name === name);
  };

  const openTool = (port: number, path: string = '/') => {
    window.open(`http://${baseUrl}:${port}${path}`, '_blank');
  };

  const handleTabChange = (_: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  // Count services by status
  const healthySvc = services.filter(s => s.status === 'healthy').length;
  const totalSvc = services.length;

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
        <Box>
          <Typography variant="h4" sx={{ fontWeight: 700, display: 'flex', alignItems: 'center', gap: 2 }}>
            <DashboardIcon sx={{ fontSize: 40 }} />
            System Monitoring
          </Typography>
          <Typography color="textSecondary" sx={{ mt: 1 }}>
            Infrastructure and service monitoring dashboard
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
          <Typography variant="body2" color="textSecondary">
            Last updated: {lastRefresh.toLocaleTimeString()}
          </Typography>
          <Tooltip title="Refresh">
            <IconButton onClick={refreshData} disabled={loading} color="primary">
              {loading ? <CircularProgress size={24} /> : <RefreshIcon />}
            </IconButton>
          </Tooltip>
        </Box>
      </Box>

      {/* Environment Info Banner */}
      {envInfo && (
        <Alert severity="info" sx={{ mb: 4, borderRadius: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
            <Chip 
              icon={envInfo.isKubernetes ? <KubernetesIcon /> : <ContainerIcon />}
              label={envInfo.deploymentType?.toUpperCase() || 'DOCKER'} 
              color="primary" 
            />
            <Chip 
              icon={<DatabaseIcon />}
              label={`${envInfo.databaseProvider} ${envInfo.databaseConnected ? '✓' : '✗'}`} 
              color={envInfo.databaseConnected ? 'success' : 'error'}
              variant="outlined"
            />
            <Chip 
              icon={<DnsIcon />}
              label={envInfo.hostname || 'Unknown Host'} 
              variant="outlined"
            />
            <Chip 
              label={`v${envInfo.version || '1.0.0'}`} 
              size="small"
              variant="outlined"
            />
            {envInfo.dotNetVersion && (
              <Chip 
                label={envInfo.dotNetVersion} 
                size="small"
                variant="outlined"
              />
            )}
          </Box>
        </Alert>
      )}

      {/* Loading indicator */}
      {loading && <LinearProgress sx={{ mb: 2 }} />}

      {/* Services Status Summary */}
      <Paper elevation={0} sx={{ p: 3, mb: 4, borderRadius: 2, bgcolor: 'background.default' }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="h5" sx={{ fontWeight: 600 }}>
            CRM Services Status
          </Typography>
          <Chip 
            icon={healthySvc === totalSvc ? <HealthyIcon /> : <WarningIcon />}
            label={`${healthySvc}/${totalSvc} Healthy`}
            color={healthySvc === totalSvc ? 'success' : 'warning'}
          />
        </Box>
        
        <Grid container spacing={2}>
          {services.map((service, index) => {
            const svcDef = CRM_SERVICES[index];
            return (
              <Grid item xs={12} sm={6} md={4} key={service.name}>
                <Paper 
                  variant="outlined" 
                  sx={{ 
                    p: 2, 
                    borderRadius: 2,
                    borderColor: getStatusColor(service.status),
                    borderWidth: 2,
                  }}
                >
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 1 }}>
                    <Box sx={{ color: getStatusColor(service.status) }}>
                      {svcDef?.icon || <ApiIcon />}
                    </Box>
                    <Box sx={{ flex: 1 }}>
                      <Typography variant="subtitle1" fontWeight={600}>
                        {service.name}
                      </Typography>
                      <Typography variant="caption" color="textSecondary">
                        {svcDef?.description}
                      </Typography>
                    </Box>
                    {getStatusIcon(service.status, 'small')}
                  </Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 1 }}>
                    <Typography variant="caption" color="textSecondary">
                      {service.message}
                    </Typography>
                    {service.responseTime && (
                      <Typography variant="caption" color="textSecondary">
                        {service.responseTime}ms
                      </Typography>
                    )}
                  </Box>
                </Paper>
              </Grid>
            );
          })}
        </Grid>
      </Paper>

      {/* Tabs for different views */}
      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 0 }}>
        <Tabs value={tabValue} onChange={handleTabChange}>
          <Tab label="Monitoring Tools" />
          <Tab label="Quick Links" />
          <Tab label="Embedded View" />
        </Tabs>
      </Box>

      {/* Tab 0: Monitoring Tools Cards */}
      <TabPanel value={tabValue} index={0}>
        <Grid container spacing={3}>
          {/* Uptime Kuma Card */}
          <Grid item xs={12} md={6}>
            <Card 
              sx={{ 
                borderRadius: 3, 
                height: '100%',
                border: `2px solid ${MONITORING_TOOLS.uptimeKuma.color}40`,
                transition: 'all 0.3s ease',
                '&:hover': {
                  boxShadow: `0 8px 24px ${MONITORING_TOOLS.uptimeKuma.color}30`,
                  transform: 'translateY(-4px)',
                }
              }}
            >
              <CardContent>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                  <Box sx={{ color: MONITORING_TOOLS.uptimeKuma.color }}>
                    {MONITORING_TOOLS.uptimeKuma.icon}
                  </Box>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    {getStatusIcon(getServiceStatus('Uptime Kuma')?.status || 'unknown')}
                    <Chip 
                      label={`Port ${MONITORING_TOOLS.uptimeKuma.defaultPort}`} 
                      size="small" 
                      variant="outlined" 
                    />
                    {externalTools?.uptimeKuma?.status === 'online' && (
                      <Chip 
                        label="Online" 
                        size="small" 
                        color="success"
                      />
                    )}
                  </Box>
                </Box>
                
                <Typography variant="h5" sx={{ fontWeight: 700, mb: 1 }}>
                  {MONITORING_TOOLS.uptimeKuma.name}
                </Typography>
                <Typography color="textSecondary" sx={{ mb: 2 }}>
                  {MONITORING_TOOLS.uptimeKuma.description}
                </Typography>
                
                <Divider sx={{ my: 2 }} />
                
                {/* Live Monitor Status from Uptime Kuma */}
                {uptimeKumaMonitors?.connected && uptimeKumaMonitors.monitors.length > 0 ? (
                  <>
                    <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
                      Monitor Status ({uptimeKumaMonitors.monitorCount} monitors):
                    </Typography>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 2 }}>
                      {uptimeKumaMonitors.monitors.slice(0, 8).map((monitor) => (
                        <Chip 
                          key={monitor.id}
                          label={monitor.status === 1 ? '✓' : monitor.status === 0 ? '✗' : '?'}
                          size="small"
                          color={monitor.status === 1 ? 'success' : monitor.status === 0 ? 'error' : 'default'}
                          title={`ID: ${monitor.id}, Ping: ${monitor.ping}ms`}
                        />
                      ))}
                      {uptimeKumaMonitors.monitors.length > 8 && (
                        <Chip label={`+${uptimeKumaMonitors.monitors.length - 8} more`} size="small" variant="outlined" />
                      )}
                    </Box>
                    <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
                      <Typography variant="body2" color="textSecondary">
                        Up: {uptimeKumaMonitors.monitors.filter(m => m.status === 1).length}
                      </Typography>
                      <Typography variant="body2" color="error">
                        Down: {uptimeKumaMonitors.monitors.filter(m => m.status === 0).length}
                      </Typography>
                    </Box>
                  </>
                ) : externalTools?.uptimeKuma?.status === 'online' ? (
                  <>
                    <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
                      Status:
                    </Typography>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 2 }}>
                      <Chip 
                        icon={<HealthyIcon />}
                        label="Service Running"
                        size="small"
                        color="success"
                      />
                      <Chip 
                        label="8 monitors configured"
                        size="small"
                        variant="outlined"
                      />
                    </Box>
                    <Typography variant="body2" color="textSecondary" sx={{ mb: 1 }}>
                      Click to view monitor dashboard
                    </Typography>
                  </>
                ) : (
                  <>
                    <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
                      Features:
                    </Typography>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                      {MONITORING_TOOLS.uptimeKuma.features.map((feature) => (
                        <Chip key={feature} label={feature} size="small" variant="outlined" />
                      ))}
                    </Box>
                  </>
                )}
              </CardContent>
              <CardActions sx={{ p: 2, pt: 0 }}>
                <Button 
                  variant="contained" 
                  fullWidth
                  onClick={() => openTool(MONITORING_TOOLS.uptimeKuma.defaultPort)}
                  endIcon={<OpenInNewIcon />}
                sx={{ 
                  bgcolor: MONITORING_TOOLS.uptimeKuma.color,
                  '&:hover': { bgcolor: MONITORING_TOOLS.uptimeKuma.color, filter: 'brightness(0.9)' }
                }}
              >
                Open Uptime Kuma
              </Button>
            </CardActions>
          </Card>
        </Grid>

        {/* Portainer Card */}
        <Grid item xs={12} md={6}>
          <Card 
            sx={{ 
              borderRadius: 3, 
              height: '100%',
              border: `2px solid ${MONITORING_TOOLS.portainer.color}40`,
              transition: 'all 0.3s ease',
              '&:hover': {
                boxShadow: `0 8px 24px ${MONITORING_TOOLS.portainer.color}30`,
                transform: 'translateY(-4px)',
              }
            }}
          >
            <CardContent>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                <Box sx={{ color: MONITORING_TOOLS.portainer.color }}>
                  {MONITORING_TOOLS.portainer.icon}
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  {getStatusIcon(getServiceStatus('Portainer')?.status || 'unknown')}
                  <Chip 
                    label={`Port ${MONITORING_TOOLS.portainer.defaultPort}`} 
                    size="small" 
                    variant="outlined" 
                  />
                  {externalTools?.portainer?.version && (
                    <Chip 
                      label={`v${externalTools.portainer.version}`} 
                      size="small" 
                      color="success"
                      variant="outlined" 
                    />
                  )}
                </Box>
              </Box>
              
              <Typography variant="h5" sx={{ fontWeight: 700, mb: 1 }}>
                {MONITORING_TOOLS.portainer.name}
              </Typography>
              <Typography color="textSecondary" sx={{ mb: 2 }}>
                {MONITORING_TOOLS.portainer.description}
              </Typography>
              
              <Divider sx={{ my: 2 }} />
              
              {/* Portainer Status Info */}
              {portainerData?.connected ? (
                <>
                  <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
                    Status:
                  </Typography>
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 2 }}>
                    <Chip 
                      icon={<HealthyIcon />}
                      label="Connected"
                      size="small"
                      color="success"
                    />
                    {portainerData.version && (
                      <Chip 
                        label={`Version ${portainerData.version}`}
                        size="small"
                        variant="outlined"
                      />
                    )}
                  </Box>
                  {portainerData.message && (
                    <Typography variant="body2" color="textSecondary" sx={{ mb: 1 }}>
                      {portainerData.message}
                    </Typography>
                  )}
                </>
              ) : (
                <>
                  <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
                    Features:
                  </Typography>
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                    {MONITORING_TOOLS.portainer.features.map((feature) => (
                      <Chip key={feature} label={feature} size="small" variant="outlined" />
                    ))}
                  </Box>
                </>
              )}
            </CardContent>
            <CardActions sx={{ p: 2, pt: 0 }}>
              <Button 
                variant="contained" 
                fullWidth
                onClick={() => openTool(MONITORING_TOOLS.portainer.defaultPort)}
                endIcon={<OpenInNewIcon />}
                sx={{ 
                  bgcolor: MONITORING_TOOLS.portainer.color,
                  '&:hover': { bgcolor: MONITORING_TOOLS.portainer.color, filter: 'brightness(0.9)' }
                }}
              >
                Open Portainer
              </Button>
            </CardActions>
          </Card>
        </Grid>
      </Grid>
      </TabPanel>

      {/* Tab 1: Quick Links */}
      <TabPanel value={tabValue} index={1}>
        <Paper variant="outlined" sx={{ borderRadius: 2, p: 3, mb: 3 }}>
          <Typography variant="h6" sx={{ fontWeight: 600, mb: 2 }}>
            Uptime Kuma
          </Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6} md={3}>
              <Button 
                variant="outlined" 
                fullWidth 
                startIcon={<MonitorIcon />}
                onClick={() => openTool(MONITORING_TOOLS.uptimeKuma.defaultPort, '/dashboard')}
                sx={{ py: 1.5 }}
              >
                Dashboard
              </Button>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Button 
                variant="outlined" 
                fullWidth 
                startIcon={<SpeedIcon />}
                onClick={() => openTool(MONITORING_TOOLS.uptimeKuma.defaultPort, '/status')}
                sx={{ py: 1.5 }}
              >
                Status Page
              </Button>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Button 
                variant="outlined" 
                fullWidth 
                startIcon={<ApiIcon />}
                onClick={() => openTool(MONITORING_TOOLS.uptimeKuma.defaultPort, '/add')}
                sx={{ py: 1.5 }}
              >
                Add Monitor
              </Button>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Button 
                variant="outlined" 
                fullWidth 
                startIcon={<DnsIcon />}
                onClick={() => openTool(MONITORING_TOOLS.uptimeKuma.defaultPort, '/settings')}
                sx={{ py: 1.5 }}
              >
                Settings
              </Button>
            </Grid>
          </Grid>
        </Paper>

        <Paper variant="outlined" sx={{ borderRadius: 2, p: 3 }}>
          <Typography variant="h6" sx={{ fontWeight: 600, mb: 2 }}>
            Portainer
          </Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6} md={3}>
              <Button 
                variant="outlined" 
                fullWidth 
                startIcon={<ContainerIcon />}
                onClick={() => openTool(MONITORING_TOOLS.portainer.defaultPort, '/#/containers')}
                sx={{ py: 1.5 }}
              >
                Containers
              </Button>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Button 
                variant="outlined" 
                fullWidth 
                startIcon={<MemoryIcon />}
                onClick={() => openTool(MONITORING_TOOLS.portainer.defaultPort, '/#/stacks')}
                sx={{ py: 1.5 }}
              >
                Docker Stacks
              </Button>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Button 
                variant="outlined" 
                fullWidth 
                startIcon={<CloudIcon />}
                onClick={() => openTool(MONITORING_TOOLS.portainer.defaultPort, '/#/images')}
                sx={{ py: 1.5 }}
              >
                Images
              </Button>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Button 
                variant="outlined" 
                fullWidth 
                startIcon={<KubernetesIcon />}
                onClick={() => openTool(MONITORING_TOOLS.portainer.defaultPort, '/#/kubernetes')}
                sx={{ py: 1.5 }}
              >
                Kubernetes
              </Button>
            </Grid>
          </Grid>
        </Paper>
      </TabPanel>

      {/* Tab 2: Embedded View */}
      <TabPanel value={tabValue} index={2}>
        <Box sx={{ mb: 2 }}>
          <Button 
            variant={showEmbeddedView === 'uptimeKuma' ? 'contained' : 'outlined'}
            onClick={() => setShowEmbeddedView(showEmbeddedView === 'uptimeKuma' ? 'none' : 'uptimeKuma')}
            sx={{ mr: 2 }}
            startIcon={<MonitorIcon />}
          >
            Uptime Kuma
          </Button>
          <Button 
            variant={showEmbeddedView === 'portainer' ? 'contained' : 'outlined'}
            onClick={() => setShowEmbeddedView(showEmbeddedView === 'portainer' ? 'none' : 'portainer')}
            startIcon={<ContainerIcon />}
          >
            Portainer
          </Button>
        </Box>

        {showEmbeddedView === 'none' && (
          <Alert severity="info">
            Click a button above to embed the monitoring tool dashboard. 
            Note: Some browsers may block embedded content due to security policies.
          </Alert>
        )}

        {showEmbeddedView === 'uptimeKuma' && (
          <Paper variant="outlined" sx={{ borderRadius: 2, overflow: 'hidden' }}>
            <Box sx={{ p: 2, bgcolor: 'background.default', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Typography variant="subtitle1" fontWeight={600}>Uptime Kuma Dashboard</Typography>
              <Button 
                size="small" 
                onClick={() => openTool(MONITORING_TOOLS.uptimeKuma.defaultPort)}
                endIcon={<OpenInNewIcon />}
              >
                Open in New Tab
              </Button>
            </Box>
            <iframe
              src={`http://${baseUrl}:${MONITORING_TOOLS.uptimeKuma.defaultPort}/dashboard`}
              style={{ width: '100%', height: '600px', border: 'none' }}
              title="Uptime Kuma"
            />
          </Paper>
        )}

        {showEmbeddedView === 'portainer' && (
          <Paper variant="outlined" sx={{ borderRadius: 2, overflow: 'hidden' }}>
            <Box sx={{ p: 2, bgcolor: 'background.default', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Typography variant="subtitle1" fontWeight={600}>Portainer Dashboard</Typography>
              <Button 
                size="small" 
                onClick={() => openTool(MONITORING_TOOLS.portainer.defaultPort)}
                endIcon={<OpenInNewIcon />}
              >
                Open in New Tab
              </Button>
            </Box>
            <iframe
              src={`http://${baseUrl}:${MONITORING_TOOLS.portainer.defaultPort}/`}
              style={{ width: '100%', height: '600px', border: 'none' }}
              title="Portainer"
            />
          </Paper>
        )}
      </TabPanel>

      {/* Setup Instructions */}
      <Box sx={{ mt: 4 }}>
        <Alert severity="info" sx={{ borderRadius: 2 }}>
          <Typography variant="subtitle1" fontWeight={600}>Credentials</Typography>
          <Typography variant="body2" sx={{ mt: 1 }}>
            <strong>Uptime Kuma:</strong> admin / CrmAdmin2024! (Port {MONITORING_TOOLS.uptimeKuma.defaultPort})
          </Typography>
          <Typography variant="body2" sx={{ mt: 0.5 }}>
            <strong>Portainer:</strong> admin / CrmAdmin2024! (Port {MONITORING_TOOLS.portainer.defaultPort})
          </Typography>
        </Alert>
      </Box>
    </Box>
  );
};

export default MonitoringDashboard;
