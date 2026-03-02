/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Node Editor - configuration panel for workflow nodes
 */

import React from 'react';
import {
  Box,
  Button,
  Divider,
  FormControlLabel,
  Switch,
  TextField,
} from '@mui/material';
import { Delete as DeleteIcon } from '@mui/icons-material';
import { AIPropertiesPanel } from './AIPropertiesPanel';
import { TriggerPropertiesPanel } from './TriggerPropertiesPanel';
import { ActionPropertiesPanel } from './ActionPropertiesPanel';
import {
  nodeTypeInfo,
  WorkflowNode,
  UpdateNodeDto,
} from '../../services/workflowService';

interface NodeEditorProps {
  selectedNode: WorkflowNode;
  entityType: string;
  versionStatus?: string;
  onUpdateProperty: (property: keyof UpdateNodeDto, value: string | number | boolean) => void;
  onDelete: () => void;
}

const aiNodeTypes = [
  'AIDecision',
  'AIAgent',
  'AIContentGenerator',
  'AIDataExtractor',
  'AIClassifier',
  'AISentimentAnalyzer',
  'HumanReview',
];

const NodeEditor: React.FC<NodeEditorProps> = ({
  selectedNode,
  entityType,
  versionStatus,
  onUpdateProperty,
  onDelete,
}) => {
  if (aiNodeTypes.includes(selectedNode.nodeType)) {
    return (
      <AIPropertiesPanel
        nodeId={selectedNode.id}
        nodeKey={selectedNode.nodeKey}
        nodeName={selectedNode.name}
        nodeType={selectedNode.nodeType}
        configuration={selectedNode.configuration || '{}'}
        onChange={(property, value) => onUpdateProperty(property as keyof UpdateNodeDto, value)}
        onDelete={onDelete}
        variables={['customer', 'ticket', 'email', 'input', 'context', 'entity', 'workflow_data']}
        readonly={versionStatus === 'Active'}
      />
    );
  }

  if (selectedNode.nodeType === 'Trigger') {
    return (
      <TriggerPropertiesPanel
        nodeId={selectedNode.id}
        nodeKey={selectedNode.nodeKey}
        nodeName={selectedNode.name}
        configuration={selectedNode.configuration || '{}'}
        entityType={entityType}
        onChange={(property, value) => onUpdateProperty(property as keyof UpdateNodeDto, value)}
        onDelete={onDelete}
        readonly={versionStatus === 'Active'}
      />
    );
  }

  if (selectedNode.nodeType === 'Action') {
    return (
      <ActionPropertiesPanel
        nodeId={selectedNode.id}
        nodeKey={selectedNode.nodeKey}
        nodeName={selectedNode.name}
        configuration={selectedNode.configuration || '{}'}
        entityType={entityType}
        onChange={(property, value) => onUpdateProperty(property as keyof UpdateNodeDto, value)}
        onDelete={onDelete}
        readonly={versionStatus === 'Active'}
      />
    );
  }

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      <TextField
        fullWidth
        size="small"
        label="Name"
        value={selectedNode.name}
        onChange={(e) => onUpdateProperty('name', e.target.value)}
      />
      <TextField
        fullWidth
        size="small"
        label="Description"
        value={selectedNode.description || ''}
        onChange={(e) => onUpdateProperty('description', e.target.value)}
        multiline
        rows={2}
      />
      <TextField
        fullWidth
        size="small"
        label="Node Type"
        value={nodeTypeInfo[selectedNode.nodeType]?.label || selectedNode.nodeType}
        disabled
      />
      <FormControlLabel
        control={
          <Switch
            checked={selectedNode.isStartNode}
            onChange={(e) => onUpdateProperty('isStartNode', e.target.checked)}
            size="small"
          />
        }
        label="Start Node"
      />
      <FormControlLabel
        control={
          <Switch
            checked={selectedNode.isEndNode}
            onChange={(e) => onUpdateProperty('isEndNode', e.target.checked)}
            size="small"
          />
        }
        label="End Node"
      />
      <TextField
        fullWidth
        size="small"
        type="number"
        label="Timeout (minutes)"
        value={selectedNode.timeoutMinutes}
        onChange={(e) => onUpdateProperty('timeoutMinutes', Number.parseInt(e.target.value, 10) || 0)}
      />
      <TextField
        fullWidth
        size="small"
        type="number"
        label="Retry Count"
        value={selectedNode.retryCount}
        onChange={(e) => onUpdateProperty('retryCount', Number.parseInt(e.target.value, 10) || 0)}
      />

      <Divider />

      <Button
        variant="outlined"
        color="error"
        startIcon={<DeleteIcon />}
        onClick={onDelete}
      >
        Delete Node
      </Button>
    </Box>
  );
};

export default NodeEditor;
