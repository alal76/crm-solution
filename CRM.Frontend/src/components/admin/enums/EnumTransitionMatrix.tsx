/**
 * ENUM-FE-012: EnumTransitionMatrix.tsx
 * Displays a visual grid of allowed state transitions for an enum category.
 * Rows = From values, Columns = To values. Cell shows a checkmark or dash.
 */
import React from 'react';
import {
  Box,
  Chip,
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
import { CheckCircle as CheckIcon, RemoveCircleOutline as BlockIcon } from '@mui/icons-material';
import type { EnumTransition, EnumValue } from '../../../types/enums';

export interface EnumTransitionMatrixProps {
  values: EnumValue[];
  transitions: EnumTransition[];
}

const EnumTransitionMatrix: React.FC<EnumTransitionMatrixProps> = ({ values, transitions }) => {
  if (!values.length) {
    return (
      <Typography color="text.secondary" variant="body2">
        No values configured. Add values to this category first.
      </Typography>
    );
  }

  /** Look up whether a specific from→to transition is allowed */
  const isAllowed = (fromId: number | undefined, toId: number): boolean => {
    if (fromId === undefined) {
      // "any" wildcard transitions
      return transitions.some(t => t.fromValueId === undefined && t.toValueId === toId && t.isAllowed);
    }
    return transitions.some(
      t => t.fromValueId === fromId && t.toValueId === toId && t.isAllowed
    );
  };

  const requiresApproval = (fromId: number | undefined, toId: number): boolean =>
    transitions.some(
      t => t.fromValueId === fromId && t.toValueId === toId && t.requiresApproval
    );

  return (
    <Box>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
        ✔ = transition allowed &nbsp;|&nbsp; A = requires approval &nbsp;|&nbsp; — = blocked
      </Typography>
      <TableContainer component={Paper} variant="outlined" sx={{ maxWidth: '100%', overflow: 'auto' }}>
        <Table size="small" sx={{ minWidth: 400 }}>
          <TableHead>
            <TableRow>
              <TableCell sx={{ fontWeight: 700, bgcolor: 'background.default', minWidth: 120 }}>
                From ↓ / To →
              </TableCell>
              {values.map(v => (
                <TableCell
                  key={v.id}
                  align="center"
                  sx={{ fontWeight: 600, minWidth: 90 }}
                >
                  <Chip
                    label={v.label}
                    size="small"
                    sx={{
                      bgcolor: v.color ? `${v.color}22` : undefined,
                      borderColor: v.color,
                      border: v.color ? '1px solid' : undefined,
                    }}
                  />
                </TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {values.map(fromVal => (
              <TableRow key={fromVal.id}>
                <TableCell sx={{ fontWeight: 500 }}>
                  <Chip
                    label={fromVal.label}
                    size="small"
                    sx={{
                      bgcolor: fromVal.color ? `${fromVal.color}22` : undefined,
                      borderColor: fromVal.color,
                      border: fromVal.color ? '1px solid' : undefined,
                    }}
                  />
                </TableCell>
                {values.map(toVal => {
                  const same = fromVal.id === toVal.id;
                  const allowed = !same && isAllowed(fromVal.id, toVal.id);
                  const approval = allowed && requiresApproval(fromVal.id, toVal.id);
                  return (
                    <TableCell key={toVal.id} align="center">
                      {same ? (
                        <Typography variant="caption" color="text.disabled">—</Typography>
                      ) : allowed ? (
                        <Tooltip title={approval ? 'Allowed (requires approval)' : 'Allowed'}>
                          <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', gap: 0.25 }}>
                            <CheckIcon fontSize="small" color={approval ? 'warning' : 'success'} />
                            {approval && <Typography variant="caption" color="warning.main">A</Typography>}
                          </Box>
                        </Tooltip>
                      ) : (
                        <Tooltip title="Blocked">
                          <BlockIcon fontSize="small" sx={{ color: 'text.disabled', opacity: 0.4 }} />
                        </Tooltip>
                      )}
                    </TableCell>
                  );
                })}
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
      {transitions.length === 0 && (
        <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
          No transition rules defined. By default all transitions are permitted.
        </Typography>
      )}
    </Box>
  );
};

export default EnumTransitionMatrix;
