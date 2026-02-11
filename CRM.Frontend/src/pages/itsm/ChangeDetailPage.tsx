import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import Paper from '@mui/material/Paper';
import Grid from '@mui/material/Grid';
import CircularProgress from '@mui/material/CircularProgress';
import apiClient from '../../services/apiClient';
import {
  ApprovalWorkflowPanel,
  RiskAssessmentForm,
  ChangeConflictDetector,
} from '../../components/itsm';
import type { ApprovalStep, ChangeConflict } from '../../components/itsm';

interface ChangeDetail {
  changeId: number;
  number: string;
  shortDescription: string;
  state: number;
  approvalStatus: number;
  plannedStartDate?: string;
  plannedEndDate?: string;
  requestorName?: string;
}

const ChangeDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [change, setChange] = useState<ChangeDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [approvalSteps, setApprovalSteps] = useState<ApprovalStep[]>([]);
  const [conflicts, setConflicts] = useState<ChangeConflict[]>([]);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await apiClient.get(`/changes/${id}`);
        setChange(response.data);
        const [approvalResp, conflictResp] = await Promise.allSettled([
          apiClient.get(`/changes/${id}/approvals`),
          apiClient.get(`/changes/${id}/conflicts`),
        ]);
        if (approvalResp.status === 'fulfilled') setApprovalSteps(approvalResp.value.data ?? []);
        if (conflictResp.status === 'fulfilled') setConflicts(conflictResp.value.data ?? []);
      } catch (error) {
        console.error('Failed to load change', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [id]);

  if (loading) return <Box sx={{ p: 3, display: 'flex', justifyContent: 'center' }}><CircularProgress /></Box>;
  if (!change) return <Box sx={{ p: 3 }}><Typography color="text.secondary">Change not found</Typography></Box>;

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h4" component="h1" fontWeight="bold">{change.number}</Typography>
          <Typography color="text.secondary">{change.shortDescription}</Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button variant="outlined" onClick={() => navigate(`/itsm/changes/${change.changeId}/approval`)}>
            Approvals
          </Button>
          <Button variant="contained" onClick={() => navigate(`/itsm/changes/${change.changeId}/edit`)}>
            Edit
          </Button>
        </Box>
      </Box>

      <Paper sx={{ p: 3 }}>
        <Grid container spacing={2} sx={{ mb: 2 }}>
          <Grid item xs={12} md={4}>
            <Typography variant="subtitle2" color="text.secondary">State</Typography>
            <Typography>State {change.state}</Typography>
          </Grid>
          <Grid item xs={12} md={4}>
            <Typography variant="subtitle2" color="text.secondary">Approval</Typography>
            <Typography>Status {change.approvalStatus}</Typography>
          </Grid>
          <Grid item xs={12} md={4}>
            <Typography variant="subtitle2" color="text.secondary">Requestor</Typography>
            <Typography>{change.requestorName || '—'}</Typography>
          </Grid>
        </Grid>
        <Box>
          <Typography variant="subtitle2" color="text.secondary">Planned Window</Typography>
          <Typography>
            {change.plannedStartDate ? new Date(change.plannedStartDate).toLocaleString() : '—'}
            {' '}→{' '}
            {change.plannedEndDate ? new Date(change.plannedEndDate).toLocaleString() : '—'}
          </Typography>
        </Box>
      </Paper>

      {/* Approval Workflow */}
      <Box sx={{ mt: 3 }}>
        <ApprovalWorkflowPanel
          steps={approvalSteps}
          currentUserId={0}
          title={`Approvals for ${change.number}`}
        />
      </Box>

      {/* Risk Assessment */}
      <Box sx={{ mt: 3 }}>
        <RiskAssessmentForm
          changeRequestId={change.changeId}
        />
      </Box>

      {/* Change Conflict Detection */}
      {conflicts.length > 0 && (
        <Box sx={{ mt: 3 }}>
          <ChangeConflictDetector
            currentChange={{
              id: change.changeId,
              changeNumber: change.number,
              title: change.shortDescription,
              scheduledStart: change.plannedStartDate ?? new Date().toISOString(),
              scheduledEnd: change.plannedEndDate ?? new Date().toISOString(),
              affectedCIs: [],
              assignedTo: change.requestorName ?? '',
            }}
            conflicts={conflicts}
          />
        </Box>
      )}
    </Box>
  );
};

export default ChangeDetailPage;
