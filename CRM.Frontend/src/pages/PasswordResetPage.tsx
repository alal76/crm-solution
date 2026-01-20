import { Box, Container, Typography, Card, CardContent, TextField, Button, Alert, CircularProgress } from '@mui/material';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useSearchParams } from 'react-router-dom';
import logo from '../assets/logo.png';
import apiClient from '../services/apiClient';

function PasswordResetPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const token = searchParams.get('token');
  
  const [email, setEmail] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [step] = useState<'request' | 'reset'>(token ? 'reset' : 'request');

  const handleRequestReset = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email.trim()) {
      setError('Please enter your email address');
      return;
    }

    try {
      setLoading(true);
      await apiClient.post('/auth/request-password-reset', { email });
      setSuccess(true);
      setEmail('');
      setError(null);
      setTimeout(() => {
        setSuccess(false);
        navigate('/login');
      }, 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to send reset email');
      console.error('Error requesting password reset:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleResetPassword = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!newPassword.trim() || !confirmPassword.trim()) {
      setError('Please fill in all fields');
      return;
    }

    if (newPassword !== confirmPassword) {
      setError('Passwords do not match');
      return;
    }

    if (newPassword.length < 8) {
      setError('Password must be at least 8 characters');
      return;
    }

    try {
      setLoading(true);
      await apiClient.post('/auth/reset-password', {
        token,
        newPassword,
      });
      setSuccess(true);
      setNewPassword('');
      setConfirmPassword('');
      setError(null);
      setTimeout(() => {
        navigate('/login');
      }, 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to reset password');
      console.error('Error resetting password:', err);
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
              Reset Password
            </Typography>
          </Box>
          <CardContent sx={{ p: 4 }}>
            {loading && <CircularProgress sx={{ display: 'block', mx: 'auto', mb: 2 }} />}
            
            {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
            {success && (
              <Alert severity="success" sx={{ mb: 2 }}>
                {step === 'request' 
                  ? 'Check your email for password reset instructions'
                  : 'Password reset successful! Redirecting to login...'}
              </Alert>
            )}

            {step === 'request' ? (
              <Box component="form" onSubmit={handleRequestReset}>
                <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
                  Enter your email address and we'll send you a link to reset your password.
                </Typography>
                <TextField
                  fullWidth
                  label="Email"
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  margin="normal"
                  required
                  disabled={loading}
                />
                <Button
                  fullWidth
                  variant="contained"
                  type="submit"
                  sx={{ mt: 3, backgroundColor: '#6750A4' }}
                  disabled={loading}
                >
                  {loading ? 'Sending...' : 'Send Reset Link'}
                </Button>
                <Button
                  fullWidth
                  variant="text"
                  onClick={() => navigate('/login')}
                  sx={{ mt: 1 }}
                  disabled={loading}
                >
                  Back to Login
                </Button>
              </Box>
            ) : (
              <Box component="form" onSubmit={handleResetPassword}>
                <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
                  Enter your new password below.
                </Typography>
                <TextField
                  fullWidth
                  label="New Password"
                  type="password"
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  margin="normal"
                  required
                  disabled={loading}
                />
                <TextField
                  fullWidth
                  label="Confirm Password"
                  type="password"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  margin="normal"
                  required
                  disabled={loading}
                />
                <Button
                  fullWidth
                  variant="contained"
                  type="submit"
                  sx={{ mt: 3, backgroundColor: '#6750A4' }}
                  disabled={loading}
                >
                  {loading ? 'Resetting...' : 'Reset Password'}
                </Button>
                <Button
                  fullWidth
                  variant="text"
                  onClick={() => navigate('/login')}
                  sx={{ mt: 1 }}
                  disabled={loading}
                >
                  Back to Login
                </Button>
              </Box>
            )}
          </CardContent>
        </Card>
      </Container>
    </Box>
  );
}

export default PasswordResetPage;
