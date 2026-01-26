/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * LLM Settings Tab - Configure AI/LLM providers and settings
 * Now supports editing and saving settings to database
 */

import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Alert,
  CircularProgress,
  Chip,
  Grid,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  ListItemSecondaryAction,
  Tooltip,
  Button,
  TextField,
  Divider,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Slider,
  FormControlLabel,
  Switch,
  Snackbar,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Paper,
  Tabs,
  Tab,
} from '@mui/material';
import {
  ExpandMore as ExpandMoreIcon,
  Psychology as LLMIcon,
  CheckCircle as ConfiguredIcon,
  Cancel as NotConfiguredIcon,
  Info as InfoIcon,
  Refresh as RefreshIcon,
  OpenInNew as OpenInNewIcon,
  SmartToy as AIIcon,
  Cloud as CloudIcon,
  Computer as LocalIcon,
  Settings as SettingsIcon,
  Save as SaveIcon,
  RestartAlt as ResetIcon,
  Tune as TuneIcon,
} from '@mui/icons-material';
import { getApiEndpoint } from '../../config/ports';

interface LLMProvider {
  value: string;
  label: string;
  isConfigured: boolean;
  models: LLMModel[];
}

interface LLMModel {
  value: string;
  label: string;
  provider: string;
  isDefault?: boolean;
}

interface LLMProviderSettings {
  defaultModel: string;
  baseUrl?: string;
  apiVersion?: string;
  location?: string;
  region?: string;
  apiFormat?: string;
  enabled?: boolean;
  useVertexAI?: boolean;
  useDefaultCredentials?: boolean;
  isConfigured: boolean;
}

interface LLMSettings {
  defaultProvider: string;
  enableFallback: boolean;
  fallbackOrder: string[];
  defaultMaxTokens: number;
  defaultTemperature: number;
  timeoutSeconds: number;
  maxRetries: number;
  openAI: LLMProviderSettings;
  azure: LLMProviderSettings;
  anthropic: LLMProviderSettings;
  google: LLMProviderSettings;
  bedrock: LLMProviderSettings;
  deepSeek: LLMProviderSettings;
  local: LLMProviderSettings;
}

interface CircuitBreakerState {
  serviceName: string;
  state: string;
  lastStateChange?: string;
  failureCount: number;
  successCount: number;
  lastFailure?: string;
  lastError?: string;
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

const providerInfo: Record<string, { description: string; docsUrl: string; icon: React.ElementType }> = {
  openai: {
    description: 'OpenAI provides GPT-4, GPT-4 Turbo, and GPT-3.5 models. Best for general-purpose AI tasks.',
    docsUrl: 'https://platform.openai.com/docs',
    icon: CloudIcon,
  },
  azure: {
    description: 'Azure OpenAI Service provides enterprise-grade OpenAI models with Azure security and compliance.',
    docsUrl: 'https://learn.microsoft.com/azure/cognitive-services/openai/',
    icon: CloudIcon,
  },
  anthropic: {
    description: 'Anthropic Claude models excel at analysis, coding, and following complex instructions.',
    docsUrl: 'https://docs.anthropic.com/',
    icon: CloudIcon,
  },
  google: {
    description: 'Google Gemini models offer multimodal capabilities and strong reasoning.',
    docsUrl: 'https://ai.google.dev/docs',
    icon: CloudIcon,
  },
  bedrock: {
    description: 'AWS Bedrock provides access to multiple AI models through AWS infrastructure.',
    docsUrl: 'https://docs.aws.amazon.com/bedrock/',
    icon: CloudIcon,
  },
  deepseek: {
    description: 'DeepSeek offers high-quality models at competitive pricing, with strong coding capabilities.',
    docsUrl: 'https://platform.deepseek.com/docs',
    icon: CloudIcon,
  },
  local: {
    description: 'Run AI models locally using Ollama, LM Studio, or vLLM for privacy and offline use.',
    docsUrl: 'https://ollama.ai/',
    icon: LocalIcon,
  },
  custom: {
    description: 'Connect to any OpenAI-compatible API endpoint.',
    docsUrl: '',
    icon: SettingsIcon,
  },
};

const availableProviders = ['openai', 'azure', 'anthropic', 'google', 'bedrock', 'deepseek', 'local'];

export const LLMSettingsTab: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [providers, setProviders] = useState<LLMProvider[]>([]);
  const [settings, setSettings] = useState<LLMSettings | null>(null);
  const [editedSettings, setEditedSettings] = useState<Partial<LLMSettings>>({});
  const [circuitBreakers, setCircuitBreakers] = useState<CircuitBreakerState[]>([]);
  const [expandedProvider, setExpandedProvider] = useState<string | false>(false);
  const [tabValue, setTabValue] = useState(0);
  const [resetDialogOpen, setResetDialogOpen] = useState(false);
  const [hasChanges, setHasChanges] = useState(false);

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const token = localStorage.getItem('accessToken');
      
      // Fetch workflow config for providers list
      const configResponse = await fetch(getApiEndpoint('/workflow/config'), {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (configResponse.ok) {
        const config = await configResponse.json();
        setProviders(config.llmProviders || []);
      }

      // Fetch LLM settings from database
      const settingsResponse = await fetch(getApiEndpoint('/workflow/llm-settings'), {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (settingsResponse.ok) {
        const settingsData = await settingsResponse.json();
        setSettings(settingsData);
        setEditedSettings({});
        setHasChanges(false);
      }

      // Fetch circuit breaker states
      try {
        const resilienceResponse = await fetch(getApiEndpoint('/monitoring/resilience'), {
          headers: { Authorization: `Bearer ${token}` },
        });
        if (resilienceResponse.ok) {
          const resilienceData = await resilienceResponse.json();
          setCircuitBreakers(resilienceData.circuitBreakers || []);
        }
      } catch {
        // Resilience endpoint may not exist yet
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch LLM configuration');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleSettingsChange = <K extends keyof LLMSettings>(key: K, value: LLMSettings[K]) => {
    setEditedSettings(prev => ({ ...prev, [key]: value }));
    setHasChanges(true);
  };

  const handleProviderSettingsChange = (provider: string, key: string, value: any) => {
    setEditedSettings(prev => ({
      ...prev,
      providers: {
        ...(prev as any)?.providers,
        [provider]: {
          ...((prev as any)?.providers?.[provider] || {}),
          [key]: value,
        },
      },
    }));
    setHasChanges(true);
  };

  const saveSettings = async () => {
    setSaving(true);
    setError(null);
    try {
      const token = localStorage.getItem('accessToken');
      const response = await fetch(getApiEndpoint('/workflow/llm-settings'), {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(editedSettings),
      });

      if (response.ok) {
        const updatedSettings = await response.json();
        setSettings(updatedSettings);
        setEditedSettings({});
        setHasChanges(false);
        setSuccess('LLM settings saved successfully');
      } else {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to save settings');
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save settings');
    } finally {
      setSaving(false);
    }
  };

  const resetSettings = async () => {
    setSaving(true);
    setError(null);
    try {
      const token = localStorage.getItem('accessToken');
      const response = await fetch(getApiEndpoint('/workflow/llm-settings/reset'), {
        method: 'POST',
        headers: { Authorization: `Bearer ${token}` },
      });

      if (response.ok) {
        const updatedSettings = await response.json();
        setSettings(updatedSettings);
        setEditedSettings({});
        setHasChanges(false);
        setSuccess('Settings reset to defaults');
        setResetDialogOpen(false);
      } else {
        throw new Error('Failed to reset settings');
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to reset settings');
    } finally {
      setSaving(false);
    }
  };

  const getMergedValue = <K extends keyof LLMSettings>(key: K): LLMSettings[K] => {
    if (editedSettings[key] !== undefined) {
      return editedSettings[key] as LLMSettings[K];
    }
    return settings?.[key] as LLMSettings[K];
  };

  const configuredCount = providers.filter(p => p.isConfigured).length;
  const totalModels = providers.reduce((acc, p) => acc + p.models.length, 0);

  const getCircuitBreakerForProvider = (providerId: string) => {
    return circuitBreakers.find(cb => 
      cb.serviceName.toLowerCase().includes(providerId.toLowerCase()) ||
      cb.serviceName.toLowerCase().includes('llm')
    );
  };

  const getCircuitStateColor = (state: string) => {
    switch (state?.toLowerCase()) {
      case 'closed': return 'success';
      case 'open': return 'error';
      case 'halfopen': return 'warning';
      default: return 'default';
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
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
        <Box>
          <Typography variant="h6" sx={{ fontWeight: 600, display: 'flex', alignItems: 'center', gap: 1 }}>
            <LLMIcon color="primary" />
            AI / LLM Configuration
          </Typography>
          <Typography variant="body2" color="textSecondary">
            Configure AI providers for workflow automation, content generation, and intelligent features.
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={fetchData}
            size="small"
            disabled={saving}
          >
            Refresh
          </Button>
          <Button
            variant="outlined"
            color="warning"
            startIcon={<ResetIcon />}
            onClick={() => setResetDialogOpen(true)}
            size="small"
            disabled={saving}
          >
            Reset
          </Button>
          {hasChanges && (
            <Button
              variant="contained"
              startIcon={saving ? <CircularProgress size={16} /> : <SaveIcon />}
              onClick={saveSettings}
              size="small"
              disabled={saving}
            >
              Save Changes
            </Button>
          )}
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2, borderRadius: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Summary Cards */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={4}>
          <Card sx={{ borderRadius: 2 }}>
            <CardContent sx={{ textAlign: 'center' }}>
              <Typography variant="h4" color="primary.main" sx={{ fontWeight: 700 }}>
                {configuredCount}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Configured Providers
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={4}>
          <Card sx={{ borderRadius: 2 }}>
            <CardContent sx={{ textAlign: 'center' }}>
              <Typography variant="h4" color="secondary.main" sx={{ fontWeight: 700 }}>
                {providers.length}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Available Providers
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={4}>
          <Card sx={{ borderRadius: 2 }}>
            <CardContent sx={{ textAlign: 'center' }}>
              <Typography variant="h4" color="info.main" sx={{ fontWeight: 700 }}>
                {totalModels}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Available Models
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Tabs */}
      <Paper sx={{ borderRadius: 2, mb: 3 }}>
        <Tabs value={tabValue} onChange={(_, v) => setTabValue(v)} sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tab label="General Settings" icon={<TuneIcon />} iconPosition="start" />
          <Tab label="Providers" icon={<CloudIcon />} iconPosition="start" />
          <Tab label="Status" icon={<InfoIcon />} iconPosition="start" />
        </Tabs>

        {/* General Settings Tab */}
        <TabPanel value={tabValue} index={0}>
          <Box sx={{ p: 2 }}>
            <Grid container spacing={3}>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth size="small">
                  <InputLabel>Default Provider</InputLabel>
                  <Select
                    value={getMergedValue('defaultProvider') || 'openai'}
                    label="Default Provider"
                    onChange={(e) => handleSettingsChange('defaultProvider', e.target.value)}
                  >
                    {availableProviders.map(p => (
                      <MenuItem key={p} value={p}>
                        {p.charAt(0).toUpperCase() + p.slice(1)}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>

              <Grid item xs={12} md={6}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={getMergedValue('enableFallback') ?? true}
                      onChange={(e) => handleSettingsChange('enableFallback', e.target.checked)}
                    />
                  }
                  label="Enable Fallback to Other Providers"
                />
              </Grid>

              <Grid item xs={12} md={6}>
                <Typography variant="body2" gutterBottom>
                  Default Max Tokens: {getMergedValue('defaultMaxTokens') || 1000}
                </Typography>
                <Slider
                  value={getMergedValue('defaultMaxTokens') || 1000}
                  onChange={(_, v) => handleSettingsChange('defaultMaxTokens', v as number)}
                  min={100}
                  max={8000}
                  step={100}
                  valueLabelDisplay="auto"
                />
              </Grid>

              <Grid item xs={12} md={6}>
                <Typography variant="body2" gutterBottom>
                  Default Temperature: {getMergedValue('defaultTemperature') || 0.7}
                </Typography>
                <Slider
                  value={getMergedValue('defaultTemperature') || 0.7}
                  onChange={(_, v) => handleSettingsChange('defaultTemperature', v as number)}
                  min={0}
                  max={2}
                  step={0.1}
                  valueLabelDisplay="auto"
                />
              </Grid>

              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  size="small"
                  label="Timeout (seconds)"
                  type="number"
                  value={getMergedValue('timeoutSeconds') || 60}
                  onChange={(e) => handleSettingsChange('timeoutSeconds', parseInt(e.target.value) || 60)}
                  inputProps={{ min: 10, max: 300 }}
                />
              </Grid>

              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  size="small"
                  label="Max Retries"
                  type="number"
                  value={getMergedValue('maxRetries') || 3}
                  onChange={(e) => handleSettingsChange('maxRetries', parseInt(e.target.value) || 3)}
                  inputProps={{ min: 0, max: 10 }}
                />
              </Grid>
            </Grid>

            <Alert severity="info" sx={{ mt: 3, borderRadius: 2 }} icon={<InfoIcon />}>
              <Typography variant="body2">
                <strong>Note:</strong> API keys are configured via environment variables on the server for security.
                Only non-sensitive settings can be changed here.
              </Typography>
            </Alert>
          </Box>
        </TabPanel>

        {/* Providers Tab */}
        <TabPanel value={tabValue} index={1}>
          <Box sx={{ p: 2 }}>
            {providers.length === 0 ? (
              <Alert severity="warning" sx={{ borderRadius: 2 }}>
                No LLM providers are available. Please check the server configuration.
              </Alert>
            ) : (
              providers.map((provider) => {
                const info = providerInfo[provider.value] || {
                  description: 'AI provider',
                  docsUrl: '',
                  icon: AIIcon,
                };
                const ProviderIcon = info.icon;
                const circuitState = getCircuitBreakerForProvider(provider.value);

                return (
                  <Accordion
                    key={provider.value}
                    expanded={expandedProvider === provider.value}
                    onChange={(_, isExpanded) => setExpandedProvider(isExpanded ? provider.value : false)}
                    sx={{ mb: 1, borderRadius: 2, '&:before': { display: 'none' } }}
                  >
                    <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flex: 1 }}>
                        <ProviderIcon color={provider.isConfigured ? 'primary' : 'disabled'} />
                        <Box sx={{ flex: 1 }}>
                          <Typography sx={{ fontWeight: 500 }}>{provider.label}</Typography>
                          <Typography variant="caption" color="textSecondary">
                            {provider.models.length} model{provider.models.length !== 1 ? 's' : ''} available
                          </Typography>
                        </Box>
                        <Chip
                          icon={provider.isConfigured ? <ConfiguredIcon /> : <NotConfiguredIcon />}
                          label={provider.isConfigured ? 'Configured' : 'Not Configured'}
                          color={provider.isConfigured ? 'success' : 'default'}
                          size="small"
                          variant={provider.isConfigured ? 'filled' : 'outlined'}
                        />
                        {circuitState && (
                          <Tooltip title={`Circuit Breaker: ${circuitState.state}`}>
                            <Chip
                              label={circuitState.state}
                              color={getCircuitStateColor(circuitState.state) as any}
                              size="small"
                              variant="outlined"
                            />
                          </Tooltip>
                        )}
                      </Box>
                    </AccordionSummary>
                    <AccordionDetails>
                      <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
                        {info.description}
                      </Typography>

                      {info.docsUrl && (
                        <Button
                          size="small"
                          startIcon={<OpenInNewIcon />}
                          href={info.docsUrl}
                          target="_blank"
                          rel="noopener noreferrer"
                          sx={{ mb: 2 }}
                        >
                          View Documentation
                        </Button>
                      )}

                      <Divider sx={{ my: 2 }} />

                      {/* Provider-specific settings */}
                      <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 2 }}>
                        Provider Settings
                      </Typography>
                      <Grid container spacing={2}>
                        <Grid item xs={12} md={6}>
                          <FormControl fullWidth size="small">
                            <InputLabel>Default Model</InputLabel>
                            <Select
                              value={(settings as any)?.[provider.value]?.defaultModel || provider.models[0]?.value || ''}
                              label="Default Model"
                              onChange={(e) => handleProviderSettingsChange(provider.value, 'defaultModel', e.target.value)}
                            >
                              {provider.models.map(m => (
                                <MenuItem key={m.value} value={m.value}>{m.label}</MenuItem>
                              ))}
                            </Select>
                          </FormControl>
                        </Grid>
                        {provider.value === 'local' && (
                          <>
                            <Grid item xs={12} md={6}>
                              <TextField
                                fullWidth
                                size="small"
                                label="Base URL"
                                value={(settings as any)?.local?.baseUrl || 'http://localhost:11434'}
                                onChange={(e) => handleProviderSettingsChange('local', 'baseUrl', e.target.value)}
                              />
                            </Grid>
                            <Grid item xs={12} md={6}>
                              <FormControlLabel
                                control={
                                  <Switch
                                    checked={(settings as any)?.local?.enabled ?? false}
                                    onChange={(e) => handleProviderSettingsChange('local', 'enabled', e.target.checked)}
                                  />
                                }
                                label="Enable Local LLM"
                              />
                            </Grid>
                          </>
                        )}
                      </Grid>

                      <Divider sx={{ my: 2 }} />

                      <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
                        Available Models
                      </Typography>

                      {provider.models.length === 0 ? (
                        <Typography variant="body2" color="textSecondary">
                          No models available for this provider.
                        </Typography>
                      ) : (
                        <List dense>
                          {provider.models.map((model) => (
                            <ListItem key={model.value} sx={{ py: 0.5 }}>
                              <ListItemIcon sx={{ minWidth: 36 }}>
                                <AIIcon fontSize="small" color={model.isDefault ? 'primary' : 'disabled'} />
                              </ListItemIcon>
                              <ListItemText
                                primary={model.label}
                                secondary={model.value}
                                primaryTypographyProps={{ fontSize: '0.875rem' }}
                                secondaryTypographyProps={{ fontSize: '0.75rem', fontFamily: 'monospace' }}
                              />
                              {model.isDefault && (
                                <ListItemSecondaryAction>
                                  <Chip label="Default" size="small" color="primary" variant="outlined" />
                                </ListItemSecondaryAction>
                              )}
                            </ListItem>
                          ))}
                        </List>
                      )}
                    </AccordionDetails>
                  </Accordion>
                );
              })
            )}
          </Box>
        </TabPanel>

        {/* Status Tab */}
        <TabPanel value={tabValue} index={2}>
          <Box sx={{ p: 2 }}>
            <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 2 }}>
              Circuit Breaker Status
            </Typography>
            {circuitBreakers.length === 0 ? (
              <Alert severity="info" sx={{ borderRadius: 2 }}>
                No circuit breaker data available. Make some LLM requests to populate this data.
              </Alert>
            ) : (
              <Grid container spacing={2}>
                {circuitBreakers.filter(cb => cb.serviceName.includes('llm')).map(cb => (
                  <Grid item xs={12} md={6} key={cb.serviceName}>
                    <Card sx={{ borderRadius: 2 }}>
                      <CardContent>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
                          <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
                            {cb.serviceName.replace('llm-', '').toUpperCase()}
                          </Typography>
                          <Chip
                            label={cb.state}
                            color={getCircuitStateColor(cb.state) as any}
                            size="small"
                          />
                        </Box>
                        <Grid container spacing={1}>
                          <Grid item xs={6}>
                            <Typography variant="caption" color="textSecondary">Successes</Typography>
                            <Typography variant="body2" color="success.main">{cb.successCount}</Typography>
                          </Grid>
                          <Grid item xs={6}>
                            <Typography variant="caption" color="textSecondary">Failures</Typography>
                            <Typography variant="body2" color="error.main">{cb.failureCount}</Typography>
                          </Grid>
                        </Grid>
                        {cb.lastError && (
                          <Box sx={{ mt: 1 }}>
                            <Typography variant="caption" color="error.main">
                              Last Error: {cb.lastError}
                            </Typography>
                          </Box>
                        )}
                      </CardContent>
                    </Card>
                  </Grid>
                ))}
              </Grid>
            )}

            {/* Usage Tips */}
            <Card sx={{ mt: 3, borderRadius: 2, bgcolor: 'grey.50' }}>
              <CardContent>
                <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1, display: 'flex', alignItems: 'center', gap: 1 }}>
                  <InfoIcon fontSize="small" color="info" />
                  Usage Tips
                </Typography>
                <List dense>
                  <ListItem sx={{ py: 0 }}>
                    <ListItemText
                      primary="• LLM actions can be used in workflows for content generation, summarization, and classification"
                      primaryTypographyProps={{ variant: 'body2' }}
                    />
                  </ListItem>
                  <ListItem sx={{ py: 0 }}>
                    <ListItemText
                      primary="• Enable fallback providers for high availability - if one provider fails, another will be used"
                      primaryTypographyProps={{ variant: 'body2' }}
                    />
                  </ListItem>
                  <ListItem sx={{ py: 0 }}>
                    <ListItemText
                      primary="• Circuit breakers protect against cascading failures when a provider is down"
                      primaryTypographyProps={{ variant: 'body2' }}
                    />
                  </ListItem>
                  <ListItem sx={{ py: 0 }}>
                    <ListItemText
                      primary="• Use JSON mode for structured output when integrating with automated workflows"
                      primaryTypographyProps={{ variant: 'body2' }}
                    />
                  </ListItem>
                </List>
              </CardContent>
            </Card>
          </Box>
        </TabPanel>
      </Paper>

      {/* Reset Confirmation Dialog */}
      <Dialog open={resetDialogOpen} onClose={() => setResetDialogOpen(false)}>
        <DialogTitle>Reset LLM Settings?</DialogTitle>
        <DialogContent>
          <Typography>
            This will reset all LLM settings to their default values from the server configuration.
            Any customizations you've made will be lost.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setResetDialogOpen(false)}>Cancel</Button>
          <Button onClick={resetSettings} color="warning" variant="contained" disabled={saving}>
            {saving ? <CircularProgress size={20} /> : 'Reset to Defaults'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Success Snackbar */}
      <Snackbar
        open={!!success}
        autoHideDuration={4000}
        onClose={() => setSuccess(null)}
        message={success}
      />
    </Box>
  );
};

export default LLMSettingsTab;
