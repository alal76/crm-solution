import React from 'react';
import { Box, Paper } from '@mui/material';
import { Login as LoginIcon } from '@mui/icons-material';
import SocialLoginSettingsTab from '../../components/settings/SocialLoginSettingsTab';
import AdminPageHeader from '../../components/admin/AdminPageHeader';

const SocialLoginSettingsPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="Social Login"
        subtitle="Configure OAuth providers for social authentication"
        icon={LoginIcon}
      />

      <Paper sx={{ p: 3 }}>
        <SocialLoginSettingsTab />
      </Paper>
    </Box>
  );
};

export default SocialLoginSettingsPage;
