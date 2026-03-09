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
  InputAdornment,
  IconButton,
} from '@mui/material';
import { Visibility, VisibilityOff, SupportAgent } from '@mui/icons-material';
import { useNavigate, Link } from 'react-router-dom';
import { portalAuthService, portalService, type PortalConfigDto } from '../../services/portalService';

const PortalLoginPage: React.FC = () => {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [config, setConfig] = useState<PortalConfigDto | null>(null);

  useEffect(() => {
    portalService.getConfig().then(setConfig).catch(() => {/* use defaults */});
    if (portalAuthService.isAuthenticated()) {
      navigate('/portal/dashboard', { replace: true });
    }
  }, [navigate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await portalAuthService.login({ email, password });
      navigate('/portal/dashboard', { replace: true });
    } catch (err: unknown) {
      setError((err as any)?.response?.data?.message ?? 'Login failed. Please check your credentials.');
    } finally {
      setLoading(false);
    }
  };

  const brandColor = config?.primaryColor ?? '#1976d2';
  const portalTitle = config?.portalTitle ?? 'Customer Portal';

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        bgcolor: 'grey.50',
        p: 2,
      }}
    >
      <Card sx={{ maxWidth: 420, width: '100%', boxShadow: 4 }}>
        <CardContent sx={{ p: 4 }}>
          <Box sx={{ textAlign: 'center', mb: 3 }}>
            {config?.logoUrl ? (
              <Box
                component="img"
                src={config.logoUrl}
                alt="Logo"
                sx={{ height: 60, mb: 1 }}
                onError={(e: any) => { e.target.style.display = 'none'; }}
              />
            ) : (
              <SupportAgent sx={{ fontSize: 48, color: brandColor, mb: 1 }} />
            )}
            <Typography variant="h5" fontWeight={700}>
              {portalTitle}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Sign in to access your support portal
            </Typography>
          </Box>

          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

          <Box component="form" onSubmit={handleSubmit}>
            <TextField
              label="Email Address"
              type="email"
              fullWidth
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              sx={{ mb: 2 }}
              autoFocus
              autoComplete="email"
            />
            <TextField
              label="Password"
              type={showPassword ? 'text' : 'password'}
              fullWidth
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              sx={{ mb: 3 }}
              autoComplete="current-password"
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton onClick={() => setShowPassword((v) => !v)} edge="end">
                      {showPassword ? <VisibilityOff /> : <Visibility />}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
            />
            <Button
              type="submit"
              variant="contained"
              fullWidth
              disabled={loading}
              sx={{ mb: 2, bgcolor: brandColor, py: 1.2 }}
            >
              {loading ? <CircularProgress size={20} color="inherit" /> : 'Sign In'}
            </Button>
          </Box>

          <Box sx={{ textAlign: 'center' }}>
            {config?.allowSelfRegistration !== false && (
              <Typography variant="body2" sx={{ mb: 1 }}>
                Don&apos;t have an account?{' '}
                <Link to="/portal/register" style={{ color: brandColor }}>
                  Register
                </Link>
              </Typography>
            )}
            <Typography variant="body2">
              <Link to="/portal/forgot-password" style={{ color: 'grey' }}>
                Forgot password?
              </Link>
            </Typography>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default PortalLoginPage;
