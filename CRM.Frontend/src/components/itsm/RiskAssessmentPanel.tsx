/**
 * RiskAssessmentForm - Assess risks for changes
 */

import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Stack,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Typography,
  Paper,
  RadioGroup,
  FormControlLabel,
  Radio,
  Divider,
} from '@mui/material';
import {
  Error as HighRiskIcon,
  Warning as MediumRiskIcon,
  Info as LowRiskIcon,
} from '@mui/icons-material';
import { ChangeRiskLevel } from '../../services/changeService';

export interface RiskAssessment {
  riskLevel: ChangeRiskLevel;
  description: string;
  potentialImpact: string;
  mitigationPlan: string;
  contingencyPlan: string;
  backupPlan: string;
}

interface RiskAssessmentFormProps {
  assessment?: RiskAssessment;
  onChange: (assessment: RiskAssessment) => void;
  readOnly?: boolean;
}

const riskLevelDescriptions = {
  [ChangeRiskLevel.Low]: 'Minor impact, standard procedures, low complexity',
  [ChangeRiskLevel.Medium]: 'Moderate impact, familiar procedures, medium complexity',
  [ChangeRiskLevel.High]: 'Significant impact, new procedures or scope, high complexity',
  [ChangeRiskLevel.VeryHigh]: 'Critical impact, uncommon procedures, very high complexity',
};

const getRiskIcon = (level: ChangeRiskLevel) => {
  switch (level) {
    case ChangeRiskLevel.VeryHigh:
    case ChangeRiskLevel.High:
      return <HighRiskIcon sx={{ color: 'error.main' }} />;
    case ChangeRiskLevel.Medium:
      return <MediumRiskIcon sx={{ color: 'warning.main' }} />;
    default:
      return <LowRiskIcon sx={{ color: 'info.main' }} />;
  }
};

export const RiskAssessmentForm: React.FC<RiskAssessmentFormProps> = ({
  assessment = {
    riskLevel: ChangeRiskLevel.Medium,
    description: '',
    potentialImpact: '',
    mitigationPlan: '',
    contingencyPlan: '',
    backupPlan: '',
  },
  onChange,
  readOnly = false,
}) => {
  const [formData, setFormData] = useState(assessment);

  const handleChange = (field: keyof RiskAssessment, value: any) => {
    const updated = { ...formData, [field]: value };
    setFormData(updated);
    onChange(updated);
  };

  return (
    <Box>
      <Typography variant="h6" sx={{ mb: 2, fontWeight: 'bold', display: 'flex', alignItems: 'center', gap: 1 }}>
        {getRiskIcon(formData.riskLevel)}
        Risk Assessment
      </Typography>

      <Card>
        <CardContent>
          <Stack spacing={3}>
            {/* Risk Level Selection */}
            <Box>
              <Typography variant="subtitle2" sx={{ mb: 1, fontWeight: 'bold' }}>
                Risk Level
              </Typography>
              <Paper sx={{ p: 2, bgcolor: 'background.default' }}>
                <RadioGroup
                  value={formData.riskLevel}
                  onChange={(e) => handleChange('riskLevel', Number(e.target.value))}
                  disabled={readOnly}
                >
                  {Object.entries(riskLevelDescriptions).map(([level, desc]) => (
                    <FormControlLabel
                      key={level}
                      value={Number(level)}
                      control={<Radio />}
                      label={
                        <Box>
                          <Typography variant="body2" sx={{ fontWeight: 500 }}>
                            {['Low', 'Medium', 'High', 'Very High'][Number(level)]}
                          </Typography>
                          <Typography variant="caption" color="text.secondary">
                            {desc}
                          </Typography>
                        </Box>
                      }
                    />
                  ))}
                </RadioGroup>
              </Paper>
            </Box>

            <Divider />

            {/* Risk Description */}
            <TextField
              fullWidth
              label="Risk Description"
              value={formData.description}
              onChange={(e) => handleChange('description', e.target.value)}
              multiline
              rows={3}
              placeholder="Describe the identified risks..."
              disabled={readOnly}
            />

            {/* Potential Impact */}
            <TextField
              fullWidth
              label="Potential Impact"
              value={formData.potentialImpact}
              onChange={(e) => handleChange('potentialImpact', e.target.value)}
              multiline
              rows={3}
              placeholder="Describe what could happen if something goes wrong..."
              disabled={readOnly}
            />

            <Divider />

            {/* Mitigation Plan */}
            <TextField
              fullWidth
              label="Mitigation Plan"
              value={formData.mitigationPlan}
              onChange={(e) => handleChange('mitigationPlan', e.target.value)}
              multiline
              rows={3}
              placeholder="Steps to reduce risk during implementation..."
              disabled={readOnly}
            />

            {/* Contingency Plan */}
            <TextField
              fullWidth
              label="Contingency Plan"
              value={formData.contingencyPlan}
              onChange={(e) => handleChange('contingencyPlan', e.target.value)}
              multiline
              rows={3}
              placeholder="What to do if things start going wrong..."
              disabled={readOnly}
            />

            {/* Backup Plan */}
            <TextField
              fullWidth
              label="Backup/Rollback Plan"
              value={formData.backupPlan}
              onChange={(e) => handleChange('backupPlan', e.target.value)}
              multiline
              rows={3}
              placeholder="Complete rollback procedure..."
              disabled={readOnly}
            />
          </Stack>
        </CardContent>
      </Card>
    </Box>
  );
};

export default RiskAssessmentForm;
