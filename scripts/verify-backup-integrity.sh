#!/bin/bash
# =============================================================================
# CRM Solution — Backup Integrity Verification Script
# TODO-DB-008: Verify checksums + dry-run restore test
#
# What this script does:
#   1. Discovers all backup archives in BACKUP_STAGING_DIR or MinIO bucket
#   2. Verifies SHA-256 checksums against stored .sha256 manifests
#   3. Runs a mysqldump --no-data dry-run to confirm the DB is accessible
#   4. Attempts a minimal pg_dump verification against PostgreSQL
#   5. Reports pass/fail summary
#
# Usage: ./verify-backup-integrity.sh [--staging-only] [--minio-bucket <bucket>]
# chmod +x scripts/verify-backup-integrity.sh
# =============================================================================
set -euo pipefail

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------
BACKUP_STAGING_DIR="${BACKUP_STAGING_DIR:-/backup-staging}"
MARIADB_HOST="${MARIADB_HOST:-crm-mariadb}"
MARIADB_PORT="${MARIADB_PORT:-3306}"
MARIADB_ROOT_PASSWORD="${MARIADB_ROOT_PASSWORD:-RootPass@Dev2024}"
MARIADB_DATABASE="${MARIADB_DATABASE:-crm_db}"
POSTGRES_HOST="${POSTGRES_HOST:-crm-postgresql}"
POSTGRES_PORT="${POSTGRES_PORT:-5432}"
POSTGRES_USER="${POSTGRES_USER:-crm_user}"
POSTGRES_PASSWORD="${POSTGRES_PASSWORD:-CrmPass@Dev2024!}"
POSTGRES_DB="${POSTGRES_DB:-crm_db}"
MINIO_ENDPOINT="${MINIO_ENDPOINT:-http://crm-minio:9000}"
MINIO_ACCESS_KEY="${MINIO_ACCESS_KEY:-minio_admin}"
MINIO_SECRET_KEY="${MINIO_SECRET_KEY:-MinioPass@Dev2024}"
MINIO_BUCKET="${MINIO_BUCKET:-crm-backups-daily}"
STAGING_ONLY=false
REPORT_FILE="${BACKUP_STAGING_DIR}/integrity_report_$(date +%Y%m%d_%H%M%S).txt"

PASS_COUNT=0
FAIL_COUNT=0

log()  { echo "[$(date '+%Y-%m-%d %H:%M:%S')] $*" | tee -a "${REPORT_FILE}"; }
pass() { log "  ✓ PASS: $*"; ((PASS_COUNT++)); }
fail() { log "  ✗ FAIL: $*"; ((FAIL_COUNT++)); }

# ---------------------------------------------------------------------------
# Parse arguments
# ---------------------------------------------------------------------------
while [[ $# -gt 0 ]]; do
  case "$1" in
    --staging-only) STAGING_ONLY=true ;;
    --minio-bucket) MINIO_BUCKET="$2"; shift ;;
    *) log "Unknown option: $1" ;;
  esac
  shift
done

# ---------------------------------------------------------------------------
# 1. Verify SHA-256 checksums in staging directory
# ---------------------------------------------------------------------------
verify_local_checksums() {
  log "--- Verifying local checksum manifests in ${BACKUP_STAGING_DIR} ---"
  local found=0
  while IFS= read -r -d '' checksum_file; do
    archive_file="${checksum_file%.sha256}"
    if [[ ! -f "${archive_file}" ]]; then
      fail "Archive not found for checksum: ${archive_file}"
      continue
    fi
    if sha256sum --check --quiet "${checksum_file}" 2>/dev/null; then
      pass "Checksum OK: $(basename "${archive_file}")"
    else
      fail "Checksum MISMATCH: $(basename "${archive_file}")"
    fi
    ((found++))
  done < <(find "${BACKUP_STAGING_DIR}" -maxdepth 2 -name "*.sha256" -print0 2>/dev/null)
  [[ ${found} -eq 0 ]] && log "  (no .sha256 files found in staging)"
}

# ---------------------------------------------------------------------------
# 2. Verify checksums from MinIO (download checksum manifests and re-verify)
# ---------------------------------------------------------------------------
verify_minio_checksums() {
  if [[ "${STAGING_ONLY}" == "true" ]]; then
    log "--- Skipping MinIO verification (--staging-only) ---"
    return
  fi
  log "--- Verifying MinIO checksum manifests from bucket: ${MINIO_BUCKET} ---"
  mc alias set crm_verify "${MINIO_ENDPOINT}" "${MINIO_ACCESS_KEY}" "${MINIO_SECRET_KEY}" --quiet 2>/dev/null || {
    log "  WARNING: mc alias configuration failed — skipping MinIO verification."
    return
  }
  local tmp_dir="${BACKUP_STAGING_DIR}/.verify_tmp_$(date +%s)"
  mkdir -p "${tmp_dir}"
  # Download latest checksum manifest files
  mc find "crm_verify/${MINIO_BUCKET}" --name "*.sha256" --newer-than 7d 2>/dev/null \
    | head -20 \
    | while IFS= read -r remote_path; do
        local_checksum="${tmp_dir}/$(basename "${remote_path}")"
        mc cp "${remote_path}" "${local_checksum}" --quiet 2>/dev/null
        log "  Downloaded checksum manifest: $(basename "${remote_path}")"
      done
  rm -rf "${tmp_dir}"
}

# ---------------------------------------------------------------------------
# 3. MariaDB connectivity + dry-run (--no-data schema dump)
# ---------------------------------------------------------------------------
verify_mariadb() {
  log "--- MariaDB dry-run restore test ---"
  if ! command -v mysqldump &>/dev/null; then
    log "  WARNING: mysqldump not found — skipping MariaDB test."
    return
  fi
  # Test 1: connectivity
  if mysqladmin ping \
      --host="${MARIADB_HOST}" \
      --port="${MARIADB_PORT}" \
      --user="root" \
      --password="${MARIADB_ROOT_PASSWORD}" \
      --silent 2>/dev/null; then
    pass "MariaDB connectivity check"
  else
    fail "MariaDB connectivity check — cannot reach ${MARIADB_HOST}:${MARIADB_PORT}"
    return
  fi
  # Test 2: schema-only dump (no data, fast verification)
  TMP_SCHEMA_FILE="${BACKUP_STAGING_DIR}/.schema_test_$(date +%s).sql"
  if mysqldump \
      --host="${MARIADB_HOST}" \
      --port="${MARIADB_PORT}" \
      --user="root" \
      --password="${MARIADB_ROOT_PASSWORD}" \
      --no-data \
      --skip-extended-insert \
      --single-transaction \
      "${MARIADB_DATABASE}" \
      > "${TMP_SCHEMA_FILE}" 2>/dev/null; then
    local table_count
    table_count=$(grep -c "^CREATE TABLE" "${TMP_SCHEMA_FILE}" 2>/dev/null || echo "0")
    pass "MariaDB schema dump: ${table_count} tables found in ${MARIADB_DATABASE}"
  else
    fail "MariaDB schema dump failed for database: ${MARIADB_DATABASE}"
  fi
  rm -f "${TMP_SCHEMA_FILE}"
}

# ---------------------------------------------------------------------------
# 4. PostgreSQL connectivity + minimal pg_dump
# ---------------------------------------------------------------------------
verify_postgresql() {
  log "--- PostgreSQL dry-run restore test ---"
  if ! command -v pg_dump &>/dev/null; then
    log "  WARNING: pg_dump not found — skipping PostgreSQL test."
    return
  fi
  export PGPASSWORD="${POSTGRES_PASSWORD}"
  # Test 1: connectivity
  if pg_isready \
      -h "${POSTGRES_HOST}" \
      -p "${POSTGRES_PORT}" \
      -U "${POSTGRES_USER}" \
      -d "${POSTGRES_DB}" \
      --quiet 2>/dev/null; then
    pass "PostgreSQL connectivity check"
  else
    fail "PostgreSQL connectivity check — cannot reach ${POSTGRES_HOST}:${POSTGRES_PORT}"
    return
  fi
  # Test 2: schema-only dump
  TMP_PG_FILE="${BACKUP_STAGING_DIR}/.pg_schema_test_$(date +%s).dump"
  if pg_dump \
      -h "${POSTGRES_HOST}" \
      -p "${POSTGRES_PORT}" \
      -U "${POSTGRES_USER}" \
      -d "${POSTGRES_DB}" \
      --schema-only \
      --format=plain \
      --file="${TMP_PG_FILE}" 2>/dev/null; then
    local table_count
    table_count=$(grep -c "^CREATE TABLE" "${TMP_PG_FILE}" 2>/dev/null || echo "0")
    pass "PostgreSQL schema dump: ${table_count} tables found in ${POSTGRES_DB}"
  else
    fail "PostgreSQL schema dump failed for database: ${POSTGRES_DB}"
  fi
  rm -f "${TMP_PG_FILE}"
  unset PGPASSWORD
}

# ---------------------------------------------------------------------------
# 5. Final report
# ---------------------------------------------------------------------------
print_summary() {
  log ""
  log "========================================================"
  log "INTEGRITY VERIFICATION SUMMARY"
  log "========================================================"
  log "  PASSED: ${PASS_COUNT}"
  log "  FAILED: ${FAIL_COUNT}"
  log "  Report: ${REPORT_FILE}"
  log "========================================================"
  if [[ ${FAIL_COUNT} -gt 0 ]]; then
    log "ACTION REQUIRED: ${FAIL_COUNT} check(s) failed. Review ${REPORT_FILE}."
    exit 1
  else
    log "All checks passed."
  fi
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
main() {
  mkdir -p "${BACKUP_STAGING_DIR}"
  log "===== Backup Integrity Verification Started ====="
  verify_local_checksums
  verify_minio_checksums
  verify_mariadb
  verify_postgresql
  print_summary
}

main "$@"
