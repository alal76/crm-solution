/**
 * SubscriptionCard - Displays a subscription summary card with status, amount, and billing info
 */
import React from 'react';
import {
  Card,
  CardContent,
  CardActionArea,
  Typography,
  Chip,
  Box,
  Stack,
} from '@mui/material';
import {
  CalendarToday as CalendarIcon,
  AttachMoney as MoneyIcon,
} from '@mui/icons-material';

interface SubscriptionCardProps {
  id: number;
  planName: string;
  status: string;
  amount: number;
  currency?: string;
  billingCycle: string;
  nextBillingDate?: string;
  onClick?: () => void;
}

const statusChipColor = (status: string): 'success' | 'info' | 'error' | 'warning' | 'default' => {
  const lower = status.toLowerCase();
  if (lower === 'active') return 'success';
  if (lower === 'trial') return 'info';
  if (lower === 'cancelled' || lower === 'suspended') return 'error';
  if (lower === 'paused' || lower === 'pending cancellation') return 'warning';
  return 'default';
};

const formatCurrency = (amount: number, currency: string): string => {
  try {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(amount);
  } catch {
    return `${currency} ${amount.toFixed(2)}`;
  }
};

const SubscriptionCard: React.FC<SubscriptionCardProps> = ({
  planName,
  status,
  amount,
  currency = 'USD',
  billingCycle,
  nextBillingDate,
  onClick,
}) => {
  const content = (
    <CardContent>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
        <Typography variant="h6" component="div" noWrap sx={{ maxWidth: '60%' }}>
          {planName}
        </Typography>
        <Chip
          label={status}
          size="small"
          color={statusChipColor(status)}
          sx={{ fontWeight: 600, textTransform: 'capitalize' }}
        />
      </Box>

      <Stack spacing={1.5}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <MoneyIcon fontSize="small" color="action" />
          <Typography variant="body1" fontWeight={600}>
            {formatCurrency(amount, currency)}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            / {billingCycle}
          </Typography>
        </Box>

        {nextBillingDate && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <CalendarIcon fontSize="small" color="action" />
            <Typography variant="body2" color="text.secondary">
              Next billing: {new Date(nextBillingDate).toLocaleDateString()}
            </Typography>
          </Box>
        )}
      </Stack>
    </CardContent>
  );

  return (
    <Card variant="outlined" sx={{ height: '100%' }}>
      {onClick ? (
        <CardActionArea onClick={onClick} sx={{ height: '100%' }}>
          {content}
        </CardActionArea>
      ) : (
        content
      )}
    </Card>
  );
};

export default SubscriptionCard;
