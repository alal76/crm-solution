import React, { useState, useEffect, useCallback } from 'react';
import featureManagementService from '../../services/featureManagementService';
import {
  Box,
  Typography,
  Paper,
  Switch,
  FormControlLabel,
  Grid,
  Card,
  CardContent,
  CardHeader,
  Divider,
  Button,
  Alert,
  CircularProgress,
  Chip,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Snackbar,
  LinearProgress,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from '@mui/material';
import {
  People as CustomersIcon,
  ContactPhone as ContactsIcon,
  TrendingUp as LeadsIcon,
  MonetizationOn as OpportunitiesIcon,
  Inventory as ProductsIcon,
  Build as ServicesIcon,
  Campaign as CampaignsIcon,
  RequestQuote as QuotesIcon,
  Task as TasksIcon,
  Event as ActivitiesIcon,
  Note as NotesIcon,
  AccountTree as WorkflowsIcon,
  Assessment as ReportsIcon,
  Dashboard as DashboardIcon,
  Email as EmailIcon,
  WhatsApp as WhatsAppIcon,
  Share as SocialMediaIcon,
  ExpandMore as ExpandMoreIcon,
  Save as SaveIcon,
  Refresh as RefreshIcon,
  Sync as SyncIcon,
  Storage as StorageIcon,
  Warning as WarningIcon,
  CheckCircle as CheckIcon,
  CloudOff as DemoIcon,
  DataObject as DatabaseIcon,
} from '@mui/icons-material';

interface FeatureStatus {
  enabled: boolean;
  name: string;
  description: string;
}

interface FeatureConfiguration {
  coreModules: {
    customers: FeatureStatus;
    contacts: FeatureStatus;
    leads: FeatureStatus;
    opportunities: FeatureStatus;
    products: FeatureStatus;
    services: FeatureStatus;
  };
  salesModules: {
    campaigns: FeatureStatus;
    quotes: FeatureStatus;
  };
  productivityModules: {
    tasks: FeatureStatus;
    activities: FeatureStatus;
    notes: FeatureStatus;
  };
  automationModules: {
    workflows: FeatureStatus;
  };
  analyticsModules: {
    reports: FeatureStatus;
    dashboard: FeatureStatus;
  };
  communicationModules: {
    email: FeatureStatus;
    whatsapp: FeatureStatus;
    socialMedia: FeatureStatus;
  };
  // DEPRECATED: Demo database feature removed - systemSettings kept for backward compatibility
  systemSettings?: {
    demoModeEnabled: boolean;
    useDemoDatabase: boolean;
  };
  databaseProviders: {
    mariadb: FeatureStatus;
    postgresql: FeatureStatus;
    sqlserver: FeatureStatus;
    sqlite: FeatureStatus;
    mysql: FeatureStatus;
  };
  activeDatabaseProvider: string;
}

interface DatabaseStatus {
  productionDatabase: {
    name: string;
    isActive: boolean;
    modules: Record<string, number>;
  };
  // DEPRECATED: demoDatabase is optional - single database policy
  demoDatabase?: {
    name: string;
    isActive: boolean;
    modules: Record<string, number>;
  };
  inSync: boolean;
  lastChecked: string;
}

const moduleIcons: Record<string, React.ReactNode> = {
  customers: <CustomersIcon />,
  contacts: <ContactsIcon />,
  leads: <LeadsIcon />,
  opportunities: <OpportunitiesIcon />,
  products: <ProductsIcon />,
  services: <ServicesIcon />,
  campaigns: <CampaignsIcon />,
  quotes: <QuotesIcon />,
  tasks: <TasksIcon />,
  activities: <ActivitiesIcon />,
  notes: <NotesIcon />,
  workflows: <WorkflowsIcon />,
  reports: <ReportsIcon />,
  dashboard: <DashboardIcon />,
  email: <EmailIcon />,
  whatsapp: <WhatsAppIcon />,
  socialMedia: <SocialMediaIcon />,
  mariadb: <DatabaseIcon />,
  postgresql: <DatabaseIcon />,
  sqlserver: <DatabaseIcon />,
  sqlite: <DatabaseIcon />,
  mysql: <DatabaseIcon />,
};

function FeatureManagementTab() {
  const [features, setFeatures] = useState<FeatureConfiguration | null>(null);
  const [databaseStatus, setDatabaseStatus] = useState<DatabaseStatus | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [syncing, setSyncing] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' | 'info' });
  const [hasChanges, setHasChanges] = useState(false);
  const [expandedPanel, setExpandedPanel] = useState<string | false>('core');

  const loadFeatures = useCallback(async () => {
    try {
      const data = await featureManagementService.getFeatures();
      setFeatures(data);
    } catch (err) {
      console.error('Error loading features:', err);
      setSnackbar({ open: true, message: 'Failed to load feature configuration', severity: 'error' });
    }
  }, []);

  const loadDatabaseStatus = useCallback(async () => {
    try {
      const data = await featureManagementService.getDatabaseStatus();
      setDatabaseStatus(data);
    } catch (err) {
      console.error('Error loading database status:', err);
    }
  }, []);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      await Promise.all([loadFeatures(), loadDatabaseStatus()]);
      setLoading(false);
    };
    load();
  }, [loadFeatures, loadDatabaseStatus]);

  const handleModuleToggle = (category: keyof FeatureConfiguration, module: string, enabled: boolean) => {
    if (!features) return;

    setFeatures((prev) => {
      if (!prev) return prev;
      const categoryData = prev[category] as Record<string, FeatureStatus>;
      return {
        ...prev,
        [category]: {
          ...categoryData,
          [module]: {
            ...categoryData[module],
            enabled,
          },
        },
      };
    });
    setHasChanges(true);
  };

  // DEPRECATED: Demo database feature removed - kept for interface compatibility
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const handleDemoModeToggle = (enabled: boolean) => {
    if (!features) return;

    setFeatures((prev) => {
      if (!prev) return prev;
      return {
        ...prev,
        systemSettings: {
          demoModeEnabled: prev.systemSettings?.demoModeEnabled ?? false,
          useDemoDatabase: enabled,
        },
      };
    });
    setHasChanges(true);
  };

  const handleDatabaseProviderChange = (provider: string) => {
    if (!features) return;

    const providers = ['mariadb', 'postgresql', 'sqlserver', 'sqlite', 'mysql'];
    const updatedProviders = providers.reduce((acc, p) => ({
      ...acc,
      [p]: {
        ...features.databaseProviders[p as keyof typeof features.databaseProviders],
        enabled: p === provider,
      },
    }), {} as typeof features.databaseProviders);

    setFeatures((prev) => {
      if (!prev) return prev;
      return {
        ...prev,
        databaseProviders: updatedProviders,
        activeDatabaseProvider: provider,
      };
    });
    setHasChanges(true);
  };

  const handleSave = async () => {
    if (!features) return;

    setSaving(true);
    try {
      const updateRequest = {
        customersEnabled: features.coreModules.customers.enabled,
        contactsEnabled: features.coreModules.contacts.enabled,
        leadsEnabled: features.coreModules.leads.enabled,
        opportunitiesEnabled: features.coreModules.opportunities.enabled,
        productsEnabled: features.coreModules.products.enabled,
        servicesEnabled: features.coreModules.services.enabled,
        campaignsEnabled: features.salesModules.campaigns.enabled,
        quotesEnabled: features.salesModules.quotes.enabled,
        tasksEnabled: features.productivityModules.tasks.enabled,
        activitiesEnabled: features.productivityModules.activities.enabled,
        notesEnabled: features.productivityModules.notes.enabled,
        workflowsEnabled: features.automationModules.workflows.enabled,
        reportsEnabled: features.analyticsModules.reports.enabled,
        dashboardEnabled: features.analyticsModules.dashboard.enabled,
        emailEnabled: features.communicationModules.email.enabled,
        whatsAppEnabled: features.communicationModules.whatsapp.enabled,
        socialMediaEnabled: features.communicationModules.socialMedia.enabled,
        // DEPRECATED: Demo database feature removed — single database policy
        // useDemoDatabase: features.systemSettings?.useDemoDatabase ?? false,
        activeDatabaseProvider: features.activeDatabaseProvider,
      };
      await featureManagementService.updateFeatures(updateRequest);
      setSnackbar({ open: true, message: 'Feature settings saved successfully', severity: 'success' });
      setHasChanges(false);
    } catch (err) {
      console.error('Error saving features:', err);
      setSnackbar({ open: true, message: 'Failed to save feature settings', severity: 'error' });
    } finally {
      setSaving(false);
    }
  };

  const handleSyncDatabases = async () => {
    setSyncing(true);
    try {
      const result = await featureManagementService.syncDatabases();
      setSnackbar({
        open: true,
        message: `Database sync completed. ${result.fieldsSynced} fields synced.`,
        severity: 'success',
      });
      await loadDatabaseStatus();
    } catch (err) {
      console.error('Error syncing databases:', err);
      setSnackbar({ open: true, message: 'Failed to sync databases', severity: 'error' });
    } finally {
      setSyncing(false);
    }
  };

  const renderModuleCard = (
    category: keyof FeatureConfiguration,
    moduleKey: string,
    module: FeatureStatus
  ) => (
    <Grid item xs={12} sm={6} md={4} key={moduleKey}>
      <Card
        variant="outlined"
        sx={{
          opacity: module.enabled ? 1 : 0.6,
          transition: 'all 0.3s',
          '&:hover': { boxShadow: 2 },
        }}
      >
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Box sx={{ color: module.enabled ? 'primary.main' : 'action.disabled' }}>
                {moduleIcons[moduleKey]}
              </Box>
              <Typography variant="subtitle1" fontWeight={600}>
                {module.name}
              </Typography>
            </Box>
            <Switch
              checked={module.enabled}
              onChange={(e) => handleModuleToggle(category, moduleKey, e.target.checked)}
              size="small"
            />
          </Box>
          <Typography variant="body2" color="text.secondary">
            {module.description}
          </Typography>
        </CardContent>
      </Card>
    </Grid>
  );

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 300 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!features) {
    return (
      <Alert severity="error">Failed to load feature configuration. Please try again.</Alert>
    );
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h5" fontWeight={600}>
            Feature & Module Management
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Enable or disable features and modules across the CRM system
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={() => {
              loadFeatures();
              loadDatabaseStatus();
            }}
          >
            Refresh
          </Button>
          <Button
            variant="contained"
            startIcon={<SaveIcon />}
            onClick={handleSave}
            disabled={saving || !hasChanges}
          >
            {saving ? 'Saving...' : 'Save Changes'}
          </Button>
        </Box>
      </Box>

      {hasChanges && (
        <Alert severity="info" sx={{ mb: 2 }}>
          You have unsaved changes. Click "Save Changes" to apply them.
        </Alert>
      )}

      {/* Database Status Card */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <StorageIcon color="primary" />
            <Typography variant="h6">Database Status</Typography>
            {databaseStatus && (
              <Chip
                icon={databaseStatus.inSync ? <CheckIcon /> : <WarningIcon />}
                label={databaseStatus.inSync ? 'In Sync' : 'Out of Sync'}
                color={databaseStatus.inSync ? 'success' : 'warning'}
                size="small"
              />
            )}
          </Box>
          <Button
            variant="outlined"
            startIcon={syncing ? <CircularProgress size={16} /> : <SyncIcon />}
            onClick={handleSyncDatabases}
            disabled={syncing}
          >
            {syncing ? 'Syncing...' : 'Sync Databases'}
          </Button>
        </Box>

        {databaseStatus && (
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <Card variant={databaseStatus.productionDatabase.isActive ? 'elevation' : 'outlined'}>
                <CardHeader
                  title={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <StorageIcon fontSize="small" />
                      Production Database
                      {databaseStatus.productionDatabase.isActive && (
                        <Chip label="ACTIVE" color="success" size="small" />
                      )}
                    </Box>
                  }
                  subheader={databaseStatus.productionDatabase.name}
                />
                <CardContent>
                  <TableContainer>
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>Module</TableCell>
                          <TableCell align="right">Field Configs</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {Object.entries(databaseStatus.productionDatabase.modules).map(([module, count]) => (
                          <TableRow key={module}>
                            <TableCell>{module}</TableCell>
                            <TableCell align="right">{count}</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                </CardContent>
              </Card>
            </Grid>
            {/* DEPRECATED: Demo Database card removed — single database policy (March 2026) */}
          </Grid>
        )}
      </Paper>

      {/* Demo Mode Toggle - DEPRECATED: Single database policy in effect */}
      {/* This section is hidden as demo database feature has been removed.
          The system now operates with a single production database only.
          See copilot-instructions.md for single database policy. */}

      {/* Database Provider Selection - Currently Disabled */}
      <Paper sx={{ p: 3, mb: 3, opacity: 0.7 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
          <DatabaseIcon color="disabled" />
          <Box>
            <Typography variant="h6" color="text.secondary">Database Provider</Typography>
            <Typography variant="body2" color="text.secondary">
              Database switching is currently disabled. Contact your administrator for changes.
            </Typography>
          </Box>
        </Box>
        <Grid container spacing={2}>
          {features.databaseProviders && Object.entries(features.databaseProviders).map(([key, provider]) => (
            <Grid item xs={12} sm={6} md={4} key={key}>
              <Card
                variant={features.activeDatabaseProvider === key ? 'elevation' : 'outlined'}
                sx={{
                  opacity: features.activeDatabaseProvider === key ? 1 : 0.4,
                  border: features.activeDatabaseProvider === key ? '2px solid' : '1px solid',
                  borderColor: features.activeDatabaseProvider === key ? 'success.main' : 'divider',
                  pointerEvents: 'none',
                }}
              >
                <CardContent>
                  <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Box sx={{ color: features.activeDatabaseProvider === key ? 'success.main' : 'action.disabled' }}>
                        {moduleIcons[key]}
                      </Box>
                      <Typography variant="subtitle1" fontWeight={600}>
                        {provider.name}
                      </Typography>
                    </Box>
                    {features.activeDatabaseProvider === key && (
                      <Chip label="ACTIVE" color="success" size="small" />
                    )}
                  </Box>
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                    {provider.description}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Paper>

      {/* Module Sections */}
      <Accordion expanded={expandedPanel === 'core'} onChange={() => setExpandedPanel(expandedPanel === 'core' ? false : 'core')}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography variant="h6">Core Modules</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={2}>
            {Object.entries(features.coreModules).map(([key, module]) =>
              renderModuleCard('coreModules', key, module)
            )}
          </Grid>
        </AccordionDetails>
      </Accordion>

      <Accordion expanded={expandedPanel === 'sales'} onChange={() => setExpandedPanel(expandedPanel === 'sales' ? false : 'sales')}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography variant="h6">Sales Modules</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={2}>
            {Object.entries(features.salesModules).map(([key, module]) =>
              renderModuleCard('salesModules', key, module)
            )}
          </Grid>
        </AccordionDetails>
      </Accordion>

      <Accordion expanded={expandedPanel === 'productivity'} onChange={() => setExpandedPanel(expandedPanel === 'productivity' ? false : 'productivity')}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography variant="h6">Productivity Modules</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={2}>
            {Object.entries(features.productivityModules).map(([key, module]) =>
              renderModuleCard('productivityModules', key, module)
            )}
          </Grid>
        </AccordionDetails>
      </Accordion>

      <Accordion expanded={expandedPanel === 'automation'} onChange={() => setExpandedPanel(expandedPanel === 'automation' ? false : 'automation')}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography variant="h6">Automation Modules</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={2}>
            {Object.entries(features.automationModules).map(([key, module]) =>
              renderModuleCard('automationModules', key, module)
            )}
          </Grid>
        </AccordionDetails>
      </Accordion>

      <Accordion expanded={expandedPanel === 'analytics'} onChange={() => setExpandedPanel(expandedPanel === 'analytics' ? false : 'analytics')}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography variant="h6">Analytics Modules</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={2}>
            {Object.entries(features.analyticsModules).map(([key, module]) =>
              renderModuleCard('analyticsModules', key, module)
            )}
          </Grid>
        </AccordionDetails>
      </Accordion>

      <Accordion expanded={expandedPanel === 'communication'} onChange={() => setExpandedPanel(expandedPanel === 'communication' ? false : 'communication')}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography variant="h6">Communication Modules</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={2}>
            {Object.entries(features.communicationModules).map(([key, module]) =>
              renderModuleCard('communicationModules', key, module)
            )}
          </Grid>
        </AccordionDetails>
      </Accordion>

      <Snackbar
        open={snackbar.open}
        autoHideDuration={5000}
        onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))}
        message={snackbar.message}
      />
    </Box>
  );
}

export default FeatureManagementTab;
