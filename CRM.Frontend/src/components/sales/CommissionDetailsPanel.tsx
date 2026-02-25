/**
 * CommissionDetailsPanel - Displays commission breakdown for a deal,
 * including agent info, period, tier structure, bonuses, payment status,
 * and export capability.
 */

import React from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Typography,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  TableContainer,
  Chip,
  Divider,
  Paper,
} from '@mui/material';
import {
  FileDownload as ExportIcon,
  Person as PersonIcon,
  DateRange as PeriodIcon,
} from '@mui/icons-material';

interface CommissionTier {
  tierName: string;
  from: number;
  to: number;
  rate: number;
  amount: number;
}

interface CommissionBonus {
  name: string;
  amount: number;
}

interface CommissionDetailsPanelProps {
  dealValue: number;
  commissionRate: number;
  commissionAmount: number;
  tiers?: CommissionTier[];
  bonuses?: CommissionBonus[];
  status: string;
  paidDate?: string;
  currency?: string;
  /** Agent / sales rep name */
  agentName?: string;
  /** Commission period label, e.g. "2024-Q1" or month name */
  period?: string;
  /** Called when the user clicks the Export button */
  onExport?: () => void;
}

const formatCurrency = (value: number, currency?: string | null) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: currency || 'USD' }).format(value);

const statusChipColor = (status: string): 'default' | 'warning' | 'info' | 'success' => {
  switch (status.toLowerCase()) {
    case 'paid': return 'success';
    case 'approved': return 'info';
    case 'pending': return 'warning';
    default: return 'default';
  }
};

const CommissionDetailsPanel: React.FC<CommissionDetailsPanelProps> = ({
  dealValue,
  commissionRate,
  commissionAmount,
  tiers,
  bonuses,
  status,
  paidDate,
  currency = 'USD',
  agentName,
  period,
  onExport,
}) => {
  const totalBonuses = bonuses?.reduce((sum, b) => sum + b.amount, 0) ?? 0;
  const totalEarnings = commissionAmount + totalBonuses;

  return (
    <Card variant="outlined">
      <CardContent>
        {/* Agent & Period meta row */}
        {(agentName || period) && (
          <Box display="flex" gap={1} flexWrap="wrap" mb={1.5}>
            {agentName && (
              <Chip
                icon={<PersonIcon fontSize="small" />}
                label={agentName}
                size="small"
                variant="outlined"
                sx={{ fontWeight: 500 }}
              />
            )}
            {period && (
              <Chip
                icon={<PeriodIcon fontSize="small" />}
                label={period}
                size="small"
                color="primary"
                variant="outlined"
              />
            )}
          </Box>
        )}

        {/* Header: total commission */}
        <Box display="flex" justifyContent="space-between" alignItems="flex-start" mb={2}>
          <Box>
            <Typography variant="caption" color="text.secondary">Total Commission</Typography>
            <Typography variant="h4" fontWeight={700} color="primary.main">
              {formatCurrency(totalEarnings, currency)}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              on {formatCurrency(dealValue, currency)} deal @ {commissionRate}%
            </Typography>
          </Box>
          <Box textAlign="right">
            <Chip label={status} color={statusChipColor(status)} size="small" />
            {paidDate && (
              <Typography variant="caption" display="block" color="text.secondary" mt={0.5}>
                Paid: {new Date(paidDate).toLocaleDateString()}
              </Typography>
            )}
          </Box>
        </Box>

        <Divider sx={{ mb: 2 }} />

        {/* Tier breakdown */}
        {tiers && tiers.length > 0 && (
          <Box mb={2}>
            <Typography variant="subtitle2" gutterBottom fontWeight={600}>
              Tier Breakdown
            </Typography>
            <TableContainer component={Paper} variant="outlined">
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell sx={{ fontWeight: 600 }}>Tier</TableCell>
                    <TableCell align="right" sx={{ fontWeight: 600 }}>Range</TableCell>
                    <TableCell align="right" sx={{ fontWeight: 600 }}>Rate</TableCell>
                    <TableCell align="right" sx={{ fontWeight: 600 }}>Amount</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {tiers.map((tier, idx) => (
                    <TableRow key={idx}>
                      <TableCell>{tier.tierName}</TableCell>
                      <TableCell align="right">
                        {formatCurrency(tier.from, currency)} – {formatCurrency(tier.to, currency)}
                      </TableCell>
                      <TableCell align="right">{tier.rate}%</TableCell>
                      <TableCell align="right">{formatCurrency(tier.amount, currency)}</TableCell>
                    </TableRow>
                  ))}
                  <TableRow>
                    <TableCell colSpan={3} align="right">
                      <Typography variant="body2" fontWeight={600}>Subtotal</Typography>
                    </TableCell>
                    <TableCell align="right">
                      <Typography variant="body2" fontWeight={600}>
                        {formatCurrency(commissionAmount, currency)}
                      </Typography>
                    </TableCell>
                  </TableRow>
                </TableBody>
              </Table>
            </TableContainer>
          </Box>
        )}

        {/* Bonuses */}
        {bonuses && bonuses.length > 0 && (
          <Box mb={1}>
            <Typography variant="subtitle2" gutterBottom fontWeight={600}>
              Bonuses
            </Typography>
            {bonuses.map((bonus, idx) => (
              <Box key={idx} display="flex" justifyContent="space-between" py={0.5}>
                <Typography variant="body2">{bonus.name}</Typography>
                <Typography variant="body2" fontWeight={500} color="success.main">
                  +{formatCurrency(bonus.amount, currency)}
                </Typography>
              </Box>
            ))}
            <Divider sx={{ mt: 1 }} />
            <Box display="flex" justifyContent="space-between" pt={1}>
              <Typography variant="body2" fontWeight={600}>Total Earnings</Typography>
              <Typography variant="body2" fontWeight={700}>
                {formatCurrency(totalEarnings, currency)}
              </Typography>
            </Box>
          </Box>
        )}

        {/* Export button */}
        {onExport && (
          <Box mt={2} display="flex" justifyContent="flex-end">
            <Button
              size="small"
              variant="outlined"
              startIcon={<ExportIcon />}
              onClick={onExport}
            >
              Export
            </Button>
          </Box>
        )}
      </CardContent>
    </Card>
  );
};

export default CommissionDetailsPanel;
