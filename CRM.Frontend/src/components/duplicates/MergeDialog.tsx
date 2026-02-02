// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Merge Dialog - Dialog for merging duplicate records

import React, { useState, useEffect, useCallback } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Typography,
  Stepper,
  Step,
  StepLabel,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TableContainer,
  Paper,
  Radio,
  RadioGroup,
  FormControlLabel,
  Chip,
  Alert,
  AlertTitle,
  TextField,
  LinearProgress,
  Checkbox,
  Divider,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
} from '@mui/material';
import {
  MergeType as MergeIcon,
  Person as PersonIcon,
  Business as BusinessIcon,
  PersonAdd as LeadIcon,
  CheckCircle as CheckCircleIcon,
  Warning as WarningIcon,
  Undo as UndoIcon,
  Link as LinkIcon,
} from '@mui/icons-material';
import {
  MergeRequest,
  MergePreview,
  MergeResult,
  previewMerge,
  mergeRecords,
} from '../../services/duplicateService';

interface RecordData {
  id: number;
  displayName: string;
  data: Record<string, any>;
}

interface MergeDialogProps {
  open: boolean;
  onClose: () => void;
  entityType: 'Lead' | 'Contact' | 'Account';
  records: RecordData[];
  onMergeComplete: (result: MergeResult) => void;
}

const steps = ['Select Master Record', 'Choose Field Values', 'Review & Confirm'];

const MergeDialog: React.FC<MergeDialogProps> = ({
  open,
  onClose,
  entityType,
  records,
  onMergeComplete,
}) => {
  const [activeStep, setActiveStep] = useState(0);
  const [masterRecordId, setMasterRecordId] = useState<number | null>(null);
  const [fieldSources, setFieldSources] = useState<Record<string, number>>({});
  const [preview, setPreview] = useState<MergePreview | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [relinkRelatedRecords, setRelinkRelatedRecords] = useState(true);
  const [notes, setNotes] = useState('');

  // Reset state when dialog opens
  useEffect(() => {
    if (open) {
      setActiveStep(0);
      setMasterRecordId(records[0]?.id ?? null);
      setFieldSources({});
      setPreview(null);
      setError(null);
      setNotes('');
    }
  }, [open, records]);

  // Load preview when moving to step 2
  useEffect(() => {
    if (activeStep === 1 && masterRecordId && !preview) {
      loadPreview();
    }
  }, [activeStep, masterRecordId]);

  const loadPreview = async () => {
    if (!masterRecordId) return;

    setIsLoading(true);
    setError(null);

    try {
      const request: MergeRequest = {
        entityType,
        masterRecordId,
        recordsToMerge: records.filter((r) => r.id !== masterRecordId).map((r) => r.id),
        fieldSourceOverrides: fieldSources,
        relinkRelatedRecords,
      };

      const result = await previewMerge(request);
      setPreview(result);

      // Initialize field sources from preview
      const sources: Record<string, number> = {};
      Object.entries(result.fieldPreviews).forEach(([fieldName, fieldPreview]) => {
        sources[fieldName] = fieldPreview.sourceRecordId;
      });
      setFieldSources(sources);
    } catch (err: any) {
      setError(err.message || 'Failed to load merge preview');
    } finally {
      setIsLoading(false);
    }
  };

  const handleNext = () => {
    if (activeStep === steps.length - 1) {
      handleMerge();
    } else {
      setActiveStep((prev) => prev + 1);
    }
  };

  const handleBack = () => {
    setActiveStep((prev) => prev - 1);
  };

  const handleMerge = async () => {
    if (!masterRecordId) return;

    setIsLoading(true);
    setError(null);

    try {
      const request: MergeRequest = {
        entityType,
        masterRecordId,
        recordsToMerge: records.filter((r) => r.id !== masterRecordId).map((r) => r.id),
        fieldSourceOverrides: fieldSources,
        relinkRelatedRecords,
        notes,
      };

      const result = await mergeRecords(request);

      if (result.success) {
        onMergeComplete(result);
        onClose();
      } else {
        setError(result.errorMessage || 'Merge failed');
      }
    } catch (err: any) {
      setError(err.message || 'Failed to merge records');
    } finally {
      setIsLoading(false);
    }
  };

  const handleFieldSourceChange = (fieldName: string, recordId: number) => {
    setFieldSources((prev) => ({
      ...prev,
      [fieldName]: recordId,
    }));
  };

  const getEntityIcon = () => {
    switch (entityType) {
      case 'Lead':
        return <LeadIcon />;
      case 'Contact':
        return <PersonIcon />;
      case 'Account':
        return <BusinessIcon />;
    }
  };

  const renderMasterSelection = () => (
    <Box>
      <Alert severity="info" sx={{ mb: 2 }}>
        <AlertTitle>Select Master Record</AlertTitle>
        The master record will be preserved. Other records will be merged into it and marked as
        duplicates.
      </Alert>

      <TableContainer component={Paper} variant="outlined">
        <Table>
          <TableHead>
            <TableRow sx={{ bgcolor: 'primary.main' }}>
              <TableCell sx={{ color: 'white' }}>Master</TableCell>
              <TableCell sx={{ color: 'white' }}>{entityType}</TableCell>
              <TableCell sx={{ color: 'white' }}>Details</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {records.map((record) => (
              <TableRow
                key={record.id}
                hover
                onClick={() => setMasterRecordId(record.id)}
                sx={{
                  cursor: 'pointer',
                  bgcolor: masterRecordId === record.id ? 'action.selected' : undefined,
                }}
              >
                <TableCell>
                  <Radio checked={masterRecordId === record.id} onChange={() => {}} />
                </TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    {getEntityIcon()}
                    <Typography fontWeight="medium">{record.displayName}</Typography>
                  </Box>
                </TableCell>
                <TableCell>
                  <Typography variant="body2" color="text.secondary">
                    ID: {record.id}
                    {record.data.email && ` • ${record.data.email}`}
                    {record.data.phone && ` • ${record.data.phone}`}
                  </Typography>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Box sx={{ mt: 2 }}>
        <FormControlLabel
          control={
            <Checkbox
              checked={relinkRelatedRecords}
              onChange={(e) => setRelinkRelatedRecords(e.target.checked)}
            />
          }
          label={
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <LinkIcon fontSize="small" />
              <Typography>Relink related records (activities, notes, etc.) to master</Typography>
            </Box>
          }
        />
      </Box>
    </Box>
  );

  const renderFieldSelection = () => (
    <Box>
      {isLoading ? (
        <Box sx={{ py: 4 }}>
          <LinearProgress />
          <Typography align="center" sx={{ mt: 2 }}>
            Loading field preview...
          </Typography>
        </Box>
      ) : preview ? (
        <>
          <Alert severity="info" sx={{ mb: 2 }}>
            For each field, select which record's value to use in the merged record.
          </Alert>

          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow sx={{ bgcolor: 'grey.100' }}>
                  <TableCell>Field</TableCell>
                  {records.map((record) => (
                    <TableCell key={record.id} align="center">
                      <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                        <Typography variant="body2" fontWeight="medium">
                          {record.displayName}
                        </Typography>
                        {record.id === masterRecordId && (
                          <Chip label="Master" size="small" color="primary" sx={{ mt: 0.5 }} />
                        )}
                      </Box>
                    </TableCell>
                  ))}
                </TableRow>
              </TableHead>
              <TableBody>
                {Object.entries(preview.fieldPreviews).map(([fieldName, fieldPreview]) => (
                  <TableRow key={fieldName}>
                    <TableCell>
                      <Typography variant="body2" fontWeight="medium">
                        {fieldPreview.displayName}
                      </Typography>
                    </TableCell>
                    {records.map((record) => {
                      const value = fieldPreview.allValues[record.id];
                      const isSelected = fieldSources[fieldName] === record.id;
                      return (
                        <TableCell
                          key={record.id}
                          align="center"
                          sx={{
                            cursor: 'pointer',
                            bgcolor: isSelected ? 'action.selected' : undefined,
                            '&:hover': { bgcolor: 'action.hover' },
                          }}
                          onClick={() => handleFieldSourceChange(fieldName, record.id)}
                        >
                          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 1 }}>
                            <Radio
                              size="small"
                              checked={isSelected}
                              onChange={() => {}}
                            />
                            <Typography
                              variant="body2"
                              sx={{
                                fontWeight: isSelected ? 'bold' : 'normal',
                                color: value ? 'text.primary' : 'text.disabled',
                              }}
                            >
                              {value || '(empty)'}
                            </Typography>
                          </Box>
                        </TableCell>
                      );
                    })}
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </>
      ) : (
        <Alert severity="error">Failed to load preview</Alert>
      )}
    </Box>
  );

  const renderReview = () => (
    <Box>
      <Alert severity="warning" sx={{ mb: 2 }} icon={<WarningIcon />}>
        <AlertTitle>Review Before Merging</AlertTitle>
        This action will merge {records.length - 1} record(s) into the master record. Merged
        records will be soft-deleted but can be unmerged later.
      </Alert>

      {/* Master Record Summary */}
      <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
        <Typography variant="subtitle2" gutterBottom>
          Master Record
        </Typography>
        {records.find((r) => r.id === masterRecordId) && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            {getEntityIcon()}
            <Typography fontWeight="bold">
              {records.find((r) => r.id === masterRecordId)?.displayName}
            </Typography>
            <Chip label={`ID: ${masterRecordId}`} size="small" />
          </Box>
        )}
      </Paper>

      {/* Records to Merge */}
      <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
        <Typography variant="subtitle2" gutterBottom>
          Records to Merge (will be marked as duplicates)
        </Typography>
        <List dense>
          {records
            .filter((r) => r.id !== masterRecordId)
            .map((record) => (
              <ListItem key={record.id}>
                <ListItemIcon>{getEntityIcon()}</ListItemIcon>
                <ListItemText
                  primary={record.displayName}
                  secondary={`ID: ${record.id}`}
                />
              </ListItem>
            ))}
        </List>
      </Paper>

      {/* Related Records */}
      {preview?.relatedRecords && preview.relatedRecords.length > 0 && (
        <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
          <Typography variant="subtitle2" gutterBottom>
            Related Records to Relink
          </Typography>
          <List dense>
            {preview.relatedRecords.map((related, index) => (
              <ListItem key={index}>
                <ListItemIcon>
                  <LinkIcon />
                </ListItemIcon>
                <ListItemText primary={related.description} />
              </ListItem>
            ))}
          </List>
        </Paper>
      )}

      {/* Warnings */}
      {preview?.warnings && preview.warnings.length > 0 && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          <AlertTitle>Warnings</AlertTitle>
          <List dense>
            {preview.warnings.map((warning, index) => (
              <ListItem key={index}>{warning}</ListItem>
            ))}
          </List>
        </Alert>
      )}

      {/* Notes */}
      <TextField
        fullWidth
        label="Notes (optional)"
        placeholder="Add a note about why these records are being merged..."
        multiline
        rows={2}
        value={notes}
        onChange={(e) => setNotes(e.target.value)}
        sx={{ mb: 2 }}
      />

      {/* Undo Notice */}
      <Alert severity="info" icon={<UndoIcon />}>
        You can undo this merge later by using the "Unmerge" feature on the master record.
      </Alert>
    </Box>
  );

  const getStepContent = () => {
    switch (activeStep) {
      case 0:
        return renderMasterSelection();
      case 1:
        return renderFieldSelection();
      case 2:
        return renderReview();
      default:
        return null;
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <MergeIcon color="primary" />
          <Typography variant="h6">Merge {entityType} Records</Typography>
          <Chip label={`${records.length} records`} size="small" sx={{ ml: 1 }} />
        </Box>
      </DialogTitle>

      <DialogContent>
        {error && (
          <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
            {error}
          </Alert>
        )}

        <Stepper activeStep={activeStep} sx={{ mb: 3 }}>
          {steps.map((label) => (
            <Step key={label}>
              <StepLabel>{label}</StepLabel>
            </Step>
          ))}
        </Stepper>

        {getStepContent()}
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose} disabled={isLoading}>
          Cancel
        </Button>
        {activeStep > 0 && (
          <Button onClick={handleBack} disabled={isLoading}>
            Back
          </Button>
        )}
        <Button
          variant="contained"
          onClick={handleNext}
          disabled={isLoading || (activeStep === 0 && !masterRecordId)}
          startIcon={activeStep === steps.length - 1 ? <MergeIcon /> : undefined}
        >
          {isLoading ? 'Processing...' : activeStep === steps.length - 1 ? 'Merge Records' : 'Next'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default MergeDialog;
