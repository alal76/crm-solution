/**
 * IncidentAssignmentModal - Dialog for assigning incidents to users or groups
 */

import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  FormControl,
  FormControlLabel,
  FormLabel,
  RadioGroup,
  Radio,
  TextField,
  Box,
  Alert,
  CircularProgress,
} from '@mui/material';
import { useApiState } from '../../hooks/useApiState';
import apiClient from '../../services/apiClient';

interface UserBrief {
  id: number;
  name: string;
  email?: string;
}

interface GroupBrief {
  id: number;
  name: string;
}

export interface AssignmentModalProps {
  open: boolean;
  incidentId: number;
  currentAssigneeId?: number;
  currentAssigneeName?: string;
  onAssign: (userId?: number, groupId?: number) => Promise<void>;
  onClose: () => void;
}

export const IncidentAssignmentModal: React.FC<AssignmentModalProps> = ({
  open,
  incidentId,
  currentAssigneeId,
  currentAssigneeName,
  onAssign,
  onClose,
}) => {
  const { loading, error: apiError } = useApiState();
  const [assignmentType, setAssignmentType] = useState<'user' | 'group'>('user');
  const [selectedId, setSelectedId] = useState<number | undefined>(currentAssigneeId);
  const [userSearch, setUserSearch] = useState('');
  const [users, setUsers] = useState<UserBrief[]>([]);
  const [groups, setGroups] = useState<GroupBrief[]>([]);
  const [listLoading, setListLoading] = useState(false);

  useEffect(() => {
    if (!open) return;
    const loadOptions = async () => {
      setListLoading(true);
      try {
        const [usersRes, groupsRes] = await Promise.all([
          apiClient.get<{ id: number; firstName: string; lastName: string; email: string }[]>('/users'),
          apiClient.get<{ id: number; name: string }[]>('/usergroups'),
        ]);
        setUsers(
          usersRes.data.map((u) => ({
            id: u.id,
            name: `${u.firstName} ${u.lastName}`.trim(),
            email: u.email,
          }))
        );
        setGroups(groupsRes.data.map((g) => ({ id: g.id, name: g.name })));
      } catch {
        // non-critical — keep empty lists and let user retry
      } finally {
        setListLoading(false);
      }
    };
    loadOptions();
  }, [open]);

  const items: (UserBrief | GroupBrief)[] = assignmentType === 'user' ? users : groups;
  const filteredItems = items.filter((item) =>
    item.name.toLowerCase().includes(userSearch.toLowerCase())
  );

  const handleAssign = async () => {
    try {
      if (assignmentType === 'user') {
        await onAssign(selectedId);
      } else {
        await onAssign(undefined, selectedId);
      }
      onClose();
    } catch (err) {
      // Error is handled by onAssign
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Assign Incident</DialogTitle>
      <DialogContent sx={{ pt: 2 }}>
        {apiError && <Alert severity="error" sx={{ mb: 2 }}>{String(apiError)}</Alert>}

        <FormControl component="fieldset" fullWidth sx={{ mb: 2 }}>
          <FormLabel component="legend">Assign to</FormLabel>
          <RadioGroup
            row
            value={assignmentType}
            onChange={(e) => {
              setAssignmentType(e.target.value as 'user' | 'group');
              setSelectedId(undefined);
              setUserSearch('');
            }}
          >
            <FormControlLabel value="user" control={<Radio />} label="User" />
            <FormControlLabel value="group" control={<Radio />} label="Group" />
          </RadioGroup>
        </FormControl>

        <TextField
          fullWidth
          placeholder={`Search ${assignmentType}s...`}
          value={userSearch}
          onChange={(e) => setUserSearch(e.target.value)}
          sx={{ mb: 2 }}
          disabled={loading || listLoading}
        />

        {listLoading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 2 }}>
            <CircularProgress size={24} />
          </Box>
        ) : (
          <FormControl component="fieldset" fullWidth>
            <RadioGroup
              value={selectedId || ''}
              onChange={(e) => setSelectedId(Number(e.target.value))}
            >
              {filteredItems.map((item) => (
                <FormControlLabel
                  key={item.id}
                  value={item.id}
                  control={<Radio />}
                  label={
                    <Box>
                      <div>{item.name}</div>
                      {'email' in item && (
                        <div style={{ fontSize: '0.875rem', color: '#666' }}>{String((item as any).email)}</div>
                      )}
                    </Box>
                  }
                />
              ))}
              {filteredItems.length === 0 && (
                <Box sx={{ py: 1, color: 'text.secondary', fontSize: '0.875rem' }}>
                  No {assignmentType}s found.
                </Box>
              )}
            </RadioGroup>
          </FormControl>
        )}

        {currentAssigneeName && (
          <Alert severity="info" sx={{ mt: 2 }}>
            Currently assigned to: {currentAssigneeName}
          </Alert>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={loading}>
          Cancel
        </Button>
        <Button
          onClick={handleAssign}
          variant="contained"
          disabled={!selectedId || loading}
          startIcon={loading ? <CircularProgress size={20} /> : undefined}
        >
          Assign
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default IncidentAssignmentModal;
