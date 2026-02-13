/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Workflow List - table view for workflow definitions
 */

import React from 'react';
import {
  Box,
  Chip,
  CircularProgress,
  IconButton,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tooltip,
  Typography,
} from '@mui/material';
import {
  AccountTree as WorkflowIcon,
  Visibility as ViewIcon,
  Timeline as TimelineIcon,
  PlayArrow as PlayIcon,
  Pause as PauseIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
} from '@mui/icons-material';
import type { WorkflowDefinition } from '../../services/workflowService';

interface WorkflowListProps {
  workflows: WorkflowDefinition[];
  loading: boolean;
  getStatusColor: (status: string) => string;
  onOpenDesigner: (workflow: WorkflowDefinition) => void;
  onViewInstances: (workflow: WorkflowDefinition) => void;
  onActivate: (workflow: WorkflowDefinition) => void;
  onPause: (workflow: WorkflowDefinition) => void;
  onEdit: (workflow: WorkflowDefinition) => void;
  onDelete: (workflow: WorkflowDefinition) => void;
}

const WorkflowList: React.FC<WorkflowListProps> = ({
  workflows,
  loading,
  getStatusColor,
  onOpenDesigner,
  onViewInstances,
  onActivate,
  onPause,
  onEdit,
  onDelete,
}) => {
  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <TableContainer component={Paper}>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>Workflow</TableCell>
            <TableCell>Entity Type</TableCell>
            <TableCell>Category</TableCell>
            <TableCell>Status</TableCell>
            <TableCell>Version</TableCell>
            <TableCell>Priority</TableCell>
            <TableCell align="right">Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {workflows.length === 0 ? (
            <TableRow>
              <TableCell colSpan={7} align="center">
                <Typography color="text.secondary" sx={{ py: 4 }}>
                  No workflows found. Create your first workflow to get started.
                </Typography>
              </TableCell>
            </TableRow>
          ) : (
            workflows.map((workflow) => (
              <TableRow key={workflow.id} hover>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Box
                      sx={{
                        width: 36,
                        height: 36,
                        borderRadius: 1,
                        backgroundColor: workflow.color || '#6750A4',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        color: 'white',
                      }}
                    >
                      <WorkflowIcon fontSize="small" />
                    </Box>
                    <Box>
                      <Typography variant="body2" fontWeight="medium">
                        {workflow.name}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {workflow.workflowKey}
                      </Typography>
                    </Box>
                  </Box>
                </TableCell>
                <TableCell>
                  <Chip label={workflow.entityType} size="small" variant="outlined" />
                </TableCell>
                <TableCell>{workflow.category || '-'}</TableCell>
                <TableCell>
                  <Chip
                    label={workflow.status}
                    size="small"
                    sx={{
                      backgroundColor: getStatusColor(workflow.status),
                      color: 'white',
                    }}
                  />
                </TableCell>
                <TableCell>v{workflow.currentVersion}</TableCell>
                <TableCell>{workflow.priority}</TableCell>
                <TableCell align="right">
                  <Tooltip title="Open Designer">
                    <IconButton size="small" onClick={() => onOpenDesigner(workflow)}>
                      <ViewIcon />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title="View Instances">
                    <IconButton size="small" onClick={() => onViewInstances(workflow)}>
                      <TimelineIcon />
                    </IconButton>
                  </Tooltip>
                  {(workflow.status === 'Draft' || workflow.status === 'Paused') && (
                    <Tooltip title="Activate">
                      <IconButton size="small" color="success" onClick={() => onActivate(workflow)}>
                        <PlayIcon />
                      </IconButton>
                    </Tooltip>
                  )}
                  {workflow.status === 'Active' && (
                    <Tooltip title="Pause">
                      <IconButton size="small" color="warning" onClick={() => onPause(workflow)}>
                        <PauseIcon />
                      </IconButton>
                    </Tooltip>
                  )}
                  <Tooltip title="Edit">
                    <IconButton size="small" onClick={() => onEdit(workflow)} disabled={workflow.isSystem}>
                      <EditIcon />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title="Delete">
                    <IconButton size="small" color="error" onClick={() => onDelete(workflow)} disabled={workflow.isSystem}>
                      <DeleteIcon />
                    </IconButton>
                  </Tooltip>
                </TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </TableContainer>
  );
};

export default WorkflowList;
