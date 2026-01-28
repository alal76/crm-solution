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
  triggerType: 'record_created' | 'record_updated' | 'field_changed' | 'scheduled' | 'webhook' | 'manual';
  
  // Field Change Triggers
  watchedFields?: string[];
  fieldConditions?: FieldCondition[];
  
  // Scheduled Triggers
  scheduleType?: 'cron' | 'interval';
  cronExpression?: string;
  intervalMinutes?: number;
  
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
  operator: 'equals' | 'not_equals' | 'changed_to' | 'changed_from' | 'contains' | 'is_empty' | 'is_not_empty';
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
  const [config, setConfig] = useState<TriggerConfiguration>({ triggerType: 'record_created' });
  
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
        triggerType: parsed.triggerType || 'record_created',
        watchedFields: parsed.watchedFields || [],
        fieldConditions: parsed.fieldConditions || [],
        scheduleType: parsed.scheduleType,
        cronExpression: parsed.cronExpression,
        intervalMinutes: parsed.intervalMinutes,
        webhookSecret: parsed.webhookSecret,
        webhookPayloadSchema: parsed.webhookPayloadSchema,
        runOnce: parsed.runOnce,
        batchMode: parsed.batchMode,
        batchSize: parsed.batchSize,
        filterExpression: parsed.filterExpression,
      });
    } catch {
      setConfig({ triggerType: 'record_created' });
    }
  }, [configuration]);

  // Update configuration
  const updateConfig = (updates: Partial<TriggerConfiguration>) => {
    const newConfig = { ...config, ...updates };
    setConfig(newConfig);
    onChange('configuration', JSON.stringify(newConfig));
  };

  // Add field condition
  const addFieldCondition = () => {
    const fieldConditions = [
      ...(config.fieldConditions || []),
      { field: '', operator: 'changed_to' as const, value: '' }
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
              <MenuItem value="record_created">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <AddIcon fontSize="small" />
                  Record Created
                </Box>
              </MenuItem>
              <MenuItem value="record_updated">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <FieldIcon fontSize="small" />
                  Record Updated (Any Field)
                </Box>
              </MenuItem>
              <MenuItem value="field_changed">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <EventIcon fontSize="small" />
                  Specific Field Changed
                </Box>
              </MenuItem>
              <MenuItem value="scheduled">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <ScheduleIcon fontSize="small" />
                  Scheduled / Recurring
                </Box>
              </MenuItem>
              <MenuItem value="webhook">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <WebhookIcon fontSize="small" />
                  Webhook
                </Box>
              </MenuItem>
              <MenuItem value="manual">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <TriggerIcon fontSize="small" />
                  Manual Trigger
                </Box>
              </MenuItem>
            </Select>
          </FormControl>

          {/* Field Changed - Select Fields to Watch */}
          {config.triggerType === 'field_changed' && (
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

          {/* Scheduled - Cron or Interval */}
          {config.triggerType === 'scheduled' && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <FormControl fullWidth size="small">
                <InputLabel>Schedule Type</InputLabel>
                <Select
                  value={config.scheduleType || 'interval'}
                  label="Schedule Type"
                  onChange={(e) => updateConfig({ scheduleType: e.target.value as 'cron' | 'interval' })}
                  disabled={readonly}
                >
                  <MenuItem value="interval">Interval (Every X minutes)</MenuItem>
                  <MenuItem value="cron">Cron Expression</MenuItem>
                </Select>
              </FormControl>

              {config.scheduleType === 'interval' && (
                <TextField
                  fullWidth
                  size="small"
                  type="number"
                  label="Interval (minutes)"
                  value={config.intervalMinutes || 60}
                  onChange={(e) => updateConfig({ intervalMinutes: parseInt(e.target.value) || 60 })}
                  disabled={readonly}
                  inputProps={{ min: 1 }}
                />
              )}

              {config.scheduleType === 'cron' && (
                <TextField
                  fullWidth
                  size="small"
                  label="Cron Expression"
                  value={config.cronExpression || '0 0 * * *'}
                  onChange={(e) => updateConfig({ cronExpression: e.target.value })}
                  disabled={readonly}
                  helperText="e.g., 0 9 * * 1-5 (9 AM Mon-Fri)"
                />
              )}
            </Box>
          )}

          {/* Webhook - Secret and Schema */}
          {config.triggerType === 'webhook' && (
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
                        <MenuItem value="changed_to">Changed To</MenuItem>
                        <MenuItem value="changed_from">Changed From</MenuItem>
                        <MenuItem value="equals">Equals</MenuItem>
                        <MenuItem value="not_equals">Not Equals</MenuItem>
                        <MenuItem value="contains">Contains</MenuItem>
                        <MenuItem value="is_empty">Is Empty</MenuItem>
                        <MenuItem value="is_not_empty">Is Not Empty</MenuItem>
                      </Select>
                    </FormControl>

                    {/* Value input - based on field type */}
                    {!['is_empty', 'is_not_empty'].includes(condition.operator) && (
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
