#!/usr/bin/env python3
"""Batch 19: Portals, Engagement & Marketing Advanced.

Covers entities not present in earlier batches:
  - Customer Portal        (/api/portal/crm/*)
  - Partner Portal         (/api/partner-portal/*)
  - Web-to-Lead Forms      (/api/webtoleadforms)
  - Landing Pages          (/api/landing-pages)
  - UTM Tracking Links     (/api/campaigns/{id}/links)
  - Campaign Conversions   (/api/campaignconversions)
  - Event Attendees        (/api/event-attendees  or  /api/events/{id}/attendees)
  - Forum Posts            (/api/forum/posts)
  - Satisfaction Surveys   (/api/satisfaction)
  - Unsubscribe            (/api/unsubscribe — write test)
  - Customer Segments      (/api/customer-segments)
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, save_ids, load_ids


def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 19: Portals, Engagement & Marketing Advanced")
    ts = int(time.time())
    acct_ids = load_ids("accounts")
    contact_ids = load_ids("contacts")
    lead_ids = load_ids("leads")
    campaign_ids = load_ids("campaigns")
    user_ids = load_ids("users")
    sr_ids = load_ids("servicerequests")

    # ─── Customer Portal ──────────────────────────────────────────────────
    log.section("Customer Portal (read/create operations)")
    # Portal tickets (customer-facing view of service requests)
    api.get("/api/portal/crm/tickets")
    api.get("/api/portal/crm/tickets?status=open")
    if contact_ids:
        api.get(f"/api/portal/crm/contacts/{contact_ids[0]}")
    # Portal knowledge base
    api.get("/api/portal/crm/knowledge-base")
    api.get("/api/portal/crm/knowledge-base/featured")

    # ─── Partner Portal ───────────────────────────────────────────────────
    log.section("Partner Portal (read/create operations)")
    api.get("/api/partner-portal/deals")
    api.get("/api/partner-portal/leads")
    api.get("/api/partner-portal/resources")
    # Register a partner deal
    if acct_ids:
        deal = {"name": f"Partner Deal {ts}", "accountId": acct_ids[0],
                "estimatedValue": 75000.00, "currency": "USD",
                "partnerContactName": "Jane Partner",
                "partnerContactEmail": f"partner-{ts}@testpartner.com",
                "description": "Partner-referred enterprise opportunity",
                "status": "Registered", "closeDate": "2026-06-30T00:00:00Z"}
        dp_code, dp_body, _ = api.post("/api/partner-portal/deals", deal)
        if dp_body and isinstance(dp_body, dict) and dp_body.get("id"):
            deal_id = dp_body["id"]
            log.track_id("partner_deals", deal_id)
            api.get(f"/api/partner-portal/deals/{deal_id}")
            api.put(f"/api/partner-portal/deals/{deal_id}",
                    {**deal, "status": "Approved", "description": "Approved partner deal"})
            save_ids("partner_deals", [deal_id])

    # ─── Web-to-Lead Forms ────────────────────────────────────────────────
    log.section("WebToLeadForms CRUD")
    wtl_forms = [
        {"name": f"Contact Us Form {ts}",
         "description": "Main website contact form",
         "status": 0,  # Draft
         "fields": [
             {"name": "firstName", "label": "First Name", "type": "text",
              "required": True, "sortOrder": 1},
             {"name": "lastName", "label": "Last Name", "type": "text",
              "required": True, "sortOrder": 2},
             {"name": "email", "label": "Email", "type": "email",
              "required": True, "sortOrder": 3},
             {"name": "company", "label": "Company", "type": "text",
              "required": False, "sortOrder": 4},
             {"name": "message", "label": "Message", "type": "textarea",
              "required": False, "sortOrder": 5},
         ],
         "redirectUrl": "https://example.com/thank-you",
         "emailNotification": True,
         "notifyEmails": ["sales@example.com"],
         "autoresponderEnabled": True,
         "autoresponderSubject": "Thanks for contacting us",
         "autoresponderBody": "We'll be in touch within 24 hours."},
        {"name": f"Demo Request Form {ts}",
         "description": "Product demo request form",
         "status": 0,
         "fields": [
             {"name": "firstName", "label": "First Name", "type": "text",
              "required": True, "sortOrder": 1},
             {"name": "email", "label": "Business Email", "type": "email",
              "required": True, "sortOrder": 2},
             {"name": "phone", "label": "Phone", "type": "tel",
              "required": False, "sortOrder": 3},
             {"name": "company", "label": "Company", "type": "text",
              "required": True, "sortOrder": 4},
             {"name": "teamSize", "label": "Team Size", "type": "select",
              "required": False, "sortOrder": 5,
              "options": ["1-10", "11-50", "51-200", "200+"]},
         ],
         "redirectUrl": "https://example.com/demo-confirmed",
         "emailNotification": True,
         "notifyEmails": ["demo@example.com"]},
    ]
    wtl_ids = []
    for f in wtl_forms:
        fields = f.pop("fields", [])
        payload = {**f, "fields": fields}
        eid = api.create_and_track("webtoleadforms", "/api/webtoleadforms", payload)
        if eid:
            wtl_ids.append(eid)
    api.get("/api/webtoleadforms")
    if wtl_ids:
        api.get(f"/api/webtoleadforms/{wtl_ids[0]}")
        api.put(f"/api/webtoleadforms/{wtl_ids[0]}",
                {**{k: v for k, v in wtl_forms[0].items() if k not in ("fields",)},
                 "status": 1,  # Published
                 "description": "Published contact form",
                 "fields": wtl_forms[0]["fields"]})
        # Test submit a form
        api.post(f"/api/webtoleadforms/{wtl_ids[0]}/submit",
                 {"firstName": "Test", "lastName": "Submitter",
                  "email": f"test-submit-{ts}@example.com",
                  "company": "Test Corp", "message": "Test submission from data loader"})
        api.get(f"/api/webtoleadforms/{wtl_ids[0]}/submissions")
    # Delete test
    del_f = {"name": f"DELETE-WTL-{ts}", "description": "Temp", "status": 0, "fields": []}
    code, body, _ = api.post("/api/webtoleadforms", del_f)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/webtoleadforms/{body['id']}")
    save_ids("webtoleadforms", wtl_ids)

    # ─── Landing Pages ────────────────────────────────────────────────────
    log.section("LandingPages CRUD")
    landing_pages = [
        {"name": f"Enterprise Trial Page {ts}",
         "title": "Start Your Enterprise Trial", "slug": f"enterprise-trial-{ts}",
         "description": "Enterprise product trial landing page",
         "status": 0,  # Draft
         "metaTitle": "Free Enterprise Trial", "metaDescription": "Try our CRM free for 30 days",
         "heroHeadline": "Transform Your Sales Process",
         "heroSubheadline": "Free 30-Day Enterprise Trial",
         "ctaText": "Start Free Trial", "ctaUrl": f"/trial/{ts}",
         "formId": wtl_ids[0] if wtl_ids else None},
        {"name": f"Product Feature Page {ts}",
         "title": "CRM Features Overview", "slug": f"features-{ts}",
         "description": "Product features landing page",
         "status": 0,
         "metaTitle": "CRM Features", "metaDescription": "Explore all CRM features",
         "heroHeadline": "Everything You Need to Close More Deals",
         "ctaText": "Book a Demo", "ctaUrl": f"/demo/{ts}",
         "formId": wtl_ids[1] if len(wtl_ids) > 1 else None},
    ]
    lp_ids = []
    for lp in landing_pages:
        payload = {k: v for k, v in lp.items() if v is not None}
        eid = api.create_and_track("landingpages", "/api/landing-pages", payload)
        if eid:
            lp_ids.append(eid)
    api.get("/api/landing-pages")
    if lp_ids:
        api.get(f"/api/landing-pages/{lp_ids[0]}")
        api.get(f"/api/landing-pages/by-slug/enterprise-trial-{ts}")
        api.put(f"/api/landing-pages/{lp_ids[0]}",
                {**{k: v for k, v in landing_pages[0].items() if v is not None},
                 "status": 1,  # Published
                 "description": "Published enterprise trial landing page"})
        api.get(f"/api/landing-pages/{lp_ids[0]}/analytics")
    # Delete test
    del_lp = {"name": f"DELETE-LP-{ts}", "title": "Delete Test",
              "slug": f"delete-test-{ts}", "status": 0}
    code, body, _ = api.post("/api/landing-pages", del_lp)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/landing-pages/{body['id']}")
    save_ids("landingpages", lp_ids)

    # ─── UTM Tracking Links ───────────────────────────────────────────────
    log.section("UTM Tracking Links CRUD")
    if campaign_ids:
        utm_links = [
            {"campaignId": campaign_ids[0],
             "name": f"LinkedIn Lead Gen App {ts}",
             "destinationUrl": "https://example.com/landing",
             "utmSource": "linkedin", "utmMedium": "paid_social",
             "utmCampaign": f"enterprise-q1-{ts}",
             "utmContent": "lead-gen-ad-v1", "utmTerm": "crm-software"},
            {"campaignId": campaign_ids[0],
             "name": f"Google Ads Brand {ts}",
             "destinationUrl": "https://example.com/products",
             "utmSource": "google", "utmMedium": "cpc",
             "utmCampaign": f"brand-search-{ts}",
             "utmContent": "brand-exact", "utmTerm": "crm+solution"},
            {"campaignId": campaign_ids[1] if len(campaign_ids) > 1 else campaign_ids[0],
             "name": f"Email Newsletter Link {ts}",
             "destinationUrl": "https://example.com/blog",
             "utmSource": "newsletter", "utmMedium": "email",
             "utmCampaign": f"monthly-newsletter-{ts}",
             "utmContent": "blog-link"},
        ]
        utm_ids = []
        for u in utm_links:
            cid = u["campaignId"]
            eid = api.create_and_track("utm_links", f"/api/campaigns/{cid}/links", u)
            if eid:
                utm_ids.append(eid)
        api.get(f"/api/campaigns/{campaign_ids[0]}/links")
        api.get(f"/api/campaigns/{campaign_ids[0]}/links/analytics")
        save_ids("utm_links", utm_ids)

    # ─── Campaign Conversions ─────────────────────────────────────────────
    log.section("CampaignConversions")
    if campaign_ids and lead_ids:
        for i, cid in enumerate(campaign_ids[:2]):
            api.post("/api/campaignconversions",
                     {"campaignId": cid,
                      "leadId": lead_ids[i % len(lead_ids)],
                      "conversionType": "Lead",
                      "convertedAt": "2026-02-15T14:30:00Z",
                      "value": 5000.00, "currency": "USD"})
        api.get(f"/api/campaignconversions?campaignId={campaign_ids[0]}")

    # ─── Event Attendees ──────────────────────────────────────────────────
    log.section("EventAttendees CRUD")
    events = [
        {"name": f"CRM User Conference {ts}", "description": "Annual user conference",
         "eventDate": "2026-05-15T09:00:00Z", "endDate": "2026-05-17T17:00:00Z",
         "location": "San Francisco, CA", "isVirtual": False,
         "maxAttendees": 500, "isActive": True},
        {"name": f"Quarterly Webinar {ts}", "description": "Q2 Product Roadmap Webinar",
         "eventDate": "2026-04-20T14:00:00Z", "endDate": "2026-04-20T15:30:00Z",
         "location": "Online", "isVirtual": True,
         "maxAttendees": 1000, "isActive": True},
    ]
    event_ids = []
    for e in events:
        eid = api.create_and_track("events", "/api/events", e)
        if eid:
            event_ids.append(eid)
    api.get("/api/events")
    if event_ids and contact_ids:
        eid = event_ids[0]
        attendees = [
            {"contactId": contact_ids[i], "registrationDate": "2026-03-01T00:00:00Z",
             "status": "Registered", "ticketType": "Full Access"}
            for i in range(min(3, len(contact_ids)))
        ]
        att_ids = []
        for att in attendees:
            aid = api.create_and_track("event_attendees", f"/api/events/{eid}/attendees", att)
            if aid:
                att_ids.append(aid)
        api.get(f"/api/events/{eid}/attendees")
        if att_ids:
            api.post(f"/api/events/{eid}/attendees/{att_ids[0]}/check-in",
                     {"checkInTime": "2026-05-15T09:15:00Z"})
        api.get(f"/api/events/{eid}/stats")
        save_ids("event_attendees", att_ids)
    save_ids("events", event_ids)

    # ─── Forum Posts ──────────────────────────────────────────────────────
    log.section("ForumPosts CRUD")
    categories_forum = [
        {"name": f"General Discussion {ts}", "description": "General CRM topics",
         "sortOrder": 1, "isActive": True},
        {"name": f"Support Q&A {ts}", "description": "Get help from the community",
         "sortOrder": 2, "isActive": True},
    ]
    forum_cat_ids = []
    for fc in categories_forum:
        eid = api.create_and_track("forum_categories", "/api/forum/categories", fc)
        if eid:
            forum_cat_ids.append(eid)
    api.get("/api/forum/categories")

    forum_posts = [
        {"title": f"How to set up lead scoring? {ts}",
         "body": "Looking for best practices on configuring lead scoring in the CRM.",
         "categoryId": forum_cat_ids[0] if forum_cat_ids else None,
         "tags": ["leads", "scoring", "tips"], "isPinned": False},
        {"title": f"Integration with Email Providers {ts}",
         "body": "Has anyone successfully integrated with Mailgun? Steps?",
         "categoryId": forum_cat_ids[0] if forum_cat_ids else None,
         "tags": ["email", "integration"], "isPinned": False},
    ]
    fp_ids = []
    for p in forum_posts:
        payload = {k: v for k, v in p.items() if v is not None}
        eid = api.create_and_track("forum_posts", "/api/forum/posts", payload)
        if eid:
            fp_ids.append(eid)
    api.get("/api/forum/posts")
    if fp_ids:
        api.get(f"/api/forum/posts/{fp_ids[0]}")
        # Add replies
        api.post(f"/api/forum/posts/{fp_ids[0]}/replies",
                 {"body": "Great question! Here's how we set ours up...",
                  "isAnswer": False})
        api.get(f"/api/forum/posts/{fp_ids[0]}/replies")
        # Upvote
        api.post(f"/api/forum/posts/{fp_ids[0]}/upvote")
    save_ids("forum_posts", fp_ids)

    # ─── Satisfaction Surveys ─────────────────────────────────────────────
    log.section("Satisfaction Surveys (CSAT/NPS)")
    # CSAT responses
    if sr_ids and contact_ids:
        for i, sr_id in enumerate(sr_ids[:3]):
            cid = contact_ids[i % len(contact_ids)]
            api.post(f"/api/satisfaction/csat",
                     {"serviceRequestId": sr_id, "contactId": cid,
                      "score": [4, 5, 3][i], "feedback": ["Good", "Excellent!", "Average"][i],
                      "surveyDate": "2026-02-15T00:00:00Z"})
    # NPS survey responses
    if contact_ids:
        for i, cid_nps in enumerate(contact_ids[:5]):
            api.post("/api/satisfaction/nps",
                     {"contactId": cid_nps, "score": [7, 9, 5, 10, 8][i],
                      "feedback": "Survey response from data loader",
                      "surveyDate": "2026-02-01T00:00:00Z"})
    api.get("/api/satisfaction/csat/summary")
    api.get("/api/satisfaction/nps/summary")
    api.get("/api/satisfaction/nps/trend?months=6")

    # ─── Customer Segments ────────────────────────────────────────────────
    log.section("CustomerSegments CRUD")
    segments = [
        {"name": f"Enterprise Customers {ts}",
         "description": "All accounts with ARR > $100k",
         "isActive": True, "isDynamic": True,
         "criteria": [
             {"field": "annualRevenue", "operator": "greaterThan", "value": 100000},
             {"field": "accountType", "operator": "equals", "value": "Enterprise"},
         ]},
        {"name": f"At-Risk Accounts {ts}",
         "description": "Accounts with declining usage in last 60 days",
         "isActive": True, "isDynamic": True,
         "criteria": [
             {"field": "usageTrend", "operator": "lessThan", "value": -10},
             {"field": "daysLastLogin", "operator": "greaterThan", "value": 30},
         ]},
        {"name": f"Champions {ts}",
         "description": "High NPS advocates",
         "isActive": True, "isDynamic": True,
         "criteria": [
             {"field": "npsScore", "operator": "greaterThanOrEqual", "value": 9},
             {"field": "lifetimeRevenue", "operator": "greaterThan", "value": 50000},
         ]},
    ]
    seg_ids = []
    for s in segments:
        criteria = s.pop("criteria", [])
        payload = {**s, "criteria": criteria}
        eid = api.create_and_track("customer_segments", "/api/customer-segments", payload)
        if eid:
            seg_ids.append(eid)
    api.get("/api/customer-segments")
    if seg_ids:
        api.get(f"/api/customer-segments/{seg_ids[0]}")
        api.get(f"/api/customer-segments/{seg_ids[0]}/members")
        api.post(f"/api/customer-segments/{seg_ids[0]}/refresh")
    # Delete test
    del_s = {"name": f"DELETE-SEG-{ts}", "description": "Temp",
             "isActive": False, "isDynamic": False, "criteria": []}
    code, body, _ = api.post("/api/customer-segments", del_s)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/customer-segments/{body['id']}")
    save_ids("customer_segments", seg_ids)

    print(f"  Batch 19 done: {log.summary_line()}")
