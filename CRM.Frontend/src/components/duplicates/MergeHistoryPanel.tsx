// TODO: Integration target — account/contact detail pages
// This component is currently orphaned (not imported by any page).

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Merge History Panel - Shows merge history for a record with unmerge capability

import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Paper,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  ListItemSecondaryAction,
  IconButton,
  Chip,
  Collapse,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
  AlertTitle,
  Tooltip,
  Divider,
  CircularProgress,
  Accordion,
  AccordionSummary,
  AccordionDetails,
} from '@mui/material';
import {
  MergeType as MergeIcon,
  Undo as UndoIcon,
  ExpandMore as ExpandMoreIcon,
  History as HistoryIcon,
  Person as PersonIcon,
  AccessTime as TimeIcon,
  Info as InfoIcon,
  CheckCircle as ActiveIcon,
  Cancel as UnmergedIcon,
} from '@mui/icons-material';
import {
  MergeGroupInfo,
  MergedRecordInfo,
  getMergeHistory,
  getMergedRecords,
  unmergeRecords,
  UnmergeRequest,
  UnmergeResult,
} from '../../services/duplicateService';

interface MergeHistoryPanelProps {
  entityType: 'Lead' | 'Contact' | 'Account';
  recordId: number;
  onUnmergeComplete?: (result: UnmergeResult) => void;
}

const MergeHistoryPanel: React.FC<MergeHistoryPanelProps> = ({
  entityType,
  recordId,
  onUnmergeComplete,
}) => {
  const [mergeHistory, setMergeHistory] = useState<MergeGroupInfo[]>([]);
  const [mergedRecords, setMergedRecords] = useState<MergedRecordInfo[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [unmergeDialogOpen, setUnmergeDialogOpen] = useState(false);
  const [selectedGroup, setSelectedGroup] = useState<MergeGroupInfo | null>(null);
  const [isUnmerging, setIsUnmerging] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadData();
  }, [entityType, recordId]);

  const loadData = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const [history, merged] = await Promise.all([
        getMergeHistory(entityType, recordId),
        getMergedRecords(entityType, recordId),
      ]);
      setMergeHistory(history);
      setMergedRecords(merged);
    } catch (err: unknown) {
      setError((err as Error).message || 'Failed to load merge history');
    } finally {
      setIsLoading(false);
    }
  };

  const handleUnmergeClick = (group: MergeGroupInfo) => {
    setSelectedGroup(group);
    setUnmergeDialogOpen(true);
  };

  const handleUnmerge = async () => {
    if (!selectedGroup) return;

    setIsUnmerging(true);
    setError(null);

    try {
      const request: UnmergeRequest = {
        mergeGroupId: selectedGroup.id,
        restoreRelatedRecords: true,
      };

      const result = await unmergeRecords(request);

      if (result.success) {
        setUnmergeDialogOpen(false);
        await loadData();
        onUnmergeComplete?.(result);
      } else {
        setError(result.errorMessage || 'Unmerge failed');
      }
    } catch (err: unknown) {
      setError((err as Error).message || 'Failed to unmerge records');
    } finally {
      setIsUnmerging(false);
    }
  };

  const getStatusChip = (status: string) => {
    switch (status) {
      case 'Active':
        return <Chip icon={<ActiveIcon />} label="Active" size="small" color="success" />;
      case 'Unmerged':
        return <Chip icon={<UnmergedIcon />} label="Unmerged" size="small" color="default" />;
      case 'PartialUnmerge':
        return <Chip icon={<InfoIcon />} label="Partially Unmerged" size="small" color="warning" />;
      default:
        return <Chip label={status} size="small" />;
    }
  };

  const formatDate = (date: Date) => {
    return new Date(date).toLocaleString();
  };

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
        <CircularProgress size={24} />
      </Box>
    );
  }

  const hasHistory = mergeHistory.length > 0 || mergedRecords.length > 0;

  if (!hasHistory) {
    return (
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, color: 'text.secondary' }}>
          <HistoryIcon />
          <Typography>No merge history for this record</Typography>
        </Box>
      </Paper>
    );
  }

  return (
    <Box>
      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Merged Into This Record */}
      {mergedRecords.length > 0 && (
        <Paper variant="outlined" sx={{ mb: 2 }}>
          <Box sx={{ p: 2, bgcolor: 'primary.main', color: 'white' }}>
            <Typography variant="subtitle2" sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <MergeIcon />
              Records Merged Into This {entityType}
            </Typography>
          </Box>
          <List dense>
            {mergedRecords.map((record, index) => (
              <React.Fragment key={record.recordId}>
                {index > 0 && <Divider />}
                <ListItem>
                  <ListItemIcon>
                    <PersonIcon />
                  </ListItemIcon>
                  <ListItemText
                    primary={`${entityType} #${record.recordId}`}
                    secondary={
                      <Box component="span" sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <TimeIcon fontSize="inherit" />
                        Merged on {formatDate(record.mergedAt)}
                      </Box>
                    }
                  />
                  <ListItemSecondaryAction>
                    <Chip
                      label={record.status}
                      size="small"
                      color={record.status === 'Merged' ? 'success' : 'default'}
                    />
                  </ListItemSecondaryAction>
                </ListItem>
              </React.Fragment>
            ))}
          </List>
        </Paper>
      )}

      {/* Merge Groups */}
      {mergeHistory.length > 0 && (
        <Box>
          <Typography variant="subtitle2" sx={{ mb: 1, display: 'flex', alignItems: 'center', gap: 1 }}>
            <HistoryIcon />
            Merge History
          </Typography>

          {mergeHistory.map((group) => (
            <Accordion key={group.id} variant="outlined" sx={{ mb: 1 }}>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, width: '100%' }}>
                  <MergeIcon color="primary" />
                  <Box sx={{ flexGrow: 1 }}>
                    <Typography variant="body2" fontWeight="medium">
                      Merge Group #{group.groupIdentifier.substring(0, 8)}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {formatDate(group.mergedAt)}
                      {group.mergedByName && ` by ${group.mergedByName}`}
                    </Typography>
                  </Box>
                  {getStatusChip(group.status)}
                </Box>
              </AccordionSummary>
              <AccordionDetails>
                <Box>
                  {/* Master Record */}
                  <Box sx={{ mb: 2 }}>
                    <Typography variant="caption" color="text.secondary">
                      Master Record
                    </Typography>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 0.5 }}>
                      <Chip label={`ID: ${group.masterRecordId}`} size="small" color="primary" />
                      {group.masterRecordId === recordId && (
                        <Chip label="This record" size="small" variant="outlined" />
                      )}
                    </Box>
                  </Box>

                  {/* Members */}
                  <Typography variant="caption" color="text.secondary">
                    Merged Records
                  </Typography>
                  <List dense sx={{ bgcolor: 'grey.50', borderRadius: 1, mt: 0.5 }}>
                    {group.members
                      .filter((m) => !m.isMaster)
                      .map((member) => (
                        <ListItem key={member.recordId}>
                          <ListItemIcon>
                            <PersonIcon fontSize="small" />
                          </ListItemIcon>
                          <ListItemText
                            primary={`${entityType} #${member.recordId}`}
                            secondary={member.status}
                          />
                        </ListItem>
                      ))}
                  </List>

                  {/* Notes */}
                  {group.notes && (
                    <Box sx={{ mt: 2, p: 1, bgcolor: 'grey.100', borderRadius: 1 }}>
                      <Typography variant="caption" color="text.secondary">
                        Notes:
                      </Typography>
                      <Typography variant="body2">{group.notes}</Typography>
                    </Box>
                  )}

                  {/* Unmerge Button */}
                  {group.status === 'Active' && group.masterRecordId === recordId && (
                    <Box sx={{ mt: 2, display: 'flex', justifyContent: 'flex-end' }}>
                      <Button
                        variant="outlined"
                        color="warning"
                        startIcon={<UndoIcon />}
                        onClick={() => handleUnmergeClick(group)}
                        size="small"
                      >
                        Unmerge Records
                      </Button>
                    </Box>
                  )}

                  {/* Unmerged Info */}
                  {group.status === 'Unmerged' && group.unmergedAt && (
                    <Alert severity="info" sx={{ mt: 2 }}>
                      This merge was undone on {formatDate(group.unmergedAt)}
                    </Alert>
                  )}
                </Box>
              </AccordionDetails>
            </Accordion>
          ))}
        </Box>
      )}

      {/* Unmerge Confirmation Dialog */}
      <Dialog open={unmergeDialogOpen} onClose={() => setUnmergeDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <UndoIcon color="warning" />
            Unmerge Records
          </Box>
        </DialogTitle>
        <DialogContent>
          <Alert severity="warning" sx={{ mb: 2 }}>
            <AlertTitle>Are you sure?</AlertTitle>
            This will restore the previously merged records to active status. Related records
            that were relinked during merge will remain with the master record.
          </Alert>

          {selectedGroup && (
            <Box>
              <Typography variant="body2" gutterBottom>
                The following records will be restored:
              </Typography>
              <List dense>
                {selectedGroup.members
                  .filter((m) => !m.isMaster && m.status === 'Merged')
                  .map((member) => (
                    <ListItem key={member.recordId}>
                      <ListItemIcon>
                        <PersonIcon />
                      </ListItemIcon>
                      <ListItemText primary={`${entityType} #${member.recordId}`} />
                    </ListItem>
                  ))}
              </List>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setUnmergeDialogOpen(false)} disabled={isUnmerging}>
            Cancel
          </Button>
          <Button
            variant="contained"
            color="warning"
            onClick={handleUnmerge}
            disabled={isUnmerging}
            startIcon={isUnmerging ? <CircularProgress size={16} /> : <UndoIcon />}
          >
            {isUnmerging ? 'Unmerging...' : 'Unmerge Records'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default MergeHistoryPanel;
