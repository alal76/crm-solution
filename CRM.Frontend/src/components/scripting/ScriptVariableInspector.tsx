/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * ScriptVariableInspector — shows available variables/context that a script
 * can access. Clicking a variable row/chip calls onVariableClick(name),
 * allowing the caller to insert the name into an editor.
 */

import React, { useCallback } from 'react';
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Box,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Tooltip,
  Typography,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';

// ---------------------------------------------------------------------------
// Props
// ---------------------------------------------------------------------------

export interface ScriptVariableInspectorProps {
  /** Variables the script can read, e.g. { account: {...}, contactId: 42 } */
  variables: Record<string, unknown>;
  /** Context fields the script can read, e.g. { userId: 1, tenantId: 2 } */
  context?: Record<string, unknown>;
  /** Called when user clicks a variable row/chip — use to insert name in editor */
  onVariableClick?: (name: string) => void;
  /**
   * compact=true renders a flat horizontal Chip list instead of a table.
   * Default false.
   */
  compact?: boolean;
}

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

/** Returns a human-readable type label for any value. */
function getType(value: unknown): string {
  if (value === null) return 'null';
  if (value === undefined) return 'undefined';
  if (Array.isArray(value)) return `array[${(value as unknown[]).length}]`;
  return typeof value;
}

/** Truncates value preview to 40 characters. */
function getPreview(value: unknown): string {
  const raw = JSON.stringify(value);
  if (raw === undefined) return 'undefined';
  return raw.length > 40 ? `${raw.slice(0, 37)}…` : raw;
}

// ---------------------------------------------------------------------------
// Sub-components
// ---------------------------------------------------------------------------

interface VariableTableProps {
  entries: [string, unknown][];
  onVariableClick?: (name: string) => void;
  namePrefix?: string;
}

const VariableTable: React.FC<VariableTableProps> = ({ entries, onVariableClick, namePrefix }) => {
  if (entries.length === 0) {
    return (
      <Typography variant="body2" color="text.secondary" sx={{ px: 1, py: 0.5, fontStyle: 'italic' }}>
        (empty)
      </Typography>
    );
  }

  return (
    <Table size="small" sx={{ tableLayout: 'fixed' }}>
      <TableHead>
        <TableRow>
          <TableCell sx={{ width: '35%', fontWeight: 600, py: 0.5 }}>Name</TableCell>
          <TableCell sx={{ width: '20%', fontWeight: 600, py: 0.5 }}>Type</TableCell>
          <TableCell sx={{ fontWeight: 600, py: 0.5 }}>Preview</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {entries.map(([name, value]) => {
          const displayName = namePrefix ? `${namePrefix}${name}` : name;
          return (
            <TableRow
              key={displayName}
              hover={!!onVariableClick}
              onClick={() => onVariableClick?.(displayName)}
              sx={{
                cursor: onVariableClick ? 'pointer' : 'default',
                '&:last-child td': { borderBottom: 0 },
              }}
            >
              <TableCell sx={{ py: 0.5 }}>
                <Typography
                  variant="body2"
                  component="span"
                  sx={{
                    fontFamily: 'monospace',
                    fontSize: 12,
                    color: 'primary.main',
                    fontWeight: 500,
                  }}
                >
                  {displayName}
                </Typography>
              </TableCell>
              <TableCell sx={{ py: 0.5 }}>
                <Chip
                  label={getType(value)}
                  size="small"
                  variant="outlined"
                  sx={{ fontSize: 10, height: 20 }}
                />
              </TableCell>
              <TableCell sx={{ py: 0.5 }}>
                <Tooltip title={JSON.stringify(value)} arrow placement="top">
                  <Typography
                    variant="body2"
                    component="span"
                    sx={{ fontFamily: 'monospace', fontSize: 11, color: 'text.secondary' }}
                  >
                    {getPreview(value)}
                  </Typography>
                </Tooltip>
              </TableCell>
            </TableRow>
          );
        })}
      </TableBody>
    </Table>
  );
};

interface CompactChipListProps {
  entries: [string, unknown][];
  onVariableClick?: (name: string) => void;
  namePrefix?: string;
}

const CompactChipList: React.FC<CompactChipListProps> = ({
  entries,
  onVariableClick,
  namePrefix,
}) => (
  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
    {entries.map(([name, value]) => {
      const displayName = namePrefix ? `${namePrefix}${name}` : name;
      const label = `${displayName}: ${getType(value)}`;
      return (
        <Tooltip key={displayName} title={getPreview(value)} arrow placement="top">
          <Chip
            label={label}
            size="small"
            variant="outlined"
            onClick={onVariableClick ? () => onVariableClick(displayName) : undefined}
            sx={{
              fontFamily: 'monospace',
              fontSize: 11,
              cursor: onVariableClick ? 'pointer' : 'default',
            }}
          />
        </Tooltip>
      );
    })}
    {entries.length === 0 && (
      <Typography variant="body2" color="text.secondary" sx={{ fontStyle: 'italic' }}>
        (empty)
      </Typography>
    )}
  </Box>
);

// ---------------------------------------------------------------------------
// Main component
// ---------------------------------------------------------------------------

const ScriptVariableInspector: React.FC<ScriptVariableInspectorProps> = ({
  variables,
  context,
  onVariableClick,
  compact = false,
}) => {
  const varEntries = Object.entries(variables);
  const ctxEntries = context ? Object.entries(context) : [];
  const isEmpty = varEntries.length === 0 && ctxEntries.length === 0;

  const handleVarClick = useCallback(
    (name: string) => onVariableClick?.(name),
    [onVariableClick],
  );

  // ------------------------------------------------------------------
  // Empty state
  // ------------------------------------------------------------------
  if (isEmpty) {
    return (
      <Box sx={{ py: 2, textAlign: 'center' }}>
        <Typography variant="body2" color="text.secondary">
          No variables available
        </Typography>
      </Box>
    );
  }

  // ------------------------------------------------------------------
  // Compact: flat chip lists
  // ------------------------------------------------------------------
  if (compact) {
    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
        {varEntries.length > 0 && (
          <CompactChipList entries={varEntries} onVariableClick={handleVarClick} />
        )}
        {ctxEntries.length > 0 && (
          <>
            <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5 }}>
              context.*
            </Typography>
            <CompactChipList
              entries={ctxEntries}
              onVariableClick={handleVarClick}
              namePrefix="context."
            />
          </>
        )}
      </Box>
    );
  }

  // ------------------------------------------------------------------
  // Full: accordions + tables
  // ------------------------------------------------------------------
  return (
    <Box>
      {/* Variables accordion */}
      <Accordion defaultExpanded disableGutters elevation={0} sx={{ border: '1px solid', borderColor: 'divider' }}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography variant="body2" fontWeight={600}>
            Variables
            {varEntries.length > 0 && (
              <Chip
                label={varEntries.length}
                size="small"
                sx={{ ml: 1, height: 18, fontSize: 10 }}
              />
            )}
          </Typography>
        </AccordionSummary>
        <AccordionDetails sx={{ p: 0 }}>
          <VariableTable entries={varEntries} onVariableClick={handleVarClick} />
        </AccordionDetails>
      </Accordion>

      {/* Context accordion (only shown when context provided) */}
      {context && (
        <Accordion
          defaultExpanded
          disableGutters
          elevation={0}
          sx={{ border: '1px solid', borderColor: 'divider', borderTop: 'none' }}
        >
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography variant="body2" fontWeight={600}>
              Context Fields
              {ctxEntries.length > 0 && (
                <Chip
                  label={ctxEntries.length}
                  size="small"
                  sx={{ ml: 1, height: 18, fontSize: 10 }}
                />
              )}
            </Typography>
            <Typography variant="caption" color="text.secondary" sx={{ ml: 1, alignSelf: 'center' }}>
              (access as context.*)
            </Typography>
          </AccordionSummary>
          <AccordionDetails sx={{ p: 0 }}>
            <VariableTable entries={ctxEntries} onVariableClick={handleVarClick} namePrefix="context." />
          </AccordionDetails>
        </Accordion>
      )}
    </Box>
  );
};

export default ScriptVariableInspector;
