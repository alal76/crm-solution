/**
 * IncidentAssignmentModal - Dialog for assigning incidents to users or groups
 */

import React, { useState } from 'react';
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
  const { loading, error: apiError, setError } = useApiState();
  const [assignmentType, setAssignmentType] = useState<'user' | 'group'>('user');
  const [selectedId, setSelectedId] = useState<number | undefined>(currentAssigneeId);
  const [userSearch, setUserSearch] = useState('');

  // Mock user/group data - replace with API call
  const mockUsers = [
    { id: 1, name: 'John Doe', email: 'john.doe@example.com' },
    { id: 2, name: 'Jane Smith', email: 'jane.smith@example.com' },
    { id: 3, name: 'Bob Johnson', email: 'bob.johnson@example.com' },
  ];

  const mockGroups = [
    { id: 10, name: 'Support Team' },
    { id: 11, name: 'Engineering Team' },
    { id: 12, name: 'Operations Team' },
  ];

  const items = assignmentType === 'user' ? mockUsers : mockGroups;
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
          disabled={loading}
        />

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
          </RadioGroup>
        </FormControl>

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
