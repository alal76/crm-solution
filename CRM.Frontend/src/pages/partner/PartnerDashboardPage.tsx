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
  Grid,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material';
import HandshakeIcon from '@mui/icons-material/Handshake';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import PeopleIcon from '@mui/icons-material/People';
import AttachMoneyIcon from '@mui/icons-material/AttachMoney';
import AccountBalanceIcon from '@mui/icons-material/AccountBalance';
import { useAuth } from '../../contexts/AuthContext';
import partnerPortalService, { type PartnerDashboardDto } from '../../services/partnerPortalService';

const stageColor = (stage: string): 'default' | 'info' | 'warning' | 'success' | 'error' => {
  const s = stage.toLowerCase();
  if (s.includes('closedwon') || s.includes('closed_won')) return 'success';
  if (s.includes('closedlost') || s.includes('closed_lost')) return 'error';
  if (s.includes('negotiation') || s.includes('proposal')) return 'warning';
  return 'info';
};

const leadStatusColor = (status: string): 'default' | 'info' | 'warning' | 'success' | 'error' => {
  const s = status.toLowerCase();
  if (s === 'qualified') return 'success';
  if (s === 'disqualified') return 'error';
  if (s === 'contacted') return 'warning';
  return 'info';
};

interface StatCard {
  label: string;
  value: string | number;
  icon: React.ReactNode;
  color: string;
}

const PartnerDashboardPage: React.FC = () => {
  const { user } = useAuth();
  const [data, setData] = useState<PartnerDashboardDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const dashboard = await partnerPortalService.getDashboard();
      setData(dashboard);
    } catch {
      setError('Failed to load partner dashboard. Please try again.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const partnerName = user?.firstName
    ? `${user.firstName}${user.lastName ? ' ' + user.lastName : ''}`
    : user?.email ?? 'Partner';

  const statCards: StatCard[] = data
    ? [
        {
          label: 'Active Deals',
          value: data.activeDealCount,
          icon: <TrendingUpIcon sx={{ fontSize: 40, color: 'primary.main' }} />,
          color: '#e3f2fd',
        },
        {
          label: 'Total Leads',
          value: data.totalLeadCount,
          icon: <PeopleIcon sx={{ fontSize: 40, color: 'success.main' }} />,
          color: '#e8f5e9',
        },
        {
          label: 'Commission This Month',
          value: `$${data.commissionEarnedThisMonth.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`,
          icon: <AttachMoneyIcon sx={{ fontSize: 40, color: 'warning.main' }} />,
          color: '#fff8e1',
        },
        {
          label: 'Pipeline Value',
          value: `$${data.pipelineValue.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`,
          icon: <AccountBalanceIcon sx={{ fontSize: 40, color: 'info.main' }} />,
          color: '#e8eaf6',
        },
      ]
    : [];

  return (
    <Box p={3}>
      {/* Header */}
      <Box display="flex" alignItems="center" gap={1} mb={3}>
        <HandshakeIcon color="primary" sx={{ fontSize: 32 }} />
        <Box>
          <Typography variant="h5" fontWeight="bold">Partner Dashboard</Typography>
          <Typography variant="body2" color="text.secondary">
            Welcome back, {partnerName}
          </Typography>
        </Box>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {loading ? (
        <Box display="flex" justifyContent="center" mt={6}>
          <CircularProgress />
        </Box>
      ) : data ? (
        <>
          {/* Stat Cards */}
          <Grid container spacing={2} mb={3}>
            {statCards.map(card => (
              <Grid item xs={12} sm={6} md={3} key={card.label}>
                <Card variant="outlined" sx={{ bgcolor: card.color }}>
                  <CardContent>
                    <Box display="flex" justifyContent="space-between" alignItems="flex-start">
                      <Box>
                        <Typography variant="body2" color="text.secondary" gutterBottom>
                          {card.label}
                        </Typography>
                        <Typography variant="h5" fontWeight="bold">
                          {card.value}
                        </Typography>
                      </Box>
                      {card.icon}
                    </Box>
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>

          {/* Recent Deals */}
          <Grid container spacing={2}>
            <Grid item xs={12} md={6}>
              <Card variant="outlined">
                <CardContent>
                  <Typography variant="h6" gutterBottom>Recent Deals</Typography>
                  {data.recentDeals.length === 0 ? (
                    <Typography color="text.secondary" variant="body2">No deals yet.</Typography>
                  ) : (
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>Deal</TableCell>
                          <TableCell>Stage</TableCell>
                          <TableCell align="right">Amount</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {data.recentDeals.map(deal => (
                          <TableRow key={deal.id} hover>
                            <TableCell>
                              <Typography variant="body2" fontWeight="medium" noWrap sx={{ maxWidth: 160 }}>
                                {deal.name}
                              </Typography>
                              <Typography variant="caption" color="text.secondary">
                                {deal.expectedCloseDate
                                  ? `Close: ${new Date(deal.expectedCloseDate).toLocaleDateString()}`
                                  : ''}
                              </Typography>
                            </TableCell>
                            <TableCell>
                              <Chip label={deal.stage} size="small" color={stageColor(deal.stage)} />
                            </TableCell>
                            <TableCell align="right">
                              <Typography variant="body2">
                                {deal.currency} {deal.amount.toLocaleString('en-US', { minimumFractionDigits: 0 })}
                              </Typography>
                            </TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  )}
                </CardContent>
              </Card>
            </Grid>

            {/* Recent Leads */}
            <Grid item xs={12} md={6}>
              <Card variant="outlined">
                <CardContent>
                  <Typography variant="h6" gutterBottom>Recent Leads</Typography>
                  {data.recentLeads.length === 0 ? (
                    <Typography color="text.secondary" variant="body2">No leads yet.</Typography>
                  ) : (
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>Lead</TableCell>
                          <TableCell>Company</TableCell>
                          <TableCell>Status</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {data.recentLeads.map(lead => (
                          <TableRow key={lead.id} hover>
                            <TableCell>
                              <Typography variant="body2" fontWeight="medium">
                                {lead.firstName} {lead.lastName}
                              </Typography>
                              <Typography variant="caption" color="text.secondary">
                                {lead.email}
                              </Typography>
                            </TableCell>
                            <TableCell>
                              <Typography variant="body2" noWrap sx={{ maxWidth: 120 }}>
                                {lead.companyName ?? '—'}
                              </Typography>
                            </TableCell>
                            <TableCell>
                              <Chip label={lead.status} size="small" color={leadStatusColor(lead.status)} />
                            </TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  )}
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </>
      ) : null}
    </Box>
  );
};

export default PartnerDashboardPage;
