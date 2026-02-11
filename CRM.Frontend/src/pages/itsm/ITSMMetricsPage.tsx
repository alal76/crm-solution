import React, { useEffect, useMemo, useState } from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Grid from '@mui/material/Grid';
import CircularProgress from '@mui/material/CircularProgress';
import Alert from '@mui/material/Alert';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import apiClient from '../../services/apiClient';

interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

interface IncidentSummary {
  incidentId: number;
  number: string;
  shortDescription: string;
  state: number;
  priority: number;
}

interface ProblemSummary {
  problemId: number;
  number: string;
  shortDescription: string;
  state: number;
}

interface ChangeSummary {
  changeId: number;
  number: string;
  shortDescription: string;
  state: number;
  approvalStatus: number;
}

interface KnowledgeSummary {
  articleId: number;
  title: string;
  viewCount: number;
  helpfulCount: number;
}

interface SLAInstance {
  slaInstanceId: number;
  targetId: number;
  targetType: number;
  responseBreached: boolean;
  resolutionBreached: boolean;
}

const ITSMMetricsPage: React.FC = () => {
  const [incidents, setIncidents] = useState<PagedResult<IncidentSummary> | null>(null);
  const [problems, setProblems] = useState<PagedResult<ProblemSummary> | null>(null);
  const [changes, setChanges] = useState<PagedResult<ChangeSummary> | null>(null);
  const [knowledge, setKnowledge] = useState<KnowledgeSummary[]>([]);
  const [breachedSlas, setBreachedSlas] = useState<SLAInstance[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        const [incidentResponse, problemResponse, changeResponse, knowledgeResponse, slaResponse] = await Promise.all([
          apiClient.get<PagedResult<IncidentSummary>>('/api/incidents?pageNumber=1&pageSize=5'),
          apiClient.get<PagedResult<ProblemSummary>>('/api/problems?pageNumber=1&pageSize=5'),
          apiClient.get<PagedResult<ChangeSummary>>('/api/changes?pageNumber=1&pageSize=5'),
          apiClient.get<KnowledgeSummary[]>('/api/knowledge/search?searchTerm='),
          apiClient.get<SLAInstance[]>('/api/sla/breached'),
        ]);

        setIncidents(incidentResponse.data);
        setProblems(problemResponse.data);
        setChanges(changeResponse.data);
        setKnowledge(knowledgeResponse.data ?? []);
        setBreachedSlas(slaResponse.data ?? []);
      } catch (loadError) {
        console.error('Failed to load metrics', loadError);
        setError('Unable to load ITSM metrics.');
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  const knowledgeSummary = useMemo(() => {
    if (!knowledge.length) return { totalViews: 0, helpful: 0, top: [] as KnowledgeSummary[] };
    const totalViews = knowledge.reduce((sum, item) => sum + (item.viewCount ?? 0), 0);
    const helpful = knowledge.reduce((sum, item) => sum + (item.helpfulCount ?? 0), 0);
    const top = [...knowledge].sort((a, b) => (b.viewCount ?? 0) - (a.viewCount ?? 0)).slice(0, 5);
    return { totalViews, helpful, top };
  }, [knowledge]);

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" fontWeight="bold" gutterBottom>ITSM Metrics</Typography>
      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>
      ) : (
      <Grid container spacing={3}>
        <Grid item xs={12} lg={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" fontWeight="bold" gutterBottom>Incident Trends</Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>Total incidents: {incidents?.totalCount ?? 0}</Typography>
            <List dense>
              {(incidents?.items ?? []).map((item) => (
                <ListItem key={item.incidentId} disablePadding>
                  <ListItemText primary={`${item.number} • ${item.shortDescription}`} primaryTypographyProps={{ variant: 'body2' }} />
                </ListItem>
              ))}
            </List>
          </Paper>
        </Grid>
        <Grid item xs={12} lg={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" fontWeight="bold" gutterBottom>SLA Compliance</Typography>
            <Typography variant="body2" color="text.secondary">Breached SLAs: {breachedSlas.length}</Typography>
            <Typography variant="body2" color="text.secondary">Active incidents: {incidents?.totalCount ?? 0}</Typography>
          </Paper>
        </Grid>
        <Grid item xs={12} lg={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" fontWeight="bold" gutterBottom>Change Success Rate</Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>Total changes: {changes?.totalCount ?? 0}</Typography>
            <List dense>
              {(changes?.items ?? []).map((item) => (
                <ListItem key={item.changeId} disablePadding>
                  <ListItemText primary={`${item.number} • ${item.shortDescription}`} primaryTypographyProps={{ variant: 'body2' }} />
                </ListItem>
              ))}
            </List>
          </Paper>
        </Grid>
        <Grid item xs={12} lg={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" fontWeight="bold" gutterBottom>Knowledge Engagement</Typography>
            <Typography variant="body2" color="text.secondary">Total views: {knowledgeSummary.totalViews}</Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>Helpful votes: {knowledgeSummary.helpful}</Typography>
            <List dense>
              {knowledgeSummary.top.map((item) => (
                <ListItem key={item.articleId} disablePadding>
                  <ListItemText primary={`${item.title} • ${item.viewCount} views`} primaryTypographyProps={{ variant: 'body2' }} />
                </ListItem>
              ))}
            </List>
          </Paper>
        </Grid>
      </Grid>
      )}
      {error && <Alert severity="error" sx={{ mt: 2 }}>{error}</Alert>}
    </Box>
  );
};

export default ITSMMetricsPage;
