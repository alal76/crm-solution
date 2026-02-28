/**
 * MKT-003: Campaign Analytics Dashboard
 * KPI cards, per-campaign metrics, breakdown table, auto-refresh, CSV export.
 */

import { useState, useEffect, useCallback, useRef } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  LinearProgress,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Alert,
  CircularProgress,
  Switch,
  FormControlLabel,
  Button,
  Stack,
  Chip,
  Container,
  Grid,
  Divider,
  SelectChangeEvent,
  Tooltip,
} from '@mui/material';
import {
  Send as SendIcon,
  Inbox as OpenIcon,
  TouchApp as ClickIcon,
  UnsubscribeOutlined as UnsubIcon,
  Refresh as RefreshIcon,
  ContentCopy as CopyIcon,
  CheckCircle as CheckIcon,
} from '@mui/icons-material';
import marketingService from '../../services/marketingService';
import {
  CampaignExecutionStatusDto,
  CampaignExecutionStatus,
} from '../../types/marketing';
import { Campaign } from '../../types/marketing';
import { PaginatedResponse } from '../../types/common';

// ─── Helpers ─────────────────────────────────────────────────────────────────

function pct(value: number): string {
  return `${(value * 100).toFixed(1)}%`;
}

function fmtDate(iso?: string): string {
  if (!iso) return '—';
  return new Date(iso).toLocaleString();
}

function statusLabel(status: CampaignExecutionStatus): string {
  return CampaignExecutionStatus[status] ?? 'Unknown';
}

function statusColor(
  status: CampaignExecutionStatus
): 'default' | 'primary' | 'success' | 'warning' | 'error' {
  switch (status) {
    case CampaignExecutionStatus.Running: return 'primary';
    case CampaignExecutionStatus.Completed: return 'success';
    case CampaignExecutionStatus.Paused: return 'warning';
    case CampaignExecutionStatus.Cancelled: return 'error';
    case CampaignExecutionStatus.Scheduled: return 'warning';
    default: return 'default';
  }
}

// ─── KPI Card ─────────────────────────────────────────────────────────────────

interface KpiCardProps {
  title: string;
  value: string | number;
  icon: React.ReactNode;
  color?: string;
  subtitle?: string;
}

function KpiCard({ title, value, icon, color = '#6750A4', subtitle }: KpiCardProps) {
  return (
    <Card variant="outlined">
      <CardContent>
        <Stack direction="row" alignItems="flex-start" justifyContent="space-between">
          <Box>
            <Typography variant="caption" color="text.secondary" textTransform="uppercase" letterSpacing={0.5}>
              {title}
            </Typography>
            <Typography variant="h4" fontWeight={700} sx={{ color, mt: 0.5 }}>
              {value}
            </Typography>
            {subtitle && (
              <Typography variant="caption" color="text.secondary">
                {subtitle}
              </Typography>
            )}
          </Box>
          <Box sx={{ color, opacity: 0.7, mt: 0.5 }}>{icon}</Box>
        </Stack>
      </CardContent>
    </Card>
  );
}

// ─── Main component ───────────────────────────────────────────────────────────

export default function CampaignAnalyticsPage() {
  const [campaigns, setCampaigns] = useState<Campaign[]>([]);
  const [selectedCampaignId, setSelectedCampaignId] = useState<number | null>(null);
  const [stats, setStats] = useState<CampaignExecutionStatusDto | null>(null);
  const [loadingCampaigns, setLoadingCampaigns] = useState(true);
  const [loadingStats, setLoadingStats] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [autoRefresh, setAutoRefresh] = useState(false);
  const [copied, setCopied] = useState(false);
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  // ── Load campaigns list ───────────────────────────────────────────────────

  useEffect(() => {
    let cancelled = false;
    setLoadingCampaigns(true);
    marketingService
      .getCampaigns(1, 100)
      .then((res) => {
        if (cancelled) return;
        const data = res.data as PaginatedResponse<Campaign> | Campaign[];
        const items = Array.isArray(data) ? data : (data as PaginatedResponse<Campaign>).items ?? [];
        setCampaigns(items);
        if (items.length > 0 && !selectedCampaignId) {
          setSelectedCampaignId(items[0].id);
        }
      })
      .catch((err: unknown) => {
        if (!cancelled) setError(err instanceof Error ? err.message : 'Failed to load campaigns.');
      })
      .finally(() => {
        if (!cancelled) setLoadingCampaigns(false);
      });
    return () => { cancelled = true; };
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // ── Load stats for selected campaign ─────────────────────────────────────

  const loadStats = useCallback(async (campaignId: number) => {
    setLoadingStats(true);
    setError(null);
    try {
      const data = await marketingService.getCampaignExecutionStatus(campaignId);
      setStats(data);
    } catch {
      // If backend returns 404/error, show empty stats
      setStats(null);
    } finally {
      setLoadingStats(false);
    }
  }, []);

  useEffect(() => {
    if (selectedCampaignId !== null) {
      void loadStats(selectedCampaignId);
    }
  }, [selectedCampaignId, loadStats]);

  // ── Auto-refresh ─────────────────────────────────────────────────────────

  useEffect(() => {
    if (intervalRef.current) clearInterval(intervalRef.current);
    if (autoRefresh && selectedCampaignId !== null) {
      intervalRef.current = setInterval(() => {
        void loadStats(selectedCampaignId);
      }, 30_000);
    }
    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current);
    };
  }, [autoRefresh, selectedCampaignId, loadStats]);

  // ── Aggregate KPIs ────────────────────────────────────────────────────────

  const totalSent = stats?.sendCount ?? 0;
  const openRate = stats?.openRate ?? 0;
  const clickRate = stats?.clickRate ?? 0;
  const unsubRate = stats?.unsubscribeRate ?? 0;

  // ── CSV Copy ──────────────────────────────────────────────────────────────

  const handleCopyCsv = () => {
    if (!stats) return;
    const rows = [
      ['Metric', 'Count', 'Rate'],
      ['Sent', String(stats.sendCount), '—'],
      ['Delivered', String(stats.totalRecipients), '—'],
      ['Opened', String(stats.openCount), pct(stats.openRate)],
      ['Clicked', String(stats.clickCount), pct(stats.clickRate)],
      ['Bounced', String(stats.bounceCount), '—'],
      ['Unsubscribed', String(stats.unsubscribeCount), pct(stats.unsubscribeRate)],
    ];
    const csv = rows.map((r) => r.join(',')).join('\n');
    void navigator.clipboard.writeText(csv).then(() => {
      setCopied(true);
      setTimeout(() => setCopied(false), 2500);
    });
  };

  // ── Render ────────────────────────────────────────────────────────────────

  if (loadingCampaigns) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', mt: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Container maxWidth="xl" sx={{ py: 3 }}>
      {/* Header */}
      <Stack direction="row" alignItems="center" justifyContent="space-between" mb={3}>
        <Box>
          <Typography variant="h5" fontWeight={600}>
            Campaign Analytics
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Real-time execution metrics per campaign.
          </Typography>
        </Box>

        <Stack direction="row" alignItems="center" spacing={2}>
          {/* Auto-refresh toggle */}
          <FormControlLabel
            control={
              <Switch
                checked={autoRefresh}
                onChange={(e) => setAutoRefresh(e.target.checked)}
                size="small"
              />
            }
            label={<Typography variant="body2">Auto-refresh (30s)</Typography>}
          />

          {/* Manual refresh */}
          <Tooltip title="Refresh now">
            <span>
              <Button
                variant="outlined"
                size="small"
                startIcon={<RefreshIcon />}
                onClick={() => selectedCampaignId && void loadStats(selectedCampaignId)}
                disabled={loadingStats || selectedCampaignId === null}
              >
                Refresh
              </Button>
            </span>
          </Tooltip>
        </Stack>
      </Stack>

      {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}

      {/* Campaign selector */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <FormControl size="small" sx={{ minWidth: 320 }}>
            <InputLabel>Select Campaign</InputLabel>
            <Select
              value={selectedCampaignId !== null ? String(selectedCampaignId) : ''}
              label="Select Campaign"
              onChange={(e: SelectChangeEvent) =>
                setSelectedCampaignId(Number(e.target.value))
              }
            >
              {campaigns.map((c) => (
                <MenuItem key={c.id} value={String(c.id)}>
                  {c.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          {/* Date range + status */}
          {stats && (
            <Stack direction="row" spacing={2} alignItems="center" mt={2}>
              <Chip
                label={statusLabel(stats.status)}
                color={statusColor(stats.status)}
                size="small"
              />
              <Typography variant="caption" color="text.secondary">
                Started: {fmtDate(stats.startedAt)}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {stats.status === CampaignExecutionStatus.Completed
                  ? `Completed: ${fmtDate(stats.completedAt)}`
                  : stats.status === CampaignExecutionStatus.Running
                  ? 'Running…'
                  : ''}
              </Typography>
            </Stack>
          )}
        </CardContent>
      </Card>

      {loadingStats && <LinearProgress sx={{ mb: 2 }} />}

      {stats ? (
        <>
          {/* KPI Cards */}
          <Grid container spacing={2} sx={{ mb: 3 }}>
            <Grid item xs={12} sm={6} md={3}>
              <KpiCard
                title="Total Sent"
                value={stats.sendCount.toLocaleString()}
                icon={<SendIcon />}
                color="#1976D2"
                subtitle={`of ${stats.totalRecipients.toLocaleString()} recipients`}
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <KpiCard
                title="Open Rate"
                value={pct(openRate)}
                icon={<OpenIcon />}
                color="#2E7D32"
                subtitle={`${stats.openCount.toLocaleString()} opens`}
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <KpiCard
                title="Click Rate"
                value={pct(clickRate)}
                icon={<ClickIcon />}
                color="#6750A4"
                subtitle={`${stats.clickCount.toLocaleString()} clicks`}
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <KpiCard
                title="Unsubscribe Rate"
                value={pct(unsubRate)}
                icon={<UnsubIcon />}
                color="#D32F2F"
                subtitle={`${stats.unsubscribeCount.toLocaleString()} unsubscribes`}
              />
            </Grid>
          </Grid>

          {/* Metric bars */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="subtitle2" mb={2}>Delivery Funnel</Typography>
              {[
                { label: 'Delivered', value: stats.totalRecipients, total: stats.totalRecipients, color: '#1976D2' },
                { label: 'Opened', value: stats.openCount, total: stats.totalRecipients, color: '#2E7D32' },
                { label: 'Clicked', value: stats.clickCount, total: stats.totalRecipients, color: '#6750A4' },
                { label: 'Bounced', value: stats.bounceCount, total: stats.totalRecipients, color: '#FF8F00' },
                { label: 'Unsubscribed', value: stats.unsubscribeCount, total: stats.totalRecipients, color: '#D32F2F' },
              ].map((bar) => (
                <Box key={bar.label} sx={{ mb: 1.5 }}>
                  <Stack direction="row" justifyContent="space-between" mb={0.5}>
                    <Typography variant="caption">{bar.label}</Typography>
                    <Typography variant="caption" color="text.secondary">
                      {bar.value.toLocaleString()}
                      {bar.total > 0 ? ` (${((bar.value / bar.total) * 100).toFixed(1)}%)` : ''}
                    </Typography>
                  </Stack>
                  <LinearProgress
                    variant="determinate"
                    value={bar.total > 0 ? Math.min((bar.value / bar.total) * 100, 100) : 0}
                    sx={{
                      height: 6,
                      borderRadius: 3,
                      bgcolor: 'grey.100',
                      '& .MuiLinearProgress-bar': { bgcolor: bar.color, borderRadius: 3 },
                    }}
                  />
                </Box>
              ))}
            </CardContent>
          </Card>

          {/* Breakdown table */}
          <Card>
            <CardContent>
              <Stack direction="row" justifyContent="space-between" alignItems="center" mb={2}>
                <Typography variant="subtitle2">Metric Breakdown</Typography>
                <Button
                  variant="outlined"
                  size="small"
                  startIcon={copied ? <CheckIcon /> : <CopyIcon />}
                  onClick={handleCopyCsv}
                  color={copied ? 'success' : 'primary'}
                >
                  {copied ? 'Copied!' : 'Copy CSV'}
                </Button>
              </Stack>

              <TableContainer component={Paper} elevation={0} variant="outlined">
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Metric</TableCell>
                      <TableCell align="right">Count</TableCell>
                      <TableCell align="right">Rate</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {[
                      { label: 'Sent', count: stats.sendCount, rate: null },
                      { label: 'Delivered', count: stats.totalRecipients, rate: null },
                      { label: 'Opened', count: stats.openCount, rate: openRate },
                      { label: 'Clicked', count: stats.clickCount, rate: clickRate },
                      { label: 'Bounced', count: stats.bounceCount, rate: null },
                      { label: 'Unsubscribed', count: stats.unsubscribeCount, rate: unsubRate },
                    ].map((row) => (
                      <TableRow key={row.label} hover>
                        <TableCell>{row.label}</TableCell>
                        <TableCell align="right">
                          {row.count.toLocaleString()}
                        </TableCell>
                        <TableCell align="right">
                          {row.rate !== null ? pct(row.rate) : '—'}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        </>
      ) : (
        !loadingStats && selectedCampaignId !== null && (
          <Card>
            <CardContent sx={{ textAlign: 'center', py: 6 }}>
              <Typography color="text.secondary" variant="body2">
                No execution data available for this campaign.
              </Typography>
            </CardContent>
          </Card>
        )
      )}
    </Container>
  );
}
