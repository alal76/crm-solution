import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Grid,
  Card,
  CardContent,
  Switch,
  FormControlLabel,
  Alert,
  CircularProgress,
  Button,
  Divider,
  Paper,
} from '@mui/material';
import {
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
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import { useProfile } from '../../contexts/ProfileContext';
import { getApiEndpoint } from '../../config/ports';

interface ModuleStatus {
  customersEnabled: boolean;
  contactsEnabled: boolean;
  leadsEnabled: boolean;
  opportunitiesEnabled: boolean;
  productsEnabled: boolean;
  servicesEnabled: boolean;
  campaignsEnabled: boolean;
  quotesEnabled: boolean;
  tasksEnabled: boolean;
  activitiesEnabled: boolean;
  notesEnabled: boolean;
  workflowsEnabled: boolean;
  reportsEnabled: boolean;
  dashboardEnabled: boolean;
}

interface ModuleInfo {
  key: keyof ModuleStatus;
  label: string;
  description: string;
  icon: React.ReactNode;
}

const modules: ModuleInfo[] = [
  { key: 'dashboardEnabled', label: 'Dashboard', description: 'Analytics and overview dashboards', icon: <DashboardIcon /> },
  { key: 'customersEnabled', label: 'Customers', description: 'Customer management and accounts', icon: <PeopleIcon /> },
  { key: 'contactsEnabled', label: 'Contacts', description: 'Contact information and relationships', icon: <PeopleIcon /> },
  { key: 'leadsEnabled', label: 'Leads', description: 'Lead capture and qualification', icon: <PeopleIcon /> },
  { key: 'opportunitiesEnabled', label: 'Opportunities', description: 'Sales opportunities and pipeline', icon: <TrendingUpIcon /> },
  { key: 'productsEnabled', label: 'Products', description: 'Product catalog and inventory', icon: <PackageIcon /> },
  { key: 'servicesEnabled', label: 'Services', description: 'Service offerings and subscriptions', icon: <ServicesIcon /> },
  { key: 'campaignsEnabled', label: 'Campaigns', description: 'Marketing campaigns and tracking', icon: <MegaphoneIcon /> },
  { key: 'quotesEnabled', label: 'Quotes', description: 'Quotations and proposals', icon: <QuoteIcon /> },
  { key: 'tasksEnabled', label: 'Tasks', description: 'Task management and assignments', icon: <TaskIcon /> },
  { key: 'activitiesEnabled', label: 'Activities', description: 'Activity logging and history', icon: <ActivityIcon /> },
  { key: 'notesEnabled', label: 'Notes', description: 'Notes and documentation', icon: <NoteIcon /> },
  { key: 'workflowsEnabled', label: 'Workflows', description: 'Automation and workflow rules', icon: <AutomationIcon /> },
  { key: 'reportsEnabled', label: 'Reports', description: 'Reports and analytics', icon: <ReportsIcon /> },
];

function ModuleSettingsTab() {
  const { refreshModuleStatus } = useProfile();
  const [moduleStatus, setModuleStatus] = useState<ModuleStatus | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  
  const getToken = () => localStorage.getItem('accessToken');

  const fetchModuleStatus = async () => {
    const token = getToken();
    if (!token) return;
    
    try {
      setLoading(true);
      const response = await fetch(getApiEndpoint('/systemsettings'), {
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });
      
      if (!response.ok) {
        throw new Error('Failed to fetch system settings');
      }
      
      const data = await response.json();
      setModuleStatus(data);
      setError(null);
    } catch (err) {
      setError('Failed to load module settings');
      console.error('Error fetching module settings:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchModuleStatus();
  }, []);

  const handleToggleModule = async (moduleKey: keyof ModuleStatus) => {
    if (!moduleStatus) return;
    
    const newValue = !moduleStatus[moduleKey];
    const token = getToken();
    
    try {
      setSaving(true);
      setError(null);
      
      const response = await fetch(getApiEndpoint(`/systemsettings/modules/${moduleKey.replace('Enabled', '')}/toggle?enabled=${newValue}`), {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });
      
      if (!response.ok) {
        throw new Error('Failed to update module setting');
      }
      
      const data = await response.json();
      setModuleStatus(data);
      setSuccess(`Module ${newValue ? 'enabled' : 'disabled'} successfully`);
      
      // Refresh the global module status
      await refreshModuleStatus();
      
      setTimeout(() => setSuccess(null), 3000);
    } catch (err) {
      setError('Failed to update module setting');
      console.error('Error toggling module:', err);
    } finally {
      setSaving(false);
    }
  };

  const handleEnableAll = async () => {
    const token = getToken();
    try {
      setSaving(true);
      setError(null);
      
      const updates: Partial<Record<keyof ModuleStatus, boolean>> = {};
      modules.forEach(m => updates[m.key] = true);
      
      const response = await fetch(getApiEndpoint('/systemsettings'), {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(updates),
      });
      
      if (!response.ok) {
        throw new Error('Failed to update settings');
      }
      
      const data = await response.json();
      setModuleStatus(data);
      setSuccess('All modules enabled successfully');
      await refreshModuleStatus();
      setTimeout(() => setSuccess(null), 3000);
    } catch (err) {
      setError('Failed to enable all modules');
    } finally {
      setSaving(false);
    }
  };

  const handleDisableAll = async () => {
    const token = getToken();
    try {
      setSaving(true);
      setError(null);
      
      const updates: Partial<Record<keyof ModuleStatus, boolean>> = {};
      // Keep dashboard enabled as it's the main page
      modules.forEach(m => updates[m.key] = m.key === 'dashboardEnabled');
      
      const response = await fetch(getApiEndpoint('/systemsettings'), {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(updates),
      });
      
      if (!response.ok) {
        throw new Error('Failed to update settings');
      }
      
      const data = await response.json();
      setModuleStatus(data);
      setSuccess('Modules disabled successfully (Dashboard kept enabled)');
      await refreshModuleStatus();
      setTimeout(() => setSuccess(null), 3000);
    } catch (err) {
      setError('Failed to disable modules');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h6" sx={{ fontWeight: 600 }}>
          Module Settings
        </Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={fetchModuleStatus}
            disabled={loading}
          >
            Refresh
          </Button>
          <Button
            variant="outlined"
            color="success"
            onClick={handleEnableAll}
            disabled={saving}
          >
            Enable All
          </Button>
          <Button
            variant="outlined"
            color="warning"
            onClick={handleDisableAll}
            disabled={saving}
          >
            Disable All
          </Button>
        </Box>
      </Box>

      <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
        Enable or disable modules across the entire CRM system. Disabled modules will not be accessible to any user.
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {success && (
        <Alert severity="success" sx={{ mb: 2 }}>
          {success}
        </Alert>
      )}

      <Grid container spacing={2}>
        {modules.map((module) => (
          <Grid item xs={12} sm={6} md={4} key={module.key}>
            <Card 
              sx={{ 
                borderRadius: 2,
                opacity: moduleStatus?.[module.key] ? 1 : 0.6,
                transition: 'opacity 0.2s',
                '&:hover': { boxShadow: 2 },
              }}
            >
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                    <Box sx={{ 
                      color: moduleStatus?.[module.key] ? 'primary.main' : 'text.disabled',
                      display: 'flex',
                      alignItems: 'center',
                    }}>
                      {module.icon}
                    </Box>
                    <Box>
                      <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                        {module.label}
                      </Typography>
                      <Typography variant="body2" color="textSecondary">
                        {module.description}
                      </Typography>
                    </Box>
                  </Box>
                  <Switch
                    checked={moduleStatus?.[module.key] ?? false}
                    onChange={() => handleToggleModule(module.key)}
                    disabled={saving}
                    color="primary"
                  />
                </Box>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Divider sx={{ my: 4 }} />

      <Paper sx={{ p: 3, bgcolor: 'warning.light', borderRadius: 2 }}>
        <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
          ⚠️ Important Notes
        </Typography>
        <Typography variant="body2">
          • Disabling a module will hide it from navigation for all users<br />
          • Users will not be able to access disabled modules even via direct URL<br />
          • Data in disabled modules is preserved and will be available when re-enabled<br />
          • System administrators can still access Settings regardless of module status
        </Typography>
      </Paper>
    </Box>
  );
}

export default ModuleSettingsTab;
