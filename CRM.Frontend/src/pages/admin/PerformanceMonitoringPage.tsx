// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the Source-Available License (see LICENSE) as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// Source-Available License (see LICENSE) for more details.
//
// You should have received a copy of the Source-Available License (see LICENSE)
// along with this program. If not, see <https://www.gnu.org/licenses/>.

import React from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  CircularProgress,
  Container,
  Grid,
  LinearProgress,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
  Alert,
  Stack,
  Chip,
  Paper
} from '@mui/material';
import {
  TrendingDown as TrendingDownIcon,
  Speed as SpeedIcon,
  Storage as StorageIcon,
  Error as ErrorIcon,
  Refresh as RefreshIcon,
  DeleteSweep as DeleteSweepIcon
} from '@mui/icons-material';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, BarChart, Bar } from 'recharts';
import apiClient from '../../services/apiClient';

interface PerformanceStats {
  endpoint: string;
  totalRequests: number;
  averageResponseTimeMs: number;
  P95ResponseTimeMs: number;
  P99ResponseTimeMs: number;
  cacheHitRate: number;
  errorRate: number;
}

interface PerformanceDashboard {
  averageResponseTimeMs: number;
  P95ResponseTimeMs: number;
  P99ResponseTimeMs: number;
  cacheHitRate: number;
  errorRate: number;
  totalRequestsLastHour: number;
  totalRequestsLastDay: number;
  topEndpoints: PerformanceStats[];
  recommendations: Array<{
    title: string;
    description: string;
    priority: 'High' | 'Medium' | 'Low';
    potentialImprovementPercent: number;
  }>;
}

interface CacheStats {
  totalHits: number;
  totalMisses: number;
  hitRate: number;
  memoryUsedBytes: number;
  maxMemoryBytes: number;
  cachedItemCount: number;
}

export const PerformanceMonitoringPage: React.FC = () => {
  const [dashboard, setDashboard] = React.useState<PerformanceDashboard | null>(null);
  const [cacheStats, setCacheStats] = React.useState<CacheStats | null>(null);
  const [loading, setLoading] = React.useState(true);
  const [refreshing, setRefreshing] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);

  React.useEffect(() => {
    loadDashboard();
    const interval = setInterval(loadDashboard, 30000); // Refresh every 30 seconds
    return () => clearInterval(interval);
  }, []);

  const loadDashboard = async () => {
    try {
      setLoading(true);
      const [dashRes, cacheRes] = await Promise.all([
        apiClient.get<PerformanceDashboard>('/performance/dashboard'),
        apiClient.get<CacheStats>('/performance/cache')
      ]);
      setDashboard(dashRes.data);
      setCacheStats(cacheRes.data);
      setError(null);
    } catch (err) {
      console.error('Failed to load performance dashboard:', err);
      setError('Failed to load performance data');
    } finally {
      setLoading(false);
    }
  };

  const handleRefresh = async () => {
    setRefreshing(true);
    await loadDashboard();
    setRefreshing(false);
  };

  const handleClearCache = async () => {
    if (!window.confirm('Clear all cache?')) return;
    try {
      await apiClient.post('/performance/cache/clear');
      await loadDashboard();
    } catch (err) {
      setError('Failed to clear cache');
    }
  };

  if (loading) {
    return (
      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center' }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (!dashboard || !cacheStats) {
    return (
      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Alert severity="error">Failed to load performance data</Alert>
      </Container>
    );
  }

  const memoryUsage = cacheStats ? (cacheStats.memoryUsedBytes ?? 0) / (cacheStats.maxMemoryBytes ?? 1) * 100 : 0;
  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'High':
        return 'error';
      case 'Medium':
        return 'warning';
      default:
        return 'info';
    }
  };

  // Provide safe defaults for cache stats
  const safeCacheStats: CacheStats = cacheStats ? {
    totalHits: cacheStats.totalHits ?? 0,
    totalMisses: cacheStats.totalMisses ?? 0,
    hitRate: cacheStats.hitRate ?? 0,
    memoryUsedBytes: cacheStats.memoryUsedBytes ?? 0,
    maxMemoryBytes: cacheStats.maxMemoryBytes ?? 1,
    cachedItemCount: cacheStats.cachedItemCount ?? 0
  } : {
    totalHits: 0,
    totalMisses: 0,
    hitRate: 0,
    memoryUsedBytes: 0,
    maxMemoryBytes: 1,
    cachedItemCount: 0
  };

  // Provide safe defaults for dashboard data
  const safeData: PerformanceDashboard = dashboard ? {
    averageResponseTimeMs: dashboard.averageResponseTimeMs ?? 0,
    P95ResponseTimeMs: dashboard.P95ResponseTimeMs ?? 0,
    P99ResponseTimeMs: dashboard.P99ResponseTimeMs ?? 0,
    cacheHitRate: dashboard.cacheHitRate ?? 0,
    errorRate: dashboard.errorRate ?? 0,
    totalRequestsLastHour: dashboard.totalRequestsLastHour ?? 0,
    totalRequestsLastDay: dashboard.totalRequestsLastDay ?? 0,
    topEndpoints: dashboard.topEndpoints ?? [],
    recommendations: dashboard.recommendations ?? []
  } : {
    averageResponseTimeMs: 0,
    P95ResponseTimeMs: 0,
    P99ResponseTimeMs: 0,
    cacheHitRate: 0,
    errorRate: 0,
    totalRequestsLastHour: 0,
    totalRequestsLastDay: 0,
    topEndpoints: [],
    recommendations: []
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Box sx={{ mb: 4, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box>
          <Typography variant="h4" gutterBottom>Performance Monitoring</Typography>
          <Typography variant="body2" color="textSecondary">
            Real-time API and query performance analytics
          </Typography>
        </Box>
        <Stack direction="row" spacing={2}>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={handleRefresh}
            disabled={refreshing}
          >
            {refreshing ? 'Refreshing...' : 'Refresh'}
          </Button>
          <Button
            variant="outlined"
            color="error"
            startIcon={<DeleteSweepIcon />}
            onClick={handleClearCache}
          >
            Clear Cache
          </Button>
        </Stack>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {/* KPI Cards */}
      <Grid container spacing={2} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Paper sx={{ p: 2 }}>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <SpeedIcon color="primary" sx={{ fontSize: 40, mr: 2 }} />
              <Box>
                <Typography variant="caption" color="textSecondary">
                  Avg Response Time
                </Typography>
                <Typography variant="h6">
                  {safeData.averageResponseTimeMs.toFixed(0)}ms
                </Typography>
              </Box>
            </Box>
          </Paper>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Paper sx={{ p: 2 }}>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <TrendingDownIcon color="success" sx={{ fontSize: 40, mr: 2 }} />
              <Box>
                <Typography variant="caption" color="textSecondary">
                  Cache Hit Rate
                </Typography>
                <Typography variant="h6">
                  {(safeData.cacheHitRate * 100).toFixed(1)}%
                </Typography>
              </Box>
            </Box>
          </Paper>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Paper sx={{ p: 2 }}>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <ErrorIcon color="error" sx={{ fontSize: 40, mr: 2 }} />
              <Box>
                <Typography variant="caption" color="textSecondary">
                  Error Rate
                </Typography>
                <Typography variant="h6">
                  {(safeData.errorRate * 100).toFixed(2)}%
                </Typography>
              </Box>
            </Box>
          </Paper>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Paper sx={{ p: 2 }}>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <StorageIcon color="warning" sx={{ fontSize: 40, mr: 2 }} />
              <Box>
                <Typography variant="caption" color="textSecondary">
                  Requests (24h)
                </Typography>
                <Typography variant="h6">
                  {safeData.totalRequestsLastDay.toLocaleString()}
                </Typography>
              </Box>
            </Box>
          </Paper>
        </Grid>
      </Grid>

      {/* Response Time Percentiles */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader title="Response Time Percentiles" />
            <CardContent>
              <Stack spacing={2}>
                <Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                    <Typography variant="body2">P95</Typography>
                    <Typography variant="body2" fontWeight="bold">
                      {safeData.P95ResponseTimeMs}ms
                    </Typography>
                  </Box>
                  <LinearProgress
                    variant="determinate"
                    value={Math.min(100, (safeData.P95ResponseTimeMs / 500) * 100)}
                    sx={{
                      backgroundColor: '#f0f0f0',
                      '& .MuiLinearProgress-bar': {
                        backgroundColor: safeData.P95ResponseTimeMs > 500 ? '#ff6b6b' : '#4caf50'
                      }
                    }}
                  />
                </Box>
                <Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                    <Typography variant="body2">P99</Typography>
                    <Typography variant="body2" fontWeight="bold">
                      {safeData.P99ResponseTimeMs}ms
                    </Typography>
                  </Box>
                  <LinearProgress
                    variant="determinate"
                    value={Math.min(100, (safeData.P99ResponseTimeMs / 1000) * 100)}
                    sx={{
                      backgroundColor: '#f0f0f0',
                      '& .MuiLinearProgress-bar': {
                        backgroundColor: safeData.P99ResponseTimeMs > 1000 ? '#ff6b6b' : '#4caf50'
                      }
                    }}
                  />
                </Box>
              </Stack>
            </CardContent>
          </Card>
        </Grid>

        {/* Cache Statistics */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader title="Cache Statistics" />
            <CardContent>
              <Stack spacing={2}>
                <Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                    <Typography variant="body2">Memory Usage</Typography>
                    <Typography variant="body2" fontWeight="bold">
                      {(safeCacheStats.memoryUsedBytes / 1024 / 1024).toFixed(2)}MB / {(safeCacheStats.maxMemoryBytes / 1024 / 1024).toFixed(2)}MB
                    </Typography>
                  </Box>
                  <LinearProgress variant="determinate" value={memoryUsage} />
                </Box>
                <Box>
                  <Typography variant="body2">Items Cached: {safeCacheStats.cachedItemCount.toLocaleString()}</Typography>
                </Box>
                <Box>
                  <Typography variant="body2">
                    Hit/Miss: {safeCacheStats.totalHits.toLocaleString()} / {safeCacheStats.totalMisses.toLocaleString()}
                  </Typography>
                </Box>
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Top Endpoints */}
      <Card sx={{ mb: 4 }}>
        <CardHeader title="Slowest Endpoints (Last 24h)" />
        <CardContent>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Endpoint</TableCell>
                  <TableCell align="right">Requests</TableCell>
                  <TableCell align="right">Avg Time</TableCell>
                  <TableCell align="right">P95 Time</TableCell>
                  <TableCell align="right">Cache Hit</TableCell>
                  <TableCell align="right">Error Rate</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {safeData.topEndpoints.map((ep) => (
                  <TableRow key={ep.endpoint} hover>
                    <TableCell>{ep.endpoint}</TableCell>
                    <TableCell align="right">{ep.totalRequests.toLocaleString()}</TableCell>
                    <TableCell align="right">{ep.averageResponseTimeMs.toFixed(0)}ms</TableCell>
                    <TableCell align="right">{ep.P95ResponseTimeMs}ms</TableCell>
                    <TableCell align="right">{(ep.cacheHitRate * 100).toFixed(1)}%</TableCell>
                    <TableCell align="right">
                      <Chip
                        label={`${(ep.errorRate * 100).toFixed(2)}%`}
                        color={ep.errorRate > 0.05 ? 'error' : 'success'}
                        size="small"
                      />
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </CardContent>
      </Card>

      {/* Recommendations */}
      {safeData.recommendations.length > 0 && (
        <Card>
          <CardHeader title="Performance Recommendations" />
          <CardContent>
            <Stack spacing={2}>
              {safeData.recommendations.map((rec, idx) => (
                <Paper key={idx} sx={{ p: 2, backgroundColor:
                  rec.priority === 'High' ? '#ffebee' : rec.priority === 'Medium' ? '#fff3e0' : '#e3f2fd'
                }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
                    <Box flex={1}>
                      <Typography variant="subtitle2" fontWeight="bold">{rec.title}</Typography>
                      <Typography variant="body2" color="textSecondary" sx={{ mt: 0.5 }}>
                        {rec.description}
                      </Typography>
                    </Box>
                    <Chip 
                      label={rec.priority} 
                      color={getPriorityColor(rec.priority) as any} 
                      size="small"
                      sx={{ ml: 2 }}
                    />
                  </Box>
                  <Typography variant="caption" color="success.main" sx={{ mt: 1, display: 'block' }}>
                    Potential improvement: {rec.potentialImprovementPercent}%
                  </Typography>
                </Paper>
              ))}
            </Stack>
          </CardContent>
        </Card>
      )}
    </Container>
  );
};

export default PerformanceMonitoringPage;
