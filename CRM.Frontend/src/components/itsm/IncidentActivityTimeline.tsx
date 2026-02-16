/**
 * IncidentActivityTimeline - Timeline view of incident activities and comments
 */

import React from 'react';
import {
  Box,
  Typography,
  Paper,
  Stack,
  Chip,
  Avatar,
  AvatarGroup,
  Divider,
  Button,
  TextField,
  CircularProgress,
  Alert,
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
  Comment as CommentIcon,
  Edit as EditIcon,
  Assignment as AssignmentIcon,
  Attachment as AttachmentIcon,
  Info as InfoIcon,
} from '@mui/icons-material';
import { IncidentActivity } from '../../services/incidentService';

interface IncidentActivityTimelineProps {
  activities: IncidentActivity[];
  loading?: boolean;
  onAddComment?: (content: string) => Promise<void>;
  canComment?: boolean;
}

export const IncidentActivityTimeline: React.FC<IncidentActivityTimelineProps> = ({
  activities,
  loading = false,
  onAddComment,
  canComment = true,
}) => {
  const [newComment, setNewComment] = React.useState('');
  const [submitting, setSubmitting] = React.useState(false);
  const [submitError, setSubmitError] = React.useState<string | null>(null);

  const handleSubmitComment = async () => {
    if (!newComment.trim() || !onAddComment) return;

    setSubmitting(true);
    setSubmitError(null);

    try {
      await onAddComment(newComment);
      setNewComment('');
    } catch (err) {
      setSubmitError(err instanceof Error ? err.message : 'Failed to add comment');
    } finally {
      setSubmitting(false);
    }
  };

  const getActivityIcon = (type: IncidentActivity['type']) => {
    const icons: Record<IncidentActivity['type'], React.ReactNode> = {
      'comment': <CommentIcon />,
      'status_change': <EditIcon />,
      'assignment': <AssignmentIcon />,
      'attachment': <AttachmentIcon />,
      'assignment_group': <AssignmentIcon />,
    };
    return icons[type];
  };

  const getActivityLabel = (type: IncidentActivity['type']) => {
    const labels: Record<IncidentActivity['type'], string> = {
      'comment': 'Comment',
      'status_change': 'Status Changed',
      'assignment': 'Assigned',
      'attachment': 'Attachment',
      'assignment_group': 'Group Assignment',
    };
    return labels[type];
  };

  const getActivityColor = (type: IncidentActivity['type']): 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' => {
    const colors: Record<IncidentActivity['type'], 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning'> = {
      'comment': 'primary',
      'status_change': 'info',
      'assignment': 'success',
      'attachment': 'warning',
      'assignment_group': 'success',
    };
    return colors[type];
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Timeline position="alternate">
        {activities.map((activity, index) => (
          <TimelineItem key={activity.id}>
            <TimelineOppositeContent color="text.secondary" sx={{ maxWidth: 150 }}>
              <Typography variant="caption">
                {new Date(activity.timestamp).toLocaleDateString()}
              </Typography>
              <Typography variant="caption" display="block">
                {new Date(activity.timestamp).toLocaleTimeString()}
              </Typography>
              <Typography variant="body2" sx={{ mt: 0.5 }}>
                {activity.userName}
              </Typography>
            </TimelineOppositeContent>
            <TimelineSeparator>
              <TimelineDot color={getActivityColor(activity.type)}>
                {getActivityIcon(activity.type)}
              </TimelineDot>
              {index < activities.length - 1 && <TimelineConnector />}
            </TimelineSeparator>
            <TimelineContent sx={{ pb: 3 }}>
              <Paper elevation={0} sx={{ p: 2, bgcolor: 'background.default' }}>
                <Chip
                  size="small"
                  label={getActivityLabel(activity.type)}
                  color={getActivityColor(activity.type)}
                  variant="outlined"
                  sx={{ mb: 1 }}
                />
                <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap', mt: 1 }}>
                  {activity.content}
                </Typography>
                {activity.metadata && (
                  <Box sx={{ mt: 1, fontSize: '0.875rem', color: 'text.secondary' }}>
                    {Object.entries(activity.metadata).map(([key, value]) => (
                      <div key={key}>
                        <strong>{key}:</strong> {String(value)}
                      </div>
                    ))}
                  </Box>
                )}
              </Paper>
            </TimelineContent>
          </TimelineItem>
        ))}
      </Timeline>

      {activities.length === 0 && !canComment && (
        <Alert severity="info" sx={{ mt: 2 }}>
          No activity yet
        </Alert>
      )}

      {canComment && (
        <Box sx={{ mt: 3, pt: 2, borderTop: 1, borderColor: 'divider' }}>
          <Typography variant="subtitle2" sx={{ mb: 1 }}>
            Add Comment
          </Typography>
          {submitError && <Alert severity="error" sx={{ mb: 1 }}>{submitError}</Alert>}
          <Stack spacing={1}>
            <TextField
              multiline
              rows={3}
              placeholder="Add a comment..."
              value={newComment}
              onChange={(e) => setNewComment(e.target.value)}
              disabled={submitting}
              fullWidth
            />
            <Button
              variant="contained"
              onClick={handleSubmitComment}
              disabled={!newComment.trim() || submitting || !onAddComment}
            >
              {submitting ? 'Adding...' : 'Add Comment'}
            </Button>
          </Stack>
        </Box>
      )}
    </Box>
  );
};

export default IncidentActivityTimeline;
