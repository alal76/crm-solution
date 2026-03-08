/**
 * WebhookTestSender - Send test webhook payloads and view results
 */

import React, { useState, useCallback } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Chip,
  CircularProgress,
  Divider,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
  Alert,
  Paper,
  Collapse,
} from '@mui/material';
import {
  Send as SendIcon,
  CheckCircle as SuccessIcon,
  Error as ErrorIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
} from '@mui/icons-material';
import webhookService, { WebhookEvent } from '../../services/webhookService';

interface TestResult {
  success: boolean;
  statusCode: number;
  responseTime: number;
  responseBody?: string;
  error?: string;
}

interface WebhookTestSenderProps {
  webhookId: number;
  webhookUrl: string;
  onTestComplete?: (result: TestResult) => void;
}

const DEFAULT_PAYLOAD = JSON.stringify(
  {
    event: 'account.created',
    timestamp: new Date().toISOString(),
    data: {
      id: 1,
      name: 'Test Account',
      email: 'test@example.com',
    },
  },
  null,
  2
);

const EVENT_OPTIONS: { value: WebhookEvent; label: string }[] = [
  { value: WebhookEvent.AccountCreated, label: 'Account Created' },
  { value: WebhookEvent.AccountUpdated, label: 'Account Updated' },
  { value: WebhookEvent.ContactCreated, label: 'Contact Created' },
  { value: WebhookEvent.ContactUpdated, label: 'Contact Updated' },
  { value: WebhookEvent.OpportunityCreated, label: 'Opportunity Created' },
  { value: WebhookEvent.OpportunityWon, label: 'Opportunity Won' },
  { value: WebhookEvent.OrderCreated, label: 'Order Created' },
  { value: WebhookEvent.IncidentCreated, label: 'Ticket Created' },
  { value: WebhookEvent.IncidentUpdated, label: 'Ticket Updated' },
  { value: WebhookEvent.Custom, label: 'Custom Event' },
];

const WebhookTestSender: React.FC<WebhookTestSenderProps> = ({
  webhookId,
  webhookUrl,
  onTestComplete,
}) => {
  const [selectedEvent, setSelectedEvent] = useState<WebhookEvent>(
    WebhookEvent.AccountCreated
  );
  const [payload, setPayload] = useState<string>(DEFAULT_PAYLOAD);
  const [sending, setSending] = useState(false);
  const [testResult, setTestResult] = useState<TestResult | null>(null);
  const [payloadError, setPayloadError] = useState<string>('');
  const [showResponse, setShowResponse] = useState(false);

  const validatePayload = useCallback((value: string): boolean => {
    try {
      JSON.parse(value);
      setPayloadError('');
      return true;
    } catch {
      setPayloadError('Invalid JSON payload');
      return false;
    }
  }, []);

  const handleSendTest = useCallback(async () => {
    if (!validatePayload(payload)) return;

    setSending(true);
    setTestResult(null);
    const startTime = Date.now();

    try {
      let parsedPayload: Record<string, unknown>;
      try {
        parsedPayload = JSON.parse(payload) as Record<string, unknown>;
      } catch {
        parsedPayload = {};
      }

      const response = await webhookService.testWebhook(webhookId, {
        event: selectedEvent,
        payload: parsedPayload,
      });

      const result: TestResult = {
        success: response.success,
        statusCode: response.statusCode ?? 0,
        responseTime: response.deliveryTime || (Date.now() - startTime),
        responseBody: response.responseBody,
        error: response.errorMessage,
      };

      setTestResult(result);
      onTestComplete?.(result);
    } catch (err: unknown) {
      const errorMessage =
        err instanceof Error ? (err as Error).message : 'Failed to send test webhook';
      const result: TestResult = {
        success: false,
        statusCode: 0,
        responseTime: Date.now() - startTime,
        error: errorMessage,
      };
      setTestResult(result);
      onTestComplete?.(result);
    } finally {
      setSending(false);
    }
  }, [webhookId, selectedEvent, payload, onTestComplete, validatePayload]);

  const getStatusChip = (statusCode: number) => {
    if (statusCode >= 200 && statusCode < 300) {
      return <Chip icon={<SuccessIcon />} label={statusCode} color="success" size="small" />;
    }
    if (statusCode >= 400) {
      return <Chip icon={<ErrorIcon />} label={statusCode} color="error" size="small" />;
    }
    if (statusCode > 0) {
      return <Chip label={statusCode} color="warning" size="small" />;
    }
    return <Chip label="N/A" color="default" size="small" />;
  };

  return (
    <Card variant="outlined">
      <CardHeader title="Test Webhook" subheader={`Target: ${webhookUrl}`} />
      <CardContent>
        <Stack spacing={2}>
          <FormControl fullWidth size="small">
            <InputLabel>Event Type</InputLabel>
            <Select
              value={selectedEvent}
              label="Event Type"
              onChange={(e) => setSelectedEvent(e.target.value as WebhookEvent)}
            >
              {EVENT_OPTIONS.map((opt) => (
                <MenuItem key={opt.value} value={opt.value}>
                  {opt.label}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <TextField
            label="Payload (JSON)"
            multiline
            rows={8}
            value={payload}
            onChange={(e) => {
              setPayload(e.target.value);
              if (payloadError) validatePayload(e.target.value);
            }}
            error={!!payloadError}
            helperText={payloadError}
            InputProps={{
              sx: { fontFamily: 'monospace', fontSize: '0.85rem' },
            }}
          />

          <Button
            variant="contained"
            startIcon={sending ? <CircularProgress size={18} /> : <SendIcon />}
            onClick={handleSendTest}
            disabled={sending || !!payloadError}
          >
            {sending ? 'Sending...' : 'Send Test'}
          </Button>

          {testResult && (
            <>
              <Divider />
              <Alert severity={testResult.success ? 'success' : 'error'}>
                {testResult.success
                  ? 'Test webhook delivered successfully!'
                  : `Test failed: ${testResult.error || 'Unknown error'}`}
              </Alert>

              <Stack direction="row" spacing={2} alignItems="center">
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    Status Code
                  </Typography>
                  <Box>{getStatusChip(testResult.statusCode)}</Box>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    Response Time
                  </Typography>
                  <Typography variant="body2" fontWeight={600}>
                    {testResult.responseTime}ms
                  </Typography>
                </Box>
              </Stack>

              {testResult.responseBody && (
                <Box>
                  <Button
                    size="small"
                    onClick={() => setShowResponse(!showResponse)}
                    endIcon={showResponse ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                  >
                    Response Body
                  </Button>
                  <Collapse in={showResponse}>
                    <Paper
                      variant="outlined"
                      sx={{
                        p: 1.5,
                        mt: 1,
                        maxHeight: 200,
                        overflow: 'auto',
                        fontFamily: 'monospace',
                        fontSize: '0.8rem',
                        whiteSpace: 'pre-wrap',
                        wordBreak: 'break-word',
                        backgroundColor: 'grey.50',
                      }}
                    >
                      {testResult.responseBody}
                    </Paper>
                  </Collapse>
                </Box>
              )}
            </>
          )}
        </Stack>
      </CardContent>
    </Card>
  );
};

export default WebhookTestSender;
