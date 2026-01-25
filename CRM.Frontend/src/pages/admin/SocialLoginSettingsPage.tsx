import React from 'react';
import { Box, Typography, Paper } from '@mui/material';
import { Login as LoginIcon } from '@mui/icons-material';
import SocialLoginSettingsTab from '../../components/settings/SocialLoginSettingsTab';
import logo from '../../assets/logo.png';

const SocialLoginSettingsPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      {/* Header */}
      <Box sx={{ mb: 4, display: 'flex', alignItems: 'center', gap: 2 }}>
        <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
          <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <LoginIcon sx={{ color: '#388e3c', fontSize: 28 }} />
          <Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>
              Social Login
            </Typography>
            <Typography color="textSecondary" variant="body2">
              Configure OAuth providers for social authentication
            </Typography>
          </Box>
        </Box>
      </Box>

      <Paper sx={{ p: 3 }}>
        <SocialLoginSettingsTab />
      </Paper>
    </Box>
  );
};

export default SocialLoginSettingsPage;
