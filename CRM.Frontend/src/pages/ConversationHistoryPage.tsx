import { useState, useEffect, useMemo, useCallback } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  TextField,
  Button,
  Chip,
  CircularProgress,
  Alert,
  Avatar,
  InputAdornment,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Rating,
  Tooltip,
  IconButton,
} from '@mui/material';
import {
  History as HistoryIcon,
  Search as SearchIcon,
  Chat as ChatIcon,
  SmartToy as SmartToyIcon,
  AccessTime as AccessTimeIcon,
  CheckCircle as CheckCircleIcon,
  Cancel as CancelIcon,
  HourglassEmpty as HourglassEmptyIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import agentService from '../services/agentService';
import {
  Agent,
  AgentConversation,
  ConversationStatus,
  AgentTypeLabels,
  getAgentTypeColor,
} from '../types/agents';

// ─── Helpers ────────────────────────────────────────────────────────────────

const formatTimestamp = (dateStr: string): string => {
  const date = new Date(dateStr);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMin = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMs / 3600000);
  const diffDays = Math.floor(diffMs / 86400000);

  if (diffMin < 1) return 'Just now';
  if (diffMin < 60) return `${diffMin}m ago`;
  if (diffHours < 24) return `${diffHours}h ago`;
  if (diffDays === 1) return 'Yesterday';
  if (diffDays < 7) return `${diffDays} days ago`;
  return date.toLocaleDateString([], { month: 'short', day: 'numeric', year: 'numeric' });
};

const statusConfig: Record<ConversationStatus, { label: string; color: string; icon: React.ReactNode }> = {
  [ConversationStatus.Active]: { label: 'Active', color: '#2e7d32', icon: <ChatIcon sx={{ fontSize: 14 }} /> },
  [ConversationStatus.Completed]: { label: 'Completed', color: '#1565c0', icon: <CheckCircleIcon sx={{ fontSize: 14 }} /> },
  [ConversationStatus.Cancelled]: { label: 'Cancelled', color: '#757575', icon: <CancelIcon sx={{ fontSize: 14 }} /> },
  [ConversationStatus.Failed]: { label: 'Failed', color: '#c62828', icon: <CancelIcon sx={{ fontSize: 14 }} /> },
  [ConversationStatus.WaitingForApproval]: { label: 'Awaiting Approval', color: '#e65100', icon: <HourglassEmptyIcon sx={{ fontSize: 14 }} /> },
};

// ─── Component ──────────────────────────────────────────────────────────────

const ConversationHistoryPage = () => {
  const navigate = useNavigate();

  // State
  const [agents, setAgents] = useState<Agent[]>([]);
  const [conversations, setConversations] = useState<AgentConversation[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<ConversationStatus | -1>(-1);
  const [agentFilter, setAgentFilter] = useState<number | -1>(-1);

  // Build agent lookup map
  const agentMap = useMemo(() => {
    const map: Record<number, Agent> = {};
    agents.forEach((a) => { map[a.id] = a; });
    return map;
  }, [agents]);

  // Load data
  const loadData = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      // Fetch agents first
      const agentRes = await agentService.getAll();
      const agentList: Agent[] = agentRes.data ?? [];
      setAgents(agentList);

      // Fetch conversations from all agents in parallel
      const allConvos: AgentConversation[] = [];
      const promises = agentList.map(async (agent) => {
        try {
          const res = await agentService.getConversations(agent.id, 50);
          return (res.data ?? []) as AgentConversation[];
        } catch {
          return [] as AgentConversation[];
        }
      });
      const results = await Promise.all(promises);
      results.forEach((convos) => allConvos.push(...convos));

      // Sort by most recent first
      allConvos.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
      setConversations(allConvos);
    } catch (err: unknown) {
      console.error('Failed to load conversations:', err);
      setError((err as any)?.response?.data?.message || 'Failed to load conversation history.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  // Client-side filtering
  const filtered = useMemo(() => {
    let result = conversations;

    if (statusFilter !== -1) {
      result = result.filter((c) => c.status === statusFilter);
    }

    if (agentFilter !== -1) {
      result = result.filter((c) => c.agentId === agentFilter);
    }

    if (searchQuery.trim()) {
      const q = searchQuery.toLowerCase().trim();
      result = result.filter((c) => {
        const agent = agentMap[c.agentId];
        const agentName = agent?.displayName?.toLowerCase() ?? '';
        const entityInfo = `${c.entityType ?? ''} ${c.entityId ?? ''}`.toLowerCase();
        return agentName.includes(q) || entityInfo.includes(q);
      });
    }

    return result;
  }, [conversations, statusFilter, agentFilter, searchQuery, agentMap]);

  // Stats
  const totalActive = conversations.filter((c) => c.status === ConversationStatus.Active).length;
  const totalCompleted = conversations.filter((c) => c.status === ConversationStatus.Completed).length;

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ p: { xs: 2, md: 3 }, maxWidth: 1200, mx: 'auto' }}>
      {/* Page Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3, gap: 1.5 }}>
        <HistoryIcon sx={{ fontSize: 32, color: '#6750A4' }} />
        <Box sx={{ flex: 1 }}>
          <Typography variant="h5" sx={{ fontWeight: 600, color: '#1C1B1F' }}>
            Conversation History
          </Typography>
          <Typography variant="body2" sx={{ color: '#49454F' }}>
            Browse and resume past AI agent conversations
          </Typography>
        </Box>
        <Tooltip title="Refresh">
          <IconButton onClick={loadData} disabled={loading}>
            <RefreshIcon />
          </IconButton>
        </Tooltip>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Summary Chips */}
      <Box sx={{ display: 'flex', gap: 1, mb: 2, flexWrap: 'wrap' }}>
        <Chip
          label={`${conversations.length} Total`}
          size="small"
          sx={{ fontWeight: 500, backgroundColor: '#F3EDF7', color: '#6750A4' }}
        />
        <Chip
          icon={<ChatIcon sx={{ fontSize: 14 }} />}
          label={`${totalActive} Active`}
          size="small"
          sx={{ fontWeight: 500, backgroundColor: '#E8F5E9', color: '#2e7d32' }}
        />
        <Chip
          icon={<CheckCircleIcon sx={{ fontSize: 14 }} />}
          label={`${totalCompleted} Completed`}
          size="small"
          sx={{ fontWeight: 500, backgroundColor: '#E3F2FD', color: '#1565c0' }}
        />
      </Box>

      {/* Filters */}
      <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
        <TextField
          size="small"
          placeholder="Search by agent or entity…"
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon sx={{ color: '#49454F', fontSize: 20 }} />
              </InputAdornment>
            ),
          }}
          sx={{ minWidth: 260, '& .MuiOutlinedInput-root': { borderRadius: 2 } }}
        />
        <FormControl size="small" sx={{ minWidth: 160 }}>
          <InputLabel>Status</InputLabel>
          <Select
            value={statusFilter}
            label="Status"
            onChange={(e) => setStatusFilter(Number(e.target.value) as ConversationStatus | -1)}
            sx={{ borderRadius: 2 }}
          >
            <MenuItem value={-1}>All Statuses</MenuItem>
            {Object.entries(statusConfig).map(([key, cfg]) => (
              <MenuItem key={key} value={Number(key)}>
                {cfg.label}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
        <FormControl size="small" sx={{ minWidth: 180 }}>
          <InputLabel>Agent</InputLabel>
          <Select
            value={agentFilter}
            label="Agent"
            onChange={(e) => setAgentFilter(Number(e.target.value))}
            sx={{ borderRadius: 2 }}
          >
            <MenuItem value={-1}>All Agents</MenuItem>
            {agents.map((agent) => (
              <MenuItem key={agent.id} value={agent.id}>
                {agent.displayName}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>

      {/* Conversation List */}
      {filtered.length === 0 ? (
        <Box
          sx={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            py: 8,
          }}
        >
          <HistoryIcon sx={{ fontSize: 64, color: '#CAC4D0', mb: 2 }} />
          <Typography variant="h6" sx={{ color: '#49454F', mb: 1 }}>
            No conversations found
          </Typography>
          <Typography variant="body2" sx={{ color: '#79747E', mb: 2 }}>
            {searchQuery || statusFilter !== -1 || agentFilter !== -1
              ? 'Try adjusting your search or filter criteria.'
              : 'Start chatting with an AI agent to see your history here.'}
          </Typography>
          <Button
            variant="outlined"
            startIcon={<SmartToyIcon />}
            onClick={() => navigate('/agents')}
            sx={{ textTransform: 'none', borderRadius: 2 }}
          >
            Browse Agents
          </Button>
        </Box>
      ) : (
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
          {filtered.map((convo) => {
            const agent = agentMap[convo.agentId];
            const typeColor = agent ? getAgentTypeColor(agent.agentType) : '#757575';
            const typeLabel = agent ? (AgentTypeLabels[agent.agentType] || 'Agent') : 'Unknown';
            const status = statusConfig[convo.status] ?? statusConfig[ConversationStatus.Active];
            const canResume = convo.status === ConversationStatus.Active || convo.status === ConversationStatus.WaitingForApproval;

            return (
              <Card
                key={convo.id}
                sx={{
                  borderRadius: 2,
                  border: '1px solid #E7E0EC',
                  boxShadow: '0 1px 2px rgba(0,0,0,0.06)',
                  transition: 'box-shadow 0.2s',
                  cursor: 'pointer',
                  '&:hover': { boxShadow: '0 3px 8px rgba(0,0,0,0.1)' },
                }}
                onClick={() => {
                  if (agent) {
                    navigate(`/agents/${agent.id}/chat`);
                  }
                }}
              >
                <CardContent sx={{ py: 2, px: 2.5, '&:last-child': { pb: 2 } }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    {/* Agent Avatar */}
                    <Avatar
                      sx={{
                        bgcolor: typeColor,
                        width: 40,
                        height: 40,
                        fontSize: 16,
                        fontWeight: 600,
                      }}
                    >
                      {agent?.displayName?.charAt(0).toUpperCase() ?? '?'}
                    </Avatar>

                    {/* Main Info */}
                    <Box sx={{ flex: 1, minWidth: 0 }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.25 }}>
                        <Typography variant="subtitle2" sx={{ fontWeight: 600, color: '#1C1B1F' }} noWrap>
                          {agent?.displayName ?? `Agent #${convo.agentId}`}
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
                        <Chip
                          icon={status.icon as React.ReactElement}
                          label={status.label}
                          size="small"
                          sx={{
                            height: 18,
                            fontSize: 10,
                            fontWeight: 500,
                            backgroundColor: `${status.color}14`,
                            color: status.color,
                            '& .MuiChip-label': { px: 0.5 },
                            '& .MuiChip-icon': { ml: 0.5 },
                          }}
                        />
                      </Box>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                        <Typography variant="caption" sx={{ color: '#79747E', display: 'flex', alignItems: 'center', gap: 0.5 }}>
                          <AccessTimeIcon sx={{ fontSize: 12 }} /> {formatTimestamp(convo.createdAt)}
                        </Typography>
                        <Typography variant="caption" sx={{ color: '#79747E' }}>
                          {convo.messageCount} message{convo.messageCount !== 1 ? 's' : ''}
                        </Typography>
                        {convo.totalTokensUsed > 0 && (
                          <Typography variant="caption" sx={{ color: '#79747E' }}>
                            {convo.totalTokensUsed.toLocaleString()} tokens
                          </Typography>
                        )}
                        {convo.entityType && (
                          <Chip
                            label={`${convo.entityType} #${convo.entityId}`}
                            size="small"
                            variant="outlined"
                            sx={{ height: 18, fontSize: 10, '& .MuiChip-label': { px: 0.5 } }}
                          />
                        )}
                      </Box>
                    </Box>

                    {/* Rating */}
                    {convo.userRating != null && convo.userRating > 0 && (
                      <Rating value={convo.userRating} precision={0.5} size="small" readOnly />
                    )}

                    {/* Resume button */}
                    {canResume && (
                      <Button
                        variant="outlined"
                        size="small"
                        startIcon={<ChatIcon />}
                        onClick={(e) => {
                          e.stopPropagation();
                          if (agent) navigate(`/agents/${agent.id}/chat`);
                        }}
                        sx={{
                          textTransform: 'none',
                          borderRadius: 2,
                          borderColor: '#6750A4',
                          color: '#6750A4',
                          '&:hover': { backgroundColor: '#F3EDF7' },
                        }}
                      >
                        Resume
                      </Button>
                    )}
                  </Box>
                </CardContent>
              </Card>
            );
          })}
        </Box>
      )}
    </Box>
  );
};

export default ConversationHistoryPage;
