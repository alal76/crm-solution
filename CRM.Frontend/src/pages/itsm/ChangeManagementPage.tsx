/**
 * ChangeManagementPage - ITSM Change Management
 */

import React, { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Typography,
  Button,
  Card,
  CardContent,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  TablePagination,
  CircularProgress,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Stack,
  Chip,
  IconButton,
  Tooltip,
  Tabs,
  Tab,
  Grid,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Visibility as ViewIcon,
  Refresh as RefreshIcon,
  EventNote as ScheduleIcon,
} from '@mui/icons-material';
import { useApiState } from '../../hooks/useApiState';
import {
  DialogError,
  DialogSuccess,
  ActionButton,
  EnhancedEmptyState,
  DialogHeader,
} from '../../components/common';
import changeService, {
  Change,
  ChangeStatus,
  ChangePriority,
  ChangeRiskLevel,
} from '../../services/changeService';
import {
  ChangeImpactAnalysisPanel,
  RiskAssessmentForm,
  ChangeApprovalWorkflow,
} from '../../components/itsm';
import logger from '../../services/logger';
import logo from '../../assets/logo.png';

// Helper functions
const getStatusLabel = (status: ChangeStatus): string => {
  const labels = [
    'Draft', 'Submitted', 'Approval In Progress', 'Approved', 'Rejected',
    'Scheduled', 'In Progress', 'On Hold', 'Completed', 'Rolled Back', 'Cancelled'
  ];
  return labels[status] || 'Unknown';
};

const getStatusColor = (status: ChangeStatus): any => {
  const colors: Record<ChangeStatus, any> = {
    [ChangeStatus.Draft]: 'default',
    [ChangeStatus.SubmittedForApproval]: 'info',
    [ChangeStatus.ApprovalInProgress]: 'warning',
    [ChangeStatus.Approved]: 'success',
    [ChangeStatus.Rejected]: 'error',
    [ChangeStatus.Scheduled]: 'info',
    [ChangeStatus.InProgress]: 'warning',
    [ChangeStatus.OnHold]: 'secondary',
    [ChangeStatus.Completed]: 'success',
    [ChangeStatus.Rolled_Back]: 'error',
    [ChangeStatus.Cancelled]: 'default',
  };
  return colors[status];
};

const getRiskColor = (risk: ChangeRiskLevel): any => {
  const colors = {
    [ChangeRiskLevel.Low]: 'success',
    [ChangeRiskLevel.Medium]: 'info',
    [ChangeRiskLevel.High]: 'warning',
    [ChangeRiskLevel.VeryHigh]: 'error',
  };
  return colors[risk];
};

export const ChangeManagementPage: React.FC = () => {
  const { loading, error, setError, clearError } = useApiState();
  const [changes, setChanges] = useState<Change[]>([]);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(20);
  const [totalCount, setTotalCount] = useState(0);
  const [selectedChange, setSelectedChange] = useState<Change | null>(null);
  const [detailDialogOpen, setDetailDialogOpen] = useState(false);
  const [previewDialogOpen, setPreviewDialogOpen] = useState(false);
  const [detailTabValue, setDetailTabValue] = useState(0);

  // Load changes
  const loadChanges = async () => {
    try {
      const result = await changeService.getChanges(page + 1, pageSize);
      setChanges(result.items);
      setTotalCount(result.totalCount);
      clearError();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to load changes';
      setError(message);
      logger.error('Failed to load changes', err);
    }
  };

  useEffect(() => {
    loadChanges();
  }, [page, pageSize]);

  const handleDeleteChange = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this change?')) return;

    try {
      await changeService.deleteChange(id);
      await loadChanges();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete change');
    }
  };

  const handleApprove = async () => {
    if (!selectedChange) return;
    try {
      await changeService.approveChange(selectedChange.id);
      await loadChanges();
      setDetailDialogOpen(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to approve change');
    }
  };

  const handleReject = async (reason: string) => {
    if (!selectedChange) return;
    try {
      await changeService.rejectChange(selectedChange.id, reason);
      await loadChanges();
      setDetailDialogOpen(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to reject change');
    }
  };

  return (
    <Box sx={{ bgcolor: 'background.default', minHeight: '100vh', py: 3 }}>
      <Container maxWidth="lg">
        {/* Header */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <img src={logo} alt="Logo" style={{ height: 40 }} />
            <Typography variant="h5" fontWeight="bold">
              Change Management
            </Typography>
          </Box>
          <Stack direction="row" spacing={1}>
            <Button
              startIcon={<RefreshIcon />}
              onClick={loadChanges}
              disabled={loading}
            >
              Refresh
            </Button>
            <Button
              startIcon={<AddIcon />}
              variant="contained"
            >
              New Change
            </Button>
          </Stack>
        </Box>

        {/* Error Alert */}
        {error && (
          <Alert severity="error" sx={{ mb: 2 }} onClose={clearError}>
            {typeof error === 'string' ? error : error.message}
          </Alert>
        )}

        {/* Loading */}
        {loading && changes.length === 0 ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Card>
            <CardContent>
              <Table>
                <TableHead>
                  <TableRow sx={{ bgcolor: 'action.hover' }}>
                    <TableCell>Number</TableCell>
                    <TableCell>Title</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Risk</TableCell>
                    <TableCell>Start Date</TableCell>
                    <TableCell>End Date</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {changes.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={7} sx={{ textAlign: 'center', py: 3 }}>
                        <EnhancedEmptyState
                          variant="no-data"
                          title="No changes found"
                          description="Create a new change to get started"
                        />
                      </TableCell>
                    </TableRow>
                  ) : (
                    changes.map((change) => (
                      <TableRow key={change.id} hover>
                        <TableCell sx={{ fontWeight: 600 }}>{change.number}</TableCell>
                        <TableCell>{change.title}</TableCell>
                        <TableCell>
                          <Chip
                            label={getStatusLabel(change.status)}
                            color={getStatusColor(change.status)}
                            size="small"
                          />
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={['Low', 'Medium', 'High', 'Very High'][change.riskLevel]}
                            color={getRiskColor(change.riskLevel)}
                            size="small"
                          />
                        </TableCell>
                        <TableCell>
                          {new Date(change.startDate).toLocaleDateString()}
                        </TableCell>
                        <TableCell>
                          {new Date(change.endDate).toLocaleDateString()}
                        </TableCell>
                        <TableCell align="right">
                          <Tooltip title="View">
                            <IconButton
                              size="small"
                              onClick={() => {
                                setSelectedChange(change);
                                setDetailDialogOpen(true);
                              }}
                            >
                              <ViewIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delete">
                            <IconButton
                              size="small"
                              color="error"
                              onClick={() => handleDeleteChange(change.id)}
                            >
                              <DeleteIcon />
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
              <TablePagination
                rowsPerPageOptions={[10, 20, 50]}
                component="div"
                count={totalCount}
                rowsPerPage={pageSize}
                page={page}
                onPageChange={(_, newPage) => setPage(newPage)}
                onRowsPerPageChange={(e) => setPageSize(Number.parseInt(e.target.value))}
              />
            </CardContent>
          </Card>
        )}
      </Container>

      {/* Detail Dialog */}
      {selectedChange && (
        <Dialog
          open={detailDialogOpen}
          onClose={() => setDetailDialogOpen(false)}
          maxWidth="md"
          fullWidth
        >
          <DialogTitle>{selectedChange.number} - {selectedChange.title}</DialogTitle>
          <DialogContent sx={{ pt: 2 }}>
            <Tabs value={detailTabValue} onChange={(_, val) => setDetailTabValue(val)}>
              <Tab label="Details" />
              <Tab label="Impact Analysis" />
              <Tab label="Risk Assessment" />
              <Tab label="Approvals" />
            </Tabs>

            {detailTabValue === 0 && (
              <Box sx={{ mt: 2 }}>
                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <Typography variant="caption" color="text.secondary">Description</Typography>
                    <Typography variant="body2">{selectedChange.description}</Typography>
                  </Grid>
                  <Grid item xs={12} sm={6}>
                    <Typography variant="caption" color="text.secondary">Start Date</Typography>
                    <Typography variant="body2">{new Date(selectedChange.startDate).toLocaleString()}</Typography>
                  </Grid>
                  <Grid item xs={12} sm={6}>
                    <Typography variant="caption" color="text.secondary">End Date</Typography>
                    <Typography variant="body2">{new Date(selectedChange.endDate).toLocaleString()}</Typography>
                  </Grid>
                </Grid>
              </Box>
            )}

            {detailTabValue === 1 && (
              <Box sx={{ mt: 2 }}>
                <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>
                  {selectedChange.impactAnalysis}
                </Typography>
              </Box>
            )}

            {detailTabValue === 2 && (
              <Box sx={{ mt: 2 }}>
                <Typography variant="body2">
                  Risk Level: {['Low', 'Medium', 'High', 'Very High'][selectedChange.riskLevel]}
                </Typography>
                <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap', mt: 1 }}>
                  {selectedChange.rollbackPlan}
                </Typography>
              </Box>
            )}

            {detailTabValue === 3 && selectedChange.approvals && (
              <Box sx={{ mt: 2 }}>
                <ChangeApprovalWorkflow
                  approvals={selectedChange.approvals}
                  changeStatus={selectedChange.status}
                  onApprove={handleApprove}
                  onReject={handleReject}
                  currentUserIsApprover={true}
                />
              </Box>
            )}
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setDetailDialogOpen(false)}>Close</Button>
          </DialogActions>
        </Dialog>
      )}
    </Box>
  );
};

export default ChangeManagementPage;
