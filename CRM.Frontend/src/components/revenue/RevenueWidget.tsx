// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
import React, { useEffect, useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  CircularProgress,
  Skeleton,
} from '@mui/material';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';
import TrendingFlatIcon from '@mui/icons-material/TrendingFlat';
import { revenueAnalyticsService, RevenueMetricsDto } from '../../services/revenueAnalyticsService';

const formatCurrency = (value: number): string => {
  if (value >= 1_000_000) return `$${(value / 1_000_000).toFixed(2)}M`;
  if (value >= 1_000) return `$${(value / 1_000).toFixed(1)}K`;
  return `$${value.toFixed(2)}`;
};

const RevenueWidget: React.FC = () => {
  const [metrics, setMetrics] = useState<RevenueMetricsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let mounted = true;
    setLoading(true);
    revenueAnalyticsService
      .getMetrics()
      .then((data) => {
        if (mounted) setMetrics(data);
      })
      .catch((err) => {
        if (mounted) setError(err?.message ?? 'Failed to load revenue data');
      })
      .finally(() => {
        if (mounted) setLoading(false);
      });
    return () => {
      mounted = false;
    };
  }, []);

  if (loading) {
    return (
      <Card elevation={2} sx={{ width: '100%', minWidth: 200 }}>
        <CardContent>
          <Skeleton variant="text" width="60%" height={28} sx={{ mb: 1 }} />
          <Skeleton variant="text" width="80%" height={44} />
          <Skeleton variant="text" width="50%" height={24} />
        </CardContent>
      </Card>
    );
  }

  if (error || !metrics) {
    return (
      <Card elevation={2} sx={{ width: '100%', minWidth: 200 }}>
        <CardContent>
          <Typography variant="subtitle2" color="text.secondary">
            Revenue
          </Typography>
          <Typography variant="body2" color="error">
            {error ?? 'No data available'}
          </Typography>
        </CardContent>
      </Card>
    );
  }

  const growth = metrics.moMGrowthRate;
  const GrowthIcon =
    growth > 0 ? TrendingUpIcon : growth < 0 ? TrendingDownIcon : TrendingFlatIcon;
  const growthColor = growth > 0 ? 'success.main' : growth < 0 ? 'error.main' : 'text.secondary';

  return (
    <Card elevation={2} sx={{ width: '100%', minWidth: 200 }}>
      <CardContent>
        <Typography variant="subtitle2" color="text.secondary" gutterBottom>
          Monthly Recurring Revenue
        </Typography>
        <Typography variant="h4" component="div" fontWeight={700} color="primary.main">
          {formatCurrency(metrics.currentMRR)}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
          ARR: <strong>{formatCurrency(metrics.currentARR)}</strong>
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', mt: 1, gap: 0.5 }}>
          <GrowthIcon fontSize="small" sx={{ color: growthColor }} />
          <Typography variant="body2" sx={{ color: growthColor, fontWeight: 500 }}>
            {growth >= 0 ? '+' : ''}
            {growth.toFixed(2)}% MoM
          </Typography>
        </Box>
        <Typography variant="caption" color="text.secondary">
          {metrics.totalCustomers} active customers
        </Typography>
      </CardContent>
    </Card>
  );
};

export default RevenueWidget;
