#!/usr/bin/env bash
# ===========================================================================
# CRM Test Data Loader — Run & Analyze
#
# Runs the full data-loader suite then analyzes the results, printing a
# succinct pass/fail summary to stdout and writing detailed error info
# to logs/error_details.log.
#
# Usage:
#   ./run_tests.sh                                  # default endpoint
#   ./run_tests.sh http://localhost:5000             # custom endpoint
#   ./run_tests.sh http://192.168.0.9:5000 --skip 13
# ===========================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LOG_DIR="$SCRIPT_DIR/logs"
JSONL="$LOG_DIR/latest.jsonl"
TEXT_LOG="$LOG_DIR/latest.log"
ERROR_LOG="$LOG_DIR/error_details.log"

BASE_URL="${1:-http://192.168.0.9:5000}"
# Shift off the base-url arg so remaining args pass through
shift 2>/dev/null || true

mkdir -p "$LOG_DIR"

# ── 1. Run the data loader ──────────────────────────────────────────────
echo "════════════════════════════════════════════════════════════════"
echo "  CRM Test Data Loader"
echo "  Target : $BASE_URL"
echo "  Time   : $(date '+%Y-%m-%d %H:%M:%S')"
echo "════════════════════════════════════════════════════════════════"
echo ""

python3 "$SCRIPT_DIR/run_all_batches.py" --base-url "$BASE_URL" "$@"
LOADER_EXIT=$?

echo ""

# ── 2. Analyze results ──────────────────────────────────────────────────
if [ ! -f "$JSONL" ]; then
    echo "ERROR: No JSONL log found at $JSONL"
    exit 1
fi

# Parse counts from the JSONL
TOTAL=$(python3 -c "
import json, sys
t=s=f=sk=ex=0
for line in open('$JSONL'):
    try: e=json.loads(line)
    except: continue
    if e.get('status') in ('success','exists','failed'): t+=1
    if e.get('status')=='success': s+=1
    if e.get('status')=='exists': ex+=1
    if e.get('status')=='failed': f+=1
    if e.get('status')=='skipped_integration': sk+=1
print(f'{t} {s} {f} {sk} {ex}')
")
read -r COUNT_TOTAL COUNT_OK COUNT_FAIL COUNT_SKIP COUNT_EXISTS <<< "$TOTAL"

# Build detailed error log
python3 -c "
import json
from collections import Counter, defaultdict

errors = defaultdict(list)
with open('$JSONL') as fh:
    for line in fh:
        try:
            e = json.loads(line)
        except:
            continue
        if e.get('status') != 'failed':
            continue
        code = e.get('http_status', '???')
        method = e.get('method', '?')
        ep = e.get('endpoint', '?')
        summary = e.get('summary', '')
        errors[code].append({'method': method, 'endpoint': ep, 'summary': summary})

if not errors:
    print('No errors.')
else:
    for code in sorted(errors, key=str):
        items = errors[code]
        print(f'══ HTTP {code} — {len(items)} failure(s) ══')
        seen = Counter(f\"{i['method']} {i['endpoint']}\" for i in items)
        for key, cnt in sorted(seen.items()):
            tag = f' (x{cnt})' if cnt > 1 else ''
            print(f'  {key}{tag}')
            # Print first matching summary for context
            for i in items:
                if f\"{i['method']} {i['endpoint']}\" == key and i['summary']:
                    print(f'    └─ {i[\"summary\"].strip()[:120]}')
                    break
        print()
" > "$ERROR_LOG"

# ── 3. Print succinct summary ───────────────────────────────────────────
echo "════════════════════════════════════════════════════════════════"
echo "  RESULTS SUMMARY"
echo "════════════════════════════════════════════════════════════════"
echo ""

if [ "$COUNT_FAIL" -eq 0 ]; then
    echo "  ✅  ALL PASSED   $COUNT_OK / $COUNT_TOTAL API calls succeeded"
else
    echo "  ❌  FAILURES     $COUNT_OK / $COUNT_TOTAL passed, $COUNT_FAIL failed"
fi

if [ "$COUNT_EXISTS" -gt 0 ]; then
    echo "  🔁  DUPLICATES   $COUNT_EXISTS already-exists (dedup rejected)"
fi

if [ "$COUNT_SKIP" -gt 0 ]; then
    echo "  ⏭   SKIPPED      $COUNT_SKIP integration-dependent endpoints"
fi

echo ""
echo "  Logs:"
echo "    Text log     : $TEXT_LOG"
echo "    JSONL log    : $JSONL"
echo "    Error details: $ERROR_LOG"
echo ""

if [ "$COUNT_FAIL" -gt 0 ]; then
    echo "── Error Details ──────────────────────────────────────────────"
    cat "$ERROR_LOG"
    echo "──────────────────────────────────────────────────────────────"
fi

echo ""
exit "$LOADER_EXIT"
