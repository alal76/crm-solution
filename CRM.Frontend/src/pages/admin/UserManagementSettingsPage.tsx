import React from 'react';
import { Box, Typography, Paper } from '@mui/material';
import { People as PeopleIcon } from '@mui/icons-material';
import UserManagementTab from '../../components/settings/UserManagementTab';
import logo from '../../assets/logo.png';

const UserManagementSettingsPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      {/* Header */}
      <Box sx={{ mb: 4, display: 'flex', alignItems: 'center', gap: 2 }}>
        <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
          <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <PeopleIcon sx={{ color: '#388e3c', fontSize: 28 }} />
          <Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>
              User Management
            </Typography>
            <Typography color="textSecondary" variant="body2">
              Manage users, roles, and permissions
            </Typography>
          </Box>
        </Box>
      </Box>

      <Paper sx={{ p: 3 }}>
        <UserManagementTab />
      </Paper>
    </Box>
  );
};

export default UserManagementSettingsPage;
