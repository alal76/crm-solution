/**
 * ServiceRequestDetailPage
 * Displays detailed view of a service request/ticket with timeline, SLA status, and resolution form
 * Priority: P0
 */

import React, { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  CircularProgress,
  Alert,
  Chip,
  Grid,
  Paper,
  Divider,
  Stack,
  Rating,
  FormControlLabel,
  Checkbox
} from '@mui/material';
import { useParams, useNavigate } from 'react-router-dom';
import itsmService from '../services/itsmService';
import { Incident, IncidentStatus, IncidentPriority } from '../types/itsm';
import { RecordComments } from '../components/common/RecordComments';

/**
 * Service Request Detail Page Component
 */
export const ServiceRequestDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const [incident, setIncident] = useState<Incident | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [resolutionDialogOpen, setResolutionDialogOpen] = useState(false);
  const [feedbackDialogOpen, setFeedbackDialogOpen] = useState(false);
  const [resolution, setResolution] = useState('');
  const [satisfactionRating, setSatisfactionRating] = useState(5);
  const [satisfactionComment, setSatisfactionComment] = useState('');
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    const loadIncident = async () => {
      if (!id) return;
      
      try {
        setLoading(true);
        const response = await itsmService.getIncidentById(parseInt(id, 10));
        setIncident(response.data);
      } catch (err) {
        setError('Failed to load service request');
        console.error('Error loading incident:', err);
      } finally {
        setLoading(false);
      }
    };

    loadIncident();
  }, [id]);

  const handleResolve = async () => {
    if (!incident) return;
    
    try {
      setSubmitting(true);
      await itsmService.resolveIncident(incident.id, resolution);
      setResolutionDialogOpen(false);
      // Reload incident
      const response = await itsmService.getIncidentById(incident.id);
      setIncident(response.data);
    } catch (err) {
      setError('Failed to resolve incident');
      console.error('Error resolving incident:', err);
    } finally {
      setSubmitting(false);
    }
  };

  const handleClose = async () => {
    if (!incident) return;
    
    try {
      setSubmitting(true);
      await itsmService.closeIncident(incident.id);
      // Reload incident
      const response = await itsmService.getIncidentById(incident.id);
      setIncident(response.data);
    } catch (err) {
      setError('Failed to close incident');
      console.error('Error closing incident:', err);
    } finally {
      setSubmitting(false);
    }
  };

  const handleMarkAsSatisfied = async () => {
    if (!incident) return;
    
    try {
      setSubmitting(true);
      // This would be an API call to save satisfaction feedback
      // await itsmService.submitSatisfactionFeedback(incident.id, satisfactionRating, satisfactionComment);
      setFeedbackDialogOpen(false);
      // Reload incident
      const response = await itsmService.getIncidentById(incident.id);
      setIncident(response.data);
    } catch (err) {
      setError('Failed to submit feedback');
      console.error('Error submitting feedback:', err);
    } finally {
      setSubmitting(false);
    }
  };

  const getPriorityColor = (priority: IncidentPriority) => {
    switch (priority) {
      case IncidentPriority.Critical:
        return 'error';
      case IncidentPriority.High:
        return 'warning';
      case IncidentPriority.Medium:
        return 'info';
      case IncidentPriority.Low:
        return 'success';
      default:
        return 'default';
    }
  };

  const getStatusColor = (status: IncidentStatus) => {
    switch (status) {
      case IncidentStatus.Closed:
        return 'success';
      case IncidentStatus.Resolved:
        return 'success';
      case IncidentStatus.OnHold:
        return 'warning';
      case IncidentStatus.New:
        return 'info';
      default:
        return 'default';
    }
  };

  if (loading) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 400 }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (!incident) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Alert severity="error">Service request not found</Alert>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {/* Header */}
      <Box sx={{ mb: 4, display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
        <Box>
          <Typography variant="h4" gutterBottom>
            {incident.title}
          </Typography>
          <Typography color="textSecondary" gutterBottom>
            Request #{incident.number}
          </Typography>
        </Box>
        <Stack direction="row" spacing={1}>
          <Chip
            label={incident.status}
            color={getStatusColor(incident.status) as any}
            variant="outlined"
          />
          <Chip
            label={IncidentPriority[incident.priority]}
            color={getPriorityColor(incident.priority) as any}
            variant="outlined"
          />
        </Stack>
      </Box>

      <Grid container spacing={3}>
        {/* Main Content */}
        <Grid item xs={12} md={8}>
          {/* Description */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>Description</Typography>
              <Typography>{incident.description}</Typography>
            </CardContent>
          </Card>

          {/* Timeline */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>Timeline</Typography>
              <ServiceRequestTimeline incidentId={incident.id} />
            </CardContent>
          </Card>

          {/* Resolution */}
          {incident.resolution && (
            <Card sx={{ mb: 3 }}>
              <CardContent>
                <Typography variant="h6" gutterBottom>Resolution</Typography>
                <Typography>{incident.resolution}</Typography>
              </CardContent>
            </Card>
          )}
        </Grid>

        {/* Sidebar */}
        <Grid item xs={12} md={4}>
          {/* SLA Status */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>SLA Status</Typography>
              <SLAStatusBadge incidentId={incident.id} />
            </CardContent>
          </Card>

          {/* Assignment */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>Assignment</Typography>
              <AssignmentPanel incidentId={incident.id} assignedTo={incident.assignedTo} />
            </CardContent>
          </Card>

          {/* Actions */}
          <Card>
            <CardContent>
              <Stack spacing={2}>
                {incident.status !== IncidentStatus.Resolved && (
                  <Button
                    variant="contained"
                    fullWidth
                    onClick={() => setResolutionDialogOpen(true)}
                  >
                    Mark as Resolved
                  </Button>
                )}
                {incident.status === IncidentStatus.Resolved && (
                  <Button
                    variant="contained"
                    color="success"
                    fullWidth
                    onClick={handleClose}
                  >
                    Close Request
                  </Button>
                )}
                {incident.status === IncidentStatus.Resolved && (
                  <Button
                    variant="outlined"
                    fullWidth
                    onClick={() => setFeedbackDialogOpen(true)}
                  >
                    Provide Feedback
                  </Button>
                )}
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Comments Section */}
      {incident && (
        <Box sx={{ mt: 4 }}>
          <RecordComments entityType="ServiceRequest" entityId={incident.id} />
        </Box>
      )}

      {/* Resolution Dialog */}
      <Dialog open={resolutionDialogOpen} onClose={() => setResolutionDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Resolve Request</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <TextField
              fullWidth
              multiline
              rows={4}
              label="Resolution Description"
              value={resolution}
              onChange={(e) => setResolution(e.target.value)}
              placeholder="Describe how the issue was resolved..."
              variant="outlined"
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setResolutionDialogOpen(false)}>Cancel</Button>
          <Button
            onClick={handleResolve}
            variant="contained"
            disabled={submitting || !resolution.trim()}
          >
            {submitting ? <CircularProgress size={24} /> : 'Resolve'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Feedback Dialog */}
      <Dialog open={feedbackDialogOpen} onClose={() => setFeedbackDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Customer Satisfaction</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Typography gutterBottom>How satisfied are you with this resolution?</Typography>
            <Rating
              size="large"
              value={satisfactionRating}
              onChange={(_, value) => setSatisfactionRating(value || 5)}
              sx={{ mb: 2 }}
            />
            <TextField
              fullWidth
              multiline
              rows={3}
              label="Additional Comments"
              value={satisfactionComment}
              onChange={(e) => setSatisfactionComment(e.target.value)}
              variant="outlined"
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setFeedbackDialogOpen(false)}>Cancel</Button>
          <Button
            onClick={handleMarkAsSatisfied}
            variant="contained"
            disabled={submitting}
          >
            {submitting ? <CircularProgress size={24} /> : 'Submit'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

/**
 * Service Request Timeline Component
 */
const ServiceRequestTimeline: React.FC<{ incidentId: number }> = ({ incidentId }) => {
  const [timeline, setTimeline] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadTimeline = async () => {
      try {
        const response = await itsmService.getIncidentTimeline(incidentId);
        setTimeline(response.data);
      } catch (err) {
        console.error('Error loading timeline:', err);
      } finally {
        setLoading(false);
      }
    };

    loadTimeline();
  }, [incidentId]);

  if (loading) return <CircularProgress />;

  return (
    <Stack spacing={2}>
      {timeline.length === 0 ? (
        <Typography color="textSecondary">No activity yet</Typography>
      ) : (
        timeline.map((item, index) => (
          <Paper key={index} sx={{ p: 2, bgcolor: 'background.default' }}>
            <Typography variant="caption" color="textSecondary">
              {new Date(item.createdAt).toLocaleString()}
            </Typography>
            <Typography>{item.description}</Typography>
          </Paper>
        ))
      )}
    </Stack>
  );
};

/**
 * SLA Status Badge Component
 */
const SLAStatusBadge: React.FC<{ incidentId: number }> = ({ incidentId }) => {
  const [slaStatus, setSLAStatus] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadSLAStatus = async () => {
      try {
        const response = await itsmService.getIncidentSLAStatus(incidentId);
        setSLAStatus(response.data);
      } catch (err) {
        console.error('Error loading SLA status:', err);
      } finally {
        setLoading(false);
      }
    };

    loadSLAStatus();
  }, [incidentId]);

  if (loading) return <CircularProgress size={24} />;
  if (!slaStatus) return <Typography color="textSecondary">No SLA</Typography>;

  return (
    <Stack spacing={1}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="body2">Response Time</Typography>
        <Chip
          size="small"
          label={slaStatus.responseBreached ? 'BREACHED' : 'ON TRACK'}
          color={slaStatus.responseBreached ? 'error' : 'success'}
        />
      </Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="body2">Resolution Time</Typography>
        <Chip
          size="small"
          label={slaStatus.resolutionBreached ? 'BREACHED' : 'ON TRACK'}
          color={slaStatus.resolutionBreached ? 'error' : 'success'}
        />
      </Box>
    </Stack>
  );
};

/**
 * Assignment Panel Component
 */
const AssignmentPanel: React.FC<{ incidentId: number; assignedTo?: number }> = ({ incidentId, assignedTo }) => {
  const [assignedToUser, setAssignedToUser] = useState<string | null>(null);

  useEffect(() => {
    // In a real application, fetch user details
    if (assignedTo) {
      setAssignedToUser(`User #${assignedTo}`);
    }
  }, [assignedTo]);

  return (
    <Box>
      {assignedToUser ? (
        <Chip label={assignedToUser} color="primary" />
      ) : (
        <Typography color="textSecondary">Unassigned</Typography>
      )}
    </Box>
  );
};

export default ServiceRequestDetailPage;
