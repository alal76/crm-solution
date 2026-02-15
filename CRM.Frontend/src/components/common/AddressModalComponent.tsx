import React from 'react';
import {
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Button,
  CircularProgress,
  Alert,
} from '@mui/material';

export interface AddressModalComponentProps {
  open: boolean;
  title: string;
  onClose: () => void;
  onSave?: () => void;
  saveLabel?: string;
  disableSave?: boolean;
  maxWidth?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
  fullWidth?: boolean;
  children: React.ReactNode;
  isLoading?: boolean;
  error?: string | null;
  showActions?: boolean;
}

const AddressModalComponent: React.FC<AddressModalComponentProps> = ({
  open,
  title,
  onClose,
  onSave,
  saveLabel = 'Save',
  disableSave = false,
  maxWidth = 'md',
  fullWidth = true,
  children,
  isLoading = false,
  error = null,
  showActions = true,
}) => {
  return (
    <Dialog open={open} onClose={onClose} maxWidth={maxWidth} fullWidth={fullWidth}>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        {children}
      </DialogContent>
      {showActions && (
        <DialogActions>
          <Button onClick={onClose} disabled={isLoading}>
            Cancel
          </Button>
          {onSave && (
            <Button
              onClick={onSave}
              variant="contained"
              color="primary"
              disabled={disableSave || isLoading}
              startIcon={isLoading ? <CircularProgress size={20} /> : undefined}
            >
              {isLoading ? 'Saving...' : saveLabel}
            </Button>
          )}
        </DialogActions>
      )}
    </Dialog>
  );
};

export default AddressModalComponent;
