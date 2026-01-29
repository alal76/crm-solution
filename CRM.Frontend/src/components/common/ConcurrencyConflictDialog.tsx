/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

import React from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Alert,
  Box,
  Divider,
} from '@mui/material';
import WarningIcon from '@mui/icons-material/Warning';
import RefreshIcon from '@mui/icons-material/Refresh';
import SaveIcon from '@mui/icons-material/Save';
import CancelIcon from '@mui/icons-material/Cancel';

export interface ConflictData {
  entityType: string;
  entityId: number;
  modifiedBy?: string;
  modifiedAt?: string;
  serverVersion?: number[];
  clientVersion?: number[];
}

interface ConcurrencyConflictDialogProps {
  open: boolean;
  onClose: () => void;
  onReload: () => void;
  onOverwrite: () => void;
  conflictData?: ConflictData;
  entityName?: string;
}

/**
 * Dialog component shown when a concurrency conflict is detected.
 * Offers the user options to reload the latest data or overwrite with their changes.
 */
export const ConcurrencyConflictDialog: React.FC<ConcurrencyConflictDialogProps> = ({
  open,
  onClose,
  onReload,
  onOverwrite,
  conflictData,
  entityName = 'record',
}) => {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="sm"
      fullWidth
      aria-labelledby="conflict-dialog-title"
    >
      <DialogTitle id="conflict-dialog-title" sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <WarningIcon color="warning" />
        <span>Concurrency Conflict Detected</span>
      </DialogTitle>
      
      <DialogContent>
        <Alert severity="warning" sx={{ mb: 2 }}>
          This {entityName} has been modified by another user since you started editing.
        </Alert>

        <Typography variant="body1" gutterBottom>
          Your changes cannot be saved because the data has been updated by someone else.
          You have the following options:
        </Typography>

        <Box sx={{ mt: 2 }}>
          <Typography variant="subtitle2" fontWeight="bold">
            Option 1: Reload Latest Data
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ ml: 2, mb: 2 }}>
            Discard your changes and reload the latest version from the server.
            Your unsaved changes will be lost.
          </Typography>

          <Typography variant="subtitle2" fontWeight="bold">
            Option 2: Overwrite Changes
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ ml: 2, mb: 2 }}>
            Force save your changes, overwriting what the other user saved.
            The other user's changes will be lost.
          </Typography>

          <Typography variant="subtitle2" fontWeight="bold">
            Option 3: Cancel
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ ml: 2 }}>
            Close this dialog and review your changes before deciding.
          </Typography>
        </Box>

        {conflictData?.modifiedBy && (
          <>
            <Divider sx={{ my: 2 }} />
            <Typography variant="body2" color="text.secondary">
              Modified by: <strong>{conflictData.modifiedBy}</strong>
              {conflictData.modifiedAt && (
                <> at {new Date(conflictData.modifiedAt).toLocaleString()}</>
              )}
            </Typography>
          </>
        )}
      </DialogContent>

      <DialogActions sx={{ px: 3, pb: 2 }}>
        <Button
          onClick={onClose}
          startIcon={<CancelIcon />}
          color="inherit"
        >
          Cancel
        </Button>
        <Button
          onClick={onReload}
          startIcon={<RefreshIcon />}
          variant="outlined"
          color="primary"
        >
          Reload Latest
        </Button>
        <Button
          onClick={onOverwrite}
          startIcon={<SaveIcon />}
          variant="contained"
          color="warning"
        >
          Overwrite
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default ConcurrencyConflictDialog;
