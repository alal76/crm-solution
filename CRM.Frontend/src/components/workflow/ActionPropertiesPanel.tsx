/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Action Properties Panel - Configuration panel for workflow action nodes
 * Allows setting field values, calling APIs, and configuring field updates
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
  FlashOn as ActionIcon,
  Email as EmailIcon,
  Http as HttpIcon,
  Edit as FieldIcon,
  Assignment as TaskIcon,
  Notifications as NotifyIcon,
  Help as HelpIcon,
  Code as CodeIcon,
  Link as RelationIcon,
  Error as ErrorIcon,
  Security as SecurityIcon,
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

export interface ActionConfiguration {
  actionType: 'update_field' | 'update_related' | 'send_email' | 'send_notification' | 'call_api' | 'create_record' | 'create_task' | 'custom';
  
  // Field Updates
  fieldUpdates?: FieldUpdate[];
  
  // Related Entity Updates
  relatedEntityUpdates?: RelatedEntityUpdate[];
  
  // Email Action
  emailTo?: string;
  emailSubject?: string;
  emailTemplate?: string;
  emailBody?: string;
  
  // Notification Action
  notificationType?: 'in_app' | 'push' | 'email' | 'sms';
  notificationRecipients?: string[];
  notificationMessage?: string;
  
  // API Call Action
  apiUrl?: string;
  apiMethod?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';
  apiHeaders?: Record<string, string>;
  apiBody?: string;
  apiResponseMapping?: Record<string, string>;
  
  // Create Record Action
  createEntityType?: string;
  createFieldValues?: FieldUpdate[];
  
  // Create Task Action
  taskTitle?: string;
  taskDescription?: string;
  taskAssignTo?: string;
  taskDueDays?: number;
  taskPriority?: 'Low' | 'Medium' | 'High' | 'Critical';
  
  // Error Handling
  retryCount?: number;
  retryDelaySeconds?: number;
  fallbackAction?: 'skip' | 'fail' | 'default_value';
  
  // Custom Script
  customScript?: string;
}

interface FieldUpdate {
  field: string;
  valueType: 'static' | 'variable' | 'expression' | 'copy_from';
  value: string;
  sourceField?: string;
}

interface RelatedEntityUpdate {
  relationship: string;
  fieldUpdates: FieldUpdate[];
}

interface ActionPropertiesPanelProps {
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

export const ActionPropertiesPanel: React.FC<ActionPropertiesPanelProps> = ({
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
  const [config, setConfig] = useState<ActionConfiguration>({ actionType: 'update_field' });
  
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

  // Get fields for a related entity
  const getRelatedEntityFields = (relName: string): EntityFieldConfig[] => {
    const rel = relatedEntities.find(r => r.name === relName);
    if (!rel || !workflowConfig?.entityFields) return [];
    return workflowConfig.entityFields[rel.entityType] || [];
  };

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

  // All entity types for create record action
  const allEntityTypes = workflowConfig?.entityTypes || [];

  // Parse configuration on mount
  useEffect(() => {
    try {
      const parsed = configuration ? JSON.parse(configuration) : {};
      setConfig({
        actionType: parsed.actionType || 'update_field',
        fieldUpdates: parsed.fieldUpdates || [],
        relatedEntityUpdates: parsed.relatedEntityUpdates || [],
        emailTo: parsed.emailTo,
        emailSubject: parsed.emailSubject,
        emailTemplate: parsed.emailTemplate,
        emailBody: parsed.emailBody,
        notificationType: parsed.notificationType,
        notificationRecipients: parsed.notificationRecipients || [],
        notificationMessage: parsed.notificationMessage,
        apiUrl: parsed.apiUrl,
        apiMethod: parsed.apiMethod || 'POST',
        apiHeaders: parsed.apiHeaders || {},
        apiBody: parsed.apiBody,
        apiResponseMapping: parsed.apiResponseMapping || {},
        createEntityType: parsed.createEntityType,
        createFieldValues: parsed.createFieldValues || [],
        taskTitle: parsed.taskTitle,
        taskDescription: parsed.taskDescription,
        taskAssignTo: parsed.taskAssignTo,
        taskDueDays: parsed.taskDueDays || 1,
        taskPriority: parsed.taskPriority || 'Medium',
        retryCount: parsed.retryCount || 3,
        retryDelaySeconds: parsed.retryDelaySeconds || 30,
        fallbackAction: parsed.fallbackAction || 'fail',
        customScript: parsed.customScript,
      });
    } catch {
      setConfig({ actionType: 'update_field' });
    }
  }, [configuration]);

  // Update configuration
  const updateConfig = (updates: Partial<ActionConfiguration>) => {
    const newConfig = { ...config, ...updates };
    setConfig(newConfig);
    onChange('configuration', JSON.stringify(newConfig));
  };

  // Add field update
  const addFieldUpdate = () => {
    const fieldUpdates = [
      ...(config.fieldUpdates || []),
      { field: '', valueType: 'static' as const, value: '' }
    ];
    updateConfig({ fieldUpdates });
  };

  // Update field update
  const updateFieldUpdate = (index: number, updates: Partial<FieldUpdate>) => {
    const fieldUpdates = [...(config.fieldUpdates || [])];
    fieldUpdates[index] = { ...fieldUpdates[index], ...updates };
    updateConfig({ fieldUpdates });
  };

  // Remove field update
  const removeFieldUpdate = (index: number) => {
    const fieldUpdates = (config.fieldUpdates || []).filter((_, i) => i !== index);
    updateConfig({ fieldUpdates });
  };

  // Add related entity update
  const addRelatedEntityUpdate = () => {
    const relatedEntityUpdates = [
      ...(config.relatedEntityUpdates || []),
      { relationship: '', fieldUpdates: [] }
    ];
    updateConfig({ relatedEntityUpdates });
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
          <ActionIcon color="secondary" />
          <Typography variant="h6">{nodeName}</Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1, mt: 0.5 }}>
          <Chip label="Action" size="small" color="secondary" variant="outlined" />
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
        <Tab icon={<ActionIcon fontSize="small" />} label="Action Type" iconPosition="start" />
        <Tab icon={<FieldIcon fontSize="small" />} label="Field Updates" iconPosition="start" />
        <Tab icon={<RelationIcon fontSize="small" />} label="Related Records" iconPosition="start" />
        <Tab icon={<ErrorIcon fontSize="small" />} label="Error Handling" iconPosition="start" />
      </Tabs>

      {/* Action Type Tab */}
      <TabPanel value={tabValue} index={0}>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          <FormControl fullWidth size="small">
            <InputLabel>Action Type</InputLabel>
            <Select
              value={config.actionType}
              label="Action Type"
              onChange={(e) => updateConfig({ actionType: e.target.value as ActionConfiguration['actionType'] })}
              disabled={readonly}
            >
              <MenuItem value="update_field">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <FieldIcon fontSize="small" />
                  Update Fields
                </Box>
              </MenuItem>
              <MenuItem value="update_related">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <RelationIcon fontSize="small" />
                  Update Related Record
                </Box>
              </MenuItem>
              <MenuItem value="send_email">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <EmailIcon fontSize="small" />
                  Send Email
                </Box>
              </MenuItem>
              <MenuItem value="send_notification">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <NotifyIcon fontSize="small" />
                  Send Notification
                </Box>
              </MenuItem>
              <MenuItem value="call_api">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <HttpIcon fontSize="small" />
                  Call API / Webhook
                </Box>
              </MenuItem>
              <MenuItem value="create_record">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <AddIcon fontSize="small" />
                  Create Record
                </Box>
              </MenuItem>
              <MenuItem value="create_task">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <TaskIcon fontSize="small" />
                  Create Task
                </Box>
              </MenuItem>
              <MenuItem value="custom">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <CodeIcon fontSize="small" />
                  Custom Script
                </Box>
              </MenuItem>
            </Select>
          </FormControl>

          {/* Send Email Configuration */}
          {config.actionType === 'send_email' && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <TextField
                fullWidth
                size="small"
                label="To"
                value={config.emailTo || ''}
                onChange={(e) => updateConfig({ emailTo: e.target.value })}
                disabled={readonly}
                helperText="Use {{entity.email}} for dynamic values"
              />
              <TextField
                fullWidth
                size="small"
                label="Subject"
                value={config.emailSubject || ''}
                onChange={(e) => updateConfig({ emailSubject: e.target.value })}
                disabled={readonly}
              />
              <TextField
                fullWidth
                size="small"
                label="Template Name (optional)"
                value={config.emailTemplate || ''}
                onChange={(e) => updateConfig({ emailTemplate: e.target.value })}
                disabled={readonly}
              />
              <TextField
                fullWidth
                size="small"
                label="Email Body"
                value={config.emailBody || ''}
                onChange={(e) => updateConfig({ emailBody: e.target.value })}
                disabled={readonly}
                multiline
                rows={4}
                helperText="Use {{entity.fieldName}} for dynamic content"
              />
            </Box>
          )}

          {/* Send Notification Configuration */}
          {config.actionType === 'send_notification' && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <FormControl fullWidth size="small">
                <InputLabel>Notification Type</InputLabel>
                <Select
                  value={config.notificationType || 'in_app'}
                  label="Notification Type"
                  onChange={(e) => updateConfig({ notificationType: e.target.value as any })}
                  disabled={readonly}
                >
                  <MenuItem value="in_app">In-App Notification</MenuItem>
                  <MenuItem value="push">Push Notification</MenuItem>
                  <MenuItem value="email">Email</MenuItem>
                  <MenuItem value="sms">SMS</MenuItem>
                </Select>
              </FormControl>
              <TextField
                fullWidth
                size="small"
                label="Recipients"
                value={(config.notificationRecipients || []).join(', ')}
                onChange={(e) => updateConfig({ notificationRecipients: e.target.value.split(',').map(s => s.trim()) })}
                disabled={readonly}
                helperText="Comma-separated list of user IDs, roles, or email addresses"
              />
              <TextField
                fullWidth
                size="small"
                label="Message"
                value={config.notificationMessage || ''}
                onChange={(e) => updateConfig({ notificationMessage: e.target.value })}
                disabled={readonly}
                multiline
                rows={2}
              />
            </Box>
          )}

          {/* Call API Configuration */}
          {config.actionType === 'call_api' && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <Box sx={{ display: 'flex', gap: 1 }}>
                <FormControl size="small" sx={{ minWidth: 100 }}>
                  <InputLabel>Method</InputLabel>
                  <Select
                    value={config.apiMethod || 'POST'}
                    label="Method"
                    onChange={(e) => updateConfig({ apiMethod: e.target.value as any })}
                    disabled={readonly}
                  >
                    <MenuItem value="GET">GET</MenuItem>
                    <MenuItem value="POST">POST</MenuItem>
                    <MenuItem value="PUT">PUT</MenuItem>
                    <MenuItem value="PATCH">PATCH</MenuItem>
                    <MenuItem value="DELETE">DELETE</MenuItem>
                  </Select>
                </FormControl>
                <TextField
                  size="small"
                  label="URL"
                  value={config.apiUrl || ''}
                  onChange={(e) => updateConfig({ apiUrl: e.target.value })}
                  disabled={readonly}
                  sx={{ flex: 1 }}
                />
              </Box>
              <TextField
                fullWidth
                size="small"
                label="Headers (JSON)"
                value={JSON.stringify(config.apiHeaders || {}, null, 2)}
                onChange={(e) => {
                  try {
                    updateConfig({ apiHeaders: JSON.parse(e.target.value) });
                  } catch {}
                }}
                disabled={readonly}
                multiline
                rows={3}
                sx={{ '& .MuiInputBase-input': { fontFamily: 'monospace', fontSize: 12 } }}
              />
              <TextField
                fullWidth
                size="small"
                label="Request Body"
                value={config.apiBody || ''}
                onChange={(e) => updateConfig({ apiBody: e.target.value })}
                disabled={readonly}
                multiline
                rows={4}
                sx={{ '& .MuiInputBase-input': { fontFamily: 'monospace', fontSize: 12 } }}
              />
            </Box>
          )}

          {/* Create Record Configuration */}
          {config.actionType === 'create_record' && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <FormControl fullWidth size="small">
                <InputLabel>Entity Type</InputLabel>
                <Select
                  value={config.createEntityType || ''}
                  label="Entity Type"
                  onChange={(e) => updateConfig({ createEntityType: e.target.value })}
                  disabled={readonly}
                >
                  {allEntityTypes.map(et => (
                    <MenuItem key={et.value} value={et.value}>{et.label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
              <Alert severity="info" sx={{ py: 0 }}>
                Configure field values in the "Field Updates" tab
              </Alert>
            </Box>
          )}

          {/* Create Task Configuration */}
          {config.actionType === 'create_task' && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <TextField
                fullWidth
                size="small"
                label="Task Title"
                value={config.taskTitle || ''}
                onChange={(e) => updateConfig({ taskTitle: e.target.value })}
                disabled={readonly}
              />
              <TextField
                fullWidth
                size="small"
                label="Description"
                value={config.taskDescription || ''}
                onChange={(e) => updateConfig({ taskDescription: e.target.value })}
                disabled={readonly}
                multiline
                rows={2}
              />
              <TextField
                fullWidth
                size="small"
                label="Assign To (User ID or Role)"
                value={config.taskAssignTo || ''}
                onChange={(e) => updateConfig({ taskAssignTo: e.target.value })}
                disabled={readonly}
              />
              <Box sx={{ display: 'flex', gap: 2 }}>
                <TextField
                  size="small"
                  type="number"
                  label="Due In (days)"
                  value={config.taskDueDays || 1}
                  onChange={(e) => updateConfig({ taskDueDays: parseInt(e.target.value) || 1 })}
                  disabled={readonly}
                  sx={{ flex: 1 }}
                />
                <FormControl size="small" sx={{ flex: 1 }}>
                  <InputLabel>Priority</InputLabel>
                  <Select
                    value={config.taskPriority || 'Medium'}
                    label="Priority"
                    onChange={(e) => updateConfig({ taskPriority: e.target.value as any })}
                    disabled={readonly}
                  >
                    <MenuItem value="Low">Low</MenuItem>
                    <MenuItem value="Medium">Medium</MenuItem>
                    <MenuItem value="High">High</MenuItem>
                    <MenuItem value="Critical">Critical</MenuItem>
                  </Select>
                </FormControl>
              </Box>
            </Box>
          )}

          {/* Custom Script */}
          {config.actionType === 'custom' && (
            <TextField
              fullWidth
              size="small"
              label="Custom Script"
              value={config.customScript || ''}
              onChange={(e) => updateConfig({ customScript: e.target.value })}
              disabled={readonly}
              multiline
              rows={8}
              helperText="JavaScript expression. Access entity data via 'entity' object."
              sx={{ '& .MuiInputBase-input': { fontFamily: 'monospace', fontSize: 12 } }}
            />
          )}
        </Box>
      </TabPanel>

      {/* Field Updates Tab */}
      <TabPanel value={tabValue} index={1}>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="subtitle2">
              Field Updates
            </Typography>
            <Button
              size="small"
              startIcon={<AddIcon />}
              onClick={addFieldUpdate}
              disabled={readonly}
            >
              Add Field
            </Button>
          </Box>

          {(!config.fieldUpdates || config.fieldUpdates.length === 0) && (
            <Alert severity="info" sx={{ py: 0 }}>
              No field updates defined. Click "Add Field" to configure field values.
            </Alert>
          )}

          {(config.fieldUpdates || []).map((update, index) => {
            const selectedField = getField(update.field);
            
            return (
              <Card key={index} variant="outlined">
                <CardContent sx={{ pb: '8px !important' }}>
                  <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                    {/* Field selector */}
                    <FormControl size="small" sx={{ minWidth: 160 }}>
                      <InputLabel>Field</InputLabel>
                      <Select
                        value={update.field}
                        label="Field"
                        onChange={(e) => updateFieldUpdate(index, { field: e.target.value })}
                        disabled={readonly}
                      >
                        {Object.entries(groupedFields).map(([group, fields]) => [
                          <ListItem key={group} sx={{ fontWeight: 'bold', bgcolor: 'action.hover' }}>
                            {group}
                          </ListItem>,
                          ...fields.map(f => (
                            <MenuItem key={f.name} value={f.name}>
                              {f.label}
                              {f.required && <Chip label="req" size="small" color="error" sx={{ ml: 1 }} />}
                            </MenuItem>
                          ))
                        ])}
                      </Select>
                    </FormControl>

                    {/* Value Type selector */}
                    <FormControl size="small" sx={{ minWidth: 120 }}>
                      <InputLabel>Value Type</InputLabel>
                      <Select
                        value={update.valueType}
                        label="Value Type"
                        onChange={(e) => updateFieldUpdate(index, { valueType: e.target.value as FieldUpdate['valueType'] })}
                        disabled={readonly}
                      >
                        <MenuItem value="static">Static Value</MenuItem>
                        <MenuItem value="variable">Variable</MenuItem>
                        <MenuItem value="expression">Expression</MenuItem>
                        <MenuItem value="copy_from">Copy From Field</MenuItem>
                      </Select>
                    </FormControl>

                    {/* Value input - based on field type and value type */}
                    {update.valueType === 'copy_from' ? (
                      <FormControl size="small" sx={{ minWidth: 160, flex: 1 }}>
                        <InputLabel>Source Field</InputLabel>
                        <Select
                          value={update.sourceField || ''}
                          label="Source Field"
                          onChange={(e) => updateFieldUpdate(index, { sourceField: e.target.value })}
                          disabled={readonly}
                        >
                          {entityFields.map(f => (
                            <MenuItem key={f.name} value={f.name}>{f.label}</MenuItem>
                          ))}
                        </Select>
                      </FormControl>
                    ) : selectedField?.type === 'enum' && selectedField.enumValues && update.valueType === 'static' ? (
                      <FormControl size="small" sx={{ minWidth: 140, flex: 1 }}>
                        <InputLabel>Value</InputLabel>
                        <Select
                          value={update.value || ''}
                          label="Value"
                          onChange={(e) => updateFieldUpdate(index, { value: e.target.value })}
                          disabled={readonly}
                        >
                          {selectedField.enumValues.map(v => (
                            <MenuItem key={v} value={v}>{v}</MenuItem>
                          ))}
                        </Select>
                      </FormControl>
                    ) : selectedField?.type === 'boolean' && update.valueType === 'static' ? (
                      <FormControl size="small" sx={{ minWidth: 100, flex: 1 }}>
                        <InputLabel>Value</InputLabel>
                        <Select
                          value={update.value || ''}
                          label="Value"
                          onChange={(e) => updateFieldUpdate(index, { value: e.target.value })}
                          disabled={readonly}
                        >
                          <MenuItem value="true">True</MenuItem>
                          <MenuItem value="false">False</MenuItem>
                        </Select>
                      </FormControl>
                    ) : (
                      <TextField
                        size="small"
                        label={update.valueType === 'expression' ? 'Expression' : update.valueType === 'variable' ? 'Variable Name' : 'Value'}
                        value={update.value || ''}
                        onChange={(e) => updateFieldUpdate(index, { value: e.target.value })}
                        disabled={readonly}
                        sx={{ flex: 1, minWidth: 120 }}
                        type={selectedField?.type === 'number' && update.valueType === 'static' ? 'number' : 'text'}
                        placeholder={
                          update.valueType === 'expression' 
                            ? 'e.g., entity.price * 1.1' 
                            : update.valueType === 'variable' 
                            ? 'e.g., workflow.approved_by' 
                            : ''
                        }
                      />
                    )}

                    {/* Delete button */}
                    <IconButton
                      size="small"
                      onClick={() => removeFieldUpdate(index)}
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
              <strong>Value Types:</strong><br/>
              • <strong>Static:</strong> Fixed value<br/>
              • <strong>Variable:</strong> Value from workflow context (e.g., workflow.approved_by)<br/>
              • <strong>Expression:</strong> Calculated value (e.g., entity.amount * 1.1)<br/>
              • <strong>Copy From:</strong> Copy value from another field
            </Typography>
          </Alert>
        </Box>
      </TabPanel>

      {/* Related Records Tab */}
      <TabPanel value={tabValue} index={2}>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="subtitle2">
              Related Entity Updates
            </Typography>
            <Button
              size="small"
              startIcon={<AddIcon />}
              onClick={addRelatedEntityUpdate}
              disabled={readonly || relatedEntities.length === 0}
            >
              Add Related Update
            </Button>
          </Box>

          {relatedEntities.length === 0 && (
            <Alert severity="warning" sx={{ py: 0 }}>
              No related entities available for {entityType}
            </Alert>
          )}

          {relatedEntities.length > 0 && (
            <Box>
              <Typography variant="caption" color="text.secondary">
                Available Related Entities:
              </Typography>
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mt: 0.5 }}>
                {relatedEntities.map(rel => (
                  <Chip
                    key={rel.name}
                    icon={<RelationIcon fontSize="small" />}
                    label={`${rel.label} (${rel.relationType})`}
                    size="small"
                    variant="outlined"
                  />
                ))}
              </Box>
            </Box>
          )}

          {(config.relatedEntityUpdates || []).map((relUpdate, rIndex) => {
            const relatedFields = getRelatedEntityFields(relUpdate.relationship);
            
            return (
              <Card key={rIndex} variant="outlined">
                <CardHeader
                  title={
                    <FormControl size="small" fullWidth>
                      <InputLabel>Related Entity</InputLabel>
                      <Select
                        value={relUpdate.relationship}
                        label="Related Entity"
                        onChange={(e) => {
                          const updates = [...(config.relatedEntityUpdates || [])];
                          updates[rIndex] = { ...updates[rIndex], relationship: e.target.value };
                          updateConfig({ relatedEntityUpdates: updates });
                        }}
                        disabled={readonly}
                      >
                        {relatedEntities.map(rel => (
                          <MenuItem key={rel.name} value={rel.name}>{rel.label}</MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  }
                  action={
                    <IconButton
                      size="small"
                      onClick={() => {
                        const updates = (config.relatedEntityUpdates || []).filter((_, i) => i !== rIndex);
                        updateConfig({ relatedEntityUpdates: updates });
                      }}
                      disabled={readonly}
                    >
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  }
                />
                <CardContent>
                  {relUpdate.relationship && (
                    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                      <Button
                        size="small"
                        startIcon={<AddIcon />}
                        onClick={() => {
                          const updates = [...(config.relatedEntityUpdates || [])];
                          updates[rIndex].fieldUpdates.push({ field: '', valueType: 'static', value: '' });
                          updateConfig({ relatedEntityUpdates: updates });
                        }}
                        disabled={readonly}
                      >
                        Add Field Update
                      </Button>
                      {relUpdate.fieldUpdates.map((fu, fIndex) => (
                        <Box key={fIndex} sx={{ display: 'flex', gap: 1 }}>
                          <FormControl size="small" sx={{ minWidth: 140 }}>
                            <InputLabel>Field</InputLabel>
                            <Select
                              value={fu.field}
                              label="Field"
                              onChange={(e) => {
                                const updates = [...(config.relatedEntityUpdates || [])];
                                updates[rIndex].fieldUpdates[fIndex].field = e.target.value;
                                updateConfig({ relatedEntityUpdates: updates });
                              }}
                              disabled={readonly}
                            >
                              {relatedFields.map(f => (
                                <MenuItem key={f.name} value={f.name}>{f.label}</MenuItem>
                              ))}
                            </Select>
                          </FormControl>
                          <TextField
                            size="small"
                            label="Value"
                            value={fu.value}
                            onChange={(e) => {
                              const updates = [...(config.relatedEntityUpdates || [])];
                              updates[rIndex].fieldUpdates[fIndex].value = e.target.value;
                              updateConfig({ relatedEntityUpdates: updates });
                            }}
                            disabled={readonly}
                            sx={{ flex: 1 }}
                          />
                          <IconButton
                            size="small"
                            onClick={() => {
                              const updates = [...(config.relatedEntityUpdates || [])];
                              updates[rIndex].fieldUpdates = updates[rIndex].fieldUpdates.filter((_, i) => i !== fIndex);
                              updateConfig({ relatedEntityUpdates: updates });
                            }}
                            disabled={readonly}
                          >
                            <DeleteIcon fontSize="small" />
                          </IconButton>
                        </Box>
                      ))}
                    </Box>
                  )}
                </CardContent>
              </Card>
            );
          })}
        </Box>
      </TabPanel>

      {/* Error Handling Tab */}
      <TabPanel value={tabValue} index={3}>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              type="number"
              size="small"
              label="Retry Count"
              value={config.retryCount || 3}
              onChange={(e) => updateConfig({ retryCount: parseInt(e.target.value) || 0 })}
              disabled={readonly}
              inputProps={{ min: 0, max: 10 }}
              sx={{ flex: 1 }}
            />
            <TextField
              type="number"
              size="small"
              label="Retry Delay (seconds)"
              value={config.retryDelaySeconds || 30}
              onChange={(e) => updateConfig({ retryDelaySeconds: parseInt(e.target.value) || 30 })}
              disabled={readonly}
              sx={{ flex: 1 }}
            />
          </Box>

          <FormControl fullWidth size="small">
            <InputLabel>On Failure</InputLabel>
            <Select
              value={config.fallbackAction || 'fail'}
              label="On Failure"
              onChange={(e) => updateConfig({ fallbackAction: e.target.value as any })}
              disabled={readonly}
            >
              <MenuItem value="fail">Fail Workflow</MenuItem>
              <MenuItem value="skip">Skip This Action</MenuItem>
              <MenuItem value="default_value">Use Default Values</MenuItem>
            </Select>
          </FormControl>

          {/* Raw JSON editor */}
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

export default ActionPropertiesPanel;
