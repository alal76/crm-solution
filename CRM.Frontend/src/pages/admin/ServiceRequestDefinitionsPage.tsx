import React from 'react';
import { Box, Typography, Paper } from '@mui/material';
import { SupportAgent as SupportAgentIcon } from '@mui/icons-material';
import ServiceRequestSettingsTab from '../../components/settings/ServiceRequestSettingsTab';
import logo from '../../assets/logo.png';

const ServiceRequestDefinitionsPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      {/* Header */}
      <Box sx={{ mb: 4, display: 'flex', alignItems: 'center', gap: 2 }}>
        <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
          <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <SupportAgentIcon sx={{ color: '#7b1fa2', fontSize: 28 }} />
          <Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>
              Service Request Definitions
            </Typography>
            <Typography color="textSecondary" variant="body2">
              Define service request types, categories, and workflows
            </Typography>
          </Box>
        </Box>
      </Box>

      <Paper sx={{ p: 3 }}>
        <ServiceRequestSettingsTab />
      </Paper>
    </Box>
  );
};

export default ServiceRequestDefinitionsPage;
