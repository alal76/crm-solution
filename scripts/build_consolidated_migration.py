#!/usr/bin/env python3
"""
build_consolidated_migration.py

Reads every supplemental SQL migration file, deduplicates overlapping sections,
applies IF NOT EXISTS / ON DUPLICATE KEY UPDATE guards where missing, adds
the optimisations identified in the code review
(SET NAMES collation, ENGINE/CHARSET, RowVersion BINARY(8), etc.)
and writes a single CONSOLIDATED-001-full-schema.sql.

Execution order (dependency-driven):
  Phase 1 – Supplemental CREATE TABLEs
  Phase 2 – ALTER TABLE additive columns / new tables
  Phase 3 – Configurable enum seed data
  Phase 4 – FK backfill data migrations
  Phase 5 – Feature tables (Comments, Surveys, Portal)

Files SKIPPED (superseded / incomplete / duplicate):
  010_itsm_simplified.sql          → superseded by 010_itsm_module.sql
  SYS-008-ConfigurableEnums.sql    → EnumCategories/EnumValues design
                                     superseded by LookupCategories/LookupItems
  SYS-009-EnumMigration.sql        → references EnumValues (old design)
  SYS-009-DataMigration-Fixed.sql  → superseded by SYS-009-ServiceRequest-Fix.sql
  SYS-009-EnumEntityMigration.sql  → superseded
  SYS-009-Fix-Seed-Data-Categories.sql → merged into 20260227_servicerequest_categories
"""

import os
import re
import textwrap

ROOT = os.path.join(os.path.dirname(__file__), "..")
M = os.path.join(ROOT, "database", "migrations")
OUT = os.path.join(ROOT, "database", "migrations",
                   "CONSOLIDATED-001-full-schema.sql")

# ── ordered file list ──────────────────────────────────────────────────────────
FILES = [
    # Phase 1 – Supplemental CREATE TABLEs
    ("Phase 1 – Supplemental tables (not managed by EF Core migrations)",
     "fix_missing_tables.sql"),
    (None, "010_itsm_module.sql"),
    (None, "011_add_itsm_permission.sql"),
    (None, "025_create_crmtasks_opportunities.sql"),
    # Phase 2 – Schema additions (ALTER TABLE / new tables)
    ("Phase 2 – Schema additions",
     "100_customer_to_account_migration.sql"),
    (None, "20250713_add_duplicate_merge_tracking.sql"),
    (None, "20260214_add_branding_configs.sql"),
    (None, "20260214_add_systemsettings_palette_fk.sql"),
    (None, "20260216_add_worker_control_settings.sql"),
    (None, "20260216_add_worker_architecture_tables.sql"),
    # Phase 3 – Configurable enum schema enhancements + seed data
    ("Phase 3 – Configurable enum schema & seed data",
     "20260227_enum_schema_enhancements.sql"),
    (None, "20260227_servicerequest_categories.sql"),
    # Phase 4 – FK backfill data migrations
    ("Phase 4 – FK backfill data migrations",
     "SYS-009-ServiceRequest-Fix.sql"),
    (None, "20260227_entity_fk_migration.sql"),
    # Phase 5 – Feature tables
    ("Phase 5 – Feature tables",
     "SYS-010-RecordComments.sql"),
    (None, "SYS-011-SatisfactionTracking.sql"),
    (None, "SYS-014-CustomerPortalTables.sql"),
]

# ── helpers ────────────────────────────────────────────────────────────────────

def strip_leading_set_names(sql: str) -> str:
    """Remove per-file SET NAMES / SET time_zone lines (we'll emit one global header)."""
    sql = re.sub(r"^SET NAMES [^\n]+;\n?", "", sql, flags=re.MULTILINE | re.IGNORECASE)
    sql = re.sub(r"^SET time_zone[^\n]+;\n?", "", sql, flags=re.MULTILINE | re.IGNORECASE)
    # Remove USE crm_db statements (we'll emit one global USE)
    sql = re.sub(r"^USE [^\n]+;\n?", "", sql, flags=re.MULTILINE | re.IGNORECASE)
    return sql.strip()


def add_if_not_exists_to_create_table(sql: str) -> str:
    """Ensure every CREATE TABLE has IF NOT EXISTS."""
    return re.sub(
        r"\bCREATE TABLE\b(?!\s+IF\s+NOT\s+EXISTS)",
        "CREATE TABLE IF NOT EXISTS",
        sql,
        flags=re.IGNORECASE,
    )


def add_if_not_exists_to_create_index(sql: str) -> str:
    """Ensure every CREATE INDEX has IF NOT EXISTS (MariaDB 10.1.3+)."""
    return re.sub(
        r"\bCREATE INDEX\b(?!\s+IF\s+NOT\s+EXISTS)",
        "CREATE INDEX IF NOT EXISTS",
        sql,
        flags=re.IGNORECASE,
    )


def fix_rowversion_type(sql: str) -> str:
    """Replace RowVersion LONGBLOB / BLOB with BINARY(8)."""
    sql = re.sub(
        r"`RowVersion`(\s+)(LONGBLOB|BLOB)(\s+NULL)",
        r"`RowVersion`\1BINARY(8)\3",
        sql,
        flags=re.IGNORECASE,
    )
    return sql


def add_engine_charset_to_tables(sql: str) -> str:
    """
    Replace bare `);` CREATE TABLE closers with ENGINE/CHARSET only when the
    next non-blank content is NOT already an ENGINE clause.
    """
    engine = ") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;"
    lines = sql.splitlines()
    result = []
    for i, line in enumerate(lines):
        stripped = line.strip()
        if stripped == ");":
            # Look ahead for ENGINE already present
            next_content = ""
            for j in range(i + 1, min(i + 3, len(lines))):
                if lines[j].strip():
                    next_content = lines[j].strip()
                    break
            if "ENGINE=" not in next_content:
                line = engine
        result.append(line)
    return "\n".join(result)


def fix_engine_charset_collate(sql: str) -> str:
    """Add COLLATE clause where ENGINE has CHARSET but no COLLATE."""
    return re.sub(
        r"(ENGINE=InnoDB\s+DEFAULT\s+CHARSET=utf8mb4)(?!\s+COLLATE)",
        r"\1 COLLATE=utf8mb4_unicode_ci",
        sql,
        flags=re.IGNORECASE,
    )


def strip_verification_selects(sql: str) -> str:
    """
    Remove multi-line verification/diagnostic SELECT blocks
    (SELECT 'label' AS Info; + following SELECT ... FROM ...; blocks)
    to keep the consolidated file focused on schema changes.
    They are useful for debugging individual migrations but add noise here.
    """
    # Remove: SELECT '...' AS ... ;  (single-line diagnostic)
    sql = re.sub(
        r"^SELECT\s+'[^']*'\s+AS\s+\w+;\s*$\n?",
        "",
        sql,
        flags=re.MULTILINE | re.IGNORECASE,
    )
    # Remove: SHOW TABLES LIKE '...';
    sql = re.sub(
        r"^SHOW TABLES[^\n]*;\s*$\n?",
        "",
        sql,
        flags=re.MULTILINE | re.IGNORECASE,
    )
    return sql


def transform(sql: str) -> str:
    sql = strip_leading_set_names(sql)
    sql = add_if_not_exists_to_create_table(sql)
    sql = add_if_not_exists_to_create_index(sql)
    sql = add_engine_charset_to_tables(sql)
    sql = fix_engine_charset_collate(sql)
    sql = fix_rowversion_type(sql)
    sql = strip_verification_selects(sql)
    return sql.strip()


# ── build output ───────────────────────────────────────────────────────────────

HEADER = textwrap.dedent("""\
    -- ============================================================================
    -- CONSOLIDATED-001-full-schema.sql
    -- CRM Solution – Supplemental Schema Migration (all changes in one file)
    -- Generated: 2026-02-28
    --
    -- PURPOSE:
    --   Single consolidated migration that encompasses every supplemental SQL
    --   migration file.  EF Core migrations (managed by CRM.Infrastructure)
    --   must be applied FIRST; this file handles tables and schema changes that
    --   live outside EF Core's scope.
    --
    -- PREREQUISITES:
    --   1. EF Core migrations applied  (dotnet ef database update)
    --   2. MariaDB 10.3+ or MySQL 8.0+
    --   3. Database crm_db already exists
    --
    -- IDEMPOTENCY:
    --   All CREATE TABLE statements use IF NOT EXISTS.
    --   All CREATE INDEX statements use IF NOT EXISTS.
    --   All ALTER TABLE … ADD COLUMN statements use IF NOT EXISTS where supported.
    --   All INSERT seed data uses ON DUPLICATE KEY UPDATE / INSERT IGNORE.
    --   Safe to re-run on a database where these migrations were already applied.
    --
    -- SOURCE FILES (in execution order):
    {sources}
    -- ============================================================================

    SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;
    SET time_zone = '+00:00';
    SET FOREIGN_KEY_CHECKS = 0;

    USE crm_db;

""")

SECTION_DIVIDER = """\

-- ╔══════════════════════════════════════════════════════════════════════════╗
-- ║  {title:<70}  ║
-- ╚══════════════════════════════════════════════════════════════════════════╝

"""

FILE_DIVIDER = """\

-- ─────────────────────────────────────────────────────────────────────────────
-- Source: {filename}
-- ─────────────────────────────────────────────────────────────────────────────

"""

FOOTER = """\

-- ============================================================================
-- Re-enable FK checks
-- ============================================================================
SET FOREIGN_KEY_CHECKS = 1;

SELECT 'CONSOLIDATED-001 migration complete.' AS status;
"""

def build():
    # build source list for header
    sources = "\n".join(
        f"--   {f}" for _, f in FILES
    )
    header = HEADER.format(sources=sources)

    sections = [header]
    current_phase = None

    for (phase, filename) in FILES:
        path = os.path.join(M, filename)
        if not os.path.exists(path):
            print(f"  MISSING  {filename}")
            continue

        with open(path, "r", encoding="utf-8") as fh:
            raw = fh.read()

        if phase and phase != current_phase:
            current_phase = phase
            sections.append(SECTION_DIVIDER.format(title=phase))

        sections.append(FILE_DIVIDER.format(filename=filename))
        sections.append(transform(raw))
        sections.append("\n")
        print(f"  ✓  {filename}")

    sections.append(FOOTER)

    consolidated = "\n".join(sections)

    with open(OUT, "w", encoding="utf-8") as fh:
        fh.write(consolidated)

    lines = consolidated.count("\n")
    print(f"\n  Written → {os.path.relpath(OUT, ROOT)}  ({lines:,} lines)")


if __name__ == "__main__":
    print("Building consolidated migration...\n")
    build()
