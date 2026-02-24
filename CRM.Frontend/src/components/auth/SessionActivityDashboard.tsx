// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// Session Activity Tracking Dashboard (TODO-AUTH-020)

import React, { useEffect, useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  IconButton,
  Tooltip,
  CircularProgress,
  Alert,
  Grid,
  Divider,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
} from '@mui/material';
import {
  Devices as DevicesIcon,
  Security as SecurityIcon,
  Laptop as LaptopIcon,
  PhoneAndroid as PhoneIcon,
  Tablet as TabletIcon,
  Delete as DeleteIcon,
  Refresh as RefreshIcon,
  Warning as WarningIcon,
  CheckCircle as CheckCircleIcon,
  LocationOn as LocationIcon,
} from '@mui/icons-material';
import { useAuth } from '../../contexts/AuthContext';
import apiClient from '../../services/apiClient';

interface TrustedDevice {
  id: number;
  deviceId: string;
  deviceName: string | null;
  lastUsedAt: string | null;
  expiresAt: string | null;
  ipAddress: string | null;
  createdAt: string;
}

interface LoginAttempt {
  timestamp: string;
  ipAddress: string;
  success: boolean;
  failureReason: string | null;
  city: string | null;
  countryCode: string | null;
  isAnomalous: boolean;
}

interface LoginStatistics {
  totalLogins: number;
  successfulLogins: number;
  failedLogins: number;
  uniqueIpAddresses: number;
  uniqueCountries: number;
  anomalousLogins: number;
  lastLoginAt: string | null;
  mostActiveHour: number;
  mostActiveDay: number;
}

interface ActiveSession {
  id: number;
  ipAddress: string;
  userAgent: string;
  deviceId: string | null;
  createdAt: string;
  lastActivityAt: string;
  expiresAt: string;
  isCurrentSession: boolean;
}

const getDeviceIcon = (userAgent: string) => {
  const ua = userAgent.toLowerCase();
  if (ua.includes('mobile') || ua.includes('android') || ua.includes('iphone')) {
    return <PhoneIcon />;
  }
  if (ua.includes('tablet') || ua.includes('ipad')) {
    return <TabletIcon />;
  }
  return <LaptopIcon />;
};

const formatDate = (dateString: string | null): string => {
  if (!dateString) return 'Never';
  return new Date(dateString).toLocaleString();
};

const getDayName = (dayIndex: number): string => {
  const days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
  return days[dayIndex] || 'Unknown';
};

const SessionActivityDashboard: React.FC = () => {
  const { user } = useAuth();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  const [trustedDevices, setTrustedDevices] = useState<TrustedDevice[]>([]);
  const [recentLogins, setRecentLogins] = useState<LoginAttempt[]>([]);
  const [loginStats, setLoginStats] = useState<LoginStatistics | null>(null);
  const [activeSessions, setActiveSessions] = useState<ActiveSession[]>([]);
  
  const [revokeDialogOpen, setRevokeDialogOpen] = useState(false);
  const [deviceToRevoke, setDeviceToRevoke] = useState<TrustedDevice | null>(null);
  const [revoking, setRevoking] = useState(false);

  const fetchData = async () => {
    setLoading(true);
    setError(null);

    try {
      const [devicesRes, loginsRes, statsRes, sessionsRes] = await Promise.all([
        apiClient.get<TrustedDevice[]>('/auth/devices/trusted'),
        apiClient.get<LoginAttempt[]>('/auth/analytics/recent-logins?count=10'),
        apiClient.get<LoginStatistics>('/auth/analytics/login-stats?days=30'),
        apiClient.get<ActiveSession[]>('/sessions/active'),
      ]);

      setTrustedDevices(devicesRes.data);
      setRecentLogins(loginsRes.data);
      setLoginStats(statsRes.data);
      setActiveSessions(sessionsRes.data);
    } catch (err: any) {
      console.error('Failed to fetch session data:', err);
      setError(err.response?.data?.message || 'Failed to load session activity data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleRevokeDevice = async () => {
    if (!deviceToRevoke) return;
    
    setRevoking(true);
    try {
      await apiClient.delete(`/api/auth/devices/trusted/${deviceToRevoke.deviceId}`);
      setTrustedDevices(devices => devices.filter(d => d.id !== deviceToRevoke.id));
      setRevokeDialogOpen(false);
      setDeviceToRevoke(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to revoke device trust');
    } finally {
      setRevoking(false);
    }
  };

  const openRevokeDialog = (device: TrustedDevice) => {
    setDeviceToRevoke(device);
    setRevokeDialogOpen(true);
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box p={3}>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" component="h1">
          <SecurityIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
          Session Activity
        </Typography>
        <Tooltip title="Refresh">
          <IconButton onClick={fetchData}>
            <RefreshIcon />
          </IconButton>
        </Tooltip>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {/* Login Statistics Cards */}
      {loginStats && (
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Total Logins (30 days)
                </Typography>
                <Typography variant="h4">{loginStats.totalLogins}</Typography>
                <Typography variant="body2" color="textSecondary">
                  {loginStats.successfulLogins} successful
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Failed Attempts
                </Typography>
                <Typography variant="h4" color="error.main">
                  {loginStats.failedLogins}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  {loginStats.anomalousLogins} flagged as anomalous
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Unique Locations
                </Typography>
                <Typography variant="h4">{loginStats.uniqueIpAddresses}</Typography>
                <Typography variant="body2" color="textSecondary">
                  {loginStats.uniqueCountries} countries
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Most Active
                </Typography>
                <Typography variant="h6">
                  {getDayName(loginStats.mostActiveDay)} @ {loginStats.mostActiveHour}:00
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Last login: {formatDate(loginStats.lastLoginAt)}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      <Divider sx={{ my: 3 }} />

      {/* Trusted Devices Section */}
      <Typography variant="h5" gutterBottom sx={{ display: 'flex', alignItems: 'center' }}>
        <DevicesIcon sx={{ mr: 1 }} />
        Trusted Devices
      </Typography>
      <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
        Devices that are trusted for 2FA bypass. Revoke access to any device you don't recognize.
      </Typography>

      <TableContainer component={Paper} sx={{ mb: 4 }}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Device</TableCell>
              <TableCell>IP Address</TableCell>
              <TableCell>Last Used</TableCell>
              <TableCell>Expires</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {trustedDevices.length === 0 ? (
              <TableRow>
                <TableCell colSpan={5} align="center">
                  No trusted devices
                </TableCell>
              </TableRow>
            ) : (
              trustedDevices.map((device) => (
                <TableRow key={device.id}>
                  <TableCell>
                    <Box display="flex" alignItems="center">
                      {getDeviceIcon(device.deviceName || '')}
                      <Box ml={1}>
                        <Typography variant="body2">
                          {device.deviceName || 'Unknown Device'}
                        </Typography>
                        <Typography variant="caption" color="textSecondary">
                          {device.deviceId.substring(0, 16)}...
                        </Typography>
                      </Box>
                    </Box>
                  </TableCell>
                  <TableCell>{device.ipAddress || 'N/A'}</TableCell>
                  <TableCell>{formatDate(device.lastUsedAt)}</TableCell>
                  <TableCell>{formatDate(device.expiresAt)}</TableCell>
                  <TableCell>
                    <Tooltip title="Revoke Trust">
                      <IconButton
                        color="error"
                        onClick={() => openRevokeDialog(device)}
                        size="small"
                      >
                        <DeleteIcon />
                      </IconButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Recent Login Activity Section */}
      <Typography variant="h5" gutterBottom sx={{ display: 'flex', alignItems: 'center' }}>
        <LocationIcon sx={{ mr: 1 }} />
        Recent Login Activity
      </Typography>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Time</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>IP Address</TableCell>
              <TableCell>Location</TableCell>
              <TableCell>Flags</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {recentLogins.length === 0 ? (
              <TableRow>
                <TableCell colSpan={5} align="center">
                  No recent login activity
                </TableCell>
              </TableRow>
            ) : (
              recentLogins.map((login, index) => (
                <TableRow key={index}>
                  <TableCell>{formatDate(login.timestamp)}</TableCell>
                  <TableCell>
                    {login.success ? (
                      <Chip
                        icon={<CheckCircleIcon />}
                        label="Success"
                        color="success"
                        size="small"
                      />
                    ) : (
                      <Chip
                        icon={<WarningIcon />}
                        label="Failed"
                        color="error"
                        size="small"
                      />
                    )}
                  </TableCell>
                  <TableCell>{login.ipAddress}</TableCell>
                  <TableCell>
                    {login.city && login.countryCode
                      ? `${login.city}, ${login.countryCode}`
                      : 'Unknown'}
                  </TableCell>
                  <TableCell>
                    {login.isAnomalous && (
                      <Chip
                        icon={<WarningIcon />}
                        label="Anomalous"
                        color="warning"
                        size="small"
                      />
                    )}
                    {login.failureReason && (
                      <Tooltip title={login.failureReason}>
                        <Chip label="Details" size="small" sx={{ ml: 1 }} />
                      </Tooltip>
                    )}
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Revoke Device Dialog */}
      <Dialog open={revokeDialogOpen} onClose={() => setRevokeDialogOpen(false)}>
        <DialogTitle>Revoke Device Trust</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to revoke trust for{' '}
            <strong>{deviceToRevoke?.deviceName || 'this device'}</strong>?
          </Typography>
          <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>
            This device will require 2FA verification on next login.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRevokeDialogOpen(false)} disabled={revoking}>
            Cancel
          </Button>
          <Button
            onClick={handleRevokeDevice}
            color="error"
            disabled={revoking}
            startIcon={revoking ? <CircularProgress size={20} /> : <DeleteIcon />}
          >
            {revoking ? 'Revoking...' : 'Revoke'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default SessionActivityDashboard;
