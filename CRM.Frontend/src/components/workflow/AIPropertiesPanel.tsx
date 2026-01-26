/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * AI Properties Panel - Configuration panel for AI-enhanced workflow nodes
 * Supports: AIDecision, AIAgent, AIContentGenerator, AIDataExtractor, AIClassifier, AISentimentAnalyzer, HumanReview
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
  Slider,
  Grid,
  Card,
  CardContent,
  CardHeader,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  ListItemSecondaryAction,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Badge,
  LinearProgress,
} from '@mui/material';
import {
  ExpandMore as ExpandIcon,
  Delete as DeleteIcon,
  Add as AddIcon,
  Code as CodeIcon,
  Settings as SettingsIcon,
  Psychology as AIIcon,
  SmartToy as AgentIcon,
  AutoAwesome as ContentIcon,
  DataObject as ExtractorIcon,
  Category as ClassifierIcon,
  SentimentSatisfied as SentimentIcon,
  RateReview as ReviewIcon,
  Route as DecisionIcon,
  Help as HelpIcon,
  PlayArrow as TestIcon,
  AttachMoney as CostIcon,
  Speed as SpeedIcon,
  Memory as MemoryIcon,
  Build as ToolIcon,
  ContentCopy as CopyIcon,
  Visibility as PreviewIcon,
  Token as TokenIcon,
  Warning as WarningIcon,
  CheckCircle as SuccessIcon,
} from '@mui/icons-material';
import {
  workflowService,
  LLMModelOption,
  AIDecisionConfig,
  AIAgentConfig,
  AIContentGeneratorConfig,
  AIDataExtractorConfig,
  AIClassifierConfig,
  AISentimentAnalyzerConfig,
  HumanReviewConfig,
  AIToolDefinition,
  AIDecisionOption,
  AIClassifierCategory,
  AIExtractionField,
  HumanReviewOption,
} from '../../services/workflowService';

// ============================================================================
// Types
// ============================================================================

interface AIPropertiesPanelProps {
  nodeId: number;
  nodeKey: string;
  nodeName: string;
  nodeType: string;
  configuration: string;
  onChange: (property: string, value: any) => void;
  onDelete: () => void;
  variables?: string[];
  readonly?: boolean;
}

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
// Helper Components
// ============================================================================

interface ModelSelectorProps {
  value: string;
  onChange: (value: string) => void;
  models: LLMModelOption[];
  disabled?: boolean;
}

const ModelSelector: React.FC<ModelSelectorProps> = ({ value, onChange, models, disabled }) => (
  <FormControl fullWidth size="small">
    <InputLabel>AI Model</InputLabel>
    <Select
      value={value}
      label="AI Model"
      onChange={(e) => onChange(e.target.value)}
      disabled={disabled}
    >
      {models.map((model) => (
        <MenuItem key={model.value} value={model.value}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Typography>{model.label}</Typography>
            {model.isDefault && (
              <Chip label="Default" size="small" color="primary" />
            )}
          </Box>
        </MenuItem>
      ))}
    </Select>
  </FormControl>
);

interface TemperatureSliderProps {
  value: number;
  onChange: (value: number) => void;
  disabled?: boolean;
}

const TemperatureSlider: React.FC<TemperatureSliderProps> = ({ value, onChange, disabled }) => (
  <Box>
    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
      <Typography variant="body2">Temperature</Typography>
      <Chip label={value.toFixed(1)} size="small" />
    </Box>
    <Slider
      value={value}
      onChange={(_, v) => onChange(v as number)}
      min={0}
      max={2}
      step={0.1}
      disabled={disabled}
      marks={[
        { value: 0, label: 'Precise' },
        { value: 1, label: 'Balanced' },
        { value: 2, label: 'Creative' },
      ]}
    />
  </Box>
);

interface PromptEditorProps {
  label: string;
  value: string;
  onChange: (value: string) => void;
  variables?: string[];
  placeholder?: string;
  rows?: number;
  disabled?: boolean;
}

const PromptEditor: React.FC<PromptEditorProps> = ({
  label,
  value,
  onChange,
  variables = [],
  placeholder,
  rows = 6,
  disabled,
}) => {
  const [tokenCount, setTokenCount] = useState(0);

  useEffect(() => {
    // Rough token estimation (4 chars per token on average)
    setTokenCount(Math.ceil(value.length / 4));
  }, [value]);

  const insertVariable = (variable: string) => {
    onChange(value + `{{${variable}}}`);
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
        <Typography variant="body2" fontWeight="medium">{label}</Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <TokenIcon fontSize="small" color="action" />
          <Typography variant="caption" color="text.secondary">
            ~{tokenCount} tokens
          </Typography>
        </Box>
      </Box>
      <TextField
        fullWidth
        multiline
        rows={rows}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        disabled={disabled}
        sx={{
          '& .MuiInputBase-input': {
            fontFamily: 'monospace',
            fontSize: 13,
          },
        }}
      />
      {variables.length > 0 && (
        <Box sx={{ mt: 1 }}>
          <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 0.5 }}>
            Available Variables:
          </Typography>
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
            {variables.map((v) => (
              <Chip
                key={v}
                label={`{{${v}}}`}
                size="small"
                variant="outlined"
                onClick={() => insertVariable(v)}
                sx={{ cursor: 'pointer', fontFamily: 'monospace', fontSize: 11 }}
              />
            ))}
          </Box>
        </Box>
      )}
    </Box>
  );
};

interface CostEstimatorProps {
  model: string;
  inputTokens: number;
  outputTokens: number;
}

const CostEstimator: React.FC<CostEstimatorProps> = ({ model, inputTokens, outputTokens }) => {
  // Rough cost estimation based on model
  const getCostPer1k = (modelName: string): { input: number; output: number } => {
    if (modelName.includes('gpt-4o')) return { input: 0.005, output: 0.015 };
    if (modelName.includes('gpt-4')) return { input: 0.03, output: 0.06 };
    if (modelName.includes('gpt-3.5')) return { input: 0.0005, output: 0.0015 };
    if (modelName.includes('claude-3-opus')) return { input: 0.015, output: 0.075 };
    if (modelName.includes('claude-3-sonnet')) return { input: 0.003, output: 0.015 };
    if (modelName.includes('claude-3-haiku')) return { input: 0.00025, output: 0.00125 };
    return { input: 0.001, output: 0.002 }; // Default estimate
  };

  const rates = getCostPer1k(model);
  const inputCost = (inputTokens / 1000) * rates.input;
  const outputCost = (outputTokens / 1000) * rates.output;
  const totalCost = inputCost + outputCost;

  return (
    <Paper variant="outlined" sx={{ p: 2, bgcolor: 'background.default' }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
        <CostIcon fontSize="small" color="primary" />
        <Typography variant="subtitle2">Estimated Cost per Execution</Typography>
      </Box>
      <Grid container spacing={2}>
        <Grid item xs={4}>
          <Typography variant="caption" color="text.secondary">Input</Typography>
          <Typography variant="body2">${inputCost.toFixed(4)}</Typography>
        </Grid>
        <Grid item xs={4}>
          <Typography variant="caption" color="text.secondary">Output</Typography>
          <Typography variant="body2">${outputCost.toFixed(4)}</Typography>
        </Grid>
        <Grid item xs={4}>
          <Typography variant="caption" color="text.secondary">Total</Typography>
          <Typography variant="body2" fontWeight="bold" color="primary">
            ${totalCost.toFixed(4)}
          </Typography>
        </Grid>
      </Grid>
    </Paper>
  );
};

// ============================================================================
// AI Decision Node Panel
// ============================================================================

interface AIDecisionPanelProps {
  config: AIDecisionConfig;
  onChange: (config: AIDecisionConfig) => void;
  models: LLMModelOption[];
  variables: string[];
  disabled?: boolean;
}

const AIDecisionPanel: React.FC<AIDecisionPanelProps> = ({
  config,
  onChange,
  models,
  variables,
  disabled,
}) => {
  const [newOption, setNewOption] = useState({ id: '', label: '', description: '', criteria: '' });
  const [addingOption, setAddingOption] = useState(false);

  const updateOption = (index: number, updates: Partial<AIDecisionOption>) => {
    const options = [...config.decisionOptions];
    options[index] = { ...options[index], ...updates };
    onChange({ ...config, decisionOptions: options });
  };

  const removeOption = (index: number) => {
    const options = config.decisionOptions.filter((_, i) => i !== index);
    onChange({ ...config, decisionOptions: options });
  };

  const addOption = () => {
    if (!newOption.id || !newOption.label) return;
    onChange({
      ...config,
      decisionOptions: [...config.decisionOptions, newOption as AIDecisionOption],
    });
    setNewOption({ id: '', label: '', description: '', criteria: '' });
    setAddingOption(false);
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      {/* Model Settings */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <DecisionIcon fontSize="small" />
          Model Settings
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <ModelSelector
              value={config.model}
              onChange={(model) => onChange({ ...config, model })}
              models={models}
              disabled={disabled}
            />
          </Grid>
          <Grid item xs={12}>
            <TemperatureSlider
              value={config.temperature}
              onChange={(temperature) => onChange({ ...config, temperature })}
              disabled={disabled}
            />
          </Grid>
          <Grid item xs={6}>
            <TextField
              fullWidth
              size="small"
              type="number"
              label="Max Tokens"
              value={config.maxTokens}
              onChange={(e) => onChange({ ...config, maxTokens: parseInt(e.target.value) || 500 })}
              disabled={disabled}
            />
          </Grid>
          <Grid item xs={6}>
            <TextField
              fullWidth
              size="small"
              type="number"
              label="Confidence Threshold"
              value={config.confidenceThreshold}
              onChange={(e) => onChange({ ...config, confidenceThreshold: parseFloat(e.target.value) || 0.7 })}
              disabled={disabled}
              inputProps={{ min: 0, max: 1, step: 0.05 }}
            />
          </Grid>
        </Grid>
      </Paper>

      {/* Prompts */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Decision Prompt</Typography>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          <PromptEditor
            label="System Prompt"
            value={config.systemPrompt}
            onChange={(systemPrompt) => onChange({ ...config, systemPrompt })}
            placeholder="You are an AI assistant that analyzes content and makes routing decisions..."
            rows={4}
            disabled={disabled}
          />
          <PromptEditor
            label="User Prompt Template"
            value={config.userPromptTemplate}
            onChange={(userPromptTemplate) => onChange({ ...config, userPromptTemplate })}
            variables={variables}
            placeholder="Analyze the following and determine the best routing option: {{input}}"
            rows={4}
            disabled={disabled}
          />
        </Box>
      </Paper>

      {/* Decision Options */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="subtitle2">Decision Options</Typography>
          <Button
            size="small"
            startIcon={<AddIcon />}
            onClick={() => setAddingOption(true)}
            disabled={disabled}
          >
            Add Option
          </Button>
        </Box>

        {config.decisionOptions.map((option, index) => (
          <Card key={option.id} variant="outlined" sx={{ mb: 1 }}>
            <CardContent sx={{ py: 1, '&:last-child': { pb: 1 } }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <Box sx={{ flex: 1 }}>
                  <TextField
                    fullWidth
                    size="small"
                    label="Label"
                    value={option.label}
                    onChange={(e) => updateOption(index, { label: e.target.value })}
                    disabled={disabled}
                    sx={{ mb: 1 }}
                  />
                  <TextField
                    fullWidth
                    size="small"
                    multiline
                    rows={2}
                    label="Criteria"
                    value={option.criteria}
                    onChange={(e) => updateOption(index, { criteria: e.target.value })}
                    disabled={disabled}
                    placeholder="When to route to this option..."
                  />
                </Box>
                <IconButton
                  size="small"
                  onClick={() => removeOption(index)}
                  disabled={disabled}
                  sx={{ ml: 1 }}
                >
                  <DeleteIcon fontSize="small" />
                </IconButton>
              </Box>
            </CardContent>
          </Card>
        ))}

        {addingOption && (
          <Card variant="outlined" sx={{ mb: 1, bgcolor: 'action.hover' }}>
            <CardContent sx={{ py: 1, '&:last-child': { pb: 1 } }}>
              <Grid container spacing={1}>
                <Grid item xs={6}>
                  <TextField
                    fullWidth
                    size="small"
                    label="ID"
                    value={newOption.id}
                    onChange={(e) => setNewOption({ ...newOption, id: e.target.value })}
                  />
                </Grid>
                <Grid item xs={6}>
                  <TextField
                    fullWidth
                    size="small"
                    label="Label"
                    value={newOption.label}
                    onChange={(e) => setNewOption({ ...newOption, label: e.target.value })}
                  />
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    size="small"
                    label="Criteria"
                    value={newOption.criteria}
                    onChange={(e) => setNewOption({ ...newOption, criteria: e.target.value })}
                  />
                </Grid>
                <Grid item xs={12}>
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    <Button size="small" onClick={addOption} variant="contained">
                      Add
                    </Button>
                    <Button size="small" onClick={() => setAddingOption(false)}>
                      Cancel
                    </Button>
                  </Box>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        )}

        {config.decisionOptions.length === 0 && !addingOption && (
          <Typography variant="body2" color="text.secondary" textAlign="center" sx={{ py: 2 }}>
            No decision options defined. Add options to configure routing paths.
          </Typography>
        )}
      </Paper>

      {/* Output */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Output Configuration</Typography>
        <TextField
          fullWidth
          size="small"
          label="Output Variable"
          value={config.outputVariable}
          onChange={(e) => onChange({ ...config, outputVariable: e.target.value })}
          disabled={disabled}
          helperText="Variable to store the decision result"
        />
      </Paper>

      {/* Cost Estimation */}
      <CostEstimator
        model={config.model}
        inputTokens={Math.ceil((config.systemPrompt.length + config.userPromptTemplate.length) / 4)}
        outputTokens={100}
      />
    </Box>
  );
};

// ============================================================================
// AI Agent Node Panel
// ============================================================================

interface AIAgentPanelProps {
  config: AIAgentConfig;
  onChange: (config: AIAgentConfig) => void;
  models: LLMModelOption[];
  tools: AIToolDefinition[];
  variables: string[];
  disabled?: boolean;
}

const AIAgentPanel: React.FC<AIAgentPanelProps> = ({
  config,
  onChange,
  models,
  tools,
  variables,
  disabled,
}) => {
  const toggleTool = (toolId: string) => {
    const currentTools = config.availableTools;
    const newTools = currentTools.includes(toolId)
      ? currentTools.filter((t) => t !== toolId)
      : [...currentTools, toolId];
    onChange({ ...config, availableTools: newTools });
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      {/* Agent Identity */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <AgentIcon fontSize="small" />
          Agent Identity
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <TextField
              fullWidth
              size="small"
              label="Agent Name"
              value={config.agentName}
              onChange={(e) => onChange({ ...config, agentName: e.target.value })}
              disabled={disabled}
              placeholder="Customer Support Agent"
            />
          </Grid>
          <Grid item xs={12}>
            <TextField
              fullWidth
              size="small"
              multiline
              rows={2}
              label="Agent Description"
              value={config.agentDescription}
              onChange={(e) => onChange({ ...config, agentDescription: e.target.value })}
              disabled={disabled}
              placeholder="An AI agent that helps resolve customer inquiries..."
            />
          </Grid>
          <Grid item xs={12}>
            <PromptEditor
              label="System Prompt"
              value={config.systemPrompt}
              onChange={(systemPrompt) => onChange({ ...config, systemPrompt })}
              variables={variables}
              placeholder="You are a helpful AI agent..."
              rows={4}
              disabled={disabled}
            />
          </Grid>
        </Grid>
      </Paper>

      {/* Model Settings */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Model Settings</Typography>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <ModelSelector
              value={config.model}
              onChange={(model) => onChange({ ...config, model })}
              models={models}
              disabled={disabled}
            />
          </Grid>
          <Grid item xs={12}>
            <TemperatureSlider
              value={config.temperature}
              onChange={(temperature) => onChange({ ...config, temperature })}
              disabled={disabled}
            />
          </Grid>
          <Grid item xs={6}>
            <TextField
              fullWidth
              size="small"
              type="number"
              label="Max Tokens"
              value={config.maxTokens}
              onChange={(e) => onChange({ ...config, maxTokens: parseInt(e.target.value) || 1000 })}
              disabled={disabled}
            />
          </Grid>
          <Grid item xs={6}>
            <TextField
              fullWidth
              size="small"
              type="number"
              label="Max Iterations"
              value={config.maxIterations}
              onChange={(e) => onChange({ ...config, maxIterations: parseInt(e.target.value) || 10 })}
              disabled={disabled}
              helperText="Max tool calls"
            />
          </Grid>
        </Grid>
      </Paper>

      {/* Tool Access */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="subtitle2" sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <ToolIcon fontSize="small" />
            Available Tools
          </Typography>
          <Chip
            label={`${config.availableTools.length} selected`}
            size="small"
            color="primary"
          />
        </Box>
        <Grid container spacing={1}>
          {tools.map((tool) => (
            <Grid item xs={6} key={tool.id}>
              <Card
                variant="outlined"
                sx={{
                  cursor: 'pointer',
                  bgcolor: config.availableTools.includes(tool.id) ? 'action.selected' : 'transparent',
                  '&:hover': { bgcolor: 'action.hover' },
                }}
                onClick={() => !disabled && toggleTool(tool.id)}
              >
                <CardContent sx={{ py: 1, '&:last-child': { pb: 1 } }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <ToolIcon fontSize="small" color={config.availableTools.includes(tool.id) ? 'primary' : 'action'} />
                    <Box sx={{ flex: 1 }}>
                      <Typography variant="body2" fontWeight="medium">{tool.name}</Typography>
                      <Typography variant="caption" color="text.secondary" noWrap>
                        {tool.description}
                      </Typography>
                    </Box>
                  </Box>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
        <FormControlLabel
          control={
            <Switch
              checked={config.toolApprovalRequired}
              onChange={(e) => onChange({ ...config, toolApprovalRequired: e.target.checked })}
              disabled={disabled}
            />
          }
          label="Require approval for tool execution"
          sx={{ mt: 2 }}
        />
      </Paper>

      {/* Autonomy Settings */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Autonomy Settings</Typography>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <FormControl fullWidth size="small">
              <InputLabel>Autonomy Level</InputLabel>
              <Select
                value={config.autonomyLevel}
                label="Autonomy Level"
                onChange={(e) => onChange({ ...config, autonomyLevel: e.target.value as any })}
                disabled={disabled}
              >
                <MenuItem value="low">Low - Requires approval for all actions</MenuItem>
                <MenuItem value="medium">Medium - Auto for reads, approval for writes</MenuItem>
                <MenuItem value="high">High - Auto for most, approval for critical</MenuItem>
                <MenuItem value="full">Full - Autonomous operation</MenuItem>
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={6}>
            <FormControlLabel
              control={
                <Switch
                  checked={config.canModifyData}
                  onChange={(e) => onChange({ ...config, canModifyData: e.target.checked })}
                  disabled={disabled}
                />
              }
              label="Can modify data"
            />
          </Grid>
          <Grid item xs={6}>
            <FormControlLabel
              control={
                <Switch
                  checked={config.canSendCommunications}
                  onChange={(e) => onChange({ ...config, canSendCommunications: e.target.checked })}
                  disabled={disabled}
                />
              }
              label="Can send communications"
            />
          </Grid>
        </Grid>
      </Paper>

      {/* Memory Settings */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <MemoryIcon fontSize="small" />
          Memory Settings
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <FormControlLabel
              control={
                <Switch
                  checked={config.enableMemory}
                  onChange={(e) => onChange({ ...config, enableMemory: e.target.checked })}
                  disabled={disabled}
                />
              }
              label="Enable agent memory"
            />
          </Grid>
          {config.enableMemory && (
            <>
              <Grid item xs={6}>
                <FormControl fullWidth size="small">
                  <InputLabel>Memory Type</InputLabel>
                  <Select
                    value={config.memoryType}
                    label="Memory Type"
                    onChange={(e) => onChange({ ...config, memoryType: e.target.value as any })}
                    disabled={disabled}
                  >
                    <MenuItem value="conversation">Conversation History</MenuItem>
                    <MenuItem value="summary">Summary Memory</MenuItem>
                    <MenuItem value="vector">Vector Memory</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={6}>
                <TextField
                  fullWidth
                  size="small"
                  type="number"
                  label="Max Memory Items"
                  value={config.maxMemoryItems}
                  onChange={(e) => onChange({ ...config, maxMemoryItems: parseInt(e.target.value) || 10 })}
                  disabled={disabled}
                />
              </Grid>
            </>
          )}
        </Grid>
      </Paper>

      {/* Goals and Constraints */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Goals & Constraints</Typography>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <TextField
              fullWidth
              size="small"
              multiline
              rows={2}
              label="Primary Goal"
              value={config.primaryGoal}
              onChange={(e) => onChange({ ...config, primaryGoal: e.target.value })}
              disabled={disabled}
              placeholder="Resolve customer inquiry and ensure satisfaction..."
            />
          </Grid>
          <Grid item xs={12}>
            <Autocomplete
              multiple
              freeSolo
              options={[]}
              value={config.constraints}
              onChange={(_, value) => onChange({ ...config, constraints: value })}
              disabled={disabled}
              renderInput={(params) => (
                <TextField {...params} size="small" label="Constraints" placeholder="Add constraint..." />
              )}
              renderTags={(value, getTagProps) =>
                value.map((option, index) => (
                  <Chip variant="outlined" label={option} size="small" {...getTagProps({ index })} />
                ))
              }
            />
          </Grid>
        </Grid>
      </Paper>

      {/* Budget Controls */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <CostIcon fontSize="small" />
          Budget Controls
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={6}>
            <TextField
              fullWidth
              size="small"
              type="number"
              label="Max Cost ($)"
              value={config.maxCost || ''}
              onChange={(e) => onChange({ ...config, maxCost: parseFloat(e.target.value) || undefined })}
              disabled={disabled}
              placeholder="No limit"
            />
          </Grid>
          <Grid item xs={6}>
            <TextField
              fullWidth
              size="small"
              type="number"
              label="Max API Calls"
              value={config.maxApiCalls || ''}
              onChange={(e) => onChange({ ...config, maxApiCalls: parseInt(e.target.value) || undefined })}
              disabled={disabled}
              placeholder="No limit"
            />
          </Grid>
        </Grid>
      </Paper>
    </Box>
  );
};

// ============================================================================
// AI Content Generator Panel
// ============================================================================

interface AIContentGeneratorPanelProps {
  config: AIContentGeneratorConfig;
  onChange: (config: AIContentGeneratorConfig) => void;
  models: LLMModelOption[];
  variables: string[];
  disabled?: boolean;
}

const AIContentGeneratorPanel: React.FC<AIContentGeneratorPanelProps> = ({
  config,
  onChange,
  models,
  variables,
  disabled,
}) => {
  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      {/* Content Type */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <ContentIcon fontSize="small" />
          Content Settings
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={6}>
            <FormControl fullWidth size="small">
              <InputLabel>Content Type</InputLabel>
              <Select
                value={config.contentType}
                label="Content Type"
                onChange={(e) => onChange({ ...config, contentType: e.target.value as any })}
                disabled={disabled}
              >
                <MenuItem value="email">Email</MenuItem>
                <MenuItem value="summary">Summary</MenuItem>
                <MenuItem value="report">Report</MenuItem>
                <MenuItem value="response">Response</MenuItem>
                <MenuItem value="document">Document</MenuItem>
                <MenuItem value="custom">Custom</MenuItem>
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={6}>
            <FormControl fullWidth size="small">
              <InputLabel>Tone</InputLabel>
              <Select
                value={config.tone}
                label="Tone"
                onChange={(e) => onChange({ ...config, tone: e.target.value as any })}
                disabled={disabled}
              >
                <MenuItem value="professional">Professional</MenuItem>
                <MenuItem value="friendly">Friendly</MenuItem>
                <MenuItem value="formal">Formal</MenuItem>
                <MenuItem value="casual">Casual</MenuItem>
                <MenuItem value="empathetic">Empathetic</MenuItem>
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={6}>
            <FormControl fullWidth size="small">
              <InputLabel>Output Format</InputLabel>
              <Select
                value={config.outputFormat}
                label="Output Format"
                onChange={(e) => onChange({ ...config, outputFormat: e.target.value as any })}
                disabled={disabled}
              >
                <MenuItem value="text">Plain Text</MenuItem>
                <MenuItem value="html">HTML</MenuItem>
                <MenuItem value="markdown">Markdown</MenuItem>
                <MenuItem value="json">JSON</MenuItem>
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={6}>
            <TextField
              fullWidth
              size="small"
              label="Language"
              value={config.language}
              onChange={(e) => onChange({ ...config, language: e.target.value })}
              disabled={disabled}
              placeholder="English"
            />
          </Grid>
        </Grid>
      </Paper>

      {/* Model Settings */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Model Settings</Typography>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <ModelSelector
              value={config.model}
              onChange={(model) => onChange({ ...config, model })}
              models={models}
              disabled={disabled}
            />
          </Grid>
          <Grid item xs={12}>
            <TemperatureSlider
              value={config.temperature}
              onChange={(temperature) => onChange({ ...config, temperature })}
              disabled={disabled}
            />
          </Grid>
          <Grid item xs={6}>
            <TextField
              fullWidth
              size="small"
              type="number"
              label="Max Tokens"
              value={config.maxTokens}
              onChange={(e) => onChange({ ...config, maxTokens: parseInt(e.target.value) || 1000 })}
              disabled={disabled}
            />
          </Grid>
          <Grid item xs={6}>
            <TextField
              fullWidth
              size="small"
              type="number"
              label="Max Length (chars)"
              value={config.maxLength || ''}
              onChange={(e) => onChange({ ...config, maxLength: parseInt(e.target.value) || undefined })}
              disabled={disabled}
              placeholder="No limit"
            />
          </Grid>
        </Grid>
      </Paper>

      {/* Prompts */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Content Generation Prompt</Typography>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          <PromptEditor
            label="System Prompt"
            value={config.systemPrompt}
            onChange={(systemPrompt) => onChange({ ...config, systemPrompt })}
            placeholder="You are a professional content writer..."
            rows={4}
            disabled={disabled}
          />
          <PromptEditor
            label="User Prompt Template"
            value={config.userPromptTemplate}
            onChange={(userPromptTemplate) => onChange({ ...config, userPromptTemplate })}
            variables={variables}
            placeholder="Generate a {{contentType}} about {{topic}}..."
            rows={4}
            disabled={disabled}
          />
        </Box>
      </Paper>

      {/* Input Variables */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Input Variables</Typography>
        <Autocomplete
          multiple
          options={variables}
          value={config.inputVariables}
          onChange={(_, value) => onChange({ ...config, inputVariables: value })}
          disabled={disabled}
          renderInput={(params) => (
            <TextField {...params} size="small" placeholder="Select input variables..." />
          )}
          renderTags={(value, getTagProps) =>
            value.map((option, index) => (
              <Chip variant="outlined" label={option} size="small" {...getTagProps({ index })} />
            ))
          }
        />
      </Paper>

      {/* Output */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Output Configuration</Typography>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <TextField
              fullWidth
              size="small"
              label="Output Variable"
              value={config.outputVariable}
              onChange={(e) => onChange({ ...config, outputVariable: e.target.value })}
              disabled={disabled}
            />
          </Grid>
          <Grid item xs={12}>
            <FormControlLabel
              control={
                <Switch
                  checked={config.requiresReview}
                  onChange={(e) => onChange({ ...config, requiresReview: e.target.checked })}
                  disabled={disabled}
                />
              }
              label="Requires human review before use"
            />
          </Grid>
        </Grid>
      </Paper>

      {/* Cost Estimation */}
      <CostEstimator
        model={config.model}
        inputTokens={Math.ceil((config.systemPrompt.length + config.userPromptTemplate.length) / 4)}
        outputTokens={config.maxTokens}
      />
    </Box>
  );
};

// ============================================================================
// AI Data Extractor Panel
// ============================================================================

interface AIDataExtractorPanelProps {
  config: AIDataExtractorConfig;
  onChange: (config: AIDataExtractorConfig) => void;
  models: LLMModelOption[];
  variables: string[];
  disabled?: boolean;
}

const AIDataExtractorPanel: React.FC<AIDataExtractorPanelProps> = ({
  config,
  onChange,
  models,
  variables,
  disabled,
}) => {
  const [addingField, setAddingField] = useState(false);
  const [newField, setNewField] = useState<AIExtractionField>({
    name: '',
    type: 'string',
    description: '',
    required: false,
  });

  const addField = () => {
    if (!newField.name) return;
    onChange({
      ...config,
      extractionSchema: [...config.extractionSchema, newField],
    });
    setNewField({ name: '', type: 'string', description: '', required: false });
    setAddingField(false);
  };

  const updateField = (index: number, updates: Partial<AIExtractionField>) => {
    const fields = [...config.extractionSchema];
    fields[index] = { ...fields[index], ...updates };
    onChange({ ...config, extractionSchema: fields });
  };

  const removeField = (index: number) => {
    const fields = config.extractionSchema.filter((_, i) => i !== index);
    onChange({ ...config, extractionSchema: fields });
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      {/* Model Settings */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <ExtractorIcon fontSize="small" />
          Model Settings
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <ModelSelector
              value={config.model}
              onChange={(model) => onChange({ ...config, model })}
              models={models}
              disabled={disabled}
            />
          </Grid>
          <Grid item xs={6}>
            <TextField
              fullWidth
              size="small"
              type="number"
              label="Max Tokens"
              value={config.maxTokens}
              onChange={(e) => onChange({ ...config, maxTokens: parseInt(e.target.value) || 500 })}
              disabled={disabled}
            />
          </Grid>
          <Grid item xs={6}>
            <FormControl fullWidth size="small">
              <InputLabel>Output Format</InputLabel>
              <Select
                value={config.outputFormat}
                label="Output Format"
                onChange={(e) => onChange({ ...config, outputFormat: e.target.value as any })}
                disabled={disabled}
              >
                <MenuItem value="object">Object</MenuItem>
                <MenuItem value="array">Array</MenuItem>
              </Select>
            </FormControl>
          </Grid>
        </Grid>
      </Paper>

      {/* Input */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Input Configuration</Typography>
        <FormControl fullWidth size="small">
          <InputLabel>Input Variable</InputLabel>
          <Select
            value={config.inputVariable}
            label="Input Variable"
            onChange={(e) => onChange({ ...config, inputVariable: e.target.value })}
            disabled={disabled}
          >
            {variables.map((v) => (
              <MenuItem key={v} value={v}>{v}</MenuItem>
            ))}
          </Select>
        </FormControl>
      </Paper>

      {/* Extraction Schema */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="subtitle2">Extraction Schema</Typography>
          <Button
            size="small"
            startIcon={<AddIcon />}
            onClick={() => setAddingField(true)}
            disabled={disabled}
          >
            Add Field
          </Button>
        </Box>

        {config.extractionSchema.map((field, index) => (
          <Card key={field.name} variant="outlined" sx={{ mb: 1 }}>
            <CardContent sx={{ py: 1, '&:last-child': { pb: 1 } }}>
              <Grid container spacing={1} alignItems="center">
                <Grid item xs={3}>
                  <TextField
                    fullWidth
                    size="small"
                    label="Name"
                    value={field.name}
                    onChange={(e) => updateField(index, { name: e.target.value })}
                    disabled={disabled}
                  />
                </Grid>
                <Grid item xs={2}>
                  <FormControl fullWidth size="small">
                    <InputLabel>Type</InputLabel>
                    <Select
                      value={field.type}
                      label="Type"
                      onChange={(e) => updateField(index, { type: e.target.value as any })}
                      disabled={disabled}
                    >
                      <MenuItem value="string">String</MenuItem>
                      <MenuItem value="number">Number</MenuItem>
                      <MenuItem value="boolean">Boolean</MenuItem>
                      <MenuItem value="date">Date</MenuItem>
                      <MenuItem value="email">Email</MenuItem>
                      <MenuItem value="phone">Phone</MenuItem>
                      <MenuItem value="address">Address</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
                <Grid item xs={4}>
                  <TextField
                    fullWidth
                    size="small"
                    label="Description"
                    value={field.description}
                    onChange={(e) => updateField(index, { description: e.target.value })}
                    disabled={disabled}
                  />
                </Grid>
                <Grid item xs={2}>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={field.required}
                        onChange={(e) => updateField(index, { required: e.target.checked })}
                        disabled={disabled}
                        size="small"
                      />
                    }
                    label="Req"
                  />
                </Grid>
                <Grid item xs={1}>
                  <IconButton size="small" onClick={() => removeField(index)} disabled={disabled}>
                    <DeleteIcon fontSize="small" />
                  </IconButton>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        ))}

        {addingField && (
          <Card variant="outlined" sx={{ mb: 1, bgcolor: 'action.hover' }}>
            <CardContent sx={{ py: 1, '&:last-child': { pb: 1 } }}>
              <Grid container spacing={1}>
                <Grid item xs={3}>
                  <TextField
                    fullWidth
                    size="small"
                    label="Name"
                    value={newField.name}
                    onChange={(e) => setNewField({ ...newField, name: e.target.value })}
                  />
                </Grid>
                <Grid item xs={3}>
                  <FormControl fullWidth size="small">
                    <InputLabel>Type</InputLabel>
                    <Select
                      value={newField.type}
                      label="Type"
                      onChange={(e) => setNewField({ ...newField, type: e.target.value as any })}
                    >
                      <MenuItem value="string">String</MenuItem>
                      <MenuItem value="number">Number</MenuItem>
                      <MenuItem value="boolean">Boolean</MenuItem>
                      <MenuItem value="date">Date</MenuItem>
                      <MenuItem value="email">Email</MenuItem>
                      <MenuItem value="phone">Phone</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
                <Grid item xs={4}>
                  <TextField
                    fullWidth
                    size="small"
                    label="Description"
                    value={newField.description}
                    onChange={(e) => setNewField({ ...newField, description: e.target.value })}
                  />
                </Grid>
                <Grid item xs={2}>
                  <Box sx={{ display: 'flex', gap: 0.5 }}>
                    <Button size="small" onClick={addField} variant="contained">Add</Button>
                    <Button size="small" onClick={() => setAddingField(false)}>Cancel</Button>
                  </Box>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        )}
      </Paper>

      {/* Output */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Output Configuration</Typography>
        <Grid container spacing={2}>
          <Grid item xs={8}>
            <TextField
              fullWidth
              size="small"
              label="Output Variable"
              value={config.outputVariable}
              onChange={(e) => onChange({ ...config, outputVariable: e.target.value })}
              disabled={disabled}
            />
          </Grid>
          <Grid item xs={4}>
            <FormControlLabel
              control={
                <Switch
                  checked={config.validateOutput}
                  onChange={(e) => onChange({ ...config, validateOutput: e.target.checked })}
                  disabled={disabled}
                />
              }
              label="Validate"
            />
          </Grid>
        </Grid>
      </Paper>
    </Box>
  );
};

// ============================================================================
// AI Classifier Panel
// ============================================================================

interface AIClassifierPanelProps {
  config: AIClassifierConfig;
  onChange: (config: AIClassifierConfig) => void;
  models: LLMModelOption[];
  variables: string[];
  disabled?: boolean;
}

const AIClassifierPanel: React.FC<AIClassifierPanelProps> = ({
  config,
  onChange,
  models,
  variables,
  disabled,
}) => {
  const [addingCategory, setAddingCategory] = useState(false);
  const [newCategory, setNewCategory] = useState<AIClassifierCategory>({
    id: '',
    name: '',
    description: '',
  });

  const addCategory = () => {
    if (!newCategory.id || !newCategory.name) return;
    onChange({
      ...config,
      categories: [...config.categories, newCategory],
    });
    setNewCategory({ id: '', name: '', description: '' });
    setAddingCategory(false);
  };

  const updateCategory = (index: number, updates: Partial<AIClassifierCategory>) => {
    const categories = [...config.categories];
    categories[index] = { ...categories[index], ...updates };
    onChange({ ...config, categories });
  };

  const removeCategory = (index: number) => {
    const categories = config.categories.filter((_, i) => i !== index);
    onChange({ ...config, categories });
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      {/* Model Settings */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <ClassifierIcon fontSize="small" />
          Model Settings
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <ModelSelector
              value={config.model}
              onChange={(model) => onChange({ ...config, model })}
              models={models}
              disabled={disabled}
            />
          </Grid>
          <Grid item xs={6}>
            <FormControl fullWidth size="small">
              <InputLabel>Classification Type</InputLabel>
              <Select
                value={config.classificationType}
                label="Classification Type"
                onChange={(e) => onChange({ ...config, classificationType: e.target.value as any })}
                disabled={disabled}
              >
                <MenuItem value="single">Single Category</MenuItem>
                <MenuItem value="multi">Multiple Categories</MenuItem>
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={6}>
            <TextField
              fullWidth
              size="small"
              type="number"
              label="Confidence Threshold"
              value={config.confidenceThreshold}
              onChange={(e) => onChange({ ...config, confidenceThreshold: parseFloat(e.target.value) || 0.7 })}
              disabled={disabled}
              inputProps={{ min: 0, max: 1, step: 0.05 }}
            />
          </Grid>
        </Grid>
      </Paper>

      {/* Input */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Input Configuration</Typography>
        <FormControl fullWidth size="small">
          <InputLabel>Input Variable</InputLabel>
          <Select
            value={config.inputVariable}
            label="Input Variable"
            onChange={(e) => onChange({ ...config, inputVariable: e.target.value })}
            disabled={disabled}
          >
            {variables.map((v) => (
              <MenuItem key={v} value={v}>{v}</MenuItem>
            ))}
          </Select>
        </FormControl>
      </Paper>

      {/* Categories */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="subtitle2">Categories</Typography>
          <Button
            size="small"
            startIcon={<AddIcon />}
            onClick={() => setAddingCategory(true)}
            disabled={disabled}
          >
            Add Category
          </Button>
        </Box>

        <Grid container spacing={1}>
          {config.categories.map((category, index) => (
            <Grid item xs={6} key={category.id}>
              <Card variant="outlined">
                <CardContent sx={{ py: 1, '&:last-child': { pb: 1 } }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                    <Box sx={{ flex: 1 }}>
                      <TextField
                        fullWidth
                        size="small"
                        label="Name"
                        value={category.name}
                        onChange={(e) => updateCategory(index, { name: e.target.value })}
                        disabled={disabled}
                        sx={{ mb: 1 }}
                      />
                      <TextField
                        fullWidth
                        size="small"
                        label="Description"
                        value={category.description}
                        onChange={(e) => updateCategory(index, { description: e.target.value })}
                        disabled={disabled}
                        multiline
                        rows={2}
                      />
                    </Box>
                    <IconButton size="small" onClick={() => removeCategory(index)} disabled={disabled}>
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </Box>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>

        {addingCategory && (
          <Card variant="outlined" sx={{ mt: 1, bgcolor: 'action.hover' }}>
            <CardContent sx={{ py: 1, '&:last-child': { pb: 1 } }}>
              <Grid container spacing={1}>
                <Grid item xs={4}>
                  <TextField
                    fullWidth
                    size="small"
                    label="ID"
                    value={newCategory.id}
                    onChange={(e) => setNewCategory({ ...newCategory, id: e.target.value })}
                  />
                </Grid>
                <Grid item xs={4}>
                  <TextField
                    fullWidth
                    size="small"
                    label="Name"
                    value={newCategory.name}
                    onChange={(e) => setNewCategory({ ...newCategory, name: e.target.value })}
                  />
                </Grid>
                <Grid item xs={4}>
                  <Box sx={{ display: 'flex', gap: 0.5 }}>
                    <Button size="small" onClick={addCategory} variant="contained">Add</Button>
                    <Button size="small" onClick={() => setAddingCategory(false)}>Cancel</Button>
                  </Box>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        )}
      </Paper>

      {/* Output Options */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Output Options</Typography>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <TextField
              fullWidth
              size="small"
              label="Output Variable"
              value={config.outputVariable}
              onChange={(e) => onChange({ ...config, outputVariable: e.target.value })}
              disabled={disabled}
            />
          </Grid>
          <Grid item xs={4}>
            <FormControlLabel
              control={
                <Switch
                  checked={config.includeConfidence}
                  onChange={(e) => onChange({ ...config, includeConfidence: e.target.checked })}
                  disabled={disabled}
                />
              }
              label="Confidence"
            />
          </Grid>
          <Grid item xs={4}>
            <FormControlLabel
              control={
                <Switch
                  checked={config.includeReasoning}
                  onChange={(e) => onChange({ ...config, includeReasoning: e.target.checked })}
                  disabled={disabled}
                />
              }
              label="Reasoning"
            />
          </Grid>
          <Grid item xs={4}>
            <FormControlLabel
              control={
                <Switch
                  checked={config.allowCustomCategories}
                  onChange={(e) => onChange({ ...config, allowCustomCategories: e.target.checked })}
                  disabled={disabled}
                />
              }
              label="Custom"
            />
          </Grid>
        </Grid>
      </Paper>
    </Box>
  );
};

// ============================================================================
// AI Sentiment Analyzer Panel
// ============================================================================

interface AISentimentAnalyzerPanelProps {
  config: AISentimentAnalyzerConfig;
  onChange: (config: AISentimentAnalyzerConfig) => void;
  models: LLMModelOption[];
  variables: string[];
  disabled?: boolean;
}

const AISentimentAnalyzerPanel: React.FC<AISentimentAnalyzerPanelProps> = ({
  config,
  onChange,
  models,
  variables,
  disabled,
}) => {
  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      {/* Model Settings */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <SentimentIcon fontSize="small" />
          Model Settings
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <ModelSelector
              value={config.model}
              onChange={(model) => onChange({ ...config, model })}
              models={models}
              disabled={disabled}
            />
          </Grid>
          <Grid item xs={12}>
            <FormControl fullWidth size="small">
              <InputLabel>Analysis Type</InputLabel>
              <Select
                value={config.analysisType}
                label="Analysis Type"
                onChange={(e) => onChange({ ...config, analysisType: e.target.value as any })}
                disabled={disabled}
              >
                <MenuItem value="basic">Basic (Positive/Neutral/Negative)</MenuItem>
                <MenuItem value="detailed">Detailed (with score and confidence)</MenuItem>
                <MenuItem value="emotional">Emotional (with emotion breakdown)</MenuItem>
              </Select>
            </FormControl>
          </Grid>
        </Grid>
      </Paper>

      {/* Input */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Input Configuration</Typography>
        <FormControl fullWidth size="small">
          <InputLabel>Input Variable</InputLabel>
          <Select
            value={config.inputVariable}
            label="Input Variable"
            onChange={(e) => onChange({ ...config, inputVariable: e.target.value })}
            disabled={disabled}
          >
            {variables.map((v) => (
              <MenuItem key={v} value={v}>{v}</MenuItem>
            ))}
          </Select>
        </FormControl>
      </Paper>

      {/* Output Options */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Output Options</Typography>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <TextField
              fullWidth
              size="small"
              label="Output Variable"
              value={config.outputVariable}
              onChange={(e) => onChange({ ...config, outputVariable: e.target.value })}
              disabled={disabled}
            />
          </Grid>
          <Grid item xs={6}>
            <FormControlLabel
              control={
                <Switch
                  checked={config.includeScore}
                  onChange={(e) => onChange({ ...config, includeScore: e.target.checked })}
                  disabled={disabled}
                />
              }
              label="Include Score"
            />
          </Grid>
          <Grid item xs={6}>
            <FormControlLabel
              control={
                <Switch
                  checked={config.includeEmotions}
                  onChange={(e) => onChange({ ...config, includeEmotions: e.target.checked })}
                  disabled={disabled}
                />
              }
              label="Include Emotions"
            />
          </Grid>
          <Grid item xs={6}>
            <FormControlLabel
              control={
                <Switch
                  checked={config.includeKeyPhrases}
                  onChange={(e) => onChange({ ...config, includeKeyPhrases: e.target.checked })}
                  disabled={disabled}
                />
              }
              label="Key Phrases"
            />
          </Grid>
          <Grid item xs={6}>
            <FormControlLabel
              control={
                <Switch
                  checked={config.includeSuggestions}
                  onChange={(e) => onChange({ ...config, includeSuggestions: e.target.checked })}
                  disabled={disabled}
                />
              }
              label="Suggestions"
            />
          </Grid>
        </Grid>
      </Paper>

      {/* Routing Thresholds */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Routing Thresholds</Typography>
        <Typography variant="caption" color="text.secondary" display="block" sx={{ mb: 2 }}>
          Define score thresholds for routing decisions
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={4}>
            <TextField
              fullWidth
              size="small"
              type="number"
              label="Positive ≥"
              value={config.sentimentThresholds?.positive ?? 0.6}
              onChange={(e) => onChange({
                ...config,
                sentimentThresholds: {
                  ...config.sentimentThresholds,
                  positive: parseFloat(e.target.value) || 0.6,
                  neutral: config.sentimentThresholds?.neutral ?? 0,
                  negative: config.sentimentThresholds?.negative ?? -0.6,
                },
              })}
              disabled={disabled}
              inputProps={{ min: -1, max: 1, step: 0.1 }}
            />
          </Grid>
          <Grid item xs={4}>
            <TextField
              fullWidth
              size="small"
              type="number"
              label="Neutral ≈"
              value={config.sentimentThresholds?.neutral ?? 0}
              onChange={(e) => onChange({
                ...config,
                sentimentThresholds: {
                  ...config.sentimentThresholds,
                  positive: config.sentimentThresholds?.positive ?? 0.6,
                  neutral: parseFloat(e.target.value) || 0,
                  negative: config.sentimentThresholds?.negative ?? -0.6,
                },
              })}
              disabled={disabled}
              inputProps={{ min: -1, max: 1, step: 0.1 }}
            />
          </Grid>
          <Grid item xs={4}>
            <TextField
              fullWidth
              size="small"
              type="number"
              label="Negative ≤"
              value={config.sentimentThresholds?.negative ?? -0.6}
              onChange={(e) => onChange({
                ...config,
                sentimentThresholds: {
                  ...config.sentimentThresholds,
                  positive: config.sentimentThresholds?.positive ?? 0.6,
                  neutral: config.sentimentThresholds?.neutral ?? 0,
                  negative: parseFloat(e.target.value) || -0.6,
                },
              })}
              disabled={disabled}
              inputProps={{ min: -1, max: 1, step: 0.1 }}
            />
          </Grid>
        </Grid>
      </Paper>
    </Box>
  );
};

// ============================================================================
// Human Review Panel
// ============================================================================

interface HumanReviewPanelProps {
  config: HumanReviewConfig;
  onChange: (config: HumanReviewConfig) => void;
  roles: string[];
  variables: string[];
  disabled?: boolean;
}

const HumanReviewPanel: React.FC<HumanReviewPanelProps> = ({
  config,
  onChange,
  roles,
  variables,
  disabled,
}) => {
  const [addingOption, setAddingOption] = useState(false);
  const [newOption, setNewOption] = useState<HumanReviewOption>({
    id: '',
    label: '',
    description: '',
    action: 'approve',
    requiresComment: false,
  });

  const addOption = () => {
    if (!newOption.id || !newOption.label) return;
    onChange({
      ...config,
      reviewOptions: [...config.reviewOptions, newOption],
    });
    setNewOption({ id: '', label: '', description: '', action: 'approve', requiresComment: false });
    setAddingOption(false);
  };

  const removeOption = (index: number) => {
    const options = config.reviewOptions.filter((_, i) => i !== index);
    onChange({ ...config, reviewOptions: options });
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      {/* Task Settings */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <ReviewIcon fontSize="small" />
          Task Settings
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <TextField
              fullWidth
              size="small"
              label="Task Title"
              value={config.taskTitle}
              onChange={(e) => onChange({ ...config, taskTitle: e.target.value })}
              disabled={disabled}
              placeholder="Review AI Generated Content"
            />
          </Grid>
          <Grid item xs={12}>
            <TextField
              fullWidth
              size="small"
              multiline
              rows={2}
              label="Task Description"
              value={config.taskDescription}
              onChange={(e) => onChange({ ...config, taskDescription: e.target.value })}
              disabled={disabled}
            />
          </Grid>
        </Grid>
      </Paper>

      {/* Assignment */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Assignment</Typography>
        <Grid container spacing={2}>
          <Grid item xs={6}>
            <FormControl fullWidth size="small">
              <InputLabel>Assign to Role</InputLabel>
              <Select
                value={config.assignToRole || ''}
                label="Assign to Role"
                onChange={(e) => onChange({ ...config, assignToRole: e.target.value })}
                disabled={disabled}
              >
                <MenuItem value="">Not specified</MenuItem>
                {roles.map((role) => (
                  <MenuItem key={role} value={role}>{role}</MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={6}>
            <TextField
              fullWidth
              size="small"
              type="number"
              label="Due In (minutes)"
              value={config.dueInMinutes}
              onChange={(e) => onChange({ ...config, dueInMinutes: parseInt(e.target.value) || 60 })}
              disabled={disabled}
            />
          </Grid>
        </Grid>
      </Paper>

      {/* Review Content */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Review Content</Typography>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <FormControl fullWidth size="small">
              <InputLabel>Variable to Review</InputLabel>
              <Select
                value={config.reviewVariable}
                label="Variable to Review"
                onChange={(e) => onChange({ ...config, reviewVariable: e.target.value })}
                disabled={disabled}
              >
                {variables.map((v) => (
                  <MenuItem key={v} value={v}>{v}</MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={6}>
            <FormControlLabel
              control={
                <Switch
                  checked={config.showOriginalInput}
                  onChange={(e) => onChange({ ...config, showOriginalInput: e.target.checked })}
                  disabled={disabled}
                />
              }
              label="Show original input"
            />
          </Grid>
          <Grid item xs={6}>
            <FormControlLabel
              control={
                <Switch
                  checked={config.allowEdit}
                  onChange={(e) => onChange({ ...config, allowEdit: e.target.checked })}
                  disabled={disabled}
                />
              }
              label="Allow editing"
            />
          </Grid>
        </Grid>
      </Paper>

      {/* Review Options */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="subtitle2">Review Options</Typography>
          <Button
            size="small"
            startIcon={<AddIcon />}
            onClick={() => setAddingOption(true)}
            disabled={disabled}
          >
            Add Option
          </Button>
        </Box>

        {config.reviewOptions.map((option, index) => (
          <Card key={option.id} variant="outlined" sx={{ mb: 1 }}>
            <CardContent sx={{ py: 1, '&:last-child': { pb: 1 } }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <Chip
                    label={option.label}
                    color={
                      option.action === 'approve' ? 'success' :
                      option.action === 'reject' ? 'error' :
                      option.action === 'modify' ? 'warning' : 'default'
                    }
                    size="small"
                  />
                  <Typography variant="caption" color="text.secondary">
                    {option.action}
                    {option.requiresComment && ' (comment required)'}
                  </Typography>
                </Box>
                <IconButton size="small" onClick={() => removeOption(index)} disabled={disabled}>
                  <DeleteIcon fontSize="small" />
                </IconButton>
              </Box>
            </CardContent>
          </Card>
        ))}

        {addingOption && (
          <Card variant="outlined" sx={{ mb: 1, bgcolor: 'action.hover' }}>
            <CardContent sx={{ py: 1, '&:last-child': { pb: 1 } }}>
              <Grid container spacing={1}>
                <Grid item xs={3}>
                  <TextField
                    fullWidth
                    size="small"
                    label="ID"
                    value={newOption.id}
                    onChange={(e) => setNewOption({ ...newOption, id: e.target.value })}
                  />
                </Grid>
                <Grid item xs={3}>
                  <TextField
                    fullWidth
                    size="small"
                    label="Label"
                    value={newOption.label}
                    onChange={(e) => setNewOption({ ...newOption, label: e.target.value })}
                  />
                </Grid>
                <Grid item xs={3}>
                  <FormControl fullWidth size="small">
                    <InputLabel>Action</InputLabel>
                    <Select
                      value={newOption.action}
                      label="Action"
                      onChange={(e) => setNewOption({ ...newOption, action: e.target.value as any })}
                    >
                      <MenuItem value="approve">Approve</MenuItem>
                      <MenuItem value="reject">Reject</MenuItem>
                      <MenuItem value="modify">Modify</MenuItem>
                      <MenuItem value="escalate">Escalate</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
                <Grid item xs={3}>
                  <Box sx={{ display: 'flex', gap: 0.5 }}>
                    <Button size="small" onClick={addOption} variant="contained">Add</Button>
                    <Button size="small" onClick={() => setAddingOption(false)}>Cancel</Button>
                  </Box>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        )}

        {config.reviewOptions.length === 0 && !addingOption && (
          <Typography variant="body2" color="text.secondary" textAlign="center" sx={{ py: 2 }}>
            No review options. Add options like "Approve", "Reject", "Request Changes".
          </Typography>
        )}
      </Paper>

      {/* Output */}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Output Configuration</Typography>
        <Grid container spacing={2}>
          <Grid item xs={8}>
            <TextField
              fullWidth
              size="small"
              label="Output Variable"
              value={config.outputVariable}
              onChange={(e) => onChange({ ...config, outputVariable: e.target.value })}
              disabled={disabled}
            />
          </Grid>
          <Grid item xs={4}>
            <FormControlLabel
              control={
                <Switch
                  checked={config.captureReviewerFeedback}
                  onChange={(e) => onChange({ ...config, captureReviewerFeedback: e.target.checked })}
                  disabled={disabled}
                />
              }
              label="Capture Feedback"
            />
          </Grid>
        </Grid>
      </Paper>
    </Box>
  );
};

// ============================================================================
// Main Component
// ============================================================================

const AI_NODE_TYPES = ['AIDecision', 'AIAgent', 'AIContentGenerator', 'AIDataExtractor', 'AIClassifier', 'AISentimentAnalyzer', 'HumanReview'];

const getDefaultConfig = (nodeType: string): any => {
  switch (nodeType) {
    case 'AIDecision':
      return {
        model: 'gpt-4o',
        temperature: 0.3,
        maxTokens: 500,
        systemPrompt: 'You are an AI assistant that analyzes content and makes routing decisions based on defined criteria.',
        userPromptTemplate: 'Analyze the following content and determine the best routing option:\n\n{{input}}',
        decisionOptions: [],
        confidenceThreshold: 0.7,
        inputVariables: [],
        outputVariable: 'decision_result',
      } as AIDecisionConfig;
    case 'AIAgent':
      return {
        agentName: 'CRM Agent',
        agentDescription: '',
        systemPrompt: 'You are a helpful AI agent that assists with CRM tasks.',
        model: 'gpt-4o',
        temperature: 0.7,
        maxTokens: 2000,
        maxIterations: 10,
        availableTools: [],
        toolApprovalRequired: true,
        autonomyLevel: 'medium',
        canModifyData: false,
        canSendCommunications: false,
        enableMemory: false,
        memoryType: 'conversation',
        maxMemoryItems: 10,
        primaryGoal: '',
        constraints: [],
        stopConditions: [],
        outputVariable: 'agent_result',
      } as AIAgentConfig;
    case 'AIContentGenerator':
      return {
        contentType: 'email',
        model: 'gpt-4o',
        temperature: 0.7,
        maxTokens: 1500,
        systemPrompt: 'You are a professional content writer.',
        userPromptTemplate: '',
        tone: 'professional',
        language: 'English',
        useTemplate: false,
        inputVariables: [],
        contextFields: [],
        outputVariable: 'generated_content',
        outputFormat: 'text',
        requiresReview: true,
      } as AIContentGeneratorConfig;
    case 'AIDataExtractor':
      return {
        model: 'gpt-4o',
        temperature: 0.1,
        maxTokens: 1000,
        inputVariable: '',
        extractionSchema: [],
        outputVariable: 'extracted_data',
        outputFormat: 'object',
        validateOutput: true,
        onExtractionFailure: 'error',
      } as AIDataExtractorConfig;
    case 'AIClassifier':
      return {
        model: 'gpt-4o',
        temperature: 0.2,
        maxTokens: 500,
        classificationType: 'single',
        categories: [],
        allowCustomCategories: false,
        inputVariable: '',
        contextVariables: [],
        outputVariable: 'classification_result',
        includeConfidence: true,
        includeReasoning: false,
        confidenceThreshold: 0.7,
      } as AIClassifierConfig;
    case 'AISentimentAnalyzer':
      return {
        model: 'gpt-4o',
        temperature: 0.2,
        maxTokens: 500,
        analysisType: 'detailed',
        inputVariable: '',
        contextVariables: [],
        outputVariable: 'sentiment_result',
        includeScore: true,
        includeEmotions: false,
        includeKeyPhrases: true,
        includeSuggestions: false,
        sentimentThresholds: { positive: 0.6, neutral: 0, negative: -0.6 },
      } as AISentimentAnalyzerConfig;
    case 'HumanReview':
      return {
        taskTitle: 'Review AI Output',
        taskDescription: '',
        reviewVariable: '',
        contextVariables: [],
        showOriginalInput: true,
        reviewOptions: [
          { id: 'approve', label: 'Approve', description: '', action: 'approve', requiresComment: false },
          { id: 'reject', label: 'Reject', description: '', action: 'reject', requiresComment: true },
        ],
        allowEdit: true,
        requireComments: false,
        dueInMinutes: 60,
        outputVariable: 'review_result',
        captureReviewerFeedback: true,
      } as HumanReviewConfig;
    default:
      return {};
  }
};

export const AIPropertiesPanel: React.FC<AIPropertiesPanelProps> = ({
  nodeId,
  nodeKey,
  nodeName,
  nodeType,
  configuration,
  onChange,
  onDelete,
  variables = [],
  readonly = false,
}) => {
  const [tabValue, setTabValue] = useState(0);
  const [config, setConfig] = useState<any>(() => {
    try {
      const parsed = configuration ? JSON.parse(configuration) : {};
      return { ...getDefaultConfig(nodeType), ...parsed };
    } catch {
      return getDefaultConfig(nodeType);
    }
  });
  const [models, setModels] = useState<LLMModelOption[]>([]);
  const [tools, setTools] = useState<AIToolDefinition[]>([]);
  const [roles, setRoles] = useState<string[]>([]);

  // Load configuration from backend
  useEffect(() => {
    workflowService.getConfig().then((cfg) => {
      setModels(cfg.llmModels);
      setRoles(cfg.roles.map(r => r.value));
    }).catch(() => {
      // Use default models
      setModels([
        { value: 'gpt-4o', label: 'GPT-4o', provider: 'openai', isDefault: true },
        { value: 'gpt-4o-mini', label: 'GPT-4o Mini', provider: 'openai', isDefault: false },
        { value: 'gpt-3.5-turbo', label: 'GPT-3.5 Turbo', provider: 'openai', isDefault: false },
        { value: 'claude-3-sonnet', label: 'Claude 3 Sonnet', provider: 'anthropic', isDefault: false },
      ]);
    });
    
    // Load available tools for agent
    setTools([
      { id: 'search_customers', name: 'Search Customers', description: 'Search customer database', category: 'crm', icon: 'Search', parameters: [], requiresApproval: false },
      { id: 'get_customer', name: 'Get Customer Details', description: 'Get detailed customer information', category: 'crm', icon: 'Person', parameters: [], requiresApproval: false },
      { id: 'update_customer', name: 'Update Customer', description: 'Update customer record', category: 'crm', icon: 'Edit', parameters: [], requiresApproval: true },
      { id: 'create_ticket', name: 'Create Ticket', description: 'Create support ticket', category: 'crm', icon: 'Ticket', parameters: [], requiresApproval: true },
      { id: 'send_email', name: 'Send Email', description: 'Send email to customer', category: 'communication', icon: 'Email', parameters: [], requiresApproval: true },
      { id: 'web_search', name: 'Web Search', description: 'Search the web for information', category: 'external', icon: 'Web', parameters: [], requiresApproval: false },
    ]);
  }, []);

  // Update configuration when it changes
  useEffect(() => {
    try {
      const parsed = configuration ? JSON.parse(configuration) : {};
      setConfig({ ...getDefaultConfig(nodeType), ...parsed });
    } catch {
      setConfig(getDefaultConfig(nodeType));
    }
  }, [configuration, nodeType]);

  const updateConfig = (newConfig: any) => {
    setConfig(newConfig);
    onChange('configuration', JSON.stringify(newConfig));
  };

  // Check if this is an AI node type
  if (!AI_NODE_TYPES.includes(nodeType)) {
    return null;
  }

  const getIcon = () => {
    switch (nodeType) {
      case 'AIDecision': return <DecisionIcon />;
      case 'AIAgent': return <AgentIcon />;
      case 'AIContentGenerator': return <ContentIcon />;
      case 'AIDataExtractor': return <ExtractorIcon />;
      case 'AIClassifier': return <ClassifierIcon />;
      case 'AISentimentAnalyzer': return <SentimentIcon />;
      case 'HumanReview': return <ReviewIcon />;
      default: return <AIIcon />;
    }
  };

  return (
    <Box sx={{ p: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <Box sx={{ 
          bgcolor: 'primary.main', 
          color: 'white', 
          p: 1, 
          borderRadius: 1,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center'
        }}>
          {getIcon()}
        </Box>
        <Box sx={{ flex: 1 }}>
          <Typography variant="h6">{nodeName}</Typography>
          <Typography variant="caption" color="text.secondary" fontFamily="monospace">
            {nodeKey}
          </Typography>
        </Box>
      </Box>

      <Divider />

      {/* Node Type Specific Panel */}
      {nodeType === 'AIDecision' && (
        <AIDecisionPanel
          config={config as AIDecisionConfig}
          onChange={updateConfig}
          models={models}
          variables={variables}
          disabled={readonly}
        />
      )}

      {nodeType === 'AIAgent' && (
        <AIAgentPanel
          config={config as AIAgentConfig}
          onChange={updateConfig}
          models={models}
          tools={tools}
          variables={variables}
          disabled={readonly}
        />
      )}

      {nodeType === 'AIContentGenerator' && (
        <AIContentGeneratorPanel
          config={config as AIContentGeneratorConfig}
          onChange={updateConfig}
          models={models}
          variables={variables}
          disabled={readonly}
        />
      )}

      {nodeType === 'AIDataExtractor' && (
        <AIDataExtractorPanel
          config={config as AIDataExtractorConfig}
          onChange={updateConfig}
          models={models}
          variables={variables}
          disabled={readonly}
        />
      )}

      {nodeType === 'AIClassifier' && (
        <AIClassifierPanel
          config={config as AIClassifierConfig}
          onChange={updateConfig}
          models={models}
          variables={variables}
          disabled={readonly}
        />
      )}

      {nodeType === 'AISentimentAnalyzer' && (
        <AISentimentAnalyzerPanel
          config={config as AISentimentAnalyzerConfig}
          onChange={updateConfig}
          models={models}
          variables={variables}
          disabled={readonly}
        />
      )}

      {nodeType === 'HumanReview' && (
        <HumanReviewPanel
          config={config as HumanReviewConfig}
          onChange={updateConfig}
          roles={roles}
          variables={variables}
          disabled={readonly}
        />
      )}

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

export default AIPropertiesPanel;
