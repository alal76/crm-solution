// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
import React, { useState, useEffect } from 'react';
import {
  AppBar,
  Box,
  Button,
  Card,
  CardContent,
  CircularProgress,
  Divider,
  IconButton,
  TextField,
  Toolbar,
  Typography,
  Alert,
} from '@mui/material';
import { ArrowBack, ExitToApp, SupportAgent } from '@mui/icons-material';
import { useNavigate, Link } from 'react-router-dom';
import {
  portalAuthService,
  portalService,
  type PortalUserDto,
  type PortalConfigDto,
} from '../../services/portalService';

const PortalProfilePage: React.FC = () => {
  const navigate = useNavigate();
  const [config, setConfig] = useState<PortalConfigDto | null>(null);
  const [profile, setProfile] = useState<PortalUserDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // profile edit state
  const [displayName, setDisplayName] = useState('');
  const [profileSaving, setProfileSaving] = useState(false);
  const [profileSuccess, setProfileSuccess] = useState<string | null>(null);
  const [profileError, setProfileError] = useState<string | null>(null);

  // password change state
  const [pwCurrent, setPwCurrent] = useState('');
  const [pwNew, setPwNew] = useState('');
  const [pwConfirm, setPwConfirm] = useState('');
  const [pwSaving, setPwSaving] = useState(false);
  const [pwSuccess, setPwSuccess] = useState<string | null>(null);
  const [pwError, setPwError] = useState<string | null>(null);

  const user = portalAuthService.getCurrentUser();

  useEffect(() => {
    if (!portalAuthService.isAuthenticated()) {
      navigate('/portal/login', { replace: true });
      return;
    }
    Promise.all([portalService.getConfig(), portalService.getProfile()])
      .then(([cfg, prof]) => {
        setConfig(cfg);
        setProfile(prof);
        setDisplayName(prof.displayName ?? '');
      })
      .catch(() => setError('Failed to load profile.'))
      .finally(() => setLoading(false));
  }, [navigate]);

  const handleLogout = () => {
    portalAuthService.logout();
    navigate('/portal/login', { replace: true });
  };

  const handleProfileSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setProfileError(null);
    setProfileSuccess(null);
    setProfileSaving(true);
    try {
      const updated = await portalService.updateProfile({ displayName });
      setProfile(updated);
      setProfileSuccess('Profile updated successfully.');
    } catch (err: any) {
      setProfileError(err?.response?.data?.message ?? 'Failed to update profile.');
    } finally {
      setProfileSaving(false);
    }
  };

  const handlePasswordChange = async (e: React.FormEvent) => {
    e.preventDefault();
    setPwError(null);
    setPwSuccess(null);
    if (pwNew.length < 8) {
      setPwError('New password must be at least 8 characters.');
      return;
    }
    if (pwNew !== pwConfirm) {
      setPwError('Passwords do not match.');
      return;
    }
    setPwSaving(true);
    try {
      await portalService.changePassword({ currentPassword: pwCurrent, newPassword: pwNew });
      setPwSuccess('Password changed successfully.');
      setPwCurrent('');
      setPwNew('');
      setPwConfirm('');
    } catch (err: any) {
      setPwError(err?.response?.data?.message ?? 'Failed to change password. Check your current password.');
    } finally {
      setPwSaving(false);
    }
  };

  const brandColor = config?.primaryColor ?? '#1976d2';

  if (loading) {
    return (
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', minHeight: '100vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <AppBar position="static" sx={{ bgcolor: brandColor }}>
        <Toolbar>
          <IconButton color="inherit" component={Link} to="/portal/dashboard" sx={{ mr: 1 }}>
            <ArrowBack />
          </IconButton>
          <SupportAgent sx={{ mr: 1 }} />
          <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: 700 }}>
            My Profile
          </Typography>
          <Typography variant="body2" sx={{ mr: 2 }}>{user?.displayName ?? user?.email}</Typography>
          <IconButton color="inherit" onClick={handleLogout} title="Sign out">
            <ExitToApp />
          </IconButton>
        </Toolbar>
      </AppBar>

      <Box sx={{ p: 3, maxWidth: 640, mx: 'auto' }}>
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        {/* Profile Information */}
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" fontWeight={700} mb={2}>Profile Information</Typography>
            <Divider sx={{ mb: 2 }} />

            <Typography variant="body2" color="text.secondary" mb={0.5}>Email</Typography>
            <Typography variant="body1" mb={2}>{profile?.email}</Typography>

            {profileSuccess && <Alert severity="success" sx={{ mb: 2 }}>{profileSuccess}</Alert>}
            {profileError && <Alert severity="error" sx={{ mb: 2 }}>{profileError}</Alert>}

            <Box component="form" onSubmit={handleProfileSave}>
              <TextField
                fullWidth
                label="Display Name"
                value={displayName}
                onChange={(e) => setDisplayName(e.target.value)}
                sx={{ mb: 2 }}
              />
              <Button
                type="submit"
                variant="contained"
                disabled={profileSaving}
                sx={{ bgcolor: brandColor }}
              >
                {profileSaving ? <CircularProgress size={20} color="inherit" /> : 'Save Changes'}
              </Button>
            </Box>
          </CardContent>
        </Card>

        {/* Change Password */}
        <Card>
          <CardContent>
            <Typography variant="h6" fontWeight={700} mb={2}>Change Password</Typography>
            <Divider sx={{ mb: 2 }} />

            {pwSuccess && <Alert severity="success" sx={{ mb: 2 }}>{pwSuccess}</Alert>}
            {pwError && <Alert severity="error" sx={{ mb: 2 }}>{pwError}</Alert>}

            <Box component="form" onSubmit={handlePasswordChange}>
              <TextField
                fullWidth
                label="Current Password"
                type="password"
                value={pwCurrent}
                onChange={(e) => setPwCurrent(e.target.value)}
                required
                sx={{ mb: 2 }}
                autoComplete="current-password"
              />
              <TextField
                fullWidth
                label="New Password"
                type="password"
                value={pwNew}
                onChange={(e) => setPwNew(e.target.value)}
                required
                sx={{ mb: 2 }}
                autoComplete="new-password"
                helperText="Minimum 8 characters"
              />
              <TextField
                fullWidth
                label="Confirm New Password"
                type="password"
                value={pwConfirm}
                onChange={(e) => setPwConfirm(e.target.value)}
                required
                sx={{ mb: 2 }}
                autoComplete="new-password"
              />
              <Button
                type="submit"
                variant="contained"
                disabled={pwSaving}
                sx={{ bgcolor: brandColor }}
              >
                {pwSaving ? <CircularProgress size={20} color="inherit" /> : 'Change Password'}
              </Button>
            </Box>
          </CardContent>
        </Card>
      </Box>
    </Box>
  );
};

export default PortalProfilePage;
