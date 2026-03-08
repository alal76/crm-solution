import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import Paper from '@mui/material/Paper';
import Grid from '@mui/material/Grid';
import CircularProgress from '@mui/material/CircularProgress';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import TextField from '@mui/material/TextField';
import Alert from '@mui/material/Alert';
import UndoIcon from '@mui/icons-material/Undo';
import changeService from '../../services/changeService';
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

  // Rollback execution state
  const [rollbackOpen, setRollbackOpen] = useState(false);
  const [rollbackReason, setRollbackReason] = useState('');
  const [rollbackSubmitting, setRollbackSubmitting] = useState(false);
  const [rollbackError, setRollbackError] = useState<string | null>(null);
  const [rollbackSuccess, setRollbackSuccess] = useState(false);

  const handleRollback = async () => {
    if (!rollbackReason.trim() || !id) return;
    setRollbackSubmitting(true);
    setRollbackError(null);
    try {
      await changeService.rollbackChange(Number(id), rollbackReason.trim());
      setRollbackSuccess(true);
      setRollbackOpen(false);
      setRollbackReason('');
      // Reload change data
      const response = await apiClient.get(`/changes/${id}`);
      setChange(response.data);
    } catch (err: unknown) {
      setRollbackError((err as any)?.response?.data?.message || 'Failed to execute rollback');
    } finally {
      setRollbackSubmitting(false);
    }
  };

  // Rollback is available when change is in InProgress (6), Completed (8), or OnHold (7) state
  const canRollback = change && [6, 7, 8].includes(change.state);

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
          {canRollback && (
            <Button
              variant="outlined"
              color="warning"
              startIcon={<UndoIcon />}
              onClick={() => setRollbackOpen(true)}
            >
              Rollback
            </Button>
          )}
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

      {/* Rollback Success Alert */}
      {rollbackSuccess && (
        <Alert severity="success" sx={{ mt: 2 }} onClose={() => setRollbackSuccess(false)}>
          Change rolled back successfully.
        </Alert>
      )}

      {/* Rollback Execution Dialog */}
      <Dialog open={rollbackOpen} onClose={() => setRollbackOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Execute Rollback — {change.number}</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            This will execute the backout plan and mark the change as rolled back. This action cannot be undone.
          </Typography>
          {rollbackError && (
            <Alert severity="error" sx={{ mb: 2 }}>{rollbackError}</Alert>
          )}
          <TextField
            autoFocus
            fullWidth
            label="Rollback Reason"
            value={rollbackReason}
            onChange={(e) => setRollbackReason(e.target.value)}
            multiline
            rows={3}
            placeholder="Describe why this change needs to be rolled back..."
            required
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRollbackOpen(false)} disabled={rollbackSubmitting}>Cancel</Button>
          <Button
            onClick={handleRollback}
            variant="contained"
            color="warning"
            disabled={rollbackSubmitting || !rollbackReason.trim()}
          >
            {rollbackSubmitting ? 'Executing...' : 'Execute Rollback'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default ChangeDetailPage;
