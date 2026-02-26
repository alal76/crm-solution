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
  Chip,
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
  Undo as UndoIcon,
  Redo as RedoIcon,
  Save as SaveIcon,
  Publish as PublishIcon,
  ContentCopy as CloneIcon,
  Code as ScriptIcon,
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
  canUndo?: boolean;
  canRedo?: boolean;
  onUndo?: () => void;
  onRedo?: () => void;
  onSave?: () => void;
  onPublish?: () => void;
  onClone?: () => void;
  hasChanges?: boolean;
  isDraftVersion?: boolean;
  showScriptPanel?: boolean;
  onToggleScriptPanel?: () => void;
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
  canUndo = false,
  canRedo = false,
  onUndo,
  onRedo,
  onSave,
  onPublish,
  onClone,
  hasChanges = false,
  isDraftVersion = true,
  showScriptPanel = false,
  onToggleScriptPanel,
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
      {/* Undo/Redo */}
      <Tooltip title="Undo (Ctrl+Z)">
        <span>
          <IconButton size="small" onClick={onUndo} disabled={!canUndo}>
            <UndoIcon />
          </IconButton>
        </span>
      </Tooltip>
      <Tooltip title="Redo (Ctrl+Y)">
        <span>
          <IconButton size="small" onClick={onRedo} disabled={!canRedo}>
            <RedoIcon />
          </IconButton>
        </span>
      </Tooltip>
      <Divider orientation="vertical" flexItem sx={{ mx: 0.5 }} />
      {/* Save/Publish/Clone */}
      {onSave && (
        <Tooltip title="Save">
          <span>
            <IconButton size="small" onClick={onSave} disabled={!hasChanges || saving} color={hasChanges ? 'primary' : 'default'}>
              <SaveIcon />
            </IconButton>
          </span>
        </Tooltip>
      )}
      {onPublish && isDraftVersion && (
        <Tooltip title="Publish Version">
          <IconButton size="small" onClick={onPublish} color="success">
            <PublishIcon />
          </IconButton>
        </Tooltip>
      )}
      {onClone && (
        <Tooltip title="Clone Workflow">
          <IconButton size="small" onClick={onClone}>
            <CloneIcon />
          </IconButton>
        </Tooltip>
      )}
      <Divider orientation="vertical" flexItem sx={{ mx: 0.5 }} />
      <Box sx={{ flex: 1 }} />
      {onToggleScriptPanel && (
        <>
          <Tooltip title={showScriptPanel ? 'Hide JSON script panel' : 'Show live JSON script panel'}>
            <IconButton
              size="small"
              onClick={onToggleScriptPanel}
              color={showScriptPanel ? 'primary' : 'default'}
              sx={showScriptPanel ? { backgroundColor: 'action.selected', borderRadius: 1 } : {}}
            >
              <ScriptIcon />
            </IconButton>
          </Tooltip>
          <Divider orientation="vertical" flexItem sx={{ mx: 0.5 }} />
        </>
      )}
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
      {hasChanges && !saving && (
        <Chip label="Unsaved" size="small" color="warning" variant="outlined" />
      )}
    </Paper>
  );
};

export default WorkflowToolbar;
