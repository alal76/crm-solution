#!/usr/bin/env bash
# ============================================================
# package-cdt.sh — Build & package the CRM Deployment Tool
# as a self-contained Docker image tarball + launcher bundle.
# ============================================================
# Usage:
#   ./package-cdt.sh              — build image + create dist/ bundle
#   ./package-cdt.sh --push       — also push image to registry
#   ./package-cdt.sh --platform linux/amd64,linux/arm64
# ============================================================

set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DIST_DIR="$SCRIPT_DIR/dist"
IMAGE_NAME="${CDT_IMAGE:-crm-cdt}"
VERSION="${CDT_VERSION:-$(cat "$SCRIPT_DIR/version.json" 2>/dev/null | python3 -c "import sys,json; print(json.load(sys.stdin).get('version','0.0.0'))" 2>/dev/null || echo '0.609.1')}"
TAG="${IMAGE_NAME}:${VERSION}"
TAG_LATEST="${IMAGE_NAME}:latest"
PLATFORM="${PLATFORM:-linux/amd64}"
PUSH=false

# ---- colours ----
CYAN='\033[0;36m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'
RED='\033[0;31m'; RESET='\033[0m'; BOLD='\033[1m'

log()  { echo -e "${CYAN}[CDT-PKG]${RESET} $*"; }
ok()   { echo -e "${GREEN}[CDT-PKG]${RESET} $*"; }
warn() { echo -e "${YELLOW}[CDT-PKG]${RESET} $*"; }
err()  { echo -e "${RED}[CDT-PKG]${RESET} $*"; exit 1; }

# ---- parse args ----
while [[ $# -gt 0 ]]; do
  case "$1" in
    --push)       PUSH=true ;;
    --platform)   shift; PLATFORM="$1" ;;
    --tag)        shift; IMAGE_NAME="$1"; TAG="${IMAGE_NAME}:${VERSION}"; TAG_LATEST="${IMAGE_NAME}:latest" ;;
    --version)    shift; VERSION="$1"; TAG="${IMAGE_NAME}:${VERSION}" ;;
    *) warn "Unknown argument: $1" ;;
  esac
  shift
done

banner() {
  echo -e "${BOLD}"
  echo "  ╔═══════════════════════════════════════════════╗"
  echo "  ║        CRM Deployment Tool Packager           ║"
  echo "  ║                v${VERSION}                    ║"
  echo "  ╚═══════════════════════════════════════════════╝"
  echo -e "${RESET}"
}

check_docker() {
  command -v docker &>/dev/null || err "Docker is required to build the image. Run: ./start-cdt.sh first."
  log "Docker: $(docker --version)"
}

build_image() {
  log "Building image ${TAG} for platform(s): ${PLATFORM}"

  local BUILD_ARGS=()
  if [[ "$PLATFORM" == *","* ]] || [[ "$PLATFORM" == "linux/amd64,linux/arm64" ]]; then
    # Multi-platform build requires buildx
    docker buildx inspect crm-cdt-builder &>/dev/null || \
      docker buildx create --name crm-cdt-builder --use
    docker buildx use crm-cdt-builder
    BUILD_ARGS=(buildx build --platform "$PLATFORM")
    [[ "$PUSH" == "true" ]] && BUILD_ARGS+=(--push) || BUILD_ARGS+=(--load)
  else
    BUILD_ARGS=(build --platform "$PLATFORM")
  fi

  docker "${BUILD_ARGS[@]}" \
    -f "$SCRIPT_DIR/Dockerfile.tool" \
    -t "$TAG" \
    -t "$TAG_LATEST" \
    "$SCRIPT_DIR"

  ok "Image built: $TAG"
}

export_tarball() {
  if [[ "$PUSH" == "true" ]]; then
    log "Skipping tarball export (--push mode)"
    return
  fi

  mkdir -p "$DIST_DIR"
  local TARBALL="$DIST_DIR/crm-cdt-${VERSION}.tar.gz"
  log "Exporting image to tarball: $TARBALL"
  docker save "$TAG" | gzip -9 > "$TARBALL"
  ok "Tarball: $TARBALL ($(du -sh "$TARBALL" | cut -f1))"
}

copy_launcher() {
  if [[ "$PUSH" == "true" ]]; then
    return   # registry users don't need local launchers
  fi
  mkdir -p "$DIST_DIR"

  # Self-contained docker-compose file
  cp "$SCRIPT_DIR/docker-compose.tool.yml" "$DIST_DIR/docker-compose.yml"

  # Lightweight shell launcher
  cat > "$DIST_DIR/run-cdt.sh" <<'LAUNCHER'
#!/usr/bin/env bash
# CRM Deployment Tool — standalone launcher
# Loads the image from tarball (first run) then opens the wizard.
set -euo pipefail
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PORT="${CDT_PORT:-5050}"
TARBALL=$(ls "$DIR"/crm-cdt-*.tar.gz 2>/dev/null | head -1 || true)

command -v docker &>/dev/null || { echo "Docker is required. Install from https://docs.docker.com/get-docker/"; exit 1; }

if [[ -n "$TARBALL" ]]; then
  echo "[CDT] Loading image from $TARBALL..."
  docker load < "$TARBALL"
fi

echo "[CDT] Starting wizard at http://localhost:${PORT}"
docker-compose -f "$DIR/docker-compose.yml" up -d

# Try to open browser
sleep 3
URL="http://localhost:${PORT}"
if [[ "$(uname -s)" == "Darwin" ]]; then open "$URL"
elif command -v xdg-open &>/dev/null; then xdg-open "$URL" &
else echo "[CDT] Open your browser at: $URL"; fi

echo "[CDT] Logs: docker-compose -f $DIR/docker-compose.yml logs -f"
echo "[CDT] Stop: docker-compose -f $DIR/docker-compose.yml down"
LAUNCHER
  chmod +x "$DIST_DIR/run-cdt.sh"

  # Windows launcher (.bat)
  cat > "$DIST_DIR/run-cdt.bat" <<'WINLAUNCHER'
@echo off
setlocal
set PORT=%CDT_PORT%
if "%PORT%"=="" set PORT=5050
set "DIR=%~dp0"

where docker >nul 2>&1 || (echo Docker is required. Install from https://docs.docker.com/get-docker/ && pause && exit /b 1)

for %%F in ("%DIR%crm-cdt-*.tar.gz") do (
  echo Loading image from %%F...
  docker load -i "%%F"
)

echo Starting CDT wizard at http://localhost:%PORT%
docker-compose -f "%DIR%docker-compose.yml" up -d
timeout /t 4 /nobreak >nul
start http://localhost:%PORT%
WINLAUNCHER

  ok "Launcher scripts written to $DIST_DIR"
}

push_image() {
  if [[ "$PUSH" == "true" ]]; then
    log "Pushing ${TAG} to registry…"
    docker push "$TAG"
    docker push "$TAG_LATEST"
    ok "Pushed: $TAG and $TAG_LATEST"
  fi
}

print_summary() {
  echo ""
  ok "Package complete!"
  echo ""
  echo -e "  ${BOLD}Distribution:${RESET} $DIST_DIR"
  if [[ "$PUSH" != "true" ]]; then
    echo -e "  Tarball:  dist/crm-cdt-${VERSION}.tar.gz"
    echo -e "  Launcher: dist/run-cdt.sh  (Linux/macOS)"
    echo -e "  Launcher: dist/run-cdt.bat (Windows)"
    echo ""
    echo -e "  ${CYAN}To run on any machine with Docker:${RESET}"
    echo    "    cd dist && ./run-cdt.sh"
    echo ""
    echo -e "  ${CYAN}To load image manually:${RESET}"
    echo    "    docker load < dist/crm-cdt-${VERSION}.tar.gz"
    echo    "    docker run -p 5050:5050 -v /var/run/docker.sock:/var/run/docker.sock crm-cdt:${VERSION}"
  else
    echo -e "  ${CYAN}Deployed to registry as:${RESET} ${TAG}"
  fi
  echo ""
}

# ---- main ----
banner
check_docker
build_image
export_tarball
copy_launcher
push_image
print_summary
