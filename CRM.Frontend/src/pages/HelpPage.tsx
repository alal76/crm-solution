import React from 'react';
import { Box, Typography, Accordion, AccordionSummary, AccordionDetails, List, ListItem, Link } from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';

// Comprehensive Help System for CRM Solution
// Covers: User, Functional Admin, System Admin, Getting Started, Workflows, Agents, API/Webhook Integration, Pluggable Architecture

const HelpPage: React.FC = () => (
  <Box sx={{ maxWidth: 900, mx: 'auto', my: 4 }}>
    <Typography variant="h3" gutterBottom>CRM Solution Help & Documentation</Typography>
    <Typography variant="subtitle1" gutterBottom>
      Welcome to the CRM Solution help system. This guide covers all roles, workflows, advanced features, and integration steps. For licensing, see the <Link href="/licenses">Licenses</Link> page.
    </Typography>

    {/* Getting Started */}
    <Accordion defaultExpanded>
      <AccordionSummary expandIcon={<ExpandMoreIcon />}>Getting Started</AccordionSummary>
      <AccordionDetails>
        <List>
          <ListItem>1. <b>Build & Deploy:</b> Follow <Link href="/about">About</Link> and <Link href="/licenses">Licenses</Link> for prerequisites. Run <code>build.sh</code> and <code>deploy-to-dev-server.sh</code> for local/dev deployment.</ListItem>
          <ListItem>2. <b>Initial Setup:</b> Access the app at <code>http://localhost</code> or your dev server. Login with default admin credentials (<code>admin@crm.local</code> / <code>Admin@123</code>).</ListItem>
          <ListItem>3. <b>Configure System:</b> Go to <Link href="/settings">Settings</Link> to set up company info, email, integrations, and feature flags.</ListItem>
          <ListItem>4. <b>Data Import:</b> Use <Link href="/accounts">Accounts</Link>, <Link href="/contacts">Contacts</Link>, <Link href="/products">Products</Link> pages to import or create data.</ListItem>
        </List>
      </AccordionDetails>
    </Accordion>

    {/* User Guide */}
    <Accordion>
      <AccordionSummary expandIcon={<ExpandMoreIcon />}>User Guide</AccordionSummary>
      <AccordionDetails>
        <List>
          <ListItem><b>Accounts & Contacts:</b> Manage organizations and people. See <Link href="/accounts">Accounts</Link> and <Link href="/contacts">Contacts</Link>.</ListItem>
          <ListItem><b>Opportunities & Leads:</b> Track sales pipeline. See <Link href="/opportunities">Opportunities</Link> and <Link href="/leads">Leads</Link>.</ListItem>
          <ListItem><b>Products & Quotes:</b> Manage catalog and generate quotes. See <Link href="/products">Products</Link> and <Link href="/quotes">Quotes</Link>.</ListItem>
          <ListItem><b>Service Requests:</b> Submit and track support tickets. See <Link href="/servicerequests">Service Requests</Link>.</ListItem>
          <ListItem><b>Reports & Analytics:</b> View dashboards and reports. See <Link href="/dashboard">Dashboard</Link> and <Link href="/reports">Reports</Link>.</ListItem>
        </List>
      </AccordionDetails>
    </Accordion>

    {/* Functional Administration */}
    <Accordion>
      <AccordionSummary expandIcon={<ExpandMoreIcon />}>Functional Administration</AccordionSummary>
      <AccordionDetails>
        <List>
          <ListItem><b>User & Group Management:</b> Add/edit users, assign roles, manage groups. See <Link href="/usermanagement">User Management</Link> and <Link href="/groupmanagement">Group Management</Link>.</ListItem>
          <ListItem><b>Department & Team Setup:</b> Organize departments and teams. See <Link href="/departments">Departments</Link> and <Link href="/teams">Teams</Link>.</ListItem>
          <ListItem><b>Workflow Tasks:</b> Configure workflow automation. See <Link href="/workflowtasks">Workflow Tasks</Link>.</ListItem>
          <ListItem><b>Knowledge Base:</b> Manage articles and FAQs. See <Link href="/knowledgebase">Knowledge Base</Link>.</ListItem>
        </List>
      </AccordionDetails>
    </Accordion>

    {/* System Administration */}
    <Accordion>
      <AccordionSummary expandIcon={<ExpandMoreIcon />}>System Administration</AccordionSummary>
      <AccordionDetails>
        <List>
          <ListItem><b>System Settings:</b> Configure global settings, feature flags, and integrations. See <Link href="/settings">Settings</Link>.</ListItem>
          <ListItem><b>Security & Authentication:</b> Manage JWT, password policies, 2FA, and rate limiting. See <Link href="/settings">Settings</Link>.</ListItem>
          <ListItem><b>Database Management:</b> All schema changes via EF Core migrations. See <Link href="/about">About</Link> and <Link href="/licenses">Licenses</Link>.</ListItem>
          <ListItem><b>Provider Health:</b> Check <Link href="/dashboard">Dashboard</Link> and <Link href="/licenses">Licenses</Link> for provider status.</ListItem>
        </List>
      </AccordionDetails>
    </Accordion>

    {/* Advanced Workflows & Agents */}
    <Accordion>
      <AccordionSummary expandIcon={<ExpandMoreIcon />}>Advanced Workflows & Agents</AccordionSummary>
      <AccordionDetails>
        <List>
          <ListItem><b>Workflow Engine:</b> Build custom workflows for automation. See <Link href="/workflowtasks">Workflow Tasks</Link>.</ListItem>
          <ListItem><b>AI Agents:</b> Use built-in and pluggable AI agents for lead scoring, support triage, analytics. See <Link href="/agentdirectory">Agent Directory</Link> and <Link href="/agentanalytics">Agent Analytics</Link>.</ListItem>
          <ListItem><b>Approval Gates:</b> Configure human approval steps in workflows. See <Link href="/approvals">Approvals</Link>.</ListItem>
        </List>
      </AccordionDetails>
    </Accordion>

    {/* API & Webhook Integration */}
    <Accordion>
      <AccordionSummary expandIcon={<ExpandMoreIcon />}>API & Webhook Integration</AccordionSummary>
      <AccordionDetails>
        <Typography variant="body2" gutterBottom>
          The CRM solution offers a full REST API and webhook system. See below for inventory and integration steps:
        </Typography>
        <List>
          <ListItem><b>API Endpoints:</b> See <Link href="/about">About</Link> and <Link href="/licenses">Licenses</Link> for endpoint inventory.</ListItem>
          <ListItem><b>Authentication:</b> JWT-based, see <Link href="/settings">Settings</Link> for keys and policies.</ListItem>
          <ListItem><b>Webhooks:</b> Configure via <Link href="/webhooksmanagement">Webhooks Management</Link>. Supported events: Account, Contact, Opportunity, Lead, ServiceRequest, WorkflowTask.</ListItem>
          <ListItem><b>Security:</b> All integrations require secure tokens and HTTPS. See <Link href="/settings">Settings</Link>.</ListItem>
        </List>
      </AccordionDetails>
    </Accordion>

    {/* Pluggable Architecture */}
    <Accordion>
      <AccordionSummary expandIcon={<ExpandMoreIcon />}>Pluggable Architecture</AccordionSummary>
      <AccordionDetails>
        <Typography variant="body2" gutterBottom>
          The CRM solution supports pluggable providers for Search, Chat, Notifications, Analytics, Signatures, AI, and Integrations. Only deployed components are available; others are documented but not active.
        </Typography>
        <List>
          <ListItem><b>Deployed:</b> Meilisearch (Search), Ollama (AI), Chatwoot (Chat), Novu (Notifications), Superset (Analytics), DocuSeal (Signatures), n8n (Integrations).</ListItem>
          <ListItem><b>Not Deployed:</b> Algolia, Typesense, Twilio, SendGrid, PowerBI, DocuSign, Zapier, etc. See <Link href="/licenses">Licenses</Link> for full list.</ListItem>
          <ListItem><b>Configuration:</b> Set provider type and feature flags in <Link href="/settings">Settings</Link>.</ListItem>
        </List>
      </AccordionDetails>
    </Accordion>

    {/* Support & Commercial Licensing */}
    <Accordion>
      <AccordionSummary expandIcon={<ExpandMoreIcon />}>Support & Commercial Licensing</AccordionSummary>
      <AccordionDetails>
        <Typography variant="body2" gutterBottom>
          For support, feature requests, or commercial license inquiries, contact the author via the <Link href="/licenses">Licenses</Link> page.
        </Typography>
      </AccordionDetails>
    </Accordion>
  </Box>
);

export default HelpPage;
