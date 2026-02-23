/**
 * OrderStatusTimeline - Vertical timeline showing the lifecycle of an order
 * with milestone tracking and status history.
 */

import React from 'react';
import {
  Box,
  Typography,
  Paper,
} from '@mui/material';
import {
  CheckCircle as CheckCircleIcon,
  RadioButtonChecked as CurrentIcon,
  RadioButtonUnchecked as PendingIcon,
} from '@mui/icons-material';

interface StatusHistoryEntry {
  status: string;
  changedAt: string;
  changedBy?: string;
  notes?: string;
}

interface OrderTimelineProps {
  currentStatus: string;
  statusHistory: StatusHistoryEntry[];
}

const ORDER_MILESTONES = [
  'Draft',
  'Submitted',
  'Approved',
  'Processing',
  'Shipped',
  'Delivered',
  'Closed',
];

const OrderStatusTimeline: React.FC<OrderTimelineProps> = ({
  currentStatus,
  statusHistory,
}) => {
  const currentIndex = ORDER_MILESTONES.findIndex(
    (m) => m.toLowerCase() === currentStatus.toLowerCase(),
  );

  const historyMap = new Map<string, StatusHistoryEntry>();
  statusHistory.forEach((entry) => {
    const key = entry.status.toLowerCase();
    if (!historyMap.has(key)) {
      historyMap.set(key, entry);
    }
  });

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Typography variant="subtitle2" gutterBottom fontWeight={600}>
        Order Status Timeline
      </Typography>

      <Box>
        {ORDER_MILESTONES.map((milestone, idx) => {
          const isCompleted = idx < currentIndex;
          const isCurrent = idx === currentIndex;
          const entry = historyMap.get(milestone.toLowerCase());

          let icon: React.ReactNode;
          let color: string;

          if (isCompleted) {
            icon = <CheckCircleIcon sx={{ fontSize: 22, color: 'success.main' }} />;
            color = 'success.main';
          } else if (isCurrent) {
            icon = <CurrentIcon sx={{ fontSize: 22, color: 'primary.main' }} />;
            color = 'primary.main';
          } else {
            icon = <PendingIcon sx={{ fontSize: 22, color: 'action.disabled' }} />;
            color = 'text.disabled';
          }

          return (
            <Box key={milestone} display="flex" position="relative">
              {/* Connector line */}
              <Box
                display="flex"
                flexDirection="column"
                alignItems="center"
                mr={2}
                sx={{ minWidth: 22 }}
              >
                {icon}
                {idx < ORDER_MILESTONES.length - 1 && (
                  <Box
                    sx={{
                      width: 2,
                      flexGrow: 1,
                      minHeight: 24,
                      bgcolor: isCompleted ? 'success.main' : 'divider',
                    }}
                  />
                )}
              </Box>

              {/* Content */}
              <Box pb={idx < ORDER_MILESTONES.length - 1 ? 1.5 : 0} minHeight={40}>
                <Typography
                  variant="body2"
                  fontWeight={isCurrent ? 700 : isCompleted ? 500 : 400}
                  color={color}
                >
                  {milestone}
                  {isCurrent && (
                    <Typography component="span" variant="caption" sx={{ ml: 1, color: 'primary.main' }}>
                      (Current)
                    </Typography>
                  )}
                </Typography>
                {entry && (
                  <Box>
                    <Typography variant="caption" color="text.secondary">
                      {new Date(entry.changedAt).toLocaleString()}
                      {entry.changedBy && ` · ${entry.changedBy}`}
                    </Typography>
                    {entry.notes && (
                      <Typography variant="caption" display="block" color="text.secondary" sx={{ fontStyle: 'italic' }}>
                        {entry.notes}
                      </Typography>
                    )}
                  </Box>
                )}
              </Box>
            </Box>
          );
        })}
      </Box>
    </Paper>
  );
};

export default OrderStatusTimeline;
