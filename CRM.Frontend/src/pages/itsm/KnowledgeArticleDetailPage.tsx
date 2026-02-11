import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import apiClient from '../../services/apiClient';
import { ArticleFeedbackWidget } from '../../components/itsm';

interface Article {
  articleId: number;
  number: string;
  title: string;
  articleBody: string;
  authorName: string;
  publishedDate: string;
  viewCount: number;
  helpfulCount: number;
}

export const KnowledgeArticleDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [article, setArticle] = useState<Article | null>(null);
  const [loading, setLoading] = useState(true);
  const [feedbackGiven, setFeedbackGiven] = useState(false);

  useEffect(() => {
    const loadArticle = async () => {
      try {
        const response = await apiClient.get(`/api/knowledge/${id}`);
        setArticle(response.data);
      } catch (error) {
        console.error('Failed to load article', error);
      } finally {
        setLoading(false);
      }
    };

    loadArticle();
  }, [id]);

  const handleFeedback = async (helpful: boolean) => {
    try {
      await apiClient.post(`/api/knowledge/${id}/feedback`, { helpful });
      setFeedbackGiven(true);
    } catch (error) {
      console.error('Failed to submit feedback', error);
    }
  };

  if (loading) return <Box sx={{ p: 3, display: 'flex', justifyContent: 'center' }}><CircularProgress /></Box>;
  if (!article) return <Box sx={{ p: 3 }}><Typography color="text.secondary">Article not found</Typography></Box>;

  return (
    <Box sx={{ p: 3, maxWidth: 900, mx: 'auto' }}>
      <Paper sx={{ p: 4 }}>
        <Typography variant="h4" component="h1" fontWeight="bold" gutterBottom>{article.title}</Typography>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3, pb: 3, borderBottom: 1, borderColor: 'divider' }}>
          <Box>
            <Typography variant="body2" color="text.secondary">By {article.authorName}</Typography>
            <Typography variant="body2" color="text.secondary">{article.number} • Published {new Date(article.publishedDate).toLocaleDateString()}</Typography>
          </Box>
          <Typography variant="body2" color="text.secondary">{article.viewCount} views</Typography>
        </Box>

        <Box sx={{ mb: 4 }}>
          <Typography sx={{ whiteSpace: 'pre-wrap' }}>{article.articleBody}</Typography>
        </Box>

        <Paper variant="outlined" sx={{ p: 3, bgcolor: 'grey.50' }}>
          <Typography variant="subtitle2" gutterBottom>Was this article helpful?</Typography>
          <Box sx={{ display: 'flex', gap: 1.5 }}>
            <Button variant="contained" color="success" disabled={feedbackGiven} onClick={() => handleFeedback(true)}>👍 Yes</Button>
            <Button variant="contained" color="error" disabled={feedbackGiven} onClick={() => handleFeedback(false)}>👎 No</Button>
          </Box>
          {feedbackGiven && <Typography variant="body2" color="text.secondary" sx={{ mt: 1.5 }}>Thank you for your feedback!</Typography>}
        </Paper>

        {/* Enhanced Article Feedback Widget */}
        <Box sx={{ mt: 3 }}>
          <ArticleFeedbackWidget
            articleId={Number(id)}
            showStats
            showRating
            onSubmitFeedback={async (feedback) => {
              await apiClient.post(`/api/knowledge/${id}/feedback`, feedback);
              setFeedbackGiven(true);
            }}
          />
        </Box>
      </Paper>
    </Box>
  );
};

export default KnowledgeArticleDetailPage;
