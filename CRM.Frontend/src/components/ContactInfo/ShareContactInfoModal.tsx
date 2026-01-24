import React, { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Checkbox,
  FormControlLabel,
  Typography,
  Alert,
  CircularProgress,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Divider,
  Chip,
} from '@mui/material';
import {
  LocationOn as AddressIcon,
  Phone as PhoneIcon,
  Email as EmailIcon,
  Share as SocialIcon,
} from '@mui/icons-material';
import { 
  contactInfoService, 
  EntityType, 
  LinkedAddressDto,
  LinkedPhoneDto,
  LinkedEmailDto,
  LinkedSocialMediaDto,
  AddressType,
  PhoneType,
  EmailType,
} from '../../services/contactInfoService';

interface ShareContactInfoModalProps {
  open: boolean;
  onClose: () => void;
  sourceEntityType: EntityType;
  sourceEntityId: number;
  onShareComplete?: () => void;
}

interface TargetEntity {
  type: EntityType;
  id: number;
  name: string;
}

/**
 * ShareContactInfoModal - Allows sharing contact information between entities
 * For example, sharing a customer's address with their contact
 */
const ShareContactInfoModal: React.FC<ShareContactInfoModalProps> = ({
  open,
  onClose,
  sourceEntityType,
  sourceEntityId,
  onShareComplete,
}) => {
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Available contact info from source
  const [addresses, setAddresses] = useState<LinkedAddressDto[]>([]);
  const [phones, setPhones] = useState<LinkedPhoneDto[]>([]);
  const [emails, setEmails] = useState<LinkedEmailDto[]>([]);
  const [socialMedia, setSocialMedia] = useState<LinkedSocialMediaDto[]>([]);
  
  // Selection state
  const [selectedAddresses, setSelectedAddresses] = useState<number[]>([]);
  const [selectedPhones, setSelectedPhones] = useState<number[]>([]);
  const [selectedEmails, setSelectedEmails] = useState<number[]>([]);
  const [selectedSocialMedia, setSelectedSocialMedia] = useState<number[]>([]);
  
  // Target entity
  const [targetEntityType, setTargetEntityType] = useState<EntityType>('Contact');
  const [targetEntityId, setTargetEntityId] = useState<number | ''>('');
  const [setAsPrimary, setSetAsPrimary] = useState(false);
  
  // Default types for shared items
  const [defaultAddressType, setDefaultAddressType] = useState<AddressType>('Primary');
  const [defaultPhoneType, setDefaultPhoneType] = useState<PhoneType>('Office');
  const [defaultEmailType, setDefaultEmailType] = useState<EmailType>('Work');

  useEffect(() => {
    if (open) {
      fetchSourceContactInfo();
    }
  }, [open, sourceEntityType, sourceEntityId]);

  const fetchSourceContactInfo = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await contactInfoService.getEntityContactInfo(sourceEntityType, sourceEntityId);
      setAddresses(response.data.addresses || []);
      setPhones(response.data.phoneNumbers || []);
      setEmails(response.data.emailAddresses || []);
      setSocialMedia(response.data.socialMediaAccounts || []);
    } catch (err) {
      setError('Failed to load contact information');
      console.error('Error fetching source contact info:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleShare = async () => {
    if (!targetEntityId) {
      setError('Please specify a target entity ID');
      return;
    }

    if (selectedAddresses.length === 0 && selectedPhones.length === 0 && 
        selectedEmails.length === 0 && selectedSocialMedia.length === 0) {
      setError('Please select at least one item to share');
      return;
    }

    try {
      setSaving(true);
      setError(null);

      await contactInfoService.shareContactInfo({
        targetEntityType,
        targetEntityId: targetEntityId as number,
        addressIds: selectedAddresses.length > 0 ? selectedAddresses : undefined,
        phoneIds: selectedPhones.length > 0 ? selectedPhones : undefined,
        emailIds: selectedEmails.length > 0 ? selectedEmails : undefined,
        socialMediaIds: selectedSocialMedia.length > 0 ? selectedSocialMedia : undefined,
        setAsPrimary,
        defaultAddressType,
        defaultPhoneType,
        defaultEmailType,
      });

      onShareComplete?.();
      handleClose();
    } catch (err) {
      setError('Failed to share contact information');
      console.error('Error sharing contact info:', err);
    } finally {
      setSaving(false);
    }
  };

  const handleClose = () => {
    setSelectedAddresses([]);
    setSelectedPhones([]);
    setSelectedEmails([]);
    setSelectedSocialMedia([]);
    setTargetEntityId('');
    setError(null);
    onClose();
  };

  const toggleAddress = (id: number) => {
    setSelectedAddresses(prev => 
      prev.includes(id) ? prev.filter(a => a !== id) : [...prev, id]
    );
  };

  const togglePhone = (id: number) => {
    setSelectedPhones(prev => 
      prev.includes(id) ? prev.filter(p => p !== id) : [...prev, id]
    );
  };

  const toggleEmail = (id: number) => {
    setSelectedEmails(prev => 
      prev.includes(id) ? prev.filter(e => e !== id) : [...prev, id]
    );
  };

  const toggleSocialMedia = (id: number) => {
    setSelectedSocialMedia(prev => 
      prev.includes(id) ? prev.filter(s => s !== id) : [...prev, id]
    );
  };

  const selectAll = () => {
    setSelectedAddresses(addresses.map(a => a.id));
    setSelectedPhones(phones.map(p => p.id));
    setSelectedEmails(emails.map(e => e.id));
    setSelectedSocialMedia(socialMedia.map(s => s.id));
  };

  const selectNone = () => {
    setSelectedAddresses([]);
    setSelectedPhones([]);
    setSelectedEmails([]);
    setSelectedSocialMedia([]);
  };

  const totalSelected = selectedAddresses.length + selectedPhones.length + 
                        selectedEmails.length + selectedSocialMedia.length;

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="md" fullWidth>
      <DialogTitle>
        Share Contact Information
        <Typography variant="body2" color="textSecondary">
          Share contact information from this {sourceEntityType.toLowerCase()} with another entity
        </Typography>
      </DialogTitle>
      
      <DialogContent dividers>
        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        {loading ? (
          <Box display="flex" justifyContent="center" p={4}>
            <CircularProgress />
          </Box>
        ) : (
          <>
            {/* Target Selection */}
            <Box mb={3}>
              <Typography variant="subtitle2" gutterBottom>
                Share With
              </Typography>
              <Box display="flex" gap={2} flexWrap="wrap">
                <FormControl size="small" sx={{ minWidth: 150 }}>
                  <InputLabel>Entity Type</InputLabel>
                  <Select
                    value={targetEntityType}
                    onChange={(e) => setTargetEntityType(e.target.value as EntityType)}
                    label="Entity Type"
                  >
                    <MenuItem value="Customer">Customer</MenuItem>
                    <MenuItem value="Contact">Contact</MenuItem>
                    <MenuItem value="Lead">Lead</MenuItem>
                    <MenuItem value="Account">Account</MenuItem>
                  </Select>
                </FormControl>
                
                <FormControl size="small" sx={{ minWidth: 150 }}>
                  <InputLabel>Entity ID</InputLabel>
                  <Select
                    value={targetEntityId}
                    onChange={(e) => setTargetEntityId(e.target.value as number)}
                    label="Entity ID"
                  >
                    {/* In a real app, this would be populated from a search/lookup */}
                    <MenuItem value="">Select...</MenuItem>
                    {/* Placeholder - would need entity search */}
                  </Select>
                </FormControl>

                <FormControlLabel
                  control={
                    <Checkbox
                      checked={setAsPrimary}
                      onChange={(e) => setSetAsPrimary(e.target.checked)}
                    />
                  }
                  label="Set as Primary"
                />
              </Box>
            </Box>

            <Divider sx={{ my: 2 }} />

            {/* Selection Controls */}
            <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
              <Typography variant="subtitle2">
                Select Items to Share
                {totalSelected > 0 && (
                  <Chip label={`${totalSelected} selected`} size="small" sx={{ ml: 1 }} />
                )}
              </Typography>
              <Box>
                <Button size="small" onClick={selectAll}>Select All</Button>
                <Button size="small" onClick={selectNone}>Clear</Button>
              </Box>
            </Box>

            {/* Addresses */}
            {addresses.length > 0 && (
              <Box mb={2}>
                <Typography variant="subtitle2" sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <AddressIcon fontSize="small" />
                  Addresses ({addresses.length})
                </Typography>
                <List dense>
                  {addresses.map((address) => (
                    <ListItem 
                      key={address.id} 
                      button 
                      onClick={() => toggleAddress(address.id)}
                      selected={selectedAddresses.includes(address.id)}
                    >
                      <ListItemIcon>
                        <Checkbox checked={selectedAddresses.includes(address.id)} />
                      </ListItemIcon>
                      <ListItemText
                        primary={`${address.line1}, ${address.city}, ${address.state}`}
                        secondary={address.addressType}
                      />
                    </ListItem>
                  ))}
                </List>
                <FormControl size="small" sx={{ ml: 6, minWidth: 120 }}>
                  <InputLabel>Share as</InputLabel>
                  <Select
                    value={defaultAddressType}
                    onChange={(e) => setDefaultAddressType(e.target.value as AddressType)}
                    label="Share as"
                  >
                    <MenuItem value="Primary">Primary</MenuItem>
                    <MenuItem value="Billing">Billing</MenuItem>
                    <MenuItem value="Shipping">Shipping</MenuItem>
                    <MenuItem value="Other">Other</MenuItem>
                  </Select>
                </FormControl>
              </Box>
            )}

            {/* Phones */}
            {phones.length > 0 && (
              <Box mb={2}>
                <Typography variant="subtitle2" sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <PhoneIcon fontSize="small" />
                  Phone Numbers ({phones.length})
                </Typography>
                <List dense>
                  {phones.map((phone) => (
                    <ListItem 
                      key={phone.id} 
                      button 
                      onClick={() => togglePhone(phone.id)}
                      selected={selectedPhones.includes(phone.id)}
                    >
                      <ListItemIcon>
                        <Checkbox checked={selectedPhones.includes(phone.id)} />
                      </ListItemIcon>
                      <ListItemText
                        primary={phone.formattedNumber || phone.fullNumber}
                        secondary={phone.phoneType}
                      />
                    </ListItem>
                  ))}
                </List>
                <FormControl size="small" sx={{ ml: 6, minWidth: 120 }}>
                  <InputLabel>Share as</InputLabel>
                  <Select
                    value={defaultPhoneType}
                    onChange={(e) => setDefaultPhoneType(e.target.value as PhoneType)}
                    label="Share as"
                  >
                    <MenuItem value="Mobile">Mobile</MenuItem>
                    <MenuItem value="Office">Office</MenuItem>
                    <MenuItem value="Direct">Direct</MenuItem>
                    <MenuItem value="Other">Other</MenuItem>
                  </Select>
                </FormControl>
              </Box>
            )}

            {/* Emails */}
            {emails.length > 0 && (
              <Box mb={2}>
                <Typography variant="subtitle2" sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <EmailIcon fontSize="small" />
                  Email Addresses ({emails.length})
                </Typography>
                <List dense>
                  {emails.map((email) => (
                    <ListItem 
                      key={email.id} 
                      button 
                      onClick={() => toggleEmail(email.id)}
                      selected={selectedEmails.includes(email.id)}
                    >
                      <ListItemIcon>
                        <Checkbox checked={selectedEmails.includes(email.id)} />
                      </ListItemIcon>
                      <ListItemText
                        primary={email.email}
                        secondary={email.emailType}
                      />
                    </ListItem>
                  ))}
                </List>
                <FormControl size="small" sx={{ ml: 6, minWidth: 120 }}>
                  <InputLabel>Share as</InputLabel>
                  <Select
                    value={defaultEmailType}
                    onChange={(e) => setDefaultEmailType(e.target.value as EmailType)}
                    label="Share as"
                  >
                    <MenuItem value="Work">Work</MenuItem>
                    <MenuItem value="Personal">Personal</MenuItem>
                    <MenuItem value="Invoicing">Invoicing</MenuItem>
                    <MenuItem value="Other">Other</MenuItem>
                  </Select>
                </FormControl>
              </Box>
            )}

            {/* Social Media */}
            {socialMedia.length > 0 && (
              <Box mb={2}>
                <Typography variant="subtitle2" sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <SocialIcon fontSize="small" />
                  Social Media ({socialMedia.length})
                </Typography>
                <List dense>
                  {socialMedia.map((account) => (
                    <ListItem 
                      key={account.id} 
                      button 
                      onClick={() => toggleSocialMedia(account.id)}
                      selected={selectedSocialMedia.includes(account.id)}
                    >
                      <ListItemIcon>
                        <Checkbox checked={selectedSocialMedia.includes(account.id)} />
                      </ListItemIcon>
                      <ListItemText
                        primary={account.handleOrUsername}
                        secondary={account.platformName || account.platform}
                      />
                    </ListItem>
                  ))}
                </List>
              </Box>
            )}

            {addresses.length === 0 && phones.length === 0 && emails.length === 0 && socialMedia.length === 0 && (
              <Alert severity="info">
                No contact information available to share
              </Alert>
            )}
          </>
        )}
      </DialogContent>

      <DialogActions>
        <Button onClick={handleClose} disabled={saving}>
          Cancel
        </Button>
        <Button
          variant="contained"
          color="primary"
          onClick={handleShare}
          disabled={saving || totalSelected === 0 || !targetEntityId}
          startIcon={saving ? <CircularProgress size={16} /> : undefined}
        >
          {saving ? 'Sharing...' : `Share ${totalSelected} Item${totalSelected !== 1 ? 's' : ''}`}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default ShareContactInfoModal;
