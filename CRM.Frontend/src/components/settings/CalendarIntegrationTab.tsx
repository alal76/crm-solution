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
} from '@mui/material';
import {
  CalendarMonth as CalendarIcon,
  Sync as SyncIcon,
  Link as LinkIcon,
  LinkOff as LinkOffIcon,
} from '@mui/icons-material';
import apiClient from '../../services/apiClient';

interface CalendarIntegrationDto {
  id: number;
  provider: string;
  externalEmail?: string;
  calendarName?: string;
  syncDirection: string;
  syncIntervalMinutes: number;
  isActive: boolean;
  lastSyncAt?: string;
  lastSyncStatus?: string;
  lastSyncError?: string;
  nextSyncAt?: string;
  totalEventsSynced: number;
}

const syncDirectionOptions = ['Import', 'Export', 'Bidirectional'];

function CalendarIntegrationTab() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [integrations, setIntegrations] = useState<CalendarIntegrationDto[]>([]);
  const [savingId, setSavingId] = useState<number | null>(null);
  const [syncingId, setSyncingId] = useState<number | null>(null);
  const [disconnectingId, setDisconnectingId] = useState<number | null>(null);

  useEffect(() => {
    loadIntegrations();
  }, []);

  const loadIntegrations = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await apiClient.get('/calendar/integrations');
      setIntegrations(response.data || []);
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || 'Failed to load calendar integrations');
    } finally {
      setLoading(false);
    }
  };

  const handleConnect = async (provider: 'google' | 'outlook') => {
    try {
      const response = await apiClient.get(`/calendar/connect/${provider}`);
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

  const handleUpdateSettings = async (integration: CalendarIntegrationDto) => {
    try {
      setSavingId(integration.id);
      await apiClient.put(`/calendar/integrations/${integration.id}`, {
        syncDirection: integration.syncDirection,
        syncIntervalMinutes: integration.syncIntervalMinutes,
      });
      await loadIntegrations();
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || 'Failed to update settings');
    } finally {
      setSavingId(null);
    }
  };

  const handleSyncNow = async (integration: CalendarIntegrationDto) => {
    try {
      setSyncingId(integration.id);
      await apiClient.post(`/calendar/sync/${integration.provider.toLowerCase()}`);
      await loadIntegrations();
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || 'Failed to sync calendar');
    } finally {
      setSyncingId(null);
    }
  };

  const handleDisconnect = async (integration: CalendarIntegrationDto) => {
    try {
      setDisconnectingId(integration.id);
      await apiClient.delete(`/calendar/integrations/${integration.provider.toLowerCase()}`);
      await loadIntegrations();
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || 'Failed to disconnect integration');
    } finally {
      setDisconnectingId(null);
    }
  };

  const updateIntegrationField = <K extends keyof CalendarIntegrationDto>(id: number, field: K, value: CalendarIntegrationDto[K]) => {
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
        Calendar Integrations
      </Typography>
      <Typography variant="body2" color="textSecondary" gutterBottom>
        Connect Google or Outlook calendars to sync meetings and activities.
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ mb: 3 }}>
        <Button
          variant="contained"
          startIcon={<LinkIcon />}
          onClick={() => handleConnect('google')}
        >
          Connect Google Calendar
        </Button>
        <Button
          variant="contained"
          color="secondary"
          startIcon={<LinkIcon />}
          onClick={() => handleConnect('outlook')}
        >
          Connect Outlook Calendar
        </Button>
      </Stack>

      <Grid container spacing={2}>
        {integrations.length === 0 && (
          <Grid item xs={12}>
            <Card variant="outlined">
              <CardContent>
                <Typography variant="body1">No calendar integrations connected yet.</Typography>
              </CardContent>
            </Card>
          </Grid>
        )}

        {integrations.map(integration => (
          <Grid item xs={12} md={6} key={integration.id}>
            <Card variant="outlined">
              <CardContent>
                <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
                  <CalendarIcon color="primary" />
                  <Typography variant="h6">{integration.provider} Calendar</Typography>
                  <Chip
                    label={integration.isActive ? 'Active' : 'Inactive'}
                    color={integration.isActive ? 'success' : 'default'}
                    size="small"
                  />
                </Stack>

                <Typography variant="body2" color="textSecondary">
                  Account: {integration.externalEmail || 'Unknown'}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Calendar: {integration.calendarName || 'Primary'}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Last Sync: {integration.lastSyncAt ? new Date(integration.lastSyncAt).toLocaleString() : 'Never'}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Next Sync: {integration.nextSyncAt ? new Date(integration.nextSyncAt).toLocaleString() : 'Pending'}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Total Events Synced: {integration.totalEventsSynced}
                </Typography>

                {integration.lastSyncError && (
                  <Alert severity="warning" sx={{ mt: 1 }}>
                    {integration.lastSyncError}
                  </Alert>
                )}

                <Box sx={{ mt: 2 }}>
                  <Grid container spacing={2}>
                    <Grid item xs={12} sm={6}>
                      <FormControl fullWidth size="small">
                        <InputLabel>Sync Direction</InputLabel>
                        <Select
                          label="Sync Direction"
                          value={integration.syncDirection}
                          onChange={(e) => updateIntegrationField(integration.id, 'syncDirection', e.target.value)}
                        >
                          {syncDirectionOptions.map(option => (
                            <MenuItem key={option} value={option}>
                              {option}
                            </MenuItem>
                          ))}
                        </Select>
                      </FormControl>
                    </Grid>
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
    </Box>
  );
}

export default CalendarIntegrationTab;
