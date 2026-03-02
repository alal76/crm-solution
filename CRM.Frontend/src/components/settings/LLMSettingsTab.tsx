// CRM Solution - LLM Provider Settings Administration Tab
// Supports encrypted API key management, provider configuration, and connection testing.
import React, { useState, useEffect, useCallback } from 'react';
import {
  Box, Typography, Tabs, Tab, Card, CardContent, Grid, TextField,
  Button, Select, MenuItem, FormControl, InputLabel, Slider, Switch,
  FormControlLabel, Chip, Alert, Snackbar, CircularProgress, Divider,
  IconButton, InputAdornment, Accordion, AccordionSummary, AccordionDetails,
  Tooltip, Dialog, DialogTitle, DialogContent, DialogActions, Paper,
  LinearProgress, Stack,
} from '@mui/material';
import {
  Visibility, VisibilityOff, CheckCircle, Error as ErrorIcon, Warning,
  ExpandMore, PlayArrow, Save, Refresh, Security, VpnKey, Speed,
  Cloud, Computer, Settings as SettingsIcon, NetworkCheck,
} from '@mui/icons-material';
import llmSettingsService, {
  LLMSettingsDto, LLMProviderSettingsDto, LLMProviderUpdateDto,
  UpdateLLMSettingsRequest, CircuitBreakerState, TestConnectionResult,
  PROVIDER_DEFINITIONS, ProviderMeta, ProviderField,
} from '../../services/llmSettingsService';

// ─── Tab Panel ──────────────────────────────────────────────────────
interface TabPanelProps { children?: React.ReactNode; value: number; index: number; }
function TabPanel({ children, value, index }: TabPanelProps) {
  return <div hidden={value !== index} style={{ paddingTop: 16 }}>{value === index && children}</div>;
}

// ─── Main Component ─────────────────────────────────────────────────
const LLMSettingsTab: React.FC = () => {
  const [tabIndex, setTabIndex] = useState(0);
  const [settings, setSettings] = useState<LLMSettingsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' | 'info' | 'warning' }>({ open: false, message: '', severity: 'info' });
  const [circuitBreakers, setCircuitBreakers] = useState<CircuitBreakerState[]>([]);
  const [testingProvider, setTestingProvider] = useState<string | null>(null);
  const [expandedProvider, setExpandedProvider] = useState<string | false>(false);
  const [hasChanges, setHasChanges] = useState(false);

  // Editable states for general settings
  const [editGeneral, setEditGeneral] = useState({
    defaultProvider: '',
    enableFallback: true,
    defaultMaxTokens: 1000,
    defaultTemperature: 0.7,
    timeoutSeconds: 60,
    maxRetries: 3,
    fallbackOrder: [] as string[],
  });

  // Editable states for provider settings (api keys, urls, models, etc.)
  const [providerEdits, setProviderEdits] = useState<Record<string, Record<string, string>>>({});
  // Track which password fields are visible
  const [showPasswords, setShowPasswords] = useState<Record<string, boolean>>({});
  // Confirmation dialog for reset
  const [resetDialogOpen, setResetDialogOpen] = useState(false);

  // ─── Data Loading ──────────────────────────────────────────────────
  const loadSettings = useCallback(async () => {
    try {
      setLoading(true);
      const [settingsRes, cbRes] = await Promise.all([
        llmSettingsService.getSettings(),
        llmSettingsService.getCircuitBreakers().catch(() => ({ data: [] })),
      ]);
      setSettings(settingsRes.data);
      setCircuitBreakers(Array.isArray(cbRes.data) ? cbRes.data : []);

      const s = settingsRes.data;
      setEditGeneral({
        defaultProvider: s.defaultProvider || 'openai',
        enableFallback: s.enableFallback,
        defaultMaxTokens: s.defaultMaxTokens,
        defaultTemperature: s.defaultTemperature,
        timeoutSeconds: s.timeoutSeconds,
        maxRetries: s.maxRetries,
        fallbackOrder: s.fallbackOrder || [],
      });
      setProviderEdits({});
      setHasChanges(false);
    } catch (err: any) {
      setSnackbar({ open: true, message: `Failed to load settings: ${err?.message || 'Unknown error'}`, severity: 'error' });
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadSettings(); }, [loadSettings]);

  // ─── Provider Data Helper ──────────────────────────────────────────
  const getProviderSettings = (key: string): LLMProviderSettingsDto | null => {
    if (!settings) return null;
    return (settings as any)[key] ?? null;
  };

  // ─── Edit Helpers ──────────────────────────────────────────────────
  const setProviderField = (providerKey: string, field: string, value: string) => {
    setProviderEdits(prev => ({
      ...prev,
      [providerKey]: { ...(prev[providerKey] || {}), [field]: value },
    }));
    setHasChanges(true);
  };

  const setGeneralField = (field: string, value: any) => {
    setEditGeneral(prev => ({ ...prev, [field]: value }));
    setHasChanges(true);
  };

  // ─── Save ──────────────────────────────────────────────────────────
  const handleSave = async () => {
    try {
      setSaving(true);
      const request: UpdateLLMSettingsRequest = {
        defaultProvider: editGeneral.defaultProvider,
        enableFallback: editGeneral.enableFallback,
        defaultMaxTokens: editGeneral.defaultMaxTokens,
        defaultTemperature: editGeneral.defaultTemperature,
        timeoutSeconds: editGeneral.timeoutSeconds,
        maxRetries: editGeneral.maxRetries,
        fallbackOrder: editGeneral.fallbackOrder,
      };

      // Build provider updates from edits
      if (Object.keys(providerEdits).length > 0) {
        request.providers = {};
        for (const [provKey, fields] of Object.entries(providerEdits)) {
          const meta = PROVIDER_DEFINITIONS.find(p => p.key === provKey);
          if (!meta) continue;
          const update: LLMProviderUpdateDto = {};
          for (const [fk, fv] of Object.entries(fields)) {
            if (fk === 'apiKey') update.apiKey = fv;
            else if (fk === 'baseUrl') update.baseUrl = fv;
            else if (fk === 'defaultModel') update.defaultModel = fv;
            else if (fk === 'apiVersion') update.apiVersion = fv;
            else if (fk === 'location') update.location = fv;
            else if (fk === 'region') update.region = fv;
            else if (fk === 'apiFormat') update.apiFormat = fv;
            else if (fk === 'endpoint') update.endpoint = fv;
            else if (fk === 'deploymentName') update.deploymentName = fv;
            else if (fk === 'projectId') update.projectId = fv;
            else if (fk === 'enabled') update.enabled = fv === 'true';
            else if (fk === 'useVertexAI') update.useVertexAI = fv === 'true';
            else if (fk === 'useDefaultCredentials') update.useDefaultCredentials = fv === 'true';
          }
          request.providers[meta.apiKey] = update;
        }
      }

      const res = await llmSettingsService.updateSettings(request);
      setSettings(res.data);
      setProviderEdits({});
      setHasChanges(false);
      setSnackbar({ open: true, message: 'Settings saved successfully', severity: 'success' });
    } catch (err: any) {
      setSnackbar({ open: true, message: `Save failed: ${err?.message || 'Unknown error'}`, severity: 'error' });
    } finally {
      setSaving(false);
    }
  };

  // ─── Reset ─────────────────────────────────────────────────────────
  const handleReset = async () => {
    try {
      setResetDialogOpen(false);
      setSaving(true);
      await llmSettingsService.resetToDefaults();
      await loadSettings();
      setSnackbar({ open: true, message: 'Settings reset to defaults', severity: 'info' });
    } catch (err: any) {
      setSnackbar({ open: true, message: `Reset failed: ${err?.message || 'Unknown error'}`, severity: 'error' });
    } finally {
      setSaving(false);
    }
  };

  // ─── Test Connection ───────────────────────────────────────────────
  const handleTestConnection = async (providerApiKey: string) => {
    try {
      setTestingProvider(providerApiKey);
      const res = await llmSettingsService.testConnection(providerApiKey);
      const result: TestConnectionResult = res.data;
      setSnackbar({
        open: true,
        message: result.success ? `✓ ${result.message}` : `✗ ${result.message}`,
        severity: result.success ? 'success' : 'error',
      });
    } catch (err: any) {
      setSnackbar({ open: true, message: `Test failed: ${err?.message || 'Unknown error'}`, severity: 'error' });
    } finally {
      setTestingProvider(null);
    }
  };

  // ─── Render: General Tab ───────────────────────────────────────────
  const renderGeneralTab = () => (
    <Box>
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>Default Provider & Fallback</Typography>
          <Grid container spacing={3}>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth size="small">
                <InputLabel>Default Provider</InputLabel>
                <Select
                  value={editGeneral.defaultProvider}
                  label="Default Provider"
                  onChange={e => setGeneralField('defaultProvider', e.target.value)}
                >
                  {PROVIDER_DEFINITIONS.map(p => (
                    <MenuItem key={p.apiKey} value={p.apiKey}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <span>{p.icon}</span> {p.label}
                        {getProviderSettings(p.key)?.isConfigured && (
                          <Chip label="Ready" size="small" color="success" variant="outlined" sx={{ ml: 1 }} />
                        )}
                      </Box>
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={editGeneral.enableFallback} onChange={e => setGeneralField('enableFallback', e.target.checked)} />}
                label="Enable Provider Fallback"
              />
              <Typography variant="caption" color="text.secondary" display="block">
                Automatically try other providers if the default fails
              </Typography>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>Model Parameters</Typography>
          <Grid container spacing={3}>
            <Grid item xs={12} sm={6}>
              <Typography gutterBottom>Max Tokens: {editGeneral.defaultMaxTokens}</Typography>
              <Slider
                value={editGeneral.defaultMaxTokens}
                onChange={(_, v) => setGeneralField('defaultMaxTokens', v as number)}
                min={100} max={32000} step={100}
                valueLabelDisplay="auto"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Typography gutterBottom>Temperature: {editGeneral.defaultTemperature.toFixed(2)}</Typography>
              <Slider
                value={editGeneral.defaultTemperature}
                onChange={(_, v) => setGeneralField('defaultTemperature', v as number)}
                min={0} max={2} step={0.05}
                valueLabelDisplay="auto"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth size="small" type="number" label="Timeout (seconds)"
                value={editGeneral.timeoutSeconds}
                onChange={e => setGeneralField('timeoutSeconds', Number.parseInt(e.target.value) || 60)}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth size="small" type="number" label="Max Retries"
                value={editGeneral.maxRetries}
                onChange={e => setGeneralField('maxRetries', Number.parseInt(e.target.value) || 3)}
              />
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {settings?.effectiveFallbackOrder && settings.effectiveFallbackOrder.length > 0 && (
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>Effective Fallback Order</Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
              Only configured providers are included in the fallback chain
            </Typography>
            <Stack direction="row" spacing={1} flexWrap="wrap">
              {settings.effectiveFallbackOrder.map((p, i) => {
                const meta = PROVIDER_DEFINITIONS.find(m => m.apiKey === p);
                return (
                  <Chip
                    key={p}
                    label={`${i + 1}. ${meta?.icon || ''} ${meta?.label || p}`}
                    color={i === 0 ? 'primary' : 'default'}
                    variant={i === 0 ? 'filled' : 'outlined'}
                  />
                );
              })}
            </Stack>
          </CardContent>
        </Card>
      )}
    </Box>
  );

  // ─── Render: Provider Card ─────────────────────────────────────────
  const renderProviderCard = (meta: ProviderMeta) => {
    const ps = getProviderSettings(meta.key);
    if (!ps) return null;

    const edits = providerEdits[meta.key] || {};
    const passwordKey = `${meta.key}_apiKey`;

    return (
      <Accordion
        key={meta.key}
        expanded={expandedProvider === meta.key}
        onChange={(_, isExpanded) => setExpandedProvider(isExpanded ? meta.key : false)}
        sx={{ mb: 1 }}
      >
        <AccordionSummary expandIcon={<ExpandMore />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, width: '100%', pr: 2 }}>
            <Typography sx={{ fontSize: '1.4rem' }}>{meta.icon}</Typography>
            <Box sx={{ flexGrow: 1 }}>
              <Typography variant="subtitle1" fontWeight={600}>{meta.label}</Typography>
              <Typography variant="caption" color="text.secondary">{meta.description}</Typography>
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              {ps.hasApiKey && (
                <Tooltip title="API key set (encrypted)">
                  <Security fontSize="small" color="success" />
                </Tooltip>
              )}
              <Chip
                label={ps.isConfigured ? 'Configured' : 'Not Configured'}
                size="small"
                color={ps.isConfigured ? 'success' : 'default'}
                icon={ps.isConfigured ? <CheckCircle /> : <Warning />}
              />
            </Box>
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={2}>
            {meta.fields.map(field => renderField(meta, field, ps, edits, passwordKey))}

            {/* Masked key display */}
            {ps.apiKeyMasked && !edits['apiKey'] && (
              <Grid item xs={12}>
                <Alert severity="info" icon={<VpnKey />} sx={{ py: 0 }}>
                  Current API key: <strong>{ps.apiKeyMasked}</strong> — Enter a new key to replace it, or leave blank to keep existing
                </Alert>
              </Grid>
            )}

            {/* Model info */}
            {ps.defaultModel && (
              <Grid item xs={12} sm={6}>
                <Typography variant="body2" color="text.secondary">
                  Active model: <strong>{ps.defaultModel}</strong>
                </Typography>
              </Grid>
            )}

            {/* Test connection */}
            <Grid item xs={12}>
              <Divider sx={{ my: 1 }} />
              <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
                <Button
                  variant="outlined" size="small" startIcon={
                    testingProvider === meta.apiKey ? <CircularProgress size={16} /> : <NetworkCheck />
                  }
                  disabled={testingProvider !== null || (!ps.isConfigured && !edits['apiKey'])}
                  onClick={() => handleTestConnection(meta.apiKey)}
                >
                  {testingProvider === meta.apiKey ? 'Testing...' : 'Test Connection'}
                </Button>
                {ps.isConfigured && (
                  <Typography variant="caption" color="success.main">
                    <CheckCircle sx={{ fontSize: 14, mr: 0.5, verticalAlign: 'middle' }} />
                    Provider is ready to use
                  </Typography>
                )}
              </Box>
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>
    );
  };

  // ─── Render: Field ─────────────────────────────────────────────────
  const renderField = (
    meta: ProviderMeta,
    field: ProviderField,
    ps: LLMProviderSettingsDto,
    edits: Record<string, string>,
    passwordKey: string,
  ) => {
    const currentValue = edits[field.key] ?? '';
    const storedValue = (ps as any)[field.key] ?? '';

    if (field.type === 'switch') {
      const checked = edits[field.key] !== undefined
        ? edits[field.key] === 'true'
        : (ps as any)[field.key] ?? false;
      return (
        <Grid item xs={12} sm={6} key={field.key}>
          <FormControlLabel
            control={<Switch checked={checked} onChange={e => setProviderField(meta.key, field.key, e.target.checked ? 'true' : 'false')} />}
            label={field.label}
          />
        </Grid>
      );
    }

    if (field.type === 'select' && field.options) {
      const val = currentValue || storedValue || '';
      return (
        <Grid item xs={12} sm={6} key={field.key}>
          <FormControl fullWidth size="small">
            <InputLabel>{field.label}</InputLabel>
            <Select
              value={val}
              label={field.label}
              onChange={e => setProviderField(meta.key, field.key, e.target.value)}
            >
              {field.options.map(opt => <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>)}
            </Select>
          </FormControl>
        </Grid>
      );
    }

    if (field.type === 'password') {
      return (
        <Grid item xs={12} sm={6} key={field.key}>
          <TextField
            fullWidth size="small"
            label={field.label}
            type={showPasswords[passwordKey] ? 'text' : 'password'}
            value={currentValue}
            onChange={e => setProviderField(meta.key, field.key, e.target.value)}
            placeholder={ps.hasApiKey ? '••••••••  (key is set)' : field.placeholder || 'Enter API key'}
            helperText={field.helperText || (ps.hasApiKey ? 'Leave blank to keep existing key' : 'Required')}
            InputProps={{
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton size="small" onClick={() => setShowPasswords(prev => ({ ...prev, [passwordKey]: !prev[passwordKey] }))}>
                    {showPasswords[passwordKey] ? <VisibilityOff fontSize="small" /> : <Visibility fontSize="small" />}
                  </IconButton>
                </InputAdornment>
              ),
            }}
          />
        </Grid>
      );
    }

    // text, url, number
    return (
      <Grid item xs={12} sm={6} key={field.key}>
        <TextField
          fullWidth size="small"
          label={field.label}
          type={field.type === 'number' ? 'number' : 'text'}
          value={currentValue || storedValue || ''}
          onChange={e => setProviderField(meta.key, field.key, e.target.value)}
          placeholder={field.placeholder}
          helperText={field.helperText}
        />
      </Grid>
    );
  };

  // ─── Render: Providers Tab ─────────────────────────────────────────
  const renderProvidersTab = () => (
    <Box>
      <Alert severity="info" sx={{ mb: 2 }} icon={<Security />}>
        API keys are <strong>encrypted at rest</strong> in the database using ASP.NET Core Data Protection.
        Keys entered here are transmitted over HTTPS and stored securely.
      </Alert>

      {/* Configured providers first, then unconfigured */}
      {PROVIDER_DEFINITIONS
        .sort((a, b) => {
          const aConf = getProviderSettings(a.key)?.isConfigured ? 1 : 0;
          const bConf = getProviderSettings(b.key)?.isConfigured ? 1 : 0;
          return bConf - aConf;
        })
        .map(meta => renderProviderCard(meta))
      }
    </Box>
  );

  // ─── Render: Status Tab ────────────────────────────────────────────
  const renderStatusTab = () => (
    <Box>
      <Typography variant="h6" gutterBottom>Provider Status Overview</Typography>
      <Grid container spacing={2} sx={{ mb: 3 }}>
        {PROVIDER_DEFINITIONS.map(meta => {
          const ps = getProviderSettings(meta.key);
          return (
            <Grid item xs={12} sm={6} md={4} key={meta.key}>
              <Paper variant="outlined" sx={{ p: 2 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                  <Typography sx={{ fontSize: '1.2rem' }}>{meta.icon}</Typography>
                  <Typography variant="subtitle2" fontWeight={600}>{meta.label}</Typography>
                </Box>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  <Chip label={ps?.isConfigured ? 'Configured' : 'Not Set'} size="small"
                    color={ps?.isConfigured ? 'success' : 'default'} variant="outlined" />
                  {ps?.hasApiKey && <Chip label="Key Set" size="small" color="info" variant="outlined" icon={<VpnKey />} />}
                  {ps?.defaultModel && <Chip label={ps.defaultModel} size="small" variant="outlined" />}
                </Box>
              </Paper>
            </Grid>
          );
        })}
      </Grid>

      {circuitBreakers.length > 0 && (
        <>
          <Typography variant="h6" gutterBottom>Circuit Breakers</Typography>
          <Grid container spacing={2}>
            {circuitBreakers
              .filter(cb => cb.serviceName.toLowerCase().includes('llm'))
              .map(cb => (
                <Grid item xs={12} sm={6} key={cb.serviceName}>
                  <Paper variant="outlined" sx={{ p: 2 }}>
                    <Typography variant="subtitle2">{cb.serviceName}</Typography>
                    <Box sx={{ display: 'flex', gap: 1, mt: 0.5 }}>
                      <Chip
                        label={cb.state}
                        size="small"
                        color={cb.state === 'Closed' ? 'success' : cb.state === 'Open' ? 'error' : 'warning'}
                      />
                      <Typography variant="caption" color="text.secondary">
                        Failures: {cb.failureCount} | Successes: {cb.successCount}
                      </Typography>
                    </Box>
                    {cb.lastError && (
                      <Typography variant="caption" color="error" display="block" sx={{ mt: 0.5 }}>
                        Last error: {cb.lastError}
                      </Typography>
                    )}
                  </Paper>
                </Grid>
              ))}
            {circuitBreakers.filter(cb => cb.serviceName.toLowerCase().includes('llm')).length === 0 && (
              <Grid item xs={12}>
                <Typography variant="body2" color="text.secondary">
                  No LLM-related circuit breakers active
                </Typography>
              </Grid>
            )}
          </Grid>
        </>
      )}
    </Box>
  );

  // ─── Main Render ───────────────────────────────────────────────────
  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 300 }}>
        <CircularProgress />
        <Typography sx={{ ml: 2 }}>Loading LLM settings...</Typography>
      </Box>
    );
  }

  return (
    <Box>
      {saving && <LinearProgress sx={{ mb: 1 }} />}

      {/* Header with action buttons */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="h5" display="flex" alignItems="center" gap={1}>
          <Speed /> LLM Provider Settings
        </Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button variant="outlined" size="small" startIcon={<Refresh />} onClick={loadSettings} disabled={saving}>
            Refresh
          </Button>
          <Button variant="outlined" size="small" color="warning" onClick={() => setResetDialogOpen(true)} disabled={saving}>
            Reset to Defaults
          </Button>
          <Button
            variant="contained" size="small" startIcon={saving ? <CircularProgress size={16} /> : <Save />}
            onClick={handleSave} disabled={saving || !hasChanges}
          >
            {saving ? 'Saving...' : 'Save Changes'}
          </Button>
        </Box>
      </Box>

      {!settings && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          No settings loaded. <Button size="small" onClick={loadSettings}>Try again</Button>
        </Alert>
      )}

      {/* Tabs */}
      <Tabs value={tabIndex} onChange={(_, v) => setTabIndex(v)} sx={{ borderBottom: 1, borderColor: 'divider' }}>
        <Tab icon={<SettingsIcon />} iconPosition="start" label="General" />
        <Tab icon={<Cloud />} iconPosition="start" label={`Providers (${PROVIDER_DEFINITIONS.filter(p => getProviderSettings(p.key)?.isConfigured).length}/${PROVIDER_DEFINITIONS.length})`} />
        <Tab icon={<Speed />} iconPosition="start" label="Status" />
      </Tabs>

      <TabPanel value={tabIndex} index={0}>{renderGeneralTab()}</TabPanel>
      <TabPanel value={tabIndex} index={1}>{renderProvidersTab()}</TabPanel>
      <TabPanel value={tabIndex} index={2}>{renderStatusTab()}</TabPanel>

      {/* Reset Confirmation Dialog */}
      <Dialog open={resetDialogOpen} onClose={() => setResetDialogOpen(false)}>
        <DialogTitle>Reset LLM Settings?</DialogTitle>
        <DialogContent>
          <Typography>
            This will remove all customized settings and API keys from the database, reverting
            to defaults from configuration files. This action cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setResetDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleReset} color="error" variant="contained">Reset</Button>
        </DialogActions>
      </Dialog>

      {/* Snackbar */}
      <Snackbar
        open={snackbar.open}
        autoHideDuration={6000}
        onClose={() => setSnackbar(prev => ({ ...prev, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert severity={snackbar.severity} onClose={() => setSnackbar(prev => ({ ...prev, open: false }))}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default LLMSettingsTab;
