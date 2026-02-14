import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Switch,
  Button,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Slider,
  Chip,
  Tabs,
  Tab,
  Alert,
  CircularProgress,
  Tooltip,
  Rating,
  Avatar,
} from '@mui/material';
import {
  SettingsOutlined,
  EditOutlined,
  SmartToyOutlined,
  ChatOutlined,
  StarOutlined,
  CheckCircleOutline,
} from '@mui/icons-material';
import {
  Agent,
  AgentType,
  AgentTypeLabels,
  UpdateAgentRequest,
  getAgentTypeColor,
} from '../types/agents';
import agentAdminService from '../services/agentAdminService';

// ─── Tab panel helper ───────────────────────────────────────────────────────
interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

const TabPanel: React.FC<TabPanelProps> = ({ children, value, index }) => (
  <Box role="tabpanel" hidden={value !== index} sx={{ pt: 2 }}>
    {value === index && children}
  </Box>
);

// ─── Summary card helper ────────────────────────────────────────────────────
interface SummaryCardProps {
  icon: React.ReactNode;
  label: string;
  value: string | number;
  color: string;
}

const SummaryCard: React.FC<SummaryCardProps> = ({ icon, label, value, color }) => (
  <Paper
    elevation={0}
    sx={{
      p: 2.5,
      flex: 1,
      minWidth: 180,
      border: '1px solid',
      borderColor: 'divider',
      borderRadius: 2,
      display: 'flex',
      alignItems: 'center',
      gap: 2,
    }}
  >
    <Avatar sx={{ bgcolor: `${color}14`, color, width: 44, height: 44 }}>
      {icon}
    </Avatar>
    <Box>
      <Typography variant="body2" color="text.secondary">
        {label}
      </Typography>
      <Typography variant="h5" fontWeight={700}>
        {value}
      </Typography>
    </Box>
  </Paper>
);

// ─── Main component ─────────────────────────────────────────────────────────
const AgentManagementPage: React.FC = () => {
  // ── state ───────────────────────────────────────────────────────────────
  const [agents, setAgents] = useState<Agent[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [selectedAgent, setSelectedAgent] = useState<Agent | null>(null);
  const [activeTab, setActiveTab] = useState(0);
  const [saving, setSaving] = useState(false);

  // edit form state
  const [editSystemPrompt, setEditSystemPrompt] = useState('');
  const [editTemperature, setEditTemperature] = useState(0.3);
  const [editMaxTokens, setEditMaxTokens] = useState(4096);
  const [editAllowedPlugins, setEditAllowedPlugins] = useState('');
  const [editModelOverride, setEditModelOverride] = useState('');

  // ── data fetching ───────────────────────────────────────────────────────
  const loadAgents = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await agentAdminService.getConfigs();
      setAgents(response.data ?? []);
    } catch (err: any) {
      const msg =
        err?.response?.data?.message ||
        err?.message ||
        'Failed to load agents';
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadAgents();
  }, [loadAgents]);

  // auto-dismiss success
  useEffect(() => {
    if (!success) return;
    const timer = setTimeout(() => setSuccess(null), 3000);
    return () => clearTimeout(timer);
  }, [success]);

  // ── toggle agent ────────────────────────────────────────────────────────
  const handleToggle = async (agent: Agent) => {
    try {
      setError(null);
      await agentAdminService.toggleAgent(agent.id);
      setSuccess(
        `${agent.displayName} has been ${agent?.isActive !== false ? 'deactivated' : 'activated'}.`,
      );
      await loadAgents();
    } catch (err: any) {
      const msg =
        err?.response?.data?.message ||
        err?.message ||
        'Failed to toggle agent';
      setError(msg);
    }
  };

  // ── open edit dialog ────────────────────────────────────────────────────
  const handleEdit = (agent: Agent) => {
    setSelectedAgent(agent);
    setEditSystemPrompt(agent.systemPrompt ?? '');
    setEditTemperature(agent.temperature ?? 0.3);
    setEditMaxTokens(agent.maxTokens ?? 4096);
    setEditAllowedPlugins(agent.allowedPlugins ?? '');
    setEditModelOverride(agent.modelOverride ?? '');
    setActiveTab(0);
    setEditDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setEditDialogOpen(false);
    setSelectedAgent(null);
  };

  // ── save config ─────────────────────────────────────────────────────────
  const handleSave = async () => {
    if (!selectedAgent) return;

    try {
      setSaving(true);
      setError(null);

      const request: UpdateAgentRequest = {
        systemPrompt: editSystemPrompt,
        temperature: editTemperature,
        maxTokens: editMaxTokens,
        allowedPlugins: editAllowedPlugins,
        modelOverride: editModelOverride || undefined,
      };

      await agentAdminService.updateConfig(selectedAgent.id, request);
      setSuccess(`Configuration for "${selectedAgent.displayName}" saved.`);
      handleCloseDialog();
      await loadAgents();
    } catch (err: any) {
      const msg =
        err?.response?.data?.message ||
        err?.message ||
        'Failed to save agent configuration';
      setError(msg);
    } finally {
      setSaving(false);
    }
  };

  // ── computed stats ──────────────────────────────────────────────────────
  const totalAgents = agents.length;
  const activeAgents = agents.filter((a) => a?.isActive !== false).length;
  const totalConversations = agents.reduce(
    (sum, a) => sum + (a.totalConversations ?? 0),
    0,
  );
  const ratedAgents = agents.filter(
    (a) => a.averageRating != null && a.averageRating > 0,
  );
  const avgRating =
    ratedAgents.length > 0
      ? ratedAgents.reduce((sum, a) => sum + (a.averageRating ?? 0), 0) /
        ratedAgents.length
      : 0;

  // ── parsed plugin chips ─────────────────────────────────────────────────
  const parsePlugins = (raw: string): string[] =>
    raw
      .split(',')
      .map((p) => p.trim())
      .filter(Boolean);

  // ── render ──────────────────────────────────────────────────────────────
  return (
    <Box sx={{ p: { xs: 2, md: 3 }, maxWidth: 1400, mx: 'auto' }}>
      {/* ── Page Header ────────────────────────────────────────────────── */}
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3, gap: 1.5 }}>
        <SettingsOutlined sx={{ fontSize: 32, color: '#6750A4' }} />
        <Box>
          <Typography variant="h5" fontWeight={700}>
            Agent Management
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Configure and manage AI agents
          </Typography>
        </Box>
      </Box>

      {/* ── Alerts ─────────────────────────────────────────────────────── */}
      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}
      {success && (
        <Alert severity="success" sx={{ mb: 2 }}>
          {success}
        </Alert>
      )}

      {/* ── Loading state ──────────────────────────────────────────────── */}
      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
          <CircularProgress />
        </Box>
      ) : (
        <>
          {/* ── Summary Cards ────────────────────────────────────────── */}
          <Box
            sx={{
              display: 'flex',
              flexWrap: 'wrap',
              gap: 2,
              mb: 3,
            }}
          >
            <SummaryCard
              icon={<SmartToyOutlined />}
              label="Total Agents"
              value={totalAgents}
              color="#6750A4"
            />
            <SummaryCard
              icon={<CheckCircleOutline />}
              label="Active Agents"
              value={activeAgents}
              color="#2e7d32"
            />
            <SummaryCard
              icon={<ChatOutlined />}
              label="Total Conversations"
              value={totalConversations.toLocaleString()}
              color="#1565c0"
            />
            <SummaryCard
              icon={<StarOutlined />}
              label="Avg Rating"
              value={avgRating > 0 ? avgRating.toFixed(1) : '—'}
              color="#e65100"
            />
          </Box>

          {/* ── Agent Table ──────────────────────────────────────────── */}
          <TableContainer
            component={Paper}
            variant="outlined"
            sx={{ borderRadius: 2 }}
          >
            <Table>
              <TableHead
                sx={{
                  backgroundColor: '#F5EFF7',
                  '& .MuiTableCell-head': {
                    color: '#6750A4',
                    fontWeight: 600,
                  },
                }}
              >
                <TableRow>
                  <TableCell>Agent</TableCell>
                  <TableCell>Type</TableCell>
                  <TableCell align="center">Active</TableCell>
                  <TableCell align="center">Approval</TableCell>
                  <TableCell align="right">Conversations</TableCell>
                  <TableCell>Rating</TableCell>
                  <TableCell align="center">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {agents.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={7} align="center" sx={{ py: 6 }}>
                      <Typography color="text.secondary">
                        No agents found.
                      </Typography>
                    </TableCell>
                  </TableRow>
                ) : (
                  agents.map((agent) => {
                    const typeColor = getAgentTypeColor(agent.agentType);
                    return (
                      <TableRow
                        key={agent.id}
                        hover
                        sx={{
                          opacity: agent?.isActive !== false ? 1 : 0.6,
                          '&:last-child td': { borderBottom: 0 },
                        }}
                      >
                        {/* Agent name + avatar */}
                        <TableCell>
                          <Box
                            sx={{
                              display: 'flex',
                              alignItems: 'center',
                              gap: 1.5,
                            }}
                          >
                            <Avatar
                              sx={{
                                bgcolor: typeColor,
                                width: 36,
                                height: 36,
                                fontSize: 14,
                              }}
                            >
                              {agent.displayName
                                .split(' ')
                                .map((w) => w[0])
                                .join('')
                                .slice(0, 2)
                                .toUpperCase()}
                            </Avatar>
                            <Box>
                              <Typography variant="body2" fontWeight={600}>
                                {agent.displayName}
                              </Typography>
                              <Typography
                                variant="caption"
                                color="text.secondary"
                              >
                                {agent.name}
                              </Typography>
                            </Box>
                          </Box>
                        </TableCell>

                        {/* Type chip */}
                        <TableCell>
                          <Chip
                            label={
                              AgentTypeLabels[agent.agentType] ??
                              AgentType[agent.agentType] ??
                              'Unknown'
                            }
                            size="small"
                            sx={{
                              bgcolor: `${typeColor}18`,
                              color: typeColor,
                              fontWeight: 500,
                              borderRadius: 1,
                            }}
                          />
                        </TableCell>

                        {/* Active toggle */}
                        <TableCell align="center">
                          <Tooltip
                            title={
                              agent?.isActive !== false ? 'Deactivate' : 'Activate'
                            }
                          >
                            <Switch
                              checked={agent?.isActive !== false}
                              onChange={() => handleToggle(agent)}
                              color="primary"
                              size="small"
                            />
                          </Tooltip>
                        </TableCell>

                        {/* Approval */}
                        <TableCell align="center">
                          {agent.requiresApproval ? (
                            <Chip
                              label="Required"
                              size="small"
                              color="warning"
                              variant="outlined"
                              sx={{ fontWeight: 500, borderRadius: 1 }}
                            />
                          ) : (
                            <Typography
                              variant="body2"
                              color="text.disabled"
                            >
                              —
                            </Typography>
                          )}
                        </TableCell>

                        {/* Conversations */}
                        <TableCell align="right">
                          <Typography variant="body2">
                            {(agent.totalConversations ?? 0).toLocaleString()}
                          </Typography>
                        </TableCell>

                        {/* Rating */}
                        <TableCell>
                          {agent.averageRating != null &&
                          agent.averageRating > 0 ? (
                            <Box
                              sx={{
                                display: 'flex',
                                alignItems: 'center',
                                gap: 0.5,
                              }}
                            >
                              <Rating
                                value={agent.averageRating}
                                readOnly
                                size="small"
                                precision={0.1}
                              />
                              <Typography
                                variant="caption"
                                color="text.secondary"
                              >
                                {agent.averageRating.toFixed(1)}
                              </Typography>
                            </Box>
                          ) : (
                            <Typography
                              variant="body2"
                              color="text.disabled"
                            >
                              —
                            </Typography>
                          )}
                        </TableCell>

                        {/* Actions */}
                        <TableCell align="center">
                          <Tooltip title="Edit Configuration">
                            <IconButton
                              size="small"
                              onClick={() => handleEdit(agent)}
                            >
                              <EditOutlined fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    );
                  })
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </>
      )}

      {/* ── Config Editor Dialog ────────────────────────────────────────── */}
      <Dialog
        open={editDialogOpen}
        onClose={handleCloseDialog}
        maxWidth="md"
        fullWidth
        PaperProps={{ sx: { borderRadius: 2 } }}
      >
        <DialogTitle
          sx={{
            display: 'flex',
            alignItems: 'center',
            gap: 1.5,
            pb: 1,
          }}
        >
          {selectedAgent && (
            <Avatar
              sx={{
                bgcolor: getAgentTypeColor(selectedAgent.agentType),
                width: 36,
                height: 36,
                fontSize: 14,
              }}
            >
              {selectedAgent.displayName
                .split(' ')
                .map((w) => w[0])
                .join('')
                .slice(0, 2)
                .toUpperCase()}
            </Avatar>
          )}
          <Box>
            <Typography variant="h6" fontWeight={600}>
              {selectedAgent?.displayName ?? 'Agent'} — Configuration
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {selectedAgent
                ? AgentTypeLabels[selectedAgent.agentType] ?? 'Agent'
                : ''}
            </Typography>
          </Box>
        </DialogTitle>

        <DialogContent dividers sx={{ p: 0 }}>
          <Tabs
            value={activeTab}
            onChange={(_, v) => setActiveTab(v)}
            sx={{
              px: 2,
              borderBottom: 1,
              borderColor: 'divider',
              '& .MuiTab-root': { textTransform: 'none', fontWeight: 500 },
            }}
          >
            <Tab label="General" />
            <Tab label="Model" />
            <Tab label="Plugins" />
            <Tab label="Approval" />
          </Tabs>

          <Box sx={{ p: 3 }}>
            {/* ─── Tab 1: General ──────────────────────────────────── */}
            <TabPanel value={activeTab} index={0}>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2.5 }}>
                <TextField
                  label="Display Name"
                  value={selectedAgent?.displayName ?? ''}
                  disabled
                  fullWidth
                  size="small"
                />
                <TextField
                  label="Name"
                  value={selectedAgent?.name ?? ''}
                  disabled
                  fullWidth
                  size="small"
                />
                <TextField
                  label="Description"
                  value={selectedAgent?.description ?? ''}
                  disabled
                  fullWidth
                  multiline
                  minRows={2}
                  size="small"
                />
                <TextField
                  label="System Prompt"
                  value={editSystemPrompt}
                  onChange={(e) => setEditSystemPrompt(e.target.value)}
                  fullWidth
                  multiline
                  minRows={6}
                  size="small"
                  helperText="Defines the agent's personality, instructions, and constraints."
                />
              </Box>
            </TabPanel>

            {/* ─── Tab 2: Model ────────────────────────────────────── */}
            <TabPanel value={activeTab} index={1}>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
                <Box>
                  <Typography variant="subtitle2" gutterBottom>
                    Temperature
                  </Typography>
                  <Typography
                    variant="caption"
                    color="text.secondary"
                    sx={{ mb: 1, display: 'block' }}
                  >
                    Controls randomness. Lower values are more focused, higher
                    values are more creative.
                  </Typography>
                  <Box sx={{ px: 1 }}>
                    <Slider
                      value={editTemperature}
                      onChange={(_, v) => setEditTemperature(v as number)}
                      min={0}
                      max={2}
                      step={0.1}
                      valueLabelDisplay="on"
                      marks={[
                        { value: 0, label: '0' },
                        { value: 0.5, label: '0.5' },
                        { value: 1, label: '1' },
                        { value: 1.5, label: '1.5' },
                        { value: 2, label: '2' },
                      ]}
                      sx={{ mt: 3 }}
                    />
                  </Box>
                </Box>

                <TextField
                  label="Max Tokens"
                  type="number"
                  value={editMaxTokens}
                  onChange={(e) => {
                    const v = parseInt(e.target.value, 10);
                    if (!isNaN(v)) setEditMaxTokens(v);
                  }}
                  inputProps={{ min: 256, max: 16384 }}
                  fullWidth
                  size="small"
                  helperText="Maximum number of tokens in the response (256 – 16 384)."
                />

                <TextField
                  label="Model Override"
                  value={editModelOverride}
                  onChange={(e) => setEditModelOverride(e.target.value)}
                  fullWidth
                  size="small"
                  placeholder="Leave blank for default"
                  helperText="Override the default model for this agent (e.g. gpt-4o, claude-3-opus)."
                />
              </Box>
            </TabPanel>

            {/* ─── Tab 3: Plugins ──────────────────────────────────── */}
            <TabPanel value={activeTab} index={2}>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                <TextField
                  label="Allowed Plugins"
                  value={editAllowedPlugins}
                  onChange={(e) => setEditAllowedPlugins(e.target.value)}
                  fullWidth
                  multiline
                  minRows={3}
                  size="small"
                  helperText="Comma-separated plugin names that this agent is allowed to use."
                />

                {editAllowedPlugins.trim() && (
                  <Box>
                    <Typography
                      variant="subtitle2"
                      color="text.secondary"
                      gutterBottom
                    >
                      Current Plugins
                    </Typography>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.75 }}>
                      {parsePlugins(editAllowedPlugins).map((plugin) => (
                        <Chip
                          key={plugin}
                          label={plugin}
                          size="small"
                          variant="outlined"
                          sx={{ borderRadius: 1 }}
                        />
                      ))}
                    </Box>
                  </Box>
                )}
              </Box>
            </TabPanel>

            {/* ─── Tab 4: Approval ─────────────────────────────────── */}
            <TabPanel value={activeTab} index={3}>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2.5 }}>
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                  }}
                >
                  <Box>
                    <Typography variant="subtitle2">
                      Requires Approval
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      Whether actions taken by this agent need human approval
                      before execution.
                    </Typography>
                  </Box>
                  <Switch
                    checked={selectedAgent?.requiresApproval ?? false}
                    disabled
                    color="primary"
                  />
                </Box>

                <TextField
                  label="Approval Tier"
                  value={selectedAgent?.approvalTier ?? 'N/A'}
                  disabled
                  fullWidth
                  size="small"
                />

                <Alert severity="info" variant="outlined" sx={{ mt: 1 }}>
                  <Typography variant="body2">
                    Approval settings are defined at the agent seed level and
                    cannot be changed through this interface. When approval is
                    required, the agent will pause before executing write
                    operations and wait for a human to approve or reject the
                    action via the Approval Hub.
                  </Typography>
                </Alert>
              </Box>
            </TabPanel>
          </Box>
        </DialogContent>

        <DialogActions sx={{ px: 3, py: 2 }}>
          <Button onClick={handleCloseDialog} color="inherit">
            Cancel
          </Button>
          <Button
            variant="contained"
            onClick={handleSave}
            disabled={saving}
            startIcon={saving ? <CircularProgress size={16} /> : undefined}
            sx={{
              bgcolor: '#6750A4',
              '&:hover': { bgcolor: '#553d8a' },
            }}
          >
            {saving ? 'Saving…' : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default AgentManagementPage;
