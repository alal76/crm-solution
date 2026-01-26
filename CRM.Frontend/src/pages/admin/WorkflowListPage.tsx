/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Button,
  IconButton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Chip,
  Alert,
  CircularProgress,
  Tabs,
  Tab,
  Grid,
  Tooltip,
  InputAdornment,
  Autocomplete,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  PlayArrow as PlayIcon,
  Pause as PauseIcon,
  Visibility as ViewIcon,
  Search as SearchIcon,
  AccountTree as WorkflowIcon,
  FilterList as FilterIcon,
  Refresh as RefreshIcon,
  ContentCopy as CloneIcon,
  Timeline as TimelineIcon,
  CheckCircle as ActiveIcon,
  Cancel as InactiveIcon,
} from '@mui/icons-material';
import {
  workflowService,
  WorkflowDefinition,
  WorkflowDefinitionDetail,
  WorkflowStatistics,
  CreateWorkflowDto,
  UpdateWorkflowDto,
  EntityTypeOption,
  statusColors,
  WorkflowStatus,
} from '../../services/workflowService';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div role="tabpanel" hidden={value !== index} {...other}>
      {value === index && <Box sx={{ py: 3 }}>{children}</Box>}
    </div>
  );
}

const iconOptions = [
  'AccountTree', 'Timeline', 'CallSplit', 'CompareArrows', 'DeviceHub',
  'Schema', 'Hub', 'Shuffle', 'Fork', 'Merge', 'Route', 'AltRoute',
  'LinearScale', 'Workspaces', 'Category', 'Settings', 'AutoAwesome'
];

const colorOptions = [
  '#6750A4', '#4CAF50', '#2196F3', '#FF9800', '#9C27B0', '#F44336',
  '#00BCD4', '#795548', '#607D8B', '#E91E63', '#3F51B5', '#009688'
];

function WorkflowListPage() {
  const navigate = useNavigate();
  const [tabValue, setTabValue] = useState(0);
  const [workflows, setWorkflows] = useState<WorkflowDefinition[]>([]);
  const [statistics, setStatistics] = useState<WorkflowStatistics | null>(null);
  const [entityTypes, setEntityTypes] = useState<EntityTypeOption[]>([]);
  const [categories, setCategories] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Filters
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [entityTypeFilter, setEntityTypeFilter] = useState<string>('');
  const [categoryFilter, setCategoryFilter] = useState<string>('');

  // Dialog state
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingWorkflow, setEditingWorkflow] = useState<WorkflowDefinition | null>(null);
  const [formData, setFormData] = useState<CreateWorkflowDto>({
    workflowKey: '',
    name: '',
    description: '',
    category: '',
    entityType: '',
    iconName: 'AccountTree',
    color: '#6750A4',
    priority: 100,
    maxConcurrentInstances: 0,
    defaultTimeoutHours: 0,
    tags: [],
  });

  // Delete confirmation
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [workflowToDelete, setWorkflowToDelete] = useState<WorkflowDefinition | null>(null);

  const loadData = useCallback(async () => {
    try {
      setLoading(true);
      const [workflowsData, statsData, entityTypesData, categoriesData] = await Promise.all([
        workflowService.getWorkflows({
          search: searchTerm || undefined,
          status: statusFilter || undefined,
          entityType: entityTypeFilter || undefined,
          category: categoryFilter || undefined,
        }),
        workflowService.getStatistics(),
        workflowService.getEntityTypes(),
        workflowService.getCategories(),
      ]);
      setWorkflows(workflowsData);
      setStatistics(statsData);
      setEntityTypes(entityTypesData);
      setCategories(categoriesData);
      setError('');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load workflows');
    } finally {
      setLoading(false);
    }
  }, [searchTerm, statusFilter, entityTypeFilter, categoryFilter]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleOpenDialog = (workflow?: WorkflowDefinition) => {
    if (workflow) {
      setEditingWorkflow(workflow);
      setFormData({
        workflowKey: workflow.workflowKey,
        name: workflow.name,
        description: workflow.description || '',
        category: workflow.category || '',
        entityType: workflow.entityType,
        iconName: workflow.iconName || 'AccountTree',
        color: workflow.color || '#6750A4',
        priority: workflow.priority,
        maxConcurrentInstances: workflow.maxConcurrentInstances,
        defaultTimeoutHours: workflow.defaultTimeoutHours,
        tags: workflow.tags || [],
      });
    } else {
      setEditingWorkflow(null);
      setFormData({
        workflowKey: '',
        name: '',
        description: '',
        category: '',
        entityType: '',
        iconName: 'AccountTree',
        color: '#6750A4',
        priority: 100,
        maxConcurrentInstances: 0,
        defaultTimeoutHours: 0,
        tags: [],
      });
    }
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setEditingWorkflow(null);
  };

  const handleSave = async () => {
    try {
      setSaving(true);
      if (editingWorkflow) {
        await workflowService.updateWorkflow(editingWorkflow.id, formData);
        setSuccess('Workflow updated successfully');
      } else {
        const result = await workflowService.createWorkflow(formData);
        setSuccess('Workflow created successfully');
        // Navigate to the designer for the new workflow
        navigate(`/admin/workflows/${result.id}/designer`);
      }
      handleCloseDialog();
      loadData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save workflow');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!workflowToDelete) return;
    try {
      setSaving(true);
      await workflowService.deleteWorkflow(workflowToDelete.id);
      setSuccess('Workflow deleted successfully');
      setDeleteDialogOpen(false);
      setWorkflowToDelete(null);
      loadData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete workflow');
    } finally {
      setSaving(false);
    }
  };

  const handleActivate = async (workflow: WorkflowDefinition) => {
    try {
      setSaving(true);
      // Get the workflow details to find the latest version
      const detail = await workflowService.getWorkflow(workflow.id);
      const draftVersion = detail.versions.find(v => v.status === 'Draft');
      if (!draftVersion) {
        setError('No draft version to activate');
        return;
      }
      await workflowService.activateWorkflow(workflow.id, draftVersion.id);
      setSuccess('Workflow activated successfully');
      loadData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to activate workflow');
    } finally {
      setSaving(false);
    }
  };

  const handlePause = async (workflow: WorkflowDefinition) => {
    try {
      setSaving(true);
      await workflowService.pauseWorkflow(workflow.id);
      setSuccess('Workflow paused successfully');
      loadData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to pause workflow');
    } finally {
      setSaving(false);
    }
  };

  const getStatusColor = (status: string) => {
    return statusColors[status] || '#9E9E9E';
  };

  const renderStatCard = (title: string, value: number, color: string) => (
    <Card sx={{ height: '100%' }}>
      <CardContent>
        <Typography color="text.secondary" gutterBottom variant="body2">
          {title}
        </Typography>
        <Typography variant="h4" sx={{ color }}>
          {value}
        </Typography>
      </CardContent>
    </Card>
  );

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <WorkflowIcon sx={{ fontSize: 32, color: 'primary.main' }} />
          <Typography variant="h4">Workflow Management</Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={loadData}
            disabled={loading}
          >
            Refresh
          </Button>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => handleOpenDialog()}
          >
            New Workflow
          </Button>
        </Box>
      </Box>

      {error && (
        <Alert severity="error" onClose={() => setError('')} sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}
      {success && (
        <Alert severity="success" onClose={() => setSuccess('')} sx={{ mb: 2 }}>
          {success}
        </Alert>
      )}

      <Tabs value={tabValue} onChange={(_, v) => setTabValue(v)} sx={{ mb: 2 }}>
        <Tab label="Workflows" icon={<WorkflowIcon />} iconPosition="start" />
        <Tab label="Statistics" icon={<TimelineIcon />} iconPosition="start" />
      </Tabs>

      <TabPanel value={tabValue} index={0}>
        {/* Filters */}
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Grid container spacing={2} alignItems="center">
              <Grid item xs={12} md={3}>
                <TextField
                  fullWidth
                  size="small"
                  placeholder="Search workflows..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <SearchIcon />
                      </InputAdornment>
                    ),
                  }}
                />
              </Grid>
              <Grid item xs={12} md={2}>
                <FormControl fullWidth size="small">
                  <InputLabel>Status</InputLabel>
                  <Select
                    value={statusFilter}
                    onChange={(e) => setStatusFilter(e.target.value)}
                    label="Status"
                  >
                    <MenuItem value="">All</MenuItem>
                    <MenuItem value="Draft">Draft</MenuItem>
                    <MenuItem value="Active">Active</MenuItem>
                    <MenuItem value="Paused">Paused</MenuItem>
                    <MenuItem value="Archived">Archived</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={2}>
                <FormControl fullWidth size="small">
                  <InputLabel>Entity Type</InputLabel>
                  <Select
                    value={entityTypeFilter}
                    onChange={(e) => setEntityTypeFilter(e.target.value)}
                    label="Entity Type"
                  >
                    <MenuItem value="">All</MenuItem>
                    {entityTypes.map((et) => (
                      <MenuItem key={et.value} value={et.value}>
                        {et.label}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={2}>
                <FormControl fullWidth size="small">
                  <InputLabel>Category</InputLabel>
                  <Select
                    value={categoryFilter}
                    onChange={(e) => setCategoryFilter(e.target.value)}
                    label="Category"
                  >
                    <MenuItem value="">All</MenuItem>
                    {categories.map((cat) => (
                      <MenuItem key={cat} value={cat}>
                        {cat}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={3}>
                <Button
                  variant="outlined"
                  onClick={() => {
                    setSearchTerm('');
                    setStatusFilter('');
                    setEntityTypeFilter('');
                    setCategoryFilter('');
                  }}
                  startIcon={<FilterIcon />}
                >
                  Clear Filters
                </Button>
              </Grid>
            </Grid>
          </CardContent>
        </Card>

        {/* Workflows Table */}
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : (
          <TableContainer component={Paper}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Workflow</TableCell>
                  <TableCell>Entity Type</TableCell>
                  <TableCell>Category</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Version</TableCell>
                  <TableCell>Priority</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {workflows.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={7} align="center">
                      <Typography color="text.secondary" sx={{ py: 4 }}>
                        No workflows found. Create your first workflow to get started.
                      </Typography>
                    </TableCell>
                  </TableRow>
                ) : (
                  workflows.map((workflow) => (
                    <TableRow key={workflow.id} hover>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Box
                            sx={{
                              width: 36,
                              height: 36,
                              borderRadius: 1,
                              backgroundColor: workflow.color || '#6750A4',
                              display: 'flex',
                              alignItems: 'center',
                              justifyContent: 'center',
                              color: 'white',
                            }}
                          >
                            <WorkflowIcon fontSize="small" />
                          </Box>
                          <Box>
                            <Typography variant="body2" fontWeight="medium">
                              {workflow.name}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              {workflow.workflowKey}
                            </Typography>
                          </Box>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={workflow.entityType}
                          size="small"
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell>{workflow.category || '-'}</TableCell>
                      <TableCell>
                        <Chip
                          label={workflow.status}
                          size="small"
                          sx={{
                            backgroundColor: getStatusColor(workflow.status),
                            color: 'white',
                          }}
                        />
                      </TableCell>
                      <TableCell>v{workflow.currentVersion}</TableCell>
                      <TableCell>{workflow.priority}</TableCell>
                      <TableCell align="right">
                        <Tooltip title="Open Designer">
                          <IconButton
                            size="small"
                            onClick={() => navigate(`/admin/workflows/${workflow.id}/designer`)}
                          >
                            <ViewIcon />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="View Instances">
                          <IconButton
                            size="small"
                            onClick={() => navigate(`/admin/workflows/${workflow.id}/instances`)}
                          >
                            <TimelineIcon />
                          </IconButton>
                        </Tooltip>
                        {workflow.status === 'Draft' && (
                          <Tooltip title="Activate">
                            <IconButton
                              size="small"
                              color="success"
                              onClick={() => handleActivate(workflow)}
                            >
                              <PlayIcon />
                            </IconButton>
                          </Tooltip>
                        )}
                        {workflow.status === 'Active' && (
                          <Tooltip title="Pause">
                            <IconButton
                              size="small"
                              color="warning"
                              onClick={() => handlePause(workflow)}
                            >
                              <PauseIcon />
                            </IconButton>
                          </Tooltip>
                        )}
                        <Tooltip title="Edit">
                          <IconButton
                            size="small"
                            onClick={() => handleOpenDialog(workflow)}
                            disabled={workflow.isSystem}
                          >
                            <EditIcon />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
                          <IconButton
                            size="small"
                            color="error"
                            onClick={() => {
                              setWorkflowToDelete(workflow);
                              setDeleteDialogOpen(true);
                            }}
                            disabled={workflow.isSystem}
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
          </TableContainer>
        )}
      </TabPanel>

      <TabPanel value={tabValue} index={1}>
        {/* Statistics */}
        {statistics && (
          <Grid container spacing={3}>
            <Grid item xs={12} md={3}>
              {renderStatCard('Total Workflows', statistics.totalWorkflows, '#6750A4')}
            </Grid>
            <Grid item xs={12} md={3}>
              {renderStatCard('Active Workflows', statistics.activeWorkflows, '#4CAF50')}
            </Grid>
            <Grid item xs={12} md={3}>
              {renderStatCard('Draft Workflows', statistics.draftWorkflows, '#FF9800')}
            </Grid>
            <Grid item xs={12} md={3}>
              {renderStatCard('Running Instances', statistics.runningInstances, '#2196F3')}
            </Grid>
            <Grid item xs={12} md={3}>
              {renderStatCard('Total Instances', statistics.totalInstances, '#9C27B0')}
            </Grid>
            <Grid item xs={12} md={3}>
              {renderStatCard('Completed', statistics.completedInstances, '#4CAF50')}
            </Grid>
            <Grid item xs={12} md={3}>
              {renderStatCard('Failed', statistics.failedInstances, '#F44336')}
            </Grid>
            <Grid item xs={12} md={3}>
              {renderStatCard('Pending Tasks', statistics.pendingTasks, '#FF9800')}
            </Grid>

            {Object.keys(statistics.workflowsByCategory).length > 0 && (
              <Grid item xs={12} md={6}>
                <Card>
                  <CardContent>
                    <Typography variant="h6" gutterBottom>
                      Workflows by Category
                    </Typography>
                    {Object.entries(statistics.workflowsByCategory).map(([cat, count]) => (
                      <Box
                        key={cat}
                        sx={{
                          display: 'flex',
                          justifyContent: 'space-between',
                          py: 1,
                          borderBottom: '1px solid',
                          borderColor: 'divider',
                        }}
                      >
                        <Typography>{cat}</Typography>
                        <Chip label={count} size="small" />
                      </Box>
                    ))}
                  </CardContent>
                </Card>
              </Grid>
            )}

            {Object.keys(statistics.workflowsByEntityType).length > 0 && (
              <Grid item xs={12} md={6}>
                <Card>
                  <CardContent>
                    <Typography variant="h6" gutterBottom>
                      Workflows by Entity Type
                    </Typography>
                    {Object.entries(statistics.workflowsByEntityType).map(([type, count]) => (
                      <Box
                        key={type}
                        sx={{
                          display: 'flex',
                          justifyContent: 'space-between',
                          py: 1,
                          borderBottom: '1px solid',
                          borderColor: 'divider',
                        }}
                      >
                        <Typography>{type}</Typography>
                        <Chip label={count} size="small" />
                      </Box>
                    ))}
                  </CardContent>
                </Card>
              </Grid>
            )}
          </Grid>
        )}
      </TabPanel>

      {/* Create/Edit Workflow Dialog */}
      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle>
          {editingWorkflow ? 'Edit Workflow' : 'Create New Workflow'}
        </DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Workflow Key"
                value={formData.workflowKey}
                onChange={(e) =>
                  setFormData({ ...formData, workflowKey: e.target.value.replace(/\s/g, '_').toLowerCase() })
                }
                disabled={!!editingWorkflow}
                helperText="Unique identifier (lowercase, no spaces)"
                required
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                required
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                multiline
                rows={2}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth required>
                <InputLabel>Entity Type</InputLabel>
                <Select
                  value={formData.entityType}
                  onChange={(e) => setFormData({ ...formData, entityType: e.target.value })}
                  label="Entity Type"
                >
                  {entityTypes.map((et) => (
                    <MenuItem key={et.value} value={et.value}>
                      {et.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={6}>
              <Autocomplete
                freeSolo
                options={categories}
                value={formData.category || ''}
                onChange={(_, value) => setFormData({ ...formData, category: value || '' })}
                renderInput={(params) => (
                  <TextField {...params} label="Category" placeholder="Select or type new" />
                )}
              />
            </Grid>
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                type="number"
                label="Priority"
                value={formData.priority}
                onChange={(e) => setFormData({ ...formData, priority: parseInt(e.target.value) || 100 })}
                helperText="Lower = higher priority"
              />
            </Grid>
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                type="number"
                label="Max Concurrent Instances"
                value={formData.maxConcurrentInstances}
                onChange={(e) =>
                  setFormData({ ...formData, maxConcurrentInstances: parseInt(e.target.value) || 0 })
                }
                helperText="0 = unlimited"
              />
            </Grid>
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                type="number"
                label="Default Timeout (hours)"
                value={formData.defaultTimeoutHours}
                onChange={(e) =>
                  setFormData({ ...formData, defaultTimeoutHours: parseInt(e.target.value) || 0 })
                }
                helperText="0 = no timeout"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Icon</InputLabel>
                <Select
                  value={formData.iconName}
                  onChange={(e) => setFormData({ ...formData, iconName: e.target.value })}
                  label="Icon"
                >
                  {iconOptions.map((icon) => (
                    <MenuItem key={icon} value={icon}>
                      {icon}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Color
              </Typography>
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                {colorOptions.map((color) => (
                  <Box
                    key={color}
                    onClick={() => setFormData({ ...formData, color })}
                    sx={{
                      width: 32,
                      height: 32,
                      backgroundColor: color,
                      borderRadius: 1,
                      cursor: 'pointer',
                      border: formData.color === color ? '3px solid #000' : '1px solid #ccc',
                    }}
                  />
                ))}
              </Box>
            </Grid>
            <Grid item xs={12}>
              <Autocomplete
                multiple
                freeSolo
                options={[]}
                value={formData.tags || []}
                onChange={(_, value) => setFormData({ ...formData, tags: value })}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="Tags"
                    placeholder="Add tags and press Enter"
                  />
                )}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleSave}
            disabled={saving || !formData.workflowKey || !formData.name || !formData.entityType}
          >
            {saving ? <CircularProgress size={24} /> : editingWorkflow ? 'Save' : 'Create & Design'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Confirm Delete</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete the workflow "{workflowToDelete?.name}"?
            This action cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" color="error" onClick={handleDelete} disabled={saving}>
            {saving ? <CircularProgress size={24} /> : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default WorkflowListPage;
