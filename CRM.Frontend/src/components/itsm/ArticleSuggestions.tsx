// Article Suggestions - Real-time KB suggestions in incident form
// Part of ITSM Enhancement Plan - Phase 3.1

import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
  Box,
  Typography,
  Paper,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  ListItemSecondaryAction,
  IconButton,
  Chip,
  Stack,
  TextField,
  InputAdornment,
  Button,
  Tooltip,
  CircularProgress,
  Collapse,
  Divider,
  Alert,
} from '@mui/material';
import {
  Article as ArticleIcon,
  Search as SearchIcon,
  Lightbulb as SuggestionIcon,
  CheckCircle as UsedIcon,
  OpenInNew as OpenIcon,
  ThumbUp as HelpfulIcon,
  AutoAwesome as AIIcon,
  ExpandMore as ExpandIcon,
  ExpandLess as CollapseIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';

export type ArticleType = 'how_to' | 'troubleshooting' | 'faq' | 'known_error' | 'reference';

export interface SuggestedArticle {
  id: number;
  number: string;
  title: string;
  shortDescription: string;
  articleType: ArticleType;
  viewCount: number;
  helpfulCount: number;
  matchScore: number; // 0-100, how relevant the article is
  attachedToIncidentCount: number;
  isKnownError?: boolean;
  relatedProblemId?: number;
}

export interface ArticleSuggestionsProps {
  incidentDescription?: string;
  categoryId?: number;
  subcategoryId?: number;
  onArticleSelect?: (article: SuggestedArticle) => void;
  onArticleAttach?: (articleId: number) => Promise<void>;
  fetchSuggestions?: (params: {
    description?: string;
    categoryId?: number;
  }) => Promise<SuggestedArticle[]>;
  searchArticles?: (query: string) => Promise<SuggestedArticle[]>;
  attachedArticleIds?: number[];
  autoSuggest?: boolean;
  maxSuggestions?: number;
}

const getArticleTypeColor = (type: ArticleType): string => {
  switch (type) {
    case 'how_to':
      return '#4caf50';
    case 'troubleshooting':
      return '#ff9800';
    case 'faq':
      return '#2196f3';
    case 'known_error':
      return '#f44336';
    case 'reference':
      return '#9c27b0';
    default:
      return '#757575';
  }
};

const getArticleTypeLabel = (type: ArticleType): string => {
  switch (type) {
    case 'how_to':
      return 'How-To';
    case 'troubleshooting':
      return 'Troubleshooting';
    case 'faq':
      return 'FAQ';
    case 'known_error':
      return 'Known Error';
    case 'reference':
      return 'Reference';
    default:
      return type;
  }
};

const getMatchScoreColor = (score: number): string => {
  if (score >= 80) return '#4caf50';
  if (score >= 60) return '#ff9800';
  if (score >= 40) return '#ffc107';
  return '#9e9e9e';
};

interface ArticleCardProps {
  article: SuggestedArticle;
  isAttached?: boolean;
  onSelect?: (article: SuggestedArticle) => void;
  onAttach?: (articleId: number) => void;
}

const ArticleCard: React.FC<ArticleCardProps> = ({
  article,
  isAttached = false,
  onSelect,
  onAttach,
}) => {
  return (
    <Paper
      variant="outlined"
      sx={{
        p: 1.5,
        mb: 1,
        cursor: 'pointer',
        borderColor: isAttached ? '#4caf50' : undefined,
        backgroundColor: isAttached ? '#4caf5008' : undefined,
        '&:hover': {
          borderColor: 'primary.main',
          backgroundColor: 'action.hover',
        },
      }}
      onClick={() => onSelect?.(article)}
    >
      <Stack direction="row" alignItems="flex-start" spacing={1}>
        <Box sx={{ pt: 0.5 }}>
          {article.isKnownError ? (
            <Tooltip title="Known Error - Has workaround">
              <SuggestionIcon sx={{ color: '#f44336' }} />
            </Tooltip>
          ) : (
            <ArticleIcon color="action" />
          )}
        </Box>

        <Box sx={{ flexGrow: 1, minWidth: 0 }}>
          <Stack direction="row" alignItems="center" spacing={1} flexWrap="wrap">
            <Typography variant="body2" fontWeight={600} noWrap sx={{ maxWidth: 200 }}>
              {article.number}
            </Typography>
            <Chip
              label={getArticleTypeLabel(article.articleType)}
              size="small"
              sx={{
                height: 18,
                fontSize: '0.65rem',
                backgroundColor: `${getArticleTypeColor(article.articleType)}20`,
                color: getArticleTypeColor(article.articleType),
              }}
            />
            {article.matchScore >= 70 && (
              <Tooltip title={`${article.matchScore}% match to incident`}>
                <Chip
                  icon={<AIIcon sx={{ fontSize: 12 }} />}
                  label={`${article.matchScore}%`}
                  size="small"
                  sx={{
                    height: 18,
                    fontSize: '0.65rem',
                    backgroundColor: `${getMatchScoreColor(article.matchScore)}20`,
                    color: getMatchScoreColor(article.matchScore),
                  }}
                />
              </Tooltip>
            )}
            {isAttached && (
              <Chip
                icon={<UsedIcon sx={{ fontSize: 12 }} />}
                label="Attached"
                size="small"
                color="success"
                sx={{ height: 18, fontSize: '0.65rem' }}
              />
            )}
          </Stack>

          <Typography variant="body2" sx={{ mt: 0.5 }} noWrap>
            {article.title}
          </Typography>

          <Typography variant="caption" color="text.secondary" noWrap>
            {article.shortDescription}
          </Typography>

          <Stack direction="row" spacing={2} sx={{ mt: 0.5 }}>
            <Typography variant="caption" color="text.secondary">
              👁 {article.viewCount} views
            </Typography>
            <Typography variant="caption" color="text.secondary">
              👍 {article.helpfulCount} helpful
            </Typography>
            {article.attachedToIncidentCount > 0 && (
              <Typography variant="caption" color="text.secondary">
                🔗 Used in {article.attachedToIncidentCount} incidents
              </Typography>
            )}
          </Stack>
        </Box>

        <Stack direction="row" spacing={0.5}>
          <Tooltip title="View Article">
            <IconButton size="small" onClick={(e) => { e.stopPropagation(); onSelect?.(article); }}>
              <OpenIcon fontSize="small" />
            </IconButton>
          </Tooltip>
          {!isAttached && onAttach && (
            <Tooltip title="Attach to Incident">
              <IconButton
                size="small"
                color="primary"
                onClick={(e) => { e.stopPropagation(); onAttach(article.id); }}
              >
                <UsedIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </Stack>
      </Stack>
    </Paper>
  );
};

export const ArticleSuggestions: React.FC<ArticleSuggestionsProps> = ({
  incidentDescription,
  categoryId,
  subcategoryId,
  onArticleSelect,
  onArticleAttach,
  fetchSuggestions,
  searchArticles,
  attachedArticleIds = [],
  autoSuggest = true,
  maxSuggestions = 5,
}) => {
  const [suggestions, setSuggestions] = useState<SuggestedArticle[]>([]);
  const [searchResults, setSearchResults] = useState<SuggestedArticle[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [expanded, setExpanded] = useState(true);
  const [showSearch, setShowSearch] = useState(false);

  // Fetch suggestions when description changes
  const loadSuggestions = useCallback(async () => {
    if (!fetchSuggestions || !autoSuggest) return;
    if (!incidentDescription && !categoryId) return;

    setLoading(true);
    try {
      const results = await fetchSuggestions({
        description: incidentDescription,
        categoryId,
      });
      setSuggestions(results.slice(0, maxSuggestions));
    } catch (error) {
      console.error('Failed to fetch suggestions:', error);
    } finally {
      setLoading(false);
    }
  }, [fetchSuggestions, incidentDescription, categoryId, autoSuggest, maxSuggestions]);

  useEffect(() => {
    const debounceTimer = setTimeout(() => {
      loadSuggestions();
    }, 500);

    return () => clearTimeout(debounceTimer);
  }, [loadSuggestions]);

  const handleSearch = async () => {
    if (!searchArticles || !searchQuery.trim()) return;

    setLoading(true);
    try {
      const results = await searchArticles(searchQuery);
      setSearchResults(results);
    } catch (error) {
      console.error('Search failed:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleAttach = async (articleId: number) => {
    await onArticleAttach?.(articleId);
  };

  // Prioritize known errors
  const sortedSuggestions = useMemo(() => {
    return [...suggestions].sort((a, b) => {
      // Known errors first
      if (a.isKnownError && !b.isKnownError) return -1;
      if (!a.isKnownError && b.isKnownError) return 1;
      // Then by match score
      return b.matchScore - a.matchScore;
    });
  }, [suggestions]);

  const knownErrorCount = suggestions.filter((s) => s.isKnownError).length;

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      {/* Header */}
      <Stack direction="row" alignItems="center" justifyContent="space-between">
        <Stack direction="row" alignItems="center" spacing={1}>
          <SuggestionIcon color="primary" />
          <Typography variant="subtitle1" fontWeight={600}>
            Knowledge Suggestions
          </Typography>
          {knownErrorCount > 0 && (
            <Chip
              label={`${knownErrorCount} Known Error${knownErrorCount > 1 ? 's' : ''}`}
              size="small"
              color="error"
              variant="outlined"
            />
          )}
        </Stack>
        <Stack direction="row" spacing={0.5}>
          <Tooltip title="Refresh Suggestions">
            <IconButton size="small" onClick={loadSuggestions} disabled={loading}>
              <RefreshIcon fontSize="small" />
            </IconButton>
          </Tooltip>
          <IconButton size="small" onClick={() => setExpanded(!expanded)}>
            {expanded ? <CollapseIcon /> : <ExpandIcon />}
          </IconButton>
        </Stack>
      </Stack>

      <Collapse in={expanded}>
        {/* AI Notice */}
        {autoSuggest && incidentDescription && (
          <Alert
            severity="info"
            icon={<AIIcon />}
            sx={{ mt: 1, mb: 2, py: 0 }}
          >
            <Typography variant="caption">
              Suggestions based on incident description and category
            </Typography>
          </Alert>
        )}

        {/* Search Toggle */}
        <Button
          size="small"
          startIcon={<SearchIcon />}
          onClick={() => setShowSearch(!showSearch)}
          sx={{ mb: 1 }}
        >
          {showSearch ? 'Hide Search' : 'Search Knowledge Base'}
        </Button>

        <Collapse in={showSearch}>
          <Stack direction="row" spacing={1} sx={{ mb: 2 }}>
            <TextField
              size="small"
              fullWidth
              placeholder="Search for articles..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon fontSize="small" />
                  </InputAdornment>
                ),
              }}
            />
            <Button
              variant="contained"
              size="small"
              onClick={handleSearch}
              disabled={loading || !searchQuery.trim()}
            >
              Search
            </Button>
          </Stack>

          {searchResults.length > 0 && (
            <Box sx={{ mb: 2 }}>
              <Typography variant="caption" color="text.secondary">
                Search Results ({searchResults.length})
              </Typography>
              {searchResults.map((article) => (
                <ArticleCard
                  key={article.id}
                  article={article}
                  isAttached={attachedArticleIds.includes(article.id)}
                  onSelect={onArticleSelect}
                  onAttach={handleAttach}
                />
              ))}
            </Box>
          )}
        </Collapse>

        <Divider sx={{ my: 1 }} />

        {/* Suggestions */}
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 3 }}>
            <CircularProgress size={24} />
          </Box>
        ) : sortedSuggestions.length > 0 ? (
          <>
            <Typography variant="caption" color="text.secondary" sx={{ mb: 1, display: 'block' }}>
              Suggested Articles ({sortedSuggestions.length})
            </Typography>
            {sortedSuggestions.map((article) => (
              <ArticleCard
                key={article.id}
                article={article}
                isAttached={attachedArticleIds.includes(article.id)}
                onSelect={onArticleSelect}
                onAttach={handleAttach}
              />
            ))}
          </>
        ) : (
          <Box sx={{ py: 2, textAlign: 'center' }}>
            <ArticleIcon sx={{ fontSize: 40, color: 'action.disabled', mb: 1 }} />
            <Typography color="text.secondary" variant="body2">
              No suggestions available
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {incidentDescription
                ? 'Try adding more details to the description'
                : 'Enter incident description to get suggestions'}
            </Typography>
          </Box>
        )}
      </Collapse>
    </Paper>
  );
};

export default ArticleSuggestions;
