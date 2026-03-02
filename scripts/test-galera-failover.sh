#!/bin/bash
# =============================================================================
# CRM Solution — Galera Cluster Failover Test Script
# TODO-DB-015: Simulate node1 failure, verify cluster continues, restore node
#
# What this script does:
#   1. Checks initial cluster state (3 nodes expected)
#   2. Stops crm-mariadb-node1 (simulates failure)
#   3. Waits 10 seconds for Galera to elect a new write coordinator
#   4. Runs a test query via ProxySQL (verifies cluster is still serving writes)
#   5. Restarts crm-mariadb-node1 and verifies it re-joins the cluster
#
# Usage: ./test-galera-failover.sh
# Prerequisites: Galera cluster running (docker-compose.galera.yml)
#               ProxySQL running (docker-compose.proxysql.yml)
#
# chmod +x scripts/test-galera-failover.sh
# =============================================================================
set -euo pipefail

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------
PROXYSQL_HOST="${PROXYSQL_HOST:-127.0.0.1}"
PROXYSQL_PORT="${PROXYSQL_PORT:-6033}"
MARIADB_USER="${MARIADB_USER:-crm_user}"
MARIADB_PASSWORD="${MARIADB_PASSWORD:?Set MARIADB_PASSWORD}"
MARIADB_DATABASE="${MARIADB_DATABASE:-crm_db}"

NODE1_CONTAINER="crm-mariadb-node1"
NODE2_CONTAINER="crm-mariadb-node2"
NODE3_CONTAINER="crm-mariadb-node3"

FAIL_WAIT_SECONDS=10    # Time to wait after stopping node1
REJOIN_WAIT_SECONDS=30  # Time to wait for node1 to re-sync

PASS=0
FAIL=0
log()  { echo "[$(date '+%Y-%m-%d %H:%M:%S')] $*"; }
pass() { log "  ✓ PASS: $*"; ((PASS++)); }
fail() { log "  ✗ FAIL: $*"; ((FAIL++)); }

# ---------------------------------------------------------------------------
# Helper — run query via ProxySQL
# ---------------------------------------------------------------------------
run_query() {
  mysql \
    -h "${PROXYSQL_HOST}" \
    -P "${PROXYSQL_PORT}" \
    -u "${MARIADB_USER}" \
    -p"${MARIADB_PASSWORD}" \
    -D "${MARIADB_DATABASE}" \
    -e "$1" 2>/dev/null
}

# ---------------------------------------------------------------------------
# Helper — get wsrep cluster size from a specific Galera node
# ---------------------------------------------------------------------------
get_cluster_size() {
  local node_container="$1"
  docker exec "${node_container}" \
    mysql -u root -p"${MARIADB_ROOT_PASSWORD:?Set MARIADB_ROOT_PASSWORD}" \
    -e "SHOW STATUS LIKE 'wsrep_cluster_size';" 2>/dev/null \
    | awk 'NR==2 {print $2}'
}

# ---------------------------------------------------------------------------
# 1. Pre-flight checks
# ---------------------------------------------------------------------------
preflight_checks() {
  log "--- Pre-flight Checks ---"

  # Verify all three node containers are running
  for node in "${NODE1_CONTAINER}" "${NODE2_CONTAINER}" "${NODE3_CONTAINER}"; do
    if docker inspect --format '{{.State.Running}}' "${node}" 2>/dev/null | grep -q "true"; then
      pass "Container running: ${node}"
    else
      fail "Container NOT running: ${node}. Aborting test."
      exit 1
    fi
  done

  # Verify cluster size is 3
  local cluster_size
  cluster_size="$(get_cluster_size "${NODE1_CONTAINER}")"
  if [[ "${cluster_size}" == "3" ]]; then
    pass "Initial cluster size: 3"
  else
    fail "Expected cluster size 3, got: ${cluster_size:-unknown}"
    exit 1
  fi

  # Verify ProxySQL connectivity
  if run_query "SELECT 1;" >/dev/null 2>&1; then
    pass "ProxySQL connectivity OK"
  else
    fail "Cannot connect via ProxySQL — is crm-proxysql running?"
    exit 1
  fi
}

# ---------------------------------------------------------------------------
# 2. Create test table and insert row (baseline write)
# ---------------------------------------------------------------------------
baseline_write() {
  log "--- Baseline Write Test ---"
  run_query "
    CREATE TABLE IF NOT EXISTS _galera_failover_test (
      id INT AUTO_INCREMENT PRIMARY KEY,
      ts DATETIME DEFAULT NOW(),
      note VARCHAR(100)
    ) ENGINE=InnoDB;" >/dev/null
  run_query "INSERT INTO _galera_failover_test (note) VALUES ('before-failover');" >/dev/null
  pass "Baseline write to _galera_failover_test succeeded"
}

# ---------------------------------------------------------------------------
# 3. Simulate node1 failure
# ---------------------------------------------------------------------------
stop_node1() {
  log "--- Simulating Node1 Failure ---"
  log "  Stopping container: ${NODE1_CONTAINER}..."
  docker stop "${NODE1_CONTAINER}" >/dev/null
  pass "Node1 (${NODE1_CONTAINER}) stopped"
}

# ---------------------------------------------------------------------------
# 4. Wait and verify cluster degrades gracefully
# ---------------------------------------------------------------------------
wait_and_verify_degraded() {
  log "--- Waiting ${FAIL_WAIT_SECONDS}s for cluster to stabilise... ---"
  sleep "${FAIL_WAIT_SECONDS}"

  # Cluster should now have 2 nodes
  local cluster_size
  cluster_size="$(get_cluster_size "${NODE2_CONTAINER}")"
  if [[ "${cluster_size}" == "2" ]]; then
    pass "Cluster degraded correctly to 2 nodes after node1 failure"
  else
    fail "Expected cluster size 2 after failure, got: ${cluster_size:-unknown}"
  fi
}

# ---------------------------------------------------------------------------
# 5. Verify writes still work with 2-node cluster
# ---------------------------------------------------------------------------
verify_writes_during_outage() {
  log "--- Verifying Writes During Node1 Outage ---"
  if run_query "INSERT INTO _galera_failover_test (note) VALUES ('during-outage');" >/dev/null 2>&1; then
    pass "Write succeeded during node1 outage (cluster still operational)"
  else
    fail "Write FAILED during node1 outage"
  fi

  if run_query "SELECT COUNT(*) FROM _galera_failover_test;" >/dev/null 2>&1; then
    pass "Read succeeded during node1 outage"
  else
    fail "Read FAILED during node1 outage"
  fi
}

# ---------------------------------------------------------------------------
# 6. Restart node1 and verify it rejoins
# ---------------------------------------------------------------------------
restart_node1() {
  log "--- Restarting Node1 (${NODE1_CONTAINER}) ---"
  docker start "${NODE1_CONTAINER}" >/dev/null
  log "  Waiting ${REJOIN_WAIT_SECONDS}s for node1 to sync via IST/SST..."
  sleep "${REJOIN_WAIT_SECONDS}"

  # Verify container is running
  if docker inspect --format '{{.State.Running}}' "${NODE1_CONTAINER}" | grep -q "true"; then
    pass "Node1 container is running"
  else
    fail "Node1 container failed to start"
    return
  fi

  # Verify cluster size is back to 3
  local cluster_size
  cluster_size="$(get_cluster_size "${NODE1_CONTAINER}")"
  if [[ "${cluster_size}" == "3" ]]; then
    pass "Cluster restored to 3 nodes after node1 rejoin"
  else
    fail "Expected cluster size 3 after rejoin, got: ${cluster_size:-unknown}"
  fi
}

# ---------------------------------------------------------------------------
# 7. Verify data consistency post-rejoin
# ---------------------------------------------------------------------------
verify_post_rejoin() {
  log "--- Verifying Data Consistency After Rejoin ---"
  local row_count
  row_count="$(run_query "SELECT COUNT(*) FROM _galera_failover_test;" \
    | awk 'NR==2 {print $1}')"
  if [[ "${row_count}" -ge "2" ]]; then
    pass "Data consistent after rejoin: ${row_count} rows in test table"
  else
    fail "Data inconsistency detected: expected ≥2 rows, got ${row_count:-0}"
  fi
}

# ---------------------------------------------------------------------------
# 8. Cleanup test table
# ---------------------------------------------------------------------------
cleanup() {
  log "--- Cleanup ---"
  run_query "DROP TABLE IF EXISTS _galera_failover_test;" >/dev/null 2>&1 || true
  log "  Test table dropped."
}

# ---------------------------------------------------------------------------
# Final report
# ---------------------------------------------------------------------------
summary() {
  log ""
  log "========================================================"
  log "GALERA FAILOVER TEST SUMMARY"
  log "========================================================"
  log "  PASSED: ${PASS}"
  log "  FAILED: ${FAIL}"
  log "========================================================"
  [[ ${FAIL} -gt 0 ]] && { log "FAILURES DETECTED. Review output above."; exit 1; }
  log "All failover tests passed!"
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
main() {
  log "===== Galera Cluster Failover Test Started ====="
  preflight_checks
  baseline_write
  stop_node1
  wait_and_verify_degraded
  verify_writes_during_outage
  restart_node1
  verify_post_rejoin
  cleanup
  summary
}

# Trap to ensure node1 is restarted even if script exits early
trap 'log "Script interrupted. Ensuring node1 is restarted..."; docker start '"${NODE1_CONTAINER}"' 2>/dev/null || true' EXIT INT TERM

main "$@"
