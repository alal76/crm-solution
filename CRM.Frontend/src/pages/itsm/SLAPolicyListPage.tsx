import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Paper from '@mui/material/Paper';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import apiClient from '../../services/apiClient';

interface SLAPolicy {
  slaPolicyId: number;
  name: string;
  targetType: number;
  p1ResponseMinutes?: number;
  p1ResolutionMinutes?: number;
  isActive: boolean;
}

const SLAPolicyListPage: React.FC = () => {
  const navigate = useNavigate();
  const [items, setItems] = useState<SLAPolicy[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await apiClient.get('/sla/policies');
        setItems(response.data ?? []);
      } catch (error) {
        console.error('Failed to load SLA policies', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1" fontWeight="bold">SLA Policies</Typography>
        <Button variant="contained" onClick={() => navigate('/itsm/sla/policies/create')}>+ New Policy</Button>
      </Box>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell><strong>Name</strong></TableCell>
                <TableCell><strong>Target</strong></TableCell>
                <TableCell><strong>Response (P1)</strong></TableCell>
                <TableCell><strong>Resolution (P1)</strong></TableCell>
                <TableCell><strong>Active</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {items.map((policy) => (
                <TableRow key={policy.slaPolicyId} hover>
                  <TableCell sx={{ color: 'primary.main', fontWeight: 500 }}>{policy.name}</TableCell>
                  <TableCell>Type {policy.targetType}</TableCell>
                  <TableCell>{policy.p1ResponseMinutes ?? '—'} min</TableCell>
                  <TableCell>{policy.p1ResolutionMinutes ?? '—'} min</TableCell>
                  <TableCell>{policy?.isActive !== false ? 'Yes' : 'No'}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  );
};

export default SLAPolicyListPage;
