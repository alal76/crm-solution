import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Switch,
  Button,
  Divider,
  Alert,
  CircularProgress,
} from '@mui/material';
import { Save as SaveIcon } from '@mui/icons-material';
import logger from '../../services/logger';
import { featureFlagService, FeatureFlagDto } from '../../services/featureFlagService';

/**
 * Feature Flags Panel - Manage experimental and optional features (REV-FE-005).
 * Loads real flags from the backend and persists toggles via featureFlagService.
 */
const FeatureFlagsPanel: React.FC = () => {
  const [flags, setFlags] = useState<FeatureFlagDto[]>([]);
  const [pendingChanges, setPendingChanges] = useState<Record<string, boolean>>({});
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [saveError, setSaveError] = useState<string | null>(null);
  const [saveSuccess, setSaveSuccess] = useState(false);

  const loadFlags = useCallback(async () => {
    setLoading(true);
    setLoadError(null);
    try {
      const data = await featureFlagService.getAllFlags();
      setFlags(data);
    } catch (err) {
      logger.error('Failed to load feature flags', err);
      setLoadError('Failed to load feature flags. Please try again.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadFlags();
  }, [loadFlags]);

  const handleToggle = (name: string) => {
    const current = flags.find(f => f.name === name);
    if (!current) return;
    const currentlyPending = Object.prototype.hasOwnProperty.call(pendingChanges, name)
      ? pendingChanges[name]
      : current.enabled;
    setPendingChanges(prev => ({ ...prev, [name]: !currentlyPending }));
  };

  const isEnabled = (flag: FeatureFlagDto): boolean =>
    Object.prototype.hasOwnProperty.call(pendingChanges, flag.name)
      ? pendingChanges[flag.name]
      : flag.enabled;

  const handleSave = async () => {
    const changedNames = Object.keys(pendingChanges).filter(
      name => pendingChanges[name] !== flags.find(f => f.name === name)?.enabled
    );
    if (changedNames.length === 0) {
      return;
    }

    setSaving(true);
    setSaveError(null);
    setSaveSuccess(false);
    try {
      for (const name of changedNames) {
        await featureFlagService.updateFlag(name, { name, enabled: pendingChanges[name] });
      }
      logger.info('Feature flags saved', changedNames);
      await loadFlags();
      setPendingChanges({});
      setSaveSuccess(true);
    } catch (err: any) {
      logger.error('Failed to save feature flags', err);
      setSaveError(err?.response?.data?.error || 'Failed to save feature flags. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  const hasPendingChanges = Object.keys(pendingChanges).some(
    name => pendingChanges[name] !== flags.find(f => f.name === name)?.enabled
  );

  return (
    <Box>
      <Card>
        <CardHeader
          title="Feature Flags"
          subheader="Control experimental and optional features"
        />
        <Divider />
        <CardContent>
          <Alert severity="warning" sx={{ mb: 2 }}>
            Changing feature flags may affect system behavior. Changes take effect immediately.
          </Alert>

          {saveError && (
            <Alert severity="error" sx={{ mb: 2 }} onClose={() => setSaveError(null)}>
              {saveError}
            </Alert>
          )}
          {saveSuccess && (
            <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSaveSuccess(false)}>
              Feature flags saved successfully.
            </Alert>
          )}
          {loadError && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {loadError}
            </Alert>
          )}

          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
              <CircularProgress />
            </Box>
          ) : (
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow sx={{ bgcolor: 'grey.100' }}>
                    <TableCell>Feature</TableCell>
                    <TableCell>Description</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell align="right">Enabled</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {flags.map(flag => (
                    <TableRow key={flag.name}>
                      <TableCell sx={{ fontWeight: 500 }}>{flag.displayName}</TableCell>
                      <TableCell>{flag.description}</TableCell>
                      <TableCell>
                        <Box
                          sx={{
                            display: 'inline-block',
                            px: 1,
                            py: 0.5,
                            bgcolor: flag.category === 'Module' ? 'warning.light' : 'success.light',
                            color: flag.category === 'Module' ? 'warning.dark' : 'success.dark',
                            borderRadius: 1,
                            textTransform: 'capitalize',
                            fontSize: '0.75rem',
                            fontWeight: 600,
                          }}
                        >
                          {flag.category}
                        </Box>
                      </TableCell>
                      <TableCell align="right">
                        <Switch
                          checked={isEnabled(flag)}
                          onChange={() => handleToggle(flag.name)}
                          disabled={saving}
                        />
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}

          <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end', mt: 3 }}>
            <Button
              variant="contained"
              startIcon={<SaveIcon />}
              onClick={handleSave}
              disabled={saving || !hasPendingChanges}
            >
              {saving ? 'Saving...' : 'Save Changes'}
            </Button>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default FeatureFlagsPanel;
export { FeatureFlagsPanel };
