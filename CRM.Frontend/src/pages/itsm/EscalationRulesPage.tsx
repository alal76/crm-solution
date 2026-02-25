import React, { useState, useEffect, useCallback } from 'react';
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
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
  TablePagination,
  TableRow,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import type { SelectChangeEvent } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import BugReportIcon from '@mui/icons-material/BugReport';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import WarningIcon from '@mui/icons-material/Warning';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import escalationService, {
  EscalationRuleDto,
  CreateEscalationRuleDto,
  EscalationRuleTestResultDto,
} from '../../services/escalationService';
import { usePagination } from '../../hooks/usePagination';

const PRIORITY_OPTIONS = ['Critical', 'High', 'Medium', 'Low'];
const CONDITION_TYPES = ['TimeElapsed', 'PriorityLevel', 'Unassigned', 'CustomerTier', 'Overdue'];
const TARGET_TYPES = ['User', 'Group', 'Manager', 'Queue'];

const DEFAULT_FORM: CreateEscalationRuleDto = {
  name: '',
  description: '',
  priority: 'High',
  conditionType: 'TimeElapsed',
  conditionValue: '',
  targetType: 'Group',
  targetName: '',
  escalationDelayMinutes: 60,
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

const EscalationRulesPage: React.FC = () => {
  const [rules, setRules] = useState<EscalationRuleDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filterPriority, setFilterPriority] = useState<string>('All');

  // Dialog state
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingRule, setEditingRule] = useState<EscalationRuleDto | null>(null);
  const [formData, setFormData] = useState<CreateEscalationRuleDto>(DEFAULT_FORM);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  // Delete state
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deletingRule, setDeletingRule] = useState<EscalationRuleDto | null>(null);
  const [deleting, setDeleting] = useState(false);

  // Test rule state
  const [testDialogOpen, setTestDialogOpen] = useState(false);
  const [testingRule, setTestingRule] = useState<EscalationRuleDto | null>(null);
  const [testServiceRequestId, setTestServiceRequestId] = useState('');
  const [testResult, setTestResult] = useState<EscalationRuleTestResultDto | null>(null);
  const [testing, setTesting] = useState(false);
  const [testError, setTestError] = useState<string | null>(null);

  const loadRules = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await escalationService.getAll();
      setRules(data);
    } catch (err) {
      console.error('Failed to load escalation rules', err);
      setError('Failed to load escalation rules. Please try again.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadRules();
  }, [loadRules]);

  const handleOpenCreate = () => {
    setEditingRule(null);
    setFormData(DEFAULT_FORM);
    setFormError(null);
    setDialogOpen(true);
  };

  const handleOpenEdit = (rule: EscalationRuleDto) => {
    setEditingRule(rule);
    setFormData({
      name: rule.name,
      description: rule.description ?? '',
      priority: rule.priority,
      conditionType: rule.conditionType,
      conditionValue: rule.conditionValue,
      targetType: rule.targetType,
      targetId: rule.targetId,
      targetName: rule.targetName ?? '',
      escalationDelayMinutes: rule.escalationDelayMinutes,
      isActive: rule.isActive,
    });
    setFormError(null);
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setEditingRule(null);
    setFormData(DEFAULT_FORM);
    setFormError(null);
  };

  const handleSave = async () => {
    if (!formData.name.trim()) {
      setFormError('Name is required.');
      return;
    }
    if (!formData.conditionValue.trim()) {
      setFormError('Condition value is required.');
      return;
    }
    setSaving(true);
    setFormError(null);
    try {
      if (editingRule) {
        await escalationService.update(editingRule.id, formData);
      } else {
        await escalationService.create(formData);
      }
      handleCloseDialog();
      await loadRules();
    } catch (err) {
      console.error('Failed to save escalation rule', err);
      setFormError('Failed to save. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  const handleOpenDelete = (rule: EscalationRuleDto) => {
    setDeletingRule(rule);
    setDeleteDialogOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!deletingRule) return;
    setDeleting(true);
    try {
      await escalationService.delete(deletingRule.id);
      setDeleteDialogOpen(false);
      setDeletingRule(null);
      await loadRules();
    } catch (err) {
      console.error('Failed to delete escalation rule', err);
    } finally {
      setDeleting(false);
    }
  };

  const handleOpenTest = (rule: EscalationRuleDto) => {
    setTestingRule(rule);
    setTestServiceRequestId('');
    setTestResult(null);
    setTestError(null);
    setTestDialogOpen(true);
  };

  const handleRunTest = async () => {
    if (!testingRule) return;
    const srId = parseInt(testServiceRequestId, 10);
    if (isNaN(srId) || srId <= 0) {
      setTestError('Please enter a valid Service Request ID.');
      return;
    }
    setTesting(true);
    setTestError(null);
    setTestResult(null);
    try {
      const result = await escalationService.testRule(testingRule.id, srId);
      setTestResult(result);
    } catch (err) {
      console.error('Failed to test rule', err);
      setTestError('Test failed. Please check the Service Request ID and try again.');
    } finally {
      setTesting(false);
    }
  };

  const filteredRules = filterPriority === 'All'
    ? rules
    : rules.filter(r => r.priority === filterPriority);

  const activeCount = rules.filter(r => r.isActive).length;
  const criticalCount = rules.filter(r => r.priority === 'Critical').length;

  // Summary stats
  const stats = [
    { label: 'Total Rules', value: rules.length, color: 'primary.main' },
    { label: 'Active Rules', value: activeCount, color: 'success.main' },
    { label: 'Critical Priority', value: criticalCount, color: 'error.main' },
    { label: 'Last 24h Escalations', value: 0, color: 'warning.main' },
  ];

  const { paginatedData: paginatedRules, page, pageSize, handlePageChange, handlePageSizeChange, pageSizeOptions } =
    usePagination(filteredRules, { defaultPageSize: 25 });

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h4" component="h1" fontWeight="bold">
            Escalation Rules
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
            Define and manage escalation rules for service requests
          </Typography>
        </Box>
        <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenCreate}>
          New Rule
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
        <>
        <TableContainer component={Paper} variant="outlined">
          <Table>
            <TableHead>
              <TableRow sx={{ bgcolor: 'grey.50' }}>
                <TableCell><strong>Name</strong></TableCell>
                <TableCell><strong>Priority</strong></TableCell>
                <TableCell><strong>Condition Type</strong></TableCell>
                <TableCell><strong>Condition Value</strong></TableCell>
                <TableCell><strong>Target Type</strong></TableCell>
                <TableCell><strong>Target</strong></TableCell>
                <TableCell><strong>Delay (min)</strong></TableCell>
                <TableCell><strong>Active</strong></TableCell>
                <TableCell align="right"><strong>Actions</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredRules.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={9} align="center" sx={{ py: 4 }}>
                    <Typography color="text.secondary">
                      {filterPriority === 'All' ? 'No escalation rules found. Create one to get started.' : `No ${filterPriority} priority rules found.`}
                    </Typography>
                  </TableCell>
                </TableRow>
              ) : (
                paginatedRules.map((rule) => (
                  <TableRow key={rule.id} hover>
                    <TableCell>
                      <Typography fontWeight={500} color="primary.main">
                        {rule.name}
                      </Typography>
                      {rule.description && (
                        <Typography variant="caption" color="text.secondary" display="block">
                          {rule.description}
                        </Typography>
                      )}
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={rule.priority}
                        color={priorityColor(rule.priority)}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>{rule.conditionType}</TableCell>
                    <TableCell>{rule.conditionValue}</TableCell>
                    <TableCell>{rule.targetType}</TableCell>
                    <TableCell>{rule.targetName ?? '—'}</TableCell>
                    <TableCell>{rule.escalationDelayMinutes}</TableCell>
                    <TableCell>
                      <Chip
                        label={rule.isActive ? 'Active' : 'Inactive'}
                        color={rule.isActive ? 'success' : 'default'}
                        size="small"
                        variant="outlined"
                      />
                    </TableCell>
                    <TableCell align="right">
                      <Tooltip title="Test Rule">
                        <IconButton size="small" onClick={() => handleOpenTest(rule)} color="info">
                          <BugReportIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Edit">
                        <IconButton size="small" onClick={() => handleOpenEdit(rule)}>
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Delete">
                        <IconButton size="small" onClick={() => handleOpenDelete(rule)} color="error">
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
        <TablePagination
          component="div"
          count={filteredRules.length}
          page={page}
          onPageChange={handlePageChange}
          rowsPerPage={pageSize}
          onRowsPerPageChange={handlePageSizeChange}
          rowsPerPageOptions={pageSizeOptions}
        />
        </>
      )}

      {/* Create / Edit Dialog */}
      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{editingRule ? 'Edit Escalation Rule' : 'Create Escalation Rule'}</DialogTitle>
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
            sx={{ mb: 2 }}
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

          <Grid container spacing={2} sx={{ mb: 2 }}>
            <Grid item xs={6}>
              <FormControl fullWidth>
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
            </Grid>
            <Grid item xs={6}>
              <FormControl fullWidth>
                <InputLabel>Condition Type</InputLabel>
                <Select
                  label="Condition Type"
                  value={formData.conditionType}
                  onChange={(e: SelectChangeEvent) => setFormData(prev => ({ ...prev, conditionType: e.target.value }))}
                >
                  {CONDITION_TYPES.map(c => (
                    <MenuItem key={c} value={c}>{c}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
          </Grid>

          <TextField
            label="Condition Value"
            value={formData.conditionValue}
            onChange={(e) => setFormData(prev => ({ ...prev, conditionValue: e.target.value }))}
            fullWidth
            required
            helperText={formData.conditionType === 'TimeElapsed' ? 'Enter number of hours (e.g. 4)' : 'Enter the threshold value'}
            sx={{ mb: 2 }}
          />

          <Grid container spacing={2} sx={{ mb: 2 }}>
            <Grid item xs={6}>
              <FormControl fullWidth>
                <InputLabel>Target Type</InputLabel>
                <Select
                  label="Target Type"
                  value={formData.targetType}
                  onChange={(e: SelectChangeEvent) => setFormData(prev => ({ ...prev, targetType: e.target.value }))}
                >
                  {TARGET_TYPES.map(t => (
                    <MenuItem key={t} value={t}>{t}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={6}>
              <TextField
                label="Target Name"
                value={formData.targetName ?? ''}
                onChange={(e) => setFormData(prev => ({ ...prev, targetName: e.target.value }))}
                fullWidth
                helperText="User, group, or queue name"
              />
            </Grid>
          </Grid>

          <TextField
            label="Escalation Delay (minutes)"
            type="number"
            value={formData.escalationDelayMinutes}
            onChange={(e) => setFormData(prev => ({ ...prev, escalationDelayMinutes: parseInt(e.target.value, 10) || 0 }))}
            fullWidth
            inputProps={{ min: 0 }}
            sx={{ mb: 2 }}
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
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={handleCloseDialog} disabled={saving}>Cancel</Button>
          <Button variant="contained" onClick={handleSave} disabled={saving}>
            {saving ? <CircularProgress size={20} /> : (editingRule ? 'Save Changes' : 'Create Rule')}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Delete Escalation Rule</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete <strong>{deletingRule?.name}</strong>? This action cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setDeleteDialogOpen(false)} disabled={deleting}>Cancel</Button>
          <Button variant="contained" color="error" onClick={handleConfirmDelete} disabled={deleting}>
            {deleting ? <CircularProgress size={20} /> : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Test Rule Dialog */}
      <Dialog open={testDialogOpen} onClose={() => setTestDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Test Rule: {testingRule?.name}</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Enter a Service Request ID to check whether this escalation rule would apply.
          </Typography>

          <TextField
            label="Service Request ID"
            type="number"
            value={testServiceRequestId}
            onChange={(e) => setTestServiceRequestId(e.target.value)}
            fullWidth
            inputProps={{ min: 1 }}
            sx={{ mb: 2 }}
          />

          {testError && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {testError}
            </Alert>
          )}

          {testResult && (
            <Accordion defaultExpanded>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  {testResult.wouldApply ? (
                    <CheckCircleIcon color="success" />
                  ) : (
                    <WarningIcon color="warning" />
                  )}
                  <Typography fontWeight={500}>
                    {testResult.wouldApply ? 'Rule Would Apply' : 'Rule Would Not Apply'}
                  </Typography>
                </Box>
              </AccordionSummary>
              <AccordionDetails>
                <Typography variant="body2" sx={{ mb: 1 }}>
                  <strong>Reason:</strong> {testResult.reason}
                </Typography>
                {testResult.matchedConditions.length > 0 && (
                  <Box>
                    <Typography variant="body2" fontWeight={500} sx={{ mb: 0.5 }}>
                      Matched Conditions:
                    </Typography>
                    {testResult.matchedConditions.map((cond, i) => (
                      <Chip key={i} label={cond} size="small" sx={{ mr: 0.5, mb: 0.5 }} />
                    ))}
                  </Box>
                )}
              </AccordionDetails>
            </Accordion>
          )}
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setTestDialogOpen(false)}>Close</Button>
          <Button
            variant="contained"
            onClick={handleRunTest}
            disabled={testing || !testServiceRequestId}
            startIcon={testing ? <CircularProgress size={16} /> : <BugReportIcon />}
          >
            Run Test
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default EscalationRulesPage;
