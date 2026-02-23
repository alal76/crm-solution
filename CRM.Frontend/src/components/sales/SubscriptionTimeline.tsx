/**
 * SubscriptionTimeline - Vertical timeline showing subscription lifecycle events
 */
import React from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Skeleton,
  Stack,
} from '@mui/material';
import {
  PlayArrow as CreatedIcon,
  TrendingUp as UpgradedIcon,
  TrendingDown as DowngradedIcon,
  Autorenew as RenewedIcon,
  Cancel as CancelledIcon,
  Pause as PausedIcon,
  PlayCircle as ResumedIcon,
  Payment as PaymentIcon,
} from '@mui/icons-material';

export interface TimelineEvent {
  date: string;
  event: string;
  description?: string;
  type: 'created' | 'upgraded' | 'downgraded' | 'renewed' | 'cancelled' | 'paused' | 'resumed' | 'payment';
}

interface SubscriptionTimelineProps {
  events: TimelineEvent[];
  loading?: boolean;
}

const EVENT_CONFIG: Record<TimelineEvent['type'], { icon: React.ReactNode; color: string }> = {
  created: { icon: <CreatedIcon fontSize="small" />, color: '#1976d2' },
  upgraded: { icon: <UpgradedIcon fontSize="small" />, color: '#2e7d32' },
  downgraded: { icon: <DowngradedIcon fontSize="small" />, color: '#ed6c02' },
  renewed: { icon: <RenewedIcon fontSize="small" />, color: '#0288d1' },
  cancelled: { icon: <CancelledIcon fontSize="small" />, color: '#d32f2f' },
  paused: { icon: <PausedIcon fontSize="small" />, color: '#f9a825' },
  resumed: { icon: <ResumedIcon fontSize="small" />, color: '#2e7d32' },
  payment: { icon: <PaymentIcon fontSize="small" />, color: '#6a1b9a' },
};

const SubscriptionTimeline: React.FC<SubscriptionTimelineProps> = ({ events, loading = false }) => {
  if (loading) {
    return (
      <Card variant="outlined">
        <CardContent>
          <Skeleton width="40%" height={28} sx={{ mb: 2 }} />
          {[1, 2, 3].map((i) => (
            <Box key={i} sx={{ display: 'flex', gap: 2, mb: 2 }}>
              <Skeleton variant="circular" width={32} height={32} />
              <Box sx={{ flex: 1 }}>
                <Skeleton width="50%" height={20} />
                <Skeleton width="30%" height={16} />
              </Box>
            </Box>
          ))}
        </CardContent>
      </Card>
    );
  }

  if (events.length === 0) {
    return (
      <Card variant="outlined">
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Timeline
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ py: 4, textAlign: 'center' }}>
            No events recorded
          </Typography>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card variant="outlined">
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Timeline
        </Typography>
        <Stack spacing={0}>
          {events.map((evt, idx) => {
            const config = EVENT_CONFIG[evt.type];
            const isLast = idx === events.length - 1;
            return (
              <Box key={idx} sx={{ display: 'flex', gap: 2, position: 'relative' }}>
                {/* Connector line */}
                {!isLast && (
                  <Box
                    sx={{
                      position: 'absolute',
                      left: 15,
                      top: 36,
                      bottom: 0,
                      width: 2,
                      bgcolor: 'divider',
                    }}
                  />
                )}
                {/* Icon */}
                <Box
                  sx={{
                    width: 32,
                    height: 32,
                    borderRadius: '50%',
                    bgcolor: config.color,
                    color: '#fff',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    flexShrink: 0,
                    zIndex: 1,
                  }}
                >
                  {config.icon}
                </Box>
                {/* Content */}
                <Box sx={{ pb: 2.5, minWidth: 0 }}>
                  <Typography variant="body2" fontWeight={600}>
                    {evt.event}
                  </Typography>
                  {evt.description && (
                    <Typography variant="caption" color="text.secondary" display="block">
                      {evt.description}
                    </Typography>
                  )}
                  <Typography variant="caption" color="text.disabled">
                    {new Date(evt.date).toLocaleDateString('en-US', {
                      year: 'numeric',
                      month: 'short',
                      day: 'numeric',
                      hour: '2-digit',
                      minute: '2-digit',
                    })}
                  </Typography>
                </Box>
              </Box>
            );
          })}
        </Stack>
      </CardContent>
    </Card>
  );
};

export default SubscriptionTimeline;
