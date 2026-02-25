import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Button,
  TextField,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  CircularProgress,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import apiClient from '../../services/apiClient';
import { usePagination } from '../../hooks/usePagination';

interface Problem {
  problemId: number;
  number: string;
  shortDescription: string;
  state: number;
  priority: number;
  createdAt: string;
}

const ProblemListPage: React.FC = () => {
  const navigate = useNavigate();
  const [items, setItems] = useState<Problem[]>([]);
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
        const response = await apiClient.get(`/problems?${params}`);
        setItems(response.data.items ?? response.data ?? []);
      } catch (error) {
        console.error('Failed to load problems', error);
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
        <Typography variant="h4" component="h1" fontWeight="bold">
          Problems
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => navigate('/itsm/problems/create')}
        >
          New Problem
        </Button>
      </Box>

      <TextField
        fullWidth
        placeholder="Search problems..."
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        sx={{ mb: 3 }}
        size="small"
      />

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : items.length === 0 ? (
        <Typography color="text.secondary">No problems found.</Typography>
      ) : (
        <>
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Number</TableCell>
                <TableCell>Description</TableCell>
                <TableCell>Priority</TableCell>
                <TableCell>State</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {paginatedItems.map((problem) => (
                <TableRow
                  key={problem.problemId}
                  hover
                  onClick={() => navigate(`/itsm/problems/${problem.problemId}`)}
                  sx={{ cursor: 'pointer' }}
                >
                  <TableCell sx={{ color: 'primary.main', fontWeight: 500 }}>{problem.number}</TableCell>
                  <TableCell>{problem.shortDescription}</TableCell>
                  <TableCell>P{problem.priority}</TableCell>
                  <TableCell>State {problem.state}</TableCell>
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

export default ProblemListPage;
