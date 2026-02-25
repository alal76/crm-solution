import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import TextField from '@mui/material/TextField';
import CircularProgress from '@mui/material/CircularProgress';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import TablePagination from '@mui/material/TablePagination';
import apiClient from '../../services/apiClient';
import { usePagination } from '../../hooks/usePagination';

interface ChangeItem {
  changeId: number;
  number: string;
  shortDescription: string;
  state: number;
  approvalStatus: number;
  plannedStartDate?: string;
}

const ChangeListPage: React.FC = () => {
  const navigate = useNavigate();
  const [items, setItems] = useState<ChangeItem[]>([]);
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
        const response = await apiClient.get(`/changes?${params}`);
        setItems(response.data.items ?? response.data ?? []);
      } catch (error) {
        console.error('Failed to load changes', error);
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
        <Typography variant="h4" component="h1" fontWeight="bold">Changes</Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button variant="outlined" onClick={() => navigate('/itsm/changes/calendar')}>
            Calendar
          </Button>
          <Button variant="contained" onClick={() => navigate('/itsm/changes/create')}>
            + New Change
          </Button>
        </Box>
      </Box>

      <Box sx={{ mb: 3 }}>
        <TextField
          fullWidth
          placeholder="Search changes..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          size="small"
        />
      </Box>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : items.length === 0 ? (
        <Typography color="text.secondary">No changes found.</Typography>
      ) : (
        <>
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell><strong>Number</strong></TableCell>
                <TableCell><strong>Description</strong></TableCell>
                <TableCell><strong>State</strong></TableCell>
                <TableCell><strong>Approval</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {paginatedItems.map((change) => (
                <TableRow
                  key={change.changeId}
                  hover
                  sx={{ cursor: 'pointer' }}
                  onClick={() => navigate(`/itsm/changes/${change.changeId}`)}
                >
                  <TableCell sx={{ color: 'primary.main', fontWeight: 500 }}>{change.number}</TableCell>
                  <TableCell>{change.shortDescription}</TableCell>
                  <TableCell>State {change.state}</TableCell>
                  <TableCell>Status {change.approvalStatus}</TableCell>
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

export default ChangeListPage;
