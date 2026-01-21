import { useState } from 'react';
import {
  AppBar,
  Toolbar,
  IconButton,
  Menu,
  MenuItem,
  Box,
  Avatar,
  Drawer,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Divider,
  Typography,
} from '@mui/material';
import {
  Dashboard as DashboardIcon,
  People as PeopleIcon,
  TrendingUp as TrendingUpIcon,
  Inventory2 as PackageIcon,
  Campaign as MegaphoneIcon,
  Settings as SettingsIcon,
  Menu as MenuIcon,
  Logout as LogoutIcon,
  AccountCircle as AccountCircleIcon,
  Lock as LockIcon,
  VpnKey as VpnKeyIcon,
  AutoAwesome as AutomationIcon,
  Assignment as TaskIcon,
  Description as QuoteIcon,
  Note as NoteIcon,
  Timeline as ActivityIcon,
  Business as BusinessIcon,
  SupportAgent as SupportAgentIcon,
} from '@mui/icons-material';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useProfile } from '../contexts/ProfileContext';
import { useBranding } from '../contexts/BrandingContext';
import logo from '../assets/logo.png';
import './Navigation.css';

function NavigationContent() {
  const { isAuthenticated, user, logout } = useAuth();
  const { profile, hasPermission, canAccessMenu } = useProfile();
  const { branding } = useBranding();
  const navigate = useNavigate();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);

  // Get header color: user's custom color, or red for admin, or primary color
  const getHeaderColor = () => {
    if (user?.headerColor) return user.headerColor;
    if (user?.role === 'Admin' || user?.role === 0 || user?.role === '0') return '#C62828';
    return branding.primaryColor || '#6750A4';
  };

  // Get user initials: first char of firstName + first char of lastName
  const getUserInitials = () => {
    const firstInitial = user?.firstName?.charAt(0)?.toUpperCase() || '';
    const lastInitial = user?.lastName?.charAt(0)?.toUpperCase() || '';
    return `${firstInitial}${lastInitial}` || 'U';
  };

  // Get logo URL: from branding settings or default
  const getLogoUrl = () => {
    if (branding.companyLogoUrl) {
      // If it's a relative URL, it's from our uploads
      if (branding.companyLogoUrl.startsWith('/uploads')) {
        return branding.companyLogoUrl;
      }
      return branding.companyLogoUrl;
    }
    return logo;
  };

  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleLogout = () => {
    logout();
    handleMenuClose();
    navigate('/login');
  };

  if (!isAuthenticated) {
    return null;
  }

  const navItems = [
    { label: 'Dashboard', icon: DashboardIcon, path: '/', menuName: 'Dashboard' },
    { label: 'Customers', icon: PeopleIcon, path: '/customers', menuName: 'Customers' },
    { label: 'Contacts', icon: PeopleIcon, path: '/contacts', menuName: 'Contacts' },
    { label: 'Leads', icon: PeopleIcon, path: '/leads', menuName: 'Leads' },
    { label: 'Opportunities', icon: TrendingUpIcon, path: '/opportunities', menuName: 'Opportunities' },
    { label: 'Products', icon: PackageIcon, path: '/products', menuName: 'Products' },
    { label: 'Services', icon: SettingsIcon, path: '/services', menuName: 'Services' },
    { label: 'Service Requests', icon: SupportAgentIcon, path: '/service-requests', menuName: 'ServiceRequests' },
    { label: 'Campaigns', icon: MegaphoneIcon, path: '/campaigns', menuName: 'Campaigns' },
    { label: 'Quotes', icon: QuoteIcon, path: '/quotes', menuName: 'Quotes' },
    { label: 'Tasks', icon: TaskIcon, path: '/tasks', menuName: 'Tasks' },
    { label: 'Activities', icon: ActivityIcon, path: '/activities', menuName: 'Activities' },
    { label: 'Notes', icon: NoteIcon, path: '/notes', menuName: 'Notes' },
  ];

  const adminItems = [
    { label: 'Workflows', icon: AutomationIcon, path: '/workflows', menuName: 'Workflows' },
    { label: 'Service Request Settings', icon: SupportAgentIcon, path: '/service-request-settings', menuName: 'Settings' },
    { label: 'Admin Settings', icon: SettingsIcon, path: '/settings', menuName: 'Settings' },
  ];
  
  // Filter nav items based on group permissions and module status
  const visibleNavItems = navItems.filter(item => canAccessMenu(item.menuName));
  const visibleAdminItems = adminItems.filter(item => canAccessMenu(item.menuName));

  return (
    <>
      <AppBar position="sticky" sx={{ boxShadow: 1, backgroundColor: getHeaderColor() }}>
        <Toolbar>
          <Box sx={{ display: 'flex', alignItems: 'center', flex: 1 }}>
            <IconButton
              color="inherit"
              aria-label="open drawer"
              edge="start"
              onClick={() => setDrawerOpen(true)}
              sx={{ mr: 2 }}
            >
              <MenuIcon />
            </IconButton>
            <Box sx={{ width: 36, height: 36, mr: 1.5, flexShrink: 0, backgroundColor: 'white', borderRadius: 1, p: 0.25 }}>
              {branding.companyLogoUrl ? (
                <img src={getLogoUrl()} alt="Company Logo" style={{ width: '100%', height: '100%', objectFit: 'contain' }} />
              ) : (
                <BusinessIcon sx={{ width: '100%', height: '100%', color: getHeaderColor() }} />
              )}
            </Box>
            <Typography variant="h6" component={RouterLink} to="/" sx={{ textDecoration: 'none', color: 'inherit', fontWeight: 600 }}>
              {branding.companyName || 'CRM System'}
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <IconButton color="inherit" onClick={handleMenuOpen}>
              <Avatar 
                src={user?.photoUrl || undefined}
                sx={{ 
                  width: 36, 
                  height: 36, 
                  bgcolor: 'rgba(255,255,255,0.2)', 
                  fontSize: '0.85rem',
                  fontWeight: 600,
                  border: '2px solid rgba(255,255,255,0.5)'
                }}
              >
                {getUserInitials()}
              </Avatar>
            </IconButton>
            <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={handleMenuClose}>
              <MenuItem disabled>
                <Box>
                  <Typography variant="body2" sx={{ fontWeight: 500 }}>
                    {user?.firstName} {user?.lastName}
                  </Typography>
                  <Typography variant="caption" color="textSecondary">
                    {user?.email}
                  </Typography>
                </Box>
              </MenuItem>
              {profile && (
                <>
                  <MenuItem disabled>
                    <Box>
                      <Typography variant="caption" color="textSecondary">
                        Department: {profile.departmentName || 'N/A'}
                      </Typography>
                    </Box>
                  </MenuItem>
                </>
              )}
              <Divider />
              <MenuItem component={RouterLink} to="/2fa" onClick={handleMenuClose}>
                <LockIcon sx={{ mr: 1, fontSize: '1.2rem' }} />
                Two-Factor Auth
              </MenuItem>
              <MenuItem component={RouterLink} to="/password-reset" onClick={handleMenuClose}>
                <VpnKeyIcon sx={{ mr: 1, fontSize: '1.2rem' }} />
                Change Password
              </MenuItem>
              <MenuItem component={RouterLink} to="/settings" onClick={handleMenuClose}>
                <AccountCircleIcon sx={{ mr: 1, fontSize: '1.2rem' }} />
                Settings
              </MenuItem>
              <Divider />
              <MenuItem onClick={handleLogout}>
                <LogoutIcon sx={{ mr: 1, fontSize: '1.2rem' }} />
                Logout
              </MenuItem>
            </Menu>
          </Box>
        </Toolbar>
      </AppBar>

      <Drawer
        anchor="left"
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        sx={{
          '& .MuiDrawer-paper': {
            width: 280,
            boxSizing: 'border-box',
          },
        }}
      >
        <Box sx={{ p: 2, display: 'flex', alignItems: 'center', gap: 1.5, bgcolor: getHeaderColor(), color: 'white' }}>
          <Box sx={{ width: 36, height: 36, flexShrink: 0, backgroundColor: 'white', borderRadius: 1, p: 0.25 }}>
            {branding.companyLogoUrl ? (
              <img src={getLogoUrl()} alt="Company Logo" style={{ width: '100%', height: '100%', objectFit: 'contain' }} />
            ) : (
              <BusinessIcon sx={{ width: '100%', height: '100%', color: getHeaderColor() }} />
            )}
          </Box>
          <Typography variant="h6" sx={{ fontWeight: 600 }}>
            {branding.companyName || 'CRM System'}
          </Typography>
        </Box>
        <Divider />
        <List>
          {visibleNavItems.map((item) => (
            <ListItem
              button
              key={item.path}
              component={RouterLink}
              to={item.path}
              onClick={() => setDrawerOpen(false)}
              sx={{
                '&:hover': {
                  backgroundColor: 'action.hover',
                },
              }}
            >
              <ListItemIcon>
                <item.icon />
              </ListItemIcon>
              <ListItemText primary={item.label} />
            </ListItem>
          ))}
        </List>

        {visibleAdminItems.length > 0 && (user?.role === 'Admin' || user?.role === 0 || user?.role === '0' || hasPermission('canManageUsers')) && (
          <>
            <Divider sx={{ my: 1 }} />
            <Box sx={{ p: 1.5, bgcolor: 'action.hover', m: 1, borderRadius: 1 }}>
              <Typography variant="overline" sx={{ color: 'textSecondary', fontWeight: 600 }}>
                Administration
              </Typography>
            </Box>
            <List>
              {visibleAdminItems.map((item) => (
                <ListItem
                  button
                  key={item.path}
                  component={RouterLink}
                  to={item.path}
                  onClick={() => setDrawerOpen(false)}
                  sx={{
                    '&:hover': {
                      backgroundColor: 'secondary.light',
                      color: 'secondary.dark',
                    },
                  }}
                >
                  <ListItemIcon sx={{ color: 'secondary.main' }}>
                    <item.icon />
                  </ListItemIcon>
                  <ListItemText primary={item.label} sx={{ '& .MuiTypography-root': { fontWeight: 500 } }} />
                </ListItem>
              ))}
            </List>
          </>
        )}
      </Drawer>
    </>
  );
}

function Navigation() {
  const { isAuthenticated } = useAuth();

  return isAuthenticated ? <NavigationContent /> : null;
}

export default Navigation;
