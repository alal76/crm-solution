#!/usr/bin/env python3
"""
AP-015 Fix: Cache feature flag at construction time in all 7 provider factories.
Eliminates per-call GetAwaiter().GetResult() blocking (thread-pool starvation risk).
Strategy: Read flag value synchronously from IConfiguration (already injected)
at constructor time. Factory is scoped = once per HTTP request.
"""

import re
import os

BASE = "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend/src/CRM.Infrastructure/Factories"

FACTORIES = [
    # (filename_base, feature_flag_constant, config_key_suffix)
    ("SearchProviderFactory",       "FeatureFlags.UseExternalSearch",       "UseExternalSearch"),
    ("ChatProviderFactory",         "FeatureFlags.UseExternalChat",         "UseExternalChat"),
    ("AIProviderFactory",           "FeatureFlags.UseExternalAI",           "UseExternalAI"),
    ("NotificationProviderFactory", "FeatureFlags.UseExternalNotifications", "UseExternalNotifications"),
    ("AnalyticsProviderFactory",    "FeatureFlags.UseExternalAnalytics",    "UseExternalAnalytics"),
    ("SignatureProviderFactory",    "FeatureFlags.UseExternalSignatures",   "UseExternalSignatures"),
    ("IntegrationProviderFactory",  "FeatureFlags.UseExternalIntegrations", "UseExternalIntegrations"),
]

NOSONAR = ("// NOSONAR S4462 -- synchronous IProviderFactory<T> interface; "
           "FeatureManager has no synchronous API // NOSONAR S4462 -- synchronous "
           "IProviderFactory<T> interface; FeatureManager has no synchronous API")


def fix_factory(factory_name, flag_constant, config_suffix):
    path = os.path.join(BASE, f"{factory_name}.cs")
    with open(path, "r") as f:
        content = f.read()

    original = content

    # 1. Add private readonly bool _useExternalProvider field after _logger field
    logger_field = f"    private readonly ILogger<{factory_name}> _logger;"
    if "private readonly bool _useExternalProvider;" not in content:
        content = content.replace(
            logger_field,
            logger_field + "\n    private readonly bool _useExternalProvider;"
        )
    else:
        print(f"  SKIPPED field (already present): {factory_name}")

    # 2. Add initialization in constructor after _logger assignment
    # The constructor ends with:
    #   _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    # }
    old_ctor_end = "        _logger = logger ?? throw new ArgumentNullException(nameof(logger));\n    }"
    new_ctor_end = (
        "        _logger = logger ?? throw new ArgumentNullException(nameof(logger));\n"
        "        // AP-015: Cache feature flag once per request scope; avoids per-call blocking on async flag check\n"
        f'        _useExternalProvider = _configuration.GetValue<bool>("FeatureManagement:{config_suffix}");\n'
        "    }"
    )
    if old_ctor_end in content and "_useExternalProvider = _configuration.GetValue" not in content:
        content = content.replace(old_ctor_end, new_ctor_end)
    elif "_useExternalProvider = _configuration.GetValue" in content:
        print(f"  SKIPPED ctor init (already present): {factory_name}")
    else:
        print(f"  WARNING: could not find ctor end pattern in {factory_name}")

    # 3. Replace both GetAwaiter().GetResult() blocking calls with cached value
    # Pattern (two lines):
    #   var useExternal = _featureManager.IsEnabledAsync(FeatureFlags.UseExternalXxx)
    #       .GetAwaiter().GetResult(); // NOSONAR ...
    blocking_pattern = re.compile(
        r'        var useExternal = _featureManager\.IsEnabledAsync\(' + re.escape(flag_constant) + r'\)\s*\n'
        r'            \.GetAwaiter\(\)\.GetResult\(\); ' + re.escape(NOSONAR)
    )
    replacement = "        var useExternal = _useExternalProvider;"

    new_content, count = blocking_pattern.subn(replacement, content)
    if count == 0:
        print(f"  WARNING: No blocking call patterns replaced in {factory_name}")
    elif count < 2:
        print(f"  WARNING: Only {count}/2 blocking patterns replaced in {factory_name}")
    else:
        print(f"  OK: Replaced {count} blocking calls in {factory_name}")
    content = new_content

    if content != original:
        with open(path, "w") as f:
            f.write(content)
        print(f"  WRITTEN: {factory_name}.cs")
    else:
        print(f"  NO CHANGE: {factory_name}.cs")


for factory_name, flag_constant, config_suffix in FACTORIES:
    print(f"\nProcessing {factory_name}...")
    fix_factory(factory_name, flag_constant, config_suffix)

print("\nDone. Run 'dotnet build' to verify.")
