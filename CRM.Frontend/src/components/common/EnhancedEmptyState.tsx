/**
 * CRM Solution - Enhanced Empty State Component
 * Copyright (C) 2024-2026 Abhishek Lal
 * 
 * Provides illustrated empty states for various CRM entities with contextual actions.
 */

import React from 'react';
import {
  Box,
  Typography,
  Button,
  Stack,
} from '@mui/material';
import {
  Person as PersonIcon,
  Business as BusinessIcon,
  TrendingUp as TrendingUpIcon,
  Assignment as AssignmentIcon,
  Campaign as CampaignIcon,
  ContactPhone as ContactPhoneIcon,
  Inbox as InboxIcon,
  Search as SearchIcon,
  FilterList as FilterIcon,
  Add as AddIcon,
  Refresh as RefreshIcon,
  CloudOff as CloudOffIcon,
  Lock as LockIcon,
  Error as ErrorIcon,
} from '@mui/icons-material';

export type EmptyStateVariant = 
  | 'no-data'
  | 'no-results'
  | 'no-access'
  | 'error'
  | 'offline';

export type EntityIllustration = 
  | 'contacts'
  | 'leads'
  | 'accounts'
  | 'opportunities'
  | 'serviceRequests'
  | 'campaigns'
  | 'activities'
  | 'quotes'
  | 'contracts'
  | 'generic';

export interface EnhancedEmptyStateProps {
  /** The type of empty state to display */
  variant?: EmptyStateVariant;
  /** Entity type for illustration selection */
  illustration?: EntityIllustration;
  /** Main title */
  title?: string;
  /** Description text */
  description?: string;
  /** Primary action button label */
  primaryActionLabel?: string;
  /** Primary action callback */
  onPrimaryAction?: () => void;
  /** Secondary action button label */
  secondaryActionLabel?: string;
  /** Secondary action callback */
  onSecondaryAction?: () => void;
  /** Custom icon override */
  icon?: React.ReactNode;
  /** Compact mode for smaller spaces */
  compact?: boolean;
  /** Show search hint for filtered results */
  showSearchHint?: boolean;
  /** Current filter/search value for context */
  searchValue?: string;
}

// Illustration configurations
const illustrations: Record<EntityIllustration, {
  icon: React.ReactNode;
  color: string;
  defaultTitle: string;
  defaultDescription: string;
}> = {
  contacts: {
    icon: <ContactPhoneIcon sx={{ fontSize: 80 }} />,
    color: '#6750A4',
    defaultTitle: 'No contacts yet',
    defaultDescription: 'Start building your network by adding your first contact.',
  },
  leads: {
    icon: <PersonIcon sx={{ fontSize: 80 }} />,
    color: '#FF9800',
    defaultTitle: 'No leads found',
    defaultDescription: 'Capture potential customers by adding leads from various sources.',
  },
  accounts: {
    icon: <BusinessIcon sx={{ fontSize: 80 }} />,
    color: '#2196F3',
    defaultTitle: 'No accounts yet',
    defaultDescription: 'Create accounts to track your customer organizations.',
  },
  opportunities: {
    icon: <TrendingUpIcon sx={{ fontSize: 80 }} />,
    color: '#06A77D',
    defaultTitle: 'No opportunities found',
    defaultDescription: 'Track your sales pipeline by creating opportunities.',
  },
  serviceRequests: {
    icon: <AssignmentIcon sx={{ fontSize: 80 }} />,
    color: '#E91E63',
    defaultTitle: 'No service requests',
    defaultDescription: 'Service requests will appear here when customers need assistance.',
  },
  campaigns: {
    icon: <CampaignIcon sx={{ fontSize: 80 }} />,
    color: '#9C27B0',
    defaultTitle: 'No campaigns yet',
    defaultDescription: 'Launch marketing campaigns to engage with your audience.',
  },
  activities: {
    icon: <AssignmentIcon sx={{ fontSize: 80 }} />,
    color: '#00BCD4',
    defaultTitle: 'No activities scheduled',
    defaultDescription: 'Schedule tasks, meetings, and calls to stay organized.',
  },
  quotes: {
    icon: <TrendingUpIcon sx={{ fontSize: 80 }} />,
    color: '#4CAF50',
    defaultTitle: 'No quotes created',
    defaultDescription: 'Create quotes to provide pricing for your opportunities.',
  },
  contracts: {
    icon: <AssignmentIcon sx={{ fontSize: 80 }} />,
    color: '#795548',
    defaultTitle: 'No contracts found',
    defaultDescription: 'Manage customer contracts and agreements here.',
  },
  generic: {
    icon: <InboxIcon sx={{ fontSize: 80 }} />,
    color: '#9e9e9e',
    defaultTitle: 'No data found',
    defaultDescription: 'There are no items to display.',
  },
};

// Variant configurations
const variantConfigs: Record<EmptyStateVariant, {
  icon: React.ReactNode;
  color: string;
  defaultTitle: string;
  defaultDescription: string;
}> = {
  'no-data': {
    icon: <InboxIcon sx={{ fontSize: 80 }} />,
    color: '#9e9e9e',
    defaultTitle: 'No data found',
    defaultDescription: 'There are no items to display.',
  },
  'no-results': {
    icon: <SearchIcon sx={{ fontSize: 80 }} />,
    color: '#FF9800',
    defaultTitle: 'No results found',
    defaultDescription: 'Try adjusting your search or filter criteria.',
  },
  'no-access': {
    icon: <LockIcon sx={{ fontSize: 80 }} />,
    color: '#f44336',
    defaultTitle: 'Access denied',
    defaultDescription: 'You do not have permission to view this content.',
  },
  'error': {
    icon: <ErrorIcon sx={{ fontSize: 80 }} />,
    color: '#f44336',
    defaultTitle: 'Something went wrong',
    defaultDescription: 'An error occurred while loading the data.',
  },
  'offline': {
    icon: <CloudOffIcon sx={{ fontSize: 80 }} />,
    color: '#795548',
    defaultTitle: 'You are offline',
    defaultDescription: 'Please check your internet connection and try again.',
  },
};

/**
 * EnhancedEmptyState - Illustrated empty state component for CRM entities
 */
export const EnhancedEmptyState: React.FC<EnhancedEmptyStateProps> = ({
  variant = 'no-data',
  illustration = 'generic',
  title,
  description,
  primaryActionLabel,
  onPrimaryAction,
  secondaryActionLabel,
  onSecondaryAction,
  icon,
  compact = false,
  showSearchHint = false,
  searchValue,
}) => {
  // Determine configuration based on variant and illustration
  const useIllustration = variant === 'no-data' && illustration !== 'generic';
  const config = useIllustration ? illustrations[illustration] : variantConfigs[variant];

  const displayIcon = icon || React.cloneElement(config.icon as React.ReactElement, {
    sx: { 
      fontSize: compact ? 48 : 80, 
      color: config.color,
      opacity: 0.6,
    }
  });

  const displayTitle = title || config.defaultTitle;
  const displayDescription = description || config.defaultDescription;

  // Determine default action label based on variant and illustration
  const defaultPrimaryLabel = variant === 'no-data' 
    ? `Add ${illustration === 'generic' ? 'Item' : illustration.slice(0, -1).replace(/^./, c => c.toUpperCase())}`
    : variant === 'no-results'
    ? 'Clear Filters'
    : variant === 'error' || variant === 'offline'
    ? 'Retry'
    : undefined;

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        py: compact ? 4 : 8,
        px: 3,
        textAlign: 'center',
      }}
    >
      {/* Illustration Circle */}
      <Box
        sx={{
          width: compact ? 80 : 120,
          height: compact ? 80 : 120,
          borderRadius: '50%',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          backgroundColor: `${config.color}10`,
          mb: compact ? 2 : 3,
        }}
      >
        {displayIcon}
      </Box>

      {/* Title */}
      <Typography 
        variant={compact ? 'h6' : 'h5'} 
        fontWeight={600}
        color="text.primary"
        gutterBottom
      >
        {displayTitle}
      </Typography>

      {/* Description */}
      <Typography 
        variant={compact ? 'body2' : 'body1'} 
        color="text.secondary"
        sx={{ 
          maxWidth: 400, 
          mb: compact ? 2 : 3,
          lineHeight: 1.6,
        }}
      >
        {displayDescription}
      </Typography>

      {/* Search Hint */}
      {showSearchHint && searchValue && (
        <Box 
          sx={{ 
            display: 'flex', 
            alignItems: 'center', 
            gap: 1, 
            mb: 2,
            px: 2,
            py: 1,
            backgroundColor: 'warning.light',
            borderRadius: 2,
          }}
        >
          <FilterIcon fontSize="small" color="warning" />
          <Typography variant="caption" color="warning.dark">
            Filtered by: "{searchValue}"
          </Typography>
        </Box>
      )}

      {/* Actions */}
      <Stack 
        direction={compact ? 'column' : 'row'} 
        spacing={compact ? 1 : 2}
        sx={{ mt: 1 }}
      >
        {(primaryActionLabel || defaultPrimaryLabel) && onPrimaryAction && (
          <Button
            variant="contained"
            color="primary"
            startIcon={
              variant === 'no-data' ? <AddIcon /> : 
              variant === 'error' || variant === 'offline' ? <RefreshIcon /> :
              <FilterIcon />
            }
            onClick={onPrimaryAction}
            size={compact ? 'small' : 'medium'}
            sx={{
              backgroundColor: config.color,
              '&:hover': {
                backgroundColor: config.color,
                filter: 'brightness(0.9)',
              },
            }}
          >
            {primaryActionLabel || defaultPrimaryLabel}
          </Button>
        )}
        {secondaryActionLabel && onSecondaryAction && (
          <Button
            variant="outlined"
            onClick={onSecondaryAction}
            size={compact ? 'small' : 'medium'}
          >
            {secondaryActionLabel}
          </Button>
        )}
      </Stack>
    </Box>
  );
};

export default EnhancedEmptyState;
