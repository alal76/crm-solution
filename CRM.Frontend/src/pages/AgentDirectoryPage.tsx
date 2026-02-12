import { useState, useEffect, useMemo } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Grid,
  TextField,
  Button,
  Chip,
  CircularProgress,
  Alert,
  Avatar,
  Rating,
  InputAdornment,
  Tooltip,
} from '@mui/material';
import {
  SmartToy as SmartToyIcon,
  Search as SearchIcon,
  Chat as ChatIcon,
  VerifiedUser as VerifiedUserIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import apiClient from '../services/apiClient';
import agentService from '../services/agentService';
import {
  Agent,
  AgentType,
  AgentTypeLabels,
  getAgentTypeColor,
} from '../types/agents';

// Category filter definitions
type CategoryKey = 'All' | 'Sales' | 'Support' | 'Analytics' | 'General';

const CATEGORY_TYPES: Record<CategoryKey, AgentType[] | null> = {
  All: null,
  Sales: [
    AgentType.LeadScoring,
    AgentType.SalesIntelligence,
    AgentType.SalesAssistant,
    AgentType.DealIntelligence,
    AgentType.ForecastAnalyst,
    AgentType.SalesCoach,
    AgentType.RevenueIntelligence,
    AgentType.ContractAnalyst,
  ],
  Support: [
    AgentType.SupportTriage,
    AgentType.TicketResolution,
    AgentType.CustomerSuccess,
    AgentType.KnowledgeExpert,
  ],
  Analytics: [
    AgentType.DataAnalyst,
    AgentType.NextBestAction,
    AgentType.DocumentIntelligence,
    AgentType.MeetingIntelligence,
    AgentType.ConversationIntelligence,
  ],
  General: [
    AgentType.GeneralAssistant,
    AgentType.EmailAssistant,
    AgentType.OnboardingGuide,
    AgentType.Orchestrator,
  ],
};

const CATEGORY_COLORS: Record<CategoryKey, string> = {
  All: '#6750A4',
  Sales: '#1565c0',
  Support: '#2e7d32',
  Analytics: '#e65100',
  General: '#6a1b9a',
};

const getCategoryForAgent = (agentType: AgentType): CategoryKey => {
  for (const [category, types] of Object.entries(CATEGORY_TYPES)) {
    if (types && types.includes(agentType)) {
      return category as CategoryKey;
    }
  }
  return 'General';
};

const AgentDirectoryPage = () => {
  const navigate = useNavigate();

  // State
  const [agents, setAgents] = useState<Agent[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<CategoryKey>('All');

  // Load agents on mount
  useEffect(() => {
    const fetchAgents = async () => {
      try {
        setLoading(true);
        setError(null);
        const response = await agentService.getAll();
        const activeAgents = (response.data || []).filter((a: Agent) => a.isActive);
        setAgents(activeAgents);
      } catch (err: any) {
        console.error('Failed to load agents:', err);
        setError(err?.response?.data?.message || 'Failed to load AI agents. Please try again.');
      } finally {
        setLoading(false);
      }
    };
    fetchAgents();
  }, []);

  // Filtered agents (client-side search + category)
  const filteredAgents = useMemo(() => {
    let result = agents;

    // Category filter
    if (selectedCategory !== 'All') {
      const allowedTypes = CATEGORY_TYPES[selectedCategory];
      if (allowedTypes) {
        result = result.filter((a) => allowedTypes.includes(a.agentType));
      }
    }

    // Search filter
    if (searchQuery.trim()) {
      const q = searchQuery.toLowerCase().trim();
      result = result.filter(
        (a) =>
          a.displayName.toLowerCase().includes(q) ||
          (a.description && a.description.toLowerCase().includes(q)) ||
          a.name.toLowerCase().includes(q)
      );
    }

    return result;
  }, [agents, searchQuery, selectedCategory]);

  // Category counts
  const categoryCounts = useMemo(() => {
    const counts: Record<CategoryKey, number> = { All: agents.length, Sales: 0, Support: 0, Analytics: 0, General: 0 };
    agents.forEach((a) => {
      const cat = getCategoryForAgent(a.agentType);
      counts[cat] = (counts[cat] || 0) + 1;
    });
    return counts;
  }, [agents]);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      {/* Page Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
        <SmartToyIcon sx={{ fontSize: 36, color: '#6750A4', mr: 1.5 }} />
        <Box sx={{ flex: 1 }}>
          <Typography variant="h5" sx={{ fontWeight: 600, color: '#1C1B1F' }}>
            AI Agents
          </Typography>
          <Typography variant="body2" sx={{ color: '#49454F' }}>
            Discover and chat with AI-powered assistants
          </Typography>
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {/* Search Bar */}
      <Box sx={{ mb: 2 }}>
        <TextField
          fullWidth
          size="small"
          placeholder="Search agents by name or description..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon sx={{ color: '#49454F' }} />
              </InputAdornment>
            ),
          }}
          sx={{
            maxWidth: 480,
            '& .MuiOutlinedInput-root': {
              borderRadius: 2,
            },
          }}
        />
      </Box>

      {/* Category Filter Chips */}
      <Box sx={{ display: 'flex', gap: 1, mb: 3, flexWrap: 'wrap' }}>
        {(Object.keys(CATEGORY_TYPES) as CategoryKey[]).map((category) => (
          <Chip
            key={category}
            label={`${category} (${categoryCounts[category] || 0})`}
            onClick={() => setSelectedCategory(category)}
            variant={selectedCategory === category ? 'filled' : 'outlined'}
            sx={{
              fontWeight: 500,
              ...(selectedCategory === category
                ? {
                    backgroundColor: CATEGORY_COLORS[category],
                    color: '#fff',
                    '&:hover': { backgroundColor: CATEGORY_COLORS[category], opacity: 0.9 },
                  }
                : {
                    borderColor: CATEGORY_COLORS[category],
                    color: CATEGORY_COLORS[category],
                    '&:hover': { backgroundColor: `${CATEGORY_COLORS[category]}14` },
                  }),
            }}
          />
        ))}
      </Box>

      {/* Agent Cards Grid */}
      {filteredAgents.length === 0 ? (
        <Box
          sx={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            py: 8,
          }}
        >
          <SmartToyIcon sx={{ fontSize: 64, color: '#CAC4D0', mb: 2 }} />
          <Typography variant="h6" sx={{ color: '#49454F', mb: 1 }}>
            No AI agents available
          </Typography>
          <Typography variant="body2" sx={{ color: '#79747E' }}>
            {searchQuery || selectedCategory !== 'All'
              ? 'Try adjusting your search or filter criteria.'
              : 'No active agents have been configured yet.'}
          </Typography>
        </Box>
      ) : (
        <Grid container spacing={2.5}>
          {filteredAgents.map((agent) => {
            const typeColor = getAgentTypeColor(agent.agentType);
            const typeLabel = AgentTypeLabels[agent.agentType] || 'Agent';

            return (
              <Grid item xs={12} sm={6} md={4} key={agent.id}>
                <Card
                  sx={{
                    height: '100%',
                    display: 'flex',
                    flexDirection: 'column',
                    borderRadius: 2,
                    border: '1px solid #E7E0EC',
                    boxShadow: '0 1px 3px rgba(0,0,0,0.08)',
                    transition: 'box-shadow 0.2s, transform 0.2s',
                    '&:hover': {
                      boxShadow: '0 4px 12px rgba(0,0,0,0.12)',
                      transform: 'translateY(-2px)',
                    },
                  }}
                >
                  <CardContent sx={{ flex: 1, display: 'flex', flexDirection: 'column', p: 2.5 }}>
                    {/* Agent Avatar + Name */}
                    <Box sx={{ display: 'flex', alignItems: 'center', mb: 1.5 }}>
                      <Avatar
                        sx={{
                          bgcolor: typeColor,
                          width: 44,
                          height: 44,
                          mr: 1.5,
                          fontSize: 18,
                          fontWeight: 600,
                        }}
                      >
                        {agent.displayName.charAt(0).toUpperCase()}
                      </Avatar>
                      <Box sx={{ flex: 1, minWidth: 0 }}>
                        <Typography
                          variant="subtitle1"
                          sx={{ fontWeight: 600, lineHeight: 1.3, color: '#1C1B1F' }}
                          noWrap
                        >
                          {agent.displayName}
                        </Typography>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.25 }}>
                          <Chip
                            label={typeLabel}
                            size="small"
                            sx={{
                              height: 20,
                              fontSize: 11,
                              fontWeight: 500,
                              backgroundColor: `${typeColor}18`,
                              color: typeColor,
                              '& .MuiChip-label': { px: 1 },
                            }}
                          />
                          {agent.requiresApproval && (
                            <Tooltip title="Actions may require human approval">
                              <Chip
                                icon={<VerifiedUserIcon sx={{ fontSize: 12 }} />}
                                label="Approval"
                                size="small"
                                sx={{
                                  height: 20,
                                  fontSize: 10,
                                  fontWeight: 500,
                                  backgroundColor: '#FFF3E0',
                                  color: '#E65100',
                                  '& .MuiChip-label': { px: 0.5 },
                                  '& .MuiChip-icon': { ml: 0.5 },
                                }}
                              />
                            </Tooltip>
                          )}
                        </Box>
                      </Box>
                    </Box>

                    {/* Description */}
                    <Typography
                      variant="body2"
                      sx={{
                        color: '#49454F',
                        mb: 1.5,
                        flex: 1,
                        display: '-webkit-box',
                        WebkitLineClamp: 2,
                        WebkitBoxOrient: 'vertical',
                        overflow: 'hidden',
                        lineHeight: 1.5,
                        minHeight: 42,
                      }}
                    >
                      {agent.description || 'No description available.'}
                    </Typography>

                    {/* Rating + Conversations */}
                    <Box sx={{ display: 'flex', alignItems: 'center', mb: 1.5 }}>
                      <Rating
                        value={agent.averageRating || 0}
                        precision={0.5}
                        size="small"
                        readOnly
                        sx={{ mr: 1 }}
                      />
                      <Typography variant="caption" sx={{ color: '#79747E' }}>
                        {agent.totalConversations > 0
                          ? `${agent.totalConversations} conversation${agent.totalConversations !== 1 ? 's' : ''}`
                          : 'No conversations yet'}
                      </Typography>
                    </Box>

                    {/* Start Chat Button */}
                    <Button
                      variant="contained"
                      startIcon={<ChatIcon />}
                      onClick={() => navigate(`/agents/${agent.id}/chat`)}
                      fullWidth
                      sx={{
                        mt: 'auto',
                        textTransform: 'none',
                        borderRadius: 2,
                        backgroundColor: '#6750A4',
                        '&:hover': { backgroundColor: '#57439B' },
                      }}
                    >
                      Start Chat
                    </Button>
                  </CardContent>
                </Card>
              </Grid>
            );
          })}
        </Grid>
      )}
    </Box>
  );
};

export default AgentDirectoryPage;
