#!/usr/bin/env python3
import os, re

src = "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend/src"
for root, dirs, files in os.walk(src):
    for fname in files:
        if not fname.endswith(".cs"):
            continue
        path = os.path.join(root, fname)
        t = open(path, errors="ignore").read()
        if "class LLMSettingsDto" in t:
            ns = re.search(r"^namespace (.+);", t, re.MULTILINE)
            print(f"LLMSettingsDto in {ns.group(1) if ns else '?'}: {path}")
            idx = t.find("class LLMSettingsDto")
            print(t[idx:idx+500])
