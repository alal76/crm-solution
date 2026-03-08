import React, { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  TextField,
  Button,
  Alert,
  CircularProgress,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  InputAdornment,
  IconButton,
} from '@mui/material';
import {
  Check as CheckIcon,
  Close as CloseIcon,
  Visibility,
  VisibilityOff,
  LockOutlined,
  WarningAmber,
} from '@mui/icons-material';
import { useNavigate, useLocation } from 'react-router-dom';
import logo from '../assets/logo.png';
import apiClient from '../services/apiClient';

interface PasswordRequirements {
  minLength: number;
  maxLength: number;
  requireUppercase: boolean;
  requireLowercase: boolean;
  requireNumbers: boolean;
  requireSpecialChars: boolean;
}

interface LocationState {
  passwordSetupToken?: string;
  isFirstTimeSetup?: boolean;
  isExpired?: boolean;
  mustChange?: boolean;
  email?: string;
}

function SetupPasswordPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const state = location.state as LocationState | null;

  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [loadingRequirements, setLoadingRequirements] = useState(true);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [requirements, setRequirements] = useState<PasswordRequirements>({
    minLength: 8,
    maxLength: 128,
    requireUppercase: true,
    requireLowercase: true,
    requireNumbers: true,
    requireSpecialChars: false,
  });

  // If no token in state, redirect to login
  useEffect(() => {
    if (!state?.passwordSetupToken) {
      navigate('/login');
      return;
    }

    // Fetch password requirements
    const fetchRequirements = async () => {
      try {
        const response = await apiClient.get('/auth/password-requirements');
        setRequirements(response.data);
      } catch (err) {
        console.error('Failed to fetch password requirements:', err);
        // Use defaults if fetch fails
      } finally {
        setLoadingRequirements(false);
      }
    };

    fetchRequirements();
  }, [state, navigate]);

  // Password validation checks
  const validations = {
    length: newPassword.length >= requirements.minLength && 
            (requirements.maxLength === 0 || newPassword.length <= requirements.maxLength),
    uppercase: !requirements.requireUppercase || /[A-Z]/.test(newPassword),
    lowercase: !requirements.requireLowercase || /[a-z]/.test(newPassword),
    number: !requirements.requireNumbers || /\d/.test(newPassword),
    special: !requirements.requireSpecialChars || /[^A-Za-z0-9]/.test(newPassword),
    match: confirmPassword.length > 0 && newPassword === confirmPassword,
  };

  const isValid = Object.values(validations).every(Boolean) && newPassword.length > 0;

  const getTitle = () => {
    if (state?.isFirstTimeSetup) return 'Set Up Your Password';
    if (state?.isExpired) return 'Password Expired';
    if (state?.mustChange) return 'Password Change Required';
    return 'Set New Password';
  };

  const getDescription = () => {
    if (state?.isFirstTimeSetup) {
      return 'Welcome! Please create a secure password for your account.';
    }
    if (state?.isExpired) {
      return 'Your password has expired. Please create a new password to continue.';
    }
    if (state?.mustChange) {
      return 'You are required to change your password before continuing.';
    }
    return 'Please create a new password for your account.';
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!isValid) {
      setError('Please ensure all password requirements are met');
      return;
    }

    try {
      setLoading(true);
      setError(null);

      await apiClient.post('/auth/setup-password', {
        passwordSetupToken: state?.passwordSetupToken,
        newPassword,
        confirmPassword,
      });

      setSuccess(true);
      setTimeout(() => {
        navigate('/login', {
          state: { message: 'Password set successfully. Please log in with your new password.' },
        });
      }, 2000);
    } catch (err: unknown) {
      setError((err as any).response?.data?.message || 'Failed to set password');
      console.error('Error setting password:', err);
    } finally {
      setLoading(false);
    }
  };

  if (loadingRequirements) {
    return (
      <Box
        sx={{
          minHeight: '100vh',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          background: 'linear-gradient(135deg, #6750A4 0%, #A085D3 100%)',
        }}
      >
        <CircularProgress sx={{ color: 'white' }} />
      </Box>
    );
  }

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        background: 'linear-gradient(135deg, #6750A4 0%, #A085D3 100%)',
        py: 3,
      }}
    >
      <Container maxWidth="sm">
        <Card sx={{ borderRadius: 3 }}>
          <Box
            sx={{
              background: state?.isExpired || state?.mustChange
                ? 'linear-gradient(135deg, #F57C00 0%, #FF9800 100%)'
                : 'linear-gradient(135deg, #6750A4 0%, #7D5B8D 100%)',
              color: 'white',
              p: 3,
              textAlign: 'center',
            }}
          >
            <Box
              sx={{
                mb: 2,
                display: 'flex',
                justifyContent: 'center',
                width: 60,
                height: 60,
                mx: 'auto',
                backgroundColor: 'rgba(255,255,255,0.15)',
                borderRadius: '50%',
                alignItems: 'center',
              }}
            >
              {state?.isExpired || state?.mustChange ? (
                <WarningAmber sx={{ fontSize: 32 }} />
              ) : (
                <img
                  src={logo}
                  alt="CRM Logo"
                  style={{ width: 40, height: 40, objectFit: 'contain' }}
                  onError={(e) => {
                    e.currentTarget.style.display = 'none';
                  }}
                />
              )}
            </Box>
            <Typography variant="h5" sx={{ fontWeight: 700 }}>
              {getTitle()}
            </Typography>
            {state?.email && (
              <Typography variant="body2" sx={{ mt: 1, opacity: 0.9 }}>
                {state.email}
              </Typography>
            )}
          </Box>

          <CardContent sx={{ p: 4 }}>
            {error && (
              <Alert severity="error" sx={{ mb: 2 }}>
                {error}
              </Alert>
            )}
            {success && (
              <Alert severity="success" sx={{ mb: 2 }}>
                Password set successfully! Redirecting to login...
              </Alert>
            )}

            <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
              {getDescription()}
            </Typography>

            <Box component="form" onSubmit={handleSubmit}>
              <TextField
                fullWidth
                label="New Password"
                type={showPassword ? 'text' : 'password'}
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                margin="normal"
                required
                disabled={loading || success}
                autoComplete="new-password"
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <LockOutlined sx={{ color: 'action.active' }} />
                    </InputAdornment>
                  ),
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => setShowPassword(!showPassword)}
                        edge="end"
                        disabled={loading || success}
                      >
                        {showPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />

              <TextField
                fullWidth
                label="Confirm Password"
                type={showConfirmPassword ? 'text' : 'password'}
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                margin="normal"
                required
                disabled={loading || success}
                autoComplete="new-password"
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <LockOutlined sx={{ color: 'action.active' }} />
                    </InputAdornment>
                  ),
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                        edge="end"
                        disabled={loading || success}
                      >
                        {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />

              {/* Password Requirements Checklist */}
              <Box sx={{ mt: 2, mb: 3, bgcolor: 'grey.50', borderRadius: 2, p: 2 }}>
                <Typography variant="subtitle2" sx={{ mb: 1, fontWeight: 600 }}>
                  Password Requirements
                </Typography>
                <List dense disablePadding>
                  <RequirementItem
                    met={validations.length}
                    text={`${requirements.minLength}-${requirements.maxLength || '∞'} characters`}
                  />
                  {requirements.requireUppercase && (
                    <RequirementItem
                      met={validations.uppercase}
                      text="At least one uppercase letter"
                    />
                  )}
                  {requirements.requireLowercase && (
                    <RequirementItem
                      met={validations.lowercase}
                      text="At least one lowercase letter"
                    />
                  )}
                  {requirements.requireNumbers && (
                    <RequirementItem met={validations.number} text="At least one number" />
                  )}
                  {requirements.requireSpecialChars && (
                    <RequirementItem
                      met={validations.special}
                      text="At least one special character"
                    />
                  )}
                  <RequirementItem met={validations.match} text="Passwords match" />
                </List>
              </Box>

              <Button
                fullWidth
                variant="contained"
                type="submit"
                disabled={!isValid || loading || success}
                sx={{
                  mt: 2,
                  backgroundColor: '#6750A4',
                  '&:hover': { backgroundColor: '#5E35B1' },
                }}
              >
                {loading ? (
                  <CircularProgress size={24} sx={{ color: 'white' }} />
                ) : (
                  'Set Password'
                )}
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
          </CardContent>
        </Card>
      </Container>
    </Box>
  );
}

// Helper component for password requirement items
function RequirementItem({ met, text }: { met: boolean; text: string }) {
  return (
    <ListItem disableGutters sx={{ py: 0.25 }}>
      <ListItemIcon sx={{ minWidth: 32 }}>
        {met ? (
          <CheckIcon sx={{ fontSize: 18, color: 'success.main' }} />
        ) : (
          <CloseIcon sx={{ fontSize: 18, color: 'error.main' }} />
        )}
      </ListItemIcon>
      <ListItemText
        primary={text}
        primaryTypographyProps={{
          variant: 'body2',
          color: met ? 'success.main' : 'text.secondary',
        }}
      />
    </ListItem>
  );
}

export default SetupPasswordPage;
