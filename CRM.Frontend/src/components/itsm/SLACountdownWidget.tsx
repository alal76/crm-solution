// SLA Countdown Widget - Visual countdown timer with status indicators
// Part of ITSM Enhancement Plan - Phase 1.3

import React, { useState, useEffect, useMemo } from 'react';
import {
  Box,
  Typography,
  Tooltip,
  LinearProgress,
  Chip,
  Paper,
  Stack,
  IconButton,
  Collapse,
} from '@mui/material';
import {
  AccessTime as ClockIcon,
  Warning as WarningIcon,
  Error as ErrorIcon,
  CheckCircle as CheckIcon,
  Pause as PauseIcon,
  ExpandMore as ExpandIcon,
  ExpandLess as CollapseIcon,
} from '@mui/icons-material';

export type SLAStatus = 'on-track' | 'at-risk' | 'breached' | 'paused' | 'completed';

export interface SLAInstanceData {
  id: number;
  type: 'response' | 'resolution';
  label: string;
  dueAt: Date | string;
  actualAt?: Date | string | null;
  breached: boolean;
  paused: boolean;
  pausedMinutes?: number;
  businessMinutes?: number;
}

export interface SLACountdownWidgetProps {
  slaInstances: SLAInstanceData[];
  showDetails?: boolean;
  compact?: boolean;
  onBreachWarning?: (slaId: number, minutesRemaining: number) => void;
}

interface TimeRemaining {
  days: number;
  hours: number;
  minutes: number;
  seconds: number;
  totalMinutes: number;
  isOverdue: boolean;
}

const calculateTimeRemaining = (dueAt: Date | string): TimeRemaining => {
  const now = new Date();
  const due = new Date(dueAt);
  const diffMs = due.getTime() - now.getTime();
  const isOverdue = diffMs < 0;
  const absDiffMs = Math.abs(diffMs);

  const totalMinutes = Math.floor(absDiffMs / (1000 * 60));
  const days = Math.floor(absDiffMs / (1000 * 60 * 60 * 24));
  const hours = Math.floor((absDiffMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
  const minutes = Math.floor((absDiffMs % (1000 * 60 * 60)) / (1000 * 60));
  const seconds = Math.floor((absDiffMs % (1000 * 60)) / 1000);

  return { days, hours, minutes, seconds, totalMinutes, isOverdue };
};

const getSLAStatus = (
  sla: SLAInstanceData,
  timeRemaining: TimeRemaining
): SLAStatus => {
  if (sla.actualAt) return 'completed';
  if (sla.paused) return 'paused';
  if (sla.breached || timeRemaining.isOverdue) return 'breached';
  // At risk if less than 25% time remaining (using minutes as proxy)
  if (timeRemaining.totalMinutes <= 30) return 'at-risk';
  return 'on-track';
};

const getStatusColor = (status: SLAStatus): string => {
  switch (status) {
    case 'on-track':
      return '#4caf50'; // green
    case 'at-risk':
      return '#ff9800'; // orange
    case 'breached':
      return '#f44336'; // red
    case 'paused':
      return '#9e9e9e'; // grey
    case 'completed':
      return '#2196f3'; // blue
    default:
      return '#9e9e9e';
  }
};

const getStatusIcon = (status: SLAStatus) => {
  switch (status) {
    case 'on-track':
      return <ClockIcon sx={{ color: getStatusColor(status) }} />;
    case 'at-risk':
      return <WarningIcon sx={{ color: getStatusColor(status) }} />;
    case 'breached':
      return <ErrorIcon sx={{ color: getStatusColor(status) }} />;
    case 'paused':
      return <PauseIcon sx={{ color: getStatusColor(status) }} />;
    case 'completed':
      return <CheckIcon sx={{ color: getStatusColor(status) }} />;
    default:
      return <ClockIcon />;
  }
};

const formatTimeRemaining = (time: TimeRemaining, isOverdue: boolean): string => {
  const prefix = isOverdue ? '-' : '';
  if (time.days > 0) {
    return `${prefix}${time.days}d ${time.hours}h`;
  }
  if (time.hours > 0) {
    return `${prefix}${time.hours}h ${time.minutes}m`;
  }
  if (time.minutes > 0) {
    return `${prefix}${time.minutes}m ${time.seconds}s`;
  }
  return `${prefix}${time.seconds}s`;
};

interface SingleSLADisplayProps {
  sla: SLAInstanceData;
  compact?: boolean;
  onBreachWarning?: (slaId: number, minutesRemaining: number) => void;
}

const SingleSLADisplay: React.FC<SingleSLADisplayProps> = ({
  sla,
  compact = false,
  onBreachWarning,
}) => {
  const [timeRemaining, setTimeRemaining] = useState<TimeRemaining>(
    calculateTimeRemaining(sla.dueAt)
  );

  useEffect(() => {
    if (sla.actualAt || sla.paused) return;

    const interval = setInterval(() => {
      const newTime = calculateTimeRemaining(sla.dueAt);
      setTimeRemaining(newTime);

      // Trigger warning at 30, 15, and 5 minutes
      if (onBreachWarning && !newTime.isOverdue) {
        if ([30, 15, 5].includes(newTime.totalMinutes)) {
          onBreachWarning(sla.id, newTime.totalMinutes);
        }
      }
    }, 1000);

    return () => clearInterval(interval);
  }, [sla.dueAt, sla.actualAt, sla.paused, sla.id, onBreachWarning]);

  const status = getSLAStatus(sla, timeRemaining);
  const color = getStatusColor(status);
  const icon = getStatusIcon(status);

  // Calculate progress percentage (inverse - 100% when due, 0% when plenty of time)
  const progressValue = useMemo(() => {
    if (status === 'completed') return 100;
    if (status === 'breached') return 100;
    if (status === 'paused') return 50;
    // Assume 4 hours (240 minutes) is full SLA time for visualization
    const maxMinutes = 240;
    const elapsed = maxMinutes - timeRemaining.totalMinutes;
    return Math.min(100, Math.max(0, (elapsed / maxMinutes) * 100));
  }, [status, timeRemaining.totalMinutes]);

  if (compact) {
    return (
      <Tooltip
        title={`${sla.label}: ${formatTimeRemaining(timeRemaining, timeRemaining.isOverdue)} ${
          timeRemaining.isOverdue ? 'overdue' : 'remaining'
        }`}
      >
        <Chip
          label={formatTimeRemaining(timeRemaining, timeRemaining.isOverdue)}
          size="small"
          sx={{
            backgroundColor: `${color}20`,
            borderColor: color,
            color: color,
            fontWeight: 600,
            fontFamily: 'monospace',
          }}
          variant="outlined"
        />
      </Tooltip>
    );
  }

  return (
    <Box sx={{ mb: 2 }}>
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 0.5 }}>
        {icon}
        <Typography variant="body2" fontWeight={500}>
          {sla.label}
        </Typography>
        <Box sx={{ flexGrow: 1 }} />
        <Typography
          variant="body2"
          fontWeight={700}
          fontFamily="monospace"
          sx={{ color }}
        >
          {status === 'completed'
            ? 'Met'
            : status === 'paused'
            ? 'Paused'
            : formatTimeRemaining(timeRemaining, timeRemaining.isOverdue)}
        </Typography>
      </Stack>
      <LinearProgress
        variant="determinate"
        value={progressValue}
        sx={{
          height: 6,
          borderRadius: 3,
          backgroundColor: `${color}20`,
          '& .MuiLinearProgress-bar': {
            backgroundColor: color,
            borderRadius: 3,
          },
        }}
      />
      <Stack direction="row" justifyContent="space-between" sx={{ mt: 0.5 }}>
        <Typography variant="caption" color="text.secondary">
          Due: {new Date(sla.dueAt).toLocaleString()}
        </Typography>
        {sla.pausedMinutes && sla.pausedMinutes > 0 && (
          <Typography variant="caption" color="text.secondary">
            Paused: {sla.pausedMinutes}m
          </Typography>
        )}
      </Stack>
    </Box>
  );
};

export const SLACountdownWidget: React.FC<SLACountdownWidgetProps> = ({
  slaInstances,
  showDetails = true,
  compact = false,
  onBreachWarning,
}) => {
  const [expanded, setExpanded] = useState(showDetails);

  // Sort SLAs: breached first, then at-risk, then on-track
  const sortedSLAs = useMemo(() => {
    return [...slaInstances].sort((a, b) => {
      const timeA = calculateTimeRemaining(a.dueAt);
      const timeB = calculateTimeRemaining(b.dueAt);
      const statusA = getSLAStatus(a, timeA);
      const statusB = getSLAStatus(b, timeB);

      const priority: Record<SLAStatus, number> = {
        breached: 0,
        'at-risk': 1,
        'on-track': 2,
        paused: 3,
        completed: 4,
      };

      return priority[statusA] - priority[statusB];
    });
  }, [slaInstances]);

  // Get worst status for summary
  const worstStatus = useMemo((): SLAStatus => {
    let worst: SLAStatus = 'completed';
    for (const sla of slaInstances) {
      const time = calculateTimeRemaining(sla.dueAt);
      const status = getSLAStatus(sla, time);
      if (status === 'breached') return 'breached';
      if (status === 'at-risk' && worst !== 'at-risk') worst = 'at-risk';
      if (status === 'on-track' && worst === 'completed') worst = 'on-track';
    }
    return worst;
  }, [slaInstances]);

  if (compact) {
    return (
      <Stack direction="row" spacing={1} flexWrap="wrap">
        {sortedSLAs.map((sla) => (
          <SingleSLADisplay
            key={sla.id}
            sla={sla}
            compact
            onBreachWarning={onBreachWarning}
          />
        ))}
      </Stack>
    );
  }

  return (
    <Paper
      variant="outlined"
      sx={{
        p: 2,
        borderColor: getStatusColor(worstStatus),
        borderWidth: 2,
      }}
    >
      <Stack
        direction="row"
        alignItems="center"
        justifyContent="space-between"
        sx={{ mb: expanded ? 2 : 0 }}
      >
        <Stack direction="row" alignItems="center" spacing={1}>
          {getStatusIcon(worstStatus)}
          <Typography variant="subtitle1" fontWeight={600}>
            SLA Status
          </Typography>
          <Chip
            label={typeof worstStatus === 'string' ? worstStatus.replace('-', ' ').toUpperCase() : 'UNKNOWN'}
            size="small"
            sx={{
              backgroundColor: `${getStatusColor(worstStatus)}20`,
              color: getStatusColor(worstStatus),
              fontWeight: 600,
            }}
          />
        </Stack>
        <IconButton size="small" onClick={() => setExpanded(!expanded)}>
          {expanded ? <CollapseIcon /> : <ExpandIcon />}
        </IconButton>
      </Stack>

      <Collapse in={expanded}>
        {sortedSLAs.map((sla) => (
          <SingleSLADisplay
            key={sla.id}
            sla={sla}
            onBreachWarning={onBreachWarning}
          />
        ))}
      </Collapse>
    </Paper>
  );
};

export default SLACountdownWidget;
