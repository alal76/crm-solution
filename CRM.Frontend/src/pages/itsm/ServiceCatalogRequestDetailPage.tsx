import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Grid from '@mui/material/Grid';
import CircularProgress from '@mui/material/CircularProgress';
import apiClient from '../../services/apiClient';

interface CatalogRequestDetail {
  requestId: number;
  catalogItemId: number;
  requestedForId: number;
  requestedById: number;
  state: number;
  approvalStatus: number;
  createdAt: string;
}

interface CatalogItemLookup {
  catalogItemId: number;
  name: string;
}

const ServiceCatalogRequestDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [request, setRequest] = useState<CatalogRequestDetail | null>(null);
  const [catalogItems, setCatalogItems] = useState<CatalogItemLookup[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const [requestResponse, catalogResponse] = await Promise.all([
          apiClient.get('/catalog/requests'),
          apiClient.get('/catalog/items')
        ]);
        const items: CatalogRequestDetail[] = requestResponse.data ?? [];
        const found = items.find((item) => item.requestId === Number(id));
        setRequest(found ?? null);
        setCatalogItems(catalogResponse.data ?? []);
      } catch (error) {
        console.error('Failed to load catalog request', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [id]);

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" fontWeight="bold" sx={{ mb: 3 }}>Catalog Request</Typography>
      <Paper sx={{ p: 3 }}>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>
        ) : !request ? (
          <Typography color="text.secondary">Request not found.</Typography>
        ) : (
          <Box>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>Request ID: {request.requestId}</Typography>
            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <Typography variant="body2" fontWeight="bold" color="text.secondary">Catalog Item</Typography>
                <Typography>
                  {catalogItems.find((item) => item.catalogItemId === request.catalogItemId)?.name ?? `Item ${request.catalogItemId}`}
                </Typography>
              </Grid>
              <Grid item xs={12} md={6}>
                <Typography variant="body2" fontWeight="bold" color="text.secondary">State</Typography>
                <Typography>State {request.state}</Typography>
              </Grid>
              <Grid item xs={12} md={6}>
                <Typography variant="body2" fontWeight="bold" color="text.secondary">Approval Status</Typography>
                <Typography>Status {request.approvalStatus}</Typography>
              </Grid>
              <Grid item xs={12} md={6}>
                <Typography variant="body2" fontWeight="bold" color="text.secondary">Requested For</Typography>
                <Typography>User {request.requestedForId}</Typography>
              </Grid>
              <Grid item xs={12} md={6}>
                <Typography variant="body2" fontWeight="bold" color="text.secondary">Requested By</Typography>
                <Typography>User {request.requestedById}</Typography>
              </Grid>
              <Grid item xs={12} md={6}>
                <Typography variant="body2" fontWeight="bold" color="text.secondary">Created At</Typography>
                <Typography>{new Date(request.createdAt).toLocaleString()}</Typography>
              </Grid>
            </Grid>
          </Box>
        )}
      </Paper>
    </Box>
  );
};

export default ServiceCatalogRequestDetailPage;
