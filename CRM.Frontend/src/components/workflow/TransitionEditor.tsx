/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Transition Editor - configuration panel for workflow transitions
 */

import React from 'react';
import {
  Box,
  Button,
  Divider,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  TextField,
} from '@mui/material';
import { Delete as DeleteIcon } from '@mui/icons-material';
import type { WorkflowTransition, UpdateTransitionDto } from '../../services/workflowService';

interface TransitionEditorProps {
  selectedTransition: WorkflowTransition;
  onUpdate: (updates: UpdateTransitionDto) => Promise<void>;
  onDelete: () => void;
}

const TransitionEditor: React.FC<TransitionEditorProps> = ({
  selectedTransition,
  onUpdate,
  onDelete,
}) => {
  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      <TextField
        fullWidth
        size="small"
        label="Label"
        value={selectedTransition.label || ''}
        onChange={(e) => onUpdate({ label: e.target.value })}
      />
      <FormControl fullWidth size="small">
        <InputLabel>Condition Type</InputLabel>
        <Select
          value={selectedTransition.conditionType}
          label="Condition Type"
          onChange={(e) => onUpdate({ conditionType: e.target.value })}
        >
          <MenuItem value="Always">Always</MenuItem>
          <MenuItem value="Expression">Expression</MenuItem>
          <MenuItem value="FieldMatch">Field Match</MenuItem>
          <MenuItem value="UserChoice">User Choice</MenuItem>
        </Select>
      </FormControl>
      <FormControl fullWidth size="small">
        <InputLabel>Line Style</InputLabel>
        <Select
          value={selectedTransition.lineStyle}
          label="Line Style"
          onChange={(e) => onUpdate({ lineStyle: e.target.value })}
        >
          <MenuItem value="solid">Solid</MenuItem>
          <MenuItem value="dashed">Dashed</MenuItem>
          <MenuItem value="dotted">Dotted</MenuItem>
        </Select>
      </FormControl>

      <Divider />

      <Button
        variant="outlined"
        color="error"
        startIcon={<DeleteIcon />}
        onClick={onDelete}
      >
        Delete Connection
      </Button>
    </Box>
  );
};

export default TransitionEditor;
