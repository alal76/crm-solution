import React, { useState } from 'react';
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
  Stepper,
  Step,
  StepLabel,
} from '@mui/material';
import { Visibility, VisibilityOff, Check as CheckIcon } from '@mui/icons-material';
import { useNavigate, Link as RouterLink } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import logo from '../assets/logo.png';

function RegisterPage() {
  const [email, setEmail] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [activeStep, setActiveStep] = useState(0);
  const [registrationSuccess, setRegistrationSuccess] = useState(false);
  const { register } = useAuth();
  const navigate = useNavigate();

  const steps = ['Enter Details', 'Set Password', 'Confirm'];

  const handleNext = () => {
    if (validateStep()) {
      setActiveStep((prevActiveStep) => prevActiveStep + 1);
    }
  };

  const handleBack = () => {
    setActiveStep((prevActiveStep) => prevActiveStep - 1);
  };

  const validateStep = () => {
    setError('');
    if (activeStep === 0) {
      if (!firstName.trim() || !lastName.trim() || !email.trim()) {
        setError('Please fill in all fields');
        return false;
      }
      if (!email.includes('@')) {
        setError('Please enter a valid email');
        return false;
      }
      return true;
    }
    if (activeStep === 1) {
      if (!password || !confirmPassword) {
        setError('Please enter a password');
        return false;
      }
      if (password.length < 8) {
        setError('Password must be at least 8 characters long');
        return false;
      }
      // Use strict string comparison - passwords should match exactly
      if (password !== confirmPassword) {
        setError('Passwords do not match');
        return false;
      }
      return true;
    }
    return true;
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      await register({ email, firstName, lastName, password });
      setRegistrationSuccess(true);
      setTimeout(() => {
        navigate('/login');
      }, 2000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Registration failed');
    } finally {
      setLoading(false);
    }
  };

  if (registrationSuccess) {
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
        <Card sx={{ borderRadius: 3, boxShadow: '0px 12px 24px rgba(0, 0, 0, 0.15)', maxWidth: 400 }}>
          <CardContent sx={{ textAlign: 'center', py: 4 }}>
            <Box sx={{ mb: 2 }}>
              <CheckIcon sx={{ fontSize: 64, color: '#06A77D' }} />
            </Box>
            <Typography variant="h5" sx={{ fontWeight: 700, mb: 1 }}>
              Registration Successful!
            </Typography>
            <Typography variant="body2" color="textSecondary">
              Redirecting to login page...
            </Typography>
            <CircularProgress sx={{ mt: 3 }} />
          </CardContent>
        </Card>
      </Box>
    );
  }

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
        <Card sx={{ borderRadius: 3, boxShadow: '0px 12px 24px rgba(0, 0, 0, 0.15)' }}>
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
            <Typography variant="h4" component="h1" sx={{ fontWeight: 700, mb: 1 }}>
              Create Account
            </Typography>
            <Typography variant="body2" sx={{ opacity: 0.9 }}>
              Join our CRM platform
            </Typography>
          </Box>

          <CardContent sx={{ p: 4 }}>
            {/* Stepper */}
            <Stepper activeStep={activeStep} sx={{ mb: 3 }}>
              {steps.map((label) => (
                <Step key={label}>
                  <StepLabel>{label}</StepLabel>
                </Step>
              ))}
            </Stepper>

            {/* Error Alert */}
            {error && (
              <Alert severity="error" sx={{ mb: 3, borderRadius: 2 }}>
                {error}
              </Alert>
            )}

            {/* Form */}
            <Box component="form" onSubmit={handleSubmit}>
              {/* Step 0: Details */}
              {activeStep === 0 && (
                <Box>
                  <TextField
                    fullWidth
                    label="First Name"
                    value={firstName}
                    onChange={(e) => setFirstName(e.target.value)}
                    disabled={loading}
                    margin="normal"
                    required
                  />
                  <TextField
                    fullWidth
                    label="Last Name"
                    value={lastName}
                    onChange={(e) => setLastName(e.target.value)}
                    disabled={loading}
                    margin="normal"
                    required
                  />
                  <TextField
                    fullWidth
                    label="Email"
                    type="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    disabled={loading}
                    margin="normal"
                    required
                  />
                </Box>
              )}

              {/* Step 1: Password */}
              {activeStep === 1 && (
                <Box>
                  <TextField
                    fullWidth
                    label="Password"
                    type={showPassword ? 'text' : 'password'}
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    disabled={loading}
                    margin="normal"
                    required
                    autoComplete="new-password"
                    error={password.length > 0 && password.length < 8}
                    helperText={password.length > 0 && password.length < 8 ? 'Must be at least 8 characters' : ''}
                    InputProps={{
                      endAdornment: (
                        <InputAdornment position="end">
                          <IconButton
                            onClick={() => setShowPassword(!showPassword)}
                            edge="end"
                            disabled={loading}
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
                    disabled={loading}
                    margin="normal"
                    required
                    autoComplete="new-password"
                    error={confirmPassword.length > 0 && password !== confirmPassword}
                    helperText={confirmPassword.length > 0 && password !== confirmPassword ? 'Passwords do not match' : ''}
                    InputProps={{
                      endAdornment: (
                        <InputAdornment position="end">
                          <IconButton
                            onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                            edge="end"
                            disabled={loading}
                          >
                            {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                          </IconButton>
                        </InputAdornment>
                      ),
                    }}
                  />
                </Box>
              )}

              {/* Step 2: Review */}
              {activeStep === 2 && (
                <Box sx={{ textAlign: 'center' }}>
                  <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
                    Confirm Your Details
                  </Typography>
                  <Box sx={{ textAlign: 'left', bgcolor: '#F5F5F5', p: 2, borderRadius: 2 }}>
                    <Typography variant="body2">
                      <strong>Name:</strong> {firstName} {lastName}
                    </Typography>
                    <Typography variant="body2">
                      <strong>Email:</strong> {email}
                    </Typography>
                  </Box>
                </Box>
              )}

              {/* Navigation Buttons */}
              <Box sx={{ display: 'flex', gap: 2, mt: 4, justifyContent: 'space-between' }}>
                <Button
                  disabled={activeStep === 0 || loading}
                  onClick={handleBack}
                  variant="outlined"
                  sx={{ flex: 1 }}
                >
                  Back
                </Button>

                {activeStep === steps.length - 1 ? (
                  <Button
                    fullWidth
                    variant="contained"
                    type="submit"
                    disabled={loading}
                    sx={{
                      background: 'linear-gradient(135deg, #6750A4 0%, #7D5B8D 100%)',
                      flex: 1,
                    }}
                  >
                    {loading ? <CircularProgress size={24} color="inherit" /> : 'Create Account'}
                  </Button>
                ) : (
                  <Button
                    fullWidth
                    variant="contained"
                    onClick={handleNext}
                    disabled={loading}
                    sx={{
                      background: 'linear-gradient(135deg, #6750A4 0%, #7D5B8D 100%)',
                      flex: 1,
                    }}
                  >
                    Next
                  </Button>
                )}
              </Box>
            </Box>

            {/* Sign In Link */}
            <Box sx={{ textAlign: 'center', mt: 3, pt: 3, borderTop: '1px solid #E0E0E0' }}>
              <Typography variant="body2">
                Already have an account?{' '}
                <MuiLink
                  component={RouterLink}
                  to="/login"
                  sx={{
                    textDecoration: 'none',
                    fontWeight: 700,
                    color: '#6750A4',
                    '&:hover': { textDecoration: 'underline' },
                  }}
                >
                  Sign in
                </MuiLink>
              </Typography>
            </Box>
          </CardContent>
        </Card>
      </Container>
    </Box>
  );
}

export default RegisterPage;
