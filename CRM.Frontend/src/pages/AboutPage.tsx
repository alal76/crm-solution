/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This software is source-available. Non-commercial use is permitted under
 * the terms of the LICENSE file. Commercial use requires a separate license.
 * See the LICENSE file in the root directory for full terms.
 */

import React, { useState, useEffect } from 'react';
import {
  Container,
  Box,
  Typography,
  Paper,
  Grid,
  Card,
  CardContent,
  Divider,
  Link,
  Chip,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Avatar,
  Tabs,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  LinearProgress,
} from '@mui/material';
import {
  Info as InfoIcon,
  Code as CodeIcon,
  Gavel as GavelIcon,
  Person as PersonIcon,
  GitHub as GitHubIcon,
  Email as EmailIcon,
  CalendarMonth as CalendarIcon,
  Storage as StorageIcon,
  Web as WebIcon,
  Security as SecurityIcon,
  CheckCircle as CheckCircleIcon,
  Science as ScienceIcon,
  Speed as SpeedIcon,
  BugReport as BugReportIcon,
  AccountTree as AccountTreeIcon,
} from '@mui/icons-material';
import { useBranding } from '../contexts/BrandingContext';
import { TabPanel } from '../components/common';
import { ArchitectureDiagram } from '../components/architecture';
import { getApiBaseUrl } from '../config/ports';

const AboutPage: React.FC = () => {
  const { branding } = useBranding();
  const companyName = branding.solutionName || branding.companyName;
  
  // Get logo URL with proper API base
  const getLogoUrl = () => {
    const logoPath = branding.brandingLogoUrl || branding.companyLogoUrl;
    if (logoPath) {
      // If it's a data URL (base64), use it directly
      if (logoPath.startsWith('data:')) {
        return logoPath;
      }
      if (logoPath.startsWith('/uploads')) {
        return `${getApiBaseUrl()}${logoPath}`;
      }
      return logoPath;
    }
    return null;
  };
  
  const logoUrl = getLogoUrl();
  const [tabValue, setTabValue] = useState(0);

  const [version, setVersion] = useState('0.614.58');
  const [apiVersion, setApiVersion] = useState('');
  const [frontendVersion, setFrontendVersion] = useState('');
  const releaseDate = 'March 2026';
  const author = 'Abhishek Lal';

  // Fetch version info from version.json
  useEffect(() => {
    const fetchVersion = async () => {
      try {
        const response = await fetch('/version.json');
        if (response.ok) {
          const data = await response.json();
          const solutionVer = `${data.major}.${data.minor}.${data.patch}`;
          setVersion(solutionVer);
          if (data.components?.api?.version) {
            setApiVersion(data.components.api.version);
          }
          if (data.components?.frontend?.version) {
            setFrontendVersion(data.components.frontend.version);
          }
        }
      } catch {
        // keep default version
      }
    };
    fetchVersion();
  }, []);

  const features = [
    { name: 'Customer Management', description: 'Complete customer lifecycle management' },
    { name: 'Sales Pipeline', description: 'Track opportunities and deals' },
    { name: 'Contact Management', description: 'Manage contacts and relationships' },
    { name: 'Relationship Management', description: 'B2B and B2C relationship tracking with hierarchies and health metrics' },
    { name: 'Task Management', description: 'Track tasks and activities' },
    { name: 'Campaign Management', description: 'Marketing campaign tracking' },
    { name: 'Campaign Execution', description: 'Multi-channel execution with batch processing and A/B testing' },
    { name: 'Quote Management', description: 'Generate and manage quotes' },
    { name: 'Workflow Automation', description: 'Automate business processes' },
    { name: 'Two-Factor Authentication', description: 'Enhanced security with 2FA' },
    { name: 'Role-Based Access Control', description: 'Fine-grained permissions' },
    { name: 'Social Login Integration', description: 'OAuth providers support' },
  ];

  const techStack = [
    { name: 'Frontend', items: ['React 18', 'TypeScript', 'Material-UI', 'Recharts'] },
    { name: 'Backend', items: ['.NET 8', 'Entity Framework Core', 'RESTful API', 'JWT Auth'] },
    { name: 'Database', items: ['MariaDB 11+', 'SQLite (dev)', 'Multi-provider support'] },
    { name: 'Infrastructure', items: ['Docker', 'Docker Compose', 'Nginx'] },
  ];

  // Test results data - updated at build time
  const testResults = {
    lastRun: 'January 2026',
    totalTests: 748,
    passed: 747,
    failed: 1,
    skipped: 0,
    duration: '45s',
    testSuites: [
      {
        name: 'Build Verification Tests (BVT)',
        description: 'Critical path tests for core entities and functionality',
        tests: 48,
        passed: 48,
        examples: [
          'BVT-001: Customer Entity Creation',
          'BVT-002: CustomerContact Relationship',
          'BVT-003: Opportunity Entity',
          'BVT-013: Workflow Definition Entity',
          'BVT-019: Activity Tracking',
          'BVT-045: Relationship Management',
          'BVT-048: Campaign Execution'
        ]
      },
      {
        name: 'Controller Tests',
        description: 'API endpoint validation and request/response testing',
        tests: 145,
        passed: 145,
        examples: [
          'CustomersController CRUD Operations',
          'OpportunitiesController Status Changes',
          'RelationshipsController CRUD Operations',
          'CampaignExecutionsController Operations',
          'HealthController Endpoint Validation'
        ]
      },
      {
        name: 'Service Tests',
        description: 'Business logic and service layer testing',
        tests: 285,
        passed: 284,
        examples: [
          'ContactsService CRUD Operations',
          'SystemSettingsService Module Toggles',
          'RelationshipService B2B/B2C Tracking',
          'CampaignExecutionService Batch Processing',
          'CustomerService Search Functionality',
          'NormalizationService Data Processing'
        ]
      },
      {
        name: 'Integration Tests',
        description: 'Database and external service integration tests',
        tests: 120,
        passed: 120,
        examples: [
          'Entity Framework InMemory Tests',
          'Authentication Flow Tests',
          'Settings Persistence Tests'
        ]
      }
    ]
  };

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Paper elevation={3} sx={{ p: 4, mb: 4 }}>
        {/* Header */}
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 4 }}>
          {logoUrl && (
            <Avatar
              src={logoUrl}
              alt={companyName}
              sx={{ width: 80, height: 80, mr: 3 }}
              variant="rounded"
            />
          )}
          <Box>
            <Typography variant="h3" component="h1" gutterBottom>
              {companyName || 'CRM Solution'}
            </Typography>
            <Typography variant="h6" color="text.secondary">
              Customer Relationship Management System
            </Typography>
          </Box>
        </Box>

        {/* Tabs Navigation */}
        <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
          <Tabs value={tabValue} onChange={handleTabChange} aria-label="About page tabs" variant="scrollable" scrollButtons="auto">
            <Tab icon={<InfoIcon />} label="Overview" iconPosition="start" />
            <Tab icon={<WebIcon />} label="Features" iconPosition="start" />
            <Tab icon={<StorageIcon />} label="Tech Stack" iconPosition="start" />
            <Tab icon={<ScienceIcon />} label="Test Results" iconPosition="start" />
            <Tab icon={<AccountTreeIcon />} label="Architecture" iconPosition="start" />
          </Tabs>
        </Box>

        {/* Tab 0: Overview */}
        <TabPanel value={tabValue} index={0}>
          <Grid container spacing={4}>
            {/* Version Info */}
            <Grid item xs={12} md={6}>
              <Card variant="outlined">
                <CardContent>
                  <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                    <InfoIcon color="primary" sx={{ mr: 1 }} />
                    <Typography variant="h6">Version Information</Typography>
                  </Box>
                  <List dense>
                    <ListItem>
                      <ListItemIcon><CodeIcon /></ListItemIcon>
                      <ListItemText primary="Solution Version" secondary={`v${version}`} />
                    </ListItem>
                    {apiVersion && (
                      <ListItem>
                        <ListItemIcon><StorageIcon /></ListItemIcon>
                        <ListItemText primary="API Version" secondary={`v${apiVersion}`} />
                      </ListItem>
                    )}
                    {frontendVersion && (
                      <ListItem>
                        <ListItemIcon><WebIcon /></ListItemIcon>
                        <ListItemText primary="Frontend Version" secondary={`v${frontendVersion}`} />
                      </ListItem>
                    )}
                    <ListItem>
                      <ListItemIcon><CalendarIcon /></ListItemIcon>
                      <ListItemText primary="Release Date" secondary={releaseDate} />
                    </ListItem>
                    <ListItem>
                      <ListItemIcon><PersonIcon /></ListItemIcon>
                      <ListItemText primary="Author" secondary={author} />
                    </ListItem>
                  </List>
                </CardContent>
              </Card>
            </Grid>

            {/* License Info */}
            <Grid item xs={12} md={6}>
              <Card variant="outlined">
                <CardContent>
                  <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                    <GavelIcon color="primary" sx={{ mr: 1 }} />
                    <Typography variant="h6">License</Typography>
                  </Box>
                  <Typography variant="body1" paragraph>
                    <strong>Source-Available License — Commercial Use Requires a License</strong>
                  </Typography>
                  <Typography variant="body2" color="text.secondary" paragraph>
                    Copyright © 2024-2026 Abhishek Lal
                  </Typography>
                  <Typography variant="body2" color="text.secondary" paragraph>
                    This software is source-available. You are free to view, study, modify, and use
                    the code for non-commercial purposes. Commercial use requires a separate license.
                  </Typography>
                  <Chip label="Source Available" color="success" size="small" sx={{ mr: 1 }} />
                  <Chip label="Non-Commercial Free" color="info" size="small" sx={{ mr: 1 }} />
                  <Chip label="Commercial License Required" color="warning" size="small" />
                </CardContent>
              </Card>
            </Grid>

            {/* Disclaimer */}
            <Grid item xs={12}>
              <Card variant="outlined" sx={{ bgcolor: 'warning.light', opacity: 0.8 }}>
                <CardContent>
                  <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                    <SecurityIcon sx={{ mr: 1, color: 'warning.dark' }} />
                    <Typography variant="h6" color="warning.dark">Disclaimer</Typography>
                  </Box>
                  <Typography variant="body2" sx={{ color: 'warning.dark' }}>
                    <strong>NO WARRANTY:</strong> This software is provided "AS IS" without warranty of any kind. 
                    The entire risk as to the quality and performance of the software is with you.
                  </Typography>
                  <Typography variant="body2" sx={{ color: 'warning.dark', mt: 1 }}>
                    <strong>NO LIABILITY:</strong> In no event shall Abhishek Lal or any contributors be liable 
                    for any damages arising out of the use or inability to use this software.
                  </Typography>
                </CardContent>
              </Card>
            </Grid>

            {/* Source Code */}
            <Grid item xs={12}>
              <Card variant="outlined">
                <CardContent>
                  <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                    <GitHubIcon color="primary" sx={{ mr: 1 }} />
                    <Typography variant="h6">Source Code</Typography>
                  </Box>
                  <Typography variant="body2" paragraph>
                    This software is source-available under a license that permits free non-commercial use.
                    Commercial use requires a separate license. See the LICENSE file for details.
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    For source code access, licensing questions, or contributions, please contact the author.
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </TabPanel>

        {/* Tab 1: Features */}
        <TabPanel value={tabValue} index={1}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <WebIcon color="primary" sx={{ mr: 1 }} />
                <Typography variant="h6">Features</Typography>
              </Box>
              <Grid container spacing={2}>
                {features.map((feature, index) => (
                  <Grid item xs={12} sm={6} md={4} key={index}>
                    <Box sx={{ p: 1 }}>
                      <Typography variant="subtitle2" color="primary">
                        {feature.name}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        {feature.description}
                      </Typography>
                    </Box>
                  </Grid>
                ))}
              </Grid>
            </CardContent>
          </Card>
        </TabPanel>

        {/* Tab 2: Tech Stack */}
        <TabPanel value={tabValue} index={2}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <StorageIcon color="primary" sx={{ mr: 1 }} />
                <Typography variant="h6">Technology Stack</Typography>
              </Box>
              <Grid container spacing={3}>
                {techStack.map((stack, index) => (
                  <Grid item xs={12} sm={6} md={3} key={index}>
                    <Typography variant="subtitle2" color="primary" gutterBottom>
                      {stack.name}
                    </Typography>
                    {stack.items.map((item, i) => (
                      <Chip
                        key={i}
                        label={item}
                        size="small"
                        variant="outlined"
                        sx={{ mr: 0.5, mb: 0.5 }}
                      />
                    ))}
                  </Grid>
                ))}
              </Grid>
            </CardContent>
          </Card>
        </TabPanel>

        {/* Tab 3: Test Results */}
        <TabPanel value={tabValue} index={3}>
          <Grid container spacing={3}>
            {/* Test Summary Cards */}
            <Grid item xs={12}>
              <Grid container spacing={2}>
                <Grid item xs={6} sm={3}>
                  <Card variant="outlined" sx={{ textAlign: 'center', p: 2 }}>
                    <ScienceIcon color="primary" sx={{ fontSize: 40, mb: 1 }} />
                    <Typography variant="h4" color="primary">{testResults.totalTests}</Typography>
                    <Typography variant="body2" color="text.secondary">Total Tests</Typography>
                  </Card>
                </Grid>
                <Grid item xs={6} sm={3}>
                  <Card variant="outlined" sx={{ textAlign: 'center', p: 2, bgcolor: 'success.light' }}>
                    <CheckCircleIcon sx={{ fontSize: 40, mb: 1, color: 'success.dark' }} />
                    <Typography variant="h4" sx={{ color: 'success.dark' }}>{testResults.passed}</Typography>
                    <Typography variant="body2" sx={{ color: 'success.dark' }}>Passed</Typography>
                  </Card>
                </Grid>
                <Grid item xs={6} sm={3}>
                  <Card variant="outlined" sx={{ textAlign: 'center', p: 2 }}>
                    <BugReportIcon color={testResults.failed > 0 ? 'error' : 'disabled'} sx={{ fontSize: 40, mb: 1 }} />
                    <Typography variant="h4" color={testResults.failed > 0 ? 'error' : 'text.secondary'}>
                      {testResults.failed}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">Failed</Typography>
                  </Card>
                </Grid>
                <Grid item xs={6} sm={3}>
                  <Card variant="outlined" sx={{ textAlign: 'center', p: 2 }}>
                    <SpeedIcon color="primary" sx={{ fontSize: 40, mb: 1 }} />
                    <Typography variant="h4" color="primary">{testResults.duration}</Typography>
                    <Typography variant="body2" color="text.secondary">Duration</Typography>
                  </Card>
                </Grid>
              </Grid>
            </Grid>

            {/* Pass Rate Progress */}
            <Grid item xs={12}>
              <Card variant="outlined">
                <CardContent>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
                    <Typography variant="subtitle1">Pass Rate</Typography>
                    <Chip 
                      label={`${((testResults.passed / testResults.totalTests) * 100).toFixed(1)}%`}
                      color="success"
                      size="small"
                    />
                  </Box>
                  <LinearProgress 
                    variant="determinate" 
                    value={(testResults.passed / testResults.totalTests) * 100}
                    color="success"
                    sx={{ height: 10, borderRadius: 5 }}
                  />
                  <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
                    Last run: {testResults.lastRun}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>

            {/* Test Suites Table */}
            <Grid item xs={12}>
              <Card variant="outlined">
                <CardContent>
                  <Typography variant="h6" gutterBottom>Test Suites</Typography>
                  <TableContainer>
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>Suite</TableCell>
                          <TableCell>Description</TableCell>
                          <TableCell align="right">Tests</TableCell>
                          <TableCell align="right">Passed</TableCell>
                          <TableCell align="right">Status</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {testResults.testSuites.map((suite, index) => (
                          <TableRow key={index}>
                            <TableCell>
                              <Typography variant="subtitle2">{suite.name}</Typography>
                            </TableCell>
                            <TableCell>
                              <Typography variant="body2" color="text.secondary">{suite.description}</Typography>
                            </TableCell>
                            <TableCell align="right">{suite.tests}</TableCell>
                            <TableCell align="right">{suite.passed}</TableCell>
                            <TableCell align="right">
                              <Chip 
                                label={suite.passed === suite.tests ? 'PASS' : 'FAIL'}
                                color={suite.passed === suite.tests ? 'success' : 'error'}
                                size="small"
                              />
                            </TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                </CardContent>
              </Card>
            </Grid>

            {/* Sample Test Names */}
            <Grid item xs={12}>
              <Card variant="outlined">
                <CardContent>
                  <Typography variant="h6" gutterBottom>Sample Tests</Typography>
                  <Grid container spacing={2}>
                    {testResults.testSuites.map((suite, index) => (
                      <Grid item xs={12} md={6} key={index}>
                        <Typography variant="subtitle2" color="primary" gutterBottom>
                          {suite.name}
                        </Typography>
                        <List dense>
                          {suite.examples.map((example, i) => (
                            <ListItem key={i} sx={{ py: 0 }}>
                              <ListItemIcon sx={{ minWidth: 30 }}>
                                <CheckCircleIcon color="success" fontSize="small" />
                              </ListItemIcon>
                              <ListItemText 
                                primary={example}
                                primaryTypographyProps={{ variant: 'body2' }}
                              />
                            </ListItem>
                          ))}
                        </List>
                      </Grid>
                    ))}
                  </Grid>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </TabPanel>

        {/* Tab 4: Architecture */}
        <TabPanel value={tabValue} index={4}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <AccountTreeIcon color="primary" sx={{ mr: 1 }} />
                <Typography variant="h6">Solution Architecture</Typography>
              </Box>
              <Typography variant="body2" color="text.secondary" paragraph>
                Visual representation of the CRM solution architecture including all modules, entities, 
                database tables, primary keys, foreign keys, and entity relationships.
              </Typography>
              <ArchitectureDiagram />
            </CardContent>
          </Card>
        </TabPanel>
      </Paper>
    </Container>
  );
};

export default AboutPage;
