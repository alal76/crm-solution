/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Comprehensive Monitoring Dashboard - Environment & Infrastructure Aware
 * Automatically adapts display based on detected deployment type (Docker/K8s/VM)
 */

import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  Alert,
  Chip,
  LinearProgress,
  IconButton,
  Tooltip,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Divider,
  CircularProgress,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Skeleton,
  Badge,
  Avatar,
  Stack,
  useTheme,
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  CheckCircle as HealthyIcon,
  Warning as WarningIcon,
  Error as ErrorIcon,
  Storage as DatabaseIcon,
  Memory as MemoryIcon,
  Speed as CpuIcon,
  Cloud as CloudIcon,
  Person as PersonIcon,
  Computer as ServerIcon,
  Api as ApiIcon,
  ExpandMore as ExpandMoreIcon,
  Schedule as ScheduleIcon,
  DataUsage as DataUsageIcon,
  Circle as CircleIcon,
  Hub as HubIcon,
  ViewInAr as ContainerIcon,
  AccountTree as KubernetesIcon,
  NetworkCheck as NetworkIcon,
  FolderOpen as DiskIcon,
  Settings as SettingsIcon,
  Info as InfoIcon,
} from '@mui/icons-material';

// ============================================================================
// TYPES & INTERFACES
// ============================================================================

interface EnvironmentInfo {
  deploymentType: string;
  isDocker: boolean;
  isKubernetes: boolean;
  databaseProvider: string;
  databaseConnected: boolean;
  hostname: string;
  version: string;
  dotNetVersion: string;
  enabledMonitors: string[];
  timestamp: string;
}

interface InfrastructureInfo {
  timestamp: string;
  deploymentType: number;
  deploymentTypeName: string;
  database: {
    provider: number;
    providerName: string;
    version: string;
    host: string;
    port: number;
    isConnected: boolean;
    edition: string;
    collation: string;
  };
  host: {
    hostname: string;
    fqdn: string;
    osDescription: string;
    architecture: string;
    processorCount: number;
    totalMemoryMB: number;
    dotNetVersion: string;
    isDocker: boolean;
    isKubernetes: boolean;
  };
  activeMonitors: string[];
  availableMonitors: string[];
}

interface SystemMetrics {
  timestamp: string;
  cpu: { usagePercent: number; processorCount: number; processCpuPercent: number };
  memory: { totalMB: number; usedMB: number; freeMB: number; usagePercent: number; processWorkingSetMB: number };
  disk: { drives: Array<{ name: string; mountPoint: string; totalGB: number; usedGB: number; freeGB: number; usagePercent: number }>; totalSpaceGB: number; usedSpaceGB: number; freeSpaceGB: number; usagePercent: number };
  network: { bytesReceived: number; bytesSent: number; interfaces: Array<{ name: string; ipAddress: string; isUp: boolean; bytesReceived: number; bytesSent: number }> };
  process: { threadCount: number; handleCount: number; workingSetMB: number; privateMemoryMB: number; cpuTimeMs: number; uptimeFormatted: string };
}

interface DatabaseMetrics {
  timestamp: string;
  provider: number;
  providerName: string;
  isHealthy: boolean;
  responseTimeMs: number;
  activeConnections: number;
  databaseSizeMB: number;
  version: string;
  providerSpecificMetrics: Record<string, unknown>;
}

interface ServiceHealth {
  name: string;
  type: string;
  status: string;
  endpoint: string;
  responseTimeMs: number;
  version: string;
  lastCheck: string;
  uptime: string;
  metadata: Record<string, unknown>;
}

interface ContainerHealth {
  containerId: string;
  containerName: string;
  image: string;
  status: string;
  health: string;
  startedAt: string;
  uptime: string;
  cpuPercent: number;
  memoryMB: number;
  memoryLimitMB: number;
  memoryPercent: number;
}

interface PodHealth {
  podName: string;
  namespace: string;
  phase: string;
  ready: boolean;
  restartCount: number;
  nodeName: string;
  podIP: string;
  startedAt: string;
  uptime: string;
  containers: ContainerHealth[];
}

interface UserSession {
  userId: string;
  userName: string;
  email: string;
  role: string;
  loginTime: string;
  lastActivity: string;
  ipAddress: string;
  isActive: boolean;
}

interface MonitoringData {
  timestamp: string;
  infrastructure: InfrastructureInfo;
  system: SystemMetrics;
  database: DatabaseMetrics;
  services: ServiceHealth[];
  containers: ContainerHealth[];
  pods: PodHealth[];
  activeSessions: UserSession[];
}

// ============================================================================
// CONSTANTS
// ============================================================================

const STATUS_COLORS = {
  healthy: '#4caf50',
  running: '#4caf50',
  degraded: '#ff9800',
  warning: '#ff9800',
  error: '#f44336',
  stopped: '#f44336',
  unknown: '#9e9e9e',
} as const;

const DEPLOYMENT_ICONS: Record<string, React.ReactNode> = {
  docker: <ContainerIcon />,
  kubernetes: <KubernetesIcon />,
  vm: <ServerIcon />,
  hybrid: <HubIcon />,
};

const DATABASE_ICONS: Record<string, string> = {
  mariadb: '🐬',
  mysql: '🐬',
  sqlserver: '🗄️',
  postgresql: '🐘',
  mongodb: '🍃',
  oracle: '🔴',
};

// ============================================================================
// HELPER COMPONENTS
// ============================================================================

interface StatusCardProps {
  icon: React.ReactNode;
  title: string;
  value: string | number;
  subtitle?: string;
  color: string;
  bgColor: string;
}

const StatusCard: React.FC<StatusCardProps> = ({ icon, title, value, subtitle, color, bgColor }) => (
  <Card sx={{ borderRadius: 2, bgcolor: bgColor, height: '100%', transition: 'transform 0.2s', '&:hover': { transform: 'translateY(-2px)' } }}>
    <CardContent sx={{ textAlign: 'center', py: 2 }}>
      <Box sx={{ color, mb: 1, '& svg': { fontSize: 32 } }}>{icon}</Box>
      <Typography variant="h4" fontWeight={700} color={color}>{value}</Typography>
      <Typography variant="body2" fontWeight={600} color="textSecondary">{title}</Typography>
      {subtitle && <Typography variant="caption" color="textSecondary">{subtitle}</Typography>}
    </CardContent>
  </Card>
);

interface MetricProgressProps {
  label: string;
  value: number;
  total?: number;
  unit?: string;
  showPercent?: boolean;
}

const MetricProgress: React.FC<MetricProgressProps> = ({ label, value, total, unit = '', showPercent = true }) => {
  const percent = total ? Math.min(100, (value / total) * 100) : value;
  const getColor = (p: number) => p < 50 ? '#4caf50' : p < 75 ? '#ff9800' : '#f44336';

  return (
    <Box sx={{ mb: 2 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
        <Typography variant="body2" fontWeight={500}>{label}</Typography>
        <Typography variant="body2" fontWeight={600}>
          {total ? `${value.toFixed(1)}${unit} / ${total.toFixed(1)}${unit}` : showPercent ? `${value.toFixed(1)}%` : `${value}${unit}`}
        </Typography>
      </Box>
      <LinearProgress
        variant="determinate"
        value={Math.min(100, percent)}
        sx={{
          height: 8,
          borderRadius: 4,
          bgcolor: '#e0e0e0',
          '& .MuiLinearProgress-bar': { bgcolor: getColor(percent), borderRadius: 4 },
        }}
      />
    </Box>
  );
};

const StatusChip: React.FC<{ status: string }> = ({ status }) => {
  const color = STATUS_COLORS[status.toLowerCase() as keyof typeof STATUS_COLORS] || STATUS_COLORS.unknown;
  const icon = status.toLowerCase() === 'healthy' || status.toLowerCase() === 'running' 
    ? <HealthyIcon sx={{ fontSize: 14 }} /> 
    : status.toLowerCase() === 'error' || status.toLowerCase() === 'stopped'
    ? <ErrorIcon sx={{ fontSize: 14 }} />
    : <WarningIcon sx={{ fontSize: 14 }} />;
  
  return (
    <Chip
      icon={icon}
      label={status}
      size="small"
      sx={{ bgcolor: `${color}20`, color, fontWeight: 600, '& .MuiChip-icon': { color } }}
    />
  );
};

// ============================================================================
// MAIN COMPONENT
// ============================================================================

export default function MonitoringSettingsTab() {
  const theme = useTheme();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [envInfo, setEnvInfo] = useState<EnvironmentInfo | null>(null);
  const [data, setData] = useState<MonitoringData | null>(null);
  const [lastRefresh, setLastRefresh] = useState(new Date());
  const [expandedSection, setExpandedSection] = useState<string | false>('overview');

  // Fetch environment info first (public endpoint, no auth required)
  const fetchEnvironment = useCallback(async () => {
    try {
      const response = await fetch('/api/monitoring/environment');
      if (response.ok) {
        const result = await response.json();
        setEnvInfo(result);
        return result;
      }
    } catch (err) {
      console.warn('Could not fetch environment info:', err);
    }
    return null;
  }, []);

  // Fetch full monitoring data (requires auth)
  const fetchMonitoringData = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const token = localStorage.getItem('token');
      const headers: HeadersInit = { 'Content-Type': 'application/json' };
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }

      const response = await fetch('/api/monitoring/all', { headers });
      
      // Check content type
      const contentType = response.headers.get('content-type');
      if (!contentType || !contentType.includes('application/json')) {
        if (response.status === 401 || response.status === 403) {
          throw new Error('Access denied. Please ensure you are logged in with Admin privileges.');
        }
        throw new Error(`Backend API unavailable. Received non-JSON response (Status: ${response.status}). Please ensure the API is running.`);
      }

      if (response.ok) {
        const result = await response.json();
        setData(result);
        setLastRefresh(new Date());
      } else if (response.status === 401) {
        throw new Error('Authentication required. Please log in with an Admin account.');
      } else {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `Failed to fetch monitoring data: ${response.status}`);
      }
    } catch (err) {
      console.error('Error fetching monitoring data:', err);
      setError(err instanceof Error ? err.message : 'Failed to fetch monitoring data');
    } finally {
      setLoading(false);
    }
  }, []);

  const refreshData = useCallback(async () => {
    await fetchEnvironment();
    await fetchMonitoringData();
  }, [fetchEnvironment, fetchMonitoringData]);

  useEffect(() => {
    refreshData();
    const interval = setInterval(refreshData, 30000);
    return () => clearInterval(interval);
  }, [refreshData]);

  // Computed values
  const deploymentType = useMemo(() => {
    if (envInfo?.isKubernetes || data?.infrastructure?.host?.isKubernetes) return 'kubernetes';
    if (envInfo?.isDocker || data?.infrastructure?.host?.isDocker) return 'docker';
    return envInfo?.deploymentType?.toLowerCase() || data?.infrastructure?.deploymentTypeName?.toLowerCase() || 'vm';
  }, [envInfo, data]);

  const databaseProvider = useMemo(() => {
    return envInfo?.databaseProvider?.toLowerCase() || data?.database?.providerName?.toLowerCase() || 'unknown';
  }, [envInfo, data]);

  const enabledMonitors = useMemo(() => {
    return envInfo?.enabledMonitors || data?.infrastructure?.activeMonitors || [];
  }, [envInfo, data]);

  const showDockerSection = useMemo(() => {
    return deploymentType === 'docker' || enabledMonitors.includes('docker') || (data?.containers?.length ?? 0) > 0;
  }, [deploymentType, enabledMonitors, data]);

  const showK8sSection = useMemo(() => {
    return deploymentType === 'kubernetes' || enabledMonitors.includes('kubernetes') || (data?.pods?.length ?? 0) > 0;
  }, [deploymentType, enabledMonitors, data]);

  // Statistics
  const serviceCount = data?.services?.length || 0;
  const healthyServiceCount = data?.services?.filter(s => s.status === 'healthy').length || 0;
  const containerCount = data?.containers?.length || 0;
  const runningContainerCount = data?.containers?.filter(c => c.status === 'running').length || 0;
  const podCount = data?.pods?.length || 0;
  const readyPodCount = data?.pods?.filter(p => p.ready).length || 0;
  const sessionCount = data?.activeSessions?.length || 0;

  const overallHealth = useMemo(() => {
    if (!data) return { status: 'unknown', message: 'Loading...', color: '#9e9e9e' };
    
    const issues: string[] = [];
    if (!data.database?.isHealthy) issues.push('Database unhealthy');
    const errorServices = data.services?.filter(s => s.status === 'error').length || 0;
    if (errorServices > 0) issues.push(`${errorServices} service(s) with errors`);
    const stoppedContainers = data.containers?.filter(c => c.status !== 'running').length || 0;
    if (stoppedContainers > 0) issues.push(`${stoppedContainers} container(s) stopped`);
    const unhealthyPods = data.pods?.filter(p => !p.ready).length || 0;
    if (unhealthyPods > 0) issues.push(`${unhealthyPods} pod(s) not ready`);

    if (issues.length === 0) return { status: 'healthy', message: 'All systems operational', color: '#4caf50' };
    if (issues.some(i => i.includes('error') || i.includes('unhealthy'))) return { status: 'error', message: issues.join(' • '), color: '#f44336' };
    return { status: 'degraded', message: issues.join(' • '), color: '#ff9800' };
  }, [data]);

  const handleAccordionChange = (panel: string) => (_: React.SyntheticEvent, isExpanded: boolean) => {
    setExpandedSection(isExpanded ? panel : false);
  };

  // Loading skeleton
  if (loading && !data) {
    return (
      <Box>
        <Skeleton variant="rectangular" height={60} sx={{ borderRadius: 2, mb: 3 }} />
        <Grid container spacing={2} sx={{ mb: 3 }}>
          {[1, 2, 3, 4, 5, 6].map(i => (
            <Grid item xs={6} sm={4} md={2} key={i}>
              <Skeleton variant="rectangular" height={120} sx={{ borderRadius: 2 }} />
            </Grid>
          ))}
        </Grid>
        {[1, 2, 3].map(i => (
          <Skeleton key={i} variant="rectangular" height={80} sx={{ borderRadius: 2, mb: 2 }} />
        ))}
      </Box>
    );
  }

  return (
    <Box>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 700, display: 'flex', alignItems: 'center', gap: 1 }}>
            {DEPLOYMENT_ICONS[deploymentType] || <ServerIcon />}
            System Monitoring
            <Chip 
              label={deploymentType.toUpperCase()} 
              size="small" 
              sx={{ ml: 1, fontWeight: 600, bgcolor: theme.palette.primary.main, color: 'white' }}
            />
          </Typography>
          <Typography color="textSecondary" sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <span>{DATABASE_ICONS[databaseProvider] || '🗄️'}</span>
            {envInfo?.databaseProvider || data?.database?.providerName || 'Database'} • 
            Host: {envInfo?.hostname || data?.infrastructure?.host?.hostname || 'Unknown'}
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
          <Typography variant="body2" color="textSecondary">
            Updated: {lastRefresh.toLocaleTimeString()}
          </Typography>
          <Tooltip title="Refresh Now">
            <IconButton onClick={refreshData} disabled={loading} color="primary">
              {loading ? <CircularProgress size={20} /> : <RefreshIcon />}
            </IconButton>
          </Tooltip>
        </Box>
      </Box>

      {/* Error Alert */}
      {error && (
        <Alert severity="error" sx={{ mb: 3, borderRadius: 2 }} onClose={() => setError(null)}>
          <Typography fontWeight={600}>Monitoring Error</Typography>
          <Typography variant="body2">{error}</Typography>
        </Alert>
      )}

      {/* Overall Health Banner */}
      <Alert
        severity={overallHealth.status === 'healthy' ? 'success' : overallHealth.status === 'degraded' ? 'warning' : 'error'}
        icon={overallHealth.status === 'healthy' ? <HealthyIcon /> : overallHealth.status === 'degraded' ? <WarningIcon /> : <ErrorIcon />}
        sx={{ mb: 3, borderRadius: 2 }}
      >
        <Typography variant="subtitle1" fontWeight={600}>{overallHealth.message}</Typography>
      </Alert>

      {/* Summary Cards */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid item xs={6} sm={4} md={2}>
          <StatusCard
            icon={<ApiIcon />}
            title="API Services"
            value={`${healthyServiceCount}/${serviceCount}`}
            color="#1976d2"
            bgColor="#E3F2FD"
          />
        </Grid>
        {showDockerSection && (
          <Grid item xs={6} sm={4} md={2}>
            <StatusCard
              icon={<ContainerIcon />}
              title="Containers"
              value={`${runningContainerCount}/${containerCount}`}
              color="#7b1fa2"
              bgColor="#F3E5F5"
            />
          </Grid>
        )}
        {showK8sSection && (
          <Grid item xs={6} sm={4} md={2}>
            <StatusCard
              icon={<KubernetesIcon />}
              title="K8s Pods"
              value={`${readyPodCount}/${podCount}`}
              color="#00796b"
              bgColor="#E0F2F1"
            />
          </Grid>
        )}
        <Grid item xs={6} sm={4} md={2}>
          <StatusCard
            icon={<DatabaseIcon />}
            title="Database"
            value={data?.database?.isHealthy ? 'Online' : 'Offline'}
            subtitle={data?.database?.responseTimeMs ? `${data.database.responseTimeMs}ms` : undefined}
            color={data?.database?.isHealthy ? '#388e3c' : '#d32f2f'}
            bgColor={data?.database?.isHealthy ? '#E8F5E9' : '#FFEBEE'}
          />
        </Grid>
        <Grid item xs={6} sm={4} md={2}>
          <StatusCard
            icon={<MemoryIcon />}
            title="Memory"
            value={`${data?.system?.memory?.processWorkingSetMB || 0}`}
            subtitle="MB Used"
            color="#f57c00"
            bgColor="#FFF3E0"
          />
        </Grid>
        <Grid item xs={6} sm={4} md={2}>
          <StatusCard
            icon={<PersonIcon />}
            title="Sessions"
            value={sessionCount}
            subtitle="Active Users"
            color="#5d4037"
            bgColor="#EFEBE9"
          />
        </Grid>
      </Grid>

      {/* Infrastructure Info */}
      <Accordion
        expanded={expandedSection === 'infrastructure'}
        onChange={handleAccordionChange('infrastructure')}
        sx={{ borderRadius: 2, mb: 2, '&:before': { display: 'none' } }}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <InfoIcon color="info" />
            <Typography variant="h6" fontWeight={600}>Infrastructure Details</Typography>
            <Chip label={deploymentType.toUpperCase()} size="small" color="primary" variant="outlined" />
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <Paper variant="outlined" sx={{ p: 2, borderRadius: 2 }}>
                <Typography variant="subtitle2" color="primary" fontWeight={600} sx={{ mb: 2 }}>Host Information</Typography>
                <Stack spacing={1}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="body2" color="textSecondary">Hostname</Typography>
                    <Typography variant="body2" fontWeight={500}>{data?.infrastructure?.host?.hostname || 'N/A'}</Typography>
                  </Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="body2" color="textSecondary">OS</Typography>
                    <Typography variant="body2" fontWeight={500} sx={{ maxWidth: 200, textAlign: 'right' }}>{data?.infrastructure?.host?.osDescription || 'N/A'}</Typography>
                  </Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="body2" color="textSecondary">Architecture</Typography>
                    <Typography variant="body2" fontWeight={500}>{data?.infrastructure?.host?.architecture || 'N/A'}</Typography>
                  </Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="body2" color="textSecondary">CPUs</Typography>
                    <Typography variant="body2" fontWeight={500}>{data?.infrastructure?.host?.processorCount || 'N/A'}</Typography>
                  </Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="body2" color="textSecondary">Total Memory</Typography>
                    <Typography variant="body2" fontWeight={500}>{data?.infrastructure?.host?.totalMemoryMB ? `${(data.infrastructure.host.totalMemoryMB / 1024).toFixed(1)} GB` : 'N/A'}</Typography>
                  </Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="body2" color="textSecondary">.NET Version</Typography>
                    <Typography variant="body2" fontWeight={500}>{data?.infrastructure?.host?.dotNetVersion || 'N/A'}</Typography>
                  </Box>
                </Stack>
              </Paper>
            </Grid>
            <Grid item xs={12} md={6}>
              <Paper variant="outlined" sx={{ p: 2, borderRadius: 2 }}>
                <Typography variant="subtitle2" color="primary" fontWeight={600} sx={{ mb: 2 }}>Database Information</Typography>
                <Stack spacing={1}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="body2" color="textSecondary">Provider</Typography>
                    <Typography variant="body2" fontWeight={500}>{data?.infrastructure?.database?.providerName || 'N/A'}</Typography>
                  </Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="body2" color="textSecondary">Version</Typography>
                    <Typography variant="body2" fontWeight={500}>{data?.infrastructure?.database?.version || data?.database?.version || 'N/A'}</Typography>
                  </Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="body2" color="textSecondary">Host</Typography>
                    <Typography variant="body2" fontWeight={500}>{data?.infrastructure?.database?.host || 'N/A'}</Typography>
                  </Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="body2" color="textSecondary">Status</Typography>
                    <StatusChip status={data?.infrastructure?.database?.isConnected ? 'healthy' : 'error'} />
                  </Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="body2" color="textSecondary">Size</Typography>
                    <Typography variant="body2" fontWeight={500}>{data?.database?.databaseSizeMB ? `${data.database.databaseSizeMB.toFixed(1)} MB` : 'N/A'}</Typography>
                  </Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="body2" color="textSecondary">Active Connections</Typography>
                    <Typography variant="body2" fontWeight={500}>{data?.database?.activeConnections ?? 'N/A'}</Typography>
                  </Box>
                </Stack>
              </Paper>
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {/* API Services */}
      <Accordion
        expanded={expandedSection === 'overview'}
        onChange={handleAccordionChange('overview')}
        sx={{ borderRadius: 2, mb: 2, '&:before': { display: 'none' } }}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <ApiIcon color="primary" />
            <Typography variant="h6" fontWeight={600}>API Services</Typography>
            <Chip label={`${healthyServiceCount}/${serviceCount} Healthy`} size="small" color={healthyServiceCount === serviceCount ? 'success' : 'warning'} />
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <TableContainer>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Service</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Endpoint</TableCell>
                  <TableCell>Response</TableCell>
                  <TableCell>Uptime</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data?.services?.map((service) => (
                  <TableRow key={service.name} hover>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        {service.type === 'api' && <ApiIcon fontSize="small" color="primary" />}
                        {service.type === 'database' && <DatabaseIcon fontSize="small" color="secondary" />}
                        {service.type === 'frontend' && <CloudIcon fontSize="small" />}
                        <Typography fontWeight={500}>{service.name}</Typography>
                      </Box>
                    </TableCell>
                    <TableCell><StatusChip status={service.status} /></TableCell>
                    <TableCell>
                      <Typography variant="body2" color="textSecondary" sx={{ maxWidth: 200, overflow: 'hidden', textOverflow: 'ellipsis' }}>
                        {service.endpoint}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Chip label={`${service.responseTimeMs}ms`} size="small" variant="outlined" 
                        color={service.responseTimeMs < 100 ? 'success' : service.responseTimeMs < 500 ? 'warning' : 'error'} />
                    </TableCell>
                    <TableCell>{service.uptime || 'N/A'}</TableCell>
                  </TableRow>
                ))}
                {(!data?.services || data.services.length === 0) && (
                  <TableRow><TableCell colSpan={5} align="center"><Typography color="textSecondary">No services detected</Typography></TableCell></TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </AccordionDetails>
      </Accordion>

      {/* Docker Containers - Only show if Docker deployment or containers exist */}
      {showDockerSection && (
        <Accordion
          expanded={expandedSection === 'containers'}
          onChange={handleAccordionChange('containers')}
          sx={{ borderRadius: 2, mb: 2, '&:before': { display: 'none' } }}
        >
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <ContainerIcon sx={{ color: '#7b1fa2' }} />
              <Typography variant="h6" fontWeight={600}>Docker Containers</Typography>
              <Chip label={containerCount > 0 ? `${runningContainerCount}/${containerCount} Running` : 'No containers'} size="small" 
                color={runningContainerCount === containerCount && containerCount > 0 ? 'success' : 'default'} />
            </Box>
          </AccordionSummary>
          <AccordionDetails>
            {data?.containers && data.containers.length > 0 ? (
              <Grid container spacing={2}>
                {data.containers.map((container) => (
                  <Grid item xs={12} md={6} lg={4} key={container.containerId}>
                    <Card variant="outlined" sx={{ borderRadius: 2, borderColor: container.status === 'running' ? '#4caf5040' : '#f4433640' }}>
                      <CardContent>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
                          <Box>
                            <Typography variant="subtitle1" fontWeight={600}>{container.containerName}</Typography>
                            <Typography variant="caption" color="textSecondary">{container.containerId.substring(0, 12)}</Typography>
                          </Box>
                          <StatusChip status={container.status} />
                        </Box>
                        <Typography variant="body2" color="textSecondary" sx={{ mb: 2, wordBreak: 'break-all' }}>{container.image}</Typography>
                        {container.status === 'running' && (
                          <>
                            <MetricProgress label="CPU" value={container.cpuPercent || 0} />
                            <MetricProgress label="Memory" value={container.memoryMB || 0} total={container.memoryLimitMB || 512} unit=" MB" />
                          </>
                        )}
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                          <Typography variant="caption" color="textSecondary">
                            <ScheduleIcon sx={{ fontSize: 12, mr: 0.5, verticalAlign: 'middle' }} />
                            {container.uptime || 'N/A'}
                          </Typography>
                          {container.health && <Chip label={container.health} size="small" variant="outlined" 
                            color={container.health === 'healthy' ? 'success' : 'warning'} />}
                        </Box>
                      </CardContent>
                    </Card>
                  </Grid>
                ))}
              </Grid>
            ) : (
              <Box sx={{ textAlign: 'center', py: 4 }}>
                <ContainerIcon sx={{ fontSize: 48, color: '#ccc', mb: 2 }} />
                <Typography color="textSecondary">No Docker containers detected</Typography>
                <Typography variant="body2" color="textSecondary">Docker monitoring may need additional configuration</Typography>
              </Box>
            )}
          </AccordionDetails>
        </Accordion>
      )}

      {/* Kubernetes Pods - Only show if K8s deployment or pods exist */}
      {showK8sSection && (
        <Accordion
          expanded={expandedSection === 'kubernetes'}
          onChange={handleAccordionChange('kubernetes')}
          sx={{ borderRadius: 2, mb: 2, '&:before': { display: 'none' } }}
        >
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <KubernetesIcon sx={{ color: '#00796b' }} />
              <Typography variant="h6" fontWeight={600}>Kubernetes Pods</Typography>
              <Chip label={podCount > 0 ? `${readyPodCount}/${podCount} Ready` : 'No pods'} size="small" 
                color={readyPodCount === podCount && podCount > 0 ? 'success' : 'default'} />
            </Box>
          </AccordionSummary>
          <AccordionDetails>
            {data?.pods && data.pods.length > 0 ? (
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Pod Name</TableCell>
                      <TableCell>Namespace</TableCell>
                      <TableCell>Phase</TableCell>
                      <TableCell>Ready</TableCell>
                      <TableCell>Restarts</TableCell>
                      <TableCell>Node</TableCell>
                      <TableCell>IP</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {data.pods.map((pod) => (
                      <TableRow key={pod.podName} hover>
                        <TableCell><Typography fontWeight={500}>{pod.podName}</Typography></TableCell>
                        <TableCell><Chip label={pod.namespace} size="small" variant="outlined" /></TableCell>
                        <TableCell><StatusChip status={pod.phase === 'Running' ? 'healthy' : pod.phase.toLowerCase()} /></TableCell>
                        <TableCell>{pod.ready ? <HealthyIcon color="success" fontSize="small" /> : <ErrorIcon color="error" fontSize="small" />}</TableCell>
                        <TableCell><Chip label={pod.restartCount} size="small" color={pod.restartCount > 5 ? 'error' : pod.restartCount > 0 ? 'warning' : 'default'} /></TableCell>
                        <TableCell>{pod.nodeName || 'N/A'}</TableCell>
                        <TableCell><Typography variant="body2" color="textSecondary">{pod.podIP || 'N/A'}</Typography></TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            ) : (
              <Box sx={{ textAlign: 'center', py: 4 }}>
                <KubernetesIcon sx={{ fontSize: 48, color: '#ccc', mb: 2 }} />
                <Typography color="textSecondary">No Kubernetes pods detected</Typography>
                <Typography variant="body2" color="textSecondary">Kubernetes monitoring may need additional configuration</Typography>
              </Box>
            )}
          </AccordionDetails>
        </Accordion>
      )}

      {/* System Resources */}
      <Accordion
        expanded={expandedSection === 'system'}
        onChange={handleAccordionChange('system')}
        sx={{ borderRadius: 2, mb: 2, '&:before': { display: 'none' } }}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <CpuIcon sx={{ color: '#f57c00' }} />
            <Typography variant="h6" fontWeight={600}>System Resources</Typography>
            <Chip label={`${data?.system?.cpu?.usagePercent?.toFixed(1) || 0}% CPU`} size="small" color="default" />
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12} md={4}>
              <Paper variant="outlined" sx={{ p: 2, borderRadius: 2 }}>
                <Typography variant="subtitle2" color="primary" fontWeight={600} sx={{ mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
                  <CpuIcon fontSize="small" /> CPU
                </Typography>
                <MetricProgress label="Usage" value={data?.system?.cpu?.usagePercent || 0} />
                <Typography variant="body2" color="textSecondary">
                  {data?.system?.cpu?.processorCount || 0} Processor(s)
                </Typography>
              </Paper>
            </Grid>
            <Grid item xs={12} md={4}>
              <Paper variant="outlined" sx={{ p: 2, borderRadius: 2 }}>
                <Typography variant="subtitle2" color="primary" fontWeight={600} sx={{ mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
                  <MemoryIcon fontSize="small" /> Memory
                </Typography>
                <MetricProgress 
                  label="Process Memory" 
                  value={data?.system?.memory?.processWorkingSetMB || 0} 
                  total={data?.system?.memory?.totalMB ? data.system.memory.totalMB / 1024 : 8192} 
                  unit=" MB" 
                />
                <Typography variant="body2" color="textSecondary">
                  Total: {data?.system?.memory?.totalMB ? `${(data.system.memory.totalMB / 1024).toFixed(1)} GB` : 'N/A'}
                </Typography>
              </Paper>
            </Grid>
            <Grid item xs={12} md={4}>
              <Paper variant="outlined" sx={{ p: 2, borderRadius: 2 }}>
                <Typography variant="subtitle2" color="primary" fontWeight={600} sx={{ mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
                  <DiskIcon fontSize="small" /> Disk
                </Typography>
                <MetricProgress 
                  label="Usage" 
                  value={data?.system?.disk?.usagePercent || 0} 
                />
                <Typography variant="body2" color="textSecondary">
                  {data?.system?.disk?.freeSpaceGB || 0} GB free of {data?.system?.disk?.totalSpaceGB || 0} GB
                </Typography>
              </Paper>
            </Grid>
            <Grid item xs={12}>
              <Paper variant="outlined" sx={{ p: 2, borderRadius: 2 }}>
                <Typography variant="subtitle2" color="primary" fontWeight={600} sx={{ mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
                  <SettingsIcon fontSize="small" /> Process Info
                </Typography>
                <Grid container spacing={2}>
                  <Grid item xs={6} sm={3}>
                    <Typography variant="body2" color="textSecondary">Uptime</Typography>
                    <Typography fontWeight={500}>{data?.system?.process?.uptimeFormatted || 'N/A'}</Typography>
                  </Grid>
                  <Grid item xs={6} sm={3}>
                    <Typography variant="body2" color="textSecondary">Threads</Typography>
                    <Typography fontWeight={500}>{data?.system?.process?.threadCount || 0}</Typography>
                  </Grid>
                  <Grid item xs={6} sm={3}>
                    <Typography variant="body2" color="textSecondary">Working Set</Typography>
                    <Typography fontWeight={500}>{data?.system?.process?.workingSetMB || 0} MB</Typography>
                  </Grid>
                  <Grid item xs={6} sm={3}>
                    <Typography variant="body2" color="textSecondary">Handles</Typography>
                    <Typography fontWeight={500}>{data?.system?.process?.handleCount || 0}</Typography>
                  </Grid>
                </Grid>
              </Paper>
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {/* Active Sessions */}
      <Accordion
        expanded={expandedSection === 'sessions'}
        onChange={handleAccordionChange('sessions')}
        sx={{ borderRadius: 2, mb: 2, '&:before': { display: 'none' } }}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <PersonIcon sx={{ color: '#5d4037' }} />
            <Typography variant="h6" fontWeight={600}>Active Sessions</Typography>
            <Chip label={`${sessionCount} Active`} size="small" color="default" />
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          {data?.activeSessions && data.activeSessions.length > 0 ? (
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>User</TableCell>
                    <TableCell>Email</TableCell>
                    <TableCell>Role</TableCell>
                    <TableCell>Last Activity</TableCell>
                    <TableCell>Status</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {data.activeSessions.map((session) => (
                    <TableRow key={session.userId} hover>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Avatar sx={{ width: 28, height: 28, fontSize: 12, bgcolor: '#5d4037' }}>
                            {session.userName.split(' ').map(n => n[0]).join('')}
                          </Avatar>
                          <Typography fontWeight={500}>{session.userName}</Typography>
                        </Box>
                      </TableCell>
                      <TableCell><Typography variant="body2" color="textSecondary">{session.email}</Typography></TableCell>
                      <TableCell><Chip label={session.role} size="small" variant="outlined" /></TableCell>
                      <TableCell><Typography variant="body2" color="textSecondary">{new Date(session.lastActivity).toLocaleString()}</Typography></TableCell>
                      <TableCell>
                        <Badge color={session.isActive ? 'success' : 'default'} variant="dot" sx={{ '& .MuiBadge-badge': { right: -8 } }}>
                          <Typography variant="body2">{session.isActive ? 'Active' : 'Inactive'}</Typography>
                        </Badge>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          ) : (
            <Box sx={{ textAlign: 'center', py: 4 }}>
              <PersonIcon sx={{ fontSize: 48, color: '#ccc', mb: 2 }} />
              <Typography color="textSecondary">No active sessions in the last 24 hours</Typography>
            </Box>
          )}
        </AccordionDetails>
      </Accordion>

      {/* Enabled Monitors Footer */}
      <Paper variant="outlined" sx={{ p: 2, borderRadius: 2, mt: 2 }}>
        <Typography variant="subtitle2" color="textSecondary" sx={{ mb: 1 }}>Active Monitors</Typography>
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
          {enabledMonitors.length > 0 ? enabledMonitors.map(monitor => (
            <Chip key={monitor} label={monitor} size="small" variant="outlined" color="primary" />
          )) : (
            <Typography variant="body2" color="textSecondary">No monitors configured</Typography>
          )}
        </Box>
      </Paper>
    </Box>
  );
}
