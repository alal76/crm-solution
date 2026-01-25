import React, { useState, useEffect } from 'react';
import { TabPanel } from '../components/common';
import { BaseEntity } from '../types';
import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  Grid,
  Tabs,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Button,
  IconButton,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  CircularProgress,
  Alert,
  Badge,
  Avatar,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  ListItemSecondaryAction,
  Divider,
  Tooltip,
  InputAdornment,
} from '@mui/material';
import {
  Email as EmailIcon,
  WhatsApp as WhatsAppIcon,
  Twitter as TwitterIcon,
  Facebook as FacebookIcon,
  Send as SendIcon,
  Inbox as InboxIcon,
  Drafts as DraftsIcon,
  Archive as ArchiveIcon,
  Star as StarIcon,
  StarBorder as StarBorderIcon,
  Delete as DeleteIcon,
  Refresh as RefreshIcon,
  Add as AddIcon,
  Search as SearchIcon,
  FilterList as FilterIcon,
  MarkEmailRead as ReadIcon,
  MarkEmailUnread as UnreadIcon,
  AttachFile as AttachmentIcon,
  Reply as ReplyIcon,
  Forum as ConversationIcon,
  Settings as SettingsIcon,
  TrendingUp as StatsIcon,
} from '@mui/icons-material';
import apiClient from '../services/apiClient';

// Types
interface Message extends BaseEntity {
  channelType: string;
  subject?: string;
  bodyPreview?: string;
  direction: string;
  status: string;
  fromAddress?: string;
  fromName?: string;
  toAddress?: string;
  toName?: string;
  attachmentCount: number;
  isRead: boolean;
  isStarred: boolean;
  sentAt?: string;
  receivedAt?: string;
  customerId?: number;
  customerName?: string;
  contactId?: number;
  contactName?: string;
}

interface Conversation extends BaseEntity {
  conversationId: string;
  channelType: string;
  subject?: string;
  lastMessagePreview?: string;
  status: string;
  participantAddress?: string;
  participantName?: string;
  messageCount: number;
  unreadCount: number;
  lastMessageAt?: string;
  isStarred: boolean;
  isPinned: boolean;
  customerId?: number;
  customerName?: string;
}

interface Channel extends BaseEntity {
  channelType: string;
  name: string;
  status: string;
  isEnabled: boolean;
  isDefault: boolean;
  socialUsername?: string;
  fromEmail?: string;
  lastConnectedAt?: string;
}

interface Stats {
  totalMessages: number;
  totalInbound: number;
  totalOutbound: number;
  unreadCount: number;
  openConversations: number;
  pendingMessages: number;
  emailStats?: ChannelStats;
  whatsAppStats?: ChannelStats;
  twitterStats?: ChannelStats;
  facebookStats?: ChannelStats;
}

interface ChannelStats {
  channelType: string;
  totalMessages: number;
  inbound: number;
  outbound: number;
  unread: number;
  pending: number;
  failed: number;
}

function CommunicationsPage() {
  const [tabValue, setTabValue] = useState(0);
  const [messages, setMessages] = useState<Message[]>([]);
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [channels, setChannels] = useState<Channel[]>([]);
  const [stats, setStats] = useState<Stats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  
  // Filters
  const [channelFilter, setChannelFilter] = useState<string>('all');
  const [directionFilter, setDirectionFilter] = useState<string>('all');
  const [searchQuery, setSearchQuery] = useState('');
  
  // Compose dialog
  const [composeOpen, setComposeOpen] = useState(false);
  const [composeForm, setComposeForm] = useState({
    channelId: 0,
    channelType: 'Email',
    toAddress: '',
    toName: '',
    subject: '',
    body: '',
  });
  
  // View message dialog
  const [viewMessageOpen, setViewMessageOpen] = useState(false);
  const [selectedMessage, setSelectedMessage] = useState<Message | null>(null);
  const [messageDetails, setMessageDetails] = useState<any>(null);

  useEffect(() => {
    fetchData();
  }, []);

  useEffect(() => {
    fetchMessages();
  }, [channelFilter, directionFilter]);

  const fetchData = async () => {
    setLoading(true);
    try {
      const [messagesRes, conversationsRes, channelsRes, statsRes] = await Promise.all([
        apiClient.get('/communications/messages'),
        apiClient.get('/communications/conversations'),
        apiClient.get('/communications/channels'),
        apiClient.get('/communications/stats'),
      ]);
      setMessages(messagesRes.data);
      setConversations(conversationsRes.data);
      setChannels(channelsRes.data);
      setStats(statsRes.data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load communications');
    } finally {
      setLoading(false);
    }
  };

  const fetchMessages = async () => {
    try {
      const params: any = {};
      if (channelFilter !== 'all') params.channelType = channelFilter;
      if (directionFilter !== 'all') params.direction = directionFilter;
      const response = await apiClient.get('/communications/messages', { params });
      setMessages(response.data);
    } catch (err) {
      console.error('Failed to fetch messages', err);
    }
  };

  const handleViewMessage = async (message: Message) => {
    setSelectedMessage(message);
    try {
      const response = await apiClient.get(`/communications/messages/${message.id}`);
      setMessageDetails(response.data);
      setViewMessageOpen(true);
    } catch (err) {
      setError('Failed to load message details');
    }
  };

  const handleStarMessage = async (id: number, isStarred: boolean) => {
    try {
      await apiClient.patch(`/communications/messages/${id}/star?isStarred=${!isStarred}`);
      setMessages(messages.map(m => m.id === id ? { ...m, isStarred: !isStarred } : m));
    } catch (err) {
      setError('Failed to update message');
    }
  };

  const handleArchiveMessage = async (id: number) => {
    try {
      await apiClient.patch(`/communications/messages/${id}/archive`);
      setMessages(messages.filter(m => m.id !== id));
      setSuccess('Message archived');
    } catch (err) {
      setError('Failed to archive message');
    }
  };

  const handleDeleteMessage = async (id: number) => {
    try {
      await apiClient.delete(`/communications/messages/${id}`);
      setMessages(messages.filter(m => m.id !== id));
      setSuccess('Message deleted');
    } catch (err) {
      setError('Failed to delete message');
    }
  };

  const handleSendMessage = async () => {
    try {
      await apiClient.post('/communications/messages/send', composeForm);
      setComposeOpen(false);
      setComposeForm({
        channelId: 0,
        channelType: 'Email',
        toAddress: '',
        toName: '',
        subject: '',
        body: '',
      });
      setSuccess('Message sent successfully');
      fetchMessages();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to send message');
    }
  };

  const getChannelIcon = (channelType: string) => {
    switch (channelType?.toLowerCase()) {
      case 'email': return <EmailIcon />;
      case 'whatsapp': return <WhatsAppIcon sx={{ color: '#25D366' }} />;
      case 'twitter': return <TwitterIcon sx={{ color: '#1DA1F2' }} />;
      case 'facebook': return <FacebookIcon sx={{ color: '#4267B2' }} />;
      default: return <EmailIcon />;
    }
  };

  const getChannelColor = (channelType: string) => {
    switch (channelType?.toLowerCase()) {
      case 'email': return 'primary';
      case 'whatsapp': return 'success';
      case 'twitter': return 'info';
      case 'facebook': return 'secondary';
      default: return 'default';
    }
  };

  const getStatusColor = (status: string): "default" | "primary" | "secondary" | "error" | "info" | "success" | "warning" => {
    switch (status?.toLowerCase()) {
      case 'sent': return 'success';
      case 'delivered': return 'success';
      case 'read': return 'info';
      case 'failed': return 'error';
      case 'queued': return 'warning';
      case 'draft': return 'default';
      default: return 'default';
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '50vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Container maxWidth="xl" sx={{ mt: 4, mb: 4 }}>
      {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccess(null)}>{success}</Alert>}

      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1">
          Communications Hub
        </Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => setComposeOpen(true)}
          >
            Compose
          </Button>
          <IconButton onClick={fetchData}>
            <RefreshIcon />
          </IconButton>
        </Box>
      </Box>

      {/* Stats Cards */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={2}>
          <Card>
            <CardContent sx={{ textAlign: 'center', py: 2 }}>
              <Typography variant="h4" color="primary">{stats?.totalMessages || 0}</Typography>
              <Typography variant="body2" color="text.secondary">Total Messages</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2}>
          <Card>
            <CardContent sx={{ textAlign: 'center', py: 2 }}>
              <Badge badgeContent={stats?.unreadCount || 0} color="error">
                <Typography variant="h4" color="info.main">{stats?.totalInbound || 0}</Typography>
              </Badge>
              <Typography variant="body2" color="text.secondary">Received</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2}>
          <Card>
            <CardContent sx={{ textAlign: 'center', py: 2 }}>
              <Typography variant="h4" color="success.main">{stats?.totalOutbound || 0}</Typography>
              <Typography variant="body2" color="text.secondary">Sent</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2}>
          <Card>
            <CardContent sx={{ textAlign: 'center', py: 2 }}>
              <Typography variant="h4" color="warning.main">{stats?.openConversations || 0}</Typography>
              <Typography variant="body2" color="text.secondary">Open Threads</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2}>
          <Card>
            <CardContent sx={{ textAlign: 'center', py: 2 }}>
              <Typography variant="h4" color="secondary.main">{stats?.emailStats?.totalMessages || 0}</Typography>
              <Typography variant="body2" color="text.secondary">Emails</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2}>
          <Card>
            <CardContent sx={{ textAlign: 'center', py: 2 }}>
              <Typography variant="h4" sx={{ color: '#25D366' }}>{stats?.whatsAppStats?.totalMessages || 0}</Typography>
              <Typography variant="body2" color="text.secondary">WhatsApp</Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Main Content */}
      <Card>
        <CardContent>
          <Tabs value={tabValue} onChange={(_, v) => setTabValue(v)} sx={{ borderBottom: 1, borderColor: 'divider' }}>
            <Tab icon={<InboxIcon />} label="Inbox" iconPosition="start" />
            <Tab icon={<SendIcon />} label="Sent" iconPosition="start" />
            <Tab icon={<ConversationIcon />} label="Conversations" iconPosition="start" />
            <Tab icon={<EmailIcon />} label="Email" iconPosition="start" />
            <Tab icon={<WhatsAppIcon />} label="WhatsApp" iconPosition="start" />
            <Tab icon={<TwitterIcon />} label="X (Twitter)" iconPosition="start" />
            <Tab icon={<FacebookIcon />} label="Facebook" iconPosition="start" />
            <Tab icon={<SettingsIcon />} label="Channels" iconPosition="start" />
          </Tabs>

          {/* Inbox Tab */}
          <TabPanel value={tabValue} index={0}>
            <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
              <TextField
                size="small"
                placeholder="Search messages..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                InputProps={{
                  startAdornment: <InputAdornment position="start"><SearchIcon /></InputAdornment>
                }}
                sx={{ flexGrow: 1 }}
              />
              <FormControl size="small" sx={{ minWidth: 120 }}>
                <InputLabel>Channel</InputLabel>
                <Select
                  value={channelFilter}
                  label="Channel"
                  onChange={(e) => setChannelFilter(e.target.value)}
                >
                  <MenuItem value="all">All Channels</MenuItem>
                  <MenuItem value="Email">Email</MenuItem>
                  <MenuItem value="WhatsApp">WhatsApp</MenuItem>
                  <MenuItem value="Twitter">X (Twitter)</MenuItem>
                  <MenuItem value="Facebook">Facebook</MenuItem>
                </Select>
              </FormControl>
            </Box>

            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell padding="checkbox" />
                    <TableCell>Channel</TableCell>
                    <TableCell>From/To</TableCell>
                    <TableCell>Subject / Preview</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Date</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {messages.filter(m => m.direction === 'Inbound').map((message) => (
                    <TableRow 
                      key={message.id} 
                      hover 
                      sx={{ 
                        cursor: 'pointer',
                        fontWeight: message.isRead ? 'normal' : 'bold',
                        bgcolor: message.isRead ? 'transparent' : 'action.hover'
                      }}
                      onClick={() => handleViewMessage(message)}
                    >
                      <TableCell padding="checkbox">
                        <IconButton size="small" onClick={(e) => { e.stopPropagation(); handleStarMessage(message.id, message.isStarred); }}>
                          {message.isStarred ? <StarIcon color="warning" /> : <StarBorderIcon />}
                        </IconButton>
                      </TableCell>
                      <TableCell>
                        <Chip
                          icon={getChannelIcon(message.channelType)}
                          label={message.channelType}
                          size="small"
                          color={getChannelColor(message.channelType) as any}
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" fontWeight={message.isRead ? 'normal' : 'bold'}>
                          {message.fromName || message.fromAddress}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" fontWeight={message.isRead ? 'normal' : 'bold'} noWrap sx={{ maxWidth: 300 }}>
                          {message.subject || message.bodyPreview}
                        </Typography>
                        {message.attachmentCount > 0 && (
                          <AttachmentIcon fontSize="small" sx={{ ml: 1, verticalAlign: 'middle', color: 'text.secondary' }} />
                        )}
                      </TableCell>
                      <TableCell>
                        <Chip label={message.status} size="small" color={getStatusColor(message.status)} />
                      </TableCell>
                      <TableCell>
                        <Typography variant="caption">
                          {new Date(message.receivedAt || message.createdAt).toLocaleString()}
                        </Typography>
                      </TableCell>
                      <TableCell align="right">
                        <Tooltip title="Reply">
                          <IconButton size="small" onClick={(e) => { e.stopPropagation(); }}>
                            <ReplyIcon />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Archive">
                          <IconButton size="small" onClick={(e) => { e.stopPropagation(); handleArchiveMessage(message.id); }}>
                            <ArchiveIcon />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
                          <IconButton size="small" onClick={(e) => { e.stopPropagation(); handleDeleteMessage(message.id); }}>
                            <DeleteIcon />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  ))}
                  {messages.filter(m => m.direction === 'Inbound').length === 0 && (
                    <TableRow>
                      <TableCell colSpan={7} align="center" sx={{ py: 4 }}>
                        <Typography color="text.secondary">No messages in inbox</Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </TabPanel>

          {/* Sent Tab */}
          <TabPanel value={tabValue} index={1}>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell padding="checkbox" />
                    <TableCell>Channel</TableCell>
                    <TableCell>To</TableCell>
                    <TableCell>Subject / Preview</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Date</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {messages.filter(m => m.direction === 'Outbound').map((message) => (
                    <TableRow key={message.id} hover sx={{ cursor: 'pointer' }} onClick={() => handleViewMessage(message)}>
                      <TableCell padding="checkbox">
                        <IconButton size="small" onClick={(e) => { e.stopPropagation(); handleStarMessage(message.id, message.isStarred); }}>
                          {message.isStarred ? <StarIcon color="warning" /> : <StarBorderIcon />}
                        </IconButton>
                      </TableCell>
                      <TableCell>
                        <Chip
                          icon={getChannelIcon(message.channelType)}
                          label={message.channelType}
                          size="small"
                          color={getChannelColor(message.channelType) as any}
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">{message.toName || message.toAddress}</Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" noWrap sx={{ maxWidth: 300 }}>
                          {message.subject || message.bodyPreview}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip label={message.status} size="small" color={getStatusColor(message.status)} />
                      </TableCell>
                      <TableCell>
                        <Typography variant="caption">
                          {new Date(message.sentAt || message.createdAt).toLocaleString()}
                        </Typography>
                      </TableCell>
                      <TableCell align="right">
                        <IconButton size="small" onClick={(e) => { e.stopPropagation(); handleDeleteMessage(message.id); }}>
                          <DeleteIcon />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  ))}
                  {messages.filter(m => m.direction === 'Outbound').length === 0 && (
                    <TableRow>
                      <TableCell colSpan={7} align="center" sx={{ py: 4 }}>
                        <Typography color="text.secondary">No sent messages</Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </TabPanel>

          {/* Conversations Tab */}
          <TabPanel value={tabValue} index={2}>
            <List>
              {conversations.map((conv) => (
                <React.Fragment key={conv.id}>
                  <ListItem alignItems="flex-start" sx={{ cursor: 'pointer', '&:hover': { bgcolor: 'action.hover' } }}>
                    <ListItemAvatar>
                      <Badge badgeContent={conv.unreadCount} color="error">
                        <Avatar sx={{ bgcolor: getChannelColor(conv.channelType) + '.main' }}>
                          {getChannelIcon(conv.channelType)}
                        </Avatar>
                      </Badge>
                    </ListItemAvatar>
                    <ListItemText
                      primary={
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Typography variant="subtitle2" fontWeight={conv.unreadCount > 0 ? 'bold' : 'normal'}>
                            {conv.participantName || conv.participantAddress}
                          </Typography>
                          {conv.isPinned && <Chip label="Pinned" size="small" />}
                        </Box>
                      }
                      secondary={
                        <>
                          <Typography component="span" variant="body2" color="text.primary">
                            {conv.subject}
                          </Typography>
                          {' — '}
                          {conv.lastMessagePreview}
                        </>
                      }
                    />
                    <ListItemSecondaryAction>
                      <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end' }}>
                        <Typography variant="caption" color="text.secondary">
                          {conv.lastMessageAt ? new Date(conv.lastMessageAt).toLocaleString() : ''}
                        </Typography>
                        <Chip label={`${conv.messageCount} msgs`} size="small" variant="outlined" sx={{ mt: 0.5 }} />
                      </Box>
                    </ListItemSecondaryAction>
                  </ListItem>
                  <Divider variant="inset" component="li" />
                </React.Fragment>
              ))}
              {conversations.length === 0 && (
                <ListItem>
                  <ListItemText primary="No conversations yet" secondary="Start a conversation by composing a message" />
                </ListItem>
              )}
            </List>
          </TabPanel>

          {/* Email Tab */}
          <TabPanel value={tabValue} index={3}>
            <Typography variant="h6" gutterBottom>Email Messages</Typography>
            <TableContainer>
              <Table size="small">
                <TableBody>
                  {messages.filter(m => m.channelType === 'Email').map((message) => (
                    <TableRow key={message.id} hover onClick={() => handleViewMessage(message)}>
                      <TableCell padding="checkbox">
                        <IconButton size="small" onClick={(e) => { e.stopPropagation(); handleStarMessage(message.id, message.isStarred); }}>
                          {message.isStarred ? <StarIcon color="warning" /> : <StarBorderIcon />}
                        </IconButton>
                      </TableCell>
                      <TableCell>
                        <Chip 
                          label={message.direction === 'Inbound' ? 'Received' : 'Sent'} 
                          size="small" 
                          color={message.direction === 'Inbound' ? 'info' : 'success'} 
                        />
                      </TableCell>
                      <TableCell>{message.direction === 'Inbound' ? message.fromAddress : message.toAddress}</TableCell>
                      <TableCell>{message.subject}</TableCell>
                      <TableCell>
                        <Chip label={message.status} size="small" color={getStatusColor(message.status)} />
                      </TableCell>
                      <TableCell>{new Date(message.createdAt).toLocaleString()}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </TabPanel>

          {/* WhatsApp Tab */}
          <TabPanel value={tabValue} index={4}>
            <Typography variant="h6" gutterBottom>WhatsApp Messages</Typography>
            <TableContainer>
              <Table size="small">
                <TableBody>
                  {messages.filter(m => m.channelType === 'WhatsApp').map((message) => (
                    <TableRow key={message.id} hover onClick={() => handleViewMessage(message)}>
                      <TableCell>{message.direction === 'Inbound' ? message.fromAddress : message.toAddress}</TableCell>
                      <TableCell>{message.bodyPreview}</TableCell>
                      <TableCell>
                        <Chip label={message.status} size="small" color={getStatusColor(message.status)} />
                      </TableCell>
                      <TableCell>{new Date(message.createdAt).toLocaleString()}</TableCell>
                    </TableRow>
                  ))}
                  {messages.filter(m => m.channelType === 'WhatsApp').length === 0 && (
                    <TableRow>
                      <TableCell colSpan={4} align="center">No WhatsApp messages</TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </TabPanel>

          {/* Twitter Tab */}
          <TabPanel value={tabValue} index={5}>
            <Typography variant="h6" gutterBottom>X (Twitter) Messages</Typography>
            <TableContainer>
              <Table size="small">
                <TableBody>
                  {messages.filter(m => m.channelType === 'Twitter').map((message) => (
                    <TableRow key={message.id} hover onClick={() => handleViewMessage(message)}>
                      <TableCell>{message.direction === 'Inbound' ? message.fromAddress : message.toAddress}</TableCell>
                      <TableCell>{message.bodyPreview}</TableCell>
                      <TableCell>
                        <Chip label={message.status} size="small" color={getStatusColor(message.status)} />
                      </TableCell>
                      <TableCell>{new Date(message.createdAt).toLocaleString()}</TableCell>
                    </TableRow>
                  ))}
                  {messages.filter(m => m.channelType === 'Twitter').length === 0 && (
                    <TableRow>
                      <TableCell colSpan={4} align="center">No X messages</TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </TabPanel>

          {/* Facebook Tab */}
          <TabPanel value={tabValue} index={6}>
            <Typography variant="h6" gutterBottom>Facebook Messages</Typography>
            <TableContainer>
              <Table size="small">
                <TableBody>
                  {messages.filter(m => m.channelType === 'Facebook').map((message) => (
                    <TableRow key={message.id} hover onClick={() => handleViewMessage(message)}>
                      <TableCell>{message.direction === 'Inbound' ? message.fromAddress : message.toAddress}</TableCell>
                      <TableCell>{message.bodyPreview}</TableCell>
                      <TableCell>
                        <Chip label={message.status} size="small" color={getStatusColor(message.status)} />
                      </TableCell>
                      <TableCell>{new Date(message.createdAt).toLocaleString()}</TableCell>
                    </TableRow>
                  ))}
                  {messages.filter(m => m.channelType === 'Facebook').length === 0 && (
                    <TableRow>
                      <TableCell colSpan={4} align="center">No Facebook messages</TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </TabPanel>

          {/* Channels Tab */}
          <TabPanel value={tabValue} index={7}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="h6">Configured Channels</Typography>
              <Button variant="outlined" startIcon={<AddIcon />}>
                Add Channel
              </Button>
            </Box>
            <Grid container spacing={2}>
              {channels.map((channel) => (
                <Grid item xs={12} sm={6} md={4} key={channel.id}>
                  <Card variant="outlined">
                    <CardContent>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                        <Avatar sx={{ bgcolor: getChannelColor(channel.channelType) + '.main' }}>
                          {getChannelIcon(channel.channelType)}
                        </Avatar>
                        <Box>
                          <Typography variant="h6">{channel.name}</Typography>
                          <Chip 
                            label={channel.status} 
                            size="small" 
                            color={channel.status === 'Connected' ? 'success' : (channel.status === 'Error' ? 'error' : 'default')} 
                          />
                        </Box>
                      </Box>
                      <Typography variant="body2" color="text.secondary">
                        {channel.channelType === 'Email' && channel.fromEmail}
                        {(channel.channelType === 'Twitter' || channel.channelType === 'Facebook') && channel.socialUsername}
                      </Typography>
                      {channel.lastConnectedAt && (
                        <Typography variant="caption" color="text.secondary">
                          Last connected: {new Date(channel.lastConnectedAt).toLocaleString()}
                        </Typography>
                      )}
                      <Box sx={{ mt: 2, display: 'flex', gap: 1 }}>
                        <Button size="small" variant="outlined">Test</Button>
                        <Button size="small" variant="outlined">Edit</Button>
                      </Box>
                    </CardContent>
                  </Card>
                </Grid>
              ))}
              {channels.length === 0 && (
                <Grid item xs={12}>
                  <Alert severity="info">
                    No channels configured yet. Add a channel to start sending and receiving messages.
                  </Alert>
                </Grid>
              )}
            </Grid>
          </TabPanel>
        </CardContent>
      </Card>

      {/* Compose Dialog */}
      <Dialog open={composeOpen} onClose={() => setComposeOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>Compose Message</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Channel</InputLabel>
                <Select
                  value={composeForm.channelId}
                  label="Channel"
                  onChange={(e) => {
                    const channelId = e.target.value as number;
                    const channel = channels.find(c => c.id === channelId);
                    setComposeForm({
                      ...composeForm,
                      channelId,
                      channelType: channel?.channelType || 'Email'
                    });
                  }}
                >
                  {channels.filter(c => c.isEnabled).map((channel) => (
                    <MenuItem key={channel.id} value={channel.id}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        {getChannelIcon(channel.channelType)}
                        {channel.name}
                      </Box>
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="To"
                value={composeForm.toAddress}
                onChange={(e) => setComposeForm({ ...composeForm, toAddress: e.target.value })}
                placeholder={composeForm.channelType === 'Email' ? 'email@example.com' : 
                            composeForm.channelType === 'WhatsApp' ? '+1234567890' : '@username'}
              />
            </Grid>
            {(composeForm.channelType === 'Email') && (
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Subject"
                  value={composeForm.subject}
                  onChange={(e) => setComposeForm({ ...composeForm, subject: e.target.value })}
                />
              </Grid>
            )}
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Message"
                multiline
                rows={6}
                value={composeForm.body}
                onChange={(e) => setComposeForm({ ...composeForm, body: e.target.value })}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setComposeOpen(false)}>Cancel</Button>
          <Button 
            variant="contained" 
            startIcon={<SendIcon />}
            onClick={handleSendMessage}
            disabled={!composeForm.channelId || !composeForm.toAddress || !composeForm.body}
          >
            Send
          </Button>
        </DialogActions>
      </Dialog>

      {/* View Message Dialog */}
      <Dialog open={viewMessageOpen} onClose={() => setViewMessageOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            {selectedMessage && getChannelIcon(selectedMessage.channelType)}
            {messageDetails?.subject || 'Message'}
          </Box>
        </DialogTitle>
        <DialogContent dividers>
          {messageDetails && (
            <Box>
              <Grid container spacing={2} sx={{ mb: 2 }}>
                <Grid item xs={6}>
                  <Typography variant="caption" color="text.secondary">From</Typography>
                  <Typography>{messageDetails.fromName || messageDetails.fromAddress}</Typography>
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="caption" color="text.secondary">To</Typography>
                  <Typography>{messageDetails.toName || messageDetails.toAddress}</Typography>
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="caption" color="text.secondary">Status</Typography>
                  <Chip label={messageDetails.status} size="small" color={getStatusColor(messageDetails.status)} />
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="caption" color="text.secondary">Date</Typography>
                  <Typography>{new Date(messageDetails.sentAt || messageDetails.receivedAt || messageDetails.createdAt).toLocaleString()}</Typography>
                </Grid>
              </Grid>
              <Divider sx={{ my: 2 }} />
              <Typography variant="body1" sx={{ whiteSpace: 'pre-wrap' }}>
                {messageDetails.htmlBody ? (
                  <div dangerouslySetInnerHTML={{ __html: messageDetails.htmlBody }} />
                ) : (
                  messageDetails.body
                )}
              </Typography>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button startIcon={<ReplyIcon />} variant="outlined">Reply</Button>
          <Button onClick={() => setViewMessageOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

export default CommunicationsPage;
