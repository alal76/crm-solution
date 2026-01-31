/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Grid,
  Card,
  CardContent,
  CardHeader,
  Typography,
  Box,
  CircularProgress,
  Alert,
  Skeleton,
  Chip,
  Tabs,
  Tab,
  IconButton,
  Tooltip,
} from '@mui/material';
import {
  TrendingUp as TrendingUpIcon,
  TrendingDown as TrendingDownIcon,
  People as PeopleIcon,
  ShoppingCart as ShoppingCartIcon,
  Campaign as CampaignIcon,
  AttachMoney as MoneyIcon,
  Schedule as ScheduleIcon,
  Business as BusinessIcon,
  CalendarMonth as CalendarIcon,
  Assessment as AssessmentIcon,
  Help as HelpIcon,
  Assignment as AssignmentIcon,
  Refresh as RefreshIcon,
  Settings as SettingsIcon,
  Inventory as ProductIcon,
} from '@mui/icons-material';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip as RechartsTooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  BarChart,
  Bar,
  Legend,
  AreaChart,
  Area,
} from 'recharts';
import logo from '../assets/logo.png';
import { 
  dashboardConfigService, 
  dashboardDataService, 
  DashboardDetail, 
  DashboardWidget, 
  DashboardStats,
  PipelineSummary,
  WidgetType,
} from '../services/dashboardService';
import { opportunityService, campaignService, customerService, Opportunity, Customer } from '../services/apiService';
import { useProfile } from '../contexts/ProfileContext';

// Icon mapping for dynamic icons
const iconMap: Record<string, React.ElementType> = {
  TrendingUp: TrendingUpIcon,
  People: PeopleIcon,
  ShoppingCart: ShoppingCartIcon,
  Campaign: CampaignIcon,
  AttachMoney: MoneyIcon,
  Schedule: ScheduleIcon,
  Business: BusinessIcon,
  CalendarMonth: CalendarIcon,
  Assessment: AssessmentIcon,
  Help: HelpIcon,
  Assignment: AssignmentIcon,
  Inventory: ProductIcon,
};

// Stage names for display
const stageNames: Record<number, string> = {
  0: 'Discovery',
  1: 'Qualification',
  2: 'Proposal',
  3: 'Negotiation',
  4: 'Closed Won',
  5: 'Closed Lost',
};

// Stage colors
const stageColors: Record<number, string> = {
  0: '#9E9E9E',
  1: '#2196F3',
  2: '#FF9800',
  3: '#9C27B0',
  4: '#4CAF50',
  5: '#F44336',
};

const pieChartColors = ['#06A77D', '#B3261E', '#F57C00', '#0092BC', '#6750A4', '#9C27B0'];

interface WidgetData {
  value?: number | string;
  items?: Opportunity[];
  chartData?: { name?: string; month?: string; value: number; count?: number; color?: string }[];
  trend?: number;
}

interface StatCardProps {
  title: string;
  value: string | number;
  icon?: React.ElementType;
  color: string;
  loading?: boolean;
  onClick?: () => void;
  clickable?: boolean;
  trend?: number;
  subtitle?: string;
}

const StatCard = ({ title, value, icon: Icon, color, loading, onClick, clickable, trend, subtitle }: StatCardProps) => (
  <Card
    onClick={clickable ? onClick : undefined}
    sx={{
      height: '100%',
      borderRadius: 3,
      background: 'linear-gradient(135deg, #F5EFF7 0%, #FFFBFE 100%)',
      border: `2px solid ${color}20`,
      cursor: clickable ? 'pointer' : 'default',
      transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
      '&:hover': {
        transform: clickable ? 'translateY(-4px)' : 'none',
        boxShadow: clickable ? `0px 12px 24px ${color}20` : 1,
        border: `2px solid ${color}40`,
      },
    }}
  >
    <CardContent sx={{ position: 'relative', py: 3 }}>
      <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
        <Box>
          <Typography color="textSecondary" sx={{ fontSize: '0.875rem', fontWeight: 500, mb: 1 }}>
            {title}
            {clickable && (
              <Typography component="span" sx={{ fontSize: '0.7rem', ml: 1, color: color }}>
                (Click to view)
              </Typography>
            )}
          </Typography>
          <Typography variant="h4" sx={{ fontWeight: 700, color: color }}>
            {loading ? <Skeleton width={80} /> : value}
          </Typography>
          {subtitle && (
            <Typography variant="caption" color="textSecondary">
              {subtitle}
            </Typography>
          )}
          {trend !== undefined && trend !== 0 && (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}>
              {trend > 0 ? (
                <TrendingUpIcon sx={{ fontSize: 16, color: '#06A77D' }} />
              ) : (
                <TrendingDownIcon sx={{ fontSize: 16, color: '#F44336' }} />
              )}
              <Typography 
                variant="caption" 
                sx={{ color: trend > 0 ? '#06A77D' : '#F44336', fontWeight: 600 }}
              >
                {trend > 0 ? '+' : ''}{trend.toFixed(1)}%
              </Typography>
            </Box>
          )}
        </Box>
        {Icon && (
          <Box
            sx={{
              p: 1.5,
              borderRadius: 2,
              backgroundColor: `${color}15`,
            }}
          >
            <Icon sx={{ fontSize: 32, color }} />
          </Box>
        )}
      </Box>
    </CardContent>
  </Card>
);

function DashboardPage() {
  const navigate = useNavigate();
  const { canAccessMenu } = useProfile();
  
  // Dashboard configuration state
  const [dashboards, setDashboards] = useState<DashboardDetail[]>([]);
  const [activeDashboardIndex, setActiveDashboardIndex] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  
  // Data state
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [pipeline, setPipeline] = useState<PipelineSummary | null>(null);
  const [opportunities, setOpportunities] = useState<Opportunity[]>([]);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [campaignCount, setCampaignCount] = useState(0);
  
  // Derived state
  const activeDashboard = dashboards[activeDashboardIndex];

  const formatCurrency = (amount: number) => {
    if (amount >= 1000000) return `$${(amount / 1000000).toFixed(1)}M`;
    if (amount >= 1000) return `$${(amount / 1000).toFixed(1)}K`;
    return `$${amount.toLocaleString()}`;
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return 'Not set';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
  };

  // Load dashboard configurations
  const loadDashboards = useCallback(async () => {
    try {
      const response = await dashboardConfigService.getDashboards();
      const dashboardList = response.data;
      
      if (dashboardList.length === 0) {
        // Try to initialize default dashboards
        try {
          await dashboardConfigService.initializeDefaults();
          const retryResponse = await dashboardConfigService.getDashboards();
          if (retryResponse.data.length > 0) {
            const details = await Promise.all(
              retryResponse.data.map(d => dashboardConfigService.getDashboard(d.id))
            );
            setDashboards(details.map(r => r.data));
          }
        } catch (initErr) {
          console.warn('Could not initialize default dashboards');
        }
      } else {
        const details = await Promise.all(
          dashboardList.map(d => dashboardConfigService.getDashboard(d.id))
        );
        setDashboards(details.map(r => r.data));
        
        const defaultIndex = dashboardList.findIndex(d => d.isDefault);
        if (defaultIndex >= 0) {
          setActiveDashboardIndex(defaultIndex);
        }
      }
    } catch (err) {
      console.warn('Could not load dashboard configurations, using fallback');
    }
  }, []);

  // Load dashboard data
  const loadData = useCallback(async () => {
    try {
      setLoading(true);
      
      try {
        const statsResponse = await dashboardDataService.getStats();
        setStats(statsResponse.data);
      } catch (err) {
        console.warn('Could not load dashboard stats');
      }

      try {
        const pipelineResponse = await dashboardDataService.getPipeline();
        setPipeline(pipelineResponse.data);
      } catch (err) {
        console.warn('Could not load pipeline data');
      }

      try {
        const oppsResponse = await opportunityService.getAll();
        setOpportunities(oppsResponse.data || []);
      } catch (err) {
        console.warn('Could not load opportunities');
      }

      try {
        const customersResponse = await customerService.getAll();
        setCustomers(customersResponse.data || []);
      } catch (err) {
        console.warn('Could not load customers');
      }

      try {
        const campaignsResponse = await campaignService.getActive();
        setCampaignCount(campaignsResponse.data.length);
      } catch (err) {
        console.warn('Could not load campaigns');
      }

    } catch (err) {
      console.error('Error loading dashboard data:', err);
      setError('Failed to load dashboard data');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadDashboards();
    loadData();
  }, [loadDashboards, loadData]);

  // Compute widget data based on data source
  const getWidgetValue = useCallback((dataSource: string): WidgetData => {
    switch (dataSource) {
      case 'customers.count':
        return { value: stats?.customers?.total ?? customers.length };
      case 'contacts.count':
        return { value: stats?.contacts?.total ?? 0 };
      case 'opportunities.count':
        return { value: stats?.opportunities?.total ?? opportunities.length };
      case 'opportunities.pipeline_value':
        return { value: stats?.opportunities?.openValue ?? 0 };
      case 'opportunities.won_value':
        return { value: stats?.opportunities?.wonValue ?? 0 };
      case 'opportunities.by_stage':
        return { 
          chartData: pipeline?.stages?.map((s, i) => ({
            name: s.stage,
            value: s.totalValue,
            count: s.count,
            color: pieChartColors[i % pieChartColors.length]
          })) || []
        };
      case 'opportunities.pipeline_trend': {
        const monthData: Record<string, number> = {};
        const now = new Date();
        for (let i = 5; i >= 0; i--) {
          const date = new Date(now.getFullYear(), now.getMonth() - i, 1);
          const key = date.toLocaleDateString('en-US', { month: 'short' });
          monthData[key] = 0;
        }
        opportunities.forEach(opp => {
          if (opp.createdAt) {
            const date = new Date(opp.createdAt);
            const key = date.toLocaleDateString('en-US', { month: 'short' });
            if (monthData[key] !== undefined) {
              monthData[key] += opp.amount || 0;
            }
          }
        });
        return {
          chartData: Object.entries(monthData).map(([month, value]) => ({ month, value }))
        };
      }
      case 'opportunities.recent':
        return {
          items: opportunities
            .filter(opp => opp.stage < 4)
            .sort((a, b) => new Date(b.createdAt || 0).getTime() - new Date(a.createdAt || 0).getTime())
            .slice(0, 5)
        };
      case 'campaigns.active':
        return { value: campaignCount };
      case 'products.count':
        return { value: stats?.products?.total ?? 0 };
      case 'tasks.count':
        return { value: stats?.tasks?.total ?? 0 };
      case 'tasks.pending':
        return { value: stats?.tasks?.pending ?? 0 };
      case 'users.active':
        return { value: stats?.users?.active ?? 0 };
      default:
        return { value: '--' };
    }
  }, [stats, pipeline, opportunities, customers, campaignCount]);

  // Render widget based on type
  const renderWidget = (widget: DashboardWidget) => {
    const data = getWidgetValue(widget.dataSource);
    const IconComponent = widget.iconName ? iconMap[widget.iconName] : AssessmentIcon;
    const color = widget.color || '#6750A4';
    const navPath = widget.navigationLink?.split('/')[1] || '';
    const canNavigate = widget.navigationLink && canAccessMenu(navPath);

    switch (widget.widgetTypeValue) {
      case WidgetType.StatCard:
      case WidgetType.KPICard: {
        const displayValue = typeof data.value === 'number' 
          ? (widget.dataSource.includes('value') || widget.dataSource.includes('budget')
            ? formatCurrency(data.value)
            : data.value.toLocaleString())
          : data.value;
        
        return (
          <StatCard
            title={widget.title}
            value={displayValue ?? '--'}
            icon={IconComponent}
            color={color}
            loading={loading}
            clickable={!!canNavigate}
            onClick={() => canNavigate && widget.navigationLink && navigate(widget.navigationLink)}
            trend={widget.showTrend ? data.trend : undefined}
            subtitle={widget.subtitle}
          />
        );
      }

      case WidgetType.LineChart:
      case WidgetType.AreaChart:
        return (
          <Card sx={{ height: '100%', borderRadius: 3, boxShadow: 1 }}>
            <CardHeader
              title={widget.title}
              subheader={widget.subtitle}
              titleTypographyProps={{ variant: 'h6', sx: { fontWeight: 600 } }}
              subheaderTypographyProps={{ variant: 'caption' }}
              sx={{ borderBottom: '1px solid #E0E0E0' }}
            />
            <CardContent>
              {loading ? (
                <Skeleton variant="rectangular" height={250} sx={{ borderRadius: 2 }} />
              ) : (
                <ResponsiveContainer width="100%" height={250}>
                  {widget.widgetTypeValue === WidgetType.AreaChart ? (
                    <AreaChart data={data.chartData || []}>
                      <defs>
                        <linearGradient id={`colorArea-${widget.id}`} x1="0" y1="0" x2="0" y2="1">
                          <stop offset="5%" stopColor={color} stopOpacity={0.8} />
                          <stop offset="95%" stopColor={color} stopOpacity={0.1} />
                        </linearGradient>
                      </defs>
                      <CartesianGrid strokeDasharray="3 3" stroke="#E0E0E0" />
                      <XAxis dataKey="month" stroke="#79747E" />
                      <YAxis stroke="#79747E" tickFormatter={(v) => formatCurrency(v)} />
                      <RechartsTooltip
                        formatter={(value: number) => [formatCurrency(value), 'Value']}
                        contentStyle={{ backgroundColor: '#FFFBFE', border: '1px solid #E0E0E0', borderRadius: '8px' }}
                      />
                      <Area type="monotone" dataKey="value" stroke={color} fill={`url(#colorArea-${widget.id})`} />
                    </AreaChart>
                  ) : (
                    <LineChart data={data.chartData || []}>
                      <CartesianGrid strokeDasharray="3 3" stroke="#E0E0E0" />
                      <XAxis dataKey="month" stroke="#79747E" />
                      <YAxis stroke="#79747E" tickFormatter={(v) => formatCurrency(v)} />
                      <RechartsTooltip
                        formatter={(value: number) => [formatCurrency(value), 'Value']}
                        contentStyle={{ backgroundColor: '#FFFBFE', border: '1px solid #E0E0E0', borderRadius: '8px' }}
                      />
                      <Line type="monotone" dataKey="value" stroke={color} strokeWidth={2} dot={{ fill: color, r: 5 }} />
                    </LineChart>
                  )}
                </ResponsiveContainer>
              )}
            </CardContent>
          </Card>
        );

      case WidgetType.BarChart:
      case WidgetType.StackedBarChart:
        return (
          <Card sx={{ height: '100%', borderRadius: 3, boxShadow: 1 }}>
            <CardHeader
              title={widget.title}
              subheader={widget.subtitle}
              titleTypographyProps={{ variant: 'h6', sx: { fontWeight: 600 } }}
              subheaderTypographyProps={{ variant: 'caption' }}
              sx={{ borderBottom: '1px solid #E0E0E0' }}
            />
            <CardContent>
              {loading ? (
                <Skeleton variant="rectangular" height={250} sx={{ borderRadius: 2 }} />
              ) : (
                <ResponsiveContainer width="100%" height={250}>
                  <BarChart data={data.chartData || []}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#E0E0E0" />
                    <XAxis dataKey="name" stroke="#79747E" />
                    <YAxis stroke="#79747E" />
                    <RechartsTooltip contentStyle={{ backgroundColor: '#FFFBFE', border: '1px solid #E0E0E0', borderRadius: '8px' }} />
                    <Legend />
                    <Bar dataKey="value" name="Value" fill={color} radius={[4, 4, 0, 0]} />
                    <Bar dataKey="count" name="Count" fill="#06A77D" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              )}
            </CardContent>
          </Card>
        );

      case WidgetType.PieChart:
        return (
          <Card sx={{ height: '100%', borderRadius: 3, boxShadow: 1 }}>
            <CardHeader
              title={widget.title}
              subheader={widget.subtitle}
              titleTypographyProps={{ variant: 'h6', sx: { fontWeight: 600 } }}
              subheaderTypographyProps={{ variant: 'caption' }}
              sx={{ borderBottom: '1px solid #E0E0E0' }}
            />
            <CardContent sx={{ display: 'flex', justifyContent: 'center' }}>
              {loading ? (
                <CircularProgress />
              ) : (
                <ResponsiveContainer width="100%" height={220}>
                  <PieChart>
                    <Pie
                      data={data.chartData || []}
                      cx="50%"
                      cy="50%"
                      labelLine={false}
                      label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                      outerRadius={80}
                      fill="#8884d8"
                      dataKey="value"
                    >
                      {(data.chartData || []).map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={entry.color || pieChartColors[index % pieChartColors.length]} />
                      ))}
                    </Pie>
                    <RechartsTooltip />
                  </PieChart>
                </ResponsiveContainer>
              )}
            </CardContent>
          </Card>
        );

      case WidgetType.DataTable:
      case WidgetType.ActivityList: {
        const items = data.items || [];
        return (
          <Card sx={{ height: '100%', borderRadius: 3, boxShadow: 1 }}>
            <CardHeader
              title={widget.title}
              subheader={widget.subtitle}
              titleTypographyProps={{ variant: 'h6', sx: { fontWeight: 600 } }}
              subheaderTypographyProps={{ variant: 'caption' }}
              sx={{ borderBottom: '1px solid #E0E0E0' }}
            />
            <CardContent>
              {loading ? (
                <Box>
                  {[1, 2, 3].map((i) => (
                    <Skeleton key={i} height={60} sx={{ mb: 1, borderRadius: 2 }} />
                  ))}
                </Box>
              ) : items.length === 0 ? (
                <Box sx={{ textAlign: 'center', py: 4 }}>
                  <Typography color="textSecondary">No data available</Typography>
                </Box>
              ) : (
                <Box>
                  {items.map((item, index) => (
                    <Card
                      key={item.id || index}
                      onClick={() => canAccessMenu('Opportunities') && navigate('/opportunities')}
                      sx={{
                        mb: 1.5,
                        p: 2,
                        borderRadius: 2,
                        cursor: canAccessMenu('Opportunities') ? 'pointer' : 'default',
                        border: '1px solid #E0E0E0',
                        transition: 'all 0.2s ease-in-out',
                        '&:hover': canAccessMenu('Opportunities') ? {
                          boxShadow: 2,
                          borderColor: color,
                        } : {},
                      }}
                    >
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <Box sx={{ flex: 1 }}>
                          <Typography variant="subtitle2" fontWeight={600}>
                            {item.name}
                          </Typography>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 0.5 }}>
                            <Chip
                              label={stageNames[item.stage] || `Stage ${item.stage}`}
                              size="small"
                              sx={{
                                backgroundColor: `${stageColors[item.stage] || '#9E9E9E'}20`,
                                color: stageColors[item.stage] || '#9E9E9E',
                                fontWeight: 600,
                                fontSize: '0.7rem',
                              }}
                            />
                            <Typography variant="caption" color="textSecondary">
                              {item.accountName || 'Unknown'}
                            </Typography>
                          </Box>
                        </Box>
                        <Box sx={{ textAlign: 'right' }}>
                          <Typography variant="subtitle1" sx={{ fontWeight: 700, color: '#06A77D' }}>
                            {formatCurrency(item.amount)}
                          </Typography>
                          <Typography variant="caption" color="textSecondary">
                            {item.probability}% · {formatDate(item.expectedCloseDate)}
                          </Typography>
                        </Box>
                      </Box>
                    </Card>
                  ))}
                </Box>
              )}
            </CardContent>
          </Card>
        );
      }

      default:
        return (
          <Card sx={{ height: '100%', borderRadius: 3, boxShadow: 1 }}>
            <CardContent>
              <Typography color="textSecondary">
                Widget type not supported: {widget.widgetType}
              </Typography>
            </CardContent>
          </Card>
        );
    }
  };

  // Calculate grid column span based on widget columnSpan
  const getGridSize = (span: number) => {
    const columnCount = activeDashboard?.columnCount || 3;
    return Math.min(12, Math.floor(12 / columnCount) * span);
  };

  // Fallback dashboard if no configurations exist
  const renderFallbackDashboard = () => {
    const totalPipeline = stats?.opportunities?.openValue ?? 0;
    const totalRevenue = stats?.opportunities?.wonValue ?? 0;
    const totalCustomers = stats?.customers?.total ?? customers.length;

    const fallbackStats = [
      { title: 'Total Pipeline', value: formatCurrency(totalPipeline), icon: TrendingUpIcon, color: '#6750A4', link: '/opportunities', menuKey: 'Opportunities' },
      { title: 'Active Campaigns', value: campaignCount, icon: CampaignIcon, color: '#06A77D', link: '/campaigns', menuKey: 'Campaigns' },
      { title: 'Accounts', value: totalCustomers.toLocaleString(), icon: PeopleIcon, color: '#0092BC', link: '/customers', menuKey: 'Customers' },
      { title: 'Total Revenue', value: formatCurrency(totalRevenue), icon: ShoppingCartIcon, color: '#F57C00', link: '/opportunities', menuKey: 'Opportunities' },
    ];

    // Build pipeline trend from opportunities
    const monthData: Record<string, number> = {};
    const now = new Date();
    for (let i = 5; i >= 0; i--) {
      const date = new Date(now.getFullYear(), now.getMonth() - i, 1);
      const key = date.toLocaleDateString('en-US', { month: 'short' });
      monthData[key] = 0;
    }
    opportunities.forEach(opp => {
      if (opp.createdAt) {
        const date = new Date(opp.createdAt);
        const key = date.toLocaleDateString('en-US', { month: 'short' });
        if (monthData[key] !== undefined) {
          monthData[key] += opp.amount || 0;
        }
      }
    });
    const trendData = Object.entries(monthData).map(([month, value]) => ({ month, value }));

    // Build stage distribution
    const stageData = pipeline?.stages?.map((s, i) => ({
      name: s.stage,
      value: s.totalValue,
      count: s.count,
      color: pieChartColors[i % pieChartColors.length]
    })) || [];

    return (
      <>
        <Grid container spacing={3} sx={{ mb: 4 }}>
          {fallbackStats.map((stat, index) => (
            <Grid item xs={12} sm={6} md={3} key={index}>
              <StatCard
                {...stat}
                loading={loading}
                clickable={canAccessMenu(stat.menuKey)}
                onClick={() => canAccessMenu(stat.menuKey) && navigate(stat.link)}
              />
            </Grid>
          ))}
        </Grid>

        <Grid container spacing={3}>
          <Grid item xs={12} md={8}>
            <Card sx={{ borderRadius: 3, boxShadow: 1 }}>
              <CardHeader
                title="Pipeline Trend"
                subheader="Last 6 months"
                titleTypographyProps={{ variant: 'h6', sx: { fontWeight: 600 } }}
                subheaderTypographyProps={{ variant: 'caption' }}
                sx={{ borderBottom: '1px solid #E0E0E0' }}
              />
              <CardContent>
                {loading ? (
                  <Skeleton variant="rectangular" height={300} sx={{ borderRadius: 2 }} />
                ) : (
                  <ResponsiveContainer width="100%" height={300}>
                    <LineChart data={trendData}>
                      <CartesianGrid strokeDasharray="3 3" stroke="#E0E0E0" />
                      <XAxis dataKey="month" stroke="#79747E" />
                      <YAxis stroke="#79747E" tickFormatter={(v) => formatCurrency(v)} />
                      <RechartsTooltip
                        formatter={(value: number) => [formatCurrency(value), 'Pipeline']}
                        contentStyle={{ backgroundColor: '#FFFBFE', border: '1px solid #E0E0E0', borderRadius: '8px' }}
                      />
                      <Line type="monotone" dataKey="value" stroke="#6750A4" strokeWidth={2} dot={{ fill: '#6750A4', r: 5 }} />
                    </LineChart>
                  </ResponsiveContainer>
                )}
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} md={4}>
            <Card sx={{ borderRadius: 3, boxShadow: 1 }}>
              <CardHeader
                title="Opportunities by Stage"
                titleTypographyProps={{ variant: 'h6', sx: { fontWeight: 600 } }}
                sx={{ borderBottom: '1px solid #E0E0E0' }}
              />
              <CardContent sx={{ display: 'flex', justifyContent: 'center' }}>
                {loading ? (
                  <CircularProgress />
                ) : (
                  <ResponsiveContainer width="100%" height={250}>
                    <PieChart>
                      <Pie
                        data={stageData}
                        cx="50%"
                        cy="50%"
                        labelLine={false}
                        label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                        outerRadius={80}
                        fill="#8884d8"
                        dataKey="value"
                      >
                        {stageData.map((entry, index) => (
                          <Cell key={`cell-${index}`} fill={entry.color || pieChartColors[index % pieChartColors.length]} />
                        ))}
                      </Pie>
                      <RechartsTooltip />
                    </PieChart>
                  </ResponsiveContainer>
                )}
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      </>
    );
  };

  return (
    <Box sx={{ py: 2 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
            <img src={logo} alt="CRM Logo" style={{ width: '100%', height: '100%', objectFit: 'contain' }} />
          </Box>
          <Box>
            <Typography variant="h3" sx={{ fontWeight: 700, mb: 0.5 }}>
              {activeDashboard?.name || 'Dashboard'}
            </Typography>
            <Typography color="textSecondary" variant="body2">
              {activeDashboard?.description || "Welcome back! Here's your performance overview."}
            </Typography>
          </Box>
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Tooltip title="Refresh">
            <IconButton onClick={loadData} disabled={loading}>
              <RefreshIcon />
            </IconButton>
          </Tooltip>
          {canAccessMenu('DashboardSettings') && (
            <Tooltip title="Dashboard Settings">
              <IconButton onClick={() => navigate('/admin/dashboards')}>
                <SettingsIcon />
              </IconButton>
            </Tooltip>
          )}
        </Box>
      </Box>

      {/* Dashboard Tabs */}
      {dashboards.length > 1 && (
        <Tabs
          value={activeDashboardIndex}
          onChange={(_, v) => setActiveDashboardIndex(v)}
          sx={{ mb: 3, borderBottom: 1, borderColor: 'divider' }}
        >
          {dashboards.map((dashboard) => (
            <Tab
              key={dashboard.id}
              label={dashboard.name}
              icon={dashboard.isDefault ? <Chip label="Default" size="small" sx={{ ml: 1 }} /> : undefined}
              iconPosition="end"
            />
          ))}
        </Tabs>
      )}

      {/* Error Alert */}
      {error && (
        <Alert severity="error" sx={{ mb: 3, borderRadius: 2 }} onClose={() => setError('')}>
          {error}
        </Alert>
      )}

      {/* Dashboard Content */}
      {activeDashboard && activeDashboard.widgets.length > 0 ? (
        <Grid container spacing={3}>
          {activeDashboard.widgets
            .filter(w => w.isVisible)
            .sort((a, b) => a.displayOrder - b.displayOrder)
            .map((widget) => (
              <Grid
                item
                xs={12}
                sm={getGridSize(widget.columnSpan) >= 6 ? 12 : 6}
                md={getGridSize(widget.columnSpan)}
                key={widget.id}
                sx={{ minHeight: widget.rowSpan > 1 ? 300 * widget.rowSpan : undefined }}
              >
                {renderWidget(widget)}
              </Grid>
            ))}
        </Grid>
      ) : (
        renderFallbackDashboard()
      )}
    </Box>
  );
}

export default DashboardPage;
