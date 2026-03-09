// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
import React, { useEffect, useState, useCallback } from 'react';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material';
import PeopleIcon from '@mui/icons-material/People';
import partnerPortalService, { type PartnerLeadDto } from '../../services/partnerPortalService';

const statusColor = (status: string): 'default' | 'info' | 'warning' | 'success' | 'error' => {
  const s = status.toLowerCase();
  if (s === 'qualified') return 'success';
  if (s === 'disqualified') return 'error';
  if (s === 'contacted') return 'warning';
  return 'info';
};

const PAGE_SIZE = 20;

const PartnerLeadsPage: React.FC = () => {
  const [leads, setLeads] = useState<PartnerLeadDto[]>([]);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [hasMore, setHasMore] = useState(true);

  const load = useCallback(async (p: number) => {
    setLoading(true);
    setError(null);
    try {
      const data = await partnerPortalService.getLeads(p, PAGE_SIZE);
      if (p === 1) {
        setLeads(data);
      } else {
        setLeads(prev => [...prev, ...data]);
      }
      setHasMore(data.length === PAGE_SIZE);
    } catch {
      setError('Failed to load leads. Please try again.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(1); }, [load]);

  const handleLoadMore = () => {
    const next = page + 1;
    setPage(next);
    load(next);
  };

  return (
    <Box p={3}>
      <Stack direction="row" alignItems="center" spacing={1} mb={3}>
        <PeopleIcon color="primary" />
        <Typography variant="h5" fontWeight="bold">My Leads</Typography>
      </Stack>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <Card variant="outlined">
        <CardContent sx={{ p: 0 }}>
          <Table>
            <TableHead>
              <TableRow sx={{ bgcolor: 'grey.50' }}>
                <TableCell><strong>Name</strong></TableCell>
                <TableCell><strong>Email</strong></TableCell>
                <TableCell><strong>Company</strong></TableCell>
                <TableCell><strong>Status</strong></TableCell>
                <TableCell><strong>Created</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {leads.map(lead => (
                <TableRow key={lead.id} hover>
                  <TableCell>
                    <Typography variant="body2" fontWeight="medium">
                      {lead.firstName} {lead.lastName}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" color="text.secondary">{lead.email}</Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">{lead.companyName ?? '—'}</Typography>
                  </TableCell>
                  <TableCell>
                    <Chip label={lead.status} size="small" color={statusColor(lead.status)} />
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" color="text.secondary">
                      {new Date(lead.createdAt).toLocaleDateString()}
                    </Typography>
                  </TableCell>
                </TableRow>
              ))}
              {leads.length === 0 && !loading && (
                <TableRow>
                  <TableCell colSpan={5} align="center">
                    <Typography color="text.secondary" variant="body2" py={3}>
                      No leads found.
                    </Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {loading && (
        <Box display="flex" justifyContent="center" mt={3}>
          <CircularProgress size={28} />
        </Box>
      )}

      {!loading && hasMore && (
        <Box display="flex" justifyContent="center" mt={2}>
          <Button variant="outlined" onClick={handleLoadMore}>Load More</Button>
        </Box>
      )}
    </Box>
  );
};

export default PartnerLeadsPage;
