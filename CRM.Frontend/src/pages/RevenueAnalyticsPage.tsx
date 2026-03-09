// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
import React, { useCallback, useEffect, useState } from 'react';
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
  Paper,
  Chip,
} from '@mui/material';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';
import ShowChartIcon from '@mui/icons-material/ShowChart';
import GroupIcon from '@mui/icons-material/Group';
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
  revenueAnalyticsService,
  RevenueMetricsDto,
  RevenueMRRMovementDto,
} from '../services/revenueAnalyticsService';

// ── Helpers ────────────────────────────────────────────────────────────────────

const fmt = (v: number): string => {
  if (v >= 1_000_000) return `$${(v / 1_000_000).toFixed(2)}M`;
  if (v >= 1_000) return `$${(v / 1_000).toFixed(1)}K`;
  return `$${v.toFixed(2)}`;
};

const fmtPct = (v: number): string => `${v >= 0 ? '+' : ''}${v.toFixed(2)}%`;

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
  data: RevenueMetricsDto['trend'];
}

const TrendChart: React.FC<TrendChartProps> = ({ data }) => {
  const chartData = data.map((s) => ({
    name: new Date(s.snapshotDate).toLocaleDateString('en-US', { month: 'short', year: '2-digit' }),
    MRR: s.mrr,
    ARR: s.arr,
  }));

  return (
    <Card elevation={2}>
      <CardContent>
        <Typography variant="h6" fontWeight={600} gutterBottom>
          MRR &amp; ARR Trend (Last 12 Months)
        </Typography>
        {chartData.length === 0 ? (
          <Typography variant="body2" color="text.secondary" sx={{ py: 4, textAlign: 'center' }}>
            No trend data available yet. Create revenue snapshots to see trends.
          </Typography>
        ) : (
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={chartData} margin={{ top: 5, right: 30, left: 20, bottom: 5 }}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="name" />
              <YAxis tickFormatter={(v) => fmt(v)} />
              <Tooltip formatter={(value: number) => fmt(value)} />
              <Legend />
              <Line type="monotone" dataKey="MRR" stroke="#1976d2" strokeWidth={2} dot={false} />
              <Line type="monotone" dataKey="ARR" stroke="#9c27b0" strokeWidth={2} dot={false} />
            </LineChart>
          </ResponsiveContainer>
        )}
      </CardContent>
    </Card>
  );
};

// ── Movements Table ────────────────────────────────────────────────────────────

interface MovementsTableProps {
  data: RevenueMRRMovementDto[];
}

const MovementsTable: React.FC<MovementsTableProps> = ({ data }) => (
  <Card elevation={2}>
    <CardContent>
      <Typography variant="h6" fontWeight={600} gutterBottom>
        MRR Movements (Waterfall Breakdown)
      </Typography>
      <TableContainer component={Box}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Period</TableCell>
              <TableCell align="right">Opening MRR</TableCell>
              <TableCell align="right" sx={{ color: 'success.main' }}>
                +New
              </TableCell>
              <TableCell align="right" sx={{ color: 'info.main' }}>
                +Expansion
              </TableCell>
              <TableCell align="right" sx={{ color: 'warning.main' }}>
                -Contraction
              </TableCell>
              <TableCell align="right" sx={{ color: 'error.main' }}>
                -Churn
              </TableCell>
              <TableCell align="right">Closing MRR</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {data.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} align="center">
                  <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
                    No movement data available.
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              data.map((row) => {
                const net = row.closingMRR - row.openingMRR;
                return (
                  <TableRow key={row.period} hover>
                    <TableCell>{row.label}</TableCell>
                    <TableCell align="right">{fmt(row.openingMRR)}</TableCell>
                    <TableCell align="right" sx={{ color: 'success.main' }}>
                      {row.newMRR > 0 ? `+${fmt(row.newMRR)}` : '—'}
                    </TableCell>
                    <TableCell align="right" sx={{ color: 'info.main' }}>
                      {row.expansionMRR > 0 ? `+${fmt(row.expansionMRR)}` : '—'}
                    </TableCell>
                    <TableCell align="right" sx={{ color: 'warning.main' }}>
                      {row.contractionMRR > 0 ? `-${fmt(row.contractionMRR)}` : '—'}
                    </TableCell>
                    <TableCell align="right" sx={{ color: 'error.main' }}>
                      {row.churnMRR > 0 ? `-${fmt(row.churnMRR)}` : '—'}
                    </TableCell>
                    <TableCell align="right">
                      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'flex-end', gap: 0.5 }}>
                        {fmt(row.closingMRR)}
                        <Chip
                          label={`${net >= 0 ? '+' : ''}${fmt(net)}`}
                          size="small"
                          color={net >= 0 ? 'success' : 'error'}
                          variant="outlined"
                        />
                      </Box>
                    </TableCell>
                  </TableRow>
                );
              })
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </CardContent>
  </Card>
);

// ── Snapshot History Table ─────────────────────────────────────────────────────

interface SnapshotTableProps {
  data: RevenueMetricsDto['trend'];
}

const SnapshotTable: React.FC<SnapshotTableProps> = ({ data }) => (
  <Card elevation={2}>
    <CardContent>
      <Typography variant="h6" fontWeight={600} gutterBottom>
        Snapshot History
      </Typography>
      <TableContainer component={Box}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Date</TableCell>
              <TableCell>Type</TableCell>
              <TableCell align="right">MRR</TableCell>
              <TableCell align="right">ARR</TableCell>
              <TableCell align="right">Net New MRR</TableCell>
              <TableCell align="right">Customers</TableCell>
              <TableCell>Notes</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {[...data].reverse().map((s) => (
              <TableRow key={s.id} hover>
                <TableCell>
                  {new Date(s.snapshotDate).toLocaleDateString('en-US', {
                    year: 'numeric',
                    month: 'short',
                    day: 'numeric',
                  })}
                </TableCell>
                <TableCell>
                  <Chip label={s.snapshotType} size="small" variant="outlined" />
                </TableCell>
                <TableCell align="right">{fmt(s.mrr)}</TableCell>
                <TableCell align="right">{fmt(s.arr)}</TableCell>
                <TableCell align="right" sx={{ color: s.netNewMRR >= 0 ? 'success.main' : 'error.main' }}>
                  {s.netNewMRR >= 0 ? '+' : ''}{fmt(s.netNewMRR)}
                </TableCell>
                <TableCell align="right">{s.customerCount.toLocaleString()}</TableCell>
                <TableCell>{s.notes ?? '—'}</TableCell>
              </TableRow>
            ))}
            {data.length === 0 && (
              <TableRow>
                <TableCell colSpan={7} align="center">
                  <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
                    No snapshots recorded yet.
                  </Typography>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </CardContent>
  </Card>
);

// ── Main Page ──────────────────────────────────────────────────────────────────

const RevenueAnalyticsPage: React.FC = () => {
  const [metrics, setMetrics] = useState<RevenueMetricsDto | null>(null);
  const [movements, setMovements] = useState<RevenueMRRMovementDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');

  const load = useCallback(() => {
    setLoading(true);
    setError(null);
    Promise.all([
      revenueAnalyticsService.getMetrics(fromDate || undefined, toDate || undefined),
      revenueAnalyticsService.getMRRMovements(12),
    ])
      .then(([m, mv]) => {
        setMetrics(m);
        setMovements(mv);
      })
      .catch((err) => setError((err as Error)?.message ?? 'Failed to load revenue analytics'))
      .finally(() => setLoading(false));
  }, [fromDate, toDate]);

  useEffect(() => {
    load();
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  const growth = metrics?.moMGrowthRate ?? 0;
  const churnRate = metrics?.churnRate ?? 0;

  return (
    <Box sx={{ p: 3 }}>
      {/* Page Header */}
      <Box sx={{ mb: 3, display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 2 }}>
        <Box>
          <Typography variant="h4" fontWeight={700} gutterBottom>
            Revenue Analytics
          </Typography>
          <Typography variant="body2" color="text.secondary">
            ARR / MRR tracking, growth rates, churn analysis, and revenue movements
          </Typography>
        </Box>
        {/* Date filters */}
        <Box sx={{ display: 'flex', gap: 1, alignItems: 'center', flexWrap: 'wrap' }}>
          <TextField
            label="From"
            type="date"
            size="small"
            InputLabelProps={{ shrink: true }}
            value={fromDate}
            onChange={(e) => setFromDate(e.target.value)}
          />
          <TextField
            label="To"
            type="date"
            size="small"
            InputLabelProps={{ shrink: true }}
            value={toDate}
            onChange={(e) => setToDate(e.target.value)}
          />
          <Button variant="contained" size="medium" onClick={load} disabled={loading}>
            {loading ? <CircularProgress size={18} /> : 'Apply'}
          </Button>
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Row 1: KPI Cards */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <KpiCard
            title="Current MRR"
            value={loading ? '...' : fmt(metrics?.currentMRR ?? 0)}
            subtitle={`ARR: ${loading ? '...' : fmt(metrics?.currentARR ?? 0)}`}
            positive={null}
            icon={<ShowChartIcon />}
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <KpiCard
            title="Annual Recurring Revenue"
            value={loading ? '...' : fmt(metrics?.currentARR ?? 0)}
            subtitle={`${metrics?.totalCustomers ?? 0} paying customers`}
            positive={null}
            icon={<TrendingUpIcon />}
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <KpiCard
            title="MoM Growth"
            value={loading ? '...' : fmtPct(growth)}
            subtitle={growth > 0 ? 'Growing' : growth < 0 ? 'Declining' : 'Stable'}
            positive={growth > 0 ? true : growth < 0 ? false : null}
            icon={growth >= 0 ? <TrendingUpIcon /> : <TrendingDownIcon />}
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <KpiCard
            title="Churn Rate"
            value={loading ? '...' : `${churnRate.toFixed(2)}%`}
            subtitle={`NRR: ${metrics?.netRevenueRetention?.toFixed(1) ?? '—'}%`}
            positive={churnRate === 0 ? null : churnRate < 2 ? true : false}
            icon={<GroupIcon />}
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
          <TrendChart data={metrics?.trend ?? []} />
        )}
      </Box>

      {/* Row 3: MRR Movements */}
      <Box sx={{ mb: 3 }}>
        {loading ? (
          <Card elevation={2}>
            <CardContent sx={{ textAlign: 'center', py: 4 }}>
              <CircularProgress />
            </CardContent>
          </Card>
        ) : (
          <MovementsTable data={movements} />
        )}
      </Box>

      {/* Row 4: Snapshot History */}
      <Box>
        {loading ? (
          <Card elevation={2}>
            <CardContent sx={{ textAlign: 'center', py: 4 }}>
              <CircularProgress />
            </CardContent>
          </Card>
        ) : (
          <SnapshotTable data={metrics?.trend ?? []} />
        )}
      </Box>
    </Box>
  );
};

export default RevenueAnalyticsPage;
