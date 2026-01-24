import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Grid,
  IconButton,
  List,
  ListItem,
  ListItemSecondaryAction,
  ListItemText,
  MenuItem,
  Select,
  TextField,
  Tooltip,
  Typography,
  FormControlLabel,
  Checkbox,
  Alert,
  CircularProgress,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  Edit as EditIcon,
  Star as StarIcon,
  StarOutline as StarOutlineIcon,
  Share as ShareIcon,
  LocationOn as LocationIcon,
  Home as HomeIcon,
  Business as BusinessIcon,
} from '@mui/icons-material';
import { contactInfoService, EntityType, LinkedAddressDto, CreateAddressDto, AddressType } from '../../services/contactInfoService';

interface AddressManagerProps {
  entityType: EntityType;
  entityId: number;
  readOnly?: boolean;
  onAddressChange?: () => void;
}

const ADDRESS_TYPES: AddressType[] = ['Primary', 'Billing', 'Shipping', 'Mailing', 'Headquarters', 'Branch', 'Warehouse', 'Other'];

const AddressManager: React.FC<AddressManagerProps> = ({
  entityType,
  entityId,
  readOnly = false,
  onAddressChange,
}) => {
  const [addresses, setAddresses] = useState<LinkedAddressDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingAddress, setEditingAddress] = useState<LinkedAddressDto | null>(null);
  const [formData, setFormData] = useState<CreateAddressDto & { addressType: AddressType; isPrimary: boolean }>({
    line1: '',
    line2: '',
    line3: '',
    city: '',
    state: '',
    postalCode: '',
    county: '',
    countryCode: 'US',
    country: 'United States',
    isResidential: false,
    deliveryInstructions: '',
    accessHours: '',
    siteContactName: '',
    siteContactPhone: '',
    notes: '',
    addressType: 'Primary',
    isPrimary: false,
  });

  const fetchAddresses = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await contactInfoService.getAddresses(entityType, entityId);
      setAddresses(response.data || []);
    } catch (err) {
      setError('Failed to load addresses');
      console.error('Error fetching addresses:', err);
    } finally {
      setLoading(false);
    }
  }, [entityType, entityId]);

  useEffect(() => {
    if (entityId) {
      fetchAddresses();
    }
  }, [entityId, fetchAddresses]);

  const handleOpenDialog = (address?: LinkedAddressDto) => {
    if (address) {
      setEditingAddress(address);
      setFormData({
        line1: address.line1,
        line2: address.line2 || '',
        line3: address.line3 || '',
        city: address.city,
        state: address.state,
        postalCode: address.postalCode,
        county: address.county || '',
        countryCode: address.countryCode || 'US',
        country: address.country || 'United States',
        isResidential: address.isResidential || false,
        deliveryInstructions: address.deliveryInstructions || '',
        accessHours: address.accessHours || '',
        siteContactName: address.siteContactName || '',
        siteContactPhone: address.siteContactPhone || '',
        notes: address.notes || '',
        addressType: address.addressType,
        isPrimary: address.isPrimary,
      });
    } else {
      setEditingAddress(null);
      setFormData({
        line1: '',
        line2: '',
        line3: '',
        city: '',
        state: '',
        postalCode: '',
        county: '',
        countryCode: 'US',
        country: 'United States',
        isResidential: false,
        deliveryInstructions: '',
        accessHours: '',
        siteContactName: '',
        siteContactPhone: '',
        notes: '',
        addressType: 'Primary',
        isPrimary: addresses.length === 0, // First address is primary by default
      });
    }
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setEditingAddress(null);
  };

  const handleSave = async () => {
    try {
      const { addressType, isPrimary, ...addressData } = formData;

      if (editingAddress) {
        // Update existing address
        await contactInfoService.updateAddress(editingAddress.id, addressData);
        // Update link if needed
        if (isPrimary && !editingAddress.isPrimary) {
          await contactInfoService.setPrimaryAddress(entityType, entityId, editingAddress.id, addressType);
        }
      } else {
        // Create and link new address
        await contactInfoService.linkAddress({
          newAddress: addressData,
          entityType,
          entityId,
          addressType,
          isPrimary,
        });
      }

      await fetchAddresses();
      handleCloseDialog();
      onAddressChange?.();
    } catch (err) {
      setError('Failed to save address');
      console.error('Error saving address:', err);
    }
  };

  const handleDelete = async (address: LinkedAddressDto) => {
    if (window.confirm('Are you sure you want to remove this address?')) {
      try {
        await contactInfoService.unlinkAddress(address.linkId);
        await fetchAddresses();
        onAddressChange?.();
      } catch (err) {
        setError('Failed to remove address');
        console.error('Error deleting address:', err);
      }
    }
  };

  const handleSetPrimary = async (address: LinkedAddressDto) => {
    try {
      await contactInfoService.setPrimaryAddress(entityType, entityId, address.id, address.addressType);
      await fetchAddresses();
      onAddressChange?.();
    } catch (err) {
      setError('Failed to set primary address');
      console.error('Error setting primary address:', err);
    }
  };

  const getAddressIcon = (addressType: AddressType) => {
    switch (addressType) {
      case 'Headquarters':
      case 'Branch':
      case 'Billing':
        return <BusinessIcon fontSize="small" />;
      case 'Shipping':
      case 'Warehouse':
        return <LocationIcon fontSize="small" />;
      default:
        return <HomeIcon fontSize="small" />;
    }
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" p={3}>
        <CircularProgress size={24} />
      </Box>
    );
  }

  return (
    <Card variant="outlined">
      <CardHeader
        title="Addresses"
        titleTypographyProps={{ variant: 'h6' }}
        action={
          !readOnly && (
            <Button
              startIcon={<AddIcon />}
              size="small"
              onClick={() => handleOpenDialog()}
            >
              Add Address
            </Button>
          )
        }
      />
      <CardContent>
        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        {addresses.length === 0 ? (
          <Typography variant="body2" color="textSecondary">
            No addresses on file
          </Typography>
        ) : (
          <List dense>
            {addresses.map((address) => (
              <ListItem key={address.linkId} divider>
                <Box mr={1}>{getAddressIcon(address.addressType)}</Box>
                <ListItemText
                  primary={
                    <Box display="flex" alignItems="center" gap={1}>
                      <Typography variant="body1">
                        {address.line1}
                        {address.line2 && `, ${address.line2}`}
                      </Typography>
                      <Chip
                        label={address.addressType}
                        size="small"
                        variant="outlined"
                      />
                      {address.isPrimary && (
                        <Chip
                          label="Primary"
                          size="small"
                          color="primary"
                        />
                      )}
                    </Box>
                  }
                  secondary={
                    <>
                      {address.city}, {address.state} {address.postalCode}
                      {address.country && `, ${address.country}`}
                      {address.isVerified && (
                        <Chip label="Verified" size="small" color="success" sx={{ ml: 1 }} />
                      )}
                    </>
                  }
                />
                {!readOnly && (
                  <ListItemSecondaryAction>
                    {!address.isPrimary && (
                      <Tooltip title="Set as Primary">
                        <IconButton size="small" onClick={() => handleSetPrimary(address)}>
                          <StarOutlineIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    )}
                    {address.isPrimary && (
                      <Tooltip title="Primary Address">
                        <IconButton size="small" disabled>
                          <StarIcon fontSize="small" color="primary" />
                        </IconButton>
                      </Tooltip>
                    )}
                    <Tooltip title="Edit">
                      <IconButton size="small" onClick={() => handleOpenDialog(address)}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Remove">
                      <IconButton size="small" onClick={() => handleDelete(address)}>
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </ListItemSecondaryAction>
                )}
              </ListItem>
            ))}
          </List>
        )}
      </CardContent>

      {/* Add/Edit Dialog */}
      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle>
          {editingAddress ? 'Edit Address' : 'Add Address'}
        </DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12} sm={6}>
              <Select
                fullWidth
                value={formData.addressType}
                onChange={(e) => setFormData({ ...formData, addressType: e.target.value as AddressType })}
                size="small"
                label="Address Type"
              >
                {ADDRESS_TYPES.map((type) => (
                  <MenuItem key={type} value={type}>
                    {type}
                  </MenuItem>
                ))}
              </Select>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={
                  <Checkbox
                    checked={formData.isPrimary}
                    onChange={(e) => setFormData({ ...formData, isPrimary: e.target.checked })}
                  />
                }
                label="Primary Address"
              />
            </Grid>

            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Address Line 1"
                value={formData.line1}
                onChange={(e) => setFormData({ ...formData, line1: e.target.value })}
                required
                size="small"
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Address Line 2"
                value={formData.line2}
                onChange={(e) => setFormData({ ...formData, line2: e.target.value })}
                size="small"
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Address Line 3"
                value={formData.line3}
                onChange={(e) => setFormData({ ...formData, line3: e.target.value })}
                size="small"
              />
            </Grid>

            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="City"
                value={formData.city}
                onChange={(e) => setFormData({ ...formData, city: e.target.value })}
                required
                size="small"
              />
            </Grid>
            <Grid item xs={12} sm={3}>
              <TextField
                fullWidth
                label="State/Province"
                value={formData.state}
                onChange={(e) => setFormData({ ...formData, state: e.target.value })}
                required
                size="small"
              />
            </Grid>
            <Grid item xs={12} sm={3}>
              <TextField
                fullWidth
                label="Postal Code"
                value={formData.postalCode}
                onChange={(e) => setFormData({ ...formData, postalCode: e.target.value })}
                required
                size="small"
              />
            </Grid>

            <Grid item xs={12} sm={4}>
              <TextField
                fullWidth
                label="County"
                value={formData.county}
                onChange={(e) => setFormData({ ...formData, county: e.target.value })}
                size="small"
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField
                fullWidth
                label="Country Code"
                value={formData.countryCode}
                onChange={(e) => setFormData({ ...formData, countryCode: e.target.value })}
                size="small"
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField
                fullWidth
                label="Country"
                value={formData.country}
                onChange={(e) => setFormData({ ...formData, country: e.target.value })}
                size="small"
              />
            </Grid>

            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={
                  <Checkbox
                    checked={formData.isResidential || false}
                    onChange={(e) => setFormData({ ...formData, isResidential: e.target.checked })}
                  />
                }
                label="Residential Address"
              />
            </Grid>

            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Delivery Instructions"
                value={formData.deliveryInstructions}
                onChange={(e) => setFormData({ ...formData, deliveryInstructions: e.target.value })}
                multiline
                rows={2}
                size="small"
              />
            </Grid>

            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Access Hours"
                value={formData.accessHours}
                onChange={(e) => setFormData({ ...formData, accessHours: e.target.value })}
                placeholder="e.g., 9AM-5PM Mon-Fri"
                size="small"
              />
            </Grid>

            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Site Contact Name"
                value={formData.siteContactName}
                onChange={(e) => setFormData({ ...formData, siteContactName: e.target.value })}
                size="small"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Site Contact Phone"
                value={formData.siteContactPhone}
                onChange={(e) => setFormData({ ...formData, siteContactPhone: e.target.value })}
                size="small"
              />
            </Grid>

            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Notes"
                value={formData.notes}
                onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                multiline
                rows={2}
                size="small"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSave} variant="contained" color="primary">
            {editingAddress ? 'Save Changes' : 'Add Address'}
          </Button>
        </DialogActions>
      </Dialog>
    </Card>
  );
};

export default AddressManager;
