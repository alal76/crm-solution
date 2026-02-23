/**
 * CampaignMetricsCard - Displays campaign delivery/engagement metrics
 * Color-coded stats: green >50%, amber 20-50%, red <20%
 */

import React from 'react';
import {
  Box,
  Card,
  CardContent,
  Grid,
  Typography,
  LinearProgress,
  Skeleton,
} from '@mui/material';

// ============================================================================
// Types
// ============================================================================

export interface CampaignMetricsData {
  sent: number;
  delivered: number;
  opened: number;
  clicked: number;
  bounced: number;
  unsubscribed: number;
  conversionRate: number;
}

export interface CampaignMetricsCardProps {
  metrics: CampaignMetricsData;
  loading?: boolean;
}

// ============================================================================
// Helpers
// ============================================================================

function getProgressColor(pct: number): 'success' | 'warning' | 'error' {
  if (pct >= 50) return 'success';
  if (pct >= 20) return 'warning';
  return 'error';
}

function safePercent(numerator: number, denominator: number): number {
  if (denominator <= 0) return 0;
  return Math.round((numerator / denominator) * 10000) / 100; // Two decimal places
}

interface MetricItem {
  label: string;
  value: number;
  suffix?: string;
}

// ============================================================================
// Component
// ============================================================================

const CampaignMetricsCard: React.FC<CampaignMetricsCardProps> = ({ metrics, loading }) => {
  const deliveryRate = safePercent(metrics.delivered, metrics.sent);
  const openRate = safePercent(metrics.opened, metrics.delivered);
  const clickRate = safePercent(metrics.clicked, metrics.opened);
  const conversionRate = metrics.conversionRate;

  const cards: MetricItem[] = [
    { label: 'Delivery Rate', value: deliveryRate, suffix: '%' },
    { label: 'Open Rate', value: openRate, suffix: '%' },
    { label: 'Click Rate', value: clickRate, suffix: '%' },
    { label: 'Conversion Rate', value: conversionRate, suffix: '%' },
  ];

  if (loading) {
    return (
      <Grid container spacing={2}>
        {[0, 1, 2, 3].map((i) => (
          <Grid item xs={12} sm={6} md={3} key={i}>
            <Card variant="outlined">
              <CardContent>
                <Skeleton variant="text" width="60%" />
                <Skeleton variant="text" width="40%" height={40} />
                <Skeleton variant="rectangular" height={8} sx={{ borderRadius: 1, mt: 1 }} />
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
    );
  }

  return (
    <Box>
      <Grid container spacing={2}>
        {cards.map((card) => {
          const color = getProgressColor(card.value);
          return (
            <Grid item xs={12} sm={6} md={3} key={card.label}>
              <Card variant="outlined">
                <CardContent>
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    {card.label}
                  </Typography>
                  <Typography
                    variant="h4"
                    fontWeight={700}
                    color={`${color}.main`}
                    sx={{ mb: 1 }}
                  >
                    {card.value}
                    {card.suffix}
                  </Typography>
                  <LinearProgress
                    variant="determinate"
                    value={Math.min(card.value, 100)}
                    color={color}
                    sx={{ height: 8, borderRadius: 4 }}
                  />
                </CardContent>
              </Card>
            </Grid>
          );
        })}
      </Grid>

      {/* Detailed numbers */}
      <Box
        sx={{
          display: 'flex',
          flexWrap: 'wrap',
          gap: 3,
          mt: 2,
          px: 1,
        }}
      >
        <Typography variant="body2" color="text.secondary">
          Sent: <strong>{metrics.sent.toLocaleString()}</strong>
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Delivered: <strong>{metrics.delivered.toLocaleString()}</strong>
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Opened: <strong>{metrics.opened.toLocaleString()}</strong>
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Clicked: <strong>{metrics.clicked.toLocaleString()}</strong>
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Bounced: <strong>{metrics.bounced.toLocaleString()}</strong>
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Unsubscribed: <strong>{metrics.unsubscribed.toLocaleString()}</strong>
        </Typography>
      </Box>
    </Box>
  );
};

export default CampaignMetricsCard;
