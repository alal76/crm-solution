/**
 * ChangeManagementPage
 * Displays list and details of ITSM changes with CAB voting panel and impact analysis
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
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tabs,
  Tab,
  Stack,
  List,
  ListItem,
  ListItemText,
  FormControl,
  InputLabel,
  Select,
  MenuItem
} from '@mui/material';
import { useParams, useNavigate } from 'react-router-dom';
import itsmService from '../../services/itsmService';
import {
  Change,
  ChangeStatus,
  ChangePriority,
  ChangeRiskLevel,
  CreateChangeDto,
  CABVote
} from '../../types/itsm';

/**
 * Change Management Page Component
 */
export const ChangeManagementPage: React.FC = () => {
  const [changes, setChanges] = useState<Change[]>([]);
  const [selectedChange, setSelectedChange] = useState<Change | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const [tabValue, setTabValue] = useState(0);
  const [filterStatus, setFilterStatus] = useState<string>('all');

  useEffect(() => {
    loadChanges();
  }, [filterStatus]);

  const loadChanges = async () => {
    try {
      setLoading(true);
      const response = await itsmService.getChanges();
      setChanges(response.data.items);
    } catch (err) {
      setError('Failed to load changes');
      console.error('Error loading changes:', err);
    } finally {
      setLoading(false);
    }
  };

  const getRiskColor = (risk: ChangeRiskLevel) => {
    switch (risk) {
      case ChangeRiskLevel.VeryHigh:
        return 'error';
      case ChangeRiskLevel.High:
        return 'warning';
      case ChangeRiskLevel.Medium:
        return 'info';
      case ChangeRiskLevel.Low:
        return 'success';
      default:
        return 'default';
    }
  };

  const getStatusColor = (status: ChangeStatus) => {
    switch (status) {
      case ChangeStatus.Completed:
        return 'success';
      case ChangeStatus.InProgress:
        return 'info';
      case ChangeStatus.Scheduled:
        return 'warning';
      case ChangeStatus.Rejected:
        return 'error';
      default:
        return 'default';
    }
  };

  const filteredChanges = filterStatus === 'all'
    ? changes
    : changes.filter(c => c.status === filterStatus);

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {/* Header */}
      <Box sx={{ mb: 4, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="h4">Change Management</Typography>
        <Button
          variant="contained"
          onClick={() => setCreateDialogOpen(true)}
        >
          New Change Request
        </Button>
      </Box>

      {/* Filter */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <FormControl sx={{ minWidth: 200 }}>
            <InputLabel>Status</InputLabel>
            <Select
              value={filterStatus}
              onChange={(e) => setFilterStatus(e.target.value)}
              label="Status"
            >
              <MenuItem value="all">All</MenuItem>
              <MenuItem value={ChangeStatus.Draft}>Draft</MenuItem>
              <MenuItem value={ChangeStatus.Scheduled}>Scheduled</MenuItem>
              <MenuItem value={ChangeStatus.InProgress}>In Progress</MenuItem>
              <MenuItem value={ChangeStatus.Completed}>Completed</MenuItem>
            </Select>
          </FormControl>
        </CardContent>
      </Card>

      {/* Changes List */}
      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
          <CircularProgress />
        </Box>
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow sx={{ bgcolor: 'background.default' }}>
                <TableCell>Number</TableCell>
                <TableCell>Title</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Priority</TableCell>
                <TableCell>Risk</TableCell>
                <TableCell>Start Date</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredChanges.map((change) => (
                <TableRow key={change.id} hover>
                  <TableCell>{change.number}</TableCell>
                  <TableCell>{change.title}</TableCell>
                  <TableCell>
                    <Chip
                      label={change.status}
                      size="small"
                      color={getStatusColor(change.status) as any}
                    />
                  </TableCell>
                  <TableCell>{ChangePriority[change.priority]}</TableCell>
                  <TableCell>
                    <Chip
                      label={ChangeRiskLevel[change.riskLevel]}
                      size="small"
                      color={getRiskColor(change.riskLevel) as any}
                    />
                  </TableCell>
                  <TableCell>{new Date(change.startDate).toLocaleDateString()}</TableCell>
                  <TableCell>
                    <Button
                      size="small"
                      onClick={() => setSelectedChange(change)}
                    >
                      View
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {/* Change Detail Dialog */}
      {selectedChange && (
        <ChangeDetailDialog
          change={selectedChange}
          open={!!selectedChange}
          onClose={() => setSelectedChange(null)}
          onReload={loadChanges}
        />
      )}

      {/* Create Change Dialog */}
      <CreateChangeDialog
        open={createDialogOpen}
        onClose={() => setCreateDialogOpen(false)}
        onSuccess={() => {
          setCreateDialogOpen(false);
          loadChanges();
        }}
      />
    </Container>
  );
};

/**
 * Change Detail Dialog Component
 */
const ChangeDetailDialog: React.FC<{
  change: Change;
  open: boolean;
  onClose: () => void;
  onReload: () => void;
}> = ({ change, open, onClose, onReload }) => {
  const [tabValue, setTabValue] = useState(0);
  const [cabVotes, setCABVotes] = useState<CABVote[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (open) {
      loadCABVotes();
    }
  }, [open, change.id]);

  const loadCABVotes = async () => {
    try {
      const response = await itsmService.getCABVotes(change.id);
      setCABVotes(response.data);
    } catch (err) {
      console.error('Error loading CAB votes:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleApproveChange = async () => {
    try {
      await itsmService.approveChange(change.id, 1); // Assuming current user ID is 1
      onReload();
      onClose();
    } catch (err) {
      console.error('Error approving change:', err);
    }
  };

  const handleRejectChange = async () => {
    try {
      await itsmService.rejectChange(change.id, 1, 'Rejected through UI');
      onReload();
      onClose();
    } catch (err) {
      console.error('Error rejecting change:', err);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>{change.title}</DialogTitle>
      <DialogContent>
        <Tabs value={tabValue} onChange={(_, value) => setTabValue(value)} sx={{ mb: 2 }}>
          <Tab label="Overview" />
          <Tab label="Impact Analysis" />
          <Tab label="CAB Voting" />
          <Tab label="Rollback Plan" />
        </Tabs>

        {tabValue === 0 && (
          <ChangeOverviewTab change={change} />
        )}
        {tabValue === 1 && (
          <ImpactAnalysisPanel changeId={change.id} />
        )}
        {tabValue === 2 && (
          <CABVotingPanel changeId={change.id} cabVotes={cabVotes} loading={loading} />
        )}
        {tabValue === 3 && (
          <RollbackPlanBuilder changeId={change.id} plan={change.rollbackPlan} />
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Close</Button>
        {change.status === ChangeStatus.SubmittedForApproval && (
          <>
            <Button
              onClick={handleRejectChange}
              color="error"
            >
              Reject
            </Button>
            <Button
              onClick={handleApproveChange}
              variant="contained"
              color="success"
            >
              Approve
            </Button>
          </>
        )}
      </DialogActions>
    </Dialog>
  );
};

/**
 * Change Overview Tab Component
 */
const ChangeOverviewTab: React.FC<{ change: Change }> = ({ change }) => {
  return (
    <Stack spacing={3} sx={{ pt: 2 }}>
      <Box>
        <Typography variant="subtitle2" color="textSecondary">Description</Typography>
        <Typography>{change.description}</Typography>
      </Box>
      <Box>
        <Typography variant="subtitle2" color="textSecondary">Affected Services</Typography>
        <Stack direction="row" spacing={1} sx={{ mt: 1 }}>
          {change.affectedServices?.map((service, idx) => (
            <Chip key={idx} label={service} variant="outlined" />
          ))}
        </Stack>
      </Box>
      <Box>
        <Typography variant="subtitle2" color="textSecondary">Timeline</Typography>
        <Typography variant="body2">Start: {new Date(change.startDate).toLocaleString()}</Typography>
        <Typography variant="body2">End: {new Date(change.endDate).toLocaleString()}</Typography>
      </Box>
    </Stack>
  );
};

/**
 * Impact Analysis Panel Component
 */
const ImpactAnalysisPanel: React.FC<{ changeId: number }> = ({ changeId }) => {
  const [impact, setImpact] = useState<string>('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Load impact analysis
    setLoading(false);
  }, [changeId]);

  return (
    <Box sx={{ pt: 2 }}>
      <Typography variant="body2">Loading impact analysis...</Typography>
    </Box>
  );
};

/**
 * CAB Voting Panel Component
 */
const CABVotingPanel: React.FC<{
  changeId: number;
  cabVotes: CABVote[];
  loading: boolean;
}> = ({ changeId, cabVotes, loading }) => {
  if (loading) return <CircularProgress />;

  return (
    <Stack spacing={2} sx={{ pt: 2 }}>
      {cabVotes.length === 0 ? (
        <Typography color="textSecondary">No votes yet</Typography>
      ) : (
        cabVotes.map((vote, idx) => (
          <Paper key={idx} sx={{ p: 2 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Typography>{vote.voterName}</Typography>
              <Chip
                label={vote.vote.toUpperCase()}
                color={vote.vote === 'approve' ? 'success' : 'error'}
              />
            </Box>
            {vote.comments && <Typography variant="body2">{vote.comments}</Typography>}
          </Paper>
        ))
      )}
    </Stack>
  );
};

/**
 * Rollback Plan Builder Component
 */
const RollbackPlanBuilder: React.FC<{ changeId: number; plan: string }> = ({ changeId, plan }) => {
  return (
    <Box sx={{ pt: 2 }}>
      <Typography>{plan}</Typography>
    </Box>
  );
};

/**
 * Create Change Dialog Component
 */
const CreateChangeDialog: React.FC<{
  open: boolean;
  onClose: () => void;
  onSuccess: () => void;
}> = ({ open, onClose, onSuccess }) => {
  const [formData, setFormData] = useState<Partial<CreateChangeDto>>({
    priority: ChangePriority.Medium,
    riskLevel: ChangeRiskLevel.Medium
  });
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async () => {
    try {
      setSubmitting(true);
      await itsmService.createChange(formData as CreateChangeDto);
      onSuccess();
    } catch (err) {
      console.error('Error creating change:', err);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Create Change Request</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 2 }}>
          <TextField
            fullWidth
            label="Title"
            value={formData.title || ''}
            onChange={(e) => setFormData({...formData, title: e.target.value})}
          />
          <TextField
            fullWidth
            multiline
            rows={3}
            label="Description"
            value={formData.description || ''}
            onChange={(e) => setFormData({...formData, description: e.target.value})}
          />
          <TextField
            fullWidth
            type="datetime-local"
            label="Start Date"
            InputLabelProps={{ shrink: true }}
            value={formData.startDate || ''}
            onChange={(e) => setFormData({...formData, startDate: e.target.value})}
          />
          <TextField
            fullWidth
            type="datetime-local"
            label="End Date"
            InputLabelProps={{ shrink: true }}
            value={formData.endDate || ''}
            onChange={(e) => setFormData({...formData, endDate: e.target.value})}
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={submitting}
        >
          Create
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default ChangeManagementPage;
