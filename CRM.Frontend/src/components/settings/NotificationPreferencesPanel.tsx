/**
 * NotificationPreferencesPanel - Let users toggle per-entity-type notification channels.
 * Implements TODO-PORTAL-07.
 *
 * Groups preferences by EntityType, shows a toggle per (EventType × Channel) combination,
 * and bulk-saves with PUT /api/users/{userId}/notification-preferences.
 */

import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Typography,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Button,
  CircularProgress,
  Alert,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Chip,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import NotificationsIcon from '@mui/icons-material/Notifications';
import apiClient from '../../services/apiClient';
import { useAuth } from '../../contexts/AuthContext';

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

/** Mirrors the backend NotificationChannel enum */
export enum NotificationChannel {
  InApp = 0,
  Email = 1,
  Push = 2,
  Sms = 3,
}

const CHANNEL_LABELS: Record<NotificationChannel, string> = {
  [NotificationChannel.InApp]: 'In-App',
  [NotificationChannel.Email]: 'Email',
  [NotificationChannel.Push]: 'Push',
  [NotificationChannel.Sms]: 'SMS',
};

interface NotificationPreference {
  id?: number;
  userId: number;
  entityType: string;
  eventType: string;
  channel: NotificationChannel;
  isEnabled: boolean;
}

// Preferences grouped by entityType → eventType → channel
type PreferenceMap = Record<string, Record<string, Record<NotificationChannel, boolean>>>;

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function buildMap(prefs: NotificationPreference[]): PreferenceMap {
  const map: PreferenceMap = {};
  for (const p of prefs) {
    map[p.entityType] ??= {};
    map[p.entityType][p.eventType] ??= {} as Record<NotificationChannel, boolean>;
    map[p.entityType][p.eventType][p.channel] = p.isEnabled;
  }
  return map;
}

function flattenMap(userId: number, map: PreferenceMap): NotificationPreference[] {
  const result: NotificationPreference[] = [];
  for (const [entityType, events] of Object.entries(map)) {
    for (const [eventType, channels] of Object.entries(events)) {
      for (const [channel, isEnabled] of Object.entries(channels)) {
        result.push({ userId, entityType, eventType, channel: Number(channel), isEnabled });
      }
    }
  }
  return result;
}

// ---------------------------------------------------------------------------
// Component
// ---------------------------------------------------------------------------

const NotificationPreferencesPanel: React.FC = () => {
  const { user } = useAuth();
  const [prefMap, setPrefMap] = useState<PreferenceMap>({});
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // ---------------------------------------------------------------------------
  // Load preferences
  // ---------------------------------------------------------------------------

  const loadPreferences = useCallback(async () => {
    if (!user?.id) return;
    setLoading(true);
    setError(null);
    try {
      const res = await apiClient.get<NotificationPreference[]>(
        `/users/${user.id}/notification-preferences`,
      );
      setPrefMap(buildMap(res.data ?? []));
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr?.response?.data?.message ?? 'Failed to load notification preferences');
    } finally {
      setLoading(false);
    }
  }, [user?.id]);

  useEffect(() => {
    loadPreferences();
  }, [loadPreferences]);

  // ---------------------------------------------------------------------------
  // Toggle a single preference
  // ---------------------------------------------------------------------------

  const handleToggle = (entityType: string, eventType: string, channel: NotificationChannel) => {
    setPrefMap((prev) => {
      const updated = { ...prev };
      updated[entityType] = { ...updated[entityType] };
      updated[entityType][eventType] = { ...updated[entityType][eventType] };
      updated[entityType][eventType][channel] = !updated[entityType][eventType][channel];
      return updated;
    });
  };

  // ---------------------------------------------------------------------------
  // Save all preferences
  // ---------------------------------------------------------------------------

  const handleSave = async () => {
    if (!user?.id) return;
    setSaving(true);
    setError(null);
    setSuccess(null);
    try {
      const payload = flattenMap(user.id, prefMap);
      await apiClient.put(`/users/${user.id}/notification-preferences`, payload);
      setSuccess('Notification preferences saved successfully.');
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr?.response?.data?.message ?? 'Failed to save preferences');
    } finally {
      setSaving(false);
    }
  };

  // ---------------------------------------------------------------------------
  // Render
  // ---------------------------------------------------------------------------

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  const entityTypes = Object.keys(prefMap);

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
        <NotificationsIcon color="primary" />
        <Typography variant="h6">Notification Preferences</Typography>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}
      {success && (
        <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccess(null)}>
          {success}
        </Alert>
      )}

      {entityTypes.length === 0 ? (
        <Typography color="text.secondary">No notification preferences configured.</Typography>
      ) : (
        entityTypes.map((entityType) => {
          const events = prefMap[entityType];
          const eventKeys = Object.keys(events);
          const channels = Object.values(NotificationChannel).filter(
            (v) => typeof v === 'number',
          ) as NotificationChannel[];

          return (
            <Accordion key={entityType} defaultExpanded={entityTypes.length <= 3}>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <Typography fontWeight={600}>{entityType}</Typography>
                  <Chip label={`${eventKeys.length} events`} size="small" variant="outlined" />
                </Box>
              </AccordionSummary>
              <AccordionDetails>
                <TableContainer component={Paper} variant="outlined">
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell><strong>Event</strong></TableCell>
                        {channels.map((ch) => (
                          <TableCell key={ch} align="center">
                            <strong>{CHANNEL_LABELS[ch]}</strong>
                          </TableCell>
                        ))}
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {eventKeys.map((eventType) => (
                        <TableRow key={eventType} hover>
                          <TableCell>{eventType}</TableCell>
                          {channels.map((ch) => (
                            <TableCell key={ch} align="center" padding="checkbox">
                              <Switch
                                size="small"
                                checked={events[eventType]?.[ch] ?? false}
                                onChange={() => handleToggle(entityType, eventType, ch)}
                                inputProps={{ 'aria-label': `${eventType} ${CHANNEL_LABELS[ch]}` }}
                              />
                            </TableCell>
                          ))}
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              </AccordionDetails>
            </Accordion>
          );
        })
      )}

      <Box sx={{ mt: 2, display: 'flex', justifyContent: 'flex-end' }}>
        <Button
          variant="contained"
          onClick={handleSave}
          disabled={saving}
          startIcon={saving ? <CircularProgress size={16} /> : undefined}
        >
          {saving ? 'Saving…' : 'Save Preferences'}
        </Button>
      </Box>
    </Box>
  );
};

export default NotificationPreferencesPanel;
