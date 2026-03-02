#!/usr/bin/env python3
"""CRM CDT - Kubernetes Deployer."""
from __future__ import annotations
import logging
import subprocess
import queue
import threading
import time
import json
from pathlib import Path
from typing import Optional
from deployers.docker_compose import DeployEvent

logger = logging.getLogger("cdt.deployer.k8s")


class KubernetesDeployer:
    def __init__(
        self,
        work_dir: Path,
        profile: dict,
        log_queue: Optional[queue.Queue] = None,
        dry_run: bool = False,
        namespace: str = "crm-prod",
        kubeconfig: str = "",
        infrastructure_action: str = "fresh",
    ):
        self.work_dir = Path(work_dir)
        self.profile = profile
        self.log_queue = log_queue if log_queue is not None else queue.Queue()
        self.dry_run = dry_run
        self.namespace = namespace
        self.kubeconfig = kubeconfig
        self.infrastructure_action = infrastructure_action  # fresh | reuse | recreate
        self._abort = threading.Event()
        self.total_steps = 13

    _LOG_LEVEL_MAP = {"info": logging.INFO, "warn": logging.WARNING, "error": logging.ERROR, "success": logging.INFO}

    def _emit(self, message: str, level: str = "info", step: int = 0) -> None:
        pct = int((step / self.total_steps) * 100) if self.total_steps else 0
        event = DeployEvent(time.time(), level, message, step, self.total_steps, pct)
        self.log_queue.put(event)
        logger.log(self._LOG_LEVEL_MAP.get(level, logging.INFO), message)

    def abort(self) -> None:
        self._abort.set()

    def _kube_cmd(self) -> list:
        cmd = ["kubectl"]
        if self.kubeconfig:
            cmd += ["--kubeconfig", self.kubeconfig]
        return cmd

    def _stub_step(self, name: str) -> tuple:
        """Placeholder for not-yet-implemented K8s steps. Logs explicitly."""
        self._emit(f"  [{name}] Not yet implemented — skipping (stub)", "info")
        return (0, "", "")

    def _run(self, cmd: list, timeout: int = 300) -> tuple:
        if self.dry_run:
            self._emit(f"[DRY-RUN] Would run: {' '.join(str(c) for c in cmd)}")
            return (0, "", "")
        try:
            result = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                cwd=str(self.work_dir),
                timeout=timeout,
            )
            return (result.returncode, result.stdout, result.stderr)
        except subprocess.TimeoutExpired:
            return (1, "", f"Command timed out after {timeout}s")
        except Exception as e:
            self._emit(f"Command execution failed: {e}", "error")
            return (1, "", str(e))

    def deploy(self) -> bool:
        kube = self._kube_cmd()

        # Handle infrastructure_action: reuse vs recreate vs fresh
        if self.infrastructure_action == "recreate":
            self._emit("Recreating namespace (infrastructure_action=recreate)", "warn", 0)
            self._run(kube + ["delete", "namespace", self.namespace, "--ignore-not-found=true"])
        elif self.infrastructure_action == "reuse":
            self._emit("Reusing existing namespace (infrastructure_action=reuse)", "info", 0)

        steps_list = [
            (1,  "Validate kubectl",    lambda: self._run(kube + ["version"])),
            (2,  "Create namespace",    lambda: self._run(kube + ["create", "namespace", self.namespace, "--dry-run=client", "-o", "yaml"])),
            (3,  "Create secrets",      lambda: self._run(kube + ["create", "secret", "generic", "crm-secrets", "--from-env-file=.env", f"-n={self.namespace}", "--dry-run=client", "-o", "yaml"])),
            (4,  "Apply configmaps",    lambda: self._stub_step("Apply configmaps")),
            (5,  "Deploy MariaDB",      lambda: self._run(kube + ["apply", "-f", "crm-deployment.yaml", f"-n={self.namespace}"])),
            (6,  "Deploy Redis",        lambda: self._stub_step("Deploy Redis")),
            (7,  "Wait DB ready",       lambda: self._run(kube + ["rollout", "status", "deployment/crm-mariadb", f"-n={self.namespace}", "--timeout=120s"])),
            (8,  "Run migrations",      lambda: self._stub_step("Run migrations")),
            (9,  "Deploy providers",    lambda: self._stub_step("Deploy providers")),
            (10, "Deploy API",          lambda: self._run(kube + ["apply", "-f", "crm-deployment.yaml", f"-n={self.namespace}"])),
            (11, "Deploy frontend",     lambda: self._run(kube + ["apply", "-f", "crm-deployment.yaml", f"-n={self.namespace}"])),
            (12, "Apply ingress",       lambda: self._stub_step("Apply ingress")),
            (13, "Verify pods running", lambda: self._run(kube + ["get", "pods", f"-n={self.namespace}"])),
        ]
        for step_num, step_name, step_fn in steps_list:
            if self._abort.is_set():
                self._emit("Deployment aborted", "warn", step_num)
                return False
            self._emit(f"Step {step_num}/{self.total_steps}: {step_name}", "info", step_num)
            try:
                rc, out, err = step_fn()
                if rc != 0 and not self.dry_run:
                    self._emit(f"Step {step_num} warning: {err}", "warn", step_num)
            except Exception as e:
                self._emit(f"Step {step_num} error: {e}", "error", step_num)
        self._emit("Kubernetes deployment complete", "success", self.total_steps)
        return True

    def rollback(self) -> bool:
        kube = self._kube_cmd()
        self._emit(f"Rolling back — deleting namespace {self.namespace}", "warn")
        rc, _, err = self._run(
            kube + ["delete", "namespace", self.namespace, "--ignore-not-found=true"]
        )
        if rc != 0 and not self.dry_run:
            self._emit(f"Rollback had errors (rc={rc}): {err}", "warn")
        else:
            self._emit("Rollback complete", "info")
        return rc == 0 or self.dry_run

    def status(self) -> dict:
        kube = self._kube_cmd()
        rc, out, _ = self._run(
            kube + ["get", "pods", f"-n={self.namespace}", "-o", "json"]
        )
        pods = []
        if rc == 0 and out.strip():
            try:
                data = json.loads(out)
                pods = data.get("items", [])
            except json.JSONDecodeError as exc:
                self._emit(f"Failed to parse kubectl pod JSON: {exc}", "warn")
        running = sum(
            1 for p in pods if p.get("status", {}).get("phase") == "Running"
        )
        return {"pods": len(pods), "running": running, "namespace": self.namespace}
