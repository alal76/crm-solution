#!/bin/bash
# =============================================================================
# CRM Solution — MinIO / S3 Cross-Region Replication Configuration
# TODO-DB-026: Configure cross-region backup replication
#
# This script configures MinIO bucket replication (or AWS S3 CRR) so that
# backup archives are automatically replicated to a secondary region/site.
#
# Modes:
#   1. MinIO → MinIO  (two self-hosted MinIO instances)
#   2. MinIO → AWS S3 (primary MinIO replicates to S3 bucket)
#   3. AWS S3 → AWS S3 (native S3 CRR via mc/awscli)
#
# Usage:
#   # MinIO-to-MinIO:
#   REPLICATION_MODE=minio ./configure-s3-cross-region.sh
#
#   # MinIO-to-S3:
#   REPLICATION_MODE=s3 \
#   AWS_REGION_SRC=us-east-1 AWS_REGION_DST=us-west-2 \
#   ./configure-s3-cross-region.sh
#
# chmod +x scripts/configure-s3-cross-region.sh
# =============================================================================
set -euo pipefail

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------
REPLICATION_MODE="${REPLICATION_MODE:-minio}"   # minio | s3

# Primary MinIO (source)
MINIO_SRC_ENDPOINT="${MINIO_SRC_ENDPOINT:-http://localhost:9000}"
MINIO_SRC_ACCESS_KEY="${MINIO_SRC_ACCESS_KEY:-minio_admin}"
MINIO_SRC_SECRET_KEY="${MINIO_SRC_SECRET_KEY:-MinioPass@Dev2024}"
MINIO_SRC_ALIAS="crm_src"

# Secondary MinIO (destination — DR site)
MINIO_DST_ENDPOINT="${MINIO_DST_ENDPOINT:-http://192.168.0.50:9000}"
MINIO_DST_ACCESS_KEY="${MINIO_DST_ACCESS_KEY:-minio_admin}"
MINIO_DST_SECRET_KEY="${MINIO_DST_SECRET_KEY:-MinioPass@Dev2024}"
MINIO_DST_ALIAS="crm_dst"

# AWS S3 mode
AWS_REGION_SRC="${AWS_REGION_SRC:-us-east-1}"
AWS_REGION_DST="${AWS_REGION_DST:-us-west-2}"
S3_DST_BUCKET_PREFIX="${S3_DST_BUCKET_PREFIX:-crm-dr-backups}"

# Buckets to replicate
BUCKETS=("crm-backups-daily" "crm-backups-weekly" "crm-backups-monthly")

log() { echo "[$(date '+%Y-%m-%d %H:%M:%S')] $*"; }

# ---------------------------------------------------------------------------
# MinIO → MinIO replication
# Uses mc replicate add with bandwidth controls
# ---------------------------------------------------------------------------
setup_minio_to_minio() {
  log "=== Configuring MinIO → MinIO cross-region replication ==="
  log "  Source: ${MINIO_SRC_ENDPOINT}"
  log "  Destination: ${MINIO_DST_ENDPOINT}"

  # Configure mc aliases
  mc alias set "${MINIO_SRC_ALIAS}" "${MINIO_SRC_ENDPOINT}" \
    "${MINIO_SRC_ACCESS_KEY}" "${MINIO_SRC_SECRET_KEY}" --quiet
  mc alias set "${MINIO_DST_ALIAS}" "${MINIO_DST_ENDPOINT}" \
    "${MINIO_DST_ACCESS_KEY}" "${MINIO_DST_SECRET_KEY}" --quiet

  for bucket in "${BUCKETS[@]}"; do
    local dst_bucket="${bucket}-dr"
    log "  Configuring replication: ${bucket} → ${dst_bucket}"

    # Ensure destination bucket exists
    mc mb --ignore-existing "${MINIO_DST_ALIAS}/${dst_bucket}"

    # Enable versioning on both (required for replication)
    mc version enable "${MINIO_SRC_ALIAS}/${bucket}" 2>/dev/null || true
    mc version enable "${MINIO_DST_ALIAS}/${dst_bucket}" 2>/dev/null || true

    # Configure replication rule
    # Remove existing rule if any (idempotent)
    mc replicate rm --all --force "${MINIO_SRC_ALIAS}/${bucket}" 2>/dev/null || true

    # Add replication rule
    mc replicate add \
      "${MINIO_SRC_ALIAS}/${bucket}" \
      --remote-bucket "http://${MINIO_DST_ACCESS_KEY}:${MINIO_DST_SECRET_KEY}@${MINIO_DST_ENDPOINT##http://}/${dst_bucket}" \
      --replicate "delete,delete-marker,existing-objects" \
      --bandwidth "100MB" \
      --priority 1 \
      --tags "backup=true"

    log "  ✓ Replication configured: ${MINIO_SRC_ALIAS}/${bucket} → ${MINIO_DST_ALIAS}/${dst_bucket}"
  done

  # Verify
  log ""
  log "Replication status:"
  for bucket in "${BUCKETS[@]}"; do
    mc replicate ls "${MINIO_SRC_ALIAS}/${bucket}" 2>/dev/null || log "  (no replication rules found)"
  done
}

# ---------------------------------------------------------------------------
# MinIO → AWS S3 replication (using mc mirror or site replication)
# ---------------------------------------------------------------------------
setup_minio_to_s3() {
  log "=== Configuring MinIO → AWS S3 cross-region replication ==="

  # Configure source MinIO alias
  mc alias set "${MINIO_SRC_ALIAS}" "${MINIO_SRC_ENDPOINT}" \
    "${MINIO_SRC_ACCESS_KEY}" "${MINIO_SRC_SECRET_KEY}" --quiet

  # Configure AWS S3 alias using environment credentials
  mc alias set "crm_s3_dst" "https://s3.amazonaws.com" \
    "${AWS_ACCESS_KEY_ID:-}" "${AWS_SECRET_ACCESS_KEY:-}" --quiet

  for bucket in "${BUCKETS[@]}"; do
    local s3_bucket="${S3_DST_BUCKET_PREFIX}-${bucket#crm-backups-}"
    log "  Setting up mirror: ${bucket} → s3://${s3_bucket} (${AWS_REGION_DST})"

    # Create S3 bucket in destination region if it doesn't exist
    mc mb --ignore-existing --region "${AWS_REGION_DST}" "crm_s3_dst/${s3_bucket}" 2>/dev/null || {
      log "  (bucket ${s3_bucket} may already exist)"
    }

    # Use mc mirror to sync existing backups
    log "  Syncing existing objects to ${s3_bucket}..."
    mc mirror \
      --watch \
      --remove \
      "${MINIO_SRC_ALIAS}/${bucket}" \
      "crm_s3_dst/${s3_bucket}" &

    log "  ✓ Background mirror started for ${bucket} → s3://${s3_bucket}"
  done

  log ""
  log "NOTE: mc mirror processes started in background (PIDs active)."
  log "Consider using systemd or supervisord to persist these processes."
}

# ---------------------------------------------------------------------------
# Print replication status
# ---------------------------------------------------------------------------
show_status() {
  log ""
  log "===== Replication Status ====="
  if [[ "${REPLICATION_MODE}" == "minio" ]]; then
    for bucket in "${BUCKETS[@]}"; do
      log "  ${bucket}:"
      mc replicate status "${MINIO_SRC_ALIAS}/${bucket}" 2>/dev/null \
        | head -5 \
        | sed 's/^/    /' || log "    (not configured)"
    done
  fi
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
main() {
  log "===== S3 Cross-Region Replication Configuration Started ====="
  log "  Mode: ${REPLICATION_MODE}"

  case "${REPLICATION_MODE}" in
    minio) setup_minio_to_minio ;;
    s3)    setup_minio_to_s3 ;;
    *)
      log "ERROR: Unknown REPLICATION_MODE '${REPLICATION_MODE}'. Use 'minio' or 's3'."
      exit 1
      ;;
  esac

  show_status
  log "===== Cross-Region Replication Configuration Complete ====="
}

main "$@"
