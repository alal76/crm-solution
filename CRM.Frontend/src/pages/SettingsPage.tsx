import React, { useState, useEffect } from 'react';
import {
  Container,
  Box,
  Tabs,
  Tab,
  Alert,
  CircularProgress,
  Typography,
  Paper,
  Card,
  CardContent,
  TextField,
  Button,
  Grid,
  Avatar,
} from '@mui/material';
import {
  PersonAdd as PersonAddIcon,
  Groups as GroupsIcon,
  Storage as StorageIcon,
  Business as BusinessIcon,
} from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';
import UserApprovalTab from '../components/settings/UserApprovalTab';
import GroupManagementTab from '../components/settings/GroupManagementTab';
import DatabaseSettingsTab from '../components/settings/DatabaseSettingsTab';

interface SettingsTab {
  id: string;
  label: string;
  icon: React.ReactNode;
  component: React.ReactNode;
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: string;
  value: string;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index } = props;
  return (
    <div hidden={value !== index} style={{ width: '100%' }}>
      {value === index && <Box sx={{ py: 3 }}>{children}</Box>}
    </div>
  );
}

function CompanyBrandingTab() {
  const [formData, setFormData] = useState({
    companyName: 'Vibe CRM Solutions',
    companyLogo: 'https://via.placeholder.com/150?text=Logo',
    primaryColor: '#6750A4',
    secondaryColor: '#625B71',
    companyWebsite: 'https://www.example.com',
    companyEmail: 'support@example.com',
    companyPhone: '(555) 000-0000',
  });
  const [logoFile, setLogoFile] = useState<File | null>(null);
  const [logoPreview, setLogoPreview] = useState(formData.companyLogo);
  const [saved, setSaved] = useState(false);

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      setLogoFile(file);
      const reader = new FileReader();
      reader.onloadend = () => {
        setLogoPreview(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleInputChange = (field: string, value: string) => {
    setFormData({ ...formData, [field]: value });
  };

  const handleSave = async () => {
    try {
      // In a real app, this would upload to the backend
      console.log('Saving company settings:', formData);
      setSaved(true);
      setTimeout(() => setSaved(false), 3000);
    } catch (err) {
      console.error('Error saving settings:', err);
    }
  };

  return (
    <Box>
      <Typography variant="h6" sx={{ fontWeight: 600, mb: 3 }}>
        Company Branding & Settings
      </Typography>

      {saved && (
        <Alert severity="success" sx={{ mb: 2, borderRadius: 2 }}>
          Company settings saved successfully!
        </Alert>
      )}

      <Grid container spacing={3}>
        <Grid item xs={12} md={4}>
          <Card sx={{ borderRadius: 3, boxShadow: 1, textAlign: 'center' }}>
            <CardContent>
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 2, color: '#625B71' }}>
                Company Logo
              </Typography>
              <Avatar
                src={logoPreview}
                sx={{
                  width: 120,
                  height: 120,
                  margin: '0 auto',
                  mb: 2,
                  backgroundColor: '#E8DEF8',
                }}
              />
              <Button
                variant="outlined"
                component="label"
                sx={{
                  textTransform: 'none',
                  color: '#6750A4',
                  borderColor: '#6750A4',
                  width: '100%',
                }}
              >
                Upload Logo
                <input
                  hidden
                  accept="image/*"
                  type="file"
                  onChange={handleFileChange}
                />
              </Button>
              <Typography variant="caption" sx={{ color: '#79747E', display: 'block', mt: 1 }}>
                Recommended: 150x150px PNG/JPG
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={8}>
          <Paper sx={{ borderRadius: 3, p: 3 }}>
            <Grid container spacing={2}>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Company Name"
                  value={formData.companyName}
                  onChange={(e) => handleInputChange('companyName', e.target.value)}
                  variant="outlined"
                  sx={{ borderRadius: 2 }}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Company Website"
                  value={formData.companyWebsite}
                  onChange={(e) => handleInputChange('companyWebsite', e.target.value)}
                  variant="outlined"
                  type="url"
                />
              </Grid>

              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Support Email"
                  value={formData.companyEmail}
                  onChange={(e) => handleInputChange('companyEmail', e.target.value)}
                  variant="outlined"
                  type="email"
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Support Phone"
                  value={formData.companyPhone}
                  onChange={(e) => handleInputChange('companyPhone', e.target.value)}
                  variant="outlined"
                />
              </Grid>

              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Primary Brand Color"
                  value={formData.primaryColor}
                  onChange={(e) => handleInputChange('primaryColor', e.target.value)}
                  variant="outlined"
                  type="color"
                  InputProps={{
                    style: { height: 56 },
                  }}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Secondary Brand Color"
                  value={formData.secondaryColor}
                  onChange={(e) => handleInputChange('secondaryColor', e.target.value)}
                  variant="outlined"
                  type="color"
                  InputProps={{
                    style: { height: 56 },
                  }}
                />
              </Grid>

              <Grid item xs={12}>
                <Button
                  variant="contained"
                  onClick={handleSave}
                  sx={{
                    backgroundColor: '#6750A4',
                    textTransform: 'none',
                    borderRadius: 2,
                    px: 4,
                  }}
                >
                  Save Branding Settings
                </Button>
              </Grid>
            </Grid>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
}

function SettingsPage() {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState('branding');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    // Simulate loading
    setLoading(false);
  }, []);

  if (error) {
    return (
      <Alert severity="error" sx={{ mt: 2 }}>
        {error}
      </Alert>
    );
  }

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  const tabs: SettingsTab[] = [
    {
      id: 'branding',
      label: 'Company Branding',
      icon: <BusinessIcon sx={{ mr: 0.5, fontSize: 20 }} />,
      component: <CompanyBrandingTab />,
    },
    {
      id: 'approvals',
      label: 'User Approvals',
      icon: <PersonAddIcon sx={{ mr: 0.5, fontSize: 20 }} />,
      component: <UserApprovalTab />,
    },
    {
      id: 'groups',
      label: 'Group Management',
      icon: <GroupsIcon sx={{ mr: 0.5, fontSize: 20 }} />,
      component: <GroupManagementTab />,
    },
    {
      id: 'database',
      label: 'Database Settings',
      icon: <StorageIcon sx={{ mr: 0.5, fontSize: 20 }} />,
      component: <DatabaseSettingsTab />,
    },
  ];

  return (
    <Box sx={{ py: 2 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h3" sx={{ fontWeight: 700, mb: 0.5 }}>
          Settings
        </Typography>
        <Typography color="textSecondary" variant="body2">
          Manage your CRM configuration and preferences
        </Typography>
      </Box>

      <Paper sx={{ borderRadius: 3, boxShadow: 1 }}>
        <Tabs
          value={activeTab}
          onChange={(e, newValue) => setActiveTab(newValue)}
          variant="fullWidth"
          sx={{
            borderBottom: '2px solid #E0E0E0',
            '& .MuiTab-root': {
              textTransform: 'none',
              fontSize: '1rem',
              fontWeight: 500,
            },
          }}
        >
          {tabs.map((tab) => (
            <Tab
              key={tab.id}
              value={tab.id}
              label={
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  {tab.icon}
                  {tab.label}
                </Box>
              }
              sx={{
                textTransform: 'none',
                fontSize: '1rem',
                fontWeight: 500,
                py: 2,
              }}
            />
          ))}
        </Tabs>

        {tabs.map((tab) => (
          <TabPanel key={tab.id} value={activeTab} index={tab.id}>
            <Container maxWidth="lg" sx={{ px: 3, py: 0 }}>
              {tab.component}
            </Container>
          </TabPanel>
        ))}
      </Paper>
    </Box>
  );
}

export default SettingsPage;
