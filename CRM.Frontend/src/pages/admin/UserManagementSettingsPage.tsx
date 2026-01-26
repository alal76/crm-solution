import React from 'react';
import { Box, Paper } from '@mui/material';
import { People as PeopleIcon } from '@mui/icons-material';
import UserManagementTab from '../../components/settings/UserManagementTab';
import AdminPageHeader from '../../components/admin/AdminPageHeader';

const UserManagementSettingsPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="User Management"
        subtitle="Manage users, roles, and permissions"
        icon={PeopleIcon}
      />

      <Paper sx={{ p: 3 }}>
        <UserManagementTab />
      </Paper>
    </Box>
  );
};

export default UserManagementSettingsPage;
