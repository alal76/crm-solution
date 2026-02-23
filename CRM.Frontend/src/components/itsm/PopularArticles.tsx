// Popular Articles - Shows most viewed/popular KB articles
// Part of Knowledge Base Enhancement - Phase 3

import React from 'react';
import {
  Box,
  Typography,
  Paper,
  List,
  ListItemButton,
  ListItemText,
  Chip,
  Skeleton,
  Stack,
  Tooltip,
  ToggleButtonGroup,
  ToggleButton,
} from '@mui/material';
import {
  TrendingUp as TrendingIcon,
  Visibility as ViewIcon,
  ThumbUp as HelpfulIcon,
  EmojiEvents as TrophyIcon,
} from '@mui/icons-material';

export interface PopularArticleDto {
  id: number;
  title: string;
  viewCount: number;
  helpfulCount: number;
  category?: string;
}

export interface PopularArticlesProps {
  articles: PopularArticleDto[];
  onArticleClick: (articleId: number) => void;
  period?: 'week' | 'month' | 'all';
  onPeriodChange?: (period: 'week' | 'month' | 'all') => void;
  loading?: boolean;
}

const getRankColor = (rank: number): string => {
  if (rank === 1) return '#ffd700'; // gold
  if (rank === 2) return '#c0c0c0'; // silver
  if (rank === 3) return '#cd7f32'; // bronze
  return 'transparent';
};

const PopularArticles: React.FC<PopularArticlesProps> = ({
  articles,
  onArticleClick,
  period = 'month',
  onPeriodChange,
  loading = false,
}) => {
  if (loading) {
    return (
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2" gutterBottom>Popular Articles</Typography>
        <Stack spacing={1}>
          {[1, 2, 3, 4, 5].map((i) => (
            <Skeleton key={i} variant="rectangular" height={48} sx={{ borderRadius: 1 }} />
          ))}
        </Stack>
      </Paper>
    );
  }

  return (
    <Paper variant="outlined" sx={{ p: 1 }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ px: 1, py: 0.5 }}>
        <Typography variant="subtitle2" color="text.secondary">
          <TrendingIcon fontSize="small" sx={{ verticalAlign: 'middle', mr: 0.5 }} />
          Popular Articles
        </Typography>
        {onPeriodChange && (
          <ToggleButtonGroup
            value={period}
            exclusive
            onChange={(_, val) => val && onPeriodChange(val as 'week' | 'month' | 'all')}
            size="small"
          >
            <ToggleButton value="week" sx={{ py: 0, px: 1, fontSize: '0.65rem' }}>Week</ToggleButton>
            <ToggleButton value="month" sx={{ py: 0, px: 1, fontSize: '0.65rem' }}>Month</ToggleButton>
            <ToggleButton value="all" sx={{ py: 0, px: 1, fontSize: '0.65rem' }}>All</ToggleButton>
          </ToggleButtonGroup>
        )}
      </Stack>

      {articles.length === 0 ? (
        <Box sx={{ p: 3, textAlign: 'center' }}>
          <TrophyIcon sx={{ fontSize: 32, color: 'text.disabled', mb: 0.5 }} />
          <Typography variant="body2" color="text.secondary">
            No articles found for this period
          </Typography>
        </Box>
      ) : (
        <List dense disablePadding>
          {articles.map((article, index) => {
            const rank = index + 1;
            const rankBg = getRankColor(rank);

            return (
              <ListItemButton
                key={article.id}
                onClick={() => onArticleClick(article.id)}
                sx={{ borderRadius: 1, mb: 0.5 }}
              >
                <Box
                  sx={{
                    width: 24,
                    height: 24,
                    borderRadius: '50%',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    backgroundColor: rankBg || 'action.hover',
                    mr: 1.5,
                    flexShrink: 0,
                  }}
                >
                  <Typography
                    variant="caption"
                    fontWeight="bold"
                    color={rank <= 3 ? 'common.black' : 'text.secondary'}
                  >
                    {rank}
                  </Typography>
                </Box>
                <ListItemText
                  primary={article.title}
                  primaryTypographyProps={{ variant: 'body2', noWrap: true }}
                  secondary={article.category}
                  secondaryTypographyProps={{ variant: 'caption' }}
                />
                <Stack direction="row" spacing={1} alignItems="center" sx={{ ml: 1, flexShrink: 0 }}>
                  <Tooltip title={`${article.viewCount} views`}>
                    <Stack direction="row" alignItems="center" spacing={0.25}>
                      <ViewIcon sx={{ fontSize: 14, color: 'text.disabled' }} />
                      <Typography variant="caption" color="text.secondary">
                        {article.viewCount >= 1000
                          ? `${(article.viewCount / 1000).toFixed(1)}k`
                          : article.viewCount}
                      </Typography>
                    </Stack>
                  </Tooltip>
                  <Tooltip title={`${article.helpfulCount} found helpful`}>
                    <Stack direction="row" alignItems="center" spacing={0.25}>
                      <HelpfulIcon sx={{ fontSize: 14, color: 'text.disabled' }} />
                      <Typography variant="caption" color="text.secondary">
                        {article.helpfulCount}
                      </Typography>
                    </Stack>
                  </Tooltip>
                  {rank <= 3 && (
                    <Chip
                      icon={<TrendingIcon sx={{ fontSize: 12 }} />}
                      label="Hot"
                      size="small"
                      color="error"
                      variant="outlined"
                      sx={{ height: 18, fontSize: '0.6rem', '& .MuiChip-icon': { ml: 0.5 } }}
                    />
                  )}
                </Stack>
              </ListItemButton>
            );
          })}
        </List>
      )}
    </Paper>
  );
};

export default PopularArticles;
