#!/usr/bin/env python3
"""
CRM Deployment Tool — System Tool Installer
============================================
Detects OS, checks for required system tools (Docker, kubectl, Helm,
cloud CLIs), and installs them automatically with live streaming output.

Supports: macOS (Homebrew), Linux (apt / yum / dnf), Windows (winget).
"""

from __future__ import annotations

import importlib
import os
import platform
import shutil
import subprocess
import sys
import threading
from dataclasses import dataclass, field
from typing import Callable, Dict, Generator, List, Optional

# ---------------------------------------------------------------------------
# Tool definitions
# ---------------------------------------------------------------------------

@dataclass
class ToolInfo:
    """Describes a system tool and how to install it per OS."""
    key: str                        # Internal identifier
    name: str                       # Display name
    description: str                # What it's used for
    binary: str                     # Binary name checked via shutil.which
    category: str                   # "required" | "recommended" | "optional"
    install_mac: List[str]          # brew install commands
    install_linux_apt: List[str]    # apt-get commands
    install_linux_yum: List[str]    # yum / dnf commands
    install_windows: List[str]      # winget commands
    version_flag: str = "--version"
    version_parse: Optional[str] = None  # regex pattern for version extraction


TOOLS: List[ToolInfo] = [
    # --- Container runtime --------------------------------------------------
    ToolInfo(
        key="docker",
        name="Docker",
        description="Container runtime — builds and runs CRM containers",
        binary="docker",
        category="required",
        install_mac=["brew install --cask docker"],
        install_linux_apt=[
            "curl -fsSL https://get.docker.com | sh",
            "sudo usermod -aG docker $USER",
        ],
        install_linux_yum=[
            "sudo yum install -y yum-utils",
            "sudo yum-config-manager --add-repo https://download.docker.com/linux/centos/docker-ce.repo",
            "sudo yum install -y docker-ce docker-ce-cli containerd.io",
            "sudo systemctl start docker && sudo systemctl enable docker",
            "sudo usermod -aG docker $USER",
        ],
        install_windows=["winget install Docker.DockerDesktop"],
    ),
    ToolInfo(
        key="docker_compose",
        name="Docker Compose",
        description="Multi-container orchestration used for monolithic deployments",
        binary="docker",          # docker compose v2 is a docker plugin
        category="required",
        install_mac=["brew install docker-compose"],
        install_linux_apt=["sudo apt-get install -y docker-compose-plugin"],
        install_linux_yum=["sudo yum install -y docker-compose-plugin"],
        install_windows=["winget install Docker.DockerDesktop"],
        version_flag="compose version",
    ),
    # --- Kubernetes ---------------------------------------------------------
    ToolInfo(
        key="kubectl",
        name="kubectl",
        description="Kubernetes CLI — required for K8s deployments",
        binary="kubectl",
        category="recommended",
        install_mac=["brew install kubectl"],
        install_linux_apt=[
            "curl -LO https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl",
            "sudo install -o root -g root -m 0755 kubectl /usr/local/bin/kubectl",
        ],
        install_linux_yum=[
            "cat <<EOF | sudo tee /etc/yum.repos.d/kubernetes.repo\n[kubernetes]\nbaseurl=https://pkgs.k8s.io/core:/stable:/v1.29/rpm/\nenabled=1\ngpgcheck=1\nEOF",
            "sudo yum install -y kubectl",
        ],
        install_windows=["winget install Kubernetes.kubectl"],
    ),
    ToolInfo(
        key="helm",
        name="Helm",
        description="Kubernetes package manager — used for Helm chart deployments",
        binary="helm",
        category="recommended",
        install_mac=["brew install helm"],
        install_linux_apt=["curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash"],
        install_linux_yum=["curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash"],
        install_windows=["winget install Helm.Helm"],
    ),
    # --- Cloud CLIs ---------------------------------------------------------
    ToolInfo(
        key="az",
        name="Azure CLI",
        description="Azure cloud management — required for Azure deployments",
        binary="az",
        category="optional",
        install_mac=["brew install azure-cli"],
        install_linux_apt=[
            "curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash",
        ],
        install_linux_yum=[
            "sudo rpm --import https://packages.microsoft.com/keys/microsoft.asc",
            "sudo dnf install -y azure-cli",
        ],
        install_windows=["winget install Microsoft.AzureCLI"],
    ),
    ToolInfo(
        key="aws",
        name="AWS CLI",
        description="Amazon Web Services — required for AWS deployments",
        binary="aws",
        category="optional",
        install_mac=["brew install awscli"],
        install_linux_apt=[
            "curl 'https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip' -o /tmp/awscliv2.zip",
            "unzip /tmp/awscliv2.zip -d /tmp/",
            "sudo /tmp/aws/install",
        ],
        install_linux_yum=[
            "curl 'https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip' -o /tmp/awscliv2.zip",
            "unzip /tmp/awscliv2.zip -d /tmp/",
            "sudo /tmp/aws/install",
        ],
        install_windows=["winget install Amazon.AWSCLI"],
    ),
    ToolInfo(
        key="gcloud",
        name="Google Cloud CLI",
        description="Google Cloud Platform — required for GCP deployments",
        binary="gcloud",
        category="optional",
        install_mac=["brew install --cask google-cloud-sdk"],
        install_linux_apt=[
            "curl https://sdk.cloud.google.com | bash",
            "exec -l $SHELL",
            "gcloud init",
        ],
        install_linux_yum=[
            "curl https://sdk.cloud.google.com | bash",
            "exec -l $SHELL",
        ],
        install_windows=["winget install Google.CloudSDK"],
    ),
    # --- Build tools --------------------------------------------------------
    ToolInfo(
        key="git",
        name="Git",
        description="Source control — needed to clone/update the CRM repository",
        binary="git",
        category="required",
        install_mac=["brew install git"],
        install_linux_apt=["sudo apt-get install -y git"],
        install_linux_yum=["sudo yum install -y git"],
        install_windows=["winget install Git.Git"],
    ),
    ToolInfo(
        key="ssh",
        name="SSH Client",
        description="Required for on-premises server discovery and deployment",
        binary="ssh",
        category="required",
        install_mac=[],   # Built-in on macOS
        install_linux_apt=["sudo apt-get install -y openssh-client"],
        install_linux_yum=["sudo yum install -y openssh-clients"],
        install_windows=["winget install Microsoft.OpenSSH.Beta"],
    ),
]

# ---------------------------------------------------------------------------
# Python SDK packages (installed into the current venv)
# ---------------------------------------------------------------------------

SDK_GROUPS: Dict[str, Dict] = {
    "ssh": {
        "label": "SSH / On-Premises",
        "description": "SSH discovery and remote deployment",
        "packages": ["paramiko>=3.3", "fabric>=3.2"],
    },
    "azure": {
        "label": "Azure SDK",
        "description": "Azure auto-discovery and deployment",
        "packages": [
            "azure-identity>=1.15",
            "azure-mgmt-compute>=30.0",
            "azure-mgmt-containerinstance>=10.1",
            "azure-mgmt-resource>=23.0",
        ],
    },
    "aws": {
        "label": "AWS SDK (boto3)",
        "description": "AWS auto-discovery and deployment",
        "packages": ["boto3>=1.34"],
    },
    "gcp": {
        "label": "GCP SDK",
        "description": "GCP auto-discovery and deployment",
        "packages": ["google-cloud-compute>=1.14", "google-cloud-container>=2.41"],
    },
    "monitoring": {
        "label": "Monitoring & Metrics",
        "description": "Prometheus / Grafana client libs",
        "packages": ["prometheus-client>=0.20"],
    },
}


# ---------------------------------------------------------------------------
# OS detection
# ---------------------------------------------------------------------------

def _os_type() -> str:
    """Return 'mac', 'linux', 'windows', or 'unknown'."""
    s = platform.system().lower()
    if s == "darwin":
        return "mac"
    if s == "linux":
        return "linux"
    if s == "windows":
        return "windows"
    return "unknown"


def _linux_pkg_manager() -> str:
    """Detect apt / yum / dnf on Linux."""
    for mgr in ("apt-get", "apt", "dnf", "yum"):
        if shutil.which(mgr):
            return "yum" if mgr in ("yum", "dnf") else "apt"
    return "apt"


def _brew_available() -> bool:
    return shutil.which("brew") is not None


# ---------------------------------------------------------------------------
# Status check
# ---------------------------------------------------------------------------

@dataclass
class ToolStatus:
    key: str
    name: str
    description: str
    category: str
    installed: bool
    version: Optional[str] = None
    path: Optional[str] = None


def check_tool(tool: ToolInfo) -> ToolStatus:
    """Return current install status for a single tool."""
    path = shutil.which(tool.binary)
    version = None
    if path:
        try:
            flags = tool.version_flag.split()
            result = subprocess.run(
                [tool.binary] + flags,
                capture_output=True, text=True, timeout=5
            )
            out = result.stdout.strip() or result.stderr.strip()
            version = out.splitlines()[0][:80] if out else "installed"
        except Exception:
            version = "installed"
    return ToolStatus(
        key=tool.key,
        name=tool.name,
        description=tool.description,
        category=tool.category,
        installed=path is not None,
        version=version,
        path=path,
    )


def check_all_tools() -> List[ToolStatus]:
    """Return status list for all defined tools."""
    return [check_tool(t) for t in TOOLS]


def check_sdk(group_key: str) -> Dict:
    """Return install status for a Python SDK group."""
    group = SDK_GROUPS.get(group_key, {})
    packages = group.get("packages", [])
    results = []
    for pkg_spec in packages:
        # Parse name from spec like "paramiko>=3.3"
        import_name = pkg_spec.split(">=")[0].split("==")[0].replace("-", "_").split(".")[0]
        # Handle special cases
        import_map = {
            "azure_identity": "azure.identity",
            "azure_mgmt_compute": "azure.mgmt.compute",
            "azure_mgmt_containerinstance": "azure.mgmt.containerinstance",
            "azure_mgmt_resource": "azure.mgmt.resource",
            "google_cloud_compute": "google.cloud.compute_v1",
            "google_cloud_container": "google.cloud.container_v1",
        }
        module_name = import_map.get(import_name, import_name)
        try:
            importlib.import_module(module_name)
            installed = True
        except ImportError:
            installed = False
        results.append({"package": pkg_spec, "installed": installed})
    all_installed = all(r["installed"] for r in results)
    return {
        "key": group_key,
        "label": group.get("label", group_key),
        "description": group.get("description", ""),
        "installed": all_installed,
        "packages": results,
    }


def check_all_sdks() -> List[Dict]:
    """Return status for all SDK groups."""
    return [check_sdk(k) for k in SDK_GROUPS]


# ---------------------------------------------------------------------------
# Installation (streaming)
# ---------------------------------------------------------------------------

def _run_command_streaming(
    cmd: str,
    emit: Callable[[str], None],
    shell: bool = True,
    env: Optional[Dict] = None,
) -> int:
    """Run a shell command, streaming each line to `emit`. Return exit code."""
    emit(f"$ {cmd}")
    try:
        proc = subprocess.Popen(
            cmd,
            shell=shell,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
            text=True,
            env={**os.environ, **(env or {})},
        )
        for line in proc.stdout:
            stripped = line.rstrip()
            if stripped:
                emit(stripped)
        proc.wait()
        return proc.returncode
    except Exception as exc:
        emit(f"ERROR: {exc}")
        return 1


def install_tool_streaming(
    tool_key: str,
    emit: Callable[[str], None],
) -> bool:
    """
    Install a system tool, streaming progress via `emit(line)`.
    Returns True on success.
    """
    tool = next((t for t in TOOLS if t.key == tool_key), None)
    if not tool:
        emit(f"Unknown tool: {tool_key}")
        return False

    os_type = _os_type()
    emit(f"Installing {tool.name} on {platform.system()}…")

    if os_type == "mac":
        if not _brew_available():
            emit("Homebrew not found — installing Homebrew first…")
            rc = _run_command_streaming(
                '/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"',
                emit,
            )
            if rc != 0:
                emit("ERROR: Failed to install Homebrew. Please install manually: https://brew.sh")
                return False
        commands = tool.install_mac
    elif os_type == "linux":
        mgr = _linux_pkg_manager()
        commands = tool.install_linux_apt if mgr == "apt" else tool.install_linux_yum
    elif os_type == "windows":
        commands = tool.install_windows
    else:
        emit(f"Unsupported OS: {platform.system()}")
        return False

    if not commands:
        emit(f"{tool.name} is built-in on this OS — no installation needed.")
        return True

    for cmd in commands:
        rc = _run_command_streaming(cmd, emit)
        if rc != 0:
            emit(f"Command failed (exit {rc}): {cmd}")
            return False

    # Verify
    if shutil.which(tool.binary):
        emit(f"✅ {tool.name} installed successfully.")
        return True
    else:
        emit(f"⚠ {tool.name} installed but binary '{tool.binary}' not in PATH. You may need to restart your shell.")
        return True  # soft success


def install_sdk_streaming(
    group_key: str,
    emit: Callable[[str], None],
    pip_bin: Optional[str] = None,
) -> bool:
    """
    Install a Python SDK group into the current venv, streaming progress.
    Returns True on success.
    """
    group = SDK_GROUPS.get(group_key)
    if not group:
        emit(f"Unknown SDK group: {group_key}")
        return False

    pip = pip_bin or sys.executable.replace("python", "pip").replace("python3", "pip3")
    if not os.path.exists(pip):
        pip = f"{sys.executable} -m pip"

    packages = " ".join(group["packages"])
    emit(f"Installing {group['label']} packages…")
    cmd = f"{sys.executable} -m pip install --quiet {packages}"
    rc = _run_command_streaming(cmd, emit)
    if rc == 0:
        emit(f"✅ {group['label']} SDK installed.")
        return True
    emit(f"❌ Failed to install {group['label']} SDK (exit {rc}).")
    return False


def install_all_pip_streaming(
    emit: Callable[[str], None],
    requirements_file: Optional[str] = None,
) -> bool:
    """Install all requirements.txt packages, streaming progress."""
    if requirements_file and os.path.exists(requirements_file):
        cmd = f"{sys.executable} -m pip install --quiet -r {requirements_file}"
    else:
        cmd = f"{sys.executable} -m pip install --quiet flask flask-socketio flask_cors pyyaml requests paramiko cryptography psutil"
    rc = _run_command_streaming(cmd, emit)
    return rc == 0


# ---------------------------------------------------------------------------
# Convenience summary
# ---------------------------------------------------------------------------

def full_status_report() -> Dict:
    """Return a single dict with all tool + SDK statuses for the UI."""
    tools = check_all_tools()
    sdks = check_all_sdks()
    required_tools = [t for t in tools if t.category == "required"]
    all_required_ok = all(t.installed for t in required_tools)
    return {
        "os": _os_type(),
        "os_detail": f"{platform.system()} {platform.release()}",
        "all_required_ok": all_required_ok,
        "tools": [
            {
                "key": t.key,
                "name": t.name,
                "description": t.description,
                "category": t.category,
                "installed": t.installed,
                "version": t.version,
                "path": t.path,
            }
            for t in tools
        ],
        "sdks": sdks,
    }
