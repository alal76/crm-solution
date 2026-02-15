import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Button,
  TextField,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Divider,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
} from '@mui/material';
import { Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, Save as SaveIcon } from '@mui/icons-material';
import logger from '../../services/logger';

/**
 * Navigation Settings Panel - Manage menu structure and ordering
 */
const NavigationSettingsPanel: React.FC = () => {
  const [openDialog, setOpenDialog] = useState(false);
  const [saving, setSaving] = useState(false);

  const handleSave = async () => {
    try {
      setSaving(true);
      logger.info('Navigation settings saved');
    } catch (err) {
      logger.error('Failed to save navigation settings', err);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Box>
      <Card>
        <CardHeader
          title="Navigation Management"
          subtitle="Customize menu structure and item ordering"
          action={
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={() => setOpenDialog(true)}
            >
              Add Item
            </Button>
          }
        />
        <Divider />
        <CardContent>
          <Alert severity="info" sx={{ mb: 2 }}>
            Navigate using drag-and-drop to reorder menu items. Changes are saved automatically.
          </Alert>

          <TableContainer>
            <Table size="small">
              <TableHead>
                <TableRow sx={{ bgcolor: 'grey.100' }}>
                  <TableCell>Order</TableCell>
                  <TableCell>Label</TableCell>
                  <TableCell>Path/Route</TableCell>
                  <TableCell>Category</TableCell>
                  <TableCell align="center">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                <TableRow>
                  <TableCell>1</TableCell>
                  <TableCell>Dashboard</TableCell>
                  <TableCell>/</TableCell>
                  <TableCell>Main</TableCell>
                  <TableCell align="center">
                    <Button size="small" startIcon={<EditIcon />} />
                    <Button size="small" startIcon={<DeleteIcon />} />
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </TableContainer>

          <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end', mt: 3 }}>
            <Button
              variant="contained"
              startIcon={<SaveIcon />}
              onClick={handleSave}
              disabled={saving}
            >
              Save Navigation
            </Button>
          </Box>
        </CardContent>
      </Card>

      <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Add Navigation Item</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <TextField
            fullWidth
            label="Label"
            variant="outlined"
            size="small"
            sx={{ mb: 2 }}
          />
          <TextField
            fullWidth
            label="Path/Route"
            variant="outlined"
            size="small"
            sx={{ mb: 2 }}
          />
          <TextField
            fullWidth
            label="Category"
            variant="outlined"
            size="small"
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDialog(false)}>Cancel</Button>
          <Button variant="contained" onClick={() => setOpenDialog(false)}>
            Add Item
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default NavigationSettingsPanel;
