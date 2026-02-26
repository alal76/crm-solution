// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  CircularProgress,
  Grid,
  Typography,
  AppBar,
  Toolbar,
  IconButton,
  Chip,
  Divider,
} from '@mui/material';
import {
  ConfirmationNumber,
  MenuBook,
  ExitToApp,
  SupportAgent,
  Add,
} from '@mui/icons-material';
import { useNavigate, Link } from 'react-router-dom';
import { portalAuthService, portalService, type PortalConfigDto, type PortalTicketDto } from '../../services/portalService';

const PortalDashboardPage: React.FC = () => {
  const navigate = useNavigate();
  const [config, setConfig] = useState<PortalConfigDto | null>(null);
  const [recentTickets, setRecentTickets] = useState<PortalTicketDto[]>([]);
  const [ticketTotal, setTicketTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const user = portalAuthService.getCurrentUser();

  const loadData = useCallback(async () => {
    try {
      const [cfg, tickets] = await Promise.all([
        portalService.getConfig(),
        portalService.getMyTickets(1, 5),
      ]);
      setConfig(cfg);
      setRecentTickets(tickets.items);
      setTicketTotal(tickets.totalCount);
    } catch {
      // ignore
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (!portalAuthService.isAuthenticated()) {
      navigate('/portal/login', { replace: true });
      return;
    }
    loadData();
  }, [navigate, loadData]);

  const handleLogout = () => {
    portalAuthService.logout();
    navigate('/portal/login', { replace: true });
  };

  const brandColor = config?.primaryColor ?? '#1976d2';
  const portalTitle = config?.portalTitle ?? 'Customer Portal';

  const statusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'new': return 'info';
      case 'open': return 'primary';
      case 'resolved': case 'closed': return 'success';
      default: return 'default';
    }
  };

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <AppBar position="static" sx={{ bgcolor: brandColor }}>
        <Toolbar>
          <SupportAgent sx={{ mr: 1 }} />
          <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: 700 }}>
            {portalTitle}
          </Typography>
          <Typography variant="body2" sx={{ mr: 2 }}>
            {user?.displayName ?? user?.email}
          </Typography>
          <IconButton color="inherit" onClick={handleLogout} title="Sign out">
            <ExitToApp />
          </IconButton>
        </Toolbar>
      </AppBar>

      <Box sx={{ p: 3, maxWidth: 1000, mx: 'auto' }}>
        {/* Welcome */}
        {config?.welcomeMessage && (
          <Card sx={{ mb: 3, bgcolor: brandColor + '12' }}>
            <CardContent>
              <Typography variant="body1">{config.welcomeMessage}</Typography>
            </CardContent>
          </Card>
        )}

        <Typography variant="h5" fontWeight={700} mb={3}>
          Welcome, {user?.displayName ?? user?.email}
        </Typography>

        {/* Quick Stats */}
        <Grid container spacing={2} mb={4}>
          <Grid item xs={12} sm={6}>
            <Card component={Link} to="/portal/tickets" sx={{ textDecoration: 'none', display: 'block', cursor: 'pointer', '&:hover': { boxShadow: 4 } }}>
              <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <ConfirmationNumber sx={{ fontSize: 40, color: brandColor }} />
                <Box>
                  <Typography variant="h4" fontWeight={700}>{ticketTotal}</Typography>
                  <Typography variant="body2" color="text.secondary">Total Support Tickets</Typography>
                </Box>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6}>
            <Card component={Link} to="/portal/knowledge-base" sx={{ textDecoration: 'none', display: 'block', cursor: 'pointer', '&:hover': { boxShadow: 4 } }}>
              <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <MenuBook sx={{ fontSize: 40, color: '#2e7d32' }} />
                <Box>
                  <Typography variant="h4" fontWeight={700}>KB</Typography>
                  <Typography variant="body2" color="text.secondary">Knowledge Base Articles</Typography>
                </Box>
              </CardContent>
            </Card>
          </Grid>
        </Grid>

        {/* Quick Actions */}
        <Box sx={{ display: 'flex', gap: 2, mb: 4 }}>
          <Button
            variant="contained"
            startIcon={<Add />}
            component={Link}
            to="/portal/tickets?create=1"
            sx={{ bgcolor: brandColor }}
          >
            New Ticket
          </Button>
          <Button
            variant="outlined"
            startIcon={<MenuBook />}
            component={Link}
            to="/portal/knowledge-base"
          >
            Browse Knowledge Base
          </Button>
        </Box>

        {/* Recent Tickets */}
        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
              <Typography variant="h6" fontWeight={700}>Recent Tickets</Typography>
              <Button size="small" component={Link} to="/portal/tickets">View All</Button>
            </Box>
            {loading ? (
              <Box sx={{ textAlign: 'center', py: 3 }}>
                <CircularProgress size={28} />
              </Box>
            ) : recentTickets.length === 0 ? (
              <Typography variant="body2" color="text.secondary" sx={{ py: 2, textAlign: 'center' }}>
                No tickets yet. Create your first support ticket above.
              </Typography>
            ) : (
              <>
                {recentTickets.map((ticket, idx) => (
                  <React.Fragment key={ticket.id}>
                    {idx > 0 && <Divider />}
                    <Box
                      component={Link}
                      to={`/portal/tickets`}
                      sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', py: 1.5, textDecoration: 'none', color: 'inherit', '&:hover': { bgcolor: 'action.hover' } }}
                    >
                      <Box>
                        <Typography variant="body2" fontWeight={600}>{ticket.title}</Typography>
                        <Typography variant="caption" color="text.secondary">{ticket.ticketNumber}</Typography>
                      </Box>
                      <Chip
                        label={ticket.status}
                        size="small"
                        color={statusColor(ticket.status) as any}
                      />
                    </Box>
                  </React.Fragment>
                ))}
              </>
            )}
          </CardContent>
        </Card>

        {/* Support Contact */}
        {config?.supportEmail && (
          <Card sx={{ mt: 3 }}>
            <CardContent>
              <Typography variant="body2" color="text.secondary">
                Need help? Email us at{' '}
                <a href={`mailto:${config.supportEmail}`} style={{ color: brandColor }}>
                  {config.supportEmail}
                </a>
              </Typography>
            </CardContent>
          </Card>
        )}
      </Box>
    </Box>
  );
};

export default PortalDashboardPage;
