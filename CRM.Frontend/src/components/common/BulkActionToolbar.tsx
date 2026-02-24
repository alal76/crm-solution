/**
 * BulkActionToolbar - Generic toolbar for bulk operations on selected items
 * Generalized from IncidentBulkActionTools for reuse across entities
 */

import React, { useState } from 'react';
import {
  Box,
  Button,
  Stack,
  Checkbox,
  Typography,
  Menu,
  MenuItem,
  Divider,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  IconButton,
  Tooltip,
  Chip,
  FormControlLabel,
  useTheme,
  alpha,
} from '@mui/material';
import {
  Delete as DeleteIcon,
  Edit as EditIcon,
  FileDownload as ExportIcon,
  Assignment as AssignIcon,
  MoreVert as MoreIcon,
  Close as CloseIcon,
  CheckCircle as CheckCircleIcon,
} from '@mui/icons-material';

// Action definition
export interface BulkAction<T = unknown> {
  id: string;
  label: string;
  icon?: React.ReactNode;
  color?: 'inherit' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning';
  onClick?: (selectedIds: (number | string)[]) => void | Promise<void>;
  // For actions that need confirmation
  requiresConfirmation?: boolean;
  confirmationTitle?: string;
  confirmationMessage?: string | ((count: number) => string);
  // For actions that open a dialog (e.g., status change)
  hasDialog?: boolean;
  dialogComponent?: React.ComponentType<{
    open: boolean;
    onClose: () => void;
    selectedIds: (number | string)[];
    onComplete: () => void;
  }>;
  // Visibility
  hidden?: boolean;
  disabled?: boolean;
  // Primary actions are shown as buttons, secondary in overflow menu
  primary?: boolean;
}

export interface BulkActionToolbarProps<T extends { id: number | string }> {
  items: T[];
  selectedIds: (number | string)[];
  onSelectionChange: (ids: (number | string)[]) => void;
  actions: BulkAction<T>[];
  // Optional callbacks
  onSelectAll?: (selected: boolean) => void;
  onClearSelection?: () => void;
  // Display
  entityName?: string;
  entityNamePlural?: string;
  showWhenEmpty?: boolean;
  // Styling
  sticky?: boolean;
  elevated?: boolean;
}

export function BulkActionToolbar<T extends { id: number | string }>({
  items,
  selectedIds,
  onSelectionChange,
  actions,
  onSelectAll,
  onClearSelection,
  entityName = 'item',
  entityNamePlural = 'items',
  showWhenEmpty = false,
  sticky = false,
  elevated = true,
}: BulkActionToolbarProps<T>): React.ReactElement | null {
  const theme = useTheme();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [confirmAction, setConfirmAction] = useState<BulkAction<T> | null>(null);
  const [dialogAction, setDialogAction] = useState<BulkAction<T> | null>(null);
  const [loading, setLoading] = useState(false);

  const selectedCount = selectedIds.length;
  const isAllSelected = items.length > 0 && selectedIds.length === items.length;
  const isIndeterminate = selectedIds.length > 0 && selectedIds.length < items.length;

  // Don't render if nothing selected (unless showWhenEmpty)
  if (selectedCount === 0 && !showWhenEmpty) {
    return null;
  }

  // Handle select all
  const handleSelectAll = (checked: boolean) => {
    if (checked) {
      onSelectionChange(items.map((item) => item.id));
      onSelectAll?.(true);
    } else {
      onSelectionChange([]);
      onSelectAll?.(false);
    }
  };

  // Handle clear
  const handleClear = () => {
    onSelectionChange([]);
    onClearSelection?.();
  };

  // Handle action click
  const handleActionClick = async (action: BulkAction<T>) => {
    if (action.disabled) return;

    if (action.requiresConfirmation) {
      setConfirmAction(action);
      return;
    }

    if (action.hasDialog) {
      setDialogAction(action);
      return;
    }

    if (action.onClick) {
      setLoading(true);
      try {
        await action.onClick(selectedIds);
      } finally {
        setLoading(false);
      }
    }
  };

  // Handle confirmation
  const handleConfirm = async () => {
    if (!confirmAction?.onClick) return;

    setLoading(true);
    try {
      await confirmAction.onClick(selectedIds);
      setConfirmAction(null);
    } finally {
      setLoading(false);
    }
  };

  // Get confirmation message
  const getConfirmationMessage = (action: BulkAction<T>): string => {
    if (typeof action.confirmationMessage === 'function') {
      return action.confirmationMessage(selectedCount);
    }
    return action.confirmationMessage || `Are you sure you want to ${action.label.toLowerCase()} ${selectedCount} ${selectedCount === 1 ? entityName : entityNamePlural}?`;
  };

  // Separate primary and secondary actions
  const primaryActions = actions.filter((a) => a.primary && !a.hidden);
  const secondaryActions = actions.filter((a) => !a.primary && !a.hidden);

  return (
    <>
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          p: 2,
          mb: 2,
          bgcolor: alpha(theme.palette.primary.main, 0.08),
          borderRadius: 1,
          ...(elevated && {
            boxShadow: theme.shadows[2],
          }),
          ...(sticky && {
            position: 'sticky',
            top: 0,
            zIndex: theme.zIndex.appBar - 1,
          }),
        }}
        role="toolbar"
        aria-label={`Bulk actions for ${selectedCount} selected ${selectedCount === 1 ? entityName : entityNamePlural}`}
      >
        <Stack direction="row" alignItems="center" spacing={2}>
          <FormControlLabel
            control={
              <Checkbox
                checked={isAllSelected}
                indeterminate={isIndeterminate}
                onChange={(e) => handleSelectAll(e.target.checked)}
                disabled={loading || items.length === 0}
                inputProps={{ 'aria-label': 'Select all' }}
              />
            }
            label=""
          />
          
          <Typography variant="subtitle2" component="span">
            {selectedCount} {selectedCount === 1 ? entityName : entityNamePlural} selected
          </Typography>

          {selectedCount > 0 && (
            <Tooltip title="Clear selection">
              <IconButton size="small" onClick={handleClear} aria-label="Clear selection">
                <CloseIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </Stack>

        <Stack direction="row" spacing={1} alignItems="center">
          {/* Primary actions */}
          {primaryActions.map((action) => (
            <Button
              key={action.id}
              size="small"
              startIcon={action.icon}
              onClick={() => handleActionClick(action)}
              disabled={loading || action.disabled || selectedCount === 0}
              color={action.color || 'inherit'}
              variant="text"
            >
              {action.label}
            </Button>
          ))}

          {/* Secondary actions (overflow menu) */}
          {secondaryActions.length > 0 && (
            <>
              <IconButton
                size="small"
                onClick={(e) => setAnchorEl(e.currentTarget)}
                disabled={loading || selectedCount === 0}
                aria-label="More actions"
                aria-haspopup="menu"
              >
                <MoreIcon />
              </IconButton>
              <Menu
                anchorEl={anchorEl}
                open={Boolean(anchorEl)}
                onClose={() => setAnchorEl(null)}
              >
                {secondaryActions.map((action, index) => (
                  <MenuItem
                    key={action.id}
                    onClick={() => {
                      setAnchorEl(null);
                      handleActionClick(action);
                    }}
                    disabled={action.disabled}
                  >
                    <Stack direction="row" spacing={1} alignItems="center">
                      {action.icon}
                      <Typography variant="body2">{action.label}</Typography>
                    </Stack>
                  </MenuItem>
                ))}
              </Menu>
            </>
          )}
        </Stack>
      </Box>

      {/* Confirmation Dialog */}
      <Dialog
        open={!!confirmAction}
        onClose={() => setConfirmAction(null)}
        maxWidth="xs"
        fullWidth
      >
        <DialogTitle>{confirmAction?.confirmationTitle || 'Confirm Action'}</DialogTitle>
        <DialogContent>
          <Typography>
            {confirmAction && getConfirmationMessage(confirmAction)}
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmAction(null)} disabled={loading}>
            Cancel
          </Button>
          <Button
            onClick={handleConfirm}
            variant="contained"
            color={confirmAction?.color || 'primary'}
            disabled={loading}
          >
            {confirmAction?.label || 'Confirm'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Custom Dialog for actions */}
      {dialogAction?.dialogComponent && (
        <dialogAction.dialogComponent
          open={true}
          onClose={() => setDialogAction(null)}
          selectedIds={selectedIds}
          onComplete={() => {
            setDialogAction(null);
            handleClear();
          }}
        />
      )}
    </>
  );
}

// Pre-configured common actions
export const createDeleteAction = <T,>(
  onDelete: (ids: (number | string)[]) => Promise<void>
): BulkAction<T> => ({
  id: 'delete',
  label: 'Delete',
  icon: <DeleteIcon />,
  color: 'error',
  primary: true,
  requiresConfirmation: true,
  confirmationTitle: 'Confirm Delete',
  confirmationMessage: (count) => `Are you sure you want to delete ${count} item${count !== 1 ? 's' : ''}? This action cannot be undone.`,
  onClick: onDelete,
});

export const createExportAction = <T,>(
  onExport: (ids: (number | string)[]) => Promise<void>
): BulkAction<T> => ({
  id: 'export',
  label: 'Export',
  icon: <ExportIcon />,
  primary: true,
  onClick: onExport,
});

export default BulkActionToolbar;
