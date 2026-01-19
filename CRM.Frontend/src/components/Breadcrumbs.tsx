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
} from '@mui/icons-material';

const BREADCRUMB_LABELS: { [key: string]: { label: string; icon?: React.ReactNode } } = {
  '/': { label: 'Dashboard', icon: <DashboardIcon sx={{ mr: 0.5, fontSize: 20 }} /> },
  '/dashboard': { label: 'Dashboard', icon: <DashboardIcon sx={{ mr: 0.5, fontSize: 20 }} /> },
  '/customers': { label: 'Customers', icon: <PeopleIcon sx={{ mr: 0.5, fontSize: 20 }} /> },
  '/opportunities': { label: 'Opportunities', icon: <TrendingUpIcon sx={{ mr: 0.5, fontSize: 20 }} /> },
  '/products': { label: 'Products', icon: <ProductsIcon sx={{ mr: 0.5, fontSize: 20 }} /> },
  '/campaigns': { label: 'Campaigns', icon: <CampaignsIcon sx={{ mr: 0.5, fontSize: 20 }} /> },
  '/leads': { label: 'Leads', icon: <PeopleIcon sx={{ mr: 0.5, fontSize: 20 }} /> },
  '/services': { label: 'Services', icon: <SettingsIcon sx={{ mr: 0.5, fontSize: 20 }} /> },
  '/2fa': { label: '2-Factor Auth' },
  '/users': { label: 'User Management', icon: <PeopleIcon sx={{ mr: 0.5, fontSize: 20 }} /> },
  '/departments': { label: 'Departments', icon: <PeopleIcon sx={{ mr: 0.5, fontSize: 20 }} /> },
  '/profiles': { label: 'Profiles', icon: <PeopleIcon sx={{ mr: 0.5, fontSize: 20 }} /> },
  '/settings': { label: 'Settings', icon: <SettingsIcon sx={{ mr: 0.5, fontSize: 20 }} /> },
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
        <Typography key={currentPath} sx={{ display: 'flex', alignItems: 'center', color: '#625B71', fontWeight: 600 }}>
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
    <Box sx={{ px: 2, py: 2, backgroundColor: '#FFFBFE', borderBottom: '1px solid #E8DEF8' }}>
      <Breadcrumbs
        separator="›"
        sx={{
          '& .MuiBreadcrumbs-separator': {
            color: '#CAC7D0',
            mx: 1,
          },
        }}
      >
        {breadcrumbs}
      </Breadcrumbs>
    </Box>
  );
}

export default BreadcrumbsComponent;
