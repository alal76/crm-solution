import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import CircularProgress from '@mui/material/CircularProgress';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import apiClient from '../../services/apiClient';

interface CatalogRequestItem {
  requestId: number;
  catalogItemId: number;
  state: number;
  createdAt: string;
}

interface CatalogItemLookup {
  catalogItemId: number;
  name: string;
}

const ServiceCatalogRequestListPage: React.FC = () => {
  const navigate = useNavigate();
  const [items, setItems] = useState<CatalogRequestItem[]>([]);
  const [catalogItems, setCatalogItems] = useState<CatalogItemLookup[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const [requestResponse, catalogResponse] = await Promise.all([
          apiClient.get('/catalog/requests'),
          apiClient.get('/catalog/items')
        ]);
        setItems(requestResponse.data ?? []);
        setCatalogItems(catalogResponse.data ?? []);
      } catch (error) {
        console.error('Failed to load catalog requests', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" fontWeight="bold" sx={{ mb: 3 }}>Catalog Requests</Typography>
      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Request</TableCell>
                <TableCell>Catalog Item</TableCell>
                <TableCell>State</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {items.map((request) => (
                <TableRow
                  key={request.requestId}
                  hover
                  sx={{ cursor: 'pointer' }}
                  onClick={() => navigate(`/itsm/catalog/requests/${request.requestId}`)}
                >
                  <TableCell sx={{ color: 'primary.main', fontWeight: 'medium' }}>REQ-{request.requestId}</TableCell>
                  <TableCell>
                    {catalogItems.find((item) => item.catalogItemId === request.catalogItemId)?.name ?? `Item ${request.catalogItemId}`}
                  </TableCell>
                  <TableCell>State {request.state}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  );
};

export default ServiceCatalogRequestListPage;
