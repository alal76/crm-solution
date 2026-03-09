import React, { useState, useEffect, useMemo, useCallback } from 'react';
import { DragDropContext, Droppable, Draggable, type DropResult } from '@hello-pangea/dnd';
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
  Tooltip,
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
  Tune as CRMConfigIcon,
  Dns as DatabaseIcon,
  Memory as WorkerIcon,
  Widgets as UiCustomIcon,
  FileCopy as TemplateIcon,
  Person as PersonIcon,
  Terminal as TerminalIcon,
  Brightness4 as DarkModeToggleIcon,
  Brightness7 as LightModeToggleIcon,
} from '@mui/icons-material';
import { Link as RouterLink, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useProfile } from '../contexts/ProfileContext';
import { useBranding } from '../contexts/BrandingContext';
import { useTheme as useCrmTheme } from '../contexts/ThemeContext';
import { LogoDisplay } from './common';
import { getApiEndpoint } from '../config/ports';
import UserSettingsDialog from './UserSettingsDialog';
import SidebarCustomizer from './layout/SidebarCustomizer';
import logo from '../assets/logo.png';
import logger from '../services/logger';
import navigationConfigService, { NavigationItemConfig } from '../services/navigationConfigService';
import './Navigation.css';
import { useNavigationPreferences } from '../hooks/useNavigationPreferences';
import { RecentItemsDropdown } from './common/RecentItemsDropdown';

function NavigationContent() {
  const { isAuthenticated, user, logout } = useAuth();
  const { profile, hasPermission, canAccessMenu } = useProfile();
  const { branding } = useBranding();
  const { effectiveTheme, setThemeMode: setCrmThemeMode } = useCrmTheme();
  const navigate = useNavigate();
  const location = useLocation();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [userSettingsOpen, setUserSettingsOpen] = useState(false);
  const [customizerOpen, setCustomizerOpen] = useState(false);
  // Demo mode feature removed - using production database only
  const [navRefreshKey, setNavRefreshKey] = useState(0); // Force re-render on nav update

  // TODO-SYS007-003: DnD category order — persisted to localStorage
  const NAV_ORDER_KEY = 'crm-nav-order';
  const [navCategoryOrder, setNavCategoryOrder] = useState<string[]>(() => {
    try {
      const stored = localStorage.getItem('crm-nav-order');
      return stored ? (JSON.parse(stored) as string[]) : [];
    } catch {
      return [];
    }
  });
  
  // Collapsible categories & admin sections — persisted to localStorage via hook
  const {
    expandedCategories,
    expandedAdminSections,
    toggleCategory,
    toggleAdminSection,
    expandCategory,
    expandAdminSection,
  } = useNavigationPreferences();

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

  // Auto-expand category and section if current route matches
  useEffect(() => {
    const path = location.pathname;
    
    // Auto-expand main categories based on route
    if (path === '/' || path === '/dashboard') {
      expandCategory('main');
    } else if (path.includes('/accounts') || path.includes('/contacts')) {
      expandCategory('main');
    } else if (path.includes('/leads') || path.includes('/opportunities') || path.includes('/products') || path.includes('/campaigns') || path.includes('/quotes') || path.includes('/contracts') || path.includes('/invoices') || path.includes('/payments') || path.includes('/orders') || path.includes('/teams')) {
      expandCategory('sales');
    } else if (path.includes('/services') || path.includes('/service-requests')) {
      expandCategory('support');
    } else if (path.includes('/itsm') || path.includes('/incidents') || path.includes('/knowledge') || path.includes('/catalog')) {
      expandCategory('itsm');
    } else if (path.includes('/queue') || path.includes('/activities') || path.includes('/notes') || path.includes('/communications') || path.includes('/interactions')) {
      expandCategory('productivity');
    } else if (path.includes('/agents')) {
      expandCategory('agents');
    } else if (path.includes('/about') || path.includes('/help') || path.includes('/licenses')) {
      expandCategory('info');
    }
    
    // Auto-expand admin sections based on current path
    if (path.startsWith('/admin/')) {
      expandCategory('admin');
      // Determine which section to expand based on path
      if (path.includes('/config/system') || path.includes('/features') || path.includes('/navigation')) {
        expandAdminSection('system-config');
      } else if (path.includes('/users') || path.includes('/approvals') || path.includes('/groups') || path.includes('/social-login') || path.includes('/security')) {
        expandAdminSection('user-management');
      } else if (path.includes('/config/crm') || path.includes('/settings/sales') || path.includes('/settings/service-desk') || path.includes('/modules') || path.includes('/master-data') || path.includes('/enum-management') || path.includes('/duplicate-rules') || path.includes('/lead-score-rules') || path.includes('/service-requests')) {
        expandAdminSection('crm-config');
      } else if (path.includes('/llm') || path.includes('/integrations') || path.includes('/analytics') || path.includes('/agents')) {
        expandAdminSection('ai-integrations');
      } else if (path.includes('/database') || path.includes('/monitoring') || path.includes('/deployment') || path.includes('/workers')) {
        expandAdminSection('infrastructure');
      } else if (path.includes('/branding') || path.includes('/dashboards') || path.includes('/ui-customization')) {
        expandAdminSection('customization');
      } else if (path.includes('/workflows')) {
        expandAdminSection('workflows');
      } else if (path.includes('/api-docs') || path.includes('/test-results') || path.includes('/audit')) {
        expandAdminSection('developer-tools');
      }
    }
  }, [location.pathname]); // eslint-disable-line react-hooks/exhaustive-deps

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
    'itsm-escalation-rules': { label: 'Escalation Rules', icon: WarningIcon as typeof DashboardIcon, path: '/itsm/escalation/rules', menuName: 'ITSMEscalation' },
    'itsm-escalation-dashboard': { label: 'Escalation Dashboard', icon: ActivityIcon as typeof DashboardIcon, path: '/itsm/escalation/dashboard', menuName: 'ITSMEscalation' },
    'itsm-escalation-policies': { label: 'Escalation Policies', icon: AssignmentIcon as typeof DashboardIcon, path: '/itsm/escalation-policies', menuName: 'ITSMEscalation' },
    'itsm-sla-policies': { label: 'SLA Policies', icon: AssignmentIcon as typeof DashboardIcon, path: '/itsm/sla-policies', menuName: 'ITSMSLA' },
    'itsm-service-queues': { label: 'Service Queues', icon: QueueIcon as typeof DashboardIcon, path: '/itsm/service-queues', menuName: 'ITSMSLA' },
    'campaigns': { label: 'Campaigns', icon: MegaphoneIcon, path: '/campaigns', menuName: 'Campaigns' },
    'email-templates': { label: 'Email Templates', icon: EmailIcon, path: '/email-templates', menuName: 'EmailTemplates' },
    'campaign-execution': { label: 'Campaign Execution', icon: CampaignExecutionIcon, path: '/campaign-execution', menuName: 'CampaignExecution' },
    'landing-pages': { label: 'Landing Pages', icon: ViewQuiltIcon, path: '/landing-pages', menuName: 'LandingPages' },
    'forms': { label: 'Forms', icon: QuoteIcon, path: '/forms', menuName: 'Forms' },
    // MKT-002: Email Sequences
    'marketing-templates': { label: 'Email Sequences', icon: EmailIcon, path: '/marketing/templates', menuName: 'EmailSequences' },
    // MKT-003: Campaign Analytics
    'campaign-analytics': { label: 'Campaign Analytics', icon: BarChartIcon, path: '/marketing/analytics', menuName: 'CampaignAnalytics' },
    'knowledge-base': { label: 'Knowledge Base', icon: QuoteIcon, path: '/knowledge-base', menuName: 'KnowledgeBase' },
    'relationships': { label: 'Relationships', icon: RelationshipsIcon, path: '/relationships', menuName: 'Relationships' },
    'partner-portal': { label: 'Partner Portal', icon: RelationshipsIcon, path: '/partner', menuName: 'PartnerPortal' },
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
    // Data Management
    'import-data': { label: 'Import Data', icon: StorageIcon as typeof DashboardIcon, path: '/data/import', menuName: 'ImportData' },
    'export-data': { label: 'Export Data', icon: StorageIcon as typeof DashboardIcon, path: '/data/export', menuName: 'ExportData' },
    // AI Agents
    'agent-directory': { label: 'AI Agents', icon: SmartToyIcon as typeof DashboardIcon, path: '/agents', menuName: 'AgentDirectory' },
    // Help & Info items
    'about': { label: 'About', icon: InfoIcon, path: '/about', menuName: 'About' },
    'help': { label: 'Help', icon: HelpIcon, path: '/help', menuName: 'Help' },
    'api-documentation': { label: 'API Documentation', icon: QuoteIcon, path: '/help/api', menuName: 'ApiDocumentation' },
    'licenses': { label: 'Licenses', icon: LicenseIcon, path: '/licenses', menuName: 'Licenses' },
  }), []);

  const staticAdminItemsConfig: Record<string, { label: string; icon: typeof DashboardIcon; path: string; menuName: string }> = useMemo(() => ({
    // System Configuration
    'general-settings': { label: 'General Settings', icon: SettingsIcon, path: '/admin/config/system', menuName: 'SystemConfiguration' },
    'feature-management': { label: 'Feature Flags', icon: FeatureToggleIcon, path: '/admin/features', menuName: 'FeatureManagement' },
    'navigation-settings': { label: 'Navigation', icon: NavAdminIcon as typeof DashboardIcon, path: '/admin/navigation', menuName: 'NavigationSettings' },
    // User Management
    'user-management': { label: 'Users', icon: PeopleIcon, path: '/admin/users', menuName: 'UserManagement' },
    'group-management': { label: 'Groups', icon: GroupsIcon, path: '/admin/groups', menuName: 'GroupManagement' },
    'user-approvals': { label: 'Approvals', icon: ThumbUpIcon, path: '/admin/approvals', menuName: 'UserApprovals' },
    'security-settings': { label: 'Security', icon: SecurityIcon, path: '/admin/security', menuName: 'SecuritySettings' },
    'social-login': { label: 'Social Login', icon: LoginIcon, path: '/admin/social-login', menuName: 'SocialLogin' },
    // CRM Configuration
    'crm-config-page': { label: 'CRM Config', icon: CRMConfigIcon as typeof DashboardIcon, path: '/admin/config/crm', menuName: 'CRMConfiguration' },
    'sales-config': { label: 'Sales Config', icon: CommissionIcon, path: '/admin/settings/sales', menuName: 'SalesConfig' },
    'service-desk-config': { label: 'Service Desk', icon: SupportAgentIcon, path: '/admin/settings/service-desk', menuName: 'ServiceDeskConfig' },
    'module-fields': { label: 'Modules & Fields', icon: ModuleIcon, path: '/admin/modules', menuName: 'ModuleFields' },
    'master-data': { label: 'Master Data', icon: StorageIcon, path: '/admin/master-data', menuName: 'MasterData' },
    'enum-management': { label: 'Enum Management', icon: CategoryIcon, path: '/admin/enum-management', menuName: 'EnumManagement' },
    'duplicate-rules': { label: 'Duplicate Rules', icon: MergeIcon, path: '/admin/duplicate-rules', menuName: 'DuplicateRules' },
    'lead-score-rules': { label: 'Lead Score Rules', icon: ScoreIcon as typeof DashboardIcon, path: '/admin/lead-score-rules', menuName: 'LeadScoreRules' },
    'sr-definitions': { label: 'Service Requests', icon: SupportAgentIcon, path: '/admin/service-requests', menuName: 'ServiceRequestDefinitions' },
    // AI & Integrations
    'llm-settings': { label: 'AI / LLM Settings', icon: LLMIcon, path: '/admin/llm', menuName: 'LLMSettings' },
    'integrations': { label: 'Integrations', icon: IntegrationsIcon, path: '/admin/integrations', menuName: 'Integrations' },
    'analytics-settings': { label: 'Analytics', icon: AnalyticsIcon, path: '/admin/analytics', menuName: 'AnalyticsSettings' },
    'agent-management': { label: 'Agent Management', icon: SmartToyIcon as typeof DashboardIcon, path: '/admin/agents', menuName: 'AgentManagement' },
    'agent-approvals': { label: 'Agent Approvals', icon: PersonAddIcon, path: '/admin/agents/approvals', menuName: 'AgentApprovals' },
    'agent-analytics': { label: 'Agent Analytics', icon: AnalyticsIcon, path: '/admin/agents/analytics', menuName: 'AgentAnalytics' },
    // Infrastructure
    'database-settings': { label: 'Database', icon: DatabaseIcon as typeof DashboardIcon, path: '/admin/database', menuName: 'DatabaseSettings' },
    'monitoring-settings': { label: 'Monitoring', icon: MonitorIcon, path: '/admin/monitoring', menuName: 'MonitoringSettings' },
    'deployment-settings': { label: 'Deployment', icon: CloudIcon, path: '/admin/deployment', menuName: 'DeploymentSettings' },
    'worker-ops': { label: 'Workers', icon: WorkerIcon as typeof DashboardIcon, path: '/admin/workers', menuName: 'WorkerOperations' },
    // Customization
    'branding-settings': { label: 'Branding', icon: PaletteIcon, path: '/admin/branding', menuName: 'BrandingSettings' },
    'dashboard-settings': { label: 'Dashboards', icon: DashboardIcon, path: '/admin/dashboards', menuName: 'DashboardSettings' },
    'ui-customization': { label: 'UI Custom', icon: UiCustomIcon as typeof DashboardIcon, path: '/admin/ui-customization', menuName: 'UICustomization' },
    'portal-config': { label: 'Customer Portal', icon: WebIcon as typeof DashboardIcon, path: '/admin/portal', menuName: 'CustomerPortal' },
    // Workflows
    'workflow-settings': { label: 'Workflow List', icon: WorkflowIcon, path: '/admin/workflows', menuName: 'WorkflowSettings' },
    'workflow-monitor': { label: 'Monitor', icon: WorkflowMonitorIcon, path: '/admin/workflows/monitor', menuName: 'WorkflowMonitor' },
    'workflow-templates': { label: 'Templates', icon: TemplateIcon as typeof DashboardIcon, path: '/admin/workflows/templates', menuName: 'WorkflowTemplates' },
    // Scripting
    'script-plugin-library': { label: 'Script Library', icon: TerminalIcon as typeof DashboardIcon, path: '/scripting/plugins', menuName: 'ScriptPluginLibrary' },
    'script-plugin-new': { label: 'New Script', icon: TerminalIcon as typeof DashboardIcon, path: '/scripting/plugins/new', menuName: 'ScriptPluginNew' },
    // Developer Tools
    'api-docs': { label: 'API Docs', icon: ApiIcon, path: '/admin/api-docs', menuName: 'ApiDocs' },
    'test-results': { label: 'Test Results', icon: TestResultsIcon, path: '/admin/test-results', menuName: 'TestResults' },
    'audit-logging': { label: 'Audit Logging', icon: QuoteIcon, path: '/admin/audit', menuName: 'AuditLogging' },
    'api-users': { label: 'API Users', icon: VpnKeyIcon, path: '/admin/api-users', menuName: 'ApiUsers' },
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
    'products', 'services', 'service-requests', 'campaigns', 'email-templates', 'campaign-execution', 'landing-pages', 'forms', 'marketing-templates', 'campaign-analytics', 'quotes',
    'contracts', 'invoices', 'payments', 'orders', 'commissions', 'subscriptions', 'teams',
    'territories', 'lead-routing', 'approvals',
    'itsm-overview', 'itsm-incidents', 'itsm-problems', 'itsm-changes', 'itsm-cmdb', 'itsm-knowledge', 'itsm-catalog', 'itsm-sla', 'itsm-metrics', 'itsm-escalation-rules', 'itsm-escalation-dashboard', 'itsm-escalation-policies', 'itsm-sla-policies', 'itsm-service-queues',
    'my-queue', 'activities', 'tasks', 'notes', 'communications', 'interactions'
  ], []);
  const defaultAdminOrder = useMemo(() => [
    'general-settings', 'feature-management', 'navigation-settings',
    'user-management', 'group-management', 'user-approvals', 'security-settings', 'social-login',
    'crm-config-page', 'sales-config', 'service-desk-config', 'module-fields', 'master-data', 'enum-management', 'duplicate-rules', 'lead-score-rules', 'sr-definitions',
    'llm-settings', 'integrations', 'analytics-settings', 'agent-management', 'agent-approvals', 'agent-analytics',
    'database-settings', 'monitoring-settings', 'deployment-settings', 'worker-ops',
    'branding-settings', 'dashboard-settings', 'ui-customization', 'portal-config',
    'workflow-settings', 'workflow-monitor', 'workflow-templates',
    'api-docs', 'test-results', 'audit-logging',
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
    { id: 'system-config', label: 'System Configuration', icon: 'SystemAdminIcon', order: 0 },
    { id: 'user-management', label: 'User Management', icon: 'UserAdminIcon', order: 1 },
    { id: 'crm-config', label: 'CRM Configuration', icon: 'CRMAdminIcon', order: 2 },
    { id: 'ai-integrations', label: 'AI & Integrations', icon: 'SmartToyIcon', order: 3 },
    { id: 'infrastructure', label: 'Infrastructure', icon: 'StorageIcon', order: 4 },
    { id: 'customization', label: 'Customization', icon: 'PaletteIcon', order: 5 },
    { id: 'workflows', label: 'Workflows', icon: 'DashboardAdminIcon', order: 6 },
    { id: 'scripting', label: 'Scripting', icon: 'TerminalIcon', order: 6.5 },
    { id: 'developer-tools', label: 'Developer Tools', icon: 'ServiceReqIcon', order: 7 },
  ], []);

  // Icon map for admin subcategories
  const adminSubcategoryIconMap: Record<string, React.ElementType> = useMemo(() => ({
    SystemAdminIcon,
    UserAdminIcon,
    CRMAdminIcon,
    SmartToyIcon,
    StorageIcon,
    PaletteIcon,
    DashboardAdminIcon,
    ServiceReqIcon,
    SettingsIcon,
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
    TerminalIcon,
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
    { id: 'itsm-escalation-rules', order: 39, visible: true, category: 'itsm' },
    { id: 'itsm-escalation-dashboard', order: 40, visible: true, category: 'itsm' },
    { id: 'itsm-escalation-policies', order: 40.5, visible: true, category: 'itsm' },
    { id: 'itsm-sla-policies', order: 41, visible: true, category: 'itsm' },
    { id: 'itsm-service-queues', order: 42, visible: true, category: 'itsm' },
    { id: 'campaigns', order: 9, visible: true, category: 'marketing' },
    { id: 'email-templates', order: 9.1, visible: true, category: 'marketing' },
    { id: 'campaign-execution', order: 9.2, visible: true, category: 'marketing' },
    { id: 'landing-pages', order: 9.5, visible: true, category: 'marketing' },
    { id: 'forms', order: 9.6, visible: true, category: 'marketing' },
    { id: 'marketing-templates', order: 9.7, visible: true, category: 'marketing' },
    { id: 'campaign-analytics', order: 9.8, visible: true, category: 'marketing' },
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
    // Data Management
    { id: 'import-data', order: 18, visible: true, category: 'productivity' },
    { id: 'export-data', order: 19, visible: true, category: 'productivity' },
    // AI Agents
    { id: 'agent-directory', order: 40, visible: true, category: 'agents' },
    // Partner Portal (FLAG-002)
    { id: 'partner-portal', order: 20, visible: true, category: 'sales' },
    // Help & Info
    { id: 'about', order: 50, visible: true, category: 'info' },
    { id: 'help', order: 51, visible: true, category: 'info' },
    { id: 'api-documentation', order: 51.5, visible: true, category: 'info' },
    { id: 'licenses', order: 52, visible: true, category: 'info' },
    // System Configuration
    { id: 'general-settings', order: 60, visible: true, category: 'admin', adminSubcategory: 'system-config' },
    { id: 'feature-management', order: 61, visible: true, category: 'admin', adminSubcategory: 'system-config' },
    { id: 'navigation-settings', order: 62, visible: true, category: 'admin', adminSubcategory: 'system-config' },
    // User Management
    { id: 'user-management', order: 63, visible: true, category: 'admin', adminSubcategory: 'user-management' },
    { id: 'group-management', order: 64, visible: true, category: 'admin', adminSubcategory: 'user-management' },
    { id: 'user-approvals', order: 65, visible: true, category: 'admin', adminSubcategory: 'user-management' },
    { id: 'security-settings', order: 66, visible: true, category: 'admin', adminSubcategory: 'user-management' },
    { id: 'social-login', order: 67, visible: true, category: 'admin', adminSubcategory: 'user-management' },
    // CRM Configuration
    { id: 'crm-config-page', order: 68, visible: true, category: 'admin', adminSubcategory: 'crm-config' },
    { id: 'sales-config', order: 69, visible: true, category: 'admin', adminSubcategory: 'crm-config' },
    { id: 'service-desk-config', order: 70, visible: true, category: 'admin', adminSubcategory: 'crm-config' },
    { id: 'module-fields', order: 71, visible: true, category: 'admin', adminSubcategory: 'crm-config' },
    { id: 'master-data', order: 72, visible: true, category: 'admin', adminSubcategory: 'crm-config' },
    { id: 'enum-management', order: 72.5, visible: true, category: 'admin', adminSubcategory: 'crm-config' },
    { id: 'duplicate-rules', order: 73, visible: true, category: 'admin', adminSubcategory: 'crm-config' },
    { id: 'lead-score-rules', order: 74, visible: true, category: 'admin', adminSubcategory: 'crm-config' },
    { id: 'sr-definitions', order: 75, visible: true, category: 'admin', adminSubcategory: 'crm-config' },
    // AI & Integrations
    { id: 'llm-settings', order: 76, visible: true, category: 'admin', adminSubcategory: 'ai-integrations' },
    { id: 'integrations', order: 77, visible: true, category: 'admin', adminSubcategory: 'ai-integrations' },
    { id: 'analytics-settings', order: 78, visible: true, category: 'admin', adminSubcategory: 'ai-integrations' },
    { id: 'agent-management', order: 79, visible: true, category: 'admin', adminSubcategory: 'ai-integrations' },
    { id: 'agent-approvals', order: 80, visible: true, category: 'admin', adminSubcategory: 'ai-integrations' },
    { id: 'agent-analytics', order: 81, visible: true, category: 'admin', adminSubcategory: 'ai-integrations' },
    // Infrastructure
    { id: 'database-settings', order: 82, visible: true, category: 'admin', adminSubcategory: 'infrastructure' },
    { id: 'monitoring-settings', order: 83, visible: true, category: 'admin', adminSubcategory: 'infrastructure' },
    { id: 'deployment-settings', order: 84, visible: true, category: 'admin', adminSubcategory: 'infrastructure' },
    { id: 'worker-ops', order: 85, visible: true, category: 'admin', adminSubcategory: 'infrastructure' },
    // Customization
    { id: 'branding-settings', order: 86, visible: true, category: 'admin', adminSubcategory: 'customization' },
    { id: 'dashboard-settings', order: 87, visible: true, category: 'admin', adminSubcategory: 'customization' },
    { id: 'ui-customization', order: 88, visible: true, category: 'admin', adminSubcategory: 'customization' },
    { id: 'portal-config', order: 89, visible: true, category: 'admin', adminSubcategory: 'customization' },
    // Workflows
    { id: 'workflow-settings', order: 89, visible: true, category: 'admin', adminSubcategory: 'workflows' },
    { id: 'workflow-monitor', order: 90, visible: true, category: 'admin', adminSubcategory: 'workflows' },
    { id: 'workflow-templates', order: 91, visible: true, category: 'admin', adminSubcategory: 'workflows' },
    // Scripting
    { id: 'script-plugin-library', order: 91.5, visible: true, category: 'admin', adminSubcategory: 'scripting' },
    { id: 'script-plugin-new', order: 91.6, visible: true, category: 'admin', adminSubcategory: 'scripting' },
    // Developer Tools
    { id: 'api-docs', order: 92, visible: true, category: 'admin', adminSubcategory: 'developer-tools' },
    { id: 'test-results', order: 93, visible: true, category: 'admin', adminSubcategory: 'developer-tools' },
    { id: 'audit-logging', order: 94, visible: true, category: 'admin', adminSubcategory: 'developer-tools' },
    { id: 'api-users', order: 95, visible: true, category: 'admin', adminSubcategory: 'developer-tools' },
  ], []);

  // Build lookup of correct defaults keyed by item id — used for auto-healing
  const defaultItemsById = useMemo(() => {
    const map: Record<string, typeof defaultNavItemsWithCategory[0]> = {};
    defaultNavItemsWithCategory.forEach(item => { map[item.id] = item; });
    return map;
  }, [defaultNavItemsWithCategory]);

  // Set of valid admin subcategory IDs for validation
  const validSubcategoryIds = useMemo(
    () => new Set(defaultAdminSubcategories.map(s => s.id)),
    [defaultAdminSubcategories]
  );

  const mergeNavItemsWithDefaults = useCallback((savedItems: any[] | undefined) => {
    const merged = Array.isArray(savedItems) ? [...savedItems] : [];
    const existingIds = new Set(merged.map(item => item.id));

    // Auto-heal existing items: fix stale/invalid adminSubcategory values
    merged.forEach(item => {
      const defaultItem = defaultItemsById[item.id];
      if (defaultItem) {
        // If saved item has an admin subcategory that no longer exists, replace with the correct one
        if (item.category === 'admin' && item.adminSubcategory && !validSubcategoryIds.has(item.adminSubcategory)) {
          item.adminSubcategory = defaultItem.adminSubcategory;
        }
        // If item should be admin but lost its subcategory, restore it
        if (defaultItem.category === 'admin' && !item.adminSubcategory) {
          item.category = 'admin';
          item.adminSubcategory = defaultItem.adminSubcategory;
        }
      }
    });

    // Backfill any items from defaults that are missing entirely
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
  }, [defaultNavItemsWithCategory, defaultItemsById, validSubcategoryIds]);

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

  // Clear stale localStorage nav config when admin subcategory IDs have changed
  useEffect(() => {
    const NAV_CONFIG_VERSION = 'v3-2026-02-26';
    const storedVersion = localStorage.getItem('crm_nav_config_version');
    if (storedVersion !== NAV_CONFIG_VERSION) {
      localStorage.removeItem('crm_nav_order');
      localStorage.removeItem('crm_admin_menu_expanded');
      localStorage.setItem('crm_nav_config_version', NAV_CONFIG_VERSION);
    }
  }, []);

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

  // TODO-SYS007-003: Apply user's drag-reorder on top of navConfig category order
  const sortedCategories = useMemo(() => {
    if (navCategoryOrder.length === 0) return categories;
    const orderMap = new Map(navCategoryOrder.map((id: string, idx: number) => [id, idx]));
    return [...categories].sort(
      (a: { id: string }, b: { id: string }) =>
        (orderMap.has(a.id) ? (orderMap.get(a.id) as number) : 9999) -
        (orderMap.has(b.id) ? (orderMap.get(b.id) as number) : 9999)
    );
  }, [categories, navCategoryOrder]);

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

  // Build label map for SidebarCustomizer
  const navItemLabels = useMemo(() => {
    const labels: Record<string, string> = {};
    Object.entries(effectiveNavItemsConfig).forEach(([id, config]) => {
      labels[id] = config.label;
    });
    Object.entries(effectiveAdminItemsConfig).forEach(([id, config]) => {
      labels[id] = config.label;
    });
    return labels;
  }, [effectiveNavItemsConfig, effectiveAdminItemsConfig]);

  // Quick dark/light mode toggle handler
  const handleThemeToggle = useCallback(() => {
    setCrmThemeMode(effectiveTheme === 'dark' ? 'light' : 'dark');
  }, [effectiveTheme, setCrmThemeMode]);

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

  // TODO-SYS007-003: Handle category drag-end — update order state + persist
  const handleNavDragEnd = (result: DropResult) => {
    if (!result.destination || result.destination.index === result.source.index) return;
    const isAdminUser = user?.role === 'Admin' || String(user?.role) === '0' || hasPermission('canManageUsers');
    const visibleCats = sortedCategories.filter((category: { id: string }) => {
      if (category.id === 'admin') return isAdminUser;
      const items = navItemsWithCategory.filter(
        (item: { category?: string; menuName: string }) =>
          item.category === category.id && canAccessMenu(item.menuName)
      );
      return items.length > 0;
    });
    const reordered: Array<{ id: string }> = Array.from(visibleCats) as Array<{ id: string }>;
    const [moved] = reordered.splice(result.source.index, 1);
    reordered.splice(result.destination.index, 0, moved);
    const newOrder = reordered.map((c: { id: string }) => c.id);
    setNavCategoryOrder(newOrder);
    try { localStorage.setItem(NAV_ORDER_KEY, JSON.stringify(newOrder)); } catch { /* ignore */ }
  };
  
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
            <Tooltip title={effectiveTheme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}>
              <IconButton
                color="inherit"
                onClick={handleThemeToggle}
                aria-label={effectiveTheme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
                size="small"
              >
                {effectiveTheme === 'dark' ? <LightModeToggleIcon /> : <DarkModeToggleIcon />}
              </IconButton>
            </Tooltip>
            <RecentItemsDropdown size="small" showBadge={true} />
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
        aria-label="Main navigation"
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
        
        {/* Render items grouped by category — TODO-SYS007-003: DnD reorderable */}
        {(() => {
          // Pre-filter visible categories so Draggable indices are always consistent (no null renders)
          const isAdminUser = user?.role === 'Admin' || String(user?.role) === '0' || hasPermission('canManageUsers');
          const visibleCategories = sortedCategories.filter((cat: { id: string }) => {
            if (cat.id === 'admin') return isAdminUser;
            const catItems = navItemsWithCategory.filter(
              (item: { category?: string; menuName: string }) =>
                item.category === cat.id && canAccessMenu(item.menuName)
            );
            return catItems.length > 0;
          });
          return (
            <DragDropContext onDragEnd={handleNavDragEnd}>
              <Droppable droppableId="nav-categories">
                {(droppableProvided) => (
                  <div ref={droppableProvided.innerRef} {...droppableProvided.droppableProps}>
                    {visibleCategories.map((category: { id: string; label: string; order: number }, catIdx: number) => {
                      if (category.id === 'admin') {
                        const isAdminExpanded = expandedCategories['admin'] ?? true;
                        return (
                          <Draggable key={category.id} draggableId={category.id} index={catIdx}>
                            {(dragProvided, dragSnapshot) => (
                              <div
                                ref={dragProvided.innerRef}
                                {...dragProvided.draggableProps}
                                style={{ ...dragProvided.draggableProps.style, opacity: dragSnapshot.isDragging ? 0.8 : 1 }}
                              >
                                {catIdx > 0 && <Divider sx={{ my: 0.5 }} />}
                                <ListItemButton
                                  onClick={() => toggleCategory('admin')}
                                  {...dragProvided.dragHandleProps}
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
                              </div>
                            )}
                          </Draggable>
                        );
                      }

                      const categoryItems = navItemsWithCategory.filter(
                        (item: { category?: string; menuName: string }) => 
                          item.category === category.id && canAccessMenu(item.menuName)
                      );

                      const isCategoryExpanded = expandedCategories[category.id] ?? true;

                      return (
                        <Draggable key={category.id} draggableId={category.id} index={catIdx}>
                          {(dragProvided, dragSnapshot) => (
                            <div
                              ref={dragProvided.innerRef}
                              {...dragProvided.draggableProps}
                              style={{ ...dragProvided.draggableProps.style, opacity: dragSnapshot.isDragging ? 0.8 : 1 }}
                            >
                              {catIdx > 0 && <Divider sx={{ my: 0.5 }} />}
                              <ListItemButton
                                onClick={() => toggleCategory(category.id)}
                                {...dragProvided.dragHandleProps}
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
                                  {category.id === 'marketing' && <MegaphoneIcon fontSize="small" sx={{ color: 'warning.main' }} />}
                                  {category.id === 'support' && <SupportAgentIcon fontSize="small" sx={{ color: 'info.main' }} />}
                                  {category.id === 'itsm' && <SettingsIcon fontSize="small" sx={{ color: 'error.main' }} />}
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
                            </div>
                          )}
                        </Draggable>
                      );
                    })}
                    {droppableProvided.placeholder}
                  </div>
                )}
              </Droppable>
            </DragDropContext>
          );
        })()}
        </Box>

        {/* Drawer footer: Customize Navigation */}
        <Box
          sx={{
            flexShrink: 0,
            borderTop: 1,
            borderColor: 'divider',
            p: 1,
            bgcolor: 'background.paper',
          }}
        >
          <MenuItem
            onClick={() => { setDrawerOpen(false); setCustomizerOpen(true); }}
            sx={{ borderRadius: 1, py: 0.75 }}
          >
            <ListItemIcon sx={{ minWidth: 28 }}>
              <CRMConfigIcon fontSize="small" sx={{ color: 'text.secondary' }} />
            </ListItemIcon>
            <ListItemText
              primary={<Typography variant="caption" sx={{ color: 'text.secondary' }}>Customize Navigation</Typography>}
            />
          </MenuItem>
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

      {/* Sidebar Customizer */}
      <SidebarCustomizer
        open={customizerOpen}
        onClose={() => setCustomizerOpen(false)}
        categories={categories}
        navItems={navConfig?.navItems || defaultNavItemsWithCategory}
        itemLabels={navItemLabels}
      />
    </>
  );
}

function Navigation() {
  const { isAuthenticated } = useAuth();

  return isAuthenticated ? <NavigationContent /> : null;
}

export default Navigation;
