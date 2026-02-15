import React, { useState, useEffect, useMemo, useCallback } from 'react';
import { getApiBaseUrl } from '../config/ports';
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
  Handshake as RelationshipsIcon,
  RocketLaunch as CampaignExecutionIcon,
  Email as EmailIcon,
  Autorenew as SubscriptionIcon,
  AttachMoney as CommissionIcon,
  Receipt as InvoiceIcon,
  Payment as PaymentIcon,
  ShoppingCart as OrderIcon,
  MergeType as MergeIcon,
  Speed as SpeedIcon,
  BarChart as BarChartIcon,
  Warning as WarningIcon,
  ChangeCircle as ChangeCircleIcon,
  Article as ArticleIcon,
  Category as CategoryIcon,
  Web as WebIcon,
  Route as RouteIcon,
  Queue as QueueIcon,
  Event as EventIcon,
  CheckCircle as CheckCircleIcon,
  Chat as ChatIcon,
  ThumbUp as ThumbUpIcon,
  Share as ShareIcon,
  Insights as InsightsIcon,
  Api as ApiIcon,
  MonitorHeart as MonitorHeartIcon,
  Flag as FlagIcon,
  Science as ScienceIcon,
  HowToReg as HowToRegIcon,
  GroupWork as GroupWorkIcon,
  Dataset as DatasetIcon,
  ContentCopy as ContentCopyIcon,
  Score as ScoreIcon,
  Assignment as AssignmentIcon,
  Extension as ExtensionIcon,
  MenuBook as MenuBookIcon,
  // Reports & Analytics icons
  Assessment as ReportsIcon,
  Analytics as AnalyticsIcon,
  DesignServices as WorkflowBuilderIcon,
  PlayCircle as WorkflowMonitorIcon,
  Hub as IntegrationsIcon,
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
  SmartToy as SmartToyIcon,
} from '@mui/icons-material';
import { Link as RouterLink, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useProfile } from '../contexts/ProfileContext';
import { useBranding } from '../contexts/BrandingContext';
import { LogoDisplay } from './common';
import { getApiEndpoint } from '../config/ports';
import UserSettingsDialog from './UserSettingsDialog';
import logo from '../assets/logo.png';
import logger from '../services/logger';
import navigationConfigService, { NavigationItemConfig } from '../services/navigationConfigService';
import './Navigation.css';

function NavigationContent() {
  const { isAuthenticated, user, logout } = useAuth();
  const { profile, hasPermission, canAccessMenu } = useProfile();
  const { branding } = useBranding();
  const navigate = useNavigate();
  const location = useLocation();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [userSettingsOpen, setUserSettingsOpen] = useState(false);
  // Demo mode feature removed - using production database only
  const [navRefreshKey, setNavRefreshKey] = useState(0); // Force re-render on nav update
  
  // Collapsible categories state
  const [expandedCategories, setExpandedCategories] = useState<Record<string, boolean>>({
    'main': true,
    'sales': true,
    'marketing': true,
    'support': true,
    'itsm': true,
    'productivity': true,
    'agents': true,
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

  // Dynamic navigation configuration from backend (provider-aware)
  const [dynamicNavConfig, setDynamicNavConfig] = useState<NavigationItemConfig[]>([]);
  const [providerStatus, setProviderStatus] = useState<Record<string, boolean>>({});
  const [configLoaded, setConfigLoaded] = useState(false);

  // Load dynamic navigation config from backend (provider-aware)
  useEffect(() => {
    const loadDynamicConfig = async () => {
      try {
        // Fetch navigation items and provider status from backend
        const [items, statusList] = await Promise.all([
          navigationConfigService.getNavigationItems(),
          navigationConfigService.getProviderStatus()
        ]);
        
        // Convert provider status array to record
        const statusRecord: Record<string, boolean> = {};
        statusList.forEach(s => {
          statusRecord[s.providerType] = s.isAvailable;
        });
        
        setDynamicNavConfig(items);
        setProviderStatus(statusRecord);
        setConfigLoaded(true);
        
        logger.info('Dynamic navigation config loaded', { itemCount: items.length, providers: Object.keys(statusRecord) });
      } catch (error) {
        // If backend config fails, fall back to static config
        logger.warn('Failed to load dynamic navigation config, using static config', { error });
        setConfigLoaded(true);
      }
    };

    if (isAuthenticated) {
      loadDynamicConfig();
    }
  }, [isAuthenticated]);

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
    } else if (path.includes('/accounts') || path.includes('/contacts')) {
      setExpandedCategories(prev => ({ ...prev, 'main': true }));
    } else if (path.includes('/leads') || path.includes('/opportunities') || path.includes('/products') || path.includes('/campaigns') || path.includes('/quotes') || path.includes('/contracts') || path.includes('/invoices') || path.includes('/payments') || path.includes('/orders') || path.includes('/teams')) {
      setExpandedCategories(prev => ({ ...prev, 'sales': true }));
    } else if (path.includes('/services') || path.includes('/service-requests')) {
      setExpandedCategories(prev => ({ ...prev, 'support': true }));
    } else if (path.includes('/itsm') || path.includes('/incidents') || path.includes('/knowledge') || path.includes('/catalog')) {
      setExpandedCategories(prev => ({ ...prev, 'itsm': true }));
    } else if (path.includes('/queue') || path.includes('/activities') || path.includes('/notes') || path.includes('/communications') || path.includes('/interactions')) {
      setExpandedCategories(prev => ({ ...prev, 'productivity': true }));
    } else if (path.includes('/agents')) {
      setExpandedCategories(prev => ({ ...prev, 'agents': true }));
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
      } else if (path.includes('workflows') || path.includes('dashboards') || path.includes('agents')) {
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
  const staticNavItemsConfig: Record<string, { label: string; icon: typeof DashboardIcon; path: string; menuName: string }> = useMemo(() => ({
    'dashboard': { label: 'Dashboard', icon: DashboardIcon, path: '/', menuName: 'Dashboard' },
    'accounts': { label: 'Accounts', icon: PeopleIcon, path: '/accounts', menuName: 'Accounts' },
    'accounts-360': { label: 'Accounts 360', icon: PersonSearchIcon, path: '/account-overview', menuName: 'Accounts360' },
    'customer-overview': { label: 'Customer 360°', icon: PersonSearchIcon, path: '/account-overview', menuName: 'Accounts360' },
    'contacts': { label: 'Contacts', icon: PeopleIcon, path: '/contacts', menuName: 'Contacts' },
    'leads': { label: 'Leads', icon: PeopleIcon, path: '/leads', menuName: 'Leads' },
    'opportunities': { label: 'Opportunities', icon: TrendingUpIcon, path: '/opportunities', menuName: 'Opportunities' },
    'products': { label: 'Products', icon: PackageIcon, path: '/products', menuName: 'Products' },
    'services': { label: 'Services', icon: SettingsIcon, path: '/services', menuName: 'Services' },
    'service-requests': { label: 'Service Requests', icon: SupportAgentIcon, path: '/service-requests', menuName: 'ServiceRequests' },
    'itsm-overview': { label: 'ITSM Overview', icon: DashboardIcon, path: '/itsm', menuName: 'ITSMOverview' },
    'itsm-incidents': { label: 'Incidents', icon: SupportAgentIcon, path: '/itsm/incidents', menuName: 'ITSMIncidents' },
    'itsm-problems': { label: 'Problems', icon: TestResultsIcon as typeof DashboardIcon, path: '/itsm/problems', menuName: 'ITSMProblems' },
    'itsm-changes': { label: 'Changes', icon: InteractionsIcon as typeof DashboardIcon, path: '/itsm/changes', menuName: 'ITSMChanges' },
    'itsm-cmdb': { label: 'CMDB', icon: WorkflowIcon as typeof DashboardIcon, path: '/itsm/cmdb', menuName: 'ITSMCMDB' },
    'itsm-knowledge': { label: 'Knowledge', icon: QuoteIcon as typeof DashboardIcon, path: '/itsm/knowledge', menuName: 'ITSMKnowledge' },
    'itsm-catalog': { label: 'Service Catalog', icon: PackageIcon as typeof DashboardIcon, path: '/itsm/catalog', menuName: 'ITSMCatalog' },
    'itsm-sla': { label: 'SLA', icon: ActivityIcon as typeof DashboardIcon, path: '/itsm/sla', menuName: 'ITSMSLA' },
    'itsm-metrics': { label: 'ITSM Metrics', icon: DashboardIcon, path: '/itsm/metrics', menuName: 'ITSMMetrics' },
    'campaigns': { label: 'Campaigns', icon: MegaphoneIcon, path: '/campaigns', menuName: 'Campaigns' },
    'email-templates': { label: 'Email Templates', icon: EmailIcon, path: '/email-templates', menuName: 'EmailTemplates' },
    'campaign-execution': { label: 'Campaign Execution', icon: CampaignExecutionIcon, path: '/campaign-execution', menuName: 'CampaignExecution' },
    'landing-pages': { label: 'Landing Pages', icon: ViewQuiltIcon, path: '/landing-pages', menuName: 'LandingPages' },
    'forms': { label: 'Forms', icon: QuoteIcon, path: '/forms', menuName: 'Forms' },
    'knowledge-base': { label: 'Knowledge Base', icon: QuoteIcon, path: '/knowledge-base', menuName: 'KnowledgeBase' },
    'relationships': { label: 'Relationships', icon: RelationshipsIcon, path: '/relationships', menuName: 'Relationships' },
    'territories': { label: 'Territories', icon: BusinessIcon, path: '/territories', menuName: 'Territories' },
    'lead-routing': { label: 'Lead Routing', icon: AutomationIcon, path: '/lead-routing', menuName: 'LeadRouting' },
    'approvals': { label: 'Approvals', icon: QuoteIcon, path: '/approvals', menuName: 'Approvals' },
    'quotes': { label: 'Quotes', icon: QuoteIcon, path: '/quotes', menuName: 'Quotes' },
    'commissions': { label: 'Commissions', icon: CommissionIcon, path: '/commissions', menuName: 'Commissions' },
    'contracts': { label: 'Contracts', icon: LicenseIcon, path: '/contracts', menuName: 'Contracts' },
    'invoices': { label: 'Invoices', icon: InvoiceIcon, path: '/invoices', menuName: 'Invoices' },
    'payments': { label: 'Payments', icon: PaymentIcon, path: '/payments', menuName: 'Payments' },
    'orders': { label: 'Orders', icon: OrderIcon, path: '/orders', menuName: 'Orders' },
    'subscriptions': { label: 'Subscriptions', icon: SubscriptionIcon, path: '/subscriptions', menuName: 'Subscriptions' },
    'teams': { label: 'Teams', icon: GroupsIcon, path: '/teams', menuName: 'Teams' },
    'departments': { label: 'Departments', icon: BusinessIcon, path: '/departments', menuName: 'Departments' },
    'my-queue': { label: 'My Queue', icon: TaskIcon, path: '/my-queue', menuName: 'MyQueue' },
    'activities': { label: 'Activities', icon: ActivityIcon, path: '/activities', menuName: 'Activities' },
    'tasks': { label: 'Tasks', icon: TaskIcon, path: '/tasks', menuName: 'Tasks' },
    'notes': { label: 'Notes', icon: NoteIcon, path: '/notes', menuName: 'Notes' },
    'communications': { label: 'Communications', icon: CommunicationsIcon, path: '/communications', menuName: 'Communications' },
    'interactions': { label: 'Interactions', icon: InteractionsIcon, path: '/interactions', menuName: 'Interactions' },
    // Reports & Analytics
    'reports': { label: 'Reports', icon: ReportsIcon as typeof DashboardIcon, path: '/reports', menuName: 'Reports' },
    'analytics': { label: 'Analytics', icon: AnalyticsIcon as typeof DashboardIcon, path: '/analytics', menuName: 'Analytics' },
    // AI Agents
    'agent-directory': { label: 'AI Agents', icon: SmartToyIcon as typeof DashboardIcon, path: '/agents', menuName: 'AgentDirectory' },
    // Help & Info items
    'about': { label: 'About', icon: InfoIcon, path: '/about', menuName: 'About' },
    'help': { label: 'Help', icon: HelpIcon, path: '/help', menuName: 'Help' },
    'api-documentation': { label: 'API Documentation', icon: QuoteIcon, path: '/help/api', menuName: 'ApiDocumentation' },
    'licenses': { label: 'Licenses', icon: LicenseIcon, path: '/licenses', menuName: 'Licenses' },
  }), []);

  const staticAdminItemsConfig: Record<string, { label: string; icon: typeof DashboardIcon; path: string; menuName: string }> = useMemo(() => ({
    // System Administration
    'monitoring-settings': { label: 'Monitoring', icon: MonitorIcon, path: '/admin/monitoring', menuName: 'MonitoringSettings' },
    'deployment-settings': { label: 'Deployment', icon: CloudIcon, path: '/admin/deployment', menuName: 'DeploymentSettings' },
    'security-settings': { label: 'Security', icon: SecurityIcon, path: '/admin/security', menuName: 'SecuritySettings' },
    'feature-management': { label: 'Features', icon: FeatureToggleIcon, path: '/admin/features', menuName: 'FeatureManagement' },
    'database-settings': { label: 'Database', icon: StorageIcon, path: '/admin/database-settings', menuName: 'DatabaseSettings' },
    // User Administration
    'user-management': { label: 'Users', icon: PeopleIcon, path: '/admin/users', menuName: 'UserManagement' },
    'user-approvals': { label: 'Approvals', icon: PersonAddIcon, path: '/admin/approvals', menuName: 'UserApprovals' },
    'group-management': { label: 'Groups', icon: GroupsIcon, path: '/admin/groups', menuName: 'GroupManagement' },
    'social-login': { label: 'Social Login', icon: LoginIcon, path: '/admin/social-login', menuName: 'SocialLogin' },
    // CRM Administration
    'branding-settings': { label: 'Branding', icon: PaletteIcon, path: '/admin/branding', menuName: 'BrandingSettings' },
    'navigation-settings': { label: 'Navigation', icon: MenuIcon, path: '/admin/navigation', menuName: 'NavigationSettings' },
    'module-fields': { label: 'Modules & Fields', icon: ModuleIcon, path: '/admin/modules', menuName: 'ModuleFields' },
    'duplicate-rules': { label: 'Duplicate Rules', icon: MergeIcon, path: '/admin/duplicate-rules', menuName: 'DuplicateRules' },
    'lead-score-rules': { label: 'Lead Score Rules', icon: TrendingUpIcon, path: '/admin/lead-score-rules', menuName: 'LeadScoreRules' },
    'sr-definitions': { label: 'Service Requests', icon: SupportAgentIcon, path: '/admin/service-requests', menuName: 'ServiceRequestDefinitions' },
    'master-data': { label: 'Master Data', icon: StorageIcon, path: '/admin/master-data', menuName: 'MasterData' },
    'dashboard-settings': { label: 'Dashboards', icon: DashboardIcon, path: '/admin/dashboards', menuName: 'DashboardSettings' },
    'workflow-settings': { label: 'Workflows', icon: WorkflowIcon, path: '/admin/workflows', menuName: 'WorkflowSettings' },
    'workflow-monitor': { label: 'Workflow Monitor', icon: WorkflowMonitorIcon, path: '/admin/workflows/monitor', menuName: 'WorkflowMonitor' },
    'integrations': { label: 'Integrations (n8n)', icon: IntegrationsIcon, path: '/admin/integrations', menuName: 'Integrations' },
    'analytics-settings': { label: 'Analytics (Superset)', icon: AnalyticsIcon, path: '/admin/analytics', menuName: 'AnalyticsSettings' },
    'test-results': { label: 'Test Results', icon: TestResultsIcon, path: '/admin/test-results', menuName: 'TestResults' },
    'llm-settings': { label: 'AI / LLM Settings', icon: LLMIcon, path: '/admin/llm', menuName: 'LLMSettings' },
    // AI Agent Administration
    'agent-management': { label: 'Agent Management', icon: SmartToyIcon as typeof DashboardIcon, path: '/admin/agents', menuName: 'AgentManagement' },
    'agent-approvals': { label: 'Agent Approvals', icon: PersonAddIcon, path: '/admin/agents/approvals', menuName: 'AgentApprovals' },
    'agent-analytics': { label: 'Agent Analytics', icon: AnalyticsIcon, path: '/admin/agents/analytics', menuName: 'AgentAnalytics' },
    // Legacy items
    'channel-settings': { label: 'Channel Settings', icon: ChannelSettingsIcon, path: '/channel-settings', menuName: 'ChannelSettings' },
  }), []);

  const iconNameMap: Record<string, typeof DashboardIcon> = useMemo(() => ({
    Dashboard: DashboardIcon,
    Business: BusinessIcon,
    Preview: PersonSearchIcon,
    ContactPage: PeopleIcon,
    PersonSearch: PersonSearchIcon,
    TrendingUp: TrendingUpIcon,
    RequestQuote: QuoteIcon,
    ShoppingCart: OrderIcon,
    Receipt: InvoiceIcon,
    Payment: PaymentIcon,
    Description: QuoteIcon,
    Subscriptions: SubscriptionIcon,
    Inventory: PackageIcon,
    AttachMoney: CommissionIcon,
    Groups: GroupsIcon,
    Map: BusinessIcon,
    SupportAgent: SupportAgentIcon,
    MenuBook: MenuBookIcon,
    Build: SettingsIcon,
    Campaign: MegaphoneIcon,
    Speed: SpeedIcon,
    BarChart: BarChartIcon,
    Warning: WarningIcon,
    ChangeCircle: ChangeCircleIcon,
    Storage: StorageIcon,
    Article: ArticleIcon,
    Category: CategoryIcon,
    Web: WebIcon,
    Route: RouteIcon,
    Queue: QueueIcon,
    Event: EventIcon,
    CheckCircle: CheckCircleIcon,
    Chat: ChatIcon,
    Forum: CommunicationsIcon,
    ThumbUp: ThumbUpIcon,
    Share: ShareIcon,
    Assessment: ReportsIcon as typeof DashboardIcon,
    Insights: InsightsIcon,
    Info: InfoIcon,
    Help: HelpIcon,
    Api: ApiIcon,
    Gavel: LicenseIcon,
    MonitorHeart: MonitorHeartIcon,
    Flag: FlagIcon,
    Science: ScienceIcon,
    Psychology: LLMIcon,
    HowToReg: HowToRegIcon,
    GroupWork: GroupWorkIcon,
    Dataset: DatasetIcon,
    ContentCopy: ContentCopyIcon,
    Score: ScoreIcon,
    Assignment: AssignmentIcon,
    Menu: MenuIcon,
    ViewModule: ModuleIcon,
    AccountTree: WorkflowIcon,
    Timeline: WorkflowMonitorIcon,
    Extension: ExtensionIcon,
    Support: SupportAgentIcon,
    Email: EmailIcon,
    RocketLaunch: CampaignExecutionIcon,
    DesignServices: WorkflowBuilderIcon,
    PlayCircle: WorkflowMonitorIcon,
    Hub: IntegrationsIcon,
    Analytics: AnalyticsIcon,
    BugReport: TestResultsIcon,
    Cloud: CloudIcon,
    Monitor: MonitorIcon,
    Security: SecurityIcon,
    ToggleOn: FeatureToggleIcon,
    PersonAdd: PersonAddIcon,
    Palette: PaletteIcon,
    Navigation: NavAdminIcon as typeof DashboardIcon,
    DashboardCustomize: DashboardAdminIcon as typeof DashboardIcon,
  }), []);

  const routeOverrides: Record<string, string> = useMemo(() => ({
    'customer-overview': '/account-overview',
  }), []);

  const dynamicItemsById = useMemo(() => {
    const map: Record<string, NavigationItemConfig> = {};
    dynamicNavConfig.forEach(item => {
      map[item.id] = item;
    });
    return map;
  }, [dynamicNavConfig]);

  const isDynamicItemEnabled = useCallback((item: NavigationItemConfig) => {
    if (item.enabled === false || item.visible === false) return false;
    if (item.requiredProvider) {
      return providerStatus[item.requiredProvider] ?? false;
    }
    return true;
  }, [providerStatus]);

  const effectiveNavItemsConfig = useMemo(() => {
    const merged = { ...staticNavItemsConfig };

    Object.values(dynamicItemsById).forEach(item => {
      const isAdminItem = item.category === 'admin' || !!item.adminSubcategory;
      if (isAdminItem) return;

      const base = staticNavItemsConfig[item.id];
      const icon = base?.icon || iconNameMap[item.icon] || DashboardIcon;
      const resolvedPath = routeOverrides[item.id] || (item.isExternal && item.externalUrl ? item.externalUrl : (item.path || base?.path || '/'));

      merged[item.id] = {
        label: item.label || base?.label || item.id,
        icon,
        path: resolvedPath,
        menuName: item.menuName || base?.menuName || item.label || item.id,
      };
    });

    return merged;
  }, [dynamicItemsById, iconNameMap, staticNavItemsConfig]);

  const effectiveAdminItemsConfig = useMemo(() => {
    const merged = { ...staticAdminItemsConfig };

    Object.values(dynamicItemsById).forEach(item => {
      const isAdminItem = item.category === 'admin' || !!item.adminSubcategory;
      if (!isAdminItem) return;

      const base = staticAdminItemsConfig[item.id];
      const icon = base?.icon || iconNameMap[item.icon] || DashboardIcon;
      const resolvedPath = routeOverrides[item.id] || (item.isExternal && item.externalUrl ? item.externalUrl : (item.path || base?.path || '/'));

      merged[item.id] = {
        label: item.label || base?.label || item.id,
        icon,
        path: resolvedPath,
        menuName: item.menuName || base?.menuName || item.label || item.id,
      };
    });

    return merged;
  }, [dynamicItemsById, iconNameMap, staticAdminItemsConfig]);

  // Default order for nav items
  const defaultNavOrder = useMemo(() => [
    'dashboard', 'accounts', 'accounts-360', 'contacts', 'relationships', 'leads', 'opportunities',
    'products', 'services', 'service-requests', 'campaigns', 'email-templates', 'campaign-execution', 'landing-pages', 'quotes',
    'contracts', 'invoices', 'payments', 'orders', 'commissions', 'subscriptions', 'teams',
    'territories', 'lead-routing', 'approvals',
    'itsm-overview', 'itsm-incidents', 'itsm-problems', 'itsm-changes', 'itsm-cmdb', 'itsm-knowledge', 'itsm-catalog', 'itsm-sla', 'itsm-metrics',
    'my-queue', 'activities', 'tasks', 'notes', 'communications', 'interactions'
  ], []);
  const defaultAdminOrder = useMemo(() => [
    'monitoring-settings', 'deployment-settings', 'security-settings', 'feature-management',
    'user-management', 'user-approvals', 'group-management', 'social-login',
    'branding-settings', 'navigation-settings', 'module-fields', 'sr-definitions', 'master-data', 'dashboard-settings', 'workflow-settings',
    'channel-settings', 'settings'
  ], []);

  // Default categories
  const defaultCategories = useMemo(() => [
    { id: 'main', label: 'Main', order: 0 },
    { id: 'sales', label: 'Sales', order: 1 },
    { id: 'marketing', label: 'Marketing', order: 2 },
    { id: 'support', label: 'Customer Support', order: 3 },
    { id: 'itsm', label: 'IT Service Management', order: 4 },
    { id: 'productivity', label: 'Productivity', order: 5 },
    { id: 'agents', label: 'AI Agents', order: 6 },
    { id: 'info', label: 'Help & Info', order: 7 },
    { id: 'admin', label: 'Administration', order: 8 },
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
    Settings: SettingsIcon,
    People: PeopleIcon,
    Business: BusinessIcon,
    Support: SupportAgentIcon,
    Menu: MenuIcon,
    ViewModule: ModuleIcon,
    AccountTree: WorkflowIcon,
    Forum: CommunicationsIcon,
    SubcategoryIcon: ViewQuiltIcon,
  }), []);

  // Default nav items with their proper categories (matching NavigationSettingsTab)
  const defaultNavItemsWithCategory = useMemo(() => [
    { id: 'dashboard', order: 0, visible: true, category: 'main' },
    { id: 'accounts', order: 1, visible: true, category: 'main' },
    { id: 'accounts-360', order: 2, visible: true, category: 'main' },
    { id: 'contacts', order: 3, visible: true, category: 'main' },
    { id: 'relationships', order: 3.5, visible: true, category: 'main' },
    { id: 'leads', order: 4, visible: true, category: 'sales' },
    { id: 'opportunities', order: 5, visible: true, category: 'sales' },
    { id: 'products', order: 6, visible: true, category: 'sales' },
    { id: 'services', order: 7, visible: true, category: 'support' },
    { id: 'service-requests', order: 8, visible: true, category: 'support' },
    { id: 'itsm-overview', order: 30, visible: true, category: 'itsm' },
    { id: 'itsm-incidents', order: 31, visible: true, category: 'itsm' },
    { id: 'itsm-problems', order: 32, visible: true, category: 'itsm' },
    { id: 'itsm-changes', order: 33, visible: true, category: 'itsm' },
    { id: 'itsm-cmdb', order: 34, visible: true, category: 'itsm' },
    { id: 'itsm-knowledge', order: 35, visible: true, category: 'itsm' },
    { id: 'itsm-catalog', order: 36, visible: true, category: 'itsm' },
    { id: 'itsm-sla', order: 37, visible: true, category: 'itsm' },
    { id: 'itsm-metrics', order: 38, visible: true, category: 'itsm' },
    { id: 'campaigns', order: 9, visible: true, category: 'marketing' },
    { id: 'email-templates', order: 9.1, visible: true, category: 'marketing' },
    { id: 'campaign-execution', order: 9.2, visible: true, category: 'marketing' },
    { id: 'landing-pages', order: 9.5, visible: true, category: 'marketing' },
    { id: 'forms', order: 9.6, visible: true, category: 'marketing' },
    { id: 'quotes', order: 10, visible: true, category: 'sales' },
    { id: 'territories', order: 10.1, visible: true, category: 'sales' },
    { id: 'lead-routing', order: 10.2, visible: true, category: 'sales' },
    { id: 'approvals', order: 10.3, visible: true, category: 'sales' },
    { id: 'commissions', order: 10.4, visible: true, category: 'sales' },
    { id: 'contracts', order: 10.5, visible: true, category: 'sales' },
    { id: 'invoices', order: 10.6, visible: true, category: 'sales' },
    { id: 'payments', order: 10.7, visible: true, category: 'sales' },
    { id: 'orders', order: 10.8, visible: true, category: 'sales' },
    { id: 'subscriptions', order: 10.85, visible: true, category: 'sales' },
    { id: 'teams', order: 10.9, visible: true, category: 'sales' },
    { id: 'departments', order: 10.95, visible: true, category: 'main' },
    { id: 'my-queue', order: 11, visible: true, category: 'productivity' },
    { id: 'activities', order: 12, visible: true, category: 'productivity' },
    { id: 'tasks', order: 12.5, visible: true, category: 'productivity' },
    { id: 'notes', order: 13, visible: true, category: 'productivity' },
    { id: 'communications', order: 14, visible: true, category: 'productivity' },
    { id: 'interactions', order: 15, visible: true, category: 'productivity' },
    { id: 'knowledge-base', order: 15.5, visible: true, category: 'support' },
    { id: 'reports', order: 16, visible: true, category: 'productivity' },
    { id: 'analytics', order: 17, visible: true, category: 'productivity' },
    // AI Agents
    { id: 'agent-directory', order: 40, visible: true, category: 'agents' },
    // Help & Info
    { id: 'about', order: 50, visible: true, category: 'info' },
    { id: 'help', order: 51, visible: true, category: 'info' },
    { id: 'api-documentation', order: 51.5, visible: true, category: 'info' },
    { id: 'licenses', order: 52, visible: true, category: 'info' },
    // System Administration
    { id: 'monitoring-settings', order: 60, visible: true, category: 'admin', adminSubcategory: 'admin-system' },
    { id: 'deployment-settings', order: 61, visible: true, category: 'admin', adminSubcategory: 'admin-system' },
    { id: 'security-settings', order: 63, visible: true, category: 'admin', adminSubcategory: 'admin-system' },
    { id: 'feature-management', order: 64, visible: true, category: 'admin', adminSubcategory: 'admin-system' },
    { id: 'database-settings', order: 64.5, visible: true, category: 'admin', adminSubcategory: 'admin-system' },
    // User Administration
    { id: 'user-management', order: 65, visible: true, category: 'admin', adminSubcategory: 'admin-users' },
    { id: 'user-approvals', order: 66, visible: true, category: 'admin', adminSubcategory: 'admin-users' },
    { id: 'group-management', order: 67, visible: true, category: 'admin', adminSubcategory: 'admin-users' },
    { id: 'social-login', order: 68, visible: true, category: 'admin', adminSubcategory: 'admin-users' },
    // CRM Administration
    { id: 'branding-settings', order: 69, visible: true, category: 'admin', adminSubcategory: 'admin-crm' },
    { id: 'navigation-settings', order: 70, visible: true, category: 'admin', adminSubcategory: 'admin-navigation' },
    { id: 'module-fields', order: 71, visible: true, category: 'admin', adminSubcategory: 'admin-modules' },
    { id: 'duplicate-rules', order: 71.5, visible: true, category: 'admin', adminSubcategory: 'admin-crm' },
    { id: 'lead-score-rules', order: 71.6, visible: true, category: 'admin', adminSubcategory: 'admin-crm' },
    { id: 'sr-definitions', order: 72, visible: true, category: 'admin', adminSubcategory: 'admin-service' },
    { id: 'master-data', order: 73, visible: true, category: 'admin', adminSubcategory: 'admin-crm' },
    { id: 'dashboard-settings', order: 74, visible: true, category: 'admin', adminSubcategory: 'admin-workflows' },
    { id: 'workflow-settings', order: 75, visible: true, category: 'admin', adminSubcategory: 'admin-workflows' },
    { id: 'workflow-monitor', order: 75.1, visible: true, category: 'admin', adminSubcategory: 'admin-workflows' },
    { id: 'integrations', order: 75.2, visible: true, category: 'admin', adminSubcategory: 'admin-workflows' },
    { id: 'analytics-settings', order: 75.3, visible: true, category: 'admin', adminSubcategory: 'admin-workflows' },
    // Channels
    { id: 'channel-settings', order: 76, visible: true, category: 'admin', adminSubcategory: 'admin-channels' },
    // Test Results
    { id: 'test-results', order: 77, visible: true, category: 'admin', adminSubcategory: 'admin-system' },
    // AI / LLM
    { id: 'llm-settings', order: 78, visible: true, category: 'admin', adminSubcategory: 'admin-workflows' },
    { id: 'agent-management', order: 79, visible: true, category: 'admin', adminSubcategory: 'admin-workflows' },
    { id: 'agent-approvals', order: 80, visible: true, category: 'admin', adminSubcategory: 'admin-workflows' },
    { id: 'agent-analytics', order: 81, visible: true, category: 'admin', adminSubcategory: 'admin-workflows' },
  ], []);

  const mergeNavItemsWithDefaults = useCallback((savedItems: any[] | undefined) => {
    const merged = Array.isArray(savedItems) ? [...savedItems] : [];
    const existingIds = new Set(merged.map(item => item.id));

    defaultNavItemsWithCategory.forEach(defaultItem => {
      if (!existingIds.has(defaultItem.id)) {
        merged.push({
          id: defaultItem.id,
          order: defaultItem.order,
          visible: defaultItem.visible,
          category: defaultItem.category,
          adminSubcategory: defaultItem.adminSubcategory,
        });
      }
    });

    return merged;
  }, [defaultNavItemsWithCategory]);

  const dynamicNavOrder = useMemo(() => {
    if (!dynamicNavConfig.length) return [];

    const items = dynamicNavConfig
      .filter(isDynamicItemEnabled)
      .map(item => ({
        id: item.id,
        order: item.order ?? 0,
        visible: true,
        category: item.category || 'main',
        adminSubcategory: item.adminSubcategory,
      }));

    return mergeNavItemsWithDefaults(items);
  }, [dynamicNavConfig, isDynamicItemEnabled, mergeNavItemsWithDefaults]);

  // Get nav config from localStorage or use defaults
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const navConfig = useMemo(() => {
    try {
      const savedConfig = localStorage.getItem('crm_nav_order');
      if (savedConfig) {
        const parsed = JSON.parse(savedConfig);
        // Support both old format (array) and new format (object with navItems, categories, and adminSubcategories)
        if (Array.isArray(parsed)) {
          return {
            navItems: dynamicNavOrder.length ? dynamicNavOrder : mergeNavItemsWithDefaults(parsed),
            categories: defaultCategories,
            adminSubcategories: defaultAdminSubcategories
          };
        }
        return {
          navItems: dynamicNavOrder.length ? dynamicNavOrder : mergeNavItemsWithDefaults(parsed.navItems || []),
          categories: parsed.categories || defaultCategories,
          adminSubcategories: parsed.adminSubcategories || defaultAdminSubcategories
        };
      }
    } catch {
      // Use defaults
    }
    if (dynamicNavOrder.length) {
      return {
        navItems: dynamicNavOrder,
        categories: defaultCategories,
        adminSubcategories: defaultAdminSubcategories
      };
    }
    return null;
  }, [defaultCategories, defaultAdminSubcategories, dynamicNavOrder, mergeNavItemsWithDefaults, navRefreshKey]); // Include navRefreshKey to force recalculation on nav update

  // Get admin subcategories from config
  const adminSubcategories = useMemo(() => {
    const subcats = navConfig?.adminSubcategories || defaultAdminSubcategories;
    return subcats.sort((a: { order: number }, b: { order: number }) => a.order - b.order);
  }, [navConfig, defaultAdminSubcategories]);

  // Build ordered nav items with category info
  const navItemsWithCategory = useMemo(() => {
    const order = navConfig?.navItems || defaultNavItemsWithCategory;
    return order
      .filter((item: { id: string; visible: boolean }) => item.visible && (effectiveNavItemsConfig[item.id] || effectiveAdminItemsConfig[item.id]))
      .sort((a: { order: number }, b: { order: number }) => a.order - b.order)
      .map((item: { id: string; customLabel?: string; category?: string }) => ({
        ...effectiveNavItemsConfig[item.id] || effectiveAdminItemsConfig[item.id],
        customLabel: item.customLabel,
        category: item.category || 'main',
        id: item.id
      }));
  }, [navConfig, defaultNavItemsWithCategory, effectiveNavItemsConfig, effectiveAdminItemsConfig]);

  // Get categories from config
  const categories = useMemo(() => {
    return (navConfig?.categories || defaultCategories).sort((a: { order: number }, b: { order: number }) => a.order - b.order);
  }, [navConfig, defaultCategories]);

  // Build ordered nav items (legacy - used for simple list)
  const navItems = useMemo(() => {
    const order = navConfig?.navItems || defaultNavItemsWithCategory;
    return order
      .filter((item: { id: string; visible: boolean }) => item.visible && effectiveNavItemsConfig[item.id])
      .sort((a: { order: number }, b: { order: number }) => a.order - b.order)
      .map((item: { id: string; customLabel?: string }) => ({
        ...effectiveNavItemsConfig[item.id],
        customLabel: item.customLabel
      }));
  }, [navConfig, defaultNavItemsWithCategory, effectiveNavItemsConfig]);

  const adminItems = useMemo(() => {
    const order = navConfig?.navItems?.filter((item: { id: string }) => effectiveAdminItemsConfig[item.id]) || 
      defaultNavItemsWithCategory.filter(item => effectiveAdminItemsConfig[item.id]);
    return order
      .filter((item: { id: string; visible: boolean }) => item.visible && effectiveAdminItemsConfig[item.id])
      .sort((a: { order: number }, b: { order: number }) => a.order - b.order)
      .map((item: { id: string; customLabel?: string }) => ({
        ...effectiveAdminItemsConfig[item.id],
        customLabel: item.customLabel
      }));
  }, [navConfig, defaultNavItemsWithCategory, effectiveAdminItemsConfig]);

  // Get header color: user's custom color, or red for admin, or primary color
  const getHeaderColor = () => {
    if (user?.headerColor) return user.headerColor;
    if (user?.role === 'Admin' || String(user?.role) === '0') return '#C62828';
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
    const logoPath = branding.brandingLogoUrl || branding.companyLogoUrl;
    if (logoPath) {
      // If it's a data URL (base64), use it directly
      if (logoPath.startsWith('data:')) {
        return logoPath;
      }
      // If it's a relative URL starting with /uploads, prepend API base URL
      if (logoPath.startsWith('/uploads')) {
        return `${getApiBaseUrl()}${logoPath}`;
      }
      return logoPath;
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
  const visibleNavItems = navItems.filter((item: { menuName: string }) => canAccessMenu(item.menuName));
  const visibleAdminItems = adminItems.filter((item: { menuName: string }) => canAccessMenu(item.menuName));

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
            <Box sx={{ mr: 1.5, flexShrink: 0 }}>
              <LogoDisplay size={36} />
            </Box>
            <Typography variant="h6" component={RouterLink} to="/" sx={{ textDecoration: 'none', color: 'inherit', fontWeight: 600 }}>
              {branding.solutionName || branding.companyName || 'CRM System'}
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <IconButton color="inherit" onClick={handleMenuOpen} aria-label="Open user menu">
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
              <MenuItem onClick={() => { handleMenuClose(); setUserSettingsOpen(true); }}>
                <SettingsIcon sx={{ mr: 1, fontSize: '1.2rem' }} />
                User Preferences
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
          <Box sx={{ width: 32, height: 32, flexShrink: 0 }}>
            <LogoDisplay size={32} />
          </Box>
          <Typography variant="subtitle1" sx={{ fontWeight: 600, flex: 1 }}>
            {branding.solutionName || branding.companyName || 'CRM System'}
          </Typography>
        </Box>
        
        {/* Scrollable Content */}
        <Box sx={{ flex: 1, overflow: 'auto' }}>
        
        {/* Render items grouped by category */}
        {categories.map((category: { id: string; label: string; order: number }, catIdx: number) => {
          // Skip admin category for non-admin users
          if (category.id === 'admin' && !(user?.role === 'Admin' || String(user?.role) === '0' || hasPermission('canManageUsers'))) {
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
                        effectiveAdminItemsConfig[item.id]
                      )
                      .map((item: { id: string; customLabel?: string }) => ({
                        ...effectiveAdminItemsConfig[item.id],
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
                  {category.id === 'agents' && <SmartToyIcon fontSize="small" sx={{ color: 'secondary.dark' }} />}
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
      
      {/* User Settings Dialog */}
      <UserSettingsDialog 
        open={userSettingsOpen} 
        onClose={() => setUserSettingsOpen(false)}
        onThemeChange={(theme) => {
          // Theme change handler - could trigger app-wide theme switch
          logger.debug('Theme changed to:', theme);
          // Force page reload to apply theme (simple approach)
          // In a more sophisticated implementation, this would update a theme context
        }}
      />
    </>
  );
}

function Navigation() {
  const { isAuthenticated } = useAuth();

  return isAuthenticated ? <NavigationContent /> : null;
}

export default Navigation;
