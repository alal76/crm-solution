import React, { useState, useEffect, useCallback, useMemo } from 'react';
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
  Divider,
  FormControlLabel,
  Grid,
  IconButton,
  Paper,
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
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import RemoveCircleOutlineIcon from '@mui/icons-material/RemoveCircleOutline';
import ArrowUpwardIcon from '@mui/icons-material/ArrowUpward';
import ArrowDownwardIcon from '@mui/icons-material/ArrowDownward';
import escalationPolicyService, {
  EscalationPolicyDto,
  EscalationLevelDto,
  CreateEscalationPolicyDto,
} from '../../services/escalationPolicyService';

// -- Constants ----------------------------------------------------------------

const NOTIFICATION_ROLES = ['Manager', 'TeamLead', 'Director', 'VP', 'CTO'];
const LEVEL_ACTIONS = ['Email', 'SMS', 'Slack', 'Reassign', 'Page'];

const EMPTY_LEVEL: EscalationLevelDto = {
  levelNumber: 1,
  name: '',
  timeThresholdMinutes: 30,
  notifyRoles: [],
  notifyUserIds: [],
  actions: ['Email'],
};

interface PolicyFormData {
  name: string;
  description: string;
  isActive: boolean;
  levels: EscalationLevelDto[];
  triggerConditions: string;
}

const DEFAULT_FORM: PolicyFormData = {
  name: '',
  description: '',
  isActive: true,
  levels: [{ ...EMPTY_LEVEL }],
  triggerConditions: '',
};

// -- Helpers ------------------------------------------------------------------

const formatDate = (iso: string): string => {
  try {
    return new Date(iso).toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  } catch {
    return iso;
  }
};

const toggleArrayItem = (arr: string[], item: string): string[] =>
  arr.includes(item) ? arr.filter(v => v !== item) : [...arr, item];

// =============================================================================
// Component
// =============================================================================

const EscalationPoliciesPage: React.FC = () => {
  // -- Data state -------------------------------------------------------------
  const [policies, setPolicies] = useState<EscalationPolicyDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // -- Dialog state -----------------------------------------------------------
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingPolicy, setEditingPolicy] = useState<EscalationPolicyDto | null>(null);
  const [formData, setFormData] = useState<PolicyFormData>(DEFAULT_FORM);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  // -- Delete state -----------------------------------------------------------
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deletingPolicy, setDeletingPolicy] = useState<EscalationPolicyDto | null>(null);
  const [deleting, setDeleting] = useState(false);

  // -- Data loading -----------------------------------------------------------

  const loadPolicies = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await escalationPolicyService.getAll();
      setPolicies(data);
    } catch (err) {
      console.error('Failed to load escalation policies', err);
      setError('Failed to load escalation policies. The API may not be available yet.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadPolicies();
  }, [loadPolicies]);

  // -- Summary stats ----------------------------------------------------------

  const stats = useMemo(() => {
    const activeCount = policies.filter(p => p.isActive).length;
    const totalLevels = policies.reduce((sum, p) => sum + (p.levels?.length ?? 0), 0);
    const avgLevels = policies.length > 0 ? (totalLevels / policies.length).toFixed(1) : '0';

    // Calculate the most common action across all levels
    const actionCounts: Record<string, number> = {};
    policies.forEach(p =>
      (p.levels ?? []).forEach(l =>
        (l.actions ?? []).forEach(a => {
          actionCounts[a] = (actionCounts[a] ?? 0) + 1;
        }),
      ),
    );
    const sortedActions = Object.entries(actionCounts).sort((a, b) => b[1] - a[1]);
    const mostCommonAction = sortedActions.length > 0 ? sortedActions[0][0] : '—';

    return [
      { label: 'Total Policies', value: String(policies.length), color: 'primary.main' },
      { label: 'Active Policies', value: String(activeCount), color: 'success.main' },
      { label: 'Avg Levels / Policy', value: avgLevels, color: 'info.main' },
      { label: 'Most Common Action', value: mostCommonAction, color: 'warning.main' },
    ];
  }, [policies]);

  // -- Dialog handlers --------------------------------------------------------

  const handleOpenCreate = () => {
    setEditingPolicy(null);
    setFormData({ ...DEFAULT_FORM, levels: [{ ...EMPTY_LEVEL }] });
    setFormError(null);
    setDialogOpen(true);
  };

  const handleOpenEdit = (policy: EscalationPolicyDto) => {
    setEditingPolicy(policy);
    setFormData({
      name: policy.name,
      description: policy.description ?? '',
      isActive: policy.isActive,
      levels: policy.levels?.length
        ? policy.levels.map(l => ({ ...l }))
        : [{ ...EMPTY_LEVEL }],
      triggerConditions: policy.triggerConditions ?? '',
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
      setFormError('Policy name is required.');
      return;
    }
    if (formData.levels.length === 0) {
      setFormError('At least one escalation level is required.');
      return;
    }
    for (let i = 0; i < formData.levels.length; i++) {
      const lvl = formData.levels[i];
      if (!lvl.name.trim()) {
        setFormError(`Level ${i + 1} name is required.`);
        return;
      }
      if (lvl.timeThresholdMinutes <= 0) {
        setFormError(`Level ${i + 1} time threshold must be greater than 0.`);
        return;
      }
      if (lvl.actions.length === 0) {
        setFormError(`Level ${i + 1} must have at least one action.`);
        return;
      }
    }

    setSaving(true);
    setFormError(null);
    try {
      const dto: CreateEscalationPolicyDto = {
        name: formData.name.trim(),
        description: formData.description.trim() || undefined,
        isActive: formData.isActive,
        levels: formData.levels.map((l, idx) => ({
          ...l,
          levelNumber: idx + 1,
        })),
        triggerConditions: formData.triggerConditions.trim() || undefined,
      };

      if (editingPolicy) {
        await escalationPolicyService.update(editingPolicy.id, dto);
      } else {
        await escalationPolicyService.create(dto);
      }
      handleCloseDialog();
      await loadPolicies();
    } catch (err) {
      console.error('Failed to save escalation policy', err);
      setFormError('Failed to save. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  // -- Delete handlers --------------------------------------------------------

  const handleOpenDelete = (policy: EscalationPolicyDto) => {
    setDeletingPolicy(policy);
    setDeleteDialogOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!deletingPolicy) return;
    setDeleting(true);
    try {
      await escalationPolicyService.delete(deletingPolicy.id);
      setDeleteDialogOpen(false);
      setDeletingPolicy(null);
      await loadPolicies();
    } catch (err) {
      console.error('Failed to delete escalation policy', err);
    } finally {
      setDeleting(false);
    }
  };

  // -- Level editor helpers ---------------------------------------------------

  const updateLevel = (index: number, patch: Partial<EscalationLevelDto>) => {
    setFormData(prev => ({
      ...prev,
      levels: prev.levels.map((l, i) => (i === index ? { ...l, ...patch } : l)),
    }));
  };

  const addLevel = () => {
    setFormData(prev => ({
      ...prev,
      levels: [
        ...prev.levels,
        { ...EMPTY_LEVEL, levelNumber: prev.levels.length + 1 },
      ],
    }));
  };

  const removeLevel = (index: number) => {
    setFormData(prev => ({
      ...prev,
      levels: prev.levels
        .filter((_, i) => i !== index)
        .map((l, i) => ({ ...l, levelNumber: i + 1 })),
    }));
  };

  const moveLevel = (index: number, direction: 'up' | 'down') => {
    setFormData(prev => {
      const levels = [...prev.levels];
      const target = direction === 'up' ? index - 1 : index + 1;
      if (target < 0 || target >= levels.length) return prev;
      [levels[index], levels[target]] = [levels[target], levels[index]];
      return {
        ...prev,
        levels: levels.map((l, i) => ({ ...l, levelNumber: i + 1 })),
      };
    });
  };

  // ===========================================================================
  // Render
  // ===========================================================================

  return (
    <Box sx={{ p: 3 }}>
      {/* ---- Header ---- */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h4" component="h1" fontWeight="bold">
            Escalation Policies
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
            Define multi-level escalation policies with notification rules and actions
          </Typography>
        </Box>
        <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenCreate}>
          New Policy
        </Button>
      </Box>

      {/* ---- Summary Cards ---- */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        {stats.map(stat => (
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

      {/* ---- Error ---- */}
      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* ---- Table ---- */}
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
                <TableCell><strong>Levels</strong></TableCell>
                <TableCell><strong>Status</strong></TableCell>
                <TableCell><strong>Created</strong></TableCell>
                <TableCell align="right"><strong>Actions</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {policies.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={5} align="center" sx={{ py: 4 }}>
                    <Typography color="text.secondary">
                      No escalation policies found. Create one to get started.
                    </Typography>
                  </TableCell>
                </TableRow>
              ) : (
                policies.map(policy => (
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
                        label={`${policy.levels?.length ?? 0} level${(policy.levels?.length ?? 0) !== 1 ? 's' : ''}`}
                        size="small"
                        color="info"
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
                    <TableCell>
                      <Typography variant="body2">{formatDate(policy.createdAt)}</Typography>
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

      {/* ================================================================== */}
      {/* Create / Edit Dialog                                                */}
      {/* ================================================================== */}
      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle>
          {editingPolicy ? 'Edit Escalation Policy' : 'Create Escalation Policy'}
        </DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          {formError && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {formError}
            </Alert>
          )}

          {/* -- Basic fields -- */}
          <TextField
            label="Policy Name"
            value={formData.name}
            onChange={e => setFormData(prev => ({ ...prev, name: e.target.value }))}
            fullWidth
            required
            sx={{ mb: 2, mt: 1 }}
          />

          <TextField
            label="Description"
            value={formData.description}
            onChange={e => setFormData(prev => ({ ...prev, description: e.target.value }))}
            fullWidth
            multiline
            rows={2}
            sx={{ mb: 2 }}
          />

          <FormControlLabel
            control={
              <Switch
                checked={formData.isActive}
                onChange={e => setFormData(prev => ({ ...prev, isActive: e.target.checked }))}
              />
            }
            label="Active"
            sx={{ mb: 2 }}
          />

          <Divider sx={{ my: 2 }} />

          {/* -- Level Editor -- */}
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h6" fontWeight="bold">
              Escalation Levels
            </Typography>
            <Button size="small" startIcon={<AddIcon />} onClick={addLevel}>
              Add Level
            </Button>
          </Box>

          {formData.levels.map((level, idx) => (
            <Card
              key={idx}
              variant="outlined"
              sx={{ mb: 2, borderLeft: 4, borderLeftColor: 'primary.main' }}
            >
              <CardContent>
                {/* Level header */}
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1.5 }}>
                  <Typography variant="subtitle1" fontWeight="bold" color="primary.main">
                    L{idx + 1}
                  </Typography>
                  <Box>
                    <Tooltip title="Move Up">
                      <span>
                        <IconButton
                          size="small"
                          disabled={idx === 0}
                          onClick={() => moveLevel(idx, 'up')}
                        >
                          <ArrowUpwardIcon fontSize="small" />
                        </IconButton>
                      </span>
                    </Tooltip>
                    <Tooltip title="Move Down">
                      <span>
                        <IconButton
                          size="small"
                          disabled={idx === formData.levels.length - 1}
                          onClick={() => moveLevel(idx, 'down')}
                        >
                          <ArrowDownwardIcon fontSize="small" />
                        </IconButton>
                      </span>
                    </Tooltip>
                    <Tooltip title="Remove Level">
                      <span>
                        <IconButton
                          size="small"
                          color="error"
                          disabled={formData.levels.length <= 1}
                          onClick={() => removeLevel(idx)}
                        >
                          <RemoveCircleOutlineIcon fontSize="small" />
                        </IconButton>
                      </span>
                    </Tooltip>
                  </Box>
                </Box>

                <Grid container spacing={2}>
                  {/* Level Name */}
                  <Grid item xs={12} sm={6}>
                    <TextField
                      label="Level Name"
                      value={level.name}
                      onChange={e => updateLevel(idx, { name: e.target.value })}
                      fullWidth
                      required
                      size="small"
                    />
                  </Grid>

                  {/* Time Threshold */}
                  <Grid item xs={12} sm={6}>
                    <TextField
                      label="Time Threshold (minutes)"
                      type="number"
                      value={level.timeThresholdMinutes}
                      onChange={e =>
                        updateLevel(idx, {
                          timeThresholdMinutes: parseInt(e.target.value, 10) || 0,
                        })
                      }
                      fullWidth
                      size="small"
                      inputProps={{ min: 1 }}
                    />
                  </Grid>

                  {/* Notification Roles */}
                  <Grid item xs={12}>
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 0.5 }}>
                      Notification Roles
                    </Typography>
                    <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
                      {NOTIFICATION_ROLES.map(role => (
                        <Chip
                          key={role}
                          label={role}
                          size="small"
                          color={level.notifyRoles.includes(role) ? 'primary' : 'default'}
                          variant={level.notifyRoles.includes(role) ? 'filled' : 'outlined'}
                          onClick={() =>
                            updateLevel(idx, {
                              notifyRoles: toggleArrayItem(level.notifyRoles, role),
                            })
                          }
                          sx={{ cursor: 'pointer' }}
                        />
                      ))}
                    </Box>
                  </Grid>

                  {/* Actions */}
                  <Grid item xs={12}>
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 0.5 }}>
                      Actions
                    </Typography>
                    <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
                      {LEVEL_ACTIONS.map(action => (
                        <Chip
                          key={action}
                          label={action}
                          size="small"
                          color={level.actions.includes(action) ? 'secondary' : 'default'}
                          variant={level.actions.includes(action) ? 'filled' : 'outlined'}
                          onClick={() =>
                            updateLevel(idx, {
                              actions: toggleArrayItem(level.actions, action),
                            })
                          }
                          sx={{ cursor: 'pointer' }}
                        />
                      ))}
                    </Box>
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          ))}

          <Divider sx={{ my: 2 }} />

          {/* -- Trigger Conditions (optional JSON) -- */}
          <TextField
            label="Trigger Conditions (JSON, optional)"
            value={formData.triggerConditions}
            onChange={e => setFormData(prev => ({ ...prev, triggerConditions: e.target.value }))}
            fullWidth
            multiline
            rows={3}
            placeholder='{"priority": "Critical", "category": "Outage"}'
            helperText="Optional JSON-based trigger condition expression"
          />
        </DialogContent>

        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={handleCloseDialog} disabled={saving}>
            Cancel
          </Button>
          <Button variant="contained" onClick={handleSave} disabled={saving}>
            {saving ? (
              <CircularProgress size={20} />
            ) : editingPolicy ? (
              'Save Changes'
            ) : (
              'Create Policy'
            )}
          </Button>
        </DialogActions>
      </Dialog>

      {/* ================================================================== */}
      {/* Delete Confirmation Dialog                                          */}
      {/* ================================================================== */}
      <Dialog
        open={deleteDialogOpen}
        onClose={() => setDeleteDialogOpen(false)}
        maxWidth="xs"
        fullWidth
      >
        <DialogTitle>Delete Escalation Policy</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete <strong>{deletingPolicy?.name}</strong>? This action
            cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setDeleteDialogOpen(false)} disabled={deleting}>
            Cancel
          </Button>
          <Button variant="contained" color="error" onClick={handleConfirmDelete} disabled={deleting}>
            {deleting ? <CircularProgress size={20} /> : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default EscalationPoliciesPage;
