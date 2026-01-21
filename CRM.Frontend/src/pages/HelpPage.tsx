/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

import React, { useState } from 'react';
import {
  Container,
  Box,
  Typography,
  Paper,
  Grid,
  Card,
  CardContent,
  CardHeader,
  Divider,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  ListItemButton,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Chip,
  Link,
  TextField,
  InputAdornment,
} from '@mui/material';
import {
  Help as HelpIcon,
  ExpandMore as ExpandMoreIcon,
  PlayCircle as PlayCircleIcon,
  MenuBook as MenuBookIcon,
  QuestionAnswer as FAQIcon,
  Keyboard as KeyboardIcon,
  Speed as SpeedIcon,
  Security as SecurityIcon,
  People as PeopleIcon,
  Dashboard as DashboardIcon,
  ContactMail as ContactIcon,
  ShoppingCart as SalesIcon,
  Campaign as CampaignIcon,
  Task as TaskIcon,
  Search as SearchIcon,
  Settings as SettingsIcon,
  Login as LoginIcon,
} from '@mui/icons-material';

interface Tutorial {
  title: string;
  description: string;
  duration: string;
  level: 'Beginner' | 'Intermediate' | 'Advanced';
  topics: string[];
}

interface FAQ {
  question: string;
  answer: string;
}

const HelpPage: React.FC = () => {
  const [searchQuery, setSearchQuery] = useState('');
  const [expandedFaq, setExpandedFaq] = useState<string | false>(false);

  const tutorials: Tutorial[] = [
    {
      title: 'Getting Started with CRM Solution',
      description: 'Learn the basics of navigating and using the CRM system effectively.',
      duration: '15 min',
      level: 'Beginner',
      topics: ['Login', 'Dashboard', 'Navigation', 'Profile Setup'],
    },
    {
      title: 'Customer Management',
      description: 'Master customer lifecycle management from lead to loyal customer.',
      duration: '20 min',
      level: 'Beginner',
      topics: ['Adding Customers', 'Customer Categories', 'Lifecycle Stages', 'Search & Filters'],
    },
    {
      title: 'Sales Pipeline Management',
      description: 'Learn to track opportunities and close deals effectively.',
      duration: '25 min',
      level: 'Intermediate',
      topics: ['Creating Opportunities', 'Pipeline Stages', 'Forecasting', 'Win/Loss Analysis'],
    },
    {
      title: 'Contact & Communication Tracking',
      description: 'Manage contacts and log all customer interactions.',
      duration: '15 min',
      level: 'Beginner',
      topics: ['Adding Contacts', 'Linking to Customers', 'Interaction Logging', 'Contact History'],
    },
    {
      title: 'Task & Activity Management',
      description: 'Stay organized with tasks, reminders, and activity tracking.',
      duration: '20 min',
      level: 'Intermediate',
      topics: ['Creating Tasks', 'Due Dates', 'Task Assignment', 'Activity Feed'],
    },
    {
      title: 'Campaign Management',
      description: 'Plan and track marketing campaigns with ROI analysis.',
      duration: '25 min',
      level: 'Intermediate',
      topics: ['Campaign Types', 'Budget Tracking', 'Metrics', 'Conversion Tracking'],
    },
    {
      title: 'Quote & Proposal Generation',
      description: 'Create professional quotes linked to opportunities.',
      duration: '20 min',
      level: 'Intermediate',
      topics: ['Quote Creation', 'Product Selection', 'Pricing', 'Quote Status'],
    },
    {
      title: 'Security & Two-Factor Authentication',
      description: 'Secure your account with 2FA and understand security features.',
      duration: '10 min',
      level: 'Beginner',
      topics: ['Enabling 2FA', 'Backup Codes', 'Password Security', 'Session Management'],
    },
    {
      title: 'Admin Configuration',
      description: 'System administration and configuration options.',
      duration: '30 min',
      level: 'Advanced',
      topics: ['User Management', 'Group Permissions', 'Module Settings', 'Database Configuration'],
    },
    {
      title: 'Workflow Automation',
      description: 'Automate repetitive tasks with workflow rules.',
      duration: '25 min',
      level: 'Advanced',
      topics: ['Trigger Events', 'Actions', 'Conditions', 'Workflow Testing'],
    },
  ];

  const faqs: FAQ[] = [
    {
      question: 'How do I reset my password?',
      answer: 'Click on "Forgot Password?" on the login page. Enter your email address and follow the instructions sent to your inbox. If you have 2FA enabled, you may need to verify your identity.',
    },
    {
      question: 'How do I enable Two-Factor Authentication (2FA)?',
      answer: 'Go to Settings > Security tab. Click "Enable 2FA" and scan the QR code with your authenticator app (Google Authenticator, Authy, etc.). Enter the 6-digit code to confirm setup. Save your backup codes in a secure location.',
    },
    {
      question: 'How do I add a new customer?',
      answer: 'Navigate to Customers from the sidebar menu. Click the "Add Customer" button. Fill in the required fields (Name, Email) and optional details. Select the customer category (Individual/Organization) and lifecycle stage. Click Save to create the customer.',
    },
    {
      question: 'What are the different opportunity stages?',
      answer: 'The default stages are: Prospecting (initial contact), Qualification (evaluating fit), Proposal (quote sent), Negotiation (discussing terms), Closed Won (deal won), and Closed Lost (deal lost). These help track the sales pipeline progress.',
    },
    {
      question: 'How do I generate a quote?',
      answer: 'Go to Quotes section. Click "Create Quote". Link it to an opportunity and customer. Add products from the catalog with quantities. Set discounts if applicable. Save and optionally export or send to customer.',
    },
    {
      question: 'How do I manage user permissions?',
      answer: 'Admin users can go to Settings > Groups tab. Create user groups with specific permissions. Assign users to groups. Permissions include module access, CRUD operations, and admin functions.',
    },
    {
      question: 'Can I import/export data?',
      answer: 'The system supports data export in various formats. For bulk imports, contact your system administrator. Individual records can be created and updated through the user interface.',
    },
    {
      question: 'How do I track campaign performance?',
      answer: 'Go to Campaigns and select a campaign. View metrics including budget spent, leads generated, and conversions. The dashboard also shows campaign performance summaries.',
    },
    {
      question: 'What browsers are supported?',
      answer: 'CRM Solution works best on modern browsers including Chrome, Firefox, Safari, and Edge. We recommend keeping your browser updated for the best experience and security.',
    },
    {
      question: 'Is my data secure?',
      answer: 'Yes. We use industry-standard security including encrypted connections (HTTPS), password hashing (BCrypt), JWT authentication, optional 2FA, and role-based access controls. Regular backups are recommended for data protection.',
    },
  ];

  const quickLinks = [
    { icon: <DashboardIcon />, label: 'Dashboard Overview', path: '/dashboard' },
    { icon: <PeopleIcon />, label: 'Customer Management', path: '/customers' },
    { icon: <ContactIcon />, label: 'Contact Management', path: '/contacts' },
    { icon: <SalesIcon />, label: 'Opportunities', path: '/opportunities' },
    { icon: <TaskIcon />, label: 'Task Management', path: '/tasks' },
    { icon: <CampaignIcon />, label: 'Campaigns', path: '/campaigns' },
    { icon: <SettingsIcon />, label: 'Settings', path: '/settings' },
  ];

  const keyboardShortcuts = [
    { keys: ['Ctrl', '/'], action: 'Open Search' },
    { keys: ['Ctrl', 'N'], action: 'New Record (context-dependent)' },
    { keys: ['Ctrl', 'S'], action: 'Save Current Form' },
    { keys: ['Esc'], action: 'Close Modal/Cancel' },
    { keys: ['Tab'], action: 'Navigate Between Fields' },
  ];

  const filteredFaqs = faqs.filter(
    (faq) =>
      faq.question.toLowerCase().includes(searchQuery.toLowerCase()) ||
      faq.answer.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const getLevelColor = (level: string) => {
    switch (level) {
      case 'Beginner':
        return 'success';
      case 'Intermediate':
        return 'warning';
      case 'Advanced':
        return 'error';
      default:
        return 'default';
    }
  };

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Paper elevation={3} sx={{ p: 4, mb: 4 }}>
        {/* Header */}
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 4 }}>
          <HelpIcon color="primary" sx={{ fontSize: 48, mr: 2 }} />
          <Box>
            <Typography variant="h3" component="h1" gutterBottom>
              Help & Tutorials
            </Typography>
            <Typography variant="h6" color="text.secondary">
              Learn how to use CRM Solution effectively
            </Typography>
          </Box>
        </Box>

        <Divider sx={{ mb: 4 }} />

        <Grid container spacing={4}>
          {/* Quick Links */}
          <Grid item xs={12} md={4}>
            <Card variant="outlined" sx={{ height: '100%' }}>
              <CardHeader
                avatar={<SpeedIcon color="primary" />}
                title="Quick Links"
                subheader="Jump to common areas"
              />
              <CardContent>
                <List dense>
                  {quickLinks.map((link, index) => (
                    <ListItemButton key={index} component={Link} href={link.path}>
                      <ListItemIcon>{link.icon}</ListItemIcon>
                      <ListItemText primary={link.label} />
                    </ListItemButton>
                  ))}
                </List>
              </CardContent>
            </Card>
          </Grid>

          {/* Keyboard Shortcuts */}
          <Grid item xs={12} md={4}>
            <Card variant="outlined" sx={{ height: '100%' }}>
              <CardHeader
                avatar={<KeyboardIcon color="primary" />}
                title="Keyboard Shortcuts"
                subheader="Work faster with shortcuts"
              />
              <CardContent>
                <List dense>
                  {keyboardShortcuts.map((shortcut, index) => (
                    <ListItem key={index}>
                      <ListItemText
                        primary={shortcut.action}
                        secondary={
                          <Box sx={{ mt: 0.5 }}>
                            {shortcut.keys.map((key, i) => (
                              <React.Fragment key={i}>
                                <Chip
                                  label={key}
                                  size="small"
                                  variant="outlined"
                                  sx={{ mr: 0.5 }}
                                />
                                {i < shortcut.keys.length - 1 && '+'}
                              </React.Fragment>
                            ))}
                          </Box>
                        }
                      />
                    </ListItem>
                  ))}
                </List>
              </CardContent>
            </Card>
          </Grid>

          {/* Getting Started */}
          <Grid item xs={12} md={4}>
            <Card variant="outlined" sx={{ height: '100%' }}>
              <CardHeader
                avatar={<LoginIcon color="primary" />}
                title="Getting Started"
                subheader="New to CRM Solution?"
              />
              <CardContent>
                <List dense>
                  <ListItem>
                    <ListItemIcon><Typography color="primary">1.</Typography></ListItemIcon>
                    <ListItemText primary="Log in with your credentials" />
                  </ListItem>
                  <ListItem>
                    <ListItemIcon><Typography color="primary">2.</Typography></ListItemIcon>
                    <ListItemText primary="Set up your profile" />
                  </ListItem>
                  <ListItem>
                    <ListItemIcon><Typography color="primary">3.</Typography></ListItemIcon>
                    <ListItemText primary="Enable Two-Factor Authentication" />
                  </ListItem>
                  <ListItem>
                    <ListItemIcon><Typography color="primary">4.</Typography></ListItemIcon>
                    <ListItemText primary="Explore the Dashboard" />
                  </ListItem>
                  <ListItem>
                    <ListItemIcon><Typography color="primary">5.</Typography></ListItemIcon>
                    <ListItemText primary="Add your first customer" />
                  </ListItem>
                </List>
              </CardContent>
            </Card>
          </Grid>

          {/* Tutorials */}
          <Grid item xs={12}>
            <Card variant="outlined">
              <CardHeader
                avatar={<PlayCircleIcon color="primary" />}
                title="Tutorials"
                subheader="Step-by-step guides for all features"
              />
              <CardContent>
                <Grid container spacing={2}>
                  {tutorials.map((tutorial, index) => (
                    <Grid item xs={12} sm={6} md={4} key={index}>
                      <Card variant="outlined" sx={{ height: '100%' }}>
                        <CardContent>
                          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                            <Chip
                              label={tutorial.level}
                              size="small"
                              color={getLevelColor(tutorial.level) as any}
                            />
                            <Typography variant="caption" color="text.secondary">
                              {tutorial.duration}
                            </Typography>
                          </Box>
                          <Typography variant="subtitle1" gutterBottom>
                            {tutorial.title}
                          </Typography>
                          <Typography variant="body2" color="text.secondary" paragraph>
                            {tutorial.description}
                          </Typography>
                          <Box>
                            {tutorial.topics.map((topic, i) => (
                              <Chip
                                key={i}
                                label={topic}
                                size="small"
                                variant="outlined"
                                sx={{ mr: 0.5, mb: 0.5 }}
                              />
                            ))}
                          </Box>
                        </CardContent>
                      </Card>
                    </Grid>
                  ))}
                </Grid>
              </CardContent>
            </Card>
          </Grid>

          {/* FAQ */}
          <Grid item xs={12}>
            <Card variant="outlined">
              <CardHeader
                avatar={<FAQIcon color="primary" />}
                title="Frequently Asked Questions"
                subheader="Find answers to common questions"
                action={
                  <TextField
                    size="small"
                    placeholder="Search FAQs..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    InputProps={{
                      startAdornment: (
                        <InputAdornment position="start">
                          <SearchIcon />
                        </InputAdornment>
                      ),
                    }}
                    sx={{ width: 250 }}
                  />
                }
              />
              <CardContent>
                {filteredFaqs.map((faq, index) => (
                  <Accordion
                    key={index}
                    expanded={expandedFaq === `panel${index}`}
                    onChange={(_, isExpanded) =>
                      setExpandedFaq(isExpanded ? `panel${index}` : false)
                    }
                  >
                    <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                      <Typography variant="subtitle1">{faq.question}</Typography>
                    </AccordionSummary>
                    <AccordionDetails>
                      <Typography variant="body2" color="text.secondary">
                        {faq.answer}
                      </Typography>
                    </AccordionDetails>
                  </Accordion>
                ))}
                {filteredFaqs.length === 0 && (
                  <Typography variant="body2" color="text.secondary" align="center" sx={{ py: 4 }}>
                    No FAQs found matching your search.
                  </Typography>
                )}
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      </Paper>
    </Container>
  );
};

export default HelpPage;
