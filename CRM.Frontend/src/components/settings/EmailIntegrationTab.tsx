import React, { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Grid,
  Button,
  CircularProgress,
  Alert,
  Chip,
  Stack,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  FormControlLabel,
  Switch,
} from '@mui/material';
import {
  Email as EmailIcon,
  Sync as SyncIcon,
  Link as LinkIcon,
  LinkOff as LinkOffIcon,
  Add as AddIcon,
} from '@mui/icons-material';
import apiClient from '../../services/apiClient';

interface EmailIntegrationDto {
  id: number;
  provider: string;
  emailAddress: string;
  displayName?: string;
  syncIntervalMinutes: number;
  syncDaysBack: number;
  isActive: boolean;
  lastSyncAt?: string;
  lastSyncStatus?: string;
  lastSyncError?: string;
  nextSyncAt?: string;
  totalMessagesSynced: number;
  foldersToSync?: string[];
}

interface ImapConfigDto {
  provider: string;
  emailAddress: string;
  displayName?: string;
  imapHost: string;
  imapPort: number;
  imapUseSsl: boolean;
  smtpHost: string;
  smtpPort: number;
  smtpUseSsl: boolean;
  password: string;
  syncDaysBack: number;
  syncIntervalMinutes: number;
}

const defaultImapConfig: ImapConfigDto = {
  provider: 'Imap',
  emailAddress: '',
  displayName: '',
  imapHost: '',
  imapPort: 993,
  imapUseSsl: true,
  smtpHost: '',
  smtpPort: 587,
  smtpUseSsl: true,
  password: '',
  syncDaysBack: 30,
  syncIntervalMinutes: 15,
};

function EmailIntegrationTab() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [integrations, setIntegrations] = useState<EmailIntegrationDto[]>([]);
  const [savingId, setSavingId] = useState<number | null>(null);
  const [syncingId, setSyncingId] = useState<number | null>(null);
  const [disconnectingId, setDisconnectingId] = useState<number | null>(null);
  const [imapDialogOpen, setImapDialogOpen] = useState(false);
  const [imapConfig, setImapConfig] = useState<ImapConfigDto>(defaultImapConfig);
  const [savingImap, setSavingImap] = useState(false);

  useEffect(() => {
    loadIntegrations();
  }, []);

  const loadIntegrations = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await apiClient.get('/email/integrations');
      setIntegrations(response.data || []);
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || 'Failed to load email integrations');
    } finally {
      setLoading(false);
    }
  };

  const handleConnectOAuth = async (provider: 'google' | 'outlook') => {
    try {
      const response = await apiClient.get(`/email/connect/${provider}`);
      const authUrl = response.data?.authorizationUrl;
      if (authUrl) {
        window.location.href = authUrl;
      } else {
        setError('Authorization URL not returned');
      }
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || 'Failed to initiate OAuth connection');
    }
  };

  const handleOpenImapDialog = () => {
    setImapConfig(defaultImapConfig);
    setImapDialogOpen(true);
  };

  const handleCloseImapDialog = () => {
    setImapDialogOpen(false);
    setImapConfig(defaultImapConfig);
  };

  const handleSaveImapConfig = async () => {
    try {
      setSavingImap(true);
      await apiClient.post('/email/integrations', {
        provider: 'Imap',
        emailAddress: imapConfig.emailAddress,
        displayName: imapConfig.displayName,
        imapHost: imapConfig.imapHost,
        imapPort: imapConfig.imapPort,
        imapUseSsl: imapConfig.imapUseSsl,
        smtpHost: imapConfig.smtpHost,
        smtpPort: imapConfig.smtpPort,
        smtpUseSsl: imapConfig.smtpUseSsl,
        password: imapConfig.password,
        syncDaysBack: imapConfig.syncDaysBack,
        syncIntervalMinutes: imapConfig.syncIntervalMinutes,
      });
      await loadIntegrations();
      handleCloseImapDialog();
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || 'Failed to add IMAP account');
    } finally {
      setSavingImap(false);
    }
  };

  const handleUpdateSettings = async (integration: EmailIntegrationDto) => {
    try {
      setSavingId(integration.id);
      await apiClient.put(`/email/integrations/${integration.id}`, {
        syncIntervalMinutes: integration.syncIntervalMinutes,
        syncDaysBack: integration.syncDaysBack,
        displayName: integration.displayName,
      });
      await loadIntegrations();
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || 'Failed to update settings');
    } finally {
      setSavingId(null);
    }
  };

  const handleSyncNow = async (integration: EmailIntegrationDto) => {
    try {
      setSyncingId(integration.id);
      await apiClient.post(`/email/integrations/${integration.id}/sync`);
      await loadIntegrations();
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || 'Failed to sync email');
    } finally {
      setSyncingId(null);
    }
  };

  const handleDisconnect = async (integration: EmailIntegrationDto) => {
    try {
      setDisconnectingId(integration.id);
      await apiClient.delete(`/email/integrations/${integration.id}`);
      await loadIntegrations();
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || 'Failed to disconnect integration');
    } finally {
      setDisconnectingId(null);
    }
  };

  const updateIntegrationField = <K extends keyof EmailIntegrationDto>(id: number, field: K, value: EmailIntegrationDto[K]) => {
    setIntegrations(prev => prev.map(i => (i.id === id ? { ...i, [field]: value } : i)));
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight={200}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h5" fontWeight={600} gutterBottom>
        Email Integrations
      </Typography>
      <Typography variant="body2" color="textSecondary" gutterBottom>
        Connect email accounts to sync messages and enable unified inbox.
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ mb: 3 }}>
        <Button
          variant="contained"
          startIcon={<LinkIcon />}
          onClick={() => handleConnectOAuth('google')}
        >
          Connect Gmail
        </Button>
        <Button
          variant="contained"
          color="secondary"
          startIcon={<LinkIcon />}
          onClick={() => handleConnectOAuth('outlook')}
        >
          Connect Outlook
        </Button>
        <Button
          variant="outlined"
          startIcon={<AddIcon />}
          onClick={handleOpenImapDialog}
        >
          Add IMAP Account
        </Button>
      </Stack>

      <Grid container spacing={2}>
        {integrations.length === 0 && (
          <Grid item xs={12}>
            <Card variant="outlined">
              <CardContent>
                <Typography variant="body1">No email integrations connected yet.</Typography>
              </CardContent>
            </Card>
          </Grid>
        )}

        {integrations.map(integration => (
          <Grid item xs={12} md={6} key={integration.id}>
            <Card variant="outlined">
              <CardContent>
                <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
                  <EmailIcon color="primary" />
                  <Typography variant="h6">{integration.provider} Email</Typography>
                  <Chip
                    label={integration.isActive ? 'Active' : 'Inactive'}
                    color={integration.isActive ? 'success' : 'default'}
                    size="small"
                  />
                </Stack>

                <Typography variant="body2" color="textSecondary">
                  Account: {integration.emailAddress}
                </Typography>
                {integration.displayName && (
                  <Typography variant="body2" color="textSecondary">
                    Display Name: {integration.displayName}
                  </Typography>
                )}
                <Typography variant="body2" color="textSecondary">
                  Last Sync: {integration.lastSyncAt ? new Date(integration.lastSyncAt).toLocaleString() : 'Never'}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Next Sync: {integration.nextSyncAt ? new Date(integration.nextSyncAt).toLocaleString() : 'Pending'}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Total Messages Synced: {integration.totalMessagesSynced}
                </Typography>

                {integration.lastSyncError && (
                  <Alert severity="warning" sx={{ mt: 1 }}>
                    {integration.lastSyncError}
                  </Alert>
                )}

                <Box sx={{ mt: 2 }}>
                  <Grid container spacing={2}>
                    <Grid item xs={12} sm={6}>
                      <TextField
                        label="Sync Interval (minutes)"
                        type="number"
                        size="small"
                        fullWidth
                        value={integration.syncIntervalMinutes}
                        onChange={(e) => updateIntegrationField(integration.id, 'syncIntervalMinutes', Number(e.target.value))}
                        inputProps={{ min: 5, max: 1440 }}
                      />
                    </Grid>
                    <Grid item xs={12} sm={6}>
                      <TextField
                        label="Sync Days Back"
                        type="number"
                        size="small"
                        fullWidth
                        value={integration.syncDaysBack}
                        onChange={(e) => updateIntegrationField(integration.id, 'syncDaysBack', Number(e.target.value))}
                        inputProps={{ min: 1, max: 365 }}
                      />
                    </Grid>
                  </Grid>

                  <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} sx={{ mt: 2 }}>
                    <Button
                      variant="outlined"
                      startIcon={<SyncIcon />}
                      disabled={syncingId === integration.id}
                      onClick={() => handleSyncNow(integration)}
                    >
                      {syncingId === integration.id ? 'Syncing...' : 'Sync Now'}
                    </Button>
                    <Button
                      variant="contained"
                      onClick={() => handleUpdateSettings(integration)}
                      disabled={savingId === integration.id}
                    >
                      {savingId === integration.id ? 'Saving...' : 'Save Settings'}
                    </Button>
                    <Button
                      variant="text"
                      color="error"
                      startIcon={<LinkOffIcon />}
                      disabled={disconnectingId === integration.id}
                      onClick={() => handleDisconnect(integration)}
                    >
                      {disconnectingId === integration.id ? 'Disconnecting...' : 'Disconnect'}
                    </Button>
                  </Stack>
                </Box>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      {/* IMAP Configuration Dialog */}
      <Dialog open={imapDialogOpen} onClose={handleCloseImapDialog} maxWidth="sm" fullWidth>
        <DialogTitle>Add IMAP Email Account</DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 1 }}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField
                  label="Email Address"
                  type="email"
                  fullWidth
                  required
                  value={imapConfig.emailAddress}
                  onChange={(e) => setImapConfig({ ...imapConfig, emailAddress: e.target.value })}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  label="Display Name"
                  fullWidth
                  value={imapConfig.displayName}
                  onChange={(e) => setImapConfig({ ...imapConfig, displayName: e.target.value })}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  label="Password"
                  type="password"
                  fullWidth
                  required
                  value={imapConfig.password}
                  onChange={(e) => setImapConfig({ ...imapConfig, password: e.target.value })}
                  helperText="For Gmail, use an App Password"
                />
              </Grid>
              <Grid item xs={12}>
                <Typography variant="subtitle2" color="textSecondary" sx={{ mb: 1 }}>
                  IMAP Settings (Incoming)
                </Typography>
              </Grid>
              <Grid item xs={8}>
                <TextField
                  label="IMAP Host"
                  fullWidth
                  required
                  value={imapConfig.imapHost}
                  onChange={(e) => setImapConfig({ ...imapConfig, imapHost: e.target.value })}
                  placeholder="imap.example.com"
                />
              </Grid>
              <Grid item xs={4}>
                <TextField
                  label="Port"
                  type="number"
                  fullWidth
                  required
                  value={imapConfig.imapPort}
                  onChange={(e) => setImapConfig({ ...imapConfig, imapPort: Number(e.target.value) })}
                />
              </Grid>
              <Grid item xs={12}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={imapConfig.imapUseSsl}
                      onChange={(e) => setImapConfig({ ...imapConfig, imapUseSsl: e.target.checked })}
                    />
                  }
                  label="Use SSL/TLS"
                />
              </Grid>
              <Grid item xs={12}>
                <Typography variant="subtitle2" color="textSecondary" sx={{ mb: 1 }}>
                  SMTP Settings (Outgoing)
                </Typography>
              </Grid>
              <Grid item xs={8}>
                <TextField
                  label="SMTP Host"
                  fullWidth
                  required
                  value={imapConfig.smtpHost}
                  onChange={(e) => setImapConfig({ ...imapConfig, smtpHost: e.target.value })}
                  placeholder="smtp.example.com"
                />
              </Grid>
              <Grid item xs={4}>
                <TextField
                  label="Port"
                  type="number"
                  fullWidth
                  required
                  value={imapConfig.smtpPort}
                  onChange={(e) => setImapConfig({ ...imapConfig, smtpPort: Number(e.target.value) })}
                />
              </Grid>
              <Grid item xs={12}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={imapConfig.smtpUseSsl}
                      onChange={(e) => setImapConfig({ ...imapConfig, smtpUseSsl: e.target.checked })}
                    />
                  }
                  label="Use SSL/TLS"
                />
              </Grid>
              <Grid item xs={12}>
                <Typography variant="subtitle2" color="textSecondary" sx={{ mb: 1 }}>
                  Sync Options
                </Typography>
              </Grid>
              <Grid item xs={6}>
                <TextField
                  label="Sync Days Back"
                  type="number"
                  fullWidth
                  value={imapConfig.syncDaysBack}
                  onChange={(e) => setImapConfig({ ...imapConfig, syncDaysBack: Number(e.target.value) })}
                  inputProps={{ min: 1, max: 365 }}
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  label="Sync Interval (min)"
                  type="number"
                  fullWidth
                  value={imapConfig.syncIntervalMinutes}
                  onChange={(e) => setImapConfig({ ...imapConfig, syncIntervalMinutes: Number(e.target.value) })}
                  inputProps={{ min: 5, max: 1440 }}
                />
              </Grid>
            </Grid>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseImapDialog}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleSaveImapConfig}
            disabled={savingImap || !imapConfig.emailAddress || !imapConfig.imapHost || !imapConfig.smtpHost || !imapConfig.password}
          >
            {savingImap ? 'Adding...' : 'Add Account'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default EmailIntegrationTab;
