// Change Conflict Detector - Alert for scheduling conflicts between changes
// Part of ITSM Enhancement Plan - Phase 2.3

import React, { useState, useMemo } from 'react';
import {
  Box,
  Paper,
  Typography,
  Alert,
  AlertTitle,
  Stack,
  Chip,
  Button,
  IconButton,
  Collapse,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Divider,
  Tooltip,
  Badge,
  Card,
  CardContent,
  CardActions,
} from '@mui/material';
import {
  Warning as WarningIcon,
  Error as ErrorIcon,
  Schedule as ScheduleIcon,
  Storage as CIIcon,
  Person as PersonIcon,
  CalendarMonth as CalendarIcon,
  ExpandMore as ExpandIcon,
  ExpandLess as CollapseIcon,
  Close as CloseIcon,
  Edit as RescheduleIcon,
  CheckCircle as ApproveIcon,
  Info as InfoIcon,
} from '@mui/icons-material';

export type ConflictType = 'schedule' | 'resource' | 'ci' | 'freeze' | 'dependency';
export type ConflictSeverity = 'warning' | 'error' | 'critical';

export interface ConflictingChange {
  id: number;
  changeNumber: string;
  title: string;
  scheduledStart: Date | string;
  scheduledEnd: Date | string;
  assignedTo: string;
  status: string;
  priority: number;
}

export interface ChangeConflict {
  id: string;
  type: ConflictType;
  severity: ConflictSeverity;
  description: string;
  affectedCI?: string[];
  conflictingChange?: ConflictingChange;
  freezePeriod?: {
    name: string;
    start: Date | string;
    end: Date | string;
    reason: string;
  };
  recommendation?: string;
}

export interface ChangeRequest {
  id: number;
  changeNumber: string;
  title: string;
  scheduledStart: Date | string;
  scheduledEnd: Date | string;
  affectedCIs: string[];
  assignedTo: string;
}

export interface ChangeConflictDetectorProps {
  currentChange: ChangeRequest;
  conflicts: ChangeConflict[];
  onDismiss?: (conflictId: string) => void;
  onReschedule?: (changeId: number) => void;
  onViewChange?: (changeNumber: string) => void;
  onRequestOverride?: (conflictId: string) => void;
  allowOverride?: boolean;
  compact?: boolean;
}

const getConflictTypeLabel = (type: ConflictType): string => {
  switch (type) {
    case 'schedule':
      return 'Schedule Overlap';
    case 'resource':
      return 'Resource Conflict';
    case 'ci':
      return 'CI Conflict';
    case 'freeze':
      return 'Change Freeze';
    case 'dependency':
      return 'Dependency Conflict';
    default:
      return type;
  }
};

const getConflictTypeIcon = (type: ConflictType) => {
  switch (type) {
    case 'schedule':
      return <ScheduleIcon />;
    case 'resource':
      return <PersonIcon />;
    case 'ci':
      return <CIIcon />;
    case 'freeze':
      return <CalendarIcon />;
    case 'dependency':
      return <WarningIcon />;
    default:
      return <InfoIcon />;
  }
};

const getSeverityColor = (severity: ConflictSeverity): 'warning' | 'error' => {
  switch (severity) {
    case 'warning':
      return 'warning';
    case 'error':
    case 'critical':
      return 'error';
    default:
      return 'warning';
  }
};

const formatDateTime = (date: Date | string): string => {
  const d = typeof date === 'string' ? new Date(date) : date;
  return d.toLocaleString('en-US', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
};

export const ChangeConflictDetector: React.FC<ChangeConflictDetectorProps> = ({
  currentChange,
  conflicts,
  onDismiss,
  onReschedule,
  onViewChange,
  onRequestOverride,
  allowOverride = false,
  compact = false,
}) => {
  const [expanded, setExpanded] = useState(!compact);
  const [selectedConflict, setSelectedConflict] = useState<ChangeConflict | null>(null);
  const [detailDialogOpen, setDetailDialogOpen] = useState(false);

  // Group conflicts by severity
  const conflictsByType = useMemo(() => {
    const critical = conflicts.filter((c) => c.severity === 'critical');
    const errors = conflicts.filter((c) => c.severity === 'error');
    const warnings = conflicts.filter((c) => c.severity === 'warning');
    return { critical, errors, warnings };
  }, [conflicts]);

  const hasBlockers = conflictsByType.critical.length > 0 || conflictsByType.errors.length > 0;

  if (conflicts.length === 0) {
    return (
      <Alert severity="success" sx={{ mb: 2 }}>
        No scheduling conflicts detected for this change.
      </Alert>
    );
  }

  const handleViewDetails = (conflict: ChangeConflict) => {
    setSelectedConflict(conflict);
    setDetailDialogOpen(true);
  };

  const renderConflictItem = (conflict: ChangeConflict) => (
    <Alert
      key={conflict.id}
      severity={getSeverityColor(conflict.severity)}
      icon={getConflictTypeIcon(conflict.type)}
      action={
        <Stack direction="row" spacing={0.5}>
          <Tooltip title="View Details">
            <IconButton
              size="small"
              onClick={() => handleViewDetails(conflict)}
              color="inherit"
            >
              <InfoIcon fontSize="small" />
            </IconButton>
          </Tooltip>
          {onDismiss && conflict.severity === 'warning' && (
            <Tooltip title="Dismiss">
              <IconButton
                size="small"
                onClick={() => onDismiss(conflict.id)}
                color="inherit"
              >
                <CloseIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </Stack>
      }
      sx={{ mb: 1 }}
    >
      <AlertTitle sx={{ fontSize: '0.9rem' }}>
        {getConflictTypeLabel(conflict.type)}
        {conflict.severity === 'critical' && (
          <Chip
            label="Blocker"
            size="small"
            color="error"
            sx={{ ml: 1, height: 18 }}
          />
        )}
      </AlertTitle>
      <Typography variant="body2">{conflict.description}</Typography>
      {conflict.conflictingChange && (
        <Stack direction="row" alignItems="center" spacing={1} sx={{ mt: 1 }}>
          <Typography
            variant="caption"
            sx={{ cursor: 'pointer', textDecoration: 'underline' }}
            onClick={() => onViewChange?.(conflict.conflictingChange!.changeNumber)}
          >
            {conflict.conflictingChange.changeNumber}
          </Typography>
          <Chip
            label={`P${conflict.conflictingChange.priority}`}
            size="small"
            sx={{ height: 18 }}
          />
        </Stack>
      )}
      {conflict.freezePeriod && (
        <Typography variant="caption" color="text.secondary">
          {formatDateTime(conflict.freezePeriod.start)} -{' '}
          {formatDateTime(conflict.freezePeriod.end)}
        </Typography>
      )}
    </Alert>
  );

  return (
    <>
      <Paper
        sx={{
          border: `2px solid ${hasBlockers ? '#f44336' : '#ff9800'}`,
          mb: 2,
        }}
      >
        {/* Header */}
        <Box
          sx={{
            backgroundColor: hasBlockers ? '#ffebee' : '#fff3e0',
            p: 2,
            cursor: 'pointer',
          }}
          onClick={() => setExpanded(!expanded)}
        >
          <Stack direction="row" alignItems="center" justifyContent="space-between">
            <Stack direction="row" alignItems="center" spacing={2}>
              <Badge
                badgeContent={conflicts.length}
                color={hasBlockers ? 'error' : 'warning'}
              >
                {hasBlockers ? <ErrorIcon color="error" /> : <WarningIcon color="warning" />}
              </Badge>
              <Box>
                <Typography variant="subtitle1" fontWeight={600}>
                  {conflicts.length} Conflict{conflicts.length > 1 ? 's' : ''} Detected
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {hasBlockers
                    ? 'This change cannot proceed until conflicts are resolved'
                    : 'Review warnings before proceeding'}
                </Typography>
              </Box>
            </Stack>
            <IconButton size="small">
              {expanded ? <CollapseIcon /> : <ExpandIcon />}
            </IconButton>
          </Stack>
        </Box>

        {/* Conflict list */}
        <Collapse in={expanded}>
          <Box sx={{ p: 2 }}>
            {/* Critical/Error conflicts */}
            {(conflictsByType.critical.length > 0 || conflictsByType.errors.length > 0) && (
              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle2" color="error" sx={{ mb: 1 }}>
                  Blocking Issues ({conflictsByType.critical.length + conflictsByType.errors.length})
                </Typography>
                {[...conflictsByType.critical, ...conflictsByType.errors].map(renderConflictItem)}
              </Box>
            )}

            {/* Warnings */}
            {conflictsByType.warnings.length > 0 && (
              <Box>
                <Typography variant="subtitle2" color="warning.dark" sx={{ mb: 1 }}>
                  Warnings ({conflictsByType.warnings.length})
                </Typography>
                {conflictsByType.warnings.map(renderConflictItem)}
              </Box>
            )}

            {/* Actions */}
            {onReschedule && (
              <Box sx={{ mt: 2, pt: 2, borderTop: '1px solid #e0e0e0' }}>
                <Button
                  startIcon={<RescheduleIcon />}
                  variant="outlined"
                  size="small"
                  onClick={() => onReschedule(currentChange.id)}
                >
                  Reschedule This Change
                </Button>
              </Box>
            )}
          </Box>
        </Collapse>
      </Paper>

      {/* Conflict Detail Dialog */}
      <Dialog
        open={detailDialogOpen}
        onClose={() => setDetailDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        {selectedConflict && (
          <>
            <DialogTitle>
              <Stack direction="row" alignItems="center" spacing={1}>
                {getConflictTypeIcon(selectedConflict.type)}
                <Typography variant="h6">
                  {getConflictTypeLabel(selectedConflict.type)}
                </Typography>
                <Chip
                  label={selectedConflict.severity.toUpperCase()}
                  size="small"
                  color={getSeverityColor(selectedConflict.severity)}
                />
              </Stack>
            </DialogTitle>
            <DialogContent>
              <Typography variant="body1" sx={{ mb: 2 }}>
                {selectedConflict.description}
              </Typography>

              {/* Current change info */}
              <Card variant="outlined" sx={{ mb: 2 }}>
                <CardContent>
                  <Typography variant="subtitle2" color="text.secondary">
                    Current Change
                  </Typography>
                  <Typography variant="body1" fontWeight={500}>
                    {currentChange.changeNumber} - {currentChange.title}
                  </Typography>
                  <Stack direction="row" spacing={2} sx={{ mt: 1 }}>
                    <Typography variant="caption">
                      <ScheduleIcon sx={{ fontSize: 14, verticalAlign: 'middle', mr: 0.5 }} />
                      {formatDateTime(currentChange.scheduledStart)} -{' '}
                      {formatDateTime(currentChange.scheduledEnd)}
                    </Typography>
                  </Stack>
                </CardContent>
              </Card>

              {/* Conflicting change info */}
              {selectedConflict.conflictingChange && (
                <Card variant="outlined" sx={{ mb: 2 }}>
                  <CardContent>
                    <Typography variant="subtitle2" color="text.secondary">
                      Conflicting Change
                    </Typography>
                    <Typography variant="body1" fontWeight={500}>
                      {selectedConflict.conflictingChange.changeNumber} -{' '}
                      {selectedConflict.conflictingChange.title}
                    </Typography>
                    <Stack direction="row" spacing={2} sx={{ mt: 1 }}>
                      <Typography variant="caption">
                        <ScheduleIcon sx={{ fontSize: 14, verticalAlign: 'middle', mr: 0.5 }} />
                        {formatDateTime(selectedConflict.conflictingChange.scheduledStart)} -{' '}
                        {formatDateTime(selectedConflict.conflictingChange.scheduledEnd)}
                      </Typography>
                      <Typography variant="caption">
                        <PersonIcon sx={{ fontSize: 14, verticalAlign: 'middle', mr: 0.5 }} />
                        {selectedConflict.conflictingChange.assignedTo}
                      </Typography>
                    </Stack>
                  </CardContent>
                  <CardActions>
                    <Button
                      size="small"
                      onClick={() =>
                        onViewChange?.(selectedConflict.conflictingChange!.changeNumber)
                      }
                    >
                      View Change
                    </Button>
                  </CardActions>
                </Card>
              )}

              {/* Freeze period info */}
              {selectedConflict.freezePeriod && (
                <Card variant="outlined" sx={{ mb: 2, backgroundColor: '#fff3e0' }}>
                  <CardContent>
                    <Typography variant="subtitle2" color="warning.dark">
                      Change Freeze Period
                    </Typography>
                    <Typography variant="body1" fontWeight={500}>
                      {selectedConflict.freezePeriod.name}
                    </Typography>
                    <Typography variant="body2" sx={{ mt: 1 }}>
                      {selectedConflict.freezePeriod.reason}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {formatDateTime(selectedConflict.freezePeriod.start)} -{' '}
                      {formatDateTime(selectedConflict.freezePeriod.end)}
                    </Typography>
                  </CardContent>
                </Card>
              )}

              {/* Affected CIs */}
              {selectedConflict.affectedCI && selectedConflict.affectedCI.length > 0 && (
                <Box sx={{ mb: 2 }}>
                  <Typography variant="subtitle2" sx={{ mb: 1 }}>
                    Affected Configuration Items
                  </Typography>
                  <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
                    {selectedConflict.affectedCI.map((ci) => (
                      <Chip
                        key={ci}
                        icon={<CIIcon />}
                        label={ci}
                        size="small"
                        variant="outlined"
                      />
                    ))}
                  </Stack>
                </Box>
              )}

              {/* Recommendation */}
              {selectedConflict.recommendation && (
                <Alert severity="info">
                  <AlertTitle>Recommendation</AlertTitle>
                  {selectedConflict.recommendation}
                </Alert>
              )}
            </DialogContent>
            <DialogActions>
              <Button onClick={() => setDetailDialogOpen(false)}>Close</Button>
              {allowOverride && selectedConflict.severity !== 'critical' && onRequestOverride && (
                <Button
                  variant="outlined"
                  color="warning"
                  startIcon={<ApproveIcon />}
                  onClick={() => {
                    onRequestOverride(selectedConflict.id);
                    setDetailDialogOpen(false);
                  }}
                >
                  Request Override
                </Button>
              )}
              {onReschedule && (
                <Button
                  variant="contained"
                  startIcon={<RescheduleIcon />}
                  onClick={() => {
                    onReschedule(currentChange.id);
                    setDetailDialogOpen(false);
                  }}
                >
                  Reschedule
                </Button>
              )}
            </DialogActions>
          </>
        )}
      </Dialog>
    </>
  );
};

export default ChangeConflictDetector;
