/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * ScriptNodeEditor — inline code editor for script workflow nodes.
 * Uses @monaco-editor/react for full IDE-quality code editing.
 * Supports JavaScript, Python, and C# via the backend scripting engine.
 */

import React, { useState, useCallback, useRef } from 'react';
import Editor, { OnMount } from '@monaco-editor/react';
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
  Skeleton,
  Stack,
  Typography,
  useTheme,
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

/** Monaco language id per ScriptLanguage enum value */
const MONACO_LANG: Record<ScriptLanguage, string> = {
  0: 'javascript',
  1: 'python',
  2: 'csharp',
};

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
      "# Python — variables available via: variables['myVar']\n# Return a value with: result = {'value': 42}\n",
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
  /** Editor height (default: '400px') */
  height?: string;
  /** Show toolbar with language selector and action buttons (default: true) */
  showToolbar?: boolean;
  /** Show the test/run result panel below (default: true) */
  showTestPanel?: boolean;
}

// ---------------------------------------------------------------------------
// Monaco editor options (built per readOnly flag)
// ---------------------------------------------------------------------------

const buildMonacoOptions = (ro: boolean) =>
  ({
    fontSize: 13,
    minimap: { enabled: false },
    scrollBeyondLastLine: false,
    wordWrap: 'on' as const,
    readOnly: ro,
    automaticLayout: true,
  }) as const;

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
  height = '400px',
  showToolbar = true,
  showTestPanel = true,
}) => {
  const muiTheme = useTheme();
  const monacoTheme = muiTheme.palette.mode === 'dark' ? 'vs-dark' : 'vs';

  // Editor state
  const [code, setCode] = useState<string>(initialCode);
  const [language, setLanguage] = useState<ScriptLanguage>(initialLanguage);
  const [editorLoaded, setEditorLoaded] = useState(false);

  // Keep a ref to the Monaco editor instance for future imperative use
  const editorRef = useRef<Parameters<OnMount>[0] | null>(null);

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

  const monacoOptions = buildMonacoOptions(readOnly);

  // ---------------------------------------------------------------------------
  // Handlers
  // ---------------------------------------------------------------------------

  const handleEditorMount: OnMount = (editor) => {
    editorRef.current = editor;
    setEditorLoaded(true);
  };

  const handleEditorChange = useCallback(
    (value: string | undefined) => {
      setCode(value ?? '');
      clearResults();
    },
    [clearResults],
  );

  const handleLanguageChange = (event: SelectChangeEvent<number>) => {
    setLanguage(event.target.value as ScriptLanguage);
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
            : (runResult.errorMessage ?? 'Execution failed')}
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
  const currentLangOption = LANGUAGE_OPTIONS.find((l) => l.value === language)!;

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

      {/* Toolbar */}
      {showToolbar && (
        <>
          {/* Language selector — hidden in readOnly mode */}
          {!readOnly && (
            <FormControl size="small" disabled={isBusy} sx={{ width: 200 }}>
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
          )}

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

            {!readOnly && (onSave || onCancel) && (
              <>
                <Box sx={{ flex: 1 }} />
                {onSave && (
                  <Button
                    variant="contained"
                    size="small"
                    startIcon={<SaveIcon />}
                    onClick={handleSave}
                    disabled={isBusy}
                  >
                    Save
                  </Button>
                )}
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
        </>
      )}

      {/* Monaco Editor with loading skeleton */}
      <Box sx={{ position: 'relative', border: 1, borderColor: 'divider', borderRadius: 1 }}>
        {/* Skeleton shown while Monaco bundle loads */}
        {!editorLoaded && (
          <Skeleton
            variant="rectangular"
            width="100%"
            height={height}
            animation="wave"
            sx={{ borderRadius: 1 }}
          />
        )}

        {/* Busy overlay */}
        {isBusy && editorLoaded && (
          <Box
            sx={{
              position: 'absolute',
              inset: 0,
              zIndex: 10,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              bgcolor: 'action.disabledBackground',
              borderRadius: 1,
            }}
          >
            <CircularProgress size={32} />
          </Box>
        )}

        <Editor
          height={height}
          language={MONACO_LANG[language]}
          value={code}
          theme={monacoTheme}
          options={monacoOptions}
          onChange={handleEditorChange}
          onMount={handleEditorMount}
          loading={null}
          defaultValue={currentLangOption.placeholder}
        />
      </Box>

      {/* Test / result panel */}
      {showTestPanel && (
        <>
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
        </>
      )}
    </Box>
  );
};

export default ScriptNodeEditor;
