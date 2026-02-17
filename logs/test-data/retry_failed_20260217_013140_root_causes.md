# Retry Failed Rows Root Cause Summary

Source log: logs/test-data/retry_failed_20260217_013140.jsonl
Failed count: 278

## Failures By HTTP Status
- 400: 170
- 500: 78
- 404: 20
- 401: 10

## Top Endpoints By Failures
- /api/accounts: 13
- /api/service-request-settings/categories: 11
- /api/roles: 10
- /api/permissions: 10
- /api/users: 10
- /api/usergroups: 10
- /api/opportunities: 10
- /api/quotes: 10
- /api/orders: 10
- /api/invoices: 10
- /api/payments: 10
- /api/contracts: 10
- /api/subscriptions: 10
- /api/commissions: 10
- /api/email-sequences: 10

## Top Source Files By Failures
- /Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/bulk_crm_seed.json: 40
- /Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/service_request_categories_seed.json: 11
- /Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/system_roles_seed.json: 10
- /Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/system_permissions_seed.json: 10
- /Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/system_user_groups_seed.json: 10
- /Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_quotes_seed.json: 10
- /Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_quote_line_items_seed.json: 10
- /Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_orders_seed.json: 10
- /Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_order_line_items_seed.json: 10
- /Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_invoices_seed.json: 10
- /Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_invoice_line_items_seed.json: 10
- /Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_payments_seed.json: 10
- /Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_contracts_seed.json: 10
- /Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_subscriptions_seed.json: 10
- /Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_commissions_seed.json: 10

## Sample Root Cause Buckets
### Status 400
{"message":"User with this email already exists"}
- POST /api/users (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/bulk_crm_seed.json[0])
- POST /api/users (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/bulk_crm_seed.json[1])
- POST /api/users (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/bulk_crm_seed.json[2])
- POST /api/users (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/bulk_crm_seed.json[3])
- POST /api/users (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/bulk_crm_seed.json[4])

### Status 500
{"error":"Failed to create user group"}
- POST /api/usergroups (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/system_user_groups_seed.json[0])
- POST /api/usergroups (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/system_user_groups_seed.json[1])
- POST /api/usergroups (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/system_user_groups_seed.json[2])
- POST /api/usergroups (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/system_user_groups_seed.json[3])
- POST /api/usergroups (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/system_user_groups_seed.json[4])

### Status 400
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"opportunity":["The opport
- POST /api/opportunities (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/bulk_crm_seed.json[0])
- POST /api/opportunities (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/bulk_crm_seed.json[1])
- POST /api/opportunities (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/bulk_crm_seed.json[2])
- POST /api/opportunities (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/bulk_crm_seed.json[3])
- POST /api/opportunities (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/bulk_crm_seed.json[4])

### Status 400
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"quote":["The quote field 
- POST /api/quotes (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_quotes_seed.json[0])
- POST /api/quotes (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_quotes_seed.json[1])
- POST /api/quotes (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_quotes_seed.json[2])
- POST /api/quotes (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_quotes_seed.json[3])
- POST /api/quotes (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_quotes_seed.json[4])

### Status 500
MySqlConnector.MySqlException (0x80004005): Table 'crm_db.Quotes' doesn't exist
   at MySqlConnector.Core.ServerSession.ReceiveReplyAsync(IOBehavior ioBehavior,
- POST /api/quotes/1/lineitems (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_quote_line_items_seed.json[0])
- POST /api/quotes/1/lineitems (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_quote_line_items_seed.json[1])
- POST /api/quotes/2/lineitems (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_quote_line_items_seed.json[2])
- POST /api/quotes/3/lineitems (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_quote_line_items_seed.json[3])
- POST /api/quotes/4/lineitems (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_quote_line_items_seed.json[4])

### Status 400
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"order":["The order field 
- POST /api/orders (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_orders_seed.json[0])
- POST /api/orders (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_orders_seed.json[1])
- POST /api/orders (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_orders_seed.json[2])
- POST /api/orders (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_orders_seed.json[3])
- POST /api/orders (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_orders_seed.json[4])

### Status 400
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"invoice":["The invoice fi
- POST /api/invoices (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_invoices_seed.json[0])
- POST /api/invoices (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_invoices_seed.json[1])
- POST /api/invoices (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_invoices_seed.json[2])
- POST /api/invoices (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_invoices_seed.json[3])
- POST /api/invoices (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_invoices_seed.json[4])

### Status 400
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"Name":["The Name field is
- POST /api/invoices/1/line-items (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_invoice_line_items_seed.json[0])
- POST /api/invoices/1/line-items (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_invoice_line_items_seed.json[1])
- POST /api/invoices/2/line-items (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_invoice_line_items_seed.json[2])
- POST /api/invoices/3/line-items (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_invoice_line_items_seed.json[3])
- POST /api/invoices/4/line-items (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_invoice_line_items_seed.json[4])

### Status 400
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"payment":["The payment fi
- POST /api/payments (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_payments_seed.json[0])
- POST /api/payments (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_payments_seed.json[1])
- POST /api/payments (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_payments_seed.json[2])
- POST /api/payments (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_payments_seed.json[3])
- POST /api/payments (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_payments_seed.json[4])

### Status 400
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"request":["The request fi
- POST /api/contracts (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_contracts_seed.json[0])
- POST /api/contracts (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_contracts_seed.json[1])
- POST /api/contracts (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_contracts_seed.json[2])
- POST /api/contracts (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_contracts_seed.json[3])
- POST /api/contracts (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/sales_contracts_seed.json[4])

### Status 400
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"sequence":["The sequence 
- POST /api/email-sequences (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/marketing_email_sequences_seed.json[0])
- POST /api/email-sequences (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/marketing_email_sequences_seed.json[1])
- POST /api/email-sequences (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/marketing_email_sequences_seed.json[2])
- POST /api/email-sequences (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/marketing_email_sequences_seed.json[3])
- POST /api/email-sequences (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/marketing_email_sequences_seed.json[4])

### Status 400
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"campaign":["The campaign 
- POST /api/campaigns (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/marketing_campaigns_seed.json[0])
- POST /api/campaigns (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/marketing_campaigns_seed.json[1])
- POST /api/campaigns (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/marketing_campaigns_seed.json[2])
- POST /api/campaigns (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/marketing_campaigns_seed.json[3])
- POST /api/campaigns (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/marketing_campaigns_seed.json[4])

### Status 500
An error occurred while creating the category
- POST /api/service-request-settings/categories (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/service_request_categories_seed.json[0])
- POST /api/service-request-settings/categories (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/service_request_categories_seed.json[1])
- POST /api/service-request-settings/categories (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/service_request_categories_seed.json[2])
- POST /api/service-request-settings/categories (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/service_request_categories_seed.json[3])
- POST /api/service-request-settings/categories (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/service_request_categories_seed.json[4])

### Status 500
"An error occurred while creating the service request"
- POST /api/servicerequests (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/bulk_crm_seed.json[0])
- POST /api/servicerequests (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/bulk_crm_seed.json[1])
- POST /api/servicerequests (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/bulk_crm_seed.json[2])
- POST /api/servicerequests (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/bulk_crm_seed.json[3])
- POST /api/servicerequests (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/bulk_crm_seed.json[4])

### Status 500
System.InvalidOperationException: Unable to resolve service for type 'CRM.Core.Interfaces.ITSM.ICMDBService' while attempting to activate 'CRM.Api.Controllers.C
- POST /api/itsm/cmdb (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/itsm_cmdb_items_seed.json[0])
- POST /api/itsm/cmdb (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/itsm_cmdb_items_seed.json[1])
- POST /api/itsm/cmdb (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/itsm_cmdb_items_seed.json[2])
- POST /api/itsm/cmdb (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/itsm_cmdb_items_seed.json[3])
- POST /api/itsm/cmdb (/Users/alal/Code/Git CRM Solution/crm-solution/e2e-tests/test-data/itsm_cmdb_items_seed.json[4])
