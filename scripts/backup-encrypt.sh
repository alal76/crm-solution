#!/bin/bash
# =============================================================================
# CRM Solution — Backup Encryption Script
# TODO-DB-006: AES-256-CBC encryption + GPG signing for backup archives
#
# Usage: ./backup-encrypt.sh <input_file>
#   Returns the path to the encrypted output file on stdout.
#
# Environment:
#   BACKUP_ENCRYPTION_KEY  – 32+ character passphrase (REQUIRED)
#   GPG_SIGN_KEY_ID        – GPG key ID for signing (optional; skipped if unset)
#
# Output: <input_file>.enc  (encrypted)
#         <input_file>.enc.sig  (detached GPG signature, if GPG_SIGN_KEY_ID set)
#
# chmod +x scripts/backup-encrypt.sh
# =============================================================================
set -euo pipefail

INPUT_FILE="${1:-}"
if [[ -z "${INPUT_FILE}" || ! -f "${INPUT_FILE}" ]]; then
  echo "ERROR: Usage: $0 <input_file>" >&2
  exit 1
fi

ENCRYPTION_KEY="${BACKUP_ENCRYPTION_KEY:-}"
if [[ -z "${ENCRYPTION_KEY}" ]]; then
  echo "ERROR: BACKUP_ENCRYPTION_KEY environment variable is not set." >&2
  exit 1
fi

if [[ ${#ENCRYPTION_KEY} -lt 32 ]]; then
  echo "ERROR: BACKUP_ENCRYPTION_KEY must be at least 32 characters." >&2
  exit 1
fi

GPG_SIGN_KEY_ID="${GPG_SIGN_KEY_ID:-}"
OUTPUT_FILE="${INPUT_FILE}.enc"

log() { echo "[$(date '+%Y-%m-%d %H:%M:%S')] $*" >&2; }

# ---------------------------------------------------------------------------
# 1. AES-256-CBC encryption with PBKDF2 key derivation
#    -pbkdf2 and -iter 100000 guard against brute-force attacks
# ---------------------------------------------------------------------------
encrypt_file() {
  log "Encrypting ${INPUT_FILE} → ${OUTPUT_FILE}..."
  openssl enc \
    -aes-256-cbc \
    -pbkdf2 \
    -iter 100000 \
    -salt \
    -pass "pass:${ENCRYPTION_KEY}" \
    -in "${INPUT_FILE}" \
    -out "${OUTPUT_FILE}"
  log "Encryption complete. Output: ${OUTPUT_FILE} ($(du -sh "${OUTPUT_FILE}" | cut -f1))"
}

# ---------------------------------------------------------------------------
# 2. GPG detached signature (optional)
#    Verifies both authenticity and integrity independent of encryption
# ---------------------------------------------------------------------------
sign_file() {
  if [[ -n "${GPG_SIGN_KEY_ID}" ]]; then
    log "Creating GPG detached signature (key: ${GPG_SIGN_KEY_ID})..."
    gpg \
      --batch \
      --yes \
      --local-user "${GPG_SIGN_KEY_ID}" \
      --detach-sign \
      --armor \
      --output "${OUTPUT_FILE}.sig" \
      "${OUTPUT_FILE}"
    log "GPG signature written to ${OUTPUT_FILE}.sig"
  else
    log "GPG_SIGN_KEY_ID not set — skipping GPG signature."
  fi
}

# ---------------------------------------------------------------------------
# 3. Generate SHA-256 checksum of the encrypted file
# ---------------------------------------------------------------------------
generate_checksum() {
  sha256sum "${OUTPUT_FILE}" > "${OUTPUT_FILE}.sha256"
  log "Checksum written to ${OUTPUT_FILE}.sha256"
}

# ---------------------------------------------------------------------------
# Decryption reference (printed to stderr for documentation purposes)
# ---------------------------------------------------------------------------
print_decrypt_instructions() {
  log ""
  log "To decrypt this backup:"
  log "  openssl enc -d -aes-256-cbc -pbkdf2 -iter 100000 \\"
  log "    -pass \"pass:\${BACKUP_ENCRYPTION_KEY}\" \\"
  log "    -in ${OUTPUT_FILE} \\"
  log "    -out ${INPUT_FILE}.restored"
  if [[ -n "${GPG_SIGN_KEY_ID}" ]]; then
    log ""
    log "To verify GPG signature:"
    log "  gpg --verify ${OUTPUT_FILE}.sig ${OUTPUT_FILE}"
  fi
}

# ---------------------------------------------------------------------------
# Main — outputs the encrypted file path to stdout
# ---------------------------------------------------------------------------
main() {
  encrypt_file
  sign_file
  generate_checksum
  print_decrypt_instructions
  # Output the encrypted file path so callers can use it
  echo "${OUTPUT_FILE}"
}

main "$@"
