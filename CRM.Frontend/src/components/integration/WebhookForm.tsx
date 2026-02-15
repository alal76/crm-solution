/**
 * WebhookForm - Create and edit webhooks
 */

import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Stack,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Chip,
  Grid,
  Alert,
  Typography,
  Paper,
} from '@mui/material';
import { WebhookEvent, Webhook } from '../../services/webhookService';

interface WebhookFormProps {
  webhook?: Webhook;
  onSave: (webhook: Omit<Webhook, 'id' | 'createdAt' | 'updatedAt' | 'lastDeliveryAt' | 'lastDeliveryStatus' | 'totalDeliveries' | 'successfulDeliveries' | 'failedDeliveries'>) => Promise<void>;
  loading?: boolean;
}

export const WebhookForm: React.FC<WebhookFormProps> = ({
  webhook,
  onSave,
  loading = false,
}) => {
  const [formData, setFormData] = useState({
    name: webhook?.name || '',
    description: webhook?.description || '',
    url: webhook?.url || '',
    events: webhook?.events || [],
    secret: webhook?.secret || '',
    isActive: webhook?.isActive !== false,
  });

  const [selectedEvent, setSelectedEvent] = useState<WebhookEvent | ''>('');
  const [customHeader, setCustomHeader] = useState<{ key: string; value: string }>({ key: '', value: '' });
  const [headers, setHeaders] = useState<Record<string, string>>(webhook?.headers || {});

  const allEvents: WebhookEvent[] = [
    WebhookEvent.IncidentCreated,
    WebhookEvent.IncidentUpdated,
    WebhookEvent.ProblemCreated,
    WebhookEvent.ChangeCreated,
    WebhookEvent.OpportunityCreated,
    WebhookEvent.OrderCreated,
    WebhookEvent.AccountCreated,
  ];

  const handleAddEvent = (event: WebhookEvent) => {
    if (!formData.events.includes(event)) {
      setFormData({
        ...formData,
        events: [...formData.events, event],
      });
    }
  };

  const handleRemoveEvent = (event: WebhookEvent) => {
    setFormData({
      ...formData,
      events: formData.events.filter((e) => e !== event),
    });
  };

  const handleAddHeader = () => {
    if (customHeader.key && customHeader.value) {
      setHeaders({
        ...headers,
        [customHeader.key]: customHeader.value,
      });
      setCustomHeader({ key: '', value: '' });
    }
  };

  const handleRemoveHeader = (key: string) => {
    const updated = { ...headers };
    delete updated[key];
    setHeaders(updated);
  };

  const handleSave = async () => {
    await onSave({
      ...formData,
      name: formData.name,
      description: formData.description || undefined,
      url: formData.url,
      events: formData.events,
      status: formData.isActive ? 0 : 1,
      secret: formData.secret || undefined,
      headers: Object.keys(headers).length > 0 ? headers : undefined,
      isActive: formData.isActive,
    } as any);
  };

  return (
    <Box>
      <Card>
        <CardContent>
          <Stack spacing={2}>
            <TextField
              fullWidth
              label="Webhook Name"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              disabled={loading}
              required
            />

            <TextField
              fullWidth
              label="Description"
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              multiline
              rows={2}
              disabled={loading}
            />

            <TextField
              fullWidth
              label="Webhook URL"
              value={formData.url}
              onChange={(e) => setFormData({ ...formData, url: e.target.value })}
              placeholder="https://example.com/webhook"
              disabled={loading}
              required
            />

            <TextField
              fullWidth
              label="Secret (for signature verification)"
              value={formData.secret}
              onChange={(e) => setFormData({ ...formData, secret: e.target.value })}
              type="password"
              disabled={loading}
            />
          </Stack>
        </CardContent>
      </Card>

      {/* Events Selection */}
      <Box sx={{ mt: 3 }}>
        <Typography variant="h6" sx={{ mb: 2, fontWeight: 'bold' }}>
          Events to Subscribe
        </Typography>
        <Grid container spacing={2}>
          {allEvents.map((event) => (
            <Grid item xs={12} sm={6} key={event}>
              <Paper
                sx={{
                  p: 2,
                  cursor: 'pointer',
                  bgcolor: formData.events.includes(event) ? 'action.selected' : 'background.paper',
                  border: formData.events.includes(event) ? '2px solid' : '1px solid',
                  borderColor: formData.events.includes(event) ? 'primary.main' : 'divider',
                }}
                onClick={() => {
                  if (formData.events.includes(event)) {
                    handleRemoveEvent(event);
                  } else {
                    handleAddEvent(event);
                  }
                }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <input
                    type="checkbox"
                    checked={formData.events.includes(event)}
                    onChange={() => {}}
                  />
                  <Typography variant="body2">{event}</Typography>
                </Box>
              </Paper>
            </Grid>
          ))}
        </Grid>
      </Box>

      {/* Custom Headers */}
      <Box sx={{ mt: 3 }}>
        <Typography variant="h6" sx={{ mb: 2, fontWeight: 'bold' }}>
          Custom Headers (Optional)
        </Typography>
        {Object.entries(headers).map(([key, value]) => (
          <Chip
            key={key}
            label={`${key}: ${value}`}
            onDelete={() => handleRemoveHeader(key)}
            sx={{ mr: 1, mb: 1 }}
          />
        ))}
        <Box sx={{ display: 'flex', gap: 1, mt: 1 }}>
          <TextField
            size="small"
            placeholder="Header name"
            value={customHeader.key}
            onChange={(e) => setCustomHeader({ ...customHeader, key: e.target.value })}
            disabled={loading}
          />
          <TextField
            size="small"
            placeholder="Header value"
            value={customHeader.value}
            onChange={(e) => setCustomHeader({ ...customHeader, value: e.target.value })}
            disabled={loading}
          />
          <Button
            onClick={handleAddHeader}
            disabled={!customHeader.key || !customHeader.value || loading}
          >
            Add
          </Button>
        </Box>
      </Box>

      {/* Save Button */}
      <Button
        onClick={handleSave}
        variant="contained"
        fullWidth
        sx={{ mt: 3 }}
        disabled={loading || !formData.name || !formData.url || formData.events.length === 0}
      >
        Save Webhook
      </Button>
    </Box>
  );
};

export default WebhookForm;
