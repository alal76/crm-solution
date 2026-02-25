#!/bin/bash
# =============================================================================
# CRM Solution — PostgreSQL Backup Script
# TODO-DB-004: pg_dump custom-format backup + WAL archiving config snippet
#
# Usage: ./backup-postgresql.sh
# Environment variables: see docker-compose.backup.yml
#
# What this script does:
#   1. Creates a pg_dump backup in custom (-Fc) format — supports parallel restore
#   2. Creates a globals dump (roles, tablespaces)
#   3. Compresses to .tar.gz
#   4. Encrypts via backup-encrypt.sh
#   5. Uploads to MinIO
#   6. Rotates local copies
#   7. Prints WAL archiving configuration snippet
#
# chmod +x scripts/backup-postgresql.sh
# =============================================================================
set -euo pipefail

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------
POSTGRES_HOST="${POSTGRES_HOST:-crm-postgresql}"
POSTGRES_PORT="${POSTGRES_PORT:-5432}"
POSTGRES_USER="${POSTGRES_USER:-crm_user}"
POSTGRES_PASSWORD="${POSTGRES_PASSWORD:-CrmPass@Dev2024!}"
POSTGRES_DB="${POSTGRES_DB:-crm_db}"

MINIO_ENDPOINT="${MINIO_ENDPOINT:-http://crm-minio:9000}"
MINIO_ACCESS_KEY="${MINIO_ACCESS_KEY:-minio_admin}"
MINIO_SECRET_KEY="${MINIO_SECRET_KEY:-MinioPass@Dev2024}"
MINIO_BUCKET_DAILY="${MINIO_BUCKET_DAILY:-crm-backups-daily}"
MINIO_BUCKET_WEEKLY="${MINIO_BUCKET_WEEKLY:-crm-backups-weekly}"
MINIO_BUCKET_MONTHLY="${MINIO_BUCKET_MONTHLY:-crm-backups-monthly}"

BACKUP_STAGING_DIR="${BACKUP_STAGING_DIR:-/backup-staging}"
LOCAL_BACKUP_RETENTION="${LOCAL_BACKUP_RETENTION:-3}"

# ---------------------------------------------------------------------------
# Derived variables
# ---------------------------------------------------------------------------
export PGPASSWORD="${POSTGRES_PASSWORD}"
TIMESTAMP="$(date +%Y%m%d_%H%M%S)"
DAY_OF_WEEK="$(date +%u)"
DAY_OF_MONTH="$(date +%d)"

DUMP_FILE="${BACKUP_STAGING_DIR}/postgresql_${POSTGRES_DB}_${TIMESTAMP}.dump"
GLOBALS_FILE="${BACKUP_STAGING_DIR}/postgresql_globals_${TIMESTAMP}.sql"
ARCHIVE_FILE="${BACKUP_STAGING_DIR}/postgresql_${POSTGRES_DB}_${TIMESTAMP}.tar.gz"

log() { echo "[$(date '+%Y-%m-%d %H:%M:%S')] $*"; }

select_bucket() {
  if [[ "${DAY_OF_MONTH}" == "01" ]]; then echo "${MINIO_BUCKET_MONTHLY}";
  elif [[ "${DAY_OF_WEEK}" == "7" ]]; then echo "${MINIO_BUCKET_WEEKLY}";
  else echo "${MINIO_BUCKET_DAILY}"; fi
}

# ---------------------------------------------------------------------------
# 1. Configure MinIO client
# ---------------------------------------------------------------------------
configure_mc() {
  log "Configuring MinIO client alias..."
  mc alias set crm_backup "${MINIO_ENDPOINT}" "${MINIO_ACCESS_KEY}" "${MINIO_SECRET_KEY}" --quiet
}

# ---------------------------------------------------------------------------
# 2. pg_dump in custom format
#    Custom format (-Fc) enables parallel restore with pg_restore -j N
# ---------------------------------------------------------------------------
dump_database() {
  log "Starting pg_dump (custom format) of ${POSTGRES_DB}..."
  pg_dump \
    -h "${POSTGRES_HOST}" \
    -p "${POSTGRES_PORT}" \
    -U "${POSTGRES_USER}" \
    -d "${POSTGRES_DB}" \
    --format=custom \
    --compress=9 \
    --verbose \
    --file="${DUMP_FILE}"
  log "pg_dump written to ${DUMP_FILE} ($(du -sh "${DUMP_FILE}" | cut -f1))"
}

# ---------------------------------------------------------------------------
# 3. Global objects (roles, tablespaces) — needed for full cluster restore
# ---------------------------------------------------------------------------
dump_globals() {
  log "Dumping global objects (roles/tablespaces)..."
  pg_dumpall \
    -h "${POSTGRES_HOST}" \
    -p "${POSTGRES_PORT}" \
    -U "${POSTGRES_USER}" \
    --globals-only \
    --file="${GLOBALS_FILE}"
  log "Globals written to ${GLOBALS_FILE}"
}

# ---------------------------------------------------------------------------
# 4. Archive both files
# ---------------------------------------------------------------------------
archive_dumps() {
  log "Archiving dump files..."
  tar -czf "${ARCHIVE_FILE}" \
    -C "${BACKUP_STAGING_DIR}" \
    "$(basename "${DUMP_FILE}")" \
    "$(basename "${GLOBALS_FILE}")"
  rm -f "${DUMP_FILE}" "${GLOBALS_FILE}"
  log "Archive written to ${ARCHIVE_FILE} ($(du -sh "${ARCHIVE_FILE}" | cut -f1))"
}

# ---------------------------------------------------------------------------
# 5. Encrypt archive
# ---------------------------------------------------------------------------
encrypt_archive() {
  log "Encrypting archive..."
  if [[ -x /usr/local/bin/backup-encrypt.sh ]]; then
    ARCHIVE_ENCRYPTED="$(/usr/local/bin/backup-encrypt.sh "${ARCHIVE_FILE}")"
    rm -f "${ARCHIVE_FILE}"
    ARCHIVE_FILE="${ARCHIVE_ENCRYPTED}"
    log "Encryption complete."
  else
    log "WARNING: backup-encrypt.sh not found — storing unencrypted."
  fi
}

# ---------------------------------------------------------------------------
# 6. Upload to MinIO
# ---------------------------------------------------------------------------
upload_to_minio() {
  local bucket
  bucket="$(select_bucket)"
  log "Uploading to MinIO bucket: ${bucket}..."
  mc cp "${ARCHIVE_FILE}" "crm_backup/${bucket}/postgresql/$(basename "${ARCHIVE_FILE}")"
  # Generate and upload checksum
  sha256sum "${ARCHIVE_FILE}" > "${ARCHIVE_FILE}.sha256"
  mc cp "${ARCHIVE_FILE}.sha256" "crm_backup/${bucket}/postgresql/checksums/$(basename "${ARCHIVE_FILE}").sha256"
  rm -f "${ARCHIVE_FILE}.sha256"
  log "Upload complete."
}

# ---------------------------------------------------------------------------
# 7. Rotate local copies
# ---------------------------------------------------------------------------
rotate_local() {
  log "Rotating local PostgreSQL backups (keeping ${LOCAL_BACKUP_RETENTION})..."
  find "${BACKUP_STAGING_DIR}" -maxdepth 1 -name "postgresql_${POSTGRES_DB}_*" \
    | sort -r \
    | tail -n "+$((LOCAL_BACKUP_RETENTION + 1))" \
    | xargs -r rm -f
  log "Rotation done."
}

# ---------------------------------------------------------------------------
# 8. Print WAL archiving configuration snippet (for reference)
# ---------------------------------------------------------------------------
print_wal_config_snippet() {
  log ""
  log "========================================================================"
  log "WAL Archiving Configuration Snippet (add to postgresql.conf):"
  log "========================================================================"
  cat <<'EOF'
# ── WAL Archiving for Point-In-Time Recovery (PITR) ──────────────────────────
# Add these settings to postgresql.conf and restart PostgreSQL.
#
# wal_level = replica               # Minimum for archiving
# archive_mode = on                 # Enable WAL archiving
# archive_command = 'mc cp %p crm_backup/crm-backups-daily/postgresql/wal/%f'
# archive_timeout = 300             # Force segment switch every 5 min
#
# For PITR restore:
#   1. Restore the base backup
#   2. Create recovery.conf (or recovery settings in postgresql.conf for PG12+):
#      restore_command = 'mc get crm_backup/crm-backups-daily/postgresql/wal/%f %p'
#      recovery_target_time = '2026-02-25 14:30:00'
#   3. Start PostgreSQL — it will replay WAL up to the target time
# ─────────────────────────────────────────────────────────────────────────────
EOF
  log "========================================================================"
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
main() {
  log "===== PostgreSQL Backup Started ====="
  mkdir -p "${BACKUP_STAGING_DIR}"
  configure_mc
  dump_database
  dump_globals
  archive_dumps
  encrypt_archive
  upload_to_minio
  rotate_local
  print_wal_config_snippet
  log "===== PostgreSQL Backup Completed Successfully ====="
}

main "$@"
