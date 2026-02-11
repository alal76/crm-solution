import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Button,
  Paper,
  Grid,
  CircularProgress,
  Alert,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import apiClient from '../../services/apiClient';
import {
  IncidentTimeline,
  SLACountdownWidget,
  SLABreachAlert,
  RelatedIncidentsWidget,
  ArticleSuggestions,
} from '../../components/itsm';
import type {
  TimelineActivity,
  SLAInstanceData,
  SLABreachInfo,
  RelatedIncident,
} from '../../components/itsm';

interface Incident {
  incidentId: number;
  number: string;
  shortDescription: string;
  description: string;
  state: number;
  priority: number;
  callerName: string;
  assignedToName?: string;
  createdAt: string;
}

export const IncidentDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [incident, setIncident] = useState<Incident | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [timelineActivities, setTimelineActivities] = useState<TimelineActivity[]>([]);
  const [slaInstances, setSlaInstances] = useState<SLAInstanceData[]>([]);
  const [slaBreaches, setSlaBreaches] = useState<SLABreachInfo[]>([]);
  const [relatedIncidents, setRelatedIncidents] = useState<RelatedIncident[]>([]);

  useEffect(() => {
    const loadIncident = async () => {
      try {
        const response = await apiClient.get(`/incidents/${id}`);
        setIncident(response.data);
        // Load supplementary data for ITSM components (best-effort)
        const [timelineResp, slaResp, breachResp, relatedResp] = await Promise.allSettled([
          apiClient.get(`/incidents/${id}/timeline`),
          apiClient.get(`/incidents/${id}/sla`),
          apiClient.get(`/incidents/${id}/sla/breaches`),
          apiClient.get(`/incidents/${id}/related`),
        ]);
        if (timelineResp.status === 'fulfilled') setTimelineActivities(timelineResp.value.data ?? []);
        if (slaResp.status === 'fulfilled') setSlaInstances(slaResp.value.data ?? []);
        if (breachResp.status === 'fulfilled') setSlaBreaches(breachResp.value.data ?? []);
        if (relatedResp.status === 'fulfilled') setRelatedIncidents(relatedResp.value.data ?? []);
      } catch (err) {
        setError('Failed to load incident');
      } finally {
        setLoading(false);
      }
    };

    loadIncident();
  }, [id]);

  if (loading) return <Box sx={{ p: 3 }}><CircularProgress /></Box>;
  if (error) return <Box sx={{ p: 3 }}><Alert severity="error">{error}</Alert></Box>;
  if (!incident) return <Box sx={{ p: 3 }}><Typography>Incident not found</Typography></Box>;

  return (
    <Box sx={{ p: 3, maxWidth: 960, mx: 'auto' }}>
      <Paper sx={{ p: 3 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 3 }}>
          <Box>
            <Typography variant="h4" component="h1" fontWeight="bold">
              {incident.number}
            </Typography>
            <Typography color="text.secondary" sx={{ mt: 1 }}>
              {incident.shortDescription}
            </Typography>
          </Box>
          <Button
            variant="contained"
            startIcon={<ArrowBackIcon />}
            onClick={() => navigate('/incidents')}
          >
            Back
          </Button>
        </Box>

        <Grid container spacing={2} sx={{ mb: 3, pt: 2, borderTop: 1, borderColor: 'divider' }}>
          <Grid item xs={6}>
            <Typography variant="subtitle2" color="text.secondary">Caller</Typography>
            <Typography variant="body1">{incident.callerName}</Typography>
          </Grid>
          <Grid item xs={6}>
            <Typography variant="subtitle2" color="text.secondary">Priority</Typography>
            <Typography variant="body1">Priority {incident.priority}</Typography>
          </Grid>
          <Grid item xs={6}>
            <Typography variant="subtitle2" color="text.secondary">State</Typography>
            <Typography variant="body1">State {incident.state}</Typography>
          </Grid>
          <Grid item xs={6}>
            <Typography variant="subtitle2" color="text.secondary">Assigned To</Typography>
            <Typography variant="body1">{incident.assignedToName || 'Unassigned'}</Typography>
          </Grid>
        </Grid>

        <Box sx={{ pt: 2, borderTop: 1, borderColor: 'divider' }}>
          <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1 }}>Description</Typography>
          <Typography sx={{ whiteSpace: 'pre-wrap' }}>{incident.description}</Typography>
        </Box>

        <Box sx={{ mt: 3, display: 'flex', gap: 2 }}>
          <Button variant="contained" color="success">Resolve</Button>
          <Button variant="contained" color="warning">Escalate</Button>
          <Button variant="contained" color="inherit">Close</Button>
        </Box>
      </Paper>

      {/* SLA Breach Alerts */}
      {slaBreaches.length > 0 && (
        <Box sx={{ mt: 3 }}>
          {slaBreaches.map((breach) => (
            <Box key={breach.id} sx={{ mb: 1 }}>
              <SLABreachAlert breach={breach} variant="inline" />
            </Box>
          ))}
        </Box>
      )}

      {/* SLA Countdown */}
      {slaInstances.length > 0 && (
        <Box sx={{ mt: 3 }}>
          <SLACountdownWidget slaInstances={slaInstances} showDetails />
        </Box>
      )}

      {/* Related Incidents */}
      <Box sx={{ mt: 3 }}>
        <RelatedIncidentsWidget
          problemId={Number(id)}
          incidents={relatedIncidents}
          readOnly
        />
      </Box>

      {/* Knowledge Article Suggestions */}
      <Box sx={{ mt: 3 }}>
        <ArticleSuggestions
          incidentDescription={incident.shortDescription}
          autoSuggest
        />
      </Box>

      {/* Incident Timeline */}
      <Box sx={{ mt: 3 }}>
        <IncidentTimeline
          activities={timelineActivities}
          showFilters
        />
      </Box>
    </Box>
  );
};

export default IncidentDetailPage;
