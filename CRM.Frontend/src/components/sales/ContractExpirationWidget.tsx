/**
 * ContractExpirationWidget - Dashboard widget showing contracts expiring soon,
 * with color-coded urgency and total value at risk.
 */

import React from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  List,
  ListItemButton,
  ListItemText,
  Chip,
  CircularProgress,
  Divider,
} from '@mui/material';
import { Warning as WarningIcon } from '@mui/icons-material';

export interface ExpiringContract {
  id: number;
  name: string;
  accountName: string;
  expiresAt: string;
  value: number;
  daysUntilExpiry: number;
}

interface ContractExpirationWidgetProps {
  contracts: ExpiringContract[];
  onContractClick: (id: number) => void;
  daysThreshold?: number;
  loading?: boolean;
}

const urgencyColor = (days: number): 'error' | 'warning' | 'success' =>
  days < 7 ? 'error' : days < 30 ? 'warning' : 'success';

const formatCurrency = (value: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(value);

const ContractExpirationWidget: React.FC<ContractExpirationWidgetProps> = ({
  contracts,
  onContractClick,
  daysThreshold = 30,
  loading = false,
}) => {
  const filtered = contracts.filter((c) => c.daysUntilExpiry <= daysThreshold);
  const totalValueAtRisk = filtered.reduce((sum, c) => sum + c.value, 0);

  return (
    <Card variant="outlined">
      <CardContent sx={{ pb: 1 }}>
        <Box display="flex" alignItems="center" justifyContent="space-between" mb={1}>
          <Box display="flex" alignItems="center" gap={1}>
            <WarningIcon color="warning" fontSize="small" />
            <Typography variant="subtitle1" fontWeight={600}>
              Expiring Contracts
            </Typography>
          </Box>
          <Chip
            label={`${filtered.length} contract${filtered.length !== 1 ? 's' : ''}`}
            size="small"
            color="warning"
            variant="outlined"
          />
        </Box>

        {loading ? (
          <Box display="flex" justifyContent="center" py={3}>
            <CircularProgress size={28} />
          </Box>
        ) : filtered.length === 0 ? (
          <Typography variant="body2" color="text.secondary" py={2} textAlign="center">
            No contracts expiring within {daysThreshold} days
          </Typography>
        ) : (
          <>
            <List dense disablePadding>
              {filtered.map((contract) => (
                <ListItemButton
                  key={contract.id}
                  onClick={() => onContractClick(contract.id)}
                  sx={{ borderRadius: 1, mb: 0.5 }}
                >
                  <ListItemText
                    primary={contract.name}
                    secondary={`${contract.accountName} · ${formatCurrency(contract.value)}`}
                    primaryTypographyProps={{ variant: 'body2', fontWeight: 500 }}
                    secondaryTypographyProps={{ variant: 'caption' }}
                  />
                  <Chip
                    label={
                      contract.daysUntilExpiry <= 0
                        ? 'Expired'
                        : `${contract.daysUntilExpiry}d`
                    }
                    size="small"
                    color={urgencyColor(contract.daysUntilExpiry)}
                    sx={{ minWidth: 48 }}
                  />
                </ListItemButton>
              ))}
            </List>

            <Divider sx={{ my: 1 }} />
            <Box display="flex" justifyContent="space-between" alignItems="center">
              <Typography variant="caption" color="text.secondary">
                Total value at risk
              </Typography>
              <Typography variant="subtitle2" fontWeight={700} color="error.main">
                {formatCurrency(totalValueAtRisk)}
              </Typography>
            </Box>
          </>
        )}
      </CardContent>
    </Card>
  );
};

export default ContractExpirationWidget;
