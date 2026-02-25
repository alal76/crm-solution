-- =============================================================================
-- CRM Solution — Read-Only User for Analytics Replica
-- TODO-DB-018: Creates crm_readonly user with SELECT-only privileges
--
-- Apply to: crm-mariadb (primary) and crm-mariadb-analytics (replica)
-- Usage: mysql -h crm-mariadb -u root -p < database/schema/create-readonly-user.sql
-- =============================================================================

-- ---------------------------------------------------------------------------
-- Create read-only user (idempotent — safe to re-run)
-- ---------------------------------------------------------------------------
CREATE USER IF NOT EXISTS 'crm_readonly'@'%'
    IDENTIFIED BY 'ReadOnlyPass@Dev2024';

-- Re-set password in case it drifted (idempotent)
ALTER USER 'crm_readonly'@'%'
    IDENTIFIED BY 'ReadOnlyPass@Dev2024';

-- ---------------------------------------------------------------------------
-- Grant SELECT-only on crm_db
-- This user cannot INSERT, UPDATE, DELETE, CREATE, DROP, or execute procedures
-- ---------------------------------------------------------------------------
REVOKE ALL PRIVILEGES, GRANT OPTION FROM 'crm_readonly'@'%';

GRANT SELECT ON crm_db.* TO 'crm_readonly'@'%';

-- ---------------------------------------------------------------------------
-- Additionally grant SHOW VIEW so Superset can introspect view definitions
-- ---------------------------------------------------------------------------
GRANT SHOW VIEW ON crm_db.* TO 'crm_readonly'@'%';

-- ---------------------------------------------------------------------------
-- Apply privileges immediately
-- ---------------------------------------------------------------------------
FLUSH PRIVILEGES;

-- ---------------------------------------------------------------------------
-- Verify (should show only SELECT and SHOW VIEW)
-- ---------------------------------------------------------------------------
SHOW GRANTS FOR 'crm_readonly'@'%';

-- =============================================================================
-- Usage notes:
--
-- Application connection string for read-only access:
--   Server=crm-mariadb-analytics;Port=3307;Database=crm_db;
--   User=crm_readonly;Password=ReadOnlyPass@Dev2024;
--
-- Superset datasource URI:
--   mysql+pymysql://crm_readonly:ReadOnlyPass@Dev2024@crm-mariadb-analytics:3307/crm_db
--
-- EF Core (ReadOnly DbContext) - see CRM.Infrastructure/Data/CrmReadOnlyDbContext.cs
-- =============================================================================
