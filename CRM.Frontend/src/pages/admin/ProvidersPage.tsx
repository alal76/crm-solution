import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Tab,
  Tabs,
  Paper,
  Grid,
  Card,
  CardContent,
  CardActions,
  Typography,
  Button,
  TextField,
  Alert,
  AlertTitle,
  Chip,
  Divider,
  CircularProgress,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Snackbar,
  Tooltip,
  IconButton,
  Link,
} from '@mui/material';
import {
  Extension as ProvidersIcon,
  ExpandMore as ExpandMoreIcon,
  Refresh as RefreshIcon,
  OpenInNew as ExternalLinkIcon,
  CheckCircle as HealthyIcon,
  Cancel as UnhealthyIcon,
  HelpOutline as UnknownIcon,
} from '@mui/icons-material';
import AdminPageHeader from '../../components/admin/AdminPageHeader';
import ProviderSelector from '../../components/admin/ProviderSelector';
import type { ProviderOption } from '../../components/admin/ProviderSelector';
import {
  providerService,
  PROVIDER_CATEGORIES,
  type ProviderRegistryEntry,
  type ProviderFeatureFlagsDto,
  type ProviderHealthStatusResult,
} from '../../services/providerService';

// ─── Tab panel helper ─────────────────────────────────────────────────────────

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel({ children, value, index }: TabPanelProps) {
  return (
    <div role="tabpanel" hidden={value !== index}>
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  );
}

// ─── Config form ──────────────────────────────────────────────────────────────

interface ConfigFormProps {
  provider: ProviderRegistryEntry;
  onSave: (config: Record<string, string>) => Promise<void>;
  onTest: () => Promise<void>;
  saving: boolean;
  testing: boolean;
  testResult?: ProviderHealthStatusResult;
}

function ConfigForm({ provider, onSave, onTest, saving, testing, testResult }: ConfigFormProps) {
  const [values, setValues] = useState<Record<string, string>>(() => {
    const init: Record<string, string> = {};
    provider.configFields.forEach((f) => {
      init[f.key] = f.defaultValue ?? '';
    });
    return init;
  });

  const handleChange = (key: string, value: string) => {
    setValues((prev) => ({ ...prev, [key]: value }));
  };

  const handleSave = async () => {
    const payload: Record<string, string> = {};
    Object.entries(values).forEach(([k, v]) => {
      if (v !== '') payload[k] = v;
    });
    await onSave(payload);
  };

  if (provider.configFields.length === 0) {
    return (
      <Alert severity="info" sx={{ mt: 1 }}>
        This provider requires no additional configuration.
      </Alert>
    );
  }

  return (
    <Box>
      <Grid container spacing={2}>
        {provider.configFields.map((field) => (
          <Grid key={field.key} item xs={12} sm={6}>
            <TextField
              label={field.label}
              type={field.type === 'password' ? 'password' : field.type === 'number' ? 'number' : 'text'}
              value={values[field.key] ?? ''}
              onChange={(e) => handleChange(field.key, e.target.value)}
              placeholder={field.placeholder}
              helperText={field.helpText}
              required={field.required}
              fullWidth
              size="small"
              InputLabelProps={field.type === 'url' ? { shrink: true } : undefined}
            />
          </Grid>
        ))}
      </Grid>

      {testResult && (
        <Alert
          severity={testResult.isHealthy ? 'success' : 'error'}
          sx={{ mt: 2 }}
        >
          <AlertTitle>{testResult.isHealthy ? 'Connected' : 'Connection Failed'}</AlertTitle>
          {testResult.message}
          {testResult.latencyMs !== undefined && (
            <Typography variant="caption" display="block">
              Latency: {testResult.latencyMs}ms
            </Typography>
          )}
          {testResult.errorDetails && (
            <Typography variant="caption" display="block" color="error.main">
              {testResult.errorDetails}
            </Typography>
          )}
        </Alert>
      )}

      <Box sx={{ display: 'flex', gap: 1, mt: 2 }}>
        <Button
          variant="outlined"
          onClick={onTest}
          disabled={testing || saving}
          startIcon={testing ? <CircularProgress size={14} /> : <RefreshIcon />}
        >
          {testing ? 'Testing…' : 'Test Connection'}
        </Button>
        <Button
          variant="contained"
          onClick={handleSave}
          disabled={saving || testing}
          startIcon={saving ? <CircularProgress size={14} /> : undefined}
        >
          {saving ? 'Saving…' : 'Save Configuration'}
        </Button>
      </Box>
    </Box>
  );
}

// ─── CategoryTab ──────────────────────────────────────────────────────────────

interface CategoryTabProps {
  category: string;
  providers: ProviderRegistryEntry[];
  featureFlags: ProviderFeatureFlagsDto | null;
  onSetActive: (category: string, providerType: string, config: Record<string, string>) => Promise<void>;
  onTest: (category: string, providerType: string) => Promise<ProviderHealthStatusResult>;
}

function CategoryTab({ category, providers, onSetActive, onTest }: CategoryTabProps) {
  const [switching, setSwitching] = useState(false);
  const [savingFor, setSavingFor] = useState<string | null>(null);
  const [testingFor, setTestingFor] = useState<string | null>(null);
  const [testResults, setTestResults] = useState<Record<string, ProviderHealthStatusResult>>({});

  const activeProvider = providers.find((p) => p.isActive);

  const providerOptions: ProviderOption[] = providers.map((p) => ({
    value: p.providerType,
    label: p.displayName,
    description: p.description,
    isConfigured: p.isConfigured,
    isBuiltIn: p.isBuiltIn,
    isSaaS: p.isSaaS,
    isHealthy: p.healthStatus?.isHealthy,
  }));

  const handleProviderChange = async (providerType: string) => {
    setSwitching(true);
    try {
      await onSetActive(category, providerType, {});
    } finally {
      setSwitching(false);
    }
  };

  const handleSave = async (providerType: string, config: Record<string, string>) => {
    setSavingFor(providerType);
    try {
      await onSetActive(category, providerType, config);
    } finally {
      setSavingFor(null);
    }
  };

  const handleTest = async (providerType: string) => {
    setTestingFor(providerType);
    try {
      const result = await onTest(category, providerType);
      setTestResults((prev) => ({ ...prev, [providerType]: result }));
    } catch {
      setTestResults((prev) => ({
        ...prev,
        [providerType]: { isHealthy: false, message: 'Test failed', checkedAt: new Date().toISOString() },
      }));
    } finally {
      setTestingFor(null);
    }
  };

  return (
    <Box>
      {/* Active provider summary */}
      {activeProvider && (
        <Alert
          severity="info"
          icon={
            activeProvider.healthStatus?.isHealthy === true ? (
              <HealthyIcon />
            ) : activeProvider.healthStatus?.isHealthy === false ? (
              <UnhealthyIcon />
            ) : (
              <UnknownIcon />
            )
          }
          sx={{ mb: 2 }}
          action={
            activeProvider.documentationUrl ? (
              <Tooltip title="Documentation">
                <IconButton
                  size="small"
                  component={Link}
                  href={activeProvider.documentationUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  <ExternalLinkIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            ) : undefined
          }
        >
          <AlertTitle>
            Active: {activeProvider.displayName}
            {activeProvider.isBuiltIn && (
              <Chip label="Built-in" size="small" sx={{ ml: 1, height: 18, fontSize: 10 }} />
            )}
            {activeProvider.isSaaS && (
              <Chip label="SaaS" size="small" color="info" sx={{ ml: 1, height: 18, fontSize: 10 }} />
            )}
          </AlertTitle>
          {activeProvider.description}
        </Alert>
      )}

      <Grid container spacing={3}>
        {/* Left: Provider selector */}
        <Grid item xs={12} md={4}>
          <Card variant="outlined">
            <CardContent>
              <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                AVAILABLE PROVIDERS
              </Typography>
              <ProviderSelector
                category={category}
                currentProvider={activeProvider?.providerType ?? ''}
                providers={providerOptions}
                onProviderChange={handleProviderChange}
                loading={switching}
              />
            </CardContent>
          </Card>
        </Grid>

        {/* Right: Configuration forms */}
        <Grid item xs={12} md={8}>
          {providers.map((provider) => (
            <Accordion
              key={provider.providerType}
              defaultExpanded={provider.isActive}
              disableGutters
              sx={{ mb: 1 }}
            >
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, width: '100%' }}>
                  <Typography variant="body2" fontWeight={600}>
                    {provider.displayName}
                  </Typography>
                  {provider.isActive && (
                    <Chip label="Active" size="small" color="primary" sx={{ height: 18, fontSize: 10 }} />
                  )}
                  {provider.isConfigured && !provider.isBuiltIn && (
                    <Chip label="Configured" size="small" color="success" variant="outlined" sx={{ height: 18, fontSize: 10 }} />
                  )}
                  <Box sx={{ ml: 'auto', display: 'flex', alignItems: 'center', gap: 0.5 }}>
                    {provider.healthStatus?.isHealthy === true && (
                      <Tooltip title={`Healthy — ${provider.healthStatus.message}`}>
                        <HealthyIcon sx={{ fontSize: 16, color: 'success.main' }} />
                      </Tooltip>
                    )}
                    {provider.healthStatus?.isHealthy === false && (
                      <Tooltip title={`Unhealthy — ${provider.healthStatus.message}`}>
                        <UnhealthyIcon sx={{ fontSize: 16, color: 'error.main' }} />
                      </Tooltip>
                    )}
                    {provider.documentationUrl && (
                      <Tooltip title="Documentation">
                        <IconButton
                          size="small"
                          component={Link}
                          href={provider.documentationUrl}
                          target="_blank"
                          rel="noopener noreferrer"
                          onClick={(e: React.MouseEvent) => e.stopPropagation()}
                        >
                          <ExternalLinkIcon sx={{ fontSize: 14 }} />
                        </IconButton>
                      </Tooltip>
                    )}
                  </Box>
                </Box>
              </AccordionSummary>
              <AccordionDetails>
                <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1.5 }}>
                  {provider.description}
                </Typography>
                <ConfigForm
                  provider={provider}
                  onSave={(config) => handleSave(provider.providerType, config)}
                  onTest={() => handleTest(provider.providerType)}
                  saving={savingFor === provider.providerType}
                  testing={testingFor === provider.providerType}
                  testResult={testResults[provider.providerType]}
                />
              </AccordionDetails>
            </Accordion>
          ))}
        </Grid>
      </Grid>
    </Box>
  );
}

// ─── Main page ────────────────────────────────────────────────────────────────

/**
 * ProvidersPage — Admin page for managing all pluggable providers.
 *
 * Route: /admin/providers
 * Tabs: Search | Chat | Notifications | Analytics | Signatures | AI | Integrations
 * Each tab: provider list with selector + configuration form + test & save actions.
 */
const ProvidersPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState(0);
  const [loading, setLoading] = useState(true);
  const [allProviders, setAllProviders] = useState<ProviderRegistryEntry[]>([]);
  const [featureFlags, setFeatureFlags] = useState<ProviderFeatureFlagsDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({
    open: false,
    message: '',
    severity: 'success',
  });

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [providers, flags] = await Promise.all([
        providerService.getAllProviders(),
        providerService.getFeatureFlags(),
      ]);
      setAllProviders(providers);
      setFeatureFlags(flags);
    } catch (err) {
      setError('Failed to load provider information. Please try again.');
      console.error('Failed to load providers:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const getProvidersForCategory = (category: string): ProviderRegistryEntry[] =>
    allProviders.filter((p) => p.category.toLowerCase() === category.toLowerCase());

  const handleSetActive = async (
    category: string,
    providerType: string,
    config: Record<string, string>
  ) => {
    try {
      await providerService.setActiveProvider(category, { providerType, config });
      await fetchData(); // Refresh
      setSnackbar({ open: true, message: `${providerType} activated for ${category}`, severity: 'success' });
    } catch (err) {
      setSnackbar({ open: true, message: 'Failed to activate provider', severity: 'error' });
      throw err;
    }
  };

  const handleTest = async (category: string, providerType: string): Promise<ProviderHealthStatusResult> => {
    return providerService.testProviderConnection(category, providerType);
  };

  const handleTabChange = (_: React.SyntheticEvent, newValue: number) => {
    setActiveTab(newValue);
  };

  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="Provider Management"
        subtitle="Configure and switch pluggable service providers for Search, AI, Chat, and more"
        icon={ProvidersIcon}
      />

      {error && (
        <Alert
          severity="error"
          action={
            <Button size="small" onClick={fetchData}>
              Retry
            </Button>
          }
          sx={{ mb: 2 }}
        >
          {error}
        </Alert>
      )}

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
          <CircularProgress />
        </Box>
      ) : (
        <Paper>
          <Box sx={{ borderBottom: 1, borderColor: 'divider', display: 'flex', alignItems: 'center' }}>
            <Tabs
              value={activeTab}
              onChange={handleTabChange}
              variant="scrollable"
              scrollButtons="auto"
              sx={{ flex: 1 }}
            >
              {PROVIDER_CATEGORIES.map((category) => {
                const categoryProviders = getProvidersForCategory(category);
                const activeProvider = categoryProviders.find((p) => p.isActive);
                const hasIssue = activeProvider?.healthStatus?.isHealthy === false;
                return (
                  <Tab
                    key={category}
                    label={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                        {category}
                        {hasIssue && (
                          <UnhealthyIcon sx={{ fontSize: 14, color: 'error.main' }} />
                        )}
                      </Box>
                    }
                  />
                );
              })}
            </Tabs>
            <Tooltip title="Refresh">
              <IconButton onClick={fetchData} sx={{ mr: 1 }}>
                <RefreshIcon />
              </IconButton>
            </Tooltip>
          </Box>

          {PROVIDER_CATEGORIES.map((category, index) => (
            <TabPanel key={category} value={activeTab} index={index}>
              <Box sx={{ p: 3 }}>
                <CategoryTab
                  category={category}
                  providers={getProvidersForCategory(category)}
                  featureFlags={featureFlags}
                  onSetActive={handleSetActive}
                  onTest={handleTest}
                />
              </Box>
            </TabPanel>
          ))}
        </Paper>
      )}

      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar((s) => ({ ...s, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert
          severity={snackbar.severity}
          onClose={() => setSnackbar((s) => ({ ...s, open: false }))}
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default ProvidersPage;
