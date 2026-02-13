/**
 * CRM Solution - Workflow Templates Page
 */

import React, { useCallback, useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Alert,
  List,
  ListItem,
  ListItemText,
  Chip,
} from '@mui/material';
import { Add as AddIcon } from '@mui/icons-material';
import { workflowService, type WorkflowDefinition } from '../../services/workflowService';

const slugify = (value: string) => value
  .toLowerCase()
  .replace(/[^a-z0-9]+/g, '-')
  .replace(/(^-|-$)/g, '');

const WorkflowTemplatesPage: React.FC = () => {
  const [templates, setTemplates] = useState<WorkflowDefinition[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [name, setName] = useState('');
  const [entityType, setEntityType] = useState('');

  const loadTemplates = useCallback(async () => {
    try {
      setLoading(true);
      const workflows = await workflowService.getWorkflows();
      const filtered = workflows.filter(w =>
        w.category?.toLowerCase() === 'template' ||
        (w.tags || []).some(tag => tag.toLowerCase() === 'template')
      );
      setTemplates(filtered);
      setError('');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load workflow templates');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadTemplates();
  }, [loadTemplates]);

  const handleCreate = async () => {
    try {
      setLoading(true);
      const workflowKey = `template-${slugify(name) || Date.now()}`;
      await workflowService.createWorkflow({
        workflowKey,
        name,
        category: 'Template',
        entityType,
        tags: ['template'],
      });
      setDialogOpen(false);
      setName('');
      setEntityType('');
      loadTemplates();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create template');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box sx={{ p: 3, display: 'grid', gap: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="h5" fontWeight="bold">Workflow Templates</Typography>
        <Button startIcon={<AddIcon />} variant="contained" onClick={() => setDialogOpen(true)}>
          New Template
        </Button>
      </Box>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper variant="outlined" sx={{ p: 2 }}>
        <List>
          {templates.map(template => (
            <ListItem key={template.id} divider>
              <ListItemText
                primary={template.name}
                secondary={`${template.entityType} • ${template.workflowKey}`}
              />
              <Chip label={template.status} size="small" />
            </ListItem>
          ))}
          {!templates.length && !loading && (
            <ListItem>
              <ListItemText primary="No workflow templates found." />
            </ListItem>
          )}
        </List>
      </Paper>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>Create Workflow Template</DialogTitle>
        <DialogContent sx={{ display: 'grid', gap: 2, mt: 1 }}>
          <TextField label="Template Name" value={name} onChange={(e) => setName(e.target.value)} />
          <TextField label="Entity Type" value={entityType} onChange={(e) => setEntityType(e.target.value)} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleCreate} disabled={!name || !entityType}>
            Create
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default WorkflowTemplatesPage;
