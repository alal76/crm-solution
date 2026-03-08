import React, { useState, useEffect, useCallback } from 'react';
import {
  Box, Tabs, Tab, Card, CardContent, TextField, Button, Switch,
  FormControlLabel, Typography, Chip, CircularProgress, Snackbar, Alert,
  IconButton, InputAdornment, Slider, Divider, Stack, Grid,
  Accordion, AccordionSummary, AccordionDetails, List, ListItem,
  ListItemText, ListItemSecondaryAction,
} from '@mui/material';
import {
  Tune as TuneIcon,
  Visibility, VisibilityOff,
  CheckCircle, Error as ErrorIcon,
  ExpandMore, Send as TestIcon, Save as SaveIcon,
  SmartToy as AgentIcon, Memory as WorkerIcon,
} from '@mui/icons-material';
import AdminPageHeader from '../../components/admin/AdminPageHeader';
// UX-CONF-009: Import PortalConfigPage to render as a tab; /admin/portal redirects to /admin/config/crm
import PortalConfigPage from '../PortalConfigPage';
import { configurationService } from '../../services/configurationService';
import type {
  CRMConfigResponseDto, AIProviderConfigDto, IntegrationConfigDto,
  WorkerConfigDto, AIAgentConfigDto, ConfigurationTestResultDto,
} from '../../services/configurationService';

// ── Helpers ──────────────────────────────────────────────────────────────────

const StatusChip: React.FC<{ status?: string }> = ({ status }) => {
  if (!status) return <Chip label="Untested" size="small" variant="outlined" />;
  if (status === 'Connected' || status === 'Success')
    return <Chip label={status} size="small" color="success" icon={<CheckCircle />} />;
  return <Chip label={status} size="small" color="error" icon={<ErrorIcon />} />;
};

const MaskedField: React.FC<{
  label: string; value: string; onChange: (v: string) => void; fullWidth?: boolean;
}> = ({ label, value, onChange, fullWidth = true }) => {
  const [show, setShow] = useState(false);
  return (
    <TextField
      label={label} value={value} onChange={e => onChange(e.target.value)}
      type={show ? 'text' : 'password'} fullWidth={fullWidth} size="small"
      InputProps={{
        endAdornment: (
          <InputAdornment position="end">
            <IconButton size="small" onClick={() => setShow(!show)}>
              {show ? <VisibilityOff fontSize="small" /> : <Visibility fontSize="small" />}
            </IconButton>
          </InputAdornment>
        ),
      }}
    />
  );
};

// ── Integration grouping ─────────────────────────────────────────────────────

const INTEGRATION_GROUPS: { label: string; type: string }[] = [
  { label: 'Search', type: 'Search' },
  { label: 'Chat', type: 'Chat' },
  { label: 'Notifications', type: 'Notifications' },
  { label: 'Analytics', type: 'Analytics' },
  { label: 'E-Signatures', type: 'Signatures' },
  { label: 'Workflows', type: 'Integrations' },
];

// ── Default worker config ────────────────────────────────────────────────────

const defaultWorker: WorkerConfigDto = {
  enabled: true, maxConcurrentJobs: 5, jobTimeoutMinutes: 30,
  retryAttempts: 3, retryDelaySeconds: 30,
};

// ── Page ─────────────────────────────────────────────────────────────────────

const CRMConfigurationPage: React.FC = () => {
  const [tab, setTab] = useState(0);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [lastUpdated, setLastUpdated] = useState('');
  const [updatedBy, setUpdatedBy] = useState('');
  const [snack, setSnack] = useState<{ open: boolean; msg: string; severity: 'success' | 'error' }>({
    open: false, msg: '', severity: 'success',
  });

  // Data
  const [aiProviders, setAiProviders] = useState<AIProviderConfigDto[]>([]);
  const [integrations, setIntegrations] = useState<IntegrationConfigDto[]>([]);
  const [worker, setWorker] = useState<WorkerConfigDto>(defaultWorker);
  const [agents, setAgents] = useState<AIAgentConfigDto[]>([]);

  // Test state
  const [testingAI, setTestingAI] = useState<Record<string, boolean>>({});
  const [aiTestResults, setAiTestResults] = useState<Record<string, ConfigurationTestResultDto>>({});
  const [testingInteg, setTestingInteg] = useState<Record<string, boolean>>({});
  const [integTestResults, setIntegTestResults] = useState<Record<string, ConfigurationTestResultDto>>({});

  const loadConfig = useCallback(async () => {
    setLoading(true);
    try {
      const data: CRMConfigResponseDto = await configurationService.getCRMConfig();
      setAiProviders(data.aiProviders ?? []);
      setIntegrations(data.integrations ?? []);
      if (data.workerConfig) setWorker(data.workerConfig);
      setAgents(data.aiAgents ?? []);
      setLastUpdated(data.lastUpdated);
      setUpdatedBy(data.updatedBy ?? '');
    } catch {
      setSnack({ open: true, msg: 'Failed to load CRM configuration', severity: 'error' });
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadConfig(); }, [loadConfig]);

  const showSuccess = (msg: string) => setSnack({ open: true, msg, severity: 'success' });
  const showError = (msg: string) => setSnack({ open: true, msg, severity: 'error' });

  // ── AI Provider handlers ───────────────────────────────────────────────

  const updateProvider = (idx: number, updates: Partial<AIProviderConfigDto>) => {
    setAiProviders(prev => prev.map((p, i) => (i === idx ? { ...p, ...updates } : p)));
  };

  const saveAIProvider = async (provider: AIProviderConfigDto) => {
    setSaving(true);
    try {
      await configurationService.updateAIProviderConfig(provider.provider, provider);
      showSuccess(`${provider.provider} configuration saved`);
      loadConfig();
    } catch { showError(`Failed to save ${provider.provider} configuration`); }
    finally { setSaving(false); }
  };

  const testAIProvider = async (provider: AIProviderConfigDto) => {
    const key = provider.provider;
    setTestingAI(p => ({ ...p, [key]: true }));
    try {
      const result = await configurationService.testAIProvider(key, provider);
      setAiTestResults(p => ({ ...p, [key]: result }));
    } catch {
      setAiTestResults(p => ({
        ...p, [key]: { success: false, message: 'Test request failed', testedAt: new Date().toISOString() },
      }));
    } finally { setTestingAI(p => ({ ...p, [key]: false })); }
  };

  // ── Integration handlers ───────────────────────────────────────────────

  const updateIntegration = (idx: number, updates: Partial<IntegrationConfigDto>) => {
    setIntegrations(prev => prev.map((it, i) => (i === idx ? { ...it, ...updates } : it)));
  };

  const saveIntegration = async (integ: IntegrationConfigDto) => {
    setSaving(true);
    try {
      await configurationService.updateIntegrationConfig(integ.type, integ.provider, integ);
      showSuccess(`${integ.provider} integration saved`);
      loadConfig();
    } catch { showError(`Failed to save ${integ.provider} integration`); }
    finally { setSaving(false); }
  };

  const testIntegration = async (integ: IntegrationConfigDto) => {
    const key = `${integ.type}-${integ.provider}`;
    setTestingInteg(p => ({ ...p, [key]: true }));
    try {
      const result = await configurationService.testIntegration(integ.type, integ.provider, integ);
      setIntegTestResults(p => ({ ...p, [key]: result }));
    } catch {
      setIntegTestResults(p => ({
        ...p, [key]: { success: false, message: 'Test request failed', testedAt: new Date().toISOString() },
      }));
    } finally { setTestingInteg(p => ({ ...p, [key]: false })); }
  };

  // ── Worker handlers ────────────────────────────────────────────────────

  const saveWorker = async () => {
    setSaving(true);
    try {
      await configurationService.updateWorkerConfig(worker);
      showSuccess('Worker configuration saved');
      loadConfig();
    } catch { showError('Failed to save worker configuration'); }
    finally { setSaving(false); }
  };

  // ── Agent handlers ─────────────────────────────────────────────────────

  const toggleAgent = (idx: number) => {
    setAgents(prev => prev.map((a, i) => (i === idx ? { ...a, enabled: !a.enabled } : a)));
  };

  const saveAgents = async () => {
    setSaving(true);
    try {
      await configurationService.updateAIAgentsConfig(agents);
      showSuccess('AI agent configuration saved');
      loadConfig();
    } catch { showError('Failed to save AI agent configuration'); }
    finally { setSaving(false); }
  };

  // ── Render ─────────────────────────────────────────────────────────────

  if (loading) {
    return (
      <Box sx={{ p: 3 }}>
        <AdminPageHeader title="CRM Configuration" subtitle="Manage CRM providers and settings" icon={TuneIcon} />
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      <AdminPageHeader title="CRM Configuration" subtitle="Manage AI providers, integrations, workers, and agents" icon={TuneIcon} />

      {lastUpdated && (
        <Typography variant="caption" color="text.secondary" sx={{ mb: 2, display: 'block' }}>
          Last updated: {new Date(lastUpdated).toLocaleString()}{updatedBy && ` by ${updatedBy}`}
        </Typography>
      )}

      <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 3 }} variant="scrollable" scrollButtons="auto">
        <Tab label="AI / LLM Providers" />
        <Tab label="Integrations" />
        <Tab label="Worker Config" />
        <Tab label="AI Agents" />
        {/* UX-CONF-009: Customer Portal tab — absorbs /admin/portal */}
        <Tab label="Customer Portal" />
      </Tabs>

      {/* ── AI / LLM Providers Tab ────────────────────────────────────────── */}
      {tab === 0 && (
        <Grid container spacing={2}>
          {aiProviders.map((prov, idx) => {
            const testResult = aiTestResults[prov.provider];
            const isTesting = testingAI[prov.provider];
            return (
              <Grid item xs={12} md={6} key={prov.provider}>
                <Card variant="outlined">
                  <CardContent>
                    <Stack spacing={2}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Typography variant="h6" sx={{ flexGrow: 1 }}>{prov.provider}</Typography>
                        <StatusChip status={prov.connectionStatus} />
                      </Box>

                      <FormControlLabel
                        control={<Switch checked={prov.enabled} onChange={e => updateProvider(idx, { enabled: e.target.checked })} />}
                        label="Enabled"
                      />

                      <MaskedField label="API Key" value={prov.apiKey ?? ''}
                        onChange={v => updateProvider(idx, { apiKey: v })} />
                      <TextField label="API URL" value={prov.apiUrl ?? ''} size="small" fullWidth
                        onChange={e => updateProvider(idx, { apiUrl: e.target.value })} />
                      <TextField label="Organization ID" value={prov.organizationId ?? ''} size="small" fullWidth
                        onChange={e => updateProvider(idx, { organizationId: e.target.value })} />
                      <TextField label="Model" value={prov.model ?? ''} size="small" fullWidth
                        onChange={e => updateProvider(idx, { model: e.target.value })} />

                      <Box>
                        <Typography variant="caption" gutterBottom>
                          Temperature: {prov.temperature?.toFixed(2) ?? '0.70'}
                        </Typography>
                        <Slider
                          value={prov.temperature ?? 0.7} min={0} max={2} step={0.01}
                          onChange={(_, v) => updateProvider(idx, { temperature: v as number })}
                          size="small"
                        />
                      </Box>

                      <TextField label="Max Tokens" type="number" value={prov.maxTokens ?? ''} size="small" fullWidth
                        onChange={e => updateProvider(idx, { maxTokens: e.target.value ? Number(e.target.value) : undefined })} />

                      <FormControlLabel
                        control={<Switch checked={prov.costTrackingEnabled} onChange={e => updateProvider(idx, { costTrackingEnabled: e.target.checked })} />}
                        label="Cost Tracking"
                      />

                      {testResult && (
                        <Alert severity={testResult.success ? 'success' : 'error'} sx={{ mt: 1 }}>
                          {testResult.message || (testResult.success ? 'Connected' : 'Failed')}
                          {testResult.errorDetails && (
                            <Typography variant="caption" display="block">{testResult.errorDetails}</Typography>
                          )}
                        </Alert>
                      )}

                      <Box sx={{ display: 'flex', gap: 1 }}>
                        <Button size="small" variant="outlined"
                          startIcon={isTesting ? <CircularProgress size={14} /> : <TestIcon />}
                          onClick={() => testAIProvider(prov)} disabled={isTesting || saving}>
                          Test
                        </Button>
                        <Button size="small" variant="contained"
                          startIcon={saving ? <CircularProgress size={14} /> : <SaveIcon />}
                          onClick={() => saveAIProvider(prov)} disabled={saving}>
                          Save
                        </Button>
                      </Box>
                    </Stack>
                  </CardContent>
                </Card>
              </Grid>
            );
          })}

          {aiProviders.length === 0 && (
            <Grid item xs={12}>
              <Alert severity="info">No AI providers configured. Providers will appear here once the backend configuration is set up.</Alert>
            </Grid>
          )}
        </Grid>
      )}

      {/* ── Integrations Tab ──────────────────────────────────────────────── */}
      {tab === 1 && (
        <Stack spacing={2}>
          {INTEGRATION_GROUPS.map(group => {
            const groupIntegrations = integrations.filter(i => i.type === group.type);
            if (groupIntegrations.length === 0) return null;

            return (
              <Accordion key={group.type} defaultExpanded disableGutters>
                <AccordionSummary expandIcon={<ExpandMore />}>
                  <Typography variant="subtitle1" fontWeight={600}>{group.label}</Typography>
                  <Chip label={groupIntegrations.length} size="small" sx={{ ml: 1 }} />
                </AccordionSummary>
                <AccordionDetails>
                  <Stack spacing={2}>
                    {groupIntegrations.map(integ => {
                      const globalIdx = integrations.indexOf(integ);
                      const testKey = `${integ.type}-${integ.provider}`;
                      const testResult = integTestResults[testKey];
                      const isTesting = testingInteg[testKey];

                      return (
                        <Card variant="outlined" key={testKey}>
                          <CardContent>
                            <Stack spacing={1.5}>
                              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <Typography variant="subtitle1" sx={{ flexGrow: 1 }}>{integ.provider}</Typography>
                                <StatusChip status={integ.connectionStatus} />
                              </Box>

                              <Box sx={{ display: 'flex', gap: 2 }}>
                                <FormControlLabel
                                  control={<Switch checked={integ.enabled} onChange={e => updateIntegration(globalIdx, { enabled: e.target.checked })} />}
                                  label="Enabled"
                                />
                                <FormControlLabel
                                  control={<Switch checked={integ.useBuiltIn} onChange={e => updateIntegration(globalIdx, { useBuiltIn: e.target.checked })} />}
                                  label="Use Built-In"
                                />
                              </Box>

                              {integ.testEndpoint && (
                                <TextField label="Test Endpoint" value={integ.testEndpoint} size="small" fullWidth disabled />
                              )}

                              {integ.credentials && Object.keys(integ.credentials).length > 0 && (
                                <>
                                  <Divider />
                                  <Typography variant="caption" fontWeight={600}>Credentials</Typography>
                                  {Object.entries(integ.credentials).map(([k, v]) => (
                                    <MaskedField key={k} label={k} value={v}
                                      onChange={val => {
                                        const creds = { ...integ.credentials, [k]: val };
                                        updateIntegration(globalIdx, { credentials: creds });
                                      }}
                                    />
                                  ))}
                                </>
                              )}

                              {testResult && (
                                <Alert severity={testResult.success ? 'success' : 'error'}>
                                  {testResult.message || (testResult.success ? 'Connected' : 'Failed')}
                                </Alert>
                              )}

                              <Box sx={{ display: 'flex', gap: 1 }}>
                                <Button size="small" variant="outlined"
                                  startIcon={isTesting ? <CircularProgress size={14} /> : <TestIcon />}
                                  onClick={() => testIntegration(integ)} disabled={isTesting || saving}>
                                  Test
                                </Button>
                                <Button size="small" variant="contained"
                                  startIcon={saving ? <CircularProgress size={14} /> : <SaveIcon />}
                                  onClick={() => saveIntegration(integ)} disabled={saving}>
                                  Save
                                </Button>
                              </Box>
                            </Stack>
                          </CardContent>
                        </Card>
                      );
                    })}
                  </Stack>
                </AccordionDetails>
              </Accordion>
            );
          })}

          {integrations.length === 0 && (
            <Alert severity="info">No integrations configured. Integrations will appear here once the backend configuration is set up.</Alert>
          )}
        </Stack>
      )}

      {/* ── Worker Config Tab ─────────────────────────────────────────────── */}
      {tab === 2 && (
        <Card>
          <CardContent>
            <Stack spacing={2.5}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <WorkerIcon />
                <Typography variant="h6">Background Worker Configuration</Typography>
              </Box>

              <FormControlLabel
                control={<Switch checked={worker.enabled} onChange={e => setWorker({ ...worker, enabled: e.target.checked })} />}
                label="Enable background worker"
              />

              <TextField label="Max Concurrent Jobs" type="number" value={worker.maxConcurrentJobs} size="small" fullWidth
                onChange={e => setWorker({ ...worker, maxConcurrentJobs: Number(e.target.value) })}
                inputProps={{ min: 1, max: 50 }} />

              <TextField label="Job Timeout (minutes)" type="number" value={worker.jobTimeoutMinutes} size="small" fullWidth
                onChange={e => setWorker({ ...worker, jobTimeoutMinutes: Number(e.target.value) })}
                inputProps={{ min: 1, max: 1440 }} />

              <TextField label="Retry Attempts" type="number" value={worker.retryAttempts} size="small" fullWidth
                onChange={e => setWorker({ ...worker, retryAttempts: Number(e.target.value) })}
                inputProps={{ min: 0, max: 10 }} />

              <TextField label="Retry Delay (seconds)" type="number" value={worker.retryDelaySeconds} size="small" fullWidth
                onChange={e => setWorker({ ...worker, retryDelaySeconds: Number(e.target.value) })}
                inputProps={{ min: 1, max: 3600 }} />

              <TextField label="Cron Schedule Expression" value={worker.scheduleExpression ?? ''} size="small" fullWidth
                onChange={e => setWorker({ ...worker, scheduleExpression: e.target.value })}
                helperText="Standard cron expression (e.g. '0 */5 * * * *' for every 5 minutes)"
                placeholder="0 */5 * * * *" />

              <Box sx={{ display: 'flex', gap: 2, mt: 1 }}>
                <Button variant="contained" startIcon={saving ? <CircularProgress size={16} /> : <SaveIcon />}
                  onClick={saveWorker} disabled={saving}>
                  Save
                </Button>
              </Box>
            </Stack>
          </CardContent>
        </Card>
      )}

      {/* ── AI Agents Tab ─────────────────────────────────────────────────── */}
      {tab === 3 && (
        <Card>
          <CardContent>
            <Stack spacing={2}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <AgentIcon />
                <Typography variant="h6">AI Agents</Typography>
              </Box>
              <Typography variant="body2" color="text.secondary">
                Enable or disable individual AI agents. Changes apply after saving.
              </Typography>

              {agents.length === 0 ? (
                <Alert severity="info">No AI agents configured.</Alert>
              ) : (
                <List>
                  {agents.map((agent, idx) => (
                    <ListItem key={agent.id} divider sx={{ py: 1.5 }}>
                      <ListItemText
                        primary={agent.name}
                        secondary={agent.description}
                        primaryTypographyProps={{ fontWeight: 500 }}
                      />
                      <ListItemSecondaryAction>
                        <Switch
                          edge="end"
                          checked={agent.enabled}
                          onChange={() => toggleAgent(idx)}
                        />
                      </ListItemSecondaryAction>
                    </ListItem>
                  ))}
                </List>
              )}

              {agents.length > 0 && (
                <Box sx={{ display: 'flex', gap: 2 }}>
                  <Button variant="contained" startIcon={saving ? <CircularProgress size={16} /> : <SaveIcon />}
                    onClick={saveAgents} disabled={saving}>
                    Save Agent Settings
                  </Button>
                </Box>
              )}
            </Stack>
          </CardContent>
        </Card>
      )}

      {/* ── Customer Portal Tab (UX-CONF-009) ──────────────────────────── */}
      {tab === 4 && (
        // UX-CONF-009: PortalConfigPage rendered inline; /admin/portal redirects here
        <PortalConfigPage />
      )}

      <Snackbar open={snack.open} autoHideDuration={4000} onClose={() => setSnack(s => ({ ...s, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}>
        <Alert severity={snack.severity} onClose={() => setSnack(s => ({ ...s, open: false }))}>{snack.msg}</Alert>
      </Snackbar>
    </Box>
  );
};

export default CRMConfigurationPage;
