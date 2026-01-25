import React from 'react';
import { Box, Typography, Paper } from '@mui/material';
import { Security as SecurityIcon } from '@mui/icons-material';
import SecuritySettingsTab from '../../components/settings/SecuritySettingsTab';
import logo from '../../assets/logo.png';

const SecuritySettingsPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      {/* Header */}
      <Box sx={{ mb: 4, display: 'flex', alignItems: 'center', gap: 2 }}>
        <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
          <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <SecurityIcon sx={{ color: '#1976d2', fontSize: 28 }} />
          <Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>
              Security Settings
            </Typography>
            <Typography color="textSecondary" variant="body2">
              Password policies, authentication, and security configuration
            </Typography>
          </Box>
        </Box>
      </Box>

      <Paper sx={{ p: 3 }}>
        <SecuritySettingsTab />
      </Paper>
    </Box>
  );
};

export default SecuritySettingsPage;
