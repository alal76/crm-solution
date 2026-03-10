#!/usr/bin/env python3
"""Audit entity/DTO definitions to find actual property names for test fixing."""
import os, re

src = "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend/src"

files_to_check = [
    "CRM.Core/Entities/Subscription.cs",
    "CRM.Core/Entities/LeadScoreRule.cs",
    "CRM.Core/Entities/Account.cs",
    "CRM.Core/Entities/Campaign.cs",
    "CRM.Core/Dtos/Webhooks/WebhookSubscriptionDto.cs",
    "CRM.Core/Dtos/Webhooks/CreateWebhookSubscriptionDto.cs",
    "CRM.Core/Dtos/AI/ChatMessageRequest.cs",
    "CRM.Core/Dtos/AI/BatchScoreRequest.cs",
    "CRM.Core/Dtos/Subscriptions/SubscriptionCreateRequest.cs",
    "CRM.Core/Dtos/ImportExport/ImportRequest.cs",
]

ctrl_files = [
    "CRM.Api/Controllers/WebhooksController.cs",
    "CRM.Api/Controllers/AIChatbotController.cs",
    "CRM.Api/Controllers/AILeadScoringController.cs",
    "CRM.Api/Controllers/DashboardConfigController.cs",
    "CRM.Api/Controllers/ImportExportController.cs",
    "CRM.Api/Controllers/SubscriptionsController.cs",
    "CRM.Api/Controllers/CampaignExecutionController.cs",
]

def find_file(rel_path):
    candidate = os.path.join(src, rel_path)
    if os.path.exists(candidate):
        return candidate
    # Try to find by filename
    name = os.path.basename(rel_path)
    for root, dirs, files in os.walk(src):
        if name in files:
            return os.path.join(root, name)
    return None

def show_props(filepath):
    if not filepath:
        return "  NOT FOUND"
    text = open(filepath).read()
    lines = []
    for line in text.split("\n"):
        stripped = line.strip()
        if re.match(r"public\s+(string|int|long|bool|decimal|double|float|DateTime|DateTimeOffset|Guid|\w+)\???\s+\w+\s*(;|{|=>)", stripped):
            lines.append("  " + stripped[:120])
    return "\n".join(lines[:30]) if lines else "  (no simple props found)"

def show_methods(filepath):
    if not filepath:
        return "  NOT FOUND"
    text = open(filepath).read()
    lines = []
    for line in text.split("\n"):
        stripped = line.strip()
        if re.match(r"public\s+(async\s+)?Task|public\s+(I|Action|OK|Bad)", stripped):
            lines.append("  " + stripped[:120])
    return "\n".join(lines[:20]) if lines else "  (no Task methods)"

print("===== ENTITY PROPERTIES =====")
for rel in files_to_check:
    path = find_file(rel)
    print(f"\n[{rel}] → {path}")
    print(show_props(path))

print("\n===== CONTROLLER METHODS =====")
for rel in ctrl_files:
    path = find_file(rel)
    print(f"\n[{rel}]")
    print(show_methods(path))
