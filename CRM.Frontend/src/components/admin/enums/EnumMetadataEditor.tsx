/**
 * ENUM-FE-009: EnumMetadataEditor.tsx
 * A textarea with real-time JSON validation for editing enum metadata blobs.
 * Shows a green/red border and an error message based on JSON parse status.
 */
import React, { useCallback, useEffect, useState } from 'react';
import { Alert, Box, TextField, Typography } from '@mui/material';

export interface EnumMetadataEditorProps {
  value: string;
  onChange: (value: string) => void;
  label?: string;
  disabled?: boolean;
  helperText?: string;
}

const EnumMetadataEditor: React.FC<EnumMetadataEditorProps> = ({
  value,
  onChange,
  label = 'Metadata (JSON)',
  disabled = false,
  helperText = 'Optional JSON object for extended configuration (e.g. { "probability": 0.8 })',
}) => {
  const [jsonError, setJsonError] = useState<string | null>(null);

  const validate = useCallback((raw: string) => {
    if (!raw.trim()) { setJsonError(null); return; }
    try {
      JSON.parse(raw);
      setJsonError(null);
    } catch (e) {
      setJsonError((e as Error).message);
    }
  }, []);

  useEffect(() => { validate(value); }, [value, validate]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    onChange(e.target.value);
    validate(e.target.value);
  };

  const isValid = !jsonError;

  return (
    <Box>
      {label && (
        <Typography variant="caption" color="text.secondary" sx={{ mb: 0.5, display: 'block' }}>
          {label}
        </Typography>
      )}
      <TextField
        multiline
        rows={4}
        value={value}
        onChange={handleChange}
        fullWidth
        disabled={disabled}
        placeholder='{ "probability": 0.8, "slaHours": 24 }'
        error={!!jsonError}
        InputProps={{
          sx: {
            fontFamily: 'monospace',
            fontSize: '0.8rem',
            borderColor: value.trim() ? (isValid ? 'success.main' : 'error.main') : undefined,
          },
        }}
        sx={{
          '& .MuiOutlinedInput-root': {
            '& fieldset': {
              borderColor: value.trim()
                ? isValid
                  ? 'success.main'
                  : 'error.main'
                : undefined,
            },
          },
        }}
      />
      {jsonError ? (
        <Alert severity="error" sx={{ mt: 0.5, py: 0.25, fontSize: '0.75rem' }}>
          Invalid JSON: {jsonError}
        </Alert>
      ) : (
        <Typography variant="caption" color="text.secondary">
          {helperText}
        </Typography>
      )}
    </Box>
  );
};

export default EnumMetadataEditor;
