/**
 * ENUM-FE-006: EnumCategoryGrid.tsx
 * Reusable grid that displays a list of LookupCategoryDto items in a MUI Table.
 * Used by EnumManagementPage as an alternative to the built-in DataGrid.
 */
import React from 'react';
import {
  Box,
  Chip,
  IconButton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tooltip,
  Typography,
  Paper,
} from '@mui/material';
import { Edit as EditIcon } from '@mui/icons-material';
import type { EnumCategory } from '../../../types/enums';

export interface EnumCategoryGridProps {
  categories: EnumCategory[];
  onSelect?: (category: EnumCategory) => void;
}

const EnumCategoryGrid: React.FC<EnumCategoryGridProps> = ({ categories, onSelect }) => {
  return (
    <TableContainer component={Paper} variant="outlined">
      <Table size="small">
        <TableHead>
          <TableRow sx={{ '& th': { fontWeight: 600 } }}>
            <TableCell width={70}>ID</TableCell>
            <TableCell>Name</TableCell>
            <TableCell width={150}>Entity</TableCell>
            <TableCell width={150}>Property</TableCell>
            <TableCell width={80} align="center">Values</TableCell>
            <TableCell width={100} align="center">Type</TableCell>
            <TableCell width={80} align="right">Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {categories.length === 0 ? (
            <TableRow>
              <TableCell colSpan={7}>
                <Box sx={{ py: 4, textAlign: 'center' }}>
                  <Typography color="text.secondary">No categories found.</Typography>
                </Box>
              </TableCell>
            </TableRow>
          ) : (
            categories.map(cat => (
              <TableRow key={cat.id} hover>
                <TableCell>
                  <Typography variant="body2" color="text.secondary">{cat.id}</Typography>
                </TableCell>
                <TableCell>
                  <Typography variant="body2" fontWeight={600}>{cat.displayName || cat.name}</Typography>
                  {cat.description && (
                    <Typography variant="caption" color="text.secondary">{cat.description}</Typography>
                  )}
                </TableCell>
                <TableCell>
                  <Typography variant="body2">{cat.entityType || '—'}</Typography>
                </TableCell>
                <TableCell>
                  <Typography variant="body2" sx={{ fontFamily: 'monospace', fontSize: '0.75rem' }}>
                    {cat.propertyName || '—'}
                  </Typography>
                </TableCell>
                <TableCell align="center">
                  <Typography variant="body2">{cat.valueCount ?? 0}</Typography>
                </TableCell>
                <TableCell align="center">
                  <Chip
                    label={cat.isSystemManaged ? 'System' : 'Custom'}
                    size="small"
                    color={cat.isSystemManaged ? 'info' : 'default'}
                    variant="outlined"
                  />
                </TableCell>
                <TableCell align="right">
                  {onSelect && (
                    <Tooltip title="Edit values">
                      <IconButton size="small" onClick={() => onSelect(cat)}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  )}
                </TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </TableContainer>
  );
};

export default EnumCategoryGrid;
