import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Grid from '@mui/material/Grid';
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Alert from '@mui/material/Alert';
import apiClient from '../../services/apiClient';

interface ChangeApprovalDetail {
  changeId: number;
  number: string;
  shortDescription: string;
  state: number;
  approvalStatus: number;
}

const ChangeApprovalPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [change, setChange] = useState<ChangeApprovalDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [comments, setComments] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [submitted, setSubmitted] = useState(false);
  const [rejecting, setRejecting] = useState(false);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await apiClient.get(`/changes/${id}`);
        setChange(response.data);
      } catch (error) {
        console.error('Failed to load change', error);
      } finally {
        setLoading(false);
      }
    };

    if (id) {
      load();
    } else {
      setLoading(false);
    }
  }, [id]);

  const handleApprove = async () => {
    if (!id) return;
    setSubmitting(true);
    setSubmitError(null);

    try {
      await apiClient.post(`/changes/${id}/approvals`, { comments });
      setSubmitted(true);
    } catch (error) {
      console.error('Failed to approve change', error);
      setSubmitError('Unable to submit approval. Please try again.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleReject = async () => {
    if (!id) return;
    setRejecting(true);
    setSubmitError(null);

    try {
      await apiClient.post(`/changes/${id}/rejections`, { comments });
      setSubmitted(true);
    } catch (error) {
      console.error('Failed to reject change', error);
      setSubmitError('Unable to submit rejection. Please try again.');
    } finally {
      setRejecting(false);
    }
  };

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" fontWeight="bold" sx={{ mb: 3 }}>Change Approvals</Typography>
      <Paper sx={{ p: 3 }}>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : !change ? (
          <Typography color="text.secondary">Change not found.</Typography>
        ) : (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <Box>
              <Typography variant="body2" color="text.secondary">{change.number}</Typography>
              <Typography variant="h6" fontWeight="bold">{change.shortDescription}</Typography>
            </Box>
            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle2" color="text.secondary">State</Typography>
                <Typography>State {change.state}</Typography>
              </Grid>
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle2" color="text.secondary">Approval</Typography>
                <Typography>Status {change.approvalStatus}</Typography>
              </Grid>
            </Grid>
            <Paper variant="outlined" sx={{ p: 3 }}>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                <TextField
                  fullWidth
                  label="Approval comments"
                  id="approval-comments"
                  value={comments}
                  onChange={(event) => setComments(event.target.value)}
                  multiline
                  rows={4}
                  placeholder="Add comments for the change owner"
                  disabled={submitted || submitting}
                />
                {submitError && <Alert severity="error">{submitError}</Alert>}
                {submitted && <Alert severity="success">Approval submitted.</Alert>}
                <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1 }}>
                  <Button
                    variant="contained"
                    color="error"
                    onClick={handleReject}
                    disabled={rejecting || submitted || submitting}
                  >
                    {rejecting ? 'Rejecting...' : 'Reject Change'}
                  </Button>
                  <Button
                    variant="contained"
                    onClick={handleApprove}
                    disabled={submitting || submitted || rejecting}
                  >
                    {submitting ? 'Submitting...' : 'Approve Change'}
                  </Button>
                </Box>
              </Box>
            </Paper>
          </Box>
        )}
      </Paper>
    </Box>
  );
};

export default ChangeApprovalPage;
