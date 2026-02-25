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
import { useSettings } from '../../contexts/SettingsContext';
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
  const { refresh: refreshGlobalSettings } = useSettings();

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
      await refreshGlobalSettings();
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
                  <MenuItem value="America/New_York">America/New_York (ET)</MenuItem>
                  <MenuItem value="America/Chicago">America/Chicago (CT)</MenuItem>
                  <MenuItem value="America/Denver">America/Denver (MT)</MenuItem>
                  <MenuItem value="America/Los_Angeles">America/Los_Angeles (PT)</MenuItem>
                  <MenuItem value="America/Phoenix">America/Phoenix (AZ)</MenuItem>
                  <MenuItem value="America/Anchorage">America/Anchorage (AK)</MenuItem>
                  <MenuItem value="America/Honolulu">America/Honolulu (HI)</MenuItem>
                  <MenuItem value="America/Toronto">America/Toronto (Canada ET)</MenuItem>
                  <MenuItem value="America/Vancouver">America/Vancouver (Canada PT)</MenuItem>
                  <MenuItem value="America/Mexico_City">America/Mexico_City</MenuItem>
                  <MenuItem value="America/Sao_Paulo">America/Sao_Paulo</MenuItem>
                  <MenuItem value="America/Buenos_Aires">America/Buenos_Aires</MenuItem>
                  <MenuItem value="Europe/London">Europe/London (GMT/BST)</MenuItem>
                  <MenuItem value="Europe/Dublin">Europe/Dublin</MenuItem>
                  <MenuItem value="Europe/Lisbon">Europe/Lisbon</MenuItem>
                  <MenuItem value="Europe/Paris">Europe/Paris (CET)</MenuItem>
                  <MenuItem value="Europe/Berlin">Europe/Berlin</MenuItem>
                  <MenuItem value="Europe/Amsterdam">Europe/Amsterdam</MenuItem>
                  <MenuItem value="Europe/Madrid">Europe/Madrid</MenuItem>
                  <MenuItem value="Europe/Rome">Europe/Rome</MenuItem>
                  <MenuItem value="Europe/Warsaw">Europe/Warsaw</MenuItem>
                  <MenuItem value="Europe/Stockholm">Europe/Stockholm</MenuItem>
                  <MenuItem value="Europe/Helsinki">Europe/Helsinki (EET)</MenuItem>
                  <MenuItem value="Europe/Athens">Europe/Athens</MenuItem>
                  <MenuItem value="Europe/Istanbul">Europe/Istanbul</MenuItem>
                  <MenuItem value="Europe/Moscow">Europe/Moscow</MenuItem>
                  <MenuItem value="Africa/Cairo">Africa/Cairo</MenuItem>
                  <MenuItem value="Africa/Johannesburg">Africa/Johannesburg</MenuItem>
                  <MenuItem value="Africa/Lagos">Africa/Lagos</MenuItem>
                  <MenuItem value="Asia/Dubai">Asia/Dubai (GST)</MenuItem>
                  <MenuItem value="Asia/Karachi">Asia/Karachi</MenuItem>
                  <MenuItem value="Asia/Kolkata">Asia/Kolkata (IST)</MenuItem>
                  <MenuItem value="Asia/Dhaka">Asia/Dhaka</MenuItem>
                  <MenuItem value="Asia/Bangkok">Asia/Bangkok</MenuItem>
                  <MenuItem value="Asia/Singapore">Asia/Singapore (SGT)</MenuItem>
                  <MenuItem value="Asia/Kuala_Lumpur">Asia/Kuala_Lumpur</MenuItem>
                  <MenuItem value="Asia/Hong_Kong">Asia/Hong_Kong</MenuItem>
                  <MenuItem value="Asia/Shanghai">Asia/Shanghai (CST)</MenuItem>
                  <MenuItem value="Asia/Taipei">Asia/Taipei</MenuItem>
                  <MenuItem value="Asia/Seoul">Asia/Seoul (KST)</MenuItem>
                  <MenuItem value="Asia/Tokyo">Asia/Tokyo (JST)</MenuItem>
                  <MenuItem value="Australia/Perth">Australia/Perth</MenuItem>
                  <MenuItem value="Australia/Darwin">Australia/Darwin</MenuItem>
                  <MenuItem value="Australia/Adelaide">Australia/Adelaide</MenuItem>
                  <MenuItem value="Australia/Sydney">Australia/Sydney (AEDT)</MenuItem>
                  <MenuItem value="Australia/Brisbane">Australia/Brisbane</MenuItem>
                  <MenuItem value="Pacific/Auckland">Pacific/Auckland (NZST)</MenuItem>
                  <MenuItem value="Pacific/Honolulu">Pacific/Honolulu (HST)</MenuItem>
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
                  <MenuItem value="USD">USD — US Dollar</MenuItem>
                  <MenuItem value="EUR">EUR — Euro</MenuItem>
                  <MenuItem value="GBP">GBP — British Pound</MenuItem>
                  <MenuItem value="JPY">JPY — Japanese Yen</MenuItem>
                  <MenuItem value="AUD">AUD — Australian Dollar</MenuItem>
                  <MenuItem value="CAD">CAD — Canadian Dollar</MenuItem>
                  <MenuItem value="CHF">CHF — Swiss Franc</MenuItem>
                  <MenuItem value="CNY">CNY — Chinese Yuan</MenuItem>
                  <MenuItem value="INR">INR — Indian Rupee</MenuItem>
                  <MenuItem value="BRL">BRL — Brazilian Real</MenuItem>
                  <MenuItem value="MXN">MXN — Mexican Peso</MenuItem>
                  <MenuItem value="SGD">SGD — Singapore Dollar</MenuItem>
                  <MenuItem value="HKD">HKD — Hong Kong Dollar</MenuItem>
                  <MenuItem value="NOK">NOK — Norwegian Krone</MenuItem>
                  <MenuItem value="SEK">SEK — Swedish Krona</MenuItem>
                  <MenuItem value="DKK">DKK — Danish Krone</MenuItem>
                  <MenuItem value="NZD">NZD — New Zealand Dollar</MenuItem>
                  <MenuItem value="ZAR">ZAR — South African Rand</MenuItem>
                  <MenuItem value="AED">AED — UAE Dirham</MenuItem>
                  <MenuItem value="SAR">SAR — Saudi Riyal</MenuItem>
                  <MenuItem value="KRW">KRW — South Korean Won</MenuItem>
                  <MenuItem value="TRY">TRY — Turkish Lira</MenuItem>
                  <MenuItem value="PLN">PLN — Polish Złoty</MenuItem>
                  <MenuItem value="THB">THB — Thai Baht</MenuItem>
                  <MenuItem value="IDR">IDR — Indonesian Rupiah</MenuItem>
                  <MenuItem value="MYR">MYR — Malaysian Ringgit</MenuItem>
                  <MenuItem value="PHP">PHP — Philippine Peso</MenuItem>
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
                  <MenuItem value="es-ES">Spanish (Spain)</MenuItem>
                  <MenuItem value="es-MX">Spanish (Mexico)</MenuItem>
                  <MenuItem value="fr-FR">French</MenuItem>
                  <MenuItem value="de-DE">German</MenuItem>
                  <MenuItem value="it-IT">Italian</MenuItem>
                  <MenuItem value="pt-BR">Portuguese (Brazil)</MenuItem>
                  <MenuItem value="pt-PT">Portuguese (Portugal)</MenuItem>
                  <MenuItem value="nl-NL">Dutch</MenuItem>
                  <MenuItem value="pl-PL">Polish</MenuItem>
                  <MenuItem value="ru-RU">Russian</MenuItem>
                  <MenuItem value="tr-TR">Turkish</MenuItem>
                  <MenuItem value="ar-SA">Arabic</MenuItem>
                  <MenuItem value="ja-JP">Japanese</MenuItem>
                  <MenuItem value="zh-CN">Chinese (Simplified)</MenuItem>
                  <MenuItem value="zh-TW">Chinese (Traditional)</MenuItem>
                  <MenuItem value="ko-KR">Korean</MenuItem>
                  <MenuItem value="hi-IN">Hindi</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth size="small">
                <InputLabel>Date Format</InputLabel>
                <Select
                  value={changes.dateFormat !== undefined ? changes.dateFormat : settings.dateFormat}
                  onChange={(e) => handleChange('dateFormat', e.target.value)}
                  label="Date Format"
                >
                  <MenuItem value="MM/dd/yyyy">MM/DD/YYYY (e.g. 02/24/2026)</MenuItem>
                  <MenuItem value="dd/MM/yyyy">DD/MM/YYYY (e.g. 24/02/2026)</MenuItem>
                  <MenuItem value="yyyy-MM-dd">YYYY-MM-DD (e.g. 2026-02-24)</MenuItem>
                  <MenuItem value="dd-MM-yyyy">DD-MM-YYYY (e.g. 24-02-2026)</MenuItem>
                  <MenuItem value="dd.MM.yyyy">DD.MM.YYYY (e.g. 24.02.2026)</MenuItem>
                  <MenuItem value="MMMM d, yyyy">Month D, YYYY (e.g. February 24, 2026)</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth size="small">
                <InputLabel>Time Format</InputLabel>
                <Select
                  value={changes.timeFormat !== undefined ? changes.timeFormat : (settings.timeFormat || '12h')}
                  onChange={(e) => handleChange('timeFormat', e.target.value)}
                  label="Time Format"
                >
                  <MenuItem value="12h">12-hour (e.g. 2:30 PM)</MenuItem>
                  <MenuItem value="24h">24-hour (e.g. 14:30)</MenuItem>
                </Select>
              </FormControl>
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
