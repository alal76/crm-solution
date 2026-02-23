/**
 * WebhookHealthDashboard - Overview of webhook health metrics and status
 */

import React, { useMemo } from 'react';
import {
  Box,
  Card,
  CardContent,
  Chip,
  Grid,
  LinearProgress,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tooltip,
  Typography,
  CircularProgress,
  IconButton,
} from '@mui/material';
import {
  CheckCircle as ActiveIcon,
  Cancel as InactiveIcon,
  Speed as SpeedIcon,
  Webhook as WebhookIcon,
  TrendingUp as SuccessRateIcon,
  ErrorOutline as FailIcon,
  Visibility as ViewIcon,
} from '@mui/icons-material';

export interface WebhookSummary {
  id: number;
  url: string;
  name: string;
  isActive: boolean;
  successRate: number;
  totalDeliveries: number;
  failedDeliveries: number;
  avgResponseTime: number;
  lastDeliveryAt?: string;
  circuitState: 'Closed' | 'Open' | 'HalfOpen';
}

interface WebhookHealthDashboardProps {
  webhooks?: WebhookSummary[];
  loading?: boolean;
  onViewWebhook?: (webhookId: number) => void;
}

const getSuccessRateColor = (rate: number): 'success' | 'warning' | 'error' => {
  if (rate >= 95) return 'success';
  if (rate >= 80) return 'warning';
  return 'error';
};

const getCircuitStateColor = (
  state: WebhookSummary['circuitState']
): 'success' | 'error' | 'warning' => {
  switch (state) {
    case 'Closed':
      return 'success';
    case 'Open':
      return 'error';
    case 'HalfOpen':
      return 'warning';
  }
};

const getCircuitStateLabel = (state: WebhookSummary['circuitState']): string => {
  switch (state) {
    case 'Closed':
      return 'Healthy';
    case 'Open':
      return 'Circuit Open';
    case 'HalfOpen':
      return 'Recovering';
  }
};

const formatDate = (dateStr: string | undefined): string => {
  if (!dateStr) return 'Never';
  try {
    return new Date(dateStr).toLocaleString();
  } catch {
    return dateStr;
  }
};

interface SummaryCardProps {
  title: string;
  value: string | number;
  subtitle?: string;
  icon: React.ReactNode;
  color: string;
}

const SummaryCard: React.FC<SummaryCardProps> = ({ title, value, subtitle, icon, color }) => (
  <Card variant="outlined" sx={{ height: '100%' }}>
    <CardContent>
      <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
        <Box>
          <Typography variant="caption" color="text.secondary">
            {title}
          </Typography>
          <Typography variant="h4" fontWeight={700}>
            {value}
          </Typography>
          {subtitle && (
            <Typography variant="caption" color="text.secondary">
              {subtitle}
            </Typography>
          )}
        </Box>
        <Box
          sx={{
            p: 1,
            borderRadius: 2,
            backgroundColor: `${color}`,
            color: 'white',
            display: 'flex',
          }}
        >
          {icon}
        </Box>
      </Stack>
    </CardContent>
  </Card>
);

const WebhookHealthDashboard: React.FC<WebhookHealthDashboardProps> = ({
  webhooks = [],
  loading = false,
  onViewWebhook,
}) => {
  const stats = useMemo(() => {
    if (webhooks.length === 0) {
      return {
        total: 0,
        active: 0,
        avgSuccessRate: 0,
        avgResponseTime: 0,
      };
    }

    const active = webhooks.filter((w) => w.isActive).length;
    const avgSuccessRate =
      webhooks.reduce((sum, w) => sum + w.successRate, 0) / webhooks.length;
    const webhooksWithDeliveries = webhooks.filter((w) => w.totalDeliveries > 0);
    const avgResponseTime =
      webhooksWithDeliveries.length > 0
        ? webhooksWithDeliveries.reduce((sum, w) => sum + w.avgResponseTime, 0) /
          webhooksWithDeliveries.length
        : 0;

    return {
      total: webhooks.length,
      active,
      avgSuccessRate: Math.round(avgSuccessRate * 10) / 10,
      avgResponseTime: Math.round(avgResponseTime),
    };
  }, [webhooks]);

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" py={6}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      {/* Summary Cards */}
      <Grid container spacing={2} mb={3}>
        <Grid item xs={12} sm={6} md={3}>
          <SummaryCard
            title="Total Webhooks"
            value={stats.total}
            icon={<WebhookIcon />}
            color="#1976d2"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <SummaryCard
            title="Active"
            value={stats.active}
            subtitle={`${stats.total - stats.active} inactive`}
            icon={<ActiveIcon />}
            color="#2e7d32"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <SummaryCard
            title="Avg Success Rate"
            value={`${stats.avgSuccessRate}%`}
            icon={<SuccessRateIcon />}
            color={
              stats.avgSuccessRate >= 95
                ? '#2e7d32'
                : stats.avgSuccessRate >= 80
                ? '#ed6c02'
                : '#d32f2f'
            }
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <SummaryCard
            title="Avg Response Time"
            value={`${stats.avgResponseTime}ms`}
            icon={<SpeedIcon />}
            color="#9c27b0"
          />
        </Grid>
      </Grid>

      {/* Webhooks Table */}
      {webhooks.length === 0 ? (
        <Paper variant="outlined" sx={{ p: 4, textAlign: 'center' }}>
          <WebhookIcon sx={{ fontSize: 48, color: 'text.disabled', mb: 1 }} />
          <Typography color="text.secondary">
            No webhooks configured yet.
          </Typography>
        </Paper>
      ) : (
        <TableContainer component={Paper} variant="outlined">
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>URL</TableCell>
                <TableCell align="center">Status</TableCell>
                <TableCell>Success Rate</TableCell>
                <TableCell align="center">Circuit State</TableCell>
                <TableCell align="right">Deliveries</TableCell>
                <TableCell align="right">Failed</TableCell>
                <TableCell>Last Delivery</TableCell>
                {onViewWebhook && <TableCell align="center">Actions</TableCell>}
              </TableRow>
            </TableHead>
            <TableBody>
              {webhooks.map((webhook) => (
                <TableRow key={webhook.id} hover>
                  <TableCell>
                    <Typography variant="body2" fontWeight={600}>
                      {webhook.name}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Tooltip title={webhook.url}>
                      <Typography
                        variant="body2"
                        fontFamily="monospace"
                        fontSize="0.8rem"
                        noWrap
                        sx={{ maxWidth: 200 }}
                      >
                        {webhook.url}
                      </Typography>
                    </Tooltip>
                  </TableCell>
                  <TableCell align="center">
                    <Chip
                      icon={webhook.isActive ? <ActiveIcon /> : <InactiveIcon />}
                      label={webhook.isActive ? 'Active' : 'Inactive'}
                      color={webhook.isActive ? 'success' : 'default'}
                      size="small"
                      variant="outlined"
                    />
                  </TableCell>
                  <TableCell>
                    <Stack direction="row" spacing={1} alignItems="center">
                      <Box sx={{ width: 80 }}>
                        <LinearProgress
                          variant="determinate"
                          value={webhook.successRate}
                          color={getSuccessRateColor(webhook.successRate)}
                          sx={{ height: 6, borderRadius: 3 }}
                        />
                      </Box>
                      <Typography
                        variant="caption"
                        fontWeight={600}
                        color={`${getSuccessRateColor(webhook.successRate)}.main`}
                      >
                        {webhook.successRate}%
                      </Typography>
                    </Stack>
                  </TableCell>
                  <TableCell align="center">
                    <Chip
                      label={getCircuitStateLabel(webhook.circuitState)}
                      color={getCircuitStateColor(webhook.circuitState)}
                      size="small"
                      variant="filled"
                    />
                  </TableCell>
                  <TableCell align="right">
                    <Typography variant="body2">
                      {webhook.totalDeliveries.toLocaleString()}
                    </Typography>
                  </TableCell>
                  <TableCell align="right">
                    <Typography
                      variant="body2"
                      color={webhook.failedDeliveries > 0 ? 'error.main' : 'text.secondary'}
                      fontWeight={webhook.failedDeliveries > 0 ? 600 : 400}
                    >
                      {webhook.failedDeliveries > 0 ? (
                        <Stack direction="row" spacing={0.5} alignItems="center" justifyContent="flex-end">
                          <FailIcon fontSize="inherit" />
                          <span>{webhook.failedDeliveries.toLocaleString()}</span>
                        </Stack>
                      ) : (
                        '0'
                      )}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" color="text.secondary">
                      {formatDate(webhook.lastDeliveryAt)}
                    </Typography>
                  </TableCell>
                  {onViewWebhook && (
                    <TableCell align="center">
                      <Tooltip title="View Details">
                        <IconButton
                          size="small"
                          onClick={() => onViewWebhook(webhook.id)}
                        >
                          <ViewIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  )}
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  );
};

export default WebhookHealthDashboard;
