import React from 'react';
import {
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Button,
} from '@mui/material';

export interface AddressModalComponentProps {
  open: boolean;
  title: string;
  onClose: () => void;
  onSave: () => void;
  saveLabel?: string;
  disableSave?: boolean;
  maxWidth?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
  fullWidth?: boolean;
  children: React.ReactNode;
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
}) => {
  return (
    <Dialog open={open} onClose={onClose} maxWidth={maxWidth} fullWidth={fullWidth}>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>{children}</DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button onClick={onSave} variant="contained" color="primary" disabled={disableSave}>
          {saveLabel}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default AddressModalComponent;
