import { Box, Container, Typography, Card, CardContent, TextField, Button, Alert } from '@mui/material';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

function PasswordResetPage() {
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setTimeout(() => {
      setSuccess(true);
      setLoading(false);
    }, 1000);
  };

  return (
    <Box sx={{ minHeight: '100vh', display: 'flex', alignItems: 'center', background: 'linear-gradient(135deg, #6750A4 0%, #A085D3 100%)', py: 3 }}>
      <Container maxWidth="sm">
        <Card sx={{ borderRadius: 3 }}>
          <CardContent sx={{ p: 4 }}>
            <Typography variant="h5" sx={{ fontWeight: 700, mb: 2 }}>Reset Password</Typography>
            {success ? (
              <Alert severity="success">Check your email for reset instructions</Alert>
            ) : (
              <Box component="form" onSubmit={handleSubmit}>
                <TextField fullWidth label="Email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} margin="normal" required />
                <Button fullWidth variant="contained" type="submit" sx={{ mt: 3 }} disabled={loading}>
                  {loading ? 'Sending...' : 'Send Reset Link'}
                </Button>
              </Box>
            )}
          </CardContent>
        </Card>
      </Container>
    </Box>
  );
}

export default PasswordResetPage;
