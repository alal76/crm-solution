/**
 * IncidentStatusBadge - Status display with color coding for incidents
 */

import React, { useMemo } from 'react';
import { Chip, ChipProps } from '@mui/material';
import { IncidentStatus } from '../../services/incidentService';

interface IncidentStatusBadgeProps {
  status: IncidentStatus;
  size?: 'small' | 'medium';
  clickable?: boolean;
  onClick?: () => void;
  icon?: React.ReactNode;
}

export const IncidentStatusBadge: React.FC<IncidentStatusBadgeProps> = ({
  status,
  size = 'medium',
  clickable = false,
  onClick,
  icon,
}) => {
  const config = useMemo(() => {
    const configs: Record<IncidentStatus, { label: string; color: ChipProps['color']; variant: ChipProps['variant'] }> = {
      [IncidentStatus.New]: {
        label: 'New',
        color: 'info',
        variant: 'outlined',
      },
      [IncidentStatus.InProgress]: {
        label: 'In Progress',
        color: 'warning',
        variant: 'filled',
      },
      [IncidentStatus.OnHold]: {
        label: 'On Hold',
        color: 'default',
        variant: 'outlined',
      },
      [IncidentStatus.Resolved]: {
        label: 'Resolved',
        color: 'success',
        variant: 'filled',
      },
      [IncidentStatus.Closed]: {
        label: 'Closed',
        color: 'default',
        variant: 'filled',
      },
      [IncidentStatus.Cancelled]: {
        label: 'Cancelled',
        color: 'error',
        variant: 'outlined',
      },
      [IncidentStatus.Reopened]: {
        label: 'Reopened',
        color: 'warning',
        variant: 'outlined',
      },
    };
    return configs[status];
  }, [status]);

  return (
    <Chip
      label={config.label}
      color={config.color}
      variant={config.variant}
      size={size === 'small' ? 'small' : 'medium'}
      icon={icon}
      onClick={onClick}
      clickable={clickable}
      sx={{
        fontWeight: 500,
        minWidth: 80,
      }}
    />
  );
};

export default IncidentStatusBadge;
