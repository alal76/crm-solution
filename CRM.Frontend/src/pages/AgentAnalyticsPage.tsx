import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Grid,
  Alert,
  CircularProgress,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  ToggleButton,
  ToggleButtonGroup,
  LinearProgress,
  Rating,
} from '@mui/material';
import {
  AnalyticsOutlined,
  ChatOutlined,
  BoltOutlined,
  StarOutlined,
  GroupOutlined,
  TrendingUpOutlined,
  TrendingDownOutlined,
} from '@mui/icons-material';
import agentAnalyticsService from '../services/agentAnalyticsService';
import { AgentUsageMetric, AgentAccuracyMetric, AgentCostMetric } from '../types/agents';

interface SummaryCard {
  title: string;
  value: string | number;
  icon: React.ReactNode;
  color: string;
}

function computeTrend(dailyCosts: { date: string; actionCount: number }[] | undefined): {
  direction: 'up' | 'down' | 'flat';
  percentage: number;
} {
  if (!dailyCosts || dailyCosts.length < 2) {
    return { direction: 'flat', percentage: 0 };
  }

  const sorted = [...dailyCosts].sort(
    (a, b) => new Date(a.date).getTime() - new Date(b.date).getTime()
  );

  const recent7 = sorted.slice(-7);
  const previous7 = sorted.slice(-14, -7);

  if (previous7.length === 0) {
    return { direction: 'flat', percentage: 0 };
  }

  const recentAvg = recent7.reduce((sum, d) => sum + d.actionCount, 0) / recent7.length;
  const previousAvg = previous7.reduce((sum, d) => sum + d.actionCount, 0) / previous7.length;

  if (previousAvg === 0) {
    return recentAvg > 0
      ? { direction: 'up', percentage: 100 }
      : { direction: 'flat', percentage: 0 };
  }

  const change = ((recentAvg - previousAvg) / previousAvg) * 100;
  return {
    direction: change > 1 ? 'up' : change < -1 ? 'down' : 'flat',
    percentage: Math.abs(Math.round(change)),
  };
}

const AgentAnalyticsPage: React.FC = () => {
  const [usage, setUsage] = useState<AgentUsageMetric[]>([]);
  const [accuracy, setAccuracy] = useState<AgentAccuracyMetric[]>([]);
  const [cost, setCost] = useState<AgentCostMetric[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [days, setDays] = useState(30);

  const loadData = useCallback(async (rangeDays: number) => {
    try {
      setLoading(true);
      setError(null);
      const [usageRes, accuracyRes, costRes] = await Promise.all([
        agentAnalyticsService.getUsage(rangeDays),
        agentAnalyticsService.getAccuracy(rangeDays),
        agentAnalyticsService.getCost(rangeDays),
      ]);
      setUsage(usageRes.data);
      setAccuracy(accuracyRes.data);
      setCost(costRes.data);
    } catch (err: any) {
      setError(err?.message || 'Failed to load analytics data');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData(days);
  }, [days, loadData]);

  const handleDaysChange = (_: React.MouseEvent<HTMLElement>, newDays: number | null) => {
    if (newDays !== null) {
      setDays(newDays);
    }
  };

  // Compute summary values
  const totalConversations = usage.reduce((sum, u) => sum + (u.totalConversations || 0), 0);
  const totalActions = usage.reduce((sum, u) => sum + (u.totalActions || 0), 0);

  const weightedRatingSum = accuracy.reduce(
    (sum, a) => sum + (a.averageRating || 0) * (a.ratedConversations || 0),
    0
  );
  const totalRated = accuracy.reduce((sum, a) => sum + (a.ratedConversations || 0), 0);
  const averageRating = totalRated > 0 ? weightedRatingSum / totalRated : 0;

  const activeAgents = usage.filter((u) => (u.totalConversations || 0) > 0).length;

  const maxConversations = Math.max(...usage.map((u) => u.totalConversations || 0), 1);

  const summaryCards: SummaryCard[] = [
    {
      title: 'Total Conversations',
      value: totalConversations.toLocaleString(),
      icon: <ChatOutlined sx={{ fontSize: 28 }} />,
      color: '#6750A4',
    },
    {
      title: 'Total Actions',
      value: totalActions.toLocaleString(),
      icon: <BoltOutlined sx={{ fontSize: 28 }} />,
      color: '#2196F3',
    },
    {
      title: 'Average Rating',
      value: averageRating.toFixed(1),
      icon: <StarOutlined sx={{ fontSize: 28 }} />,
      color: '#FF9800',
    },
    {
      title: 'Active Agents',
      value: activeAgents,
      icon: <GroupOutlined sx={{ fontSize: 28 }} />,
      color: '#4CAF50',
    },
  ];

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      {/* Page Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <AnalyticsOutlined sx={{ fontSize: 32, color: '#6750A4' }} />
          <Box>
            <Typography variant="h5" fontWeight={700}>
              Agent Analytics
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Monitor AI agent performance and usage
            </Typography>
          </Box>
        </Box>
        <ToggleButtonGroup
          value={days}
          exclusive
          onChange={handleDaysChange}
          size="small"
        >
          <ToggleButton value={7}>7 days</ToggleButton>
          <ToggleButton value={30}>30 days</ToggleButton>
          <ToggleButton value={90}>90 days</ToggleButton>
        </ToggleButtonGroup>
      </Box>

      {/* Error */}
      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {/* Summary Cards */}
      <Grid container spacing={2} sx={{ mb: 4 }}>
        {summaryCards.map((card) => (
          <Grid item xs={12} sm={6} md={3} key={card.title}>
            <Card variant="outlined">
              <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <Box
                  sx={{
                    width: 48,
                    height: 48,
                    borderRadius: 2,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    backgroundColor: `${card.color}15`,
                    color: card.color,
                  }}
                >
                  {card.icon}
                </Box>
                <Box>
                  <Typography variant="h5" fontWeight={700}>
                    {card.value}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {card.title}
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      {/* Section 1 — Usage */}
      <Paper variant="outlined" sx={{ mb: 3 }}>
        <Box sx={{ p: 2, borderBottom: '1px solid #e0e0e0' }}>
          <Typography variant="h6" fontWeight={600}>
            Usage
          </Typography>
        </Box>
        <TableContainer>
          <Table>
            <TableHead
              sx={{
                backgroundColor: '#F5EFF7',
                '& .MuiTableCell-head': { color: '#6750A4', fontWeight: 600 },
              }}
            >
              <TableRow>
                <TableCell>Agent</TableCell>
                <TableCell align="right">Conversations</TableCell>
                <TableCell sx={{ width: '25%' }}>Distribution</TableCell>
                <TableCell align="right">Actions</TableCell>
                <TableCell align="right">Avg Messages/Conv</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {usage.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={5} align="center" sx={{ py: 3 }}>
                    <Typography color="text.secondary">No usage data available.</Typography>
                  </TableCell>
                </TableRow>
              ) : (
                usage.map((u) => {
                  const pct =
                    maxConversations > 0
                      ? ((u.totalConversations || 0) / maxConversations) * 100
                      : 0;
                  return (
                    <TableRow key={u.agentId} hover>
                      <TableCell>
                        <Typography variant="body2" fontWeight={500}>
                          {u.agentName || `Agent #${u.agentId}`}
                        </Typography>
                      </TableCell>
                      <TableCell align="right">
                        {(u.totalConversations || 0).toLocaleString()}
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <LinearProgress
                            variant="determinate"
                            value={pct}
                            sx={{
                              flex: 1,
                              height: 8,
                              borderRadius: 4,
                              backgroundColor: '#e0e0e0',
                              '& .MuiLinearProgress-bar': { backgroundColor: '#6750A4' },
                            }}
                          />
                          <Typography variant="caption" sx={{ minWidth: 36, textAlign: 'right' }}>
                            {Math.round(pct)}%
                          </Typography>
                        </Box>
                      </TableCell>
                      <TableCell align="right">
                        {(u.totalActions || 0).toLocaleString()}
                      </TableCell>
                      <TableCell align="right">
                        {u.totalConversations
                          ? (u.averageMessagesPerConversation ?? 0).toFixed(1)
                          : '—'}
                      </TableCell>
                    </TableRow>
                  );
                })
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>

      {/* Section 2 — Accuracy */}
      <Paper variant="outlined" sx={{ mb: 3 }}>
        <Box sx={{ p: 2, borderBottom: '1px solid #e0e0e0' }}>
          <Typography variant="h6" fontWeight={600}>
            Accuracy
          </Typography>
        </Box>
        <TableContainer>
          <Table>
            <TableHead
              sx={{
                backgroundColor: '#F5EFF7',
                '& .MuiTableCell-head': { color: '#6750A4', fontWeight: 600 },
              }}
            >
              <TableRow>
                <TableCell>Agent</TableCell>
                <TableCell>Avg Rating</TableCell>
                <TableCell align="right">Rated</TableCell>
                <TableCell align="right">Total</TableCell>
                <TableCell sx={{ width: '25%' }}>Rating %</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {accuracy.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={5} align="center" sx={{ py: 3 }}>
                    <Typography color="text.secondary">No accuracy data available.</Typography>
                  </TableCell>
                </TableRow>
              ) : (
                accuracy.map((a) => {
                  const ratingPct =
                    a.totalConversations && a.totalConversations > 0
                      ? ((a.ratedConversations || 0) / a.totalConversations) * 100
                      : 0;
                  return (
                    <TableRow key={a.agentId} hover>
                      <TableCell>
                        <Typography variant="body2" fontWeight={500}>
                          {a.agentName || `Agent #${a.agentId}`}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Rating
                            value={a.averageRating || 0}
                            precision={0.1}
                            readOnly
                            size="small"
                          />
                          <Typography variant="body2" color="text.secondary">
                            {(a.averageRating || 0).toFixed(1)}
                          </Typography>
                        </Box>
                      </TableCell>
                      <TableCell align="right">
                        {(a.ratedConversations || 0).toLocaleString()}
                      </TableCell>
                      <TableCell align="right">
                        {(a.totalConversations || 0).toLocaleString()}
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <LinearProgress
                            variant="determinate"
                            value={ratingPct}
                            sx={{
                              flex: 1,
                              height: 8,
                              borderRadius: 4,
                              backgroundColor: '#e0e0e0',
                              '& .MuiLinearProgress-bar': { backgroundColor: '#FF9800' },
                            }}
                          />
                          <Typography variant="caption" sx={{ minWidth: 36, textAlign: 'right' }}>
                            {Math.round(ratingPct)}%
                          </Typography>
                        </Box>
                      </TableCell>
                    </TableRow>
                  );
                })
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>

      {/* Section 3 — Cost / Activity */}
      <Paper variant="outlined">
        <Box sx={{ p: 2, borderBottom: '1px solid #e0e0e0' }}>
          <Typography variant="h6" fontWeight={600}>
            Cost &amp; Activity
          </Typography>
        </Box>
        <TableContainer>
          <Table>
            <TableHead
              sx={{
                backgroundColor: '#F5EFF7',
                '& .MuiTableCell-head': { color: '#6750A4', fontWeight: 600 },
              }}
            >
              <TableRow>
                <TableCell>Agent</TableCell>
                <TableCell align="right">Total Actions</TableCell>
                <TableCell align="right">Daily Avg</TableCell>
                <TableCell>Trend</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {cost.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={4} align="center" sx={{ py: 3 }}>
                    <Typography color="text.secondary">No cost data available.</Typography>
                  </TableCell>
                </TableRow>
              ) : (
                cost.map((c) => {
                  const trend = computeTrend(c.dailyCosts);
                  const dailyAvg =
                    c.dailyCosts && c.dailyCosts.length > 0
                      ? (
                          c.dailyCosts.reduce((sum, d) => sum + d.actionCount, 0) / c.dailyCosts.length
                        ).toFixed(1)
                      : '—';

                  return (
                    <TableRow key={c.agentId} hover>
                      <TableCell>
                        <Typography variant="body2" fontWeight={500}>
                          {c.agentName || `Agent #${c.agentId}`}
                        </Typography>
                      </TableCell>
                      <TableCell align="right">
                        {(c.totalActions || 0).toLocaleString()}
                      </TableCell>
                      <TableCell align="right">{dailyAvg}</TableCell>
                      <TableCell>
                        {trend.direction === 'flat' ? (
                          <Typography variant="body2" color="text.secondary">
                            — No change
                          </Typography>
                        ) : (
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                            {trend.direction === 'up' ? (
                              <TrendingUpOutlined sx={{ fontSize: 18, color: '#F44336' }} />
                            ) : (
                              <TrendingDownOutlined sx={{ fontSize: 18, color: '#4CAF50' }} />
                            )}
                            <Typography
                              variant="body2"
                              sx={{
                                color: trend.direction === 'up' ? '#F44336' : '#4CAF50',
                                fontWeight: 500,
                              }}
                            >
                              {trend.direction === 'up' ? '↑' : '↓'} {trend.percentage}%
                            </Typography>
                          </Box>
                        )}
                      </TableCell>
                    </TableRow>
                  );
                })
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>
    </Box>
  );
};

export default AgentAnalyticsPage;
