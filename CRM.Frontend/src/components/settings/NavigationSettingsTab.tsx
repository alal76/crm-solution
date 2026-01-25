/**
 * CRM Solution - Navigation Settings Tab
 * Allows admins to reorder and configure navigation menu items
 */

import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Button,
  Alert,
  CircularProgress,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  ListItemSecondaryAction,
  IconButton,
  Switch,
  Divider,
  Paper,
  Chip,
  Tooltip,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Collapse,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
} from '@mui/material';
import {
  DragIndicator as DragIcon,
  ArrowUpward as ArrowUpIcon,
  ArrowDownward as ArrowDownIcon,
  Visibility as VisibleIcon,
  VisibilityOff as HiddenIcon,
  Save as SaveIcon,
  Restore as RestoreIcon,
  Dashboard as DashboardIcon,
  People as PeopleIcon,
  PersonSearch as PersonSearchIcon,
  TrendingUp as TrendingUpIcon,
  Inventory2 as PackageIcon,
  Campaign as MegaphoneIcon,
  Settings as SettingsIcon,
  SupportAgent as SupportAgentIcon,
  Assignment as TaskIcon,
  Description as QuoteIcon,
  Note as NoteIcon,
  Timeline as ActivityIcon,
  AutoAwesome as AutomationIcon,
  Edit as EditIcon,
  ExpandMore as ExpandMoreIcon,
  Category as CategoryIcon,
  Add as AddIcon,
  Delete as DeleteIcon,
  Message as CommunicationsIcon,
  SwapHoriz as InteractionsIcon,
  Tune as ChannelSettingsIcon,
  Storage as StorageIcon,
  Cloud as CloudIcon,
  MonitorHeart as MonitorIcon,
  Security as SecurityIcon,
  ToggleOn as FeatureToggleIcon,
  PersonAdd as PersonAddIcon,
  Groups as GroupsIcon,
  Login as LoginIcon,
  Palette as PaletteIcon,
  ViewModule as ModuleIcon,
  Menu as MenuIcon,
} from '@mui/icons-material';

interface NavCategory {
  id: string;
  label: string;
  order: number;
}

interface NavItem {
  id: string;
  label: string;
  menuName: string;
  icon: string;
  order: number;
  visible: boolean;
  isAdmin: boolean;
  category: string;
  customLabel?: string; // User-defined custom label
}

// Default categories for grouping menu items
const DEFAULT_CATEGORIES = [
  { id: 'main', label: 'Main', order: 0 },
  { id: 'sales', label: 'Sales & Marketing', order: 1 },
  { id: 'support', label: 'Customer Support', order: 2 },
  { id: 'productivity', label: 'Productivity', order: 3 },
  { id: 'admin', label: 'Administration', order: 4 },
];

const DEFAULT_NAV_ITEMS: NavItem[] = [
  { id: 'dashboard', label: 'Dashboard', menuName: 'Dashboard', icon: 'DashboardIcon', order: 0, visible: true, isAdmin: false, category: 'main' },
  { id: 'customers', label: 'Customers', menuName: 'Customers', icon: 'PeopleIcon', order: 1, visible: true, isAdmin: false, category: 'main' },
  { id: 'customer-overview', label: 'Customer Overview', menuName: 'CustomerOverview', icon: 'PersonSearchIcon', order: 2, visible: true, isAdmin: false, category: 'main' },
  { id: 'contacts', label: 'Contacts', menuName: 'Contacts', icon: 'PeopleIcon', order: 3, visible: true, isAdmin: false, category: 'main' },
  { id: 'leads', label: 'Leads', menuName: 'Leads', icon: 'PeopleIcon', order: 4, visible: true, isAdmin: false, category: 'sales' },
  { id: 'opportunities', label: 'Opportunities', menuName: 'Opportunities', icon: 'TrendingUpIcon', order: 5, visible: true, isAdmin: false, category: 'sales' },
  { id: 'products', label: 'Products', menuName: 'Products', icon: 'PackageIcon', order: 6, visible: true, isAdmin: false, category: 'sales' },
  { id: 'services', label: 'Services', menuName: 'Services', icon: 'SettingsIcon', order: 7, visible: true, isAdmin: false, category: 'support' },
  { id: 'service-requests', label: 'Service Requests', menuName: 'ServiceRequests', icon: 'SupportAgentIcon', order: 8, visible: true, isAdmin: false, category: 'support' },
  { id: 'campaigns', label: 'Campaigns', menuName: 'Campaigns', icon: 'MegaphoneIcon', order: 9, visible: true, isAdmin: false, category: 'sales' },
  { id: 'quotes', label: 'Quotes', menuName: 'Quotes', icon: 'QuoteIcon', order: 10, visible: true, isAdmin: false, category: 'sales' },
  { id: 'my-queue', label: 'My Queue', menuName: 'MyQueue', icon: 'TaskIcon', order: 11, visible: true, isAdmin: false, category: 'productivity' },
  { id: 'activities', label: 'Activities', menuName: 'Activities', icon: 'ActivityIcon', order: 12, visible: true, isAdmin: false, category: 'productivity' },
  { id: 'notes', label: 'Notes', menuName: 'Notes', icon: 'NoteIcon', order: 13, visible: true, isAdmin: false, category: 'productivity' },
  { id: 'communications', label: 'Communications', menuName: 'Communications', icon: 'CommunicationsIcon', order: 14, visible: true, isAdmin: false, category: 'productivity' },
  { id: 'interactions', label: 'Interactions', menuName: 'Interactions', icon: 'InteractionsIcon', order: 15, visible: true, isAdmin: false, category: 'productivity' },
  { id: 'workflows', label: 'Workflows', menuName: 'Workflows', icon: 'AutomationIcon', order: 16, visible: true, isAdmin: true, category: 'admin' },
  // System Administration
  { id: 'database-settings', label: 'Database', menuName: 'DatabaseSettings', icon: 'StorageIcon', order: 17, visible: true, isAdmin: true, category: 'admin' },
  { id: 'deployment-settings', label: 'Deployment', menuName: 'DeploymentSettings', icon: 'CloudIcon', order: 18, visible: true, isAdmin: true, category: 'admin' },
  { id: 'monitoring-settings', label: 'Monitoring', menuName: 'MonitoringSettings', icon: 'MonitorIcon', order: 19, visible: true, isAdmin: true, category: 'admin' },
  { id: 'security-settings', label: 'Security', menuName: 'SecuritySettings', icon: 'SecurityIcon', order: 20, visible: true, isAdmin: true, category: 'admin' },
  { id: 'feature-management', label: 'Features', menuName: 'FeatureManagement', icon: 'FeatureToggleIcon', order: 21, visible: true, isAdmin: true, category: 'admin' },
  // User Administration
  { id: 'user-management', label: 'Users', menuName: 'UserManagement', icon: 'PeopleIcon', order: 22, visible: true, isAdmin: true, category: 'admin' },
  { id: 'user-approvals', label: 'Approvals', menuName: 'UserApprovals', icon: 'PersonAddIcon', order: 23, visible: true, isAdmin: true, category: 'admin' },
  { id: 'group-management', label: 'Groups', menuName: 'GroupManagement', icon: 'GroupsIcon', order: 24, visible: true, isAdmin: true, category: 'admin' },
  { id: 'social-login', label: 'Social Login', menuName: 'SocialLogin', icon: 'LoginIcon', order: 25, visible: true, isAdmin: true, category: 'admin' },
  // CRM Administration
  { id: 'branding-settings', label: 'Branding', menuName: 'BrandingSettings', icon: 'PaletteIcon', order: 26, visible: true, isAdmin: true, category: 'admin' },
  { id: 'navigation-settings', label: 'Navigation', menuName: 'NavigationSettings', icon: 'MenuIcon', order: 27, visible: true, isAdmin: true, category: 'admin' },
  { id: 'module-fields', label: 'Modules & Fields', menuName: 'ModuleFields', icon: 'ModuleIcon', order: 28, visible: true, isAdmin: true, category: 'admin' },
  { id: 'sr-definitions', label: 'Service Requests', menuName: 'ServiceRequestDefinitions', icon: 'SupportAgentIcon', order: 29, visible: true, isAdmin: true, category: 'admin' },
  { id: 'master-data', label: 'Master Data', menuName: 'MasterData', icon: 'StorageIcon', order: 30, visible: true, isAdmin: true, category: 'admin' },
  // Legacy items
  { id: 'channel-settings', label: 'Channel Settings', menuName: 'ChannelSettings', icon: 'ChannelSettingsIcon', order: 31, visible: true, isAdmin: true, category: 'admin' },
  { id: 'settings', label: 'All Settings', menuName: 'Settings', icon: 'SettingsIcon', order: 32, visible: true, isAdmin: true, category: 'admin' },
];

const iconMap: Record<string, React.ReactNode> = {
  DashboardIcon: <DashboardIcon />,
  PeopleIcon: <PeopleIcon />,
  PersonSearchIcon: <PersonSearchIcon />,
  TrendingUpIcon: <TrendingUpIcon />,
  PackageIcon: <PackageIcon />,
  MegaphoneIcon: <MegaphoneIcon />,
  SettingsIcon: <SettingsIcon />,
  SupportAgentIcon: <SupportAgentIcon />,
  TaskIcon: <TaskIcon />,
  QuoteIcon: <QuoteIcon />,
  NoteIcon: <NoteIcon />,
  ActivityIcon: <ActivityIcon />,
  AutomationIcon: <AutomationIcon />,
  CommunicationsIcon: <CommunicationsIcon />,
  InteractionsIcon: <InteractionsIcon />,
  ChannelSettingsIcon: <ChannelSettingsIcon />,
  StorageIcon: <StorageIcon />,
  CloudIcon: <CloudIcon />,
  MonitorIcon: <MonitorIcon />,
  SecurityIcon: <SecurityIcon />,
  FeatureToggleIcon: <FeatureToggleIcon />,
  PersonAddIcon: <PersonAddIcon />,
  GroupsIcon: <GroupsIcon />,
  LoginIcon: <LoginIcon />,
  PaletteIcon: <PaletteIcon />,
  ModuleIcon: <ModuleIcon />,
  MenuIcon: <MenuIcon />,
};

function NavigationSettingsTab() {
  const [navItems, setNavItems] = useState<NavItem[]>([]);
  const [categories, setCategories] = useState<NavCategory[]>(DEFAULT_CATEGORIES);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [success, setSuccess] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [hasChanges, setHasChanges] = useState(false);
  const [editingItem, setEditingItem] = useState<NavItem | null>(null);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [expandedCategories, setExpandedCategories] = useState<string[]>(DEFAULT_CATEGORIES.map(c => c.id));
  const [viewMode, setViewMode] = useState<'list' | 'category'>('category');
  // Category edit state
  const [editingCategory, setEditingCategory] = useState<NavCategory | null>(null);
  const [categoryDialogOpen, setCategoryDialogOpen] = useState(false);
  const [newCategoryName, setNewCategoryName] = useState('');

  const getApiUrl = () => {
    return window.location.hostname === 'localhost'
      ? 'http://localhost:5000/api'
      : `http://${window.location.hostname}:5000/api`;
  };

  const loadSettings = useCallback(async () => {
    try {
      const token = localStorage.getItem('accessToken');
      const response = await fetch(`${getApiUrl()}/systemsettings`, {
        headers: { Authorization: `Bearer ${token}` },
      });

      if (response.ok) {
        const data = await response.json();
        if (data.navOrderConfig) {
          try {
            const savedConfig = JSON.parse(data.navOrderConfig);
            // Support both old format (array) and new format (object with navItems and categories)
            const savedOrder = Array.isArray(savedConfig) ? savedConfig : savedConfig.navItems || [];
            const savedCategories = savedConfig.categories || null;
            
            // Load categories
            if (savedCategories && Array.isArray(savedCategories)) {
              setCategories(savedCategories);
              setExpandedCategories(savedCategories.map((c: NavCategory) => c.id));
            }
            
            // Merge saved order with default items
            const mergedItems = DEFAULT_NAV_ITEMS.map(item => {
              const saved = savedOrder.find((s: NavItem) => s.id === item.id);
              return saved ? { 
                ...item, 
                order: saved.order, 
                visible: saved.visible,
                category: saved.category || item.category,
                customLabel: saved.customLabel,
              } : item;
            });
            mergedItems.sort((a, b) => a.order - b.order);
            setNavItems(mergedItems);
          } catch {
            setNavItems([...DEFAULT_NAV_ITEMS]);
          }
        } else {
          setNavItems([...DEFAULT_NAV_ITEMS]);
        }
      } else {
        setNavItems([...DEFAULT_NAV_ITEMS]);
      }
    } catch (err) {
      console.error('Error loading nav settings:', err);
      setNavItems([...DEFAULT_NAV_ITEMS]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadSettings();
  }, [loadSettings]);

  // Category management functions
  const handleAddCategory = () => {
    if (!newCategoryName.trim()) return;
    const newId = newCategoryName.toLowerCase().replace(/\s+/g, '-');
    const newCategory: NavCategory = {
      id: newId,
      label: newCategoryName.trim(),
      order: categories.length
    };
    setCategories([...categories, newCategory]);
    setNewCategoryName('');
    setHasChanges(true);
    setExpandedCategories([...expandedCategories, newId]);
  };

  const handleEditCategory = (category: NavCategory) => {
    setEditingCategory({ ...category });
    setCategoryDialogOpen(true);
  };

  const handleSaveCategoryEdit = () => {
    if (editingCategory) {
      const newCategories = categories.map(c => 
        c.id === editingCategory.id ? editingCategory : c
      );
      setCategories(newCategories);
      setHasChanges(true);
      setCategoryDialogOpen(false);
      setEditingCategory(null);
    }
  };

  const handleDeleteCategory = (categoryId: string) => {
    // Move all items in this category to 'main'
    const newItems = navItems.map(item => 
      item.category === categoryId ? { ...item, category: 'main' } : item
    );
    setNavItems(newItems);
    setCategories(categories.filter(c => c.id !== categoryId));
    setHasChanges(true);
  };

  const moveCategoryUp = (index: number) => {
    if (index <= 0) return;
    const newCategories = [...categories];
    [newCategories[index - 1], newCategories[index]] = [newCategories[index], newCategories[index - 1]];
    newCategories.forEach((c, i) => c.order = i);
    setCategories(newCategories);
    setHasChanges(true);
  };

  const moveCategoryDown = (index: number) => {
    if (index >= categories.length - 1) return;
    const newCategories = [...categories];
    [newCategories[index], newCategories[index + 1]] = [newCategories[index + 1], newCategories[index]];
    newCategories.forEach((c, i) => c.order = i);
    setCategories(newCategories);
    setHasChanges(true);
  };

  const moveItem = (index: number, direction: 'up' | 'down') => {
    const newItems = [...navItems];
    const targetIndex = direction === 'up' ? index - 1 : index + 1;
    
    if (targetIndex < 0 || targetIndex >= newItems.length) return;
    
    // Swap items
    [newItems[index], newItems[targetIndex]] = [newItems[targetIndex], newItems[index]];
    
    // Update order values
    newItems.forEach((item, idx) => {
      item.order = idx;
    });
    
    setNavItems(newItems);
    setHasChanges(true);
  };

  const toggleVisibility = (index: number) => {
    const newItems = [...navItems];
    newItems[index].visible = !newItems[index].visible;
    setNavItems(newItems);
    setHasChanges(true);
  };

  const handleEditClick = (item: NavItem) => {
    setEditingItem({ ...item });
    setEditDialogOpen(true);
  };

  const handleEditSave = () => {
    if (editingItem) {
      const newItems = navItems.map(item => 
        item.id === editingItem.id ? editingItem : item
      );
      setNavItems(newItems);
      setHasChanges(true);
      setEditDialogOpen(false);
      setEditingItem(null);
    }
  };

  const handleEditCancel = () => {
    setEditDialogOpen(false);
    setEditingItem(null);
  };

  const handleCategoryChange = (itemId: string, newCategory: string) => {
    const newItems = navItems.map(item => 
      item.id === itemId ? { ...item, category: newCategory } : item
    );
    setNavItems(newItems);
    setHasChanges(true);
  };

  const toggleCategoryExpanded = (categoryId: string) => {
    setExpandedCategories(prev => 
      prev.includes(categoryId) 
        ? prev.filter(id => id !== categoryId)
        : [...prev, categoryId]
    );
  };

  const getItemsByCategory = (categoryId: string) => {
    return navItems.filter(item => item.category === categoryId);
  };

  const handleSave = async () => {
    setSaving(true);
    setError(null);
    setSuccess(null);

    try {
      const token = localStorage.getItem('accessToken');
      const navOrderData = navItems.map(item => ({
        id: item.id,
        order: item.order,
        visible: item.visible,
        category: item.category,
        customLabel: item.customLabel,
      }));
      
      // Save both nav items and categories in the new format
      const configToSave = {
        navItems: navOrderData,
        categories: categories.map(c => ({ id: c.id, label: c.label, order: c.order }))
      };
      const navOrderConfig = JSON.stringify(configToSave);

      const response = await fetch(`${getApiUrl()}/systemsettings/navigation/order`, {
        method: 'PUT',
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ navOrderConfig }),
      });

      if (response.ok) {
        // Save to localStorage for immediate use by Navigation component (include categories)
        localStorage.setItem('crm_nav_order', JSON.stringify(configToSave));
        setSuccess('Navigation order saved successfully. Page will reload to apply changes.');
        setHasChanges(false);
        // Trigger page reload to apply changes
        setTimeout(() => {
          window.location.reload();
        }, 1500);
      } else {
        const errorData = await response.json();
        setError(errorData.message || 'Failed to save navigation order');
      }
    } catch (err) {
      setError('Failed to save navigation order');
    } finally {
      setSaving(false);
    }
  };

  const handleReset = () => {
    setNavItems([...DEFAULT_NAV_ITEMS]);
    setCategories([...DEFAULT_CATEGORIES]);
    setHasChanges(true);
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  const regularItems = navItems.filter(item => !item.isAdmin);
  const adminItems = navItems.filter(item => item.isAdmin);

  const renderNavItem = (item: NavItem, index: number, itemsInGroup: NavItem[]) => {
    const actualIndex = navItems.findIndex(n => n.id === item.id);
    const displayLabel = item.customLabel || item.label;
    const categoryInfo = DEFAULT_CATEGORIES.find(c => c.id === item.category);
    
    return (
      <React.Fragment key={item.id}>
        {index > 0 && <Divider />}
        <ListItem
          sx={{
            opacity: item.visible ? 1 : 0.5,
            bgcolor: item.visible ? 'transparent' : 'action.disabledBackground',
            py: 1.5,
          }}
        >
          <ListItemIcon sx={{ minWidth: 36 }}>
            <DragIcon sx={{ color: 'text.secondary' }} />
          </ListItemIcon>
          <ListItemIcon sx={{ minWidth: 40 }}>
            {iconMap[item.icon] || <SettingsIcon />}
          </ListItemIcon>
          <ListItemText
            primary={
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                {displayLabel}
                {item.customLabel && item.customLabel !== item.label && (
                  <Chip label="Renamed" size="small" color="info" sx={{ height: 18, fontSize: '0.65rem' }} />
                )}
              </Box>
            }
            secondary={
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 0.5 }}>
                <Typography variant="caption" color="text.secondary">
                  Route: {item.menuName}
                </Typography>
                <Chip 
                  label={categoryInfo?.label || item.category} 
                  size="small" 
                  sx={{ height: 18, fontSize: '0.6rem' }} 
                  variant="outlined"
                />
              </Box>
            }
          />
          <ListItemSecondaryAction>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <Tooltip title="Edit Menu Item">
                <IconButton size="small" onClick={() => handleEditClick(item)}>
                  <EditIcon fontSize="small" />
                </IconButton>
              </Tooltip>
              <Tooltip title="Move Up">
                <span>
                  <IconButton
                    size="small"
                    onClick={() => moveItem(actualIndex, 'up')}
                    disabled={index === 0}
                  >
                    <ArrowUpIcon fontSize="small" />
                  </IconButton>
                </span>
              </Tooltip>
              <Tooltip title="Move Down">
                <span>
                  <IconButton
                    size="small"
                    onClick={() => moveItem(actualIndex, 'down')}
                    disabled={index === itemsInGroup.length - 1}
                  >
                    <ArrowDownIcon fontSize="small" />
                  </IconButton>
                </span>
              </Tooltip>
              <Divider orientation="vertical" flexItem sx={{ mx: 0.5 }} />
              <Tooltip title={item.visible ? 'Hide from menu' : 'Show in menu'}>
                <Switch
                  size="small"
                  checked={item.visible}
                  onChange={() => toggleVisibility(actualIndex)}
                  icon={<HiddenIcon fontSize="small" />}
                  checkedIcon={<VisibleIcon fontSize="small" />}
                />
              </Tooltip>
            </Box>
          </ListItemSecondaryAction>
        </ListItem>
      </React.Fragment>
    );
  };

  return (
    <Box>
      <Typography variant="h6" sx={{ fontWeight: 600, mb: 1 }}>
        Navigation Settings
      </Typography>
      <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
        Customize navigation menu items by category. Click the edit button to rename items or change categories.
      </Typography>

      {success && (
        <Alert severity="success" sx={{ mb: 2, borderRadius: 2 }} onClose={() => setSuccess(null)}>
          {success}
        </Alert>
      )}

      {error && (
        <Alert severity="error" sx={{ mb: 2, borderRadius: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Action Buttons */}
      <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap', alignItems: 'center' }}>
        <Button
          variant="contained"
          startIcon={saving ? <CircularProgress size={16} color="inherit" /> : <SaveIcon />}
          onClick={handleSave}
          disabled={!hasChanges || saving}
          sx={{ textTransform: 'none' }}
        >
          Save Changes
        </Button>
        <Button
          variant="outlined"
          startIcon={<RestoreIcon />}
          onClick={handleReset}
          disabled={saving}
          sx={{ textTransform: 'none' }}
        >
          Reset to Defaults
        </Button>
        <Box sx={{ display: 'flex', gap: 1, ml: 'auto', alignItems: 'center' }}>
          <Chip 
            label="Category View" 
            onClick={() => setViewMode('category')}
            variant={viewMode === 'category' ? 'filled' : 'outlined'}
            color={viewMode === 'category' ? 'primary' : 'default'}
            size="small"
          />
          <Chip 
            label="List View" 
            onClick={() => setViewMode('list')}
            variant={viewMode === 'list' ? 'filled' : 'outlined'}
            color={viewMode === 'list' ? 'primary' : 'default'}
            size="small"
          />
        </Box>
        {hasChanges && (
          <Chip label="Unsaved Changes" color="warning" size="small" />
        )}
      </Box>

      {/* Category View */}
      {viewMode === 'category' && (
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          {/* Category Management Section */}
          <Card sx={{ borderRadius: 2, mb: 1 }}>
            <CardContent sx={{ py: 2, '&:last-child': { pb: 2 } }}>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                <Typography variant="subtitle1" sx={{ fontWeight: 600, display: 'flex', alignItems: 'center', gap: 1 }}>
                  <CategoryIcon color="primary" />
                  Manage Categories
                </Typography>
              </Box>
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 2 }}>
                {categories.map((cat, idx) => (
                  <Chip
                    key={cat.id}
                    label={cat.label}
                    onDelete={cat.id !== 'main' ? () => handleDeleteCategory(cat.id) : undefined}
                    onClick={() => handleEditCategory(cat)}
                    icon={<EditIcon fontSize="small" />}
                    variant="outlined"
                    color="primary"
                    sx={{ cursor: 'pointer' }}
                  />
                ))}
              </Box>
              <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
                <TextField
                  size="small"
                  placeholder="New category name..."
                  value={newCategoryName}
                  onChange={(e) => setNewCategoryName(e.target.value)}
                  onKeyDown={(e) => e.key === 'Enter' && handleAddCategory()}
                  sx={{ flex: 1, maxWidth: 250 }}
                />
                <Button
                  size="small"
                  variant="contained"
                  startIcon={<AddIcon />}
                  onClick={handleAddCategory}
                  disabled={!newCategoryName.trim()}
                >
                  Add Category
                </Button>
              </Box>
            </CardContent>
          </Card>

          {/* Category Accordions */}
          {categories.sort((a, b) => a.order - b.order).map((category, catIdx) => {
            const categoryItems = getItemsByCategory(category.id);
            
            return (
              <Accordion
                key={category.id}
                expanded={expandedCategories.includes(category.id)}
                onChange={() => toggleCategoryExpanded(category.id)}
                sx={{ borderRadius: 2, '&:before': { display: 'none' } }}
              >
                <AccordionSummary
                  expandIcon={<ExpandMoreIcon />}
                  sx={{ 
                    bgcolor: 'action.hover',
                    borderRadius: expandedCategories.includes(category.id) ? '8px 8px 0 0' : 2,
                  }}
                >
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flex: 1 }}>
                    <CategoryIcon color="primary" />
                    <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                      {category.label}
                    </Typography>
                    <Chip 
                      label={`${categoryItems.length} items`} 
                      size="small" 
                      sx={{ height: 20, fontSize: '0.7rem' }}
                    />
                    <Chip 
                      label={`${categoryItems.filter(i => i.visible).length} visible`} 
                      size="small" 
                      color="success"
                      variant="outlined"
                      sx={{ height: 20, fontSize: '0.7rem' }}
                    />
                    <Box sx={{ ml: 'auto', display: 'flex', gap: 0.5 }} onClick={(e) => e.stopPropagation()}>
                      <Tooltip title="Move Up">
                        <span>
                          <IconButton size="small" onClick={() => moveCategoryUp(catIdx)} disabled={catIdx === 0}>
                            <ArrowUpIcon fontSize="small" />
                          </IconButton>
                        </span>
                      </Tooltip>
                      <Tooltip title="Move Down">
                        <span>
                          <IconButton size="small" onClick={() => moveCategoryDown(catIdx)} disabled={catIdx === categories.length - 1}>
                            <ArrowDownIcon fontSize="small" />
                          </IconButton>
                        </span>
                      </Tooltip>
                    </Box>
                  </Box>
                </AccordionSummary>
                <AccordionDetails sx={{ p: 0 }}>
                  {categoryItems.length === 0 ? (
                    <Box sx={{ p: 3, textAlign: 'center' }}>
                      <Typography color="textSecondary">No items in this category. Drag items here or edit an item's category.</Typography>
                    </Box>
                  ) : (
                    <Paper variant="outlined" sx={{ borderRadius: 0, borderTop: 'none' }}>
                      <List disablePadding>
                        {categoryItems.map((item, index) => renderNavItem(item, index, categoryItems))}
                      </List>
                    </Paper>
                  )}
                </AccordionDetails>
              </Accordion>
            );
          })}
        </Box>
      )}

      {/* List View (Original) */}
      {viewMode === 'list' && (
        <>
          {/* Main Navigation Items */}
          <Card sx={{ borderRadius: 3, mb: 3 }}>
            <CardContent>
              <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 2 }}>
                Main Navigation
              </Typography>
              <Paper variant="outlined" sx={{ borderRadius: 2 }}>
                <List disablePadding>
                  {regularItems.map((item, index) => renderNavItem(item, index, regularItems))}
                </List>
              </Paper>
            </CardContent>
          </Card>

          {/* Admin Navigation Items */}
          <Card sx={{ borderRadius: 3 }}>
            <CardContent>
              <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 2 }}>
                Administration Menu
              </Typography>
              <Paper variant="outlined" sx={{ borderRadius: 2 }}>
                <List disablePadding>
                  {adminItems.map((item, index) => renderNavItem(item, index, adminItems))}
                </List>
              </Paper>
            </CardContent>
          </Card>
        </>
      )}

      {/* Edit Dialog */}
      <Dialog open={editDialogOpen} onClose={handleEditCancel} maxWidth="sm" fullWidth>
        <DialogTitle>Edit Menu Item</DialogTitle>
        <DialogContent>
          {editingItem && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3, mt: 1 }}>
              <TextField
                label="Display Label"
                value={editingItem.customLabel || editingItem.label}
                onChange={(e) => setEditingItem({ 
                  ...editingItem, 
                  customLabel: e.target.value || undefined 
                })}
                fullWidth
                helperText={`Original: ${editingItem.label}`}
              />
              <FormControl fullWidth>
                <InputLabel>Category</InputLabel>
                <Select
                  value={editingItem.category}
                  label="Category"
                  onChange={(e) => setEditingItem({ 
                    ...editingItem, 
                    category: e.target.value 
                  })}
                >
                  {categories.map((cat) => (
                    <MenuItem key={cat.id} value={cat.id}>
                      {cat.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <Typography>Visible:</Typography>
                <Switch
                  checked={editingItem.visible}
                  onChange={(e) => setEditingItem({ 
                    ...editingItem, 
                    visible: e.target.checked 
                  })}
                />
              </Box>
              <Alert severity="info" sx={{ borderRadius: 2 }}>
                <Typography variant="caption">
                  Route: <strong>{editingItem.menuName}</strong> (cannot be changed)
                </Typography>
              </Alert>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleEditCancel}>Cancel</Button>
          <Button onClick={handleEditSave} variant="contained">Save</Button>
        </DialogActions>
      </Dialog>

      {/* Category Edit Dialog */}
      <Dialog open={categoryDialogOpen} onClose={() => setCategoryDialogOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Edit Category</DialogTitle>
        <DialogContent>
          {editingCategory && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
              <TextField
                label="Category Name"
                value={editingCategory.label}
                onChange={(e) => setEditingCategory({ ...editingCategory, label: e.target.value })}
                fullWidth
                autoFocus
              />
              <Alert severity="info" sx={{ borderRadius: 2 }}>
                <Typography variant="caption">
                  Category ID: <strong>{editingCategory.id}</strong>
                </Typography>
              </Alert>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCategoryDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleSaveCategoryEdit} variant="contained">Save</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default NavigationSettingsTab;
