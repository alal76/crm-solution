#!/bin/bash
# =============================================================================
# CRM Solution - Database Migration Script
# =============================================================================
# Purpose: Apply EF Core migrations to MariaDB using idempotent SQL scripts.
#
# WHY THIS EXISTS:
#   MariaDB/MySQL DDL statements (CREATE TABLE, ALTER TABLE, etc.) are
#   auto-committed and cannot be rolled back in a transaction. When EF Core's
#   `dotnet ef database update` fails partway through a migration, the
#   already-created tables persist but the migration is NOT recorded in
#   __EFMigrationsHistory. On retry, it fails with "Table already exists".
#
#   This script generates an idempotent SQL migration script that wraps each
#   statement in IF NOT EXISTS checks via stored procedures, making it safe
#   to re-run after partial failures.
#
# USAGE:
#   ./scripts/apply-migrations.sh [options]
#
# OPTIONS:
#   --local          Apply to local Docker MariaDB (default)
#   --remote         Apply to remote server (192.168.0.9)
#   --reset          Drop and recreate the database before migrating
#   --stop-api       Stop API container before migrating (recommended with --reset)
#   --start-api      Start API container after migrating
#   --help           Show this help
#
# EXAMPLES:
#   ./scripts/apply-migrations.sh --local --reset --stop-api --start-api
#   ./scripts/apply-migrations.sh --remote --reset --stop-api --start-api
#   ./scripts/apply-migrations.sh --local   # Just apply pending migrations
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
MIGRATION_SQL="/tmp/crm_migration_$(date +%Y%m%d_%H%M%S).sql"

# Defaults
TARGET="local"
RESET_DB=false
STOP_API=false
START_API=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --local) TARGET="local"; shift ;;
        --remote) TARGET="remote"; shift ;;
        --reset) RESET_DB=true; shift ;;
        --stop-api) STOP_API=true; shift ;;
        --start-api) START_API=true; shift ;;
        --help)
            head -35 "$0" | tail -30
            exit 0
            ;;
        *) echo "Unknown option: $1"; exit 1 ;;
    esac
done

echo "=== CRM Database Migration ==="
echo "Target:    $TARGET"
echo "Reset DB:  $RESET_DB"
echo "Stop API:  $STOP_API"
echo "Start API: $START_API"
echo ""

# Helper function to run MariaDB commands
run_mariadb() {
    local sql="$1"
    if [[ "$TARGET" == "local" ]]; then
        docker exec crm-mariadb mariadb -u root -p'RootPass@Dev2024' crm_db -e "$sql"
    else
        ssh root@192.168.0.9 "docker exec crm-mariadb mariadb -u root -p'RootPass@Dev2024' crm_db -e \"$sql\""
    fi
}

run_mariadb_nodb() {
    local sql="$1"
    if [[ "$TARGET" == "local" ]]; then
        docker exec crm-mariadb mariadb -u root -p'RootPass@Dev2024' -e "$sql"
    else
        ssh root@192.168.0.9 "docker exec crm-mariadb mariadb -u root -p'RootPass@Dev2024' -e \"$sql\""
    fi
}

copy_and_run_sql() {
    local sql_file="$1"
    if [[ "$TARGET" == "local" ]]; then
        docker cp "$sql_file" crm-mariadb:/tmp/migration.sql
        docker exec crm-mariadb mariadb -u root -p'RootPass@Dev2024' crm_db -e "source /tmp/migration.sql"
    else
        scp "$sql_file" root@192.168.0.9:/tmp/migration.sql
        ssh root@192.168.0.9 "docker cp /tmp/migration.sql crm-mariadb:/tmp/migration.sql && docker exec crm-mariadb mariadb -u root -p'RootPass@Dev2024' crm_db -e 'source /tmp/migration.sql'"
    fi
}

stop_api() {
    echo "Stopping API container..."
    if [[ "$TARGET" == "local" ]]; then
        docker stop crm-api 2>/dev/null || true
    else
        ssh root@192.168.0.9 "docker stop crm-api 2>/dev/null || true"
    fi
    echo "API stopped."
}

start_api() {
    echo "Starting API container..."
    if [[ "$TARGET" == "local" ]]; then
        docker start crm-api 2>/dev/null || true
    else
        ssh root@192.168.0.9 "docker start crm-api 2>/dev/null || true"
    fi
    echo "API started."
}

# Step 1: Stop API if requested
if [[ "$STOP_API" == true ]]; then
    stop_api
fi

# Step 2: Reset database if requested
if [[ "$RESET_DB" == true ]]; then
    echo "Resetting database..."
    run_mariadb_nodb "DROP DATABASE IF EXISTS crm_db; CREATE DATABASE crm_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci; GRANT ALL PRIVILEGES ON crm_db.* TO 'crm_user'@'%'; FLUSH PRIVILEGES;"
    echo "Database reset complete."
fi

# Step 3: Generate idempotent migration SQL
echo "Generating idempotent migration SQL..."
cd "$PROJECT_ROOT"
dotnet ef migrations script \
    --project CRM.Backend/src/CRM.Infrastructure \
    --startup-project CRM.Backend/src/CRM.Api \
    --idempotent \
    -o "$MIGRATION_SQL" 2>&1 | grep -v "warning\|WRN\|NU1904" || true

echo "Migration script generated: $MIGRATION_SQL ($(wc -l < "$MIGRATION_SQL") lines)"

# Step 4: Apply migration SQL
echo "Applying migration SQL to $TARGET MariaDB..."
copy_and_run_sql "$MIGRATION_SQL"
echo "Migration SQL applied."

# Step 5: Verify
echo ""
echo "=== Verification ==="
TABLE_COUNT=$(run_mariadb "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'crm_db';" 2>/dev/null | tail -1)
echo "Tables created: $TABLE_COUNT"

echo "Migration history:"
run_mariadb "SELECT * FROM __EFMigrationsHistory;" 2>/dev/null || echo "(no migration history table)"

# Step 6: Start API if requested
if [[ "$START_API" == true ]]; then
    start_api
fi

echo ""
echo "=== Migration Complete ==="
echo "Run 'dotnet ef database update --project CRM.Backend/src/CRM.Infrastructure --startup-project CRM.Backend/src/CRM.Api' to verify from host."

# Cleanup
rm -f "$MIGRATION_SQL"
