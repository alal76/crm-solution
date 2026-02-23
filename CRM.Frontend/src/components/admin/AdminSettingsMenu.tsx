import React, { useState, useEffect } from 'react';
import {
  Box,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Collapse,
  Card,
  CardContent,
  Badge,
} from '@mui/material';
import {
  Settings as SettingsIcon,
  ExpandLess,
  ExpandMore,
  Person as UserIcon,
  Flag as FlagIcon,
  Navigation as NavigationIcon,
  Description as AuditIcon,
  GroupWork as GroupIcon,
  Security as SecurityIcon,
  Login as SocialLoginIcon,
  Tune as CRMConfigIcon,
  AttachMoney as SalesIcon,
  SupportAgent as ServiceDeskIcon,
  ViewModule as ModulesIcon,
  Storage as MasterDataIcon,
  ContentCopy as DuplicateIcon,
  Score as LeadScoreIcon,
  Psychology as AIIcon,
  Extension as IntegrationsIcon,
  BarChart as AnalyticsIcon,
  Dns as DatabaseIcon,
  Monitor as MonitoringIcon,
  Cloud as DeploymentIcon,
  Memory as WorkerIcon,
  Palette as BrandingIcon,
  Dashboard as DashboardIcon,
  Widgets as UiCustomIcon,
  AccountTree as WorkflowIcon,
  Visibility as WfMonitorIcon,
  FileCopy as TemplateIcon,
  Api as ApiDocsIcon,
  BugReport as TestIcon,
  ThumbUp as ApprovalIcon,
} from '@mui/icons-material';
import { useNavigate, useLocation } from 'react-router-dom';

interface MenuItem {
  id: string;
  label: string;
  icon: React.ElementType;
  path: string;
  badge?: string;
}

interface MenuSection {
  id: string;
  title: string;
  items: MenuItem[];
}

const SECTIONS: MenuSection[] = [
  {
    id: 'system-config',
    title: 'System Configuration',
    items: [
      { id: 'general-settings', label: 'General Settings', icon: SettingsIcon, path: '/admin/config/system' },
      { id: 'feature-flags', label: 'Feature Flags', icon: FlagIcon, path: '/admin/features' },
      { id: 'navigation', label: 'Navigation', icon: NavigationIcon, path: '/admin/navigation' },
    ],
  },
  {
    id: 'user-management',
    title: 'User Management',
    items: [
      { id: 'users', label: 'Users', icon: UserIcon, path: '/admin/users' },
      { id: 'groups', label: 'Groups', icon: GroupIcon, path: '/admin/groups' },
      { id: 'approvals', label: 'Approvals', icon: ApprovalIcon, path: '/admin/approvals' },
      { id: 'security', label: 'Security', icon: SecurityIcon, path: '/admin/security' },
      { id: 'social-login', label: 'Social Login', icon: SocialLoginIcon, path: '/admin/social-login' },
    ],
  },
  {
    id: 'crm-config',
    title: 'CRM Configuration',
    items: [
      { id: 'crm-config-page', label: 'CRM Config', icon: CRMConfigIcon, path: '/admin/config/crm' },
      { id: 'sales-config', label: 'Sales Config', icon: SalesIcon, path: '/admin/settings/sales' },
      { id: 'service-desk', label: 'Service Desk', icon: ServiceDeskIcon, path: '/admin/settings/service-desk' },
      { id: 'modules-fields', label: 'Modules & Fields', icon: ModulesIcon, path: '/admin/modules' },
      { id: 'master-data', label: 'Master Data', icon: MasterDataIcon, path: '/admin/master-data' },
      { id: 'duplicate-rules', label: 'Duplicate Rules', icon: DuplicateIcon, path: '/admin/duplicate-rules' },
      { id: 'lead-score-rules', label: 'Lead Score Rules', icon: LeadScoreIcon, path: '/admin/lead-score-rules' },
    ],
  },
  {
    id: 'ai-integrations',
    title: 'AI & Integrations',
    items: [
      { id: 'llm-settings', label: 'AI / LLM Settings', icon: AIIcon, path: '/admin/llm' },
      { id: 'integrations', label: 'Integrations', icon: IntegrationsIcon, path: '/admin/integrations' },
      { id: 'analytics', label: 'Analytics', icon: AnalyticsIcon, path: '/admin/analytics' },
    ],
  },
  {
    id: 'infrastructure',
    title: 'Infrastructure',
    items: [
      { id: 'database', label: 'Database', icon: DatabaseIcon, path: '/admin/database' },
      { id: 'monitoring', label: 'Monitoring', icon: MonitoringIcon, path: '/admin/monitoring' },
      { id: 'deployment', label: 'Deployment', icon: DeploymentIcon, path: '/admin/deployment' },
      { id: 'workers', label: 'Workers', icon: WorkerIcon, path: '/admin/workers' },
    ],
  },
  {
    id: 'customization',
    title: 'Customization',
    items: [
      { id: 'branding', label: 'Branding', icon: BrandingIcon, path: '/admin/branding' },
      { id: 'dashboards', label: 'Dashboards', icon: DashboardIcon, path: '/admin/dashboards' },
      { id: 'ui-custom', label: 'UI Custom', icon: UiCustomIcon, path: '/admin/ui-customization' },
    ],
  },
  {
    id: 'workflows',
    title: 'Workflows',
    items: [
      { id: 'workflow-list', label: 'Workflow List', icon: WorkflowIcon, path: '/admin/workflows' },
      { id: 'workflow-monitor', label: 'Monitor', icon: WfMonitorIcon, path: '/admin/workflows/monitor' },
      { id: 'workflow-templates', label: 'Templates', icon: TemplateIcon, path: '/admin/workflows/templates' },
    ],
  },
  {
    id: 'developer-tools',
    title: 'Developer Tools',
    items: [
      { id: 'api-docs', label: 'API Docs', icon: ApiDocsIcon, path: '/admin/api-docs' },
      { id: 'test-results', label: 'Test Results', icon: TestIcon, path: '/admin/test-results' },
      { id: 'audit', label: 'Audit Logging', icon: AuditIcon, path: '/admin/audit' },
    ],
  },
];

/**
 * Admin Settings Menu with 8 collapsible sections.
 * Persists expanded state in localStorage.
 */
export const AdminSettingsMenu: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();

  const [expanded, setExpanded] = useState<Record<string, boolean>>(() => {
    try {
      const saved = localStorage.getItem('crm_admin_menu_expanded');
      if (saved) return JSON.parse(saved);
    } catch { /* ignore */ }
    return { 'system-config': true };
  });

  useEffect(() => {
    localStorage.setItem('crm_admin_menu_expanded', JSON.stringify(expanded));
  }, [expanded]);

  const toggle = (id: string) => setExpanded(prev => ({ ...prev, [id]: !prev[id] }));

  return (
    <Box sx={{ display: 'flex', height: '100%' }}>
      <Card
        sx={{
          width: 280,
          boxShadow: 1,
          borderRadius: 0,
          display: 'flex',
          flexDirection: 'column',
          maxHeight: '100vh',
          overflow: 'auto',
        }}
      >
        <CardContent sx={{ p: 1.5, flexGrow: 1, overflow: 'auto' }}>
          {SECTIONS.map((section) => {
            const isOpen = !!expanded[section.id];
            const hasActive = section.items.some(i => location.pathname === i.path);

            return (
              <Box key={section.id} sx={{ mb: 0.5 }}>
                {/* Section header */}
                <ListItemButton
                  onClick={() => toggle(section.id)}
                  sx={{
                    py: 0.75,
                    px: 1.5,
                    borderRadius: 1,
                    bgcolor: hasActive ? 'primary.50' : 'transparent',
                    '&:hover': { bgcolor: 'grey.100' },
                  }}
                >
                  <ListItemText
                    primary={section.title}
                    primaryTypographyProps={{
                      variant: 'caption',
                      fontWeight: 700,
                      textTransform: 'uppercase',
                      letterSpacing: 0.5,
                      color: hasActive ? 'primary.main' : 'text.secondary',
                    }}
                  />
                  {isOpen ? <ExpandLess fontSize="small" /> : <ExpandMore fontSize="small" />}
                </ListItemButton>

                {/* Items */}
                <Collapse in={isOpen} timeout={200} unmountOnExit>
                  <List component="div" disablePadding dense>
                    {section.items.map(item => {
                      const isActive = location.pathname === item.path;
                      const Icon = item.icon;
                      return (
                        <ListItemButton
                          key={item.id}
                          onClick={() => navigate(item.path)}
                          selected={isActive}
                          sx={{
                            pl: 3,
                            py: 0.75,
                            borderLeft: '3px solid',
                            borderLeftColor: isActive ? 'primary.main' : 'transparent',
                            bgcolor: isActive ? 'primary.light' : 'transparent',
                            '&:hover': { bgcolor: isActive ? 'primary.light' : 'grey.100' },
                            '&.Mui-selected': { bgcolor: 'primary.light' },
                            '&.Mui-selected:hover': { bgcolor: 'primary.light' },
                            transition: 'all 0.15s ease',
                          }}
                        >
                          <ListItemIcon sx={{ minWidth: 32, color: 'inherit' }}>
                            {item.badge ? (
                              <Badge badgeContent={item.badge} color="error" overlap="circular">
                                <Icon sx={{ fontSize: '1.1rem' }} />
                              </Badge>
                            ) : (
                              <Icon sx={{ fontSize: '1.1rem' }} />
                            )}
                          </ListItemIcon>
                          <ListItemText
                            primary={item.label}
                            primaryTypographyProps={{
                              fontSize: '0.85rem',
                              fontWeight: isActive ? 600 : 500,
                              color: isActive ? 'primary.main' : 'text.primary',
                            }}
                          />
                        </ListItemButton>
                      );
                    })}
                  </List>
                </Collapse>
              </Box>
            );
          })}
        </CardContent>
      </Card>
    </Box>
  );
};

export default AdminSettingsMenu;
