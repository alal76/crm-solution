/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Comprehensive Monitoring Dashboard Component
 * Displays infrastructure, system, database, and service metrics
 */

import React, { useState, useEffect, useCallback } from 'react';
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
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Avatar,
  CircularProgress,
  Tabs,
  Tab,
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  CheckCircle as HealthyIcon,
  Warning as WarningIcon,
  Error as ErrorIcon,
  Storage as DatabaseIcon,
  Memory as MemoryIcon,
  Speed as CpuIcon,
  Dns as DnsIcon,
  Cloud as CloudIcon,
  Person as PersonIcon,
  Computer as ServerIcon,
  Language as WebIcon,
  Api as ApiIcon,
  NetworkCheck as NetworkIcon,
  Inventory as ContainerIcon,
  Apps as KubernetesIcon,
} from '@mui/icons-material';

// Status colors
const STATUS_COLORS = {
  healthy: '#4caf50',
  degraded: '#ff9800',
  error: '#f44336',
  unknown: '#9e9e9e',
  running: '#4caf50',
  stopped: '#f44336',
};

// Interfaces matching backend DTOs
interface InfrastructureInfo {
  timestamp: string;
  deploymentType: number;
  deploymentTypeName: string;
  database: DatabaseInfo;
  host: HostInfo;
  activeMonitors: string[];
  availableMonitors: string[];
}

interface DatabaseInfo {
  provider: number;
  providerName: string;
  version: string;
  host: string;
  port: number;
  isConnected: boolean;
  edition: string;
  collation: string;
}

interface HostInfo {
  hostname: string;
  fqdn: string;
  osDescription: string;
  architecture: string;
  processorCount: number;
  totalMemoryMB: number;
  dotNetVersion: string;
  isDocker: boolean;
  isKubernetes: boolean;
}

interface SystemMetrics {
  timestamp: string;
  cpu: CpuMetrics;
  memory: MemoryMetrics;
  disk: DiskMetrics;
  network: NetworkMetrics;
  process: ProcessMetrics;
}

interface CpuMetrics {
  usagePercent: number;
  processorCount: number;
  processCpuPercent: number;
}

interface MemoryMetrics {
  totalMB: number;
  usedMB: number;
  freeMB: number;
  usagePercent: number;
  processWorkingSetMB: number;
}

interface DiskMetrics {
  drives: DiskInfo[];
  totalSpaceGB: number;
  usedSpaceGB: number;
  freeSpaceGB: number;
  usagePercent: number;
}

interface DiskInfo {
  name: string;
  mountPoint: string;
  totalGB: number;
  usedGB: number;
  freeGB: number;
  usagePercent: number;
}

interface NetworkMetrics {
  bytesReceived: number;
  bytesSent: number;
  interfaces: NetworkInterfaceInfo[];
}

interface NetworkInterfaceInfo {
  name: string;
  ipAddress: string;
  isUp: boolean;
  bytesReceived: number;
  bytesSent: number;
}

interface ProcessMetrics {
  threadCount: number;
  handleCount: number;
  workingSetMB: number;
  privateMemoryMB: number;
  cpuTimeMs: number;
  uptimeFormatted: string;
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
  providerSpecificMetrics: Record<string, any>;
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
  metadata: Record<string, any>;
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

// Tab panel component
interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div role="tabpanel" hidden={value !== index} {...other}>
      {value === index && <Box sx={{ py: 2 }}>{children}</Box>}
    </div>
  );
}

export default function MonitoringSettingsTab() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [data, setData] = useState<MonitoringData | null>(null);
  const [lastRefresh, setLastRefresh] = useState(new Date());
  const [tabValue, setTabValue] = useState(0);

  const refreshData = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const token = localStorage.getItem('token');
      const headers: HeadersInit = {
        'Content-Type': 'application/json',
      };
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }
      
      const response = await fetch('/api/monitoring/all', { headers });
      
      if (response.ok) {
        const result = await response.json();
        setData(result);
        setLastRefresh(new Date());
      } else {
        throw new Error(`Failed to fetch monitoring data: ${response.status}`);
      }
    } catch (err) {
      console.error('Error fetching monitoring data:', err);
      setError(err instanceof Error ? err.message : 'Failed to fetch monitoring data');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    refreshData();
    const interval = setInterval(refreshData, 30000); // Refresh every 30 seconds
    return () => clearInterval(interval);
  }, [refreshData]);

  const getStatusIcon = (status: string) => {
    switch (status?.toLowerCase()) {
      case 'healthy':
      case 'running':
        return <HealthyIcon sx={{ color: STATUS_COLORS.healthy }} />;
      case 'degraded':
        return <WarningIcon sx={{ color: STATUS_COLORS.degraded }} />;
      case 'error':
      case 'stopped':
        return <ErrorIcon sx={{ color: STATUS_COLORS.error }} />;
      default:
        return <WarningIcon sx={{ color: STATUS_COLORS.unknown }} />;
    }
  };

  const getProgressColor = (percent: number) => {
    if (percent < 50) return '#4caf50';
    if (percent < 75) return '#ff9800';
    return '#f44336';
  };

  const formatBytes = (bytes: number) => {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const formatDuration = (isoString: string) => {
    if (!isoString) return 'N/A';
    const diff = Date.now() - new Date(isoString).getTime();
    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(minutes / 60);
    if (hours > 0) return `${hours}h ${minutes % 60}m ago`;
    if (minutes > 0) return `${minutes}m ago`;
    return 'Just now';
  };

  const getDeploymentTypeName = (type: number) => {
    switch (type) {
      case 0: return 'Docker';
      case 1: return 'Kubernetes';
      case 2: return 'Virtual Machine';
      case 3: return 'Hybrid';
      default: return 'Unknown';
    }
  };

  const calculateOverallHealth = () => {
    if (!data?.services) return { status: 'unknown', message: 'Loading...' };
    
    const errorCount = data.services.filter(s => s.status === 'error').length;
    const degradedCount = data.services.filter(s => s.status === 'degraded').length;
    
    if (errorCount > 0) return { status: 'error', message: `${errorCount} service(s) have errors` };
    if (degradedCount > 0) return { status: 'degraded', message: `${degradedCount} service(s) degraded` };
    return { status: 'healthy', message: 'All systems operational' };
  };

  const overallHealth = calculateOverallHealth();

  return (
    <Box>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 700 }}>
            System Monitoring
          </Typography>
          <Typography color="textSecondary">
            Real-time infrastructure and resource monitoring
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
          <Typography variant="body2" color="textSecondary">
            Last updated: {lastRefresh.toLocaleTimeString()}
          </Typography>
          <Tooltip title="Refresh Now">
            <IconButton onClick={refreshData} disabled={loading}>
              {loading ? <CircularProgress size={20} /> : <RefreshIcon />}
            </IconButton>
          </Tooltip>
        </Box>
      </Box>

      {/* Error Alert */}
      {error && (
        <Alert severity="error" sx={{ mb: 3, borderRadius: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Overall Health Banner */}
      <Alert
        severity={overallHealth.status === 'healthy' ? 'success' : overallHealth.status === 'degraded' ? 'warning' : 'error'}
        icon={getStatusIcon(overallHealth.status)}
        sx={{ mb: 3, borderRadius: 2 }}
      >
        <Typography variant="subtitle1" fontWeight={600}>
          {overallHealth.message}
        </Typography>
      </Alert>

      {/* Summary Cards */}
      {data && (
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid item xs={6} md={3}>
            <Card sx={{ borderRadius: 2, bgcolor: '#E8F5E9' }}>
              <CardContent sx={{ textAlign: 'center' }}>
                <PersonIcon sx={{ fontSize: 40, color: '#4caf50', mb: 1 }} />
                <Typography variant="h4" fontWeight={700} color="#4caf50">
                  {data.activeSessions?.length || 0}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Active Sessions
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={6} md={3}>
            <Card sx={{ borderRadius: 2, bgcolor: '#E3F2FD' }}>
              <CardContent sx={{ textAlign: 'center' }}>
                <MemoryIcon sx={{ fontSize: 40, color: '#2196f3', mb: 1 }} />
                <Typography variant="h4" fontWeight={700} color="#2196f3">
                  {data.system?.process?.threadCount || 0}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Active Threads
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={6} md={3}>
            <Card sx={{ borderRadius: 2, bgcolor: '#FFF3E0' }}>
              <CardContent sx={{ textAlign: 'center' }}>
                <DnsIcon sx={{ fontSize: 40, color: '#ff9800', mb: 1 }} />
                <Typography variant="h4" fontWeight={700} color="#ff9800">
                  {data.database?.activeConnections || 0}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  DB Connections
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={6} md={3}>
            <Card sx={{ borderRadius: 2, bgcolor: '#F3E5F5' }}>
              <CardContent sx={{ textAlign: 'center' }}>
                <CloudIcon sx={{ fontSize: 40, color: '#9c27b0', mb: 1 }} />
                <Typography variant="h4" fontWeight={700} color="#9c27b0">
                  {data.services?.length || 0}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Services
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      {/* Tabs */}
      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
        <Tabs value={tabValue} onChange={(_, v) => setTabValue(v)}>
          <Tab label="Infrastructure" />
          <Tab label="System Resources" />
          <Tab label="Services" />
          {data?.containers && data.containers.length > 0 && <Tab label="Containers" />}
          {data?.pods && data.pods.length > 0 && <Tab label="Kubernetes" />}
          <Tab label="Sessions" />
        </Tabs>
      </Box>

      {/* Tab 0: Infrastructure */}
      <TabPanel value={tabValue} index={0}>
        {data?.infrastructure && (
          <Grid container spacing={3}>
            {/* Deployment Info */}
            <Grid item xs={12} md={6}>
              <Card sx={{ borderRadius: 2 }}>
                <CardContent>
                  <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
                    <ServerIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                    Deployment
                  </Typography>
                  <Grid container spacing={2}>
                    <Grid item xs={6}>
                      <Paper sx={{ p: 2, bgcolor: '#fafafa', borderRadius: 2 }}>
                        <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                          Deployment Type
                        </Typography>
                        <Typography variant="body1" fontWeight={600}>
                          {getDeploymentTypeName(data.infrastructure.deploymentType)}
                        </Typography>
                      </Paper>
                    </Grid>
                    <Grid item xs={6}>
                      <Paper sx={{ p: 2, bgcolor: '#fafafa', borderRadius: 2 }}>
                        <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                          Hostname
                        </Typography>
                        <Typography variant="body1" fontWeight={600}>
                          {data.infrastructure.host?.hostname || 'N/A'}
                        </Typography>
                      </Paper>
                    </Grid>
                    <Grid item xs={12}>
                      <Paper sx={{ p: 2, bgcolor: '#fafafa', borderRadius: 2 }}>
                        <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                          Operating System
                        </Typography>
                        <Typography variant="body1" fontWeight={600}>
                          {data.infrastructure.host?.osDescription || 'N/A'}
                        </Typography>
                      </Paper>
                    </Grid>
                    <Grid item xs={6}>
                      <Paper sx={{ p: 2, bgcolor: '#fafafa', borderRadius: 2 }}>
                        <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                          Architecture
                        </Typography>
                        <Typography variant="body1" fontWeight={600}>
                          {data.infrastructure.host?.architecture || 'N/A'}
                        </Typography>
                      </Paper>
                    </Grid>
                    <Grid item xs={6}>
                      <Paper sx={{ p: 2, bgcolor: '#fafafa', borderRadius: 2 }}>
                        <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                          CPU Cores
                        </Typography>
                        <Typography variant="body1" fontWeight={600}>
                          {data.infrastructure.host?.processorCount || 'N/A'}
                        </Typography>
                      </Paper>
                    </Grid>
                    <Grid item xs={12}>
                      <Paper sx={{ p: 2, bgcolor: '#fafafa', borderRadius: 2 }}>
                        <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                          .NET Runtime
                        </Typography>
                        <Typography variant="body1" fontWeight={600}>
                          {data.infrastructure.host?.dotNetVersion || 'N/A'}
                        </Typography>
                      </Paper>
                    </Grid>
                  </Grid>
                </CardContent>
              </Card>
            </Grid>

            {/* Database Info */}
            <Grid item xs={12} md={6}>
              <Card sx={{ borderRadius: 2 }}>
                <CardContent>
                  <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
                    <DatabaseIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                    Database
                  </Typography>
                  <Grid container spacing={2}>
                    <Grid item xs={6}>
                      <Paper sx={{ p: 2, bgcolor: '#fafafa', borderRadius: 2 }}>
                        <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                          Provider
                        </Typography>
                        <Typography variant="body1" fontWeight={600}>
                          {data.database?.providerName || data.infrastructure.database?.providerName || 'N/A'}
                        </Typography>
                      </Paper>
                    </Grid>
                    <Grid item xs={6}>
                      <Paper sx={{ p: 2, bgcolor: '#fafafa', borderRadius: 2 }}>
                        <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                          Status
                        </Typography>
                        <Chip
                          icon={getStatusIcon(data.database?.isHealthy ? 'healthy' : 'error')}
                          label={data.database?.isHealthy ? 'Connected' : 'Disconnected'}
                          size="small"
                          sx={{
                            bgcolor: data.database?.isHealthy ? '#E8F5E9' : '#FFEBEE',
                            color: data.database?.isHealthy ? '#4caf50' : '#f44336',
                          }}
                        />
                      </Paper>
                    </Grid>
                    <Grid item xs={12}>
                      <Paper sx={{ p: 2, bgcolor: '#fafafa', borderRadius: 2 }}>
                        <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                          Version
                        </Typography>
                        <Typography variant="body1" fontWeight={600}>
                          {data.database?.version || data.infrastructure.database?.version || 'N/A'}
                        </Typography>
                      </Paper>
                    </Grid>
                    <Grid item xs={6}>
                      <Paper sx={{ p: 2, bgcolor: '#fafafa', borderRadius: 2 }}>
                        <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                          Host
                        </Typography>
                        <Typography variant="body1" fontWeight={600}>
                          {data.infrastructure.database?.host || 'N/A'}
                        </Typography>
                      </Paper>
                    </Grid>
                    <Grid item xs={6}>
                      <Paper sx={{ p: 2, bgcolor: '#fafafa', borderRadius: 2 }}>
                        <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                          Response Time
                        </Typography>
                        <Typography variant="body1" fontWeight={600}>
                          {data.database?.responseTimeMs || 0}ms
                        </Typography>
                      </Paper>
                    </Grid>
                    <Grid item xs={6}>
                      <Paper sx={{ p: 2, bgcolor: '#fafafa', borderRadius: 2 }}>
                        <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                          Database Size
                        </Typography>
                        <Typography variant="body1" fontWeight={600}>
                          {(data.database?.databaseSizeMB || 0).toFixed(2)} MB
                        </Typography>
                      </Paper>
                    </Grid>
                    <Grid item xs={6}>
                      <Paper sx={{ p: 2, bgcolor: '#fafafa', borderRadius: 2 }}>
                        <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                          Active Connections
                        </Typography>
                        <Typography variant="body1" fontWeight={600}>
                          {data.database?.activeConnections || 0}
                        </Typography>
                      </Paper>
                    </Grid>
                  </Grid>
                </CardContent>
              </Card>
            </Grid>

            {/* Active Monitors */}
            <Grid item xs={12}>
              <Card sx={{ borderRadius: 2 }}>
                <CardContent>
                  <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
                    Active Monitors
                  </Typography>
                  <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                    {data.infrastructure.activeMonitors?.map((monitor) => (
                      <Chip
                        key={monitor}
                        label={monitor}
                        size="small"
                        color="primary"
                        variant="outlined"
                      />
                    ))}
                  </Box>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        )}
      </TabPanel>

      {/* Tab 1: System Resources */}
      <TabPanel value={tabValue} index={1}>
        {data?.system && (
          <Grid container spacing={3}>
            {/* CPU */}
            <Grid item xs={12} md={6}>
              <Card sx={{ borderRadius: 2 }}>
                <CardContent>
                  <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
                    <CpuIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                    CPU
                  </Typography>
                  <Box sx={{ mb: 3 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                      <Typography variant="body2">Process CPU Usage</Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {data.system.cpu?.processCpuPercent?.toFixed(1) || 0}%
                      </Typography>
                    </Box>
                    <LinearProgress
                      variant="determinate"
                      value={Math.min(100, data.system.cpu?.processCpuPercent || 0)}
                      sx={{
                        height: 10,
                        borderRadius: 5,
                        bgcolor: '#e0e0e0',
                        '& .MuiLinearProgress-bar': {
                          bgcolor: getProgressColor(data.system.cpu?.processCpuPercent || 0),
                        },
                      }}
                    />
                  </Box>
                  <Typography variant="body2" color="textSecondary">
                    Processor Count: {data.system.cpu?.processorCount || 0}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>

            {/* Memory */}
            <Grid item xs={12} md={6}>
              <Card sx={{ borderRadius: 2 }}>
                <CardContent>
                  <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
                    <MemoryIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                    Memory
                  </Typography>
                  <Box sx={{ mb: 3 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                      <Typography variant="body2">Process Memory</Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {data.system.memory?.processWorkingSetMB || 0} MB
                      </Typography>
                    </Box>
                    <LinearProgress
                      variant="determinate"
                      value={Math.min(100, data.system.memory?.usagePercent || 0)}
                      sx={{
                        height: 10,
                        borderRadius: 5,
                        bgcolor: '#e0e0e0',
                        '& .MuiLinearProgress-bar': {
                          bgcolor: getProgressColor(data.system.memory?.usagePercent || 0),
                        },
                      }}
                    />
                  </Box>
                  <Typography variant="body2" color="textSecondary">
                    Total Available: {(data.system.memory?.totalMB || 0).toLocaleString()} MB
                  </Typography>
                </CardContent>
              </Card>
            </Grid>

            {/* Disk */}
            <Grid item xs={12} md={6}>
              <Card sx={{ borderRadius: 2 }}>
                <CardContent>
                  <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
                    <DatabaseIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                    Disk
                  </Typography>
                  {data.system.disk?.drives?.map((drive, index) => (
                    <Box key={index} sx={{ mb: 2 }}>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                        <Typography variant="body2">{drive.name || drive.mountPoint}</Typography>
                        <Typography variant="body2" fontWeight={600}>
                          {drive.usedGB} / {drive.totalGB} GB
                        </Typography>
                      </Box>
                      <LinearProgress
                        variant="determinate"
                        value={Math.min(100, drive.usagePercent || 0)}
                        sx={{
                          height: 10,
                          borderRadius: 5,
                          bgcolor: '#e0e0e0',
                          '& .MuiLinearProgress-bar': {
                            bgcolor: getProgressColor(drive.usagePercent || 0),
                          },
                        }}
                      />
                    </Box>
                  ))}
                  {(!data.system.disk?.drives || data.system.disk.drives.length === 0) && (
                    <Typography variant="body2" color="textSecondary">
                      No disk information available
                    </Typography>
                  )}
                </CardContent>
              </Card>
            </Grid>

            {/* Network */}
            <Grid item xs={12} md={6}>
              <Card sx={{ borderRadius: 2 }}>
                <CardContent>
                  <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
                    <NetworkIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                    Network
                  </Typography>
                  <Grid container spacing={2}>
                    <Grid item xs={6}>
                      <Paper sx={{ p: 2, bgcolor: '#E3F2FD', borderRadius: 2, textAlign: 'center' }}>
                        <Typography variant="subtitle2" color="textSecondary">
                          Bytes Received
                        </Typography>
                        <Typography variant="h6" fontWeight={600} color="#2196f3">
                          {formatBytes(data.system.network?.bytesReceived || 0)}
                        </Typography>
                      </Paper>
                    </Grid>
                    <Grid item xs={6}>
                      <Paper sx={{ p: 2, bgcolor: '#E8F5E9', borderRadius: 2, textAlign: 'center' }}>
                        <Typography variant="subtitle2" color="textSecondary">
                          Bytes Sent
                        </Typography>
                        <Typography variant="h6" fontWeight={600} color="#4caf50">
                          {formatBytes(data.system.network?.bytesSent || 0)}
                        </Typography>
                      </Paper>
                    </Grid>
                  </Grid>
                  <Divider sx={{ my: 2 }} />
                  <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                    Active Interfaces
                  </Typography>
                  {data.system.network?.interfaces?.slice(0, 3).map((iface, index) => (
                    <Box key={index} sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                      <Typography variant="body2">{iface.name}</Typography>
                      <Typography variant="body2" color="textSecondary">
                        {iface.ipAddress || 'N/A'}
                      </Typography>
                    </Box>
                  ))}
                </CardContent>
              </Card>
            </Grid>

            {/* Process Info */}
            <Grid item xs={12}>
              <Card sx={{ borderRadius: 2 }}>
                <CardContent>
                  <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
                    Process Information
                  </Typography>
                  <Grid container spacing={2}>
                    <Grid item xs={6} md={2}>
                      <Paper sx={{ p: 2, bgcolor: '#fafafa', borderRadius: 2, textAlign: 'center' }}>
                        <Typography variant="subtitle2" color="textSecondary">Threads</Typography>
                        <Typography variant="h5" fontWeight={600}>
                          {data.system.process?.threadCount || 0}
                        </Typography>
                      </Paper>
                    </Grid>
                    <Grid item xs={6} md={2}>
                      <Paper sx={{ p: 2, bgcolor: '#fafafa', borderRadius: 2, textAlign: 'center' }}>
                        <Typography variant="subtitle2" color="textSecondary">Handles</Typography>
                        <Typography variant="h5" fontWeight={600}>
                          {data.system.process?.handleCount || 0}
                        </Typography>
                      </Paper>
                    </Grid>
                    <Grid item xs={6} md={2}>
                      <Paper sx={{ p: 2, bgcolor: '#fafafa', borderRadius: 2, textAlign: 'center' }}>
                        <Typography variant="subtitle2" color="textSecondary">Working Set</Typography>
                        <Typography variant="h5" fontWeight={600}>
                          {data.system.process?.workingSetMB || 0} MB
                        </Typography>
                      </Paper>
                    </Grid>
                    <Grid item xs={6} md={2}>
                      <Paper sx={{ p: 2, bgcolor: '#fafafa', borderRadius: 2, textAlign: 'center' }}>
                        <Typography variant="subtitle2" color="textSecondary">Private Memory</Typography>
                        <Typography variant="h5" fontWeight={600}>
                          {data.system.process?.privateMemoryMB || 0} MB
                        </Typography>
                      </Paper>
                    </Grid>
                    <Grid item xs={6} md={2}>
                      <Paper sx={{ p: 2, bgcolor: '#fafafa', borderRadius: 2, textAlign: 'center' }}>
                        <Typography variant="subtitle2" color="textSecondary">CPU Time</Typography>
                        <Typography variant="h5" fontWeight={600}>
                          {((data.system.process?.cpuTimeMs || 0) / 1000).toFixed(1)}s
                        </Typography>
                      </Paper>
                    </Grid>
                    <Grid item xs={6} md={2}>
                      <Paper sx={{ p: 2, bgcolor: '#fafafa', borderRadius: 2, textAlign: 'center' }}>
                        <Typography variant="subtitle2" color="textSecondary">Uptime</Typography>
                        <Typography variant="h5" fontWeight={600}>
                          {data.system.process?.uptimeFormatted || 'N/A'}
                        </Typography>
                      </Paper>
                    </Grid>
                  </Grid>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        )}
      </TabPanel>

      {/* Tab 2: Services */}
      <TabPanel value={tabValue} index={2}>
        <Card sx={{ borderRadius: 2 }}>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
              Service Status
            </Typography>
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Service</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Endpoint</TableCell>
                    <TableCell>Response Time</TableCell>
                    <TableCell>Version</TableCell>
                    <TableCell>Uptime</TableCell>
                    <TableCell>Last Check</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {data?.services?.map((service) => (
                    <TableRow key={service.name}>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          {service.type === 'api' && <ApiIcon color="primary" />}
                          {service.type === 'database' && <DatabaseIcon color="secondary" />}
                          {service.type === 'frontend' && <WebIcon color="action" />}
                          <Typography fontWeight={500}>{service.name}</Typography>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Chip
                          icon={getStatusIcon(service.status)}
                          label={service.status}
                          size="small"
                          sx={{
                            bgcolor: `${STATUS_COLORS[service.status as keyof typeof STATUS_COLORS] || STATUS_COLORS.unknown}20`,
                            color: STATUS_COLORS[service.status as keyof typeof STATUS_COLORS] || STATUS_COLORS.unknown,
                            fontWeight: 600,
                          }}
                        />
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" color="textSecondary">
                          {service.endpoint}
                        </Typography>
                      </TableCell>
                      <TableCell>{service.responseTimeMs}ms</TableCell>
                      <TableCell>{service.version}</TableCell>
                      <TableCell>{service.uptime}</TableCell>
                      <TableCell>{formatDuration(service.lastCheck)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>
      </TabPanel>

      {/* Tab 3: Containers (conditional) */}
      {data?.containers && data.containers.length > 0 && (
        <TabPanel value={tabValue} index={3}>
          <Card sx={{ borderRadius: 2 }}>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
                <ContainerIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                Docker Containers
              </Typography>
              <TableContainer>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Container</TableCell>
                      <TableCell>Image</TableCell>
                      <TableCell>Status</TableCell>
                      <TableCell>Health</TableCell>
                      <TableCell>Uptime</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {data.containers.map((container) => (
                      <TableRow key={container.containerId}>
                        <TableCell>
                          <Typography fontWeight={500}>{container.containerName}</Typography>
                          <Typography variant="caption" color="textSecondary">
                            {container.containerId.substring(0, 12)}
                          </Typography>
                        </TableCell>
                        <TableCell>{container.image}</TableCell>
                        <TableCell>
                          <Chip
                            icon={getStatusIcon(container.status)}
                            label={container.status}
                            size="small"
                            sx={{
                              bgcolor: container.status === 'running' ? '#E8F5E9' : '#FFEBEE',
                              color: container.status === 'running' ? '#4caf50' : '#f44336',
                            }}
                          />
                        </TableCell>
                        <TableCell>
                          <Chip
                            icon={getStatusIcon(container.health)}
                            label={container.health || 'none'}
                            size="small"
                            variant="outlined"
                          />
                        </TableCell>
                        <TableCell>{container.uptime}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        </TabPanel>
      )}

      {/* Tab 4: Kubernetes (conditional) */}
      {data?.pods && data.pods.length > 0 && (
        <TabPanel value={tabValue} index={data?.containers && data.containers.length > 0 ? 4 : 3}>
          <Card sx={{ borderRadius: 2 }}>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
                <KubernetesIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                Kubernetes Pods
              </Typography>
              <TableContainer>
                <Table>
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
                      <TableRow key={pod.podName}>
                        <TableCell>
                          <Typography fontWeight={500}>{pod.podName}</Typography>
                        </TableCell>
                        <TableCell>{pod.namespace}</TableCell>
                        <TableCell>
                          <Chip
                            label={pod.phase}
                            size="small"
                            sx={{
                              bgcolor: pod.phase === 'Running' ? '#E8F5E9' : '#FFF3E0',
                              color: pod.phase === 'Running' ? '#4caf50' : '#ff9800',
                            }}
                          />
                        </TableCell>
                        <TableCell>
                          {pod.ready ? (
                            <HealthyIcon sx={{ color: '#4caf50' }} />
                          ) : (
                            <WarningIcon sx={{ color: '#ff9800' }} />
                          )}
                        </TableCell>
                        <TableCell>{pod.restartCount}</TableCell>
                        <TableCell>{pod.nodeName}</TableCell>
                        <TableCell>{pod.podIP}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        </TabPanel>
      )}

      {/* Tab 5: Sessions */}
      <TabPanel value={tabValue} index={
        (data?.containers && data.containers.length > 0 ? 1 : 0) +
        (data?.pods && data.pods.length > 0 ? 1 : 0) + 3
      }>
        <Card sx={{ borderRadius: 2 }}>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
              <PersonIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
              Active Sessions ({data?.activeSessions?.length || 0})
            </Typography>
            {data?.activeSessions && data.activeSessions.length > 0 ? (
              <List>
                {data.activeSessions.map((session, index) => (
                  <React.Fragment key={session.userId}>
                    {index > 0 && <Divider />}
                    <ListItem>
                      <ListItemIcon>
                        <Avatar sx={{ bgcolor: '#6750A4' }}>
                          {session.userName?.charAt(0) || '?'}
                        </Avatar>
                      </ListItemIcon>
                      <ListItemText
                        primary={
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <Typography fontWeight={500}>{session.userName}</Typography>
                            <Chip label={session.role} size="small" variant="outlined" />
                          </Box>
                        }
                        secondary={
                          <Box>
                            <Typography variant="caption" color="textSecondary" display="block">
                              {session.email}
                            </Typography>
                            <Typography variant="caption" color="textSecondary">
                              Last activity: {formatDuration(session.lastActivity)}
                            </Typography>
                          </Box>
                        }
                      />
                    </ListItem>
                  </React.Fragment>
                ))}
              </List>
            ) : (
              <Typography color="textSecondary" sx={{ textAlign: 'center', py: 4 }}>
                No active sessions in the last 24 hours
              </Typography>
            )}
          </CardContent>
        </Card>
      </TabPanel>
    </Box>
  );
}
