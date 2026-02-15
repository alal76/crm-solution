/**
 * ChangeImpactAnalysisPanel - Editor for change impact analysis
 */

import React, { useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  TextField,
  Button,
  Stack,
  Alert,
  Card,
  CardContent,
  Divider,
  Chip,
  Grid,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  Edit as EditIcon,
} from '@mui/icons-material';

export interface ImpactItem {
  id: string;
  service: string;
  affectedComponents: string;
  estimatedImpact: string;
  severity: 'low' | 'medium' | 'high' | 'critical';
  notes?: string;
}

interface ChangeImpactAnalysisPanelProps {
  impacts: ImpactItem[];
  onChange: (impacts: ImpactItem[]) => void;
  readOnly?: boolean;
}

const severityColors: Record<string, 'default' | 'warning' | 'error' | 'info' | 'success'> = {
  low: 'success',
  medium: 'info',
  high: 'warning',
  critical: 'error',
};

export const ChangeImpactAnalysisPanel: React.FC<ChangeImpactAnalysisPanelProps> = ({
  impacts = [],
  onChange,
  readOnly = false,
}) => {
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editingItem, setEditingItem] = useState<ImpactItem | null>(null);
  const [newItem, setNewItem] = useState<Partial<ImpactItem>>({
    severity: 'medium',
  });

  const handleAddNew = () => {
    if (!newItem.service || !newItem.affectedComponents) {
      return;
    }

    const item: ImpactItem = {
      id: Date.now().toString(),
      service: newItem.service || '',
      affectedComponents: newItem.affectedComponents || '',
      estimatedImpact: newItem.estimatedImpact || '',
      severity: (newItem.severity || 'medium') as ImpactItem['severity'],
      notes: newItem.notes,
    };

    onChange([...impacts, item]);
    setNewItem({ severity: 'medium' });
  };

  const handleRemove = (id: string) => {
    onChange(impacts.filter((item) => item.id !== id));
  };

  const handleEdit = (item: ImpactItem) => {
    setEditingId(item.id);
    setEditingItem({ ...item });
  };

  const handleSaveEdit = () => {
    if (!editingItem) return;
    onChange(impacts.map((item) => (item.id === editingId ? editingItem : item)));
    setEditingId(null);
    setEditingItem(null);
  };

  return (
    <Box>
      <Typography variant="h6" sx={{ mb: 2, fontWeight: 'bold' }}>
        Impact Analysis
      </Typography>

      {impacts.length === 0 && (
        <Alert severity="info" sx={{ mb: 2 }}>
          No impacts documented yet
        </Alert>
      )}

      <Stack spacing={2} sx={{ mb: 3 }}>
        {impacts.map((item) => (
          <Card key={item.id}>
            <CardContent>
              {editingId === item.id ? (
                // Edit Mode
                <Stack spacing={2}>
                  <TextField
                    fullWidth
                    label="Service"
                    value={editingItem?.service || ''}
                    onChange={(e) =>
                      setEditingItem(editingItem ? { ...editingItem, service: e.target.value } : null)
                    }
                    size="small"
                  />
                  <TextField
                    fullWidth
                    label="Affected Components"
                    value={editingItem?.affectedComponents || ''}
                    onChange={(e) =>
                      setEditingItem(editingItem ? { ...editingItem, affectedComponents: e.target.value } : null)
                    }
                    size="small"
                  />
                  <TextField
                    fullWidth
                    label="Estimated Impact"
                    value={editingItem?.estimatedImpact || ''}
                    onChange={(e) =>
                      setEditingItem(editingItem ? { ...editingItem, estimatedImpact: e.target.value } : null)
                    }
                    size="small"
                  />
                  <TextField
                    fullWidth
                    label="Notes"
                    value={editingItem?.notes || ''}
                    onChange={(e) =>
                      setEditingItem(editingItem ? { ...editingItem, notes: e.target.value } : null)
                    }
                    multiline
                    rows={2}
                    size="small"
                  />
                  <Stack direction="row" spacing={1}>
                    <Button variant="contained" size="small" onClick={handleSaveEdit}>
                      Save
                    </Button>
                    <Button
                      variant="outlined"
                      size="small"
                      onClick={() => {
                        setEditingId(null);
                        setEditingItem(null);
                      }}
                    >
                      Cancel
                    </Button>
                  </Stack>
                </Stack>
              ) : (
                // View Mode
                <Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', mb: 1 }}>
                    <Box>
                      <Typography variant="subtitle2" fontWeight="bold">
                        {item.service}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        {item.affectedComponents}
                      </Typography>
                    </Box>
                    <Chip
                      label={item.severity.toUpperCase()}
                      size="small"
                      color={severityColors[item.severity]}
                      variant="filled"
                    />
                  </Box>
                  {item.estimatedImpact && (
                    <>
                      <Divider sx={{ my: 1 }} />
                      <Typography variant="body2">
                        <strong>Impact:</strong> {item.estimatedImpact}
                      </Typography>
                    </>
                  )}
                  {item.notes && (
                    <Typography variant="body2" sx={{ mt: 1, color: 'text.secondary' }}>
                      <strong>Notes:</strong> {item.notes}
                    </Typography>
                  )}
                  {!readOnly && (
                    <Stack direction="row" spacing={1} sx={{ mt: 2 }}>
                      <Button
                        size="small"
                        startIcon={<EditIcon />}
                        variant="outlined"
                        onClick={() => handleEdit(item)}
                      >
                        Edit
                      </Button>
                      <Button
                        size="small"
                        startIcon={<DeleteIcon />}
                        color="error"
                        variant="outlined"
                        onClick={() => handleRemove(item.id)}
                      >
                        Remove
                      </Button>
                    </Stack>
                  )}
                </Box>
              )}
            </CardContent>
          </Card>
        ))}
      </Stack>

      {!readOnly && (
        <>
          <Divider sx={{ my: 2 }} />
          <Typography variant="subtitle2" sx={{ mb: 2, fontWeight: 'bold' }}>
            Add New Impact
          </Typography>
          <Stack spacing={2}>
            <Grid container spacing={2}>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Service"
                  value={newItem.service || ''}
                  onChange={(e) => setNewItem({ ...newItem, service: e.target.value })}
                  size="small"
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Affected Components"
                  value={newItem.affectedComponents || ''}
                  onChange={(e) => setNewItem({ ...newItem, affectedComponents: e.target.value })}
                  size="small"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Estimated Impact"
                  value={newItem.estimatedImpact || ''}
                  onChange={(e) => setNewItem({ ...newItem, estimatedImpact: e.target.value })}
                  size="small"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Notes"
                  value={newItem.notes || ''}
                  onChange={(e) => setNewItem({ ...newItem, notes: e.target.value })}
                  multiline
                  rows={2}
                  size="small"
                />
              </Grid>
            </Grid>
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={handleAddNew}
              disabled={!newItem.service || !newItem.affectedComponents}
            >
              Add Impact
            </Button>
          </Stack>
        </>
      )}
    </Box>
  );
};

export default ChangeImpactAnalysisPanel;
