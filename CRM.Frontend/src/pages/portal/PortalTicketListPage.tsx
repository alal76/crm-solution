// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
  AppBar,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  TablePagination,
  TextField,
  Toolbar,
  Typography,
  Alert,
} from '@mui/material';
import { Add, ArrowBack, ExitToApp, SupportAgent } from '@mui/icons-material';
import { useNavigate, Link } from 'react-router-dom';
import {
  portalAuthService,
  portalService,
  type PortalTicketDto,
  type PortalConfigDto,
} from '../../services/portalService';

const PortalTicketListPage: React.FC = () => {
  const navigate = useNavigate();
  const [tickets, setTickets] = useState<PortalTicketDto[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(0); // 0-indexed for TablePagination
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);
  const [config, setConfig] = useState<PortalConfigDto | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [createForm, setCreateForm] = useState({ title: '', description: '', priority: 'Medium' });
  const [createLoading, setCreateLoading] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);

  const user = portalAuthService.getCurrentUser();
  const brandColor = config?.primaryColor ?? '#1976d2';

  const loadTickets = useCallback(async () => {
    setLoading(true);
    try {
      const result = await portalService.getMyTickets(page + 1, pageSize);
      setTickets(result.items);
      setTotal(result.totalCount);
    } catch {
      // ignore
    } finally {
      setLoading(false);
    }
  }, [page, pageSize]);

  useEffect(() => {
    if (!portalAuthService.isAuthenticated()) {
      navigate('/portal/login', { replace: true });
      return;
    }
    portalService.getConfig().then(setConfig).catch(() => {});
    loadTickets();
  }, [navigate, loadTickets]);

  const handleLogout = () => {
    portalAuthService.logout();
    navigate('/portal/login', { replace: true });
  };

  const handleCreate = async () => {
    if (!createForm.title.trim()) {
      setCreateError('Title is required.');
      return;
    }
    setCreateError(null);
    setCreateLoading(true);
    try {
      await portalService.createTicket(createForm);
      setCreateOpen(false);
      setCreateForm({ title: '', description: '', priority: 'Medium' });
      loadTickets();
    } catch (err: unknown) {
      setCreateError((err as any)?.response?.data?.message ?? 'Failed to create ticket.');
    } finally {
      setCreateLoading(false);
    }
  };

  const statusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'new': return 'info';
      case 'open': return 'primary';
      case 'resolved': case 'closed': return 'success';
      case 'on hold': return 'warning';
      default: return 'default';
    }
  };

  const priorityColor = (priority: string) => {
    switch (priority.toLowerCase()) {
      case 'critical': return 'error';
      case 'high': return 'warning';
      case 'medium': return 'info';
      default: return 'default';
    }
  };

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <AppBar position="static" sx={{ bgcolor: brandColor }}>
        <Toolbar>
          <IconButton color="inherit" component={Link} to="/portal/dashboard" sx={{ mr: 1 }}>
            <ArrowBack />
          </IconButton>
          <SupportAgent sx={{ mr: 1 }} />
          <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: 700 }}>
            My Tickets
          </Typography>
          <Typography variant="body2" sx={{ mr: 2 }}>{user?.displayName ?? user?.email}</Typography>
          <IconButton color="inherit" onClick={handleLogout} title="Sign out">
            <ExitToApp />
          </IconButton>
        </Toolbar>
      </AppBar>

      <Box sx={{ p: 3, maxWidth: 1000, mx: 'auto' }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h5" fontWeight={700}>Support Tickets</Typography>
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={() => setCreateOpen(true)}
            sx={{ bgcolor: brandColor }}
          >
            New Ticket
          </Button>
        </Box>

        {loading ? (
          <Box sx={{ textAlign: 'center', py: 6 }}>
            <CircularProgress />
          </Box>
        ) : tickets.length === 0 ? (
          <Card>
            <CardContent sx={{ textAlign: 'center', py: 6 }}>
              <Typography color="text.secondary" mb={2}>No tickets found.</Typography>
              <Button variant="contained" startIcon={<Add />} onClick={() => setCreateOpen(true)} sx={{ bgcolor: brandColor }}>
                Create Your First Ticket
              </Button>
            </CardContent>
          </Card>
        ) : (
          <Card>
            <CardContent sx={{ p: 0 }}>
              {tickets.map((ticket, idx) => (
                <React.Fragment key={ticket.id}>
                  {idx > 0 && <Divider />}
                  <Box sx={{ p: 2, display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
                    <Box sx={{ flexGrow: 1, mr: 2 }}>
                      <Typography variant="body1" fontWeight={600}>{ticket.title}</Typography>
                      <Typography variant="caption" color="text.secondary" sx={{ mr: 1 }}>
                        {ticket.ticketNumber}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        · {new Date(ticket.createdAt).toLocaleDateString()}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', gap: 1 }}>
                      <Chip label={ticket.priority} size="small" color={priorityColor(ticket.priority) as any} />
                      <Chip label={ticket.status} size="small" color={statusColor(ticket.status) as any} />
                    </Box>
                  </Box>
                </React.Fragment>
              ))}
              <Divider />
              <TablePagination
                component="div"
                count={total}
                page={page}
                onPageChange={(_e, newPage) => setPage(newPage)}
                rowsPerPage={pageSize}
                onRowsPerPageChange={(e) => { setPageSize(Number.parseInt(e.target.value, 10)); setPage(0); }}
                rowsPerPageOptions={[5, 10, 25]}
              />
            </CardContent>
          </Card>
        )}
      </Box>

      {/* Create Ticket Dialog */}
      <Dialog open={createOpen} onClose={() => setCreateOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>New Support Ticket</DialogTitle>
        <DialogContent>
          {createError && <Alert severity="error" sx={{ mb: 2 }}>{createError}</Alert>}
          <TextField
            label="Title *"
            fullWidth
            value={createForm.title}
            onChange={(e) => setCreateForm((prev) => ({ ...prev, title: e.target.value }))}
            sx={{ mt: 1, mb: 2 }}
            autoFocus
          />
          <TextField
            label="Description"
            fullWidth
            multiline
            rows={4}
            value={createForm.description}
            onChange={(e) => setCreateForm((prev) => ({ ...prev, description: e.target.value }))}
            sx={{ mb: 2 }}
          />
          <FormControl fullWidth>
            <InputLabel>Priority</InputLabel>
            <Select
              value={createForm.priority}
              label="Priority"
              onChange={(e) => setCreateForm((prev) => ({ ...prev, priority: e.target.value }))}
            >
              <MenuItem value="Low">Low</MenuItem>
              <MenuItem value="Medium">Medium</MenuItem>
              <MenuItem value="High">High</MenuItem>
              <MenuItem value="Critical">Critical</MenuItem>
            </Select>
          </FormControl>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCreateOpen(false)}>Cancel</Button>
          <Button
            onClick={handleCreate}
            variant="contained"
            disabled={createLoading}
            sx={{ bgcolor: brandColor }}
          >
            {createLoading ? <CircularProgress size={20} color="inherit" /> : 'Create Ticket'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default PortalTicketListPage;
