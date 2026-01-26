/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Enhanced Node Properties Panel - Advanced configuration for workflow nodes
 * Configuration is driven by backend API - no hardcoded dropdown options
 */

import React, { useState, useEffect } from 'react';
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
  CircularProgress,
} from '@mui/material';
import {
  ExpandMore as ExpandIcon,
  Delete as DeleteIcon,
  Add as AddIcon,
  Code as CodeIcon,
  Settings as SettingsIcon,
  Security as SecurityIcon,
  Psychology as LLMIcon,
  Error as ErrorIcon,
  Output as OutputIcon,
  Input as InputIcon,
  Help as HelpIcon,
} from '@mui/icons-material';
import { RuleBuilder, ConditionGroup, FieldDefinition, VariableDefinition, createDefaultRuleGroup } from './RuleBuilder';
import { 
  workflowService, 
  ActionTypeOption, 
  LLMModelOption, 
  EntityTypeOption,
  WorkflowConfig 
} from '../../services/workflowService';

// ============================================================================
// Types
// ============================================================================

export interface NodeConfiguration {
  // Basic
  actionType?: string;
  
  // Inputs
  inputs?: { key: string; value: string; valueType: 'static' | 'variable' | 'expression' }[];
  
  // Outputs
  outputMappings?: { key: string; path: string }[];
  
  // Conditions (for Condition nodes)
  conditionRules?: ConditionGroup;
  
  // LLM Settings
  llmModel?: string;
  llmPrompt?: string;
  llmTemperature?: number;
  llmMaxTokens?: number;
  llmJsonMode?: boolean;
  llmJsonSchema?: string;
  
  // Error Handling
  retryCount?: number;
  retryDelaySeconds?: number;
  useExponentialBackoff?: boolean;
  fallbackAction?: string;
  fallbackNodeKey?: string;
  
  // Permissions
  editPermission?: string;
  executePermission?: string;
  approvalRequired?: boolean;
  approvers?: string[];
  
  // Human Task specific
  title?: string;
  description?: string;
  formSchema?: any;
  assignToRole?: string;
  assignToUser?: string;
  dueInMinutes?: number;
  escalationRules?: any;
  
  // Wait/Timer specific
  waitMinutes?: number;
  waitUntilDate?: string;
  exitCondition?: string;
  
  // Webhook specific
  url?: string;
  method?: string;
  headers?: Record<string, string>;
  
  // Email specific
  to?: string;
  subject?: string;
  template?: string;
  
  // Generic
  [key: string]: any;
}

interface EnhancedPropertiesPanelProps {
  nodeId: number;
  nodeKey: string;
  nodeName: string;
  nodeType: string;
  configuration: string;
  timeoutMinutes: number;
  retryCount: number;
  isStartNode: boolean;
  isEndNode: boolean;
  onChange: (property: string, value: any) => void;
  onDelete: () => void;
  fields?: FieldDefinition[];
  variables?: VariableDefinition[];
  readonly?: boolean;
}

// ============================================================================
// Tab Panels
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

export const EnhancedPropertiesPanel: React.FC<EnhancedPropertiesPanelProps> = ({
  nodeId,
  nodeKey,
  nodeName,
  nodeType,
  configuration,
  timeoutMinutes,
  retryCount,
  isStartNode,
  isEndNode,
  onChange,
  onDelete,
  fields = [],
  variables = [],
  readonly = false,
}) => {
  const [tabValue, setTabValue] = useState(0);
  const [config, setConfig] = useState<NodeConfiguration>({});
  
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

  // Derived options from backend config
  const actionTypes = workflowConfig?.actionTypes || [];
  const llmModels = workflowConfig?.llmModels || [];
  const roles = workflowConfig?.roles.map(r => r.value) || [];
  const fallbackActions = workflowConfig?.fallbackActions || [];

  // Parse configuration on mount
  useEffect(() => {
    try {
      const parsed = configuration ? JSON.parse(configuration) : {};
      setConfig(parsed);
    } catch {
      setConfig({});
    }
  }, [configuration]);

  // Update configuration
  const updateConfig = (updates: Partial<NodeConfiguration>) => {
    const newConfig = { ...config, ...updates };
    setConfig(newConfig);
    onChange('configuration', JSON.stringify(newConfig));
  };

  // Render inputs section
  const renderInputs = () => (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
        <Typography variant="subtitle2">Input Variables</Typography>
        <Button
          size="small"
          startIcon={<AddIcon />}
          onClick={() => {
            const inputs = [...(config.inputs || []), { key: '', value: '', valueType: 'static' as const }];
            updateConfig({ inputs });
          }}
          disabled={readonly}
        >
          Add
        </Button>
      </Box>
      
      {(config.inputs || []).map((input, index) => (
        <Box key={index} sx={{ display: 'flex', gap: 1, mb: 1, alignItems: 'center' }}>
          <TextField
            size="small"
            label="Key"
            value={input.key}
            onChange={(e) => {
              const inputs = [...(config.inputs || [])];
              inputs[index] = { ...inputs[index], key: e.target.value };
              updateConfig({ inputs });
            }}
            disabled={readonly}
            sx={{ flex: 1 }}
          />
          <FormControl size="small" sx={{ minWidth: 80 }}>
            <Select
              value={input.valueType}
              onChange={(e) => {
                const inputs = [...(config.inputs || [])];
                inputs[index] = { ...inputs[index], valueType: e.target.value as any };
                updateConfig({ inputs });
              }}
              disabled={readonly}
            >
              <MenuItem value="static">Value</MenuItem>
              <MenuItem value="variable">Var</MenuItem>
              <MenuItem value="expression">Expr</MenuItem>
            </Select>
          </FormControl>
          <TextField
            size="small"
            label="Value"
            value={input.value}
            onChange={(e) => {
              const inputs = [...(config.inputs || [])];
              inputs[index] = { ...inputs[index], value: e.target.value };
              updateConfig({ inputs });
            }}
            disabled={readonly}
            sx={{ flex: 2 }}
          />
          <IconButton
            size="small"
            onClick={() => {
              const inputs = (config.inputs || []).filter((_, i) => i !== index);
              updateConfig({ inputs });
            }}
            disabled={readonly}
          >
            <DeleteIcon fontSize="small" />
          </IconButton>
        </Box>
      ))}

      {(config.inputs || []).length === 0 && (
        <Typography variant="body2" color="text.secondary" sx={{ py: 2, textAlign: 'center' }}>
          No input variables defined
        </Typography>
      )}
    </Box>
  );

  // Render outputs section
  const renderOutputs = () => (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
        <Typography variant="subtitle2">Output Mappings</Typography>
        <Button
          size="small"
          startIcon={<AddIcon />}
          onClick={() => {
            const outputMappings = [...(config.outputMappings || []), { key: '', path: '' }];
            updateConfig({ outputMappings });
          }}
          disabled={readonly}
        >
          Add
        </Button>
      </Box>

      {(config.outputMappings || []).map((output, index) => (
        <Box key={index} sx={{ display: 'flex', gap: 1, mb: 1, alignItems: 'center' }}>
          <TextField
            size="small"
            label="Variable Name"
            value={output.key}
            onChange={(e) => {
              const outputMappings = [...(config.outputMappings || [])];
              outputMappings[index] = { ...outputMappings[index], key: e.target.value };
              updateConfig({ outputMappings });
            }}
            disabled={readonly}
            sx={{ flex: 1 }}
          />
          <TextField
            size="small"
            label="Path (e.g., response.data.value)"
            value={output.path}
            onChange={(e) => {
              const outputMappings = [...(config.outputMappings || [])];
              outputMappings[index] = { ...outputMappings[index], path: e.target.value };
              updateConfig({ outputMappings });
            }}
            disabled={readonly}
            sx={{ flex: 2 }}
          />
          <IconButton
            size="small"
            onClick={() => {
              const outputMappings = (config.outputMappings || []).filter((_, i) => i !== index);
              updateConfig({ outputMappings });
            }}
            disabled={readonly}
          >
            <DeleteIcon fontSize="small" />
          </IconButton>
        </Box>
      ))}
    </Box>
  );

  // Render LLM settings
  const renderLLMSettings = () => (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      <FormControl fullWidth size="small">
        <InputLabel>Model</InputLabel>
        <Select
          value={config.llmModel || 'gpt-4'}
          label="Model"
          onChange={(e) => updateConfig({ llmModel: e.target.value })}
          disabled={readonly}
        >
          {llmModels.map((model) => (
            <MenuItem key={model.value} value={model.value}>
              {model.label}
            </MenuItem>
          ))}
        </Select>
      </FormControl>

      <TextField
        fullWidth
        multiline
        rows={6}
        size="small"
        label="Prompt Template"
        placeholder="Enter your prompt. Use {{variable}} for variable substitution."
        value={config.llmPrompt || ''}
        onChange={(e) => updateConfig({ llmPrompt: e.target.value })}
        disabled={readonly}
        helperText="Available variables will be substituted at runtime"
      />

      <Box sx={{ display: 'flex', gap: 2 }}>
        <TextField
          type="number"
          size="small"
          label="Temperature"
          value={config.llmTemperature ?? 0.7}
          onChange={(e) => updateConfig({ llmTemperature: parseFloat(e.target.value) })}
          disabled={readonly}
          inputProps={{ min: 0, max: 2, step: 0.1 }}
          sx={{ flex: 1 }}
        />
        <TextField
          type="number"
          size="small"
          label="Max Tokens"
          value={config.llmMaxTokens ?? 1000}
          onChange={(e) => updateConfig({ llmMaxTokens: parseInt(e.target.value) })}
          disabled={readonly}
          inputProps={{ min: 1, max: 128000 }}
          sx={{ flex: 1 }}
        />
      </Box>

      <FormControlLabel
        control={
          <Switch
            checked={config.llmJsonMode || false}
            onChange={(e) => updateConfig({ llmJsonMode: e.target.checked })}
            disabled={readonly}
          />
        }
        label="JSON Output Mode"
      />

      {config.llmJsonMode && (
        <TextField
          fullWidth
          multiline
          rows={4}
          size="small"
          label="JSON Schema (optional)"
          placeholder='{"type": "object", "properties": {...}}'
          value={config.llmJsonSchema || ''}
          onChange={(e) => updateConfig({ llmJsonSchema: e.target.value })}
          disabled={readonly}
          sx={{ '& .MuiInputBase-input': { fontFamily: 'monospace', fontSize: 12 } }}
        />
      )}
    </Box>
  );

  // Render error handling section
  const renderErrorHandling = () => (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      <Box sx={{ display: 'flex', gap: 2 }}>
        <TextField
          type="number"
          size="small"
          label="Retry Count"
          value={retryCount || config.retryCount || 3}
          onChange={(e) => {
            onChange('retryCount', parseInt(e.target.value));
            updateConfig({ retryCount: parseInt(e.target.value) });
          }}
          disabled={readonly}
          inputProps={{ min: 0, max: 10 }}
          sx={{ flex: 1 }}
        />
        <TextField
          type="number"
          size="small"
          label="Retry Delay (seconds)"
          value={config.retryDelaySeconds ?? 30}
          onChange={(e) => updateConfig({ retryDelaySeconds: parseInt(e.target.value) })}
          disabled={readonly}
          sx={{ flex: 1 }}
        />
      </Box>

      <FormControlLabel
        control={
          <Switch
            checked={config.useExponentialBackoff ?? true}
            onChange={(e) => updateConfig({ useExponentialBackoff: e.target.checked })}
            disabled={readonly}
          />
        }
        label="Use Exponential Backoff"
      />

      <FormControl fullWidth size="small">
        <InputLabel>Fallback Action</InputLabel>
        <Select
          value={config.fallbackAction || 'none'}
          label="Fallback Action"
          onChange={(e) => updateConfig({ fallbackAction: e.target.value })}
          disabled={readonly}
        >
          <MenuItem value="none">None (Fail Workflow)</MenuItem>
          <MenuItem value="skip">Skip This Node</MenuItem>
          <MenuItem value="goto">Go To Specific Node</MenuItem>
          <MenuItem value="default">Use Default Value</MenuItem>
          <MenuItem value="manual">Create Manual Task</MenuItem>
        </Select>
      </FormControl>

      {config.fallbackAction === 'goto' && (
        <TextField
          fullWidth
          size="small"
          label="Fallback Node Key"
          value={config.fallbackNodeKey || ''}
          onChange={(e) => updateConfig({ fallbackNodeKey: e.target.value })}
          disabled={readonly}
        />
      )}
    </Box>
  );

  // Render permissions section
  const renderPermissions = () => (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      <FormControl fullWidth size="small">
        <InputLabel>Edit Permission</InputLabel>
        <Select
          value={config.editPermission || 'Admin'}
          label="Edit Permission"
          onChange={(e) => updateConfig({ editPermission: e.target.value })}
          disabled={readonly}
        >
          {roles.map((role) => (
            <MenuItem key={role} value={role}>{role}</MenuItem>
          ))}
        </Select>
      </FormControl>

      <FormControl fullWidth size="small">
        <InputLabel>Execute Permission</InputLabel>
        <Select
          value={config.executePermission || 'System'}
          label="Execute Permission"
          onChange={(e) => updateConfig({ executePermission: e.target.value })}
          disabled={readonly}
        >
          <MenuItem value="System">System (Automatic)</MenuItem>
          {roles.map((role) => (
            <MenuItem key={role} value={role}>{role}</MenuItem>
          ))}
        </Select>
      </FormControl>

      <FormControlLabel
        control={
          <Switch
            checked={config.approvalRequired || false}
            onChange={(e) => updateConfig({ approvalRequired: e.target.checked })}
            disabled={readonly}
          />
        }
        label="Requires Approval"
      />

      {config.approvalRequired && (
        <Autocomplete
          multiple
          size="small"
          options={roles}
          value={config.approvers || []}
          onChange={(_, value) => updateConfig({ approvers: value })}
          disabled={readonly}
          renderInput={(params) => (
            <TextField {...params} label="Approvers (Roles)" />
          )}
        />
      )}
    </Box>
  );

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
    <Box sx={{ p: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
      {/* Header */}
      <Box>
        <Typography variant="h6">{nodeName}</Typography>
        <Box sx={{ display: 'flex', gap: 1, mt: 0.5 }}>
          <Chip label={nodeType} size="small" />
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

      <Box sx={{ display: 'flex', gap: 2 }}>
        <FormControlLabel
          control={
            <Switch
              checked={isStartNode}
              onChange={(e) => onChange('isStartNode', e.target.checked)}
              disabled={readonly}
            />
          }
          label="Start Node"
        />
        <FormControlLabel
          control={
            <Switch
              checked={isEndNode}
              onChange={(e) => onChange('isEndNode', e.target.checked)}
              disabled={readonly}
            />
          }
          label="End Node"
        />
      </Box>

      <TextField
        type="number"
        size="small"
        label="Timeout (minutes)"
        value={timeoutMinutes}
        onChange={(e) => onChange('timeoutMinutes', parseInt(e.target.value) || 0)}
        disabled={readonly}
        helperText="0 = no timeout"
      />

      <Divider />

      {/* Tabs for different configuration sections */}
      <Tabs
        value={tabValue}
        onChange={(_, v) => setTabValue(v)}
        variant="scrollable"
        scrollButtons="auto"
      >
        <Tab icon={<SettingsIcon fontSize="small" />} label="Config" iconPosition="start" />
        <Tab icon={<InputIcon fontSize="small" />} label="Inputs" iconPosition="start" />
        <Tab icon={<OutputIcon fontSize="small" />} label="Outputs" iconPosition="start" />
        {nodeType === 'LLMAction' && (
          <Tab icon={<LLMIcon fontSize="small" />} label="LLM" iconPosition="start" />
        )}
        <Tab icon={<ErrorIcon fontSize="small" />} label="Errors" iconPosition="start" />
        <Tab icon={<SecurityIcon fontSize="small" />} label="Permissions" iconPosition="start" />
      </Tabs>

      {/* Config Tab */}
      <TabPanel value={tabValue} index={0}>
        {nodeType === 'Action' && (
          <FormControl fullWidth size="small" sx={{ mb: 2 }}>
            <InputLabel>Action Type</InputLabel>
            <Select
              value={config.actionType || ''}
              label="Action Type"
              onChange={(e) => updateConfig({ actionType: e.target.value })}
              disabled={readonly}
            >
              {actionTypes.map((action) => (
                <MenuItem key={action.value} value={action.value}>
                  {action.label}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        )}

        {nodeType === 'Condition' && (
          <RuleBuilder
            value={config.conditionRules || createDefaultRuleGroup()}
            onChange={(rules) => updateConfig({ conditionRules: rules })}
            fields={fields}
            variables={variables}
            readonly={readonly}
          />
        )}

        {nodeType === 'Wait' && (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <TextField
              type="number"
              size="small"
              label="Wait Duration (minutes)"
              value={config.waitMinutes || 60}
              onChange={(e) => updateConfig({ waitMinutes: parseInt(e.target.value) })}
              disabled={readonly}
            />
            <TextField
              size="small"
              label="Exit Condition (optional)"
              placeholder="e.g., entity.status == 'Completed'"
              value={config.exitCondition || ''}
              onChange={(e) => updateConfig({ exitCondition: e.target.value })}
              disabled={readonly}
              helperText="Workflow continues early if this condition becomes true"
            />
          </Box>
        )}

        {nodeType === 'HumanTask' && (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <TextField
              fullWidth
              size="small"
              label="Task Title"
              value={config.title || ''}
              onChange={(e) => updateConfig({ title: e.target.value })}
              disabled={readonly}
            />
            <TextField
              fullWidth
              multiline
              rows={3}
              size="small"
              label="Description"
              value={config.description || ''}
              onChange={(e) => updateConfig({ description: e.target.value })}
              disabled={readonly}
            />
            <FormControl fullWidth size="small">
              <InputLabel>Assign To Role</InputLabel>
              <Select
                value={config.assignToRole || ''}
                label="Assign To Role"
                onChange={(e) => updateConfig({ assignToRole: e.target.value })}
                disabled={readonly}
              >
                {roles.map((role) => (
                  <MenuItem key={role} value={role}>{role}</MenuItem>
                ))}
              </Select>
            </FormControl>
            <TextField
              type="number"
              size="small"
              label="Due In (minutes)"
              value={config.dueInMinutes || 1440}
              onChange={(e) => updateConfig({ dueInMinutes: parseInt(e.target.value) })}
              disabled={readonly}
            />
          </Box>
        )}

        {/* Raw JSON editor for advanced config */}
        <Accordion sx={{ mt: 2 }}>
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
      </TabPanel>

      {/* Inputs Tab */}
      <TabPanel value={tabValue} index={1}>
        {renderInputs()}
      </TabPanel>

      {/* Outputs Tab */}
      <TabPanel value={tabValue} index={2}>
        {renderOutputs()}
      </TabPanel>

      {/* LLM Tab (conditional) */}
      {nodeType === 'LLMAction' && (
        <TabPanel value={tabValue} index={3}>
          {renderLLMSettings()}
        </TabPanel>
      )}

      {/* Errors Tab */}
      <TabPanel value={tabValue} index={nodeType === 'LLMAction' ? 4 : 3}>
        {renderErrorHandling()}
      </TabPanel>

      {/* Permissions Tab */}
      <TabPanel value={tabValue} index={nodeType === 'LLMAction' ? 5 : 4}>
        {renderPermissions()}
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

export default EnhancedPropertiesPanel;
