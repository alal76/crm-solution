#!/usr/bin/env bash
# ============================================================
# CRM Consolidated Deployment Tool (CDT) - Self-Bootstrap Launcher
# Version: 0.609.1
# Usage: ./start-cdt.sh [--port PORT] [--no-browser] [--headless] [--reset-venv]
# ============================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CDT_VERSION="0.609.1"
DEFAULT_PORT=5050
PORT=$DEFAULT_PORT
NO_BROWSER=false
HEADLESS=false
RESET_VENV=false
VENV_DIR="$SCRIPT_DIR/.venv"
CDT_BIN_DIR="$HOME/.crm-cdt/bin"
CDT_SNAP_DIR="$HOME/.crm-cdt/snapshots"

# Colors
RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[0;33m'; CYAN='\033[0;36m'; BOLD='\033[1m'; RESET='\033[0m'

log_info()  { echo -e "${GREEN}[INFO]${RESET} $1"; }
log_warn()  { echo -e "${YELLOW}[WARN]${RESET} $1"; }
log_error() { echo -e "${RED}[ERROR]${RESET} $1" >&2; }
log_step()  { echo -e "${CYAN}[STEP $1/$2]${RESET} $3"; }
log_banner() {
  echo -e "${BOLD}${CYAN}"
  echo "╔══════════════════════════════════════════════════╗"
  echo "║       CRM Consolidated Deployment Tool           ║"
  echo "║              CDT v${CDT_VERSION}                        ║"
  echo "╚══════════════════════════════════════════════════╝"
  echo -e "${RESET}"
}

# Parse arguments
while [[ $# -gt 0 ]]; do
  case $1 in
    --port) PORT="$2"; shift 2 ;;
    --no-browser) NO_BROWSER=true; shift ;;
    --headless) HEADLESS=true; NO_BROWSER=true; shift ;;
    --reset-venv) RESET_VENV=true; shift ;;
    -h|--help) echo "Usage: $0 [--port PORT] [--no-browser] [--headless] [--reset-venv]"; exit 0 ;;
    *) log_warn "Unknown argument: $1"; shift ;;
  esac
done

check_python() {
  log_step 1 5 "Detecting Python 3.10+"
  local PYTHON=""
  for candidate in python3.12 python3.11 python3.10 python3 python; do
    if command -v "$candidate" &>/dev/null; then
      local version
      version=$("$candidate" -c "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')" 2>/dev/null || echo "0.0")
      local major minor
      major=$(echo "$version" | cut -d. -f1)
      minor=$(echo "$version" | cut -d. -f2)
      if [[ "$major" -ge 3 && "$minor" -ge 10 ]]; then
        PYTHON="$candidate"
        log_info "Found Python $version at $(command -v $candidate)"
        break
      fi
    fi
  done
  if [[ -z "$PYTHON" ]]; then
    log_error "Python 3.10 or higher is required. Install from https://www.python.org/downloads/"
    exit 1
  fi
  echo "$PYTHON"
}

setup_venv() {
  local python_cmd="$1"
  log_step 2 5 "Setting up Python virtual environment"
  if [[ "$RESET_VENV" == "true" && -d "$VENV_DIR" ]]; then
    log_warn "Removing existing venv (--reset-venv)"
    rm -rf "$VENV_DIR"
  fi
  if [[ ! -f "$VENV_DIR/bin/python" ]]; then
    log_info "Creating virtual environment at $VENV_DIR"
    "$python_cmd" -m venv "$VENV_DIR"
  else
    log_info "Reusing existing virtual environment"
  fi
  log_info "Installing/updating dependencies..."
  "$VENV_DIR/bin/pip" install -q --upgrade pip
  "$VENV_DIR/bin/pip" install -q -r "$SCRIPT_DIR/requirements.txt"
  log_info "Dependencies installed"
}

download_cli_tools() {
  log_step 3 5 "Checking CLI tools (kubectl, helm)"
  mkdir -p "$CDT_BIN_DIR"
  mkdir -p "$CDT_SNAP_DIR"

  local OS ARCH
  OS=$(uname -s | tr '[:upper:]' '[:lower:]')
  ARCH=$(uname -m)
  [[ "$ARCH" == "x86_64" ]] && ARCH="amd64"
  [[ "$ARCH" == "aarch64" || "$ARCH" == "arm64" ]] && ARCH="arm64"

  local KUBECTL_VERSION="v1.31.4"
  local KUBECTL_PATH="$CDT_BIN_DIR/kubectl"
  if [[ ! -f "$KUBECTL_PATH" ]]; then
    local KUBECTL_URL="https://dl.k8s.io/release/${KUBECTL_VERSION}/bin/${OS}/${ARCH}/kubectl"
    log_info "Downloading kubectl $KUBECTL_VERSION..."
    if curl -sL --fail -o "$KUBECTL_PATH" "$KUBECTL_URL" 2>/dev/null; then
      chmod +x "$KUBECTL_PATH"
      log_info "kubectl downloaded"
    else
      log_warn "kubectl download failed — Kubernetes features may not work"
    fi
  else
    log_info "kubectl already present"
  fi

  if [[ ! -f "$CDT_BIN_DIR/helm" ]]; then
    local HELM_VERSION="v3.17.0"
    local HELM_URL="https://get.helm.sh/helm-${HELM_VERSION}-${OS}-${ARCH}.tar.gz"
    log_info "Downloading helm $HELM_VERSION..."
    local HELM_TMP
    HELM_TMP=$(mktemp -d)
    if curl -sL --fail -o "$HELM_TMP/helm.tar.gz" "$HELM_URL" 2>/dev/null; then
      tar -xzf "$HELM_TMP/helm.tar.gz" -C "$HELM_TMP"
      cp "$HELM_TMP/${OS}-${ARCH}/helm" "$CDT_BIN_DIR/helm"
      chmod +x "$CDT_BIN_DIR/helm"
      log_info "helm downloaded"
    else
      log_warn "helm download failed — Kubernetes Helm features may not work"
    fi
    rm -rf "$HELM_TMP"
  else
    log_info "helm already present"
  fi

  export PATH="$CDT_BIN_DIR:$PATH"
}

open_browser() {
  if [[ "$NO_BROWSER" == "true" ]]; then
    return
  fi
  log_step 4 5 "Opening browser"
  local URL="http://localhost:${PORT}"
  sleep 2  # Let Flask start
  if [[ "$(uname -s)" == "Darwin" ]]; then
    open "$URL" &
  elif command -v xdg-open &>/dev/null; then
    xdg-open "$URL" &
  elif command -v firefox &>/dev/null; then
    firefox "$URL" &
  elif command -v chromium-browser &>/dev/null; then
    chromium-browser "$URL" &
  else
    log_info "CDT is running at $URL — open in your browser"
  fi
}

cleanup() {
  echo -e "\n${YELLOW}[CDT]${RESET} Stopping CRM Deployment Tool..."
}
trap cleanup SIGINT SIGTERM

main() {
  log_banner
  local PYTHON
  PYTHON=$(check_python)
  setup_venv "$PYTHON"
  download_cli_tools
  log_step 4 5 "Opening browser"
  open_browser &
  log_step 5 5 "Starting CDT server on port $PORT"
  log_info "CDT wizard available at: http://localhost:${PORT}"
  log_info "Day-2 operations at:     http://localhost:${PORT}/day2"
  log_info "Press Ctrl+C to stop"
  exec "$VENV_DIR/bin/python" "$SCRIPT_DIR/gui/app.py" --port "$PORT"
}

main "$@"
