#!/usr/bin/env python3
import sys
import os

base = "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Infrastructure/deployment-tool"
routes = os.path.join(base, "gui/routes/day2_routes.py")
frag = os.path.join(base, "_component_routes_fragment.py")

existing = open(routes).read()
if "day2_components_status" in existing:
    print("Already present, skipping")
    sys.exit(0)

fragment = open(frag).read()
with open(routes, "a") as f:
    f.write("\n")
    f.write(fragment)
print(f"Appended {len(fragment)} chars to day2_routes.py")
