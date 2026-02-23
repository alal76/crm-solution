// Publish Workflow - Article publish workflow status and actions
// Part of Knowledge Base Enhancement - Phase 3

import React from 'react';
import {
  Box,
  Typography,
  Paper,
  Chip,
  Button,
  Stepper,
  Step,
  StepLabel,
  Stack,
  Skeleton,
  Divider,
} from '@mui/material';
import {
  Edit as DraftIcon,
  RateReview as ReviewIcon,
  CheckCircle as ApprovedIcon,
  Public as PublishedIcon,
  Archive as ArchivedIcon,
  ArrowForward as ArrowIcon,
} from '@mui/icons-material';

export type ArticlePublishStatus = 'Draft' | 'InReview' | 'Approved' | 'Published' | 'Archived';

export interface PublishWorkflowProps {
  currentStatus: ArticlePublishStatus;
  onStatusChange: (newStatus: string) => void;
  author?: string;
  reviewedBy?: string;
  lastPublished?: string;
  loading?: boolean;
}

const WORKFLOW_STEPS: ArticlePublishStatus[] = ['Draft', 'InReview', 'Approved', 'Published'];

const STATUS_CONFIG: Record<ArticlePublishStatus, { label: string; color: 'default' | 'primary' | 'warning' | 'success' | 'info' | 'error'; icon: React.ReactElement }> = {
  Draft: { label: 'Draft', color: 'default', icon: <DraftIcon fontSize="small" /> },
  InReview: { label: 'In Review', color: 'warning', icon: <ReviewIcon fontSize="small" /> },
  Approved: { label: 'Approved', color: 'info', icon: <ApprovedIcon fontSize="small" /> },
  Published: { label: 'Published', color: 'success', icon: <PublishedIcon fontSize="small" /> },
  Archived: { label: 'Archived', color: 'error', icon: <ArchivedIcon fontSize="small" /> },
};

const getValidTransitions = (status: ArticlePublishStatus): ArticlePublishStatus[] => {
  switch (status) {
    case 'Draft':
      return ['InReview'];
    case 'InReview':
      return ['Approved', 'Draft'];
    case 'Approved':
      return ['Published', 'Draft'];
    case 'Published':
      return ['Archived', 'Draft'];
    case 'Archived':
      return ['Draft'];
    default:
      return [];
  }
};

const getActiveStep = (status: ArticlePublishStatus): number => {
  const index = WORKFLOW_STEPS.indexOf(status);
  return index >= 0 ? index : -1;
};

const PublishWorkflow: React.FC<PublishWorkflowProps> = ({
  currentStatus,
  onStatusChange,
  author,
  reviewedBy,
  lastPublished,
  loading = false,
}) => {
  if (loading) {
    return (
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Skeleton variant="rectangular" height={32} sx={{ mb: 2, borderRadius: 1 }} />
        <Skeleton variant="rectangular" height={60} sx={{ mb: 2, borderRadius: 1 }} />
        <Skeleton variant="rectangular" height={36} sx={{ borderRadius: 1 }} />
      </Paper>
    );
  }

  const config = STATUS_CONFIG[currentStatus];
  const transitions = getValidTransitions(currentStatus);
  const activeStep = getActiveStep(currentStatus);
  const isArchived = currentStatus === 'Archived';

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
        <Typography variant="subtitle2" color="text.secondary">
          Publish Workflow
        </Typography>
        <Chip
          icon={config.icon}
          label={config.label}
          color={config.color}
          size="small"
          sx={{ fontWeight: 'bold' }}
        />
      </Stack>

      {!isArchived && (
        <Stepper activeStep={activeStep} alternativeLabel sx={{ mb: 2 }}>
          {WORKFLOW_STEPS.map((step) => {
            const stepConfig = STATUS_CONFIG[step];
            return (
              <Step key={step} completed={WORKFLOW_STEPS.indexOf(step) < activeStep}>
                <StepLabel>{stepConfig.label}</StepLabel>
              </Step>
            );
          })}
        </Stepper>
      )}

      {isArchived && (
        <Box sx={{ textAlign: 'center', py: 1, mb: 2, backgroundColor: 'action.hover', borderRadius: 1 }}>
          <ArchivedIcon sx={{ fontSize: 24, color: 'text.disabled', mb: 0.5 }} />
          <Typography variant="body2" color="text.secondary">
            This article has been archived
          </Typography>
        </Box>
      )}

      <Stack direction="row" spacing={1} sx={{ mb: 2 }} flexWrap="wrap" useFlexGap>
        {transitions.map((target) => {
          const targetConfig = STATUS_CONFIG[target];
          const isPrimary = target !== 'Draft' && target !== 'Archived';
          return (
            <Button
              key={target}
              variant={isPrimary ? 'contained' : 'outlined'}
              size="small"
              color={isPrimary ? 'primary' : 'inherit'}
              startIcon={targetConfig.icon}
              endIcon={isPrimary ? <ArrowIcon fontSize="small" /> : undefined}
              onClick={() => onStatusChange(target)}
            >
              {target === 'Draft' ? 'Return to Draft' : targetConfig.label}
            </Button>
          );
        })}
        {currentStatus !== 'Archived' && (
          <Button
            variant="outlined"
            size="small"
            color="error"
            startIcon={<ArchivedIcon fontSize="small" />}
            onClick={() => onStatusChange('Archived')}
          >
            Archive
          </Button>
        )}
      </Stack>

      <Divider sx={{ my: 1.5 }} />

      <Stack spacing={0.5}>
        {author && (
          <Typography variant="caption" color="text.secondary">
            <strong>Author:</strong> {author}
          </Typography>
        )}
        {reviewedBy && (
          <Typography variant="caption" color="text.secondary">
            <strong>Reviewed by:</strong> {reviewedBy}
          </Typography>
        )}
        {lastPublished && (
          <Typography variant="caption" color="text.secondary">
            <strong>Last published:</strong> {new Date(lastPublished).toLocaleString()}
          </Typography>
        )}
      </Stack>
    </Paper>
  );
};

export default PublishWorkflow;
