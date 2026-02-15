/**
 * ProblemManagementPage - ITSM Problem Management
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
  TextField,
  MenuItem,
  Stack,
  Chip,
  IconButton,
  Tooltip,
  Tabs,
  Tab,
  Grid,
  Paper,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Visibility as ViewIcon,
  Refresh as RefreshIcon,
  GetApp as DownloadIcon,
} from '@mui/icons-material';
import { useApiState } from '../hooks/useApiState';
import {
  DialogError,
  DialogSuccess,
  ActionButton,
  EnhancedEmptyState,
  DialogHeader,
} from '../components/common';
import problemService, {
  Problem,
  ProblemStatus,
  ProblemPriority,
  CreateProblemRequest,
  UpdateProblemRequest,
} from '../services/problemService';
import { ProblemRelatedIncidentsList } from '../components/itsm/ProblemRelatedIncidentsList';
import logger from '../services/logger';
import logo from '../assets/logo.png';

// Helper functions
const getStatusLabel = (status: ProblemStatus): string => {
  const labels = ['Draft', 'Open', 'In Progress', 'On Hold', 'Resolved', 'Closed', 'Cancelled'];
  return labels[status] || 'Unknown';
};

const getStatusColor = (status: ProblemStatus): 'default' | 'warning' | 'success' | 'error' | 'info' | 'primary' | 'secondary' => {
  const colors: Record<ProblemStatus, any> = {
    [ProblemStatus.Draft]: 'default',
    [ProblemStatus.Open]: 'info',
    [ProblemStatus.InProgress]: 'warning',
    [ProblemStatus.OnHold]: 'secondary',
    [ProblemStatus.Resolved]: 'success',
    [ProblemStatus.Closed]: 'default',
    [ProblemStatus.Cancelled]: 'error',
  };
  return colors[status];
};

const getPriorityLabel = (priority: ProblemPriority): string => {
  const labels = ['Critical', 'High', 'Medium', 'Low', 'Planning'];
  return labels[priority] || 'Unknown';
};

const getPriorityColor = (priority: ProblemPriority): 'error' | 'warning' | 'info' | 'success' | 'default' => {
  const colors: Record<ProblemPriority, any> = {
    [ProblemPriority.Critical]: 'error',
    [ProblemPriority.High]: 'warning',
    [ProblemPriority.Medium]: 'info',
    [ProblemPriority.Low]: 'success',
    [ProblemPriority.Planning]: 'default',
  };
  return colors[priority];
};

export const ProblemManagementPage: React.FC = () => {
  const { loading, error, setError, clearError } = useApiState();
  const [problems, setProblems] = useState<Problem[]>([]);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(20);
  const [totalCount, setTotalCount] = useState(0);
  const [selectedProblem, setSelectedProblem] = useState<Problem | null>(null);
  const [detailDialogOpen, setDetailDialogOpen] = useState(false);
  const [detailTabValue, setDetailTabValue] = useState(0);
  const [formDialogOpen, setFormDialogOpen] = useState(false);
  const [formData, setFormData] = useState<CreateProblemRequest>({
    title: '',
    description: '',
    priority: ProblemPriority.Medium,
    category: 0,
  });
  const [relatedIncidents, setRelatedIncidents] = useState<any[]>([]);

  // Load problems
  const loadProblems = async () => {
    try {
      const result = await problemService.getProblems(page + 1, pageSize);
      setProblems(result.items);
      setTotalCount(result.totalCount);
      clearError();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to load problems';
      setError(message);
      logger.error('Failed to load problems', err);
    }
  };

  useEffect(() => {
    loadProblems();
  }, [page, pageSize]);

  // Load related incidents when problem is selected
  useEffect(() => {
    if (selectedProblem) {
      problemService
        .getRelatedIncidents(selectedProblem.id)
        .then(setRelatedIncidents)
        .catch((err) => logger.error('Failed to load related incidents', err));
    }
  }, [selectedProblem]);

  const handleCreateProblem = async () => {
    try {
      await problemService.createProblem(formData);
      setFormDialogOpen(false);
      setFormData({
        title: '',
        description: '',
        priority: ProblemPriority.Medium,
        category: 0,
      });
      await loadProblems();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create problem');
    }
  };

  const handleDeleteProblem = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this problem?')) return;

    try {
      await problemService.deleteProblem(id);
      await loadProblems();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete problem');
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
              Problem Management
            </Typography>
          </Box>
          <Stack direction="row" spacing={1}>
            <Button
              startIcon={<RefreshIcon />}
              onClick={loadProblems}
              disabled={loading}
            >
              Refresh
            </Button>
            <Button
              startIcon={<AddIcon />}
              variant="contained"
              onClick={() => setFormDialogOpen(true)}
            >
              New Problem
            </Button>
          </Stack>
        </Box>

        {/* Error Alert */}
        {error && (
          <Alert severity="error" sx={{ mb: 2 }} onClose={clearError}>
            {error}
          </Alert>
        )}

        {/* Loading */}
        {loading && problems.length === 0 ? (
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
                    <TableCell>Priority</TableCell>
                    <TableCell>Related Incidents</TableCell>
                    <TableCell>Created</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {problems.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={7} sx={{ textAlign: 'center', py: 3 }}>
                        <EnhancedEmptyState
                          variant="no-data"
                          title="No problems found"
                          message="Create a new problem to get started"
                        />
                      </TableCell>
                    </TableRow>
                  ) : (
                    problems.map((problem) => (
                      <TableRow key={problem.id} hover>
                        <TableCell sx={{ fontWeight: 600 }}>{problem.number}</TableCell>
                        <TableCell>{problem.title}</TableCell>
                        <TableCell>
                          <Chip
                            label={getStatusLabel(problem.status)}
                            color={getStatusColor(problem.status)}
                            size="small"
                          />
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={getPriorityLabel(problem.priority)}
                            color={getPriorityColor(problem.priority)}
                            size="small"
                          />
                        </TableCell>
                        <TableCell>{problem.relatedIncidentCount}</TableCell>
                        <TableCell>
                          {new Date(problem.createdAt).toLocaleDateString()}
                        </TableCell>
                        <TableCell align="right">
                          <Tooltip title="View">
                            <IconButton
                              size="small"
                              onClick={() => {
                                setSelectedProblem(problem);
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
                              onClick={() => handleDeleteProblem(problem.id)}
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
                onRowsPerPageChange={(e) => setPageSize(parseInt(e.target.value))}
              />
            </CardContent>
          </Card>
        )}
      </Container>

      {/* Detail Dialog */}
      {selectedProblem && (
        <Dialog
          open={detailDialogOpen}
          onClose={() => setDetailDialogOpen(false)}
          maxWidth="md"
          fullWidth
        >
          <DialogTitle>{selectedProblem.number} - {selectedProblem.title}</DialogTitle>
          <DialogContent sx={{ pt: 2 }}>
            <Tabs value={detailTabValue} onChange={(_, val) => setDetailTabValue(val)}>
              <Tab label="Details" />
              <Tab label="Root Cause" />
              <Tab label="Related Incidents" />
            </Tabs>

            {detailTabValue === 0 && (
              <Box sx={{ mt: 2 }}>
                <Grid container spacing={2}>
                  <Grid item xs={12} sm={6}>
                    <Typography variant="caption" color="text.secondary">Status</Typography>
                    <Typography variant="body2">{getStatusLabel(selectedProblem.status)}</Typography>
                  </Grid>
                  <Grid item xs={12} sm={6}>
                    <Typography variant="caption" color="text.secondary">Priority</Typography>
                    <Typography variant="body2">{getPriorityLabel(selectedProblem.priority)}</Typography>
                  </Grid>
                  <Grid item xs={12}>
                    <Typography variant="caption" color="text.secondary">Description</Typography>
                    <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>
                      {selectedProblem.description}
                    </Typography>
                  </Grid>
                  {selectedProblem.workaround && (
                    <Grid item xs={12}>
                      <Typography variant="caption" color="text.secondary">Workaround</Typography>
                      <Typography variant="body2">{selectedProblem.workaround}</Typography>
                    </Grid>
                  )}
                </Grid>
              </Box>
            )}

            {detailTabValue === 1 && (
              <Box sx={{ mt: 2 }}>
                <Typography variant="body2">
                  {selectedProblem.rootCauseAnalysis || 'No root cause analysis yet'}
                </Typography>
              </Box>
            )}

            {detailTabValue === 2 && (
              <Box sx={{ mt: 2 }}>
                <ProblemRelatedIncidentsList
                  incidents={relatedIncidents}
                  onOpen={(id) => {
                    // Navigate to incident detail
                  }}
                />
              </Box>
            )}
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setDetailDialogOpen(false)}>Close</Button>
          </DialogActions>
        </Dialog>
      )}

      {/* Create/Edit Problem Dialog */}
      <Dialog open={formDialogOpen} onClose={() => setFormDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogHeader mode="create" entityType="Problem" onClose={() => setFormDialogOpen(false)} />
        <DialogContent sx={{ pt: 2 }}>
          <Stack spacing={2}>
            <TextField
              fullWidth
              label="Title"
              value={formData.title}
              onChange={(e) => setFormData({ ...formData, title: e.target.value })}
            />
            <TextField
              fullWidth
              label="Description"
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              multiline
              rows={3}
            />
            <TextField
              select
              fullWidth
              label="Priority"
              value={formData.priority}
              onChange={(e) => setFormData({ ...formData, priority: parseInt(e.target.value) as ProblemPriority })}
            >
              <MenuItem value={ProblemPriority.Critical}>Critical</MenuItem>
              <MenuItem value={ProblemPriority.High}>High</MenuItem>
              <MenuItem value={ProblemPriority.Medium}>Medium</MenuItem>
              <MenuItem value={ProblemPriority.Low}>Low</MenuItem>
            </TextField>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setFormDialogOpen(false)}>Cancel</Button>
          <Button
            onClick={handleCreateProblem}
            variant="contained"
            disabled={loading || !formData.title}
          >
            Create
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default ProblemManagementPage;
