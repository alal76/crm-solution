/**
 * CRM Solution - Consistent Dialog Header Component
 * Copyright (C) 2024-2026 Abhishek Lal
 * 
 * Provides a consistent header for all CRUD dialogs across the CRM.
 */

import React from 'react';
import {
  Box,
  Typography,
  IconButton,
  Chip,
  Tooltip,
  Avatar,
  Stack,
  Divider,
} from '@mui/material';
import {
  Close as CloseIcon,
  Edit as EditIcon,
  Add as AddIcon,
  Visibility as ViewIcon,
  Person as PersonIcon,
  Business as BusinessIcon,
  TrendingUp as TrendingUpIcon,
  Assignment as AssignmentIcon,
  Campaign as CampaignIcon,
  ContactPhone as ContactPhoneIcon,
  Receipt as ReceiptIcon,
  Payment as PaymentIcon,
  ShoppingCart as ShoppingCartIcon,
  Group as GroupIcon,
  Subscriptions as SubscriptionsIcon,
  AttachMoney as AttachMoneyIcon,
} from '@mui/icons-material';

export type DialogMode = 'create' | 'edit' | 'view';

export type DialogEntityType = 
  | 'contact'
  | 'lead'
  | 'account'
  | 'opportunity'
  | 'serviceRequest'
  | 'campaign'
  | 'quote'
  | 'contract'
  | 'activity'
  | 'product'
  | 'invoice'
  | 'payment'
  | 'order'
  | 'team'
  | 'user'
  | 'subscription'
  | 'commission';

export interface DialogHeaderProps {
  /** Mode of the dialog */
  mode: DialogMode;
  /** Entity type */
  entityType: DialogEntityType;
  /** Entity name (for edit/view mode) */
  entityName?: string;
  /** Entity ID (for edit/view mode) */
  entityId?: number;
  /** Close handler */
  onClose: () => void;
  /** Custom title override */
  title?: string;
  /** Custom subtitle */
  subtitle?: string;
  /** Show entity metadata (created/modified) */
  showMetadata?: boolean;
  /** Created date */
  createdAt?: string;
  /** Modified date */
  modifiedAt?: string;
  /** Created by user */
  createdBy?: string;
  /** Status badge */
  status?: string;
  /** Status badge color */
  statusColor?: string;
  /** Additional actions in header */
  headerActions?: React.ReactNode;
}

// Entity type configurations
const entityConfigs: Record<DialogEntityType, {
  icon: React.ReactNode;
  color: string;
  singularLabel: string;
}> = {
  contact: {
    icon: <ContactPhoneIcon />,
    color: '#6750A4',
    singularLabel: 'Contact',
  },
  lead: {
    icon: <PersonIcon />,
    color: '#FF9800',
    singularLabel: 'Lead',
  },
  account: {
    icon: <BusinessIcon />,
    color: '#2196F3',
    singularLabel: 'Account',
  },
  opportunity: {
    icon: <TrendingUpIcon />,
    color: '#06A77D',
    singularLabel: 'Opportunity',
  },
  serviceRequest: {
    icon: <AssignmentIcon />,
    color: '#E91E63',
    singularLabel: 'Service Request',
  },
  campaign: {
    icon: <CampaignIcon />,
    color: '#9C27B0',
    singularLabel: 'Campaign',
  },
  quote: {
    icon: <TrendingUpIcon />,
    color: '#4CAF50',
    singularLabel: 'Quote',
  },
  contract: {
    icon: <AssignmentIcon />,
    color: '#795548',
    singularLabel: 'Contract',
  },
  activity: {
    icon: <AssignmentIcon />,
    color: '#00BCD4',
    singularLabel: 'Activity',
  },
  product: {
    icon: <AssignmentIcon />,
    color: '#607D8B',
    singularLabel: 'Product',
  },
  invoice: {
    icon: <ReceiptIcon />,
    color: '#3F51B5',
    singularLabel: 'Invoice',
  },
  payment: {
    icon: <PaymentIcon />,
    color: '#009688',
    singularLabel: 'Payment',
  },
  order: {
    icon: <ShoppingCartIcon />,
    color: '#FF5722',
    singularLabel: 'Order',
  },
  team: {
    icon: <GroupIcon />,
    color: '#673AB7',
    singularLabel: 'Team',
  },
  user: {
    icon: <PersonIcon />,
    color: '#FF9800',
    singularLabel: 'User',
  },
  subscription: {
    icon: <SubscriptionsIcon />,
    color: '#00BCD4',
    singularLabel: 'Subscription',
  },
  commission: {
    icon: <AttachMoneyIcon />,
    color: '#4CAF50',
    singularLabel: 'Commission',
  },
};

// Mode icons - explicitly typed as ReactElement for Chip compatibility
const modeIcons: Record<DialogMode, React.ReactElement> = {
  create: <AddIcon fontSize="small" />,
  edit: <EditIcon fontSize="small" />,
  view: <ViewIcon fontSize="small" />,
};

/**
 * DialogHeader - Consistent header for all CRUD dialogs
 */
export const DialogHeader: React.FC<DialogHeaderProps> = ({
  mode,
  entityType,
  entityName,
  entityId,
  onClose,
  title,
  subtitle,
  showMetadata = false,
  createdAt,
  modifiedAt,
  createdBy,
  status,
  statusColor,
  headerActions,
}) => {
  const config = entityConfigs[entityType];
  
  // Generate default title based on mode and entity type
  const defaultTitle = mode === 'create' 
    ? `New ${config.singularLabel}`
    : mode === 'edit'
    ? `Edit ${config.singularLabel}`
    : `View ${config.singularLabel}`;

  const displayTitle = title || (entityName ? entityName : defaultTitle);

  // Format dates
  const formatDate = (dateStr?: string) => {
    if (!dateStr) return null;
    return new Date(dateStr).toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  return (
    <Box
      sx={{
        px: 3,
        py: 2,
        borderBottom: '1px solid',
        borderColor: 'divider',
        backgroundColor: 'grey.50',
      }}
    >
      <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
        {/* Left side - Icon and Title */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Avatar
            sx={{
              width: 48,
              height: 48,
              backgroundColor: `${config.color}20`,
              color: config.color,
            }}
          >
            {config.icon}
          </Avatar>
          <Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Typography variant="h6" fontWeight={600} sx={{ color: 'text.primary' }}>
                {displayTitle}
              </Typography>
              {mode !== 'create' && (
                <Chip
                  icon={modeIcons[mode]}
                  label={mode.charAt(0).toUpperCase() + mode.slice(1)}
                  size="small"
                  variant="outlined"
                  sx={{ 
                    height: 24,
                    '& .MuiChip-icon': { fontSize: 14 },
                    '& .MuiChip-label': { px: 1, fontSize: '0.75rem' },
                  }}
                />
              )}
              {status && (
                <Chip
                  label={status}
                  size="small"
                  sx={{
                    height: 24,
                    backgroundColor: statusColor ? `${statusColor}20` : undefined,
                    color: statusColor,
                    fontWeight: 600,
                  }}
                />
              )}
            </Box>
            {subtitle && (
              <Typography variant="body2" color="text.secondary">
                {subtitle}
              </Typography>
            )}
            {entityId && mode !== 'create' && (
              <Typography variant="caption" color="text.disabled">
                ID: {entityId}
              </Typography>
            )}
          </Box>
        </Box>

        {/* Right side - Actions and Close */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          {headerActions}
          <Tooltip title="Close">
            <IconButton 
              onClick={onClose}
              size="small"
              sx={{ 
                color: 'text.secondary',
                '&:hover': { backgroundColor: 'action.hover' },
              }}
              aria-label="Close dialog"
            >
              <CloseIcon />
            </IconButton>
          </Tooltip>
        </Box>
      </Box>

      {/* Metadata row */}
      {showMetadata && mode !== 'create' && (createdAt || modifiedAt || createdBy) && (
        <>
          <Divider sx={{ my: 1.5 }} />
          <Stack 
            direction="row" 
            spacing={3} 
            sx={{ 
              color: 'text.secondary',
              '& > *': { display: 'flex', alignItems: 'center', gap: 0.5 },
            }}
          >
            {createdBy && (
              <Typography variant="caption">
                <strong>Created by:</strong> {createdBy}
              </Typography>
            )}
            {createdAt && (
              <Typography variant="caption">
                <strong>Created:</strong> {formatDate(createdAt)}
              </Typography>
            )}
            {modifiedAt && (
              <Typography variant="caption">
                <strong>Modified:</strong> {formatDate(modifiedAt)}
              </Typography>
            )}
          </Stack>
        </>
      )}
    </Box>
  );
};

export default DialogHeader;
