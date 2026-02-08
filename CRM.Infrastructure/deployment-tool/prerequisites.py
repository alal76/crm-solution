#!/usr/bin/env python3
"""
CRM Solution - Prerequisite Checker & Runtime Installer

Checks for required and optional Python packages at startup.
Prompts the user to install missing packages before proceeding.
Resolves the best available version at install time rather than
pinning versions in requirements.txt.

Author: Abhishek Lal
License: AGPL-3.0
"""

import importlib
import subprocess
import sys
from typing import Dict, List, Optional, Tuple


# ---------------------------------------------------------------------------
# Package group definitions
#
# Each group maps a *feature area* to the packages it needs.
# For every package we store:
#   - import_name : the Python module name to try importing
#   - pip_name    : the pip package name used for installation
#   - min_version : minimum acceptable version (None = any)
#   - description : human-readable purpose
# ---------------------------------------------------------------------------

class PackageInfo:
    """Metadata for a single pip package."""
    __slots__ = ("import_name", "pip_name", "min_version", "description")

    def __init__(self, import_name: str, pip_name: str,
                 min_version: Optional[str] = None,
                 description: str = ""):
        self.import_name = import_name
        self.pip_name = pip_name
        self.min_version = min_version
        self.description = description

    @property
    def pip_spec(self) -> str:
        """Return the pip install specifier, e.g. 'flask>=3.0'."""
        if self.min_version:
            return f"{self.pip_name}>={self.min_version}"
        return self.pip_name


# ---- Core: required for the GUI / CLI to start at all --------------------
CORE_PACKAGES: List[PackageInfo] = [
    PackageInfo("flask",  "flask",  "3.0",  "Web framework for the configuration GUI"),
    PackageInfo("yaml",   "pyyaml", "6.0",  "YAML configuration file parsing"),
]

# ---- Feature groups: installed on demand when the user needs them --------
SSH_PACKAGES: List[PackageInfo] = [
    PackageInfo("paramiko", "paramiko", "3.3",  "SSH connections for on-premises discovery"),
    PackageInfo("fabric",   "fabric",   "3.2",  "Remote command execution over SSH"),
]

AZURE_PACKAGES: List[PackageInfo] = [
    PackageInfo("azure.identity",                "azure-identity",               "1.14", "Azure authentication"),
    PackageInfo("azure.mgmt.resource",           "azure-mgmt-resource",          "23.0", "Azure resource management"),
    PackageInfo("azure.mgmt.compute",            "azure-mgmt-compute",           "30.0", "Azure VM management"),
    PackageInfo("azure.mgmt.containerservice",   "azure-mgmt-containerservice",  "28.0", "Azure AKS management"),
    PackageInfo("azure.mgmt.sql",                "azure-mgmt-sql",               "3.0",  "Azure SQL management"),
    PackageInfo("azure.mgmt.network",            "azure-mgmt-network",           "25.0", "Azure network management"),
]

AWS_PACKAGES: List[PackageInfo] = [
    PackageInfo("boto3",     "boto3",     "1.28", "AWS SDK for Python"),
    PackageInfo("botocore",  "botocore",  "1.31", "AWS SDK low-level core"),
]

GCP_PACKAGES: List[PackageInfo] = [
    PackageInfo("google.cloud.compute_v1", "google-cloud-compute",   "1.14", "GCP Compute Engine SDK"),
    PackageInfo("google.cloud.container_v1", "google-cloud-container", "2.32", "GCP GKE SDK"),
]

DOCKER_K8S_PACKAGES: List[PackageInfo] = [
    PackageInfo("docker",     "docker",     "6.1",  "Docker engine API client"),
    PackageInfo("kubernetes", "kubernetes", "28.1",  "Kubernetes API client"),
]

HTTP_PACKAGES: List[PackageInfo] = [
    PackageInfo("requests", "requests", "2.31", "HTTP client for health checks"),
]

CLI_PACKAGES: List[PackageInfo] = [
    PackageInfo("rich",         "rich",         "13.5", "Rich terminal output"),
    PackageInfo("click",        "click",        "8.1",  "CLI framework"),
    PackageInfo("questionary",  "questionary",  "2.0",  "Interactive CLI prompts"),
    PackageInfo("colorlog",     "colorlog",     "6.7",  "Coloured log output"),
]

VALIDATION_PACKAGES: List[PackageInfo] = [
    PackageInfo("jsonschema",       "jsonschema",       "4.19", "JSON schema validation"),
    PackageInfo("dataclasses_json", "dataclasses-json", "0.6",  "Dataclass JSON serialisation"),
    PackageInfo("dateutil",         "python-dateutil",  "2.8",  "Date/time utilities"),
]

# ---- Aggregate registry ---------------------------------------------------
PACKAGE_GROUPS: Dict[str, Tuple[str, List[PackageInfo], bool]] = {
    # key: (display_label, package_list, is_required_for_startup)
    "core":        ("Core (Flask GUI)",              CORE_PACKAGES,        True),
    "http":        ("HTTP Client",                   HTTP_PACKAGES,        False),
    "ssh":         ("SSH / On-Premises Discovery",   SSH_PACKAGES,         False),
    "azure":       ("Azure Cloud SDK",               AZURE_PACKAGES,       False),
    "aws":         ("AWS Cloud SDK",                 AWS_PACKAGES,         False),
    "gcp":         ("Google Cloud SDK",              GCP_PACKAGES,         False),
    "docker_k8s":  ("Docker & Kubernetes",           DOCKER_K8S_PACKAGES,  False),
    "cli":         ("CLI Enhancements",              CLI_PACKAGES,         False),
    "validation":  ("Validation & Serialization",    VALIDATION_PACKAGES,  False),
}


# ---------------------------------------------------------------------------
# Checking logic
# ---------------------------------------------------------------------------

def _is_installed(pkg: PackageInfo) -> bool:
    """Return True if the package can be imported."""
    try:
        importlib.import_module(pkg.import_name)
        return True
    except ImportError:
        return False


def check_group(group_key: str) -> Tuple[List[PackageInfo], List[PackageInfo]]:
    """Check which packages in a group are installed vs missing.

    Returns (installed, missing).
    """
    _, packages, _ = PACKAGE_GROUPS[group_key]
    installed, missing = [], []
    for pkg in packages:
        (_installed_list := installed if _is_installed(pkg) else missing).append(pkg)
    return installed, missing


def check_all() -> Dict[str, Tuple[List[PackageInfo], List[PackageInfo]]]:
    """Check every group.  Returns {group_key: (installed, missing)}."""
    return {key: check_group(key) for key in PACKAGE_GROUPS}


# ---------------------------------------------------------------------------
# Installation logic
# ---------------------------------------------------------------------------

def install_packages(packages: List[PackageInfo], quiet: bool = False) -> bool:
    """Install a list of packages via pip.  Returns True on success."""
    if not packages:
        return True

    specs = [pkg.pip_spec for pkg in packages]
    cmd = [sys.executable, "-m", "pip", "install"] + specs
    if quiet:
        cmd.append("--quiet")

    try:
        print(f"  Installing: {', '.join(specs)}")
        result = subprocess.run(cmd, check=True,
                                capture_output=quiet,
                                text=True)
        # Invalidate import caches so newly installed packages are visible.
        importlib.invalidate_caches()
        return True
    except subprocess.CalledProcessError as exc:
        print(f"  ✗ Installation failed (exit {exc.returncode})")
        if quiet and exc.stderr:
            print(f"    {exc.stderr[:500]}")
        return False


# ---------------------------------------------------------------------------
# Interactive startup check  (works for both GUI and CLI)
# ---------------------------------------------------------------------------

def _print_header():
    print()
    print("=" * 64)
    print("   CRM Solution - Prerequisite Check")
    print("=" * 64)


def _print_group_status(label: str, installed: List[PackageInfo],
                        missing: List[PackageInfo]):
    if not missing:
        print(f"  ✓  {label}")
    else:
        names = ", ".join(p.pip_name for p in missing)
        print(f"  ✗  {label}  — missing: {names}")


def run_startup_check(*, require_groups: Optional[List[str]] = None,
                       headless: bool = False) -> bool:
    """Run the interactive prerequisite check.

    Parameters
    ----------
    require_groups : list[str] | None
        Group keys that *must* be present before the application may
        start.  Defaults to groups marked ``is_required_for_startup``.
    headless : bool
        If True, skip interactive prompts and only report status.

    Returns
    -------
    bool
        True  – all required groups are satisfied.
        False – the user declined to install required packages.
    """
    _print_header()
    print()

    results = check_all()

    # Determine which groups are required
    if require_groups is None:
        require_groups = [k for k, (_, _, req) in PACKAGE_GROUPS.items() if req]

    any_missing = False
    required_missing: List[PackageInfo] = []
    optional_groups_missing: Dict[str, List[PackageInfo]] = {}

    for key, (installed, missing) in results.items():
        label, _, _ = PACKAGE_GROUPS[key]
        _print_group_status(label, installed, missing)
        if missing:
            any_missing = True
            if key in require_groups:
                required_missing.extend(missing)
            else:
                optional_groups_missing[key] = missing

    print()

    if not any_missing:
        print("  All prerequisites are satisfied. ✓")
        print()
        return True

    # -- Handle required packages first ------------------------------------
    if required_missing:
        names = ", ".join(p.pip_name for p in required_missing)
        print(f"  The following packages are REQUIRED to start:")
        print(f"    {names}")
        print()

        if headless:
            print("  Cannot proceed without required packages (headless mode).")
            return False

        answer = _prompt_yn("  Install required packages now?")
        if not answer:
            print("  Cannot proceed without required packages.")
            return False

        print()
        ok = install_packages(required_missing)
        if not ok:
            print("  Failed to install required packages. Please install manually:")
            print(f"    pip install {' '.join(p.pip_spec for p in required_missing)}")
            return False
        print("  Required packages installed successfully. ✓")
        print()

    # -- Offer to install optional groups ----------------------------------
    if optional_groups_missing:
        print("  Optional packages are available for additional features:")
        print()
        for key, missing in optional_groups_missing.items():
            label, _, _ = PACKAGE_GROUPS[key]
            names = ", ".join(p.pip_name for p in missing)
            print(f"    [{key}]  {label}")
            print(f"            {names}")
        print()

        if not headless:
            answer = _prompt_yn("  Install all optional packages now?")
            if answer:
                all_optional = []
                for pkgs in optional_groups_missing.values():
                    all_optional.extend(pkgs)
                print()
                ok = install_packages(all_optional)
                if ok:
                    print("  Optional packages installed successfully. ✓")
                else:
                    print("  Some optional packages failed to install.")
                    print("  You can install them later when needed.")
                print()
            else:
                print()
                print("  Skipped. Optional packages will be installed on demand when needed.")
                print()
        else:
            print("  (Skipping optional packages in headless mode.)")
            print()

    return True


def ensure_group_installed(group_key: str, *, interactive: bool = True) -> bool:
    """Ensure a specific feature group is installed.

    Call this at runtime just before you need the feature.  If packages
    are missing the user is prompted (unless interactive=False).

    Returns True if the group is fully available after this call.
    """
    installed, missing = check_group(group_key)
    if not missing:
        return True

    label, _, _ = PACKAGE_GROUPS[group_key]
    names = ", ".join(p.pip_name for p in missing)
    print()
    print(f"  Feature '{label}' requires additional packages: {names}")

    if interactive:
        answer = _prompt_yn("  Install them now?")
        if not answer:
            print("  Skipped. This feature will not be available.")
            return False
    else:
        print("  Auto-installing required packages...")

    ok = install_packages(missing, quiet=True)
    if ok:
        print(f"  Packages installed for '{label}'. ✓")
        # Re-import so the rest of the code can use them
        importlib.invalidate_caches()
        return True
    else:
        print(f"  Failed to install packages for '{label}'.")
        print(f"  Install manually:  pip install {' '.join(p.pip_spec for p in missing)}")
        return False


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def _prompt_yn(prompt: str, default: bool = True) -> bool:
    """Prompt the user for yes / no."""
    suffix = " [Y/n] " if default else " [y/N] "
    try:
        answer = input(prompt + suffix).strip().lower()
    except (EOFError, KeyboardInterrupt):
        print()
        return default
    if not answer:
        return default
    return answer.startswith("y")


# ---------------------------------------------------------------------------
# CLI entry: python prerequisites.py  → run a standalone check
# ---------------------------------------------------------------------------

if __name__ == "__main__":
    ok = run_startup_check()
    sys.exit(0 if ok else 1)
