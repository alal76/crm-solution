#!/usr/bin/env python3
"""
Setup CRM datasets, charts, and dashboard in Apache Superset.
Run inside the crm-superset container:
  docker exec crm-superset python /tmp/setup-superset-crm.py
"""
import sys
sys.path.insert(0, "/app/pythonpath")

from superset.app import create_app

app = create_app()

with app.app_context():
    from superset.models.core import Database
    from superset.connectors.sqla.models import SqlaTable
    from superset.models.slice import Slice
    from superset.models.dashboard import Dashboard
    from superset.extensions import db as sadb
    import json

    crm_db = sadb.session.query(Database).filter_by(database_name="CRM MariaDB").first()
    if not crm_db:
        print("ERROR: CRM MariaDB database not found in Superset")
        sys.exit(1)

    print(f"Using CRM MariaDB (ID: {crm_db.id})")

    # Create datasets for key CRM tables
    tables_to_add = [
        "Customers", "Contacts", "Opportunities", "Leads", "Products",
        "ServiceRequests", "MarketingCampaigns", "Quotes", "Invoices", "Users"
    ]

    dataset_map = {}
    for tbl in tables_to_add:
        existing = sadb.session.query(SqlaTable).filter_by(
            table_name=tbl, database_id=crm_db.id
        ).first()
        if existing:
            dataset_map[tbl] = existing
            print(f"  Dataset '{tbl}' already exists (ID: {existing.id})")
        else:
            new_ds = SqlaTable(
                table_name=tbl,
                database_id=crm_db.id,
                schema=None,   # MariaDB uses no schema prefix; using database name causes column lookup failures
            )
            sadb.session.add(new_ds)
            sadb.session.flush()
            dataset_map[tbl] = new_ds
            print(f"  Dataset '{tbl}' created (ID: {new_ds.id})")

    sadb.session.commit()

    # Sync column metadata from live MariaDB schema for all datasets
    print("\nSyncing column metadata from MariaDB (fetch_metadata)...")
    for tbl, ds in dataset_map.items():
        try:
            ds.fetch_metadata(commit=True)
            print(f"  ✓ {tbl}: {len(ds.columns)} columns synced")
        except Exception as e:
            print(f"  ✗ {tbl}: fetch_metadata failed — {e}")

    # Create Charts
    charts_config = [
        {
            "name": "Accounts by Industry",
            "viz_type": "pie",
            "datasource": "Customers",
            "params": {
                "viz_type": "pie",
                "groupby": ["Industry"],
                "metric": {"expressionType": "SIMPLE", "column": {"column_name": "Id"}, "aggregate": "COUNT"},
                "adhoc_filters": [{"expressionType": "SIMPLE", "subject": "IsDeleted", "operator": "==", "comparator": "0", "clause": "WHERE"}],
                "row_limit": 20,
                "color_scheme": "supersetColors",
            }
        },
        {
            "name": "Revenue by Account (Top 15)",
            "viz_type": "dist_bar",
            "datasource": "Customers",
            "params": {
                "viz_type": "dist_bar",
                "groupby": ["Company"],
                "metrics": [{"expressionType": "SIMPLE", "column": {"column_name": "AnnualRevenue"}, "aggregate": "SUM"}],
                "adhoc_filters": [{"expressionType": "SIMPLE", "subject": "IsDeleted", "operator": "==", "comparator": "0", "clause": "WHERE"}],
                "row_limit": 15,
                "order_desc": True,
            }
        },
        {
            "name": "Opportunity Pipeline by Stage",
            "viz_type": "dist_bar",
            "datasource": "Opportunities",
            "params": {
                "viz_type": "dist_bar",
                "groupby": ["Stage"],
                "metrics": [{"expressionType": "SIMPLE", "column": {"column_name": "Amount"}, "aggregate": "SUM"}],
                "adhoc_filters": [{"expressionType": "SIMPLE", "subject": "IsDeleted", "operator": "==", "comparator": "0", "clause": "WHERE"}],
                "order_desc": True,
            }
        },
        {
            "name": "Lead Sources Distribution",
            "viz_type": "pie",
            "datasource": "Leads",
            "params": {
                "viz_type": "pie",
                "groupby": ["Source"],
                "metric": {"expressionType": "SIMPLE", "column": {"column_name": "Id"}, "aggregate": "COUNT"},
                "adhoc_filters": [{"expressionType": "SIMPLE", "subject": "IsDeleted", "operator": "==", "comparator": "0", "clause": "WHERE"}],
                "color_scheme": "supersetColors",
            }
        },
        {
            "name": "Leads by Status",
            "viz_type": "dist_bar",
            "datasource": "Leads",
            "params": {
                "viz_type": "dist_bar",
                "groupby": ["Status"],
                "metrics": [{"expressionType": "SIMPLE", "column": {"column_name": "Id"}, "aggregate": "COUNT"}],
                "adhoc_filters": [{"expressionType": "SIMPLE", "subject": "IsDeleted", "operator": "==", "comparator": "0", "clause": "WHERE"}],
            }
        },
        {
            "name": "Service Requests by Priority",
            "viz_type": "pie",
            "datasource": "ServiceRequests",
            "params": {
                "viz_type": "pie",
                "groupby": ["Priority"],
                "metric": {"expressionType": "SIMPLE", "column": {"column_name": "Id"}, "aggregate": "COUNT"},
                "adhoc_filters": [{"expressionType": "SIMPLE", "subject": "IsDeleted", "operator": "==", "comparator": "0", "clause": "WHERE"}],
                "color_scheme": "supersetColors",
            }
        },
    ]

    chart_ids = []
    for cfg in charts_config:
        ds = dataset_map.get(cfg["datasource"])
        if not ds:
            print(f"  Skipping chart '{cfg['name']}' - no dataset for '{cfg['datasource']}'")
            continue

        existing_chart = sadb.session.query(Slice).filter_by(slice_name=cfg["name"]).first()
        if existing_chart:
            chart_ids.append(existing_chart.id)
            print(f"  Chart '{cfg['name']}' already exists (ID: {existing_chart.id})")
            continue

        chart = Slice(
            slice_name=cfg["name"],
            viz_type=cfg["viz_type"],
            datasource_type="table",
            datasource_id=ds.id,
            params=json.dumps(cfg["params"]),
        )
        sadb.session.add(chart)
        sadb.session.flush()
        chart_ids.append(chart.id)
        print(f"  Chart '{cfg['name']}' created (ID: {chart.id})")

    sadb.session.commit()

    # Create CRM Dashboard
    existing_dash = sadb.session.query(Dashboard).filter_by(dashboard_title="CRM Overview").first()
    if existing_dash:
        print(f"\nDashboard 'CRM Overview' already exists (ID: {existing_dash.id})")
    else:
        charts = sadb.session.query(Slice).filter(Slice.id.in_(chart_ids)).all()
        dash = Dashboard(
            dashboard_title="CRM Overview",
            slug="crm-overview",
            published=True,
        )
        dash.slices = charts
        sadb.session.add(dash)
        sadb.session.commit()
        print(f"\nDashboard 'CRM Overview' created (ID: {dash.id}) with {len(charts)} charts")

    print("\n=== Superset CRM Setup Complete ===")
    print(f"Database: CRM MariaDB (ID: {crm_db.id})")
    print(f"Datasets: {len(dataset_map)}")
    print(f"Charts: {len(chart_ids)}")
    print(f"Dashboard URL: http://192.168.0.9:8088/superset/dashboard/crm-overview/")
    print(f"SQL Lab URL: http://192.168.0.9:8088/sqllab/")
    print(f"Credentials: admin / admin123")
