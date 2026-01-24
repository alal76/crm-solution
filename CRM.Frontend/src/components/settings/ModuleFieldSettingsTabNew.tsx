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
  CircularProgress,
  Divider,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  ListItemSecondaryAction,
  Checkbox,
  Stack,
  Collapse,
} from '@mui/material';
import {
  Edit as EditIcon,
  DragIndicator as DragIcon,
  Visibility as VisibilityIcon,
  VisibilityOff as VisibilityOffIcon,
  Refresh as RefreshIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
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
  ChevronRight as ChevronRightIcon,
  Storage as StorageIcon,
  RestartAlt as ResetIcon,
  Folder as FolderIcon,
  FolderOpen as FolderOpenIcon,
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

// Available field types for configuration
const FIELD_TYPES = [
  { value: 'text', label: 'Text', description: 'Single line text input' },
  { value: 'textarea', label: 'Text Area', description: 'Multi-line text input' },
  { value: 'number', label: 'Number', description: 'Numeric input' },
  { value: 'currency', label: 'Currency', description: 'Currency/Money field' },
  { value: 'date', label: 'Date', description: 'Date picker' },
  { value: 'datetime', label: 'Date & Time', description: 'Date and time picker' },
  { value: 'email', label: 'Email', description: 'Email address input' },
  { value: 'phone', label: 'Phone', description: 'Phone number input' },
  { value: 'url', label: 'URL', description: 'Website URL input' },
  { value: 'select', label: 'Dropdown', description: 'Single selection dropdown' },
  { value: 'multiselect', label: 'Multi-Select', description: 'Multiple selection' },
  { value: 'checkbox', label: 'Checkbox', description: 'Yes/No checkbox' },
  { value: 'radio', label: 'Radio Buttons', description: 'Radio button options' },
  { value: 'lookup', label: 'Lookup', description: 'Reference to another entity' },
  { value: 'autocomplete', label: 'Auto-Complete', description: 'Search and select' },
  { value: 'rating', label: 'Rating', description: 'Star rating input' },
  { value: 'file', label: 'File Upload', description: 'File attachment' },
  { value: 'image', label: 'Image', description: 'Image upload' },
  { value: 'color', label: 'Color Picker', description: 'Color selection' },
  { value: 'richtext', label: 'Rich Text', description: 'Rich text editor' },
  { value: 'json', label: 'JSON', description: 'JSON data input' },
  { value: 'readonly', label: 'Read Only', description: 'Display only field' },
];

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
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  
  // Module list state
  const [modules, setModules] = useState<ModuleUIConfig[]>([]);
  const [expandedModules, setExpandedModules] = useState<Set<string>>(new Set());
  const [expandedTabs, setExpandedTabs] = useState<Set<string>>(new Set());
  
  // Configuration state per module
  const [moduleConfigs, setModuleConfigs] = useState<Record<string, {
    fields: FieldConfig[];
    tabs: TabConfig[];
    linkedEntities: LinkedEntity[];
  }>>({});
  
  // Database foreign keys state
  const [databaseForeignKeys, setDatabaseForeignKeys] = useState<Record<string, any[]>>({});
  
  // Edit dialog state
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [editingField, setEditingField] = useState<FieldConfig | null>(null);
  
  // Add linked entity dialog
  const [linkDialogOpen, setLinkDialogOpen] = useState(false);
  const [linkDialogModule, setLinkDialogModule] = useState<string>('');
  const [newLinkEntity, setNewLinkEntity] = useState('');

  // Load all module configurations
  const loadModules = useCallback(async () => {
    try {
      setLoading(true);
      
      // First, initialize field configurations for all modules
      // This ensures fields are available without requiring users to visit each entity first
      try {
        await apiClient.post('/modulefieldconfigurations/initialize-all');
      } catch (initErr: any) {
        // Non-critical - may fail if already initialized or if user is not admin
        // Silently handled - 403 means user is not admin, otherwise already initialized
      }
      
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

  // Load complete configuration for a module
  const loadModuleConfig = useCallback(async (moduleName: string) => {
    try {
      // First try to load existing config
      let fieldsResponse;
      try {
        fieldsResponse = await apiClient.get(`/modulefieldconfigurations/${moduleName}`);
      } catch {
        fieldsResponse = { data: [] };
      }

      // If no fields exist, auto-initialize
      if (!fieldsResponse.data || fieldsResponse.data.length === 0) {
        try {
          await apiClient.post(`/modulefieldconfigurations/initialize/${moduleName}`);
          fieldsResponse = await apiClient.get(`/modulefieldconfigurations/${moduleName}`);
        } catch {
          // Initialization might fail if module has no schema
        }
      }
      
      const fields: FieldConfig[] = fieldsResponse.data || [];
      
      let tabs: TabConfig[] = [];
      let linkedEntities: LinkedEntity[] = [];
      
      try {
        const configResponse = await apiClient.get(`/moduleuiconfig/${moduleName}/complete`);
        if (configResponse.data) {
          tabs = configResponse.data.tabs || [];
          linkedEntities = configResponse.data.linkedEntities || [];
        }
      } catch {
        // Use defaults from fields
      }
      
      // If no tabs configured, derive from fields
      if (tabs.length === 0 && fields.length > 0) {
        const uniqueTabs = Array.from(new Set(fields.map(f => f.tabName)));
        tabs = uniqueTabs.map((name, index) => ({
          index,
          name: name as string,
          enabled: true,
          order: index
        }));
      }
      
      setModuleConfigs(prev => ({
        ...prev,
        [moduleName]: { fields, tabs, linkedEntities }
      }));
    } catch (err: any) {
      console.error(`Failed to load config for ${moduleName}:`, err);
    }
  }, []);

  // Load database foreign keys
  const loadDatabaseForeignKeys = useCallback(async () => {
    try {
      const response = await apiClient.get('/database/linked-entities-schema');
      setDatabaseForeignKeys(response.data || {});
    } catch {
      // Non-critical
    }
  }, []);

  useEffect(() => {
    loadModules();
    loadDatabaseForeignKeys();
  }, [loadModules, loadDatabaseForeignKeys]);

  // Toggle module expansion
  const toggleModuleExpand = async (moduleName: string) => {
    const newExpanded = new Set(expandedModules);
    if (newExpanded.has(moduleName)) {
      newExpanded.delete(moduleName);
    } else {
      newExpanded.add(moduleName);
      // Load config if not already loaded
      if (!moduleConfigs[moduleName]) {
        await loadModuleConfig(moduleName);
      }
    }
    setExpandedModules(newExpanded);
  };

  // Toggle tab expansion
  const toggleTabExpand = (moduleTab: string) => {
    const newExpanded = new Set(expandedTabs);
    if (newExpanded.has(moduleTab)) {
      newExpanded.delete(moduleTab);
    } else {
      newExpanded.add(moduleTab);
    }
    setExpandedTabs(newExpanded);
  };

  // Get fields for a specific tab
  const getFieldsForTab = (moduleName: string, tabName: string) => {
    const config = moduleConfigs[moduleName];
    if (!config) return [];
    return config.fields
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

  // Toggle field visibility
  const handleToggleFieldEnabled = (moduleName: string, fieldId: number) => {
    setModuleConfigs(prev => {
      const config = prev[moduleName];
      if (!config) return prev;
      return {
        ...prev,
        [moduleName]: {
          ...config,
          fields: config.fields.map(f => 
            f.id === fieldId ? { ...f, isEnabled: !f.isEnabled } : f
          )
        }
      };
    });
  };

  // Toggle field required
  const handleToggleFieldRequired = (moduleName: string, fieldId: number) => {
    setModuleConfigs(prev => {
      const config = prev[moduleName];
      if (!config) return prev;
      return {
        ...prev,
        [moduleName]: {
          ...config,
          fields: config.fields.map(f => 
            f.id === fieldId ? { ...f, isRequired: !f.isRequired } : f
          )
        }
      };
    });
  };

  // Change field type
  const handleChangeFieldType = (moduleName: string, fieldId: number, newType: string) => {
    setModuleConfigs(prev => {
      const config = prev[moduleName];
      if (!config) return prev;
      return {
        ...prev,
        [moduleName]: {
          ...config,
          fields: config.fields.map(f => 
            f.id === fieldId ? { ...f, fieldType: newType } : f
          )
        }
      };
    });
  };

  // Toggle tab visibility
  const handleToggleTab = (moduleName: string, tabName: string) => {
    setModuleConfigs(prev => {
      const config = prev[moduleName];
      if (!config) return prev;
      return {
        ...prev,
        [moduleName]: {
          ...config,
          tabs: config.tabs.map(t => 
            t.name === tabName ? { ...t, enabled: !t.enabled } : t
          )
        }
      };
    });
  };

  // Move tab order
  const handleMoveTab = (moduleName: string, tabIndex: number, direction: 'up' | 'down') => {
    setModuleConfigs(prev => {
      const config = prev[moduleName];
      if (!config) return prev;
      
      const newTabs = [...config.tabs].sort((a, b) => a.order - b.order);
      const newIndex = direction === 'up' ? tabIndex - 1 : tabIndex + 1;
      if (newIndex < 0 || newIndex >= newTabs.length) return prev;
      
      [newTabs[tabIndex], newTabs[newIndex]] = [newTabs[newIndex], newTabs[tabIndex]];
      newTabs.forEach((t, i) => t.order = i);
      
      return {
        ...prev,
        [moduleName]: { ...config, tabs: newTabs }
      };
    });
  };

  // Toggle linked entity
  const handleToggleLinkedEntity = (moduleName: string, entityName: string) => {
    setModuleConfigs(prev => {
      const config = prev[moduleName];
      if (!config) return prev;
      return {
        ...prev,
        [moduleName]: {
          ...config,
          linkedEntities: config.linkedEntities.map(le => 
            le.entityName === entityName ? { ...le, enabled: !le.enabled } : le
          )
        }
      };
    });
  };

  // Remove linked entity
  const handleRemoveLinkedEntity = (moduleName: string, entityName: string) => {
    setModuleConfigs(prev => {
      const config = prev[moduleName];
      if (!config) return prev;
      return {
        ...prev,
        [moduleName]: {
          ...config,
          linkedEntities: config.linkedEntities.filter(le => le.entityName !== entityName)
        }
      };
    });
  };

  // Add linked entity
  const handleAddLinkedEntity = () => {
    if (!newLinkEntity || !linkDialogModule) return;
    
    setModuleConfigs(prev => {
      const config = prev[linkDialogModule];
      if (!config || config.linkedEntities.some(le => le.entityName === newLinkEntity)) return prev;
      
      return {
        ...prev,
        [linkDialogModule]: {
          ...config,
          linkedEntities: [...config.linkedEntities, {
            entityName: newLinkEntity,
            relationshipType: 'one-to-many',
            enabled: true,
            tabName: newLinkEntity,
            displayOrder: config.linkedEntities.length
          }]
        }
      };
    });
    
    setNewLinkEntity('');
    setLinkDialogOpen(false);
  };

  // Edit field
  const handleEditField = (field: FieldConfig) => {
    setEditingField({ ...field });
    setEditDialogOpen(true);
  };

  const handleSaveFieldEdit = () => {
    if (!editingField) return;
    
    setModuleConfigs(prev => {
      const config = prev[editingField.moduleName];
      if (!config) return prev;
      return {
        ...prev,
        [editingField.moduleName]: {
          ...config,
          fields: config.fields.map(f => f.id === editingField.id ? editingField : f)
        }
      };
    });
    
    setEditDialogOpen(false);
  };

  // Field drag and drop
  const handleFieldDragEnd = (result: DropResult, moduleName: string, tabName: string) => {
    if (!result.destination) return;

    setModuleConfigs(prev => {
      const config = prev[moduleName];
      if (!config) return prev;

      const tabFields = config.fields
        .filter(f => f.tabName === tabName)
        .sort((a, b) => a.displayOrder - b.displayOrder);
      
      const [reorderedItem] = tabFields.splice(result.source.index, 1);
      tabFields.splice(result.destination!.index, 0, reorderedItem);
      
      const updatedFieldIds = new Set(tabFields.map(f => f.id));
      const otherFields = config.fields.filter(f => !updatedFieldIds.has(f.id));
      
      tabFields.forEach((f, idx) => f.displayOrder = idx);

      return {
        ...prev,
        [moduleName]: {
          ...config,
          fields: [...otherFields, ...tabFields]
        }
      };
    });
  };

  // Save all changes for a module
  const handleSaveModuleConfig = async (moduleName: string) => {
    const config = moduleConfigs[moduleName];
    if (!config) return;

    try {
      setSaving(true);
      setError(null);

      await apiClient.put(`/moduleuiconfig/${moduleName}/complete`, {
        tabs: config.tabs,
        fields: config.fields.map(f => ({
          id: f.id,
          isEnabled: f.isEnabled,
          isRequired: f.isRequired,
          displayOrder: f.displayOrder,
          gridSize: f.gridSize,
          fieldType: f.fieldType,
          fieldLabel: f.fieldLabel,
          placeholder: f.placeholder,
          helpText: f.helpText,
          options: f.options
        })),
        linkedEntities: config.linkedEntities
      });

      setSuccess(`Configuration saved for ${moduleName}`);
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save configuration');
    } finally {
      setSaving(false);
    }
  };

  // Reset module to defaults
  const handleResetToDefaults = async (moduleName: string) => {
    if (!window.confirm(`Reset all configurations for ${moduleName} to defaults? This will delete custom field settings.`)) {
      return;
    }

    try {
      setSaving(true);
      setError(null);

      await apiClient.post(`/moduleuiconfig/${moduleName}/reset-defaults`);
      
      // Reload the config
      await loadModuleConfig(moduleName);
      
      setSuccess(`Reset ${moduleName} configuration to defaults`);
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to reset to defaults');
    } finally {
      setSaving(false);
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

  if (loading && modules.length === 0) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}

      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h6" sx={{ fontWeight: 600 }}>
            Module Configuration
          </Typography>
          <Typography variant="body2" color="textSecondary">
            Expand modules to configure tabs, fields, and linked entities in a single integrated view
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

      {/* Module Tree View */}
      <Paper sx={{ borderRadius: 2, overflow: 'hidden' }}>
        {modules.map((module) => {
          const isExpanded = expandedModules.has(module.moduleName);
          const config = moduleConfigs[module.moduleName];
          const fieldCount = config?.fields?.length || 0;
          const tabCount = config?.tabs?.length || 0;
          const linkCount = config?.linkedEntities?.length || 0;

          return (
            <Box key={module.moduleName}>
              {/* Module Row */}
              <Box
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  p: 2,
                  bgcolor: isExpanded ? 'primary.light' : (module.isEnabled ? 'background.paper' : 'action.disabledBackground'),
                  borderBottom: '1px solid',
                  borderColor: 'divider',
                  cursor: 'pointer',
                  transition: 'all 0.2s',
                  '&:hover': { bgcolor: isExpanded ? 'primary.light' : 'action.hover' }
                }}
                onClick={() => toggleModuleExpand(module.moduleName)}
              >
                <IconButton size="small" sx={{ mr: 1 }}>
                  {isExpanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                </IconButton>
                
                <Box sx={{ color: module.isEnabled ? 'primary.main' : 'text.disabled', mr: 1.5, display: 'flex' }}>
                  {moduleIcons[module.moduleName] || <ViewModuleIcon />}
                </Box>
                
                <Box sx={{ flex: 1 }}>
                  <Typography variant="subtitle1" sx={{ fontWeight: 600, color: module.isEnabled ? 'text.primary' : 'text.disabled' }}>
                    {module.displayName || module.moduleName}
                  </Typography>
                  <Typography variant="caption" color="textSecondary">
                    {module.description || 'Click to configure'}
                  </Typography>
                </Box>

                {isExpanded && config && (
                  <Stack direction="row" spacing={1} sx={{ mr: 2 }}>
                    <Chip label={`${tabCount} Tabs`} size="small" color="secondary" variant="outlined" />
                    <Chip label={`${fieldCount} Fields`} size="small" color="primary" variant="outlined" />
                    <Chip label={`${linkCount} Links`} size="small" color="info" variant="outlined" />
                  </Stack>
                )}

                <Chip 
                  label={module.isEnabled ? 'Enabled' : 'Disabled'}
                  size="small"
                  color={module.isEnabled ? 'success' : 'default'}
                  sx={{ mr: 1 }}
                />
                
                <Switch
                  checked={module.isEnabled}
                  onChange={(e) => {
                    e.stopPropagation();
                    handleToggleModule(module.moduleName, !module.isEnabled);
                  }}
                  onClick={(e) => e.stopPropagation()}
                  size="small"
                />
              </Box>

              {/* Expanded Module Content */}
              <Collapse in={isExpanded}>
                {config ? (
                  <Box sx={{ bgcolor: 'grey.50', p: 2 }}>
                    {/* Action Buttons */}
                    <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1, mb: 2 }}>
                      <Button
                        variant="outlined"
                        size="small"
                        startIcon={<ResetIcon />}
                        onClick={() => handleResetToDefaults(module.moduleName)}
                        color="warning"
                      >
                        Reset to Defaults
                      </Button>
                      <Button
                        variant="contained"
                        size="small"
                        startIcon={saving ? <CircularProgress size={16} color="inherit" /> : <SaveIcon />}
                        onClick={() => handleSaveModuleConfig(module.moduleName)}
                        disabled={saving}
                        sx={{ bgcolor: '#6750A4' }}
                      >
                        Save All Changes
                      </Button>
                    </Box>

                    {/* Tabs Section */}
                    {config.tabs.sort((a, b) => a.order - b.order).map((tab, tabIdx) => {
                      const tabKey = `${module.moduleName}-${tab.name}`;
                      const isTabExpanded = expandedTabs.has(tabKey);
                      const tabFields = getFieldsForTab(module.moduleName, tab.name);

                      return (
                        <Box key={tab.name} sx={{ mb: 1 }}>
                          {/* Tab Header */}
                          <Box
                            sx={{
                              display: 'flex',
                              alignItems: 'center',
                              p: 1.5,
                              pl: 4,
                              bgcolor: tab.enabled ? 'background.paper' : 'action.disabledBackground',
                              borderRadius: 1,
                              border: '1px solid',
                              borderColor: isTabExpanded ? 'primary.main' : 'divider',
                              cursor: 'pointer',
                              '&:hover': { borderColor: 'primary.main' }
                            }}
                            onClick={() => toggleTabExpand(tabKey)}
                          >
                            <IconButton size="small" sx={{ mr: 1 }}>
                              {isTabExpanded ? <FolderOpenIcon color="primary" /> : <FolderIcon />}
                            </IconButton>
                            
                            <Typography variant="subtitle2" sx={{ fontWeight: 600, flex: 1 }}>
                              {tab.name}
                            </Typography>
                            
                            <Chip 
                              label={`${tabFields.length} fields`} 
                              size="small" 
                              sx={{ mr: 1 }}
                            />
                            
                            <Chip 
                              label={tab.enabled ? 'Visible' : 'Hidden'} 
                              size="small" 
                              color={tab.enabled ? 'success' : 'default'}
                              variant="outlined"
                              sx={{ mr: 1 }}
                            />

                            <Box onClick={(e) => e.stopPropagation()} sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                              <IconButton size="small" onClick={() => handleMoveTab(module.moduleName, tabIdx, 'up')} disabled={tabIdx === 0}>
                                <ArrowUpIcon fontSize="small" />
                              </IconButton>
                              <IconButton size="small" onClick={() => handleMoveTab(module.moduleName, tabIdx, 'down')} disabled={tabIdx === config.tabs.length - 1}>
                                <ArrowDownIcon fontSize="small" />
                              </IconButton>
                              <Switch
                                checked={tab.enabled}
                                onChange={() => handleToggleTab(module.moduleName, tab.name)}
                                size="small"
                              />
                            </Box>
                          </Box>

                          {/* Fields Table */}
                          <Collapse in={isTabExpanded}>
                            <Box sx={{ pl: 6, pr: 2, py: 1 }}>
                              <DragDropContext onDragEnd={(result) => handleFieldDragEnd(result, module.moduleName, tab.name)}>
                                <Droppable droppableId={`fields-${module.moduleName}-${tab.name}`}>
                                  {(provided) => (
                                    <Table {...provided.droppableProps} ref={provided.innerRef} size="small" sx={{ bgcolor: 'background.paper', borderRadius: 1 }}>
                                      <TableHead>
                                        <TableRow sx={{ bgcolor: 'grey.100' }}>
                                          <TableCell width="40"></TableCell>
                                          <TableCell><strong>Field</strong></TableCell>
                                          <TableCell width="150"><strong>Type</strong></TableCell>
                                          <TableCell width="80"><strong>Width</strong></TableCell>
                                          <TableCell width="70" align="center"><strong>Show</strong></TableCell>
                                          <TableCell width="70" align="center"><strong>Req</strong></TableCell>
                                          <TableCell width="50" align="center"><strong>Edit</strong></TableCell>
                                        </TableRow>
                                      </TableHead>
                                      <TableBody>
                                        {tabFields.map((field, idx) => (
                                          <Draggable
                                            key={field.id}
                                            draggableId={`field-${field.id}`}
                                            index={idx}
                                            isDragDisabled={!field.isReorderable}
                                          >
                                            {(provided, snapshot) => (
                                              <TableRow
                                                ref={provided.innerRef}
                                                {...provided.draggableProps}
                                                sx={{
                                                  bgcolor: snapshot.isDragging ? 'primary.light' : field.isEnabled ? 'transparent' : 'action.disabledBackground',
                                                  opacity: field.isEnabled ? 1 : 0.6,
                                                  '&:hover': { bgcolor: 'action.hover' }
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
                                                  <Typography variant="body2" fontWeight={500}>{field.fieldLabel}</Typography>
                                                  <Typography variant="caption" color="textSecondary">{field.fieldName}</Typography>
                                                </TableCell>
                                                <TableCell>
                                                  <FormControl size="small" fullWidth>
                                                    <Select
                                                      value={field.fieldType}
                                                      onChange={(e) => handleChangeFieldType(module.moduleName, field.id, e.target.value)}
                                                      sx={{ fontSize: '0.75rem', height: 28 }}
                                                    >
                                                      {FIELD_TYPES.map(type => (
                                                        <MenuItem key={type.value} value={type.value} sx={{ fontSize: '0.8rem' }}>
                                                          {type.label}
                                                        </MenuItem>
                                                      ))}
                                                    </Select>
                                                  </FormControl>
                                                </TableCell>
                                                <TableCell>
                                                  <Chip 
                                                    label={field.gridSize === 12 ? 'Full' : field.gridSize === 6 ? 'Half' : `${field.gridSize}/12`} 
                                                    size="small" 
                                                    variant="outlined"
                                                  />
                                                </TableCell>
                                                <TableCell align="center">
                                                  <IconButton
                                                    size="small"
                                                    onClick={() => handleToggleFieldEnabled(module.moduleName, field.id)}
                                                    disabled={!field.isHideable}
                                                    color={field.isEnabled ? 'primary' : 'default'}
                                                  >
                                                    {field.isEnabled ? <VisibilityIcon fontSize="small" /> : <VisibilityOffIcon fontSize="small" />}
                                                  </IconButton>
                                                </TableCell>
                                                <TableCell align="center">
                                                  <Checkbox
                                                    checked={field.isRequired}
                                                    onChange={() => handleToggleFieldRequired(module.moduleName, field.id)}
                                                    disabled={!field.isRequiredConfigurable}
                                                    size="small"
                                                  />
                                                </TableCell>
                                                <TableCell align="center">
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
                          </Collapse>
                        </Box>
                      );
                    })}

                    {/* Linked Entities & Master Data Section */}
                    <Box sx={{ mt: 2 }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
                        <Typography variant="subtitle2" sx={{ fontWeight: 600, display: 'flex', alignItems: 'center', gap: 1 }}>
                          <LinkIcon fontSize="small" />
                          Linked Entities & Field Relationships
                        </Typography>
                        <Button
                          size="small"
                          startIcon={<AddIcon />}
                          onClick={() => {
                            setLinkDialogModule(module.moduleName);
                            setLinkDialogOpen(true);
                          }}
                        >
                          Add Link
                        </Button>
                      </Box>

                      {/* Database Foreign Keys - Full Width with Field Details */}
                      <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
                        <Typography variant="subtitle2" fontWeight={600} sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1.5 }}>
                          <StorageIcon fontSize="small" color="primary" />
                          Database Foreign Key Relationships
                        </Typography>
                        {(() => {
                          const tableKeys = databaseForeignKeys[module.moduleName] || [];
                          if (tableKeys.length === 0) {
                            return (
                              <Typography variant="body2" color="textSecondary" sx={{ py: 1 }}>
                                No foreign key relationships found for this module
                              </Typography>
                            );
                          }
                          
                          // Group by referenced table for better display
                          const groupedByTarget: Record<string, any[]> = {};
                          tableKeys.forEach((fk: any) => {
                            const target = fk.referencedTable;
                            if (!groupedByTarget[target]) {
                              groupedByTarget[target] = [];
                            }
                            groupedByTarget[target].push(fk);
                          });

                          return (
                            <Box>
                              {Object.entries(groupedByTarget).map(([targetTable, fks]) => (
                                <Paper 
                                  key={targetTable} 
                                  variant="outlined" 
                                  sx={{ 
                                    p: 1.5, 
                                    mb: 1, 
                                    bgcolor: 'grey.50',
                                    borderLeft: '3px solid',
                                    borderLeftColor: 'primary.main'
                                  }}
                                >
                                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                                    {moduleIcons[targetTable] || <StorageIcon fontSize="small" />}
                                    <Typography variant="subtitle2" fontWeight={600} color="primary">
                                      → {targetTable}
                                    </Typography>
                                    <Chip 
                                      label={`${fks.length} link${fks.length > 1 ? 's' : ''}`} 
                                      size="small" 
                                      variant="outlined"
                                      sx={{ ml: 'auto' }}
                                    />
                                  </Box>
                                  <Table size="small" sx={{ bgcolor: 'background.paper', borderRadius: 1 }}>
                                    <TableHead>
                                      <TableRow sx={{ bgcolor: 'grey.100' }}>
                                        <TableCell sx={{ py: 0.5, fontWeight: 600, fontSize: '0.75rem' }}>
                                          Source Field ({module.moduleName})
                                        </TableCell>
                                        <TableCell sx={{ py: 0.5, width: 40, textAlign: 'center' }}></TableCell>
                                        <TableCell sx={{ py: 0.5, fontWeight: 600, fontSize: '0.75rem' }}>
                                          Target Field ({targetTable})
                                        </TableCell>
                                        <TableCell sx={{ py: 0.5, fontWeight: 600, fontSize: '0.75rem', width: 120 }}>
                                          On Delete
                                        </TableCell>
                                      </TableRow>
                                    </TableHead>
                                    <TableBody>
                                      {fks.map((fk: any, idx: number) => (
                                        <TableRow key={`${fk.constraintName}-${idx}`} sx={{ '&:last-child td': { borderBottom: 0 } }}>
                                          <TableCell sx={{ py: 0.75 }}>
                                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                                              <Chip 
                                                label={fk.sourceColumn} 
                                                size="small" 
                                                color="default"
                                                variant="outlined"
                                                sx={{ fontFamily: 'monospace', fontSize: '0.7rem' }}
                                              />
                                            </Box>
                                          </TableCell>
                                          <TableCell sx={{ py: 0.75, textAlign: 'center' }}>
                                            <ChevronRightIcon sx={{ fontSize: 16, color: 'primary.main' }} />
                                          </TableCell>
                                          <TableCell sx={{ py: 0.75 }}>
                                            <Chip 
                                              label={fk.referencedColumn} 
                                              size="small" 
                                              color="primary"
                                              variant="outlined"
                                              sx={{ fontFamily: 'monospace', fontSize: '0.7rem' }}
                                            />
                                          </TableCell>
                                          <TableCell sx={{ py: 0.75 }}>
                                            <Typography variant="caption" color="textSecondary">
                                              {fk.onDelete || 'NO ACTION'}
                                            </Typography>
                                          </TableCell>
                                        </TableRow>
                                      ))}
                                    </TableBody>
                                  </Table>
                                </Paper>
                              ))}
                            </Box>
                          );
                        })()}
                      </Paper>

                      {/* Configured Entity Links */}
                      <Paper variant="outlined" sx={{ p: 2 }}>
                        <Typography variant="subtitle2" fontWeight={600} sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1.5 }}>
                          <LinkIcon fontSize="small" color="info" />
                          Configured Entity Links
                        </Typography>
                        {config.linkedEntities.length === 0 ? (
                          <Typography variant="body2" color="textSecondary" sx={{ py: 1 }}>
                            No custom entity links configured. Use "Add Link" to create relationships.
                          </Typography>
                        ) : (
                          <Grid container spacing={1}>
                            {config.linkedEntities.map((entity) => (
                              <Grid item xs={12} sm={6} md={4} key={entity.entityName}>
                                <Paper 
                                  variant="outlined" 
                                  sx={{ 
                                    p: 1.5, 
                                    display: 'flex', 
                                    alignItems: 'center',
                                    gap: 1,
                                    bgcolor: entity.enabled ? 'background.paper' : 'action.disabledBackground',
                                    opacity: entity.enabled ? 1 : 0.7
                                  }}
                                >
                                  <Box sx={{ color: entity.enabled ? 'info.main' : 'text.disabled' }}>
                                    {moduleIcons[entity.entityName] || <LinkIcon fontSize="small" />}
                                  </Box>
                                  <Box sx={{ flex: 1 }}>
                                    <Typography variant="body2" fontWeight={500}>
                                      {entity.entityName}
                                    </Typography>
                                    <Typography variant="caption" color="textSecondary">
                                      {entity.relationshipType || 'one-to-many'}
                                    </Typography>
                                  </Box>
                                  <Switch
                                    checked={entity.enabled}
                                    onChange={() => handleToggleLinkedEntity(module.moduleName, entity.entityName)}
                                    size="small"
                                  />
                                  <IconButton 
                                    size="small" 
                                    onClick={() => handleRemoveLinkedEntity(module.moduleName, entity.entityName)}
                                    color="error"
                                  >
                                    <DeleteIcon fontSize="small" />
                                  </IconButton>
                                </Paper>
                              </Grid>
                            ))}
                          </Grid>
                        )}
                      </Paper>
                    </Box>

                    {config.fields.length === 0 && (
                      <Alert severity="info" sx={{ mt: 2 }}>
                        No field configurations found for this module. Click "Reset to Defaults" to initialize fields from the database schema, or field configurations may not be available for this module type yet.
                      </Alert>
                    )}
                  </Box>
                ) : (
                  <Box sx={{ p: 3, textAlign: 'center', bgcolor: 'grey.50' }}>
                    <CircularProgress size={24} />
                    <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>
                      Loading configuration...
                    </Typography>
                  </Box>
                )}
              </Collapse>
            </Box>
          );
        })}
      </Paper>

      {/* Help */}
      <Paper sx={{ p: 2, bgcolor: 'warning.light', borderRadius: 2, mt: 3 }}>
        <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
          ⚠️ Important Notes
        </Typography>
        <Typography variant="body2">
          • <strong>Expand a module</strong> to see its tabs, fields, and linked entities in one integrated view<br />
          • <strong>Expand a tab</strong> to view and configure its fields (drag to reorder)<br />
          • <strong>Save All Changes</strong> saves tabs, fields, and linked entities in a single operation<br />
          • <strong>Reset to Defaults</strong> will reinitialize field configurations from the database schema
        </Typography>
      </Paper>

      {/* Edit Field Dialog */}
      <Dialog open={editDialogOpen} onClose={() => setEditDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <EditIcon />
            Edit Field Settings
          </Box>
        </DialogTitle>
        <DialogContent>
          {editingField && (
            <Grid container spacing={2} sx={{ mt: 1 }}>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Field Label"
                  value={editingField.fieldLabel}
                  onChange={(e) => setEditingField({ ...editingField, fieldLabel: e.target.value })}
                  helperText="Display name shown to users"
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Field Name"
                  value={editingField.fieldName}
                  disabled
                  helperText="Database field name (read-only)"
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth>
                  <InputLabel>Field Type</InputLabel>
                  <Select
                    value={editingField.fieldType}
                    onChange={(e) => setEditingField({ ...editingField, fieldType: e.target.value })}
                    label="Field Type"
                  >
                    {FIELD_TYPES.map(type => (
                      <MenuItem key={type.value} value={type.value}>
                        <Box>
                          <Typography variant="body2">{type.label}</Typography>
                          <Typography variant="caption" color="textSecondary">{type.description}</Typography>
                        </Box>
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth>
                  <InputLabel>Grid Size</InputLabel>
                  <Select
                    value={editingField.gridSize}
                    onChange={(e) => setEditingField({ ...editingField, gridSize: Number(e.target.value) })}
                    label="Grid Size"
                  >
                    {[1, 2, 3, 4, 6, 12].map(size => (
                      <MenuItem key={size} value={size}>
                        {size === 12 ? 'Full Width' : size === 6 ? 'Half Width' : `${size}/12 columns`}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Placeholder"
                  value={editingField.placeholder || ''}
                  onChange={(e) => setEditingField({ ...editingField, placeholder: e.target.value })}
                  helperText="Placeholder text shown when field is empty"
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
                  helperText="Additional instructions shown below the field"
                />
              </Grid>
              {(editingField.fieldType === 'select' || editingField.fieldType === 'multiselect' || editingField.fieldType === 'radio') && (
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Options (comma-separated or JSON)"
                    value={editingField.options || ''}
                    onChange={(e) => setEditingField({ ...editingField, options: e.target.value })}
                    multiline
                    rows={3}
                    placeholder='Option1, Option2, Option3 or [{"value":"1","label":"Option 1"}]'
                    helperText="Enter options as comma-separated values or JSON array"
                  />
                </Grid>
              )}
              <Grid item xs={12}>
                <Divider sx={{ my: 1 }} />
                <Typography variant="subtitle2" sx={{ mb: 1 }}>Field Behavior</Typography>
                <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={editingField.isEnabled}
                        onChange={(e) => setEditingField({ ...editingField, isEnabled: e.target.checked })}
                        disabled={!editingField.isHideable}
                      />
                    }
                    label="Visible"
                  />
                  <FormControlLabel
                    control={
                      <Switch
                        checked={editingField.isRequired}
                        onChange={(e) => setEditingField({ ...editingField, isRequired: e.target.checked })}
                        disabled={!editingField.isRequiredConfigurable}
                      />
                    }
                    label="Required"
                  />
                </Box>
              </Grid>
            </Grid>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleSaveFieldEdit} variant="contained" sx={{ backgroundColor: '#6750A4' }}>
            Apply Changes
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
                .filter(m => {
                  const config = moduleConfigs[linkDialogModule];
                  return m !== linkDialogModule && (!config || !config.linkedEntities.some(le => le.entityName === m));
                })
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
