// Related Incidents Widget - Show incidents linked to a problem
// Part of ITSM Enhancement Plan - Phase 1.2

import React, { useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  ListItemSecondaryAction,
  IconButton,
  Chip,
  Stack,
  TextField,
  InputAdornment,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Tooltip,
  Avatar,
  Divider,
  Badge,
} from '@mui/material';
import {
  BugReport as IncidentIcon,
  Search as SearchIcon,
  Add as AddIcon,
  Link as LinkIcon,
  LinkOff as UnlinkIcon,
  OpenInNew as OpenIcon,
  Warning as WarningIcon,
  Error as ErrorIcon,
  Info as InfoIcon,
  CheckCircle as ResolvedIcon,
  Schedule as PendingIcon,
} from '@mui/icons-material';

export type IncidentState = 'new' | 'assigned' | 'in_progress' | 'on_hold' | 'resolved' | 'closed';
export type IncidentPriority = 1 | 2 | 3 | 4 | 5;

export interface RelatedIncident {
  id: number;
  number: string;
  shortDescription: string;
  state: IncidentState;
  priority: IncidentPriority;
  assignedTo?: string;
  createdAt: Date | string;
  resolvedAt?: Date | string;
  usedWorkaround?: boolean;
}

export interface RelatedIncidentsWidgetProps {
  problemId: number;
  incidents: RelatedIncident[];
  onLinkIncident?: (incidentId: number) => Promise<void>;
  onUnlinkIncident?: (incidentId: number) => Promise<void>;
  onViewIncident?: (incident: RelatedIncident) => void;
  searchIncidents?: (query: string) => Promise<RelatedIncident[]>;
  readOnly?: boolean;
  maxDisplay?: number;
}

const getStateIcon = (state: IncidentState) => {
  switch (state) {
    case 'new':
      return <WarningIcon sx={{ color: '#ff9800' }} />;
    case 'assigned':
      return <PendingIcon sx={{ color: '#2196f3' }} />;
    case 'in_progress':
      return <InfoIcon sx={{ color: '#00bcd4' }} />;
    case 'on_hold':
      return <PendingIcon sx={{ color: '#9e9e9e' }} />;
    case 'resolved':
      return <ResolvedIcon sx={{ color: '#4caf50' }} />;
    case 'closed':
      return <ResolvedIcon sx={{ color: '#388e3c' }} />;
    default:
      return <IncidentIcon />;
  }
};

const getStateLabel = (state: IncidentState): string => {
  return state.replaceAll('_', ' ').replaceAll(/\b\w/g, (l) => l.toUpperCase());
};

const getPriorityColor = (priority: IncidentPriority): string => {
  switch (priority) {
    case 1:
      return '#d32f2f'; // Critical
    case 2:
      return '#f57c00'; // High
    case 3:
      return '#fbc02d'; // Medium
    case 4:
      return '#388e3c'; // Low
    case 5:
      return '#1976d2'; // Planning
    default:
      return '#757575';
  }
};

const getPriorityLabel = (priority: IncidentPriority): string => {
  const labels: Record<IncidentPriority, string> = {
    1: 'Critical',
    2: 'High',
    3: 'Medium',
    4: 'Low',
    5: 'Planning',
  };
  return labels[priority];
};

interface LinkIncidentDialogProps {
  open: boolean;
  onClose: () => void;
  onLink: (incidentId: number) => void;
  searchIncidents?: (query: string) => Promise<RelatedIncident[]>;
  existingIds: number[];
}

const LinkIncidentDialog: React.FC<LinkIncidentDialogProps> = ({
  open,
  onClose,
  onLink,
  searchIncidents,
  existingIds,
}) => {
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<RelatedIncident[]>([]);
  const [loading, setLoading] = useState(false);

  const handleSearch = async () => {
    if (!searchIncidents || !searchQuery.trim()) return;
    setLoading(true);
    try {
      const results = await searchIncidents(searchQuery);
      // Filter out already linked incidents
      setSearchResults(results.filter((r) => !existingIds.includes(r.id)));
    } catch (error) {
      console.error('Search failed:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleSearch();
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Link Incident to Problem</DialogTitle>
      <DialogContent>
        <TextField
          fullWidth
          placeholder="Search by incident number or description..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          onKeyPress={handleKeyPress}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon />
              </InputAdornment>
            ),
          }}
          sx={{ mt: 1, mb: 2 }}
        />
        <Button
          variant="contained"
          onClick={handleSearch}
          disabled={loading || !searchQuery.trim()}
          sx={{ mb: 2 }}
        >
          {loading ? 'Searching...' : 'Search'}
        </Button>

        {searchResults.length > 0 ? (
          <List>
            {searchResults.map((incident) => (
              <ListItem
                key={incident.id}
                button
                onClick={() => {
                  onLink(incident.id);
                  onClose();
                }}
                sx={{
                  border: '1px solid #e0e0e0',
                  borderRadius: 1,
                  mb: 1,
                }}
              >
                <ListItemIcon>{getStateIcon(incident.state)}</ListItemIcon>
                <ListItemText
                  primary={
                    <Stack direction="row" alignItems="center" spacing={1}>
                      <Typography variant="body2" fontWeight={600}>
                        {incident.number}
                      </Typography>
                      <Chip
                        label={`P${incident.priority}`}
                        size="small"
                        sx={{
                          backgroundColor: `${getPriorityColor(incident.priority)}20`,
                          color: getPriorityColor(incident.priority),
                          height: 20,
                          fontSize: '0.7rem',
                        }}
                      />
                    </Stack>
                  }
                  secondary={incident.shortDescription}
                />
                <ListItemSecondaryAction>
                  <IconButton edge="end" onClick={() => onLink(incident.id)}>
                    <LinkIcon />
                  </IconButton>
                </ListItemSecondaryAction>
              </ListItem>
            ))}
          </List>
        ) : searchQuery && !loading ? (
          <Typography color="text.secondary" textAlign="center">
            No incidents found
          </Typography>
        ) : null}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
      </DialogActions>
    </Dialog>
  );
};

export const RelatedIncidentsWidget: React.FC<RelatedIncidentsWidgetProps> = ({
  problemId,
  incidents,
  onLinkIncident,
  onUnlinkIncident,
  onViewIncident,
  searchIncidents,
  readOnly = false,
  maxDisplay = 10,
}) => {
  const [dialogOpen, setDialogOpen] = useState(false);
  const [showAll, setShowAll] = useState(false);

  const displayedIncidents = showAll ? incidents : incidents.slice(0, maxDisplay);
  const hasMore = incidents.length > maxDisplay;

  // Statistics
  const stats = {
    total: incidents.length,
    open: incidents.filter((i) => !['resolved', 'closed'].includes(i.state)).length,
    resolved: incidents.filter((i) => ['resolved', 'closed'].includes(i.state)).length,
    critical: incidents.filter((i) => i.priority === 1).length,
    usedWorkaround: incidents.filter((i) => i.usedWorkaround).length,
  };

  const handleLinkIncident = async (incidentId: number) => {
    await onLinkIncident?.(incidentId);
  };

  const handleUnlinkIncident = async (incidentId: number) => {
    await onUnlinkIncident?.(incidentId);
  };

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      {/* Header */}
      <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 2 }}>
        <Stack direction="row" alignItems="center" spacing={1}>
          <Badge badgeContent={stats.total} color="primary">
            <IncidentIcon color="action" />
          </Badge>
          <Typography variant="subtitle1" fontWeight={600}>
            Related Incidents
          </Typography>
        </Stack>
        {!readOnly && onLinkIncident && (
          <Button
            size="small"
            startIcon={<AddIcon />}
            onClick={() => setDialogOpen(true)}
            variant="outlined"
          >
            Link Incident
          </Button>
        )}
      </Stack>

      {/* Statistics */}
      <Stack direction="row" spacing={1} sx={{ mb: 2 }} flexWrap="wrap">
        <Chip
          size="small"
          label={`${stats.open} Open`}
          sx={{ backgroundColor: '#ff980020', color: '#ff9800' }}
        />
        <Chip
          size="small"
          label={`${stats.resolved} Resolved`}
          sx={{ backgroundColor: '#4caf5020', color: '#4caf50' }}
        />
        {stats.critical > 0 && (
          <Chip
            size="small"
            icon={<ErrorIcon sx={{ fontSize: 14 }} />}
            label={`${stats.critical} Critical`}
            sx={{ backgroundColor: '#f4433620', color: '#f44336' }}
          />
        )}
        {stats.usedWorkaround > 0 && (
          <Chip
            size="small"
            label={`${stats.usedWorkaround} Used Workaround`}
            variant="outlined"
          />
        )}
      </Stack>

      <Divider sx={{ mb: 1 }} />

      {/* Incidents List */}
      {incidents.length === 0 ? (
        <Box sx={{ py: 3, textAlign: 'center' }}>
          <IncidentIcon sx={{ fontSize: 48, color: 'action.disabled', mb: 1 }} />
          <Typography color="text.secondary">No related incidents</Typography>
          {!readOnly && (
            <Typography variant="caption" color="text.secondary">
              Link incidents to this problem to track affected users
            </Typography>
          )}
        </Box>
      ) : (
        <>
          <List dense sx={{ py: 0 }}>
            {displayedIncidents.map((incident) => (
              <ListItem
                key={incident.id}
                sx={{
                  borderRadius: 1,
                  mb: 0.5,
                  '&:hover': { backgroundColor: 'action.hover' },
                }}
              >
                <ListItemIcon sx={{ minWidth: 36 }}>
                  {getStateIcon(incident.state)}
                </ListItemIcon>
                <ListItemText
                  primary={
                    <Stack direction="row" alignItems="center" spacing={1}>
                      <Typography
                        variant="body2"
                        fontWeight={600}
                        sx={{ cursor: 'pointer' }}
                        onClick={() => onViewIncident?.(incident)}
                      >
                        {incident.number}
                      </Typography>
                      <Chip
                        label={`P${incident.priority}`}
                        size="small"
                        sx={{
                          backgroundColor: `${getPriorityColor(incident.priority)}20`,
                          color: getPriorityColor(incident.priority),
                          height: 18,
                          fontSize: '0.65rem',
                        }}
                      />
                      <Chip
                        label={getStateLabel(incident.state)}
                        size="small"
                        variant="outlined"
                        sx={{ height: 18, fontSize: '0.65rem' }}
                      />
                      {incident.usedWorkaround && (
                        <Tooltip title="Used workaround from this problem">
                          <Chip
                            label="Workaround"
                            size="small"
                            color="info"
                            sx={{ height: 18, fontSize: '0.65rem' }}
                          />
                        </Tooltip>
                      )}
                    </Stack>
                  }
                  secondary={
                    <Typography variant="caption" color="text.secondary" noWrap>
                      {incident.shortDescription}
                    </Typography>
                  }
                />
                <ListItemSecondaryAction>
                  <Stack direction="row" spacing={0.5}>
                    {onViewIncident && (
                      <Tooltip title="View Incident">
                        <IconButton
                          size="small"
                          onClick={() => onViewIncident(incident)}
                        >
                          <OpenIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    )}
                    {!readOnly && onUnlinkIncident && (
                      <Tooltip title="Unlink Incident">
                        <IconButton
                          size="small"
                          onClick={() => handleUnlinkIncident(incident.id)}
                          color="error"
                        >
                          <UnlinkIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    )}
                  </Stack>
                </ListItemSecondaryAction>
              </ListItem>
            ))}
          </List>

          {hasMore && (
            <Button
              size="small"
              onClick={() => setShowAll(!showAll)}
              sx={{ mt: 1 }}
            >
              {showAll ? 'Show Less' : `Show ${incidents.length - maxDisplay} More`}
            </Button>
          )}
        </>
      )}

      {/* Link Dialog */}
      <LinkIncidentDialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        onLink={handleLinkIncident}
        searchIncidents={searchIncidents}
        existingIds={incidents.map((i) => i.id)}
      />
    </Paper>
  );
};

export default RelatedIncidentsWidget;
