/**
 * ServiceRequestCard - Card component showing a service request summary
 * Displays title, status chip, priority chip, assigned user, created date, and SLA indicator
 */

import React from 'react';
import {
  Card,
  CardContent,
  CardActionArea,
  Typography,
  Chip,
  Box,
  Avatar,
  Tooltip,
} from '@mui/material';
import {
  AccessTime as AccessTimeIcon,
  Person as PersonIcon,
} from '@mui/icons-material';

export interface ServiceRequestCardProps {
  id: number;
  title: string;
  status: string;
  priority: string;
  assignedTo?: string;
  createdAt: string;
  slaStatus?: string;
  onClick?: () => void;
}

const statusColorMap: Record<string, 'info' | 'warning' | 'success' | 'default' | 'error' | 'primary' | 'secondary'> = {
  Open: 'info',
  New: 'info',
  InProgress: 'warning',
  in_progress: 'warning',
  Active: 'warning',
  OnHold: 'default',
  on_hold: 'default',
  Resolved: 'success',
  resolved: 'success',
  Closed: 'default',
  closed: 'default',
  Escalated: 'error',
  Reopened: 'secondary',
  reopened: 'secondary',
};

const priorityColorMap: Record<string, 'error' | 'warning' | 'info' | 'success' | 'default'> = {
  Critical: 'error',
  High: 'error',
  Medium: 'warning',
  Low: 'info',
  None: 'default',
};

const slaColorMap: Record<string, 'success' | 'warning' | 'error' | 'default'> = {
  OnTrack: 'success',
  Met: 'success',
  AtRisk: 'warning',
  Breached: 'error',
  Unknown: 'default',
};

const formatDate = (dateStr: string): string => {
  try {
    return new Date(dateStr).toLocaleDateString(undefined, {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  } catch {
    return dateStr;
  }
};

const ServiceRequestCard: React.FC<ServiceRequestCardProps> = ({
  id,
  title,
  status,
  priority,
  assignedTo,
  createdAt,
  slaStatus,
  onClick,
}) => {
  const cardContent = (
    <CardContent>
      <Box display="flex" justifyContent="space-between" alignItems="flex-start" mb={1}>
        <Typography variant="subtitle2" color="text.secondary">
          #{id}
        </Typography>
        {slaStatus && (
          <Chip
            label={slaStatus}
            size="small"
            color={slaColorMap[slaStatus] ?? 'default'}
            variant="outlined"
          />
        )}
      </Box>

      <Typography variant="subtitle1" fontWeight={600} noWrap gutterBottom>
        {title}
      </Typography>

      <Box display="flex" gap={1} flexWrap="wrap" mb={1.5}>
        <Chip
          label={status}
          size="small"
          color={statusColorMap[status] ?? 'default'}
        />
        <Chip
          label={priority}
          size="small"
          color={priorityColorMap[priority] ?? 'default'}
          variant="outlined"
        />
      </Box>

      <Box display="flex" justifyContent="space-between" alignItems="center">
        {assignedTo ? (
          <Tooltip title={assignedTo}>
            <Box display="flex" alignItems="center" gap={0.5}>
              <Avatar sx={{ width: 24, height: 24, fontSize: 12 }}>
                {assignedTo.charAt(0).toUpperCase()}
              </Avatar>
              <Typography variant="caption" color="text.secondary" noWrap maxWidth={100}>
                {assignedTo}
              </Typography>
            </Box>
          </Tooltip>
        ) : (
          <Box display="flex" alignItems="center" gap={0.5}>
            <PersonIcon sx={{ fontSize: 16, color: 'text.disabled' }} />
            <Typography variant="caption" color="text.disabled">
              Unassigned
            </Typography>
          </Box>
        )}

        <Box display="flex" alignItems="center" gap={0.5}>
          <AccessTimeIcon sx={{ fontSize: 14, color: 'text.secondary' }} />
          <Typography variant="caption" color="text.secondary">
            {formatDate(createdAt)}
          </Typography>
        </Box>
      </Box>
    </CardContent>
  );

  return (
    <Card
      sx={{
        transition: 'box-shadow 0.2s, transform 0.2s',
        '&:hover': {
          boxShadow: 6,
          transform: 'translateY(-2px)',
        },
      }}
    >
      {onClick ? (
        <CardActionArea onClick={onClick}>{cardContent}</CardActionArea>
      ) : (
        cardContent
      )}
    </Card>
  );
};

export default ServiceRequestCard;
