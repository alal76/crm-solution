import { useState, useEffect, useCallback, useMemo } from 'react';
import {
  Box, Card, CardContent, Typography, Button, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, TablePagination, Dialog, DialogTitle, DialogContent, DialogActions, Alert, CircularProgress,
  TextField, Container, FormControl, InputLabel, Select, MenuItem, Chip, Tabs, Tab,
  Grid, IconButton, Tooltip, SelectChangeEvent
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, 
  Inventory as InventoryIcon, ShoppingCart as CartIcon,
  Subscriptions as SubscriptionIcon, Note as NoteIcon
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import logger from '../services/logger';
import { DialogError, DialogSuccess, ActionButton } from '../components/common';

import ImportExportButtons from '../components/ImportExportButtons';
import NotesTab from '../components/NotesTab';
import AdvancedSearch, { SearchField, SearchFilter, filterData } from '../components/AdvancedSearch';
import DynamicEntityForm, { ExtraTab } from '../components/DynamicEntityForm';
import { usePagination } from '../hooks/usePagination';
import { useApiState } from '../hooks/useApiState';
import { useEntityTypeSubscription } from '../hooks/useSignalR';
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
  const [editingId, setEditingId] = useState<number | null>(null);
  const [dialogTab, setDialogTab] = useState(0);
  const [searchFilters, setSearchFilters] = useState<SearchFilter[]>([]);
  const [searchText, setSearchText] = useState('');

  // Dynamic field configuration
  // Field configuration now handled internally by DynamicEntityForm
  // API state for dialog operations
  const dialogApi = useApiState({ successTimeout: 3000 });

  const handleSearch = (filters: SearchFilter[], text: string) => {
    setSearchFilters(filters);
    setSearchText(text);
  };

  // Memoize filtered products for performance
  const filteredProducts = useMemo(() => {
    return filterData(products, searchFilters, searchText, SEARCHABLE_FIELDS);
  }, [products, searchFilters, searchText]);

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

  // Fetch products function (defined early for SignalR callbacks)
  const fetchProducts = useCallback(async () => {
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
  }, []);

  // SignalR subscription for real-time updates
  useEntityTypeSubscription('Product', {
    onCreated: useCallback(() => {
      logger.debug('[SignalR] Product created - refreshing list');
      fetchProducts();
    }, [fetchProducts]),
    onUpdated: useCallback(() => {
      logger.debug('[SignalR] Product updated - refreshing list');
      fetchProducts();
    }, [fetchProducts]),
    onDeleted: useCallback(() => {
      logger.debug('[SignalR] Product deleted - refreshing list');
      fetchProducts();
    }, [fetchProducts]),
  });

  useEffect(() => { fetchProducts(); }, [fetchProducts]);

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

  const handleCloseDialog = () => { setOpenDialog(false); setEditingId(null); dialogApi.reset(); };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    const checked = (e.target as HTMLInputElement).checked;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : type === 'number' ? Number.parseFloat(value) || 0 : value,
    }));
  };

  const handleSelectChange = (e: SelectChangeEvent<string | number>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSaveProduct = async () => {
    if (!formData.name.trim() || !formData.sku.trim()) {
      dialogApi.setError('Please fill in required fields (Name, SKU)');
      return;
    }
    
    const result = await dialogApi.execute(async () => {
      if (editingId) {
        await apiClient.put(`/products/${editingId}`, formData);
        return 'Product updated successfully';
      } else {
        await apiClient.post('/products', formData);
        return 'Product created successfully';
      }
    }, editingId ? 'Product updated successfully' : 'Product created successfully');
    
    if (result) {
      handleCloseDialog();
      fetchProducts();
    }
  };

  const handleDeleteProduct = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this product?')) {
      const result = await dialogApi.execute(async () => {
        await apiClient.delete(`/products/${id}`);
        return true;
      }, 'Product deleted successfully');
      
      if (result) {
        fetchProducts();
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
        <DialogContent sx={{ pt: 0, minHeight: 400 }}>
          <DialogError error={dialogApi.error} onClose={() => dialogApi.reset()} />

          <DynamicEntityForm
            moduleName="Products"
            formData={formData}
            onChange={handleInputChange}
            onSelectChange={(e: any) => setFormData(prev => ({ ...prev, [e.target.name]: e.target.value }))}
            setFormData={setFormData}
            activeTab={dialogTab}
            editingId={editingId}
            onTabChange={setDialogTab}
            excludeFields={['tags', 'customFields']}
            extraTabs={[
              {
                index: 100,
                name: 'Notes',
                icon: <NoteIcon fontSize="small" />,
                render: () => editingId ? (
                  <NotesTab entityType="Product" entityId={editingId} entityName={formData.name || 'Product'} />
                ) : (
                  <Alert severity="info" sx={{ mt: 2 }}>Please save the product first to add notes.</Alert>
                ),
              },
            ]}
          />
        </DialogContent>
        <DialogActions>
          <DialogError error={dialogApi.error} />
          <DialogSuccess message={dialogApi.success} />
          <Button onClick={handleCloseDialog} disabled={dialogApi.loading}>Cancel</Button>
          <ActionButton
            label={editingId ? 'Update' : 'Create'}
            loading={dialogApi.loading}
            onClick={handleSaveProduct}
            color="primary"
          />
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default ProductsPage;
