import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      await apiClient.post('/incidents', formData);
      navigate('/incidents');
    } catch (error) {
      console.error('Failed to create incident', error);
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
        <Box component="form" onSubmit={handleSubmit}>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
            <TextField
              label="Short Description"
              value={formData.shortDescription}
              onChange={(e) => setFormData({...formData, shortDescription: e.target.value})}
              required
              fullWidth
              placeholder="Briefly describe the issue"
            />

            <TextField
              label="Description"
              value={formData.description}
              onChange={(e) => setFormData({...formData, description: e.target.value})}
              multiline
              rows={4}
              fullWidth
              placeholder="Detailed description..."
            />

            <Grid container spacing={2}>
              <Grid item xs={6}>
                <TextField
                  select
                  label="Impact"
                  value={formData.impact}
                  onChange={(e) => setFormData({...formData, impact: parseInt(e.target.value)})}
                  required
                  fullWidth
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
                  onChange={(e) => setFormData({...formData, urgency: parseInt(e.target.value)})}
                  required
                  fullWidth
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
