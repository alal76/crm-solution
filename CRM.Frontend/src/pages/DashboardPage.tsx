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
} from '@mui/material';
import {
  TrendingUp as TrendingUpIcon,
  People as PeopleIcon,
  ShoppingCart as ShoppingCartIcon,
  Campaign as CampaignIcon,
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
} from 'recharts';
import logo from '../assets/logo.png';
import { opportunityService, campaignService, customerService } from '../services/apiService';
import { useProfile } from '../contexts/ProfileContext';

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
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadDashboardData();
  }, []);

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

      // Load campaigns data
      try {
        const campaignsResponse = await campaignService.getActive();
        setActiveCampaigns(campaignsResponse.data.length);
      } catch (err) {
        console.warn('Could not load campaigns data');
      }
      
      // Load customers count
      try {
        const customersResponse = await customerService.getAll();
        setTotalCustomers(customersResponse.data.length);
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

        {/* Recent Activity */}
        <Grid item xs={12}>
          <Card sx={{ borderRadius: 3, boxShadow: 1 }}>
            <CardHeader
              title="Recent Activity"
              subheader="Last 7 days"
              titleTypographyProps={{ variant: 'h6', sx: { fontWeight: 600 } }}
              subheaderTypographyProps={{ variant: 'caption' }}
              sx={{ borderBottom: '1px solid #E0E0E0' }}
            />
            <CardContent>
              {loading ? (
                <Box>
                  {[1, 2, 3].map((i) => (
                    <Skeleton key={i} height={60} sx={{ mb: 1 }} />
                  ))}
                </Box>
              ) : (
                <Box>
                  {[
                    { activity: 'New opportunity created', user: 'John Doe', time: '2 hours ago', link: '/opportunities', canAccess: canViewOpportunities },
                    { activity: 'Campaign updated', user: 'Jane Smith', time: '4 hours ago', link: '/campaigns', canAccess: canViewCampaigns },
                    { activity: 'Customer added', user: 'Mike Johnson', time: '1 day ago', link: '/customers', canAccess: canViewCustomers },
                  ].map((item, index) => (
                    <Box
                      key={index}
                      onClick={() => item.canAccess && navigate(item.link)}
                      sx={{
                        display: 'flex',
                        justifyContent: 'space-between',
                        alignItems: 'center',
                        py: 2,
                        px: 1,
                        borderRadius: 1,
                        cursor: item.canAccess ? 'pointer' : 'default',
                        borderBottom: index < 2 ? '1px solid #E0E0E0' : 'none',
                        transition: 'background-color 0.2s ease-in-out',
                        '&:hover': item.canAccess ? {
                          backgroundColor: 'rgba(103, 80, 164, 0.08)',
                        } : {},
                      }}
                    >
                      <Box>
                        <Typography 
                          variant="body2" 
                          sx={{ 
                            fontWeight: 500,
                            color: item.canAccess ? 'primary.main' : 'text.primary',
                            textDecoration: item.canAccess ? 'underline' : 'none',
                          }}
                        >
                          {item.activity}
                        </Typography>
                        <Typography variant="caption" color="textSecondary">
                          by {item.user}
                        </Typography>
                      </Box>
                      <Typography variant="caption" color="textSecondary">
                        {item.time}
                      </Typography>
                    </Box>
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
