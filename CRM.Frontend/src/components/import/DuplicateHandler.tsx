/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This software is source-available. Non-commercial use is permitted under
 * the terms of the LICENSE file. Commercial use requires a separate license.
 * See the LICENSE file in the root directory for full terms.
 */

import { useMemo } from 'react';
import {
  Box,
  Typography,
  Radio,
  RadioGroup,
  FormControlLabel,
  FormControl,
  FormLabel,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Button,
  Alert,
  Chip,
  ButtonGroup,
  Stack,
  Divider,
} from '@mui/material';
import {
  SkipNext as SkipIcon,
  Sync as OverwriteIcon,
  MergeType as MergeIcon,
} from '@mui/icons-material';

// ============================================================================
// Types
// ============================================================================

export type DuplicateStrategy = 'skip_all' | 'overwrite_all' | 'ask_each' | 'merge_all';
export type RecordDecision = 'skip' | 'overwrite' | 'merge';

export interface DuplicateRecord {
  importRow: Record<string, unknown>;
  existingRecord: Record<string, unknown>;
  matchField: string;
  matchValue: string;
  decision?: RecordDecision;
}

export interface DuplicateHandlerProps {
  duplicates: DuplicateRecord[];
  onStrategyChange: (strategy: DuplicateStrategy) => void;
  onRecordDecision: (matchValue: string, action: RecordDecision) => void;
  currentStrategy: DuplicateStrategy;
}

// ============================================================================
// Helpers
// ============================================================================

const STRATEGY_LABELS: Record<DuplicateStrategy, string> = {
  skip_all: 'Skip All — keep existing records unchanged',
  overwrite_all: 'Overwrite All — replace all existing records',
  merge_all: 'Merge All — update only blank fields in existing records',
  ask_each: 'Ask Each — decide per duplicate record',
};

function summarise(duplicates: DuplicateRecord[], strategy: DuplicateStrategy) {
  if (strategy === 'skip_all') {
    return { skip: duplicates.length, overwrite: 0, merge: 0, pending: 0 };
  }
  if (strategy === 'overwrite_all') {
    return { skip: 0, overwrite: duplicates.length, merge: 0, pending: 0 };
  }
  if (strategy === 'merge_all') {
    return { skip: 0, overwrite: 0, merge: duplicates.length, pending: 0 };
  }
  // ask_each
  let skip = 0, overwrite = 0, merge = 0, pending = 0;
  for (const d of duplicates) {
    if (d.decision === 'skip') skip++;
    else if (d.decision === 'overwrite') overwrite++;
    else if (d.decision === 'merge') merge++;
    else pending++;
  }
  return { skip, overwrite, merge, pending };
}

// ============================================================================
// Pre-formatted row values for display
// ============================================================================

function getDisplayFields(record: Record<string, unknown>, max = 3): string {
  return Object.entries(record)
    .filter(([, v]) => v != null && v !== '')
    .slice(0, max)
    .map(([k, v]) => `${k}: ${String(v)}`)
    .join(' · ');
}

// ============================================================================
// Component
// ============================================================================

export default function DuplicateHandler({
  duplicates,
  onStrategyChange,
  onRecordDecision,
  currentStrategy,
}: DuplicateHandlerProps) {
  const summary = useMemo(
    () => summarise(duplicates, currentStrategy),
    [duplicates, currentStrategy],
  );

  if (duplicates.length === 0) {
    return (
      <Alert severity="success">
        No duplicate records detected. You can proceed with the import.
      </Alert>
    );
  }

  return (
    <Box>
      {/* Summary alert */}
      <Alert
        severity="warning"
        sx={{ mb: 3 }}
        icon={false}
      >
        <Typography variant="body2">
          <strong>{duplicates.length}</strong> duplicate{duplicates.length !== 1 ? 's' : ''} found.{' '}
          {summary.skip > 0 && <>{summary.skip} will be <strong>skipped</strong>. </>}
          {summary.overwrite > 0 && <>{summary.overwrite} will be <strong>overwritten</strong>. </>}
          {summary.merge > 0 && <>{summary.merge} will be <strong>merged</strong>. </>}
          {summary.pending > 0 && (
            <Typography component="span" color="error.main">
              {summary.pending} still need a decision.
            </Typography>
          )}
        </Typography>
      </Alert>

      {/* Strategy selector */}
      <Paper variant="outlined" sx={{ p: 2, mb: 3 }}>
        <FormControl component="fieldset">
          <FormLabel component="legend">
            <Typography variant="subtitle2" fontWeight={600} gutterBottom>
              Duplicate Handling Strategy
            </Typography>
          </FormLabel>
          <RadioGroup
            value={currentStrategy}
            onChange={(_, val) => onStrategyChange(val as DuplicateStrategy)}
          >
            {(Object.entries(STRATEGY_LABELS) as [DuplicateStrategy, string][]).map(
              ([value, label]) => (
                <FormControlLabel
                  key={value}
                  value={value}
                  control={<Radio size="small" />}
                  label={<Typography variant="body2">{label}</Typography>}
                />
              ),
            )}
          </RadioGroup>
        </FormControl>
      </Paper>

      {/* Per-record table — only shown in ask_each mode */}
      {currentStrategy === 'ask_each' && (
        <>
          <Divider sx={{ mb: 2 }} />
          <Typography variant="subtitle2" fontWeight={600} gutterBottom>
            Review Each Duplicate
          </Typography>
          <TableContainer component={Paper} variant="outlined" sx={{ maxHeight: 420, overflow: 'auto' }}>
            <Table size="small" stickyHeader>
              <TableHead>
                <TableRow sx={{ bgcolor: 'grey.50' }}>
                  <TableCell sx={{ fontWeight: 600, width: '20%' }}>
                    Match Field
                  </TableCell>
                  <TableCell sx={{ fontWeight: 600, width: '30%' }}>
                    Importing Row
                  </TableCell>
                  <TableCell sx={{ fontWeight: 600, width: '30%' }}>
                    Existing Record
                  </TableCell>
                  <TableCell sx={{ fontWeight: 600, width: '20%' }} align="center">
                    Decision
                  </TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {duplicates.map((dup) => {
                  const decision = dup.decision;
                  return (
                    <TableRow
                      key={dup.matchValue}
                      sx={{
                        bgcolor: decision
                          ? decision === 'skip'
                            ? 'grey.50'
                            : decision === 'overwrite'
                            ? 'warning.50'
                            : 'info.50'
                          : 'error.50',
                        '&:hover': { filter: 'brightness(0.97)' },
                      }}
                    >
                      {/* Match field */}
                      <TableCell>
                        <Stack spacing={0.25}>
                          <Typography variant="caption" color="text.secondary">
                            {dup.matchField}
                          </Typography>
                          <Chip
                            label={dup.matchValue}
                            size="small"
                            variant="outlined"
                            sx={{ maxWidth: 160 }}
                          />
                        </Stack>
                      </TableCell>

                      {/* Import row preview */}
                      <TableCell>
                        <Typography variant="body2" color="text.secondary" sx={{ fontSize: 11 }}>
                          {getDisplayFields(dup.importRow)}
                        </Typography>
                      </TableCell>

                      {/* Existing record preview */}
                      <TableCell>
                        <Typography variant="body2" color="text.secondary" sx={{ fontSize: 11 }}>
                          {getDisplayFields(dup.existingRecord)}
                        </Typography>
                      </TableCell>

                      {/* Decision buttons */}
                      <TableCell align="center">
                        <ButtonGroup size="small" variant="outlined">
                          <Button
                            onClick={() => onRecordDecision(dup.matchValue, 'skip')}
                            color={decision === 'skip' ? 'inherit' : 'inherit'}
                            variant={decision === 'skip' ? 'contained' : 'outlined'}
                            startIcon={<SkipIcon />}
                            sx={{ fontSize: 11 }}
                          >
                            Skip
                          </Button>
                          <Button
                            onClick={() => onRecordDecision(dup.matchValue, 'overwrite')}
                            color={decision === 'overwrite' ? 'warning' : 'inherit'}
                            variant={decision === 'overwrite' ? 'contained' : 'outlined'}
                            startIcon={<OverwriteIcon />}
                            sx={{ fontSize: 11 }}
                          >
                            Overwrite
                          </Button>
                          <Button
                            onClick={() => onRecordDecision(dup.matchValue, 'merge')}
                            color={decision === 'merge' ? 'info' : 'inherit'}
                            variant={decision === 'merge' ? 'contained' : 'outlined'}
                            startIcon={<MergeIcon />}
                            sx={{ fontSize: 11 }}
                          >
                            Merge
                          </Button>
                        </ButtonGroup>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </TableContainer>

          {summary.pending > 0 && (
            <Alert severity="error" sx={{ mt: 2 }}>
              <Typography variant="body2">
                <strong>{summary.pending}</strong> record
                {summary.pending !== 1 ? 's still need' : ' still needs'} a decision before you
                can continue.
              </Typography>
            </Alert>
          )}
        </>
      )}
    </Box>
  );
}
