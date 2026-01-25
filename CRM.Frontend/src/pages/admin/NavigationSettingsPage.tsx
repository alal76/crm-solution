import React from 'react';
import { Box, Typography, Paper } from '@mui/material';
import { Menu as MenuIcon } from '@mui/icons-material';
import NavigationSettingsTab from '../../components/settings/NavigationSettingsTab';
import logo from '../../assets/logo.png';

const NavigationSettingsPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      {/* Header */}
      <Box sx={{ mb: 4, display: 'flex', alignItems: 'center', gap: 2 }}>
        <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
          <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <MenuIcon sx={{ color: '#7b1fa2', fontSize: 28 }} />
          <Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>
              Navigation Settings
            </Typography>
            <Typography color="textSecondary" variant="body2">
              Customize menu structure, categories, and navigation order
            </Typography>
          </Box>
        </Box>
      </Box>

      <Paper sx={{ p: 3 }}>
        <NavigationSettingsTab />
      </Paper>
    </Box>
  );
};

export default NavigationSettingsPage;
