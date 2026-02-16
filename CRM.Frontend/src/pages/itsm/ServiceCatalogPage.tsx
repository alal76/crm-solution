import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Button from '@mui/material/Button';
import Grid from '@mui/material/Grid';
import TextField from '@mui/material/TextField';
import CircularProgress from '@mui/material/CircularProgress';
import Chip from '@mui/material/Chip';
import apiClient from '../../services/apiClient';
import { CatalogCategoryBrowser } from '../../components/itsm';
import type { CatalogCategory } from '../../components/itsm';

interface CatalogItem {
  catalogItemId: number;
  name: string;
  shortDescription: string;
  categoryName: string;
  price?: number;
  isFeatured: boolean;
}

export const ServiceCatalogPage: React.FC = () => {
  const navigate = useNavigate();
  const [items, setItems] = useState<CatalogItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [categories, setCategories] = useState<CatalogCategory[]>([]);
  const [selectedCategoryId, setSelectedCategoryId] = useState<number | null>(null);

  useEffect(() => {
    const loadItems = async () => {
      setLoading(true);
      try {
        const params = searchTerm ? `?searchTerm=${searchTerm}` : '';
        const response = await apiClient.get(`/catalog/search${params}`);
        setItems(response.data ?? []);
      } catch (error) {
        console.error('Failed to load catalog', error);
      } finally {
        setLoading(false);
      }
    };

    loadItems();
  }, [searchTerm]);

  useEffect(() => {
    const loadCategories = async () => {
      try {
        const response = await apiClient.get('/api/catalog/categories');
        setCategories(response.data ?? []);
      } catch (error) {
        console.error('Failed to load categories', error);
      }
    };
    loadCategories();
  }, []);

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" fontWeight="bold" sx={{ mb: 3 }}>Service Catalog</Typography>

      {/* Category Browser */}
      {categories.length > 0 && (
        <Box sx={{ mb: 3 }}>
          <CatalogCategoryBrowser
            categories={categories}
            selectedCategoryId={selectedCategoryId ?? undefined}
            onCategorySelect={(catId) => setSelectedCategoryId(catId)}
            onItemSelect={(itemId) => navigate(`/catalog/${itemId}`)}
            variant="grid"
            showSearch
          />
        </Box>
      )}

      <Box sx={{ mb: 4 }}>
        <TextField
          placeholder="Search services..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          size="small"
          sx={{ maxWidth: 400, width: '100%' }}
        />
      </Box>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>
      ) : items.length === 0 ? (
        <Typography color="text.secondary">No catalog items found.</Typography>
      ) : (
        <Grid container spacing={3}>
          {items.map((item) => (
            <Grid item xs={12} md={6} lg={4} key={item.catalogItemId}>
              <Paper
                sx={{ p: 3, cursor: 'pointer', '&:hover': { boxShadow: 4 }, transition: 'box-shadow 0.2s' }}
                onClick={() => navigate(`/catalog/${item.catalogItemId}`)}
              >
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1.5 }}>
                  <Typography variant="subtitle1" fontWeight="bold">{item.name}</Typography>
                  {item.isFeatured && <Chip label="Featured" color="warning" size="small" />}
                </Box>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 1.5 }}>{item.shortDescription}</Typography>
                <Typography variant="caption" color="text.secondary" sx={{ mb: 2, display: 'block' }}>{item.categoryName}</Typography>
                {item.price && <Typography variant="h6" fontWeight="bold" color="success.main">${item.price}</Typography>}
                <Button
                  variant="contained"
                  fullWidth
                  sx={{ mt: 2 }}
                  onClick={(e) => {
                    e.stopPropagation();
                    navigate(`/catalog/${item.catalogItemId}/request`);
                  }}
                >
                  Request Service
                </Button>
              </Paper>
            </Grid>
          ))}
        </Grid>
      )}
    </Box>
  );
};

export default ServiceCatalogPage;
