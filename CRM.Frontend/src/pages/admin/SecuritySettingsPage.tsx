import React from 'react';
import { Box, Paper } from '@mui/material';
import { Security as SecurityIcon } from '@mui/icons-material';
import SecuritySettingsTab from '../../components/settings/SecuritySettingsTab';
import AdminPageHeader from '../../components/admin/AdminPageHeader';

const SecuritySettingsPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="Security Settings"
        subtitle="Password policies, authentication, and security configuration"
        icon={SecurityIcon}
      />

      <Paper sx={{ p: 3 }}>
        <SecuritySettingsTab />
      </Paper>
    </Box>
  );
};

export default SecuritySettingsPage;
