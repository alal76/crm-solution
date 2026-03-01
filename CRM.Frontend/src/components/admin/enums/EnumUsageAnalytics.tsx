/**
 * ENUM-FE-013: EnumUsageAnalytics.tsx
 * Displays a usage frequency bar for each enum value.
 * Uses locally-tracked mock data until a backend analytics endpoint is available.
 * When ready, replace `getMockUsage` with a real API call.
 */
import React, { useEffect, useState } from 'react';
import {
  Box,
  Chip,
  CircularProgress,
  LinearProgress,
  Paper,
  Tooltip,
  Typography,
} from '@mui/material';
import { Analytics as AnalyticsIcon } from '@mui/icons-material';
import type { EnumValue } from '../../../types/enums';

export interface EnumUsageAnalyticsProps {
  values: EnumValue[];
  /** Optional external usage map: { [valueKey]: count }. If not provided, uses mock. */
  usageMap?: Record<string, number>;
  loading?: boolean;
  title?: string;
}

/** Generates deterministic mock usage from value IDs so the UI is non-trivial. */
function getMockUsage(values: EnumValue[]): Record<string, number> {
  const seed = values.reduce((acc, v) => acc + v.id, 0) || 100;
  return values.reduce<Record<string, number>>((acc, v, i) => {
    // Use a simple deterministic formula so the numbers look different per value
    acc[v.key] = Math.round(((seed * (i + 1) * 37) % 200) + 5);
    return acc;
  }, {});
}

const EnumUsageAnalytics: React.FC<EnumUsageAnalyticsProps> = ({
  values,
  usageMap,
  loading = false,
  title = 'Value Usage Distribution',
}) => {
  const [usage, setUsage] = useState<Record<string, number>>({});

  useEffect(() => {
    if (usageMap) {
      setUsage(usageMap);
      return undefined;
    }
    if (values.length > 0) {
      // Simulate async fetch with small delay
      const t = setTimeout(() => setUsage(getMockUsage(values)), 300);
      return () => clearTimeout(t);
    }
    return undefined;
  }, [values, usageMap]);

  const maxCount = Math.max(...Object.values(usage), 1);
  const total = Object.values(usage).reduce((a, b) => a + b, 0) || 1;

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress size={24} />
      </Box>
    );
  }

  if (!values.length) {
    return (
      <Typography color="text.secondary" variant="body2">
        No values to analyse.
      </Typography>
    );
  }

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
        <AnalyticsIcon color="action" />
        <Typography variant="subtitle2" fontWeight={600}>{title}</Typography>
        <Typography variant="caption" color="text.secondary" sx={{ ml: 'auto' }}>
          Total records: {total.toLocaleString()}
        </Typography>
      </Box>

      {!usageMap && (
        <Typography variant="caption" color="warning.main" sx={{ display: 'block', mb: 1.5 }}>
          Showing mock data — connect to analytics API for real counts.
        </Typography>
      )}

      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
        {[...values]
          .sort((a, b) => (usage[b.key] ?? 0) - (usage[a.key] ?? 0))
          .map(val => {
            const count = usage[val.key] ?? 0;
            const pct = Math.round((count / maxCount) * 100);
            const globalPct = Math.round((count / total) * 100);

            return (
              <Box key={val.id}>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 0.4 }}>
                  <Chip
                    label={val.label}
                    size="small"
                    sx={{
                      mr: 1,
                      bgcolor: val.color ? `${val.color}22` : undefined,
                      borderColor: val.color,
                      border: val.color ? '1px solid' : undefined,
                      minWidth: 80,
                    }}
                  />
                  <Typography variant="caption" color="text.secondary" sx={{ ml: 'auto' }}>
                    {count.toLocaleString()} ({globalPct}%)
                  </Typography>
                </Box>
                <Tooltip title={`${count.toLocaleString()} records (${globalPct}% of total)`}>
                  <LinearProgress
                    variant="determinate"
                    value={pct}
                    sx={{
                      height: 8,
                      borderRadius: 4,
                      bgcolor: 'action.hover',
                      '& .MuiLinearProgress-bar': {
                        bgcolor: val.color || 'primary.main',
                        borderRadius: 4,
                      },
                    }}
                  />
                </Tooltip>
              </Box>
            );
          })}
      </Box>
    </Paper>
  );
};

export default EnumUsageAnalytics;
