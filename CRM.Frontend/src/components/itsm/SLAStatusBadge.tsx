/**
 * SLAStatusBadge - Shows SLA compliance status as a compact chip or expanded card
 * Color-coded with icons for OnTrack, AtRisk, Breached, Met, Unknown
 */

import React, { useMemo } from 'react';
import { Chip, Box, Typography, Tooltip } from '@mui/material';
import {
  Check as CheckIcon,
  Warning as WarningIcon,
  Error as ErrorIcon,
  CheckCircle as CheckCircleIcon,
  HelpOutline as UnknownIcon,
  Schedule as ScheduleIcon,
} from '@mui/icons-material';

export interface SLAStatusBadgeProps {
  responseDeadline?: string;
  resolutionDeadline?: string;
  status: 'OnTrack' | 'AtRisk' | 'Breached' | 'Met' | 'Unknown';
  compact?: boolean;
}

interface SLAConfig {
  label: string;
  color: 'success' | 'warning' | 'error' | 'default';
  icon: React.ReactElement;
}

const slaConfigMap: Record<string, SLAConfig> = {
  OnTrack: { label: 'On Track', color: 'success', icon: <CheckIcon fontSize="small" /> },
  AtRisk: { label: 'At Risk', color: 'warning', icon: <WarningIcon fontSize="small" /> },
  Breached: { label: 'Breached', color: 'error', icon: <ErrorIcon fontSize="small" /> },
  Met: { label: 'SLA Met', color: 'success', icon: <CheckCircleIcon fontSize="small" /> },
  Unknown: { label: 'Unknown', color: 'default', icon: <UnknownIcon fontSize="small" /> },
};

const formatTimeRemaining = (deadline: string): string => {
  const now = new Date();
  const target = new Date(deadline);
  const diffMs = target.getTime() - now.getTime();

  if (diffMs <= 0) return 'Overdue';

  const totalMinutes = Math.floor(diffMs / 60000);
  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;

  if (hours > 24) {
    const days = Math.floor(hours / 24);
    return `${days}d ${hours % 24}h remaining`;
  }

  if (hours > 0) {
    return `${hours}h ${minutes}m remaining`;
  }

  return `${minutes}m remaining`;
};

const SLAStatusBadge: React.FC<SLAStatusBadgeProps> = ({
  responseDeadline,
  resolutionDeadline,
  status,
  compact = true,
}) => {
  const config = slaConfigMap[status] ?? slaConfigMap.Unknown;

  const timeInfo = useMemo(() => {
    const items: { label: string; value: string }[] = [];
    if (responseDeadline) {
      items.push({ label: 'Response', value: formatTimeRemaining(responseDeadline) });
    }
    if (resolutionDeadline) {
      items.push({ label: 'Resolution', value: formatTimeRemaining(resolutionDeadline) });
    }
    return items;
  }, [responseDeadline, resolutionDeadline]);

  if (compact) {
    return (
      <Tooltip title={timeInfo.map((t) => `${t.label}: ${t.value}`).join(' | ') || config.label}>
        <Chip label={config.label} color={config.color} icon={config.icon} size="small" />
      </Tooltip>
    );
  }

  return (
    <Box
      sx={{
        p: 1.5,
        borderRadius: 1,
        border: '1px solid',
        borderColor: `${config.color}.main`,
        bgcolor: `${config.color === 'default' ? 'grey.50' : `${config.color}.50`}`,
      }}
    >
      <Box display="flex" alignItems="center" gap={1} mb={timeInfo.length > 0 ? 1 : 0}>
        {config.icon}
        <Typography variant="subtitle2">{config.label}</Typography>
      </Box>

      {timeInfo.map((item) => (
        <Box key={item.label} display="flex" alignItems="center" gap={0.5} mt={0.5}>
          <ScheduleIcon sx={{ fontSize: 14, color: 'text.secondary' }} />
          <Typography variant="caption" color="text.secondary">
            {item.label}: <strong>{item.value}</strong>
          </Typography>
        </Box>
      ))}
    </Box>
  );
};

export default SLAStatusBadge;
