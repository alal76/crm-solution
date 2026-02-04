// Root Cause Analysis Template - 5-Whys form for problem analysis
// Part of ITSM Enhancement Plan - Phase 1.2

import React, { useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  TextField,
  Button,
  Stack,
  IconButton,
  Chip,
  Divider,
  Alert,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Tooltip,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  ExpandMore as ExpandIcon,
  Lightbulb as IdeaIcon,
  CheckCircle as SolutionIcon,
  Warning as ProblemIcon,
  Save as SaveIcon,
  Refresh as ResetIcon,
  HelpOutline as HelpIcon,
} from '@mui/icons-material';

export interface WhyStep {
  id: number;
  question: string;
  answer: string;
  evidence?: string;
}

export interface RootCauseAnalysis {
  problemStatement: string;
  whySteps: WhyStep[];
  rootCause: string;
  proposedSolution: string;
  workaround?: string;
  preventiveMeasures?: string;
}

export interface RootCauseAnalysisTemplateProps {
  initialData?: Partial<RootCauseAnalysis>;
  onSave?: (data: RootCauseAnalysis) => void;
  readOnly?: boolean;
  problemDescription?: string;
}

const DEFAULT_WHY_STEPS: WhyStep[] = [
  { id: 1, question: 'Why did the problem occur?', answer: '', evidence: '' },
  { id: 2, question: 'Why? (based on answer 1)', answer: '', evidence: '' },
  { id: 3, question: 'Why? (based on answer 2)', answer: '', evidence: '' },
  { id: 4, question: 'Why? (based on answer 3)', answer: '', evidence: '' },
  { id: 5, question: 'Why? (based on answer 4)', answer: '', evidence: '' },
];

const EXAMPLE_ANALYSIS: RootCauseAnalysis = {
  problemStatement: 'Web application response time increased to 10+ seconds',
  whySteps: [
    {
      id: 1,
      question: 'Why did the problem occur?',
      answer: 'Database queries are taking too long',
      evidence: 'Query logs show 8+ second execution times',
    },
    {
      id: 2,
      question: 'Why are database queries slow?',
      answer: 'Missing indexes on frequently queried columns',
      evidence: 'Execution plan shows full table scans',
    },
    {
      id: 3,
      question: 'Why are indexes missing?',
      answer: 'New feature added without database optimization review',
      evidence: 'Recent deployment added new query patterns',
    },
    {
      id: 4,
      question: 'Why was there no optimization review?',
      answer: 'No mandatory database review step in deployment process',
      evidence: 'Deployment checklist missing DB review',
    },
    {
      id: 5,
      question: 'Why is there no mandatory review?',
      answer: 'Process was never established for database changes',
      evidence: 'No documented procedure exists',
    },
  ],
  rootCause: 'Lack of mandatory database performance review in the deployment process',
  proposedSolution: 'Add database review step to deployment checklist and create indexes',
  workaround: 'Add temporary indexes to improve immediate performance',
  preventiveMeasures: 'Implement automated query performance testing in CI/CD pipeline',
};

export const RootCauseAnalysisTemplate: React.FC<RootCauseAnalysisTemplateProps> = ({
  initialData,
  onSave,
  readOnly = false,
  problemDescription,
}) => {
  const [problemStatement, setProblemStatement] = useState(
    initialData?.problemStatement || problemDescription || ''
  );
  const [whySteps, setWhySteps] = useState<WhyStep[]>(
    initialData?.whySteps || [...DEFAULT_WHY_STEPS]
  );
  const [rootCause, setRootCause] = useState(initialData?.rootCause || '');
  const [proposedSolution, setProposedSolution] = useState(
    initialData?.proposedSolution || ''
  );
  const [workaround, setWorkaround] = useState(initialData?.workaround || '');
  const [preventiveMeasures, setPreventiveMeasures] = useState(
    initialData?.preventiveMeasures || ''
  );
  const [showExample, setShowExample] = useState(false);

  const handleWhyChange = (id: number, field: 'answer' | 'evidence', value: string) => {
    setWhySteps((prev) =>
      prev.map((step) => (step.id === id ? { ...step, [field]: value } : step))
    );
  };

  const handleAddWhyStep = () => {
    const newId = Math.max(...whySteps.map((s) => s.id)) + 1;
    setWhySteps((prev) => [
      ...prev,
      {
        id: newId,
        question: `Why? (based on answer ${prev.length})`,
        answer: '',
        evidence: '',
      },
    ]);
  };

  const handleRemoveWhyStep = (id: number) => {
    if (whySteps.length <= 1) return;
    setWhySteps((prev) => prev.filter((step) => step.id !== id));
  };

  const handleReset = () => {
    setProblemStatement(problemDescription || '');
    setWhySteps([...DEFAULT_WHY_STEPS]);
    setRootCause('');
    setProposedSolution('');
    setWorkaround('');
    setPreventiveMeasures('');
  };

  const handleLoadExample = () => {
    setProblemStatement(EXAMPLE_ANALYSIS.problemStatement);
    setWhySteps([...EXAMPLE_ANALYSIS.whySteps]);
    setRootCause(EXAMPLE_ANALYSIS.rootCause);
    setProposedSolution(EXAMPLE_ANALYSIS.proposedSolution);
    setWorkaround(EXAMPLE_ANALYSIS.workaround || '');
    setPreventiveMeasures(EXAMPLE_ANALYSIS.preventiveMeasures || '');
    setShowExample(false);
  };

  const handleSave = () => {
    onSave?.({
      problemStatement,
      whySteps,
      rootCause,
      proposedSolution,
      workaround,
      preventiveMeasures,
    });
  };

  const isComplete =
    problemStatement.trim() &&
    whySteps.every((s) => s.answer.trim()) &&
    rootCause.trim() &&
    proposedSolution.trim();

  const completionPercentage = () => {
    let completed = 0;
    let total = 4; // problem, root cause, solution, at least one why

    if (problemStatement.trim()) completed++;
    if (rootCause.trim()) completed++;
    if (proposedSolution.trim()) completed++;
    if (whySteps.some((s) => s.answer.trim())) completed++;

    return Math.round((completed / total) * 100);
  };

  return (
    <Paper variant="outlined" sx={{ p: 3 }}>
      {/* Header */}
      <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 3 }}>
        <Stack direction="row" alignItems="center" spacing={1}>
          <IdeaIcon color="primary" />
          <Typography variant="h6">Root Cause Analysis (5 Whys)</Typography>
        </Stack>
        <Stack direction="row" spacing={1}>
          <Chip
            label={`${completionPercentage()}% Complete`}
            color={isComplete ? 'success' : 'warning'}
            size="small"
          />
          {!readOnly && (
            <>
              <Tooltip title="Load Example">
                <IconButton size="small" onClick={handleLoadExample}>
                  <HelpIcon />
                </IconButton>
              </Tooltip>
              <Tooltip title="Reset Form">
                <IconButton size="small" onClick={handleReset}>
                  <ResetIcon />
                </IconButton>
              </Tooltip>
            </>
          )}
        </Stack>
      </Stack>

      {/* Problem Statement */}
      <Box sx={{ mb: 3 }}>
        <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 1 }}>
          <ProblemIcon color="error" fontSize="small" />
          <Typography variant="subtitle2" fontWeight={600}>
            Problem Statement
          </Typography>
        </Stack>
        <TextField
          fullWidth
          multiline
          rows={2}
          value={problemStatement}
          onChange={(e) => setProblemStatement(e.target.value)}
          disabled={readOnly}
          placeholder="Clearly describe the problem that occurred..."
          helperText="Be specific about what happened, when, and what impact it had"
        />
      </Box>

      <Divider sx={{ my: 3 }} />

      {/* 5 Whys Section */}
      <Typography variant="subtitle2" fontWeight={600} sx={{ mb: 2 }}>
        The 5 Whys Analysis
      </Typography>
      <Alert severity="info" sx={{ mb: 2 }}>
        Ask "Why?" repeatedly until you reach the root cause. Each answer becomes the basis for the
        next question. You typically need 5 levels, but may need more or fewer.
      </Alert>

      <Stack spacing={2}>
        {whySteps.map((step, index) => (
          <Paper key={step.id} variant="outlined" sx={{ p: 2 }}>
            <Stack direction="row" alignItems="flex-start" spacing={2}>
              <Chip
                label={`${index + 1}`}
                size="small"
                color="primary"
                sx={{ minWidth: 32 }}
              />
              <Box sx={{ flexGrow: 1 }}>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                  {step.question}
                </Typography>
                <TextField
                  fullWidth
                  multiline
                  rows={2}
                  value={step.answer}
                  onChange={(e) => handleWhyChange(step.id, 'answer', e.target.value)}
                  disabled={readOnly}
                  placeholder="Enter your answer..."
                  sx={{ mb: 1 }}
                />
                <TextField
                  fullWidth
                  size="small"
                  value={step.evidence || ''}
                  onChange={(e) => handleWhyChange(step.id, 'evidence', e.target.value)}
                  disabled={readOnly}
                  placeholder="Evidence or data supporting this answer (optional)"
                  helperText="Link to logs, metrics, or other supporting data"
                />
              </Box>
              {!readOnly && whySteps.length > 1 && (
                <IconButton
                  size="small"
                  onClick={() => handleRemoveWhyStep(step.id)}
                  color="error"
                >
                  <DeleteIcon />
                </IconButton>
              )}
            </Stack>
          </Paper>
        ))}
      </Stack>

      {!readOnly && (
        <Button
          startIcon={<AddIcon />}
          onClick={handleAddWhyStep}
          sx={{ mt: 2 }}
          variant="outlined"
          size="small"
        >
          Add Another Why
        </Button>
      )}

      <Divider sx={{ my: 3 }} />

      {/* Root Cause */}
      <Box sx={{ mb: 3 }}>
        <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 1 }}>
          <IdeaIcon color="warning" fontSize="small" />
          <Typography variant="subtitle2" fontWeight={600}>
            Identified Root Cause
          </Typography>
        </Stack>
        <TextField
          fullWidth
          multiline
          rows={2}
          value={rootCause}
          onChange={(e) => setRootCause(e.target.value)}
          disabled={readOnly}
          placeholder="What is the fundamental root cause based on the analysis above?"
          helperText="This should address the underlying issue, not just symptoms"
        />
      </Box>

      {/* Solution & Workaround */}
      <Box sx={{ mb: 3 }}>
        <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 1 }}>
          <SolutionIcon color="success" fontSize="small" />
          <Typography variant="subtitle2" fontWeight={600}>
            Proposed Solution
          </Typography>
        </Stack>
        <TextField
          fullWidth
          multiline
          rows={3}
          value={proposedSolution}
          onChange={(e) => setProposedSolution(e.target.value)}
          disabled={readOnly}
          placeholder="What permanent solution will address the root cause?"
        />
      </Box>

      <Accordion>
        <AccordionSummary expandIcon={<ExpandIcon />}>
          <Typography variant="subtitle2">Workaround & Preventive Measures (Optional)</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Stack spacing={2}>
            <TextField
              fullWidth
              multiline
              rows={2}
              label="Workaround"
              value={workaround}
              onChange={(e) => setWorkaround(e.target.value)}
              disabled={readOnly}
              placeholder="Temporary fix to restore service while permanent solution is implemented"
            />
            <TextField
              fullWidth
              multiline
              rows={2}
              label="Preventive Measures"
              value={preventiveMeasures}
              onChange={(e) => setPreventiveMeasures(e.target.value)}
              disabled={readOnly}
              placeholder="What changes will prevent this problem from recurring?"
            />
          </Stack>
        </AccordionDetails>
      </Accordion>

      {/* Save Button */}
      {!readOnly && (
        <Box sx={{ mt: 3, display: 'flex', justifyContent: 'flex-end', gap: 2 }}>
          <Button variant="outlined" onClick={handleReset}>
            Reset
          </Button>
          <Button
            variant="contained"
            startIcon={<SaveIcon />}
            onClick={handleSave}
            disabled={!isComplete}
          >
            Save Analysis
          </Button>
        </Box>
      )}
    </Paper>
  );
};

export default RootCauseAnalysisTemplate;
