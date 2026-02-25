import React, { useState } from 'react';
import {
  Alert, Box, Button, Card, CardContent, CircularProgress, Divider,
  IconButton, InputAdornment, Stack, TextField, Tooltip, Typography
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import ThumbUpIcon from '@mui/icons-material/ThumbUp';
import ThumbDownIcon from '@mui/icons-material/ThumbDown';
import apiClient from '../../services/apiClient';

interface KBArticle {
  id: number;
  title: string;
  summary: string;
  status: string;
  viewCount: number;
  helpfulCount: number;
}

interface FeedbackState {
  [articleId: number]: 'up' | 'down' | null;
}

const PortalKBSearchPage: React.FC = () => {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<KBArticle[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [searched, setSearched] = useState(false);
  const [feedback, setFeedback] = useState<FeedbackState>({});
  const [fbMessage, setFbMessage] = useState<string | null>(null);

  const handleSearch = async () => {
    if (!query.trim()) return;
    setLoading(true);
    setError(null);
    setSearched(true);
    try {
      const res = await apiClient.get<KBArticle[]>(`/api/portal/kb/search?q=${encodeURIComponent(query)}`);
      setResults(res.data);
    } catch {
      setError('Search failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const submitFeedback = async (articleId: number, isHelpful: boolean) => {
    if (feedback[articleId] !== undefined && feedback[articleId] !== null) return;
    try {
      await apiClient.post(`/api/portal/kb/${articleId}/feedback`, { isHelpful });
      setFeedback(prev => ({ ...prev, [articleId]: isHelpful ? 'up' : 'down' }));
      setFbMessage('Thank you for your feedback!');
      setTimeout(() => setFbMessage(null), 3000);
    } catch {
      setError('Failed to submit feedback.');
    }
  };

  return (
    <Box p={3} maxWidth={800} mx="auto">
      <Typography variant="h5" fontWeight="bold" gutterBottom>Knowledge Base Search</Typography>
      <Typography variant="body2" color="text.secondary" mb={3}>
        Search our knowledge base for articles, guides, and solutions.
      </Typography>

      <Stack direction="row" spacing={1} mb={2}>
        <TextField
          fullWidth size="small" placeholder="Search articles…"
          value={query} onChange={e => setQuery(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && handleSearch()}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start"><SearchIcon fontSize="small" /></InputAdornment>
            ),
          }}
        />
        <Button variant="contained" onClick={handleSearch} disabled={loading || !query.trim()}>
          Search
        </Button>
      </Stack>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {fbMessage && <Alert severity="success" sx={{ mb: 2 }}>{fbMessage}</Alert>}

      {loading && <Box display="flex" justifyContent="center" mt={4}><CircularProgress /></Box>}

      {!loading && searched && results.length === 0 && (
        <Typography color="text.secondary" align="center" mt={4}>
          No articles found for "{query}".
        </Typography>
      )}

      {!loading && results.length > 0 && (
        <>
          <Typography variant="body2" color="text.secondary" mb={1}>
            {results.length} result{results.length !== 1 ? 's' : ''} for "{query}"
          </Typography>
          <Stack spacing={2}>
            {results.map(a => (
              <Card key={a.id} variant="outlined">
                <CardContent>
                  <Typography variant="h6" gutterBottom>{a.title}</Typography>
                  <Typography variant="body2" color="text.secondary" mb={1}>{a.summary}</Typography>
                  <Divider sx={{ mb: 1 }} />
                  <Stack direction="row" justifyContent="space-between" alignItems="center">
                    <Typography variant="caption" color="text.secondary">
                      👁 {a.viewCount} views · 👍 {a.helpfulCount} helpful
                    </Typography>
                    <Stack direction="row" spacing={0.5} alignItems="center">
                      <Typography variant="caption">Was this helpful?</Typography>
                      <Tooltip title="Yes, helpful">
                        <IconButton size="small" color={feedback[a.id] === 'up' ? 'success' : 'default'}
                          onClick={() => submitFeedback(a.id, true)}
                          disabled={feedback[a.id] !== undefined && feedback[a.id] !== null}>
                          <ThumbUpIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Not helpful">
                        <IconButton size="small" color={feedback[a.id] === 'down' ? 'error' : 'default'}
                          onClick={() => submitFeedback(a.id, false)}
                          disabled={feedback[a.id] !== undefined && feedback[a.id] !== null}>
                          <ThumbDownIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </Stack>
                  </Stack>
                </CardContent>
              </Card>
            ))}
          </Stack>
        </>
      )}
    </Box>
  );
};

export default PortalKBSearchPage;
