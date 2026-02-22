#!/usr/bin/env python3
"""Batch 06: Marketing.

Covers: Campaigns, CampaignRecipients, CampaignConversions, CampaignMetrics,
CampaignExecution, EmailTemplates, EmailSequences, Forms, LandingPages.
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, ENUMS, save_ids, load_ids


def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 06: Marketing")
    ts = int(time.time())
    contact_ids = load_ids("contacts")
    acct_ids = load_ids("accounts")

    # ---- Email Templates ----
    log.section("EmailTemplates CRUD")
    templates = [
        {"name": f"Welcome Template {ts}", "subject": "Welcome to our company!",
         "htmlBody": "<h1>Welcome!</h1><p>Thank you for your interest.</p>",
         "textBody": "Welcome! Thank you for your interest.",
         "category": "Onboarding", "isActive": True},
        {"name": f"Follow-Up Template {ts}", "subject": "Following up on our conversation",
         "htmlBody": "<p>Hi, just following up on our recent conversation.</p>",
         "textBody": "Hi, just following up on our recent conversation.",
         "category": "Sales", "isActive": True},
    ]
    template_ids = []
    for t in templates:
        eid = api.create_and_track("emailtemplates", "/api/emailtemplates", t)
        if eid:
            template_ids.append(eid)
    api.get("/api/emailtemplates")
    if template_ids:
        api.get(f"/api/emailtemplates/{template_ids[0]}")
        api.put(f"/api/emailtemplates/{template_ids[0]}", {**templates[0], "subject": "Updated Welcome!"})
    save_ids("emailtemplates", template_ids)

    # ---- Email Sequences ----
    log.section("EmailSequences CRUD")
    seq = {"name": f"Onboarding Sequence {ts}", "description": "New customer onboarding",
           "status": 0, "isActive": True,
           "steps": [
               {"order": 1, "type": 0, "delayDays": 0, "subject": "Welcome!",
                "htmlBody": "<p>Welcome aboard!</p>"},
               {"order": 2, "type": 1, "delayDays": 3},
               {"order": 3, "type": 0, "delayDays": 0, "subject": "Getting Started",
                "htmlBody": "<p>Here are some tips to get started.</p>"},
           ]}
    eid = api.create_and_track("emailsequences", "/api/email-sequences", seq)
    if eid:
        api.get(f"/api/email-sequences/{eid}")
        save_ids("emailsequences", [eid])
    api.get("/api/email-sequences")

    # ---- Campaigns ----
    log.section("Campaigns CRUD")
    campaigns = [
        {"name": f"Spring Promo Campaign {ts}", "description": "Spring 2026 promotion",
         "campaignType": 0, "status": 0,
         "startDate": "2026-03-01T00:00:00Z", "endDate": "2026-03-31T23:59:59Z",
         "budget": 50000},
        {"name": f"Product Launch Campaign {ts}", "description": "New product launch",
         "campaignType": 6, "status": 0,
         "startDate": "2026-04-01T00:00:00Z", "endDate": "2026-04-30T23:59:59Z",
         "budget": 100000},
    ]
    campaign_ids = []
    for c in campaigns:
        eid = api.create_and_track("campaigns", "/api/campaigns", c)
        if eid:
            campaign_ids.append(eid)
    api.get("/api/campaigns")
    api.get("/api/campaigns/active")
    if campaign_ids:
        api.get(f"/api/campaigns/{campaign_ids[0]}")
        api.put(f"/api/campaigns/{campaign_ids[0]}", {**campaigns[0], "status": 1,
                                                       "description": "Scheduled for March"})
    # Delete test
    del_c = {"name": f"DELETE-Campaign-{ts}", "campaignType": 0, "status": 0, "budget": 100,
             "startDate": "2026-12-01T00:00:00Z", "endDate": "2026-12-31T00:00:00Z"}
    code, body, _ = api.post("/api/campaigns", del_c)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/campaigns/{body['id']}")
    save_ids("campaigns", campaign_ids)

    # ---- Campaign Recipients ----
    log.section("CampaignRecipients CRUD")
    if campaign_ids and contact_ids:
        recipients = [
            {"campaignId": campaign_ids[0], "contactId": contact_ids[i],
             "status": 0, "email": f"recipient{i}_{ts}@test.com"}
            for i in range(min(3, len(contact_ids)))
        ]
        recip_ids = []
        for r in recipients:
            eid = api.create_and_track("campaignrecipients", "/api/campaign-recipients", r)
            if eid:
                recip_ids.append(eid)
        api.get("/api/campaign-recipients")
        api.get(f"/api/campaign-recipients/campaign/{campaign_ids[0]}")
        api.get(f"/api/campaign-recipients/campaign/{campaign_ids[0]}/count")
        save_ids("campaignrecipients", recip_ids)

    # ---- Campaign Conversions ----
    log.section("CampaignConversions CRUD")
    if campaign_ids:
        conv = {"campaignId": campaign_ids[0],
                "conversionType": "Lead", "conversionDate": "2026-03-15T00:00:00Z",
                "value": 25000, "description": "Lead converted from spring promo"}
        eid = api.create_and_track("campaignconversions", "/api/campaign-conversions", conv)
        if eid:
            api.get(f"/api/campaign-conversions/{eid}")
            api.get(f"/api/campaign-conversions/campaign/{campaign_ids[0]}")
            save_ids("campaignconversions", [eid])

    # ---- Campaign Metrics ----
    log.section("CampaignMetrics")
    if campaign_ids:
        metric = {"campaignId": campaign_ids[0],
                  "metricDate": "2026-03-15T00:00:00Z",
                  "sent": 500, "delivered": 490, "opened": 200,
                  "clicked": 50, "bounced": 10, "unsubscribed": 2}
        api.post("/api/campaign-metrics", metric)

    # ---- Campaign Execution ----
    log.section("CampaignExecution")
    if campaign_ids:
        api.get(f"/api/campaigns/{campaign_ids[0]}/analytics")
        api.get(f"/api/campaigns/{campaign_ids[0]}/recipients")
        api.get(f"/api/campaigns/{campaign_ids[0]}/workflows")
        api.get(f"/api/campaigns/{campaign_ids[0]}/abtests")

    # ---- Forms ----
    log.section("Forms CRUD")
    form = {"name": f"Contact Us Form {ts}", "formKey": f"contact-us-{ts}",
            "description": "Website contact form", "status": 0,
            "fields": [
                {"fieldName": "fullName", "label": "Full Name", "fieldType": 0, "isRequired": True, "order": 1},
                {"fieldName": "email", "label": "Email", "fieldType": 2, "isRequired": True, "order": 2},
                {"fieldName": "message", "label": "Message", "fieldType": 1, "isRequired": False, "order": 3},
            ]}
    eid = api.create_and_track("forms", "/api/forms", form)
    if eid:
        api.get(f"/api/forms/{eid}")
        api.put(f"/api/forms/{eid}", {**form, "description": "Updated contact form"})
        save_ids("forms", [eid])
    api.get("/api/forms")

    # ---- Landing Pages ----
    log.section("LandingPages CRUD")
    lp = {"title": f"Spring Promo Landing Page {ts}",
          "slug": f"spring-promo-{ts}", "status": 0,
          "htmlContent": "<h1>Spring Promotion</h1><p>Sign up now!</p>",
          "metaDescription": "Spring 2026 promotion landing page"}
    if campaign_ids:
        lp["campaignId"] = campaign_ids[0]
    eid = api.create_and_track("landingpages", "/api/landing-pages", lp)
    if eid:
        api.get(f"/api/landing-pages/{eid}")
        save_ids("landingpages", [eid])
    api.get("/api/landing-pages")

    print(f"  Batch 06 done: {log.summary_line()}")
