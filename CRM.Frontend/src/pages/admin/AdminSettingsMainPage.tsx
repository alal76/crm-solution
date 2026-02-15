import React, { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Tabs,
  Tab,
  Typography,
  Breadcrumbs,
  Link,
  CircularProgress,
  Alert,
} from '@mui/material';
import { useNavigate, useLocation } from 'react-router-dom';
import AdminSettingsMenu from '../../components/admin/AdminSettingsMenu';
import SystemSettingsPanel from '../../components/admin/SystemSettingsPanel';
import UserSettingsPanel from '../../components/admin/UserSettingsPanel';
import FeatureFlagsPanel from '../../components/admin/FeatureFlagsPanel';
import NavigationSettingsPanel from '../../components/admin/NavigationSettingsPanel';
import AuditLogsPanel from '../../components/admin/AuditLogsPanel';
import LoadingSpinner from '../../components/common/LoadingSpinner';

/**
 * Main Admin Settings Page with hierarchical sidebar menu
 * CRITICAL FEATURE: Settings submenu properly organized with persistence
 */
const AdminSettingsPage: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [activeTab, setActiveTab] = useState('system');
  const [loading, setLoading] = useState(false);

  // Map path to active tab
  useEffect(() => {
    const path = location.pathname;
    if (path === '/settings') {
      setActiveTab('user');
    } else if (path === '/admin/settings') {
      setActiveTab('system');
    } else if (path === '/admin/features') {
      setActiveTab('features');
    } else if (path === '/admin/navigation') {
      setActiveTab('navigation');
    } else if (path === '/admin/audit') {
      setActiveTab('audit');
    }
  }, [location.pathname]);

  const handleTabChange = (event: React.SyntheticEvent, newValue: string) => {
    setActiveTab(newValue);

    // Navigate to appropriate path
    const paths: Record<string, string> = {
      'user': '/settings',
      'system': '/admin/settings',
      'features': '/admin/features',
      'navigation': '/admin/navigation',
      'audit': '/admin/audit',
    };

    if (paths[newValue]) {
      navigate(paths[newValue]);
    }
  };

  if (loading) {
    return <LoadingSpinner message="Loading settings..." />;
  }

  return (
    <Box sx={{ display: 'flex', height: 'calc(100vh - 64px)', bgcolor: 'background.default' }}>
      {/* Left Sidebar with Hierarchical Settings Menu */}
      <Box sx={{ width: 280, boxShadow: 'inset -1px 0 0 rgba(0,0,0,0.1)' }}>
        <AdminSettingsMenu />
      </Box>

      {/* Main Content Area */}
      <Box sx={{ flex: 1, display: 'flex', flexDirection: 'column', overflow: 'auto' }}>
        {/* Header with Breadcrumbs */}
        <Box sx={{ p: 2, borderBottom: '1px solid', borderColor: 'divider', bgcolor: 'background.paper' }}>
          <Breadcrumbs sx={{ mb: 1.5 }}>
            <Link
              href="/"
              underline="hover"
              color="text.secondary"
              sx={{ cursor: 'pointer' }}
            >
              Dashboard
            </Link>
            <Link
              href="/admin"
              underline="hover"
              color="text.secondary"
              sx={{ cursor: 'pointer' }}
            >
              Administration
            </Link>
            <Typography color="text.primary" sx={{ fontWeight: 600 }}>
              Settings
            </Typography>
          </Breadcrumbs>
          <Typography variant="h4" sx={{ fontWeight: 700 }}>
            Admin Settings & Configuration
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
            Manage system settings, feature flags, navigation, and module configurations
          </Typography>
        </Box>

        {/* Content Tabs */}
        <Box sx={{ borderBottom: 'solid 1px', borderColor: 'divider' }}>
          <Tabs
            value={activeTab}
            onChange={handleTabChange}
            aria-label="Settings tabs"
            sx={{
              bgcolor: 'background.paper',
              '& .MuiTab-root': {
                textTransform: 'none',
                fontWeight: 500,
                fontSize: '0.95rem',
              },
            }}
          >
            <Tab label="User Settings" value="user" />
            <Tab label="System Settings" value="system" />
            <Tab label="Feature Flags" value="features" />
            <Tab label="Navigation" value="navigation" />
            <Tab label="Audit Logging" value="audit" />
          </Tabs>
        </Box>

        {/* Settings Content Panels */}
        <Box sx={{ flex: 1, overflow: 'auto', p: 3 }}>
          {/* User Settings Panel */}
          {activeTab === 'user' && <UserSettingsPanel />}

          {/* System Settings Panel */}
          {activeTab === 'system' && <SystemSettingsPanel />}

          {/* Feature Flags Panel */}
          {activeTab === 'features' && <FeatureFlagsPanel />}

          {/* Navigation Settings Panel */}
          {activeTab === 'navigation' && <NavigationSettingsPanel />}

          {/* Audit Logs Panel */}
          {activeTab === 'audit' && <AuditLogsPanel />}
        </Box>
      </Box>
    </Box>
  );
};

export default AdminSettingsPage;
