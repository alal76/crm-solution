import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { getApiUrl } from '../config/ports';
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
  useMediaQuery,
  useTheme,
} from '@mui/material';
import {
  Visibility,
  VisibilityOff,
  LockOutlined,
  EmailOutlined,
  Security as SecurityIcon,
  ArrowBack as ArrowBackIcon,
} from '@mui/icons-material';
import { useNavigate, Link as RouterLink } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { debugLog, debugWarn, debugError, debugInfo } from '../utils/debug';

// Small header logo for the login card
const HeaderLogo = React.memo(({ logoUrl }: { logoUrl: string | null }) => {
  const [logoLoaded, setLogoLoaded] = useState(false);
  const [logoError, setLogoError] = useState(false);
  
  // If no logo or error, just show lock icon
  if (!logoUrl || logoError) {
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
        }}
      >
        <LockOutlined sx={{ fontSize: 32, color: 'white' }} />
      </Box>
    );
  }
  
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
        src={logoUrl}
        alt="Company Logo"
        loading="lazy"
        onLoad={() => setLogoLoaded(true)}
        onError={() => setLogoError(true)}
        style={{
          maxWidth: 40,
          maxHeight: 40,
          objectFit: 'contain',
          opacity: logoLoaded ? 1 : 0,
          transition: 'opacity 0.3s ease',
        }}
      />
      {!logoLoaded && <LockOutlined sx={{ fontSize: 32, color: 'white' }} />}
    </Box>
  );
});

// Large side panel logo with placeholder
const SidePanelLogo = React.memo(({ logoUrl, companyName }: { logoUrl: string | null; companyName: string | null }) => {
  const [logoLoaded, setLogoLoaded] = useState(false);
  const [logoError, setLogoError] = useState(false);
  
  const showPlaceholder = !logoUrl || logoError;
  
  return (
    <Box
      sx={{
        width: '100%',
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        p: 4,
      }}
    >
      {showPlaceholder ? (
        <Box
          sx={{
            width: '100%',
            maxWidth: 280,
            aspectRatio: '1',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            backgroundColor: 'rgba(255,255,255,0.1)',
            borderRadius: 4,
            border: '3px dashed rgba(255,255,255,0.4)',
            p: 4,
          }}
        >
          <LockOutlined sx={{ fontSize: 64, color: 'rgba(255,255,255,0.5)', mb: 2 }} />
          <Typography
            variant="h6"
            sx={{
              color: 'rgba(255,255,255,0.7)',
              textAlign: 'center',
              fontWeight: 600,
              lineHeight: 1.4,
            }}
          >
            YOUR LOGO
          </Typography>
          <Typography
            variant="body2"
            sx={{
              color: 'rgba(255,255,255,0.5)',
              textAlign: 'center',
              mt: 1,
            }}
          >
            Upload via Settings → System
          </Typography>
        </Box>
      ) : (
        <Box
          component="img"
          src={logoUrl}
          alt={companyName || 'Company Logo'}
          onLoad={() => setLogoLoaded(true)}
          onError={() => setLogoError(true)}
          sx={{
            maxWidth: '100%',
            maxHeight: '80%',
            objectFit: 'contain',
            borderRadius: 3,
            opacity: logoLoaded ? 1 : 0,
            transition: 'opacity 0.3s ease',
          }}
        />
      )}
      {companyName && !showPlaceholder && (
        <Typography
          variant="h5"
          sx={{
            color: 'white',
            fontWeight: 700,
            mt: 3,
            textShadow: '0 2px 10px rgba(0,0,0,0.3)',
            textAlign: 'center',
          }}
        >
          {companyName}
        </Typography>
      )}
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
  const theme = useTheme();
  const isDesktop = useMediaQuery(theme.breakpoints.up('md'));
  
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
  
  // 2FA state
  const [showTwoFactor, setShowTwoFactor] = useState(false);
  const [twoFactorToken, setTwoFactorToken] = useState('');
  const [twoFactorCode, setTwoFactorCode] = useState('');
  
  // OAuth state
  const [oauthEnabled, setOauthEnabled] = useState(false);
  
  // Quick Admin Login state
  const [quickAdminLoginEnabled, setQuickAdminLoginEnabled] = useState(false);
  
  // Branding state
  const [branding, setBranding] = useState<{
    companyName: string | null;
    companyLogoUrl: string | null;
    companyLoginLogoUrl: string | null;
    primaryColor: string | null;
  }>({
    companyName: null,
    companyLogoUrl: null,
    companyLoginLogoUrl: null,
    primaryColor: null,
  });
  
  const { login, verifyTwoFactor, googleLogin } = useAuth();
  const navigate = useNavigate();
  const styles = useStyles();

  // Get API URL from centralized config
  const apiUrl = getApiUrl();

  // Load login settings (Quick Admin Login enabled status + branding)
  useEffect(() => {
    const loadLoginSettings = async () => {
      try {
        const response = await fetch(`${apiUrl}/systemsettings/login-settings`);
        if (response.ok) {
          const data = await response.json();
          setQuickAdminLoginEnabled(data.quickAdminLoginEnabled ?? false);
          // Load branding
          setBranding({
            companyName: data.companyName || null,
            companyLogoUrl: data.companyLogoUrl || null,
            companyLoginLogoUrl: data.companyLoginLogoUrl || null,
            primaryColor: data.primaryColor || null,
          });
        }
      } catch (err) {
        console.error('Error loading login settings:', err);
      }
    };
    loadLoginSettings();
  }, [apiUrl]);

  // Load saved email on mount
  useEffect(() => {
    setMounted(true);
    const savedEmail = localStorage.getItem('savedEmail');
    if (savedEmail && rememberMe) {
      setFormData(prev => ({ ...prev, email: savedEmail }));
    }
    // If there was a session restoration error, show it once
    try {
      const sessErr = window.sessionStorage.getItem('crm_login_error');
      if (sessErr) {
        setError(sessErr);
        window.sessionStorage.removeItem('crm_login_error');
      }
      // clear redirected flag so future 401s can redirect once more
      window.sessionStorage.removeItem('crm_redirected_to_login');
    } catch (e) {
      // ignore
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
        if (window.google?.accounts?.id) {
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

  // Prevent duplicate Google login requests
  const googleLoginInProgress = React.useRef(false);
  const handleGoogleResponse = useCallback(
    async (response: { credential?: string }) => {
      if (!response.credential || googleLoginInProgress.current) return;
      googleLoginInProgress.current = true;
      setError('');
      setLoading(true);
      try {
        await googleLogin(response.credential);
        navigate('/');
      } catch (err: unknown) {
        const error = err as {
          response?: { data?: { message?: string }; status?: number };
          request?: unknown;
          message?: string;
          code?: string;
        };
        const status = (error as any).response?.status;
        const serverMsg = (error as any).response?.data?.message;
        let errorMessage: string;
        if (!error.response && error.request) {
          errorMessage = 'Cannot reach the CRM API to complete Google sign-in. Check that the backend service is running.';
        } else if (status === 401 || status === 403) {
          errorMessage = serverMsg || 'Google account not authorised for this CRM. Contact your administrator.';
        } else if (status === 502) {
          errorMessage = 'Bad Gateway (502): The API is not reachable. Please try again shortly.';
        } else if (status === 500) {
          errorMessage = `Server error (500) during Google sign-in${serverMsg ? ': ' + serverMsg : ''}. Please try again.`;
        } else {
          errorMessage = serverMsg || (error as Error).message || 'Google sign-in failed. Please try again.';
        }
        setError(errorMessage);
        debugError('[LoginPage] Google login error', { status, errorMessage, code: error.code, error });
      } finally {
        setLoading(false);
        googleLoginInProgress.current = false;
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

  const handleQuickLogin = useCallback(() => {
    // Credentials sourced from env vars only — never hardcoded in source
    setFormData({
      email: process.env.REACT_APP_QUICK_LOGIN_EMAIL ?? 'admin@crm.local',
      password: process.env.REACT_APP_QUICK_LOGIN_PASSWORD ?? '',
    });
    setError('');
  }, []);

  // Prevent duplicate login submissions
  const loginInProgress = React.useRef(false);
  const handleSubmit = useCallback(
    async (e: React.FormEvent<HTMLFormElement>) => {
      e.preventDefault();
      if (loginInProgress.current || loading) return;
      loginInProgress.current = true;
      debugLog('[LoginPage] Submit clicked', formData);
      if (!formData.email.trim() || !formData.password) {
        setError('Please enter both email and password');
        debugWarn('[LoginPage] Missing email or password');
        loginInProgress.current = false;
        return;
      }
      setError('');
      setLoading(true);
      try {
        const result = await login(formData.email.trim(), formData.password);
        debugLog('[LoginPage] Login result', result);
        
        // Check if password setup is required (first-time login or expired)
        if (result.requiresPasswordSetup || result.passwordExpired || result.mustChangePassword) {
          navigate('/setup-password', {
            state: {
              passwordSetupToken: result.passwordSetupToken,
              isFirstTimeSetup: result.requiresPasswordSetup,
              isExpired: result.passwordExpired,
              mustChange: result.mustChangePassword,
              email: formData.email.trim(),
            },
          });
          setLoading(false);
          loginInProgress.current = false;
          debugInfo('[LoginPage] Password setup required');
          return;
        }
        
        // Check if 2FA is required
        if (result.requiresTwoFactor && result.twoFactorToken) {
          setTwoFactorToken(result.twoFactorToken);
          setShowTwoFactor(true);
          setLoading(false);
          loginInProgress.current = false;
          debugInfo('[LoginPage] 2FA required');
          return;
        }
        if (rememberMe) {
          localStorage.setItem('savedEmail', formData.email.trim());
          localStorage.setItem('rememberMe', 'true');
        } else {
          localStorage.removeItem('savedEmail');
          localStorage.removeItem('rememberMe');
        }
        
        // Show password expiration warning if applicable
        if (result.passwordExpirationWarning && result.daysUntilPasswordExpiration !== undefined) {
          // Store warning for display after login
          sessionStorage.setItem('passwordExpirationWarning', JSON.stringify({
            daysRemaining: result.daysUntilPasswordExpiration,
          }));
        }
        
        navigate('/');
      } catch (err: unknown) {
        const error = err as {
          response?: { data?: { message?: string; error?: string }; status?: number };
          request?: unknown;
          message?: string;
          code?: string;
        };
        const status = (error as any).response?.status;
        const serverMsg = (error as any).response?.data?.message || (error as any).response?.data?.error;
        let errorMessage: string;

        if (!error.response && error.request) {
          // Request made but no response — API unreachable
          if (error.code === 'ECONNABORTED' || (error as Error).message?.includes('timeout')) {
            errorMessage = 'Connection timed out. The API server is not responding — please check that the backend service is running.';
          } else {
            errorMessage = 'Cannot reach the CRM API server. The service may be down or unreachable — verify the API container is running and the network is accessible.';
          }
        } else if (!error.response) {
          // Network error before request was sent
          errorMessage = `Network error: ${(error as Error).message || 'Unknown network failure'}. Check your connection and that the CRM API is reachable.`;
        } else if (status === 400) {
          errorMessage = serverMsg || 'Invalid login request. Please check your email and password format.';
        } else if (status === 401) {
          errorMessage = serverMsg || 'Invalid email or password. Please try again.';
        } else if (status === 403) {
          errorMessage = serverMsg || 'Your account does not have permission to log in. Contact your administrator.';
        } else if (status === 423) {
          errorMessage = serverMsg || 'Your account is locked due to too many failed attempts. Please wait or contact your administrator.';
        } else if (status === 429) {
          errorMessage = 'Too many login attempts. Please wait a moment before trying again.';
        } else if (status === 500) {
          errorMessage = `Internal server error (500). The API encountered an unexpected error${serverMsg ? ': ' + serverMsg : ' — this may indicate a database connectivity issue'}. Please try again or contact support.`;
        } else if (status === 502) {
          errorMessage = 'Bad Gateway (502): The API gateway could not reach the backend service. The API container may be down, restarting, or misconfigured. Check that crm-api is running.';
        } else if (status === 503) {
          errorMessage = 'Service Unavailable (503): The CRM API is temporarily unavailable — it may be starting up or overloaded. Please wait a moment and retry.';
        } else if (status === 504) {
          errorMessage = 'Gateway Timeout (504): The API did not respond in time. The database or a dependent service may be slow or unresponsive.';
        } else if (status) {
          errorMessage = `Login failed with status ${status}${serverMsg ? ': ' + serverMsg : ''}. Please try again or contact support.`;
        } else {
          errorMessage = (error as Error).message || 'An unexpected error occurred. Please try again.';
        }

        setError(errorMessage);
        debugError('[LoginPage] Login error', { status, errorMessage, code: error.code, serverMsg, error });
      } finally {
        setLoading(false);
        loginInProgress.current = false;
        debugLog('[LoginPage] Login finished');
      }
    },
    [formData, login, navigate, rememberMe, loading]
  );

  const handleTwoFactorSubmit = useCallback(
    async (e: React.FormEvent<HTMLFormElement>) => {
      e.preventDefault();
      
      if (!twoFactorCode.trim()) {
        setError('Please enter the verification code');
        return;
      }

      setError('');
      setLoading(true);

      try {
        await verifyTwoFactor(twoFactorToken, twoFactorCode.trim());
        
        if (rememberMe) {
          localStorage.setItem('savedEmail', formData.email.trim());
          localStorage.setItem('rememberMe', 'true');
        }
        
        navigate('/');
      } catch (err: unknown) {
        const error = err as {
          response?: { data?: { message?: string }; status?: number };
          request?: unknown;
          message?: string;
          code?: string;
        };
        const status = (error as any).response?.status;
        const serverMsg = (error as any).response?.data?.message;
        let errorMessage: string;

        if (!error.response && error.request) {
          errorMessage = 'Cannot reach the CRM API to verify the code. Check that the backend service is running.';
        } else if (status === 400 || status === 401) {
          errorMessage = serverMsg || 'Invalid or expired verification code. Please try again.';
        } else if (status === 429) {
          errorMessage = 'Too many verification attempts. Please wait before retrying.';
        } else if (status === 500) {
          errorMessage = `Server error (500) during 2FA verification${serverMsg ? ': ' + serverMsg : ''}. Please try again.`;
        } else if (status === 502) {
          errorMessage = 'Bad Gateway (502): Cannot reach the authentication service. Please try again shortly.';
        } else if (status === 503 || status === 504) {
          errorMessage = `Service temporarily unavailable (${status}). Please wait a moment and retry.`;
        } else if (status) {
          errorMessage = serverMsg || `Verification failed (${status}). Please try again.`;
        } else {
          errorMessage = serverMsg || 'Invalid verification code. Please try again.';
        }

        setError(errorMessage);
        debugError('[LoginPage] 2FA verification error', { status, errorMessage, code: error.code, error });
      } finally {
        setLoading(false);
      }
    },
    [twoFactorCode, twoFactorToken, verifyTwoFactor, navigate, rememberMe, formData.email]
  );

  const handleBackToLogin = useCallback(() => {
    setShowTwoFactor(false);
    setTwoFactorToken('');
    setTwoFactorCode('');
    setError('');
  }, []);

  const togglePassword = useCallback(() => {
    setShowPassword(prev => !prev);
  }, []);

  const handleRememberMe = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setRememberMe(e.target.checked);
  }, []);

  return (
    <Box sx={styles.container}>
      <Box
        sx={{
          display: 'flex',
          width: '100%',
          maxWidth: isDesktop ? 900 : 440,
          minHeight: isDesktop ? 'auto' : undefined,
          alignItems: 'stretch',
          justifyContent: 'center',
          borderRadius: 4,
          overflow: 'hidden',
          boxShadow: '0 20px 60px rgba(0, 0, 0, 0.3)',
        }}
      >
        {/* Login Form Side */}
        <Box
          sx={{ 
            flex: isDesktop ? '0 0 50%' : '1',
            display: 'flex',
            flexDirection: 'column',
          }}
        >
          <Fade in={mounted} timeout={600}>
            <Card sx={{ ...styles.card, height: '100%', maxWidth: 'none', borderRadius: isDesktop ? 0 : 4 }}>
              <Box sx={styles.header}>
                <HeaderLogo logoUrl={branding.companyLogoUrl} />
                <Typography
                  variant="h4"
                  component="h1"
                  sx={{ fontWeight: 700, mb: 0.5, letterSpacing: '-0.5px' }}
                >
                  {showTwoFactor ? 'Two-Factor Authentication' : 'Welcome Back'}
              </Typography>
              <Typography variant="body2" sx={{ opacity: 0.85 }}>
                {showTwoFactor 
                  ? 'Enter the code from your authenticator app' 
                  : 'Sign in to continue to your CRM'}
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

              {showTwoFactor ? (
                /* Two-Factor Authentication Form */
                <Box component="form" onSubmit={handleTwoFactorSubmit} noValidate>
                  <Box sx={{ textAlign: 'center', mb: 3 }}>
                    <SecurityIcon sx={{ fontSize: 48, color: '#5E35B1', mb: 1 }} />
                    <Typography variant="body2" color="text.secondary">
                      Open your authenticator app and enter the 6-digit code, or use a backup code.
                    </Typography>
                  </Box>

                  <TextField
                    fullWidth
                    label="Verification Code"
                    value={twoFactorCode}
                    onChange={(e) => setTwoFactorCode(e.target.value.replaceAll(/\D/g, '').slice(0, 6))}
                    disabled={loading}
                    margin="normal"
                    autoComplete="one-time-code"
                    autoFocus
                    placeholder="000000"
                    inputProps={{
                      maxLength: 6,
                      style: { textAlign: 'center', fontSize: '1.5rem', letterSpacing: '0.5rem' },
                    }}
                    InputProps={{
                      startAdornment: (
                        <InputAdornment position="start">
                          <SecurityIcon sx={{ color: 'action.active' }} />
                        </InputAdornment>
                      ),
                    }}
                    sx={styles.input}
                  />

                  <Button
                    fullWidth
                    variant="contained"
                    size="large"
                    type="submit"
                    disabled={loading || twoFactorCode.length < 6}
                    sx={{ ...styles.button, mt: 3 }}
                  >
                    {loading ? (
                      <CircularProgress size={24} sx={{ color: 'white' }} />
                    ) : (
                      'Verify'
                    )}
                  </Button>

                  <Button
                    fullWidth
                    variant="text"
                    startIcon={<ArrowBackIcon />}
                    onClick={handleBackToLogin}
                    disabled={loading}
                    sx={{ mt: 2, textTransform: 'none', color: '#5E35B1' }}
                  >
                    Back to Login
                  </Button>
                </Box>
              ) : (
                /* Regular Login Form */
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

                {quickAdminLoginEnabled && (
                <Button
                  fullWidth
                  variant="outlined"
                  size="medium"
                  onClick={loading ? undefined : handleQuickLogin}
                  disabled={loading}
                  sx={{
                    mb: 2,
                    textTransform: 'none',
                    borderColor: '#5E35B1',
                    color: '#5E35B1',
                    '&:hover': {
                      borderColor: '#4527A0',
                      backgroundColor: 'rgba(94, 53, 177, 0.04)',
                    },
                  }}
                >
                  Quick Admin Login
                </Button>
                )}

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
              )}

              {!showTwoFactor && oauthEnabled && (
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

              {!showTwoFactor && (
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
              )}
            </CardContent>
          </Card>
        </Fade>
      </Box>

      {/* Login Logo Panel - shown on desktop (always visible with placeholder or logo) */}
      {isDesktop && (
        <Fade in={mounted} timeout={800}>
          <Box
            sx={{
              flex: '0 0 50%',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              backgroundColor: 'rgba(255,255,255,0.08)',
              backdropFilter: 'blur(10px)',
              minHeight: 500,
            }}
          >
            <SidePanelLogo 
              logoUrl={branding.companyLoginLogoUrl} 
              companyName={branding.companyName}
            />
          </Box>
        </Fade>
      )}
      </Box>

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
        © {new Date().getFullYear()} {branding.companyName || 'CRM System'}. All rights reserved.
      </Typography>
    </Box>
  );
};

export default React.memo(LoginPage);
