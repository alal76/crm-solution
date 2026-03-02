// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Lead Score Rules Management Page

import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Typography,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  Button,
  Chip,
  Tooltip,
  TextField,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Switch,
  FormControlLabel,
  Grid,
  Card,
  CardContent,
  Alert,
  Snackbar,
  CircularProgress,
  Tabs,
  Tab,
  Divider,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Refresh as RefreshIcon,
  TrendingUp as ScoreIcon,
  TrendingDown as DecayIcon,
  Timeline as BehaviorIcon,
  Person as DemographicIcon,
  DragIndicator as DragIcon,
} from '@mui/icons-material';
import api from '../../services/apiClient';

// Types
interface LeadScoreRule {
  id: number;
  name: string;
  description?: string;
  ruleType: LeadScoreRuleType;
  fieldName?: string;
  operator: RuleOperator;
  value?: string;
  conditionsJson?: string;
  scoreImpact: number;
  maxApplications?: number;
  decayDaysThreshold?: number;
  decayPointsPerPeriod?: number;
  decayPeriodDays?: number;
  isActive: boolean;
  priority: number;
  category?: string;
  actionType?: string;
  actionIdentifier?: string;
  createdAt: string;
  updatedAt?: string;
}

enum LeadScoreRuleType {
  Attribute = 0,
  Behavior = 1,
  Decay = 2,
  Demographic = 3,
  FitScore = 4,
}

enum RuleOperator {
  Equals = 0,
  NotEquals = 1,
  Contains = 2,
  NotContains = 3,
  GreaterThan = 4,
  LessThan = 5,
  GreaterThanOrEquals = 6,
  LessThanOrEquals = 7,
  IsEmpty = 8,
  IsNotEmpty = 9,
  In = 10,
  NotIn = 11,
}

interface FieldDefinition {
  name: string;
  displayName: string;
  dataType: string;
}

interface Stats {
  totalRules: number;
  activeRules: number;
  inactiveRules: number;
  rulesByType: { type: string; count: number }[];
  rulesByCategory: { category: string; count: number }[];
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`lead-score-rules-tabpanel-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  );
}

const ruleTypeLabels: Record<LeadScoreRuleType, { label: string; icon: React.ReactNode; color: 'primary' | 'secondary' | 'warning' | 'info' | 'success' }> = {
  [LeadScoreRuleType.Attribute]: { label: 'Attribute', icon: <DemographicIcon />, color: 'primary' },
  [LeadScoreRuleType.Behavior]: { label: 'Behavior', icon: <BehaviorIcon />, color: 'secondary' },
  [LeadScoreRuleType.Decay]: { label: 'Decay', icon: <DecayIcon />, color: 'warning' },
  [LeadScoreRuleType.Demographic]: { label: 'Demographic', icon: <DemographicIcon />, color: 'info' },
  [LeadScoreRuleType.FitScore]: { label: 'Fit Score', icon: <ScoreIcon />, color: 'success' },
};

const operatorLabels: Record<RuleOperator, string> = {
  [RuleOperator.Equals]: '=',
  [RuleOperator.NotEquals]: '≠',
  [RuleOperator.Contains]: 'contains',
  [RuleOperator.NotContains]: '!contains',
  [RuleOperator.GreaterThan]: '>',
  [RuleOperator.LessThan]: '<',
  [RuleOperator.GreaterThanOrEquals]: '≥',
  [RuleOperator.LessThanOrEquals]: '≤',
  [RuleOperator.IsEmpty]: 'is empty',
  [RuleOperator.IsNotEmpty]: 'is not empty',
  [RuleOperator.In]: 'in',
  [RuleOperator.NotIn]: 'not in',
};

const LeadScoreRulesPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState(0);
  const [rules, setRules] = useState<LeadScoreRule[]>([]);
  const [fields, setFields] = useState<FieldDefinition[]>([]);
  const [stats, setStats] = useState<Stats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Dialog state
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingRule, setEditingRule] = useState<LeadScoreRule | null>(null);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [ruleToDelete, setRuleToDelete] = useState<LeadScoreRule | null>(null);

  // Form state
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    ruleType: LeadScoreRuleType.Attribute,
    fieldName: '',
    operator: RuleOperator.Equals,
    value: '',
    scoreImpact: 10,
    maxApplications: undefined as number | undefined,
    decayDaysThreshold: 30,
    decayPointsPerPeriod: 5,
    decayPeriodDays: 7,
    isActive: true,
    priority: 100,
    category: '',
    actionType: '',
    actionIdentifier: '',
  });

  // Fetch data
  const fetchRules = useCallback(async () => {
    try {
      setLoading(true);
      const response = await api.get('/admin/leadscoreRules');
      setRules(response.data);
    } catch (err: unknown) {
      setError('Failed to load lead scoring rules');
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchFields = useCallback(async () => {
    try {
      const response = await api.get('/admin/leadscoreRules/fields');
      setFields(response.data);
    } catch (err) {
      console.error('Failed to load fields:', err);
    }
  }, []);

  const fetchStats = useCallback(async () => {
    try {
      const response = await api.get('/admin/leadscoreRules/stats');
      setStats(response.data);
    } catch (err) {
      console.error('Failed to load stats:', err);
    }
  }, []);

  useEffect(() => {
    fetchRules();
    fetchFields();
    fetchStats();
  }, [fetchRules, fetchFields, fetchStats]);

  // Handlers
  const handleOpenDialog = (rule?: LeadScoreRule) => {
    if (rule) {
      setEditingRule(rule);
      setFormData({
        name: rule.name,
        description: rule.description || '',
        ruleType: rule.ruleType,
        fieldName: rule.fieldName || '',
        operator: rule.operator,
        value: rule.value || '',
        scoreImpact: rule.scoreImpact,
        maxApplications: rule.maxApplications,
        decayDaysThreshold: rule.decayDaysThreshold || 30,
        decayPointsPerPeriod: rule.decayPointsPerPeriod || 5,
        decayPeriodDays: rule.decayPeriodDays || 7,
        isActive: rule?.isActive !== false,
        priority: rule.priority,
        category: rule.category || '',
        actionType: rule.actionType || '',
        actionIdentifier: rule.actionIdentifier || '',
      });
    } else {
      setEditingRule(null);
      setFormData({
        name: '',
        description: '',
        ruleType: LeadScoreRuleType.Attribute,
        fieldName: '',
        operator: RuleOperator.Equals,
        value: '',
        scoreImpact: 10,
        maxApplications: undefined,
        decayDaysThreshold: 30,
        decayPointsPerPeriod: 5,
        decayPeriodDays: 7,
        isActive: true,
        priority: 100,
        category: '',
        actionType: '',
        actionIdentifier: '',
      });
    }
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setEditingRule(null);
  };

  const handleSaveRule = async () => {
    try {
      const payload = {
        name: formData.name,
        description: formData.description || null,
        ruleType: formData.ruleType,
        fieldName: formData.ruleType !== LeadScoreRuleType.Decay ? formData.fieldName : null,
        operator: formData.operator,
        value: formData.value || null,
        scoreImpact: formData.scoreImpact,
        maxApplications: formData.maxApplications || null,
        decayDaysThreshold: formData.ruleType === LeadScoreRuleType.Decay ? formData.decayDaysThreshold : null,
        decayPointsPerPeriod: formData.ruleType === LeadScoreRuleType.Decay ? formData.decayPointsPerPeriod : null,
        decayPeriodDays: formData.ruleType === LeadScoreRuleType.Decay ? formData.decayPeriodDays : null,
        isActive: formData?.isActive !== false,
        priority: formData.priority,
        category: formData.category || null,
        actionType: formData.ruleType === LeadScoreRuleType.Behavior ? formData.actionType : null,
        actionIdentifier: formData.ruleType === LeadScoreRuleType.Behavior ? formData.actionIdentifier : null,
      };

      if (editingRule) {
        await api.put(`/api/admin/leadscoreRules/${editingRule.id}`, payload);
        setSuccessMessage('Rule updated successfully');
      } else {
        await api.post('/admin/leadscoreRules', payload);
        setSuccessMessage('Rule created successfully');
      }

      handleCloseDialog();
      fetchRules();
      fetchStats();
    } catch (err) {
      setError('Failed to save rule');
      console.error(err);
    }
  };

  const handleToggleRule = async (rule: LeadScoreRule) => {
    try {
      await api.patch(`/api/admin/leadscoreRules/${rule.id}/toggle`);
      setSuccessMessage(`Rule ${rule?.isActive !== false ? 'deactivated' : 'activated'}`);
      fetchRules();
      fetchStats();
    } catch (err) {
      setError('Failed to toggle rule');
      console.error(err);
    }
  };

  const handleDeleteClick = (rule: LeadScoreRule) => {
    setRuleToDelete(rule);
    setDeleteConfirmOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!ruleToDelete) return;
    try {
      await api.delete(`/api/admin/leadscoreRules/${ruleToDelete.id}`);
      setSuccessMessage('Rule deleted successfully');
      setDeleteConfirmOpen(false);
      setRuleToDelete(null);
      fetchRules();
      fetchStats();
    } catch (err) {
      setError('Failed to delete rule');
      console.error(err);
    }
  };

  // Render
  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h4" component="h1" gutterBottom>
            Lead Scoring Rules
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Configure rules to automatically score leads based on attributes and behaviors
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={() => { fetchRules(); fetchStats(); }}
          >
            Refresh
          </Button>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => handleOpenDialog()}
          >
            Add Rule
          </Button>
        </Box>
      </Box>

      {/* Stats Cards */}
      {stats && (
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="text.secondary" gutterBottom>Total Rules</Typography>
                <Typography variant="h4">{stats.totalRules}</Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="text.secondary" gutterBottom>Active Rules</Typography>
                <Typography variant="h4" color="success.main">{stats.activeRules}</Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="text.secondary" gutterBottom>Inactive Rules</Typography>
                <Typography variant="h4" color="text.disabled">{stats.inactiveRules}</Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="text.secondary" gutterBottom>Rule Types</Typography>
                <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
                  {stats.rulesByType.map(t => (
                    <Chip key={t.type} label={`${t.type}: ${t.count}`} size="small" />
                  ))}
                </Box>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      {/* Tabs */}
      <Paper sx={{ mb: 3 }}>
        <Tabs value={activeTab} onChange={(_, v) => setActiveTab(v)}>
          <Tab label="All Rules" />
          <Tab label="Attribute Rules" />
          <Tab label="Behavior Rules" />
          <Tab label="Decay Rules" />
        </Tabs>
      </Paper>

      {/* Rules Table */}
      <TabPanel value={activeTab} index={0}>
        <RulesTable
          rules={rules}
          loading={loading}
          onEdit={handleOpenDialog}
          onToggle={handleToggleRule}
          onDelete={handleDeleteClick}
        />
      </TabPanel>
      <TabPanel value={activeTab} index={1}>
        <RulesTable
          rules={rules.filter(r => r.ruleType === LeadScoreRuleType.Attribute || r.ruleType === LeadScoreRuleType.Demographic)}
          loading={loading}
          onEdit={handleOpenDialog}
          onToggle={handleToggleRule}
          onDelete={handleDeleteClick}
        />
      </TabPanel>
      <TabPanel value={activeTab} index={2}>
        <RulesTable
          rules={rules.filter(r => r.ruleType === LeadScoreRuleType.Behavior)}
          loading={loading}
          onEdit={handleOpenDialog}
          onToggle={handleToggleRule}
          onDelete={handleDeleteClick}
        />
      </TabPanel>
      <TabPanel value={activeTab} index={3}>
        <RulesTable
          rules={rules.filter(r => r.ruleType === LeadScoreRuleType.Decay)}
          loading={loading}
          onEdit={handleOpenDialog}
          onToggle={handleToggleRule}
          onDelete={handleDeleteClick}
        />
      </TabPanel>

      {/* Create/Edit Dialog */}
      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle>{editingRule ? 'Edit Rule' : 'Create New Rule'}</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12} sm={8}>
              <TextField
                label="Rule Name"
                fullWidth
                required
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <FormControl fullWidth>
                <InputLabel>Rule Type</InputLabel>
                <Select
                  value={formData.ruleType}
                  label="Rule Type"
                  onChange={(e) => setFormData({ ...formData, ruleType: e.target.value as LeadScoreRuleType })}
                >
                  <MenuItem value={LeadScoreRuleType.Attribute}>Attribute</MenuItem>
                  <MenuItem value={LeadScoreRuleType.Behavior}>Behavior</MenuItem>
                  <MenuItem value={LeadScoreRuleType.Decay}>Decay</MenuItem>
                  <MenuItem value={LeadScoreRuleType.Demographic}>Demographic</MenuItem>
                  <MenuItem value={LeadScoreRuleType.FitScore}>Fit Score</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <TextField
                label="Description"
                fullWidth
                multiline
                rows={2}
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              />
            </Grid>

            <Grid item xs={12}>
              <Divider sx={{ my: 1 }} />
              <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1 }}>
                Condition
              </Typography>
            </Grid>

            {formData.ruleType !== LeadScoreRuleType.Decay && (
              <>
                <Grid item xs={12} sm={4}>
                  <FormControl fullWidth>
                    <InputLabel>Field</InputLabel>
                    <Select
                      value={formData.fieldName}
                      label="Field"
                      onChange={(e) => setFormData({ ...formData, fieldName: e.target.value })}
                    >
                      {fields.map(f => (
                        <MenuItem key={f.name} value={f.name}>{f.displayName}</MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Grid>
                <Grid item xs={12} sm={4}>
                  <FormControl fullWidth>
                    <InputLabel>Operator</InputLabel>
                    <Select
                      value={formData.operator}
                      label="Operator"
                      onChange={(e) => setFormData({ ...formData, operator: e.target.value as RuleOperator })}
                    >
                      <MenuItem value={RuleOperator.Equals}>Equals</MenuItem>
                      <MenuItem value={RuleOperator.NotEquals}>Not Equals</MenuItem>
                      <MenuItem value={RuleOperator.Contains}>Contains</MenuItem>
                      <MenuItem value={RuleOperator.NotContains}>Does Not Contain</MenuItem>
                      <MenuItem value={RuleOperator.GreaterThan}>Greater Than</MenuItem>
                      <MenuItem value={RuleOperator.LessThan}>Less Than</MenuItem>
                      <MenuItem value={RuleOperator.IsEmpty}>Is Empty</MenuItem>
                      <MenuItem value={RuleOperator.IsNotEmpty}>Is Not Empty</MenuItem>
                      <MenuItem value={RuleOperator.In}>In List</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
                <Grid item xs={12} sm={4}>
                  <TextField
                    label="Value"
                    fullWidth
                    value={formData.value}
                    onChange={(e) => setFormData({ ...formData, value: e.target.value })}
                    helperText="For 'In List', use comma-separated values"
                  />
                </Grid>
              </>
            )}

            {formData.ruleType === LeadScoreRuleType.Decay && (
              <>
                <Grid item xs={12} sm={4}>
                  <TextField
                    label="Days Until Decay Starts"
                    type="number"
                    fullWidth
                    value={formData.decayDaysThreshold}
                    onChange={(e) => setFormData({ ...formData, decayDaysThreshold: Number.parseInt(e.target.value) || 30 })}
                  />
                </Grid>
                <Grid item xs={12} sm={4}>
                  <TextField
                    label="Points to Decay"
                    type="number"
                    fullWidth
                    value={formData.decayPointsPerPeriod}
                    onChange={(e) => setFormData({ ...formData, decayPointsPerPeriod: Number.parseInt(e.target.value) || 5 })}
                  />
                </Grid>
                <Grid item xs={12} sm={4}>
                  <TextField
                    label="Decay Period (Days)"
                    type="number"
                    fullWidth
                    value={formData.decayPeriodDays}
                    onChange={(e) => setFormData({ ...formData, decayPeriodDays: Number.parseInt(e.target.value) || 7 })}
                  />
                </Grid>
              </>
            )}

            {formData.ruleType === LeadScoreRuleType.Behavior && (
              <>
                <Grid item xs={12} sm={6}>
                  <FormControl fullWidth>
                    <InputLabel>Action Type</InputLabel>
                    <Select
                      value={formData.actionType}
                      label="Action Type"
                      onChange={(e) => setFormData({ ...formData, actionType: e.target.value })}
                    >
                      <MenuItem value="EmailOpen">Email Opened</MenuItem>
                      <MenuItem value="EmailClick">Email Link Clicked</MenuItem>
                      <MenuItem value="PageView">Page View</MenuItem>
                      <MenuItem value="FormSubmit">Form Submission</MenuItem>
                      <MenuItem value="FileDownload">File Download</MenuItem>
                      <MenuItem value="WebinarAttend">Webinar Attended</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    label="Action Identifier (optional)"
                    fullWidth
                    value={formData.actionIdentifier}
                    onChange={(e) => setFormData({ ...formData, actionIdentifier: e.target.value })}
                    helperText="Campaign ID, Page URL, Form ID, etc."
                  />
                </Grid>
              </>
            )}

            <Grid item xs={12}>
              <Divider sx={{ my: 1 }} />
              <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1 }}>
                Impact & Priority
              </Typography>
            </Grid>

            <Grid item xs={12} sm={4}>
              <TextField
                label="Score Impact"
                type="number"
                fullWidth
                required
                value={formData.scoreImpact}
                onChange={(e) => setFormData({ ...formData, scoreImpact: Number.parseInt(e.target.value) || 0 })}
                helperText="Positive = bonus, Negative = penalty"
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField
                label="Priority"
                type="number"
                fullWidth
                value={formData.priority}
                onChange={(e) => setFormData({ ...formData, priority: Number.parseInt(e.target.value) || 100 })}
                helperText="Lower = higher priority"
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField
                label="Category"
                fullWidth
                value={formData.category}
                onChange={(e) => setFormData({ ...formData, category: e.target.value })}
                helperText="e.g., Demographics, Engagement"
              />
            </Grid>

            <Grid item xs={12}>
              <FormControlLabel
                control={
                  <Switch
                    checked={formData?.isActive !== false}
                    onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                  />
                }
                label="Active"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleSaveRule}
            disabled={!formData.name}
          >
            {editingRule ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteConfirmOpen} onClose={() => setDeleteConfirmOpen(false)}>
        <DialogTitle>Delete Rule</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete the rule "{ruleToDelete?.name}"?
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteConfirmOpen(false)}>Cancel</Button>
          <Button color="error" onClick={handleDeleteConfirm}>Delete</Button>
        </DialogActions>
      </Dialog>

      {/* Snackbars */}
      <Snackbar open={!!error} autoHideDuration={6000} onClose={() => setError(null)}>
        <Alert severity="error" onClose={() => setError(null)}>{error}</Alert>
      </Snackbar>
      <Snackbar open={!!successMessage} autoHideDuration={4000} onClose={() => setSuccessMessage(null)}>
        <Alert severity="success" onClose={() => setSuccessMessage(null)}>{successMessage}</Alert>
      </Snackbar>
    </Box>
  );
};

// Rules Table Component
interface RulesTableProps {
  rules: LeadScoreRule[];
  loading: boolean;
  onEdit: (rule: LeadScoreRule) => void;
  onToggle: (rule: LeadScoreRule) => void;
  onDelete: (rule: LeadScoreRule) => void;
}

const RulesTable: React.FC<RulesTableProps> = ({ rules, loading, onEdit, onToggle, onDelete }) => {
  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (rules.length === 0) {
    return (
      <Paper sx={{ p: 4, textAlign: 'center' }}>
        <Typography color="text.secondary">No rules configured</Typography>
      </Paper>
    );
  }

  return (
    <TableContainer component={Paper}>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell width={40}></TableCell>
            <TableCell>Name</TableCell>
            <TableCell>Type</TableCell>
            <TableCell>Condition</TableCell>
            <TableCell align="center">Score Impact</TableCell>
            <TableCell align="center">Priority</TableCell>
            <TableCell align="center">Status</TableCell>
            <TableCell align="right">Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {rules.map((rule) => {
            const typeInfo = ruleTypeLabels[rule.ruleType];
            return (
              <TableRow key={rule.id} sx={{ opacity: rule?.isActive !== false ? 1 : 0.6 }}>
                <TableCell>
                  <DragIcon color="disabled" />
                </TableCell>
                <TableCell>
                  <Typography variant="body2" fontWeight="medium">{rule.name}</Typography>
                  {rule.description && (
                    <Typography variant="caption" color="text.secondary" display="block">
                      {rule.description}
                    </Typography>
                  )}
                </TableCell>
                <TableCell>
                  <Chip
                    icon={typeInfo.icon as React.ReactElement}
                    label={typeInfo.label}
                    color={typeInfo.color}
                    size="small"
                    variant="outlined"
                  />
                </TableCell>
                <TableCell>
                  {rule.ruleType === LeadScoreRuleType.Decay ? (
                    <Typography variant="body2" color="text.secondary">
                      After {rule.decayDaysThreshold} days, -{rule.decayPointsPerPeriod} pts every {rule.decayPeriodDays} days
                    </Typography>
                  ) : rule.fieldName ? (
                    <Typography variant="body2">
                      {rule.fieldName} {operatorLabels[rule.operator]} {rule.value || ''}
                    </Typography>
                  ) : (
                    <Typography variant="body2" color="text.secondary">
                      {rule.actionType || 'No condition'}
                    </Typography>
                  )}
                </TableCell>
                <TableCell align="center">
                  <Chip
                    label={`${rule.scoreImpact > 0 ? '+' : ''}${rule.scoreImpact}`}
                    color={rule.scoreImpact > 0 ? 'success' : rule.scoreImpact < 0 ? 'error' : 'default'}
                    size="small"
                  />
                </TableCell>
                <TableCell align="center">{rule.priority}</TableCell>
                <TableCell align="center">
                  <Switch
                    checked={rule?.isActive !== false}
                    onChange={() => onToggle(rule)}
                    size="small"
                  />
                </TableCell>
                <TableCell align="right">
                  <Tooltip title="Edit">
                    <IconButton size="small" onClick={() => onEdit(rule)}>
                      <EditIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title="Delete">
                    <IconButton size="small" color="error" onClick={() => onDelete(rule)}>
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </TableContainer>
  );
};

export default LeadScoreRulesPage;
