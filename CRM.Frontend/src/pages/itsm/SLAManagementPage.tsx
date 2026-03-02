import React, { useState, useEffect, useCallback } from 'react';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  FormControlLabel,
  Grid,
  IconButton,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import type { SelectChangeEvent } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import slaService, {
  SLAPolicyDto,
  CreateSLAPolicyDto,
} from '../../services/slaService';

const PRIORITY_OPTIONS = ['Critical', 'High', 'Medium', 'Low'];

const DEFAULT_FORM: CreateSLAPolicyDto = {
  name: '',
  description: '',
  priority: 'Medium',
  responseTimeMinutes: 60,
  resolutionTimeMinutes: 480,
  escalationEnabled: true,
  isActive: true,
};

const priorityColor = (priority: string): 'error' | 'warning' | 'info' | 'default' => {
  switch (priority) {
    case 'Critical': return 'error';
    case 'High': return 'warning';
    case 'Medium': return 'info';
    default: return 'default';
  }
};

const formatMinutes = (minutes: number): string => {
  if (minutes < 60) return `${minutes}m`;
  const h = Math.floor(minutes / 60);
  const m = minutes % 60;
  return m > 0 ? `${h}h ${m}m` : `${h}h`;
};

const SLAManagementPage: React.FC = () => {
  const [policies, setPolicies] = useState<SLAPolicyDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filterPriority, setFilterPriority] = useState<string>('All');

  // Dialog state
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingPolicy, setEditingPolicy] = useState<SLAPolicyDto | null>(null);
  const [formData, setFormData] = useState<CreateSLAPolicyDto>(DEFAULT_FORM);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  // Delete state
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deletingPolicy, setDeletingPolicy] = useState<SLAPolicyDto | null>(null);
  const [deleting, setDeleting] = useState(false);

  const loadPolicies = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await slaService.getAll();
      setPolicies(data.items ?? []);
    } catch (err) {
      console.error('Failed to load SLA policies', err);
      setError('Failed to load SLA policies. Please try again.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadPolicies();
  }, [loadPolicies]);

  const handleOpenCreate = () => {
    setEditingPolicy(null);
    setFormData(DEFAULT_FORM);
    setFormError(null);
    setDialogOpen(true);
  };

  const handleOpenEdit = (policy: SLAPolicyDto) => {
    setEditingPolicy(policy);
    setFormData({
      name: policy.name,
      description: policy.description ?? '',
      priority: policy.priority,
      responseTimeMinutes: policy.responseTimeMinutes,
      resolutionTimeMinutes: policy.resolutionTimeMinutes,
      escalationEnabled: policy.escalationEnabled,
      isActive: policy.isActive,
    });
    setFormError(null);
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setEditingPolicy(null);
    setFormData(DEFAULT_FORM);
    setFormError(null);
  };

  const handleSave = async () => {
    if (!formData.name.trim()) {
      setFormError('Name is required.');
      return;
    }
    if (formData.responseTimeMinutes <= 0) {
      setFormError('Response time must be greater than 0.');
      return;
    }
    if (formData.resolutionTimeMinutes <= 0) {
      setFormError('Resolution time must be greater than 0.');
      return;
    }
    if (formData.resolutionTimeMinutes <= formData.responseTimeMinutes) {
      setFormError('Resolution time must be greater than response time.');
      return;
    }
    setSaving(true);
    setFormError(null);
    try {
      if (editingPolicy) {
        await slaService.update(editingPolicy.id, formData);
      } else {
        await slaService.create(formData);
      }
      handleCloseDialog();
      await loadPolicies();
    } catch (err) {
      console.error('Failed to save SLA policy', err);
      setFormError('Failed to save. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  const handleOpenDelete = (policy: SLAPolicyDto) => {
    setDeletingPolicy(policy);
    setDeleteDialogOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!deletingPolicy) return;
    setDeleting(true);
    try {
      await slaService.delete(deletingPolicy.id);
      setDeleteDialogOpen(false);
      setDeletingPolicy(null);
      await loadPolicies();
    } catch (err) {
      console.error('Failed to delete SLA policy', err);
    } finally {
      setDeleting(false);
    }
  };

  const filteredPolicies = filterPriority === 'All'
    ? policies
    : policies.filter(p => p.priority === filterPriority);

  const activeCount = policies.filter(p => p.isActive).length;
  const avgResponse = policies.length > 0
    ? Math.round(policies.reduce((sum, p) => sum + p.responseTimeMinutes, 0) / policies.length)
    : 0;
  const avgResolution = policies.length > 0
    ? Math.round(policies.reduce((sum, p) => sum + p.resolutionTimeMinutes, 0) / policies.length)
    : 0;

  const stats = [
    { label: 'Total Policies', value: policies.length, color: 'primary.main' },
    { label: 'Active Policies', value: activeCount, color: 'success.main' },
    { label: 'Avg Response Time', value: formatMinutes(avgResponse), color: 'warning.main' },
    { label: 'Avg Resolution Time', value: formatMinutes(avgResolution), color: 'info.main' },
  ];

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h4" component="h1" fontWeight="bold">
            SLA Policies
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
            Define and manage Service Level Agreement policies for service requests
          </Typography>
        </Box>
        <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenCreate}>
          New Policy
        </Button>
      </Box>

      {/* Stats Cards */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        {stats.map((stat) => (
          <Grid item xs={12} sm={6} md={3} key={stat.label}>
            <Card variant="outlined">
              <CardContent sx={{ textAlign: 'center', py: 2 }}>
                <Typography variant="h4" fontWeight="bold" sx={{ color: stat.color }}>
                  {stat.value}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {stat.label}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      {/* Filter Chips */}
      <Box sx={{ display: 'flex', gap: 1, mb: 2, flexWrap: 'wrap' }}>
        {['All', ...PRIORITY_OPTIONS].map((p) => (
          <Chip
            key={p}
            label={p}
            onClick={() => setFilterPriority(p)}
            color={filterPriority === p ? 'primary' : 'default'}
            variant={filterPriority === p ? 'filled' : 'outlined'}
          />
        ))}
      </Box>

      {/* Error */}
      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Table */}
      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
          <CircularProgress />
        </Box>
      ) : (
        <TableContainer component={Paper} variant="outlined">
          <Table>
            <TableHead>
              <TableRow sx={{ bgcolor: 'grey.50' }}>
                <TableCell><strong>Name</strong></TableCell>
                <TableCell><strong>Priority</strong></TableCell>
                <TableCell><strong>Response Time</strong></TableCell>
                <TableCell><strong>Resolution Time</strong></TableCell>
                <TableCell><strong>Escalation</strong></TableCell>
                <TableCell><strong>Active</strong></TableCell>
                <TableCell align="right"><strong>Actions</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredPolicies.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} align="center" sx={{ py: 4 }}>
                    <Typography color="text.secondary">
                      {filterPriority === 'All' ? 'No SLA policies found. Create one to get started.' : `No ${filterPriority} priority policies found.`}
                    </Typography>
                  </TableCell>
                </TableRow>
              ) : (
                filteredPolicies.map((policy) => (
                  <TableRow key={policy.id} hover>
                    <TableCell>
                      <Typography fontWeight={500} color="primary.main">
                        {policy.name}
                      </Typography>
                      {policy.description && (
                        <Typography variant="caption" color="text.secondary" display="block">
                          {policy.description}
                        </Typography>
                      )}
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={policy.priority}
                        color={priorityColor(policy.priority)}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>{formatMinutes(policy.responseTimeMinutes)}</TableCell>
                    <TableCell>{formatMinutes(policy.resolutionTimeMinutes)}</TableCell>
                    <TableCell>
                      <Chip
                        label={policy.escalationEnabled ? 'Enabled' : 'Disabled'}
                        color={policy.escalationEnabled ? 'warning' : 'default'}
                        size="small"
                        variant="outlined"
                      />
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={policy.isActive ? 'Active' : 'Inactive'}
                        color={policy.isActive ? 'success' : 'default'}
                        size="small"
                        variant="outlined"
                      />
                    </TableCell>
                    <TableCell align="right">
                      <Tooltip title="Edit">
                        <IconButton size="small" onClick={() => handleOpenEdit(policy)}>
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Delete">
                        <IconButton size="small" onClick={() => handleOpenDelete(policy)} color="error">
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {/* Create / Edit Dialog */}
      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{editingPolicy ? 'Edit SLA Policy' : 'Create SLA Policy'}</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          {formError && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {formError}
            </Alert>
          )}

          <TextField
            label="Name"
            value={formData.name}
            onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
            fullWidth
            required
            sx={{ mb: 2, mt: 1 }}
          />

          <TextField
            label="Description"
            value={formData.description ?? ''}
            onChange={(e) => setFormData(prev => ({ ...prev, description: e.target.value }))}
            fullWidth
            multiline
            rows={2}
            sx={{ mb: 2 }}
          />

          <FormControl fullWidth sx={{ mb: 2 }}>
            <InputLabel>Priority</InputLabel>
            <Select
              label="Priority"
              value={formData.priority}
              onChange={(e: SelectChangeEvent) => setFormData(prev => ({ ...prev, priority: e.target.value }))}
            >
              {PRIORITY_OPTIONS.map(p => (
                <MenuItem key={p} value={p}>{p}</MenuItem>
              ))}
            </Select>
          </FormControl>

          <Grid container spacing={2} sx={{ mb: 2 }}>
            <Grid item xs={6}>
              <TextField
                label="Response Time (minutes)"
                type="number"
                value={formData.responseTimeMinutes}
                onChange={(e) => setFormData(prev => ({ ...prev, responseTimeMinutes: Number.parseInt(e.target.value, 10) || 0 }))}
                fullWidth
                required
                inputProps={{ min: 1 }}
                helperText={`= ${formatMinutes(formData.responseTimeMinutes)}`}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                label="Resolution Time (minutes)"
                type="number"
                value={formData.resolutionTimeMinutes}
                onChange={(e) => setFormData(prev => ({ ...prev, resolutionTimeMinutes: Number.parseInt(e.target.value, 10) || 0 }))}
                fullWidth
                required
                inputProps={{ min: 1 }}
                helperText={`= ${formatMinutes(formData.resolutionTimeMinutes)}`}
              />
            </Grid>
          </Grid>

          <Box sx={{ display: 'flex', gap: 3 }}>
            <FormControlLabel
              control={
                <Switch
                  checked={formData.escalationEnabled ?? true}
                  onChange={(e) => setFormData(prev => ({ ...prev, escalationEnabled: e.target.checked }))}
                />
              }
              label="Escalation Enabled"
            />
            <FormControlLabel
              control={
                <Switch
                  checked={formData.isActive ?? true}
                  onChange={(e) => setFormData(prev => ({ ...prev, isActive: e.target.checked }))}
                />
              }
              label="Active"
            />
          </Box>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={handleCloseDialog} disabled={saving}>Cancel</Button>
          <Button variant="contained" onClick={handleSave} disabled={saving}>
            {saving ? <CircularProgress size={20} /> : (editingPolicy ? 'Save Changes' : 'Create Policy')}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Delete SLA Policy</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete <strong>{deletingPolicy?.name}</strong>? This action cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setDeleteDialogOpen(false)} disabled={deleting}>Cancel</Button>
          <Button variant="contained" color="error" onClick={handleConfirmDelete} disabled={deleting}>
            {deleting ? <CircularProgress size={20} /> : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default SLAManagementPage;
