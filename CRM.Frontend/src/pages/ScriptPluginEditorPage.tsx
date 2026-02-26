import { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Button,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Switch,
  FormControlLabel,
  CircularProgress,
  Alert,
  Snackbar,
  Paper,
  Stack,
  Divider,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Chip,
  SelectChangeEvent,
} from '@mui/material';
import {
  ArrowBack as ArrowBackIcon,
  Save as SaveIcon,
  PlayArrow as PlayArrowIcon,
  ExpandMore as ExpandMoreIcon,
  Code as CodeIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
} from '@mui/icons-material';
import { useNavigate, useParams } from 'react-router-dom';
import {
  getScriptPlugin,
  createScriptPlugin,
  updateScriptPlugin,
  testScriptPlugin,
  ScriptLanguage,
  ScriptPluginTestResult,
} from '../services/scriptPluginService';
import logger from '../services/logger';

// ─── Constants ────────────────────────────────────────────────────────────

const LANGUAGE_OPTIONS: { value: ScriptLanguage; label: string }[] = [
  { value: 0, label: 'JavaScript' },
  { value: 1, label: 'Python' },
  { value: 2, label: 'C#' },
];

const LANGUAGE_LABELS: Record<number, { label: string; color: 'primary' | 'success' | 'secondary' }> = {
  0: { label: 'JavaScript', color: 'primary' },
  1: { label: 'Python', color: 'success' },
  2: { label: 'C#', color: 'secondary' },
};

const CODE_PLACEHOLDERS: Record<number, string> = {
  0: `// JavaScript plugin example
// Available: variables, context, log()
const result = variables.input ?? 'hello';
log('Running with: ' + result);
return { output: result };`,
  1: `# Python plugin example
# Available: variables, context, log()
result = variables.get('input', 'hello')
log(f'Running with: {result}')
return {'output': result}`,
  2: `// C# plugin example
// Available: variables, context, Log()
var result = variables.GetValueOrDefault("input", "hello");
Log($"Running with: {result}");
return new { output = result };`,
};

// ─── Form state ───────────────────────────────────────────────────────────

interface FormValues {
  name: string;
  description: string;
  language: ScriptLanguage;
  code: string;
  parameterSchema: string;
  returnValueDescription: string;
  isActive: boolean;
}

const INITIAL_FORM: FormValues = {
  name: '',
  description: '',
  language: 0,
  code: '',
  parameterSchema: '',
  returnValueDescription: '',
  isActive: true,
};

// ─── Component ────────────────────────────────────────────────────────────

const ScriptPluginEditorPage = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const isEditMode = id !== undefined && id !== 'new';
  const pluginId = isEditMode ? parseInt(id as string, 10) : null;

  // Form
  const [form, setForm] = useState<FormValues>(INITIAL_FORM);
  const [formErrors, setFormErrors] = useState<Partial<Record<keyof FormValues, string>>>({});

  // Loading / saving
  const [loadingData, setLoadingData] = useState(isEditMode);
  const [saving, setSaving] = useState(false);
  const [loadError, setLoadError] = useState<string | null>(null);

  // Snackbar
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({
    open: false,
    message: '',
    severity: 'success',
  });

  // Test Run
  const [testVariables, setTestVariables] = useState('{}');
  const [testContext, setTestContext] = useState('{}');
  const [testRunning, setTestRunning] = useState(false);
  const [testResult, setTestResult] = useState<ScriptPluginTestResult | null>(null);
  const [testError, setTestError] = useState<string | null>(null);

  // ── Load existing plugin ─────────────────────────────────────────────────

  useEffect(() => {
    if (!isEditMode || pluginId === null) return;
    setLoadingData(true);
    setLoadError(null);

    getScriptPlugin(pluginId)
      .then((plugin) => {
        setForm({
          name: plugin.name,
          description: plugin.description ?? '',
          language: plugin.language,
          code: plugin.code,
          parameterSchema: plugin.parameterSchema ?? '',
          returnValueDescription: plugin.returnValueDescription ?? '',
          isActive: plugin.isActive,
        });
      })
      .catch((err) => {
        logger.error('ScriptPluginEditorPage: failed to load plugin', err);
        setLoadError('Failed to load plugin data. It may have been deleted.');
      })
      .finally(() => {
        setLoadingData(false);
      });
  }, [isEditMode, pluginId]);

  // ── Validation ───────────────────────────────────────────────────────────

  const validate = (): boolean => {
    const errors: Partial<Record<keyof FormValues, string>> = {};
    if (!form.name.trim()) errors.name = 'Name is required.';
    if (!form.code.trim()) errors.code = 'Code is required.';

    // Validate JSON fields if provided
    if (form.parameterSchema.trim()) {
      try {
        JSON.parse(form.parameterSchema);
      } catch {
        errors.parameterSchema = 'Must be valid JSON.';
      }
    }

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  // ── Field helpers ─────────────────────────────────────────────────────────

  const handleTextField =
    (field: keyof FormValues) => (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
      setForm((prev) => ({ ...prev, [field]: e.target.value }));
      if (formErrors[field]) setFormErrors((prev) => ({ ...prev, [field]: undefined }));
    };

  const handleLanguageChange = (e: SelectChangeEvent<number>) => {
    const lang = e.target.value as ScriptLanguage;
    setForm((prev) => ({
      ...prev,
      language: lang,
      // Replace placeholder code when switching language and code is still a placeholder
      code:
        prev.code === '' || Object.values(CODE_PLACEHOLDERS).includes(prev.code.trim())
          ? CODE_PLACEHOLDERS[lang] ?? prev.code
          : prev.code,
    }));
  };

  // ── Save ──────────────────────────────────────────────────────────────────

  const handleSave = async () => {
    if (!validate()) return;

    setSaving(true);
    try {
      if (isEditMode && pluginId !== null) {
        await updateScriptPlugin(pluginId, {
          name: form.name.trim(),
          description: form.description.trim() || undefined,
          code: form.code,
          parameterSchema: form.parameterSchema.trim() || undefined,
          returnValueDescription: form.returnValueDescription.trim() || undefined,
          isActive: form.isActive,
        });
        setSnackbar({ open: true, message: 'Plugin updated successfully.', severity: 'success' });
      } else {
        const created = await createScriptPlugin({
          name: form.name.trim(),
          description: form.description.trim() || undefined,
          language: form.language,
          code: form.code,
          parameterSchema: form.parameterSchema.trim() || undefined,
          returnValueDescription: form.returnValueDescription.trim() || undefined,
        });
        setSnackbar({ open: true, message: 'Plugin created successfully.', severity: 'success' });
        // Navigate to edit mode so the user can test the new plugin
        setTimeout(() => navigate(`/scripting/plugins/${created.id}/edit`, { replace: true }), 800);
      }
    } catch (err) {
      logger.error('ScriptPluginEditorPage: save failed', err);
      setSnackbar({ open: true, message: 'Failed to save plugin. Please try again.', severity: 'error' });
    } finally {
      setSaving(false);
    }
  };

  // ── Test Run ─────────────────────────────────────────────────────────────

  const handleTestRun = async () => {
    if (pluginId === null) return;
    setTestRunning(true);
    setTestResult(null);
    setTestError(null);

    let parsedVars: Record<string, unknown> = {};
    let parsedCtx: Record<string, unknown> = {};

    try {
      parsedVars = JSON.parse(testVariables || '{}');
    } catch {
      setTestError('Variables field must be valid JSON.');
      setTestRunning(false);
      return;
    }
    try {
      parsedCtx = JSON.parse(testContext || '{}');
    } catch {
      setTestError('Context field must be valid JSON.');
      setTestRunning(false);
      return;
    }

    try {
      const result = await testScriptPlugin(pluginId, {
        variables: parsedVars,
        context: parsedCtx,
        timeout: 30,
      });
      setTestResult(result);
    } catch (err) {
      logger.error('ScriptPluginEditorPage: test run failed', err);
      setTestError('Test execution request failed. Check the API logs.');
    } finally {
      setTestRunning(false);
    }
  };

  // ── Guard: loading ────────────────────────────────────────────────────────

  if (loadingData) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="60vh">
        <CircularProgress />
      </Box>
    );
  }

  if (loadError) {
    return (
      <Box p={3}>
        <Alert severity="error" sx={{ mb: 2 }}>
          {loadError}
        </Alert>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/scripting/plugins')}>
          Back to Library
        </Button>
      </Box>
    );
  }

  const langInfo = LANGUAGE_LABELS[form.language] ?? { label: 'Unknown', color: 'default' as const };

  // ── Render ────────────────────────────────────────────────────────────────

  return (
    <Box sx={{ p: 3, maxWidth: 960, mx: 'auto' }}>
      {/* Header */}
      <Stack direction="row" alignItems="center" spacing={1} mb={0.5}>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/scripting/plugins')}
          size="small"
          sx={{ mr: 1 }}
        >
          Back
        </Button>
        <CodeIcon color="primary" />
        <Typography variant="h5" fontWeight={600}>
          {isEditMode ? `Edit: ${form.name || '…'}` : 'New Script Plugin'}
        </Typography>
        {isEditMode && (
          <Chip
            label={langInfo.label}
            color={langInfo.color as 'primary' | 'success' | 'secondary'}
            size="small"
            variant="outlined"
          />
        )}
      </Stack>
      <Typography variant="body2" color="text.secondary" mb={3}>
        {isEditMode
          ? 'Modify the plugin definition and save.'
          : 'Define a reusable script that AI agents can call as a plugin.'}
      </Typography>

      {/* ── Core form ──────────────────────────────────────────────────── */}
      <Paper variant="outlined" sx={{ p: 3, mb: 3 }}>
        <Typography variant="subtitle1" fontWeight={600} mb={2}>
          Plugin Details
        </Typography>

        <Stack spacing={2.5}>
          {/* Name */}
          <TextField
            label="Plugin Name"
            required
            fullWidth
            value={form.name}
            onChange={handleTextField('name')}
            error={!!formErrors.name}
            helperText={formErrors.name ?? 'A unique, descriptive name for this plugin.'}
            disabled={saving}
            inputProps={{ maxLength: 100 }}
          />

          {/* Description */}
          <TextField
            label="Description"
            fullWidth
            multiline
            minRows={2}
            value={form.description}
            onChange={handleTextField('description')}
            helperText="Optional: briefly describe what this plugin does."
            disabled={saving}
          />

          {/* Language — hidden/read-only in edit mode since language cannot change after creation */}
          {!isEditMode && (
            <FormControl fullWidth disabled={saving}>
              <InputLabel id="language-label">Language</InputLabel>
              <Select<number>
                labelId="language-label"
                label="Language"
                value={form.language}
                onChange={handleLanguageChange}
              >
                {LANGUAGE_OPTIONS.map((opt) => (
                  <MenuItem key={opt.value} value={opt.value}>
                    {opt.label}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          )}

          {/* Code */}
          <TextField
            label="Code"
            required
            fullWidth
            multiline
            minRows={14}
            value={form.code}
            onChange={handleTextField('code')}
            error={!!formErrors.code}
            helperText={formErrors.code ?? 'The plugin source code.'}
            disabled={saving}
            placeholder={CODE_PLACEHOLDERS[form.language]}
            inputProps={{
              style: {
                fontFamily: '"Fira Code", "Cascadia Code", "Consolas", "Courier New", monospace',
                fontSize: 13,
                lineHeight: 1.6,
              },
            }}
            sx={{
              '& .MuiInputBase-root': {
                bgcolor: 'grey.50',
              },
            }}
          />

          {/* Return Value Description */}
          <TextField
            label="Return Value Description"
            fullWidth
            value={form.returnValueDescription}
            onChange={handleTextField('returnValueDescription')}
            helperText="Optional: describe the shape and meaning of the return value."
            disabled={saving}
          />
        </Stack>
      </Paper>

      {/* ── Additional Information accordion ──────────────────────────── */}
      <Accordion variant="outlined" sx={{ mb: 3 }}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography variant="subtitle2" fontWeight={600}>
            Additional Information
          </Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Stack spacing={2.5}>
            {/* Parameter Schema */}
            <TextField
              label="Parameter Schema"
              fullWidth
              multiline
              minRows={4}
              value={form.parameterSchema}
              onChange={handleTextField('parameterSchema')}
              error={!!formErrors.parameterSchema}
              helperText={
                formErrors.parameterSchema ??
                'Optional: JSON Schema describing the expected input variables.'
              }
              placeholder={'{\n  "type": "object",\n  "properties": {\n    "input": { "type": "string" }\n  }\n}'}
              disabled={saving}
              inputProps={{
                style: {
                  fontFamily: '"Fira Code", "Consolas", "Courier New", monospace',
                  fontSize: 13,
                },
              }}
            />

            {/* Active toggle — only in edit mode */}
            {isEditMode && (
              <>
                <Divider />
                <FormControlLabel
                  control={
                    <Switch
                      checked={form.isActive}
                      onChange={(e) =>
                        setForm((prev) => ({ ...prev, isActive: e.target.checked }))
                      }
                      disabled={saving}
                    />
                  }
                  label={
                    <Box>
                      <Typography variant="body2" fontWeight={500}>
                        Active
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        Inactive plugins are hidden from agent plugin lists.
                      </Typography>
                    </Box>
                  }
                />
              </>
            )}
          </Stack>
        </AccordionDetails>
      </Accordion>

      {/* ── Action buttons ─────────────────────────────────────────────── */}
      <Stack direction="row" spacing={2} mb={3}>
        <Button
          variant="contained"
          startIcon={saving ? <CircularProgress size={16} color="inherit" /> : <SaveIcon />}
          onClick={handleSave}
          disabled={saving}
        >
          {saving ? 'Saving…' : isEditMode ? 'Save Changes' : 'Create Plugin'}
        </Button>
        <Button
          variant="outlined"
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/scripting/plugins')}
          disabled={saving}
        >
          Cancel
        </Button>
      </Stack>

      {/* ── Test Run (edit mode only) ───────────────────────────────────── */}
      {isEditMode && (
        <Accordion variant="outlined">
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Stack direction="row" alignItems="center" spacing={1}>
              <PlayArrowIcon fontSize="small" color="action" />
              <Typography variant="subtitle2" fontWeight={600}>
                Test Run
              </Typography>
              <Typography variant="caption" color="text.secondary">
                Execute this plugin with custom input
              </Typography>
            </Stack>
          </AccordionSummary>
          <AccordionDetails>
            <Stack spacing={2.5}>
              <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
                {/* Variables */}
                <TextField
                  label="Variables (JSON)"
                  fullWidth
                  multiline
                  minRows={5}
                  value={testVariables}
                  onChange={(e) => {
                    setTestVariables(e.target.value);
                    setTestError(null);
                  }}
                  placeholder='{"input": "hello world"}'
                  disabled={testRunning}
                  inputProps={{
                    style: {
                      fontFamily: '"Fira Code", "Consolas", "Courier New", monospace',
                      fontSize: 13,
                    },
                  }}
                  helperText="Key-value pairs passed as 'variables' inside the script."
                />
                {/* Context */}
                <TextField
                  label="Context (JSON)"
                  fullWidth
                  multiline
                  minRows={5}
                  value={testContext}
                  onChange={(e) => {
                    setTestContext(e.target.value);
                    setTestError(null);
                  }}
                  placeholder='{"userId": 1}'
                  disabled={testRunning}
                  inputProps={{
                    style: {
                      fontFamily: '"Fira Code", "Consolas", "Courier New", monospace',
                      fontSize: 13,
                    },
                  }}
                  helperText="Optional execution context (e.g. user session info)."
                />
              </Stack>

              {/* Test error */}
              {testError && (
                <Alert severity="error" onClose={() => setTestError(null)}>
                  {testError}
                </Alert>
              )}

              {/* Run button */}
              <Box>
                <Button
                  variant="contained"
                  color="secondary"
                  startIcon={
                    testRunning ? (
                      <CircularProgress size={16} color="inherit" />
                    ) : (
                      <PlayArrowIcon />
                    )
                  }
                  onClick={handleTestRun}
                  disabled={testRunning}
                >
                  {testRunning ? 'Running…' : 'Run Test'}
                </Button>
              </Box>

              {/* Test result */}
              {testResult !== null && (
                <Paper
                  variant="outlined"
                  sx={{ p: 2, bgcolor: testResult.success ? 'success.50' : 'error.50' }}
                >
                  <Stack spacing={1.5}>
                    {/* Status badge */}
                    <Stack direction="row" alignItems="center" spacing={1}>
                      {testResult.success ? (
                        <CheckCircleIcon fontSize="small" color="success" />
                      ) : (
                        <ErrorIcon fontSize="small" color="error" />
                      )}
                      <Typography
                        variant="subtitle2"
                        color={testResult.success ? 'success.main' : 'error.main'}
                        fontWeight={600}
                      >
                        {testResult.success ? 'Success' : 'Failed'}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        ({testResult.executionTime})
                      </Typography>
                    </Stack>

                    {/* Error message */}
                    {testResult.errorMessage && (
                      <Alert severity="error" variant="outlined">
                        {testResult.errorMessage}
                      </Alert>
                    )}

                    {/* Return value */}
                    {testResult.success && (
                      <Box>
                        <Typography variant="caption" fontWeight={600} color="text.secondary">
                          RETURN VALUE
                        </Typography>
                        <Paper
                          variant="outlined"
                          sx={{
                            p: 1.5,
                            mt: 0.5,
                            bgcolor: 'background.paper',
                            fontFamily: '"Fira Code", "Consolas", "Courier New", monospace',
                            fontSize: 12,
                            whiteSpace: 'pre-wrap',
                            wordBreak: 'break-word',
                          }}
                        >
                          {JSON.stringify(testResult.returnValue, null, 2)}
                        </Paper>
                      </Box>
                    )}

                    {/* Logs */}
                    {testResult.logs.length > 0 && (
                      <Box>
                        <Typography variant="caption" fontWeight={600} color="text.secondary">
                          LOGS ({testResult.logs.length})
                        </Typography>
                        <Paper
                          variant="outlined"
                          sx={{
                            p: 1.5,
                            mt: 0.5,
                            bgcolor: 'grey.900',
                            maxHeight: 200,
                            overflowY: 'auto',
                          }}
                        >
                          {testResult.logs.map((line, idx) => (
                            <Typography
                              key={idx}
                              variant="caption"
                              component="div"
                              sx={{
                                fontFamily: '"Fira Code", "Consolas", "Courier New", monospace',
                                color: 'grey.200',
                                lineHeight: 1.7,
                                whiteSpace: 'pre-wrap',
                              }}
                            >
                              {line}
                            </Typography>
                          ))}
                        </Paper>
                      </Box>
                    )}
                  </Stack>
                </Paper>
              )}
            </Stack>
          </AccordionDetails>
        </Accordion>
      )}

      {/* Snackbar */}
      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert
          severity={snackbar.severity}
          onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))}
          variant="filled"
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default ScriptPluginEditorPage;
