/**
 * ServiceRequestTimeline - Timeline showing history of a service request
 * TODO-SD001-002 (P2)
 *
 * Displays color-coded, icon-enhanced events for status changes, comments,
 * assignments, SLA breaches, resolutions, and escalations.
 */

import React from 'react';
import {
  Box,
  Typography,
  Paper,
  Avatar,
  Skeleton,
  Chip,
  Tooltip,
} from '@mui/material';
import {
  Timeline,
  TimelineItem,
  TimelineSeparator,
  TimelineConnector,
  TimelineContent,
  TimelineDot,
  TimelineOppositeContent,
} from '@mui/lab';
import {
  SwapHoriz as StatusChangeIcon,
  Comment as CommentIcon,
  PersonAdd as AssignmentIcon,
  Warning as SLABreachIcon,
  CheckCircle as ResolutionIcon,
  ArrowUpward as EscalationIcon,
} from '@mui/icons-material';

// ─── Types ───────────────────────────────────────────────────────────────────

export type TimelineEventType =
  | 'status_change'
  | 'comment'
  | 'assignment'
  | 'sla_breach'
  | 'resolution'
  | 'escalation';

export interface TimelineEvent {
  id: number;
  timestamp: string;
  action: string;
  description: string;
  userId?: number;
  userName?: string;
  type: TimelineEventType;
}

export interface ServiceRequestTimelineProps {
  serviceRequestId: number;
  events?: TimelineEvent[];
  loading?: boolean;
}

// ─── Event config ─────────────────────────────────────────────────────────────

interface EventConfig {
  icon: React.ReactElement;
  color: 'primary' | 'warning' | 'success' | 'error' | 'grey' | 'secondary';
  chipColor: 'primary' | 'warning' | 'success' | 'error' | 'default' | 'secondary';
  label: string;
}

const EVENT_CONFIG: Record<TimelineEventType, EventConfig> = {
  status_change: {
    icon: <StatusChangeIcon fontSize="small" />,
    color: 'primary',
    chipColor: 'primary',
    label: 'Status Change',
  },
  comment: {
    icon: <CommentIcon fontSize="small" />,
    color: 'secondary',
    chipColor: 'secondary',
    label: 'Comment',
  },
  assignment: {
    icon: <AssignmentIcon fontSize="small" />,
    color: 'primary',
    chipColor: 'default',
    label: 'Assignment',
  },
  sla_breach: {
    icon: <SLABreachIcon fontSize="small" />,
    color: 'warning',
    chipColor: 'warning',
    label: 'SLA Breach',
  },
  resolution: {
    icon: <ResolutionIcon fontSize="small" />,
    color: 'success',
    chipColor: 'success',
    label: 'Resolution',
  },
  escalation: {
    icon: <EscalationIcon fontSize="small" />,
    color: 'error',
    chipColor: 'error',
    label: 'Escalation',
  },
};

// ─── Helper ───────────────────────────────────────────────────────────────────

function formatTimestamp(ts: string): string {
  const date = new Date(ts);
  if (isNaN(date.getTime())) return ts;
  return date.toLocaleString(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  });
}

function getUserInitials(name?: string): string {
  if (!name) return '?';
  const parts = name.trim().split(' ');
  if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
}

// ─── Skeleton loading rows ────────────────────────────────────────────────────

const TimelineSkeleton: React.FC = () => (
  <Box>
    {[1, 2, 3].map((i) => (
      <Box key={i} sx={{ display: 'flex', gap: 2, mb: 3, alignItems: 'flex-start' }}>
        <Skeleton variant="circular" width={36} height={36} sx={{ flexShrink: 0 }} />
        <Box sx={{ flex: 1 }}>
          <Skeleton variant="text" width="40%" height={20} />
          <Skeleton variant="text" width="75%" height={18} sx={{ mt: 0.5 }} />
          <Skeleton variant="text" width="30%" height={16} sx={{ mt: 0.5 }} />
        </Box>
      </Box>
    ))}
  </Box>
);

// ─── Component ────────────────────────────────────────────────────────────────

const ServiceRequestTimeline: React.FC<ServiceRequestTimelineProps> = ({
  serviceRequestId: _serviceRequestId,
  events = [],
  loading = false,
}) => {
  if (loading) {
    return (
      <Paper variant="outlined" sx={{ p: 3 }}>
        <Typography variant="subtitle1" fontWeight="bold" gutterBottom>
          Activity Timeline
        </Typography>
        <TimelineSkeleton />
      </Paper>
    );
  }

  if (events.length === 0) {
    return (
      <Paper variant="outlined" sx={{ p: 3 }}>
        <Typography variant="subtitle1" fontWeight="bold" gutterBottom>
          Activity Timeline
        </Typography>
        <Typography color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
          No timeline events yet.
        </Typography>
      </Paper>
    );
  }

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Typography variant="subtitle1" fontWeight="bold" sx={{ px: 1, mb: 1 }}>
        Activity Timeline
      </Typography>

      <Timeline sx={{ p: 0, m: 0 }}>
        {events.map((event, idx) => {
          const config = EVENT_CONFIG[event.type] ?? EVENT_CONFIG.comment;
          const isLast = idx === events.length - 1;

          return (
            <TimelineItem key={event.id}>
              {/* Left column – timestamp */}
              <TimelineOppositeContent
                sx={{ flex: 0.35, pr: 1.5, pt: '14px' }}
                variant="body2"
                color="text.secondary"
              >
                <Tooltip title={event.timestamp} placement="left">
                  <span>{formatTimestamp(event.timestamp)}</span>
                </Tooltip>
              </TimelineOppositeContent>

              {/* Centre – dot + connector */}
              <TimelineSeparator>
                <TimelineDot color={config.color} sx={{ m: 0.5 }}>
                  {config.icon}
                </TimelineDot>
                {!isLast && <TimelineConnector />}
              </TimelineSeparator>

              {/* Right column – event card */}
              <TimelineContent sx={{ py: 1, px: 1.5 }}>
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    flexWrap: 'wrap',
                    gap: 1,
                    mb: 0.5,
                  }}
                >
                  <Chip
                    label={config.label}
                    size="small"
                    color={config.chipColor}
                    variant="outlined"
                  />
                  <Typography variant="body2" fontWeight="medium">
                    {event.action}
                  </Typography>
                </Box>

                <Typography variant="body2" color="text.secondary">
                  {event.description}
                </Typography>

                {event.userName && (
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75, mt: 0.75 }}>
                    <Avatar
                      sx={{ width: 20, height: 20, fontSize: 10, bgcolor: 'primary.light' }}
                    >
                      {getUserInitials(event.userName)}
                    </Avatar>
                    <Typography variant="caption" color="text.secondary">
                      {event.userName}
                    </Typography>
                  </Box>
                )}
              </TimelineContent>
            </TimelineItem>
          );
        })}
      </Timeline>
    </Paper>
  );
};

export default ServiceRequestTimeline;
