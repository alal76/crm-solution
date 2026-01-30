import React from 'react';
import { Box, Paper } from '@mui/material';
import { Monitor as MonitorIcon } from '@mui/icons-material';
import MonitoringDashboard from './MonitoringDashboard';
import AdminPageHeader from '../../components/admin/AdminPageHeader';

const MonitoringSettingsPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="Monitoring"
        subtitle="Infrastructure monitoring with Uptime Kuma and Portainer"
        icon={MonitorIcon}
      />

      <Paper sx={{ p: 0 }}>
        <MonitoringDashboard />
      </Paper>
    </Box>
  );
};

export default MonitoringSettingsPage;
