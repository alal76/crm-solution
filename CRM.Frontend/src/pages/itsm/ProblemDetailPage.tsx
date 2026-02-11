import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Box,
  Typography,
  Button,
  Paper,
  Grid,
  CircularProgress,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import apiClient from '../../services/apiClient';
import { RootCauseAnalysisTemplate, RelatedIncidentsWidget } from '../../components/itsm';
import type { RelatedIncident } from '../../components/itsm';

interface ProblemDetail {
  problemId: number;
  number: string;
  shortDescription: string;
  description?: string;
  state: number;
  priority: number;
  rootCause?: string;
  workaround?: string;
  knownError?: boolean;
}

const ProblemDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [problem, setProblem] = useState<ProblemDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [relatedIncidents, setRelatedIncidents] = useState<RelatedIncident[]>([]);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await apiClient.get(`/problems/${id}`);
        setProblem(response.data);
        // Load related incidents (best-effort)
        try {
          const relResp = await apiClient.get(`/problems/${id}/incidents`);
          setRelatedIncidents(relResp.data ?? []);
        } catch { /* non-critical */ }
      } catch (error) {
        console.error('Failed to load problem', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [id]);

  if (loading) return <Box sx={{ p: 3 }}><CircularProgress /></Box>;
  if (!problem) return <Box sx={{ p: 3 }}><Typography>Problem not found</Typography></Box>;

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h4" component="h1" fontWeight="bold">
            {problem.number}
          </Typography>
          <Typography color="text.secondary">{problem.shortDescription}</Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<EditIcon />}
          onClick={() => navigate(`/itsm/problems/${problem.problemId}/edit`)}
        >
          Edit
        </Button>
      </Box>

      <Paper sx={{ p: 3 }}>
        <Box sx={{ mb: 3 }}>
          <Typography variant="subtitle2" color="text.secondary">Description</Typography>
          <Typography sx={{ whiteSpace: 'pre-wrap' }}>{problem.description || '—'}</Typography>
        </Box>
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid item xs={12} md={6}>
            <Typography variant="subtitle2" color="text.secondary">Priority</Typography>
            <Typography>P{problem.priority}</Typography>
          </Grid>
          <Grid item xs={12} md={6}>
            <Typography variant="subtitle2" color="text.secondary">State</Typography>
            <Typography>State {problem.state}</Typography>
          </Grid>
        </Grid>
        <Box sx={{ mb: 3 }}>
          <Typography variant="subtitle2" color="text.secondary">Root Cause</Typography>
          <Typography sx={{ whiteSpace: 'pre-wrap' }}>{problem.rootCause || '—'}</Typography>
        </Box>
        <Box sx={{ mb: 3 }}>
          <Typography variant="subtitle2" color="text.secondary">Workaround</Typography>
          <Typography sx={{ whiteSpace: 'pre-wrap' }}>{problem.workaround || '—'}</Typography>
        </Box>
        <Box>
          <Typography variant="subtitle2" color="text.secondary">Known Error</Typography>
          <Typography>{problem.knownError ? 'Yes' : 'No'}</Typography>
        </Box>
      </Paper>

      {/* Root Cause Analysis */}
      <Box sx={{ mt: 3 }}>
        <RootCauseAnalysisTemplate
          problemDescription={problem.shortDescription}
          readOnly={false}
        />
      </Box>

      {/* Related Incidents */}
      <Box sx={{ mt: 3 }}>
        <RelatedIncidentsWidget
          problemId={Number(id)}
          incidents={relatedIncidents}
        />
      </Box>
    </Box>
  );
};

export default ProblemDetailPage;
