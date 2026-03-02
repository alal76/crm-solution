#!/usr/bin/env python3
"""Flask GUI for CRM Deployment Configuration Wizard."""

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent))

# ---------------------------------------------------------------------------
# Prerequisite check — runs before any heavy imports.
# Core packages (Flask, PyYAML) must be present; optional SDKs are deferred.
# ---------------------------------------------------------------------------
from prerequisites import run_startup_check

if not run_startup_check(require_groups=["core"]):
    print("\n  Cannot start the GUI without required prerequisites.\n")
    sys.exit(1)

# --- Core imports (guaranteed present after the check above) ---------------
from flask import Flask, render_template, request, jsonify, session, Response
import json
import yaml
import secrets
import string
import os
import argparse

try:
    from flask_socketio import SocketIO
    _SOCKETIO_AVAILABLE = True
except ImportError:
    _SOCKETIO_AVAILABLE = False
    SocketIO = None

# --- Model imports (pure Python / stdlib — always safe) --------------------
from models.config_models import (
    TargetPlatform, DeploymentArchitecture, DatabaseType, ProviderStrategy,
    DeploymentType, SSLConfiguration, HostConfiguration, ServiceHosts
)
from models.platform_models import (
    AzureRegion, AWSRegion, GCPRegion,
    OnPremVirtualization, OnPremContainerRuntime, OnPremOrchestration,
    AzureVMSize, AzureDatabaseSKU, AzureRedisSKU,
    AWSInstanceType, AWSRDSInstanceClass, AWSElastiCacheNodeType,
    GCPMachineType, GCPCloudSQLTier,
    get_size_recommendation
)
from models.provider_models import (
    SEARCH_PROVIDERS, CHAT_PROVIDERS, NOTIFICATION_PROVIDERS,
    ANALYTICS_PROVIDERS, SIGNATURE_PROVIDERS, AI_PROVIDERS, INTEGRATION_PROVIDERS
)

# discovery_models no longer imports paramiko/requests at module level,
# so this import is safe even when those SDKs are not yet installed.
from models.discovery_models import (
    discovery_manager, DeploymentDiscoveryError, SSHConnectionError, CloudAPIError
)

app = Flask(__name__)
app.secret_key = os.environ.get('SECRET_KEY', secrets.token_hex(24))

# --- SocketIO (optional, required for live-deploy streaming) --------------
if _SOCKETIO_AVAILABLE:
    socketio = SocketIO(app, cors_allowed_origins="*", async_mode="threading")
else:
    socketio = None

# Wire the socket helpers so route modules can call emit_log() etc.
try:
    from core.socket_helpers import init_socketio as _init_sio
    _init_sio(socketio)
except ImportError:
    pass

# SocketIO event handlers
if socketio:
    from flask_socketio import join_room, leave_room  # noqa: PLC0415

    @socketio.on("join_deploy")
    def _on_join_deploy(data):
        """Client joins a deploy log room to receive live log events."""
        run_id = (data or {}).get("run_id", "")
        if run_id:
            join_room(f"deploy_{run_id}")

    @socketio.on("leave_deploy")
    def _on_leave_deploy(data):
        run_id = (data or {}).get("run_id", "")
        if run_id:
            leave_room(f"deploy_{run_id}")

# --- Register new blueprints (best-effort; skip on import error) ----------
try:
    from gui.routes.profile_routes import profile_bp
    app.register_blueprint(profile_bp)
except Exception as _e:  # noqa: BLE001
    print(f"[CDT] profile_bp not registered: {_e}")

try:
    from gui.routes.probe_routes import probe_bp
    app.register_blueprint(probe_bp)
except Exception as _e:
    print(f"[CDT] probe_bp not registered: {_e}")

try:
    from gui.routes.wizard_routes import wizard_bp
    app.register_blueprint(wizard_bp)
except Exception as _e:
    print(f"[CDT] wizard_bp not registered: {_e}")

try:
    from gui.routes.deploy_routes import deploy_bp
    app.register_blueprint(deploy_bp)
except Exception as _e:
    print(f"[CDT] deploy_bp not registered: {_e}")

try:
    from gui.routes.day2_routes import day2_bp
    app.register_blueprint(day2_bp)
except Exception as _e:
    print(f"[CDT] day2_bp not registered: {_e}")

try:
    from gui.routes.setup_routes import setup_bp
    app.register_blueprint(setup_bp)
except Exception as _e:
    print(f"[CDT] setup_bp not registered: {_e}")

try:
    from gui.routes.ssl_routes import ssl_bp
    app.register_blueprint(ssl_bp)
except Exception as _e:
    print(f"[CDT] ssl_bp not registered: {_e}")

try:
    from gui.routes.registry_routes import registry_bp
    app.register_blueprint(registry_bp)
except Exception as _e:
    print(f"[CDT] registry_bp not registered: {_e}")


@app.route('/health', methods=['GET'])
def health_check():
    """Liveness probe for Docker HEALTHCHECK and k8s probes."""
    from flask import jsonify
    return jsonify({"status": "ok", "service": "crm-cdt"}), 200

def generate_secure_password(length=16):
    """Generate a secure password with mixed characters."""
    chars = string.ascii_letters + string.digits + "!@#$%^&*"
    return ''.join(secrets.choice(chars) for _ in range(length))

def get_enum_choices(enum_cls):
    return [{"value": e.value, "label": e.value} for e in enum_cls]

def get_provider_choices(provider_dict):
    return [{"value": k, "label": v.display_name, "desc": v.description} for k, v in provider_dict.items()]

ENUM_MAP = {
    "target_platform": TargetPlatform,
    "deployment_architecture": DeploymentArchitecture,
    "database_type": DatabaseType,
    "provider_strategy": ProviderStrategy,
    "azure_region": AzureRegion,
    "aws_region": AWSRegion,
    "gcp_region": GCPRegion,
    "onprem_virtualization": OnPremVirtualization,
    "onprem_container_runtime": OnPremContainerRuntime,
    "onprem_orchestration": OnPremOrchestration,
}

PROVIDER_MAP = {
    "search": SEARCH_PROVIDERS,
    "chat": CHAT_PROVIDERS,
    "notification": NOTIFICATION_PROVIDERS,
    "analytics": ANALYTICS_PROVIDERS,
    "signature": SIGNATURE_PROVIDERS,
    "ai": AI_PROVIDERS,
    "integration": INTEGRATION_PROVIDERS,
}

@app.route("/", methods=["GET"])
def index():
    return render_template("index.html")

@app.route("/wizard", methods=["GET"])
def wizard():
    return render_template("wizard.html")

@app.route("/day2", methods=["GET"])
def day2_page():
    return render_template("day2.html")


@app.route("/profiles", methods=["GET"])
def profiles_page():
    return render_template("profiles.html")


@app.route("/api/enums/<enum_name>", methods=["GET"])
def get_enum(enum_name):
    if enum_name not in ENUM_MAP:
        return jsonify({"error": f"Unknown enum: {enum_name}"}), 404
    return jsonify(get_enum_choices(ENUM_MAP[enum_name]))

@app.route("/api/providers/<category>", methods=["GET"])
def get_providers(category):
    if category not in PROVIDER_MAP:
        return jsonify({"error": f"Unknown provider category: {category}"}), 404
    return jsonify(get_provider_choices(PROVIDER_MAP[category]))

@app.route("/api/size-recommendation/<int:users>", methods=["GET"])
def get_size_rec(users):
    rec = get_size_recommendation(users)
    return jsonify({
        "user_range": f"{rec.user_count_min}-{rec.user_count_max}",
        "description": rec.description,
        "azure": {
            "frontend_vm": rec.azure_frontend_vm.value,
            "api_vm": rec.azure_api_vm.value,
            "database_sku": rec.azure_database_sku.value,
            "redis_sku": rec.azure_redis_sku.value
        },
        "aws": {
            "frontend_instance": rec.aws_frontend_instance.value,
            "api_instance": rec.aws_api_instance.value,
            "rds_class": rec.aws_rds_class.value,
            "elasticache_type": rec.aws_elasticache_type.value
        },
        "gcp": {
            "frontend_machine": rec.gcp_frontend_machine.value,
            "api_machine": rec.gcp_api_machine.value,
            "cloudsql_tier": rec.gcp_cloudsql_tier.value
        }
    })

@app.route("/api/defaults", methods=["GET"])
def get_defaults():
    """Return smart defaults for container names, ports, URLs, and service mappings.

    Query params:
      - architecture: monolithic | microservices  (default: monolithic)
      - platform: on_premises | azure | aws | gcp (default: on_premises)
      - ssl: true | false (default: false)
      - host: hostname/IP for URL generation   (default: localhost)
    """
    arch = request.args.get("architecture", "monolithic")
    platform = request.args.get("platform", "on_premises")
    ssl_enabled = request.args.get("ssl", "false").lower() == "true"
    host = request.args.get("host", "localhost")

    protocol = "https" if ssl_enabled else "http"

    # ── Core services (always present) ──────────────────────────────
    core_services = {
        "crm-api":       {"port": 5000, "protocol": protocol, "description": ".NET Web API"},
        "crm-frontend":  {"port": 443 if ssl_enabled else 80, "protocol": protocol, "description": "React SPA (Nginx)"},
    }

    # ── Database services ───────────────────────────────────────────
    database_services = {
        "crm-mariadb":   {"port": 3306, "description": "MariaDB (primary)"},
        "crm-redis":     {"port": 6379, "description": "Redis cache & sessions"},
        "crm-postgres":  {"port": 5432, "description": "PostgreSQL (optional)"},
        "crm-sqlserver": {"port": 1433, "description": "SQL Server (optional)"},
    }

    # ── Provider / component services ───────────────────────────────
    provider_services = {
        "crm-meilisearch": {"port": 7700, "description": "Full-text search engine"},
        "crm-ollama":      {"port": 11434, "description": "Local LLM inference"},
        "crm-chatwoot":    {"port": 3000, "description": "Customer chat support"},
        "crm-novu":        {"port": 3001, "description": "Multi-channel notifications"},
        "crm-superset":    {"port": 8088, "description": "BI & data visualization"},
        "crm-docuseal":    {"port": 3002, "description": "E-signature workflows"},
        "crm-n8n":         {"port": 5678, "description": "Workflow automation"},
    }

    # ── Microservices (only in microservices architecture) ──────────
    microservices = {
        "crm-gateway":     {"port": 5000, "protocol": protocol, "description": "YARP API Gateway"},
        "crm-identity":    {"port": 5001, "protocol": protocol, "description": "Auth, Users, Groups"},
        "crm-customer":    {"port": 5002, "protocol": protocol, "description": "Accounts, Contacts"},
        "crm-sales":       {"port": 5003, "protocol": protocol, "description": "Opportunities, Quotes"},
        "crm-marketing":   {"port": 5004, "protocol": protocol, "description": "Campaigns, Leads"},
        "crm-servicedesk": {"port": 5005, "protocol": protocol, "description": "Tickets, Workflows"},
        "crm-core":        {"port": 5006, "protocol": protocol, "description": "Settings, Monitoring"},
    }

    # ── Docker networks ─────────────────────────────────────────────
    networks = {
        "crm_crm-network":        "Main unified network",
        "crm-core-network":       "Core stack isolation",
        "crm-db-network":         "Database stack isolation",
        "crm-components-network": "Components stack isolation",
    }

    # ── Registry defaults per platform ──────────────────────────────
    registry_defaults = {
        "on_premises":  {"registry": "registry.internal:5000", "org": "crm", "build_locally": True},
        "azure":        {"registry": "crmacr.azurecr.io",      "org": "crm", "build_locally": False},
        "aws":          {"registry": "<account>.dkr.ecr.<region>.amazonaws.com", "org": "crm", "build_locally": False},
        "gcp":          {"registry": "<region>-docker.pkg.dev/<project>", "org": "crm", "build_locally": False},
        "local_docker": {"registry": "",                        "org": "crm", "build_locally": True},
    }

    # ── Database defaults ───────────────────────────────────────────
    database_defaults = {
        "mariadb":    {"port": 3306, "container": "crm-mariadb",   "user": "crm_user", "db_name": "crm_db"},
        "mysql":      {"port": 3306, "container": "crm-mysql",     "user": "crm_user", "db_name": "crm_db"},
        "postgresql": {"port": 5432, "container": "crm-postgres",  "user": "crm_user", "db_name": "crm_db"},
        "sqlserver":  {"port": 1433, "container": "crm-sqlserver", "user": "sa",       "db_name": "crm_db"},
    }

    # ── Secret defaults (safe placeholder hints, not real secrets) ─
    secret_hints = {
        "db_user":     "crm_user",
        "db_name":     "crm_db",
        "admin_username": "admin",
        "admin_email":    "admin@crm.local",
        "jwt_issuer":     "CRM.Api",
        "jwt_audience":   "CRM.Client",
    }

    # ── Build URL helpers ───────────────────────────────────────────
    def build_url(_svc_name, svc_info):
        p = svc_info.get("protocol", protocol)
        port = svc_info["port"]
        # Omit port for standard HTTP/HTTPS
        if (p == "https" and port == 443) or (p == "http" and port == 80):
            return f"{p}://{host}"
        return f"{p}://{host}:{port}"

    # URLs for core access points
    urls = {
        "api_url":      build_url("crm-api", core_services["crm-api"]),
        "frontend_url": build_url("crm-frontend", core_services["crm-frontend"]),
    }
    if arch == "microservices":
        urls["gateway_url"] = build_url("crm-gateway", microservices["crm-gateway"])
        for ms_name, ms_info in microservices.items():
            urls[ms_name.replace("-", "_") + "_url"] = build_url(ms_name, ms_info)

    result = {
        "core_services": core_services,
        "database_services": database_services,
        "provider_services": provider_services,
        "microservices": microservices if arch == "microservices" else {},
        "networks": networks,
        "registry": registry_defaults.get(platform, registry_defaults["on_premises"]),
        "database_defaults": database_defaults,
        "secret_hints": secret_hints,
        "urls": urls,
        "protocol": protocol,
        "architecture": arch,
        "platform": platform,
    }
    return jsonify(result)


@app.route("/api/system/info", methods=["GET"])
def system_info():
    """Return build server environment info for the deployment details panel."""
    import platform as _platform
    import socket
    import subprocess as _sp

    # Build server hostname, OS & architecture
    hostname = socket.gethostname()
    machine_arch = _platform.machine()  # e.g. 'arm64', 'x86_64', 'aarch64'
    os_info = f"{_platform.system()} {_platform.release()} ({machine_arch})"
    python_ver = _platform.python_version()

    # Map machine arch to Docker platform string
    _arch_map = {"arm64": "linux/arm64", "aarch64": "linux/arm64", "x86_64": "linux/amd64", "amd64": "linux/amd64"}
    docker_platform = _arch_map.get(machine_arch.lower(), f"linux/{machine_arch}")

    # Docker version
    docker_ver = "not installed"
    try:
        r = _sp.run(["docker", "--version"], capture_output=True, text=True, timeout=5)
        if r.returncode == 0:
            docker_ver = r.stdout.strip()
    except Exception:
        pass

    # Docker Compose version
    compose_ver = "not installed"
    try:
        r = _sp.run(["docker", "compose", "version", "--short"], capture_output=True, text=True, timeout=5)
        if r.returncode == 0:
            compose_ver = r.stdout.strip()
    except Exception:
        pass

    # CRM solution version from version.json
    crm_version = "unknown"
    ver_file = Path(__file__).parent.parent.parent.parent / "version.json"
    if ver_file.exists():
        try:
            vj = json.loads(ver_file.read_text())
            crm_version = f"{vj.get('major', 0)}.{vj.get('minor', 0)}.{vj.get('patch', 0)}"
        except Exception:
            pass

    return jsonify({
        "build_server": {
            "hostname": hostname,
            "os": os_info,
            "machine_arch": machine_arch,
            "docker_platform": docker_platform,
            "python": python_ver,
            "docker": docker_ver,
            "docker_compose": compose_ver,
        },
        "crm_version": crm_version,
        "cdt_version": crm_version,
        "timestamp": __import__("datetime").datetime.now(__import__("datetime").timezone.utc).isoformat(),
    })


@app.route("/api/config", methods=["GET", "POST"])
def handle_config():
    if request.method == "GET":
        return jsonify(session.get("config", {}))
    session["config"] = request.json
    return jsonify({"status": "ok"})

@app.route("/api/generate", methods=["POST"])
def generate_files():
    config = request.json
    if not config:
        return jsonify({"status": "error", "message": "No configuration provided"}), 400

    output_dir = Path(__file__).parent.parent / "generated"
    output_dir.mkdir(exist_ok=True)
    
    # Generate deployment files based on platform
    generated_files = []
    
    try:
        if config.get('platform') == 'azure':
            # Generate Azure ARM templates or Bicep
            pass
        elif config.get('platform') == 'aws':
            # Generate CloudFormation
            pass
        elif config.get('platform') == 'gcp':
            # Generate Deployment Manager
            pass
        else:
            # Generate Docker Compose for on-premises
            docker_compose = generate_docker_compose(config)
            compose_file = output_dir / "docker-compose.yml"
            with open(compose_file, "w") as f:
                f.write(docker_compose)
            generated_files.append(str(compose_file))
            
            # Generate .env file
            env_content = generate_env_file(config)
            env_file = output_dir / ".env"
            with open(env_file, "w") as f:
                f.write(env_content)
            generated_files.append(str(env_file))
            
            # Generate deployment script
            script_content = generate_deployment_script(config)
            script_file = output_dir / "deploy.sh"
            with open(script_file, "w") as f:
                f.write(script_content)
            # Make script executable
            script_file.chmod(0o755)
            generated_files.append(str(script_file))
            
            # Generate Kubernetes manifests if needed
            if config.get('architecture') == 'microservices':
                k8s_content = generate_kubernetes(config)
                k8s_file = output_dir / "kubernetes.yml"
                with open(k8s_file, "w") as f:
                    f.write(k8s_content)
                generated_files.append(str(k8s_file))
    except ValueError as e:
        return jsonify({"status": "error", "message": str(e)}), 400
    except Exception as e:
        app.logger.exception("Failed to generate deployment files")
        return jsonify({"status": "error", "message": f"Generation failed: {e}"}), 500
    
    # Save config
    config_file = output_dir / "deployment-config.json"
    with open(config_file, "w") as f:
        json.dump(config, f, indent=2)
    generated_files.append(str(config_file))
    
    return jsonify({
        "status": "ok", 
        "message": "Deployment files generated successfully", 
        "files": generated_files,
        "output_dir": str(output_dir)
    })

@app.route("/api/discovery/platforms", methods=["GET"])
def get_discovery_platforms():
    """Get available discovery platforms with SDK availability and missing packages."""
    import importlib
    importlib.invalidate_caches()  # pick up any SDKs just installed via pip
    platforms = discovery_manager.get_available_platforms()
    availability = {}
    missing_packages: dict = {}
    _sdk_map = {
        'azure': ['azure-identity', 'azure-mgmt-compute'],  # azure-mgmt-containerinstance is optional (ACI discovery)
        'aws': ['boto3'],
        'gcp': ['google-cloud-compute', 'google-cloud-container'],
    }
    for platform in platforms:
        avail = discovery_manager.check_platform_availability(platform)
        if isinstance(avail, dict):
            availability[platform] = avail.get('available', bool(avail))
            missing_packages[platform] = avail.get('missing_packages', [])
        else:
            availability[platform] = bool(avail)
            missing_packages[platform] = [] if avail else _sdk_map.get(platform, [])
    return jsonify({
        "platforms": platforms,
        "availability": availability,
        "missing_packages": missing_packages,
    })

@app.route("/api/discovery/discover", methods=["POST"])
def discover_deployment():
    """Discover existing deployment."""
    try:
        config = request.json
        platform = config.get('platform')
        
        if not platform:
            return jsonify({"error": "Platform is required"}), 400
        
        # Discover deployment
        deployment_info = discovery_manager.discover_deployment(platform, config)
        
        # Convert to JSON-serializable format
        result = {
            "platform": deployment_info.platform,
            "architecture": deployment_info.architecture,
            "version": deployment_info.version,
            "environment": deployment_info.environment,
            "health_status": deployment_info.health_status,
            "last_checked": deployment_info.last_checked.isoformat() if deployment_info.last_checked else None,
            "components": []
        }
        
        for component in deployment_info.components:
            component_data = {
                "name": component.name,
                "type": component.type,
                "status": component.status,
                "version": component.version,
                "image": component.image,
                "ports": component.ports,
                "environment": component.environment,
                "health_url": component.health_url,
                "last_updated": component.last_updated.isoformat() if component.last_updated else None,
                "metadata": component.metadata
            }
            result["components"].append(component_data)
        
        return jsonify(result)
        
    except DeploymentDiscoveryError as e:
        return jsonify({"error": str(e)}), 400
    except Exception as e:
        return jsonify({"error": f"Discovery failed: {str(e)}"}), 500


@app.route("/api/discovery/credential-status", methods=["GET"])
def credential_status():
    """Check current CLI credential status for a cloud platform."""
    import subprocess as _sp
    import json as _json
    platform = request.args.get("platform", "")
    result = {"platform": platform, "logged_in": False, "identity": None, "error": None}
    try:
        _check_platform_credentials(platform, result, _sp, _json)
    except FileNotFoundError:
        result["error"] = "CLI not installed on this host — use manual credentials below."
    except Exception as e:
        result["error"] = str(e)
    return jsonify(result)


def _check_platform_credentials(platform, result, _sp, _json):
    """Populate result dict with credential information for the given platform."""
    if platform == "azure":
        _check_azure_credentials(result, _sp, _json)
    elif platform == "aws":
        _check_aws_credentials(result, _sp, _json)
    elif platform == "gcp":
        _check_gcp_credentials(result, _sp)


def _check_azure_credentials(result, _sp, _json):
    r = _sp.run(["az", "account", "show"], capture_output=True, text=True, timeout=10)
    if r.returncode == 0:
        d = _json.loads(r.stdout)
        user = d.get("user", {}).get("name", "?")
        sub  = d.get("name", "?")
        sid  = d.get("id", "?")[:8]
        result["logged_in"] = True
        result["identity"]  = f"{user}  ·  {sub}  ({sid}…)"
    else:
        result["error"] = "Not signed in — use 'az login' below or enter credentials manually."


def _check_aws_credentials(result, _sp, _json):
    r = _sp.run(["aws", "sts", "get-caller-identity"], capture_output=True, text=True, timeout=10)
    if r.returncode == 0:
        d = _json.loads(r.stdout)
        result["logged_in"] = True
        result["identity"]  = f"Account {d.get('Account', '?')}  ·  {d.get('Arn', '?')}"
    else:
        result["error"] = "No default credentials — configure below or enter keys manually."


def _check_gcp_credentials(result, _sp):
    r = _sp.run(["gcloud", "config", "get-value", "core/account"],
                capture_output=True, text=True, timeout=10)
    r2 = _sp.run(["gcloud", "config", "get-value", "core/project"],
                 capture_output=True, text=True, timeout=10)
    if r.returncode == 0 and r.stdout.strip():
        result["logged_in"] = True
        result["identity"]  = (
            f"{r.stdout.strip()}  ·  project: {r2.stdout.strip() or 'none'}"
        )
    else:
        result["error"] = "Not signed in — use 'gcloud auth login' below or enter a service account."


@app.route("/api/discovery/cloud-resources", methods=["GET"])
def cloud_resources():
    """List cloud resources for populating dropdowns (subscriptions, resource groups, regions, projects)."""
    import subprocess as _sp2
    import json as _json2
    platform = request.args.get("platform", "")
    resource = request.args.get("resource", "")
    sub_id   = request.args.get("subscription", "")
    data = {"items": [], "error": None}
    try:
        _fetch_cloud_resources(platform, resource, sub_id, data, _sp2, _json2)
    except FileNotFoundError:
        data["error"] = f"{platform.title()} CLI not installed — enter values manually."
    except Exception as exc:
        data["error"] = str(exc)
    return jsonify(data)


def _fetch_cloud_resources(platform, resource, sub_id, data, _sp2, _json2):
    """Populate data['items'] for the given platform/resource combination."""
    if platform == "azure":
        _fetch_azure_resources(resource, sub_id, data, _sp2, _json2)
    elif platform == "aws":
        _fetch_aws_resources(resource, data, _sp2, _json2)
    elif platform == "gcp":
        _fetch_gcp_resources(resource, data, _sp2, _json2)


def _fetch_azure_resources(resource, sub_id, data, _sp2, _json2):
    if resource == "subscriptions":
        r = _sp2.run(["az", "account", "list", "--output", "json"],
                     capture_output=True, text=True, timeout=20)
        if r.returncode == 0:
            data["items"] = [
                {"value": s["id"], "label": f"{s['name']} ({s['id'][:8]}…)"}
                for s in _json2.loads(r.stdout)
            ]
    elif resource == "resource_groups":
        cmd = ["az", "group", "list", "--subscription", sub_id, "--output", "json"] \
              if sub_id else ["az", "group", "list", "--output", "json"]
        r = _sp2.run(cmd, capture_output=True, text=True, timeout=20)
        if r.returncode == 0:
            data["items"] = [{"value": g["name"], "label": g["name"]}
                             for g in _json2.loads(r.stdout)]
    elif resource == "regions":
        r = _sp2.run(["az", "account", "list-locations", "--output", "json"],
                     capture_output=True, text=True, timeout=20)
        if r.returncode == 0:
            data["items"] = [
                {"value": loc["name"], "label": f"{loc['displayName']} ({loc['name']})"}
                for loc in _json2.loads(r.stdout)
            ]


def _fetch_aws_resources(resource, data, _sp2, _json2):
    if resource == "regions":
        r = _sp2.run(["aws", "ec2", "describe-regions", "--output", "json"],
                     capture_output=True, text=True, timeout=20)
        if r.returncode == 0:
            regions = sorted(
                reg["RegionName"]
                for reg in _json2.loads(r.stdout).get("Regions", [])
            )
            data["items"] = [{"value": reg, "label": reg} for reg in regions]
    elif resource == "vpcs":
        r = _sp2.run(["aws", "ec2", "describe-vpcs", "--output", "json"],
                     capture_output=True, text=True, timeout=20)
        if r.returncode == 0:
            vpcs = _json2.loads(r.stdout).get("Vpcs", [])
            data["items"] = [
                {"value": v["VpcId"],
                 "label": (
                     f"{next((t['Value'] for t in v.get('Tags', []) if t['Key']=='Name'), v['VpcId'])}"
                     f" ({v['VpcId']})"
                 )}
                for v in vpcs
            ]


def _fetch_gcp_resources(resource, data, _sp2, _json2):
    if resource == "projects":
        r = _sp2.run(["gcloud", "projects", "list", "--format=json"],
                     capture_output=True, text=True, timeout=20)
        if r.returncode == 0:
            data["items"] = [
                {"value": p["projectId"], "label": f"{p['name']} ({p['projectId']})"}
                for p in _json2.loads(r.stdout)
            ]
    elif resource == "regions":
        r = _sp2.run(["gcloud", "compute", "regions", "list", "--format=json"],
                     capture_output=True, text=True, timeout=20)
        if r.returncode == 0:
            data["items"] = [{"value": reg["name"], "label": reg["name"]}
                             for reg in _json2.loads(r.stdout)]


@app.route("/api/discovery/cloud-auth", methods=["POST"])
def cloud_auth():
    """SSE: run a cloud CLI auth command and stream its output."""
    import subprocess as _sp
    data     = request.json or {}
    platform = data.get("platform", "")
    method   = data.get("method", "")

    cmd_map = {
        ("azure", "cli"):        ["az", "login"],
        ("azure", "device"):     ["az", "login", "--use-device-code"],
        ("azure", "sp"):         None,   # SP creds are entered manually — nothing to run
        ("aws",   "configure"):  ["aws", "configure"],
        ("aws",   "sso"):        ["aws", "sso", "login"],
        ("aws",   "keys"):       None,   # keys entered manually
        ("gcp",   "login"):      ["gcloud", "auth", "login"],
        ("gcp",   "adc"):        ["gcloud", "auth", "application-default", "login"],
        ("gcp",   "sa"):         None,   # SA JSON pasted manually
    }
    cmd = cmd_map.get((platform, method))

    def _generate():
        if cmd is None:
            yield "data: Enter your credentials in the form below\n\n"
            yield "data: DONE\n\n"
            return
        yield f"data: $ {' '.join(cmd)}\n\n"
        try:
            proc = _sp.Popen(cmd, stdout=_sp.PIPE, stderr=_sp.STDOUT, text=True)
            for line in proc.stdout:
                yield f"data: {line.rstrip()}\n\n"
            proc.wait()
            if proc.returncode == 0:
                yield "data: ✅ Authentication successful\n\n"
            else:
                yield f"data: ❌ Exit code {proc.returncode}\n\n"
        except FileNotFoundError:
            yield "data: ❌ CLI not found — install it or use manual credentials below.\n\n"
        except Exception as exc:
            yield f"data: ❌ {exc}\n\n"
        yield "data: DONE\n\n"

    return Response(
        _generate(),
        mimetype="text/event-stream",
        headers={"Cache-Control": "no-cache", "X-Accel-Buffering": "no"},
    )


@app.route("/api/discovery/test-connection", methods=["POST"])
def test_connection():
    """Test connection to target environment."""
    try:
        config = request.json
        platform = config.get('platform')
        
        if not platform:
            return jsonify({"error": "Platform is required"}), 400
        
        if platform == 'on_premises':
            # Test SSH connection (frontend sends 'host', accept both)
            hostname = config.get('host') or config.get('hostname', '')
            if not hostname:
                return jsonify({"status": "failed", "message": "Host/IP address is required for on-premises connection"}), 400
            username = config.get('username', 'root') or 'root'
            password = config.get('password')
            key_path = config.get('key_path')
            port = int(config.get('port', 22) or 22)
            
            try:
                client = discovery_manager.clients[platform]()
                client.connect(hostname, username, password, key_path, port)
                client.disconnect()
                return jsonify({"status": "success", "message": f"SSH connection to {hostname} successful"})
            except Exception as e:
                return jsonify({"status": "failed", "message": f"SSH connection failed: {str(e)}"}), 400
                
        elif platform in ['azure', 'aws', 'gcp']:
            # Test cloud API connection using explicit credentials from the UI
            try:
                client = discovery_manager.clients[platform]()
                result = client.test_connection(config)
                http_code = 200 if result.get('status') == 'success' else 400
                return jsonify(result), http_code
            except Exception as exc:
                return jsonify({"status": "failed", "message": f"{platform.title()} connection failed: {str(exc)}"}), 400
        
        return jsonify({"status": "unknown", "message": f"Connection test not implemented for {platform}"}), 400
        
    except Exception as e:
        return jsonify({"error": f"Connection test failed: {str(e)}"}), 500

def _test_mysql_connection(host, port, username, password, database):
    """Return (success, message) for a MySQL/MariaDB connection attempt."""
    try:
        import pymysql  # type: ignore
        conn = pymysql.connect(host=host, port=port, user=username,
                               password=password, database=database or None,
                               connect_timeout=6)
        with conn.cursor() as cur:
            cur.execute("SELECT VERSION()")
            ver = cur.fetchone()[0]
        conn.close()
        return True, f"Connected — server version {ver}"
    except ImportError:
        return None, "pymysql not installed"
    except Exception as exc:
        return False, str(exc)


def _test_pg_connection(host, port, username, password, database):
    """Return (success, message) for a PostgreSQL connection attempt."""
    try:
        import psycopg2  # type: ignore
        conn = psycopg2.connect(host=host, port=port, user=username,
                                password=password, dbname=database or "postgres",
                                connect_timeout=6)
        ver  = conn.server_version
        conn.close()
        return True, f"Connected — PG server {ver // 10000}.{ver % 10000 // 100}"
    except ImportError:
        return None, "psycopg2 not installed"
    except Exception as exc:
        return False, str(exc)


def _test_sqlserver_connection(host, port, username, password, database):
    """Return (success, message) for a SQL Server connection attempt."""
    try:
        import pyodbc  # type: ignore
        cs = (f"DRIVER={{ODBC Driver 17 for SQL Server}};SERVER={host},{port};"
              f"DATABASE={database or 'master'};UID={username};PWD={password};"
              "Connection Timeout=6")
        conn = pyodbc.connect(cs, timeout=6)
        conn.close()
        return True, "Connected to SQL Server"
    except ImportError:
        return None, "pyodbc not installed"
    except Exception as exc:
        return False, str(exc)


@app.route("/api/discovery/test-migration-source", methods=["POST"])
def test_migration_source():
    """Test connectivity to a source database for the data-migration feature."""
    import socket
    try:
        data     = request.json or {}
        db_type  = data.get("db_type", "mariadb").lower()
        host     = (data.get("host") or "").strip()
        port     = int(data.get("port") or 3306)
        database = (data.get("database") or "").strip()
        username = (data.get("username") or "").strip()
        password = data.get("password") or ""

        if not host:
            return jsonify({"success": False, "message": "Host is required"}), 400

        # TCP reachability check
        try:
            sock = socket.create_connection((host, port), timeout=5)
            sock.close()
        except OSError as exc:
            return jsonify({"success": False,
                            "message": f"Cannot reach {host}:{port} — {exc}"}), 200

        # Driver-level checks
        driver_funcs = {
            "mariadb":    _test_mysql_connection,
            "mysql":      _test_mysql_connection,
            "postgresql": _test_pg_connection,
            "sqlserver":  _test_sqlserver_connection,
        }
        fn = driver_funcs.get(db_type)
        if fn:
            ok, msg = fn(host, port, username, password, database)
            if ok is False:
                return jsonify({"success": False, "message": msg}), 200
            if ok is True:
                return jsonify({"success": True, "message": msg})
            # ok is None → driver not installed, fall through

        return jsonify({"success": True,
                        "message": (f"TCP reachable at {host}:{port} "
                                    "(install Python DB driver for full verification)")})

    except Exception as exc:
        return jsonify({"success": False, "message": f"Unexpected error: {exc}"}), 500


POSTGRES_15_IMAGE = 'postgres:15'


def _add_provider_services(config, services, volumes, network_name, get_host_config, redis_host):
    """Populate services/volumes dict with optional provider containers."""
    if config.get('search_provider') == 'meilisearch':
        meili_host = get_host_config('meilisearch', 'localhost', 7700)
        services['crm-meilisearch'] = {
            'image': 'getmeili/meilisearch:v1.6',
            'container_name': 'crm-meilisearch',
            'restart': 'unless-stopped',
            'ports': [f"{meili_host['port']}:7700"],
            'environment': {'MEILI_MASTER_KEY': '${MEILI_MASTER_KEY}'},
            'volumes': ['meili_data:/meili_data'],
            'networks': [network_name],
        }
        volumes['meili_data'] = {'driver': 'local'}

    if config.get('chat_provider') == 'chatwoot':
        chatwoot_host = get_host_config('chatwoot', 'localhost', 3000)
        services['crm-chatwoot'] = {
            'image': 'chatwoot/chatwoot:latest',
            'container_name': 'crm-chatwoot',
            'restart': 'unless-stopped',
            'ports': [f"{chatwoot_host['port']}:3000"],
            'environment': {
                'INSTALLATION_ENV': 'docker',
                'SECRET_KEY_BASE': '${CHATWOOT_SECRET_KEY}',
                'POSTGRES_HOST': 'crm-chatwoot-postgres',
                'POSTGRES_USERNAME': 'chatwoot',
                'POSTGRES_PASSWORD': '${CHATWOOT_DB_PASSWORD}',
                'REDIS_URL': f"redis://crm-redis:{redis_host['port']}/1",
            },
            'depends_on': ['crm-chatwoot-postgres'],
            'networks': [network_name],
        }
        services['crm-chatwoot-postgres'] = _pg_service('chatwoot', 'crm-chatwoot-postgres', network_name)
        volumes['chatwoot_data'] = {'driver': 'local'}

    if config.get('notification_provider') == 'novu':
        novu_host = get_host_config('novu', 'localhost', 3001)
        services['crm-novu'] = {
            'image': 'novu/novu:latest',
            'container_name': 'crm-novu',
            'restart': 'unless-stopped',
            'ports': [f"{novu_host['port']}:3000"],
            'environment': {
                'NODE_ENV': 'production',
                'MONGO_URL': 'mongodb://crm-novu-mongo:27017/novu',
                'REDIS_URL': f"redis://crm-redis:{redis_host['port']}/2",
                'API_SECRET_KEY': '${NOVU_API_KEY}',
                'JWT_SECRET': '${NOVU_JWT_SECRET}',
            },
            'depends_on': ['crm-novu-mongo'],
            'networks': [network_name],
        }
        services['crm-novu-mongo'] = {
            'image': 'mongo:7', 'container_name': 'crm-novu-mongo',
            'restart': 'unless-stopped',
            'environment': {'MONGO_INITDB_DATABASE': 'novu'},
            'volumes': ['novu_data:/data/db'],
            'networks': [network_name],
        }
        volumes['novu_data'] = {'driver': 'local'}

    if config.get('analytics_provider') == 'superset':
        superset_host = get_host_config('superset', 'localhost', 8088)
        services['crm-superset'] = {
            'image': 'apache/superset:latest',
            'container_name': 'crm-superset',
            'restart': 'unless-stopped',
            'ports': [f"{superset_host['port']}:8088"],
            'environment': {
                'SUPERSET_SECRET_KEY': '${SUPERSET_SECRET_KEY}',
                'POSTGRES_DB': 'superset', 'POSTGRES_USER': 'superset',
                'POSTGRES_PASSWORD': '${SUPERSET_DB_PASSWORD}',
                'POSTGRES_HOST': 'crm-superset-postgres',
            },
            'depends_on': ['crm-superset-postgres'],
            'networks': [network_name],
        }
        services['crm-superset-postgres'] = _pg_service('superset', 'crm-superset-postgres', network_name)
        volumes['superset_data'] = {'driver': 'local'}

    if config.get('signature_provider') == 'docuseal':
        docuseal_host = get_host_config('docuseal', 'localhost', 3002)
        services['crm-docuseal'] = {
            'image': 'docuseal/docuseal:latest',
            'container_name': 'crm-docuseal',
            'restart': 'unless-stopped',
            'ports': [f"{docuseal_host['port']}:3000"],
            'environment': {
                'DATABASE_URL': (
                    "postgresql://docuseal:${DOCUSEAL_DB_PASSWORD}"
                    "@crm-docuseal-postgres:5432/docuseal"
                ),
                'SECRET_KEY_BASE': '${DOCUSEAL_SECRET_KEY}',
                'HOST': '0.0.0.0',
            },
            'depends_on': ['crm-docuseal-postgres'],
            'networks': [network_name],
        }
        services['crm-docuseal-postgres'] = _pg_service('docuseal', 'crm-docuseal-postgres', network_name)
        volumes['docuseal_data'] = {'driver': 'local'}

    if config.get('ai_provider') == 'ollama':
        ollama_host = get_host_config('ollama', 'localhost', 11434)
        services['crm-ollama'] = {
            'image': 'ollama/ollama:latest',
            'container_name': 'crm-ollama',
            'restart': 'unless-stopped',
            'ports': [f"{ollama_host['port']}:11434"],
            'volumes': ['ollama_data:/root/.ollama'],
            'networks': [network_name],
        }
        volumes['ollama_data'] = {'driver': 'local'}

    if config.get('integration_provider') == 'n8n':
        n8n_host = get_host_config('n8n', 'localhost', 5678)
        services['crm-n8n'] = {
            'image': 'n8nio/n8n:latest',
            'container_name': 'crm-n8n',
            'restart': 'unless-stopped',
            'ports': [f"{n8n_host['port']}:5678"],
            'environment': {
                'N8N_BASIC_AUTH_ACTIVE': 'true',
                'N8N_BASIC_AUTH_USER': '${N8N_USERNAME}',
                'N8N_BASIC_AUTH_PASSWORD': '${N8N_PASSWORD}',
                'N8N_ENCRYPTION_KEY': '${N8N_ENCRYPTION_KEY}',
                'DB_TYPE': 'postgresdb',
                'DB_POSTGRESDB_DATABASE': 'n8n',
                'DB_POSTGRESDB_HOST': 'crm-n8n-postgres',
                'DB_POSTGRESDB_PORT': '5432',
                'DB_POSTGRESDB_USER': 'n8n',
                'DB_POSTGRESDB_PASSWORD': '${N8N_DB_PASSWORD}',
            },
            'depends_on': ['crm-n8n-postgres'],
            'volumes': ['n8n_data:/home/node/.n8n'],
            'networks': [network_name],
        }
        services['crm-n8n-postgres'] = _pg_service('n8n', 'crm-n8n-postgres', network_name)
        volumes['n8n_data'] = {'driver': 'local'}


def _pg_service(db_name: str, container_name: str, network_name: str) -> dict:
    """Return a standard PostgreSQL 15 service definition."""
    return {
        'image': POSTGRES_15_IMAGE,
        'container_name': container_name,
        'restart': 'unless-stopped',
        'environment': {
            'POSTGRES_DB': db_name,
            'POSTGRES_USER': db_name,
            'POSTGRES_PASSWORD': f"${{{db_name.upper()}_DB_PASSWORD}}",
        },
        'volumes': [f"{db_name}_data:/var/lib/postgresql/data"],
        'networks': [network_name],
    }


def generate_docker_compose(config):
    """Generate docker-compose.yml from config."""
    import yaml
    
    network_topology = config.get('network_topology', 'single')
    expose_db = config.get('network_expose_db', False)
    expose_redis = config.get('network_expose_redis', False)
    
    # Build network names based on topology
    if network_topology == 'segmented':
        net_core = 'crm-core-network'
        net_data = 'crm-data-network'
        net_comp = 'crm-components-network'
        compose_networks = {
            net_core: {'driver': 'bridge'},
            net_data: {'driver': 'bridge'},
            net_comp: {'driver': 'bridge'},
        }
    elif network_topology == 'dmz':
        net_core = 'crm-dmz-network'      # public-facing (frontend, reverse proxy)
        net_data = 'crm-internal-network'  # private (API, DB, providers)
        net_comp = 'crm-internal-network'
        compose_networks = {
            net_core: {'driver': 'bridge'},
            net_data: {'driver': 'bridge'},
        }
    elif network_topology == 'host':
        net_core = None  # use network_mode: host
        net_data = None
        net_comp = None
        compose_networks = {}
    else:  # 'single'
        net_core = 'crm-network'
        net_data = 'crm-network'
        net_comp = 'crm-network'
        compose_networks = {
            'crm-network': {'driver': 'bridge'},
        }

    def _svc_networks(nets):
        """Return network list or network_mode for host topology."""
        if network_topology == 'host':
            return {}            # will add network_mode: host separately
        return {'networks': list(set(n for n in nets if n))}

    def _apply_host_mode(svc_dict):
        """For host topology, replace networks with network_mode: host."""
        if network_topology == 'host':
            svc_dict.pop('networks', None)
            svc_dict['network_mode'] = 'host'

    # Keep a single name for _add_provider_services compatibility
    provider_network_name = net_comp or 'crm-network'
    
    services = {}
    volumes = {}
    
    # Get host configurations from config or use defaults
    hosts = config.get('hosts', {})
    deployment_type = config.get('deployment_type', 'development')
    ssl_enabled = config.get('ssl_enabled', False)
    # ssl_config available via config.get('ssl_config') if needed in future

    # Resolve the deployment target host from the profile — NO localhost fallback.
    # This is the external IP/domain that browsers use to reach the CRM.
    _target = config.get('target', {})
    deployment_host = (
        _target.get('host')
        or _target.get('domain_name')
        or config.get('deployment_host')
        or config.get('host')
    )
    if not deployment_host:
        raise ValueError(
            "Deployment host is not configured in the profile. "
            "Set config.target.host in the wizard before generating."
        )
    api_port = str(_target.get('api_port', config.get('api_port', 5000)))
    frontend_port = str(_target.get('frontend_port', config.get('frontend_port', 80)))
    protocol = 'https' if ssl_enabled else 'http'
    
    # Helper function to get host config
    def get_host_config(service_name, default_host="localhost", default_port=80):
        host_config = hosts.get(service_name, {})
        return {
            'hostname': host_config.get('hostname', default_host),
            'port': host_config.get('port', default_port),
            'protocol': host_config.get('protocol', 'http'),
            'external_url': host_config.get('external_url'),
            'internal_only': host_config.get('internal_only', False)
        }
    
    # Database
    db_host = get_host_config('database', 'localhost', 3306)
    if config.get('database_type', 'mariadb') == 'mariadb':
        db_svc = {
            'image': 'mariadb:11',
            'container_name': 'crm-mariadb',
            'restart': 'unless-stopped',
            'environment': {
                'MARIADB_ROOT_PASSWORD': '${DB_ROOT_PASSWORD}',
                'MARIADB_DATABASE': config.get('database_name', 'crm_db'),
                'MARIADB_USER': 'crm_user',
                'MARIADB_PASSWORD': '${DB_PASSWORD}'
            },
            'volumes': ['db_data:/var/lib/mysql'],
            **_svc_networks([net_data]),
            'healthcheck': {
                'test': ["CMD", "healthcheck.sh", "--connect", "--innodb_initialized"],
                'interval': '10s',
                'timeout': '5s',
                'retries': 3
            }
        }
        if expose_db:
            db_svc['ports'] = [f"{db_host['port']}:3306"]
        _apply_host_mode(db_svc)
        services['crm-mariadb'] = db_svc
        volumes['db_data'] = {'driver': 'local'}
    
    # Redis
    redis_host = get_host_config('redis', 'localhost', 6379)
    redis_svc = {
        'image': 'redis:alpine',
        'container_name': 'crm-redis',
        'restart': 'unless-stopped',
        **_svc_networks([net_data])
    }
    if expose_redis:
        redis_svc['ports'] = [f"{redis_host['port']}:6379"]
    _apply_host_mode(redis_svc)
    services['crm-redis'] = redis_svc
    
    # API
    api_host = get_host_config('api', 'localhost', 5000)
    api_env = {
        'ASPNETCORE_ENVIRONMENT': 'Production' if deployment_type == 'production' else 'Development',
        'DatabaseProvider': config.get('database_type', 'mariadb'),
        'ConnectionStrings__DefaultConnection': f"Server=crm-mariadb;Port={db_host['port']};Database={config.get('database_name', 'crm_db')};User=crm_user;Password=${{DB_PASSWORD}}",
        'Jwt__Secret': '${JWT_SECRET}',
        'Redis__ConnectionString': f"crm-redis:{redis_host['port']}",
        'ADMIN_USERNAME': '${ADMIN_USERNAME}',
        'ADMIN_EMAIL': '${ADMIN_EMAIL}',
        'ADMIN_PASSWORD': '${ADMIN_PASSWORD}'
    }
    
    # Add SSL configuration if enabled
    if ssl_enabled:
        api_env.update({
            'ASPNETCORE_URLS': f"https://+:{api_host['port']};http://+:5000",
            'Kestrel__Certificates__Default__Path': '/app/ssl/server.pfx',
            'Kestrel__Certificates__Default__Password': '${SSL_CERT_PASSWORD}'
        })
    
    # Add provider environment variables to API
    if config.get('search_provider') == 'meilisearch':
        meili_host = get_host_config('meilisearch', 'localhost', 7700)
        api_env.update({
            'FeatureManagement__UseExternalSearch': 'true',
            'Providers__Search__Type': 'Meilisearch',
            'Providers__Search__Meilisearch__Url': f"http://{meili_host['hostname']}:{meili_host['port']}",
            'Providers__Search__Meilisearch__ApiKey': '${MEILI_MASTER_KEY}'
        })
    
    if config.get('chat_provider') == 'chatwoot':
        chatwoot_host = get_host_config('chatwoot', 'localhost', 3000)
        api_env.update({
            'FeatureManagement__UseExternalChat': 'true',
            'Providers__Chat__Type': 'Chatwoot',
            'Providers__Chat__Chatwoot__BaseUrl': f"http://{chatwoot_host['hostname']}:{chatwoot_host['port']}",
            'Providers__Chat__Chatwoot__ApiKey': '${CHATWOOT_API_KEY}',
            'Providers__Chat__Chatwoot__AccountId': '1'
        })
    
    if config.get('notification_provider') == 'novu':
        novu_host = get_host_config('novu', 'localhost', 3001)
        api_env.update({
            'FeatureManagement__UseExternalNotifications': 'true',
            'Providers__Notifications__Type': 'Novu',
            'Providers__Notifications__Novu__ApiKey': '${NOVU_API_KEY}',
            'Providers__Notifications__Novu__ApplicationId': '${NOVU_APPLICATION_ID}',
            'Providers__Notifications__Novu__BaseUrl': f"http://{get_host_config('novu', 'localhost', 3001)['hostname']}:{novu_host['port']}",
        })
    
    if config.get('analytics_provider') == 'superset':
        superset_host = get_host_config('superset', 'localhost', 8088)
        api_env.update({
            'FeatureManagement__UseExternalAnalytics': 'true',
            'Providers__Analytics__Type': 'Superset',
            'Providers__Analytics__Superset__Url': f"http://{superset_host['hostname']}:{superset_host['port']}",
            'Providers__Analytics__Superset__Username': 'admin',
            'Providers__Analytics__Superset__Password': '${SUPERSET_ADMIN_PASSWORD}'
        })
    
    if config.get('signature_provider') == 'docuseal':
        docuseal_host = get_host_config('docuseal', 'localhost', 3002)
        api_env.update({
            'FeatureManagement__UseExternalSignatures': 'true',
            'Providers__Signatures__Type': 'DocuSeal',
            'Providers__Signatures__DocuSeal__Url': f"http://{docuseal_host['hostname']}:{docuseal_host['port']}",
            'Providers__Signatures__DocuSeal__ApiKey': '${DOCUSEAL_API_KEY}'
        })
    
    if config.get('ai_provider') == 'ollama':
        ollama_host = get_host_config('ollama', 'localhost', 11434)
        api_env.update({
            'FeatureManagement__UseExternalAI': 'true',
            'Providers__AI__Type': 'Ollama',
            'Providers__AI__Ollama__Url': f"http://{ollama_host['hostname']}:{ollama_host['port']}",
            'Providers__AI__Ollama__Model': 'llama3'
        })
    
    if config.get('integration_provider') == 'n8n':
        n8n_host = get_host_config('n8n', 'localhost', 5678)
        api_env.update({
            'FeatureManagement__UseExternalIntegrations': 'true',
            'Providers__Integrations__Type': 'N8n',
            'Providers__Integrations__N8n__BaseUrl': f"http://{n8n_host['hostname']}:{n8n_host['port']}",
            'Providers__Integrations__N8n__ApiKey': '${N8N_API_KEY}'
        })
    
    # API bridges all networks so it can reach DB, Redis, and provider services
    if network_topology == 'segmented':
        api_nets = [net_core, net_data, net_comp]
    elif network_topology == 'dmz':
        api_nets = [net_data]          # API is internal-only; proxy bridges DMZ→internal
    else:
        api_nets = [net_core] if net_core else []
    api_svc = {
        'image': 'crm-api:latest',
        'container_name': 'crm-api',
        'restart': 'unless-stopped',
        'environment': api_env,
        'ports': [f"{api_host['port']}:5000"],
        'depends_on': ['crm-mariadb', 'crm-redis'],
        **_svc_networks(api_nets),
        'healthcheck': {
            'test': ["CMD", "curl", "-f", "http://localhost:5000/health"],
            'interval': '30s',
            'timeout': '10s',
            'retries': 3
        }
    }
    _apply_host_mode(api_svc)
    services['crm-api'] = api_svc
    
    # Add SSL volumes if SSL is enabled
    if ssl_enabled:
        services['crm-api']['volumes'] = ['ssl_certs:/app/ssl']
        volumes['ssl_certs'] = {'driver': 'local'}
    
    # Frontend — sits on the public-facing / core network
    frontend_host = get_host_config('frontend', 'localhost', 80)
    fe_svc = {
        'image': 'crm-frontend:latest',
        'container_name': 'crm-frontend',
        'restart': 'unless-stopped',
        'environment': {
            'REACT_APP_API_URL': f"{protocol}://{deployment_host}:{api_port}/api"
        },
        'ports': [f"{frontend_host['port']}:80"],
        'depends_on': ['crm-api'],
        **_svc_networks([net_core])
    }
    _apply_host_mode(fe_svc)
    services['crm-frontend'] = fe_svc
    
    # Add provider services based on selection (providers go on components network)
    _add_provider_services(config, services, volumes, provider_network_name, get_host_config, redis_host)
    
    compose = {
        'services': services,
        'volumes': volumes,
    }
    if compose_networks:
        compose['networks'] = compose_networks
    
    return yaml.dump(compose, default_flow_style=False, sort_keys=False)

def generate_env_file(config):
    """Generate .env file with environment variables."""
    env_vars = []

    # Resolve deployment target host from profile — no assumptions
    _target = config.get('target', {})
    deployment_host = (
        _target.get('host')
        or _target.get('domain_name')
        or config.get('deployment_host')
        or config.get('host')
    )
    if not deployment_host:
        raise ValueError(
            "Deployment host is not configured. "
            "Set config.target.host before generating .env."
        )
    api_port = str(_target.get('api_port', config.get('api_port', 5000)))
    frontend_port = str(_target.get('frontend_port', config.get('frontend_port', 80)))
    ssl_enabled = config.get('ssl', {}).get('ssl_enabled', config.get('ssl_enabled', False))
    env_protocol = 'https' if ssl_enabled else 'http'

    # Deployment target
    env_vars.extend([
        f"# Deployment Target",
        f"CRM_DEPLOYMENT_HOST={deployment_host}",
        f"CRM_API_PORT={api_port}",
        f"CRM_FRONTEND_PORT={frontend_port}",
        f"ASPNETCORE_ENVIRONMENT=Production",
        f"REACT_APP_API_URL={env_protocol}://{deployment_host}:{api_port}/api",
        f"AllowedOrigins__0={env_protocol}://{deployment_host}:{frontend_port}",
        f"AllowedOrigins__1={env_protocol}://{deployment_host}",
        f"RateLimiting__EnableEndpointRateLimiting=false",
        "",
    ])

    # Database
    env_vars.extend([
        "DB_HOST=crm-mariadb",
        f"DB_PORT={config.get('database_port', 3306)}",
        f"DB_NAME={config.get('database_name', 'crm_db')}",
        "DB_USER=crm_user",
        f"DB_PASSWORD={generate_secure_password()}",
        f"DB_ROOT_PASSWORD={generate_secure_password()}"
    ])
    
    # JWT
    env_vars.extend([
        f"JWT_SECRET={secrets.token_urlsafe(32)}",
        "JWT_ISSUER=CRM.Api",
        "JWT_AUDIENCE=CRM.Client"
    ])
    
    # Admin (generate secure password instead of default)
    admin_password = generate_secure_password()
    env_vars.extend([
        "ADMIN_USERNAME=admin",
        "ADMIN_EMAIL=admin@crm.local",
        f"ADMIN_PASSWORD={admin_password}"
    ])
    
    # Provider environment variables
    if config.get('search_provider') == 'meilisearch':
        env_vars.append(f"MEILI_MASTER_KEY={secrets.token_urlsafe(16)}")
    
    if config.get('chat_provider') == 'chatwoot':
        env_vars.extend([
            f"CHATWOOT_API_KEY={secrets.token_urlsafe(32)}",
            f"CHATWOOT_SECRET_KEY={secrets.token_urlsafe(32)}",
            f"CHATWOOT_DB_PASSWORD={generate_secure_password()}"
        ])
    
    if config.get('notification_provider') == 'novu':
        env_vars.extend([
            f"NOVU_API_KEY={secrets.token_urlsafe(32)}",
            f"NOVU_APPLICATION_ID={secrets.token_hex(8)}",
            f"NOVU_JWT_SECRET={secrets.token_urlsafe(32)}"
        ])
    
    if config.get('analytics_provider') == 'superset':
        env_vars.extend([
            f"SUPERSET_SECRET_KEY={secrets.token_urlsafe(32)}",
            f"SUPERSET_ADMIN_PASSWORD={generate_secure_password()}",
            f"SUPERSET_DB_PASSWORD={generate_secure_password()}"
        ])
    
    if config.get('signature_provider') == 'docuseal':
        env_vars.extend([
            f"DOCUSEAL_SECRET_KEY={secrets.token_urlsafe(32)}",
            f"DOCUSEAL_API_KEY={secrets.token_urlsafe(32)}",
            f"DOCUSEAL_DB_PASSWORD={generate_secure_password()}"
        ])
    
    if config.get('ai_provider') == 'ollama':
        env_vars.append("OLLAMA_MODEL=llama3")
    
    if config.get('integration_provider') == 'n8n':
        env_vars.extend([
            "N8N_USERNAME=admin",
            f"N8N_PASSWORD={generate_secure_password()}",
            f"N8N_ENCRYPTION_KEY={secrets.token_urlsafe(32)}",
            f"N8N_DB_PASSWORD={generate_secure_password()}",
            f"N8N_API_KEY={secrets.token_urlsafe(32)}"
        ])
    
    return "\n".join(env_vars)

def generate_deployment_script(config):
    """Generate one-click deployment script with logging and rollback."""
    # Resolve deployment host from profile — no localhost fallback
    _target = config.get('target', {})
    deploy_host = (
        _target.get('host')
        or _target.get('domain_name')
        or config.get('deployment_host')
        or config.get('host')
    )
    if not deploy_host:
        raise ValueError(
            "Deployment host is not configured. "
            "Set config.target.host before generating the deployment script."
        )
    d_api_port = str(_target.get('api_port', config.get('api_port', 5000)))
    d_frontend_port = str(_target.get('frontend_port', config.get('frontend_port', 80)))
    d_ssl = config.get('ssl', {}).get('ssl_enabled', config.get('ssl_enabled', False))
    d_protocol = 'https' if d_ssl else 'http'

    script = f"""#!/bin/bash
# CRM Solution Deployment Script
# Generated by CRM Deployment Wizard
# This script provides detailed logging and rollback on error
# Target host: {deploy_host}

set -e  # Exit on any error

# Deployment target configuration (from profile)
CRM_HOST="{deploy_host}"
CRM_API_PORT="{d_api_port}"
CRM_FRONTEND_PORT="{d_frontend_port}"
CRM_PROTOCOL="{d_protocol}"
"""

    script += """
# Colors for output
RED='\\033[0;31m'
GREEN='\\033[0;32m'
YELLOW='\\033[1;33m'
BLUE='\\033[0;34m'
NC='\\033[0m' # No Color

# Logging functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a deployment.log
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a deployment.log
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a deployment.log
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a deployment.log
}

# Rollback function
rollback() {
    log_error "Deployment failed! Starting rollback..."
    
    # Stop and remove containers
    if docker-compose ps -q | grep -q .; then
        log_info "Stopping running containers..."
        docker-compose down --remove-orphans || true
    fi
    
    # Remove volumes if cleanup requested
    if [ "$FULL_CLEANUP" = "true" ]; then
        log_info "Removing volumes..."
        docker volume rm $(docker volume ls -q | grep crm) 2>/dev/null || true
    fi
    
    # Remove images if cleanup requested
    if [ "$FULL_CLEANUP" = "true" ]; then
        log_info "Removing CRM images..."
        docker rmi $(docker images | grep crm | awk '{print $3}') 2>/dev/null || true
    fi
    
    log_info "Rollback completed."
    exit 1
}

# Trap errors and call rollback
trap rollback ERR

# Configuration
DEPLOYMENT_NAME="crm-deployment-$(date +%Y%m%d-%H%M%S)"
BACKUP_DIR="./backups/$DEPLOYMENT_NAME"
FULL_CLEANUP="${FULL_CLEANUP:-false}"

log_info "Starting CRM Solution deployment..."
log_info "Deployment ID: $DEPLOYMENT_NAME"

# Pre-deployment checks
log_info "Performing pre-deployment checks..."

# Check if Docker is running
if ! docker info >/dev/null 2>&1; then
    log_error "Docker is not running. Please start Docker and try again."
    exit 1
fi

# Check if Docker Compose is available
if ! command -v docker-compose >/dev/null 2>&1; then
    log_error "Docker Compose is not installed. Please install Docker Compose and try again."
    exit 1
fi

# Check if required files exist
required_files=("docker-compose.yml" ".env")
for file in "${required_files[@]}"; do
    if [ ! -f "$file" ]; then
        log_error "Required file $file not found in current directory."
        exit 1
    fi
done

log_success "Pre-deployment checks passed."

# Create backup directory
log_info "Creating backup directory: $BACKUP_DIR"
mkdir -p "$BACKUP_DIR"

# Backup existing deployment if it exists
if [ -f "docker-compose.yml" ] && docker-compose ps -q | grep -q .; then
    log_info "Backing up existing deployment..."
    docker-compose config > "$BACKUP_DIR/docker-compose.backup.yml" 2>/dev/null || true
    docker-compose ps > "$BACKUP_DIR/containers.backup.txt" 2>/dev/null || true
    cp .env "$BACKUP_DIR/.env.backup" 2>/dev/null || true
    log_success "Backup created in $BACKUP_DIR"
fi

# Validate configuration
log_info "Validating Docker Compose configuration..."
if ! docker-compose config >/dev/null 2>&1; then
    log_error "Docker Compose configuration is invalid."
    exit 1
fi
log_success "Configuration validation passed."

# Pull images
log_info "Pulling Docker images..."
docker-compose pull
log_success "Images pulled successfully."

# Start services
log_info "Starting CRM services..."
docker-compose up -d
log_success "Services started successfully."

# Wait for services to be healthy
log_info "Waiting for services to be healthy..."
max_attempts=30
attempt=1

while [ $attempt -le $max_attempts ]; do
    log_info "Health check attempt $attempt/$max_attempts..."
    
    # Check core services
    healthy=true
    
    # Check MariaDB
    if ! docker-compose exec -T mariadb mysqladmin ping -h localhost --silent; then
        healthy=false
    fi
    
    # Check Redis
    if ! docker-compose exec -T redis redis-cli ping | grep -q PONG; then
        healthy=false
    fi
    
    # Check API health endpoint
    if ! curl -f -s ${CRM_PROTOCOL}://${CRM_HOST}:${CRM_API_PORT}/health >/dev/null 2>&1; then
        healthy=false
    fi
    
    if [ "$healthy" = true ]; then
        log_success "All services are healthy!"
        break
    fi
    
    if [ $attempt -eq $max_attempts ]; then
        log_error "Services failed to become healthy within timeout."
        exit 1
    fi
    
    sleep 10
    ((attempt++))
done

# Post-deployment tasks
log_info "Running post-deployment tasks..."

# Initialize provider services if needed
"""
    
    # Add provider-specific initialization
    if config.get('ai_provider') == 'ollama':
        script += """
# Initialize Ollama model
log_info "Initializing Ollama model..."
if docker-compose exec -T ollama ollama list | grep -q llama3; then
    log_info "Llama3 model already available."
else
    log_info "Pulling Llama3 model (this may take a while)..."
    docker-compose exec -T ollama ollama pull llama3
    log_success "Llama3 model initialized."
fi
"""
    
    if config.get('analytics_provider') == 'superset':
        script += """
# Initialize Superset
log_info "Initializing Superset..."
sleep 30  # Wait for Superset to fully start
docker-compose exec -T superset superset db upgrade
docker-compose exec -T superset superset init
log_success "Superset initialized."
"""
    
    script += """
# Final verification
log_info "Performing final verification..."

# Test API endpoints
if curl -f -s ${CRM_PROTOCOL}://${CRM_HOST}:${CRM_API_PORT}/api/health >/dev/null 2>&1; then
    log_success "API is responding correctly."
else
    log_warning "API health check failed, but services are running."
fi

# Display service status
log_info "Service Status:"
docker-compose ps

# Display access information
echo
echo "=================================================="
echo "  CRM Solution Deployment Complete!"
echo "=================================================="
echo
echo "Access URLs:"
echo "  Frontend:    ${CRM_PROTOCOL}://${CRM_HOST}:${CRM_FRONTEND_PORT}"
echo "  API:         ${CRM_PROTOCOL}://${CRM_HOST}:${CRM_API_PORT}"
echo "  Database:    ${CRM_HOST}:3306"
"""

    # Add provider access URLs
    if config.get('search_provider') == 'meilisearch':
        script += """echo "  Meilisearch: ${CRM_PROTOCOL}://${CRM_HOST}:7700"\n"""
    
    if config.get('chat_provider') == 'chatwoot':
        script += """echo "  Chatwoot:    ${CRM_PROTOCOL}://${CRM_HOST}:3000"\n"""
    
    if config.get('notification_provider') == 'novu':
        script += """echo "  Novu:        ${CRM_PROTOCOL}://${CRM_HOST}:3002"\n"""
    
    if config.get('analytics_provider') == 'superset':
        script += """echo "  Superset:    ${CRM_PROTOCOL}://${CRM_HOST}:8088"\n"""
    
    if config.get('signature_provider') == 'docuseal':
        script += """echo "  DocuSeal:    ${CRM_PROTOCOL}://${CRM_HOST}:3001"\n"""
    
    if config.get('ai_provider') == 'ollama':
        script += """echo "  Ollama:      ${CRM_PROTOCOL}://${CRM_HOST}:11434"\n"""

    script += """
echo
echo "Admin Credentials:"
echo "  Username: admin"
echo "  Email:    admin@crm.local"
echo "  Password: (check .env file)"
echo
echo "Deployment logs saved to: deployment.log"
echo "Backup saved to: $BACKUP_DIR"
echo
echo "To stop the deployment: docker-compose down"
echo "To view logs: docker-compose logs -f"
echo
echo "=================================================="

log_success "CRM Solution deployment completed successfully!"
log_info "Deployment ID: $DEPLOYMENT_NAME"

# Save deployment info
cat > deployment-info.txt << EOF
CRM Solution Deployment Information
===================================

Deployment ID: $DEPLOYMENT_NAME
Deployed At: $(date)
Status: SUCCESS

Access URLs:
  Frontend: ${CRM_PROTOCOL}://${CRM_HOST}:${CRM_FRONTEND_PORT}
  API: ${CRM_PROTOCOL}://${CRM_HOST}:${CRM_API_PORT}
  Database: ${CRM_HOST}:3306
"""

    if config.get('search_provider') == 'meilisearch':
        script += """  Meilisearch: ${CRM_PROTOCOL}://${CRM_HOST}:7700\n"""
    
    if config.get('chat_provider') == 'chatwoot':
        script += """  Chatwoot: ${CRM_PROTOCOL}://${CRM_HOST}:3000\n"""
    
    if config.get('notification_provider') == 'novu':
        script += """  Novu: ${CRM_PROTOCOL}://${CRM_HOST}:3002\n"""
    
    if config.get('analytics_provider') == 'superset':
        script += """  Superset: ${CRM_PROTOCOL}://${CRM_HOST}:8088\n"""
    
    if config.get('signature_provider') == 'docuseal':
        script += """  DocuSeal: ${CRM_PROTOCOL}://${CRM_HOST}:3001\n"""
    
    if config.get('ai_provider') == 'ollama':
        script += """  Ollama: ${CRM_PROTOCOL}://${CRM_HOST}:11434\n"""

    script += """
Admin Credentials:
  Username: admin
  Email: admin@crm.local
  Password: (stored securely in .env)

Backup Location: $BACKUP_DIR
Log File: deployment.log
EOF

log_info "Deployment information saved to deployment-info.txt"
"""

    return script

def generate_kubernetes(_config):
    """Generate Kubernetes manifests for microservices."""
    # This would be more complex - for now return a basic template
    return "# Kubernetes manifests would be generated here\n# Based on microservices architecture"

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="CRM CDT GUI")
    parser.add_argument("--port", type=int, default=int(os.environ.get("CDT_PORT", os.environ.get("PORT", 5050))))
    parser.add_argument("--host", default=os.environ.get("BIND_HOST", "0.0.0.0"))
    parser.add_argument("--no-debug", action="store_true")
    parser.add_argument("--headless", action="store_true",
                        help="Run without trying to open a browser (Docker/CI mode)")
    args = parser.parse_args()
    debug_mode = not args.no_debug
    print()
    print("=" * 60)
    print("   CRM Solution - Deployment Configuration GUI")
    print("=" * 60)
    print()
    print(f"   Wizard:   http://localhost:{args.port}")
    print(f"   Day-2:    http://localhost:{args.port}/day2")
    print()
    print("   Press Ctrl+C to stop")
    print("=" * 60)
    print()
    if socketio:
        socketio.run(app, host=args.host, port=args.port, debug=debug_mode)
    else:
        app.run(host=args.host, port=args.port, debug=debug_mode)
