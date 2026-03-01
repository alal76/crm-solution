#!/usr/bin/env python3
"""CRM CDT — Registry Management Routes.

Provides endpoints to:
- List local CRM Docker images
- View deployed container image versions and detect updates
- Purge old / dangling images
- Get recommended registry configuration per platform
"""
from __future__ import annotations

from flask import Blueprint, jsonify, request

from deployers.docker_compose import (
    list_local_images,
    get_deployed_image_versions,
    purge_images,
    recommend_registry,
)

registry_bp = Blueprint("registry", __name__)


# ─── List local images ─────────────────────────────────────────────── #
@registry_bp.route("/api/registry/images", methods=["GET"])
def api_registry_images():
    """Return all local Docker images (optionally filtered to CRM only).

    Query params:
        all  — set to ``true`` to include non-CRM images.
    """
    filter_crm = request.args.get("all", "").lower() != "true"
    images = list_local_images(filter_crm=filter_crm)
    return jsonify({"images": images, "count": len(images)})


# ─── Deployed container versions ───────────────────────────────────── #
@registry_bp.route("/api/registry/deployed", methods=["GET"])
def api_registry_deployed():
    """Return running CRM containers and whether they need an update."""
    containers = get_deployed_image_versions()
    needs_update = sum(1 for c in containers if c.get("needs_update"))
    return jsonify({
        "containers": containers,
        "count": len(containers),
        "needs_update": needs_update,
    })


# ─── Purge images ──────────────────────────────────────────────────── #
@registry_bp.route("/api/registry/purge", methods=["POST"])
def api_registry_purge():
    """Remove specified images or prune dangling images.

    JSON body:
        { "image_ids": ["abc123", ...] }   — remove specific images
      OR
        { "dangling": true }               — prune all dangling images
    """
    body = request.get_json(silent=True) or {}
    image_ids = body.get("image_ids", [])
    dangling = body.get("dangling", False)

    if not image_ids and not dangling:
        return jsonify({"error": "Provide image_ids or set dangling=true"}), 400

    result = purge_images(image_ids=image_ids, dangling_only=dangling)
    return jsonify(result)


# ─── Recommend registry for platform ──────────────────────────────── #
@registry_bp.route("/api/registry/recommend", methods=["GET"])
def api_registry_recommend():
    """Return recommended registry settings for a given platform.

    Query params:
        platform — one of: local_docker, on_premises, azure, aws, gcp
    """
    platform = request.args.get("platform", "local_docker")
    rec = recommend_registry(platform)
    return jsonify({"platform": platform, "recommendation": rec})
