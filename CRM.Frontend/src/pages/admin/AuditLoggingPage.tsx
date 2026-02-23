import React from 'react';
import { Box } from '@mui/material';
import { Policy as AuditIcon } from '@mui/icons-material';
import AuditLogsPanel from '../../components/admin/AuditLogsPanel';
import AdminPageHeader from '../../components/admin/AdminPageHeader';

const AuditLoggingPage: React.FC = () => {
  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="Audit Logging"
        subtitle="View system activity, user actions, and change history"
        icon={AuditIcon}
      />
      <AuditLogsPanel />
    </Box>
  );
};

export default AuditLoggingPage;
