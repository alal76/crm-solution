import React, { useState, useEffect } from 'react';
import {
  Box,
  Paper,
  Typography,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  IconButton,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Grid,
  Switch,
  FormControlLabel,
  Alert,
  Tooltip,
  Card,
  CardContent,
  Tabs,
  Tab,
  CircularProgress,
} from '@mui/material';
import {
  Edit as EditIcon,
  DragIndicator as DragIcon,
  Visibility as VisibilityIcon,
  VisibilityOff as VisibilityOffIcon,
  Settings as SettingsIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import { DragDropContext, Droppable, Draggable, DropResult } from 'react-beautiful-dnd';
import apiClient from '../../services/apiClient';

interface FieldConfig {
  id: number;
  moduleName: string;
  fieldName: string;
  fieldLabel: string;
  fieldType: string;
  tabIndex: number;
  tabName: string;
  displayOrder: number;
  isEnabled: boolean;
  isRequired: boolean;
  gridSize: number;
  placeholder?: string;
  helpText?: string;
  options?: string;
  parentField?: string;
  parentFieldValue?: string;
  isReorderable: boolean;
  isRequiredConfigurable: boolean;
  isHideable: boolean;
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div role="tabpanel" hidden={value !== index} {...other}>
      {value === index && <Box sx={{ pt: 2 }}>{children}</Box>}
    </div>
  );
}

const MODULES = [
  { value: 'Customers', label: 'Customers' },
  { value: 'Contacts', label: 'Contacts' },
  { value: 'Leads', label: 'Leads' },
  { value: 'Opportunities', label: 'Opportunities' },
];

const ModuleFieldSettingsTab: React.FC = () => {
  const [selectedModule, setSelectedModule] = useState('Customers');
  const [fields, setFields] = useState<FieldConfig[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [selectedTab, setSelectedTab] = useState(0);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [editingField, setEditingField] = useState<FieldConfig | null>(null);

  // Group fields by tab
  const tabs = Array.from(new Set(fields.map(f => f.tabName))).sort((a, b) => {
    const aIdx = fields.find(f => f.tabName === a)?.tabIndex || 0;
    const bIdx = fields.find(f => f.tabName === b)?.tabIndex || 0;
    return aIdx - bIdx;
  });

  const getFieldsForTab = (tabName: string) => {
    return fields
      .filter(f => f.tabName === tabName)
      .sort((a, b) => a.displayOrder - b.displayOrder);
  };

  useEffect(() => {
    loadFieldConfigurations();
  }, [selectedModule]);

  const loadFieldConfigurations = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await apiClient.get(`/modulefieldconfigurations/${selectedModule}`);
      setFields(response.data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load field configurations');
    } finally {
      setLoading(false);
    }
  };

  const handleInitializeDefaults = async () => {
    console.log('[ModuleFieldSettingsTab] Initialize Defaults clicked for module:', selectedModule);
    
    if (!selectedModule) {
      setError('Please select a module first');
      return;
    }

    const confirmed = window.confirm(`Initialize default field configurations for ${selectedModule}? This will only work if no configurations exist.`);
    console.log('[ModuleFieldSettingsTab] User confirmed:', confirmed);
    
    if (!confirmed) {
      return;
    }

    try {
      setLoading(true);
      setError(null);
      console.log('[ModuleFieldSettingsTab] Calling API to initialize:', selectedModule);
      const response = await apiClient.post(`/modulefieldconfigurations/initialize/${selectedModule}`);
      console.log('[ModuleFieldSettingsTab] API response:', response.data);
      setSuccess(`Default configurations initialized successfully for ${selectedModule}`);
      setTimeout(() => setSuccess(null), 5000);
      await loadFieldConfigurations();
    } catch (err: any) {
      console.error('[ModuleFieldSettingsTab] Initialize error:', err);
      const errorMsg = err.response?.data?.message || err.message || 'Failed to initialize default configurations';
      setError(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  const handleToggleEnabled = async (field: FieldConfig) => {
    if (!field.isHideable) {
      setError('This field cannot be hidden');
      setTimeout(() => setError(null), 3000);
      return;
    }

    try {
      await apiClient.put(`/modulefieldconfigurations/${field.id}`, {
        isEnabled: !field.isEnabled,
      });
      setFields(fields.map(f => f.id === field.id ? { ...f, isEnabled: !f.isEnabled } : f));
      setSuccess('Field visibility updated');
      setTimeout(() => setSuccess(null), 2000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update field');
    }
  };

  const handleToggleRequired = async (field: FieldConfig) => {
    if (!field.isRequiredConfigurable) {
      setError('This field\'s required status cannot be changed');
      setTimeout(() => setError(null), 3000);
      return;
    }

    try {
      await apiClient.put(`/modulefieldconfigurations/${field.id}`, {
        isRequired: !field.isRequired,
      });
      setFields(fields.map(f => f.id === field.id ? { ...f, isRequired: !f.isRequired } : f));
      setSuccess('Field requirement updated');
      setTimeout(() => setSuccess(null), 2000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update field');
    }
  };

  const handleEditField = (field: FieldConfig) => {
    setEditingField({ ...field });
    setEditDialogOpen(true);
  };

  const handleSaveFieldEdit = async () => {
    if (!editingField) return;

    try {
      await apiClient.put(`/modulefieldconfigurations/${editingField.id}`, {
        fieldLabel: editingField.fieldLabel,
        placeholder: editingField.placeholder,
        helpText: editingField.helpText,
        gridSize: editingField.gridSize,
      });
      setFields(fields.map(f => f.id === editingField.id ? editingField : f));
      setEditDialogOpen(false);
      setSuccess('Field updated successfully');
      setTimeout(() => setSuccess(null), 2000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update field');
    }
  };

  const handleDragEnd = async (result: DropResult, tabName: string) => {
    if (!result.destination) return;

    const tabFields = getFieldsForTab(tabName);
    const items = Array.from(tabFields);
    const [reorderedItem] = items.splice(result.source.index, 1);
    items.splice(result.destination.index, 0, reorderedItem);

    // Update display order
    const updatedFields = items.map((item, index) => ({
      ...item,
      displayOrder: index,
    }));

    // Optimistically update UI
    setFields(fields.map(f => {
      const updated = updatedFields.find(uf => uf.id === f.id);
      return updated || f;
    }));

    // Send bulk update to backend
    try {
      await apiClient.post('/modulefieldconfigurations/bulk-update-order', {
        moduleName: selectedModule,
        tabIndex: tabFields[0].tabIndex,
        fields: updatedFields.map(f => ({ id: f.id, displayOrder: f.displayOrder })),
      });
      setSuccess('Field order updated');
      setTimeout(() => setSuccess(null), 2000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update field order');
      // Reload on error
      loadFieldConfigurations();
    }
  };

  if (loading && fields.length === 0) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box>
          <Typography variant="h6" gutterBottom>
            Module Field Settings
          </Typography>
          <Typography variant="body2" color="textSecondary">
            Configure field visibility, order, and requirements for each module
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <FormControl sx={{ minWidth: 200 }}>
            <InputLabel>Module</InputLabel>
            <Select
              value={selectedModule}
              onChange={(e) => setSelectedModule(e.target.value)}
              label="Module"
            >
              {MODULES.map(module => (
                <MenuItem key={module.value} value={module.value}>{module.label}</MenuItem>
              ))}
            </Select>
          </FormControl>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={handleInitializeDefaults}
            disabled={loading}
          >
            Initialize Defaults
          </Button>
        </Box>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}

      {fields.length === 0 ? (
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 6 }}>
            <SettingsIcon sx={{ fontSize: 64, color: '#ccc', mb: 2 }} />
            <Typography variant="h6" gutterBottom>
              No field configurations found
            </Typography>
            <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
              Click "Initialize Defaults" to create default field configurations for {selectedModule}
            </Typography>
            <Button
              variant="contained"
              startIcon={<RefreshIcon />}
              onClick={handleInitializeDefaults}
              sx={{ backgroundColor: '#6750A4' }}
            >
              Initialize Defaults
            </Button>
          </CardContent>
        </Card>
      ) : (
        <Paper sx={{ borderRadius: 2 }}>
          <Tabs value={selectedTab} onChange={(_, v) => setSelectedTab(v)} sx={{ borderBottom: 1, borderColor: 'divider', px: 2 }}>
            {tabs.map((tabName, index) => (
              <Tab key={tabName} label={tabName} />
            ))}
          </Tabs>

          {tabs.map((tabName, index) => (
            <TabPanel key={tabName} value={selectedTab} index={index}>
              <Box sx={{ p: 2 }}>
                <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                  Drag to reorder • Toggle visibility • Configure requirements
                </Typography>

                <DragDropContext onDragEnd={(result) => handleDragEnd(result, tabName)}>
                  <Droppable droppableId={`fields-${tabName}`}>
                    {(provided) => (
                      <Table {...provided.droppableProps} ref={provided.innerRef}>
                        <TableHead>
                          <TableRow>
                            <TableCell width="40"></TableCell>
                            <TableCell><strong>Field</strong></TableCell>
                            <TableCell><strong>Type</strong></TableCell>
                            <TableCell><strong>Grid Size</strong></TableCell>
                            <TableCell><strong>Visible</strong></TableCell>
                            <TableCell><strong>Required</strong></TableCell>
                            <TableCell><strong>Actions</strong></TableCell>
                          </TableRow>
                        </TableHead>
                        <TableBody>
                          {getFieldsForTab(tabName).map((field, index) => (
                            <Draggable
                              key={field.id}
                              draggableId={field.id.toString()}
                              index={index}
                              isDragDisabled={!field.isReorderable}
                            >
                              {(provided, snapshot) => (
                                <TableRow
                                  ref={provided.innerRef}
                                  {...provided.draggableProps}
                                  sx={{
                                    backgroundColor: snapshot.isDragging ? '#f5f5f5' : 'transparent',
                                    opacity: field.isEnabled ? 1 : 0.5,
                                  }}
                                >
                                  <TableCell {...provided.dragHandleProps}>
                                    {field.isReorderable ? (
                                      <DragIcon sx={{ color: '#999', cursor: 'grab' }} />
                                    ) : (
                                      <Tooltip title="Cannot reorder this field">
                                        <DragIcon sx={{ color: '#ddd' }} />
                                      </Tooltip>
                                    )}
                                  </TableCell>
                                  <TableCell>
                                    <Box>
                                      <Typography variant="body2" fontWeight={500}>
                                        {field.fieldLabel}
                                      </Typography>
                                      <Typography variant="caption" color="textSecondary">
                                        {field.fieldName}
                                      </Typography>
                                      {field.parentField && (
                                        <Chip
                                          label={`Conditional: ${field.parentField}=${field.parentFieldValue}`}
                                          size="small"
                                          sx={{ mt: 0.5 }}
                                        />
                                      )}
                                    </Box>
                                  </TableCell>
                                  <TableCell>
                                    <Chip label={field.fieldType} size="small" variant="outlined" />
                                  </TableCell>
                                  <TableCell>
                                    <Chip label={`${field.gridSize}/12`} size="small" />
                                  </TableCell>
                                  <TableCell>
                                    <Tooltip title={field.isHideable ? 'Toggle visibility' : 'Cannot hide this field'}>
                                      <span>
                                        <IconButton
                                          size="small"
                                          onClick={() => handleToggleEnabled(field)}
                                          disabled={!field.isHideable}
                                          color={field.isEnabled ? 'primary' : 'default'}
                                        >
                                          {field.isEnabled ? <VisibilityIcon /> : <VisibilityOffIcon />}
                                        </IconButton>
                                      </span>
                                    </Tooltip>
                                  </TableCell>
                                  <TableCell>
                                    <Tooltip title={field.isRequiredConfigurable ? 'Toggle required' : 'Cannot change requirement'}>
                                      <span>
                                        <Switch
                                          checked={field.isRequired}
                                          onChange={() => handleToggleRequired(field)}
                                          disabled={!field.isRequiredConfigurable}
                                          size="small"
                                        />
                                      </span>
                                    </Tooltip>
                                  </TableCell>
                                  <TableCell>
                                    <Tooltip title="Edit field settings">
                                      <IconButton size="small" onClick={() => handleEditField(field)} sx={{ color: '#6750A4' }}>
                                        <EditIcon fontSize="small" />
                                      </IconButton>
                                    </Tooltip>
                                  </TableCell>
                                </TableRow>
                              )}
                            </Draggable>
                          ))}
                          {provided.placeholder}
                        </TableBody>
                      </Table>
                    )}
                  </Droppable>
                </DragDropContext>
              </Box>
            </TabPanel>
          ))}
        </Paper>
      )}

      {/* Edit Field Dialog */}
      <Dialog open={editDialogOpen} onClose={() => setEditDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Edit Field Settings</DialogTitle>
        <DialogContent>
          {editingField && (
            <Grid container spacing={2} sx={{ mt: 1 }}>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Field Label"
                  value={editingField.fieldLabel}
                  onChange={(e) => setEditingField({ ...editingField, fieldLabel: e.target.value })}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Placeholder"
                  value={editingField.placeholder || ''}
                  onChange={(e) => setEditingField({ ...editingField, placeholder: e.target.value })}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Help Text"
                  value={editingField.helpText || ''}
                  onChange={(e) => setEditingField({ ...editingField, helpText: e.target.value })}
                  multiline
                  rows={2}
                />
              </Grid>
              <Grid item xs={12}>
                <FormControl fullWidth>
                  <InputLabel>Grid Size (1-12)</InputLabel>
                  <Select
                    value={editingField.gridSize}
                    onChange={(e) => setEditingField({ ...editingField, gridSize: Number(e.target.value) })}
                    label="Grid Size (1-12)"
                  >
                    {[1, 2, 3, 4, 6, 12].map(size => (
                      <MenuItem key={size} value={size}>{size} columns</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
            </Grid>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleSaveFieldEdit} variant="contained" sx={{ backgroundColor: '#6750A4' }}>
            Save Changes
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default ModuleFieldSettingsTab;
