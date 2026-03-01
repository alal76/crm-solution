/**
 * MKT-002: Email Template Builder
 * Tabs for HTML source editing and live sandboxed preview.
 * Supports subject line, variable insertion, and char/word count.
 */

import { useState, useRef, useCallback } from 'react';
import {
  Box,
  Tabs,
  Tab,
  TextField,
  Typography,
  Button,
  ButtonGroup,
  Paper,
  Stack,
  Chip,
  Divider,
  Tooltip,
} from '@mui/material';
import {
  Visibility as PreviewIcon,
  Code as CodeIcon,
  Add as InsertIcon,
} from '@mui/icons-material';

// ─── Template variable suggestions ────────────────────────────────────────────

const TEMPLATE_VARIABLES = [
  { label: '{{first_name}}', tooltip: 'Recipient first name' },
  { label: '{{last_name}}', tooltip: 'Recipient last name' },
  { label: '{{company}}', tooltip: 'Company name' },
  { label: '{{email}}', tooltip: 'Recipient email' },
  { label: '{{unsubscribe_url}}', tooltip: 'Unsubscribe link URL' },
];

// ─── Component ─────────────────────────────────────────────────────────────────

export interface EmailTemplateBuilderProps {
  /** HTML body of the email */
  value: string;
  onChange: (html: string) => void;
  /** Email subject line */
  subject: string;
  onSubjectChange: (subject: string) => void;
}

function countWords(text: string): number {
  return text.trim() === '' ? 0 : text.trim().split(/\s+/).length;
}

export default function EmailTemplateBuilder({
  value,
  onChange,
  subject,
  onSubjectChange,
}: EmailTemplateBuilderProps) {
  const [activeTab, setActiveTab] = useState<0 | 1>(1); // 0 = Preview, 1 = Source
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  // ── Variable insertion ──────────────────────────────────────────────────────

  const insertVariable = useCallback(
    (variable: string) => {
      const el = textareaRef.current;
      if (!el) {
        onChange(value + variable);
        return;
      }
      const start = el.selectionStart ?? value.length;
      const end = el.selectionEnd ?? value.length;
      const newValue = value.slice(0, start) + variable + value.slice(end);
      onChange(newValue);
      // Restore cursor
      setTimeout(() => {
        el.focus();
        const pos = start + variable.length;
        el.setSelectionRange(pos, pos);
      }, 0);
    },
    [value, onChange]
  );

  // ── Stats ───────────────────────────────────────────────────────────────────

  const charCount = value.length;
  const wordCount = countWords(value.replace(/<[^>]+>/gu, ' ')); // NOSONAR - safe regex using negated character class, no backtracking risk

  // ── Render ──────────────────────────────────────────────────────────────────

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      {/* Subject line */}
      <TextField
        label="Subject Line"
        value={subject}
        onChange={(e) => onSubjectChange(e.target.value)}
        fullWidth
        size="small"
        placeholder="Enter email subject…"
        inputProps={{ maxLength: 200 }}
        helperText={`${subject.length}/200 characters`}
      />

      <Divider />

      {/* Variable insertion helpers */}
      <Box>
        <Typography variant="caption" color="text.secondary" sx={{ mr: 1 }}>
          Insert variable:
        </Typography>
        <Stack direction="row" spacing={0.5} flexWrap="wrap" useFlexGap>
          {TEMPLATE_VARIABLES.map((v) => (
            <Tooltip key={v.label} title={v.tooltip} arrow>
              <Chip
                label={v.label}
                size="small"
                icon={<InsertIcon fontSize="small" />}
                onClick={() => insertVariable(v.label)}
                sx={{ cursor: 'pointer', fontFamily: 'monospace', fontSize: '0.7rem' }}
                variant="outlined"
                color="primary"
              />
            </Tooltip>
          ))}
        </Stack>
      </Box>

      {/* Tab selector */}
      <Tabs
        value={activeTab}
        onChange={(_, v) => setActiveTab(v as 0 | 1)}
        variant="fullWidth"
        sx={{ borderBottom: 1, borderColor: 'divider' }}
      >
        <Tab icon={<PreviewIcon fontSize="small" />} iconPosition="start" label="Visual Preview" value={0} />
        <Tab icon={<CodeIcon fontSize="small" />} iconPosition="start" label="HTML Source" value={1} />
      </Tabs>

      {/* HTML Source editor */}
      {activeTab === 1 && (
        <Box>
          <TextField
            inputRef={textareaRef}
            multiline
            minRows={14}
            maxRows={28}
            fullWidth
            value={value}
            onChange={(e) => onChange(e.target.value)}
            placeholder="<html><body><p>Hello {{first_name}},</p></body></html>"
            sx={{
              fontFamily: 'monospace',
              '& textarea': { fontFamily: 'monospace', fontSize: '0.82rem' },
            }}
            inputProps={{ spellCheck: false }}
          />
          <Stack direction="row" spacing={2} sx={{ mt: 0.5 }}>
            <Typography variant="caption" color="text.secondary">
              {charCount} chars
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {wordCount} words
            </Typography>
          </Stack>
        </Box>
      )}

      {/* Live preview */}
      {activeTab === 0 && (
        <Paper variant="outlined" sx={{ overflow: 'hidden', borderRadius: 1 }}>
          {value.trim() === '' ? (
            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                minHeight: 280,
                bgcolor: 'grey.50',
              }}
            >
              <Typography color="text.disabled" variant="body2">
                Add HTML in the "HTML Source" tab to see a preview here.
              </Typography>
            </Box>
          ) : (
            <iframe
              srcDoc={value}
              sandbox="allow-same-origin"
              title="Email Preview"
              style={{
                width: '100%',
                minHeight: 320,
                border: 'none',
                display: 'block',
                backgroundColor: '#fff',
              }}
            />
          )}
        </Paper>
      )}
    </Box>
  );
}
