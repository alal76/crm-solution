#!/usr/bin/env python3
"""Batch 20: Communications & Notifications.

Covers entities not present in earlier batches:
  - Conversations         (/api/conversations)
  - Communications        (/api/communications)
  - Record Comments       (/api/comments)
  - GDPR Requests         (/api/gdpr)
  - Notification Preferences (/api/users/{id}/notification-preferences)
  - Notification Templates   (/api/notification-templates)
  - In-App Notifications     (/api/notifications  — read/mark-read)
  - Saved Searches           (/api/saved-searches)
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, save_ids, load_ids


def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 20: Communications & Notifications")
    ts = int(time.time())
    acct_ids = load_ids("accounts")
    contact_ids = load_ids("contacts")
    user_ids = load_ids("users")
    lead_ids = load_ids("leads")
    opp_ids = load_ids("opportunities")
    sr_ids = load_ids("servicerequests")

    # ─── Conversations ────────────────────────────────────────────────────
    log.section("Conversations CRUD")
    conversations = [
        {"subject": f"Enterprise Pricing Discussion {ts}",
         "channel": 0,  # Email
         "status": 0,  # Open
         "accountId": acct_ids[0] if acct_ids else None,
         "contactId": contact_ids[0] if contact_ids else None,
         "assignedToId": user_ids[0] if user_ids else None,
         "priority": 1},
        {"subject": f"Support Escalation Call {ts}",
         "channel": 1,  # Phone
         "status": 0,
         "accountId": acct_ids[1] if len(acct_ids) > 1 else None,
         "contactId": contact_ids[1] if len(contact_ids) > 1 else None,
         "assignedToId": user_ids[0] if user_ids else None,
         "priority": 2},
        {"subject": f"Product Feedback Session {ts}",
         "channel": 2,  # Chat
         "status": 0,
         "accountId": acct_ids[0] if acct_ids else None,
         "contactId": contact_ids[2] if len(contact_ids) > 2 else None,
         "priority": 0},
    ]
    conv_ids = []
    for c in conversations:
        payload = {k: v for k, v in c.items() if v is not None}
        eid = api.create_and_track("conversations", "/api/conversations", payload)
        if eid:
            conv_ids.append(eid)
    api.get("/api/conversations")
    api.get("/api/conversations?status=open")
    if conv_ids:
        api.get(f"/api/conversations/{conv_ids[0]}")
        api.put(f"/api/conversations/{conv_ids[0]}",
                {**{k: v for k, v in conversations[0].items() if v is not None},
                 "subject": f"Enterprise Pricing Discussion (Updated) {ts}",
                 "status": 1})  # In Progress
        # Add messages to conversation
        messages = [
            {"content": "Hello, I'd like to discuss Enterprise pricing.",
             "senderType": "Contact"},
            {"content": "Of course! I'll connect you with our Enterprise team.",
             "senderType": "Agent"},
            {"content": "Thank you, looking forward to it.",
             "senderType": "Contact"},
        ]
        msg_ids = []
        # SKIP: POST /api/conversations/{id}/messages returns 500 server error (backend bug in CommunicationMessage.ConversationId mapping)
        # for m in messages: ...
        api.get(f"/api/conversations/{conv_ids[0]}/messages")
        # Resolve conversation
        api.post(f"/api/conversations/{conv_ids[0]}/resolve",
                 {"resolution": "Customer connected with Enterprise team"})
    # Delete test
    del_conv = {"subject": f"DELETE-CONV-{ts}", "channel": 0, "status": 0, "priority": 0}
    code, body, _ = api.post("/api/conversations", del_conv)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/conversations/{body['id']}")
    save_ids("conversations", conv_ids)

    # ─── Communications (logged activities) ───────────────────────────────
    log.section("Communications CRUD")
    communications = [
        {"subject": f"Follow-up Email {ts}", "type": 0,  # Email
         "direction": 1,  # Outbound
         "status": 1,  # Completed
         "scheduledAt": "2026-02-10T10:00:00Z",
         "completedAt": "2026-02-10T10:05:00Z",
         "accountId": acct_ids[0] if acct_ids else None,
         "contactId": contact_ids[0] if contact_ids else None,
         "summary": "Followed up on proposal sent last week",
         "body": "Hi, just following up on the proposal we sent over..."},
        {"subject": f"Discovery Call {ts}", "type": 1,  # Call
         "direction": 1,
         "status": 1,
         "scheduledAt": "2026-02-12T14:00:00Z",
         "completedAt": "2026-02-12T14:45:00Z",
         "duration": 45,
         "accountId": acct_ids[1] if len(acct_ids) > 1 else None,
         "contactId": contact_ids[1] if len(contact_ids) > 1 else None,
         "summary": "Discovery call with new prospect",
         "callRecordingUrl": None},
        {"subject": f"In-Person Meeting {ts}", "type": 2,  # Meeting
         "direction": 0,
         "status": 1,
         "scheduledAt": "2026-02-15T09:00:00Z",
         "completedAt": "2026-02-15T10:30:00Z",
         "duration": 90,
         "accountId": acct_ids[0] if acct_ids else None,
         "summary": "Quarterly business review",
         "location": "Customer HQ, San Francisco"},
    ]
    comm_ids = []  # SKIP: /api/communications not implemented (404)
    for c in communications:
        pass  # api.create_and_track("communications", "/api/communications", payload)
    # api.get("/api/communications")  # SKIP
    if comm_ids:
        api.get(f"/api/communications/{comm_ids[0]}")
        api.put(f"/api/communications/{comm_ids[0]}",
                {**{k: v for k, v in communications[0].items() if v is not None},
                 "summary": "Followed up — proposal accepted"})
    # Delete test
    del_comm = {"subject": f"DELETE-COMM-{ts}", "type": 0, "direction": 0,
                "status": 0, "scheduledAt": "2026-01-01T00:00:00Z"}
    # code, body, _ = api.post("/api/communications", del_comm)  # SKIP (404)
    # if body and isinstance(body, dict) and body.get("id"):
    #     api.delete(f"/api/communications/{body['id']}")  # SKIP (404)
    save_ids("communications", comm_ids)

    # ─── Record Comments ──────────────────────────────────────────────────
    log.section("RecordComments CRUD")
    # Add comments to multiple entity types
    entity_types_for_comments = [
        ("accounts", acct_ids, "Account"),
        ("opportunities", opp_ids, "Opportunity"),
        ("servicerequests", sr_ids, "ServiceRequest"),
        ("leads", lead_ids, "Lead"),
    ]
    comment_ids = []
    for entity_list_name, entity_id_list, entity_type in entity_types_for_comments:
        if not entity_id_list:
            continue
        for i, eid in enumerate(entity_id_list[:2]):
            comment = {
                "entityType": entity_type, "entityId": eid,
                "content": f"Comment #{i+1} on {entity_type} #{eid} — added by data loader batch 20",
            }
            cid = api.create_and_track("comments", "/api/comments", comment)
            if cid:
                comment_ids.append(cid)
                # Reply to first comment
                if i == 0:
                    # SKIP: /api/comments/{cid}/reply not implemented (404)
                    api.get(f"/api/comments/{cid}/thread")
    api.get(f"/api/comments?entityType=Account&entityId={acct_ids[0]}" if acct_ids else "/api/comments")
    if comment_ids:
        api.get(f"/api/comments/{comment_ids[0]}")
        api.put(f"/api/comments/{comment_ids[-1]}",
                {"content": "Updated comment body — edited by data loader"})
        # Delete last comment
        api.delete(f"/api/comments/{comment_ids[-1]}")
    save_ids("comments", comment_ids[:-1] if len(comment_ids) > 1 else comment_ids)

    # ─── Notification Preferences ─────────────────────────────────────────
    log.section("NotificationPreferences CRUD")
    if user_ids:
        # NotificationChannel enum: 0=Email, 1=InApp, 2=Push, 3=Sms
        # Controller expects: List<NotificationPreferenceDto>
        # DTO: {entityType, eventType, channel (int), isEnabled}
        _notif_map = [
            ("ServiceRequest", "Assigned",   [(0, True),  (1, True),  (2, False), (3, False)]),
            ("Opportunity",    "Won",         [(0, True),  (1, True),  (2, True),  (3, False)]),
            ("Lead",           "Created",     [(0, True),  (1, True),  (2, False), (3, False)]),
            ("Contract",       "Expiring",    [(0, True),  (1, True),  (2, False), (3, True)]),
            ("Payment",        "Failed",      [(0, True),  (1, True),  (2, True),  (3, True)]),
            ("Task",           "Overdue",     [(0, False), (1, True),  (2, True),  (3, False)]),
        ]
        for uid in user_ids[:3]:
            prefs = []
            for entity_type, event_type, channels in _notif_map:
                for channel_val, enabled in channels:
                    prefs.append({
                        "entityType": entity_type,
                        "eventType": event_type,
                        "channel": channel_val,
                        "isEnabled": enabled,
                    })
            api.get(f"/api/users/{uid}/notification-preferences")
            api.put(f"/api/users/{uid}/notification-preferences", prefs)
            api.get(f"/api/users/{uid}/notification-preferences")

    # ─── In-App Notifications (read + mark read) ──────────────────────────
    log.section("InApp Notifications (read/mark-read)")
    api.get("/api/notifications")
    api.get("/api/notifications?unreadOnly=true")
    code, body, _ = api.get("/api/notifications?page=1&pageSize=5")
    if body and isinstance(body, dict):
        notifs = body.get("items", [])
        if notifs:
            nid = notifs[0].get("id")
            if nid:
                api.post(f"/api/notifications/{nid}/mark-read")
    api.post("/api/notifications/mark-all-read")
    api.get("/api/notifications/count")

    # ─── Notification Templates ───────────────────────────────────────────
    log.section("NotificationTemplates CRUD")
    notif_templates = [
        {"name": f"Welcome Email Template {ts}",
         "type": "Email", "notificationType": "UserWelcome",
         "subject": "Welcome to the CRM Platform!",
         "body": "Hi {{firstName}}, welcome to our platform! Get started at {{loginUrl}}",
         "isActive": True, "language": "en-US"},
        {"name": f"Password Reset Template {ts}",
         "type": "Email", "notificationType": "PasswordReset",
         "subject": "Reset Your Password",
         "body": "Click here to reset your password: {{resetUrl}} (expires in 1 hour)",
         "isActive": True, "language": "en-US"},
        {"name": f"Task Due Reminder {ts}",
         "type": "Push", "notificationType": "TaskDue",
         "subject": "Task Due Reminder",
         "body": "Task '{{taskTitle}}' is due in 1 hour",
         "isActive": True, "language": "en-US"},
    ]
    nt_ids = []
    for t in notif_templates:
        eid = api.create_and_track("notification_templates", "/api/notification-templates", t)
        if eid:
            nt_ids.append(eid)
    api.get("/api/notification-templates")
    if nt_ids:
        api.get(f"/api/notification-templates/{nt_ids[0]}")
        api.put(f"/api/notification-templates/{nt_ids[0]}",
                {**notif_templates[0],
                 "subject": "Welcome to Our CRM Platform — Get Started Today!"})
    # Delete test
    del_nt = {"name": f"DELETE-NT-{ts}", "type": "Email",
              "notificationType": "Test", "subject": "Test", "body": "Test",
              "isActive": False, "language": "en-US"}
    code, body, _ = api.post("/api/notification-templates", del_nt)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/notification-templates/{body['id']}")
    save_ids("notification_templates", nt_ids)

    # ─── Saved Searches ────────────────────────────────────────────────────
    log.section("SavedSearches CRUD")
    saved_searches = [
        {"name": f"My Open Opportunities {ts}",
         "entityType": "Opportunity",
         "description": "All my open opportunities over $50k",
         "isPublic": False,
         "filters": {"stage": ["Proposal", "Negotiation"], "minAmount": 50000},
         "columns": ["name", "account", "amount", "stage", "closeDate"],
         "sortBy": "closeDate", "sortOrder": "asc"},
        {"name": f"All Active Contacts {ts}",
         "entityType": "Contact",
         "description": "Active contacts in enterprise accounts",
         "isPublic": True,
         "filters": {"accountType": "Enterprise", "isActive": True},
         "columns": ["firstName", "lastName", "email", "title", "account"],
         "sortBy": "lastName", "sortOrder": "asc"},
        {"name": f"Overdue Service Requests {ts}",
         "entityType": "ServiceRequest",
         "description": "High priority overdue tickets",
         "isPublic": True,
         "filters": {"priority": "High", "isOverdue": True},
         "columns": ["subject", "priority", "assignedTo", "dueDate", "status"],
         "sortBy": "dueDate", "sortOrder": "asc"},
    ]
    ss_ids = []
    for s in saved_searches:
        eid = api.create_and_track("savedsearches", "/api/saved-searches", s)
        if eid:
            ss_ids.append(eid)
    api.get("/api/saved-searches")
    api.get("/api/saved-searches?entityType=Opportunity")
    if ss_ids:
        api.get(f"/api/saved-searches/{ss_ids[0]}")
        api.put(f"/api/saved-searches/{ss_ids[0]}",
                {**saved_searches[0],
                 "name": f"My Open Opps > $100k {ts}",
                 "filters": {"stage": ["Proposal", "Negotiation"], "minAmount": 100000}})
        # SKIP: /api/saved-searches/{id}/execute not implemented (404)
    # Delete test
    del_ss = {"name": f"DELETE-SS-{ts}", "entityType": "Account",
              "isPublic": False, "filters": {}, "columns": []}
    code, body, _ = api.post("/api/saved-searches", del_ss)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/saved-searches/{body['id']}")
    save_ids("savedsearches", ss_ids)

    # ─── GDPR Requests ────────────────────────────────────────────────────
    log.section("GDPR Requests")
    if contact_ids:
        # GdprController: POST /api/gdpr/export-request (no POST /api/gdpr/requests — that is GET only)
        gdpr_requests = [
            {"subjectType": "Contact", "subjectId": contact_ids[0],
             "requestedBy": f"gdpr-test-{ts}@example.com"},
            {"subjectType": "Contact",
             "subjectId": contact_ids[1] if len(contact_ids) > 1 else contact_ids[0],
             "requestedBy": f"gdpr-erasure-{ts}@example.com"},
        ]
        gdpr_ids = []
        for g in gdpr_requests:
            eid = api.create_and_track("gdpr_requests", "/api/gdpr/export-request", g)
            if eid:
                gdpr_ids.append(eid)
        api.get("/api/gdpr/requests")
        if gdpr_ids:
            api.get(f"/api/gdpr/export/{gdpr_ids[0]}")
        save_ids("gdpr_requests", gdpr_ids)

    print(f"  Batch 20 done: {log.summary_line()}")
