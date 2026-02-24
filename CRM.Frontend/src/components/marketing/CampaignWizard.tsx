/**
 * CampaignWizard - Multi-step wizard for creating marketing campaigns
 * Steps: Details, Schedule, Audience, Content, Review & Confirm
 */

import React, { useState, useCallback, useMemo } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Stepper,
  Step,
  StepLabel,
  Button,
  TextField,
  Box,
  Typography,
  MenuItem,
  Grid,
  Chip,
  Paper,
  Divider,
  IconButton,
  Alert,
  FormControl,
  InputLabel,
  Select,
  SelectChangeEvent,
  FormHelperText,
} from '@mui/material';
import {
  Close as CloseIcon,
  ArrowBack as ArrowBackIcon,
  ArrowForward as ArrowForwardIcon,
  Check as CheckIcon,
  Campaign as CampaignIcon,
} from '@mui/icons-material';

// ============================================================================
// Types
// ============================================================================

export interface CreateCampaignData {
  name: string;
  type: 'email' | 'sms' | 'social' | 'event' | 'webinar';
  description?: string;
  startDate: string;
  endDate?: string;
  budget?: number;
  targetAudience: string[];
  emailTemplateId?: number;
  status: 'draft' | 'scheduled' | 'active';
  timezone?: string;
}

export interface CampaignWizardProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (campaign: CreateCampaignData) => void;
  initialData?: Partial<CreateCampaignData>;
}

// ============================================================================
// Constants
// ============================================================================

const STEPS = ['Campaign Details', 'Schedule', 'Audience', 'Content', 'Review & Confirm'];

const CAMPAIGN_TYPE_LIST: { value: CreateCampaignData['type']; label: string }[] = [
  { value: 'email', label: 'Email Campaign' },
  { value: 'sms', label: 'SMS Campaign' },
  { value: 'social', label: 'Social Media' },
  { value: 'event', label: 'Event' },
  { value: 'webinar', label: 'Webinar' },
];

const AUDIENCE_SEGMENTS = [
  'All Contacts',
  'Active Customers',
  'New Leads',
  'Enterprise Accounts',
  'SMB Accounts',
  'Churned Customers',
  'Newsletter Subscribers',
  'Product Users',
  'Trial Users',
  'VIP Customers',
];

const TIMEZONES = [
  'UTC',
  'America/New_York',
  'America/Chicago',
  'America/Denver',
  'America/Los_Angeles',
  'Europe/London',
  'Europe/Paris',
  'Europe/Berlin',
  'Asia/Tokyo',
  'Asia/Shanghai',
  'Australia/Sydney',
];

const DEFAULT_DATA: CreateCampaignData = {
  name: '',
  type: 'email',
  description: '',
  startDate: new Date().toISOString().split('T')[0],
  endDate: '',
  budget: undefined,
  targetAudience: [],
  emailTemplateId: undefined,
  status: 'draft',
  timezone: 'UTC',
};

// ============================================================================
// Component
// ============================================================================

const CampaignWizard: React.FC<CampaignWizardProps> = ({
  open,
  onClose,
  onSubmit,
  initialData,
}) => {
  const [activeStep, setActiveStep] = useState(0);
  const [formData, setFormData] = useState<CreateCampaignData>({
    ...DEFAULT_DATA,
    ...initialData,
  });
  const [errors, setErrors] = useState<Record<string, string>>({});

  // Reset when dialog opens
  React.useEffect(() => {
    if (open) {
      setActiveStep(0);
      setFormData({ ...DEFAULT_DATA, ...initialData });
      setErrors({});
    }
  }, [open, initialData]);

  const handleFieldChange = useCallback(
    (field: keyof CreateCampaignData, value: unknown) => {
      setFormData((prev) => ({ ...prev, [field]: value }));
      setErrors((prev) => {
        const next = { ...prev };
        delete next[field];
        return next;
      });
    },
    []
  );

  const handleSelectChange = useCallback(
    (field: keyof CreateCampaignData) => (event: SelectChangeEvent<string>) => {
      handleFieldChange(field, event.target.value);
    },
    [handleFieldChange]
  );

  const handleAudienceToggle = useCallback((segment: string) => {
    setFormData((prev) => {
      const exists = prev.targetAudience.includes(segment);
      return {
        ...prev,
        targetAudience: exists
          ? prev.targetAudience.filter((s) => s !== segment)
          : [...prev.targetAudience, segment],
      };
    });
  }, []);

  // Step validation
  const validateStep = useCallback(
    (step: number): boolean => {
      const newErrors: Record<string, string> = {};

      switch (step) {
        case 0:
          if (!formData.name.trim()) newErrors.name = 'Campaign name is required';
          if (!formData.type) newErrors.type = 'Campaign type is required';
          break;
        case 1:
          if (!formData.startDate) newErrors.startDate = 'Start date is required';
          if (formData.endDate && formData.startDate && formData.endDate < formData.startDate) {
            newErrors.endDate = 'End date must be after start date';
          }
          break;
        case 2:
          if (formData.targetAudience.length === 0)
            newErrors.targetAudience = 'Select at least one audience segment';
          break;
        default:
          break;
      }

      setErrors(newErrors);
      return Object.keys(newErrors).length === 0;
    },
    [formData]
  );

  const handleNext = useCallback(() => {
    if (validateStep(activeStep)) {
      setActiveStep((prev) => Math.min(prev + 1, STEPS.length - 1));
    }
  }, [activeStep, validateStep]);

  const handleBack = useCallback(() => {
    setActiveStep((prev) => Math.max(prev - 1, 0));
  }, []);

  const handleSubmit = useCallback(() => {
    onSubmit(formData);
    onClose();
  }, [formData, onSubmit, onClose]);

  const estimatedReach = useMemo(() => {
    return formData.targetAudience.length * 2500;
  }, [formData.targetAudience]);

  // =========================================================================
  // Step Content Renderers
  // =========================================================================

  const renderDetailsStep = () => (
    <Grid container spacing={3}>
      <Grid item xs={12}>
        <TextField
          fullWidth
          label="Campaign Name"
          value={formData.name}
          onChange={(e) => handleFieldChange('name', e.target.value)}
          error={!!errors.name}
          helperText={errors.name}
          required
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <FormControl fullWidth error={!!errors.type}>
          <InputLabel>Campaign Type</InputLabel>
          <Select
            value={formData.type}
            label="Campaign Type"
            onChange={handleSelectChange('type')}
          >
            {CAMPAIGN_TYPE_LIST.map((ct) => (
              <MenuItem key={ct.value} value={ct.value}>
                {ct.label}
              </MenuItem>
            ))}
          </Select>
          {errors.type && <FormHelperText>{errors.type}</FormHelperText>}
        </FormControl>
      </Grid>
      <Grid item xs={12} sm={6}>
        <FormControl fullWidth>
          <InputLabel>Status</InputLabel>
          <Select
            value={formData.status}
            label="Status"
            onChange={handleSelectChange('status')}
          >
            <MenuItem value="draft">Draft</MenuItem>
            <MenuItem value="scheduled">Scheduled</MenuItem>
            <MenuItem value="active">Active</MenuItem>
          </Select>
        </FormControl>
      </Grid>
      <Grid item xs={12}>
        <TextField
          fullWidth
          multiline
          rows={3}
          label="Description"
          value={formData.description || ''}
          onChange={(e) => handleFieldChange('description', e.target.value)}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <TextField
          fullWidth
          type="number"
          label="Budget ($)"
          value={formData.budget ?? ''}
          onChange={(e) =>
            handleFieldChange('budget', e.target.value ? Number(e.target.value) : undefined)
          }
          inputProps={{ min: 0, step: 100 }}
        />
      </Grid>
    </Grid>
  );

  const renderScheduleStep = () => (
    <Grid container spacing={3}>
      <Grid item xs={12} sm={6}>
        <TextField
          fullWidth
          type="date"
          label="Start Date"
          value={formData.startDate}
          onChange={(e) => handleFieldChange('startDate', e.target.value)}
          InputLabelProps={{ shrink: true }}
          error={!!errors.startDate}
          helperText={errors.startDate}
          required
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <TextField
          fullWidth
          type="date"
          label="End Date"
          value={formData.endDate || ''}
          onChange={(e) => handleFieldChange('endDate', e.target.value || undefined)}
          InputLabelProps={{ shrink: true }}
          error={!!errors.endDate}
          helperText={errors.endDate || 'Optional'}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <FormControl fullWidth>
          <InputLabel>Timezone</InputLabel>
          <Select
            value={formData.timezone || 'UTC'}
            label="Timezone"
            onChange={handleSelectChange('timezone')}
          >
            {TIMEZONES.map((tz) => (
              <MenuItem key={tz} value={tz}>
                {tz}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </Grid>
      <Grid item xs={12}>
        <Alert severity="info" sx={{ mt: 1 }}>
          Ensure the start date is at least 24 hours in the future for scheduled campaigns.
        </Alert>
      </Grid>
    </Grid>
  );

  const renderAudienceStep = () => (
    <Box>
      {errors.targetAudience && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {errors.targetAudience}
        </Alert>
      )}
      <Typography variant="subtitle1" gutterBottom>
        Select Target Audience Segments
      </Typography>
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 3 }}>
        {AUDIENCE_SEGMENTS.map((segment) => (
          <Chip
            key={segment}
            label={segment}
            color={formData.targetAudience.includes(segment) ? 'primary' : 'default'}
            variant={formData.targetAudience.includes(segment) ? 'filled' : 'outlined'}
            onClick={() => handleAudienceToggle(segment)}
            sx={{ cursor: 'pointer' }}
          />
        ))}
      </Box>
      <Divider sx={{ my: 2 }} />
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" color="text.secondary">
          Estimated Reach
        </Typography>
        <Typography variant="h4" color="primary">
          {estimatedReach.toLocaleString()} contacts
        </Typography>
        <Typography variant="caption" color="text.secondary">
          Based on {formData.targetAudience.length} selected segment(s)
        </Typography>
      </Paper>
    </Box>
  );

  const renderContentStep = () => (
    <Grid container spacing={3}>
      <Grid item xs={12}>
        <Typography variant="subtitle1" gutterBottom>
          Email Template (Optional)
        </Typography>
        <TextField
          fullWidth
          type="number"
          label="Email Template ID"
          value={formData.emailTemplateId ?? ''}
          onChange={(e) =>
            handleFieldChange(
              'emailTemplateId',
              e.target.value ? Number(e.target.value) : undefined
            )
          }
          helperText="Enter the ID of an existing email template, or leave blank to use default content"
        />
      </Grid>
      <Grid item xs={12}>
        <Alert severity="info">
          To create or edit email templates, use the Email Template Editor from the Marketing menu.
        </Alert>
      </Grid>
    </Grid>
  );

  const renderReviewStep = () => (
    <Box>
      <Typography variant="h6" gutterBottom>
        Campaign Summary
      </Typography>
      <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
        <Grid container spacing={2}>
          <Grid item xs={6}>
            <Typography variant="caption" color="text.secondary">
              Campaign Name
            </Typography>
            <Typography variant="body1" fontWeight={600}>
              {formData.name}
            </Typography>
          </Grid>
          <Grid item xs={6}>
            <Typography variant="caption" color="text.secondary">
              Type
            </Typography>
            <Typography variant="body1">
              {CAMPAIGN_TYPE_LIST.find((ct) => ct.value === formData.type)?.label || formData.type}
            </Typography>
          </Grid>
          <Grid item xs={6}>
            <Typography variant="caption" color="text.secondary">
              Status
            </Typography>
            <Box>
              <Chip
                size="small"
                label={formData.status}
                color={
                  formData.status === 'active'
                    ? 'success'
                    : formData.status === 'scheduled'
                    ? 'warning'
                    : 'default'
                }
              />
            </Box>
          </Grid>
          <Grid item xs={6}>
            <Typography variant="caption" color="text.secondary">
              Budget
            </Typography>
            <Typography variant="body1">
              {formData.budget ? `$${formData.budget.toLocaleString()}` : 'Not set'}
            </Typography>
          </Grid>
          <Grid item xs={6}>
            <Typography variant="caption" color="text.secondary">
              Start Date
            </Typography>
            <Typography variant="body1">{formData.startDate}</Typography>
          </Grid>
          <Grid item xs={6}>
            <Typography variant="caption" color="text.secondary">
              End Date
            </Typography>
            <Typography variant="body1">{formData.endDate || 'Ongoing'}</Typography>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="caption" color="text.secondary">
              Target Audience ({formData.targetAudience.length} segments)
            </Typography>
            <Box sx={{ mt: 0.5, display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
              {formData.targetAudience.map((segment) => (
                <Chip key={segment} label={segment} size="small" variant="outlined" />
              ))}
            </Box>
          </Grid>
          {formData.emailTemplateId && (
            <Grid item xs={12}>
              <Typography variant="caption" color="text.secondary">
                Email Template ID
              </Typography>
              <Typography variant="body1">#{formData.emailTemplateId}</Typography>
            </Grid>
          )}
          {formData.description && (
            <Grid item xs={12}>
              <Typography variant="caption" color="text.secondary">
                Description
              </Typography>
              <Typography variant="body2">{formData.description}</Typography>
            </Grid>
          )}
        </Grid>
      </Paper>
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" color="text.secondary">
          Estimated Reach
        </Typography>
        <Typography variant="h5" color="primary">
          {estimatedReach.toLocaleString()} contacts
        </Typography>
      </Paper>
    </Box>
  );

  const renderStepContent = (step: number) => {
    switch (step) {
      case 0:
        return renderDetailsStep();
      case 1:
        return renderScheduleStep();
      case 2:
        return renderAudienceStep();
      case 3:
        return renderContentStep();
      case 4:
        return renderReviewStep();
      default:
        return null;
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <CampaignIcon color="primary" />
          <Typography variant="h6">Create Campaign</Typography>
        </Box>
        <IconButton onClick={onClose} size="small">
          <CloseIcon />
        </IconButton>
      </DialogTitle>

      <DialogContent dividers>
        <Stepper activeStep={activeStep} sx={{ mb: 4, pt: 1 }}>
          {STEPS.map((label) => (
            <Step key={label}>
              <StepLabel>{label}</StepLabel>
            </Step>
          ))}
        </Stepper>
        <Box sx={{ minHeight: 300 }}>{renderStepContent(activeStep)}</Box>
      </DialogContent>

      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={onClose} color="inherit">
          Cancel
        </Button>
        <Box sx={{ flex: 1 }} />
        {activeStep > 0 && (
          <Button onClick={handleBack} startIcon={<ArrowBackIcon />}>
            Back
          </Button>
        )}
        {activeStep < STEPS.length - 1 ? (
          <Button variant="contained" onClick={handleNext} endIcon={<ArrowForwardIcon />}>
            Next
          </Button>
        ) : (
          <Button
            variant="contained"
            color="success"
            onClick={handleSubmit}
            startIcon={<CheckIcon />}
          >
            Create Campaign
          </Button>
        )}
      </DialogActions>
    </Dialog>
  );
};

export default CampaignWizard;
