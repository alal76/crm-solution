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
  FormControlLabel,
  Switch,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Chip,
  Alert,
  CircularProgress,
  Tabs,
  Tab,
  Grid,
  Tooltip,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Slider,
  Divider,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Dashboard as DashboardIcon,
  Widgets as WidgetsIcon,
  Save as SaveIcon,
  ExpandMore as ExpandMoreIcon,
  Refresh as RefreshIcon,
  Star as StarIcon,
  StarBorder as StarBorderIcon,
  Visibility as VisibilityIcon,
  VisibilityOff as VisibilityOffIcon,
  DragIndicator as DragIcon,
  ContentCopy as CopyIcon,
} from '@mui/icons-material';
import {
  Dashboard,
  DashboardDetail,
  DashboardWidget,
  CreateDashboard,
  UpdateDashboard,
  CreateWidget,
  UpdateWidget,
  DataSource,
  dashboardConfigService,
  WidgetType,
  widgetTypeLabels,
} from '../../services/dashboardService';

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
  'Dashboard', 'TrendingUp', 'People', 'Assessment', 'BarChart', 'PieChart',
  'ShowChart', 'Analytics', 'Business', 'Work', 'AttachMoney', 'ShoppingCart',
  'Campaign', 'Help', 'Assignment', 'Build', 'Settings', 'Speed', 'Timeline'
];

const colorOptions = [
  '#6750A4', '#06A77D', '#0092BC', '#F57C00', '#9C27B0', '#2196F3',
  '#4CAF50', '#F44336', '#FF9800', '#795548', '#607607', '#E91E63'
];

function DashboardSettingsPage() {
  const [tabValue, setTabValue] = useState(0);
  const [dashboards, setDashboards] = useState<Dashboard[]>([]);
  const [selectedDashboard, setSelectedDashboard] = useState<DashboardDetail | null>(null);
  const [dataSources, setDataSources] = useState<DataSource[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Dashboard dialog state
  const [dashboardDialogOpen, setDashboardDialogOpen] = useState(false);
  const [editingDashboard, setEditingDashboard] = useState<Dashboard | null>(null);
  const [dashboardForm, setDashboardForm] = useState<CreateDashboard>({
    name: '',
    description: '',
    isDefault: false,
    isActive: true,
    iconName: 'Dashboard',
    displayOrder: 0,
    columnCount: 3,
    refreshIntervalSeconds: 300,
    visibility: 'Public',
    allowedRoles: '',
  });

  // Widget dialog state
  const [widgetDialogOpen, setWidgetDialogOpen] = useState(false);
  const [editingWidget, setEditingWidget] = useState<DashboardWidget | null>(null);
  const [widgetForm, setWidgetForm] = useState<CreateWidget>({
    dashboardId: 0,
    title: '',
    subtitle: '',
    widgetType: 'StatCard',
    dataSource: '',
    rowIndex: 0,
    columnIndex: 0,
    columnSpan: 1,
    rowSpan: 1,
    displayOrder: 0,
    isVisible: true,
    iconName: 'Assessment',
    color: '#6750A4',
    navigationLink: '',
    showTrend: false,
    trendPeriodDays: 30,
    refreshIntervalSeconds: 0,
  });

  const loadDashboards = useCallback(async () => {
    try {
      setLoading(true);
      const [dashboardsRes, dataSourcesRes] = await Promise.all([
        dashboardConfigService.getAllDashboards(),
        dashboardConfigService.getDataSources(),
      ]);
      setDashboards(dashboardsRes.data);
      setDataSources(dataSourcesRes.data);
      setError('');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load dashboards');
    } finally {
      setLoading(false);
    }
  }, []);

  const loadDashboardDetail = useCallback(async (id: number) => {
    try {
      const res = await dashboardConfigService.getDashboard(id);
      setSelectedDashboard(res.data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load dashboard details');
    }
  }, []);

  useEffect(() => {
    loadDashboards();
  }, [loadDashboards]);

  // Dashboard CRUD handlers
  const handleOpenDashboardDialog = (dashboard?: Dashboard) => {
    if (dashboard) {
      setEditingDashboard(dashboard);
      setDashboardForm({
        name: dashboard.name,
        description: dashboard.description || '',
        isDefault: dashboard.isDefault,
        isActive: dashboard.isActive,
        iconName: dashboard.iconName,
        displayOrder: dashboard.displayOrder,
        columnCount: dashboard.columnCount,
        refreshIntervalSeconds: dashboard.refreshIntervalSeconds,
        visibility: dashboard.visibility,
        allowedRoles: dashboard.allowedRoles || '',
      });
    } else {
      setEditingDashboard(null);
      setDashboardForm({
        name: '',
        description: '',
        isDefault: false,
        isActive: true,
        iconName: 'Dashboard',
        displayOrder: dashboards.length,
        columnCount: 3,
        refreshIntervalSeconds: 300,
        visibility: 'Public',
        allowedRoles: '',
      });
    }
    setDashboardDialogOpen(true);
  };

  const handleSaveDashboard = async () => {
    if (!dashboardForm.name.trim()) {
      setError('Dashboard name is required');
      return;
    }

    try {
      setSaving(true);
      if (editingDashboard) {
        await dashboardConfigService.updateDashboard(editingDashboard.id, dashboardForm as UpdateDashboard);
        setSuccess('Dashboard updated successfully');
      } else {
        await dashboardConfigService.createDashboard(dashboardForm);
        setSuccess('Dashboard created successfully');
      }
      setDashboardDialogOpen(false);
      loadDashboards();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save dashboard');
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteDashboard = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this dashboard?')) return;

    try {
      await dashboardConfigService.deleteDashboard(id);
      setSuccess('Dashboard deleted successfully');
      if (selectedDashboard?.id === id) {
        setSelectedDashboard(null);
      }
      loadDashboards();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete dashboard');
    }
  };

  const handleSetDefault = async (dashboard: Dashboard) => {
    try {
      await dashboardConfigService.updateDashboard(dashboard.id, { isDefault: true });
      setSuccess('Default dashboard updated');
      loadDashboards();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to set default');
    }
  };

  // Widget CRUD handlers
  const handleOpenWidgetDialog = (widget?: DashboardWidget) => {
    if (widget) {
      setEditingWidget(widget);
      setWidgetForm({
        dashboardId: selectedDashboard?.id || 0,
        title: widget.title,
        subtitle: widget.subtitle || '',
        widgetType: widget.widgetType,
        dataSource: widget.dataSource,
        rowIndex: widget.rowIndex,
        columnIndex: widget.columnIndex,
        columnSpan: widget.columnSpan,
        rowSpan: widget.rowSpan,
        displayOrder: widget.displayOrder,
        isVisible: widget.isVisible,
        iconName: widget.iconName || 'Assessment',
        color: widget.color || '#6750A4',
        backgroundColor: widget.backgroundColor || '',
        navigationLink: widget.navigationLink || '',
        showTrend: widget.showTrend,
        trendPeriodDays: widget.trendPeriodDays,
        refreshIntervalSeconds: widget.refreshIntervalSeconds,
      });
    } else {
      setEditingWidget(null);
      setWidgetForm({
        dashboardId: selectedDashboard?.id || 0,
        title: '',
        subtitle: '',
        widgetType: 'StatCard',
        dataSource: '',
        rowIndex: 0,
        columnIndex: 0,
        columnSpan: 1,
        rowSpan: 1,
        displayOrder: selectedDashboard?.widgets.length || 0,
        isVisible: true,
        iconName: 'Assessment',
        color: '#6750A4',
        navigationLink: '',
        showTrend: false,
        trendPeriodDays: 30,
        refreshIntervalSeconds: 0,
      });
    }
    setWidgetDialogOpen(true);
  };

  const handleSaveWidget = async () => {
    if (!widgetForm.title.trim()) {
      setError('Widget title is required');
      return;
    }

    try {
      setSaving(true);
      if (editingWidget) {
        await dashboardConfigService.updateWidget(editingWidget.id, widgetForm as UpdateWidget);
        setSuccess('Widget updated successfully');
      } else {
        await dashboardConfigService.createWidget(widgetForm);
        setSuccess('Widget created successfully');
      }
      setWidgetDialogOpen(false);
      if (selectedDashboard) {
        loadDashboardDetail(selectedDashboard.id);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save widget');
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteWidget = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this widget?')) return;

    try {
      await dashboardConfigService.deleteWidget(id);
      setSuccess('Widget deleted successfully');
      if (selectedDashboard) {
        loadDashboardDetail(selectedDashboard.id);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete widget');
    }
  };

  const handleToggleWidgetVisibility = async (widget: DashboardWidget) => {
    try {
      await dashboardConfigService.updateWidget(widget.id, { isVisible: !widget.isVisible });
      if (selectedDashboard) {
        loadDashboardDetail(selectedDashboard.id);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update widget');
    }
  };

  const handleInitializeDefaults = async () => {
    try {
      setSaving(true);
      const res = await dashboardConfigService.initializeDefaults();
      setSuccess(res.data.message);
      loadDashboards();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to initialize dashboards');
    } finally {
      setSaving(false);
    }
  };

  const groupDataSourcesByCategory = () => {
    const grouped: Record<string, DataSource[]> = {};
    dataSources.forEach(ds => {
      if (!grouped[ds.category]) {
        grouped[ds.category] = [];
      }
      grouped[ds.category].push(ds);
    });
    return grouped;
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 400 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <DashboardIcon sx={{ fontSize: 32, color: 'primary.main' }} />
          <Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>
              Dashboard Settings
            </Typography>
            <Typography variant="body2" color="textSecondary">
              Create and manage dashboards and widgets
            </Typography>
          </Box>
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={loadDashboards}
          >
            Refresh
          </Button>
          {dashboards.length === 0 && (
            <Button
              variant="outlined"
              color="secondary"
              onClick={handleInitializeDefaults}
              disabled={saving}
            >
              Initialize Defaults
            </Button>
          )}
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => handleOpenDashboardDialog()}
          >
            New Dashboard
          </Button>
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError('')}>
          {error}
        </Alert>
      )}
      {success && (
        <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccess('')}>
          {success}
        </Alert>
      )}

      <Tabs value={tabValue} onChange={(_, v) => setTabValue(v)} sx={{ mb: 2, borderBottom: 1, borderColor: 'divider' }}>
        <Tab label="Dashboards" icon={<DashboardIcon />} iconPosition="start" />
        <Tab label="Widget Editor" icon={<WidgetsIcon />} iconPosition="start" disabled={!selectedDashboard} />
      </Tabs>

      <TabPanel value={tabValue} index={0}>
        <TableContainer component={Paper} sx={{ borderRadius: 2 }}>
          <Table>
            <TableHead>
              <TableRow sx={{ backgroundColor: '#F5F5F5' }}>
                <TableCell sx={{ fontWeight: 600 }}>Name</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Description</TableCell>
                <TableCell align="center" sx={{ fontWeight: 600 }}>Widgets</TableCell>
                <TableCell align="center" sx={{ fontWeight: 600 }}>Status</TableCell>
                <TableCell align="center" sx={{ fontWeight: 600 }}>Visibility</TableCell>
                <TableCell align="center" sx={{ fontWeight: 600 }}>Default</TableCell>
                <TableCell align="right" sx={{ fontWeight: 600 }}>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {dashboards.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} align="center" sx={{ py: 4 }}>
                    <Typography color="textSecondary">No dashboards configured</Typography>
                    <Button
                      variant="text"
                      startIcon={<AddIcon />}
                      onClick={() => handleOpenDashboardDialog()}
                      sx={{ mt: 1 }}
                    >
                      Create your first dashboard
                    </Button>
                  </TableCell>
                </TableRow>
              ) : (
                dashboards.map((dashboard) => (
                  <TableRow
                    key={dashboard.id}
                    hover
                    sx={{
                      cursor: 'pointer',
                      backgroundColor: selectedDashboard?.id === dashboard.id ? 'action.selected' : 'inherit',
                    }}
                    onClick={() => {
                      loadDashboardDetail(dashboard.id);
                      setTabValue(1);
                    }}
                  >
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <DashboardIcon sx={{ color: 'primary.main' }} />
                        <Typography fontWeight={500}>{dashboard.name}</Typography>
                        {dashboard.isSystem && (
                          <Chip label="System" size="small" color="info" />
                        )}
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" color="textSecondary" sx={{ maxWidth: 200, overflow: 'hidden', textOverflow: 'ellipsis' }}>
                        {dashboard.description || '-'}
                      </Typography>
                    </TableCell>
                    <TableCell align="center">
                      <Chip label={dashboard.widgetCount} size="small" />
                    </TableCell>
                    <TableCell align="center">
                      <Chip
                        label={dashboard.isActive ? 'Active' : 'Inactive'}
                        color={dashboard.isActive ? 'success' : 'default'}
                        size="small"
                      />
                    </TableCell>
                    <TableCell align="center">
                      <Chip
                        label={dashboard.visibility}
                        size="small"
                        variant="outlined"
                      />
                    </TableCell>
                    <TableCell align="center">
                      <IconButton
                        size="small"
                        onClick={(e) => {
                          e.stopPropagation();
                          if (!dashboard.isDefault) handleSetDefault(dashboard);
                        }}
                        color={dashboard.isDefault ? 'warning' : 'default'}
                      >
                        {dashboard.isDefault ? <StarIcon /> : <StarBorderIcon />}
                      </IconButton>
                    </TableCell>
                    <TableCell align="right">
                      <IconButton
                        size="small"
                        onClick={(e) => {
                          e.stopPropagation();
                          handleOpenDashboardDialog(dashboard);
                        }}
                      >
                        <EditIcon />
                      </IconButton>
                      {!dashboard.isSystem && (
                        <IconButton
                          size="small"
                          color="error"
                          onClick={(e) => {
                            e.stopPropagation();
                            handleDeleteDashboard(dashboard.id);
                          }}
                        >
                          <DeleteIcon />
                        </IconButton>
                      )}
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </TabPanel>

      <TabPanel value={tabValue} index={1}>
        {selectedDashboard ? (
          <Box>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
              <Box>
                <Typography variant="h5" sx={{ fontWeight: 600 }}>
                  {selectedDashboard.name}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  {selectedDashboard.widgets.length} widgets · {selectedDashboard.columnCount} columns
                </Typography>
              </Box>
              <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={() => handleOpenWidgetDialog()}
              >
                Add Widget
              </Button>
            </Box>

            <Grid container spacing={2}>
              {selectedDashboard.widgets.map((widget) => (
                <Grid item xs={12} sm={6} md={4} key={widget.id}>
                  <Card
                    sx={{
                      borderRadius: 2,
                      border: '1px solid',
                      borderColor: widget.isVisible ? 'divider' : 'error.light',
                      opacity: widget.isVisible ? 1 : 0.6,
                    }}
                  >
                    <CardContent>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Box
                            sx={{
                              width: 8,
                              height: 8,
                              borderRadius: '50%',
                              backgroundColor: widget.color || '#6750A4',
                            }}
                          />
                          <Typography variant="subtitle1" fontWeight={600}>
                            {widget.title}
                          </Typography>
                        </Box>
                        <Chip
                          label={widget.widgetType}
                          size="small"
                          variant="outlined"
                          sx={{ fontSize: '0.7rem' }}
                        />
                      </Box>
                      {widget.subtitle && (
                        <Typography variant="body2" color="textSecondary" sx={{ mb: 1 }}>
                          {widget.subtitle}
                        </Typography>
                      )}
                      <Typography variant="caption" color="textSecondary" display="block">
                        Data: {widget.dataSource || 'None'}
                      </Typography>
                      <Typography variant="caption" color="textSecondary" display="block">
                        Size: {widget.columnSpan}x{widget.rowSpan} · Order: {widget.displayOrder}
                      </Typography>
                      <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 0.5, mt: 2 }}>
                        <Tooltip title={widget.isVisible ? 'Hide widget' : 'Show widget'}>
                          <IconButton size="small" onClick={() => handleToggleWidgetVisibility(widget)}>
                            {widget.isVisible ? <VisibilityIcon /> : <VisibilityOffIcon />}
                          </IconButton>
                        </Tooltip>
                        <IconButton size="small" onClick={() => handleOpenWidgetDialog(widget)}>
                          <EditIcon />
                        </IconButton>
                        <IconButton size="small" color="error" onClick={() => handleDeleteWidget(widget.id)}>
                          <DeleteIcon />
                        </IconButton>
                      </Box>
                    </CardContent>
                  </Card>
                </Grid>
              ))}
            </Grid>

            {selectedDashboard.widgets.length === 0 && (
              <Box sx={{ textAlign: 'center', py: 8 }}>
                <WidgetsIcon sx={{ fontSize: 64, color: 'text.disabled', mb: 2 }} />
                <Typography color="textSecondary">No widgets configured</Typography>
                <Button
                  variant="text"
                  startIcon={<AddIcon />}
                  onClick={() => handleOpenWidgetDialog()}
                  sx={{ mt: 1 }}
                >
                  Add your first widget
                </Button>
              </Box>
            )}
          </Box>
        ) : (
          <Box sx={{ textAlign: 'center', py: 8 }}>
            <Typography color="textSecondary">Select a dashboard to edit widgets</Typography>
          </Box>
        )}
      </TabPanel>

      {/* Dashboard Dialog */}
      <Dialog open={dashboardDialogOpen} onClose={() => setDashboardDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          {editingDashboard ? 'Edit Dashboard' : 'Create Dashboard'}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
            <TextField
              label="Name"
              value={dashboardForm.name}
              onChange={(e) => setDashboardForm({ ...dashboardForm, name: e.target.value })}
              fullWidth
              required
            />
            <TextField
              label="Description"
              value={dashboardForm.description}
              onChange={(e) => setDashboardForm({ ...dashboardForm, description: e.target.value })}
              fullWidth
              multiline
              rows={2}
            />
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <FormControl fullWidth>
                  <InputLabel>Icon</InputLabel>
                  <Select
                    value={dashboardForm.iconName}
                    label="Icon"
                    onChange={(e) => setDashboardForm({ ...dashboardForm, iconName: e.target.value })}
                  >
                    {iconOptions.map((icon) => (
                      <MenuItem key={icon} value={icon}>{icon}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={6}>
                <FormControl fullWidth>
                  <InputLabel>Columns</InputLabel>
                  <Select
                    value={dashboardForm.columnCount}
                    label="Columns"
                    onChange={(e) => setDashboardForm({ ...dashboardForm, columnCount: Number(e.target.value) })}
                  >
                    {[1, 2, 3, 4].map((n) => (
                      <MenuItem key={n} value={n}>{n} Column{n > 1 ? 's' : ''}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
            </Grid>
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <TextField
                  label="Display Order"
                  type="number"
                  value={dashboardForm.displayOrder}
                  onChange={(e) => setDashboardForm({ ...dashboardForm, displayOrder: Number(e.target.value) })}
                  fullWidth
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  label="Refresh Interval (seconds)"
                  type="number"
                  value={dashboardForm.refreshIntervalSeconds}
                  onChange={(e) => setDashboardForm({ ...dashboardForm, refreshIntervalSeconds: Number(e.target.value) })}
                  fullWidth
                />
              </Grid>
            </Grid>
            <FormControl fullWidth>
              <InputLabel>Visibility</InputLabel>
              <Select
                value={dashboardForm.visibility}
                label="Visibility"
                onChange={(e) => setDashboardForm({ ...dashboardForm, visibility: e.target.value })}
              >
                <MenuItem value="Public">Public (All Users)</MenuItem>
                <MenuItem value="Private">Private (Owner Only)</MenuItem>
                <MenuItem value="RoleBased">Role-Based</MenuItem>
              </Select>
            </FormControl>
            {dashboardForm.visibility === 'RoleBased' && (
              <TextField
                label="Allowed Roles (comma-separated)"
                value={dashboardForm.allowedRoles}
                onChange={(e) => setDashboardForm({ ...dashboardForm, allowedRoles: e.target.value })}
                fullWidth
                placeholder="Admin, Manager, Sales"
              />
            )}
            <Box sx={{ display: 'flex', gap: 2 }}>
              <FormControlLabel
                control={
                  <Switch
                    checked={dashboardForm.isActive}
                    onChange={(e) => setDashboardForm({ ...dashboardForm, isActive: e.target.checked })}
                  />
                }
                label="Active"
              />
              <FormControlLabel
                control={
                  <Switch
                    checked={dashboardForm.isDefault}
                    onChange={(e) => setDashboardForm({ ...dashboardForm, isDefault: e.target.checked })}
                  />
                }
                label="Default Dashboard"
              />
            </Box>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDashboardDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleSaveDashboard}
            disabled={saving}
            startIcon={saving ? <CircularProgress size={16} /> : <SaveIcon />}
          >
            {saving ? 'Saving...' : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Widget Dialog */}
      <Dialog open={widgetDialogOpen} onClose={() => setWidgetDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          {editingWidget ? 'Edit Widget' : 'Add Widget'}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
            <Grid container spacing={2}>
              <Grid item xs={8}>
                <TextField
                  label="Title"
                  value={widgetForm.title}
                  onChange={(e) => setWidgetForm({ ...widgetForm, title: e.target.value })}
                  fullWidth
                  required
                />
              </Grid>
              <Grid item xs={4}>
                <FormControl fullWidth>
                  <InputLabel>Widget Type</InputLabel>
                  <Select
                    value={widgetForm.widgetType}
                    label="Widget Type"
                    onChange={(e) => setWidgetForm({ ...widgetForm, widgetType: e.target.value })}
                  >
                    {Object.entries(widgetTypeLabels).map(([value, label]) => (
                      <MenuItem key={value} value={WidgetType[Number(value) as WidgetType]}>
                        {label}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
            </Grid>

            <TextField
              label="Subtitle"
              value={widgetForm.subtitle}
              onChange={(e) => setWidgetForm({ ...widgetForm, subtitle: e.target.value })}
              fullWidth
            />

            <Accordion defaultExpanded>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography fontWeight={500}>Data Source</Typography>
              </AccordionSummary>
              <AccordionDetails>
                <FormControl fullWidth>
                  <InputLabel>Data Source</InputLabel>
                  <Select
                    value={widgetForm.dataSource}
                    label="Data Source"
                    onChange={(e) => setWidgetForm({ ...widgetForm, dataSource: e.target.value })}
                  >
                    <MenuItem value="">None</MenuItem>
                    {Object.entries(groupDataSourcesByCategory()).map(([category, sources]) => [
                      <MenuItem key={category} disabled sx={{ fontWeight: 600, color: 'primary.main' }}>
                        {category}
                      </MenuItem>,
                      ...sources.map((ds) => (
                        <MenuItem key={ds.id} value={ds.id} sx={{ pl: 4 }}>
                          {ds.name}
                        </MenuItem>
                      ))
                    ])}
                  </Select>
                </FormControl>
              </AccordionDetails>
            </Accordion>

            <Accordion>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography fontWeight={500}>Layout & Position</Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Grid container spacing={2}>
                  <Grid item xs={3}>
                    <TextField
                      label="Row"
                      type="number"
                      value={widgetForm.rowIndex}
                      onChange={(e) => setWidgetForm({ ...widgetForm, rowIndex: Number(e.target.value) })}
                      fullWidth
                      inputProps={{ min: 0 }}
                    />
                  </Grid>
                  <Grid item xs={3}>
                    <TextField
                      label="Column"
                      type="number"
                      value={widgetForm.columnIndex}
                      onChange={(e) => setWidgetForm({ ...widgetForm, columnIndex: Number(e.target.value) })}
                      fullWidth
                      inputProps={{ min: 0, max: 3 }}
                    />
                  </Grid>
                  <Grid item xs={3}>
                    <TextField
                      label="Column Span"
                      type="number"
                      value={widgetForm.columnSpan}
                      onChange={(e) => setWidgetForm({ ...widgetForm, columnSpan: Number(e.target.value) })}
                      fullWidth
                      inputProps={{ min: 1, max: 4 }}
                    />
                  </Grid>
                  <Grid item xs={3}>
                    <TextField
                      label="Row Span"
                      type="number"
                      value={widgetForm.rowSpan}
                      onChange={(e) => setWidgetForm({ ...widgetForm, rowSpan: Number(e.target.value) })}
                      fullWidth
                      inputProps={{ min: 1, max: 4 }}
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <TextField
                      label="Display Order"
                      type="number"
                      value={widgetForm.displayOrder}
                      onChange={(e) => setWidgetForm({ ...widgetForm, displayOrder: Number(e.target.value) })}
                      fullWidth
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={widgetForm.isVisible}
                          onChange={(e) => setWidgetForm({ ...widgetForm, isVisible: e.target.checked })}
                        />
                      }
                      label="Visible"
                    />
                  </Grid>
                </Grid>
              </AccordionDetails>
            </Accordion>

            <Accordion>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography fontWeight={500}>Appearance</Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Grid container spacing={2}>
                  <Grid item xs={6}>
                    <FormControl fullWidth>
                      <InputLabel>Icon</InputLabel>
                      <Select
                        value={widgetForm.iconName || ''}
                        label="Icon"
                        onChange={(e) => setWidgetForm({ ...widgetForm, iconName: e.target.value })}
                      >
                        <MenuItem value="">None</MenuItem>
                        {iconOptions.map((icon) => (
                          <MenuItem key={icon} value={icon}>{icon}</MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body2" sx={{ mb: 1 }}>Color</Typography>
                    <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
                      {colorOptions.map((color) => (
                        <Box
                          key={color}
                          onClick={() => setWidgetForm({ ...widgetForm, color })}
                          sx={{
                            width: 28,
                            height: 28,
                            borderRadius: 1,
                            backgroundColor: color,
                            cursor: 'pointer',
                            border: widgetForm.color === color ? '3px solid #000' : '1px solid #ccc',
                          }}
                        />
                      ))}
                    </Box>
                  </Grid>
                  <Grid item xs={12}>
                    <TextField
                      label="Navigation Link"
                      value={widgetForm.navigationLink}
                      onChange={(e) => setWidgetForm({ ...widgetForm, navigationLink: e.target.value })}
                      fullWidth
                      placeholder="/opportunities"
                    />
                  </Grid>
                </Grid>
              </AccordionDetails>
            </Accordion>

            <Accordion>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography fontWeight={500}>Trend & Refresh</Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Grid container spacing={2}>
                  <Grid item xs={6}>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={widgetForm.showTrend}
                          onChange={(e) => setWidgetForm({ ...widgetForm, showTrend: e.target.checked })}
                        />
                      }
                      label="Show Trend Indicator"
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <TextField
                      label="Trend Period (days)"
                      type="number"
                      value={widgetForm.trendPeriodDays}
                      onChange={(e) => setWidgetForm({ ...widgetForm, trendPeriodDays: Number(e.target.value) })}
                      fullWidth
                      disabled={!widgetForm.showTrend}
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <TextField
                      label="Refresh Interval (seconds, 0 = use dashboard default)"
                      type="number"
                      value={widgetForm.refreshIntervalSeconds}
                      onChange={(e) => setWidgetForm({ ...widgetForm, refreshIntervalSeconds: Number(e.target.value) })}
                      fullWidth
                    />
                  </Grid>
                </Grid>
              </AccordionDetails>
            </Accordion>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setWidgetDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleSaveWidget}
            disabled={saving}
            startIcon={saving ? <CircularProgress size={16} /> : <SaveIcon />}
          >
            {saving ? 'Saving...' : 'Save Widget'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default DashboardSettingsPage;
