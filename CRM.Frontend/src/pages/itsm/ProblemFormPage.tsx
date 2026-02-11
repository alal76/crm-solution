import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Button,
  TextField,
  MenuItem,
  Paper,
} from '@mui/material';
import apiClient from '../../services/apiClient';

const ProblemFormPage: React.FC = () => {
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState(false);
  const [formData, setFormData] = useState({
    shortDescription: '',
    description: '',
    priority: 2
  });

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setSubmitting(true);

    try {
      await apiClient.post('/problems', formData);
      navigate('/itsm/problems');
    } catch (error) {
      console.error('Failed to create problem', error);
      setSubmitting(false);
    }
  };

  return (
    <Box sx={{ p: 3, maxWidth: 720, mx: 'auto' }}>
      <Typography variant="h4" component="h1" fontWeight="bold" sx={{ mb: 3 }}>
        Create Problem
      </Typography>
      <Paper sx={{ p: 3 }}>
        <form onSubmit={handleSubmit}>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
            <TextField
              label="Short Description"
              required
              fullWidth
              value={formData.shortDescription}
              onChange={(e) => setFormData({ ...formData, shortDescription: e.target.value })}
            />
            <TextField
              label="Description"
              fullWidth
              multiline
              rows={4}
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            />
            <TextField
              label="Priority"
              select
              fullWidth
              value={formData.priority}
              onChange={(e) => setFormData({ ...formData, priority: Number(e.target.value) })}
            >
              <MenuItem value={1}>P1 - Critical</MenuItem>
              <MenuItem value={2}>P2 - High</MenuItem>
              <MenuItem value={3}>P3 - Medium</MenuItem>
              <MenuItem value={4}>P4 - Low</MenuItem>
            </TextField>
            <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2 }}>
              <Button
                variant="outlined"
                onClick={() => navigate('/itsm/problems')}
              >
                Cancel
              </Button>
              <Button
                type="submit"
                variant="contained"
                disabled={submitting}
              >
                {submitting ? 'Saving...' : 'Create'}
              </Button>
            </Box>
          </Box>
        </form>
      </Paper>
    </Box>
  );
};

export default ProblemFormPage;
