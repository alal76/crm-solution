import { Breadcrumbs, Link, Box, Typography } from '@mui/material';
import { Link as RouterLink, useLocation } from 'react-router-dom';
import {
  Dashboard as DashboardIcon,
  People as PeopleIcon,
  TrendingUp as TrendingUpIcon,
  Inventory2 as ProductsIcon,
  Campaign as CampaignsIcon,
  Settings as SettingsIcon,
  Home as HomeIcon,
  Contacts as ContactsIcon,
  Assignment as TaskIcon,
  Description as QuoteIcon,
  Note as NoteIcon,
  Timeline as ActivityIcon,
  AutoAwesome as WorkflowIcon,
  Lock as LockIcon,
  Business as DepartmentIcon,
  AccountCircle as ProfileIcon,
  MiscellaneousServices as ServicesIcon,
  SupportAgent as SupportAgentIcon,
  Storage as StorageIcon,
  Cloud as CloudIcon,
  Monitor as MonitorIcon,
  Security as SecurityIcon,
  ToggleOn as FeatureToggleIcon,
  PersonAdd as PersonAddIcon,
  Groups as GroupsIcon,
  Login as LoginIcon,
  Palette as PaletteIcon,
  ViewModule as ModuleIcon,
  AdminPanelSettings as AdminIcon,
} from '@mui/icons-material';

// Define admin category structure
const ADMIN_CATEGORIES: Record<string, { label: string; items: string[] }> = {
  'system': {
    label: 'System Administration',
    items: ['database', 'deployment', 'monitoring', 'security', 'features']
  },
  'users': {
    label: 'User Administration',
    items: ['users', 'approvals', 'groups', 'social-login']
  },
  'crm': {
    label: 'CRM Administration',
    items: ['branding', 'navigation', 'modules', 'service-requests', 'master-data', 'dashboards', 'workflows']
  },
  'advanced': {
    label: 'Advanced',
    items: ['test-results', 'llm', 'api-docs']
  }
};

// Helper function to find admin category for a path
const getAdminCategory = (adminPath: string): { category: string; categoryLabel: string } | null => {
  for (const [categoryId, category] of Object.entries(ADMIN_CATEGORIES)) {
    if (category.items.includes(adminPath)) {
      return { category: categoryId, categoryLabel: category.label };
    }
  }
  return null;
};

const BREADCRUMB_LABELS: { [key: string]: { label: string; icon?: React.ReactNode } } = {
  '/': { label: 'My Queue', icon: <TaskIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/dashboard': { label: 'Dashboard', icon: <DashboardIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/my-queue': { label: 'My Queue', icon: <TaskIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/customers': { label: 'Customers', icon: <PeopleIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/contacts': { label: 'Contacts', icon: <ContactsIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/leads': { label: 'Leads', icon: <PeopleIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/opportunities': { label: 'Opportunities', icon: <TrendingUpIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/products': { label: 'Products', icon: <ProductsIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/services': { label: 'Services', icon: <ServicesIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/service-requests': { label: 'Service Requests', icon: <SupportAgentIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/service-request-settings': { label: 'Service Request Settings', icon: <SupportAgentIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/campaigns': { label: 'Campaigns', icon: <CampaignsIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/quotes': { label: 'Quotes', icon: <QuoteIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/tasks': { label: 'Tasks', icon: <TaskIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/activities': { label: 'Activities', icon: <ActivityIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/notes': { label: 'Notes', icon: <NoteIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/workflows': { label: 'Workflows', icon: <WorkflowIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/2fa': { label: '2-Factor Auth', icon: <LockIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/password-reset': { label: 'Change Password', icon: <LockIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/users': { label: 'User Management', icon: <PeopleIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/departments': { label: 'Departments', icon: <DepartmentIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/profiles': { label: 'Profiles', icon: <ProfileIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/settings': { label: 'Settings', icon: <SettingsIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  // Admin pages
  '/admin': { label: 'Administration', icon: <AdminIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/admin/database': { label: 'Database', icon: <StorageIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/admin/deployment': { label: 'Deployment', icon: <CloudIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/admin/monitoring': { label: 'Monitoring', icon: <MonitorIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/admin/security': { label: 'Security', icon: <SecurityIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/admin/features': { label: 'Features', icon: <FeatureToggleIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/admin/users': { label: 'Users', icon: <PeopleIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/admin/approvals': { label: 'Approvals', icon: <PersonAddIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/admin/groups': { label: 'Groups', icon: <GroupsIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/admin/social-login': { label: 'Social Login', icon: <LoginIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/admin/branding': { label: 'Branding', icon: <PaletteIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/admin/navigation': { label: 'Navigation', icon: <SettingsIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/admin/modules': { label: 'Modules & Fields', icon: <ModuleIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/admin/service-requests': { label: 'Service Requests', icon: <SupportAgentIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/admin/master-data': { label: 'Master Data', icon: <StorageIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/admin/dashboards': { label: 'Dashboards', icon: <DashboardIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/admin/workflows': { label: 'Workflows', icon: <WorkflowIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/admin/test-results': { label: 'Test Results', icon: <SettingsIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/admin/llm': { label: 'AI / LLM Settings', icon: <SettingsIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/admin/api-docs': { label: 'API Documentation', icon: <SettingsIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
};

function BreadcrumbsComponent() {
  const location = useLocation();
  const pathnames = location.pathname.split('/').filter((x) => x);

  // Don't show breadcrumbs on login/register pages
  if (location.pathname === '/login' || location.pathname === '/register' || location.pathname === '/password-reset') {
    return null;
  }

  const breadcrumbs = [
    <Link
      component={RouterLink}
      to="/"
      key="home"
      sx={{
        display: 'flex',
        alignItems: 'center',
        color: '#6750A4',
        textDecoration: 'none',
        '&:hover': { textDecoration: 'underline' },
      }}
    >
      <HomeIcon sx={{ mr: 0.5, fontSize: 20 }} />
      Home
    </Link>,
  ];

  // Special handling for admin pages to show category hierarchy
  const isAdminPath = location.pathname.startsWith('/admin/');
  
  if (isAdminPath && pathnames.length >= 2) {
    // For admin pages, show: Home > Category > Page
    const adminItem = pathnames[1]; // e.g., 'database', 'monitoring', etc.
    const categoryInfo = getAdminCategory(adminItem);
    
    if (categoryInfo) {
      // Add the category as a clickable breadcrumb (links to first item in that category)
      breadcrumbs.push(
        <Typography 
          key="admin-category" 
          sx={{ 
            display: 'flex', 
            alignItems: 'center', 
            color: '#6750A4',
            fontSize: '0.875rem',
          }}
        >
          <AdminIcon sx={{ mr: 0.5, fontSize: 18 }} />
          {categoryInfo.categoryLabel}
        </Typography>
      );
    }
    
    // Add the final page item
    const fullPath = `/${pathnames.join('/')}`;
    const breadcrumbData = BREADCRUMB_LABELS[fullPath];
    const label = breadcrumbData?.label || adminItem.charAt(0).toUpperCase() + adminItem.slice(1).replace(/-/g, ' ');
    const icon = breadcrumbData?.icon;
    
    breadcrumbs.push(
      <Typography key={fullPath} sx={{ display: 'flex', alignItems: 'center', color: '#625B71', fontWeight: 600, fontSize: '0.875rem' }}>
        {icon}
        {label}
      </Typography>
    );
  } else {
    // Standard breadcrumb handling for non-admin pages
    let currentPath = '';
    pathnames.forEach((pathname, index) => {
      currentPath += `/${pathname}`;
      const isLast = index === pathnames.length - 1;
      const breadcrumbData = BREADCRUMB_LABELS[currentPath];
      const label = breadcrumbData?.label || pathname.charAt(0).toUpperCase() + pathname.slice(1).replace(/-/g, ' ');
      const icon = breadcrumbData?.icon;

      if (isLast) {
        breadcrumbs.push(
          <Typography key={currentPath} sx={{ display: 'flex', alignItems: 'center', color: '#625B71', fontWeight: 600, fontSize: '0.875rem' }}>
            {icon}
            {label}
          </Typography>
        );
      } else {
        breadcrumbs.push(
          <Link
            component={RouterLink}
            to={currentPath}
            key={currentPath}
            sx={{
              display: 'flex',
              alignItems: 'center',
              color: '#6750A4',
              textDecoration: 'none',
              fontSize: '0.875rem',
              '&:hover': { textDecoration: 'underline' },
            }}
          >
            {icon}
            {label}
          </Link>
        );
      }
    });
  }

  return (
    <Box sx={{ 
      px: 3, 
      py: 1, 
      backgroundColor: '#FAFAFA', 
      borderBottom: '1px solid #E0E0E0',
      boxShadow: '0 1px 2px rgba(0,0,0,0.05)'
    }}>
      <Breadcrumbs
        separator="›"
        sx={{
          '& .MuiBreadcrumbs-separator': {
            color: '#CAC7D0',
            mx: 0.75,
            fontSize: '0.9rem',
          },
        }}
      >
        {breadcrumbs}
      </Breadcrumbs>
    </Box>
  );
}

export default BreadcrumbsComponent;
