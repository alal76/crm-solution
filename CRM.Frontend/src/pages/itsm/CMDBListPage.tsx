import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box, Typography, Button, TextField, CircularProgress,
  TableContainer, Table, TableHead, TableBody, TableRow, TableCell, Paper, TablePagination
} from '@mui/material';
import apiClient from '../../services/apiClient';
import { usePagination } from '../../hooks/usePagination';

interface ConfigurationItem {
  ciId: number;
  ciName: string;
  ciNumber: string;
  ciType: number;
  operationalStatus: number;
}

const CMDBListPage: React.FC = () => {
  const navigate = useNavigate();
  const [items, setItems] = useState<ConfigurationItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      try {
        const params = new URLSearchParams({
          searchTerm,
          pageNumber: '1',
          pageSize: '20'
        });
        const response = await apiClient.get(`/cmdb?${params}`);
        setItems(response.data ?? []);
      } catch (error) {
        console.error('Failed to load configuration items', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [searchTerm]);

  const { paginatedData: paginatedItems, page, pageSize, handlePageChange, handlePageSizeChange, pageSizeOptions } =
    usePagination(items, { defaultPageSize: 25 });

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1" fontWeight="bold">CMDB</Typography>
        <Button variant="contained" onClick={() => navigate('/itsm/cmdb/create')}>
          + New CI
        </Button>
      </Box>

      <TextField
        fullWidth
        placeholder="Search configuration items..."
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        sx={{ mb: 3 }}
      />

      {loading ? (
        <CircularProgress />
      ) : items.length === 0 ? (
        <Typography color="text.secondary">No configuration items found.</Typography>
      ) : (
        <>
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Number</TableCell>
                <TableCell>Type</TableCell>
                <TableCell>Status</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {paginatedItems.map((ci) => (
                <TableRow
                  key={ci.ciId}
                  hover
                  sx={{ cursor: 'pointer' }}
                  onClick={() => navigate(`/itsm/cmdb/${ci.ciId}`)}
                >
                  <TableCell sx={{ color: 'primary.main', fontWeight: 500 }}>{ci.ciName}</TableCell>
                  <TableCell>{ci.ciNumber}</TableCell>
                  <TableCell>Type {ci.ciType}</TableCell>
                  <TableCell>Status {ci.operationalStatus}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
        <TablePagination
          component="div"
          count={items.length}
          page={page}
          onPageChange={handlePageChange}
          rowsPerPage={pageSize}
          onRowsPerPageChange={handlePageSizeChange}
          rowsPerPageOptions={pageSizeOptions}
        />
        </>
      )}
    </Box>
  );
};

export default CMDBListPage;
