import { useState, useEffect } from 'react';
import { TabPanel } from '../components/common';
import {
  Box, Container, Typography, Card, CardContent, TextField, Button, Grid,
  CircularProgress, Alert, Tabs, Tab, Chip, Avatar, Divider, IconButton,
  Table, TableBody, TableCell, TableHead, TableRow, Paper, InputAdornment,
  List, ListItem, ListItemAvatar, ListItemText, Tooltip, Link, Autocomplete
} from '@mui/material';
import {
  LIFECYCLE_STAGE_OPTIONS,
  CUSTOMER_TYPE_OPTIONS,
  getLabelByValue,
  getColorByValue
} from '../utils/constants';
import {
  Search as SearchIcon, Business as BusinessIcon, Person as PersonIcon,
  Email as EmailIcon, Phone as PhoneIcon, LinkedIn as LinkedInIcon,
  Twitter as TwitterIcon, Language as WebIcon, LocationOn as LocationIcon,
  AttachMoney as MoneyIcon, CalendarToday as CalendarIcon,
  Article as NewsIcon, Public as SocialIcon, Refresh as RefreshIcon,
  AccountCircle as AccountManagerIcon, OpenInNew as OpenInNewIcon,
  Facebook as FacebookIcon
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import { getNewsSocialFeeds, refreshNewsSocialFeeds, getNewsSocialStatus, NewsItem, SocialFeed, NewsSocialStatus } from '../services/newsSocialService';
import logo from '../assets/logo.png';

interface Customer {
  id: number;
  company: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  industry?: string;
  website?: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  customerCategory?: number;
  customerType?: number;
  lifecycleStage?: number;
  annualRevenue?: number;
  employeeCount?: number;
  accountManagerId?: number;
  accountManagerName?: string;
  createdAt?: string;
  linkedInUrl?: string;
  twitterHandle?: string;
  facebookUrl?: string;
  instagramHandle?: string;
}

interface Contact {
  id: number;
  firstName: string;
  lastName: string;
  emailPrimary?: string;
  phonePrimary?: string;
  jobTitle?: string;
  role?: string;
  isPrimaryContact?: boolean;
  linkedInUrl?: string;
  twitterHandle?: string;
}

interface User {
  id: number;
  firstName: string;
  lastName: string;
}

// Use shared constants - aliased for backward compatibility in this file
const LIFECYCLE_STAGES = LIFECYCLE_STAGE_OPTIONS;
const CUSTOMER_TYPES = CUSTOMER_TYPE_OPTIONS;

function CustomerOverviewPage() {
  const [searchQuery, setSearchQuery] = useState('');
  const [searchType, setSearchType] = useState<'all' | 'name' | 'email' | 'id' | 'phone' | 'accountManager'>('all');
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [filteredCustomers, setFilteredCustomers] = useState<Customer[]>([]);
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(null);
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [newsItems, setNewsItems] = useState<NewsItem[]>([]);
  const [socialFeeds, setSocialFeeds] = useState<SocialFeed[]>([]);
  const [accountManagers, setAccountManagers] = useState<User[]>([]);
  const [selectedAccountManager, setSelectedAccountManager] = useState<User | null>(null);
  const [loading, setLoading] = useState(false);
  const [loadingDetails, setLoadingDetails] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [tabValue, setTabValue] = useState(0);
  const [loadingNews, setLoadingNews] = useState(false);
  const [apiStatus, setApiStatus] = useState<NewsSocialStatus | null>(null);

  useEffect(() => {
    fetchCustomers();
    fetchAccountManagers();
    fetchApiStatus();
  }, []);

  const fetchApiStatus = async () => {
    try {
      const status = await getNewsSocialStatus();
      setApiStatus(status);
    } catch (err) {
      console.error('Error fetching API status:', err);
    }
  };

  useEffect(() => {
    filterCustomers();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchQuery, searchType, customers, selectedAccountManager]);

  const fetchCustomers = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/customers');
      setCustomers(response.data || []);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch customers');
    } finally {
      setLoading(false);
    }
  };

  const fetchAccountManagers = async () => {
    try {
      const response = await apiClient.get('/users');
      setAccountManagers(response.data || []);
    } catch (err) {
      console.error('Error fetching account managers:', err);
    }
  };

  const filterCustomers = () => {
    let filtered = [...customers];
    const query = searchQuery.toLowerCase().trim();

    if (selectedAccountManager) {
      filtered = filtered.filter(c => c.accountManagerId === selectedAccountManager.id);
    }

    if (query) {
      filtered = filtered.filter(customer => {
        switch (searchType) {
          case 'name':
            return customer.company?.toLowerCase().includes(query) ||
                   `${customer.firstName} ${customer.lastName}`.toLowerCase().includes(query);
          case 'email':
            return customer.email?.toLowerCase().includes(query);
          case 'id':
            return customer.id.toString().includes(query);
          case 'phone':
            return customer.phone?.includes(query);
          case 'accountManager':
            return customer.accountManagerName?.toLowerCase().includes(query);
          default:
            return customer.company?.toLowerCase().includes(query) ||
                   `${customer.firstName} ${customer.lastName}`.toLowerCase().includes(query) ||
                   customer.email?.toLowerCase().includes(query) ||
                   customer.id.toString().includes(query) ||
                   customer.phone?.includes(query) ||
                   customer.accountManagerName?.toLowerCase().includes(query);
        }
      });
    }

    setFilteredCustomers(filtered);
  };

  const selectCustomer = async (customer: Customer) => {
    setSelectedCustomer(customer);
    setTabValue(0);
    await fetchCustomerDetails(customer.id);
  };

  const fetchCustomerDetails = async (customerId: number) => {
    try {
      setLoadingDetails(true);
      
      // Fetch contacts for this customer
      const contactsResponse = await apiClient.get(`/customers/${customerId}/contacts`);
      setContacts(contactsResponse.data || []);

      // Fetch news and social feeds from real API
      try {
        const feedsResponse = await getNewsSocialFeeds(customerId, 10, 10);
        setNewsItems(feedsResponse.newsItems || []);
        setSocialFeeds(feedsResponse.socialFeeds || []);
      } catch (feedError) {
        console.error('Error fetching news/social feeds:', feedError);
        // Set empty arrays if API fails - no fallback to mock data
        setNewsItems([]);
        setSocialFeeds([]);
      }

    } catch (err: any) {
      console.error('Error fetching customer details:', err);
    } finally {
      setLoadingDetails(false);
    }
  };

  const refreshNewsFeed = async () => {
    if (!selectedCustomer) return;
    setLoadingNews(true);
    try {
      // Call refresh endpoint to bypass cache
      const feedsResponse = await refreshNewsSocialFeeds(selectedCustomer.id, 10, 10);
      setNewsItems(feedsResponse.newsItems || []);
      setSocialFeeds(feedsResponse.socialFeeds || []);
    } catch (err) {
      console.error('Error refreshing feeds:', err);
    } finally {
      setLoadingNews(false);
    }
  };

  const getLifecycleStage = (value?: number) => LIFECYCLE_STAGES.find(s => s.value === value);
  const getCustomerType = (value?: number) => CUSTOMER_TYPES.find(t => t.value === value);

  const getSentimentColor = (sentiment?: string) => {
    switch (sentiment) {
      case 'positive': return '#4caf50';
      case 'negative': return '#f44336';
      default: return '#9e9e9e';
    }
  };

  const getPlatformIcon = (platform: string) => {
    switch (platform) {
      case 'linkedin': return <LinkedInIcon sx={{ color: '#0077b5' }} />;
      case 'twitter': return <TwitterIcon sx={{ color: '#1da1f2' }} />;
      case 'facebook': return <FacebookIcon sx={{ color: '#1877f2' }} />;
      default: return <SocialIcon />;
    }
  };

  return (
    <Box sx={{ py: 2 }}>
      <Container maxWidth="xl">
        {/* Header */}
        <Box sx={{ mb: 4, display: 'flex', alignItems: 'center', gap: 2 }}>
          <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
            <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
          </Box>
          <Box>
            <Typography variant="h3" sx={{ fontWeight: 700, mb: 0.5 }}>
              Account Overview
            </Typography>
            <Typography color="textSecondary" variant="body2">
              360° view of account information, contacts, news and social feeds
            </Typography>
          </Box>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        <Grid container spacing={3}>
          {/* Left Panel - Search & Customer List */}
          <Grid item xs={12} md={4}>
            <Card sx={{ borderRadius: 3, boxShadow: 1 }}>
              <CardContent>
                <Typography variant="h6" sx={{ fontWeight: 600, mb: 2 }}>
                  Find Account
                </Typography>

                {/* Search Type Selector */}
                <Box sx={{ display: 'flex', gap: 1, mb: 2, flexWrap: 'wrap' }}>
                  {[
                    { value: 'all', label: 'All' },
                    { value: 'name', label: 'Name' },
                    { value: 'email', label: 'Email' },
                    { value: 'id', label: 'ID' },
                    { value: 'phone', label: 'Phone' },
                  ].map(type => (
                    <Chip
                      key={type.value}
                      label={type.label}
                      size="small"
                      variant={searchType === type.value ? 'filled' : 'outlined'}
                      color={searchType === type.value ? 'primary' : 'default'}
                      onClick={() => setSearchType(type.value as any)}
                      sx={{ cursor: 'pointer' }}
                    />
                  ))}
                </Box>

                {/* Search Input */}
                <TextField
                  fullWidth
                  placeholder="Search accounts..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  size="small"
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <SearchIcon color="action" />
                      </InputAdornment>
                    ),
                  }}
                  sx={{ mb: 2 }}
                />

                {/* Account Manager Filter */}
                <Autocomplete
                  options={accountManagers}
                  getOptionLabel={(option) => `${option.firstName} ${option.lastName}`}
                  value={selectedAccountManager}
                  onChange={(_, newValue) => setSelectedAccountManager(newValue)}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Filter by Account Manager"
                      size="small"
                      InputProps={{
                        ...params.InputProps,
                        startAdornment: (
                          <>
                            <InputAdornment position="start">
                              <AccountManagerIcon color="action" />
                            </InputAdornment>
                            {params.InputProps.startAdornment}
                          </>
                        ),
                      }}
                    />
                  )}
                  sx={{ mb: 2 }}
                />

                <Divider sx={{ my: 2 }} />

                {/* Customer List */}
                <Typography variant="subtitle2" color="textSecondary" sx={{ mb: 1 }}>
                  {filteredCustomers.length} customer(s) found
                </Typography>

                {loading ? (
                  <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
                    <CircularProgress size={30} />
                  </Box>
                ) : (
                  <List sx={{ maxHeight: 400, overflow: 'auto' }}>
                    {filteredCustomers.slice(0, 50).map((customer) => (
                      <ListItem
                        key={customer.id}
                        button
                        selected={selectedCustomer?.id === customer.id}
                        onClick={() => selectCustomer(customer)}
                        sx={{
                          borderRadius: 2,
                          mb: 0.5,
                          '&.Mui-selected': { backgroundColor: '#E8DEF8' },
                        }}
                      >
                        <ListItemAvatar>
                          <Avatar sx={{ bgcolor: customer.customerCategory === 1 ? '#6750A4' : '#7D5260' }}>
                            {customer.customerCategory === 1 ? <BusinessIcon /> : <PersonIcon />}
                          </Avatar>
                        </ListItemAvatar>
                        <ListItemText
                          primary={customer.company || `${customer.firstName} ${customer.lastName}`}
                          secondary={
                            <Box component="span">
                              <Typography variant="caption" display="block">
                                ID: {customer.id} • {customer.email}
                              </Typography>
                              {customer.accountManagerName && (
                                <Typography variant="caption" color="primary">
                                  AM: {customer.accountManagerName}
                                </Typography>
                              )}
                            </Box>
                          }
                        />
                      </ListItem>
                    ))}
                    {filteredCustomers.length === 0 && (
                      <Typography sx={{ textAlign: 'center', py: 4, color: 'textSecondary' }}>
                        No customers found
                      </Typography>
                    )}
                  </List>
                )}
              </CardContent>
            </Card>
          </Grid>

          {/* Right Panel - Customer Details */}
          <Grid item xs={12} md={8}>
            {selectedCustomer ? (
              <Card sx={{ borderRadius: 3, boxShadow: 1 }}>
                <CardContent>
                  {/* Customer Header */}
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
                    <Avatar sx={{ width: 64, height: 64, bgcolor: '#6750A4', fontSize: 24 }}>
                      {selectedCustomer.customerCategory === 1 ? <BusinessIcon /> : <PersonIcon />}
                    </Avatar>
                    <Box sx={{ flex: 1 }}>
                      <Typography variant="h5" sx={{ fontWeight: 700 }}>
                        {selectedCustomer.company || `${selectedCustomer.firstName} ${selectedCustomer.lastName}`}
                      </Typography>
                      <Box sx={{ display: 'flex', gap: 1, alignItems: 'center', mt: 0.5 }}>
                        <Chip
                          label={`ID: ${selectedCustomer.id}`}
                          size="small"
                          variant="outlined"
                        />
                        {selectedCustomer.lifecycleStage !== undefined && (
                          <Chip
                            label={getLifecycleStage(selectedCustomer.lifecycleStage)?.label}
                            size="small"
                            sx={{
                              backgroundColor: getLifecycleStage(selectedCustomer.lifecycleStage)?.color,
                              color: 'white'
                            }}
                          />
                        )}
                        {selectedCustomer.customerType !== undefined && (
                          <Chip
                            label={getCustomerType(selectedCustomer.customerType)?.label}
                            size="small"
                            variant="outlined"
                            color="primary"
                          />
                        )}
                      </Box>
                    </Box>
                    {/* Social Links */}
                    <Box sx={{ display: 'flex', gap: 1 }}>
                      {selectedCustomer.website && (
                        <Tooltip title="Website">
                          <IconButton href={selectedCustomer.website} target="_blank" size="small">
                            <WebIcon />
                          </IconButton>
                        </Tooltip>
                      )}
                      {selectedCustomer.linkedInUrl && (
                        <Tooltip title="LinkedIn">
                          <IconButton href={selectedCustomer.linkedInUrl} target="_blank" size="small">
                            <LinkedInIcon sx={{ color: '#0077b5' }} />
                          </IconButton>
                        </Tooltip>
                      )}
                      {selectedCustomer.twitterHandle && (
                        <Tooltip title="Twitter">
                          <IconButton href={`https://twitter.com/${selectedCustomer.twitterHandle}`} target="_blank" size="small">
                            <TwitterIcon sx={{ color: '#1da1f2' }} />
                          </IconButton>
                        </Tooltip>
                      )}
                    </Box>
                  </Box>

                  {/* Tabs */}
                  <Tabs value={tabValue} onChange={(_, v) => setTabValue(v)} sx={{ mb: 2 }}>
                    <Tab label="Overview" />
                    <Tab label="Contacts" />
                    <Tab label="News & Social" />
                  </Tabs>

                  {loadingDetails ? (
                    <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
                      <CircularProgress />
                    </Box>
                  ) : (
                    <>
                      {/* Overview Tab */}
                      <TabPanel value={tabValue} index={0}>
                        <Grid container spacing={3}>
                          {/* Contact Information */}
                          <Grid item xs={12} md={6}>
                            <Paper sx={{ p: 2, backgroundColor: '#F5EFF7', borderRadius: 2 }}>
                              <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 2 }}>
                                Contact Information
                              </Typography>
                              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                  <EmailIcon fontSize="small" color="action" />
                                  <Typography variant="body2">{selectedCustomer.email || 'N/A'}</Typography>
                                </Box>
                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                  <PhoneIcon fontSize="small" color="action" />
                                  <Typography variant="body2">{selectedCustomer.phone || 'N/A'}</Typography>
                                </Box>
                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                  <LocationIcon fontSize="small" color="action" />
                                  <Typography variant="body2">
                                    {[selectedCustomer.address, selectedCustomer.city, selectedCustomer.state, selectedCustomer.country]
                                      .filter(Boolean).join(', ') || 'N/A'}
                                  </Typography>
                                </Box>
                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                  <WebIcon fontSize="small" color="action" />
                                  <Typography variant="body2">
                                    {selectedCustomer.website ? (
                                      <Link href={selectedCustomer.website} target="_blank">{selectedCustomer.website}</Link>
                                    ) : 'N/A'}
                                  </Typography>
                                </Box>
                              </Box>
                            </Paper>
                          </Grid>

                          {/* Business Information */}
                          <Grid item xs={12} md={6}>
                            <Paper sx={{ p: 2, backgroundColor: '#FFF3E0', borderRadius: 2 }}>
                              <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 2 }}>
                                Business Information
                              </Typography>
                              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                  <BusinessIcon fontSize="small" color="action" />
                                  <Typography variant="body2">Industry: {selectedCustomer.industry || 'N/A'}</Typography>
                                </Box>
                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                  <MoneyIcon fontSize="small" color="action" />
                                  <Typography variant="body2">
                                    Annual Revenue: {selectedCustomer.annualRevenue 
                                      ? `$${selectedCustomer.annualRevenue.toLocaleString()}` 
                                      : 'N/A'}
                                  </Typography>
                                </Box>
                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                  <PersonIcon fontSize="small" color="action" />
                                  <Typography variant="body2">
                                    Employees: {selectedCustomer.employeeCount || 'N/A'}
                                  </Typography>
                                </Box>
                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                  <AccountManagerIcon fontSize="small" color="action" />
                                  <Typography variant="body2">
                                    Account Manager: {selectedCustomer.accountManagerName || 'N/A'}
                                  </Typography>
                                </Box>
                              </Box>
                            </Paper>
                          </Grid>

                          {/* Account Details */}
                          <Grid item xs={12}>
                            <Paper sx={{ p: 2, borderRadius: 2 }}>
                              <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 2 }}>
                                Account Details
                              </Typography>
                              <Grid container spacing={2}>
                                <Grid item xs={6} md={3}>
                                  <Typography variant="caption" color="textSecondary">Account Since</Typography>
                                  <Typography variant="body2">
                                    {selectedCustomer.createdAt 
                                      ? new Date(selectedCustomer.createdAt).toLocaleDateString() 
                                      : 'N/A'}
                                  </Typography>
                                </Grid>
                                <Grid item xs={6} md={3}>
                                  <Typography variant="caption" color="textSecondary">Lifecycle Stage</Typography>
                                  <Typography variant="body2">
                                    {getLifecycleStage(selectedCustomer.lifecycleStage)?.label || 'N/A'}
                                  </Typography>
                                </Grid>
                                <Grid item xs={6} md={3}>
                                  <Typography variant="caption" color="textSecondary">Account Type</Typography>
                                  <Typography variant="body2">
                                    {getCustomerType(selectedCustomer.customerType)?.label || 'N/A'}
                                  </Typography>
                                </Grid>
                                <Grid item xs={6} md={3}>
                                  <Typography variant="caption" color="textSecondary">Total Contacts</Typography>
                                  <Typography variant="body2">{contacts.length}</Typography>
                                </Grid>
                              </Grid>
                            </Paper>
                          </Grid>
                        </Grid>
                      </TabPanel>

                      {/* Contacts Tab */}
                      <TabPanel value={tabValue} index={1}>
                        <Table size="small">
                          <TableHead>
                            <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                              <TableCell><strong>Name</strong></TableCell>
                              <TableCell><strong>Job Title</strong></TableCell>
                              <TableCell><strong>Email</strong></TableCell>
                              <TableCell><strong>Phone</strong></TableCell>
                              <TableCell><strong>Role</strong></TableCell>
                              <TableCell align="center"><strong>Social</strong></TableCell>
                            </TableRow>
                          </TableHead>
                          <TableBody>
                            {contacts.map((contact) => (
                              <TableRow key={contact.id} hover>
                                <TableCell>
                                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                    <Avatar sx={{ width: 32, height: 32, bgcolor: '#6750A4' }}>
                                      {contact.firstName?.[0]}{contact.lastName?.[0]}
                                    </Avatar>
                                    <Box>
                                      <Typography variant="body2" fontWeight={500}>
                                        {contact.firstName} {contact.lastName}
                                      </Typography>
                                      {contact.isPrimaryContact && (
                                        <Chip label="Primary" size="small" color="primary" sx={{ height: 18, fontSize: 10 }} />
                                      )}
                                    </Box>
                                  </Box>
                                </TableCell>
                                <TableCell>{contact.jobTitle || '—'}</TableCell>
                                <TableCell>{contact.emailPrimary || '—'}</TableCell>
                                <TableCell>{contact.phonePrimary || '—'}</TableCell>
                                <TableCell>{contact.role || '—'}</TableCell>
                                <TableCell align="center">
                                  {contact.linkedInUrl && (
                                    <IconButton href={contact.linkedInUrl} target="_blank" size="small">
                                      <LinkedInIcon sx={{ color: '#0077b5', fontSize: 18 }} />
                                    </IconButton>
                                  )}
                                  {contact.twitterHandle && (
                                    <IconButton href={`https://twitter.com/${contact.twitterHandle}`} target="_blank" size="small">
                                      <TwitterIcon sx={{ color: '#1da1f2', fontSize: 18 }} />
                                    </IconButton>
                                  )}
                                </TableCell>
                              </TableRow>
                            ))}
                            {contacts.length === 0 && (
                              <TableRow>
                                <TableCell colSpan={6} align="center" sx={{ py: 4, color: 'textSecondary' }}>
                                  No contacts found for this customer
                                </TableCell>
                              </TableRow>
                            )}
                          </TableBody>
                        </Table>
                      </TabPanel>

                      {/* News & Social Tab */}
                      <TabPanel value={tabValue} index={2}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                          {apiStatus && !apiStatus.newsApiConfigured && !apiStatus.socialApiConfigured && (
                            <Alert severity="info" sx={{ flex: 1, mr: 2 }}>
                              External APIs not configured. Configure NewsAPI and Twitter/LinkedIn API keys in settings to fetch real data.
                            </Alert>
                          )}
                          <Button
                            startIcon={loadingNews ? <CircularProgress size={16} /> : <RefreshIcon />}
                            onClick={refreshNewsFeed}
                            disabled={loadingNews}
                            size="small"
                          >
                            Refresh Feeds
                          </Button>
                        </Box>

                        <Grid container spacing={3}>
                          {/* News Feed */}
                          <Grid item xs={12} md={6}>
                            <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
                              <NewsIcon color="primary" /> Latest News
                              {apiStatus?.newsApiConfigured && (
                                <Chip label="Live" size="small" color="success" sx={{ ml: 1 }} />
                              )}
                            </Typography>
                            {newsItems.length === 0 ? (
                              <Paper sx={{ p: 3, textAlign: 'center', borderRadius: 2 }}>
                                <NewsIcon sx={{ fontSize: 48, color: '#E8DEF8', mb: 1 }} />
                                <Typography variant="body2" color="textSecondary">
                                  {apiStatus?.newsApiConfigured 
                                    ? 'No news found for this company'
                                    : 'Configure NewsAPI key to fetch company news'}
                                </Typography>
                              </Paper>
                            ) : (
                            <List>
                              {newsItems.map((news) => (
                                <Paper key={news.id} sx={{ p: 2, mb: 2, borderRadius: 2 }}>
                                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                                    <Box sx={{ flex: 1 }}>
                                      <Typography variant="body2" fontWeight={500}>
                                        {news.title}
                                      </Typography>
                                      <Typography variant="caption" color="textSecondary" sx={{ display: 'block', mt: 0.5 }}>
                                        {news.source} • {new Date(news.publishedAt).toLocaleDateString()}
                                      </Typography>
                                      {news.summary && (
                                        <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>
                                          {news.summary}
                                        </Typography>
                                      )}
                                    </Box>
                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                      <Chip
                                        label={news.sentiment}
                                        size="small"
                                        sx={{ 
                                          backgroundColor: getSentimentColor(news.sentiment),
                                          color: 'white',
                                          textTransform: 'capitalize'
                                        }}
                                      />
                                      <IconButton href={news.url} target="_blank" size="small">
                                        <OpenInNewIcon fontSize="small" />
                                      </IconButton>
                                    </Box>
                                  </Box>
                                </Paper>
                              ))}
                            </List>
                            )}
                          </Grid>

                          {/* Social Feeds */}
                          <Grid item xs={12} md={6}>
                            <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
                              <SocialIcon color="primary" /> Social Media
                              {apiStatus?.socialApiConfigured && (
                                <Chip label="Live" size="small" color="success" sx={{ ml: 1 }} />
                              )}
                            </Typography>
                            {socialFeeds.length === 0 ? (
                              <Paper sx={{ p: 3, textAlign: 'center', borderRadius: 2 }}>
                                <SocialIcon sx={{ fontSize: 48, color: '#E8DEF8', mb: 1 }} />
                                <Typography variant="body2" color="textSecondary">
                                  {apiStatus?.socialApiConfigured 
                                    ? 'No social posts found'
                                    : 'Configure Twitter/LinkedIn API keys to fetch social feeds'}
                                </Typography>
                              </Paper>
                            ) : (
                            <List>
                              {socialFeeds.map((feed) => (
                                <Paper key={feed.id} sx={{ p: 2, mb: 2, borderRadius: 2 }}>
                                  <Box sx={{ display: 'flex', gap: 2 }}>
                                    {getPlatformIcon(feed.platform)}
                                    <Box sx={{ flex: 1 }}>
                                      <Typography variant="body2" fontWeight={500}>
                                        {feed.authorHandle ? `${feed.author} (${feed.authorHandle})` : feed.author}
                                      </Typography>
                                      <Typography variant="body2" sx={{ mt: 0.5 }}>
                                        {feed.content}
                                      </Typography>
                                      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mt: 1 }}>
                                        <Typography variant="caption" color="textSecondary">
                                          {new Date(feed.publishedAt).toLocaleDateString()} • {feed.engagementCount} engagements
                                        </Typography>
                                        {feed.url && (
                                          <IconButton href={feed.url} target="_blank" size="small">
                                            <OpenInNewIcon fontSize="small" />
                                          </IconButton>
                                        )}
                                      </Box>
                                    </Box>
                                  </Box>
                                </Paper>
                              ))}
                            </List>
                            )}
                          </Grid>
                        </Grid>
                      </TabPanel>
                    </>
                  )}
                </CardContent>
              </Card>
            ) : (
              <Card sx={{ borderRadius: 3, boxShadow: 1 }}>
                <CardContent sx={{ textAlign: 'center', py: 10 }}>
                  <BusinessIcon sx={{ fontSize: 80, color: '#E8DEF8', mb: 2 }} />
                  <Typography variant="h6" color="textSecondary">
                    Select an account to view details
                  </Typography>
                  <Typography variant="body2" color="textSecondary">
                    Use the search panel on the left to find and select an account
                  </Typography>
                </CardContent>
              </Card>
            )}
          </Grid>
        </Grid>
      </Container>
    </Box>
  );
}

export default CustomerOverviewPage;
