import React, { useState } from 'react';
import {
  Alert, Box, Button, Card, CardContent, CircularProgress, Grid,
  InputAdornment, Stack, TextField, Typography
} from '@mui/material';
import HandshakeIcon from '@mui/icons-material/Handshake';
import apiClient from '../../services/apiClient';

/** Validates email format without a regex to avoid potential ReDoS concerns. */
const isEmailLikelyValid = (email: string): boolean => {
  const atIndex = email.indexOf('@');
  if (atIndex <= 0 || atIndex !== email.lastIndexOf('@')) return false;
  const domain = email.slice(atIndex + 1);
  const dotIndex = domain.lastIndexOf('.');
  return dotIndex > 0 && dotIndex < domain.length - 1;
};

interface DealRegistration {
  contactFirstName: string;
  contactLastName: string;
  companyName: string;
  email: string;
  dealValue: string;
  notes: string;
}

const INITIAL: DealRegistration = {
  contactFirstName: '',
  contactLastName: '',
  companyName: '',
  email: '',
  dealValue: '',
  notes: '',
};

const PartnerPortalPage: React.FC = () => {
  const [form, setForm] = useState<DealRegistration>(INITIAL);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [errors, setErrors] = useState<Partial<Record<keyof DealRegistration, string>>>({});

  const update = (field: keyof DealRegistration) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm(prev => ({ ...prev, [field]: e.target.value }));

  const validate = (): boolean => {
    const errs: Partial<Record<keyof DealRegistration, string>> = {};
    if (!form.contactFirstName.trim()) errs.contactFirstName = 'Required';
    if (!form.contactLastName.trim()) errs.contactLastName = 'Required';
    if (!form.companyName.trim()) errs.companyName = 'Required';
    if (!form.email.trim() || !isEmailLikelyValid(form.email)) errs.email = 'Valid email required';
    if (!form.dealValue || isNaN(Number(form.dealValue)) || Number(form.dealValue) <= 0)
      errs.dealValue = 'Enter a positive number';
    setErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleSubmit = async () => {
    if (!validate()) return;
    setSubmitting(true);
    setError(null);
    try {
      await apiClient.post('/api/portal/deals', {
        ...form,
        dealValue: Number(form.dealValue),
      });
      setSuccess(true);
      setForm(INITIAL);
    } catch {
      setError('Failed to submit deal registration. Please try again.');
    } finally {
      setSubmitting(false);
    }
  };

  if (success) {
    return (
      <Box p={3} maxWidth={600} mx="auto" textAlign="center">
        <HandshakeIcon sx={{ fontSize: 64, color: 'success.main', mb: 2 }} />
        <Typography variant="h5" fontWeight="bold" gutterBottom>Deal Registered!</Typography>
        <Typography color="text.secondary" mb={3}>
          Your deal has been submitted successfully. Our partner team will review it and get back to you soon.
        </Typography>
        <Button variant="outlined" onClick={() => setSuccess(false)}>Register Another Deal</Button>
      </Box>
    );
  }

  return (
    <Box p={3} maxWidth={700} mx="auto">
      <Stack direction="row" spacing={1} alignItems="center" mb={1}>
        <HandshakeIcon color="primary" />
        <Typography variant="h5" fontWeight="bold">Partner Deal Registration</Typography>
      </Stack>
      <Typography variant="body2" color="text.secondary" mb={3}>
        Register a new deal opportunity. Our team will review and assign a partner manager.
      </Typography>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <Card variant="outlined">
        <CardContent>
          <Typography variant="subtitle2" gutterBottom>Contact Information</Typography>
          <Grid container spacing={2} mb={2}>
            <Grid item xs={6}>
              <TextField fullWidth size="small" label="First Name *"
                value={form.contactFirstName} onChange={update('contactFirstName')}
                error={!!errors.contactFirstName} helperText={errors.contactFirstName} />
            </Grid>
            <Grid item xs={6}>
              <TextField fullWidth size="small" label="Last Name *"
                value={form.contactLastName} onChange={update('contactLastName')}
                error={!!errors.contactLastName} helperText={errors.contactLastName} />
            </Grid>
            <Grid item xs={6}>
              <TextField fullWidth size="small" label="Company Name *"
                value={form.companyName} onChange={update('companyName')}
                error={!!errors.companyName} helperText={errors.companyName} />
            </Grid>
            <Grid item xs={6}>
              <TextField fullWidth size="small" label="Email *" type="email"
                value={form.email} onChange={update('email')}
                error={!!errors.email} helperText={errors.email} />
            </Grid>
          </Grid>

          <Typography variant="subtitle2" gutterBottom>Deal Details</Typography>
          <Grid container spacing={2}>
            <Grid item xs={6}>
              <TextField fullWidth size="small" label="Estimated Deal Value *" type="number"
                value={form.dealValue} onChange={update('dealValue')}
                error={!!errors.dealValue} helperText={errors.dealValue}
                InputProps={{
                  startAdornment: <InputAdornment position="start">$</InputAdornment>,
                }} />
            </Grid>
            <Grid item xs={12}>
              <TextField fullWidth size="small" label="Notes" multiline rows={3}
                value={form.notes} onChange={update('notes')}
                placeholder="Additional context, competitor info, timeline, etc." />
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      <Stack direction="row" justifyContent="flex-end" mt={2}>
        <Button variant="contained" onClick={handleSubmit} disabled={submitting}
          startIcon={submitting ? <CircularProgress size={16} /> : <HandshakeIcon />}>
          {submitting ? 'Submitting…' : 'Register Deal'}
        </Button>
      </Stack>
    </Box>
  );
};

export default PartnerPortalPage;
