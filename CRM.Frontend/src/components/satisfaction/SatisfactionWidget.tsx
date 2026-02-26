// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
import React, { useEffect, useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  CircularProgress,
  Divider,
  Stack,
  Tooltip,
  Typography,
} from '@mui/material';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import StarIcon from '@mui/icons-material/Star';
import satisfactionService from '../../services/satisfactionService';

interface Props {
  /** Optional entity type filter (e.g. "ServiceRequest") */
  entityType?: string;
}

/**
 * Compact widget showing the current NPS and CSAT scores.
 * Designed to be embedded in the main CRM dashboard.
 */
const SatisfactionWidget: React.FC<Props> = ({ entityType }) => {
  const [nps, setNps] = useState<number | null>(null);
  const [csat, setCsat] = useState<number | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;
    const fetch = async () => {
      try {
        setLoading(true);
        const [npsScore, csatScore] = await Promise.all([
          satisfactionService.getNPS(),
          satisfactionService.getCSAT(),
        ]);
        if (!cancelled) {
          setNps(npsScore);
          setCsat(csatScore);
        }
      } catch {
        // silently ignore — widget is non-critical
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    void fetch();
    return () => {
      cancelled = true;
    };
  }, [entityType]);

  const npsColour = (score: number) => {
    if (score >= 50) return 'success.main';
    if (score >= 0) return 'warning.main';
    return 'error.main';
  };

  const csatColour = (score: number) => {
    if (score >= 75) return 'success.main';
    if (score >= 50) return 'warning.main';
    return 'error.main';
  };

  return (
    <Card variant="outlined" sx={{ height: '100%' }}>
      <CardContent>
        <Typography variant="subtitle2" color="text.secondary" gutterBottom>
          Customer Satisfaction
        </Typography>
        {loading ? (
          <Box display="flex" justifyContent="center" py={1}>
            <CircularProgress size={24} />
          </Box>
        ) : (
          <Stack direction="row" spacing={3} divider={<Divider orientation="vertical" flexItem />}>
            {/* NPS */}
            <Tooltip title="Net Promoter Score: (Promoters − Detractors) / Total × 100">
              <Box textAlign="center">
                <Stack direction="row" spacing={0.5} alignItems="center" justifyContent="center">
                  <TrendingUpIcon fontSize="small" sx={{ color: npsColour(nps ?? 0) }} />
                  <Typography
                    variant="h5"
                    fontWeight={700}
                    sx={{ color: npsColour(nps ?? 0) }}
                  >
                    {nps !== null ? nps.toFixed(0) : '—'}
                  </Typography>
                </Stack>
                <Typography variant="caption" color="text.secondary">
                  NPS
                </Typography>
              </Box>
            </Tooltip>
            {/* CSAT */}
            <Tooltip title="Customer Satisfaction Score: satisfied responses / total × 100">
              <Box textAlign="center">
                <Stack direction="row" spacing={0.5} alignItems="center" justifyContent="center">
                  <StarIcon fontSize="small" sx={{ color: csatColour(csat ?? 0) }} />
                  <Typography
                    variant="h5"
                    fontWeight={700}
                    sx={{ color: csatColour(csat ?? 0) }}
                  >
                    {csat !== null ? `${csat.toFixed(0)}%` : '—'}
                  </Typography>
                </Stack>
                <Typography variant="caption" color="text.secondary">
                  CSAT
                </Typography>
              </Box>
            </Tooltip>
          </Stack>
        )}
      </CardContent>
    </Card>
  );
};

export default SatisfactionWidget;
