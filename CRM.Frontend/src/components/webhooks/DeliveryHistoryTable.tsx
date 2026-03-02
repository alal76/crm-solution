/**
 * DeliveryHistoryTable - Display webhook delivery history with retry support
 */

import React, { useState, useCallback } from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  IconButton,
  Tooltip,
  Typography,
  Box,
  TablePagination,
  CircularProgress,
  Stack,
} from '@mui/material';
import {
  Refresh as RetryIcon,
  Visibility as ViewIcon,
  CheckCircle as SuccessIcon,
  Cancel as FailIcon,
} from '@mui/icons-material';

export interface WebhookDelivery {
  id: number;
  eventType: string;
  statusCode: number;
  success: boolean;
  responseTime: number;
  createdAt: string;
  retryCount: number;
  error?: string;
}

interface DeliveryHistoryTableProps {
  webhookId: number;
  deliveries?: WebhookDelivery[];
  loading?: boolean;
  onRetry?: (deliveryId: number) => void;
  onViewDetail?: (delivery: WebhookDelivery) => void;
}

const formatDate = (dateStr: string): string => {
  try {
    return new Date(dateStr).toLocaleString();
  } catch {
    return dateStr;
  }
};

const formatEventType = (eventType: string): string => {
  return eventType
    .split('.')
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(' → ');
};

const DeliveryHistoryTable: React.FC<DeliveryHistoryTableProps> = ({
  webhookId: _webhookId,
  deliveries = [],
  loading = false,
  onRetry,
  onViewDetail,
}) => {
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [retryingId, setRetryingId] = useState<number | null>(null);

  const handleRetry = useCallback(
    async (deliveryId: number) => {
      setRetryingId(deliveryId);
      try {
        onRetry?.(deliveryId);
      } finally {
        // Small delay to show feedback
        setTimeout(() => setRetryingId(null), 500);
      }
    },
    [onRetry]
  );

  const handleChangePage = useCallback((_event: unknown, newPage: number) => {
    setPage(newPage);
  }, []);

  const handleChangeRowsPerPage = useCallback(
    (event: React.ChangeEvent<HTMLInputElement>) => {
      setRowsPerPage(Number.parseInt(event.target.value, 10));
      setPage(0);
    },
    []
  );

  const paginatedDeliveries = deliveries.slice(
    page * rowsPerPage,
    page * rowsPerPage + rowsPerPage
  );

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" py={4}>
        <CircularProgress />
      </Box>
    );
  }

  if (deliveries.length === 0) {
    return (
      <Paper variant="outlined" sx={{ p: 3, textAlign: 'center' }}>
        <Typography color="text.secondary">No delivery history found.</Typography>
      </Paper>
    );
  }

  return (
    <Box>
      <TableContainer component={Paper} variant="outlined">
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Event Type</TableCell>
              <TableCell>Status</TableCell>
              <TableCell align="right">Status Code</TableCell>
              <TableCell align="right">Response Time</TableCell>
              <TableCell align="right">Retries</TableCell>
              <TableCell>Created At</TableCell>
              <TableCell align="center">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {paginatedDeliveries.map((delivery) => (
              <TableRow
                key={delivery.id}
                hover
                sx={{
                  '&:last-child td, &:last-child th': { border: 0 },
                  cursor: onViewDetail ? 'pointer' : 'default',
                }}
                onClick={() => onViewDetail?.(delivery)}
              >
                <TableCell>
                  <Typography variant="body2" fontFamily="monospace" fontSize="0.8rem">
                    {formatEventType(delivery.eventType)}
                  </Typography>
                </TableCell>
                <TableCell>
                  <Chip
                    icon={delivery.success ? <SuccessIcon /> : <FailIcon />}
                    label={delivery.success ? 'Success' : 'Failed'}
                    color={delivery.success ? 'success' : 'error'}
                    size="small"
                    variant="outlined"
                  />
                </TableCell>
                <TableCell align="right">
                  <Chip
                    label={delivery.statusCode || 'N/A'}
                    size="small"
                    color={
                      delivery.statusCode >= 200 && delivery.statusCode < 300
                        ? 'success'
                        : delivery.statusCode >= 400
                        ? 'error'
                        : 'default'
                    }
                    variant="filled"
                  />
                </TableCell>
                <TableCell align="right">
                  <Typography variant="body2">{delivery.responseTime}ms</Typography>
                </TableCell>
                <TableCell align="right">
                  <Typography variant="body2">
                    {delivery.retryCount > 0 ? delivery.retryCount : '—'}
                  </Typography>
                </TableCell>
                <TableCell>
                  <Typography variant="body2" color="text.secondary">
                    {formatDate(delivery.createdAt)}
                  </Typography>
                </TableCell>
                <TableCell align="center">
                  <Stack
                    direction="row"
                    spacing={0.5}
                    justifyContent="center"
                    onClick={(e) => e.stopPropagation()}
                  >
                    {onViewDetail && (
                      <Tooltip title="View Details">
                        <IconButton
                          size="small"
                          onClick={() => onViewDetail(delivery)}
                        >
                          <ViewIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    )}
                    {!delivery.success && onRetry && (
                      <Tooltip title="Retry Delivery">
                        <IconButton
                          size="small"
                          color="warning"
                          onClick={() => handleRetry(delivery.id)}
                          disabled={retryingId === delivery.id}
                        >
                          {retryingId === delivery.id ? (
                            <CircularProgress size={16} />
                          ) : (
                            <RetryIcon fontSize="small" />
                          )}
                        </IconButton>
                      </Tooltip>
                    )}
                  </Stack>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <TablePagination
        component="div"
        count={deliveries.length}
        page={page}
        onPageChange={handleChangePage}
        rowsPerPage={rowsPerPage}
        onRowsPerPageChange={handleChangeRowsPerPage}
        rowsPerPageOptions={[5, 10, 25, 50]}
      />
    </Box>
  );
};

export default DeliveryHistoryTable;
