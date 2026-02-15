import React from 'react';
import { Alert, Box, Typography } from '@mui/material';

/**
 * TerritoryAssignmentPanel Component
 * 
 * Allows assigning territories to an account
 * PENDING: Awaiting API endpoint implementation for territory assignments
 */
export const TerritoryAssignmentPanel: React.FC<TerritoryAssignmentPanelProps> = ({ 
  accountId 
}) => {
  return (
    <Box sx={{ p: 2 }}>
      <Alert severity="info">
        <Typography variant="body2">
          Territory assignment feature is pending API endpoint implementation.
        </Typography>
        <Typography variant="caption">
          Required endpoints: POST /territories/assign, DELETE /territories/unassign
        </Typography>
      </Alert>
    </Box>
  );
};

export interface TerritoryAssignmentPanelProps {
  accountId: number;
  onSave?: (territories: number[]) => void;
  onError?: (error: string) => void;
}
