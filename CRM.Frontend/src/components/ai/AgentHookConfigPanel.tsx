import React, { useState } from 'react';
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Box,
  FormControlLabel,
  Grid,
  Switch,
  TextField,
  Typography,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';

interface AgentHookConfig {
  onActivateScriptId?: string;
  onPlanScriptId?: string;
  onBeforeToolCallScriptId?: string;
  onAfterToolCallScriptId?: string;
  onResponseScriptId?: string;
  onErrorScriptId?: string;
  onDeactivateScriptId?: string;
  guardrailScriptId?: string;
  maxTokensPerCall?: number;
  maxCallsPerHour?: number;
  maxCostPerDay?: number;
}

interface Props {
  agentId: string;
  config?: AgentHookConfig;
  onChange?: (config: AgentHookConfig) => void;
}

const HOOKS: { key: keyof AgentHookConfig; label: string; description: string }[] = [
  { key: 'onActivateScriptId', label: 'On Activate', description: 'Script runs when agent session begins' },
  { key: 'onPlanScriptId', label: 'On Plan', description: 'Script runs when agent generates a plan' },
  { key: 'onBeforeToolCallScriptId', label: 'Before Tool Call', description: 'Script runs before any tool is invoked' },
  { key: 'onAfterToolCallScriptId', label: 'After Tool Call', description: 'Script runs after each tool invocation' },
  { key: 'onResponseScriptId', label: 'On Response', description: 'Script runs before agent sends response' },
  { key: 'onErrorScriptId', label: 'On Error', description: 'Script runs when agent encounters an error' },
  { key: 'onDeactivateScriptId', label: 'On Deactivate', description: 'Script runs when session ends' },
  { key: 'guardrailScriptId', label: 'Guardrail Script', description: 'Safety guardrail script (PII, toxicity, etc.)' },
];

const AgentHookConfigPanel: React.FC<Props> = ({ agentId, config = {}, onChange }) => {
  const [localConfig, setLocalConfig] = useState<AgentHookConfig>(config);
  const [budgetEnabled, setBudgetEnabled] = useState(
    !!(config.maxTokensPerCall || config.maxCallsPerHour || config.maxCostPerDay),
  );

  const handleHookChange = (key: keyof AgentHookConfig, value: string | number | undefined) => {
    const updated: AgentHookConfig = { ...localConfig, [key]: value || undefined };
    setLocalConfig(updated);
    onChange?.(updated);
  };

  return (
    <Box data-testid={`agent-hook-config-${agentId}`}>
      <Accordion defaultExpanded>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography variant="subtitle1" fontWeight="bold">
            Lifecycle Hooks
          </Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={2}>
            {HOOKS.map((hook) => (
              <Grid item xs={12} md={6} key={hook.key}>
                <TextField
                  fullWidth
                  label={hook.label}
                  helperText={hook.description}
                  value={(localConfig[hook.key] as string | undefined) ?? ''}
                  onChange={(e) => handleHookChange(hook.key, e.target.value)}
                  size="small"
                  placeholder="Script ID (optional)"
                />
              </Grid>
            ))}
          </Grid>
        </AccordionDetails>
      </Accordion>

      <Accordion>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography variant="subtitle1" fontWeight="bold">
            Budget Enforcement
          </Typography>
        </AccordionSummary>
        <AccordionDetails>
          <FormControlLabel
            control={
              <Switch
                checked={budgetEnabled}
                onChange={(e) => setBudgetEnabled(e.target.checked)}
              />
            }
            label="Enable Budget Limits"
            sx={{ mb: 2 }}
          />
          {budgetEnabled && (
            <Grid container spacing={2}>
              <Grid item xs={12} md={4}>
                <TextField
                  fullWidth
                  label="Max Tokens Per Call"
                  type="number"
                  value={localConfig.maxTokensPerCall ?? ''}
                  onChange={(e) =>
                    handleHookChange(
                      'maxTokensPerCall',
                      e.target.value ? Number(e.target.value) : undefined,
                    )
                  }
                  size="small"
                  inputProps={{ min: 1 }}
                />
              </Grid>
              <Grid item xs={12} md={4}>
                <TextField
                  fullWidth
                  label="Max Calls Per Hour"
                  type="number"
                  value={localConfig.maxCallsPerHour ?? ''}
                  onChange={(e) =>
                    handleHookChange(
                      'maxCallsPerHour',
                      e.target.value ? Number(e.target.value) : undefined,
                    )
                  }
                  size="small"
                  inputProps={{ min: 1 }}
                />
              </Grid>
              <Grid item xs={12} md={4}>
                <TextField
                  fullWidth
                  label="Max Cost Per Day ($)"
                  type="number"
                  value={localConfig.maxCostPerDay ?? ''}
                  onChange={(e) =>
                    handleHookChange(
                      'maxCostPerDay',
                      e.target.value ? Number(e.target.value) : undefined,
                    )
                  }
                  size="small"
                  inputProps={{ step: 0.01, min: 0 }}
                />
              </Grid>
            </Grid>
          )}
        </AccordionDetails>
      </Accordion>
    </Box>
  );
};

export default AgentHookConfigPanel;
