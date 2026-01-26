import React from 'react';
import { Box, Paper } from '@mui/material';
import { ToggleOn as ModuleIcon } from '@mui/icons-material';
import ModuleFieldSettingsTab from '../../components/settings/ModuleFieldSettingsTabNew';
import AdminPageHeader from '../../components/admin/AdminPageHeader';

const ModuleFieldSettingsPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="Modules & Fields"
        subtitle="Configure module fields, visibility, and display settings"
        icon={ModuleIcon}
      />

      <Paper sx={{ p: 3 }}>
        <ModuleFieldSettingsTab />
      </Paper>
    </Box>
  );
};

export default ModuleFieldSettingsPage;
