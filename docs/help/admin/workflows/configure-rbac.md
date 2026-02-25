# Workflow: Configure RBAC (Roles & Permissions)

> **Persona:** System Admin / Admin
> **Outcome:** Roles with correct permissions, assigned to users/groups

## Steps
1) **Review requirements**: Identify personas (e.g., Sales Rep, Support Agent, Finance Reviewer) and required access.
2) **Create roles**: Admin Center → Roles → Create. Set name, description, hierarchy level (0=SystemAdmin, 1=Admin, 2=Manager, 3=User, 4=Guest).
3) **Assign permissions**: Toggle fine-grained permissions (accounts, contacts, leads, opportunities, quotes, ITSM, workflows, agents). Save.
4) **Map to groups**: Assign roles to groups to simplify user onboarding.
5) **Assign users**: Add users to groups or directly to roles for exceptions.
6) **Validate access**: Use a test user to confirm CRUD, list, and admin pages behave (expect 403 where not allowed).
7) **Enable auditing**: Ensure audit logs capture role/permission changes.

## Best Practices
- Keep **SystemAdmin** minimal; use **Admin** for daily ops.
- Prefer **group-based** assignments over direct user-role mappings.
- Use **Guest** for read-only external reviewers.
- Re-certify roles quarterly; remove dormant users.

## Troubleshooting
- **403 errors**: Check role assignment and feature flags; confirm resource scope matches data filters.
- **Over-permission**: Lower hierarchy level and remove broad permissions; retest.
- **API 500**: Ensure Redis is reachable (`crm-redis:6379`) and RBAC service is healthy.
