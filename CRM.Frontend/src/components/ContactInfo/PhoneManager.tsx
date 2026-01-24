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
  Phone as PhoneIcon,
  Smartphone as SmartphoneIcon,
  PhoneAndroid as MobileIcon,
  Fax as FaxIcon,
  Block as BlockIcon,
} from '@mui/icons-material';
import { contactInfoService, EntityType, LinkedPhoneDto, CreatePhoneNumberDto, PhoneType } from '../../services/contactInfoService';

interface PhoneManagerProps {
  entityType: EntityType;
  entityId: number;
  readOnly?: boolean;
  onPhoneChange?: () => void;
}

const PHONE_TYPES: PhoneType[] = ['Mobile', 'Home', 'Office', 'Direct', 'Fax', 'Toll-Free', 'Pager', 'Other'];

const PhoneManager: React.FC<PhoneManagerProps> = ({
  entityType,
  entityId,
  readOnly = false,
  onPhoneChange,
}) => {
  const [phones, setPhones] = useState<LinkedPhoneDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingPhone, setEditingPhone] = useState<LinkedPhoneDto | null>(null);
  const [formData, setFormData] = useState<CreatePhoneNumberDto & { phoneType: PhoneType; isPrimary: boolean; doNotCall: boolean }>({
    countryCode: '+1',
    areaCode: '',
    number: '',
    extension: '',
    canSMS: false,
    canWhatsApp: false,
    canFax: false,
    bestTimeToCall: '',
    notes: '',
    phoneType: 'Office',
    isPrimary: false,
    doNotCall: false,
  });

  const fetchPhones = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await contactInfoService.getPhoneNumbers(entityType, entityId);
      setPhones(response.data || []);
    } catch (err) {
      setError('Failed to load phone numbers');
      console.error('Error fetching phones:', err);
    } finally {
      setLoading(false);
    }
  }, [entityType, entityId]);

  useEffect(() => {
    if (entityId) {
      fetchPhones();
    }
  }, [entityId, fetchPhones]);

  const handleOpenDialog = (phone?: LinkedPhoneDto) => {
    if (phone) {
      setEditingPhone(phone);
      setFormData({
        countryCode: phone.countryCode,
        areaCode: phone.areaCode || '',
        number: phone.number,
        extension: phone.extension || '',
        canSMS: phone.canSMS || false,
        canWhatsApp: phone.canWhatsApp || false,
        canFax: phone.canFax || false,
        bestTimeToCall: phone.bestTimeToCall || '',
        notes: phone.notes || '',
        phoneType: phone.phoneType,
        isPrimary: phone.isPrimary,
        doNotCall: phone.doNotCall,
      });
    } else {
      setEditingPhone(null);
      setFormData({
        countryCode: '+1',
        areaCode: '',
        number: '',
        extension: '',
        canSMS: false,
        canWhatsApp: false,
        canFax: false,
        bestTimeToCall: '',
        notes: '',
        phoneType: 'Office',
        isPrimary: phones.length === 0,
        doNotCall: false,
      });
    }
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setEditingPhone(null);
  };

  const handleSave = async () => {
    try {
      const { phoneType, isPrimary, doNotCall, ...phoneData } = formData;

      if (editingPhone) {
        await contactInfoService.updatePhoneNumber(editingPhone.id, phoneData);
        if (isPrimary && !editingPhone.isPrimary) {
          await contactInfoService.setPrimaryPhone(entityType, entityId, editingPhone.id, phoneType);
        }
      } else {
        await contactInfoService.linkPhoneNumber({
          newPhone: phoneData,
          entityType,
          entityId,
          phoneType,
          isPrimary,
          doNotCall,
        });
      }

      await fetchPhones();
      handleCloseDialog();
      onPhoneChange?.();
    } catch (err) {
      setError('Failed to save phone number');
      console.error('Error saving phone:', err);
    }
  };

  const handleDelete = async (phone: LinkedPhoneDto) => {
    if (window.confirm('Are you sure you want to remove this phone number?')) {
      try {
        await contactInfoService.unlinkPhoneNumber(phone.linkId);
        await fetchPhones();
        onPhoneChange?.();
      } catch (err) {
        setError('Failed to remove phone number');
        console.error('Error deleting phone:', err);
      }
    }
  };

  const handleSetPrimary = async (phone: LinkedPhoneDto) => {
    try {
      await contactInfoService.setPrimaryPhone(entityType, entityId, phone.id, phone.phoneType);
      await fetchPhones();
      onPhoneChange?.();
    } catch (err) {
      setError('Failed to set primary phone');
      console.error('Error setting primary phone:', err);
    }
  };

  const getPhoneIcon = (phoneType: PhoneType) => {
    switch (phoneType) {
      case 'Mobile':
        return <MobileIcon fontSize="small" />;
      case 'Fax':
        return <FaxIcon fontSize="small" />;
      case 'Office':
      case 'Direct':
        return <PhoneIcon fontSize="small" />;
      default:
        return <SmartphoneIcon fontSize="small" />;
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
        title="Phone Numbers"
        titleTypographyProps={{ variant: 'h6' }}
        action={
          !readOnly && (
            <Button
              startIcon={<AddIcon />}
              size="small"
              onClick={() => handleOpenDialog()}
            >
              Add Phone
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

        {phones.length === 0 ? (
          <Typography variant="body2" color="textSecondary">
            No phone numbers on file
          </Typography>
        ) : (
          <List dense>
            {phones.map((phone) => (
              <ListItem key={phone.linkId} divider>
                <Box mr={1}>{getPhoneIcon(phone.phoneType)}</Box>
                <ListItemText
                  primary={
                    <Box display="flex" alignItems="center" gap={1}>
                      <Typography variant="body1">
                        {phone.formattedNumber || phone.fullNumber || `${phone.countryCode} ${phone.areaCode} ${phone.number}`}
                        {phone.extension && ` x${phone.extension}`}
                      </Typography>
                      <Chip
                        label={phone.phoneType}
                        size="small"
                        variant="outlined"
                      />
                      {phone.isPrimary && (
                        <Chip
                          label="Primary"
                          size="small"
                          color="primary"
                        />
                      )}
                      {phone.doNotCall && (
                        <Chip
                          icon={<BlockIcon />}
                          label="Do Not Call"
                          size="small"
                          color="error"
                          variant="outlined"
                        />
                      )}
                    </Box>
                  }
                  secondary={
                    <Box display="flex" gap={1} mt={0.5}>
                      {phone.canSMS && <Chip label="SMS" size="small" variant="outlined" />}
                      {phone.canWhatsApp && <Chip label="WhatsApp" size="small" variant="outlined" />}
                      {phone.canFax && <Chip label="Fax" size="small" variant="outlined" />}
                      {phone.bestTimeToCall && (
                        <Typography variant="caption" color="textSecondary">
                          Best time: {phone.bestTimeToCall}
                        </Typography>
                      )}
                    </Box>
                  }
                />
                {!readOnly && (
                  <ListItemSecondaryAction>
                    {!phone.isPrimary && (
                      <Tooltip title="Set as Primary">
                        <IconButton size="small" onClick={() => handleSetPrimary(phone)}>
                          <StarOutlineIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    )}
                    {phone.isPrimary && (
                      <Tooltip title="Primary Phone">
                        <IconButton size="small" disabled>
                          <StarIcon fontSize="small" color="primary" />
                        </IconButton>
                      </Tooltip>
                    )}
                    <Tooltip title="Edit">
                      <IconButton size="small" onClick={() => handleOpenDialog(phone)}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Remove">
                      <IconButton size="small" onClick={() => handleDelete(phone)}>
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
          {editingPhone ? 'Edit Phone Number' : 'Add Phone Number'}
        </DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12}>
              <FormControl fullWidth size="small">
                <InputLabel>Phone Type</InputLabel>
                <Select
                  value={formData.phoneType}
                  onChange={(e) => setFormData({ ...formData, phoneType: e.target.value as PhoneType })}
                  label="Phone Type"
                >
                  {PHONE_TYPES.map((type) => (
                    <MenuItem key={type} value={type}>
                      {type}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            <Grid item xs={4}>
              <TextField
                fullWidth
                label="Country Code"
                value={formData.countryCode}
                onChange={(e) => setFormData({ ...formData, countryCode: e.target.value })}
                size="small"
                placeholder="+1"
              />
            </Grid>
            <Grid item xs={4}>
              <TextField
                fullWidth
                label="Area Code"
                value={formData.areaCode}
                onChange={(e) => setFormData({ ...formData, areaCode: e.target.value })}
                size="small"
                placeholder="555"
              />
            </Grid>
            <Grid item xs={4}>
              <TextField
                fullWidth
                label="Extension"
                value={formData.extension}
                onChange={(e) => setFormData({ ...formData, extension: e.target.value })}
                size="small"
              />
            </Grid>

            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Phone Number"
                value={formData.number}
                onChange={(e) => setFormData({ ...formData, number: e.target.value })}
                required
                size="small"
                placeholder="555-1234"
              />
            </Grid>

            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Best Time to Call"
                value={formData.bestTimeToCall}
                onChange={(e) => setFormData({ ...formData, bestTimeToCall: e.target.value })}
                size="small"
                placeholder="e.g., 9AM-5PM EST"
              />
            </Grid>

            <Grid item xs={12}>
              <Box display="flex" flexWrap="wrap" gap={2}>
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={formData.canSMS || false}
                      onChange={(e) => setFormData({ ...formData, canSMS: e.target.checked })}
                    />
                  }
                  label="Can receive SMS"
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={formData.canWhatsApp || false}
                      onChange={(e) => setFormData({ ...formData, canWhatsApp: e.target.checked })}
                    />
                  }
                  label="WhatsApp"
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={formData.canFax || false}
                      onChange={(e) => setFormData({ ...formData, canFax: e.target.checked })}
                    />
                  }
                  label="Can Fax"
                />
              </Box>
            </Grid>

            <Grid item xs={12}>
              <Box display="flex" flexWrap="wrap" gap={2}>
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={formData.isPrimary}
                      onChange={(e) => setFormData({ ...formData, isPrimary: e.target.checked })}
                    />
                  }
                  label="Primary Phone"
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={formData.doNotCall}
                      onChange={(e) => setFormData({ ...formData, doNotCall: e.target.checked })}
                    />
                  }
                  label="Do Not Call"
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
            {editingPhone ? 'Save Changes' : 'Add Phone'}
          </Button>
        </DialogActions>
      </Dialog>
    </Card>
  );
};

export default PhoneManager;
