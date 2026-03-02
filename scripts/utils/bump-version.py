#!/usr/bin/env python3
"""Auto-bump CRM component versions based on git changes.

Detects which files changed (via ``git diff``) and bumps the
appropriate component version(s) in ``version.json``.  Also syncs
derived files:
  - CRM.Backend/src/CRM.Api/CRM.Api.csproj  (API version)
  - CRM.Frontend/package.json                (frontend version)

Usage:
    # Auto-detect from staged changes (default)
    python scripts/utils/bump-version.py

    # Auto-detect from last N commits
    python scripts/utils/bump-version.py --commits 1

    # Explicitly bump a component
    python scripts/utils/bump-version.py --component api
    python scripts/utils/bump-version.py --component frontend
    python scripts/utils/bump-version.py --component all

    # Minor bump (default is patch)
    python scripts/utils/bump-version.py --minor

    # Dry run (show what would change)
    python scripts/utils/bump-version.py --dry-run
"""
from __future__ import annotations

import argparse
import json
import re
import subprocess
import sys
from datetime import datetime, timezone
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent.parent
VERSION_FILE = REPO_ROOT / "version.json"
CSPROJ_FILE = REPO_ROOT / "CRM.Backend" / "src" / "CRM.Api" / "CRM.Api.csproj"
PACKAGE_JSON = REPO_ROOT / "CRM.Frontend" / "package.json"

# Path prefixes → component mapping
COMPONENT_RULES: list[tuple[str, str]] = [
    ("CRM.Backend/", "api"),
    ("CRM.Frontend/", "frontend"),
    ("CRM.Infrastructure/deployment-tool/", "cdt"),
    ("docker/", "api"),        # Dockerfiles affect both but primarily api
    ("database/", "api"),      # DB changes are backend
    ("scripts/", "cdt"),
]

# These paths bump the solution but not a specific component
SOLUTION_ONLY_PATHS = [
    "version.json",
    "deploy-to-dev-server.sh",
    "build.sh",
    "README.md",
    "docs/",
    ".github/",
]


def git_changed_files(commits: int | None = None) -> list[str]:
    """Return list of changed file paths relative to repo root."""
    if commits:
        cmd = ["git", "diff", "--name-only", f"HEAD~{commits}", "HEAD"]
    else:
        # Staged + unstaged changes
        cmd = ["git", "diff", "--name-only", "HEAD"]
    try:
        result = subprocess.run(
            cmd, capture_output=True, text=True, cwd=REPO_ROOT, check=False
        )
        if result.returncode != 0:
            # Fallback: staged only
            result = subprocess.run(
                ["git", "diff", "--name-only", "--cached"],
                capture_output=True, text=True, cwd=REPO_ROOT, check=False,
            )
        return [f for f in result.stdout.strip().split("\n") if f]
    except FileNotFoundError:
        print("ERROR: git not found", file=sys.stderr)
        return []


def detect_components(files: list[str]) -> set[str]:
    """Determine which components are affected by the changed files."""
    components: set[str] = set()
    for filepath in files:
        matched = False
        for prefix, component in COMPONENT_RULES:
            if filepath.startswith(prefix):
                components.add(component)
                matched = True
                break
        if not matched:
            # Check solution-only paths (don't add to components)
            for sol_path in SOLUTION_ONLY_PATHS:
                if filepath.startswith(sol_path):
                    matched = True
                    break
    return components


def parse_version(ver_str: str) -> tuple[int, int, int]:
    """Parse 'major.minor.patch' into a tuple."""
    parts = ver_str.split(".")
    return int(parts[0]), int(parts[1]), int(parts[2])


def bump_version(ver_str: str, minor: bool = False) -> str:
    """Bump a version string. If minor=True, bump minor and reset patch."""
    major, mn, patch = parse_version(ver_str)
    if minor:
        return f"{major}.{mn + 1}.0"
    return f"{major}.{mn}.{patch + 1}"


def load_version_json() -> dict:
    """Load version.json."""
    with open(VERSION_FILE) as f:
        return json.load(f)


def save_version_json(data: dict) -> None:
    """Save version.json with consistent formatting."""
    with open(VERSION_FILE, "w") as f:
        json.dump(data, f, indent=2, ensure_ascii=False)
        f.write("\n")


def sync_csproj(version: str) -> bool:
    """Update <Version>, <AssemblyVersion>, <FileVersion> in CRM.Api.csproj."""
    if not CSPROJ_FILE.is_file():
        print(f"  WARN: {CSPROJ_FILE} not found", file=sys.stderr)
        return False

    content = CSPROJ_FILE.read_text()
    content = re.sub(
        r"<Version>[^<]+</Version>",
        f"<Version>{version}</Version>",
        content,
    )
    content = re.sub(
        r"<AssemblyVersion>[^<]+</AssemblyVersion>",
        f"<AssemblyVersion>{version}.0</AssemblyVersion>",
        content,
    )
    content = re.sub(
        r"<FileVersion>[^<]+</FileVersion>",
        f"<FileVersion>{version}.0</FileVersion>",
        content,
    )
    CSPROJ_FILE.write_text(content)
    return True


def sync_package_json(version: str) -> bool:
    """Update 'version' field in CRM.Frontend/package.json."""
    if not PACKAGE_JSON.is_file():
        print(f"  WARN: {PACKAGE_JSON} not found", file=sys.stderr)
        return False

    data = json.loads(PACKAGE_JSON.read_text())
    data["version"] = version
    PACKAGE_JSON.write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n")
    return True


def main() -> int:
    parser = argparse.ArgumentParser(description="Bump CRM component versions")
    parser.add_argument(
        "--component", "-c",
        choices=["api", "frontend", "cdt", "all"],
        help="Explicitly bump a specific component (skip auto-detect)",
    )
    parser.add_argument(
        "--commits", "-n", type=int, default=None,
        help="Detect changes from last N commits (default: staged+unstaged)",
    )
    parser.add_argument(
        "--minor", action="store_true",
        help="Minor version bump (default: patch)",
    )
    parser.add_argument(
        "--dry-run", "-d", action="store_true",
        help="Show what would change without modifying files",
    )
    parser.add_argument(
        "--description", "-m", type=str, default=None,
        help="Version description for version.json",
    )
    args = parser.parse_args()

    # Determine which components to bump
    if args.component == "all":
        components = {"api", "frontend", "cdt"}
    elif args.component:
        components = {args.component}
    else:
        files = git_changed_files(args.commits)
        if not files:
            print("No changed files detected. Use --component to bump explicitly.")
            return 0
        print(f"Changed files ({len(files)}):")
        for f in files[:20]:
            print(f"  {f}")
        if len(files) > 20:
            print(f"  ... and {len(files) - 20} more")
        components = detect_components(files)
        if not components:
            print("No component-specific changes detected. Only solution-level files changed.")
            print("Use --component to explicitly bump a component.")

    # Load current version.json
    data = load_version_json()
    today = datetime.now(timezone.utc).strftime("%Y-%m-%d")

    # Ensure components section exists
    if "components" not in data:
        sol_ver = f"{data['major']}.{data['minor']}.{data['patch']}"
        data["components"] = {
            "api": {"version": sol_ver, "lastUpdate": today},
            "frontend": {"version": sol_ver, "lastUpdate": today},
            "cdt": {"version": sol_ver, "lastUpdate": today},
        }

    changes: list[str] = []

    # Bump each affected component
    for comp in sorted(components):
        comp_data = data["components"].get(comp, {})
        old_ver = comp_data.get("version", f"{data['major']}.{data['minor']}.{data['patch']}")
        new_ver = bump_version(old_ver, args.minor)
        changes.append(f"  {comp}: {old_ver} → {new_ver}")

        data["components"][comp] = {
            "version": new_ver,
            "lastUpdate": today,
        }

    # Bump solution-level version (always bumps when any component bumps)
    old_solution = f"{data['major']}.{data['minor']}.{data['patch']}"
    if components:
        new_solution = bump_version(old_solution, args.minor)
        major, minor, patch = parse_version(new_solution)
        data["major"] = major
        data["minor"] = minor
        data["patch"] = patch
        data["lastUpdate"] = today
        changes.append(f"  solution: {old_solution} → {new_solution}")

    if args.description:
        data["description"] = args.description

    # Print summary
    print(f"\nComponents affected: {', '.join(sorted(components)) or 'none'}")
    if changes:
        print("Version bumps:")
        for c in changes:
            print(c)
    else:
        print("No version changes needed.")
        return 0

    if args.dry_run:
        print("\n(dry run — no files modified)")
        return 0

    # Write version.json
    save_version_json(data)
    print(f"  ✓ Updated {VERSION_FILE.relative_to(REPO_ROOT)}")

    # Sync derived files
    if "api" in components:
        api_ver = data["components"]["api"]["version"]
        if sync_csproj(api_ver):
            print(f"  ✓ Synced {CSPROJ_FILE.relative_to(REPO_ROOT)} → {api_ver}")

    if "frontend" in components:
        fe_ver = data["components"]["frontend"]["version"]
        if sync_package_json(fe_ver):
            print(f"  ✓ Synced {PACKAGE_JSON.relative_to(REPO_ROOT)} → {fe_ver}")

    print("\nDone! Remember to commit the updated version files.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
