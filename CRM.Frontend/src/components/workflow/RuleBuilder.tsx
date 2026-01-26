/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Visual Rule Builder Component - AND/OR condition builder for workflow conditions
 * Supports nested groups, field comparisons, and variable references
 */

import React, { useState, useCallback } from 'react';
import {
  Box,
  Paper,
  Typography,
  Button,
  IconButton,
  Select,
  MenuItem,
  TextField,
  FormControl,
  InputLabel,
  Chip,
  Tooltip,
  Autocomplete,
  Collapse,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  DragIndicator as DragIcon,
  ContentCopy as CopyIcon,
  Code as CodeIcon,
  PlaylistAdd as AddGroupIcon,
} from '@mui/icons-material';

// ============================================================================
// Types
// ============================================================================

export type ConditionOperator =
  | 'equals'
  | 'notEquals'
  | 'contains'
  | 'notContains'
  | 'startsWith'
  | 'endsWith'
  | 'greaterThan'
  | 'lessThan'
  | 'greaterThanOrEqual'
  | 'lessThanOrEqual'
  | 'isEmpty'
  | 'isNotEmpty'
  | 'in'
  | 'notIn'
  | 'between'
  | 'regex';

export type LogicalOperator = 'AND' | 'OR';

export interface ConditionRule {
  id: string;
  type: 'rule';
  field: string;
  operator: ConditionOperator;
  value: string | string[] | number | boolean | null;
  valueType: 'static' | 'variable' | 'expression';
}

export interface ConditionGroup {
  id: string;
  type: 'group';
  operator: LogicalOperator;
  conditions: (ConditionRule | ConditionGroup)[];
}

export interface FieldDefinition {
  name: string;
  label: string;
  type: 'string' | 'number' | 'boolean' | 'date' | 'datetime' | 'enum' | 'array';
  enumValues?: { value: string; label: string }[];
  category?: string;
}

export interface VariableDefinition {
  name: string;
  label: string;
  type: string;
  source: 'entity' | 'workflow' | 'system' | 'previous_step';
}

export interface RuleBuilderProps {
  value: ConditionGroup;
  onChange: (value: ConditionGroup) => void;
  fields: FieldDefinition[];
  variables?: VariableDefinition[];
  entityType?: string;
  readonly?: boolean;
  compact?: boolean;
}

// ============================================================================
// Operator Definitions
// ============================================================================

const operatorLabels: Record<ConditionOperator, string> = {
  equals: 'equals',
  notEquals: 'does not equal',
  contains: 'contains',
  notContains: 'does not contain',
  startsWith: 'starts with',
  endsWith: 'ends with',
  greaterThan: 'greater than',
  lessThan: 'less than',
  greaterThanOrEqual: 'greater than or equal to',
  lessThanOrEqual: 'less than or equal to',
  isEmpty: 'is empty',
  isNotEmpty: 'is not empty',
  in: 'is one of',
  notIn: 'is not one of',
  between: 'is between',
  regex: 'matches pattern',
};

const operatorsByType: Record<string, ConditionOperator[]> = {
  string: ['equals', 'notEquals', 'contains', 'notContains', 'startsWith', 'endsWith', 'isEmpty', 'isNotEmpty', 'in', 'notIn', 'regex'],
  number: ['equals', 'notEquals', 'greaterThan', 'lessThan', 'greaterThanOrEqual', 'lessThanOrEqual', 'between', 'isEmpty', 'isNotEmpty'],
  boolean: ['equals', 'notEquals'],
  date: ['equals', 'notEquals', 'greaterThan', 'lessThan', 'greaterThanOrEqual', 'lessThanOrEqual', 'between', 'isEmpty', 'isNotEmpty'],
  datetime: ['equals', 'notEquals', 'greaterThan', 'lessThan', 'greaterThanOrEqual', 'lessThanOrEqual', 'between', 'isEmpty', 'isNotEmpty'],
  enum: ['equals', 'notEquals', 'in', 'notIn', 'isEmpty', 'isNotEmpty'],
  array: ['contains', 'notContains', 'isEmpty', 'isNotEmpty'],
};

const noValueOperators: ConditionOperator[] = ['isEmpty', 'isNotEmpty'];
const multiValueOperators: ConditionOperator[] = ['in', 'notIn', 'between'];

// ============================================================================
// Helper Functions
// ============================================================================

const generateId = () => `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;

const createEmptyRule = (): ConditionRule => ({
  id: generateId(),
  type: 'rule',
  field: '',
  operator: 'equals',
  value: '',
  valueType: 'static',
});

const createEmptyGroup = (operator: LogicalOperator = 'AND'): ConditionGroup => ({
  id: generateId(),
  type: 'group',
  operator,
  conditions: [createEmptyRule()],
});

// ============================================================================
// Sub-Components
// ============================================================================

interface RuleEditorProps {
  rule: ConditionRule;
  onChange: (rule: ConditionRule) => void;
  onDelete: () => void;
  fields: FieldDefinition[];
  variables: VariableDefinition[];
  readonly?: boolean;
  compact?: boolean;
}

const RuleEditor: React.FC<RuleEditorProps> = ({
  rule,
  onChange,
  onDelete,
  fields,
  variables,
  readonly,
  compact,
}) => {
  const selectedField = fields.find(f => f.name === rule.field);
  const fieldType = selectedField?.type || 'string';
  const availableOperators = operatorsByType[fieldType] || operatorsByType.string;
  const showValue = !noValueOperators.includes(rule.operator);
  const isMultiValue = multiValueOperators.includes(rule.operator);

  const handleFieldChange = (fieldName: string) => {
    const field = fields.find(f => f.name === fieldName);
    const newOperator = field && operatorsByType[field.type]?.includes(rule.operator)
      ? rule.operator
      : 'equals';
    onChange({ ...rule, field: fieldName, operator: newOperator, value: '' });
  };

  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'center',
        gap: 1,
        p: compact ? 0.5 : 1,
        backgroundColor: 'grey.50',
        borderRadius: 1,
        flexWrap: 'wrap',
      }}
    >
      {!readonly && (
        <IconButton size="small" sx={{ cursor: 'grab', color: 'grey.400' }}>
          <DragIcon fontSize="small" />
        </IconButton>
      )}

      {/* Field Selector */}
      <FormControl size="small" sx={{ minWidth: 180 }}>
        <InputLabel>Field</InputLabel>
        <Select
          value={rule.field}
          label="Field"
          onChange={(e) => handleFieldChange(e.target.value)}
          disabled={readonly}
        >
          {fields.map((field) => (
            <MenuItem key={field.name} value={field.name}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Typography variant="body2">{field.label}</Typography>
                <Chip label={field.type} size="small" sx={{ height: 18, fontSize: 10 }} />
              </Box>
            </MenuItem>
          ))}
        </Select>
      </FormControl>

      {/* Operator Selector */}
      <FormControl size="small" sx={{ minWidth: 160 }}>
        <InputLabel>Operator</InputLabel>
        <Select
          value={rule.operator}
          label="Operator"
          onChange={(e) => onChange({ ...rule, operator: e.target.value as ConditionOperator })}
          disabled={readonly}
        >
          {availableOperators.map((op) => (
            <MenuItem key={op} value={op}>
              {operatorLabels[op]}
            </MenuItem>
          ))}
        </Select>
      </FormControl>

      {/* Value Input */}
      {showValue && (
        <>
          {/* Value Type Selector */}
          <FormControl size="small" sx={{ minWidth: 100 }}>
            <Select
              value={rule.valueType}
              onChange={(e) => onChange({ ...rule, valueType: e.target.value as 'static' | 'variable' | 'expression' })}
              disabled={readonly}
              sx={{ '& .MuiSelect-select': { py: 0.75 } }}
            >
              <MenuItem value="static">Value</MenuItem>
              <MenuItem value="variable">Variable</MenuItem>
              <MenuItem value="expression">Expression</MenuItem>
            </Select>
          </FormControl>

          {/* Value Input Based on Type */}
          {rule.valueType === 'variable' ? (
            <Autocomplete
              size="small"
              sx={{ minWidth: 200, flex: 1 }}
              options={variables}
              getOptionLabel={(option) => option.label || option.name}
              groupBy={(option) => option.source}
              value={variables.find(v => v.name === rule.value) || null}
              onChange={(_, newValue) => onChange({ ...rule, value: newValue?.name || '' })}
              disabled={readonly}
              renderInput={(params) => (
                <TextField {...params} label="Variable" placeholder="Select variable..." />
              )}
              renderOption={(props, option) => (
                <li {...props}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <CodeIcon fontSize="small" color="action" />
                    <Typography variant="body2">{option.label}</Typography>
                    <Chip label={option.type} size="small" sx={{ height: 16, fontSize: 10 }} />
                  </Box>
                </li>
              )}
            />
          ) : rule.valueType === 'expression' ? (
            <TextField
              size="small"
              sx={{ minWidth: 200, flex: 1 }}
              label="Expression"
              placeholder="e.g., {{entity.field}} + 10"
              value={rule.value || ''}
              onChange={(e) => onChange({ ...rule, value: e.target.value })}
              disabled={readonly}
              InputProps={{
                startAdornment: <CodeIcon fontSize="small" color="action" sx={{ mr: 0.5 }} />,
              }}
            />
          ) : selectedField?.type === 'enum' && selectedField.enumValues ? (
            isMultiValue ? (
              <Autocomplete
                multiple
                size="small"
                sx={{ minWidth: 200, flex: 1 }}
                options={selectedField.enumValues}
                getOptionLabel={(option) => option.label}
                value={selectedField.enumValues.filter(e => 
                  Array.isArray(rule.value) && rule.value.includes(e.value)
                )}
                onChange={(_, newValue) => onChange({ ...rule, value: newValue.map(v => v.value) })}
                disabled={readonly}
                renderInput={(params) => (
                  <TextField {...params} label="Values" placeholder="Select values..." />
                )}
              />
            ) : (
              <FormControl size="small" sx={{ minWidth: 160 }}>
                <InputLabel>Value</InputLabel>
                <Select
                  value={rule.value || ''}
                  label="Value"
                  onChange={(e) => onChange({ ...rule, value: e.target.value })}
                  disabled={readonly}
                >
                  {selectedField.enumValues.map((ev) => (
                    <MenuItem key={ev.value} value={ev.value}>
                      {ev.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            )
          ) : selectedField?.type === 'boolean' ? (
            <FormControl size="small" sx={{ minWidth: 100 }}>
              <InputLabel>Value</InputLabel>
              <Select
                value={rule.value?.toString() || 'true'}
                label="Value"
                onChange={(e) => onChange({ ...rule, value: e.target.value === 'true' })}
                disabled={readonly}
              >
                <MenuItem value="true">True</MenuItem>
                <MenuItem value="false">False</MenuItem>
              </Select>
            </FormControl>
          ) : isMultiValue ? (
            <TextField
              size="small"
              sx={{ minWidth: 200, flex: 1 }}
              label={rule.operator === 'between' ? 'Range (min, max)' : 'Values (comma-separated)'}
              placeholder={rule.operator === 'between' ? '10, 100' : 'value1, value2, value3'}
              value={Array.isArray(rule.value) ? rule.value.join(', ') : rule.value || ''}
              onChange={(e) => onChange({ ...rule, value: e.target.value.split(',').map(v => v.trim()) })}
              disabled={readonly}
            />
          ) : (
            <TextField
              size="small"
              sx={{ minWidth: 160, flex: 1 }}
              label="Value"
              type={selectedField?.type === 'number' ? 'number' : selectedField?.type === 'date' ? 'date' : 'text'}
              value={rule.value || ''}
              onChange={(e) => onChange({ ...rule, value: e.target.value })}
              disabled={readonly}
            />
          )}
        </>
      )}

      {/* Actions */}
      {!readonly && (
        <Box sx={{ display: 'flex', gap: 0.5 }}>
          <Tooltip title="Duplicate rule">
            <IconButton size="small" onClick={() => {/* Handle copy */}}>
              <CopyIcon fontSize="small" />
            </IconButton>
          </Tooltip>
          <Tooltip title="Delete rule">
            <IconButton size="small" onClick={onDelete} color="error">
              <DeleteIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        </Box>
      )}
    </Box>
  );
};

interface GroupEditorProps {
  group: ConditionGroup;
  onChange: (group: ConditionGroup) => void;
  onDelete?: () => void;
  fields: FieldDefinition[];
  variables: VariableDefinition[];
  depth?: number;
  readonly?: boolean;
  compact?: boolean;
  isRoot?: boolean;
}

const GroupEditor: React.FC<GroupEditorProps> = ({
  group,
  onChange,
  onDelete,
  fields,
  variables,
  depth = 0,
  readonly,
  compact,
  isRoot,
}) => {
  const borderColors = ['#1976d2', '#9c27b0', '#ff9800', '#4caf50', '#f44336'];
  const borderColor = borderColors[depth % borderColors.length];

  const handleOperatorChange = (operator: LogicalOperator) => {
    onChange({ ...group, operator });
  };

  const handleConditionChange = (index: number, condition: ConditionRule | ConditionGroup) => {
    const newConditions = [...group.conditions];
    newConditions[index] = condition;
    onChange({ ...group, conditions: newConditions });
  };

  const handleConditionDelete = (index: number) => {
    const newConditions = group.conditions.filter((_, i) => i !== index);
    if (newConditions.length === 0) {
      newConditions.push(createEmptyRule());
    }
    onChange({ ...group, conditions: newConditions });
  };

  const handleAddRule = () => {
    onChange({ ...group, conditions: [...group.conditions, createEmptyRule()] });
  };

  const handleAddGroup = () => {
    const newGroup = createEmptyGroup(group.operator === 'AND' ? 'OR' : 'AND');
    onChange({ ...group, conditions: [...group.conditions, newGroup] });
  };

  return (
    <Paper
      variant="outlined"
      sx={{
        p: compact ? 1 : 1.5,
        borderLeft: `3px solid ${borderColor}`,
        borderRadius: 1,
        backgroundColor: depth % 2 === 0 ? 'background.paper' : 'grey.50',
      }}
    >
      {/* Group Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
        {!readonly && !isRoot && (
          <IconButton size="small" sx={{ cursor: 'grab', color: 'grey.400' }}>
            <DragIcon fontSize="small" />
          </IconButton>
        )}

        <Typography variant="body2" color="text.secondary">
          Match
        </Typography>

        <Box sx={{ display: 'flex', borderRadius: 1, overflow: 'hidden', border: '1px solid', borderColor: 'divider' }}>
          <Button
            size="small"
            variant={group.operator === 'AND' ? 'contained' : 'text'}
            onClick={() => handleOperatorChange('AND')}
            disabled={readonly}
            sx={{ 
              borderRadius: 0, 
              minWidth: 50,
              px: 1.5,
              py: 0.25,
            }}
          >
            AND
          </Button>
          <Button
            size="small"
            variant={group.operator === 'OR' ? 'contained' : 'text'}
            onClick={() => handleOperatorChange('OR')}
            disabled={readonly}
            sx={{ 
              borderRadius: 0, 
              minWidth: 50,
              px: 1.5,
              py: 0.25,
            }}
          >
            OR
          </Button>
        </Box>

        <Typography variant="body2" color="text.secondary">
          of the following:
        </Typography>

        <Box sx={{ flex: 1 }} />

        {!readonly && !isRoot && onDelete && (
          <Tooltip title="Delete group">
            <IconButton size="small" onClick={onDelete} color="error">
              <DeleteIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        )}
      </Box>

      {/* Conditions */}
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
        {group.conditions.map((condition, index) => (
          <Box key={condition.id}>
            {index > 0 && (
              <Box sx={{ display: 'flex', alignItems: 'center', my: 0.5 }}>
                <Chip 
                  label={group.operator} 
                  size="small" 
                  color={group.operator === 'AND' ? 'primary' : 'secondary'}
                  sx={{ fontSize: 10, height: 20 }}
                />
                <Box sx={{ flex: 1, height: 1, backgroundColor: 'divider', ml: 1 }} />
              </Box>
            )}
            {condition.type === 'rule' ? (
              <RuleEditor
                rule={condition}
                onChange={(updated) => handleConditionChange(index, updated)}
                onDelete={() => handleConditionDelete(index)}
                fields={fields}
                variables={variables}
                readonly={readonly}
                compact={compact}
              />
            ) : (
              <GroupEditor
                group={condition}
                onChange={(updated) => handleConditionChange(index, updated)}
                onDelete={() => handleConditionDelete(index)}
                fields={fields}
                variables={variables}
                depth={depth + 1}
                readonly={readonly}
                compact={compact}
              />
            )}
          </Box>
        ))}
      </Box>

      {/* Add Buttons */}
      {!readonly && (
        <Box sx={{ display: 'flex', gap: 1, mt: 1.5 }}>
          <Button
            size="small"
            startIcon={<AddIcon />}
            onClick={handleAddRule}
            variant="outlined"
          >
            Add Rule
          </Button>
          <Button
            size="small"
            startIcon={<AddGroupIcon />}
            onClick={handleAddGroup}
            variant="outlined"
            color="secondary"
          >
            Add Group
          </Button>
        </Box>
      )}
    </Paper>
  );
};

// ============================================================================
// Main Component
// ============================================================================

export const RuleBuilder: React.FC<RuleBuilderProps> = ({
  value,
  onChange,
  fields,
  variables = [],
  entityType,
  readonly,
  compact,
}) => {
  const [showJson, setShowJson] = useState(false);

  // Convert rule builder state to JSON expression
  const toJsonExpression = useCallback((group: ConditionGroup): string => {
    return JSON.stringify(group, null, 2);
  }, []);

  // Convert JSON expression back to rule builder state
  const fromJsonExpression = useCallback((json: string): ConditionGroup | null => {
    try {
      const parsed = JSON.parse(json);
      if (parsed.type === 'group') {
        return parsed as ConditionGroup;
      }
      return null;
    } catch {
      return null;
    }
  }, []);

  return (
    <Box>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
        <Typography variant="subtitle2" color="text.secondary">
          Condition Rules {entityType && `(${entityType})`}
        </Typography>
        <Tooltip title={showJson ? 'Show visual editor' : 'Show JSON'}>
          <IconButton size="small" onClick={() => setShowJson(!showJson)}>
            <CodeIcon fontSize="small" color={showJson ? 'primary' : 'inherit'} />
          </IconButton>
        </Tooltip>
      </Box>

      {/* Editor */}
      <Collapse in={!showJson}>
        <GroupEditor
          group={value}
          onChange={onChange}
          fields={fields}
          variables={variables}
          readonly={readonly}
          compact={compact}
          isRoot
        />
      </Collapse>

      {/* JSON View */}
      <Collapse in={showJson}>
        <TextField
          fullWidth
          multiline
          rows={10}
          value={toJsonExpression(value)}
          onChange={(e) => {
            const parsed = fromJsonExpression(e.target.value);
            if (parsed) {
              onChange(parsed);
            }
          }}
          disabled={readonly}
          sx={{
            '& .MuiInputBase-input': {
              fontFamily: 'monospace',
              fontSize: 12,
            },
          }}
        />
      </Collapse>
    </Box>
  );
};

// ============================================================================
// Utility Functions for External Use
// ============================================================================

export const createDefaultRuleGroup = (): ConditionGroup => createEmptyGroup('AND');

export const ruleGroupToExpression = (group: ConditionGroup): string => {
  const conditionToString = (condition: ConditionRule | ConditionGroup): string => {
    if (condition.type === 'group') {
      const parts = condition.conditions.map(c => conditionToString(c));
      return `(${parts.join(` ${condition.operator} `)})`;
    }
    
    const rule = condition;
    const valueStr = rule.valueType === 'variable' 
      ? `{{${rule.value}}}`
      : rule.valueType === 'expression'
      ? rule.value
      : JSON.stringify(rule.value);

    switch (rule.operator) {
      case 'equals': return `${rule.field} == ${valueStr}`;
      case 'notEquals': return `${rule.field} != ${valueStr}`;
      case 'greaterThan': return `${rule.field} > ${valueStr}`;
      case 'lessThan': return `${rule.field} < ${valueStr}`;
      case 'greaterThanOrEqual': return `${rule.field} >= ${valueStr}`;
      case 'lessThanOrEqual': return `${rule.field} <= ${valueStr}`;
      case 'contains': return `${rule.field}.contains(${valueStr})`;
      case 'isEmpty': return `${rule.field}.isEmpty()`;
      case 'isNotEmpty': return `!${rule.field}.isEmpty()`;
      case 'in': return `${rule.field} in ${valueStr}`;
      default: return `${rule.field} ${rule.operator} ${valueStr}`;
    }
  };

  return conditionToString(group);
};

export default RuleBuilder;
