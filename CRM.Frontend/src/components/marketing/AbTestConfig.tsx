/**
 * MKT-008: A/B Test Configuration Component
 * Provides a lightweight toggle for enabling A/B subject line testing.
 * Stores config as JSON in the campaign's CampaignAbTestJson field.
 */

import {
  Box,
  Switch,
  FormControlLabel,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  RadioGroup,
  Radio,
  Typography,
  Stack,
  Collapse,
  SelectChangeEvent,
} from '@mui/material';
import { AbTestConfig as AbTestConfigType } from '../../types/marketing';

// ─── Hour options ──────────────────────────────────────────────────────────

const WINNER_HOURS = [
  { value: 2, label: '2 hours' },
  { value: 4, label: '4 hours' },
  { value: 8, label: '8 hours' },
  { value: 24, label: '24 hours' },
];

// ─── Props ──────────────────────────────────────────────────────────────────

export interface AbTestConfigProps {
  value: AbTestConfigType;
  onChange: (cfg: AbTestConfigType) => void;
}

// ─── Default ────────────────────────────────────────────────────────────────

export const defaultAbTestConfig = (): AbTestConfigType => ({
  enabled: false,
  subjectA: '',
  subjectB: '',
  autoSelectAfterHours: 4,
  winnerMetric: 'openRate',
});

// ─── Component ──────────────────────────────────────────────────────────────

export default function AbTestConfig({ value, onChange }: AbTestConfigProps) {
  const update = (patch: Partial<AbTestConfigType>) => onChange({ ...value, ...patch });

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      {/* Toggle */}
      <FormControlLabel
        control={
          <Switch
            checked={value.enabled}
            onChange={(e) => update({ enabled: e.target.checked })}
            color="primary"
          />
        }
        label={
          <Typography variant="body2" fontWeight={500}>
            Enable A/B Subject Line Test
          </Typography>
        }
      />

      {/* Config (shown only when enabled) */}
      <Collapse in={value.enabled} unmountOnExit>
        <Stack spacing={2}>
          {/* Variant subjects */}
          <TextField
            label="Variant A — Subject"
            value={value.subjectA}
            onChange={(e) => update({ subjectA: e.target.value })}
            fullWidth
            size="small"
            placeholder="Subject line for Variant A"
          />
          <TextField
            label="Variant B — Subject"
            value={value.subjectB}
            onChange={(e) => update({ subjectB: e.target.value })}
            fullWidth
            size="small"
            placeholder="Subject line for Variant B"
          />

          {/* Auto-select winner */}
          <FormControl size="small" sx={{ maxWidth: 220 }}>
            <InputLabel>Auto-select winner after</InputLabel>
            <Select
              value={String(value.autoSelectAfterHours)}
              label="Auto-select winner after"
              onChange={(e: SelectChangeEvent) =>
                update({ autoSelectAfterHours: Number(e.target.value) })
              }
            >
              {WINNER_HOURS.map((h) => (
                <MenuItem key={h.value} value={String(h.value)}>
                  {h.label}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          {/* Winner metric */}
          <Box>
            <Typography variant="caption" color="text.secondary" gutterBottom>
              Winner metric
            </Typography>
            <RadioGroup
              row
              value={value.winnerMetric}
              onChange={(e) =>
                update({ winnerMetric: e.target.value as 'openRate' | 'clickRate' })
              }
            >
              <FormControlLabel value="openRate" control={<Radio size="small" />} label="Open Rate" />
              <FormControlLabel value="clickRate" control={<Radio size="small" />} label="Click Rate" />
            </RadioGroup>
          </Box>
        </Stack>
      </Collapse>
    </Box>
  );
}
