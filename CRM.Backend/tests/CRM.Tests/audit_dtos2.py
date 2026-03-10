#!/usr/bin/env python3
"""Find Campaign entity and SubscriptionCreateRequest."""
import os, re

src = "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend/src"

for root, dirs, files in os.walk(src):
    for fname in files:
        if not fname.endswith(".cs"):
            continue
        path = os.path.join(root, fname)
        text = open(path, errors="ignore").read()

        # Campaign entity
        if "MarketingCampaign" in fname or (fname == "Campaign.cs" and "Core" in path):
            print(f"=== {path} ===")
            for line in text.split("\n")[:120]:
                s = line.strip()
                if "public " in s and ("Type" in s or "Status" in s or "class" in s or "namespace" in s):
                    print("  " + s[:100])

        # SubscriptionCreateRequest
        if "class SubscriptionCreateRequest" in text:
            ns = re.search(r"^namespace (.+);", text, re.MULTILINE)
            print(f"\n=== SubscriptionCreateRequest in {ns.group(1) if ns else '?'} ===")
            print(f"File: {path}")
            in_class = False
            for line in text.split("\n"):
                s = line.strip()
                if "class SubscriptionCreateRequest" in s:
                    in_class = True
                if in_class:
                    print("  " + s[:100])
                if in_class and s == "}":
                    break

        # LLMSettings class / LLMSetting DbSet
        if "DbSet" in text and "LLM" in text:
            for line in text.split("\n"):
                s = line.strip()
                if "DbSet" in s and "LLM" in s and "public" in s:
                    print(f"LLMDbSet in {fname}: {s[:100]}")

# Find BatchScoreRequest specifically
print("\n=== BatchScoreRequest in AILeadScoringController ===")
ctrl = os.path.join(src, "CRM.Api/Controllers/AILeadScoringController.cs")
text = open(ctrl).read()
idx = text.find("class BatchScoreRequest")
if idx >= 0:
    print(text[idx:idx+200])
idx2 = text.find("class BatchScoreResponse")
if idx2 >= 0:
    print(text[idx2:idx2+200])

# Check AIChatbotController constructor for LLMSettings / LLMSetting
print("\n=== AIChatbotController constructor ===")
ctrl2 = os.path.join(src, "CRM.Api/Controllers/AIChatbotController.cs")
t2 = open(ctrl2).read()
idx3 = t2.find("public AIChatbotController(")
print(t2[idx3:idx3+300])

# Check what GetHistory returns (sync / async) and what the seed uses for LLM settings
print("\n=== AIChatbot GetHistory ===")
idx4 = t2.find("public IActionResult GetHistory")
print(t2[idx4:idx4+200])
