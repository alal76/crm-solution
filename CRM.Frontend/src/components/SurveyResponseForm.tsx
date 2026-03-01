// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// Public-facing survey response form for CSAT / NPS / CES surveys.
// Renders a star rating (1-5), a comment textarea, and a submit button.
import React, { useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  CircularProgress,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import StarIcon from '@mui/icons-material/Star';
import StarBorderIcon from '@mui/icons-material/StarBorder';
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';
import satisfactionService, {
  SurveyType,
} from '../services/satisfactionService';

// ── Helpers ────────────────────────────────────────────────────────────────────

const CSAT_LABELS: Record<number, string> = {
  1: 'Very Dissatisfied',
  2: 'Dissatisfied',
  3: 'Neutral',
  4: 'Satisfied',
  5: 'Very Satisfied',
};

const NPS_SCALE = Array.from({ length: 11 }, (_, i) => i); // 0–10

function csatLabel(score: number | null): string {
  if (score === null) return '';
  return CSAT_LABELS[score] ?? '';
}

// ── Component ─────────────────────────────────────────────────────────────────

export interface SurveyResponseFormProps {
  /** Numeric survey ID (used only if surveyToken is not provided). */
  surveyId?: number;
  /** Public external token from the survey invite link. */
  surveyToken?: string;
  /** Numeric survey type enum value or string label. */
  surveyType?: SurveyType | 'CSAT' | 'NPS' | 'CES';
  /** Optional callback fired after a successful submission. */
  onSubmit?: () => void;
}

const SurveyResponseForm: React.FC<SurveyResponseFormProps> = ({
  surveyToken,
  surveyType = SurveyType.CSAT,
  onSubmit,
}) => {
  const [score, setScore] = useState<number | null>(null);
  const [hovered, setHovered] = useState<number | null>(null);
  const [comment, setComment] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Resolve type to enum number
  const resolvedType: SurveyType =
    typeof surveyType === 'string'
      ? surveyType === 'NPS'
        ? SurveyType.NPS
        : surveyType === 'CES'
          ? SurveyType.CES
          : SurveyType.CSAT
      : surveyType;

  const isNPS = resolvedType === SurveyType.NPS;
  const isCES = resolvedType === SurveyType.CES;

  const maxScore = isNPS ? 10 : isCES ? 7 : 5;
  const minScore = isNPS ? 0 : 1;

  const title =
    isNPS
      ? 'How likely are you to recommend us? (0–10)'
      : isCES
        ? 'How easy was it to resolve your issue? (1–7)'
        : 'How satisfied are you with our service?';

  const handleSubmit = async () => {
    if (score === null) {
      setError('Please select a score before submitting.');
      return;
    }
    if (!surveyToken) {
      setError('Survey token is missing. Please use the survey link from your email.');
      return;
    }
    setSubmitting(true);
    setError(null);
    try {
      await satisfactionService.submitResponse({
        surveyToken,
        score,
        comment: comment.trim() || undefined,
      });
      setSubmitted(true);
      onSubmit?.();
    } catch {
      setError('Failed to submit your response. Please try again.');
    } finally {
      setSubmitting(false);
    }
  };

  // ── Thank-you screen ────────────────────────────────────────────────────────

  if (submitted) {
    return (
      <Card sx={{ maxWidth: 480, mx: 'auto', mt: 4, textAlign: 'center' }}>
        <CardContent>
          <CheckCircleOutlineIcon sx={{ fontSize: 64, color: 'success.main', mb: 1 }} />
          <Typography variant="h5" gutterBottom>
            Thank you for your feedback!
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Your response has been recorded and helps us improve.
          </Typography>
        </CardContent>
      </Card>
    );
  }

  // ── Main form ───────────────────────────────────────────────────────────────

  return (
    <Card sx={{ maxWidth: 480, mx: 'auto', mt: 4 }}>
      <CardContent>
        <Stack spacing={3}>
          {/* Title */}
          <Box textAlign="center">
            <Typography variant="h5" gutterBottom fontWeight={600}>
              How did we do?
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {title}
            </Typography>
          </Box>

          {/* Score selector */}
          {isNPS ? (
            // NPS: numeric buttons 0–10
            <Box>
              <Stack direction="row" spacing={0.5} flexWrap="wrap" justifyContent="center">
                {NPS_SCALE.map((n) => (
                  <Button
                    key={n}
                    variant={score === n ? 'contained' : 'outlined'}
                    size="small"
                    onClick={() => setScore(n)}
                    sx={{ minWidth: 40, px: 0 }}
                  >
                    {n}
                  </Button>
                ))}
              </Stack>
              <Stack direction="row" justifyContent="space-between" mt={0.5}>
                <Typography variant="caption" color="text.secondary">
                  Not likely
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  Very likely
                </Typography>
              </Stack>
            </Box>
          ) : (
            // CSAT (1-5) / CES (1-7): star-style buttons
            <Box textAlign="center">
              <Stack direction="row" spacing={1} justifyContent="center">
                {Array.from({ length: maxScore - minScore + 1 }, (_, i) => i + minScore).map(
                  (val) => {
                    const isActive = hovered !== null ? val <= hovered : score !== null && val <= score;
                    return (
                      <Tooltip key={val} title={isCES ? `${val}` : csatLabel(val)}>
                        <Box
                          component="span"
                          onMouseEnter={() => setHovered(val)}
                          onMouseLeave={() => setHovered(null)}
                          onClick={() => setScore(val)}
                          sx={{ cursor: 'pointer', color: isActive ? 'warning.main' : 'action.disabled', fontSize: 40 }}
                        >
                          {isActive ? (
                            <StarIcon fontSize="inherit" />
                          ) : (
                            <StarBorderIcon fontSize="inherit" />
                          )}
                        </Box>
                      </Tooltip>
                    );
                  },
                )}
              </Stack>
              {score !== null && !isNPS && (
                <Typography variant="body2" color="text.secondary" mt={0.5}>
                  {isCES ? `${score} / ${maxScore}` : csatLabel(score)}
                </Typography>
              )}
            </Box>
          )}

          {/* Comment */}
          <TextField
            multiline
            minRows={3}
            maxRows={6}
            label="Additional comments (optional)"
            placeholder="Tell us more about your experience…"
            value={comment}
            onChange={(e) => setComment(e.target.value)}
            inputProps={{ maxLength: 1000 }}
            fullWidth
          />

          {/* Error */}
          {error && <Alert severity="error">{error}</Alert>}

          {/* Submit */}
          <Button
            variant="contained"
            size="large"
            fullWidth
            disabled={submitting || score === null}
            onClick={handleSubmit}
            startIcon={submitting ? <CircularProgress size={18} color="inherit" /> : undefined}
          >
            {submitting ? 'Submitting…' : 'Submit Feedback'}
          </Button>
        </Stack>
      </CardContent>
    </Card>
  );
};

export default SurveyResponseForm;
