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

import { useState, useEffect } from 'react';
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
  LinearProgress,
  Avatar,
  Divider,
} from '@mui/material';
import {
  TrendingUp as TrendingUpIcon,
  People as PeopleIcon,
  ShoppingCart as ShoppingCartIcon,
  Campaign as CampaignIcon,
  AttachMoney as MoneyIcon,
  Schedule as ScheduleIcon,
  Business as BusinessIcon,
  CalendarMonth as CalendarIcon,
} from '@mui/icons-material';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  BarChart,
  Bar,
  Legend,
} from 'recharts';
import logo from '../assets/logo.png';
import { opportunityService, campaignService, customerService, Opportunity, Customer } from '../services/apiService';
import { useProfile } from '../contexts/ProfileContext';

// Stage names for display
const stageNames: Record<number, string> = {
  0: 'Prospecting',
  1: 'Qualification',
  2: 'Needs Analysis',
  3: 'Value Proposition',
  4: 'Decision Makers',
  5: 'Perception Analysis',
  6: 'Proposal/Quote',
  7: 'Negotiation',
  8: 'Verbal Commitment',
  9: 'Contract Sent',
  10: 'Closed Won',
  11: 'Closed Lost',
  12: 'On Hold',
  13: 'Disqualified',
};

// Stage colors for visual distinction
const stageColors: Record<number, string> = {
  0: '#9E9E9E',
  1: '#2196F3',
  2: '#03A9F4',
  3: '#00BCD4',
  4: '#009688',
  5: '#4CAF50',
  6: '#8BC34A',
  7: '#CDDC39',
  8: '#FFC107',
  9: '#FF9800',
  10: '#06A77D',
  11: '#B3261E',
  12: '#795548',
  13: '#607D8B',
};

// Sample data for charts
const pipelineData = [
  { month: 'Jan', value: 40000 },
  { month: 'Feb', value: 50000 },
  { month: 'Mar', value: 45000 },
  { month: 'Apr', value: 65000 },
  { month: 'May', value: 75000 },
  { month: 'Jun', value: 85000 },
];

const statusData = [
  { name: 'Won', value: 400, color: '#06A77D' },
  { name: 'Lost', value: 200, color: '#B3261E' },
  { name: 'Pending', value: 300, color: '#F57C00' },
  { name: 'Negotiating', value: 250, color: '#0092BC' },
];

const StatCard = ({ title, value, icon: Icon, color, loading, onClick, clickable }) => (
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
        </Box>
        <Box
          sx={{
            p: 1.5,
            borderRadius: 2,
            backgroundColor: `${color}15`,
          }}
        >
          <Icon sx={{ fontSize: 32, color }} />
        </Box>
      </Box>
    </CardContent>
  </Card>
);

function DashboardPage() {
  const navigate = useNavigate();
  const { canAccessMenu } = useProfile();
  const [totalPipeline, setTotalPipeline] = useState(0);
  const [activeCampaigns, setActiveCampaigns] = useState(0);
  const [totalCustomers, setTotalCustomers] = useState(0);
  const [totalRevenue, setTotalRevenue] = useState(0);
  const [opportunities, setOpportunities] = useState<Opportunity[]>([]);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [pipelineByMonth, setPipelineByMonth] = useState<{month: string; value: number; count: number}[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadDashboardData();
  }, []);

  // Helper function to format currency
  const formatCurrency = (amount: number) => {
    if (amount >= 1000000) {
      return `$${(amount / 1000000).toFixed(1)}M`;
    } else if (amount >= 1000) {
      return `$${(amount / 1000).toFixed(1)}K`;
    }
    return `$${amount.toLocaleString()}`;
  };

  // Helper function to get time ago string
  const getTimeAgo = (dateString?: string) => {
    if (!dateString) return 'Unknown';
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    const diffMinutes = Math.floor(diffMs / (1000 * 60));
    
    if (diffDays > 30) return `${Math.floor(diffDays / 30)} month${Math.floor(diffDays / 30) > 1 ? 's' : ''} ago`;
    if (diffDays > 0) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
    if (diffHours > 0) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
    if (diffMinutes > 0) return `${diffMinutes} minute${diffMinutes > 1 ? 's' : ''} ago`;
    return 'Just now';
  };

  // Helper function to format date
  const formatDate = (dateString?: string) => {
    if (!dateString) return 'Not set';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
  };

  // Build pipeline by expected closing month
  const buildPipelineByMonth = (opps: Opportunity[]) => {
    const monthMap: Record<string, { value: number; count: number }> = {};
    const now = new Date();
    
    // Initialize next 6 months
    for (let i = 0; i < 6; i++) {
      const date = new Date(now.getFullYear(), now.getMonth() + i, 1);
      const key = date.toLocaleDateString('en-US', { month: 'short', year: '2-digit' });
      monthMap[key] = { value: 0, count: 0 };
    }
    
    // Group opportunities by expected close month
    opps.forEach((opp) => {
      if (opp.expectedCloseDate && opp.stage < 10) { // Only open opportunities
        const closeDate = new Date(opp.expectedCloseDate);
        const key = closeDate.toLocaleDateString('en-US', { month: 'short', year: '2-digit' });
        if (monthMap[key]) {
          monthMap[key].value += opp.amount || 0;
          monthMap[key].count += 1;
        }
      }
    });
    
    return Object.entries(monthMap).map(([month, data]) => ({
      month,
      value: data.value,
      count: data.count,
    }));
  };

  const loadDashboardData = async () => {
    try {
      setLoading(true);
      
      // Load opportunities data
      try {
        const pipelineResponse = await opportunityService.getTotalPipeline();
        setTotalPipeline(pipelineResponse.data.totalPipeline);
        setTotalRevenue(pipelineResponse.data.wonValue || 0);
      } catch (err) {
        console.warn('Could not load pipeline data');
      }

      // Load all opportunities for detailed views
      try {
        const opportunitiesResponse = await opportunityService.getAll();
        const opps = opportunitiesResponse.data || [];
        setOpportunities(opps);
        setPipelineByMonth(buildPipelineByMonth(opps));
      } catch (err) {
        console.warn('Could not load opportunities data');
      }

      // Load campaigns data
      try {
        const campaignsResponse = await campaignService.getActive();
        setActiveCampaigns(campaignsResponse.data.length);
      } catch (err) {
        console.warn('Could not load campaigns data');
      }
      
      // Load customers data
      try {
        const customersResponse = await customerService.getAll();
        const custs = customersResponse.data || [];
        setCustomers(custs);
        setTotalCustomers(custs.length);
      } catch (err) {
        console.warn('Could not load customers data');
      }
    } catch (err) {
      console.error('Error loading dashboard data:', err);
      setError('Failed to load dashboard data');
    } finally {
      setLoading(false);
    }
  };

  // Get customer name by ID
  const getCustomerName = (customerId: number) => {
    const customer = customers.find(c => c.id === customerId);
    if (customer) {
      return `${customer.firstName} ${customer.lastName}`.trim() || customer.company || 'Unknown';
    }
    return 'Unknown';
  };

  // Get recent opportunities sorted by creation or last activity
  const getRecentOpportunities = () => {
    return [...opportunities]
      .filter(opp => opp.stage < 10) // Only open opportunities
      .sort((a, b) => {
        const dateA = new Date(a.lastActivityDate || a.createdAt || 0).getTime();
        const dateB = new Date(b.lastActivityDate || b.createdAt || 0).getTime();
        return dateB - dateA;
      })
      .slice(0, 5);
  };

  // Check access permissions for each stat card
  const canViewOpportunities = canAccessMenu('Opportunities');
  const canViewCampaigns = canAccessMenu('Campaigns');
  const canViewCustomers = canAccessMenu('Customers');
  const canViewReports = canAccessMenu('Reports');

  const stats = [
    {
      title: 'Total Pipeline',
      value: `$${totalPipeline.toLocaleString()}`,
      icon: TrendingUpIcon,
      color: '#6750A4',
      link: '/opportunities',
      canAccess: canViewOpportunities,
    },
    {
      title: 'Active Campaigns',
      value: activeCampaigns,
      icon: CampaignIcon,
      color: '#06A77D',
      link: '/campaigns',
      canAccess: canViewCampaigns,
    },
    {
      title: 'Customers',
      value: totalCustomers.toLocaleString(),
      icon: PeopleIcon,
      color: '#0092BC',
      link: '/customers',
      canAccess: canViewCustomers,
    },
    {
      title: 'Total Revenue',
      value: `$${totalRevenue.toLocaleString()}`,
      icon: ShoppingCartIcon,
      color: '#F57C00',
      link: '/reports',
      canAccess: canViewReports,
    },
  ];

  const handleStatClick = (link: string, canAccess: boolean) => {
    if (canAccess) {
      navigate(link);
    }
  };

  return (
    <Box sx={{ py: 2 }}>
      {/* Header */}
      <Box sx={{ mb: 4, display: 'flex', alignItems: 'center', gap: 2 }}>
        <Box sx={{ width: 40, height: 40, flexShrink: 0 }}><img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} /></Box>
        <Box>
          <Typography variant="h3" sx={{ fontWeight: 700, mb: 0.5 }}>
            Dashboard
          </Typography>
          <Typography color="textSecondary" variant="body2">
            Welcome back! Here's your performance overview.
          </Typography>
        </Box>
      </Box>

      {/* Error Alert */}
      {error && (
        <Alert severity="error" sx={{ mb: 3, borderRadius: 2 }}>
          {error}
        </Alert>
      )}

      {/* Stats Grid */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        {stats.map((stat, index) => (
          <Grid item xs={12} sm={6} md={3} key={index}>
            <StatCard 
              {...stat} 
              loading={loading} 
              clickable={stat.canAccess}
              onClick={() => handleStatClick(stat.link, stat.canAccess)}
            />
          </Grid>
        ))}
      </Grid>

      {/* Charts Grid */}
      <Grid container spacing={3}>
        {/* Pipeline Trend */}
        <Grid item xs={12} md={8}>
          <Card 
            sx={{ 
              borderRadius: 3, 
              boxShadow: 1,
              cursor: canViewOpportunities ? 'pointer' : 'default',
              transition: 'all 0.2s ease-in-out',
              '&:hover': canViewOpportunities ? {
                boxShadow: 3,
                transform: 'translateY(-2px)',
              } : {},
            }}
            onClick={() => canViewOpportunities && navigate('/opportunities')}
          >
            <CardHeader
              title="Pipeline Trend"
              subheader={canViewOpportunities ? "Last 6 months - Click to view opportunities" : "Last 6 months"}
              titleTypographyProps={{ variant: 'h6', sx: { fontWeight: 600 } }}
              subheaderTypographyProps={{ variant: 'caption' }}
              sx={{ borderBottom: '1px solid #E0E0E0' }}
            />
            <CardContent>
              {loading ? (
                <Skeleton variant="rectangular" height={300} sx={{ borderRadius: 2 }} />
              ) : (
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={pipelineData}>
                    <defs>
                      <linearGradient id="colorValue" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="5%" stopColor="#6750A4" stopOpacity={0.8} />
                        <stop offset="95%" stopColor="#6750A4" stopOpacity={0.1} />
                      </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="3 3" stroke="#E0E0E0" />
                    <XAxis dataKey="month" stroke="#79747E" />
                    <YAxis stroke="#79747E" />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: '#FFFBFE',
                        border: '1px solid #E0E0E0',
                        borderRadius: '8px',
                      }}
                    />
                    <Line
                      type="monotone"
                      dataKey="value"
                      stroke="#6750A4"
                      strokeWidth={2}
                      dot={{ fill: '#6750A4', r: 5 }}
                      activeDot={{ r: 7 }}
                      fill="url(#colorValue)"
                    />
                  </LineChart>
                </ResponsiveContainer>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Status Distribution */}
        <Grid item xs={12} md={4}>
          <Card 
            sx={{ 
              borderRadius: 3, 
              boxShadow: 1,
              cursor: canViewOpportunities ? 'pointer' : 'default',
              transition: 'all 0.2s ease-in-out',
              '&:hover': canViewOpportunities ? {
                boxShadow: 3,
                transform: 'translateY(-2px)',
              } : {},
            }}
            onClick={() => canViewOpportunities && navigate('/opportunities')}
          >
            <CardHeader
              title="Opportunity Status"
              subheader={canViewOpportunities ? "Click to view opportunities" : undefined}
              titleTypographyProps={{ variant: 'h6', sx: { fontWeight: 600 } }}
              subheaderTypographyProps={{ variant: 'caption' }}
              sx={{ borderBottom: '1px solid #E0E0E0' }}
            />
            <CardContent sx={{ display: 'flex', justifyContent: 'center' }}>
              {loading ? (
                <CircularProgress />
              ) : (
                <ResponsiveContainer width="100%" height={250}>
                  <PieChart>
                    <Pie
                      data={statusData}
                      cx="50%"
                      cy="50%"
                      labelLine={false}
                      label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                      outerRadius={80}
                      fill="#8884d8"
                      dataKey="value"
                    >
                      {statusData.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={entry.color} />
                      ))}
                    </Pie>
                    <Tooltip />
                  </PieChart>
                </ResponsiveContainer>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Pipeline by Expected Closing Month */}
        <Grid item xs={12}>
          <Card sx={{ borderRadius: 3, boxShadow: 1 }}>
            <CardHeader
              title={
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <CalendarIcon sx={{ color: '#6750A4' }} />
                  <Typography variant="h6" sx={{ fontWeight: 600 }}>
                    Pipeline by Expected Closing Month
                  </Typography>
                </Box>
              }
              subheader="Opportunities grouped by expected close date - Next 6 months"
              subheaderTypographyProps={{ variant: 'caption' }}
              sx={{ borderBottom: '1px solid #E0E0E0' }}
            />
            <CardContent>
              {loading ? (
                <Skeleton variant="rectangular" height={250} sx={{ borderRadius: 2 }} />
              ) : (
                <ResponsiveContainer width="100%" height={250}>
                  <BarChart data={pipelineByMonth}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#E0E0E0" />
                    <XAxis dataKey="month" stroke="#79747E" />
                    <YAxis 
                      stroke="#79747E"
                      tickFormatter={(value) => formatCurrency(value)}
                    />
                    <Tooltip
                      formatter={(value: number, name: string) => [
                        name === 'value' ? formatCurrency(value) : value,
                        name === 'value' ? 'Pipeline Value' : 'Deal Count'
                      ]}
                      contentStyle={{
                        backgroundColor: '#FFFBFE',
                        border: '1px solid #E0E0E0',
                        borderRadius: '8px',
                      }}
                    />
                    <Legend />
                    <Bar 
                      dataKey="value" 
                      name="Pipeline Value" 
                      fill="#6750A4" 
                      radius={[4, 4, 0, 0]}
                    />
                    <Bar 
                      dataKey="count" 
                      name="Deal Count" 
                      fill="#06A77D" 
                      radius={[4, 4, 0, 0]}
                    />
                  </BarChart>
                </ResponsiveContainer>
              )}
              {/* Summary row */}
              {!loading && (
                <Box sx={{ display: 'flex', justifyContent: 'space-around', mt: 2, pt: 2, borderTop: '1px solid #E0E0E0' }}>
                  <Box sx={{ textAlign: 'center' }}>
                    <Typography variant="h6" sx={{ fontWeight: 700, color: '#6750A4' }}>
                      {formatCurrency(pipelineByMonth.reduce((sum, m) => sum + m.value, 0))}
                    </Typography>
                    <Typography variant="caption" color="textSecondary">
                      Total Expected Pipeline
                    </Typography>
                  </Box>
                  <Divider orientation="vertical" flexItem />
                  <Box sx={{ textAlign: 'center' }}>
                    <Typography variant="h6" sx={{ fontWeight: 700, color: '#06A77D' }}>
                      {pipelineByMonth.reduce((sum, m) => sum + m.count, 0)}
                    </Typography>
                    <Typography variant="caption" color="textSecondary">
                      Total Opportunities
                    </Typography>
                  </Box>
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Recent Activity - Enhanced */}
        <Grid item xs={12}>
          <Card sx={{ borderRadius: 3, boxShadow: 1 }}>
            <CardHeader
              title={
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <TrendingUpIcon sx={{ color: '#6750A4' }} />
                  <Typography variant="h6" sx={{ fontWeight: 600 }}>
                    Recent Opportunities
                  </Typography>
                </Box>
              }
              subheader="Latest pipeline activity with detailed information"
              subheaderTypographyProps={{ variant: 'caption' }}
              sx={{ borderBottom: '1px solid #E0E0E0' }}
            />
            <CardContent>
              {loading ? (
                <Box>
                  {[1, 2, 3, 4, 5].map((i) => (
                    <Skeleton key={i} height={100} sx={{ mb: 1, borderRadius: 2 }} />
                  ))}
                </Box>
              ) : getRecentOpportunities().length === 0 ? (
                <Box sx={{ textAlign: 'center', py: 4 }}>
                  <Typography color="textSecondary">No recent opportunities found</Typography>
                </Box>
              ) : (
                <Box>
                  {getRecentOpportunities().map((opp, index) => (
                    <Card
                      key={opp.id || index}
                      onClick={() => canViewOpportunities && navigate(`/opportunities`)}
                      sx={{
                        mb: 2,
                        p: 2,
                        borderRadius: 2,
                        cursor: canViewOpportunities ? 'pointer' : 'default',
                        border: '1px solid #E0E0E0',
                        background: 'linear-gradient(135deg, #FAFAFA 0%, #FFFFFF 100%)',
                        transition: 'all 0.2s ease-in-out',
                        '&:hover': canViewOpportunities ? {
                          boxShadow: 2,
                          transform: 'translateX(4px)',
                          borderColor: '#6750A4',
                        } : {},
                      }}
                    >
                      {/* Header Row: Name and Amount */}
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1.5 }}>
                        <Box sx={{ flex: 1 }}>
                          <Typography 
                            variant="subtitle1" 
                            sx={{ 
                              fontWeight: 600,
                              color: canViewOpportunities ? 'primary.main' : 'text.primary',
                              mb: 0.5,
                            }}
                          >
                            {opp.name}
                          </Typography>
                          {opp.description && (
                            <Typography 
                              variant="body2" 
                              color="textSecondary"
                              sx={{ 
                                overflow: 'hidden',
                                textOverflow: 'ellipsis',
                                whiteSpace: 'nowrap',
                                maxWidth: '400px',
                              }}
                            >
                              {opp.description}
                            </Typography>
                          )}
                        </Box>
                        <Box sx={{ textAlign: 'right' }}>
                          <Typography variant="h6" sx={{ fontWeight: 700, color: '#06A77D' }}>
                            {formatCurrency(opp.amount)}
                          </Typography>
                          {opp.weightedAmount && (
                            <Typography variant="caption" color="textSecondary">
                              Weighted: {formatCurrency(opp.weightedAmount)}
                            </Typography>
                          )}
                        </Box>
                      </Box>

                      {/* Info Row: Stage, Probability, Customer, Expected Close */}
                      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 1.5 }}>
                        <Chip
                          label={stageNames[opp.stage] || `Stage ${opp.stage}`}
                          size="small"
                          sx={{
                            backgroundColor: `${stageColors[opp.stage] || '#9E9E9E'}20`,
                            color: stageColors[opp.stage] || '#9E9E9E',
                            fontWeight: 600,
                            border: `1px solid ${stageColors[opp.stage] || '#9E9E9E'}40`,
                          }}
                        />
                        <Chip
                          icon={<MoneyIcon sx={{ fontSize: 16 }} />}
                          label={`${opp.probability}% Probability`}
                          size="small"
                          variant="outlined"
                          sx={{ borderColor: '#6750A4', color: '#6750A4' }}
                        />
                        <Chip
                          icon={<BusinessIcon sx={{ fontSize: 16 }} />}
                          label={getCustomerName(opp.customerId)}
                          size="small"
                          variant="outlined"
                        />
                        {opp.expectedCloseDate && (
                          <Chip
                            icon={<ScheduleIcon sx={{ fontSize: 16 }} />}
                            label={`Close: ${formatDate(opp.expectedCloseDate)}`}
                            size="small"
                            variant="outlined"
                            sx={{ borderColor: '#F57C00', color: '#F57C00' }}
                          />
                        )}
                      </Box>

                      {/* Progress Bar for Probability */}
                      <Box sx={{ mb: 1 }}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                          <Typography variant="caption" color="textSecondary">
                            Win Probability
                          </Typography>
                          <Typography variant="caption" sx={{ fontWeight: 600, color: '#6750A4' }}>
                            {opp.probability}%
                          </Typography>
                        </Box>
                        <LinearProgress
                          variant="determinate"
                          value={opp.probability}
                          sx={{
                            height: 6,
                            borderRadius: 3,
                            backgroundColor: '#E0E0E0',
                            '& .MuiLinearProgress-bar': {
                              borderRadius: 3,
                              backgroundColor: opp.probability >= 70 ? '#06A77D' : opp.probability >= 40 ? '#F57C00' : '#6750A4',
                            },
                          }}
                        />
                      </Box>

                      {/* Footer Row: Last Activity and Owner */}
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', pt: 1, borderTop: '1px dashed #E0E0E0' }}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Avatar sx={{ width: 24, height: 24, fontSize: 12, bgcolor: '#6750A4' }}>
                            {(opp.ownerName || 'U')[0]}
                          </Avatar>
                          <Typography variant="caption" color="textSecondary">
                            {opp.ownerName || 'Unassigned'}
                          </Typography>
                        </Box>
                        <Typography variant="caption" color="textSecondary">
                          Last activity: {getTimeAgo(opp.lastActivityDate || opp.createdAt)}
                        </Typography>
                      </Box>
                    </Card>
                  ))}
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}

export default DashboardPage;
