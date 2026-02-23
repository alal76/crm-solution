/**
 * BillingStatsCards - Displays key subscription analytics metrics in stat cards
 */
import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  Grid,
  Box,
  Skeleton,
} from '@mui/material';
import {
  TrendingUp as TrendingUpIcon,
  TrendingDown as TrendingDownIcon,
  People as PeopleIcon,
  AttachMoney as MoneyIcon,
  CalendarMonth as CalendarIcon,
  Warning as WarningIcon,
} from '@mui/icons-material';
import type { SubscriptionAnalyticsDto } from '../../services/billingService';

interface BillingStatsCardsProps {
  stats?: SubscriptionAnalyticsDto;
  loading?: boolean;
}

interface StatCardData {
  title: string;
  value: string;
  icon: React.ReactNode;
  trend?: number;
  color: string;
}

const formatCurrency = (value: number): string => {
  if (value >= 1_000_000) return `$${(value / 1_000_000).toFixed(1)}M`;
  if (value >= 1_000) return `$${(value / 1_000).toFixed(1)}K`;
  return `$${value.toFixed(0)}`;
};

const StatCard: React.FC<{ data: StatCardData; loading?: boolean }> = ({ data, loading }) => (
  <Card variant="outlined" sx={{ height: '100%' }}>
    <CardContent>
      {loading ? (
        <>
          <Skeleton width="60%" height={20} />
          <Skeleton width="40%" height={40} sx={{ mt: 1 }} />
          <Skeleton width="30%" height={16} sx={{ mt: 1 }} />
        </>
      ) : (
        <>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
            <Typography variant="body2" color="text.secondary" fontWeight={500}>
              {data.title}
            </Typography>
            <Box sx={{ color: data.color, display: 'flex', alignItems: 'center' }}>
              {data.icon}
            </Box>
          </Box>
          <Typography variant="h5" fontWeight={700}>
            {data.value}
          </Typography>
          {data.trend !== undefined && (
            <Box sx={{ display: 'flex', alignItems: 'center', mt: 1, gap: 0.5 }}>
              {data.trend >= 0 ? (
                <TrendingUpIcon fontSize="small" sx={{ color: 'success.main' }} />
              ) : (
                <TrendingDownIcon fontSize="small" sx={{ color: 'error.main' }} />
              )}
              <Typography
                variant="caption"
                sx={{ color: data.trend >= 0 ? 'success.main' : 'error.main', fontWeight: 600 }}
              >
                {data.trend >= 0 ? '+' : ''}{data.trend.toFixed(1)}%
              </Typography>
            </Box>
          )}
        </>
      )}
    </CardContent>
  </Card>
);

const BillingStatsCards: React.FC<BillingStatsCardsProps> = ({ stats, loading = false }) => {
  const cards: StatCardData[] = [
    {
      title: 'Monthly Recurring Revenue',
      value: stats ? formatCurrency(stats.mrr) : '$0',
      icon: <MoneyIcon />,
      trend: stats?.revenueGrowthRate,
      color: '#2e7d32',
    },
    {
      title: 'Annual Recurring Revenue',
      value: stats ? formatCurrency(stats.arr) : '$0',
      icon: <CalendarIcon />,
      color: '#1565c0',
    },
    {
      title: 'Active Subscriptions',
      value: stats ? stats.activeSubscriptions.toLocaleString() : '0',
      icon: <PeopleIcon />,
      color: '#6a1b9a',
    },
    {
      title: 'Churn Rate',
      value: stats ? `${stats.churnRate.toFixed(1)}%` : '0%',
      icon: <WarningIcon />,
      trend: stats ? -stats.churnRate : undefined,
      color: stats && stats.churnRate > 5 ? '#d32f2f' : '#ed6c02',
    },
  ];

  return (
    <Grid container spacing={2}>
      {cards.map((card) => (
        <Grid item xs={12} sm={6} md={3} key={card.title}>
          <StatCard data={card} loading={loading} />
        </Grid>
      ))}
    </Grid>
  );
};

export default BillingStatsCards;
