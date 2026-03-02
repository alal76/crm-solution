#!/bin/bash
# =============================================================================
# CRM Solution — MariaDB Backup Script
# TODO-DB-003: Logical backup (mysqldump) + Physical backup (mariabackup)
#
# Usage: ./backup-mariadb.sh
# Environment variables: see docker-compose.backup.yml
#
# What this script does:
#   1. Creates a logical backup via mysqldump (for individual table restores)
#   2. Creates a physical backup via mariabackup (for full instance recovery)
#   3. Compresses both to .tar.gz
#   4. Encrypts the archives via backup-encrypt.sh
#   5. Uploads to MinIO (daily/weekly/monthly buckets)
#   6. Prunes local staging copies, keeping LOCAL_BACKUP_RETENTION most-recent
#
# chmod +x scripts/backup-mariadb.sh
# =============================================================================
set -euo pipefail

# ---------------------------------------------------------------------------
# Configuration (override via environment variables)
# ---------------------------------------------------------------------------
MARIADB_HOST="${MARIADB_HOST:-crm-mariadb}"
MARIADB_PORT="${MARIADB_PORT:-3306}"
MARIADB_USER="${MARIADB_USER:-crm_user}"
MARIADB_PASSWORD="${MARIADB_PASSWORD:?Set MARIADB_PASSWORD}"
MARIADB_ROOT_PASSWORD="${MARIADB_ROOT_PASSWORD:?Set MARIADB_ROOT_PASSWORD}"
MARIADB_DATABASE="${MARIADB_DATABASE:-crm_db}"

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
TIMESTAMP="$(date +%Y%m%d_%H%M%S)"
DAY_OF_WEEK="$(date +%u)"    # 1=Monday … 7=Sunday
DAY_OF_MONTH="$(date +%d)"

LOGICAL_ARCHIVE="${BACKUP_STAGING_DIR}/mariadb_logical_${TIMESTAMP}.sql.gz"
PHYSICAL_DIR="${BACKUP_STAGING_DIR}/mariadb_physical_${TIMESTAMP}"
PHYSICAL_ARCHIVE="${BACKUP_STAGING_DIR}/mariadb_physical_${TIMESTAMP}.tar.gz"

log() { echo "[$(date '+%Y-%m-%d %H:%M:%S')] $*"; }

# ---------------------------------------------------------------------------
# Determine destination bucket (monthly > weekly > daily)
# ---------------------------------------------------------------------------
select_bucket() {
  if [[ "${DAY_OF_MONTH}" == "01" ]]; then
    echo "${MINIO_BUCKET_MONTHLY}"
  elif [[ "${DAY_OF_WEEK}" == "7" ]]; then
    echo "${MINIO_BUCKET_WEEKLY}"
  else
    echo "${MINIO_BUCKET_DAILY}"
  fi
}

# ---------------------------------------------------------------------------
# 1. Configure MinIO client alias
# ---------------------------------------------------------------------------
configure_mc() {
  log "Configuring MinIO client alias..."
  mc alias set crm_backup "${MINIO_ENDPOINT}" "${MINIO_ACCESS_KEY}" "${MINIO_SECRET_KEY}" --quiet
}

# ---------------------------------------------------------------------------
# 2. Logical backup — mysqldump
#    Produces a compressed SQL dump suitable for per-table restores
# ---------------------------------------------------------------------------
logical_backup() {
  log "Starting logical backup (mysqldump) of ${MARIADB_DATABASE}..."
  mysqldump \
    --host="${MARIADB_HOST}" \
    --port="${MARIADB_PORT}" \
    --user="root" \
    --password="${MARIADB_ROOT_PASSWORD}" \
    --single-transaction \
    --routines \
    --triggers \
    --events \
    --flush-logs \
    --master-data=2 \
    "${MARIADB_DATABASE}" \
  | gzip > "${LOGICAL_ARCHIVE}"
  log "Logical backup written to ${LOGICAL_ARCHIVE} ($(du -sh "${LOGICAL_ARCHIVE}" | cut -f1))"
}

# ---------------------------------------------------------------------------
# 3. Physical backup — mariabackup
#    Full filesystem-level backup; faster to restore for large databases
# ---------------------------------------------------------------------------
physical_backup() {
  log "Starting physical backup (mariabackup)..."
  mkdir -p "${PHYSICAL_DIR}"
  mariabackup \
    --backup \
    --target-dir="${PHYSICAL_DIR}" \
    --user="root" \
    --password="${MARIADB_ROOT_PASSWORD}" \
    --host="${MARIADB_HOST}" \
    --port="${MARIADB_PORT}"
  # Prepare (apply redo logs) so the backup is ready to restore
  mariabackup --prepare --target-dir="${PHYSICAL_DIR}"
  # Archive the prepared backup
  tar -czf "${PHYSICAL_ARCHIVE}" -C "${BACKUP_STAGING_DIR}" "mariadb_physical_${TIMESTAMP}"
  rm -rf "${PHYSICAL_DIR}"
  log "Physical backup written to ${PHYSICAL_ARCHIVE} ($(du -sh "${PHYSICAL_ARCHIVE}" | cut -f1))"
}

# ---------------------------------------------------------------------------
# 4. Encrypt archives
# ---------------------------------------------------------------------------
encrypt_archives() {
  log "Encrypting backup archives..."
  if [[ -x /usr/local/bin/backup-encrypt.sh ]]; then
    LOGICAL_ENCRYPTED="$(/usr/local/bin/backup-encrypt.sh "${LOGICAL_ARCHIVE}")"
    PHYSICAL_ENCRYPTED="$(/usr/local/bin/backup-encrypt.sh "${PHYSICAL_ARCHIVE}")"
    # Remove unencrypted originals
    rm -f "${LOGICAL_ARCHIVE}" "${PHYSICAL_ARCHIVE}"
    LOGICAL_ARCHIVE="${LOGICAL_ENCRYPTED}"
    PHYSICAL_ARCHIVE="${PHYSICAL_ENCRYPTED}"
    log "Encryption complete."
  else
    log "WARNING: backup-encrypt.sh not found — uploading unencrypted backups."
  fi
}

# ---------------------------------------------------------------------------
# 5. Upload to MinIO
# ---------------------------------------------------------------------------
upload_to_minio() {
  local bucket
  bucket="$(select_bucket)"
  log "Uploading to MinIO bucket: ${bucket}..."
  mc cp "${LOGICAL_ARCHIVE}" "crm_backup/${bucket}/mariadb/logical/$(basename "${LOGICAL_ARCHIVE}")"
  mc cp "${PHYSICAL_ARCHIVE}" "crm_backup/${bucket}/mariadb/physical/$(basename "${PHYSICAL_ARCHIVE}")"
  log "Upload complete."
}

# ---------------------------------------------------------------------------
# 6. Rotate local copies — keep last N files per type
# ---------------------------------------------------------------------------
rotate_local() {
  log "Rotating local backups (keeping ${LOCAL_BACKUP_RETENTION} most-recent)..."
  # Count and delete oldest logical backups
  find "${BACKUP_STAGING_DIR}" -maxdepth 1 -name "mariadb_logical_*" \
    | sort -r \
    | tail -n "+$((LOCAL_BACKUP_RETENTION + 1))" \
    | xargs -r rm -f
  # Count and delete oldest physical backups
  find "${BACKUP_STAGING_DIR}" -maxdepth 1 -name "mariadb_physical_*" \
    | sort -r \
    | tail -n "+$((LOCAL_BACKUP_RETENTION + 1))" \
    | xargs -r rm -f
  log "Local rotation done."
}

# ---------------------------------------------------------------------------
# 7. Generate checksum manifest
# ---------------------------------------------------------------------------
generate_checksums() {
  log "Generating SHA-256 checksums..."
  sha256sum "${LOGICAL_ARCHIVE}" "${PHYSICAL_ARCHIVE}" \
    > "${BACKUP_STAGING_DIR}/mariadb_checksums_${TIMESTAMP}.sha256"
  mc cp \
    "${BACKUP_STAGING_DIR}/mariadb_checksums_${TIMESTAMP}.sha256" \
    "crm_backup/$(select_bucket)/mariadb/checksums/mariadb_checksums_${TIMESTAMP}.sha256"
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
main() {
  log "===== MariaDB Backup Started ====="
  mkdir -p "${BACKUP_STAGING_DIR}"
  configure_mc
  logical_backup
  physical_backup
  encrypt_archives
  generate_checksums
  upload_to_minio
  rotate_local
  log "===== MariaDB Backup Completed Successfully ====="
}

main "$@"
