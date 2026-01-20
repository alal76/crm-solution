import { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
  CircularProgress,
  TextField,
  Container,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';

interface Product {
  id: number;
  name: string;
  sku: string;
  price: number;
  category: string;
  stock: number;
  status: number;
  createdAt: string;
}

interface ProductForm {
  name: string;
  sku: string;
  category: string;
  price: number;
  stock: number;
  status: number;
}

function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<ProductForm>({
    name: '',
    sku: '',
    category: '',
    price: 0,
    stock: 0,
    status: 1,
  });

  useEffect(() => {
    fetchProducts();
  }, []);

  const fetchProducts = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/products');
      setProducts(response.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch products');
      console.error('Error fetching products:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDialog = (product?: Product) => {
    if (product) {
      setEditingId(product.id);
      setFormData({
        name: product.name,
        sku: product.sku,
        category: product.category,
        price: product.price,
        stock: product.stock,
        status: product.status,
      });
    } else {
      setEditingId(null);
      setFormData({
        name: '',
        sku: '',
        category: '',
        price: 0,
        stock: 0,
        status: 1,
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'price' || name === 'stock' || name === 'status' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleSaveProduct = async () => {
    if (!formData.name.trim() || !formData.sku.trim() || !formData.category.trim()) {
      setError('Please fill in required fields (Name, SKU, Category)');
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
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save product');
      console.error('Error saving product:', err);
    }
  };

  const handleDeleteProduct = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this product?')) {
      try {
        await apiClient.delete(`/products/${id}`);
        setSuccessMessage('Product deleted successfully');
        fetchProducts();
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete product');
        console.error('Error deleting product:', err);
      }
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ py: 4 }}>
      <Container maxWidth="lg">
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Box sx={{ width: 40, height: 40, flexShrink: 0 }}><img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} /></Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>Products</Typography>
          </Box>
          <Button
            variant="contained"
            color="primary"
            startIcon={<AddIcon />}
            onClick={() => handleOpenDialog()}
            sx={{ backgroundColor: '#6750A4' }}
          >
            Add Product
          </Button>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

        <Card>
          <CardContent>
            <Table>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                  <TableCell><strong>Name</strong></TableCell>
                  <TableCell><strong>SKU</strong></TableCell>
                  <TableCell><strong>Category</strong></TableCell>
                  <TableCell><strong>Price</strong></TableCell>
                  <TableCell><strong>Stock</strong></TableCell>
                  <TableCell align="center"><strong>Actions</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {products.map((product) => (
                  <TableRow key={product.id}>
                    <TableCell>{product.name}</TableCell>
                    <TableCell>{product.sku}</TableCell>
                    <TableCell>{product.category}</TableCell>
                    <TableCell>${product.price.toFixed(2)}</TableCell>
                    <TableCell>{product.stock}</TableCell>
                    <TableCell align="center">
                      <Button
                        size="small"
                        color="primary"
                        startIcon={<EditIcon />}
                        onClick={() => handleOpenDialog(product)}
                        sx={{ mr: 1 }}
                      >
                        Edit
                      </Button>
                      <Button
                        size="small"
                        color="error"
                        startIcon={<DeleteIcon />}
                        onClick={() => handleDeleteProduct(product.id)}
                      >
                        Delete
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
            {products.length === 0 && (
              <Typography sx={{ textAlign: 'center', py: 2, color: 'textSecondary' }}>
                No products found
              </Typography>
            )}
          </CardContent>
        </Card>
      </Container>

      {/* Add/Edit Product Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{editingId ? 'Edit Product' : 'Create New Product'}</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <TextField
            autoFocus
            fullWidth
            label="Product Name"
            name="name"
            value={formData.name}
            onChange={handleInputChange}
            margin="normal"
            required
          />
          <TextField
            fullWidth
            label="SKU"
            name="sku"
            value={formData.sku}
            onChange={handleInputChange}
            margin="normal"
            required
          />
          <TextField
            fullWidth
            label="Category"
            name="category"
            value={formData.category}
            onChange={handleInputChange}
            margin="normal"
            required
          />
          <TextField
            fullWidth
            label="Price"
            name="price"
            type="number"
            value={formData.price}
            onChange={handleInputChange}
            margin="normal"
            inputProps={{ step: "0.01" }}
          />
          <TextField
            fullWidth
            label="Stock"
            name="stock"
            type="number"
            value={formData.stock}
            onChange={handleInputChange}
            margin="normal"
            inputProps={{ step: "1" }}
          />
          <TextField
            fullWidth
            label="Status"
            name="status"
            type="number"
            value={formData.status}
            onChange={handleInputChange}
            margin="normal"
            inputProps={{ step: "1" }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSaveProduct} variant="contained" color="primary">
            {editingId ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default ProductsPage;
