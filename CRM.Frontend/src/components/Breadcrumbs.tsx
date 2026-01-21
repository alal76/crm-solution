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
} from '@mui/icons-material';

const BREADCRUMB_LABELS: { [key: string]: { label: string; icon?: React.ReactNode } } = {
  '/': { label: 'Dashboard', icon: <DashboardIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
  '/dashboard': { label: 'Dashboard', icon: <DashboardIcon sx={{ mr: 0.5, fontSize: 18 }} /> },
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

  let currentPath = '';
  pathnames.forEach((pathname, index) => {
    currentPath += `/${pathname}`;
    const isLast = index === pathnames.length - 1;
    const breadcrumbData = BREADCRUMB_LABELS[currentPath];
    const label = breadcrumbData?.label || pathname.charAt(0).toUpperCase() + pathname.slice(1);
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
