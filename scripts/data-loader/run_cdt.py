#!/usr/bin/env python3
"""
Comprehensive CRUD Tests (CDT) - CRM Solution
==============================================
Runs all 14 data-loader batch modules against the target API,
captures timing + status for every call, and generates a rich
standalone HTML report in logs/cdt-report-{timestamp}.html.

Usage:
    python3 run_cdt.py [--base-url http://192.168.0.9:5000] [--open]

Options:
    --base-url  Target CRM API base URL  (default: http://localhost:5000)
    --open      Open the HTML report in the default browser after run
    --email     Admin email              (default: admin@crm.local)
    --password  Admin password           (default: Admin@123)
    --output    Custom report output path
"""

from __future__ import annotations

import argparse
import importlib
import json
import os
import sys
import time
import traceback
from dataclasses import dataclass, field
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

# ─── Ensure data-loader directory is on sys.path ─────────────────────────────
SCRIPT_DIR = Path(__file__).resolve().parent
sys.path.insert(0, str(SCRIPT_DIR))

# ─── Result data classes ─────────────────────────────────────────────────────

@dataclass
class TestResult:
    method: str
    endpoint: str
    status: int | None
    elapsed_ms: float
    ok: bool
    skipped: bool = False
    skip_reason: str = ""
    error: str = ""
    payload_summary: str = ""

@dataclass
class BatchResult:
    batch_id: str
    name: str
    passed: int = 0
    failed: int = 0
    skipped: int = 0
    total_ms: float = 0.0
    tests: list[TestResult] = field(default_factory=list)
    error: str = ""

@dataclass
class CdtRunSummary:
    run_id: str
    base_url: str
    started_at: datetime
    finished_at: datetime | None = None
    batches: list[BatchResult] = field(default_factory=list)

    @property
    def total_passed(self) -> int:
        return sum(b.passed for b in self.batches)

    @property
    def total_failed(self) -> int:
        return sum(b.failed for b in self.batches)

    @property
    def total_skipped(self) -> int:
        return sum(b.skipped for b in self.batches)

    @property
    def total_tests(self) -> int:
        return self.total_passed + self.total_failed + self.total_skipped

    @property
    def elapsed_s(self) -> float:
        if self.finished_at:
            return (self.finished_at - self.started_at).total_seconds()
        return 0.0

    @property
    def pass_rate(self) -> float:
        total = self.total_passed + self.total_failed
        return (self.total_passed / total * 100) if total > 0 else 100.0

# ─── Intercepting loader_utils.CrmApiClient wrapper ──────────────────────────

class CdtInterceptor:
    """
    Wraps the CrmApiClient from loader_utils so every request is
    recorded as a TestResult inside the active BatchResult.
    """

    def __init__(self, api_client: Any, batch_result: BatchResult):
        self._api = api_client
        self._batch = batch_result

    def get(self, endpoint: str, **kwargs):
        return self._call("GET", endpoint, None, **kwargs)

    def post(self, endpoint: str, body: Any = None, **kwargs):
        return self._call("POST", endpoint, body, **kwargs)

    def put(self, endpoint: str, body: Any = None, **kwargs):
        return self._call("PUT", endpoint, body, **kwargs)

    def patch(self, endpoint: str, body: Any = None, **kwargs):
        return self._call("PATCH", endpoint, body, **kwargs)

    def delete(self, endpoint: str, **kwargs):
        return self._call("DELETE", endpoint, None, **kwargs)

    def _call(self, method: str, endpoint: str, body: Any = None, **kwargs):
        t0 = time.monotonic()
        try:
            # Determine which underlying method to call
            fn = getattr(self._api, method.lower())
            if body is not None:
                result = fn(endpoint, body, **kwargs)
            else:
                result = fn(endpoint, **kwargs)

            elapsed_ms = (time.monotonic() - t0) * 1000

            # loader_utils returns (status, data, error_msg) tuples
            status, _data, err = result if isinstance(result, tuple) else (result, None, "")
            status_code = int(status) if status else None

            ok = status_code is not None and 200 <= status_code < 300
            skipped = status_code in (0, None) or (err and "skip" in str(err).lower())

            tr = TestResult(
                method=method,
                endpoint=endpoint,
                status=status_code,
                elapsed_ms=elapsed_ms,
                ok=ok and not skipped,
                skipped=skipped,
                skip_reason=str(err) if skipped else "",
                error=str(err) if not ok and not skipped else "",
                payload_summary=_truncate_payload(body),
            )
        except Exception as exc:
            elapsed_ms = (time.monotonic() - t0) * 1000
            tr = TestResult(
                method=method,
                endpoint=endpoint,
                status=None,
                elapsed_ms=elapsed_ms,
                ok=False,
                error=str(exc),
                payload_summary=_truncate_payload(body),
            )

        self._record(tr)
        return (tr.status, None, tr.error) if not tr.ok else (tr.status, None, "")

    def _record(self, tr: TestResult):
        self._batch.tests.append(tr)
        self._batch.total_ms += tr.elapsed_ms
        if tr.skipped:
            self._batch.skipped += 1
        elif tr.ok:
            self._batch.passed += 1
        else:
            self._batch.failed += 1

        # Live progress indicator
        icon = "✓" if tr.ok else ("~" if tr.skipped else "✗")
        color = "\033[32m" if tr.ok else ("\033[33m" if tr.skipped else "\033[31m")
        reset = "\033[0m"
        http = f"HTTP {tr.status}" if tr.status else "ERR "
        print(
            f"  {color}{icon}{reset} {tr.method:<6} {http}  "
            f"{tr.elapsed_ms:>6.0f}ms  {tr.endpoint}"
        )


def _truncate_payload(body: Any, limit: int = 120) -> str:
    if body is None:
        return ""
    s = json.dumps(body, default=str) if not isinstance(body, str) else body
    return s[:limit] + ("…" if len(s) > limit else "")


# Batch module names (matches batch_01_*.py … batch_14_*.py)
BATCH_MODULES = [
    ("01", "batch_01_system_users_settings"),
    ("02", "batch_02_accounts_contacts"),
    ("03", "batch_03_leads_products"),
    ("04", "batch_04_opportunities_quotes_orders"),
    ("05", "batch_05_interactions_tasks"),
    ("06", "batch_06_campaigns_templates"),
    ("07", "batch_07_service_desk"),
    ("08", "batch_08_commissions_territories"),
    ("09", "batch_09_workflows_approvals"),
    ("10", "batch_10_ai_analytics_webhooks"),
    ("11", "batch_11_monitoring_config"),
    ("12", "batch_12_files_tags_misc"),
    ("13", "batch_13_integration_probes"),
    ("14", "batch_14_rules_workflows"),
]


def run_all_batches(base_url: str, email: str, password: str) -> CdtRunSummary:
    """Authenticate once, then run all 14 batches while intercepting API calls."""
    from loader_utils import CrmApiClient  # type: ignore

    run_id = datetime.now(timezone.utc).strftime("%Y%m%d_%H%M%S")
    summary = CdtRunSummary(
        run_id=run_id,
        base_url=base_url,
        started_at=datetime.now(timezone.utc),
    )

    print(f"\n{'='*70}")
    print(f"  CDT — Comprehensive CRUD Tests")
    print(f"  Target : {base_url}")
    print(f"  Run ID : {run_id}")
    print(f"{'='*70}\n")

    # Authenticate once
    print("Authenticating … ", end="", flush=True)
    api = CrmApiClient(base_url, email, password)
    print("OK\n")

    for batch_id, module_name in BATCH_MODULES:
        br = BatchResult(batch_id=batch_id, name=module_name.replace("_", " ").title())
        print(f"{'─'*70}")
        print(f"  Batch {batch_id}: {br.name}")
        print(f"{'─'*70}")
        t_start = time.monotonic()

        try:
            mod = importlib.import_module(module_name)
            # Wrap the real client
            interceptor = CdtInterceptor(api, br)
            # Each batch exposes a run(api) function
            if hasattr(mod, "run"):
                mod.run(interceptor)
            elif hasattr(mod, "main"):
                mod.main(interceptor)
            else:
                br.error = f"Module {module_name} has no run() or main() function"
        except Exception as exc:
            br.error = traceback.format_exc()
            print(f"  [BATCH ERROR] {exc}")

        br.total_ms = (time.monotonic() - t_start) * 1000
        summary.batches.append(br)

        total = br.passed + br.failed + br.skipped
        print(
            f"\n  → {br.passed}/{total} passed  "
            f"{br.failed} failed  {br.skipped} skipped  "
            f"({br.total_ms/1000:.1f}s)\n"
        )

    summary.finished_at = datetime.now(timezone.utc)
    return summary


# ─── HTML Report Generator ───────────────────────────────────────────────────

def generate_html_report(summary: CdtRunSummary, output_path: Path) -> Path:
    """Generate a beautiful self-contained HTML report from the CdtRunSummary."""

    pass_rate = summary.pass_rate
    rate_color = "#22c55e" if pass_rate >= 95 else ("#f59e0b" if pass_rate >= 75 else "#ef4444")

    # Build per-batch HTML sections
    batch_sections = "\n".join(_render_batch(b) for b in summary.batches)

    # Timeline sparkline (ASCII-style SVG bar chart)
    timeline_svg = _build_timeline_svg(summary)

    html = f"""<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>CDT Report — {summary.run_id}</title>
<style>
  *, *::before, *::after {{ box-sizing: border-box; }}
  :root {{
    --bg: #0f172a; --card: #1e293b; --card2: #162032;
    --border: #334155; --text: #e2e8f0; --muted: #94a3b8;
    --green: #22c55e; --red: #ef4444; --yellow: #f59e0b;
    --blue: #3b82f6; --purple: #a855f7; --orange: #f97316;
    --radius: 10px; --shadow: 0 4px 24px rgba(0,0,0,.4);
  }}
  body {{
    margin: 0; font-family: 'Inter', system-ui, sans-serif;
    background: var(--bg); color: var(--text); font-size: 14px; line-height: 1.5;
  }}
  a {{ color: var(--blue); text-decoration: none; }}
  h1, h2, h3 {{ margin: 0; font-weight: 700; }}

  /* ─── Header ─────────────────────────────── */
  .header {{
    background: linear-gradient(135deg, #1e3a5f 0%, #0f172a 60%);
    border-bottom: 1px solid var(--border);
    padding: 28px 40px 24px;
  }}
  .header-row {{ display: flex; align-items: center; gap: 16px; }}
  .logo {{
    width: 48px; height: 48px; border-radius: 12px;
    background: linear-gradient(135deg, #3b82f6, #6366f1);
    display: flex; align-items: center; justify-content: center;
    font-size: 22px; font-weight: 800; color: #fff;
  }}
  .header-title {{ font-size: 22px; letter-spacing: -.5px; }}
  .header-sub {{ margin-top: 2px; font-size: 12px; color: var(--muted); }}
  .header-meta {{
    display: flex; flex-wrap: wrap; gap: 24px;
    margin-top: 16px; padding-top: 16px;
    border-top: 1px solid rgba(255,255,255,.08);
    font-size: 12px; color: var(--muted);
  }}
  .meta-item span {{ font-weight: 600; color: var(--text); }}

  /* ─── Summary cards ──────────────────────── */
  .summary {{
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
    gap: 16px; padding: 28px 40px 0;
  }}
  .metric {{
    background: var(--card); border: 1px solid var(--border);
    border-radius: var(--radius); padding: 18px 20px;
    box-shadow: var(--shadow);
  }}
  .metric-value {{
    font-size: 36px; font-weight: 800; letter-spacing: -1px;
    line-height: 1;
  }}
  .metric-label {{ margin-top: 4px; font-size: 11px; color: var(--muted); text-transform: uppercase; letter-spacing: .5px; }}
  .metric-bar {{ margin-top: 12px; height: 4px; background: rgba(255,255,255,.1); border-radius: 99px; overflow: hidden; }}
  .metric-bar-fill {{ height: 100%; border-radius: 99px; transition: width .8s ease; }}

  /* ─── Pass-rate ring ─────────────────────── */
  .pass-ring-card {{
    background: var(--card); border: 1px solid var(--border);
    border-radius: var(--radius); padding: 18px 20px;
    display: flex; align-items: center; gap: 20px;
    box-shadow: var(--shadow);
  }}
  .ring-wrap {{ position: relative; width: 72px; height: 72px; flex-shrink: 0; }}
  .ring-wrap svg {{ transform: rotate(-90deg); }}
  .ring-pct {{
    position: absolute; inset: 0; display: flex;
    align-items: center; justify-content: center;
    font-size: 13px; font-weight: 800; color: {rate_color};
  }}
  .ring-info h3 {{ font-size: 20px; color: {rate_color}; }}
  .ring-info p {{ font-size: 11px; color: var(--muted); margin-top: 4px; }}

  /* ─── Timeline card ──────────────────────── */
  .timeline-card {{
    background: var(--card); border: 1px solid var(--border);
    border-radius: var(--radius); padding: 18px 20px 10px;
    box-shadow: var(--shadow);
  }}
  .timeline-label {{ font-size: 11px; color: var(--muted); text-transform: uppercase; letter-spacing: .5px; margin-bottom: 10px; }}

  /* ─── Toolbar ────────────────────────────── */
  .toolbar {{
    display: flex; gap: 10px; align-items: center; flex-wrap: wrap;
    padding: 24px 40px 0;
  }}
  .btn {{
    padding: 6px 14px; border-radius: 6px; border: 1px solid var(--border);
    background: var(--card); color: var(--text); font-size: 12px;
    cursor: pointer; font-weight: 500; transition: all .15s;
  }}
  .btn:hover {{ background: #2d3f55; border-color: #4b6280; }}
  .btn.active {{ background: #1d4ed8; border-color: #3b82f6; color: #fff; }}
  .search-box {{
    flex: 1; min-width: 200px; max-width: 360px;
    padding: 6px 12px; border-radius: 6px;
    border: 1px solid var(--border); background: var(--card);
    color: var(--text); font-size: 12px;
  }}
  .search-box:focus {{ outline: none; border-color: var(--blue); }}

  /* ─── Batches ────────────────────────────── */
  .batches {{ padding: 24px 40px 48px; }}
  .batch-card {{
    background: var(--card2); border: 1px solid var(--border);
    border-radius: var(--radius); margin-bottom: 16px;
    box-shadow: var(--shadow); overflow: hidden;
  }}
  .batch-header {{
    display: flex; align-items: center; gap: 14px;
    padding: 14px 18px; cursor: pointer;
    border-bottom: 1px solid transparent;
    transition: background .15s;
  }}
  .batch-header:hover {{ background: rgba(255,255,255,.03); }}
  .batch-header.expanded {{ border-bottom-color: var(--border); }}
  .batch-num {{
    min-width: 28px; height: 28px; border-radius: 8px;
    background: linear-gradient(135deg, #1d4ed8, #6366f1);
    display: flex; align-items: center; justify-content: center;
    font-size: 11px; font-weight: 800; color: #fff;
  }}
  .batch-name {{ flex: 1; font-weight: 600; font-size: 13px; }}
  .batch-meta {{ display: flex; gap: 10px; align-items: center; font-size: 12px; color: var(--muted); }}
  .chip {{
    padding: 2px 8px; border-radius: 99px; font-size: 11px; font-weight: 600;
    white-space: nowrap;
  }}
  .chip-pass  {{ background: rgba(34,197,94,.15);  color: var(--green); }}
  .chip-fail  {{ background: rgba(239,68,68,.15);  color: var(--red);   }}
  .chip-skip  {{ background: rgba(245,158,11,.15); color: var(--yellow); }}
  .chip-info  {{ background: rgba(59,130,246,.12); color: var(--blue);   }}
  .batch-progress {{
    height: 3px; background: rgba(255,255,255,.06);
    display: flex;
  }}
  .prog-pass {{ background: var(--green); }}
  .prog-fail {{ background: var(--red);   }}
  .prog-skip {{ background: var(--yellow); }}
  .batch-error {{
    margin: 0 18px 14px;
    padding: 10px 14px; border-radius: 6px;
    background: rgba(239,68,68,.08); border: 1px solid rgba(239,68,68,.2);
    font-size: 12px; color: #fca5a5; font-family: monospace; white-space: pre-wrap;
    word-break: break-all;
  }}

  /* ─── Test table ─────────────────────────── */
  .test-table {{ width: 100%; border-collapse: collapse; }}
  .test-table th {{
    padding: 8px 14px; text-align: left; font-size: 11px;
    color: var(--muted); font-weight: 600; text-transform: uppercase;
    letter-spacing: .4px; background: rgba(0,0,0,.25);
    border-bottom: 1px solid var(--border);
  }}
  .test-table td {{
    padding: 7px 14px; font-size: 12px;
    border-bottom: 1px solid rgba(255,255,255,.04);
    vertical-align: middle;
  }}
  .test-row:last-child td {{ border-bottom: none; }}
  .test-row.fail   td {{ background: rgba(239,68,68,.04); }}
  .test-row.skip   td {{ opacity: .65; }}
  .test-row:hover  td {{ background: rgba(255,255,255,.025); }}

  .method-badge {{
    padding: 2px 6px; border-radius: 4px; font-size: 10px; font-weight: 700;
    font-family: monospace; white-space: nowrap; display: inline-block;
  }}
  .m-get    {{ background: rgba(59,130,246,.2); color: #93c5fd; }}
  .m-post   {{ background: rgba(34,197,94,.2);  color: #86efac; }}
  .m-put    {{ background: rgba(249,115,22,.2); color: #fdba74; }}
  .m-patch  {{ background: rgba(168,85,247,.2); color: #d8b4fe; }}
  .m-delete {{ background: rgba(239,68,68,.2);  color: #fca5a5; }}

  .status-badge {{
    padding: 2px 7px; border-radius: 4px; font-size: 11px; font-weight: 600;
    font-family: monospace;
  }}
  .s-2xx {{ background: rgba(34,197,94,.15);  color: var(--green); }}
  .s-3xx {{ background: rgba(59,130,246,.15); color: var(--blue);  }}
  .s-4xx {{ background: rgba(245,158,11,.15); color: var(--yellow); }}
  .s-5xx {{ background: rgba(239,68,68,.15);  color: var(--red);   }}
  .s-err {{ background: rgba(100,116,139,.15); color: var(--muted); }}

  .endpoint  {{ font-family: monospace; word-break: break-all; color: var(--text); }}
  .err-msg   {{ color: #fca5a5; font-size: 11px; white-space: pre-wrap; word-break: break-all; }}
  .skip-msg  {{ color: var(--yellow); font-size: 11px; }}
  .timing    {{ color: var(--muted); text-align: right; font-family: monospace; }}

  /* ─── Footer ─────────────────────────────── */
  .footer {{
    text-align: center; padding: 24px 40px; color: var(--muted);
    font-size: 11px; border-top: 1px solid var(--border);
  }}

  /* ─── Print / batch visibility toggles ───── */
  .batch-body {{ display: none; }}
  .batch-body.visible {{ display: block; }}
  .test-row.hidden {{ display: none; }}

  @media (max-width: 640px) {{
    .summary, .toolbar, .batches {{ padding-left: 16px; padding-right: 16px; }}
    .header {{ padding: 20px 16px 16px; }}
  }}
</style>
</head>
<body>

<!-- ─── Header ──────────────────────────────────────────────── -->
<header class="header">
  <div class="header-row">
    <div class="logo">C</div>
    <div>
      <div class="header-title">Comprehensive CRUD Tests (CDT)</div>
      <div class="header-sub">CRM Solution · Data Integrity & API Verification Suite</div>
    </div>
  </div>
  <div class="header-meta">
    <div class="meta-item">Run ID <span>{summary.run_id}</span></div>
    <div class="meta-item">Target <span>{summary.base_url}</span></div>
    <div class="meta-item">Started <span>{summary.started_at.strftime('%Y-%m-%d %H:%M:%S UTC')}</span></div>
    <div class="meta-item">Duration <span>{summary.elapsed_s:.1f}s</span></div>
    <div class="meta-item">Batches <span>{len(summary.batches)}</span></div>
  </div>
</header>

<!-- ─── Summary metrics ──────────────────────────────────────── -->
<section class="summary">

  {_ring_card(pass_rate, rate_color, summary.total_passed, summary.total_tests)}

  {_metric_card("Total Tests", summary.total_tests, "#3b82f6", 100)}
  {_metric_card("Passed", summary.total_passed, "#22c55e",
                summary.total_passed / max(summary.total_tests, 1) * 100)}
  {_metric_card("Failed", summary.total_failed, "#ef4444",
                summary.total_failed / max(summary.total_tests, 1) * 100)}
  {_metric_card("Skipped", summary.total_skipped, "#f59e0b",
                summary.total_skipped / max(summary.total_tests, 1) * 100)}
  {_metric_card("Duration", f"{summary.elapsed_s:.1f}s", "#a855f7", None)}

  <div class="timeline-card" style="grid-column: span 2;">
    <div class="timeline-label">Batch timing (ms)</div>
    {timeline_svg}
  </div>

</section>

<!-- ─── Toolbar ──────────────────────────────────────────────── -->
<div class="toolbar">
  <button class="btn active" onclick="filterAll()">All</button>
  <button class="btn" onclick="filterFailed()">Failures only</button>
  <button class="btn" onclick="expandAll()">Expand All</button>
  <button class="btn" onclick="collapseAll()">Collapse All</button>
  <input class="search-box" type="text" placeholder="Search endpoints…" oninput="doSearch(this.value)">
  <button class="btn" onclick="window.print()">Print / Save PDF</button>
</div>

<!-- ─── Batch sections ───────────────────────────────────────── -->
<div class="batches" id="batches">
{batch_sections}
</div>

<footer class="footer">
  CDT report generated by <strong>run_cdt.py</strong> · CRM Solution v0.614.80 ·
  {summary.finished_at.strftime('%Y-%m-%d %H:%M:%S UTC') if summary.finished_at else 'in progress'}
</footer>

<script>
// Toggle batch body
function toggleBatch(id) {{
  const body = document.getElementById('body-' + id);
  const hdr  = document.getElementById('hdr-'  + id);
  const vis  = body.classList.toggle('visible');
  hdr.classList.toggle('expanded', vis);
}}

// Filter helpers
function filterAll() {{
  document.querySelectorAll('.test-row').forEach(r => r.classList.remove('hidden'));
  document.querySelectorAll('.batch-body').forEach(b => b.classList.add('visible'));
  document.querySelectorAll('.batch-header').forEach(h => h.classList.add('expanded'));
  setActive('All');
}}
function filterFailed() {{
  document.querySelectorAll('.batch-body').forEach(b => b.classList.add('visible'));
  document.querySelectorAll('.batch-header').forEach(h => h.classList.add('expanded'));
  document.querySelectorAll('.test-row').forEach(r => {{
    r.classList.toggle('hidden', !r.classList.contains('fail'));
  }});
  setActive('Failures only');
}}
function expandAll()   {{ document.querySelectorAll('.batch-body').forEach(b => b.classList.add('visible')); }}
function collapseAll() {{ document.querySelectorAll('.batch-body').forEach(b => b.classList.remove('visible')); }}
function setActive(label) {{
  document.querySelectorAll('.toolbar .btn').forEach(b => b.classList.toggle('active', b.textContent === label));
}}

function doSearch(q) {{
  const lq = q.toLowerCase();
  document.querySelectorAll('.test-row').forEach(r => {{
    const ep = r.querySelector('.endpoint')?.textContent.toLowerCase() || '';
    r.classList.toggle('hidden', q && !ep.includes(lq));
  }});
  if (q) {{ expandAll(); }}
}}

// Auto-expand batches with failures
document.querySelectorAll('.batch-card').forEach(card => {{
  if (card.querySelector('.test-row.fail')) {{
    const id = card.dataset.batchId;
    document.getElementById('body-' + id).classList.add('visible');
    document.getElementById('hdr-'  + id).classList.add('expanded');
  }}
}});
</script>
</body>
</html>"""

    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(html, encoding="utf-8")
    return output_path


# ─── HTML helpers ─────────────────────────────────────────────────────────────

def _ring_card(pct: float, color: str, passed: int, total: int) -> str:
    r = 30
    circ = 2 * 3.14159 * r
    dash = circ * (pct / 100)
    return f"""
<div class="pass-ring-card">
  <div class="ring-wrap">
    <svg width="72" height="72" viewBox="0 0 72 72">
      <circle cx="36" cy="36" r="{r}" fill="none" stroke="rgba(255,255,255,.08)" stroke-width="6"/>
      <circle cx="36" cy="36" r="{r}" fill="none" stroke="{color}" stroke-width="6"
              stroke-dasharray="{dash:.1f} {circ:.1f}" stroke-linecap="round"/>
    </svg>
    <div class="ring-pct">{pct:.0f}%</div>
  </div>
  <div class="ring-info">
    <h3>Pass Rate</h3>
    <p>{passed} / {total} tests passed</p>
  </div>
</div>"""


def _metric_card(label: str, value: Any, color: str, pct: float | None) -> str:
    bar = ""
    if pct is not None:
        bar = f"""
    <div class="metric-bar">
      <div class="metric-bar-fill" style="width:{pct:.1f}%;background:{color}"></div>
    </div>"""
    return f"""
<div class="metric">
  <div class="metric-value" style="color:{color}">{value}</div>
  <div class="metric-label">{label}</div>
  {bar}
</div>"""


def _build_timeline_svg(summary: CdtRunSummary) -> str:
    if not summary.batches:
        return ""
    max_ms = max((b.total_ms for b in summary.batches), default=1) or 1
    bars = []
    for b in summary.batches:
        ht = max(4, int(b.total_ms / max_ms * 60))
        clr = "#ef4444" if b.failed > 0 else "#22c55e"
        bars.append(
            f'<div title="Batch {b.batch_id}: {b.total_ms:.0f}ms" '
            f'style="flex:1;min-width:8px;height:{ht}px;background:{clr};'
            f'border-radius:4px 4px 0 0;align-self:flex-end">'
            f'</div>'
        )
    return f'<div style="display:flex;gap:4px;height:64px;align-items:flex-end">{"".join(bars)}</div>'


def _render_batch(b: BatchResult) -> str:
    bid = b.batch_id
    total = b.passed + b.failed + b.skipped
    pct_pass = b.passed / total * 100 if total else 100
    pct_fail = b.failed / total * 100 if total else 0
    pct_skip = b.skipped / total * 100 if total else 0

    header_chip = (
        f'<span class="chip chip-fail">{b.failed} failed</span>'
        if b.failed > 0 else
        f'<span class="chip chip-pass">All passed</span>'
    )

    rows = "".join(_render_test_row(t) for t in b.tests)

    error_block = ""
    if b.error:
        error_block = f'<pre class="batch-error">{_esc(b.error)}</pre>'

    return f"""
<div class="batch-card" data-batch-id="{bid}">
  <div class="batch-header" id="hdr-{bid}" onclick="toggleBatch('{bid}')">
    <div class="batch-num">{bid}</div>
    <div class="batch-name">{_esc(b.name)}</div>
    <div class="batch-meta">
      <span class="chip chip-pass">{b.passed}✓</span>
      <span class="chip chip-fail">{b.failed}✗</span>
      <span class="chip chip-skip">{b.skipped}~</span>
      <span style="color:var(--muted);font-size:11px">{b.total_ms/1000:.1f}s</span>
      {header_chip}
    </div>
    <span style="margin-left:auto;color:var(--muted);font-size:16px" class="expand-icon">▾</span>
  </div>
  <div class="batch-progress">
    <div class="prog-pass" style="width:{pct_pass:.1f}%"></div>
    <div class="prog-fail" style="width:{pct_fail:.1f}%"></div>
    <div class="prog-skip" style="width:{pct_skip:.1f}%"></div>
  </div>
  <div class="batch-body" id="body-{bid}">
    {error_block}
    <table class="test-table">
      <thead>
        <tr>
          <th style="width:70px">Method</th>
          <th style="width:80px">Status</th>
          <th>Endpoint</th>
          <th>Details</th>
          <th style="width:80px">Time</th>
        </tr>
      </thead>
      <tbody>
        {rows if rows else '<tr><td colspan="5" style="text-align:center;color:var(--muted);padding:24px">No tests recorded</td></tr>'}
      </tbody>
    </table>
  </div>
</div>"""


def _render_test_row(t: TestResult) -> str:
    row_cls = "fail" if not t.ok and not t.skipped else ("skip" if t.skipped else "pass")
    method_cls = {
        "GET": "m-get", "POST": "m-post", "PUT": "m-put",
        "PATCH": "m-patch", "DELETE": "m-delete"
    }.get(t.method.upper(), "m-get")

    if t.status is None:
        status_html = '<span class="status-badge s-err">ERR</span>'
    elif 200 <= t.status < 300:
        status_html = f'<span class="status-badge s-2xx">{t.status}</span>'
    elif 300 <= t.status < 400:
        status_html = f'<span class="status-badge s-3xx">{t.status}</span>'
    elif 400 <= t.status < 500:
        status_html = f'<span class="status-badge s-4xx">{t.status}</span>'
    else:
        status_html = f'<span class="status-badge s-5xx">{t.status}</span>'

    detail = ""
    if t.error:
        detail = f'<span class="err-msg">{_esc(t.error[:200])}</span>'
    elif t.skipped and t.skip_reason:
        detail = f'<span class="skip-msg">⏭ {_esc(t.skip_reason[:160])}</span>'
    elif t.payload_summary:
        detail = f'<span style="color:var(--muted);font-size:11px">{_esc(t.payload_summary)}</span>'

    timing_color = "#ef4444" if t.elapsed_ms > 2000 else ("#f59e0b" if t.elapsed_ms > 500 else "var(--muted)")

    return f"""
<tr class="test-row {row_cls}">
  <td><span class="method-badge {method_cls}">{t.method}</span></td>
  <td>{status_html}</td>
  <td class="endpoint">{_esc(t.endpoint)}</td>
  <td>{detail}</td>
  <td class="timing" style="color:{timing_color}">{t.elapsed_ms:.0f}ms</td>
</tr>"""


def _esc(s: str) -> str:
    return (s
        .replace("&", "&amp;")
        .replace("<", "&lt;")
        .replace(">", "&gt;")
        .replace('"', "&quot;"))


# ─── Main entry point ─────────────────────────────────────────────────────────

def main():
    parser = argparse.ArgumentParser(
        description="Comprehensive CRUD Tests (CDT) — run all batches & generate HTML report"
    )
    parser.add_argument("--base-url", default="http://localhost:5000",
                       help="CRM API base URL (default: http://localhost:5000)")
    parser.add_argument("--email", default="admin@crm.local")
    parser.add_argument("--password", default="Admin@123")
    parser.add_argument("--output", default=None,
                       help="Custom HTML output path")
    parser.add_argument("--open", action="store_true",
                       help="Open the report in the browser after generation")
    args = parser.parse_args()

    # Run all batches
    summary = run_all_batches(args.base_url, args.email, args.password)

    # Generate HTML report
    ts = summary.run_id
    if args.output:
        report_path = Path(args.output)
    else:
        logs_dir = SCRIPT_DIR / "logs"
        report_path = logs_dir / f"cdt-report-{ts}.html"

    out = generate_html_report(summary, report_path)
    latest = SCRIPT_DIR / "logs" / "cdt-report-latest.html"
    latest.parent.mkdir(parents=True, exist_ok=True)
    import shutil
    shutil.copy(out, latest)

    # Print summary
    print(f"\n{'='*70}")
    print(f"  CDT COMPLETE — {summary.total_passed}/{summary.total_tests} passed  "
          f"({summary.pass_rate:.1f}%)  {summary.elapsed_s:.1f}s")
    if summary.total_failed:
        print(f"  ⚠  {summary.total_failed} failures — see report for details")
    print(f"\n  Report: {out}")
    print(f"  Latest: {latest}")
    print(f"{'='*70}\n")

    # Also save JSON for API consumption
    json_path = SCRIPT_DIR / "logs" / f"cdt-result-{ts}.json"
    _save_json(summary, json_path)
    print(f"  JSON:   {json_path}")

    if args.open:
        import webbrowser
        webbrowser.open(out.as_uri())

    return 0 if summary.total_failed == 0 else 1


def _save_json(summary: CdtRunSummary, path: Path):
    """Save a machine-readable JSON summary (used by the frontend CDT tab)."""
    data = {
        "run_id": summary.run_id,
        "base_url": summary.base_url,
        "started_at": summary.started_at.isoformat(),
        "finished_at": summary.finished_at.isoformat() if summary.finished_at else None,
        "elapsed_s": summary.elapsed_s,
        "pass_rate": summary.pass_rate,
        "total_passed": summary.total_passed,
        "total_failed": summary.total_failed,
        "total_skipped": summary.total_skipped,
        "total_tests": summary.total_tests,
        "batches": [
            {
                "id": b.batch_id,
                "name": b.name,
                "passed": b.passed,
                "failed": b.failed,
                "skipped": b.skipped,
                "total_ms": b.total_ms,
                "error": b.error,
                "tests": [
                    {
                        "method": t.method,
                        "endpoint": t.endpoint,
                        "status": t.status,
                        "elapsed_ms": t.elapsed_ms,
                        "ok": t.ok,
                        "skipped": t.skipped,
                        "error": t.error,
                    }
                    for t in b.tests
                    if not t.ok or t.skipped  # Only failures/skips in JSON to keep it small
                ],
            }
            for b in summary.batches
        ],
    }
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, indent=2, default=str), encoding="utf-8")


if __name__ == "__main__":
    sys.exit(main())
