import React from 'react';
import { Chip } from '@mui/material';
import {
  CheckCircle as CheckCircleIcon,
  Warning as WarningIcon,
  Send as SendIcon,
  Visibility as VisibilityIcon,
  Cancel as CancelIcon,
  Block as BlockIcon,
  Edit as EditIcon,
  HourglassEmpty as HourglassEmptyIcon,
} from '@mui/icons-material';

/**
 * Invoice Status enum matching backend
 */
export enum InvoiceStatus {
  Draft = 0,
  PendingApproval = 1,
  Approved = 2,
  Sent = 3,
  Viewed = 4,
  PartiallyPaid = 5,
  Paid = 6,
  Overdue = 7,
  Disputed = 8,
  Voided = 9,
  WrittenOff = 10,
  Collections = 11,
  Refunded = 12,
}

type ChipColor = 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning';

interface StatusConfig {
  label: string;
  color: ChipColor;
  icon: React.ReactElement;
}

const STATUS_CONFIG: Record<InvoiceStatus, StatusConfig> = {
  [InvoiceStatus.Draft]: {
    label: 'Draft',
    color: 'default',
    icon: <EditIcon fontSize="small" />,
  },
  [InvoiceStatus.PendingApproval]: {
    label: 'Pending Approval',
    color: 'warning',
    icon: <HourglassEmptyIcon fontSize="small" />,
  },
  [InvoiceStatus.Approved]: {
    label: 'Approved',
    color: 'info',
    icon: <CheckCircleIcon fontSize="small" />,
  },
  [InvoiceStatus.Sent]: {
    label: 'Sent',
    color: 'info',
    icon: <SendIcon fontSize="small" />,
  },
  [InvoiceStatus.Viewed]: {
    label: 'Viewed',
    color: 'info',
    icon: <VisibilityIcon fontSize="small" />,
  },
  [InvoiceStatus.PartiallyPaid]: {
    label: 'Partially Paid',
    color: 'warning',
    icon: <WarningIcon fontSize="small" />,
  },
  [InvoiceStatus.Paid]: {
    label: 'Paid',
    color: 'success',
    icon: <CheckCircleIcon fontSize="small" />,
  },
  [InvoiceStatus.Overdue]: {
    label: 'Overdue',
    color: 'error',
    icon: <WarningIcon fontSize="small" />,
  },
  [InvoiceStatus.Disputed]: {
    label: 'Disputed',
    color: 'error',
    icon: <WarningIcon fontSize="small" />,
  },
  [InvoiceStatus.Voided]: {
    label: 'Voided',
    color: 'default',
    icon: <BlockIcon fontSize="small" />,
  },
  [InvoiceStatus.WrittenOff]: {
    label: 'Written Off',
    color: 'default',
    icon: <CancelIcon fontSize="small" />,
  },
  [InvoiceStatus.Collections]: {
    label: 'Collections',
    color: 'warning',
    icon: <WarningIcon fontSize="small" />,
  },
  [InvoiceStatus.Refunded]: {
    label: 'Refunded',
    color: 'secondary',
    icon: <CheckCircleIcon fontSize="small" />,
  },
};

export interface InvoiceStatusBadgeProps {
  status: InvoiceStatus;
  size?: 'small' | 'medium';
  showIcon?: boolean;
}

/**
 * InvoiceStatusBadge - Reusable status badge for invoices
 * 
 * Features:
 * - Color-coded Chip component
 * - Icon per status
 * - Consistent styling across app
 */
export const InvoiceStatusBadge: React.FC<InvoiceStatusBadgeProps> = ({
  status,
  size = 'small',
  showIcon = true,
}) => {
  const config = STATUS_CONFIG[status] || {
    label: 'Unknown',
    color: 'default' as ChipColor,
    icon: <BlockIcon fontSize="small" />,
  };

  return (
    <Chip
      label={config.label}
      size={size}
      color={config.color}
      icon={showIcon ? config.icon : undefined}
      sx={{
        fontWeight: 500,
        ...(size === 'medium' && { fontSize: '0.875rem', height: 32 }),
      }}
    />
  );
};

export default InvoiceStatusBadge;
