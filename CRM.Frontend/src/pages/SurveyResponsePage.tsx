// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import {
  Alert,
  Box,
  CircularProgress,
  Container,
  Paper,
  Typography,
} from '@mui/material';
import satisfactionService, {
  SatisfactionSurveyDto,
  SurveyStatus,
} from '../services/satisfactionService';
import SurveyResponseForm from '../components/satisfaction/SurveyResponseForm';

/**
 * Public-facing page for the survey link: /survey/:token
 * This page is accessible without authentication.
 */
const SurveyResponsePage: React.FC = () => {
  const { token } = useParams<{ token: string }>();
  const [survey, setSurvey] = useState<SatisfactionSurveyDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [done, setDone] = useState(false);

  useEffect(() => {
    if (!token) {
      setError('Invalid survey link.');
      setLoading(false);
      return;
    }

    const fetchSurvey = async () => {
      try {
        const data = await satisfactionService.getSurveyByToken(token);
        if (!data) {
          setError('Survey not found. The link may be invalid or the survey may have been removed.');
        } else if (data.status === SurveyStatus.Responded) {
          setDone(true);
        } else if (data.status === SurveyStatus.Expired || data.status === SurveyStatus.Cancelled) {
          setError('This survey link has expired or been cancelled.');
        } else {
          setSurvey(data);
        }
      } catch {
        setError('Unable to load the survey. Please try again later.');
      } finally {
        setLoading(false);
      }
    };

    void fetchSurvey();
  }, [token]);

  return (
    <Box
      sx={{
        minHeight: '100vh',
        bgcolor: 'background.default',
        display: 'flex',
        alignItems: 'center',
        py: 4,
      }}
    >
      <Container maxWidth="sm">
        {/* Branding header */}
        <Box textAlign="center" mb={3}>
          <Typography variant="h5" fontWeight={700} color="primary">
            Customer Feedback
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Your opinion helps us improve
          </Typography>
        </Box>

        <Paper elevation={3} sx={{ p: 3 }}>
          {loading ? (
            <Box display="flex" justifyContent="center" py={4}>
              <CircularProgress />
            </Box>
          ) : error ? (
            <Alert severity="error">{error}</Alert>
          ) : done ? (
            <Alert severity="success">
              You have already responded to this survey. Thank you for your feedback!
            </Alert>
          ) : survey ? (
            <SurveyResponseForm
              survey={survey}
              token={token ?? ''}
              onSubmitted={() => setDone(true)}
            />
          ) : null}
        </Paper>
      </Container>
    </Box>
  );
};

export default SurveyResponsePage;
