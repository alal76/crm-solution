import React from 'react';
import { Box, Typography, Paper } from '@mui/material';
import { Storage as StorageIcon } from '@mui/icons-material';
import DatabaseSettingsTab from '../../components/settings/DatabaseSettingsTab';
import logo from '../../assets/logo.png';

const DatabaseSettingsPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      {/* Header */}
      <Box sx={{ mb: 4, display: 'flex', alignItems: 'center', gap: 2 }}>
        <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
          <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <StorageIcon sx={{ color: '#1976d2', fontSize: 28 }} />
          <Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>
              Database Settings
            </Typography>
            <Typography color="textSecondary" variant="body2">
              Manage database backups, demo mode, and data management
            </Typography>
          </Box>
        </Box>
      </Box>

      <Paper sx={{ p: 3 }}>
        <DatabaseSettingsTab />
      </Paper>
    </Box>
  );
};

export default DatabaseSettingsPage;
