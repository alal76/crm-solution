/**
 * ServiceRequestStats - Dashboard stats component for service request metrics
 * Shows Open tickets, In Progress, Avg Response Time, and SLA Compliance
 */

import React from 'react';
import {
  Grid,
  Card,
  CardContent,
  Typography,
  Box,
  Skeleton,
} from '@mui/material';
import {
  ConfirmationNumber as TicketIcon,
  Engineering as InProgressIcon,
  Speed as ResponseIcon,
  VerifiedUser as SLAIcon,
} from '@mui/icons-material';

export interface ServiceRequestStatsProps {
  stats?: {
    total: number;
    open: number;
    inProgress: number;
    resolved: number;
    avgResponseTime?: number;
    avgResolutionTime?: number;
    slaComplianceRate?: number;
  };
  loading?: boolean;
}

interface StatCardConfig {
  title: string;
  getValue: (stats: NonNullable<ServiceRequestStatsProps['stats']>) => string;
  icon: React.ReactElement;
  bgColor: string;
  iconColor: string;
}

const formatMinutes = (minutes: number | undefined): string => {
  if (minutes === undefined || minutes === null) return 'N/A';
  if (minutes < 60) return `${Math.round(minutes)}m`;
  const hours = Math.floor(minutes / 60);
  const mins = Math.round(minutes % 60);
  if (hours < 24) return `${hours}h ${mins}m`;
  const days = Math.floor(hours / 24);
  return `${days}d ${hours % 24}h`;
};

const statCards: StatCardConfig[] = [
  {
    title: 'Open Tickets',
    getValue: (stats) => String(stats.open),
    icon: <TicketIcon />,
    bgColor: '#e3f2fd',
    iconColor: '#1565c0',
  },
  {
    title: 'In Progress',
    getValue: (stats) => String(stats.inProgress),
    icon: <InProgressIcon />,
    bgColor: '#fff8e1',
    iconColor: '#f57f17',
  },
  {
    title: 'Avg Response',
    getValue: (stats) => formatMinutes(stats.avgResponseTime),
    icon: <ResponseIcon />,
    bgColor: '#e0f7fa',
    iconColor: '#00838f',
  },
  {
    title: 'SLA Compliance',
    getValue: (stats) =>
      stats.slaComplianceRate !== undefined ? `${Math.round(stats.slaComplianceRate)}%` : 'N/A',
    icon: <SLAIcon />,
    bgColor: '#e8f5e9',
    iconColor: '#2e7d32',
  },
];

const getSlaColor = (rate: number | undefined): string => {
  if (rate === undefined) return '#2e7d32';
  if (rate >= 90) return '#2e7d32'; // green
  if (rate >= 75) return '#f57f17'; // amber
  return '#c62828'; // red
};

const ServiceRequestStats: React.FC<ServiceRequestStatsProps> = ({
  stats,
  loading = false,
}) => {
  return (
    <Grid container spacing={2}>
      {statCards.map((card, index) => (
        <Grid item xs={12} sm={6} md={3} key={card.title}>
          <Card elevation={1}>
            <CardContent>
              {loading ? (
                <Box>
                  <Skeleton width="60%" height={20} />
                  <Skeleton width="40%" height={40} sx={{ mt: 1 }} />
                </Box>
              ) : (
                <Box display="flex" justifyContent="space-between" alignItems="flex-start">
                  <Box>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      {card.title}
                    </Typography>
                    <Typography
                      variant="h4"
                      fontWeight={700}
                      sx={{
                        color:
                          index === 3 && stats
                            ? getSlaColor(stats.slaComplianceRate)
                            : 'text.primary',
                      }}
                    >
                      {stats ? card.getValue(stats) : '—'}
                    </Typography>
                  </Box>
                  <Box
                    sx={{
                      borderRadius: 2,
                      p: 1,
                      bgcolor: card.bgColor,
                      color: card.iconColor,
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                    }}
                  >
                    {card.icon}
                  </Box>
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>
      ))}
    </Grid>
  );
};

export default ServiceRequestStats;
