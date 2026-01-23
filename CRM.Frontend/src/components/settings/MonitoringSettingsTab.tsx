import { useState, useEffect, useCallback } from 'react';
import {
  Box, Typography, Card, CardContent, Grid, Chip, LinearProgress,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Paper, IconButton, Tooltip, Button, CircularProgress, Alert,
  List, ListItem, ListItemText, ListItemIcon, Divider, Avatar
} from '@mui/material';
import {
  CheckCircle as HealthyIcon, Error as ErrorIcon, Warning as WarningIcon,
  Refresh as RefreshIcon, Person as PersonIcon, Memory as MemoryIcon,
  Speed as CpuIcon, Storage as StorageIcon, Api as ApiIcon,
  Web as WebIcon, Storage as DatabaseIcon, Timeline as TimelineIcon,
  CloudQueue as CloudIcon, Computer as ServerIcon, Dns as DnsIcon
} from '@mui/icons-material';

interface ServiceStatus {
  name: string;
  type: 'frontend' | 'api' | 'database';
  status: 'healthy' | 'degraded' | 'error';
  endpoint: string;
  uptime: string;
  lastCheck: string;
  responseTime: number;
  version?: string;
}

interface ResourceMetric {
  current: number;
  max: number;
  unit: string;
}

interface ServerMetrics {
  cpu: ResourceMetric;
  memory: ResourceMetric;
  disk: ResourceMetric;
  network: ResourceMetric;
  threads: number;
  connections: number;
}

interface LoggedInUser {
  id: string;
  name: string;
  email: string;
  loginTime: string;
  lastActivity: string;
  ipAddress: string;
  sessionId: string;
}

interface ServerLoad {
  service: string;
  load: number; // 0-100
  status: 'green' | 'amber' | 'red';
}

const STATUS_COLORS = {
  healthy: '#4caf50',
  degraded: '#ff9800',
  error: '#f44336',
};

const LOAD_COLORS = {
  green: '#4caf50',
  amber: '#ff9800',
  red: '#f44336',
};

function MonitoringSettingsTab() {
  const [loading, setLoading] = useState(true);
  const [lastRefresh, setLastRefresh] = useState(new Date());
  const [autoRefresh, setAutoRefresh] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Real data from API
  const [services, setServices] = useState<ServiceStatus[]>([]);

  const [serverMetrics, setServerMetrics] = useState<Record<string, ServerMetrics>>({
    api: {
      cpu: { current: 0, max: 100, unit: '%' },
      memory: { current: 0, max: 4096, unit: 'MB' },
      disk: { current: 0, max: 100, unit: 'GB' },
      network: { current: 0, max: 1000, unit: 'Mbps' },
      threads: 0,
      connections: 0,
    },
    database: {
      cpu: { current: 0, max: 100, unit: '%' },
      memory: { current: 0, max: 8192, unit: 'MB' },
      disk: { current: 0, max: 500, unit: 'GB' },
      network: { current: 0, max: 1000, unit: 'Mbps' },
      threads: 0,
      connections: 0,
    },
  });

  const [loggedInUsers, setLoggedInUsers] = useState<LoggedInUser[]>([]);

  const [serverLoads, setServerLoads] = useState<ServerLoad[]>([]);

  const refreshData = useCallback(async () => {
    setLoading(true);
    setError(null);
    
    try {
      const token = localStorage.getItem('token');
      const headers = {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      };

      // Fetch all monitoring data in one call
      const response = await fetch('/api/monitoring/all', { headers });
      
      if (response.ok) {
        const data = await response.json();
        
        // Update services
        if (data.services && data.services.length > 0) {
          setServices(data.services.map((s: any) => ({
            name: s.name,
            type: s.type,
            status: s.status as 'healthy' | 'degraded' | 'error',
            endpoint: s.endpoint,
            uptime: s.uptime,
            lastCheck: s.lastCheck,
            responseTime: s.responseTime,
            version: s.version,
          })));
        }

        // Update server metrics
        if (data.systemMetrics) {
          const sm = data.systemMetrics;
          setServerMetrics({
            api: {
              cpu: { current: sm.process?.cpuTimeMs ? Math.min(100, sm.process.cpuTimeMs / 10000) : 0, max: 100, unit: '%' },
              memory: { current: sm.process?.workingSetMB || 0, max: 4096, unit: 'MB' },
              disk: { current: sm.database?.databaseSizeMB || 0, max: 500, unit: 'MB' },
              network: { current: 0, max: 1000, unit: 'Mbps' },
              threads: sm.process?.threadCount || 0,
              connections: sm.database?.activeConnections || 0,
            },
            database: {
              cpu: { current: 0, max: 100, unit: '%' },
              memory: { current: 0, max: 8192, unit: 'MB' },
              disk: { current: sm.database?.databaseSizeMB || 0, max: 500, unit: 'MB' },
              network: { current: 0, max: 1000, unit: 'Mbps' },
              threads: 0,
              connections: sm.database?.activeConnections || 0,
            },
          });
        }

        // Update server loads
        if (data.serverLoad?.services && data.serverLoad.services.length > 0) {
          setServerLoads(data.serverLoad.services.map((s: any) => ({
            service: s.service,
            load: Math.round(s.cpuLoad || 0),
            status: (s.status || 'green') as 'green' | 'amber' | 'red',
          })));
        }

        // Update logged in users
        if (data.activeSessions && data.activeSessions.length > 0) {
          setLoggedInUsers(data.activeSessions.map((u: any) => ({
            id: u.userId,
            name: u.name,
            email: u.email,
            loginTime: u.loginTime,
            lastActivity: u.lastActivity,
            ipAddress: u.ipAddress || 'N/A',
            sessionId: `session_${u.userId}`,
          })));
        }
      } else {
        // Fallback to individual endpoints if /all fails
        const [servicesRes, loadRes, sessionsRes] = await Promise.allSettled([
          fetch('/api/monitoring/services', { headers }),
          fetch('/api/monitoring/load', { headers }),
          fetch('/api/monitoring/sessions', { headers }),
        ]);

        if (servicesRes.status === 'fulfilled' && servicesRes.value.ok) {
          const servicesData = await servicesRes.value.json();
          setServices(servicesData.map((s: any) => ({
            name: s.name,
            type: s.type,
            status: s.status as 'healthy' | 'degraded' | 'error',
            endpoint: s.endpoint,
            uptime: s.uptime,
            lastCheck: s.lastCheck,
            responseTime: s.responseTime,
            version: s.version,
          })));
        }

        if (loadRes.status === 'fulfilled' && loadRes.value.ok) {
          const loadData = await loadRes.value.json();
          if (loadData.services) {
            setServerLoads(loadData.services.map((s: any) => ({
              service: s.service,
              load: Math.round(s.cpuLoad || 0),
              status: (s.status || 'green') as 'green' | 'amber' | 'red',
            })));
          }
        }

        if (sessionsRes.status === 'fulfilled' && sessionsRes.value.ok) {
          const sessionsData = await sessionsRes.value.json();
          setLoggedInUsers(sessionsData.map((u: any) => ({
            id: u.userId,
            name: u.name,
            email: u.email,
            loginTime: u.loginTime,
            lastActivity: u.lastActivity,
            ipAddress: u.ipAddress || 'N/A',
            sessionId: `session_${u.userId}`,
          })));
        }
      }

      setLastRefresh(new Date());
    } catch (err) {
      console.error('Error fetching monitoring data:', err);
      setError('Failed to fetch monitoring data');
    } finally {
      setLoading(false);
    }
  }, []);

  // Initial load
  useEffect(() => {
    refreshData();
  }, [refreshData]);

  useEffect(() => {
    if (autoRefresh) {
      const interval = setInterval(refreshData, 30000); // Refresh every 30 seconds
      return () => clearInterval(interval);
    }
  }, [autoRefresh, refreshData]);

  const getServiceIcon = (type: string) => {
    switch (type) {
      case 'frontend': return <WebIcon />;
      case 'api': return <ApiIcon />;
      case 'database': return <DatabaseIcon />;
      default: return <ServerIcon />;
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'healthy': return <HealthyIcon sx={{ color: STATUS_COLORS.healthy }} />;
      case 'degraded': return <WarningIcon sx={{ color: STATUS_COLORS.degraded }} />;
      case 'error': return <ErrorIcon sx={{ color: STATUS_COLORS.error }} />;
      default: return null;
    }
  };

  const getLoadColor = (status: 'green' | 'amber' | 'red') => LOAD_COLORS[status];

  const formatDuration = (isoString: string) => {
    const diff = Date.now() - new Date(isoString).getTime();
    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(minutes / 60);
    if (hours > 0) return `${hours}h ${minutes % 60}m ago`;
    if (minutes > 0) return `${minutes}m ago`;
    return 'Just now';
  };

  const calculateOverallHealth = () => {
    const errorCount = services.filter(s => s.status === 'error').length;
    const degradedCount = services.filter(s => s.status === 'degraded').length;
    
    if (errorCount > 0) return { status: 'error', message: 'System experiencing issues' };
    if (degradedCount > 0) return { status: 'degraded', message: 'Some services degraded' };
    return { status: 'healthy', message: 'All systems operational' };
  };

  const overallHealth = calculateOverallHealth();
  const totalThreads = Object.values(serverMetrics).reduce((sum, m) => sum + m.threads, 0);
  const totalConnections = Object.values(serverMetrics).reduce((sum, m) => sum + m.connections, 0);

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 700 }}>
            System Monitoring
          </Typography>
          <Typography color="textSecondary">
            Real-time status and resource utilization
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
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid item xs={6} md={3}>
          <Card sx={{ borderRadius: 2, bgcolor: '#E8F5E9' }}>
            <CardContent sx={{ textAlign: 'center' }}>
              <PersonIcon sx={{ fontSize: 40, color: '#4caf50', mb: 1 }} />
              <Typography variant="h4" fontWeight={700} color="#4caf50">
                {loggedInUsers.length}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Logged In Users
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={6} md={3}>
          <Card sx={{ borderRadius: 2, bgcolor: '#E3F2FD' }}>
            <CardContent sx={{ textAlign: 'center' }}>
              <MemoryIcon sx={{ fontSize: 40, color: '#2196f3', mb: 1 }} />
              <Typography variant="h4" fontWeight={700} color="#2196f3">
                {totalThreads}
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
                {totalConnections}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Active Connections
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={6} md={3}>
          <Card sx={{ borderRadius: 2, bgcolor: '#F3E5F5' }}>
            <CardContent sx={{ textAlign: 'center' }}>
              <CloudIcon sx={{ fontSize: 40, color: '#9c27b0', mb: 1 }} />
              <Typography variant="h4" fontWeight={700} color="#9c27b0">
                {services.length}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Services Running
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <Grid container spacing={3}>
        {/* Service Status */}
        <Grid item xs={12} lg={8}>
          <Card sx={{ borderRadius: 2, mb: 3 }}>
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
                      <TableCell>Uptime</TableCell>
                      <TableCell>Response Time</TableCell>
                      <TableCell>Last Check</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {services.map((service) => (
                      <TableRow key={service.name}>
                        <TableCell>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            {getServiceIcon(service.type)}
                            <Box>
                              <Typography variant="body2" fontWeight={500}>
                                {service.name}
                              </Typography>
                              <Typography variant="caption" color="textSecondary">
                                {service.endpoint}
                              </Typography>
                            </Box>
                          </Box>
                        </TableCell>
                        <TableCell>
                          <Chip
                            icon={getStatusIcon(service.status)}
                            label={service.status}
                            size="small"
                            sx={{
                              bgcolor: `${STATUS_COLORS[service.status]}20`,
                              color: STATUS_COLORS[service.status],
                              fontWeight: 600,
                            }}
                          />
                        </TableCell>
                        <TableCell>{service.uptime}</TableCell>
                        <TableCell>{Math.round(service.responseTime)}ms</TableCell>
                        <TableCell>{formatDuration(service.lastCheck)}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>

          {/* Resource Utilization */}
          <Card sx={{ borderRadius: 2 }}>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
                Resource Utilization
              </Typography>
              <Grid container spacing={3}>
                {Object.entries(serverMetrics).map(([key, metrics]) => (
                  <Grid item xs={12} md={4} key={key}>
                    <Paper sx={{ p: 2, bgcolor: '#fafafa', borderRadius: 2 }}>
                      <Typography variant="subtitle2" fontWeight={600} sx={{ mb: 2, textTransform: 'capitalize' }}>
                        {key === 'frontend' ? 'Frontend' : key === 'api' ? 'API' : 'Database'}
                      </Typography>
                      
                      {/* CPU */}
                      <Box sx={{ mb: 2 }}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                          <Typography variant="caption" color="textSecondary">
                            <CpuIcon sx={{ fontSize: 14, mr: 0.5, verticalAlign: 'middle' }} />
                            CPU
                          </Typography>
                          <Typography variant="caption" fontWeight={500}>
                            {Math.round(metrics.cpu.current)}%
                          </Typography>
                        </Box>
                        <LinearProgress
                          variant="determinate"
                          value={metrics.cpu.current}
                          sx={{
                            height: 8,
                            borderRadius: 4,
                            bgcolor: '#e0e0e0',
                            '& .MuiLinearProgress-bar': {
                              bgcolor: metrics.cpu.current < 50 ? '#4caf50' : metrics.cpu.current < 75 ? '#ff9800' : '#f44336',
                            },
                          }}
                        />
                      </Box>

                      {/* Memory */}
                      <Box sx={{ mb: 2 }}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                          <Typography variant="caption" color="textSecondary">
                            <MemoryIcon sx={{ fontSize: 14, mr: 0.5, verticalAlign: 'middle' }} />
                            Memory
                          </Typography>
                          <Typography variant="caption" fontWeight={500}>
                            {Math.round(metrics.memory.current)} / {metrics.memory.max} {metrics.memory.unit}
                          </Typography>
                        </Box>
                        <LinearProgress
                          variant="determinate"
                          value={(metrics.memory.current / metrics.memory.max) * 100}
                          sx={{
                            height: 8,
                            borderRadius: 4,
                            bgcolor: '#e0e0e0',
                            '& .MuiLinearProgress-bar': {
                              bgcolor: (metrics.memory.current / metrics.memory.max) < 0.5 ? '#4caf50' : (metrics.memory.current / metrics.memory.max) < 0.75 ? '#ff9800' : '#f44336',
                            },
                          }}
                        />
                      </Box>

                      {/* Disk */}
                      <Box sx={{ mb: 2 }}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                          <Typography variant="caption" color="textSecondary">
                            <StorageIcon sx={{ fontSize: 14, mr: 0.5, verticalAlign: 'middle' }} />
                            Disk
                          </Typography>
                          <Typography variant="caption" fontWeight={500}>
                            {metrics.disk.current} / {metrics.disk.max} {metrics.disk.unit}
                          </Typography>
                        </Box>
                        <LinearProgress
                          variant="determinate"
                          value={(metrics.disk.current / metrics.disk.max) * 100}
                          sx={{
                            height: 8,
                            borderRadius: 4,
                            bgcolor: '#e0e0e0',
                            '& .MuiLinearProgress-bar': {
                              bgcolor: (metrics.disk.current / metrics.disk.max) < 0.5 ? '#4caf50' : (metrics.disk.current / metrics.disk.max) < 0.75 ? '#ff9800' : '#f44336',
                            },
                          }}
                        />
                      </Box>

                      <Divider sx={{ my: 1 }} />
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant="caption" color="textSecondary">
                          Threads: <strong>{metrics.threads}</strong>
                        </Typography>
                        <Typography variant="caption" color="textSecondary">
                          Connections: <strong>{metrics.connections}</strong>
                        </Typography>
                      </Box>
                    </Paper>
                  </Grid>
                ))}
              </Grid>
            </CardContent>
          </Card>
        </Grid>

        {/* Right Column - Server Load & Logged In Users */}
        <Grid item xs={12} lg={4}>
          {/* RAG Server Load */}
          <Card sx={{ borderRadius: 2, mb: 3 }}>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
                Server Load (RAG)
              </Typography>
              {serverLoads.map((load) => (
                <Box key={load.service} sx={{ mb: 2 }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 0.5 }}>
                    <Typography variant="body2">{load.service}</Typography>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Typography variant="body2" fontWeight={500}>{load.load}%</Typography>
                      <Box
                        sx={{
                          width: 12,
                          height: 12,
                          borderRadius: '50%',
                          bgcolor: getLoadColor(load.status),
                          boxShadow: `0 0 8px ${getLoadColor(load.status)}`,
                        }}
                      />
                    </Box>
                  </Box>
                  <LinearProgress
                    variant="determinate"
                    value={load.load}
                    sx={{
                      height: 6,
                      borderRadius: 3,
                      bgcolor: '#e0e0e0',
                      '& .MuiLinearProgress-bar': {
                        bgcolor: getLoadColor(load.status),
                      },
                    }}
                  />
                </Box>
              ))}
              
              {/* Legend */}
              <Box sx={{ display: 'flex', justifyContent: 'center', gap: 3, mt: 3, pt: 2, borderTop: '1px solid #e0e0e0' }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                  <Box sx={{ width: 10, height: 10, borderRadius: '50%', bgcolor: '#4caf50' }} />
                  <Typography variant="caption" color="textSecondary">Normal</Typography>
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                  <Box sx={{ width: 10, height: 10, borderRadius: '50%', bgcolor: '#ff9800' }} />
                  <Typography variant="caption" color="textSecondary">Warning</Typography>
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                  <Box sx={{ width: 10, height: 10, borderRadius: '50%', bgcolor: '#f44336' }} />
                  <Typography variant="caption" color="textSecondary">Critical</Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>

          {/* Logged In Users */}
          <Card sx={{ borderRadius: 2 }}>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
                Logged In Users ({loggedInUsers.length})
              </Typography>
              <List dense>
                {loggedInUsers.map((user, index) => (
                  <Box key={user.id}>
                    {index > 0 && <Divider />}
                    <ListItem sx={{ px: 0 }}>
                      <ListItemIcon>
                        <Avatar sx={{ width: 32, height: 32, bgcolor: '#6750A4' }}>
                          {user.name.charAt(0)}
                        </Avatar>
                      </ListItemIcon>
                      <ListItemText
                        primary={
                          <Typography variant="body2" fontWeight={500}>
                            {user.name}
                          </Typography>
                        }
                        secondary={
                          <Box>
                            <Typography variant="caption" color="textSecondary" display="block">
                              {user.email}
                            </Typography>
                            <Typography variant="caption" color="textSecondary">
                              Last active: {formatDuration(user.lastActivity)}
                            </Typography>
                          </Box>
                        }
                      />
                    </ListItem>
                  </Box>
                ))}
              </List>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}

export default MonitoringSettingsTab;
