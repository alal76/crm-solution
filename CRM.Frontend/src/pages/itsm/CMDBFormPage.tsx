import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Box, Typography, Paper, TextField, MenuItem, Button } from '@mui/material';
import apiClient from '../../services/apiClient';

const CMDBFormPage: React.FC = () => {
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState(false);
  const [formData, setFormData] = useState({
    ciName: '',
    ciType: 1,
    ciSubtype: '',
    description: ''
  });

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setSubmitting(true);

    try {
      await apiClient.post('/api/cmdb', formData);
      navigate('/itsm/cmdb');
    } catch (error) {
      console.error('Failed to create configuration item', error);
      setSubmitting(false);
    }
  };

  return (
    <Box sx={{ p: 3, maxWidth: 720, mx: 'auto' }}>
      <Typography variant="h4" component="h1" fontWeight="bold" gutterBottom>
        Create Configuration Item
      </Typography>
      <Paper sx={{ p: 3 }}>
        <Box component="form" onSubmit={handleSubmit} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          <TextField
            label="Name"
            value={formData.ciName}
            onChange={(e) => setFormData({ ...formData, ciName: e.target.value })}
            required
            fullWidth
          />
          <TextField
            label="Type"
            select
            value={formData.ciType}
            onChange={(e) => setFormData({ ...formData, ciType: Number(e.target.value) })}
            fullWidth
          >
            <MenuItem value={1}>Hardware</MenuItem>
            <MenuItem value={2}>Software</MenuItem>
            <MenuItem value={3}>Service</MenuItem>
            <MenuItem value={4}>Network</MenuItem>
          </TextField>
          <TextField
            label="Subtype"
            value={formData.ciSubtype}
            onChange={(e) => setFormData({ ...formData, ciSubtype: e.target.value })}
            fullWidth
          />
          <TextField
            label="Description"
            value={formData.description}
            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            multiline
            rows={4}
            fullWidth
          />
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1 }}>
            <Button variant="outlined" onClick={() => navigate('/itsm/cmdb')}>
              Cancel
            </Button>
            <Button type="submit" variant="contained" disabled={submitting}>
              {submitting ? 'Saving...' : 'Create'}
            </Button>
          </Box>
        </Box>
      </Paper>
    </Box>
  );
};

export default CMDBFormPage;
