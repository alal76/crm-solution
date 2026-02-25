import React, { useEffect, useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Chip,
  CircularProgress,
  Divider,
  LinearProgress,
  Typography,
} from '@mui/material';
import CampaignIcon from '@mui/icons-material/Campaign';
import apiClient from '../../services/apiClient';

// ─── Types ────────────────────────────────────────────────────────────────────

interface Campaign {
  id: number;
  name: string;
  status: string;
  type?: string;
  startDate?: string;
  endDate?: string;
  budget?: number;
  actualCost?: number;
  expectedRevenue?: number;
  actualRevenue?: number;
  targetCount?: number;
  sentCount?: number;
  openRate?: number;
  clickRate?: number;
  conversionRate?: number;
}

interface PagedResponse<T> {
  items: T[];
  totalCount: number;
}

// ─── Component ────────────────────────────────────────────────────────────────

/**
 * Dashboard widget that shows the top 5 active marketing campaigns with
 * budget utilisation and key performance metrics.
 *
 * Calls: GET /api/campaigns?status=Active&pageSize=5
 * TODO-GAP-MARKETING-001
 */
const CampaignMetricsWidget: React.FC = () => {
  const [campaigns, setCampaigns] = useState<Campaign[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchCampaigns = async () => {
      setLoading(true);
      try {
        const res = await apiClient.get<PagedResponse<Campaign>>('/campaigns', {
          params: { status: 'Active', pageSize: 5 },
        });
        const data = res.data;
        // Handle both paged and plain-array response shapes defensively
        setCampaigns(Array.isArray(data) ? data : (data.items ?? []));
      } catch {
        setError('Failed to load campaigns.');
      } finally {
        setLoading(false);
      }
    };

    fetchCampaigns();
  }, []);

  const budgetPct = (c: Campaign): number => {
    if (!c.budget || c.budget === 0 || c.actualCost === undefined) return 0;
    return Math.min(100, Math.round((c.actualCost / c.budget) * 100));
  };

  const statusColor = (status: string): 'success' | 'primary' | 'warning' | 'default' => {
    const s = status.toLowerCase();
    if (s === 'active') return 'success';
    if (s === 'planned') return 'primary';
    if (s === 'paused') return 'warning';
    return 'default';
  };

  return (
    <Card sx={{ height: '100%' }} aria-label="Active Campaigns widget">
      <CardHeader
        avatar={<CampaignIcon color="primary" aria-hidden="true" />}
        title="Active Campaigns"
        subheader="Top 5 by activity"
        titleTypographyProps={{ variant: 'h6' }}
      />
      <Divider />
      <CardContent sx={{ pt: 1, '&:last-child': { pb: 2 } }}>
        {loading && (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress size={28} />
          </Box>
        )}

        {!loading && error && (
          <Typography color="error" variant="body2">
            {error}
          </Typography>
        )}

        {!loading && !error && campaigns.length === 0 && (
          <Typography variant="body2" color="text.secondary">
            No active campaigns found.
          </Typography>
        )}

        {!loading &&
          !error &&
          campaigns.map((c) => (
            <Box key={c.id} sx={{ mb: 2 }} role="listitem" aria-label={`Campaign: ${c.name}, status: ${c.status}`}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 0.5 }}>
                <Typography variant="body2" fontWeight={500} noWrap sx={{ maxWidth: '60%' }}>
                  {c.name}
                </Typography>
                <Chip
                  label={c.status}
                  size="small"
                  color={statusColor(c.status)}
                  sx={{ height: 20, fontSize: '0.7rem' }}
                  aria-label={`Status: ${c.status}`}
                />
              </Box>

              {/* Budget utilisation bar */}
              {c.budget != null && c.budget > 0 && (
                <Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="caption" color="text.secondary">
                      Budget: ${c.budget.toLocaleString()}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {budgetPct(c)}% used
                    </Typography>
                  </Box>
                  <LinearProgress
                    variant="determinate"
                    value={budgetPct(c)}
                    color={budgetPct(c) >= 90 ? 'error' : budgetPct(c) >= 70 ? 'warning' : 'primary'}
                    sx={{ height: 5, borderRadius: 1 }}
                    aria-label={`${c.name} budget utilisation: ${budgetPct(c)}%`}
                    aria-valuenow={budgetPct(c)}
                    aria-valuemin={0}
                    aria-valuemax={100}
                  />
                </Box>
              )}

              {/* KPIs */}
              <Box sx={{ display: 'flex', gap: 2, mt: 0.5 }}>
                {c.openRate != null && (
                  <Typography variant="caption" color="text.secondary">
                    Open {(c.openRate * 100).toFixed(1)}%
                  </Typography>
                )}
                {c.clickRate != null && (
                  <Typography variant="caption" color="text.secondary">
                    Click {(c.clickRate * 100).toFixed(1)}%
                  </Typography>
                )}
                {c.conversionRate != null && (
                  <Typography variant="caption" color="success.main">
                    Conv {(c.conversionRate * 100).toFixed(1)}%
                  </Typography>
                )}
              </Box>
            </Box>
          ))}
      </CardContent>
    </Card>
  );
};

export default CampaignMetricsWidget;
