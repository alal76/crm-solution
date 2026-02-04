/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 * Licensed under the GNU Affero General Public License v3.0
 */

import { useState, useEffect, useCallback } from 'react';
import {
  Box, Container, Typography, Card, CardContent, Table, TableBody, TableCell,
  TableHead, TableRow, Button, Dialog, DialogTitle, DialogContent, DialogActions,
  TextField, MenuItem, Stack, Chip, IconButton, Tooltip, CircularProgress,
  Alert, Grid, Tabs, Tab, FormControl, InputLabel, Select, FormControlLabel,
  Checkbox, Divider, Paper, SelectChangeEvent, LinearProgress,
  List, ListItem, ListItemIcon, ListItemText, ListItemButton,
} from '@mui/material';
import {
  Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon,
  Visibility as PreviewIcon, ContentCopy as CopyIcon,
  Code as EmbedIcon, Close as CloseIcon, Refresh as RefreshIcon,
  Publish as PublishIcon, Unpublished as UnpublishIcon,
  Analytics as AnalyticsIcon, Share as ShareIcon,
  Web as WebIcon, TrendingUp as TrendingUpIcon,
  ArrowUpward, ArrowDownward, DragIndicator as DragIcon,
  Title as HeroIcon, TextFields as TextIcon, Image as ImageIcon,
  SmartButton as ButtonIcon, ViewModule as FeaturesIcon,
  FormatQuote as TestimonialIcon, VideoLibrary as VideoIcon,
  DynamicForm as FormIcon, ViewCarousel as BannerIcon, Html as HtmlIcon,
  SplitscreenRounded as ABTestIcon, Link as LinkIcon, Campaign as CampaignIcon,
} from '@mui/icons-material';
import { DialogError, ActionButton, TabPanel } from '../components/common';
import { useProfile } from '../contexts/ProfileContext';
import apiClient from '../services/apiClient';
import logo from '../assets/logo.png';

// ==================== TYPES ====================

enum LandingPageStatus {
  Draft = 0,
  Published = 1,
  Archived = 2,
  Scheduled = 3,
}

enum LandingPageTemplate {
  Blank = 0,
  LeadCapture = 1,
  ProductShowcase = 2,
  EventRegistration = 3,
  Webinar = 4,
  EBook = 5,
  ContactUs = 6,
  Newsletter = 7,
  ComingSoon = 8,
  ThankYou = 9,
}

enum LandingPageBlockType {
  Hero = 0,
  Text = 1,
  Image = 2,
  Form = 3,
  Button = 4,
  Features = 5,
  Testimonial = 6,
  Video = 7,
  Banner = 8,
  Html = 9,
}

interface LandingPageBlock {
  id?: number;
  landingPageId?: number;
  blockType: LandingPageBlockType;
  sortOrder: number;
  contentJson: string;
  styleJson?: string;
  visibilityCondition?: string;
  isVisible: boolean;
}

interface LandingPage {
  id: number;
  name: string;
  slug: string;
  title?: string;
  metaDescription?: string;
  metaKeywords?: string;
  template: LandingPageTemplate;
  status: LandingPageStatus;
  contentJson?: string;
  htmlContent?: string;
  customCss?: string;
  customJs?: string;
  featuredImageUrl?: string;
  facebookPixelId?: string;
  googleAnalyticsId?: string;
  trackingCode?: string;
  formDefinitionId?: number;
  campaignId?: number;
  thankYouPageId?: number;
  redirectUrl?: string;
  createdByUserId: number;
  createdByUserName?: string;
  publishedAt?: string;
  scheduledPublishAt?: string;
  scheduledUnpublishAt?: string;
  isActive: boolean;
  abTestVariant?: string;
  originalPageId?: number;
  abTestTrafficPercentage?: number;
  pageViews: number;
  uniqueVisitors: number;
  conversions: number;
  conversionRate?: number;
  averageTimeOnPage: number;
  bounceRate: number;
  blocks?: LandingPageBlock[];
  createdAt: string;
  updatedAt?: string;
}

interface LandingPageForm {
  name: string;
  slug: string;
  title: string;
  metaDescription: string;
  metaKeywords: string;
  template: LandingPageTemplate;
  customCss: string;
  customJs: string;
  featuredImageUrl: string;
  facebookPixelId: string;
  googleAnalyticsId: string;
  trackingCode: string;
  formDefinitionId: number | null;
  campaignId: number | null;
  thankYouPageId: number | null;
  redirectUrl: string;
  scheduledPublishAt: string;
  scheduledUnpublishAt: string;
  isActive: boolean;
}

interface LandingPageAnalytics {
  pageId: number;
  pageName: string;
  totalViews: number;
  uniqueVisitors: number;
  totalConversions: number;
  conversionRate: number;
  averageTimeOnPage: number;
  bounceRate: number;
  viewsByDay: { date: string; count: number }[];
  deviceBreakdown: { deviceType: string; count: number }[];
  topReferrers: { referrer: string; count: number }[];
  topUtmSources: { source: string; count: number }[];
}

// ==================== CONSTANTS ====================

const STATUS_OPTIONS = [
  { value: LandingPageStatus.Draft, label: 'Draft', color: 'default' },
  { value: LandingPageStatus.Published, label: 'Published', color: 'success' },
  { value: LandingPageStatus.Archived, label: 'Archived', color: 'warning' },
  { value: LandingPageStatus.Scheduled, label: 'Scheduled', color: 'info' },
];

const TEMPLATE_OPTIONS = [
  { value: LandingPageTemplate.Blank, label: 'Blank' },
  { value: LandingPageTemplate.LeadCapture, label: 'Lead Capture' },
  { value: LandingPageTemplate.ProductShowcase, label: 'Product Showcase' },
  { value: LandingPageTemplate.EventRegistration, label: 'Event Registration' },
  { value: LandingPageTemplate.Webinar, label: 'Webinar' },
  { value: LandingPageTemplate.EBook, label: 'E-Book Download' },
  { value: LandingPageTemplate.ContactUs, label: 'Contact Us' },
  { value: LandingPageTemplate.Newsletter, label: 'Newsletter Signup' },
  { value: LandingPageTemplate.ComingSoon, label: 'Coming Soon' },
  { value: LandingPageTemplate.ThankYou, label: 'Thank You' },
];

const BLOCK_TYPE_OPTIONS = [
  { value: LandingPageBlockType.Hero, label: 'Hero Section', icon: HeroIcon },
  { value: LandingPageBlockType.Text, label: 'Text Block', icon: TextIcon },
  { value: LandingPageBlockType.Image, label: 'Image', icon: ImageIcon },
  { value: LandingPageBlockType.Form, label: 'Form', icon: FormIcon },
  { value: LandingPageBlockType.Button, label: 'Button', icon: ButtonIcon },
  { value: LandingPageBlockType.Features, label: 'Features', icon: FeaturesIcon },
  { value: LandingPageBlockType.Testimonial, label: 'Testimonial', icon: TestimonialIcon },
  { value: LandingPageBlockType.Video, label: 'Video', icon: VideoIcon },
  { value: LandingPageBlockType.Banner, label: 'Banner', icon: BannerIcon },
  { value: LandingPageBlockType.Html, label: 'Custom HTML', icon: HtmlIcon },
];

const getStatusChip = (status: LandingPageStatus) => {
  const option = STATUS_OPTIONS.find(o => o.value === status);
  return option ? (
    <Chip
      size="small"
      label={option.label}
      color={option.color as 'default' | 'success' | 'warning' | 'info'}
    />
  ) : null;
};

const getTemplateName = (template: LandingPageTemplate) => {
  return TEMPLATE_OPTIONS.find(o => o.value === template)?.label || 'Unknown';
};

const getBlockTypeName = (blockType: LandingPageBlockType) => {
  return BLOCK_TYPE_OPTIONS.find(o => o.value === blockType)?.label || 'Unknown';
};

const getBlockIcon = (blockType: LandingPageBlockType) => {
  const BlockIcon = BLOCK_TYPE_OPTIONS.find(o => o.value === blockType)?.icon || HtmlIcon;
  return <BlockIcon />;
};

const DEFAULT_FORM: LandingPageForm = {
  name: '',
  slug: '',
  title: '',
  metaDescription: '',
  metaKeywords: '',
  template: LandingPageTemplate.LeadCapture,
  customCss: '',
  customJs: '',
  featuredImageUrl: '',
  facebookPixelId: '',
  googleAnalyticsId: '',
  trackingCode: '',
  formDefinitionId: null,
  campaignId: null,
  thankYouPageId: null,
  redirectUrl: '',
  scheduledPublishAt: '',
  scheduledUnpublishAt: '',
  isActive: true,
};

// ==================== MAIN COMPONENT ====================

export default function LandingPagesPage() {
  const { hasPermission, isLoading: profileLoading } = useProfile();
  
  // State
  const [pages, setPages] = useState<LandingPage[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [mainTabIndex, setMainTabIndex] = useState(0);
  
  // Form state
  const [formDialogOpen, setFormDialogOpen] = useState(false);
  const [editingPage, setEditingPage] = useState<LandingPage | null>(null);
  const [form, setForm] = useState<LandingPageForm>(DEFAULT_FORM);
  const [formError, setFormError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [formTabIndex, setFormTabIndex] = useState(0);
  
  // Block designer state
  const [designerOpen, setDesignerOpen] = useState(false);
  const [designerPage, setDesignerPage] = useState<LandingPage | null>(null);
  const [blocks, setBlocks] = useState<LandingPageBlock[]>([]);
  const [selectedBlockIndex, setSelectedBlockIndex] = useState<number | null>(null);
  const [blockEditorOpen, setBlockEditorOpen] = useState(false);
  const [editingBlock, setEditingBlock] = useState<LandingPageBlock | null>(null);
  const [blocksLoading, setBlocksLoading] = useState(false);
  const [blocksSaving, setBlocksSaving] = useState(false);
  
  // Preview state
  const [previewOpen, setPreviewOpen] = useState(false);
  const [previewHtml, setPreviewHtml] = useState('');
  const [previewLoading, setPreviewLoading] = useState(false);
  
  // Analytics state
  const [analyticsOpen, setAnalyticsOpen] = useState(false);
  const [analyticsPage, setAnalyticsPage] = useState<LandingPage | null>(null);
  const [analytics, setAnalytics] = useState<LandingPageAnalytics | null>(null);
  const [analyticsLoading, setAnalyticsLoading] = useState(false);
  
  // Delete confirmation
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [pageToDelete, setPageToDelete] = useState<LandingPage | null>(null);
  const [deleting, setDeleting] = useState(false);

  // Fetch landing pages
  const fetchPages = useCallback(async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/api/landing-pages');
      setPages(response.data);
      setError(null);
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || 'Failed to load landing pages');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchPages();
  }, [fetchPages]);

  // Generate slug from name
  const generateSlug = (name: string) => {
    return name
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-|-$/g, '');
  };

  // Handlers
  const handleCreate = () => {
    setEditingPage(null);
    setForm(DEFAULT_FORM);
    setFormError(null);
    setFormTabIndex(0);
    setFormDialogOpen(true);
  };

  const handleEdit = (page: LandingPage) => {
    setEditingPage(page);
    setForm({
      name: page.name,
      slug: page.slug,
      title: page.title || '',
      metaDescription: page.metaDescription || '',
      metaKeywords: page.metaKeywords || '',
      template: page.template,
      customCss: page.customCss || '',
      customJs: page.customJs || '',
      featuredImageUrl: page.featuredImageUrl || '',
      facebookPixelId: page.facebookPixelId || '',
      googleAnalyticsId: page.googleAnalyticsId || '',
      trackingCode: page.trackingCode || '',
      formDefinitionId: page.formDefinitionId || null,
      campaignId: page.campaignId || null,
      thankYouPageId: page.thankYouPageId || null,
      redirectUrl: page.redirectUrl || '',
      scheduledPublishAt: page.scheduledPublishAt?.split('T')[0] || '',
      scheduledUnpublishAt: page.scheduledUnpublishAt?.split('T')[0] || '',
      isActive: page.isActive,
    });
    setFormError(null);
    setFormTabIndex(0);
    setFormDialogOpen(true);
  };

  const handleFormChange = (field: keyof LandingPageForm, value: any) => {
    setForm(prev => {
      const newForm = { ...prev, [field]: value };
      // Auto-generate slug from name
      if (field === 'name' && !editingPage) {
        newForm.slug = generateSlug(value);
      }
      return newForm;
    });
  };

  const handleSubmit = async () => {
    if (!form.name.trim()) {
      setFormError('Name is required');
      return;
    }
    if (!form.slug.trim()) {
      setFormError('Slug is required');
      return;
    }

    try {
      setSubmitting(true);
      setFormError(null);

      const payload = {
        ...form,
        formDefinitionId: form.formDefinitionId || undefined,
        campaignId: form.campaignId || undefined,
        thankYouPageId: form.thankYouPageId || undefined,
      };

      if (editingPage) {
        await apiClient.put(`/api/landing-pages/${editingPage.id}`, payload);
      } else {
        await apiClient.post('/api/landing-pages', payload);
      }

      setFormDialogOpen(false);
      fetchPages();
    } catch (err: any) {
      setFormError(err?.message || 'Failed to save landing page');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = (page: LandingPage) => {
    setPageToDelete(page);
    setDeleteDialogOpen(true);
  };

  const confirmDelete = async () => {
    if (!pageToDelete) return;

    try {
      setDeleting(true);
      await apiClient.delete(`/api/landing-pages/${pageToDelete.id}`);
      setDeleteDialogOpen(false);
      setPageToDelete(null);
      fetchPages();
    } catch (err: any) {
      setFormError(err?.message || 'Failed to delete landing page');
    } finally {
      setDeleting(false);
    }
  };

  const handlePublish = async (page: LandingPage) => {
    try {
      await apiClient.post(`/api/landing-pages/${page.id}/publish`);
      fetchPages();
    } catch (err: any) {
      setError(err?.message || 'Failed to publish page');
    }
  };

  const handleUnpublish = async (page: LandingPage) => {
    try {
      await apiClient.post(`/api/landing-pages/${page.id}/unpublish`);
      fetchPages();
    } catch (err: any) {
      setError(err?.message || 'Failed to unpublish page');
    }
  };

  const handleDuplicate = async (page: LandingPage) => {
    try {
      await apiClient.post(`/api/landing-pages/${page.id}/duplicate`);
      fetchPages();
    } catch (err: any) {
      setError(err?.message || 'Failed to duplicate page');
    }
  };

  // Block Designer
  const openDesigner = async (page: LandingPage) => {
    setDesignerPage(page);
    setDesignerOpen(true);
    setSelectedBlockIndex(null);

    try {
      setBlocksLoading(true);
      const response = await apiClient.get(`/api/landing-pages/${page.id}/blocks`);
      setBlocks(response.data);
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || 'Failed to load blocks');
    } finally {
      setBlocksLoading(false);
    }
  };

  const addBlock = (blockType: LandingPageBlockType) => {
    const newBlock: LandingPageBlock = {
      blockType,
      sortOrder: blocks.length,
      contentJson: JSON.stringify(getDefaultBlockContent(blockType)),
      isVisible: true,
    };
    setBlocks([...blocks, newBlock]);
    setSelectedBlockIndex(blocks.length);
  };

  const getDefaultBlockContent = (blockType: LandingPageBlockType): Record<string, string> => {
    switch (blockType) {
      case LandingPageBlockType.Hero:
        return { headline: 'Welcome', subheadline: 'Your subheadline here', ctaText: 'Get Started', ctaUrl: '#' };
      case LandingPageBlockType.Text:
        return { content: 'Enter your text here...' };
      case LandingPageBlockType.Image:
        return { src: '', alt: 'Image description' };
      case LandingPageBlockType.Button:
        return { text: 'Click Here', url: '#', style: 'primary' };
      case LandingPageBlockType.Features:
        return { features: JSON.stringify([{ title: 'Feature 1', description: 'Description' }]) };
      case LandingPageBlockType.Testimonial:
        return { quote: 'Great product!', author: 'Customer Name', company: 'Company' };
      case LandingPageBlockType.Video:
        return { url: '', caption: '' };
      case LandingPageBlockType.Banner:
        return { text: 'Banner text', backgroundColor: '#007bff' };
      case LandingPageBlockType.Html:
        return { html: '<!-- Custom HTML -->' };
      case LandingPageBlockType.Form:
        return { formId: '' };
      default:
        return {};
    }
  };

  const moveBlock = (index: number, direction: 'up' | 'down') => {
    const newBlocks = [...blocks];
    const targetIndex = direction === 'up' ? index - 1 : index + 1;
    if (targetIndex < 0 || targetIndex >= blocks.length) return;

    [newBlocks[index], newBlocks[targetIndex]] = [newBlocks[targetIndex], newBlocks[index]];
    newBlocks.forEach((block, i) => {
      block.sortOrder = i;
    });
    setBlocks(newBlocks);
    setSelectedBlockIndex(targetIndex);
  };

  const removeBlock = (index: number) => {
    const newBlocks = blocks.filter((_, i) => i !== index);
    newBlocks.forEach((block, i) => {
      block.sortOrder = i;
    });
    setBlocks(newBlocks);
    setSelectedBlockIndex(null);
  };

  const editBlock = (index: number) => {
    setEditingBlock({ ...blocks[index] });
    setBlockEditorOpen(true);
    setSelectedBlockIndex(index);
  };

  const saveBlockEdit = () => {
    if (editingBlock && selectedBlockIndex !== null) {
      const newBlocks = [...blocks];
      newBlocks[selectedBlockIndex] = editingBlock;
      setBlocks(newBlocks);
    }
    setBlockEditorOpen(false);
    setEditingBlock(null);
  };

  const saveBlocks = async () => {
    if (!designerPage) return;

    try {
      setBlocksSaving(true);
      await apiClient.put(`/api/landing-pages/${designerPage.id}/blocks`, blocks);
      setDesignerOpen(false);
      fetchPages();
    } catch (err: any) {
      setError(err?.message || 'Failed to save blocks');
    } finally {
      setBlocksSaving(false);
    }
  };

  // Preview
  const openPreview = async (page: LandingPage) => {
    try {
      setPreviewLoading(true);
      setPreviewOpen(true);
      const response = await apiClient.get(`/api/landing-pages/${page.id}/preview`);
      setPreviewHtml(response.data.html);
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || 'Failed to generate preview');
      setPreviewOpen(false);
    } finally {
      setPreviewLoading(false);
    }
  };

  // Analytics
  const openAnalytics = async (page: LandingPage) => {
    setAnalyticsPage(page);
    setAnalyticsOpen(true);

    try {
      setAnalyticsLoading(true);
      const response = await apiClient.get(`/api/landing-pages/${page.id}/analytics`);
      setAnalytics(response.data);
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || 'Failed to load analytics');
    } finally {
      setAnalyticsLoading(false);
    }
  };

  // Filtered pages by status
  const draftPages = pages.filter(p => p.status === LandingPageStatus.Draft);
  const publishedPages = pages.filter(p => p.status === LandingPageStatus.Published);
  const archivedPages = pages.filter(p => p.status === LandingPageStatus.Archived || p.status === LandingPageStatus.Scheduled);

  if (profileLoading) {
    return (
      <Container maxWidth="xl" sx={{ py: 3, textAlign: 'center' }}>
        <CircularProgress />
      </Container>
    );
  }

  return (
    <Container maxWidth="xl" sx={{ py: 3 }}>
      {/* Header */}
      <Stack direction="row" alignItems="center" justifyContent="space-between" mb={3}>
        <Stack direction="row" alignItems="center" spacing={2}>
          <Box component="img" src={logo} alt="Logo" sx={{ width: 40, height: 40 }} />
          <Typography variant="h4" fontWeight="bold">Landing Pages</Typography>
        </Stack>
        <Stack direction="row" spacing={2}>
          <Button startIcon={<RefreshIcon />} onClick={fetchPages} disabled={loading}>
            Refresh
          </Button>
          <Button variant="contained" startIcon={<AddIcon />} onClick={handleCreate}>
            Create Page
          </Button>
        </Stack>
      </Stack>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Main Tabs */}
      <Tabs value={mainTabIndex} onChange={(_, v) => setMainTabIndex(v)} sx={{ mb: 2 }}>
        <Tab label={`All Pages (${pages.length})`} />
        <Tab label={`Drafts (${draftPages.length})`} />
        <Tab label={`Published (${publishedPages.length})`} />
        <Tab label={`Archived (${archivedPages.length})`} />
      </Tabs>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : (
        <>
          {/* All Pages */}
          <TabPanel value={mainTabIndex} index={0}>
            <LandingPageTable
              pages={pages}
              onEdit={handleEdit}
              onDelete={handleDelete}
              onPublish={handlePublish}
              onUnpublish={handleUnpublish}
              onDuplicate={handleDuplicate}
              onDesign={openDesigner}
              onPreview={openPreview}
              onAnalytics={openAnalytics}
            />
          </TabPanel>

          {/* Drafts */}
          <TabPanel value={mainTabIndex} index={1}>
            <LandingPageTable
              pages={draftPages}
              onEdit={handleEdit}
              onDelete={handleDelete}
              onPublish={handlePublish}
              onUnpublish={handleUnpublish}
              onDuplicate={handleDuplicate}
              onDesign={openDesigner}
              onPreview={openPreview}
              onAnalytics={openAnalytics}
            />
          </TabPanel>

          {/* Published */}
          <TabPanel value={mainTabIndex} index={2}>
            <LandingPageTable
              pages={publishedPages}
              onEdit={handleEdit}
              onDelete={handleDelete}
              onPublish={handlePublish}
              onUnpublish={handleUnpublish}
              onDuplicate={handleDuplicate}
              onDesign={openDesigner}
              onPreview={openPreview}
              onAnalytics={openAnalytics}
            />
          </TabPanel>

          {/* Archived */}
          <TabPanel value={mainTabIndex} index={3}>
            <LandingPageTable
              pages={archivedPages}
              onEdit={handleEdit}
              onDelete={handleDelete}
              onPublish={handlePublish}
              onUnpublish={handleUnpublish}
              onDuplicate={handleDuplicate}
              onDesign={openDesigner}
              onPreview={openPreview}
              onAnalytics={openAnalytics}
            />
          </TabPanel>
        </>
      )}

      {/* Create/Edit Dialog */}
      <Dialog open={formDialogOpen} onClose={() => setFormDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          {editingPage ? 'Edit Landing Page' : 'Create Landing Page'}
          <IconButton
            onClick={() => setFormDialogOpen(false)}
            sx={{ position: 'absolute', right: 8, top: 8 }}
          >
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        <DialogContent dividers>
          <Tabs value={formTabIndex} onChange={(_, v) => setFormTabIndex(v)} sx={{ mb: 2 }}>
            <Tab label="Basic Info" />
            <Tab label="SEO & Tracking" />
            <Tab label="Form & Campaign" />
            <Tab label="Custom Code" />
          </Tabs>

          {formError && <DialogError error={formError} />}

          {/* Basic Info Tab */}
          <TabPanel value={formTabIndex} index={0}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Name"
                  value={form.name}
                  onChange={e => handleFormChange('name', e.target.value)}
                  required
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="URL Slug"
                  value={form.slug}
                  onChange={e => handleFormChange('slug', e.target.value)}
                  required
                  helperText={`URL: /pages/${form.slug}`}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth>
                  <InputLabel>Template</InputLabel>
                  <Select
                    value={form.template}
                    label="Template"
                    onChange={e => handleFormChange('template', e.target.value)}
                  >
                    {TEMPLATE_OPTIONS.map(opt => (
                      <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Page Title"
                  value={form.title}
                  onChange={e => handleFormChange('title', e.target.value)}
                  helperText="Displayed in browser tab"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Featured Image URL"
                  value={form.featuredImageUrl}
                  onChange={e => handleFormChange('featuredImageUrl', e.target.value)}
                  helperText="Used for social sharing"
                />
              </Grid>
              <Grid item xs={12}>
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={form.isActive}
                      onChange={e => handleFormChange('isActive', e.target.checked)}
                    />
                  }
                  label="Active"
                />
              </Grid>
            </Grid>
          </TabPanel>

          {/* SEO & Tracking Tab */}
          <TabPanel value={formTabIndex} index={1}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Meta Description"
                  value={form.metaDescription}
                  onChange={e => handleFormChange('metaDescription', e.target.value)}
                  multiline
                  rows={2}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Meta Keywords"
                  value={form.metaKeywords}
                  onChange={e => handleFormChange('metaKeywords', e.target.value)}
                  helperText="Comma-separated keywords"
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Facebook Pixel ID"
                  value={form.facebookPixelId}
                  onChange={e => handleFormChange('facebookPixelId', e.target.value)}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Google Analytics ID"
                  value={form.googleAnalyticsId}
                  onChange={e => handleFormChange('googleAnalyticsId', e.target.value)}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Additional Tracking Code"
                  value={form.trackingCode}
                  onChange={e => handleFormChange('trackingCode', e.target.value)}
                  multiline
                  rows={4}
                  helperText="Added to page head"
                />
              </Grid>
            </Grid>
          </TabPanel>

          {/* Form & Campaign Tab */}
          <TabPanel value={formTabIndex} index={2}>
            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  type="number"
                  label="Linked Form ID"
                  value={form.formDefinitionId || ''}
                  onChange={e => handleFormChange('formDefinitionId', e.target.value ? parseInt(e.target.value) : null)}
                  helperText="Enter the Form ID to embed on this page"
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  type="number"
                  label="Campaign ID"
                  value={form.campaignId || ''}
                  onChange={e => handleFormChange('campaignId', e.target.value ? parseInt(e.target.value) : null)}
                  helperText="Link this page to a marketing campaign"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Redirect URL (after form submission)"
                  value={form.redirectUrl}
                  onChange={e => handleFormChange('redirectUrl', e.target.value)}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  type="date"
                  label="Scheduled Publish Date"
                  value={form.scheduledPublishAt}
                  onChange={e => handleFormChange('scheduledPublishAt', e.target.value)}
                  InputLabelProps={{ shrink: true }}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  type="date"
                  label="Scheduled Unpublish Date"
                  value={form.scheduledUnpublishAt}
                  onChange={e => handleFormChange('scheduledUnpublishAt', e.target.value)}
                  InputLabelProps={{ shrink: true }}
                />
              </Grid>
            </Grid>
          </TabPanel>

          {/* Custom Code Tab */}
          <TabPanel value={formTabIndex} index={3}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Custom CSS"
                  value={form.customCss}
                  onChange={e => handleFormChange('customCss', e.target.value)}
                  multiline
                  rows={6}
                  placeholder="/* Your custom CSS */"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Custom JavaScript"
                  value={form.customJs}
                  onChange={e => handleFormChange('customJs', e.target.value)}
                  multiline
                  rows={6}
                  placeholder="// Your custom JavaScript"
                />
              </Grid>
            </Grid>
          </TabPanel>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setFormDialogOpen(false)}>Cancel</Button>
          <ActionButton loading={submitting} onClick={handleSubmit}>
            {editingPage ? 'Update' : 'Create'}
          </ActionButton>
        </DialogActions>
      </Dialog>

      {/* Block Designer Dialog */}
      <Dialog open={designerOpen} onClose={() => setDesignerOpen(false)} maxWidth="xl" fullWidth>
        <DialogTitle>
          Design: {designerPage?.name}
          <IconButton
            onClick={() => setDesignerOpen(false)}
            sx={{ position: 'absolute', right: 8, top: 8 }}
          >
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        <DialogContent dividers>
          <Grid container spacing={2}>
            {/* Block Palette */}
            <Grid item xs={12} md={3}>
              <Paper variant="outlined" sx={{ p: 2 }}>
                <Typography variant="subtitle2" gutterBottom>Add Block</Typography>
                <List dense>
                  {BLOCK_TYPE_OPTIONS.map(opt => (
                    <ListItemButton key={opt.value} onClick={() => addBlock(opt.value)}>
                      <ListItemIcon>{<opt.icon />}</ListItemIcon>
                      <ListItemText primary={opt.label} />
                    </ListItemButton>
                  ))}
                </List>
              </Paper>
            </Grid>

            {/* Block Canvas */}
            <Grid item xs={12} md={9}>
              <Paper variant="outlined" sx={{ p: 2, minHeight: 400 }}>
                {blocksLoading ? (
                  <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
                    <CircularProgress />
                  </Box>
                ) : blocks.length === 0 ? (
                  <Box sx={{ textAlign: 'center', py: 4, color: 'text.secondary' }}>
                    <Typography>No blocks yet. Add blocks from the palette.</Typography>
                  </Box>
                ) : (
                  <Stack spacing={1}>
                    {blocks.map((block, index) => (
                      <Paper
                        key={index}
                        variant="outlined"
                        sx={{
                          p: 2,
                          cursor: 'pointer',
                          bgcolor: selectedBlockIndex === index ? 'action.selected' : 'background.paper',
                          '&:hover': { bgcolor: 'action.hover' },
                        }}
                        onClick={() => setSelectedBlockIndex(index)}
                      >
                        <Stack direction="row" alignItems="center" justifyContent="space-between">
                          <Stack direction="row" alignItems="center" spacing={1}>
                            <DragIcon sx={{ color: 'text.secondary' }} />
                            {getBlockIcon(block.blockType)}
                            <Typography variant="body2">{getBlockTypeName(block.blockType)}</Typography>
                            {!block.isVisible && <Chip size="small" label="Hidden" variant="outlined" />}
                          </Stack>
                          <Stack direction="row" spacing={0.5}>
                            <Tooltip title="Move Up">
                              <IconButton size="small" onClick={e => { e.stopPropagation(); moveBlock(index, 'up'); }} disabled={index === 0}>
                                <ArrowUpward fontSize="small" />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Move Down">
                              <IconButton size="small" onClick={e => { e.stopPropagation(); moveBlock(index, 'down'); }} disabled={index === blocks.length - 1}>
                                <ArrowDownward fontSize="small" />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Edit">
                              <IconButton size="small" onClick={e => { e.stopPropagation(); editBlock(index); }}>
                                <EditIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Delete">
                              <IconButton size="small" color="error" onClick={e => { e.stopPropagation(); removeBlock(index); }}>
                                <DeleteIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                          </Stack>
                        </Stack>
                      </Paper>
                    ))}
                  </Stack>
                )}
              </Paper>
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDesignerOpen(false)}>Cancel</Button>
          <ActionButton loading={blocksSaving} onClick={saveBlocks} variant="contained">
            Save Blocks
          </ActionButton>
        </DialogActions>
      </Dialog>

      {/* Block Editor Dialog */}
      <Dialog open={blockEditorOpen} onClose={() => setBlockEditorOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Edit Block</DialogTitle>
        <DialogContent dividers>
          {editingBlock && (
            <Stack spacing={2}>
              <Typography variant="body2" color="text.secondary">
                Block Type: {getBlockTypeName(editingBlock.blockType)}
              </Typography>
              <TextField
                fullWidth
                label="Content (JSON)"
                value={editingBlock.contentJson}
                onChange={e => setEditingBlock({ ...editingBlock, contentJson: e.target.value })}
                multiline
                rows={8}
              />
              <TextField
                fullWidth
                label="Style (JSON)"
                value={editingBlock.styleJson || ''}
                onChange={e => setEditingBlock({ ...editingBlock, styleJson: e.target.value })}
                multiline
                rows={4}
              />
              <FormControlLabel
                control={
                  <Checkbox
                    checked={editingBlock.isVisible}
                    onChange={e => setEditingBlock({ ...editingBlock, isVisible: e.target.checked })}
                  />
                }
                label="Visible"
              />
            </Stack>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setBlockEditorOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={saveBlockEdit}>Save</Button>
        </DialogActions>
      </Dialog>

      {/* Preview Dialog */}
      <Dialog open={previewOpen} onClose={() => setPreviewOpen(false)} maxWidth="lg" fullWidth>
        <DialogTitle>
          Preview
          <IconButton
            onClick={() => setPreviewOpen(false)}
            sx={{ position: 'absolute', right: 8, top: 8 }}
          >
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        <DialogContent dividers sx={{ p: 0, height: '70vh' }}>
          {previewLoading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%' }}>
              <CircularProgress />
            </Box>
          ) : (
            <iframe
              srcDoc={previewHtml}
              style={{ width: '100%', height: '100%', border: 'none' }}
              title="Landing Page Preview"
            />
          )}
        </DialogContent>
      </Dialog>

      {/* Analytics Dialog */}
      <Dialog open={analyticsOpen} onClose={() => setAnalyticsOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          Analytics: {analyticsPage?.name}
          <IconButton
            onClick={() => setAnalyticsOpen(false)}
            sx={{ position: 'absolute', right: 8, top: 8 }}
          >
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        <DialogContent dividers>
          {analyticsLoading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
              <CircularProgress />
            </Box>
          ) : analytics ? (
            <Grid container spacing={3}>
              <Grid item xs={12} md={3}>
                <Card>
                  <CardContent sx={{ textAlign: 'center' }}>
                    <Typography variant="h4" color="primary">{analytics.totalViews}</Typography>
                    <Typography variant="body2" color="text.secondary">Total Views</Typography>
                  </CardContent>
                </Card>
              </Grid>
              <Grid item xs={12} md={3}>
                <Card>
                  <CardContent sx={{ textAlign: 'center' }}>
                    <Typography variant="h4" color="primary">{analytics.uniqueVisitors}</Typography>
                    <Typography variant="body2" color="text.secondary">Unique Visitors</Typography>
                  </CardContent>
                </Card>
              </Grid>
              <Grid item xs={12} md={3}>
                <Card>
                  <CardContent sx={{ textAlign: 'center' }}>
                    <Typography variant="h4" color="success.main">{analytics.totalConversions}</Typography>
                    <Typography variant="body2" color="text.secondary">Conversions</Typography>
                  </CardContent>
                </Card>
              </Grid>
              <Grid item xs={12} md={3}>
                <Card>
                  <CardContent sx={{ textAlign: 'center' }}>
                    <Typography variant="h4" color="info.main">{analytics.conversionRate.toFixed(1)}%</Typography>
                    <Typography variant="body2" color="text.secondary">Conversion Rate</Typography>
                  </CardContent>
                </Card>
              </Grid>
              <Grid item xs={12} md={6}>
                <Card>
                  <CardContent>
                    <Typography variant="subtitle2" gutterBottom>Device Breakdown</Typography>
                    {analytics.deviceBreakdown.map(d => (
                      <Box key={d.deviceType} sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                        <Typography variant="body2">{d.deviceType}</Typography>
                        <Typography variant="body2" fontWeight="bold">{d.count}</Typography>
                      </Box>
                    ))}
                  </CardContent>
                </Card>
              </Grid>
              <Grid item xs={12} md={6}>
                <Card>
                  <CardContent>
                    <Typography variant="subtitle2" gutterBottom>Top Referrers</Typography>
                    {analytics.topReferrers.slice(0, 5).map(r => (
                      <Box key={r.referrer} sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                        <Typography variant="body2" noWrap sx={{ maxWidth: 200 }}>{r.referrer || 'Direct'}</Typography>
                        <Typography variant="body2" fontWeight="bold">{r.count}</Typography>
                      </Box>
                    ))}
                  </CardContent>
                </Card>
              </Grid>
            </Grid>
          ) : (
            <Typography>No analytics data available</Typography>
          )}
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Delete Landing Page</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete "{pageToDelete?.name}"? This action cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
          <ActionButton loading={deleting} onClick={confirmDelete} color="error">
            Delete
          </ActionButton>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

// ==================== LANDING PAGE TABLE COMPONENT ====================

interface LandingPageTableProps {
  pages: LandingPage[];
  onEdit: (page: LandingPage) => void;
  onDelete: (page: LandingPage) => void;
  onPublish: (page: LandingPage) => void;
  onUnpublish: (page: LandingPage) => void;
  onDuplicate: (page: LandingPage) => void;
  onDesign: (page: LandingPage) => void;
  onPreview: (page: LandingPage) => void;
  onAnalytics: (page: LandingPage) => void;
}

function LandingPageTable({
  pages,
  onEdit,
  onDelete,
  onPublish,
  onUnpublish,
  onDuplicate,
  onDesign,
  onPreview,
  onAnalytics,
}: LandingPageTableProps) {
  if (pages.length === 0) {
    return (
      <Paper variant="outlined" sx={{ p: 4, textAlign: 'center' }}>
        <WebIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
        <Typography variant="h6">No landing pages found</Typography>
        <Typography color="text.secondary">Create your first landing page to get started.</Typography>
      </Paper>
    );
  }

  return (
    <Card>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>Name</TableCell>
            <TableCell>Slug</TableCell>
            <TableCell>Template</TableCell>
            <TableCell>Status</TableCell>
            <TableCell align="right">Views</TableCell>
            <TableCell align="right">Conversions</TableCell>
            <TableCell>Created</TableCell>
            <TableCell align="right">Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {pages.map(page => (
            <TableRow key={page.id} hover>
              <TableCell>
                <Stack direction="row" alignItems="center" spacing={1}>
                  <WebIcon fontSize="small" color="primary" />
                  <Typography fontWeight="medium">{page.name}</Typography>
                </Stack>
              </TableCell>
              <TableCell>
                <Chip size="small" label={`/${page.slug}`} variant="outlined" />
              </TableCell>
              <TableCell>{getTemplateName(page.template)}</TableCell>
              <TableCell>{getStatusChip(page.status)}</TableCell>
              <TableCell align="right">{page.pageViews.toLocaleString()}</TableCell>
              <TableCell align="right">
                {page.conversions}
                {page.pageViews > 0 && (
                  <Typography variant="caption" color="text.secondary" sx={{ ml: 1 }}>
                    ({((page.conversions / page.pageViews) * 100).toFixed(1)}%)
                  </Typography>
                )}
              </TableCell>
              <TableCell>{new Date(page.createdAt).toLocaleDateString()}</TableCell>
              <TableCell align="right">
                <Stack direction="row" spacing={0.5} justifyContent="flex-end">
                  <Tooltip title="Preview">
                    <IconButton size="small" onClick={() => onPreview(page)}>
                      <PreviewIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title="Design">
                    <IconButton size="small" onClick={() => onDesign(page)}>
                      <EditIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title="Analytics">
                    <IconButton size="small" onClick={() => onAnalytics(page)}>
                      <AnalyticsIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  {page.status === LandingPageStatus.Draft && (
                    <Tooltip title="Publish">
                      <IconButton size="small" color="success" onClick={() => onPublish(page)}>
                        <PublishIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  )}
                  {page.status === LandingPageStatus.Published && (
                    <Tooltip title="Unpublish">
                      <IconButton size="small" color="warning" onClick={() => onUnpublish(page)}>
                        <UnpublishIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  )}
                  <Tooltip title="Duplicate">
                    <IconButton size="small" onClick={() => onDuplicate(page)}>
                      <CopyIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title="Settings">
                    <IconButton size="small" onClick={() => onEdit(page)}>
                      <WebIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title="Delete">
                    <IconButton size="small" color="error" onClick={() => onDelete(page)}>
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </Stack>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Card>
  );
}
