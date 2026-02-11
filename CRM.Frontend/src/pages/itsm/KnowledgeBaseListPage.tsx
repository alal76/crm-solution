import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import TextField from '@mui/material/TextField';
import CircularProgress from '@mui/material/CircularProgress';
import apiClient from '../../services/apiClient';

interface Article {
  articleId: number;
  number: string;
  title: string;
  shortDescription: string;
  viewCount: number;
  helpfulCount: number;
  publishedDate: string;
}

export const KnowledgeBaseListPage: React.FC = () => {
  const navigate = useNavigate();
  const [articles, setArticles] = useState<Article[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    const loadArticles = async () => {
      setLoading(true);
      try {
        const params = new URLSearchParams({
          searchTerm: searchTerm,
          pageNumber: '1',
          pageSize: '20'
        });
        const response = await apiClient.get(`/api/knowledge/search?${params}`);
        setArticles(response.data ?? []);
      } catch (error) {
        console.error('Failed to load articles', error);
      } finally {
        setLoading(false);
      }
    };

    loadArticles();
  }, [searchTerm]);

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" fontWeight="bold" gutterBottom>Knowledge Base</Typography>

      <Box sx={{ mb: 4 }}>
        <TextField
          placeholder="Search knowledge articles..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          fullWidth
          sx={{ maxWidth: 600 }}
        />
      </Box>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>
      ) : articles.length === 0 ? (
        <Typography color="text.secondary">No articles found.</Typography>
      ) : (
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          {articles.map((article) => (
            <Paper
              key={article.articleId}
              onClick={() => navigate(`/knowledge/${article.articleId}`)}
              sx={{ p: 3, cursor: 'pointer', '&:hover': { boxShadow: 3 }, transition: 'box-shadow 0.2s', borderLeft: 4, borderColor: 'primary.main' }}
            >
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <Box sx={{ flex: 1 }}>
                  <Typography variant="h6" fontWeight="bold" gutterBottom>{article.title}</Typography>
                  <Typography color="text.secondary" sx={{ mb: 1.5 }}>{article.shortDescription}</Typography>
                  <Typography variant="body2" color="text.secondary">{article.number}</Typography>
                </Box>
                <Box sx={{ ml: 2, textAlign: 'right' }}>
                  <Typography variant="h5" fontWeight="bold" color="success.main">{article.helpfulCount}</Typography>
                  <Typography variant="caption" color="text.secondary">helpful</Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>{article.viewCount} views</Typography>
                </Box>
              </Box>
            </Paper>
          ))}
        </Box>
      )}
    </Box>
  );
};

export default KnowledgeBaseListPage;
