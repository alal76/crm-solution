// Risk Assessment Form - Guided risk assessment wizard for change requests
// Part of ITSM Enhancement Plan - Phase 2.2

import React, { useState, useMemo, useCallback } from 'react';
import {
  Box,
  Paper,
  Typography,
  Stepper,
  Step,
  StepLabel,
  StepContent,
  Button,
  Stack,
  FormControl,
  FormLabel,
  RadioGroup,
  FormControlLabel,
  Radio,
  TextField,
  Chip,
  Alert,
  Divider,
  Slider,
  Card,
  CardContent,
  LinearProgress,
  Tooltip,
  IconButton,
} from '@mui/material';
import {
  Warning as WarningIcon,
  CheckCircle as LowRiskIcon,
  Error as HighRiskIcon,
  Help as HelpIcon,
  ArrowBack as BackIcon,
  ArrowForward as NextIcon,
  Save as SaveIcon,
} from '@mui/icons-material';

export type RiskLevel = 'low' | 'medium' | 'high' | 'critical';

export interface RiskFactor {
  id: string;
  category: string;
  question: string;
  description?: string;
  options: {
    value: number;
    label: string;
    description?: string;
  }[];
}

export interface RiskAnswer {
  factorId: string;
  value: number;
  notes?: string;
}

export interface RiskAssessmentResult {
  overallRisk: RiskLevel;
  riskScore: number;
  maxScore: number;
  categoryScores: { category: string; score: number; maxScore: number }[];
  answers: RiskAnswer[];
  mitigationRequired: boolean;
  recommendations: string[];
  assessedAt: Date;
}

export interface RiskAssessmentFormProps {
  changeRequestId?: number;
  existingAnswers?: RiskAnswer[];
  customFactors?: RiskFactor[];
  onComplete?: (result: RiskAssessmentResult) => void;
  onSaveDraft?: (answers: RiskAnswer[]) => void;
  readOnly?: boolean;
}

// Default ITIL-aligned risk factors
const DEFAULT_RISK_FACTORS: RiskFactor[] = [
  {
    id: 'impact_scope',
    category: 'Impact',
    question: 'What is the scope of systems/services affected?',
    description: 'Consider all dependent systems and services',
    options: [
      { value: 1, label: 'Single system/component', description: 'No dependencies' },
      { value: 2, label: 'Multiple systems', description: 'Few direct dependencies' },
      { value: 3, label: 'Business service', description: 'Customer-facing impact possible' },
      { value: 4, label: 'Multiple services/Enterprise-wide', description: 'Critical business impact' },
    ],
  },
  {
    id: 'impact_users',
    category: 'Impact',
    question: 'How many users will be affected?',
    options: [
      { value: 1, label: 'Less than 10 users' },
      { value: 2, label: '10-100 users' },
      { value: 3, label: '100-1000 users' },
      { value: 4, label: 'More than 1000 users or external customers' },
    ],
  },
  {
    id: 'impact_downtime',
    category: 'Impact',
    question: 'Expected service interruption?',
    options: [
      { value: 1, label: 'No downtime', description: 'Zero impact to availability' },
      { value: 2, label: 'Less than 30 minutes' },
      { value: 3, label: '30 minutes to 4 hours' },
      { value: 4, label: 'More than 4 hours' },
    ],
  },
  {
    id: 'complexity_technical',
    category: 'Complexity',
    question: 'How complex is the technical implementation?',
    options: [
      { value: 1, label: 'Standard/routine change', description: 'Well-documented procedure' },
      { value: 2, label: 'Moderately complex', description: 'Some custom work required' },
      { value: 3, label: 'Complex', description: 'Multiple technologies involved' },
      { value: 4, label: 'Highly complex', description: 'Novel or experimental' },
    ],
  },
  {
    id: 'complexity_rollback',
    category: 'Complexity',
    question: 'How difficult is the rollback/backout plan?',
    options: [
      { value: 1, label: 'Automatic/simple rollback', description: 'One-click or scripted' },
      { value: 2, label: 'Manual rollback possible', description: 'Clear steps documented' },
      { value: 3, label: 'Partial rollback', description: 'Some manual intervention' },
      { value: 4, label: 'No rollback possible', description: 'Forward-fix required' },
    ],
  },
  {
    id: 'experience_team',
    category: 'Experience',
    question: 'Team experience with this type of change?',
    options: [
      { value: 1, label: 'Highly experienced', description: 'Done many times before' },
      { value: 2, label: 'Some experience', description: 'Similar changes completed' },
      { value: 3, label: 'Limited experience', description: 'First time for team' },
      { value: 4, label: 'No experience', description: 'New technology/process' },
    ],
  },
  {
    id: 'testing_level',
    category: 'Testing',
    question: 'What level of testing has been completed?',
    options: [
      { value: 1, label: 'Full testing cycle', description: 'Including UAT and performance' },
      { value: 2, label: 'Standard testing', description: 'Functional and integration tests' },
      { value: 3, label: 'Limited testing', description: 'Basic functional tests only' },
      { value: 4, label: 'Minimal/no testing', description: 'Emergency change' },
    ],
  },
  {
    id: 'timing_window',
    category: 'Timing',
    question: 'When is the change scheduled?',
    options: [
      { value: 1, label: 'Standard maintenance window', description: 'Low traffic period' },
      { value: 2, label: 'Off-peak hours', description: 'Outside business hours' },
      { value: 3, label: 'Business hours', description: 'Normal working hours' },
      { value: 4, label: 'Peak/critical period', description: 'High traffic or business-critical time' },
    ],
  },
];

const getRiskLevel = (percentage: number): RiskLevel => {
  if (percentage <= 25) return 'low';
  if (percentage <= 50) return 'medium';
  if (percentage <= 75) return 'high';
  return 'critical';
};

const getRiskColor = (level: RiskLevel): string => {
  switch (level) {
    case 'low':
      return '#4caf50';
    case 'medium':
      return '#ff9800';
    case 'high':
      return '#f44336';
    case 'critical':
      return '#9c27b0';
    default:
      return '#9e9e9e';
  }
};

const getRecommendations = (categoryScores: { category: string; score: number; maxScore: number }[]): string[] => {
  const recommendations: string[] = [];
  
  categoryScores.forEach(({ category, score, maxScore }) => {
    const percentage = (score / maxScore) * 100;
    if (percentage > 50) {
      switch (category) {
        case 'Impact':
          recommendations.push('Consider breaking down the change into smaller, less impactful phases');
          recommendations.push('Ensure stakeholder communication plan is in place');
          break;
        case 'Complexity':
          recommendations.push('Document detailed step-by-step implementation procedures');
          recommendations.push('Prepare comprehensive rollback scripts and verify they work');
          break;
        case 'Experience':
          recommendations.push('Consider involving subject matter experts or vendors');
          recommendations.push('Schedule additional team training before implementation');
          break;
        case 'Testing':
          recommendations.push('Expand test coverage before proceeding');
          recommendations.push('Consider a phased rollout or pilot deployment');
          break;
        case 'Timing':
          recommendations.push('Reschedule to a lower-risk maintenance window if possible');
          recommendations.push('Ensure additional support resources are available');
          break;
      }
    }
  });
  
  return [...new Set(recommendations)];
};

export const RiskAssessmentForm: React.FC<RiskAssessmentFormProps> = ({
  changeRequestId,
  existingAnswers = [],
  customFactors,
  onComplete,
  onSaveDraft,
  readOnly = false,
}) => {
  const factors = customFactors || DEFAULT_RISK_FACTORS;
  const categories = [...new Set(factors.map((f) => f.category))];

  // Initialize answers from existing or empty
  const [answers, setAnswers] = useState<RiskAnswer[]>(() => {
    if (existingAnswers.length > 0) return existingAnswers;
    return factors.map((f) => ({ factorId: f.id, value: 0 }));
  });

  const [activeStep, setActiveStep] = useState(0);
  const [showResult, setShowResult] = useState(false);

  // Calculate current risk score
  const riskResult = useMemo((): RiskAssessmentResult => {
    let totalScore = 0;
    let maxScore = 0;
    const categoryScores: { category: string; score: number; maxScore: number }[] = [];

    categories.forEach((category) => {
      const categoryFactors = factors.filter((f) => f.category === category);
      let catScore = 0;
      let catMax = 0;

      categoryFactors.forEach((factor) => {
        const answer = answers.find((a) => a.factorId === factor.id);
        const factorMax = Math.max(...factor.options.map((o) => o.value));
        catMax += factorMax;
        if (answer && answer.value > 0) {
          catScore += answer.value;
        }
      });

      categoryScores.push({ category, score: catScore, maxScore: catMax });
      totalScore += catScore;
      maxScore += catMax;
    });

    const percentage = maxScore > 0 ? (totalScore / maxScore) * 100 : 0;
    const overallRisk = getRiskLevel(percentage);
    const recommendations = getRecommendations(categoryScores);

    return {
      overallRisk,
      riskScore: totalScore,
      maxScore,
      categoryScores,
      answers,
      mitigationRequired: overallRisk === 'high' || overallRisk === 'critical',
      recommendations,
      assessedAt: new Date(),
    };
  }, [answers, factors, categories]);

  const handleAnswerChange = useCallback((factorId: string, value: number) => {
    setAnswers((prev) =>
      prev.map((a) =>
        a.factorId === factorId ? { ...a, value } : a
      )
    );
  }, []);

  const handleNotesChange = useCallback((factorId: string, notes: string) => {
    setAnswers((prev) =>
      prev.map((a) =>
        a.factorId === factorId ? { ...a, notes } : a
      )
    );
  }, []);

  const handleNext = () => {
    if (activeStep < categories.length - 1) {
      setActiveStep(activeStep + 1);
    } else {
      setShowResult(true);
    }
  };

  const handleBack = () => {
    if (showResult) {
      setShowResult(false);
    } else if (activeStep > 0) {
      setActiveStep(activeStep - 1);
    }
  };

  const handleComplete = () => {
    onComplete?.(riskResult);
  };

  const handleSaveDraft = () => {
    onSaveDraft?.(answers);
  };

  // Check if current category is complete
  const isCategoryComplete = (categoryIndex: number) => {
    const category = categories[categoryIndex];
    const categoryFactors = factors.filter((f) => f.category === category);
    return categoryFactors.every((factor) => {
      const answer = answers.find((a) => a.factorId === factor.id);
      return answer && answer.value > 0;
    });
  };

  const isAllComplete = categories.every((_, index) => isCategoryComplete(index));
  const progressPercentage = riskResult.maxScore > 0
    ? (riskResult.riskScore / riskResult.maxScore) * 100
    : 0;

  return (
    <Paper sx={{ p: 3 }}>
      <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 3 }}>
        <Typography variant="h6">
          Risk Assessment
          {changeRequestId && (
            <Typography component="span" variant="body2" color="text.secondary" sx={{ ml: 1 }}>
              (CR-{changeRequestId})
            </Typography>
          )}
        </Typography>
        {!readOnly && onSaveDraft && (
          <Button startIcon={<SaveIcon />} onClick={handleSaveDraft} size="small">
            Save Draft
          </Button>
        )}
      </Stack>

      {/* Progress indicator */}
      <Box sx={{ mb: 3 }}>
        <Stack direction="row" justifyContent="space-between" sx={{ mb: 1 }}>
          <Typography variant="body2" color="text.secondary">
            Current Risk Score
          </Typography>
          <Chip
            label={riskResult.overallRisk.toUpperCase()}
            size="small"
            sx={{
              backgroundColor: getRiskColor(riskResult.overallRisk),
              color: 'white',
            }}
          />
        </Stack>
        <LinearProgress
          variant="determinate"
          value={progressPercentage}
          sx={{
            height: 10,
            borderRadius: 5,
            backgroundColor: '#e0e0e0',
            '& .MuiLinearProgress-bar': {
              backgroundColor: getRiskColor(riskResult.overallRisk),
            },
          }}
        />
        <Typography variant="caption" color="text.secondary">
          {riskResult.riskScore} / {riskResult.maxScore} points
        </Typography>
      </Box>

      <Divider sx={{ mb: 3 }} />

      {showResult ? (
        // Results view
        <Box>
          <Alert
            severity={riskResult.mitigationRequired ? 'error' : 'success'}
            icon={riskResult.mitigationRequired ? <HighRiskIcon /> : <LowRiskIcon />}
            sx={{ mb: 3 }}
          >
            <Typography variant="subtitle1" fontWeight={600}>
              Overall Risk: {riskResult.overallRisk.toUpperCase()}
            </Typography>
            {riskResult.mitigationRequired && (
              <Typography variant="body2">
                This change requires additional review and mitigation strategies.
              </Typography>
            )}
          </Alert>

          {/* Category breakdown */}
          <Typography variant="subtitle2" sx={{ mb: 2 }}>
            Category Breakdown
          </Typography>
          <Stack spacing={2} sx={{ mb: 3 }}>
            {riskResult.categoryScores.map(({ category, score, maxScore }) => {
              const pct = maxScore > 0 ? (score / maxScore) * 100 : 0;
              const level = getRiskLevel(pct);
              return (
                <Box key={category}>
                  <Stack direction="row" justifyContent="space-between" sx={{ mb: 0.5 }}>
                    <Typography variant="body2">{category}</Typography>
                    <Typography variant="body2">
                      {score}/{maxScore}
                    </Typography>
                  </Stack>
                  <LinearProgress
                    variant="determinate"
                    value={pct}
                    sx={{
                      height: 6,
                      borderRadius: 3,
                      backgroundColor: '#e0e0e0',
                      '& .MuiLinearProgress-bar': {
                        backgroundColor: getRiskColor(level),
                      },
                    }}
                  />
                </Box>
              );
            })}
          </Stack>

          {/* Recommendations */}
          {riskResult.recommendations.length > 0 && (
            <Box sx={{ mb: 3 }}>
              <Typography variant="subtitle2" sx={{ mb: 1 }}>
                Recommendations
              </Typography>
              <Stack spacing={1}>
                {riskResult.recommendations.map((rec, index) => (
                  <Alert key={index} severity="info" icon={<WarningIcon />}>
                    {rec}
                  </Alert>
                ))}
              </Stack>
            </Box>
          )}

          <Stack direction="row" spacing={2}>
            <Button variant="outlined" startIcon={<BackIcon />} onClick={handleBack}>
              Review Answers
            </Button>
            {!readOnly && (
              <Button
                variant="contained"
                onClick={handleComplete}
                color={riskResult.mitigationRequired ? 'warning' : 'primary'}
              >
                {riskResult.mitigationRequired ? 'Submit for Review' : 'Complete Assessment'}
              </Button>
            )}
          </Stack>
        </Box>
      ) : (
        // Stepper view
        <Stepper activeStep={activeStep} orientation="vertical">
          {categories.map((category, index) => {
            const categoryFactors = factors.filter((f) => f.category === category);
            return (
              <Step key={category} completed={isCategoryComplete(index)}>
                <StepLabel>
                  <Stack direction="row" alignItems="center" spacing={1}>
                    <Typography>{category}</Typography>
                    {isCategoryComplete(index) && (
                      <Chip label="Complete" size="small" color="success" sx={{ height: 20 }} />
                    )}
                  </Stack>
                </StepLabel>
                <StepContent>
                  <Stack spacing={3}>
                    {categoryFactors.map((factor) => {
                      const answer = answers.find((a) => a.factorId === factor.id);
                      return (
                        <Card key={factor.id} variant="outlined">
                          <CardContent>
                            <FormControl component="fieldset" disabled={readOnly} fullWidth>
                              <Stack direction="row" alignItems="flex-start" spacing={1}>
                                <FormLabel component="legend" sx={{ fontWeight: 500 }}>
                                  {factor.question}
                                </FormLabel>
                                {factor.description && (
                                  <Tooltip title={factor.description}>
                                    <IconButton size="small">
                                      <HelpIcon fontSize="small" />
                                    </IconButton>
                                  </Tooltip>
                                )}
                              </Stack>
                              <RadioGroup
                                value={answer?.value || 0}
                                onChange={(e) =>
                                  handleAnswerChange(factor.id, Number(e.target.value))
                                }
                              >
                                {factor.options.map((option) => (
                                  <FormControlLabel
                                    key={option.value}
                                    value={option.value}
                                    control={<Radio size="small" />}
                                    label={
                                      <Stack>
                                        <Typography variant="body2">{option.label}</Typography>
                                        {option.description && (
                                          <Typography variant="caption" color="text.secondary">
                                            {option.description}
                                          </Typography>
                                        )}
                                      </Stack>
                                    }
                                    sx={{ alignItems: 'flex-start', py: 0.5 }}
                                  />
                                ))}
                              </RadioGroup>
                              <TextField
                                size="small"
                                placeholder="Additional notes (optional)"
                                value={answer?.notes || ''}
                                onChange={(e) => handleNotesChange(factor.id, e.target.value)}
                                sx={{ mt: 2 }}
                                disabled={readOnly}
                              />
                            </FormControl>
                          </CardContent>
                        </Card>
                      );
                    })}
                  </Stack>

                  <Stack direction="row" spacing={2} sx={{ mt: 2 }}>
                    <Button disabled={index === 0} onClick={handleBack}>
                      Back
                    </Button>
                    <Button
                      variant="contained"
                      onClick={handleNext}
                      endIcon={<NextIcon />}
                    >
                      {index === categories.length - 1 ? 'View Results' : 'Next'}
                    </Button>
                  </Stack>
                </StepContent>
              </Step>
            );
          })}
        </Stepper>
      )}
    </Paper>
  );
};

export default RiskAssessmentForm;
