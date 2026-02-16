/**
 * IncidentPriorityBadge - Priority display with icon and color
 */

import React, { useMemo } from 'react';
import { Chip, ChipProps } from '@mui/material';
import {
  ErrorOutline as CriticalIcon,
  SignalCellularAlt as HighIcon,
  SignalCellularAlt2Bar as MediumIcon,
  SignalCellular1Bar as LowIcon,
  Schedule as PlanningIcon,
} from '@mui/icons-material';
import { IncidentPriority } from '../../services/incidentService';

interface IncidentPriorityBadgeProps {
  priority: IncidentPriority;
  size?: 'small' | 'medium';
}

export const IncidentPriorityBadge: React.FC<IncidentPriorityBadgeProps> = ({
  priority,
  size = 'medium',
}) => {
  const config = useMemo(() => {
    const configs: Record<IncidentPriority, { label: string; color: ChipProps['color']; icon: React.ReactNode }> = {
      [IncidentPriority.Critical]: {
        label: 'Critical',
        color: 'error',
        icon: <CriticalIcon />,
      },
      [IncidentPriority.High]: {
        label: 'High',
        color: 'warning',
        icon: <HighIcon />,
      },
      [IncidentPriority.Medium]: {
        label: 'Medium',
        color: 'info',
        icon: <MediumIcon />,
      },
      [IncidentPriority.Low]: {
        label: 'Low',
        color: 'success',
        icon: <LowIcon />,
      },
      [IncidentPriority.Planning]: {
        label: 'Planning',
        color: 'default',
        icon: <PlanningIcon />,
      },
    };
    return configs[priority];
  }, [priority]);

  return (
    <Chip
      label={config.label}
      color={config.color}
      icon={config.icon}
      size={size === 'small' ? 'small' : 'medium'}
      variant="filled"
      sx={{ fontWeight: 500 }}
    />
  );
};

export default IncidentPriorityBadge;
