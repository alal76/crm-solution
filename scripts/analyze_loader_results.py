#!/usr/bin/env python3
"""Analyze test data loader results from JSONL log files."""
import json
import sys
import os
from collections import Counter

def analyze(jsonl_path):
    unknowns = []
    skips = []
    failures = []
    sections = []
    
    with open(jsonl_path) as f:
        for line in f:
            if not line.strip():
                continue
            d = json.loads(line)
            
            # Section markers have {"event": "section"} instead of {"status": ...}
            if d.get('event') == 'section':
                sections.append(d)
                continue
            
            status = d.get('status', 'unknown')
            
            if status == 'unknown':
                unknowns.append(d)
            elif status == 'skipped':
                skips.append(d)
            elif status == 'failed':
                failures.append(d)
    
    print("=" * 70)
    print(f"ANALYSIS OF: {os.path.basename(jsonl_path)}")
    print("=" * 70)
    
    # --- FAILURES ---
    print(f"\n{'='*30} FAILURES ({len(failures)}) {'='*30}")
    if failures:
        fail_groups = Counter()
        for f in failures:
            key = f"{f.get('method','?')} {f.get('endpoint','?')} -> {f.get('http_status','?')}"
            fail_groups[key] += 1
        for key, count in fail_groups.most_common():
            print(f"  [{count}x] {key}")
            # Show first example
            for f in failures:
                k = f"{f.get('method','?')} {f.get('endpoint','?')} -> {f.get('http_status','?')}"
                if k == key:
                    seed = os.path.basename(f.get('file','')) if f.get('file') else 'inline'
                    err = (f.get('error','') or '')[:150]
                    resp = (f.get('response_body','') or '')[:200]
                    print(f"        file: {seed}, index: {f.get('index','?')}")
                    print(f"        error: {err}")
                    if resp:
                        print(f"        response: {resp}")
                    break
    else:
        print("  None!")

    # --- UNKNOWNS ---
    print(f"\n{'='*30} UNKNOWNS ({len(unknowns)}) {'='*30}")
    if unknowns:
        unk_groups = Counter()
        for u in unknowns:
            key = f"{u.get('method','?')} {u.get('endpoint','?')} -> {u.get('http_status','?')}"
            unk_groups[key] += 1
        for key, count in unk_groups.most_common():
            print(f"  [{count}x] {key}")
            for u in unknowns:
                k = f"{u.get('method','?')} {u.get('endpoint','?')} -> {u.get('http_status','?')}"
                if k == key:
                    seed = os.path.basename(u.get('file','')) if u.get('file') else 'inline'
                    err = (u.get('error','') or '')[:150]
                    resp = (u.get('response_body','') or '')[:200]
                    req = u.get('request', {})
                    print(f"        file: {seed}, index: {u.get('index','?')}")
                    print(f"        error: {err}")
                    if resp:
                        print(f"        response: {resp}")
                    if req:
                        print(f"        request: {json.dumps(req)[:200]}")
                    break
    else:
        print("  None!")
    
    # --- SKIPS ---
    print(f"\n{'='*30} SKIPS ({len(skips)}) {'='*30}")
    if skips:
        skip_groups = Counter()
        for s in skips:
            seed = os.path.basename(s.get('file','')) if s.get('file') else 'inline'
            reason = (s.get('error','') or s.get('summary','') or 'no reason')[:80]
            skip_groups[(seed, reason)] += 1
        for (seed, reason), count in skip_groups.most_common():
            print(f"  [{count}x] {seed}: {reason}")
    else:
        print("  None!")
    
    print(f"\n{'='*70}")
    print("TOTALS:")
    # Count all statuses
    all_counts = Counter()
    section_count = 0
    with open(jsonl_path) as f:
        for line in f:
            if line.strip():
                d = json.loads(line)
                if d.get('event') == 'section':
                    section_count += 1
                else:
                    all_counts[d.get('status','unknown')] += 1
    for k, v in all_counts.most_common():
        print(f"  {k}: {v}")
    print(f"  sections: {section_count} (informational)")
    print(f"  TOTAL records: {sum(all_counts.values())}")

if __name__ == '__main__':
    # Find the latest JSONL
    log_dir = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), 'logs', 'test-data')
    jsonl_files = sorted([f for f in os.listdir(log_dir) if f.endswith('.jsonl')], reverse=True)
    if sys.argv[1:]:
        target = sys.argv[1]
    elif jsonl_files:
        target = os.path.join(log_dir, jsonl_files[0])
    else:
        print("No JSONL files found")
        sys.exit(1)
    analyze(target)
