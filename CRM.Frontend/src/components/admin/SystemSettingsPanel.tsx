import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  CardHeader,
  TextField,
  Switch,
  FormControlLabel,
  Button,
  Grid,
  Typography,
  Alert,
  CircularProgress,
  Divider,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
} from '@mui/material';
import { Save as SaveIcon, RestartAlt as ResetIcon } from '@mui/icons-material';
import settingsService from '../../services/settingsService';
import logger from '../../services/logger';

interface SystemSettings {
  id: number;
  organizationName: string;
  solutionName: string;
  smtpHost: string;
  smtpPort: number;
  smtpUsername: string;
  smtpUseSSL: boolean;
  defaultTimezone: string;
  defaultCurrency: string;
  defaultLanguage: string;
  dateFormat: string;
  timeFormat: string;
  logoUrl: string;
  faviconUrl: string;
  rateLimitPerMinute: number;
  accountsEnabled: boolean;
  contactsEnabled: boolean;
  leadsEnabled: boolean;
  opportunitiesEnabled: boolean;
  productsEnabled: boolean;
  campaignsEnabled: boolean;
  quoteEnabled: boolean;
}

/**
 * System Settings Panel - Manage all 21 system configuration options
 */
const SystemSettingsPanel: React.FC = () => {
  const [settings, setSettings] = useState<SystemSettings | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [changes, setChanges] = useState<Partial<SystemSettings>>({});

  useEffect(() => {
    loadSettings();
  }, []);

  const loadSettings = async () => {
    try {
      setLoading(true);
      const data = await settingsService.getSettings();
      setSettings(data);
      setError(null);
    } catch (err) {
      logger.error('Failed to load settings', err);
      setError('Failed to load settings. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (field: keyof SystemSettings, value: any) => {
    setChanges(prev => ({
      ...prev,
      [field]: value
    }));
    setSuccess(false);
  };

  const handleSave = async () => {
    try {
      setSaving(true);
      setError(null);
      await settingsService.updateSettings(changes);
      await loadSettings();
      setChanges({});
      setSuccess(true);
      setTimeout(() => setSuccess(false), 5000);
      logger.info('Settings saved successfully');
    } catch (err) {
      logger.error('Failed to save settings', err);
      setError('Failed to save settings. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  const handleReset = () => {
    setChanges({});
    setSuccess(false);
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 400 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!settings) {
    return (
      <Alert severity="error">
        Failed to load system settings. Please refresh and try again.
      </Alert>
    );
  }

  return (
    <Box>
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }}>Settings saved successfully!</Alert>}

      {/* Organization Settings */}
      <Card sx={{ mb: 3 }}>
        <CardHeader title="Organization" subtitle="Basic organization information" />
        <Divider />
        <CardContent>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Organization Name"
                value={changes.organizationName !== undefined ? changes.organizationName : settings.organizationName}
                onChange={(e) => handleChange('organizationName', e.target.value)}
                variant="outlined"
                size="small"
                helperText="Used throughout the CRM system"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Solution Name"
                value={changes.solutionName !== undefined ? changes.solutionName : settings.solutionName}
                onChange={(e) => handleChange('solutionName', e.target.value)}
                variant="outlined"
                size="small"
                helperText="Display name of the CRM solution"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Logo URL"
                value={changes.logoUrl !== undefined ? changes.logoUrl : settings.logoUrl}
                onChange={(e) => handleChange('logoUrl', e.target.value)}
                variant="outlined"
                size="small"
                helperText="URL to organization logo image"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Favicon URL"
                value={changes.faviconUrl !== undefined ? changes.faviconUrl : settings.faviconUrl}
                onChange={(e) => handleChange('faviconUrl', e.target.value)}
                variant="outlined"
                size="small"
                helperText="URL to favicon (16x16 or 32x32 pixels)"
              />
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Email Settings */}
      <Card sx={{ mb: 3 }}>
        <CardHeader title="Email Configuration" subtitle="SMTP and email delivery settings" />
        <Divider />
        <CardContent>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="SMTP Host"
                value={changes.smtpHost !== undefined ? changes.smtpHost : settings.smtpHost}
                onChange={(e) => handleChange('smtpHost', e.target.value)}
                variant="outlined"
                size="small"
                placeholder="smtp.gmail.com"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="SMTP Port"
                type="number"
                value={changes.smtpPort !== undefined ? changes.smtpPort : settings.smtpPort}
                onChange={(e) => handleChange('smtpPort', parseInt(e.target.value))}
                variant="outlined"
                size="small"
                placeholder="587"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="SMTP Username"
                value={changes.smtpUsername !== undefined ? changes.smtpUsername : settings.smtpUsername}
                onChange={(e) => handleChange('smtpUsername', e.target.value)}
                variant="outlined"
                size="small"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={changes.smtpUseSSL !== undefined ? changes.smtpUseSSL : settings.smtpUseSSL}
                    onChange={(e) => handleChange('smtpUseSSL', e.target.checked)}
                  />
                }
                label="Use SSL/TLS"
              />
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Localization Settings */}
      <Card sx={{ mb: 3 }}>
        <CardHeader title="Localization" subtitle="Regional settings and formats" />
        <Divider />
        <CardContent>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth size="small">
                <InputLabel>Default Timezone</InputLabel>
                <Select
                  value={changes.defaultTimezone !== undefined ? changes.defaultTimezone : settings.defaultTimezone}
                  onChange={(e) => handleChange('defaultTimezone', e.target.value)}
                  label="Default Timezone"
                >
                  <MenuItem value="UTC">UTC</MenuItem>
                  <MenuItem value="America/New_York">America/New_York</MenuItem>
                  <MenuItem value="America/Chicago">America/Chicago</MenuItem>
                  <MenuItem value="America/Denver">America/Denver</MenuItem>
                  <MenuItem value="America/Los_Angeles">America/Los_Angeles</MenuItem>
                  <MenuItem value="Europe/London">Europe/London</MenuItem>
                  <MenuItem value="Europe/Paris">Europe/Paris</MenuItem>
                  <MenuItem value="Asia/Tokyo">Asia/Tokyo</MenuItem>
                  <MenuItem value="Asia/Shanghai">Asia/Shanghai</MenuItem>
                  <MenuItem value="Australia/Sydney">Australia/Sydney</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth size="small">
                <InputLabel>Default Currency</InputLabel>
                <Select
                  value={changes.defaultCurrency !== undefined ? changes.defaultCurrency : settings.defaultCurrency}
                  onChange={(e) => handleChange('defaultCurrency', e.target.value)}
                  label="Default Currency"
                >
                  <MenuItem value="USD">USD - US Dollar</MenuItem>
                  <MenuItem value="EUR">EUR - Euro</MenuItem>
                  <MenuItem value="GBP">GBP - British Pound</MenuItem>
                  <MenuItem value="JPY">JPY - Japanese Yen</MenuItem>
                  <MenuItem value="AUD">AUD - Australian Dollar</MenuItem>
                  <MenuItem value="CAD">CAD - Canadian Dollar</MenuItem>
                  <MenuItem value="CHF">CHF - Swiss Franc</MenuItem>
                  <MenuItem value="CNY">CNY - Chinese Yuan</MenuItem>
                  <MenuItem value="INR">INR - Indian Rupee</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth size="small">
                <InputLabel>Default Language</InputLabel>
                <Select
                  value={changes.defaultLanguage !== undefined ? changes.defaultLanguage : settings.defaultLanguage}
                  onChange={(e) => handleChange('defaultLanguage', e.target.value)}
                  label="Default Language"
                >
                  <MenuItem value="en-US">English (US)</MenuItem>
                  <MenuItem value="en-GB">English (UK)</MenuItem>
                  <MenuItem value="es-ES">Spanish</MenuItem>
                  <MenuItem value="fr-FR">French</MenuItem>
                  <MenuItem value="de-DE">German</MenuItem>
                  <MenuItem value="ja-JP">Japanese</MenuItem>
                  <MenuItem value="zh-CN">Chinese (Simplified)</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Date Format"
                value={changes.dateFormat !== undefined ? changes.dateFormat : settings.dateFormat}
                onChange={(e) => handleChange('dateFormat', e.target.value)}
                variant="outlined"
                size="small"
                helperText="e.g., MM/DD/YYYY"
              />
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Module Settings */}
      <Card sx={{ mb: 3 }}>
        <CardHeader title="Modules" subtitle="Enable or disable CRM modules" />
        <Divider />
        <CardContent>
          <Grid container spacing={2}>
            {[
              { field: 'accountsEnabled', label: 'Accounts' },
              { field: 'contactsEnabled', label: 'Contacts' },
              { field: 'leadsEnabled', label: 'Leads' },
              { field: 'opportunitiesEnabled', label: 'Opportunities' },
              { field: 'productsEnabled', label: 'Products' },
              { field: 'campaignsEnabled', label: 'Campaigns' },
            ].map(({ field, label }) => (
              <Grid item xs={12} sm={6} key={field}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={Boolean(
                        changes[field as keyof SystemSettings] !== undefined
                          ? changes[field as keyof SystemSettings]
                          : settings[field as keyof SystemSettings]
                      )}
                      onChange={(e) => handleChange(field as keyof SystemSettings, e.target.checked)}
                    />
                  }
                  label={label}
                />
              </Grid>
            ))}
          </Grid>
        </CardContent>
      </Card>

      {/* API Settings */}
      <Card sx={{ mb: 3 }}>
        <CardHeader title="API Settings" subtitle="API throttling and rate limits" />
        <Divider />
        <CardContent>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Rate Limit per Minute"
                type="number"
                value={changes.rateLimitPerMinute !== undefined ? changes.rateLimitPerMinute : settings.rateLimitPerMinute}
                onChange={(e) => handleChange('rateLimitPerMinute', parseInt(e.target.value))}
                variant="outlined"
                size="small"
                helperText="Maximum API requests per minute per user"
              />
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Action Buttons */}
      <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end' }}>
        <Button
          variant="outlined"
          startIcon={<ResetIcon />}
          onClick={handleReset}
          disabled={Object.keys(changes).length === 0}
        >
          Reset Changes
        </Button>
        <Button
          variant="contained"
          startIcon={<SaveIcon />}
          onClick={handleSave}
          disabled={Object.keys(changes).length === 0 || saving}
        >
          {saving ? 'Saving...' : 'Save Changes'}
        </Button>
      </Box>
    </Box>
  );
};

export default SystemSettingsPanel;
export { SystemSettingsPanel };
