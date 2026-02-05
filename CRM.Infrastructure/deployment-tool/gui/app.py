#!/usr/bin/env python3
"""Flask GUI for CRM Deployment Configuration Wizard."""

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent))

from flask import Flask, render_template, request, jsonify, session
import json

from models.config_models import (
    TargetPlatform, DeploymentArchitecture, DatabaseType, ProviderStrategy
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

app = Flask(__name__)
app.secret_key = "crm-deployment-wizard-secret-key"

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

@app.route("/")
def index():
    return render_template("index.html")

@app.route("/wizard")
def wizard():
    return render_template("wizard.html")

@app.route("/api/enums/<enum_name>")
def get_enum(enum_name):
    if enum_name not in ENUM_MAP:
        return jsonify({"error": f"Unknown enum: {enum_name}"}), 404
    return jsonify(get_enum_choices(ENUM_MAP[enum_name]))

@app.route("/api/providers/<category>")
def get_providers(category):
    if category not in PROVIDER_MAP:
        return jsonify({"error": f"Unknown provider category: {category}"}), 404
    return jsonify(get_provider_choices(PROVIDER_MAP[category]))

@app.route("/api/size-recommendation/<int:users>")
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

@app.route("/api/config", methods=["GET", "POST"])
def handle_config():
    if request.method == "GET":
        return jsonify(session.get("config", {}))
    session["config"] = request.json
    return jsonify({"status": "ok"})

@app.route("/api/generate", methods=["POST"])
def generate_files():
    config = request.json
    output_dir = Path(__file__).parent.parent / "output"
    output_dir.mkdir(exist_ok=True)
    config_file = output_dir / "deployment-config.json"
    with open(config_file, "w") as f:
        json.dump(config, f, indent=2)
    return jsonify({"status": "ok", "message": "Configuration saved", "file": str(config_file)})

if __name__ == "__main__":
    print()
    print("=" * 60)
    print("   CRM Solution - Deployment Configuration GUI")
    print("=" * 60)
    print()
    print("   Open: http://localhost:5050")
    print()
    print("   Press Ctrl+C to stop")
    print("=" * 60)
    print()
    app.run(host="0.0.0.0", port=5050, debug=True)
