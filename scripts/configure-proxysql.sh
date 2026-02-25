#!/bin/bash
# =============================================================================
# CRM Solution — ProxySQL Runtime Configuration Script
# TODO-DB-013: Programmatic ProxySQL setup via admin SQL interface
#
# Applies cluster topology, monitoring credentials, and flush to disk.
# Run this AFTER the crm-proxysql container is healthy.
#
# Usage: ./configure-proxysql.sh [--host <proxysql-host>] [--port <admin-port>]
# chmod +x scripts/configure-proxysql.sh
# =============================================================================
set -euo pipefail

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------
PROXYSQL_HOST="${PROXYSQL_HOST:-127.0.0.1}"
PROXYSQL_ADMIN_PORT="${PROXYSQL_ADMIN_PORT:-6032}"
PROXYSQL_ADMIN_USER="${PROXYSQL_ADMIN_USER:-admin}"
PROXYSQL_ADMIN_PASSWORD="${PROXYSQL_ADMIN_PASSWORD:-admin}"

MARIADB_NODE1="${MARIADB_NODE1:-crm-mariadb-node1}"
MARIADB_NODE2="${MARIADB_NODE2:-crm-mariadb-node2}"
MARIADB_NODE3="${MARIADB_NODE3:-crm-mariadb-node3}"
MARIADB_PORT="${MARIADB_PORT:-3306}"

CRM_USER="${CRM_USER:-crm_user}"
CRM_PASSWORD="${CRM_PASSWORD:-CrmPass@Dev2024}"
CRM_READONLY_USER="${CRM_READONLY_USER:-crm_readonly}"
CRM_READONLY_PASSWORD="${CRM_READONLY_PASSWORD:-ReadOnlyPass@Dev2024}"
MONITOR_USER="${MONITOR_USER:-proxysql_monitor}"
MONITOR_PASSWORD="${MONITOR_PASSWORD:-MonitorPass@Dev2024}"

log() { echo "[$(date '+%Y-%m-%d %H:%M:%S')] $*"; }

# ---------------------------------------------------------------------------
# Helper — run SQL against ProxySQL admin interface
# ---------------------------------------------------------------------------
proxysql_sql() {
  mysql \
    -h "${PROXYSQL_HOST}" \
    -P "${PROXYSQL_ADMIN_PORT}" \
    -u "${PROXYSQL_ADMIN_USER}" \
    -p"${PROXYSQL_ADMIN_PASSWORD}" \
    -e "$1" 2>/dev/null
}

# ---------------------------------------------------------------------------
# Wait for ProxySQL admin port
# ---------------------------------------------------------------------------
wait_for_proxysql() {
  log "Waiting for ProxySQL admin port ${PROXYSQL_HOST}:${PROXYSQL_ADMIN_PORT}..."
  local attempts=0
  until mysql \
      -h "${PROXYSQL_HOST}" -P "${PROXYSQL_ADMIN_PORT}" \
      -u "${PROXYSQL_ADMIN_USER}" -p"${PROXYSQL_ADMIN_PASSWORD}" \
      -e "SELECT 1" >/dev/null 2>&1; do
    ((attempts++))
    [[ ${attempts} -ge 30 ]] && { log "ERROR: ProxySQL not reachable after 30 attempts."; exit 1; }
    sleep 2
  done
  log "ProxySQL is ready."
}

# ---------------------------------------------------------------------------
# 1. Monitor credentials
# ---------------------------------------------------------------------------
configure_monitor() {
  log "Setting monitor credentials..."
  proxysql_sql "UPDATE global_variables
    SET variable_value = '${MONITOR_USER}'
    WHERE variable_name = 'mysql-monitor_username';"
  proxysql_sql "UPDATE global_variables
    SET variable_value = '${MONITOR_PASSWORD}'
    WHERE variable_name = 'mysql-monitor_password';"
  proxysql_sql "LOAD MYSQL VARIABLES TO RUNTIME;"
  proxysql_sql "SAVE MYSQL VARIABLES TO DISK;"
}

# ---------------------------------------------------------------------------
# 2. Backend servers
#    Clears existing entries and re-inserts for idempotent run
# ---------------------------------------------------------------------------
configure_servers() {
  log "Configuring backend servers..."
  proxysql_sql "DELETE FROM mysql_servers;"
  proxysql_sql "INSERT INTO mysql_servers
    (hostgroup_id, hostname, port, weight, status, comment)
    VALUES
    (10, '${MARIADB_NODE1}', ${MARIADB_PORT}, 1, 'ONLINE', 'node1-write'),
    (20, '${MARIADB_NODE2}', ${MARIADB_PORT}, 1, 'ONLINE', 'node2-read'),
    (20, '${MARIADB_NODE3}', ${MARIADB_PORT}, 1, 'ONLINE', 'node3-read');"
  proxysql_sql "LOAD MYSQL SERVERS TO RUNTIME;"
  proxysql_sql "SAVE MYSQL SERVERS TO DISK;"
  log "Backend servers configured."
}

# ---------------------------------------------------------------------------
# 3. Application users
# ---------------------------------------------------------------------------
configure_users() {
  log "Configuring MySQL users..."
  proxysql_sql "DELETE FROM mysql_users WHERE username IN
    ('${CRM_USER}','${CRM_READONLY_USER}');"
  proxysql_sql "INSERT INTO mysql_users
    (username, password, default_hostgroup, default_schema,
     active, transaction_persistent)
    VALUES
    ('${CRM_USER}', '${CRM_PASSWORD}', 10, 'crm_db', 1, 1),
    ('${CRM_READONLY_USER}', '${CRM_READONLY_PASSWORD}', 20, 'crm_db', 1, 0);"
  proxysql_sql "LOAD MYSQL USERS TO RUNTIME;"
  proxysql_sql "SAVE MYSQL USERS TO DISK;"
  log "Users configured."
}

# ---------------------------------------------------------------------------
# 4. Query routing rules
# ---------------------------------------------------------------------------
configure_query_rules() {
  log "Configuring query routing rules..."
  proxysql_sql "DELETE FROM mysql_query_rules
    WHERE rule_id IN (1,2,3);"
  proxysql_sql "INSERT INTO mysql_query_rules
    (rule_id, active, match_pattern, destination_hostgroup, apply, comment)
    VALUES
    (1, 1, '^SELECT .* FOR UPDATE', 10, 1, 'SELECT FOR UPDATE -> write'),
    (2, 1, '^(BEGIN|COMMIT|ROLLBACK|START TRANSACTION)', 10, 1, 'Transactions -> write'),
    (3, 1, '^SELECT', 20, 1, 'SELECTs -> read');"
  proxysql_sql "LOAD MYSQL QUERY RULES TO RUNTIME;"
  proxysql_sql "SAVE MYSQL QUERY RULES TO DISK;"
  log "Query rules configured."
}

# ---------------------------------------------------------------------------
# 5. Galera hostgroups (writer/reader monitoring)
# ---------------------------------------------------------------------------
configure_galera_hostgroups() {
  log "Configuring Galera hostgroups..."
  proxysql_sql "DELETE FROM mysql_galera_hostgroups;"
  proxysql_sql "INSERT INTO mysql_galera_hostgroups
    (writer_hostgroup, reader_hostgroup, backup_writer_hostgroup,
     offline_hostgroup, active, max_writers, writer_is_also_reader,
     max_transactions_behind)
    VALUES (10, 20, 30, 9999, 1, 1, 0, 10);"
  proxysql_sql "LOAD MYSQL GALERA HOSTGROUPS TO RUNTIME;"
  proxysql_sql "SAVE MYSQL GALERA HOSTGROUPS TO DISK;"
  log "Galera hostgroups configured."
}

# ---------------------------------------------------------------------------
# 6. Verify configuration
# ---------------------------------------------------------------------------
verify() {
  log "Verifying configuration..."
  log "  Backend servers:"
  proxysql_sql "SELECT hostgroup_id, hostname, port, status FROM mysql_servers;" | column -t
  log "  Query rules:"
  proxysql_sql "SELECT rule_id, match_pattern, destination_hostgroup FROM mysql_query_rules;" | column -t
  log "  Users:"
  proxysql_sql "SELECT username, default_hostgroup FROM mysql_users;" | column -t
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
main() {
  log "===== ProxySQL Configuration Started ====="
  wait_for_proxysql
  configure_monitor
  configure_servers
  configure_users
  configure_query_rules
  configure_galera_hostgroups
  verify
  log "===== ProxySQL Configuration Completed ====="
  log ""
  log "Application connection string:"
  log "  Server=${PROXYSQL_HOST};Port=6033;Database=crm_db;User=${CRM_USER};Password=<password>"
}

main "$@"
