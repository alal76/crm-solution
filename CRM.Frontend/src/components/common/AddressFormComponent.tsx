/**
 * Address Form Component
 * Formik-based component for creating and editing addresses
 */
import React, { useEffect } from 'react';
import {
  Box,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormControlLabel,
  Checkbox,
  Button,
  Grid,
  CircularProgress,
  Alert,
} from '@mui/material';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { Address, CreateAddressDto, UpdateAddressDto, ADDRESS_TYPES } from '../../types/address.types';

const addressValidationSchema = Yup.object({
  line1: Yup.string().required('Address Line 1 is required'),
  line2: Yup.string().optional(),
  city: Yup.string().required('City is required'),
  state: Yup.string().optional(),
  zipCode: Yup.string().optional(),
  country: Yup.string().required('Country is required'),
  label: Yup.string().optional(),
  addressType: Yup.string()
    .oneOf(['Billing', 'Shipping', 'Primary', 'Other'])
    .required('Address Type is required'),
  isPrimary: Yup.boolean().optional(),
});

export interface AddressFormComponentProps {
  address?: Address;
  onSubmit: (values: CreateAddressDto | UpdateAddressDto) => Promise<void>;
  onCancel?: () => void;
  isLoading?: boolean;
  error?: string | null;
}

const AddressFormComponent: React.FC<AddressFormComponentProps> = ({
  address,
  onSubmit,
  onCancel,
  isLoading = false,
  error = null,
}) => {
  const isEditMode = !!address?.id;

  const initialValues: CreateAddressDto | UpdateAddressDto = address
    ? {
        line1: address.line1 || '',
        line2: address.line2 || '',
        city: address.city || '',
        state: address.state || '',
        zipCode: address.zipCode || '',
        country: address.country || '',
        label: address.label || '',
        addressType: address.addressType || 'Other',
        isPrimary: address.isPrimary || false,
      }
    : {
        line1: '',
        line2: '',
        city: '',
        state: '',
        zipCode: '',
        country: '',
        label: '',
        addressType: 'Other',
        isPrimary: false,
      };

  return (
    <Formik
      initialValues={initialValues}
      validationSchema={addressValidationSchema}
      onSubmit={async (values) => {
        try {
          await onSubmit(values);
        } catch (err) {
          console.error('Form submission error:', err);
        }
      }}
      enableReinitialize
    >
      {({ values, errors, touched, handleChange, handleBlur, isSubmitting }) => (
        <Form>
          {error && <Alert severity="error">{error}</Alert>}

          <Box sx={{ mt: 2 }}>
            <Grid container spacing={2}>
              {/* Line 1 */}
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  name="line1"
                  label="Address Line 1"
                  value={values.line1}
                  onChange={handleChange}
                  onBlur={handleBlur}
                  error={touched.line1 && !!errors.line1}
                  helperText={touched.line1 && errors.line1}
                  disabled={isLoading}
                  placeholder="Street address"
                />
              </Grid>

              {/* Line 2 */}
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  name="line2"
                  label="Address Line 2"
                  value={values.line2}
                  onChange={handleChange}
                  onBlur={handleBlur}
                  error={touched.line2 && !!errors.line2}
                  helperText={touched.line2 && errors.line2}
                  disabled={isLoading}
                  placeholder="Apartment, suite, etc. (optional)"
                />
              </Grid>

              {/* City */}
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  name="city"
                  label="City"
                  value={values.city}
                  onChange={handleChange}
                  onBlur={handleBlur}
                  error={touched.city && !!errors.city}
                  helperText={touched.city && errors.city}
                  disabled={isLoading}
                />
              </Grid>

              {/* State */}
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  name="state"
                  label="State/Province"
                  value={values.state}
                  onChange={handleChange}
                  onBlur={handleBlur}
                  error={touched.state && !!errors.state}
                  helperText={touched.state && errors.state}
                  disabled={isLoading}
                />
              </Grid>

              {/* Zip Code */}
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  name="zipCode"
                  label="Zip/Postal Code"
                  value={values.zipCode}
                  onChange={handleChange}
                  onBlur={handleBlur}
                  error={touched.zipCode && !!errors.zipCode}
                  helperText={touched.zipCode && errors.zipCode}
                  disabled={isLoading}
                />
              </Grid>

              {/* Country */}
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  name="country"
                  label="Country"
                  value={values.country}
                  onChange={handleChange}
                  onBlur={handleBlur}
                  error={touched.country && !!errors.country}
                  helperText={touched.country && errors.country}
                  disabled={isLoading}
                />
              </Grid>

              {/* Label */}
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  name="label"
                  label="Label"
                  value={values.label}
                  onChange={handleChange}
                  onBlur={handleBlur}
                  error={touched.label && !!errors.label}
                  helperText={(touched.label && errors.label) || 'e.g., Main Office, Warehouse'}
                  disabled={isLoading}
                  placeholder="Optional label"
                />
              </Grid>

              {/* Address Type */}
              <Grid item xs={12} sm={6}>
                <FormControl fullWidth disabled={isLoading} error={touched.addressType && !!errors.addressType}>
                  <InputLabel>Address Type</InputLabel>
                  <Select
                    name="addressType"
                    label="Address Type"
                    value={values.addressType}
                    onChange={handleChange}
                  >
                    {ADDRESS_TYPES.map((type) => (
                      <MenuItem key={type} value={type}>
                        {type}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>

              {/* Is Primary Checkbox */}
              <Grid item xs={12}>
                <FormControlLabel
                  control={
                    <Checkbox
                      name="isPrimary"
                      checked={values.isPrimary}
                      onChange={handleChange}
                      disabled={isLoading}
                    />
                  }
                  label="Set as Primary Address"
                />
              </Grid>
            </Grid>

            {/* Action Buttons */}
            <Box sx={{ display: 'flex', gap: 2, mt: 3, justifyContent: 'flex-end' }}>
              <Button
                onClick={onCancel}
                disabled={isLoading}
              >
                Cancel
              </Button>
              <Button
                type="submit"
                variant="contained"
                color="primary"
                disabled={isLoading || isSubmitting}
                startIcon={isLoading ? <CircularProgress size={20} /> : undefined}
              >
                {isLoading ? 'Saving...' : isEditMode ? 'Update Address' : 'Create Address'}
              </Button>
            </Box>
          </Box>
        </Form>
      )}
    </Formik>
  );
};

export default AddressFormComponent;
