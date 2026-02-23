import React, { useState, useEffect, useCallback } from 'react';
import {
  Box, Tabs, Tab, Card, CardContent, TextField, Button, Switch,
  FormControlLabel, Typography, Chip, CircularProgress, Snackbar, Alert,
  IconButton, InputAdornment, Select, MenuItem, FormControl, InputLabel,
  Accordion, AccordionSummary, AccordionDetails, Divider, Stack,
} from '@mui/material';
import {
  Settings as SettingsIcon,
  Visibility, VisibilityOff,
  CheckCircle, Error as ErrorIcon, HelpOutline,
  ExpandMore, Send as TestIcon, Save as SaveIcon,
} from '@mui/icons-material';
import AdminPageHeader from '../../components/admin/AdminPageHeader';
import { configurationService } from '../../services/configurationService';
import type {
  SystemConfigResponseDto, EmailServerConfigDto, TwoFactorConfigDto,
  SocialLoginConfigDto, SocialLoginProviderConfig, ConfigurationTestResultDto,
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

// ── Default values ───────────────────────────────────────────────────────────

const defaultEmail: EmailServerConfigDto = {
  smtpServer: '', smtpPort: 587, useTls: true, fromEmail: '', fromName: '',
  username: '', password: '', isConfigured: false,
};
const defaultTwoFactor: TwoFactorConfigDto = { provider: 'disabled', required: false };
const defaultSocial: SocialLoginConfigDto = {
  google: { enabled: false }, microsoft: { enabled: false },
  azureAd: { enabled: false }, linkedIn: { enabled: false }, facebook: { enabled: false },
};

// ── Page ─────────────────────────────────────────────────────────────────────

const SystemConfigurationPage: React.FC = () => {
  const [tab, setTab] = useState(0);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [lastUpdated, setLastUpdated] = useState('');
  const [updatedBy, setUpdatedBy] = useState('');
  const [snack, setSnack] = useState<{ open: boolean; msg: string; severity: 'success' | 'error' }>({
    open: false, msg: '', severity: 'success',
  });

  // Form state
  const [email, setEmail] = useState<EmailServerConfigDto>(defaultEmail);
  const [twoFactor, setTwoFactor] = useState<TwoFactorConfigDto>(defaultTwoFactor);
  const [social, setSocial] = useState<SocialLoginConfigDto>(defaultSocial);

  // Test state
  const [testingEmail, setTestingEmail] = useState(false);
  const [emailTestResult, setEmailTestResult] = useState<ConfigurationTestResultDto | null>(null);
  const [testingSocial, setTestingSocial] = useState<Record<string, boolean>>({});
  const [socialTestResults, setSocialTestResults] = useState<Record<string, ConfigurationTestResultDto>>({});

  const loadConfig = useCallback(async () => {
    setLoading(true);
    try {
      const data: SystemConfigResponseDto = await configurationService.getSystemConfig();
      if (data.emailServer) setEmail(data.emailServer);
      if (data.twoFactor) setTwoFactor(data.twoFactor);
      if (data.socialLogin) setSocial(data.socialLogin);
      setLastUpdated(data.lastUpdated);
      setUpdatedBy(data.updatedBy ?? '');
    } catch {
      setSnack({ open: true, msg: 'Failed to load system configuration', severity: 'error' });
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadConfig(); }, [loadConfig]);

  const showSuccess = (msg: string) => setSnack({ open: true, msg, severity: 'success' });
  const showError = (msg: string) => setSnack({ open: true, msg, severity: 'error' });

  // ── Email handlers ─────────────────────────────────────────────────────

  const saveEmail = async () => {
    setSaving(true);
    try {
      await configurationService.updateEmailConfig(email);
      showSuccess('Email configuration saved');
      loadConfig();
    } catch { showError('Failed to save email configuration'); }
    finally { setSaving(false); }
  };

  const testEmail = async () => {
    setTestingEmail(true);
    setEmailTestResult(null);
    try {
      const result = await configurationService.testEmailConfig(email);
      setEmailTestResult(result);
    } catch { setEmailTestResult({ success: false, message: 'Test request failed', testedAt: new Date().toISOString() }); }
    finally { setTestingEmail(false); }
  };

  // ── Two-factor handlers ────────────────────────────────────────────────

  const saveTwoFactor = async () => {
    setSaving(true);
    try {
      await configurationService.updateTwoFactorConfig(twoFactor);
      showSuccess('Two-factor configuration saved');
      loadConfig();
    } catch { showError('Failed to save two-factor configuration'); }
    finally { setSaving(false); }
  };

  // ── Social login handlers ──────────────────────────────────────────────

  const saveSocial = async () => {
    setSaving(true);
    try {
      await configurationService.updateSocialLoginConfig(social);
      showSuccess('Social login configuration saved');
      loadConfig();
    } catch { showError('Failed to save social login configuration'); }
    finally { setSaving(false); }
  };

  const testSocialProvider = async (provider: string) => {
    setTestingSocial(p => ({ ...p, [provider]: true }));
    try {
      const cfg = (social as any)[provider] as SocialLoginProviderConfig | undefined;
      const result = await configurationService.testSocialLoginProvider(provider, cfg ?? {});
      setSocialTestResults(p => ({ ...p, [provider]: result }));
    } catch {
      setSocialTestResults(p => ({
        ...p, [provider]: { success: false, message: 'Test request failed', testedAt: new Date().toISOString() },
      }));
    } finally { setTestingSocial(p => ({ ...p, [provider]: false })); }
  };

  const updateSocialProvider = (key: string, updates: Partial<SocialLoginProviderConfig>) => {
    setSocial(prev => ({ ...prev, [key]: { ...(prev as any)[key], ...updates } }));
  };

  // ── Render ──────────────────────────────────────────────────────────────

  if (loading) {
    return (
      <Box sx={{ p: 3 }}>
        <AdminPageHeader title="System Configuration" subtitle="Manage system-level settings" icon={SettingsIcon} />
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      <AdminPageHeader title="System Configuration" subtitle="Manage email, authentication, and social login settings" icon={SettingsIcon} />

      {lastUpdated && (
        <Typography variant="caption" color="text.secondary" sx={{ mb: 2, display: 'block' }}>
          Last updated: {new Date(lastUpdated).toLocaleString()}{updatedBy && ` by ${updatedBy}`}
        </Typography>
      )}

      <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 3 }}>
        <Tab label="Email Server" />
        <Tab label="Two-Factor Auth" />
        <Tab label="Social Login" />
      </Tabs>

      {/* ── Email Server Tab ──────────────────────────────────────────────── */}
      {tab === 0 && (
        <Card>
          <CardContent>
            <Stack spacing={2.5}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <Typography variant="h6">SMTP Email Server</Typography>
                <StatusChip status={email.connectionStatus} />
              </Box>
              <Box sx={{ display: 'flex', gap: 2 }}>
                <TextField label="SMTP Server" value={email.smtpServer} onChange={e => setEmail({ ...email, smtpServer: e.target.value })} fullWidth size="small" />
                <TextField label="Port" type="number" value={email.smtpPort} onChange={e => setEmail({ ...email, smtpPort: Number(e.target.value) })} sx={{ width: 120 }} size="small" />
              </Box>
              <FormControlLabel
                control={<Switch checked={email.useTls} onChange={e => setEmail({ ...email, useTls: e.target.checked })} />}
                label="Use TLS / SSL"
              />
              <Box sx={{ display: 'flex', gap: 2 }}>
                <TextField label="From Email" value={email.fromEmail} onChange={e => setEmail({ ...email, fromEmail: e.target.value })} fullWidth size="small" />
                <TextField label="From Name" value={email.fromName} onChange={e => setEmail({ ...email, fromName: e.target.value })} fullWidth size="small" />
              </Box>
              <Box sx={{ display: 'flex', gap: 2 }}>
                <TextField label="Username" value={email.username ?? ''} onChange={e => setEmail({ ...email, username: e.target.value })} fullWidth size="small" />
                <MaskedField label="Password" value={email.password ?? ''} onChange={v => setEmail({ ...email, password: v })} />
              </Box>

              {emailTestResult && (
                <Alert severity={emailTestResult.success ? 'success' : 'error'} sx={{ mt: 1 }}>
                  {emailTestResult.message || (emailTestResult.success ? 'Connection successful' : 'Connection failed')}
                  {emailTestResult.errorDetails && (
                    <Typography variant="caption" display="block">{emailTestResult.errorDetails}</Typography>
                  )}
                </Alert>
              )}

              <Box sx={{ display: 'flex', gap: 2, mt: 1 }}>
                <Button variant="outlined" startIcon={testingEmail ? <CircularProgress size={16} /> : <TestIcon />}
                  onClick={testEmail} disabled={testingEmail || saving}>
                  Test Connection
                </Button>
                <Button variant="contained" startIcon={saving ? <CircularProgress size={16} /> : <SaveIcon />}
                  onClick={saveEmail} disabled={saving}>
                  Save
                </Button>
              </Box>
            </Stack>
          </CardContent>
        </Card>
      )}

      {/* ── Two-Factor Auth Tab ───────────────────────────────────────────── */}
      {tab === 1 && (
        <Card>
          <CardContent>
            <Stack spacing={2.5}>
              <Typography variant="h6">Two-Factor Authentication</Typography>

              <FormControl fullWidth size="small">
                <InputLabel>2FA Provider</InputLabel>
                <Select
                  value={twoFactor.provider}
                  label="2FA Provider"
                  onChange={e => setTwoFactor({ ...twoFactor, provider: e.target.value })}
                >
                  <MenuItem value="disabled">Disabled</MenuItem>
                  <MenuItem value="email">Email</MenuItem>
                  <MenuItem value="sms">SMS</MenuItem>
                  <MenuItem value="totp">TOTP (Authenticator App)</MenuItem>
                </Select>
              </FormControl>

              <FormControlLabel
                control={<Switch checked={twoFactor.required} onChange={e => setTwoFactor({ ...twoFactor, required: e.target.checked })} />}
                label="Require for all users"
              />

              {twoFactor.provider === 'sms' && (
                <>
                  <Divider />
                  <Typography variant="subtitle2">SMS Provider Settings</Typography>
                  <TextField label="SMS Provider" value={twoFactor.smsProvider ?? ''} onChange={e => setTwoFactor({ ...twoFactor, smsProvider: e.target.value })} fullWidth size="small" />
                  <MaskedField label="Twilio Account SID" value={twoFactor.twilioAccountSid ?? ''} onChange={v => setTwoFactor({ ...twoFactor, twilioAccountSid: v })} />
                  <MaskedField label="Twilio Auth Token" value={twoFactor.twilioAuthToken ?? ''} onChange={v => setTwoFactor({ ...twoFactor, twilioAuthToken: v })} />
                  <TextField label="Twilio From Number" value={twoFactor.twilioFromNumber ?? ''} onChange={e => setTwoFactor({ ...twoFactor, twilioFromNumber: e.target.value })} fullWidth size="small" />
                </>
              )}

              {twoFactor.provider === 'totp' && (
                <>
                  <Divider />
                  <Typography variant="subtitle2">TOTP Settings</Typography>
                  <TextField label="Issuer Name" value={twoFactor.issuer ?? ''} onChange={e => setTwoFactor({ ...twoFactor, issuer: e.target.value })} fullWidth size="small"
                    helperText="Displayed in authenticator apps (e.g. 'CRM Solution')" />
                </>
              )}

              <Box sx={{ display: 'flex', gap: 2, mt: 1 }}>
                <Button variant="contained" startIcon={saving ? <CircularProgress size={16} /> : <SaveIcon />}
                  onClick={saveTwoFactor} disabled={saving}>
                  Save
                </Button>
              </Box>
            </Stack>
          </CardContent>
        </Card>
      )}

      {/* ── Social Login Tab ──────────────────────────────────────────────── */}
      {tab === 2 && (
        <Card>
          <CardContent>
            <Stack spacing={2}>
              <Typography variant="h6">Social Login Providers</Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                Enable OAuth2 / OpenID Connect providers for user authentication.
              </Typography>

              {([
                { key: 'google' as const, label: 'Google', fields: ['clientId', 'clientSecret'] as string[] },
                { key: 'microsoft' as const, label: 'Microsoft', fields: ['clientId', 'clientSecret', 'tenantId'] as string[] },
                { key: 'azureAd' as const, label: 'Azure AD', fields: ['clientId', 'clientSecret', 'tenantId', 'authority'] as string[] },
                { key: 'linkedIn' as const, label: 'LinkedIn', fields: ['clientId', 'clientSecret'] as string[] },
                { key: 'facebook' as const, label: 'Facebook', fields: ['appId', 'appSecret'] as string[] },
              ]).map(({ key, label, fields }) => {
                const cfg = (social as any)[key] as SocialLoginProviderConfig | undefined;
                const testResult = socialTestResults[key];
                const isTesting = testingSocial[key];

                return (
                  <Accordion key={key} disableGutters>
                    <AccordionSummary expandIcon={<ExpandMore />}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, width: '100%' }}>
                        <Typography sx={{ flexGrow: 1 }}>{label}</Typography>
                        <Chip
                          label={cfg?.enabled ? 'Enabled' : 'Disabled'}
                          size="small"
                          color={cfg?.enabled ? 'success' : 'default'}
                          variant="outlined"
                        />
                      </Box>
                    </AccordionSummary>
                    <AccordionDetails>
                      <Stack spacing={2}>
                        <FormControlLabel
                          control={
                            <Switch
                              checked={cfg?.enabled ?? false}
                              onChange={e => updateSocialProvider(key, { enabled: e.target.checked })}
                            />
                          }
                          label={`Enable ${label} login`}
                        />

                        {fields.includes('clientId') && (
                          <TextField label="Client ID" value={cfg?.clientId ?? ''} size="small" fullWidth
                            onChange={e => updateSocialProvider(key, { clientId: e.target.value })} />
                        )}
                        {fields.includes('clientSecret') && (
                          <MaskedField label="Client Secret" value={cfg?.clientSecret ?? ''}
                            onChange={v => updateSocialProvider(key, { clientSecret: v })} />
                        )}
                        {fields.includes('appId') && (
                          <TextField label="App ID" value={cfg?.appId ?? ''} size="small" fullWidth
                            onChange={e => updateSocialProvider(key, { appId: e.target.value })} />
                        )}
                        {fields.includes('appSecret') && (
                          <MaskedField label="App Secret" value={cfg?.appSecret ?? ''}
                            onChange={v => updateSocialProvider(key, { appSecret: v })} />
                        )}
                        {fields.includes('tenantId') && (
                          <TextField label="Tenant ID" value={cfg?.tenantId ?? ''} size="small" fullWidth
                            onChange={e => updateSocialProvider(key, { tenantId: e.target.value })} />
                        )}
                        {fields.includes('authority') && (
                          <TextField label="Authority URL" value={cfg?.authority ?? ''} size="small" fullWidth
                            onChange={e => updateSocialProvider(key, { authority: e.target.value })} />
                        )}

                        {testResult && (
                          <Alert severity={testResult.success ? 'success' : 'error'} sx={{ mt: 1 }}>
                            {testResult.message || (testResult.success ? 'Provider reachable' : 'Provider unreachable')}
                          </Alert>
                        )}

                        <Box sx={{ display: 'flex', gap: 2 }}>
                          <Button size="small" variant="outlined"
                            startIcon={isTesting ? <CircularProgress size={14} /> : <TestIcon />}
                            onClick={() => testSocialProvider(key)} disabled={isTesting}>
                            Test
                          </Button>
                        </Box>
                      </Stack>
                    </AccordionDetails>
                  </Accordion>
                );
              })}

              <Box sx={{ display: 'flex', gap: 2, mt: 2 }}>
                <Button variant="contained" startIcon={saving ? <CircularProgress size={16} /> : <SaveIcon />}
                  onClick={saveSocial} disabled={saving}>
                  Save All Social Login Settings
                </Button>
              </Box>
            </Stack>
          </CardContent>
        </Card>
      )}

      <Snackbar open={snack.open} autoHideDuration={4000} onClose={() => setSnack(s => ({ ...s, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}>
        <Alert severity={snack.severity} onClose={() => setSnack(s => ({ ...s, open: false }))}>{snack.msg}</Alert>
      </Snackbar>
    </Box>
  );
};

export default SystemConfigurationPage;
