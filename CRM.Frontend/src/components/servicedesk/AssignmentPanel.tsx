/**
 * AssignmentPanel - Shows and allows reassignment of a service request agent
 * TODO-SD001-004 (P2)
 *
 * Displays current assignee with avatar/initials and team.
 * "Reassign" button opens a dialog with agent search + confirm.
 * Calls PUT /api/servicerequests/{id}/assign on confirm.
 */

import React, { useState, useMemo } from 'react';
import {
  Box,
  Typography,
  Avatar,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  List,
  ListItemButton,
  ListItemAvatar,
  ListItemText,
  Chip,
  CircularProgress,
  Alert,
  Divider,
  Paper,
} from '@mui/material';
import {
  PersonOutline as PersonOutlineIcon,
  PersonAdd as PersonAddIcon,
  SwapHoriz as ReassignIcon,
} from '@mui/icons-material';
import apiClient from '../../services/apiClient';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface Agent {
  id: number;
  name: string;
  department?: string;
  activeTickets?: number;
}

export interface AssignmentPanelProps {
  serviceRequestId: number;
  currentAssigneeId?: number;
  currentAssigneeName?: string;
  currentTeam?: string;
  agents?: Agent[];
  onAssignmentChange?: (assigneeId: number) => void;
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

function getInitials(name: string): string {
  const parts = name.trim().split(' ');
  if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
}

function stringToColor(s: string): string {
  let hash = 0;
  for (let i = 0; i < s.length; i++) {
    hash = (s.codePointAt(i) ?? 0) + ((hash << 5) - hash);
  }
  const h = Math.abs(hash) % 360;
  return `hsl(${h}, 45%, 45%)`;
}

// ─── Default mock agents ─────────────────────────────────────────────────────

const DEFAULT_AGENTS: Agent[] = [
  { id: 1, name: 'Alice Johnson', department: 'Level 1 Support', activeTickets: 4 },
  { id: 2, name: 'Bob Martinez', department: 'Level 2 Support', activeTickets: 2 },
  { id: 3, name: 'Carol Zhang', department: 'Engineering', activeTickets: 1 },
  { id: 4, name: 'David Kim', department: 'Operations', activeTickets: 6 },
  { id: 5, name: 'Eve Patel', department: 'Level 1 Support', activeTickets: 3 },
];

// ─── Component ────────────────────────────────────────────────────────────────

const AssignmentPanel: React.FC<AssignmentPanelProps> = ({
  serviceRequestId,
  currentAssigneeId,
  currentAssigneeName,
  currentTeam,
  agents,
  onAssignmentChange,
}) => {
  const [dialogOpen, setDialogOpen] = useState(false);
  const [search, setSearch] = useState('');
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const agentList = agents ?? DEFAULT_AGENTS;

  const filteredAgents = useMemo(
    () =>
      agentList.filter(
        (a) =>
          a.name.toLowerCase().includes(search.toLowerCase()) ||
          (a.department ?? '').toLowerCase().includes(search.toLowerCase())
      ),
    [agentList, search]
  );

  const handleOpenDialog = () => {
    setSearch('');
    setSelectedId(currentAssigneeId ?? null);
    setError(null);
    setDialogOpen(true);
  };

  const handleConfirm = async () => {
    if (selectedId === null) return;
    setLoading(true);
    setError(null);
    try {
      await apiClient.put(`/api/servicerequests/${serviceRequestId}/assign`, {
        assigneeId: selectedId,
      });
      onAssignmentChange?.(selectedId);
      setDialogOpen(false);
    } catch (err) {
      setError('Failed to update assignment. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const hasAssignee = Boolean(currentAssigneeName);

  return (
    <>
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" color="text.secondary" gutterBottom>
          Assigned To
        </Typography>

        {hasAssignee ? (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mt: 0.5 }}>
            <Avatar
              sx={{
                width: 40,
                height: 40,
                bgcolor: stringToColor(currentAssigneeName!),
                fontSize: 14,
              }}
            >
              {getInitials(currentAssigneeName!)}
            </Avatar>

            <Box sx={{ flex: 1, minWidth: 0 }}>
              <Typography variant="body2" fontWeight="medium" noWrap>
                {currentAssigneeName}
              </Typography>
              {currentTeam && (
                <Typography variant="caption" color="text.secondary" noWrap>
                  {currentTeam}
                </Typography>
              )}
            </Box>

            <Button
              size="small"
              variant="outlined"
              startIcon={<ReassignIcon />}
              onClick={handleOpenDialog}
            >
              Reassign
            </Button>
          </Box>
        ) : (
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 1.5,
              mt: 0.5,
              p: 1.5,
              borderRadius: 1,
              bgcolor: 'action.hover',
            }}
          >
            <PersonOutlineIcon color="disabled" sx={{ fontSize: 32 }} />
            <Typography variant="body2" color="text.secondary" sx={{ flex: 1 }}>
              Unassigned
            </Typography>
            <Button
              size="small"
              variant="contained"
              startIcon={<PersonAddIcon />}
              onClick={handleOpenDialog}
            >
              Assign
            </Button>
          </Box>
        )}
      </Paper>

      {/* ── Reassign Dialog ──────────────────────────────────────────────── */}
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          {hasAssignee ? 'Reassign Service Request' : 'Assign Service Request'}
        </DialogTitle>

        <DialogContent sx={{ pt: 1 }}>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          <TextField
            autoFocus
            fullWidth
            size="small"
            placeholder="Search agents by name or department…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            sx={{ mb: 1.5 }}
          />

          <Divider />

          <List dense disablePadding sx={{ maxHeight: 300, overflowY: 'auto' }}>
            {filteredAgents.length === 0 && (
              <Typography
                variant="body2"
                color="text.secondary"
                sx={{ textAlign: 'center', py: 3 }}
              >
                No agents match your search.
              </Typography>
            )}

            {filteredAgents.map((agent) => (
              <ListItemButton
                key={agent.id}
                selected={selectedId === agent.id}
                onClick={() => setSelectedId(agent.id)}
                sx={{ borderRadius: 1, mb: 0.25 }}
              >
                <ListItemAvatar>
                  <Avatar
                    sx={{
                      width: 34,
                      height: 34,
                      bgcolor: stringToColor(agent.name),
                      fontSize: 12,
                    }}
                  >
                    {getInitials(agent.name)}
                  </Avatar>
                </ListItemAvatar>

                <ListItemText
                  primary={agent.name}
                  secondary={agent.department ?? ''}
                />

                {agent.activeTickets !== undefined && (
                  <Chip
                    label={`${agent.activeTickets} open`}
                    size="small"
                    color={agent.activeTickets > 5 ? 'warning' : 'default'}
                    variant="outlined"
                    sx={{ ml: 1 }}
                  />
                )}
              </ListItemButton>
            ))}
          </List>
        </DialogContent>

        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setDialogOpen(false)} disabled={loading}>
            Cancel
          </Button>
          <Button
            variant="contained"
            onClick={handleConfirm}
            disabled={selectedId === null || loading}
            startIcon={loading ? <CircularProgress size={16} color="inherit" /> : undefined}
          >
            {loading ? 'Assigning…' : 'Confirm Assignment'}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};

export default AssignmentPanel;
