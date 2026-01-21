import React, { useState, useEffect, useCallback, useMemo } from 'react';
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
  Fade,
  Collapse,
} from '@mui/material';
import {
  Visibility,
  VisibilityOff,
  LockOutlined,
  EmailOutlined,
} from '@mui/icons-material';
import { useNavigate, Link as RouterLink } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

// Lazy load logo for faster initial render
const Logo = React.memo(() => {
  const [logoLoaded, setLogoLoaded] = useState(false);
  
  return (
    <Box
      sx={{
        width: 60,
        height: 60,
        mx: 'auto',
        mb: 2,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: 'rgba(255,255,255,0.15)',
        borderRadius: '50%',
        transition: 'transform 0.3s ease',
        '&:hover': { transform: 'scale(1.05)' },
      }}
    >
      <img
        src="/logo.png"
        alt="CRM"
        loading="lazy"
        onLoad={() => setLogoLoaded(true)}
        style={{
          width: 40,
          height: 40,
          objectFit: 'contain',
          opacity: logoLoaded ? 1 : 0,
          transition: 'opacity 0.3s ease',
        }}
        onError={(e) => {
          (e.target as HTMLImageElement).style.display = 'none';
        }}
      />
      {!logoLoaded && <LockOutlined sx={{ fontSize: 32, color: 'white' }} />}
    </Box>
  );
});

// Memoized styles for better performance
const useStyles = () =>
  useMemo(
    () => ({
      container: {
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'linear-gradient(135deg, #6750A4 0%, #7E57C2 100%)',
        py: 3,
        px: 2,
      },
      card: {
        borderRadius: 4,
        boxShadow: '0 20px 60px rgba(0, 0, 0, 0.2)',
        overflow: 'hidden',
        maxWidth: 440,
        width: '100%',
        backgroundColor: '#ffffff',
      },
      header: {
        background: 'linear-gradient(135deg, #5E35B1 0%, #7E57C2 100%)',
        color: 'white',
        p: 4,
        textAlign: 'center' as const,
      },
      input: {
        '& .MuiOutlinedInput-root': {
          borderRadius: 2,
        },
      },
      button: {
        textTransform: 'none' as const,
        fontSize: '1rem',
        fontWeight: 600,
        py: 1.5,
        borderRadius: 3,
        background: 'linear-gradient(135deg, #5E35B1 0%, #7E57C2 100%)',
        boxShadow: '0 4px 15px rgba(94, 53, 177, 0.35)',
        '&:hover': {
          background: 'linear-gradient(135deg, #4527A0 0%, #6A1B9A 100%)',
          boxShadow: '0 6px 20px rgba(94, 53, 177, 0.45)',
        },
        '&:active': {
          background: 'linear-gradient(135deg, #4527A0 0%, #6A1B9A 100%)',
        },
        '&:disabled': {
          background: '#B8B5BD',
          boxShadow: 'none',
        },
      },
    }),
    []
  );

const LoginPage: React.FC = () => {
  // Form state
  const [formData, setFormData] = useState({ email: '', password: '' });
  const [showPassword, setShowPassword] = useState(false);
  const [rememberMe, setRememberMe] = useState(() => {
    return localStorage.getItem('rememberMe') === 'true';
  });
  
  // UI state
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [mounted, setMounted] = useState(false);
  
  // OAuth state
  const [oauthEnabled, setOauthEnabled] = useState(false);
  
  const { login, googleLogin } = useAuth();
  const navigate = useNavigate();
  const styles = useStyles();

  // Load saved email on mount
  useEffect(() => {
    setMounted(true);
    const savedEmail = localStorage.getItem('savedEmail');
    if (savedEmail && rememberMe) {
      setFormData(prev => ({ ...prev, email: savedEmail }));
    }
  }, [rememberMe]);

  // Initialize Google OAuth lazily
  useEffect(() => {
    const googleClientId = process.env.REACT_APP_GOOGLE_CLIENT_ID;
    if (!googleClientId) return;

    setOauthEnabled(true);

    // Defer script loading for faster initial render
    const timer = setTimeout(() => {
      const script = document.createElement('script');
      script.src = 'https://accounts.google.com/gsi/client';
      script.async = true;
      script.defer = true;
      
      script.onload = () => {
        if (window.google) {
          window.google.accounts.id.initialize({
            client_id: googleClientId,
            callback: handleGoogleResponse,
          });
          const btn = document.getElementById('google-login-button');
          if (btn) {
            window.google.accounts.id.renderButton(btn, {
              theme: 'outline',
              size: 'large',
              width: 380,
            });
          }
        }
      };
      
      document.head.appendChild(script);
    }, 500);

    return () => clearTimeout(timer);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleGoogleResponse = useCallback(
    async (response: { credential?: string }) => {
      if (!response.credential) return;
      
      setError('');
      setLoading(true);
      
      try {
        await googleLogin(response.credential);
        navigate('/');
      } catch (err: unknown) {
        const error = err as { response?: { data?: { message?: string } } };
        setError(error.response?.data?.message || 'Google login failed');
      } finally {
        setLoading(false);
      }
    },
    [googleLogin, navigate]
  );

  const handleInputChange = useCallback(
    (field: 'email' | 'password') => (e: React.ChangeEvent<HTMLInputElement>) => {
      setFormData(prev => ({ ...prev, [field]: e.target.value }));
      if (error) setError('');
    },
    [error]
  );

  const handleSubmit = useCallback(
    async (e: React.FormEvent<HTMLFormElement>) => {
      e.preventDefault();
      
      if (!formData.email.trim() || !formData.password) {
        setError('Please enter both email and password');
        return;
      }

      setError('');
      setLoading(true);

      try {
        await login(formData.email.trim(), formData.password);
        
        if (rememberMe) {
          localStorage.setItem('savedEmail', formData.email.trim());
          localStorage.setItem('rememberMe', 'true');
        } else {
          localStorage.removeItem('savedEmail');
          localStorage.removeItem('rememberMe');
        }
        
        navigate('/');
      } catch (err: unknown) {
        const error = err as { 
          response?: { data?: { message?: string; error?: string }; status?: number };
          message?: string;
        };
        
        let errorMessage = 'Login failed. Please check your credentials.';
        
        if (error.response?.status === 401) {
          errorMessage = 'Invalid email or password';
        } else if (error.response?.data?.message) {
          errorMessage = error.response.data.message;
        } else if (error.response?.data?.error) {
          errorMessage = error.response.data.error;
        } else if (error.message) {
          errorMessage = error.message;
        }
        
        setError(errorMessage);
      } finally {
        setLoading(false);
      }
    },
    [formData, login, navigate, rememberMe]
  );

  const togglePassword = useCallback(() => {
    setShowPassword(prev => !prev);
  }, []);

  const handleRememberMe = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setRememberMe(e.target.checked);
  }, []);

  return (
    <Box sx={styles.container}>
      <Container maxWidth="sm" sx={{ display: 'flex', justifyContent: 'center' }}>
        <Fade in={mounted} timeout={600}>
          <Card sx={styles.card}>
            <Box sx={styles.header}>
              <Logo />
              <Typography
                variant="h4"
                component="h1"
                sx={{ fontWeight: 700, mb: 0.5, letterSpacing: '-0.5px' }}
              >
                Welcome Back
              </Typography>
              <Typography variant="body2" sx={{ opacity: 0.85 }}>
                Sign in to continue to your CRM
              </Typography>
            </Box>

            <CardContent sx={{ p: 4 }}>
              <Collapse in={!!error}>
                <Alert
                  severity="error"
                  onClose={() => setError('')}
                  sx={{ mb: 3, borderRadius: 2 }}
                >
                  {error}
                </Alert>
              </Collapse>

              <Box component="form" onSubmit={handleSubmit} noValidate>
                <TextField
                  fullWidth
                  label="Email or Username"
                  type="email"
                  value={formData.email}
                  onChange={handleInputChange('email')}
                  disabled={loading}
                  margin="normal"
                  autoComplete="email"
                  autoFocus
                  placeholder="you@company.com"
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <EmailOutlined sx={{ color: 'action.active' }} />
                      </InputAdornment>
                    ),
                  }}
                  sx={styles.input}
                />

                <TextField
                  fullWidth
                  label="Password"
                  type={showPassword ? 'text' : 'password'}
                  value={formData.password}
                  onChange={handleInputChange('password')}
                  disabled={loading}
                  margin="normal"
                  autoComplete="current-password"
                  placeholder="••••••••"
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <LockOutlined sx={{ color: 'action.active' }} />
                      </InputAdornment>
                    ),
                    endAdornment: (
                      <InputAdornment position="end">
                        <IconButton
                          onClick={togglePassword}
                          edge="end"
                          disabled={loading}
                          size="small"
                          aria-label={showPassword ? 'Hide password' : 'Show password'}
                        >
                          {showPassword ? <VisibilityOff /> : <Visibility />}
                        </IconButton>
                      </InputAdornment>
                    ),
                  }}
                  sx={styles.input}
                />

                <Box
                  sx={{
                    mt: 2,
                    mb: 3,
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    flexWrap: 'wrap',
                    gap: 1,
                  }}
                >
                  <FormControlLabel
                    control={
                      <Checkbox
                        checked={rememberMe}
                        onChange={handleRememberMe}
                        disabled={loading}
                        size="small"
                        sx={{
                          color: '#7E57C2',
                          '&.Mui-checked': { color: '#5E35B1' },
                        }}
                      />
                    }
                    label={
                      <Typography variant="body2" color="text.secondary">
                        Remember me
                      </Typography>
                    }
                  />
                  <MuiLink
                    component={RouterLink}
                    to="/password-reset"
                    sx={{
                      textDecoration: 'none',
                      fontSize: '0.875rem',
                      fontWeight: 500,
                      color: '#5E35B1',
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
                  disabled={loading || !formData.email || !formData.password}
                  sx={styles.button}
                >
                  {loading ? (
                    <CircularProgress size={24} sx={{ color: 'white' }} />
                  ) : (
                    'Sign In'
                  )}
                </Button>
              </Box>

              {oauthEnabled && (
                <Fade in timeout={800}>
                  <Box>
                    <Divider sx={{ my: 3 }}>
                      <Typography variant="body2" color="text.secondary">
                        or
                      </Typography>
                    </Divider>
                    <Box id="google-login-button" sx={{ display: 'flex', justifyContent: 'center' }} />
                  </Box>
                </Fade>
              )}

              <Box
                sx={{
                  textAlign: 'center',
                  mt: 4,
                  pt: 3,
                  borderTop: '1px solid',
                  borderColor: 'divider',
                }}
              >
                <Typography variant="body2" color="text.secondary">
                  Don't have an account?{' '}
                  <MuiLink
                    component={RouterLink}
                    to="/register"
                    sx={{
                      textDecoration: 'none',
                      fontWeight: 600,
                      color: '#5E35B1',
                      '&:hover': { textDecoration: 'underline' },
                    }}
                  >
                    Create one
                  </MuiLink>
                </Typography>
              </Box>
            </CardContent>
          </Card>
        </Fade>
      </Container>

      <Typography
        variant="caption"
        sx={{
          position: 'fixed',
          bottom: 16,
          left: 0,
          right: 0,
          textAlign: 'center',
          color: 'rgba(255,255,255,0.7)',
        }}
      >
        © {new Date().getFullYear()} CRM System. All rights reserved.
      </Typography>
    </Box>
  );
};

export default React.memo(LoginPage);
