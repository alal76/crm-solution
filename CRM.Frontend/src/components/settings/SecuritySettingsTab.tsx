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
} from '@mui/material';
import {
  Security as SecurityIcon,
  QrCode as QrCodeIcon,
  ContentCopy as CopyIcon,
  Check as CheckIcon,
  Warning as WarningIcon,
  Shield as ShieldIcon,
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

  const getApiUrl = () => {
    return window.location.hostname === 'localhost'
      ? 'http://localhost:5001/api'
      : `http://${window.location.hostname}:5001/api`;
  };

  useEffect(() => {
    loadTwoFactorStatus();
  }, []);

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
