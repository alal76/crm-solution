import React from 'react';
import { Box, Paper } from '@mui/material';
import { Storage as MasterDataIcon } from '@mui/icons-material';
import MasterDataSettingsTab from '../../components/settings/MasterDataSettingsTab';
import AdminPageHeader from '../../components/admin/AdminPageHeader';

const MasterDataSettingsPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="Master Data"
        subtitle="Manage lookup tables, reference data, and system values"
        icon={MasterDataIcon}
      />

      <Paper sx={{ p: 3 }}>
        <MasterDataSettingsTab />
      </Paper>
    </Box>
  );
};

export default MasterDataSettingsPage;
