import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Grid,
  IconButton,
  List,
  ListItem,
  ListItemSecondaryAction,
  ListItemText,
  MenuItem,
  Select,
  TextField,
  Tooltip,
  Typography,
  FormControlLabel,
  Checkbox,
  Alert,
  CircularProgress,
  InputLabel,
  FormControl,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  Edit as EditIcon,
  Star as StarIcon,
  StarOutline as StarOutlineIcon,
  Email as EmailIcon,
  VerifiedUser as VerifiedIcon,
  Block as BlockIcon,
  Campaign as MarketingIcon,
} from '@mui/icons-material';
import { contactInfoService, EntityType, LinkedEmailDto, CreateEmailAddressDto, EmailType } from '../../services/contactInfoService';

interface EmailManagerProps {
  entityType: EntityType;
  entityId: number;
  readOnly?: boolean;
  onEmailChange?: () => void;
}

const EMAIL_TYPES: EmailType[] = ['Personal', 'Work', 'Invoicing', 'Support', 'Marketing', 'General', 'Other'];

const EmailManager: React.FC<EmailManagerProps> = ({
  entityType,
  entityId,
  readOnly = false,
  onEmailChange,
}) => {
  const [emails, setEmails] = useState<LinkedEmailDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingEmail, setEditingEmail] = useState<LinkedEmailDto | null>(null);
  const [formData, setFormData] = useState<CreateEmailAddressDto & { 
    emailType: EmailType; 
    isPrimary: boolean; 
    doNotEmail: boolean;
    marketingOptIn: boolean;
    transactionalOnly: boolean;
  }>({
    email: '',
    displayName: '',
    notes: '',
    emailType: 'Work',
    isPrimary: false,
    doNotEmail: false,
    marketingOptIn: true,
    transactionalOnly: false,
  });

  const fetchEmails = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await contactInfoService.getEmailAddresses(entityType, entityId);
      setEmails(response.data || []);
    } catch (err) {
      setError('Failed to load email addresses');
      console.error('Error fetching emails:', err);
    } finally {
      setLoading(false);
    }
  }, [entityType, entityId]);

  useEffect(() => {
    if (entityId) {
      fetchEmails();
    }
  }, [entityId, fetchEmails]);

  const handleOpenDialog = (email?: LinkedEmailDto) => {
    if (email) {
      setEditingEmail(email);
      setFormData({
        email: email.email,
        displayName: email.displayName || '',
        notes: email.notes || '',
        emailType: email.emailType,
        isPrimary: email.isPrimary,
        doNotEmail: email.doNotEmail,
        marketingOptIn: email.marketingOptIn,
        transactionalOnly: email.transactionalOnly,
      });
    } else {
      setEditingEmail(null);
      setFormData({
        email: '',
        displayName: '',
        notes: '',
        emailType: 'Work',
        isPrimary: emails.length === 0,
        doNotEmail: false,
        marketingOptIn: true,
        transactionalOnly: false,
      });
    }
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setEditingEmail(null);
  };

  const handleSave = async () => {
    try {
      const { emailType, isPrimary, doNotEmail, marketingOptIn, transactionalOnly, ...emailData } = formData;

      if (editingEmail) {
        await contactInfoService.updateEmailAddress(editingEmail.id, emailData);
        // Update preferences on the link
        await contactInfoService.updateEmailPreferences(editingEmail.linkId, {
          doNotEmail,
          marketingOptIn,
          transactionalOnly,
        });
        if (isPrimary && !editingEmail.isPrimary) {
          await contactInfoService.setPrimaryEmail(entityType, entityId, editingEmail.id, emailType);
        }
      } else {
        await contactInfoService.linkEmailAddress({
          newEmail: emailData,
          entityType,
          entityId,
          emailType,
          isPrimary,
          doNotEmail,
          marketingOptIn,
          transactionalOnly,
        });
      }

      await fetchEmails();
      handleCloseDialog();
      onEmailChange?.();
    } catch (err) {
      setError('Failed to save email address');
      console.error('Error saving email:', err);
    }
  };

  const handleDelete = async (email: LinkedEmailDto) => {
    if (window.confirm('Are you sure you want to remove this email address?')) {
      try {
        await contactInfoService.unlinkEmailAddress(email.linkId);
        await fetchEmails();
        onEmailChange?.();
      } catch (err) {
        setError('Failed to remove email address');
        console.error('Error deleting email:', err);
      }
    }
  };

  const handleSetPrimary = async (email: LinkedEmailDto) => {
    try {
      await contactInfoService.setPrimaryEmail(entityType, entityId, email.id, email.emailType);
      await fetchEmails();
      onEmailChange?.();
    } catch (err) {
      setError('Failed to set primary email');
      console.error('Error setting primary email:', err);
    }
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" p={3}>
        <CircularProgress size={24} />
      </Box>
    );
  }

  return (
    <Card variant="outlined">
      <CardHeader
        title="Email Addresses"
        titleTypographyProps={{ variant: 'h6' }}
        action={
          !readOnly && (
            <Button
              startIcon={<AddIcon />}
              size="small"
              onClick={() => handleOpenDialog()}
            >
              Add Email
            </Button>
          )
        }
      />
      <CardContent>
        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        {emails.length === 0 ? (
          <Typography variant="body2" color="textSecondary">
            No email addresses on file
          </Typography>
        ) : (
          <List dense>
            {emails.map((email) => (
              <ListItem key={email.linkId} divider>
                <Box mr={1}>
                  <EmailIcon fontSize="small" />
                </Box>
                <ListItemText
                  primary={
                    <Box display="flex" alignItems="center" gap={1} flexWrap="wrap">
                      <Typography variant="body1">
                        {email.displayName ? `${email.displayName} <${email.email}>` : email.email}
                      </Typography>
                      <Chip
                        label={email.emailType}
                        size="small"
                        variant="outlined"
                      />
                      {email.isPrimary && (
                        <Chip
                          label="Primary"
                          size="small"
                          color="primary"
                        />
                      )}
                      {email.isVerified && (
                        <Chip
                          icon={<VerifiedIcon />}
                          label="Verified"
                          size="small"
                          color="success"
                          variant="outlined"
                        />
                      )}
                    </Box>
                  }
                  secondary={
                    <Box display="flex" gap={1} mt={0.5} flexWrap="wrap">
                      {email.doNotEmail && (
                        <Chip
                          icon={<BlockIcon />}
                          label="Do Not Email"
                          size="small"
                          color="error"
                          variant="outlined"
                        />
                      )}
                      {email.marketingOptIn && !email.doNotEmail && email.canSendMarketing && (
                        <Chip
                          icon={<MarketingIcon />}
                          label="Marketing OK"
                          size="small"
                          color="success"
                          variant="outlined"
                        />
                      )}
                      {email.transactionalOnly && (
                        <Chip
                          label="Transactional Only"
                          size="small"
                          variant="outlined"
                        />
                      )}
                      {email.hardBounce && (
                        <Chip
                          label="Hard Bounce"
                          size="small"
                          color="error"
                        />
                      )}
                      {email.bounceCount && email.bounceCount > 0 && !email.hardBounce && (
                        <Typography variant="caption" color="warning.main">
                          {email.bounceCount} bounce(s)
                        </Typography>
                      )}
                    </Box>
                  }
                />
                {!readOnly && (
                  <ListItemSecondaryAction>
                    {!email.isPrimary && (
                      <Tooltip title="Set as Primary">
                        <IconButton size="small" onClick={() => handleSetPrimary(email)}>
                          <StarOutlineIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    )}
                    {email.isPrimary && (
                      <Tooltip title="Primary Email">
                        <IconButton size="small" disabled>
                          <StarIcon fontSize="small" color="primary" />
                        </IconButton>
                      </Tooltip>
                    )}
                    <Tooltip title="Edit">
                      <IconButton size="small" onClick={() => handleOpenDialog(email)}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Remove">
                      <IconButton size="small" onClick={() => handleDelete(email)}>
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </ListItemSecondaryAction>
                )}
              </ListItem>
            ))}
          </List>
        )}
      </CardContent>

      {/* Add/Edit Dialog */}
      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>
          {editingEmail ? 'Edit Email Address' : 'Add Email Address'}
        </DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12}>
              <FormControl fullWidth size="small">
                <InputLabel>Email Type</InputLabel>
                <Select
                  value={formData.emailType}
                  onChange={(e) => setFormData({ ...formData, emailType: e.target.value as EmailType })}
                  label="Email Type"
                >
                  {EMAIL_TYPES.map((type) => (
                    <MenuItem key={type} value={type}>
                      {type}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Email Address"
                type="email"
                value={formData.email}
                onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                required
                size="small"
                placeholder="email@example.com"
              />
            </Grid>

            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Display Name"
                value={formData.displayName}
                onChange={(e) => setFormData({ ...formData, displayName: e.target.value })}
                size="small"
                placeholder="John Doe"
              />
            </Grid>

            <Grid item xs={12}>
              <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                Communication Preferences
              </Typography>
              <Box display="flex" flexDirection="column" gap={1}>
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={formData.isPrimary}
                      onChange={(e) => setFormData({ ...formData, isPrimary: e.target.checked })}
                    />
                  }
                  label="Primary Email"
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={formData.marketingOptIn}
                      onChange={(e) => setFormData({ ...formData, marketingOptIn: e.target.checked })}
                      disabled={formData.doNotEmail}
                    />
                  }
                  label="Marketing Opt-In (can send marketing emails)"
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={formData.transactionalOnly}
                      onChange={(e) => setFormData({ ...formData, transactionalOnly: e.target.checked })}
                      disabled={formData.doNotEmail}
                    />
                  }
                  label="Transactional Only (invoices, receipts only)"
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={formData.doNotEmail}
                      onChange={(e) => setFormData({ ...formData, doNotEmail: e.target.checked })}
                    />
                  }
                  label="Do Not Email"
                />
              </Box>
            </Grid>

            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Notes"
                value={formData.notes}
                onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                multiline
                rows={2}
                size="small"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSave} variant="contained" color="primary">
            {editingEmail ? 'Save Changes' : 'Add Email'}
          </Button>
        </DialogActions>
      </Dialog>
    </Card>
  );
};

export default EmailManager;
