import React from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Button,
} from '@mui/material';

export interface ConfirmDialogProps {
  open: boolean;
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  confirmColor?: 'primary' | 'secondary' | 'error' | 'warning' | 'info' | 'success';
  onConfirm: () => void;
  onCancel: () => void;
}

/**
 * Reusable confirmation dialog component
 * Use this instead of window.confirm() for consistent UI
 */
export const ConfirmDialog: React.FC<ConfirmDialogProps> = ({
  open,
  title,
  message,
  confirmText = 'Confirm',
  cancelText = 'Cancel',
  confirmColor = 'primary',
  onConfirm,
  onCancel,
}) => {
  return (
    <Dialog
      open={open}
      onClose={onCancel}
      aria-labelledby="confirm-dialog-title"
      aria-describedby="confirm-dialog-description"
    >
      <DialogTitle id="confirm-dialog-title">{title}</DialogTitle>
      <DialogContent>
        <DialogContentText id="confirm-dialog-description">
          {message}
        </DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel}>{cancelText}</Button>
        <Button onClick={onConfirm} color={confirmColor} variant="contained" autoFocus>
          {confirmText}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

/**
 * Hook for easier confirmation dialog management
 * Usage:
 * const { showConfirm, ConfirmDialogComponent } = useConfirmDialog();
 * 
 * const handleDelete = async () => {
 *   const confirmed = await showConfirm({
 *     title: 'Delete Item',
 *     message: 'Are you sure you want to delete this item?',
 *     confirmColor: 'error'
 *   });
 *   if (confirmed) {
 *     // perform delete
 *   }
 * };
 */
export const useConfirmDialog = () => {
  const [dialogState, setDialogState] = React.useState<{
    open: boolean;
    title: string;
    message: string;
    confirmText?: string;
    cancelText?: string;
    confirmColor?: ConfirmDialogProps['confirmColor'];
    resolve?: (value: boolean) => void;
  }>({
    open: false,
    title: '',
    message: '',
  });

  const showConfirm = React.useCallback(
    (options: Omit<ConfirmDialogProps, 'open' | 'onConfirm' | 'onCancel'>): Promise<boolean> => {
      return new Promise((resolve) => {
        setDialogState({
          ...options,
          open: true,
          resolve,
        });
      });
    },
    []
  );

  const handleConfirm = React.useCallback(() => {
    dialogState.resolve?.(true);
    setDialogState((prev) => ({ ...prev, open: false }));
  }, [dialogState.resolve]);

  const handleCancel = React.useCallback(() => {
    dialogState.resolve?.(false);
    setDialogState((prev) => ({ ...prev, open: false }));
  }, [dialogState.resolve]);

  const ConfirmDialogComponent = React.useMemo(
    () => (
      <ConfirmDialog
        open={dialogState.open}
        title={dialogState.title}
        message={dialogState.message}
        confirmText={dialogState.confirmText}
        cancelText={dialogState.cancelText}
        confirmColor={dialogState.confirmColor}
        onConfirm={handleConfirm}
        onCancel={handleCancel}
      />
    ),
    [
      dialogState.open,
      dialogState.title,
      dialogState.message,
      dialogState.confirmText,
      dialogState.cancelText,
      dialogState.confirmColor,
      handleConfirm,
      handleCancel,
    ]
  );

  return { showConfirm, ConfirmDialogComponent };
};

export default ConfirmDialog;
