import React, { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Button from '@mui/material/Button';
import Grid from '@mui/material/Grid';
import CircularProgress from '@mui/material/CircularProgress';
import Alert from '@mui/material/Alert';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import apiClient from '../../services/apiClient';

interface CatalogAdminItem {
  catalogItemId: number;
  name: string;
  shortDescription?: string;
  categoryName?: string;
  isFeatured: boolean;
  isActive: boolean;
  price?: number;
  requestCount: number;
}

const ServiceCatalogAdminPage: React.FC = () => {
  const navigate = useNavigate();
  const [items, setItems] = useState<CatalogAdminItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await apiClient.get('/api/catalog/items');
        setItems(response.data ?? []);
      } catch (loadError) {
        console.error('Failed to load catalog items', loadError);
        setError('Unable to load catalog items.');
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  const summary = useMemo(() => {
    const total = items.length;
    const featured = items.filter((item) => item.isFeatured).length;
    const active = items.filter((item) => item?.isActive !== false).length;
    const totalRequests = items.reduce((sum, item) => sum + (item.requestCount ?? 0), 0);
    return { total, featured, active, totalRequests };
  }, [items]);

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1" fontWeight="bold">Catalog Administration</Typography>
        <Button variant="outlined" onClick={() => navigate('/itsm/catalog')}>Back to Catalog</Button>
      </Box>
      <Paper sx={{ p: 3 }}>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>
        ) : (
          <>
            <Grid container spacing={2} sx={{ mb: 3 }}>
              <Grid item xs={12} md={3}>
                <Paper variant="outlined" sx={{ p: 2 }}>
                  <Typography variant="body2" color="text.secondary">Total items</Typography>
                  <Typography variant="h5" fontWeight="bold">{summary.total}</Typography>
                </Paper>
              </Grid>
              <Grid item xs={12} md={3}>
                <Paper variant="outlined" sx={{ p: 2 }}>
                  <Typography variant="body2" color="text.secondary">Active items</Typography>
                  <Typography variant="h5" fontWeight="bold">{summary.active}</Typography>
                </Paper>
              </Grid>
              <Grid item xs={12} md={3}>
                <Paper variant="outlined" sx={{ p: 2 }}>
                  <Typography variant="body2" color="text.secondary">Featured items</Typography>
                  <Typography variant="h5" fontWeight="bold">{summary.featured}</Typography>
                </Paper>
              </Grid>
              <Grid item xs={12} md={3}>
                <Paper variant="outlined" sx={{ p: 2 }}>
                  <Typography variant="body2" color="text.secondary">Total requests</Typography>
                  <Typography variant="h5" fontWeight="bold">{summary.totalRequests}</Typography>
                </Paper>
              </Grid>
            </Grid>

            {items.length === 0 ? (
              <Typography color="text.secondary">No catalog items available.</Typography>
            ) : (
              <TableContainer>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Name</TableCell>
                      <TableCell>Category</TableCell>
                      <TableCell>Status</TableCell>
                      <TableCell>Requests</TableCell>
                      <TableCell>Price</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {items.map((item) => (
                      <TableRow key={item.catalogItemId} hover>
                        <TableCell>
                          <Typography variant="body2" fontWeight="medium">{item.name}</Typography>
                          {item.shortDescription && (
                            <Typography variant="caption" color="text.secondary">{item.shortDescription}</Typography>
                          )}
                        </TableCell>
                        <TableCell>{item.categoryName ?? 'Uncategorized'}</TableCell>
                        <TableCell>
                          {item?.isActive !== false ? 'Active' : 'Inactive'}
                          {item.isFeatured ? ' • Featured' : ''}
                        </TableCell>
                        <TableCell>{item.requestCount}</TableCell>
                        <TableCell>{item.price ? `$${item.price}` : '—'}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
          </>
        )}
        {error && <Alert severity="error" sx={{ mt: 2 }}>{error}</Alert>}
      </Paper>
    </Box>
  );
};

export default ServiceCatalogAdminPage;
