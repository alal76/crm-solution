import { useState, useEffect } from 'react';
import {
  Box, Card, CardContent, Typography, Button, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, TablePagination, Dialog, DialogTitle, DialogContent, DialogActions, Alert, CircularProgress,
  TextField, Container, FormControl, InputLabel, Select, MenuItem, Chip, Tabs, Tab,
  FormControlLabel, Checkbox, Grid, IconButton, Tooltip, SelectChangeEvent
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, 
  Inventory as InventoryIcon, ShoppingCart as CartIcon,
  Subscriptions as SubscriptionIcon, Note as NoteIcon
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import { TabPanel, DialogError } from '../components/common';
import LookupSelect from '../components/LookupSelect';
import ImportExportButtons from '../components/ImportExportButtons';
import NotesTab from '../components/NotesTab';
import AdvancedSearch, { SearchField, SearchFilter, filterData } from '../components/AdvancedSearch';
import { usePagination } from '../hooks/usePagination';
import logo from '../assets/logo.png';
import { BaseEntity } from '../types';

// Search fields for Advanced Search
const SEARCH_FIELDS: SearchField[] = [
  { name: 'name', label: 'Product Name', type: 'text' },
  { name: 'sku', label: 'SKU', type: 'text' },
  { name: 'category', label: 'Category', type: 'select', options: [
    { value: 'Software', label: 'Software' },
    { value: 'Hardware', label: 'Hardware' },
    { value: 'Services', label: 'Services' },
    { value: 'Consulting', label: 'Consulting' },
    { value: 'Training', label: 'Training' },
    { value: 'Support', label: 'Support' },
    { value: 'Maintenance', label: 'Maintenance' },
    { value: 'Licensing', label: 'Licensing' },
    { value: 'Subscription', label: 'Subscription' },
    { value: 'Other', label: 'Other' },
  ]},
  { name: 'description', label: 'Description', type: 'text' },
  { name: 'status', label: 'Status', type: 'select', options: [
    { value: 0, label: 'Draft' },
    { value: 1, label: 'Active' },
    { value: 2, label: 'Discontinued' },
    { value: 3, label: 'Out of Stock' },
    { value: 4, label: 'Coming Soon' },
    { value: 5, label: 'Archived' },
  ]},
];

const SEARCHABLE_FIELDS = ['name', 'sku', 'category', 'description', 'shortDescription', 'tags'];

// Enums matching backend
const PRODUCT_TYPES = [
  { value: 0, label: 'Physical', icon: '📦' },
  { value: 1, label: 'Digital', icon: '💾' },
  { value: 2, label: 'Service', icon: '🛠️' },
  { value: 3, label: 'Subscription', icon: '🔄' },
  { value: 4, label: 'Bundle', icon: '📦📦' },
  { value: 5, label: 'Rental', icon: '🔑' },
];

const PRODUCT_STATUSES = [
  { value: 0, label: 'Draft', color: '#9e9e9e' },
  { value: 1, label: 'Active', color: '#4caf50' },
  { value: 2, label: 'Discontinued', color: '#f44336' },
  { value: 3, label: 'Out of Stock', color: '#ff9800' },
  { value: 4, label: 'Coming Soon', color: '#2196f3' },
  { value: 5, label: 'Archived', color: '#607d8b' },
];

const BILLING_FREQUENCIES = [
  { value: 0, label: 'One-Time' },
  { value: 1, label: 'Monthly' },
  { value: 2, label: 'Quarterly' },
  { value: 3, label: 'Semi-Annual' },
  { value: 4, label: 'Annual' },
  { value: 5, label: 'Custom' },
];

const CATEGORIES = [
  'Software', 'Hardware', 'Services', 'Consulting', 'Training', 
  'Support', 'Maintenance', 'Licensing', 'Subscription', 'Other'
];

interface Product extends BaseEntity {
  name: string;
  sku: string;
  barcode?: string;
  price: number;
  listPrice: number;
  minimumPrice: number;
  costPrice: number;
  margin: number;
  category: string;
  subcategory?: string;
  stock: number;
  productType: number;
  status: number;
  description?: string;
  shortDescription?: string;
  features?: string;
  // Subscription fields
  isSubscription: boolean;
  billingFrequency: number;
  recurringPrice: number;
  setupFee: number;
  trialPeriodDays: number;
  contractLengthMonths: number;
  // Inventory
  trackInventory: boolean;
  reorderLevel: number;
  reorderQuantity: number;
  warehouseLocation?: string;
  // Physical product
  weight?: number;
  dimensions?: string;
  // Media
  thumbnailUrl?: string;
  videoUrl?: string;
  // SEO
  slug?: string;
  metaTitle?: string;
  metaDescription?: string;
  isTaxable: boolean;
  taxRate: number;
  isFeatured: boolean;
  tags?: string;
}

interface ProductForm {
  name: string;
  sku: string;
  barcode: string;
  category: string;
  subcategory: string;
  price: number;
  listPrice: number;
  minimumPrice: number;
  costPrice: number;
  stock: number;
  productType: number;
  status: number;
  description: string;
  shortDescription: string;
  features: string;
  isSubscription: boolean;
  billingFrequency: number;
  recurringPrice: number;
  setupFee: number;
  trialPeriodDays: number;
  contractLengthMonths: number;
  trackInventory: boolean;
  reorderLevel: number;
  reorderQuantity: number;
  warehouseLocation: string;
  weight: number;
  dimensions: string;
  thumbnailUrl: string;
  videoUrl: string;
  slug: string;
  metaTitle: string;
  metaDescription: string;
  isTaxable: boolean;
  taxRate: number;
  isFeatured: boolean;
  tags: string;
}

function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [dialogError, setDialogError] = useState<string | null>(null);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [dialogTab, setDialogTab] = useState(0);
  const [searchFilters, setSearchFilters] = useState<SearchFilter[]>([]);
  const [searchText, setSearchText] = useState('');

  const handleSearch = (filters: SearchFilter[], text: string) => {
    setSearchFilters(filters);
    setSearchText(text);
  };

  const filteredProducts = filterData(products, searchFilters, searchText, SEARCHABLE_FIELDS);

  const {
    page,
    pageSize,
    paginatedData: paginatedProducts,
    handlePageChange,
    handlePageSizeChange,
    pageSizeOptions,
  } = usePagination(filteredProducts, { defaultPageSize: 25 });

  const emptyForm: ProductForm = {
    name: '', sku: '', barcode: '', category: '', subcategory: '', price: 0,
    listPrice: 0, minimumPrice: 0, costPrice: 0, stock: 0, productType: 0, status: 1,
    description: '', shortDescription: '', features: '', isSubscription: false,
    billingFrequency: 0, recurringPrice: 0, setupFee: 0, trialPeriodDays: 0,
    contractLengthMonths: 0, trackInventory: true, reorderLevel: 10, reorderQuantity: 50,
    warehouseLocation: '', weight: 0, dimensions: '', thumbnailUrl: '', videoUrl: '',
    slug: '', metaTitle: '', metaDescription: '', isTaxable: true, taxRate: 0,
    isFeatured: false, tags: '',
  };
  const [formData, setFormData] = useState<ProductForm>(emptyForm);

  useEffect(() => { fetchProducts(); }, []);

  const fetchProducts = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/products');
      setProducts(response.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch products');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDialog = (product?: Product) => {
    setDialogTab(0);
    if (product) {
      setEditingId(product.id);
      setFormData({
        name: product.name, sku: product.sku, barcode: product.barcode || '',
        category: product.category, subcategory: product.subcategory || '',
        price: product.price, listPrice: product.listPrice, minimumPrice: product.minimumPrice,
        costPrice: product.costPrice, stock: product.stock, productType: product.productType,
        status: product.status, description: product.description || '',
        shortDescription: product.shortDescription || '', features: product.features || '',
        isSubscription: product.isSubscription, billingFrequency: product.billingFrequency,
        recurringPrice: product.recurringPrice, setupFee: product.setupFee,
        trialPeriodDays: product.trialPeriodDays, contractLengthMonths: product.contractLengthMonths,
        trackInventory: product.trackInventory, reorderLevel: product.reorderLevel,
        reorderQuantity: product.reorderQuantity, warehouseLocation: product.warehouseLocation || '',
        weight: product.weight || 0, dimensions: product.dimensions || '',
        thumbnailUrl: product.thumbnailUrl || '', videoUrl: product.videoUrl || '',
        slug: product.slug || '', metaTitle: product.metaTitle || '',
        metaDescription: product.metaDescription || '', isTaxable: product.isTaxable,
        taxRate: product.taxRate, isFeatured: product.isFeatured, tags: product.tags || '',
      });
    } else {
      setEditingId(null);
      setFormData(emptyForm);
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => { setOpenDialog(false); setEditingId(null); setDialogError(null); };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    const checked = (e.target as HTMLInputElement).checked;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : type === 'number' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleSelectChange = (e: SelectChangeEvent<string | number>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSaveProduct = async () => {
    if (!formData.name.trim() || !formData.sku.trim()) {
      setDialogError('Please fill in required fields (Name, SKU)');
      return;
    }
    try {
      if (editingId) {
        await apiClient.put(`/products/${editingId}`, formData);
        setSuccessMessage('Product updated successfully');
      } else {
        await apiClient.post('/products', formData);
        setSuccessMessage('Product created successfully');
      }
      handleCloseDialog();
      fetchProducts();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setDialogError(err.response?.data?.message || 'Failed to save product');
    }
  };

  const handleDeleteProduct = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this product?')) {
      try {
        await apiClient.delete(`/products/${id}`);
        setSuccessMessage('Product deleted successfully');
        fetchProducts();
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete product');
      }
    }
  };

  const getStatus = (value: number) => PRODUCT_STATUSES.find(s => s.value === value);
  const getType = (value: number) => PRODUCT_TYPES.find(t => t.value === value);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ py: 4 }}>
      <Container maxWidth="xl">
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
              <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
            </Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>Products</Typography>
          </Box>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <ImportExportButtons entityType="products" entityLabel="Products" onImportComplete={fetchProducts} />
            <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpenDialog()} sx={{ backgroundColor: '#6750A4' }}>
              Add Product
            </Button>
          </Box>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

        <AdvancedSearch
          fields={SEARCH_FIELDS}
          onSearch={handleSearch}
          placeholder="Search products by name, SKU, category..."
        />

        <Card>
          <CardContent sx={{ p: 0 }}>
            <TableContainer sx={{ overflowX: 'auto' }}>
              <Table sx={{ minWidth: 800 }}>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                  <TableCell><strong>Product</strong></TableCell>
                  <TableCell><strong>SKU</strong></TableCell>
                  <TableCell><strong>Type</strong></TableCell>
                  <TableCell><strong>Category</strong></TableCell>
                  <TableCell><strong>Price</strong></TableCell>
                  <TableCell><strong>Stock</strong></TableCell>
                  <TableCell><strong>Status</strong></TableCell>
                  <TableCell align="center"><strong>Actions</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {paginatedProducts.map((product) => {
                  const status = getStatus(product.status);
                  const type = getType(product.productType);
                  return (
                    <TableRow key={product.id} hover>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          {product.isSubscription ? <SubscriptionIcon sx={{ color: '#2196f3' }} /> : <InventoryIcon sx={{ color: '#666' }} />}
                          <Box>
                            <Typography fontWeight={500}>{product.name}</Typography>
                            {product.shortDescription && (
                              <Typography variant="caption" color="textSecondary" sx={{ display: 'block', maxWidth: 200, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                                {product.shortDescription}
                              </Typography>
                            )}
                          </Box>
                          {product.isFeatured && <Chip label="Featured" size="small" color="primary" sx={{ ml: 1 }} />}
                        </Box>
                      </TableCell>
                      <TableCell><Typography variant="body2" fontFamily="monospace">{product.sku}</Typography></TableCell>
                      <TableCell>
                        <Chip label={`${type?.icon || ''} ${type?.label || 'Unknown'}`} size="small" variant="outlined" />
                      </TableCell>
                      <TableCell>{product.category}</TableCell>
                      <TableCell>
                        <Box>
                          <Typography fontWeight={500}>${product.price.toFixed(2)}</Typography>
                          {product.listPrice > product.price && (
                            <Typography variant="caption" sx={{ textDecoration: 'line-through', color: 'text.secondary' }}>
                              ${product.listPrice.toFixed(2)}
                            </Typography>
                          )}
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <CartIcon fontSize="small" sx={{ color: product.stock > product.reorderLevel ? '#4caf50' : '#f44336' }} />
                          <Typography sx={{ color: product.stock > product.reorderLevel ? 'inherit' : '#f44336' }}>
                            {product.stock}
                          </Typography>
                          {product.stock <= product.reorderLevel && (
                            <Chip label="Low" size="small" color="error" />
                          )}
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Chip label={status?.label || 'Unknown'} size="small" sx={{ backgroundColor: status?.color, color: 'white' }} />
                      </TableCell>
                      <TableCell align="center">
                        <Tooltip title="Edit">
                          <IconButton size="small" onClick={() => handleOpenDialog(product)} sx={{ color: '#6750A4' }}>
                            <EditIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
                          <IconButton size="small" onClick={() => handleDeleteProduct(product.id)} sx={{ color: '#f44336' }}>
                            <DeleteIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
              </Table>
            </TableContainer>
            <TablePagination
              component="div"
              count={filteredProducts.length}
              page={page}
              onPageChange={handlePageChange}
              rowsPerPage={pageSize}
              onRowsPerPageChange={handlePageSizeChange}
              rowsPerPageOptions={pageSizeOptions}
              showFirstButton
              showLastButton
            />
            {products.length === 0 && (
              <Typography sx={{ textAlign: 'center', py: 4, color: 'textSecondary' }}>
                No products found. Add your first product to get started.
              </Typography>
            )}
          </CardContent>
        </Card>
      </Container>

      {/* Enhanced Add/Edit Product Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle sx={{ pb: 0 }}>{editingId ? 'Edit Product' : 'Add Product'}</DialogTitle>
        <Box sx={{ borderBottom: 1, borderColor: 'divider', px: 3 }}>
          <Tabs value={dialogTab} onChange={(_, v) => setDialogTab(v)}>
            <Tab label="Basic Info" />
            <Tab label="Pricing" />
            <Tab label="Subscription" />
            <Tab label="Inventory" />
            <Tab label="SEO & Media" />
            <Tab label="Notes" icon={<NoteIcon fontSize="small" />} iconPosition="start" />
          </Tabs>
        </Box>
        <DialogContent sx={{ pt: 2, minHeight: 400 }}>
          <DialogError error={dialogError} onClose={() => setDialogError(null)} />
          <TabPanel value={dialogTab} index={0}>
            <Grid container spacing={2}>
              <Grid item xs={8}>
                <TextField fullWidth label="Product Name" name="name" value={formData.name} onChange={handleInputChange} required />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="SKU" name="sku" value={formData.sku} onChange={handleInputChange} required />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="Barcode" name="barcode" value={formData.barcode} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={4}>
                <LookupSelect
                  category="ProductType"
                  name="productType"
                  value={formData.productType}
                  onChange={handleSelectChange}
                  label="Product Type"
                  fallback={PRODUCT_TYPES.map(t => ({ value: t.value, label: t.label }))}
                />
              </Grid>
              <Grid item xs={4}>
                <LookupSelect
                  category="ProductStatus"
                  name="status"
                  value={formData.status}
                  onChange={handleSelectChange}
                  label="Status"
                  fallback={PRODUCT_STATUSES.map(s => ({ value: s.value, label: s.label }))}
                />
              </Grid>
              <Grid item xs={6}>
                <LookupSelect
                  category="ProductCategory"
                  name="category"
                  value={formData.category}
                  onChange={handleSelectChange}
                  label="Category"
                  fallback={CATEGORIES.map(c => ({ value: c, label: c }))}
                />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Subcategory" name="subcategory" value={formData.subcategory} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Short Description" name="shortDescription" value={formData.shortDescription} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Full Description" name="description" value={formData.description} onChange={handleInputChange} multiline rows={3} />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Features (comma-separated)" name="features" value={formData.features} onChange={handleInputChange} placeholder="Feature 1, Feature 2, Feature 3" />
              </Grid>
              <Grid item xs={12}>
                <FormControlLabel
                  control={<Checkbox name="isFeatured" checked={formData.isFeatured} onChange={handleInputChange} />}
                  label="Featured Product"
                />
              </Grid>
            </Grid>
          </TabPanel>

          <TabPanel value={dialogTab} index={1}>
            <Grid container spacing={2}>
              <Grid item xs={4}>
                <TextField fullWidth label="Sale Price ($)" name="price" type="number" value={formData.price} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="List Price ($)" name="listPrice" type="number" value={formData.listPrice} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="Minimum Price ($)" name="minimumPrice" type="number" value={formData.minimumPrice} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="Cost Price ($)" name="costPrice" type="number" value={formData.costPrice} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={4}>
                <TextField 
                  fullWidth 
                  label="Margin (%)" 
                  value={formData.price > 0 && formData.costPrice > 0 ? (((formData.price - formData.costPrice) / formData.price) * 100).toFixed(1) : 0}
                  InputProps={{ readOnly: true }}
                />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="Tax Rate (%)" name="taxRate" type="number" value={formData.taxRate} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={12}>
                <FormControlLabel
                  control={<Checkbox name="isTaxable" checked={formData.isTaxable} onChange={handleInputChange} />}
                  label="Product is Taxable"
                />
              </Grid>
            </Grid>
          </TabPanel>

          <TabPanel value={dialogTab} index={2}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <FormControlLabel
                  control={<Checkbox name="isSubscription" checked={formData.isSubscription} onChange={handleInputChange} />}
                  label="This is a Subscription Product"
                />
              </Grid>
              {formData.isSubscription && (
                <>
                  <Grid item xs={4}>
                    <LookupSelect
                      category="BillingCycle"
                      name="billingFrequency"
                      value={formData.billingFrequency}
                      onChange={handleSelectChange}
                      label="Billing Frequency"
                      fallback={BILLING_FREQUENCIES.map(f => ({ value: f.value, label: f.label }))}
                    />
                  </Grid>
                  <Grid item xs={4}>
                    <TextField fullWidth label="Recurring Price ($)" name="recurringPrice" type="number" value={formData.recurringPrice} onChange={handleInputChange} />
                  </Grid>
                  <Grid item xs={4}>
                    <TextField fullWidth label="Setup Fee ($)" name="setupFee" type="number" value={formData.setupFee} onChange={handleInputChange} />
                  </Grid>
                  <Grid item xs={6}>
                    <TextField fullWidth label="Trial Period (Days)" name="trialPeriodDays" type="number" value={formData.trialPeriodDays} onChange={handleInputChange} />
                  </Grid>
                  <Grid item xs={6}>
                    <TextField fullWidth label="Contract Length (Months)" name="contractLengthMonths" type="number" value={formData.contractLengthMonths} onChange={handleInputChange} />
                  </Grid>
                </>
              )}
            </Grid>
          </TabPanel>

          <TabPanel value={dialogTab} index={3}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <FormControlLabel
                  control={<Checkbox name="trackInventory" checked={formData.trackInventory} onChange={handleInputChange} />}
                  label="Track Inventory"
                />
              </Grid>
              {formData.trackInventory && (
                <>
                  <Grid item xs={4}>
                    <TextField fullWidth label="Stock Quantity" name="stock" type="number" value={formData.stock} onChange={handleInputChange} />
                  </Grid>
                  <Grid item xs={4}>
                    <TextField fullWidth label="Reorder Level" name="reorderLevel" type="number" value={formData.reorderLevel} onChange={handleInputChange} />
                  </Grid>
                  <Grid item xs={4}>
                    <TextField fullWidth label="Reorder Quantity" name="reorderQuantity" type="number" value={formData.reorderQuantity} onChange={handleInputChange} />
                  </Grid>
                  <Grid item xs={12}>
                    <TextField fullWidth label="Warehouse Location" name="warehouseLocation" value={formData.warehouseLocation} onChange={handleInputChange} />
                  </Grid>
                </>
              )}
              <Grid item xs={4}>
                <TextField fullWidth label="Weight (lbs)" name="weight" type="number" value={formData.weight} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={8}>
                <TextField fullWidth label="Dimensions (LxWxH)" name="dimensions" value={formData.dimensions} onChange={handleInputChange} placeholder="10x5x3 inches" />
              </Grid>
            </Grid>
          </TabPanel>

          <TabPanel value={dialogTab} index={4}>
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <TextField fullWidth label="Thumbnail URL" name="thumbnailUrl" value={formData.thumbnailUrl} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Video URL" name="videoUrl" value={formData.videoUrl} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="URL Slug" name="slug" value={formData.slug} onChange={handleInputChange} placeholder="product-name" />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Meta Title" name="metaTitle" value={formData.metaTitle} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Meta Description" name="metaDescription" value={formData.metaDescription} onChange={handleInputChange} multiline rows={2} />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Tags (comma-separated)" name="tags" value={formData.tags} onChange={handleInputChange} placeholder="tag1, tag2, tag3" />
              </Grid>
            </Grid>
          </TabPanel>

          <TabPanel value={dialogTab} index={5}>
            {editingId ? (
              <NotesTab
                entityType="Product"
                entityId={editingId}
                entityName={formData.name || 'Product'}
              />
            ) : (
              <Alert severity="info" sx={{ mt: 2 }}>
                Please save the product first to add notes.
              </Alert>
            )}
          </TabPanel>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSaveProduct} variant="contained" sx={{ backgroundColor: '#6750A4' }}>
            {editingId ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default ProductsPage;
