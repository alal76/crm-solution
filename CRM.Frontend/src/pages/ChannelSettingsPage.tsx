/**
 * CRM Solution - Channel Settings Page
 * Configure communication channels: Email (SMTP/IMAP/OAuth), Social Media (Twitter, Facebook, Instagram, LinkedIn),
 * Web Form ingestion settings, WhatsApp Business, and email templates/signatures
 */
import React, { useState, useEffect } from 'react';
import { TabPanel } from '../components/common';
import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  CardActions,
  CardHeader,
  Grid,
  Tabs,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Button,
  IconButton,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  CircularProgress,
  Alert,
  Avatar,
  Tooltip,
  FormControlLabel,
  Switch,
  Divider,
  Stack,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  InputAdornment,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  ListItemSecondaryAction,
} from '@mui/material';
import {
  Email as EmailIcon,
  WhatsApp as WhatsAppIcon,
  Twitter as TwitterIcon,
  Facebook as FacebookIcon,
  LinkedIn as LinkedInIcon,
  Instagram as InstagramIcon,
  Web as WebFormIcon,
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Refresh as RefreshIcon,
  Settings as SettingsIcon,
  Save as SaveIcon,
  Check as CheckIcon,
  Close as CloseIcon,
  ExpandMore as ExpandMoreIcon,
  Visibility as VisibilityIcon,
  VisibilityOff as VisibilityOffIcon,
  ContentCopy as CopyIcon,
  Link as LinkIcon,
  Security as SecurityIcon,
  VpnKey as ApiKeyIcon,
  Verified as VerifiedIcon,
  Error as ErrorIcon,
  Schedule as ScheduleIcon,
  Send as SendIcon,
  TextFields as TemplateIcon,
  FormatQuote as SignatureIcon,
  Description as SalutationIcon,
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';

// Types
interface Channel {
  id: number;
  channelType: string;
  name: string;
  status: string;
  isEnabled: boolean;
  isDefault: boolean;
  // Email settings
  smtpHost?: string;
  smtpPort?: number;
  smtpUsername?: string;
  smtpPassword?: string;
  smtpUseSsl?: boolean;
  imapHost?: string;
  imapPort?: number;
  imapUsername?: string;
  imapUseSsl?: boolean;
  fromEmail?: string;
  fromName?: string;
  replyToEmail?: string;
  // OAuth
  oauthProvider?: string;
  oauthClientId?: string;
  oauthTenantId?: string;
  oauthTokenExpiry?: string;
  // Social Media
  socialUsername?: string;
  socialPageId?: string;
  apiKey?: string;
  apiSecret?: string;
  accessToken?: string;
  accessTokenSecret?: string;
  webhookUrl?: string;
  webhookSecret?: string;
  // WhatsApp
  whatsAppBusinessId?: string;
  whatsAppPhoneNumberId?: string;
  // Timestamps
  lastConnectedAt?: string;
  lastSyncAt?: string;
  createdAt: string;
  updatedAt?: string;
}

interface EmailTemplate {
  id: number;
  name: string;
  subject: string;
  body: string;
  templateType: string;
  isActive: boolean;
  channelId?: number;
  createdAt: string;
}

interface EmailSignature {
  id: number;
  name: string;
  signature: string;
  isDefault: boolean;
  userId?: number;
  createdAt: string;
}

const CHANNEL_TYPES = [
  { value: 'Email', label: 'Email (SMTP/IMAP)', icon: <EmailIcon />, color: '#2196f3' },
  { value: 'EmailOAuth', label: 'Email (OAuth/Microsoft 365)', icon: <EmailIcon />, color: '#0078d4' },
  { value: 'WhatsApp', label: 'WhatsApp Business', icon: <WhatsAppIcon />, color: '#25D366' },
  { value: 'Twitter', label: 'Twitter/X', icon: <TwitterIcon />, color: '#1DA1F2' },
  { value: 'Facebook', label: 'Facebook Messenger', icon: <FacebookIcon />, color: '#4267B2' },
  { value: 'Instagram', label: 'Instagram', icon: <InstagramIcon />, color: '#E1306C' },
  { value: 'LinkedIn', label: 'LinkedIn', icon: <LinkedInIcon />, color: '#0077B5' },
  { value: 'WebForm', label: 'Web Form Ingestion', icon: <WebFormIcon />, color: '#607d8b' },
];

function ChannelSettingsPage() {
  // State
  const [channels, setChannels] = useState<Channel[]>([]);
  const [templates, setTemplates] = useState<EmailTemplate[]>([]);
  const [signatures, setSignatures] = useState<EmailSignature[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [tabValue, setTabValue] = useState(0);

  // Dialog state
  const [channelDialogOpen, setChannelDialogOpen] = useState(false);
  const [templateDialogOpen, setTemplateDialogOpen] = useState(false);
  const [signatureDialogOpen, setSignatureDialogOpen] = useState(false);
  const [editingChannel, setEditingChannel] = useState<Channel | null>(null);
  const [editingTemplate, setEditingTemplate] = useState<EmailTemplate | null>(null);
  const [editingSignature, setEditingSignature] = useState<EmailSignature | null>(null);
  const [showPassword, setShowPassword] = useState(false);

  // Channel form
  const [channelForm, setChannelForm] = useState({
    channelType: 'Email',
    name: '',
    isEnabled: true,
    isDefault: false,
    // Email SMTP/IMAP
    smtpHost: '',
    smtpPort: 587,
    smtpUsername: '',
    smtpPassword: '',
    smtpUseSsl: true,
    imapHost: '',
    imapPort: 993,
    imapUsername: '',
    imapUseSsl: true,
    fromEmail: '',
    fromName: '',
    replyToEmail: '',
    // OAuth
    oauthProvider: '',
    oauthClientId: '',
    oauthTenantId: '',
    // Social media
    socialUsername: '',
    socialPageId: '',
    apiKey: '',
    apiSecret: '',
    accessToken: '',
    accessTokenSecret: '',
    webhookUrl: '',
    webhookSecret: '',
    // WhatsApp
    whatsAppBusinessId: '',
    whatsAppPhoneNumberId: '',
  });

  // Template form
  const [templateForm, setTemplateForm] = useState({
    name: '',
    subject: '',
    body: '',
    templateType: 'General',
    isActive: true,
    channelId: 0,
  });

  // Signature form
  const [signatureForm, setSignatureForm] = useState({
    name: '',
    signature: '',
    isDefault: false,
  });

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    setLoading(true);
    try {
      const [channelsRes, templatesRes] = await Promise.all([
        apiClient.get('/communications/channels'),
        apiClient.get('/communications/templates'),
      ]);
      setChannels(channelsRes.data);
      setTemplates(templatesRes.data);
      // Try to fetch signatures if endpoint exists
      try {
        const signaturesRes = await apiClient.get('/communications/signatures');
        setSignatures(signaturesRes.data);
      } catch {
        // Endpoint may not exist yet
        setSignatures([]);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load settings');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenChannelDialog = (channel?: Channel) => {
    if (channel) {
      setEditingChannel(channel);
      setChannelForm({
        channelType: channel.channelType,
        name: channel.name,
        isEnabled: channel.isEnabled,
        isDefault: channel.isDefault,
        smtpHost: channel.smtpHost || '',
        smtpPort: channel.smtpPort || 587,
        smtpUsername: channel.smtpUsername || '',
        smtpPassword: channel.smtpPassword || '',
        smtpUseSsl: channel.smtpUseSsl ?? true,
        imapHost: channel.imapHost || '',
        imapPort: channel.imapPort || 993,
        imapUsername: channel.imapUsername || '',
        imapUseSsl: channel.imapUseSsl ?? true,
        fromEmail: channel.fromEmail || '',
        fromName: channel.fromName || '',
        replyToEmail: channel.replyToEmail || '',
        oauthProvider: channel.oauthProvider || '',
        oauthClientId: channel.oauthClientId || '',
        oauthTenantId: channel.oauthTenantId || '',
        socialUsername: channel.socialUsername || '',
        socialPageId: channel.socialPageId || '',
        apiKey: channel.apiKey || '',
        apiSecret: channel.apiSecret || '',
        accessToken: channel.accessToken || '',
        accessTokenSecret: channel.accessTokenSecret || '',
        webhookUrl: channel.webhookUrl || '',
        webhookSecret: channel.webhookSecret || '',
        whatsAppBusinessId: channel.whatsAppBusinessId || '',
        whatsAppPhoneNumberId: channel.whatsAppPhoneNumberId || '',
      });
    } else {
      setEditingChannel(null);
      setChannelForm({
        channelType: 'Email',
        name: '',
        isEnabled: true,
        isDefault: false,
        smtpHost: '',
        smtpPort: 587,
        smtpUsername: '',
        smtpPassword: '',
        smtpUseSsl: true,
        imapHost: '',
        imapPort: 993,
        imapUsername: '',
        imapUseSsl: true,
        fromEmail: '',
        fromName: '',
        replyToEmail: '',
        oauthProvider: '',
        oauthClientId: '',
        oauthTenantId: '',
        socialUsername: '',
        socialPageId: '',
        apiKey: '',
        apiSecret: '',
        accessToken: '',
        accessTokenSecret: '',
        webhookUrl: '',
        webhookSecret: '',
        whatsAppBusinessId: '',
        whatsAppPhoneNumberId: '',
      });
    }
    setChannelDialogOpen(true);
  };

  const handleSaveChannel = async () => {
    try {
      const payload = {
        ...channelForm,
        smtpPort: parseInt(String(channelForm.smtpPort)) || 587,
        imapPort: parseInt(String(channelForm.imapPort)) || 993,
      };
      
      if (editingChannel) {
        await apiClient.put(`/communications/channels/${editingChannel.id}`, payload);
        setSuccess('Channel updated successfully');
      } else {
        await apiClient.post('/communications/channels', payload);
        setSuccess('Channel created successfully');
      }
      setChannelDialogOpen(false);
      fetchData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save channel');
    }
  };

  const handleDeleteChannel = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this channel?')) return;
    try {
      await apiClient.delete(`/communications/channels/${id}`);
      setSuccess('Channel deleted successfully');
      fetchData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete channel');
    }
  };

  const handleToggleChannel = async (channel: Channel) => {
    try {
      await apiClient.put(`/communications/channels/${channel.id}`, {
        ...channel,
        isEnabled: !channel.isEnabled,
      });
      setChannels(channels.map(c => c.id === channel.id ? { ...c, isEnabled: !c.isEnabled } : c));
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to toggle channel');
    }
  };

  const handleOpenTemplateDialog = (template?: EmailTemplate) => {
    if (template) {
      setEditingTemplate(template);
      setTemplateForm({
        name: template.name,
        subject: template.subject,
        body: template.body,
        templateType: template.templateType,
        isActive: template.isActive,
        channelId: template.channelId || 0,
      });
    } else {
      setEditingTemplate(null);
      setTemplateForm({
        name: '',
        subject: '',
        body: '',
        templateType: 'General',
        isActive: true,
        channelId: 0,
      });
    }
    setTemplateDialogOpen(true);
  };

  const handleSaveTemplate = async () => {
    try {
      const payload = {
        ...templateForm,
        channelId: templateForm.channelId || null,
      };
      
      if (editingTemplate) {
        await apiClient.put(`/communications/templates/${editingTemplate.id}`, payload);
        setSuccess('Template updated successfully');
      } else {
        await apiClient.post('/communications/templates', payload);
        setSuccess('Template created successfully');
      }
      setTemplateDialogOpen(false);
      fetchData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save template');
    }
  };

  const handleDeleteTemplate = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this template?')) return;
    try {
      await apiClient.delete(`/communications/templates/${id}`);
      setSuccess('Template deleted successfully');
      fetchData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete template');
    }
  };

  const handleOpenSignatureDialog = (signature?: EmailSignature) => {
    if (signature) {
      setEditingSignature(signature);
      setSignatureForm({
        name: signature.name,
        signature: signature.signature,
        isDefault: signature.isDefault,
      });
    } else {
      setEditingSignature(null);
      setSignatureForm({
        name: '',
        signature: '',
        isDefault: false,
      });
    }
    setSignatureDialogOpen(true);
  };

  const handleSaveSignature = async () => {
    try {
      if (editingSignature) {
        await apiClient.put(`/communications/signatures/${editingSignature.id}`, signatureForm);
        setSuccess('Signature updated successfully');
      } else {
        await apiClient.post('/communications/signatures', signatureForm);
        setSuccess('Signature created successfully');
      }
      setSignatureDialogOpen(false);
      fetchData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save signature');
    }
  };

  const getChannelTypeInfo = (channelType: string) => {
    return CHANNEL_TYPES.find(ct => ct.value === channelType) || CHANNEL_TYPES[0];
  };

  const getWebhookUrl = (channelType: string) => {
    const baseUrl = window.location.origin.replace('3000', '5000');
    switch (channelType) {
      case 'Email': return `${baseUrl}/api/webhooks/email/inbound`;
      case 'Twitter': return `${baseUrl}/api/webhooks/twitter`;
      case 'Facebook': return `${baseUrl}/api/webhooks/facebook`;
      case 'Instagram': return `${baseUrl}/api/webhooks/instagram`;
      case 'LinkedIn': return `${baseUrl}/api/webhooks/linkedin`;
      case 'WhatsApp': return `${baseUrl}/api/webhooks/whatsapp`;
      case 'WebForm': return `${baseUrl}/api/webhooks/web-form`;
      default: return `${baseUrl}/api/webhooks/verify`;
    }
  };

  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
    setSuccess('Copied to clipboard');
  };

  // Render channel card
  const renderChannelCard = (channel: Channel) => {
    const typeInfo = getChannelTypeInfo(channel.channelType);
    
    return (
      <Card key={channel.id} sx={{ mb: 2 }}>
        <CardHeader
          avatar={
            <Avatar sx={{ bgcolor: typeInfo.color }}>
              {typeInfo.icon}
            </Avatar>
          }
          action={
            <Box>
              <Switch
                checked={channel.isEnabled}
                onChange={() => handleToggleChannel(channel)}
                color="primary"
              />
              <IconButton onClick={() => handleOpenChannelDialog(channel)}>
                <EditIcon />
              </IconButton>
              <IconButton onClick={() => handleDeleteChannel(channel.id)} color="error">
                <DeleteIcon />
              </IconButton>
            </Box>
          }
          title={
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              {channel.name}
              {channel.isDefault && <Chip size="small" label="Default" color="primary" />}
              <Chip
                size="small"
                label={channel.status}
                color={channel.status === 'Active' || channel.status === 'Configured' ? 'success' : 'default'}
              />
            </Box>
          }
          subheader={typeInfo.label}
        />
        <CardContent>
          <Grid container spacing={2}>
            {channel.channelType === 'Email' || channel.channelType === 'EmailOAuth' ? (
              <>
                <Grid item xs={12} md={6}>
                  <Typography variant="caption" color="text.secondary">From Email</Typography>
                  <Typography variant="body2">{channel.fromEmail || '-'}</Typography>
                </Grid>
                <Grid item xs={12} md={6}>
                  <Typography variant="caption" color="text.secondary">From Name</Typography>
                  <Typography variant="body2">{channel.fromName || '-'}</Typography>
                </Grid>
                {channel.smtpHost && (
                  <Grid item xs={12} md={6}>
                    <Typography variant="caption" color="text.secondary">SMTP Server</Typography>
                    <Typography variant="body2">{channel.smtpHost}:{channel.smtpPort}</Typography>
                  </Grid>
                )}
              </>
            ) : channel.channelType === 'WhatsApp' ? (
              <>
                <Grid item xs={12} md={6}>
                  <Typography variant="caption" color="text.secondary">Business ID</Typography>
                  <Typography variant="body2">{channel.whatsAppBusinessId || '-'}</Typography>
                </Grid>
                <Grid item xs={12} md={6}>
                  <Typography variant="caption" color="text.secondary">Phone Number ID</Typography>
                  <Typography variant="body2">{channel.whatsAppPhoneNumberId || '-'}</Typography>
                </Grid>
              </>
            ) : (
              <>
                <Grid item xs={12} md={6}>
                  <Typography variant="caption" color="text.secondary">Username/Page</Typography>
                  <Typography variant="body2">{channel.socialUsername || channel.socialPageId || '-'}</Typography>
                </Grid>
              </>
            )}
            <Grid item xs={12}>
              <Divider sx={{ my: 1 }} />
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Typography variant="caption" color="text.secondary">Webhook URL:</Typography>
                <code style={{ fontSize: '0.75rem', background: '#f5f5f5', padding: '2px 6px', borderRadius: 4 }}>
                  {getWebhookUrl(channel.channelType)}
                </code>
                <IconButton size="small" onClick={() => copyToClipboard(getWebhookUrl(channel.channelType))}>
                  <CopyIcon fontSize="small" />
                </IconButton>
              </Box>
            </Grid>
          </Grid>
        </CardContent>
      </Card>
    );
  };

  // Render channel form fields based on type
  const renderChannelFormFields = () => {
    switch (channelForm.channelType) {
      case 'Email':
        return (
          <>
            <Typography variant="subtitle2" sx={{ mt: 2, mb: 1 }}>SMTP Settings (Outgoing)</Typography>
            <Grid container spacing={2}>
              <Grid item xs={8}>
                <TextField
                  label="SMTP Host"
                  value={channelForm.smtpHost}
                  onChange={(e) => setChannelForm({ ...channelForm, smtpHost: e.target.value })}
                  fullWidth
                  placeholder="smtp.example.com"
                />
              </Grid>
              <Grid item xs={4}>
                <TextField
                  label="Port"
                  type="number"
                  value={channelForm.smtpPort}
                  onChange={(e) => setChannelForm({ ...channelForm, smtpPort: parseInt(e.target.value) || 587 })}
                  fullWidth
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  label="Username"
                  value={channelForm.smtpUsername}
                  onChange={(e) => setChannelForm({ ...channelForm, smtpUsername: e.target.value })}
                  fullWidth
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  label="Password"
                  type={showPassword ? 'text' : 'password'}
                  value={channelForm.smtpPassword}
                  onChange={(e) => setChannelForm({ ...channelForm, smtpPassword: e.target.value })}
                  fullWidth
                  InputProps={{
                    endAdornment: (
                      <InputAdornment position="end">
                        <IconButton onClick={() => setShowPassword(!showPassword)}>
                          {showPassword ? <VisibilityOffIcon /> : <VisibilityIcon />}
                        </IconButton>
                      </InputAdornment>
                    ),
                  }}
                />
              </Grid>
              <Grid item xs={12}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={channelForm.smtpUseSsl}
                      onChange={(e) => setChannelForm({ ...channelForm, smtpUseSsl: e.target.checked })}
                    />
                  }
                  label="Use SSL/TLS"
                />
              </Grid>
            </Grid>

            <Typography variant="subtitle2" sx={{ mt: 2, mb: 1 }}>IMAP Settings (Incoming)</Typography>
            <Grid container spacing={2}>
              <Grid item xs={8}>
                <TextField
                  label="IMAP Host"
                  value={channelForm.imapHost}
                  onChange={(e) => setChannelForm({ ...channelForm, imapHost: e.target.value })}
                  fullWidth
                  placeholder="imap.example.com"
                />
              </Grid>
              <Grid item xs={4}>
                <TextField
                  label="Port"
                  type="number"
                  value={channelForm.imapPort}
                  onChange={(e) => setChannelForm({ ...channelForm, imapPort: parseInt(e.target.value) || 993 })}
                  fullWidth
                />
              </Grid>
              <Grid item xs={12}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={channelForm.imapUseSsl}
                      onChange={(e) => setChannelForm({ ...channelForm, imapUseSsl: e.target.checked })}
                    />
                  }
                  label="Use SSL/TLS"
                />
              </Grid>
            </Grid>

            <Typography variant="subtitle2" sx={{ mt: 2, mb: 1 }}>Email Identity</Typography>
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <TextField
                  label="From Email"
                  value={channelForm.fromEmail}
                  onChange={(e) => setChannelForm({ ...channelForm, fromEmail: e.target.value })}
                  fullWidth
                  required
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  label="From Name"
                  value={channelForm.fromName}
                  onChange={(e) => setChannelForm({ ...channelForm, fromName: e.target.value })}
                  fullWidth
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  label="Reply-To Email"
                  value={channelForm.replyToEmail}
                  onChange={(e) => setChannelForm({ ...channelForm, replyToEmail: e.target.value })}
                  fullWidth
                />
              </Grid>
            </Grid>
          </>
        );

      case 'EmailOAuth':
        return (
          <>
            <Typography variant="subtitle2" sx={{ mt: 2, mb: 1 }}>OAuth Configuration (Microsoft 365 / Google)</Typography>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <FormControl fullWidth>
                  <InputLabel>OAuth Provider</InputLabel>
                  <Select
                    value={channelForm.oauthProvider}
                    label="OAuth Provider"
                    onChange={(e) => setChannelForm({ ...channelForm, oauthProvider: e.target.value })}
                  >
                    <MenuItem value="Microsoft">Microsoft 365 / Outlook</MenuItem>
                    <MenuItem value="Google">Google Workspace / Gmail</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12}>
                <TextField
                  label="Client ID (Application ID)"
                  value={channelForm.oauthClientId}
                  onChange={(e) => setChannelForm({ ...channelForm, oauthClientId: e.target.value })}
                  fullWidth
                  required
                />
              </Grid>
              {channelForm.oauthProvider === 'Microsoft' && (
                <Grid item xs={12}>
                  <TextField
                    label="Tenant ID"
                    value={channelForm.oauthTenantId}
                    onChange={(e) => setChannelForm({ ...channelForm, oauthTenantId: e.target.value })}
                    fullWidth
                    helperText="Leave blank for multi-tenant apps"
                  />
                </Grid>
              )}
              <Grid item xs={12}>
                <Alert severity="info">
                  After saving, you'll need to complete the OAuth authorization flow by clicking "Connect" on the channel card.
                </Alert>
              </Grid>
            </Grid>

            <Typography variant="subtitle2" sx={{ mt: 2, mb: 1 }}>Email Identity</Typography>
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <TextField
                  label="From Email"
                  value={channelForm.fromEmail}
                  onChange={(e) => setChannelForm({ ...channelForm, fromEmail: e.target.value })}
                  fullWidth
                  required
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  label="From Name"
                  value={channelForm.fromName}
                  onChange={(e) => setChannelForm({ ...channelForm, fromName: e.target.value })}
                  fullWidth
                />
              </Grid>
            </Grid>
          </>
        );

      case 'Twitter':
        return (
          <>
            <Typography variant="subtitle2" sx={{ mt: 2, mb: 1 }}>Twitter/X API Credentials</Typography>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField
                  label="Twitter Username"
                  value={channelForm.socialUsername}
                  onChange={(e) => setChannelForm({ ...channelForm, socialUsername: e.target.value })}
                  fullWidth
                  InputProps={{ startAdornment: <InputAdornment position="start">@</InputAdornment> }}
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  label="API Key"
                  value={channelForm.apiKey}
                  onChange={(e) => setChannelForm({ ...channelForm, apiKey: e.target.value })}
                  fullWidth
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  label="API Secret"
                  type={showPassword ? 'text' : 'password'}
                  value={channelForm.apiSecret}
                  onChange={(e) => setChannelForm({ ...channelForm, apiSecret: e.target.value })}
                  fullWidth
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  label="Access Token"
                  value={channelForm.accessToken}
                  onChange={(e) => setChannelForm({ ...channelForm, accessToken: e.target.value })}
                  fullWidth
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  label="Access Token Secret"
                  type={showPassword ? 'text' : 'password'}
                  value={channelForm.accessTokenSecret}
                  onChange={(e) => setChannelForm({ ...channelForm, accessTokenSecret: e.target.value })}
                  fullWidth
                />
              </Grid>
              <Grid item xs={12}>
                <Alert severity="info" sx={{ mt: 1 }}>
                  Configure this webhook URL in your Twitter Developer Portal:
                  <code style={{ display: 'block', marginTop: 8 }}>{getWebhookUrl('Twitter')}</code>
                </Alert>
              </Grid>
            </Grid>
          </>
        );

      case 'Facebook':
      case 'Instagram':
        return (
          <>
            <Typography variant="subtitle2" sx={{ mt: 2, mb: 1 }}>
              {channelForm.channelType === 'Facebook' ? 'Facebook' : 'Instagram'} API Credentials
            </Typography>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField
                  label="Page ID"
                  value={channelForm.socialPageId}
                  onChange={(e) => setChannelForm({ ...channelForm, socialPageId: e.target.value })}
                  fullWidth
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  label="Access Token (Page Token)"
                  value={channelForm.accessToken}
                  onChange={(e) => setChannelForm({ ...channelForm, accessToken: e.target.value })}
                  fullWidth
                  multiline
                  rows={2}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  label="Webhook Verify Token"
                  value={channelForm.webhookSecret}
                  onChange={(e) => setChannelForm({ ...channelForm, webhookSecret: e.target.value })}
                  fullWidth
                  helperText="Custom token you set in Facebook Developer Console"
                />
              </Grid>
              <Grid item xs={12}>
                <Alert severity="info">
                  Configure this webhook URL in Meta Developer Console:
                  <code style={{ display: 'block', marginTop: 8 }}>{getWebhookUrl(channelForm.channelType)}</code>
                </Alert>
              </Grid>
            </Grid>
          </>
        );

      case 'LinkedIn':
        return (
          <>
            <Typography variant="subtitle2" sx={{ mt: 2, mb: 1 }}>LinkedIn API Credentials</Typography>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField
                  label="Company/Page ID"
                  value={channelForm.socialPageId}
                  onChange={(e) => setChannelForm({ ...channelForm, socialPageId: e.target.value })}
                  fullWidth
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  label="Client ID"
                  value={channelForm.apiKey}
                  onChange={(e) => setChannelForm({ ...channelForm, apiKey: e.target.value })}
                  fullWidth
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  label="Client Secret"
                  type={showPassword ? 'text' : 'password'}
                  value={channelForm.apiSecret}
                  onChange={(e) => setChannelForm({ ...channelForm, apiSecret: e.target.value })}
                  fullWidth
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  label="Access Token"
                  value={channelForm.accessToken}
                  onChange={(e) => setChannelForm({ ...channelForm, accessToken: e.target.value })}
                  fullWidth
                  multiline
                  rows={2}
                />
              </Grid>
            </Grid>
          </>
        );

      case 'WhatsApp':
        return (
          <>
            <Typography variant="subtitle2" sx={{ mt: 2, mb: 1 }}>WhatsApp Business API Credentials</Typography>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField
                  label="WhatsApp Business Account ID"
                  value={channelForm.whatsAppBusinessId}
                  onChange={(e) => setChannelForm({ ...channelForm, whatsAppBusinessId: e.target.value })}
                  fullWidth
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  label="Phone Number ID"
                  value={channelForm.whatsAppPhoneNumberId}
                  onChange={(e) => setChannelForm({ ...channelForm, whatsAppPhoneNumberId: e.target.value })}
                  fullWidth
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  label="Access Token"
                  value={channelForm.accessToken}
                  onChange={(e) => setChannelForm({ ...channelForm, accessToken: e.target.value })}
                  fullWidth
                  multiline
                  rows={2}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  label="Webhook Verify Token"
                  value={channelForm.webhookSecret}
                  onChange={(e) => setChannelForm({ ...channelForm, webhookSecret: e.target.value })}
                  fullWidth
                />
              </Grid>
              <Grid item xs={12}>
                <Alert severity="info">
                  Configure this webhook URL in Meta Business Suite:
                  <code style={{ display: 'block', marginTop: 8 }}>{getWebhookUrl('WhatsApp')}</code>
                </Alert>
              </Grid>
            </Grid>
          </>
        );

      case 'WebForm':
        return (
          <>
            <Typography variant="subtitle2" sx={{ mt: 2, mb: 1 }}>Web Form Ingestion Settings</Typography>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <Alert severity="info" sx={{ mb: 2 }}>
                  Use this endpoint to receive web form submissions:
                  <code style={{ display: 'block', marginTop: 8 }}>{getWebhookUrl('WebForm')}</code>
                </Alert>
              </Grid>
              <Grid item xs={12}>
                <TextField
                  label="API Key (Optional Security)"
                  value={channelForm.apiKey}
                  onChange={(e) => setChannelForm({ ...channelForm, apiKey: e.target.value })}
                  fullWidth
                  helperText="If set, requests must include this in X-API-Key header"
                />
              </Grid>
              <Grid item xs={12}>
                <Typography variant="body2" color="text.secondary">
                  Expected JSON payload format:
                </Typography>
                <Paper variant="outlined" sx={{ p: 2, mt: 1, bgcolor: 'grey.50' }}>
                  <pre style={{ margin: 0, fontSize: '0.8rem' }}>
{`{
  "name": "John Doe",
  "email": "john@example.com",
  "phone": "+1234567890",
  "subject": "Contact Form Submission",
  "message": "Hello, I'm interested...",
  "formName": "Contact Form",
  "pageUrl": "https://example.com/contact"
}`}
                  </pre>
                </Paper>
              </Grid>
            </Grid>
          </>
        );

      default:
        return null;
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '50vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
            <img src={logo} alt="CRM Logo" style={{ width: '100%', height: '100%', objectFit: 'contain' }} />
          </Box>
          <Typography variant="h4" fontWeight={700}>Channel Settings</Typography>
        </Box>
        <Button
          variant="outlined"
          startIcon={<RefreshIcon />}
          onClick={fetchData}
        >
          Refresh
        </Button>
      </Box>

      {/* Alerts */}
      {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccess(null)}>{success}</Alert>}

      {/* Tabs */}
      <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
        <Tabs value={tabValue} onChange={(_, v) => setTabValue(v)}>
          <Tab label="Channels" icon={<SettingsIcon />} iconPosition="start" />
          <Tab label="Email Templates" icon={<TemplateIcon />} iconPosition="start" />
          <Tab label="Signatures & Salutations" icon={<SignatureIcon />} iconPosition="start" />
        </Tabs>
      </Box>

      {/* Channels Tab */}
      <TabPanel value={tabValue} index={0}>
        <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpenChannelDialog()}>
            Add Channel
          </Button>
        </Box>
        
        {channels.length === 0 ? (
          <Paper sx={{ p: 4, textAlign: 'center' }}>
            <Typography color="text.secondary">No channels configured. Add your first channel to get started.</Typography>
          </Paper>
        ) : (
          channels.map(channel => renderChannelCard(channel))
        )}
      </TabPanel>

      {/* Templates Tab */}
      <TabPanel value={tabValue} index={1}>
        <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpenTemplateDialog()}>
            Add Template
          </Button>
        </Box>
        
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Subject</TableCell>
                <TableCell>Type</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {templates.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={5} align="center">
                    <Typography color="text.secondary" sx={{ py: 4 }}>No templates found</Typography>
                  </TableCell>
                </TableRow>
              ) : (
                templates.map(template => (
                  <TableRow key={template.id}>
                    <TableCell>{template.name}</TableCell>
                    <TableCell>{template.subject}</TableCell>
                    <TableCell>
                      <Chip size="small" label={template.templateType} />
                    </TableCell>
                    <TableCell>
                      <Chip
                        size="small"
                        label={template.isActive ? 'Active' : 'Inactive'}
                        color={template.isActive ? 'success' : 'default'}
                      />
                    </TableCell>
                    <TableCell>
                      <IconButton size="small" onClick={() => handleOpenTemplateDialog(template)}>
                        <EditIcon />
                      </IconButton>
                      <IconButton size="small" color="error" onClick={() => handleDeleteTemplate(template.id)}>
                        <DeleteIcon />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </TabPanel>

      {/* Signatures Tab */}
      <TabPanel value={tabValue} index={2}>
        <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpenSignatureDialog()}>
            Add Signature
          </Button>
        </Box>
        
        <Grid container spacing={2}>
          {signatures.length === 0 ? (
            <Grid item xs={12}>
              <Paper sx={{ p: 4, textAlign: 'center' }}>
                <Typography color="text.secondary">No signatures found. Add your first signature.</Typography>
              </Paper>
            </Grid>
          ) : (
            signatures.map(sig => (
              <Grid item xs={12} md={6} key={sig.id}>
                <Card>
                  <CardHeader
                    title={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        {sig.name}
                        {sig.isDefault && <Chip size="small" label="Default" color="primary" />}
                      </Box>
                    }
                    action={
                      <Box>
                        <IconButton onClick={() => handleOpenSignatureDialog(sig)}>
                          <EditIcon />
                        </IconButton>
                      </Box>
                    }
                  />
                  <CardContent>
                    <Paper variant="outlined" sx={{ p: 2, maxHeight: 150, overflow: 'auto' }}>
                      <div dangerouslySetInnerHTML={{ __html: sig.signature }} />
                    </Paper>
                  </CardContent>
                </Card>
              </Grid>
            ))
          )}
        </Grid>
      </TabPanel>

      {/* Channel Dialog */}
      <Dialog open={channelDialogOpen} onClose={() => setChannelDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>{editingChannel ? 'Edit Channel' : 'Add Channel'}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth>
                  <InputLabel>Channel Type</InputLabel>
                  <Select
                    value={channelForm.channelType}
                    label="Channel Type"
                    onChange={(e) => setChannelForm({ ...channelForm, channelType: e.target.value })}
                    disabled={!!editingChannel}
                  >
                    {CHANNEL_TYPES.map(ct => (
                      <MenuItem key={ct.value} value={ct.value}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          {ct.icon}
                          {ct.label}
                        </Box>
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  label="Channel Name"
                  value={channelForm.name}
                  onChange={(e) => setChannelForm({ ...channelForm, name: e.target.value })}
                  fullWidth
                  required
                />
              </Grid>
              <Grid item xs={6}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={channelForm.isEnabled}
                      onChange={(e) => setChannelForm({ ...channelForm, isEnabled: e.target.checked })}
                    />
                  }
                  label="Enabled"
                />
              </Grid>
              <Grid item xs={6}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={channelForm.isDefault}
                      onChange={(e) => setChannelForm({ ...channelForm, isDefault: e.target.checked })}
                    />
                  }
                  label="Default Channel"
                />
              </Grid>
            </Grid>
            
            {renderChannelFormFields()}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setChannelDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSaveChannel} disabled={!channelForm.name}>
            {editingChannel ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Template Dialog */}
      <Dialog open={templateDialogOpen} onClose={() => setTemplateDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>{editingTemplate ? 'Edit Template' : 'Add Template'}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label="Template Name"
              value={templateForm.name}
              onChange={(e) => setTemplateForm({ ...templateForm, name: e.target.value })}
              fullWidth
              required
            />
            <FormControl fullWidth>
              <InputLabel>Template Type</InputLabel>
              <Select
                value={templateForm.templateType}
                label="Template Type"
                onChange={(e) => setTemplateForm({ ...templateForm, templateType: e.target.value })}
              >
                <MenuItem value="General">General</MenuItem>
                <MenuItem value="Welcome">Welcome</MenuItem>
                <MenuItem value="FollowUp">Follow Up</MenuItem>
                <MenuItem value="ThankYou">Thank You</MenuItem>
                <MenuItem value="Notification">Notification</MenuItem>
                <MenuItem value="ServiceRequest">Service Request</MenuItem>
                <MenuItem value="Quote">Quote</MenuItem>
                <MenuItem value="Invoice">Invoice</MenuItem>
              </Select>
            </FormControl>
            <TextField
              label="Subject"
              value={templateForm.subject}
              onChange={(e) => setTemplateForm({ ...templateForm, subject: e.target.value })}
              fullWidth
              required
              helperText="Use {{variables}} for dynamic content: {{customerName}}, {{ticketNumber}}, etc."
            />
            <TextField
              label="Body"
              value={templateForm.body}
              onChange={(e) => setTemplateForm({ ...templateForm, body: e.target.value })}
              fullWidth
              multiline
              rows={10}
              helperText="HTML is supported. Use {{variables}} for dynamic content."
            />
            <FormControlLabel
              control={
                <Switch
                  checked={templateForm.isActive}
                  onChange={(e) => setTemplateForm({ ...templateForm, isActive: e.target.checked })}
                />
              }
              label="Active"
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setTemplateDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleSaveTemplate}
            disabled={!templateForm.name || !templateForm.subject}
          >
            {editingTemplate ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Signature Dialog */}
      <Dialog open={signatureDialogOpen} onClose={() => setSignatureDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>{editingSignature ? 'Edit Signature' : 'Add Signature'}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label="Signature Name"
              value={signatureForm.name}
              onChange={(e) => setSignatureForm({ ...signatureForm, name: e.target.value })}
              fullWidth
              required
            />
            <TextField
              label="Signature (HTML)"
              value={signatureForm.signature}
              onChange={(e) => setSignatureForm({ ...signatureForm, signature: e.target.value })}
              fullWidth
              multiline
              rows={8}
              helperText="HTML is supported for rich formatting"
            />
            <FormControlLabel
              control={
                <Switch
                  checked={signatureForm.isDefault}
                  onChange={(e) => setSignatureForm({ ...signatureForm, isDefault: e.target.checked })}
                />
              }
              label="Set as Default"
            />
            {signatureForm.signature && (
              <Box>
                <Typography variant="subtitle2" gutterBottom>Preview:</Typography>
                <Paper variant="outlined" sx={{ p: 2 }}>
                  <div dangerouslySetInnerHTML={{ __html: signatureForm.signature }} />
                </Paper>
              </Box>
            )}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSignatureDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleSaveSignature}
            disabled={!signatureForm.name || !signatureForm.signature}
          >
            {editingSignature ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

export default ChannelSettingsPage;
