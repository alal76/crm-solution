/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 * Licensed under the GNU Affero General Public License v3.0
 */

import { useState, useEffect } from 'react';
import {
  Box, Container, Typography, Card, CardContent, Table, TableBody, TableCell,
  TableHead, TableRow, Button, Dialog, DialogTitle, DialogContent, DialogActions,
  TextField, MenuItem, Stack, Chip, IconButton, Tooltip, CircularProgress,
  Alert, Grid, Tabs, Tab, FormControl, InputLabel, Select, SelectChangeEvent,
  Paper, InputAdornment, CardActionArea, Rating, Divider,
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon,
  Search as SearchIcon, Close as CloseIcon, Refresh as RefreshIcon,
  Article as ArticleIcon, ThumbUp as ThumbUpIcon, ThumbDown as ThumbDownIcon,
  Visibility as ViewIcon, Print as PrintIcon, Category as CategoryIcon,
  LocalOffer as TagIcon,
} from '@mui/icons-material';
import { DialogError, ActionButton, TabPanel } from '../components/common';
import { useApiState } from '../hooks/useApiState';
import { useProfile } from '../contexts/ProfileContext';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';

// ==================== TYPES ====================

enum ArticleStatus {
  Draft = 0,
  InReview = 1,
  Published = 2,
  NeedsUpdate = 3,
  Archived = 4,
  Deprecated = 5,
}

enum ArticleVisibility {
  Internal = 0,
  CustomerPortal = 1,
  Public = 2,
}

interface KnowledgeCategory {
  id: number;
  name: string;
  description?: string;
  parentId?: number;
  order: number;
  articleCount: number;
}

interface KnowledgeArticle {
  id: number;
  title: string;
  slug: string;
  summary?: string;
  content: string;
  status: ArticleStatus;
  visibility: ArticleVisibility;
  categoryId?: number;
  categoryName?: string;
  tags?: string;
  authorId?: number;
  authorName?: string;
  viewCount: number;
  helpfulCount: number;
  notHelpfulCount: number;
  rating: number;
  isFeatured: boolean;
  publishedAt?: string;
  lastReviewedAt?: string;
  expiresAt?: string;
  relatedArticleIds?: string;
  attachments?: string;
  createdAt?: string;
  updatedAt?: string;
}

interface ArticleForm {
  title: string;
  slug: string;
  summary: string;
  content: string;
  status: ArticleStatus;
  visibility: ArticleVisibility;
  categoryId: number | null;
  tags: string;
  isFeatured: boolean;
  expiresAt: string;
}

// ==================== CONSTANTS ====================

const ARTICLE_STATUS_OPTIONS = [
  { value: ArticleStatus.Draft, label: 'Draft', color: 'default' },
  { value: ArticleStatus.InReview, label: 'In Review', color: 'warning' },
  { value: ArticleStatus.Published, label: 'Published', color: 'success' },
  { value: ArticleStatus.NeedsUpdate, label: 'Needs Update', color: 'warning' },
  { value: ArticleStatus.Archived, label: 'Archived', color: 'error' },
  { value: ArticleStatus.Deprecated, label: 'Deprecated', color: 'error' },
];

const VISIBILITY_OPTIONS = [
  { value: ArticleVisibility.Internal, label: 'Internal Only' },
  { value: ArticleVisibility.CustomerPortal, label: 'Customer Portal' },
  { value: ArticleVisibility.Public, label: 'Public' },
];

// ==================== HELPER FUNCTIONS ====================

const getStatusInfo = (status: ArticleStatus) =>
  ARTICLE_STATUS_OPTIONS.find(s => s.value === status) || { label: 'Unknown', color: 'default' };

const getVisibilityLabel = (visibility: ArticleVisibility) =>
  VISIBILITY_OPTIONS.find(v => v.value === visibility)?.label || 'Unknown';

const generateSlug = (title: string): string => {
  return title.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '');
};

const formatDate = (dateString?: string) =>
  dateString ? new Date(dateString).toLocaleDateString() : '-';

// ==================== MAIN COMPONENT ====================

function KnowledgeBasePage() {
  // State
  const [articles, setArticles] = useState<KnowledgeArticle[]>([]);
  const [categories, setCategories] = useState<KnowledgeCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [dialogTab, setDialogTab] = useState(0);
  const [searchQuery, setSearchQuery] = useState('');
  const [filterCategory, setFilterCategory] = useState<number | 'all'>('all');
  const [filterStatus, setFilterStatus] = useState<ArticleStatus | 'all'>('all');
  const [viewMode, setViewMode] = useState<'list' | 'cards'>('list');
  const [viewArticle, setViewArticle] = useState<KnowledgeArticle | null>(null);
  const [viewDialogOpen, setViewDialogOpen] = useState(false);

  // Category dialog
  const [categoryDialogOpen, setCategoryDialogOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<KnowledgeCategory | null>(null);
  const [categoryForm, setCategoryForm] = useState({ name: '', description: '', parentId: null as number | null, order: 0 });

  const emptyForm: ArticleForm = {
    title: '',
    slug: '',
    summary: '',
    content: '',
    status: ArticleStatus.Draft,
    visibility: ArticleVisibility.Internal,
    categoryId: null,
    tags: '',
    isFeatured: false,
    expiresAt: '',
  };
  const [formData, setFormData] = useState<ArticleForm>(emptyForm);

  const dialogApi = useApiState();
  const { hasPermission } = useProfile();

  // ==================== DATA FETCHING ====================

  useEffect(() => {
    fetchArticles();
    fetchCategories();
  }, []);

  const fetchArticles = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/knowledge/articles');
      setArticles(response.data);
      setError(null);
    } catch (err: any) {
      if (err.response?.status === 404) {
        setArticles([]);
        setError(null);
      } else {
        setError(err.response?.data?.message || 'Failed to fetch articles');
      }
    } finally {
      setLoading(false);
    }
  };

  const fetchCategories = async () => {
    try {
      const response = await apiClient.get('/knowledge/categories');
      setCategories(response.data);
    } catch (err: any) {
      // Categories endpoint might not exist yet
      setCategories([]);
    }
  };

  // ==================== DIALOG HANDLERS ====================

  const handleOpenDialog = (article?: KnowledgeArticle) => {
    setDialogTab(0);
    if (article) {
      setEditingId(article.id);
      setFormData({
        title: article.title,
        slug: article.slug,
        summary: article.summary || '',
        content: article.content,
        status: article.status,
        visibility: article.visibility,
        categoryId: article.categoryId || null,
        tags: article.tags || '',
        isFeatured: article.isFeatured,
        expiresAt: article.expiresAt?.split('T')[0] || '',
      });
    } else {
      setEditingId(null);
      setFormData(emptyForm);
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingId(null);
    dialogApi.clearError();
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    const checked = (e.target as HTMLInputElement).checked;

    // Auto-generate slug from title
    if (name === 'title' && !editingId) {
      setFormData(prev => ({
        ...prev,
        [name]: value,
        slug: generateSlug(value),
      }));
      return;
    }

    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }));
  };

  const handleSelectChange = (e: SelectChangeEvent<number | string>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name as string]: value }));
  };

  // ==================== SAVE OPERATIONS ====================

  const handleSaveArticle = async () => {
    if (!formData.title?.trim() || !formData.content?.trim()) {
      dialogApi.setError('Title and content are required');
      return;
    }

    await dialogApi.execute(async () => {
      if (editingId) {
        await apiClient.put(`/knowledge/articles/${editingId}`, formData);
        setSuccessMessage('Article updated successfully');
      } else {
        await apiClient.post('/knowledge/articles', formData);
        setSuccessMessage('Article created successfully');
      }
      handleCloseDialog();
      fetchArticles();
      setTimeout(() => setSuccessMessage(null), 3000);
    });
  };

  const handleDeleteArticle = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this article?')) {
      try {
        await apiClient.delete(`/knowledge/articles/${id}`);
        setSuccessMessage('Article deleted successfully');
        fetchArticles();
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete article');
      }
    }
  };

  // ==================== VIEW ARTICLE ====================

  const handleViewArticle = async (article: KnowledgeArticle) => {
    setViewArticle(article);
    setViewDialogOpen(true);
    // Views are tracked automatically by backend when fetching by slug
  };

  const handleRateArticle = async (helpful: boolean) => {
    if (!viewArticle) return;
    try {
      await apiClient.post(`/knowledge/articles/${viewArticle.id}/feedback`, { 
        isHelpful: helpful 
      });
      setSuccessMessage(helpful ? 'Thanks for your feedback!' : 'Thanks for your feedback. We\'ll work to improve this article.');
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch {
      // Ignore rating errors
    }
  };

  const handlePrintArticle = (article: KnowledgeArticle) => {
    const printWindow = window.open('', '_blank');
    if (!printWindow) return;

    printWindow.document.write(`
      <!DOCTYPE html>
      <html>
      <head>
        <title>${article.title}</title>
        <style>
          body { font-family: Arial, sans-serif; padding: 40px; max-width: 800px; margin: 0 auto; line-height: 1.6; }
          h1 { color: #333; border-bottom: 2px solid #1976d2; padding-bottom: 10px; }
          .meta { color: #666; font-size: 14px; margin-bottom: 20px; }
          .content { margin-top: 20px; }
          .tags { margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; }
          .tag { display: inline-block; background: #e3f2fd; padding: 4px 12px; border-radius: 16px; margin-right: 8px; font-size: 12px; }
          @media print { button { display: none; } }
        </style>
      </head>
      <body>
        <h1>${article.title}</h1>
        <div class="meta">
          ${article.categoryName ? `Category: ${article.categoryName} | ` : ''}
          Published: ${formatDate(article.publishedAt)} |
          Views: ${article.viewCount}
        </div>
        ${article.summary ? `<p><em>${article.summary}</em></p>` : ''}
        <div class="content">${article.content}</div>
        ${article.tags ? `
          <div class="tags">
            ${article.tags.split(',').map(tag => `<span class="tag">${tag.trim()}</span>`).join('')}
          </div>
        ` : ''}
        <button onclick="window.print()" style="margin-top: 30px; padding: 10px 20px; cursor: pointer;">Print Article</button>
      </body>
      </html>
    `);
    printWindow.document.close();
  };

  // ==================== CATEGORY OPERATIONS ====================

  const handleOpenCategoryDialog = (category?: KnowledgeCategory) => {
    if (category) {
      setEditingCategory(category);
      setCategoryForm({
        name: category.name,
        description: category.description || '',
        parentId: category.parentId || null,
        order: category.order,
      });
    } else {
      setEditingCategory(null);
      setCategoryForm({ name: '', description: '', parentId: null, order: 0 });
    }
    setCategoryDialogOpen(true);
  };

  const handleSaveCategory = async () => {
    if (!categoryForm.name?.trim()) return;

    try {
      if (editingCategory) {
        await apiClient.put(`/knowledge/categories/${editingCategory.id}`, categoryForm);
        setSuccessMessage('Category updated');
      } else {
        await apiClient.post('/knowledge/categories', categoryForm);
        setSuccessMessage('Category created');
      }
      setCategoryDialogOpen(false);
      fetchCategories();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save category');
    }
  };

  const handleDeleteCategory = async (id: number) => {
    if (window.confirm('Are you sure? Articles in this category will be uncategorized.')) {
      try {
        await apiClient.delete(`/knowledge/categories/${id}`);
        setSuccessMessage('Category deleted');
        fetchCategories();
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to delete category');
      }
    }
  };

  // ==================== FILTERING ====================

  const filteredArticles = articles.filter(article => {
    const matchesSearch = !searchQuery ||
      article.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
      article.summary?.toLowerCase().includes(searchQuery.toLowerCase()) ||
      article.tags?.toLowerCase().includes(searchQuery.toLowerCase());

    const matchesCategory = filterCategory === 'all' || article.categoryId === filterCategory;
    const matchesStatus = filterStatus === 'all' || article.status === filterStatus;

    return matchesSearch && matchesCategory && matchesStatus;
  });

  // ==================== RENDER ====================

  if (loading) {
    return (
      <Container maxWidth="lg">
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="50vh">
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg">
      <Box mb={4}>
        {/* Header */}
        <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
          <Box display="flex" alignItems="center" gap={2}>
            <img src={logo} alt="CRM Logo" style={{ height: 40, borderRadius: 8 }} />
            <Typography variant="h4">Knowledge Base</Typography>
          </Box>
          <Stack direction="row" spacing={2}>
            <Button variant="outlined" startIcon={<CategoryIcon />} onClick={() => handleOpenCategoryDialog()}>
              Manage Categories
            </Button>
            <Button variant="outlined" startIcon={<RefreshIcon />} onClick={fetchArticles}>
              Refresh
            </Button>
            <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpenDialog()}>
              New Article
            </Button>
          </Stack>
        </Box>

        {/* Alerts */}
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

        {/* Search & Filters */}
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Grid container spacing={2} alignItems="center">
              <Grid item xs={12} md={4}>
                <TextField
                  fullWidth
                  size="small"
                  placeholder="Search articles..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <SearchIcon />
                      </InputAdornment>
                    ),
                  }}
                />
              </Grid>
              <Grid item xs={6} md={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>Category</InputLabel>
                  <Select
                    value={filterCategory}
                    onChange={(e) => setFilterCategory(e.target.value as number | 'all')}
                    label="Category"
                  >
                    <MenuItem value="all">All Categories</MenuItem>
                    {categories.map(cat => (
                      <MenuItem key={cat.id} value={cat.id}>{cat.name}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={6} md={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>Status</InputLabel>
                  <Select
                    value={filterStatus}
                    onChange={(e) => setFilterStatus(e.target.value as ArticleStatus | 'all')}
                    label="Status"
                  >
                    <MenuItem value="all">All Statuses</MenuItem>
                    {ARTICLE_STATUS_OPTIONS.map(opt => (
                      <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={2}>
                <Typography variant="body2" color="text.secondary">
                  {filteredArticles.length} article(s)
                </Typography>
              </Grid>
            </Grid>
          </CardContent>
        </Card>

        {/* Categories Overview */}
        {categories.length > 0 && (
          <Box mb={3}>
            <Stack direction="row" spacing={1} flexWrap="wrap" gap={1}>
              {categories.map(cat => (
                <Chip
                  key={cat.id}
                  label={`${cat.name} (${cat.articleCount || 0})`}
                  onClick={() => setFilterCategory(cat.id)}
                  color={filterCategory === cat.id ? 'primary' : 'default'}
                  variant={filterCategory === cat.id ? 'filled' : 'outlined'}
                />
              ))}
              {filterCategory !== 'all' && (
                <Chip
                  label="Clear Filter"
                  onClick={() => setFilterCategory('all')}
                  onDelete={() => setFilterCategory('all')}
                  color="secondary"
                  variant="outlined"
                />
              )}
            </Stack>
          </Box>
        )}

        {/* Articles Table */}
        <Card>
          <CardContent>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Title</TableCell>
                  <TableCell>Category</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Visibility</TableCell>
                  <TableCell align="center">Views</TableCell>
                  <TableCell align="center">Helpful</TableCell>
                  <TableCell>Updated</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredArticles.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={8} align="center">
                      <Typography color="text.secondary" py={4}>
                        No articles found. Create your first knowledge base article!
                      </Typography>
                    </TableCell>
                  </TableRow>
                ) : (
                  filteredArticles.map((article) => {
                    const statusInfo = getStatusInfo(article.status);
                    const helpfulPercent = article.helpfulCount + article.notHelpfulCount > 0
                      ? Math.round((article.helpfulCount / (article.helpfulCount + article.notHelpfulCount)) * 100)
                      : 0;

                    return (
                      <TableRow key={article.id} hover>
                        <TableCell>
                          <Box>
                            <Typography fontWeight="medium">
                              {article.isFeatured && '⭐ '}
                              {article.title}
                            </Typography>
                            {article.summary && (
                              <Typography variant="caption" color="text.secondary" noWrap sx={{ maxWidth: 300, display: 'block' }}>
                                {article.summary}
                              </Typography>
                            )}
                          </Box>
                        </TableCell>
                        <TableCell>{article.categoryName || '-'}</TableCell>
                        <TableCell>
                          <Chip
                            label={statusInfo.label}
                            size="small"
                            color={statusInfo.color as any}
                          />
                        </TableCell>
                        <TableCell>{getVisibilityLabel(article.visibility)}</TableCell>
                        <TableCell align="center">{article.viewCount}</TableCell>
                        <TableCell align="center">
                          {helpfulPercent > 0 ? `${helpfulPercent}%` : '-'}
                        </TableCell>
                        <TableCell>{formatDate(article.updatedAt)}</TableCell>
                        <TableCell align="right">
                          <Tooltip title="View">
                            <IconButton size="small" onClick={() => handleViewArticle(article)}>
                              <ViewIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Edit">
                            <IconButton size="small" onClick={() => handleOpenDialog(article)}>
                              <EditIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Print">
                            <IconButton size="small" onClick={() => handlePrintArticle(article)}>
                              <PrintIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delete">
                            <IconButton size="small" color="error" onClick={() => handleDeleteArticle(article.id)}>
                              <DeleteIcon />
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    );
                  })
                )}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </Box>

      {/* Article Editor Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="lg" fullWidth>
        <DialogTitle>
          <Box display="flex" justifyContent="space-between" alignItems="center">
            <Box display="flex" alignItems="center" gap={1}>
              <ArticleIcon />
              {editingId ? 'Edit Article' : 'Create New Article'}
            </Box>
            <IconButton onClick={handleCloseDialog}><CloseIcon /></IconButton>
          </Box>
        </DialogTitle>
        <DialogContent dividers>
          <Tabs value={dialogTab} onChange={(_, v) => setDialogTab(v)} sx={{ mb: 2 }}>
            <Tab label="Content" />
            <Tab label="Settings" />
          </Tabs>

          <DialogError error={dialogApi.error} />

          {/* Tab 0: Content */}
          <TabPanel value={dialogTab} index={0}>
            <Grid container spacing={3}>
              <Grid item xs={12} md={8}>
                <TextField
                  fullWidth
                  required
                  label="Title"
                  name="title"
                  value={formData.title}
                  onChange={handleInputChange}
                />
              </Grid>
              <Grid item xs={12} md={4}>
                <TextField
                  fullWidth
                  label="URL Slug"
                  name="slug"
                  value={formData.slug}
                  onChange={handleInputChange}
                  helperText="URL-friendly identifier"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  multiline
                  rows={2}
                  label="Summary"
                  name="summary"
                  value={formData.summary}
                  onChange={handleInputChange}
                  helperText="Brief description shown in search results"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  required
                  multiline
                  rows={15}
                  label="Content"
                  name="content"
                  value={formData.content}
                  onChange={handleInputChange}
                  helperText="HTML content supported"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Tags"
                  name="tags"
                  value={formData.tags}
                  onChange={handleInputChange}
                  helperText="Comma-separated tags for search"
                  placeholder="troubleshooting, setup, configuration"
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <TagIcon />
                      </InputAdornment>
                    ),
                  }}
                />
              </Grid>
            </Grid>
          </TabPanel>

          {/* Tab 1: Settings */}
          <TabPanel value={dialogTab} index={1}>
            <Grid container spacing={3}>
              <Grid item xs={12} md={4}>
                <FormControl fullWidth>
                  <InputLabel>Status</InputLabel>
                  <Select
                    name="status"
                    value={formData.status}
                    onChange={handleSelectChange}
                    label="Status"
                  >
                    {ARTICLE_STATUS_OPTIONS.map(opt => (
                      <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={4}>
                <FormControl fullWidth>
                  <InputLabel>Visibility</InputLabel>
                  <Select
                    name="visibility"
                    value={formData.visibility}
                    onChange={handleSelectChange}
                    label="Visibility"
                  >
                    {VISIBILITY_OPTIONS.map(opt => (
                      <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={4}>
                <FormControl fullWidth>
                  <InputLabel>Category</InputLabel>
                  <Select
                    name="categoryId"
                    value={formData.categoryId || ''}
                    onChange={handleSelectChange}
                    label="Category"
                  >
                    <MenuItem value="">None</MenuItem>
                    {categories.map(cat => (
                      <MenuItem key={cat.id} value={cat.id}>{cat.name}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  type="date"
                  label="Expires At"
                  name="expiresAt"
                  value={formData.expiresAt}
                  onChange={handleInputChange}
                  InputLabelProps={{ shrink: true }}
                  helperText="Leave empty for no expiration"
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <Box display="flex" alignItems="center" height="100%">
                  <label>
                    <input
                      type="checkbox"
                      name="isFeatured"
                      checked={formData.isFeatured}
                      onChange={handleInputChange}
                    />
                    {' '}Featured Article (shown prominently)
                  </label>
                </Box>
              </Grid>
            </Grid>
          </TabPanel>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <ActionButton
            onClick={handleSaveArticle}
            loading={dialogApi.loading}
            variant="contained"
          >
            {editingId ? 'Update Article' : 'Create Article'}
          </ActionButton>
        </DialogActions>
      </Dialog>

      {/* View Article Dialog */}
      <Dialog open={viewDialogOpen} onClose={() => setViewDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          <Box display="flex" justifyContent="space-between" alignItems="center">
            {viewArticle?.title}
            <IconButton onClick={() => setViewDialogOpen(false)}><CloseIcon /></IconButton>
          </Box>
        </DialogTitle>
        <DialogContent dividers>
          {viewArticle && (
            <Box>
              {viewArticle.summary && (
                <Typography variant="subtitle1" color="text.secondary" gutterBottom fontStyle="italic">
                  {viewArticle.summary}
                </Typography>
              )}
              <Divider sx={{ my: 2 }} />
              <Box
                sx={{ '& img': { maxWidth: '100%' }, '& pre': { bgcolor: 'grey.100', p: 2, borderRadius: 1, overflow: 'auto' } }}
                dangerouslySetInnerHTML={{ __html: viewArticle.content }}
              />
              {viewArticle.tags && (
                <Box mt={3}>
                  <Divider sx={{ mb: 2 }} />
                  <Stack direction="row" spacing={1}>
                    {viewArticle.tags.split(',').map((tag, i) => (
                      <Chip key={i} label={tag.trim()} size="small" variant="outlined" />
                    ))}
                  </Stack>
                </Box>
              )}
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Box flex={1} display="flex" alignItems="center" gap={2} px={2}>
            <Typography variant="body2">Was this article helpful?</Typography>
            <IconButton color="success" onClick={() => handleRateArticle(true)}>
              <ThumbUpIcon />
            </IconButton>
            <IconButton color="error" onClick={() => handleRateArticle(false)}>
              <ThumbDownIcon />
            </IconButton>
          </Box>
          <Button onClick={() => handlePrintArticle(viewArticle!)} startIcon={<PrintIcon />}>
            Print
          </Button>
          <Button onClick={() => setViewDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>

      {/* Category Manager Dialog */}
      <Dialog open={categoryDialogOpen} onClose={() => setCategoryDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          {editingCategory ? 'Edit Category' : 'Manage Categories'}
        </DialogTitle>
        <DialogContent>
          {!editingCategory && (
            <Box mb={3}>
              <Typography variant="subtitle2" gutterBottom>Existing Categories</Typography>
              {categories.length === 0 ? (
                <Typography color="text.secondary">No categories yet</Typography>
              ) : (
                <Table size="small">
                  <TableBody>
                    {categories.map(cat => (
                      <TableRow key={cat.id}>
                        <TableCell>{cat.name}</TableCell>
                        <TableCell>{cat.articleCount || 0} articles</TableCell>
                        <TableCell align="right">
                          <IconButton size="small" onClick={() => handleOpenCategoryDialog(cat)}>
                            <EditIcon fontSize="small" />
                          </IconButton>
                          <IconButton size="small" color="error" onClick={() => handleDeleteCategory(cat.id)}>
                            <DeleteIcon fontSize="small" />
                          </IconButton>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
              <Divider sx={{ my: 2 }} />
              <Typography variant="subtitle2" gutterBottom>Add New Category</Typography>
            </Box>
          )}
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Category Name"
                value={categoryForm.name}
                onChange={(e) => setCategoryForm(prev => ({ ...prev, name: e.target.value }))}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                multiline
                rows={2}
                label="Description"
                value={categoryForm.description}
                onChange={(e) => setCategoryForm(prev => ({ ...prev, description: e.target.value }))}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Parent Category</InputLabel>
                <Select
                  value={categoryForm.parentId || ''}
                  onChange={(e) => setCategoryForm(prev => ({ ...prev, parentId: e.target.value as number | null }))}
                  label="Parent Category"
                >
                  <MenuItem value="">None (Top Level)</MenuItem>
                  {categories.filter(c => c.id !== editingCategory?.id).map(cat => (
                    <MenuItem key={cat.id} value={cat.id}>{cat.name}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                type="number"
                label="Display Order"
                value={categoryForm.order}
                onChange={(e) => setCategoryForm(prev => ({ ...prev, order: parseInt(e.target.value) || 0 }))}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCategoryDialogOpen(false)}>Close</Button>
          <Button variant="contained" onClick={handleSaveCategory}>
            {editingCategory ? 'Update Category' : 'Add Category'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

export default KnowledgeBasePage;
