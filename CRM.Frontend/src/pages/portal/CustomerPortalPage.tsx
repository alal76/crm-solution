import React, { useState, useEffect, useCallback } from 'react';
import {
  Alert, Box, Button, Card, CardContent, Chip, CircularProgress,
  Divider, Grid, Stack, Tab, Tabs, Typography
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import SearchIcon from '@mui/icons-material/Search';
import ForumIcon from '@mui/icons-material/Forum';
import HandshakeIcon from '@mui/icons-material/Handshake';
import { useNavigate } from 'react-router-dom';
import apiClient from '../../services/apiClient';

interface Ticket {
  id: number;
  subject: string;
  status: string;
  priority: string;
  createdAt: string;
  updatedAt: string;
}

const statusColor = (s: string): 'default' | 'warning' | 'info' | 'success' | 'error' => {
  switch (s?.toLowerCase()) {
    case 'open': return 'info';
    case 'in_progress': return 'warning';
    case 'resolved': return 'success';
    case 'closed': return 'default';
    default: return 'default';
  }
};

const CustomerPortalPage: React.FC = () => {
  const navigate = useNavigate();
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [tab, setTab] = useState(0);

  const loadTickets = useCallback(async () => {
    setLoading(true);
    try {
      const res = await apiClient.get<Ticket[]>('/api/portal/tickets');
      setTickets(res.data);
    } catch {
      setError('Failed to load tickets.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadTickets(); }, [loadTickets]);

  const filtered = tab === 0 ? tickets
    : tab === 1 ? tickets.filter(t => !['resolved', 'closed'].includes(t.status?.toLowerCase()))
    : tickets.filter(t => ['resolved', 'closed'].includes(t.status?.toLowerCase()));

  return (
    <Box p={3}>
      <Typography variant="h5" fontWeight="bold" gutterBottom>Customer Portal</Typography>

      {/* Quick links */}
      <Grid container spacing={2} mb={3}>
        {[
          { icon: <SearchIcon />, label: 'Search Knowledge Base', path: '/portal/kb' },
          { icon: <HandshakeIcon />, label: 'Partner Deal Registration', path: '/portal/partner' },
          { icon: <ForumIcon />, label: 'Community Forum', path: '/portal/forum' },
        ].map(item => (
          <Grid item xs={12} sm={4} key={item.label}>
            <Card variant="outlined" sx={{ cursor: 'pointer', '&:hover': { bgcolor: 'action.hover' } }}
              onClick={() => navigate(item.path)}>
              <CardContent>
                <Stack direction="row" spacing={1} alignItems="center">
                  {item.icon}
                  <Typography variant="body2" fontWeight="bold">{item.label}</Typography>
                </Stack>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Divider sx={{ mb: 2 }} />

      {/* Ticket list */}
      <Stack direction="row" justifyContent="space-between" alignItems="center" mb={1}>
        <Typography variant="h6">My Tickets</Typography>
        <Button variant="contained" size="small" startIcon={<AddIcon />}
          onClick={() => navigate('/service-requests/new')}>
          New Ticket
        </Button>
      </Stack>

      <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 2 }}>
        <Tab label="All" />
        <Tab label="Open" />
        <Tab label="Resolved" />
      </Tabs>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {loading ? (
        <Box display="flex" justifyContent="center" mt={4}><CircularProgress /></Box>
      ) : filtered.length === 0 ? (
        <Typography color="text.secondary" align="center" mt={4}>No tickets found.</Typography>
      ) : (
        <Stack spacing={1}>
          {filtered.map(t => (
            <Card key={t.id} variant="outlined" sx={{ cursor: 'pointer', '&:hover': { bgcolor: 'action.hover' } }}
              onClick={() => navigate(`/service-requests/${t.id}`)}>
              <CardContent sx={{ py: 1.5, '&:last-child': { pb: 1.5 } }}>
                <Stack direction="row" justifyContent="space-between" alignItems="center">
                  <Box>
                    <Typography variant="body1" fontWeight="medium">#{t.id} — {t.subject}</Typography>
                    <Typography variant="caption" color="text.secondary">
                      Updated {new Date(t.updatedAt).toLocaleDateString()}
                    </Typography>
                  </Box>
                  <Stack direction="row" spacing={1}>
                    <Chip size="small" label={t.priority} />
                    <Chip size="small" label={t.status} color={statusColor(t.status)} />
                  </Stack>
                </Stack>
              </CardContent>
            </Card>
          ))}
        </Stack>
      )}
    </Box>
  );
};

export default CustomerPortalPage;
