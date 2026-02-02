import React, { useState, useEffect } from 'react';
import {
  Box,
  CircularProgress,
  Typography,
  Paper,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Alert,
  Grid,
  Avatar,
  Card,
  CardContent,
} from '@mui/material';
import {
  PersonAdd as PersonAddIcon,
  Groups as GroupsIcon,
  Business as BusinessIcon,
  ToggleOn as ModuleIcon,
  People as PeopleIcon,
  Login as LoginIcon,
  Security as SecurityIcon,
  Menu as MenuIcon,
  SupportAgent as SupportAgentIcon,
  ExpandMore as ExpandMoreIcon,
  AdminPanelSettings as AdminPanelSettingsIcon,
  ManageAccounts as ManageAccountsIcon,
  Hub as HubIcon,
  Settings as SettingsIcon,
} from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';
import UserApprovalTab from '../components/settings/UserApprovalTab';
import GroupManagementTab from '../components/settings/GroupManagementTab';
import ModuleFieldSettingsTab from '../components/settings/ModuleFieldSettingsTabNew';
import UserManagementTab from '../components/settings/UserManagementTab';
import SocialLoginSettingsTab from '../components/settings/SocialLoginSettingsTab';
import SecuritySettingsTab from '../components/settings/SecuritySettingsTab';
import NavigationSettingsTab from '../components/settings/NavigationSettingsTab';
import ServiceRequestSettingsTab from '../components/settings/ServiceRequestSettingsTab';
import DeploymentSettingsTab from '../components/settings/DeploymentSettingsTab';
import MonitoringDashboard from './admin/MonitoringDashboard';
import MasterDataSettingsTab from '../components/settings/MasterDataSettingsTab';
import FeatureManagementTab from '../components/settings/FeatureManagementTab';
import CompanyBrandingTab from '../components/settings/CompanyBrandingTab';
import logo from '../assets/logo.png';
import {
  Cloud as CloudIcon,
  Monitor as MonitorIcon,
  Storage as MasterDataIcon,
  ToggleOn as FeatureToggleIcon,
} from '@mui/icons-material';

interface SettingsTab {
  id: string;
  label: string;
  icon: React.ReactNode;
  component: React.ReactNode;
}

// Settings section configuration
interface SettingsSection {
  id: string;
  title: string;
  description: string;
  icon: React.ReactNode;
  color: string;
  items: SettingsTab[];
}

function SettingsPage() {
  useAuth();
  const [expandedSection, setExpandedSection] = useState<string | false>('crmadmin');
  const [activeTab, setActiveTab] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error] = useState('');

  useEffect(() => {
    setLoading(false);
  }, []);

  // Define the three main sections
  const sections: SettingsSection[] = [
    {
      id: 'systemadmin',
      title: 'System Administration',
      description: 'Core technical settings, deployment, monitoring, and security',
      icon: <AdminPanelSettingsIcon />,
      color: '#1976d2',
      items: [
        {
          id: 'monitoring',
          label: 'Monitoring',
          icon: <MonitorIcon sx={{ fontSize: 20 }} />,
          component: <MonitoringDashboard />,
        },
        {
          id: 'deployment',
          label: 'Deployment & Hosting',
          icon: <CloudIcon sx={{ fontSize: 20 }} />,
          component: <DeploymentSettingsTab />,
        },
        {
          id: 'security',
          label: 'Security',
          icon: <SecurityIcon sx={{ fontSize: 20 }} />,
          component: <SecuritySettingsTab />,
        },
        {
          id: 'features',
          label: 'Features & Modules',
          icon: <FeatureToggleIcon sx={{ fontSize: 20 }} />,
          component: <FeatureManagementTab />,
        },
      ],
    },
    {
      id: 'useradmin',
      title: 'User Administration',
      description: 'User management, approvals, groups, and authentication settings',
      icon: <ManageAccountsIcon />,
      color: '#388e3c',
      items: [
        {
          id: 'users',
          label: 'User Management',
          icon: <PeopleIcon sx={{ fontSize: 20 }} />,
          component: <UserManagementTab />,
        },
        {
          id: 'approvals',
          label: 'User Approvals',
          icon: <PersonAddIcon sx={{ fontSize: 20 }} />,
          component: <UserApprovalTab />,
        },
        {
          id: 'groups',
          label: 'Group Management',
          icon: <GroupsIcon sx={{ fontSize: 20 }} />,
          component: <GroupManagementTab />,
        },
        {
          id: 'sociallogin',
          label: 'Social Login',
          icon: <LoginIcon sx={{ fontSize: 20 }} />,
          component: <SocialLoginSettingsTab />,
        },
      ],
    },
    {
      id: 'crmadmin',
      title: 'CRM Administration',
      description: 'Branding, navigation, modules, service requests, and master data',
      icon: <HubIcon />,
      color: '#7b1fa2',
      items: [
        {
          id: 'branding',
          label: 'Company Branding',
          icon: <BusinessIcon sx={{ fontSize: 20 }} />,
          component: <CompanyBrandingTab />,
        },
        {
          id: 'navigation',
          label: 'Navigation',
          icon: <MenuIcon sx={{ fontSize: 20 }} />,
          component: <NavigationSettingsTab />,
        },
        {
          id: 'modules',
          label: 'Modules & Fields',
          icon: <ModuleIcon sx={{ fontSize: 20 }} />,
          component: <ModuleFieldSettingsTab />,
        },
        {
          id: 'servicerequests',
          label: 'Service Request Definitions',
          icon: <SupportAgentIcon sx={{ fontSize: 20 }} />,
          component: <ServiceRequestSettingsTab />,
        },
        {
          id: 'masterdata',
          label: 'Master Data',
          icon: <MasterDataIcon sx={{ fontSize: 20 }} />,
          component: <MasterDataSettingsTab />,
        },
      ],
    },
  ];

  const handleAccordionChange = (sectionId: string) => (_event: React.SyntheticEvent, isExpanded: boolean) => {
    setExpandedSection(isExpanded ? sectionId : false);
    if (!isExpanded) {
      setActiveTab(null);
    }
  };

  const handleTabSelect = (tabId: string) => {
    setActiveTab(activeTab === tabId ? null : tabId);
  };

  // Find the active tab component
  const getActiveComponent = () => {
    for (const section of sections) {
      const tab = section.items.find(item => item.id === activeTab);
      if (tab) return tab.component;
    }
    return null;
  };

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

  return (
    <Box sx={{ py: 2 }}>
      {/* Header */}
      <Box sx={{ mb: 4, display: 'flex', alignItems: 'center', gap: 2 }}>
        <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
          <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
        </Box>
        <Box>
          <Typography variant="h3" sx={{ fontWeight: 700, mb: 0.5 }}>
            Settings
          </Typography>
          <Typography color="textSecondary" variant="body2">
            Manage your CRM configuration and preferences
          </Typography>
        </Box>
      </Box>

      <Grid container spacing={3}>
        {/* Left sidebar with sections */}
        <Grid item xs={12} md={4} lg={3}>
          <Box sx={{ position: 'sticky', top: 16 }}>
            {sections.map((section) => (
              <Accordion
                key={section.id}
                expanded={expandedSection === section.id}
                onChange={handleAccordionChange(section.id)}
                sx={{
                  mb: 1,
                  borderRadius: '12px !important',
                  overflow: 'hidden',
                  '&:before': { display: 'none' },
                  boxShadow: expandedSection === section.id ? 3 : 1,
                  border: expandedSection === section.id ? `2px solid ${section.color}` : '1px solid #e0e0e0',
                }}
              >
                <AccordionSummary
                  expandIcon={<ExpandMoreIcon />}
                  sx={{
                    bgcolor: expandedSection === section.id ? `${section.color}10` : 'transparent',
                    '& .MuiAccordionSummary-content': { my: 1.5 },
                  }}
                >
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                    <Avatar
                      sx={{
                        bgcolor: section.color,
                        width: 36,
                        height: 36,
                      }}
                    >
                      {section.icon}
                    </Avatar>
                    <Box>
                      <Typography variant="subtitle1" sx={{ fontWeight: 600, lineHeight: 1.2 }}>
                        {section.title}
                      </Typography>
                      <Typography variant="caption" color="textSecondary" sx={{ lineHeight: 1.2 }}>
                        {section.items.length} settings
                      </Typography>
                    </Box>
                  </Box>
                </AccordionSummary>
                <AccordionDetails sx={{ p: 0 }}>
                  <List dense sx={{ py: 0 }}>
                    {section.items.map((item) => (
                      <ListItem key={item.id} disablePadding>
                        <ListItemButton
                          selected={activeTab === item.id}
                          onClick={() => handleTabSelect(item.id)}
                          sx={{
                            py: 1.5,
                            pl: 3,
                            '&.Mui-selected': {
                              bgcolor: `${section.color}15`,
                              borderLeft: `3px solid ${section.color}`,
                              '&:hover': {
                                bgcolor: `${section.color}20`,
                              },
                            },
                          }}
                        >
                          <ListItemIcon sx={{ minWidth: 36, color: activeTab === item.id ? section.color : 'inherit' }}>
                            {item.icon}
                          </ListItemIcon>
                          <ListItemText
                            primary={item.label}
                            primaryTypographyProps={{
                              fontSize: '0.875rem',
                              fontWeight: activeTab === item.id ? 600 : 400,
                              color: activeTab === item.id ? section.color : 'inherit',
                            }}
                          />
                        </ListItemButton>
                      </ListItem>
                    ))}
                  </List>
                </AccordionDetails>
              </Accordion>
            ))}
          </Box>
        </Grid>

        {/* Right content area */}
        <Grid item xs={12} md={8} lg={9}>
          {activeTab ? (
            <Paper sx={{ borderRadius: 3, boxShadow: 1, p: 3, minHeight: 400 }}>
              {getActiveComponent()}
            </Paper>
          ) : (
            <Paper sx={{ borderRadius: 3, boxShadow: 1, p: 4, minHeight: 400, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center' }}>
              <SettingsIcon sx={{ fontSize: 80, color: 'action.disabled', mb: 2 }} />
              <Typography variant="h5" color="textSecondary" sx={{ mb: 1 }}>
                Select a Setting
              </Typography>
              <Typography variant="body2" color="textSecondary" textAlign="center" sx={{ maxWidth: 400 }}>
                Choose a section from the left panel and click on a setting to configure it.
              </Typography>
              
              {/* Quick access cards */}
              <Box sx={{ mt: 4, display: 'flex', gap: 2, flexWrap: 'wrap', justifyContent: 'center' }}>
                {sections.map((section) => (
                  <Card
                    key={section.id}
                    sx={{
                      width: 180,
                      cursor: 'pointer',
                      transition: 'all 0.2s',
                      '&:hover': {
                        transform: 'translateY(-4px)',
                        boxShadow: 4,
                      },
                    }}
                    onClick={() => {
                      setExpandedSection(section.id);
                      setActiveTab(section.items[0]?.id || null);
                    }}
                  >
                    <CardContent sx={{ textAlign: 'center', py: 3 }}>
                      <Avatar
                        sx={{
                          bgcolor: section.color,
                          width: 48,
                          height: 48,
                          mx: 'auto',
                          mb: 1.5,
                        }}
                      >
                        {section.icon}
                      </Avatar>
                      <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
                        {section.title.replace(' Administration', '')}
                      </Typography>
                      <Typography variant="caption" color="textSecondary">
                        {section.items.length} options
                      </Typography>
                    </CardContent>
                  </Card>
                ))}
              </Box>
            </Paper>
          )}
        </Grid>
      </Grid>
    </Box>
  );
}

export default SettingsPage;

