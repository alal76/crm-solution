// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the Source-Available License (see LICENSE) as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// Source-Available License (see LICENSE) for more details.
//
// You should have received a copy of the Source-Available License (see LICENSE)
// along with this program. If not, see <https://www.gnu.org/licenses/>.

import React from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Chip,
  CircularProgress,
  Container,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  FormControl,
  FormControlLabel,
  Grid,
  MenuItem,
  Select,
  SelectChangeEvent,
  Stack,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Alert,
  Typography
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  Edit as EditIcon,
  Refresh as RefreshIcon,
  CloudUpload as CloudUploadIcon,
  CloudDownload as CloudDownloadIcon
} from '@mui/icons-material';
import apiClient from '../../services/apiClient';

interface FeatureFlag {
  name: string;
  displayName: string;
  description: string;
  enabled: boolean;
  category: 'Module' | 'Provider';
  providerCategory?: string;
  activeProvider?: string;
  requiresRestart: boolean;
  rolloutPercentage: number;
}

interface AuditEntry {
  id: number;
  flagName: string;
  changeType: string;
  oldValue: string;
  newValue: string;
  changedByName: string;
  changedAt: string;
  reason?: string;
}

export const FeatureFlagsDashboard: React.FC = () => {
  const [flags, setFlags] = React.useState<FeatureFlag[]>([]);
  const [auditLog, setAuditLog] = React.useState<AuditEntry[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [saving, setSaving] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);
  const [successMessage, setSuccessMessage] = React.useState<string | null>(null);
  const [selectedFlag, setSelectedFlag] = React.useState<FeatureFlag | null>(null);
  const [rolloutPercentage, setRolloutPercentage] = React.useState(100);
  const [editDialogOpen, setEditDialogOpen] = React.useState(false);
  const [auditDialogOpen, setAuditDialogOpen] = React.useState(false);
  const [selectedFlagForAudit, setSelectedFlagForAudit] = React.useState<string | null>(null);

  React.useEffect(() => {
    loadFlags();
    loadAuditLog();
  }, []);

  const loadFlags = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get<FeatureFlag[]>('/feature-flags');
      setFlags(response.data);
      setError(null);
    } catch (err) {
      console.error('Failed to load feature flags:', err);
      setError('Failed to load feature flags');
    } finally {
      setLoading(false);
    }
  };

  const loadAuditLog = async () => {
    try {
      const response = await apiClient.get<AuditEntry[]>('/feature-flags/audit');
      setAuditLog(response.data);
    } catch (err) {
      console.error('Failed to load audit log:', err);
    }
  };

  const handleToggleFlag = async (flag: FeatureFlag) => {
    setSaving(true);
    try {
      await apiClient.put(`/feature-flags/${flag.name}`, {
        enabled: !flag.enabled,
        rolloutPercentage: flag.rolloutPercentage
      });
      await loadFlags();
      setSuccessMessage(`${flag.name} ${!flag.enabled ? 'enabled' : 'disabled'}`);
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err) {
      setError('Failed to update flag');
    } finally {
      setSaving(false);
    }
  };

  const handleSetRollout = async () => {
    if (!selectedFlag) return;

    setSaving(true);
    try {
      await apiClient.put(`/feature-flags/${selectedFlag.name}/rollout`, rolloutPercentage);
      await loadFlags();
      setSuccessMessage(`Rollout set to ${rolloutPercentage}%`);
      setEditDialogOpen(false);
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err) {
      setError('Failed to set rollout percentage');
    } finally {
      setSaving(false);
    }
  };

  const handleResetFlags = async () => {
    if (!window.confirm('Reset all flags to defaults?')) return;

    setSaving(true);
    try {
      await apiClient.post('/feature-flags/reset');
      await loadFlags();
      setSuccessMessage('Flags reset to defaults');
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err) {
      setError('Failed to reset flags');
    } finally {
      setSaving(false);
    }
  };

  const openEditDialog = (flag: FeatureFlag) => {
    setSelectedFlag(flag);
    setRolloutPercentage(flag.rolloutPercentage);
    setEditDialogOpen(true);
  };

  const openAuditDialog = async (flagName: string) => {
    try {
      const response = await apiClient.get<AuditEntry[]>(`/api/feature-flags/${flagName}/audit`);
      setAuditLog(response.data);
      setSelectedFlagForAudit(flagName);
      setAuditDialogOpen(true);
    } catch (err) {
      setError('Failed to load audit log');
    }
  };

  if (loading) {
    return (
      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center' }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  const moduleFlags = flags.filter(f => f.category === 'Module');
  const providerFlags = flags.filter(f => f.category === 'Provider');

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" gutterBottom>Feature Flags Management</Typography>
        <Typography variant="body2" color="textSecondary">
         Manage feature flags, rollout percentages, and A/B testing configurations
        </Typography>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

      {/* Actions */}
      <Stack direction="row" spacing={2} sx={{ mb: 4 }}>
        <Button variant="contained" startIcon={<RefreshIcon />} onClick={loadFlags} disabled={loading}>
          Refresh
        </Button>
        <Button variant="outlined" color="error" onClick={handleResetFlags} disabled={saving}>
          Reset to Defaults
        </Button>
      </Stack>

      {/* Module Flags */}
      <Card sx={{ mb: 4 }}>
        <CardHeader title="Module Flags" subtitle="Enable/disable CRM modules" />
        <CardContent>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#f5f5f5' }}>
                  <TableCell>Module</TableCell>
                  <TableCell>Description</TableCell>
                  <TableCell align="center">Status</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {moduleFlags.map((flag) => (
                  <TableRow key={flag.name} hover>
                    <TableCell>{flag.displayName}</TableCell>
                    <TableCell>{flag.description}</TableCell>
                    <TableCell align="center">
                      <Switch
                        checked={flag.enabled}
                        onChange={() => handleToggleFlag(flag)}
                        disabled={saving}
                      />
                      {flag.requiresRestart && (
                        <Chip label="Restart Required" size="small" variant="outlined" sx={{ ml: 1 }} />
                      )}
                    </TableCell>
                    <TableCell align="right">
                      <Button size="small" onClick={() => openAuditDialog(flag.name)}>
                        Audit
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </CardContent>
      </Card>

      {/* Provider Flags */}
      <Card>
        <CardHeader title="Provider Flags" subtitle="Manage external providers" />
        <CardContent>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#f5f5f5' }}>
                  <TableCell>Provider</TableCell>
                  <TableCell>Category</TableCell>
                  <TableCell>Active Provider</TableCell>
                  <TableCell align="center">Enabled</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {providerFlags.map((flag) => (
                  <TableRow key={flag.name} hover>
                    <TableCell>{flag.displayName}</TableCell>
                    <TableCell>{flag.providerCategory}</TableCell>
                    <TableCell>
                      <Chip label={flag.activeProvider || 'BuiltIn'} size="small" />
                    </TableCell>
                    <TableCell align="center">
                      <Switch
                        checked={flag.enabled}
                        onChange={() => handleToggleFlag(flag)}
                        disabled={saving}
                      />
                    </TableCell>
                    <TableCell align="right">
                      <Button size="small" onClick={() => openEditDialog(flag)} sx={{ mr: 1 }}>
                        <EditIcon fontSize="small" />
                      </Button>
                      <Button size="small" onClick={() => openAuditDialog(flag.name)}>
                        Audit
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </CardContent>
      </Card>

      {/* Edit Rollout Dialog */}
      {selectedFlag && (
        <Dialog open={editDialogOpen} onClose={() => setEditDialogOpen(false)} maxWidth="sm" fullWidth>
          <DialogTitle>Edit Rollout Percentage: {selectedFlag.displayName}</DialogTitle>
          <DialogContent sx={{ pt: 3 }}>
            <FormControl fullWidth>
              <Typography variant="body2" gutterBottom>
                Rollout Percentage (0-100%)
              </Typography>
              <TextField
                type="number"
                value={rolloutPercentage}
                onChange={(e) => setRolloutPercentage(Math.min(100, Math.max(0, Number.parseInt(e.target.value) || 0)))}
                inputProps={{ min: 0, max: 100 }}
                fullWidth
                sx={{ mt: 2 }}
              />
              <Typography variant="caption" color="textSecondary" sx={{ mt: 1 }}>
                {rolloutPercentage}% of users will have this feature enabled
              </Typography>
            </FormControl>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setEditDialogOpen(false)}>Cancel</Button>
            <Button onClick={handleSetRollout} variant="contained" disabled={saving}>
              {saving ? 'Saving...' : 'Save'}
            </Button>
          </DialogActions>
        </Dialog>
      )}

      {/* Audit Log Dialog */}
      <Dialog open={auditDialogOpen} onClose={() => setAuditDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>Audit Log: {selectedFlagForAudit}</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Date</TableCell>
                  <TableCell>Changed By</TableCell>
                  <TableCell>Change Type</TableCell>
                  <TableCell>Old Value</TableCell>
                  <TableCell>New Value</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {auditLog.map((entry) => (
                  <TableRow key={entry.id}>
                    <TableCell>{new Date(entry.changedAt).toLocaleString()}</TableCell>
                    <TableCell>{entry.changedByName}</TableCell>
                    <TableCell>{entry.changeType}</TableCell>
                    <TableCell>{entry.oldValue}</TableCell>
                    <TableCell>{entry.newValue}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAuditDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default FeatureFlagsDashboard;
