// Related Articles - Shows related KB articles with relevance indicators
// Part of Knowledge Base Enhancement - Phase 3

import React from 'react';
import {
  Box,
  Typography,
  Paper,
  List,
  ListItemButton,
  ListItemText,
  ListItemIcon,
  Chip,
  Skeleton,
  Stack,
  Tooltip,
  LinearProgress,
} from '@mui/material';
import {
  Article as ArticleIcon,
  Visibility as ViewIcon,
  Link as LinkIcon,
} from '@mui/icons-material';

export interface RelatedArticleDto {
  id: number;
  title: string;
  category?: string;
  relevanceScore?: number;
  viewCount?: number;
}

export interface RelatedArticlesProps {
  articles: RelatedArticleDto[];
  onArticleClick: (articleId: number) => void;
  maxDisplay?: number;
  loading?: boolean;
}

const getRelevanceColor = (score: number): 'success' | 'warning' | 'info' => {
  if (score >= 75) return 'success';
  if (score >= 50) return 'warning';
  return 'info';
};

const RelatedArticles: React.FC<RelatedArticlesProps> = ({
  articles,
  onArticleClick,
  maxDisplay = 5,
  loading = false,
}) => {
  const displayArticles = articles.slice(0, maxDisplay);

  if (loading) {
    return (
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Related Articles</Typography>
        <Stack spacing={1}>
          {[1, 2, 3].map((i) => (
            <Skeleton key={i} variant="rectangular" height={48} sx={{ borderRadius: 1 }} />
          ))}
        </Stack>
      </Paper>
    );
  }

  if (articles.length === 0) {
    return (
      <Paper variant="outlined" sx={{ p: 2, textAlign: 'center' }}>
        <LinkIcon sx={{ fontSize: 32, color: 'text.disabled', mb: 0.5 }} />
        <Typography variant="body2" color="text.secondary">
          No related articles found
        </Typography>
      </Paper>
    );
  }

  return (
    <Paper variant="outlined" sx={{ p: 1 }}>
      <Typography variant="subtitle2" sx={{ px: 1, py: 0.5, color: 'text.secondary' }}>
        Related Articles
      </Typography>
      <List dense disablePadding>
        {displayArticles.map((article) => (
          <ListItemButton
            key={article.id}
            onClick={() => onArticleClick(article.id)}
            sx={{ borderRadius: 1, mb: 0.5 }}
          >
            <ListItemIcon sx={{ minWidth: 32 }}>
              <ArticleIcon fontSize="small" color="action" />
            </ListItemIcon>
            <ListItemText
              primary={article.title}
              primaryTypographyProps={{ variant: 'body2', noWrap: true }}
            />
            <Stack direction="row" spacing={0.5} alignItems="center" sx={{ ml: 1, flexShrink: 0 }}>
              {article.category && (
                <Chip label={article.category} size="small" variant="outlined" sx={{ height: 20, fontSize: '0.65rem' }} />
              )}
              {article.relevanceScore !== undefined && (
                <Tooltip title={`Relevance: ${article.relevanceScore}%`}>
                  <Box sx={{ width: 40 }}>
                    <LinearProgress
                      variant="determinate"
                      value={article.relevanceScore}
                      color={getRelevanceColor(article.relevanceScore)}
                      sx={{ height: 6, borderRadius: 3 }}
                    />
                  </Box>
                </Tooltip>
              )}
              {article.viewCount !== undefined && (
                <Tooltip title={`${article.viewCount} views`}>
                  <Stack direction="row" alignItems="center" spacing={0.25}>
                    <ViewIcon sx={{ fontSize: 14, color: 'text.disabled' }} />
                    <Typography variant="caption" color="text.secondary">
                      {article.viewCount}
                    </Typography>
                  </Stack>
                </Tooltip>
              )}
            </Stack>
          </ListItemButton>
        ))}
      </List>
      {articles.length > maxDisplay && (
        <Typography variant="caption" color="text.secondary" sx={{ px: 1, display: 'block', textAlign: 'center', py: 0.5 }}>
          +{articles.length - maxDisplay} more
        </Typography>
      )}
    </Paper>
  );
};

export default RelatedArticles;
