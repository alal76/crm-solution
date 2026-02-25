import React, { useState, useEffect, useCallback } from 'react';
import {
  Alert, Box, Button, Card, CardActionArea, CardContent, Chip,
  CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle,
  Stack, TextField, Typography
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import PushPinIcon from '@mui/icons-material/PushPin';
import VisibilityIcon from '@mui/icons-material/Visibility';
import apiClient from '../../services/apiClient';

interface ForumPost {
  id: number;
  title: string;
  body: string;
  authorName: string;
  isApproved: boolean;
  isPinned: boolean;
  viewCount: number;
  replyCount: number;
  createdAt: string;
}

const CommunityForumPage: React.FC = () => {
  const [posts, setPosts] = useState<ForumPost[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [newTitle, setNewTitle] = useState('');
  const [newBody, setNewBody] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  const loadPosts = useCallback(async () => {
    setLoading(true);
    try {
      const res = await apiClient.get<ForumPost[]>('/api/forum/posts');
      setPosts(res.data);
    } catch {
      setError('Failed to load forum posts.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadPosts(); }, [loadPosts]);

  const handleCreatePost = async () => {
    if (!newTitle.trim() || !newBody.trim()) {
      setSubmitError('Title and body are required.');
      return;
    }
    setSubmitting(true);
    setSubmitError(null);
    try {
      await apiClient.post('/api/forum/posts', { title: newTitle, body: newBody });
      setDialogOpen(false);
      setNewTitle('');
      setNewBody('');
      setSuccessMsg('Post submitted! It will appear after moderation.');
      setTimeout(() => setSuccessMsg(null), 4000);
      loadPosts();
    } catch {
      setSubmitError('Failed to create post. Please try again.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleClose = () => {
    setDialogOpen(false);
    setNewTitle('');
    setNewBody('');
    setSubmitError(null);
  };

  const pinned = posts.filter(p => p.isPinned && p.isApproved);
  const regular = posts.filter(p => !p.isPinned && p.isApproved);

  return (
    <Box p={3}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" mb={2}>
        <Box>
          <Typography variant="h5" fontWeight="bold">Community Forum</Typography>
          <Typography variant="body2" color="text.secondary">
            Ask questions, share tips, and connect with other users.
          </Typography>
        </Box>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setDialogOpen(true)}>
          New Post
        </Button>
      </Stack>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {successMsg && <Alert severity="success" sx={{ mb: 2 }}>{successMsg}</Alert>}

      {loading ? (
        <Box display="flex" justifyContent="center" mt={4}><CircularProgress /></Box>
      ) : (
        <>
          {pinned.length > 0 && (
            <>
              <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                📌 Pinned
              </Typography>
              <Stack spacing={1} mb={2}>
                {pinned.map(p => <PostCard key={p.id} post={p} />)}
              </Stack>
            </>
          )}
          <Typography variant="subtitle2" color="text.secondary" gutterBottom>
            Recent Posts
          </Typography>
          {regular.length === 0 ? (
            <Typography color="text.secondary" align="center" mt={4}>
              No posts yet. Be the first to start a discussion!
            </Typography>
          ) : (
            <Stack spacing={1}>
              {regular.map(p => <PostCard key={p.id} post={p} />)}
            </Stack>
          )}
        </>
      )}

      <Dialog open={dialogOpen} onClose={handleClose} maxWidth="sm" fullWidth>
        <DialogTitle>Start a New Discussion</DialogTitle>
        <DialogContent>
          <Stack spacing={2} mt={1}>
            {submitError && <Alert severity="error">{submitError}</Alert>}
            <TextField fullWidth size="small" label="Title *"
              value={newTitle} onChange={e => setNewTitle(e.target.value)}
              placeholder="What's your question or topic?" />
            <TextField fullWidth size="small" label="Body *" multiline rows={5}
              value={newBody} onChange={e => setNewBody(e.target.value)}
              placeholder="Describe your question or topic in detail…" />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose}>Cancel</Button>
          <Button variant="contained" onClick={handleCreatePost} disabled={submitting}>
            {submitting ? 'Posting…' : 'Post'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

const PostCard: React.FC<{ post: ForumPost }> = ({ post }) => (
  <Card variant="outlined">
    <CardActionArea>
      <CardContent sx={{ py: 1.5, '&:last-child': { pb: 1.5 } }}>
        <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
          <Box flex={1} mr={1}>
            <Stack direction="row" spacing={1} alignItems="center" mb={0.5}>
              {post.isPinned && <PushPinIcon fontSize="small" color="primary" />}
              <Typography variant="body1" fontWeight="medium">{post.title}</Typography>
            </Stack>
            <Typography variant="body2" color="text.secondary" noWrap>{post.body}</Typography>
            <Typography variant="caption" color="text.secondary" mt={0.5} display="block">
              by {post.authorName} · {new Date(post.createdAt).toLocaleDateString()}
            </Typography>
          </Box>
          <Stack direction="row" spacing={1} alignItems="center" flexShrink={0}>
            <Chip size="small" icon={<VisibilityIcon />} label={post.viewCount} />
            {post.replyCount > 0 && (
              <Chip size="small" label={`${post.replyCount} replies`} color="info" />
            )}
          </Stack>
        </Stack>
      </CardContent>
    </CardActionArea>
  </Card>
);

export default CommunityForumPage;
