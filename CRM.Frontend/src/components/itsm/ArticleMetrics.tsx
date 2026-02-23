// Article Metrics - Dashboard metrics for a single KB article
// Part of Knowledge Base Enhancement - Phase 3

import React from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Skeleton,
  Stack,
  Tooltip,
} from '@mui/material';
import {
  Visibility as ViewIcon,
  PersonOutline as UniqueViewIcon,
  ThumbUp as HelpfulIcon,
  AccessTime as TimeIcon,
  ConfirmationNumber as TicketIcon,
  TrendingUp as TrendIcon,
} from '@mui/icons-material';

export interface ArticleMetricsData {
  totalViews: number;
  uniqueViews: number;
  helpfulVotes: number;
  notHelpfulVotes: number;
  avgTimeOnPage?: number; // seconds
  linkedTickets: number;
}

export interface ArticleMetricsProps {
  metrics?: ArticleMetricsData;
  loading?: boolean;
}

interface MetricCardProps {
  title: string;
  value: string | number;
  subtitle?: string;
  icon: React.ReactNode;
  color: string;
  tooltip?: string;
}

const MetricCard: React.FC<MetricCardProps> = ({ title, value, subtitle, icon, color, tooltip }) => {
  const card = (
    <Paper
      variant="outlined"
      sx={{
        p: 2,
        height: '100%',
        borderLeft: 4,
        borderLeftColor: color,
        transition: 'box-shadow 0.2s',
        '&:hover': { boxShadow: 2 },
      }}
    >
      <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
        <Box>
          <Typography variant="caption" color="text.secondary" gutterBottom>
            {title}
          </Typography>
          <Typography variant="h5" fontWeight="bold" sx={{ color }}>
            {value}
          </Typography>
          {subtitle && (
            <Typography variant="caption" color="text.secondary">
              {subtitle}
            </Typography>
          )}
        </Box>
        <Box sx={{ color, opacity: 0.7 }}>{icon}</Box>
      </Stack>
    </Paper>
  );

  return tooltip ? <Tooltip title={tooltip}>{card}</Tooltip> : card;
};

const formatDuration = (seconds: number): string => {
  if (seconds < 60) return `${seconds}s`;
  const minutes = Math.floor(seconds / 60);
  const secs = seconds % 60;
  return secs > 0 ? `${minutes}m ${secs}s` : `${minutes}m`;
};

const getHelpfulColor = (percentage: number): string => {
  if (percentage >= 75) return '#4caf50'; // green
  if (percentage >= 50) return '#ff9800'; // orange
  return '#f44336'; // red
};

const ArticleMetrics: React.FC<ArticleMetricsProps> = ({ metrics, loading = false }) => {
  if (loading || !metrics) {
    return (
      <Grid container spacing={2}>
        {[1, 2, 3, 4, 5, 6].map((i) => (
          <Grid item xs={12} sm={6} md={4} key={i}>
            <Skeleton variant="rectangular" height={100} sx={{ borderRadius: 1 }} />
          </Grid>
        ))}
      </Grid>
    );
  }

  const totalVotes = metrics.helpfulVotes + metrics.notHelpfulVotes;
  const helpfulPct = totalVotes > 0 ? Math.round((metrics.helpfulVotes / totalVotes) * 100) : 0;
  const helpfulColor = getHelpfulColor(helpfulPct);

  const cards: MetricCardProps[] = [
    {
      title: 'Total Views',
      value: metrics.totalViews.toLocaleString(),
      icon: <ViewIcon />,
      color: '#2196f3',
      tooltip: 'Total number of page views',
    },
    {
      title: 'Unique Views',
      value: metrics.uniqueViews.toLocaleString(),
      subtitle: metrics.totalViews > 0
        ? `${Math.round((metrics.uniqueViews / metrics.totalViews) * 100)}% of total`
        : undefined,
      icon: <UniqueViewIcon />,
      color: '#9c27b0',
      tooltip: 'Number of unique visitors',
    },
    {
      title: 'Helpful Rate',
      value: `${helpfulPct}%`,
      subtitle: `${metrics.helpfulVotes} of ${totalVotes} votes`,
      icon: <HelpfulIcon />,
      color: helpfulColor,
      tooltip: `${metrics.helpfulVotes} helpful / ${metrics.notHelpfulVotes} not helpful`,
    },
    {
      title: 'Avg. Time on Page',
      value: metrics.avgTimeOnPage !== undefined ? formatDuration(metrics.avgTimeOnPage) : 'N/A',
      icon: <TimeIcon />,
      color: '#00bcd4',
      tooltip: 'Average time users spend reading this article',
    },
    {
      title: 'Linked Tickets',
      value: metrics.linkedTickets,
      icon: <TicketIcon />,
      color: '#ff5722',
      tooltip: 'Number of tickets linked to this article',
    },
    {
      title: 'Engagement Score',
      value: totalVotes > 0 ? `${Math.min(100, Math.round((totalVotes / Math.max(1, metrics.totalViews)) * 100))}%` : 'N/A',
      subtitle: `${totalVotes} total votes`,
      icon: <TrendIcon />,
      color: '#607d8b',
      tooltip: 'Percentage of viewers who voted',
    },
  ];

  return (
    <Grid container spacing={2}>
      {cards.map((card) => (
        <Grid item xs={12} sm={6} md={4} key={card.title}>
          <MetricCard {...card} />
        </Grid>
      ))}
    </Grid>
  );
};

export default ArticleMetrics;
