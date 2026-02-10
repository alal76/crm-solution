// TODO: Integration target — email compose dialog
// This component is currently orphaned (not imported by any page).

/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * EmailAIAssist - AI-powered email assistance component
 * Provides: sentiment analysis, response suggestions, subject optimization, email improvement
 */

import React, { useState, useCallback } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Collapse,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  Grid,
  IconButton,
  LinearProgress,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Paper,
  Tab,
  Tabs,
  TextField,
  Tooltip,
  Typography,
  Alert,
  Fade,
  Slider,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from '@mui/material';
import {
  AutoAwesome as AIIcon,
  Psychology as AnalyzeIcon,
  Reply as ReplyIcon,
  Subject as SubjectIcon,
  Edit as EditIcon,
  Close as CloseIcon,
  Check as CheckIcon,
  ContentCopy as CopyIcon,
  SentimentSatisfied as PositiveIcon,
  SentimentDissatisfied as NegativeIcon,
  SentimentNeutral as NeutralIcon,
  PriorityHigh as UrgentIcon,
  Schedule as ScheduleIcon,
  AttachMoney as MoneyIcon,
  Person as PersonIcon,
  Task as TaskIcon,
  Lightbulb as TipIcon,
  Speed as SpeedIcon,
  TrendingUp as ImproveIcon,
} from '@mui/icons-material';
import apiClient from '../../services/apiClient';

interface EmailAIAssistProps {
  open: boolean;
  onClose: () => void;
  emailContent: string;
  emailSubject?: string;
  accountId?: number;
  onApplySuggestion?: (suggestion: { subject?: string; body: string }) => void;
  onApplySubject?: (subject: string) => void;
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
      {value === index && <Box sx={{ p: 2 }}>{children}</Box>}
    </div>
  );
}

// API Response Types
interface SentimentInfo {
  label: string;
  confidence: number;
  explanation?: string;
}

interface EntityInfo {
  dates?: string[];
  amounts?: string[];
  names?: string[];
  action_items?: string[];
}

interface EmailAnalysis {
  sentiment?: SentimentInfo;
  urgency?: string;
  classification?: string;
  entities?: EntityInfo;
  suggested_actions?: string[];
  topics?: string[];
  summary?: string;
}

interface EmailSuggestion {
  subject: string;
  body: string;
  tone?: string;
  intent?: string;
}

interface SubjectSuggestion {
  subject: string;
  score: number;
  reason?: string;
}

interface EmailChange {
  original: string;
  improved: string;
  reason?: string;
}

interface ScoreSet {
  clarity: number;
  tone: number;
  grammar: number;
  overall: number;
}

interface EmailScores {
  original?: ScoreSet;
  improved?: ScoreSet;
}

interface ImprovedEmail {
  subject?: string;
  body: string;
}

const EmailAIAssist: React.FC<EmailAIAssistProps> = ({
  open,
  onClose,
  emailContent,
  emailSubject,
  accountId,
  onApplySuggestion,
  onApplySubject,
}) => {
  const [activeTab, setActiveTab] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Analysis state
  const [analysis, setAnalysis] = useState<EmailAnalysis | null>(null);
  const [rawAnalysis, setRawAnalysis] = useState<string | null>(null);
  
  // Response suggestions state
  const [responseSuggestions, setResponseSuggestions] = useState<EmailSuggestion[]>([]);
  const [quickReplies, setQuickReplies] = useState<string[]>([]);
  const [responseTone, setResponseTone] = useState('professional');
  
  // Subject optimization state
  const [subjectSuggestions, setSubjectSuggestions] = useState<SubjectSuggestion[]>([]);
  const [originalSubjectScore, setOriginalSubjectScore] = useState(0);
  const [subjectTips, setSubjectTips] = useState<string[]>([]);
  
  // Email improvement state
  const [improvedEmail, setImprovedEmail] = useState<ImprovedEmail | null>(null);
  const [emailChanges, setEmailChanges] = useState<EmailChange[]>([]);
  const [emailScores, setEmailScores] = useState<EmailScores | null>(null);
  const [improvementSummary, setImprovementSummary] = useState<string | null>(null);

  const analyzeEmail = useCallback(async () => {
    if (!emailContent.trim()) {
      setError('Email content is empty');
      return;
    }
    
    setLoading(true);
    setError(null);
    
    try {
      const response = await apiClient.post<{
        success: boolean;
        analysis?: EmailAnalysis;
        rawAnalysis?: string;
        error?: string;
      }>('/ai/email/analyze', {
        emailContent,
        subject: emailSubject,
        accountId,
      });
      
      if (response.data.success) {
        setAnalysis(response.data.analysis || null);
        setRawAnalysis(response.data.rawAnalysis || null);
      } else {
        setError(response.data.error || 'Analysis failed');
      }
    } catch (err) {
      console.error('Email analysis error:', err);
      setError('Failed to analyze email. Please try again.');
    } finally {
      setLoading(false);
    }
  }, [emailContent, emailSubject, accountId]);

  const getResponseSuggestions = useCallback(async () => {
    if (!emailContent.trim()) {
      setError('Email content is empty');
      return;
    }
    
    setLoading(true);
    setError(null);
    
    try {
      const response = await apiClient.post<{
        success: boolean;
        suggestions?: EmailSuggestion[];
        quickReplies?: string[];
        error?: string;
      }>('/ai/email/suggest-response', {
        emailContent,
        subject: emailSubject,
        tone: responseTone,
        numSuggestions: 3,
        accountId,
      });
      
      if (response.data.success) {
        setResponseSuggestions(response.data.suggestions || []);
        setQuickReplies(response.data.quickReplies || []);
      } else {
        setError(response.data.error || 'Failed to generate suggestions');
      }
    } catch (err) {
      console.error('Response suggestion error:', err);
      setError('Failed to generate response suggestions. Please try again.');
    } finally {
      setLoading(false);
    }
  }, [emailContent, emailSubject, responseTone, accountId]);

  const optimizeSubject = useCallback(async () => {
    if (!emailSubject?.trim() && !emailContent.trim()) {
      setError('Subject or email content is required');
      return;
    }
    
    setLoading(true);
    setError(null);
    
    try {
      const response = await apiClient.post<{
        success: boolean;
        originalScore: number;
        suggestions?: SubjectSuggestion[];
        tips?: string[];
        error?: string;
      }>('/ai/email/optimize-subject', {
        subject: emailSubject,
        emailBody: emailContent,
        purpose: 'sales',
      });
      
      if (response.data.success) {
        setOriginalSubjectScore(response.data.originalScore);
        setSubjectSuggestions(response.data.suggestions || []);
        setSubjectTips(response.data.tips || []);
      } else {
        setError(response.data.error || 'Failed to optimize subject');
      }
    } catch (err) {
      console.error('Subject optimization error:', err);
      setError('Failed to optimize subject. Please try again.');
    } finally {
      setLoading(false);
    }
  }, [emailSubject, emailContent]);

  const improveEmail = useCallback(async () => {
    if (!emailContent.trim()) {
      setError('Email content is empty');
      return;
    }
    
    setLoading(true);
    setError(null);
    
    try {
      const response = await apiClient.post<{
        success: boolean;
        improvedEmail?: ImprovedEmail;
        changes?: EmailChange[];
        scores?: EmailScores;
        summary?: string;
        error?: string;
      }>('/ai/email/improve', {
        emailContent,
        subject: emailSubject,
        improvementAreas: ['clarity', 'grammar', 'professionalism'],
      });
      
      if (response.data.success) {
        setImprovedEmail(response.data.improvedEmail || null);
        setEmailChanges(response.data.changes || []);
        setEmailScores(response.data.scores || null);
        setImprovementSummary(response.data.summary || null);
      } else {
        setError(response.data.error || 'Failed to improve email');
      }
    } catch (err) {
      console.error('Email improvement error:', err);
      setError('Failed to improve email. Please try again.');
    } finally {
      setLoading(false);
    }
  }, [emailContent, emailSubject]);

  const getSentimentIcon = (sentiment?: string) => {
    switch (sentiment?.toLowerCase()) {
      case 'positive':
        return <PositiveIcon sx={{ color: 'success.main' }} />;
      case 'negative':
        return <NegativeIcon sx={{ color: 'error.main' }} />;
      case 'mixed':
        return <NeutralIcon sx={{ color: 'warning.main' }} />;
      default:
        return <NeutralIcon sx={{ color: 'text.secondary' }} />;
    }
  };

  const getUrgencyColor = (urgency?: string): 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' => {
    switch (urgency?.toLowerCase()) {
      case 'critical':
        return 'error';
      case 'high':
        return 'warning';
      case 'medium':
        return 'info';
      default:
        return 'default';
    }
  };

  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
  };

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setActiveTab(newValue);
    setError(null);
  };

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="md"
      fullWidth
      PaperProps={{
        sx: { minHeight: '70vh' }
      }}
    >
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <AIIcon color="primary" />
          <Typography variant="h6">AI Email Assistant</Typography>
        </Box>
        <IconButton onClick={onClose} size="small">
          <CloseIcon />
        </IconButton>
      </DialogTitle>
      
      <Divider />
      
      <DialogContent sx={{ p: 0 }}>
        <Tabs
          value={activeTab}
          onChange={handleTabChange}
          variant="fullWidth"
          sx={{ borderBottom: 1, borderColor: 'divider' }}
        >
          <Tab icon={<AnalyzeIcon />} label="Analyze" />
          <Tab icon={<ReplyIcon />} label="Suggest Response" />
          <Tab icon={<SubjectIcon />} label="Optimize Subject" />
          <Tab icon={<EditIcon />} label="Improve Writing" />
        </Tabs>

        {loading && <LinearProgress />}

        {error && (
          <Alert severity="error" sx={{ m: 2 }} onClose={() => setError(null)}>
            {error}
          </Alert>
        )}

        {/* Analyze Tab */}
        <TabPanel value={activeTab} index={0}>
          <Box sx={{ textAlign: 'center', mb: 2 }}>
            <Button
              variant="contained"
              startIcon={loading ? <CircularProgress size={20} /> : <AnalyzeIcon />}
              onClick={analyzeEmail}
              disabled={loading || !emailContent.trim()}
            >
              Analyze Email
            </Button>
          </Box>

          {analysis && (
            <Fade in>
              <Box>
                {/* Sentiment & Classification */}
                <Grid container spacing={2} sx={{ mb: 2 }}>
                  <Grid item xs={6}>
                    <Card variant="outlined">
                      <CardContent>
                        <Typography variant="subtitle2" color="text.secondary">Sentiment</Typography>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 1 }}>
                          {getSentimentIcon(analysis.sentiment?.label)}
                          <Typography variant="h6" sx={{ textTransform: 'capitalize' }}>
                            {analysis.sentiment?.label || 'Unknown'}
                          </Typography>
                          <Chip
                            size="small"
                            label={`${analysis.sentiment?.confidence || 0}%`}
                            color="primary"
                            variant="outlined"
                          />
                        </Box>
                        <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                          {analysis.sentiment?.explanation}
                        </Typography>
                      </CardContent>
                    </Card>
                  </Grid>
                  <Grid item xs={6}>
                    <Card variant="outlined">
                      <CardContent>
                        <Typography variant="subtitle2" color="text.secondary">Classification</Typography>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 1, flexWrap: 'wrap' }}>
                          <Chip
                            label={analysis.classification || 'Unknown'}
                            color="primary"
                            sx={{ textTransform: 'capitalize' }}
                          />
                          {analysis.urgency && (
                            <Chip
                              icon={<UrgentIcon />}
                              label={analysis.urgency}
                              color={getUrgencyColor(analysis.urgency)}
                              size="small"
                              sx={{ textTransform: 'capitalize' }}
                            />
                          )}
                        </Box>
                      </CardContent>
                    </Card>
                  </Grid>
                </Grid>

                {/* Summary */}
                {analysis.summary && (
                  <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
                    <Typography variant="subtitle2" color="text.secondary">Summary</Typography>
                    <Typography variant="body1">{analysis.summary}</Typography>
                  </Paper>
                )}

                {/* Entities */}
                {analysis.entities && (
                  <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
                    <Typography variant="subtitle2" color="text.secondary" gutterBottom>Extracted Entities</Typography>
                    <Grid container spacing={2}>
                      {analysis.entities.dates && analysis.entities.dates.length > 0 && (
                        <Grid item xs={6}>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <ScheduleIcon fontSize="small" color="action" />
                            <Typography variant="body2">
                              <strong>Dates:</strong> {analysis.entities.dates.join(', ')}
                            </Typography>
                          </Box>
                        </Grid>
                      )}
                      {analysis.entities.amounts && analysis.entities.amounts.length > 0 && (
                        <Grid item xs={6}>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <MoneyIcon fontSize="small" color="action" />
                            <Typography variant="body2">
                              <strong>Amounts:</strong> {analysis.entities.amounts.join(', ')}
                            </Typography>
                          </Box>
                        </Grid>
                      )}
                      {analysis.entities.names && analysis.entities.names.length > 0 && (
                        <Grid item xs={6}>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <PersonIcon fontSize="small" color="action" />
                            <Typography variant="body2">
                              <strong>Names:</strong> {analysis.entities.names.join(', ')}
                            </Typography>
                          </Box>
                        </Grid>
                      )}
                      {analysis.entities.action_items && analysis.entities.action_items.length > 0 && (
                        <Grid item xs={12}>
                          <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1 }}>
                            <TaskIcon fontSize="small" color="action" />
                            <Box>
                              <Typography variant="body2"><strong>Action Items:</strong></Typography>
                              <List dense disablePadding>
                                {analysis.entities.action_items.map((item, idx) => (
                                  <ListItem key={idx} sx={{ py: 0 }}>
                                    <ListItemText primary={`• ${item}`} />
                                  </ListItem>
                                ))}
                              </List>
                            </Box>
                          </Box>
                        </Grid>
                      )}
                    </Grid>
                  </Paper>
                )}

                {/* Suggested Actions */}
                {analysis.suggested_actions && analysis.suggested_actions.length > 0 && (
                  <Paper variant="outlined" sx={{ p: 2 }}>
                    <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                      <TipIcon fontSize="small" sx={{ verticalAlign: 'middle', mr: 0.5 }} />
                      Suggested Actions
                    </Typography>
                    <List dense>
                      {analysis.suggested_actions.map((action, idx) => (
                        <ListItem key={idx}>
                          <ListItemIcon sx={{ minWidth: 32 }}>
                            <CheckIcon fontSize="small" color="primary" />
                          </ListItemIcon>
                          <ListItemText primary={action} />
                        </ListItem>
                      ))}
                    </List>
                  </Paper>
                )}

                {/* Topics */}
                {analysis.topics && analysis.topics.length > 0 && (
                  <Box sx={{ mt: 2 }}>
                    <Typography variant="subtitle2" color="text.secondary" gutterBottom>Topics</Typography>
                    <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
                      {analysis.topics.map((topic, idx) => (
                        <Chip key={idx} label={topic} size="small" variant="outlined" />
                      ))}
                    </Box>
                  </Box>
                )}
              </Box>
            </Fade>
          )}

          {rawAnalysis && !analysis && (
            <Paper variant="outlined" sx={{ p: 2 }}>
              <Typography variant="body2" component="pre" sx={{ whiteSpace: 'pre-wrap' }}>
                {rawAnalysis}
              </Typography>
            </Paper>
          )}
        </TabPanel>

        {/* Suggest Response Tab */}
        <TabPanel value={activeTab} index={1}>
          <Box sx={{ mb: 2 }}>
            <FormControl size="small" sx={{ minWidth: 200, mr: 2 }}>
              <InputLabel>Response Tone</InputLabel>
              <Select
                value={responseTone}
                label="Response Tone"
                onChange={(e) => setResponseTone(e.target.value)}
              >
                <MenuItem value="formal">Formal</MenuItem>
                <MenuItem value="friendly">Friendly</MenuItem>
                <MenuItem value="casual">Casual</MenuItem>
                <MenuItem value="apologetic">Apologetic</MenuItem>
                <MenuItem value="enthusiastic">Enthusiastic</MenuItem>
              </Select>
            </FormControl>
            <Button
              variant="contained"
              startIcon={loading ? <CircularProgress size={20} /> : <ReplyIcon />}
              onClick={getResponseSuggestions}
              disabled={loading || !emailContent.trim()}
            >
              Generate Responses
            </Button>
          </Box>

          {quickReplies.length > 0 && (
            <Box sx={{ mb: 3 }}>
              <Typography variant="subtitle2" color="text.secondary" gutterBottom>Quick Replies</Typography>
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                {quickReplies.map((reply, idx) => (
                  <Chip
                    key={idx}
                    label={reply}
                    variant="outlined"
                    onClick={() => onApplySuggestion?.({ body: reply })}
                    onDelete={() => copyToClipboard(reply)}
                    deleteIcon={<CopyIcon fontSize="small" />}
                    sx={{ maxWidth: 300 }}
                  />
                ))}
              </Box>
            </Box>
          )}

          {responseSuggestions.length > 0 && (
            <Grid container spacing={2}>
              {responseSuggestions.map((suggestion, idx) => (
                <Grid item xs={12} key={idx}>
                  <Card variant="outlined">
                    <CardContent>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
                        <Typography variant="subtitle1" fontWeight="medium">
                          Option {idx + 1}
                          {suggestion.tone && (
                            <Chip label={suggestion.tone} size="small" sx={{ ml: 1, textTransform: 'capitalize' }} />
                          )}
                          {suggestion.intent && (
                            <Chip label={suggestion.intent} size="small" variant="outlined" sx={{ ml: 0.5, textTransform: 'capitalize' }} />
                          )}
                        </Typography>
                        <Box>
                          <Tooltip title="Copy">
                            <IconButton size="small" onClick={() => copyToClipboard(suggestion.body)}>
                              <CopyIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Use this response">
                            <IconButton 
                              size="small" 
                              color="primary"
                              onClick={() => onApplySuggestion?.(suggestion)}
                            >
                              <CheckIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        </Box>
                      </Box>
                      {suggestion.subject && (
                        <Typography variant="body2" color="text.secondary" gutterBottom>
                          <strong>Subject:</strong> {suggestion.subject}
                        </Typography>
                      )}
                      <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>
                        {suggestion.body}
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
              ))}
            </Grid>
          )}
        </TabPanel>

        {/* Optimize Subject Tab */}
        <TabPanel value={activeTab} index={2}>
          <Box sx={{ textAlign: 'center', mb: 3 }}>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              Current Subject: <strong>{emailSubject || '(No subject)'}</strong>
            </Typography>
            <Button
              variant="contained"
              startIcon={loading ? <CircularProgress size={20} /> : <SubjectIcon />}
              onClick={optimizeSubject}
              disabled={loading || (!emailSubject?.trim() && !emailContent.trim())}
            >
              Optimize Subject
            </Button>
          </Box>

          {originalSubjectScore > 0 && (
            <Box sx={{ mb: 3, textAlign: 'center' }}>
              <Typography variant="subtitle2" color="text.secondary">Original Subject Score</Typography>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 2 }}>
                <SpeedIcon color={originalSubjectScore >= 70 ? 'success' : originalSubjectScore >= 50 ? 'warning' : 'error'} />
                <Typography variant="h4">{originalSubjectScore}%</Typography>
              </Box>
            </Box>
          )}

          {subjectSuggestions.length > 0 && (
            <Box sx={{ mb: 3 }}>
              <Typography variant="subtitle2" color="text.secondary" gutterBottom>Optimized Subject Lines</Typography>
              <List>
                {subjectSuggestions.map((suggestion, idx) => (
                  <ListItem
                    key={idx}
                    sx={{
                      border: 1,
                      borderColor: 'divider',
                      borderRadius: 1,
                      mb: 1,
                    }}
                    secondaryAction={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Chip
                          label={`${suggestion.score}%`}
                          size="small"
                          color={suggestion.score >= 80 ? 'success' : suggestion.score >= 60 ? 'warning' : 'default'}
                        />
                        <Tooltip title="Use this subject">
                          <IconButton
                            size="small"
                            color="primary"
                            onClick={() => onApplySubject?.(suggestion.subject)}
                          >
                            <CheckIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </Box>
                    }
                  >
                    <ListItemText
                      primary={suggestion.subject}
                      secondary={suggestion.reason}
                    />
                  </ListItem>
                ))}
              </List>
            </Box>
          )}

          {subjectTips.length > 0 && (
            <Paper variant="outlined" sx={{ p: 2 }}>
              <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                <TipIcon fontSize="small" sx={{ verticalAlign: 'middle', mr: 0.5 }} />
                Tips for Better Subject Lines
              </Typography>
              <List dense>
                {subjectTips.map((tip, idx) => (
                  <ListItem key={idx} sx={{ py: 0.5 }}>
                    <ListItemText primary={`• ${tip}`} />
                  </ListItem>
                ))}
              </List>
            </Paper>
          )}
        </TabPanel>

        {/* Improve Writing Tab */}
        <TabPanel value={activeTab} index={3}>
          <Box sx={{ textAlign: 'center', mb: 2 }}>
            <Button
              variant="contained"
              startIcon={loading ? <CircularProgress size={20} /> : <ImproveIcon />}
              onClick={improveEmail}
              disabled={loading || !emailContent.trim()}
            >
              Improve Email
            </Button>
          </Box>

          {emailScores && (
            <Grid container spacing={2} sx={{ mb: 3 }}>
              <Grid item xs={6}>
                <Paper variant="outlined" sx={{ p: 2 }}>
                  <Typography variant="subtitle2" color="text.secondary" gutterBottom>Original Scores</Typography>
                  {emailScores.original && (
                    <>
                      <Box sx={{ mb: 1 }}>
                        <Typography variant="body2">Clarity: {emailScores.original.clarity}%</Typography>
                        <LinearProgress variant="determinate" value={emailScores.original.clarity} sx={{ height: 6, borderRadius: 1 }} />
                      </Box>
                      <Box sx={{ mb: 1 }}>
                        <Typography variant="body2">Tone: {emailScores.original.tone}%</Typography>
                        <LinearProgress variant="determinate" value={emailScores.original.tone} sx={{ height: 6, borderRadius: 1 }} />
                      </Box>
                      <Box sx={{ mb: 1 }}>
                        <Typography variant="body2">Grammar: {emailScores.original.grammar}%</Typography>
                        <LinearProgress variant="determinate" value={emailScores.original.grammar} sx={{ height: 6, borderRadius: 1 }} />
                      </Box>
                      <Divider sx={{ my: 1 }} />
                      <Typography variant="body1" fontWeight="medium">
                        Overall: {emailScores.original.overall}%
                      </Typography>
                    </>
                  )}
                </Paper>
              </Grid>
              <Grid item xs={6}>
                <Paper variant="outlined" sx={{ p: 2, bgcolor: 'success.50' }}>
                  <Typography variant="subtitle2" color="text.secondary" gutterBottom>Improved Scores</Typography>
                  {emailScores.improved && (
                    <>
                      <Box sx={{ mb: 1 }}>
                        <Typography variant="body2">Clarity: {emailScores.improved.clarity}%</Typography>
                        <LinearProgress variant="determinate" value={emailScores.improved.clarity} color="success" sx={{ height: 6, borderRadius: 1 }} />
                      </Box>
                      <Box sx={{ mb: 1 }}>
                        <Typography variant="body2">Tone: {emailScores.improved.tone}%</Typography>
                        <LinearProgress variant="determinate" value={emailScores.improved.tone} color="success" sx={{ height: 6, borderRadius: 1 }} />
                      </Box>
                      <Box sx={{ mb: 1 }}>
                        <Typography variant="body2">Grammar: {emailScores.improved.grammar}%</Typography>
                        <LinearProgress variant="determinate" value={emailScores.improved.grammar} color="success" sx={{ height: 6, borderRadius: 1 }} />
                      </Box>
                      <Divider sx={{ my: 1 }} />
                      <Typography variant="body1" fontWeight="medium" color="success.main">
                        Overall: {emailScores.improved.overall}%
                      </Typography>
                    </>
                  )}
                </Paper>
              </Grid>
            </Grid>
          )}

          {improvementSummary && (
            <Alert severity="info" sx={{ mb: 2 }}>
              {improvementSummary}
            </Alert>
          )}

          {improvedEmail && (
            <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
                <Typography variant="subtitle2" color="text.secondary">Improved Email</Typography>
                <Box>
                  <Tooltip title="Copy">
                    <IconButton size="small" onClick={() => copyToClipboard(improvedEmail.body)}>
                      <CopyIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title="Use this version">
                    <IconButton
                      size="small"
                      color="primary"
                      onClick={() => onApplySuggestion?.({
                        subject: improvedEmail.subject,
                        body: improvedEmail.body,
                      })}
                    >
                      <CheckIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </Box>
              </Box>
              {improvedEmail.subject && (
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  <strong>Subject:</strong> {improvedEmail.subject}
                </Typography>
              )}
              <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap', bgcolor: 'grey.50', p: 2, borderRadius: 1 }}>
                {improvedEmail.body}
              </Typography>
            </Paper>
          )}

          {emailChanges.length > 0 && (
            <Paper variant="outlined" sx={{ p: 2 }}>
              <Typography variant="subtitle2" color="text.secondary" gutterBottom>Changes Made</Typography>
              <List dense>
                {emailChanges.map((change, idx) => (
                  <ListItem key={idx} sx={{ flexDirection: 'column', alignItems: 'flex-start' }}>
                    <Box sx={{ display: 'flex', width: '100%', gap: 1, alignItems: 'center' }}>
                      <Typography
                        variant="body2"
                        sx={{ textDecoration: 'line-through', color: 'text.secondary' }}
                      >
                        "{change.original}"
                      </Typography>
                      <Typography variant="body2">→</Typography>
                      <Typography variant="body2" color="success.main" fontWeight="medium">
                        "{change.improved}"
                      </Typography>
                    </Box>
                    {change.reason && (
                      <Typography variant="caption" color="text.secondary">
                        {change.reason}
                      </Typography>
                    )}
                  </ListItem>
                ))}
              </List>
            </Paper>
          )}
        </TabPanel>
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
};

export default EmailAIAssist;
