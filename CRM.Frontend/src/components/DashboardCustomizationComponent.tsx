// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

import React from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  CircularProgress,
  Container,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  FormControlLabel,
  Grid,
  IconButton,
  List,
  ListItem,
  ListItemText,
  Paper,
  Stack,
  Switch,
  TextField,
  Typography,
  Alert,
  Chip
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  Edit as EditIcon,
  DragIndicator as DragIndicatorIcon,
  Settings as SettingsIcon,
  ContentCopy as ContentCopyIcon,
  Star as StarIcon,
  StarOutline as StarOutlineIcon
} from '@mui/icons-material';
import apiClient from '../services/apiClient';

interface DashboardWidget {
  id: string;
  type: string;
  title: string;
  position: { x: number; y: number; width: number; height: number };
  config?: any;
}

interface Dashboard {
  dashboardName: string;
  layoutConfig: any;
  widgets: DashboardWidget[];
  isDefault: boolean;
  gridColumns: number;
  autoRefresh: boolean;
  refreshIntervalSeconds: number;
  lastModified: string;
}

export const DashboardCustomizationComponent: React.FC<{ userId?: number }> = () => {
  const [dashboards, setDashboards] = React.useState<Dashboard[]>([]);
  const [selectedDashboard, setSelectedDashboard] = React.useState<Dashboard | null>(null);
  const [loading, setLoading] = React.useState(true);
  const [saving, setSaving] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);
  const [successMessage, setSuccessMessage] = React.useState<string | null>(null);
  const [newDashboardName, setNewDashboardName] = React.useState('');
  const [createDialogOpen, setCreateDialogOpen] = React.useState(false);
  const [editWidgetDialogOpen, setEditWidgetDialogOpen] = React.useState(false);
  const [editingWidget, setEditingWidget] = React.useState<DashboardWidget | null>(null);

  const AVAILABLE_WIDGET_TYPES = [
    'LineChart',
    'BarChart',
    'StatCard',
    'Table',
    'Calendar',
    'Timeline',
    'Map',
    'Weather',
    'TodoList',
    'Notes'
  ];

  React.useEffect(() => {
    loadDashboards();
  }, []);

  const loadDashboards = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get<Dashboard[]>('/api/ui-preferences/dashboards');
      setDashboards(response.data);
      if (response.data.length > 0) {
        const defaultDash = response.data.find(d => d.isDefault) || response.data[0];
        setSelectedDashboard(defaultDash);
      }
      setError(null);
    } catch (err) {
      console.error('Failed to load dashboards:', err);
      setError('Failed to load dashboard configurations');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateDashboard = async () => {
    if (!newDashboardName.trim()) return;

    setSaving(true);
    try {
      const newDashboard: Dashboard = {
        dashboardName: newDashboardName,
        layoutConfig: { version: '1.0' },
        widgets: [],
        isDefault: false,
        gridColumns: 12,
        autoRefresh: false,
        refreshIntervalSeconds: 30,
        lastModified: new Date().toISOString()
      };

      const response = await apiClient.post<Dashboard>('/api/ui-preferences/dashboards', newDashboard);
      setDashboards([...dashboards, response.data]);
      setSelectedDashboard(response.data);
      setNewDashboardName('');
      setCreateDialogOpen(false);
      setSuccessMessage('Dashboard created successfully');
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err) {
      setError('Failed to create dashboard');
    } finally {
      setSaving(false);
    }
  };

  const handleSetDefaultDashboard = async (dashboardName: string) => {
    setSaving(true);
    try {
      await apiClient.put(`/api/ui-preferences/dashboards/${dashboardName}/default`);
      await loadDashboards();
      setSuccessMessage(`${dashboardName} set as default`);
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err) {
      setError('Failed to set default dashboard');
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteDashboard = async (dashboardName: string) => {
    if (!window.confirm(`Delete dashboard "${dashboardName}"?`)) return;

    setSaving(true);
    try {
      await apiClient.delete(`/api/ui-preferences/dashboards/${dashboardName}`);
      const updated = dashboards.filter(d => d.dashboardName !== dashboardName);
      setDashboards(updated);
      if (selectedDashboard?.dashboardName === dashboardName) {
        setSelectedDashboard(updated[0] || null);
      }
      setSuccessMessage('Dashboard deleted');
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err) {
      setError('Failed to delete dashboard');
    } finally {
      setSaving(false);
    }
  };

  const handleAddWidget = (type: string) => {
    if (!selectedDashboard) return;

    const newWidget: DashboardWidget = {
      id: `widget-${Date.now()}`,
      type,
      title: `${type} Widget`,
      position: { x: 0, y: selectedDashboard.widgets.length, width: 4, height: 4 },
      config: {}
    };

    const updated = {
      ...selectedDashboard,
      widgets: [...selectedDashboard.widgets, newWidget],
      lastModified: new Date().toISOString()
    };

    saveDashboard(updated);
  };

  const handleRemoveWidget = (widgetId: string) => {
    if (!selectedDashboard) return;

    const updated = {
      ...selectedDashboard,
      widgets: selectedDashboard.widgets.filter(w => w.id !== widgetId),
      lastModified: new Date().toISOString()
    };

    saveDashboard(updated);
  };

  const handleEditWidget = (widget: DashboardWidget) => {
    setEditingWidget(widget);
    setEditWidgetDialogOpen(true);
  };

  const handleSaveWidget = () => {
    if (!selectedDashboard || !editingWidget) return;

    const updated = {
      ...selectedDashboard,
      widgets: selectedDashboard.widgets.map(w =>
        w.id === editingWidget.id ? editingWidget : w
      ),
      lastModified: new Date().toISOString()
    };

    saveDashboard(updated);
    setEditWidgetDialogOpen(false);
  };

  const handleAutoRefreshToggle = async () => {
    if (!selectedDashboard) return;

    const updated = {
      ...selectedDashboard,
      autoRefresh: !selectedDashboard.autoRefresh
    };

    await saveDashboard(updated);
  };

  const saveDashboard = async (dashboard: Dashboard) => {
    setSaving(true);
    try {
      const response = await apiClient.post<Dashboard>('/api/ui-preferences/dashboards', dashboard);
      setSelectedDashboard(response.data);
      setDashboards(dashboards.map((d: Dashboard) =>
        d.dashboardName === response.data.dashboardName ? response.data : d
      ));
      setError(null);
    } catch (err) {
      console.error('Failed to save dashboard:', err);
      setError('Failed to save dashboard');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center' }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" gutterBottom>Dashboard Customization</Typography>
        <Typography variant="body2" color="textSecondary">
          Create and customize your dashboards with widgets
        </Typography>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

      <Grid container spacing={3}>
        {/* Dashboards List */}
        <Grid item xs={12} md={3}>
          <Card>
            <CardHeader
              title="Dashboards"
              action={
                <Button
                  size="small"
                  startIcon={<AddIcon />}
                  onClick={() => setCreateDialogOpen(true)}
                >
                  New
                </Button>
              }
            />
            <CardContent>
              <List>
                {dashboards.map((dashboard) => (
                  <ListItem
                    key={dashboard.dashboardName}
                    button
                    selected={selectedDashboard?.dashboardName === dashboard.dashboardName}
                    onClick={() => setSelectedDashboard(dashboard)}
                    secondaryAction={
                      <Stack direction="row" spacing={0.5}>
                        {dashboard.isDefault && (
                          <StarIcon fontSize="small" color="primary" />
                        )}
                        <IconButton
                          edge="end"
                          size="small"
                          onClick={() => handleDeleteDashboard(dashboard.dashboardName)}
                        >
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </Stack>
                    }
                  >
                    <ListItemText
                      primary={dashboard.dashboardName}
                      secondary={`${dashboard.widgets.length} widgets`}
                    />
                  </ListItem>
                ))}
              </List>
            </CardContent>
          </Card>
        </Grid>

        {/* Dashboard Editor */}
        <Grid item xs={12} md={9}>
          {selectedDashboard ? (
            <Card>
              <CardHeader
                title={selectedDashboard.dashboardName}
                action={
                  <Stack direction="row" spacing={1}>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={selectedDashboard.autoRefresh}
                          onChange={handleAutoRefreshToggle}
                        />
                      }
                      label="Auto-refresh"
                    />
                    <IconButton
                      onClick={() => handleSetDefaultDashboard(selectedDashboard.dashboardName)}
                      disabled={selectedDashboard.isDefault}
                    >
                      {selectedDashboard.isDefault ? <StarIcon /> : <StarOutlineIcon />}
                    </IconButton>
                  </Stack>
                }
              />
              <Divider />
              <CardContent>
                {/* Widget Types */}
                <Box sx={{ mb: 3 }}>
                  <Typography variant="subtitle2" gutterBottom>Available Widgets</Typography>
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                    {AVAILABLE_WIDGET_TYPES.map((type) => (
                      <Button
                        key={type}
                        variant="outlined"
                        size="small"
                        startIcon={<AddIcon />}
                        onClick={() => handleAddWidget(type)}
                        disabled={saving}
                      >
                        {type}
                      </Button>
                    ))}
                  </Box>
                </Box>

                <Divider sx={{ my: 3 }} />

                {/* Widgets List */}
                <Typography variant="subtitle2" gutterBottom>Current Widgets ({selectedDashboard.widgets.length})</Typography>
                {selectedDashboard.widgets.length === 0 ? (
                  <Typography variant="body2" color="textSecondary">
                    No widgets. Add widgets using the buttons above.
                  </Typography>
                ) : (
                  <Stack spacing={2}>
                    {selectedDashboard.widgets.map((widget) => (
                      <Paper key={widget.id} sx={{ p: 2, backgroundColor: '#fafafa' }}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
                          <Box>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                              <DragIndicatorIcon fontSize="small" color="action" />
                              <Typography variant="subtitle2">{widget.title}</Typography>
                              <Chip label={widget.type} size="small" variant="outlined" />
                            </Box>
                            <Typography variant="caption" color="textSecondary" sx={{ mt: 0.5, display: 'block' }}>
                              Position: ({widget.position.x}, {widget.position.y}) | Size: {widget.position.width}x{widget.position.height}
                            </Typography>
                          </Box>
                          <Stack direction="row" spacing={0.5}>
                            <IconButton
                              size="small"
                              onClick={() => handleEditWidget(widget)}
                            >
                              <EditIcon fontSize="small" />
                            </IconButton>
                            <IconButton
                              size="small"
                              onClick={() => handleRemoveWidget(widget.id)}
                            >
                              <DeleteIcon fontSize="small" />
                            </IconButton>
                          </Stack>
                        </Box>
                      </Paper>
                    ))}
                  </Stack>
                )}
              </CardContent>
            </Card>
          ) : (
            <Card>
              <CardContent>
                <Typography color="textSecondary">
                  No dashboards available. Create one to get started.
                </Typography>
              </CardContent>
            </Card>
          )}
        </Grid>
      </Grid>

      {/* Create Dashboard Dialog */}
      <Dialog open={createDialogOpen} onClose={() => setCreateDialogOpen(false)}>
        <DialogTitle>Create New Dashboard</DialogTitle>
        <DialogContent sx={{ pt: 3 }}>
          <TextField
            autoFocus
            fullWidth
            label="Dashboard Name"
            value={newDashboardName}
            onChange={(e) => setNewDashboardName(e.target.value)}
            placeholder="e.g., Sales Dashboard"
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCreateDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleCreateDashboard} variant="contained" disabled={!newDashboardName.trim() || saving}>
            {saving ? 'Creating...' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Edit Widget Dialog */}
      {editingWidget && (
        <Dialog open={editWidgetDialogOpen} onClose={() => setEditWidgetDialogOpen(false)} maxWidth="sm" fullWidth>
          <DialogTitle>Edit Widget: {editingWidget.title}</DialogTitle>
          <DialogContent sx={{ pt: 3 }}>
            <Stack spacing={2}>
              <TextField
                fullWidth
                label="Title"
                value={editingWidget.title}
                onChange={(e) => setEditingWidget({ ...editingWidget, title: e.target.value })}
              />
              <TextField
                fullWidth
                label="X Position"
                type="number"
                value={editingWidget.position.x}
                onChange={(e) => setEditingWidget({
                  ...editingWidget,
                  position: { ...editingWidget.position, x: parseInt(e.target.value) }
                })}
              />
              <TextField
                fullWidth
                label="Y Position"
                type="number"
                value={editingWidget.position.y}
                onChange={(e) => setEditingWidget({
                  ...editingWidget,
                  position: { ...editingWidget.position, y: parseInt(e.target.value) }
                })}
              />
              <TextField
                fullWidth
                label="Width"
                type="number"
                value={editingWidget.position.width}
                onChange={(e) => setEditingWidget({
                  ...editingWidget,
                  position: { ...editingWidget.position, width: parseInt(e.target.value) }
                })}
              />
              <TextField
                fullWidth
                label="Height"
                type="number"
                value={editingWidget.position.height}
                onChange={(e) => setEditingWidget({
                  ...editingWidget,
                  position: { ...editingWidget.position, height: parseInt(e.target.value) }
                })}
              />
            </Stack>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setEditWidgetDialogOpen(false)}>Cancel</Button>
            <Button onClick={handleSaveWidget} variant="contained" disabled={saving}>
              {saving ? 'Saving...' : 'Save'}
            </Button>
          </DialogActions>
        </Dialog>
      )}
    </Container>
  );
};

export { DashboardCustomizationComponent };
export default DashboardCustomizationComponent;
