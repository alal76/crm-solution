#!/usr/bin/env python3
"""Find remaining DTO/entity definitions."""
import os, re

src = "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend/src"

search_types = [
    "BatchScoreRequest",
    "InboundEmailDto",
    "WebhookSubscriptionDto",
    "CreateWebhookSubscriptionDto",
    "LlmSetting",
    "LLMSetting",
    "ChatMessageRequest",
]

for typename in search_types:
    for root, dirs, files in os.walk(src):
        for fname in files:
            if not fname.endswith(".cs"):
                continue
            path = os.path.join(root, fname)
            text = open(path, errors="ignore").read()
            if f"class {typename}" in text or f"record {typename}" in text:
                # Print namespace + properties
                ns_match = re.search(r"^namespace (.+);", text, re.MULTILINE)
                ns = ns_match.group(1) if ns_match else "(no ns)"
                props = []
                for line in text.split("\n"):
                    s = line.strip()
                    if re.match(r"public\s+(string|int|long|bool|decimal|double|\w+\??)\s+\w+\s*(;|{|=>)", s):
                        props.append("  " + s[:100])
                print(f"\n[{typename}] in {ns}")
                print(f"  File: {path}")
                print("\n".join(props[:20]))

# Check Campaign entity specifically
print("\n\n===== Campaign entity =====")
for root, dirs, files in os.walk(src):
    for fname in files:
        if fname == "Campaign.cs":
            path = os.path.join(root, fname)
            text = open(path, errors="ignore").read()
            ns_match = re.search(r"^namespace (.+);", text, re.MULTILINE)
            print(f"File: {path}")
            for line in text.split("\n"):
                s = line.strip()
                if ("Type" in s or "Status" in s) and "public" in s:
                    print("  " + s[:120])

# Check Subscription.Create() method signature in SubscriptionsController
print("\n\n===== SubscriptionsController.Create() signature =====")
sc_path = os.path.join(src, "CRM.Api/Controllers/SubscriptionsController.cs")
text = open(sc_path, errors="ignore").read()
idx = text.find("public async Task<ActionResult<Subscription>> Create(")
if idx >= 0:
    chunk = text[idx:idx+400]
    print(chunk[:400])

# Check ITSMWebhooksController
print("\n\n===== ITSMWebhooksController.cs =====")
for root, dirs, files in os.walk(src):
    for fname in files:
        if fname == "ITSMWebhooksController.cs":
            path = os.path.join(root, fname)
            text = open(path, errors="ignore").read()
            print(f"File: {path}")
            for line in text.split("\n")[:80]:
                s = line.strip()
                if "public" in s or "using" in s or "namespace" in s:
                    print("  " + s[:120])
