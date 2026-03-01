import React, { useState } from 'react';
import {
  Alert,
  Box,
  FormControlLabel,
  Grid,
  Switch,
  TextField,
  Typography,
} from '@mui/material';

interface GuardrailConfig {
  enablePiiDetection: boolean;
  enablePromptInjectionDetection: boolean;
  enableToxicityFilter: boolean;
  customGuardrailScriptId?: string;
  blockedKeywords?: string[];
}

interface Props {
  config?: GuardrailConfig;
  onChange?: (config: GuardrailConfig) => void;
}

const DEFAULT_CONFIG: GuardrailConfig = {
  enablePiiDetection: true,
  enablePromptInjectionDetection: true,
  enableToxicityFilter: false,
  blockedKeywords: [],
};

const GuardrailConfigPanel: React.FC<Props> = ({ config = DEFAULT_CONFIG, onChange }) => {
  const [localConfig, setLocalConfig] = useState<GuardrailConfig>(config);

  const update = (patch: Partial<GuardrailConfig>) => {
    const updated = { ...localConfig, ...patch };
    setLocalConfig(updated);
    onChange?.(updated);
  };

  return (
    <Box data-testid="guardrail-config-panel">
      <Typography variant="subtitle1" fontWeight="bold" gutterBottom>
        Guardrail Configuration
      </Typography>
      <Alert severity="info" sx={{ mb: 2 }}>
        Built-in guardrails run before custom scripts. PII detection blocks SSNs and credit card
        numbers. Prompt injection detection blocks jailbreak attempts.
      </Alert>
      <Grid container spacing={2}>
        <Grid item xs={12}>
          <FormControlLabel
            control={
              <Switch
                checked={localConfig.enablePiiDetection}
                onChange={(e) => update({ enablePiiDetection: e.target.checked })}
              />
            }
            label="PII Detection (SSN, Credit Cards)"
          />
        </Grid>
        <Grid item xs={12}>
          <FormControlLabel
            control={
              <Switch
                checked={localConfig.enablePromptInjectionDetection}
                onChange={(e) => update({ enablePromptInjectionDetection: e.target.checked })}
              />
            }
            label="Prompt Injection Detection"
          />
        </Grid>
        <Grid item xs={12}>
          <FormControlLabel
            control={
              <Switch
                checked={localConfig.enableToxicityFilter}
                onChange={(e) => update({ enableToxicityFilter: e.target.checked })}
              />
            }
            label="Toxicity Filter (requires AI model)"
          />
        </Grid>
        <Grid item xs={12}>
          <TextField
            fullWidth
            label="Custom Guardrail Script ID"
            value={localConfig.customGuardrailScriptId ?? ''}
            onChange={(e) =>
              update({ customGuardrailScriptId: e.target.value || undefined })
            }
            size="small"
            helperText="Optional: Script ID for custom guardrail logic"
          />
        </Grid>
      </Grid>
    </Box>
  );
};

export default GuardrailConfigPanel;
