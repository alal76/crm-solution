import React from 'react';
import { Box, Paper } from '@mui/material';
import { Storage as StorageIcon } from '@mui/icons-material';
import DatabaseSettingsTab from '../../components/settings/DatabaseSettingsTab';
import AdminPageHeader from '../../components/admin/AdminPageHeader';

const DatabaseSettingsPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="Database Settings"
        subtitle="Manage database backups, demo mode, and data management"
        icon={StorageIcon}
      />

      <Paper sx={{ p: 3 }}>
        <DatabaseSettingsTab />
      </Paper>
    </Box>
  );
};

export default DatabaseSettingsPage;
