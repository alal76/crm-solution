// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
import React, { useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Divider,
  FormControlLabel,
  LinearProgress,
  Step,
  StepLabel,
  Stepper,
  Switch,
  TextField,
  Typography,
} from '@mui/material';
import apiClient from '../../services/apiClient';

// ─── Types ────────────────────────────────────────────────────────────────────

interface BANTData {
  hasBudget: boolean;
  hasAuthority: boolean;
  hasNeed: boolean;
  hasTimeline: boolean;
}

interface MEDDICData {
  metrics: string;
  economicBuyer: string;
  decisionCriteria: string;
  decisionProcess: string;
  identifyPain: string;
  champion: string;
}

interface LeadQualificationResult {
  leadId: number;
  framework: string;
  combinedScore: number;
  qualificationLevel: string;
  dimensionScores: Record<string, number>;
  recommendations: string[];
  scoredAt: string;
}

interface LeadQualificationPanelProps {
  leadId: number;
  onScored?: (result: LeadQualificationResult) => void;
}

// ─── Helpers ──────────────────────────────────────────────────────────────────

const STEPS = ['BANT Scoring', 'MEDDIC Scoring', 'Results'];

function scoreColor(score: number): 'success' | 'warning' | 'error' {
  if (score >= 70) return 'success';
  if (score >= 40) return 'warning';
  return 'error';
}

// ─── Component ────────────────────────────────────────────────────────────────

/**
 * LeadQualificationPanel — two-step wizard for BANT + MEDDIC lead scoring.
 * Calls POST /api/leads/{leadId}/qualify with the collected data.
 * (TODO-CRM002-08)
 */
const LeadQualificationPanel: React.FC<LeadQualificationPanelProps> = ({
  leadId,
  onScored,
}) => {
  const [activeStep, setActiveStep] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<LeadQualificationResult | null>(null);

  const [bant, setBant] = useState<BANTData>({
    hasBudget: false,
    hasAuthority: false,
    hasNeed: false,
    hasTimeline: false,
  });

  const [meddic, setMeddic] = useState<MEDDICData>({
    metrics: '',
    economicBuyer: '',
    decisionCriteria: '',
    decisionProcess: '',
    identifyPain: '',
    champion: '',
  });

  // ─── Handlers ───────────────────────────────────────────────────────────

  const handleNext = () => setActiveStep((s) => s + 1);
  const handleBack = () => setActiveStep((s) => s - 1);

  const handleSubmit = async () => {
    setLoading(true);
    setError(null);
    try {
      const payload = { ...bant, ...meddic };
      const response = await apiClient.post<LeadQualificationResult>(
        `/leads/${leadId}/qualify`,
        payload,
      );
      setResult(response.data);
      setActiveStep(2);
      onScored?.(response.data);
    } catch {
      setError('Qualification failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  // ─── Step content ────────────────────────────────────────────────────────

  const bantStep = (
    <Box sx={{ mt: 2 }}>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        Answer each BANT dimension to assess budget, authority, need, and timeline.
      </Typography>
      {(['hasBudget', 'hasAuthority', 'hasNeed', 'hasTimeline'] as const).map((field) => {
        const labels: Record<string, string> = {
          hasBudget: 'Has confirmed budget',
          hasAuthority: 'Has decision-making authority',
          hasNeed: 'Has an identified business need',
          hasTimeline: 'Has a defined buying timeline',
        };
        return (
          <FormControlLabel
            key={field}
            control={
              <Switch
                checked={bant[field]}
                onChange={(e) => setBant((prev) => ({ ...prev, [field]: e.target.checked }))}
              />
            }
            label={labels[field]}
            sx={{ display: 'block', mb: 1 }}
          />
        );
      })}
    </Box>
  );

  const meddicStep = (
    <Box sx={{ mt: 2 }}>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        Provide evidence for each MEDDIC dimension. Leave blank if unknown.
      </Typography>
      {(
        [
          ['metrics', 'Metrics — quantifiable value the customer will achieve'],
          ['economicBuyer', 'Economic Buyer — who controls the budget'],
          ['decisionCriteria', 'Decision Criteria — how will they evaluate solutions'],
          ['decisionProcess', 'Decision Process — steps to approval'],
          ['identifyPain', 'Identify Pain — critical business problem'],
          ['champion', 'Champion — internal advocate for your solution'],
        ] as [keyof MEDDICData, string][]
      ).map(([field, label]) => (
        <TextField
          key={field}
          label={label}
          value={meddic[field]}
          onChange={(e) => setMeddic((prev) => ({ ...prev, [field]: e.target.value }))}
          fullWidth
          size="small"
          multiline
          rows={2}
          sx={{ mb: 2 }}
        />
      ))}
    </Box>
  );

  const resultsStep = result && (
    <Box sx={{ mt: 2 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
        <Typography variant="h4" fontWeight={700}>
          {result.combinedScore}
        </Typography>
        <Box>
          <Chip
            label={result.qualificationLevel}
            color={scoreColor(result.combinedScore)}
            size="small"
          />
          <Typography variant="caption" display="block" color="text.secondary">
            {result.framework} Framework
          </Typography>
        </Box>
      </Box>

      <LinearProgress
        variant="determinate"
        value={result.combinedScore}
        color={scoreColor(result.combinedScore)}
        sx={{ height: 8, borderRadius: 4, mb: 3 }}
      />

      <Typography variant="subtitle2" gutterBottom>
        Dimension Scores
      </Typography>
      {Object.entries(result.dimensionScores).map(([dim, score]) => (
        <Box key={dim} sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
          <Typography variant="body2" sx={{ minWidth: 160 }}>
            {dim}
          </Typography>
          <LinearProgress
            variant="determinate"
            value={score}
            color={scoreColor(score)}
            sx={{ flex: 1, height: 6, borderRadius: 3 }}
          />
          <Typography variant="caption" sx={{ minWidth: 32, textAlign: 'right' }}>
            {score}
          </Typography>
        </Box>
      ))}

      {result.recommendations.length > 0 && (
        <>
          <Divider sx={{ my: 2 }} />
          <Typography variant="subtitle2" gutterBottom>
            Recommendations
          </Typography>
          {result.recommendations.map((rec, i) => (
            <Alert key={i} severity="info" sx={{ mb: 1 }} icon={false}>
              {rec}
            </Alert>
          ))}
        </>
      )}
    </Box>
  );

  // ─── Render ──────────────────────────────────────────────────────────────

  return (
    <Box>
      <Stepper activeStep={activeStep} sx={{ mb: 3 }}>
        {STEPS.map((label) => (
          <Step key={label}>
            <StepLabel>{label}</StepLabel>
          </Step>
        ))}
      </Stepper>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {activeStep === 0 && bantStep}
      {activeStep === 1 && meddicStep}
      {activeStep === 2 && resultsStep}

      <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 3 }}>
        <Button onClick={handleBack} disabled={activeStep === 0 || activeStep === 2}>
          Back
        </Button>
        {activeStep === 0 && <Button variant="contained" onClick={handleNext}>Next</Button>}
        {activeStep === 1 && (
          <Button
            variant="contained"
            onClick={() => void handleSubmit()}
            disabled={loading}
            startIcon={loading ? <CircularProgress size={16} /> : undefined}
          >
            {loading ? 'Qualifying…' : 'Submit & Score'}
          </Button>
        )}
      </Box>
    </Box>
  );
};

export default LeadQualificationPanel;
