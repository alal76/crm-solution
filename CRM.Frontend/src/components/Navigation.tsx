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
} from '@mui/icons-material';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useProfile } from '../contexts/ProfileContext';
import logo from '../assets/logo.png';
import './Navigation.css';

function NavigationContent() {
  const { isAuthenticated, user, logout } = useAuth();
  const { profile, canAccessPage, hasPermission } = useProfile();
  const navigate = useNavigate();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);

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
    { label: 'Dashboard', icon: DashboardIcon, path: '/', permission: 'Dashboard' },
    { label: 'Customers', icon: PeopleIcon, path: '/customers', permission: 'Customers' },
    { label: 'Contacts', icon: PeopleIcon, path: '/contacts', permission: 'Contacts' },
    { label: 'Opportunities', icon: TrendingUpIcon, path: '/opportunities', permission: 'Opportunities' },
    { label: 'Products', icon: PackageIcon, path: '/products', permission: 'Products' },
    { label: 'Campaigns', icon: MegaphoneIcon, path: '/campaigns', permission: 'Campaigns' },
    { label: 'Leads', icon: PeopleIcon, path: '/leads', permission: 'Customers' },
    { label: 'Services', icon: SettingsIcon, path: '/services', permission: 'Products' },
  ];

  const adminItems = [
    { label: 'Workflows', icon: AutomationIcon, path: '/workflows' },
    { label: 'Admin Settings', icon: SettingsIcon, path: '/settings' },
  ];

  return (
    <>
      <AppBar position="sticky" sx={{ boxShadow: 1 }}>
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
            <Box sx={{ width: 36, height: 36, mr: 1.5, flexShrink: 0 }}>
              <img src={logo} alt="CRM Logo" style={{ width: '100%', height: '100%', objectFit: 'contain' }} />
            </Box>
            <Typography variant="h6" component={RouterLink} to="/" sx={{ textDecoration: 'none', color: 'inherit', fontWeight: 600 }}>
              CRM System
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <IconButton color="inherit" onClick={handleMenuOpen}>
              <Avatar sx={{ width: 32, height: 32, bgcolor: 'secondary.main', fontSize: '0.9rem' }}>
                {user?.firstName?.charAt(0)?.toUpperCase()}
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
        <Box sx={{ p: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
          <Box sx={{ width: 32, height: 32, flexShrink: 0 }}>
            <img src={logo} alt="CRM Logo" style={{ width: '100%', height: '100%', objectFit: 'contain' }} />
          </Box>
          <Typography variant="h6" sx={{ fontWeight: 600 }}>
            Menu
          </Typography>
        </Box>
        <Divider />
        <List>
          {navItems.map((item) => (
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

        {(user?.role === 'Admin' || user?.role === 0 || user?.role === '0' || hasPermission('canManageUsers')) && (
          <>
            <Divider sx={{ my: 1 }} />
            <Box sx={{ p: 1.5, bgcolor: 'action.hover', m: 1, borderRadius: 1 }}>
              <Typography variant="overline" sx={{ color: 'textSecondary', fontWeight: 600 }}>
                Administration
              </Typography>
            </Box>
            <List>
              {adminItems.map((item) => (
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
