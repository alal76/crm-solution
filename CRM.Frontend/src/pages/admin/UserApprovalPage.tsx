import React from 'react';
import { Box, Paper } from '@mui/material';
import { PersonAdd as PersonAddIcon } from '@mui/icons-material';
import UserApprovalTab from '../../components/settings/UserApprovalTab';
import AdminPageHeader from '../../components/admin/AdminPageHeader';

const UserApprovalPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="User Approvals"
        subtitle="Review and approve new user registrations"
        icon={PersonAddIcon}
      />

      <Paper sx={{ p: 3 }}>
        <UserApprovalTab />
      </Paper>
    </Box>
  );
};

export default UserApprovalPage;
