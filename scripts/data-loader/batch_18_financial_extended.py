#!/usr/bin/env python3
"""Batch 18: Financial Extended.

Covers financial entities not fully covered by earlier batches:
  - Credit Memos        (/api/creditmemos)
  - Order Returns       (/api/orderreturns)
  - Pricing Rules       (/api/pricingrules)
  - Revenue Schedule    (/api/revenue/schedule)
  - Payment Methods     (/api/payments)
  - Tax Rates           (/api/taxrates)
  - Tax Rules           (/api/taxrules)
  - Expense Reports     (/api/expenses)
  - Commissions (link)  (/api/commissions)
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, save_ids, load_ids


def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 18: Financial Extended")
    ts = int(time.time())
    acct_ids = load_ids("accounts")
    contact_ids = load_ids("contacts")
    order_ids = load_ids("orders")
    invoice_ids = load_ids("invoices")
    opp_ids = load_ids("opportunities")
    user_ids = load_ids("users")

    # ─── Credit Memos ─────────────────────────────────────────────────────
    log.section("CreditMemos CRUD")
    credit_memos = [
        {"accountId": acct_ids[0] if acct_ids else 1,
         "number": f"CM-{ts}-001", "status": 0,
         "issueDate": "2026-02-15T00:00:00Z",
         "amount": 1500.00, "currency": "USD",
         "reason": "Product returned by customer",
         "description": "Full credit for returned Enterprise license",
         "lineItems": [
             {"description": "Enterprise License Refund", "quantity": 1,
              "unitPrice": 1500.00, "total": 1500.00}
         ]},
        {"accountId": acct_ids[1] if len(acct_ids) > 1 else 1,
         "number": f"CM-{ts}-002", "status": 0,
         "issueDate": "2026-02-20T00:00:00Z",
         "amount": 500.00, "currency": "USD",
         "reason": "Service SLA breach compensation",
         "description": "Credit for SLA breach in January",
         "lineItems": [
             {"description": "SLA Compensation", "quantity": 1,
              "unitPrice": 500.00, "total": 500.00}
         ]},
        {"accountId": acct_ids[2] if len(acct_ids) > 2 else 1,
         "number": f"CM-{ts}-003", "status": 0,
         "issueDate": "2026-03-01T00:00:00Z",
         "amount": 2500.00, "currency": "USD",
         "reason": "Billing error correction",
         "description": "Credit for double-billing error in February",
         "lineItems": [
             {"description": "Duplicate Charge Refund - Month 1", "quantity": 1,
              "unitPrice": 1250.00, "total": 1250.00},
             {"description": "Duplicate Charge Refund - Month 2", "quantity": 1,
              "unitPrice": 1250.00, "total": 1250.00},
         ]},
    ]
    cm_ids = []
    for cm in credit_memos:
        line_items = cm.pop("lineItems", [])
        payload = {**cm, "lineItems": line_items}
        eid = api.create_and_track("creditmemos", "/api/creditmemos", payload)
        if eid:
            cm_ids.append(eid)
    api.get("/api/creditmemos")
    if cm_ids:
        api.get(f"/api/creditmemos/{cm_ids[0]}")
        api.get(f"/api/creditmemos/by-number/CM-{ts}-001")
        api.put(f"/api/creditmemos/{cm_ids[0]}",
                {**{k: v for k, v in credit_memos[0].items() if k != "lineItems"},
                 "reason": "Product returned — full refund approved",
                 "lineItems": credit_memos[0]["lineItems"]})
        # Void a credit memo
        api.post(f"/api/creditmemos/{cm_ids[0]}/void",
                 {"reason": "Void for testing purposes"})
    # Delete test
    del_cm = {"accountId": acct_ids[0] if acct_ids else 1,
              "number": f"CM-DEL-{ts}", "status": 0,
              "issueDate": "2026-01-01T00:00:00Z",
              "amount": 1.00, "currency": "USD",
              "reason": "Delete test", "lineItems": []}
    code, body, _ = api.post("/api/creditmemos", del_cm)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/creditmemos/{body['id']}")
    save_ids("creditmemos", cm_ids)

    # ─── Order Returns ─────────────────────────────────────────────────────
    log.section("OrderReturns CRUD")
    if order_ids:
        returns = [
            {"orderId": order_ids[0], "status": 0,
             "requestedDate": "2026-02-10T00:00:00Z",
             "reason": "Defective product",
             "returnType": 0,  # Refund
             "refundAmount": 2500.00, "notes": "Customer reported hardware defect",
             "lineItems": [
                 {"orderLineItemId": None, "productId": None,
                  "description": "Defective hardware unit", "quantity": 1,
                  "unitPrice": 2500.00, "total": 2500.00}
             ]},
            {"orderId": order_ids[1] if len(order_ids) > 1 else order_ids[0], "status": 0,
             "requestedDate": "2026-02-15T00:00:00Z",
             "reason": "Wrong item shipped",
             "returnType": 1,  # Replacement
             "refundAmount": 0, "notes": "Replaced with correct item",
             "lineItems": [
                 {"orderLineItemId": None, "productId": None,
                  "description": "Wrong model returned", "quantity": 1,
                  "unitPrice": 3200.00, "total": 3200.00}
             ]},
        ]
        ret_ids = []
        for r in returns:
            line_items = r.pop("lineItems", [])
            payload = {**r, "lineItems": line_items}
            eid = api.create_and_track("orderreturns", "/api/orderreturns", payload)
            if eid:
                ret_ids.append(eid)
        api.get("/api/orderreturns")
        if ret_ids:
            api.get(f"/api/orderreturns/{ret_ids[0]}")
            api.put(f"/api/orderreturns/{ret_ids[0]}",
                    {**{k: v for k, v in returns[0].items() if k not in ("lineItems",)},
                     "notes": "Defect confirmed — full refund authorized",
                     "lineItems": returns[0]["lineItems"]})
            api.post(f"/api/orderreturns/{ret_ids[0]}/approve",
                     {"approvedBy": user_ids[0] if user_ids else 1,
                      "notes": "Approved after inspection"})
        # Delete test
        del_r = {"orderId": order_ids[0], "status": 0,
                 "requestedDate": "2026-01-01T00:00:00Z",
                 "reason": "Test delete", "returnType": 0,
                 "refundAmount": 0.01, "lineItems": []}
        code, body, _ = api.post("/api/orderreturns", del_r)
        if body and isinstance(body, dict) and body.get("id"):
            api.delete(f"/api/orderreturns/{body['id']}")
        save_ids("orderreturns", ret_ids)

    # ─── Pricing Rules ────────────────────────────────────────────────────
    log.section("PricingRules CRUD")
    pricing_rules = [
        {"name": f"Volume Discount 10+ {ts}",
         "description": "10% off when ordering 10+ units",
         "ruleType": "VolumeDiscount", "isActive": True,
         "discountType": "Percentage", "discountValue": 10.0,
         "conditions": {"minQuantity": 10},
         "priority": 10},
        {"name": f"Annual Plan Discount {ts}",
         "description": "20% off for annual billing",
         "ruleType": "BillingCycle", "isActive": True,
         "discountType": "Percentage", "discountValue": 20.0,
         "conditions": {"billingCycle": "Annual"},
         "priority": 20},
        {"name": f"Enterprise Fixed Discount {ts}",
         "description": "$5000 discount for enterprise accounts",
         "ruleType": "AccountTier", "isActive": True,
         "discountType": "Fixed", "discountValue": 5000.0,
         "conditions": {"accountTier": "Enterprise"},
         "priority": 5},
        {"name": f"Loyalty Discount 2yr {ts}",
         "description": "5% off for customers with 2+ years",
         "ruleType": "Loyalty", "isActive": True,
         "discountType": "Percentage", "discountValue": 5.0,
         "conditions": {"minYears": 2},
         "priority": 30},
    ]
    pr_ids = []
    for p in pricing_rules:
        eid = api.create_and_track("pricingrules", "/api/pricingrules", p)
        if eid:
            pr_ids.append(eid)
    api.get("/api/pricingrules")
    api.get("/api/pricingrules?activeOnly=true")
    if pr_ids:
        api.get(f"/api/pricingrules/{pr_ids[0]}")
        api.put(f"/api/pricingrules/{pr_ids[0]}",
                {**pricing_rules[0], "discountValue": 12.0,
                 "description": "Updated — 12% off for 10+ units"})
    # Delete test
    del_p = {"name": f"DELETE-PR-{ts}", "description": "Temp",
             "ruleType": "Manual", "isActive": False,
             "discountType": "Percentage", "discountValue": 0, "priority": 99}
    code, body, _ = api.post("/api/pricingrules", del_p)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/pricingrules/{body['id']}")
    save_ids("pricingrules", pr_ids)

    # Apply pricing rule to opportunity
    if opp_ids and pr_ids:
        api.post(f"/api/opportunities/{opp_ids[0]}/apply-pricing-rule/{pr_ids[0]}")

    # ─── Tax Rates & Rules ────────────────────────────────────────────────
    log.section("TaxRates CRUD")
    tax_rates = [
        {"name": f"US Sales Tax CA {ts}", "rate": 8.25, "region": "CA",
         "country": "US", "taxType": "SalesTax", "isActive": True},
        {"name": f"EU VAT Standard {ts}", "rate": 20.0, "region": "EU",
         "country": "GB", "taxType": "VAT", "isActive": True},
        {"name": f"GST Canada {ts}", "rate": 5.0, "region": "All",
         "country": "CA", "taxType": "GST", "isActive": True},
    ]
    tax_ids = []
    for t in tax_rates:
        eid = api.create_and_track("taxrates", "/api/taxrates", t)
        if eid:
            tax_ids.append(eid)
    api.get("/api/taxrates")
    if tax_ids:
        api.get(f"/api/taxrates/{tax_ids[0]}")
        api.put(f"/api/taxrates/{tax_ids[0]}",
                {**tax_rates[0], "rate": 8.5, "description": "Updated CA sales tax rate"})
    # Delete test
    del_t = {"name": f"DELETE-TAX-{ts}", "rate": 0.0, "region": "None",
             "country": "XX", "taxType": "SalesTax", "isActive": False}
    code, body, _ = api.post("/api/taxrates", del_t)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/taxrates/{body['id']}")
    save_ids("taxrates", tax_ids)

    # ─── Payment Methods ──────────────────────────────────────────────────
    log.section("PaymentMethods CRUD")
    payment_methods = [
        {"name": f"Corporate Visa {ts}",
         "type": "CreditCard", "provider": "Stripe",
         "isDefault": False, "isActive": True,
         "maskedNumber": "****4242", "expiryMonth": 12, "expiryYear": 2027,
         "cardholderName": "ACME Corp",
         "accountId": acct_ids[0] if acct_ids else None},
        {"name": f"ACH Bank Transfer {ts}",
         "type": "BankTransfer", "provider": "ACH",
         "isDefault": False, "isActive": True,
         "bankName": "Chase Bank",
         "routingNumber": "****5678", "accountNumber": "****9012",
         "accountId": acct_ids[0] if acct_ids else None},
    ]
    pm_ids = []
    for pm in payment_methods:
        payload = {k: v for k, v in pm.items() if v is not None}
        eid = api.create_and_track("paymentmethods", "/api/payments", payload)
        if eid:
            pm_ids.append(eid)
    api.get("/api/payments")
    if pm_ids:
        api.get(f"/api/payments/{pm_ids[0]}")
    # Delete test
    del_pm = {"name": f"DELETE-PM-{ts}", "type": "CreditCard", "provider": "Test",
              "isDefault": False, "isActive": False}
    code, body, _ = api.post("/api/payments", del_pm)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/payments/{body['id']}")
    save_ids("paymentmethods", pm_ids)

    # ─── Commission Payouts (view) ─────────────────────────────────────────
    log.section("Commissions Analytics (read)")
    api.get("/api/commissions/analytics/overview")
    api.get("/api/commissions/analytics/by-rep")
    api.get("/api/commissions/analytics/by-period")
    if user_ids:
        api.get(f"/api/commissions?repId={user_ids[0]}")
    api.get("/api/commissions/periods")
    api.get("/api/commissions/settings")

    print(f"  Batch 18 done: {log.summary_line()}")
