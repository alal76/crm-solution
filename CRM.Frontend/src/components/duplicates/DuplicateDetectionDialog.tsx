// TODO: Integration target — account/contact create and edit pages
// This component is currently orphaned (not imported by any page).

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Duplicate Detection Dialog - Shows potential duplicates when creating/updating records

import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Typography,
  Alert,
  AlertTitle,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TableContainer,
  Paper,
  Chip,
  IconButton,
  Collapse,
  LinearProgress,
  Tooltip,
  Radio,
  RadioGroup,
  FormControlLabel,
  Divider,
} from '@mui/material';
import {
  Warning as WarningIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
  CheckCircle as CheckCircleIcon,
  ContentCopy as DuplicateIcon,
  MergeType as MergeIcon,
  Edit as EditIcon,
  Add as AddIcon,
} from '@mui/icons-material';
import {
  DuplicateCheckResult,
  DuplicateMatch,
  getRecommendationText,
  getMatchTypeText,
  getMatchScoreColor,
  formatMatchScore,
} from '../../services/duplicateService';

interface DuplicateDetectionDialogProps {
  open: boolean;
  onClose: () => void;
  checkResult: DuplicateCheckResult | null;
  isLoading?: boolean;
  entityType: 'Lead' | 'Contact' | 'Account';
  onCreateNew: () => void;
  onUpdateExisting: (recordId: number) => void;
  onViewRecord: (recordId: number) => void;
  onMergeRecords?: (masterRecordId: number, recordsToMerge: number[]) => void;
}

const DuplicateDetectionDialog: React.FC<DuplicateDetectionDialogProps> = ({
  open,
  onClose,
  checkResult,
  isLoading = false,
  entityType,
  onCreateNew,
  onUpdateExisting,
  onViewRecord,
  onMergeRecords,
}) => {
  const [expandedRow, setExpandedRow] = useState<number | null>(null);
  const [selectedAction, setSelectedAction] = useState<'create' | 'update'>('create');
  const [selectedRecordId, setSelectedRecordId] = useState<number | null>(null);

  useEffect(() => {
    if (checkResult?.recommendedRecordId) {
      setSelectedRecordId(checkResult.recommendedRecordId);
      setSelectedAction(checkResult.recommendation === 'UpdateExisting' ? 'update' : 'create');
    }
  }, [checkResult]);

  const handleToggleExpand = (recordId: number) => {
    setExpandedRow(expandedRow === recordId ? null : recordId);
  };

  const handleConfirm = () => {
    if (selectedAction === 'create') {
      onCreateNew();
    } else if (selectedRecordId) {
      onUpdateExisting(selectedRecordId);
    }
    onClose();
  };

  const renderMatchScore = (score: number) => {
    const color = getMatchScoreColor(score);
    return (
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <LinearProgress
          variant="determinate"
          value={score}
          color={color as 'error' | 'warning' | 'success'}
          sx={{ width: 60, height: 8, borderRadius: 4 }}
        />
        <Typography variant="body2" fontWeight="bold">
          {formatMatchScore(score)}
        </Typography>
      </Box>
    );
  };

  const renderFieldComparisons = (match: DuplicateMatch) => (
    <TableContainer component={Paper} variant="outlined" sx={{ mt: 1 }}>
      <Table size="small">
        <TableHead>
          <TableRow sx={{ bgcolor: 'grey.100' }}>
            <TableCell>Field</TableCell>
            <TableCell>Your Value</TableCell>
            <TableCell>Existing Value</TableCell>
            <TableCell>Match</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {match.matchedFields.map((field) => (
            <TableRow key={field.fieldName}>
              <TableCell>
                <Typography variant="body2" fontWeight="medium">
                  {field.displayName}
                </Typography>
              </TableCell>
              <TableCell>
                <Typography variant="body2">{field.currentValue || '-'}</Typography>
              </TableCell>
              <TableCell>
                <Typography variant="body2">{field.matchedValue || '-'}</Typography>
              </TableCell>
              <TableCell>
                {field.matchType !== 'None' && (
                  <Tooltip title={`${Math.round(field.matchConfidence)}% confidence`}>
                    <Chip
                      label={getMatchTypeText(field.matchType)}
                      size="small"
                      color={
                        field.matchType === 'Exact'
                          ? 'error'
                          : field.matchType === 'Fuzzy' || field.matchType === 'Phonetic'
                            ? 'warning'
                            : 'default'
                      }
                      variant="outlined"
                    />
                  </Tooltip>
                )}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );

  if (!checkResult && !isLoading) {
    return null;
  }

  const hasDuplicates = checkResult?.hasDuplicates ?? false;
  const matches = checkResult?.matches ?? [];
  const highConfidence = checkResult?.highConfidenceMatch ?? false;

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          {hasDuplicates ? (
            <WarningIcon color={highConfidence ? 'error' : 'warning'} />
          ) : (
            <CheckCircleIcon color="success" />
          )}
          <Typography variant="h6">
            {hasDuplicates
              ? `Potential Duplicate ${entityType}${matches.length > 1 ? 's' : ''} Found`
              : 'No Duplicates Found'}
          </Typography>
        </Box>
      </DialogTitle>

      <DialogContent>
        {isLoading ? (
          <Box sx={{ py: 4 }}>
            <LinearProgress />
            <Typography align="center" sx={{ mt: 2 }}>
              Checking for duplicates...
            </Typography>
          </Box>
        ) : (
          <>
            {/* Recommendation Alert */}
            {checkResult && (
              <Alert
                severity={
                  checkResult.recommendation === 'CreateNew'
                    ? 'success'
                    : checkResult.recommendation === 'UpdateExisting'
                      ? 'warning'
                      : 'info'
                }
                sx={{ mb: 2 }}
              >
                <AlertTitle>Recommendation</AlertTitle>
                {getRecommendationText(checkResult)}
              </Alert>
            )}

            {/* Matches Table */}
            {hasDuplicates && (
              <>
                <Typography variant="subtitle2" gutterBottom>
                  Found {matches.length} potential match{matches.length > 1 ? 'es' : ''}:
                </Typography>

                <TableContainer component={Paper} variant="outlined">
                  <Table>
                    <TableHead>
                      <TableRow sx={{ bgcolor: 'primary.main' }}>
                        <TableCell sx={{ color: 'white' }}>Select</TableCell>
                        <TableCell sx={{ color: 'white' }}>{entityType}</TableCell>
                        <TableCell sx={{ color: 'white' }}>Match Score</TableCell>
                        <TableCell sx={{ color: 'white' }}>Created</TableCell>
                        <TableCell sx={{ color: 'white' }}>Actions</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {matches.map((match) => (
                        <React.Fragment key={match.recordId}>
                          <TableRow
                            hover
                            sx={{
                              cursor: 'pointer',
                              bgcolor:
                                selectedRecordId === match.recordId ? 'action.selected' : undefined,
                            }}
                            onClick={() => setSelectedRecordId(match.recordId)}
                          >
                            <TableCell>
                              <Radio
                                checked={
                                  selectedAction === 'update' && selectedRecordId === match.recordId
                                }
                                onChange={() => {
                                  setSelectedAction('update');
                                  setSelectedRecordId(match.recordId);
                                }}
                              />
                            </TableCell>
                            <TableCell>
                              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <IconButton
                                  size="small"
                                  onClick={(e) => {
                                    e.stopPropagation();
                                    handleToggleExpand(match.recordId);
                                  }}
                                >
                                  {expandedRow === match.recordId ? (
                                    <ExpandLessIcon />
                                  ) : (
                                    <ExpandMoreIcon />
                                  )}
                                </IconButton>
                                <Box>
                                  <Typography variant="body2" fontWeight="bold">
                                    {match.recordSummary.displayName}
                                  </Typography>
                                  {match.recordSummary.additionalInfo.email && (
                                    <Typography variant="caption" color="text.secondary">
                                      {match.recordSummary.additionalInfo.email}
                                    </Typography>
                                  )}
                                </Box>
                              </Box>
                            </TableCell>
                            <TableCell>{renderMatchScore(match.matchScore)}</TableCell>
                            <TableCell>
                              <Typography variant="body2">
                                {new Date(match.recordSummary.createdAt).toLocaleDateString()}
                              </Typography>
                            </TableCell>
                            <TableCell>
                              <Tooltip title="View Record">
                                <IconButton
                                  size="small"
                                  onClick={(e) => {
                                    e.stopPropagation();
                                    onViewRecord(match.recordId);
                                  }}
                                >
                                  <EditIcon fontSize="small" />
                                </IconButton>
                              </Tooltip>
                            </TableCell>
                          </TableRow>
                          <TableRow>
                            <TableCell colSpan={5} sx={{ py: 0, border: 0 }}>
                              <Collapse
                                in={expandedRow === match.recordId}
                                timeout="auto"
                                unmountOnExit
                              >
                                <Box sx={{ p: 2 }}>{renderFieldComparisons(match)}</Box>
                              </Collapse>
                            </TableCell>
                          </TableRow>
                        </React.Fragment>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>

                <Divider sx={{ my: 2 }} />

                {/* Action Selection */}
                <Typography variant="subtitle2" gutterBottom>
                  What would you like to do?
                </Typography>

                <RadioGroup
                  value={selectedAction}
                  onChange={(e) => setSelectedAction(e.target.value as 'create' | 'update')}
                >
                  <FormControlLabel
                    value="create"
                    control={<Radio />}
                    label={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <AddIcon fontSize="small" />
                        <Typography>Create as a new {entityType.toLowerCase()} anyway</Typography>
                      </Box>
                    }
                  />
                  <FormControlLabel
                    value="update"
                    control={<Radio />}
                    disabled={!selectedRecordId}
                    label={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <EditIcon fontSize="small" />
                        <Typography>
                          Update the selected existing {entityType.toLowerCase()}
                        </Typography>
                      </Box>
                    }
                  />
                </RadioGroup>

                {onMergeRecords && matches.length > 1 && (
                  <Box sx={{ mt: 2 }}>
                    <Button
                      variant="outlined"
                      startIcon={<MergeIcon />}
                      onClick={() => {
                        if (selectedRecordId) {
                          onMergeRecords(
                            selectedRecordId,
                            matches
                              .filter((m) => m.recordId !== selectedRecordId)
                              .map((m) => m.recordId)
                          );
                        }
                      }}
                      disabled={!selectedRecordId}
                    >
                      Merge Duplicate Records
                    </Button>
                    <Typography variant="caption" display="block" color="text.secondary" sx={{ mt: 0.5 }}>
                      Combine multiple records into one master record
                    </Typography>
                  </Box>
                )}
              </>
            )}
          </>
        )}
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose} color="inherit">
          Cancel
        </Button>
        {!isLoading && (
          <Button
            variant="contained"
            onClick={handleConfirm}
            color={selectedAction === 'create' ? 'primary' : 'secondary'}
            startIcon={selectedAction === 'create' ? <AddIcon /> : <EditIcon />}
          >
            {selectedAction === 'create' ? `Create New ${entityType}` : `Update Existing`}
          </Button>
        )}
      </DialogActions>
    </Dialog>
  );
};

export default DuplicateDetectionDialog;
