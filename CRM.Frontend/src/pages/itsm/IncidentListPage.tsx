import React, { useState, useEffect } from 'react';
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
  CircularProgress,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import apiClient from '../../services/apiClient';

interface Incident {
  incidentId: number;
  number: string;
  shortDescription: string;
  state: number;
  priority: number;
  callerName: string;
  createdAt: string;
}

export const IncidentListPage: React.FC = () => {
  const navigate = useNavigate();
  const [incidents, setIncidents] = useState<Incident[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [pageNumber, setPageNumber] = useState(1);

  useEffect(() => {
    const loadIncidents = async () => {
      setLoading(true);
      try {
        const params = new URLSearchParams({
          searchTerm: searchTerm,
          pageNumber: pageNumber.toString(),
          pageSize: '20'
        });
        const response = await apiClient.get(`/incidents?${params}`);
        setIncidents(response.data.items ?? []);
      } catch (error) {
        console.error('Failed to load incidents', error);
      } finally {
        setLoading(false);
      }
    };

    loadIncidents();
  }, [searchTerm, pageNumber]);

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1" fontWeight="bold">
          Incidents
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => navigate('/incidents/create')}
        >
          New Incident
        </Button>
      </Box>

      <TextField
        fullWidth
        placeholder="Search incidents..."
        value={searchTerm}
        onChange={(e) => { setSearchTerm(e.target.value); setPageNumber(1); }}
        sx={{ mb: 3 }}
        size="small"
      />

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : incidents.length === 0 ? (
        <Typography color="text.secondary">No incidents found.</Typography>
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Number</TableCell>
                <TableCell>Description</TableCell>
                <TableCell>Caller</TableCell>
                <TableCell>Priority</TableCell>
                <TableCell>State</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {incidents.map((incident) => (
                <TableRow
                  key={incident.incidentId}
                  hover
                  onClick={() => navigate(`/incidents/${incident.incidentId}`)}
                  sx={{ cursor: 'pointer' }}
                >
                  <TableCell sx={{ color: 'primary.main', fontWeight: 500 }}>{incident.number}</TableCell>
                  <TableCell>{incident.shortDescription}</TableCell>
                  <TableCell>{incident.callerName}</TableCell>
                  <TableCell>P{incident.priority}</TableCell>
                  <TableCell>State {incident.state}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  );
};

export default IncidentListPage;
