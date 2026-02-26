import { useState } from 'react';
import Editor from '@monaco-editor/react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  TextField,
  Button,
  Slider,
  Switch,
  FormControlLabel,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Alert,
  CircularProgress,
  Divider,
  Chip,
  Stepper,
  Step,
  StepLabel,
  IconButton,
  Tooltip,
} from '@mui/material';
import {
  Add as AddIcon,
  SmartToy as SmartToyIcon,
  ArrowBack as ArrowBackIcon,
  Save as SaveIcon,
  NavigateNext as NextIcon,
  NavigateBefore as PrevIcon,
  Settings as SettingsIcon,
  Psychology as PsychologyIcon,
  Security as SecurityIcon,
  Code as ScriptIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import agentAdminService from '../services/agentAdminService';
import { AgentType, AgentTypeLabels, CreateAgentRequest } from '../types/agents';

// ─── Constants ──────────────────────────────────────────────────────────────

const STEPS = ['Identity', 'Behavior', 'Safeguards'];

const agentTypeOptions = Object.entries(AgentTypeLabels).map(([key, label]) => ({
  value: Number(key),
  label,
}));

const pluginSuggestions = [
  'AccountPlugin',
  'ContactPlugin',
  'OpportunityPlugin',
  'LeadPlugin',
  'ServiceRequestPlugin',
  'EmailPlugin',
  'KnowledgeBasePlugin',
  'SearchPlugin',
  'CalendarPlugin',
  'NotificationPlugin',
  'QuotePlugin',
  'ContractPlugin',
];

// ─── Component ──────────────────────────────────────────────────────────────

const AgentCreatorPage = () => {
  const navigate = useNavigate();

  // Form state
  const [name, setName] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [description, setDescription] = useState('');
  const [agentType, setAgentType] = useState<number>(AgentType.GeneralAssistant);
  const [systemPrompt, setSystemPrompt] = useState('');
  const [selectedPlugins, setSelectedPlugins] = useState<string[]>([]);
  const [temperature, setTemperature] = useState<number>(0.3);
  const [maxTokens, setMaxTokens] = useState<number>(4096);
  const [modelOverride, setModelOverride] = useState('');
  const [requiresApproval, setRequiresApproval] = useState(false);
  const [approvalTier, setApprovalTier] = useState('');

  // Wizard state
  const [activeStep, setActiveStep] = useState(0);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [showScriptView, setShowScriptView] = useState(false);

  // Auto-generate Name from DisplayName
  const handleDisplayNameChange = (val: string) => {
    setDisplayName(val);
    if (!name || name === toSnakeCase(displayName)) {
      setName(toSnakeCase(val));
    }
  };

  const toSnakeCase = (str: string) =>
    str
      .trim()
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '_')
      .replace(/^_|_$/g, '');

  const togglePlugin = (plugin: string) => {
    setSelectedPlugins((prev) =>
      prev.includes(plugin) ? prev.filter((p) => p !== plugin) : [...prev, plugin]
    );
  };

  // Validation per step
  const isStepValid = (step: number): boolean => {
    switch (step) {
      case 0:
        return name.trim().length > 0 && displayName.trim().length > 0;
      case 1:
        return systemPrompt.trim().length > 0;
      case 2:
        return true; // safeguards are optional
      default:
        return false;
    }
  };

  const handleSubmit = async () => {
    if (!isStepValid(0) || !isStepValid(1)) {
      setError('Please fill in all required fields (Name, Display Name, System Prompt).');
      return;
    }

    setSaving(true);
    setError(null);

    try {
      const request: CreateAgentRequest = {
        name: name.trim(),
        displayName: displayName.trim(),
        description: description.trim(),
        agentType,
        systemPrompt: systemPrompt.trim(),
        allowedPlugins: selectedPlugins.join(','),
        requiresApproval,
        approvalTier: requiresApproval ? approvalTier.trim() || undefined : undefined,
        temperature,
        maxTokens,
        modelOverride: modelOverride.trim() || undefined,
      };

      await agentAdminService.createAgent(request);
      setSuccess(true);
      setTimeout(() => navigate('/admin/agents'), 1500);
    } catch (err: any) {
      const message = err?.response?.data?.message || err?.response?.data || 'Failed to create agent.';
      setError(typeof message === 'string' ? message : 'Failed to create agent.');
    } finally {
      setSaving(false);
    }
  };

  // ─── Render Steps ─────────────────────────────────────────────────────────

  const renderStep0 = () => (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <Typography variant="subtitle1" sx={{ fontWeight: 600, color: '#1C1B1F', display: 'flex', alignItems: 'center', gap: 1 }}>
        <SmartToyIcon sx={{ color: '#6750A4', fontSize: 20 }} /> Agent Identity
      </Typography>

      <TextField
        label="Display Name *"
        fullWidth
        value={displayName}
        onChange={(e) => handleDisplayNameChange(e.target.value)}
        placeholder="e.g. Sales Assistant"
        helperText="Human-readable name shown in the UI"
        sx={{ '& .MuiOutlinedInput-root': { borderRadius: 2 } }}
      />

      <TextField
        label="Internal Name *"
        fullWidth
        value={name}
        onChange={(e) => setName(e.target.value)}
        placeholder="e.g. sales_assistant"
        helperText="Unique identifier (auto-generated from display name)"
        sx={{ '& .MuiOutlinedInput-root': { borderRadius: 2 } }}
      />

      <TextField
        label="Description"
        fullWidth
        multiline
        minRows={2}
        maxRows={4}
        value={description}
        onChange={(e) => setDescription(e.target.value)}
        placeholder="Describe what this agent does and when to use it…"
        sx={{ '& .MuiOutlinedInput-root': { borderRadius: 2 } }}
      />

      <FormControl fullWidth>
        <InputLabel>Agent Type</InputLabel>
        <Select
          value={agentType}
          label="Agent Type"
          onChange={(e) => setAgentType(Number(e.target.value))}
          sx={{ borderRadius: 2 }}
        >
          {agentTypeOptions.map((opt) => (
            <MenuItem key={opt.value} value={opt.value}>
              {opt.label}
            </MenuItem>
          ))}
        </Select>
      </FormControl>
    </Box>
  );

  const renderStep1 = () => (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <Typography variant="subtitle1" sx={{ fontWeight: 600, color: '#1C1B1F', display: 'flex', alignItems: 'center', gap: 1 }}>
        <PsychologyIcon sx={{ color: '#6750A4', fontSize: 20 }} /> Behavior & Configuration
      </Typography>

      {/* System Prompt with Script/Monaco editor toggle */}
      <Box>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
          <Typography variant="body2" sx={{ fontWeight: 500, color: '#49454F' }}>System Prompt *</Typography>
          <Tooltip title={showScriptView ? 'Switch to plain text editor' : 'Open in script / code editor (Monaco)'}>
            <IconButton
              size="small"
              onClick={() => setShowScriptView(v => !v)}
              color={showScriptView ? 'primary' : 'default'}
              sx={showScriptView ? { backgroundColor: 'action.selected', borderRadius: 1 } : {}}
            >
              <ScriptIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        </Box>

        {showScriptView ? (
          <Box sx={{ border: '1px solid #CAC4D0', borderRadius: 2, overflow: 'hidden' }}>
            <Box sx={{ px: 1.5, py: 0.5, backgroundColor: '#2d2d2d', borderBottom: '1px solid rgba(255,255,255,0.12)', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
              <Typography variant="caption" sx={{ color: '#ccc', fontFamily: 'monospace', fontWeight: 600 }}>SYSTEM PROMPT — script editor</Typography>
              <Typography variant="caption" sx={{ color: '#888', fontFamily: 'monospace' }}>{systemPrompt.length} chars</Typography>
            </Box>
            <Editor
              height="220px"
              language="plaintext"
              value={systemPrompt}
              onChange={(val) => setSystemPrompt(val ?? '')}
              theme="vs-dark"
              options={{
                minimap: { enabled: false },
                fontSize: 13,
                wordWrap: 'on',
                lineNumbers: 'on',
                scrollBeyondLastLine: false,
                automaticLayout: true,
                tabSize: 2,
              }}
            />
          </Box>
        ) : (
          <TextField
            fullWidth
            multiline
            minRows={4}
            maxRows={10}
            value={systemPrompt}
            onChange={(e) => setSystemPrompt(e.target.value)}
            placeholder="You are a helpful CRM assistant that…"
            helperText={`${systemPrompt.length} characters`}
            sx={{ '& .MuiOutlinedInput-root': { borderRadius: 2 } }}
          />
        )}
      </Box>

      <Box>
        <Typography variant="body2" sx={{ fontWeight: 500, mb: 1, color: '#49454F' }}>
          Allowed Plugins
        </Typography>
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.75 }}>
          {pluginSuggestions.map((plugin) => (
            <Chip
              key={plugin}
              label={plugin.replace('Plugin', '')}
              size="small"
              onClick={() => togglePlugin(plugin)}
              variant={selectedPlugins.includes(plugin) ? 'filled' : 'outlined'}
              sx={{
                borderRadius: 2,
                fontWeight: 500,
                fontSize: 12,
                ...(selectedPlugins.includes(plugin)
                  ? { backgroundColor: '#6750A4', color: '#fff' }
                  : { borderColor: '#CAC4D0', color: '#49454F' }),
              }}
            />
          ))}
        </Box>
      </Box>

      <Divider />

      <Box>
        <Typography variant="body2" sx={{ fontWeight: 500, mb: 1, color: '#49454F' }}>
          Temperature: {temperature.toFixed(2)}
        </Typography>
        <Typography variant="caption" sx={{ color: '#79747E', display: 'block', mb: 1 }}>
          Lower = more deterministic, Higher = more creative
        </Typography>
        <Slider
          value={temperature}
          onChange={(_, val) => setTemperature(val as number)}
          min={0}
          max={1}
          step={0.05}
          valueLabelDisplay="auto"
          sx={{ color: '#6750A4', maxWidth: 400 }}
        />
      </Box>

      <TextField
        label="Max Tokens"
        type="number"
        value={maxTokens}
        onChange={(e) => setMaxTokens(Math.max(256, Math.min(32768, Number(e.target.value))))}
        helperText="Maximum response length (256 – 32,768)"
        sx={{ maxWidth: 200, '& .MuiOutlinedInput-root': { borderRadius: 2 } }}
      />

      <TextField
        label="Model Override"
        fullWidth
        value={modelOverride}
        onChange={(e) => setModelOverride(e.target.value)}
        placeholder="Leave blank to use the default model"
        helperText="e.g. gpt-4o, claude-3-sonnet, llama3"
        sx={{ '& .MuiOutlinedInput-root': { borderRadius: 2 } }}
      />
    </Box>
  );

  const renderStep2 = () => (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <Typography variant="subtitle1" sx={{ fontWeight: 600, color: '#1C1B1F', display: 'flex', alignItems: 'center', gap: 1 }}>
        <SecurityIcon sx={{ color: '#6750A4', fontSize: 20 }} /> Safeguards
      </Typography>

      <FormControlLabel
        control={
          <Switch
            checked={requiresApproval}
            onChange={(e) => setRequiresApproval(e.target.checked)}
            sx={{
              '& .MuiSwitch-switchBase.Mui-checked': { color: '#6750A4' },
              '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': { backgroundColor: '#6750A4' },
            }}
          />
        }
        label={
          <Box>
            <Typography variant="body2" sx={{ fontWeight: 500 }}>
              Require Human Approval
            </Typography>
            <Typography variant="caption" sx={{ color: '#79747E' }}>
              Actions marked with [RequiresApproval] will be held for review
            </Typography>
          </Box>
        }
      />

      {requiresApproval && (
        <TextField
          label="Approval Tier"
          fullWidth
          value={approvalTier}
          onChange={(e) => setApprovalTier(e.target.value)}
          placeholder="e.g. Tier1, Tier2, Manager"
          helperText="Which approval group handles this agent's requests"
          sx={{ ml: 6, maxWidth: 320, '& .MuiOutlinedInput-root': { borderRadius: 2 } }}
        />
      )}

      <Divider />

      {/* Summary Preview */}
      <Box>
        <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1.5, color: '#49454F' }}>
          Summary Preview
        </Typography>
        <Card sx={{ borderRadius: 2, border: '1px solid #E7E0EC', backgroundColor: '#FAFAFA' }}>
          <CardContent>
            <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
              {displayName || '(Unnamed Agent)'}
            </Typography>
            <Typography variant="caption" sx={{ color: '#79747E', display: 'block', mb: 1 }}>
              {name || '(no internal name)'}
            </Typography>
            <Typography variant="body2" sx={{ color: '#49454F', mb: 1 }}>
              {description || '(No description)'}
            </Typography>
            <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
              <Chip label={AgentTypeLabels[agentType as AgentType] || 'Unknown'} size="small" sx={{ fontSize: 10 }} />
              <Chip label={`Temp ${temperature}`} size="small" variant="outlined" sx={{ fontSize: 10 }} />
              <Chip label={`${maxTokens} tokens`} size="small" variant="outlined" sx={{ fontSize: 10 }} />
              {requiresApproval && <Chip label="Approval Required" size="small" color="warning" sx={{ fontSize: 10 }} />}
              {selectedPlugins.length > 0 && (
                <Chip label={`${selectedPlugins.length} plugins`} size="small" variant="outlined" sx={{ fontSize: 10 }} />
              )}
            </Box>
          </CardContent>
        </Card>
      </Box>
    </Box>
  );

  // ─── Main Render ──────────────────────────────────────────────────────────

  return (
    <Box sx={{ p: { xs: 2, md: 3 }, maxWidth: 820, mx: 'auto' }}>
      {/* Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3, gap: 1.5 }}>
        <Tooltip title="Back to Agent Management">
          <IconButton onClick={() => navigate('/admin/agents')} size="small">
            <ArrowBackIcon />
          </IconButton>
        </Tooltip>
        <AddIcon sx={{ fontSize: 32, color: '#6750A4' }} />
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 600, color: '#1C1B1F' }}>
            Create New Agent
          </Typography>
          <Typography variant="body2" sx={{ color: '#49454F' }}>
            Configure a new AI-powered CRM assistant
          </Typography>
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {success && (
        <Alert severity="success" sx={{ mb: 2 }}>
          Agent created successfully! Redirecting…
        </Alert>
      )}

      {/* Stepper */}
      <Stepper activeStep={activeStep} sx={{ mb: 4 }}>
        {STEPS.map((label, i) => (
          <Step key={label} completed={i < activeStep}>
            <StepLabel
              StepIconProps={{
                sx: {
                  '&.Mui-active': { color: '#6750A4' },
                  '&.Mui-completed': { color: '#6750A4' },
                },
              }}
            >
              {label}
            </StepLabel>
          </Step>
        ))}
      </Stepper>

      {/* Step Content */}
      <Card sx={{ borderRadius: 2, border: '1px solid #E7E0EC', mb: 3 }}>
        <CardContent sx={{ p: 3 }}>
          {activeStep === 0 && renderStep0()}
          {activeStep === 1 && renderStep1()}
          {activeStep === 2 && renderStep2()}
        </CardContent>
      </Card>

      {/* Navigation Buttons */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
        <Button
          variant="outlined"
          startIcon={<PrevIcon />}
          disabled={activeStep === 0}
          onClick={() => setActiveStep((s) => s - 1)}
          sx={{ textTransform: 'none', borderRadius: 2 }}
        >
          Back
        </Button>
        <Box sx={{ display: 'flex', gap: 1 }}>
          {activeStep < STEPS.length - 1 ? (
            <Button
              variant="contained"
              endIcon={<NextIcon />}
              disabled={!isStepValid(activeStep)}
              onClick={() => setActiveStep((s) => s + 1)}
              sx={{
                textTransform: 'none',
                borderRadius: 2,
                backgroundColor: '#6750A4',
                '&:hover': { backgroundColor: '#5639A0' },
              }}
            >
              Next
            </Button>
          ) : (
            <Button
              variant="contained"
              startIcon={saving ? <CircularProgress size={18} sx={{ color: '#fff' }} /> : <SaveIcon />}
              disabled={saving || success}
              onClick={handleSubmit}
              sx={{
                textTransform: 'none',
                borderRadius: 2,
                backgroundColor: '#6750A4',
                '&:hover': { backgroundColor: '#5639A0' },
              }}
            >
              {saving ? 'Creating…' : 'Create Agent'}
            </Button>
          )}
        </Box>
      </Box>
    </Box>
  );
};

export default AgentCreatorPage;
