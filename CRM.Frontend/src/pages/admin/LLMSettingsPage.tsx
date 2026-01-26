import React from 'react';
import { Box, Paper } from '@mui/material';
import { Psychology as LLMIcon } from '@mui/icons-material';
import LLMSettingsTab from '../../components/settings/LLMSettingsTab';
import AdminPageHeader from '../../components/admin/AdminPageHeader';

const LLMSettingsPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="AI / LLM Settings"
        subtitle="Configure AI providers for workflow automation and intelligent features"
        icon={LLMIcon}
      />

      <Paper sx={{ p: 3 }}>
        <LLMSettingsTab />
      </Paper>
    </Box>
  );
};

export default LLMSettingsPage;
