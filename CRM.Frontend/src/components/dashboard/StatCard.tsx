/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This software is source-available. Non-commercial use is permitted under
 * the terms of the LICENSE file. Commercial use requires a separate license.
 * See the LICENSE file in the root directory for full terms.
 */

import { Card, CardContent, Typography, Box, Skeleton } from '@mui/material';
import {
  TrendingUp as TrendingUpIcon,
  TrendingDown as TrendingDownIcon,
} from '@mui/icons-material';

const CARD_GRADIENT = 'linear-gradient(135deg, #F5EFF7 0%, #FFFBFE 100%)';

export interface StatCardProps {
  title: string;
  value: string | number;
  icon?: React.ElementType;
  color: string;
  loading?: boolean;
  onClick?: () => void;
  clickable?: boolean;
  trend?: number;
  subtitle?: string;
}

const StatCard = ({ title, value, icon: Icon, color, loading, onClick, clickable, trend, subtitle }: StatCardProps) => (
  <Card
    onClick={clickable ? onClick : undefined}
    sx={{
      height: '100%',
      borderRadius: 3,
      background: CARD_GRADIENT,
      border: `2px solid ${color}20`,
      cursor: clickable ? 'pointer' : 'default',
      transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
      '&:hover': {
        transform: clickable ? 'translateY(-4px)' : 'none',
        boxShadow: clickable ? `0px 12px 24px ${color}20` : 1,
        border: `2px solid ${color}40`,
      },
    }}
  >
    <CardContent sx={{ position: 'relative', py: 3 }}>
      <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
        <Box>
          <Typography color="textSecondary" sx={{ fontSize: '0.875rem', fontWeight: 500, mb: 1 }}>
            {title}
            {clickable && (
              <Typography component="span" sx={{ fontSize: '0.7rem', ml: 1, color: color }}>
                (Click to view)
              </Typography>
            )}
          </Typography>
          <Typography variant="h4" sx={{ fontWeight: 700, color: color }}>
            {loading ? <Skeleton width={80} /> : value}
          </Typography>
          {subtitle && (
            <Typography variant="caption" color="textSecondary">
              {subtitle}
            </Typography>
          )}
          {trend !== undefined && trend !== 0 && (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}>
              {trend > 0 ? (
                <TrendingUpIcon sx={{ fontSize: 16, color: '#06A77D' }} />
              ) : (
                <TrendingDownIcon sx={{ fontSize: 16, color: '#F44336' }} />
              )}
              <Typography
                variant="caption"
                sx={{ color: trend > 0 ? '#06A77D' : '#F44336', fontWeight: 600 }}
              >
                {trend > 0 ? '+' : ''}{trend.toFixed(1)}%
              </Typography>
            </Box>
          )}
        </Box>
        {Icon && (
          <Box
            sx={{
              p: 1.5,
              borderRadius: 2,
              backgroundColor: `${color}15`,
            }}
          >
            <Icon sx={{ fontSize: 32, color }} />
          </Box>
        )}
      </Box>
    </CardContent>
  </Card>
);

export default StatCard;
