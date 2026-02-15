/**
 * CommissionPlanForm - Create and edit commission plans
 */

import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Stack,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Chip,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  Edit as EditIcon,
} from '@mui/icons-material';
import { CommissionType, CommissionTrigger } from '../../services/commissionService';

export interface CommissionTier {
  id: string;
  minAmount: number;
  maxAmount?: number;
  rate: number;
  bonus?: number;
}

export interface CommissionPlan {
  id?: number;
  name: string;
  description?: string;
  type: CommissionType;
  trigger: CommissionTrigger;
  baseRate: number;
  tiers: CommissionTier[];
  isActive: boolean;
  capAmount?: number;
  quotaAmount?: number;
}

interface CommissionPlanFormProps {
  plan?: CommissionPlan;
  onSave: (plan: CommissionPlan) => Promise<void>;
  loading?: boolean;
}

export const CommissionPlanForm: React.FC<CommissionPlanFormProps> = ({
  plan,
  onSave,
  loading = false,
}) => {
  const [formData, setFormData] = useState<CommissionPlan>(
    plan || {
      name: '',
      type: CommissionType.FlatPercentage,
      trigger: CommissionTrigger.OnClose,
      baseRate: 0,
      tiers: [],
      isActive: true,
    }
  );
  const [editingTier, setEditingTier] = useState<CommissionTier | null>(null);
  const [tierDialogOpen, setTierDialogOpen] = useState(false);

  const handleAddTier = () => {
    const newTier: CommissionTier = {
      id: Date.now().toString(),
      minAmount: 0,
      rate: 0,
    };
    setEditingTier(newTier);
    setTierDialogOpen(true);
  };

  const handleSaveTier = () => {
    if (!editingTier) return;

    if (editingTier.id.includes('new')) {
      setFormData({
        ...formData,
        tiers: [...formData.tiers, editingTier],
      });
    } else {
      setFormData({
        ...formData,
        tiers: formData.tiers.map((t) => (t.id === editingTier.id ? editingTier : t)),
      });
    }

    setEditingTier(null);
    setTierDialogOpen(false);
  };

  const handleRemoveTier = (id: string) => {
    setFormData({
      ...formData,
      tiers: formData.tiers.filter((t) => t.id !== id),
    });
  };

  const handleSave = async () => {
    await onSave(formData);
  };

  return (
    <Box>
      <Card>
        <CardContent>
          <Stack spacing={2}>
            <TextField
              fullWidth
              label="Plan Name"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              disabled={loading}
            />

            <TextField
              fullWidth
              label="Description"
              value={formData.description || ''}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              multiline
              rows={2}
              disabled={loading}
            />

            <FormControl fullWidth disabled={loading}>
              <InputLabel>Commission Type</InputLabel>
              <Select
                value={formData.type}
                onChange={(e) => setFormData({ ...formData, type: e.target.value as CommissionType })}
                label="Commission Type"
              >
                <MenuItem value={CommissionType.FlatPercentage}>Flat Percentage</MenuItem>
                <MenuItem value={CommissionType.TieredPercentage}>Tiered Percentage</MenuItem>
                <MenuItem value={CommissionType.FixedAmount}>Fixed Amount</MenuItem>
                <MenuItem value={CommissionType.TieredAmount}>Tiered Amount</MenuItem>
                <MenuItem value={CommissionType.MarginBased}>Margin Based</MenuItem>
              </Select>
            </FormControl>

            <FormControl fullWidth disabled={loading}>
              <InputLabel>Trigger Event</InputLabel>
              <Select
                value={formData.trigger}
                onChange={(e) => setFormData({ ...formData, trigger: e.target.value as CommissionTrigger })}
                label="Trigger Event"
              >
                <MenuItem value={CommissionTrigger.OnClose}>On Close</MenuItem>
                <MenuItem value={CommissionTrigger.OnOrder}>On Order</MenuItem>
                <MenuItem value={CommissionTrigger.OnInvoice}>On Invoice</MenuItem>
                <MenuItem value={CommissionTrigger.OnPayment}>On Payment</MenuItem>
              </Select>
            </FormControl>

            <TextField
              fullWidth
              type="number"
              label="Base Rate (%)"
              value={formData.baseRate}
              onChange={(e) => setFormData({ ...formData, baseRate: parseFloat(e.target.value) })}
              disabled={loading}
              inputProps={{ step: 0.01 }}
            />

            <TextField
              fullWidth
              type="number"
              label="Cap Amount (optional)"
              value={formData.capAmount || ''}
              onChange={(e) => setFormData({ ...formData, capAmount: e.target.value ? parseFloat(e.target.value) : undefined })}
              disabled={loading}
            />

            <TextField
              fullWidth
              type="number"
              label="Quota Amount (optional)"
              value={formData.quotaAmount || ''}
              onChange={(e) => setFormData({ ...formData, quotaAmount: e.target.value ? parseFloat(e.target.value) : undefined })}
              disabled={loading}
            />
          </Stack>
        </CardContent>
      </Card>

      {/* Tiers Section */}
      {formData.type.toString().includes('Tiered') && (
        <Box sx={{ mt: 3 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <h3>Commission Tiers</h3>
            <Button
              startIcon={<AddIcon />}
              onClick={handleAddTier}
              variant="contained"
              size="small"
              disabled={loading}
            >
              Add Tier
            </Button>
          </Box>

          <TableContainer component={Paper}>
            <Table>
              <TableHead>
                <TableRow sx={{ bgcolor: 'action.hover' }}>
                  <TableCell>Min Amount</TableCell>
                  <TableCell>Max Amount</TableCell>
                  <TableCell>Rate (%)</TableCell>
                  <TableCell>Bonus</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {formData.tiers.map((tier) => (
                  <TableRow key={tier.id}>
                    <TableCell>${tier.minAmount}</TableCell>
                    <TableCell>{tier.maxAmount ? `$${tier.maxAmount}` : 'Unlimited'}</TableCell>
                    <TableCell>{tier.rate}%</TableCell>
                    <TableCell>{tier.bonus || '-'}</TableCell>
                    <TableCell align="right">
                      <IconButton
                        size="small"
                        onClick={() => {
                          setEditingTier(tier);
                          setTierDialogOpen(true);
                        }}
                        disabled={loading}
                      >
                        <EditIcon />
                      </IconButton>
                      <IconButton
                        size="small"
                        color="error"
                        onClick={() => handleRemoveTier(tier.id)}
                        disabled={loading}
                      >
                        <DeleteIcon />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </Box>
      )}

      {/* Tier Dialog */}
      <Dialog open={tierDialogOpen} onClose={() => setTierDialogOpen(false)}>
        <DialogTitle>Edit Tier</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <Stack spacing={2}>
            <TextField
              fullWidth
              type="number"
              label="Minimum Amount"
              value={editingTier?.minAmount || ''}
              onChange={(e) =>
                setEditingTier(editingTier ? { ...editingTier, minAmount: parseFloat(e.target.value) } : null)
              }
            />
            <TextField
              fullWidth
              type="number"
              label="Maximum Amount (optional)"
              value={editingTier?.maxAmount || ''}
              onChange={(e) =>
                setEditingTier(editingTier ? { ...editingTier, maxAmount: e.target.value ? parseFloat(e.target.value) : undefined } : null)
              }
            />
            <TextField
              fullWidth
              type="number"
              label="Rate (%)"
              value={editingTier?.rate || ''}
              onChange={(e) =>
                setEditingTier(editingTier ? { ...editingTier, rate: parseFloat(e.target.value) } : null)
              }
            />
            <TextField
              fullWidth
              type="number"
              label="Bonus (optional)"
              value={editingTier?.bonus || ''}
              onChange={(e) =>
                setEditingTier(editingTier ? { ...editingTier, bonus: e.target.value ? parseFloat(e.target.value) : undefined } : null)
              }
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setTierDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleSaveTier} variant="contained">
            Save
          </Button>
        </DialogActions>
      </Dialog>

      {/* Save Button */}
      <Button
        onClick={handleSave}
        variant="contained"
        fullWidth
        sx={{ mt: 3 }}
        disabled={loading || !formData.name}
      >
        Save Commission Plan
      </Button>
    </Box>
  );
};

export default CommissionPlanForm;
