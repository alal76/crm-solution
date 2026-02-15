import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  FormControl,
  FormControlLabel,
  FormLabel,
  RadioGroup,
  Radio,
  Select,
  MenuItem,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Alert,
  CircularProgress,
  Stack,
  Chip,
  Typography,
  Divider,
} from '@mui/material';
import MergeIcon from '@mui/icons-material/Merge';
import accountService from '../../../services/accountService';

interface AccountMergeDialogProps {
  open: boolean;
  onClose: () => void;
  onSuccess: (mergedAccount: any) => void;
  selectedAccounts?: number[];
}

interface Account {
  id: number;
  firstName?: string;
  lastName?: string;
  company?: string;
  email?: string;
}

interface MergePreview {
  fieldName: string;
  leftValue: string | null;
  rightValue: string | null;
  survivor: 'left' | 'right';
}

/**
 * AccountMergeDialog Component
 * Allows users to merge two accounts by selecting:
 * - Two accounts to merge
 * - Which account survives (keeps its ID)
 * - Field-by-field preview of which values will be kept
 */
export const AccountMergeDialog: React.FC<AccountMergeDialogProps> = ({
  open,
  onClose,
  onSuccess,
  selectedAccounts = [],
}) => {
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [leftAccountId, setLeftAccountId] = useState<number | ''>('');
  const [rightAccountId, setRightAccountId] = useState<number | ''>('');
  const [survivor, setSurvivor] = useState<'left' | 'right'>('left');
  const [mergePreview, setMergePreview] = useState<MergePreview[]>([]);
  const [loading, setLoading] = useState(false);
  const [merging, setMerging] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Fetch all accounts on mount
  useEffect(() => {
    if (open) {
      loadAccounts();
      // Pre-populate with selected accounts if available
      if (selectedAccounts.length >= 2) {
        setLeftAccountId(selectedAccounts[0]);
        setRightAccountId(selectedAccounts[1]);
      }
    }
  }, [open, selectedAccounts]);

  // Update merge preview when accounts or survivor change
  useEffect(() => {
    if (leftAccountId && rightAccountId && leftAccountId !== rightAccountId) {
      generateMergePreview();
    }
  }, [leftAccountId, rightAccountId, survivor]);

  const loadAccounts = async () => {
    try {
      setLoading(true);
      const response = await accountService.getAll();
      setAccounts(response);
      setError(null);
    } catch (err) {
      setError('Failed to load accounts');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const generateMergePreview = async () => {
    if (!leftAccountId || !rightAccountId) return;

    try {
      const leftAccount = accounts.find(a => a.id === leftAccountId);
      const rightAccount = accounts.find(a => a.id === rightAccountId);

      if (!leftAccount || !rightAccount) return;

      const preview: MergePreview[] = [
        {
          fieldName: 'First Name',
          leftValue: leftAccount.firstName || null,
          rightValue: rightAccount.firstName || null,
          survivor,
        },
        {
          fieldName: 'Last Name',
          leftValue: leftAccount.lastName || null,
          rightValue: rightAccount.lastName || null,
          survivor,
        },
        {
          fieldName: 'Company',
          leftValue: leftAccount.company || null,
          rightValue: rightAccount.company || null,
          survivor,
        },
        {
          fieldName: 'Email',
          leftValue: leftAccount.email || null,
          rightValue: rightAccount.email || null,
          survivor,
        },
      ];

      setMergePreview(preview);
    } catch (err) {
      console.error('Error generating merge preview:', err);
    }
  };

  const handleMerge = async () => {
    if (!leftAccountId || !rightAccountId) {
      setError('Please select two different accounts');
      return;
    }

    try {
      setMerging(true);
      setError(null);

      const survivorId = survivor === 'left' ? leftAccountId : rightAccountId;
      const mergeId = survivor === 'left' ? rightAccountId : leftAccountId;

      const response = await accountService.merge(survivorId as number, mergeId as number);

      onSuccess(response);
      handleClose();
    } catch (err) {
      setError('Failed to merge accounts. Please try again.');
      console.error(err);
    } finally {
      setMerging(false);
    }
  };

  const handleClose = () => {
    setLeftAccountId('');
    setRightAccountId('');
    setSurvivor('left');
    setMergePreview([]);
    setError(null);
    onClose();
  };

  const getKeptValue = (preview: MergePreview): string => {
    return preview.survivor === 'left'
      ? preview.leftValue || preview.rightValue || '(empty)'
      : preview.rightValue || preview.leftValue || '(empty)';
  };

  const getAccountDisplay = (accountId: number | ''): string => {
    if (!accountId) return '';
    const account = accounts.find(a => a.id === accountId);
    if (!account) return '';
    return `${account.firstName || ''} ${account.lastName || ''} (${account.company || account.email || 'N/A'})`.trim();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="md" fullWidth>
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <MergeIcon />
        Merge Accounts
      </DialogTitle>

      <DialogContent>
        <Stack spacing={3} sx={{ pt: 2 }}>
          {error && <Alert severity="error">{error}</Alert>}

          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
              <CircularProgress />
            </Box>
          ) : (
            <>
              {/* Account Selection */}
              <Box>
                <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 2 }}>
                  Select Accounts to Merge
                </Typography>
                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                  <FormControl fullWidth size="small">
                    <Select
                      value={leftAccountId}
                      onChange={(e) => setLeftAccountId(e.target.value as number)}
                      displayEmpty
                      disabled={merging}
                    >
                      <MenuItem value="">Select account...</MenuItem>
                      {accounts
                        .filter(a => a.id !== rightAccountId)
                        .map(account => (
                          <MenuItem key={account.id} value={account.id}>
                            {getAccountDisplay(account.id)}
                          </MenuItem>
                        ))}
                    </Select>
                  </FormControl>

                  <FormControl fullWidth size="small">
                    <Select
                      value={rightAccountId}
                      onChange={(e) => setRightAccountId(e.target.value as number)}
                      displayEmpty
                      disabled={merging}
                    >
                      <MenuItem value="">Select account...</MenuItem>
                      {accounts
                        .filter(a => a.id !== leftAccountId)
                        .map(account => (
                          <MenuItem key={account.id} value={account.id}>
                            {getAccountDisplay(account.id)}
                          </MenuItem>
                        ))}
                    </Select>
                  </FormControl>
                </Stack>
              </Box>

              {leftAccountId && rightAccountId && (
                <>
                  <Divider />

                  {/* Survivor Selection */}
                  <Box>
                    <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 1 }}>
                      Which account should survive?
                    </Typography>
                    <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
                      The surviving account keeps its ID. The other account is deleted.
                    </Typography>
                    <FormControl disabled={merging}>
                      <RadioGroup
                        value={survivor}
                        onChange={(e) => setSurvivor(e.target.value as 'left' | 'right')}
                      >
                        <FormControlLabel
                          value="left"
                          control={<Radio />}
                          label={
                            <Stack>
                              <span>Keep Left Account</span>
                              <Typography variant="caption" color="textSecondary">
                                {getAccountDisplay(leftAccountId as number)}
                              </Typography>
                            </Stack>
                          }
                        />
                        <FormControlLabel
                          value="right"
                          control={<Radio />}
                          label={
                            <Stack>
                              <span>Keep Right Account</span>
                              <Typography variant="caption" color="textSecondary">
                                {getAccountDisplay(rightAccountId as number)}
                              </Typography>
                            </Stack>
                          }
                        />
                      </RadioGroup>
                    </FormControl>
                  </Box>

                  <Divider />

                  {/* Merge Preview Table */}
                  {mergePreview.length > 0 && (
                    <Box>
                      <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 2 }}>
                        Preview: Which values will be kept
                      </Typography>
                      <TableContainer component={Paper} variant="outlined">
                        <Table size="small">
                          <TableHead>
                            <TableRow sx={{ backgroundColor: '#f5f5f5' }}>
                              <TableCell>Field</TableCell>
                              <TableCell align="center">Left Account</TableCell>
                              <TableCell align="center">Right Account</TableCell>
                              <TableCell align="center">
                                <Chip
                                  label={survivor === 'left' ? 'Will Keep (L)' : 'Will Keep (R)'}
                                  size="small"
                                  color="primary"
                                  variant="outlined"
                                />
                              </TableCell>
                            </TableRow>
                          </TableHead>
                          <TableBody>
                            {mergePreview.map((row, idx) => (
                              <TableRow key={idx}>
                                <TableCell>
                                  <Typography variant="body2" fontWeight={500}>
                                    {row.fieldName}
                                  </Typography>
                                </TableCell>
                                <TableCell align="center">
                                  <Typography
                                    variant="body2"
                                    sx={{
                                      backgroundColor:
                                        survivor === 'left' ? '#e3f2fd' : 'transparent',
                                      padding: '4px 8px',
                                      borderRadius: '4px',
                                    }}
                                  >
                                    {row.leftValue || '—'}
                                  </Typography>
                                </TableCell>
                                <TableCell align="center">
                                  <Typography
                                    variant="body2"
                                    sx={{
                                      backgroundColor:
                                        survivor === 'right' ? '#e3f2fd' : 'transparent',
                                      padding: '4px 8px',
                                      borderRadius: '4px',
                                    }}
                                  >
                                    {row.rightValue || '—'}
                                  </Typography>
                                </TableCell>
                                <TableCell align="center">
                                  <Chip
                                    label={getKeptValue(row)}
                                    size="small"
                                    color={survivor === 'left' ? 'primary' : 'default'}
                                  />
                                </TableCell>
                              </TableRow>
                            ))}
                          </TableBody>
                        </Table>
                      </TableContainer>
                    </Box>
                  )}
                </>
              )}
            </>
          )}
        </Stack>
      </DialogContent>

      <DialogActions sx={{ p: 2 }}>
        <Button onClick={handleClose} disabled={merging}>
          Cancel
        </Button>
        <Button
          onClick={handleMerge}
          variant="contained"
          color="primary"
          disabled={!leftAccountId || !rightAccountId || merging}
          startIcon={merging ? <CircularProgress size={20} /> : <MergeIcon />}
        >
          {merging ? 'Merging...' : 'Merge Accounts'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default AccountMergeDialog;
