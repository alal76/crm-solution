/**
 * FeedbackForm - Customer satisfaction feedback form for service requests
 * Captures star rating, comment, and recommendation preference
 */

import React, { useState, useCallback } from 'react';
import {
  Box,
  Button,
  TextField,
  Typography,
  Rating,
  Switch,
  FormControlLabel,
  Paper,
} from '@mui/material';
import {
  Send as SendIcon,
  Cancel as CancelIcon,
  Star as StarIcon,
} from '@mui/icons-material';

export interface FeedbackData {
  rating: number;
  comment?: string;
  wouldRecommend: boolean;
}

export interface FeedbackFormProps {
  serviceRequestId: number;
  onSubmit: (data: FeedbackData) => void;
  onCancel: () => void;
}

const ratingLabels: Record<number, string> = {
  1: 'Very Dissatisfied',
  2: 'Dissatisfied',
  3: 'Neutral',
  4: 'Satisfied',
  5: 'Very Satisfied',
};

const FeedbackForm: React.FC<FeedbackFormProps> = ({
  serviceRequestId,
  onSubmit,
  onCancel,
}) => {
  const [rating, setRating] = useState<number | null>(null);
  const [comment, setComment] = useState('');
  const [wouldRecommend, setWouldRecommend] = useState(true);
  const [ratingError, setRatingError] = useState(false);

  const handleSubmit = useCallback(
    (e: React.FormEvent) => {
      e.preventDefault();

      if (!rating || rating < 1) {
        setRatingError(true);
        return;
      }

      setRatingError(false);
      onSubmit({
        rating,
        comment: comment.trim() || undefined,
        wouldRecommend,
      });
    },
    [rating, comment, wouldRecommend, onSubmit]
  );

  return (
    <Paper elevation={0} sx={{ p: 2, border: '1px solid', borderColor: 'divider' }}>
      <Typography variant="h6" gutterBottom>
        Service Feedback
      </Typography>
      <Typography variant="body2" color="text.secondary" gutterBottom>
        How was your experience with service request #{serviceRequestId}?
      </Typography>

      <Box component="form" onSubmit={handleSubmit} noValidate sx={{ mt: 2 }}>
        <Box mb={2}>
          <Typography component="legend" variant="subtitle2" gutterBottom>
            Overall Rating *
          </Typography>
          <Box display="flex" alignItems="center" gap={2}>
            <Rating
              value={rating}
              onChange={(_, newValue) => {
                setRating(newValue);
                if (newValue) setRatingError(false);
              }}
              precision={1}
              size="large"
              emptyIcon={<StarIcon style={{ opacity: 0.3 }} fontSize="inherit" />}
            />
            {rating !== null && (
              <Typography variant="body2" color="text.secondary">
                {ratingLabels[rating] ?? ''}
              </Typography>
            )}
          </Box>
          {ratingError && (
            <Typography variant="caption" color="error" sx={{ mt: 0.5, display: 'block' }}>
              Please provide a rating
            </Typography>
          )}
        </Box>

        <TextField
          label="Comments"
          multiline
          rows={3}
          fullWidth
          value={comment}
          onChange={(e) => setComment(e.target.value)}
          placeholder="Tell us more about your experience..."
          sx={{ mb: 2 }}
        />

        <FormControlLabel
          control={
            <Switch
              checked={wouldRecommend}
              onChange={(e) => setWouldRecommend(e.target.checked)}
            />
          }
          label="I would recommend this service to others"
          sx={{ mb: 2, display: 'block' }}
        />

        <Box display="flex" justifyContent="flex-end" gap={1}>
          <Button variant="outlined" onClick={onCancel} startIcon={<CancelIcon />}>
            Cancel
          </Button>
          <Button
            variant="contained"
            type="submit"
            color="primary"
            startIcon={<SendIcon />}
          >
            Submit Feedback
          </Button>
        </Box>
      </Box>
    </Paper>
  );
};

export default FeedbackForm;
