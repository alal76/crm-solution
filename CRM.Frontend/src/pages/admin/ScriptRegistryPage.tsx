import React from 'react';
import { Box, Typography, Alert } from '@mui/material';

/**
 * SARCH-018/019/020: Script governance UI scaffold.
 * Lifecycle transitions (Draft → Review → Approved → Deployed → Retired),
 * version history, and audit log viewer.
 * Full implementation is planned for Sprint 3.
 */
const ScriptRegistryPage: React.FC = () => {
  return (
    <Box p={3}>
      <Typography variant="h4" gutterBottom>
        Script Registry
      </Typography>
      <Alert severity="info">
        Script governance UI — lifecycle transitions (Draft → Review → Approved → Deployed →
        Retired), version history, and audit log viewer. Full implementation coming in Sprint 3.
      </Alert>
    </Box>
  );
};

export default ScriptRegistryPage;
