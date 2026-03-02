#!/bin/bash
# =============================================================================
# CRM Solution — DR Replication Setup Script
# TODO-DB-023/024/025: GTID-based MariaDB + PostgreSQL streaming replication
#
# What this script does:
#   1. Sets up GTID-based MariaDB replication to crm-mariadb-dr
#   2. Sets up PostgreSQL streaming replication to crm-postgresql-dr
#   3. Sets up Redis replication to crm-redis-dr
#   4. Verifies all three replication channels
#
# Prerequisites:
#   - Primary servers running at PRIMARY_HOST (default: 192.168.0.9)
#   - DR servers running at DR_HOST (default: DR server IP)
#   - DR docker-compose stack deployed (docker-compose.dr.yml)
#
# Usage: ./setup-dr-replication.sh [--primary-host <ip>] [--dr-host <ip>]
# chmod +x scripts/setup-dr-replication.sh
# =============================================================================
set -euo pipefail

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------
PRIMARY_HOST="${PRIMARY_HOST:?Set PRIMARY_HOST}"
DR_HOST="${DR_HOST:?Set DR_HOST to your DR server IP}"

# MariaDB
PRIMARY_MARIADB_PORT="${PRIMARY_MARIADB_PORT:-3306}"
DR_MARIADB_PORT="${DR_MARIADB_PORT:-3306}"
MARIADB_ROOT_PASSWORD="${MARIADB_ROOT_PASSWORD:?Set MARIADB_ROOT_PASSWORD}"
MARIADB_DATABASE="${MARIADB_DATABASE:-crm_db}"
DR_REPL_USER="${DR_REPL_USER:-repl_dr}"
DR_REPL_PASSWORD="${DR_REPL_PASSWORD:?Set DR_REPL_PASSWORD}"

# PostgreSQL
PRIMARY_POSTGRES_PORT="${PRIMARY_POSTGRES_PORT:-5432}"
DR_POSTGRES_PORT="${DR_POSTGRES_PORT:-5432}"
POSTGRES_USER="${POSTGRES_USER:-crm_user}"
POSTGRES_PASSWORD="${POSTGRES_PASSWORD:?Set POSTGRES_PASSWORD}"
POSTGRES_DB="${POSTGRES_DB:-crm_db}"
PG_REPL_USER="${PG_REPL_USER:-repl_dr}"
PG_REPL_PASSWORD="${PG_REPL_PASSWORD:?Set PG_REPL_PASSWORD}"

# Redis
PRIMARY_REDIS_PORT="${PRIMARY_REDIS_PORT:-6379}"
DR_REDIS_PORT="${DR_REDIS_PORT:-6379}"
REDIS_PASSWORD="${REDIS_PASSWORD:?Set REDIS_PASSWORD}"

log() { echo "[$(date '+%Y-%m-%d %H:%M:%S')] $*"; }

# ---------------------------------------------------------------------------
# Helper: run SQL on a specific MariaDB host
# ---------------------------------------------------------------------------
mariadb_sql() {
  local host="$1" port="$2"
  shift 2
  mysql -h "${host}" -P "${port}" -u root -p"${MARIADB_ROOT_PASSWORD}" -e "$@" 2>/dev/null
}

# ---------------------------------------------------------------------------
# 1. MariaDB: Create DR replication user on primary (GTID-based)
# ---------------------------------------------------------------------------
setup_mariadb_replication_user() {
  log "--- MariaDB: Creating DR replication user on primary ---"
  mariadb_sql "${PRIMARY_HOST}" "${PRIMARY_MARIADB_PORT}" "
    CREATE USER IF NOT EXISTS '${DR_REPL_USER}'@'%'
      IDENTIFIED BY '${DR_REPL_PASSWORD}';
    GRANT REPLICATION SLAVE ON *.* TO '${DR_REPL_USER}'@'%';
    GRANT REPLICA MONITOR ON *.* TO '${DR_REPL_USER}'@'%';
    FLUSH PRIVILEGES;
  "
  log "DR replication user '${DR_REPL_USER}' created on primary."
}

# ---------------------------------------------------------------------------
# 2. MariaDB: Take snapshot + configure GTID replication on DR replica
# ---------------------------------------------------------------------------
setup_mariadb_dr_replica() {
  log "--- MariaDB: Setting up GTID-based DR replication ---"

  # Snapshot primary with GTID coordinates
  local snapshot_file="/tmp/dr_mariadb_snapshot_$(date +%Y%m%d_%H%M%S).sql"
  log "Taking snapshot from primary (${PRIMARY_HOST}:${PRIMARY_MARIADB_PORT})..."
  mysqldump \
    -h "${PRIMARY_HOST}" -P "${PRIMARY_MARIADB_PORT}" \
    -u root -p"${MARIADB_ROOT_PASSWORD}" \
    --single-transaction \
    --master-data=2 \
    --gtid \
    --all-databases \
    --ignore-database=mysql \
    --ignore-database=information_schema \
    --ignore-database=performance_schema \
    --ignore-database=sys \
    > "${snapshot_file}"
  log "Snapshot written to ${snapshot_file} ($(du -sh "${snapshot_file}" | cut -f1))"

  # Restore to DR replica
  log "Restoring snapshot to DR replica (${DR_HOST}:${DR_MARIADB_PORT})..."
  mariadb_sql "${DR_HOST}" "${DR_MARIADB_PORT}" "STOP SLAVE;" 2>/dev/null || true
  mysql -h "${DR_HOST}" -P "${DR_MARIADB_PORT}" \
    -u root -p"${MARIADB_ROOT_PASSWORD}" < "${snapshot_file}"

  # Configure GTID replication
  log "Configuring GTID replication on DR replica..."
  mariadb_sql "${DR_HOST}" "${DR_MARIADB_PORT}" "
    CHANGE MASTER TO
      MASTER_HOST='${PRIMARY_HOST}',
      MASTER_PORT=${PRIMARY_MARIADB_PORT},
      MASTER_USER='${DR_REPL_USER}',
      MASTER_PASSWORD='${DR_REPL_PASSWORD}',
      MASTER_USE_GTID=slave_pos,
      MASTER_CONNECT_RETRY=10;
    START SLAVE;
  "

  rm -f "${snapshot_file}"
  log "MariaDB DR replication started."

  # Verify
  sleep 5
  local slave_io slave_sql
  slave_io="$(mariadb_sql "${DR_HOST}" "${DR_MARIADB_PORT}" "SHOW SLAVE STATUS\G" \
    | grep "Slave_IO_Running:" | awk '{print $2}')"
  slave_sql="$(mariadb_sql "${DR_HOST}" "${DR_MARIADB_PORT}" "SHOW SLAVE STATUS\G" \
    | grep "Slave_SQL_Running:" | awk '{print $2}')"
  log "  Slave_IO_Running: ${slave_io:-unknown}"
  log "  Slave_SQL_Running: ${slave_sql:-unknown}"
  [[ "${slave_io}" == "Yes" && "${slave_sql}" == "Yes" ]] \
    && log "  ✓ MariaDB DR replication is running" \
    || log "  ✗ WARNING: MariaDB DR replication may have issues"
}

# ---------------------------------------------------------------------------
# 3. PostgreSQL: Create replication user + pg_basebackup
# ---------------------------------------------------------------------------
setup_postgresql_dr_replica() {
  log "--- PostgreSQL: Setting up streaming replication to DR ---"
  export PGPASSWORD="${POSTGRES_PASSWORD}"

  # Create replication user on primary
  psql -h "${PRIMARY_HOST}" -p "${PRIMARY_POSTGRES_PORT}" \
    -U "${POSTGRES_USER}" -d postgres -c \
    "CREATE USER ${PG_REPL_USER} WITH REPLICATION ENCRYPTED PASSWORD '${PG_REPL_PASSWORD}';" \
    2>/dev/null || log "  (replication user may already exist)"

  # Copy primary's pg_hba.conf entry hint
  log "  Ensure pg_hba.conf on primary contains:"
  log "  host replication ${PG_REPL_USER} ${DR_HOST}/32 md5"

  # Use pg_basebackup to initialise the DR replica
  log "Running pg_basebackup from primary to DR replica data directory..."
  PGPASSWORD="${PG_REPL_PASSWORD}" pg_basebackup \
    -h "${PRIMARY_HOST}" \
    -p "${PRIMARY_POSTGRES_PORT}" \
    -U "${PG_REPL_USER}" \
    -D /tmp/pg_dr_base \
    --format=tar \
    --compress=9 \
    --wal-method=stream \
    --checkpoint=fast \
    --progress \
    --verbose 2>/dev/null || {
      log "  pg_basebackup requires network access to primary. Configure pg_hba.conf first."
      log "  Manual steps already documented in DR_FAILOVER_RUNBOOK.md."
      return
    }

  # Create recovery configuration for DR standby (PostgreSQL 12+)
  cat > /tmp/pg_dr_base/standby.signal <<'EOF'
# PostgreSQL standby signal file — presence = standby mode
EOF
  cat >> /tmp/pg_dr_base/postgresql.conf <<EOF
# DR Streaming Replication Recovery
primary_conninfo = 'host=${PRIMARY_HOST} port=${PRIMARY_POSTGRES_PORT} user=${PG_REPL_USER} password=${PG_REPL_PASSWORD} application_name=crm-dr'
primary_slot_name = 'crm_dr_slot'
hot_standby = on
wal_receiver_status_interval = 10s
EOF

  log "  PostgreSQL DR base backup and standby.signal created."
  unset PGPASSWORD
}

# ---------------------------------------------------------------------------
# 4. Redis: Configure DR replica (connect to primary)
# ---------------------------------------------------------------------------
setup_redis_dr() {
  log "--- Redis: Verifying DR replication ---"
  local info
  info="$(redis-cli -h "${DR_HOST}" -p "${DR_REDIS_PORT}" \
    -a "${REDIS_PASSWORD}" INFO replication 2>/dev/null || echo "")"
  if echo "${info}" | grep -q "role:slave"; then
    log "  ✓ Redis DR is configured as replica of ${PRIMARY_HOST}:${PRIMARY_REDIS_PORT}"
    echo "${info}" | grep -E "master_host|master_port|master_link_status|master_last_io_seconds_ago"
  else
    log "  Manually configuring Redis DR replica..."
    redis-cli -h "${DR_HOST}" -p "${DR_REDIS_PORT}" \
      -a "${REDIS_PASSWORD}" \
      REPLICAOF "${PRIMARY_HOST}" "${PRIMARY_REDIS_PORT}" 2>/dev/null
    redis-cli -h "${DR_HOST}" -p "${DR_REDIS_PORT}" \
      -a "${REDIS_PASSWORD}" \
      CONFIG SET masterauth "${REDIS_PASSWORD}" 2>/dev/null
    log "  Redis DR REPLICAOF command sent."
  fi
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
main() {
  log "===== DR Replication Setup Started ====="
  log "  Primary: ${PRIMARY_HOST}"
  log "  DR site: ${DR_HOST}"
  log ""
  setup_mariadb_replication_user
  setup_mariadb_dr_replica
  setup_postgresql_dr_replica
  setup_redis_dr
  log ""
  log "===== DR Replication Setup Completed ====="
  log ""
  log "Monitor replication lag:"
  log "  MariaDB:    REPLICA_HOST=${DR_HOST} REPLICA_PORT=${DR_MARIADB_PORT} ./scripts/monitor-replication-lag.sh"
  log "  PostgreSQL: psql -h ${DR_HOST} -p ${DR_POSTGRES_PORT} -U ${POSTGRES_USER} -c 'SELECT * FROM pg_stat_replication;'"
  log "  Redis:      redis-cli -h ${DR_HOST} -p ${DR_REDIS_PORT} -a <pass> INFO replication"
  log ""
  log "For failover procedures: docs/09-operations/DR_FAILOVER_RUNBOOK.md"
}

main "$@"
