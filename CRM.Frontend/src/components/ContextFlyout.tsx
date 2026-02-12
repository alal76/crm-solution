/**
 * Context Flyout Panel
 * 
 * A right-side panel that provides:
 * 1. Account selector for contextual filtering (top section)
 * 2. AI Chatbot assistant with CRM documentation context (bottom section)
 */
import React, { useState, useEffect, useRef, useCallback } from 'react';
import {
  Box,
  Drawer,
  IconButton,
  Typography,
  TextField,
  Chip,
  Autocomplete,
  Divider,
  Paper,
  Avatar,
  CircularProgress,
  Tooltip,
  Badge,
  Fab,
  InputAdornment,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  Alert,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
} from '@mui/material';
import {
  Close as CloseIcon,
  Search as SearchIcon,
  Clear as ClearIcon,
  SmartToy as BotIcon,
  Person as PersonIcon,
  Send as SendIcon,
  Business as BusinessIcon,
  FilterAlt as FilterIcon,
  Chat as ChatIcon,
  AccountCircle as AccountIcon,
  AutoAwesome as AIIcon,
  Psychology as AgentIcon,
} from '@mui/icons-material';
import { useAccountContext } from '../contexts/AccountContextProvider';
import { useAuth } from '../contexts/AuthContext';
import { Account } from '../services/accountService';
import apiClient from '../services/apiClient';
import agentService from '../services/agentService';
import { Agent, ChatRequest } from '../types/agents';

interface ChatMessage {
  id: string;
  role: 'user' | 'assistant' | 'system';
  content: string;
  timestamp: Date;
  isLoading?: boolean;
}

interface ContextFlyoutProps {
  onAccountsChange?: (accounts: Account[]) => void;
}

const FLYOUT_WIDTH = 400;

const ContextFlyout: React.FC<ContextFlyoutProps> = ({ onAccountsChange }) => {
  const { isAuthenticated } = useAuth();
  const {
    selectedAccounts,
    addAccount,
    removeAccount,
    clearAccounts,
    isFlyoutOpen,
    toggleFlyout,
    setFlyoutOpen,
    isContextActive,
  } = useAccountContext();

  // Account search state
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<Account[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [accountsLoaded, setAccountsLoaded] = useState<Account[]>([]);

  // Chat state
  const [messages, setMessages] = useState<ChatMessage[]>([
    {
      id: 'welcome',
      role: 'assistant',
      content: "Hello! I'm your CRM Assistant. I've been trained on the CRM documentation and can help you with:\n\n• Understanding features and workflows\n• Finding information about accounts and contacts\n• Navigating the system\n• Best practices for CRM usage\n\nHow can I help you today?",
      timestamp: new Date(),
    },
  ]);
  const [inputMessage, setInputMessage] = useState('');
  const [isSending, setIsSending] = useState(false);
  const [docsLoaded, setDocsLoaded] = useState(false);
  const [docsLoading, setDocsLoading] = useState(false);

  // Agent selector state
  const [agents, setAgents] = useState<Agent[]>([]);
  const [selectedAgentId, setSelectedAgentId] = useState<number | ''>('');
  const [agentConversationId, setAgentConversationId] = useState<number | null>(null);
  
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const chatInputRef = useRef<HTMLInputElement>(null);

  // Load all accounts on mount for autocomplete
  useEffect(() => {
    if (!isAuthenticated) return;
    const loadAccounts = async () => {
      try {
        const response = await apiClient.get<Account[]>('/accounts?limit=500');
        setAccountsLoaded(response.data || []);
      } catch (error) {
        console.error('Failed to load accounts:', error);
      }
    };
    loadAccounts();
  }, [isAuthenticated]);

  // Load CRM documentation on mount for chatbot context
  useEffect(() => {
    if (!isAuthenticated) return;
    const loadDocumentation = async () => {
      if (docsLoaded || docsLoading) return;
      
      setDocsLoading(true);
      try {
        // The chatbot backend will load documentation - just notify it to initialize
        await apiClient.post('/ai/chatbot/initialize', {});
        setDocsLoaded(true);
      } catch (error) {
        console.error('Failed to initialize chatbot context:', error);
        // Don't block - chatbot will work without pre-loaded docs
        setDocsLoaded(true);
      } finally {
        setDocsLoading(false);
      }
    };
    
    if (isFlyoutOpen) {
      loadDocumentation();
    }
  }, [isFlyoutOpen, docsLoaded, docsLoading, isAuthenticated]);

  // Scroll to bottom when new messages arrive

  // Load available AI agents
  useEffect(() => {
    if (!isAuthenticated) return;
    const loadAgents = async () => {
      try {
        const response = await agentService.getAll();
        const activeAgents = (response.data || []).filter((a: Agent) => a.isActive);
        setAgents(activeAgents);
      } catch (error) {
        console.error('Failed to load agents:', error);
      }
    };
    loadAgents();
  }, [isAuthenticated]);
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  // Notify parent when accounts change
  useEffect(() => {
    onAccountsChange?.(selectedAccounts);
  }, [selectedAccounts, onAccountsChange]);

  // Search accounts as user types
  useEffect(() => {
    if (!searchQuery.trim()) {
      setSearchResults([]);
      return;
    }

    const searchTerm = searchQuery.toLowerCase();
    const filtered = accountsLoaded.filter(account => {
      const name = `${account.firstName || ''} ${account.lastName || ''} ${account.company || ''}`.toLowerCase();
      const email = (account.email || '').toLowerCase();
      return name.includes(searchTerm) || email.includes(searchTerm);
    });
    setSearchResults(filtered.slice(0, 10));
  }, [searchQuery, accountsLoaded]);

  const handleAccountSelect = (account: Account | null) => {
    if (account) {
      addAccount(account);
      setSearchQuery('');
    }
  };

  const handleRemoveAccount = (accountId: number) => {
    removeAccount(accountId);
  };

  const handleSendMessage = async () => {
    if (!inputMessage.trim() || isSending) return;

    const userMessage: ChatMessage = {
      id: `user-${Date.now()}`,
      role: 'user',
      content: inputMessage.trim(),
      timestamp: new Date(),
    };

    const loadingMessage: ChatMessage = {
      id: `loading-${Date.now()}`,
      role: 'assistant',
      content: '',
      timestamp: new Date(),
      isLoading: true,
    };

    setMessages(prev => [...prev, userMessage, loadingMessage]);
    const sentMessage = inputMessage.trim();
    setInputMessage('');
    setIsSending(true);

    try {
      let responseText = '';

      if (selectedAgentId) {
        // Route through Semantic Kernel agent
        const chatRequest: ChatRequest = {
          message: sentMessage,
          conversationId: agentConversationId ?? undefined,
          entityType: selectedAccounts.length > 0 ? 'account' : undefined,
          entityId: selectedAccounts.length > 0 ? selectedAccounts[0].id : undefined,
        };
        const agentResponse = await agentService.chat(selectedAgentId as number, chatRequest);
        responseText = agentResponse.data?.response || "I couldn't process that request.";
        if (agentResponse.data?.conversationId) {
          setAgentConversationId(agentResponse.data.conversationId);
        }
      } else {
        // Default CRM Assistant via /ai/chatbot/message
        const accountContext = selectedAccounts.length > 0
          ? `Current context: Working with ${selectedAccounts.length} account(s): ${selectedAccounts.map(a => a.company || `${a.firstName} ${a.lastName}`).join(', ')}`
          : '';

        const conversationHistory = messages
          .filter(m => !m.isLoading && m.id !== 'welcome' && (m.role === 'user' || m.role === 'assistant'))
          .map(m => ({ role: m.role, content: m.content }));

        const response = await apiClient.post<{ response: string }>('/ai/chatbot/message', {
          message: sentMessage,
          accountContext,
          accountIds: selectedAccounts.map(a => a.id),
          conversationHistory,
        });
        responseText = response.data?.response || "I couldn't process that request.";
      }

      const assistantMessage: ChatMessage = {
        id: `assistant-${Date.now()}`,
        role: 'assistant',
        content: responseText,
        timestamp: new Date(),
      };

      setMessages(prev => prev.filter(m => !m.isLoading).concat(assistantMessage));
    } catch (error) {
      console.error('Failed to send message:', error);
      
      const errorMessage: ChatMessage = {
        id: `error-${Date.now()}`,
        role: 'assistant',
        content: "I apologize, but I'm having trouble connecting to the AI service. Please check your LLM settings or try again later.",
        timestamp: new Date(),
      };
      
      setMessages(prev => prev.filter(m => !m.isLoading).concat(errorMessage));
    } finally {
      setIsSending(false);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSendMessage();
    }
  };

  const handleAgentChange = (agentId: number | '') => {
    setSelectedAgentId(agentId);
    setAgentConversationId(null);
    const agent = agents.find(a => a.id === agentId);
    const welcomeContent = agent
      ? `Hi! I'm **${agent.displayName}**. ${agent.description || 'How can I help you?'}`
      : "Hello! I'm your CRM Assistant. I've been trained on the CRM documentation and can help you with:\n\n• Understanding features and workflows\n• Finding information about accounts and contacts\n• Navigating the system\n• Best practices for CRM usage\n\nHow can I help you today?";
    setMessages([{
      id: 'welcome',
      role: 'assistant',
      content: welcomeContent,
      timestamp: new Date(),
    }]);
  };

  const getAccountDisplayName = (account: Account): string => {
    if (account.company) return account.company;
    return `${account.firstName || ''} ${account.lastName || ''}`.trim() || `Account #${account.id}`;
  };

  // Don't render if user is not authenticated
  if (!isAuthenticated) {
    return null;
  }

  return (
    <>
      {/* Toggle Button */}
      <Tooltip title={isFlyoutOpen ? "Close Context Panel" : "Open Context Panel"}>
        <Fab
          color={isContextActive ? "primary" : "default"}
          size="medium"
          onClick={toggleFlyout}
          sx={{
            position: 'fixed',
            right: isFlyoutOpen ? FLYOUT_WIDTH + 16 : 16,
            bottom: 80,
            zIndex: 1200,
            transition: 'right 0.3s ease-in-out',
          }}
        >
          <Badge badgeContent={selectedAccounts.length} color="error">
            {isFlyoutOpen ? <CloseIcon /> : <ChatIcon />}
          </Badge>
        </Fab>
      </Tooltip>

      {/* Flyout Drawer */}
      <Drawer
        anchor="right"
        open={isFlyoutOpen}
        onClose={() => setFlyoutOpen(false)}
        variant="persistent"
        sx={{
          width: FLYOUT_WIDTH,
          flexShrink: 0,
          '& .MuiDrawer-paper': {
            width: FLYOUT_WIDTH,
            boxSizing: 'border-box',
            top: 64, // Below navbar
            height: 'calc(100% - 64px)',
            borderLeft: '1px solid',
            borderColor: 'divider',
          },
        }}
      >
        <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
          {/* Header */}
          <Box sx={{ p: 2, borderBottom: 1, borderColor: 'divider', bgcolor: 'primary.main', color: 'white' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <FilterIcon />
                <Typography variant="h6">Context Panel</Typography>
              </Box>
              <IconButton size="small" onClick={() => setFlyoutOpen(false)} sx={{ color: 'white' }}>
                <CloseIcon />
              </IconButton>
            </Box>
          </Box>

          {/* Account Selector Section */}
          <Box sx={{ p: 2, borderBottom: 1, borderColor: 'divider', bgcolor: 'grey.50' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <BusinessIcon color="primary" />
              <Typography variant="subtitle1" fontWeight="bold">
                Account Context
              </Typography>
              {selectedAccounts.length > 0 && (
                <Chip
                  size="small"
                  label={`${selectedAccounts.length} selected`}
                  color="primary"
                  onDelete={clearAccounts}
                />
              )}
            </Box>

            {/* Account Search */}
            <Autocomplete
              freeSolo
              options={searchResults}
              getOptionLabel={(option) => 
                typeof option === 'string' ? option : getAccountDisplayName(option)
              }
              inputValue={searchQuery}
              onInputChange={(_, value) => setSearchQuery(value)}
              onChange={(_, value) => handleAccountSelect(value as Account)}
              loading={isSearching}
              renderInput={(params) => (
                <TextField
                  {...params}
                  size="small"
                  placeholder="Search accounts..."
                  InputProps={{
                    ...params.InputProps,
                    startAdornment: (
                      <InputAdornment position="start">
                        <SearchIcon color="action" />
                      </InputAdornment>
                    ),
                  }}
                />
              )}
              renderOption={(props, option) => (
                <li {...props} key={option.id}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Avatar sx={{ width: 32, height: 32, bgcolor: 'primary.light' }}>
                      {option.company ? <BusinessIcon /> : <PersonIcon />}
                    </Avatar>
                    <Box>
                      <Typography variant="body2">{getAccountDisplayName(option)}</Typography>
                      <Typography variant="caption" color="text.secondary">
                        {option.email || 'No email'}
                      </Typography>
                    </Box>
                  </Box>
                </li>
              )}
            />

            {/* Selected Accounts */}
            {selectedAccounts.length > 0 && (
              <Box sx={{ mt: 2, display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                {selectedAccounts.map(account => (
                  <Chip
                    key={account.id}
                    label={getAccountDisplayName(account)}
                    size="small"
                    onDelete={() => handleRemoveAccount(account.id!)}
                    avatar={
                      <Avatar sx={{ width: 24, height: 24 }}>
                        {account.company ? <BusinessIcon sx={{ fontSize: 14 }} /> : <PersonIcon sx={{ fontSize: 14 }} />}
                      </Avatar>
                    }
                    sx={{ maxWidth: '100%' }}
                  />
                ))}
              </Box>
            )}

            {selectedAccounts.length === 0 && (
              <Alert severity="info" sx={{ mt: 2 }}>
                Select accounts to filter data across pages
              </Alert>
            )}
          </Box>

          {/* Agent Selector */}
          {agents.length > 0 && (
            <Box sx={{ px: 2, py: 1, borderBottom: 1, borderColor: 'divider' }}>
              <FormControl fullWidth size="small">
                <InputLabel id="agent-selector-label">
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                    <AgentIcon sx={{ fontSize: 16 }} />
                    AI Agent
                  </Box>
                </InputLabel>
                <Select
                  labelId="agent-selector-label"
                  value={selectedAgentId}
                  onChange={(e) => handleAgentChange(e.target.value as number | '')}
                  label="AI Agent"
                >
                  <MenuItem value="">
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <AIIcon sx={{ fontSize: 18, color: 'secondary.main' }} />
                      <Typography variant="body2">CRM Assistant (Default)</Typography>
                    </Box>
                  </MenuItem>
                  <Divider />
                  {agents.map((agent) => (
                    <MenuItem key={agent.id} value={agent.id}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <AgentIcon sx={{ fontSize: 18, color: 'primary.main' }} />
                        <Box>
                          <Typography variant="body2">{agent.displayName || agent.name}</Typography>
                          {agent.description && (
                            <Typography variant="caption" color="text.secondary" sx={{ display: 'block', maxWidth: 280, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                              {agent.description}
                            </Typography>
                          )}
                        </Box>
                      </Box>
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Box>
          )}

          {/* Chatbot Section */}
          <Box sx={{ flex: 1, display: 'flex', flexDirection: 'column', minHeight: 0 }}>
            {/* Chat Header */}
            <Box sx={{ p: 1.5, borderBottom: 1, borderColor: 'divider', display: 'flex', alignItems: 'center', gap: 1 }}>
              {selectedAgentId ? <AgentIcon color="primary" /> : <AIIcon color="secondary" />}
              <Typography variant="subtitle1" fontWeight="bold">
                {selectedAgentId
                  ? (agents.find(a => a.id === selectedAgentId)?.displayName || 'AI Agent')
                  : 'CRM Assistant'}
              </Typography>
              {docsLoading && (
                <Chip
                  size="small"
                  icon={<CircularProgress size={12} />}
                  label="Loading docs..."
                  variant="outlined"
                />
              )}
            </Box>

            {/* Messages */}
            <Box
              sx={{
                flex: 1,
                overflow: 'auto',
                p: 2,
                display: 'flex',
                flexDirection: 'column',
                gap: 2,
              }}
            >
              {messages.map(message => (
                <Box
                  key={message.id}
                  sx={{
                    display: 'flex',
                    justifyContent: message.role === 'user' ? 'flex-end' : 'flex-start',
                  }}
                >
                  <Paper
                    elevation={1}
                    sx={{
                      p: 1.5,
                      maxWidth: '85%',
                      bgcolor: message.role === 'user' ? 'primary.main' : 'grey.100',
                      color: message.role === 'user' ? 'white' : 'text.primary',
                      borderRadius: 2,
                      borderTopRightRadius: message.role === 'user' ? 0 : 2,
                      borderTopLeftRadius: message.role === 'user' ? 2 : 0,
                    }}
                  >
                    {message.isLoading ? (
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <CircularProgress size={16} />
                        <Typography variant="body2">Thinking...</Typography>
                      </Box>
                    ) : (
                      <Typography
                        variant="body2"
                        sx={{ whiteSpace: 'pre-wrap', wordBreak: 'break-word' }}
                      >
                        {message.content}
                      </Typography>
                    )}
                    <Typography
                      variant="caption"
                      sx={{
                        display: 'block',
                        mt: 0.5,
                        opacity: 0.7,
                        textAlign: message.role === 'user' ? 'right' : 'left',
                      }}
                    >
                      {message.timestamp.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                    </Typography>
                  </Paper>
                </Box>
              ))}
              <div ref={messagesEndRef} />
            </Box>

            {/* Input */}
            <Box sx={{ p: 2, borderTop: 1, borderColor: 'divider', bgcolor: 'background.paper' }}>
              <TextField
                fullWidth
                size="small"
                placeholder="Ask me anything about the CRM..."
                value={inputMessage}
                onChange={e => setInputMessage(e.target.value)}
                onKeyPress={handleKeyPress}
                inputRef={chatInputRef}
                disabled={isSending}
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={handleSendMessage}
                        disabled={!inputMessage.trim() || isSending}
                        color="primary"
                        size="small"
                      >
                        {isSending ? <CircularProgress size={20} /> : <SendIcon />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />
            </Box>
          </Box>
        </Box>
      </Drawer>
    </>
  );
};

export default ContextFlyout;
