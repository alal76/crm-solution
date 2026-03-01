/**
 * EventFilterBuilder - Advanced webhook event filter builder
 * TODO-INT001-24: Build complex event filters for webhooks
 */

import React, { useState, useCallback, useMemo } from 'react';
import {
  Box,
  Paper,
  Typography,
  Button,
  IconButton,
  Chip,
  Stack,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  Tooltip,
  Divider,
  Collapse,
  Alert,
  useTheme,
  alpha,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  FilterList as FilterIcon,
  Save as SaveIcon,
  RestartAlt as ResetIcon,
} from '@mui/icons-material';
import { WebhookEvent } from '../../services/webhookService';

// --------------------------------------------------------------------------
// Types
// --------------------------------------------------------------------------

export type EventCategory = 'incident' | 'problem' | 'change' | 'sales' | 'account' | 'campaign' | 'custom';

export interface EventFilterCondition {
  id: string;
  field: string;
  operator: 'equals' | 'contains' | 'startsWith' | 'regex' | 'in';
  value: string;
}

export interface EventFilterGroup {
  id: string;
  logic: 'AND' | 'OR';
  events: WebhookEvent[];
  conditions: EventFilterCondition[];
}

export interface EventFilterConfig {
  groups: EventFilterGroup[];
  logic: 'AND' | 'OR';
}

export interface EventFilterBuilderProps {
  /** Current filter config */
  value?: EventFilterConfig;
  /** Called when filter changes */
  onChange: (config: EventFilterConfig) => void;
  /** Available events to choose from */
  availableEvents?: WebhookEvent[];
  /** Callback when Save is clicked */
  onSave?: (config: EventFilterConfig) => void;
  /** Show save button */
  showSave?: boolean;
  /** Compact mode */
  compact?: boolean;
}

// --------------------------------------------------------------------------
// Constants
// --------------------------------------------------------------------------

const EVENT_CATEGORIES: Record<EventCategory, { label: string; events: WebhookEvent[] }> = {
  incident: {
    label: 'Incidents',
    events: [
      WebhookEvent.IncidentCreated,
      WebhookEvent.IncidentUpdated,
      WebhookEvent.IncidentStatusChanged,
      WebhookEvent.IncidentClosed,
    ],
  },
  problem: {
    label: 'Problems',
    events: [WebhookEvent.ProblemCreated, WebhookEvent.ProblemUpdated, WebhookEvent.ProblemResolved],
  },
  change: {
    label: 'Changes',
    events: [
      WebhookEvent.ChangeCreated,
      WebhookEvent.ChangeApproved,
      WebhookEvent.ChangeRejected,
      WebhookEvent.ChangeImplemented,
    ],
  },
  sales: {
    label: 'Sales',
    events: [
      WebhookEvent.OpportunityCreated,
      WebhookEvent.OpportunityWon,
      WebhookEvent.OpportunityLost,
      WebhookEvent.OrderCreated,
      WebhookEvent.OrderFulfilled,
    ],
  },
  account: {
    label: 'Accounts & Contacts',
    events: [
      WebhookEvent.AccountCreated,
      WebhookEvent.AccountUpdated,
      WebhookEvent.ContactCreated,
      WebhookEvent.ContactUpdated,
    ],
  },
  campaign: {
    label: 'Campaigns',
    events: [WebhookEvent.CampaignStarted, WebhookEvent.CampaignCompleted],
  },
  custom: {
    label: 'Custom',
    events: [WebhookEvent.Custom],
  },
};

const CONDITION_FIELDS = [
  { value: 'payload.type', label: 'Payload Type' },
  { value: 'payload.priority', label: 'Priority' },
  { value: 'payload.status', label: 'Status' },
  { value: 'payload.assignee', label: 'Assignee' },
  { value: 'payload.source', label: 'Source' },
  { value: 'payload.tags', label: 'Tags' },
];

const CONDITION_OPERATORS: { value: EventFilterCondition['operator']; label: string }[] = [
  { value: 'equals', label: 'Equals' },
  { value: 'contains', label: 'Contains' },
  { value: 'startsWith', label: 'Starts with' },
  { value: 'regex', label: 'Regex' },
  { value: 'in', label: 'In (comma-separated)' },
];

const uid = () => `${Date.now()}-${Math.random().toString(36).slice(2, 9)}`; // NOSONAR - non-security use: UI element ID generation

const emptyConfig: EventFilterConfig = { groups: [], logic: 'AND' };

// --------------------------------------------------------------------------
// Component
// --------------------------------------------------------------------------

export const EventFilterBuilder: React.FC<EventFilterBuilderProps> = ({
  value = emptyConfig,
  onChange,
  availableEvents,
  onSave,
  showSave = true,
  compact = false,
}) => {
  const theme = useTheme();
  const config = value;

  // Resolve available events
  const allEvents = useMemo(
    () => availableEvents ?? Object.values(EVENT_CATEGORIES).flatMap((c) => c.events),
    [availableEvents],
  );

  // ---- Mutation helpers (immutable) --------------------------------------

  const updateConfig = useCallback(
    (updater: (prev: EventFilterConfig) => EventFilterConfig) => {
      onChange(updater(config));
    },
    [config, onChange],
  );

  const addGroup = useCallback(() => {
    updateConfig((prev) => ({
      ...prev,
      groups: [
        ...prev.groups,
        { id: uid(), logic: 'AND', events: [], conditions: [] },
      ],
    }));
  }, [updateConfig]);

  const removeGroup = useCallback(
    (groupId: string) => {
      updateConfig((prev) => ({
        ...prev,
        groups: prev.groups.filter((g) => g.id !== groupId),
      }));
    },
    [updateConfig],
  );

  const toggleGroupLogic = useCallback(() => {
    updateConfig((prev) => ({ ...prev, logic: prev.logic === 'AND' ? 'OR' : 'AND' }));
  }, [updateConfig]);

  const toggleEvent = useCallback(
    (groupId: string, event: WebhookEvent) => {
      updateConfig((prev) => ({
        ...prev,
        groups: prev.groups.map((g) => {
          if (g.id !== groupId) return g;
          const has = g.events.includes(event);
          return { ...g, events: has ? g.events.filter((e) => e !== event) : [...g.events, event] };
        }),
      }));
    },
    [updateConfig],
  );

  const addCondition = useCallback(
    (groupId: string) => {
      updateConfig((prev) => ({
        ...prev,
        groups: prev.groups.map((g) =>
          g.id !== groupId
            ? g
            : {
                ...g,
                conditions: [
                  ...g.conditions,
                  { id: uid(), field: CONDITION_FIELDS[0].value, operator: 'equals' as const, value: '' },
                ],
              },
        ),
      }));
    },
    [updateConfig],
  );

  const updateCondition = useCallback(
    (groupId: string, conditionId: string, updates: Partial<EventFilterCondition>) => {
      updateConfig((prev) => ({
        ...prev,
        groups: prev.groups.map((g) =>
          g.id !== groupId
            ? g
            : {
                ...g,
                conditions: g.conditions.map((c) =>
                  c.id !== conditionId ? c : { ...c, ...updates },
                ),
              },
        ),
      }));
    },
    [updateConfig],
  );

  const removeCondition = useCallback(
    (groupId: string, conditionId: string) => {
      updateConfig((prev) => ({
        ...prev,
        groups: prev.groups.map((g) =>
          g.id !== groupId
            ? g
            : { ...g, conditions: g.conditions.filter((c) => c.id !== conditionId) },
        ),
      }));
    },
    [updateConfig],
  );

  const handleReset = useCallback(() => onChange(emptyConfig), [onChange]);

  // ---- Render ------------------------------------------------------------

  return (
    <Paper sx={{ p: 2 }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
        <Stack direction="row" spacing={1} alignItems="center">
          <FilterIcon color="primary" />
          <Typography variant="subtitle1" fontWeight={600}>
            Event Filter Builder
          </Typography>
        </Stack>
        <Stack direction="row" spacing={1}>
          <Button size="small" startIcon={<ResetIcon />} onClick={handleReset}>
            Reset
          </Button>
          {showSave && onSave && (
            <Button
              size="small"
              variant="contained"
              startIcon={<SaveIcon />}
              onClick={() => onSave(config)}
            >
              Save
            </Button>
          )}
        </Stack>
      </Stack>

      <Divider sx={{ mb: 2 }} />

      {/* Top-level logic toggle */}
      {config.groups.length > 1 && (
        <Box sx={{ mb: 2 }}>
          <Chip
            label={`Groups joined by ${config.logic}`}
            onClick={toggleGroupLogic}
            color="primary"
            variant="outlined"
            size="small"
          />
        </Box>
      )}

      {/* Groups */}
      {config.groups.length === 0 ? (
        <Alert severity="info" sx={{ mb: 2 }}>
          No filter groups. Click "Add Group" to start building a filter.
        </Alert>
      ) : (
        config.groups.map((group, gi) => (
          <Paper
            key={group.id}
            variant="outlined"
            sx={{ p: 2, mb: 2, borderColor: alpha(theme.palette.primary.main, 0.3) }}
          >
            <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 1 }}>
              <Typography variant="subtitle2">Group {gi + 1}</Typography>
              <IconButton size="small" color="error" onClick={() => removeGroup(group.id)}>
                <DeleteIcon fontSize="small" />
              </IconButton>
            </Stack>

            {/* Event chips by category */}
            {Object.entries(EVENT_CATEGORIES).map(([catKey, cat]) => {
              const catEvents = cat.events.filter((e) => allEvents.includes(e));
              if (catEvents.length === 0) return null;
              return (
                <Box key={catKey} sx={{ mb: 1 }}>
                  <Typography variant="caption" color="text.secondary">
                    {cat.label}
                  </Typography>
                  <Stack direction="row" spacing={0.5} flexWrap="wrap" useFlexGap sx={{ mt: 0.5 }}>
                    {catEvents.map((ev) => {
                      const selected = group.events.includes(ev);
                      return (
                        <Chip
                          key={ev}
                          label={ev.split('.').pop()}
                          size="small"
                          color={selected ? 'primary' : 'default'}
                          variant={selected ? 'filled' : 'outlined'}
                          onClick={() => toggleEvent(group.id, ev)}
                        />
                      );
                    })}
                  </Stack>
                </Box>
              );
            })}

            <Divider sx={{ my: 1 }} />

            {/* Payload conditions */}
            <Typography variant="caption" color="text.secondary" sx={{ mb: 0.5, display: 'block' }}>
              Payload Conditions (optional)
            </Typography>
            {group.conditions.map((cond) => (
              <Stack key={cond.id} direction="row" spacing={1} alignItems="center" sx={{ mb: 1 }}>
                <FormControl size="small" sx={{ minWidth: 140 }}>
                  <Select
                    value={cond.field}
                    onChange={(e) => updateCondition(group.id, cond.id, { field: e.target.value })}
                  >
                    {CONDITION_FIELDS.map((f) => (
                      <MenuItem key={f.value} value={f.value}>
                        {f.label}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
                <FormControl size="small" sx={{ minWidth: 130 }}>
                  <Select
                    value={cond.operator}
                    onChange={(e) =>
                      updateCondition(group.id, cond.id, {
                        operator: e.target.value as EventFilterCondition['operator'],
                      })
                    }
                  >
                    {CONDITION_OPERATORS.map((op) => (
                      <MenuItem key={op.value} value={op.value}>
                        {op.label}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
                <TextField
                  size="small"
                  placeholder="Value"
                  value={cond.value}
                  onChange={(e) => updateCondition(group.id, cond.id, { value: e.target.value })}
                  sx={{ flex: 1 }}
                />
                <IconButton size="small" onClick={() => removeCondition(group.id, cond.id)}>
                  <DeleteIcon fontSize="small" />
                </IconButton>
              </Stack>
            ))}
            <Button size="small" startIcon={<AddIcon />} onClick={() => addCondition(group.id)}>
              Add Condition
            </Button>
          </Paper>
        ))
      )}

      <Button variant="outlined" startIcon={<AddIcon />} onClick={addGroup} fullWidth>
        Add Filter Group
      </Button>
    </Paper>
  );
};

export default EventFilterBuilder;
