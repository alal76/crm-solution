/**/**







































































































































































































































































































































































































































































































































































































export default CampaignWizard;};  );    </Dialog>      </DialogActions>        )}          </Button>            Create Campaign          >            startIcon={<CheckIcon />}            onClick={handleSubmit}            color="success"            variant="contained"          <Button        ) : (          </Button>            Next          <Button variant="contained" onClick={handleNext} endIcon={<ArrowForwardIcon />}>        {activeStep < STEPS.length - 1 ? (        )}          </Button>            Back          <Button onClick={handleBack} startIcon={<ArrowBackIcon />}>        {activeStep > 0 && (        <Box sx={{ flex: 1 }} />        </Button>          Cancel        <Button onClick={onClose} color="inherit">      <DialogActions sx={{ px: 3, py: 2 }}>      </DialogContent>        <Box sx={{ minHeight: 300 }}>{renderStepContent(activeStep)}</Box>        </Stepper>          ))}            </Step>              <StepLabel>{label}</StepLabel>            <Step key={label}>          {STEPS.map((label) => (        <Stepper activeStep={activeStep} sx={{ mb: 4, pt: 1 }}>      <DialogContent dividers>      </DialogTitle>        </IconButton>          <CloseIcon />        <IconButton onClick={onClose} size="small">        </Box>          <Typography variant="h6">Create Campaign</Typography>          <CampaignIcon color="primary" />        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>      <DialogTitle sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>  return (  };    }        return null;      default:        return renderReviewStep();      case 4:        return renderContentStep();      case 3:        return renderAudienceStep();      case 2:        return renderScheduleStep();      case 1:        return renderDetailsStep();      case 0:    switch (step) {  const renderStepContent = (step: number) => {  );    </Box>      </Paper>        </Typography>          {estimatedReach.toLocaleString()} contacts        <Typography variant="h5" color="primary">        </Typography>          Estimated Reach        <Typography variant="subtitle2" color="text.secondary">      <Paper variant="outlined" sx={{ p: 2 }}>      </Paper>        </Grid>          )}            </Grid>              <Typography variant="body2">{formData.description}</Typography>              </Typography>                Description              <Typography variant="caption" color="text.secondary">            <Grid item xs={12}>          {formData.description && (          )}            </Grid>              <Typography variant="body1">#{formData.emailTemplateId}</Typography>              </Typography>                Email Template ID              <Typography variant="caption" color="text.secondary">            <Grid item xs={12}>          {formData.emailTemplateId && (          </Grid>            </Box>              ))}                <Chip key={segment} label={segment} size="small" variant="outlined" />              {formData.targetAudience.map((segment) => (            <Box sx={{ mt: 0.5, display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>            </Typography>              Target Audience ({formData.targetAudience.length} segments)            <Typography variant="caption" color="text.secondary">          <Grid item xs={12}>          </Grid>            <Typography variant="body1">{formData.endDate || 'Ongoing'}</Typography>            </Typography>              End Date            <Typography variant="caption" color="text.secondary">          <Grid item xs={6}>          </Grid>            <Typography variant="body1">{formData.startDate}</Typography>            </Typography>              Start Date            <Typography variant="caption" color="text.secondary">          <Grid item xs={6}>          </Grid>            </Typography>              {formData.budget ? `$${formData.budget.toLocaleString()}` : 'Not set'}            <Typography variant="body1">            </Typography>              Budget            <Typography variant="caption" color="text.secondary">          <Grid item xs={6}>          </Grid>            />              }                  : 'default'                  ? 'warning'                  : formData.status === 'scheduled'                  ? 'success'                formData.status === 'active'              color={              label={formData.status}              size="small"            <Chip            </Typography>              Status            <Typography variant="caption" color="text.secondary">          <Grid item xs={6}>          </Grid>            </Typography>              {CAMPAIGN_TYPES.find((ct) => ct.value === formData.type)?.label || formData.type}            <Typography variant="body1">            </Typography>              Type            <Typography variant="caption" color="text.secondary">          <Grid item xs={6}>          </Grid>            </Typography>              {formData.name}            <Typography variant="body1" fontWeight={600}>            </Typography>              Campaign Name            <Typography variant="caption" color="text.secondary">          <Grid item xs={6}>        <Grid container spacing={2}>      <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>      </Typography>        Campaign Summary      <Typography variant="h6" gutterBottom>    <Box>  const renderReviewStep = () => (  );    </Grid>      </Grid>        </Alert>          To create or edit email templates, use the Email Template Editor from the Marketing menu.        <Alert severity="info">      <Grid item xs={12}>      </Grid>        />          helperText="Enter the ID of an existing email template, or leave blank to use default content"          }            )              e.target.value ? Number(e.target.value) : undefined              'emailTemplateId',            handleFieldChange(          onChange={(e) =>          value={formData.emailTemplateId ?? ''}          label="Email Template ID"          type="number"          fullWidth        <TextField        </Typography>          Email Template (Optional)        <Typography variant="subtitle1" gutterBottom>      <Grid item xs={12}>    <Grid container spacing={3}>  const renderContentStep = () => (  );    </Box>      </Paper>        </Typography>          Based on {formData.targetAudience.length} selected segment(s)        <Typography variant="caption" color="text.secondary">        </Typography>          {estimatedReach.toLocaleString()} contacts        <Typography variant="h4" color="primary">        </Typography>          Estimated Reach        <Typography variant="subtitle2" color="text.secondary">      <Paper variant="outlined" sx={{ p: 2 }}>      <Divider sx={{ my: 2 }} />      </Box>        ))}          />            sx={{ cursor: 'pointer' }}            onClick={() => handleAudienceToggle(segment)}            variant={formData.targetAudience.includes(segment) ? 'filled' : 'outlined'}            color={formData.targetAudience.includes(segment) ? 'primary' : 'default'}            label={segment}            key={segment}          <Chip        {AUDIENCE_SEGMENTS.map((segment) => (      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 3 }}>      </Typography>        Select Target Audience Segments      <Typography variant="subtitle1" gutterBottom>      )}        </Alert>          {errors.targetAudience}        <Alert severity="error" sx={{ mb: 2 }}>      {errors.targetAudience && (    <Box>  const renderAudienceStep = () => (  );    </Grid>      </Grid>        </Alert>          at least 24 hours in the future for scheduled campaigns.          Campaign emails will be sent according to the selected timezone. Ensure the start date is        <Alert severity="info" sx={{ mt: 1 }}>      <Grid item xs={12}>      </Grid>        </FormControl>          </Select>            ))}              </MenuItem>                {tz}              <MenuItem key={tz} value={tz}>            {TIMEZONES.map((tz) => (          >            onChange={handleSelectChange('timezone')}            label="Timezone"            value={formData.timezone || 'UTC'}          <Select          <InputLabel>Timezone</InputLabel>        <FormControl fullWidth>      <Grid item xs={12} sm={6}>      </Grid>        />          helperText={errors.endDate || 'Optional – leave blank for ongoing campaigns'}          error={!!errors.endDate}          InputLabelProps={{ shrink: true }}          onChange={(e) => handleFieldChange('endDate', e.target.value || undefined)}          value={formData.endDate || ''}          label="End Date"          type="date"          fullWidth        <TextField      <Grid item xs={12} sm={6}>      </Grid>        />          required          helperText={errors.startDate}          error={!!errors.startDate}          InputLabelProps={{ shrink: true }}          onChange={(e) => handleFieldChange('startDate', e.target.value)}          value={formData.startDate}          label="Start Date"          type="date"          fullWidth        <TextField      <Grid item xs={12} sm={6}>    <Grid container spacing={3}>  const renderScheduleStep = () => (  );    </Grid>      </Grid>        />          inputProps={{ min: 0, step: 100 }}          }            handleFieldChange('budget', e.target.value ? Number(e.target.value) : undefined)          onChange={(e) =>          value={formData.budget ?? ''}          label="Budget ($)"          type="number"          fullWidth        <TextField      <Grid item xs={12} sm={6}>      </Grid>        />          onChange={(e) => handleFieldChange('description', e.target.value)}          value={formData.description || ''}          label="Description"          rows={3}          multiline          fullWidth        <TextField      <Grid item xs={12}>      </Grid>        </FormControl>          </Select>            <MenuItem value="active">Active</MenuItem>            <MenuItem value="scheduled">Scheduled</MenuItem>            <MenuItem value="draft">Draft</MenuItem>          >            onChange={handleSelectChange('status')}            label="Status"            value={formData.status}          <Select          <InputLabel>Status</InputLabel>        <FormControl fullWidth>      <Grid item xs={12} sm={6}>      </Grid>        </FormControl>          {errors.type && <FormHelperText>{errors.type}</FormHelperText>}          </Select>            ))}              </MenuItem>                {ct.label}              <MenuItem key={ct.value} value={ct.value}>            {CAMPAIGN_TYPES.map((ct) => (          >            onChange={handleSelectChange('type')}            label="Campaign Type"            value={formData.type}          <Select          <InputLabel>Campaign Type</InputLabel>        <FormControl fullWidth error={!!errors.type}>      <Grid item xs={12} sm={6}>      </Grid>        />          required          helperText={errors.name}          error={!!errors.name}          onChange={(e) => handleFieldChange('name', e.target.value)}          value={formData.name}          label="Campaign Name"          fullWidth        <TextField      <Grid item xs={12}>    <Grid container spacing={3}>  const renderDetailsStep = () => (  // =========================================================================  // Step Content Renderers  // =========================================================================  }, [formData.targetAudience]);    return formData.targetAudience.length * 2500;    // Simulated reach based on audience count  const estimatedReach = useMemo(() => {  }, [formData, onSubmit, onClose]);    onClose();    onSubmit(formData);  const handleSubmit = useCallback(() => {  }, []);    setActiveStep((prev) => Math.max(prev - 1, 0));  const handleBack = useCallback(() => {  }, [activeStep, validateStep]);    }      setActiveStep((prev) => Math.min(prev + 1, STEPS.length - 1));    if (validateStep(activeStep)) {  const handleNext = useCallback(() => {  );    [formData]    },      return Object.keys(newErrors).length === 0;      setErrors(newErrors);      }          break;          // Content step is optional        case 3: // Content          break;            newErrors.targetAudience = 'Select at least one audience segment';          if (formData.targetAudience.length === 0)        case 2: // Audience          break;          }            newErrors.endDate = 'End date must be after start date';          if (formData.endDate && formData.startDate && formData.endDate < formData.startDate) {          if (!formData.startDate) newErrors.startDate = 'Start date is required';        case 1: // Schedule          break;          if (!formData.type) newErrors.type = 'Campaign type is required';          if (!formData.name.trim()) newErrors.name = 'Campaign name is required';        case 0: // Details      switch (step) {      const newErrors: Record<string, string> = {};    (step: number): boolean => {  const validateStep = useCallback(  // Step validation  }, []);    });      };          : [...prev.targetAudience, segment],          ? prev.targetAudience.filter((s) => s !== segment)        targetAudience: exists        ...prev,      return {      const exists = prev.targetAudience.includes(segment);    setFormData((prev) => {  const handleAudienceToggle = useCallback((segment: string) => {  );    [handleFieldChange]    },      handleFieldChange(field, event.target.value);    (field: keyof CreateCampaignData) => (event: SelectChangeEvent<string>) => {  const handleSelectChange = useCallback(  );    []    },      });        return next;        delete next[field];        const next = { ...prev };      setErrors((prev) => {      setFormData((prev) => ({ ...prev, [field]: value }));    (field: keyof CreateCampaignData, value: unknown) => {  const handleFieldChange = useCallback(  }, [open, initialData]);    }      setErrors({});      setFormData({ ...DEFAULT_DATA, ...initialData });      setActiveStep(0);    if (open) {  React.useEffect(() => {  // Reset when dialog opens  const [errors, setErrors] = useState<Record<string, string>>({});  });    ...initialData,    ...DEFAULT_DATA,  const [formData, setFormData] = useState<CreateCampaignData>({  const [activeStep, setActiveStep] = useState(0);}) => {  initialData,  onSubmit,  onClose,  open,const CampaignWizard: React.FC<CampaignWizardProps> = ({// ============================================================================// Component// ============================================================================};  timezone: 'UTC',  status: 'draft',  emailTemplateId: undefined,  targetAudience: [],  budget: undefined,  endDate: '',  startDate: new Date().toISOString().split('T')[0],  description: '',  type: 'email',  name: '',const DEFAULT_DATA: CreateCampaignData = {];  'Australia/Sydney',  'Asia/Shanghai',  'Asia/Tokyo',  'Europe/Berlin',  'Europe/Paris',  'Europe/London',  'America/Los_Angeles',  'America/Denver',  'America/Chicago',  'America/New_York',  'UTC',const TIMEZONES = [];  'VIP Customers',  'Trial Users',  'Product Users',  'Newsletter Subscribers',  'Churned Customers',  'SMB Accounts',  'Enterprise Accounts',  'New Leads',  'Active Customers',  'All Contacts',const AUDIENCE_SEGMENTS = [];  { value: 'webinar', label: 'Webinar' },  { value: 'event', label: 'Event' },  { value: 'social', label: 'Social Media' },  { value: 'sms', label: 'SMS Campaign' },  { value: 'email', label: 'Email Campaign' },const CAMPAIGN_TYPES: { value: CreateCampaignData['type']; label: string }[] = [const STEPS = ['Campaign Details', 'Schedule', 'Audience', 'Content', 'Review & Confirm'];// ============================================================================// Constants// ============================================================================}  initialData?: Partial<CreateCampaignData>;  onSubmit: (campaign: CreateCampaignData) => void;  onClose: () => void;  open: boolean;export interface CampaignWizardProps {}  timezone?: string;  status: 'draft' | 'scheduled' | 'active';  emailTemplateId?: number;  targetAudience: string[];  budget?: number;  endDate?: string;  startDate: string;  description?: string;  type: 'email' | 'sms' | 'social' | 'event' | 'webinar';  name: string;export interface CreateCampaignData {// ============================================================================// Types// ============================================================================} from '@mui/icons-material';  Campaign as CampaignIcon,  Check as CheckIcon,  ArrowForward as ArrowForwardIcon,  ArrowBack as ArrowBackIcon,  Close as CloseIcon,import {} from '@mui/material';  FormHelperText,  SelectChangeEvent,  Select,  InputLabel,  FormControl,  Alert,  IconButton,  Divider,  Paper,  Chip,  Grid,  MenuItem,  Typography,  Box,  TextField,  Button,  StepLabel,  Step,  Stepper,  DialogActions,  DialogContent,  DialogTitle,  Dialog,import {import React, { useState, useCallback, useMemo } from 'react'; */ * Steps: Details → Schedule → Audience → Content → Review * CampaignWizard - Multi-step wizard for creating marketing campaigns * CampaignWizard - Multi-step wizard for creating marketing campaigns
 * Steps: Details → Schedule → Audience → Content → Review
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
  MenuItem,
  Box,
  Typography,
  Grid,
  Chip,
  FormControl,
  InputLabel,
  Select,
  IconButton,
  Alert,
  Divider,
  Paper,
  List,
  ListItem,
  ListItemText,
  SelectChangeEvent,
} from '@mui/material';
import {
  Close as CloseIcon,
  ArrowBack as BackIcon,
  ArrowForward as NextIcon,
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

const CAMPAIGN_TYPES: { value: CreateCampaignData['type']; label: string }[] = [
  { value: 'email', label: 'Email Campaign' },
  { value: 'sms', label: 'SMS Campaign' },
  { value: 'social', label: 'Social Media' },
  { value: 'event', label: 'Event' },
  { value: 'webinar', label: 'Webinar' },
];

const AUDIENCE_SEGMENTS = [
  'All Contacts',
  'Active Customers',
  'Inactive Customers',
  'New Leads',
  'Enterprise Accounts',
  'SMB Accounts',
  'Newsletter Subscribers',
  'Event Attendees',
  'Trial Users',
  'Churned Customers',
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

  // Reset on open
  React.useEffect(() => {
    if (open) {
      setActiveStep(0);
      setFormData({ ...DEFAULT_DATA, ...initialData });
      setErrors({});
    }
  }, [open, initialData]);

  const updateField = useCallback(
    <K extends keyof CreateCampaignData>(field: K, value: CreateCampaignData[K]) => {
      setFormData((prev) => ({ ...prev, [field]: value }));
      setErrors((prev) => {
        const next = { ...prev };
        delete next[field];
        return next;
      });
    },
    []
  );

  // ---- Validation per step ----
  const validateStep = useCallback(
    (step: number): boolean => {
      const newErrors: Record<string, string> = {};

      if (step === 0) {
        if (!formData.name.trim()) newErrors.name = 'Campaign name is required';
        if (!formData.type) newErrors.type = 'Campaign type is required';
      }

      if (step === 1) {
        if (!formData.startDate) newErrors.startDate = 'Start date is required';
        if (
          formData.endDate &&
          formData.startDate &&
          formData.endDate < formData.startDate
        ) {
          newErrors.endDate = 'End date must be after start date';
        }
      }

      if (step === 2) {
        if (formData.targetAudience.length === 0) {
          newErrors.targetAudience = 'Select at least one audience segment';
        }
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
    if (validateStep(activeStep)) {
      onSubmit(formData);
    }
  }, [activeStep, formData, onSubmit, validateStep]);

  const toggleAudienceSegment = useCallback(
    (segment: string) => {
      setFormData((prev) => {
        const exists = prev.targetAudience.includes(segment);
        return {
          ...prev,
          targetAudience: exists
            ? prev.targetAudience.filter((s) => s !== segment)
            : [...prev.targetAudience, segment],
        };
      });
      setErrors((prev) => {
        const next = { ...prev };
        delete next.targetAudience;
        return next;
      });
    },
    []
  );

  // Estimated reach calculation (mock)
  const estimatedReach = useMemo(() => {
    const base = formData.targetAudience.length * 1250;
    return base > 0 ? base + Math.floor(Math.random() * 500) : 0;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formData.targetAudience.length]);

  // ---- Step renderers ----

  const renderStepDetails = () => (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2.5 }}>
      <TextField
        label="Campaign Name"
        value={formData.name}
        onChange={(e) => updateField('name', e.target.value)}
        error={!!errors.name}
        helperText={errors.name}
        fullWidth
        required
        autoFocus
      />
      <FormControl fullWidth error={!!errors.type}>
        <InputLabel>Campaign Type</InputLabel>
        <Select
          value={formData.type}
          label="Campaign Type"
          onChange={(e: SelectChangeEvent) =>
            updateField('type', e.target.value as CreateCampaignData['type'])
          }
        >
          {CAMPAIGN_TYPES.map((t) => (
            <MenuItem key={t.value} value={t.value}>
              {t.label}
            </MenuItem>
          ))}
        </Select>
        {errors.type && (
          <Typography variant="caption" color="error" sx={{ mt: 0.5, ml: 1.5 }}>
            {errors.type}
          </Typography>
        )}
      </FormControl>
      <TextField
        label="Description"
        value={formData.description || ''}
        onChange={(e) => updateField('description', e.target.value)}
        fullWidth
        multiline
        rows={3}
        placeholder="Briefly describe this campaign's goals and strategy"
      />
    </Box>
  );

  const renderStepSchedule = () => (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2.5 }}>
      <Grid container spacing={2}>
        <Grid item xs={12} sm={6}>
          <TextField
            label="Start Date"
            type="date"
            value={formData.startDate}
            onChange={(e) => updateField('startDate', e.target.value)}
            error={!!errors.startDate}
            helperText={errors.startDate}
            fullWidth
            required
            InputLabelProps={{ shrink: true }}
          />
        </Grid>
        <Grid item xs={12} sm={6}>
          <TextField
            label="End Date"
            type="date"
            value={formData.endDate || ''}
            onChange={(e) => updateField('endDate', e.target.value || undefined)}
            error={!!errors.endDate}
            helperText={errors.endDate || 'Optional – leave blank for ongoing'}
            fullWidth
            InputLabelProps={{ shrink: true }}
          />
        </Grid>
      </Grid>
      <TextField
        label="Budget"
        type="number"
        value={formData.budget ?? ''}
        onChange={(e) =>
          updateField('budget', e.target.value ? Number(e.target.value) : undefined)
        }
        fullWidth
        InputProps={{ startAdornment: <Typography sx={{ mr: 0.5 }}>$</Typography> }}
        placeholder="0.00"
      />
      <FormControl fullWidth>
        <InputLabel>Status</InputLabel>
        <Select
          value={formData.status}
          label="Status"
          onChange={(e: SelectChangeEvent) =>
            updateField('status', e.target.value as CreateCampaignData['status'])
          }
        >
          <MenuItem value="draft">Draft</MenuItem>
          <MenuItem value="scheduled">Scheduled</MenuItem>
          <MenuItem value="active">Active</MenuItem>
        </Select>
      </FormControl>
    </Box>
  );

  const renderStepAudience = () => (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      <Typography variant="subtitle1" gutterBottom>
        Select Target Audience Segments
      </Typography>
      {errors.targetAudience && (
        <Alert severity="error" sx={{ mb: 1 }}>
          {errors.targetAudience}
        </Alert>
      )}
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
        {AUDIENCE_SEGMENTS.map((seg) => (
          <Chip
            key={seg}
            label={seg}
            onClick={() => toggleAudienceSegment(seg)}
            color={formData.targetAudience.includes(seg) ? 'primary' : 'default'}
            variant={formData.targetAudience.includes(seg) ? 'filled' : 'outlined'}
            clickable
          />
        ))}
      </Box>
      {formData.targetAudience.length > 0 && (
        <Paper variant="outlined" sx={{ p: 2, mt: 1 }}>
          <Typography variant="body2" color="text.secondary">
            Estimated Reach:{' '}
            <Typography component="span" fontWeight="bold" color="primary">
              {estimatedReach.toLocaleString()} contacts
            </Typography>
          </Typography>
        </Paper>
      )}
    </Box>
  );

  const renderStepContent = () => (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2.5 }}>
      {formData.type === 'email' && (
        <TextField
          label="Email Template ID"
          type="number"
          value={formData.emailTemplateId ?? ''}
          onChange={(e) =>
            updateField(
              'emailTemplateId',
              e.target.value ? Number(e.target.value) : undefined
            )
          }
          fullWidth
          placeholder="Enter template ID or leave blank to create inline"
          helperText="Select an existing email template to use for this campaign"
        />
      )}
      <Alert severity="info" sx={{ mt: 1 }}>
        {formData.type === 'email'
          ? 'Link an existing email template or create content after the campaign is created.'
          : `Content for ${formData.type} campaigns can be configured after creation.`}
      </Alert>
    </Box>
  );

  const renderStepReview = () => (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      <Typography variant="h6" gutterBottom>
        Campaign Summary
      </Typography>
      <Divider />
      <List dense disablePadding>
        <ListItem>
          <ListItemText primary="Name" secondary={formData.name} />
        </ListItem>
        <ListItem>
          <ListItemText
            primary="Type"
            secondary={
              CAMPAIGN_TYPES.find((t) => t.value === formData.type)?.label || formData.type
            }
          />
        </ListItem>
        {formData.description && (
          <ListItem>
            <ListItemText primary="Description" secondary={formData.description} />
          </ListItem>
        )}
        <ListItem>
          <ListItemText primary="Start Date" secondary={formData.startDate} />
        </ListItem>
        {formData.endDate && (
          <ListItem>
            <ListItemText primary="End Date" secondary={formData.endDate} />
          </ListItem>
        )}
        {formData.budget !== undefined && (
          <ListItem>
            <ListItemText
              primary="Budget"
              secondary={`$${formData.budget.toLocaleString()}`}
            />
          </ListItem>
        )}
        <ListItem>
          <ListItemText primary="Status" secondary={formData.status} />
        </ListItem>
        <ListItem>
          <ListItemText
            primary="Target Audience"
            secondary={
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5, mt: 0.5 }}>
                {formData.targetAudience.map((seg) => (
                  <Chip key={seg} label={seg} size="small" color="primary" variant="outlined" />
                ))}
              </Box>
            }
          />
        </ListItem>
        {formData.emailTemplateId && (
          <ListItem>
            <ListItemText
              primary="Email Template"
              secondary={`Template #${formData.emailTemplateId}`}
            />
          </ListItem>
        )}
      </List>
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="body2" color="text.secondary">
          Estimated Reach:{' '}
          <strong>{estimatedReach.toLocaleString()} contacts</strong>
        </Typography>
      </Paper>
    </Box>
  );

  const renderCurrentStep = () => {
    switch (activeStep) {
      case 0:
        return renderStepDetails();
      case 1:
        return renderStepSchedule();
      case 2:
        return renderStepAudience();
      case 3:
        return renderStepContent();
      case 4:
        return renderStepReview();
      default:
        return null;
    }
  };

  const isLastStep = activeStep === STEPS.length - 1;

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle
        sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}
      >
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <CampaignIcon color="primary" />
          <Typography variant="h6">Create Campaign</Typography>
        </Box>
        <IconButton onClick={onClose} size="small" aria-label="close">
          <CloseIcon />
        </IconButton>
      </DialogTitle>

      <DialogContent dividers>
        <Stepper activeStep={activeStep} sx={{ mb: 3 }} alternativeLabel>
          {STEPS.map((label) => (
            <Step key={label}>
              <StepLabel>{label}</StepLabel>
            </Step>
          ))}
        </Stepper>
        <Box sx={{ minHeight: 280, py: 1 }}>{renderCurrentStep()}</Box>
      </DialogContent>

      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={onClose} color="inherit">
          Cancel
        </Button>
        <Box sx={{ flex: 1 }} />
        {activeStep > 0 && (
          <Button onClick={handleBack} startIcon={<BackIcon />}>
            Back
          </Button>
        )}
        {isLastStep ? (
          <Button
            variant="contained"
            onClick={handleSubmit}
            startIcon={<CheckIcon />}
            color="primary"
          >
            Create Campaign
          </Button>
        ) : (
          <Button variant="contained" onClick={handleNext} endIcon={<NextIcon />}>
            Next
          </Button>
        )}
      </DialogActions>
    </Dialog>
  );
};

export default CampaignWizard;
