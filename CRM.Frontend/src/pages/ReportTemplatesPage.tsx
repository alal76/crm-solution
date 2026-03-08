import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  CardActions,
  Typography,
  Button,
  Grid,
  Chip,
  TextField,
  InputAdornment,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Rating,
  Avatar,
  Stack,
  Alert,
  CircularProgress,
} from '@mui/material';
import {
  Search as SearchIcon,
  Download as DownloadIcon,
  Visibility as PreviewIcon,
  Star as StarIcon,
  TrendingUp as TrendingIcon,
  InsertChart as ChartIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import logger from '../services/logger';

interface ReportTemplate {
  id: number;
  name: string;
  description: string;
  category: string;
  author: string;
  rating: number;
  downloads: number;
  tags: string[];
  previewImage?: string;
  reportConfig: object;
  createdAt: string;
}

const mockTemplates: ReportTemplate[] = [
  {
    id: 1,
    name: 'Sales Pipeline Report',
    description: 'Comprehensive view of your sales pipeline with stage-by-stage analysis, conversion rates, and forecasted revenue.',
    category: 'Sales',
    author: 'CRM Solution Team',
    rating: 4.8,
    downloads: 1523,
    tags: ['sales', 'pipeline', 'forecasting'],
    createdAt: '2026-01-15',
    reportConfig: { type: 'pipeline', groupBy: 'stage', metrics: ['count', 'value', 'conversionRate'] },
  },
  {
    id: 2,
    name: 'Campaign ROI Dashboard',
    description: 'Track marketing campaign performance, cost per lead, conversion rates, and overall ROI across all channels.',
    category: 'Marketing',
    author: 'Marketing Ops',
    rating: 4.6,
    downloads: 982,
    tags: ['marketing', 'roi', 'campaigns'],
    createdAt: '2026-01-22',
    reportConfig: { type: 'campaign', metrics: ['spend', 'leads', 'conversions', 'roi'] },
  },
  {
    id: 3,
    name: 'Customer Churn Analysis',
    description: 'Identify at-risk customers, analyze churn patterns by segment, and track retention metrics over time.',
    category: 'Customer Success',
    author: 'CS Analytics',
    rating: 4.9,
    downloads: 756,
    tags: ['churn', 'retention', 'customer-success'],
    createdAt: '2026-02-03',
    reportConfig: { type: 'churn', groupBy: 'segment', timeRange: 'last12months' },
  },
  {
    id: 4,
    name: 'Territory Performance',
    description: 'Compare sales performance across territories, regions, and individual reps with quota attainment tracking.',
    category: 'Sales',
    author: 'Sales Ops',
    rating: 4.7,
    downloads: 1102,
    tags: ['territory', 'quota', 'performance'],
    createdAt: '2026-02-10',
    reportConfig: { type: 'territory', groupBy: 'region', metrics: ['revenue', 'quota', 'attainment'] },
  },
  {
    id: 5,
    name: 'Lead Source Attribution',
    description: 'Understand which lead sources drive the most revenue with multi-touch attribution modeling.',
    category: 'Marketing',
    author: 'Demand Gen',
    rating: 4.5,
    downloads: 634,
    tags: ['attribution', 'lead-source', 'marketing'],
    createdAt: '2026-02-18',
    reportConfig: { type: 'attribution', model: 'multi-touch', groupBy: 'source' },
  },
];

const categories = ['All', 'Sales', 'Marketing', 'Customer Success', 'Finance', 'Service Desk'];

const ReportTemplatesPage: React.FC = () => {
  const navigate = useNavigate();
  const [templates, setTemplates] = useState<ReportTemplate[]>(mockTemplates);
  const [filteredTemplates, setFilteredTemplates] = useState<ReportTemplate[]>(mockTemplates);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('All');
  const [previewTemplate, setPreviewTemplate] = useState<ReportTemplate | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    filterTemplates();
  }, [searchQuery, selectedCategory, templates]);

  const filterTemplates = () => {
    let filtered = templates;

    if (selectedCategory !== 'All') {
      filtered = filtered.filter((t) => t.category === selectedCategory);
    }

    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      filtered = filtered.filter(
        (t) =>
          t.name.toLowerCase().includes(query) ||
          t.description.toLowerCase().includes(query) ||
          t.tags.some((tag) => tag.toLowerCase().includes(query))
      );
    }

    setFilteredTemplates(filtered);
  };

  const handleDownloadTemplate = async (template: ReportTemplate) => {
    try {
      setLoading(true);
      logger.info(`Downloading template: ${template.name}`);

      // PRA-019: TODO — wire to report templates API once backend endpoint is implemented.
      // Proposed: GET /api/reports/templates   → returns ReportTemplate[]
      //           POST /api/reports/templates/{id}/apply  → applies template config to a new report
      // No /api/reports/templates endpoint exists on ReportsController yet.
      // await reportService.importTemplate(template.reportConfig);

      // For now, redirect to report designer with pre-loaded config
      navigate('/reports/designer', {
        state: { templateConfig: template.reportConfig, templateName: template.name },
      });

      setTemplates((prev) =>
        prev.map((t) => (t.id === template.id ? { ...t, downloads: t.downloads + 1 } : t))
      );
    } catch (error) {
      logger.error('Failed to download template:', error);
    } finally {
      setLoading(false);
    }
  };

  const handlePreview = (template: ReportTemplate) => {
    setPreviewTemplate(template);
  };

  const handleClosePreview = () => {
    setPreviewTemplate(null);
  };

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ mb: 3 }}>
        <Typography variant="h4" fontWeight={700} gutterBottom>
          Report Templates Marketplace
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Browse and download pre-built report templates to jumpstart your analytics
        </Typography>
      </Box>

      {/* Filters */}
      <Box sx={{ mb: 3, display: 'flex', gap: 2, alignItems: 'center', flexWrap: 'wrap' }}>
        <TextField
          size="small"
          placeholder="Search templates..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          sx={{ minWidth: 300 }}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon />
              </InputAdornment>
            ),
          }}
        />
        <FormControl size="small" sx={{ minWidth: 180 }}>
          <InputLabel>Category</InputLabel>
          <Select
            value={selectedCategory}
            label="Category"
            onChange={(e) => setSelectedCategory(e.target.value)}
          >
            {categories.map((cat) => (
              <MenuItem key={cat} value={cat}>
                {cat}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
        <Chip icon={<TrendingIcon />} label="Trending" clickable />
        <Chip icon={<StarIcon />} label="Top Rated" clickable />
      </Box>

      {/* Template Grid */}
      {loading && <CircularProgress sx={{ display: 'block', mx: 'auto', my: 4 }} />}
      {!loading && filteredTemplates.length === 0 && (
        <Alert severity="info">No templates found matching your criteria.</Alert>
      )}
      {!loading && (
        <Grid container spacing={3}>
          {filteredTemplates.map((template) => (
            <Grid item xs={12} sm={6} md={4} key={template.id}>
              <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
                <CardContent sx={{ flexGrow: 1 }}>
                  <Box sx={{ display: 'flex', alignItems: 'flex-start', mb: 2 }}>
                    <Avatar sx={{ bgcolor: 'primary.main', mr: 2 }}>
                      <ChartIcon />
                    </Avatar>
                    <Box sx={{ flexGrow: 1 }}>
                      <Typography variant="h6" fontWeight={600} gutterBottom>
                        {template.name}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        by {template.author}
                      </Typography>
                    </Box>
                  </Box>

                  <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                    {template.description}
                  </Typography>

                  <Box sx={{ mb: 1 }}>
                    {template.tags.map((tag) => (
                      <Chip
                        key={tag}
                        label={tag}
                        size="small"
                        sx={{ mr: 0.5, mb: 0.5, fontSize: '0.7rem' }}
                      />
                    ))}
                  </Box>

                  <Stack direction="row" spacing={2} alignItems="center" sx={{ mt: 2 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center' }}>
                      <Rating value={template.rating} precision={0.1} size="small" readOnly />
                      <Typography variant="caption" sx={{ ml: 0.5 }}>
                        {template.rating}
                      </Typography>
                    </Box>
                    <Typography variant="caption" color="text.secondary">
                      {template.downloads} downloads
                    </Typography>
                  </Stack>
                </CardContent>

                <CardActions>
                  <Button
                    size="small"
                    startIcon={<PreviewIcon />}
                    onClick={() => handlePreview(template)}
                  >
                    Preview
                  </Button>
                  <Button
                    size="small"
                    variant="contained"
                    startIcon={<DownloadIcon />}
                    onClick={() => handleDownloadTemplate(template)}
                    disabled={loading}
                  >
                    Use Template
                  </Button>
                </CardActions>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}

      {/* Preview Dialog */}
      <Dialog open={!!previewTemplate} onClose={handleClosePreview} maxWidth="md" fullWidth>
        {previewTemplate && (
          <>
            <DialogTitle>{previewTemplate.name}</DialogTitle>
            <DialogContent>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                <strong>Category:</strong> {previewTemplate.category}
              </Typography>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                <strong>Author:</strong> {previewTemplate.author}
              </Typography>
              <Typography variant="body2" paragraph sx={{ mt: 2 }}>
                {previewTemplate.description}
              </Typography>
              <Box sx={{ mt: 2 }}>
                <Typography variant="subtitle2" gutterBottom>
                  Tags:
                </Typography>
                {previewTemplate.tags.map((tag) => (
                  <Chip key={tag} label={tag} size="small" sx={{ mr: 0.5, mb: 0.5 }} />
                ))}
              </Box>
              <Box sx={{ mt: 3, p: 2, bgcolor: 'grey.100', borderRadius: 1 }}>
                <Typography variant="subtitle2" gutterBottom>
                  Report Configuration Preview:
                </Typography>
                <Typography
                  variant="caption"
                  component="pre"
                  sx={{ fontFamily: 'monospace', whiteSpace: 'pre-wrap' }}
                >
                  {JSON.stringify(previewTemplate.reportConfig, null, 2)}
                </Typography>
              </Box>
            </DialogContent>
            <DialogActions>
              <Button onClick={handleClosePreview}>Close</Button>
              <Button
                variant="contained"
                startIcon={<DownloadIcon />}
                onClick={() => {
                  handleDownloadTemplate(previewTemplate);
                  handleClosePreview();
                }}
              >
                Use This Template
              </Button>
            </DialogActions>
          </>
        )}
      </Dialog>
    </Box>
  );
};

export default ReportTemplatesPage;
