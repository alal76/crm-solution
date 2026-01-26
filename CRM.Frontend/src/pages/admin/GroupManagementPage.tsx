import React from 'react';
import { Box, Paper } from '@mui/material';
import { Groups as GroupsIcon } from '@mui/icons-material';
import GroupManagementTab from '../../components/settings/GroupManagementTab';
import AdminPageHeader from '../../components/admin/AdminPageHeader';

const GroupManagementPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="Group Management"
        subtitle="Create and manage user groups and team assignments"
        icon={GroupsIcon}
      />

      <Paper sx={{ p: 3 }}>
        <GroupManagementTab />
      </Paper>
    </Box>
  );
};

export default GroupManagementPage;
