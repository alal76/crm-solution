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
  Collapse,
  ListItemButton,
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
  PersonSearch as PersonSearchIcon,
  Forum as CommunicationsIcon,
  SwapHoriz as InteractionsIcon,
  SettingsInputAntenna as ChannelSettingsIcon,
  AccountTree as WorkflowIcon,
  BugReport as TestResultsIcon,
  Psychology as LLMIcon,
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
  // Expand/Collapse icons
  ExpandLess,
  ExpandMore,
  // Admin subcategory icons
  AdminPanelSettings as SystemAdminIcon,
  ManageAccounts as UserAdminIcon,
  Store as CRMAdminIcon,
  Build as ServiceReqIcon,
  Navigation as NavAdminIcon,
  ViewQuilt as ModulesIcon,
  DashboardCustomize as DashboardAdminIcon,
  Podcasts as ChannelAdminIcon,
  FolderSpecial as ViewQuiltIcon,
} from '@mui/icons-material';
import { Link as RouterLink, useNavigate, useLocation } from 'react-router-dom';
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
  const location = useLocation();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);
  // Demo mode feature removed - using production database only
  const [navRefreshKey, setNavRefreshKey] = useState(0); // Force re-render on nav update
  
  // Collapsible categories state
  const [expandedCategories, setExpandedCategories] = useState<Record<string, boolean>>({
    'main': true,
    'sales': true,
    'support': true,
    'productivity': true,
    'info': false,
    'admin': true,
  });

  // Toggle category expansion
  const toggleCategory = (categoryId: string) => {
    setExpandedCategories(prev => ({
      ...prev,
      [categoryId]: !prev[categoryId],
    }));
  };

  // Collapsible admin subcategories state
  const [expandedAdminSections, setExpandedAdminSections] = useState<Record<string, boolean>>({
    'admin-system': false,
    'admin-users': false,
    'admin-crm': false,
    'admin-service': false,
    'admin-navigation': false,
    'admin-modules': false,
    'admin-workflows': false,
    'admin-channels': false,
  });

  // Toggle admin subcategory expansion
  const toggleAdminSection = (sectionId: string) => {
    setExpandedAdminSections(prev => ({
      ...prev,
      [sectionId]: !prev[sectionId],
    }));
  };

  // Auto-expand category and section if current route matches
  useEffect(() => {
    const path = location.pathname;
    
    // Auto-expand main categories based on route
    if (path === '/' || path === '/dashboard') {
      setExpandedCategories(prev => ({ ...prev, 'main': true }));
    } else if (path.includes('/customers') || path.includes('/contacts')) {
      setExpandedCategories(prev => ({ ...prev, 'main': true }));
    } else if (path.includes('/leads') || path.includes('/opportunities') || path.includes('/products') || path.includes('/campaigns') || path.includes('/quotes')) {
      setExpandedCategories(prev => ({ ...prev, 'sales': true }));
    } else if (path.includes('/services') || path.includes('/service-requests')) {
      setExpandedCategories(prev => ({ ...prev, 'support': true }));
    } else if (path.includes('/queue') || path.includes('/activities') || path.includes('/notes') || path.includes('/communications') || path.includes('/interactions')) {
      setExpandedCategories(prev => ({ ...prev, 'productivity': true }));
    } else if (path.includes('/about') || path.includes('/help') || path.includes('/licenses')) {
      setExpandedCategories(prev => ({ ...prev, 'info': true }));
    }
    
    // Auto-expand admin sections
    if (path.startsWith('/admin/')) {
      setExpandedCategories(prev => ({ ...prev, 'admin': true }));
      // Determine which section to expand based on path
      if (path.includes('database') || path.includes('deployment') || path.includes('monitoring') || path.includes('security') || path.includes('features')) {
        setExpandedAdminSections(prev => ({ ...prev, 'admin-system': true }));
      } else if (path.includes('users') || path.includes('approvals') || path.includes('groups') || path.includes('social-login')) {
        setExpandedAdminSections(prev => ({ ...prev, 'admin-users': true }));
      } else if (path.includes('branding') || path.includes('master-data')) {
        setExpandedAdminSections(prev => ({ ...prev, 'admin-crm': true }));
      } else if (path.includes('service-requests')) {
        setExpandedAdminSections(prev => ({ ...prev, 'admin-service': true }));
      } else if (path.includes('navigation')) {
        setExpandedAdminSections(prev => ({ ...prev, 'admin-navigation': true }));
      } else if (path.includes('modules')) {
        setExpandedAdminSections(prev => ({ ...prev, 'admin-modules': true }));
      } else if (path.includes('workflows') || path.includes('dashboards')) {
        setExpandedAdminSections(prev => ({ ...prev, 'admin-workflows': true }));
      }
    }
    if (path.includes('channel')) {
      setExpandedCategories(prev => ({ ...prev, 'admin': true }));
      setExpandedAdminSections(prev => ({ ...prev, 'admin-channels': true }));
    }
  }, [location.pathname]);

  // Listen for navigation and branding updates to refresh drawer
  useEffect(() => {
    const handleNavUpdate = () => {
      setNavRefreshKey(k => k + 1);
    };
    const handleBrandingUpdate = () => {
      // Branding is already handled by context, but force refresh key for edge cases
      setNavRefreshKey(k => k + 1);
    };
    window.addEventListener('navigationUpdated', handleNavUpdate);
    window.addEventListener('brandingUpdated', handleBrandingUpdate);
    return () => {
      window.removeEventListener('navigationUpdated', handleNavUpdate);
      window.removeEventListener('brandingUpdated', handleBrandingUpdate);
    };
  }, []);

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
    'test-results': { label: 'Test Results', icon: TestResultsIcon, path: '/admin/test-results', menuName: 'TestResults' },
    'llm-settings': { label: 'AI / LLM Settings', icon: LLMIcon, path: '/admin/llm', menuName: 'LLMSettings' },
    // Legacy items
    'channel-settings': { label: 'Channel Settings', icon: ChannelSettingsIcon, path: '/channel-settings', menuName: 'ChannelSettings' },
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

  // Default admin subcategories with icons for collapsible sections
  const defaultAdminSubcategories = useMemo(() => [
    { id: 'admin-system', label: 'System Settings', icon: 'SystemAdminIcon', order: 0 },
    { id: 'admin-users', label: 'User & Group Settings', icon: 'UserAdminIcon', order: 1 },
    { id: 'admin-crm', label: 'CRM Settings', icon: 'CRMAdminIcon', order: 2 },
    { id: 'admin-service', label: 'Service Request Setup', icon: 'ServiceReqIcon', order: 3 },
    { id: 'admin-navigation', label: 'Navigation', icon: 'NavAdminIcon', order: 4 },
    { id: 'admin-modules', label: 'Modules & Fields', icon: 'ModulesIcon', order: 5 },
    { id: 'admin-workflows', label: 'Workflows & Dashboards', icon: 'DashboardAdminIcon', order: 6 },
    { id: 'admin-channels', label: 'Channels', icon: 'ChannelAdminIcon', order: 7 },
  ], []);

  // Icon map for admin subcategories
  const adminSubcategoryIconMap: Record<string, React.ElementType> = useMemo(() => ({
    SystemAdminIcon,
    UserAdminIcon,
    CRMAdminIcon,
    ServiceReqIcon,
    NavAdminIcon,
    ModulesIcon,
    DashboardAdminIcon,
    ChannelAdminIcon,
    SettingsIcon,
    StorageIcon,
    SecurityIcon,
    SubcategoryIcon: ViewQuiltIcon,
  }), []);

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
    { id: 'database-settings', order: 60, visible: true, category: 'admin', adminSubcategory: 'admin-system' },
    { id: 'deployment-settings', order: 61, visible: true, category: 'admin', adminSubcategory: 'admin-system' },
    { id: 'monitoring-settings', order: 62, visible: true, category: 'admin', adminSubcategory: 'admin-system' },
    { id: 'security-settings', order: 63, visible: true, category: 'admin', adminSubcategory: 'admin-system' },
    { id: 'feature-management', order: 64, visible: true, category: 'admin', adminSubcategory: 'admin-system' },
    // User Administration
    { id: 'user-management', order: 65, visible: true, category: 'admin', adminSubcategory: 'admin-users' },
    { id: 'user-approvals', order: 66, visible: true, category: 'admin', adminSubcategory: 'admin-users' },
    { id: 'group-management', order: 67, visible: true, category: 'admin', adminSubcategory: 'admin-users' },
    { id: 'social-login', order: 68, visible: true, category: 'admin', adminSubcategory: 'admin-users' },
    // CRM Administration
    { id: 'branding-settings', order: 69, visible: true, category: 'admin', adminSubcategory: 'admin-crm' },
    { id: 'navigation-settings', order: 70, visible: true, category: 'admin', adminSubcategory: 'admin-navigation' },
    { id: 'module-fields', order: 71, visible: true, category: 'admin', adminSubcategory: 'admin-modules' },
    { id: 'sr-definitions', order: 72, visible: true, category: 'admin', adminSubcategory: 'admin-service' },
    { id: 'master-data', order: 73, visible: true, category: 'admin', adminSubcategory: 'admin-crm' },
    { id: 'dashboard-settings', order: 74, visible: true, category: 'admin', adminSubcategory: 'admin-workflows' },
    { id: 'workflow-settings', order: 75, visible: true, category: 'admin', adminSubcategory: 'admin-workflows' },
    // Channels
    { id: 'channel-settings', order: 76, visible: true, category: 'admin', adminSubcategory: 'admin-channels' },
    // Test Results
    { id: 'test-results', order: 77, visible: true, category: 'admin', adminSubcategory: 'admin-system' },
    // AI / LLM
    { id: 'llm-settings', order: 78, visible: true, category: 'admin', adminSubcategory: 'admin-workflows' },
  ], []);

  // Get nav config from localStorage or use defaults
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const navConfig = useMemo(() => {
    try {
      const savedConfig = localStorage.getItem('crm_nav_order');
      if (savedConfig) {
        const parsed = JSON.parse(savedConfig);
        // Support both old format (array) and new format (object with navItems, categories, and adminSubcategories)
        if (Array.isArray(parsed)) {
          return { navItems: parsed, categories: defaultCategories, adminSubcategories: defaultAdminSubcategories };
        }
        return {
          navItems: parsed.navItems || [],
          categories: parsed.categories || defaultCategories,
          adminSubcategories: parsed.adminSubcategories || defaultAdminSubcategories
        };
      }
    } catch {
      // Use defaults
    }
    return null;
  }, [defaultCategories, defaultAdminSubcategories, navRefreshKey]); // Include navRefreshKey to force recalculation on nav update

  // Get admin subcategories from config
  const adminSubcategories = useMemo(() => {
    const subcats = navConfig?.adminSubcategories || defaultAdminSubcategories;
    return subcats.sort((a: { order: number }, b: { order: number }) => a.order - b.order);
  }, [navConfig, defaultAdminSubcategories]);

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
            width: 260,
            boxSizing: 'border-box',
            display: 'flex',
            flexDirection: 'column',
          },
        }}
      >
        {/* Fixed Header */}
        <Box sx={{ 
          p: 1.5, 
          display: 'flex', 
          alignItems: 'center', 
          gap: 1.5, 
          bgcolor: getHeaderColor(), 
          color: 'white',
          flexShrink: 0,
        }}>
          <Box sx={{ width: 32, height: 32, flexShrink: 0, backgroundColor: 'white', borderRadius: 1, p: 0.25 }}>
            {branding.companyLogoUrl ? (
              <img src={getLogoUrl()} alt="Company Logo" style={{ width: '100%', height: '100%', objectFit: 'contain' }} />
            ) : (
              <BusinessIcon sx={{ width: '100%', height: '100%', color: getHeaderColor() }} />
            )}
          </Box>
          <Typography variant="subtitle1" sx={{ fontWeight: 600, flex: 1 }}>
            {branding.companyName || 'CRM System'}
          </Typography>
        </Box>
        
        {/* Scrollable Content */}
        <Box sx={{ flex: 1, overflow: 'auto' }}>
        
        {/* Render items grouped by category */}
        {categories.map((category: { id: string; label: string; order: number }, catIdx: number) => {
          // Skip admin category for non-admin users
          if (category.id === 'admin' && !(user?.role === 'Admin' || user?.role === 0 || user?.role === '0' || hasPermission('canManageUsers'))) {
            return null;
          }
          
          // For admin category, render collapsible subcategories
          if (category.id === 'admin') {
            const isAdminExpanded = expandedCategories['admin'] ?? true;
            return (
              <React.Fragment key={category.id}>
                {catIdx > 0 && <Divider sx={{ my: 0.5 }} />}
                <ListItemButton
                  onClick={() => toggleCategory('admin')}
                  sx={{ 
                    py: 0.5, 
                    px: 1.5,
                    bgcolor: 'warning.light',
                    '&:hover': { bgcolor: 'warning.200' },
                    minHeight: 36,
                  }}
                >
                  <ListItemIcon sx={{ minWidth: 28 }}>
                    <SettingsIcon fontSize="small" sx={{ color: 'warning.dark' }} />
                  </ListItemIcon>
                  <ListItemText 
                    primary={category.label}
                    primaryTypographyProps={{ 
                      sx: { color: 'warning.dark', fontWeight: 600, fontSize: '0.75rem', textTransform: 'uppercase', letterSpacing: 0.5 } 
                    }}
                  />
                  {isAdminExpanded ? <ExpandLess fontSize="small" sx={{ color: 'warning.dark' }} /> : <ExpandMore fontSize="small" sx={{ color: 'warning.dark' }} />}
                </ListItemButton>
                <Collapse in={isAdminExpanded} timeout="auto" unmountOnExit>
                <List dense sx={{ py: 0 }}>
                  {adminSubcategories.map((subcat: { id: string; label: string; icon: string; order: number }) => {
                    // Get items for this subcategory from navConfig
                    const subcatItems = (navConfig?.navItems || defaultNavItemsWithCategory)
                      .filter((item: { id: string; visible: boolean; category?: string; adminSubcategory?: string }) => 
                        item.visible && 
                        item.category === 'admin' && 
                        item.adminSubcategory === subcat.id && 
                        adminItemsConfig[item.id]
                      )
                      .map((item: { id: string; customLabel?: string }) => ({
                        ...adminItemsConfig[item.id],
                        id: item.id,
                        customLabel: item.customLabel,
                      }))
                      .filter((item: { menuName: string }) => canAccessMenu(item.menuName));
                    
                    if (subcatItems.length === 0) return null;
                    
                    const SubcatIcon = adminSubcategoryIconMap[subcat.icon] || SettingsIcon;
                    const isExpanded = expandedAdminSections[subcat.id];
                    
                    return (
                      <React.Fragment key={subcat.id}>
                        <ListItemButton
                          onClick={() => toggleAdminSection(subcat.id)}
                          sx={{
                            py: 0.25,
                            pl: 2,
                            minHeight: 32,
                            bgcolor: isExpanded ? 'grey.100' : 'transparent',
                            '&:hover': { bgcolor: 'grey.100' },
                          }}
                        >
                          <ListItemIcon sx={{ minWidth: 24 }}>
                            <SubcatIcon sx={{ fontSize: '1rem', color: 'text.secondary' }} />
                          </ListItemIcon>
                          <ListItemText 
                            primary={subcat.label} 
                            primaryTypographyProps={{ fontSize: '0.8rem', fontWeight: 500, color: 'text.primary' }} 
                          />
                          {isExpanded ? <ExpandLess sx={{ fontSize: '1rem' }} /> : <ExpandMore sx={{ fontSize: '1rem' }} />}
                        </ListItemButton>
                        <Collapse in={isExpanded} timeout="auto" unmountOnExit>
                          <List component="div" disablePadding dense sx={{ bgcolor: 'grey.50' }}>
                            {subcatItems.map((item: { id: string; path: string; icon: React.ElementType; label: string; customLabel?: string }) => (
                              <ListItemButton
                                key={item.id}
                                component={RouterLink}
                                to={item.path}
                                onClick={() => setDrawerOpen(false)}
                                sx={{
                                  pl: 5,
                                  py: 0.25,
                                  minHeight: 28,
                                  bgcolor: location.pathname === item.path ? 'primary.light' : 'transparent',
                                  '&:hover': { bgcolor: 'grey.200' },
                                  borderLeft: location.pathname === item.path ? '3px solid' : '3px solid transparent',
                                  borderLeftColor: location.pathname === item.path ? 'primary.main' : 'transparent',
                                }}
                              >
                                <ListItemIcon sx={{ minWidth: 22 }}>
                                  <item.icon sx={{ fontSize: '0.9rem', color: location.pathname === item.path ? 'primary.main' : 'text.secondary' }} />
                                </ListItemIcon>
                                <ListItemText 
                                  primary={item.customLabel || item.label} 
                                  primaryTypographyProps={{ 
                                    fontSize: '0.75rem', 
                                    color: location.pathname === item.path ? 'primary.main' : 'text.primary',
                                    fontWeight: location.pathname === item.path ? 600 : 400,
                                  }} 
                                />
                              </ListItemButton>
                            ))}
                          </List>
                        </Collapse>
                      </React.Fragment>
                    );
                  })}
                </List>
                </Collapse>
              </React.Fragment>
            );
          }
          
          const categoryItems = navItemsWithCategory.filter(
            (item: { category?: string; menuName: string }) => 
              item.category === category.id && canAccessMenu(item.menuName)
          );
          
          if (categoryItems.length === 0) return null;
          
          const isCategoryExpanded = expandedCategories[category.id] ?? true;
          
          return (
            <React.Fragment key={category.id}>
              {catIdx > 0 && <Divider sx={{ my: 0.5 }} />}
              <ListItemButton
                onClick={() => toggleCategory(category.id)}
                sx={{ 
                  py: 0.5, 
                  px: 1.5,
                  bgcolor: 'grey.100',
                  '&:hover': { bgcolor: 'grey.200' },
                  minHeight: 36,
                }}
              >
                <ListItemIcon sx={{ minWidth: 28 }}>
                  {category.id === 'main' && <DashboardIcon fontSize="small" sx={{ color: 'primary.main' }} />}
                  {category.id === 'sales' && <TrendingUpIcon fontSize="small" sx={{ color: 'success.main' }} />}
                  {category.id === 'support' && <SupportAgentIcon fontSize="small" sx={{ color: 'info.main' }} />}
                  {category.id === 'productivity' && <TaskIcon fontSize="small" sx={{ color: 'secondary.main' }} />}
                  {category.id === 'info' && <InfoIcon fontSize="small" sx={{ color: 'text.secondary' }} />}
                </ListItemIcon>
                <ListItemText 
                  primary={category.label}
                  primaryTypographyProps={{ 
                    sx: { color: 'text.primary', fontWeight: 600, fontSize: '0.75rem', textTransform: 'uppercase', letterSpacing: 0.5 } 
                  }}
                />
                {isCategoryExpanded ? <ExpandLess fontSize="small" sx={{ color: 'text.secondary' }} /> : <ExpandMore fontSize="small" sx={{ color: 'text.secondary' }} />}
              </ListItemButton>
              <Collapse in={isCategoryExpanded} timeout={200} unmountOnExit>
                <List dense sx={{ py: 0, bgcolor: 'background.paper' }}>
                  {categoryItems.map((item: { id: string; path: string; icon: typeof DashboardIcon; label: string; customLabel?: string }) => (
                    <ListItemButton
                      key={item.id || item.path}
                      component={RouterLink}
                      to={item.path}
                      onClick={() => setDrawerOpen(false)}
                      sx={{
                        py: 0.35,
                        pl: 5,
                        minHeight: 32,
                        bgcolor: location.pathname === item.path ? 'primary.light' : 'transparent',
                        '&:hover': { bgcolor: 'grey.100' },
                        borderLeft: location.pathname === item.path ? '3px solid' : '3px solid transparent',
                        borderLeftColor: location.pathname === item.path ? 'primary.main' : 'transparent',
                      }}
                    >
                      <ListItemIcon sx={{ minWidth: 24 }}>
                        <item.icon sx={{ fontSize: '1rem', color: location.pathname === item.path ? 'primary.main' : 'text.secondary' }} />
                      </ListItemIcon>
                      <ListItemText 
                        primary={item.customLabel || item.label} 
                        primaryTypographyProps={{ 
                          fontSize: '0.8rem',
                          color: location.pathname === item.path ? 'primary.main' : 'text.primary',
                          fontWeight: location.pathname === item.path ? 600 : 400,
                        }} 
                      />
                    </ListItemButton>
                  ))}
                </List>
              </Collapse>
            </React.Fragment>
          );
        })}
        </Box>
      </Drawer>
    </>
  );
}

function Navigation() {
  const { isAuthenticated } = useAuth();

  return isAuthenticated ? <NavigationContent /> : null;
}

export default Navigation;
