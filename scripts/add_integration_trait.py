#!/usr/bin/env python3
"""Add [Trait("Category", "Integration")] to every IClassFixture<ApiTestFactory> class."""
import os
import re
import glob

base = "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend/tests"
files = (
    glob.glob(os.path.join(base, "Integration", "Controllers", "*.cs")) +
    glob.glob(os.path.join(base, "Integration", "*.cs"))
)

TRAIT = '[Trait("Category", "Integration")]'
# Match public class Xxx : ... IClassFixture<ApiTestFactory>
CLASS_RE = re.compile(r'public class (\w+)\s*:([^\n]*IClassFixture<ApiTestFactory>[^\n]*)')

changed = []
for path in files:
    with open(path, encoding="utf-8") as f:
        text = f.read()

    if "IClassFixture<ApiTestFactory>" not in text:
        continue
    if TRAIT in text:
        continue  # already tagged

    def add_trait(m):
        # Find indentation of 'public class'
        idx = m.start()
        line_start = text.rfind('\n', 0, idx) + 1
        indent = ""
        for c in text[line_start:]:
            if c in (' ', '\t'):
                indent += c
            else:
                break
        return f"{TRAIT}\n{indent}{m.group(0)}"

    new_text = CLASS_RE.sub(add_trait, text)
    if new_text != text:
        with open(path, "w", encoding="utf-8") as f:
            f.write(new_text)
        changed.append(os.path.relpath(path, base))

print(f"Modified {len(changed)} file(s):")
for name in sorted(changed):
    print(f"  {name}")
