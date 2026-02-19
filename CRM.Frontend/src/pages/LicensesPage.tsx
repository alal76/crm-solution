/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This software is source-available. Non-commercial use is permitted under
 * the terms of the LICENSE file. Commercial use requires a separate license.
 * See the LICENSE file in the root directory for full terms.
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
  // Pluggable Providers Inventory
  const pluggableProviders = [
    {
      name: 'Meilisearch',
      deployed: true,
      license: 'MIT',
      licenseUrl: 'https://github.com/meilisearch/MeiliSearch/blob/main/LICENSE',
      description: 'Open-source search engine. Deployed as external provider.'
    },
    {
      name: 'Ollama',
      deployed: true,
      license: 'MIT',
      licenseUrl: 'https://github.com/ollama/ollama/blob/main/LICENSE',
      description: 'Local LLM inference. Deployed as external AI provider.'
    },
    {
      name: 'Chatwoot',
      deployed: true,
      license: 'MIT',
      licenseUrl: 'https://github.com/chatwoot/chatwoot/blob/develop/LICENSE.md',
      description: 'Customer chat support. Deployed as external chat provider.'
    },
    {
      name: 'Novu',
      deployed: true,
      license: 'MIT',
      licenseUrl: 'https://github.com/novuhq/novu/blob/main/LICENSE',
      description: 'Multi-channel notifications. Deployed as external notification provider.'
    },
    {
      name: 'Superset',
      deployed: true,
      license: 'Apache-2.0',
      licenseUrl: 'https://github.com/apache/superset/blob/master/LICENSE',
      description: 'BI & data visualization. Deployed as external analytics provider.'
    },
    {
      name: 'DocuSeal',
      deployed: true,
      license: 'AGPL-3.0',
      licenseUrl: 'https://github.com/docuseal/docuseal/blob/main/LICENSE',
      description: 'E-signature workflows. Deployed as external signature provider.'
    },
    {
      name: 'n8n',
      deployed: true,
      license: 'Fair Code',
      licenseUrl: 'https://github.com/n8n-io/n8n/blob/main/LICENSE.md',
      description: 'Workflow automation. Deployed as external integration provider.'
    },
    // Not deployed
    {
      name: 'Algolia',
      deployed: false,
      license: 'Commercial',
      licenseUrl: 'https://www.algolia.com/policies/terms/',
      description: 'SaaS search provider. Not deployed.'
    },
    {
      name: 'Typesense',
      deployed: false,
      license: 'GPL-3.0',
      licenseUrl: 'https://github.com/typesense/typesense/blob/main/LICENSE',
      description: 'Open-source search provider. Not deployed.'
    },
    {
      name: 'Twilio',
      deployed: false,
      license: 'Commercial',
      licenseUrl: 'https://www.twilio.com/legal/tos',
      description: 'SMS/voice notifications. Not deployed.'
    },
    {
      name: 'SendGrid',
      deployed: false,
      license: 'Commercial',
      licenseUrl: 'https://www.twilio.com/legal/sendgrid-terms',
      description: 'Email notifications. Not deployed.'
    },
    {
      name: 'PowerBI',
      deployed: false,
      license: 'Commercial',
      licenseUrl: 'https://www.microsoft.com/en-us/licensing/product-licensing/power-bi',
      description: 'Analytics SaaS. Not deployed.'
    },
    {
      name: 'DocuSign',
      deployed: false,
      license: 'Commercial',
      licenseUrl: 'https://www.docusign.com/company/terms-and-conditions',
      description: 'E-signature SaaS. Not deployed.'
    },
    {
      name: 'Zapier',
      deployed: false,
      license: 'Commercial',
      licenseUrl: 'https://zapier.com/legal/terms/',
      description: 'Workflow automation SaaS. Not deployed.'
    },
  ];

  // Frontend dependencies
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

  const getLicenseColor = (license: string): 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' => {
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
              <strong>Source-Available License — Commercial Use Requires a License</strong>
            </Typography>
            <Typography variant="body2" sx={{ color: 'primary.contrastText' }}>
              Copyright © 2024-2026 Abhishek Lal. This software is source-available.<br />
              <b>Non-commercial use</b> (personal, educational, research) is freely permitted.<br />
              <b>Commercial use</b> requires a separate commercial license from the copyright holder.<br />
              <b>Contact:</b> <a href="mailto:abhishek.lal@crm.local">abhishek.lal@crm.local</a> for commercial license inquiries, support, or feature requests.
            </Typography>
            <Box sx={{ mt: 2 }}>
              <Chip label="Source Available" color="secondary" sx={{ mr: 1 }} />
              <Chip label="Non-Commercial Free" sx={{ mr: 1, bgcolor: 'white' }} />
              <Chip label="Commercial License Required" sx={{ bgcolor: 'white' }} />
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
              CRM Solution is <b>source-available</b> with a commercial license requirement.<br />
              All third-party dependencies and pluggable providers are open source or commercial software with their own license terms.<br />
              <b>Pluggable providers</b> may have additional license restrictions. See below for details.
            </Typography>
            <Grid container spacing={2}>
              <Grid item xs={6} sm={3}>
                <Box sx={{ textAlign: 'center', p: 2 }}>
                  <Typography variant="h4" color="success.main">
                    {categories.reduce((acc, cat) => acc + cat.dependencies.filter(d => d.license === 'MIT').length, 0) + pluggableProviders.filter(p => p.license === 'MIT').length}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">MIT License</Typography>
                </Box>
              </Grid>
              <Grid item xs={6} sm={3}>
                <Box sx={{ textAlign: 'center', p: 2 }}>
                  <Typography variant="h4" color="info.main">
                    {categories.reduce((acc, cat) => acc + cat.dependencies.filter(d => d.license === 'Apache-2.0').length, 0) + pluggableProviders.filter(p => p.license === 'Apache-2.0').length}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">Apache 2.0</Typography>
                </Box>
              </Grid>
              <Grid item xs={6} sm={3}>
                <Box sx={{ textAlign: 'center', p: 2 }}>
                  <Typography variant="h4" color="warning.main">
                    {categories.reduce((acc, cat) => acc + cat.dependencies.filter(d => d.license.includes('BSD') || d.license === 'ISC').length, 0) + pluggableProviders.filter(p => p.license.includes('BSD') || p.license === 'ISC').length}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">BSD/ISC</Typography>
                </Box>
              </Grid>
              <Grid item xs={6} sm={3}>
                <Box sx={{ textAlign: 'center', p: 2 }}>
                  <Typography variant="h4" color="error.main">
                    {categories.reduce((acc, cat) => acc + cat.dependencies.filter(d => d.license.includes('GPL')).length, 0) + pluggableProviders.filter(p => p.license.includes('GPL')).length}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">GPL/AGPL</Typography>
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
                                color={getLicenseColor(dep.license)}
                              />
                            </Link>
                          ) : (
                            <Chip
                              label={dep.license}
                              size="small"
                              color={getLicenseColor(dep.license)}
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

        {/* Pluggable Providers Inventory */}
        <Accordion defaultExpanded>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography variant="h6">Pluggable Architecture Inventory</Typography>
            <Chip label={pluggableProviders.length} size="small" sx={{ ml: 2 }} />
          </AccordionSummary>
          <AccordionDetails>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell><strong>Provider</strong></TableCell>
                    <TableCell><strong>Deployed</strong></TableCell>
                    <TableCell><strong>License</strong></TableCell>
                    <TableCell><strong>Description</strong></TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {pluggableProviders.map((prov, idx) => (
                    <TableRow key={idx}>
                      <TableCell>
                        <Typography variant="body2" fontWeight="medium">
                          {prov.name}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip label={prov.deployed ? 'Yes' : 'No'} color={prov.deployed ? 'success' : 'default'} size="small" />
                      </TableCell>
                      <TableCell>
                        {prov.licenseUrl ? (
                          <Link href={prov.licenseUrl} target="_blank" rel="noopener">
                            <Chip label={prov.license} size="small" color={getLicenseColor(prov.license)} />
                          </Link>
                        ) : (
                          <Chip label={prov.license} size="small" color={getLicenseColor(prov.license)} />
                        )}
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" color="text.secondary">
                          {prov.description}
                        </Typography>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
            <Box sx={{ mt: 2 }}>
              <Typography variant="body2" color="text.secondary">
                <b>Note:</b> Only deployed providers are available in the current environment. Others are documented for reference and can be enabled via configuration and feature flags.
              </Typography>
            </Box>
          </AccordionDetails>
        </Accordion>

        {/* Notice */}
        <Card variant="outlined" sx={{ mt: 4, bgcolor: 'grey.100' }}>
          <CardContent>
            <Typography variant="subtitle1" gutterBottom>
              License Details & Commercial Use
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              <b>CRM Solution:</b> Source-available license. Non-commercial use is free. Commercial use requires a license.<br />
              <b>Third-party dependencies:</b> Used in accordance with their license terms. See tables above.<br />
              <b>Pluggable providers:</b> May have additional license restrictions. See provider inventory above.<br />
              <b>Commercial License Contact:</b> For commercial use, support, or feature requests, contact <a href="mailto:abhishek.lal@crm.local">abhishek.lal@crm.local</a>.<br />
              The presence of a package or provider does not imply endorsement by its authors. For full license texts, see linked files.
            </Typography>
          </CardContent>
        </Card>
      </Paper>
    </Container>
  );
};

export default LicensesPage;
