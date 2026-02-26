/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * ScriptNodeEditor — inline code editor for script workflow nodes.
 * Uses a MUI TextField (multiline) as a Monaco placeholder.
 * Supports JavaScript, Python, and C# via the backend scripting engine.
 */

import React, { useState, useCallback } from 'react';
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Divider,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  SelectChangeEvent,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import {
  Code as CodeIcon,
  CheckCircleOutline as ValidateIcon,
  PlayArrow as RunIcon,
  Save as SaveIcon,
  Cancel as CancelIcon,
} from '@mui/icons-material';
import {
  validateScript,
  executeScript,
  ScriptDiagnostic,
  ScriptExecuteResult,
  ScriptLanguage,
} from '../../services/scriptPluginService';

// ---------------------------------------------------------------------------
// Constants
// ---------------------------------------------------------------------------

const LANGUAGE_OPTIONS: { value: ScriptLanguage; label: string; placeholder: string }[] = [
  {
    value: 0,
    label: 'JavaScript',
    placeholder:
      '// JavaScript — variables available via: variables.myVar\n// Return a value with: return { result: 42 };\n',
  },
  {
    value: 1,
    label: 'Python',
    placeholder:
      '# Python — variables available via: variables[\'myVar\']\n# Return a value with: result = {"value": 42}\n',
  },
  {
    value: 2,
    label: 'C#',
    placeholder:
      '// C# — variables available via: Variables["myVar"]\n// Return a value with: return new { result = 42 };\n',
  },
];

// ---------------------------------------------------------------------------
// Props
// ---------------------------------------------------------------------------

export interface ScriptNodeEditorProps {
  /** Initial source code shown in the editor */
  initialCode?: string;
  /** Initial language selection (0=JS, 1=Python, 2=C#) */
  initialLanguage?: ScriptLanguage;
  /** Variables passed to the script at runtime (used when clicking Run) */
  variables?: Record<string, unknown>;
  /** Called when the user clicks Save */
  onSave?: (code: string, language: ScriptLanguage) => void;
  /** Called when the user clicks Cancel */
  onCancel?: () => void;
  /** Prevent editing */
  readOnly?: boolean;
}

// ---------------------------------------------------------------------------
// Component
// ---------------------------------------------------------------------------

const ScriptNodeEditor: React.FC<ScriptNodeEditorProps> = ({
  initialCode = '',
  initialLanguage = 0,
  variables = {},
  onSave,
  onCancel,
  readOnly = false,
}) => {
  // Editor state
  const [code, setCode] = useState<string>(initialCode);
  const [language, setLanguage] = useState<ScriptLanguage>(initialLanguage);

  // Operation states
  const [validating, setValidating] = useState(false);
  const [running, setRunning] = useState(false);

  // Results
  const [diagnostics, setDiagnostics] = useState<ScriptDiagnostic[] | null>(null);
  const [isValid, setIsValid] = useState<boolean | null>(null);
  const [runResult, setRunResult] = useState<ScriptExecuteResult | null>(null);
  const [error, setError] = useState<string | null>(null);

  // Helpers
  const clearResults = useCallback(() => {
    setDiagnostics(null);
    setIsValid(null);
    setRunResult(null);
    setError(null);
  }, []);

  const selectedLanguage = LANGUAGE_OPTIONS.find((l) => l.value === language)!;

  // ---------------------------------------------------------------------------
  // Handlers
  // ---------------------------------------------------------------------------

  const handleLanguageChange = (event: SelectChangeEvent<number>) => {
    setLanguage(event.target.value as ScriptLanguage);
    clearResults();
  };

  const handleCodeChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setCode(event.target.value);
    clearResults();
  };

  const handleValidate = async () => {
    if (!code.trim()) return;
    setValidating(true);
    setError(null);
    setDiagnostics(null);
    setIsValid(null);
    try {
      const result = await validateScript({ language, code });
      setIsValid(result.isValid);
      setDiagnostics(result.diagnostics);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Validation request failed.';
      setError(msg);
    } finally {
      setValidating(false);
    }
  };

  const handleRun = async () => {
    if (!code.trim()) return;
    setRunning(true);
    setError(null);
    setRunResult(null);
    try {
      const result = await executeScript({ language, code, variables });
      setRunResult(result);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Execution request failed.';
      setError(msg);
    } finally {
      setRunning(false);
    }
  };

  const handleSave = () => {
    onSave?.(code, language);
  };

  // ---------------------------------------------------------------------------
  // Render helpers
  // ---------------------------------------------------------------------------

  const renderDiagnostics = () => {
    if (isValid === null) return null;

    if (isValid && (!diagnostics || diagnostics.length === 0)) {
      return (
        <Alert severity="success" sx={{ mt: 1 }}>
          Script is valid — no issues found.
        </Alert>
      );
    }

    return (
      <Box sx={{ mt: 1 }}>
        <Alert severity={isValid ? 'warning' : 'error'} sx={{ mb: 0.5 }}>
          {isValid ? 'Valid with warnings' : 'Validation failed'}
        </Alert>
        {diagnostics && diagnostics.length > 0 && (
          <Paper variant="outlined" sx={{ p: 1, maxHeight: 160, overflowY: 'auto' }}>
            {diagnostics.map((d, i) => (
              <Typography
                key={i}
                variant="caption"
                component="div"
                color={d.severity === 'error' ? 'error.main' : 'warning.main'}
                sx={{ fontFamily: 'monospace', lineHeight: 1.6 }}
              >
                [{d.severity.toUpperCase()}] Line {d.line}:{d.column} — {d.message}
              </Typography>
            ))}
          </Paper>
        )}
      </Box>
    );
  };

  const renderRunResult = () => {
    if (!runResult) return null;

    return (
      <Box sx={{ mt: 1 }}>
        <Alert severity={runResult.success ? 'success' : 'error'} sx={{ mb: 0.5 }}>
          {runResult.success
            ? `Executed in ${runResult.executionTimeMs} ms`
            : runResult.errorMessage ?? 'Execution failed'}
        </Alert>

        {/* Return value */}
        {runResult.success && runResult.returnValue !== undefined && (
          <Paper variant="outlined" sx={{ p: 1, mb: 0.5 }}>
            <Typography variant="caption" color="text.secondary" display="block" gutterBottom>
              Return value
            </Typography>
            <Box
              component="pre"
              sx={{
                m: 0,
                fontSize: '0.75rem',
                fontFamily: 'monospace',
                overflowX: 'auto',
                whiteSpace: 'pre-wrap',
                wordBreak: 'break-word',
              }}
            >
              {JSON.stringify(runResult.returnValue, null, 2)}
            </Box>
          </Paper>
        )}

        {/* Logs */}
        {runResult.logs && runResult.logs.length > 0 && (
          <Paper variant="outlined" sx={{ p: 1, maxHeight: 140, overflowY: 'auto' }}>
            <Typography variant="caption" color="text.secondary" display="block" gutterBottom>
              Console output
            </Typography>
            {runResult.logs.map((line, i) => (
              <Typography
                key={i}
                variant="caption"
                component="div"
                sx={{ fontFamily: 'monospace', lineHeight: 1.6 }}
              >
                {line}
              </Typography>
            ))}
          </Paper>
        )}
      </Box>
    );
  };

  // ---------------------------------------------------------------------------
  // Render
  // ---------------------------------------------------------------------------

  const isBusy = validating || running;

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
      {/* Header */}
      <Stack direction="row" alignItems="center" spacing={1}>
        <CodeIcon fontSize="small" color="primary" />
        <Typography variant="subtitle1" fontWeight={600}>
          Script Editor
        </Typography>
        {isBusy && <CircularProgress size={16} thickness={4} />}
      </Stack>

      <Divider />

      {/* Language selector */}
      <FormControl size="small" disabled={readOnly || isBusy} sx={{ width: 200 }}>
        <InputLabel id="script-language-label">Language</InputLabel>
        <Select
          labelId="script-language-label"
          value={language}
          label="Language"
          onChange={handleLanguageChange}
        >
          {LANGUAGE_OPTIONS.map((opt) => (
            <MenuItem key={opt.value} value={opt.value}>
              {opt.label}
            </MenuItem>
          ))}
        </Select>
      </FormControl>

      {/* Code editor (TextField multiline — Monaco placeholder) */}
      <TextField
        label={`${selectedLanguage.label} script`}
        placeholder={selectedLanguage.placeholder}
        multiline
        minRows={12}
        maxRows={28}
        fullWidth
        value={code}
        onChange={handleCodeChange}
        disabled={readOnly || isBusy}
        InputProps={{
          sx: {
            fontFamily: 'monospace',
            fontSize: '0.82rem',
            lineHeight: 1.6,
            alignItems: 'flex-start',
          },
        }}
        inputProps={{ spellCheck: false }}
        variant="outlined"
        size="small"
      />

      {/* Action buttons */}
      <Stack direction="row" spacing={1} flexWrap="wrap">
        <Button
          variant="outlined"
          size="small"
          startIcon={validating ? <CircularProgress size={14} /> : <ValidateIcon />}
          onClick={handleValidate}
          disabled={readOnly || isBusy || !code.trim()}
        >
          Validate
        </Button>

        <Button
          variant="outlined"
          size="small"
          color="secondary"
          startIcon={running ? <CircularProgress size={14} /> : <RunIcon />}
          onClick={handleRun}
          disabled={readOnly || isBusy || !code.trim()}
        >
          Run
        </Button>

        {!readOnly && (
          <>
            <Box sx={{ flex: 1 }} />
            <Button
              variant="contained"
              size="small"
              startIcon={<SaveIcon />}
              onClick={handleSave}
              disabled={isBusy}
            >
              Save
            </Button>
            {onCancel && (
              <Button
                variant="outlined"
                size="small"
                color="inherit"
                startIcon={<CancelIcon />}
                onClick={onCancel}
                disabled={isBusy}
              >
                Cancel
              </Button>
            )}
          </>
        )}
      </Stack>

      {/* Global error */}
      {error && (
        <Alert severity="error" onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Validation diagnostics */}
      {renderDiagnostics()}

      {/* Execution result */}
      {renderRunResult()}
    </Box>
  );
};

export default ScriptNodeEditor;
