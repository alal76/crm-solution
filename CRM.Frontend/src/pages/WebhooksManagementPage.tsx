/**
 * WebhooksManagementPage - Webhook CRUD and testing
 */

import React, { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Typography,
  Button,
  Card,
  CardContent,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  TablePagination,
  CircularProgress,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Stack,
  Chip,
  IconButton,
  Tooltip,
  Tabs,
  Tab,
  Switch,
  FormControlLabel,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Visibility as ViewIcon,
  Refresh as RefreshIcon,
  PlayArrow as TestIcon,
  History as HistoryIcon,
} from '@mui/icons-material';
import { useApiState } from '../hooks/useApiState';
import {
  DialogError,
  DialogSuccess,
  ActionButton,
  EnhancedEmptyState,
  DialogHeader,
} from '../components/common';
import webhookService, {
  Webhook,
  WebhookStatus,
  WebhookDelivery,
} from '../services/webhookService';
import {
  WebhookForm,
  WebhookDeliveryHistoryTable,
} from '../components/integration';
import logger from '../services/logger';
import logo from '../assets/logo.png';

const getStatusLabel = (status: WebhookStatus): string => {
  const labels = ['Active', 'Inactive', 'Paused', 'Disabled'];
  return labels[status] || 'Unknown';
};

const getStatusColor = (status: WebhookStatus): any => {
  const colors = {
    [WebhookStatus.Active]: 'success',
    [WebhookStatus.Inactive]: 'default',
    [WebhookStatus.Paused]: 'warning',
    [WebhookStatus.Disabled]: 'error',
  };
  return colors[status];
};

export const WebhooksManagementPage: React.FC = () => {
  const { loading, error, setError, clearError } = useApiState();
  const [webhooks, setWebhooks] = useState<Webhook[]>([]);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(20);
  const [totalCount, setTotalCount] = useState(0);
  const [selectedWebhook, setSelectedWebhook] = useState<Webhook | null>(null);
  const [detailDialogOpen, setDetailDialogOpen] = useState(false);
  const [formDialogOpen, setFormDialogOpen] = useState(false);
  const [editingWebhook, setEditingWebhook] = useState<Webhook | undefined>();
  const [detailTabValue, setDetailTabValue] = useState(0);
  const [deliveryHistory, setDeliveryHistory] = useState<WebhookDelivery[]>([]);
  const [deliveryPage, setDeliveryPage] = useState(0);

  // Load webhooks
  const loadWebhooks = async () => {
    try {
      const result = await webhookService.getWebhooks(page + 1, pageSize);
      setWebhooks(result.items);
      setTotalCount(result.totalCount);
      clearError();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to load webhooks';
      setError(message);
      logger.error('Failed to load webhooks', err);
    }
  };

  useEffect(() => {
    loadWebhooks();
  }, [page, pageSize]);

  // Load delivery history when webhook is selected
  useEffect(() => {
    if (selectedWebhook && detailTabValue === 1) {
      webhookService
        .getDeliveries(selectedWebhook.id, deliveryPage + 1, 20)
        .then((result) => setDeliveryHistory(result.items))
        .catch((err) => logger.error('Failed to load delivery history', err));
    }
  }, [selectedWebhook, detailTabValue, deliveryPage]);

  const handleSaveWebhook = async (webhook: any) => {
    try {
      if (editingWebhook?.id) {
        await webhookService.updateWebhook(editingWebhook.id, webhook);
      } else {
        await webhookService.createWebhook(webhook);
      }
      setFormDialogOpen(false);
      setEditingWebhook(undefined);
      await loadWebhooks();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save webhook');
    }
  };

  const handleDeleteWebhook = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this webhook?')) return;

    try {
      await webhookService.deleteWebhook(id);
      await loadWebhooks();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete webhook');
    }
  };

  const handleToggleStatus = async (webhook: Webhook) => {
    try {
      if (webhook.isActive) {
        await webhookService.pauseWebhook(webhook.id);
      } else {
        await webhookService.resumeWebhook(webhook.id);
      }
      await loadWebhooks();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update webhook status');
    }
  };

  const handleTestWebhook = async (webhook: Webhook) => {
    try {
      const result = await webhookService.testWebhook(webhook.id, {
        event: webhook.events[0],
        payload: { test: true },
      });

      if (result.success) {
        alert(`Webhook test successful!\nStatus Code: ${result.statusCode}\nDelivery Time: ${result.deliveryTime}ms`);
      } else {
        alert(`Webhook test failed!\n${result.errorMessage}`);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to test webhook');
    }
  };

  return (
    <Box sx={{ bgcolor: 'background.default', minHeight: '100vh', py: 3 }}>
      <Container maxWidth="lg">
        {/* Header */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <img src={logo} alt="Logo" style={{ height: 40 }} />
            <Typography variant="h5" fontWeight="bold">
              Webhooks Management
            </Typography>
          </Box>
          <Stack direction="row" spacing={1}>
            <Button
              startIcon={<RefreshIcon />}
              onClick={loadWebhooks}
              disabled={loading}
            >
              Refresh
            </Button>
            <Button
              startIcon={<AddIcon />}
              variant="contained"
              onClick={() => {
                setEditingWebhook(undefined);
                setFormDialogOpen(true);
              }}
            >
              New Webhook
            </Button>
          </Stack>
        </Box>

        {/* Error Alert */}
        {error && (
          <Alert severity="error" sx={{ mb: 2 }} onClose={clearError}>
            {typeof error === 'string' ? error : error.message}
          </Alert>
        )}

        {/* Loading */}
        {loading && webhooks.length === 0 ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Card>
            <CardContent>
              <Table>
                <TableHead>
                  <TableRow sx={{ bgcolor: 'action.hover' }}>
                    <TableCell>Name</TableCell>
                    <TableCell>URL</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Events</TableCell>
                    <TableCell>Success Rate</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {webhooks.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={6} sx={{ textAlign: 'center', py: 3 }}>
                        <EnhancedEmptyState
                          variant="no-data"
                          title="No webhooks found"
                          description="Create a new webhook to get started"
                        />
                      </TableCell>
                    </TableRow>
                  ) : (
                    webhooks.map((webhook) => {
                      const successRate = webhook.totalDeliveries > 0
                        ? Math.round((webhook.successfulDeliveries / webhook.totalDeliveries) * 100)
                        : '-';

                      return (
                        <TableRow key={webhook.id} hover>
                          <TableCell sx={{ fontWeight: 600 }}>{webhook.name}</TableCell>
                          <TableCell sx={{ fontSize: '0.875rem', fontFamily: 'monospace' }}>
                            {webhook.url.length > 40 ? `${webhook.url.slice(0, 40)}...` : webhook.url}
                          </TableCell>
                          <TableCell>
                            <Chip
                              label={getStatusLabel(webhook.status)}
                              color={getStatusColor(webhook.status)}
                              size="small"
                            />
                          </TableCell>
                          <TableCell>
                            <Typography variant="caption">
                              {webhook.events.length} events
                            </Typography>
                          </TableCell>
                          <TableCell>
                            {successRate !== '-' ? `${successRate}%` : 'N/A'}
                          </TableCell>
                          <TableCell align="right">
                            <Tooltip title="View">
                              <IconButton
                                size="small"
                                onClick={() => {
                                  setSelectedWebhook(webhook);
                                  setDetailTabValue(0);
                                  setDetailDialogOpen(true);
                                }}
                              >
                                <ViewIcon />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Test">
                              <IconButton
                                size="small"
                                onClick={() => handleTestWebhook(webhook)}
                              >
                                <TestIcon />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Edit">
                              <IconButton
                                size="small"
                                onClick={() => {
                                  setEditingWebhook(webhook);
                                  setFormDialogOpen(true);
                                }}
                              >
                                <EditIcon />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Delete">
                              <IconButton
                                size="small"
                                color="error"
                                onClick={() => handleDeleteWebhook(webhook.id)}
                              >
                                <DeleteIcon />
                              </IconButton>
                            </Tooltip>
                          </TableCell>
                        </TableRow>
                      );
                    })
                  )}
                </TableBody>
              </Table>
              <TablePagination
                rowsPerPageOptions={[10, 20, 50]}
                component="div"
                count={totalCount}
                rowsPerPage={pageSize}
                page={page}
                onPageChange={(_, newPage) => setPage(newPage)}
                onRowsPerPageChange={(e) => setPageSize(parseInt(e.target.value))}
              />
            </CardContent>
          </Card>
        )}
      </Container>

      {/* Form Dialog */}
      <Dialog open={formDialogOpen} onClose={() => setFormDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogHeader
          mode={editingWebhook ? 'edit' : 'create'}
          entityType="webhook"
          onClose={() => setFormDialogOpen(false)}
        />
        <DialogContent sx={{ pt: 2 }}>
          <WebhookForm
            webhook={editingWebhook}
            onSave={handleSaveWebhook}
            loading={loading}
          />
        </DialogContent>
      </Dialog>

      {/* Detail Dialog */}
      {selectedWebhook && (
        <Dialog
          open={detailDialogOpen}
          onClose={() => setDetailDialogOpen(false)}
          maxWidth="md"
          fullWidth
        >
          <DialogTitle>{selectedWebhook.name}</DialogTitle>
          <DialogContent sx={{ pt: 2 }}>
            <Tabs value={detailTabValue} onChange={(_, val) => setDetailTabValue(val)}>
              <Tab label="Details" />
              <Tab label="Delivery History" />
              <Tab label="Statistics" />
            </Tabs>

            {detailTabValue === 0 && (
              <Box sx={{ mt: 2 }}>
                <Stack spacing={2}>
                  <Box>
                    <Typography variant="caption" color="text.secondary">URL</Typography>
                    <Typography variant="body2" sx={{ fontFamily: 'monospace', wordBreak: 'break-all' }}>
                      {selectedWebhook.url}
                    </Typography>
                  </Box>
                  <Box>
                    <Typography variant="caption" color="text.secondary">Status</Typography>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={selectedWebhook.isActive}
                          onChange={() => handleToggleStatus(selectedWebhook)}
                        />
                      }
                      label={selectedWebhook.isActive ? 'Active' : 'Inactive'}
                    />
                  </Box>
                  <Box>
                    <Typography variant="caption" color="text.secondary">Events</Typography>
                    <Stack direction="row" spacing={1} sx={{ mt: 1, flexWrap: 'wrap' }}>
                      {selectedWebhook.events.map((event) => (
                        <Chip key={event} label={event} size="small" />
                      ))}
                    </Stack>
                  </Box>
                </Stack>
              </Box>
            )}

            {detailTabValue === 1 && (
              <Box sx={{ mt: 2 }}>
                <WebhookDeliveryHistoryTable
                  deliveries={deliveryHistory}
                  onRetry={async (id) => { await webhookService.retryDelivery(selectedWebhook.id, id); }}
                />
              </Box>
            )}

            {detailTabValue === 2 && (
              <Box sx={{ mt: 2 }}>
                <Stack spacing={1}>
                  <Box>
                    <Typography variant="body2" color="text.secondary">Total Deliveries</Typography>
                    <Typography variant="h6">{selectedWebhook.totalDeliveries}</Typography>
                  </Box>
                  <Box>
                    <Typography variant="body2" color="text.secondary">Successful</Typography>
                    <Typography variant="h6">{selectedWebhook.successfulDeliveries}</Typography>
                  </Box>
                  <Box>
                    <Typography variant="body2" color="text.secondary">Failed</Typography>
                    <Typography variant="h6">{selectedWebhook.failedDeliveries}</Typography>
                  </Box>
                </Stack>
              </Box>
            )}
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setDetailDialogOpen(false)}>Close</Button>
          </DialogActions>
        </Dialog>
      )}
    </Box>
  );
};

export default WebhooksManagementPage;
