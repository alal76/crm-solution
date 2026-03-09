/**
 * Address List Component
 * Displays a list of addresses with actions (edit, delete, set as primary)
 */
import React, { useState, useEffect } from 'react';
import {
  Box,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Button,
  IconButton,
  CircularProgress,
  Alert,
  Tooltip,
  Chip,
  Stack,
  Typography,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
} from '@mui/material';
import {
  Edit as EditIcon,
  Delete as DeleteIcon,
  LocationOn as LocationIcon,
} from '@mui/icons-material';
import { Address } from '../../types/address.types';
import addressService from '../../services/addressService';

export interface AddressListComponentProps {
  accountId: number;
  addresses?: Address[];
  isLoading?: boolean;
  error?: string | null;
  onAddClick?: () => void;
  onEditClick?: (address: Address) => void;
  onDeleteSuccess?: () => void;
  onSetPrimaryClick?: (address: Address, type: 'billing' | 'shipping') => void;
}

const AddressListComponent: React.FC<AddressListComponentProps> = ({
  accountId,
  addresses = [],
  isLoading = false,
  error = null,
  onAddClick,
  onEditClick,
  onDeleteSuccess,
  onSetPrimaryClick,
}) => {
  const [localAddresses, setLocalAddresses] = useState<Address[]>(addresses);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [selectedAddressToDelete, setSelectedAddressToDelete] = useState<Address | null>(null);
  const [deleting, setDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  useEffect(() => {
    setLocalAddresses(addresses);
  }, [addresses]);

  const handleDeleteClick = (address: Address) => {
    setSelectedAddressToDelete(address);
    setDeleteConfirmOpen(true);
    setDeleteError(null);
  };

  const handleConfirmDelete = async () => {
    if (!selectedAddressToDelete) return;

    setDeleting(true);
    setDeleteError(null);

    try {
      await addressService.deleteAddress(accountId, selectedAddressToDelete.id);
      setLocalAddresses((prev) =>
        prev.filter((a) => a.id !== selectedAddressToDelete.id)
      );
      setDeleteConfirmOpen(false);
      setSelectedAddressToDelete(null);
      onDeleteSuccess?.();
    } catch (err: unknown) {
      setDeleteError(
        (err as any)?.response?.data?.message || 'Failed to delete address. Please try again.'
      );
    } finally {
      setDeleting(false);
    }
  };

  const handleCancelDelete = () => {
    setDeleteConfirmOpen(false);
    setSelectedAddressToDelete(null);
    setDeleteError(null);
  };

  const handleSetPrimary = async (address: Address, type: 'billing' | 'shipping') => {
    try {
      if (type === 'billing') {
        await addressService.setPrimaryBillingAddress(accountId, address.id);
      } else {
        await addressService.setPrimaryShippingAddress(accountId, address.id);
      }
      onSetPrimaryClick?.(address, type);
    } catch (err: unknown) {
      console.error(`Error setting primary ${type} address:`, err);
    }
  };

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return <Alert severity="error">{error}</Alert>;
  }

  if (localAddresses.length === 0) {
    return (
      <Paper sx={{ p: 3, textAlign: 'center' }}>
        <LocationIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 1 }} />
        <p>No addresses found</p>
        {onAddClick && (
          <Button
            variant="contained"
            color="primary"
            onClick={onAddClick}
            sx={{ mt: 2 }}
          >
            Add Address
          </Button>
        )}
      </Paper>
    );
  }

  return (
    <>
      <Box sx={{ mb: 2, display: 'flex', justifyContent: 'flex-end' }}>
        {onAddClick && (
          <Button
            variant="contained"
            color="primary"
            onClick={onAddClick}
          >
            Add Address
          </Button>
        )}
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow sx={{ backgroundColor: '#f5f5f5' }}>
              <TableCell><strong>Location</strong></TableCell>
              <TableCell><strong>Type</strong></TableCell>
              <TableCell><strong>Label</strong></TableCell>
              <TableCell><strong>Primary</strong></TableCell>
              <TableCell><strong>Actions</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {localAddresses.map((address) => (
              <TableRow key={address.id}>
                <TableCell>
                  <div>
                    {address.line1}
                    {address.line2 && <Typography variant="caption" display="block" color="text.secondary">{address.line2}</Typography>}
                    <Typography variant="caption" display="block" color="text.secondary">
                      {address.city}, {address.state} {address.zipCode}
                    </Typography>
                    <Typography variant="caption" display="block" color="text.secondary">{address.country}</Typography>
                  </div>
                </TableCell>
                <TableCell>{address.addressType}</TableCell>
                <TableCell>{address.label || '-'}</TableCell>
                <TableCell>
                  {address.isPrimary && (
                    <Chip label="Primary" size="small" color="primary" variant="outlined" />
                  )}
                </TableCell>
                <TableCell>
                  <Stack direction="row" spacing={1}>
                    {onEditClick && (
                      <Tooltip title="Edit address">
                        <IconButton
                          size="small"
                          onClick={() => onEditClick(address)}
                          color="primary"
                        >
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    )}
                    <Tooltip title="Delete address">
                      <IconButton
                        size="small"
                        onClick={() => handleDeleteClick(address)}
                        color="error"
                      >
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </Stack>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteConfirmOpen} onClose={handleCancelDelete}>
        <DialogTitle>Delete Address</DialogTitle>
        <DialogContent>
          {deleteError && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {deleteError}
            </Alert>
          )}
          <p>
            Are you sure you want to delete this address?
          </p>
          {selectedAddressToDelete && (
            <Box sx={{ p: 2, backgroundColor: '#f5f5f5', borderRadius: 1, mt: 2 }}>
              <p style={{ margin: '0 0 8px 0' }}>
                <strong>{selectedAddressToDelete.line1}</strong>
              </p>
              {selectedAddressToDelete.line2 && (
                <p style={{ margin: '0 0 8px 0' }}>{selectedAddressToDelete.line2}</p>
              )}
              <p style={{ margin: '0 0 8px 0' }}>
                {selectedAddressToDelete.city}, {selectedAddressToDelete.state}{' '}
                {selectedAddressToDelete.zipCode}
              </p>
              <p style={{ margin: '0' }}>{selectedAddressToDelete.country}</p>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCancelDelete} disabled={deleting}>
            Cancel
          </Button>
          <Button
            onClick={handleConfirmDelete}
            color="error"
            variant="contained"
            disabled={deleting}
          >
            {deleting ? 'Deleting...' : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};

export default AddressListComponent;
