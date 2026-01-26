import React from 'react';
import { Box, Paper } from '@mui/material';
import { Cloud as CloudIcon } from '@mui/icons-material';
import DeploymentSettingsTab from '../../components/settings/DeploymentSettingsTab';
import AdminPageHeader from '../../components/admin/AdminPageHeader';

const DeploymentSettingsPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="Deployment & Hosting"
        subtitle="Configure cloud deployment, SSL, and hosting settings"
        icon={CloudIcon}
      />

      <Paper sx={{ p: 3 }}>
        <DeploymentSettingsTab />
      </Paper>
    </Box>
  );
};

export default DeploymentSettingsPage;
