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
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Stepper,
  Step,
  StepLabel,
  Paper,
  Chip,
  Divider,
  IconButton,
  InputAdornment,
  Tooltip,
  LinearProgress,
  FormControlLabel,
} from '@mui/material';
import {
  Security as SecurityIcon,
  QrCode as QrCodeIcon,
  ContentCopy as CopyIcon,
  Check as CheckIcon,
  Warning as WarningIcon,
  Shield as ShieldIcon,
  Https as HttpsIcon,
  CloudUpload as UploadIcon,
  Delete as DeleteIcon,
  Verified as VerifiedIcon,
  Error as ErrorIcon,
  Lock as LockIcon,
} from '@mui/icons-material';
import { QRCodeSVG } from 'qrcode.react';

interface TwoFactorSetupProps {
  userId?: number;
}

interface SetupData {
  qrCodeUrl: string;
  secret: string;
  backupCodes: string[];
}

function SecuritySettingsTab({ userId }: TwoFactorSetupProps) {
  const [twoFactorEnabled, setTwoFactorEnabled] = useState(false);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  
  // Setup dialog state
  const [setupDialogOpen, setSetupDialogOpen] = useState(false);
  const [setupData, setSetupData] = useState<SetupData | null>(null);
  const [activeStep, setActiveStep] = useState(0);
  const [verificationCode, setVerificationCode] = useState('');
  const [copied, setCopied] = useState<string | null>(null);
  
  // Disable dialog state
  const [disableDialogOpen, setDisableDialogOpen] = useState(false);
  const [disableCode, setDisableCode] = useState('');

  // SSL/TLS state
  const [sslStatus, setSslStatus] = useState<{
    httpsEnabled: boolean;
    forceRedirect: boolean;
    hasCertificate: boolean;
    hasPrivateKey: boolean;
    certificateExpiry: string | null;
    certificateSubject: string | null;
    isExpired: boolean;
    expiresInDays: number | null;
  } | null>(null);
  const [sslLoading, setSslLoading] = useState(false);
  const [uploadingCert, setUploadingCert] = useState(false);
  const [generatingCert, setGeneratingCert] = useState(false);
  const [certFile, setCertFile] = useState<File | null>(null);
  const [keyFile, setKeyFile] = useState<File | null>(null);
  const [certPassword, setCertPassword] = useState('');

  const getApiUrl = () => {
    return window.location.hostname === 'localhost'
      ? 'http://localhost:5000/api'
      : `http://${window.location.hostname}:5000/api`;
  };

  useEffect(() => {
    loadTwoFactorStatus();
    loadSslStatus();
  }, []);

  const loadSslStatus = async () => {
    try {
      setSslLoading(true);
      const token = localStorage.getItem('accessToken');
      const response = await fetch(`${getApiUrl()}/systemsettings/ssl/status`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      
      if (response.ok) {
        const data = await response.json();
        setSslStatus(data);
      }
    } catch (err) {
      console.error('Error loading SSL status:', err);
    } finally {
      setSslLoading(false);
    }
  };

  const handleCertificateUpload = async () => {
    if (!certFile) {
      setError('Please select a certificate file');
      return;
    }

    // For PFX files, we don't need a separate key file
    const isPfx = certFile.name.toLowerCase().endsWith('.pfx') || certFile.name.toLowerCase().endsWith('.p12');
    if (!isPfx && !keyFile) {
      setError('Please select both certificate and private key files for CRT/PEM format');
      return;
    }

    setUploadingCert(true);
    setError(null);

    try {
      const token = localStorage.getItem('accessToken');
      const formData = new FormData();
      formData.append('certificate', certFile);
      if (keyFile) {
        formData.append('privateKey', keyFile);
      }
      if (certPassword) {
        formData.append('password', certPassword);
      }

      const response = await fetch(`${getApiUrl()}/systemsettings/ssl/upload`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${token}` },
        body: formData,
      });

      if (response.ok) {
        const data = await response.json();
        setSuccess(data.message || 'SSL certificate uploaded successfully. Restart the server to apply.');
        setCertFile(null);
        setKeyFile(null);
        setCertPassword('');
        await loadSslStatus();
      } else {
        const errorData = await response.json();
        setError(errorData.message || 'Failed to upload certificate');
      }
    } catch (err) {
      setError('Failed to upload SSL certificate');
    } finally {
      setUploadingCert(false);
    }
  };

  const handleToggleHttps = async (enabled: boolean, forceRedirect: boolean = false) => {
    setSslLoading(true);
    setError(null);

    try {
      const token = localStorage.getItem('accessToken');
      const response = await fetch(`${getApiUrl()}/systemsettings/ssl/toggle`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ enabled, forceRedirect }),
      });

      if (response.ok) {
        setSuccess(enabled ? 'HTTPS enabled' : 'HTTPS disabled');
        await loadSslStatus();
      } else {
        const errorData = await response.json();
        setError(errorData.message || 'Failed to toggle HTTPS');
      }
    } catch (err) {
      setError('Failed to toggle HTTPS');
    } finally {
      setSslLoading(false);
    }
  };

  const handleRemoveCertificate = async () => {
    if (!window.confirm('Are you sure you want to remove the SSL certificate? HTTPS will be disabled.')) {
      return;
    }

    setSslLoading(true);
    setError(null);

    try {
      const token = localStorage.getItem('accessToken');
      const response = await fetch(`${getApiUrl()}/systemsettings/ssl`, {
        method: 'DELETE',
        headers: { Authorization: `Bearer ${token}` },
      });

      if (response.ok) {
        setSuccess('SSL certificate removed');
        await loadSslStatus();
      } else {
        setError('Failed to remove certificate');
      }
    } catch (err) {
      setError('Failed to remove SSL certificate');
    } finally {
      setSslLoading(false);
    }
  };

  const handleGenerateSelfSignedCertificate = async () => {
    if (!window.confirm('Generate a self-signed SSL certificate? This will replace any existing certificate. A server restart will be required.')) {
      return;
    }

    setGeneratingCert(true);
    setError(null);

    try {
      const token = localStorage.getItem('accessToken');
      const response = await fetch(`${getApiUrl()}/systemsettings/ssl/generate`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          commonName: window.location.hostname || 'localhost',
          validityDays: 365,
        }),
      });

      if (response.ok) {
        const data = await response.json();
        setSuccess(`Self-signed certificate generated successfully. Valid until ${new Date(data.expiresOn).toLocaleDateString()}. Restart the server to apply HTTPS.`);
        await loadSslStatus();
      } else {
        const errorData = await response.json();
        setError(errorData.message || 'Failed to generate certificate');
      }
    } catch (err) {
      setError('Failed to generate self-signed certificate');
    } finally {
      setGeneratingCert(false);
    }
  };

  const loadTwoFactorStatus = async () => {
    try {
      const token = localStorage.getItem('accessToken');
      const response = await fetch(`${getApiUrl()}/auth/me`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      
      if (response.ok) {
        const userData = JSON.parse(localStorage.getItem('crm_user_data') || '{}');
        setTwoFactorEnabled(userData.twoFactorEnabled || false);
      }
    } catch (err) {
      console.error('Error loading 2FA status:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleEnableTwoFactor = async () => {
    setSaving(true);
    setError(null);
    
    try {
      const token = localStorage.getItem('accessToken');
      const response = await fetch(`${getApiUrl()}/auth/2fa/setup`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${token}` },
      });
      
      if (response.ok) {
        const data = await response.json();
        setSetupData(data);
        setSetupDialogOpen(true);
        setActiveStep(0);
      } else {
        const errorData = await response.json();
        setError(errorData.message || 'Failed to setup 2FA');
      }
    } catch (err) {
      setError('Failed to setup two-factor authentication');
    } finally {
      setSaving(false);
    }
  };

  const handleVerifyAndEnable = async () => {
    if (!setupData || !verificationCode) return;
    
    setSaving(true);
    setError(null);
    
    try {
      const token = localStorage.getItem('accessToken');
      
      // First, enable 2FA with the secret
      const enableResponse = await fetch(`${getApiUrl()}/auth/2fa/enable`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          secret: setupData.secret,
          backupCodes: setupData.backupCodes,
        }),
      });
      
      if (!enableResponse.ok) {
        throw new Error('Failed to enable 2FA');
      }
      
      // Verify the code
      const verifyResponse = await fetch(`${getApiUrl()}/auth/2fa/verify`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ code: verificationCode }),
      });
      
      if (verifyResponse.ok) {
        setTwoFactorEnabled(true);
        setActiveStep(2);
        setSuccess('Two-factor authentication enabled successfully!');
        
        // Update stored user data
        const userData = JSON.parse(localStorage.getItem('crm_user_data') || '{}');
        userData.twoFactorEnabled = true;
        localStorage.setItem('crm_user_data', JSON.stringify(userData));
      } else {
        const errorData = await verifyResponse.json();
        setError(errorData.message || 'Invalid verification code');
      }
    } catch (err) {
      setError('Failed to verify code');
    } finally {
      setSaving(false);
    }
  };

  const handleDisableTwoFactor = async () => {
    setSaving(true);
    setError(null);
    
    try {
      const token = localStorage.getItem('accessToken');
      
      // Verify code first
      const verifyResponse = await fetch(`${getApiUrl()}/auth/2fa/verify`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ code: disableCode }),
      });
      
      if (!verifyResponse.ok) {
        setError('Invalid verification code');
        setSaving(false);
        return;
      }
      
      // Disable 2FA
      const response = await fetch(`${getApiUrl()}/auth/2fa/disable`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${token}` },
      });
      
      if (response.ok) {
        setTwoFactorEnabled(false);
        setDisableDialogOpen(false);
        setDisableCode('');
        setSuccess('Two-factor authentication disabled');
        
        // Update stored user data
        const userData = JSON.parse(localStorage.getItem('crm_user_data') || '{}');
        userData.twoFactorEnabled = false;
        localStorage.setItem('crm_user_data', JSON.stringify(userData));
      } else {
        setError('Failed to disable 2FA');
      }
    } catch (err) {
      setError('Failed to disable two-factor authentication');
    } finally {
      setSaving(false);
    }
  };

  const copyToClipboard = (text: string, label: string) => {
    navigator.clipboard.writeText(text);
    setCopied(label);
    setTimeout(() => setCopied(null), 2000);
  };

  const handleCloseSetupDialog = () => {
    setSetupDialogOpen(false);
    setSetupData(null);
    setVerificationCode('');
    setActiveStep(0);
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
      <Typography variant="h6" sx={{ fontWeight: 600, mb: 1 }}>
        Security Settings
      </Typography>
      <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
        Manage your account security settings including two-factor authentication.
      </Typography>

      {success && (
        <Alert severity="success" sx={{ mb: 2, borderRadius: 2 }} onClose={() => setSuccess(null)}>
          {success}
        </Alert>
      )}

      {error && (
        <Alert severity="error" sx={{ mb: 2, borderRadius: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* SSL/TLS Certificate Management Card */}
      <Card sx={{ borderRadius: 3, mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 2, mb: 3 }}>
            <HttpsIcon sx={{ fontSize: 40, color: sslStatus?.httpsEnabled ? '#4CAF50' : '#9E9E9E' }} />
            <Box sx={{ flexGrow: 1 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 1 }}>
                <Typography variant="h6" sx={{ fontWeight: 600 }}>
                  SSL/TLS Encryption
                </Typography>
                <Chip
                  size="small"
                  label={sslStatus?.httpsEnabled ? 'HTTPS Enabled' : 'HTTP Only'}
                  color={sslStatus?.httpsEnabled ? 'success' : 'default'}
                />
              </Box>
              <Typography variant="body2" color="textSecondary">
                Enable encrypted HTTPS connections between the browser and server. Upload your SSL certificate and private key to enable secure communications.
              </Typography>
            </Box>
          </Box>

          {sslLoading && <LinearProgress sx={{ mb: 2 }} />}

          {/* HTTPS Information - Explain how HTTPS works in different deployments */}
          <Paper sx={{ p: 2, mb: 3, bgcolor: '#e3f2fd', borderRadius: 2, border: '1px solid #90caf9' }}>
            <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 2 }}>
              <HttpsIcon sx={{ color: '#1976d2', mt: 0.5 }} />
              <Box>
                <Typography variant="subtitle2" sx={{ fontWeight: 600, color: '#1565c0' }}>
                  HTTPS Configuration
                </Typography>
                <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>
                  <strong>Kubernetes/Docker deployments:</strong> HTTPS is handled by your ingress controller or load balancer (SSL termination). 
                  The application runs on HTTP internally while the proxy handles secure external connections.
                </Typography>
                <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>
                  <strong>Standalone deployments:</strong> Upload an SSL certificate below to enable direct HTTPS connections from the application server.
                </Typography>
              </Box>
            </Box>
          </Paper>

          {/* Certificate Status */}
          {sslStatus?.hasCertificate && (
            <Paper sx={{ p: 2, mb: 3, bgcolor: '#f5f5f5', borderRadius: 2 }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1, display: 'flex', alignItems: 'center', gap: 1 }}>
                {sslStatus.isExpired ? (
                  <ErrorIcon color="error" fontSize="small" />
                ) : (
                  <VerifiedIcon color="success" fontSize="small" />
                )}
                Current Certificate
              </Typography>
              <Grid container spacing={2}>
                <Grid item xs={12} sm={6}>
                  <Typography variant="body2" color="textSecondary">Subject</Typography>
                  <Typography variant="body2" sx={{ fontFamily: 'monospace', wordBreak: 'break-all' }}>
                    {sslStatus.certificateSubject || 'Unknown'}
                  </Typography>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Typography variant="body2" color="textSecondary">Expires</Typography>
                  <Typography variant="body2" color={sslStatus.isExpired ? 'error' : 'textPrimary'}>
                    {sslStatus.certificateExpiry ? new Date(sslStatus.certificateExpiry).toLocaleDateString() : 'Unknown'}
                    {sslStatus.expiresInDays !== null && (
                      <Chip 
                        size="small" 
                        label={sslStatus.isExpired ? 'Expired' : `${sslStatus.expiresInDays} days left`}
                        color={sslStatus.isExpired ? 'error' : sslStatus.expiresInDays < 30 ? 'warning' : 'success'}
                        sx={{ ml: 1 }}
                      />
                    )}
                  </Typography>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Typography variant="body2" color="textSecondary">Certificate File</Typography>
                  <Chip size="small" label="Uploaded" color="success" icon={<CheckIcon />} />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Typography variant="body2" color="textSecondary">Private Key</Typography>
                  <Chip 
                    size="small" 
                    label={sslStatus.hasPrivateKey ? 'Uploaded' : 'Not Uploaded'} 
                    color={sslStatus.hasPrivateKey ? 'success' : 'warning'}
                    icon={sslStatus.hasPrivateKey ? <CheckIcon /> : <WarningIcon />}
                  />
                </Grid>
              </Grid>
              <Box sx={{ mt: 2, display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
                <Button
                  size="small"
                  color="error"
                  startIcon={<DeleteIcon />}
                  onClick={handleRemoveCertificate}
                  disabled={sslLoading}
                  sx={{ textTransform: 'none' }}
                >
                  Remove Certificate
                </Button>
              </Box>
            </Paper>
          )}

          {/* Upload Certificate */}
          <Divider sx={{ my: 2 }} />
          <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 2 }}>
            {sslStatus?.hasCertificate ? 'Replace Certificate' : 'Upload SSL Certificate'}
          </Typography>
          
          {/* Quick Generate Button */}
          <Box sx={{ mb: 3 }}>
            <Button
              variant="outlined"
              color="primary"
              startIcon={generatingCert ? <CircularProgress size={16} color="inherit" /> : <HttpsIcon />}
              onClick={handleGenerateSelfSignedCertificate}
              disabled={generatingCert || sslLoading}
              sx={{ textTransform: 'none', mr: 2 }}
            >
              Generate Self-Signed Certificate
            </Button>
            <Typography variant="caption" color="textSecondary" sx={{ display: 'block', mt: 1 }}>
              Quick option for testing/development. For production, upload a certificate from a trusted CA.
            </Typography>
          </Box>
          
          <Divider sx={{ my: 2 }}>
            <Typography variant="caption" color="textSecondary">OR UPLOAD YOUR CERTIFICATE</Typography>
          </Divider>
          
          <Grid container spacing={2}>
            <Grid item xs={12} sm={4}>
              <Paper
                sx={{
                  p: 2,
                  border: '2px dashed',
                  borderColor: certFile ? 'success.main' : 'divider',
                  borderRadius: 2,
                  textAlign: 'center',
                  cursor: 'pointer',
                  '&:hover': { borderColor: 'primary.main' },
                }}
                component="label"
              >
                <input
                  type="file"
                  hidden
                  accept=".crt,.pem,.cer,.pfx,.p12"
                  onChange={(e) => setCertFile(e.target.files?.[0] || null)}
                />
                <LockIcon sx={{ fontSize: 32, color: certFile ? 'success.main' : 'text.secondary', mb: 1 }} />
                <Typography variant="body2" sx={{ fontWeight: 500 }}>
                  {certFile ? certFile.name : 'Certificate File'}
                </Typography>
                <Typography variant="caption" color="textSecondary">
                  .pfx, .p12, .crt, .pem
                </Typography>
              </Paper>
            </Grid>
            <Grid item xs={12} sm={4}>
              <Paper
                sx={{
                  p: 2,
                  border: '2px dashed',
                  borderColor: keyFile ? 'success.main' : 'divider',
                  borderRadius: 2,
                  textAlign: 'center',
                  cursor: 'pointer',
                  opacity: certFile?.name.toLowerCase().match(/\.(pfx|p12)$/) ? 0.5 : 1,
                  '&:hover': { borderColor: 'primary.main' },
                }}
                component="label"
              >
                <input
                  type="file"
                  hidden
                  accept=".key,.pem"
                  onChange={(e) => setKeyFile(e.target.files?.[0] || null)}
                  disabled={!!certFile?.name.toLowerCase().match(/\.(pfx|p12)$/)}
                />
                <SecurityIcon sx={{ fontSize: 32, color: keyFile ? 'success.main' : 'text.secondary', mb: 1 }} />
                <Typography variant="body2" sx={{ fontWeight: 500 }}>
                  {keyFile ? keyFile.name : 'Private Key'}
                </Typography>
                <Typography variant="caption" color="textSecondary">
                  {certFile?.name.toLowerCase().match(/\.(pfx|p12)$/) ? 'Not needed for PFX' : '.key, .pem'}
                </Typography>
              </Paper>
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField
                fullWidth
                label="Certificate Password"
                type="password"
                value={certPassword}
                onChange={(e) => setCertPassword(e.target.value)}
                placeholder="Enter password (if any)"
                size="small"
                helperText="For password-protected PFX files"
              />
            </Grid>
          </Grid>

          <Box sx={{ mt: 2, display: 'flex', gap: 2 }}>
            <Button
              variant="contained"
              startIcon={uploadingCert ? <CircularProgress size={16} color="inherit" /> : <UploadIcon />}
              onClick={handleCertificateUpload}
              disabled={!certFile || uploadingCert}
              sx={{ textTransform: 'none' }}
            >
              Upload Certificate
            </Button>
            {(certFile || keyFile) && (
              <Button
                variant="outlined"
                onClick={() => { setCertFile(null); setKeyFile(null); setCertPassword(''); }}
                sx={{ textTransform: 'none' }}
              >
                Clear
              </Button>
            )}
          </Box>

          <Alert severity="info" sx={{ mt: 2, borderRadius: 2 }}>
            <Typography variant="body2">
              <strong>Supported formats:</strong> PFX/P12 (recommended - includes key) or CRT/PEM + KEY files.<br />
              <strong>Note:</strong> Server restart is required after uploading certificates for HTTPS to take effect.
            </Typography>
          </Alert>
        </CardContent>
      </Card>

      {/* Two-Factor Authentication Card */}
      <Card sx={{ borderRadius: 3, mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 2 }}>
            <ShieldIcon sx={{ fontSize: 40, color: twoFactorEnabled ? '#4CAF50' : '#9E9E9E' }} />
            <Box sx={{ flexGrow: 1 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 1 }}>
                <Typography variant="h6" sx={{ fontWeight: 600 }}>
                  Two-Factor Authentication
                </Typography>
                <Chip
                  size="small"
                  label={twoFactorEnabled ? 'Enabled' : 'Disabled'}
                  color={twoFactorEnabled ? 'success' : 'default'}
                />
              </Box>
              <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
                Add an extra layer of security to your account by requiring a verification code 
                from an authenticator app when signing in.
              </Typography>
              
              {twoFactorEnabled ? (
                <Button
                  variant="outlined"
                  color="error"
                  onClick={() => setDisableDialogOpen(true)}
                  disabled={saving}
                  sx={{ textTransform: 'none' }}
                >
                  Disable Two-Factor Authentication
                </Button>
              ) : (
                <Button
                  variant="contained"
                  onClick={handleEnableTwoFactor}
                  disabled={saving}
                  startIcon={saving ? <CircularProgress size={16} /> : <SecurityIcon />}
                  sx={{
                    textTransform: 'none',
                    backgroundColor: '#6750A4',
                    '&:hover': { backgroundColor: '#5940A2' },
                  }}
                >
                  Enable Two-Factor Authentication
                </Button>
              )}
            </Box>
          </Box>
        </CardContent>
      </Card>

      {/* Recommended Authenticator Apps */}
      <Card sx={{ borderRadius: 3 }}>
        <CardContent>
          <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 2 }}>
            Recommended Authenticator Apps
          </Typography>
          <Grid container spacing={2}>
            {[
              { name: 'Google Authenticator', platform: 'iOS, Android' },
              { name: 'Microsoft Authenticator', platform: 'iOS, Android' },
              { name: 'Authy', platform: 'iOS, Android, Desktop' },
              { name: '1Password', platform: 'iOS, Android, Desktop' },
            ].map((app) => (
              <Grid item xs={12} sm={6} key={app.name}>
                <Paper sx={{ p: 2, borderRadius: 2, backgroundColor: '#f5f5f5' }}>
                  <Typography variant="body2" sx={{ fontWeight: 600 }}>
                    {app.name}
                  </Typography>
                  <Typography variant="caption" color="textSecondary">
                    {app.platform}
                  </Typography>
                </Paper>
              </Grid>
            ))}
          </Grid>
        </CardContent>
      </Card>

      {/* Setup Dialog */}
      <Dialog
        open={setupDialogOpen}
        onClose={activeStep === 2 ? handleCloseSetupDialog : undefined}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <SecurityIcon color="primary" />
            Set Up Two-Factor Authentication
          </Box>
        </DialogTitle>
        <DialogContent>
          <Stepper activeStep={activeStep} sx={{ mb: 3, mt: 1 }}>
            <Step>
              <StepLabel>Scan QR Code</StepLabel>
            </Step>
            <Step>
              <StepLabel>Verify</StepLabel>
            </Step>
            <Step>
              <StepLabel>Complete</StepLabel>
            </Step>
          </Stepper>

          {activeStep === 0 && setupData && (
            <Box>
              <Typography variant="body2" sx={{ mb: 2 }}>
                Scan this QR code with your authenticator app:
              </Typography>
              <Box sx={{ display: 'flex', justifyContent: 'center', mb: 3 }}>
                <Paper sx={{ p: 2, borderRadius: 2 }}>
                  <QRCodeSVG value={setupData.qrCodeUrl} size={200} />
                </Paper>
              </Box>
              <Typography variant="body2" sx={{ mb: 1 }}>
                Or enter this code manually:
              </Typography>
              <Paper sx={{ p: 2, backgroundColor: '#f5f5f5', borderRadius: 2, mb: 2 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                  <Typography variant="body2" sx={{ fontFamily: 'monospace', wordBreak: 'break-all' }}>
                    {setupData.secret}
                  </Typography>
                  <Tooltip title={copied === 'secret' ? 'Copied!' : 'Copy'}>
                    <IconButton
                      size="small"
                      onClick={() => copyToClipboard(setupData.secret, 'secret')}
                    >
                      {copied === 'secret' ? <CheckIcon color="success" /> : <CopyIcon />}
                    </IconButton>
                  </Tooltip>
                </Box>
              </Paper>
              <Button
                variant="contained"
                fullWidth
                onClick={() => setActiveStep(1)}
                sx={{ textTransform: 'none' }}
              >
                I've Scanned the QR Code
              </Button>
            </Box>
          )}

          {activeStep === 1 && (
            <Box>
              <Typography variant="body2" sx={{ mb: 2 }}>
                Enter the 6-digit code from your authenticator app to verify:
              </Typography>
              <TextField
                fullWidth
                label="Verification Code"
                value={verificationCode}
                onChange={(e) => setVerificationCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                placeholder="000000"
                inputProps={{
                  maxLength: 6,
                  style: { textAlign: 'center', fontSize: '1.5rem', letterSpacing: '0.5rem' },
                }}
                sx={{ mb: 2 }}
              />
              {error && (
                <Alert severity="error" sx={{ mb: 2 }}>
                  {error}
                </Alert>
              )}
              <Button
                variant="contained"
                fullWidth
                onClick={handleVerifyAndEnable}
                disabled={verificationCode.length !== 6 || saving}
                sx={{ textTransform: 'none' }}
              >
                {saving ? <CircularProgress size={20} /> : 'Verify and Enable'}
              </Button>
            </Box>
          )}

          {activeStep === 2 && setupData && (
            <Box>
              <Alert severity="success" sx={{ mb: 3 }}>
                Two-factor authentication is now enabled!
              </Alert>
              <Alert severity="warning" icon={<WarningIcon />} sx={{ mb: 2 }}>
                <Typography variant="body2" sx={{ fontWeight: 600, mb: 1 }}>
                  Save Your Backup Codes
                </Typography>
                <Typography variant="body2">
                  These codes can be used to access your account if you lose your authenticator device.
                  Each code can only be used once. Store them in a safe place.
                </Typography>
              </Alert>
              <Paper sx={{ p: 2, backgroundColor: '#f5f5f5', borderRadius: 2, mb: 2 }}>
                <Grid container spacing={1}>
                  {setupData.backupCodes.map((code, index) => (
                    <Grid item xs={6} key={index}>
                      <Typography
                        variant="body2"
                        sx={{ fontFamily: 'monospace', fontSize: '0.9rem' }}
                      >
                        {code}
                      </Typography>
                    </Grid>
                  ))}
                </Grid>
              </Paper>
              <Button
                variant="outlined"
                fullWidth
                onClick={() => copyToClipboard(setupData.backupCodes.join('\n'), 'backup')}
                startIcon={copied === 'backup' ? <CheckIcon /> : <CopyIcon />}
                sx={{ textTransform: 'none' }}
              >
                {copied === 'backup' ? 'Copied!' : 'Copy Backup Codes'}
              </Button>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          {activeStep === 2 && (
            <Button onClick={handleCloseSetupDialog} sx={{ textTransform: 'none' }}>
              Done
            </Button>
          )}
          {activeStep < 2 && (
            <Button onClick={handleCloseSetupDialog} sx={{ textTransform: 'none' }}>
              Cancel
            </Button>
          )}
        </DialogActions>
      </Dialog>

      {/* Disable 2FA Dialog */}
      <Dialog open={disableDialogOpen} onClose={() => setDisableDialogOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Disable Two-Factor Authentication</DialogTitle>
        <DialogContent>
          <Alert severity="warning" sx={{ mb: 2 }}>
            This will make your account less secure. Are you sure you want to continue?
          </Alert>
          <Typography variant="body2" sx={{ mb: 2 }}>
            Enter your current verification code to confirm:
          </Typography>
          <TextField
            fullWidth
            label="Verification Code"
            value={disableCode}
            onChange={(e) => setDisableCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
            placeholder="000000"
            inputProps={{
              maxLength: 6,
              style: { textAlign: 'center', fontSize: '1.5rem', letterSpacing: '0.5rem' },
            }}
          />
          {error && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {error}
            </Alert>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDisableDialogOpen(false)} sx={{ textTransform: 'none' }}>
            Cancel
          </Button>
          <Button
            onClick={handleDisableTwoFactor}
            color="error"
            variant="contained"
            disabled={disableCode.length !== 6 || saving}
            sx={{ textTransform: 'none' }}
          >
            {saving ? <CircularProgress size={20} /> : 'Disable 2FA'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default SecuritySettingsTab;
