#!/usr/bin/env python3
"""
fix_migration_optimizations.py
Applies the following optimizations across all DB migration SQL files:
  1.  SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci added / fixed on every file
  2.  USE crm_dev  →  USE crm_db  (010_itsm_module.sql)
  3.  ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
      added to every CREATE TABLE that is missing it (010_itsm_module.sql &
      010_itsm_simplified.sql)
  4.  COLLATE=utf8mb4_unicode_ci appended where ENGINE clause was utf8mb4-only
      (025_create_crmtasks_opportunities.sql)
  5.  RowVersion LONGBLOB  →  BINARY(8)  (SYS-014-CustomerPortalTables.sql)
"""

import os
import re

MIGRATIONS = os.path.join(
    os.path.dirname(__file__),
    "..", "database", "migrations"
)

SET_NAMES_LINE  = "SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;"
TIME_ZONE_LINE  = "SET time_zone = '+00:00';"
SET_NAMES_BLOCK = SET_NAMES_LINE + "\n" + TIME_ZONE_LINE
ITSM_MODULE_SQL = "010_itsm_module.sql"

ENGINE_CLAUSE = (
    ") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;"
)


# ---------------------------------------------------------------------------
# helpers
# ---------------------------------------------------------------------------

def read(path):
    with open(path, "r", encoding="utf-8") as f:
        return f.read()


def write(path, content):
    with open(path, "w", encoding="utf-8") as f:
        f.write(content)


def patch(path, *transforms):
    """Apply a sequence of transform functions to a file, writing only if changed."""
    if not os.path.exists(path):
        print(f"  MISSING  {os.path.basename(path)}")
        return
    src = read(path)
    dst = src
    for fn in transforms:
        dst = fn(dst)
    if dst != src:
        write(path, dst)
        print(f"  PATCHED  {os.path.basename(path)}")
    else:
        print(f"  no-change {os.path.basename(path)}")


# ---------------------------------------------------------------------------
# transform functions
# ---------------------------------------------------------------------------

def add_set_names(src: str) -> str:
    """
    Ensure SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci is present.
    If already present without COLLATE, adds it.
    If completely absent, inserts after the opening comment block.
    """
    # Fix existing SET NAMES that is missing COLLATE
    src = re.sub(
        r"SET NAMES utf8mb4\s*(?!COLLATE);",
        SET_NAMES_LINE,
        src,
    )
    # Guard: if SET NAMES is now present, return
    if SET_NAMES_LINE in src:
        return src

    # Insert after the opening comment block (lines starting with -- or empty)
    lines = src.splitlines(keepends=False)
    insert_at = 0
    for i, line in enumerate(lines):
        stripped = line.strip()
        if stripped.startswith("--") or stripped == "":
            insert_at = i + 1
        else:
            break

    lines.insert(insert_at, "")
    lines.insert(insert_at, SET_NAMES_BLOCK)
    lines.insert(insert_at, "")
    return "\n".join(lines)


def fix_use_database(src: str) -> str:
    """Replace USE crm_dev with USE crm_db."""
    return src.replace("USE crm_dev;", "USE crm_db;")


def add_engine_charset_to_tables(src: str) -> str:
    """
    Replace bare `);` CREATE TABLE closers with ENGINE/CHARSET clause.
    Matches lines that consist solely of ');' (possibly preceded by whitespace)
    and are NOT already followed by ENGINE.
    """
    # We process line by line to avoid touching sub-expressions that happen to
    # start with `);` (there are none in these files, but be safe).
    lines = src.splitlines(keepends=False)
    result = []
    for i, line in enumerate(lines):
        stripped = line.strip()
        if stripped == ");" :
            # Check that the next non-blank line is NOT already an ENGINE clause
            next_content = ""
            for j in range(i + 1, min(i + 3, len(lines))):
                if lines[j].strip():
                    next_content = lines[j].strip()
                    break
            if "ENGINE=" not in next_content:
                line = ENGINE_CLAUSE
        result.append(line)
    return "\n".join(result)


def add_collate_to_engine(src: str) -> str:
    """
    Append COLLATE=utf8mb4_unicode_ci to ENGINE clauses that only have CHARSET.
    """
    return re.sub(
        r"(ENGINE=InnoDB\s+DEFAULT\s+CHARSET=utf8mb4)(?!\s+COLLATE)",
        r"\1 COLLATE=utf8mb4_unicode_ci",
        src,
    )


def fix_rowversion_type(src: str) -> str:
    """Replace RowVersion LONGBLOB with BINARY(8) to match schema convention."""
    return re.sub(
        r"`RowVersion`(\s+)LONGBLOB(\s+NULL)",
        r"`RowVersion`\1BINARY(8)\2",
        src,
    )


# ---------------------------------------------------------------------------
# apply
# ---------------------------------------------------------------------------

def p(filename):
    return os.path.join(MIGRATIONS, filename)


print("=== Migration Optimization Pass ===\n")

print("1. SET NAMES COLLATE header ─────────────────────────────────────────")
files_need_set_names = [
    "SYS-008-ConfigurableEnums.sql",
    "SYS-009-DataMigration-Fixed.sql",
    "SYS-009-EnumEntityMigration.sql",
    "SYS-009-Fix-Seed-Data-Categories.sql",
    "SYS-009-ServiceRequest-Fix.sql",
    "SYS-010-RecordComments.sql",
    "SYS-011-SatisfactionTracking.sql",
    "SYS-014-CustomerPortalTables.sql",
    "010_itsm_module.sql",
    "010_itsm_simplified.sql",
    "011_add_itsm_permission.sql",
    "025_create_crmtasks_opportunities.sql",
    "100_customer_to_account_migration.sql",
    "20250713_add_duplicate_merge_tracking.sql",
    "20260214_add_branding_configs.sql",
    "20260214_add_systemsettings_palette_fk.sql",
    "20260216_add_worker_control_settings.sql",
    "20260216_add_worker_architecture_tables.sql",
    "20260227_entity_fk_migration.sql",
    "20260227_enum_schema_enhancements.sql",
    "20260227_servicerequest_categories.sql",
    "fix_missing_tables.sql",
]
for fn in files_need_set_names:
    patch(p(fn), add_set_names)

print("\n2. USE crm_dev → crm_db ─────────────────────────────────────────────")
patch(p(ITSM_MODULE_SQL), fix_use_database)

print("\n3. ENGINE/CHARSET on ITSM tables ────────────────────────────────────")
patch(p(ITSM_MODULE_SQL),    add_engine_charset_to_tables)
patch(p("010_itsm_simplified.sql"), add_engine_charset_to_tables)

print("\n4. COLLATE missing from ENGINE clause ───────────────────────────────")
patch(p("025_create_crmtasks_opportunities.sql"), add_collate_to_engine)
# Apply defensively to all files in case any have utf8mb4 without collate
for fn in files_need_set_names:
    patch(p(fn), add_collate_to_engine)

print("\n5. RowVersion LONGBLOB → BINARY(8) ──────────────────────────────────")
patch(p("SYS-014-CustomerPortalTables.sql"), fix_rowversion_type)

print("\nDone.")
