/**
 * EntitySelect Component
 * A reusable dropdown component for selecting entities with an "Add New" option
 * that opens a quick-create dialog for the respective entity type.
 */
import React, { useState, useEffect, useCallback } from 'react';
import {
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Grid,
  Box,
  Divider,
  ListItemIcon,
  ListItemText,
  CircularProgress,
  Alert,
} from '@mui/material';
import { Add as AddIcon } from '@mui/icons-material';
import apiClient from '../services/apiClient';

// Entity types supported by this component
export type EntityType = 'customer' | 'contact' | 'product' | 'opportunity' | 'user';

interface EntitySelectProps {
  entityType: EntityType;
  name: string;
  value: number | string;
  onChange: (e: React.ChangeEvent<{ name?: string; value: unknown }> | any) => void;
  label: string;
  required?: boolean;
  disabled?: boolean;
  showAddNew?: boolean;
  fullWidth?: boolean;
  size?: 'small' | 'medium';
  helperText?: string;
  error?: boolean;
  excludeIds?: number[];
}

// Entity item interfaces
interface CustomerItem {
  id: number;
  firstName: string;
  lastName: string;
  company?: string;
  displayName?: string;
}

interface ContactItem {
  id: number;
  firstName: string;
  lastName: string;
  company?: string;
  emailPrimary?: string;
}

interface ProductItem {
  id: number;
  name: string;
  sku?: string;
  price?: number;
}

interface OpportunityItem {
  id: number;
  name: string;
  amount?: number;
  stage?: number;
}

interface UserItem {
  id: number;
  firstName: string;
  lastName: string;
  email?: string;
  username?: string;
}

type EntityItem = CustomerItem | ContactItem | ProductItem | OpportunityItem | UserItem;

// Quick create form data interfaces
interface CustomerFormData {
  firstName: string;
  lastName: string;
  company: string;
  emailPrimary: string;
  phonePrimary: string;
}

interface ContactFormData {
  firstName: string;
  lastName: string;
  company: string;
  emailPrimary: string;
  phonePrimary: string;
  jobTitle: string;
}

interface ProductFormData {
  name: string;
  sku: string;
  price: number;
  category: string;
  description: string;
}

interface OpportunityFormData {
  name: string;
  description: string;
  amount: number;
  stage: number;
  closeDate: string;
}

const EntitySelect: React.FC<EntitySelectProps> = ({
  entityType,
  name,
  value,
  onChange,
  label,
  required = false,
  disabled = false,
  showAddNew = true,
  fullWidth = true,
  size = 'medium',
  helperText,
  error,
  excludeIds = [],
}) => {
  const [items, setItems] = useState<EntityItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [openDialog, setOpenDialog] = useState(false);
  const [saving, setSaving] = useState(false);
  const [dialogError, setDialogError] = useState<string | null>(null);

  // Form data for quick create
  const [customerForm, setCustomerForm] = useState<CustomerFormData>({
    firstName: '', lastName: '', company: '', emailPrimary: '', phonePrimary: ''
  });
  const [contactForm, setContactForm] = useState<ContactFormData>({
    firstName: '', lastName: '', company: '', emailPrimary: '', phonePrimary: '', jobTitle: ''
  });
  const [productForm, setProductForm] = useState<ProductFormData>({
    name: '', sku: '', price: 0, category: '', description: ''
  });
  const [opportunityForm, setOpportunityForm] = useState<OpportunityFormData>({
    name: '', description: '', amount: 0, stage: 0, closeDate: new Date().toISOString().split('T')[0]
  });

  // Fetch entities based on type
  const fetchEntities = useCallback(async () => {
    try {
      setLoading(true);
      let endpoint = '';
      switch (entityType) {
        case 'customer': endpoint = '/customers'; break;
        case 'contact': endpoint = '/contacts'; break;
        case 'product': endpoint = '/products'; break;
        case 'opportunity': endpoint = '/opportunities'; break;
        case 'user': endpoint = '/users'; break;
      }
      const response = await apiClient.get(endpoint);
      setItems(response.data);
    } catch (err) {
      console.error(`Failed to fetch ${entityType}s:`, err);
      setItems([]);
    } finally {
      setLoading(false);
    }
  }, [entityType]);

  useEffect(() => {
    fetchEntities();
  }, [fetchEntities]);

  // Get display text for an entity item
  const getDisplayText = (item: EntityItem): string => {
    switch (entityType) {
      case 'customer': {
        const c = item as CustomerItem;
        const name = c.displayName || `${c.firstName || ''} ${c.lastName || ''}`.trim();
        return c.company ? `${name} (${c.company})` : name || 'Unnamed';
      }
      case 'contact': {
        const ct = item as ContactItem;
        const name = `${ct.firstName || ''} ${ct.lastName || ''}`.trim();
        return ct.company ? `${name} (${ct.company})` : name || 'Unnamed';
      }
      case 'product': {
        const p = item as ProductItem;
        return p.sku ? `${p.name} (${p.sku})` : p.name;
      }
      case 'opportunity': {
        const o = item as OpportunityItem;
        return o.amount ? `${o.name} - $${o.amount.toLocaleString()}` : o.name;
      }
      case 'user': {
        const u = item as UserItem;
        return `${u.firstName || ''} ${u.lastName || ''}`.trim() || u.username || 'Unknown';
      }
      default:
        return 'Unknown';
    }
  };

  // Handle "Add New" click
  const handleAddNewClick = () => {
    // Reset forms
    setCustomerForm({ firstName: '', lastName: '', company: '', emailPrimary: '', phonePrimary: '' });
    setContactForm({ firstName: '', lastName: '', company: '', emailPrimary: '', phonePrimary: '', jobTitle: '' });
    setProductForm({ name: '', sku: '', price: 0, category: '', description: '' });
    setOpportunityForm({ name: '', description: '', amount: 0, stage: 0, closeDate: new Date().toISOString().split('T')[0] });
    setDialogError(null);
    setOpenDialog(true);
  };

  // Handle dialog close
  const handleCloseDialog = () => {
    setOpenDialog(false);
    setDialogError(null);
  };

  // Handle save new entity
  const handleSave = async () => {
    try {
      setSaving(true);
      setDialogError(null);
      let endpoint = '';
      let payload: any = {};

      switch (entityType) {
        case 'customer':
          endpoint = '/customers';
          payload = {
            ...customerForm,
            customerCategory: 0, // Individual
            lifecycleStage: 0,
          };
          if (!payload.firstName && !payload.lastName && !payload.company) {
            throw new Error('Please provide at least a name or company');
          }
          break;
        case 'contact':
          endpoint = '/contacts';
          payload = {
            ...contactForm,
            contactType: 0, // Contact
          };
          if (!payload.firstName && !payload.lastName) {
            throw new Error('Please provide at least a first or last name');
          }
          break;
        case 'product':
          endpoint = '/products';
          payload = {
            ...productForm,
            productType: 0,
            status: 1, // Active
            listPrice: productForm.price,
            minimumPrice: productForm.price * 0.8,
            costPrice: productForm.price * 0.5,
          };
          if (!payload.name) {
            throw new Error('Product name is required');
          }
          break;
        case 'opportunity':
          endpoint = '/opportunities';
          payload = {
            ...opportunityForm,
            probability: 50,
          };
          if (!payload.name) {
            throw new Error('Opportunity name is required');
          }
          break;
        case 'user':
          // Users cannot be quick-created
          throw new Error('Users cannot be created from this dialog');
      }

      const response = await apiClient.post(endpoint, payload);
      const newId = response.data.id;

      // Refresh the list
      await fetchEntities();

      // Select the new item
      onChange({ target: { name, value: newId } });

      handleCloseDialog();
    } catch (err: any) {
      setDialogError(err.response?.data?.message || err.message || 'Failed to create');
    } finally {
      setSaving(false);
    }
  };

  // Render quick create form based on entity type
  const renderQuickCreateForm = () => {
    switch (entityType) {
      case 'customer':
        return (
          <Grid container spacing={2}>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="First Name"
                value={customerForm.firstName}
                onChange={(e) => setCustomerForm({ ...customerForm, firstName: e.target.value })}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="Last Name"
                value={customerForm.lastName}
                onChange={(e) => setCustomerForm({ ...customerForm, lastName: e.target.value })}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Company"
                value={customerForm.company}
                onChange={(e) => setCustomerForm({ ...customerForm, company: e.target.value })}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="Email"
                type="email"
                value={customerForm.emailPrimary}
                onChange={(e) => setCustomerForm({ ...customerForm, emailPrimary: e.target.value })}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="Phone"
                value={customerForm.phonePrimary}
                onChange={(e) => setCustomerForm({ ...customerForm, phonePrimary: e.target.value })}
              />
            </Grid>
          </Grid>
        );

      case 'contact':
        return (
          <Grid container spacing={2}>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="First Name"
                value={contactForm.firstName}
                onChange={(e) => setContactForm({ ...contactForm, firstName: e.target.value })}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="Last Name"
                value={contactForm.lastName}
                onChange={(e) => setContactForm({ ...contactForm, lastName: e.target.value })}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="Company"
                value={contactForm.company}
                onChange={(e) => setContactForm({ ...contactForm, company: e.target.value })}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="Job Title"
                value={contactForm.jobTitle}
                onChange={(e) => setContactForm({ ...contactForm, jobTitle: e.target.value })}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="Email"
                type="email"
                value={contactForm.emailPrimary}
                onChange={(e) => setContactForm({ ...contactForm, emailPrimary: e.target.value })}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="Phone"
                value={contactForm.phonePrimary}
                onChange={(e) => setContactForm({ ...contactForm, phonePrimary: e.target.value })}
              />
            </Grid>
          </Grid>
        );

      case 'product':
        return (
          <Grid container spacing={2}>
            <Grid item xs={8}>
              <TextField
                fullWidth
                label="Product Name"
                required
                value={productForm.name}
                onChange={(e) => setProductForm({ ...productForm, name: e.target.value })}
              />
            </Grid>
            <Grid item xs={4}>
              <TextField
                fullWidth
                label="SKU"
                value={productForm.sku}
                onChange={(e) => setProductForm({ ...productForm, sku: e.target.value })}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="Price"
                type="number"
                value={productForm.price}
                onChange={(e) => setProductForm({ ...productForm, price: parseFloat(e.target.value) || 0 })}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="Category"
                value={productForm.category}
                onChange={(e) => setProductForm({ ...productForm, category: e.target.value })}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Description"
                multiline
                rows={2}
                value={productForm.description}
                onChange={(e) => setProductForm({ ...productForm, description: e.target.value })}
              />
            </Grid>
          </Grid>
        );

      case 'opportunity':
        return (
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Opportunity Name"
                required
                value={opportunityForm.name}
                onChange={(e) => setOpportunityForm({ ...opportunityForm, name: e.target.value })}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="Amount"
                type="number"
                value={opportunityForm.amount}
                onChange={(e) => setOpportunityForm({ ...opportunityForm, amount: parseFloat(e.target.value) || 0 })}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="Close Date"
                type="date"
                value={opportunityForm.closeDate}
                onChange={(e) => setOpportunityForm({ ...opportunityForm, closeDate: e.target.value })}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Description"
                multiline
                rows={2}
                value={opportunityForm.description}
                onChange={(e) => setOpportunityForm({ ...opportunityForm, description: e.target.value })}
              />
            </Grid>
          </Grid>
        );

      case 'user':
        return (
          <Alert severity="info">
            Users must be created through the User Management section.
          </Alert>
        );

      default:
        return null;
    }
  };

  // Get dialog title based on entity type
  const getDialogTitle = () => {
    switch (entityType) {
      case 'customer': return 'Quick Create Customer';
      case 'contact': return 'Quick Create Contact';
      case 'product': return 'Quick Create Product';
      case 'opportunity': return 'Quick Create Opportunity';
      case 'user': return 'Create User';
      default: return 'Quick Create';
    }
  };

  // Filter out excluded IDs
  const filteredItems = items.filter(item => !excludeIds.includes((item as any).id));

  return (
    <>
      <FormControl fullWidth={fullWidth} size={size} error={error} required={required} disabled={disabled}>
        <InputLabel>{label}</InputLabel>
        <Select
          name={name}
          value={value}
          onChange={onChange}
          label={label}
        >
          {/* Empty/None option */}
          <MenuItem value="">
            <em>None</em>
          </MenuItem>

          {/* Add New option - only show if showAddNew is true and not for users */}
          {showAddNew && entityType !== 'user' && (
            <>
              <MenuItem
                value="__add_new__"
                onClick={(e) => {
                  e.preventDefault();
                  e.stopPropagation();
                  handleAddNewClick();
                }}
                sx={{
                  color: 'primary.main',
                  fontWeight: 600,
                  '&:hover': { backgroundColor: 'primary.light', color: 'primary.contrastText' }
                }}
              >
                <ListItemIcon sx={{ minWidth: 36 }}>
                  <AddIcon color="primary" />
                </ListItemIcon>
                <ListItemText primary={`Add New ${label}`} />
              </MenuItem>
              <Divider />
            </>
          )}

          {/* Loading state */}
          {loading && (
            <MenuItem disabled>
              <CircularProgress size={20} sx={{ mr: 1 }} />
              Loading...
            </MenuItem>
          )}

          {/* Entity items */}
          {filteredItems.map((item) => (
            <MenuItem key={(item as any).id} value={(item as any).id}>
              {getDisplayText(item)}
            </MenuItem>
          ))}

          {/* No items message */}
          {!loading && filteredItems.length === 0 && (
            <MenuItem disabled>
              <em>No {entityType}s available</em>
            </MenuItem>
          )}
        </Select>
        {helperText && (
          <Box sx={{ fontSize: '0.75rem', color: error ? 'error.main' : 'text.secondary', mt: 0.5, ml: 1.5 }}>
            {helperText}
          </Box>
        )}
      </FormControl>

      {/* Quick Create Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{getDialogTitle()}</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          {dialogError && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {dialogError}
            </Alert>
          )}
          {renderQuickCreateForm()}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog} disabled={saving}>
            Cancel
          </Button>
          {entityType !== 'user' && (
            <Button
              onClick={handleSave}
              variant="contained"
              disabled={saving}
              sx={{ backgroundColor: '#6750A4' }}
            >
              {saving ? <CircularProgress size={20} /> : 'Create & Select'}
            </Button>
          )}
        </DialogActions>
      </Dialog>
    </>
  );
};

export default EntitySelect;
