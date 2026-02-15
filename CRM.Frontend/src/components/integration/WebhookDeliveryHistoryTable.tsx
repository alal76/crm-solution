/**
 * WebhookDeliveryHistoryTable - Show delivery history for webhooks
 */

import React, { useState } from 'react';
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
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Box,
  Code,
  Tabs,
  Tab,
} from '@mui/material';
import {
  Visibility as ViewIcon,
  Refresh as RetryIcon,
} from '@mui/icons-material';
import { WebhookDelivery, DeliveryStatus } from '../../services/webhookService';

interface WebhookDeliveryHistoryTableProps {
  deliveries: WebhookDelivery[];
  loading?: boolean;
  onRetry?: (deliveryId: number) => Promise<void>;
}

export const WebhookDeliveryHistoryTable: React.FC<WebhookDeliveryHistoryTableProps> = ({
  deliveries = [],
  loading = false,
  onRetry,
}) => {
  const [selectedDelivery, setSelectedDelivery] = useState<WebhookDelivery | null>(null);
  const [tabValue, setTabValue] = useState(0);
  const [retrying, setRetrying] = useState(false);

  const handleRetry = async () => {
    if (!selectedDelivery || !onRetry) return;
    setRetrying(true);
    try {
      await onRetry(selectedDelivery.id);
      setSelectedDelivery(null);
    } finally {
      setRetrying(false);
    }
  };

  const getStatusColor = (status: DeliveryStatus): 'default' | 'success' | 'error' | 'warning' => {
    switch (status) {
      case DeliveryStatus.Delivered:
        return 'success';
      case DeliveryStatus.Failed:
        return 'error';
      case DeliveryStatus.Retrying:
        return 'warning';
      default:
        return 'default';
    }
  };

  const getStatusLabel = (status: DeliveryStatus): string => {
    const labels = ['Pending', 'Delivered', 'Failed', 'Retrying'];
    return labels[status] || 'Unknown';
  };

  return (
    <>
      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow sx={{ bgcolor: 'action.hover' }}>
              <TableCell>Event</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Status Code</TableCell>
              <TableCell>Attempts</TableCell>
              <TableCell>Delivered At</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {deliveries.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} sx={{ textAlign: 'center', py: 3 }}>
                  <Typography color="text.secondary">No delivery history</Typography>
                </TableCell>
              </TableRow>
            ) : (
              deliveries.map((delivery) => (
                <TableRow key={delivery.id} hover>
                  <TableCell>{delivery.event}</TableCell>
                  <TableCell>
                    <Chip
                      label={getStatusLabel(delivery.status)}
                      color={getStatusColor(delivery.status)}
                      size="small"
                    />
                  </TableCell>
                  <TableCell>{delivery.statusCode || '-'}</TableCell>
                  <TableCell>{delivery.attemptCount}</TableCell>
                  <TableCell>
                    {new Date(delivery.deliveredAt).toLocaleString()}
                  </TableCell>
                  <TableCell align="right">
                    <Tooltip title="View Details">
                      <IconButton
                        size="small"
                        onClick={() => setSelectedDelivery(delivery)}
                      >
                        <ViewIcon />
                      </IconButton>
                    </Tooltip>
                    {delivery.status === DeliveryStatus.Failed && onRetry && (
                      <Tooltip title="Retry">
                        <IconButton
                          size="small"
                          onClick={handleRetry}
                        >
                          <RetryIcon />
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

      {/* Detail Dialog */}
      <Dialog open={!!selectedDelivery} onClose={() => setSelectedDelivery(null)} maxWidth="md" fullWidth>
        <DialogTitle>Delivery Details - {selectedDelivery?.event}</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <Tabs value={tabValue} onChange={(_, val) => setTabValue(val)} sx={{ mb: 2 }}>
            <Tab label="Payload" />
            <Tab label="Response" />
            <Tab label="Error" />
          </Tabs>

          {tabValue === 0 && (
            <Box sx={{ bgcolor: '#f5f5f5', p: 2, borderRadius: 1, fontFamily: 'monospace', fontSize: '0.875rem', maxHeight: 300, overflow: 'auto' }}>
              <pre style={{ margin: 0 }}>
                {JSON.stringify(selectedDelivery?.payload, null, 2)}
              </pre>
            </Box>
          )}

          {tabValue === 1 && (
            <Box sx={{ bgcolor: '#f5f5f5', p: 2, borderRadius: 1, fontFamily: 'monospace', fontSize: '0.875rem', maxHeight: 300, overflow: 'auto' }}>
              <pre style={{ margin: 0 }}>
                {selectedDelivery?.responseBody || 'No response body'}
              </pre>
            </Box>
          )}

          {tabValue === 2 && (
            <Box sx={{ bgcolor: '#f5f5f5', p: 2, borderRadius: 1, fontFamily: 'monospace', fontSize: '0.875rem', maxHeight: 300, overflow: 'auto' }}>
              <pre style={{ margin: 0 }}>
                {selectedDelivery?.errorMessage || 'No error'}
              </pre>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSelectedDelivery(null)}>Close</Button>
          {selectedDelivery?.status === DeliveryStatus.Failed && onRetry && (
            <Button
              onClick={handleRetry}
              variant="contained"
              disabled={retrying}
            >
              Retry
            </Button>
          )}
        </DialogActions>
      </Dialog>
    </>
  );
};

export default WebhookDeliveryHistoryTable;
