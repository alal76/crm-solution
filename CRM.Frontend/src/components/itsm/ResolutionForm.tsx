/**
 * ResolutionForm - Form for resolving a service request
 * Captures resolution details, root cause, resolution type, and KB article creation flag
 */

import React, { useState, useCallback } from 'react';
import {
  Box,
  Button,
  TextField,
  MenuItem,
  FormControlLabel,
  Checkbox,
  Typography,
  CircularProgress,
  Paper,
} from '@mui/material';
import {
  CheckCircle as CheckCircleIcon,
  Cancel as CancelIcon,
} from '@mui/icons-material';

export interface ResolutionData {
  resolution: string;
  rootCause?: string;
  resolutionType: string;
  knowledgeArticleCreated: boolean;
}

export interface ResolutionFormProps {
  onSubmit: (data: ResolutionData) => void;
  onCancel: () => void;
  loading?: boolean;
}

const resolutionTypes = [
  { value: 'Fixed', label: 'Fixed' },
  { value: 'Workaround', label: 'Workaround' },
  { value: 'NotReproducible', label: 'Not Reproducible' },
  { value: 'Duplicate', label: 'Duplicate' },
  { value: 'WontFix', label: "Won't Fix" },
];

const ResolutionForm: React.FC<ResolutionFormProps> = ({
  onSubmit,
  onCancel,
  loading = false,
}) => {
  const [resolution, setResolution] = useState('');
  const [rootCause, setRootCause] = useState('');
  const [resolutionType, setResolutionType] = useState('Fixed');
  const [knowledgeArticleCreated, setKnowledgeArticleCreated] = useState(false);
  const [resolutionError, setResolutionError] = useState(false);

  const handleSubmit = useCallback(
    (e: React.FormEvent) => {
      e.preventDefault();

      if (!resolution.trim()) {
        setResolutionError(true);
        return;
      }

      setResolutionError(false);
      onSubmit({
        resolution: resolution.trim(),
        rootCause: rootCause.trim() || undefined,
        resolutionType,
        knowledgeArticleCreated,
      });
    },
    [resolution, rootCause, resolutionType, knowledgeArticleCreated, onSubmit]
  );

  return (
    <Paper elevation={0} sx={{ p: 2, border: '1px solid', borderColor: 'divider' }}>
      <Typography variant="h6" gutterBottom>
        Resolve Service Request
      </Typography>

      <Box component="form" onSubmit={handleSubmit} noValidate>
        <TextField
          label="Resolution *"
          multiline
          rows={4}
          fullWidth
          value={resolution}
          onChange={(e) => {
            setResolution(e.target.value);
            if (e.target.value.trim()) setResolutionError(false);
          }}
          error={resolutionError}
          helperText={resolutionError ? 'Resolution is required' : ''}
          disabled={loading}
          placeholder="Describe the resolution..."
          sx={{ mb: 2 }}
        />

        <TextField
          label="Root Cause"
          multiline
          rows={2}
          fullWidth
          value={rootCause}
          onChange={(e) => setRootCause(e.target.value)}
          disabled={loading}
          placeholder="Optional: Describe the root cause..."
          sx={{ mb: 2 }}
        />

        <TextField
          label="Resolution Type"
          select
          fullWidth
          value={resolutionType}
          onChange={(e) => setResolutionType(e.target.value)}
          disabled={loading}
          sx={{ mb: 2 }}
        >
          {resolutionTypes.map((option) => (
            <MenuItem key={option.value} value={option.value}>
              {option.label}
            </MenuItem>
          ))}
        </TextField>

        <FormControlLabel
          control={
            <Checkbox
              checked={knowledgeArticleCreated}
              onChange={(e) => setKnowledgeArticleCreated(e.target.checked)}
              disabled={loading}
            />
          }
          label="Create Knowledge Article from this resolution"
          sx={{ mb: 2, display: 'block' }}
        />

        <Box display="flex" justifyContent="flex-end" gap={1}>
          <Button
            variant="outlined"
            onClick={onCancel}
            disabled={loading}
            startIcon={<CancelIcon />}
          >
            Cancel
          </Button>
          <Button
            variant="contained"
            type="submit"
            disabled={loading}
            color="success"
            startIcon={
              loading ? <CircularProgress size={18} color="inherit" /> : <CheckCircleIcon />
            }
          >
            {loading ? 'Resolving...' : 'Resolve'}
          </Button>
        </Box>
      </Box>
    </Paper>
  );
};

export default ResolutionForm;
