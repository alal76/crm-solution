#!/usr/bin/env python3
"""Batch 04: Sales Pipeline.

Covers: Opportunities, Quotes, QuoteLineItems, Orders, Invoices, Payments,
Contracts, Subscriptions, Pipelines, Stages, SalesQuotas, SalesForecasts.
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, ENUMS, save_ids, load_ids


def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 04: Sales Pipeline")
    ts = int(time.time())
    acct_ids = load_ids("accounts")
    contact_ids = load_ids("contacts")
    product_ids = load_ids("products")
    user_ids = load_ids("users")

    # ---- Pipelines ----
    log.section("Pipelines")
    api.get("/api/pipelines")

    # ---- Stages ----
    log.section("Stages")
    api.get("/api/stages")

    # ---- Opportunities ----
    log.section("Opportunities CRUD")
    opps = [
        {"name": f"Enterprise Deal {ts}", "accountId": acct_ids[0] if acct_ids else 1,
         "contactId": contact_ids[0] if contact_ids else None,
         "stage": 0, "amount": 250000, "probability": 60,
         "expectedCloseDate": "2026-06-30T00:00:00Z", "source": "Website",
         "description": "Large enterprise opportunity"},
        {"name": f"Mid-Market Sale {ts}", "accountId": acct_ids[1] if len(acct_ids) > 1 else 1,
         "stage": 1, "amount": 75000, "probability": 40,
         "expectedCloseDate": "2026-04-30T00:00:00Z", "source": "Referral"},
        {"name": f"Startup Pilot {ts}", "accountId": acct_ids[2] if len(acct_ids) > 2 else 1,
         "stage": 2, "amount": 15000, "probability": 80,
         "expectedCloseDate": "2026-03-31T00:00:00Z", "source": "Trade Show"},
    ]
    opp_ids = []
    for o in opps:
        payload = {k: v for k, v in o.items() if v is not None}
        eid = api.create_and_track("opportunities", "/api/opportunities", payload)
        if eid:
            opp_ids.append(eid)
    api.get("/api/opportunities")
    if opp_ids:
        api.get(f"/api/opportunities/{opp_ids[0]}")
        api.put(f"/api/opportunities/{opp_ids[0]}", {**opps[0], "stage": 2, "probability": 70,
                                                      "description": "Updated - moving to proposal"})
        api.patch(f"/api/opportunities/{opp_ids[0]}", {"stage": 3})
    # Delete test
    del_o = {"name": f"DELETE-Opp-{ts}", "accountId": acct_ids[0] if acct_ids else 1,
             "stage": 0, "amount": 100, "expectedCloseDate": "2026-12-31T00:00:00Z"}
    code, body, _ = api.post("/api/opportunities", del_o)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/opportunities/{body['id']}")
    save_ids("opportunities", opp_ids)

    # ---- Quotes ----
    log.section("Quotes CRUD")
    quotes = []
    if opp_ids and acct_ids:
        q = {"title": f"Quote for Enterprise Deal {ts}", "opportunityId": opp_ids[0],
             "accountId": acct_ids[0], "status": 0,
             "validUntil": "2026-04-30T00:00:00Z", "subtotal": 250000,
             "discount": 10, "tax": 22500, "total": 247500,
             "notes": "Enterprise pricing"}
        eid = api.create_and_track("quotes", "/api/quotes", q)
        if eid:
            quotes.append(eid)
            api.get(f"/api/quotes/{eid}")
            api.put(f"/api/quotes/{eid}", {**q, "discount": 15, "total": 240000})
            # Quote line items
            if product_ids:
                li = {"quoteId": eid, "productId": product_ids[0],
                      "quantity": 5, "unitPrice": 99999, "discount": 10}
                api.post(f"/api/quotes/{eid}/lineitems", li)
                api.get(f"/api/quotes/{eid}/lineitems")
    api.get("/api/quotes")
    save_ids("quotes", quotes)

    # ---- Orders ----
    log.section("Orders CRUD")
    order_ids = []
    if acct_ids:
        order = {"accountId": acct_ids[0], "status": 0,
                 "orderDate": "2026-02-22T00:00:00Z", "totalAmount": 250000,
                 "shippingAddress": "123 Main St, San Francisco, CA",
                 "billingAddress": "123 Main St, San Francisco, CA",
                 "notes": "Enterprise order"}
        if quotes:
            order["quoteId"] = quotes[0]
        eid = api.create_and_track("orders", "/api/orders", order)
        if eid:
            order_ids.append(eid)
            api.get(f"/api/orders/{eid}")
            api.put(f"/api/orders/{eid}", {**order, "status": 2, "notes": "Approved order"})
    api.get("/api/orders")
    save_ids("orders", order_ids)

    # ---- Invoices ----
    log.section("Invoices CRUD")
    invoice_ids = []
    if acct_ids:
        inv = {"accountId": acct_ids[0], "status": 0,
               "issueDate": "2026-02-22T00:00:00Z", "dueDate": "2026-03-22T00:00:00Z",
               "subtotal": 250000, "tax": 22500, "total": 272500,
               "notes": "Invoice for enterprise order"}
        if order_ids:
            inv["orderId"] = order_ids[0]
        eid = api.create_and_track("invoices", "/api/invoices", inv)
        if eid:
            invoice_ids.append(eid)
            api.get(f"/api/invoices/{eid}")
            api.put(f"/api/invoices/{eid}", {**inv, "status": 3})
    api.get("/api/invoices")
    save_ids("invoices", invoice_ids)

    # ---- Payments ----
    log.section("Payments CRUD")
    payment_ids = []
    if invoice_ids:
        pay = {"invoiceId": invoice_ids[0], "amount": 272500,
               "paymentDate": "2026-02-25T00:00:00Z", "paymentMethod": 2,
               "status": 0, "referenceNumber": f"PAY-{ts}",
               "notes": "Wire transfer payment"}
        eid = api.create_and_track("payments", "/api/payments", pay)
        if eid:
            payment_ids.append(eid)
            api.get(f"/api/payments/{eid}")
    api.get("/api/payments")
    save_ids("payments", payment_ids)

    # ---- Contracts ----
    log.section("Contracts CRUD")
    contract_ids = []
    if acct_ids:
        contract = {"accountId": acct_ids[0], "title": f"Service Agreement {ts}",
                    "status": 0, "startDate": "2026-01-01T00:00:00Z",
                    "endDate": "2027-01-01T00:00:00Z", "value": 500000,
                    "description": "Annual service agreement", "type": "Service"}
        eid = api.create_and_track("contracts", "/api/contracts", contract)
        if eid:
            contract_ids.append(eid)
            api.get(f"/api/contracts/{eid}")
            api.put(f"/api/contracts/{eid}", {**contract, "status": 3, "description": "Active agreement"})
    api.get("/api/contracts")
    save_ids("contracts", contract_ids)

    # ---- Subscriptions ----
    log.section("Subscriptions CRUD")
    sub_ids = []
    if acct_ids:
        sub = {"accountId": acct_ids[0], "name": f"Premium Plan {ts}",
               "status": 0, "startDate": "2026-01-01T00:00:00Z",
               "renewalDate": "2027-01-01T00:00:00Z",
               "monthlyAmount": 9999, "billingCycle": "Monthly",
               "description": "Premium subscription plan"}
        if product_ids:
            sub["productId"] = product_ids[0]
        eid = api.create_and_track("subscriptions", "/api/subscriptions", sub)
        if eid:
            sub_ids.append(eid)
            api.get(f"/api/subscriptions/{eid}")
            api.put(f"/api/subscriptions/{eid}", {**sub, "monthlyAmount": 10999})
    api.get("/api/subscriptions")
    save_ids("subscriptions", sub_ids)

    # ---- Sales Quotas ----
    log.section("SalesQuotas CRUD")
    if user_ids:
        sq = {"userId": user_ids[0], "year": 2026, "quarter": 1,
              "targetAmount": 500000, "description": "Q1 2026 quota"}
        eid = api.create_and_track("salesquotas", "/api/salesquotas", sq)
        if eid:
            api.get(f"/api/salesquotas/{eid}")
            save_ids("salesquotas", [eid])
    api.get("/api/salesquotas")

    # ---- Sales Forecasts ----
    log.section("SalesForecasts CRUD")
    if user_ids:
        sf = {"userId": user_ids[0], "period": "2026-Q1",
              "forecastAmount": 350000, "bestCase": 500000, "worstCase": 200000,
              "forecast": 350000, "description": "Q1 forecast"}
        eid = api.create_and_track("salesforecasts", "/api/salesforecasts", sf)
        if eid:
            api.get(f"/api/salesforecasts/{eid}")
            save_ids("salesforecasts", [eid])
    api.get("/api/salesforecasts")

    print(f"  Batch 04 done: {log.summary_line()}")
