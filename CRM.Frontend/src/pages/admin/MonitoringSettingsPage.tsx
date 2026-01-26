import React from 'react';
import { Box, Paper } from '@mui/material';
import { Monitor as MonitorIcon } from '@mui/icons-material';
import MonitoringSettingsTab from '../../components/settings/MonitoringSettingsTab';
import AdminPageHeader from '../../components/admin/AdminPageHeader';

const MonitoringSettingsPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="Monitoring"
        subtitle="System health, logs, and performance monitoring"
        icon={MonitorIcon}
      />

      <Paper sx={{ p: 3 }}>
        <MonitoringSettingsTab />
      </Paper>
    </Box>
  );
};

export default MonitoringSettingsPage;
