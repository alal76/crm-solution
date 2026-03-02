// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
import React, { useState, useEffect, useCallback } from 'react';
import {
  AppBar,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Divider,
  IconButton,
  TextField,
  Toolbar,
  Typography,
  Alert,
  List,
  ListItem,
  ListItemText,
} from '@mui/material';
import { ArrowBack, ExitToApp, SupportAgent, Send, AttachFile } from '@mui/icons-material';
import { useNavigate, useParams, Link } from 'react-router-dom';
import {
  portalAuthService,
  portalService,
  type PortalTicketDto,
  type PortalCommentDto,
  type PortalConfigDto,
  type PortalAttachmentDto,
} from '../../services/portalService';

const statusColor = (status: string) => {
  switch (status.toLowerCase()) {
    case 'new': return 'info';
    case 'open': return 'primary';
    case 'resolved': case 'closed': return 'success';
    case 'on hold': return 'warning';
    case 'cancelled': return 'default';
    default: return 'default';
  }
};

const priorityColor = (priority: string) => {
  switch (priority.toLowerCase()) {
    case 'critical': return 'error';
    case 'high': return 'warning';
    case 'medium': return 'info';
    default: return 'default';
  }
};

const PortalTicketDetailPage: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const ticketId = Number.parseInt(id ?? '0', 10);

  const [ticket, setTicket] = useState<PortalTicketDto | null>(null);
  const [comments, setComments] = useState<PortalCommentDto[]>([]);
  const [attachments, setAttachments] = useState<PortalAttachmentDto[]>([]);
  const [config, setConfig] = useState<PortalConfigDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [comment, setComment] = useState('');
  const [commentLoading, setCommentLoading] = useState(false);
  const [commentError, setCommentError] = useState<string | null>(null);
  const [cancelLoading, setCancelLoading] = useState(false);

  const user = portalAuthService.getCurrentUser();

  const loadData = useCallback(async () => {
    if (!ticketId) return;
    setLoading(true);
    setError(null);
    try {
      const [t, c, cfg] = await Promise.all([
        portalService.getTicket(ticketId),
        portalService.getTicketComments(ticketId),
        portalService.getConfig(),
      ]);
      setTicket(t);
      setComments(c);
      setConfig(cfg);
      // Attachments are optional (endpoint may not exist yet)
      try {
        const att = await portalService.getAttachments(ticketId);
        setAttachments(att);
      } catch {
        setAttachments([]);
      }
    } catch {
      setError('Failed to load ticket details.');
    } finally {
      setLoading(false);
    }
  }, [ticketId]);

  useEffect(() => {
    if (!portalAuthService.isAuthenticated()) {
      navigate('/portal/login', { replace: true });
      return;
    }
    loadData();
  }, [navigate, loadData]);

  const handleLogout = () => {
    portalAuthService.logout();
    navigate('/portal/login', { replace: true });
  };

  const handleAddComment = async () => {
    if (!comment.trim()) return;
    setCommentLoading(true);
    setCommentError(null);
    try {
      const newComment = await portalService.addComment(ticketId, comment);
      setComments((prev) => [...prev, newComment]);
      setComment('');
    } catch (err: any) {
      setCommentError(err?.response?.data?.message ?? 'Failed to add comment.');
    } finally {
      setCommentLoading(false);
    }
  };

  const handleCancel = async () => {
    if (!window.confirm('Are you sure you want to cancel this ticket?')) return;
    setCancelLoading(true);
    try {
      await portalService.cancelTicket(ticketId);
      await loadData();
    } catch (err: any) {
      setError(err?.response?.data?.message ?? 'Failed to cancel ticket.');
    } finally {
      setCancelLoading(false);
    }
  };

  const brandColor = config?.primaryColor ?? '#1976d2';
  const canCancel = ticket && ['new', 'open', 'pending'].includes(ticket.status.toLowerCase());

  if (loading) {
    return (
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', minHeight: '100vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <AppBar position="static" sx={{ bgcolor: brandColor }}>
        <Toolbar>
          <IconButton color="inherit" component={Link} to="/portal/tickets" sx={{ mr: 1 }}>
            <ArrowBack />
          </IconButton>
          <SupportAgent sx={{ mr: 1 }} />
          <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: 700 }}>
            Ticket Detail
          </Typography>
          <Typography variant="body2" sx={{ mr: 2 }}>{user?.displayName ?? user?.email}</Typography>
          <IconButton color="inherit" onClick={handleLogout} title="Sign out">
            <ExitToApp />
          </IconButton>
        </Toolbar>
      </AppBar>

      <Box sx={{ p: 3, maxWidth: 800, mx: 'auto' }}>
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        {!ticket ? (
          <Alert severity="warning">Ticket not found.</Alert>
        ) : (
          <>
            {/* Ticket Header */}
            <Card sx={{ mb: 3 }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', mb: 2 }}>
                  <Box>
                    <Typography variant="caption" color="text.secondary">{ticket.ticketNumber}</Typography>
                    <Typography variant="h6" fontWeight={700} mt={0.5}>
                      {ticket.title}
                    </Typography>
                  </Box>
                  <Box sx={{ display: 'flex', gap: 1, flexShrink: 0, ml: 2 }}>
                    <Chip
                      label={ticket.priority}
                      size="small"
                      color={priorityColor(ticket.priority) as any}
                    />
                    <Chip
                      label={ticket.status}
                      size="small"
                      color={statusColor(ticket.status) as any}
                    />
                  </Box>
                </Box>

                <Divider sx={{ mb: 2 }} />

                <Box sx={{ display: 'flex', gap: 4, flexWrap: 'wrap', mb: 2 }}>
                  <Box>
                    <Typography variant="caption" color="text.secondary">Created</Typography>
                    <Typography variant="body2">{new Date(ticket.createdAt).toLocaleString()}</Typography>
                  </Box>
                  <Box>
                    <Typography variant="caption" color="text.secondary">Last Updated</Typography>
                    <Typography variant="body2">{new Date(ticket.updatedAt).toLocaleString()}</Typography>
                  </Box>
                </Box>

                {ticket.description && (
                  <Box>
                    <Typography variant="subtitle2" fontWeight={700} gutterBottom>Description</Typography>
                    <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>{ticket.description}</Typography>
                  </Box>
                )}

                {canCancel && (
                  <Box sx={{ mt: 2 }}>
                    <Button
                      variant="outlined"
                      color="error"
                      size="small"
                      onClick={handleCancel}
                      disabled={cancelLoading}
                    >
                      {cancelLoading ? <CircularProgress size={16} /> : 'Cancel Ticket'}
                    </Button>
                  </Box>
                )}
              </CardContent>
            </Card>

            {/* Attachments */}
            {attachments.length > 0 && (
              <Card sx={{ mb: 3 }}>
                <CardContent>
                  <Typography variant="subtitle2" fontWeight={700} gutterBottom>
                    <AttachFile fontSize="small" sx={{ verticalAlign: 'middle', mr: 0.5 }} />
                    Attachments
                  </Typography>
                  <List dense>
                    {attachments.map((att) => (
                      <ListItem key={att.id} disableGutters>
                        <ListItemText
                          primary={att.fileName}
                          secondary={`${(att.fileSize / 1024).toFixed(1)} KB · ${new Date(att.uploadedAt).toLocaleDateString()}`}
                        />
                        {att.downloadUrl && (
                          <Button size="small" href={att.downloadUrl} target="_blank" rel="noopener noreferrer">
                            Download
                          </Button>
                        )}
                      </ListItem>
                    ))}
                  </List>
                </CardContent>
              </Card>
            )}

            {/* Comments */}
            <Card>
              <CardContent>
                <Typography variant="subtitle2" fontWeight={700} mb={2}>
                  Conversation ({comments.length})
                </Typography>

                {comments.length === 0 ? (
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                    No replies yet.
                  </Typography>
                ) : (
                  comments.map((c, idx) => (
                    <React.Fragment key={c.id}>
                      {idx > 0 && <Divider sx={{ my: 1.5 }} />}
                      <Box sx={{
                        p: 1.5,
                        borderRadius: 1,
                        bgcolor: c.isStaff ? 'primary.50' : 'grey.100',
                        border: c.isStaff ? '1px solid' : 'none',
                        borderColor: 'primary.200',
                      }}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                          <Typography variant="caption" fontWeight={700}>
                            {c.authorName}
                            {c.isStaff && (
                              <Chip label="Support" size="small" color="primary" sx={{ ml: 1, height: 16, fontSize: '0.65rem' }} />
                            )}
                          </Typography>
                          <Typography variant="caption" color="text.secondary">
                            {new Date(c.createdAt).toLocaleString()}
                          </Typography>
                        </Box>
                        <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>{c.content}</Typography>
                      </Box>
                    </React.Fragment>
                  ))
                )}

                {/* Add Reply */}
                {ticket && !['resolved', 'closed', 'cancelled'].includes(ticket.status.toLowerCase()) && (
                  <Box sx={{ mt: 2 }}>
                    <Divider sx={{ mb: 2 }} />
                    {commentError && <Alert severity="error" sx={{ mb: 1 }}>{commentError}</Alert>}
                    <TextField
                      fullWidth
                      multiline
                      rows={3}
                      placeholder="Write a reply..."
                      value={comment}
                      onChange={(e) => setComment(e.target.value)}
                      sx={{ mb: 1 }}
                    />
                    <Button
                      variant="contained"
                      endIcon={commentLoading ? <CircularProgress size={16} color="inherit" /> : <Send />}
                      disabled={!comment.trim() || commentLoading}
                      onClick={handleAddComment}
                      sx={{ bgcolor: brandColor }}
                    >
                      Send Reply
                    </Button>
                  </Box>
                )}
              </CardContent>
            </Card>
          </>
        )}
      </Box>
    </Box>
  );
};

export default PortalTicketDetailPage;
