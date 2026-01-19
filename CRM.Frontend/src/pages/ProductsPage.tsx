import { useState } from 'react';
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
  TextField,
  Alert,
  IconButton,
  Chip,
} from '@mui/material';
import {
  Edit as EditIcon,
  Delete as DeleteIcon,
  Add as AddIcon,
} from '@mui/icons-material';

interface Product {
  id: string;
  name: string;
  sku: string;
  price: number;
  category: string;
  stock: number;
  status: 'active' | 'inactive';
}

function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([
    {
      id: '1',
      name: 'Premium Software License',
      sku: 'PSL-001',
      price: 99.99,
      category: 'Software',
      stock: 50,
      status: 'active',
    },
    {
      id: '2',
      name: 'Training Package',
      sku: 'TRN-001',
      price: 299.99,
      category: 'Services',
      stock: 100,
      status: 'active',
    },
  ]);

  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    sku: '',
    price: '',
    category: '',
    stock: '',
    status: 'active',
  });
  const [error, setError] = useState('');

  const handleOpenDialog = (product?: Product) => {
    if (product) {
      setEditingId(product.id);
      setFormData({
        name: product.name,
        sku: product.sku,
        price: product.price.toString(),
        category: product.category,
        stock: product.stock.toString(),
        status: product.status,
      });
    } else {
      setEditingId(null);
      setFormData({ name: '', sku: '', price: '', category: '', stock: '', status: 'active' });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setError('');
  };

  const handleSave = () => {
    if (!formData.name || !formData.sku || !formData.price || !formData.category) {
      setError('Please fill in all required fields');
      return;
    }

    if (editingId) {
      setProducts(
        products.map((p) =>
          p.id === editingId
            ? {
                ...p,
                name: formData.name,
                sku: formData.sku,
                price: parseFloat(formData.price),
                category: formData.category,
                stock: parseInt(formData.stock),
                status: formData.status as 'active' | 'inactive',
              }
            : p
        )
      );
    } else {
      const newProduct: Product = {
        id: Date.now().toString(),
        name: formData.name,
        sku: formData.sku,
        price: parseFloat(formData.price),
        category: formData.category,
        stock: parseInt(formData.stock),
        status: formData.status as 'active' | 'inactive',
      };
      setProducts([...products, newProduct]);
    }
    handleCloseDialog();
  };

  const handleDelete = (id: string) => {
    if (window.confirm('Are you sure you want to delete this product?')) {
      setProducts(products.filter((p) => p.id !== id));
    }
  };

  return (
    <Box sx={{ py: 2 }}>
      <Box sx={{ mb: 4, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box>
          <Typography variant="h3" sx={{ fontWeight: 700, mb: 0.5 }}>
            Products
          </Typography>
          <Typography color="textSecondary" variant="body2">
            Manage your product catalog and inventory
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenDialog()}
          sx={{ backgroundColor: '#6750A4', textTransform: 'none', borderRadius: 2 }}
        >
          Add Product
        </Button>
      </Box>

      <Card sx={{ borderRadius: 3, boxShadow: 1 }}>
        <CardContent sx={{ p: 0 }}>
          <Table>
            <TableHead>
              <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Name</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>SKU</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }}>Category</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }} align="right">
                  Price
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }} align="center">
                  Stock
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }} align="center">
                  Status
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#6750A4' }} align="center">
                  Actions
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {products.map((product) => (
                <TableRow
                  key={product.id}
                  sx={{
                    '&:hover': { backgroundColor: '#F5EFF7' },
                    borderBottom: '1px solid #E8DEF8',
                  }}
                >
                  <TableCell sx={{ fontWeight: 500 }}>{product.name}</TableCell>
                  <TableCell>{product.sku}</TableCell>
                  <TableCell>{product.category}</TableCell>
                  <TableCell align="right" sx={{ fontWeight: 600, color: '#6750A4' }}>
                    ${product.price.toFixed(2)}
                  </TableCell>
                  <TableCell align="center">{product.stock}</TableCell>
                  <TableCell align="center">
                    <Chip
                      label={product.status.charAt(0).toUpperCase() + product.status.slice(1)}
                      size="small"
                      sx={{
                        backgroundColor: product.status === 'active' ? '#E8F5E9' : '#FFEBEE',
                        color: product.status === 'active' ? '#06A77D' : '#B3261E',
                        fontWeight: 600,
                      }}
                    />
                  </TableCell>
                  <TableCell align="center">
                    <IconButton
                      size="small"
                      onClick={() => handleOpenDialog(product)}
                      sx={{ color: '#6750A4' }}
                    >
                      <EditIcon fontSize="small" />
                    </IconButton>
                    <IconButton
                      size="small"
                      onClick={() => handleDelete(product.id)}
                      sx={{ color: '#B3261E' }}
                    >
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 600, color: '#6750A4' }}>
          {editingId ? 'Edit Product' : 'Add Product'}
        </DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}
          <TextField
            fullWidth
            label="Product Name"
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            margin="normal"
            variant="outlined"
            required
          />
          <TextField
            fullWidth
            label="SKU"
            value={formData.sku}
            onChange={(e) => setFormData({ ...formData, sku: e.target.value })}
            margin="normal"
            variant="outlined"
            required
          />
          <TextField
            fullWidth
            label="Price"
            type="number"
            value={formData.price}
            onChange={(e) => setFormData({ ...formData, price: e.target.value })}
            margin="normal"
            variant="outlined"
            required
            inputProps={{ step: '0.01' }}
          />
          <TextField
            fullWidth
            label="Category"
            value={formData.category}
            onChange={(e) => setFormData({ ...formData, category: e.target.value })}
            margin="normal"
            variant="outlined"
            required
          />
          <TextField
            fullWidth
            label="Stock Quantity"
            type="number"
            value={formData.stock}
            onChange={(e) => setFormData({ ...formData, stock: e.target.value })}
            margin="normal"
            variant="outlined"
          />
          <TextField
            fullWidth
            label="Status"
            select
            value={formData.status}
            onChange={(e) => setFormData({ ...formData, status: e.target.value })}
            margin="normal"
            variant="outlined"
            SelectProps={{
              native: true,
            }}
          >
            <option value="active">Active</option>
            <option value="inactive">Inactive</option>
          </TextField>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button
            onClick={handleSave}
            variant="contained"
            sx={{ backgroundColor: '#6750A4', textTransform: 'none' }}
          >
            {editingId ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default ProductsPage;
