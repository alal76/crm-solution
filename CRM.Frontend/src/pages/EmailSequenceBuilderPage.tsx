/**
 * EmailSequenceBuilderPage
 * Visual sequence builder with drag-and-drop support for email sequences
 * Priority: P1
 */

import React, { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  CircularProgress,
  Alert,
  Chip,
  Grid,
  Paper,
  Stack,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  IconButton,
  Menu,
  Divider
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import MoreVertIcon from '@mui/icons-material/MoreVert';
import ArrowDownwardIcon from '@mui/icons-material/ArrowDownward';
import marketingService from '../services/marketingService';
import {
  EmailSequence,
  SequenceStep,
  SequenceStepType,
  CreateEmailSequenceDto
} from '../types/marketing';

/**
 * Email Sequence Builder Page Component
 */
export const EmailSequenceBuilderPage: React.FC = () => {
  const [sequences, setSequences] = useState<EmailSequence[]>([]);
  const [selectedSequence, setSelectedSequence] = useState<EmailSequence | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [newStepType, setNewStepType] = useState<SequenceStepType>(SequenceStepType.Email);
  const [selectedStep, setSelectedStep] = useState<SequenceStep | null>(null);

  useEffect(() => {
    loadSequences();
  }, []);

  const loadSequences = async () => {
    try {
      setLoading(true);
      const response = await marketingService.getEmailSequences();
      setSequences(response.data.items);
    } catch (err) {
      setError('Failed to load email sequences');
      console.error('Error loading sequences:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleAddStep = (type: SequenceStepType) => {
    if (!selectedSequence) return;

    const newStep: SequenceStep = {
      sequence: (selectedSequence.steps?.length || 0) + 1,
      type
    };

    setSelectedSequence({
      ...selectedSequence,
      steps: [...(selectedSequence.steps || []), newStep]
    });
    setNewStepType(SequenceStepType.Email);
  };

  const handleDeleteStep = (index: number) => {
    if (!selectedSequence) return;

    setSelectedSequence({
      ...selectedSequence,
      steps: selectedSequence.steps?.filter((_, i) => i !== index) || []
    });
  };

  const handleSaveSequence = async () => {
    if (!selectedSequence) return;

    try {
      setLoading(true);
      if (selectedSequence.id) {
        await marketingService.updateEmailSequence(selectedSequence.id, {
          steps: selectedSequence.steps
        });
      }
      loadSequences();
      setSelectedSequence(null);
    } catch (err) {
      setError('Failed to save sequence');
      console.error('Error saving sequence:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {/* Header */}
      <Box sx={{ mb: 4, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="h4">Email Sequence Builder</Typography>
        <Button
          variant="contained"
          onClick={() => setCreateDialogOpen(true)}
        >
          New Sequence
        </Button>
      </Box>

      <Grid container spacing={3}>
        {/* Sequences List */}
        <Grid item xs={12} md={3}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>Sequences</Typography>
              {loading ? (
                <CircularProgress />
              ) : (
                <Stack spacing={1}>
                  {sequences.map((seq) => (
                    <Paper
                      key={seq.id}
                      sx={{
                        p: 2,
                        cursor: 'pointer',
                        bgcolor: selectedSequence?.id === seq.id ? 'primary.light' : 'background.default',
                        '&:hover': { bgcolor: 'action.hover' }
                      }}
                      onClick={() => setSelectedSequence(seq)}
                    >
                      <Typography variant="body2" fontWeight="bold">{seq.name}</Typography>
                      <Chip
                        size="small"
                        label={seq.status}
                        sx={{ mt: 1 }}
                        color={seq.status === 'active' ? 'success' : 'default'}
                      />
                    </Paper>
                  ))}
                </Stack>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Sequence Builder */}
        <Grid item xs={12} md={9}>
          {selectedSequence ? (
            <SequenceBuilder
              sequence={selectedSequence}
              onSequenceChange={setSelectedSequence}
              onSave={handleSaveSequence}
              onAddStep={handleAddStep}
              onDeleteStep={handleDeleteStep}
              onClose={() => setSelectedSequence(null)}
            />
          ) : (
            <Card>
              <CardContent sx={{ textAlign: 'center', py: 6 }}>
                <Typography color="textSecondary">
                  Select or create a sequence to get started
                </Typography>
              </CardContent>
            </Card>
          )}
        </Grid>
      </Grid>

      {/* Create Sequence Dialog */}
      <CreateSequenceDialog
        open={createDialogOpen}
        onClose={() => setCreateDialogOpen(false)}
        onSuccess={() => {
          setCreateDialogOpen(false);
          loadSequences();
        }}
      />
    </Container>
  );
};

/**
 * Sequence Builder Component
 */
const SequenceBuilder: React.FC<{
  sequence: EmailSequence;
  onSequenceChange: (seq: EmailSequence) => void;
  onSave: () => Promise<void>;
  onAddStep: (type: SequenceStepType) => void;
  onDeleteStep: (index: number) => void;
  onClose: () => void;
}> = ({
  sequence,
  onSequenceChange,
  onSave,
  onAddStep,
  onDeleteStep,
  onClose
}) => {
  const [saving, setSaving] = useState(false);
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  const handleMenuOpen = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleAddClick = (type: SequenceStepType) => {
    onAddStep(type);
    handleMenuClose();
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      await onSave();
    } finally {
      setSaving(false);
    }
  };

  return (
    <Card>
      <CardContent>
        {/* Header */}
        <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Box>
            <Typography variant="h6">{sequence.name}</Typography>
            <Typography variant="body2" color="textSecondary">{sequence.description}</Typography>
          </Box>
          <Box>
            <Button
              size="small"
              variant="outlined"
              onClick={handleMenuOpen}
              startIcon={<AddIcon />}
            >
              Add Step
            </Button>
            <Menu
              anchorEl={anchorEl}
              open={Boolean(anchorEl)}
              onClose={handleMenuClose}
            >
              <MenuItem onClick={() => handleAddClick(SequenceStepType.Email)}>
                Email
              </MenuItem>
              <MenuItem onClick={() => handleAddClick(SequenceStepType.Delay)}>
                Delay
              </MenuItem>
              <MenuItem onClick={() => handleAddClick(SequenceStepType.Condition)}>
                Condition
              </MenuItem>
            </Menu>
          </Box>
        </Box>

        <Divider sx={{ mb: 3 }} />

        {/* Steps */}
        <Stack spacing={2}>
          {sequence.steps?.map((step, index) => (
            <Box key={index}>
              <SequenceStepCard
                step={step}
                onStepChange={(updated) => {
                  const newSteps = [...sequence.steps!];
                  newSteps[index] = updated;
                  onSequenceChange({...sequence, steps: newSteps});
                }}
                onDelete={() => onDeleteStep(index)}
              />
              {index < (sequence.steps?.length || 0) - 1 && (
                <Box sx={{ display: 'flex', justifyContent: 'center', py: 1 }}>
                  <ArrowDownwardIcon />
                </Box>
              )}
            </Box>
          ))}
        </Stack>

        {(!sequence.steps || sequence.steps.length === 0) && (
          <Typography color="textSecondary" sx={{ textAlign: 'center', py: 4 }}>
            No steps yet. Add a step to get started.
          </Typography>
        )}

        {/* Actions */}
        <Divider sx={{ my: 3 }} />
        <Stack direction="row" spacing={2} sx={{ justifyContent: 'flex-end' }}>
          <Button onClick={onClose}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleSave}
            disabled={saving}
          >
            {saving ? 'Saving...' : 'Save Sequence'}
          </Button>
        </Stack>
      </CardContent>
    </Card>
  );
};

/**
 * Sequence Step Card Component
 */
const SequenceStepCard: React.FC<{
  step: SequenceStep;
  onStepChange: (step: SequenceStep) => void;
  onDelete: () => void;
}> = ({ step, onStepChange, onDelete }) => {
  const [editDialogOpen, setEditDialogOpen] = useState(false);

  const getStepLabel = () => {
    switch (step.type) {
      case SequenceStepType.Email:
        return `Email: ${step.emailTemplateId || 'Select template'}`;
      case SequenceStepType.Delay:
        return `Delay: ${step.delayDays || 0} days, ${step.delayHours || 0} hours`;
      case SequenceStepType.Condition:
        return `Condition: ${step.condition?.field || 'Select field'}`;
      default:
        return step.name || 'Unknown Step';
    }
  };

  return (
    <>
      <Paper
        sx={{
          p: 2,
          bgcolor: 'background.default',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center'
        }}
      >
        <Box>
          <Chip label={`Step ${step.sequence}`} size="small" sx={{ mr: 1 }} />
          <Typography variant="body2">{getStepLabel()}</Typography>
        </Box>
        <Stack direction="row" spacing={1}>
          <IconButton size="small" onClick={() => setEditDialogOpen(true)}>
            <EditIcon />
          </IconButton>
          <IconButton size="small" onClick={onDelete} color="error">
            <DeleteIcon />
          </IconButton>
        </Stack>
      </Paper>

      {/* Edit Dialog */}
      <StepEditDialog
        step={step}
        open={editDialogOpen}
        onClose={() => setEditDialogOpen(false)}
        onSave={(updated) => {
          onStepChange(updated);
          setEditDialogOpen(false);
        }}
      />
    </>
  );
};

/**
 * Step Edit Dialog Component
 */
const StepEditDialog: React.FC<{
  step: SequenceStep;
  open: boolean;
  onClose: () => void;
  onSave: (step: SequenceStep) => void;
}> = ({ step, open, onClose, onSave }) => {
  const [editedStep, setEditedStep] = useState(step);

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Edit Step</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 2 }}>
          {editedStep.type === SequenceStepType.Email && (
            <>
              <TextField
                fullWidth
                type="number"
                label="Template ID"
                value={editedStep.emailTemplateId || ''}
                onChange={(e) => setEditedStep({
                  ...editedStep,
                  emailTemplateId: parseInt(e.target.value) || undefined
                })}
              />
              <TextField
                fullWidth
                label="Template Name"
                value={editedStep.name || ''}
                onChange={(e) => setEditedStep({...editedStep, name: e.target.value})}
              />
            </>
          )}
          {editedStep.type === SequenceStepType.Delay && (
            <>
              <TextField
                fullWidth
                type="number"
                label="Days"
                value={editedStep.delayDays || 0}
                onChange={(e) => setEditedStep({
                  ...editedStep,
                  delayDays: parseInt(e.target.value) || 0
                })}
              />
              <TextField
                fullWidth
                type="number"
                label="Hours"
                value={editedStep.delayHours || 0}
                onChange={(e) => setEditedStep({
                  ...editedStep,
                  delayHours: parseInt(e.target.value) || 0
                })}
              />
            </>
          )}
          {editedStep.type === SequenceStepType.Condition && (
            <ConditionBuilder
              condition={editedStep.condition}
              onChange={(condition) => setEditedStep({...editedStep, condition})}
            />
          )}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          onClick={() => onSave(editedStep)}
          variant="contained"
        >
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
};

/**
 * Condition Builder Component
 */
const ConditionBuilder: React.FC<{
  condition?: any;
  onChange: (condition: any) => void;
}> = ({ condition = {}, onChange }) => {
  return (
    <Stack spacing={2}>
      <TextField
        fullWidth
        label="Field Name"
        value={condition.field || ''}
        onChange={(e) => onChange({...condition, field: e.target.value})}
      />
      <FormControl fullWidth>
        <InputLabel>Operator</InputLabel>
        <Select
          value={condition.operator || 'equals'}
          onChange={(e) => onChange({...condition, operator: e.target.value})}
          label="Operator"
        >
          <MenuItem value="equals">Equals</MenuItem>
          <MenuItem value="not_equals">Not Equals</MenuItem>
          <MenuItem value="contains">Contains</MenuItem>
          <MenuItem value="greater_than">Greater Than</MenuItem>
          <MenuItem value="less_than">Less Than</MenuItem>
        </Select>
      </FormControl>
      <TextField
        fullWidth
        label="Value"
        value={condition.value || ''}
        onChange={(e) => onChange({...condition, value: e.target.value})}
      />
    </Stack>
  );
};

/**
 * Create Sequence Dialog Component
 */
const CreateSequenceDialog: React.FC<{
  open: boolean;
  onClose: () => void;
  onSuccess: () => void;
}> = ({ open, onClose, onSuccess }) => {
  const [formData, setFormData] = useState({
    name: '',
    description: ''
  });
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async () => {
    try {
      setSubmitting(true);
      await marketingService.createEmailSequence({
        name: formData.name,
        description: formData.description,
        steps: [],
        triggerType: 'manual'
      });
      onSuccess();
      setFormData({ name: '', description: '' });
    } catch (err) {
      console.error('Error creating sequence:', err);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Create Email Sequence</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 2 }}>
          <TextField
            fullWidth
            label="Sequence Name"
            value={formData.name}
            onChange={(e) => setFormData({...formData, name: e.target.value})}
          />
          <TextField
            fullWidth
            multiline
            rows={3}
            label="Description"
            value={formData.description}
            onChange={(e) => setFormData({...formData, description: e.target.value})}
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={submitting || !formData.name}
        >
          Create
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default EmailSequenceBuilderPage;
