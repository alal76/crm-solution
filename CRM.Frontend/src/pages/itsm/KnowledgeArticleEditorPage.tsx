import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import TextField from '@mui/material/TextField';
import MenuItem from '@mui/material/MenuItem';
import Grid from '@mui/material/Grid';
import Button from '@mui/material/Button';
import apiClient from '../../services/apiClient';

const KnowledgeArticleEditorPage: React.FC = () => {
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState(false);
  const [formData, setFormData] = useState({
    title: '',
    shortDescription: '',
    articleBody: '',
    articleType: 1,
    isInternal: true
  });

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setSubmitting(true);

    try {
      await apiClient.post('/api/knowledge', formData);
      navigate('/itsm/knowledge');
    } catch (error) {
      console.error('Failed to create article', error);
      setSubmitting(false);
    }
  };

  return (
    <Box sx={{ p: 3, maxWidth: 900, mx: 'auto' }}>
      <Typography variant="h4" component="h1" fontWeight="bold" gutterBottom>Knowledge Article Editor</Typography>
      <Paper sx={{ p: 3 }} component="form" onSubmit={handleSubmit}>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          <TextField
            label="Title"
            value={formData.title}
            onChange={(e) => setFormData({ ...formData, title: e.target.value })}
            fullWidth
            required
          />
          <TextField
            label="Short Description"
            value={formData.shortDescription}
            onChange={(e) => setFormData({ ...formData, shortDescription: e.target.value })}
            fullWidth
          />
          <TextField
            label="Article Body"
            value={formData.articleBody}
            onChange={(e) => setFormData({ ...formData, articleBody: e.target.value })}
            fullWidth
            multiline
            rows={8}
            required
          />
          <Grid container spacing={2}>
            <Grid item xs={12} md={6}>
              <TextField
                label="Article Type"
                select
                value={formData.articleType}
                onChange={(e) => setFormData({ ...formData, articleType: Number(e.target.value) })}
                fullWidth
              >
                <MenuItem value={1}>How-To</MenuItem>
                <MenuItem value={2}>Troubleshooting</MenuItem>
                <MenuItem value={3}>FAQ</MenuItem>
                <MenuItem value={4}>Known Error</MenuItem>
                <MenuItem value={5}>Reference</MenuItem>
                <MenuItem value={6}>Best Practice</MenuItem>
              </TextField>
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                label="Visibility"
                select
                value={formData.isInternal ? 'internal' : 'external'}
                onChange={(e) => setFormData({ ...formData, isInternal: e.target.value === 'internal' })}
                fullWidth
              >
                <MenuItem value="internal">Internal</MenuItem>
                <MenuItem value="external">External</MenuItem>
              </TextField>
            </Grid>
          </Grid>
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1.5 }}>
            <Button variant="outlined" onClick={() => navigate('/itsm/knowledge')}>Cancel</Button>
            <Button variant="contained" type="submit" disabled={submitting}>{submitting ? 'Saving...' : 'Create'}</Button>
          </Box>
        </Box>
      </Paper>
    </Box>
  );
};

export default KnowledgeArticleEditorPage;
