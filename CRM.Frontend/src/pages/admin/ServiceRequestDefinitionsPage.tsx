import React from 'react';
import { Box, Paper } from '@mui/material';
import { SupportAgent as SupportAgentIcon } from '@mui/icons-material';
import ServiceRequestSettingsTab from '../../components/settings/ServiceRequestSettingsTab';
import AdminPageHeader from '../../components/admin/AdminPageHeader';

const ServiceRequestDefinitionsPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="Service Request Definitions"
        subtitle="Define service request types, categories, and workflows"
        icon={SupportAgentIcon}
      />

      <Paper sx={{ p: 3 }}>
        <ServiceRequestSettingsTab />
      </Paper>
    </Box>
  );
};

export default ServiceRequestDefinitionsPage;
