#!/usr/bin/env python3
"""Batch 05: Activities & Interactions.

Covers: Interactions, Activities, Tasks, Notes, EventAttendees, Conversations.
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, ENUMS, save_ids, load_ids


def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 05: Activities & Interactions")
    ts = int(time.time())
    acct_ids = load_ids("accounts")
    contact_ids = load_ids("contacts")
    opp_ids = load_ids("opportunities")
    user_ids = load_ids("users")

    # ---- Interactions ----
    log.section("Interactions CRUD")
    interactions = [
        {"interactionType": 1, "direction": 1, "outcome": 1,
         "subject": f"Sales call with Acme {ts}", "description": "Discussed enterprise deal",
         "accountId": acct_ids[0] if acct_ids else None,
         "contactId": contact_ids[0] if contact_ids else None,
         "durationMinutes": 30, "interactionDate": "2026-02-20T10:00:00Z"},
        {"interactionType": 2, "direction": 0, "outcome": 3,
         "subject": f"Product demo meeting {ts}", "description": "Presented CRM features",
         "accountId": acct_ids[1] if len(acct_ids) > 1 else None,
         "durationMinutes": 60, "interactionDate": "2026-02-21T14:00:00Z"},
        {"interactionType": 0, "direction": 1, "outcome": 1,
         "subject": f"Follow-up email {ts}", "description": "Sent proposal follow-up",
         "contactId": contact_ids[1] if len(contact_ids) > 1 else None,
         "interactionDate": "2026-02-22T09:00:00Z"},
    ]
    interaction_ids = []
    for i in interactions:
        payload = {k: v for k, v in i.items() if v is not None}
        eid = api.create_and_track("interactions", "/api/interactions", payload)
        if eid:
            interaction_ids.append(eid)
    api.get("/api/interactions")
    if interaction_ids:
        api.get(f"/api/interactions/{interaction_ids[0]}")
    # Timeline views
    if acct_ids:
        api.get(f"/api/activities/account/{acct_ids[0]}/timeline")
    if opp_ids:
        api.get(f"/api/activities/opportunity/{opp_ids[0]}/timeline")
    api.get("/api/activities/recent")
    api.get("/api/activities/stats")
    # Delete test
    del_i = {"interactionType": 0, "direction": 1, "outcome": 0,
             "subject": f"DELETE-Interaction-{ts}", "interactionDate": "2026-02-22T00:00:00Z"}
    code, body, _ = api.post("/api/interactions", del_i)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/interactions/{body['id']}")
    save_ids("interactions", interaction_ids)

    # ---- Activities ----
    log.section("Activities CRUD")
    activities = [
        {"activityType": 0, "title": f"Sent proposal email {ts}",
         "description": "Enterprise proposal emailed",
         "entityType": "Account", "entityId": acct_ids[0] if acct_ids else 1,
         "activityDate": "2026-02-22T11:00:00Z"},
        {"activityType": 4, "title": f"Scheduled demo {ts}",
         "description": "Product demo scheduled",
         "entityType": "Opportunity", "entityId": opp_ids[0] if opp_ids else 1,
         "activityDate": "2026-02-25T14:00:00Z"},
    ]
    activity_ids = []
    for a in activities:
        eid = api.create_and_track("activities", "/api/activities", a)
        if eid:
            activity_ids.append(eid)
    api.get("/api/activities")
    if activity_ids:
        api.get(f"/api/activities/{activity_ids[0]}")
    # Delete test
    del_a = {"activityType": 99, "title": f"DELETE-Activity-{ts}",
             "entityType": "Account", "entityId": acct_ids[0] if acct_ids else 1,
             "activityDate": "2026-02-22T00:00:00Z"}
    code, body, _ = api.post("/api/activities", del_a)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/activities/{body['id']}")
    save_ids("activities", activity_ids)

    # ---- Event Attendees ----
    log.section("EventAttendees")
    if activity_ids and contact_ids:
        att = {"attendeeType": 1, "attendeeId": contact_ids[0],
               "isOrganizer": False, "isRequired": True}
        api.post(f"/api/activities/{activity_ids[0]}/attendees", att)
        api.get(f"/api/activities/{activity_ids[0]}/attendees")

    # ---- Tasks ----
    log.section("Tasks CRUD")
    tasks = [
        {"title": f"Follow up on Enterprise deal {ts}", "description": "Call John Smith re: proposal",
         "taskType": 0, "status": 0, "priority": 2,
         "dueDate": "2026-02-28T17:00:00Z",
         "assignedToUserId": user_ids[0] if user_ids else None,
         "relatedEntityType": "Opportunity", "relatedEntityId": opp_ids[0] if opp_ids else None},
        {"title": f"Prepare demo materials {ts}", "description": "Create slides for product demo",
         "taskType": 4, "status": 0, "priority": 1,
         "dueDate": "2026-02-24T12:00:00Z"},
        {"title": f"Send contract draft {ts}", "description": "Draft and send contract",
         "taskType": 6, "status": 0, "priority": 2,
         "dueDate": "2026-03-01T17:00:00Z"},
    ]
    task_ids = []
    for t in tasks:
        payload = {k: v for k, v in t.items() if v is not None}
        eid = api.create_and_track("tasks", "/api/tasks", payload)
        if eid:
            task_ids.append(eid)
    api.get("/api/tasks")
    if task_ids:
        api.get(f"/api/tasks/{task_ids[0]}")
        api.put(f"/api/tasks/{task_ids[0]}", {**tasks[0], "status": 1,
                                               "description": "In progress - called John"})
        api.put(f"/api/tasks/{task_ids[0]}", {**tasks[0], "status": 2,
                                               "description": "Completed - task done"})
    # Delete test
    del_t = {"title": f"DELETE-Task-{ts}", "taskType": 8, "status": 0, "priority": 0,
             "dueDate": "2026-12-31T00:00:00Z"}
    code, body, _ = api.post("/api/tasks", del_t)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/tasks/{body['id']}")
    save_ids("tasks", task_ids)

    # ---- Notes ----
    log.section("Notes CRUD")
    notes = [
        {"title": f"Meeting Notes {ts}", "content": "Discussed proposal terms and timeline",
         "noteType": 1, "visibility": 1,
         "entityType": "Account", "entityId": acct_ids[0] if acct_ids else 1},
        {"title": f"Call Summary {ts}", "content": "John confirmed interest in Enterprise tier",
         "noteType": 2, "visibility": 2,
         "entityType": "Contact", "entityId": contact_ids[0] if contact_ids else 1},
    ]
    note_ids = []
    for n in notes:
        eid = api.create_and_track("notes", "/api/notes", n)
        if eid:
            note_ids.append(eid)
    api.get("/api/notes")
    if note_ids:
        api.get(f"/api/notes/{note_ids[0]}")
        api.put(f"/api/notes/{note_ids[0]}", {**notes[0], "id": note_ids[0], "content": "Updated meeting notes with action items"})
    # Delete test
    del_n = {"title": f"DELETE-Note-{ts}", "content": "To be deleted", "noteType": 0,
             "visibility": 0, "entityType": "Account", "entityId": acct_ids[0] if acct_ids else 1}
    code, body, _ = api.post("/api/notes", del_n)
    if body and isinstance(body, dict) and body.get("id"):
        api.delete(f"/api/notes/{body['id']}")
    save_ids("notes", note_ids)

    # ---- Conversations ----
    log.section("Conversations")
    api.get("/api/conversations")

    print(f"  Batch 05 done: {log.summary_line()}")
