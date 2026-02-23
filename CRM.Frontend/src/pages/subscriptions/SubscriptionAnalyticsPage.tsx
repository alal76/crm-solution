/**
 * SubscriptionAnalyticsPage - Dashboard with MRR, ARR, churn, and subscription metrics
 * Route: /subscriptions/analytics
 */
import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  CircularProgress,
  Alert,
  Grid,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Paper,
  Button,
  Stack,
  Divider,
} from '@mui/material';
import {
  ArrowBack as ArrowBackIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import subscriptionService, {
  Subscription,
  SubscriptionStatistics,
  getStatusLabel,
  getStatusColor,
} from '../../services/subscriptionService';
import billingService, { SubscriptionAnalyticsDto } from '../../services/billingService';
import BillingStatsCards from '../../components/sales/BillingStatsCards';

const formatCurrency = (amount: number): string => {
  try {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);
  } catch {
    return `$${amount.toFixed(2)}`;
  }
};

interface StatusDistribution {
  label: string;
  count: number;
  color: 'success' | 'warning' | 'error' | 'info' | 'default';
}

const SubscriptionAnalyticsPage: React.FC = () => {
  const navigate = useNavigate();

  const [analytics, setAnalytics] = useState<SubscriptionAnalyticsDto | null>(null);
  const [statistics, setStatistics] = useState<SubscriptionStatistics | null>(null);
  const [subscriptions, setSubscriptions] = useState<Subscription[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [analyticsRes, statsRes, subsRes] = await Promise.allSettled([
        billingService.getAnalytics(),
        subscriptionService.getStatistics(),
        subscriptionService.getAll(),
      ]);

      if (analyticsRes.status === 'fulfilled') {
        setAnalytics(analyticsRes.value);
      }
      if (statsRes.status === 'fulfilled') {
        setStatistics(statsRes.value.data);
      }
      if (subsRes.status === 'fulfilled') {
        setSubscriptions(subsRes.value.data);
      }

      // If all failed, show error
      if (
        analyticsRes.status === 'rejected' &&
        statsRes.status === 'rejected' &&
        subsRes.status === 'rejected'
      ) {
        setError('Failed to load analytics data.');
      }
    } catch {
      setError('Failed to load analytics data.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  // Build status distribution from statistics
  const statusDistribution: StatusDistribution[] = statistics
    ? [
        { label: 'Active', count: statistics.activeSubscriptions, color: 'success' },
        { label: 'Trial', count: statistics.trialSubscriptions, color: 'info' },
        { label: 'Paused', count: statistics.pausedSubscriptions, color: 'warning' },
        { label: 'Cancelled', count: statistics.cancelledSubscriptions, color: 'error' },
      ]
    : [];

  // Top subscriptions by amount (sorted descending, top 10)
  const topSubscriptions = [...subscriptions]
    .sort((a, b) => b.amount - a.amount)
    .slice(0, 10);

  // MRR trend - simple display using statistics data
  const mrrData = analytics
    ? { current: analytics.mrr, growth: analytics.revenueGrowthRate }
    : null;

  return (
    <Container maxWidth="lg" sx={{ py: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Stack direction="row" spacing={2} alignItems="center">
          <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/subscriptions')}>
            Subscriptions
          </Button>
          <Typography variant="h5" fontWeight={700}>
            Subscription Analytics
          </Typography>
        </Stack>
        <Button startIcon={<RefreshIcon />} onClick={loadData} disabled={loading}>
          Refresh
        </Button>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
          <CircularProgress />
        </Box>
      ) : (
        <Stack spacing={3}>
          {/* Stats Cards */}
          <BillingStatsCards stats={analytics || undefined} loading={false} />

          <Grid container spacing={3}>
            {/* MRR Trend Card */}
            <Grid item xs={12} md={6}>
              <Card variant="outlined" sx={{ height: '100%' }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    MRR Trend
                  </Typography>
                  <Divider sx={{ mb: 2 }} />
                  {mrrData ? (
                    <Box>
                      <Typography variant="h3" fontWeight={700} color="primary.main">
                        {formatCurrency(mrrData.current)}
                      </Typography>
                      <Typography
                        variant="body2"
                        sx={{
                          mt: 1,
                          color: mrrData.growth >= 0 ? 'success.main' : 'error.main',
                          fontWeight: 600,
                        }}
                      >
                        {mrrData.growth >= 0 ? '▲' : '▼'} {Math.abs(mrrData.growth).toFixed(1)}% growth rate
                      </Typography>
                      {statistics && (
                        <Box sx={{ mt: 2 }}>
                          <Typography variant="body2" color="text.secondary">
                            New this month: {statistics.newSubscriptionsThisMonth}
                          </Typography>
                          <Typography variant="body2" color="text.secondary">
                            Cancellations this month: {statistics.cancellationsThisMonth}
                          </Typography>
                          <Typography variant="body2" color="text.secondary">
                            Avg Revenue Per User: {formatCurrency(statistics.averageRevenuePerUser)}
                          </Typography>
                        </Box>
                      )}
                    </Box>
                  ) : (
                    <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
                      No MRR data available
                    </Typography>
                  )}
                </CardContent>
              </Card>
            </Grid>

            {/* Status Distribution */}
            <Grid item xs={12} md={6}>
              <Card variant="outlined" sx={{ height: '100%' }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Status Distribution
                  </Typography>
                  <Divider sx={{ mb: 2 }} />
                  {statusDistribution.length > 0 ? (
                    <Stack spacing={2}>
                      {statusDistribution.map((item) => (
                        <Box key={item.label} sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                          <Stack direction="row" spacing={1} alignItems="center">
                            <Chip label={item.label} size="small" color={item.color} />
                          </Stack>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                            <Box
                              sx={{
                                width: 120,
                                height: 8,
                                bgcolor: 'grey.100',
                                borderRadius: 1,
                                overflow: 'hidden',
                              }}
                            >
                              <Box
                                sx={{
                                  height: '100%',
                                  width: `${statistics ? Math.min((item.count / Math.max(statistics.totalSubscriptions, 1)) * 100, 100) : 0}%`,
                                  bgcolor: item.color === 'success' ? 'success.main' :
                                    item.color === 'info' ? 'info.main' :
                                    item.color === 'warning' ? 'warning.main' :
                                    item.color === 'error' ? 'error.main' : 'grey.400',
                                  borderRadius: 1,
                                  transition: 'width 0.6s ease',
                                }}
                              />
                            </Box>
                            <Typography variant="body2" fontWeight={600} sx={{ minWidth: 32, textAlign: 'right' }}>
                              {item.count}
                            </Typography>
                          </Box>
                        </Box>
                      ))}
                      {statistics && (
                        <Box sx={{ pt: 1 }}>
                          <Typography variant="caption" color="text.secondary">
                            Total: {statistics.totalSubscriptions} subscriptions
                          </Typography>
                        </Box>
                      )}
                    </Stack>
                  ) : (
                    <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
                      No distribution data available
                    </Typography>
                  )}
                </CardContent>
              </Card>
            </Grid>
          </Grid>

          {/* Top Subscriptions by Revenue */}
          <Card variant="outlined">
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Top Subscriptions by Revenue
              </Typography>
              <Divider sx={{ mb: 2 }} />
              {topSubscriptions.length === 0 ? (
                <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
                  No subscription data available
                </Typography>
              ) : (
                <TableContainer component={Paper} variant="outlined">
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Subscription #</TableCell>
                        <TableCell>Account</TableCell>
                        <TableCell>Product</TableCell>
                        <TableCell>Status</TableCell>
                        <TableCell>Billing Cycle</TableCell>
                        <TableCell align="right">Amount</TableCell>
                        <TableCell align="right">MRR</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {topSubscriptions.map((sub) => (
                        <TableRow
                          key={sub.id}
                          hover
                          sx={{ cursor: 'pointer' }}
                          onClick={() => navigate(`/subscriptions/${sub.id}`)}
                        >
                          <TableCell>{sub.subscriptionNumber || `#${sub.id}`}</TableCell>
                          <TableCell>{sub.account?.company || '—'}</TableCell>
                          <TableCell>{sub.product?.name || '—'}</TableCell>
                          <TableCell>
                            <Chip
                              label={getStatusLabel(sub.subscriptionStatus)}
                              size="small"
                              color={getStatusColor(sub.subscriptionStatus)}
                            />
                          </TableCell>
                          <TableCell>{sub.billingCycle || '—'}</TableCell>
                          <TableCell align="right">{formatCurrency(sub.amount)}</TableCell>
                          <TableCell align="right">{formatCurrency(sub.mrr || 0)}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              )}
            </CardContent>
          </Card>
        </Stack>
      )}
    </Container>
  );
};

export default SubscriptionAnalyticsPage;
