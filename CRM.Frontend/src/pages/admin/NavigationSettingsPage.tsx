import React from 'react';
import { Box, Paper } from '@mui/material';
import { Menu as MenuIcon } from '@mui/icons-material';
import NavigationSettingsTab from '../../components/settings/NavigationSettingsTab';
import AdminPageHeader from '../../components/admin/AdminPageHeader';

const NavigationSettingsPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="Navigation Settings"
        subtitle="Customize menu structure, categories, and navigation order"
        icon={MenuIcon}
      />

      <Paper sx={{ p: 3 }}>
        <NavigationSettingsTab />
      </Paper>
    </Box>
  );
};

export default NavigationSettingsPage;
