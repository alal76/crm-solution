import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Alert,
  Box,
  Typography,
  Button,
  TextField,
  MenuItem,
  Paper,
  Grid,
} from '@mui/material';
import apiClient from '../../services/apiClient';
import { ImpactUrgencyMatrix } from '../../components/itsm';
import type { ImpactLevel, UrgencyLevel } from '../../components/itsm';
import { incidentFormSchema } from '../../validation/itsmFormSchemas';
import type { ValidationError } from 'yup';

export const IncidentFormPage: React.FC = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    shortDescription: '',
    description: '',
    callerId: 0,
    impact: 1,
    urgency: 1,
    categoryId: 0
  });
  const [submitting, setSubmitting] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);

  const validate = async (): Promise<boolean> => {
    try {
      await incidentFormSchema.validate(formData, { abortEarly: false });
      setErrors({});
      return true;
    } catch (err) {
      const validationError = err as ValidationError;
      const fieldErrors: Record<string, string> = {};
      validationError.inner.forEach((e) => {
        if (e.path) fieldErrors[e.path] = e.message;
      });
      setErrors(fieldErrors);
      return false;
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitError(null);
    const isValid = await validate();
    if (!isValid) return;

    setSubmitting(true);
    try {
      await apiClient.post('/incidents', formData);
      navigate('/incidents');
    } catch (error: any) {
      setSubmitError(error?.response?.data?.message || 'Failed to create incident');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Box sx={{ p: 3, maxWidth: 720, mx: 'auto' }}>
      <Typography variant="h4" component="h1" fontWeight="bold" sx={{ mb: 3 }}>
        Create Incident
      </Typography>

      <Paper sx={{ p: 3 }}>
        {submitError && <Alert severity="error" sx={{ mb: 2 }}>{submitError}</Alert>}
        <Box component="form" onSubmit={handleSubmit}>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
            <TextField
              label="Short Description"
              value={formData.shortDescription}
              onChange={(e) => setFormData({...formData, shortDescription: e.target.value})}
              required
              fullWidth
              placeholder="Briefly describe the issue"
              error={!!errors.shortDescription}
              helperText={errors.shortDescription}
            />

            <TextField
              label="Description"
              value={formData.description}
              onChange={(e) => setFormData({...formData, description: e.target.value})}
              multiline
              rows={4}
              fullWidth
              placeholder="Detailed description..."
              error={!!errors.description}
              helperText={errors.description}
            />

            <Grid container spacing={2}>
              <Grid item xs={6}>
                <TextField
                  select
                  label="Impact"
                  value={formData.impact}
                  onChange={(e) => setFormData({...formData, impact: Number.parseInt(e.target.value)})}
                  required
                  fullWidth
                  error={!!errors.impact}
                  helperText={errors.impact}
                >
                  <MenuItem value={1}>Low</MenuItem>
                  <MenuItem value={2}>Medium</MenuItem>
                  <MenuItem value={3}>High</MenuItem>
                </TextField>
              </Grid>
              <Grid item xs={6}>
                <TextField
                  select
                  label="Urgency"
                  value={formData.urgency}
                  onChange={(e) => setFormData({...formData, urgency: Number.parseInt(e.target.value)})}
                  required
                  fullWidth
                  error={!!errors.urgency}
                  helperText={errors.urgency}
                >
                  <MenuItem value={1}>Low</MenuItem>
                  <MenuItem value={2}>Medium</MenuItem>
                  <MenuItem value={3}>High</MenuItem>
                </TextField>
              </Grid>
            </Grid>

            {/* Impact/Urgency Priority Matrix */}
            <Box sx={{ mt: 1 }}>
              <ImpactUrgencyMatrix
                impact={formData.impact as ImpactLevel}
                urgency={formData.urgency as UrgencyLevel}
                onChange={(impact, urgency) => setFormData({...formData, impact, urgency})}
                showMatrix
              />
            </Box>

            <Box sx={{ display: 'flex', gap: 2, pt: 2 }}>
              <Button
                type="submit"
                variant="contained"
                disabled={submitting}
              >
                {submitting ? 'Creating...' : 'Create Incident'}
              </Button>
              <Button
                variant="outlined"
                onClick={() => navigate('/incidents')}
              >
                Cancel
              </Button>
            </Box>
          </Box>
        </Box>
      </Paper>
    </Box>
  );
};

export default IncidentFormPage;
