/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Workflow Simulator - Test workflows on sample data without side effects
 */

import React, { useState, useCallback } from 'react';
import {
  Box,
  Typography,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Paper,
  Stepper,
  Step,
  StepLabel,
  StepContent,
  Chip,
  Alert,
  CircularProgress,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Divider,
  IconButton,
  Collapse,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  ListItemSecondaryAction,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Tooltip,
} from '@mui/material';
import {
  PlayArrow as PlayIcon,
  Stop as StopIcon,
  SkipNext as NextIcon,
  Pause as PauseIcon,
  Refresh as ResetIcon,
  Close as CloseIcon,
  CheckCircle as SuccessIcon,
  Error as ErrorIcon,
  Warning as WarningIcon,
  Info as InfoIcon,
  ExpandMore as ExpandIcon,
  Code as CodeIcon,
  Email as EmailIcon,
  Update as UpdateIcon,
  Webhook as WebhookIcon,
  Person as PersonIcon,
  Schedule as TimerIcon,
  Psychology as LLMIcon,
} from '@mui/icons-material';
import { WorkflowNode, WorkflowTransition } from '../../services/workflowService';

// ============================================================================
// Types
// ============================================================================

interface SimulationStep {
  nodeId: number;
  nodeKey: string;
  nodeName: string;
  nodeType: string;
  status: 'pending' | 'running' | 'completed' | 'skipped' | 'failed';
  inputs: Record<string, any>;
  outputs: Record<string, any>;
  actions: SimulatedAction[];
  duration?: number;
  error?: string;
  conditionResult?: boolean;
  transitionTaken?: string;
}

interface SimulatedAction {
  type: 'email' | 'update' | 'webhook' | 'notification' | 'llm' | 'create' | 'log';
  description: string;
  details: Record<string, any>;
  wouldExecute: boolean;
}

interface WorkflowSimulatorProps {
  open: boolean;
  onClose: () => void;
  workflowId: number;
  workflowName: string;
  nodes: WorkflowNode[];
  transitions: WorkflowTransition[];
  entityType: string;
  onSimulate?: (params: {
    entityId?: number;
    sampleData: Record<string, any>;
    variables: Record<string, any>;
  }) => Promise<SimulationStep[]>;
}

interface SampleEntity {
  id: number;
  label: string;
  data: Record<string, any>;
}

// ============================================================================
// Sample Data Templates
// ============================================================================

const sampleDataTemplates: Record<string, SampleEntity[]> = {
  Customer: [
    { id: 1, label: 'New Enterprise Customer', data: { name: 'Acme Corp', email: 'contact@acme.com', type: 'Enterprise', status: 'New', industry: 'Technology', revenue: 5000000 } },
    { id: 2, label: 'SMB Customer at Risk', data: { name: 'Small Biz LLC', email: 'info@smallbiz.com', type: 'SMB', status: 'AtRisk', industry: 'Retail', revenue: 250000, healthScore: 35 } },
    { id: 3, label: 'Churned Customer', data: { name: 'Former Client Inc', email: 'old@former.com', type: 'Enterprise', status: 'Churned', lastPurchase: '2025-06-15' } },
  ],
  Lead: [
    { id: 1, label: 'Hot Lead - Enterprise', data: { name: 'Big Corp', email: 'lead@bigcorp.com', company: 'Big Corporation', score: 85, source: 'Website', status: 'New' } },
    { id: 2, label: 'Cold Lead - Unqualified', data: { name: 'Random Visitor', email: 'random@email.com', company: '', score: 15, source: 'Cold Outreach', status: 'New' } },
    { id: 3, label: 'Marketing Qualified Lead', data: { name: 'Marketing Lead', email: 'mql@company.com', company: 'Tech Startup', score: 65, source: 'Webinar', status: 'MQL' } },
  ],
  Opportunity: [
    { id: 1, label: 'New Opportunity', data: { name: 'Enterprise Deal', value: 100000, stage: 'Qualification', probability: 20, closeDate: '2026-03-15' } },
    { id: 2, label: 'Stalled Opportunity', data: { name: 'Slow Deal', value: 50000, stage: 'Proposal', probability: 50, closeDate: '2025-12-31', lastActivity: '2025-11-01' } },
    { id: 3, label: 'Closing Opportunity', data: { name: 'Almost Won', value: 75000, stage: 'Negotiation', probability: 80, closeDate: '2026-02-01' } },
  ],
  Campaign: [
    { id: 1, label: 'Active Email Campaign', data: { name: 'Q1 Promo', type: 'Email', status: 'Active', budget: 10000, targetAudience: 'Enterprise' } },
    { id: 2, label: 'Planned Webinar', data: { name: 'Product Launch', type: 'Webinar', status: 'Planned', budget: 5000, date: '2026-02-20' } },
  ],
};

// ============================================================================
// Action Icons
// ============================================================================

const actionIcons: Record<string, React.ReactNode> = {
  email: <EmailIcon fontSize="small" />,
  update: <UpdateIcon fontSize="small" />,
  webhook: <WebhookIcon fontSize="small" />,
  notification: <InfoIcon fontSize="small" />,
  llm: <LLMIcon fontSize="small" />,
  create: <SuccessIcon fontSize="small" />,
  log: <CodeIcon fontSize="small" />,
};

// ============================================================================
// Simulation Engine (Client-side)
// ============================================================================

const simulateWorkflow = async (
  nodes: WorkflowNode[],
  transitions: WorkflowTransition[],
  sampleData: Record<string, any>,
  variables: Record<string, any>
): Promise<SimulationStep[]> => {
  const steps: SimulationStep[] = [];
  const startNode = nodes.find(n => n.isStartNode) || nodes.find(n => n.nodeType === 'Trigger');
  
  if (!startNode) {
    throw new Error('No start node found in workflow');
  }

  let currentNode: WorkflowNode | undefined = startNode;
  const context = { ...sampleData, ...variables };
  const maxSteps = 50; // Prevent infinite loops

  while (currentNode && steps.length < maxSteps) {
    // Capture current node for use in closures within this iteration
    const nodeForThisIteration = currentNode;
    
    await new Promise(resolve => setTimeout(resolve, 500)); // Simulate processing time

    const step = await simulateNode(nodeForThisIteration, context, nodes, transitions);
    steps.push(step);

    if (step.status === 'failed' || nodeForThisIteration.isEndNode || nodeForThisIteration.nodeType === 'End') {
      break;
    }

    // Find next node based on transitions
    const outgoingTransitions = transitions
      .filter(t => t.sourceNodeId === nodeForThisIteration.id)
      .sort((a, b) => (a.priority || 100) - (b.priority || 100));

    let nextTransition: WorkflowTransition | undefined;
    for (const trans of outgoingTransitions) {
      if (evaluateCondition(trans, context, step.outputs)) {
        nextTransition = trans;
        step.transitionTaken = trans.label || trans.transitionKey;
        break;
      }
    }

    if (!nextTransition) {
      nextTransition = outgoingTransitions.find(t => t.isDefault);
      if (nextTransition) {
        step.transitionTaken = nextTransition.label || 'default';
      }
    }

    if (nextTransition) {
      currentNode = nodes.find(n => n.id === nextTransition!.targetNodeId);
      Object.assign(context, step.outputs);
    } else {
      break;
    }
  }

  return steps;
};

const simulateNode = async (
  node: WorkflowNode,
  context: Record<string, any>,
  allNodes: WorkflowNode[],
  allTransitions: WorkflowTransition[]
): Promise<SimulationStep> => {
  const step: SimulationStep = {
    nodeId: node.id,
    nodeKey: node.nodeKey,
    nodeName: node.name,
    nodeType: node.nodeType,
    status: 'completed',
    inputs: { ...context },
    outputs: {},
    actions: [],
    duration: Math.floor(Math.random() * 500) + 100,
  };

  try {
    const config = node.configuration ? JSON.parse(node.configuration) : {};

    switch (node.nodeType) {
      case 'Trigger':
        step.outputs = { triggered: true, timestamp: new Date().toISOString() };
        break;

      case 'Condition':
        step.conditionResult = Math.random() > 0.3; // Simulate condition evaluation
        step.outputs = { conditionResult: step.conditionResult };
        break;

      case 'Action':
        step.actions = generateSimulatedActions(config, context);
        step.outputs = { actionCompleted: true, ...config.outputMapping };
        break;

      case 'HumanTask':
        step.actions.push({
          type: 'notification',
          description: `Human task "${config.title || node.name}" would be created`,
          details: { assignTo: config.assignToRole || config.assignToUser, formSchema: config.formSchema },
          wouldExecute: true,
        });
        step.outputs = { taskAssigned: true, assignedTo: 'Simulated User' };
        break;

      case 'Wait':
        const waitMinutes = config.waitMinutes || 60;
        step.actions.push({
          type: 'log',
          description: `Would wait for ${waitMinutes} minutes`,
          details: { waitMinutes },
          wouldExecute: true,
        });
        step.outputs = { waitCompleted: true };
        break;

      case 'LLMAction':
        step.actions.push({
          type: 'llm',
          description: `LLM would be invoked with model "${config.model || 'gpt-4'}"`,
          details: { 
            model: config.model || 'gpt-4',
            prompt: interpolateTemplate(config.prompt || '', context),
          },
          wouldExecute: true,
        });
        step.outputs = { 
          llmResponse: 'Simulated LLM response based on prompt',
          tokensUsed: Math.floor(Math.random() * 500) + 100,
        };
        break;

      case 'End':
        step.outputs = { workflowCompleted: true };
        break;

      default:
        step.outputs = { processed: true };
    }
  } catch (error: any) {
    step.status = 'failed';
    step.error = error.message;
  }

  return step;
};

const generateSimulatedActions = (config: any, context: Record<string, any>): SimulatedAction[] => {
  const actions: SimulatedAction[] = [];
  const actionType = config.actionType || 'log';

  switch (actionType) {
    case 'sendEmail':
      actions.push({
        type: 'email',
        description: `Email would be sent to "${interpolateTemplate(config.to || '', context)}"`,
        details: {
          to: interpolateTemplate(config.to || '', context),
          template: config.template,
          subject: config.subject,
        },
        wouldExecute: true,
      });
      break;

    case 'updateEntity':
      actions.push({
        type: 'update',
        description: `Entity would be updated with new field values`,
        details: { updates: config.updates || config.fields },
        wouldExecute: true,
      });
      break;

    case 'webhook':
      actions.push({
        type: 'webhook',
        description: `Webhook would be called: ${config.url || 'URL not configured'}`,
        details: { url: config.url, method: config.method || 'POST' },
        wouldExecute: true,
      });
      break;

    case 'sendNotification':
      actions.push({
        type: 'notification',
        description: `Notification would be sent`,
        details: { to: config.to, message: config.message },
        wouldExecute: true,
      });
      break;

    default:
      actions.push({
        type: 'log',
        description: `Action "${actionType}" would be executed`,
        details: config,
        wouldExecute: true,
      });
  }

  return actions;
};

const evaluateCondition = (
  transition: WorkflowTransition,
  context: Record<string, any>,
  outputs: Record<string, any>
): boolean => {
  if (transition.conditionType === 'Always') return true;
  if (transition.isDefault) return false; // Default is checked last
  
  // Simple simulation - randomly evaluate conditions
  if (transition.conditionType === 'Expression') {
    return Math.random() > 0.4;
  }
  
  return Math.random() > 0.5;
};

const interpolateTemplate = (template: string, context: Record<string, any>): string => {
  return template.replace(/\{\{(\w+(?:\.\w+)*)\}\}/g, (match, path) => {
    const parts = path.split('.');
    let value: any = context;
    for (const part of parts) {
      value = value?.[part];
    }
    return value !== undefined ? String(value) : match;
  });
};

// ============================================================================
// Main Component
// ============================================================================

export const WorkflowSimulator: React.FC<WorkflowSimulatorProps> = ({
  open,
  onClose,
  workflowId,
  workflowName,
  nodes,
  transitions,
  entityType,
  onSimulate,
}) => {
  const [selectedSample, setSelectedSample] = useState<number>(0);
  const [customData, setCustomData] = useState<string>('{}');
  const [customVariables, setCustomVariables] = useState<string>('{}');
  const [useCustomData, setUseCustomData] = useState(false);
  
  const [simulating, setSimulating] = useState(false);
  const [steps, setSteps] = useState<SimulationStep[]>([]);
  const [currentStepIndex, setCurrentStepIndex] = useState(-1);
  const [error, setError] = useState<string>('');
  const [paused, setPaused] = useState(false);

  const samples = sampleDataTemplates[entityType] || [];

  const handleReset = useCallback(() => {
    setSteps([]);
    setCurrentStepIndex(-1);
    setError('');
    setPaused(false);
    setSimulating(false);
  }, []);

  const handleStart = useCallback(async () => {
    handleReset();
    setSimulating(true);
    setError('');

    try {
      let sampleData: Record<string, any>;
      let variables: Record<string, any>;

      if (useCustomData) {
        try {
          sampleData = JSON.parse(customData);
          variables = JSON.parse(customVariables);
        } catch (e) {
          throw new Error('Invalid JSON in custom data or variables');
        }
      } else {
        sampleData = samples[selectedSample]?.data || {};
        variables = {};
      }

      if (onSimulate) {
        const result = await onSimulate({ sampleData, variables });
        setSteps(result);
        setCurrentStepIndex(result.length - 1);
      } else {
        // Client-side simulation
        const result = await simulateWorkflow(nodes, transitions, sampleData, variables);
        setSteps(result);
        setCurrentStepIndex(result.length - 1);
      }
    } catch (e: any) {
      setError(e.message || 'Simulation failed');
    } finally {
      setSimulating(false);
    }
  }, [nodes, transitions, samples, selectedSample, useCustomData, customData, customVariables, onSimulate, handleReset]);

  return (
    <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth>
      <DialogTitle>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <PlayIcon color="primary" />
          <Box>
            <Typography variant="h6">Workflow Simulator</Typography>
            <Typography variant="caption" color="text.secondary">
              {workflowName} • Test Mode (No Side Effects)
            </Typography>
          </Box>
          <Box sx={{ flex: 1 }} />
          <IconButton onClick={onClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent dividers>
        <Box sx={{ display: 'flex', gap: 3 }}>
          {/* Left Panel - Configuration */}
          <Box sx={{ width: 320, flexShrink: 0 }}>
            <Typography variant="subtitle2" gutterBottom>Sample Data</Typography>
            
            {/* Sample Selection */}
            <FormControl fullWidth size="small" sx={{ mb: 2 }}>
              <InputLabel>Select Sample {entityType}</InputLabel>
              <Select
                value={selectedSample}
                label={`Select Sample ${entityType}`}
                onChange={(e) => setSelectedSample(e.target.value as number)}
                disabled={useCustomData || simulating}
              >
                {samples.map((sample, index) => (
                  <MenuItem key={sample.id} value={index}>
                    {sample.label}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            {/* Toggle Custom Data */}
            <Button
              size="small"
              startIcon={<CodeIcon />}
              onClick={() => setUseCustomData(!useCustomData)}
              variant={useCustomData ? 'contained' : 'outlined'}
              sx={{ mb: 2 }}
            >
              Use Custom Data
            </Button>

            {/* Custom Data Input */}
            <Collapse in={useCustomData}>
              <Box sx={{ mb: 2 }}>
                <Typography variant="caption" color="text.secondary">Entity Data (JSON)</Typography>
                <TextField
                  fullWidth
                  multiline
                  rows={6}
                  value={customData}
                  onChange={(e) => setCustomData(e.target.value)}
                  disabled={simulating}
                  sx={{ '& .MuiInputBase-input': { fontFamily: 'monospace', fontSize: 12 } }}
                />
              </Box>
              <Box sx={{ mb: 2 }}>
                <Typography variant="caption" color="text.secondary">Variables (JSON)</Typography>
                <TextField
                  fullWidth
                  multiline
                  rows={4}
                  value={customVariables}
                  onChange={(e) => setCustomVariables(e.target.value)}
                  disabled={simulating}
                  sx={{ '& .MuiInputBase-input': { fontFamily: 'monospace', fontSize: 12 } }}
                />
              </Box>
            </Collapse>

            {/* Preview Sample Data */}
            {!useCustomData && samples[selectedSample] && (
              <Paper variant="outlined" sx={{ p: 1.5, mb: 2 }}>
                <Typography variant="caption" color="text.secondary" gutterBottom display="block">
                  Sample Data Preview
                </Typography>
                <pre style={{ fontSize: 11, margin: 0, overflow: 'auto', maxHeight: 200 }}>
                  {JSON.stringify(samples[selectedSample].data, null, 2)}
                </pre>
              </Paper>
            )}

            <Divider sx={{ my: 2 }} />

            {/* Controls */}
            <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
              <Button
                variant="contained"
                startIcon={simulating ? <CircularProgress size={16} color="inherit" /> : <PlayIcon />}
                onClick={handleStart}
                disabled={simulating}
              >
                {simulating ? 'Simulating...' : 'Run Simulation'}
              </Button>
              <Button
                variant="outlined"
                startIcon={<ResetIcon />}
                onClick={handleReset}
                disabled={simulating}
              >
                Reset
              </Button>
            </Box>

            {error && (
              <Alert severity="error" sx={{ mt: 2 }}>
                {error}
              </Alert>
            )}
          </Box>

          {/* Right Panel - Results */}
          <Box sx={{ flex: 1 }}>
            <Typography variant="subtitle2" gutterBottom>
              Execution Path
              {steps.length > 0 && (
                <Chip label={`${steps.length} steps`} size="small" sx={{ ml: 1 }} />
              )}
            </Typography>

            {steps.length === 0 ? (
              <Paper variant="outlined" sx={{ p: 4, textAlign: 'center' }}>
                <PlayIcon sx={{ fontSize: 48, color: 'grey.400', mb: 1 }} />
                <Typography color="text.secondary">
                  Click "Run Simulation" to see the execution path
                </Typography>
              </Paper>
            ) : (
              <Stepper orientation="vertical" activeStep={currentStepIndex}>
                {steps.map((step, index) => (
                  <Step key={step.nodeId} completed={step.status === 'completed'}>
                    <StepLabel
                      error={step.status === 'failed'}
                      optional={
                        <Box sx={{ display: 'flex', gap: 0.5, alignItems: 'center' }}>
                          <Chip
                            label={step.nodeType}
                            size="small"
                            sx={{ height: 18, fontSize: 10 }}
                          />
                          {step.duration && (
                            <Typography variant="caption" color="text.secondary">
                              {step.duration}ms
                            </Typography>
                          )}
                          {step.transitionTaken && (
                            <Chip
                              label={`→ ${step.transitionTaken}`}
                              size="small"
                              color="info"
                              sx={{ height: 18, fontSize: 10 }}
                            />
                          )}
                        </Box>
                      }
                    >
                      {step.nodeName}
                    </StepLabel>
                    <StepContent>
                      {/* Actions */}
                      {step.actions.length > 0 && (
                        <Box sx={{ mb: 1 }}>
                          <Typography variant="caption" color="text.secondary">
                            Actions (would execute):
                          </Typography>
                          <List dense disablePadding>
                            {step.actions.map((action, i) => (
                              <ListItem key={i} disablePadding sx={{ pl: 0 }}>
                                <ListItemIcon sx={{ minWidth: 28 }}>
                                  {actionIcons[action.type] || <CodeIcon fontSize="small" />}
                                </ListItemIcon>
                                <ListItemText
                                  primary={action.description}
                                  primaryTypographyProps={{ variant: 'body2' }}
                                />
                              </ListItem>
                            ))}
                          </List>
                        </Box>
                      )}

                      {/* Condition Result */}
                      {step.conditionResult !== undefined && (
                        <Alert 
                          severity={step.conditionResult ? 'success' : 'warning'}
                          sx={{ py: 0, mb: 1 }}
                        >
                          Condition evaluated to: {step.conditionResult ? 'TRUE' : 'FALSE'}
                        </Alert>
                      )}

                      {/* Error */}
                      {step.error && (
                        <Alert severity="error" sx={{ py: 0, mb: 1 }}>
                          {step.error}
                        </Alert>
                      )}

                      {/* Outputs */}
                      {Object.keys(step.outputs).length > 0 && (
                        <Accordion sx={{ mt: 1 }}>
                          <AccordionSummary expandIcon={<ExpandIcon />}>
                            <Typography variant="caption">Outputs</Typography>
                          </AccordionSummary>
                          <AccordionDetails sx={{ pt: 0 }}>
                            <pre style={{ fontSize: 11, margin: 0 }}>
                              {JSON.stringify(step.outputs, null, 2)}
                            </pre>
                          </AccordionDetails>
                        </Accordion>
                      )}
                    </StepContent>
                  </Step>
                ))}
              </Stepper>
            )}
          </Box>
        </Box>
      </DialogContent>

      <DialogActions>
        <Alert severity="info" sx={{ flex: 1, py: 0 }}>
          This is a simulation. No emails will be sent, no data will be modified.
        </Alert>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
};

export default WorkflowSimulator;
