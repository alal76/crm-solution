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
  Divider,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  ListItemSecondaryAction,
  Checkbox,
  Autocomplete,
  Stack,
  Breadcrumbs,
} from '@mui/material';
import {
  Edit as EditIcon,
  DragIndicator as DragIcon,
  Visibility as VisibilityIcon,
  VisibilityOff as VisibilityOffIcon,
  Settings as SettingsIcon,
  Refresh as RefreshIcon,
  ExpandMore as ExpandMoreIcon,
  Link as LinkIcon,
  ViewModule as ViewModuleIcon,
  Dashboard as DashboardIcon,
  People as PeopleIcon,
  TrendingUp as TrendingUpIcon,
  Inventory2 as PackageIcon,
  Campaign as MegaphoneIcon,
  Assignment as TaskIcon,
  Description as QuoteIcon,
  Note as NoteIcon,
  Timeline as ActivityIcon,
  AutoAwesome as AutomationIcon,
  Assessment as ReportsIcon,
  Build as ServicesIcon,
  PersonAdd as PersonAddIcon,
  Contacts as ContactsIcon,
  Save as SaveIcon,
  Add as AddIcon,
  Delete as DeleteIcon,
  ArrowUpward as ArrowUpIcon,
  ArrowDownward as ArrowDownIcon,
  ArrowBack as ArrowBackIcon,
  ChevronRight as ChevronRightIcon,
  Storage as StorageIcon,
  TableChart as TableChartIcon,
} from '@mui/icons-material';
import { DragDropContext, Droppable, Draggable, DropResult } from 'react-beautiful-dnd';
import apiClient from '../../services/apiClient';
import { useProfile } from '../../contexts/ProfileContext';

// Types
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

interface TabConfig {
  index: number;
  name: string;
  enabled: boolean;
  order: number;
  icon?: string;
}

interface LinkedEntity {
  entityName: string;
  relationshipType: string;
  enabled: boolean;
  tabName?: string;
  displayOrder: number;
  foreignKeyField?: string;
}

interface ModuleUIConfig {
  id: number;
  moduleName: string;
  isEnabled: boolean;
  displayName: string;
  description?: string;
  iconName: string;
  displayOrder: number;
  tabsConfig?: string;
  linkedEntitiesConfig?: string;
  listViewConfig?: string;
  detailViewConfig?: string;
  quickCreateConfig?: string;
  searchFilterConfig?: string;
  moduleSettings?: string;
}

interface CompleteModuleConfig {
  moduleConfig: ModuleUIConfig;
  fieldConfigurations: FieldConfig[];
  tabs: TabConfig[];
  linkedEntities: LinkedEntity[];
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

// Icon map for modules
const moduleIcons: { [key: string]: React.ReactNode } = {
  Dashboard: <DashboardIcon />,
  Customers: <PeopleIcon />,
  Contacts: <ContactsIcon />,
  Leads: <PersonAddIcon />,
  Opportunities: <TrendingUpIcon />,
  Products: <PackageIcon />,
  Services: <ServicesIcon />,
  Campaigns: <MegaphoneIcon />,
  Quotes: <QuoteIcon />,
  Tasks: <TaskIcon />,
  Activities: <ActivityIcon />,
  Notes: <NoteIcon />,
  Workflows: <AutomationIcon />,
  Reports: <ReportsIcon />,
};

// Available modules for linking
const AVAILABLE_MODULES = [
  'Customers', 'Contacts', 'Leads', 'Opportunities', 'Products',
  'Services', 'Campaigns', 'Quotes', 'Tasks', 'Activities', 'Notes'
];

const ModuleFieldSettingsTab: React.FC = () => {
  const { refreshModuleStatus } = useProfile();
  
  // Main state
  const [selectedModule, setSelectedModule] = useState('Customers');
  const [selectedSection, setSelectedSection] = useState(0); // 0: Modules, 1: Fields, 2: Tabs, 3: Linked Entities
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  
  // Unified view mode
  const [showUnifiedView, setShowUnifiedView] = useState(false);
  const [unifiedConfigModule, setUnifiedConfigModule] = useState<ModuleUIConfig | null>(null);
  const [unifiedSubTab, setUnifiedSubTab] = useState(0); // 0: Overview, 1: Fields, 2: Tabs, 3: Linked Entities
  
  // Module list state
  const [modules, setModules] = useState<ModuleUIConfig[]>([]);
  
  // Field configuration state
  const [fields, setFields] = useState<FieldConfig[]>([]);
  const [selectedFieldTab, setSelectedFieldTab] = useState(0);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [editingField, setEditingField] = useState<FieldConfig | null>(null);
  
  // Tab configuration state
  const [tabs, setTabs] = useState<TabConfig[]>([]);
  const [tabDialogOpen, setTabDialogOpen] = useState(false);
  const [editingTab, setEditingTab] = useState<TabConfig | null>(null);
  
  // Linked entities state
  const [linkedEntities, setLinkedEntities] = useState<LinkedEntity[]>([]);
  const [linkDialogOpen, setLinkDialogOpen] = useState(false);
  const [newLinkEntity, setNewLinkEntity] = useState('');
  
  // Database foreign keys state
  const [databaseForeignKeys, setDatabaseForeignKeys] = useState<Record<string, any[]>>({});
  const [loadingForeignKeys, setLoadingForeignKeys] = useState(false);

  // Load all module configurations
  const loadModules = useCallback(async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/moduleuiconfig');
      if (response.data && response.data.length > 0) {
        setModules(response.data);
      } else {
        // Initialize defaults if none exist
        await apiClient.post('/moduleuiconfig/initialize');
        const retryResponse = await apiClient.get('/moduleuiconfig');
        setModules(retryResponse.data);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load module configurations');
    } finally {
      setLoading(false);
    }
  }, []);

  // Load complete configuration for selected module
  const loadModuleConfig = useCallback(async () => {
    if (!selectedModule) return;
    
    try {
      setLoading(true);
      setError(null);
      
      // Load field configurations
      const fieldsResponse = await apiClient.get(`/modulefieldconfigurations/${selectedModule}`);
      setFields(fieldsResponse.data || []);
      
      // Try to load complete module config
      try {
        const configResponse = await apiClient.get(`/moduleuiconfig/${selectedModule}/complete`);
        if (configResponse.data) {
          setTabs(configResponse.data.tabs || []);
          setLinkedEntities(configResponse.data.linkedEntities || []);
        }
      } catch {
        // Module UI config may not exist yet, use defaults from fields
        const uniqueTabs = Array.from(new Set((fieldsResponse.data || []).map((f: FieldConfig) => f.tabName)));
        setTabs(uniqueTabs.map((name, index) => ({
          index,
          name: name as string,
          enabled: true,
          order: index
        })));
        setLinkedEntities([]);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load module configuration');
    } finally {
      setLoading(false);
    }
  }, [selectedModule]);

  // Load database foreign keys
  const loadDatabaseForeignKeys = useCallback(async () => {
    try {
      setLoadingForeignKeys(true);
      const response = await apiClient.get('/database/linked-entities-schema');
      setDatabaseForeignKeys(response.data || {});
    } catch (err: any) {
      console.error('Failed to load database foreign keys:', err);
      // Non-critical error, don't show to user
    } finally {
      setLoadingForeignKeys(false);
    }
  }, []);

  useEffect(() => {
    loadModules();
    loadDatabaseForeignKeys();
  }, [loadModules, loadDatabaseForeignKeys]);

  useEffect(() => {
    if (selectedModule && selectedSection > 0) {
      loadModuleConfig();
    }
  }, [selectedModule, selectedSection, loadModuleConfig]);

  // Group fields by tab
  const fieldTabs = Array.from(new Set(fields.map(f => f.tabName))).sort((a, b) => {
    const aIdx = fields.find(f => f.tabName === a)?.tabIndex || 0;
    const bIdx = fields.find(f => f.tabName === b)?.tabIndex || 0;
    return aIdx - bIdx;
  });

  const getFieldsForTab = (tabName: string) => {
    return fields
      .filter(f => f.tabName === tabName)
      .sort((a, b) => a.displayOrder - b.displayOrder);
  };

  // Module enable/disable toggle
  const handleToggleModule = async (moduleName: string, enabled: boolean) => {
    try {
      await apiClient.post(`/moduleuiconfig/${moduleName}/toggle?enabled=${enabled}`);
      setModules(modules.map(m => m.moduleName === moduleName ? { ...m, isEnabled: enabled } : m));
      setSuccess(`Module ${enabled ? 'enabled' : 'disabled'} successfully`);
      await refreshModuleStatus();
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to toggle module');
    }
  };

  // Enable/Disable all modules
  const handleEnableAllModules = async () => {
    try {
      const updates = modules.map(m => ({ moduleName: m.moduleName, isEnabled: true }));
      await apiClient.put('/moduleuiconfig/batch', { modules: updates });
      setModules(modules.map(m => ({ ...m, isEnabled: true })));
      setSuccess('All modules enabled');
      await refreshModuleStatus();
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to enable all modules');
    }
  };

  const handleDisableAllModules = async () => {
    try {
      // Keep Dashboard enabled
      const updates = modules.map(m => ({ 
        moduleName: m.moduleName, 
        isEnabled: m.moduleName === 'Dashboard' 
      }));
      await apiClient.put('/moduleuiconfig/batch', { modules: updates });
      setModules(modules.map(m => ({ ...m, isEnabled: m.moduleName === 'Dashboard' })));
      setSuccess('Modules disabled (Dashboard kept enabled)');
      await refreshModuleStatus();
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to disable modules');
    }
  };

  // Field visibility toggle
  const handleToggleFieldEnabled = async (field: FieldConfig) => {
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

  // Field required toggle
  const handleToggleFieldRequired = async (field: FieldConfig) => {
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

  // Edit field
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

  // Field drag and drop
  const handleFieldDragEnd = async (result: DropResult, tabName: string) => {
    if (!result.destination) return;

    const tabFields = getFieldsForTab(tabName);
    const items = Array.from(tabFields);
    const [reorderedItem] = items.splice(result.source.index, 1);
    items.splice(result.destination.index, 0, reorderedItem);

    const updatedFields = items.map((item, index) => ({
      ...item,
      displayOrder: index,
    }));

    setFields(fields.map(f => {
      const updated = updatedFields.find(uf => uf.id === f.id);
      return updated || f;
    }));

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
      loadModuleConfig();
    }
  };

  // Initialize defaults for a module
  const handleInitializeDefaults = async () => {
    console.log('[ModuleFieldSettings] Initialize Defaults clicked for module:', selectedModule);
    
    if (!selectedModule) {
      setError('Please select a module first');
      return;
    }

    // Use confirm but proceed even if blocked (some browsers block confirm in iframes)
    const confirmed = window.confirm(`Initialize default field configurations for ${selectedModule}?`);
    console.log('[ModuleFieldSettings] User confirmed:', confirmed);
    
    if (!confirmed) {
      console.log('[ModuleFieldSettings] User cancelled initialization');
      return;
    }

    try {
      setLoading(true);
      setError(null);
      console.log('[ModuleFieldSettings] Calling API to initialize:', selectedModule);
      const response = await apiClient.post(`/modulefieldconfigurations/initialize/${selectedModule}`);
      console.log('[ModuleFieldSettings] API response:', response.data);
      setSuccess(`Default configurations initialized successfully for ${selectedModule}`);
      setTimeout(() => setSuccess(null), 5000);
      await loadModuleConfig();
    } catch (err: any) {
      console.error('[ModuleFieldSettings] Initialize error:', err);
      const errorMsg = err.response?.data?.message || err.message || 'Failed to initialize default configurations';
      setError(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  // Tab management
  const handleToggleTab = (tabName: string) => {
    const updatedTabs = tabs.map(t => 
      t.name === tabName ? { ...t, enabled: !t.enabled } : t
    );
    setTabs(updatedTabs);
  };

  const handleSaveTabs = async () => {
    try {
      await apiClient.put(`/moduleuiconfig/${selectedModule}/tabs`, tabs);
      setSuccess('Tab configuration saved');
      setTimeout(() => setSuccess(null), 2000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save tab configuration');
    }
  };

  const moveTab = (index: number, direction: 'up' | 'down') => {
    const newTabs = [...tabs];
    const newIndex = direction === 'up' ? index - 1 : index + 1;
    if (newIndex < 0 || newIndex >= newTabs.length) return;
    [newTabs[index], newTabs[newIndex]] = [newTabs[newIndex], newTabs[index]];
    newTabs.forEach((t, i) => t.order = i);
    setTabs(newTabs);
  };

  // Linked entity management
  const handleToggleLinkedEntity = (entityName: string) => {
    const updated = linkedEntities.map(le => 
      le.entityName === entityName ? { ...le, enabled: !le.enabled } : le
    );
    setLinkedEntities(updated);
  };

  const handleAddLinkedEntity = () => {
    if (!newLinkEntity || linkedEntities.some(le => le.entityName === newLinkEntity)) {
      return;
    }
    setLinkedEntities([...linkedEntities, {
      entityName: newLinkEntity,
      relationshipType: 'one-to-many',
      enabled: true,
      tabName: newLinkEntity,
      displayOrder: linkedEntities.length
    }]);
    setNewLinkEntity('');
    setLinkDialogOpen(false);
  };

  const handleRemoveLinkedEntity = (entityName: string) => {
    setLinkedEntities(linkedEntities.filter(le => le.entityName !== entityName));
  };

  const handleSaveLinkedEntities = async () => {
    try {
      await apiClient.put(`/moduleuiconfig/${selectedModule}/linked-entities`, linkedEntities);
      setSuccess('Linked entities configuration saved');
      setTimeout(() => setSuccess(null), 2000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save linked entities');
    }
  };

  // Render module list section
  const renderModulesSection = () => (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h6" sx={{ fontWeight: 600 }}>
            Module Settings
          </Typography>
          <Typography variant="body2" color="textSecondary">
            Enable or disable modules across the entire CRM system
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button variant="outlined" startIcon={<RefreshIcon />} onClick={loadModules} disabled={loading}>
            Refresh
          </Button>
          <Button variant="outlined" color="success" onClick={handleEnableAllModules}>
            Enable All
          </Button>
          <Button variant="outlined" color="warning" onClick={handleDisableAllModules}>
            Disable All
          </Button>
        </Box>
      </Box>

      <Grid container spacing={2}>
        {modules.map((module) => (
          <Grid item xs={12} sm={6} md={4} key={module.moduleName}>
            <Card 
              sx={{ 
                borderRadius: 2,
                opacity: module.isEnabled ? 1 : 0.6,
                transition: 'all 0.2s',
                cursor: 'pointer',
                '&:hover': { boxShadow: 3 },
              }}
              onClick={() => {
                setSelectedModule(module.moduleName);
                setUnifiedConfigModule(module);
                setShowUnifiedView(true);
                setUnifiedSubTab(0);
              }}
            >
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                    <Box sx={{ 
                      color: module.isEnabled ? 'primary.main' : 'text.disabled',
                      display: 'flex',
                      alignItems: 'center',
                    }}>
                      {moduleIcons[module.moduleName] || <ViewModuleIcon />}
                    </Box>
                    <Box>
                      <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                        {module.displayName || module.moduleName}
                      </Typography>
                      <Typography variant="body2" color="textSecondary">
                        {module.description || 'Click to configure'}
                      </Typography>
                    </Box>
                  </Box>
                  <Switch
                    checked={module.isEnabled}
                    onChange={(e) => {
                      e.stopPropagation();
                      handleToggleModule(module.moduleName, !module.isEnabled);
                    }}
                    onClick={(e) => e.stopPropagation()}
                    color="primary"
                  />
                </Box>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Paper sx={{ p: 3, bgcolor: 'warning.light', borderRadius: 2, mt: 3 }}>
        <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
          ⚠️ Important Notes
        </Typography>
        <Typography variant="body2">
          • Disabling a module will hide it from navigation for all users<br />
          • Click on a module card to configure its fields, tabs, and linked entities<br />
          • Data in disabled modules is preserved and will be available when re-enabled
        </Typography>
      </Paper>
    </Box>
  );

  // Render field configuration section
  const renderFieldsSection = () => (
    <Box>
      <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box>
          <Typography variant="h6" gutterBottom>
            Field Configuration - {selectedModule}
          </Typography>
          <Typography variant="body2" color="textSecondary">
            Configure field visibility, order, and requirements
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <FormControl sx={{ minWidth: 180 }}>
            <InputLabel>Module</InputLabel>
            <Select
              value={selectedModule}
              onChange={(e) => setSelectedModule(e.target.value)}
              label="Module"
              size="small"
            >
              {modules.map(module => (
                <MenuItem key={module.moduleName} value={module.moduleName}>
                  {module.displayName || module.moduleName}
                </MenuItem>
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

      {fields.length === 0 ? (
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 6 }}>
            <SettingsIcon sx={{ fontSize: 64, color: '#ccc', mb: 2 }} />
            <Typography variant="h6" gutterBottom>
              No field configurations found
            </Typography>
            <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
              Click "Initialize Defaults" to create default field configurations
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
          <Tabs 
            value={selectedFieldTab} 
            onChange={(_, v) => setSelectedFieldTab(v)} 
            sx={{ borderBottom: 1, borderColor: 'divider', px: 2 }}
            variant="scrollable"
            scrollButtons="auto"
          >
            {fieldTabs.map((tabName, index) => (
              <Tab key={tabName} label={tabName} />
            ))}
          </Tabs>

          {fieldTabs.map((tabName, index) => (
            <TabPanel key={tabName} value={selectedFieldTab} index={index}>
              <Box sx={{ p: 2 }}>
                <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                  Drag to reorder • Toggle visibility • Configure requirements
                </Typography>

                <DragDropContext onDragEnd={(result) => handleFieldDragEnd(result, tabName)}>
                  <Droppable droppableId={`fields-${tabName}`}>
                    {(provided) => (
                      <Table {...provided.droppableProps} ref={provided.innerRef} size="small">
                        <TableHead>
                          <TableRow>
                            <TableCell width="40"></TableCell>
                            <TableCell><strong>Field</strong></TableCell>
                            <TableCell><strong>Type</strong></TableCell>
                            <TableCell><strong>Grid</strong></TableCell>
                            <TableCell><strong>Visible</strong></TableCell>
                            <TableCell><strong>Required</strong></TableCell>
                            <TableCell><strong>Actions</strong></TableCell>
                          </TableRow>
                        </TableHead>
                        <TableBody>
                          {getFieldsForTab(tabName).map((field, idx) => (
                            <Draggable
                              key={field.id}
                              draggableId={field.id.toString()}
                              index={idx}
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
                                      <Tooltip title="Cannot reorder">
                                        <DragIcon sx={{ color: '#ddd' }} />
                                      </Tooltip>
                                    )}
                                  </TableCell>
                                  <TableCell>
                                    <Typography variant="body2" fontWeight={500}>
                                      {field.fieldLabel}
                                    </Typography>
                                    <Typography variant="caption" color="textSecondary">
                                      {field.fieldName}
                                    </Typography>
                                  </TableCell>
                                  <TableCell>
                                    <Chip label={field.fieldType} size="small" variant="outlined" />
                                  </TableCell>
                                  <TableCell>
                                    <Chip label={`${field.gridSize}/12`} size="small" />
                                  </TableCell>
                                  <TableCell>
                                    <IconButton
                                      size="small"
                                      onClick={() => handleToggleFieldEnabled(field)}
                                      disabled={!field.isHideable}
                                      color={field.isEnabled ? 'primary' : 'default'}
                                    >
                                      {field.isEnabled ? <VisibilityIcon /> : <VisibilityOffIcon />}
                                    </IconButton>
                                  </TableCell>
                                  <TableCell>
                                    <Switch
                                      checked={field.isRequired}
                                      onChange={() => handleToggleFieldRequired(field)}
                                      disabled={!field.isRequiredConfigurable}
                                      size="small"
                                    />
                                  </TableCell>
                                  <TableCell>
                                    <IconButton size="small" onClick={() => handleEditField(field)} sx={{ color: '#6750A4' }}>
                                      <EditIcon fontSize="small" />
                                    </IconButton>
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
    </Box>
  );

  // Render tabs configuration section
  const renderTabsSection = () => (
    <Box>
      <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box>
          <Typography variant="h6" gutterBottom>
            Tab Configuration - {selectedModule}
          </Typography>
          <Typography variant="body2" color="textSecondary">
            Configure tab visibility and order for the module form
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <FormControl sx={{ minWidth: 180 }}>
            <InputLabel>Module</InputLabel>
            <Select
              value={selectedModule}
              onChange={(e) => setSelectedModule(e.target.value)}
              label="Module"
              size="small"
            >
              {modules.map(module => (
                <MenuItem key={module.moduleName} value={module.moduleName}>
                  {module.displayName || module.moduleName}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
          <Button
            variant="contained"
            startIcon={<SaveIcon />}
            onClick={handleSaveTabs}
            sx={{ backgroundColor: '#6750A4' }}
          >
            Save Changes
          </Button>
        </Box>
      </Box>

      <Paper sx={{ borderRadius: 2 }}>
        <List>
          {tabs.sort((a, b) => a.order - b.order).map((tab, index) => (
            <ListItem key={tab.name} divider>
              <ListItemIcon>
                <DragIcon sx={{ color: '#999' }} />
              </ListItemIcon>
              <ListItemText
                primary={tab.name}
                secondary={`Order: ${tab.order + 1}`}
              />
              <ListItemSecondaryAction sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <IconButton size="small" onClick={() => moveTab(index, 'up')} disabled={index === 0}>
                  <ArrowUpIcon fontSize="small" />
                </IconButton>
                <IconButton size="small" onClick={() => moveTab(index, 'down')} disabled={index === tabs.length - 1}>
                  <ArrowDownIcon fontSize="small" />
                </IconButton>
                <Switch
                  checked={tab.enabled}
                  onChange={() => handleToggleTab(tab.name)}
                  size="small"
                />
              </ListItemSecondaryAction>
            </ListItem>
          ))}
        </List>
        {tabs.length === 0 && (
          <Box sx={{ p: 4, textAlign: 'center' }}>
            <Typography color="textSecondary">
              No tabs configured. Initialize field defaults first.
            </Typography>
          </Box>
        )}
      </Paper>
    </Box>
  );

  // Render linked entities section
  const renderLinkedEntitiesSection = () => (
    <Box>
      <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box>
          <Typography variant="h6" gutterBottom>
            Linked Entities - {selectedModule}
          </Typography>
          <Typography variant="body2" color="textSecondary">
            Configure which entities can be linked to {selectedModule}
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <FormControl sx={{ minWidth: 180 }}>
            <InputLabel>Module</InputLabel>
            <Select
              value={selectedModule}
              onChange={(e) => setSelectedModule(e.target.value)}
              label="Module"
              size="small"
            >
              {modules.map(module => (
                <MenuItem key={module.moduleName} value={module.moduleName}>
                  {module.displayName || module.moduleName}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
          <Button
            variant="outlined"
            startIcon={<AddIcon />}
            onClick={() => setLinkDialogOpen(true)}
          >
            Add Link
          </Button>
          <Button
            variant="contained"
            startIcon={<SaveIcon />}
            onClick={handleSaveLinkedEntities}
            sx={{ backgroundColor: '#6750A4' }}
          >
            Save Changes
          </Button>
        </Box>
      </Box>

      <Paper sx={{ borderRadius: 2 }}>
        <List>
          {linkedEntities.map((entity) => (
            <ListItem key={entity.entityName} divider>
              <ListItemIcon>
                {moduleIcons[entity.entityName] || <LinkIcon />}
              </ListItemIcon>
              <ListItemText
                primary={entity.entityName}
                secondary={
                  <Box sx={{ display: 'flex', gap: 1, mt: 0.5 }}>
                    <Chip label={entity.relationshipType} size="small" variant="outlined" />
                    {entity.tabName && <Chip label={`Tab: ${entity.tabName}`} size="small" />}
                  </Box>
                }
              />
              <ListItemSecondaryAction sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Switch
                  checked={entity.enabled}
                  onChange={() => handleToggleLinkedEntity(entity.entityName)}
                  size="small"
                />
                <IconButton 
                  size="small" 
                  onClick={() => handleRemoveLinkedEntity(entity.entityName)}
                  color="error"
                >
                  <DeleteIcon fontSize="small" />
                </IconButton>
              </ListItemSecondaryAction>
            </ListItem>
          ))}
        </List>
        {linkedEntities.length === 0 && (
          <Box sx={{ p: 4, textAlign: 'center' }}>
            <LinkIcon sx={{ fontSize: 48, color: '#ccc', mb: 2 }} />
            <Typography color="textSecondary">
              No linked entities configured. Click "Add Link" to connect entities.
            </Typography>
          </Box>
        )}
      </Paper>

      <Paper sx={{ p: 2, mt: 2, bgcolor: 'info.light', borderRadius: 2 }}>
        <Typography variant="body2">
          <strong>Relationship Types:</strong><br />
          • <strong>one-to-many:</strong> One {selectedModule} record can have many linked records<br />
          • <strong>many-to-many:</strong> Many-to-many relationship through junction table<br />
          • <strong>one-to-one:</strong> One-to-one relationship
        </Typography>
      </Paper>
    </Box>
  );

  if (loading && modules.length === 0) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  // Unified Module Configuration View
  const renderUnifiedConfigView = () => {
    if (!unifiedConfigModule) return null;

    const moduleFields = fields.filter(f => f.moduleName === selectedModule);
    const moduleTabs = tabs.length > 0 ? tabs : 
      Array.from(new Set(moduleFields.map(f => f.tabName))).map((name, idx) => ({
        index: idx,
        name: name as string,
        enabled: true,
        order: idx
      }));

    return (
      <Box>
        {/* Breadcrumb and Back Button */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
          <Button
            startIcon={<ArrowBackIcon />}
            onClick={() => {
              setShowUnifiedView(false);
              setUnifiedConfigModule(null);
            }}
            variant="outlined"
            size="small"
          >
            Back to Modules
          </Button>
          <Breadcrumbs separator={<ChevronRightIcon fontSize="small" />}>
            <Typography color="text.secondary">Module Settings</Typography>
            <Typography color="primary" sx={{ fontWeight: 600 }}>
              {unifiedConfigModule.displayName || unifiedConfigModule.moduleName}
            </Typography>
          </Breadcrumbs>
        </Box>

        {/* Module Header Card */}
        <Card sx={{ mb: 3, borderRadius: 2 }}>
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <Box sx={{ 
                  color: unifiedConfigModule.isEnabled ? 'primary.main' : 'text.disabled',
                  p: 1.5,
                  bgcolor: 'action.hover',
                  borderRadius: 2,
                  display: 'flex',
                }}>
                  {moduleIcons[unifiedConfigModule.moduleName] || <ViewModuleIcon fontSize="large" />}
                </Box>
                <Box>
                  <Typography variant="h5" sx={{ fontWeight: 600 }}>
                    {unifiedConfigModule.displayName || unifiedConfigModule.moduleName}
                  </Typography>
                  <Typography variant="body2" color="textSecondary">
                    {unifiedConfigModule.description || 'Configure all aspects of this module'}
                  </Typography>
                </Box>
              </Box>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <Chip 
                  label={unifiedConfigModule.isEnabled ? 'Enabled' : 'Disabled'}
                  color={unifiedConfigModule.isEnabled ? 'success' : 'default'}
                />
                <Switch
                  checked={unifiedConfigModule.isEnabled}
                  onChange={(e) => handleToggleModule(unifiedConfigModule.moduleName, e.target.checked)}
                  color="primary"
                />
              </Box>
            </Box>
          </CardContent>
        </Card>

        {/* Configuration Sub-Tabs */}
        <Paper sx={{ borderRadius: 2 }}>
          <Tabs 
            value={unifiedSubTab} 
            onChange={(_, v) => setUnifiedSubTab(v)}
            sx={{ borderBottom: 1, borderColor: 'divider', px: 2 }}
          >
            <Tab icon={<SettingsIcon />} label="Overview" iconPosition="start" />
            <Tab icon={<TableChartIcon />} label="Fields" iconPosition="start" />
            <Tab icon={<DashboardIcon />} label="Tabs" iconPosition="start" />
            <Tab icon={<LinkIcon />} label="Linked Entities" iconPosition="start" />
          </Tabs>

          {/* Overview Tab */}
          {unifiedSubTab === 0 && (
            <Box sx={{ p: 3 }}>
              <Grid container spacing={3}>
                <Grid item xs={12} md={6}>
                  <Card variant="outlined" sx={{ height: '100%' }}>
                    <CardContent>
                      <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 2 }}>
                        <SettingsIcon sx={{ mr: 1, verticalAlign: 'middle', fontSize: 20 }} />
                        Module Settings
                      </Typography>
                      <Grid container spacing={2}>
                        <Grid item xs={12}>
                          <TextField
                            fullWidth
                            label="Display Name"
                            value={unifiedConfigModule.displayName || ''}
                            size="small"
                            disabled
                            helperText="Module display name (contact admin to change)"
                          />
                        </Grid>
                        <Grid item xs={12}>
                          <TextField
                            fullWidth
                            label="Description"
                            value={unifiedConfigModule.description || ''}
                            size="small"
                            multiline
                            rows={2}
                            disabled
                          />
                        </Grid>
                        <Grid item xs={6}>
                          <TextField
                            fullWidth
                            label="Display Order"
                            value={unifiedConfigModule.displayOrder}
                            size="small"
                            type="number"
                            disabled
                          />
                        </Grid>
                        <Grid item xs={6}>
                          <TextField
                            fullWidth
                            label="Icon"
                            value={unifiedConfigModule.iconName || 'Default'}
                            size="small"
                            disabled
                          />
                        </Grid>
                      </Grid>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} md={6}>
                  <Card variant="outlined" sx={{ height: '100%' }}>
                    <CardContent>
                      <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 2 }}>
                        <StorageIcon sx={{ mr: 1, verticalAlign: 'middle', fontSize: 20 }} />
                        Statistics
                      </Typography>
                      <Grid container spacing={2}>
                        <Grid item xs={6}>
                          <Paper sx={{ p: 2, textAlign: 'center', bgcolor: 'action.hover' }}>
                            <Typography variant="h4" color="primary">{moduleFields.length}</Typography>
                            <Typography variant="caption">Total Fields</Typography>
                          </Paper>
                        </Grid>
                        <Grid item xs={6}>
                          <Paper sx={{ p: 2, textAlign: 'center', bgcolor: 'action.hover' }}>
                            <Typography variant="h4" color="success.main">
                              {moduleFields.filter(f => f.isEnabled).length}
                            </Typography>
                            <Typography variant="caption">Visible Fields</Typography>
                          </Paper>
                        </Grid>
                        <Grid item xs={6}>
                          <Paper sx={{ p: 2, textAlign: 'center', bgcolor: 'action.hover' }}>
                            <Typography variant="h4" color="secondary">{moduleTabs.length}</Typography>
                            <Typography variant="caption">Tabs</Typography>
                          </Paper>
                        </Grid>
                        <Grid item xs={6}>
                          <Paper sx={{ p: 2, textAlign: 'center', bgcolor: 'action.hover' }}>
                            <Typography variant="h4" color="info.main">{linkedEntities.length}</Typography>
                            <Typography variant="caption">Linked Entities</Typography>
                          </Paper>
                        </Grid>
                      </Grid>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12}>
                  <Alert severity="info" sx={{ borderRadius: 2 }}>
                    <Typography variant="body2">
                      <strong>Quick Actions:</strong> Use the tabs above to configure fields, form tabs, and entity relationships.
                      Changes to fields take effect immediately. Tab and linked entity changes require saving.
                    </Typography>
                  </Alert>
                </Grid>
              </Grid>
            </Box>
          )}

          {/* Fields Tab - Inline edit */}
          {unifiedSubTab === 1 && (
            <Box sx={{ p: 2 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                  Field Configuration
                </Typography>
                <Button
                  variant="outlined"
                  startIcon={<RefreshIcon />}
                  onClick={handleInitializeDefaults}
                  size="small"
                >
                  Initialize Defaults
                </Button>
              </Box>
              {moduleFields.length === 0 ? (
                <Paper sx={{ p: 4, textAlign: 'center', bgcolor: 'action.hover', borderRadius: 2 }}>
                  <SettingsIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
                  <Typography>No fields configured. Click "Initialize Defaults" to create field configurations.</Typography>
                </Paper>
              ) : (
                <Box>
                  <Tabs 
                    value={selectedFieldTab} 
                    onChange={(_, v) => setSelectedFieldTab(v)} 
                    sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}
                    variant="scrollable"
                    scrollButtons="auto"
                  >
                    {fieldTabs.map((tabName) => (
                      <Tab key={tabName} label={tabName} />
                    ))}
                  </Tabs>
                  {fieldTabs.map((tabName, index) => (
                    <TabPanel key={tabName} value={selectedFieldTab} index={index}>
                      <DragDropContext onDragEnd={(result) => handleFieldDragEnd(result, tabName)}>
                        <Droppable droppableId={`unified-fields-${tabName}`}>
                          {(provided) => (
                            <Table {...provided.droppableProps} ref={provided.innerRef} size="small">
                              <TableHead>
                                <TableRow>
                                  <TableCell width="40"></TableCell>
                                  <TableCell><strong>Field</strong></TableCell>
                                  <TableCell><strong>Type</strong></TableCell>
                                  <TableCell><strong>Grid</strong></TableCell>
                                  <TableCell><strong>Visible</strong></TableCell>
                                  <TableCell><strong>Required</strong></TableCell>
                                  <TableCell><strong>Actions</strong></TableCell>
                                </TableRow>
                              </TableHead>
                              <TableBody>
                                {getFieldsForTab(tabName).map((field, idx) => (
                                  <Draggable
                                    key={field.id}
                                    draggableId={`unified-${field.id.toString()}`}
                                    index={idx}
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
                                            <Tooltip title="Cannot reorder">
                                              <DragIcon sx={{ color: '#ddd' }} />
                                            </Tooltip>
                                          )}
                                        </TableCell>
                                        <TableCell>
                                          <Typography variant="body2" fontWeight={500}>
                                            {field.fieldLabel}
                                          </Typography>
                                          <Typography variant="caption" color="textSecondary">
                                            {field.fieldName}
                                          </Typography>
                                        </TableCell>
                                        <TableCell>
                                          <Chip label={field.fieldType} size="small" variant="outlined" />
                                        </TableCell>
                                        <TableCell>
                                          <Chip label={`${field.gridSize}/12`} size="small" />
                                        </TableCell>
                                        <TableCell>
                                          <IconButton
                                            size="small"
                                            onClick={() => handleToggleFieldEnabled(field)}
                                            disabled={!field.isHideable}
                                            color={field.isEnabled ? 'primary' : 'default'}
                                          >
                                            {field.isEnabled ? <VisibilityIcon /> : <VisibilityOffIcon />}
                                          </IconButton>
                                        </TableCell>
                                        <TableCell>
                                          <Switch
                                            checked={field.isRequired}
                                            onChange={() => handleToggleFieldRequired(field)}
                                            disabled={!field.isRequiredConfigurable}
                                            size="small"
                                          />
                                        </TableCell>
                                        <TableCell>
                                          <IconButton size="small" onClick={() => handleEditField(field)} sx={{ color: '#6750A4' }}>
                                            <EditIcon fontSize="small" />
                                          </IconButton>
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
                    </TabPanel>
                  ))}
                </Box>
              )}
            </Box>
          )}

          {/* Tabs Configuration Tab */}
          {unifiedSubTab === 2 && (
            <Box sx={{ p: 2 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                  Form Tab Configuration
                </Typography>
                <Button
                  variant="contained"
                  startIcon={<SaveIcon />}
                  onClick={handleSaveTabs}
                  size="small"
                >
                  Save Changes
                </Button>
              </Box>
              <Paper variant="outlined" sx={{ borderRadius: 2 }}>
                <List>
                  {moduleTabs.sort((a, b) => a.order - b.order).map((tab, index) => (
                    <ListItem key={tab.name} divider={index < moduleTabs.length - 1}>
                      <ListItemIcon>
                        <DragIcon sx={{ color: '#999' }} />
                      </ListItemIcon>
                      <ListItemText
                        primary={tab.name}
                        secondary={`Order: ${tab.order + 1}`}
                      />
                      <ListItemSecondaryAction sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <IconButton size="small" onClick={() => moveTab(index, 'up')} disabled={index === 0}>
                          <ArrowUpIcon fontSize="small" />
                        </IconButton>
                        <IconButton size="small" onClick={() => moveTab(index, 'down')} disabled={index === moduleTabs.length - 1}>
                          <ArrowDownIcon fontSize="small" />
                        </IconButton>
                        <Switch
                          checked={tab.enabled}
                          onChange={() => handleToggleTab(tab.name)}
                          size="small"
                        />
                      </ListItemSecondaryAction>
                    </ListItem>
                  ))}
                </List>
                {moduleTabs.length === 0 && (
                  <Box sx={{ p: 4, textAlign: 'center' }}>
                    <Typography color="textSecondary">
                      No tabs configured. Initialize field defaults first.
                    </Typography>
                  </Box>
                )}
              </Paper>
            </Box>
          )}

          {/* Linked Entities Tab */}
          {unifiedSubTab === 3 && (
            <Box sx={{ p: 2 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                  Linked Entities Configuration
                </Typography>
                <Box sx={{ display: 'flex', gap: 1 }}>
                  <Button
                    variant="outlined"
                    startIcon={<AddIcon />}
                    onClick={() => setLinkDialogOpen(true)}
                    size="small"
                  >
                    Add Link
                  </Button>
                  <Button
                    variant="contained"
                    startIcon={<SaveIcon />}
                    onClick={handleSaveLinkedEntities}
                    size="small"
                  >
                    Save Changes
                  </Button>
                </Box>
              </Box>

              {/* Database Foreign Keys Section */}
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1, mt: 2 }}>
                <StorageIcon sx={{ fontSize: 18, mr: 0.5, verticalAlign: 'middle' }} />
                Database Foreign Keys
              </Typography>
              <Paper variant="outlined" sx={{ borderRadius: 2, mb: 3 }}>
                {loadingForeignKeys ? (
                  <Box sx={{ p: 3, textAlign: 'center' }}>
                    <CircularProgress size={24} />
                    <Typography variant="body2" sx={{ mt: 1 }}>Loading database schema...</Typography>
                  </Box>
                ) : (
                  <List dense>
                    {(() => {
                      // Get the table name for the current module (assuming plural form)
                      const tableName = selectedModule;
                      const tableKeys = databaseForeignKeys[tableName] || [];
                      
                      if (tableKeys.length === 0) {
                        return (
                          <ListItem>
                            <ListItemText 
                              primary="No foreign keys found"
                              secondary={`Table "${tableName}" has no foreign key relationships defined in the database.`}
                            />
                          </ListItem>
                        );
                      }
                      
                      return tableKeys.map((fk: any, idx: number) => (
                        <ListItem key={`${fk.constraintName}-${idx}`} divider={idx < tableKeys.length - 1}>
                          <ListItemIcon>
                            <StorageIcon color="primary" />
                          </ListItemIcon>
                          <ListItemText
                            primary={
                              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <Typography variant="body2" sx={{ fontWeight: 600 }}>
                                  {fk.sourceColumn}
                                </Typography>
                                <ChevronRightIcon sx={{ color: 'text.secondary', fontSize: 16 }} />
                                <Typography variant="body2" color="primary">
                                  {fk.referencedTable}.{fk.referencedColumn}
                                </Typography>
                              </Box>
                            }
                            secondary={
                              <Box sx={{ display: 'flex', gap: 1, mt: 0.5, flexWrap: 'wrap' }}>
                                <Chip label={fk.constraintName} size="small" sx={{ height: 18, fontSize: '0.65rem' }} />
                                <Chip 
                                  label={`ON DELETE: ${fk.onDelete}`} 
                                  size="small" 
                                  variant="outlined"
                                  sx={{ height: 18, fontSize: '0.65rem' }} 
                                />
                                <Chip 
                                  label={`ON UPDATE: ${fk.onUpdate}`} 
                                  size="small" 
                                  variant="outlined"
                                  sx={{ height: 18, fontSize: '0.65rem' }} 
                                />
                              </Box>
                            }
                          />
                        </ListItem>
                      ));
                    })()}
                  </List>
                )}
              </Paper>

              {/* Configured Linked Entities Section */}
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
                <LinkIcon sx={{ fontSize: 18, mr: 0.5, verticalAlign: 'middle' }} />
                Configured Linked Entities
              </Typography>
              <Paper variant="outlined" sx={{ borderRadius: 2 }}>
                <List>
                  {linkedEntities.map((entity, idx) => (
                    <ListItem key={entity.entityName} divider={idx < linkedEntities.length - 1}>
                      <ListItemIcon>
                        {moduleIcons[entity.entityName] || <LinkIcon />}
                      </ListItemIcon>
                      <ListItemText
                        primary={entity.entityName}
                        secondary={
                          <Box sx={{ display: 'flex', gap: 1, mt: 0.5 }}>
                            <Chip label={entity.relationshipType} size="small" variant="outlined" />
                            {entity.foreignKeyField && (
                              <Chip label={`FK: ${entity.foreignKeyField}`} size="small" color="info" variant="outlined" />
                            )}
                          </Box>
                        }
                      />
                      <ListItemSecondaryAction sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Switch
                          checked={entity.enabled}
                          onChange={() => handleToggleLinkedEntity(entity.entityName)}
                          size="small"
                        />
                        <IconButton 
                          size="small" 
                          onClick={() => handleRemoveLinkedEntity(entity.entityName)}
                          color="error"
                        >
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </ListItemSecondaryAction>
                    </ListItem>
                  ))}
                </List>
                {linkedEntities.length === 0 && (
                  <Box sx={{ p: 4, textAlign: 'center' }}>
                    <LinkIcon sx={{ fontSize: 48, color: '#ccc', mb: 2 }} />
                    <Typography color="textSecondary">
                      No linked entities configured. Click "Add Link" to connect entities.
                    </Typography>
                  </Box>
                )}
              </Paper>
              <Alert severity="info" sx={{ mt: 2, borderRadius: 2 }}>
                <Typography variant="body2">
                  <strong>Note:</strong> Database foreign keys show actual database relationships. Configured linked entities control UI behavior.
                </Typography>
              </Alert>
            </Box>
          )}
        </Paper>
      </Box>
    );
  };

  return (
    <Box>
      {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}

      {/* Show unified view when a module is selected */}
      {showUnifiedView ? renderUnifiedConfigView() : (
        <>
          {/* Section tabs */}
          <Paper sx={{ mb: 3, borderRadius: 2 }}>
            <Tabs 
              value={selectedSection} 
              onChange={(_, v) => setSelectedSection(v)}
              sx={{ borderBottom: 1, borderColor: 'divider' }}
            >
              <Tab icon={<ViewModuleIcon />} label="Modules" iconPosition="start" />
              <Tab icon={<SettingsIcon />} label="Fields" iconPosition="start" />
              <Tab icon={<DashboardIcon />} label="Tabs" iconPosition="start" />
              <Tab icon={<LinkIcon />} label="Linked Entities" iconPosition="start" />
            </Tabs>
          </Paper>

          {/* Section content */}
          {selectedSection === 0 && renderModulesSection()}
          {selectedSection === 1 && renderFieldsSection()}
          {selectedSection === 2 && renderTabsSection()}
          {selectedSection === 3 && renderLinkedEntitiesSection()}
        </>
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

      {/* Add Linked Entity Dialog */}
      <Dialog open={linkDialogOpen} onClose={() => setLinkDialogOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Add Linked Entity</DialogTitle>
        <DialogContent>
          <FormControl fullWidth sx={{ mt: 2 }}>
            <InputLabel>Entity</InputLabel>
            <Select
              value={newLinkEntity}
              onChange={(e) => setNewLinkEntity(e.target.value)}
              label="Entity"
            >
              {AVAILABLE_MODULES
                .filter(m => m !== selectedModule && !linkedEntities.some(le => le.entityName === m))
                .map(module => (
                  <MenuItem key={module} value={module}>{module}</MenuItem>
                ))
              }
            </Select>
          </FormControl>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setLinkDialogOpen(false)}>Cancel</Button>
          <Button 
            onClick={handleAddLinkedEntity} 
            variant="contained" 
            disabled={!newLinkEntity}
            sx={{ backgroundColor: '#6750A4' }}
          >
            Add
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default ModuleFieldSettingsTab;
