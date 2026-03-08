/**
 * SignatureVerificationUI - Webhook signature verification helper
 * TODO-INT001-28: UI for verifying HMAC signatures on webhook payloads
 */

import React, { useState, useCallback } from 'react';
import {
  Box,
  Paper,
  Typography,
  TextField,
  Button,
  Alert,
  Stack,
  Divider,
  Chip,
  IconButton,
  Tooltip,
  InputAdornment,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  useTheme,
} from '@mui/material';
import {
  VerifiedUser as VerifiedIcon,
  GppBad as FailedIcon,
  ContentCopy as CopyIcon,
  Visibility as ShowIcon,
  VisibilityOff as HideIcon,
  Info as InfoIcon,
  Security as SecurityIcon,
} from '@mui/icons-material';

// --------------------------------------------------------------------------
// Types
// --------------------------------------------------------------------------

export type HashAlgorithm = 'sha256' | 'sha1' | 'sha512' | 'md5';

interface VerificationResult {
  valid: boolean;
  computedSignature: string;
  message: string;
}

export interface SignatureVerificationUIProps {
  /** Pre-filled webhook secret */
  defaultSecret?: string;
  /** Pre-filled payload */
  defaultPayload?: string;
  /** Pre-filled signature header value */
  defaultSignature?: string;
  /** Custom verification function (e.g. call server-side) */
  onVerify?: (params: {
    secret: string;
    payload: string;
    signature: string;
    algorithm: HashAlgorithm;
  }) => Promise<VerificationResult>;
  /** Header name hint */
  signatureHeaderName?: string;
}

// --------------------------------------------------------------------------
// Client‑side HMAC (Web Crypto API)
// --------------------------------------------------------------------------

async function computeHmac(
  algorithm: HashAlgorithm,
  secret: string,
  payload: string,
): Promise<string> {
  const algoMap: Record<HashAlgorithm, string> = {
    sha256: 'SHA-256',
    sha1: 'SHA-1',
    sha512: 'SHA-512',
    md5: 'SHA-256', // fallback — MD5 not available in Web Crypto
  };

  const encoder = new TextEncoder();
  const keyData = encoder.encode(secret);
  const data = encoder.encode(payload);

  const cryptoKey = await crypto.subtle.importKey(
    'raw',
    keyData,
    { name: 'HMAC', hash: algoMap[algorithm] },
    false,
    ['sign'],
  );

  const signature = await crypto.subtle.sign('HMAC', cryptoKey, data);
  return Array.from(new Uint8Array(signature))
    .map((b) => b.toString(16).padStart(2, '0'))
    .join('');
}

// --------------------------------------------------------------------------
// Component
// --------------------------------------------------------------------------

export const SignatureVerificationUI: React.FC<SignatureVerificationUIProps> = ({
  defaultSecret = '',
  defaultPayload = '',
  defaultSignature = '',
  onVerify,
  signatureHeaderName = 'X-Webhook-Signature',
}) => {
  const theme = useTheme();

  const [secret, setSecret] = useState(defaultSecret);
  const [payload, setPayload] = useState(defaultPayload);
  const [signature, setSignature] = useState(defaultSignature);
  const [algorithm, setAlgorithm] = useState<HashAlgorithm>('sha256');
  const [showSecret, setShowSecret] = useState(false);
  const [verifying, setVerifying] = useState(false);
  const [result, setResult] = useState<VerificationResult | null>(null);

  // Verify handler
  const handleVerify = useCallback(async () => {
    if (!secret || !payload || !signature) return;

    setVerifying(true);
    setResult(null);

    try {
      if (onVerify) {
        const res = await onVerify({ secret, payload, signature, algorithm });
        setResult(res);
      } else {
        // Client-side verification
        const computed = await computeHmac(algorithm, secret, payload);

        // Normalise — strip common prefixes like "sha256=" before comparing
        const normalise = (s: string): string =>
          s.replace(/^(sha256=|sha1=|sha512=|v1=)/, '').toLowerCase().trim();

        const valid = normalise(computed) === normalise(signature);

        setResult({
          valid,
          computedSignature: computed,
          message: valid
            ? 'Signature is valid — the payload matches the secret.'
            : 'Signature mismatch — payload may have been tampered with or the secret is incorrect.',
        });
      }
    } catch (err) {
      setResult({
        valid: false,
        computedSignature: '',
        message: `Verification error: ${err instanceof Error ? (err as Error).message : 'Unknown error'}`,
      });
    } finally {
      setVerifying(false);
    }
  }, [secret, payload, signature, algorithm, onVerify]);

  // Copy helper
  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text).catch(() => {});
  };

  return (
    <Paper sx={{ p: 3 }}>
      <Stack direction="row" spacing={1} alignItems="center" sx={{ mb: 2 }}>
        <SecurityIcon color="primary" />
        <Typography variant="h6">Webhook Signature Verification</Typography>
      </Stack>

      <Alert severity="info" sx={{ mb: 2 }} icon={<InfoIcon />}>
        Verify that a webhook payload matches the expected HMAC signature using your webhook secret.
        Header: <strong>{signatureHeaderName}</strong>
      </Alert>

      <Divider sx={{ mb: 2 }} />

      {/* Algorithm */}
      <FormControl size="small" sx={{ mb: 2, minWidth: 160 }}>
        <InputLabel>Algorithm</InputLabel>
        <Select
          value={algorithm}
          label="Algorithm"
          onChange={(e) => setAlgorithm(e.target.value as HashAlgorithm)}
        >
          <MenuItem value="sha256">HMAC-SHA256</MenuItem>
          <MenuItem value="sha1">HMAC-SHA1</MenuItem>
          <MenuItem value="sha512">HMAC-SHA512</MenuItem>
        </Select>
      </FormControl>

      {/* Secret */}
      <TextField
        label="Webhook Secret"
        value={secret}
        onChange={(e) => setSecret(e.target.value)}
        fullWidth
        size="small"
        type={showSecret ? 'text' : 'password'}
        sx={{ mb: 2 }}
        InputProps={{
          endAdornment: (
            <InputAdornment position="end">
              <IconButton size="small" onClick={() => setShowSecret((s) => !s)}>
                {showSecret ? <HideIcon fontSize="small" /> : <ShowIcon fontSize="small" />}
              </IconButton>
            </InputAdornment>
          ),
        }}
      />

      {/* Payload */}
      <TextField
        label="Payload (request body)"
        value={payload}
        onChange={(e) => setPayload(e.target.value)}
        fullWidth
        multiline
        minRows={4}
        maxRows={12}
        size="small"
        sx={{ mb: 2 }}
        placeholder='{"event":"incident.created","data":{...}}'
      />

      {/* Signature */}
      <TextField
        label={`Signature (from ${signatureHeaderName})`}
        value={signature}
        onChange={(e) => setSignature(e.target.value)}
        fullWidth
        size="small"
        sx={{ mb: 2 }}
        placeholder="sha256=abc123..."
      />

      {/* Actions */}
      <Stack direction="row" spacing={1} sx={{ mb: 2 }}>
        <Button
          variant="contained"
          onClick={handleVerify}
          disabled={verifying || !secret || !payload || !signature}
          startIcon={<VerifiedIcon />}
        >
          {verifying ? 'Verifying...' : 'Verify Signature'}
        </Button>
      </Stack>

      {/* Result */}
      {result && (
        <Alert
          severity={result.valid ? 'success' : 'error'}
          icon={result.valid ? <VerifiedIcon /> : <FailedIcon />}
          sx={{ mb: 1 }}
        >
          <Typography variant="body2" fontWeight={500}>
            {result.valid ? 'Signature Valid' : 'Signature Invalid'}
          </Typography>
          <Typography variant="body2">{result.message}</Typography>
          {result.computedSignature && (
            <Stack direction="row" spacing={1} alignItems="center" sx={{ mt: 1 }}>
              <Typography variant="caption" sx={{ fontFamily: 'monospace', wordBreak: 'break-all' }}>
                Computed: {result.computedSignature}
              </Typography>
              <Tooltip title="Copy computed signature">
                <IconButton size="small" onClick={() => copyToClipboard(result.computedSignature)}>
                  <CopyIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            </Stack>
          )}
        </Alert>
      )}
    </Paper>
  );
};

export default SignatureVerificationUI;
