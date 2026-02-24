/**
 * WebhookList - Displays a list of webhooks with actions
 * Implements TODO-INT001-21
 */

import React from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Chip,
  Box,
  Tooltip,
  Switch,
  Typography,
  CircularProgress,
} from '@mui/material';
import {
  Edit as EditIcon,
  Delete as DeleteIcon,
  Visibility as ViewIcon,
  PlayArrow as TestIcon,
} from '@mui/icons-material';
import { Webhook, WebhookStatus, DeliveryStatus } from '../../services/webhookService';

interface WebhookListProps {
  webhooks: Webhook[];
  onEdit?: (webhook: Webhook) => void;
  onDelete?: (webhook: Webhook) => void;
  onToggle?: (webhook: Webhook) => void;
  onView?: (webhook: Webhook) => void;
  onTest?: (webhook: Webhook) => void;
  loading?: boolean;
  emptyMessage?: string;
}

export const WebhookList: React.FC<WebhookListProps> = ({
  webhooks,
  onEdit,
  onDelete,
  onToggle,
  onView,
  onTest,
  loading = false,
  emptyMessage = 'No webhooks configured',
}) => {
  const getStatusColor = (status: WebhookStatus): 'success' | 'warning' | 'error' | 'default' => {
    switch (status) {
      case WebhookStatus.Active:
        return 'success';
      case WebhookStatus.Inactive:
        return 'default';
      case WebhookStatus.Paused:
        return 'warning';
      case WebhookStatus.Disabled:
        return 'error';
      default:
        return 'default';
    }
  };

  const getStatusLabel = (status: WebhookStatus): string => {
    switch (status) {
      case WebhookStatus.Active:
        return 'Active';
      case WebhookStatus.Inactive:
        return 'Inactive';
      case WebhookStatus.Paused:
        return 'Paused';
      case WebhookStatus.Disabled:
        return 'Disabled';
      default:
        return 'Unknown';
    }
  };

  const getLastDeliveryColor = (status?: DeliveryStatus): 'success' | 'error' | 'warning' | 'default' => {
    if (status === undefined) return 'default';
    switch (status) {
      case DeliveryStatus.Delivered:
        return 'success';
      case DeliveryStatus.Failed:
        return 'error';
      case DeliveryStatus.Retrying:
        return 'warning';
      case DeliveryStatus.Pending:
      default:
        return 'default';
    }
  };

  const getDeliveryStatusLabel = (status?: DeliveryStatus): string => {
    if (status === undefined) return 'Never';
    switch (status) {
      case DeliveryStatus.Delivered:
        return 'Delivered';
      case DeliveryStatus.Failed:
        return 'Failed';
      case DeliveryStatus.Retrying:
        return 'Retrying';
      case DeliveryStatus.Pending:
        return 'Pending';
      default:
        return 'Unknown';
    }
  };

  const formatDate = (dateString?: string): string => {
    if (!dateString) return 'Never';
    try {
      return new Date(dateString).toLocaleString();
    } catch {
      return 'Invalid date';
    }
  };

  const calculateSuccessRate = (webhook: Webhook): string => {
    const total = webhook.totalDeliveries;
    if (total === 0) return 'N/A';
    const rate = (webhook.successfulDeliveries / total) * 100;
    return `${rate.toFixed(1)}%`;
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" py={4}>
        <CircularProgress />
      </Box>
    );
  }

  if (webhooks.length === 0) {
    return (
      <Paper elevation={0} sx={{ p: 4, textAlign: 'center' }}>
        <Typography color="text.secondary">{emptyMessage}</Typography>
      </Paper>
    );
  }

  return (
    <TableContainer component={Paper}>
      <Table aria-label="webhooks list">
        <TableHead>
          <TableRow>
            <TableCell>Name</TableCell>
            <TableCell>URL</TableCell>
            <TableCell>Events</TableCell>
            <TableCell align="center">Status</TableCell>
            <TableCell align="center">Enabled</TableCell>
            <TableCell>Last Delivery</TableCell>
            <TableCell align="center">Success Rate</TableCell>
            <TableCell align="right">Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {webhooks.map((webhook) => (
            <TableRow key={webhook.id} hover>
              <TableCell>
                <Typography variant="body2" fontWeight="medium">
                  {webhook.name}
                </Typography>
                {webhook.description && (
                  <Typography variant="caption" color="text.secondary" display="block">
                    {webhook.description}
                  </Typography>
                )}
              </TableCell>
              <TableCell>
                <Tooltip title={webhook.url}>
                  <Typography
                    variant="body2"
                    sx={{
                      maxWidth: 200,
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      whiteSpace: 'nowrap',
                    }}
                  >
                    {webhook.url}
                  </Typography>
                </Tooltip>
              </TableCell>
              <TableCell>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  {webhook.events.slice(0, 2).map((event) => (
                    <Chip
                      key={event}
                      label={event.split('.').pop()}
                      size="small"
                      variant="outlined"
                    />
                  ))}
                  {webhook.events.length > 2 && (
                    <Tooltip title={webhook.events.slice(2).join(', ')}>
                      <Chip
                        label={`+${webhook.events.length - 2}`}
                        size="small"
                        variant="outlined"
                      />
                    </Tooltip>
                  )}
                </Box>
              </TableCell>
              <TableCell align="center">
                <Chip
                  label={getStatusLabel(webhook.status)}
                  color={getStatusColor(webhook.status)}
                  size="small"
                />
              </TableCell>
              <TableCell align="center">
                <Switch
                  checked={webhook.isActive}
                  onChange={() => onToggle?.(webhook)}
                  size="small"
                  disabled={!onToggle}
                />
              </TableCell>
              <TableCell>
                <Box>
                  <Chip
                    label={getDeliveryStatusLabel(webhook.lastDeliveryStatus)}
                    color={getLastDeliveryColor(webhook.lastDeliveryStatus)}
                    size="small"
                    sx={{ mb: 0.5 }}
                  />
                  <Typography variant="caption" display="block" color="text.secondary">
                    {formatDate(webhook.lastDeliveryAt)}
                  </Typography>
                </Box>
              </TableCell>
              <TableCell align="center">
                <Typography
                  variant="body2"
                  color={
                    webhook.totalDeliveries > 0
                      ? webhook.successfulDeliveries === webhook.totalDeliveries
                        ? 'success.main'
                        : webhook.successfulDeliveries === 0
                        ? 'error.main'
                        : 'warning.main'
                      : 'text.secondary'
                  }
                >
                  {calculateSuccessRate(webhook)}
                </Typography>
                <Typography variant="caption" color="text.secondary" display="block">
                  {webhook.successfulDeliveries}/{webhook.totalDeliveries}
                </Typography>
              </TableCell>
              <TableCell align="right">
                <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 0.5 }}>
                  {onView && (
                    <Tooltip title="View Details">
                      <IconButton size="small" onClick={() => onView(webhook)}>
                        <ViewIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  )}
                  {onTest && (
                    <Tooltip title="Test Webhook">
                      <IconButton size="small" onClick={() => onTest(webhook)}>
                        <TestIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  )}
                  {onEdit && (
                    <Tooltip title="Edit">
                      <IconButton size="small" onClick={() => onEdit(webhook)}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  )}
                  {onDelete && (
                    <Tooltip title="Delete">
                      <IconButton
                        size="small"
                        color="error"
                        onClick={() => onDelete(webhook)}
                      >
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  )}
                </Box>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
};

export default WebhookList;
