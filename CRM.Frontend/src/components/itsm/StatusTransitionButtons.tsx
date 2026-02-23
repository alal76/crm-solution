/**
 * StatusTransitionButtons - Renders valid status transition buttons based on current status
 * Implements ITSM workflow status transition rules
 */

import React, { useMemo } from 'react';
import { Box, Button, CircularProgress, Tooltip } from '@mui/material';
import {
  PlayArrow as PlayArrowIcon,
  Pause as PauseIcon,
  CheckCircle as CheckCircleIcon,
  Close as CloseIcon,
  ArrowUpward as EscalateIcon,
  Replay as ReopenIcon,
} from '@mui/icons-material';

export interface StatusTransitionButtonsProps {
  currentStatus: string;
  onTransition: (newStatus: string) => void;
  disabled?: boolean;
  loading?: boolean;
}

interface TransitionOption {
  status: string;
  label: string;
  icon: React.ReactElement;
  color: 'primary' | 'secondary' | 'success' | 'warning' | 'error' | 'info' | 'inherit';
}

const transitionMap: Record<string, TransitionOption[]> = {
  Open: [
    { status: 'InProgress', label: 'Start Work', icon: <PlayArrowIcon />, color: 'primary' },
    { status: 'OnHold', label: 'Put On Hold', icon: <PauseIcon />, color: 'warning' },
    { status: 'Closed', label: 'Close', icon: <CloseIcon />, color: 'inherit' },
  ],
  New: [
    { status: 'InProgress', label: 'Start Work', icon: <PlayArrowIcon />, color: 'primary' },
    { status: 'OnHold', label: 'Put On Hold', icon: <PauseIcon />, color: 'warning' },
    { status: 'Closed', label: 'Close', icon: <CloseIcon />, color: 'inherit' },
  ],
  InProgress: [
    { status: 'OnHold', label: 'Put On Hold', icon: <PauseIcon />, color: 'warning' },
    { status: 'Resolved', label: 'Resolve', icon: <CheckCircleIcon />, color: 'success' },
    { status: 'Escalated', label: 'Escalate', icon: <EscalateIcon />, color: 'error' },
  ],
  in_progress: [
    { status: 'OnHold', label: 'Put On Hold', icon: <PauseIcon />, color: 'warning' },
    { status: 'Resolved', label: 'Resolve', icon: <CheckCircleIcon />, color: 'success' },
    { status: 'Escalated', label: 'Escalate', icon: <EscalateIcon />, color: 'error' },
  ],
  OnHold: [
    { status: 'InProgress', label: 'Resume Work', icon: <PlayArrowIcon />, color: 'primary' },
    { status: 'Closed', label: 'Close', icon: <CloseIcon />, color: 'inherit' },
  ],
  on_hold: [
    { status: 'InProgress', label: 'Resume Work', icon: <PlayArrowIcon />, color: 'primary' },
    { status: 'Closed', label: 'Close', icon: <CloseIcon />, color: 'inherit' },
  ],
  Resolved: [
    { status: 'Closed', label: 'Close', icon: <CloseIcon />, color: 'inherit' },
    { status: 'InProgress', label: 'Reopen', icon: <ReopenIcon />, color: 'warning' },
  ],
  resolved: [
    { status: 'Closed', label: 'Close', icon: <CloseIcon />, color: 'inherit' },
    { status: 'InProgress', label: 'Reopen', icon: <ReopenIcon />, color: 'warning' },
  ],
  Escalated: [
    { status: 'InProgress', label: 'Resume Work', icon: <PlayArrowIcon />, color: 'primary' },
    { status: 'Closed', label: 'Close', icon: <CloseIcon />, color: 'inherit' },
  ],
};

const StatusTransitionButtons: React.FC<StatusTransitionButtonsProps> = ({
  currentStatus,
  onTransition,
  disabled = false,
  loading = false,
}) => {
  const transitions = useMemo(() => {
    return transitionMap[currentStatus] ?? [];
  }, [currentStatus]);

  if (transitions.length === 0) {
    return null;
  }

  return (
    <Box display="flex" gap={1} flexWrap="wrap">
      {transitions.map((transition) => (
        <Tooltip key={transition.status} title={`Transition to ${transition.status}`}>
          <span>
            <Button
              variant="outlined"
              size="small"
              color={transition.color}
              startIcon={
                loading ? (
                  <CircularProgress size={16} color="inherit" />
                ) : (
                  transition.icon
                )
              }
              onClick={() => onTransition(transition.status)}
              disabled={disabled || loading}
            >
              {transition.label}
            </Button>
          </span>
        </Tooltip>
      ))}
    </Box>
  );
};

export default StatusTransitionButtons;
