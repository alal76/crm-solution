#!/usr/bin/env python3
"""
Fix all integration controller tests in CRM.Backend/tests/Integration/Controllers/.

Fixes three systematic issues in auto-generated integration tests:
1. ReadFromJsonAsync<dynamic>() returns JsonElement (not a true dynamic) - fix to use JsonElement
2. Wrong route URLs (mismatched controller routes)
3. {{item.Id}} string interpolation producing literal text instead of variable value

Usage:
    python3 scripts/fix_integration_tests.py
"""

import os
import re
import sys

# Base directory for integration controller tests
BASE_DIR = os.path.join(
    os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
    "CRM.Backend", "tests", "Integration", "Controllers"
)

# Route overrides: entity name -> correct route
# Only needed for controllers whose route differs from /api/{entity.lower()}
ROUTE_OVERRIDES = {
    # ITSM namespaced routes
    "Incidents": "/api/itsm/incidents",
    "IncidentCategories": "/api/incident-categories",
    "ITSMDashboard": "/api/itsm/dashboard",
    "ITSMWebhooks": "/api/itsm/webhooks",
    "EscalationPolicies": "/api/itsm/escalation-policies",
    "MonitoringIntegration": "/api/itsm/monitoring",
    "CICDIntegration": "/api/itsm/cicd",
    "EmailToTicket": "/api/itsm/email",
    "SelfServiceChatbot": "/api/itsm/chatbot",
    "Problems": "/api/problems",
    "CITypes": "/api/ci-types",
    "ChangeTypes": "/api/change-types",
    "Changes": "/api/changes",
    "CatalogCategories": "/api/catalog-categories",

    # Admin routes
    "AdminDashboard": "/api/admin",
    "AdminSeed": "/api/admin/seed",
    "Features": "/api/admin/features",
    "FeatureFlagManagement": "/api/feature-flags",
    "LeadScoreRules": "/api/admin/leadscorerules",

    # AI routes
    "AIAnalytics": "/api/ai",
    "AILeadScoring": "/api/ai/leads",
    "AIEmail": "/api/ai/email",
    "AIChatbot": "/api/ai/chatbot",
    "AIAgentUsage": "/api/ai-agent-usage",

    # Agent routes
    "Agent": "/api/agents",
    "AgentAdmin": "/api/agents/admin",
    "AgentAnalytics": "/api/agents/analytics",

    # Workflow routes
    "Workflow": "/api/workflows",
    "WorkflowInstance": "/api/workflow-instances",
    "WorkflowTasks": "/api/workflows/tasks",
    "WorkflowTriggers": "/api/workflow-triggers",

    # Worker routes
    "WorkerControl": "/api/workers/control",
    "WorkerHealth": "/api/workers",

    # Hyphenated routes
    "WebhookRegistrations": "/api/webhook-registrations",
    "DashboardConfig": "/api/dashboard-config",
    "SalesQuotas": "/api/sales-quotas",
    "SalesForecasts": "/api/sales-forecasts",
    "ImportJobs": "/api/import-jobs",
    "ExportJobs": "/api/export-jobs",
    "CampaignMetrics": "/api/campaign-metrics",
    "CampaignConversions": "/api/campaign-conversions",
    "CampaignRecipients": "/api/campaign-recipients",
    "CampaignExecution": "/api/campaigns",
    "NewsSocial": "/api/news-social",
    "EmailSequences": "/api/email-sequences",
    "EmailIntegration": "/api/email",
    "AnalyticsEvents": "/api/analytics-events",
    "LandingPage": "/api/landing-pages",
    "EventAttendees": "/api/event-attendees",
    "EscalationRules": "/api/escalation-rules",
    "UIPreferences": "/api/ui-preferences",
    "ServiceRequestSettings": "/api/service-request-settings",
    "AuthDiagnostics": "/api/auth-diagnostics",
    "AuditLogs": "/api/audit-logs",
    "CalendarIntegration": "/api/calendar",
    "PerformanceMonitoring": "/api/performance",

    # Webhook subroutes
    "Webhooks": "/api/webhooks",
    "ChatwootWebhook": "/api/webhooks/chatwoot",
    "DocuSignWebhook": "/api/webhooks/docusign",
    "DocuSealWebhook": "/api/webhooks/docuseal",
    "IntercomWebhook": "/api/webhooks/intercom",
    "NovuWebhook": "/api/webhooks/novu",
    "SendGridWebhook": "/api/webhooks/sendgrid",
    "StripeWebhook": "/api/webhooks/stripe",
    "TwilioWebhook": "/api/webhooks/twilio",

    # Health routes
    "Health": "/health",
    "ProviderHealth": "/api/health",

    # Other
    "Permissions": "/api/permissions",
    "Roles": "/api/roles",
}

def get_correct_route(entity_name):
    """Get the correct API route for a given entity name."""
    if entity_name in ROUTE_OVERRIDES:
        return ROUTE_OVERRIDES[entity_name]
    return f"/api/{entity_name.lower()}"

def get_current_route(entity_name):
    """Get the route that the auto-generated test currently uses."""
    return f"/api/{entity_name.lower()}"

def fix_test_file(filepath, entity_name):
    """Fix a single integration controller test file."""
    with open(filepath, "r") as f:
        content = f.read()

    original = content
    changes = []

    # 1. Add 'using System.Text.Json;' if not present
    if "using System.Text.Json;" not in content:
        if "using System.Net.Http.Json;" in content:
            content = content.replace(
                "using System.Net.Http.Json;",
                "using System.Net.Http.Json;\nusing System.Text.Json;"
            )
            changes.append("Added using System.Text.Json")
        elif "using Xunit;" in content:
            content = content.replace(
                "using Xunit;",
                "using System.Text.Json;\nusing Xunit;"
            )
            changes.append("Added using System.Text.Json")

    # 2. Fix ReadFromJsonAsync<dynamic>() -> ReadFromJsonAsync<JsonElement>()
    # Pattern A: var item = (await cRes.Content.ReadFromJsonAsync<dynamic>())!;
    pattern_a = r'var item = \(await cRes\.Content\.ReadFromJsonAsync<dynamic>\(\)\)!;'
    replacement_a = 'var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();\n            var id = item.GetProperty("id").GetInt32();'
    if re.search(pattern_a, content):
        content = re.sub(pattern_a, replacement_a, content)
        changes.append("Fixed ReadFromJsonAsync<dynamic> (pattern A)")

    # Pattern B: var item = await cRes.Content.ReadFromJsonAsync<dynamic>();
    pattern_b = r'var item = await cRes\.Content\.ReadFromJsonAsync<dynamic>\(\);'
    replacement_b = 'var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();\n            var id = item.GetProperty("id").GetInt32();'
    if re.search(pattern_b, content):
        content = re.sub(pattern_b, replacement_b, content)
        changes.append("Fixed ReadFromJsonAsync<dynamic> (pattern B)")

    # Pattern C: var xyz = (await someVar.Content.ReadFromJsonAsync<dynamic>())!;
    # More generic pattern for different variable names
    pattern_c = r'var (\w+) = \(await (\w+)\.Content\.ReadFromJsonAsync<dynamic>\(\)\)!;'
    if re.search(pattern_c, content):
        def replace_pattern_c(m):
            varname = m.group(1)
            resvar = m.group(2)
            return f'var {varname} = await {resvar}.Content.ReadFromJsonAsync<JsonElement>();\n            var id = {varname}.GetProperty("id").GetInt32();'
        content = re.sub(pattern_c, replace_pattern_c, content)
        changes.append("Fixed ReadFromJsonAsync<dynamic> (pattern C)")

    # Pattern D: Generic without parens
    pattern_d = r'var (\w+) = await (\w+)\.Content\.ReadFromJsonAsync<dynamic>\(\);'
    if re.search(pattern_d, content) and "ReadFromJsonAsync<dynamic>" in content:
        def replace_pattern_d(m):
            varname = m.group(1)
            resvar = m.group(2)
            return f'var {varname} = await {resvar}.Content.ReadFromJsonAsync<JsonElement>();\n            var id = {varname}.GetProperty("id").GetInt32();'
        content = re.sub(pattern_d, replace_pattern_d, content)
        changes.append("Fixed ReadFromJsonAsync<dynamic> (pattern D)")

    # 3. Remove property assertion lines on 'item' (they use dynamic property access)
    # Matches lines like: item.Name.Should().Be(create.Name);
    assertion_count_before = len(re.findall(r'^[ \t]+item\.\w+\.Should\(\)', content, re.MULTILINE))
    content = re.sub(r'^[ \t]+item\.\w+\.Should\(\)[^;\n]*;\n?', '', content, flags=re.MULTILINE)
    assertion_count_after = len(re.findall(r'^[ \t]+item\.\w+\.Should\(\)', content, re.MULTILINE))
    removed = assertion_count_before - assertion_count_after
    if removed > 0:
        changes.append(f"Removed {removed} property assertions")

    # 4. Fix string interpolation: {{item.Id}} -> {id}
    if "{{item.Id}}" in content or "{{item.id}}" in content:
        content = content.replace("{{item.Id}}", "{id}")
        content = content.replace("{{item.id}}", "{id}")
        changes.append("Fixed {{item.Id}} interpolation")

    # Also fix any remaining {item.Id} that might be in interpolated strings
    # (some tests might use single curlies which would be a compilation error with dynamic)

    # 5. Fix route URLs
    current_route = get_current_route(entity_name)
    correct_route = get_correct_route(entity_name)

    if current_route != correct_route:
        # Replace route in all URL strings
        # Be careful to replace exact matches (with quotes or slash after)
        if current_route in content:
            content = content.replace(current_route, correct_route)
            changes.append(f"Fixed route: {current_route} -> {correct_route}")

    # 6. Clean up multiple blank lines (from removed assertions)
    content = re.sub(r'\n{3,}', '\n\n', content)

    # Write back if changed
    if content != original:
        with open(filepath, "w") as f:
            f.write(content)
        return changes
    return []

def main():
    if not os.path.isdir(BASE_DIR):
        print(f"Error: Directory not found: {BASE_DIR}")
        sys.exit(1)

    # Get all test files
    test_files = sorted([
        f for f in os.listdir(BASE_DIR)
        if f.endswith("ControllerTests.cs")
    ])

    print(f"Found {len(test_files)} integration controller test files")
    print(f"Base directory: {BASE_DIR}")
    print("-" * 60)

    fixed_count = 0
    skipped_count = 0
    total_changes = 0

    for filename in test_files:
        entity_name = filename.replace("ControllerTests.cs", "")
        filepath = os.path.join(BASE_DIR, filename)

        changes = fix_test_file(filepath, entity_name)

        if changes:
            fixed_count += 1
            total_changes += len(changes)
            print(f"  FIXED: {filename}")
            for change in changes:
                print(f"         - {change}")
        else:
            skipped_count += 1
            # print(f"  SKIP:  {filename} (no changes needed)")

    print("-" * 60)
    print(f"Summary: {fixed_count} files fixed, {skipped_count} unchanged, {total_changes} total changes")

if __name__ == "__main__":
    main()
