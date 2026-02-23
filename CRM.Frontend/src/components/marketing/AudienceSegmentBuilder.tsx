/**
 * AudienceSegmentBuilder - Dynamic rule-based audience segment builder
 * Features: add/remove rules, field/operator/value selectors, AND/OR toggle
 */

import React, { useCallback, useMemo } from 'react';
import {
  Box,
  Grid,
  TextField,
  Button,
  Typography,
  IconButton,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Chip,
  Paper,
  Divider,
  Tooltip,
  ToggleButton,
  ToggleButtonGroup,
  SelectChangeEvent,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  People as PeopleIcon,
} from '@mui/icons-material';

// ============================================================================
// Types
// ============================================================================

export interface SegmentRule {
  id: string;
  field: string;
  operator: 'equals' | 'contains' | 'greaterThan' | 'lessThan' | 'between' | 'in';
  value: string | string[];
  conjunction: 'AND' | 'OR';
}

export interface AudienceSegmentBuilderProps {
  segments: SegmentRule[];
  onChange: (segments: SegmentRule[]) => void;
}

// ============================================================================
// Constants
// ============================================================================

interface FieldDef {
  value: string;
  label: string;
  type: 'text' | 'select' | 'date' | 'number';
  options?: string[];
}

const FIELD_DEFINITIONS: FieldDef[] = [
  { value: 'industry', label: 'Industry', type: 'select', options: ['Technology', 'Healthcare', 'Finance', 'Manufacturing', 'Retail', 'Education', 'Other'] },
  { value: 'city', label: 'City', type: 'text' },
  { value: 'country', label: 'Country', type: 'select', options: ['United States', 'United Kingdom', 'Canada', 'Germany', 'France', 'Australia', 'Japan', 'India', 'Other'] },
  { value: 'accountType', label: 'Account Type', type: 'select', options: ['Enterprise', 'SMB', 'Startup', 'Government', 'Non-Profit'] },
  { value: 'tags', label: 'Tags', type: 'text' },
  { value: 'lastActivityDate', label: 'Last Activity Date', type: 'date' },
  { value: 'createdDate', label: 'Created Date', type: 'date' },
  { value: 'revenue', label: 'Annual Revenue', type: 'number' },
  { value: 'employeeCount', label: 'Employee Count', type: 'number' },
  { value: 'leadScore', label: 'Lead Score', type: 'number' },
  { value: 'status', label: 'Status', type: 'select', options: ['Active', 'Inactive', 'Prospect', 'Churned'] },
];

const OPERATORS_BY_TYPE: Record<string, { value: SegmentRule['operator']; label: string }[]> = {
  text: [
    { value: 'equals', label: 'Equals' },
    { value: 'contains', label: 'Contains' },
    { value: 'in', label: 'In' },
  ],
  select: [
    { value: 'equals', label: 'Equals' },
    { value: 'in', label: 'In (multiple)' },
  ],
  date: [
    { value: 'equals', label: 'On' },
    { value: 'greaterThan', label: 'After' },
    { value: 'lessThan', label: 'Before' },
    { value: 'between', label: 'Between' },
  ],
  number: [
    { value: 'equals', label: 'Equals' },
    { value: 'greaterThan', label: 'Greater Than' },
    { value: 'lessThan', label: 'Less Than' },
    { value: 'between', label: 'Between' },
  ],
};

function generateId(): string {
  return `rule_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
}

function getFieldDef(fieldValue: string): FieldDef | undefined {
  return FIELD_DEFINITIONS.find((f) => f.value === fieldValue);
}

// ============================================================================
// Component
// ============================================================================

const AudienceSegmentBuilder: React.FC<AudienceSegmentBuilderProps> = ({
  segments,
  onChange,
}) => {
  const addRule = useCallback(() => {
    const newRule: SegmentRule = {
      id: generateId(),
      field: 'industry',
      operator: 'equals',
      value: '',
      conjunction: 'AND',
    };
    onChange([...segments, newRule]);
  }, [segments, onChange]);

  const removeRule = useCallback(
    (id: string) => {
      onChange(segments.filter((r) => r.id !== id));
    },
    [segments, onChange]
  );

  const updateRule = useCallback(
    (id: string, updates: Partial<SegmentRule>) => {
      onChange(
        segments.map((r) => (r.id === id ? { ...r, ...updates } : r))
      );
    },
    [segments, onChange]
  );

  const handleFieldChange = useCallback(
    (ruleId: string, newField: string) => {
      const fieldDef = getFieldDef(newField);
      const defaultOp = fieldDef
        ? (OPERATORS_BY_TYPE[fieldDef.type]?.[0]?.value || 'equals')
        : 'equals';
      updateRule(ruleId, { field: newField, operator: defaultOp, value: '' });
    },
    [updateRule]
  );

  // Estimated audience count (mock)
  const estimatedCount = useMemo(() => {
    if (segments.length === 0) return 0;
    const filledRules = segments.filter(
      (r) => r.field && r.value && (typeof r.value === 'string' ? r.value.trim() : r.value.length > 0)
    );
    if (filledRules.length === 0) return 0;
    // Mock calculation
    const base = 5000;
    const reduction = filledRules.length * 800;
    return Math.max(base - reduction + Math.floor(Math.random() * 200), 150);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [segments.length, segments.map((s) => `${s.field}:${s.operator}:${s.value}`).join(',')]);

  const renderValueInput = (rule: SegmentRule) => {
    const fieldDef = getFieldDef(rule.field);
    const fieldType = fieldDef?.type || 'text';

    if (fieldType === 'select' && rule.operator !== 'in') {
      return (
        <FormControl fullWidth size="small">
          <InputLabel>Value</InputLabel>
          <Select
            value={typeof rule.value === 'string' ? rule.value : ''}
            label="Value"
            onChange={(e: SelectChangeEvent) => updateRule(rule.id, { value: e.target.value })}
          >
            {(fieldDef?.options || []).map((opt) => (
              <MenuItem key={opt} value={opt}>
                {opt}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      );
    }

    if (fieldType === 'date') {
      return (
        <TextField
          fullWidth
          size="small"
          type="date"
          label="Value"
          value={typeof rule.value === 'string' ? rule.value : ''}
          onChange={(e) => updateRule(rule.id, { value: e.target.value })}
          InputLabelProps={{ shrink: true }}
        />
      );
    }

    if (fieldType === 'number') {
      return (
        <TextField
          fullWidth
          size="small"
          type="number"
          label="Value"
          value={typeof rule.value === 'string' ? rule.value : ''}
          onChange={(e) => updateRule(rule.id, { value: e.target.value })}
        />
      );
    }

    // Default: text input
    return (
      <TextField
        fullWidth
        size="small"
        label="Value"
        value={typeof rule.value === 'string' ? rule.value : Array.isArray(rule.value) ? rule.value.join(', ') : ''}
        onChange={(e) => updateRule(rule.id, { value: e.target.value })}
        placeholder={rule.operator === 'in' ? 'value1, value2, value3' : 'Enter value'}
      />
    );
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
        <Typography variant="subtitle1" fontWeight={600}>
          Audience Segment Rules
        </Typography>
        <Button startIcon={<AddIcon />} variant="outlined" size="small" onClick={addRule}>
          Add Rule
        </Button>
      </Box>

      {segments.length === 0 && (
        <Paper
          variant="outlined"
          sx={{
            p: 4,
            textAlign: 'center',
            color: 'text.secondary',
          }}
        >
          <PeopleIcon sx={{ fontSize: 40, mb: 1, opacity: 0.4 }} />
          <Typography variant="body2">
            No rules defined. Click &quot;Add Rule&quot; to start building your audience segment.
          </Typography>
        </Paper>
      )}

      {segments.map((rule, index) => {
        const fieldDef = getFieldDef(rule.field);
        const fieldType = fieldDef?.type || 'text';
        const operators = OPERATORS_BY_TYPE[fieldType] || OPERATORS_BY_TYPE.text;

        return (
          <Box key={rule.id}>
            {/* Conjunction toggle (between rules) */}
            {index > 0 && (
              <Box sx={{ display: 'flex', justifyContent: 'center', my: 1 }}>
                <ToggleButtonGroup
                  value={rule.conjunction}
                  exclusive
                  size="small"
                  onChange={(_, val) => {
                    if (val) updateRule(rule.id, { conjunction: val });
                  }}
                >
                  <ToggleButton value="AND">
                    <Typography variant="caption" fontWeight={600}>
                      AND
                    </Typography>
                  </ToggleButton>
                  <ToggleButton value="OR">
                    <Typography variant="caption" fontWeight={600}>
                      OR
                    </Typography>
                  </ToggleButton>
                </ToggleButtonGroup>
              </Box>
            )}

            <Paper variant="outlined" sx={{ p: 2, mb: 1 }}>
              <Grid container spacing={1.5} alignItems="center">
                {/* Field selector */}
                <Grid item xs={12} sm={3}>
                  <FormControl fullWidth size="small">
                    <InputLabel>Field</InputLabel>
                    <Select
                      value={rule.field}
                      label="Field"
                      onChange={(e: SelectChangeEvent) =>
                        handleFieldChange(rule.id, e.target.value)
                      }
                    >
                      {FIELD_DEFINITIONS.map((fd) => (
                        <MenuItem key={fd.value} value={fd.value}>
                          {fd.label}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Grid>

                {/* Operator selector */}
                <Grid item xs={12} sm={3}>
                  <FormControl fullWidth size="small">
                    <InputLabel>Operator</InputLabel>
                    <Select
                      value={rule.operator}
                      label="Operator"
                      onChange={(e: SelectChangeEvent) =>
                        updateRule(rule.id, {
                          operator: e.target.value as SegmentRule['operator'],
                        })
                      }
                    >
                      {operators.map((op) => (
                        <MenuItem key={op.value} value={op.value}>
                          {op.label}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Grid>

                {/* Value input */}
                <Grid item xs={12} sm={5}>
                  {renderValueInput(rule)}
                </Grid>

                {/* Delete button */}
                <Grid item xs={12} sm={1} sx={{ textAlign: 'center' }}>
                  <Tooltip title="Remove rule">
                    <IconButton
                      onClick={() => removeRule(rule.id)}
                      size="small"
                      color="error"
                    >
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </Grid>
              </Grid>
            </Paper>
          </Box>
        );
      })}

      {/* Estimated audience count */}
      {segments.length > 0 && (
        <>
          <Divider sx={{ my: 2 }} />
          <Paper
            variant="outlined"
            sx={{
              p: 2,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
            }}
          >
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <PeopleIcon color="primary" />
              <Typography variant="subtitle2">Estimated Audience</Typography>
            </Box>
            <Chip
              label={`~${estimatedCount.toLocaleString()} contacts`}
              color="primary"
              variant="outlined"
            />
          </Paper>
        </>
      )}
    </Box>
  );
};

export default AudienceSegmentBuilder;
