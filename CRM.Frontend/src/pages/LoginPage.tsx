import React, { useState, useEffect } from 'react';
import {
  Container,
  TextField,
  Button,
  Box,
  Typography,
  Alert,
  CircularProgress,
  InputAdornment,
  IconButton,
  Link as MuiLink,
  Card,
  CardContent,
  Divider,
  FormControlLabel,
  Checkbox,
} from '@mui/material';
import { Visibility, VisibilityOff } from '@mui/icons-material';
import { useNavigate, Link as RouterLink } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [rememberMe, setRememberMe] = useState(false);
  const [oauthEnabled, setOauthEnabled] = useState(false);
  const { login, googleLogin } = useAuth();
  const navigate = useNavigate();

  const googleClientId = process.env.REACT_APP_GOOGLE_CLIENT_ID;

  useEffect(() => {
    const hasOAuthConfig = googleClientId;
    setOauthEnabled(!!hasOAuthConfig);

    if (!googleClientId) {
      console.warn('Google OAuth: REACT_APP_GOOGLE_CLIENT_ID not configured');
      return;
    }

    const script = document.createElement('script');
    script.src = 'https://accounts.google.com/gsi/client';
    script.async = true;
    script.defer = true;
    document.head.appendChild(script);

    script.onload = () => {
      if (window.google) {
        window.google.accounts.id.initialize({
          client_id: googleClientId,
          callback: handleGoogleResponse,
        });
        const googleButton = document.getElementById('google-login-button');
        if (googleButton) {
          window.google.accounts.id.renderButton(googleButton, {
            theme: 'outline',
            size: 'large',
            width: '100%',
          });
        }
      }
    };

    return () => {
      if (document.head.contains(script)) {
        document.head.removeChild(script);
      }
    };
  }, [googleClientId]);

  const handleGoogleResponse = async (response: any) => {
    if (response.credential) {
      setError('');
      setLoading(true);
      try {
        await googleLogin(response.credential);
        navigate('/');
      } catch (err: any) {
        setError(err.response?.data?.message || 'Google login failed');
        setLoading(false);
      }
    }
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      await login(email, password);
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data?.message || err.response?.data?.error || 'Login failed. Please check your credentials.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'linear-gradient(135deg, #6750A4 0%, #A085D3 100%)',
        py: 3,
        px: 2,
      }}
    >
      <Container maxWidth="sm">
        <Card
          sx={{
            borderRadius: 3,
            boxShadow: '0px 12px 24px rgba(0, 0, 0, 0.15)',
            overflow: 'hidden',
          }}
        >
          <Box
            sx={{
              background: 'linear-gradient(135deg, #6750A4 0%, #7D5B8D 100%)',
              color: 'white',
              p: 3,
              textAlign: 'center',
            }}
          >
            <Typography variant="h4" component="h1" sx={{ fontWeight: 700, mb: 1 }}>
              Welcome Back
            </Typography>
            <Typography variant="body2" sx={{ opacity: 0.9 }}>
              Sign in to your CRM account
            </Typography>
          </Box>

          <CardContent sx={{ p: 4 }}>
            {/* Error Alert */}
            {error && (
              <Alert
                severity="error"
                sx={{
                  mb: 3,
                  borderRadius: 2,
                  fontWeight: 500,
                }}
              >
                {error}
              </Alert>
            )}

            {/* Login Form */}
            <Box component="form" onSubmit={handleSubmit} sx={{ mb: 3 }}>
              <TextField
                fullWidth
                label="Email or Username"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                disabled={loading}
                margin="normal"
                variant="outlined"
                placeholder="you@example.com"
                required
                InputProps={{
                  sx: { borderRadius: 2 },
                }}
              />

              <TextField
                fullWidth
                label="Password"
                type={showPassword ? 'text' : 'password'}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                disabled={loading}
                margin="normal"
                variant="outlined"
                placeholder="••••••••"
                required
                InputProps={{
                  sx: { borderRadius: 2 },
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => setShowPassword(!showPassword)}
                        edge="end"
                        disabled={loading}
                        sx={{
                          '&:hover': {
                            backgroundColor: 'rgba(103, 80, 164, 0.08)',
                          },
                        }}
                      >
                        {showPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />

              <Box sx={{ mt: 2, mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={rememberMe}
                      onChange={(e) => setRememberMe(e.target.checked)}
                      disabled={loading}
                      size="small"
                    />
                  }
                  label={<Typography variant="body2">Remember me</Typography>}
                />
                <MuiLink
                  component={RouterLink}
                  to="/password-reset"
                  sx={{
                    textDecoration: 'none',
                    fontSize: '0.9rem',
                    fontWeight: 500,
                    color: '#6750A4',
                    '&:hover': { textDecoration: 'underline' },
                  }}
                >
                  Forgot password?
                </MuiLink>
              </Box>

              <Button
                fullWidth
                variant="contained"
                size="large"
                type="submit"
                disabled={loading}
                sx={{
                  textTransform: 'none',
                  fontSize: '1rem',
                  fontWeight: 600,
                  py: 1.5,
                  borderRadius: 3,
                  background: 'linear-gradient(135deg, #6750A4 0%, #7D5B8D 100%)',
                  boxShadow: '0px 4px 12px rgba(103, 80, 164, 0.3)',
                  '&:hover': {
                    boxShadow: '0px 8px 20px rgba(103, 80, 164, 0.4)',
                  },
                  '&:disabled': {
                    background: '#CAC7D0',
                  },
                }}
              >
                {loading ? <CircularProgress size={24} color="inherit" /> : 'Sign In'}
              </Button>
            </Box>

            {/* Divider */}
            {oauthEnabled && (
              <>
                <Divider sx={{ my: 3, fontWeight: 500 }}>Or continue with</Divider>

                {/* Google Login */}
                <Box sx={{ mb: 3 }}>
                  <div id="google-login-button" style={{ width: '100%' }}></div>
                </Box>
              </>
            )}

            {/* Sign Up Link */}
            <Box sx={{ textAlign: 'center', mt: 3, pt: 3, borderTop: '1px solid #E0E0E0' }}>
              <Typography variant="body2">
                Don't have an account?{' '}
                <MuiLink
                  component={RouterLink}
                  to="/register"
                  sx={{
                    textDecoration: 'none',
                    fontWeight: 700,
                    color: '#6750A4',
                    '&:hover': {
                      textDecoration: 'underline',
                    },
                  }}
                >
                  Create one
                </MuiLink>
              </Typography>
            </Box>
          </CardContent>
        </Card>

        {/* Footer */}
        <Box sx={{ textAlign: 'center', mt: 4, color: 'rgba(255, 255, 255, 0.8)' }}>
          <Typography variant="caption" sx={{ fontSize: '0.8rem' }}>
            © 2026 CRM System. All rights reserved.
          </Typography>
        </Box>
      </Container>
    </Box>
  );
}

export default LoginPage;

