import { 
  Box, Container, Typography, Card, CardContent, Table, TableBody, TableCell, 
  TableHead, TableRow, CircularProgress, Alert, Button, Dialog, DialogTitle, 
  DialogContent, DialogActions, TextField, FormControl, InputLabel, Select, 
  MenuItem, Chip, Tabs, Tab, Slider, FormControlLabel, Checkbox, Grid,
  SelectChangeEvent, IconButton, Tooltip
} from '@mui/material';
import { useState, useEffect } from 'react';
import { 
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, 
  Business as BusinessIcon, Person as PersonIcon, Email as EmailIcon,
  Phone as PhoneIcon, LinkedIn as LinkedInIcon, Twitter as TwitterIcon
} from '@mui/icons-material';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';

// Enums matching backend
const LIFECYCLE_STAGES = [
  { value: 0, label: 'Lead', color: '#9e9e9e' },
  { value: 1, label: 'Prospect', color: '#2196f3' },
  { value: 2, label: 'Opportunity', color: '#ff9800' },
  { value: 3, label: 'Customer', color: '#4caf50' },
  { value: 4, label: 'Churned', color: '#f44336' },
  { value: 5, label: 'Reactivated', color: '#9c27b0' },
];

const CUSTOMER_TYPES = [
  { value: 0, label: 'Individual' },
  { value: 1, label: 'Small Business' },
  { value: 2, label: 'Mid-Market' },
  { value: 3, label: 'Enterprise' },
  { value: 4, label: 'Government' },
  { value: 5, label: 'Non-Profit' },
];

const PRIORITIES = [
  { value: 0, label: 'Low', color: '#9e9e9e' },
  { value: 1, label: 'Medium', color: '#2196f3' },
  { value: 2, label: 'High', color: '#ff9800' },
  { value: 3, label: 'Critical', color: '#f44336' },
];

const INDUSTRIES = [
  'Technology', 'Healthcare', 'Finance', 'Retail', 'Manufacturing', 
  'Education', 'Real Estate', 'Consulting', 'Marketing', 'Legal', 
  'Non-Profit', 'Government', 'Other'
];

const LEAD_SOURCES = [
  'Website', 'Referral', 'Social Media', 'Cold Call', 'Trade Show', 
  'Advertisement', 'Email Campaign', 'Partner', 'Other'
];

interface Customer {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  secondaryEmail?: string;
  phone: string;
  mobilePhone?: string;
  company: string;
  jobTitle?: string;
  website?: string;
  address: string;
  address2?: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  industry?: string;
  numberOfEmployees?: number;
  annualRevenue: number;
  customerType: number;
  priority: number;
  lifecycleStage: number;
  leadSource?: string;
  firstContactDate?: string;
  conversionDate?: string;
  lastActivityDate?: string;
  nextFollowUpDate?: string;
  totalPurchases: number;
  accountBalance: number;
  creditLimit: number;
  paymentTerms?: string;
  leadScore: number;
  customerHealthScore: number;
  npsScore: number;
  satisfactionRating: number;
  linkedInUrl?: string;
  twitterHandle?: string;
  optInEmail: boolean;
  optInSms: boolean;
  optInPhone: boolean;
  preferredContactMethod?: string;
  timezone?: string;
  assignedToUserId?: number;
  territory?: string;
  tags?: string;
  segment?: string;
  notes: string;
  description?: string;
  createdAt: string;
}

interface CustomerForm {
  firstName: string;
  lastName: string;
  email: string;
  secondaryEmail: string;
  phone: string;
  mobilePhone: string;
  company: string;
  jobTitle: string;
  website: string;
  address: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  industry: string;
  numberOfEmployees: number;
  annualRevenue: number;
  customerType: number;
  priority: number;
  lifecycleStage: number;
  leadSource: string;
  leadScore: number;
  creditLimit: number;
  paymentTerms: string;
  linkedInUrl: string;
  twitterHandle: string;
  optInEmail: boolean;
  optInSms: boolean;
  optInPhone: boolean;
  preferredContactMethod: string;
  timezone: string;
  territory: string;
  tags: string;
  notes: string;
  description: string;
}

interface User {
  id: number;
  firstName: string;
  lastName: string;
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div role="tabpanel" hidden={value !== index} {...other}>
      {value === index && <Box sx={{ pt: 2 }}>{children}</Box>}
    </div>
  );
}

function CustomersPage() {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [dialogTab, setDialogTab] = useState(0);
  const [formData, setFormData] = useState<CustomerForm>({
    firstName: '',
    lastName: '',
    email: '',
    secondaryEmail: '',
    phone: '',
    mobilePhone: '',
    company: '',
    jobTitle: '',
    website: '',
    address: '',
    city: '',
    state: '',
    zipCode: '',
    country: 'USA',
    industry: '',
    numberOfEmployees: 0,
    annualRevenue: 0,
    customerType: 0,
    priority: 1,
    lifecycleStage: 0,
    leadSource: '',
    leadScore: 0,
    creditLimit: 0,
    paymentTerms: 'Net 30',
    linkedInUrl: '',
    twitterHandle: '',
    optInEmail: true,
    optInSms: false,
    optInPhone: true,
    preferredContactMethod: 'Email',
    timezone: '',
    territory: '',
    tags: '',
    notes: '',
    description: '',
  });

  useEffect(() => {
    fetchCustomers();
    fetchUsers();
  }, []);

  const fetchCustomers = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/customers');
      setCustomers(response.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch customers');
    } finally {
      setLoading(false);
    }
  };

  const fetchUsers = async () => {
    try {
      const response = await apiClient.get('/users');
      setUsers(response.data);
    } catch (err) {
      console.error('Error fetching users:', err);
    }
  };

  const handleOpenDialog = (customer?: Customer) => {
    setDialogTab(0);
    if (customer) {
      setEditingId(customer.id);
      setFormData({
        firstName: customer.firstName,
        lastName: customer.lastName,
        email: customer.email,
        secondaryEmail: customer.secondaryEmail || '',
        phone: customer.phone,
        mobilePhone: customer.mobilePhone || '',
        company: customer.company,
        jobTitle: customer.jobTitle || '',
        website: customer.website || '',
        address: customer.address,
        city: customer.city,
        state: customer.state,
        zipCode: customer.zipCode,
        country: customer.country,
        industry: customer.industry || '',
        numberOfEmployees: customer.numberOfEmployees || 0,
        annualRevenue: customer.annualRevenue,
        customerType: customer.customerType,
        priority: customer.priority,
        lifecycleStage: customer.lifecycleStage,
        leadSource: customer.leadSource || '',
        leadScore: customer.leadScore,
        creditLimit: customer.creditLimit,
        paymentTerms: customer.paymentTerms || 'Net 30',
        linkedInUrl: customer.linkedInUrl || '',
        twitterHandle: customer.twitterHandle || '',
        optInEmail: customer.optInEmail,
        optInSms: customer.optInSms,
        optInPhone: customer.optInPhone,
        preferredContactMethod: customer.preferredContactMethod || 'Email',
        timezone: customer.timezone || '',
        territory: customer.territory || '',
        tags: customer.tags || '',
        notes: customer.notes,
        description: customer.description || '',
      });
    } else {
      setEditingId(null);
      setFormData({
        firstName: '', lastName: '', email: '', secondaryEmail: '', phone: '', mobilePhone: '',
        company: '', jobTitle: '', website: '', address: '', city: '', state: '', zipCode: '',
        country: 'USA', industry: '', numberOfEmployees: 0, annualRevenue: 0, customerType: 0,
        priority: 1, lifecycleStage: 0, leadSource: '', leadScore: 0, creditLimit: 0,
        paymentTerms: 'Net 30', linkedInUrl: '', twitterHandle: '', optInEmail: true,
        optInSms: false, optInPhone: true, preferredContactMethod: 'Email', timezone: '',
        territory: '', tags: '', notes: '', description: '',
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    const checked = (e.target as HTMLInputElement).checked;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : type === 'number' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleSelectChange = (e: SelectChangeEvent<string | number>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSaveCustomer = async () => {
    if (!formData.firstName.trim() || !formData.lastName.trim() || !formData.email.trim()) {
      setError('Please fill in required fields (First Name, Last Name, Email)');
      return;
    }

    try {
      if (editingId) {
        await apiClient.put(`/customers/${editingId}`, formData);
        setSuccessMessage('Customer updated successfully');
      } else {
        await apiClient.post('/customers', formData);
        setSuccessMessage('Customer created successfully');
      }
      handleCloseDialog();
      fetchCustomers();
      setError(null);
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save customer');
    }
  };

  const handleDeleteCustomer = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this customer?')) {
      try {
        await apiClient.delete(`/customers/${id}`);
        setSuccessMessage('Customer deleted successfully');
        fetchCustomers();
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete customer');
      }
    }
  };

  const getLifecycleStage = (value: number) => LIFECYCLE_STAGES.find(s => s.value === value);
  const getPriority = (value: number) => PRIORITIES.find(p => p.value === value);
  const getCustomerType = (value: number) => CUSTOMER_TYPES.find(t => t.value === value);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ py: 4 }}>
      <Container maxWidth="xl">
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Box sx={{ width: 40, height: 40, flexShrink: 0 }}>
              <img src={logo} alt="CRM Logo" style={{ width: "100%", height: "100%", objectFit: "contain" }} />
            </Box>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>Customers</Typography>
          </Box>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => handleOpenDialog()}
            sx={{ backgroundColor: '#6750A4' }}
          >
            Add Customer
          </Button>
        </Box>
        
        {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}
        
        <Card>
          <CardContent sx={{ p: 0 }}>
            <Table>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#F5EFF7' }}>
                  <TableCell><strong>Name</strong></TableCell>
                  <TableCell><strong>Company</strong></TableCell>
                  <TableCell><strong>Contact</strong></TableCell>
                  <TableCell><strong>Type</strong></TableCell>
                  <TableCell><strong>Stage</strong></TableCell>
                  <TableCell><strong>Priority</strong></TableCell>
                  <TableCell><strong>Revenue</strong></TableCell>
                  <TableCell><strong>Score</strong></TableCell>
                  <TableCell align="center"><strong>Actions</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {customers.map((customer) => {
                  const stage = getLifecycleStage(customer.lifecycleStage);
                  const priority = getPriority(customer.priority);
                  const type = getCustomerType(customer.customerType);
                  return (
                    <TableRow key={customer.id} hover>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <PersonIcon fontSize="small" sx={{ color: '#6750A4' }} />
                          <Box>
                            <Typography fontWeight={500}>{customer.firstName} {customer.lastName}</Typography>
                            {customer.jobTitle && (
                              <Typography variant="caption" color="textSecondary">{customer.jobTitle}</Typography>
                            )}
                          </Box>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <BusinessIcon fontSize="small" sx={{ color: '#666' }} />
                          <Box>
                            <Typography>{customer.company || '—'}</Typography>
                            {customer.industry && (
                              <Typography variant="caption" color="textSecondary">{customer.industry}</Typography>
                            )}
                          </Box>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                            <EmailIcon fontSize="small" sx={{ color: '#666', fontSize: 14 }} />
                            <Typography variant="body2">{customer.email}</Typography>
                          </Box>
                          {customer.phone && (
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                              <PhoneIcon fontSize="small" sx={{ color: '#666', fontSize: 14 }} />
                              <Typography variant="body2">{customer.phone}</Typography>
                            </Box>
                          )}
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Chip label={type?.label || 'Unknown'} size="small" variant="outlined" />
                      </TableCell>
                      <TableCell>
                        <Chip 
                          label={stage?.label || 'Unknown'} 
                          size="small" 
                          sx={{ backgroundColor: stage?.color, color: 'white' }} 
                        />
                      </TableCell>
                      <TableCell>
                        <Chip 
                          label={priority?.label || 'Medium'} 
                          size="small" 
                          sx={{ backgroundColor: priority?.color, color: 'white' }} 
                        />
                      </TableCell>
                      <TableCell>
                        ${customer.annualRevenue?.toLocaleString() || 0}
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Typography variant="body2">{customer.leadScore}</Typography>
                          <Box sx={{ 
                            width: 40, 
                            height: 6, 
                            backgroundColor: '#e0e0e0', 
                            borderRadius: 3,
                            overflow: 'hidden'
                          }}>
                            <Box sx={{ 
                              width: `${customer.leadScore}%`, 
                              height: '100%', 
                              backgroundColor: customer.leadScore > 70 ? '#4caf50' : customer.leadScore > 40 ? '#ff9800' : '#f44336',
                              borderRadius: 3
                            }} />
                          </Box>
                        </Box>
                      </TableCell>
                      <TableCell align="center">
                        <Tooltip title="Edit">
                          <IconButton size="small" onClick={() => handleOpenDialog(customer)} sx={{ color: '#6750A4' }}>
                            <EditIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
                          <IconButton size="small" onClick={() => handleDeleteCustomer(customer.id)} sx={{ color: '#f44336' }}>
                            <DeleteIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
            {customers.length === 0 && (
              <Typography sx={{ textAlign: 'center', py: 4, color: 'textSecondary' }}>
                No customers found. Add your first customer to get started.
              </Typography>
            )}
          </CardContent>
        </Card>
      </Container>

      {/* Enhanced Add/Edit Customer Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle sx={{ pb: 0 }}>{editingId ? 'Edit Customer' : 'Add Customer'}</DialogTitle>
        <Box sx={{ borderBottom: 1, borderColor: 'divider', px: 3 }}>
          <Tabs value={dialogTab} onChange={(_, v) => setDialogTab(v)}>
            <Tab label="Basic Info" />
            <Tab label="Business" />
            <Tab label="Contact Preferences" />
            <Tab label="Additional" />
          </Tabs>
        </Box>
        <DialogContent sx={{ pt: 2, minHeight: 400 }}>
          <TabPanel value={dialogTab} index={0}>
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <TextField fullWidth label="First Name" name="firstName" value={formData.firstName} onChange={handleInputChange} required />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Last Name" name="lastName" value={formData.lastName} onChange={handleInputChange} required />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Email" name="email" type="email" value={formData.email} onChange={handleInputChange} required />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Secondary Email" name="secondaryEmail" type="email" value={formData.secondaryEmail} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Phone" name="phone" value={formData.phone} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Mobile Phone" name="mobilePhone" value={formData.mobilePhone} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Company" name="company" value={formData.company} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Job Title" name="jobTitle" value={formData.jobTitle} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Address" name="address" value={formData.address} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="City" name="city" value={formData.city} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="State" name="state" value={formData.state} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={4}>
                <TextField fullWidth label="Zip Code" name="zipCode" value={formData.zipCode} onChange={handleInputChange} />
              </Grid>
            </Grid>
          </TabPanel>

          <TabPanel value={dialogTab} index={1}>
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <FormControl fullWidth>
                  <InputLabel>Customer Type</InputLabel>
                  <Select name="customerType" value={formData.customerType} onChange={handleSelectChange} label="Customer Type">
                    {CUSTOMER_TYPES.map(t => <MenuItem key={t.value} value={t.value}>{t.label}</MenuItem>)}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={6}>
                <FormControl fullWidth>
                  <InputLabel>Lifecycle Stage</InputLabel>
                  <Select name="lifecycleStage" value={formData.lifecycleStage} onChange={handleSelectChange} label="Lifecycle Stage">
                    {LIFECYCLE_STAGES.map(s => (
                      <MenuItem key={s.value} value={s.value}>
                        <Chip label={s.label} size="small" sx={{ backgroundColor: s.color, color: 'white' }} />
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={6}>
                <FormControl fullWidth>
                  <InputLabel>Priority</InputLabel>
                  <Select name="priority" value={formData.priority} onChange={handleSelectChange} label="Priority">
                    {PRIORITIES.map(p => (
                      <MenuItem key={p.value} value={p.value}>
                        <Chip label={p.label} size="small" sx={{ backgroundColor: p.color, color: 'white' }} />
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={6}>
                <FormControl fullWidth>
                  <InputLabel>Industry</InputLabel>
                  <Select name="industry" value={formData.industry} onChange={handleSelectChange} label="Industry">
                    {INDUSTRIES.map(i => <MenuItem key={i} value={i}>{i}</MenuItem>)}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Annual Revenue ($)" name="annualRevenue" type="number" value={formData.annualRevenue} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Number of Employees" name="numberOfEmployees" type="number" value={formData.numberOfEmployees} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Credit Limit ($)" name="creditLimit" type="number" value={formData.creditLimit} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={6}>
                <FormControl fullWidth>
                  <InputLabel>Lead Source</InputLabel>
                  <Select name="leadSource" value={formData.leadSource} onChange={handleSelectChange} label="Lead Source">
                    {LEAD_SOURCES.map(s => <MenuItem key={s} value={s}>{s}</MenuItem>)}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12}>
                <Typography gutterBottom>Lead Score: {formData.leadScore}</Typography>
                <Slider
                  value={formData.leadScore}
                  onChange={(_, v) => setFormData(prev => ({ ...prev, leadScore: v as number }))}
                  min={0}
                  max={100}
                  valueLabelDisplay="auto"
                  sx={{ color: formData.leadScore > 70 ? '#4caf50' : formData.leadScore > 40 ? '#ff9800' : '#f44336' }}
                />
              </Grid>
            </Grid>
          </TabPanel>

          <TabPanel value={dialogTab} index={2}>
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <FormControl fullWidth>
                  <InputLabel>Preferred Contact Method</InputLabel>
                  <Select name="preferredContactMethod" value={formData.preferredContactMethod} onChange={handleSelectChange} label="Preferred Contact Method">
                    <MenuItem value="Email">Email</MenuItem>
                    <MenuItem value="Phone">Phone</MenuItem>
                    <MenuItem value="SMS">SMS</MenuItem>
                    <MenuItem value="Mail">Mail</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Timezone" name="timezone" value={formData.timezone} onChange={handleInputChange} placeholder="e.g., America/New_York" />
              </Grid>
              <Grid item xs={12}>
                <Typography variant="subtitle2" gutterBottom>Communication Opt-In</Typography>
                <FormControlLabel
                  control={<Checkbox name="optInEmail" checked={formData.optInEmail} onChange={handleInputChange} />}
                  label="Email Communications"
                />
                <FormControlLabel
                  control={<Checkbox name="optInPhone" checked={formData.optInPhone} onChange={handleInputChange} />}
                  label="Phone Calls"
                />
                <FormControlLabel
                  control={<Checkbox name="optInSms" checked={formData.optInSms} onChange={handleInputChange} />}
                  label="SMS Messages"
                />
              </Grid>
              <Grid item xs={6}>
                <TextField 
                  fullWidth 
                  label="LinkedIn URL" 
                  name="linkedInUrl" 
                  value={formData.linkedInUrl} 
                  onChange={handleInputChange}
                  InputProps={{ startAdornment: <LinkedInIcon sx={{ mr: 1, color: '#0077b5' }} /> }}
                />
              </Grid>
              <Grid item xs={6}>
                <TextField 
                  fullWidth 
                  label="Twitter Handle" 
                  name="twitterHandle" 
                  value={formData.twitterHandle} 
                  onChange={handleInputChange}
                  InputProps={{ startAdornment: <TwitterIcon sx={{ mr: 1, color: '#1da1f2' }} /> }}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Website" name="website" value={formData.website} onChange={handleInputChange} />
              </Grid>
            </Grid>
          </TabPanel>

          <TabPanel value={dialogTab} index={3}>
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <TextField fullWidth label="Territory" name="territory" value={formData.territory} onChange={handleInputChange} />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Payment Terms" name="paymentTerms" value={formData.paymentTerms} onChange={handleInputChange} placeholder="e.g., Net 30" />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Tags (comma-separated)" name="tags" value={formData.tags} onChange={handleInputChange} placeholder="vip, enterprise, priority" />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Description" name="description" value={formData.description} onChange={handleInputChange} multiline rows={2} />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Notes" name="notes" value={formData.notes} onChange={handleInputChange} multiline rows={3} />
              </Grid>
            </Grid>
          </TabPanel>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSaveCustomer} variant="contained" sx={{ backgroundColor: '#6750A4' }}>
            {editingId ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default CustomersPage;
