/**
 * DeliveryDetailModal - Show detailed information about a single webhook delivery
 */

import React, { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Box,
  Chip,
  Divider,
  Tabs,
  Tab,
  Paper,
  Stack,
  IconButton,
} from '@mui/material';
import {
  Close as CloseIcon,
  CheckCircle as SuccessIcon,
  Cancel as FailIcon,
  ContentCopy as CopyIcon,
} from '@mui/icons-material';
import { type WebhookDelivery } from './DeliveryHistoryTable';

interface DeliveryDetailModalProps {
  open: boolean;
  onClose: () => void;
  delivery: WebhookDelivery | null;
}

interface TabPanelProps {
  children: React.ReactNode;
  value: number;
  index: number;
}

const TabPanel: React.FC<TabPanelProps> = ({ children, value, index }) => (
  <Box role="tabpanel" hidden={value !== index} sx={{ pt: 2 }}>
    {value === index && children}
  </Box>
);

const CodeBlock: React.FC<{ content: string; label?: string }> = ({ content, label }) => {
  const handleCopy = () => {
    navigator.clipboard.writeText(content).catch(() => {
      // Clipboard write failed, ignore silently
    });
  };

  return (
    <Box>
      {label && (
        <Stack direction="row" justifyContent="space-between" alignItems="center" mb={0.5}>
          <Typography variant="caption" color="text.secondary" fontWeight={600}>
            {label}
          </Typography>
          <IconButton size="small" onClick={handleCopy} title="Copy to clipboard">
            <CopyIcon fontSize="small" />
          </IconButton>
        </Stack>
      )}
      <Paper
        variant="outlined"
        sx={{
          p: 1.5,
          maxHeight: 300,
          overflow: 'auto',
          fontFamily: 'monospace',
          fontSize: '0.8rem',
          whiteSpace: 'pre-wrap',
          wordBreak: 'break-word',
          backgroundColor: 'grey.50',
          lineHeight: 1.6,
        }}
      >
        {content}
      </Paper>
    </Box>
  );
};

const formatDate = (dateStr: string): string => {
  try {
    return new Date(dateStr).toLocaleString();
  } catch {
    return dateStr;
  }
};

const formatJson = (value: string | undefined): string => {
  if (!value) return '(empty)';
  try {
    return JSON.stringify(JSON.parse(value), null, 2);
  } catch {
    return value;
  }
};

const DeliveryDetailModal: React.FC<DeliveryDetailModalProps> = ({
  open,
  onClose,
  delivery,
}) => {
  const [tabValue, setTabValue] = useState(0);

  if (!delivery) return null;

  const sampleRequestHeaders = JSON.stringify(
    {
      'Content-Type': 'application/json',
      'X-Webhook-Event': delivery.eventType,
      'X-Webhook-Delivery': delivery.id.toString(),
      'User-Agent': 'CRM-Webhook/1.0',
    },
    null,
    2
  );

  const sampleRequestBody = JSON.stringify(
    {
      event: delivery.eventType,
      deliveryId: delivery.id,
      timestamp: delivery.createdAt,
      data: {},
    },
    null,
    2
  );

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Stack direction="row" spacing={1} alignItems="center">
            <Typography variant="h6">Delivery Details</Typography>
            <Chip
              icon={delivery.success ? <SuccessIcon /> : <FailIcon />}
              label={delivery.success ? 'Success' : 'Failed'}
              color={delivery.success ? 'success' : 'error'}
              size="small"
            />
          </Stack>
          <IconButton onClick={onClose} size="small">
            <CloseIcon />
          </IconButton>
        </Stack>
      </DialogTitle>
      <DialogContent dividers>
        {/* Summary */}
        <Stack direction="row" spacing={3} mb={2} flexWrap="wrap">
          <Box>
            <Typography variant="caption" color="text.secondary">
              Event Type
            </Typography>
            <Typography variant="body2" fontFamily="monospace">
              {delivery.eventType}
            </Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary">
              Status Code
            </Typography>
            <Box>
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
              />
            </Box>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary">
              Response Time
            </Typography>
            <Typography variant="body2" fontWeight={600}>
              {delivery.responseTime}ms
            </Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary">
              Retry Count
            </Typography>
            <Typography variant="body2">{delivery.retryCount}</Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary">
              Created At
            </Typography>
            <Typography variant="body2">{formatDate(delivery.createdAt)}</Typography>
          </Box>
        </Stack>

        {delivery.error && (
          <Box mb={2}>
            <Typography variant="caption" color="error" fontWeight={600}>
              Error
            </Typography>
            <Paper
              variant="outlined"
              sx={{
                p: 1,
                backgroundColor: 'error.50',
                borderColor: 'error.light',
              }}
            >
              <Typography variant="body2" color="error">
                {delivery.error}
              </Typography>
            </Paper>
          </Box>
        )}

        <Divider sx={{ mb: 1 }} />

        <Tabs
          value={tabValue}
          onChange={(_e, v: number) => setTabValue(v)}
          variant="scrollable"
          scrollButtons="auto"
        >
          <Tab label="Request Headers" />
          <Tab label="Request Body" />
          <Tab label="Response" />
          <Tab label="Retry Timeline" />
        </Tabs>

        <TabPanel value={tabValue} index={0}>
          <CodeBlock content={sampleRequestHeaders} label="Request Headers" />
        </TabPanel>

        <TabPanel value={tabValue} index={1}>
          <CodeBlock content={sampleRequestBody} label="Request Body" />
        </TabPanel>

        <TabPanel value={tabValue} index={2}>
          <Stack spacing={2}>
            <CodeBlock
              content={JSON.stringify({ 'Content-Type': 'application/json' }, null, 2)}
              label="Response Headers"
            />
            <CodeBlock
              content={formatJson(delivery.error || undefined)}
              label="Response Body"
            />
          </Stack>
        </TabPanel>

        <TabPanel value={tabValue} index={3}>
          <Box>
            <Typography variant="subtitle2" gutterBottom>
              Delivery Attempts
            </Typography>
            {delivery.retryCount === 0 ? (
              <Typography variant="body2" color="text.secondary">
                No retry attempts — delivered on first try.
              </Typography>
            ) : (
              <Stack spacing={1}>
                {Array.from({ length: delivery.retryCount + 1 }, (_, i) => (
                  <Paper key={i} variant="outlined" sx={{ p: 1.5 }}>
                    <Stack direction="row" spacing={2} alignItems="center">
                      <Chip
                        label={i === 0 ? 'Initial' : `Retry #${i}`}
                        size="small"
                        color={
                          i === delivery.retryCount && delivery.success
                            ? 'success'
                            : i < delivery.retryCount
                            ? 'error'
                            : delivery.success
                            ? 'success'
                            : 'error'
                        }
                        variant="outlined"
                      />
                      <Typography variant="body2" color="text.secondary">
                        {i === delivery.retryCount
                          ? delivery.success
                            ? 'Delivered successfully'
                            : 'Failed — no more retries'
                          : `Failed — will retry (attempt ${i + 1})`}
                      </Typography>
                    </Stack>
                  </Paper>
                ))}
              </Stack>
            )}
          </Box>
        </TabPanel>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
};

export default DeliveryDetailModal;
