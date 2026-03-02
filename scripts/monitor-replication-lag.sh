#!/bin/bash
# =============================================================================
# CRM Solution — Replication Lag Monitor
# TODO-DB-021: Queries SHOW SLAVE STATUS, alerts if lag > 30 seconds
#
# What this script does:
#   1. Connects to analytics replica (crm-mariadb-analytics:3307)
#   2. Reads Seconds_Behind_Master from SHOW SLAVE STATUS
#   3. Alerts (stderr + exit code 1) if lag > ALERT_THRESHOLD_SECONDS
#   4. Prints structured status report
#   5. Optionally sends a webhook alert (Slack/Teams) if ALERT_WEBHOOK_URL set
#
# Usage:
#   ./monitor-replication-lag.sh
#   # Or as a cron job:
#   */5 * * * * /scripts/monitor-replication-lag.sh >> /var/log/replication-lag.log 2>&1
#
# chmod +x scripts/monitor-replication-lag.sh
# =============================================================================
set -euo pipefail

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------
REPLICA_HOST="${REPLICA_HOST:-127.0.0.1}"
REPLICA_PORT="${REPLICA_PORT:-3307}"
REPLICA_ROOT_PASSWORD="${REPLICA_ROOT_PASSWORD:?Set REPLICA_ROOT_PASSWORD}"

ALERT_THRESHOLD_SECONDS="${ALERT_THRESHOLD_SECONDS:-30}"
ALERT_WEBHOOK_URL="${ALERT_WEBHOOK_URL:-}"   # Set for Slack/Teams notifications
HOSTNAME_LABEL="${HOSTNAME_LABEL:-$(hostname -s 2>/dev/null || echo 'crm-server')}"

log()   { echo "[$(date '+%Y-%m-%d %H:%M:%S')] $*"; }
alert() { echo "[$(date '+%Y-%m-%d %H:%M:%S')] ALERT: $*" >&2; }

# ---------------------------------------------------------------------------
# Query replica status
# ---------------------------------------------------------------------------
get_slave_status() {
  mysql \
    -h "${REPLICA_HOST}" \
    -P "${REPLICA_PORT}" \
    -u root \
    -p"${REPLICA_ROOT_PASSWORD}" \
    --batch \
    --skip-column-names \
    -e "SHOW SLAVE STATUS\G" 2>/dev/null
}

extract_field() {
  local field="$1"
  echo "${SLAVE_STATUS}" | grep "^ *${field}:" | awk -F': ' '{print $2}' | xargs
}

# ---------------------------------------------------------------------------
# Send webhook alert
# ---------------------------------------------------------------------------
send_webhook_alert() {
  local message="$1"
  if [[ -z "${ALERT_WEBHOOK_URL}" ]]; then
    return
  fi
  # Generic JSON compatible with both Slack and Teams incoming webhooks
  local payload
  payload="{\"text\": \"🚨 CRM Replication Alert on ${HOSTNAME_LABEL}: ${message}\"}"
  curl -s -X POST \
    -H "Content-Type: application/json" \
    -d "${payload}" \
    "${ALERT_WEBHOOK_URL}" >/dev/null 2>&1 || true
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
main() {
  log "===== Replication Lag Monitor ====="
  log "  Replica:   ${REPLICA_HOST}:${REPLICA_PORT}"
  log "  Threshold: ${ALERT_THRESHOLD_SECONDS}s"

  # ---------------------------------------------------------------------------
  # Connectivity check
  # ---------------------------------------------------------------------------
  if ! mysqladmin ping -h "${REPLICA_HOST}" -P "${REPLICA_PORT}" \
      -u root -p"${REPLICA_ROOT_PASSWORD}" --silent 2>/dev/null; then
    alert "Cannot connect to analytics replica at ${REPLICA_HOST}:${REPLICA_PORT}"
    send_webhook_alert "Cannot connect to analytics replica at ${REPLICA_HOST}:${REPLICA_PORT}"
    exit 1
  fi

  # ---------------------------------------------------------------------------
  # Fetch slave status
  # ---------------------------------------------------------------------------
  SLAVE_STATUS="$(get_slave_status)"
  if [[ -z "${SLAVE_STATUS}" ]]; then
    alert "SHOW SLAVE STATUS returned empty — replication may not be configured."
    send_webhook_alert "Replication not configured or slave status empty on ${REPLICA_HOST}:${REPLICA_PORT}"
    exit 1
  fi

  # ---------------------------------------------------------------------------
  # Extract key fields
  # ---------------------------------------------------------------------------
  SLAVE_IO_RUNNING="$(extract_field "Slave_IO_Running")"
  SLAVE_SQL_RUNNING="$(extract_field "Slave_SQL_Running")"
  SECONDS_BEHIND="$(extract_field "Seconds_Behind_Master")"
  LAST_IO_ERROR="$(extract_field "Last_IO_Error")"
  LAST_SQL_ERROR="$(extract_field "Last_SQL_Error")"
  MASTER_HOST="$(extract_field "Master_Host")"
  RELAY_LOG_POS="$(extract_field "Relay_Log_Pos")"
  EXEC_LOG_POS="$(extract_field "Exec_Master_Log_Pos")"

  # ---------------------------------------------------------------------------
  # Print status report
  # ---------------------------------------------------------------------------
  log "  Slave_IO_Running:       ${SLAVE_IO_RUNNING:-unknown}"
  log "  Slave_SQL_Running:      ${SLAVE_SQL_RUNNING:-unknown}"
  log "  Seconds_Behind_Master:  ${SECONDS_BEHIND:-NULL}"
  log "  Master_Host:            ${MASTER_HOST:-unknown}"
  log "  Exec_Master_Log_Pos:    ${EXEC_LOG_POS:-unknown}"

  # ---------------------------------------------------------------------------
  # Check IO thread
  # ---------------------------------------------------------------------------
  if [[ "${SLAVE_IO_RUNNING}" != "Yes" ]]; then
    alert "Slave IO thread is NOT running. Last IO error: ${LAST_IO_ERROR:-none}"
    send_webhook_alert "Slave IO thread NOT running on ${REPLICA_HOST}. Error: ${LAST_IO_ERROR:-none}"
    exit 1
  fi

  # ---------------------------------------------------------------------------
  # Check SQL thread
  # ---------------------------------------------------------------------------
  if [[ "${SLAVE_SQL_RUNNING}" != "Yes" ]]; then
    alert "Slave SQL thread is NOT running. Last SQL error: ${LAST_SQL_ERROR:-none}"
    send_webhook_alert "Slave SQL thread NOT running on ${REPLICA_HOST}. Error: ${LAST_SQL_ERROR:-none}"
    exit 1
  fi

  # ---------------------------------------------------------------------------
  # Check lag threshold
  # ---------------------------------------------------------------------------
  if [[ "${SECONDS_BEHIND}" == "NULL" ]]; then
    alert "Seconds_Behind_Master is NULL — replica may have lost connection to master."
    send_webhook_alert "Replica ${REPLICA_HOST}:${REPLICA_PORT} lag is NULL (disconnected?)."
    exit 1
  fi

  if (( SECONDS_BEHIND > ALERT_THRESHOLD_SECONDS )); then
    alert "Replication lag is ${SECONDS_BEHIND}s — exceeds threshold of ${ALERT_THRESHOLD_SECONDS}s!"
    send_webhook_alert "Replication lag ${SECONDS_BEHIND}s > threshold ${ALERT_THRESHOLD_SECONDS}s on ${REPLICA_HOST}:${REPLICA_PORT}"
    exit 1
  fi

  log "  ✓ Replication healthy (lag: ${SECONDS_BEHIND}s ≤ ${ALERT_THRESHOLD_SECONDS}s)"
  log "===== Monitor Complete ====="
}

main "$@"
