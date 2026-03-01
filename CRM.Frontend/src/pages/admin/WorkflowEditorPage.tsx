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
  Paper,
  Tab,
  Tabs,
  TextField,
  Typography,
} from '@mui/material';

const DEFAULT_YAML = `id: my-workflow
name: My Workflow
version: "1.0.0"
steps:
  - name: greet
    type: script
    script: greet-customer
    input:
      customerId: \${input.customerId}
  - name: send_email
    type: tool
    tool: SendEmail
    input:
      to: \${steps.greet.output.email}
      subject: "Welcome!"
`;

/** SARCH-060 — YAML WDL frontend editor with validate / save / deploy stubs. */
const WorkflowEditorPage: React.FC = () => {
  const [yaml, setYaml] = useState<string>(DEFAULT_YAML);
  const [tab, setTab] = useState<number>(0);
  const [validated, setValidated] = useState<boolean | null>(null);

  const handleValidate = (): void => {
    const hasId = yaml.includes('id:');
    const hasSteps = yaml.includes('steps:');
    setValidated(hasId && hasSteps);
  };

  const handleTabChange = (_: React.SyntheticEvent, newValue: number): void => {
    setTab(newValue);
  };

  return (
    <Box p={3}>
      <Typography variant="h4" gutterBottom>
        Workflow YAML Editor
      </Typography>

      <Tabs value={tab} onChange={handleTabChange} sx={{ mb: 2 }}>
        <Tab label="YAML Definition" />
        <Tab label="Visual Graph (Coming Soon)" />
        <Tab label="Run History" />
      </Tabs>

      {tab === 0 && (
        <Box>
          <TextField
            multiline
            fullWidth
            rows={20}
            value={yaml}
            onChange={(e) => setYaml(e.target.value)}
            variant="outlined"
            inputProps={{ style: { fontFamily: 'monospace', fontSize: 13 } }}
            aria-label="Workflow YAML definition"
          />
          <Box mt={2} display="flex" gap={2}>
            <Button variant="contained" onClick={handleValidate}>
              Validate
            </Button>
            <Button variant="outlined">Save</Button>
            <Button variant="outlined" color="secondary">
              Deploy
            </Button>
          </Box>
          {validated === true && (
            <Alert severity="success" sx={{ mt: 2 }}>
              YAML is valid!
            </Alert>
          )}
          {validated === false && (
            <Alert severity="error" sx={{ mt: 2 }}>
              YAML validation failed: missing &apos;id&apos; or &apos;steps&apos;
            </Alert>
          )}
        </Box>
      )}

      {tab === 1 && (
        <Alert severity="info">Visual workflow graph editor is planned for Q3 2026.</Alert>
      )}

      {tab === 2 && (
        <Alert severity="info">Workflow run history viewer coming soon.</Alert>
      )}
    </Box>
  );
};

export default WorkflowEditorPage;
