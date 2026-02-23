/**
 * UsageChart - Displays subscription usage data as simple CSS-based bar chart
 * Groups records by metric name and renders percentage-width bars
 */
import React, { useMemo } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Skeleton,
  Stack,
  Tooltip,
} from '@mui/material';
import type { UsageRecordDto } from '../../services/billingService';

interface UsageChartProps {
  usageData: UsageRecordDto[];
  loading?: boolean;
}

interface MetricGroup {
  metricName: string;
  totalQuantity: number;
  totalCost: number;
  records: UsageRecordDto[];
}

const CHART_COLORS = ['#1976d2', '#2e7d32', '#ed6c02', '#9c27b0', '#d32f2f', '#0288d1'];

const UsageChart: React.FC<UsageChartProps> = ({ usageData, loading = false }) => {
  const grouped = useMemo<MetricGroup[]>(() => {
    const map = new Map<string, MetricGroup>();
    for (const record of usageData) {
      const existing = map.get(record.metricName);
      if (existing) {
        existing.totalQuantity += record.quantity;
        existing.totalCost += record.total;
        existing.records.push(record);
      } else {
        map.set(record.metricName, {
          metricName: record.metricName,
          totalQuantity: record.quantity,
          totalCost: record.total,
          records: [record],
        });
      }
    }
    return Array.from(map.values());
  }, [usageData]);

  const maxQuantity = useMemo(
    () => Math.max(...grouped.map((g) => g.totalQuantity), 1),
    [grouped]
  );

  if (loading) {
    return (
      <Card variant="outlined">
        <CardContent>
          <Skeleton width="40%" height={28} sx={{ mb: 2 }} />
          {[1, 2, 3].map((i) => (
            <Box key={i} sx={{ mb: 2 }}>
              <Skeleton width="30%" height={18} />
              <Skeleton width={`${60 - i * 10}%`} height={32} />
            </Box>
          ))}
        </CardContent>
      </Card>
    );
  }

  if (grouped.length === 0) {
    return (
      <Card variant="outlined">
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Usage Overview
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ py: 4, textAlign: 'center' }}>
            No usage data available
          </Typography>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card variant="outlined">
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Usage Overview
        </Typography>
        <Stack spacing={2} sx={{ mt: 2 }}>
          {grouped.map((metric, idx) => {
            const pct = (metric.totalQuantity / maxQuantity) * 100;
            const color = CHART_COLORS[idx % CHART_COLORS.length];
            return (
              <Box key={metric.metricName}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                  <Typography variant="body2" fontWeight={600}>
                    {metric.metricName}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {metric.totalQuantity.toLocaleString()} units &middot; ${metric.totalCost.toFixed(2)}
                  </Typography>
                </Box>
                <Tooltip title={`${pct.toFixed(1)}% of max usage`} arrow>
                  <Box
                    sx={{
                      width: '100%',
                      height: 28,
                      bgcolor: 'grey.100',
                      borderRadius: 1,
                      overflow: 'hidden',
                    }}
                  >
                    <Box
                      sx={{
                        width: `${Math.max(pct, 2)}%`,
                        height: '100%',
                        bgcolor: color,
                        borderRadius: 1,
                        transition: 'width 0.6s ease',
                        display: 'flex',
                        alignItems: 'center',
                        pl: 1,
                      }}
                    >
                      {pct > 15 && (
                        <Typography variant="caption" sx={{ color: '#fff', fontWeight: 600 }}>
                          {metric.totalQuantity.toLocaleString()}
                        </Typography>
                      )}
                    </Box>
                  </Box>
                </Tooltip>
              </Box>
            );
          })}
        </Stack>
      </CardContent>
    </Card>
  );
};

export default UsageChart;
