import React, { useState, useEffect } from 'react';
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

interface FeatureFlag {
  id: string;
  name: string;
  description: string;
  enabled: boolean;
  status: 'alpha' | 'beta' | 'stable';
}

/**
 * Feature Flags Panel - Manage experimental and optional features
 */
const FeatureFlagsPanel: React.FC = () => {
  const [flags, setFlags] = useState<FeatureFlag[]>([
    {
      id: 'use_external_search',
      name: 'External Search Integration',
      description: 'Enable Meilisearch or other external search providers',
      enabled: false,
      status: 'stable'
    },
    {
      id: 'use_external_chat',
      name: 'External Chat Provider',
      description: 'Enable Chatwoot or other chat providers',
      enabled: false,
      status: 'beta'
    },
    {
      id: 'enable_itsm',
      name: 'ITSM Module',
      description: 'Enable IT Service Management features',
      enabled: true,
      status: 'beta'
    },
    {
      id: 'enable_ai_agents',
      name: 'AI Agents',
      description: 'Enable Semantic Kernel AI agents',
      enabled: true,
      status: 'stable'
    },
  ]);
  const [saving, setSaving] = useState(false);

  const handleToggle = (id: string) => {
    setFlags(flags.map(flag =>
      flag.id === id ? { ...flag, enabled: !flag.enabled } : flag
    ));
  };

  const handleSave = async () => {
    try {
      setSaving(true);
      // TODO: Call API to save flags
      logger.info('Feature flags saved', flags);
    } catch (err) {
      logger.error('Failed to save feature flags', err);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Box>
      <Card>
        <CardHeader
          title="Feature Flags"
          subtitle="Control experimental and optional features"
        />
        <Divider />
        <CardContent>
          <Alert severity="warning" sx={{ mb: 2 }}>
            Changing feature flags may affect system behavior. Changes take effect immediately.
          </Alert>

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
                  <TableRow key={flag.id}>
                    <TableCell sx={{ fontWeight: 500 }}>{flag.name}</TableCell>
                    <TableCell>{flag.description}</TableCell>
                    <TableCell>
                      <Box
                        sx={{
                          display: 'inline-block',
                          px: 1,
                          py: 0.5,
                          bgcolor:
                            flag.status === 'stable'
                              ? 'success.light'
                              : flag.status === 'beta'
                                ? 'warning.light'
                                : 'info.light',
                          color:
                            flag.status === 'stable'
                              ? 'success.dark'
                              : flag.status === 'beta'
                                ? 'warning.dark'
                                : 'info.dark',
                          borderRadius: 1,
                          textTransform: 'capitalize',
                          fontSize: '0.75rem',
                          fontWeight: 600,
                        }}
                      >
                        {flag.status}
                      </Box>
                    </TableCell>
                    <TableCell align="right">
                      <Switch
                        checked={flag.enabled}
                        onChange={() => handleToggle(flag.id)}
                        disabled={saving}
                      />
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>

          <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end', mt: 3 }}>
            <Button
              variant="contained"
              startIcon={<SaveIcon />}
              onClick={handleSave}
              disabled={saving}
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
