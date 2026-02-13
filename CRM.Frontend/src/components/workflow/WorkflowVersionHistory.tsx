/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Workflow Version History - list of versions with compare support
 */

import React, { useState } from 'react';
import {
  Box,
  Typography,
  List,
  ListItem,
  ListItemText,
  Button,
  Chip,
  Stack,
} from '@mui/material';
import type { WorkflowVersionSummary } from '../../services/workflowService';

interface WorkflowVersionHistoryProps {
  versions: WorkflowVersionSummary[];
  onSelect?: (versionId: number) => void;
  onCompare?: (versionId1: number, versionId2: number) => void;
}

const WorkflowVersionHistory: React.FC<WorkflowVersionHistoryProps> = ({
  versions,
  onSelect,
  onCompare,
}) => {
  const [selectedIds, setSelectedIds] = useState<number[]>([]);

  const handleToggleSelect = (versionId: number) => {
    setSelectedIds((prev) => {
      if (prev.includes(versionId)) return prev.filter(id => id !== versionId);
      return prev.length >= 2 ? [prev[1], versionId] : [...prev, versionId];
    });
  };

  const canCompare = selectedIds.length === 2 && onCompare;

  return (
    <Box>
      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 1 }}>
        <Typography variant="h6">Version History</Typography>
        <Button
          variant="outlined"
          size="small"
          disabled={!canCompare}
          onClick={() => onCompare?.(selectedIds[0], selectedIds[1])}
        >
          Compare
        </Button>
      </Stack>
      <List dense>
        {versions.map((version) => (
          <ListItem
            key={version.id}
            onClick={() => onSelect?.(version.id)}
            secondaryAction={
              <Button size="small" onClick={() => handleToggleSelect(version.id)}>
                {selectedIds.includes(version.id) ? 'Selected' : 'Select'}
              </Button>
            }
          >
            <ListItemText
              primary={`v${version.versionNumber}${version.label ? ` • ${version.label}` : ''}`}
              secondary={version.publishedAt ? `Published ${new Date(version.publishedAt).toLocaleString()}` : 'Draft'}
            />
            <Chip
              size="small"
              label={version.status}
              color={version.status === 'Active' ? 'success' : 'default'}
              sx={{ mr: 2 }}
            />
          </ListItem>
        ))}
        {!versions.length && (
          <ListItem>
            <ListItemText primary="No versions found" />
          </ListItem>
        )}
      </List>
    </Box>
  );
};

export default WorkflowVersionHistory;
