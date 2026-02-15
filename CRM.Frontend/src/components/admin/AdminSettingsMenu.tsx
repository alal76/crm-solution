import React, { useState, useEffect } from 'react';
import {
  Box,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Collapse,
  Divider,
  Card,
  CardContent,
  Typography,
  Badge,
} from '@mui/material';
import {
  Settings as SettingsIcon,
  ExpandLess,
  ExpandMore,
  Person as UserIcon,
  AdminPanelSettings as SystemIcon,
  Flag as FlagIcon,
  Navigation as NavigationIcon,
  Description as AuditIcon,
  GroupWork as GroupIcon,
  Business as CompanyIcon,
  SwapHoriz as ProcessIcon,
} from '@mui/icons-material';
import { useNavigate, useLocation } from 'react-router-dom';

interface SettingsMenuItem {
  id: string;
  label: string;
  icon: React.ElementType;
  path: string;
  badge?: string;
  submenu?: SettingsMenuItem[];
}

/**
 * Admin Settings Menu Component with hierarchical Settings submenu
 * CRITICAL FIX: Properly nested Settings menu with persistence
 */
export const AdminSettingsMenu: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [expandedSections, setExpandedSections] = useState<Record<string, boolean>>({
    'settings': true,  // Settings submenu starts expanded by default
    'general': false,
    'security': false,
  });

  // Load expanded state from localStorage on mount
  useEffect(() => {
    try {
      const saved = localStorage.getItem('crm_admin_menu_expanded');
      if (saved) {
        setExpandedSections(JSON.parse(saved));
      }
    } catch (e) {
      // Ignore parse errors
    }
  }, []);

  // Save expanded state to localStorage whenever it changes
  useEffect(() => {
    localStorage.setItem('crm_admin_menu_expanded', JSON.stringify(expandedSections));
  }, [expandedSections]);

  const toggleSection = (sectionId: string) => {
    setExpandedSections(prev => ({
      ...prev,
      [sectionId]: !prev[sectionId]
    }));
  };

  const settingsMenu: SettingsMenuItem[] = [
    {
      id: 'user-settings',
      label: 'User Settings',
      icon: UserIcon,
      path: '/settings',
    },
    {
      id: 'system-settings',
      label: 'System Settings',
      icon: SystemIcon,
      path: '/admin/settings',
    },
    {
      id: 'feature-flags',
      label: 'Feature Flags',
      icon: FlagIcon,
      path: '/admin/features',
    },
    {
      id: 'navigation',
      label: 'Navigation Settings',
      icon: NavigationIcon,
      path: '/admin/navigation',
    },
    {
      id: 'audit',
      label: 'Audit Logging',
      icon: AuditIcon,
      path: '/admin/audit',
      badge: 'Optional',
    },
  ];

  const generalAdminMenu: SettingsMenuItem[] = [
    {
      id: 'users',
      label: 'Users',
      icon: UserIcon,
      path: '/admin/users',
    },
    {
      id: 'groups',
      label: 'Groups',
      icon: GroupIcon,
      path: '/admin/groups',
    },
  ];

  const configMenu: SettingsMenuItem[] = [
    {
      id: 'company',
      label: 'Company Settings',
      icon: CompanyIcon,
      path: '/admin/company-settings',
    },
    {
      id: 'sales',
      label: 'Sales Configuration',
      icon: ProcessIcon,
      path: '/admin/settings/sales',
    },
    {
      id: 'service-desk',
      label: 'Service Desk Configuration',
      icon: ProcessIcon,
      path: '/admin/settings/service-desk',
    },
  ];

  const renderMenuItem = (item: SettingsMenuItem, isNested: boolean = false) => {
    const isActive = location.pathname === item.path;
    const isExpanded = expandedSections[item.id];
    const hasSubmenu = item.submenu && item.submenu.length > 0;
    const Icon = item.icon;

    return (
      <React.Fragment key={item.id}>
        <ListItemButton
          onClick={() => hasSubmenu ? toggleSection(item.id) : navigate(item.path)}
          selected={isActive}
          sx={{
            pl: isNested ? 6 : 4,
            py: 1,
            borderLeft: isActive ? '3px solid' : '3px solid transparent',
            borderLeftColor: isActive ? 'primary.main' : 'transparent',
            bgcolor: isActive ? 'primary.light' : 'transparent',
            '&:hover': {
              bgcolor: isActive ? 'primary.light' : 'grey.100',
            },
            '&.Mui-selected': {
              bgcolor: 'primary.light',
            },
            '&.Mui-selected:hover': {
              bgcolor: 'primary.light',
            },
            transition: 'all 0.2s ease-in-out',
          }}
        >
          <ListItemIcon sx={{ minWidth: 36, color: 'inherit' }}>
            {item.badge ? (
              <Badge badgeContent={item.badge} color="error" overlap="circular">
                <Icon sx={{ fontSize: '1.2rem' }} />
              </Badge>
            ) : (
              <Icon sx={{ fontSize: '1.2rem' }} />
            )}
          </ListItemIcon>
          <ListItemText
            primary={item.label}
            primaryTypographyProps={{
              sx: {
                fontSize: '0.9rem',
                fontWeight: isActive ? 600 : 500,
                color: isActive ? 'primary.main' : 'text.primary',
              }
            }}
          />
          {hasSubmenu && (
            isExpanded ? <ExpandLess fontSize="small" /> : <ExpandMore fontSize="small" />
          )}
        </ListItemButton>

        {hasSubmenu && (
          <Collapse in={isExpanded} timeout="auto" unmountOnExit>
            <List component="div" disablePadding dense>
              {item.submenu!.map(subitem => renderMenuItem(subitem, true))}
            </List>
          </Collapse>
        )}
      </React.Fragment>
    );
  };

  return (
    <Box sx={{ display: 'flex', height: '100%' }}>
      {/* Sidebar Menu */}
      <Card
        sx={{
          width: 280,
          boxShadow: 1,
          borderRadius: 0,
          display: 'flex',
          flexDirection: 'column',
          maxHeight: '100vh',
          overflow: 'auto',
        }}
      >
        <CardContent sx={{ p: 2, flexGrow: 1, overflow: 'auto' }}>
          {/* CRITICAL: Settings Submenu Section */}
          <Typography variant="subtitle2" sx={{ mb: 1.5, ml: 1, fontWeight: 700, color: 'primary.main', textTransform: 'uppercase', letterSpacing: 0.5 }}>
            Settings
          </Typography>
          <ListItemButton
            onClick={() => toggleSection('settings')}
            sx={{
              pl: 2,
              py: 1,
              mb: 1,
              bgcolor: expandedSections['settings'] ? 'primary.light' : 'transparent',
              border: '1px solid',
              borderColor: 'divider',
              borderRadius: 1,
              fontWeight: 600,
              '&:hover': {
                bgcolor: 'primary.light',
              },
              transition: 'all 0.2s ease-in-out',
            }}
          >
            <ListItemIcon sx={{ minWidth: 36 }}>
              <SettingsIcon sx={{ fontSize: '1.2rem', color: 'primary.main' }} />
            </ListItemIcon>
            <ListItemText
              primary="Settings"
              primaryTypographyProps={{
                sx: {
                  fontWeight: 600,
                  color: 'primary.main',
                }
              }}
            />
            {expandedSections['settings'] ? <ExpandLess /> : <ExpandMore />}
          </ListItemButton>

          <Collapse in={expandedSections['settings']} timeout={300} unmountOnExit>
            <List component="div" disablePadding dense sx={{ mb: 2, bgcolor: 'grey.50', borderRadius: 1, border: '1px solid', borderColor: 'divider' }}>
              {settingsMenu.map(item => renderMenuItem(item, true))}
            </List>
          </Collapse>

          <Divider sx={{ my: 2 }} />

          {/* General Admin Section */}
          <Typography variant="subtitle2" sx={{ mb: 1.5, ml: 1, fontWeight: 700, color: 'text.secondary', textTransform: 'uppercase', letterSpacing: 0.5 }}>
            Administration
          </Typography>
          <List component="div" disablePadding dense>
            {generalAdminMenu.map(item => renderMenuItem(item, false))}
          </List>

          <Divider sx={{ my: 2 }} />

          {/* Configuration Section */}
          <Typography variant="subtitle2" sx={{ mb: 1.5, ml: 1, fontWeight: 700, color: 'text.secondary', textTransform: 'uppercase', letterSpacing: 0.5 }}>
            Configuration
          </Typography>
          <List component="div" disablePadding dense>
            {configMenu.map(item => renderMenuItem(item, false))}
          </List>
        </CardContent>
      </Card>
    </Box>
  );
};

export default AdminSettingsMenu;
