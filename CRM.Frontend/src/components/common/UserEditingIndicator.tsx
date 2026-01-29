/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

import React, { useEffect, useState } from 'react';
import { Chip, Tooltip, Box, Avatar, Typography } from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import PersonIcon from '@mui/icons-material/Person';
import { useRecordSubscription } from '../../hooks/useSignalR';
import { UserEditingNotification } from '../../services/signalRService';

interface EditingUser {
  userId: string;
  userName: string;
  startedAt: Date;
}

interface UserEditingIndicatorProps {
  entityType: string;
  entityId: number | null | undefined;
  currentUserId?: string;
}

/**
 * Component that shows which users are currently editing a record.
 * Uses SignalR to receive real-time updates about editing status.
 */
export const UserEditingIndicator: React.FC<UserEditingIndicatorProps> = ({
  entityType,
  entityId,
  currentUserId,
}) => {
  const [editingUsers, setEditingUsers] = useState<Map<string, EditingUser>>(new Map());

  useRecordSubscription(entityType, entityId, {
    onUserEditing: (data: UserEditingNotification) => {
      // Don't show ourselves
      if (data.userId === currentUserId) return;

      setEditingUsers(prev => {
        const next = new Map(prev);
        
        if (data.isEditing) {
          next.set(data.userId, {
            userId: data.userId,
            userName: data.userName,
            startedAt: new Date(),
          });
        } else {
          next.delete(data.userId);
        }
        
        return next;
      });
    },
  });

  // Clean up stale editing indicators after 5 minutes
  useEffect(() => {
    const interval = setInterval(() => {
      const fiveMinutesAgo = new Date(Date.now() - 5 * 60 * 1000);
      
      setEditingUsers(prev => {
        const next = new Map(prev);
        let changed = false;
        
        for (const [userId, user] of next) {
          if (user.startedAt < fiveMinutesAgo) {
            next.delete(userId);
            changed = true;
          }
        }
        
        return changed ? next : prev;
      });
    }, 60000); // Check every minute

    return () => clearInterval(interval);
  }, []);

  if (!entityId || editingUsers.size === 0) {
    return null;
  }

  const users = Array.from(editingUsers.values());

  if (users.length === 1) {
    const user = users[0];
    return (
      <Tooltip title={`${user.userName} is currently editing this record`}>
        <Chip
          icon={<EditIcon />}
          label={`${user.userName} is editing`}
          color="warning"
          size="small"
          variant="outlined"
          sx={{ animation: 'pulse 2s infinite' }}
        />
      </Tooltip>
    );
  }

  // Multiple users editing
  return (
    <Tooltip
      title={
        <Box>
          <Typography variant="body2" fontWeight="bold">
            Currently editing:
          </Typography>
          {users.map(user => (
            <Typography key={user.userId} variant="body2">
              • {user.userName}
            </Typography>
          ))}
        </Box>
      }
    >
      <Chip
        icon={<EditIcon />}
        label={`${users.length} users editing`}
        color="warning"
        size="small"
        variant="outlined"
        sx={{ animation: 'pulse 2s infinite' }}
      />
    </Tooltip>
  );
};

/**
 * Compact version showing just avatars of editing users.
 */
export const UserEditingAvatars: React.FC<UserEditingIndicatorProps> = ({
  entityType,
  entityId,
  currentUserId,
}) => {
  const [editingUsers, setEditingUsers] = useState<Map<string, EditingUser>>(new Map());

  useRecordSubscription(entityType, entityId, {
    onUserEditing: (data: UserEditingNotification) => {
      if (data.userId === currentUserId) return;

      setEditingUsers(prev => {
        const next = new Map(prev);
        
        if (data.isEditing) {
          next.set(data.userId, {
            userId: data.userId,
            userName: data.userName,
            startedAt: new Date(),
          });
        } else {
          next.delete(data.userId);
        }
        
        return next;
      });
    },
  });

  if (!entityId || editingUsers.size === 0) {
    return null;
  }

  const users = Array.from(editingUsers.values());

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
      {users.slice(0, 3).map(user => (
        <Tooltip key={user.userId} title={`${user.userName} is editing`}>
          <Avatar
            sx={{
              width: 24,
              height: 24,
              fontSize: 12,
              bgcolor: 'warning.main',
              border: '2px solid',
              borderColor: 'warning.light',
            }}
          >
            {user.userName.charAt(0).toUpperCase()}
          </Avatar>
        </Tooltip>
      ))}
      {users.length > 3 && (
        <Tooltip title={`${users.length - 3} more users editing`}>
          <Avatar
            sx={{
              width: 24,
              height: 24,
              fontSize: 10,
              bgcolor: 'grey.500',
            }}
          >
            +{users.length - 3}
          </Avatar>
        </Tooltip>
      )}
    </Box>
  );
};

export default UserEditingIndicator;
