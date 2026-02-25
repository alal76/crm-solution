# Workflow: Onboard a New User

> **Persona:** System Admin / Admin
> **Outcome:** A secure, fully configured user with correct access and MFA

## Steps
1) **Create user**: Admin Center → Users → Create. Enter email, name, role hint, primary group.
2) **Assign role & group**: Choose `Admin`/`Manager`/`User` (or custom) and set Primary Group.
3) **Set password policy**: Enforce reset on first login; require strong passwords; set expiry if needed.
4) **Enable MFA**: Require TOTP; optionally WebAuthn. Store recovery codes securely.
5) **Configure features**: Toggle modules in Feature Flags (CRM Core, Sales, Marketing, ITSM, AI Agents) per persona.
6) **Provision data access**: Scope accounts/territories (if enabled); set data access to All/Team/Owned.
7) **Assign permissions**: Add to additional groups for contextual access (e.g., Finance, Support). Confirm RBAC checks.
8) **Notify user**: Send login URL (http://192.168.0.9) and temporary password; include 2FA instructions.
9) **Verify login**: User signs in, changes password, completes MFA, and lands on correct homepage.
10) **Audit**: Check audit log for creation and first login; ensure no 401/403 in logs.

## Checklist
- [ ] MFA enforced
- [ ] Default password changed
- [ ] Role + group set
- [ ] Feature flags aligned to persona
- [ ] Data scope confirmed
- [ ] Audit entry present

## Troubleshooting
- **401 on login**: Verify role/group and feature flags; ensure account is Active.
- **MFA errors**: Resync TOTP; check time drift; allow backup codes.
- **Wrong landing page**: Update primary group/homepage preference in user profile.
