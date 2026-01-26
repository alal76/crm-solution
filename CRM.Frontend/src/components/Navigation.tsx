import React, { useState, useEffect, useMemo } from 'react';
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
  Chip,
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
  Science as ScienceIcon,
  PersonSearch as PersonSearchIcon,
  Forum as CommunicationsIcon,
  SwapHoriz as InteractionsIcon,
  SettingsInputAntenna as ChannelSettingsIcon,
  AccountTree as WorkflowIcon,
  // Admin section icons
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
  // About, Help, Licenses icons
  Info as InfoIcon,
  Help as HelpIcon,
  Gavel as LicenseIcon,
} from '@mui/icons-material';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useProfile } from '../contexts/ProfileContext';
import { useBranding } from '../contexts/BrandingContext';
import { getApiEndpoint } from '../config/ports';
import logo from '../assets/logo.png';
import './Navigation.css';

function NavigationContent() {
  const { isAuthenticated, user, logout } = useAuth();
  const { profile, hasPermission, canAccessMenu } = useProfile();
  const { branding } = useBranding();
  const navigate = useNavigate();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [isDemoMode, setIsDemoMode] = useState(false);

  // Fetch demo mode status
  useEffect(() => {
    const fetchDemoStatus = async () => {
      try {
        const token = localStorage.getItem('accessToken');
        if (!token) return;
        
        const response = await fetch(getApiEndpoint('/systemsettings/demo/status'), {
          headers: { 'Authorization': `Bearer ${token}` }
        });
        if (response.ok) {
          const data = await response.json();
          setIsDemoMode(data.useDemoDatabase);
        }
      } catch (err) {
        console.error('Failed to fetch demo status', err);
      }
    };

    if (isAuthenticated) {
      fetchDemoStatus();
      // Refresh demo status every 30 seconds
      const interval = setInterval(fetchDemoStatus, 30000);
      return () => clearInterval(interval);
    }
  }, [isAuthenticated]);

  // Nav item ID to path/icon mapping (defined outside useMemo for stability)
  const navItemsConfig: Record<string, { label: string; icon: typeof DashboardIcon; path: string; menuName: string }> = useMemo(() => ({
    'dashboard': { label: 'Dashboard', icon: DashboardIcon, path: '/', menuName: 'Dashboard' },
    'customers': { label: 'Accounts', icon: PeopleIcon, path: '/customers', menuName: 'Customers' }, // Industry-standard label
    'customer-overview': { label: 'Account Overview', icon: PersonSearchIcon, path: '/customer-overview', menuName: 'CustomerOverview' },
    'contacts': { label: 'Contacts', icon: PeopleIcon, path: '/contacts', menuName: 'Contacts' },
    'leads': { label: 'Leads', icon: PeopleIcon, path: '/leads', menuName: 'Leads' },
    'opportunities': { label: 'Opportunities', icon: TrendingUpIcon, path: '/opportunities', menuName: 'Opportunities' },
    'products': { label: 'Products', icon: PackageIcon, path: '/products', menuName: 'Products' },
    'services': { label: 'Services', icon: SettingsIcon, path: '/services', menuName: 'Services' },
    'service-requests': { label: 'Service Requests', icon: SupportAgentIcon, path: '/service-requests', menuName: 'ServiceRequests' },
    'campaigns': { label: 'Campaigns', icon: MegaphoneIcon, path: '/campaigns', menuName: 'Campaigns' },
    'quotes': { label: 'Quotes', icon: QuoteIcon, path: '/quotes', menuName: 'Quotes' },
    'my-queue': { label: 'My Queue', icon: TaskIcon, path: '/my-queue', menuName: 'MyQueue' },
    'activities': { label: 'Activities', icon: ActivityIcon, path: '/activities', menuName: 'Activities' },
    'notes': { label: 'Notes', icon: NoteIcon, path: '/notes', menuName: 'Notes' },
    'communications': { label: 'Communications', icon: CommunicationsIcon, path: '/communications', menuName: 'Communications' },
    'interactions': { label: 'Interactions', icon: InteractionsIcon, path: '/interactions', menuName: 'Interactions' },
    // Help & Info items
    'about': { label: 'About', icon: InfoIcon, path: '/about', menuName: 'About' },
    'help': { label: 'Help', icon: HelpIcon, path: '/help', menuName: 'Help' },
    'licenses': { label: 'Licenses', icon: LicenseIcon, path: '/licenses', menuName: 'Licenses' },
  }), []);

  const adminItemsConfig: Record<string, { label: string; icon: typeof DashboardIcon; path: string; menuName: string }> = useMemo(() => ({
    // System Administration
    'database-settings': { label: 'Database', icon: StorageIcon, path: '/admin/database', menuName: 'DatabaseSettings' },
    'deployment-settings': { label: 'Deployment', icon: CloudIcon, path: '/admin/deployment', menuName: 'DeploymentSettings' },
    'monitoring-settings': { label: 'Monitoring', icon: MonitorIcon, path: '/admin/monitoring', menuName: 'MonitoringSettings' },
    'security-settings': { label: 'Security', icon: SecurityIcon, path: '/admin/security', menuName: 'SecuritySettings' },
    'feature-management': { label: 'Features', icon: FeatureToggleIcon, path: '/admin/features', menuName: 'FeatureManagement' },
    // User Administration
    'user-management': { label: 'Users', icon: PeopleIcon, path: '/admin/users', menuName: 'UserManagement' },
    'user-approvals': { label: 'Approvals', icon: PersonAddIcon, path: '/admin/approvals', menuName: 'UserApprovals' },
    'group-management': { label: 'Groups', icon: GroupsIcon, path: '/admin/groups', menuName: 'GroupManagement' },
    'social-login': { label: 'Social Login', icon: LoginIcon, path: '/admin/social-login', menuName: 'SocialLogin' },
    // CRM Administration
    'branding-settings': { label: 'Branding', icon: PaletteIcon, path: '/admin/branding', menuName: 'BrandingSettings' },
    'navigation-settings': { label: 'Navigation', icon: MenuIcon, path: '/admin/navigation', menuName: 'NavigationSettings' },
    'module-fields': { label: 'Modules & Fields', icon: ModuleIcon, path: '/admin/modules', menuName: 'ModuleFields' },
    'sr-definitions': { label: 'Service Requests', icon: SupportAgentIcon, path: '/admin/service-requests', menuName: 'ServiceRequestDefinitions' },
    'master-data': { label: 'Master Data', icon: StorageIcon, path: '/admin/master-data', menuName: 'MasterData' },
    'dashboard-settings': { label: 'Dashboards', icon: DashboardIcon, path: '/admin/dashboards', menuName: 'DashboardSettings' },
    'workflow-settings': { label: 'Workflows', icon: WorkflowIcon, path: '/admin/workflows', menuName: 'WorkflowSettings' },
    // Legacy items
    'channel-settings': { label: 'Channel Settings', icon: ChannelSettingsIcon, path: '/channel-settings', menuName: 'ChannelSettings' },
    'settings': { label: 'All Settings', icon: SettingsIcon, path: '/settings', menuName: 'Settings' },
  }), []);

  // Default order for nav items
  const defaultNavOrder = useMemo(() => [
    'dashboard', 'customers', 'customer-overview', 'contacts', 'leads', 'opportunities',
    'products', 'services', 'service-requests', 'campaigns', 'quotes',
    'my-queue', 'activities', 'notes', 'communications', 'interactions'
  ], []);
  const defaultAdminOrder = useMemo(() => [
    'database-settings', 'deployment-settings', 'monitoring-settings', 'security-settings', 'feature-management',
    'user-management', 'user-approvals', 'group-management', 'social-login',
    'branding-settings', 'navigation-settings', 'module-fields', 'sr-definitions', 'master-data', 'dashboard-settings', 'workflow-settings',
    'channel-settings', 'settings'
  ], []);

  // Default categories
  const defaultCategories = useMemo(() => [
    { id: 'main', label: 'Main', order: 0 },
    { id: 'sales', label: 'Sales & Marketing', order: 1 },
    { id: 'support', label: 'Customer Support', order: 2 },
    { id: 'productivity', label: 'Productivity', order: 3 },
    { id: 'info', label: 'Help & Info', order: 4 },
    { id: 'admin', label: 'Administration', order: 5 },
  ], []);

  // Default nav items with their proper categories (matching NavigationSettingsTab)
  const defaultNavItemsWithCategory = useMemo(() => [
    { id: 'dashboard', order: 0, visible: true, category: 'main' },
    { id: 'customers', order: 1, visible: true, category: 'main' },
    { id: 'customer-overview', order: 2, visible: true, category: 'main' },
    { id: 'contacts', order: 3, visible: true, category: 'main' },
    { id: 'leads', order: 4, visible: true, category: 'sales' },
    { id: 'opportunities', order: 5, visible: true, category: 'sales' },
    { id: 'products', order: 6, visible: true, category: 'sales' },
    { id: 'services', order: 7, visible: true, category: 'support' },
    { id: 'service-requests', order: 8, visible: true, category: 'support' },
    { id: 'campaigns', order: 9, visible: true, category: 'sales' },
    { id: 'quotes', order: 10, visible: true, category: 'sales' },
    { id: 'my-queue', order: 11, visible: true, category: 'productivity' },
    { id: 'activities', order: 12, visible: true, category: 'productivity' },
    { id: 'notes', order: 13, visible: true, category: 'productivity' },
    { id: 'communications', order: 14, visible: true, category: 'productivity' },
    { id: 'interactions', order: 15, visible: true, category: 'productivity' },
    // Help & Info
    { id: 'about', order: 50, visible: true, category: 'info' },
    { id: 'help', order: 51, visible: true, category: 'info' },
    { id: 'licenses', order: 52, visible: true, category: 'info' },
    // System Administration
    { id: 'database-settings', order: 60, visible: true, category: 'admin' },
    { id: 'deployment-settings', order: 61, visible: true, category: 'admin' },
    { id: 'monitoring-settings', order: 62, visible: true, category: 'admin' },
    { id: 'security-settings', order: 63, visible: true, category: 'admin' },
    { id: 'feature-management', order: 64, visible: true, category: 'admin' },
    // User Administration
    { id: 'user-management', order: 65, visible: true, category: 'admin' },
    { id: 'user-approvals', order: 66, visible: true, category: 'admin' },
    { id: 'group-management', order: 67, visible: true, category: 'admin' },
    { id: 'social-login', order: 68, visible: true, category: 'admin' },
    // CRM Administration
    { id: 'branding-settings', order: 69, visible: true, category: 'admin' },
    { id: 'navigation-settings', order: 70, visible: true, category: 'admin' },
    { id: 'module-fields', order: 71, visible: true, category: 'admin' },
    { id: 'sr-definitions', order: 72, visible: true, category: 'admin' },
    { id: 'master-data', order: 73, visible: true, category: 'admin' },
    { id: 'dashboard-settings', order: 74, visible: true, category: 'admin' },
    { id: 'workflow-settings', order: 75, visible: true, category: 'admin' },
    // Legacy
    { id: 'channel-settings', order: 76, visible: true, category: 'admin' },
    { id: 'settings', order: 77, visible: true, category: 'admin' },
  ], []);

  // Get nav config from localStorage or use defaults
  const navConfig = useMemo(() => {
    try {
      const savedConfig = localStorage.getItem('crm_nav_order');
      if (savedConfig) {
        const parsed = JSON.parse(savedConfig);
        // Support both old format (array) and new format (object with navItems and categories)
        if (Array.isArray(parsed)) {
          return { navItems: parsed, categories: defaultCategories };
        }
        return {
          navItems: parsed.navItems || [],
          categories: parsed.categories || defaultCategories
        };
      }
    } catch {
      // Use defaults
    }
    return null;
  }, [defaultCategories]);

  // Build ordered nav items with category info
  const navItemsWithCategory = useMemo(() => {
    const order = navConfig?.navItems || defaultNavItemsWithCategory;
    return order
      .filter((item: { id: string; visible: boolean }) => item.visible && (navItemsConfig[item.id] || adminItemsConfig[item.id]))
      .sort((a: { order: number }, b: { order: number }) => a.order - b.order)
      .map((item: { id: string; customLabel?: string; category?: string }) => ({
        ...navItemsConfig[item.id] || adminItemsConfig[item.id],
        customLabel: item.customLabel,
        category: item.category || 'main',
        id: item.id
      }));
  }, [navConfig, defaultNavItemsWithCategory, navItemsConfig, adminItemsConfig]);

  // Get categories from config
  const categories = useMemo(() => {
    return (navConfig?.categories || defaultCategories).sort((a: { order: number }, b: { order: number }) => a.order - b.order);
  }, [navConfig, defaultCategories]);

  // Build ordered nav items (legacy - used for simple list)
  const navItems = useMemo(() => {
    const order = navConfig?.navItems || defaultNavItemsWithCategory;
    return order
      .filter((item: { id: string; visible: boolean }) => item.visible && navItemsConfig[item.id])
      .sort((a: { order: number }, b: { order: number }) => a.order - b.order)
      .map((item: { id: string; customLabel?: string }) => ({
        ...navItemsConfig[item.id],
        customLabel: item.customLabel
      }));
  }, [navConfig, defaultNavItemsWithCategory, navItemsConfig]);

  const adminItems = useMemo(() => {
    const order = navConfig?.navItems?.filter((item: { id: string }) => adminItemsConfig[item.id]) || 
      defaultNavItemsWithCategory.filter(item => adminItemsConfig[item.id]);
    return order
      .filter((item: { id: string; visible: boolean }) => item.visible && adminItemsConfig[item.id])
      .sort((a: { order: number }, b: { order: number }) => a.order - b.order)
      .map((item: { id: string; customLabel?: string }) => ({
        ...adminItemsConfig[item.id],
        customLabel: item.customLabel
      }));
  }, [navConfig, defaultNavItemsWithCategory, adminItemsConfig]);

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

  // Get API base URL for uploads
  const getApiBaseUrl = () => {
    return window.location.hostname === 'localhost'
      ? 'http://localhost:5000'
      : `http://${window.location.hostname}:5000`;
  };

  // Get logo URL: from branding settings or default
  const getLogoUrl = () => {
    if (branding.companyLogoUrl) {
      // If it's a data URL (base64), use it directly
      if (branding.companyLogoUrl.startsWith('data:')) {
        return branding.companyLogoUrl;
      }
      // If it's a relative URL starting with /uploads, prepend API base URL
      if (branding.companyLogoUrl.startsWith('/uploads')) {
        return `${getApiBaseUrl()}${branding.companyLogoUrl}`;
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
            {isDemoMode && (
              <Chip
                icon={<ScienceIcon />}
                label="DEMO"
                size="small"
                sx={{
                  ml: 2,
                  backgroundColor: '#ff9800',
                  color: 'white',
                  fontWeight: 'bold',
                  '& .MuiChip-icon': { color: 'white' }
                }}
              />
            )}
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
        
        {/* Render items grouped by category */}
        {categories.map((category: { id: string; label: string; order: number }, catIdx: number) => {
          // Skip admin category for non-admin users
          if (category.id === 'admin' && !(user?.role === 'Admin' || user?.role === 0 || user?.role === '0' || hasPermission('canManageUsers'))) {
            return null;
          }
          
          const categoryItems = navItemsWithCategory.filter(
            (item: { category?: string; menuName: string }) => 
              item.category === category.id && canAccessMenu(item.menuName)
          );
          
          if (categoryItems.length === 0) return null;
          
          return (
            <React.Fragment key={category.id}>
              {catIdx > 0 && <Divider sx={{ my: 0.5 }} />}
              <Box sx={{ px: 2, py: 1, bgcolor: category.id === 'admin' ? 'warning.light' : 'action.hover' }}>
                <Typography variant="overline" sx={{ color: category.id === 'admin' ? 'warning.dark' : 'text.secondary', fontWeight: 600, fontSize: '0.65rem' }}>
                  {category.label}
                </Typography>
              </Box>
              <List dense sx={{ py: 0 }}>
                {categoryItems.map((item: { id: string; path: string; icon: typeof DashboardIcon; label: string; customLabel?: string }) => (
                  <ListItem
                    button
                    key={item.id || item.path}
                    component={RouterLink}
                    to={item.path}
                    onClick={() => setDrawerOpen(false)}
                    sx={{
                      py: 0.75,
                      '&:hover': {
                        backgroundColor: 'action.hover',
                      },
                    }}
                  >
                    <ListItemIcon sx={{ minWidth: 36 }}>
                      <item.icon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText primary={item.customLabel || item.label} primaryTypographyProps={{ fontSize: '0.9rem' }} />
                  </ListItem>
                ))}
              </List>
            </React.Fragment>
          );
        })}
      </Drawer>
    </>
  );
}

function Navigation() {
  const { isAuthenticated } = useAuth();

  return isAuthenticated ? <NavigationContent /> : null;
}

export default Navigation;
