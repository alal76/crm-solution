/**
 * SavedSearchesPanel - UI panel listing saved search presets for a given entity type.
 * Implements TODO-PORTAL-06.
 *
 * Features:
 *  - Lists saved searches filtered by entityType
 *  - "Apply" button calls POST /api/saved-searches/{id}/use and fires onApply(filterJson)
 *  - "Delete" icon removes a preset
 *  - "New" button opens an inline form to save the current filter
 */

import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  IconButton,
  Typography,
  Button,
  TextField,
  CircularProgress,
  Alert,
  Tooltip,
  Divider,
  Collapse,
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import BookmarkAddIcon from '@mui/icons-material/BookmarkAdd';
import SearchIcon from '@mui/icons-material/Search';
import apiClient from '../../services/apiClient';

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

interface SavedSearch {
  id: number;
  name: string;
  entityType: string;
  filterCriteriaJson: string;
  isPinned: boolean;
  usageCount: number;
  description?: string;
}

interface SavedSearchesPanelProps {
  /** EntityType key (e.g., "Account", "Contact", "Lead") */
  entityType: string;
  /** Current filter criteria JSON to persist when saving a new preset */
  currentFilterJson?: string;
  /** Called when the user clicks "Apply" with the stored filterCriteriaJson */
  onApply?: (filterJson: string) => void;
}

// ---------------------------------------------------------------------------
// Component
// ---------------------------------------------------------------------------

const SavedSearchesPanel: React.FC<SavedSearchesPanelProps> = ({
  entityType,
  currentFilterJson = '{}',
  onApply,
}) => {
  const [searches, setSearches] = useState<SavedSearch[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showNewForm, setShowNewForm] = useState(false);
  const [newName, setNewName] = useState('');
  const [saving, setSaving] = useState(false);

  // ---------------------------------------------------------------------------
  // Fetch saved searches
  // ---------------------------------------------------------------------------

  const loadSearches = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await apiClient.get<SavedSearch[]>('/saved-searches', {
        params: { entityType },
      });
      setSearches(res.data ?? []);
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr?.response?.data?.message ?? 'Failed to load saved searches');
    } finally {
      setLoading(false);
    }
  }, [entityType]);

  useEffect(() => {
    loadSearches();
  }, [loadSearches]);

  // ---------------------------------------------------------------------------
  // Apply a saved search
  // ---------------------------------------------------------------------------

  const handleApply = async (search: SavedSearch) => {
    try {
      await apiClient.post(`/saved-searches/${search.id}/use`);
    } catch {
      // Non-fatal – just track usage
    }
    onApply?.(search.filterCriteriaJson);
  };

  // ---------------------------------------------------------------------------
  // Delete a saved search
  // ---------------------------------------------------------------------------

  const handleDelete = async (id: number) => {
    try {
      await apiClient.delete(`/saved-searches/${id}`);
      setSearches((prev) => prev.filter((s) => s.id !== id));
    } catch {
      setError('Failed to delete saved search');
    }
  };

  // ---------------------------------------------------------------------------
  // Save current filter as new preset
  // ---------------------------------------------------------------------------

  const handleSaveNew = async () => {
    if (!newName.trim()) return;
    setSaving(true);
    try {
      const res = await apiClient.post<SavedSearch>('/saved-searches', {
        name: newName.trim(),
        entityType,
        filterCriteriaJson: currentFilterJson,
      });
      setSearches((prev) => [...prev, res.data]);
      setNewName('');
      setShowNewForm(false);
    } catch {
      setError('Failed to save search preset');
    } finally {
      setSaving(false);
    }
  };

  // ---------------------------------------------------------------------------
  // Render
  // ---------------------------------------------------------------------------

  return (
    <Box sx={{ minWidth: 260, maxWidth: 360 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', px: 1, pt: 1 }}>
        <Typography variant="subtitle2" fontWeight={600}>
          Saved Searches
        </Typography>
        <Tooltip title="Save current filter">
          <IconButton size="small" onClick={() => setShowNewForm((v) => !v)}>
            <BookmarkAddIcon fontSize="small" />
          </IconButton>
        </Tooltip>
      </Box>

      {/* Inline new-search form */}
      <Collapse in={showNewForm}>
        <Box sx={{ px: 1, pb: 1, display: 'flex', gap: 1, alignItems: 'center' }}>
          <TextField
            size="small"
            placeholder="Preset name…"
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSaveNew()}
            sx={{ flex: 1 }}
          />
          <Button size="small" variant="contained" disabled={saving || !newName.trim()} onClick={handleSaveNew}>
            {saving ? <CircularProgress size={16} /> : 'Save'}
          </Button>
        </Box>
        <Divider />
      </Collapse>

      {error && (
        <Alert severity="error" sx={{ mx: 1, mb: 1 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 2 }}>
          <CircularProgress size={24} />
        </Box>
      ) : searches.length === 0 ? (
        <Typography variant="body2" color="text.secondary" sx={{ px: 2, py: 1.5 }}>
          No saved searches for {entityType}.
        </Typography>
      ) : (
        <List dense disablePadding>
          {searches.map((s) => (
            <ListItem
              key={s.id}
              sx={{ '&:hover': { backgroundColor: 'action.hover' }, cursor: 'pointer' }}
              onClick={() => handleApply(s)}
            >
              <SearchIcon fontSize="small" sx={{ mr: 1, color: 'text.secondary' }} />
              <ListItemText
                primary={s.name}
                secondary={s.usageCount > 0 ? `Used ${s.usageCount}×` : undefined}
                primaryTypographyProps={{ variant: 'body2' }}
                secondaryTypographyProps={{ variant: 'caption' }}
              />
              <ListItemSecondaryAction>
                <IconButton
                  size="small"
                  edge="end"
                  onClick={(e) => { e.stopPropagation(); handleDelete(s.id); }}
                  aria-label="delete preset"
                >
                  <DeleteIcon fontSize="small" />
                </IconButton>
              </ListItemSecondaryAction>
            </ListItem>
          ))}
        </List>
      )}
    </Box>
  );
};

export default SavedSearchesPanel;
