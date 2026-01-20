import { Box, Container, Typography, Card, CardContent, TextField, Button, Alert, CircularProgress, FormControlLabel, Checkbox } from '@mui/material';
import { useState, useEffect } from 'react';
import logo from '../assets/logo.png';
import apiClient from '../services/apiClient';

function TwoFactorPage() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [verificationCode, setVerificationCode] = useState('');
  const [isVerifying, setIsVerifying] = useState(false);
  const [enabled, setEnabled] = useState(false);

  useEffect(() => {
    checkTwoFactorStatus();
  }, []);

  const checkTwoFactorStatus = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/auth/2fa-status');
      setEnabled(response.data.enabled);
      setError(null);
    } catch (err: any) {
      console.error('Error checking 2FA status:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleEnableTwoFactor = async () => {
    try {
      setLoading(true);
      const response = await apiClient.post('/auth/enable-2fa');
      // In a real scenario, QR code would be displayed here
      setSuccess(true);
      setIsVerifying(true);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to enable 2FA');
      console.error('Error enabling 2FA:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDisableTwoFactor = async () => {
    if (window.confirm('Are you sure you want to disable two-factor authentication?')) {
      try {
        setLoading(true);
        await apiClient.post('/auth/disable-2fa');
        setEnabled(false);
        setSuccess(true);
        setError(null);
        setTimeout(() => setSuccess(false), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to disable 2FA');
        console.error('Error disabling 2FA:', err);
      } finally {
        setLoading(false);
      }
    }
  };

  const handleVerifyCode = async () => {
    if (!verificationCode.trim()) {
      setError('Please enter the verification code');
      return;
    }

    try {
      setLoading(true);
      await apiClient.post('/auth/verify-2fa', { code: verificationCode });
      setEnabled(true);
      setIsVerifying(false);
      setVerificationCode('');
      setSuccess(true);
      setError(null);
      setTimeout(() => setSuccess(false), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Invalid verification code');
      console.error('Error verifying code:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box sx={{ minHeight: '100vh', display: 'flex', alignItems: 'center', background: 'linear-gradient(135deg, #6750A4 0%, #A085D3 100%)', py: 3 }}>
      <Container maxWidth="sm">
        <Card sx={{ borderRadius: 3 }}>
          <Box
            sx={{
              background: 'linear-gradient(135deg, #6750A4 0%, #7D5B8D 100%)',
              color: 'white',
              p: 3,
              textAlign: 'center',
            }}
          >
            <Box sx={{ mb: 2, display: 'flex', justifyContent: 'center', width: 50, height: 50, mx: 'auto' }}>
              <img src={logo} alt="CRM Logo" style={{ width: '100%', height: '100%', objectFit: 'contain' }} />
            </Box>
            <Typography variant="h5" sx={{ fontWeight: 700 }}>
              Two-Factor Authentication
            </Typography>
          </Box>
          <CardContent sx={{ p: 4 }}>
            {loading && <CircularProgress sx={{ display: 'block', mx: 'auto', mb: 2 }} />}
            
            {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
            {success && <Alert severity="success" sx={{ mb: 2 }}>Two-factor authentication updated successfully!</Alert>}

            {!isVerifying ? (
              <>
                <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
                  Enhance your account security with two-factor authentication. You'll need to verify with a code from your authenticator app when you log in.
                </Typography>

                {enabled ? (
                  <Box>
                    <Box sx={{ p: 2, backgroundColor: '#E8F5E9', borderRadius: 1, mb: 2 }}>
                      <Typography variant="body2" sx={{ color: '#2E7D32', fontWeight: 500 }}>
                        ✓ Two-factor authentication is enabled
                      </Typography>
                    </Box>
                    <Button
                      fullWidth
                      variant="outlined"
                      color="error"
                      onClick={handleDisableTwoFactor}
                      disabled={loading}
                    >
                      Disable 2FA
                    </Button>
                  </Box>
                ) : (
                  <Button
                    fullWidth
                    variant="contained"
                    sx={{ backgroundColor: '#6750A4' }}
                    onClick={handleEnableTwoFactor}
                    disabled={loading}
                  >
                    Enable Two-Factor Authentication
                  </Button>
                )}
              </>
            ) : (
              <Box>
                <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
                  Scan the QR code with your authenticator app and enter the code:
                </Typography>
                <TextField
                  fullWidth
                  label="Verification Code"
                  value={verificationCode}
                  onChange={(e) => setVerificationCode(e.target.value)}
                  margin="normal"
                  placeholder="000000"
                  inputProps={{ maxLength: 6, pattern: '[0-9]*' }}
                />
                <Button
                  fullWidth
                  variant="contained"
                  sx={{ backgroundColor: '#6750A4', mt: 2 }}
                  onClick={handleVerifyCode}
                  disabled={loading}
                >
                  Verify Code
                </Button>
                <Button
                  fullWidth
                  variant="outlined"
                  sx={{ mt: 1 }}
                  onClick={() => setIsVerifying(false)}
                  disabled={loading}
                >
                  Cancel
                </Button>
              </Box>
            )}
          </CardContent>
        </Card>
      </Container>
    </Box>
  );
}

export default TwoFactorPage;
