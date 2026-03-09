import { useState, useEffect, useRef, useCallback } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  TextField,
  Button,
  Chip,
  IconButton,
  CircularProgress,
  Alert,
  Avatar,
  Divider,
  Rating,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Tooltip,
  List,
  ListItemButton,
  ListItemText,
  ListItemIcon,
} from '@mui/material';
import {
  SmartToy as SmartToyIcon,
  Send as SendIcon,
  ArrowBack as ArrowBackIcon,
  Add as AddIcon,
  Chat as ChatIcon,
  ChatBubbleOutline as ChatBubbleOutlineIcon,
  Info as InfoIcon,
} from '@mui/icons-material';
import { useParams, useNavigate } from 'react-router-dom';
import apiClient from '../services/apiClient';
import agentService from '../services/agentService';
import {
  Agent,
  AgentConversation,
  ChatMessageRecord,
  ConversationStatus,
  AgentTypeLabels,
  getAgentTypeColor,
} from '../types/agents';

// Suggested prompts shown when no conversation is active
const SUGGESTED_PROMPTS = [
  'What can you help me with?',
  'Show me a summary of recent activity.',
  'Help me draft a follow-up email.',
  'Analyze my pipeline performance.',
];

const formatTimestamp = (dateStr: string): string => {
  const date = new Date(dateStr);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

  if (diffDays === 0) {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }
  if (diffDays === 1) return 'Yesterday';
  if (diffDays < 7) return `${diffDays} days ago`;
  return date.toLocaleDateString([], { month: 'short', day: 'numeric' });
};

const parseMessages = (messagesJson: string): ChatMessageRecord[] => {
  try {
    const parsed = JSON.parse(messagesJson);
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
};

const AgentChatPage = () => {
  const { agentId } = useParams<{ agentId: string }>();
  const navigate = useNavigate();
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  // State
  const [agent, setAgent] = useState<Agent | null>(null);
  const [conversations, setConversations] = useState<AgentConversation[]>([]);
  const [activeConversationId, setActiveConversationId] = useState<number | null>(null);
  const [messages, setMessages] = useState<ChatMessageRecord[]>([]);
  const [inputMessage, setInputMessage] = useState('');
  const [loading, setLoading] = useState(true);
  const [sending, setSending] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showRating, setShowRating] = useState(false);
  const [ratingValue, setRatingValue] = useState<number | null>(null);
  const [feedbackText, setFeedbackText] = useState('');
  const [ratingSubmitting, setRatingSubmitting] = useState(false);

  const numericAgentId = agentId ? Number.parseInt(agentId, 10) : 0;

  // Auto-scroll to bottom on new messages
  const scrollToBottom = useCallback(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, []);

  useEffect(() => {
    scrollToBottom();
  }, [messages, sending, scrollToBottom]);

  // Load agent info and conversations on mount
  useEffect(() => {
    if (!numericAgentId) return;

    const fetchData = async () => {
      try {
        setLoading(true);
        setError(null);
        const [agentRes, convRes] = await Promise.all([
          agentService.getById(numericAgentId),
          agentService.getConversations(numericAgentId),
        ]);
        setAgent(agentRes.data);
        setConversations(convRes.data || []);
      } catch (err: unknown) {
        console.error('Failed to load agent data:', err);
        setError((err as any)?.response?.data?.message || 'Failed to load agent. Please try again.');
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [numericAgentId]);

  // Load messages when a conversation is selected
  const handleSelectConversation = useCallback(
    async (conversationId: number) => {
      setActiveConversationId(conversationId);
      const conv = conversations.find((c) => c.id === conversationId);
      if (conv) {
        setMessages(parseMessages(conv.messages));
      } else {
        // Fetch full conversation
        try {
          const res = await agentService.getConversation(conversationId);
          setMessages(parseMessages(res.data.messages));
        } catch (err: unknown) {
          console.error('Failed to load conversation:', err);
          setError('Failed to load conversation messages.');
        }
      }
    },
    [conversations]
  );

  // Start a new conversation
  const handleNewConversation = () => {
    setActiveConversationId(null);
    setMessages([]);
    setInputMessage('');
  };

  // Send message
  const handleSendMessage = async (messageText?: string) => {
    const text = (messageText || inputMessage).trim();
    if (!text || sending || !numericAgentId) return;

    // Optimistically add user message
    const userMsg: ChatMessageRecord = { role: 'user', content: text };
    setMessages((prev) => [...prev, userMsg]);
    setInputMessage('');
    setSending(true);
    setError(null);

    try {
      const response = await agentService.chat(numericAgentId, {
        message: text,
        conversationId: activeConversationId || undefined,
      });

      const chatRes = response.data;

      // Update active conversation ID if new
      if (!activeConversationId && chatRes.conversationId) {
        setActiveConversationId(chatRes.conversationId);
      }

      // Use history from response if available, otherwise append assistant message
      if (chatRes.history && chatRes.history.length > 0) {
        setMessages(chatRes.history);
      } else {
        const assistantMsg: ChatMessageRecord = { role: 'assistant', content: chatRes.response };
        setMessages((prev) => [...prev, assistantMsg]);
      }

      // Refresh conversation list
      try {
        const convRes = await agentService.getConversations(numericAgentId);
        setConversations(convRes.data || []);
      } catch {
        // Non-critical, ignore
      }
    } catch (err: unknown) {
      console.error('Failed to send message:', err);
      setError((err as any)?.response?.data?.message || 'Failed to send message. Please try again.');
      // Remove optimistic user message on error
      setMessages((prev) => prev.filter((m) => m !== userMsg));
    } finally {
      setSending(false);
      inputRef.current?.focus();
    }
  };

  // Handle Enter key
  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSendMessage();
    }
  };

  // Submit rating
  const handleSubmitRating = async () => {
    if (!activeConversationId || !ratingValue) return;
    try {
      setRatingSubmitting(true);
      await agentService.rateConversation(activeConversationId, {
        rating: ratingValue,
        feedback: feedbackText || undefined,
      });
      setShowRating(false);
      setRatingValue(null);
      setFeedbackText('');
      // Refresh conversations
      const convRes = await agentService.getConversations(numericAgentId);
      setConversations(convRes.data || []);
    } catch (err: unknown) {
      console.error('Failed to submit rating:', err);
    } finally {
      setRatingSubmitting(false);
    }
  };

  // Show completed-conversation rating prompt
  const activeConv = conversations.find((c) => c.id === activeConversationId);
  const isConversationCompleted = activeConv?.status === ConversationStatus.Completed;
  const hasRated = activeConv?.userRating != null && activeConv.userRating > 0;

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!agent) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="error" sx={{ mb: 2 }}>
          Agent not found. It may have been removed or deactivated.
        </Alert>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/agents')}>
          Back to Agents
        </Button>
      </Box>
    );
  }

  const typeColor = getAgentTypeColor(agent.agentType);
  const typeLabel = AgentTypeLabels[agent.agentType] || 'Agent';

  return (
    <Box sx={{ display: 'flex', height: 'calc(100vh - 64px)', overflow: 'hidden' }}>
      {/* Left Sidebar — Conversation History */}
      <Box
        sx={{
          width: 280,
          borderRight: '1px solid #E7E0EC',
          display: { xs: 'none', md: 'flex' },
          flexDirection: 'column',
          backgroundColor: '#FAFAFA',
        }}
      >
        {/* Sidebar Header */}
        <Box sx={{ p: 2, borderBottom: '1px solid #E7E0EC' }}>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={handleNewConversation}
            fullWidth
            size="small"
            sx={{
              textTransform: 'none',
              borderRadius: 2,
              backgroundColor: '#6750A4',
              '&:hover': { backgroundColor: '#57439B' },
            }}
          >
            New Conversation
          </Button>
        </Box>

        {/* Conversation List */}
        <Box sx={{ flex: 1, overflow: 'auto' }}>
          {conversations.length === 0 ? (
            <Box sx={{ p: 2, textAlign: 'center' }}>
              <ChatBubbleOutlineIcon sx={{ fontSize: 32, color: '#CAC4D0', mb: 1 }} />
              <Typography variant="body2" sx={{ color: '#79747E' }}>
                No conversations yet
              </Typography>
            </Box>
          ) : (
            <List disablePadding>
              {conversations.map((conv) => {
                const isActive = conv.id === activeConversationId;
                const preview = (() => {
                  const msgs = parseMessages(conv.messages);
                  const lastUserMsg = [...msgs].reverse().find((m) => m.role === 'user');
                  return lastUserMsg?.content?.substring(0, 60) || 'Empty conversation';
                })();

                return (
                  <ListItemButton
                    key={conv.id}
                    selected={isActive}
                    onClick={() => handleSelectConversation(conv.id)}
                    sx={{
                      borderBottom: '1px solid #F0ECF4',
                      py: 1.5,
                      px: 2,
                      '&.Mui-selected': {
                        backgroundColor: '#F3EDF7',
                        '&:hover': { backgroundColor: '#EDE8F2' },
                      },
                    }}
                  >
                    <ListItemIcon sx={{ minWidth: 32 }}>
                      <ChatIcon sx={{ fontSize: 18, color: isActive ? '#6750A4' : '#79747E' }} />
                    </ListItemIcon>
                    <ListItemText
                      primary={
                        <Typography
                          variant="body2"
                          noWrap
                          sx={{
                            fontWeight: isActive ? 600 : 400,
                            color: isActive ? '#1C1B1F' : '#49454F',
                            fontSize: 13,
                          }}
                        >
                          {preview}
                        </Typography>
                      }
                      secondary={
                        <Typography variant="caption" sx={{ color: '#79747E' }}>
                          {formatTimestamp(conv.createdAt)} · {conv.messageCount} msg{conv.messageCount !== 1 ? 's' : ''}
                        </Typography>
                      }
                    />
                  </ListItemButton>
                );
              })}
            </List>
          )}
        </Box>
      </Box>

      {/* Main Chat Area */}
      <Box sx={{ flex: 1, display: 'flex', flexDirection: 'column', minWidth: 0 }}>
        {/* Chat Header */}
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            px: 2,
            py: 1.5,
            borderBottom: '1px solid #E7E0EC',
            backgroundColor: '#fff',
          }}
        >
          <IconButton onClick={() => navigate('/agents')} sx={{ mr: 1 }}>
            <ArrowBackIcon />
          </IconButton>
          <Avatar
            sx={{
              bgcolor: typeColor,
              width: 36,
              height: 36,
              mr: 1.5,
              fontSize: 16,
              fontWeight: 600,
            }}
          >
            {(agent.displayName ?? agent.name ?? '?').charAt(0).toUpperCase()}
          </Avatar>
          <Box sx={{ flex: 1, minWidth: 0 }}>
            <Typography variant="subtitle1" sx={{ fontWeight: 600, lineHeight: 1.2 }} noWrap>
              {agent.displayName ?? agent.name}
            </Typography>
            <Chip
              label={typeLabel}
              size="small"
              sx={{
                height: 18,
                fontSize: 10,
                fontWeight: 500,
                backgroundColor: `${typeColor}18`,
                color: typeColor,
                '& .MuiChip-label': { px: 0.75 },
              }}
            />
          </Box>
          {isConversationCompleted && !hasRated && (
            <Button
              size="small"
              variant="outlined"
              onClick={() => setShowRating(true)}
              sx={{ textTransform: 'none', borderRadius: 2 }}
            >
              Rate
            </Button>
          )}
        </Box>

        {error && (
          <Alert severity="error" sx={{ m: 2, mb: 0 }} onClose={() => setError(null)}>
            {error}
          </Alert>
        )}

        {/* Messages Area */}
        <Box
          sx={{
            flex: 1,
            overflow: 'auto',
            px: 2,
            py: 2,
            display: 'flex',
            flexDirection: 'column',
            gap: 1.5,
            backgroundColor: '#FEFBFF',
          }}
        >
          {messages.length === 0 && !activeConversationId ? (
            /* Welcome / Empty State */
            <Box
              sx={{
                flex: 1,
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                textAlign: 'center',
                px: 2,
              }}
            >
              <Avatar
                sx={{
                  bgcolor: typeColor,
                  width: 64,
                  height: 64,
                  mb: 2,
                  fontSize: 28,
                  fontWeight: 600,
                }}
              >
                {(agent.displayName ?? agent.name ?? '?').charAt(0).toUpperCase()}
              </Avatar>
              <Typography variant="h6" sx={{ fontWeight: 600, color: '#1C1B1F', mb: 0.5 }}>
                {agent.displayName ?? agent.name}
              </Typography>
              <Typography variant="body2" sx={{ color: '#49454F', mb: 3, maxWidth: 480 }}>
                {agent.description || 'Start a conversation to get help from this AI assistant.'}
              </Typography>

              <Typography variant="caption" sx={{ color: '#79747E', mb: 1.5, fontWeight: 500 }}>
                Try asking:
              </Typography>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1, maxWidth: 360, width: '100%' }}>
                {SUGGESTED_PROMPTS.map((prompt) => (
                  <Button
                    key={prompt}
                    variant="outlined"
                    size="small"
                    onClick={() => handleSendMessage(prompt)}
                    sx={{
                      textTransform: 'none',
                      borderRadius: 2,
                      borderColor: '#E7E0EC',
                      color: '#49454F',
                      justifyContent: 'flex-start',
                      px: 2,
                      '&:hover': { borderColor: '#6750A4', color: '#6750A4', backgroundColor: '#F9F5FF' },
                    }}
                  >
                    {prompt}
                  </Button>
                ))}
              </Box>
            </Box>
          ) : (
            /* Message Bubbles */
            <>
              {messages.map((msg, idx) => {
                if (msg.role === 'system' || msg.role === 'tool') {
                  return (
                    <Box key={idx} sx={{ display: 'flex', justifyContent: 'center', my: 0.5 }}>
                      <Chip
                        icon={<InfoIcon sx={{ fontSize: 14 }} />}
                        label={msg.content.length > 120 ? msg.content.substring(0, 120) + '…' : msg.content}
                        size="small"
                        variant="outlined"
                        sx={{
                          maxWidth: '80%',
                          height: 'auto',
                          '& .MuiChip-label': {
                            whiteSpace: 'normal',
                            py: 0.5,
                            fontSize: 11,
                            color: '#79747E',
                          },
                          borderColor: '#E7E0EC',
                        }}
                      />
                    </Box>
                  );
                }

                const isUser = msg.role === 'user';

                return (
                  <Box
                    key={idx}
                    sx={{
                      display: 'flex',
                      justifyContent: isUser ? 'flex-end' : 'flex-start',
                    }}
                  >
                    {!isUser && (
                      <Avatar
                        sx={{
                          bgcolor: typeColor,
                          width: 28,
                          height: 28,
                          mr: 1,
                          mt: 0.5,
                          fontSize: 12,
                          fontWeight: 600,
                        }}
                      >
                        {(agent.displayName ?? agent.name ?? '?').charAt(0).toUpperCase()}
                      </Avatar>
                    )}
                    <Box
                      sx={{
                        maxWidth: '70%',
                        px: 2,
                        py: 1.25,
                        borderRadius: isUser ? '16px 16px 4px 16px' : '16px 16px 16px 4px',
                        backgroundColor: isUser ? '#E3F2FD' : '#F5F5F5',
                        ...(isUser ? { ml: 'auto' } : { mr: 'auto' }),
                      }}
                    >
                      <Typography
                        variant="body2"
                        sx={{
                          color: '#1C1B1F',
                          whiteSpace: 'pre-wrap',
                          wordBreak: 'break-word',
                          lineHeight: 1.55,
                          fontSize: 14,
                        }}
                      >
                        {msg.content}
                      </Typography>
                    </Box>
                  </Box>
                );
              })}

              {/* Thinking indicator */}
              {sending && (
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <Avatar
                    sx={{
                      bgcolor: typeColor,
                      width: 28,
                      height: 28,
                      fontSize: 12,
                      fontWeight: 600,
                    }}
                  >
                    {(agent.displayName ?? agent.name ?? '?').charAt(0).toUpperCase()}
                  </Avatar>
                  <Box
                    sx={{
                      px: 2,
                      py: 1.25,
                      borderRadius: '16px 16px 16px 4px',
                      backgroundColor: '#F5F5F5',
                      display: 'flex',
                      alignItems: 'center',
                      gap: 1,
                    }}
                  >
                    <CircularProgress size={14} sx={{ color: '#79747E' }} />
                    <Typography variant="body2" sx={{ color: '#79747E', fontStyle: 'italic', fontSize: 13 }}>
                      Agent is thinking...
                    </Typography>
                  </Box>
                </Box>
              )}

              <div ref={messagesEndRef} />
            </>
          )}
        </Box>

        {/* Input Area */}
        <Box
          sx={{
            px: 2,
            py: 1.5,
            borderTop: '1px solid #E7E0EC',
            backgroundColor: '#fff',
            display: 'flex',
            alignItems: 'flex-end',
            gap: 1,
          }}
        >
          <TextField
            inputRef={inputRef}
            fullWidth
            multiline
            maxRows={4}
            placeholder={sending ? 'Waiting for response...' : 'Type a message...'}
            value={inputMessage}
            onChange={(e) => setInputMessage(e.target.value)}
            onKeyDown={handleKeyDown}
            disabled={sending}
            size="small"
            sx={{
              '& .MuiOutlinedInput-root': {
                borderRadius: 3,
                backgroundColor: '#F9F5FF',
              },
            }}
          />
          <IconButton
            onClick={() => handleSendMessage()}
            disabled={!inputMessage.trim() || sending}
            sx={{
              backgroundColor: '#6750A4',
              color: '#fff',
              '&:hover': { backgroundColor: '#57439B' },
              '&.Mui-disabled': { backgroundColor: '#E7E0EC', color: '#CAC4D0' },
              width: 40,
              height: 40,
            }}
          >
            <SendIcon sx={{ fontSize: 20 }} />
          </IconButton>
        </Box>
      </Box>

      {/* Rating Dialog */}
      <Dialog open={showRating} onClose={() => setShowRating(false)} maxWidth="xs" fullWidth>
        <DialogTitle sx={{ fontWeight: 600 }}>Rate this conversation</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2, pt: 1 }}>
            <Rating
              value={ratingValue}
              onChange={(_, newValue) => setRatingValue(newValue)}
              size="large"
            />
            <TextField
              fullWidth
              multiline
              rows={3}
              placeholder="Any feedback? (optional)"
              value={feedbackText}
              onChange={(e) => setFeedbackText(e.target.value)}
              size="small"
            />
          </Box>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setShowRating(false)} sx={{ textTransform: 'none' }}>
            Cancel
          </Button>
          <Button
            variant="contained"
            onClick={handleSubmitRating}
            disabled={!ratingValue || ratingSubmitting}
            sx={{
              textTransform: 'none',
              backgroundColor: '#6750A4',
              '&:hover': { backgroundColor: '#57439B' },
            }}
          >
            {ratingSubmitting ? <CircularProgress size={20} sx={{ color: '#fff' }} /> : 'Submit'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default AgentChatPage;
