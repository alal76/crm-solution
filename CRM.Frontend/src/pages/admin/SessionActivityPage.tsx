import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Paper,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Button,
  TextField,
  CircularProgress,
  Alert,
  IconButton,
  Tooltip,
  TablePagination,
} from '@mui/material';
import {
  Devices as DevicesIcon,
  Refresh as RefreshIcon,
  Block as BlockIcon,
  Search as SearchIcon,
} from '@mui/icons-material';
import AdminPageHeader from '../../components/admin/AdminPageHeader';
import apiClient from '../../services/apiClient';

interface UserSession {
  id: number;
  userId: number;
  userName?: string;
  sessionToken: string;
  ipAddress: string;
  userAgent: string;
  createdAt: string;
  lastActivityAt: string;
  expiresAt: string;
  isRevoked: boolean;
  deviceId?: string;
  ipBindingEnabled: boolean;
}

const SessionActivityPage: React.FC = () => {
  const [sessions, setSessions] = useState<UserSession[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchUserId, setSearchUserId] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(25);
  const [totalCount, setTotalCount] = useState(0);

  const fetchSessions = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const params: Record<string, string | number> = {
        page: page + 1,
        pageSize: rowsPerPage,
      };
      if (searchUserId) {
        params.userId = Number.parseInt(searchUserId, 10);
      }
      const response = await apiClient.get('/auth/sessions', { params });
      const data = response.data;
      setSessions(data.items || data || []);
      setTotalCount(data.totalCount || (data.items?.length ?? 0));
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Failed to load sessions';
      setError(message);
    } finally {
      setLoading(false);
    }
  }, [page, rowsPerPage, searchUserId]);

  useEffect(() => {
    fetchSessions();
  }, [fetchSessions]);

  const handleRevokeSession = async (sessionToken: string) => {
    try {
      await apiClient.post('/auth/sessions/revoke', { sessionToken });
      fetchSessions();
    } catch {
      setError('Failed to revoke session');
    }
  };

  const isSessionActive = (session: UserSession) =>
    !session.isRevoked && new Date(session.expiresAt) > new Date();

  const getStatusChip = (session: UserSession) => {
    if (session.isRevoked) return <Chip label="Revoked" color="error" size="small" />;
    if (new Date(session.expiresAt) <= new Date())
      return <Chip label="Expired" color="warning" size="small" />;
    return <Chip label="Active" color="success" size="small" />;
  };

  const formatDate = (dateStr: string) => {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleString();
  };

  const truncateToken = (token: string) =>
    token ? `${token.substring(0, 8)}...` : '—';

  return (
    <Box sx={{ py: 2 }}>
      <AdminPageHeader
        title="Session Activity"
        subtitle="Monitor and manage active user sessions across the system"
        icon={DevicesIcon}
      />

      <Paper sx={{ p: 3, mb: 2 }}>
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: 2 }}>
          <TextField
            label="Filter by User ID"
            size="small"
            value={searchUserId}
            onChange={(e) => setSearchUserId(e.target.value)}
            sx={{ width: 200 }}
            InputProps={{
              endAdornment: <SearchIcon color="action" />,
            }}
          />
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={fetchSessions}
            disabled={loading}
          >
            Refresh
          </Button>
        </Box>

        {error && (
          <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
            {error}
          </Alert>
        )}

        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : (
          <>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>ID</TableCell>
                    <TableCell>User ID</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>IP Address</TableCell>
                    <TableCell>IP Binding</TableCell>
                    <TableCell>Device</TableCell>
                    <TableCell>Token</TableCell>
                    <TableCell>Created</TableCell>
                    <TableCell>Last Activity</TableCell>
                    <TableCell>Expires</TableCell>
                    <TableCell>Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {sessions.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={11} align="center">
                        <Typography color="text.secondary">No sessions found</Typography>
                      </TableCell>
                    </TableRow>
                  ) : (
                    sessions.map((session) => (
                      <TableRow key={session.id} hover>
                        <TableCell>{session.id}</TableCell>
                        <TableCell>{session.userId}</TableCell>
                        <TableCell>{getStatusChip(session)}</TableCell>
                        <TableCell>
                          <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                            {session.ipAddress}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          {session.ipBindingEnabled ? (
                            <Chip label="Enabled" color="info" size="small" variant="outlined" />
                          ) : (
                            <Chip label="Off" size="small" variant="outlined" />
                          )}
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2" noWrap sx={{ maxWidth: 120 }}>
                            {session.deviceId || '—'}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Tooltip title={session.sessionToken || ''}>
                            <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                              {truncateToken(session.sessionToken)}
                            </Typography>
                          </Tooltip>
                        </TableCell>
                        <TableCell>{formatDate(session.createdAt)}</TableCell>
                        <TableCell>{formatDate(session.lastActivityAt)}</TableCell>
                        <TableCell>{formatDate(session.expiresAt)}</TableCell>
                        <TableCell>
                          {isSessionActive(session) && (
                            <Tooltip title="Revoke session">
                              <IconButton
                                size="small"
                                color="error"
                                onClick={() => handleRevokeSession(session.sessionToken)}
                              >
                                <BlockIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                          )}
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </TableContainer>
            <TablePagination
              component="div"
              count={totalCount}
              page={page}
              onPageChange={(_, newPage) => setPage(newPage)}
              rowsPerPage={rowsPerPage}
              onRowsPerPageChange={(e) => {
                setRowsPerPage(Number.parseInt(e.target.value, 10));
                setPage(0);
              }}
              rowsPerPageOptions={[10, 25, 50, 100]}
            />
          </>
        )}
      </Paper>
    </Box>
  );
};

export default SessionActivityPage;
