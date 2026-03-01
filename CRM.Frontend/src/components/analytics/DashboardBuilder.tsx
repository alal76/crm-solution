// TODO: Integration target — analytics/dashboard page
// This component is currently orphaned (not imported by any page).

/**
 * DashboardBuilder Component
 * 
 * A visual dashboard builder that allows users to create custom dashboards
 * by adding, arranging, and configuring widgets.
 */

import React, { useState, useCallback } from 'react';
import {
  Box,
  Paper,
  Typography,
  Button,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Grid,
  Card,
  CardContent,
  CardActions,
  Drawer,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Divider,
  Tooltip,
  Chip,
  Alert,
  Snackbar,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  Edit as EditIcon,
  Save as SaveIcon,
  Settings as SettingsIcon,
  Dashboard as DashboardIcon,
  BarChart as BarChartIcon,
  PieChart as PieChartIcon,
  ShowChart as LineChartIcon,
  TableChart as TableIcon,
  Numbers as NumberIcon,
  Timeline as TimelineIcon,
  DragIndicator as DragIcon,
  Close as CloseIcon,
  Refresh as RefreshIcon,
  Preview as PreviewIcon,
} from '@mui/icons-material';

// Widget types available for dashboard
export type WidgetType = 
  | 'bar-chart' 
  | 'pie-chart' 
  | 'line-chart' 
  | 'table' 
  | 'metric' 
  | 'timeline';

// Widget size options
export type WidgetSize = 'small' | 'medium' | 'large' | 'full';

// Data source options
export type DataSource = 
  | 'opportunities' 
  | 'leads' 
  | 'accounts' 
  | 'activities' 
  | 'revenue'
  | 'pipeline'
  | 'forecasts';

// Widget configuration
export interface WidgetConfig {
  id: string;
  type: WidgetType;
  title: string;
  dataSource: DataSource;
  size: WidgetSize;
  filters?: Record<string, string>;
  refreshInterval?: number;
  order: number;
}

// Dashboard configuration
export interface DashboardConfig {
  id?: number;
  name: string;
  description?: string;
  widgets: WidgetConfig[];
  isDefault?: boolean;
  createdAt?: string;
  updatedAt?: string;
}

// Props for the DashboardBuilder component
interface DashboardBuilderProps {
  dashboard?: DashboardConfig;
  onSave?: (dashboard: DashboardConfig) => Promise<void>;
  onCancel?: () => void;
}

// Widget type definitions
const WIDGET_TYPES: Array<{ type: WidgetType; label: string; icon: React.ReactNode; description: string }> = [
  { type: 'bar-chart', label: 'Bar Chart', icon: <BarChartIcon />, description: 'Compare values across categories' },
  { type: 'pie-chart', label: 'Pie Chart', icon: <PieChartIcon />, description: 'Show proportional data' },
  { type: 'line-chart', label: 'Line Chart', icon: <LineChartIcon />, description: 'Display trends over time' },
  { type: 'table', label: 'Data Table', icon: <TableIcon />, description: 'Display tabular data' },
  { type: 'metric', label: 'Metric Card', icon: <NumberIcon />, description: 'Show a single KPI value' },
  { type: 'timeline', label: 'Timeline', icon: <TimelineIcon />, description: 'Show activity timeline' },
];

// Data source definitions
const DATA_SOURCES: Array<{ source: DataSource; label: string }> = [
  { source: 'opportunities', label: 'Opportunities' },
  { source: 'leads', label: 'Leads' },
  { source: 'accounts', label: 'Accounts' },
  { source: 'activities', label: 'Activities' },
  { source: 'revenue', label: 'Revenue' },
  { source: 'pipeline', label: 'Pipeline' },
  { source: 'forecasts', label: 'Forecasts' },
];

// Size definitions
const SIZE_OPTIONS: Array<{ size: WidgetSize; label: string; gridSize: number }> = [
  { size: 'small', label: 'Small (1/4)', gridSize: 3 },
  { size: 'medium', label: 'Medium (1/2)', gridSize: 6 },
  { size: 'large', label: 'Large (3/4)', gridSize: 9 },
  { size: 'full', label: 'Full Width', gridSize: 12 },
];

// Generate unique ID
const generateId = () => `widget-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`; // NOSONAR - non-security use: UI element ID generation

// Default widget config
const getDefaultWidget = (): WidgetConfig => ({
  id: generateId(),
  type: 'metric',
  title: 'New Widget',
  dataSource: 'opportunities',
  size: 'medium',
  order: 0,
});

/**
 * DashboardBuilder Component
 */
export const DashboardBuilder: React.FC<DashboardBuilderProps> = ({
  dashboard,
  onSave,
  onCancel,
}) => {
  // State
  const [config, setConfig] = useState<DashboardConfig>(
    dashboard || { name: 'New Dashboard', widgets: [] }
  );
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [editingWidget, setEditingWidget] = useState<WidgetConfig | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [previewMode, setPreviewMode] = useState(false);
  const [saving, setSaving] = useState(false);
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({
    open: false,
    message: '',
    severity: 'success',
  });

  // Add widget
  const handleAddWidget = useCallback((type: WidgetType) => {
    const newWidget: WidgetConfig = {
      ...getDefaultWidget(),
      type,
      title: WIDGET_TYPES.find(w => w.type === type)?.label || 'New Widget',
      order: config.widgets.length,
    };
    
    setConfig(prev => ({
      ...prev,
      widgets: [...prev.widgets, newWidget],
    }));
    setDrawerOpen(false);
    setEditingWidget(newWidget);
    setDialogOpen(true);
  }, [config.widgets.length]);

  // Update widget
  const handleUpdateWidget = useCallback((updatedWidget: WidgetConfig) => {
    setConfig(prev => ({
      ...prev,
      widgets: prev.widgets.map(w => 
        w.id === updatedWidget.id ? updatedWidget : w
      ),
    }));
    setDialogOpen(false);
    setEditingWidget(null);
  }, []);

  // Delete widget
  const handleDeleteWidget = useCallback((widgetId: string) => {
    setConfig(prev => ({
      ...prev,
      widgets: prev.widgets.filter(w => w.id !== widgetId),
    }));
  }, []);

  // Edit widget
  const handleEditWidget = useCallback((widget: WidgetConfig) => {
    setEditingWidget({ ...widget });
    setDialogOpen(true);
  }, []);

  // Save dashboard
  const handleSave = useCallback(async () => {
    if (!config.name.trim()) {
      setSnackbar({ open: true, message: 'Dashboard name is required', severity: 'error' });
      return;
    }

    setSaving(true);
    try {
      if (onSave) {
        await onSave(config);
      }
      setSnackbar({ open: true, message: 'Dashboard saved successfully', severity: 'success' });
    } catch (error) {
      setSnackbar({ open: true, message: 'Failed to save dashboard', severity: 'error' });
    } finally {
      setSaving(false);
    }
  }, [config, onSave]);

  // Get grid size for widget
  const getGridSize = (size: WidgetSize): number => {
    return SIZE_OPTIONS.find(s => s.size === size)?.gridSize || 6;
  };

  // Render widget preview
  const renderWidgetPreview = (widget: WidgetConfig) => {
    const widgetType = WIDGET_TYPES.find(w => w.type === widget.type);
    
    return (
      <Card 
        sx={{ 
          height: '100%', 
          minHeight: widget.type === 'metric' ? 120 : 200,
          display: 'flex',
          flexDirection: 'column',
        }}
      >
        <CardContent sx={{ flexGrow: 1 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              {widgetType?.icon}
              <Typography variant="subtitle1" fontWeight="medium">
                {widget.title}
              </Typography>
            </Box>
            {!previewMode && (
              <Box>
                <IconButton size="small" onClick={() => handleEditWidget(widget)}>
                  <EditIcon fontSize="small" />
                </IconButton>
                <IconButton size="small" onClick={() => handleDeleteWidget(widget.id)} color="error">
                  <DeleteIcon fontSize="small" />
                </IconButton>
              </Box>
            )}
          </Box>
          <Chip 
            label={DATA_SOURCES.find(d => d.source === widget.dataSource)?.label} 
            size="small" 
            variant="outlined" 
            sx={{ mb: 1 }}
          />
          <Box 
            sx={{ 
              display: 'flex', 
              alignItems: 'center', 
              justifyContent: 'center', 
              height: widget.type === 'metric' ? 40 : 100,
              bgcolor: 'action.hover',
              borderRadius: 1,
            }}
          >
            <Typography color="text.secondary" variant="body2">
              {previewMode ? 'Loading data...' : 'Widget Preview'}
            </Typography>
          </Box>
        </CardContent>
        {!previewMode && (
          <CardActions sx={{ pt: 0, px: 2, pb: 1 }}>
            <DragIcon sx={{ color: 'text.disabled', cursor: 'move' }} />
            <Typography variant="caption" color="text.secondary">
              {SIZE_OPTIONS.find(s => s.size === widget.size)?.label}
            </Typography>
          </CardActions>
        )}
      </Card>
    );
  };

  return (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      {/* Header */}
      <Paper sx={{ p: 2, mb: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <DashboardIcon color="primary" />
            <TextField
              size="small"
              label="Dashboard Name"
              value={config.name}
              onChange={(e) => setConfig(prev => ({ ...prev, name: e.target.value }))}
              sx={{ minWidth: 250 }}
            />
            <TextField
              size="small"
              label="Description"
              value={config.description || ''}
              onChange={(e) => setConfig(prev => ({ ...prev, description: e.target.value }))}
              sx={{ minWidth: 300 }}
            />
          </Box>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button
              variant="outlined"
              startIcon={<PreviewIcon />}
              onClick={() => setPreviewMode(!previewMode)}
            >
              {previewMode ? 'Edit' : 'Preview'}
            </Button>
            <Button
              variant="outlined"
              startIcon={<AddIcon />}
              onClick={() => setDrawerOpen(true)}
              disabled={previewMode}
            >
              Add Widget
            </Button>
            <Button
              variant="contained"
              startIcon={<SaveIcon />}
              onClick={handleSave}
              disabled={saving}
            >
              {saving ? 'Saving...' : 'Save Dashboard'}
            </Button>
            {onCancel && (
              <Button onClick={onCancel}>
                Cancel
              </Button>
            )}
          </Box>
        </Box>
      </Paper>

      {/* Dashboard Canvas */}
      <Paper sx={{ flexGrow: 1, p: 2, overflow: 'auto' }}>
        {config.widgets.length === 0 ? (
          <Box
            sx={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              justifyContent: 'center',
              height: '100%',
              minHeight: 300,
            }}
          >
            <DashboardIcon sx={{ fontSize: 64, color: 'text.disabled', mb: 2 }} />
            <Typography variant="h6" color="text.secondary" gutterBottom>
              No Widgets Yet
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Click "Add Widget" to start building your dashboard
            </Typography>
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={() => setDrawerOpen(true)}
            >
              Add Your First Widget
            </Button>
          </Box>
        ) : (
          <Grid container spacing={2}>
            {config.widgets
              .sort((a, b) => a.order - b.order)
              .map((widget) => (
                <Grid item xs={12} md={getGridSize(widget.size)} key={widget.id}>
                  {renderWidgetPreview(widget)}
                </Grid>
              ))}
          </Grid>
        )}
      </Paper>

      {/* Widget Selector Drawer */}
      <Drawer
        anchor="right"
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
      >
        <Box sx={{ width: 320, p: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
            <Typography variant="h6">Add Widget</Typography>
            <IconButton onClick={() => setDrawerOpen(false)}>
              <CloseIcon />
            </IconButton>
          </Box>
          <Divider sx={{ mb: 2 }} />
          <List>
            {WIDGET_TYPES.map((widgetType) => (
              <ListItem
                key={widgetType.type}
                button
                onClick={() => handleAddWidget(widgetType.type)}
                sx={{ borderRadius: 1, mb: 1 }}
              >
                <ListItemIcon>{widgetType.icon}</ListItemIcon>
                <ListItemText
                  primary={widgetType.label}
                  secondary={widgetType.description}
                />
              </ListItem>
            ))}
          </List>
        </Box>
      </Drawer>

      {/* Widget Configuration Dialog */}
      <Dialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <SettingsIcon />
            Configure Widget
          </Box>
        </DialogTitle>
        <DialogContent>
          {editingWidget && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
              <TextField
                label="Widget Title"
                value={editingWidget.title}
                onChange={(e) => setEditingWidget(prev => prev ? { ...prev, title: e.target.value } : null)}
                fullWidth
              />
              <FormControl fullWidth>
                <InputLabel>Widget Type</InputLabel>
                <Select
                  value={editingWidget.type}
                  label="Widget Type"
                  onChange={(e) => setEditingWidget(prev => 
                    prev ? { ...prev, type: e.target.value as WidgetType } : null
                  )}
                >
                  {WIDGET_TYPES.map((wt) => (
                    <MenuItem key={wt.type} value={wt.type}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        {wt.icon}
                        {wt.label}
                      </Box>
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
              <FormControl fullWidth>
                <InputLabel>Data Source</InputLabel>
                <Select
                  value={editingWidget.dataSource}
                  label="Data Source"
                  onChange={(e) => setEditingWidget(prev => 
                    prev ? { ...prev, dataSource: e.target.value as DataSource } : null
                  )}
                >
                  {DATA_SOURCES.map((ds) => (
                    <MenuItem key={ds.source} value={ds.source}>{ds.label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
              <FormControl fullWidth>
                <InputLabel>Widget Size</InputLabel>
                <Select
                  value={editingWidget.size}
                  label="Widget Size"
                  onChange={(e) => setEditingWidget(prev => 
                    prev ? { ...prev, size: e.target.value as WidgetSize } : null
                  )}
                >
                  {SIZE_OPTIONS.map((so) => (
                    <MenuItem key={so.size} value={so.size}>{so.label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
              <FormControl fullWidth>
                <InputLabel>Auto-Refresh</InputLabel>
                <Select
                  value={editingWidget.refreshInterval || 0}
                  label="Auto-Refresh"
                  onChange={(e) => setEditingWidget(prev => 
                    prev ? { ...prev, refreshInterval: e.target.value as number } : null
                  )}
                >
                  <MenuItem value={0}>Manual Refresh</MenuItem>
                  <MenuItem value={30}>Every 30 seconds</MenuItem>
                  <MenuItem value={60}>Every minute</MenuItem>
                  <MenuItem value={300}>Every 5 minutes</MenuItem>
                  <MenuItem value={900}>Every 15 minutes</MenuItem>
                </Select>
              </FormControl>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={() => editingWidget && handleUpdateWidget(editingWidget)}
          >
            Save Widget
          </Button>
        </DialogActions>
      </Dialog>

      {/* Snackbar */}
      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar(prev => ({ ...prev, open: false }))}
      >
        <Alert severity={snackbar.severity} onClose={() => setSnackbar(prev => ({ ...prev, open: false }))}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default DashboardBuilder;
