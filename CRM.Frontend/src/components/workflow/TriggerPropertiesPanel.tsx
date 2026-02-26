/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Trigger Properties Panel - Configuration panel for workflow trigger nodes
 * Allows selection of trigger events, watched fields, and conditions
 */

import React, { useState, useEffect, useMemo } from 'react';
import {
  Box,
  Typography,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Switch,
  FormControlLabel,
  Button,
  IconButton,
  Divider,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Chip,
  Tooltip,
  Alert,
  Autocomplete,
  Paper,
  Tabs,
  Tab,
  Card,
  CardContent,
  CardHeader,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  ListItemSecondaryAction,
  CircularProgress,
} from '@mui/material';
import {
  ExpandMore as ExpandIcon,
  Delete as DeleteIcon,
  Add as AddIcon,
  PlayCircle as TriggerIcon,
  Schedule as ScheduleIcon,
  FlashOn as EventIcon,
  Edit as FieldIcon,
  Webhook as WebhookIcon,
  Help as HelpIcon,
  Code as CodeIcon,
} from '@mui/icons-material';
import {
  workflowService,
  WorkflowConfig,
  EntityFieldConfig,
  RelatedEntityConfig,
} from '../../services/workflowService';

// ============================================================================
// Types
// ============================================================================

export interface TriggerConfiguration {
  triggerType: 'Manual' | 'OnCreate' | 'OnUpdate' | 'OnDelete' | 'OnFieldChange' | 'Scheduled' | 'OnEvent' | 'OnWebhook' | 'OnSLABreach' | 'OnEscalation' | 'OnStatusChange' | 'OnApproval' | 'OnRejection' | 'OnAssignment';
  
  // Field Change Triggers
  watchedFields?: string[];
  fieldConditions?: FieldCondition[];
  
  // Scheduled Triggers
  scheduleType?: 'cron' | 'interval';
  schedulePreset?: 'interval' | 'hourly' | 'daily' | 'weekly' | 'monthly' | 'custom_cron';
  cronExpression?: string;
  intervalMinutes?: number;
  scheduleHour?: number;
  scheduleMinute?: number;
  scheduleDaysOfWeek?: number[];
  scheduleDayOfMonth?: number;
  timeZone?: string;
  
  // Webhook Triggers
  webhookSecret?: string;
  webhookPayloadSchema?: string;
  
  // Common settings
  runOnce?: boolean;
  batchMode?: boolean;
  batchSize?: number;
  filterExpression?: string;
}

interface FieldCondition {
  field: string;
  operator: 'equals' | 'notEquals' | 'contains' | 'notContains' | 'startsWith' | 'endsWith' | 'greaterThan' | 'lessThan' | 'greaterThanOrEqual' | 'lessThanOrEqual' | 'isNull' | 'isNotNull' | 'in' | 'notIn' | 'between' | 'regex' | 'changed_to' | 'changed_from';
  value?: string;
  oldValue?: string;
}

interface TriggerPropertiesPanelProps {
  nodeId: number;
  nodeKey: string;
  nodeName: string;
  configuration: string;
  entityType: string;
  onChange: (property: string, value: any) => void;
  onDelete: () => void;
  readonly?: boolean;
}

// ============================================================================
// Tab Panel Component
// ============================================================================

interface TabPanelProps {
  children?: React.ReactNode;
  value: number;
  index: number;
}

const TabPanel: React.FC<TabPanelProps> = ({ children, value, index }) => (
  <div role="tabpanel" hidden={value !== index}>
    {value === index && <Box sx={{ pt: 2 }}>{children}</Box>}
  </div>
);

// ============================================================================
// Schedule helpers
// ============================================================================

const COMMON_TIMEZONES = [
  'UTC',
  'America/New_York', 'America/Chicago', 'America/Denver', 'America/Los_Angeles',
  'America/Toronto', 'America/Vancouver', 'America/Mexico_City', 'America/Sao_Paulo',
  'Europe/London', 'Europe/Paris', 'Europe/Berlin', 'Europe/Amsterdam',
  'Europe/Moscow', 'Africa/Cairo', 'Asia/Dubai', 'Asia/Kolkata',
  'Asia/Singapore', 'Asia/Shanghai', 'Asia/Tokyo', 'Australia/Sydney', 'Pacific/Auckland',
];

const buildCronFromConfig = (cfg: TriggerConfiguration): string => {
  const h = cfg.scheduleHour ?? 9;
  const m = cfg.scheduleMinute ?? 0;
  const dow = (cfg.scheduleDaysOfWeek ?? [1]).slice().sort((a, b) => a - b).join(',');
  const dom = cfg.scheduleDayOfMonth ?? 1;
  switch (cfg.schedulePreset) {
    case 'hourly': return `${m} * * * *`;
    case 'daily': return `${m} ${h} * * *`;
    case 'weekly': return `${m} ${h} * * ${dow}`;
    case 'monthly': return `${m} ${h} ${dom} * *`;
    case 'custom_cron': return cfg.cronExpression || '0 9 * * *';
    default: return '';
  }
};

const describeCronConfig = (cfg: TriggerConfiguration): string => {
  const h = cfg.scheduleHour ?? 9;
  const m = cfg.scheduleMinute ?? 0;
  const timeStr = `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}`;
  const dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
  const tz = cfg.timeZone || 'UTC';
  switch (cfg.schedulePreset) {
    case 'interval': return `Runs every ${cfg.intervalMinutes || 60} minute(s)`;
    case 'hourly': return `Runs every hour at :${String(m).padStart(2, '0')} (${tz})`;
    case 'daily': return `Runs every day at ${timeStr} (${tz})`;
    case 'weekly': {
      const days = (cfg.scheduleDaysOfWeek ?? [1]).slice().sort((a, b) => a - b).map(d => dayNames[d]).join(', ');
      return `Runs every week on ${days} at ${timeStr} (${tz})`;
    }
    case 'monthly': return `Runs on day ${cfg.scheduleDayOfMonth ?? 1} of every month at ${timeStr} (${tz})`;
    case 'custom_cron': return `Custom schedule: ${cfg.cronExpression || '—'}`;
    default: return 'Choose a schedule pattern above';
  }
};

// ============================================================================
// Main Component
// ============================================================================

export const TriggerPropertiesPanel: React.FC<TriggerPropertiesPanelProps> = ({
  nodeId,
  nodeKey,
  nodeName,
  configuration,
  entityType,
  onChange,
  onDelete,
  readonly = false,
}) => {
  const [tabValue, setTabValue] = useState(0);
  const [config, setConfig] = useState<TriggerConfiguration>({ triggerType: 'OnCreate' });
  
  // Backend-driven configuration state
  const [workflowConfig, setWorkflowConfig] = useState<WorkflowConfig | null>(null);
  const [configLoading, setConfigLoading] = useState(true);

  // Load configuration from backend on mount
  useEffect(() => {
    let mounted = true;
    workflowService.getConfig().then(cfg => {
      if (mounted) {
        setWorkflowConfig(cfg);
        setConfigLoading(false);
      }
    }).catch(() => {
      if (mounted) setConfigLoading(false);
    });
    return () => { mounted = false; };
  }, []);

  // Get entity fields for current entity type
  const entityFields = useMemo(() => {
    if (!workflowConfig?.entityFields || !entityType) return [];
    return workflowConfig.entityFields[entityType] || [];
  }, [workflowConfig, entityType]);

  // Get related entities for current entity type
  const relatedEntities = useMemo(() => {
    if (!workflowConfig?.relatedEntities || !entityType) return [];
    return workflowConfig.relatedEntities[entityType] || [];
  }, [workflowConfig, entityType]);

  // Group fields by category
  const groupedFields = useMemo(() => {
    const groups: Record<string, EntityFieldConfig[]> = {};
    entityFields.forEach(field => {
      const group = field.group || 'General';
      if (!groups[group]) groups[group] = [];
      groups[group].push(field);
    });
    return groups;
  }, [entityFields]);

  // Parse configuration on mount
  useEffect(() => {
    try {
      const parsed = configuration ? JSON.parse(configuration) : {};
      setConfig({
        triggerType: parsed.triggerType || 'OnCreate',
        watchedFields: parsed.watchedFields || [],
        fieldConditions: parsed.fieldConditions || [],
        scheduleType: parsed.scheduleType,
        schedulePreset: parsed.schedulePreset ?? (parsed.scheduleType === 'interval' ? 'interval' : parsed.scheduleType === 'cron' ? 'custom_cron' : undefined),
        cronExpression: parsed.cronExpression,
        intervalMinutes: parsed.intervalMinutes,
        scheduleHour: parsed.scheduleHour ?? 9,
        scheduleMinute: parsed.scheduleMinute ?? 0,
        scheduleDaysOfWeek: parsed.scheduleDaysOfWeek ?? [1],
        scheduleDayOfMonth: parsed.scheduleDayOfMonth ?? 1,
        timeZone: parsed.timeZone ?? 'UTC',
        webhookSecret: parsed.webhookSecret,
        webhookPayloadSchema: parsed.webhookPayloadSchema,
        runOnce: parsed.runOnce,
        batchMode: parsed.batchMode,
        batchSize: parsed.batchSize,
        filterExpression: parsed.filterExpression,
      });
    } catch {
      setConfig({ triggerType: 'OnCreate' });
    }
  }, [configuration]);

  // Update configuration
  const updateConfig = (updates: Partial<TriggerConfiguration>) => {
    const newConfig = { ...config, ...updates };
    setConfig(newConfig);
    onChange('configuration', JSON.stringify(newConfig));
  };

  // Schedule wrapper — auto-recomputes cron expression when visual fields change
  const updateSchedule = (updates: Partial<TriggerConfiguration>) => {
    const merged = { ...config, ...updates };
    const preset = merged.schedulePreset;
    if (preset && preset !== 'interval' && preset !== 'custom_cron') {
      const cron = buildCronFromConfig(merged);
      updates = { ...updates, cronExpression: cron, scheduleType: 'cron' };
    } else if (preset === 'interval') {
      updates = { ...updates, scheduleType: 'interval', cronExpression: undefined };
    }
    updateConfig(updates);
  };

  // Add field condition
  const addFieldCondition = () => {
    const fieldConditions = [
      ...(config.fieldConditions || []),
      { field: '', operator: 'equals' as const, value: '' }
    ];
    updateConfig({ fieldConditions });
  };

  // Update field condition
  const updateFieldCondition = (index: number, updates: Partial<FieldCondition>) => {
    const fieldConditions = [...(config.fieldConditions || [])];
    fieldConditions[index] = { ...fieldConditions[index], ...updates };
    updateConfig({ fieldConditions });
  };

  // Remove field condition
  const removeFieldCondition = (index: number) => {
    const fieldConditions = (config.fieldConditions || []).filter((_, i) => i !== index);
    updateConfig({ fieldConditions });
  };

  // Get field by name
  const getField = (fieldName: string): EntityFieldConfig | undefined => {
    return entityFields.find(f => f.name === fieldName);
  };

  // Show loading state while config is being fetched
  if (configLoading) {
    return (
      <Box sx={{ p: 2, display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 200 }}>
        <CircularProgress size={24} />
        <Typography sx={{ ml: 2 }}>Loading configuration...</Typography>
      </Box>
    );
  }

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      {/* Header */}
      <Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <TriggerIcon color="primary" />
          <Typography variant="h6">{nodeName}</Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1, mt: 0.5 }}>
          <Chip label="Trigger" size="small" color="primary" variant="outlined" />
          <Chip label={entityType} size="small" variant="outlined" />
          <Typography variant="caption" color="text.secondary" fontFamily="monospace">
            {nodeKey}
          </Typography>
        </Box>
      </Box>

      <Divider />

      {/* Basic Properties */}
      <TextField
        fullWidth
        size="small"
        label="Name"
        value={nodeName}
        onChange={(e) => onChange('name', e.target.value)}
        disabled={readonly}
      />

      <Divider />

      {/* Tabs for configuration sections */}
      <Tabs
        value={tabValue}
        onChange={(_, v) => setTabValue(v)}
        variant="scrollable"
        scrollButtons="auto"
      >
        <Tab icon={<TriggerIcon fontSize="small" />} label="Trigger Type" iconPosition="start" />
        <Tab icon={<FieldIcon fontSize="small" />} label="Field Conditions" iconPosition="start" />
        <Tab icon={<CodeIcon fontSize="small" />} label="Advanced" iconPosition="start" />
      </Tabs>

      {/* Trigger Type Tab */}
      <TabPanel value={tabValue} index={0}>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          <FormControl fullWidth size="small">
            <InputLabel>Trigger Type</InputLabel>
            <Select
              value={config.triggerType}
              label="Trigger Type"
              onChange={(e) => updateConfig({ triggerType: e.target.value as TriggerConfiguration['triggerType'] })}
              disabled={readonly}
            >
              <MenuItem value="OnCreate">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <AddIcon fontSize="small" />
                  Record Created
                </Box>
              </MenuItem>
              <MenuItem value="OnUpdate">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <FieldIcon fontSize="small" />
                  Record Updated (Any Field)
                </Box>
              </MenuItem>
              <MenuItem value="OnDelete">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <DeleteIcon fontSize="small" />
                  Record Deleted
                </Box>
              </MenuItem>
              <MenuItem value="OnFieldChange">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <EventIcon fontSize="small" />
                  Specific Field Changed
                </Box>
              </MenuItem>
              <MenuItem value="OnStatusChange">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <EventIcon fontSize="small" />
                  Status Changed
                </Box>
              </MenuItem>
              <MenuItem value="Scheduled">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <ScheduleIcon fontSize="small" />
                  Scheduled / Recurring
                </Box>
              </MenuItem>
              <MenuItem value="OnEvent">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <EventIcon fontSize="small" />
                  On Event
                </Box>
              </MenuItem>
              <MenuItem value="OnWebhook">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <WebhookIcon fontSize="small" />
                  Webhook
                </Box>
              </MenuItem>
              <MenuItem value="OnSLABreach">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <EventIcon fontSize="small" />
                  SLA Breach
                </Box>
              </MenuItem>
              <MenuItem value="OnEscalation">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <EventIcon fontSize="small" />
                  Escalation
                </Box>
              </MenuItem>
              <MenuItem value="OnApproval">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <EventIcon fontSize="small" />
                  Approved
                </Box>
              </MenuItem>
              <MenuItem value="OnRejection">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <EventIcon fontSize="small" />
                  Rejected
                </Box>
              </MenuItem>
              <MenuItem value="OnAssignment">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <EventIcon fontSize="small" />
                  Assigned
                </Box>
              </MenuItem>
              <MenuItem value="Manual">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <TriggerIcon fontSize="small" />
                  Manual Trigger
                </Box>
              </MenuItem>
            </Select>
          </FormControl>

          {/* Field Changed - Select Fields to Watch */}
          {config.triggerType === 'OnFieldChange' && (
            <Box>
              <Typography variant="subtitle2" sx={{ mb: 1 }}>
                Fields to Watch
              </Typography>
              <Autocomplete
                multiple
                size="small"
                options={entityFields.map(f => f.name)}
                value={config.watchedFields || []}
                onChange={(_, value) => updateConfig({ watchedFields: value })}
                disabled={readonly}
                getOptionLabel={(option) => {
                  const field = getField(option);
                  return field ? `${field.label} (${field.name})` : option;
                }}
                groupBy={(option) => {
                  const field = getField(option);
                  return field?.group || 'General';
                }}
                renderInput={(params) => (
                  <TextField {...params} placeholder="Select fields to watch..." />
                )}
                renderTags={(value, getTagProps) =>
                  value.map((option, index) => {
                    const field = getField(option);
                    return (
                      <Chip
                        {...getTagProps({ index })}
                        key={option}
                        label={field?.label || option}
                        size="small"
                      />
                    );
                  })
                }
              />
              <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5 }}>
                Workflow triggers when any of these fields change
              </Typography>
            </Box>
          )}

          {/* Scheduled - Enhanced Schedule Builder */}
          {config.triggerType === 'Scheduled' && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>

              {/* Pattern preset */}
              <FormControl fullWidth size="small">
                <InputLabel>Schedule Pattern</InputLabel>
                <Select
                  value={config.schedulePreset || 'daily'}
                  label="Schedule Pattern"
                  onChange={(e) => updateSchedule({ schedulePreset: e.target.value as TriggerConfiguration['schedulePreset'] })}
                  disabled={readonly}
                >
                  <MenuItem value="interval">Every X minutes</MenuItem>
                  <MenuItem value="hourly">Every hour</MenuItem>
                  <MenuItem value="daily">Every day</MenuItem>
                  <MenuItem value="weekly">Every week</MenuItem>
                  <MenuItem value="monthly">Every month</MenuItem>
                  <MenuItem value="custom_cron">Custom cron expression</MenuItem>
                </Select>
              </FormControl>

              {/* Interval */}
              {(config.schedulePreset === 'interval' || (!config.schedulePreset && config.scheduleType === 'interval')) && (
                <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
                  <Typography variant="body2" sx={{ color: 'text.secondary', whiteSpace: 'nowrap' }}>Every</Typography>
                  <TextField
                    size="small"
                    type="number"
                    value={config.intervalMinutes || 60}
                    onChange={(e) => updateSchedule({ intervalMinutes: Math.max(1, parseInt(e.target.value) || 60) })}
                    disabled={readonly}
                    inputProps={{ min: 1, style: { textAlign: 'center' } }}
                    sx={{ width: 90 }}
                  />
                  <Typography variant="body2" sx={{ color: 'text.secondary' }}>minutes</Typography>
                </Box>
              )}

              {/* Time of day — hour + minute — for hourly/daily/weekly/monthly */}
              {['hourly', 'daily', 'weekly', 'monthly'].includes(config.schedulePreset || '') && (
                <Box sx={{ display: 'flex', gap: 1.5, alignItems: 'center' }}>
                  <ScheduleIcon fontSize="small" sx={{ color: 'text.secondary' }} />
                  <Typography variant="body2" sx={{ color: 'text.secondary' }}>At</Typography>
                  {config.schedulePreset !== 'hourly' && (
                    <FormControl size="small" sx={{ minWidth: 90 }}>
                      <InputLabel>Hour</InputLabel>
                      <Select
                        value={config.scheduleHour ?? 9}
                        label="Hour"
                        onChange={(e) => updateSchedule({ scheduleHour: e.target.value as number })}
                        disabled={readonly}
                      >
                        {Array.from({ length: 24 }, (_, i) => (
                          <MenuItem key={i} value={i}>{String(i).padStart(2, '0')}h</MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  )}
                  <FormControl size="small" sx={{ minWidth: 90 }}>
                    <InputLabel>Minute</InputLabel>
                    <Select
                      value={config.scheduleMinute ?? 0}
                      label="Minute"
                      onChange={(e) => updateSchedule({ scheduleMinute: e.target.value as number })}
                      disabled={readonly}
                    >
                      {[0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55].map(m => (
                        <MenuItem key={m} value={m}>:{String(m).padStart(2, '0')}</MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Box>
              )}

              {/* Day-of-week chip picker for weekly */}
              {config.schedulePreset === 'weekly' && (
                <Box>
                  <Typography variant="caption" sx={{ color: 'text.secondary', mb: 0.75, display: 'block' }}>Days of Week</Typography>
                  <Box sx={{ display: 'flex', gap: 0.5 }}>
                    {([['Mon', 1], ['Tue', 2], ['Wed', 3], ['Thu', 4], ['Fri', 5], ['Sat', 6], ['Sun', 0]] as [string, number][]).map(([label, val]) => {
                      const dow = config.scheduleDaysOfWeek ?? [1];
                      const active = dow.includes(val);
                      return (
                        <Chip
                          key={val}
                          label={label}
                          size="small"
                          onClick={readonly ? undefined : () => {
                            const next = active ? dow.filter(d => d !== val) : [...dow, val];
                            updateSchedule({ scheduleDaysOfWeek: next.length > 0 ? next : [val] });
                          }}
                          sx={{
                            cursor: readonly ? 'default' : 'pointer',
                            fontWeight: 600,
                            backgroundColor: active ? 'primary.main' : 'action.hover',
                            color: active ? 'primary.contrastText' : 'text.secondary',
                          }}
                        />
                      );
                    })}
                  </Box>
                </Box>
              )}

              {/* Day of month for monthly */}
              {config.schedulePreset === 'monthly' && (
                <TextField
                  fullWidth
                  size="small"
                  type="number"
                  label="Day of month (1–28)"
                  value={config.scheduleDayOfMonth ?? 1}
                  onChange={(e) => updateSchedule({ scheduleDayOfMonth: Math.min(28, Math.max(1, parseInt(e.target.value) || 1)) })}
                  disabled={readonly}
                  inputProps={{ min: 1, max: 28 }}
                  helperText="Use 1 for first day · max 28 avoids month-end issues"
                />
              )}

              {/* Custom cron */}
              {(config.schedulePreset === 'custom_cron' || (!config.schedulePreset && config.scheduleType === 'cron')) && (
                <TextField
                  fullWidth
                  size="small"
                  label="Cron Expression"
                  value={config.cronExpression || '0 9 * * *'}
                  onChange={(e) => updateSchedule({ cronExpression: e.target.value })}
                  disabled={readonly}
                  helperText="Format: min hour dom month dow  —  e.g., 0 9 * * 1-5 (weekdays at 9 AM)"
                  sx={{ '& .MuiInputBase-input': { fontFamily: 'monospace' } }}
                />
              )}

              {/* Timezone */}
              {config.schedulePreset !== 'interval' && (
                <FormControl fullWidth size="small">
                  <InputLabel>Timezone</InputLabel>
                  <Select
                    value={config.timeZone || 'UTC'}
                    label="Timezone"
                    onChange={(e) => updateSchedule({ timeZone: e.target.value })}
                    disabled={readonly}
                  >
                    {COMMON_TIMEZONES.map(tz => (
                      <MenuItem key={tz} value={tz}>{tz.replace(/_/g, ' ')}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              )}

              {/* Human-readable summary */}
              <Alert
                severity="info"
                icon={<ScheduleIcon fontSize="small" />}
                sx={{ '& .MuiAlert-message': { fontSize: 13 } }}
              >
                {describeCronConfig(config)}
                {config.schedulePreset && config.schedulePreset !== 'interval' && (
                  <Typography variant="caption" sx={{ display: 'block', fontFamily: 'monospace', color: 'text.secondary', mt: 0.5 }}>
                    cron: {buildCronFromConfig(config)}
                  </Typography>
                )}
              </Alert>

            </Box>
          )}

          {/* Webhook - Secret and Schema */}
          {config.triggerType === 'OnWebhook' && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <TextField
                fullWidth
                size="small"
                label="Webhook Secret (optional)"
                type="password"
                value={config.webhookSecret || ''}
                onChange={(e) => updateConfig({ webhookSecret: e.target.value })}
                disabled={readonly}
                helperText="Used to validate incoming webhook requests"
              />
              <TextField
                fullWidth
                size="small"
                label="Payload Schema (JSON)"
                value={config.webhookPayloadSchema || ''}
                onChange={(e) => updateConfig({ webhookPayloadSchema: e.target.value })}
                disabled={readonly}
                multiline
                rows={4}
                sx={{ '& .MuiInputBase-input': { fontFamily: 'monospace', fontSize: 12 } }}
              />
            </Box>
          )}

          {/* Common options */}
          <Divider />
          <FormControlLabel
            control={
              <Switch
                checked={config.runOnce || false}
                onChange={(e) => updateConfig({ runOnce: e.target.checked })}
                disabled={readonly}
              />
            }
            label="Run Once Per Record"
          />
          <FormControlLabel
            control={
              <Switch
                checked={config.batchMode || false}
                onChange={(e) => updateConfig({ batchMode: e.target.checked })}
                disabled={readonly}
              />
            }
            label="Batch Mode"
          />
          {config.batchMode && (
            <TextField
              fullWidth
              size="small"
              type="number"
              label="Batch Size"
              value={config.batchSize || 100}
              onChange={(e) => updateConfig({ batchSize: parseInt(e.target.value) || 100 })}
              disabled={readonly}
              inputProps={{ min: 1, max: 1000 }}
            />
          )}
        </Box>
      </TabPanel>

      {/* Field Conditions Tab */}
      <TabPanel value={tabValue} index={1}>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="subtitle2">
              Field Conditions
            </Typography>
            <Button
              size="small"
              startIcon={<AddIcon />}
              onClick={addFieldCondition}
              disabled={readonly}
            >
              Add Condition
            </Button>
          </Box>

          {(!config.fieldConditions || config.fieldConditions.length === 0) && (
            <Alert severity="info" sx={{ py: 0 }}>
              No field conditions defined. The trigger will fire for any matching event.
            </Alert>
          )}

          {(config.fieldConditions || []).map((condition, index) => {
            const selectedField = getField(condition.field);
            
            return (
              <Card key={index} variant="outlined">
                <CardContent sx={{ pb: '8px !important' }}>
                  <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                    {/* Field selector */}
                    <FormControl size="small" sx={{ minWidth: 180 }}>
                      <InputLabel>Field</InputLabel>
                      <Select
                        value={condition.field}
                        label="Field"
                        onChange={(e) => updateFieldCondition(index, { field: e.target.value })}
                        disabled={readonly}
                      >
                        {Object.entries(groupedFields).map(([group, fields]) => [
                          <ListItem key={group} sx={{ fontWeight: 'bold', bgcolor: 'action.hover' }}>
                            {group}
                          </ListItem>,
                          ...fields.map(f => (
                            <MenuItem key={f.name} value={f.name}>
                              {f.label}
                              {f.type === 'enum' && <Chip label="enum" size="small" sx={{ ml: 1 }} />}
                            </MenuItem>
                          ))
                        ])}
                      </Select>
                    </FormControl>

                    {/* Operator selector */}
                    <FormControl size="small" sx={{ minWidth: 140 }}>
                      <InputLabel>Operator</InputLabel>
                      <Select
                        value={condition.operator}
                        label="Operator"
                        onChange={(e) => updateFieldCondition(index, { operator: e.target.value as FieldCondition['operator'] })}
                        disabled={readonly}
                      >
                        <MenuItem value="equals">Equals</MenuItem>
                        <MenuItem value="notEquals">Not Equals</MenuItem>
                        <MenuItem value="contains">Contains</MenuItem>
                        <MenuItem value="notContains">Not Contains</MenuItem>
                        <MenuItem value="startsWith">Starts With</MenuItem>
                        <MenuItem value="endsWith">Ends With</MenuItem>
                        <MenuItem value="greaterThan">Greater Than</MenuItem>
                        <MenuItem value="lessThan">Less Than</MenuItem>
                        <MenuItem value="greaterThanOrEqual">Greater Than or Equal</MenuItem>
                        <MenuItem value="lessThanOrEqual">Less Than or Equal</MenuItem>
                        <MenuItem value="isNull">Is Empty</MenuItem>
                        <MenuItem value="isNotNull">Is Not Empty</MenuItem>
                        <MenuItem value="in">In (comma-separated)</MenuItem>
                        <MenuItem value="notIn">Not In</MenuItem>
                        <MenuItem value="between">Between</MenuItem>
                        <MenuItem value="regex">Regex Match</MenuItem>
                        <MenuItem value="changed_to">Changed To</MenuItem>
                        <MenuItem value="changed_from">Changed From</MenuItem>
                      </Select>
                    </FormControl>

                    {/* Value input - based on field type */}
                    {!['isNull', 'isNotNull'].includes(condition.operator) && (
                      selectedField?.type === 'enum' && selectedField.enumValues ? (
                        <FormControl size="small" sx={{ minWidth: 140, flex: 1 }}>
                          <InputLabel>Value</InputLabel>
                          <Select
                            value={condition.value || ''}
                            label="Value"
                            onChange={(e) => updateFieldCondition(index, { value: e.target.value })}
                            disabled={readonly}
                          >
                            {selectedField.enumValues.map(v => (
                              <MenuItem key={v} value={v}>{v}</MenuItem>
                            ))}
                          </Select>
                        </FormControl>
                      ) : selectedField?.type === 'boolean' ? (
                        <FormControl size="small" sx={{ minWidth: 100, flex: 1 }}>
                          <InputLabel>Value</InputLabel>
                          <Select
                            value={condition.value || ''}
                            label="Value"
                            onChange={(e) => updateFieldCondition(index, { value: e.target.value })}
                            disabled={readonly}
                          >
                            <MenuItem value="true">True</MenuItem>
                            <MenuItem value="false">False</MenuItem>
                          </Select>
                        </FormControl>
                      ) : (
                        <TextField
                          size="small"
                          label="Value"
                          value={condition.value || ''}
                          onChange={(e) => updateFieldCondition(index, { value: e.target.value })}
                          disabled={readonly}
                          sx={{ flex: 1, minWidth: 100 }}
                          type={selectedField?.type === 'number' ? 'number' : 'text'}
                        />
                      )
                    )}

                    {/* Delete button */}
                    <IconButton
                      size="small"
                      onClick={() => removeFieldCondition(index)}
                      disabled={readonly}
                    >
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </Box>
                </CardContent>
              </Card>
            );
          })}

          <Alert severity="info" icon={<HelpIcon />}>
            <Typography variant="body2">
              <strong>Tip:</strong> Field conditions are evaluated with AND logic. 
              All conditions must be true for the trigger to fire.
            </Typography>
          </Alert>
        </Box>
      </TabPanel>

      {/* Advanced Tab */}
      <TabPanel value={tabValue} index={2}>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          <TextField
            fullWidth
            size="small"
            label="Filter Expression"
            value={config.filterExpression || ''}
            onChange={(e) => updateConfig({ filterExpression: e.target.value })}
            disabled={readonly}
            multiline
            rows={3}
            helperText="Optional expression to filter which records trigger the workflow (e.g., entity.type == 'VIP')"
            sx={{ '& .MuiInputBase-input': { fontFamily: 'monospace', fontSize: 12 } }}
          />

          {/* Related Entities Info */}
          {relatedEntities.length > 0 && (
            <Box>
              <Typography variant="subtitle2" sx={{ mb: 1 }}>
                Related Entities Available
              </Typography>
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                {relatedEntities.map(rel => (
                  <Chip
                    key={rel.name}
                    label={`${rel.label} (${rel.relationType})`}
                    size="small"
                    variant="outlined"
                  />
                ))}
              </Box>
              <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5 }}>
                These related entities can be accessed in actions and conditions
              </Typography>
            </Box>
          )}

          {/* Raw JSON editor for advanced config */}
          <Accordion>
            <AccordionSummary expandIcon={<ExpandIcon />}>
              <CodeIcon fontSize="small" sx={{ mr: 1 }} />
              <Typography variant="body2">Raw Configuration (JSON)</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <TextField
                fullWidth
                multiline
                rows={10}
                value={JSON.stringify(config, null, 2)}
                onChange={(e) => {
                  try {
                    const parsed = JSON.parse(e.target.value);
                    setConfig(parsed);
                    onChange('configuration', e.target.value);
                  } catch {
                    // Invalid JSON, don't update
                  }
                }}
                disabled={readonly}
                sx={{ '& .MuiInputBase-input': { fontFamily: 'monospace', fontSize: 11 } }}
              />
            </AccordionDetails>
          </Accordion>
        </Box>
      </TabPanel>

      <Divider />

      {/* Delete Button */}
      <Button
        variant="outlined"
        color="error"
        startIcon={<DeleteIcon />}
        onClick={onDelete}
        disabled={readonly}
      >
        Delete Node
      </Button>
    </Box>
  );
};

export default TriggerPropertiesPanel;
