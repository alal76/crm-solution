// Article Feedback Widget - Helpful/not helpful with comments
// Part of ITSM Enhancement Plan - Phase 3.1

import React, { useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  Button,
  TextField,
  Stack,
  IconButton,
  Chip,
  Divider,
  Collapse,
  Alert,
  Rating,
  Tooltip,
  LinearProgress,
} from '@mui/material';
import {
  ThumbUp as HelpfulIcon,
  ThumbUpOutlined as HelpfulOutlineIcon,
  ThumbDown as NotHelpfulIcon,
  ThumbDownOutlined as NotHelpfulOutlineIcon,
  Send as SendIcon,
  Close as CloseIcon,
  CheckCircle as SuccessIcon,
  Star as StarIcon,
} from '@mui/icons-material';

export interface FeedbackStats {
  helpfulCount: number;
  notHelpfulCount: number;
  totalVotes: number;
  helpfulPercentage: number;
}

export interface ArticleFeedbackWidgetProps {
  articleId: number;
  stats?: FeedbackStats;
  userFeedback?: 'helpful' | 'not_helpful' | null;
  onSubmitFeedback?: (feedback: {
    isHelpful: boolean;
    comment?: string;
    rating?: number;
  }) => Promise<void>;
  showStats?: boolean;
  showRating?: boolean;
  compact?: boolean;
  usedToResolveIncident?: boolean;
}

export const ArticleFeedbackWidget: React.FC<ArticleFeedbackWidgetProps> = ({
  articleId,
  stats,
  userFeedback: initialFeedback = null,
  onSubmitFeedback,
  showStats = true,
  showRating = false,
  compact = false,
  usedToResolveIncident = false,
}) => {
  const [userFeedback, setUserFeedback] = useState<'helpful' | 'not_helpful' | null>(
    initialFeedback
  );
  const [showCommentForm, setShowCommentForm] = useState(false);
  const [comment, setComment] = useState('');
  const [rating, setRating] = useState<number | null>(null);
  const [loading, setLoading] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [pendingFeedback, setPendingFeedback] = useState<'helpful' | 'not_helpful' | null>(null);

  const handleFeedbackClick = (isHelpful: boolean) => {
    const feedbackType = isHelpful ? 'helpful' : 'not_helpful';
    
    // If clicking the same button, toggle off
    if (userFeedback === feedbackType) {
      setUserFeedback(null);
      setPendingFeedback(null);
      setShowCommentForm(false);
      return;
    }

    setPendingFeedback(feedbackType);
    
    // For not helpful, always show comment form
    if (!isHelpful) {
      setShowCommentForm(true);
    } else {
      // For helpful, submit immediately unless rating is enabled
      if (!showRating) {
        submitFeedback(true, '');
      } else {
        setShowCommentForm(true);
      }
    }
  };

  const submitFeedback = async (isHelpful: boolean, feedbackComment: string) => {
    setLoading(true);
    try {
      await onSubmitFeedback?.({
        isHelpful,
        comment: feedbackComment || undefined,
        rating: rating || undefined,
      });
      setUserFeedback(isHelpful ? 'helpful' : 'not_helpful');
      setSubmitted(true);
      setShowCommentForm(false);
      setPendingFeedback(null);
      
      // Reset submitted state after showing success
      setTimeout(() => setSubmitted(false), 3000);
    } catch (error) {
      console.error('Failed to submit feedback:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmitWithComment = () => {
    if (pendingFeedback) {
      submitFeedback(pendingFeedback === 'helpful', comment);
    }
  };

  const handleCancel = () => {
    setShowCommentForm(false);
    setPendingFeedback(null);
    setComment('');
    setRating(null);
  };

  if (compact) {
    return (
      <Stack direction="row" alignItems="center" spacing={1}>
        <Typography variant="caption" color="text.secondary">
          Was this helpful?
        </Typography>
        <Tooltip title="Yes, helpful">
          <IconButton
            size="small"
            onClick={() => handleFeedbackClick(true)}
            color={userFeedback === 'helpful' ? 'success' : 'default'}
          >
            {userFeedback === 'helpful' ? (
              <HelpfulIcon fontSize="small" />
            ) : (
              <HelpfulOutlineIcon fontSize="small" />
            )}
          </IconButton>
        </Tooltip>
        <Tooltip title="No, not helpful">
          <IconButton
            size="small"
            onClick={() => handleFeedbackClick(false)}
            color={userFeedback === 'not_helpful' ? 'error' : 'default'}
          >
            {userFeedback === 'not_helpful' ? (
              <NotHelpfulIcon fontSize="small" />
            ) : (
              <NotHelpfulOutlineIcon fontSize="small" />
            )}
          </IconButton>
        </Tooltip>
        {stats && showStats && (
          <Chip
            size="small"
            label={`${stats.helpfulCount} found helpful`}
            sx={{ height: 20, fontSize: '0.65rem' }}
          />
        )}
      </Stack>
    );
  }

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      {/* Success message */}
      <Collapse in={submitted}>
        <Alert
          severity="success"
          icon={<SuccessIcon />}
          sx={{ mb: 2 }}
          onClose={() => setSubmitted(false)}
        >
          Thank you for your feedback!
        </Alert>
      </Collapse>

      {/* Stats bar */}
      {stats && showStats && !submitted && (
        <Box sx={{ mb: 2 }}>
          <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 0.5 }}>
            <Typography variant="caption" color="text.secondary">
              {stats.totalVotes} {stats.totalVotes === 1 ? 'person' : 'people'} rated this article
            </Typography>
            <Typography variant="caption" fontWeight={600} color="success.main">
              {stats.helpfulPercentage}% found helpful
            </Typography>
          </Stack>
          <LinearProgress
            variant="determinate"
            value={stats.helpfulPercentage}
            sx={{
              height: 6,
              borderRadius: 3,
              backgroundColor: '#f4433620',
              '& .MuiLinearProgress-bar': {
                backgroundColor: '#4caf50',
                borderRadius: 3,
              },
            }}
          />
        </Box>
      )}

      {/* Used to resolve indicator */}
      {usedToResolveIncident && (
        <Alert severity="success" sx={{ mb: 2, py: 0 }}>
          <Typography variant="caption">
            This article was used to resolve the incident
          </Typography>
        </Alert>
      )}

      {/* Main feedback question */}
      {!submitted && (
        <>
          <Typography variant="subtitle2" fontWeight={600} sx={{ mb: 1.5 }}>
            Was this article helpful?
          </Typography>

          <Stack direction="row" spacing={2} sx={{ mb: 2 }}>
            <Button
              variant={userFeedback === 'helpful' || pendingFeedback === 'helpful' ? 'contained' : 'outlined'}
              color="success"
              startIcon={
                userFeedback === 'helpful' ? <HelpfulIcon /> : <HelpfulOutlineIcon />
              }
              onClick={() => handleFeedbackClick(true)}
              disabled={loading}
            >
              Yes, helpful
            </Button>
            <Button
              variant={userFeedback === 'not_helpful' || pendingFeedback === 'not_helpful' ? 'contained' : 'outlined'}
              color="error"
              startIcon={
                userFeedback === 'not_helpful' ? <NotHelpfulIcon /> : <NotHelpfulOutlineIcon />
              }
              onClick={() => handleFeedbackClick(false)}
              disabled={loading}
            >
              No, not helpful
            </Button>
          </Stack>
        </>
      )}

      {/* Comment form */}
      <Collapse in={showCommentForm}>
        <Divider sx={{ mb: 2 }} />

        {showRating && pendingFeedback === 'helpful' && (
          <Box sx={{ mb: 2 }}>
            <Typography variant="body2" sx={{ mb: 0.5 }}>
              Rate this article:
            </Typography>
            <Rating
              value={rating}
              onChange={(_, newValue) => setRating(newValue)}
              icon={<StarIcon sx={{ color: '#ffc107' }} />}
              emptyIcon={<StarIcon sx={{ color: '#e0e0e0' }} />}
            />
          </Box>
        )}

        <TextField
          fullWidth
          multiline
          rows={3}
          placeholder={
            pendingFeedback === 'helpful'
              ? 'What did you find helpful? (Optional)'
              : 'Please tell us how we can improve this article...'
          }
          value={comment}
          onChange={(e) => setComment(e.target.value)}
          disabled={loading}
          sx={{ mb: 2 }}
          required={pendingFeedback === 'not_helpful'}
          helperText={
            pendingFeedback === 'not_helpful'
              ? 'Please provide feedback to help us improve'
              : ''
          }
        />

        <Stack direction="row" justifyContent="flex-end" spacing={1}>
          <Button onClick={handleCancel} disabled={loading}>
            Cancel
          </Button>
          <Button
            variant="contained"
            startIcon={<SendIcon />}
            onClick={handleSubmitWithComment}
            disabled={loading || (pendingFeedback === 'not_helpful' && !comment.trim())}
          >
            {loading ? 'Submitting...' : 'Submit Feedback'}
          </Button>
        </Stack>
      </Collapse>

      {/* Already submitted state */}
      {userFeedback && !showCommentForm && !submitted && (
        <Box sx={{ mt: 2, pt: 2, borderTop: '1px solid #e0e0e0' }}>
          <Stack direction="row" alignItems="center" spacing={1}>
            <SuccessIcon color="success" fontSize="small" />
            <Typography variant="body2" color="text.secondary">
              You marked this article as{' '}
              <strong>{userFeedback === 'helpful' ? 'helpful' : 'not helpful'}</strong>
            </Typography>
          </Stack>
        </Box>
      )}
    </Paper>
  );
};

export default ArticleFeedbackWidget;
