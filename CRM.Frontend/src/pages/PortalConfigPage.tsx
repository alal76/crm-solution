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
  Chip,
  CircularProgress,
  Divider,
  FormControlLabel,
  Grid,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
  Alert,
  Paper,
} from '@mui/material';
import { CheckCircle, Block } from '@mui/icons-material';
import { portalAdminService, type PortalConfigDto, type PortalUserDto } from '../services/portalService';

const PortalConfigPage: React.FC = () => {
  const [config, setConfig] = useState<PortalConfigDto | null>(null);
  const [users, setUsers] = useState<PortalUserDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [success, setSuccess] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState({
    isEnabled: false,
    allowSelfRegistration: true,
    welcomeMessage: '',
    supportEmail: '',
    logoUrl: '',
    primaryColor: '#1976d2',
    portalTitle: '',
    allowedDomains: '',
  });

  useEffect(() => {
    const load = async () => {
      try {
        const [cfg, usersResult] = await Promise.all([
          portalAdminService.getConfig(),
          portalAdminService.getPortalUsers(),
        ]);
        setConfig(cfg);
        setUsers(usersResult.items);
        setForm({
          isEnabled: cfg.isEnabled,
          allowSelfRegistration: cfg.allowSelfRegistration,
          welcomeMessage: cfg.welcomeMessage ?? '',
          supportEmail: cfg.supportEmail ?? '',
          logoUrl: cfg.logoUrl ?? '',
          primaryColor: cfg.primaryColor ?? '#1976d2',
          portalTitle: cfg.portalTitle ?? '',
          allowedDomains: cfg.allowedDomains ?? '',
        });
      } catch {
        setError('Failed to load portal configuration.');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const handleField = (field: string) => (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm((prev) => ({ ...prev, [field]: e.target.value }));
  };

  const handleSwitch = (field: string) => (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm((prev) => ({ ...prev, [field]: e.target.checked }));
  };

  const handleSave = async () => {
    setSaving(true);
    setError(null);
    setSuccess(null);
    try {
      const updated = await portalAdminService.updateConfig({
        isEnabled: form.isEnabled,
        allowSelfRegistration: form.allowSelfRegistration,
        welcomeMessage: form.welcomeMessage || undefined,
        supportEmail: form.supportEmail || undefined,
        logoUrl: form.logoUrl || undefined,
        primaryColor: form.primaryColor || undefined,
        portalTitle: form.portalTitle || undefined,
        allowedDomains: form.allowedDomains || undefined,
      });
      setConfig(updated);
      setSuccess('Portal configuration saved successfully.');
    } catch {
      setError('Failed to save configuration. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  const handleToggleUser = async (uid: number, activate: boolean) => {
    try {
      if (activate) {
        await portalAdminService.activateUser(uid);
      } else {
        await portalAdminService.deactivateUser(uid);
      }
      setUsers((prev) =>
        prev.map((u) => (u.id === uid ? { ...u, isActive: activate } : u))
      );
    } catch {
      setError('Failed to update user status.');
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 300 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3, maxWidth: 900, mx: 'auto' }}>
      <Typography variant="h5" fontWeight={700} gutterBottom>
        Customer Portal Configuration
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Manage the self-service customer portal settings and user access.
      </Typography>

      {success && <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccess(null)}>{success}</Alert>}
      {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}

      {/* Status & Toggles */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="subtitle1" fontWeight={600} gutterBottom>Portal Access</Typography>
          <Divider sx={{ mb: 2 }} />
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={form.isEnabled} onChange={handleSwitch('isEnabled')} color="success" />}
                label={
                  <Box>
                    <Typography variant="body2" fontWeight={600}>Enable Portal</Typography>
                    <Typography variant="caption" color="text.secondary">
                      Allow customers to access the self-service portal
                    </Typography>
                  </Box>
                }
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={<Switch checked={form.allowSelfRegistration} onChange={handleSwitch('allowSelfRegistration')} />}
                label={
                  <Box>
                    <Typography variant="body2" fontWeight={600}>Self Registration</Typography>
                    <Typography variant="caption" color="text.secondary">
                      Allow customers to register without an invitation
                    </Typography>
                  </Box>
                }
              />
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Branding */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="subtitle1" fontWeight={600} gutterBottom>Branding & Content</Typography>
          <Divider sx={{ mb: 2 }} />
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Portal Title"
                fullWidth
                value={form.portalTitle}
                onChange={handleField('portalTitle')}
                placeholder="Customer Support Portal"
                size="small"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Primary Color"
                fullWidth
                value={form.primaryColor}
                onChange={handleField('primaryColor')}
                placeholder="#1976d2"
                size="small"
                InputProps={{
                  startAdornment: (
                    <Box
                      component="span"
                      sx={{
                        width: 20,
                        height: 20,
                        borderRadius: '50%',
                        bgcolor: form.primaryColor || '#1976d2',
                        mr: 1,
                        border: '1px solid',
                        borderColor: 'divider',
                      }}
                    />
                  ),
                }}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                label="Welcome Message"
                fullWidth
                multiline
                minRows={2}
                value={form.welcomeMessage}
                onChange={handleField('welcomeMessage')}
                placeholder="Welcome! Submit tickets and browse articles."
                size="small"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Support Email"
                fullWidth
                value={form.supportEmail}
                onChange={handleField('supportEmail')}
                placeholder="support@yourcompany.com"
                size="small"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Logo URL"
                fullWidth
                value={form.logoUrl}
                onChange={handleField('logoUrl')}
                placeholder="https://..."
                size="small"
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                label="Allowed Email Domains"
                fullWidth
                value={form.allowedDomains}
                onChange={handleField('allowedDomains')}
                placeholder="example.com, partner.org (leave blank to allow all)"
                size="small"
                helperText="Comma-separated list. Leave blank to allow all domains."
              />
            </Grid>
          </Grid>

          <Box sx={{ mt: 2, display: 'flex', justifyContent: 'flex-end' }}>
            <Button
              variant="contained"
              onClick={handleSave}
              disabled={saving}
              startIcon={saving ? <CircularProgress size={16} color="inherit" /> : undefined}
            >
              {saving ? 'Saving…' : 'Save Configuration'}
            </Button>
          </Box>
        </CardContent>
      </Card>

      {/* Portal Users */}
      <Card>
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
            <Typography variant="subtitle1" fontWeight={600}>Portal Users</Typography>
            <Chip label={`${users.length} total`} size="small" variant="outlined" />
          </Box>
          <Divider sx={{ mb: 2 }} />
          {users.length === 0 ? (
            <Typography color="text.secondary" variant="body2">No portal users have registered yet.</Typography>
          ) : (
            <TableContainer component={Paper} variant="outlined">
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Name</TableCell>
                    <TableCell>Email</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Registered</TableCell>
                    <TableCell align="right">Action</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {users.map((u) => (
                    <TableRow key={u.id} hover>
                      <TableCell>{u.displayName ?? '—'}</TableCell>
                      <TableCell>{u.email}</TableCell>
                      <TableCell>
                        <Chip
                          size="small"
                          label={u.isActive ? 'Active' : 'Suspended'}
                          color={u.isActive ? 'success' : 'default'}
                          icon={u.isActive ? <CheckCircle sx={{ fontSize: 14 }} /> : <Block sx={{ fontSize: 14 }} />}
                        />
                      </TableCell>
                      <TableCell>
                        {u.createdAt ? new Date(u.createdAt).toLocaleDateString() : '—'}
                      </TableCell>
                      <TableCell align="right">
                        <Button
                          size="small"
                          color={u.isActive ? 'warning' : 'success'}
                          onClick={() => handleToggleUser(u.id, !u.isActive)}
                        >
                          {u.isActive ? 'Suspend' : 'Activate'}
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </CardContent>
      </Card>
    </Box>
  );
};

export default PortalConfigPage;
