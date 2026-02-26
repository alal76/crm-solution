// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Box,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Container,
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
  TextField,
  Typography,
} from '@mui/material';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import StarIcon from '@mui/icons-material/Star';
import PeopleIcon from '@mui/icons-material/People';
import SentimentSatisfiedAltIcon from '@mui/icons-material/SentimentSatisfiedAlt';
import satisfactionService, {
  SatisfactionMetricsDto,
  SatisfactionSurveyDto,
  SurveyStatus,
  SurveyType,
} from '../services/satisfactionService';

// ── Helpers ────────────────────────────────────────────────────────────────────

const surveyTypeLabel: Record<SurveyType, string> = {
  [SurveyType.CSAT]: 'CSAT',
  [SurveyType.NPS]: 'NPS',
  [SurveyType.CES]: 'CES',
};

const statusColour: Record<SurveyStatus, 'default' | 'info' | 'success' | 'error' | 'warning'> = {
  [SurveyStatus.Pending]: 'default',
  [SurveyStatus.Sent]: 'info',
  [SurveyStatus.Responded]: 'success',
  [SurveyStatus.Expired]: 'error',
  [SurveyStatus.Cancelled]: 'warning',
};

const statusLabel: Record<SurveyStatus, string> = {
  [SurveyStatus.Pending]: 'Pending',
  [SurveyStatus.Sent]: 'Sent',
  [SurveyStatus.Responded]: 'Responded',
  [SurveyStatus.Expired]: 'Expired',
  [SurveyStatus.Cancelled]: 'Cancelled',
};

// ── Main page ─────────────────────────────────────────────────────────────────

const SatisfactionDashboardPage: React.FC = () => {
  const [metrics, setMetrics] = useState<SatisfactionMetricsDto | null>(null);
  const [surveys, setSurveys] = useState<SatisfactionSurveyDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');

  const loadData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [metricsData, surveysData] = await Promise.all([
        satisfactionService.getMetrics(
          fromDate || undefined,
          toDate || undefined,
        ),
        satisfactionService.getSurveys(1, 15),
      ]);
      setMetrics(metricsData);
      setSurveys(surveysData.items);
    } catch {
      setError('Failed to load satisfaction data. Please try again.');
    } finally {
      setLoading(false);
    }
  }, [fromDate, toDate]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  const npsColour = useMemo(() => {
    if (!metrics) return 'text.primary';
    return metrics.npsScore >= 50
      ? 'success.main'
      : metrics.npsScore >= 0
      ? 'warning.main'
      : 'error.main';
  }, [metrics]);

  const maxDistCount = useMemo(() => {
    if (!metrics) return 1;
    return Math.max(1, ...Object.values(metrics.scoreDistribution));
  }, [metrics]);

  return (
    <Container maxWidth="xl" sx={{ py: 3 }}>
      {/* Header */}
      <Stack direction="row" justifyContent="space-between" alignItems="center" mb={3}>
        <Box>
          <Typography variant="h4" fontWeight={700}>
            Customer Satisfaction
          </Typography>
          <Typography variant="body2" color="text.secondary">
            CSAT · NPS · CES survey analytics
          </Typography>
        </Box>
        {/* Date range filters */}
        <Stack direction="row" spacing={2}>
          <TextField
            size="small"
            type="date"
            label="From"
            value={fromDate}
            onChange={(e) => setFromDate(e.target.value)}
            InputLabelProps={{ shrink: true }}
          />
          <TextField
            size="small"
            type="date"
            label="To"
            value={toDate}
            onChange={(e) => setToDate(e.target.value)}
            InputLabelProps={{ shrink: true }}
          />
        </Stack>
      </Stack>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {loading ? (
        <Box display="flex" justifyContent="center" py={8}>
          <CircularProgress />
        </Box>
      ) : (
        <>
          {/* KPI Cards */}
          <Grid container spacing={3} mb={3}>
            <Grid item xs={12} sm={6} md={3}>
              <KpiCard
                icon={<TrendingUpIcon />}
                label="NPS Score"
                value={metrics?.npsScore?.toFixed(0) ?? '—'}
                colour={npsColour}
                sublabel="Net Promoter Score"
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <KpiCard
                icon={<StarIcon />}
                label="CSAT Score"
                value={metrics ? `${metrics.averageCSATScore.toFixed(0)}%` : '—'}
                colour={
                  (metrics?.averageCSATScore ?? 0) >= 75 ? 'success.main' : 'warning.main'
                }
                sublabel="Satisfaction Rate"
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <KpiCard
                icon={<PeopleIcon />}
                label="Total Surveys"
                value={metrics?.totalSurveys?.toString() ?? '0'}
                sublabel={`${metrics?.totalResponses ?? 0} responses`}
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <KpiCard
                icon={<SentimentSatisfiedAltIcon />}
                label="Response Rate"
                value={metrics ? `${metrics.responseRate.toFixed(0)}%` : '—'}
                colour={
                  (metrics?.responseRate ?? 0) >= 50 ? 'success.main' : 'warning.main'
                }
                sublabel="Survey completion"
              />
            </Grid>
          </Grid>

          <Grid container spacing={3}>
            {/* Score distribution */}
            <Grid item xs={12} md={5}>
              <Card variant="outlined">
                <CardContent>
                  <Typography variant="subtitle1" fontWeight={600} mb={2}>
                    Score Distribution
                  </Typography>
                  {metrics && Object.keys(metrics.scoreDistribution).length > 0 ? (
                    <Stack spacing={1}>
                      {Object.entries(metrics.scoreDistribution)
                        .sort(([a], [b]) => Number(b) - Number(a))
                        .map(([score, count]) => (
                          <Stack key={score} direction="row" spacing={1} alignItems="center">
                            <Typography variant="body2" sx={{ minWidth: 32 }}>
                              {score}
                            </Typography>
                            <Box flex={1}>
                              <LinearProgress
                                variant="determinate"
                                value={(count / maxDistCount) * 100}
                                sx={{ height: 8, borderRadius: 4 }}
                              />
                            </Box>
                            <Typography variant="caption" sx={{ minWidth: 32 }}>
                              {count}
                            </Typography>
                          </Stack>
                        ))}
                    </Stack>
                  ) : (
                    <Typography variant="body2" color="text.secondary">
                      No data yet
                    </Typography>
                  )}
                </CardContent>
              </Card>
            </Grid>

            {/* Monthly trend */}
            <Grid item xs={12} md={7}>
              <Card variant="outlined">
                <CardContent>
                  <Typography variant="subtitle1" fontWeight={600} mb={2}>
                    Monthly Trend
                  </Typography>
                  {metrics && metrics.byMonth.length > 0 ? (
                    <TableContainer component={Paper} variant="outlined">
                      <Table size="small">
                        <TableHead>
                          <TableRow>
                            <TableCell>Month</TableCell>
                            <TableCell align="right">Avg Score</TableCell>
                            <TableCell align="right">Responses</TableCell>
                          </TableRow>
                        </TableHead>
                        <TableBody>
                          {metrics.byMonth.map((m) => (
                            <TableRow key={m.month} hover>
                              <TableCell>{m.month}</TableCell>
                              <TableCell align="right">{m.averageScore.toFixed(1)}</TableCell>
                              <TableCell align="right">{m.count}</TableCell>
                            </TableRow>
                          ))}
                        </TableBody>
                      </Table>
                    </TableContainer>
                  ) : (
                    <Typography variant="body2" color="text.secondary">
                      No trend data yet
                    </Typography>
                  )}
                </CardContent>
              </Card>
            </Grid>

            {/* Recent surveys table */}
            <Grid item xs={12}>
              <Card variant="outlined">
                <CardContent>
                  <Typography variant="subtitle1" fontWeight={600} mb={2}>
                    Recent Surveys
                  </Typography>
                  <TableContainer>
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>ID</TableCell>
                          <TableCell>Type</TableCell>
                          <TableCell>Entity</TableCell>
                          <TableCell>Contact</TableCell>
                          <TableCell>Status</TableCell>
                          <TableCell align="right">Score</TableCell>
                          <TableCell>Created</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {surveys.length === 0 ? (
                          <TableRow>
                            <TableCell colSpan={7} align="center">
                              <Typography variant="body2" color="text.secondary" py={2}>
                                No surveys yet
                              </Typography>
                            </TableCell>
                          </TableRow>
                        ) : (
                          surveys.map((s) => (
                            <TableRow key={s.id} hover>
                              <TableCell>{s.id}</TableCell>
                              <TableCell>
                                <Chip
                                  label={surveyTypeLabel[s.type]}
                                  size="small"
                                  variant="outlined"
                                />
                              </TableCell>
                              <TableCell>
                                {s.entityType}/{s.entityId}
                              </TableCell>
                              <TableCell>{s.contactName ?? '—'}</TableCell>
                              <TableCell>
                                <Chip
                                  label={statusLabel[s.status]}
                                  size="small"
                                  color={statusColour[s.status]}
                                />
                              </TableCell>
                              <TableCell align="right">
                                {s.score !== undefined && s.score !== null ? s.score : '—'}
                              </TableCell>
                              <TableCell>
                                {new Date(s.createdAt).toLocaleDateString()}
                              </TableCell>
                            </TableRow>
                          ))
                        )}
                      </TableBody>
                    </Table>
                  </TableContainer>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </>
      )}
    </Container>
  );
};

// ── KPI Card sub-component ────────────────────────────────────────────────────

interface KpiCardProps {
  icon: React.ReactNode;
  label: string;
  value: string;
  colour?: string;
  sublabel?: string;
}

const KpiCard: React.FC<KpiCardProps> = ({ icon, label, value, colour = 'text.primary', sublabel }) => (
  <Card variant="outlined" sx={{ height: '100%' }}>
    <CardContent>
      <Stack direction="row" spacing={1} alignItems="center" color="text.secondary" mb={1}>
        {icon}
        <Typography variant="caption" textTransform="uppercase" letterSpacing={0.5}>
          {label}
        </Typography>
      </Stack>
      <Typography variant="h4" fontWeight={700} sx={{ color: colour }}>
        {value}
      </Typography>
      {sublabel && (
        <Typography variant="caption" color="text.secondary">
          {sublabel}
        </Typography>
      )}
    </CardContent>
  </Card>
);

export default SatisfactionDashboardPage;
