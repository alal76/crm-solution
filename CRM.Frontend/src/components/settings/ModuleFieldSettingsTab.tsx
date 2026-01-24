import React, { useState, useEffect, useCallback } from 'react';
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
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  Divider,
  Autocomplete,
} from '@mui/material';
import {
  Edit as EditIcon,
  DragIndicator as DragIcon,
  Visibility as VisibilityIcon,
  VisibilityOff as VisibilityOffIcon,
  Settings as SettingsIcon,
  Refresh as RefreshIcon,
  Add as AddIcon,
  Delete as DeleteIcon,
  Link as LinkIcon,
  Storage as StorageIcon,
  CheckCircle as CheckCircleIcon,
  Block as BlockIcon,
  AccountTree as AccountTreeIcon,
  Save as SaveIcon,
} from '@mui/icons-material';
import { DragDropContext, Droppable, Draggable, DropResult } from 'react-beautiful-dnd';
import apiClient from '../../services/apiClient';
import fieldMasterDataService, {
  FieldMasterDataLink,
  MasterDataSource,
  CreateFieldMasterDataLink,
} from '../../services/fieldMasterDataService';

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
  
  // Master data linking state
  const [editDialogTab, setEditDialogTab] = useState(0);
  const [masterDataLinks, setMasterDataLinks] = useState<FieldMasterDataLink[]>([]);
  const [availableSources, setAvailableSources] = useState<MasterDataSource[]>([]);
  const [loadingLinks, setLoadingLinks] = useState(false);
  const [newLink, setNewLink] = useState<Partial<CreateFieldMasterDataLink>>({});
  const [showAddLink, setShowAddLink] = useState(false);
  const [allFieldsMap, setAllFieldsMap] = useState<Record<number, FieldConfig>>({});
  
  // Module-level links map for showing icons in the field list
  const [moduleMasterDataLinks, setModuleMasterDataLinks] = useState<Record<number, FieldMasterDataLink[]>>({});
  const [editingLink, setEditingLink] = useState<FieldMasterDataLink | null>(null);

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

  // Helper to check if a field has master data links
  const getFieldLinks = (fieldId: number): FieldMasterDataLink[] => {
    return moduleMasterDataLinks[fieldId] || [];
  };

  const hasEntityLink = (fieldId: number): boolean => {
    return getFieldLinks(fieldId).some(link => link.sourceType === 'Table');
  };

  const hasMasterDataValidation = (fieldId: number): boolean => {
    return getFieldLinks(fieldId).some(link => 
      link.sourceType === 'LookupCategory' || 
      (link.validationType && link.validationType !== '')
    );
  };

  const isDataRestricted = (fieldId: number): boolean => {
    return getFieldLinks(fieldId).some(link => !link.allowFreeText);
  };

  useEffect(() => {
    loadFieldConfigurations();
  }, [selectedModule]);

  // Load module-level master data links when module changes
  useEffect(() => {
    const loadModuleLinks = async () => {
      try {
        const links = await fieldMasterDataService.getLinksForModule(selectedModule);
        setModuleMasterDataLinks(links);
      } catch (err) {
        console.error('Failed to load module master data links:', err);
      }
    };
    if (selectedModule) {
      loadModuleLinks();
    }
  }, [selectedModule, fields]);

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

  const handleEditField = async (field: FieldConfig) => {
    setEditingField({ ...field });
    setEditDialogTab(0);
    setShowAddLink(false);
    setNewLink({});
    setEditingLink(null);
    setEditDialogOpen(true);
    
    // Load master data links for this field
    setLoadingLinks(true);
    try {
      const [links, sources] = await Promise.all([
        fieldMasterDataService.getLinksForField(field.id),
        fieldMasterDataService.getAvailableSources(),
      ]);
      setMasterDataLinks(links);
      setAvailableSources(sources);
    } catch (err) {
      console.error('Failed to load master data links:', err);
    } finally {
      setLoadingLinks(false);
    }
  };

  // Build a map of all fields for dependent field selection
  useEffect(() => {
    const map: Record<number, FieldConfig> = {};
    fields.forEach(f => { map[f.id] = f; });
    setAllFieldsMap(map);
  }, [fields]);

  const handleAddMasterDataLink = async () => {
    if (!editingField || !newLink.sourceType || !newLink.sourceName) return;

    try {
      const created = await fieldMasterDataService.createLink({
        fieldConfigurationId: editingField.id,
        sourceType: newLink.sourceType,
        sourceName: newLink.sourceName,
        displayField: newLink.displayField,
        valueField: newLink.valueField,
        filterExpression: newLink.filterExpression,
        dependsOnField: newLink.dependsOnField,
        dependsOnSourceColumn: newLink.dependsOnSourceColumn,
        allowFreeText: newLink.allowFreeText ?? false,
        validationType: newLink.validationType,
        validationPattern: newLink.validationPattern,
        validationMessage: newLink.validationMessage,
        sortOrder: masterDataLinks.length,
        isActive: true,
      });
      setMasterDataLinks([...masterDataLinks, created]);
      
      // Also update the module-level links for the icons
      setModuleMasterDataLinks(prev => ({
        ...prev,
        [editingField.id]: [...(prev[editingField.id] || []), created]
      }));
      
      setShowAddLink(false);
      setNewLink({});
      setSuccess('Master data link added');
      setTimeout(() => setSuccess(null), 2000);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to add master data link');
    }
  };

  const handleDeleteMasterDataLink = async (linkId: number) => {
    try {
      await fieldMasterDataService.deleteLink(linkId);
      setMasterDataLinks(masterDataLinks.filter(l => l.id !== linkId));
      // Also update the module-level links
      if (editingField) {
        setModuleMasterDataLinks(prev => ({
          ...prev,
          [editingField.id]: (prev[editingField.id] || []).filter(l => l.id !== linkId)
        }));
      }
      setSuccess('Master data link removed');
      setTimeout(() => setSuccess(null), 2000);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to delete master data link');
    }
  };

  const handleSaveEditedLink = async () => {
    if (!editingLink || !editingField) return;

    try {
      const updated = await fieldMasterDataService.updateLink(editingLink.id, {
        fieldConfigurationId: editingField.id,
        sourceType: editingLink.sourceType,
        sourceName: editingLink.sourceName,
        displayField: editingLink.displayField,
        valueField: editingLink.valueField,
        filterExpression: editingLink.filterExpression,
        dependsOnField: editingLink.dependsOnField,
        dependsOnSourceColumn: editingLink.dependsOnSourceColumn,
        allowFreeText: editingLink.allowFreeText,
        validationType: editingLink.validationType,
        validationPattern: editingLink.validationPattern,
        validationMessage: editingLink.validationMessage,
        sortOrder: editingLink.sortOrder,
        isActive: editingLink.isActive,
      });
      
      // Update local state
      setMasterDataLinks(masterDataLinks.map(l => l.id === updated.id ? updated : l));
      
      // Update module-level links
      setModuleMasterDataLinks(prev => ({
        ...prev,
        [editingField.id]: (prev[editingField.id] || []).map(l => l.id === updated.id ? updated : l)
      }));
      
      setEditingLink(null);
      setSuccess('Master data link updated');
      setTimeout(() => setSuccess(null), 2000);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to update master data link');
    }
  };

  const getSelectedSource = (): MasterDataSource | undefined => {
    if (!newLink.sourceType || !newLink.sourceName) return undefined;
    return availableSources.find(
      s => s.sourceType === newLink.sourceType && s.sourceName === newLink.sourceName
    );
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
                  Drag to reorder • Toggle visibility • Configure requirements • 
                  <Tooltip title="Linked to entity">
                    <AccountTreeIcon sx={{ fontSize: 14, mx: 0.5, color: '#1976d2' }} />
                  </Tooltip>Entity Link
                  <Tooltip title="Master data validation">
                    <StorageIcon sx={{ fontSize: 14, mx: 0.5, color: '#9c27b0' }} />
                  </Tooltip>Master Data
                  <Tooltip title="Restricted (no free text)">
                    <BlockIcon sx={{ fontSize: 14, mx: 0.5, color: '#f44336' }} />
                  </Tooltip>Restricted
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
                            <TableCell><strong>Links</strong></TableCell>
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
                                    <Box sx={{ display: 'flex', gap: 0.5 }}>
                                      {hasEntityLink(field.id) && (
                                        <Tooltip title={`Linked to entity: ${getFieldLinks(field.id).filter(l => l.sourceType === 'Table').map(l => l.sourceName).join(', ')}`}>
                                          <AccountTreeIcon sx={{ fontSize: 18, color: '#1976d2' }} />
                                        </Tooltip>
                                      )}
                                      {hasMasterDataValidation(field.id) && (
                                        <Tooltip title={`Master data: ${getFieldLinks(field.id).filter(l => l.sourceType === 'LookupCategory').map(l => l.sourceName).join(', ')}`}>
                                          <StorageIcon sx={{ fontSize: 18, color: '#9c27b0' }} />
                                        </Tooltip>
                                      )}
                                      {isDataRestricted(field.id) && (
                                        <Tooltip title="Data entry restricted to master data values">
                                          <BlockIcon sx={{ fontSize: 18, color: '#f44336' }} />
                                        </Tooltip>
                                      )}
                                      {getFieldLinks(field.id).length === 0 && (
                                        <Typography variant="caption" color="textSecondary">-</Typography>
                                      )}
                                    </Box>
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
      <Dialog open={editDialogOpen} onClose={() => setEditDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          Edit Field Settings
          {editingField && (
            <Typography variant="caption" display="block" color="textSecondary">
              {editingField.fieldLabel} ({editingField.fieldName})
            </Typography>
          )}
        </DialogTitle>
        <DialogContent>
          <Tabs value={editDialogTab} onChange={(_, v) => setEditDialogTab(v)} sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
            <Tab label="General" />
            <Tab label="Data Source & Validation" icon={<LinkIcon fontSize="small" />} iconPosition="start" />
          </Tabs>

          {editingField && editDialogTab === 0 && (
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

          {editingField && editDialogTab === 1 && (
            <Box sx={{ mt: 1 }}>
              <Typography variant="subtitle2" gutterBottom>
                Link this field to master data for dropdown options and validation
              </Typography>

              {loadingLinks ? (
                <Box sx={{ display: 'flex', justifyContent: 'center', py: 3 }}>
                  <CircularProgress size={24} />
                </Box>
              ) : (
                <>
                  {/* Existing Links */}
                  {masterDataLinks.length > 0 && (
                    <List sx={{ mb: 2 }}>
                      {masterDataLinks.map((link) => (
                        <React.Fragment key={link.id}>
                          <ListItem>
                            <ListItemText
                              primary={
                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                  <Chip 
                                    label={fieldMasterDataService.getSourceTypeDisplayName(link.sourceType)} 
                                    size="small" 
                                    color="primary" 
                                    variant="outlined" 
                                  />
                                  <Typography variant="body2" fontWeight={500}>
                                    {link.sourceName}
                                  </Typography>
                                </Box>
                              }
                              secondary={
                                <Box sx={{ mt: 0.5 }}>
                                  <Typography variant="caption" display="block" color="textSecondary">
                                    Display: {link.displayField || 'default'} | Value: {link.valueField || 'default'}
                                  </Typography>
                                  {link.dependsOnField && (
                                    <Chip 
                                      label={`Depends on: ${link.dependsOnField}`} 
                                      size="small" 
                                      sx={{ mt: 0.5 }} 
                                    />
                                  )}
                                  {link.validationType && (
                                    <Chip 
                                      label={`Validation: ${fieldMasterDataService.getValidationTypeDisplayName(link.validationType)}`} 
                                      size="small" 
                                      sx={{ mt: 0.5, ml: 0.5 }} 
                                    />
                                  )}
                                  {link.allowFreeText && (
                                    <Chip 
                                      label="Allows free text" 
                                      size="small" 
                                      color="success"
                                      sx={{ mt: 0.5, ml: 0.5 }} 
                                    />
                                  )}
                                  {!link.allowFreeText && (
                                    <Chip 
                                      label="Restricted" 
                                      size="small" 
                                      color="error"
                                      sx={{ mt: 0.5, ml: 0.5 }} 
                                    />
                                  )}
                                </Box>
                              }
                            />
                            <ListItemSecondaryAction>
                              <Tooltip title="Edit link settings">
                                <IconButton 
                                  edge="end" 
                                  onClick={() => setEditingLink(link)} 
                                  sx={{ color: '#6750A4', mr: 0.5 }}
                                  size="small"
                                >
                                  <EditIcon fontSize="small" />
                                </IconButton>
                              </Tooltip>
                              <Tooltip title="Remove link">
                                <IconButton edge="end" onClick={() => handleDeleteMasterDataLink(link.id)} color="error" size="small">
                                  <DeleteIcon fontSize="small" />
                                </IconButton>
                              </Tooltip>
                            </ListItemSecondaryAction>
                          </ListItem>
                          <Divider />
                        </React.Fragment>
                      ))}
                    </List>
                  )}

                  {/* Edit Existing Link Form */}
                  {editingLink && (
                    <Paper sx={{ p: 2, mb: 2, backgroundColor: '#e3f2fd' }}>
                      <Typography variant="subtitle2" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <EditIcon fontSize="small" />
                        Edit Link: {editingLink.sourceName}
                      </Typography>
                      <Grid container spacing={2}>
                        <Grid item xs={12}>
                          <Alert severity="info" sx={{ py: 0.5 }}>
                            Source: <strong>{fieldMasterDataService.getSourceTypeDisplayName(editingLink.sourceType)}</strong> - {editingLink.sourceName}
                          </Alert>
                        </Grid>
                        <Grid item xs={6} sm={3}>
                          <TextField
                            fullWidth
                            size="small"
                            label="Display Field"
                            value={editingLink.displayField || ''}
                            onChange={(e) => setEditingLink({ ...editingLink, displayField: e.target.value })}
                          />
                        </Grid>
                        <Grid item xs={6} sm={3}>
                          <TextField
                            fullWidth
                            size="small"
                            label="Value Field"
                            value={editingLink.valueField || ''}
                            onChange={(e) => setEditingLink({ ...editingLink, valueField: e.target.value })}
                          />
                        </Grid>

                        <Grid item xs={12}>
                          <Divider sx={{ my: 1 }} />
                          <Typography variant="caption" color="textSecondary">
                            Cascading / Dependent Field (Optional)
                          </Typography>
                        </Grid>

                        <Grid item xs={6}>
                          <FormControl fullWidth size="small">
                            <InputLabel>Depends On Field</InputLabel>
                            <Select
                              value={editingLink.dependsOnField || ''}
                              onChange={(e) => setEditingLink({ ...editingLink, dependsOnField: e.target.value })}
                              label="Depends On Field"
                            >
                              <MenuItem value="">None</MenuItem>
                              {fields.filter(f => f.id !== editingField?.id).map(f => (
                                <MenuItem key={f.id} value={f.fieldName}>{f.fieldLabel} ({f.fieldName})</MenuItem>
                              ))}
                            </Select>
                          </FormControl>
                        </Grid>
                        <Grid item xs={6}>
                          <TextField
                            fullWidth
                            size="small"
                            label="Filter Column in Source"
                            value={editingLink.dependsOnSourceColumn || ''}
                            onChange={(e) => setEditingLink({ ...editingLink, dependsOnSourceColumn: e.target.value })}
                            placeholder="e.g., CountryCode, StateCode"
                          />
                        </Grid>

                        <Grid item xs={12}>
                          <Divider sx={{ my: 1 }} />
                          <Typography variant="caption" color="textSecondary">
                            Validation Options
                          </Typography>
                        </Grid>

                        <Grid item xs={6} sm={4}>
                          <FormControl fullWidth size="small">
                            <InputLabel>Validation Type</InputLabel>
                            <Select
                              value={editingLink.validationType || ''}
                              onChange={(e) => setEditingLink({ ...editingLink, validationType: e.target.value })}
                              label="Validation Type"
                            >
                              <MenuItem value="">None</MenuItem>
                              <MenuItem value="regex">Regex Pattern</MenuItem>
                              <MenuItem value="required">Required</MenuItem>
                              <MenuItem value="range">Range</MenuItem>
                              <MenuItem value="length">Length</MenuItem>
                            </Select>
                          </FormControl>
                        </Grid>
                        {editingLink.validationType === 'regex' && (
                          <Grid item xs={6} sm={4}>
                            <TextField
                              fullWidth
                              size="small"
                              label="Regex Pattern"
                              value={editingLink.validationPattern || ''}
                              onChange={(e) => setEditingLink({ ...editingLink, validationPattern: e.target.value })}
                              placeholder="^[A-Z]{2}$"
                            />
                          </Grid>
                        )}
                        <Grid item xs={12} sm={4}>
                          <TextField
                            fullWidth
                            size="small"
                            label="Validation Message"
                            value={editingLink.validationMessage || ''}
                            onChange={(e) => setEditingLink({ ...editingLink, validationMessage: e.target.value })}
                            placeholder="Invalid value"
                          />
                        </Grid>

                        <Grid item xs={12}>
                          <FormControlLabel
                            control={
                              <Switch
                                checked={editingLink.allowFreeText || false}
                                onChange={(e) => setEditingLink({ ...editingLink, allowFreeText: e.target.checked })}
                              />
                            }
                            label="Allow free text (values not in the list)"
                          />
                        </Grid>

                        <Grid item xs={12}>
                          <TextField
                            fullWidth
                            size="small"
                            label="Static Filter (JSON)"
                            value={editingLink.filterExpression || ''}
                            onChange={(e) => setEditingLink({ ...editingLink, filterExpression: e.target.value })}
                            placeholder='{"CountryCode": "US"}'
                            helperText="Optional: Filter data permanently"
                          />
                        </Grid>

                        <Grid item xs={12}>
                          <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end' }}>
                            <Button size="small" onClick={() => setEditingLink(null)}>
                              Cancel
                            </Button>
                            <Button 
                              size="small" 
                              variant="contained" 
                              onClick={handleSaveEditedLink}
                              startIcon={<SaveIcon />}
                              sx={{ backgroundColor: '#6750A4' }}
                            >
                              Save Link
                            </Button>
                          </Box>
                        </Grid>
                      </Grid>
                    </Paper>
                  )}

                  {masterDataLinks.length === 0 && !showAddLink && !editingLink && (
                    <Alert severity="info" sx={{ mb: 2 }}>
                      No data source linked to this field. Click "Add Data Source" to link master data for dropdown options.
                    </Alert>
                  )}

                  {/* Add New Link Form */}
                  {showAddLink ? (
                    <Paper sx={{ p: 2, mb: 2, backgroundColor: '#f9f9f9' }}>
                      <Typography variant="subtitle2" gutterBottom>Add Data Source</Typography>
                      <Grid container spacing={2}>
                        <Grid item xs={12} sm={6}>
                          <Autocomplete
                            options={availableSources}
                            getOptionLabel={(opt) => opt.displayName}
                            groupBy={(opt) => fieldMasterDataService.getSourceTypeDisplayName(opt.sourceType)}
                            value={getSelectedSource() || null}
                            onChange={(_, value) => {
                              if (value) {
                                setNewLink({
                                  ...newLink,
                                  sourceType: value.sourceType,
                                  sourceName: value.sourceName,
                                  displayField: value.availableFields[0],
                                  valueField: value.availableFields[0],
                                });
                              } else {
                                setNewLink({ ...newLink, sourceType: undefined, sourceName: undefined });
                              }
                            }}
                            renderInput={(params) => (
                              <TextField {...params} label="Data Source" size="small" fullWidth />
                            )}
                          />
                        </Grid>
                        {getSelectedSource() && (
                          <>
                            <Grid item xs={6} sm={3}>
                              <FormControl fullWidth size="small">
                                <InputLabel>Display Field</InputLabel>
                                <Select
                                  value={newLink.displayField || ''}
                                  onChange={(e) => setNewLink({ ...newLink, displayField: e.target.value })}
                                  label="Display Field"
                                >
                                  {getSelectedSource()?.availableFields.map(f => (
                                    <MenuItem key={f} value={f}>{f}</MenuItem>
                                  ))}
                                </Select>
                              </FormControl>
                            </Grid>
                            <Grid item xs={6} sm={3}>
                              <FormControl fullWidth size="small">
                                <InputLabel>Value Field</InputLabel>
                                <Select
                                  value={newLink.valueField || ''}
                                  onChange={(e) => setNewLink({ ...newLink, valueField: e.target.value })}
                                  label="Value Field"
                                >
                                  {getSelectedSource()?.availableFields.map(f => (
                                    <MenuItem key={f} value={f}>{f}</MenuItem>
                                  ))}
                                </Select>
                              </FormControl>
                            </Grid>
                          </>
                        )}

                        <Grid item xs={12}>
                          <Divider sx={{ my: 1 }} />
                          <Typography variant="caption" color="textSecondary">
                            Cascading / Dependent Field (Optional)
                          </Typography>
                        </Grid>

                        <Grid item xs={6}>
                          <FormControl fullWidth size="small">
                            <InputLabel>Depends On Field</InputLabel>
                            <Select
                              value={newLink.dependsOnField || ''}
                              onChange={(e) => setNewLink({ ...newLink, dependsOnField: e.target.value })}
                              label="Depends On Field"
                            >
                              <MenuItem value="">None</MenuItem>
                              {fields.filter(f => f.id !== editingField.id).map(f => (
                                <MenuItem key={f.id} value={f.fieldName}>{f.fieldLabel} ({f.fieldName})</MenuItem>
                              ))}
                            </Select>
                          </FormControl>
                        </Grid>
                        <Grid item xs={6}>
                          <TextField
                            fullWidth
                            size="small"
                            label="Filter Column in Source"
                            value={newLink.dependsOnSourceColumn || ''}
                            onChange={(e) => setNewLink({ ...newLink, dependsOnSourceColumn: e.target.value })}
                            placeholder="e.g., CountryCode, StateCode"
                            helperText="Column to filter by parent value"
                          />
                        </Grid>

                        <Grid item xs={12}>
                          <Divider sx={{ my: 1 }} />
                          <Typography variant="caption" color="textSecondary">
                            Validation Options
                          </Typography>
                        </Grid>

                        <Grid item xs={6} sm={4}>
                          <FormControl fullWidth size="small">
                            <InputLabel>Validation Type</InputLabel>
                            <Select
                              value={newLink.validationType || ''}
                              onChange={(e) => setNewLink({ ...newLink, validationType: e.target.value })}
                              label="Validation Type"
                            >
                              <MenuItem value="">None</MenuItem>
                              <MenuItem value="regex">Regex Pattern</MenuItem>
                              <MenuItem value="required">Required</MenuItem>
                              <MenuItem value="range">Range</MenuItem>
                              <MenuItem value="length">Length</MenuItem>
                            </Select>
                          </FormControl>
                        </Grid>
                        {newLink.validationType === 'regex' && (
                          <Grid item xs={6} sm={4}>
                            <TextField
                              fullWidth
                              size="small"
                              label="Regex Pattern"
                              value={newLink.validationPattern || ''}
                              onChange={(e) => setNewLink({ ...newLink, validationPattern: e.target.value })}
                              placeholder="^[A-Z]{2}$"
                            />
                          </Grid>
                        )}
                        <Grid item xs={12} sm={4}>
                          <TextField
                            fullWidth
                            size="small"
                            label="Validation Message"
                            value={newLink.validationMessage || ''}
                            onChange={(e) => setNewLink({ ...newLink, validationMessage: e.target.value })}
                            placeholder="Invalid value"
                          />
                        </Grid>

                        <Grid item xs={12}>
                          <FormControlLabel
                            control={
                              <Switch
                                checked={newLink.allowFreeText || false}
                                onChange={(e) => setNewLink({ ...newLink, allowFreeText: e.target.checked })}
                              />
                            }
                            label="Allow free text (values not in the list)"
                          />
                        </Grid>

                        <Grid item xs={12}>
                          <TextField
                            fullWidth
                            size="small"
                            label="Static Filter (JSON)"
                            value={newLink.filterExpression || ''}
                            onChange={(e) => setNewLink({ ...newLink, filterExpression: e.target.value })}
                            placeholder='{"CountryCode": "US"}'
                            helperText="Optional: Filter data permanently (e.g., only show US data)"
                          />
                        </Grid>

                        <Grid item xs={12}>
                          <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end' }}>
                            <Button size="small" onClick={() => { setShowAddLink(false); setNewLink({}); }}>
                              Cancel
                            </Button>
                            <Button 
                              size="small" 
                              variant="contained" 
                              onClick={handleAddMasterDataLink}
                              disabled={!newLink.sourceType || !newLink.sourceName}
                              sx={{ backgroundColor: '#6750A4' }}
                            >
                              Add Link
                            </Button>
                          </Box>
                        </Grid>
                      </Grid>
                    </Paper>
                  ) : !editingLink && (
                    <Button
                      startIcon={<AddIcon />}
                      onClick={() => setShowAddLink(true)}
                      sx={{ mb: 2 }}
                    >
                      Add Data Source
                    </Button>
                  )}
                </>
              )}
            </Box>
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
