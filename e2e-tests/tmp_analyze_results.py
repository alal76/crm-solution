import json
import sys
from collections import defaultdict, Counter

p = sys.argv[1] if len(sys.argv) > 1 else '/tmp/crm_playwright_full_chromium.jsonraw'
with open(p, encoding='utf-8', errors='ignore') as f:
    s = f.read()

s = s[s.find('{'):]
d = json.loads(s)

byfile = defaultdict(list)


def walk_suite(suite):
    for spec in suite.get('specs', []):
        file = spec.get('file', '')
        for t in spec.get('tests', []):
            for r in t.get('results', []):
                if r.get('status') == 'failed':
                    err = ''
                    er = r.get('error') or {}
                    if isinstance(er, dict):
                        err = er.get('message', '') or er.get('value', '')
                    if not err and r.get('errors'):
                        e = r['errors'][0]
                        err = e.get('message', '') if isinstance(e, dict) else str(e)
                    title = ' > '.join([*spec.get('titlePath', []), t.get('title', '')])
                    byfile[file].append((title, err))

    for child in suite.get('suites', []):
        walk_suite(child)


for s0 in d.get('suites', []):
    walk_suite(s0)

items = sorted(((f, len(v)) for f, v in byfile.items()), key=lambda x: x[1], reverse=True)
print('TOTAL_FAILS', sum(c for _, c in items))
print('TOTAL_FAIL_FILES', len(items))

for file, count in items[:12]:
    print(f'\n=== {file} {count} ===')
    c = Counter()
    for _, err in byfile[file]:
        e = (err or '').lower()
        k = 'other'
        if 'networkidle' in e:
            k = 'networkidle timeout'
        elif 'tobevisible' in e:
            k = 'visibility assertion'
        elif 'tohaveurl' in e or 'waitforurl' in e:
            k = 'url assertion/timeout'
        elif 'tocontain' in e:
            k = 'text assertion'
        elif 'tobetruthy' in e:
            k = 'truthy assertion'
        elif 'timeout' in e:
            k = 'timeout other'
        c[k] += 1
    print(dict(c))
    for title, err in byfile[file][:2]:
        one = (err or '').replace('\n', ' ')[:180]
        print(f'- {title}')
        print(f'  {one}')
