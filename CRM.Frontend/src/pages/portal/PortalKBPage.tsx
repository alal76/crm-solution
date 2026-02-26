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
  Card,
  CardContent,
  CardActionArea,
  CircularProgress,
  Dialog,
  DialogContent,
  DialogTitle,
  Divider,
  IconButton,
  InputAdornment,
  TablePagination,
  TextField,
  Toolbar,
  Typography,
  Alert,
} from '@mui/material';
import { ArrowBack, ExitToApp, MenuBook, Search, Close, Visibility } from '@mui/icons-material';
import { useNavigate, Link } from 'react-router-dom';
import {
  portalAuthService,
  portalService,
  type PortalKBArticleDto,
  type PortalConfigDto,
} from '../../services/portalService';

const PortalKBPage: React.FC = () => {
  const navigate = useNavigate();
  const [articles, setArticles] = useState<PortalKBArticleDto[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(12);
  const [search, setSearch] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [loading, setLoading] = useState(true);
  const [config, setConfig] = useState<PortalConfigDto | null>(null);
  const [selected, setSelected] = useState<PortalKBArticleDto | null>(null);
  const [articleLoading, setArticleLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const user = portalAuthService.getCurrentUser();
  const brandColor = config?.primaryColor ?? '#1976d2';

  const loadArticles = useCallback(async () => {
    setLoading(true);
    try {
      const result = await portalService.getKBArticles(search || undefined, page + 1, pageSize);
      setArticles(result.items);
      setTotal(result.totalCount);
    } catch {
      setError('Failed to load articles.');
    } finally {
      setLoading(false);
    }
  }, [search, page, pageSize]);

  useEffect(() => {
    portalService.getConfig().then(setConfig).catch(() => {});
  }, []);

  useEffect(() => {
    loadArticles();
  }, [loadArticles]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setSearch(searchInput);
    setPage(0);
  };

  const handleViewArticle = async (id: number) => {
    setArticleLoading(true);
    try {
      const article = await portalService.getKBArticle(id);
      setSelected(article);
    } catch {
      setError('Failed to load article.');
    } finally {
      setArticleLoading(false);
    }
  };

  const handleLogout = () => {
    portalAuthService.logout();
    navigate('/portal/login', { replace: true });
  };

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <AppBar position="static" sx={{ bgcolor: brandColor }}>
        <Toolbar>
          <IconButton color="inherit" component={Link} to="/portal/dashboard" sx={{ mr: 1 }}>
            <ArrowBack />
          </IconButton>
          <MenuBook sx={{ mr: 1 }} />
          <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: 700 }}>
            Knowledge Base
          </Typography>
          <Typography variant="body2" sx={{ mr: 2 }}>{user?.displayName ?? user?.email}</Typography>
          <IconButton color="inherit" onClick={handleLogout} title="Sign out">
            <ExitToApp />
          </IconButton>
        </Toolbar>
      </AppBar>

      <Box sx={{ p: 3, maxWidth: 1000, mx: 'auto' }}>
        {/* Search */}
        <Box component="form" onSubmit={handleSearch} sx={{ mb: 3 }}>
          <TextField
            fullWidth
            placeholder="Search articles..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <Search />
                </InputAdornment>
              ),
            }}
          />
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>{error}</Alert>}

        {loading ? (
          <Box sx={{ textAlign: 'center', py: 6 }}>
            <CircularProgress />
          </Box>
        ) : articles.length === 0 ? (
          <Card>
            <CardContent sx={{ textAlign: 'center', py: 6 }}>
              <MenuBook sx={{ fontSize: 48, color: 'text.disabled', mb: 2 }} />
              <Typography color="text.secondary">
                {search ? `No articles found for "${search}".` : 'No knowledge-base articles published yet.'}
              </Typography>
            </CardContent>
          </Card>
        ) : (
          <>
            <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: 2, mb: 2 }}>
              {articles.map((article) => (
                <Card key={article.id} sx={{ height: '100%' }}>
                  <CardActionArea
                    onClick={() => handleViewArticle(article.id)}
                    sx={{ height: '100%', alignItems: 'flex-start' }}
                  >
                    <CardContent>
                      <Typography variant="subtitle1" fontWeight={600} gutterBottom>
                        {article.title}
                      </Typography>
                      {article.summary && (
                        <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                          {article.summary.length > 120 ? article.summary.slice(0, 120) + '…' : article.summary}
                        </Typography>
                      )}
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                        <Visibility sx={{ fontSize: 14, color: 'text.disabled' }} />
                        <Typography variant="caption" color="text.disabled">{article.viewCount}</Typography>
                      </Box>
                    </CardContent>
                  </CardActionArea>
                </Card>
              ))}
            </Box>

            <TablePagination
              component="div"
              count={total}
              page={page}
              onPageChange={(_e, newPage) => setPage(newPage)}
              rowsPerPage={pageSize}
              onRowsPerPageChange={(e) => { setPageSize(parseInt(e.target.value, 10)); setPage(0); }}
              rowsPerPageOptions={[12, 24, 48]}
            />
          </>
        )}
      </Box>

      {/* Article Detail Dialog */}
      <Dialog
        open={!!selected || articleLoading}
        onClose={() => setSelected(null)}
        maxWidth="md"
        fullWidth
        scroll="paper"
      >
        <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          {selected?.title ?? 'Loading…'}
          <IconButton onClick={() => setSelected(null)}>
            <Close />
          </IconButton>
        </DialogTitle>
        <Divider />
        <DialogContent>
          {articleLoading ? (
            <Box sx={{ textAlign: 'center', py: 4 }}>
              <CircularProgress />
            </Box>
          ) : (
            <Box sx={{ '& p': { mb: 1.5 } }}>
              <Typography component="div" sx={{ whiteSpace: 'pre-wrap' }}>
                {selected?.content}
              </Typography>
            </Box>
          )}
        </DialogContent>
      </Dialog>
    </Box>
  );
};

export default PortalKBPage;
