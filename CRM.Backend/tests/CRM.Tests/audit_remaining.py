#!/usr/bin/env python3
"""Find remaining types: SubscriptionStatus enum, IAllenAIService.BatchScoreLeadsAsync, MarketingCampaign.CampaignType prop."""
import os, re

src = "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend/src"

# 1. SubscriptionStatus enum values
print("=== SubscriptionStatus enum ===")
for root, dirs, files in os.walk(src):
    for fname in files:
        if fname.endswith(".cs"):
            path = os.path.join(root, fname)
            t = open(path, errors="ignore").read()
            if "enum SubscriptionStatus" in t:
                idx = t.find("enum SubscriptionStatus")
                print(t[idx:idx+300])
                break

# 2. IAllenAIService BatchScoreLeadsAsync
print("\n=== IAllenAIService.BatchScoreLeadsAsync ===")
for root, dirs, files in os.walk(src):
    for fname in files:
        if fname.endswith(".cs"):
            path = os.path.join(root, fname)
            t = open(path, errors="ignore").read()
            if "interface IAllenAIService" in t or "BatchScoreLeadsAsync" in t:
                for line in t.split("\n"):
                    if "BatchScoreLeadsAsync" in line:
                        print(f"  {fname}: {line.strip()}")

# 3. MarketingCampaign entity full props
print("\n=== MarketingCampaign entity ===")
for root, dirs, files in os.walk(src):
    for fname in files:
        if fname == "MarketingCampaign.cs":
            path = os.path.join(root, fname)
            t = open(path, errors="ignore").read()
            print(f"File: {path}")
            ns = re.search(r"^namespace (.+);", t, re.MULTILINE)
            print(f"Namespace: {ns.group(1) if ns else '?'}")
            for line in t.split("\n")[:60]:
                s = line.strip()
                if "public " in s:
                    print("  " + s[:100])

# 4. Check Subscription entity for Startdate / Amount
print("\n=== Subscription entity key props ===")
sub_path = os.path.join(src, "CRM.Core/Entities/Subscription.cs")
if os.path.exists(sub_path):
    t = open(sub_path).read()
    for line in t.split("\n"):
        s = line.strip()
        if "public " in s and ("Start" in s or "Amount" in s or "Plan" in s or "Name" in s or "Status" in s or "Active" in s):
            print("  " + s[:100])

# 5. DashboardControllerTests - what methods does DashboardController have?
print("\n=== DashboardController methods ===")
dc_path = os.path.join(src, "CRM.Api/Controllers/DashboardController.cs")
if os.path.exists(dc_path):
    t = open(dc_path).read()
    for line in t.split("\n"):
        s = line.strip()
        if "public " in s and ("Task" in s or "IAction" in s):
            print("  " + s[:100])
