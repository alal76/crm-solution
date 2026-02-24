/**
 * EventFilterBuilder - Build complex event filter conditions for webhooks
 * Implements TODO-INT001-24
 */

import React, { useState, useCallback } from 'react';
import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Grid,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Button,
  IconButton,
  Chip,
  Typography,
  Divider,
  Stack,
  Tooltip,
  Paper,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  ContentCopy as CopyIcon,
} from '@mui/icons-material';

// Types
export interface FilterCondition {
  id: string;
  field: string;
  operator: FilterOperator;
  value: string;
}

export interface FilterGroup {
  id: string;
  logic: 'AND' | 'OR';
  conditions: FilterCondition[];
}

export type FilterOperator =
  | 'equals'
  | 'not_equals'
  | 'contains'
  | 'not_contains'
  | 'starts_with'
  | 'ends_with'
  | 'greater_than'
  | 'less_than'
  | 'greater_or_equal'
  | 'less_or_equal'
  | 'is_null'
  | 'is_not_null'
  | 'in'
  | 'not_in';

interface EventFilterBuilderProps {
  filterGroups: FilterGroup[];
  onChange: (groups: FilterGroup[]) => void;
  availableFields?: FilterField[];
  eventType?: string;
}

export interface FilterField {
  name: string;
  label: string;
  type: 'string' | 'number' | 'boolean' | 'date' | 'enum';
  enumValues?: string[];
}

const defaultFields: FilterField[] = [
  { name: 'event.type', label: 'Event Type', type: 'string' },
  { name: 'entity.id', label: 'Entity ID', type: 'number' },
  { name: 'entity.type', label: 'Entity Type', type: 'string' },
  { name: 'entity.name', label: 'Entity Name', type: 'string' },
  { name: 'entity.status', label: 'Status', type: 'string' },
  { name: 'entity.priority', label: 'Priority', type: 'enum', enumValues: ['Low', 'Medium', 'High', 'Critical'] },
  { name: 'entity.owner_id', label: 'Owner ID', type: 'number' },
  { name: 'entity.created_at', label: 'Created At', type: 'date' },
  { name: 'entity.updated_at', label: 'Updated At', type: 'date' },
  { name: 'user.id', label: 'Triggered By User ID', type: 'number' },
  { name: 'user.email', label: 'Triggered By Email', type: 'string' },
  { name: 'source.ip', label: 'Source IP', type: 'string' },
  { name: 'metadata.custom_field', label: 'Custom Field', type: 'string' },
];

const operatorLabels: Record<FilterOperator, string> = {
  equals: 'Equals',
  not_equals: 'Does Not Equal',
  contains: 'Contains',
  not_contains: 'Does Not Contain',
  starts_with: 'Starts With',
  ends_with: 'Ends With',
  greater_than: 'Greater Than',
  less_than: 'Less Than',
  greater_or_equal: 'Greater Than or Equal',
  less_or_equal: 'Less Than or Equal',
  is_null: 'Is Empty',
  is_not_null: 'Is Not Empty',
  in: 'Is In List',
  not_in: 'Is Not In List',
};

const getOperatorsForType = (type: FilterField['type']): FilterOperator[] => {
  const baseOperators: FilterOperator[] = ['equals', 'not_equals', 'is_null', 'is_not_null'];
  
  switch (type) {
    case 'string':
      return [...baseOperators, 'contains', 'not_contains', 'starts_with', 'ends_with', 'in', 'not_in'];
    case 'number':
      return [...baseOperators, 'greater_than', 'less_than', 'greater_or_equal', 'less_or_equal', 'in', 'not_in'];
    case 'boolean':
      return ['equals', 'not_equals'];
    case 'date':
      return [...baseOperators, 'greater_than', 'less_than', 'greater_or_equal', 'less_or_equal'];
    case 'enum':
      return ['equals', 'not_equals', 'in', 'not_in'];
    default:
      return baseOperators;
  }
};

const generateId = (): string => Math.random().toString(36).substring(2, 9);

export const EventFilterBuilder: React.FC<EventFilterBuilderProps> = ({
  filterGroups,
  onChange,
  availableFields = defaultFields,
}) => {
  const [jsonView, setJsonView] = useState(false);

  const addFilterGroup = useCallback(() => {
    const newGroup: FilterGroup = {
      id: generateId(),
      logic: 'AND',
      conditions: [
        { id: generateId(), field: 'entity.type', operator: 'equals', value: '' },
      ],
    };
    onChange([...filterGroups, newGroup]);
  }, [filterGroups, onChange]);

  const removeFilterGroup = useCallback((groupId: string) => {
    onChange(filterGroups.filter((g) => g.id !== groupId));
  }, [filterGroups, onChange]);

  const updateGroupLogic = useCallback((groupId: string, logic: 'AND' | 'OR') => {
    onChange(
      filterGroups.map((g) => (g.id === groupId ? { ...g, logic } : g))
    );
  }, [filterGroups, onChange]);

  const addCondition = useCallback((groupId: string) => {
    onChange(
      filterGroups.map((g) =>
        g.id === groupId
          ? {
              ...g,
              conditions: [
                ...g.conditions,
                { id: generateId(), field: 'entity.type', operator: 'equals' as FilterOperator, value: '' },
              ],
            }
          : g
      )
    );
  }, [filterGroups, onChange]);

  const removeCondition = useCallback((groupId: string, conditionId: string) => {
    onChange(
      filterGroups.map((g) =>
        g.id === groupId
          ? { ...g, conditions: g.conditions.filter((c) => c.id !== conditionId) }
          : g
      )
    );
  }, [filterGroups, onChange]);

  const updateCondition = useCallback(
    (groupId: string, conditionId: string, updates: Partial<FilterCondition>) => {
      onChange(
        filterGroups.map((g) =>
          g.id === groupId
            ? {
                ...g,
                conditions: g.conditions.map((c) =>
                  c.id === conditionId ? { ...c, ...updates } : c
                ),
              }
            : g
        )
      );
    },
    [filterGroups, onChange]
  );

  const getFieldType = (fieldName: string): FilterField['type'] => {
    const field = availableFields.find((f) => f.name === fieldName);
    return field?.type || 'string';
  };

  const getFieldEnumValues = (fieldName: string): string[] | undefined => {
    const field = availableFields.find((f) => f.name === fieldName);
    return field?.enumValues;
  };

  const copyAsJson = () => {
    const json = JSON.stringify(filterGroups, null, 2);
    navigator.clipboard.writeText(json);
  };

  const renderConditionValueInput = (
    groupId: string,
    condition: FilterCondition,
    fieldType: FilterField['type']
  ) => {
    const enumValues = getFieldEnumValues(condition.field);
    const noValueRequired = ['is_null', 'is_not_null'].includes(condition.operator);

    if (noValueRequired) {
      return (
        <Typography variant="body2" color="text.secondary" sx={{ py: 1 }}>
          (No value needed)
        </Typography>
      );
    }

    if (fieldType === 'enum' && enumValues) {
      return (
        <FormControl fullWidth size="small">
          <InputLabel>Value</InputLabel>
          <Select
            value={condition.value}
            onChange={(e) =>
              updateCondition(groupId, condition.id, { value: e.target.value })
            }
            label="Value"
          >
            {enumValues.map((val) => (
              <MenuItem key={val} value={val}>
                {val}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      );
    }

    if (fieldType === 'boolean') {
      return (
        <FormControl fullWidth size="small">
          <InputLabel>Value</InputLabel>
          <Select
            value={condition.value}
            onChange={(e) =>
              updateCondition(groupId, condition.id, { value: e.target.value })
            }
            label="Value"
          >
            <MenuItem value="true">True</MenuItem>
            <MenuItem value="false">False</MenuItem>
          </Select>
        </FormControl>
      );
    }

    if (fieldType === 'date') {
      return (
        <TextField
          fullWidth
          type="datetime-local"
          size="small"
          value={condition.value}
          onChange={(e) =>
            updateCondition(groupId, condition.id, { value: e.target.value })
          }
          InputLabelProps={{ shrink: true }}
          label="Value"
        />
      );
    }

    return (
      <TextField
        fullWidth
        size="small"
        value={condition.value}
        onChange={(e) =>
          updateCondition(groupId, condition.id, { value: e.target.value })
        }
        label="Value"
        placeholder={
          ['in', 'not_in'].includes(condition.operator)
            ? 'Comma-separated values'
            : 'Enter value'
        }
        type={fieldType === 'number' ? 'number' : 'text'}
      />
    );
  };

  return (
    <Card>
      <CardHeader
        title="Event Filters"
        subheader="Build conditions to filter which events trigger this webhook"
        action={
          <Stack direction="row" spacing={1}>
            <Tooltip title="Copy as JSON">
              <IconButton onClick={copyAsJson} size="small">
                <CopyIcon />
              </IconButton>
            </Tooltip>
            <Button
              size="small"
              variant={jsonView ? 'contained' : 'outlined'}
              onClick={() => setJsonView(!jsonView)}
            >
              {jsonView ? 'Visual' : 'JSON'}
            </Button>
          </Stack>
        }
      />
      <CardContent>
        {jsonView ? (
          <Paper variant="outlined" sx={{ p: 2, bgcolor: 'grey.50' }}>
            <pre style={{ margin: 0, whiteSpace: 'pre-wrap', wordBreak: 'break-all' }}>
              {JSON.stringify(filterGroups, null, 2)}
            </pre>
          </Paper>
        ) : (
          <Stack spacing={2}>
            {filterGroups.map((group, groupIndex) => (
              <Card key={group.id} variant="outlined">
                <Box sx={{ p: 2 }}>
                  <Box
                    display="flex"
                    justifyContent="space-between"
                    alignItems="center"
                    mb={2}
                  >
                    <Stack direction="row" spacing={1} alignItems="center">
                      <Typography variant="subtitle2" color="text.secondary">
                        Group {groupIndex + 1}
                      </Typography>
                      <Chip
                        label={group.logic}
                        size="small"
                        color={group.logic === 'AND' ? 'primary' : 'secondary'}
                        onClick={() =>
                          updateGroupLogic(group.id, group.logic === 'AND' ? 'OR' : 'AND')
                        }
                      />
                    </Stack>
                    <IconButton
                      size="small"
                      color="error"
                      onClick={() => removeFilterGroup(group.id)}
                    >
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </Box>

                  <Stack spacing={2}>
                    {group.conditions.map((condition, index) => (
                      <Box key={condition.id}>
                        {index > 0 && (
                          <Typography
                            variant="caption"
                            color="primary"
                            sx={{ display: 'block', textAlign: 'center', mb: 1 }}
                          >
                            {group.logic}
                          </Typography>
                        )}
                        <Grid container spacing={2} alignItems="center">
                          <Grid item xs={12} sm={3}>
                            <FormControl fullWidth size="small">
                              <InputLabel>Field</InputLabel>
                              <Select
                                value={condition.field}
                                onChange={(e) =>
                                  updateCondition(group.id, condition.id, {
                                    field: e.target.value,
                                    operator: 'equals',
                                    value: '',
                                  })
                                }
                                label="Field"
                              >
                                {availableFields.map((field) => (
                                  <MenuItem key={field.name} value={field.name}>
                                    {field.label}
                                  </MenuItem>
                                ))}
                              </Select>
                            </FormControl>
                          </Grid>
                          <Grid item xs={12} sm={3}>
                            <FormControl fullWidth size="small">
                              <InputLabel>Operator</InputLabel>
                              <Select
                                value={condition.operator}
                                onChange={(e) =>
                                  updateCondition(group.id, condition.id, {
                                    operator: e.target.value as FilterOperator,
                                  })
                                }
                                label="Operator"
                              >
                                {getOperatorsForType(getFieldType(condition.field)).map((op) => (
                                  <MenuItem key={op} value={op}>
                                    {operatorLabels[op]}
                                  </MenuItem>
                                ))}
                              </Select>
                            </FormControl>
                          </Grid>
                          <Grid item xs={12} sm={5}>
                            {renderConditionValueInput(
                              group.id,
                              condition,
                              getFieldType(condition.field)
                            )}
                          </Grid>
                          <Grid item xs={12} sm={1}>
                            <IconButton
                              size="small"
                              color="error"
                              onClick={() => removeCondition(group.id, condition.id)}
                              disabled={group.conditions.length <= 1}
                            >
                              <DeleteIcon fontSize="small" />
                            </IconButton>
                          </Grid>
                        </Grid>
                      </Box>
                    ))}
                  </Stack>

                  <Button
                    startIcon={<AddIcon />}
                    size="small"
                    onClick={() => addCondition(group.id)}
                    sx={{ mt: 2 }}
                  >
                    Add Condition
                  </Button>
                </Box>
              </Card>
            ))}

            {filterGroups.length > 1 && (
              <Divider>
                <Chip label="OR" size="small" color="secondary" />
              </Divider>
            )}

            <Button
              variant="outlined"
              startIcon={<AddIcon />}
              onClick={addFilterGroup}
              fullWidth
            >
              Add Filter Group
            </Button>

            {filterGroups.length === 0 && (
              <Typography
                variant="body2"
                color="text.secondary"
                textAlign="center"
                sx={{ py: 2 }}
              >
                No filters configured. All events will trigger this webhook.
              </Typography>
            )}
          </Stack>
        )}
      </CardContent>
    </Card>
  );
};

export default EventFilterBuilder;
