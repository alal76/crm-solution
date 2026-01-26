/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

import React, { useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Card,
  CardContent,
  Tabs,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  TextField,
  InputAdornment,
  IconButton,
  Tooltip,
  Alert,
  Divider,
  Link,
  Button,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
} from '@mui/material';
import {
  Api as ApiIcon,
  ExpandMore as ExpandMoreIcon,
  Search as SearchIcon,
  ContentCopy as CopyIcon,
  Code as CodeIcon,
  Security as SecurityIcon,
  WebhookOutlined as WebhookIcon,
  CloudQueue as CloudIcon,
  Storage as StorageIcon,
  Sync as SyncIcon,
  Email as EmailIcon,
  Notifications as NotificationsIcon,
  Check as CheckIcon,
  Info as InfoIcon,
  Warning as WarningIcon,
  OpenInNew as OpenInNewIcon,
} from '@mui/icons-material';

interface ApiEndpoint {
  method: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH';
  path: string;
  description: string;
  auth: boolean;
  parameters?: { name: string; type: string; required: boolean; description: string }[];
  requestBody?: string;
  responseExample?: string;
  category: string;
}

interface Webhook {
  event: string;
  description: string;
  payload: string;
  category: string;
}

const apiEndpoints: ApiEndpoint[] = [
  // Authentication
  { method: 'POST', path: '/api/auth/login', description: 'Authenticate user and receive JWT token', auth: false, category: 'Authentication', requestBody: '{ "email": "string", "password": "string" }', responseExample: '{ "token": "jwt_token", "refreshToken": "refresh_token", "user": {...} }' },
  { method: 'POST', path: '/api/auth/register', description: 'Register a new user account', auth: false, category: 'Authentication', requestBody: '{ "email": "string", "password": "string", "firstName": "string", "lastName": "string" }' },
  { method: 'POST', path: '/api/auth/refresh', description: 'Refresh JWT token using refresh token', auth: false, category: 'Authentication', requestBody: '{ "refreshToken": "string" }' },
  { method: 'POST', path: '/api/auth/logout', description: 'Invalidate current session', auth: true, category: 'Authentication' },
  { method: 'GET', path: '/api/auth/me', description: 'Get current authenticated user profile', auth: true, category: 'Authentication', responseExample: '{ "id": 1, "email": "user@example.com", "firstName": "John", "lastName": "Doe", "roles": ["Admin"] }' },
  { method: 'POST', path: '/api/auth/password-reset', description: 'Request password reset email', auth: false, category: 'Authentication', requestBody: '{ "email": "string" }' },
  { method: 'POST', path: '/api/auth/2fa/setup', description: 'Set up two-factor authentication', auth: true, category: 'Authentication' },
  { method: 'POST', path: '/api/auth/2fa/verify', description: 'Verify 2FA code during login', auth: false, category: 'Authentication', requestBody: '{ "code": "string", "tempToken": "string" }' },

  // Customers
  { method: 'GET', path: '/api/customers', description: 'Get paginated list of customers', auth: true, category: 'Customers', parameters: [{ name: 'page', type: 'int', required: false, description: 'Page number' }, { name: 'pageSize', type: 'int', required: false, description: 'Items per page' }, { name: 'search', type: 'string', required: false, description: 'Search query' }] },
  { method: 'GET', path: '/api/customers/{id}', description: 'Get customer by ID', auth: true, category: 'Customers' },
  { method: 'POST', path: '/api/customers', description: 'Create new customer', auth: true, category: 'Customers', requestBody: '{ "firstName": "string", "lastName": "string", "email": "string", "phone": "string", "category": "Individual|Organization", "lifecycleStage": "string" }' },
  { method: 'PUT', path: '/api/customers/{id}', description: 'Update customer', auth: true, category: 'Customers' },
  { method: 'DELETE', path: '/api/customers/{id}', description: 'Soft delete customer', auth: true, category: 'Customers' },
  { method: 'GET', path: '/api/customers/{id}/contacts', description: 'Get contacts for customer', auth: true, category: 'Customers' },
  { method: 'GET', path: '/api/customers/{id}/opportunities', description: 'Get opportunities for customer', auth: true, category: 'Customers' },

  // Contacts
  { method: 'GET', path: '/api/contacts', description: 'Get paginated list of contacts', auth: true, category: 'Contacts' },
  { method: 'GET', path: '/api/contacts/{id}', description: 'Get contact by ID', auth: true, category: 'Contacts' },
  { method: 'POST', path: '/api/contacts', description: 'Create new contact', auth: true, category: 'Contacts', requestBody: '{ "firstName": "string", "lastName": "string", "email": "string", "phone": "string", "customerId": "int?" }' },
  { method: 'PUT', path: '/api/contacts/{id}', description: 'Update contact', auth: true, category: 'Contacts' },
  { method: 'DELETE', path: '/api/contacts/{id}', description: 'Delete contact', auth: true, category: 'Contacts' },
  { method: 'GET', path: '/api/contactinfo/{contactId}', description: 'Get all contact info entries', auth: true, category: 'Contacts' },
  { method: 'POST', path: '/api/contactinfo', description: 'Add contact info entry (phone, email, address)', auth: true, category: 'Contacts' },

  // Accounts
  { method: 'GET', path: '/api/accounts', description: 'Get paginated list of accounts', auth: true, category: 'Accounts' },
  { method: 'GET', path: '/api/accounts/{id}', description: 'Get account by ID', auth: true, category: 'Accounts' },
  { method: 'POST', path: '/api/accounts', description: 'Create new account', auth: true, category: 'Accounts', requestBody: '{ "name": "string", "mrr": "decimal", "arr": "decimal", "status": "string" }' },
  { method: 'PUT', path: '/api/accounts/{id}', description: 'Update account', auth: true, category: 'Accounts' },
  { method: 'DELETE', path: '/api/accounts/{id}', description: 'Delete account', auth: true, category: 'Accounts' },
  { method: 'GET', path: '/api/accounts/mrr-report', description: 'Get MRR report', auth: true, category: 'Accounts' },

  // Leads
  { method: 'GET', path: '/api/leads', description: 'Get paginated list of leads', auth: true, category: 'Leads' },
  { method: 'GET', path: '/api/leads/{id}', description: 'Get lead by ID', auth: true, category: 'Leads' },
  { method: 'POST', path: '/api/leads', description: 'Create new lead', auth: true, category: 'Leads', requestBody: '{ "firstName": "string", "lastName": "string", "email": "string", "source": "string", "score": "int" }' },
  { method: 'PUT', path: '/api/leads/{id}', description: 'Update lead', auth: true, category: 'Leads' },
  { method: 'DELETE', path: '/api/leads/{id}', description: 'Delete lead', auth: true, category: 'Leads' },
  { method: 'POST', path: '/api/leads/{id}/convert', description: 'Convert lead to opportunity', auth: true, category: 'Leads' },

  // Opportunities
  { method: 'GET', path: '/api/opportunities', description: 'Get paginated list of opportunities', auth: true, category: 'Opportunities' },
  { method: 'GET', path: '/api/opportunities/{id}', description: 'Get opportunity by ID', auth: true, category: 'Opportunities' },
  { method: 'POST', path: '/api/opportunities', description: 'Create new opportunity', auth: true, category: 'Opportunities', requestBody: '{ "title": "string", "amount": "decimal", "probability": "int", "stageId": "int", "accountId": "int" }' },
  { method: 'PUT', path: '/api/opportunities/{id}', description: 'Update opportunity', auth: true, category: 'Opportunities' },
  { method: 'DELETE', path: '/api/opportunities/{id}', description: 'Delete opportunity', auth: true, category: 'Opportunities' },
  { method: 'GET', path: '/api/opportunities/pipeline', description: 'Get pipeline value summary', auth: true, category: 'Opportunities' },
  { method: 'PUT', path: '/api/opportunities/{id}/stage', description: 'Update opportunity stage', auth: true, category: 'Opportunities' },

  // Pipelines & Stages
  { method: 'GET', path: '/api/pipelines', description: 'Get all pipelines', auth: true, category: 'Pipelines' },
  { method: 'GET', path: '/api/pipelines/{id}', description: 'Get pipeline by ID with stages', auth: true, category: 'Pipelines' },
  { method: 'POST', path: '/api/pipelines', description: 'Create new pipeline', auth: true, category: 'Pipelines' },
  { method: 'GET', path: '/api/stages', description: 'Get all stages', auth: true, category: 'Pipelines' },
  { method: 'POST', path: '/api/stages', description: 'Create new stage', auth: true, category: 'Pipelines' },

  // Products & Quotes
  { method: 'GET', path: '/api/products', description: 'Get all products', auth: true, category: 'Products' },
  { method: 'GET', path: '/api/products/{id}', description: 'Get product by ID', auth: true, category: 'Products' },
  { method: 'POST', path: '/api/products', description: 'Create new product', auth: true, category: 'Products', requestBody: '{ "name": "string", "sku": "string", "price": "decimal", "category": "string" }' },
  { method: 'PUT', path: '/api/products/{id}', description: 'Update product', auth: true, category: 'Products' },
  { method: 'DELETE', path: '/api/products/{id}', description: 'Delete product', auth: true, category: 'Products' },
  { method: 'GET', path: '/api/quotes', description: 'Get all quotes', auth: true, category: 'Products' },
  { method: 'POST', path: '/api/quotes', description: 'Create new quote', auth: true, category: 'Products' },

  // Tasks & Activities
  { method: 'GET', path: '/api/tasks', description: 'Get tasks for current user', auth: true, category: 'Tasks' },
  { method: 'GET', path: '/api/tasks/{id}', description: 'Get task by ID', auth: true, category: 'Tasks' },
  { method: 'POST', path: '/api/tasks', description: 'Create new task', auth: true, category: 'Tasks', requestBody: '{ "title": "string", "description": "string", "dueDate": "datetime", "priority": "string", "assignedToId": "int?" }' },
  { method: 'PUT', path: '/api/tasks/{id}', description: 'Update task', auth: true, category: 'Tasks' },
  { method: 'DELETE', path: '/api/tasks/{id}', description: 'Delete task', auth: true, category: 'Tasks' },
  { method: 'PUT', path: '/api/tasks/{id}/complete', description: 'Mark task as complete', auth: true, category: 'Tasks' },
  { method: 'GET', path: '/api/activities', description: 'Get activities', auth: true, category: 'Tasks' },
  { method: 'POST', path: '/api/activities', description: 'Log activity', auth: true, category: 'Tasks' },

  // Campaigns
  { method: 'GET', path: '/api/campaigns', description: 'Get all campaigns', auth: true, category: 'Campaigns' },
  { method: 'GET', path: '/api/campaigns/{id}', description: 'Get campaign by ID', auth: true, category: 'Campaigns' },
  { method: 'POST', path: '/api/campaigns', description: 'Create new campaign', auth: true, category: 'Campaigns', requestBody: '{ "name": "string", "type": "string", "budget": "decimal", "startDate": "datetime", "endDate": "datetime" }' },
  { method: 'PUT', path: '/api/campaigns/{id}', description: 'Update campaign', auth: true, category: 'Campaigns' },
  { method: 'DELETE', path: '/api/campaigns/{id}', description: 'Delete campaign', auth: true, category: 'Campaigns' },
  { method: 'GET', path: '/api/campaigns/{id}/metrics', description: 'Get campaign metrics', auth: true, category: 'Campaigns' },

  // Service Requests
  { method: 'GET', path: '/api/servicerequests', description: 'Get service requests', auth: true, category: 'Service Desk' },
  { method: 'POST', path: '/api/servicerequests', description: 'Create service request', auth: true, category: 'Service Desk' },
  { method: 'PUT', path: '/api/servicerequests/{id}', description: 'Update service request', auth: true, category: 'Service Desk' },
  { method: 'GET', path: '/api/servicerequestsettings', description: 'Get service request types', auth: true, category: 'Service Desk' },

  // Communications
  { method: 'GET', path: '/api/communications', description: 'Get communications', auth: true, category: 'Communications' },
  { method: 'POST', path: '/api/communications', description: 'Log communication', auth: true, category: 'Communications' },
  { method: 'GET', path: '/api/interactions', description: 'Get interactions', auth: true, category: 'Communications' },
  { method: 'POST', path: '/api/interactions', description: 'Log interaction', auth: true, category: 'Communications' },

  // Users & Admin
  { method: 'GET', path: '/api/users', description: 'Get all users (Admin only)', auth: true, category: 'Admin' },
  { method: 'GET', path: '/api/users/{id}', description: 'Get user by ID', auth: true, category: 'Admin' },
  { method: 'POST', path: '/api/users', description: 'Create new user (Admin only)', auth: true, category: 'Admin' },
  { method: 'PUT', path: '/api/users/{id}', description: 'Update user', auth: true, category: 'Admin' },
  { method: 'DELETE', path: '/api/users/{id}', description: 'Deactivate user', auth: true, category: 'Admin' },
  { method: 'GET', path: '/api/usergroups', description: 'Get all user groups', auth: true, category: 'Admin' },
  { method: 'POST', path: '/api/usergroups', description: 'Create user group', auth: true, category: 'Admin' },
  { method: 'GET', path: '/api/departments', description: 'Get all departments', auth: true, category: 'Admin' },
  { method: 'POST', path: '/api/departments', description: 'Create department', auth: true, category: 'Admin' },

  // System Settings
  { method: 'GET', path: '/api/systemsettings', description: 'Get system settings', auth: true, category: 'Settings' },
  { method: 'PUT', path: '/api/systemsettings', description: 'Update system settings (Admin only)', auth: true, category: 'Settings' },
  { method: 'GET', path: '/api/adminsettings', description: 'Get admin settings', auth: true, category: 'Settings' },
  { method: 'PUT', path: '/api/adminsettings', description: 'Update admin settings', auth: true, category: 'Settings' },
  { method: 'GET', path: '/api/adminsettings/llm', description: 'Get LLM provider settings', auth: true, category: 'Settings' },
  { method: 'PUT', path: '/api/adminsettings/llm', description: 'Update LLM provider settings', auth: true, category: 'Settings' },
  { method: 'GET', path: '/api/modulefieldconfigurations', description: 'Get module field configs', auth: true, category: 'Settings' },
  { method: 'PUT', path: '/api/modulefieldconfigurations', description: 'Update module field configs', auth: true, category: 'Settings' },

  // Dashboard
  { method: 'GET', path: '/api/dashboard', description: 'Get dashboard data', auth: true, category: 'Dashboard' },
  { method: 'GET', path: '/api/dashboard/metrics', description: 'Get dashboard metrics', auth: true, category: 'Dashboard' },
  { method: 'GET', path: '/api/dashboardconfig', description: 'Get dashboard configuration', auth: true, category: 'Dashboard' },
  { method: 'PUT', path: '/api/dashboardconfig', description: 'Update dashboard configuration', auth: true, category: 'Dashboard' },

  // Workflows
  { method: 'GET', path: '/api/workflow/definitions', description: 'Get workflow definitions', auth: true, category: 'Workflows' },
  { method: 'POST', path: '/api/workflow/definitions', description: 'Create workflow definition', auth: true, category: 'Workflows' },
  { method: 'GET', path: '/api/workflow/instances', description: 'Get workflow instances', auth: true, category: 'Workflows' },
  { method: 'POST', path: '/api/workflow/instances/{id}/execute', description: 'Execute workflow', auth: true, category: 'Workflows' },

  // Webhooks
  { method: 'GET', path: '/api/webhooks', description: 'Get registered webhooks', auth: true, category: 'Webhooks' },
  { method: 'POST', path: '/api/webhooks', description: 'Register new webhook', auth: true, category: 'Webhooks', requestBody: '{ "name": "string", "url": "string", "events": ["string"], "secret": "string?" }' },
  { method: 'PUT', path: '/api/webhooks/{id}', description: 'Update webhook', auth: true, category: 'Webhooks' },
  { method: 'DELETE', path: '/api/webhooks/{id}', description: 'Delete webhook', auth: true, category: 'Webhooks' },
  { method: 'POST', path: '/api/webhooks/{id}/test', description: 'Test webhook delivery', auth: true, category: 'Webhooks' },

  // Monitoring & Health
  { method: 'GET', path: '/api/health', description: 'Health check endpoint', auth: false, category: 'System' },
  { method: 'GET', path: '/api/health/ready', description: 'Readiness probe', auth: false, category: 'System' },
  { method: 'GET', path: '/api/health/live', description: 'Liveness probe', auth: false, category: 'System' },
  { method: 'GET', path: '/api/monitoring', description: 'Get monitoring metrics', auth: true, category: 'System' },
  { method: 'GET', path: '/api/database/status', description: 'Get database status', auth: true, category: 'System' },

  // Import/Export
  { method: 'POST', path: '/api/importexport/import', description: 'Import data from file', auth: true, category: 'Import/Export' },
  { method: 'GET', path: '/api/importexport/export/{entity}', description: 'Export entity data', auth: true, category: 'Import/Export' },
  { method: 'GET', path: '/api/importexport/template/{entity}', description: 'Get import template', auth: true, category: 'Import/Export' },

  // Lookups & Master Data
  { method: 'GET', path: '/api/lookups/{type}', description: 'Get lookup values by type', auth: true, category: 'Lookups' },
  { method: 'GET', path: '/api/masterdata/{type}', description: 'Get master data by type', auth: true, category: 'Lookups' },
  { method: 'GET', path: '/api/zipcodes/search', description: 'Search zip codes', auth: true, category: 'Lookups', parameters: [{ name: 'query', type: 'string', required: true, description: 'Zip code or city' }] },
  { method: 'GET', path: '/api/fieldmasterdata/{fieldName}', description: 'Get field master data', auth: true, category: 'Lookups' },
];

const webhooks: Webhook[] = [
  { event: 'customer.created', description: 'Fired when a new customer is created', category: 'Customer', payload: '{ "event": "customer.created", "timestamp": "ISO8601", "data": { "id": 1, "firstName": "John", "lastName": "Doe", "email": "john@example.com" } }' },
  { event: 'customer.updated', description: 'Fired when a customer is updated', category: 'Customer', payload: '{ "event": "customer.updated", "timestamp": "ISO8601", "data": { "id": 1, "changes": {...}, "previousValues": {...} } }' },
  { event: 'customer.deleted', description: 'Fired when a customer is deleted', category: 'Customer', payload: '{ "event": "customer.deleted", "timestamp": "ISO8601", "data": { "id": 1 } }' },
  { event: 'contact.created', description: 'Fired when a new contact is created', category: 'Contact', payload: '{ "event": "contact.created", "timestamp": "ISO8601", "data": { "id": 1, "firstName": "Jane", "lastName": "Smith", "customerId": 1 } }' },
  { event: 'lead.created', description: 'Fired when a new lead is created', category: 'Lead', payload: '{ "event": "lead.created", "timestamp": "ISO8601", "data": { "id": 1, "firstName": "Bob", "source": "Web", "score": 75 } }' },
  { event: 'lead.converted', description: 'Fired when a lead is converted to opportunity', category: 'Lead', payload: '{ "event": "lead.converted", "timestamp": "ISO8601", "data": { "leadId": 1, "opportunityId": 5, "accountId": 10 } }' },
  { event: 'opportunity.created', description: 'Fired when a new opportunity is created', category: 'Opportunity', payload: '{ "event": "opportunity.created", "timestamp": "ISO8601", "data": { "id": 1, "title": "New Deal", "amount": 50000, "stageId": 1 } }' },
  { event: 'opportunity.stage_changed', description: 'Fired when opportunity stage changes', category: 'Opportunity', payload: '{ "event": "opportunity.stage_changed", "timestamp": "ISO8601", "data": { "id": 1, "previousStage": "Qualification", "newStage": "Proposal", "probability": 50 } }' },
  { event: 'opportunity.won', description: 'Fired when an opportunity is marked as won', category: 'Opportunity', payload: '{ "event": "opportunity.won", "timestamp": "ISO8601", "data": { "id": 1, "amount": 50000, "accountId": 10, "closedDate": "ISO8601" } }' },
  { event: 'opportunity.lost', description: 'Fired when an opportunity is marked as lost', category: 'Opportunity', payload: '{ "event": "opportunity.lost", "timestamp": "ISO8601", "data": { "id": 1, "reason": "Budget constraints", "competitorId": null } }' },
  { event: 'task.created', description: 'Fired when a task is created', category: 'Task', payload: '{ "event": "task.created", "timestamp": "ISO8601", "data": { "id": 1, "title": "Follow up", "dueDate": "ISO8601", "assignedToId": 5 } }' },
  { event: 'task.completed', description: 'Fired when a task is completed', category: 'Task', payload: '{ "event": "task.completed", "timestamp": "ISO8601", "data": { "id": 1, "completedBy": 5, "completedAt": "ISO8601" } }' },
  { event: 'campaign.started', description: 'Fired when a campaign starts', category: 'Campaign', payload: '{ "event": "campaign.started", "timestamp": "ISO8601", "data": { "id": 1, "name": "Q1 Promo", "type": "Email", "budget": 10000 } }' },
  { event: 'service_request.created', description: 'Fired when a service request is created', category: 'Service', payload: '{ "event": "service_request.created", "timestamp": "ISO8601", "data": { "id": 1, "title": "Support Request", "priority": "High", "customerId": 10 } }' },
  { event: 'service_request.resolved', description: 'Fired when a service request is resolved', category: 'Service', payload: '{ "event": "service_request.resolved", "timestamp": "ISO8601", "data": { "id": 1, "resolution": "Issue fixed", "resolvedBy": 5 } }' },
  { event: 'user.created', description: 'Fired when a user is created', category: 'User', payload: '{ "event": "user.created", "timestamp": "ISO8601", "data": { "id": 1, "email": "new@example.com", "roles": ["User"] } }' },
  { event: 'user.login', description: 'Fired when a user logs in', category: 'User', payload: '{ "event": "user.login", "timestamp": "ISO8601", "data": { "userId": 1, "ip": "192.168.1.1", "userAgent": "..." } }' },
];

const ApiDocumentationPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState(0);
  const [searchQuery, setSearchQuery] = useState('');
  const [expandedCategory, setExpandedCategory] = useState<string | false>('Authentication');
  const [copiedText, setCopiedText] = useState<string | null>(null);

  const handleCopy = (text: string) => {
    navigator.clipboard.writeText(text);
    setCopiedText(text);
    setTimeout(() => setCopiedText(null), 2000);
  };

  const getMethodColor = (method: string) => {
    switch (method) {
      case 'GET': return 'success';
      case 'POST': return 'primary';
      case 'PUT': return 'warning';
      case 'DELETE': return 'error';
      case 'PATCH': return 'info';
      default: return 'default';
    }
  };

  const categories = [...new Set(apiEndpoints.map(e => e.category))];

  const filteredEndpoints = apiEndpoints.filter(endpoint =>
    endpoint.path.toLowerCase().includes(searchQuery.toLowerCase()) ||
    endpoint.description.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const filteredWebhooks = webhooks.filter(webhook =>
    webhook.event.toLowerCase().includes(searchQuery.toLowerCase()) ||
    webhook.description.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const baseUrl = typeof window !== 'undefined' ? `${window.location.protocol}//${window.location.host}` : 'https://your-domain.com';

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <ApiIcon color="primary" sx={{ fontSize: 40 }} />
          <Box>
            <Typography variant="h4" component="h1">
              API Documentation
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Complete reference for CRM Solution REST API, Webhooks, and Integrations
            </Typography>
          </Box>
        </Box>
        <Button
          variant="outlined"
          startIcon={<OpenInNewIcon />}
          href="/swagger"
          target="_blank"
        >
          Open Swagger UI
        </Button>
      </Box>

      <Paper sx={{ mb: 3 }}>
        <Tabs value={activeTab} onChange={(e, v) => setActiveTab(v)} variant="scrollable" scrollButtons="auto">
          <Tab icon={<ApiIcon />} label="REST API" iconPosition="start" />
          <Tab icon={<WebhookIcon />} label="Webhooks" iconPosition="start" />
          <Tab icon={<SyncIcon />} label="Event Grid / Messaging" iconPosition="start" />
          <Tab icon={<SecurityIcon />} label="Authentication" iconPosition="start" />
          <Tab icon={<CodeIcon />} label="Integration Guide" iconPosition="start" />
        </Tabs>
      </Paper>

      {/* Search */}
      <TextField
        fullWidth
        size="small"
        placeholder="Search endpoints, webhooks, or documentation..."
        value={searchQuery}
        onChange={(e) => setSearchQuery(e.target.value)}
        InputProps={{
          startAdornment: <InputAdornment position="start"><SearchIcon /></InputAdornment>,
        }}
        sx={{ mb: 3 }}
      />

      {/* Tab 0: REST API */}
      {activeTab === 0 && (
        <Grid container spacing={3}>
          <Grid item xs={12} md={3}>
            <Paper sx={{ p: 2 }}>
              <Typography variant="subtitle2" gutterBottom>Categories</Typography>
              <List dense>
                {categories.map(cat => (
                  <ListItem
                    key={cat}
                    button
                    selected={expandedCategory === cat}
                    onClick={() => setExpandedCategory(cat)}
                  >
                    <ListItemText
                      primary={cat}
                      secondary={`${apiEndpoints.filter(e => e.category === cat).length} endpoints`}
                    />
                  </ListItem>
                ))}
              </List>
            </Paper>
          </Grid>
          <Grid item xs={12} md={9}>
            <Paper sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom>
                {expandedCategory} Endpoints
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Base URL: <code>{baseUrl}/api</code>
              </Typography>

              {filteredEndpoints
                .filter(e => e.category === expandedCategory)
                .map((endpoint, idx) => (
                  <Accordion key={idx}>
                    <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, width: '100%' }}>
                        <Chip
                          label={endpoint.method}
                          size="small"
                          color={getMethodColor(endpoint.method) as any}
                          sx={{ minWidth: 60 }}
                        />
                        <Typography sx={{ fontFamily: 'monospace', flexGrow: 1 }}>
                          {endpoint.path}
                        </Typography>
                        {endpoint.auth && (
                          <Tooltip title="Requires Authentication">
                            <SecurityIcon color="action" fontSize="small" />
                          </Tooltip>
                        )}
                      </Box>
                    </AccordionSummary>
                    <AccordionDetails>
                      <Typography variant="body2" color="text.secondary" paragraph>
                        {endpoint.description}
                      </Typography>

                      {endpoint.parameters && endpoint.parameters.length > 0 && (
                        <Box sx={{ mb: 2 }}>
                          <Typography variant="subtitle2">Parameters</Typography>
                          <TableContainer>
                            <Table size="small">
                              <TableHead>
                                <TableRow>
                                  <TableCell>Name</TableCell>
                                  <TableCell>Type</TableCell>
                                  <TableCell>Required</TableCell>
                                  <TableCell>Description</TableCell>
                                </TableRow>
                              </TableHead>
                              <TableBody>
                                {endpoint.parameters.map((param, pidx) => (
                                  <TableRow key={pidx}>
                                    <TableCell><code>{param.name}</code></TableCell>
                                    <TableCell>{param.type}</TableCell>
                                    <TableCell>{param.required ? 'Yes' : 'No'}</TableCell>
                                    <TableCell>{param.description}</TableCell>
                                  </TableRow>
                                ))}
                              </TableBody>
                            </Table>
                          </TableContainer>
                        </Box>
                      )}

                      {endpoint.requestBody && (
                        <Box sx={{ mb: 2 }}>
                          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <Typography variant="subtitle2">Request Body</Typography>
                            <IconButton size="small" onClick={() => handleCopy(endpoint.requestBody!)}>
                              {copiedText === endpoint.requestBody ? <CheckIcon fontSize="small" color="success" /> : <CopyIcon fontSize="small" />}
                            </IconButton>
                          </Box>
                          <Box sx={{ bgcolor: 'grey.100', p: 2, borderRadius: 1, fontFamily: 'monospace', fontSize: 12 }}>
                            <pre style={{ margin: 0 }}>{endpoint.requestBody}</pre>
                          </Box>
                        </Box>
                      )}

                      {endpoint.responseExample && (
                        <Box>
                          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <Typography variant="subtitle2">Response Example</Typography>
                            <IconButton size="small" onClick={() => handleCopy(endpoint.responseExample!)}>
                              {copiedText === endpoint.responseExample ? <CheckIcon fontSize="small" color="success" /> : <CopyIcon fontSize="small" />}
                            </IconButton>
                          </Box>
                          <Box sx={{ bgcolor: 'grey.100', p: 2, borderRadius: 1, fontFamily: 'monospace', fontSize: 12 }}>
                            <pre style={{ margin: 0 }}>{endpoint.responseExample}</pre>
                          </Box>
                        </Box>
                      )}

                      <Box sx={{ mt: 2 }}>
                        <Typography variant="caption" color="text.secondary">
                          cURL Example:
                        </Typography>
                        <Box sx={{ bgcolor: 'grey.900', color: 'grey.100', p: 2, borderRadius: 1, fontFamily: 'monospace', fontSize: 11, mt: 1 }}>
                          <code>
                            curl -X {endpoint.method} "{baseUrl}{endpoint.path}" \<br />
                            {endpoint.auth && '  -H "Authorization: Bearer YOUR_JWT_TOKEN" \\\n'}
                            {'  -H "Content-Type: application/json"'}
                            {endpoint.requestBody && ` \\\n  -d '${endpoint.requestBody}'`}
                          </code>
                        </Box>
                      </Box>
                    </AccordionDetails>
                  </Accordion>
                ))}
            </Paper>
          </Grid>
        </Grid>
      )}

      {/* Tab 1: Webhooks */}
      {activeTab === 1 && (
        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom>
            Webhook Events
          </Typography>
          <Alert severity="info" sx={{ mb: 3 }}>
            Webhooks allow you to receive real-time notifications when events occur in the CRM.
            Register webhook URLs in Settings → Webhooks to start receiving events.
          </Alert>

          <Typography variant="subtitle2" gutterBottom>Registering a Webhook</Typography>
          <Box sx={{ bgcolor: 'grey.100', p: 2, borderRadius: 1, fontFamily: 'monospace', fontSize: 12, mb: 3 }}>
            <pre style={{ margin: 0 }}>{`POST /api/webhooks
{
  "name": "My Integration",
  "url": "https://your-server.com/webhook",
  "events": ["customer.created", "opportunity.won"],
  "secret": "your_signing_secret"  // Optional: for signature verification
}`}</pre>
          </Box>

          <Typography variant="subtitle2" gutterBottom>Available Events</Typography>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Event</TableCell>
                  <TableCell>Category</TableCell>
                  <TableCell>Description</TableCell>
                  <TableCell>Payload</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredWebhooks.map((webhook, idx) => (
                  <TableRow key={idx} hover>
                    <TableCell>
                      <code style={{ color: '#1976d2' }}>{webhook.event}</code>
                    </TableCell>
                    <TableCell>
                      <Chip label={webhook.category} size="small" variant="outlined" />
                    </TableCell>
                    <TableCell>{webhook.description}</TableCell>
                    <TableCell>
                      <Tooltip title={<pre style={{ margin: 0, fontSize: 10 }}>{webhook.payload}</pre>}>
                        <IconButton size="small" onClick={() => handleCopy(webhook.payload)}>
                          <CodeIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>

          <Box sx={{ mt: 4 }}>
            <Typography variant="subtitle2" gutterBottom>Webhook Signature Verification</Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              If you provide a secret when registering, each webhook request will include a <code>X-CRM-Signature</code> header.
              Verify this signature to ensure the webhook is from CRM Solution.
            </Typography>
            <Box sx={{ bgcolor: 'grey.100', p: 2, borderRadius: 1, fontFamily: 'monospace', fontSize: 12 }}>
              <pre style={{ margin: 0 }}>{`// Node.js verification example
const crypto = require('crypto');

function verifySignature(payload, signature, secret) {
  const expectedSignature = crypto
    .createHmac('sha256', secret)
    .update(payload)
    .digest('hex');
  return signature === 'sha256=' + expectedSignature;
}`}</pre>
            </Box>
          </Box>
        </Paper>
      )}

      {/* Tab 2: Event Grid / Messaging */}
      {activeTab === 2 && (
        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <CloudIcon /> Event Grid & Messaging Integration
          </Typography>

          <Alert severity="info" sx={{ mb: 3 }}>
            CRM Solution supports integration with Azure Event Grid, Azure Service Bus, RabbitMQ, and other message brokers
            for enterprise-grade event-driven architectures.
          </Alert>

          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <Card variant="outlined">
                <CardContent>
                  <Typography variant="subtitle1" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <CloudIcon color="primary" /> Azure Event Grid
                  </Typography>
                  <Typography variant="body2" color="text.secondary" paragraph>
                    Publish CRM events to Azure Event Grid for serverless event handling.
                  </Typography>
                  <Typography variant="subtitle2">Configuration</Typography>
                  <Box sx={{ bgcolor: 'grey.100', p: 2, borderRadius: 1, fontFamily: 'monospace', fontSize: 11, mt: 1 }}>
                    <pre style={{ margin: 0 }}>{`// appsettings.json
{
  "EventGrid": {
    "Enabled": true,
    "TopicEndpoint": "https://your-topic.region.eventgrid.azure.net/api/events",
    "TopicKey": "your-access-key",
    "Events": [
      "customer.created",
      "opportunity.won"
    ]
  }
}`}</pre>
                  </Box>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} md={6}>
              <Card variant="outlined">
                <CardContent>
                  <Typography variant="subtitle1" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <StorageIcon color="primary" /> Azure Service Bus
                  </Typography>
                  <Typography variant="body2" color="text.secondary" paragraph>
                    Use Service Bus queues and topics for reliable message delivery.
                  </Typography>
                  <Typography variant="subtitle2">Configuration</Typography>
                  <Box sx={{ bgcolor: 'grey.100', p: 2, borderRadius: 1, fontFamily: 'monospace', fontSize: 11, mt: 1 }}>
                    <pre style={{ margin: 0 }}>{`// appsettings.json
{
  "ServiceBus": {
    "Enabled": true,
    "ConnectionString": "Endpoint=sb://...",
    "QueueName": "crm-events",
    "TopicName": "crm-topic"
  }
}`}</pre>
                  </Box>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} md={6}>
              <Card variant="outlined">
                <CardContent>
                  <Typography variant="subtitle1" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <SyncIcon color="primary" /> RabbitMQ
                  </Typography>
                  <Typography variant="body2" color="text.secondary" paragraph>
                    Connect to RabbitMQ for on-premise message queuing.
                  </Typography>
                  <Typography variant="subtitle2">Configuration</Typography>
                  <Box sx={{ bgcolor: 'grey.100', p: 2, borderRadius: 1, fontFamily: 'monospace', fontSize: 11, mt: 1 }}>
                    <pre style={{ margin: 0 }}>{`// appsettings.json
{
  "RabbitMQ": {
    "Enabled": true,
    "Host": "rabbitmq.local",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "Exchange": "crm.events"
  }
}`}</pre>
                  </Box>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} md={6}>
              <Card variant="outlined">
                <CardContent>
                  <Typography variant="subtitle1" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <EmailIcon color="primary" /> Email Notifications
                  </Typography>
                  <Typography variant="body2" color="text.secondary" paragraph>
                    Configure SMTP or SendGrid for email notifications.
                  </Typography>
                  <Typography variant="subtitle2">Configuration</Typography>
                  <Box sx={{ bgcolor: 'grey.100', p: 2, borderRadius: 1, fontFamily: 'monospace', fontSize: 11, mt: 1 }}>
                    <pre style={{ margin: 0 }}>{`// appsettings.json
{
  "Email": {
    "Provider": "SendGrid", // or "SMTP"
    "ApiKey": "SG.xxx",
    "FromAddress": "noreply@crm.com",
    "FromName": "CRM Notifications"
  }
}`}</pre>
                  </Box>
                </CardContent>
              </Card>
            </Grid>
          </Grid>

          <Box sx={{ mt: 4 }}>
            <Typography variant="h6" gutterBottom>Event Message Format</Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              All events follow the CloudEvents 1.0 specification:
            </Typography>
            <Box sx={{ bgcolor: 'grey.100', p: 2, borderRadius: 1, fontFamily: 'monospace', fontSize: 12 }}>
              <pre style={{ margin: 0 }}>{`{
  "specversion": "1.0",
  "type": "com.crm.customer.created",
  "source": "/crm/customers",
  "id": "uuid-here",
  "time": "2026-01-26T17:38:00Z",
  "datacontenttype": "application/json",
  "data": {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com"
  }
}`}</pre>
            </Box>
          </Box>
        </Paper>
      )}

      {/* Tab 3: Authentication */}
      {activeTab === 3 && (
        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <SecurityIcon /> Authentication & Authorization
          </Typography>

          <Alert severity="warning" sx={{ mb: 3 }}>
            All API endpoints except login, register, and health checks require authentication.
            Include the JWT token in the Authorization header.
          </Alert>

          <Typography variant="subtitle1" gutterBottom>JWT Authentication Flow</Typography>
          <Box sx={{ bgcolor: 'grey.100', p: 2, borderRadius: 1, fontFamily: 'monospace', fontSize: 12, mb: 3 }}>
            <pre style={{ margin: 0 }}>{`// 1. Login to get tokens
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "your-password"
}

// Response:
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "refresh-token-here",
  "expiresIn": 3600,
  "user": { "id": 1, "email": "user@example.com", ... }
}

// 2. Use token in subsequent requests
GET /api/customers
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...

// 3. Refresh token before expiry
POST /api/auth/refresh
{
  "refreshToken": "refresh-token-here"
}`}</pre>
          </Box>

          <Typography variant="subtitle1" gutterBottom>Two-Factor Authentication</Typography>
          <Typography variant="body2" color="text.secondary" paragraph>
            If 2FA is enabled, login returns a temporary token that must be verified:
          </Typography>
          <Box sx={{ bgcolor: 'grey.100', p: 2, borderRadius: 1, fontFamily: 'monospace', fontSize: 12, mb: 3 }}>
            <pre style={{ margin: 0 }}>{`// Login with 2FA enabled returns:
{
  "requires2FA": true,
  "tempToken": "temp-token-for-2fa"
}

// Verify 2FA code:
POST /api/auth/2fa/verify
{
  "tempToken": "temp-token-for-2fa",
  "code": "123456"  // From authenticator app
}`}</pre>
          </Box>

          <Typography variant="subtitle1" gutterBottom>Role-Based Access Control</Typography>
          <TableContainer>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Role</TableCell>
                  <TableCell>Permissions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                <TableRow><TableCell><strong>Admin</strong></TableCell><TableCell>Full access to all features and settings</TableCell></TableRow>
                <TableRow><TableCell><strong>Manager</strong></TableCell><TableCell>Full CRUD on all entities, view reports, manage team</TableCell></TableRow>
                <TableRow><TableCell><strong>Sales</strong></TableCell><TableCell>CRUD on customers, contacts, leads, opportunities, tasks</TableCell></TableRow>
                <TableRow><TableCell><strong>Marketing</strong></TableCell><TableCell>CRUD on campaigns, leads, view customers</TableCell></TableRow>
                <TableRow><TableCell><strong>Support</strong></TableCell><TableCell>CRUD on service requests, view customers and contacts</TableCell></TableRow>
                <TableRow><TableCell><strong>User</strong></TableCell><TableCell>View access, create tasks and activities</TableCell></TableRow>
              </TableBody>
            </Table>
          </TableContainer>
        </Paper>
      )}

      {/* Tab 4: Integration Guide */}
      {activeTab === 4 && (
        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <CodeIcon /> Integration Guide
          </Typography>

          <Alert severity="success" sx={{ mb: 3 }}>
            Follow these steps to integrate your application with CRM Solution.
          </Alert>

          <Accordion defaultExpanded>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Typography variant="subtitle1">Step 1: Obtain API Credentials</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <Typography variant="body2" paragraph>
                1. Log in to CRM Solution as an Admin user<br />
                2. Navigate to Settings → Security<br />
                3. Create an API user or use existing credentials<br />
                4. Note the email and password for API access
              </Typography>
            </AccordionDetails>
          </Accordion>

          <Accordion>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Typography variant="subtitle1">Step 2: Authenticate & Get Token</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <Box sx={{ bgcolor: 'grey.100', p: 2, borderRadius: 1, fontFamily: 'monospace', fontSize: 12 }}>
                <pre style={{ margin: 0 }}>{`// JavaScript/TypeScript
const response = await fetch('${baseUrl}/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'api-user@company.com',
    password: 'your-api-password'
  })
});
const { token } = await response.json();

// Python
import requests
response = requests.post('${baseUrl}/api/auth/login', json={
    'email': 'api-user@company.com',
    'password': 'your-api-password'
})
token = response.json()['token']`}</pre>
              </Box>
            </AccordionDetails>
          </Accordion>

          <Accordion>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Typography variant="subtitle1">Step 3: Make API Calls</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <Box sx={{ bgcolor: 'grey.100', p: 2, borderRadius: 1, fontFamily: 'monospace', fontSize: 12 }}>
                <pre style={{ margin: 0 }}>{`// JavaScript/TypeScript
const customers = await fetch('${baseUrl}/api/customers', {
  headers: { 
    'Authorization': \`Bearer \${token}\`,
    'Content-Type': 'application/json'
  }
}).then(r => r.json());

// Create a customer
await fetch('${baseUrl}/api/customers', {
  method: 'POST',
  headers: { 
    'Authorization': \`Bearer \${token}\`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    firstName: 'Jane',
    lastName: 'Doe',
    email: 'jane@example.com',
    category: 'Individual'
  })
});`}</pre>
              </Box>
            </AccordionDetails>
          </Accordion>

          <Accordion>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Typography variant="subtitle1">Step 4: Handle Pagination</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <Typography variant="body2" paragraph>
                List endpoints support pagination with <code>page</code> and <code>pageSize</code> parameters:
              </Typography>
              <Box sx={{ bgcolor: 'grey.100', p: 2, borderRadius: 1, fontFamily: 'monospace', fontSize: 12 }}>
                <pre style={{ margin: 0 }}>{`GET /api/customers?page=1&pageSize=50

// Response includes:
{
  "items": [...],
  "totalCount": 500,
  "page": 1,
  "pageSize": 50,
  "totalPages": 10
}`}</pre>
              </Box>
            </AccordionDetails>
          </Accordion>

          <Accordion>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Typography variant="subtitle1">Step 5: Set Up Webhooks</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <Typography variant="body2" paragraph>
                Register a webhook to receive real-time notifications:
              </Typography>
              <Box sx={{ bgcolor: 'grey.100', p: 2, borderRadius: 1, fontFamily: 'monospace', fontSize: 12 }}>
                <pre style={{ margin: 0 }}>{`POST /api/webhooks
{
  "name": "My Integration Webhook",
  "url": "https://your-server.com/crm-webhook",
  "events": [
    "customer.created",
    "customer.updated",
    "opportunity.won",
    "opportunity.lost"
  ],
  "secret": "your-signing-secret"
}`}</pre>
              </Box>
            </AccordionDetails>
          </Accordion>

          <Accordion>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Typography variant="subtitle1">Error Handling</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <Typography variant="body2" paragraph>
                The API returns standard HTTP status codes:
              </Typography>
              <TableContainer>
                <Table size="small">
                  <TableBody>
                    <TableRow><TableCell><strong>200 OK</strong></TableCell><TableCell>Request successful</TableCell></TableRow>
                    <TableRow><TableCell><strong>201 Created</strong></TableCell><TableCell>Resource created successfully</TableCell></TableRow>
                    <TableRow><TableCell><strong>400 Bad Request</strong></TableCell><TableCell>Invalid request body or parameters</TableCell></TableRow>
                    <TableRow><TableCell><strong>401 Unauthorized</strong></TableCell><TableCell>Missing or invalid authentication</TableCell></TableRow>
                    <TableRow><TableCell><strong>403 Forbidden</strong></TableCell><TableCell>Insufficient permissions</TableCell></TableRow>
                    <TableRow><TableCell><strong>404 Not Found</strong></TableCell><TableCell>Resource not found</TableCell></TableRow>
                    <TableRow><TableCell><strong>409 Conflict</strong></TableCell><TableCell>Resource already exists</TableCell></TableRow>
                    <TableRow><TableCell><strong>500 Internal Error</strong></TableCell><TableCell>Server error</TableCell></TableRow>
                  </TableBody>
                </Table>
              </TableContainer>
            </AccordionDetails>
          </Accordion>

          <Box sx={{ mt: 4 }}>
            <Typography variant="subtitle1" gutterBottom>Rate Limiting</Typography>
            <Alert severity="info">
              API requests are limited to 1000 requests per minute per user.
              The remaining quota is returned in response headers:
              <code>X-RateLimit-Remaining</code>, <code>X-RateLimit-Reset</code>
            </Alert>
          </Box>
        </Paper>
      )}
    </Box>
  );
};

export default ApiDocumentationPage;
