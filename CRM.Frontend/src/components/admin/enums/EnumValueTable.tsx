/**
 * ENUM-FE-007: EnumValueTable.tsx
 * Reusable table that displays EnumValue rows with edit/delete action callbacks.
 * Supports row highlighting for default/inactive values.
 */
import React from 'react';
import {
  Box,
  Chip,
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
import { Delete as DeleteIcon, Edit as EditIcon } from '@mui/icons-material';
import type { EnumValue } from '../../../types/enums';

export interface EnumValueTableProps {
  values: EnumValue[];
  onEdit?: (value: EnumValue) => void;
  onDelete?: (value: EnumValue) => void;
}

const EnumValueTable: React.FC<EnumValueTableProps> = ({ values, onEdit, onDelete }) => {
  return (
    <TableContainer component={Paper} variant="outlined">
      <Table size="small">
        <TableHead>
          <TableRow sx={{ '& th': { fontWeight: 600 } }}>
            <TableCell width={60}>#</TableCell>
            <TableCell>Key</TableCell>
            <TableCell>Label</TableCell>
            <TableCell width={80}>Color</TableCell>
            <TableCell width={90}>Status</TableCell>
            <TableCell width={90}>Default</TableCell>
            <TableCell width={90}>System</TableCell>
            <TableCell width={100} align="right">Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {values.length === 0 ? (
            <TableRow>
              <TableCell colSpan={8}>
                <Box sx={{ py: 4, textAlign: 'center' }}>
                  <Typography color="text.secondary">No values found.</Typography>
                </Box>
              </TableCell>
            </TableRow>
          ) : (
            values.map(val => (
              <TableRow
                key={val.id}
                hover
                sx={{ opacity: val.isActive ? 1 : 0.5 }}
              >
                <TableCell>
                  <Typography variant="body2" color="text.secondary">{val.sortOrder}</Typography>
                </TableCell>
                <TableCell>
                  <Box
                    component="span"
                    sx={{
                      fontFamily: 'monospace',
                      fontSize: '0.72rem',
                      bgcolor: 'action.hover',
                      px: 0.5,
                      py: 0.2,
                      borderRadius: 0.5,
                    }}
                  >
                    {val.key}
                  </Box>
                </TableCell>
                <TableCell>
                  <Typography variant="body2" fontWeight={500}>{val.label}</Typography>
                  {val.description && (
                    <Typography variant="caption" color="text.secondary">{val.description}</Typography>
                  )}
                </TableCell>
                <TableCell>
                  {val.color ? (
                    <Tooltip title={val.color}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                        <Box
                          sx={{
                            width: 16,
                            height: 16,
                            borderRadius: '50%',
                            bgcolor: val.color,
                            border: '1px solid',
                            borderColor: 'divider',
                          }}
                        />
                      </Box>
                    </Tooltip>
                  ) : '—'}
                </TableCell>
                <TableCell>
                  <Chip
                    label={val.isActive ? 'Active' : 'Inactive'}
                    size="small"
                    color={val.isActive ? 'success' : 'default'}
                    variant="outlined"
                  />
                </TableCell>
                <TableCell>
                  {val.isDefault ? <Chip label="Default" size="small" color="primary" /> : null}
                </TableCell>
                <TableCell>
                  {val.isSystemValue ? <Chip label="System" size="small" color="warning" /> : null}
                </TableCell>
                <TableCell align="right">
                  {onEdit && (
                    <Tooltip title={val.isSystemValue ? 'View (system)' : 'Edit'}>
                      <IconButton size="small" onClick={() => onEdit(val)}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  )}
                  {onDelete && !val.isSystemValue && (
                    <Tooltip title="Delete">
                      <IconButton size="small" color="error" onClick={() => onDelete(val)}>
                        <DeleteIcon fontSize="small" />
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

export default EnumValueTable;
