/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Workflow Toolbar - designer toolbar actions and status messages
 */

import React from 'react';
import {
  Alert,
  Box,
  CircularProgress,
  Divider,
  IconButton,
  Paper,
  Tooltip,
  Typography,
} from '@mui/material';
import {
  ZoomOut as ZoomOutIcon,
  ZoomIn as ZoomInIcon,
  FitScreen as FitScreenIcon,
  GridOn as GridIcon,
  Science as SimulatorIcon,
  History as VersionIcon,
} from '@mui/icons-material';

interface WorkflowToolbarProps {
  error?: string;
  success?: string;
  onClearError: () => void;
  onClearSuccess: () => void;
  zoom: number;
  onZoomOut: () => void;
  onZoomIn: () => void;
  onFitScreen: () => void;
  showGrid: boolean;
  onToggleGrid: () => void;
  onOpenSimulator: () => void;
  onOpenVersionHistory: () => void;
  saving?: boolean;
}

const WorkflowToolbar: React.FC<WorkflowToolbarProps> = ({
  error,
  success,
  onClearError,
  onClearSuccess,
  zoom,
  onZoomOut,
  onZoomIn,
  onFitScreen,
  showGrid,
  onToggleGrid,
  onOpenSimulator,
  onOpenVersionHistory,
  saving = false,
}) => {
  return (
    <Paper sx={{ p: 1, display: 'flex', alignItems: 'center', gap: 1 }}>
      {error && (
        <Alert severity="error" onClose={onClearError} sx={{ flex: 1, py: 0 }}>
          {error}
        </Alert>
      )}
      {success && (
        <Alert severity="success" onClose={onClearSuccess} sx={{ flex: 1, py: 0 }}>
          {success}
        </Alert>
      )}
      <Box sx={{ flex: 1 }} />
      <Tooltip title="Zoom Out">
        <IconButton size="small" onClick={onZoomOut}>
          <ZoomOutIcon />
        </IconButton>
      </Tooltip>
      <Typography variant="body2" sx={{ minWidth: 50, textAlign: 'center' }}>
        {Math.round(zoom * 100)}%
      </Typography>
      <Tooltip title="Zoom In">
        <IconButton size="small" onClick={onZoomIn}>
          <ZoomInIcon />
        </IconButton>
      </Tooltip>
      <Tooltip title="Fit to Screen">
        <IconButton size="small" onClick={onFitScreen}>
          <FitScreenIcon />
        </IconButton>
      </Tooltip>
      <Divider orientation="vertical" flexItem sx={{ mx: 1 }} />
      <Tooltip title="Toggle Grid">
        <IconButton
          size="small"
          onClick={onToggleGrid}
          color={showGrid ? 'primary' : 'default'}
        >
          <GridIcon />
        </IconButton>
      </Tooltip>
      <Tooltip title="Test Workflow">
        <IconButton size="small" onClick={onOpenSimulator} color="default">
          <SimulatorIcon />
        </IconButton>
      </Tooltip>
      <Tooltip title="Version History">
        <IconButton size="small" onClick={onOpenVersionHistory} color="default">
          <VersionIcon />
        </IconButton>
      </Tooltip>
      <Divider orientation="vertical" flexItem sx={{ mx: 1 }} />
      {saving && <CircularProgress size={24} />}
    </Paper>
  );
};

export default WorkflowToolbar;
