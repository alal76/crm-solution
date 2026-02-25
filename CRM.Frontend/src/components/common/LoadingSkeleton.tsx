/**
 * LoadingSkeleton - Table / list loading placeholder.
 *
 * TODO-SYS008-004: LoadingSkeleton for list pages
 *
 * Renders animated MUI Skeleton rows that mimic a data table layout,
 * giving users a clear visual cue that content is being fetched.
 *
 * Usage:
 *   <LoadingSkeleton rows={8} columns={5} />
 *   <LoadingSkeleton variant="card" count={3} />
 */

import React from 'react';
import {
  Box,
  Skeleton,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Paper,
  Card,
  CardContent,
  Grid,
} from '@mui/material';

// --------------------------------------------------------------------------
// Table skeleton
// --------------------------------------------------------------------------

export interface TableSkeletonProps {
  /** Number of data rows to render (default: 5) */
  rows?: number;
  /** Number of columns to render (default: 4) */
  columns?: number;
  /** Show a header row (default: true) */
  showHeader?: boolean;
  /** Whether to wrap in a Paper/card (default: true) */
  elevated?: boolean;
  /** aria-label for the skeleton table */
  ariaLabel?: string;
}

export const TableSkeleton: React.FC<TableSkeletonProps> = ({
  rows = 5,
  columns = 4,
  showHeader = true,
  elevated = true,
  ariaLabel = 'Loading data…',
}) => {
  const table = (
    <Table size="small" aria-label={ariaLabel} aria-busy="true">
      {showHeader && (
        <TableHead>
          <TableRow>
            {/* Checkbox column */}
            <TableCell padding="checkbox" sx={{ width: 48 }}>
              <Skeleton variant="rectangular" width={20} height={20} />
            </TableCell>
            {Array.from({ length: columns }).map((_, ci) => (
              <TableCell key={ci}>
                <Skeleton
                  variant="text"
                  width={`${60 + (ci % 3) * 20}%`}
                  height={20}
                />
              </TableCell>
            ))}
          </TableRow>
        </TableHead>
      )}
      <TableBody>
        {Array.from({ length: rows }).map((_, ri) => (
          <TableRow key={ri} sx={{ '&:last-child td': { borderBottom: 0 } }}>
            <TableCell padding="checkbox">
              <Skeleton variant="rectangular" width={20} height={20} />
            </TableCell>
            {Array.from({ length: columns }).map((_, ci) => (
              <TableCell key={ci}>
                <Skeleton
                  variant="text"
                  width={`${40 + ((ri + ci) % 4) * 15}%`}
                  height={22}
                />
              </TableCell>
            ))}
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );

  if (elevated) {
    return (
      <Paper
        variant="outlined"
        sx={{ overflow: 'hidden' }}
        aria-label={ariaLabel}
        aria-busy="true"
      >
        {table}
      </Paper>
    );
  }

  return <Box aria-label={ariaLabel} aria-busy="true">{table}</Box>;
};

// --------------------------------------------------------------------------
// Card skeleton
// --------------------------------------------------------------------------

export interface CardSkeletonProps {
  /** Number of cards to render (default: 3) */
  count?: number;
  /** MUI Grid xs column span (default: 12 = full width) */
  xs?: number;
  /** MUI Grid sm column span */
  sm?: number;
  /** MUI Grid md column span */
  md?: number;
  ariaLabel?: string;
}

export const CardSkeleton: React.FC<CardSkeletonProps> = ({
  count = 3,
  xs = 12,
  sm,
  md,
  ariaLabel = 'Loading cards…',
}) => (
  <Grid container spacing={2} aria-label={ariaLabel} aria-busy="true">
    {Array.from({ length: count }).map((_, i) => (
      <Grid item xs={xs} sm={sm} md={md} key={i}>
        <Card variant="outlined">
          <CardContent>
            <Skeleton variant="text" width="60%" height={28} />
            <Skeleton variant="text" width="40%" height={20} sx={{ mt: 1 }} />
            <Skeleton variant="rectangular" height={80} sx={{ mt: 2, borderRadius: 1 }} />
            <Box sx={{ display: 'flex', gap: 1, mt: 1.5 }}>
              <Skeleton variant="rounded" width={72} height={28} />
              <Skeleton variant="rounded" width={72} height={28} />
            </Box>
          </CardContent>
        </Card>
      </Grid>
    ))}
  </Grid>
);

// --------------------------------------------------------------------------
// List-row skeleton (single-column dense)
// --------------------------------------------------------------------------

export interface ListSkeletonProps {
  rows?: number;
  ariaLabel?: string;
}

export const ListSkeleton: React.FC<ListSkeletonProps> = ({
  rows = 8,
  ariaLabel = 'Loading list…',
}) => (
  <Box aria-label={ariaLabel} aria-busy="true">
    {Array.from({ length: rows }).map((_, i) => (
      <Box
        key={i}
        sx={{
          display: 'flex',
          alignItems: 'center',
          gap: 2,
          py: 1,
          px: 2,
          borderBottom: 1,
          borderColor: 'divider',
        }}
      >
        <Skeleton variant="circular" width={36} height={36} />
        <Box sx={{ flex: 1 }}>
          <Skeleton variant="text" width={`${50 + (i % 3) * 15}%`} height={22} />
          <Skeleton variant="text" width={`${30 + (i % 4) * 10}%`} height={18} />
        </Box>
        <Skeleton variant="rounded" width={64} height={24} />
      </Box>
    ))}
  </Box>
);

// --------------------------------------------------------------------------
// Primary export — unified LoadingSkeleton
// --------------------------------------------------------------------------

export interface LoadingSkeletonProps {
  /** Layout variant (default: 'table') */
  variant?: 'table' | 'card' | 'list';
  /** Rows / items to render */
  rows?: number;
  /** Columns – only relevant for variant='table' */
  columns?: number;
  /** Whether to show a header – only relevant for variant='table' */
  showHeader?: boolean;
  /** Wrap in Paper elevation – only relevant for variant='table' */
  elevated?: boolean;
  ariaLabel?: string;
}

export const LoadingSkeleton: React.FC<LoadingSkeletonProps> = ({
  variant = 'table',
  rows = 8,
  columns = 4,
  showHeader = true,
  elevated = true,
  ariaLabel,
}) => {
  switch (variant) {
    case 'card':
      return <CardSkeleton count={rows} ariaLabel={ariaLabel} />;
    case 'list':
      return <ListSkeleton rows={rows} ariaLabel={ariaLabel} />;
    default:
      return (
        <TableSkeleton
          rows={rows}
          columns={columns}
          showHeader={showHeader}
          elevated={elevated}
          ariaLabel={ariaLabel}
        />
      );
  }
};

export default LoadingSkeleton;
