import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import TextField from '@mui/material/TextField';
import MenuItem from '@mui/material/MenuItem';
import Grid from '@mui/material/Grid';
import Button from '@mui/material/Button';
import Alert from '@mui/material/Alert';
import apiClient from '../../services/apiClient';
import { changeFormSchema } from '../../validation/itsmFormSchemas';
import type { ValidationError } from 'yup';

const ChangeFormPage: React.FC = () => {
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [formData, setFormData] = useState({
    shortDescription: '',
    description: '',
    type: 2,
    risk: 2,
    impact: 2,
    plannedStartDate: '',
    plannedEndDate: '',
    implementationPlan: '',
    backoutPlan: ''
  });

  const validate = async (): Promise<boolean> => {
    try {
      await changeFormSchema.validate(formData, { abortEarly: false });
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

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setSubmitError(null);
    const isValid = await validate();
    if (!isValid) return;

    setSubmitting(true);
    try {
      await apiClient.post('/changes', formData);
      navigate('/itsm/changes');
    } catch (error: any) {
      setSubmitError(error?.response?.data?.message || 'Failed to create change');
      setSubmitting(false);
    }
  };

  return (
    <Box sx={{ p: 3, maxWidth: 900, mx: 'auto' }}>
      <Typography variant="h4" component="h1" fontWeight="bold" sx={{ mb: 3 }}>Create Change</Typography>
      <Paper sx={{ p: 3 }}>
        {submitError && <Alert severity="error" sx={{ mb: 2 }}>{submitError}</Alert>}
        <form onSubmit={handleSubmit}>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <TextField
              fullWidth
              label="Short Description"
              value={formData.shortDescription}
              onChange={(e) => setFormData({ ...formData, shortDescription: e.target.value })}
              required
              error={!!errors.shortDescription}
              helperText={errors.shortDescription}
            />
            <TextField
              fullWidth
              label="Description"
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              multiline
              rows={4}
              error={!!errors.description}
              helperText={errors.description}
            />
            <Grid container spacing={2}>
              <Grid item xs={12} md={4}>
                <TextField
                  fullWidth
                  select
                  label="Type"
                  value={formData.type}
                  onChange={(e) => setFormData({ ...formData, type: Number(e.target.value) })}
                >
                  <MenuItem value={1}>Standard</MenuItem>
                  <MenuItem value={2}>Normal</MenuItem>
                  <MenuItem value={3}>Emergency</MenuItem>
                </TextField>
              </Grid>
              <Grid item xs={12} md={4}>
                <TextField
                  fullWidth
                  select
                  label="Risk"
                  value={formData.risk}
                  onChange={(e) => setFormData({ ...formData, risk: Number(e.target.value) })}
                >
                  <MenuItem value={1}>High</MenuItem>
                  <MenuItem value={2}>Medium</MenuItem>
                  <MenuItem value={3}>Low</MenuItem>
                </TextField>
              </Grid>
              <Grid item xs={12} md={4}>
                <TextField
                  fullWidth
                  select
                  label="Impact"
                  value={formData.impact}
                  onChange={(e) => setFormData({ ...formData, impact: Number(e.target.value) })}
                >
                  <MenuItem value={1}>High</MenuItem>
                  <MenuItem value={2}>Medium</MenuItem>
                  <MenuItem value={3}>Low</MenuItem>
                </TextField>
              </Grid>
            </Grid>
            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Planned Start"
                  type="datetime-local"
                  value={formData.plannedStartDate}
                  onChange={(e) => setFormData({ ...formData, plannedStartDate: e.target.value })}
                  InputLabelProps={{ shrink: true }}
                  error={!!errors.plannedStartDate}
                  helperText={errors.plannedStartDate}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Planned End"
                  type="datetime-local"
                  value={formData.plannedEndDate}
                  onChange={(e) => setFormData({ ...formData, plannedEndDate: e.target.value })}
                  InputLabelProps={{ shrink: true }}
                  error={!!errors.plannedEndDate}
                  helperText={errors.plannedEndDate}
                />
              </Grid>
            </Grid>
            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Implementation Plan"
                  value={formData.implementationPlan}
                  onChange={(e) => setFormData({ ...formData, implementationPlan: e.target.value })}
                  multiline
                  rows={3}
                  error={!!errors.implementationPlan}
                  helperText={errors.implementationPlan}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Backout Plan"
                  value={formData.backoutPlan}
                  onChange={(e) => setFormData({ ...formData, backoutPlan: e.target.value })}
                  multiline
                  rows={3}
                  error={!!errors.backoutPlan}
                  helperText={errors.backoutPlan}
                />
              </Grid>
            </Grid>
            <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1, mt: 1 }}>
              <Button variant="outlined" onClick={() => navigate('/itsm/changes')}>
                Cancel
              </Button>
              <Button variant="contained" type="submit" disabled={submitting}>
                {submitting ? 'Saving...' : 'Create'}
              </Button>
            </Box>
          </Box>
        </form>
      </Paper>
    </Box>
  );
};

export default ChangeFormPage;
