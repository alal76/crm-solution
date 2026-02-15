/**
 * IncidentSLAIndicator - Shows SLA progress with visual indicator
 */

import React from 'react';
import {
  Box,
  LinearProgress,
  Typography,
  Paper,
  Stack,
  Chip,
  ChipProps,
} from '@mui/material';
import {
  Warning as WarningIcon,
  CheckCircle as OkIcon,
} from '@mui/icons-material';
import { IncidentSLA } from '../../services/incidentService';

interface IncidentSLAIndicatorProps {
  sla: IncidentSLA;
  dense?: boolean;
}

export const IncidentSLAIndicator: React.FC<IncidentSLAIndicatorProps> = ({
  sla,
  dense = false,
}) => {
  const getStatusColor = (breached: boolean): ChipProps['color'] => {
    return breached ? 'error' : 'success';
  };

  const getProgressColor = (percent: number): 'error' | 'warning' | 'success' => {
    if (percent >= 100) return 'error';
    if (percent >= 70) return 'warning';
    return 'success';
  };

  const getFormattedTime = (minutes: number): string => {
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    if (hours === 0) return `${mins}m`;
    if (mins === 0) return `${hours}h`;
    return `${hours}h ${mins}m`;
  };

  if (dense) {
    return (
      <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
        <Box sx={{ flex: 1 }}>
          <Typography variant="caption" color="text.secondary">
            Response: {getFormattedTime(sla.responseTime)}
          </Typography>
          <LinearProgress
            variant="determinate"
            value={sla.responsePercentComplete}
            color={getProgressColor(sla.responsePercentComplete)}
            sx={{ height: 4, mb: 0.5 }}
          />
          <Chip
            icon={sla.responseBreached ? <WarningIcon /> : <OkIcon />}
            label={sla.responseBreached ? 'Breached' : 'On Track'}
            size="small"
            color={getStatusColor(sla.responseBreached)}
            variant="outlined"
          />
        </Box>
        <Box sx={{ flex: 1 }}>
          <Typography variant="caption" color="text.secondary">
            Resolution: {getFormattedTime(sla.resolutionTime)}
          </Typography>
          <LinearProgress
            variant="determinate"
            value={sla.resolutionPercentComplete}
            color={getProgressColor(sla.resolutionPercentComplete)}
            sx={{ height: 4, mb: 0.5 }}
          />
          <Chip
            icon={sla.resolutionBreached ? <WarningIcon /> : <OkIcon />}
            label={sla.resolutionBreached ? 'Breached' : 'On Track'}
            size="small"
            color={getStatusColor(sla.resolutionBreached)}
            variant="outlined"
          />
        </Box>
      </Box>
    );
  }

  return (
    <Paper sx={{ p: 2, bgcolor: 'background.default' }}>
      <Stack spacing={2}>
        {/* Response SLA */}
        <Box>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
            <Typography variant="subtitle2" fontWeight="bold">
              Response SLA
            </Typography>
            <Chip
              icon={sla.responseBreached ? <WarningIcon /> : <OkIcon />}
              label={`${sla.responsePercentComplete.toFixed(0)}%`}
              size="small"
              color={getStatusColor(sla.responseBreached)}
              variant="filled"
            />
          </Box>
          <LinearProgress
            variant="determinate"
            value={Math.min(sla.responsePercentComplete, 100)}
            color={getProgressColor(sla.responsePercentComplete)}
            sx={{ height: 8, borderRadius: 1 }}
          />
          <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5, display: 'block' }}>
            Due: {new Date(sla.responseDeadline).toLocaleString()} ({getFormattedTime(sla.responseTime)})
          </Typography>
        </Box>

        {/* Resolution SLA */}
        <Box>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
            <Typography variant="subtitle2" fontWeight="bold">
              Resolution SLA
            </Typography>
            <Chip
              icon={sla.resolutionBreached ? <WarningIcon /> : <OkIcon />}
              label={`${sla.resolutionPercentComplete.toFixed(0)}%`}
              size="small"
              color={getStatusColor(sla.resolutionBreached)}
              variant="filled"
            />
          </Box>
          <LinearProgress
            variant="determinate"
            value={Math.min(sla.resolutionPercentComplete, 100)}
            color={getProgressColor(sla.resolutionPercentComplete)}
            sx={{ height: 8, borderRadius: 1 }}
          />
          <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5, display: 'block' }}>
            Due: {new Date(sla.resolutionDeadline).toLocaleString()} ({getFormattedTime(sla.resolutionTime)})
          </Typography>
        </Box>
      </Stack>
    </Paper>
  );
};

export default IncidentSLAIndicator;
