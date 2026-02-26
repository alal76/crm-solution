// CRM Solution - Customer Relationship Management System
// FEAT-AISCORING: AI Lead Scoring Real-time Triggers — Score history sparkline chart
import React, { useEffect, useState } from 'react';
import {
  Box,
  CircularProgress,
  Typography,
  Alert,
} from '@mui/material';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from 'recharts';
import { getScoreHistory, LeadScoreHistoryItem } from '../../services/leadScoreService';

interface Props {
  leadId: number;
}

const formatDate = (isoStr: string): string => {
  const d = new Date(isoStr);
  return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
};

const getTrendColor = (trend: 'improving' | 'declining' | 'stable'): string => {
  if (trend === 'improving') return '#2e7d32';
  if (trend === 'declining') return '#c62828';
  return '#757575';
};

const calcTrend = (items: LeadScoreHistoryItem[]): 'improving' | 'declining' | 'stable' => {
  if (items.length < 2) return 'stable';
  const half = Math.floor(items.length / 2);
  // items are sorted newest-first from the API; reverse so newest is last
  const scores = [...items].reverse().map(i => i.score);
  const newerAvg = scores.slice(half).reduce((a, b) => a + b, 0) / (scores.length - half);
  const olderAvg = scores.slice(0, half).reduce((a, b) => a + b, 0) / half;
  const delta = newerAvg - olderAvg;
  if (delta > 5) return 'improving';
  if (delta < -5) return 'declining';
  return 'stable';
};

const LeadScoreHistoryChart: React.FC<Props> = ({ leadId }) => {
  const [history, setHistory] = useState<LeadScoreHistoryItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let mounted = true;
    setLoading(true);
    setError(null);
    getScoreHistory(leadId, 10)
      .then(data => {
        if (mounted) {
          setHistory(data);
          setLoading(false);
        }
      })
      .catch(err => {
        if (mounted) {
          setError(err?.message ?? 'Failed to load score history');
          setLoading(false);
        }
      });
    return () => { mounted = false; };
  }, [leadId]);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 2 }}>
        <CircularProgress size={24} />
      </Box>
    );
  }

  if (error) {
    return <Alert severity="warning" sx={{ fontSize: '0.75rem' }}>{error}</Alert>;
  }

  if (history.length === 0) {
    return (
      <Typography variant="caption" color="text.secondary" sx={{ p: 1, display: 'block', textAlign: 'center' }}>
        No score history yet
      </Typography>
    );
  }

  const trend = calcTrend(history);
  const lineColor = getTrendColor(trend);

  // Reverse to chronological order for chart (oldest → newest on X axis)
  const chartData = [...history]
    .reverse()
    .map(item => ({
      date: formatDate(item.scoredAt),
      score: item.score,
      reason: item.reason,
    }));

  return (
    <Box>
      <Typography
        variant="caption"
        sx={{
          display: 'block',
          mb: 0.5,
          fontWeight: 600,
          color: lineColor,
          textAlign: 'center',
        }}
      >
        {trend === 'improving' ? '⬆ Improving' : trend === 'declining' ? '⬇ Declining' : '→ Stable'}
      </Typography>
      <ResponsiveContainer width="100%" height={110}>
        <LineChart data={chartData} margin={{ top: 4, right: 8, left: -20, bottom: 0 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="#e0e0e0" />
          <XAxis dataKey="date" tick={{ fontSize: 10 }} />
          <YAxis domain={[0, 100]} tick={{ fontSize: 10 }} />
          <Tooltip
            formatter={(value: number) => [`${value}`, 'Score']}
            labelStyle={{ fontSize: 11 }}
            contentStyle={{ fontSize: 11 }}
          />
          <Line
            type="monotone"
            dataKey="score"
            stroke={lineColor}
            strokeWidth={2}
            dot={{ r: 3, fill: lineColor }}
            activeDot={{ r: 5 }}
            isAnimationActive={false}
          />
        </LineChart>
      </ResponsiveContainer>
    </Box>
  );
};

export default LeadScoreHistoryChart;
