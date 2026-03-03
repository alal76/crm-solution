#!/usr/bin/env bash
# ============================================================
# CRM Consolidated Deployment Tool (CDT) - Self-Bootstrap Launcher
# Version: 0.609.1
# Usage: ./start-cdt.sh [--port PORT] [--no-browser] [--headless] [--reset-venv]
# ============================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CDT_VERSION="0.611.0"
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
  log_step 2 7 "Detecting Python 3.10+"
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
    log_warn "Python 3.10+ not found — attempting auto-install…"
    local OS_TYPE
    OS_TYPE=$(uname -s)

    if [[ "$OS_TYPE" == "Darwin" ]]; then
      # macOS — ensure Homebrew then install python
      if ! command -v brew &>/dev/null; then
        log_info "Installing Homebrew…"
        /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)" || true
        # Add brew to path for Apple Silicon
        [[ -f /opt/homebrew/bin/brew ]] && eval "$(/opt/homebrew/bin/brew shellenv)"
        [[ -f /usr/local/bin/brew ]]    && eval "$(/usr/local/bin/brew shellenv)"
      fi
      if command -v brew &>/dev/null; then
        log_info "Installing Python 3.12 via Homebrew…"
        brew install python@3.12 || true
        PYTHON=$(brew --prefix python@3.12)/bin/python3.12
        [[ -x "$PYTHON" ]] || PYTHON=$(command -v python3 || true)
      fi

    elif [[ "$OS_TYPE" == "Linux" ]]; then
      if command -v apt-get &>/dev/null; then
        log_info "Installing python3.11 via apt…"
        sudo apt-get update -qq && sudo apt-get install -y python3.11 python3.11-venv python3-pip || true
        PYTHON=$(command -v python3.11 || command -v python3 || true)
      elif command -v dnf &>/dev/null; then
        log_info "Installing python3.11 via dnf…"
        sudo dnf install -y python3.11 python3.11-venv || true
        PYTHON=$(command -v python3.11 || command -v python3 || true)
      elif command -v yum &>/dev/null; then
        log_info "Installing python3 via yum…"
        sudo yum install -y python3 python3-pip || true
        PYTHON=$(command -v python3 || true)
      fi
    fi

    if [[ -z "$PYTHON" || ! -x "$PYTHON" ]]; then
      log_error "Could not auto-install Python 3.10+."
      log_error "Please install from: https://www.python.org/downloads/"
      exit 1
    fi
    log_info "Auto-installed Python at: $PYTHON"
  fi
  echo "$PYTHON"
}

setup_venv() {
  local python_cmd="$1"
  log_step 3 7 "Setting up Python virtual environment"
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

bootstrap_system_tools() {
  log_step 4 7 "Checking system tools (Docker, Git, curl)"
  local OS_TYPE
  OS_TYPE=$(uname -s)

  _ensure_brew() {
    if [[ "$OS_TYPE" == "Darwin" ]] && ! command -v brew &>/dev/null; then
      log_info "Installing Homebrew…"
      /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)" || true
      [[ -f /opt/homebrew/bin/brew ]] && eval "$(/opt/homebrew/bin/brew shellenv)"
      [[ -f /usr/local/bin/brew    ]] && eval "$(/usr/local/bin/brew shellenv)"
    fi
  }

  _install_mac() {
    local pkg="$1"
    _ensure_brew
    if command -v brew &>/dev/null; then
      log_info "Installing $pkg via Homebrew…"
      brew install "$pkg" 2>/dev/null || log_warn "brew install $pkg failed (may need manual install)"
    else
      log_warn "Homebrew unavailable — cannot install $pkg automatically"
    fi
  }

  _install_linux() {
    local pkg="$1"
    if command -v apt-get &>/dev/null; then
      sudo apt-get install -y "$pkg" -qq || log_warn "apt install $pkg failed"
    elif command -v dnf &>/dev/null; then
      sudo dnf install -y "$pkg" || log_warn "dnf install $pkg failed"
    elif command -v yum &>/dev/null; then
      sudo yum install -y "$pkg" || log_warn "yum install $pkg failed"
    else
      log_warn "No package manager found — cannot install $pkg"
    fi
  }

  # Docker
  if ! command -v docker &>/dev/null; then
    log_warn "Docker not found — installing…"
    if [[ "$OS_TYPE" == "Darwin" ]]; then
      _install_mac docker
      [[ ! -x /Applications/Docker.app/Contents/MacOS/com.docker.backend ]] && \
        log_warn "Docker Desktop may need to be started manually: open /Applications/Docker.app"
    else
      log_info "Installing Docker via official script…"
      curl -fsSL https://get.docker.com | sudo sh 2>/dev/null || _install_linux docker.io
      sudo systemctl enable docker 2>/dev/null || true
      sudo systemctl start  docker 2>/dev/null || true
      # Add current user to docker group
      sudo usermod -aG docker "$(whoami)" 2>/dev/null || true
      log_warn "You may need to log out/in for docker group to take effect"
    fi
  else
    log_info "Docker: $(docker --version 2>/dev/null | head -1)"
  fi

  # Docker Compose (v2 plugin)
  if ! docker compose version &>/dev/null && ! command -v docker-compose &>/dev/null; then
    log_warn "Docker Compose not found — installing…"
    if [[ "$OS_TYPE" == "Darwin" ]]; then
      _install_mac docker-compose
    else
      # Install compose plugin
      DOCKER_CONFIG=${DOCKER_CONFIG:-$HOME/.docker}
      mkdir -p "$DOCKER_CONFIG/cli-plugins"
      local COMPOSE_VERSION="v2.27.0"
      local COMPOSE_ARCH
      COMPOSE_ARCH=$(uname -m); [[ "$COMPOSE_ARCH" == "x86_64" ]] && COMPOSE_ARCH="x86_64"
      curl -SL "https://github.com/docker/compose/releases/download/${COMPOSE_VERSION}/docker-compose-linux-${COMPOSE_ARCH}" \
        -o "$DOCKER_CONFIG/cli-plugins/docker-compose" 2>/dev/null || true
      chmod +x "$DOCKER_CONFIG/cli-plugins/docker-compose" 2>/dev/null || true
    fi
  else
    COMPOSE_VER=$(docker compose version --short 2>/dev/null || docker-compose --version 2>/dev/null | head -1)
    log_info "Docker Compose: $COMPOSE_VER"
  fi

  # Git
  if ! command -v git &>/dev/null; then
    log_warn "git not found — installing…"
    [[ "$OS_TYPE" == "Darwin" ]] && _install_mac git || _install_linux git
  else
    log_info "git: $(git --version)"
  fi

  # curl
  if ! command -v curl &>/dev/null; then
    [[ "$OS_TYPE" == "Darwin" ]] && _install_mac curl || _install_linux curl
  fi
}

download_cli_tools() {
  log_step 5 7 "Checking CLI tools (kubectl, helm)"
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
  log_step 6 7 "Opening browser"
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

kill_existing_cdt() {
  log_step 1 7 "Checking for existing CDT instances"
  local found=false

  # 1. Kill any process listening on the target port
  local port_pids
  port_pids=$(lsof -ti :"$PORT" 2>/dev/null || true)
  if [[ -n "$port_pids" ]]; then
    found=true
    log_warn "Found existing process(es) on port $PORT: $port_pids"
    for pid in $port_pids; do
      log_info "Sending SIGTERM to PID $pid..."
      kill "$pid" 2>/dev/null || true
    done
    sleep 2
    # Force-kill any survivors
    local survivors
    survivors=$(lsof -ti :"$PORT" 2>/dev/null || true)
    if [[ -n "$survivors" ]]; then
      log_warn "Force-killing stubborn process(es): $survivors"
      for pid in $survivors; do
        kill -9 "$pid" 2>/dev/null || true
      done
      sleep 1
    fi
  fi

  # 2. Kill any orphaned gui/app.py processes not on the port
  local app_pids
  app_pids=$(pgrep -f 'gui/app\.py' 2>/dev/null || true)
  if [[ -n "$app_pids" ]]; then
    for pid in $app_pids; do
      # Skip if already killed above
      if kill -0 "$pid" 2>/dev/null; then
        found=true
        log_warn "Killing orphaned CDT process PID $pid"
        kill "$pid" 2>/dev/null || true
        sleep 1
        kill -0 "$pid" 2>/dev/null && kill -9 "$pid" 2>/dev/null || true
      fi
    done
  fi

  if [[ "$found" == "true" ]]; then
    log_info "Old CDT instance(s) terminated"
  else
    log_info "No existing CDT instance found — clean start"
  fi
}

cleanup() {
  echo -e "\n${YELLOW}[CDT]${RESET} Stopping CRM Deployment Tool..."
}
trap cleanup SIGINT SIGTERM

main() {
  log_banner
  kill_existing_cdt
  local PYTHON
  PYTHON=$(check_python)
  bootstrap_system_tools
  setup_venv "$PYTHON"
  download_cli_tools
  log_step 6 7 "Opening browser"
  open_browser &
  log_step 7 7 "Starting CDT server on port $PORT"
  log_info "CDT wizard available at: http://localhost:${PORT}"
  log_info "Day-2 operations at:     http://localhost:${PORT}/day2"
  log_info "Press Ctrl+C to stop"
  exec "$VENV_DIR/bin/python" "$SCRIPT_DIR/gui/app.py" --port "$PORT" --no-debug
}

main "$@"
