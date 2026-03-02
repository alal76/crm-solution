/**
 * SubscriptionDetailPage - Full subscription detail with tabs for Overview, Billing, Usage, Timeline
 * Route: /subscriptions/:id
 */
import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  Button,
  CircularProgress,
  Alert,
  Chip,
  Grid,
  Divider,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Tabs,
  Tab,
  IconButton,
  Tooltip,
} from '@mui/material';
import {
  ArrowBack as ArrowBackIcon,
  Receipt as InvoiceIcon,
  Refresh as RefreshIcon,
  PlayArrow as ActivateIcon,
  Pause as PauseIcon,
  PlayCircle as ResumeIcon,
  Cancel as CancelIcon,
  Autorenew as RenewIcon,
} from '@mui/icons-material';
import { useParams, useNavigate } from 'react-router-dom';
import { TabPanel } from '../../components/common';
import subscriptionService, {
  Subscription,
  SubscriptionStatus,
  getStatusLabel,
  getStatusColor,
  Invoice,
} from '../../services/subscriptionService';
import billingService, {
  BillingHistoryDto,
  UsageRecordDto,
} from '../../services/billingService';
import SubscriptionCard from '../../components/sales/SubscriptionCard';
import UsageChart from '../../components/sales/UsageChart';
import SubscriptionTimeline from '../../components/sales/SubscriptionTimeline';
import type { TimelineEvent } from '../../components/sales/SubscriptionTimeline';

const buildTimeline = (sub: Subscription): TimelineEvent[] => {
  const events: TimelineEvent[] = [];
  if (sub.createdAt) {
    events.push({ date: sub.createdAt, event: 'Subscription Created', type: 'created' });
  }
  if (sub.startDate && sub.startDate !== sub.createdAt) {
    events.push({ date: sub.startDate, event: 'Subscription Started', type: 'created' });
  }
  if (sub.pausedAt) {
    events.push({
      date: sub.pausedAt,
      event: 'Subscription Paused',
      description: sub.pauseReason || undefined,
      type: 'paused',
    });
  }
  if (sub.cancelledAt) {
    events.push({
      date: sub.cancelledAt,
      event: 'Subscription Cancelled',
      description: sub.cancellationReason || undefined,
      type: 'cancelled',
    });
  }
  if (sub.renewalDate) {
    events.push({ date: sub.renewalDate, event: 'Renewal Scheduled', type: 'renewed' });
  }
  // Sort descending
  events.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
  return events;
};

const formatDate = (d?: string): string => {
  if (!d) return '—';
  return new Date(d).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
};

const formatCurrency = (amount: number, currency?: string): string => {
  try {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: currency || 'USD' }).format(amount);
  } catch {
    return `${currency || 'USD'} ${amount.toFixed(2)}`;
  }
};

const SubscriptionDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [subscription, setSubscription] = useState<Subscription | null>(null);
  const [billingHistory, setBillingHistory] = useState<BillingHistoryDto[]>([]);
  const [usageRecords, setUsageRecords] = useState<UsageRecordDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [tabIndex, setTabIndex] = useState(0);
  const [actionLoading, setActionLoading] = useState(false);

  const subscriptionId = Number(id);

  const loadSubscription = useCallback(async () => {
    if (!id || Number.isNaN(subscriptionId)) {
      setError('Invalid subscription ID');
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const res = await subscriptionService.getById(subscriptionId);
      setSubscription(res.data);
    } catch (err) {
      console.error('Failed to load subscription', err);
      setError('Failed to load subscription details.');
    } finally {
      setLoading(false);
    }
  }, [id, subscriptionId]);

  const loadBilling = useCallback(async () => {
    if (Number.isNaN(subscriptionId)) return;
    try {
      const data = await billingService.getBillingHistory(subscriptionId);
      setBillingHistory(data);
    } catch {
      // Non-critical — billing may not be available
      setBillingHistory([]);
    }
  }, [subscriptionId]);

  const loadUsage = useCallback(async () => {
    if (Number.isNaN(subscriptionId)) return;
    try {
      const data = await billingService.getUsageRecords(subscriptionId);
      setUsageRecords(data);
    } catch {
      setUsageRecords([]);
    }
  }, [subscriptionId]);

  useEffect(() => {
    loadSubscription();
    loadBilling();
    loadUsage();
  }, [loadSubscription, loadBilling, loadUsage]);

  const handleAction = async (action: () => Promise<unknown>) => {
    setActionLoading(true);
    try {
      await action();
      await loadSubscription();
    } catch (err) {
      console.error('Action failed', err);
      setError('Action failed. Please try again.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleGenerateInvoice = () => handleAction(() => billingService.generateInvoice(subscriptionId));
  const handleActivate = () => handleAction(() => subscriptionService.activate(subscriptionId));
  const handlePause = () => handleAction(() => subscriptionService.pause(subscriptionId));
  const handleResume = () => handleAction(() => subscriptionService.resume(subscriptionId));
  const handleCancel = () => handleAction(() => subscriptionService.cancel(subscriptionId, 'User requested'));
  const handleRenew = () => handleAction(() => subscriptionService.renew(subscriptionId));

  if (loading) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (error && !subscription) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/subscriptions')}>
          Back to Subscriptions
        </Button>
      </Container>
    );
  }

  if (!subscription) return null;

  const status = subscription.subscriptionStatus;
  const timeline = buildTimeline(subscription);

  return (
    <Container maxWidth="lg" sx={{ py: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Stack direction="row" spacing={2} alignItems="center">
          <IconButton onClick={() => navigate('/subscriptions')}>
            <ArrowBackIcon />
          </IconButton>
          <Box>
            <Typography variant="h5" fontWeight={700}>
              {subscription.subscriptionNumber || `Subscription #${subscription.id}`}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {subscription.product?.name || 'No product'}
              {subscription.account?.company ? ` • ${subscription.account.company}` : ''}
            </Typography>
          </Box>
          <Chip
            label={getStatusLabel(status)}
            color={getStatusColor(status)}
            size="small"
          />
        </Stack>

        <Stack direction="row" spacing={1}>
          {status === SubscriptionStatus.Trial && (
            <Tooltip title="Activate">
              <IconButton color="success" onClick={handleActivate} disabled={actionLoading}>
                <ActivateIcon />
              </IconButton>
            </Tooltip>
          )}
          {status === SubscriptionStatus.Active && (
            <Tooltip title="Pause">
              <IconButton color="warning" onClick={handlePause} disabled={actionLoading}>
                <PauseIcon />
              </IconButton>
            </Tooltip>
          )}
          {status === SubscriptionStatus.Paused && (
            <Tooltip title="Resume">
              <IconButton color="success" onClick={handleResume} disabled={actionLoading}>
                <ResumeIcon />
              </IconButton>
            </Tooltip>
          )}
          {(status === SubscriptionStatus.Active || status === SubscriptionStatus.Trial) && (
            <Tooltip title="Cancel">
              <IconButton color="error" onClick={handleCancel} disabled={actionLoading}>
                <CancelIcon />
              </IconButton>
            </Tooltip>
          )}
          {status === SubscriptionStatus.Active && (
            <Tooltip title="Renew">
              <IconButton color="primary" onClick={handleRenew} disabled={actionLoading}>
                <RenewIcon />
              </IconButton>
            </Tooltip>
          )}
          <Tooltip title="Generate Invoice">
            <IconButton onClick={handleGenerateInvoice} disabled={actionLoading}>
              <InvoiceIcon />
            </IconButton>
          </Tooltip>
          <Tooltip title="Refresh">
            <IconButton onClick={loadSubscription}>
              <RefreshIcon />
            </IconButton>
          </Tooltip>
        </Stack>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {/* Tabs */}
      <Paper variant="outlined" sx={{ mb: 3 }}>
        <Tabs value={tabIndex} onChange={(_, v) => setTabIndex(v)} sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tab label="Overview" />
          <Tab label="Billing History" />
          <Tab label="Usage" />
          <Tab label="Timeline" />
        </Tabs>

        {/* Overview Tab */}
        <TabPanel value={tabIndex} index={0}>
          <Grid container spacing={3}>
            <Grid item xs={12} md={4}>
              <SubscriptionCard
                id={subscription.id}
                planName={subscription.product?.name || 'Subscription'}
                status={getStatusLabel(status)}
                amount={subscription.amount}
                currency={subscription.currency}
                billingCycle={subscription.billingCycle || 'Monthly'}
                nextBillingDate={subscription.nextBillingDate}
              />
            </Grid>
            <Grid item xs={12} md={8}>
              <Card variant="outlined">
                <CardContent>
                  <Typography variant="h6" gutterBottom>Details</Typography>
                  <Divider sx={{ mb: 2 }} />
                  <Grid container spacing={2}>
                    <Grid item xs={6} sm={4}>
                      <Typography variant="caption" color="text.secondary">Start Date</Typography>
                      <Typography variant="body2">{formatDate(subscription.startDate)}</Typography>
                    </Grid>
                    <Grid item xs={6} sm={4}>
                      <Typography variant="caption" color="text.secondary">End Date</Typography>
                      <Typography variant="body2">{formatDate(subscription.endDate)}</Typography>
                    </Grid>
                    <Grid item xs={6} sm={4}>
                      <Typography variant="caption" color="text.secondary">Auto Renew</Typography>
                      <Typography variant="body2">{subscription.isAutoRenew ? 'Yes' : 'No'}</Typography>
                    </Grid>
                    <Grid item xs={6} sm={4}>
                      <Typography variant="caption" color="text.secondary">MRR</Typography>
                      <Typography variant="body2">{formatCurrency(subscription.mrr || 0, subscription.currency)}</Typography>
                    </Grid>
                    <Grid item xs={6} sm={4}>
                      <Typography variant="caption" color="text.secondary">ARR</Typography>
                      <Typography variant="body2">{formatCurrency(subscription.arr || 0, subscription.currency)}</Typography>
                    </Grid>
                    <Grid item xs={6} sm={4}>
                      <Typography variant="caption" color="text.secondary">Billing Contact</Typography>
                      <Typography variant="body2">{subscription.billingContactName || '—'}</Typography>
                    </Grid>
                    {subscription.contractReference && (
                      <Grid item xs={12}>
                        <Typography variant="caption" color="text.secondary">Contract Reference</Typography>
                        <Typography variant="body2">{subscription.contractReference}</Typography>
                      </Grid>
                    )}
                    {subscription.tags && (
                      <Grid item xs={12}>
                        <Typography variant="caption" color="text.secondary">Tags</Typography>
                        <Box sx={{ mt: 0.5 }}>
                          {subscription.tags.split(',').map((tag, i) => (
                            <Chip key={i} label={tag.trim()} size="small" sx={{ mr: 0.5, mb: 0.5 }} />
                          ))}
                        </Box>
                      </Grid>
                    )}
                  </Grid>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </TabPanel>

        {/* Billing History Tab */}
        <TabPanel value={tabIndex} index={1}>
          {billingHistory.length === 0 ? (
            <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
              No billing history available
            </Typography>
          ) : (
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Invoice #</TableCell>
                    <TableCell>Billing Date</TableCell>
                    <TableCell>Due Date</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell align="right">Amount</TableCell>
                    <TableCell>Paid Date</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {billingHistory.map((invoice) => (
                    <TableRow key={invoice.id}>
                      <TableCell>{invoice.invoiceNumber}</TableCell>
                      <TableCell>{formatDate(invoice.billingDate)}</TableCell>
                      <TableCell>{formatDate(invoice.dueDate)}</TableCell>
                      <TableCell>
                        <Chip
                          label={invoice.status}
                          size="small"
                          color={invoice.status === 'Paid' ? 'success' : invoice.status === 'Overdue' ? 'error' : 'default'}
                        />
                      </TableCell>
                      <TableCell align="right">{formatCurrency(invoice.amount, invoice.currency)}</TableCell>
                      <TableCell>{formatDate(invoice.paidDate)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </TabPanel>

        {/* Usage Tab */}
        <TabPanel value={tabIndex} index={2}>
          <UsageChart usageData={usageRecords} />
          {usageRecords.length > 0 && (
            <Box sx={{ mt: 3 }}>
              <Typography variant="subtitle2" gutterBottom>Usage Records</Typography>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Metric</TableCell>
                      <TableCell align="right">Quantity</TableCell>
                      <TableCell align="right">Unit Price</TableCell>
                      <TableCell align="right">Total</TableCell>
                      <TableCell>Recorded At</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {usageRecords.map((record) => (
                      <TableRow key={record.id}>
                        <TableCell>{record.metricName}</TableCell>
                        <TableCell align="right">{record.quantity.toLocaleString()}</TableCell>
                        <TableCell align="right">{formatCurrency(record.unitPrice)}</TableCell>
                        <TableCell align="right">{formatCurrency(record.total)}</TableCell>
                        <TableCell>{formatDate(record.recordedAt)}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </Box>
          )}
        </TabPanel>

        {/* Timeline Tab */}
        <TabPanel value={tabIndex} index={3}>
          <SubscriptionTimeline events={timeline} />
        </TabPanel>
      </Paper>
    </Container>
  );
};

export default SubscriptionDetailPage;
