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
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Accordion,
  AccordionSummary,
  AccordionDetails,
} from '@mui/material';
import {
  Gavel as GavelIcon,
  ExpandMore as ExpandMoreIcon,
  Web as WebIcon,
  Storage as StorageIcon,
  Code as CodeIcon,
  Build as BuildIcon,
} from '@mui/icons-material';

interface Dependency {
  name: string;
  version: string;
  license: string;
  licenseUrl?: string;
  description: string;
}

interface DependencyCategory {
  category: string;
  icon: React.ReactNode;
  dependencies: Dependency[];
}

const LicensesPage: React.FC = () => {
  const frontendDependencies: Dependency[] = [
    {
      name: 'React',
      version: '18.2.0',
      license: 'MIT',
      licenseUrl: 'https://github.com/facebook/react/blob/main/LICENSE',
      description: 'A JavaScript library for building user interfaces',
    },
    {
      name: 'React DOM',
      version: '18.2.0',
      license: 'MIT',
      licenseUrl: 'https://github.com/facebook/react/blob/main/LICENSE',
      description: 'React package for working with the DOM',
    },
    {
      name: 'React Router DOM',
      version: '6.22.0',
      license: 'MIT',
      licenseUrl: 'https://github.com/remix-run/react-router/blob/main/LICENSE.md',
      description: 'Declarative routing for React applications',
    },
    {
      name: 'Material-UI (@mui/material)',
      version: '5.15.6',
      license: 'MIT',
      licenseUrl: 'https://github.com/mui/material-ui/blob/master/LICENSE',
      description: 'React components for faster and easier web development',
    },
    {
      name: '@mui/icons-material',
      version: '5.15.6',
      license: 'MIT',
      licenseUrl: 'https://github.com/mui/material-ui/blob/master/LICENSE',
      description: 'Material Design icons for Material-UI',
    },
    {
      name: '@emotion/react',
      version: '11.11.3',
      license: 'MIT',
      licenseUrl: 'https://github.com/emotion-js/emotion/blob/main/LICENSE',
      description: 'CSS-in-JS library for styling React components',
    },
    {
      name: '@emotion/styled',
      version: '11.11.0',
      license: 'MIT',
      licenseUrl: 'https://github.com/emotion-js/emotion/blob/main/LICENSE',
      description: 'Styled components for Emotion',
    },
    {
      name: 'Axios',
      version: '1.6.7',
      license: 'MIT',
      licenseUrl: 'https://github.com/axios/axios/blob/v1.x/LICENSE',
      description: 'Promise-based HTTP client for the browser and Node.js',
    },
    {
      name: 'Formik',
      version: '2.4.5',
      license: 'Apache-2.0',
      licenseUrl: 'https://github.com/jaredpalmer/formik/blob/master/LICENSE',
      description: 'Build forms in React without the tears',
    },
    {
      name: 'Yup',
      version: '1.3.3',
      license: 'MIT',
      licenseUrl: 'https://github.com/jquense/yup/blob/master/LICENSE.md',
      description: 'Schema validation library',
    },
    {
      name: 'Recharts',
      version: '2.12.0',
      license: 'MIT',
      licenseUrl: 'https://github.com/recharts/recharts/blob/master/LICENSE',
      description: 'Composable charting library built on React components',
    },
    {
      name: 'React Icons',
      version: '5.0.1',
      license: 'MIT',
      licenseUrl: 'https://github.com/react-icons/react-icons/blob/master/LICENSE',
      description: 'Popular icons for React projects',
    },
    {
      name: 'QRCode.react',
      version: '3.1.0',
      license: 'ISC',
      licenseUrl: 'https://github.com/zpao/qrcode.react/blob/master/LICENSE',
      description: 'QR Code component for React',
    },
    {
      name: 'TypeScript',
      version: '4.9.5',
      license: 'Apache-2.0',
      licenseUrl: 'https://github.com/microsoft/TypeScript/blob/main/LICENSE.txt',
      description: 'TypeScript language for JavaScript with types',
    },
  ];

  const backendDependencies: Dependency[] = [
    {
      name: '.NET 8.0',
      version: '8.0',
      license: 'MIT',
      licenseUrl: 'https://github.com/dotnet/runtime/blob/main/LICENSE.TXT',
      description: 'Cross-platform, open-source developer platform',
    },
    {
      name: 'Entity Framework Core',
      version: '8.0.0',
      license: 'MIT',
      licenseUrl: 'https://github.com/dotnet/efcore/blob/main/LICENSE.txt',
      description: 'Modern object-database mapper for .NET',
    },
    {
      name: 'Pomelo.EntityFrameworkCore.MySql',
      version: '8.0.0',
      license: 'MIT',
      licenseUrl: 'https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/blob/main/LICENSE',
      description: 'MySQL/MariaDB provider for Entity Framework Core',
    },
    {
      name: 'Microsoft.EntityFrameworkCore.Sqlite',
      version: '8.0.0',
      license: 'MIT',
      licenseUrl: 'https://github.com/dotnet/efcore/blob/main/LICENSE.txt',
      description: 'SQLite database provider for Entity Framework Core',
    },
    {
      name: 'Serilog',
      version: '3.1.1',
      license: 'Apache-2.0',
      licenseUrl: 'https://github.com/serilog/serilog/blob/dev/LICENSE',
      description: 'Flexible, structured logging for .NET',
    },
    {
      name: 'Swashbuckle.AspNetCore',
      version: '6.5.0',
      license: 'MIT',
      licenseUrl: 'https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/LICENSE',
      description: 'Swagger tooling for ASP.NET Core APIs',
    },
    {
      name: 'BCrypt.Net-Next',
      version: '4.0.3',
      license: 'BSD-3-Clause',
      licenseUrl: 'https://github.com/BcryptNet/bcrypt.net/blob/main/LICENSE',
      description: 'BCrypt password hashing for .NET',
    },
    {
      name: 'Microsoft.AspNetCore.Authentication.JwtBearer',
      version: '8.0.0',
      license: 'MIT',
      licenseUrl: 'https://github.com/dotnet/aspnetcore/blob/main/LICENSE.txt',
      description: 'JWT Bearer authentication for ASP.NET Core',
    },
  ];

  const testingDependencies: Dependency[] = [
    {
      name: 'xUnit',
      version: '2.6.2',
      license: 'Apache-2.0',
      licenseUrl: 'https://github.com/xunit/xunit/blob/main/LICENSE',
      description: 'Free, open source, community-focused unit testing tool',
    },
    {
      name: 'Moq',
      version: '4.20.70',
      license: 'BSD-3-Clause',
      licenseUrl: 'https://github.com/moq/moq4/blob/main/License.txt',
      description: 'The most popular mocking library for .NET',
    },
    {
      name: 'FluentAssertions',
      version: '6.12.0',
      license: 'Apache-2.0',
      licenseUrl: 'https://github.com/fluentassertions/fluentassertions/blob/master/LICENSE',
      description: 'Fluent API for assertions in unit tests',
    },
    {
      name: '@testing-library/react',
      version: '14.1.2',
      license: 'MIT',
      licenseUrl: 'https://github.com/testing-library/react-testing-library/blob/main/LICENSE',
      description: 'Simple and complete React DOM testing utilities',
    },
    {
      name: '@testing-library/jest-dom',
      version: '6.2.0',
      license: 'MIT',
      licenseUrl: 'https://github.com/testing-library/jest-dom/blob/main/LICENSE',
      description: 'Custom Jest matchers for DOM testing',
    },
    {
      name: 'Jest',
      version: '29.7.0',
      license: 'MIT',
      licenseUrl: 'https://github.com/facebook/jest/blob/main/LICENSE',
      description: 'JavaScript testing framework',
    },
  ];

  const infrastructureDependencies: Dependency[] = [
    {
      name: 'Docker',
      version: 'Latest',
      license: 'Apache-2.0',
      licenseUrl: 'https://github.com/moby/moby/blob/master/LICENSE',
      description: 'Container platform for building and running applications',
    },
    {
      name: 'MariaDB',
      version: '11.0+',
      license: 'GPL-2.0',
      licenseUrl: 'https://mariadb.com/kb/en/licensing-faq/',
      description: 'Community-developed fork of MySQL database',
    },
    {
      name: 'Nginx',
      version: 'Latest',
      license: 'BSD-2-Clause',
      licenseUrl: 'https://nginx.org/LICENSE',
      description: 'High-performance HTTP server and reverse proxy',
    },
    {
      name: 'Node.js',
      version: '20.x',
      license: 'MIT',
      licenseUrl: 'https://github.com/nodejs/node/blob/main/LICENSE',
      description: 'JavaScript runtime built on Chrome V8 engine',
    },
  ];

  const categories: DependencyCategory[] = [
    { category: 'Frontend Dependencies', icon: <WebIcon />, dependencies: frontendDependencies },
    { category: 'Backend Dependencies', icon: <StorageIcon />, dependencies: backendDependencies },
    { category: 'Testing Dependencies', icon: <CodeIcon />, dependencies: testingDependencies },
    { category: 'Infrastructure', icon: <BuildIcon />, dependencies: infrastructureDependencies },
  ];

  const getLicenseColor = (license: string) => {
    switch (license) {
      case 'MIT':
        return 'success';
      case 'Apache-2.0':
        return 'info';
      case 'BSD-2-Clause':
      case 'BSD-3-Clause':
        return 'warning';
      case 'ISC':
        return 'secondary';
      case 'GPL-2.0':
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
          <GavelIcon color="primary" sx={{ fontSize: 48, mr: 2 }} />
          <Box>
            <Typography variant="h3" component="h1" gutterBottom>
              Third-Party Licenses
            </Typography>
            <Typography variant="h6" color="text.secondary">
              Open source dependencies used in CRM Solution
            </Typography>
          </Box>
        </Box>

        <Divider sx={{ mb: 4 }} />

        {/* Project License */}
        <Card variant="outlined" sx={{ mb: 4, bgcolor: 'primary.light', opacity: 0.9 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom sx={{ color: 'primary.contrastText' }}>
              CRM Solution License
            </Typography>
            <Typography variant="body1" paragraph sx={{ color: 'primary.contrastText' }}>
              <strong>GNU Affero General Public License v3.0 (AGPL-3.0)</strong>
            </Typography>
            <Typography variant="body2" sx={{ color: 'primary.contrastText' }}>
              Copyright © 2024-2026 Abhishek Lal. This software is free and open source, 
              licensed under the AGPL-3.0. The AGPL-3.0 is a copyleft license that requires 
              anyone who distributes this code or a derivative work to make the source 
              available under the same terms.
            </Typography>
            <Box sx={{ mt: 2 }}>
              <Chip label="AGPL-3.0" color="secondary" sx={{ mr: 1 }} />
              <Chip label="Copyleft" sx={{ mr: 1, bgcolor: 'white' }} />
              <Chip label="Free Software" sx={{ bgcolor: 'white' }} />
            </Box>
          </CardContent>
        </Card>

        {/* License Summary */}
        <Card variant="outlined" sx={{ mb: 4 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              License Summary
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              All third-party dependencies used in CRM Solution are open source software 
              with permissive or copyleft licenses. No commercial or proprietary software 
              is included in this project.
            </Typography>
            <Grid container spacing={2}>
              <Grid item xs={6} sm={3}>
                <Box sx={{ textAlign: 'center', p: 2 }}>
                  <Typography variant="h4" color="success.main">
                    {categories.reduce((acc, cat) => 
                      acc + cat.dependencies.filter(d => d.license === 'MIT').length, 0
                    )}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">MIT License</Typography>
                </Box>
              </Grid>
              <Grid item xs={6} sm={3}>
                <Box sx={{ textAlign: 'center', p: 2 }}>
                  <Typography variant="h4" color="info.main">
                    {categories.reduce((acc, cat) => 
                      acc + cat.dependencies.filter(d => d.license === 'Apache-2.0').length, 0
                    )}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">Apache 2.0</Typography>
                </Box>
              </Grid>
              <Grid item xs={6} sm={3}>
                <Box sx={{ textAlign: 'center', p: 2 }}>
                  <Typography variant="h4" color="warning.main">
                    {categories.reduce((acc, cat) => 
                      acc + cat.dependencies.filter(d => 
                        d.license.includes('BSD') || d.license === 'ISC'
                      ).length, 0
                    )}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">BSD/ISC</Typography>
                </Box>
              </Grid>
              <Grid item xs={6} sm={3}>
                <Box sx={{ textAlign: 'center', p: 2 }}>
                  <Typography variant="h4" color="error.main">
                    {categories.reduce((acc, cat) => 
                      acc + cat.dependencies.filter(d => d.license.includes('GPL')).length, 0
                    )}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">GPL</Typography>
                </Box>
              </Grid>
            </Grid>
          </CardContent>
        </Card>

        {/* Dependency Tables */}
        {categories.map((category, index) => (
          <Accordion key={index} defaultExpanded={index === 0}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                {category.icon}
                <Typography variant="h6" sx={{ ml: 1 }}>
                  {category.category}
                </Typography>
                <Chip
                  label={category.dependencies.length}
                  size="small"
                  sx={{ ml: 2 }}
                />
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell><strong>Package</strong></TableCell>
                      <TableCell><strong>Version</strong></TableCell>
                      <TableCell><strong>License</strong></TableCell>
                      <TableCell><strong>Description</strong></TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {category.dependencies.map((dep, idx) => (
                      <TableRow key={idx}>
                        <TableCell>
                          <Typography variant="body2" fontWeight="medium">
                            {dep.name}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2" color="text.secondary">
                            {dep.version}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          {dep.licenseUrl ? (
                            <Link href={dep.licenseUrl} target="_blank" rel="noopener">
                              <Chip
                                label={dep.license}
                                size="small"
                                color={getLicenseColor(dep.license) as any}
                              />
                            </Link>
                          ) : (
                            <Chip
                              label={dep.license}
                              size="small"
                              color={getLicenseColor(dep.license) as any}
                            />
                          )}
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2" color="text.secondary">
                            {dep.description}
                          </Typography>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </AccordionDetails>
          </Accordion>
        ))}

        {/* Notice */}
        <Card variant="outlined" sx={{ mt: 4, bgcolor: 'grey.100' }}>
          <CardContent>
            <Typography variant="subtitle1" gutterBottom>
              Notice
            </Typography>
            <Typography variant="body2" color="text.secondary">
              This page lists the third-party open source software used in CRM Solution. 
              Each package is used in accordance with its respective license terms. 
              The presence of a package on this list does not imply any endorsement by 
              the package authors or maintainers. For the complete license text of each 
              package, please refer to the linked license files.
            </Typography>
          </CardContent>
        </Card>
      </Paper>
    </Container>
  );
};

export default LicensesPage;
