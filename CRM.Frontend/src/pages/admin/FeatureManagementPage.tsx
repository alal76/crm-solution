import React from 'react';
import { Box, Paper } from '@mui/material';
import { ToggleOn as FeatureToggleIcon } from '@mui/icons-material';
import FeatureManagementTab from '../../components/settings/FeatureManagementTab';
import AdminPageHeader from '../../components/admin/AdminPageHeader';

const FeatureManagementPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="Features & Modules"
        subtitle="Enable or disable CRM features and modules"
        icon={FeatureToggleIcon}
      />

      <Paper sx={{ p: 3 }}>
        <FeatureManagementTab />
      </Paper>
    </Box>
  );
};

export default FeatureManagementPage;
