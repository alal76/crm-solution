import React from 'react';
import { Box, Typography, Paper } from '@mui/material';
import { Groups as GroupsIcon } from '@mui/icons-material';
import GroupManagementTab from '../../components/settings/GroupManagementTab';
import logo from '../../assets/logo.png';

const GroupManagementPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      {/* Header */}
      <Box sx={{ mb: 4, display: 'flex', alignItems: 'center', gap: 2 }}>
        <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
          <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <GroupsIcon sx={{ color: '#388e3c', fontSize: 28 }} />
          <Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>
              Group Management
            </Typography>
            <Typography color="textSecondary" variant="body2">
              Create and manage user groups and team assignments
            </Typography>
          </Box>
        </Box>
      </Box>

      <Paper sx={{ p: 3 }}>
        <GroupManagementTab />
      </Paper>
    </Box>
  );
};

export default GroupManagementPage;
