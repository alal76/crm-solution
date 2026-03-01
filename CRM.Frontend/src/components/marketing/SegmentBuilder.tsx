/**
 * MKT-007: Campaign Recipient Segmentation — SegmentBuilder
 * Dynamic filter-rule builder. Saves SegmentRulesJson to the campaign via PATCH.
 */

import { useState, useCallback } from 'react';
import {
  Box,
  Button,
  Typography,
  IconButton,
  Select,
  MenuItem,
  TextField,
  FormControl,
  InputLabel,
  ToggleButtonGroup,
  ToggleButton,
  Stack,
  Divider,
  CircularProgress,
  Alert,
  Tooltip,
  SelectChangeEvent,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  People as PeopleIcon,
  Save as SaveIcon,
} from '@mui/icons-material';
import { SegmentConfig, SegmentRule } from '../../types/marketing';
import marketingService from '../../services/marketingService';

// ─── Field definitions ─────────────────────────────────────────────────────

type FieldType = 'string' | 'number' | 'enum';

interface FieldDef {
  label: string;
  type: FieldType;
  operators: string[];
  options?: string[]; // for enum type
}

const FIELD_DEFS: Record<SegmentRule['field'], FieldDef> = {
  status: {
    label: 'Status',
    type: 'enum',
    operators: ['is one of'],
    options: ['New', 'Contacted', 'Qualified', 'Unqualified', 'Converted'],
  },
  leadScore: {
    label: 'Lead Score',
    type: 'number',
    operators: ['>=', '<=', '='],
  },
  industry: {
    label: 'Industry',
    type: 'string',
    operators: ['is', 'is not', 'contains'],
  },
  country: {
    label: 'Country',
    type: 'string',
    operators: ['is', 'is not', 'contains'],
  },
  source: {
    label: 'Source',
    type: 'string',
    operators: ['is', 'is not', 'contains'],
  },
  tag: {
    label: 'Tag',
    type: 'string',
    operators: ['is', 'is not', 'contains'],
  },
};

const FIELD_KEYS = Object.keys(FIELD_DEFS) as SegmentRule['field'][];

function makeId(): string {
  return Math.random().toString(36).slice(2, 10); // NOSONAR - non-security use: UI element ID generation
}

function emptyRule(): SegmentRule {
  return {
    id: makeId(),
    field: 'status',
    operator: 'is one of',
    value: '',
  };
}

// ─── Props ──────────────────────────────────────────────────────────────────

export interface SegmentBuilderProps {
  campaignId: number;
  /** Initial segment config (parsed from campaign.SegmentRulesJson) */
  initialConfig?: SegmentConfig;
  /** Called after successful save */
  onSaved?: (config: SegmentConfig) => void;
}

// ─── Component ──────────────────────────────────────────────────────────────

export default function SegmentBuilder({ campaignId, initialConfig, onSaved }: SegmentBuilderProps) {
  const [matchMode, setMatchMode] = useState<'AND' | 'OR'>(initialConfig?.matchMode ?? 'AND');
  const [rules, setRules] = useState<SegmentRule[]>(initialConfig?.rules ?? [emptyRule()]);
  const [previewCount, setPreviewCount] = useState<number | null>(null);
  const [previewing, setPreviewing] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [saved, setSaved] = useState(false);

  // ── Rule management ─────────────────────────────────────────────────────

  const addRule = () => setRules((r) => [...r, emptyRule()]);

  const removeRule = (id: string) => setRules((r) => r.filter((rule) => rule.id !== id));

  const updateRule = useCallback((id: string, patch: Partial<SegmentRule>) => {
    setRules((prev) =>
      prev.map((r) => {
        if (r.id !== id) return r;
        const updated = { ...r, ...patch };
        // Reset operator and value when field changes
        if (patch.field) {
          const def = FIELD_DEFS[patch.field];
          updated.operator = def.operators[0] as SegmentRule['operator'];
          updated.value = '';
        }
        return updated;
      })
    );
  }, []);

  // ── Preview ─────────────────────────────────────────────────────────────

  const handlePreview = async () => {
    setPreviewing(true);
    setPreviewCount(null);
    try {
      const config: SegmentConfig = { matchMode, rules };
      const count = await marketingService.previewRecipients(campaignId, JSON.stringify(config));
      setPreviewCount(count);
    } catch {
      setPreviewCount(0);
    } finally {
      setPreviewing(false);
    }
  };

  // ── Save ─────────────────────────────────────────────────────────────────

  const handleSave = async () => {
    setSaving(true);
    setError(null);
    setSaved(false);
    try {
      const config: SegmentConfig = { matchMode, rules };
      await marketingService.updateCampaign(campaignId, {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        segmentRulesJson: JSON.stringify(config),
      } as any);
      setSaved(true);
      onSaved?.(config);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to save segment');
    } finally {
      setSaving(false);
    }
  };

  // ── Render ───────────────────────────────────────────────────────────────

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      {/* Match mode */}
      <Stack direction="row" alignItems="center" spacing={2}>
        <Typography variant="body2" fontWeight={500}>
          Match:
        </Typography>
        <ToggleButtonGroup
          value={matchMode}
          exclusive
          onChange={(_, v) => v && setMatchMode(v)}
          size="small"
        >
          <ToggleButton value="AND">ALL rules (AND)</ToggleButton>
          <ToggleButton value="OR">ANY rule (OR)</ToggleButton>
        </ToggleButtonGroup>
      </Stack>

      <Divider />

      {/* Rules */}
      {rules.map((rule, idx) => {
        const def = FIELD_DEFS[rule.field];
        return (
          <Stack key={rule.id} direction="row" spacing={1} alignItems="center">
            <Typography variant="caption" color="text.secondary" sx={{ minWidth: 18 }}>
              {idx + 1}.
            </Typography>

            {/* Field */}
            <FormControl size="small" sx={{ minWidth: 130 }}>
              <InputLabel>Field</InputLabel>
              <Select
                value={rule.field}
                label="Field"
                onChange={(e: SelectChangeEvent) =>
                  updateRule(rule.id, { field: e.target.value as SegmentRule['field'] })
                }
              >
                {FIELD_KEYS.map((k) => (
                  <MenuItem key={k} value={k}>
                    {FIELD_DEFS[k].label}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            {/* Operator */}
            <FormControl size="small" sx={{ minWidth: 120 }}>
              <InputLabel>Operator</InputLabel>
              <Select
                value={rule.operator}
                label="Operator"
                onChange={(e: SelectChangeEvent) =>
                  updateRule(rule.id, { operator: e.target.value as SegmentRule['operator'] })
                }
              >
                {def.operators.map((op) => (
                  <MenuItem key={op} value={op}>
                    {op}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            {/* Value */}
            <TextField
              size="small"
              sx={{ flex: 1 }}
              label="Value"
              value={rule.value}
              onChange={(e) => updateRule(rule.id, { value: e.target.value })}
              placeholder={def.type === 'number' ? '0' : 'Enter value…'}
              type={def.type === 'number' ? 'number' : 'text'}
            />

            {/* Remove */}
            <Tooltip title="Remove rule">
              <IconButton
                size="small"
                onClick={() => removeRule(rule.id)}
                disabled={rules.length === 1}
                color="error"
              >
                <DeleteIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          </Stack>
        );
      })}

      {/* Add rule button */}
      <Box>
        <Button startIcon={<AddIcon />} size="small" variant="outlined" onClick={addRule}>
          Add Rule
        </Button>
      </Box>

      <Divider />

      {/* Preview + Save row */}
      <Stack direction="row" spacing={1} alignItems="center">
        <Button
          variant="outlined"
          startIcon={previewing ? <CircularProgress size={14} /> : <PeopleIcon />}
          onClick={handlePreview}
          disabled={previewing}
          size="small"
        >
          Preview recipients
        </Button>
        {previewCount !== null && (
          <Typography variant="body2" color="text.secondary">
            ~<strong>{previewCount}</strong> recipients match
          </Typography>
        )}

        <Box sx={{ flex: 1 }} />

        <Button
          variant="contained"
          startIcon={saving ? <CircularProgress size={14} /> : <SaveIcon />}
          onClick={handleSave}
          disabled={saving}
          size="small"
        >
          Save Segment
        </Button>
      </Stack>

      {error && <Alert severity="error" onClose={() => setError(null)}>{error}</Alert>}
      {saved && <Alert severity="success" onClose={() => setSaved(false)}>Segment saved successfully.</Alert>}
    </Box>
  );
}
