import React, { useState, useEffect, useCallback } from 'react';
import { TabPanel } from '../common';
import {
  Box,
  Grid,
  Tabs,
  Tab,
  Typography,
  Alert,
  CircularProgress,
  Paper,
} from '@mui/material';
import {
  LocationOn as AddressIcon,
  Phone as PhoneIcon,
  Email as EmailIcon,
  Share as SocialIcon,
  Language as WebIcon,
} from '@mui/icons-material';
import AddressManager from './AddressManager';
import PhoneManager from './PhoneManager';
import EmailManager from './EmailManager';
import SocialMediaManager from './SocialMediaManager';
import { contactInfoService, EntityType, EntityContactInfoDto } from '../../services/contactInfoService';

interface ContactInfoPanelProps {
  entityType: EntityType;
  entityId: number;
  readOnly?: boolean;
  layout?: 'tabs' | 'grid' | 'stacked';
  showCounts?: boolean;
  onContactInfoChange?: () => void;
}

/**
 * ContactInfoPanel - A comprehensive panel for managing all contact information
 * Can be used in Customer, Contact, Lead, or Account detail views
 */
const ContactInfoPanel: React.FC<ContactInfoPanelProps> = ({
  entityType,
  entityId,
  readOnly = false,
  layout = 'tabs',
  showCounts = true,
  onContactInfoChange,
}) => {
  const [tabValue, setTabValue] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [contactInfo, setContactInfo] = useState<EntityContactInfoDto | null>(null);

  const fetchContactInfo = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await contactInfoService.getEntityContactInfo(entityType, entityId);
      setContactInfo(response.data);
    } catch (err) {
      setError('Failed to load contact information');
      console.error('Error fetching contact info:', err);
    } finally {
      setLoading(false);
    }
  }, [entityType, entityId]);

  useEffect(() => {
    if (entityId) {
      fetchContactInfo();
    }
  }, [entityId, fetchContactInfo]);

  const handleContactInfoChange = useCallback(() => {
    fetchContactInfo();
    onContactInfoChange?.();
  }, [fetchContactInfo, onContactInfoChange]);

  const getTabLabel = (label: string, count: number) => {
    if (!showCounts) return label;
    return `${label} (${count})`;
  };

  if (loading && !contactInfo) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight={200}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ mb: 2 }}>
        {error}
      </Alert>
    );
  }

  // Tab Layout
  if (layout === 'tabs') {
    return (
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Tabs
          value={tabValue}
          onChange={(_, newValue) => setTabValue(newValue)}
          variant="scrollable"
          scrollButtons="auto"
          sx={{ borderBottom: 1, borderColor: 'divider' }}
        >
          <Tab
            icon={<AddressIcon />}
            iconPosition="start"
            label={getTabLabel('Addresses', contactInfo?.addresses?.length || 0)}
          />
          <Tab
            icon={<PhoneIcon />}
            iconPosition="start"
            label={getTabLabel('Phones', contactInfo?.phoneNumbers?.length || 0)}
          />
          <Tab
            icon={<EmailIcon />}
            iconPosition="start"
            label={getTabLabel('Emails', contactInfo?.emailAddresses?.length || 0)}
          />
          <Tab
            icon={<WebIcon />}
            iconPosition="start"
            label={getTabLabel('Social & Web', contactInfo?.socialMediaAccounts?.length || 0)}
          />
        </Tabs>

        <TabPanel value={tabValue} index={0}>
          <AddressManager
            entityType={entityType}
            entityId={entityId}
            readOnly={readOnly}
            onAddressChange={handleContactInfoChange}
          />
        </TabPanel>
        <TabPanel value={tabValue} index={1}>
          <PhoneManager
            entityType={entityType}
            entityId={entityId}
            readOnly={readOnly}
            onPhoneChange={handleContactInfoChange}
          />
        </TabPanel>
        <TabPanel value={tabValue} index={2}>
          <EmailManager
            entityType={entityType}
            entityId={entityId}
            readOnly={readOnly}
            onEmailChange={handleContactInfoChange}
          />
        </TabPanel>
        <TabPanel value={tabValue} index={3}>
          <SocialMediaManager
            entityType={entityType}
            entityId={entityId}
            readOnly={readOnly}
            onSocialMediaChange={handleContactInfoChange}
          />
        </TabPanel>
      </Paper>
    );
  }

  // Grid Layout (2 columns)
  if (layout === 'grid') {
    return (
      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <AddressManager
            entityType={entityType}
            entityId={entityId}
            readOnly={readOnly}
            onAddressChange={handleContactInfoChange}
          />
        </Grid>
        <Grid item xs={12} md={6}>
          <PhoneManager
            entityType={entityType}
            entityId={entityId}
            readOnly={readOnly}
            onPhoneChange={handleContactInfoChange}
          />
        </Grid>
        <Grid item xs={12} md={6}>
          <EmailManager
            entityType={entityType}
            entityId={entityId}
            readOnly={readOnly}
            onEmailChange={handleContactInfoChange}
          />
        </Grid>
        <Grid item xs={12} md={6}>
          <SocialMediaManager
            entityType={entityType}
            entityId={entityId}
            readOnly={readOnly}
            onSocialMediaChange={handleContactInfoChange}
          />
        </Grid>
      </Grid>
    );
  }

  // Stacked Layout (full width, vertically stacked)
  return (
    <Box display="flex" flexDirection="column" gap={3}>
      <AddressManager
        entityType={entityType}
        entityId={entityId}
        readOnly={readOnly}
        onAddressChange={handleContactInfoChange}
      />
      <PhoneManager
        entityType={entityType}
        entityId={entityId}
        readOnly={readOnly}
        onPhoneChange={handleContactInfoChange}
      />
      <EmailManager
        entityType={entityType}
        entityId={entityId}
        readOnly={readOnly}
        onEmailChange={handleContactInfoChange}
      />
      <SocialMediaManager
        entityType={entityType}
        entityId={entityId}
        readOnly={readOnly}
        onSocialMediaChange={handleContactInfoChange}
      />
    </Box>
  );
};

export default ContactInfoPanel;
