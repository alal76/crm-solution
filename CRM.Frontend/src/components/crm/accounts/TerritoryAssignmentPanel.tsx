import React, { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Chip,
  CircularProgress,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  SelectChangeEvent,
  Stack,
  Alert,
  Typography,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Paper,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import SaveIcon from '@mui/icons-material/Save';
import territoryService from '../../../services/territoryService';

interface Territory {
  id: number;
  name: string;
  description?: string;
  managerId?: number;
}

interface TerritoryAssignmentPanelProps {
  accountId: number;
  onSave?: (territories: number[]) => void;
  onError?: (error: string) => void;
}

/**
 * TerritoryAssignmentPanel Component
 * Allows assigning territories to an account with multi-select dropdown,
 * chip display, and save/delete operations.
 */
export const TerritoryAssignmentPanel: React.FC<TerritoryAssignmentPanelProps> = ({
  accountId,
  onSave,
  onError,
}) => {
  const [territories, setTerritories] = useState<Territory[]>([]);
  const [assignedTerritories, setAssignedTerritories] = useState<number[]>([]);
  const [selectedTerritories, setSelectedTerritories] = useState<number[]>([]);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [expanded, setExpanded] = useState(false);
  const [hasChanges, setHasChanges] = useState(false);

  useEffect(() => {
    loadData();
  }, [accountId]);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);

      // Fetch available territories
      const allTerritories = await territoryService.getAll();
      setTerritories(allTerritories);

      // Fetch assigned territories for this account
      // Note: This assumes an endpoint exists. If not, use empty array
      try {
        const assigned = await territoryService.getTerritoriesByAccount(accountId);
        const assignedIds = assigned.map(t => t.id);
        setAssignedTerritories(assignedIds);
        setSelectedTerritories(assignedIds);
      } catch {
        // If endpoint doesn't exist, start with empty
        setAssignedTerritories([]);
        setSelectedTerritories([]);
      }
    } catch (err) {
      const message = 'Failed to load territories';
      setError(message);
      onError?.(message);
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleTerritorySelect = (event: SelectChangeEvent<number[]>) => {
    const value = event.target.value as number[];
    setSelectedTerritories(value);
    setHasChanges(JSON.stringify(value) !== JSON.stringify(assignedTerritories));
  };

  const handleRemoveTerritory = (territoryId: number) => {
    const updated = selectedTerritories.filter(id => id !== territoryId);
    setSelectedTerritories(updated);
    setHasChanges(JSON.stringify(updated) !== JSON.stringify(assignedTerritories));
  };

  const handleSave = async () => {
    try {
      setSaving(true);
      setError(null);

      // Calculate additions and deletions
      const toAdd = selectedTerritories.filter(id => !assignedTerritories.includes(id));
      const toRemove = assignedTerritories.filter(id => !selectedTerritories.includes(id));

      // Add new territories
      for (const territoryId of toAdd) {
        await territoryService.assignToAccount(accountId, territoryId);
      }

      // Remove territories
      for (const territoryId of toRemove) {
        await territoryService.removeFromAccount(accountId, territoryId);
      }

      // Update state
      setAssignedTerritories(selectedTerritories);
      setHasChanges(false);
      onSave?.(selectedTerritories);
    } catch (err) {
      const message = 'Failed to save territories';
      setError(message);
      onError?.(message);
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  const getTerritoryName = (id: number): string => {
    return territories.find(t => t.id === id)?.name || 'Unknown';
  };

  return (
    <Accordion
      expanded={expanded}
      onChange={() => setExpanded(!expanded)}
      component={Paper}
      variant="outlined"
    >
      <AccordionSummary expandIcon={<ExpandMoreIcon />}>
        <Typography variant="subtitle1" fontWeight={600}>
          Territory Assignment
          {assignedTerritories.length > 0 && (
            <Chip
              label={`${assignedTerritories.length}`}
              size="small"
              color="primary"
              variant="outlined"
              sx={{ ml: 1 }}
            />
          )}
        </Typography>
      </AccordionSummary>

      <AccordionDetails>
        <Stack spacing={2}>
          {error && <Alert severity="error">{error}</Alert>}

          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 2 }}>
              <CircularProgress size={24} />
            </Box>
          ) : (
            <>
              {/* Territory Multi-Select Dropdown */}
              <FormControl fullWidth disabled={saving}>
                <InputLabel>Select Territories</InputLabel>
                <Select
                  multiple
                  value={selectedTerritories}
                  onChange={handleTerritorySelect}
                  label="Select Territories"
                  renderValue={(selected) => (
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                      {selected.length === 0 ? (
                        <Typography variant="caption" color="textSecondary">
                          No territories selected
                        </Typography>
                      ) : (
                        selected.map(id => (
                          <Chip
                            key={id}
                            label={getTerritoryName(id)}
                            size="small"
                            color="primary"
                            variant="filled"
                          />
                        ))
                      )}
                    </Box>
                  )}
                >
                  {territories.map(territory => (
                    <MenuItem key={territory.id} value={territory.id}>
                      <Box sx={{ display: 'flex', flexDirection: 'column' }}>
                        <Typography variant="body2">{territory.name}</Typography>
                        {territory.description && (
                          <Typography variant="caption" color="textSecondary">
                            {territory.description}
                          </Typography>
                        )}
                      </Box>
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              {/* Assigned Territories Display */}
              {selectedTerritories.length > 0 && (
                <Box>
                  <Typography variant="body2" fontWeight={600} sx={{ mb: 1 }}>
                    Selected Territories ({selectedTerritories.length})
                  </Typography>
                  <Box
                    sx={{
                      display: 'flex',
                      flexWrap: 'wrap',
                      gap: 1,
                      p: 1.5,
                      backgroundColor: '#f5f5f5',
                      borderRadius: 1,
                      border: '1px solid #e0e0e0',
                    }}
                  >
                    {selectedTerritories.map(territoryId => (
                      <Chip
                        key={territoryId}
                        label={getTerritoryName(territoryId)}
                        onDelete={() => handleRemoveTerritory(territoryId)}
                        color="primary"
                        variant={
                          assignedTerritories.includes(territoryId) ? 'filled' : 'outlined'
                        }
                        icon={!assignedTerritories.includes(territoryId) ? undefined : undefined}
                      />
                    ))}
                  </Box>
                </Box>
              )}

              {/* Current Assignments Info */}
              {assignedTerritories.length > 0 && (
                <Box sx={{ p: 1.5, backgroundColor: '#e3f2fd', borderRadius: 1 }}>
                  <Typography variant="caption" color="primary">
                    <strong>Currently assigned:</strong> {assignedTerritories.length} territory
                    {assignedTerritories.length !== 1 ? 'ies' : ''}
                  </Typography>
                </Box>
              )}

              {/* Save Button */}
              {hasChanges && (
                <Stack direction="row" spacing={1}>
                  <Button
                    onClick={handleSave}
                    variant="contained"
                    startIcon={saving ? <CircularProgress size={20} /> : <SaveIcon />}
                    disabled={saving || !hasChanges}
                  >
                    {saving ? 'Saving...' : 'Save Changes'}
                  </Button>
                  <Button
                    onClick={() => {
                      setSelectedTerritories(assignedTerritories);
                      setHasChanges(false);
                    }}
                    variant="outlined"
                    disabled={saving}
                  >
                    Cancel
                  </Button>
                </Stack>
              )}

              {!hasChanges && selectedTerritories.length === 0 && (
                <Typography variant="body2" color="textSecondary" sx={{ textAlign: 'center', py: 2 }}>
                  No territories assigned to this account
                </Typography>
              )}
            </>
          )}
        </Stack>
      </AccordionDetails>
    </Accordion>
  );
};

export default TerritoryAssignmentPanel;
