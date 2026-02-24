import React, { useEffect, useState } from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Alert from '@mui/material/Alert';
import apiClient from '../../services/apiClient';

interface KnowledgeApprovalItem {
  articleId: number;
  number: string;
  title: string;
  shortDescription?: string;
  publishingState: number;
  authorName?: string;
  publishedDate?: string;
}

const KnowledgeArticleApprovalPage: React.FC = () => {
  const [items, setItems] = useState<KnowledgeApprovalItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [publishingId, setPublishingId] = useState<number | null>(null);
  const [rejectingId, setRejectingId] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await apiClient.get('/knowledge/pending');
        setItems(response.data ?? []);
      } catch (loadError) {
        console.error('Failed to load pending articles', loadError);
        setError('Unable to load approval queue.');
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  const handlePublish = async (articleId: number) => {
    setPublishingId(articleId);
    setError(null);

    try {
      await apiClient.patch(`/knowledge/${articleId}/publish`);
      setItems((prev) => prev.filter((item) => item.articleId !== articleId));
    } catch (publishError) {
      console.error('Failed to publish article', publishError);
      setError('Unable to publish article.');
    } finally {
      setPublishingId(null);
    }
  };

  const handleReject = async (articleId: number) => {
    setRejectingId(articleId);
    setError(null);

    try {
      await apiClient.patch(`/knowledge/${articleId}/retire`);
      setItems((prev) => prev.filter((item) => item.articleId !== articleId));
    } catch (rejectError) {
      console.error('Failed to reject article', rejectError);
      setError('Unable to reject article.');
    } finally {
      setRejectingId(null);
    }
  };

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" fontWeight="bold" gutterBottom>Knowledge Article Approvals</Typography>
      <Paper sx={{ p: 3 }}>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>
        ) : items.length === 0 ? (
          <Typography color="text.secondary">No articles awaiting approval.</Typography>
        ) : (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            {items.map((item) => (
              <Paper key={item.articleId} variant="outlined" sx={{ p: 2 }}>
                <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', gap: 2 }}>
                  <Box>
                    <Typography variant="body2" color="text.secondary">{item.number}</Typography>
                    <Typography variant="h6" fontWeight="bold">{item.title}</Typography>
                    {item.shortDescription && (
                      <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>{item.shortDescription}</Typography>
                    )}
                    <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>Draft state {item.publishingState}</Typography>
                  </Box>
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    <Button
                      variant="contained"
                      color="error"
                      onClick={() => handleReject(item.articleId)}
                      disabled={rejectingId === item.articleId || publishingId === item.articleId}
                    >
                      {rejectingId === item.articleId ? 'Rejecting...' : 'Reject'}
                    </Button>
                    <Button
                      variant="contained"
                      onClick={() => handlePublish(item.articleId)}
                      disabled={publishingId === item.articleId || rejectingId === item.articleId}
                    >
                      {publishingId === item.articleId ? 'Publishing...' : 'Publish'}
                    </Button>
                  </Box>
                </Box>
              </Paper>
            ))}
          </Box>
        )}
        {error && <Alert severity="error" sx={{ mt: 2 }}>{error}</Alert>}
      </Paper>
    </Box>
  );
};

export default KnowledgeArticleApprovalPage;
