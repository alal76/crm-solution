/**
 * ProblemRelatedIncidentsList - Display incidents related to a problem
 */

import React, { useEffect, useState } from 'react';
import {
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  CircularProgress,
  Alert,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  IconButton,
  Tooltip,
} from '@mui/material';
import {
  Delete as UnlinkIcon,
  OpenInNew as OpenIcon,
} from '@mui/icons-material';
import { IncidentStatusBadge } from './IncidentStatusBadge';
import { IncidentPriorityBadge } from './IncidentPriorityBadge';

export interface RelatedIncident {
  id: number;
  number: string;
  title: string;
  status: number;
  priority: number;
  createdAt: string;
}

interface ProblemRelatedIncidentsListProps {
  incidents: RelatedIncident[];
  loading?: boolean;
  onUnlink?: (incidentId: number) => Promise<void>;
  onOpen?: (incidentId: number) => void;
}

export const ProblemRelatedIncidentsList: React.FC<ProblemRelatedIncidentsListProps> = ({
  incidents,
  loading = false,
  onUnlink,
  onOpen,
}) => {
  const [unlinkConfirmOpen, setUnlinkConfirmOpen] = useState(false);
  const [selectedIncidentId, setSelectedIncidentId] = useState<number | null>(null);
  const [unlinking, setUnlinking] = useState(false);

  const handleUnlinkClick = (incidentId: number) => {
    setSelectedIncidentId(incidentId);
    setUnlinkConfirmOpen(true);
  };

  const handleConfirmUnlink = async () => {
    if (!selectedIncidentId || !onUnlink) return;
    
    setUnlinking(true);
    try {
      await onUnlink(selectedIncidentId);
      setUnlinkConfirmOpen(false);
      setSelectedIncidentId(null);
    } finally {
      setUnlinking(false);
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (incidents.length === 0) {
    return (
      <Alert severity="info">
        No related incidents found
      </Alert>
    );
  }

  return (
    <>
      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow sx={{ bgcolor: 'action.hover' }}>
              <TableCell>Number</TableCell>
              <TableCell>Title</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Priority</TableCell>
              <TableCell>Created</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {incidents.map((incident) => (
              <TableRow key={incident.id} hover>
                <TableCell sx={{ fontWeight: 600 }}>
                  {incident.number}
                </TableCell>
                <TableCell>{incident.title}</TableCell>
                <TableCell>
                  <IncidentStatusBadge status={incident.status as any} size="small" />
                </TableCell>
                <TableCell>
                  <IncidentPriorityBadge priority={incident.priority as any} size="small" />
                </TableCell>
                <TableCell>
                  {new Date(incident.createdAt).toLocaleDateString()}
                </TableCell>
                <TableCell align="right">
                  <Tooltip title="Open">
                    <IconButton
                      size="small"
                      onClick={() => onOpen?.(incident.id)}
                    >
                      <OpenIcon />
                    </IconButton>
                  </Tooltip>
                  {onUnlink && (
                    <Tooltip title="Unlink">
                      <IconButton
                        size="small"
                        color="error"
                        onClick={() => handleUnlinkClick(incident.id)}
                      >
                        <UnlinkIcon />
                      </IconButton>
                    </Tooltip>
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Unlink Confirmation */}
      <Dialog open={unlinkConfirmOpen} onClose={() => setUnlinkConfirmOpen(false)}>
        <DialogTitle>Unlink Incident</DialogTitle>
        <DialogContent>
          Are you sure you want to unlink this incident from the problem?
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setUnlinkConfirmOpen(false)} disabled={unlinking}>
            Cancel
          </Button>
          <Button
            onClick={handleConfirmUnlink}
            variant="contained"
            color="error"
            disabled={unlinking}
          >
            Unlink
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};

export default ProblemRelatedIncidentsList;
