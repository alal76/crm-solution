#!/usr/bin/env python3
"""Shared constants for the CRM CDT.

Centralised here so that every module (day-2 routes, deploy routes,
deployers, probes) uses the same set of values.  Adding a new cloud
compute option in *one* place is enough.
"""
from __future__ import annotations

# ---------------------------------------------------------------------------
# Cloud compute options that map to a Kubernetes-style deployer.
# These are *true* Kubernetes platforms where ``kubectl`` works.
# ---------------------------------------------------------------------------
K8S_COMPUTES: frozenset[str] = frozenset({
    "aks",           # Azure Kubernetes Service
    "eks",           # Amazon Elastic Kubernetes Service
    "gke",           # Google Kubernetes Engine
})

# ---------------------------------------------------------------------------
# Serverless / managed-container platforms.
# They are container-based but do NOT use kubectl — they need their own
# CLI tooling (az containerapp, aws ecs, gcloud run).
# Today the CDT routes these through the Docker-compose deployer path with
# a ``"serverless"`` runtime hint so that Day-2 and preflight code can
# distinguish them from plain Docker Compose or true K8s.
# ---------------------------------------------------------------------------
SERVERLESS_COMPUTES: frozenset[str] = frozenset({
    "container_apps",  # Azure Container Apps
    "fargate",         # AWS ECS/Fargate
    "cloud_run",       # Google Cloud Run
})

# Union of both — anything that is NOT a plain Docker VM.
CLOUD_COMPUTES: frozenset[str] = K8S_COMPUTES | SERVERLESS_COMPUTES

# ---------------------------------------------------------------------------
# Recognised runtime identifiers used across the tool.
# ---------------------------------------------------------------------------
RUNTIME_DOCKER_COMPOSE = "docker_compose"
RUNTIME_KUBERNETES = "kubernetes"
RUNTIME_SERVERLESS = "serverless"

# CDT profile directory name (under $HOME)
CDT_DIR = ".crm-cdt"
ACTIVE_PROFILE_NAME_FILE = "active_profile_name.txt"

# Docker container filter for CRM containers
DOCKER_FILTER_CRM = "name=crm"


def detect_runtime(profile: dict) -> str:
    """Return the canonical runtime string for a profile.

    Returns one of ``RUNTIME_KUBERNETES``, ``RUNTIME_SERVERLESS``, or
    ``RUNTIME_DOCKER_COMPOSE``.
    """
    arch = profile.get("architecture", "")
    if isinstance(arch, dict):
        rt = arch.get("container_runtime", "")
        if rt in (RUNTIME_KUBERNETES, RUNTIME_DOCKER_COMPOSE, RUNTIME_SERVERLESS):
            return rt

    cloud_svc = profile.get("cloud_services", {})
    platform = profile.get("platform", "on_premises")
    if isinstance(cloud_svc.get(platform), dict):
        compute = cloud_svc[platform].get("compute", "")
        if compute in K8S_COMPUTES:
            return RUNTIME_KUBERNETES
        if compute in SERVERLESS_COMPUTES:
            return RUNTIME_SERVERLESS

    return RUNTIME_DOCKER_COMPOSE


def get_kubeconfig(profile: dict) -> str:
    """Extract a kubeconfig path from a profile (empty string if unset)."""
    target = profile.get("target", {})
    return (
        target.get("kubeconfig")
        or target.get("kubeconfig_path")
        or profile.get("kubeconfig")
        or ""
    )
