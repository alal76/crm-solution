import { useState, useEffect, useCallback } from 'react';
import {
  Box, Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  IconButton, Button, TextField, Typography, Paper, Tooltip, Dialog, DialogTitle,
  DialogContent, DialogActions, FormControl, InputLabel, Select, MenuItem,
  Alert, CircularProgress, Chip, InputAdornment, Checkbox, FormControlLabel
} from '@mui/material';
import {
  Add as AddIcon, Delete as DeleteIcon, Edit as EditIcon,
  Save as SaveIcon, Cancel as CancelIcon, DragIndicator as DragIcon
} from '@mui/icons-material';
import apiClient from '../services/apiClient';

// Discount types matching backend enum
const DISCOUNT_TYPES = [
  { value: 0, label: 'None' },
  { value: 1, label: 'Percentage' },
  { value: 2, label: 'Fixed Amount' },
];

interface Product {
  id: number;
  name: string;
  sku?: string;
  price: number;
  listPrice?: number;
  category?: string;
}

interface QuoteLineItem {
  id: number;
  quoteId: number;
  lineNumber: number;
  productId?: number;
  product?: Product;
  sku?: string;
  name: string;
  description?: string;
  category?: string;
  quantity: number;
  unitOfMeasure?: string;
  unitPrice: number;
  listPrice?: number;
  costPrice?: number;
  discountType: number;
  discountPercent: number;
  discountAmount: number;
  discountReason?: string;
  taxRate: number;
  isTaxable: boolean;
  subtotal: number;
  totalDiscount: number;
  taxAmount: number;
  total: number;
  isIncluded: boolean;
  isOptional: boolean;
  internalNotes?: string;
}

interface QuoteLineItemForm {
  id?: number;
  productId: number | '';
  sku: string;
  name: string;
  description: string;
  quantity: number;
  unitOfMeasure: string;
  unitPrice: number;
  discountType: number;
  discountPercent: number;
  discountAmount: number;
  discountReason: string;
  taxRate: number;
  isTaxable: boolean;
  isIncluded: boolean;
  isOptional: boolean;
  internalNotes: string;
}

interface QuoteLineItemsEditorProps {
  quoteId: number;
  readOnly?: boolean;
  onTotalsChange?: (totals: { subtotal: number; discount: number; tax: number; total: number }) => void;
}

const emptyLineItemForm: QuoteLineItemForm = {
  productId: '',
  sku: '',
  name: '',
  description: '',
  quantity: 1,
  unitOfMeasure: 'each',
  unitPrice: 0,
  discountType: 0,
  discountPercent: 0,
  discountAmount: 0,
  discountReason: '',
  taxRate: 0,
  isTaxable: true,
  isIncluded: true,
  isOptional: false,
  internalNotes: '',
};

export default function QuoteLineItemsEditor({ quoteId, readOnly = false, onTotalsChange }: QuoteLineItemsEditorProps) {
  const [lineItems, setLineItems] = useState<QuoteLineItem[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<QuoteLineItemForm>(emptyLineItemForm);
  const [saving, setSaving] = useState(false);

  const fetchLineItems = useCallback(async () => {
    try {
      setLoading(true);
      const response = await apiClient.get(`/quotes/${quoteId}/lineitems`);
      setLineItems(response.data);
      setError(null);
      calculateTotals(response.data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch line items');
    } finally {
      setLoading(false);
    }
  }, [quoteId]);

  const fetchProducts = useCallback(async () => {
    try {
      const response = await apiClient.get('/products');
      setProducts(response.data);
    } catch (err) {
      console.error('Error fetching products:', err);
    }
  }, []);

  useEffect(() => {
    if (quoteId) {
      fetchLineItems();
      fetchProducts();
    }
  }, [quoteId, fetchLineItems, fetchProducts]);

  const calculateTotals = (items: QuoteLineItem[]) => {
    const included = items.filter(li => li.isIncluded);
    const subtotal = included.reduce((sum, li) => sum + li.subtotal, 0);
    const discount = included.reduce((sum, li) => sum + li.totalDiscount, 0);
    const tax = included.reduce((sum, li) => sum + li.taxAmount, 0);
    const total = included.reduce((sum, li) => sum + li.total, 0);
    
    if (onTotalsChange) {
      onTotalsChange({ subtotal, discount, tax, total });
    }
  };

  const calculateLineItemPreview = (form: QuoteLineItemForm) => {
    const subtotal = form.quantity * form.unitPrice;
    let discount = 0;
    if (form.discountType === 1) { // Percentage
      discount = subtotal * (form.discountPercent / 100);
    } else if (form.discountType === 2) { // Fixed
      discount = form.discountAmount;
    }
    const afterDiscount = subtotal - discount;
    const tax = form.isTaxable ? afterDiscount * (form.taxRate / 100) : 0;
    const total = afterDiscount + tax;
    return { subtotal, discount, tax, total };
  };

  const handleOpenAddDialog = () => {
    setEditingItem({ ...emptyLineItemForm });
    setEditDialogOpen(true);
  };

  const handleOpenEditDialog = (item: QuoteLineItem) => {
    setEditingItem({
      id: item.id,
      productId: item.productId || '',
      sku: item.sku || '',
      name: item.name,
      description: item.description || '',
      quantity: item.quantity,
      unitOfMeasure: item.unitOfMeasure || 'each',
      unitPrice: item.unitPrice,
      discountType: item.discountType,
      discountPercent: item.discountPercent,
      discountAmount: item.discountAmount,
      discountReason: item.discountReason || '',
      taxRate: item.taxRate,
      isTaxable: item.isTaxable,
      isIncluded: item.isIncluded,
      isOptional: item.isOptional,
      internalNotes: item.internalNotes || '',
    });
    setEditDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setEditDialogOpen(false);
    setEditingItem(emptyLineItemForm);
  };

  const handleProductSelect = (productId: number) => {
    const product = products.find(p => p.id === productId);
    if (product) {
      setEditingItem(prev => ({
        ...prev,
        productId: product.id,
        name: product.name,
        sku: product.sku || '',
        unitPrice: product.price,
        category: product.category || '',
      }));
    }
  };

  const handleFormChange = (field: keyof QuoteLineItemForm, value: any) => {
    setEditingItem(prev => ({ ...prev, [field]: value }));
  };

  const handleSaveLineItem = async () => {
    if (!editingItem.name.trim()) {
      setError('Please enter a line item name');
      return;
    }
    if (editingItem.quantity <= 0) {
      setError('Quantity must be greater than 0');
      return;
    }

    setSaving(true);
    try {
      const payload = {
        ...editingItem,
        productId: editingItem.productId || null,
        quoteId: quoteId,
      };

      if (editingItem.id) {
        await apiClient.put(`/quotes/${quoteId}/lineitems/${editingItem.id}`, payload);
      } else {
        await apiClient.post(`/quotes/${quoteId}/lineitems`, payload);
      }

      handleCloseDialog();
      await fetchLineItems();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save line item');
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteLineItem = async (itemId: number) => {
    if (!window.confirm('Are you sure you want to remove this line item?')) return;
    
    try {
      await apiClient.delete(`/quotes/${quoteId}/lineitems/${itemId}`);
      await fetchLineItems();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete line item');
    }
  };

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
  };

  const preview = calculateLineItemPreview(editingItem);

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" p={3}>
        <CircularProgress size={24} />
        <Typography ml={2}>Loading line items...</Typography>
      </Box>
    );
  }

  return (
    <Box>
      {error && (
        <Alert severity="error" onClose={() => setError(null)} sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
        <Typography variant="h6">Line Items</Typography>
        {!readOnly && (
          <Button
            variant="contained"
            color="primary"
            startIcon={<AddIcon />}
            onClick={handleOpenAddDialog}
          >
            Add Line Item
          </Button>
        )}
      </Box>

      {lineItems.length === 0 ? (
        <Paper sx={{ p: 3, textAlign: 'center' }}>
          <Typography color="textSecondary">
            No line items yet. Click "Add Line Item" to add products or services to this quote.
          </Typography>
        </Paper>
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell width={50}>#</TableCell>
                <TableCell>Item</TableCell>
                <TableCell align="right">Qty</TableCell>
                <TableCell align="right">Unit Price</TableCell>
                <TableCell align="right">Discount</TableCell>
                <TableCell align="right">Tax</TableCell>
                <TableCell align="right">Total</TableCell>
                <TableCell width={100}>Status</TableCell>
                {!readOnly && <TableCell width={100}>Actions</TableCell>}
              </TableRow>
            </TableHead>
            <TableBody>
              {lineItems.map((item) => (
                <TableRow 
                  key={item.id}
                  sx={{ 
                    opacity: item.isIncluded ? 1 : 0.5,
                    backgroundColor: item.isOptional ? 'action.hover' : 'inherit'
                  }}
                >
                  <TableCell>{item.lineNumber}</TableCell>
                  <TableCell>
                    <Box>
                      <Typography variant="body2" fontWeight="medium">{item.name}</Typography>
                      {item.sku && (
                        <Typography variant="caption" color="textSecondary">SKU: {item.sku}</Typography>
                      )}
                      {item.description && (
                        <Typography variant="caption" display="block" color="textSecondary">
                          {item.description}
                        </Typography>
                      )}
                    </Box>
                  </TableCell>
                  <TableCell align="right">
                    {item.quantity} {item.unitOfMeasure}
                  </TableCell>
                  <TableCell align="right">{formatCurrency(item.unitPrice)}</TableCell>
                  <TableCell align="right">
                    {item.totalDiscount > 0 && (
                      <Typography color="error">-{formatCurrency(item.totalDiscount)}</Typography>
                    )}
                  </TableCell>
                  <TableCell align="right">
                    {item.taxAmount > 0 && formatCurrency(item.taxAmount)}
                  </TableCell>
                  <TableCell align="right">
                    <Typography fontWeight="medium">{formatCurrency(item.total)}</Typography>
                  </TableCell>
                  <TableCell>
                    <Box display="flex" gap={0.5}>
                      {item.isOptional && (
                        <Chip label="Optional" size="small" variant="outlined" />
                      )}
                      {!item.isIncluded && (
                        <Chip label="Excluded" size="small" color="warning" variant="outlined" />
                      )}
                    </Box>
                  </TableCell>
                  {!readOnly && (
                    <TableCell>
                      <Tooltip title="Edit">
                        <IconButton size="small" onClick={() => handleOpenEditDialog(item)}>
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Delete">
                        <IconButton size="small" color="error" onClick={() => handleDeleteLineItem(item.id)}>
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  )}
                </TableRow>
              ))}
              {/* Totals Row */}
              <TableRow sx={{ backgroundColor: 'grey.100' }}>
                <TableCell colSpan={6} align="right">
                  <Typography fontWeight="bold">Total:</Typography>
                </TableCell>
                <TableCell align="right">
                  <Typography fontWeight="bold">
                    {formatCurrency(lineItems.filter(li => li.isIncluded).reduce((sum, li) => sum + li.total, 0))}
                  </Typography>
                </TableCell>
                <TableCell colSpan={readOnly ? 1 : 2} />
              </TableRow>
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {/* Add/Edit Line Item Dialog */}
      <Dialog open={editDialogOpen} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle>{editingItem.id ? 'Edit Line Item' : 'Add Line Item'}</DialogTitle>
        <DialogContent dividers>
          <Box display="flex" flexDirection="column" gap={2} pt={1}>
            {/* Product Selection */}
            <FormControl fullWidth>
              <InputLabel>Select Product (Optional)</InputLabel>
              <Select
                value={editingItem.productId}
                onChange={(e) => handleProductSelect(e.target.value as number)}
                label="Select Product (Optional)"
              >
                <MenuItem value="">
                  <em>Manual Entry</em>
                </MenuItem>
                {products.map((product) => (
                  <MenuItem key={product.id} value={product.id}>
                    {product.name} {product.sku && `(${product.sku})`} - {formatCurrency(product.price)}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            {/* Name & SKU */}
            <Box display="flex" gap={2}>
              <TextField
                label="Name"
                value={editingItem.name}
                onChange={(e) => handleFormChange('name', e.target.value)}
                required
                fullWidth
              />
              <TextField
                label="SKU"
                value={editingItem.sku}
                onChange={(e) => handleFormChange('sku', e.target.value)}
                sx={{ width: 200 }}
              />
            </Box>

            {/* Description */}
            <TextField
              label="Description"
              value={editingItem.description}
              onChange={(e) => handleFormChange('description', e.target.value)}
              multiline
              rows={2}
              fullWidth
            />

            {/* Quantity & Price */}
            <Box display="flex" gap={2}>
              <TextField
                label="Quantity"
                type="number"
                value={editingItem.quantity}
                onChange={(e) => handleFormChange('quantity', Number.parseFloat(e.target.value) || 0)}
                required
                sx={{ width: 120 }}
                inputProps={{ min: 0, step: 1 }}
              />
              <TextField
                label="Unit"
                value={editingItem.unitOfMeasure}
                onChange={(e) => handleFormChange('unitOfMeasure', e.target.value)}
                sx={{ width: 100 }}
              />
              <TextField
                label="Unit Price"
                type="number"
                value={editingItem.unitPrice}
                onChange={(e) => handleFormChange('unitPrice', Number.parseFloat(e.target.value) || 0)}
                required
                fullWidth
                InputProps={{
                  startAdornment: <InputAdornment position="start">$</InputAdornment>,
                }}
                inputProps={{ min: 0, step: 0.01 }}
              />
            </Box>

            {/* Discount */}
            <Box display="flex" gap={2} alignItems="flex-start">
              <FormControl sx={{ width: 160 }}>
                <InputLabel>Discount Type</InputLabel>
                <Select
                  value={editingItem.discountType}
                  onChange={(e) => handleFormChange('discountType', e.target.value)}
                  label="Discount Type"
                >
                  {DISCOUNT_TYPES.map((dt) => (
                    <MenuItem key={dt.value} value={dt.value}>{dt.label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
              {editingItem.discountType === 1 && (
                <TextField
                  label="Discount %"
                  type="number"
                  value={editingItem.discountPercent}
                  onChange={(e) => handleFormChange('discountPercent', Number.parseFloat(e.target.value) || 0)}
                  sx={{ width: 120 }}
                  InputProps={{
                    endAdornment: <InputAdornment position="end">%</InputAdornment>,
                  }}
                  inputProps={{ min: 0, max: 100, step: 0.5 }}
                />
              )}
              {editingItem.discountType === 2 && (
                <TextField
                  label="Discount Amount"
                  type="number"
                  value={editingItem.discountAmount}
                  onChange={(e) => handleFormChange('discountAmount', Number.parseFloat(e.target.value) || 0)}
                  sx={{ width: 150 }}
                  InputProps={{
                    startAdornment: <InputAdornment position="start">$</InputAdornment>,
                  }}
                  inputProps={{ min: 0, step: 0.01 }}
                />
              )}
              {editingItem.discountType !== 0 && (
                <TextField
                  label="Discount Reason"
                  value={editingItem.discountReason}
                  onChange={(e) => handleFormChange('discountReason', e.target.value)}
                  fullWidth
                />
              )}
            </Box>

            {/* Tax */}
            <Box display="flex" gap={2} alignItems="center">
              <FormControlLabel
                control={
                  <Checkbox
                    checked={editingItem.isTaxable}
                    onChange={(e) => handleFormChange('isTaxable', e.target.checked)}
                  />
                }
                label="Taxable"
              />
              {editingItem.isTaxable && (
                <TextField
                  label="Tax Rate"
                  type="number"
                  value={editingItem.taxRate}
                  onChange={(e) => handleFormChange('taxRate', Number.parseFloat(e.target.value) || 0)}
                  sx={{ width: 120 }}
                  InputProps={{
                    endAdornment: <InputAdornment position="end">%</InputAdornment>,
                  }}
                  inputProps={{ min: 0, max: 100, step: 0.25 }}
                />
              )}
            </Box>

            {/* Options */}
            <Box display="flex" gap={2}>
              <FormControlLabel
                control={
                  <Checkbox
                    checked={editingItem.isIncluded}
                    onChange={(e) => handleFormChange('isIncluded', e.target.checked)}
                  />
                }
                label="Include in quote total"
              />
              <FormControlLabel
                control={
                  <Checkbox
                    checked={editingItem.isOptional}
                    onChange={(e) => handleFormChange('isOptional', e.target.checked)}
                  />
                }
                label="Optional item"
              />
            </Box>

            {/* Notes */}
            <TextField
              label="Notes"
              value={editingItem.internalNotes}
              onChange={(e) => handleFormChange('internalNotes', e.target.value)}
              multiline
              rows={2}
              fullWidth
            />

            {/* Preview */}
            <Paper sx={{ p: 2, backgroundColor: 'grey.50' }}>
              <Typography variant="subtitle2" gutterBottom>Line Item Preview</Typography>
              <Box display="flex" justifyContent="space-between">
                <Typography>Subtotal ({editingItem.quantity} × {formatCurrency(editingItem.unitPrice)})</Typography>
                <Typography>{formatCurrency(preview.subtotal)}</Typography>
              </Box>
              {preview.discount > 0 && (
                <Box display="flex" justifyContent="space-between">
                  <Typography color="error">Discount</Typography>
                  <Typography color="error">-{formatCurrency(preview.discount)}</Typography>
                </Box>
              )}
              {preview.tax > 0 && (
                <Box display="flex" justifyContent="space-between">
                  <Typography>Tax ({editingItem.taxRate}%)</Typography>
                  <Typography>{formatCurrency(preview.tax)}</Typography>
                </Box>
              )}
              <Box display="flex" justifyContent="space-between" mt={1} pt={1} borderTop={1} borderColor="divider">
                <Typography fontWeight="bold">Total</Typography>
                <Typography fontWeight="bold">{formatCurrency(preview.total)}</Typography>
              </Box>
            </Paper>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog} disabled={saving}>Cancel</Button>
          <Button 
            onClick={handleSaveLineItem} 
            variant="contained" 
            color="primary"
            disabled={saving}
            startIcon={saving ? <CircularProgress size={16} /> : <SaveIcon />}
          >
            {editingItem.id ? 'Update' : 'Add'} Line Item
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
