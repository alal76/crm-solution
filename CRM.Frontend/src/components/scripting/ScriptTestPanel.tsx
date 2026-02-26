/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * ScriptTestPanel — embeddable run-and-inspect panel for script plugins.
 * Supports two modes:
 *   - pluginId set  → calls testScriptPlugin(pluginId, { variables, context, timeout })
 *   - pluginId unset → calls executeScript({ code, language, variables, timeout })
 *
 * Result type differences handled internally:
 *   ScriptPluginTestResult.executionTime  → string  (e.g. "12ms")
 *   ScriptExecuteResult.executionTimeMs   → number (ms)
 */

import React, { useState, useCallback } from 'react';
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Alert,
  Box,
  Button,
  CircularProgress,
  Chip,
  Divider,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  SelectChangeEvent,
  Typography,
  TextField,
} from '@mui/material';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import ClearIcon from '@mui/icons-material/Clear';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';

import {
  testScriptPlugin,
  executeScript,
  ScriptPluginTestResult,
  ScriptExecuteResult,
  ScriptLanguage,
} from '../../services/scriptPluginService';

// ---------------------------------------------------------------------------
// Props
// ---------------------------------------------------------------------------

export interface ScriptTestPanelProps {
  /** If set, calls testScriptPlugin(pluginId, …) */
  pluginId?: number;
  /** Used when pluginId is absent: raw code to execute ad-hoc */
  code?: string;
  /** Script language enum: 0=JS (default), 1=Python, 2=CSharp */
  language?: ScriptLanguage;
  /** Pre-populate the variables JSON editor */
  initialVariables?: Record<string, unknown>;
  /** Fired whenever the result changes (also null on clear) */
  onResultChange?: (result: ScriptPluginTestResult | null) => void;
  /** compact=true hides the Context accordion and uses inline layout */
  compact?: boolean;
}

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function tryParseJson(raw: string): { ok: true; value: Record<string, unknown> } | { ok: false } {
  try {
    const parsed = JSON.parse(raw);
    if (typeof parsed === 'object' && parsed !== null && !Array.isArray(parsed)) {
      return { ok: true, value: parsed as Record<string, unknown> };
    }
    return { ok: false };
  } catch {
    return { ok: false };
  }
}

/** Normalise a ScriptExecuteResult into ScriptPluginTestResult shape. */
function normaliseExecuteResult(r: ScriptExecuteResult): ScriptPluginTestResult {
  return {
    success: r.success,
    returnValue: r.returnValue,
    logs: r.logs,
    errorMessage: r.errorMessage,
    executionTime: `${r.executionTimeMs}ms`,
  };
}

const TIMEOUT_OPTIONS = [
  { label: '5 s', value: 5 },
  { label: '10 s', value: 10 },
  { label: '30 s', value: 30 },
  { label: '60 s', value: 60 },
] as const;

// ---------------------------------------------------------------------------
// Component
// ---------------------------------------------------------------------------

const ScriptTestPanel: React.FC<ScriptTestPanelProps> = ({
  pluginId,
  code,
  language = 0,
  initialVariables,
  onResultChange,
  compact = false,
}) => {
  const [variablesRaw, setVariablesRaw] = useState<string>(
    initialVariables ? JSON.stringify(initialVariables, null, 2) : '{}',
  );
  const [contextRaw, setContextRaw] = useState<string>('{}');
  const [timeout, setTimeout] = useState<number>(10);
  const [running, setRunning] = useState<boolean>(false);
  const [result, setResult] = useState<ScriptPluginTestResult | null>(null);
  const [variablesError, setVariablesError] = useState<string | null>(null);
  const [contextError, setContextError] = useState<string | null>(null);
  const [runError, setRunError] = useState<string | null>(null);

  const handleRun = useCallback(async () => {
    // --- validate JSON inputs ---
    const varsParsed = tryParseJson(variablesRaw);
    const ctxParsed = tryParseJson(contextRaw);

    if (!varsParsed.ok) {
      setVariablesError('Invalid JSON — must be a plain object, e.g. {"key": "value"}');
    } else {
      setVariablesError(null);
    }

    if (!ctxParsed.ok) {
      setContextError('Invalid JSON — must be a plain object, e.g. {"key": "value"}');
    } else {
      setContextError(null);
    }

    if (!varsParsed.ok || !ctxParsed.ok) return;

    setRunning(true);
    setRunError(null);

    try {
      let testResult: ScriptPluginTestResult;

      if (pluginId !== undefined) {
        testResult = await testScriptPlugin(pluginId, {
          variables: varsParsed.value,
          context: ctxParsed.value,
          timeout,
        });
      } else {
        if (!code) {
          setRunError('No code provided to execute.');
          setRunning(false);
          return;
        }
        const raw = await executeScript({
          language,
          code,
          variables: varsParsed.value,
          timeout,
        });
        testResult = normaliseExecuteResult(raw);
      }

      setResult(testResult);
      onResultChange?.(testResult);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Unexpected error running script.';
      setRunError(msg);
    } finally {
      setRunning(false);
    }
  }, [variablesRaw, contextRaw, timeout, pluginId, code, language, onResultChange]);

  const handleClear = useCallback(() => {
    setResult(null);
    setRunError(null);
    setVariablesError(null);
    setContextError(null);
    onResultChange?.(null);
  }, [onResultChange]);

  const handleTimeoutChange = useCallback((e: SelectChangeEvent<number>) => {
    setTimeout(Number(e.target.value));
  }, []);

  // ------------------------------------------------------------------
  // Render helpers
  // ------------------------------------------------------------------

  const variablesEditor = (
    <TextField
      label="Variables (JSON object)"
      multiline
      minRows={compact ? 3 : 4}
      maxRows={10}
      fullWidth
      value={variablesRaw}
      onChange={(e) => setVariablesRaw(e.target.value)}
      error={!!variablesError}
      helperText={variablesError ?? 'Pass variables accessible inside the script'}
      inputProps={{ style: { fontFamily: 'monospace', fontSize: 13 } }}
      size="small"
    />
  );

  const contextAccordion = !compact && (
    <Accordion disableGutters elevation={0} sx={{ border: '1px solid', borderColor: 'divider', mt: 1 }}>
      <AccordionSummary expandIcon={<ExpandMoreIcon />}>
        <Typography variant="body2" fontWeight={500}>
          Context Override
        </Typography>
      </AccordionSummary>
      <AccordionDetails sx={{ pt: 0 }}>
        <TextField
          label="Context (JSON object)"
          multiline
          minRows={3}
          maxRows={8}
          fullWidth
          value={contextRaw}
          onChange={(e) => setContextRaw(e.target.value)}
          error={!!contextError}
          helperText={contextError ?? 'Override CRM context fields passed to the script'}
          inputProps={{ style: { fontFamily: 'monospace', fontSize: 13 } }}
          size="small"
        />
      </AccordionDetails>
    </Accordion>
  );

  const timeoutSelector = (
    <FormControl size="small" sx={{ minWidth: 100 }}>
      <InputLabel>Timeout</InputLabel>
      <Select<number> value={timeout} label="Timeout" onChange={handleTimeoutChange}>
        {TIMEOUT_OPTIONS.map((opt) => (
          <MenuItem key={opt.value} value={opt.value}>
            {opt.label}
          </MenuItem>
        ))}
      </Select>
    </FormControl>
  );

  const actionRow = (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 1.5, flexWrap: 'wrap' }}>
      {timeoutSelector}
      <Box sx={{ flex: 1 }} />
      {result !== null && (
        <Button
          size="small"
          variant="outlined"
          color="inherit"
          startIcon={<ClearIcon />}
          onClick={handleClear}
          disabled={running}
        >
          Clear
        </Button>
      )}
      <Button
        variant="contained"
        color="primary"
        startIcon={running ? <CircularProgress size={16} color="inherit" /> : <PlayArrowIcon />}
        onClick={handleRun}
        disabled={running}
        size="small"
      >
        {running ? 'Running…' : 'Run Test'}
      </Button>
    </Box>
  );

  // Result panel
  const resultPanel = result !== null && (
    <Box sx={{ mt: 2 }}>
      <Divider sx={{ mb: 1.5 }} />

      {/* Status header */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
        <Chip
          label={result.success ? '✓ Success' : '✗ Failed'}
          color={result.success ? 'success' : 'error'}
          size="small"
          sx={{ fontWeight: 700 }}
        />
        <Typography variant="caption" color="text.secondary">
          {result.executionTime}
        </Typography>
      </Box>

      {/* Return value */}
      <Typography variant="overline" color="text.secondary" display="block" sx={{ mb: 0.5 }}>
        Return Value
      </Typography>
      <Paper
        variant="outlined"
        sx={{
          p: 1,
          mb: 1,
          fontFamily: 'monospace',
          fontSize: 12,
          whiteSpace: 'pre-wrap',
          wordBreak: 'break-all',
          bgcolor: 'action.hover',
          maxHeight: 200,
          overflowY: 'auto',
        }}
      >
        {JSON.stringify(result.returnValue, null, 2)}
      </Paper>

      {/* Logs */}
      {result.logs.length > 0 && (
        <>
          <Typography variant="overline" color="text.secondary" display="block" sx={{ mb: 0.5 }}>
            Console Logs ({result.logs.length})
          </Typography>
          <Paper
            variant="outlined"
            sx={{
              p: 1,
              mb: 1,
              maxHeight: 120,
              overflowY: 'auto',
              bgcolor: 'grey.900',
              color: 'grey.100',
            }}
          >
            {result.logs.map((line, idx) => (
              <Typography
                key={idx}
                component="div"
                sx={{ fontFamily: 'monospace', fontSize: 12, lineHeight: 1.6 }}
              >
                {line}
              </Typography>
            ))}
          </Paper>
        </>
      )}

      {/* Error */}
      {result.errorMessage && (
        <Alert severity="error" sx={{ mt: 1 }}>
          {result.errorMessage}
        </Alert>
      )}
    </Box>
  );

  // Transport-level error (network / unexpected)
  const runErrorAlert = runError && (
    <Alert severity="error" sx={{ mt: 1 }}>
      {runError}
    </Alert>
  );

  // ------------------------------------------------------------------
  // Layout variants
  // ------------------------------------------------------------------

  if (compact) {
    return (
      <Box>
        {variablesEditor}
        {actionRow}
        {runErrorAlert}
        {resultPanel}
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="subtitle2" fontWeight={600} gutterBottom>
        Test Script
      </Typography>
      {variablesEditor}
      {contextAccordion}
      {actionRow}
      {runErrorAlert}
      {resultPanel}
    </Box>
  );
};

export default ScriptTestPanel;
