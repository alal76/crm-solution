#!/usr/bin/env python3
"""Script that appends the 3 component management endpoints to day2_routes.py"""
import os

routes_path = os.path.join(
    os.path.dirname(__file__), "gui", "routes", "day2_routes.py"
)

new_code = r"""

# ===========================================================================
# Pluggable Architecture — Component Management
# ===========================================================================

# Map each category → which OSS provider uses which Docker container name
_COMPONENT_CONTAINERS = {
    "search":       {"meilisearch": "crm-meilisearch"},
    "chat":         {"chatwoot": "crm-chatwoot", "rocketchat": "crm-rocketchat"},
    "notification": {"novu": "crm-novu"},
    "analytics":    {"superset": "crm-superset"},
    "signature":    {"docuseal": "crm-docuseal"},
    "ai":           {"ollama": "crm-ollama"},
    "integration":  {"n8n": "crm-n8n"},
}

# Feature-flag env-var name per category
_CATEGORY_FEATURE_FLAGS = {
    "search":       "FeatureManagement__UseExternalSearch",
    "chat":         "FeatureManagement__UseExternalChat",
    "notification": "FeatureManagement__UseExternalNotifications",
    "analytics":    "FeatureManagement__UseExternalAnalytics",
    "signature":    "FeatureManagement__UseExternalSignatures",
    "ai":           "FeatureManagement__UseExternalAI",
    "integration":  "FeatureManagement__UseExternalIntegrations",
}

# Env-var key prefix for each specific provider
_PROVIDER_ENV_PREFIXES = {
    "meilisearch":  "Providers__Search__Meilisearch__",
    "algolia":      "Providers__Search__Algolia__",
    "typesense":    "Providers__Search__Typesense__",
    "chatwoot":     "Providers__Chat__Chatwoot__",
    "intercom":     "Providers__Chat__Intercom__",
    "rocketchat":   "Providers__Chat__RocketChat__",
    "novu":         "Providers__Notifications__Novu__",
    "twilio":       "Providers__Notifications__Twilio__",
    "sendgrid":     "Providers__Notifications__SendGrid__",
    "superset":     "Providers__Analytics__Superset__",
    "powerbi":      "Providers__Analytics__PowerBI__",
    "metabase":     "Providers__Analytics__Metabase__",
    "docuseal":     "Providers__Signatures__DocuSeal__",
    "docusign":     "Providers__Signatures__DocuSign__",
    "ollama":       "Providers__AI__Ollama__",
    "openai":       "Providers__AI__OpenAI__",
    "azure_openai": "Providers__AI__AzureOpenAI__",
    "anthropic":    "Providers__AI__Anthropic__",
    "gemini":       "Providers__AI__Gemini__",
    "openrouter":   "Providers__AI__OpenRouter__",
    "n8n":          "Providers__Integrations__N8n__",
    "zapier":       "Providers__Integrations__Zapier__",
    "make":         "Providers__Integrations__Make__",
    "workato":      "Providers__Integrations__Workato__",
}

_CATEGORY_DISPLAY = {
    "search": "Search", "chat": "Chat", "notification": "Notifications",
    "analytics": "Analytics", "signature": "Signatures", "ai": "AI",
    "integration": "Integrations",
}


def _container_status(container_name: str) -> str:
    """Return 'running', 'stopped', 'not_found', or 'unknown'."""
    try:
        r = subprocess.run(
            ["docker", "inspect", "--format={{.State.Running}}", container_name],
            capture_output=True, text=True, timeout=5,
        )
        if r.returncode != 0:
            return "not_found"
        return "running" if r.stdout.strip() == "true" else "stopped"
    except Exception:
        return "unknown"


@day2_bp.route("/api/day2/components/status", methods=["GET"])
def day2_components_status():
    """Return current provider selections + config from the active profile,
    plus live container status for OSS providers."""
    profile_name = request.args.get("profile") or ""
    profile, pname = _resolve_profile(profile_name)

    providers = profile.get("providers", {
        "search": "builtin", "chat": "builtin", "notification": "builtin",
        "analytics": "builtin", "signature": "builtin", "ai": "ollama", "integration": "builtin",
    })
    provider_configs = profile.get("provider_configs", {})

    result = {}
    for cat in _CATEGORY_DISPLAY:
        selected = providers.get(cat, "builtin") or "builtin"
        container_map = _COMPONENT_CONTAINERS.get(cat, {})
        cname = container_map.get(selected)
        cstatus = _container_status(cname) if cname else None
        is_builtin = selected in ("builtin", "")
        result[cat] = {
            "selected": selected,
            "is_builtin": is_builtin,
            "container_name": cname,
            "container_status": cstatus,
            "config": provider_configs.get(cat, {}),
            "feature_flag": _CATEGORY_FEATURE_FLAGS.get(cat, ""),
            "enabled": not is_builtin,
        }
    return jsonify({"profile": pname, "components": result})


@day2_bp.route("/api/day2/components/configure", methods=["POST"])
def day2_components_configure():
    """Save provider selection + configuration (API keys, URLs) to the active profile."""
    body = request.get_json(force=True) or {}
    profile_name = body.get("profile_name") or request.args.get("profile") or ""
    category = (body.get("category") or "").strip()
    provider = (body.get("provider") or "").strip()
    cfg = body.get("config", {})

    if not category:
        return jsonify({"error": "category is required"}), 400

    profile, pname = _resolve_profile(profile_name)

    if "providers" not in profile:
        profile["providers"] = {}
    if provider:
        profile["providers"][category] = provider

    if "provider_configs" not in profile:
        profile["provider_configs"] = {}
    if category not in profile["provider_configs"]:
        profile["provider_configs"][category] = {}
    profile["provider_configs"][category].update(cfg)

    try:
        from core.profile import ProfileManager  # noqa: PLC0415
        pm = ProfileManager()
        pm.save(pname, profile)
    except Exception as exc:
        return jsonify({"error": f"Failed to save profile: {exc}"}), 500

    return jsonify({
        "success": True,
        "message": f"Configuration saved for '{category}' ({provider or 'no provider change'})",
    })


@day2_bp.route("/api/day2/components/apply", methods=["POST"])
def day2_components_apply():
    """Apply the profile's provider selections to the running CRM API.

    Writes a .env.providers override file with the correct feature-flags and
    provider-specific env-vars, then restarts crm-api via docker compose.
    """
    body = request.get_json(force=True) or {}
    profile_name = body.get("profile_name") or request.args.get("profile") or ""
    profile, pname = _resolve_profile(profile_name)

    providers = profile.get("providers", {})
    provider_configs = profile.get("provider_configs", {})

    env_lines = [
        "# Auto-generated by CRM CDT — Pluggable Architecture Provider Configuration",
        "# DO NOT EDIT MANUALLY — use the CDT Day-2 Components tab",
        "",
    ]

    for cat, display in _CATEGORY_DISPLAY.items():
        selected = providers.get(cat, "builtin") or "builtin"
        is_builtin = selected in ("builtin", "")
        flag_key = _CATEGORY_FEATURE_FLAGS.get(cat, "")

        env_lines.append(f"# {display} provider: {selected}")
        if flag_key:
            env_lines.append(f"{flag_key}={'false' if is_builtin else 'true'}")

        if not is_builtin:
            # Provider type key — TitleCase, underscores stripped
            type_val = selected.replace("_", "")
            type_val = type_val[0].upper() + type_val[1:]
            env_lines.append(f"Providers__{display}__Type={type_val}")

            cfg = provider_configs.get(cat, {})
            prefix = _PROVIDER_ENV_PREFIXES.get(selected, f"Providers__{display}__{type_val}__")
            for k, v in cfg.items():
                if v:
                    env_key = prefix + k[0].upper() + k[1:]
                    env_lines.append(f"{env_key}={v}")
        env_lines.append("")

    deploy_dir = profile.get("deploy_dir") or str(Path.home() / CDT_DIR / "deployments" / pname)
    env_providers_path = Path(deploy_dir) / ".env.providers"

    try:
        env_providers_path.parent.mkdir(parents=True, exist_ok=True)
        env_providers_path.write_text("\n".join(env_lines))
    except Exception as exc:
        return jsonify({"success": False, "error": f"Cannot write provider env file: {exc}"}), 500

    compose_file = Path(deploy_dir) / "docker-compose.yml"
    restart_output = ""
    restart_ok = True
    if compose_file.exists():
        try:
            r = subprocess.run(
                ["docker", "compose", "-f", str(compose_file),
                 "--env-file", str(env_providers_path), "restart", "crm-api"],
                capture_output=True, text=True, timeout=90,
            )
            restart_output = (r.stdout + r.stderr).strip()
            restart_ok = r.returncode == 0
        except subprocess.TimeoutExpired:
            restart_output = "Restart timed out (90 s). Check container status manually."
            restart_ok = False
        except Exception as exc:
            restart_output = str(exc)
            restart_ok = False
    else:
        restart_output = (
            "docker-compose.yml not found in deploy directory — "
            "env file written but no restart performed."
        )

    return jsonify({
        "success": restart_ok,
        "env_file": str(env_providers_path),
        "env_content": "\n".join(env_lines),
        "restart_output": restart_output,
        "message": (
            "Provider configuration applied and crm-api restarted." if restart_ok
            else f"Env file written but restart failed: {restart_output}"
        ),
    })
"""

# Guard: only append if the endpoint isn't already there
with open(routes_path) as f:
    existing = f.read()

if "day2_components_status" in existing:
    print("Endpoints already present, skipping.")
else:
    with open(routes_path, "a") as f:
        f.write(new_code)
    print(f"Appended {len(new_code)} chars to {routes_path}")
