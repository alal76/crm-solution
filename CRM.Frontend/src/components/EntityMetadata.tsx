import React from 'react';
import { Box, Typography, Tooltip, Divider } from '@mui/material';
import { BaseEntity } from '../types';

interface EntityMetadataProps {
  entity: BaseEntity;
  showDivider?: boolean;
  compact?: boolean;
}

/**
 * Displays audit/metadata information for entities.
 * Consistent way to show createdAt, updatedAt across all entity views.
 */
const EntityMetadata: React.FC<EntityMetadataProps> = ({ 
  entity, 
  showDivider = true,
  compact = false 
}) => {
  const formatDate = (dateString?: string | null): string => {
    if (!dateString) return 'N/A';
    try {
      return new Date(dateString).toLocaleString();
    } catch {
      return dateString;
    }
  };

  if (compact) {
    return (
      <Tooltip title={`Created: ${formatDate(entity.createdAt)}${entity.updatedAt ? ` • Updated: ${formatDate(entity.updatedAt)}` : ''}`}>
        <Typography variant="caption" color="textSecondary" sx={{ cursor: 'help' }}>
          {entity.createdAt ? formatDate(entity.createdAt).split(',')[0] : 'N/A'}
        </Typography>
      </Tooltip>
    );
  }

  return (
    <Box sx={{ mt: 2 }}>
      {showDivider && <Divider sx={{ mb: 1 }} />}
      <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
        <Typography variant="caption" color="textSecondary">
          <strong>Created:</strong> {formatDate(entity.createdAt)}
        </Typography>
        {entity.updatedAt && (
          <Typography variant="caption" color="textSecondary">
            <strong>Last Updated:</strong> {formatDate(entity.updatedAt)}
          </Typography>
        )}
        {entity.isDeleted !== undefined && entity.isDeleted && (
          <Typography variant="caption" color="error">
            <strong>Status:</strong> Deleted
          </Typography>
        )}
      </Box>
    </Box>
  );
};

export default EntityMetadata;
