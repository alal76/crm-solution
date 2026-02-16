import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Button from '@mui/material/Button';
import TextField from '@mui/material/TextField';
import apiClient from '../../services/apiClient';
import { CatalogRequestForm } from '../../components/itsm';
import type { CatalogItemDetails } from '../../components/itsm';

const ServiceCatalogRequestCreatePage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState(false);
  const [requestedForId, setRequestedForId] = useState('');
  const [catalogItem, setCatalogItem] = useState<CatalogItemDetails | null>(null);

  useEffect(() => {
    const loadCatalogItem = async () => {
      try {
        const response = await apiClient.get(`/catalog/items/${id}`);
        setCatalogItem(response.data);
      } catch (error) {
        console.error('Failed to load catalog item', error);
      }
    };
    loadCatalogItem();
  }, [id]);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setSubmitting(true);

    try {
      await apiClient.post('/api/catalog/requests', {
        catalogItemId: Number(id),
        requestedForId: Number(requestedForId)
      });
      navigate('/itsm/catalog/requests');
    } catch (error) {
      console.error('Failed to submit request', error);
      setSubmitting(false);
    }
  };

  return (
    <Box sx={{ p: 3, maxWidth: 700, mx: 'auto' }}>
      <Typography variant="h4" component="h1" fontWeight="bold" sx={{ mb: 3 }}>Request Service</Typography>
      <Paper component="form" onSubmit={handleSubmit} sx={{ p: 3, display: 'flex', flexDirection: 'column', gap: 2 }}>
        <TextField
          label="Catalog Item ID"
          value={id}
          InputProps={{ readOnly: true }}
          fullWidth
        />
        <TextField
          label="Requested For (User ID)"
          type="number"
          value={requestedForId}
          onChange={(e) => setRequestedForId(e.target.value)}
          required
          fullWidth
        />
        <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1.5 }}>
          <Button variant="outlined" onClick={() => navigate('/itsm/catalog')}>Cancel</Button>
          <Button variant="contained" type="submit" disabled={submitting}>
            {submitting ? 'Submitting...' : 'Submit Request'}
          </Button>
        </Box>
      </Paper>

      {/* Enhanced Catalog Request Form */}
      {catalogItem && (
        <Box sx={{ mt: 3 }}>
          <CatalogRequestForm
            catalogItem={catalogItem}
            onSubmit={async (data) => {
              setSubmitting(true);
              try {
                await apiClient.post('/api/catalog/requests', {
                  catalogItemId: Number(id),
                  ...data,
                });
                navigate('/itsm/catalog/requests');
              } catch (error) {
                console.error('Failed to submit request', error);
              } finally {
                setSubmitting(false);
              }
            }}
            onCancel={() => navigate('/itsm/catalog')}
          />
        </Box>
      )}
    </Box>
  );
};

export default ServiceCatalogRequestCreatePage;
