// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
import React, { useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Divider,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import StarIcon from '@mui/icons-material/Star';
import StarBorderIcon from '@mui/icons-material/StarBorder';
import satisfactionService, {
  SatisfactionSurveyDto,
  SurveyType,
} from '../../services/satisfactionService';

interface Props {
  /** The pre-loaded survey object (load by token before rendering). */
  survey: SatisfactionSurveyDto;
  /** The raw token from the URL — used as the surveyToken in the API call. */
  token: string;
  /** Called once the response is submitted successfully. */
  onSubmitted?: () => void;
}

/**
 * Public-facing survey response form supporting NPS (0-10) and CSAT (1-5 stars).
 */
const SurveyResponseForm: React.FC<Props> = ({ survey, token, onSubmitted }) => {
  const [score, setScore] = useState<number | null>(null);
  const [comment, setComment] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async () => {
    if (score === null) return;
    setSubmitting(true);
    setError(null);
    try {
      await satisfactionService.submitResponse({
        surveyToken: token,
        score,
        comment: comment || undefined,
      });
      setSubmitted(true);
      onSubmitted?.();
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { error?: string } } })?.response?.data?.error ??
        'Failed to submit your response. Please try again.';
      setError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  if (submitted) {
    return (
      <Card variant="outlined">
        <CardContent>
          <Stack spacing={2} alignItems="center" py={4}>
            <Typography variant="h5">Thank you! 🎉</Typography>
            <Typography color="text.secondary">
              Your feedback has been recorded and will help us improve.
            </Typography>
          </Stack>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card variant="outlined">
      <CardContent>
        <Stack spacing={3}>
          {/* Header */}
          <Box>
            <Typography variant="h6">{survey.subject ?? 'How was your experience?'}</Typography>
            <Typography variant="body2" color="text.secondary">
              {survey.type === SurveyType.NPS
                ? 'On a scale of 0–10, how likely are you to recommend us to a friend or colleague?'
                : survey.type === SurveyType.CSAT
                ? 'How satisfied were you with our service today?'
                : 'How much effort did it take to resolve your issue?'}
            </Typography>
          </Box>

          <Divider />

          {/* Score picker */}
          {survey.type === SurveyType.NPS ? (
            <NPSPicker value={score} onChange={setScore} />
          ) : (
            <StarPicker value={score} onChange={setScore} max={survey.type === SurveyType.CES ? 7 : 5} />
          )}

          {/* Comment */}
          <TextField
            label="Additional comments (optional)"
            multiline
            rows={3}
            value={comment}
            onChange={(e) => setComment(e.target.value)}
            inputProps={{ maxLength: 1000 }}
            helperText={`${comment.length}/1000`}
          />

          {error && <Alert severity="error">{error}</Alert>}

          <Button
            variant="contained"
            size="large"
            disabled={score === null || submitting}
            onClick={handleSubmit}
            startIcon={submitting ? <CircularProgress size={16} color="inherit" /> : undefined}
          >
            {submitting ? 'Submitting…' : 'Submit Feedback'}
          </Button>
        </Stack>
      </CardContent>
    </Card>
  );
};

// ── Sub-components ─────────────────────────────────────────────────────────────

interface NPSPickerProps {
  value: number | null;
  onChange: (v: number) => void;
}

const NPSPicker: React.FC<NPSPickerProps> = ({ value, onChange }) => {
  const labels: Record<number, string> = { 0: 'Not at all', 5: 'Neutral', 10: 'Extremely likely' };
  return (
    <Box>
      <Stack direction="row" spacing={0.5} flexWrap="wrap" useFlexGap>
        {Array.from({ length: 11 }, (_, i) => (
          <Tooltip key={i} title={labels[i] ?? ''} placement="top">
            <Chip
              label={i}
              onClick={() => onChange(i)}
              color={value === i ? (i >= 9 ? 'success' : i >= 7 ? 'warning' : 'error') : 'default'}
              variant={value === i ? 'filled' : 'outlined'}
              sx={{ minWidth: 40, cursor: 'pointer' }}
            />
          </Tooltip>
        ))}
      </Stack>
      <Stack direction="row" justifyContent="space-between" mt={0.5}>
        <Typography variant="caption" color="text.secondary">Not at all likely</Typography>
        <Typography variant="caption" color="text.secondary">Extremely likely</Typography>
      </Stack>
    </Box>
  );
};

interface StarPickerProps {
  value: number | null;
  onChange: (v: number) => void;
  max: number;
}

const StarPicker: React.FC<StarPickerProps> = ({ value, onChange, max }) => (
  <Stack direction="row" spacing={0.5} justifyContent="center">
    {Array.from({ length: max }, (_, i) => {
      const starValue = i + 1;
      const filled = value !== null && starValue <= value;
      return (
        <Box
          key={starValue}
          onClick={() => onChange(starValue)}
          sx={{ cursor: 'pointer', color: filled ? 'warning.main' : 'action.disabled', fontSize: 40 }}
        >
          {filled ? <StarIcon fontSize="inherit" /> : <StarBorderIcon fontSize="inherit" />}
        </Box>
      );
    })}
  </Stack>
);

export default SurveyResponseForm;
