/**
 * SLAStatusBadge (servicedesk) - Compact badge/chip showing SLA compliance status
 * TODO-SD001-005 (P2)
 *
 * Colors: met=green, at_risk=orange, breached=red, paused=gray, not_applicable=gray
 * compact mode: dot + abbreviation
 * full mode:    label + remaining / overdue time
 */

import React, { useMemo } from 'react';
import { Chip, Box, Typography, Tooltip } from '@mui/material';
import {
  CheckCircle as MetIcon,
  Warning as AtRiskIcon,
  Error as BreachedIcon,
  PauseCircleOutline as PausedIcon,
  RemoveCircleOutline as NAIcon,
} from '@mui/icons-material';

// ─── Types ────────────────────────────────────────────────────────────────────

export type SLAStatus = 'met' | 'at_risk' | 'breached' | 'paused' | 'not_applicable';

export interface SLAStatusBadgeProps {
  status: SLAStatus;
  dueAt?: string;
  compact?: boolean;
}

// ─── Config ───────────────────────────────────────────────────────────────────

interface SLAConfig {
  label: string;
  abbrev: string;
  chipColor: 'success' | 'warning' | 'error' | 'default';
  dotColor: string;
  icon: React.ReactElement;
}

const STATUS_CONFIG: Record<SLAStatus, SLAConfig> = {
  met: {
    label: 'SLA Met',
    abbrev: 'MET',
    chipColor: 'success',
    dotColor: '#2e7d32',
    icon: <MetIcon fontSize="small" />,
  },
  at_risk: {
    label: 'At Risk',
    abbrev: 'RISK',
    chipColor: 'warning',
    dotColor: '#ed6c02',
    icon: <AtRiskIcon fontSize="small" />,
  },
  breached: {
    label: 'Breached',
    abbrev: 'BCHD',
    chipColor: 'error',
    dotColor: '#d32f2f',
    icon: <BreachedIcon fontSize="small" />,
  },
  paused: {
    label: 'Paused',
    abbrev: 'PAUS',
    chipColor: 'default',
    dotColor: '#9e9e9e',
    icon: <PausedIcon fontSize="small" />,
  },
  not_applicable: {
    label: 'N/A',
    abbrev: 'N/A',
    chipColor: 'default',
    dotColor: '#bdbdbd',
    icon: <NAIcon fontSize="small" />,
  },
};

// ─── Time helper ─────────────────────────────────────────────────────────────

function formatRelativeTime(dueAt: string, breached: boolean): string {
  const now = Date.now();
  const due = new Date(dueAt).getTime();
  const diffMs = breached ? now - due : due - now;

  if (diffMs <= 0) return breached ? 'less than a minute ago' : 'less than a minute';

  const totalMinutes = Math.floor(diffMs / 60_000);
  const days = Math.floor(totalMinutes / 1440);
  const hours = Math.floor((totalMinutes % 1440) / 60);
  const minutes = totalMinutes % 60;

  const parts: string[] = [];
  if (days > 0) parts.push(`${days}d`);
  if (hours > 0) parts.push(`${hours}h`);
  if (minutes > 0 || parts.length === 0) parts.push(`${minutes}m`);

  const timeStr = parts.join(' ');
  return breached ? `${timeStr} ago` : `${timeStr} remaining`;
}

// ─── Component ────────────────────────────────────────────────────────────────

const SLAStatusBadge: React.FC<SLAStatusBadgeProps> = ({
  status,
  dueAt,
  compact = false,
}) => {
  const config = STATUS_CONFIG[status] ?? STATUS_CONFIG.not_applicable;

  const timeLabel = useMemo(() => {
    if (!dueAt) return null;
    return formatRelativeTime(dueAt, status === 'breached');
  }, [dueAt, status]);

  // ── Compact mode ─────────────────────────────────────────────────────────
  if (compact) {
    return (
      <Tooltip title={`${config.label}${timeLabel ? ` · ${timeLabel}` : ''}`} arrow>
        <Box
          component="span"
          sx={{ display: 'inline-flex', alignItems: 'center', gap: 0.5 }}
        >
          <Box
            component="span"
            sx={{
              width: 8,
              height: 8,
              borderRadius: '50%',
              bgcolor: config.dotColor,
              flexShrink: 0,
            }}
          />
          <Typography
            variant="caption"
            fontWeight="medium"
            sx={{ color: config.dotColor, lineHeight: 1 }}
          >
            {config.abbrev}
          </Typography>
        </Box>
      </Tooltip>
    );
  }

  // ── Full mode ─────────────────────────────────────────────────────────────
  return (
    <Box sx={{ display: 'inline-flex', alignItems: 'center', gap: 0.75 }}>
      <Chip
        icon={config.icon}
        label={config.label}
        color={config.chipColor}
        size="small"
        variant={status === 'breached' ? 'filled' : 'outlined'}
      />
      {timeLabel && (
        <Typography
          variant="caption"
          color={
            status === 'breached'
              ? 'error.main'
              : status === 'at_risk'
              ? 'warning.main'
              : 'text.secondary'
          }
          sx={{ whiteSpace: 'nowrap' }}
        >
          {timeLabel}
        </Typography>
      )}
    </Box>
  );
};

export default SLAStatusBadge;
