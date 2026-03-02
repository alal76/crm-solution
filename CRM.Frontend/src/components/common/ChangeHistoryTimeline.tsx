// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  CircularProgress,
  Alert,
  Chip,
  Tooltip,
  Paper,
} from '@mui/material';
import Timeline from '@mui/lab/Timeline';
import TimelineItem from '@mui/lab/TimelineItem';
import TimelineSeparator from '@mui/lab/TimelineSeparator';
import TimelineConnector from '@mui/lab/TimelineConnector';
import TimelineContent from '@mui/lab/TimelineContent';
import TimelineDot from '@mui/lab/TimelineDot';
import TimelineOppositeContent from '@mui/lab/TimelineOppositeContent';
import AddCircleOutlineIcon from '@mui/icons-material/AddCircleOutline';
import EditIcon from '@mui/icons-material/Edit';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';
import apiClient from '../../services/apiClient';

// ============================================================================
// Change History Timeline Component (TODO-SYS006-003)
// ============================================================================

/** A single field change record from the API. */
export interface FieldChange {
  id: number;
  fieldName: string;
  oldValue: string | null;
  newValue: string | null;
  dataType: string;
  changedAt: string;
  changedByUserName: string;
  changedByUserId: number;
}

export interface ChangeHistoryTimelineProps {
  /** The entity type to fetch change history for (e.g., "Account", "Contact"). */
  entityType: string;
  /** The entity ID to fetch change history for. */
  entityId: number;
  /** Maximum number of items to display. Defaults to 50. */
  maxItems?: number;
}

/**
 * Determine the change type based on old/new values.
 */
function getChangeType(change: FieldChange): 'created' | 'updated' | 'deleted' {
  if (change.oldValue === null && change.newValue !== null) return 'created';
  if (change.oldValue !== null && change.newValue === null) return 'deleted';
  return 'updated';
}

/**
 * Get color for the timeline dot based on change type.
 */
function getDotColor(changeType: 'created' | 'updated' | 'deleted'): 'success' | 'primary' | 'error' {
  switch (changeType) {
    case 'created': return 'success';
    case 'updated': return 'primary';
    case 'deleted': return 'error';
  }
}

/**
 * Get the icon for the timeline dot based on change type.
 */
function getDotIcon(changeType: 'created' | 'updated' | 'deleted'): React.ReactElement {
  switch (changeType) {
    case 'created': return <AddCircleOutlineIcon fontSize="small" />;
    case 'updated': return <EditIcon fontSize="small" />;
    case 'deleted': return <DeleteOutlineIcon fontSize="small" />;
  }
}

/**
 * Format a camelCase or PascalCase field name to a human-readable label.
 */
function formatFieldName(fieldName: string): string {
  return fieldName
    .replaceAll(/([A-Z])/g, ' $1')
    .replace(/^./, (str: string) => str.toUpperCase())
    .trim();
}

/**
 * Format a value for display, handling null and long strings.
 */
function formatValue(value: string | null, dataType: string): string {
  if (value === null || value === undefined) return '(empty)';

  // Handle date types
  if (dataType === 'DateTime' || dataType === 'DateTimeOffset') {
    try {
      const date = new Date(value);
      if (!Number.isNaN(date.getTime())) {
        return date.toLocaleString();
      }
    } catch {
      // Fall through to default
    }
  }

  // Handle boolean types
  if (dataType === 'Boolean') {
    return value === 'true' || value === 'True' ? 'Yes' : 'No';
  }

  // Truncate long strings
  if (value.length > 100) {
    return value.substring(0, 100) + '…';
  }

  return value;
}

/**
 * Get a human-readable relative timestamp (e.g., "2 hours ago").
 */
function getRelativeTime(dateStr: string): string {
  const now = new Date();
  const date = new Date(dateStr);
  const diffMs = now.getTime() - date.getTime();
  const diffSec = Math.floor(diffMs / 1000);
  const diffMin = Math.floor(diffSec / 60);
  const diffHour = Math.floor(diffMin / 60);
  const diffDay = Math.floor(diffHour / 24);

  if (diffSec < 60) return 'just now';
  if (diffMin < 60) return `${diffMin}m ago`;
  if (diffHour < 24) return `${diffHour}h ago`;
  if (diffDay < 7) return `${diffDay}d ago`;
  return date.toLocaleDateString();
}

/**
 * Change History Timeline component.
 *
 * Displays a vertical timeline of field-level changes for a specific entity.
 * Features:
 * - Colored dots: green (created), blue (updated), red (deleted)
 * - Field name with old→new value comparison
 * - User attribution and relative timestamps
 * - Loading and error states
 */
const ChangeHistoryTimeline: React.FC<ChangeHistoryTimelineProps> = ({
  entityType,
  entityId,
  maxItems = 50,
}) => {
  const [changes, setChanges] = useState<FieldChange[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    const fetchHistory = async () => {
      if (!entityType || !entityId) {
        setLoading(false);
        return;
      }

      setLoading(true);
      setError(null);

      try {
        const response = await apiClient.get<FieldChange[]>(
          `/fieldchangelogs/${encodeURIComponent(entityType)}/${entityId}`
        );
        if (!cancelled) {
          setChanges((response.data || []).slice(0, maxItems));
        }
      } catch (err) {
        if (!cancelled) {
          setError('Unable to load change history. The feature may not be configured yet.');
          setChanges([]);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    fetchHistory();

    return () => {
      cancelled = true;
    };
  }, [entityType, entityId, maxItems]);

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" py={4}>
        <CircularProgress size={30} />
        <Typography variant="body2" color="text.secondary" sx={{ ml: 2 }}>
          Loading change history…
        </Typography>
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="info" sx={{ my: 1 }}>
        {error}
      </Alert>
    );
  }

  if (changes.length === 0) {
    return (
      <Box py={3} textAlign="center">
        <Typography variant="body2" color="text.secondary">
          No change history available for this {entityType.toLowerCase()}.
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="subtitle2" gutterBottom sx={{ px: 2, pt: 1 }}>
        Change History ({changes.length} {changes.length === 1 ? 'change' : 'changes'})
      </Typography>
      <Timeline position="right" sx={{ px: 0, py: 0 }}>
        {changes.map((change, index) => {
          const changeType = getChangeType(change);
          const dotColor = getDotColor(changeType);
          const icon = getDotIcon(changeType);
          const isLast = index === changes.length - 1;

          return (
            <TimelineItem key={change.id}>
              <TimelineOppositeContent
                sx={{ maxWidth: 100, flex: '0 0 100px', px: 1 }}
                variant="caption"
                color="text.secondary"
              >
                <Tooltip title={new Date(change.changedAt).toLocaleString()} arrow>
                  <span>{getRelativeTime(change.changedAt)}</span>
                </Tooltip>
              </TimelineOppositeContent>

              <TimelineSeparator>
                <TimelineDot color={dotColor} variant="outlined" sx={{ p: 0.5 }}>
                  {icon}
                </TimelineDot>
                {!isLast && <TimelineConnector />}
              </TimelineSeparator>

              <TimelineContent sx={{ pb: 2, px: 2 }}>
                <Paper
                  elevation={0}
                  sx={{
                    p: 1.5,
                    backgroundColor: 'grey.50',
                    borderRadius: 1,
                    border: '1px solid',
                    borderColor: 'divider',
                  }}
                >
                  <Box display="flex" alignItems="center" gap={1} mb={0.5}>
                    <Typography variant="body2" fontWeight={600}>
                      {formatFieldName(change.fieldName)}
                    </Typography>
                    <Chip
                      label={changeType}
                      size="small"
                      color={dotColor}
                      variant="outlined"
                      sx={{ height: 20, fontSize: '0.65rem' }}
                    />
                  </Box>

                  {changeType === 'updated' && (
                    <Box display="flex" alignItems="center" gap={0.5} flexWrap="wrap">
                      <Typography
                        variant="caption"
                        sx={{
                          textDecoration: 'line-through',
                          color: 'error.main',
                          wordBreak: 'break-word',
                        }}
                      >
                        {formatValue(change.oldValue, change.dataType)}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        →
                      </Typography>
                      <Typography
                        variant="caption"
                        sx={{
                          color: 'success.main',
                          fontWeight: 500,
                          wordBreak: 'break-word',
                        }}
                      >
                        {formatValue(change.newValue, change.dataType)}
                      </Typography>
                    </Box>
                  )}

                  {changeType === 'created' && (
                    <Typography variant="caption" sx={{ color: 'success.main' }}>
                      Set to: {formatValue(change.newValue, change.dataType)}
                    </Typography>
                  )}

                  {changeType === 'deleted' && (
                    <Typography variant="caption" sx={{ color: 'error.main' }}>
                      Removed: {formatValue(change.oldValue, change.dataType)}
                    </Typography>
                  )}

                  <Typography variant="caption" display="block" color="text.secondary" mt={0.5}>
                    by {change.changedByUserName || `User #${change.changedByUserId}`}
                  </Typography>
                </Paper>
              </TimelineContent>
            </TimelineItem>
          );
        })}
      </Timeline>
    </Box>
  );
};

export default ChangeHistoryTimeline;
