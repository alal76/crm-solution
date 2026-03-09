// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
import React, { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  CircularProgress,
  TextField,
  Typography,
  Alert,
} from '@mui/material';
import { SupportAgent } from '@mui/icons-material';
import { useNavigate, Link } from 'react-router-dom';
import { portalAuthService, portalService, type PortalConfigDto } from '../../services/portalService';

const PortalRegisterPage: React.FC = () => {
  const navigate = useNavigate();
  const [form, setForm] = useState({
    email: '',
    password: '',
    confirmPassword: '',
    displayName: '',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [config, setConfig] = useState<PortalConfigDto | null>(null);

  useEffect(() => {
    portalService.getConfig().then(setConfig).catch(() => {});
  }, []);

  const handleChange = (field: keyof typeof form) => (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm((prev) => ({ ...prev, [field]: e.target.value }));
  };

  const validate = () => {
    if (form.password.length < 8) return 'Password must be at least 8 characters.';
    if (form.password !== form.confirmPassword) return 'Passwords do not match.';
    return null;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const validationError = validate();
    if (validationError) { setError(validationError); return; }

    setError(null);
    setLoading(true);
    try {
      await portalAuthService.register(form);
      setSuccess(true);
    } catch (err: unknown) {
      setError((err as any)?.response?.data?.message ?? 'Registration failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const brandColor = config?.primaryColor ?? '#1976d2';
  const portalTitle = config?.portalTitle ?? 'Customer Portal';

  if (success) {
    return (
      <Box sx={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', bgcolor: 'grey.50', p: 2 }}>
        <Card sx={{ maxWidth: 420, width: '100%', boxShadow: 4 }}>
          <CardContent sx={{ p: 4, textAlign: 'center' }}>
            <SupportAgent sx={{ fontSize: 48, color: 'success.main', mb: 2 }} />
            <Typography variant="h6" fontWeight={700} mb={1}>Registration Successful!</Typography>
            <Typography variant="body2" color="text.secondary" mb={3}>
              Your account has been created. Please check your email for a verification link.
            </Typography>
            <Button variant="contained" component={Link} to="/portal/login" sx={{ bgcolor: brandColor }}>
              Go to Login
            </Button>
          </CardContent>
        </Card>
      </Box>
    );
  }

  return (
    <Box sx={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', bgcolor: 'grey.50', p: 2 }}>
      <Card sx={{ maxWidth: 460, width: '100%', boxShadow: 4 }}>
        <CardContent sx={{ p: 4 }}>
          <Box sx={{ textAlign: 'center', mb: 3 }}>
            <SupportAgent sx={{ fontSize: 48, color: brandColor, mb: 1 }} />
            <Typography variant="h5" fontWeight={700}>{portalTitle}</Typography>
            <Typography variant="body2" color="text.secondary">Create your support account</Typography>
          </Box>

          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

          <Box component="form" onSubmit={handleSubmit}>
            <TextField
              label="Display Name"
              fullWidth
              value={form.displayName}
              onChange={handleChange('displayName')}
              sx={{ mb: 2 }}
              autoFocus
            />
            <TextField
              label="Email Address"
              type="email"
              fullWidth
              required
              value={form.email}
              onChange={handleChange('email')}
              sx={{ mb: 2 }}
            />
            <TextField
              label="Password"
              type="password"
              fullWidth
              required
              value={form.password}
              onChange={handleChange('password')}
              sx={{ mb: 2 }}
              helperText="Minimum 8 characters"
            />
            <TextField
              label="Confirm Password"
              type="password"
              fullWidth
              required
              value={form.confirmPassword}
              onChange={handleChange('confirmPassword')}
              sx={{ mb: 3 }}
            />
            <Button
              type="submit"
              variant="contained"
              fullWidth
              disabled={loading}
              sx={{ mb: 2, bgcolor: brandColor, py: 1.2 }}
            >
              {loading ? <CircularProgress size={20} color="inherit" /> : 'Create Account'}
            </Button>
          </Box>

          <Box sx={{ textAlign: 'center' }}>
            <Typography variant="body2">
              Already have an account?{' '}
              <Link to="/portal/login" style={{ color: brandColor }}>Sign in</Link>
            </Typography>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default PortalRegisterPage;
