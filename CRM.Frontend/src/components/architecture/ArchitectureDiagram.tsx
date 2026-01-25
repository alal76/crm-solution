/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Architecture Diagram Component
 * Displays a graphical view of the solution architecture including
 * modules, entities, database tables, and relationships.
 */

import React, { useState, useMemo } from 'react';
import {
  Box,
  Typography,
  Paper,
  Tabs,
  Tab,
  Card,
  CardContent,
  Chip,
  Grid,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  TextField,
  InputAdornment,
  Tooltip,
  Badge,
  Divider,
  Alert,
  AlertTitle,
  useTheme,
} from '@mui/material';
import {
  ExpandMore as ExpandMoreIcon,
  Search as SearchIcon,
  Storage as StorageIcon,
  AccountTree as AccountTreeIcon,
  TableChart as TableChartIcon,
  Link as LinkIcon,
  Key as KeyIcon,
  ViewModule as ViewModuleIcon,
  Warning as WarningIcon,
  CheckCircle as CheckCircleIcon,
  ArrowForward as ArrowForwardIcon,
  ArrowBack as ArrowBackIcon,
  SwapHoriz as SwapHorizIcon,
} from '@mui/icons-material';

// ============ DATA DEFINITIONS ============

// Module definitions with their entities
interface Module {
  name: string;
  description: string;
  icon: string;
  entities: string[];
  controllers: string[];
  frontendPages: string[];
}

// Entity definition with relationships
interface Entity {
  name: string;
  tableName: string;
  module: string;
  primaryKey: string;
  description: string;
  foreignKeys: ForeignKey[];
  hasDbSet: boolean;
  isJunctionTable: boolean;
}

interface ForeignKey {
  column: string;
  referencesTable: string;
  referencesColumn: string;
  relationship: 'one-to-one' | 'many-to-one' | 'one-to-many' | 'many-to-many';
  onDelete: string;
}

// Module data
const modules: Module[] = [
  {
    name: 'Customer Management',
    description: 'Core customer lifecycle and relationship management',
    icon: '👥',
    entities: ['Customer', 'CustomerContact', 'Contact', 'SocialMediaLink'],
    controllers: ['CustomersController', 'ContactsController'],
    frontendPages: ['CustomersPage', 'ContactsPage', 'CustomerOverviewPage'],
  },
  {
    name: 'Account Management',
    description: 'Contract and account lifecycle management',
    icon: '📋',
    entities: ['Account'],
    controllers: ['AccountsController'],
    frontendPages: ['AccountPage'],
  },
  {
    name: 'Sales Pipeline',
    description: 'Lead tracking, opportunities, and quotes',
    icon: '💰',
    entities: ['Lead', 'LeadProductInterest', 'Opportunity', 'OpportunityProduct', 'Quote'],
    controllers: ['OpportunitiesController', 'QuotesController', 'PipelinesController', 'StagesController'],
    frontendPages: ['LeadsPage', 'OpportunitiesPage', 'QuotesPage'],
  },
  {
    name: 'Product Catalog',
    description: 'Product and service management',
    icon: '📦',
    entities: ['Product'],
    controllers: ['ProductsController'],
    frontendPages: ['ProductsPage', 'ServicesPage'],
  },
  {
    name: 'Marketing',
    description: 'Campaign management and metrics',
    icon: '📢',
    entities: ['MarketingCampaign', 'CampaignMetric'],
    controllers: ['CampaignsController'],
    frontendPages: ['CampaignsPage'],
  },
  {
    name: 'Service Requests',
    description: 'Customer support and ticketing system',
    icon: '🎫',
    entities: [
      'ServiceRequest', 'ServiceRequestCategory', 'ServiceRequestSubcategory',
      'ServiceRequestType', 'ServiceRequestCustomFieldDefinition', 'ServiceRequestCustomFieldValue'
    ],
    controllers: ['ServiceRequestsController', 'ServiceRequestSettingsController'],
    frontendPages: ['ServiceRequestsPage', 'ServiceRequestSettingsPage'],
  },
  {
    name: 'Task Management',
    description: 'Tasks, notes, and activities tracking',
    icon: '✅',
    entities: ['CrmTask', 'Note', 'Activity'],
    controllers: ['TasksController', 'NotesController', 'ActivitiesController'],
    frontendPages: ['TasksPage', 'NotesPage', 'ActivitiesPage'],
  },
  {
    name: 'Communication',
    description: 'Multi-channel communication management',
    icon: '💬',
    entities: ['CommunicationChannel', 'CommunicationMessage', 'EmailTemplate', 'Conversation', 'Interaction'],
    controllers: ['CommunicationsController', 'InteractionsController'],
    frontendPages: ['CommunicationsPage', 'InteractionsPage', 'ChannelSettingsPage'],
  },
  {
    name: 'User Management',
    description: 'Users, roles, departments, and permissions',
    icon: '👤',
    entities: ['User', 'UserGroup', 'UserGroupMember', 'UserProfile', 'UserApprovalRequest', 'Department', 'OAuthToken'],
    controllers: ['UsersController', 'UserGroupsController', 'UserProfilesController', 'DepartmentsController', 'AuthController'],
    frontendPages: ['UserManagementPage', 'ProfileManagementPage', 'DepartmentManagementPage', 'LoginPage', 'RegisterPage'],
  },
  {
    name: 'Contact Information',
    description: 'Normalized address, phone, email, and social media data',
    icon: '📞',
    entities: [
      'Address', 'PhoneNumber', 'EmailAddress', 'SocialMediaAccount', 'SocialAccount',
      'EntityAddressLink', 'EntityPhoneLink', 'EntityEmailLink', 'EntitySocialMediaLink',
      'ContactDetail', 'ContactInfoLink', 'SocialMediaFollow'
    ],
    controllers: ['ContactInfoController'],
    frontendPages: [],
  },
  {
    name: 'Master Data',
    description: 'Reference data, lookups, and geographic data',
    icon: '📊',
    entities: ['LookupCategory', 'LookupItem', 'ZipCode', 'Locality', 'Tag', 'EntityTag', 'CustomField', 'FieldMasterDataLink'],
    controllers: ['LookupsController', 'MasterDataController', 'ZipCodesController', 'FieldMasterDataController'],
    frontendPages: ['MasterDataSettingsPage'],
  },
  {
    name: 'System Settings',
    description: 'System configuration and branding',
    icon: '⚙️',
    entities: ['SystemSettings', 'ColorPalette', 'ModuleFieldConfiguration', 'ModuleUIConfig'],
    controllers: ['SystemSettingsController', 'ColorPalettesController', 'ModuleFieldConfigurationsController', 'ModuleUIConfigController', 'AdminSettingsController'],
    frontendPages: ['SettingsPage', 'BrandingSettingsPage', 'ModuleFieldSettingsPage', 'NavigationSettingsPage', 'SecuritySettingsPage'],
  },
  {
    name: 'Database Administration',
    description: 'Backup, monitoring, and database management',
    icon: '🗄️',
    entities: ['DatabaseBackup', 'BackupSchedule'],
    controllers: ['DatabaseController', 'MonitoringController'],
    frontendPages: ['DatabaseSettingsPage', 'MonitoringSettingsPage'],
  },
  {
    name: 'Cloud Deployment',
    description: 'Cloud provider and deployment management',
    icon: '☁️',
    entities: ['CloudProvider', 'CloudDeployment', 'DeploymentAttempt', 'HealthCheckLog'],
    controllers: ['CloudDeploymentController'],
    frontendPages: ['DeploymentSettingsPage'],
  },
  {
    name: 'Dashboard & Analytics',
    description: 'Dashboard views and data analytics',
    icon: '📈',
    entities: [],
    controllers: ['DashboardController'],
    frontendPages: ['DashboardPage'],
  },
  {
    name: 'Infrastructure & Utilities',
    description: 'File uploads, webhooks, health checks, and demo data',
    icon: '🔧',
    entities: [],
    controllers: ['FileUploadController', 'HealthController', 'DemoDataController', 'WebhooksController'],
    frontendPages: [],
  },
  {
    name: 'Import/Export',
    description: 'Data import and export functionality',
    icon: '📤',
    entities: [],
    controllers: ['ImportExportController'],
    frontendPages: ['ImportExportPage'],
  },
];

// Entity definitions with relationships
const entities: Entity[] = [
  // Customer Module
  {
    name: 'Customer',
    tableName: 'Customers',
    module: 'Customer Management',
    primaryKey: 'Id',
    description: 'Core customer entity supporting both Individual and Organization types',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'ReferredByCustomerId', referencesTable: 'Customers', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'ParentCustomerId', referencesTable: 'Customers', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'ConvertedFromLeadId', referencesTable: 'Leads', referencesColumn: 'Id', relationship: 'one-to-one', onDelete: 'SetNull' },
      { column: 'SourceCampaignId', referencesTable: 'MarketingCampaigns', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'CurrencyLookupId', referencesTable: 'LookupItems', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'BillingCycleLookupId', referencesTable: 'LookupItems', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },
  {
    name: 'CustomerContact',
    tableName: 'CustomerContacts',
    module: 'Customer Management',
    primaryKey: 'Id',
    description: 'Junction table linking Customers to Contacts (many-to-many)',
    hasDbSet: true,
    isJunctionTable: true,
    foreignKeys: [
      { column: 'CustomerId', referencesTable: 'Customers', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
      { column: 'ContactId', referencesTable: 'Contacts', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
    ],
  },
  {
    name: 'Contact',
    tableName: 'Contacts',
    module: 'Customer Management',
    primaryKey: 'Id',
    description: 'Contact entity for employees, partners, leads, and other contact types',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'CustomerId', referencesTable: 'Customers', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'PreferredContactMethodLookupId', referencesTable: 'LookupItems', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },
  {
    name: 'SocialMediaLink',
    tableName: 'SocialMediaLinks',
    module: 'Customer Management',
    primaryKey: 'Id',
    description: 'Social media profile links for contacts',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'ContactId', referencesTable: 'Contacts', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
    ],
  },

  // Account Module
  {
    name: 'Account',
    tableName: 'Accounts',
    module: 'Account Management',
    primaryKey: 'Id',
    description: 'Contract and subscription management entity',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'CustomerId', referencesTable: 'Customers', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
      { column: 'ProductId', referencesTable: 'Products', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'CurrencyLookupId', referencesTable: 'LookupItems', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },

  // Sales Pipeline
  {
    name: 'Lead',
    tableName: 'Leads',
    module: 'Sales Pipeline',
    primaryKey: 'Id',
    description: 'Sales lead with qualification and scoring',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'OwnerId', referencesTable: 'Users', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'CampaignId', referencesTable: 'MarketingCampaigns', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'AccountId', referencesTable: 'Accounts', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'ContactId', referencesTable: 'Contacts', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },
  {
    name: 'LeadProductInterest',
    tableName: 'LeadProductInterests',
    module: 'Sales Pipeline',
    primaryKey: 'LeadId, ProductId',
    description: 'Junction table for Lead-Product interests (many-to-many)',
    hasDbSet: true,
    isJunctionTable: true,
    foreignKeys: [
      { column: 'LeadId', referencesTable: 'Leads', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
      { column: 'ProductId', referencesTable: 'Products', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
    ],
  },
  {
    name: 'Opportunity',
    tableName: 'Opportunities',
    module: 'Sales Pipeline',
    primaryKey: 'Id',
    description: 'Sales opportunity with pipeline stages and forecasting',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'AccountId', referencesTable: 'Accounts', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Restrict' },
      { column: 'PrimaryContactId', referencesTable: 'Contacts', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'SalesOwnerId', referencesTable: 'Users', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'LeadId', referencesTable: 'Leads', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },
  {
    name: 'OpportunityProduct',
    tableName: 'OpportunityProducts',
    module: 'Sales Pipeline',
    primaryKey: 'OpportunityId, ProductId',
    description: 'Junction table for Opportunity-Product line items (many-to-many)',
    hasDbSet: true,
    isJunctionTable: true,
    foreignKeys: [
      { column: 'OpportunityId', referencesTable: 'Opportunities', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
      { column: 'ProductId', referencesTable: 'Products', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
    ],
  },
  {
    name: 'Quote',
    tableName: 'Quotes',
    module: 'Sales Pipeline',
    primaryKey: 'Id',
    description: 'Sales quotes with versioning support',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'CustomerId', referencesTable: 'Customers', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'OpportunityId', referencesTable: 'Opportunities', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'AssignedToUserId', referencesTable: 'Users', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'ParentQuoteId', referencesTable: 'Quotes', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Restrict' },
    ],
  },

  // Product
  {
    name: 'Product',
    tableName: 'Products',
    module: 'Product Catalog',
    primaryKey: 'Id',
    description: 'Product and service catalog with pricing and inventory',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },

  // Marketing
  {
    name: 'MarketingCampaign',
    tableName: 'MarketingCampaigns',
    module: 'Marketing',
    primaryKey: 'Id',
    description: 'Marketing campaign with budget and targeting',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },
  {
    name: 'CampaignMetric',
    tableName: 'CampaignMetrics',
    module: 'Marketing',
    primaryKey: 'Id',
    description: 'Campaign performance metrics and analytics',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'CampaignId', referencesTable: 'MarketingCampaigns', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
    ],
  },

  // Service Requests
  {
    name: 'ServiceRequest',
    tableName: 'ServiceRequests',
    module: 'Service Requests',
    primaryKey: 'Id',
    description: 'Customer support tickets with SLA tracking',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'CategoryId', referencesTable: 'ServiceRequestCategories', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'SubcategoryId', referencesTable: 'ServiceRequestSubcategories', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'CustomerId', referencesTable: 'Customers', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'ContactId', referencesTable: 'Contacts', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'AssignedToUserId', referencesTable: 'Users', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'AssignedToGroupId', referencesTable: 'UserGroups', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'CreatedByUserId', referencesTable: 'Users', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'RelatedOpportunityId', referencesTable: 'Opportunities', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'RelatedProductId', referencesTable: 'Products', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'ParentServiceRequestId', referencesTable: 'ServiceRequests', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },
  {
    name: 'ServiceRequestCategory',
    tableName: 'ServiceRequestCategories',
    module: 'Service Requests',
    primaryKey: 'Id',
    description: 'Service request category classification',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },
  {
    name: 'ServiceRequestSubcategory',
    tableName: 'ServiceRequestSubcategories',
    module: 'Service Requests',
    primaryKey: 'Id',
    description: 'Service request subcategory under categories',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'CategoryId', referencesTable: 'ServiceRequestCategories', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
    ],
  },
  {
    name: 'ServiceRequestType',
    tableName: 'ServiceRequestTypes',
    module: 'Service Requests',
    primaryKey: 'Id',
    description: 'Service request type definitions with workflow',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'CategoryId', referencesTable: 'ServiceRequestCategories', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'SubcategoryId', referencesTable: 'ServiceRequestSubcategories', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },
  {
    name: 'ServiceRequestCustomFieldDefinition',
    tableName: 'ServiceRequestCustomFieldDefinitions',
    module: 'Service Requests',
    primaryKey: 'Id',
    description: 'Custom field definitions for service requests',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'CategoryId', referencesTable: 'ServiceRequestCategories', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'SubcategoryId', referencesTable: 'ServiceRequestSubcategories', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },
  {
    name: 'ServiceRequestCustomFieldValue',
    tableName: 'ServiceRequestCustomFieldValues',
    module: 'Service Requests',
    primaryKey: 'Id',
    description: 'Custom field values for service requests',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'ServiceRequestId', referencesTable: 'ServiceRequests', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
      { column: 'CustomFieldDefinitionId', referencesTable: 'ServiceRequestCustomFieldDefinitions', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
    ],
  },

  // Task Management
  {
    name: 'CrmTask',
    tableName: 'CrmTasks',
    module: 'Task Management',
    primaryKey: 'Id',
    description: 'Task management with hierarchy support',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'CustomerId', referencesTable: 'Customers', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'OpportunityId', referencesTable: 'Opportunities', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'AssignedToUserId', referencesTable: 'Users', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'ParentTaskId', referencesTable: 'CrmTasks', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Restrict' },
      { column: 'CampaignId', referencesTable: 'MarketingCampaigns', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },
  {
    name: 'Note',
    tableName: 'Notes',
    module: 'Task Management',
    primaryKey: 'Id',
    description: 'Notes attached to various entities',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'CustomerId', referencesTable: 'Customers', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
      { column: 'OpportunityId', referencesTable: 'Opportunities', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
      { column: 'CampaignId', referencesTable: 'MarketingCampaigns', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'CreatedByUserId', referencesTable: 'Users', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },
  {
    name: 'Activity',
    tableName: 'Activities',
    module: 'Task Management',
    primaryKey: 'Id',
    description: 'Activity tracking for sales and support',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'UserId', referencesTable: 'Users', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'CustomerId', referencesTable: 'Customers', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
      { column: 'OpportunityId', referencesTable: 'Opportunities', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
      { column: 'CampaignId', referencesTable: 'MarketingCampaigns', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },

  // Communication
  {
    name: 'CommunicationChannel',
    tableName: 'CommunicationChannels',
    module: 'Communication',
    primaryKey: 'Id',
    description: 'Communication channel configuration (Email, SMS, WhatsApp, etc.)',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },
  {
    name: 'CommunicationMessage',
    tableName: 'CommunicationMessages',
    module: 'Communication',
    primaryKey: 'Id',
    description: 'Individual communication messages',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'ChannelId', referencesTable: 'CommunicationChannels', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'ConversationId', referencesTable: 'Conversations', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },
  {
    name: 'EmailTemplate',
    tableName: 'EmailTemplates',
    module: 'Communication',
    primaryKey: 'Id',
    description: 'Email templates with merge fields',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },
  {
    name: 'Conversation',
    tableName: 'Conversations',
    module: 'Communication',
    primaryKey: 'Id',
    description: 'Conversation threads for multi-message interactions',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'CustomerId', referencesTable: 'Customers', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },
  {
    name: 'Interaction',
    tableName: 'Interactions',
    module: 'Communication',
    primaryKey: 'Id',
    description: 'Customer interaction history',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'CustomerId', referencesTable: 'Customers', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
      { column: 'CampaignId', referencesTable: 'MarketingCampaigns', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },

  // User Management
  {
    name: 'User',
    tableName: 'Users',
    module: 'User Management',
    primaryKey: 'Id',
    description: 'System users with authentication and roles',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'DepartmentId', referencesTable: 'Departments', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'UserProfileId', referencesTable: 'UserProfiles', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'PrimaryGroupId', referencesTable: 'UserGroups', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },
  {
    name: 'UserGroup',
    tableName: 'UserGroups',
    module: 'User Management',
    primaryKey: 'Id',
    description: 'User groups for team organization',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },
  {
    name: 'UserGroupMember',
    tableName: 'UserGroupMembers',
    module: 'User Management',
    primaryKey: 'Id',
    description: 'Junction table for User-Group membership (many-to-many)',
    hasDbSet: true,
    isJunctionTable: true,
    foreignKeys: [
      { column: 'UserId', referencesTable: 'Users', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
      { column: 'UserGroupId', referencesTable: 'UserGroups', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
    ],
  },
  {
    name: 'UserProfile',
    tableName: 'UserProfiles',
    module: 'User Management',
    primaryKey: 'Id',
    description: 'User profile with permissions and accessible pages',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'DepartmentId', referencesTable: 'Departments', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
    ],
  },
  {
    name: 'UserApprovalRequest',
    tableName: 'UserApprovalRequests',
    module: 'User Management',
    primaryKey: 'Id',
    description: 'User registration approval workflow',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'ApprovedByUserId', referencesTable: 'Users', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },
  {
    name: 'Department',
    tableName: 'Departments',
    module: 'User Management',
    primaryKey: 'Id',
    description: 'Department hierarchy with parent-child relationships',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'ParentDepartmentId', referencesTable: 'Departments', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },
  {
    name: 'OAuthToken',
    tableName: 'OAuthTokens',
    module: 'User Management',
    primaryKey: 'Id',
    description: 'OAuth tokens for social login providers',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'UserId', referencesTable: 'Users', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
    ],
  },

  // Contact Information (Normalized)
  {
    name: 'Address',
    tableName: 'Addresses',
    module: 'Contact Information',
    primaryKey: 'Id',
    description: 'Normalized address data with geocoding support',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'ZipCodeId', referencesTable: 'ZipCodes', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'LocalityId', referencesTable: 'Localities', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },
  {
    name: 'PhoneNumber',
    tableName: 'PhoneNumbers',
    module: 'Contact Information',
    primaryKey: 'Id',
    description: 'Normalized phone number data',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },
  {
    name: 'EmailAddress',
    tableName: 'EmailAddresses',
    module: 'Contact Information',
    primaryKey: 'Id',
    description: 'Normalized email address data with engagement tracking',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },
  {
    name: 'SocialMediaAccount',
    tableName: 'SocialMediaAccounts',
    module: 'Contact Information',
    primaryKey: 'Id',
    description: 'Social media account information',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },
  {
    name: 'SocialAccount',
    tableName: 'SocialAccounts',
    module: 'Contact Information',
    primaryKey: 'Id',
    description: 'Legacy social account entity',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },
  {
    name: 'EntityAddressLink',
    tableName: 'EntityAddressLinks',
    module: 'Contact Information',
    primaryKey: 'Id',
    description: 'Polymorphic junction linking any entity to addresses',
    hasDbSet: true,
    isJunctionTable: true,
    foreignKeys: [
      { column: 'AddressId', referencesTable: 'Addresses', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
    ],
  },
  {
    name: 'EntityPhoneLink',
    tableName: 'EntityPhoneLinks',
    module: 'Contact Information',
    primaryKey: 'Id',
    description: 'Polymorphic junction linking any entity to phone numbers',
    hasDbSet: true,
    isJunctionTable: true,
    foreignKeys: [
      { column: 'PhoneId', referencesTable: 'PhoneNumbers', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
    ],
  },
  {
    name: 'EntityEmailLink',
    tableName: 'EntityEmailLinks',
    module: 'Contact Information',
    primaryKey: 'Id',
    description: 'Polymorphic junction linking any entity to email addresses',
    hasDbSet: true,
    isJunctionTable: true,
    foreignKeys: [
      { column: 'EmailId', referencesTable: 'EmailAddresses', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
    ],
  },
  {
    name: 'EntitySocialMediaLink',
    tableName: 'EntitySocialMediaLinks',
    module: 'Contact Information',
    primaryKey: 'Id',
    description: 'Polymorphic junction linking any entity to social media accounts',
    hasDbSet: true,
    isJunctionTable: true,
    foreignKeys: [
      { column: 'SocialMediaAccountId', referencesTable: 'SocialMediaAccounts', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
    ],
  },
  {
    name: 'ContactDetail',
    tableName: 'ContactDetails',
    module: 'Contact Information',
    primaryKey: 'Id',
    description: 'Generic contact detail (phone/email)',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },
  {
    name: 'ContactInfoLink',
    tableName: 'ContactInfoLinks',
    module: 'Contact Information',
    primaryKey: 'Id',
    description: 'Legacy polymorphic contact info junction table',
    hasDbSet: true,
    isJunctionTable: true,
    foreignKeys: [
      { column: 'AddressId', referencesTable: 'Addresses', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'ContactDetailId', referencesTable: 'ContactDetails', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
      { column: 'SocialAccountId', referencesTable: 'SocialAccounts', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },
  {
    name: 'SocialMediaFollow',
    tableName: 'SocialMediaFollows',
    module: 'Contact Information',
    primaryKey: 'Id',
    description: 'Track user follows of social media accounts',
    hasDbSet: true,
    isJunctionTable: true,
    foreignKeys: [
      { column: 'SocialMediaAccountId', referencesTable: 'SocialMediaAccounts', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
      { column: 'FollowedByUserId', referencesTable: 'Users', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
    ],
  },

  // Master Data
  {
    name: 'LookupCategory',
    tableName: 'LookupCategories',
    module: 'Master Data',
    primaryKey: 'Id',
    description: 'Lookup category definitions',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },
  {
    name: 'LookupItem',
    tableName: 'LookupItems',
    module: 'Master Data',
    primaryKey: 'Id',
    description: 'Lookup values within categories',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'LookupCategoryId', referencesTable: 'LookupCategories', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
    ],
  },
  {
    name: 'ZipCode',
    tableName: 'ZipCodes',
    module: 'Master Data',
    primaryKey: 'Id',
    description: 'Geographic postal code master data',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },
  {
    name: 'Locality',
    tableName: 'Localities',
    module: 'Master Data',
    primaryKey: 'Id',
    description: 'Locality/neighborhood data linked to zip codes',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'ZipCodeId', referencesTable: 'ZipCodes', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },
  {
    name: 'Tag',
    tableName: 'Tags',
    module: 'Master Data',
    primaryKey: 'Id',
    description: 'Reusable tags for entity classification',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },
  {
    name: 'EntityTag',
    tableName: 'EntityTags',
    module: 'Master Data',
    primaryKey: 'Id',
    description: 'Polymorphic junction for entity tagging',
    hasDbSet: true,
    isJunctionTable: true,
    foreignKeys: [
      { column: 'TagId', referencesTable: 'Tags', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'SetNull' },
    ],
  },
  {
    name: 'CustomField',
    tableName: 'CustomFields',
    module: 'Master Data',
    primaryKey: 'Id',
    description: 'Polymorphic custom field storage',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },
  {
    name: 'FieldMasterDataLink',
    tableName: 'FieldMasterDataLinks',
    module: 'Master Data',
    primaryKey: 'Id',
    description: 'Links field configurations to lookup categories',
    hasDbSet: true,
    isJunctionTable: true,
    foreignKeys: [
      { column: 'LookupCategoryId', referencesTable: 'LookupCategories', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
    ],
  },

  // System Settings
  {
    name: 'SystemSettings',
    tableName: 'SystemSettings',
    module: 'System Settings',
    primaryKey: 'Id',
    description: 'System-wide configuration settings',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },
  {
    name: 'ColorPalette',
    tableName: 'ColorPalettes',
    module: 'System Settings',
    primaryKey: 'Id',
    description: 'Theme color palette definitions',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },
  {
    name: 'ModuleFieldConfiguration',
    tableName: 'ModuleFieldConfigurations',
    module: 'System Settings',
    primaryKey: 'Id',
    description: 'Dynamic field configuration for modules',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },
  {
    name: 'ModuleUIConfig',
    tableName: 'ModuleUIConfigs',
    module: 'System Settings',
    primaryKey: 'Id',
    description: 'UI configuration for modules',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },

  // Database Administration
  {
    name: 'DatabaseBackup',
    tableName: 'DatabaseBackups',
    module: 'Database Administration',
    primaryKey: 'Id',
    description: 'Database backup history and metadata',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },
  {
    name: 'BackupSchedule',
    tableName: 'BackupSchedules',
    module: 'Database Administration',
    primaryKey: 'Id',
    description: 'Scheduled backup configuration',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },

  // Cloud Deployment
  {
    name: 'CloudProvider',
    tableName: 'CloudProviders',
    module: 'Cloud Deployment',
    primaryKey: 'Id',
    description: 'Cloud provider configuration (AWS, Azure, GCP)',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [],
  },
  {
    name: 'CloudDeployment',
    tableName: 'CloudDeployments',
    module: 'Cloud Deployment',
    primaryKey: 'Id',
    description: 'Cloud deployment instances',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'CloudProviderId', referencesTable: 'CloudProviders', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Restrict' },
    ],
  },
  {
    name: 'DeploymentAttempt',
    tableName: 'DeploymentAttempts',
    module: 'Cloud Deployment',
    primaryKey: 'Id',
    description: 'Deployment attempt history with logs',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'CloudDeploymentId', referencesTable: 'CloudDeployments', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
    ],
  },
  {
    name: 'HealthCheckLog',
    tableName: 'HealthCheckLogs',
    module: 'Cloud Deployment',
    primaryKey: 'Id',
    description: 'Health check monitoring logs',
    hasDbSet: true,
    isJunctionTable: false,
    foreignKeys: [
      { column: 'CloudDeploymentId', referencesTable: 'CloudDeployments', referencesColumn: 'Id', relationship: 'many-to-one', onDelete: 'Cascade' },
    ],
  },
];

// Helper components
const RelationshipIcon: React.FC<{ type: string }> = ({ type }) => {
  switch (type) {
    case 'one-to-one':
      return <SwapHorizIcon fontSize="small" />;
    case 'many-to-one':
      return <ArrowForwardIcon fontSize="small" />;
    case 'one-to-many':
      return <ArrowBackIcon fontSize="small" />;
    case 'many-to-many':
      return <LinkIcon fontSize="small" />;
    default:
      return <LinkIcon fontSize="small" />;
  }
};

interface TabPanelProps {
  children?: React.ReactNode;
  value: number;
  index: number;
}

const TabPanel: React.FC<TabPanelProps> = ({ children, value, index }) => (
  <div role="tabpanel" hidden={value !== index}>
    {value === index && <Box sx={{ pt: 2 }}>{children}</Box>}
  </div>
);

// Main Component
const ArchitectureDiagram: React.FC = () => {
  const theme = useTheme();
  const [tabValue, setTabValue] = useState(0);
  const [searchTerm, setSearchTerm] = useState('');
  const [expandedModule, setExpandedModule] = useState<string | false>(false);

  // Filter entities by search term
  const filteredEntities = useMemo(() => {
    if (!searchTerm) return entities;
    const lower = searchTerm.toLowerCase();
    return entities.filter(
      e =>
        e.name.toLowerCase().includes(lower) ||
        e.tableName.toLowerCase().includes(lower) ||
        e.module.toLowerCase().includes(lower) ||
        e.description.toLowerCase().includes(lower)
    );
  }, [searchTerm]);

  // Statistics
  const stats = useMemo(() => {
    const totalEntities = entities.length;
    const totalTables = entities.filter(e => e.hasDbSet).length;
    const junctionTables = entities.filter(e => e.isJunctionTable).length;
    const totalRelationships = entities.reduce((acc, e) => acc + e.foreignKeys.length, 0);
    const entitiesWithoutTables = entities.filter(e => !e.hasDbSet).length;
    
    return {
      totalEntities,
      totalTables,
      junctionTables,
      totalRelationships,
      entitiesWithoutTables,
      totalModules: modules.length,
    };
  }, []);

  // Group entities by module
  const entitiesByModule = useMemo(() => {
    const grouped: Record<string, Entity[]> = {};
    entities.forEach(e => {
      if (!grouped[e.module]) grouped[e.module] = [];
      grouped[e.module].push(e);
    });
    return grouped;
  }, []);

  const handleTabChange = (_: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  return (
    <Box>
      {/* Statistics Cards */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid item xs={6} sm={4} md={2}>
          <Card variant="outlined" sx={{ textAlign: 'center', p: 1.5 }}>
            <ViewModuleIcon color="primary" sx={{ fontSize: 32 }} />
            <Typography variant="h5" color="primary">{stats.totalModules}</Typography>
            <Typography variant="caption" color="text.secondary">Modules</Typography>
          </Card>
        </Grid>
        <Grid item xs={6} sm={4} md={2}>
          <Card variant="outlined" sx={{ textAlign: 'center', p: 1.5 }}>
            <AccountTreeIcon color="secondary" sx={{ fontSize: 32 }} />
            <Typography variant="h5" color="secondary">{stats.totalEntities}</Typography>
            <Typography variant="caption" color="text.secondary">Entities</Typography>
          </Card>
        </Grid>
        <Grid item xs={6} sm={4} md={2}>
          <Card variant="outlined" sx={{ textAlign: 'center', p: 1.5 }}>
            <TableChartIcon color="success" sx={{ fontSize: 32 }} />
            <Typography variant="h5" color="success.main">{stats.totalTables}</Typography>
            <Typography variant="caption" color="text.secondary">DB Tables</Typography>
          </Card>
        </Grid>
        <Grid item xs={6} sm={4} md={2}>
          <Card variant="outlined" sx={{ textAlign: 'center', p: 1.5 }}>
            <LinkIcon color="info" sx={{ fontSize: 32 }} />
            <Typography variant="h5" color="info.main">{stats.junctionTables}</Typography>
            <Typography variant="caption" color="text.secondary">Junction Tables</Typography>
          </Card>
        </Grid>
        <Grid item xs={6} sm={4} md={2}>
          <Card variant="outlined" sx={{ textAlign: 'center', p: 1.5 }}>
            <KeyIcon color="warning" sx={{ fontSize: 32 }} />
            <Typography variant="h5" color="warning.main">{stats.totalRelationships}</Typography>
            <Typography variant="caption" color="text.secondary">Foreign Keys</Typography>
          </Card>
        </Grid>
        <Grid item xs={6} sm={4} md={2}>
          <Card variant="outlined" sx={{ textAlign: 'center', p: 1.5, bgcolor: stats.entitiesWithoutTables > 0 ? 'warning.light' : 'success.light' }}>
            {stats.entitiesWithoutTables > 0 ? (
              <WarningIcon sx={{ fontSize: 32, color: 'warning.dark' }} />
            ) : (
              <CheckCircleIcon sx={{ fontSize: 32, color: 'success.dark' }} />
            )}
            <Typography variant="h5" sx={{ color: stats.entitiesWithoutTables > 0 ? 'warning.dark' : 'success.dark' }}>
              {stats.entitiesWithoutTables}
            </Typography>
            <Typography variant="caption" sx={{ color: stats.entitiesWithoutTables > 0 ? 'warning.dark' : 'success.dark' }}>
              Missing DbSet
            </Typography>
          </Card>
        </Grid>
      </Grid>

      {/* Search */}
      <TextField
        fullWidth
        size="small"
        placeholder="Search entities, tables, or modules..."
        value={searchTerm}
        onChange={e => setSearchTerm(e.target.value)}
        InputProps={{
          startAdornment: (
            <InputAdornment position="start">
              <SearchIcon />
            </InputAdornment>
          ),
        }}
        sx={{ mb: 2 }}
      />

      {/* Tabs */}
      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
        <Tabs value={tabValue} onChange={handleTabChange}>
          <Tab icon={<ViewModuleIcon />} label="Modules" iconPosition="start" />
          <Tab icon={<TableChartIcon />} label="Tables & Entities" iconPosition="start" />
          <Tab icon={<LinkIcon />} label="Relationships" iconPosition="start" />
        </Tabs>
      </Box>

      {/* Tab 0: Modules View */}
      <TabPanel value={tabValue} index={0}>
        <Grid container spacing={2}>
          {modules.map((module, idx) => (
            <Grid item xs={12} md={6} lg={4} key={idx}>
              <Card variant="outlined" sx={{ height: '100%' }}>
                <CardContent>
                  <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                    <Typography variant="h4" sx={{ mr: 1 }}>{module.icon}</Typography>
                    <Typography variant="h6">{module.name}</Typography>
                  </Box>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                    {module.description}
                  </Typography>
                  
                  <Divider sx={{ my: 1 }} />
                  
                  <Typography variant="caption" fontWeight="bold" color="primary">
                    Entities ({module.entities.length})
                  </Typography>
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5, mt: 0.5, mb: 1 }}>
                    {module.entities.map((entity, i) => (
                      <Chip
                        key={i}
                        label={entity}
                        size="small"
                        variant="outlined"
                        color={entities.find(e => e.name === entity)?.isJunctionTable ? 'info' : 'default'}
                      />
                    ))}
                  </Box>
                  
                  <Typography variant="caption" fontWeight="bold" color="secondary">
                    Controllers ({module.controllers.length})
                  </Typography>
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5, mt: 0.5, mb: 1 }}>
                    {module.controllers.map((ctrl, i) => (
                      <Chip key={i} label={ctrl.replace('Controller', '')} size="small" color="secondary" variant="outlined" />
                    ))}
                  </Box>
                  
                  {module.frontendPages.length > 0 && (
                    <>
                      <Typography variant="caption" fontWeight="bold" color="success.main">
                        Pages ({module.frontendPages.length})
                      </Typography>
                      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5, mt: 0.5 }}>
                        {module.frontendPages.map((page, i) => (
                          <Chip key={i} label={page.replace('Page', '')} size="small" color="success" variant="outlined" />
                        ))}
                      </Box>
                    </>
                  )}
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      </TabPanel>

      {/* Tab 1: Tables & Entities View */}
      <TabPanel value={tabValue} index={1}>
        {Object.entries(entitiesByModule).map(([moduleName, moduleEntities]) => {
          const filteredModuleEntities = moduleEntities.filter(e => 
            filteredEntities.includes(e)
          );
          if (filteredModuleEntities.length === 0) return null;
          
          return (
            <Accordion
              key={moduleName}
              expanded={expandedModule === moduleName}
              onChange={(_, expanded) => setExpandedModule(expanded ? moduleName : false)}
              sx={{ mb: 1 }}
            >
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <Typography variant="subtitle1" fontWeight="bold">{moduleName}</Typography>
                  <Badge badgeContent={filteredModuleEntities.length} color="primary" />
                </Box>
              </AccordionSummary>
              <AccordionDetails>
                <TableContainer>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell><strong>Entity</strong></TableCell>
                        <TableCell><strong>Table Name</strong></TableCell>
                        <TableCell><strong>Primary Key</strong></TableCell>
                        <TableCell><strong>Type</strong></TableCell>
                        <TableCell><strong>FKs</strong></TableCell>
                        <TableCell><strong>Description</strong></TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {filteredModuleEntities.map((entity, idx) => (
                        <TableRow key={idx} hover>
                          <TableCell>
                            <Typography variant="body2" fontWeight="medium">
                              {entity.name}
                            </Typography>
                          </TableCell>
                          <TableCell>
                            <Chip 
                              label={entity.tableName} 
                              size="small" 
                              color={entity.hasDbSet ? 'success' : 'error'}
                              variant="outlined"
                              icon={<StorageIcon />}
                            />
                          </TableCell>
                          <TableCell>
                            <Chip 
                              label={entity.primaryKey} 
                              size="small" 
                              color="warning"
                              icon={<KeyIcon />}
                            />
                          </TableCell>
                          <TableCell>
                            {entity.isJunctionTable ? (
                              <Chip label="Junction" size="small" color="info" />
                            ) : (
                              <Chip label="Entity" size="small" color="primary" variant="outlined" />
                            )}
                          </TableCell>
                          <TableCell>{entity.foreignKeys.length}</TableCell>
                          <TableCell>
                            <Typography variant="caption" color="text.secondary">
                              {entity.description}
                            </Typography>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              </AccordionDetails>
            </Accordion>
          );
        })}
      </TabPanel>

      {/* Tab 2: Relationships View */}
      <TabPanel value={tabValue} index={2}>
        <Alert severity="info" sx={{ mb: 2 }}>
          <AlertTitle>Relationship Types</AlertTitle>
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <ArrowForwardIcon fontSize="small" /> Many-to-One (N:1)
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <SwapHorizIcon fontSize="small" /> One-to-One (1:1)
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <LinkIcon fontSize="small" /> Many-to-Many (N:N via junction)
            </Box>
          </Box>
        </Alert>

        <TableContainer component={Paper} variant="outlined">
          <Table size="small">
            <TableHead>
              <TableRow sx={{ bgcolor: theme.palette.grey[100] }}>
                <TableCell><strong>From Entity</strong></TableCell>
                <TableCell><strong>FK Column</strong></TableCell>
                <TableCell><strong>Type</strong></TableCell>
                <TableCell><strong>To Table</strong></TableCell>
                <TableCell><strong>On Delete</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredEntities
                .filter(e => e.foreignKeys.length > 0)
                .flatMap(entity =>
                  entity.foreignKeys.map((fk, idx) => (
                    <TableRow key={`${entity.name}-${idx}`} hover>
                      <TableCell>
                        <Tooltip title={entity.description}>
                          <Typography variant="body2" fontWeight="medium">
                            {entity.name}
                          </Typography>
                        </Tooltip>
                      </TableCell>
                      <TableCell>
                        <Chip 
                          label={fk.column} 
                          size="small" 
                          variant="outlined"
                          icon={<KeyIcon />}
                        />
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                          <RelationshipIcon type={fk.relationship} />
                          <Typography variant="caption">{fk.relationship}</Typography>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Chip 
                          label={`${fk.referencesTable}.${fk.referencesColumn}`} 
                          size="small" 
                          color="primary"
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell>
                        <Chip 
                          label={fk.onDelete} 
                          size="small"
                          color={
                            fk.onDelete === 'Cascade' ? 'error' : 
                            fk.onDelete === 'Restrict' ? 'warning' : 'default'
                          }
                          variant="outlined"
                        />
                      </TableCell>
                    </TableRow>
                  ))
                )}
            </TableBody>
          </Table>
        </TableContainer>
      </TabPanel>
    </Box>
  );
};

export default ArchitectureDiagram;
