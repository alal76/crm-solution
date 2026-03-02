#!/usr/bin/env python3
"""
Superset Datasource Refresh & Schema Sync Script
=================================================
Fixes missing columns in Superset datasets by syncing from the live MariaDB schema.

Run inside the crm-superset container:
  docker cp scripts/superset-refresh-datasources.py crm-superset:/tmp/
  docker exec crm-superset python /tmp/superset-refresh-datasources.py

What this script does:
  1. Verifies the CRM MariaDB database is registered in Superset
     - If not registered: registers it automatically
  2. Ensures all key CRM tables are added as physical datasets
     - If not added: creates them
  3. For EVERY dataset: calls fetch_metadata() → syncs all columns/types
     from the live database schema (fixes missing column issue)
  4. Reports a summary of tables and column counts

Run after every EF Core migration to keep Superset in sync.
"""
import sys
import os

sys.path.insert(0, "/app/pythonpath")

import urllib.parse

CRM_MARIADB_HOST = os.environ.get("CRM_MARIADB_HOST", "crm-mariadb")
CRM_MARIADB_PORT = os.environ.get("CRM_MARIADB_PORT", "3306")
CRM_MARIADB_DB   = os.environ.get("CRM_MARIADB_DB",   "crm_db")
CRM_MARIADB_USER = os.environ.get("CRM_MARIADB_USER", "crm_user")
# Password is URL-encoded -- @ must be %40 in the URI
# otherwise the parser splits at the @ and treats the remainder as the host
_raw_pass = os.environ.get("CRM_MARIADB_PASS")
if not _raw_pass:
    print("ERROR: CRM_MARIADB_PASS environment variable must be set", file=sys.stderr)
    sys.exit(1)
CRM_MARIADB_PASS_ENC = urllib.parse.quote_plus(_raw_pass)

# All CRM tables to register as Superset datasets
# NOTE: MariaDB table names after EF Core migrations - verify with: SHOW TABLES;
CRM_TABLES = [
    # Core CRM
    "Accounts",            # Was "Customers" — renamed in EF migration
    "Contacts",
    "Leads",
    "Opportunities",
    "OpportunityProducts",
    "Products",
    "Interactions",
    "AccountContacts",
    "AccountRelationships",
    "AccountHealthSnapshots",
    # Sales
    "Quotes",
    "QuoteLineItems",
    "Orders",
    "OrderLineItems",
    "Invoices",
    "InvoiceLineItems",
    "Payments",
    "Contracts",
    "Subscriptions",
    "SubscriptionItems",
    "SubscriptionUsages",     # Was "SubscriptionUsage" — EF pluralizes
    "SubscriptionUsageLimits",
    "CreditMemos",
    # Marketing
    "MarketingCampaigns",
    "CampaignMetrics",
    "CampaignRecipients",
    "CampaignConversions",
    "CampaignLinkClicks",
    "EmailTemplates",
    "EmailSequences",
    "EmailSequenceSteps",
    "WebVisitors",
    # Sales Performance
    "SalesQuotas",
    "SalesForecasts",
    "ForecastHistories",      # Was "SalesForecastHistory"
    "ForecastLineItems",
    "CommissionPlans",
    "CommissionStatements",
    # Service Desk / ITSM
    "ServiceRequests",
    "KnowledgeArticles",
    "SLAPolicies",
    "ITSMEscalationRules",    # Was "EscalationRules"
    "ITSMEscalationPolicies",
    "ITSMEscalationLevels",
    # Users & Auth
    "Users",
    "UserGroups",
    "Departments",
    "AuditLogs",
    # System
    "SystemSettings",
    "WorkflowDefinitions",
    "AIAgentUsages",
]

# Dataset name corrections: old incorrect table_name → correct table_name
# (handles cases where the dataset was registered with a wrong table name)
DATASET_NAME_CORRECTIONS = {
    "Customers":             "Accounts",
    "SubscriptionUsage":     "SubscriptionUsages",
    "SalesForecastHistory":  "ForecastHistories",
    "EscalationRules":       "ITSMEscalationRules",
}


def main():
    from superset.app import create_app
    app = create_app()

    with app.app_context():
        from superset.models.core import Database
        from superset.connectors.sqla.models import SqlaTable
        from superset.extensions import db as sadb

        print("=" * 60)
        print("CRM Superset Datasource Refresh")
        print("=" * 60)

        # ── Step 1: Ensure CRM MariaDB is registered ────────────────
        crm_db = sadb.session.query(Database).filter_by(
            database_name="CRM MariaDB"
        ).first()

        if not crm_db:
            print("\n[REGISTER] CRM MariaDB not found — registering now...")
            sqlalchemy_uri = (
                f"mysql+mysqldb://{CRM_MARIADB_USER}:{CRM_MARIADB_PASS_ENC}"
                f"@{CRM_MARIADB_HOST}:{CRM_MARIADB_PORT}/{CRM_MARIADB_DB}"
                f"?charset=utf8mb4"
            )
            crm_db = Database(
                database_name="CRM MariaDB",
                sqlalchemy_uri=sqlalchemy_uri,
                expose_in_sqllab=True,
                allow_run_async=True,
                allow_ctas=False,
                allow_cvas=False,
                allow_dml=False,
                allow_file_upload=False,
                extra="{\"metadata_params\":{},\"engine_params\":{},\"metadata_cache_timeout\":{},\"schemas_allowed_for_file_upload\":[]}",
            )
            sadb.session.add(crm_db)
            sadb.session.commit()
            print(f"  ✓ Registered CRM MariaDB (ID: {crm_db.id})")
        else:
            # Fix the URI: ensure correct driver (mysqldb not pymysql) and URL-encoded password
            correct_uri = (
                f"mysql+mysqldb://{CRM_MARIADB_USER}:{CRM_MARIADB_PASS_ENC}"
                f"@{CRM_MARIADB_HOST}:{CRM_MARIADB_PORT}/{CRM_MARIADB_DB}"
                f"?charset=utf8mb4"
            )
            if crm_db.sqlalchemy_uri != correct_uri:
                print(f"\n[FIX] Updating CRM MariaDB connection URI (password URL-encoding + host)")
                print(f"  Old URI: {crm_db.sqlalchemy_uri}")
                crm_db.sqlalchemy_uri = correct_uri
                sadb.session.commit()
                print(f"  ✓ URI updated (ID: {crm_db.id})")
            else:
                print(f"\n[OK] CRM MariaDB already registered with correct URI (ID: {crm_db.id})")

        # ── Step 1b: Fix misnamed datasets ───────────────────────────
        corrections_applied = 0
        for old_name, new_name in DATASET_NAME_CORRECTIONS.items():
            stale = sadb.session.query(SqlaTable).filter_by(
                table_name=old_name, database_id=crm_db.id
            ).first()
            if stale:
                stale.table_name = new_name
                corrections_applied += 1
                print(f"\n[FIX] Renamed dataset '{old_name}' → '{new_name}'")
        if corrections_applied:
            sadb.session.commit()
            print(f"  ✓ {corrections_applied} dataset name(s) corrected")

        # ── Step 2: Ensure all tables are registered as datasets ─────
        print(f"\n[DATASETS] Ensuring {len(CRM_TABLES)} tables are registered...")

        dataset_map = {}
        created_count = 0
        existing_count = 0

        for table_name in CRM_TABLES:
            existing = sadb.session.query(SqlaTable).filter_by(
                table_name=table_name,
                database_id=crm_db.id,
            ).first()

            if existing:
                dataset_map[table_name] = existing
                existing_count += 1
            else:
                new_ds = SqlaTable(
                    table_name=table_name,
                    database_id=crm_db.id,
                    schema=None,   # MariaDB uses no schema prefix
                )
                sadb.session.add(new_ds)
                sadb.session.flush()
                dataset_map[table_name] = new_ds
                created_count += 1
                print(f"  + Created dataset: {table_name}")

        sadb.session.commit()
        print(f"  ✓ {existing_count} existing, {created_count} newly created")

        # ── Step 3: Refresh ALL datasets (sync columns from MariaDB) ─
        print(f"\n[REFRESH] Syncing column metadata from MariaDB schema...")
        print(f"  This fixes missing/outdated columns after EF Core migrations.\n")

        success = 0
        failed = 0
        failed_tables = []

        for table_name, dataset in dataset_map.items():
            try:
                # fetch_metadata() queries the live DB via INFORMATION_SCHEMA
                # and upserts all column definitions into Superset's metadata store
                dataset.fetch_metadata(commit=True)
                col_count = len(dataset.columns)
                print(f"  ✓ {table_name:<40} {col_count:>3} columns")
                success += 1
            except Exception as ex:
                print(f"  ✗ {table_name:<40} FAILED: {ex}")
                failed += 1
                failed_tables.append(table_name)

        # ── Step 4: Summary ──────────────────────────────────────────
        print("\n" + "=" * 60)
        print(f"DONE: {success} tables refreshed, {failed} failed")
        if failed_tables:
            print(f"Failed tables (check if they exist in crm_db):")
            for t in failed_tables:
                print(f"  - {t}")
        print("=" * 60)
        print("\nSuperset datasources are now up to date.")
        print("Reload your browser to see updated columns in charts/datasets.")

        if failed > 0:
            sys.exit(1)


if __name__ == "__main__":
    main()
