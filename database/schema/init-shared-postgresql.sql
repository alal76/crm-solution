-- =============================================================================
-- CRM Solution — Shared PostgreSQL Initialisation Script
-- TODO-DB-029: Create per-service databases and users on a shared PostgreSQL
--
-- PURPOSE:
--   When operators want to share a single PostgreSQL instance across all
--   provider services (Chatwoot, DocuSeal, Superset, n8n) instead of
--   running four separate postgres containers, run this script once on
--   the shared `crm-postgresql` container from docker-compose.databases.yml.
--
-- USAGE:
--   docker exec -i crm-postgresql psql -U postgres < database/schema/init-shared-postgresql.sql
--   OR
--   psql -h <host> -U postgres -f database/schema/init-shared-postgresql.sql
--
-- AFTER running this script, update per-service env vars in docker-compose.providers.yml:
--   chatwoot:   POSTGRES_HOST=crm-postgresql, POSTGRES_USER=chatwoot
--   docuseal:   DATABASE_URL=postgresql://docuseal:<pass>@crm-postgresql:5432/docuseal
--   superset:   DATABASE_HOST=crm-postgresql, DATABASE_USER=superset
--   n8n:        DB_POSTGRESDB_HOST=crm-postgresql, DB_POSTGRESDB_USER=n8n
-- =============================================================================

\echo '=== CRM Shared PostgreSQL Initialisation ==='

-- ─────────────────────────────────────────────
-- 1. Chatwoot
-- ─────────────────────────────────────────────
\echo 'Creating chatwoot database and user...'

DO $$
BEGIN
  IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'chatwoot') THEN
    CREATE USER chatwoot WITH
      ENCRYPTED PASSWORD 'chatwoot_password'
      CONNECTION LIMIT 50;
    RAISE NOTICE 'User chatwoot created.';
  ELSE
    RAISE NOTICE 'User chatwoot already exists — skipping.';
  END IF;
END
$$;

SELECT 'CREATE DATABASE chatwoot_production OWNER chatwoot'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'chatwoot_production') \gexec

GRANT ALL PRIVILEGES ON DATABASE chatwoot_production TO chatwoot;

\connect chatwoot_production
GRANT ALL ON SCHEMA public TO chatwoot;
ALTER SCHEMA public OWNER TO chatwoot;
\connect postgres

\echo 'chatwoot: ✓'

-- ─────────────────────────────────────────────
-- 2. DocuSeal
-- ─────────────────────────────────────────────
\echo 'Creating docuseal database and user...'

DO $$
BEGIN
  IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'docuseal') THEN
    CREATE USER docuseal WITH
      ENCRYPTED PASSWORD 'docuseal_password'
      CONNECTION LIMIT 30;
    RAISE NOTICE 'User docuseal created.';
  ELSE
    RAISE NOTICE 'User docuseal already exists — skipping.';
  END IF;
END
$$;

SELECT 'CREATE DATABASE docuseal OWNER docuseal'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'docuseal') \gexec

GRANT ALL PRIVILEGES ON DATABASE docuseal TO docuseal;

\connect docuseal
GRANT ALL ON SCHEMA public TO docuseal;
ALTER SCHEMA public OWNER TO docuseal;
\connect postgres

\echo 'docuseal: ✓'

-- ─────────────────────────────────────────────
-- 3. Apache Superset
-- ─────────────────────────────────────────────
\echo 'Creating superset database and user...'

DO $$
BEGIN
  IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'superset') THEN
    CREATE USER superset WITH
      ENCRYPTED PASSWORD 'superset_password'
      CONNECTION LIMIT 30;
    RAISE NOTICE 'User superset created.';
  ELSE
    RAISE NOTICE 'User superset already exists — skipping.';
  END IF;
END
$$;

SELECT 'CREATE DATABASE superset OWNER superset'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'superset') \gexec

GRANT ALL PRIVILEGES ON DATABASE superset TO superset;

\connect superset
GRANT ALL ON SCHEMA public TO superset;
ALTER SCHEMA public OWNER TO superset;
\connect postgres

\echo 'superset: ✓'

-- ─────────────────────────────────────────────
-- 4. n8n Workflow Automation
-- ─────────────────────────────────────────────
\echo 'Creating n8n database and user...'

DO $$
BEGIN
  IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'n8n') THEN
    CREATE USER n8n WITH
      ENCRYPTED PASSWORD 'n8n_password'
      CONNECTION LIMIT 30;
    RAISE NOTICE 'User n8n created.';
  ELSE
    RAISE NOTICE 'User n8n already exists — skipping.';
  END IF;
END
$$;

SELECT 'CREATE DATABASE n8n OWNER n8n'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'n8n') \gexec

GRANT ALL PRIVILEGES ON DATABASE n8n TO n8n;

\connect n8n
GRANT ALL ON SCHEMA public TO n8n;
ALTER SCHEMA public OWNER TO n8n;
\connect postgres

\echo 'n8n: ✓'

-- ─────────────────────────────────────────────
-- 5. Verification
-- ─────────────────────────────────────────────
\echo ''
\echo 'Verification: databases in this cluster'
SELECT datname, pg_catalog.pg_get_userbyid(datdba) AS owner
FROM pg_database
WHERE datname IN ('chatwoot_production', 'docuseal', 'superset', 'n8n')
ORDER BY datname;

\echo ''
\echo 'Verification: roles created'
SELECT rolname, rolconnlimit
FROM pg_roles
WHERE rolname IN ('chatwoot', 'docuseal', 'superset', 'n8n')
ORDER BY rolname;

\echo ''
\echo '=== Shared PostgreSQL Initialisation Complete ==='
\echo 'Next step: update POSTGRES_HOST env vars in docker-compose.providers.yml'
\echo '          to point to crm-postgresql instead of per-service postgres containers.'
