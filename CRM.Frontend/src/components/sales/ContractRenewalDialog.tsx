/**
 * ContractRenewalDialog - Dialog for renewing an expiring contract
 * with value adjustment and date selection.
 */

import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Box,
  Typography,
  Card,
  CardContent,
  CircularProgress,
  Slider,
  InputAdornment,
} from '@mui/material';

export interface RenewalData {
  newStartDate: string;
  newEndDate: string;
  newValue: number;
  adjustmentPercent?: number;
  notes?: string;
}

interface ContractRenewalDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: RenewalData) => void;
  currentContract: {
    id: number;
    name: string;
    endDate: string;
    totalValue: number;
  };
  loading?: boolean;
}

const formatCurrency = (value: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);

const addYears = (dateStr: string, years: number): string => {
  const d = new Date(dateStr);
  d.setFullYear(d.getFullYear() + years);
  return d.toISOString().split('T')[0];
};

const ContractRenewalDialog: React.FC<ContractRenewalDialogProps> = ({
  open,
  onClose,
  onSubmit,
  currentContract,
  loading = false,
}) => {
  const [newStartDate, setNewStartDate] = useState('');
  const [newEndDate, setNewEndDate] = useState('');
  const [adjustmentPercent, setAdjustmentPercent] = useState(0);
  const [notes, setNotes] = useState('');

  const newValue = Math.round(currentContract.totalValue * (1 + adjustmentPercent / 100) * 100) / 100;

  useEffect(() => {
    if (open) {
      const start = currentContract.endDate.split('T')[0];
      setNewStartDate(start);
      setNewEndDate(addYears(start, 1));
      setAdjustmentPercent(0);
      setNotes('');
    }
  }, [open, currentContract]);

  const isValid = newStartDate !== '' && newEndDate !== '' && newEndDate > newStartDate;

  const handleSubmit = () => {
    onSubmit({
      newStartDate,
      newEndDate,
      newValue,
      adjustmentPercent: adjustmentPercent !== 0 ? adjustmentPercent : undefined,
      notes: notes || undefined,
    });
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Renew Contract</DialogTitle>
      <DialogContent dividers>
        <Box display="flex" flexDirection="column" gap={2.5} mt={1}>
          {/* Current contract summary */}
          <Card variant="outlined">
            <CardContent sx={{ pb: '12px !important' }}>
              <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                Current Contract
              </Typography>
              <Typography variant="body1" fontWeight={600}>{currentContract.name}</Typography>
              <Typography variant="body2">
                Expires: {new Date(currentContract.endDate).toLocaleDateString()}
              </Typography>
              <Typography variant="body2">
                Value: {formatCurrency(currentContract.totalValue)}
              </Typography>
            </CardContent>
          </Card>

          {/* New date range */}
          <Box display="flex" gap={2}>
            <TextField
              label="New Start Date"
              type="date"
              value={newStartDate}
              onChange={(e) => setNewStartDate(e.target.value)}
              InputLabelProps={{ shrink: true }}
              fullWidth
            />
            <TextField
              label="New End Date"
              type="date"
              value={newEndDate}
              onChange={(e) => setNewEndDate(e.target.value)}
              InputLabelProps={{ shrink: true }}
              fullWidth
              error={newEndDate !== '' && newEndDate <= newStartDate}
              helperText={newEndDate !== '' && newEndDate <= newStartDate ? 'Must be after start date' : undefined}
            />
          </Box>

          {/* Value adjustment */}
          <Box>
            <Typography variant="body2" gutterBottom>
              Value Adjustment: {adjustmentPercent > 0 ? '+' : ''}{adjustmentPercent}%
            </Typography>
            <Slider
              value={adjustmentPercent}
              onChange={(_, val) => setAdjustmentPercent(val as number)}
              min={-50}
              max={50}
              step={1}
              marks={[
                { value: -50, label: '-50%' },
                { value: 0, label: '0%' },
                { value: 50, label: '+50%' },
              ]}
              valueLabelDisplay="auto"
              valueLabelFormat={(v) => `${v > 0 ? '+' : ''}${v}%`}
            />
            <TextField
              label="New Contract Value"
              type="number"
              value={newValue}
              InputProps={{
                readOnly: true,
                startAdornment: <InputAdornment position="start">$</InputAdornment>,
              }}
              fullWidth
              size="small"
            />
          </Box>

          <TextField
            label="Notes"
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            multiline
            minRows={2}
            fullWidth
          />

          {/* Summary card */}
          <Card variant="outlined" sx={{ bgcolor: 'action.hover' }}>
            <CardContent sx={{ pb: '12px !important' }}>
              <Typography variant="subtitle2" gutterBottom>Renewal Summary</Typography>
              <Typography variant="body2">
                Period: {newStartDate} → {newEndDate}
              </Typography>
              <Typography variant="body2">
                Value: {formatCurrency(currentContract.totalValue)} → {formatCurrency(newValue)}
                {adjustmentPercent !== 0 && (
                  <Typography component="span" color={adjustmentPercent > 0 ? 'error.main' : 'success.main'}>
                    {' '}({adjustmentPercent > 0 ? '+' : ''}{adjustmentPercent}%)
                  </Typography>
                )}
              </Typography>
            </CardContent>
          </Card>
        </Box>
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose} disabled={loading}>Cancel</Button>
        <Button
          variant="contained"
          onClick={handleSubmit}
          disabled={!isValid || loading}
          startIcon={loading ? <CircularProgress size={18} /> : undefined}
        >
          Renew Contract
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default ContractRenewalDialog;
