# AI Agent: User Management Assistant

> **Persona:** Admin / Helpdesk
> **Purpose:** Guide user onboarding, role assignment, and access troubleshooting.

## What It Does
- Generates onboarding checklists per persona (Sales, Support, Marketing, ITSM)
- Suggests roles/groups based on requested access
- Validates RBAC coverage and flags missing permissions
- Automates routine updates (reset password, unlock account, enforce MFA)
- Drafts user notifications with login + MFA steps

## Workflow
1) Provide user details (name, email), team, required modules, data scope (All/Team/Owned).
2) Agent proposes role + group mapping and feature flags to enable.
3) Agent outputs step list: create user, assign role, enforce MFA, notify user.
4) Optionally executes via APIs if admin confirms.

## Inputs
- Persona: Sales/Support/Marketing/IT/Finance
- Required modules: Accounts, Opportunities, ITSM, Campaigns, Workflows, Agents
- Data scope: all/team/owned; territories if applicable

## Outputs
- Role + group recommendation
- Feature flag checklist
- Onboarding steps and validation checks
- Notification template to send to the user

## Troubleshooting
- **403 after assignment**: Verify role hierarchy and feature flags; ensure account Active.
- **MFA not enforced**: Confirm policy and user MFA status; regenerate recovery codes.
- **Too much access**: Move user to lower hierarchy role or adjust group membership.
