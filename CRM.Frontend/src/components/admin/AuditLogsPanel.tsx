import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Button,
  Divider,
  TextField,
  InputAdornment,
  Pagination,
  CircularProgress,
  Alert,
} from '@mui/material';
import { Search as SearchIcon, FileDownload as DownloadIcon } from '@mui/icons-material';
import logger from '../../services/logger';
import auditLogService, { AuditLogDto } from '../../services/auditLogService';

/**
 * Audit Logs Panel - View system activity and changes
 */
const AuditLogsPanel: React.FC = () => {
  const [logs, setLogs] = useState<AuditLogDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [totalPages, setTotalPages] = useState(1);

  const loadAuditLogs = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      let result;
      if (searchTerm.trim()) {
        result = await auditLogService.searchAuditLogs(searchTerm.trim(), page, pageSize);
      } else {
        result = await auditLogService.getAuditLogs({ pageNumber: page, pageSize });
      }
      setLogs(result.items);
      setTotalPages(result.totalPages || 1);
      logger.info('Audit logs loaded');
    } catch (err) {
      logger.error('Failed to load audit logs', err);
      setError('Failed to load audit logs. Please try again.');
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, searchTerm]);

  useEffect(() => {
    loadAuditLogs();
  }, [loadAuditLogs]);

  const handleExport = async () => {
    try {
      logger.info('Exporting audit logs');
      const blob = await auditLogService.exportAuditLogs();
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `audit-logs-${new Date().toISOString().slice(0, 10)}.csv`;
      a.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      logger.error('Failed to export audit logs', err);
    }
  };

  return (
    <Box>
      <Card>
        <CardHeader
          title="Audit Logging"
          subtitle="View and track system changes"
          action={
            <Button
              variant="outlined"
              startIcon={<DownloadIcon />}
              onClick={handleExport}
            >
              Export
            </Button>
          }
        />
        <Divider />
        <CardContent>
          <Alert severity="info" sx={{ mb: 2 }}>
            This audit log tracks all system changes made by administrators. Logs are retained for 90 days.
          </Alert>

          {error && (
            <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
              {error}
            </Alert>
          )}

          <TextField
            fullWidth
            placeholder="Search audit logs..."
            variant="outlined"
            size="small"
            sx={{ mb: 2 }}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon />
                </InputAdornment>
              ),
            }}
            value={searchTerm}
            onChange={(e) => {
              setSearchTerm(e.target.value);
              setPage(1);
            }}
          />

          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
              <CircularProgress />
            </Box>
          ) : (
            <>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow sx={{ bgcolor: 'grey.100' }}>
                      <TableCell>Timestamp</TableCell>
                      <TableCell>User</TableCell>
                      <TableCell>Action</TableCell>
                      <TableCell>Entity</TableCell>
                      <TableCell>IP Address</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {logs.map(log => (
                      <TableRow key={log.id}>
                        <TableCell sx={{ fontSize: '0.85rem' }}>
                          {new Date(log.timestamp).toLocaleString()}
                        </TableCell>
                        <TableCell>{log.userName}</TableCell>
                        <TableCell>
                          <Box
                            sx={{
                              display: 'inline-block',
                              px: 1,
                              py: 0.5,
                              bgcolor:
                                log.action === 'UPDATE'
                                  ? 'warning.light'
                                  : log.action === 'DELETE'
                                    ? 'error.light'
                                    : 'success.light',
                              color:
                                log.action === 'UPDATE'
                                  ? 'warning.dark'
                                  : log.action === 'DELETE'
                                    ? 'error.dark'
                                    : 'success.dark',
                              borderRadius: 1,
                              fontSize: '0.75rem',
                              fontWeight: 600,
                            }}
                          >
                            {log.action}
                          </Box>
                        </TableCell>
                        <TableCell>
                          {log.entityType} #{log.entityId}
                        </TableCell>
                        <TableCell sx={{ fontSize: '0.85rem' }}>
                          {log.ipAddress}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>

              <Box sx={{ display: 'flex', justifyContent: 'center', mt: 2 }}>
                <Pagination
                  count={totalPages}
                  page={page}
                  onChange={(e, p) => setPage(p)}
                />
              </Box>
            </>
          )}
        </CardContent>
      </Card>
    </Box>
  );
};

export default AuditLogsPanel;
export { AuditLogsPanel };
