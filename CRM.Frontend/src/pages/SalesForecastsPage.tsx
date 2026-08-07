// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  TextField,
  Button,
  CircularProgress,
  Alert,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  MenuItem,
  Accordion,
  AccordionSummary,
  AccordionDetails,
} from '@mui/material';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import FlagIcon from '@mui/icons-material/Flag';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import SpeedIcon from '@mui/icons-material/Speed';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import {
  ResponsiveContainer,
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
} from 'recharts';
import {
  salesForecastService,
  SalesForecastDto,
  ForecastLineItemDto,
  ForecastHistoryDto,
  ForecastCategory,
  FORECAST_CATEGORY_LABELS,
  SalesForecastFilters,
} from '../services/salesForecastService';

// ── Helpers ────────────────────────────────────────────────────────────────────

const fmt = (v: number): string => {
  if (v >= 1_000_000) return `$${(v / 1_000_000).toFixed(2)}M`;
  if (v >= 1_000) return `$${(v / 1_000).toFixed(1)}K`;
  return `$${v.toFixed(2)}`;
};

const forecastAmountOf = (f: SalesForecastDto): number =>
  f.forecastAmount ?? f.closedWonAmount + f.commitAmount;

const gapToQuotaOf = (f: SalesForecastDto): number =>
  f.gapToQuota ?? Math.max(0, f.quotaAmount - forecastAmountOf(f));

const attainmentOf = (f: SalesForecastDto): number =>
  f.forecastAttainmentPercent ?? (f.quotaAmount > 0 ? (forecastAmountOf(f) / f.quotaAmount) * 100 : 0);

// ── KPI Card ───────────────────────────────────────────────────────────────────

interface KpiCardProps {
  title: string;
  value: string;
  subtitle?: string;
  positive?: boolean | null;
  icon: React.ReactNode;
}

const KpiCard: React.FC<KpiCardProps> = ({ title, value, subtitle, positive, icon }) => (
  <Card elevation={2} sx={{ height: '100%' }}>
    <CardContent>
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 1, gap: 1 }}>
        <Box sx={{ color: 'primary.main' }}>{icon}</Box>
        <Typography variant="subtitle2" color="text.secondary">
          {title}
        </Typography>
      </Box>
      <Typography variant="h5" fontWeight={700} color="text.primary">
        {value}
      </Typography>
      {subtitle && (
        <Typography
          variant="body2"
          sx={{
            mt: 0.5,
            color: positive === true ? 'success.main' : positive === false ? 'error.main' : 'text.secondary',
          }}
        >
          {subtitle}
        </Typography>
      )}
    </CardContent>
  </Card>
);

// ── Trend Chart ────────────────────────────────────────────────────────────────

interface TrendChartProps {
  data: ForecastHistoryDto[];
}

const TrendChart: React.FC<TrendChartProps> = ({ data }) => {
  const chartData = data.map((s) => ({
    name: new Date(s.snapshotDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
    Quota: s.quotaAmount,
    'Closed Won': s.closedWonAmount,
    Commit: s.commitAmount,
    Pipeline: s.pipelineAmount,
  }));

  return (
    <Card elevation={2}>
      <CardContent>
        <Typography variant="h6" fontWeight={600} gutterBottom>
          Forecast Trend
        </Typography>
        {chartData.length === 0 ? (
          <Typography variant="body2" color="text.secondary" sx={{ py: 4, textAlign: 'center' }}>
            No forecast history available for this period yet. Create a snapshot to start tracking trends.
          </Typography>
        ) : (
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={chartData} margin={{ top: 5, right: 30, left: 20, bottom: 5 }}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="name" />
              <YAxis tickFormatter={(v) => fmt(v)} />
              <Tooltip formatter={(value: number) => fmt(value)} />
              <Legend />
              <Line type="monotone" dataKey="Quota" stroke="#616161" strokeWidth={2} dot={false} />
              <Line type="monotone" dataKey="Closed Won" stroke="#2e7d32" strokeWidth={2} dot={false} />
              <Line type="monotone" dataKey="Commit" stroke="#1976d2" strokeWidth={2} dot={false} />
              <Line type="monotone" dataKey="Pipeline" stroke="#ed6c02" strokeWidth={2} dot={false} />
            </LineChart>
          </ResponsiveContainer>
        )}
      </CardContent>
    </Card>
  );
};

// ── Forecasts Table ────────────────────────────────────────────────────────────

interface ForecastsTableProps {
  data: SalesForecastDto[];
  selectedId: number | null;
  onSelect: (forecast: SalesForecastDto) => void;
  onSubmit: (forecast: SalesForecastDto) => void;
  submitting: number | null;
}

const ForecastsTable: React.FC<ForecastsTableProps> = ({ data, selectedId, onSelect, onSubmit, submitting }) => (
  <Card elevation={2}>
    <CardContent>
      <Typography variant="h6" fontWeight={600} gutterBottom>
        Forecasts
      </Typography>
      <TableContainer component={Box}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Period</TableCell>
              <TableCell align="right">Quota</TableCell>
              <TableCell align="right">Forecast</TableCell>
              <TableCell align="right">Closed Won</TableCell>
              <TableCell align="right">Attainment</TableCell>
              <TableCell>Status</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {data.length === 0 ? (
              <TableRow>
                <TableCell colSpan={8} align="center">
                  <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
                    No sales forecasts found for the selected filters.
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              data.map((f) => (
                <TableRow
                  key={f.id}
                  hover
                  selected={f.id === selectedId}
                  onClick={() => onSelect(f)}
                  sx={{ cursor: 'pointer' }}
                >
                  <TableCell>{f.name}</TableCell>
                  <TableCell>{f.period}</TableCell>
                  <TableCell align="right">{fmt(f.quotaAmount)}</TableCell>
                  <TableCell align="right">{fmt(forecastAmountOf(f))}</TableCell>
                  <TableCell align="right">{fmt(f.closedWonAmount)}</TableCell>
                  <TableCell align="right">{attainmentOf(f).toFixed(1)}%</TableCell>
                  <TableCell>
                    <Chip
                      label={f.isSubmitted ? 'Submitted' : 'Draft'}
                      size="small"
                      color={f.isSubmitted ? 'success' : 'default'}
                      variant={f.isSubmitted ? 'filled' : 'outlined'}
                    />
                  </TableCell>
                  <TableCell align="right">
                    <Button
                      size="small"
                      variant="outlined"
                      disabled={f.isSubmitted || submitting === f.id}
                      onClick={(e) => {
                        e.stopPropagation();
                        onSubmit(f);
                      }}
                    >
                      {submitting === f.id ? <CircularProgress size={14} /> : 'Submit'}
                    </Button>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </CardContent>
  </Card>
);

// ── Line Items Panel (grouped by ForecastCategory) ─────────────────────────────

interface LineItemsPanelProps {
  forecast: SalesForecastDto;
  lineItems: ForecastLineItemDto[];
  loading: boolean;
}

const LineItemsPanel: React.FC<LineItemsPanelProps> = ({ forecast, lineItems, loading }) => {
  const grouped = useMemo(() => {
    const groups = new Map<ForecastCategory, ForecastLineItemDto[]>();
    lineItems.forEach((li) => {
      const key = li.overrideCategory ?? li.category;
      const list = groups.get(key) ?? [];
      list.push(li);
      groups.set(key, list);
    });
    return groups;
  }, [lineItems]);

  const categoryOrder: ForecastCategory[] = [
    ForecastCategory.ClosedWon,
    ForecastCategory.Commit,
    ForecastCategory.BestCase,
    ForecastCategory.Pipeline,
    ForecastCategory.MostLikely,
    ForecastCategory.Omitted,
  ];

  return (
    <Card elevation={2}>
      <CardContent>
        <Typography variant="h6" fontWeight={600} gutterBottom>
          Line Items — {forecast.name}
        </Typography>
        {loading ? (
          <Box sx={{ textAlign: 'center', py: 4 }}>
            <CircularProgress size={24} />
          </Box>
        ) : lineItems.length === 0 ? (
          <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
            No line items recorded for this forecast.
          </Typography>
        ) : (
          categoryOrder
            .filter((cat) => grouped.has(cat))
            .map((cat) => {
              const items = grouped.get(cat) ?? [];
              const total = items.reduce((sum, li) => sum + (li.overrideAmount ?? li.amount), 0);
              return (
                <Accordion key={cat} disableGutters defaultExpanded>
                  <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', width: '100%', pr: 2 }}>
                      <Typography fontWeight={600}>
                        {FORECAST_CATEGORY_LABELS[cat]} ({items.length})
                      </Typography>
                      <Typography fontWeight={600}>{fmt(total)}</Typography>
                    </Box>
                  </AccordionSummary>
                  <AccordionDetails>
                    <TableContainer component={Box}>
                      <Table size="small">
                        <TableHead>
                          <TableRow>
                            <TableCell>Opportunity ID</TableCell>
                            <TableCell>Stage</TableCell>
                            <TableCell align="right">Probability</TableCell>
                            <TableCell>Close Date</TableCell>
                            <TableCell align="right">Amount</TableCell>
                          </TableRow>
                        </TableHead>
                        <TableBody>
                          {items.map((li) => (
                            <TableRow key={li.id}>
                              <TableCell>{li.opportunityId}</TableCell>
                              <TableCell>{li.stage ?? '—'}</TableCell>
                              <TableCell align="right">{li.probability}%</TableCell>
                              <TableCell>{new Date(li.closeDate).toLocaleDateString()}</TableCell>
                              <TableCell align="right">{fmt(li.overrideAmount ?? li.amount)}</TableCell>
                            </TableRow>
                          ))}
                        </TableBody>
                      </Table>
                    </TableContainer>
                  </AccordionDetails>
                </Accordion>
              );
            })
        )}
      </CardContent>
    </Card>
  );
};

// ── Main Page ──────────────────────────────────────────────────────────────────

const SalesForecastsPage: React.FC = () => {
  const currentYear = new Date().getFullYear();

  const [forecasts, setForecasts] = useState<SalesForecastDto[]>([]);
  const [history, setHistory] = useState<ForecastHistoryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Filters
  const [userIdFilter, setUserIdFilter] = useState('');
  const [teamIdFilter, setTeamIdFilter] = useState('');
  const [fiscalYearFilter, setFiscalYearFilter] = useState(String(currentYear));
  const [isSubmittedFilter, setIsSubmittedFilter] = useState<'all' | 'true' | 'false'>('all');
  const [trendPeriod, setTrendPeriod] = useState(String(currentYear));

  // Selection / drill-in
  const [selectedForecast, setSelectedForecast] = useState<SalesForecastDto | null>(null);
  const [lineItems, setLineItems] = useState<ForecastLineItemDto[]>([]);
  const [lineItemsLoading, setLineItemsLoading] = useState(false);
  const [submitting, setSubmitting] = useState<number | null>(null);

  const buildFilters = useCallback((): SalesForecastFilters => {
    const filters: SalesForecastFilters = {};
    if (userIdFilter.trim()) filters.userId = Number(userIdFilter);
    if (teamIdFilter.trim()) filters.teamId = Number(teamIdFilter);
    if (fiscalYearFilter.trim()) filters.fiscalYear = Number(fiscalYearFilter);
    if (isSubmittedFilter !== 'all') filters.isSubmitted = isSubmittedFilter === 'true';
    return filters;
  }, [userIdFilter, teamIdFilter, fiscalYearFilter, isSubmittedFilter]);

  const load = useCallback(() => {
    setLoading(true);
    setError(null);
    const userId = userIdFilter.trim() ? Number(userIdFilter) : undefined;
    Promise.all([
      salesForecastService.getAll(buildFilters()),
      salesForecastService.getHistory(trendPeriod || String(currentYear), userId),
    ])
      .then(([f, h]) => {
        setForecasts(f);
        setHistory(h);
      })
      .catch((err) => setError((err as Error)?.message ?? 'Failed to load sales forecasts'))
      .finally(() => setLoading(false));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [buildFilters, trendPeriod, currentYear, userIdFilter]);

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleSelect = (forecast: SalesForecastDto) => {
    setSelectedForecast(forecast);
    setLineItemsLoading(true);
    salesForecastService
      .getLineItems(forecast.id)
      .then(setLineItems)
      .catch((err) => setError((err as Error)?.message ?? 'Failed to load line items'))
      .finally(() => setLineItemsLoading(false));
  };

  const handleSubmit = (forecast: SalesForecastDto) => {
    setSubmitting(forecast.id);
    salesForecastService
      .submit(forecast.id)
      .then(() => {
        setForecasts((prev) =>
          prev.map((f) => (f.id === forecast.id ? { ...f, isSubmitted: true, submittedAt: new Date().toISOString() } : f))
        );
        if (selectedForecast?.id === forecast.id) {
          setSelectedForecast((prev) => (prev ? { ...prev, isSubmitted: true } : prev));
        }
      })
      .catch((err) => setError((err as Error)?.message ?? 'Failed to submit forecast'))
      .finally(() => setSubmitting(null));
  };

  // KPI aggregation across the currently filtered forecast list
  const totals = useMemo(() => {
    const totalQuota = forecasts.reduce((sum, f) => sum + f.quotaAmount, 0);
    const totalForecast = forecasts.reduce((sum, f) => sum + forecastAmountOf(f), 0);
    const totalClosedWon = forecasts.reduce((sum, f) => sum + f.closedWonAmount, 0);
    const totalPipeline = forecasts.reduce((sum, f) => sum + f.pipelineAmount, 0);
    const totalGap = forecasts.reduce((sum, f) => sum + gapToQuotaOf(f), 0);
    const coverageRatio = totalGap > 0 ? totalPipeline / totalGap : 0;
    const attainment = totalQuota > 0 ? (totalForecast / totalQuota) * 100 : 0;
    return { totalQuota, totalForecast, totalClosedWon, coverageRatio, attainment };
  }, [forecasts]);

  return (
    <Box sx={{ p: 3 }}>
      {/* Page Header */}
      <Box sx={{ mb: 3, display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 2 }}>
        <Box>
          <Typography variant="h4" sx={{ fontWeight: 700 }} gutterBottom>
            Sales Forecasts
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Quota attainment, pipeline coverage, and forecast category tracking
          </Typography>
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Filters */}
      <Box sx={{ mb: 3, display: 'flex', gap: 1, alignItems: 'center', flexWrap: 'wrap' }}>
        <TextField
          label="User ID"
          size="small"
          value={userIdFilter}
          onChange={(e) => setUserIdFilter(e.target.value)}
          sx={{ width: 110 }}
        />
        <TextField
          label="Team ID"
          size="small"
          value={teamIdFilter}
          onChange={(e) => setTeamIdFilter(e.target.value)}
          sx={{ width: 110 }}
        />
        <TextField
          label="Fiscal Year"
          size="small"
          value={fiscalYearFilter}
          onChange={(e) => setFiscalYearFilter(e.target.value)}
          sx={{ width: 130 }}
        />
        <TextField
          label="Status"
          select
          size="small"
          value={isSubmittedFilter}
          onChange={(e) => setIsSubmittedFilter(e.target.value as 'all' | 'true' | 'false')}
          sx={{ width: 140 }}
        >
          <MenuItem value="all">All</MenuItem>
          <MenuItem value="true">Submitted</MenuItem>
          <MenuItem value="false">Draft</MenuItem>
        </TextField>
        <TextField
          label="Trend Period"
          size="small"
          value={trendPeriod}
          onChange={(e) => setTrendPeriod(e.target.value)}
          sx={{ width: 130 }}
        />
        <Button variant="contained" size="medium" onClick={load} disabled={loading}>
          {loading ? <CircularProgress size={18} /> : 'Apply'}
        </Button>
      </Box>

      {/* Row 1: KPI Cards */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <KpiCard
            title="Total Quota"
            value={loading ? '...' : fmt(totals.totalQuota)}
            subtitle={`FY ${fiscalYearFilter || currentYear}`}
            positive={null}
            icon={<FlagIcon />}
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <KpiCard
            title="Total Forecast"
            value={loading ? '...' : fmt(totals.totalForecast)}
            subtitle={`${totals.attainment.toFixed(1)}% of quota`}
            positive={totals.attainment >= 100 ? true : totals.attainment >= 70 ? null : false}
            icon={<TrendingUpIcon />}
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <KpiCard
            title="Closed Won"
            value={loading ? '...' : fmt(totals.totalClosedWon)}
            subtitle={`${forecasts.length} forecast${forecasts.length === 1 ? '' : 's'}`}
            positive={null}
            icon={<CheckCircleIcon />}
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <KpiCard
            title="Coverage Ratio"
            value={loading ? '...' : `${totals.coverageRatio.toFixed(2)}x`}
            subtitle={totals.coverageRatio >= 3 ? 'Healthy pipeline' : totals.coverageRatio > 0 ? 'Below target (3x)' : 'Quota met'}
            positive={totals.coverageRatio === 0 ? null : totals.coverageRatio >= 3 ? true : false}
            icon={<SpeedIcon />}
          />
        </Grid>
      </Grid>

      {/* Row 2: Trend Chart */}
      <Box sx={{ mb: 3 }}>
        {loading ? (
          <Card elevation={2}>
            <CardContent sx={{ textAlign: 'center', py: 8 }}>
              <CircularProgress />
            </CardContent>
          </Card>
        ) : (
          <TrendChart data={history} />
        )}
      </Box>

      {/* Row 3: Forecasts Table */}
      <Box sx={{ mb: 3 }}>
        {loading ? (
          <Card elevation={2}>
            <CardContent sx={{ textAlign: 'center', py: 4 }}>
              <CircularProgress />
            </CardContent>
          </Card>
        ) : (
          <ForecastsTable
            data={forecasts}
            selectedId={selectedForecast?.id ?? null}
            onSelect={handleSelect}
            onSubmit={handleSubmit}
            submitting={submitting}
          />
        )}
      </Box>

      {/* Row 4: Line Items drill-in */}
      {selectedForecast && (
        <Box>
          <LineItemsPanel forecast={selectedForecast} lineItems={lineItems} loading={lineItemsLoading} />
        </Box>
      )}
    </Box>
  );
};

export default SalesForecastsPage;
