#!/usr/bin/env python3
"""Regenerate CDT output for 192.168.1.251 with password recovery from last CDT run."""
import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent))
from core.generator import ConfigGenerator

CDT_ROOT = Path(__file__).parent
PREVIOUS_ENV = CDT_ROOT / "generated" / ".env"
PROFILE_PATH = CDT_ROOT / "generated" / "deployment-config.json"
OUTPUT_DIR = Path("/tmp/cdt-251-deploy")

# Load the saved CDT profile
with open(PROFILE_PATH) as f:
    profile = json.load(f)

# Recover secrets from the last CDT run's .env
recovered = ConfigGenerator.recover_secrets_from_env(PREVIOUS_ENV)
print(f"Recovered {len(recovered)} secrets from {PREVIOUS_ENV}")

# Inject recovered DB secrets into profile (required for validation)
if "secrets" not in profile or not isinstance(profile.get("secrets"), dict):
    profile["secrets"] = {}
for key in ("db_password", "db_root_password", "jwt_secret", "admin_password"):
    if key in recovered:
        profile["secrets"][key] = recovered[key]
        profile[key] = recovered[key]

# Generate with auto-recovery + password preservation
gen = ConfigGenerator()
result = gen.generate(
    profile,
    output_dir=OUTPUT_DIR,
    previous_env=PREVIOUS_ENV,  # explicit: ensures provider passwords are recovered
)

print(f"\nSuccess: {result.success}")
for f in result.files:
    print(f"Generated: {f.filename} ({f.path.stat().st_size} bytes)")

# Verify key properties
env_text = (OUTPUT_DIR / ".env").read_text()
localhost_count = env_text.count("DOMAIN_NAME=localhost")
services = []
compose = (OUTPUT_DIR / "docker-compose.yml").read_text()
import re
for m in re.finditer(r"^\s{2}(\S+):", compose, re.MULTILINE):
    name = m.group(1)
    if name not in ("version", "services", "volumes", "networks") and not name.startswith("#"):
        services.append(name)
# Deduplicate preserving order
seen = set()
unique = []
for s in services:
    if s not in seen:
        seen.add(s)
        unique.append(s)

print(f"\nServices in compose ({len(unique)}):")
for s in unique:
    print(f"  - {s}")
print(f"\n'localhost' in DOMAIN_NAME: {localhost_count}")

# Check domain
domain_lines = [l for l in env_text.splitlines() if l.startswith("DOMAIN_NAME=")]
if domain_lines:
    print(f"DOMAIN_NAME: {domain_lines[0]}")

# Verify a recovered password survived
if "db_password" in recovered:
    db_pw = recovered["db_password"]
    # It may be docker-escaped in the .env, so check the un-escaped version
    if db_pw.replace("$", "$$") in env_text or db_pw in env_text:
        print("DB_PASSWORD: preserved from previous .env")
    else:
        print("WARNING: DB_PASSWORD not found in output!")
