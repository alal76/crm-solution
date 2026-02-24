import React, { useEffect, useState } from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import CircularProgress from '@mui/material/CircularProgress';
import Paper from '@mui/material/Paper';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import apiClient from '../../services/apiClient';

interface SLAInstance {
  slaInstanceId: number;
  targetId: number;
  targetType: number;
  responseDueAt?: string;
  resolutionDueAt?: string;
  responseBreached: boolean;
  resolutionBreached: boolean;
  state: number;
}

const SLAInstanceListPage: React.FC = () => {
  const [items, setItems] = useState<SLAInstance[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await apiClient.get('/sla/breached');
        setItems(response.data ?? []);
      } catch (error) {
        console.error('Failed to load SLA instances', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" fontWeight="bold" sx={{ mb: 3 }}>SLA Instances</Typography>
      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell><strong>Target</strong></TableCell>
                <TableCell><strong>Response Due</strong></TableCell>
                <TableCell><strong>Resolution Due</strong></TableCell>
                <TableCell><strong>State</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {items.map((sla) => (
                <TableRow key={sla.slaInstanceId} hover>
                  <TableCell>{sla.targetType} / {sla.targetId}</TableCell>
                  <TableCell>{sla.responseDueAt ? new Date(sla.responseDueAt).toLocaleString() : '—'}</TableCell>
                  <TableCell>{sla.resolutionDueAt ? new Date(sla.resolutionDueAt).toLocaleString() : '—'}</TableCell>
                  <TableCell>State {sla.state}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  );
};

export default SLAInstanceListPage;
