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
} from '@mui/icons-material';

interface NavItem {
  id: string;
  label: string;
  menuName: string;
  icon: string;
  order: number;
  visible: boolean;
  isAdmin: boolean;
}

const DEFAULT_NAV_ITEMS: NavItem[] = [
  { id: 'dashboard', label: 'Dashboard', menuName: 'Dashboard', icon: 'DashboardIcon', order: 0, visible: true, isAdmin: false },
  { id: 'customers', label: 'Customers', menuName: 'Customers', icon: 'PeopleIcon', order: 1, visible: true, isAdmin: false },
  { id: 'customer-overview', label: 'Customer Overview', menuName: 'CustomerOverview', icon: 'PersonSearchIcon', order: 2, visible: true, isAdmin: false },
  { id: 'contacts', label: 'Contacts', menuName: 'Contacts', icon: 'PeopleIcon', order: 3, visible: true, isAdmin: false },
  { id: 'leads', label: 'Leads', menuName: 'Leads', icon: 'PeopleIcon', order: 4, visible: true, isAdmin: false },
  { id: 'opportunities', label: 'Opportunities', menuName: 'Opportunities', icon: 'TrendingUpIcon', order: 5, visible: true, isAdmin: false },
  { id: 'products', label: 'Products', menuName: 'Products', icon: 'PackageIcon', order: 6, visible: true, isAdmin: false },
  { id: 'services', label: 'Services', menuName: 'Services', icon: 'SettingsIcon', order: 7, visible: true, isAdmin: false },
  { id: 'service-requests', label: 'Service Requests', menuName: 'ServiceRequests', icon: 'SupportAgentIcon', order: 8, visible: true, isAdmin: false },
  { id: 'campaigns', label: 'Campaigns', menuName: 'Campaigns', icon: 'MegaphoneIcon', order: 9, visible: true, isAdmin: false },
  { id: 'quotes', label: 'Quotes', menuName: 'Quotes', icon: 'QuoteIcon', order: 10, visible: true, isAdmin: false },
  { id: 'tasks', label: 'Tasks', menuName: 'Tasks', icon: 'TaskIcon', order: 11, visible: true, isAdmin: false },
  { id: 'activities', label: 'Activities', menuName: 'Activities', icon: 'ActivityIcon', order: 12, visible: true, isAdmin: false },
  { id: 'notes', label: 'Notes', menuName: 'Notes', icon: 'NoteIcon', order: 13, visible: true, isAdmin: false },
  { id: 'workflows', label: 'Workflows', menuName: 'Workflows', icon: 'AutomationIcon', order: 14, visible: true, isAdmin: true },
  { id: 'settings', label: 'Admin Settings', menuName: 'Settings', icon: 'SettingsIcon', order: 15, visible: true, isAdmin: true },
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
};

function NavigationSettingsTab() {
  const [navItems, setNavItems] = useState<NavItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [success, setSuccess] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [hasChanges, setHasChanges] = useState(false);

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
            const savedOrder = JSON.parse(data.navOrderConfig);
            // Merge saved order with default items
            const mergedItems = DEFAULT_NAV_ITEMS.map(item => {
              const saved = savedOrder.find((s: NavItem) => s.id === item.id);
              return saved ? { ...item, order: saved.order, visible: saved.visible } : item;
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
      }));
      const navOrderConfig = JSON.stringify(navOrderData);

      const response = await fetch(`${getApiUrl()}/systemsettings/navigation/order`, {
        method: 'PUT',
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ navOrderConfig }),
      });

      if (response.ok) {
        // Save to localStorage for immediate use by Navigation component
        localStorage.setItem('crm_nav_order', JSON.stringify(navOrderData));
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

  return (
    <Box>
      <Typography variant="h6" sx={{ fontWeight: 600, mb: 1 }}>
        Navigation Settings
      </Typography>
      <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
        Customize the order and visibility of navigation menu items. Changes will apply to all users.
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
      <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
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
        {hasChanges && (
          <Chip label="Unsaved Changes" color="warning" size="small" sx={{ ml: 'auto' }} />
        )}
      </Box>

      {/* Main Navigation Items */}
      <Card sx={{ borderRadius: 3, mb: 3 }}>
        <CardContent>
          <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 2 }}>
            Main Navigation
          </Typography>
          <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
            Use the arrow buttons to reorder items. Toggle visibility to show/hide items in the navigation menu.
          </Typography>
          <Paper variant="outlined" sx={{ borderRadius: 2 }}>
            <List disablePadding>
              {regularItems.map((item, index) => {
                const actualIndex = navItems.findIndex(n => n.id === item.id);
                return (
                  <React.Fragment key={item.id}>
                    {index > 0 && <Divider />}
                    <ListItem
                      sx={{
                        opacity: item.visible ? 1 : 0.5,
                        bgcolor: item.visible ? 'transparent' : 'action.disabledBackground',
                      }}
                    >
                      <ListItemIcon sx={{ minWidth: 36 }}>
                        <DragIcon sx={{ color: 'text.secondary' }} />
                      </ListItemIcon>
                      <ListItemIcon sx={{ minWidth: 40 }}>
                        {iconMap[item.icon] || <SettingsIcon />}
                      </ListItemIcon>
                      <ListItemText
                        primary={item.label}
                        secondary={item.menuName}
                      />
                      <ListItemSecondaryAction>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Tooltip title="Move Up">
                            <span>
                              <IconButton
                                size="small"
                                onClick={() => moveItem(actualIndex, 'up')}
                                disabled={actualIndex === 0 || navItems[actualIndex - 1]?.isAdmin !== item.isAdmin}
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
                                disabled={actualIndex === navItems.length - 1 || navItems[actualIndex + 1]?.isAdmin !== item.isAdmin}
                              >
                                <ArrowDownIcon fontSize="small" />
                              </IconButton>
                            </span>
                          </Tooltip>
                          <Divider orientation="vertical" flexItem sx={{ mx: 1 }} />
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
              })}
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
          <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
            Admin-only navigation items. These are only visible to users with admin privileges.
          </Typography>
          <Paper variant="outlined" sx={{ borderRadius: 2 }}>
            <List disablePadding>
              {adminItems.map((item, index) => {
                const actualIndex = navItems.findIndex(n => n.id === item.id);
                return (
                  <React.Fragment key={item.id}>
                    {index > 0 && <Divider />}
                    <ListItem
                      sx={{
                        opacity: item.visible ? 1 : 0.5,
                        bgcolor: item.visible ? 'transparent' : 'action.disabledBackground',
                      }}
                    >
                      <ListItemIcon sx={{ minWidth: 36 }}>
                        <DragIcon sx={{ color: 'text.secondary' }} />
                      </ListItemIcon>
                      <ListItemIcon sx={{ minWidth: 40 }}>
                        {iconMap[item.icon] || <SettingsIcon />}
                      </ListItemIcon>
                      <ListItemText
                        primary={item.label}
                        secondary={
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            {item.menuName}
                            <Chip label="Admin Only" size="small" color="secondary" sx={{ height: 18, fontSize: '0.7rem' }} />
                          </Box>
                        }
                      />
                      <ListItemSecondaryAction>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
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
                                disabled={index === adminItems.length - 1}
                              >
                                <ArrowDownIcon fontSize="small" />
                              </IconButton>
                            </span>
                          </Tooltip>
                          <Divider orientation="vertical" flexItem sx={{ mx: 1 }} />
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
              })}
            </List>
          </Paper>
        </CardContent>
      </Card>
    </Box>
  );
}

export default NavigationSettingsTab;
