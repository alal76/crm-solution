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

import React from 'react';
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
} from '@mui/icons-material';
import { useBranding } from '../contexts/BrandingContext';

const AboutPage: React.FC = () => {
  const { branding } = useBranding();
  const companyName = branding.companyName;
  const logoUrl = branding.companyLogoUrl;

  const version = '1.3.0';
  const releaseDate = 'January 2026';
  const author = 'Abhishek Lal';

  const features = [
    { name: 'Customer Management', description: 'Complete customer lifecycle management' },
    { name: 'Sales Pipeline', description: 'Track opportunities and deals' },
    { name: 'Contact Management', description: 'Manage contacts and relationships' },
    { name: 'Task Management', description: 'Track tasks and activities' },
    { name: 'Campaign Management', description: 'Marketing campaign tracking' },
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

        <Divider sx={{ mb: 4 }} />

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
                    <ListItemText primary="Version" secondary={version} />
                  </ListItem>
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
                  <strong>GNU Affero General Public License v3.0 (AGPL-3.0)</strong>
                </Typography>
                <Typography variant="body2" color="text.secondary" paragraph>
                  Copyright © 2024-2026 Abhishek Lal
                </Typography>
                <Typography variant="body2" color="text.secondary" paragraph>
                  This is free software released under a copyleft license. You are free to use, 
                  modify, and distribute this software under the terms of the AGPL-3.0.
                </Typography>
                <Chip
                  label="Open Source"
                  color="success"
                  size="small"
                  sx={{ mr: 1 }}
                />
                <Chip
                  label="Copyleft"
                  color="info"
                  size="small"
                  sx={{ mr: 1 }}
                />
                <Chip
                  label="Free Use"
                  color="secondary"
                  size="small"
                />
              </CardContent>
            </Card>
          </Grid>

          {/* Features */}
          <Grid item xs={12}>
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
          </Grid>

          {/* Tech Stack */}
          <Grid item xs={12}>
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
          </Grid>

          {/* Disclaimer */}
          <Grid item xs={12}>
            <Card variant="outlined" sx={{ bgcolor: 'warning.light', opacity: 0.8 }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <SecurityIcon sx={{ mr: 1, color: 'warning.dark' }} />
                  <Typography variant="h6" color="warning.dark">
                    Disclaimer
                  </Typography>
                </Box>
                <Typography variant="body2" sx={{ color: 'warning.dark' }}>
                  <strong>NO WARRANTY:</strong> This software is provided "AS IS" without warranty of any kind. 
                  The entire risk as to the quality and performance of the software is with you.
                </Typography>
                <Typography variant="body2" sx={{ color: 'warning.dark', mt: 1 }}>
                  <strong>NO LIABILITY:</strong> In no event shall Abhishek Lal or any contributors be liable 
                  for any damages arising out of the use or inability to use this software.
                </Typography>
                <Typography variant="body2" sx={{ color: 'warning.dark', mt: 1 }}>
                  <strong>FREE USE:</strong> This software is provided free of charge for any use, 
                  including commercial use, subject to the AGPL-3.0 license terms.
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
                  This software is open source under the AGPL-3.0 license. 
                  As required by the license, the source code is available for download.
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  For source code access, licensing questions, or contributions, please contact the author.
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      </Paper>
    </Container>
  );
};

export default AboutPage;
