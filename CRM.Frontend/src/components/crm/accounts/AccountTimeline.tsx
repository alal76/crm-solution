import React, { useState, useEffect } from 'react';
import {
  Box,
  CircularProgress,
  Alert,
  Typography,
  Card,
  CardContent,
  Chip,
  Stack,
  Collapse,
  IconButton,
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
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import NotesIcon from '@mui/icons-material/Notes';
import EventNoteIcon from '@mui/icons-material/EventNote';
import CallReceivedIcon from '@mui/icons-material/CallReceived';
import SupportAgentIcon from '@mui/icons-material/SupportAgent';
import activityService from '../../../services/activityService';
import noteService from '../../../services/noteService';
import interactionService from '../../../services/interactionService';
import serviceRequestService from '../../../services/serviceRequestService';

interface TimelineEvent {
  id: string;
  type: 'activity' | 'note' | 'interaction' | 'service_request';
  timestamp: Date;
  title: string;
  description?: string;
  details?: any;
  color: string;
}

interface AccountTimelineProps {
  accountId: number;
  onRefresh?: () => void;
}

/**
 * AccountTimeline Component
 * Aggregates and displays account activities, notes, interactions, and service requests
 * in a chronological timeline view.
 */
export const AccountTimeline: React.FC<AccountTimelineProps> = ({ accountId, onRefresh }) => {
  const [events, setEvents] = useState<TimelineEvent[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [expandedEventId, setExpandedEventId] = useState<string | null>(null);

  useEffect(() => {
    loadTimeline();
  }, [accountId]);

  const loadTimeline = async () => {
    try {
      setLoading(true);
      setError(null);
      const allEvents: TimelineEvent[] = [];

      try {
        // Fetch activities
        const activities = await activityService.getByEntity('Account', accountId);
        activities.forEach(activity => {
          allEvents.push({
            id: `activity-${activity.id}`,
            type: 'activity',
            timestamp: new Date(activity.createdAt),
            title: activity.subject || activity.activityType,
            description: activity.details,
            details: activity,
            color: '#2196F3', // Blue
          });
        });
      } catch (err) {
        console.warn('Failed to load activities:', err);
      }

      try {
        // Fetch notes
        const notes = await noteService.getByAccount(accountId);
        notes.forEach(note => {
          allEvents.push({
            id: `note-${note.id}`,
            type: 'note',
            timestamp: new Date(note.createdAt),
            title: 'Note Added',
            description: note.content,
            details: note,
            color: '#4CAF50', // Green
          });
        });
      } catch (err) {
        console.warn('Failed to load notes:', err);
      }

      try {
        // Fetch interactions
        const interactions = await interactionService.getByEntity('Account', accountId);
        interactions.forEach(interaction => {
          allEvents.push({
            id: `interaction-${interaction.id}`,
            type: 'interaction',
            timestamp: new Date(interaction.createdAt),
            title: `${interaction.interactionType || 'Interaction'}: ${interaction.subject || ''}`,
            description: interaction.notes,
            details: interaction,
            color: '#FF9800', // Orange
          });
        });
      } catch (err) {
        console.warn('Failed to load interactions:', err);
      }

      try {
        // Fetch recent service requests
        const serviceRequests = await serviceRequestService.getByCustomer(accountId);
        // Take only recent ones with comments
        const recent = serviceRequests
          .filter(sr => sr.status !== 'Closed' || sr.updatedAt > new Date(Date.now() - 30 * 24 * 60 * 60 * 1000))
          .slice(0, 10);

        recent.forEach(sr => {
          allEvents.push({
            id: `service-request-${sr.id}`,
            type: 'service_request',
            timestamp: new Date(sr.updatedAt || sr.createdAt),
            title: `Service Request #${sr.ticketNumber}: ${sr.title}`,
            description: sr.description,
            details: sr,
            color: '#F44336', // Red
          });
        });
      } catch (err) {
        console.warn('Failed to load service requests:', err);
      }

      // Sort by timestamp (descending - most recent first)
      allEvents.sort((a, b) => b.timestamp.getTime() - a.timestamp.getTime());

      setEvents(allEvents);
    } catch (err) {
      setError('Failed to load timeline');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (date: Date): string => {
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));

    if (days === 0) {
      const hours = Math.floor(diff / (1000 * 60 * 60));
      if (hours === 0) {
        const minutes = Math.floor(diff / (1000 * 60));
        return `${minutes}m ago`;
      }
      return `${hours}h ago`;
    }
    if (days === 1) return 'Yesterday';
    if (days < 7) return `${days} days ago`;

    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: date.getFullYear() !== now.getFullYear() ? 'numeric' : undefined,
    });
  };

  const formatDateTime = (date: Date): string => {
    return date.toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getEventIcon = (type: string) => {
    switch (type) {
      case 'activity':
        return <EventNoteIcon />;
      case 'note':
        return <NotesIcon />;
      case 'interaction':
        return <CallReceivedIcon />;
      case 'service_request':
        return <SupportAgentIcon />;
      default:
        return <EventNoteIcon />;
    }
  };

  const getEventTypeLabel = (type: string): string => {
    switch (type) {
      case 'activity':
        return 'Activity';
      case 'note':
        return 'Note';
      case 'interaction':
        return 'Interaction';
      case 'service_request':
        return 'Service Request';
      default:
        return 'Event';
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return <Alert severity="error">{error}</Alert>;
  }

  if (events.length === 0) {
    return (
      <Alert severity="info">
        No timeline events found for this account. Activities, notes, and interactions will appear here.
      </Alert>
    );
  }

  return (
    <Box sx={{ p: 2 }}>
      <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="body2" color="textSecondary">
          {events.length} event{events.length !== 1 ? 's' : ''} found
        </Typography>
        <Tooltip title="Refresh timeline">
          <IconButton
            size="small"
            onClick={() => {
              loadTimeline();
              onRefresh?.();
            }}
            disabled={loading}
          >
            ↻
          </IconButton>
        </Tooltip>
      </Box>

      <Timeline position="left" sx={{ p: 0 }}>
        {events.map((event, index) => (
          <TimelineItem key={event.id} sx={{ minHeight: 'auto' }}>
            <TimelineSeparator>
              <TimelineDot
                sx={{
                  backgroundColor: event.color,
                  width: 40,
                  height: 40,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  cursor: 'pointer',
                }}
                onClick={() =>
                  setExpandedEventId(expandedEventId === event.id ? null : event.id)
                }
              >
                {getEventIcon(event.type)}
              </TimelineDot>
              {index < events.length - 1 && <TimelineConnector />}
            </TimelineSeparator>

            <TimelineContent sx={{ pb: 2 }}>
              <Card
                variant="outlined"
                sx={{
                  borderLeft: `4px solid ${event.color}`,
                  cursor: 'pointer',
                  transition: 'all 0.2s',
                  '&:hover': {
                    boxShadow: 2,
                  },
                }}
                onClick={() =>
                  setExpandedEventId(expandedEventId === event.id ? null : event.id)
                }
              >
                <CardContent sx={{ pb: 1.5 }}>
                  <Stack direction="row" spacing={1} alignItems="flex-start" justifyContent="space-between">
                    <Stack spacing={0.5} flex={1}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Chip
                          label={getEventTypeLabel(event.type)}
                          size="small"
                          color="primary"
                          variant="outlined"
                        />
                        <Typography variant="caption" color="textSecondary">
                          {formatDate(event.timestamp)}
                        </Typography>
                      </Box>
                      <Typography variant="body2" fontWeight={600}>
                        {event.title}
                      </Typography>
                    </Stack>
                    <IconButton
                      size="small"
                      onClick={(e) => {
                        e.stopPropagation();
                        setExpandedEventId(expandedEventId === event.id ? null : event.id);
                      }}
                    >
                      {expandedEventId === event.id ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                    </IconButton>
                  </Stack>

                  {event.description && (
                    <Typography
                      variant="body2"
                      color="textSecondary"
                      sx={{
                        mt: 1,
                        display: '-webkit-box',
                        WebkitLineClamp: 2,
                        WebkitBoxOrient: 'vertical',
                        overflow: 'hidden',
                        textOverflow: 'ellipsis',
                      }}
                    >
                      {event.description}
                    </Typography>
                  )}
                </CardContent>

                {/* Expanded Details */}
                <Collapse in={expandedEventId === event.id} timeout="auto" unmountOnExit>
                  <Box sx={{ borderTop: '1px solid #e0e0e0', p: 2, backgroundColor: '#fafafa' }}>
                    <Stack spacing={1.5}>
                      {event.description && (
                        <Box>
                          <Typography variant="caption" fontWeight={600} color="textSecondary">
                            Details:
                          </Typography>
                          <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap', mt: 0.5 }}>
                            {event.description}
                          </Typography>
                        </Box>
                      )}

                      {event.details && (
                        <Box>
                          <Typography variant="caption" fontWeight={600} color="textSecondary">
                            Timestamp:
                          </Typography>
                          <Typography variant="body2">
                            {formatDateTime(event.timestamp)}
                          </Typography>
                        </Box>
                      )}

                      {event.type === 'activity' && event.details?.dueDate && (
                        <Box>
                          <Typography variant="caption" fontWeight={600} color="textSecondary">
                            Due Date:
                          </Typography>
                          <Typography variant="body2">
                            {formatDateTime(new Date(event.details.dueDate))}
                          </Typography>
                        </Box>
                      )}

                      {event.type === 'service_request' && event.details && (
                        <Box>
                          <Typography variant="caption" fontWeight={600} color="textSecondary">
                            Status: <Chip label={event.details.status} size="small" sx={{ ml: 1 }} />
                          </Typography>
                        </Box>
                      )}

                      {event.details?.createdBy && (
                        <Box>
                          <Typography variant="caption" fontWeight={600} color="textSecondary">
                            Created by: {event.details.createdBy}
                          </Typography>
                        </Box>
                      )}
                    </Stack>
                  </Box>
                </Collapse>
              </Card>
            </TimelineContent>
          </TimelineItem>
        ))}
      </Timeline>
    </Box>
  );
};

export default AccountTimeline;
