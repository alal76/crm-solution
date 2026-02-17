# Common Development Issues

## API enum binding requires numeric values

- Symptom: 400 validation errors such as `The JSON value could not be converted to CRM.Core.Entities.AccountCategory` and `The dto field is required.`
- Cause: API uses default JSON enum handling (numeric only). String enum values fail model binding.
- Fix: Send numeric enum values (example: `AccountCategory.Organization` = `1`, `AccountType.Enterprise` = `3`).
- Where seen: `POST /api/accounts` during test data load.

## Orders tables missing in MariaDB

- Symptom: `Table 'crm_db.Orders' doesn't exist` or `Table 'crm_db.Quotes' doesn't exist` when seeding sales data.
- Cause: Schema deployments before 2026-02-17 did not include Orders/OrderLineItems tables; Quotes may be missing if schema files were not applied.
- Fix: Apply database schema updates (schema files include `010_sales_orders.sql`). For existing databases, run the schema deploy or apply the file directly.
- Where seen: `POST /api/orders/*` and `POST /api/quotes/*/lineitems` during test data load.
