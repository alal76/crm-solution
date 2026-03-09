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
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import partnerPortalService, { type PartnerDealDto } from '../../services/partnerPortalService';

const stageColor = (stage: string): 'default' | 'info' | 'warning' | 'success' | 'error' => {
  const s = stage.toLowerCase();
  if (s.includes('closedwon') || s.includes('closed_won')) return 'success';
  if (s.includes('closedlost') || s.includes('closed_lost')) return 'error';
  if (s.includes('negotiation') || s.includes('proposal')) return 'warning';
  return 'info';
};

const PartnerDealsPage: React.FC = () => {
  const [deals, setDeals] = useState<PartnerDealDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      // Pass 0 to use current-user context (server filters by userId)
      const data = await partnerPortalService.getDeals(0);
      setDeals(data);
    } catch {
      setError('Failed to load deals. Please try again.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const totalPipeline = deals.reduce((sum, d) => sum + d.amount, 0);

  return (
    <Box p={3}>
      <Stack direction="row" alignItems="center" spacing={1} mb={3}>
        <TrendingUpIcon color="primary" />
        <Box>
          <Typography variant="h5" fontWeight="bold">My Deals</Typography>
          {!loading && (
            <Typography variant="body2" color="text.secondary">
              {deals.length} deal{deals.length !== 1 ? 's' : ''} — Pipeline:{' '}
              ${totalPipeline.toLocaleString('en-US', { minimumFractionDigits: 0 })}
            </Typography>
          )}
        </Box>
      </Stack>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {loading ? (
        <Box display="flex" justifyContent="center" mt={6}>
          <CircularProgress />
        </Box>
      ) : (
        <Card variant="outlined">
          <CardContent sx={{ p: 0 }}>
            <Table>
              <TableHead>
                <TableRow sx={{ bgcolor: 'grey.50' }}>
                  <TableCell><strong>Deal Name</strong></TableCell>
                  <TableCell><strong>Stage</strong></TableCell>
                  <TableCell align="right"><strong>Amount</strong></TableCell>
                  <TableCell><strong>Expected Close</strong></TableCell>
                  <TableCell><strong>Created</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {deals.map(deal => (
                  <TableRow key={deal.id} hover>
                    <TableCell>
                      <Typography variant="body2" fontWeight="medium">{deal.name}</Typography>
                    </TableCell>
                    <TableCell>
                      <Chip label={deal.stage} size="small" color={stageColor(deal.stage)} />
                    </TableCell>
                    <TableCell align="right">
                      <Typography variant="body2">
                        {deal.currency}{' '}
                        {deal.amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" color="text.secondary">
                        {deal.expectedCloseDate
                          ? new Date(deal.expectedCloseDate).toLocaleDateString()
                          : '—'}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" color="text.secondary">
                        {new Date(deal.createdAt).toLocaleDateString()}
                      </Typography>
                    </TableCell>
                  </TableRow>
                ))}
                {deals.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={5} align="center">
                      <Typography color="text.secondary" variant="body2" py={3}>
                        No deals found.
                      </Typography>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}
    </Box>
  );
};

export default PartnerDealsPage;
