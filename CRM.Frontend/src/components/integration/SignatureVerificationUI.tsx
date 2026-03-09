/**
 * SignatureVerificationUI - Display and manage webhook signature verification settings
 * Implements TODO-INT001-28
 */

import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Grid,
  TextField,
  Button,
  Typography,
  Chip,
  Stack,
  Alert,
  AlertTitle,
  IconButton,
  Tooltip,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  InputAdornment,
  Paper,
  Divider,
  CircularProgress,
} from '@mui/material';
import {
  Visibility as VisibilityIcon,
  VisibilityOff as VisibilityOffIcon,
  ContentCopy as CopyIcon,
  Refresh as RefreshIcon,
  CheckCircle as CheckIcon,
  Error as ErrorIcon,
  Security as SecurityIcon,
  VpnKey as KeyIcon,
} from '@mui/icons-material';

// Types
export type SignatureAlgorithm = 'HMAC-SHA256' | 'HMAC-SHA512' | 'RSA-SHA256' | 'ED25519';

export interface SignatureSettings {
  algorithm: SignatureAlgorithm;
  secret: string;
  headerName: string;
  timestampHeaderName?: string;
  timestampToleranceSeconds: number;
  lastRotatedAt?: string;
  rotationIntervalDays?: number;
}

export interface VerificationTestResult {
  success: boolean;
  message: string;
  computedSignature?: string;
  expectedSignature?: string;
  timestampValid?: boolean;
}

interface SignatureVerificationUIProps {
  settings: SignatureSettings;
  onChange: (settings: SignatureSettings) => void;
  onRotateKey?: () => Promise<void>;
  onTestVerification?: (payload: string, signature: string) => Promise<VerificationTestResult>;
  readOnly?: boolean;
}

const algorithmDescriptions: Record<SignatureAlgorithm, string> = {
  'HMAC-SHA256': 'Standard HMAC using SHA-256 hash. Recommended for most use cases.',
  'HMAC-SHA512': 'HMAC using SHA-512 hash. Provides stronger security.',
  'RSA-SHA256': 'Asymmetric RSA signature. Requires public/private key pair.',
  'ED25519': 'Modern elliptic curve signature. Fast and secure.',
};

export const SignatureVerificationUI: React.FC<SignatureVerificationUIProps> = ({
  settings,
  onChange,
  onRotateKey,
  onTestVerification,
  readOnly = false,
}) => {
  const [showSecret, setShowSecret] = useState(false);
  const [rotating, setRotating] = useState(false);
  const [testing, setTesting] = useState(false);
  const [testPayload, setTestPayload] = useState<string>('{"event":"test","timestamp":"2026-02-24T10:00:00Z"}');
  const [testSignature, setTestSignature] = useState<string>('');
  const [testResult, setTestResult] = useState<VerificationTestResult | null>(null);
  const [copySuccess, setCopySuccess] = useState(false);

  const handleCopySecret = async () => {
    try {
      await navigator.clipboard.writeText(settings.secret);
      setCopySuccess(true);
      setTimeout(() => setCopySuccess(false), 2000);
    } catch (err) {
      console.error('Failed to copy secret:', err);
    }
  };

  const handleRotateKey = async () => {
    if (!onRotateKey) return;
    setRotating(true);
    try {
      await onRotateKey();
    } finally {
      setRotating(false);
    }
  };

  const handleTestVerification = async () => {
    if (!onTestVerification) return;
    setTesting(true);
    setTestResult(null);
    try {
      const result = await onTestVerification(testPayload, testSignature);
      setTestResult(result);
    } catch (err) {
      setTestResult({
        success: false,
        message: `Test failed: ${err instanceof Error ? (err as Error).message : 'Unknown error'}`,
      });
    } finally {
      setTesting(false);
    }
  };

  const formatDate = (dateString?: string): string => {
    if (!dateString) return 'Never';
    try {
      return new Date(dateString).toLocaleString();
    } catch {
      return 'Invalid date';
    }
  };

  const getRotationStatus = (): { color: 'success' | 'warning' | 'error'; text: string } => {
    if (!settings.lastRotatedAt || !settings.rotationIntervalDays) {
      return { color: 'warning', text: 'Rotation not configured' };
    }

    const lastRotated = new Date(settings.lastRotatedAt);
    const daysSinceRotation = Math.floor((Date.now() - lastRotated.getTime()) / (1000 * 60 * 60 * 24));

    if (daysSinceRotation > settings.rotationIntervalDays) {
      return { color: 'error', text: `Key rotation overdue (${daysSinceRotation} days)` };
    }
    if (daysSinceRotation > settings.rotationIntervalDays * 0.8) {
      return { color: 'warning', text: `Key rotation due soon (${settings.rotationIntervalDays - daysSinceRotation} days remaining)` };
    }
    return { color: 'success', text: `Key rotated ${daysSinceRotation} days ago` };
  };

  const rotationStatus = getRotationStatus();

  return (
    <Card>
      <CardHeader
        avatar={<SecurityIcon color="primary" />}
        title="Signature Verification"
        subheader="Configure how webhook payloads are signed for authenticity verification"
      />
      <CardContent>
        <Stack spacing={3}>
          {/* Algorithm Selection */}
          <Grid container spacing={2}>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth disabled={readOnly}>
                <InputLabel>Signature Algorithm</InputLabel>
                <Select
                  value={settings.algorithm}
                  onChange={(e) =>
                    onChange({ ...settings, algorithm: e.target.value as SignatureAlgorithm })
                  }
                  label="Signature Algorithm"
                >
                  {Object.keys(algorithmDescriptions).map((alg) => (
                    <MenuItem key={alg} value={alg}>
                      {alg}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
              <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
                {algorithmDescriptions[settings.algorithm]}
              </Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Signature Header Name"
                value={settings.headerName}
                onChange={(e) => onChange({ ...settings, headerName: e.target.value })}
                disabled={readOnly}
                placeholder="X-Webhook-Signature"
                helperText="HTTP header containing the signature"
              />
            </Grid>
          </Grid>

          {/* Secret Key */}
          <Box>
            <Typography variant="subtitle2" gutterBottom>
              <KeyIcon fontSize="small" sx={{ verticalAlign: 'middle', mr: 0.5 }} />
              Signing Secret
            </Typography>
            <TextField
              fullWidth
              type={showSecret ? 'text' : 'password'}
              value={settings.secret}
              onChange={(e) => onChange({ ...settings, secret: e.target.value })}
              disabled={readOnly}
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <Tooltip title={showSecret ? 'Hide secret' : 'Show secret'}>
                      <IconButton
                        onClick={() => setShowSecret(!showSecret)}
                        edge="end"
                      >
                        {showSecret ? <VisibilityOffIcon /> : <VisibilityIcon />}
                      </IconButton>
                    </Tooltip>
                    <Tooltip title={copySuccess ? 'Copied!' : 'Copy to clipboard'}>
                      <IconButton onClick={handleCopySecret} edge="end">
                        <CopyIcon color={copySuccess ? 'success' : 'inherit'} />
                      </IconButton>
                    </Tooltip>
                  </InputAdornment>
                ),
              }}
            />
          </Box>

          {/* Key Rotation */}
          <Paper variant="outlined" sx={{ p: 2 }}>
            <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
              <Typography variant="subtitle2">Key Rotation</Typography>
              <Chip
                label={rotationStatus.text}
                color={rotationStatus.color}
                size="small"
              />
            </Box>
            <Grid container spacing={2} alignItems="center">
              <Grid item xs={12} sm={4}>
                <TextField
                  fullWidth
                  type="number"
                  label="Rotation Interval (days)"
                  value={settings.rotationIntervalDays || 90}
                  onChange={(e) =>
                    onChange({
                      ...settings,
                      rotationIntervalDays: Number.parseInt(e.target.value, 10) || 90,
                    })
                  }
                  disabled={readOnly}
                  size="small"
                />
              </Grid>
              <Grid item xs={12} sm={4}>
                <Typography variant="body2" color="text.secondary">
                  Last rotated: {formatDate(settings.lastRotatedAt)}
                </Typography>
              </Grid>
              <Grid item xs={12} sm={4}>
                <Button
                  variant="outlined"
                  startIcon={rotating ? <CircularProgress size={16} /> : <RefreshIcon />}
                  onClick={handleRotateKey}
                  disabled={readOnly || rotating || !onRotateKey}
                  fullWidth
                >
                  Rotate Key Now
                </Button>
              </Grid>
            </Grid>
          </Paper>

          {/* Timestamp Verification */}
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Timestamp Header Name"
                value={settings.timestampHeaderName || ''}
                onChange={(e) =>
                  onChange({ ...settings, timestampHeaderName: e.target.value || undefined })
                }
                disabled={readOnly}
                placeholder="X-Webhook-Timestamp"
                helperText="Optional: Used to prevent replay attacks"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                type="number"
                label="Timestamp Tolerance (seconds)"
                value={settings.timestampToleranceSeconds}
                onChange={(e) =>
                  onChange({
                    ...settings,
                    timestampToleranceSeconds: Number.parseInt(e.target.value, 10) || 300,
                  })
                }
                disabled={readOnly}
                helperText="Maximum age of requests before rejection"
              />
            </Grid>
          </Grid>

          <Divider />

          {/* Verification Test */}
          <Box>
            <Typography variant="subtitle2" gutterBottom>
              Test Signature Verification
            </Typography>
            <Typography variant="body2" color="text.secondary" mb={2}>
              Test your signature verification by providing a sample payload and signature
            </Typography>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  multiline
                  rows={3}
                  label="Test Payload (JSON)"
                  value={testPayload}
                  onChange={(e) => setTestPayload(e.target.value)}
                  placeholder='{"event":"test"}'
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Signature to Verify"
                  value={testSignature}
                  onChange={(e) => setTestSignature(e.target.value)}
                  placeholder="sha256=..."
                />
              </Grid>
              <Grid item xs={12}>
                <Button
                  variant="contained"
                  onClick={handleTestVerification}
                  disabled={testing || !onTestVerification}
                  startIcon={testing ? <CircularProgress size={16} /> : undefined}
                >
                  Verify Signature
                </Button>
              </Grid>
            </Grid>

            {testResult && (
              <Alert
                severity={testResult.success ? 'success' : 'error'}
                icon={testResult.success ? <CheckIcon /> : <ErrorIcon />}
                sx={{ mt: 2 }}
              >
                <AlertTitle>{testResult.success ? 'Verification Passed' : 'Verification Failed'}</AlertTitle>
                <Typography variant="body2">{testResult.message}</Typography>
                {testResult.computedSignature && (
                  <Box mt={1}>
                    <Typography variant="caption" display="block">
                      Computed: <code>{testResult.computedSignature}</code>
                    </Typography>
                    {testResult.expectedSignature && (
                      <Typography variant="caption" display="block">
                        Expected: <code>{testResult.expectedSignature}</code>
                      </Typography>
                    )}
                  </Box>
                )}
                {testResult.timestampValid !== undefined && (
                  <Typography variant="caption" display="block" mt={1}>
                    Timestamp: {testResult.timestampValid ? '✓ Valid' : '✗ Invalid or expired'}
                  </Typography>
                )}
              </Alert>
            )}
          </Box>

          {/* Security Best Practices */}
          <Alert severity="info" sx={{ mt: 2 }}>
            <AlertTitle>Security Best Practices</AlertTitle>
            <ul style={{ margin: 0, paddingLeft: 20 }}>
              <li>Use HTTPS endpoints only for webhook delivery</li>
              <li>Store the signing secret securely (never expose in client-side code)</li>
              <li>Always verify signatures before processing webhook payloads</li>
              <li>Use timestamp validation to prevent replay attacks</li>
              <li>Rotate signing keys periodically (every 90 days recommended)</li>
            </ul>
          </Alert>
        </Stack>
      </CardContent>
    </Card>
  );
};

export default SignatureVerificationUI;
