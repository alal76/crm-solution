/**
 * ContractForm - Standalone contract create/edit form
 * TODO-SALES005-003 (P2)
 *
 * Uses Formik + Yup for validation.
 * Fields: title, accountId, startDate, endDate, value, status,
 *         description, terms, autoRenew, renewalNoticeDays
 */

import React from 'react';
import {
  Box,
  Button,
  CircularProgress,
  Divider,
  FormControl,
  FormControlLabel,
  FormHelperText,
  Grid,
  InputAdornment,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Switch,
  TextField,
  Typography,
} from '@mui/material';
import { useFormik } from 'formik';
import * as Yup from 'yup';

// ─── Types ────────────────────────────────────────────────────────────────────

export type ContractStatus =
  | 'Draft'
  | 'Active'
  | 'Expired'
  | 'Terminated'
  | 'Renewed';

export interface ContractFormValues {
  title: string;
  accountId: number | '';
  startDate: string;
  endDate: string;
  value: number | '';
  status: ContractStatus;
  description?: string;
  terms?: string;
  autoRenew: boolean;
  renewalNoticeDays?: number | '';
}

export interface Contract {
  id?: number;
  title?: string;
  accountId?: number;
  startDate?: string;
  endDate?: string;
  value?: number;
  status?: ContractStatus;
  description?: string;
  terms?: string;
  autoRenew?: boolean;
  renewalNoticeDays?: number;
}

export interface ContractFormProps {
  initialValues?: Partial<Contract>;
  onSubmit: (values: ContractFormValues) => Promise<void>;
  onCancel?: () => void;
  isCreate?: boolean;
  loading?: boolean;
}

// ─── Constants ────────────────────────────────────────────────────────────────

const CONTRACT_STATUSES: ContractStatus[] = [
  'Draft',
  'Active',
  'Expired',
  'Terminated',
  'Renewed',
];

const DEFAULT_VALUES: ContractFormValues = {
  title: '',
  accountId: '',
  startDate: '',
  endDate: '',
  value: '',
  status: 'Draft',
  description: '',
  terms: '',
  autoRenew: false,
  renewalNoticeDays: '',
};

// ─── Validation schema ────────────────────────────────────────────────────────

const validationSchema = Yup.object({
  title: Yup.string().trim().required('Title is required'),
  accountId: Yup.number()
    .typeError('Account is required')
    .positive('Account is required')
    .required('Account is required'),
  startDate: Yup.string().required('Start date is required'),
  endDate: Yup.string()
    .required('End date is required')
    .test('after-start', 'End date must be after start date', function (endDate) {
      const { startDate } = this.parent as { startDate: string };
      if (!startDate || !endDate) return true;
      return new Date(endDate) > new Date(startDate);
    }),
  value: Yup.number()
    .typeError('Value must be a number')
    .min(0, 'Value must be 0 or greater')
    .required('Contract value is required'),
  status: Yup.string().required('Status is required'),
  renewalNoticeDays: Yup.number()
    .nullable()
    .when('autoRenew', {
      is: true,
      then: (schema) =>
        schema
          .typeError('Must be a number')
          .min(1, 'Must be at least 1 day')
          .required('Required when auto-renew is enabled'),
    }),
});

// ─── Helper ───────────────────────────────────────────────────────────────────

function buildInitialValues(partial?: Partial<Contract>): ContractFormValues {
  if (!partial) return DEFAULT_VALUES;
  return {
    title: partial.title ?? '',
    accountId: partial.accountId ?? '',
    startDate: partial.startDate ?? '',
    endDate: partial.endDate ?? '',
    value: partial.value ?? '',
    status: partial.status ?? 'Draft',
    description: partial.description ?? '',
    terms: partial.terms ?? '',
    autoRenew: partial.autoRenew ?? false,
    renewalNoticeDays: partial.renewalNoticeDays ?? '',
  };
}

// ─── Component ────────────────────────────────────────────────────────────────

const ContractForm: React.FC<ContractFormProps> = ({
  initialValues,
  onSubmit,
  onCancel,
  isCreate = true,
  loading = false,
}) => {
  const formik = useFormik<ContractFormValues>({
    initialValues: buildInitialValues(initialValues),
    validationSchema,
    onSubmit: async (values) => {
      await onSubmit(values);
    },
  });

  const isSubmitting = loading || formik.isSubmitting;

  return (
    <Paper variant="outlined" sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom>
        {isCreate ? 'New Contract' : 'Edit Contract'}
      </Typography>
      <Divider sx={{ mb: 3 }} />

      <Box component="form" onSubmit={formik.handleSubmit} noValidate>
        <Grid container spacing={2.5}>

          {/* Title */}
          <Grid item xs={12}>
            <TextField
              required
              fullWidth
              size="small"
              id="title"
              name="title"
              label="Contract Title"
              value={formik.values.title}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              error={formik.touched.title && Boolean(formik.errors.title)}
              helperText={formik.touched.title && formik.errors.title}
            />
          </Grid>

          {/* Account ID */}
          <Grid item xs={12} sm={6}>
            <TextField
              required
              fullWidth
              size="small"
              id="accountId"
              name="accountId"
              label="Account ID"
              type="number"
              value={formik.values.accountId}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              error={formik.touched.accountId && Boolean(formik.errors.accountId)}
              helperText={
                (formik.touched.accountId && formik.errors.accountId) ||
                'Enter the numeric account ID'
              }
              inputProps={{ min: 1 }}
            />
          </Grid>

          {/* Status */}
          <Grid item xs={12} sm={6}>
            <FormControl
              fullWidth
              size="small"
              required
              error={formik.touched.status && Boolean(formik.errors.status)}
            >
              <InputLabel id="status-label">Status</InputLabel>
              <Select
                labelId="status-label"
                id="status"
                name="status"
                label="Status"
                value={formik.values.status}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
              >
                {CONTRACT_STATUSES.map((s) => (
                  <MenuItem key={s} value={s}>
                    {s}
                  </MenuItem>
                ))}
              </Select>
              {formik.touched.status && formik.errors.status && (
                <FormHelperText>{formik.errors.status}</FormHelperText>
              )}
            </FormControl>
          </Grid>

          {/* Start Date */}
          <Grid item xs={12} sm={6}>
            <TextField
              required
              fullWidth
              size="small"
              id="startDate"
              name="startDate"
              label="Start Date"
              type="date"
              value={formik.values.startDate}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              error={formik.touched.startDate && Boolean(formik.errors.startDate)}
              helperText={formik.touched.startDate && formik.errors.startDate}
              InputLabelProps={{ shrink: true }}
            />
          </Grid>

          {/* End Date */}
          <Grid item xs={12} sm={6}>
            <TextField
              required
              fullWidth
              size="small"
              id="endDate"
              name="endDate"
              label="End Date"
              type="date"
              value={formik.values.endDate}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              error={formik.touched.endDate && Boolean(formik.errors.endDate)}
              helperText={formik.touched.endDate && formik.errors.endDate}
              InputLabelProps={{ shrink: true }}
              inputProps={{ min: formik.values.startDate || undefined }}
            />
          </Grid>

          {/* Value */}
          <Grid item xs={12} sm={6}>
            <TextField
              required
              fullWidth
              size="small"
              id="value"
              name="value"
              label="Contract Value"
              type="number"
              value={formik.values.value}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              error={formik.touched.value && Boolean(formik.errors.value)}
              helperText={formik.touched.value && formik.errors.value}
              InputProps={{
                startAdornment: <InputAdornment position="start">$</InputAdornment>,
              }}
              inputProps={{ min: 0, step: '0.01' }}
            />
          </Grid>

          {/* Description */}
          <Grid item xs={12}>
            <TextField
              fullWidth
              size="small"
              id="description"
              name="description"
              label="Description"
              multiline
              minRows={3}
              value={formik.values.description}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              error={formik.touched.description && Boolean(formik.errors.description)}
              helperText={formik.touched.description && formik.errors.description}
            />
          </Grid>

          {/* Terms */}
          <Grid item xs={12}>
            <TextField
              fullWidth
              size="small"
              id="terms"
              name="terms"
              label="Terms & Conditions"
              multiline
              minRows={4}
              value={formik.values.terms}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              error={formik.touched.terms && Boolean(formik.errors.terms)}
              helperText={formik.touched.terms && formik.errors.terms}
            />
          </Grid>

          {/* Auto Renew */}
          <Grid item xs={12}>
            <FormControlLabel
              control={
                <Switch
                  id="autoRenew"
                  name="autoRenew"
                  checked={formik.values.autoRenew}
                  onChange={formik.handleChange}
                  size="small"
                />
              }
              label="Auto-Renew"
            />
          </Grid>

          {/* Renewal Notice Days – shown only when autoRenew is true */}
          {formik.values.autoRenew && (
            <Grid item xs={12} sm={6}>
              <TextField
                required
                fullWidth
                size="small"
                id="renewalNoticeDays"
                name="renewalNoticeDays"
                label="Renewal Notice (days)"
                type="number"
                value={formik.values.renewalNoticeDays}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
                error={
                  formik.touched.renewalNoticeDays &&
                  Boolean(formik.errors.renewalNoticeDays)
                }
                helperText={
                  (formik.touched.renewalNoticeDays && formik.errors.renewalNoticeDays) ||
                  'Days before expiry to send renewal notice'
                }
                inputProps={{ min: 1 }}
              />
            </Grid>
          )}
        </Grid>

        {/* Actions */}
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'flex-end',
            gap: 1.5,
            mt: 4,
            pt: 2,
            borderTop: 1,
            borderColor: 'divider',
          }}
        >
          {onCancel && (
            <Button variant="outlined" onClick={onCancel} disabled={isSubmitting}>
              Cancel
            </Button>
          )}
          <Button
            type="submit"
            variant="contained"
            disabled={isSubmitting}
            startIcon={isSubmitting ? <CircularProgress size={16} color="inherit" /> : undefined}
          >
            {isSubmitting ? 'Saving…' : isCreate ? 'Create Contract' : 'Save Changes'}
          </Button>
        </Box>
      </Box>
    </Paper>
  );
};

export default ContractForm;
