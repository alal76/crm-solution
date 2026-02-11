import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import TextField from '@mui/material/TextField';
import MenuItem from '@mui/material/MenuItem';
import Grid from '@mui/material/Grid';
import Button from '@mui/material/Button';
import FormControlLabel from '@mui/material/FormControlLabel';
import Checkbox from '@mui/material/Checkbox';
import apiClient from '../../services/apiClient';

const SLAPolicyFormPage: React.FC = () => {
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState(false);
  const [formData, setFormData] = useState({
    name: '',
    targetType: 1,
    p1ResponseMinutes: 15,
    p1ResolutionMinutes: 240,
    useBusinessHours: true,
    isActive: true
  });

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setSubmitting(true);

    try {
      await apiClient.post('/api/sla/policies', formData);
      navigate('/itsm/sla/policies');
    } catch (error) {
      console.error('Failed to create SLA policy', error);
      setSubmitting(false);
    }
  };

  return (
    <Box sx={{ p: 3, maxWidth: 700, mx: 'auto' }}>
      <Typography variant="h4" component="h1" fontWeight="bold" sx={{ mb: 3 }}>Create SLA Policy</Typography>
      <Paper component="form" onSubmit={handleSubmit} sx={{ p: 3, display: 'flex', flexDirection: 'column', gap: 2 }}>
        <TextField
          label="Name"
          value={formData.name}
          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
          fullWidth
          required
        />
        <TextField
          label="Target Type"
          select
          value={formData.targetType}
          onChange={(e) => setFormData({ ...formData, targetType: Number(e.target.value) })}
          fullWidth
        >
          <MenuItem value={1}>Incident</MenuItem>
          <MenuItem value={2}>Service Request</MenuItem>
          <MenuItem value={3}>Change</MenuItem>
        </TextField>
        <Grid container spacing={2}>
          <Grid item xs={12} md={6}>
            <TextField
              label="P1 Response (minutes)"
              type="number"
              value={formData.p1ResponseMinutes}
              onChange={(e) => setFormData({ ...formData, p1ResponseMinutes: Number(e.target.value) })}
              fullWidth
              inputProps={{ min: 1 }}
            />
          </Grid>
          <Grid item xs={12} md={6}>
            <TextField
              label="P1 Resolution (minutes)"
              type="number"
              value={formData.p1ResolutionMinutes}
              onChange={(e) => setFormData({ ...formData, p1ResolutionMinutes: Number(e.target.value) })}
              fullWidth
              inputProps={{ min: 1 }}
            />
          </Grid>
        </Grid>
        <FormControlLabel
          control={<Checkbox checked={formData.useBusinessHours} onChange={(e) => setFormData({ ...formData, useBusinessHours: e.target.checked })} />}
          label="Use business hours"
        />
        <FormControlLabel
          control={<Checkbox checked={formData.isActive} onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })} />}
          label="Active"
        />
        <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1 }}>
          <Button variant="outlined" onClick={() => navigate('/itsm/sla/policies')}>Cancel</Button>
          <Button type="submit" variant="contained" disabled={submitting}>
            {submitting ? 'Saving...' : 'Create'}
          </Button>
        </Box>
      </Paper>
    </Box>
  );
};

export default SLAPolicyFormPage;
