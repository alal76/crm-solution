#!/bin/bash
# =============================================================================
# CRM Solution — MinIO S3 Lifecycle Policy Configuration
# TODO-DB-005: Configure retention lifecycles for backup buckets
#
# Lifecycle policy:
#   crm-backups-daily   → expire after 14 days
#   crm-backups-weekly  → expire after 56 days  (8 weeks)
#   crm-backups-monthly → expire after 365 days (1 year)
#
# Prerequisites:
#   - MinIO server running and reachable
#   - mc (MinIO client) installed and in PATH
#
# Usage: ./configure-s3-lifecycle.sh
# chmod +x scripts/configure-s3-lifecycle.sh
# =============================================================================
set -euo pipefail

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------
MINIO_ENDPOINT="${MINIO_ENDPOINT:-http://localhost:9000}"
MINIO_ACCESS_KEY="${MINIO_ACCESS_KEY:-minio_admin}"
MINIO_SECRET_KEY="${MINIO_SECRET_KEY:-MinioPass@Dev2024}"
MC_ALIAS="${MC_ALIAS:-crm_lifecycle}"

# Retention periods in days
DAILY_EXPIRY_DAYS=14
WEEKLY_EXPIRY_DAYS=56
MONTHLY_EXPIRY_DAYS=365

log() { echo "[$(date '+%Y-%m-%d %H:%M:%S')] $*"; }

# ---------------------------------------------------------------------------
# Configure MinIO client alias
# ---------------------------------------------------------------------------
configure_mc() {
  log "Configuring mc alias '${MC_ALIAS}' → ${MINIO_ENDPOINT}"
  mc alias set "${MC_ALIAS}" "${MINIO_ENDPOINT}" "${MINIO_ACCESS_KEY}" "${MINIO_SECRET_KEY}" --quiet
  log "mc alias configured."
}

# ---------------------------------------------------------------------------
# Ensure bucket exists (idempotent)
# ---------------------------------------------------------------------------
ensure_bucket() {
  local bucket="$1"
  if mc ls "${MC_ALIAS}/${bucket}" >/dev/null 2>&1; then
    log "Bucket exists: ${bucket}"
  else
    log "Creating bucket: ${bucket}"
    mc mb "${MC_ALIAS}/${bucket}"
  fi
}

# ---------------------------------------------------------------------------
# Apply lifecycle rule using mc ilm add
# ---------------------------------------------------------------------------
apply_lifecycle() {
  local bucket="$1"
  local expiry_days="$2"

  log "Applying lifecycle to ${bucket}: expire after ${expiry_days} days..."

  # Remove existing lifecycle rules before re-applying (idempotent)
  mc ilm ls "${MC_ALIAS}/${bucket}" 2>/dev/null \
    | awk 'NR>1 {print $1}' \
    | while read -r rule_id; do
        [[ -n "${rule_id}" ]] && mc ilm rm --id "${rule_id}" "${MC_ALIAS}/${bucket}" 2>/dev/null || true
      done

  # Add new expiration rule
  mc ilm add \
    --expiry-days "${expiry_days}" \
    "${MC_ALIAS}/${bucket}"

  log "Lifecycle applied to ${bucket}."
}

# ---------------------------------------------------------------------------
# Verify lifecycle rules
# ---------------------------------------------------------------------------
verify_lifecycle() {
  local bucket="$1"
  log "Lifecycle rules for ${bucket}:"
  mc ilm ls "${MC_ALIAS}/${bucket}" 2>/dev/null || log "  (no rules or ilm not supported)"
}

# ---------------------------------------------------------------------------
# Apply versioning (recommended for backup buckets)
# ---------------------------------------------------------------------------
enable_versioning() {
  local bucket="$1"
  log "Enabling versioning on ${bucket}..."
  mc version enable "${MC_ALIAS}/${bucket}" 2>/dev/null || log "  Versioning not available (may require MinIO enterprise)."
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
main() {
  log "===== MinIO S3 Lifecycle Configuration ====="

  configure_mc

  # Daily bucket — 14-day retention
  ensure_bucket "crm-backups-daily"
  apply_lifecycle "crm-backups-daily" "${DAILY_EXPIRY_DAYS}"
  enable_versioning "crm-backups-daily"
  verify_lifecycle "crm-backups-daily"

  # Weekly bucket — 56-day retention (8 weeks)
  ensure_bucket "crm-backups-weekly"
  apply_lifecycle "crm-backups-weekly" "${WEEKLY_EXPIRY_DAYS}"
  enable_versioning "crm-backups-weekly"
  verify_lifecycle "crm-backups-weekly"

  # Monthly bucket — 365-day retention (1 year)
  ensure_bucket "crm-backups-monthly"
  apply_lifecycle "crm-backups-monthly" "${MONTHLY_EXPIRY_DAYS}"
  enable_versioning "crm-backups-monthly"
  verify_lifecycle "crm-backups-monthly"

  log "===== Lifecycle Configuration Complete ====="
  log ""
  log "Summary:"
  log "  crm-backups-daily   → expires after ${DAILY_EXPIRY_DAYS} days"
  log "  crm-backups-weekly  → expires after ${WEEKLY_EXPIRY_DAYS} days"
  log "  crm-backups-monthly → expires after ${MONTHLY_EXPIRY_DAYS} days"
}

main "$@"
