/**
 * EmptyState — lightweight, composable empty-state component.
 *
 * TODO-SYS008-004: EmptyState for list pages
 *
 * Complements the more feature-rich `EnhancedEmptyState` (entity-specific
 * illustrations) with a simpler building block that accepts any icon, title,
 * description, and an optional primary action button.
 *
 * Usage:
 *   <EmptyState
 *     icon={<InboxIcon sx={{ fontSize: 64 }} />}
 *     title="No accounts yet"
 *     description="Create your first account to get started."
 *     actionLabel="Create Account"
 *     onAction={() => navigate('/accounts/new')}
 *   />
 */

import React from 'react';
import { Box, Typography, Button, Stack } from '@mui/material';
import type { SxProps, Theme } from '@mui/material';

// --------------------------------------------------------------------------
// Types
// --------------------------------------------------------------------------

export interface EmptyStateAction {
  label: string;
  onClick: () => void;
  variant?: 'contained' | 'outlined' | 'text';
  color?: 'primary' | 'secondary' | 'inherit';
  startIcon?: React.ReactNode;
}

export interface EmptyStateProps {
  /** Optional illustration or icon element */
  icon?: React.ReactNode;
  /** Primary heading */
  title: string;
  /** Supporting body text beneath the title */
  description?: string;
  /** Shorthand for a single primary action button */
  actionLabel?: string;
  /** Handler for the primary action button */
  onAction?: () => void;
  /** Full list of actions (ignored when `actionLabel`/`onAction` is used) */
  actions?: EmptyStateAction[];
  /** Additional sx overrides for the root container */
  sx?: SxProps<Theme>;
  /** Accessibility label for the empty state region */
  ariaLabel?: string;
}

// --------------------------------------------------------------------------
// Component
// --------------------------------------------------------------------------

export const EmptyState: React.FC<EmptyStateProps> = ({
  icon,
  title,
  description,
  actionLabel,
  onAction,
  actions,
  sx,
  ariaLabel,
}) => {
  // Normalise actions from shorthand props
  const resolvedActions: EmptyStateAction[] =
    actionLabel && onAction
      ? [{ label: actionLabel, onClick: onAction, variant: 'contained', color: 'primary' }]
      : actions ?? [];

  return (
    <Box
      role="status"
      aria-label={ariaLabel ?? title}
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        textAlign: 'center',
        py: 8,
        px: 3,
        gap: 2,
        ...sx,
      }}
    >
      {icon && (
        <Box
          aria-hidden="true"
          sx={{
            color: 'text.disabled',
            mb: 1,
            '& .MuiSvgIcon-root': { fontSize: 72 },
          }}
        >
          {icon}
        </Box>
      )}

      <Typography variant="h6" color="text.primary" fontWeight={600}>
        {title}
      </Typography>

      {description && (
        <Typography
          variant="body2"
          color="text.secondary"
          sx={{ maxWidth: 420, lineHeight: 1.6 }}
        >
          {description}
        </Typography>
      )}

      {resolvedActions.length > 0 && (
        <Stack direction="row" spacing={1.5} mt={1} flexWrap="wrap" justifyContent="center">
          {resolvedActions.map((action) => (
            <Button
              key={action.label}
              variant={action.variant ?? 'contained'}
              color={action.color ?? 'primary'}
              startIcon={action.startIcon}
              onClick={action.onClick}
            >
              {action.label}
            </Button>
          ))}
        </Stack>
      )}
    </Box>
  );
};

export default EmptyState;
