import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Switch,
  TextField,
  Button,
  Grid,
  Alert,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  CircularProgress,
  Divider,
  Link,
  IconButton,
  InputAdornment,
  Chip,
} from '@mui/material';
import {
  ExpandMore as ExpandMoreIcon,
  Google as GoogleIcon,
  Visibility,
  VisibilityOff,
  Info as InfoIcon,
  Launch as LaunchIcon,
} from '@mui/icons-material';
import { FaMicrosoft, FaLinkedin, FaFacebook } from 'react-icons/fa';
import { SiMicrosoftazure } from 'react-icons/si';

interface SocialLoginSettings {
  // Google
  googleAuthEnabled: boolean;
  googleClientId: string;
  googleClientSecret: string;
  // Microsoft
  microsoftAuthEnabled: boolean;
  microsoftClientId: string;
  microsoftClientSecret: string;
  microsoftTenantId: string;
  // Azure AD
  azureAdAuthEnabled: boolean;
  azureAdClientId: string;
  azureAdClientSecret: string;
  azureAdTenantId: string;
  azureAdAuthority: string;
  // LinkedIn
  linkedInAuthEnabled: boolean;
  linkedInClientId: string;
  linkedInClientSecret: string;
  // Facebook
  facebookAuthEnabled: boolean;
  facebookAppId: string;
  facebookAppSecret: string;
}

const defaultSettings: SocialLoginSettings = {
  googleAuthEnabled: false,
  googleClientId: '',
  googleClientSecret: '',
  microsoftAuthEnabled: false,
  microsoftClientId: '',
  microsoftClientSecret: '',
  microsoftTenantId: 'common',
  azureAdAuthEnabled: false,
  azureAdClientId: '',
  azureAdClientSecret: '',
  azureAdTenantId: '',
  azureAdAuthority: '',
  linkedInAuthEnabled: false,
  linkedInClientId: '',
  linkedInClientSecret: '',
  facebookAuthEnabled: false,
  facebookAppId: '',
  facebookAppSecret: '',
};

function SocialLoginSettingsTab() {
  const [settings, setSettings] = useState<SocialLoginSettings>(defaultSettings);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showSecrets, setShowSecrets] = useState<{ [key: string]: boolean }>({});
  const [expandedProvider, setExpandedProvider] = useState<string | false>(false);

  const getApiUrl = () => {
    return window.location.hostname === 'localhost'
      ? 'http://localhost:5001/api'
      : `http://${window.location.hostname}:5001/api`;
  };

  useEffect(() => {
    const loadSettings = async () => {
      try {
        const token = localStorage.getItem('accessToken');
        const response = await fetch(`${getApiUrl()}/systemsettings`, {
          headers: { Authorization: `Bearer ${token}` },
        });

        if (response.ok) {
          const data = await response.json();
          setSettings({
            googleAuthEnabled: data.googleAuthEnabled || false,
            googleClientId: data.googleClientId || '',
            googleClientSecret: data.googleClientSecret || '',
            microsoftAuthEnabled: data.microsoftAuthEnabled || false,
            microsoftClientId: data.microsoftClientId || '',
            microsoftClientSecret: data.microsoftClientSecret || '',
            microsoftTenantId: data.microsoftTenantId || 'common',
            azureAdAuthEnabled: data.azureAdAuthEnabled || false,
            azureAdClientId: data.azureAdClientId || '',
            azureAdClientSecret: data.azureAdClientSecret || '',
            azureAdTenantId: data.azureAdTenantId || '',
            azureAdAuthority: data.azureAdAuthority || '',
            linkedInAuthEnabled: data.linkedInAuthEnabled || false,
            linkedInClientId: data.linkedInClientId || '',
            linkedInClientSecret: data.linkedInClientSecret || '',
            facebookAuthEnabled: data.facebookAuthEnabled || false,
            facebookAppId: data.facebookAppId || '',
            facebookAppSecret: data.facebookAppSecret || '',
          });
        }
      } catch (err) {
        console.error('Error loading social login settings:', err);
        setError('Failed to load social login settings');
      } finally {
        setLoading(false);
      }
    };
    loadSettings();
  }, []);

  const handleSave = async () => {
    setSaving(true);
    setError(null);
    try {
      const token = localStorage.getItem('accessToken');
      const response = await fetch(`${getApiUrl()}/systemsettings`, {
        method: 'PUT',
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(settings),
      });

      if (response.ok) {
        setSaved(true);
        setTimeout(() => setSaved(false), 3000);
      } else {
        setError('Failed to save social login settings');
      }
    } catch (err) {
      console.error('Error saving social login settings:', err);
      setError('Failed to save social login settings');
    } finally {
      setSaving(false);
    }
  };

  const toggleSecretVisibility = (field: string) => {
    setShowSecrets((prev) => ({ ...prev, [field]: !prev[field] }));
  };

  const handleAccordionChange = (panel: string) => (event: React.SyntheticEvent, isExpanded: boolean) => {
    setExpandedProvider(isExpanded ? panel : false);
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  const SecretField = ({
    label,
    value,
    fieldKey,
    onChange,
    placeholder,
  }: {
    label: string;
    value: string;
    fieldKey: string;
    onChange: (value: string) => void;
    placeholder?: string;
  }) => (
    <TextField
      fullWidth
      label={label}
      value={value}
      onChange={(e) => onChange(e.target.value)}
      placeholder={placeholder}
      type={showSecrets[fieldKey] ? 'text' : 'password'}
      InputProps={{
        endAdornment: (
          <InputAdornment position="end">
            <IconButton onClick={() => toggleSecretVisibility(fieldKey)} edge="end">
              {showSecrets[fieldKey] ? <VisibilityOff /> : <Visibility />}
            </IconButton>
          </InputAdornment>
        ),
      }}
      sx={{ mt: 2 }}
    />
  );

  return (
    <Box>
      <Typography variant="h6" sx={{ fontWeight: 600, mb: 1 }}>
        Social Login Configuration
      </Typography>
      <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
        Configure OAuth providers to allow users to sign in with their social accounts.
        Click on each provider to expand setup instructions.
      </Typography>

      {saved && (
        <Alert severity="success" sx={{ mb: 2, borderRadius: 2 }}>
          Social login settings saved successfully!
        </Alert>
      )}

      {error && (
        <Alert severity="error" sx={{ mb: 2, borderRadius: 2 }}>
          {error}
        </Alert>
      )}

      {/* Google OAuth */}
      <Accordion
        expanded={expandedProvider === 'google'}
        onChange={handleAccordionChange('google')}
        sx={{ mb: 2, borderRadius: 2, '&:before': { display: 'none' } }}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, width: '100%' }}>
            <GoogleIcon sx={{ color: '#4285F4', fontSize: 28 }} />
            <Box sx={{ flexGrow: 1 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                Google
              </Typography>
              <Typography variant="caption" color="textSecondary">
                Sign in with Google accounts
              </Typography>
            </Box>
            <Switch
              checked={settings.googleAuthEnabled}
              onChange={(e) => {
                e.stopPropagation();
                setSettings({ ...settings, googleAuthEnabled: e.target.checked });
              }}
              onClick={(e) => e.stopPropagation()}
            />
            <Chip
              size="small"
              label={settings.googleAuthEnabled ? 'Enabled' : 'Disabled'}
              color={settings.googleAuthEnabled ? 'success' : 'default'}
            />
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <Alert severity="info" icon={<InfoIcon />} sx={{ mb: 3 }}>
            <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
              Setup Instructions for Google OAuth:
            </Typography>
            <ol style={{ margin: 0, paddingLeft: 20 }}>
              <li>
                Go to the{' '}
                <Link href="https://console.cloud.google.com/apis/credentials" target="_blank" rel="noopener">
                  Google Cloud Console - Credentials <LaunchIcon sx={{ fontSize: 14, ml: 0.5 }} />
                </Link>
              </li>
              <li>Create a new project or select an existing one</li>
              <li>Click "Create Credentials" → "OAuth client ID"</li>
              <li>Select "Web application" as the application type</li>
              <li>Add your app's URL to "Authorized JavaScript origins" (e.g., http://localhost:3000)</li>
              <li>
                Add callback URL to "Authorized redirect URIs":
                <code style={{ display: 'block', marginTop: 4, backgroundColor: '#f5f5f5', padding: 8, borderRadius: 4 }}>
                  {window.location.origin}/auth/google/callback
                </code>
              </li>
              <li>Copy the Client ID and Client Secret below</li>
            </ol>
          </Alert>

          <Grid container spacing={2}>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Google Client ID"
                value={settings.googleClientId}
                onChange={(e) => setSettings({ ...settings, googleClientId: e.target.value })}
                placeholder="xxxxxxxxxxxx-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.apps.googleusercontent.com"
                disabled={!settings.googleAuthEnabled}
              />
            </Grid>
            <Grid item xs={12}>
              <SecretField
                label="Google Client Secret"
                value={settings.googleClientSecret}
                fieldKey="googleClientSecret"
                onChange={(value) => setSettings({ ...settings, googleClientSecret: value })}
                placeholder="GOCSPX-xxxxxxxxxxxxxxxxxxxxxxxxx"
              />
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {/* Microsoft Account */}
      <Accordion
        expanded={expandedProvider === 'microsoft'}
        onChange={handleAccordionChange('microsoft')}
        sx={{ mb: 2, borderRadius: 2, '&:before': { display: 'none' } }}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, width: '100%' }}>
            <FaMicrosoft style={{ color: '#00A4EF', fontSize: 24 }} />
            <Box sx={{ flexGrow: 1 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                Microsoft Account (Live ID)
              </Typography>
              <Typography variant="caption" color="textSecondary">
                Sign in with personal Microsoft accounts (@outlook.com, @hotmail.com, @live.com)
              </Typography>
            </Box>
            <Switch
              checked={settings.microsoftAuthEnabled}
              onChange={(e) => {
                e.stopPropagation();
                setSettings({ ...settings, microsoftAuthEnabled: e.target.checked });
              }}
              onClick={(e) => e.stopPropagation()}
            />
            <Chip
              size="small"
              label={settings.microsoftAuthEnabled ? 'Enabled' : 'Disabled'}
              color={settings.microsoftAuthEnabled ? 'success' : 'default'}
            />
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <Alert severity="info" icon={<InfoIcon />} sx={{ mb: 3 }}>
            <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
              Setup Instructions for Microsoft Account Login:
            </Typography>
            <ol style={{ margin: 0, paddingLeft: 20 }}>
              <li>
                Go to the{' '}
                <Link href="https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationsListBlade" target="_blank" rel="noopener">
                  Azure Portal - App Registrations <LaunchIcon sx={{ fontSize: 14, ml: 0.5 }} />
                </Link>
              </li>
              <li>Click "New registration"</li>
              <li>Enter a name for your application</li>
              <li>
                Select <strong>"Accounts in any organizational directory and personal Microsoft accounts"</strong>
              </li>
              <li>
                Add Redirect URI as "Web" with:
                <code style={{ display: 'block', marginTop: 4, backgroundColor: '#f5f5f5', padding: 8, borderRadius: 4 }}>
                  {window.location.origin}/auth/microsoft/callback
                </code>
              </li>
              <li>Copy the "Application (client) ID" from the Overview page</li>
              <li>Go to "Certificates & secrets" → "New client secret"</li>
              <li>Copy the secret value immediately (it won't be shown again)</li>
              <li>Set Tenant ID to "common" for all account types, "consumers" for personal accounts only</li>
            </ol>
          </Alert>

          <Grid container spacing={2}>
            <Grid item xs={12} md={8}>
              <TextField
                fullWidth
                label="Microsoft Client ID (Application ID)"
                value={settings.microsoftClientId}
                onChange={(e) => setSettings({ ...settings, microsoftClientId: e.target.value })}
                placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                disabled={!settings.microsoftAuthEnabled}
              />
            </Grid>
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                label="Tenant ID"
                value={settings.microsoftTenantId}
                onChange={(e) => setSettings({ ...settings, microsoftTenantId: e.target.value })}
                placeholder="common"
                helperText="Use 'common' for all accounts, 'consumers' for personal only"
                disabled={!settings.microsoftAuthEnabled}
              />
            </Grid>
            <Grid item xs={12}>
              <SecretField
                label="Microsoft Client Secret"
                value={settings.microsoftClientSecret}
                fieldKey="microsoftClientSecret"
                onChange={(value) => setSettings({ ...settings, microsoftClientSecret: value })}
                placeholder="xxxxxxxx~xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
              />
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {/* Azure Active Directory */}
      <Accordion
        expanded={expandedProvider === 'azuread'}
        onChange={handleAccordionChange('azuread')}
        sx={{ mb: 2, borderRadius: 2, '&:before': { display: 'none' } }}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, width: '100%' }}>
            <SiMicrosoftazure style={{ color: '#0078D4', fontSize: 24 }} />
            <Box sx={{ flexGrow: 1 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                Azure Active Directory (Entra ID)
              </Typography>
              <Typography variant="caption" color="textSecondary">
                Sign in with organizational Azure AD accounts (Enterprise SSO)
              </Typography>
            </Box>
            <Switch
              checked={settings.azureAdAuthEnabled}
              onChange={(e) => {
                e.stopPropagation();
                setSettings({ ...settings, azureAdAuthEnabled: e.target.checked });
              }}
              onClick={(e) => e.stopPropagation()}
            />
            <Chip
              size="small"
              label={settings.azureAdAuthEnabled ? 'Enabled' : 'Disabled'}
              color={settings.azureAdAuthEnabled ? 'success' : 'default'}
            />
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <Alert severity="info" icon={<InfoIcon />} sx={{ mb: 3 }}>
            <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
              Setup Instructions for Azure AD (Enterprise):
            </Typography>
            <ol style={{ margin: 0, paddingLeft: 20 }}>
              <li>
                Go to the{' '}
                <Link href="https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationsListBlade" target="_blank" rel="noopener">
                  Azure Portal - App Registrations <LaunchIcon sx={{ fontSize: 14, ml: 0.5 }} />
                </Link>
              </li>
              <li>Click "New registration"</li>
              <li>Enter a name for your application</li>
              <li>
                Select <strong>"Accounts in this organizational directory only"</strong> (Single tenant) or{' '}
                <strong>"Accounts in any organizational directory"</strong> (Multi-tenant)
              </li>
              <li>
                Add Redirect URI as "Web" with:
                <code style={{ display: 'block', marginTop: 4, backgroundColor: '#f5f5f5', padding: 8, borderRadius: 4 }}>
                  {window.location.origin}/auth/azuread/callback
                </code>
              </li>
              <li>Copy the "Application (client) ID" from the Overview page</li>
              <li>Copy the "Directory (tenant) ID" from the Overview page</li>
              <li>Go to "Certificates & secrets" → "New client secret"</li>
              <li>Copy the secret value immediately</li>
              <li>
                Under "API permissions", ensure you have: <code>openid</code>, <code>profile</code>, <code>email</code>, <code>User.Read</code>
              </li>
            </ol>
          </Alert>

          <Grid container spacing={2}>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Azure AD Client ID (Application ID)"
                value={settings.azureAdClientId}
                onChange={(e) => setSettings({ ...settings, azureAdClientId: e.target.value })}
                placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                disabled={!settings.azureAdAuthEnabled}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Azure AD Tenant ID (Directory ID)"
                value={settings.azureAdTenantId}
                onChange={(e) => setSettings({ ...settings, azureAdTenantId: e.target.value })}
                placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                disabled={!settings.azureAdAuthEnabled}
              />
            </Grid>
            <Grid item xs={12}>
              <SecretField
                label="Azure AD Client Secret"
                value={settings.azureAdClientSecret}
                fieldKey="azureAdClientSecret"
                onChange={(value) => setSettings({ ...settings, azureAdClientSecret: value })}
                placeholder="xxxxxxxx~xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Authority URL (Optional)"
                value={settings.azureAdAuthority}
                onChange={(e) => setSettings({ ...settings, azureAdAuthority: e.target.value })}
                placeholder="https://login.microsoftonline.com/{tenant-id}"
                helperText="Leave blank to use default. For B2C, use your custom authority URL."
                disabled={!settings.azureAdAuthEnabled}
              />
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {/* LinkedIn */}
      <Accordion
        expanded={expandedProvider === 'linkedin'}
        onChange={handleAccordionChange('linkedin')}
        sx={{ mb: 2, borderRadius: 2, '&:before': { display: 'none' } }}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, width: '100%' }}>
            <FaLinkedin style={{ color: '#0A66C2', fontSize: 24 }} />
            <Box sx={{ flexGrow: 1 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                LinkedIn
              </Typography>
              <Typography variant="caption" color="textSecondary">
                Sign in with LinkedIn professional accounts
              </Typography>
            </Box>
            <Switch
              checked={settings.linkedInAuthEnabled}
              onChange={(e) => {
                e.stopPropagation();
                setSettings({ ...settings, linkedInAuthEnabled: e.target.checked });
              }}
              onClick={(e) => e.stopPropagation()}
            />
            <Chip
              size="small"
              label={settings.linkedInAuthEnabled ? 'Enabled' : 'Disabled'}
              color={settings.linkedInAuthEnabled ? 'success' : 'default'}
            />
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <Alert severity="info" icon={<InfoIcon />} sx={{ mb: 3 }}>
            <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
              Setup Instructions for LinkedIn OAuth:
            </Typography>
            <ol style={{ margin: 0, paddingLeft: 20 }}>
              <li>
                Go to the{' '}
                <Link href="https://www.linkedin.com/developers/apps" target="_blank" rel="noopener">
                  LinkedIn Developers Portal <LaunchIcon sx={{ fontSize: 14, ml: 0.5 }} />
                </Link>
              </li>
              <li>Click "Create app"</li>
              <li>Fill in the app details (name, LinkedIn page, logo)</li>
              <li>Once created, go to the "Auth" tab</li>
              <li>
                Add OAuth 2.0 Authorized Redirect URL:
                <code style={{ display: 'block', marginTop: 4, backgroundColor: '#f5f5f5', padding: 8, borderRadius: 4 }}>
                  {window.location.origin}/auth/linkedin/callback
                </code>
              </li>
              <li>Copy the Client ID and Client Secret from the Auth tab</li>
              <li>
                Under "Products", request access to:
                <ul style={{ marginTop: 4 }}>
                  <li><strong>Sign In with LinkedIn using OpenID Connect</strong> (required)</li>
                  <li>Share on LinkedIn (optional)</li>
                </ul>
              </li>
              <li>Wait for product access approval (usually instant for Sign In)</li>
            </ol>
          </Alert>

          <Grid container spacing={2}>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="LinkedIn Client ID"
                value={settings.linkedInClientId}
                onChange={(e) => setSettings({ ...settings, linkedInClientId: e.target.value })}
                placeholder="xxxxxxxxxxxx"
                disabled={!settings.linkedInAuthEnabled}
              />
            </Grid>
            <Grid item xs={12}>
              <SecretField
                label="LinkedIn Client Secret"
                value={settings.linkedInClientSecret}
                fieldKey="linkedInClientSecret"
                onChange={(value) => setSettings({ ...settings, linkedInClientSecret: value })}
                placeholder="xxxxxxxxxxxxxxxx"
              />
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {/* Facebook */}
      <Accordion
        expanded={expandedProvider === 'facebook'}
        onChange={handleAccordionChange('facebook')}
        sx={{ mb: 2, borderRadius: 2, '&:before': { display: 'none' } }}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, width: '100%' }}>
            <FaFacebook style={{ color: '#1877F2', fontSize: 24 }} />
            <Box sx={{ flexGrow: 1 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                Facebook
              </Typography>
              <Typography variant="caption" color="textSecondary">
                Sign in with Facebook accounts
              </Typography>
            </Box>
            <Switch
              checked={settings.facebookAuthEnabled}
              onChange={(e) => {
                e.stopPropagation();
                setSettings({ ...settings, facebookAuthEnabled: e.target.checked });
              }}
              onClick={(e) => e.stopPropagation()}
            />
            <Chip
              size="small"
              label={settings.facebookAuthEnabled ? 'Enabled' : 'Disabled'}
              color={settings.facebookAuthEnabled ? 'success' : 'default'}
            />
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          <Alert severity="info" icon={<InfoIcon />} sx={{ mb: 3 }}>
            <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
              Setup Instructions for Facebook Login:
            </Typography>
            <ol style={{ margin: 0, paddingLeft: 20 }}>
              <li>
                Go to{' '}
                <Link href="https://developers.facebook.com/apps/" target="_blank" rel="noopener">
                  Facebook Developers <LaunchIcon sx={{ fontSize: 14, ml: 0.5 }} />
                </Link>
              </li>
              <li>Click "Create App" → Select "Consumer" or "Business"</li>
              <li>Enter your app name and contact email</li>
              <li>Once created, go to "Add Products" and set up "Facebook Login"</li>
              <li>Click "Settings" under Facebook Login in the left menu</li>
              <li>
                Add to "Valid OAuth Redirect URIs":
                <code style={{ display: 'block', marginTop: 4, backgroundColor: '#f5f5f5', padding: 8, borderRadius: 4 }}>
                  {window.location.origin}/auth/facebook/callback
                </code>
              </li>
              <li>Go to "Settings" → "Basic" in the left menu</li>
              <li>Copy the App ID and App Secret</li>
              <li>
                <strong>Important:</strong> To go live, you need to:
                <ul style={{ marginTop: 4 }}>
                  <li>Add a Privacy Policy URL</li>
                  <li>Complete Data Use Checkup</li>
                  <li>Set App Mode to "Live" (top of dashboard)</li>
                </ul>
              </li>
            </ol>
          </Alert>

          <Grid container spacing={2}>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Facebook App ID"
                value={settings.facebookAppId}
                onChange={(e) => setSettings({ ...settings, facebookAppId: e.target.value })}
                placeholder="xxxxxxxxxxxxxxxxxxxx"
                disabled={!settings.facebookAuthEnabled}
              />
            </Grid>
            <Grid item xs={12}>
              <SecretField
                label="Facebook App Secret"
                value={settings.facebookAppSecret}
                fieldKey="facebookAppSecret"
                onChange={(value) => setSettings({ ...settings, facebookAppSecret: value })}
                placeholder="xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
              />
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      <Divider sx={{ my: 3 }} />

      <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2 }}>
        <Button
          variant="contained"
          onClick={handleSave}
          disabled={saving}
          sx={{
            textTransform: 'none',
            px: 4,
            py: 1,
            backgroundColor: '#6750A4',
            '&:hover': { backgroundColor: '#5940A2' },
          }}
        >
          {saving ? <CircularProgress size={20} color="inherit" /> : 'Save Social Login Settings'}
        </Button>
      </Box>
    </Box>
  );
}

export default SocialLoginSettingsTab;
