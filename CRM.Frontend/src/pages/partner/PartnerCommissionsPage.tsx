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
import AttachMoneyIcon from '@mui/icons-material/AttachMoney';
import partnerPortalService, { type PartnerCommissionDto } from '../../services/partnerPortalService';

const statusColor = (status: string): 'default' | 'info' | 'warning' | 'success' | 'error' => {
  const s = status.toLowerCase();
  if (s === 'paid') return 'success';
  if (s === 'approved') return 'info';
  if (s === 'rejected' || s === 'clawedback') return 'error';
  return 'warning'; // Pending
};

const PartnerCommissionsPage: React.FC = () => {
  const [commissions, setCommissions] = useState<PartnerCommissionDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await partnerPortalService.getCommissions();
      setCommissions(data);
    } catch {
      setError('Failed to load commissions. Please try again.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const totalPaid = commissions
    .filter(c => c.status.toLowerCase() === 'paid')
    .reduce((sum, c) => sum + c.finalCommissionAmount, 0);

  const totalPending = commissions
    .filter(c => !['paid', 'rejected', 'clawedback'].includes(c.status.toLowerCase()))
    .reduce((sum, c) => sum + c.finalCommissionAmount, 0);

  return (
    <Box p={3}>
      <Stack direction="row" alignItems="center" spacing={1} mb={3}>
        <AttachMoneyIcon color="primary" />
        <Box>
          <Typography variant="h5" fontWeight="bold">My Commissions</Typography>
          {!loading && (
            <Typography variant="body2" color="text.secondary">
              Paid: ${totalPaid.toLocaleString('en-US', { minimumFractionDigits: 2 })} |{' '}
              Pending: ${totalPending.toLocaleString('en-US', { minimumFractionDigits: 2 })}
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
                  <TableCell><strong>Commission #</strong></TableCell>
                  <TableCell><strong>Period</strong></TableCell>
                  <TableCell align="right"><strong>Amount</strong></TableCell>
                  <TableCell align="right"><strong>Final Amount</strong></TableCell>
                  <TableCell><strong>Status</strong></TableCell>
                  <TableCell><strong>Earned</strong></TableCell>
                  <TableCell><strong>Paid</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {commissions.map(commission => (
                  <TableRow key={commission.id} hover>
                    <TableCell>
                      <Typography variant="body2" fontWeight="medium">
                        {commission.commissionNumber}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">{commission.commissionPeriod}</Typography>
                    </TableCell>
                    <TableCell align="right">
                      <Typography variant="body2">
                        {commission.currency}{' '}
                        {commission.commissionAmount.toLocaleString('en-US', {
                          minimumFractionDigits: 2,
                          maximumFractionDigits: 2,
                        })}
                      </Typography>
                    </TableCell>
                    <TableCell align="right">
                      <Typography variant="body2" fontWeight="medium">
                        {commission.currency}{' '}
                        {commission.finalCommissionAmount.toLocaleString('en-US', {
                          minimumFractionDigits: 2,
                          maximumFractionDigits: 2,
                        })}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Chip label={commission.status} size="small" color={statusColor(commission.status)} />
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" color="text.secondary">
                        {new Date(commission.earnedDate).toLocaleDateString()}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" color="text.secondary">
                        {commission.paidDate
                          ? new Date(commission.paidDate).toLocaleDateString()
                          : '—'}
                      </Typography>
                    </TableCell>
                  </TableRow>
                ))}
                {commissions.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={7} align="center">
                      <Typography color="text.secondary" variant="body2" py={3}>
                        No commission records found.
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

export default PartnerCommissionsPage;
