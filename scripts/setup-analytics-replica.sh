#!/bin/bash
# =============================================================================
# CRM Solution — Analytics Replica Initialisation Script
# TODO-DB-017: Set up MariaDB streaming replication to analytics replica
#
# What this script does:
#   1. Creates the replication user on the primary (crm-mariadb)
#   2. Takes a consistent snapshot via mysqldump --master-data
#   3. Restores snapshot to crm-mariadb-analytics
#   4. Issues CHANGE MASTER TO to point replica at primary
#   5. Starts replication and verifies
#   6. Creates crm_readonly user (calls create-readonly-user.sql)
#
# Usage: ./setup-analytics-replica.sh
# Prerequisites:
#   - crm-mariadb (primary) is running and healthy
#   - crm-mariadb-analytics (replica) is running and healthy
#   - Both containers are on crm-database-network
#
# chmod +x scripts/setup-analytics-replica.sh
# =============================================================================
set -euo pipefail

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------
PRIMARY_HOST="${PRIMARY_HOST:-crm-mariadb}"
PRIMARY_PORT="${PRIMARY_PORT:-3306}"
PRIMARY_ROOT_PASSWORD="${PRIMARY_ROOT_PASSWORD:-RootPass@Dev2024}"
MARIADB_DATABASE="${MARIADB_DATABASE:-crm_db}"

REPLICA_HOST="${REPLICA_HOST:-127.0.0.1}"
REPLICA_PORT="${REPLICA_PORT:-3307}"
REPLICA_ROOT_PASSWORD="${REPLICA_ROOT_PASSWORD:-RootPass@Dev2024}"

REPL_USER="${REPL_USER:-repl_analytics}"
REPL_PASSWORD="${REPL_PASSWORD:-ReplAnalytics@Dev2024}"

SNAPSHOT_FILE="/tmp/crm_primary_snapshot_$(date +%Y%m%d_%H%M%S).sql"

log() { echo "[$(date '+%Y-%m-%d %H:%M:%S')] $*"; }

# ---------------------------------------------------------------------------
# Convenience wrappers
# ---------------------------------------------------------------------------
primary_sql() {
  mysql -h "${PRIMARY_HOST}" -P "${PRIMARY_PORT}" \
    -u root -p"${PRIMARY_ROOT_PASSWORD}" -e "$1" 2>/dev/null
}

replica_sql() {
  mysql -h "${REPLICA_HOST}" -P "${REPLICA_PORT}" \
    -u root -p"${REPLICA_ROOT_PASSWORD}" -e "$1" 2>/dev/null
}

# ---------------------------------------------------------------------------
# 1. Wait for both servers to be ready
# ---------------------------------------------------------------------------
wait_for_servers() {
  log "Waiting for primary (${PRIMARY_HOST}:${PRIMARY_PORT})..."
  until mysqladmin ping -h "${PRIMARY_HOST}" -P "${PRIMARY_PORT}" \
      -u root -p"${PRIMARY_ROOT_PASSWORD}" --silent 2>/dev/null; do
    sleep 2
  done
  log "Primary is ready."

  log "Waiting for analytics replica (${REPLICA_HOST}:${REPLICA_PORT})..."
  until mysqladmin ping -h "${REPLICA_HOST}" -P "${REPLICA_PORT}" \
      -u root -p"${REPLICA_ROOT_PASSWORD}" --silent 2>/dev/null; do
    sleep 2
  done
  log "Replica is ready."
}

# ---------------------------------------------------------------------------
# 2. Create replication user on primary
# ---------------------------------------------------------------------------
create_replication_user() {
  log "Creating replication user '${REPL_USER}' on primary..."
  primary_sql "
    CREATE USER IF NOT EXISTS '${REPL_USER}'@'%'
      IDENTIFIED BY '${REPL_PASSWORD}';
    GRANT REPLICATION SLAVE ON *.* TO '${REPL_USER}'@'%';
    FLUSH PRIVILEGES;
  "
  log "Replication user created."
}

# ---------------------------------------------------------------------------
# 3. Take consistent snapshot with binary log coordinates
# ---------------------------------------------------------------------------
take_snapshot() {
  log "Taking consistent snapshot of ${MARIADB_DATABASE} (including master-data)..."
  mysqldump \
    -h "${PRIMARY_HOST}" -P "${PRIMARY_PORT}" \
    -u root -p"${PRIMARY_ROOT_PASSWORD}" \
    --single-transaction \
    --master-data=2 \
    --routines \
    --triggers \
    --events \
    --flush-logs \
    "${MARIADB_DATABASE}" > "${SNAPSHOT_FILE}"
  log "Snapshot written to ${SNAPSHOT_FILE} ($(du -sh "${SNAPSHOT_FILE}" | cut -f1))"
}

# ---------------------------------------------------------------------------
# 4. Restore snapshot to replica
# ---------------------------------------------------------------------------
restore_to_replica() {
  log "Restoring snapshot to analytics replica..."
  # Stop any existing replication before restoring
  replica_sql "STOP SLAVE;" 2>/dev/null || true
  # Recreate database
  replica_sql "DROP DATABASE IF EXISTS \`${MARIADB_DATABASE}\`;"
  replica_sql "CREATE DATABASE \`${MARIADB_DATABASE}\`
    CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
  # Restore
  mysql -h "${REPLICA_HOST}" -P "${REPLICA_PORT}" \
    -u root -p"${REPLICA_ROOT_PASSWORD}" \
    "${MARIADB_DATABASE}" < "${SNAPSHOT_FILE}"
  log "Snapshot restored."
}

# ---------------------------------------------------------------------------
# 5. Extract binary log position from snapshot header
# ---------------------------------------------------------------------------
extract_binlog_position() {
  BINLOG_FILE="$(grep 'CHANGE MASTER TO' "${SNAPSHOT_FILE}" \
    | grep -oP "MASTER_LOG_FILE='[^']+'" \
    | cut -d"'" -f2)"
  BINLOG_POS="$(grep 'CHANGE MASTER TO' "${SNAPSHOT_FILE}" \
    | grep -oP 'MASTER_LOG_POS=\d+' \
    | cut -d= -f2)"
  log "Detected binary log position: FILE=${BINLOG_FILE}, POS=${BINLOG_POS}"
}

# ---------------------------------------------------------------------------
# 6. Configure replication (CHANGE MASTER TO)
# ---------------------------------------------------------------------------
configure_replication() {
  log "Configuring replication on analytics replica..."
  replica_sql "
    CHANGE MASTER TO
      MASTER_HOST='${PRIMARY_HOST}',
      MASTER_PORT=${PRIMARY_PORT},
      MASTER_USER='${REPL_USER}',
      MASTER_PASSWORD='${REPL_PASSWORD}',
      MASTER_LOG_FILE='${BINLOG_FILE}',
      MASTER_LOG_POS=${BINLOG_POS},
      MASTER_CONNECT_RETRY=10;
  "
  log "Replication configured."
}

# ---------------------------------------------------------------------------
# 7. Start replication
# ---------------------------------------------------------------------------
start_replication() {
  log "Starting replication..."
  replica_sql "START SLAVE;"
  sleep 3
}

# ---------------------------------------------------------------------------
# 8. Verify replication status
# ---------------------------------------------------------------------------
verify_replication() {
  log "Verifying replication status..."
  local slave_io_running slave_sql_running seconds_behind
  slave_io_running="$(replica_sql "SHOW SLAVE STATUS\G" \
    | grep "Slave_IO_Running:" | awk '{print $2}')"
  slave_sql_running="$(replica_sql "SHOW SLAVE STATUS\G" \
    | grep "Slave_SQL_Running:" | awk '{print $2}')"
  seconds_behind="$(replica_sql "SHOW SLAVE STATUS\G" \
    | grep "Seconds_Behind_Master:" | awk '{print $2}')"

  if [[ "${slave_io_running}" == "Yes" && "${slave_sql_running}" == "Yes" ]]; then
    log "  ✓ Replication is running (Seconds_Behind_Master: ${seconds_behind:-0})"
  else
    log "  ✗ Replication problem:"
    log "    Slave_IO_Running:  ${slave_io_running:-unknown}"
    log "    Slave_SQL_Running: ${slave_sql_running:-unknown}"
    replica_sql "SHOW SLAVE STATUS\G" | grep -E "Error|Running|Behind"
    exit 1
  fi
}

# ---------------------------------------------------------------------------
# 9. Apply read-only user SQL
# ---------------------------------------------------------------------------
apply_readonly_user() {
  local sql_file
  sql_file="$(dirname "$0")/../database/schema/create-readonly-user.sql"
  if [[ -f "${sql_file}" ]]; then
    log "Applying create-readonly-user.sql to primary..."
    mysql -h "${PRIMARY_HOST}" -P "${PRIMARY_PORT}" \
      -u root -p"${PRIMARY_ROOT_PASSWORD}" < "${sql_file}"
    log "Read-only user created on primary (will replicate to analytics replica)."
  else
    log "WARNING: create-readonly-user.sql not found at ${sql_file}"
  fi
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
main() {
  log "===== Analytics Replica Setup Started ====="
  wait_for_servers
  create_replication_user
  take_snapshot
  restore_to_replica
  extract_binlog_position
  configure_replication
  start_replication
  verify_replication
  apply_readonly_user
  rm -f "${SNAPSHOT_FILE}"
  log "===== Analytics Replica Setup Completed ====="
  log ""
  log "Replica connection details:"
  log "  Host: ${REPLICA_HOST}:${REPLICA_PORT}"
  log "  User: crm_readonly / ReadOnlyPass@Dev2024"
  log "  Database: ${MARIADB_DATABASE}"
  log ""
  log "Monitor replication lag: scripts/monitor-replication-lag.sh"
}

main "$@"
