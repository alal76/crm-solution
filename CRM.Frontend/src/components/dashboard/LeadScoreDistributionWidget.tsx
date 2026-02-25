import React, { useEffect, useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  CardHeader,
  CircularProgress,
  Divider,
  Typography,
} from '@mui/material';
import LeaderboardIcon from '@mui/icons-material/Leaderboard';
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import apiClient from '../../services/apiClient';

// ─── Types ────────────────────────────────────────────────────────────────────

interface Lead {
  id: number;
  score?: number;
  leadScore?: number; // alternate field name used in some DTO versions
}

interface PagedResponse<T> {
  items: T[];
  totalCount: number;
}

interface ScoreBand {
  label: string;
  count: number;
  color: string;
}

// ─── Helpers ──────────────────────────────────────────────────────────────────

const BANDS: { label: string; min: number; max: number; color: string }[] = [
  { label: '0–25', min: 0, max: 25, color: '#f44336' },
  { label: '26–50', min: 26, max: 50, color: '#ff9800' },
  { label: '51–75', min: 51, max: 75, color: '#2196f3' },
  { label: '76–100', min: 76, max: 100, color: '#4caf50' },
];

function buildBands(leads: Lead[]): ScoreBand[] {
  return BANDS.map(({ label, min, max, color }) => ({
    label,
    count: leads.filter((l) => {
      const s = l.score ?? l.leadScore ?? 0;
      return s >= min && s <= max;
    }).length,
    color,
  }));
}

// ─── Component ────────────────────────────────────────────────────────────────

/**
 * Dashboard widget displaying the distribution of lead scores across four bands:
 * 0–25 (cold), 26–50 (warm), 51–75 (hot), 76–100 (very hot).
 *
 * Fetches up to 500 leads and computes the distribution in-memory because the
 * backend does not expose a dedicated score-distribution endpoint.
 *
 * TODO-GAP-MARKETING-001
 */
const LeadScoreDistributionWidget: React.FC = () => {
  const [bands, setBands] = useState<ScoreBand[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [totalLeads, setTotalLeads] = useState(0);

  useEffect(() => {
    const fetch = async () => {
      setLoading(true);
      try {
        const res = await apiClient.get<PagedResponse<Lead> | Lead[]>('/leads', {
          params: { pageSize: 500, page: 1 },
        });
        const payload = res.data;
        const leads = Array.isArray(payload) ? payload : (payload.items ?? []);
        setTotalLeads(Array.isArray(payload) ? leads.length : (payload.totalCount ?? leads.length));
        setBands(buildBands(leads));
      } catch {
        setError('Failed to load lead score data.');
      } finally {
        setLoading(false);
      }
    };

    fetch();
  }, []);

  return (
    <Card sx={{ height: '100%' }}>
      <CardHeader
        avatar={<LeaderboardIcon color="secondary" />}
        title="Lead Score Distribution"
        subheader={totalLeads > 0 ? `${totalLeads} leads analysed` : 'Score bands'}
        titleTypographyProps={{ variant: 'h6' }}
      />
      <Divider />
      <CardContent sx={{ '&:last-child': { pb: 2 } }}>
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

        {!loading && !error && (
          <ResponsiveContainer width="100%" height={180}>
            <BarChart data={bands} margin={{ top: 8, right: 8, left: -20, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} />
              <XAxis dataKey="label" tick={{ fontSize: 12 }} />
              <YAxis allowDecimals={false} tick={{ fontSize: 12 }} />
              <Tooltip
                formatter={(value: number) => [`${value} leads`, 'Count']}
              />
              <Bar dataKey="count" radius={[4, 4, 0, 0]}>
                {bands.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={entry.color} />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        )}

        {/* Legend */}
        {!loading && !error && (
          <Box sx={{ display: 'flex', justifyContent: 'center', gap: 2, mt: 1, flexWrap: 'wrap' }}>
            {bands.map((b) => (
              <Box key={b.label} sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                <Box
                  sx={{
                    width: 10,
                    height: 10,
                    borderRadius: '50%',
                    bgcolor: b.color,
                    flexShrink: 0,
                  }}
                />
                <Typography variant="caption">{b.label}</Typography>
              </Box>
            ))}
          </Box>
        )}
      </CardContent>
    </Card>
  );
};

export default LeadScoreDistributionWidget;
