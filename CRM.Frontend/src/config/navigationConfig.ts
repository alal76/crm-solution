/**
 * NAVIGATION CONFIGURATION - SINGLE SOURCE OF TRUTH
 * 
 * This file serves as the central configuration for all navigation items in the CRM.
 * Any new feature or page should be added here, and all navigation components will
 * automatically pick up the changes.
 * 
 * To add a new navigation item:
 * 1. Add the route to App.tsx
 * 2. Add an entry to NAV_ITEMS_CONFIG below
 * 3. Import the icon in the relevant components (Navigation.tsx, NavigationSettingsTab.tsx)
 * 4. Rebuild - the nav item will appear automatically
 * 
 * The database (DbSeed.cs) should be updated for new installations to have correct defaults.
 */

import {
  Dashboard as DashboardIcon,
  Business as CustomersIcon,
  PersonSearch as CustomerOverviewIcon,
  People as ContactsIcon,
  Handshake as RelationshipsIcon,
  TrendingUp as LeadsIcon,
  StarBorder as OpportunitiesIcon,
  Inventory as ProductsIcon,
  Build as ServicesIcon,
  ConfirmationNumber as ServiceRequestsIcon,
  Campaign as CampaignsIcon,
  RocketLaunch as CampaignExecutionIcon,
  RequestQuote as QuotesIcon,
  Queue as QueueIcon,
  Assignment as ActivitiesIcon,
  Note as NotesIcon,
  Mail as CommunicationsIcon,
  ConnectWithoutContact as InteractionsIcon,
  Info as AboutIcon,
  Help as HelpIcon,
  Gavel as LicensesIcon,
  AccountTree as WorkflowsIcon,
  Settings as SettingsIcon,
  ChatBubbleOutline as ChannelSettingsIcon,
} from '@mui/icons-material';
import { SvgIconComponent } from '@mui/icons-material';
import React from 'react';

/**
 * Category definitions for organizing navigation items
 */
export interface NavCategory {
  id: string;
  label: string;
  order: number;
}

/**
 * Navigation item definition
 */
export interface NavItemConfig {
  id: string;
  label: string;
  path: string;
  icon: SvgIconComponent;
  order: number;
  category: string;
  isAdmin?: boolean;
  requiredPermission?: string;
  description?: string;
}

/**
 * Central category configuration
 * All UI components should reference this array
 */
export const CATEGORIES: NavCategory[] = [
  { id: 'main', label: 'Main', order: 0 },
  { id: 'sales', label: 'Sales & Marketing', order: 1 },
  { id: 'support', label: 'Customer Support', order: 2 },
  { id: 'productivity', label: 'Productivity', order: 3 },
  { id: 'info', label: 'Help & Info', order: 4 },
  { id: 'admin', label: 'Administration', order: 5 },
];

/**
 * Central navigation items configuration
 * This is the SINGLE SOURCE OF TRUTH for all navigation items
 */
export const NAV_ITEMS_CONFIG: NavItemConfig[] = [
  // Main category
  {
    id: 'dashboard',
    label: 'Dashboard',
    path: '/dashboard',
    icon: DashboardIcon,
    order: 0,
    category: 'main',
    description: 'Overview and key metrics',
  },
  {
    id: 'customers',
    label: 'Customers',
    path: '/customers',
    icon: CustomersIcon,
    order: 1,
    category: 'main',
    description: 'Customer account management',
  },
  {
    id: 'customer-overview',
    label: 'Customer Overview',
    path: '/customer-overview',
    icon: CustomerOverviewIcon,
    order: 2,
    category: 'main',
    description: 'Consolidated customer view',
  },
  {
    id: 'contacts',
    label: 'Contacts',
    path: '/contacts',
    icon: ContactsIcon,
    order: 3,
    category: 'main',
    description: 'Contact management',
  },
  {
    id: 'relationships',
    label: 'Relationships',
    path: '/relationships',
    icon: RelationshipsIcon,
    order: 4,
    category: 'main',
    description: 'Manage entity relationships',
  },

  // Sales & Marketing category
  {
    id: 'leads',
    label: 'Leads',
    path: '/leads',
    icon: LeadsIcon,
    order: 5,
    category: 'sales',
    description: 'Lead tracking and conversion',
  },
  {
    id: 'opportunities',
    label: 'Opportunities',
    path: '/opportunities',
    icon: OpportunitiesIcon,
    order: 6,
    category: 'sales',
    description: 'Sales pipeline management',
  },
  {
    id: 'products',
    label: 'Products',
    path: '/products',
    icon: ProductsIcon,
    order: 7,
    category: 'sales',
    description: 'Product catalog',
  },
  {
    id: 'campaigns',
    label: 'Campaigns',
    path: '/campaigns',
    icon: CampaignsIcon,
    order: 10,
    category: 'sales',
    description: 'Marketing campaign management',
  },
  {
    id: 'campaign-execution',
    label: 'Campaign Execution',
    path: '/campaign-execution',
    icon: CampaignExecutionIcon,
    order: 11,
    category: 'sales',
    description: 'Execute and monitor campaigns',
  },
  {
    id: 'quotes',
    label: 'Quotes',
    path: '/quotes',
    icon: QuotesIcon,
    order: 12,
    category: 'sales',
    description: 'Sales quotes and proposals',
  },

  // Customer Support category
  {
    id: 'services',
    label: 'Services',
    path: '/services',
    icon: ServicesIcon,
    order: 8,
    category: 'support',
    description: 'Service catalog',
  },
  {
    id: 'service-requests',
    label: 'Service Requests',
    path: '/service-requests',
    icon: ServiceRequestsIcon,
    order: 9,
    category: 'support',
    description: 'Customer service tickets',
  },

  // Productivity category
  {
    id: 'my-queue',
    label: 'My Queue',
    path: '/my-queue',
    icon: QueueIcon,
    order: 13,
    category: 'productivity',
    description: 'Personal work queue',
  },
  {
    id: 'activities',
    label: 'Activities',
    path: '/activities',
    icon: ActivitiesIcon,
    order: 14,
    category: 'productivity',
    description: 'Tasks and scheduled activities',
  },
  {
    id: 'notes',
    label: 'Notes',
    path: '/notes',
    icon: NotesIcon,
    order: 15,
    category: 'productivity',
    description: 'Notes and annotations',
  },
  {
    id: 'communications',
    label: 'Communications',
    path: '/communications',
    icon: CommunicationsIcon,
    order: 16,
    category: 'productivity',
    description: 'Email and messaging',
  },
  {
    id: 'interactions',
    label: 'Interactions',
    path: '/interactions',
    icon: InteractionsIcon,
    order: 17,
    category: 'productivity',
    description: 'Customer interaction history',
  },

  // Help & Info category
  {
    id: 'about',
    label: 'About',
    path: '/about',
    icon: AboutIcon,
    order: 18,
    category: 'info',
    description: 'About this application',
  },
  {
    id: 'help',
    label: 'Help',
    path: '/help',
    icon: HelpIcon,
    order: 19,
    category: 'info',
    description: 'Documentation and help',
  },
  {
    id: 'licenses',
    label: 'Licenses',
    path: '/licenses',
    icon: LicensesIcon,
    order: 20,
    category: 'info',
    description: 'Open source licenses',
  },

  // Administration category
  {
    id: 'workflows',
    label: 'Workflows',
    path: '/admin/workflows',
    icon: WorkflowsIcon,
    order: 21,
    category: 'admin',
    isAdmin: true,
    description: 'Workflow automation',
  },
  {
    id: 'channel-settings',
    label: 'Channel Settings',
    path: '/admin/channel-settings',
    icon: ChannelSettingsIcon,
    order: 22,
    category: 'admin',
    isAdmin: true,
    description: 'Communication channel configuration',
  },
  {
    id: 'settings',
    label: 'Settings',
    path: '/settings',
    icon: SettingsIcon,
    order: 23,
    category: 'admin',
    isAdmin: true,
    description: 'System settings',
  },
];

/**
 * Get a navigation item by ID
 */
export const getNavItem = (id: string): NavItemConfig | undefined => {
  return NAV_ITEMS_CONFIG.find(item => item.id === id);
};

/**
 * Get all navigation items for a category
 */
export const getNavItemsByCategory = (categoryId: string): NavItemConfig[] => {
  return NAV_ITEMS_CONFIG.filter(item => item.category === categoryId)
    .sort((a, b) => a.order - b.order);
};

/**
 * Get the icon component for a navigation item
 */
export const getIconForNavItem = (id: string): SvgIconComponent | undefined => {
  const item = getNavItem(id);
  return item?.icon;
};

/**
 * Create icon map for components that need string-based icon lookup
 */
export const createIconMap = (): Record<string, React.ReactElement> => {
  const iconMap: Record<string, React.ReactElement> = {};
  NAV_ITEMS_CONFIG.forEach(item => {
    const IconComponent = item.icon;
    iconMap[`${item.id}Icon`] = React.createElement(IconComponent);
  });
  
  // Also add by component name for backward compatibility
  iconMap['DashboardIcon'] = React.createElement(DashboardIcon);
  iconMap['CustomersIcon'] = React.createElement(CustomersIcon);
  iconMap['CustomerOverviewIcon'] = React.createElement(CustomerOverviewIcon);
  iconMap['ContactsIcon'] = React.createElement(ContactsIcon);
  iconMap['RelationshipsIcon'] = React.createElement(RelationshipsIcon);
  iconMap['LeadsIcon'] = React.createElement(LeadsIcon);
  iconMap['OpportunitiesIcon'] = React.createElement(OpportunitiesIcon);
  iconMap['ProductsIcon'] = React.createElement(ProductsIcon);
  iconMap['ServicesIcon'] = React.createElement(ServicesIcon);
  iconMap['ServiceRequestsIcon'] = React.createElement(ServiceRequestsIcon);
  iconMap['CampaignsIcon'] = React.createElement(CampaignsIcon);
  iconMap['CampaignExecutionIcon'] = React.createElement(CampaignExecutionIcon);
  iconMap['QuotesIcon'] = React.createElement(QuotesIcon);
  iconMap['QueueIcon'] = React.createElement(QueueIcon);
  iconMap['ActivitiesIcon'] = React.createElement(ActivitiesIcon);
  iconMap['NotesIcon'] = React.createElement(NotesIcon);
  iconMap['CommunicationsIcon'] = React.createElement(CommunicationsIcon);
  iconMap['InteractionsIcon'] = React.createElement(InteractionsIcon);
  iconMap['AboutIcon'] = React.createElement(AboutIcon);
  iconMap['HelpIcon'] = React.createElement(HelpIcon);
  iconMap['LicensesIcon'] = React.createElement(LicensesIcon);
  iconMap['WorkflowsIcon'] = React.createElement(WorkflowsIcon);
  iconMap['ChannelSettingsIcon'] = React.createElement(ChannelSettingsIcon);
  iconMap['SettingsIcon'] = React.createElement(SettingsIcon);
  
  return iconMap;
};

/**
 * Generate default nav order for localStorage initialization
 */
export const generateDefaultNavOrder = (): string[] => {
  return NAV_ITEMS_CONFIG
    .filter(item => !item.isAdmin)
    .sort((a, b) => a.order - b.order)
    .map(item => item.id);
};

/**
 * Generate JSON configuration for database seeding
 * This can be used to keep DbSeed.cs in sync
 */
export const generateDbSeedConfig = (): string => {
  const navItems = NAV_ITEMS_CONFIG.map(item => ({
    id: item.id,
    order: item.order,
    visible: true,
    category: item.category,
  }));

  const categories = CATEGORIES.map(cat => ({
    id: cat.id,
    label: cat.label,
    order: cat.order,
  }));

  return JSON.stringify({ navItems, categories });
};

/**
 * Export all nav item IDs for type safety
 */
export type NavItemId = typeof NAV_ITEMS_CONFIG[number]['id'];

/**
 * Export category IDs for type safety
 */
export type CategoryId = typeof CATEGORIES[number]['id'];
