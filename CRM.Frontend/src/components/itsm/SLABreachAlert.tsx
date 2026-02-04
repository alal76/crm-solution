// SLA Breach Alert - Alert component for SLA breaches
// Part of ITSM Enhancement Plan - Phase 1.3

import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Alert,
  AlertTitle,
  Stack,
  Chip,
  Button,
  IconButton,
  Collapse,
  Snackbar,
  Slide,
  SlideProps,
  Tooltip,
  LinearProgress,
  Divider,
} from '@mui/material';
import {
  Warning as WarningIcon,
  Error as ErrorIcon,
  Notifications as NotificationIcon,
  NotificationsOff as DismissIcon,
  AccessTime as TimeIcon,
  ArrowUpward as EscalateIcon,
  Close as CloseIcon,
  ExpandMore as ExpandIcon,
  ExpandLess as CollapseIcon,
  Person as AssigneeIcon,
} from '@mui/icons-material';

export type BreachType = 'response' | 'resolution';
export type BreachSeverity = 'warning' | 'imminent' | 'breached';

export interface SLABreachInfo {
  id: number;
  ticketNumber: string;
  ticketTitle: string;
  breachType: BreachType;
  severity: BreachSeverity;
  dueAt: Date | string;
  breachedAt?: Date | string;
  minutesRemaining?: number;
  minutesOverdue?: number;
  assignedTo?: string;
  priority: number;
  escalationLevel: number;
}

export interface SLABreachAlertProps {
  breach: SLABreachInfo;
  onDismiss?: (breachId: number) => void;
  onEscalate?: (breachId: number) => void;
  onViewTicket?: (ticketNumber: string) => void;
  showSnackbar?: boolean;
  autoHideDuration?: number;
  variant?: 'inline' | 'banner' | 'snackbar';
}

export interface SLABreachBannerProps {
  breaches: SLABreachInfo[];
  onDismiss?: (breachId: number) => void;
  onEscalate?: (breachId: number) => void;
  onViewTicket?: (ticketNumber: string) => void;
  onDismissAll?: () => void;
  maxDisplay?: number;
}

const getSeverityColor = (severity: BreachSeverity): 'warning' | 'error' => {
  switch (severity) {
    case 'warning':
      return 'warning';
    case 'imminent':
      return 'warning';
    case 'breached':
      return 'error';
    default:
      return 'warning';
  }
};

const getSeverityLabel = (severity: BreachSeverity): string => {
  switch (severity) {
    case 'warning':
      return 'At Risk';
    case 'imminent':
      return 'Imminent Breach';
    case 'breached':
      return 'Breached';
    default:
      return severity;
  }
};

const formatTimeRemaining = (minutes: number | undefined): string => {
  if (minutes === undefined) return 'N/A';
  if (minutes < 0) return `${Math.abs(minutes)}m overdue`;
  if (minutes === 0) return 'Due now';
  if (minutes < 60) return `${minutes}m remaining`;
  const hours = Math.floor(minutes / 60);
  const mins = minutes % 60;
  return `${hours}h ${mins}m remaining`;
};

const formatOverdue = (minutes: number | undefined): string => {
  if (minutes === undefined) return '';
  if (minutes < 60) return `${minutes}m overdue`;
  const hours = Math.floor(minutes / 60);
  const mins = minutes % 60;
  return `${hours}h ${mins}m overdue`;
};

function SlideTransition(props: SlideProps) {
  return <Slide {...props} direction="down" />;
}

// Single breach alert component
export const SLABreachAlert: React.FC<SLABreachAlertProps> = ({
  breach,
  onDismiss,
  onEscalate,
  onViewTicket,
  showSnackbar = false,
  autoHideDuration = 10000,
  variant = 'inline',
}) => {
  const [open, setOpen] = useState(true);
  const severityColor = getSeverityColor(breach.severity);

  const handleDismiss = () => {
    setOpen(false);
    onDismiss?.(breach.id);
  };

  const handleViewTicket = () => {
    onViewTicket?.(breach.ticketNumber);
  };

  const alertContent = (
    <Alert
      severity={severityColor}
      icon={breach.severity === 'breached' ? <ErrorIcon /> : <WarningIcon />}
      action={
        <Stack direction="row" spacing={0.5}>
          {onEscalate && breach.severity !== 'breached' && (
            <Tooltip title="Escalate">
              <IconButton
                size="small"
                onClick={() => onEscalate(breach.id)}
                color="inherit"
              >
                <EscalateIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
          <Tooltip title="Dismiss">
            <IconButton size="small" onClick={handleDismiss} color="inherit">
              <CloseIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        </Stack>
      }
      sx={{
        alignItems: 'flex-start',
        '& .MuiAlert-action': { pt: 0 },
      }}
    >
      <AlertTitle sx={{ fontWeight: 600 }}>
        SLA {breach.breachType === 'response' ? 'Response' : 'Resolution'}{' '}
        {getSeverityLabel(breach.severity)}
      </AlertTitle>

      <Stack spacing={1}>
        <Stack direction="row" alignItems="center" spacing={1} flexWrap="wrap">
          <Typography
            variant="body2"
            sx={{ cursor: 'pointer', textDecoration: 'underline' }}
            onClick={handleViewTicket}
          >
            {breach.ticketNumber}
          </Typography>
          <Chip
            label={`P${breach.priority}`}
            size="small"
            color={breach.priority === 1 ? 'error' : 'default'}
            sx={{ height: 20 }}
          />
          {breach.escalationLevel > 0 && (
            <Chip
              icon={<EscalateIcon sx={{ fontSize: 12 }} />}
              label={`Escalation ${breach.escalationLevel}`}
              size="small"
              color="warning"
              sx={{ height: 20 }}
            />
          )}
        </Stack>

        <Typography variant="body2" noWrap sx={{ maxWidth: 400 }}>
          {breach.ticketTitle}
        </Typography>

        <Stack direction="row" alignItems="center" spacing={2}>
          <Stack direction="row" alignItems="center" spacing={0.5}>
            <TimeIcon fontSize="small" />
            <Typography variant="caption" fontWeight={600}>
              {breach.severity === 'breached'
                ? formatOverdue(breach.minutesOverdue)
                : formatTimeRemaining(breach.minutesRemaining)}
            </Typography>
          </Stack>
          {breach.assignedTo && (
            <Stack direction="row" alignItems="center" spacing={0.5}>
              <AssigneeIcon fontSize="small" />
              <Typography variant="caption">{breach.assignedTo}</Typography>
            </Stack>
          )}
        </Stack>
      </Stack>
    </Alert>
  );

  if (variant === 'snackbar' || showSnackbar) {
    return (
      <Snackbar
        open={open}
        autoHideDuration={autoHideDuration}
        onClose={handleDismiss}
        TransitionComponent={SlideTransition}
        anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
      >
        {alertContent}
      </Snackbar>
    );
  }

  return <Collapse in={open}>{alertContent}</Collapse>;
};

// Multi-breach banner component
export const SLABreachBanner: React.FC<SLABreachBannerProps> = ({
  breaches,
  onDismiss,
  onEscalate,
  onViewTicket,
  onDismissAll,
  maxDisplay = 3,
}) => {
  const [expanded, setExpanded] = useState(false);

  if (breaches.length === 0) return null;

  // Sort by severity and time
  const sortedBreaches = [...breaches].sort((a, b) => {
    // Breached first
    if (a.severity === 'breached' && b.severity !== 'breached') return -1;
    if (b.severity === 'breached' && a.severity !== 'breached') return 1;
    // Then by priority
    return a.priority - b.priority;
  });

  const criticalCount = breaches.filter((b) => b.severity === 'breached').length;
  const warningCount = breaches.filter((b) => b.severity !== 'breached').length;
  const displayedBreaches = expanded ? sortedBreaches : sortedBreaches.slice(0, maxDisplay);
  const hasMore = breaches.length > maxDisplay;

  return (
    <Box
      sx={{
        backgroundColor: criticalCount > 0 ? '#ffebee' : '#fff3e0',
        borderBottom: `2px solid ${criticalCount > 0 ? '#f44336' : '#ff9800'}`,
        p: 2,
      }}
    >
      {/* Summary header */}
      <Stack
        direction="row"
        alignItems="center"
        justifyContent="space-between"
        sx={{ mb: 1 }}
      >
        <Stack direction="row" alignItems="center" spacing={1}>
          {criticalCount > 0 ? (
            <ErrorIcon color="error" />
          ) : (
            <WarningIcon color="warning" />
          )}
          <Typography variant="subtitle1" fontWeight={600}>
            {breaches.length} SLA Alert{breaches.length > 1 ? 's' : ''}
          </Typography>
          {criticalCount > 0 && (
            <Chip
              label={`${criticalCount} Breached`}
              size="small"
              color="error"
            />
          )}
          {warningCount > 0 && (
            <Chip
              label={`${warningCount} At Risk`}
              size="small"
              color="warning"
            />
          )}
        </Stack>

        <Stack direction="row" spacing={1}>
          {hasMore && (
            <Button
              size="small"
              endIcon={expanded ? <CollapseIcon /> : <ExpandIcon />}
              onClick={() => setExpanded(!expanded)}
            >
              {expanded ? 'Show Less' : `Show All (${breaches.length})`}
            </Button>
          )}
          {onDismissAll && (
            <Tooltip title="Dismiss All">
              <IconButton size="small" onClick={onDismissAll}>
                <DismissIcon />
              </IconButton>
            </Tooltip>
          )}
        </Stack>
      </Stack>

      {/* Individual alerts */}
      <Stack spacing={1}>
        {displayedBreaches.map((breach) => (
          <SLABreachAlert
            key={breach.id}
            breach={breach}
            onDismiss={onDismiss}
            onEscalate={onEscalate}
            onViewTicket={onViewTicket}
            variant="inline"
          />
        ))}
      </Stack>
    </Box>
  );
};

export default SLABreachAlert;
