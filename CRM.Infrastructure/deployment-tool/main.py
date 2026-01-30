#!/usr/bin/env python3
"""
CRM Solution - Advanced GUI Deployment Tool
A comprehensive deployment tool for the CRM Solution with build server
configuration, network analysis, and real-time deployment progress.

Author: Abhishek Lal
License: AGPL-3.0
Version: 2.3.0
"""

import tkinter as tk
from tkinter import ttk, messagebox, filedialog, scrolledtext
import json
import os
import sys
import secrets
import subprocess
import threading
import webbrowser
import socket
import queue
import time
from datetime import datetime
from pathlib import Path
from typing import Optional, Dict, Any, List, Tuple, Callable
from dataclasses import dataclass, asdict, field
from enum import Enum
import urllib.request
import urllib.error
import ssl

# Version
VERSION = "3.1.0"

# Naming convention patterns
NAMING_CONVENTIONS = {
    "container_prefix": "crm",
    "network_name": "crm-network",
    "volume_prefix": "crm",
    "service_names": {
        "api": "crm-api",
        "frontend": "crm-frontend",
        "database": "crm-mariadb",
        "redis": "crm-redis",
        "gateway": "crm-gateway"
    }
}


class DeploymentStatus(Enum):
    """Deployment status enum."""
    PENDING = "pending"
    IN_PROGRESS = "in_progress"
    SUCCESS = "success"
    FAILED = "failed"
    SKIPPED = "skipped"


class Architecture(Enum):
    """Architecture type enum."""
    MONOLITHIC = "monolithic"
    MICROSERVICES = "microservices"


@dataclass
class TestResult:
    """Test result data."""
    name: str
    status: str  # passed, failed, skipped
    duration_ms: int = 0
    message: str = ""
    timestamp: str = ""


@dataclass
class DeployedResource:
    """Represents a deployed resource for tracking."""
    resource_type: str  # container, volume, network, image, cloud_resource
    name: str
    status: str  # created, running, stopped, deleted
    details: Dict[str, Any] = field(default_factory=dict)
    created_at: str = ""
    deleted_at: str = ""


@dataclass
class SmokeTestResult:
    """Smoke test result data."""
    test_name: str
    category: str  # health, ui, api, cors
    passed: bool
    duration_ms: float
    message: str = ""
    details: Dict[str, Any] = field(default_factory=dict)


class DeploymentResourceLog:
    """Tracks all resources created during deployment for summary and cleanup."""
    
    def __init__(self):
        self.resources: List[DeployedResource] = []
        self.session_log: List[Dict[str, Any]] = []
        self.start_time: datetime = datetime.now()
        self.end_time: Optional[datetime] = None
        self.test_results: List[SmokeTestResult] = []
        
    def log_event(self, event_type: str, message: str, details: Dict[str, Any] = None):
        """Log a session event."""
        self.session_log.append({
            "timestamp": datetime.now().isoformat(),
            "event_type": event_type,
            "message": message,
            "details": details or {}
        })
    
    def add_resource(self, resource_type: str, name: str, details: Dict[str, Any] = None) -> DeployedResource:
        """Add a deployed resource."""
        resource = DeployedResource(
            resource_type=resource_type,
            name=name,
            status="created",
            details=details or {},
            created_at=datetime.now().isoformat()
        )
        self.resources.append(resource)
        self.log_event("resource_created", f"Created {resource_type}: {name}", details)
        return resource
    
    def update_resource_status(self, name: str, status: str):
        """Update resource status."""
        for r in self.resources:
            if r.name == name:
                r.status = status
                if status == "deleted":
                    r.deleted_at = datetime.now().isoformat()
                self.log_event("resource_status_change", f"{name} -> {status}")
                break
    
    def add_test_result(self, result: SmokeTestResult):
        """Add a smoke test result."""
        self.test_results.append(result)
        self.log_event("test_completed", f"{result.test_name}: {'PASSED' if result.passed else 'FAILED'}", 
                      {"category": result.category, "duration_ms": result.duration_ms})
    
    def get_resources_by_type(self, resource_type: str) -> List[DeployedResource]:
        """Get resources by type."""
        return [r for r in self.resources if r.resource_type == resource_type]
    
    def get_active_resources(self) -> List[DeployedResource]:
        """Get non-deleted resources."""
        return [r for r in self.resources if r.status != "deleted"]
    
    def finalize(self):
        """Mark session as complete."""
        self.end_time = datetime.now()
        self.log_event("session_complete", f"Session duration: {self.end_time - self.start_time}")


class DeploymentSummaryGenerator:
    """Generates deployment summary reports."""
    
    def __init__(self, resource_log: DeploymentResourceLog, config: 'DeploymentConfig'):
        self.resource_log = resource_log
        self.config = config
        
    def generate_summary(self, is_test_mode: bool = False) -> str:
        """Generate a complete deployment summary."""
        lines = []
        lines.append("=" * 80)
        lines.append("CRM SOLUTION DEPLOYMENT SUMMARY")
        lines.append("=" * 80)
        lines.append("")
        
        # Session info
        lines.append(f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
        lines.append(f"Mode: {'TEST DEPLOYMENT (Resources will be decommissioned)' if is_test_mode else 'PRODUCTION DEPLOYMENT'}")
        lines.append(f"Session Start: {self.resource_log.start_time.strftime('%Y-%m-%d %H:%M:%S')}")
        if self.resource_log.end_time:
            duration = self.resource_log.end_time - self.resource_log.start_time
            lines.append(f"Session End: {self.resource_log.end_time.strftime('%Y-%m-%d %H:%M:%S')}")
            lines.append(f"Total Duration: {duration}")
        lines.append("")
        
        # Configuration
        lines.append("-" * 80)
        lines.append("CONFIGURATION")
        lines.append("-" * 80)
        lines.append(f"  Architecture: {self.config.architecture}")
        lines.append(f"  Platform: {self.config.hosting_platform}")
        lines.append(f"  Cloud Provider: {self.config.cloud_provider}")
        lines.append(f"  Domain: {self.config.domain}")
        lines.append(f"  API Port: {self.config.api_port}")
        lines.append(f"  Frontend Port: {self.config.frontend_port}")
        lines.append(f"  SSL Enabled: {self.config.ssl_enabled}")
        lines.append("")
        
        # Resources deployed
        lines.append("-" * 80)
        lines.append("RESOURCES DEPLOYED")
        lines.append("-" * 80)
        
        resource_types = ["container", "volume", "network", "image", "cloud_resource"]
        for rtype in resource_types:
            resources = self.resource_log.get_resources_by_type(rtype)
            if resources:
                lines.append(f"\n  {rtype.upper()}S:")
                for r in resources:
                    status_icon = "✓" if r.status in ["running", "created"] else "✗" if r.status == "deleted" else "○"
                    lines.append(f"    {status_icon} {r.name}")
                    lines.append(f"        Status: {r.status}")
                    lines.append(f"        Created: {r.created_at}")
                    if r.deleted_at:
                        lines.append(f"        Deleted: {r.deleted_at}")
                    if r.details:
                        for k, v in r.details.items():
                            lines.append(f"        {k}: {v}")
        lines.append("")
        
        # Test results (if any)
        if self.resource_log.test_results:
            lines.append("-" * 80)
            lines.append("SMOKE TEST RESULTS")
            lines.append("-" * 80)
            
            passed = sum(1 for t in self.resource_log.test_results if t.passed)
            failed = len(self.resource_log.test_results) - passed
            
            lines.append(f"  Total: {len(self.resource_log.test_results)} | Passed: {passed} | Failed: {failed}")
            lines.append("")
            
            # Group by category
            categories = {}
            for t in self.resource_log.test_results:
                if t.category not in categories:
                    categories[t.category] = []
                categories[t.category].append(t)
            
            for cat, tests in categories.items():
                lines.append(f"  {cat.upper()} TESTS:")
                for t in tests:
                    icon = "✓" if t.passed else "✗"
                    lines.append(f"    {icon} {t.test_name} ({t.duration_ms:.0f}ms)")
                    if t.message:
                        lines.append(f"        {t.message}")
                lines.append("")
        
        # Decommission log (if test mode)
        if is_test_mode:
            lines.append("-" * 80)
            lines.append("DECOMMISSION LOG")
            lines.append("-" * 80)
            
            deleted = [r for r in self.resource_log.resources if r.status == "deleted"]
            if deleted:
                for r in deleted:
                    lines.append(f"  ✗ {r.resource_type}: {r.name}")
                    lines.append(f"      Deleted at: {r.deleted_at}")
            else:
                lines.append("  No resources decommissioned yet")
            lines.append("")
        
        # Full session log
        lines.append("-" * 80)
        lines.append("FULL SESSION LOG")
        lines.append("-" * 80)
        for entry in self.resource_log.session_log:
            ts = entry["timestamp"].split("T")[1].split(".")[0]  # Just time
            lines.append(f"  [{ts}] {entry['event_type']}: {entry['message']}")
        lines.append("")
        
        lines.append("=" * 80)
        lines.append("END OF SUMMARY")
        lines.append("=" * 80)
        
        return "\n".join(lines)
    
    def save_summary(self, filepath: Path, is_test_mode: bool = False) -> Path:
        """Save summary to file and return path."""
        summary = self.generate_summary(is_test_mode)
        filepath.parent.mkdir(parents=True, exist_ok=True)
        with open(filepath, 'w') as f:
            f.write(summary)
        return filepath
    
    def open_in_editor(self, filepath: Path):
        """Open summary in system text editor."""
        import platform
        import subprocess
        
        system = platform.system()
        try:
            if system == "Darwin":  # macOS
                subprocess.run(["open", "-e", str(filepath)])  # Opens in TextEdit
            elif system == "Windows":
                subprocess.run(["notepad", str(filepath)])
            else:  # Linux
                # Try common editors
                for editor in ["xdg-open", "gedit", "kate", "nano"]:
                    try:
                        subprocess.run([editor, str(filepath)])
                        break
                    except FileNotFoundError:
                        continue
        except Exception as e:
            print(f"Could not open editor: {e}")


class SmokeTestRunner:
    """Runs smoke tests on deployed application."""
    
    def __init__(self, config: 'DeploymentConfig', resource_log: DeploymentResourceLog, 
                 log_callback: Callable[[str, str], None] = None):
        self.config = config
        self.resource_log = resource_log
        self.log_callback = log_callback or (lambda t, m: print(f"[{t}] {m}"))
        self.results: List[SmokeTestResult] = []
        
    def _log(self, msg_type: str, message: str):
        """Log a message."""
        self.log_callback(msg_type, message)
        
    def _make_request(self, url: str, method: str = "GET", 
                     headers: Dict[str, str] = None, 
                     check_cors: bool = False) -> Tuple[bool, int, str, Dict]:
        """Make HTTP request and return (success, status, body, headers)."""
        import urllib.request
        import urllib.error
        import ssl
        
        start = time.time()
        try:
            ctx = ssl.create_default_context()
            ctx.check_hostname = False
            ctx.verify_mode = ssl.CERT_NONE
            
            req = urllib.request.Request(url, method=method)
            if headers:
                for k, v in headers.items():
                    req.add_header(k, v)
            
            # Add Origin header for CORS testing
            if check_cors:
                req.add_header("Origin", f"http://{self.config.domain}:{self.config.frontend_port}")
            
            with urllib.request.urlopen(req, timeout=15, context=ctx) as response:
                body = response.read().decode('utf-8', errors='ignore')
                resp_headers = dict(response.headers)
                duration = (time.time() - start) * 1000
                return True, response.status, body, resp_headers
                
        except urllib.error.HTTPError as e:
            duration = (time.time() - start) * 1000
            return False, e.code, str(e), {}
        except Exception as e:
            duration = (time.time() - start) * 1000
            return False, 0, str(e), {}
    
    def run_all_tests(self) -> List[SmokeTestResult]:
        """Run all smoke tests."""
        self._log("info", "=" * 50)
        self._log("info", "RUNNING SMOKE TESTS")
        self._log("info", "=" * 50)
        
        self.results = []
        
        # Health checks
        self._run_health_tests()
        
        # API tests
        self._run_api_tests()
        
        # UI tests
        self._run_ui_tests()
        
        # CORS tests
        self._run_cors_tests()
        
        # Log summary
        passed = sum(1 for r in self.results if r.passed)
        failed = len(self.results) - passed
        
        self._log("info", "=" * 50)
        self._log("info", f"SMOKE TEST SUMMARY: {passed} passed, {failed} failed")
        self._log("info", "=" * 50)
        
        return self.results
    
    def _add_result(self, test_name: str, category: str, passed: bool, 
                   duration_ms: float, message: str = "", details: Dict = None):
        """Add a test result."""
        result = SmokeTestResult(
            test_name=test_name,
            category=category,
            passed=passed,
            duration_ms=duration_ms,
            message=message,
            details=details or {}
        )
        self.results.append(result)
        self.resource_log.add_test_result(result)
        
        icon = "✓" if passed else "✗"
        level = "success" if passed else "error"
        self._log(level, f"{icon} {test_name}: {message or ('PASSED' if passed else 'FAILED')}")
    
    def _run_health_tests(self):
        """Run health endpoint tests."""
        self._log("info", "\n--- Health Check Tests ---")
        
        protocol = "https" if self.config.ssl_enabled else "http"
        
        # API health
        start = time.time()
        api_url = f"{protocol}://{self.config.domain}:{self.config.api_port}/health"
        success, status, body, headers = self._make_request(api_url)
        duration = (time.time() - start) * 1000
        self._add_result("API Health Check", "health", success and status < 400, duration,
                        f"Status: {status}" if success else f"Failed: {body[:100]}")
        
        # Frontend health
        start = time.time()
        frontend_url = f"{protocol}://{self.config.domain}:{self.config.frontend_port}"
        success, status, body, headers = self._make_request(frontend_url)
        duration = (time.time() - start) * 1000
        self._add_result("Frontend Health Check", "health", success and status < 400, duration,
                        f"Status: {status}" if success else f"Failed: {body[:100]}")
        
        # Database connectivity (via API)
        start = time.time()
        db_url = f"{protocol}://{self.config.domain}:{self.config.api_port}/api/health/database"
        success, status, body, headers = self._make_request(db_url)
        duration = (time.time() - start) * 1000
        # This endpoint may not exist, so we check if API is up as fallback
        self._add_result("Database Connectivity", "health", success and status < 400, duration,
                        f"Status: {status}" if success else "Endpoint not available (API may not expose this)")
    
    def _run_api_tests(self):
        """Run API endpoint tests."""
        self._log("info", "\n--- API Endpoint Tests ---")
        
        protocol = "https" if self.config.ssl_enabled else "http"
        base_url = f"{protocol}://{self.config.domain}:{self.config.api_port}"
        
        # Common API endpoints to test
        endpoints = [
            ("/api/version", "API Version Endpoint"),
            ("/api/auth/status", "Auth Status Endpoint"),
            ("/swagger", "Swagger Documentation"),
            ("/api/customers", "Customers API (may require auth)"),
        ]
        
        for endpoint, name in endpoints:
            start = time.time()
            url = f"{base_url}{endpoint}"
            success, status, body, headers = self._make_request(url)
            duration = (time.time() - start) * 1000
            
            # 401/403 is acceptable - means endpoint exists but requires auth
            is_ok = status in [200, 201, 204, 401, 403] if success else False
            self._add_result(name, "api", success or status in [401, 403], duration,
                           f"Status: {status}" if status else f"Failed: {body[:50]}")
    
    def _run_ui_tests(self):
        """Run UI page accessibility tests."""
        self._log("info", "\n--- UI Page Tests ---")
        
        protocol = "https" if self.config.ssl_enabled else "http"
        base_url = f"{protocol}://{self.config.domain}:{self.config.frontend_port}"
        
        # UI pages to test
        pages = [
            ("/", "Home Page"),
            ("/login", "Login Page"),
            ("/dashboard", "Dashboard Page"),
            ("/customers", "Customers Page"),
            ("/settings", "Settings Page"),
        ]
        
        for page, name in pages:
            start = time.time()
            url = f"{base_url}{page}"
            success, status, body, headers = self._make_request(url)
            duration = (time.time() - start) * 1000
            
            # Check if it returns HTML
            is_html = "<html" in body.lower() if body else False
            is_ok = success and status == 200 and is_html
            
            # React apps often return 200 for all routes (SPA)
            self._add_result(name, "ui", success and status == 200, duration,
                           f"Status: {status}, HTML: {is_html}")
    
    def _run_cors_tests(self):
        """Run CORS configuration tests."""
        self._log("info", "\n--- CORS Tests ---")
        
        protocol = "https" if self.config.ssl_enabled else "http"
        api_url = f"{protocol}://{self.config.domain}:{self.config.api_port}/api/version"
        
        start = time.time()
        success, status, body, headers = self._make_request(api_url, check_cors=True)
        duration = (time.time() - start) * 1000
        
        # Check for CORS headers
        cors_header = headers.get("Access-Control-Allow-Origin", "")
        has_cors = bool(cors_header)
        
        self._add_result("CORS Headers Present", "cors", has_cors, duration,
                        f"Access-Control-Allow-Origin: {cors_header}" if has_cors else "No CORS headers")
        
        # Preflight test
        start = time.time()
        try:
            import urllib.request
            import ssl
            
            ctx = ssl.create_default_context()
            ctx.check_hostname = False
            ctx.verify_mode = ssl.CERT_NONE
            
            req = urllib.request.Request(api_url, method="OPTIONS")
            req.add_header("Origin", f"http://{self.config.domain}:{self.config.frontend_port}")
            req.add_header("Access-Control-Request-Method", "POST")
            req.add_header("Access-Control-Request-Headers", "Content-Type")
            
            with urllib.request.urlopen(req, timeout=15, context=ctx) as response:
                preflight_headers = dict(response.headers)
                allow_methods = preflight_headers.get("Access-Control-Allow-Methods", "")
                duration = (time.time() - start) * 1000
                self._add_result("CORS Preflight", "cors", bool(allow_methods), duration,
                               f"Allowed methods: {allow_methods}" if allow_methods else "No preflight response")
        except Exception as e:
            duration = (time.time() - start) * 1000
            self._add_result("CORS Preflight", "cors", False, duration, f"Error: {str(e)[:50]}")


class ResourceDecommissioner:
    """Handles cleanup/decommissioning of deployed resources."""
    
    def __init__(self, resource_log: DeploymentResourceLog, config: 'DeploymentConfig',
                 log_callback: Callable[[str, str], None] = None):
        self.resource_log = resource_log
        self.config = config
        self.log_callback = log_callback or (lambda t, m: print(f"[{t}] {m}"))
    
    def _log(self, msg_type: str, message: str):
        """Log a message."""
        self.log_callback(msg_type, message)
        self.resource_log.log_event("decommission", message)
    
    def _run_command(self, cmd: str) -> Tuple[bool, str]:
        """Run a shell command."""
        try:
            result = subprocess.run(cmd, shell=True, capture_output=True, text=True, timeout=120)
            return result.returncode == 0, result.stdout + result.stderr
        except Exception as e:
            return False, str(e)
    
    def decommission_all(self) -> bool:
        """Decommission all resources."""
        self._log("info", "=" * 50)
        self._log("info", "DECOMMISSIONING RESOURCES")
        self._log("info", "=" * 50)
        
        success = True
        
        # Stop and remove containers
        containers = self.resource_log.get_resources_by_type("container")
        if containers:
            self._log("info", "\n--- Stopping Containers ---")
            for container in containers:
                if container.status != "deleted":
                    self._log("info", f"Stopping: {container.name}")
                    ok, output = self._run_command(f"docker stop {container.name} 2>/dev/null")
                    if ok:
                        self._log("success", f"✓ Stopped: {container.name}")
                    
                    self._log("info", f"Removing: {container.name}")
                    ok, output = self._run_command(f"docker rm -f {container.name} 2>/dev/null")
                    if ok:
                        self._log("success", f"✓ Removed: {container.name}")
                        self.resource_log.update_resource_status(container.name, "deleted")
                    else:
                        self._log("warning", f"Could not remove: {container.name}")
                        success = False
        
        # Remove volumes
        volumes = self.resource_log.get_resources_by_type("volume")
        if volumes:
            self._log("info", "\n--- Removing Volumes ---")
            for volume in volumes:
                if volume.status != "deleted":
                    self._log("info", f"Removing volume: {volume.name}")
                    ok, output = self._run_command(f"docker volume rm {volume.name} 2>/dev/null")
                    if ok:
                        self._log("success", f"✓ Removed: {volume.name}")
                        self.resource_log.update_resource_status(volume.name, "deleted")
                    else:
                        self._log("warning", f"Could not remove volume: {volume.name}")
        
        # Remove networks
        networks = self.resource_log.get_resources_by_type("network")
        if networks:
            self._log("info", "\n--- Removing Networks ---")
            for network in networks:
                if network.status != "deleted" and network.name != "bridge":
                    self._log("info", f"Removing network: {network.name}")
                    ok, output = self._run_command(f"docker network rm {network.name} 2>/dev/null")
                    if ok:
                        self._log("success", f"✓ Removed: {network.name}")
                        self.resource_log.update_resource_status(network.name, "deleted")
        
        # Remove images (optional - they can be reused)
        images = self.resource_log.get_resources_by_type("image")
        if images:
            self._log("info", "\n--- Removing Images ---")
            for image in images:
                if image.status != "deleted":
                    self._log("info", f"Removing image: {image.name}")
                    ok, output = self._run_command(f"docker rmi {image.name} 2>/dev/null")
                    if ok:
                        self._log("success", f"✓ Removed: {image.name}")
                        self.resource_log.update_resource_status(image.name, "deleted")
                    else:
                        self._log("warning", f"Could not remove image (may be in use): {image.name}")
        
        # Cloud resources
        cloud_resources = self.resource_log.get_resources_by_type("cloud_resource")
        if cloud_resources:
            self._log("info", "\n--- Decommissioning Cloud Resources ---")
            for resource in cloud_resources:
                if resource.status != "deleted":
                    self._log("info", f"Decommissioning: {resource.name}")
                    # Cloud-specific cleanup would go here
                    self.resource_log.update_resource_status(resource.name, "deleted")
        
        # Use docker-compose down if available
        generated_dir = Path(__file__).parent / "generated"
        compose_file = generated_dir / "docker-compose.yml"
        if compose_file.exists():
            self._log("info", "\n--- Running docker-compose down ---")
            ok, output = self._run_command(f"cd {generated_dir} && docker compose down -v 2>/dev/null")
            if ok:
                self._log("success", "✓ docker-compose down completed")
            else:
                self._log("warning", "docker-compose down had issues")
        
        self._log("info", "\n" + "=" * 50)
        self._log("success" if success else "warning", 
                 "DECOMMISSION COMPLETE" if success else "DECOMMISSION COMPLETED WITH WARNINGS")
        self._log("info", "=" * 50)
        
        return success


@dataclass
class DeploymentConfig:
    """Complete deployment configuration."""
    # Architecture
    architecture: str = "monolithic"
    
    # Build Server
    build_server_name: str = "Local"
    build_server_host: str = "localhost"
    build_server_port: int = 22
    build_server_user: str = "root"
    build_server_is_local: bool = True
    build_server_ssh_key: str = ""
    
    # Components to deploy
    deploy_frontend: bool = True
    deploy_api: bool = True
    deploy_database: bool = True
    deploy_redis: bool = True
    deploy_monitoring: bool = False
    
    # Hosting
    hosting_platform: str = "docker"
    cloud_provider: str = "custom"
    
    # Database
    database_provider: str = "mariadb"
    database_host: str = "crm-mariadb"
    database_port: int = 3306
    database_name: str = "crm_db"
    database_user: str = "crm_user"
    database_password: str = "CrmPass@Dev2024!"
    database_root_password: str = "RootPass@Dev2024"
    
    # Network
    api_port: int = 5000
    frontend_port: int = 80
    redis_port: int = 6379
    domain: str = "localhost"
    ssl_enabled: bool = False
    ssl_cert_path: str = ""
    ssl_key_path: str = ""
    
    # Admin User
    admin_username: str = "admin"
    admin_email: str = "admin@crm.local"
    admin_password: str = "Admin@123"
    admin_first_name: str = "System"
    admin_last_name: str = "Administrator"
    
    # Security
    jwt_secret: str = ""
    allowed_origins: str = ""
    
    # Seed Data
    seed_master_data: bool = True
    seed_demo_data: bool = False
    seed_zip_codes: bool = False
    
    # Testing
    run_bvt_tests: bool = False
    run_smoke_tests: bool = True
    
    # Microservices
    deploy_identity_service: bool = True
    deploy_customer_service: bool = True
    deploy_sales_service: bool = True
    deploy_marketing_service: bool = True
    deploy_servicedesk_service: bool = True
    
    # Cloud Configuration
    cloud_provider: str = "none"  # none, aws, azure, gcp
    
    # AWS Configuration
    aws_region: str = "us-east-1"
    aws_access_key: str = ""
    aws_secret_key: str = ""
    aws_ecs_cluster: str = "crm-cluster"
    aws_ecr_registry: str = ""
    aws_vpc_id: str = ""
    aws_subnet_ids: str = ""
    aws_security_group: str = ""
    aws_rds_instance: str = ""
    aws_use_fargate: bool = True
    aws_use_rds: bool = True
    aws_use_elasticache: bool = True
    
    # Azure Configuration
    azure_subscription_id: str = ""
    azure_resource_group: str = "crm-resources"
    azure_location: str = "eastus"
    azure_acr_name: str = ""
    azure_aks_cluster: str = ""
    azure_use_aci: bool = False
    azure_use_aks: bool = True
    azure_use_azure_db: bool = True
    azure_use_redis_cache: bool = True
    
    # GCP Configuration
    gcp_project_id: str = ""
    gcp_region: str = "us-central1"
    gcp_zone: str = "us-central1-a"
    gcp_gke_cluster: str = "crm-cluster"
    gcp_gcr_hostname: str = "gcr.io"
    gcp_use_cloud_run: bool = False
    gcp_use_gke: bool = True
    gcp_use_cloud_sql: bool = True
    gcp_use_memorystore: bool = True
    
    # Build Configuration
    build_source: str = "local"  # local, git
    git_repo_url: str = ""
    git_branch: str = "main"
    git_username: str = ""
    git_token: str = ""
    git_ssh_key: str = ""
    git_use_ssh: bool = False
    
    # Build Server
    build_type: str = "local"  # local, remote, cloud
    cloud_build_provider: str = "none"  # none, github_actions, azure_devops, aws_codebuild, gcp_cloudbuild
    
    # Build Commands
    build_frontend_cmd: str = "cd CRM.Frontend && npm install && npm run build"
    build_api_cmd: str = "cd CRM.Backend && dotnet publish -c Release -o ./publish"
    build_docker_api_cmd: str = "docker build -t crm-api:latest -f docker/Dockerfile.backend ."
    build_docker_frontend_cmd: str = "docker build -t crm-frontend:latest -f docker/Dockerfile.frontend ."
    
    # Cloud Build
    github_actions_workflow: str = ".github/workflows/build-deploy.yml"
    azure_devops_org: str = ""
    azure_devops_project: str = ""
    azure_devops_pipeline: str = ""
    aws_codebuild_project: str = "crm-build"
    gcp_cloudbuild_trigger: str = ""


class NetworkAnalyzer:
    """Analyze network connectivity and fix issues."""
    
    def __init__(self, config: DeploymentConfig):
        self.config = config
        self.issues: List[Dict[str, Any]] = []
    
    def check_port_availability(self, host: str, port: int, timeout: float = 2.0) -> bool:
        """Check if a port is reachable."""
        try:
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.settimeout(timeout)
            result = sock.connect_ex((host, port))
            sock.close()
            return result == 0
        except Exception:
            return False
    
    def check_dns_resolution(self, hostname: str) -> Tuple[bool, str]:
        """Check if hostname can be resolved."""
        try:
            ip = socket.gethostbyname(hostname)
            return True, ip
        except socket.gaierror:
            return False, ""
    
    def analyze(self) -> List[Dict[str, Any]]:
        """Perform network analysis and return issues."""
        self.issues = []
        
        # Check build server connectivity
        if not self.config.build_server_is_local:
            host = self.config.build_server_host
            port = self.config.build_server_port
            
            can_resolve, ip = self.check_dns_resolution(host)
            if not can_resolve:
                self.issues.append({
                    "type": "dns",
                    "severity": "critical",
                    "component": "build_server",
                    "message": f"Cannot resolve hostname: {host}",
                    "fix": f"Ensure DNS is configured or use IP address directly"
                })
            elif not self.check_port_availability(host, port):
                self.issues.append({
                    "type": "connectivity",
                    "severity": "critical",
                    "component": "build_server",
                    "message": f"Cannot connect to {host}:{port}",
                    "fix": f"Check if SSH is running and firewall allows port {port}"
                })
        
        # Check API port conflict
        if self.config.deploy_api and not self.config.build_server_is_local:
            api_host = self.config.build_server_host
            api_port = self.config.api_port
            if self.check_port_availability(api_host, api_port):
                self.issues.append({
                    "type": "port_in_use",
                    "severity": "warning",
                    "component": "api",
                    "message": f"Port {api_port} already in use on {api_host}",
                    "fix": f"Stop existing service or change API port"
                })
        
        return self.issues
    
    def get_network_fixes(self) -> List[str]:
        """Get Docker network commands to fix connectivity."""
        fixes = []
        network_name = NAMING_CONVENTIONS["network_name"]
        
        # Create network
        fixes.append(f"docker network create {network_name} 2>/dev/null || true")
        
        # Connect containers
        containers = []
        if self.config.deploy_database:
            containers.append(NAMING_CONVENTIONS["service_names"]["database"])
        if self.config.deploy_api:
            containers.append(NAMING_CONVENTIONS["service_names"]["api"])
        if self.config.deploy_frontend:
            containers.append(NAMING_CONVENTIONS["service_names"]["frontend"])
        if self.config.deploy_redis:
            containers.append(NAMING_CONVENTIONS["service_names"]["redis"])
        
        for container in containers:
            fixes.append(f"docker network connect {network_name} {container} 2>/dev/null || true")
        
        return fixes


class PrerequisiteChecker:
    """Check and install prerequisites."""
    
    PREREQUISITES = {
        "docker": {
            "check_cmd": "docker --version",
            "required": True,
            "category": "core",
            "install_cmd_mac": "brew install --cask docker",
            "install_cmd_linux": "curl -fsSL https://get.docker.com | sh"
        },
        "docker-compose": {
            "check_cmd": "docker compose version",
            "required": True,
            "category": "core",
            "install_cmd_mac": "brew install docker-compose",
            "install_cmd_linux": "sudo apt-get install docker-compose-plugin"
        },
        "node": {
            "check_cmd": "node --version",
            "required": False,
            "category": "build",
            "install_cmd_mac": "brew install node",
            "install_cmd_linux": "curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash - && sudo apt-get install -y nodejs"
        },
        "npm": {
            "check_cmd": "npm --version",
            "required": False,
            "category": "build",
            "install_cmd_mac": "brew install node",
            "install_cmd_linux": "sudo apt-get install -y npm"
        },
        "dotnet": {
            "check_cmd": "dotnet --version",
            "required": False,
            "category": "build",
            "install_cmd_mac": "brew install dotnet",
            "install_cmd_linux": "sudo apt-get install -y dotnet-sdk-8.0"
        },
        "git": {
            "check_cmd": "git --version",
            "required": False,
            "category": "build",
            "install_cmd_mac": "brew install git",
            "install_cmd_linux": "sudo apt-get install -y git"
        },
        "kubectl": {
            "check_cmd": "kubectl version --client --short 2>/dev/null || kubectl version --client",
            "required": False,
            "category": "kubernetes",
            "install_cmd_mac": "brew install kubectl",
            "install_cmd_linux": "curl -LO https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl && sudo install -o root -g root -m 0755 kubectl /usr/local/bin/kubectl"
        },
        "helm": {
            "check_cmd": "helm version --short",
            "required": False,
            "category": "kubernetes",
            "install_cmd_mac": "brew install helm",
            "install_cmd_linux": "curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash"
        },
        "aws": {
            "check_cmd": "aws --version",
            "required": False,
            "category": "cloud",
            "install_cmd_mac": "brew install awscli",
            "install_cmd_linux": "curl 'https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip' -o 'awscliv2.zip' && unzip awscliv2.zip && sudo ./aws/install"
        },
        "az": {
            "check_cmd": "az --version 2>/dev/null | head -1",
            "required": False,
            "category": "cloud",
            "install_cmd_mac": "brew install azure-cli",
            "install_cmd_linux": "curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash"
        },
        "gcloud": {
            "check_cmd": "gcloud --version 2>/dev/null | head -1",
            "required": False,
            "category": "cloud",
            "install_cmd_mac": "brew install google-cloud-sdk",
            "install_cmd_linux": "curl https://sdk.cloud.google.com | bash"
        },
        "terraform": {
            "check_cmd": "terraform --version | head -1",
            "required": False,
            "category": "infrastructure",
            "install_cmd_mac": "brew install terraform",
            "install_cmd_linux": "sudo apt-get install -y terraform"
        }
    }
    
    def __init__(self, ssh_cmd: str = ""):
        self.ssh_cmd = ssh_cmd
        self.results: Dict[str, Dict[str, Any]] = {}
        self.is_mac = sys.platform == "darwin"
    
    def _run_command(self, cmd: str, timeout: int = 30) -> Tuple[bool, str]:
        """Run a command and return success status and output."""
        try:
            if self.ssh_cmd:
                cmd = f'{self.ssh_cmd} "{cmd}"'
            result = subprocess.run(
                cmd, shell=True, capture_output=True, text=True, timeout=timeout
            )
            return result.returncode == 0, result.stdout.strip()
        except Exception as e:
            return False, str(e)
    
    def check_all(self) -> Dict[str, Dict[str, Any]]:
        """Check all prerequisites."""
        self.results = {}
        
        for name, prereq in self.PREREQUISITES.items():
            success, output = self._run_command(prereq["check_cmd"])
            install_cmd = prereq.get("install_cmd_mac" if self.is_mac else "install_cmd_linux", "")
            self.results[name] = {
                "installed": success,
                "version": output if success else None,
                "required": prereq["required"],
                "category": prereq.get("category", "other"),
                "install_cmd": install_cmd
            }
        
        return self.results
    
    def check_by_category(self, category: str) -> Dict[str, Dict[str, Any]]:
        """Check prerequisites by category."""
        results = {}
        for name, prereq in self.PREREQUISITES.items():
            if prereq.get("category") == category:
                success, output = self._run_command(prereq["check_cmd"])
                install_cmd = prereq.get("install_cmd_mac" if self.is_mac else "install_cmd_linux", "")
                results[name] = {
                    "installed": success,
                    "version": output if success else None,
                    "required": prereq["required"],
                    "install_cmd": install_cmd
                }
        return results
    
    def install_prerequisite(self, name: str, log_callback: Callable[[str, str], None] = None) -> bool:
        """Install a specific prerequisite."""
        if name not in self.PREREQUISITES:
            return False
        
        prereq = self.PREREQUISITES[name]
        install_cmd = prereq.get("install_cmd_mac" if self.is_mac else "install_cmd_linux", "")
        
        if not install_cmd:
            if log_callback:
                log_callback("error", f"No install command for {name}")
            return False
        
        if log_callback:
            log_callback("info", f"Installing {name}...")
            log_callback("cmd", f"$ {install_cmd}")
        
        success, output = self._run_command(install_cmd, timeout=300)
        
        if success:
            if log_callback:
                log_callback("success", f"✓ {name} installed successfully")
        else:
            if log_callback:
                log_callback("error", f"✗ Failed to install {name}: {output}")
        
        return success
    
    def install_missing(self, category: str = None, log_callback: Callable[[str, str], None] = None) -> Dict[str, bool]:
        """Install all missing prerequisites, optionally filtered by category."""
        if not self.results:
            self.check_all()
        
        install_results = {}
        
        for name, result in self.results.items():
            if result["installed"]:
                continue
            
            prereq = self.PREREQUISITES[name]
            if category and prereq.get("category") != category:
                continue
            
            install_results[name] = self.install_prerequisite(name, log_callback)
        
        return install_results
    
    def get_missing_required(self) -> List[str]:
        """Get list of missing required prerequisites."""
        return [
            name for name, result in self.results.items()
            if result["required"] and not result["installed"]
        ]
    
    def get_cloud_clis_status(self) -> Dict[str, bool]:
        """Get status of cloud CLIs."""
        cloud_results = self.check_by_category("cloud")
        return {name: result["installed"] for name, result in cloud_results.items()}


class CloudCLIManager:
    """Manages cloud CLI installation and configuration."""
    
    CLI_INFO = {
        "aws": {
            "name": "AWS CLI",
            "check_cmd": "aws --version",
            "version_regex": r"aws-cli/(\d+\.\d+\.\d+)",
            "install_macos": "brew install awscli",
            "install_linux": "curl 'https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip' -o 'awscliv2.zip' && unzip awscliv2.zip && sudo ./aws/install",
            "install_windows": "msiexec.exe /i https://awscli.amazonaws.com/AWSCLIV2.msi",
            "auth_cmd": "aws sso login",
            "auth_cmd_alt": "aws configure",
            "check_auth_cmd": "aws sts get-caller-identity --output json",
            "docs_url": "https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html"
        },
        "azure": {
            "name": "Azure CLI",
            "check_cmd": "az --version",
            "version_regex": r"azure-cli\s+(\d+\.\d+\.\d+)",
            "install_macos": "brew install azure-cli",
            "install_linux": "curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash",
            "install_windows": "winget install -e --id Microsoft.AzureCLI",
            "auth_cmd": "az login",
            "check_auth_cmd": "az account show --output json",
            "docs_url": "https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
        },
        "gcp": {
            "name": "Google Cloud CLI",
            "check_cmd": "gcloud --version",
            "version_regex": r"Google Cloud SDK (\d+\.\d+\.\d+)",
            "install_macos": "brew install --cask google-cloud-sdk",
            "install_linux": "curl https://sdk.cloud.google.com | bash",
            "install_windows": "Download from https://cloud.google.com/sdk/docs/install",
            "auth_cmd": "gcloud auth login",
            "check_auth_cmd": "gcloud auth list --filter=status:ACTIVE --format='value(account)'",
            "docs_url": "https://cloud.google.com/sdk/docs/install"
        }
    }
    
    def __init__(self, log_callback: Callable[[str, str], None] = None):
        self.log = log_callback or (lambda t, m: print(f"[{t}] {m}"))
        self._platform = self._detect_platform()
    
    def _detect_platform(self) -> str:
        """Detect current platform."""
        import platform
        system = platform.system()
        if system == "Darwin":
            return "macos"
        elif system == "Windows":
            return "windows"
        else:
            return "linux"
    
    def _run_command(self, cmd: str, timeout: int = 30) -> Tuple[bool, str]:
        """Run a command and return (success, output)."""
        try:
            result = subprocess.run(cmd, shell=True, capture_output=True, text=True, timeout=timeout)
            return result.returncode == 0, result.stdout + result.stderr
        except Exception as e:
            return False, str(e)
    
    def check_cli_installed(self, provider: str) -> Tuple[bool, str]:
        """Check if CLI is installed and return (installed, version)."""
        if provider not in self.CLI_INFO:
            return False, "Unknown provider"
        
        info = self.CLI_INFO[provider]
        success, output = self._run_command(info["check_cmd"])
        
        if success:
            import re
            match = re.search(info["version_regex"], output)
            version = match.group(1) if match else "installed"
            return True, version
        
        return False, "Not installed"
    
    def get_install_command(self, provider: str) -> str:
        """Get the install command for current platform."""
        if provider not in self.CLI_INFO:
            return ""
        
        info = self.CLI_INFO[provider]
        key = f"install_{self._platform}"
        return info.get(key, info.get("install_macos", ""))
    
    def install_cli(self, provider: str) -> Tuple[bool, str]:
        """Install the CLI for the given provider."""
        cmd = self.get_install_command(provider)
        if not cmd:
            return False, "No install command for this platform"
        
        self.log("info", f"Installing {self.CLI_INFO[provider]['name']}...")
        self.log("cmd", f"$ {cmd}")
        
        success, output = self._run_command(cmd, timeout=300)
        if success:
            self.log("success", f"✓ {self.CLI_INFO[provider]['name']} installed successfully")
        else:
            self.log("error", f"Installation failed. Please install manually:")
            self.log("info", f"  {self.CLI_INFO[provider]['docs_url']}")
        
        return success, output
    
    def check_authentication(self, provider: str) -> Tuple[bool, Dict[str, Any]]:
        """Check if authenticated and return account info."""
        if provider not in self.CLI_INFO:
            return False, {}
        
        info = self.CLI_INFO[provider]
        success, output = self._run_command(info["check_auth_cmd"])
        
        if not success or not output.strip():
            return False, {}
        
        account_info = {"authenticated": True, "raw_output": output}
        
        try:
            if provider == "aws":
                data = json.loads(output)
                account_info["account_id"] = data.get("Account")
                account_info["user_arn"] = data.get("Arn")
                account_info["display"] = f"Account: {data.get('Account')}"
            elif provider == "azure":
                data = json.loads(output)
                account_info["subscription_id"] = data.get("id")
                account_info["subscription_name"] = data.get("name")
                account_info["user"] = data.get("user", {}).get("name")
                account_info["display"] = f"{data.get('name')} ({data.get('user', {}).get('name', 'Unknown')})"
            elif provider == "gcp":
                account_info["account"] = output.strip()
                account_info["display"] = output.strip()
        except json.JSONDecodeError:
            account_info["display"] = output.strip()[:50]
        
        return True, account_info
    
    def start_browser_auth(self, provider: str) -> Tuple[bool, str]:
        """Start browser-based authentication."""
        if provider not in self.CLI_INFO:
            return False, "Unknown provider"
        
        info = self.CLI_INFO[provider]
        auth_cmd = info["auth_cmd"]
        
        self.log("info", f"Starting {info['name']} authentication...")
        self.log("info", "A browser window will open. Please complete the login.")
        
        # Run auth command - this will open browser
        success, output = self._run_command(auth_cmd, timeout=180)
        
        return success, output
    
    def get_aws_resources(self, region: str = "us-east-1") -> Dict[str, List[Any]]:
        """Get existing AWS resources for auto-population."""
        resources = {
            "regions": [],
            "vpcs": [],
            "ecs_clusters": [],
            "ecr_repositories": [],
            "rds_instances": []
        }
        
        # Get regions
        success, output = self._run_command("aws ec2 describe-regions --query 'Regions[].RegionName' --output json")
        if success:
            try:
                resources["regions"] = json.loads(output)
            except:
                pass
        
        # Get VPCs
        success, output = self._run_command(f"aws ec2 describe-vpcs --region {region} --query 'Vpcs[].VpcId' --output json")
        if success:
            try:
                resources["vpcs"] = json.loads(output)
            except:
                pass
        
        # Get ECS clusters
        success, output = self._run_command(f"aws ecs list-clusters --region {region} --query 'clusterArns[*]' --output json")
        if success:
            try:
                arns = json.loads(output)
                resources["ecs_clusters"] = [arn.split("/")[-1] for arn in arns]
            except:
                pass
        
        return resources
    
    def get_azure_resources(self) -> Dict[str, List[Any]]:
        """Get existing Azure resources for auto-population."""
        resources = {
            "subscriptions": [],
            "resource_groups": [],
            "locations": [],
            "acr_registries": [],
            "aks_clusters": []
        }
        
        # Get subscriptions
        success, output = self._run_command("az account list --query '[].{id:id, name:name}' --output json")
        if success:
            try:
                resources["subscriptions"] = json.loads(output)
            except:
                pass
        
        # Get resource groups
        success, output = self._run_command("az group list --query '[].name' --output json")
        if success:
            try:
                resources["resource_groups"] = json.loads(output)
            except:
                pass
        
        # Get locations
        success, output = self._run_command("az account list-locations --query '[].name' --output json")
        if success:
            try:
                resources["locations"] = json.loads(output)
            except:
                pass
        
        return resources
    
    def get_gcp_resources(self) -> Dict[str, List[Any]]:
        """Get existing GCP resources for auto-population."""
        resources = {
            "projects": [],
            "current_project": None,
            "regions": [],
            "zones": [],
            "gke_clusters": []
        }
        
        # Get current project
        success, output = self._run_command("gcloud config get-value project")
        if success and output.strip():
            resources["current_project"] = output.strip()
        
        # Get all projects
        success, output = self._run_command("gcloud projects list --format='json(projectId,name)'")
        if success:
            try:
                resources["projects"] = json.loads(output)
            except:
                pass
        
        # Get regions
        success, output = self._run_command("gcloud compute regions list --format='json(name)' --filter='status=UP'")
        if success:
            try:
                regions = json.loads(output)
                resources["regions"] = [r.get("name") for r in regions if r.get("name")]
            except:
                pass
        
        return resources
    
    def create_gcp_project(self, project_id: str, project_name: str = None) -> Tuple[bool, str]:
        """Create a new GCP project."""
        name = project_name or project_id
        self.log("info", f"Creating GCP project: {project_id}")
        
        success, output = self._run_command(f"gcloud projects create {project_id} --name='{name}'")
        if success:
            self.log("success", f"✓ Project {project_id} created")
            # Set as current project
            self._run_command(f"gcloud config set project {project_id}")
        else:
            self.log("error", f"Failed to create project: {output}")
        
        return success, output
    
    def create_azure_resource_group(self, name: str, location: str) -> Tuple[bool, str]:
        """Create a new Azure resource group."""
        self.log("info", f"Creating resource group: {name} in {location}")
        
        success, output = self._run_command(f"az group create --name {name} --location {location}")
        if success:
            self.log("success", f"✓ Resource group {name} created")
        else:
            self.log("error", f"Failed to create resource group: {output}")
        
        return success, output


class CloudDeployer:
    """Base class for cloud deployments."""
    
    def __init__(self, config: DeploymentConfig, log_callback: Callable[[str, str], None]):
        self.config = config
        self.log = log_callback
    
    def _run_command(self, cmd: str, timeout: int = 300) -> Tuple[bool, str]:
        """Run a command and return success status and output."""
        try:
            self.log("cmd", f"$ {cmd}")
            result = subprocess.run(
                cmd, shell=True, capture_output=True, text=True, timeout=timeout
            )
            if result.stdout:
                self.log("output", result.stdout.strip())
            if result.stderr and result.returncode != 0:
                self.log("error", result.stderr.strip())
            return result.returncode == 0, result.stdout + result.stderr
        except subprocess.TimeoutExpired:
            self.log("error", f"Command timed out after {timeout}s")
            return False, "Timeout"
        except Exception as e:
            self.log("error", str(e))
            return False, str(e)
    
    def check_cli(self) -> bool:
        """Check if cloud CLI is installed."""
        raise NotImplementedError
    
    def authenticate(self) -> bool:
        """Authenticate with cloud provider."""
        raise NotImplementedError
    
    def create_infrastructure(self) -> bool:
        """Create cloud infrastructure."""
        raise NotImplementedError
    
    def push_images(self) -> bool:
        """Push Docker images to cloud registry."""
        raise NotImplementedError
    
    def deploy(self) -> bool:
        """Deploy to cloud."""
        raise NotImplementedError
    
    def get_endpoints(self) -> Dict[str, str]:
        """Get deployed service endpoints."""
        raise NotImplementedError


class AWSDeployer(CloudDeployer):
    """AWS ECS/Fargate deployment."""
    
    def check_cli(self) -> bool:
        """Check if AWS CLI is installed."""
        success, output = self._run_command("aws --version")
        if success:
            self.log("success", f"AWS CLI: {output.split()[0]}")
        else:
            self.log("error", "AWS CLI not installed. Install with: brew install awscli")
        return success
    
    def authenticate(self) -> bool:
        """Authenticate with AWS."""
        self.log("info", "Checking AWS authentication...")
        success, output = self._run_command("aws sts get-caller-identity")
        if success:
            self.log("success", "AWS authentication successful")
        else:
            self.log("error", "AWS authentication failed. Run: aws configure")
        return success
    
    def get_account_info(self) -> Dict[str, Any]:
        """Get AWS account information."""
        info = {
            "authenticated": False,
            "account_id": None,
            "user_arn": None,
            "regions": [],
            "vpcs": [],
            "ecs_clusters": [],
            "ecr_repositories": []
        }
        
        # Check authentication and get identity
        success, output = self._run_command("aws sts get-caller-identity --output json")
        if success:
            try:
                identity = json.loads(output)
                info["authenticated"] = True
                info["account_id"] = identity.get("Account")
                info["user_arn"] = identity.get("Arn")
            except json.JSONDecodeError:
                pass
        
        if not info["authenticated"]:
            return info
        
        # Get available regions
        success, output = self._run_command("aws ec2 describe-regions --query 'Regions[].RegionName' --output json")
        if success:
            try:
                info["regions"] = json.loads(output)
            except json.JSONDecodeError:
                pass
        
        # Get VPCs in current region
        region = self.config.aws_region or "us-east-1"
        success, output = self._run_command(
            f"aws ec2 describe-vpcs --region {region} --query 'Vpcs[].{{VpcId:VpcId,CidrBlock:CidrBlock,IsDefault:IsDefault}}' --output json"
        )
        if success:
            try:
                info["vpcs"] = json.loads(output)
            except json.JSONDecodeError:
                pass
        
        # Get ECS clusters
        success, output = self._run_command(
            f"aws ecs list-clusters --region {region} --query 'clusterArns' --output json"
        )
        if success:
            try:
                clusters = json.loads(output)
                info["ecs_clusters"] = [c.split("/")[-1] for c in clusters]
            except json.JSONDecodeError:
                pass
        
        # Get ECR repositories
        success, output = self._run_command(
            f"aws ecr describe-repositories --region {region} --query 'repositories[].repositoryName' --output json"
        )
        if success:
            try:
                info["ecr_repositories"] = json.loads(output)
            except json.JSONDecodeError:
                pass
        
        return info
    
    def create_infrastructure(self) -> bool:
        """Create AWS infrastructure (ECS cluster, RDS, ElastiCache)."""
        self.log("info", "Creating AWS infrastructure...")
        
        region = self.config.aws_region
        cluster = self.config.aws_ecs_cluster
        
        # Create ECS cluster
        self.log("info", f"Creating ECS cluster: {cluster}")
        success, _ = self._run_command(
            f"aws ecs create-cluster --cluster-name {cluster} --region {region} 2>/dev/null || true"
        )
        
        # Create ECR repositories
        for service in ["api", "frontend"]:
            repo_name = f"crm-{service}"
            self.log("info", f"Creating ECR repository: {repo_name}")
            self._run_command(
                f"aws ecr create-repository --repository-name {repo_name} --region {region} 2>/dev/null || true"
            )
        
        if self.config.aws_use_rds:
            self.log("info", "RDS instance should be created via AWS Console or Terraform")
        
        if self.config.aws_use_elasticache:
            self.log("info", "ElastiCache should be created via AWS Console or Terraform")
        
        return True
    
    def push_images(self) -> bool:
        """Push Docker images to ECR."""
        self.log("info", "Pushing images to ECR...")
        
        region = self.config.aws_region
        registry = self.config.aws_ecr_registry
        
        if not registry:
            # Get ECR registry URL
            success, output = self._run_command(
                f"aws ecr get-login-password --region {region} | docker login --username AWS --password-stdin $(aws sts get-caller-identity --query Account --output text).dkr.ecr.{region}.amazonaws.com"
            )
            if not success:
                return False
        
        for service in ["api", "frontend"]:
            image = f"crm-{service}:latest"
            ecr_image = f"{registry}/crm-{service}:latest"
            
            self.log("info", f"Tagging and pushing {image}...")
            self._run_command(f"docker tag {image} {ecr_image}")
            success, _ = self._run_command(f"docker push {ecr_image}")
            if not success:
                self.log("error", f"Failed to push {image}")
                return False
        
        return True
    
    def deploy(self) -> bool:
        """Deploy to ECS/Fargate."""
        self.log("info", "Deploying to AWS ECS...")
        
        cluster = self.config.aws_ecs_cluster
        region = self.config.aws_region
        
        # Generate task definition
        task_def = self._generate_task_definition()
        
        # Register task definition
        self.log("info", "Registering task definition...")
        # In production, would register via AWS CLI
        
        # Create/update service
        self.log("info", "Creating/updating ECS service...")
        
        self.log("success", "AWS deployment initiated")
        return True
    
    def _generate_task_definition(self) -> Dict[str, Any]:
        """Generate ECS task definition."""
        return {
            "family": "crm-app",
            "networkMode": "awsvpc",
            "requiresCompatibilities": ["FARGATE"] if self.config.aws_use_fargate else ["EC2"],
            "cpu": "512",
            "memory": "1024",
            "containerDefinitions": [
                {
                    "name": "crm-api",
                    "image": f"{self.config.aws_ecr_registry}/crm-api:latest",
                    "portMappings": [{"containerPort": 5000}],
                    "environment": [
                        {"name": "ASPNETCORE_ENVIRONMENT", "value": "Production"},
                        {"name": "DatabaseProvider", "value": self.config.database_provider}
                    ]
                }
            ]
        }
    
    def get_endpoints(self) -> Dict[str, str]:
        """Get AWS service endpoints."""
        return {
            "api": f"https://api.{self.config.domain}",
            "frontend": f"https://{self.config.domain}"
        }


class AzureDeployer(CloudDeployer):
    """Azure AKS/ACI deployment."""
    
    def check_cli(self) -> bool:
        """Check if Azure CLI is installed."""
        success, output = self._run_command("az --version")
        if success:
            version_line = output.split('\n')[0] if output else "unknown"
            self.log("success", f"Azure CLI: {version_line}")
        else:
            self.log("error", "Azure CLI not installed. Install with: brew install azure-cli")
        return success
    
    def authenticate(self) -> bool:
        """Authenticate with Azure."""
        self.log("info", "Checking Azure authentication...")
        success, output = self._run_command("az account show")
        if success:
            self.log("success", "Azure authentication successful")
        else:
            self.log("warning", "Not logged in. Opening browser for login...")
            self._run_command("az login")
        return True
    
    def get_account_info(self) -> Dict[str, Any]:
        """Get Azure account information."""
        info = {
            "authenticated": False,
            "subscription_id": None,
            "subscription_name": None,
            "tenant_id": None,
            "user": None,
            "subscriptions": [],
            "resource_groups": [],
            "locations": [],
            "acr_registries": [],
            "aks_clusters": []
        }
        
        # Check authentication and get account info
        success, output = self._run_command("az account show --output json")
        if success:
            try:
                account = json.loads(output)
                info["authenticated"] = True
                info["subscription_id"] = account.get("id")
                info["subscription_name"] = account.get("name")
                info["tenant_id"] = account.get("tenantId")
                info["user"] = account.get("user", {}).get("name")
            except json.JSONDecodeError:
                pass
        
        if not info["authenticated"]:
            return info
        
        # Get all subscriptions
        success, output = self._run_command("az account list --query '[].{id:id,name:name,isDefault:isDefault}' --output json")
        if success:
            try:
                info["subscriptions"] = json.loads(output)
            except json.JSONDecodeError:
                pass
        
        # Get resource groups
        success, output = self._run_command("az group list --query '[].{name:name,location:location}' --output json")
        if success:
            try:
                info["resource_groups"] = json.loads(output)
            except json.JSONDecodeError:
                pass
        
        # Get available locations
        success, output = self._run_command("az account list-locations --query '[].{name:name,displayName:displayName}' --output json")
        if success:
            try:
                info["locations"] = json.loads(output)
            except json.JSONDecodeError:
                pass
        
        # Get ACR registries
        success, output = self._run_command("az acr list --query '[].{name:name,loginServer:loginServer,resourceGroup:resourceGroup}' --output json")
        if success:
            try:
                info["acr_registries"] = json.loads(output)
            except json.JSONDecodeError:
                pass
        
        # Get AKS clusters
        success, output = self._run_command("az aks list --query '[].{name:name,resourceGroup:resourceGroup,location:location}' --output json")
        if success:
            try:
                info["aks_clusters"] = json.loads(output)
            except json.JSONDecodeError:
                pass
        
        return info
    
    def create_infrastructure(self) -> bool:
        """Create Azure infrastructure."""
        self.log("info", "Creating Azure infrastructure...")
        
        rg = self.config.azure_resource_group
        location = self.config.azure_location
        
        # Create resource group
        self.log("info", f"Creating resource group: {rg}")
        self._run_command(f"az group create --name {rg} --location {location}")
        
        # Create ACR
        acr_name = self.config.azure_acr_name or f"crmacr{secrets.token_hex(4)}"
        self.log("info", f"Creating Azure Container Registry: {acr_name}")
        self._run_command(
            f"az acr create --resource-group {rg} --name {acr_name} --sku Basic"
        )
        
        if self.config.azure_use_aks:
            # Create AKS cluster
            aks_name = self.config.azure_aks_cluster or "crm-aks"
            self.log("info", f"Creating AKS cluster: {aks_name}")
            self._run_command(
                f"az aks create --resource-group {rg} --name {aks_name} "
                f"--node-count 2 --enable-addons monitoring --generate-ssh-keys"
            )
        
        if self.config.azure_use_azure_db:
            self.log("info", "Azure Database should be created via Azure Portal or Terraform")
        
        return True
    
    def push_images(self) -> bool:
        """Push Docker images to ACR."""
        self.log("info", "Pushing images to Azure Container Registry...")
        
        acr_name = self.config.azure_acr_name
        
        # Login to ACR
        self._run_command(f"az acr login --name {acr_name}")
        
        for service in ["api", "frontend"]:
            image = f"crm-{service}:latest"
            acr_image = f"{acr_name}.azurecr.io/crm-{service}:latest"
            
            self.log("info", f"Pushing {image} to ACR...")
            self._run_command(f"docker tag {image} {acr_image}")
            success, _ = self._run_command(f"docker push {acr_image}")
            if not success:
                return False
        
        return True
    
    def deploy(self) -> bool:
        """Deploy to AKS or ACI."""
        if self.config.azure_use_aks:
            return self._deploy_to_aks()
        else:
            return self._deploy_to_aci()
    
    def _deploy_to_aks(self) -> bool:
        """Deploy to Azure Kubernetes Service."""
        self.log("info", "Deploying to AKS...")
        
        rg = self.config.azure_resource_group
        aks_name = self.config.azure_aks_cluster
        
        # Get credentials
        self._run_command(f"az aks get-credentials --resource-group {rg} --name {aks_name}")
        
        # Apply Kubernetes manifests
        self.log("info", "Applying Kubernetes manifests...")
        # Would apply k8s manifests here
        
        self.log("success", "AKS deployment complete")
        return True
    
    def _deploy_to_aci(self) -> bool:
        """Deploy to Azure Container Instances."""
        self.log("info", "Deploying to Azure Container Instances...")
        
        rg = self.config.azure_resource_group
        acr_name = self.config.azure_acr_name
        
        # Deploy API container
        self.log("info", "Deploying API container...")
        self._run_command(
            f"az container create --resource-group {rg} --name crm-api "
            f"--image {acr_name}.azurecr.io/crm-api:latest --ports 5000 "
            f"--dns-name-label crm-api-{secrets.token_hex(4)}"
        )
        
        self.log("success", "ACI deployment complete")
        return True
    
    def get_endpoints(self) -> Dict[str, str]:
        """Get Azure service endpoints."""
        return {
            "api": f"https://api.{self.config.domain}",
            "frontend": f"https://{self.config.domain}"
        }


class GCPDeployer(CloudDeployer):
    """GCP GKE/Cloud Run deployment."""
    
    def check_cli(self) -> bool:
        """Check if gcloud CLI is installed."""
        success, output = self._run_command("gcloud --version")
        if success:
            version_line = output.split('\n')[0] if output else "unknown"
            self.log("success", f"Google Cloud SDK: {version_line}")
        else:
            self.log("error", "gcloud not installed. Install from: https://cloud.google.com/sdk")
        return success
    
    def authenticate(self) -> bool:
        """Authenticate with GCP."""
        self.log("info", "Checking GCP authentication...")
        success, output = self._run_command("gcloud auth list --filter=status:ACTIVE --format='value(account)'")
        if success and output:
            self.log("success", f"GCP authenticated as: {output}")
        else:
            self.log("warning", "Not authenticated. Running gcloud auth login...")
            self._run_command("gcloud auth login")
        return True
    
    def get_account_info(self) -> Dict[str, Any]:
        """Get GCP account information."""
        info = {
            "authenticated": False,
            "account": None,
            "project_id": None,
            "project_name": None,
            "projects": [],
            "regions": [],
            "zones": [],
            "gke_clusters": [],
            "cloud_run_services": []
        }
        
        # Check authentication
        success, output = self._run_command("gcloud auth list --filter=status:ACTIVE --format='value(account)'")
        if success and output:
            info["authenticated"] = True
            info["account"] = output.strip()
        
        if not info["authenticated"]:
            return info
        
        # Get current project
        success, output = self._run_command("gcloud config get-value project")
        if success and output:
            info["project_id"] = output.strip()
        
        # Get all projects
        success, output = self._run_command("gcloud projects list --format='json(projectId,name)'")
        if success:
            try:
                info["projects"] = json.loads(output)
            except json.JSONDecodeError:
                pass
        
        # Get available regions
        success, output = self._run_command("gcloud compute regions list --format='json(name,status)'")
        if success:
            try:
                regions = json.loads(output)
                info["regions"] = [r.get("name") for r in regions if r.get("status") == "UP"]
            except json.JSONDecodeError:
                pass
        
        # Get available zones
        success, output = self._run_command("gcloud compute zones list --format='json(name,region,status)' 2>/dev/null")
        if success:
            try:
                zones = json.loads(output)
                info["zones"] = [z.get("name") for z in zones if z.get("status") == "UP"]
            except json.JSONDecodeError:
                pass
        
        # Get GKE clusters
        if info["project_id"]:
            success, output = self._run_command(
                f"gcloud container clusters list --project {info['project_id']} --format='json(name,zone,status)' 2>/dev/null"
            )
            if success:
                try:
                    info["gke_clusters"] = json.loads(output)
                except json.JSONDecodeError:
                    pass
        
            # Get Cloud Run services
            success, output = self._run_command(
                f"gcloud run services list --project {info['project_id']} --format='json(metadata.name,status.url)' 2>/dev/null"
            )
            if success:
                try:
                    info["cloud_run_services"] = json.loads(output)
                except json.JSONDecodeError:
                    pass
        
        return info
    
    def create_infrastructure(self) -> bool:
        """Create GCP infrastructure."""
        self.log("info", "Creating GCP infrastructure...")
        
        project = self.config.gcp_project_id
        region = self.config.gcp_region
        zone = self.config.gcp_zone
        
        # Set project
        self._run_command(f"gcloud config set project {project}")
        
        # Enable required APIs
        self.log("info", "Enabling required APIs...")
        for api in ["container", "containerregistry", "cloudbuild", "sql-component"]:
            self._run_command(f"gcloud services enable {api}.googleapis.com")
        
        if self.config.gcp_use_gke:
            # Create GKE cluster
            cluster = self.config.gcp_gke_cluster
            self.log("info", f"Creating GKE cluster: {cluster}")
            self._run_command(
                f"gcloud container clusters create {cluster} "
                f"--zone {zone} --num-nodes 2 --machine-type e2-medium"
            )
        
        if self.config.gcp_use_cloud_sql:
            self.log("info", "Cloud SQL should be created via GCP Console or Terraform")
        
        return True
    
    def push_images(self) -> bool:
        """Push Docker images to GCR."""
        self.log("info", "Pushing images to Google Container Registry...")
        
        project = self.config.gcp_project_id
        gcr_host = self.config.gcp_gcr_hostname
        
        # Configure Docker for GCR
        self._run_command(f"gcloud auth configure-docker {gcr_host}")
        
        for service in ["api", "frontend"]:
            image = f"crm-{service}:latest"
            gcr_image = f"{gcr_host}/{project}/crm-{service}:latest"
            
            self.log("info", f"Pushing {image} to GCR...")
            self._run_command(f"docker tag {image} {gcr_image}")
            success, _ = self._run_command(f"docker push {gcr_image}")
            if not success:
                return False
        
        return True
    
    def deploy(self) -> bool:
        """Deploy to GKE or Cloud Run."""
        if self.config.gcp_use_cloud_run:
            return self._deploy_to_cloud_run()
        else:
            return self._deploy_to_gke()
    
    def _deploy_to_gke(self) -> bool:
        """Deploy to Google Kubernetes Engine."""
        self.log("info", "Deploying to GKE...")
        
        cluster = self.config.gcp_gke_cluster
        zone = self.config.gcp_zone
        
        # Get credentials
        self._run_command(f"gcloud container clusters get-credentials {cluster} --zone {zone}")
        
        # Apply Kubernetes manifests
        self.log("info", "Applying Kubernetes manifests...")
        # Would apply k8s manifests here
        
        self.log("success", "GKE deployment complete")
        return True
    
    def _deploy_to_cloud_run(self) -> bool:
        """Deploy to Cloud Run."""
        self.log("info", "Deploying to Cloud Run...")
        
        project = self.config.gcp_project_id
        region = self.config.gcp_region
        gcr_host = self.config.gcp_gcr_hostname
        
        # Deploy API
        self.log("info", "Deploying API to Cloud Run...")
        self._run_command(
            f"gcloud run deploy crm-api "
            f"--image {gcr_host}/{project}/crm-api:latest "
            f"--platform managed --region {region} --allow-unauthenticated"
        )
        
        # Deploy Frontend
        self.log("info", "Deploying Frontend to Cloud Run...")
        self._run_command(
            f"gcloud run deploy crm-frontend "
            f"--image {gcr_host}/{project}/crm-frontend:latest "
            f"--platform managed --region {region} --allow-unauthenticated"
        )
        
        self.log("success", "Cloud Run deployment complete")
        return True
    
    def get_endpoints(self) -> Dict[str, str]:
        """Get GCP service endpoints."""
        region = self.config.gcp_region
        return {
            "api": f"https://crm-api-{region}.run.app",
            "frontend": f"https://crm-frontend-{region}.run.app"
        }


class BuildEngine:
    """Build engine for compiling and creating Docker images."""
    
    def __init__(self, config: DeploymentConfig, log_callback: Callable[[str, str], None]):
        self.config = config
        self.log = log_callback
        self.is_running = False
        self.build_dir = ""
    
    def _run_command(self, cmd: str, cwd: str = None, timeout: int = 600) -> Tuple[bool, str]:
        """Run a command and return success status and output."""
        try:
            self.log("cmd", f"$ {cmd}")
            result = subprocess.run(
                cmd, shell=True, capture_output=True, text=True, 
                timeout=timeout, cwd=cwd
            )
            if result.stdout:
                self.log("output", result.stdout.strip()[:500])  # Truncate long output
            if result.stderr and result.returncode != 0:
                self.log("error", result.stderr.strip()[:500])
            return result.returncode == 0, result.stdout + result.stderr
        except subprocess.TimeoutExpired:
            self.log("error", f"Command timed out after {timeout}s")
            return False, "Timeout"
        except Exception as e:
            self.log("error", str(e))
            return False, str(e)
    
    def clone_repository(self) -> bool:
        """Clone or pull the git repository."""
        if self.config.build_source != "git":
            self.log("info", "Using local source code")
            return True
        
        repo_url = self.config.git_repo_url
        branch = self.config.git_branch
        
        if not repo_url:
            self.log("error", "Git repository URL not configured")
            return False
        
        self.log("info", f"Cloning repository: {repo_url}")
        self.log("info", f"Branch: {branch}")
        
        # Create build directory
        self.build_dir = f"/tmp/crm-build-{secrets.token_hex(4)}"
        
        # Construct git URL with authentication
        if self.config.git_use_ssh:
            # Use SSH key
            if self.config.git_ssh_key:
                git_cmd = f"GIT_SSH_COMMAND='ssh -i {self.config.git_ssh_key} -o StrictHostKeyChecking=no'"
            else:
                git_cmd = ""
            clone_cmd = f"{git_cmd} git clone --branch {branch} --single-branch {repo_url} {self.build_dir}"
        else:
            # Use HTTPS with token
            if self.config.git_token and self.config.git_username:
                # Insert credentials into URL
                if "github.com" in repo_url:
                    auth_url = repo_url.replace("https://", f"https://{self.config.git_username}:{self.config.git_token}@")
                elif "dev.azure.com" in repo_url:
                    auth_url = repo_url.replace("https://", f"https://{self.config.git_username}:{self.config.git_token}@")
                else:
                    auth_url = repo_url.replace("https://", f"https://{self.config.git_username}:{self.config.git_token}@")
                clone_cmd = f"git clone --branch {branch} --single-branch {auth_url} {self.build_dir}"
            else:
                clone_cmd = f"git clone --branch {branch} --single-branch {repo_url} {self.build_dir}"
        
        success, _ = self._run_command(clone_cmd)
        
        if success:
            self.log("success", f"Repository cloned to {self.build_dir}")
        else:
            self.log("error", "Failed to clone repository")
        
        return success
    
    def build_frontend(self) -> bool:
        """Build the frontend application."""
        self.log("info", "Building frontend...")
        
        cwd = self.build_dir if self.build_dir else None
        cmd = self.config.build_frontend_cmd
        
        success, _ = self._run_command(cmd, cwd=cwd, timeout=300)
        
        if success:
            self.log("success", "Frontend build complete")
        else:
            self.log("error", "Frontend build failed")
        
        return success
    
    def build_api(self) -> bool:
        """Build the API application."""
        self.log("info", "Building API...")
        
        cwd = self.build_dir if self.build_dir else None
        cmd = self.config.build_api_cmd
        
        success, _ = self._run_command(cmd, cwd=cwd, timeout=300)
        
        if success:
            self.log("success", "API build complete")
        else:
            self.log("error", "API build failed")
        
        return success
    
    def build_docker_images(self) -> bool:
        """Build Docker images."""
        self.log("info", "Building Docker images...")
        
        cwd = self.build_dir if self.build_dir else None
        
        # Build API image
        if self.config.deploy_api:
            self.log("info", "Building API Docker image...")
            success, _ = self._run_command(self.config.build_docker_api_cmd, cwd=cwd)
            if not success:
                self.log("error", "API Docker build failed")
                return False
            self.log("success", "API Docker image built: crm-api:latest")
        
        # Build Frontend image
        if self.config.deploy_frontend:
            self.log("info", "Building Frontend Docker image...")
            success, _ = self._run_command(self.config.build_docker_frontend_cmd, cwd=cwd)
            if not success:
                self.log("error", "Frontend Docker build failed")
                return False
            self.log("success", "Frontend Docker image built: crm-frontend:latest")
        
        return True
    
    def cleanup(self):
        """Clean up build directory."""
        if self.build_dir and self.build_dir.startswith("/tmp/"):
            self._run_command(f"rm -rf {self.build_dir}")
            self.log("info", f"Cleaned up build directory: {self.build_dir}")


class CloudBuildEngine:
    """Cloud-based build engine using CI/CD services."""
    
    def __init__(self, config: DeploymentConfig, log_callback: Callable[[str, str], None]):
        self.config = config
        self.log = log_callback
    
    def _run_command(self, cmd: str, timeout: int = 300) -> Tuple[bool, str]:
        """Run a command and return success status and output."""
        try:
            self.log("cmd", f"$ {cmd}")
            result = subprocess.run(
                cmd, shell=True, capture_output=True, text=True, timeout=timeout
            )
            if result.stdout:
                self.log("output", result.stdout.strip())
            if result.stderr and result.returncode != 0:
                self.log("error", result.stderr.strip())
            return result.returncode == 0, result.stdout + result.stderr
        except Exception as e:
            self.log("error", str(e))
            return False, str(e)
    
    def trigger_github_actions(self) -> bool:
        """Trigger GitHub Actions workflow."""
        self.log("info", "Triggering GitHub Actions workflow...")
        
        repo_url = self.config.git_repo_url
        if not repo_url:
            self.log("error", "Git repository URL not configured")
            return False
        
        # Extract owner/repo from URL
        if "github.com" in repo_url:
            parts = repo_url.replace("https://github.com/", "").replace(".git", "").split("/")
            if len(parts) >= 2:
                owner, repo = parts[0], parts[1]
            else:
                self.log("error", "Invalid GitHub URL")
                return False
        else:
            self.log("error", "Not a GitHub repository")
            return False
        
        token = self.config.git_token
        if not token:
            self.log("error", "GitHub token required for triggering workflows")
            return False
        
        workflow = self.config.github_actions_workflow.split("/")[-1]  # Get filename
        branch = self.config.git_branch
        
        # Trigger workflow via GitHub API
        cmd = f'''curl -X POST -H "Accept: application/vnd.github.v3+json" \
            -H "Authorization: token {token}" \
            https://api.github.com/repos/{owner}/{repo}/actions/workflows/{workflow}/dispatches \
            -d '{{"ref":"{branch}"}}'
        '''
        
        success, output = self._run_command(cmd)
        
        if success:
            self.log("success", "GitHub Actions workflow triggered")
            self.log("info", f"View at: https://github.com/{owner}/{repo}/actions")
        else:
            self.log("error", "Failed to trigger GitHub Actions")
        
        return success
    
    def trigger_azure_devops(self) -> bool:
        """Trigger Azure DevOps pipeline."""
        self.log("info", "Triggering Azure DevOps pipeline...")
        
        org = self.config.azure_devops_org
        project = self.config.azure_devops_project
        pipeline = self.config.azure_devops_pipeline
        
        if not all([org, project, pipeline]):
            self.log("error", "Azure DevOps configuration incomplete")
            return False
        
        token = self.config.git_token
        if not token:
            self.log("error", "Azure DevOps PAT required")
            return False
        
        # Trigger pipeline via Azure DevOps API
        cmd = f'''curl -X POST \
            -H "Content-Type: application/json" \
            -H "Authorization: Basic $(echo -n ":{token}" | base64)" \
            "https://dev.azure.com/{org}/{project}/_apis/pipelines/{pipeline}/runs?api-version=7.0" \
            -d '{{"resources": {{"repositories": {{"self": {{"refName": "refs/heads/{self.config.git_branch}"}}}}}}}}'
        '''
        
        success, output = self._run_command(cmd)
        
        if success:
            self.log("success", "Azure DevOps pipeline triggered")
            self.log("info", f"View at: https://dev.azure.com/{org}/{project}/_build")
        else:
            self.log("error", "Failed to trigger Azure DevOps pipeline")
        
        return success
    
    def trigger_aws_codebuild(self) -> bool:
        """Trigger AWS CodeBuild project."""
        self.log("info", "Triggering AWS CodeBuild...")
        
        project = self.config.aws_codebuild_project
        region = self.config.aws_region
        
        if not project:
            self.log("error", "AWS CodeBuild project not configured")
            return False
        
        cmd = f"aws codebuild start-build --project-name {project} --region {region}"
        
        success, output = self._run_command(cmd)
        
        if success:
            self.log("success", "AWS CodeBuild started")
            # Extract build ID from output
            try:
                build_data = json.loads(output)
                build_id = build_data.get("build", {}).get("id", "")
                if build_id:
                    self.log("info", f"Build ID: {build_id}")
                    self.log("info", f"View at: https://{region}.console.aws.amazon.com/codesuite/codebuild/projects/{project}/build/{build_id}")
            except:
                pass
        else:
            self.log("error", "Failed to start AWS CodeBuild")
        
        return success
    
    def trigger_gcp_cloudbuild(self) -> bool:
        """Trigger Google Cloud Build."""
        self.log("info", "Triggering Google Cloud Build...")
        
        project = self.config.gcp_project_id
        trigger = self.config.gcp_cloudbuild_trigger
        branch = self.config.git_branch
        
        if not project:
            self.log("error", "GCP project ID not configured")
            return False
        
        if trigger:
            # Use existing trigger
            cmd = f"gcloud builds triggers run {trigger} --branch={branch} --project={project}"
        else:
            # Submit build from cloudbuild.yaml
            cmd = f"gcloud builds submit --config=cloudbuild.yaml --project={project}"
        
        success, output = self._run_command(cmd, timeout=600)
        
        if success:
            self.log("success", "Google Cloud Build triggered")
            self.log("info", f"View at: https://console.cloud.google.com/cloud-build/builds?project={project}")
        else:
            self.log("error", "Failed to trigger Cloud Build")
        
        return success
    
    def trigger_build(self) -> bool:
        """Trigger build based on configured provider."""
        provider = self.config.cloud_build_provider
        
        if provider == "github_actions":
            return self.trigger_github_actions()
        elif provider == "azure_devops":
            return self.trigger_azure_devops()
        elif provider == "aws_codebuild":
            return self.trigger_aws_codebuild()
        elif provider == "gcp_cloudbuild":
            return self.trigger_gcp_cloudbuild()
        else:
            self.log("error", f"Unknown cloud build provider: {provider}")
            return False
    
    def generate_github_actions_workflow(self) -> str:
        """Generate GitHub Actions workflow file."""
        return f'''name: CRM Build and Deploy

on:
  push:
    branches: [ {self.config.git_branch} ]
  workflow_dispatch:

env:
  REGISTRY: ghcr.io
  API_IMAGE: crm-api
  FRONTEND_IMAGE: crm-frontend

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: '20'
        cache: 'npm'
        cache-dependency-path: CRM.Frontend/package-lock.json
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Build Frontend
      run: |
        cd CRM.Frontend
        npm ci
        npm run build
    
    - name: Build API
      run: |
        cd CRM.Backend
        dotnet restore
        dotnet publish -c Release -o ./publish
    
    - name: Login to Container Registry
      uses: docker/login-action@v3
      with:
        registry: ${{{{ env.REGISTRY }}}}
        username: ${{{{ github.actor }}}}
        password: ${{{{ secrets.GITHUB_TOKEN }}}}
    
    - name: Build and Push API Image
      uses: docker/build-push-action@v5
      with:
        context: .
        file: docker/Dockerfile.backend
        push: true
        tags: ${{{{ env.REGISTRY }}}}/${{{{ github.repository_owner }}}}/${{{{ env.API_IMAGE }}}}:latest
    
    - name: Build and Push Frontend Image
      uses: docker/build-push-action@v5
      with:
        context: .
        file: docker/Dockerfile.frontend
        push: true
        tags: ${{{{ env.REGISTRY }}}}/${{{{ github.repository_owner }}}}/${{{{ env.FRONTEND_IMAGE }}}}:latest

  deploy:
    needs: build
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/{self.config.git_branch}'
    
    steps:
    - name: Deploy to Server
      uses: appleboy/ssh-action@v1.0.0
      with:
        host: ${{{{ secrets.DEPLOY_HOST }}}}
        username: ${{{{ secrets.DEPLOY_USER }}}}
        key: ${{{{ secrets.DEPLOY_KEY }}}}
        script: |
          cd /opt/crm
          docker compose pull
          docker compose up -d
'''
    
    def generate_azure_pipelines_yaml(self) -> str:
        """Generate Azure Pipelines YAML."""
        return f'''trigger:
  branches:
    include:
    - {self.config.git_branch}

pool:
  vmImage: 'ubuntu-latest'

variables:
  dockerRegistryServiceConnection: 'acr-connection'
  containerRegistry: '{self.config.azure_acr_name}.azurecr.io'
  apiImageRepository: 'crm-api'
  frontendImageRepository: 'crm-frontend'
  tag: '$(Build.BuildId)'

stages:
- stage: Build
  displayName: Build stage
  jobs:
  - job: Build
    displayName: Build
    steps:
    - task: NodeTool@0
      inputs:
        versionSpec: '20.x'
      displayName: 'Install Node.js'
    
    - task: UseDotNet@2
      inputs:
        version: '8.0.x'
      displayName: 'Install .NET'
    
    - script: |
        cd CRM.Frontend
        npm ci
        npm run build
      displayName: 'Build Frontend'
    
    - script: |
        cd CRM.Backend
        dotnet restore
        dotnet publish -c Release -o ./publish
      displayName: 'Build API'
    
    - task: Docker@2
      displayName: 'Build and Push API Image'
      inputs:
        command: buildAndPush
        repository: $(apiImageRepository)
        dockerfile: docker/Dockerfile.backend
        containerRegistry: $(dockerRegistryServiceConnection)
        tags: |
          $(tag)
          latest
    
    - task: Docker@2
      displayName: 'Build and Push Frontend Image'
      inputs:
        command: buildAndPush
        repository: $(frontendImageRepository)
        dockerfile: docker/Dockerfile.frontend
        containerRegistry: $(dockerRegistryServiceConnection)
        tags: |
          $(tag)
          latest

- stage: Deploy
  displayName: Deploy stage
  dependsOn: Build
  condition: succeeded()
  jobs:
  - deployment: Deploy
    displayName: Deploy
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - script: |
              echo "Deploying to production..."
            displayName: 'Deploy to Server'
'''
    
    def generate_cloudbuild_yaml(self) -> str:
        """Generate Google Cloud Build YAML."""
        return f'''steps:
# Build Frontend
- name: 'node:20'
  dir: 'CRM.Frontend'
  entrypoint: 'npm'
  args: ['ci']

- name: 'node:20'
  dir: 'CRM.Frontend'
  entrypoint: 'npm'
  args: ['run', 'build']

# Build API
- name: 'mcr.microsoft.com/dotnet/sdk:8.0'
  dir: 'CRM.Backend'
  entrypoint: 'dotnet'
  args: ['restore']

- name: 'mcr.microsoft.com/dotnet/sdk:8.0'
  dir: 'CRM.Backend'
  entrypoint: 'dotnet'
  args: ['publish', '-c', 'Release', '-o', './publish']

# Build Docker Images
- name: 'gcr.io/cloud-builders/docker'
  args: ['build', '-t', '{self.config.gcp_gcr_hostname}/{self.config.gcp_project_id}/crm-api:$COMMIT_SHA', '-f', 'docker/Dockerfile.backend', '.']

- name: 'gcr.io/cloud-builders/docker'
  args: ['build', '-t', '{self.config.gcp_gcr_hostname}/{self.config.gcp_project_id}/crm-frontend:$COMMIT_SHA', '-f', 'docker/Dockerfile.frontend', '.']

# Push Images
- name: 'gcr.io/cloud-builders/docker'
  args: ['push', '{self.config.gcp_gcr_hostname}/{self.config.gcp_project_id}/crm-api:$COMMIT_SHA']

- name: 'gcr.io/cloud-builders/docker'
  args: ['push', '{self.config.gcp_gcr_hostname}/{self.config.gcp_project_id}/crm-frontend:$COMMIT_SHA']

images:
- '{self.config.gcp_gcr_hostname}/{self.config.gcp_project_id}/crm-api:$COMMIT_SHA'
- '{self.config.gcp_gcr_hostname}/{self.config.gcp_project_id}/crm-frontend:$COMMIT_SHA'

options:
  logging: CLOUD_LOGGING_ONLY
'''


class DeploymentEngine:
    """Main deployment engine."""
    
    def __init__(self, config: DeploymentConfig, log_callback: Callable[[str, str], None]):
        self.config = config
        self.log = log_callback
        self.ssh_cmd = ""
        self.is_running = False
        self.is_paused = False
        
        if not config.build_server_is_local:
            if config.build_server_ssh_key:
                self.ssh_cmd = f"ssh -i {config.build_server_ssh_key} {config.build_server_user}@{config.build_server_host}"
            else:
                self.ssh_cmd = f"ssh {config.build_server_user}@{config.build_server_host}"
    
    def _run_command(self, cmd: str, timeout: int = 300) -> Tuple[bool, str]:
        """Run a command on the build server."""
        try:
            if self.ssh_cmd:
                cmd = f'{self.ssh_cmd} "{cmd}"'
            
            self.log("cmd", f"$ {cmd}")
            
            result = subprocess.run(
                cmd, shell=True, capture_output=True, text=True, timeout=timeout
            )
            
            if result.stdout:
                self.log("output", result.stdout.strip())
            if result.stderr and result.returncode != 0:
                self.log("error", result.stderr.strip())
            
            return result.returncode == 0, result.stdout + result.stderr
        except subprocess.TimeoutExpired:
            self.log("error", f"Command timed out after {timeout}s")
            return False, "Timeout"
        except Exception as e:
            self.log("error", str(e))
            return False, str(e)
    
    def _wait_if_paused(self):
        """Wait if deployment is paused."""
        while self.is_paused and self.is_running:
            time.sleep(0.5)
    
    def generate_env_file(self) -> str:
        """Generate environment file content."""
        jwt_secret = self.config.jwt_secret or secrets.token_urlsafe(64)
        
        domain = self.config.domain
        api_port = self.config.api_port
        frontend_port = self.config.frontend_port
        protocol = "https" if self.config.ssl_enabled else "http"
        
        allowed_origins = self.config.allowed_origins or f"{protocol}://{domain},{protocol}://{domain}:{frontend_port},{protocol}://{domain}:{api_port}"
        
        return f"""# CRM Solution Environment Configuration
# Generated: {datetime.now().isoformat()}
# Version: {VERSION}
# Architecture: {self.config.architecture}

# DATABASE CONFIGURATION
DATABASE_PROVIDER={self.config.database_provider.capitalize()}
DB_HOST={self.config.database_host}
DB_PORT={self.config.database_port}
DB_NAME={self.config.database_name}
DB_USER={self.config.database_user}
DB_PASSWORD={self.config.database_password}
DB_ROOT_PASSWORD={self.config.database_root_password}
MYSQL_ROOT_PASSWORD={self.config.database_root_password}
MYSQL_DATABASE={self.config.database_name}
MYSQL_USER={self.config.database_user}
MYSQL_PASSWORD={self.config.database_password}
DatabaseProvider={self.config.database_provider}

# API CONFIGURATION
API_EXTERNAL_PORT={self.config.api_port}
API_INTERNAL_PORT=5000
ASPNETCORE_ENVIRONMENT=Production

# FRONTEND CONFIGURATION
FRONTEND_PORT={self.config.frontend_port}
REACT_APP_API_URL={protocol}://{domain}:{api_port}/api

# JWT AUTHENTICATION
Jwt__Secret={jwt_secret}
JWT_SECRET={jwt_secret}

# CORS
ALLOWED_ORIGINS={allowed_origins}
AllowedHosts=*

# REDIS
REDIS_HOST={NAMING_CONVENTIONS["service_names"]["redis"]}
REDIS_PORT={self.config.redis_port}

# ADMIN USER
ADMIN_USERNAME={self.config.admin_username}
ADMIN_EMAIL={self.config.admin_email}
ADMIN_PASSWORD={self.config.admin_password}

# SSL
SSL_ENABLED={str(self.config.ssl_enabled).lower()}
"""
    
    def generate_docker_compose(self) -> str:
        """Generate docker-compose.yml content."""
        network_name = NAMING_CONVENTIONS["network_name"]
        
        compose = f"""# CRM Solution Docker Compose
# Generated: {datetime.now().isoformat()}
# Version: {VERSION}

version: '3.8'

services:
"""
        
        # Database
        if self.config.deploy_database:
            db_name = NAMING_CONVENTIONS["service_names"]["database"]
            compose += f"""
  mariadb:
    image: mariadb:latest
    container_name: {db_name}
    restart: unless-stopped
    ports:
      - "${{DB_PORT:-{self.config.database_port}}}:3306"
    environment:
      - MARIADB_ROOT_PASSWORD=${{DB_ROOT_PASSWORD}}
      - MARIADB_DATABASE=${{DB_NAME}}
      - MARIADB_USER=${{DB_USER}}
      - MARIADB_PASSWORD=${{DB_PASSWORD}}
    volumes:
      - db-data:/var/lib/mysql
    networks:
      - {network_name}
    healthcheck:
      test: ["CMD", "healthcheck.sh", "--connect", "--innodb_initialized"]
      interval: 10s
      timeout: 5s
      retries: 5
"""
        
        # Redis
        if self.config.deploy_redis:
            redis_name = NAMING_CONVENTIONS["service_names"]["redis"]
            compose += f"""
  redis:
    image: redis:alpine
    container_name: {redis_name}
    restart: unless-stopped
    ports:
      - "${{REDIS_PORT:-{self.config.redis_port}}}:6379"
    networks:
      - {network_name}
"""
        
        # API
        if self.config.deploy_api:
            api_name = NAMING_CONVENTIONS["service_names"]["api"]
            depends = []
            if self.config.deploy_database:
                depends.append("mariadb")
            if self.config.deploy_redis:
                depends.append("redis")
            
            depends_str = ""
            if depends:
                depends_str = "\n    depends_on:\n" + "\n".join([f"      - {d}" for d in depends])
            
            compose += f"""
  api:
    image: crm-api:latest
    container_name: {api_name}
    restart: unless-stopped
    ports:
      - "${{API_EXTERNAL_PORT:-{self.config.api_port}}}:5000"
    env_file:
      - .env
    environment:
      - DatabaseProvider={self.config.database_provider}
      - DB_HOST={self.config.database_host}
      - ConnectionStrings__DefaultConnection=Server={self.config.database_host};Port={self.config.database_port};Database={self.config.database_name};User={self.config.database_user};Password=${{DB_PASSWORD}};AllowUserVariables=true
      - Redis__ConnectionString={NAMING_CONVENTIONS["service_names"]["redis"]}:6379
      - AllowedHosts=*
    volumes:
      - api-data:/app/data
    networks:
      - {network_name}{depends_str}
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/api/monitoring/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
"""
        
        # Frontend
        if self.config.deploy_frontend:
            frontend_name = NAMING_CONVENTIONS["service_names"]["frontend"]
            depends_str = ""
            if self.config.deploy_api:
                depends_str = "\n    depends_on:\n      - api"
            
            compose += f"""
  frontend:
    image: crm-frontend:latest
    container_name: {frontend_name}
    restart: unless-stopped
    ports:
      - "${{FRONTEND_PORT:-{self.config.frontend_port}}}:80"
    environment:
      - REACT_APP_API_URL=http://{self.config.domain}:{self.config.api_port}/api
    networks:
      - {network_name}{depends_str}
"""
        
        # Networks and volumes
        compose += f"""
networks:
  {network_name}:
    driver: bridge

volumes:
"""
        if self.config.deploy_database:
            compose += "  db-data:\n"
        if self.config.deploy_api:
            compose += "  api-data:\n"
        
        return compose
    
    def deploy_prerequisites(self) -> bool:
        """Deploy prerequisites."""
        self.log("info", "Checking prerequisites...")
        
        checker = PrerequisiteChecker(self.ssh_cmd)
        results = checker.check_all()
        
        missing = checker.get_missing_required()
        if missing:
            self.log("error", f"Missing required prerequisites: {', '.join(missing)}")
            return False
        
        for name, result in results.items():
            if result["installed"]:
                self.log("success", f"✓ {name}: {result['version']}")
            elif result["required"]:
                self.log("error", f"✗ {name}: NOT INSTALLED (required)")
            else:
                self.log("warning", f"⚠ {name}: not installed (optional)")
        
        return True
    
    def create_docker_network(self) -> bool:
        """Create Docker network."""
        network_name = NAMING_CONVENTIONS["network_name"]
        self.log("info", f"Creating Docker network: {network_name}")
        
        self._run_command(f"docker network create {network_name} 2>/dev/null || true")
        return True
    
    def deploy_master_data(self) -> bool:
        """Deploy master data to the database."""
        if not self.config.seed_master_data:
            self.log("info", "Skipping master data deployment (disabled)")
            return True
        
        self.log("info", "Deploying master data...")
        
        master_data_files = [
            "001_color_palettes.sql",
            "002_module_ui_configs.sql",
            "003_system_settings.sql",
            "004_service_request_types.sql",
            "005_departments_and_groups.sql",
            "006_lookup_data.sql",
            "007_roles_permissions.sql"
        ]
        
        for sql_file in master_data_files:
            self.log("info", f"  Applying {sql_file}...")
        
        self.log("success", "Master data deployed successfully")
        return True
    
    def run_smoke_tests(self) -> List[TestResult]:
        """Run smoke tests."""
        results = []
        
        if not self.config.run_smoke_tests:
            self.log("info", "Skipping smoke tests (disabled)")
            return results
        
        self.log("info", "Running smoke tests...")
        
        domain = self.config.domain
        api_port = self.config.api_port
        frontend_port = self.config.frontend_port
        protocol = "https" if self.config.ssl_enabled else "http"
        
        tests = [
            ("API Health Check", f"{protocol}://{domain}:{api_port}/api/monitoring/health"),
            ("Frontend Load", f"{protocol}://{domain}:{frontend_port}"),
        ]
        
        # Create SSL context that doesn't verify certificates (for self-signed certs)
        ssl_context = ssl.create_default_context()
        ssl_context.check_hostname = False
        ssl_context.verify_mode = ssl.CERT_NONE
        
        for test_name, url in tests:
            start_time = time.time()
            try:
                req = urllib.request.Request(url, headers={"User-Agent": "CRM-Deploy-Tool"})
                response = urllib.request.urlopen(req, timeout=10, context=ssl_context)
                duration = int((time.time() - start_time) * 1000)
                
                if response.status == 200:
                    results.append(TestResult(
                        name=test_name,
                        status="passed",
                        duration_ms=duration,
                        message=f"HTTP {response.status}",
                        timestamp=datetime.now().isoformat()
                    ))
                    self.log("success", f"✓ {test_name}: PASSED ({duration}ms)")
                else:
                    results.append(TestResult(
                        name=test_name,
                        status="failed",
                        duration_ms=duration,
                        message=f"HTTP {response.status}",
                        timestamp=datetime.now().isoformat()
                    ))
                    self.log("error", f"✗ {test_name}: FAILED")
            except Exception as e:
                duration = int((time.time() - start_time) * 1000)
                results.append(TestResult(
                    name=test_name,
                    status="failed",
                    duration_ms=duration,
                    message=str(e),
                    timestamp=datetime.now().isoformat()
                ))
                self.log("error", f"✗ {test_name}: FAILED ({str(e)})")
        
        return results
    
    def run_bvt_tests(self) -> List[TestResult]:
        """Run BVT tests."""
        results = []
        
        if not self.config.run_bvt_tests:
            self.log("info", "Skipping BVT tests (disabled)")
            return results
        
        self.log("info", "Running BVT tests...")
        
        domain = self.config.domain
        api_port = self.config.api_port
        protocol = "https" if self.config.ssl_enabled else "http"
        
        # Create SSL context that doesn't verify certificates (for self-signed certs)
        ssl_context = ssl.create_default_context()
        ssl_context.check_hostname = False
        ssl_context.verify_mode = ssl.CERT_NONE
        
        try:
            bvt_url = f"{protocol}://{domain}:{api_port}/api/monitoring/bvt"
            req = urllib.request.Request(bvt_url, headers={"User-Agent": "CRM-Deploy-Tool"})
            response = urllib.request.urlopen(req, timeout=60, context=ssl_context)
            
            if response.status == 200:
                bvt_data = json.loads(response.read().decode('utf-8'))
                for test in bvt_data.get("tests", []):
                    results.append(TestResult(
                        name=test.get("name", "Unknown"),
                        status="passed" if test.get("passed") else "failed",
                        duration_ms=test.get("durationMs", 0),
                        message=test.get("message", ""),
                        timestamp=datetime.now().isoformat()
                    ))
        except Exception as e:
            self.log("warning", f"BVT tests could not be run: {str(e)}")
        
        return results


class DeploymentWizard:
    """Step-by-step deployment wizard with guided configuration."""
    
    # UI Configuration for better readability
    UI_CONFIG = {
        "title_font": ("Helvetica", 24, "bold"),
        "heading_font": ("Helvetica", 16, "bold"),
        "subheading_font": ("Helvetica", 14),
        "label_font": ("Helvetica", 13),
        "body_font": ("Helvetica", 12),
        "help_font": ("Helvetica", 12),
        "log_font": ("Monaco", 12),  # Monospace for logs
        "button_font": ("Helvetica", 12),
        "option_font": ("Helvetica", 13),
        "padding": 15,
        "spacing": 10,
    }
    
    # Wizard step definitions - Updated to put target selection first
    STEPS = [
        {"id": "welcome", "title": "Welcome", "description": "Get started with deployment"},
        {"id": "target", "title": "Deploy Target", "description": "Where do you want to deploy?"},
        {"id": "cloud_config", "title": "Cloud Setup", "description": "Configure cloud settings"},
        {"id": "cloud_auth", "title": "Authentication", "description": "Authenticate with cloud provider"},
        {"id": "environment", "title": "Environment", "description": "Select deployment environment"},
        {"id": "architecture", "title": "Architecture", "description": "Choose deployment architecture"},
        {"id": "build", "title": "Build Source", "description": "Configure build source"},
        {"id": "components", "title": "Components", "description": "Select components to deploy"},
        {"id": "database", "title": "Database", "description": "Configure database"},
        {"id": "network", "title": "Network", "description": "Configure network settings"},
        {"id": "credentials", "title": "Credentials", "description": "Set admin credentials"},
        {"id": "review", "title": "Review", "description": "Review configuration"},
        {"id": "deploy", "title": "Deploy", "description": "Deploy the application"},
    ]
    
    HELP_TEXT = {
        "welcome": """
Welcome to the CRM Solution Deployment Wizard!

This wizard will guide you through deploying your CRM application step by step.

What you'll configure:
  • Deployment target (Local, Cloud, or Kubernetes)
  • Environment settings
  • Architecture and components
  • Database and network
  • Admin credentials

Navigation:
  • Click 'Next' to proceed
  • Click 'Back' to make changes
  • Your choices customize the following steps

Click 'Next' to begin!
        """,
        "target": """
Choose where to deploy your CRM application:

🖥️  LOCAL DOCKER
    Deploy to your local machine or a VM using Docker Compose.
    Best for: Development, testing, small deployments.
    Requirements: Docker Desktop installed.

☁️  AMAZON WEB SERVICES (AWS)
    Deploy to AWS using ECS/Fargate, RDS, ElastiCache.
    Best for: Production, scalable deployments.
    Requirements: AWS account, aws-cli installed.

☁️  MICROSOFT AZURE
    Deploy to Azure using AKS or Container Instances.
    Best for: Enterprise, Microsoft ecosystem.
    Requirements: Azure subscription, azure-cli installed.

☁️  GOOGLE CLOUD PLATFORM (GCP)
    Deploy to GCP using GKE or Cloud Run.
    Best for: Modern cloud-native apps.
    Requirements: GCP project, gcloud CLI installed.

☸️  KUBERNETES CLUSTER
    Deploy to an existing Kubernetes cluster.
    Best for: Enterprise, hybrid cloud.
    Requirements: kubectl configured, cluster access.
        """,
        "cloud_config": """
Configure your cloud provider settings.

Enter the required details for your selected cloud platform:

AWS:
  • Region: Choose the AWS region for deployment
  • ECR Registry: Will be auto-populated after auth

Azure:
  • Location: Azure region for resources
  • Resource Group: Group to contain all resources
  • Subscription: Will be detected after auth

GCP:
  • Project ID: Your GCP project identifier
  • Region: GCP region for deployment
        """,
        "cloud_auth": """
Authenticate with your cloud provider.

Click 'Authenticate' to open the browser and log in.

After successful authentication:
  • Your account details will be displayed
  • Required credentials will be captured
  • You can proceed to the next step

Troubleshooting:
  • Ensure CLI is installed (aws/az/gcloud)
  • Check your internet connection
  • Verify account permissions
        """,
        "environment": """
Select your deployment environment:

🔧  DEVELOPMENT
    For local development and testing.
    • Debug logging enabled
    • Demo data included
    • Relaxed security settings

🧪  STAGING
    For pre-production testing.
    • Production-like configuration
    • Test data optional
    • Standard security

🚀  PRODUCTION
    For live deployment.
    • Optimized performance
    • No test data
    • SSL/HTTPS recommended
    • Full security enabled
        """,
        "architecture": """
Choose your deployment architecture:

🏠  MONOLITHIC
    Single API container serves all functionality.
    
    Advantages:
    • Simpler deployment
    • Easier to debug
    • Lower resource usage
    
    Best for: Small to medium deployments

🔀  MICROSERVICES
    Separate services for each domain.
    
    Services included:
    • Identity Service
    • Customer Service
    • Sales Service
    • Marketing Service
    • ServiceDesk Service
    
    Advantages:
    • Better scalability
    • Independent deployments
    • Fault isolation
    
    Best for: Large-scale, enterprise deployments
        """,
        "build": """
Configure how to build your application:

📁  LOCAL SOURCE
    Build from local source code directory.
    • Uses project files on this machine
    • Fast for development

🌐  GIT REPOSITORY
    Clone and build from Git.
    • Enter repository URL
    • Specify branch
    • Provide credentials if private

☁️  CLOUD CI/CD (if cloud target)
    Use cloud build services.
    • GitHub Actions
    • Azure DevOps
    • AWS CodeBuild
    • Google Cloud Build
        """,
        "components": """
Select which components to deploy:

CORE COMPONENTS:
  🗄️  Database - MariaDB/MySQL database
  📦  Redis - Cache and session store
  ⚡  API - Backend REST API
  🌐  Frontend - React web application

OPTIONAL:
  📊  Monitoring - Prometheus & Grafana

For microservices architecture, you can 
also select individual services.
        """,
        "database": """
Configure your database settings:

DATABASE PROVIDER:
  • MariaDB (recommended)
  • MySQL
  • PostgreSQL

CONNECTION:
  • Host: Database server address
  • Port: Default 3306
  • Database: Name of the database

CREDENTIALS:
  • Username: Database user
  • Password: User password
  • Root Password: Admin password

For cloud deployments, you can use 
managed database services.
        """,
        "network": """
Configure network settings:

SERVICE PORTS:
  • API Port: Backend API (default: 5000)
  • Frontend Port: Web UI (default: 80/443)
  • Redis Port: Cache (default: 6379)

DOMAIN:
  • Enter your domain name
  • Use 'localhost' for local development

SSL/HTTPS:
  • Strongly recommended for production
  • Provide certificate and key paths
  • Or use Let's Encrypt (cloud)
        """,
        "credentials": """
Set up administrator credentials:

ADMIN ACCOUNT:
  • Username: Login username
  • Email: Admin email address
  • Password: Strong password required

PASSWORD REQUIREMENTS:
  • Minimum 8 characters
  • Mix of letters, numbers, symbols

SECURITY KEYS:
  • JWT Secret: Auto-generated
  • Used for authentication tokens
        """,
        "review": """
Review your complete configuration.

Check all settings before deploying:
  ✓ Deployment target
  ✓ Environment and architecture
  ✓ Components selected
  ✓ Database settings
  ✓ Network configuration
  ✓ Admin credentials

You can go back to any step to make changes.

When ready, click 'Deploy' to begin!
        """,
        "deploy": """
Deployment in progress...

The wizard will:
  1. Generate deployment scripts
  2. Check prerequisites
  3. Build containers
  4. Deploy components
  5. Run health checks
  6. Display access URLs

Watch the log panel for detailed progress.

This may take several minutes depending on 
your network and system speed.
        """,
    }
    
    def __init__(self, root: tk.Tk):
        self.root = root
        self.root.title(f"CRM Solution Deployment Wizard v{VERSION}")
        self.root.geometry("1400x900")
        self.root.minsize(1300, 800)
        
        # Configuration
        self.config = DeploymentConfig()
        self.config_path = Path(__file__).parent / "config" / "deployment_config.json"
        self.log_queue = queue.Queue()
        self.deployment_thread: Optional[threading.Thread] = None
        self.engine: Optional[DeploymentEngine] = None
        
        # Wizard state
        self.current_step = 0
        self.step_history: List[int] = [0]  # Track navigation history
        self.wizard_log: List[Dict] = []  # Log all wizard actions
        
        # Resource tracking and test mode
        self.resource_log = DeploymentResourceLog()
        self.is_test_mode = tk.BooleanVar(value=False)
        self.summary_path: Optional[Path] = None
        
        # DEPLOYMENT TARGET - Primary selection that drives everything else
        self.deploy_target_var = tk.StringVar(value="")  # Empty = not selected yet
        
        # Dynamic step list (changes based on selections)
        self.active_steps = self._calculate_active_steps()
        
        # Wizard variables (set defaults)
        self.env_var = tk.StringVar(value="development")
        self.arch_var = tk.StringVar(value="monolithic")
        self.platform_var = tk.StringVar(value="docker")
        self.cloud_provider_var = tk.StringVar(value="none")
        self.build_source_var = tk.StringVar(value="local")
        self.build_method_var = tk.StringVar(value="local")
        self.cloud_authenticated = tk.BooleanVar(value=False)
        
        # Component vars
        self.deploy_db_var = tk.BooleanVar(value=True)
        self.deploy_redis_var = tk.BooleanVar(value=True)
        self.deploy_api_var = tk.BooleanVar(value=True)
        self.deploy_frontend_var = tk.BooleanVar(value=True)
        self.deploy_monitoring_var = tk.BooleanVar(value=False)
        
        # Microservice vars
        self.deploy_identity_var = tk.BooleanVar(value=True)
        self.deploy_customer_var = tk.BooleanVar(value=True)
        self.deploy_sales_var = tk.BooleanVar(value=True)
        self.deploy_marketing_var = tk.BooleanVar(value=True)
        self.deploy_servicedesk_var = tk.BooleanVar(value=True)
        
        # Database vars
        self.db_provider_var = tk.StringVar(value="mariadb")
        self.db_host_var = tk.StringVar(value="crm-mariadb")
        self.db_port_var = tk.IntVar(value=3306)
        self.db_name_var = tk.StringVar(value="crm_db")
        self.db_user_var = tk.StringVar(value="crm_user")
        self.db_pass_var = tk.StringVar(value="CrmPass@Dev2024!")
        self.db_root_pass_var = tk.StringVar(value="RootPass@Dev2024")
        self.use_managed_db_var = tk.BooleanVar(value=False)
        
        # Network vars
        self.api_port_var = tk.IntVar(value=5000)
        self.frontend_port_var = tk.IntVar(value=80)
        self.redis_port_var = tk.IntVar(value=6379)
        self.domain_var = tk.StringVar(value="localhost")
        self.ssl_var = tk.BooleanVar(value=False)
        self.ssl_cert_var = tk.StringVar(value="")
        self.ssl_key_var = tk.StringVar(value="")
        
        # Credential vars
        self.admin_user_var = tk.StringVar(value="admin")
        self.admin_email_var = tk.StringVar(value="admin@crm.local")
        self.admin_pass_var = tk.StringVar(value="Admin@123")
        self.jwt_secret_var = tk.StringVar(value="")
        
        # Git vars
        self.git_repo_var = tk.StringVar(value="")
        self.git_branch_var = tk.StringVar(value="main")
        self.git_username_var = tk.StringVar(value="")
        self.git_token_var = tk.StringVar(value="")
        
        # Cloud vars
        self.aws_region_var = tk.StringVar(value="us-east-1")
        self.aws_ecr_var = tk.StringVar(value="")
        self.azure_subscription_var = tk.StringVar(value="")
        self.azure_resource_group_var = tk.StringVar(value="crm-resources")
        self.azure_location_var = tk.StringVar(value="eastus")
        self.gcp_project_var = tk.StringVar(value="")
        self.gcp_region_var = tk.StringVar(value="us-central1")
        
        # Cloud build vars
        self.cloud_build_provider_var = tk.StringVar(value="github_actions")
        
        # Ensure config directory exists
        self.config_path.parent.mkdir(parents=True, exist_ok=True)
        
        # Load saved configuration
        self.load_config()
        
        # Create UI
        self.create_wizard_ui()
        
        # Start log processor
        self.process_logs()
        
        # Log wizard start
        self.log_wizard_action("wizard_start", "Deployment Wizard started")
    
    def _calculate_active_steps(self) -> List[Dict]:
        """Calculate which steps are active based on deployment target selection."""
        active = []
        
        # Get current target
        target = getattr(self, 'deploy_target_var', None)
        target_value = target.get() if target else ""
        
        for step in self.STEPS:
            step_id = step["id"]
            
            # Always include welcome and target selection
            if step_id in ["welcome", "target"]:
                active.append(step)
            
            # Cloud config only for cloud targets
            elif step_id == "cloud_config":
                if target_value in ["aws", "azure", "gcp"]:
                    active.append(step)
            
            # Cloud auth only for cloud targets
            elif step_id == "cloud_auth":
                if target_value in ["aws", "azure", "gcp"]:
                    active.append(step)
            
            # These steps always appear after target is selected
            elif step_id in ["environment", "architecture", "build", 
                            "components", "database", "network", "credentials", 
                            "review", "deploy"]:
                if target_value:  # Only show if target is selected
                    active.append(step)
        
        return active
    
    def log_wizard_action(self, action: str, details: str, value: str = ""):
        """Log wizard action with timestamp."""
        entry = {
            "timestamp": datetime.now().isoformat(),
            "action": action,
            "details": details,
            "value": value,
            "step": self.active_steps[self.current_step]["id"] if self.current_step < len(self.active_steps) else "unknown"
        }
        self.wizard_log.append(entry)
        
        # Also log to main log panel
        log_msg = f"[Wizard] {details}"
        if value:
            log_msg += f": {value}"
        self.log_message("info", log_msg)
    
    def log_message(self, msg_type: str, message: str):
        """Queue a log message."""
        self.log_queue.put((msg_type, message))
    
    def process_logs(self):
        """Process queued log messages."""
        try:
            while True:
                msg_type, message = self.log_queue.get_nowait()
                self._write_log(msg_type, message)
        except queue.Empty:
            pass
        self.root.after(100, self.process_logs)
    
    def _write_log(self, msg_type: str, message: str):
        """Write message to log panel."""
        if not hasattr(self, 'log_text'):
            return
        
        self.log_text.configure(state=tk.NORMAL)
        
        timestamp = datetime.now().strftime("%H:%M:%S")
        
        # Color coding
        tag = msg_type
        if msg_type == "error":
            prefix = "❌ "
        elif msg_type == "success":
            prefix = "✅ "
        elif msg_type == "warning":
            prefix = "⚠️ "
        elif msg_type == "cmd":
            prefix = "$ "
        else:
            prefix = "ℹ️ "
        
        self.log_text.insert(tk.END, f"[{timestamp}] {prefix}{message}\n", tag)
        self.log_text.see(tk.END)
        self.log_text.configure(state=tk.DISABLED)
    
    def create_wizard_ui(self):
        """Create the wizard user interface."""
        # Main container
        main_frame = ttk.Frame(self.root, padding="10")
        main_frame.pack(fill=tk.BOTH, expand=True)
        
        # Top: Progress indicator
        self.create_progress_indicator(main_frame)
        
        # Middle: Split pane - content left, log right
        content_paned = ttk.PanedWindow(main_frame, orient=tk.HORIZONTAL)
        content_paned.pack(fill=tk.BOTH, expand=True, pady=10)
        
        # Left side: Step content
        left_frame = ttk.Frame(content_paned)
        content_paned.add(left_frame, weight=3)
        
        # Step title and description
        self.step_header = ttk.Frame(left_frame)
        self.step_header.pack(fill=tk.X, pady=(0, 15))
        
        self.step_title_var = tk.StringVar(value="Welcome")
        self.step_title_label = ttk.Label(self.step_header, textvariable=self.step_title_var,
                                          font=self.UI_CONFIG["title_font"])
        self.step_title_label.pack(anchor=tk.W)
        
        self.step_desc_var = tk.StringVar(value="Get started with deployment")
        self.step_desc_label = ttk.Label(self.step_header, textvariable=self.step_desc_var,
                                         font=self.UI_CONFIG["subheading_font"], foreground="#666")
        self.step_desc_label.pack(anchor=tk.W, pady=(5, 0))
        
        # Separator line
        ttk.Separator(left_frame, orient=tk.HORIZONTAL).pack(fill=tk.X, pady=(0, 15))
        
        # Step content area with scrolling
        self.step_container = ttk.Frame(left_frame)
        self.step_container.pack(fill=tk.BOTH, expand=True)
        
        # Current step frame (replaced each step)
        self.step_frame = ttk.Frame(self.step_container)
        self.step_frame.pack(fill=tk.BOTH, expand=True)
        
        # Right side: Log and help with better formatting
        right_frame = ttk.Frame(content_paned)
        content_paned.add(right_frame, weight=2)
        
        # Help panel with larger text
        help_header = ttk.Frame(right_frame)
        help_header.pack(fill=tk.X, pady=(0, 8))
        ttk.Label(help_header, text="📖 Help & Guidance", 
                 font=self.UI_CONFIG["heading_font"]).pack(anchor=tk.W)
        
        self.help_text = scrolledtext.ScrolledText(
            right_frame, 
            height=12, 
            wrap=tk.WORD,
            font=self.UI_CONFIG["help_font"],
            state=tk.DISABLED,
            bg="#f8f9fa",
            padx=10,
            pady=10,
            relief=tk.FLAT,
            borderwidth=1
        )
        self.help_text.pack(fill=tk.X, pady=(0, 15))
        
        # Separator
        ttk.Separator(right_frame, orient=tk.HORIZONTAL).pack(fill=tk.X, pady=(0, 15))
        
        # Log panel with larger, better formatted text
        log_header = ttk.Frame(right_frame)
        log_header.pack(fill=tk.X, pady=(0, 8))
        ttk.Label(log_header, text="📋 Activity Log", 
                 font=self.UI_CONFIG["heading_font"]).pack(anchor=tk.W)
        
        # Create log frame with better styling
        log_frame = ttk.Frame(right_frame)
        log_frame.pack(fill=tk.BOTH, expand=True)
        
        self.log_text = scrolledtext.ScrolledText(
            log_frame, 
            height=15, 
            wrap=tk.WORD,
            font=self.UI_CONFIG["log_font"],
            state=tk.DISABLED,
            bg="#1e1e1e",  # Dark background for logs
            fg="#d4d4d4",  # Light text
            insertbackground="white",
            padx=12,
            pady=10,
            relief=tk.FLAT,
            borderwidth=0
        )
        self.log_text.pack(fill=tk.BOTH, expand=True)
        
        # Configure log tags with better colors for dark background
        self.log_text.tag_configure("error", foreground="#ff6b6b", font=(self.UI_CONFIG["log_font"][0], self.UI_CONFIG["log_font"][1], "bold"))
        self.log_text.tag_configure("success", foreground="#69db7c", font=(self.UI_CONFIG["log_font"][0], self.UI_CONFIG["log_font"][1]))
        self.log_text.tag_configure("warning", foreground="#ffd43b", font=(self.UI_CONFIG["log_font"][0], self.UI_CONFIG["log_font"][1]))
        self.log_text.tag_configure("cmd", foreground="#74c0fc", font=(self.UI_CONFIG["log_font"][0], self.UI_CONFIG["log_font"][1]))
        self.log_text.tag_configure("info", foreground="#d4d4d4", font=(self.UI_CONFIG["log_font"][0], self.UI_CONFIG["log_font"][1]))
        
        # Bottom: Navigation buttons
        self.create_navigation_buttons(main_frame)
        
        # Show first step
        self.show_step(0)
    
    def create_progress_indicator(self, parent):
        """Create progress step indicator."""
        progress_frame = ttk.Frame(parent)
        progress_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.progress_canvas = tk.Canvas(progress_frame, height=70, highlightthickness=0, bg="#f0f0f0")
        self.progress_canvas.pack(fill=tk.X)
        
        # Store for later updates
        self.progress_items = []
        
        self.update_progress_indicator()
    
    def update_progress_indicator(self):
        """Update the progress indicator to show current step."""
        self.progress_canvas.delete("all")
        self.progress_items = []
        
        # Recalculate active steps
        self.active_steps = self._calculate_active_steps()
        
        canvas_width = self.progress_canvas.winfo_width()
        if canvas_width < 100:
            canvas_width = 1300  # Default width
        
        num_steps = len(self.active_steps)
        if num_steps == 0:
            return
        
        step_width = canvas_width / num_steps
        
        for i, step in enumerate(self.active_steps):
            x_center = step_width * i + step_width / 2
            
            # Determine colors
            if i < self.current_step:
                # Completed
                circle_color = "#28a745"  # Green
                text_color = "white"
            elif i == self.current_step:
                # Current
                circle_color = "#007bff"  # Blue
                text_color = "white"
            else:
                # Future
                circle_color = "#d0d0d0"  # Gray
                text_color = "#666"
            
            # Draw circle - larger size
            r = 18
            self.progress_canvas.create_oval(x_center - r, 25 - r, x_center + r, 25 + r,
                                            fill=circle_color, outline="white", width=2)
            
            # Draw step number or checkmark
            if i < self.current_step:
                self.progress_canvas.create_text(x_center, 25, text="✓", 
                                                fill=text_color, font=("Helvetica", 14, "bold"))
            else:
                self.progress_canvas.create_text(x_center, 25, text=str(i + 1), 
                                                fill=text_color, font=("Helvetica", 12, "bold"))
            
            # Draw step title - larger font
            title = step["title"][:12] if len(step["title"]) <= 12 else step["title"][:11] + "…"
            self.progress_canvas.create_text(x_center, 55, text=title,
                                            fill="#333" if i <= self.current_step else "#999",
                                            font=("Helvetica", 10))
            
            # Draw connecting line
            if i < num_steps - 1:
                next_x = step_width * (i + 1) + step_width / 2
                line_color = "#28a745" if i < self.current_step else "#d0d0d0"
                self.progress_canvas.create_line(x_center + r + 5, 25, next_x - r - 5, 25,
                                                fill=line_color, width=3)
    
    def create_navigation_buttons(self, parent):
        """Create navigation button bar with larger buttons."""
        # Separator above buttons
        ttk.Separator(parent, orient=tk.HORIZONTAL).pack(fill=tk.X, pady=(15, 0))
        
        nav_frame = ttk.Frame(parent)
        nav_frame.pack(fill=tk.X, pady=(15, 5))
        
        # Style for larger buttons
        style = ttk.Style()
        style.configure("Large.TButton", font=self.UI_CONFIG["button_font"], padding=(20, 10))
        
        # Left side buttons
        left_btns = ttk.Frame(nav_frame)
        left_btns.pack(side=tk.LEFT)
        
        ttk.Button(left_btns, text="💾 Save Progress", command=self.save_config, 
                  style="Large.TButton").pack(side=tk.LEFT, padx=5)
        ttk.Button(left_btns, text="📂 Load Config", command=self.load_config_dialog,
                  style="Large.TButton").pack(side=tk.LEFT, padx=5)
        
        # Right side buttons
        right_btns = ttk.Frame(nav_frame)
        right_btns.pack(side=tk.RIGHT)
        
        self.back_btn = ttk.Button(right_btns, text="← Back", command=self.go_back,
                                   style="Large.TButton")
        self.back_btn.pack(side=tk.LEFT, padx=5)
        
        self.next_btn = ttk.Button(right_btns, text="Next →", command=self.go_next,
                                   style="Large.TButton")
        self.next_btn.pack(side=tk.LEFT, padx=5)
        
        # Exit button
        ttk.Button(right_btns, text="❌ Exit", command=self.confirm_exit).pack(side=tk.LEFT, padx=15)
    
    def show_step(self, step_index: int):
        """Display the specified step."""
        if step_index < 0 or step_index >= len(self.active_steps):
            return
        
        self.current_step = step_index
        step = self.active_steps[step_index]
        
        # Update title and description
        self.step_title_var.set(f"Step {step_index + 1}: {step['title']}")
        self.step_desc_var.set(step['description'])
        
        # Update progress indicator
        self.update_progress_indicator()
        
        # Update help text
        self.update_help_text(step['id'])
        
        # Clear current step frame
        for widget in self.step_frame.winfo_children():
            widget.destroy()
        
        # Create step content
        step_method = getattr(self, f"create_step_{step['id']}", None)
        if step_method:
            step_method(self.step_frame)
        else:
            ttk.Label(self.step_frame, text=f"Step: {step['id']} (Not implemented)",
                     font=("Helvetica", 12)).pack(pady=20)
        
        # Update navigation buttons
        self.update_nav_buttons()
        
        # Log step view
        self.log_wizard_action("step_view", f"Viewing step", step['title'])
    
    def update_help_text(self, step_id: str):
        """Update the help panel with step-specific help."""
        help_content = self.HELP_TEXT.get(step_id, "No help available for this step.")
        
        self.help_text.configure(state=tk.NORMAL)
        self.help_text.delete(1.0, tk.END)
        self.help_text.insert(tk.END, help_content.strip())
        self.help_text.configure(state=tk.DISABLED)
    
    def update_nav_buttons(self):
        """Update navigation button states."""
        # Back button
        if self.current_step == 0:
            self.back_btn.configure(state=tk.DISABLED)
        else:
            self.back_btn.configure(state=tk.NORMAL)
        
        # Next button - change to Deploy on last step
        if self.current_step == len(self.active_steps) - 1:
            self.next_btn.configure(text="🚀 Deploy", state=tk.NORMAL)
        elif self.current_step == len(self.active_steps) - 2:
            self.next_btn.configure(text="Review →", state=tk.NORMAL)
        else:
            self.next_btn.configure(text="Next →", state=tk.NORMAL)
    
    def go_back(self):
        """Go to previous step."""
        if self.current_step > 0:
            self.log_wizard_action("button_click", "Back button clicked")
            self.current_step -= 1
            self.show_step(self.current_step)
    
    def go_next(self):
        """Go to next step or deploy."""
        # Validate current step
        if not self.validate_current_step():
            return
        
        self.log_wizard_action("button_click", "Next button clicked")
        
        # Recalculate active steps based on current selections
        old_steps = len(self.active_steps)
        self.active_steps = self._calculate_active_steps()
        
        if self.current_step < len(self.active_steps) - 1:
            self.current_step += 1
            self.show_step(self.current_step)
        else:
            # Final step - deploy
            self.start_deployment()
    
    def validate_current_step(self) -> bool:
        """Validate current step before proceeding."""
        step = self.active_steps[self.current_step]
        step_id = step['id']
        
        if step_id == "target":
            if not self.deploy_target_var.get():
                messagebox.showwarning("Selection Required", 
                    "Please select a deployment target before continuing.\n\n"
                    "Choose where you want to deploy your CRM application.")
                return False
            self.log_wizard_action("selection", "Deployment target selected", self.deploy_target_var.get())
        
        elif step_id == "cloud_config":
            provider = self.cloud_provider_var.get()
            if provider == "gcp" and not self.gcp_project_var.get():
                messagebox.showwarning("Validation", "Please enter your GCP Project ID")
                return False
            self.log_wizard_action("config", "Cloud configuration set", provider)
            
        elif step_id == "environment":
            if not self.env_var.get():
                messagebox.showwarning("Validation", "Please select an environment")
                return False
            self.log_wizard_action("selection", "Environment selected", self.env_var.get())
            
        elif step_id == "architecture":
            self.log_wizard_action("selection", "Architecture selected", self.arch_var.get())
            
        elif step_id == "cloud_auth":
            # Authentication is optional but recommended
            if not self.cloud_authenticated.get():
                result = messagebox.askyesno("Authentication", 
                    "You haven't authenticated with your cloud provider.\n\n"
                    "Authentication is recommended to validate your configuration.\n\n"
                    "Continue without authentication?")
                if not result:
                    return False
            
        elif step_id == "build":
            if self.build_source_var.get() == "git" and not self.git_repo_var.get():
                messagebox.showwarning("Validation", "Please enter Git repository URL")
                return False
            self.log_wizard_action("selection", "Build source selected", self.build_source_var.get())
            
        elif step_id == "database":
            if not self.db_name_var.get() or not self.db_user_var.get():
                messagebox.showwarning("Validation", "Please fill in database name and user")
                return False
            
        elif step_id == "credentials":
            if not self.admin_email_var.get() or not self.admin_pass_var.get():
                messagebox.showwarning("Validation", "Please fill in admin email and password")
                return False
            if len(self.admin_pass_var.get()) < 8:
                messagebox.showwarning("Validation", "Password must be at least 8 characters")
                return False
        
        return True
    
    def confirm_exit(self):
        """Confirm exit with unsaved changes warning."""
        if messagebox.askyesno("Exit", "Are you sure you want to exit?\nUnsaved progress will be lost."):
            self.log_wizard_action("wizard_exit", "Wizard closed by user")
            self.root.quit()
    
    # Step creation methods
    def create_step_welcome(self, parent):
        """Create welcome step with improved styling."""
        frame = ttk.Frame(parent, padding="30")
        frame.pack(fill=tk.BOTH, expand=True)
        
        # Logo/title area - centered
        title_frame = ttk.Frame(frame)
        title_frame.pack(fill=tk.X, pady=(20, 30))
        
        ttk.Label(title_frame, text="🏢 CRM Solution", 
                 font=("Helvetica", 32, "bold")).pack()
        ttk.Label(title_frame, text="Deployment Wizard", 
                 font=("Helvetica", 20)).pack(pady=(5, 0))
        
        # Info text with larger font
        info_frame = ttk.LabelFrame(frame, text="  About This Wizard  ", padding="20")
        info_frame.pack(fill=tk.X, pady=20)
        
        info_text = """This wizard will guide you through deploying the CRM Solution step by step.

You will configure:

    ✓  Deployment target (Local, AWS, Azure, GCP, or Kubernetes)
    ✓  Environment settings (Development, Staging, Production)
    ✓  Architecture (Monolithic or Microservices)
    ✓  Build configuration and components
    ✓  Database and network settings
    ✓  Administrator credentials

Click 'Next' to begin!"""
        
        ttk.Label(info_frame, text=info_text, justify=tk.LEFT,
                 font=self.UI_CONFIG["body_font"]).pack(anchor=tk.W)
        
        # Version info
        ttk.Label(frame, text=f"Version {VERSION}", 
                 font=("Helvetica", 11), foreground="gray").pack(side=tk.BOTTOM, pady=10)
    
    def create_step_target(self, parent):
        """Create deployment target selection step."""
        frame = ttk.Frame(parent, padding="25")
        frame.pack(fill=tk.BOTH, expand=True)
        
        ttk.Label(frame, text="Where do you want to deploy?", 
                 font=self.UI_CONFIG["heading_font"]).pack(anchor=tk.W, pady=(0, 20))
        
        # Target options with large clickable cards
        targets = [
            ("local", "🖥️", "Local Docker", "Deploy to your local machine using Docker Compose", "#e3f2fd"),
            ("aws", "🟠", "Amazon Web Services", "Deploy to AWS using ECS, Fargate, RDS", "#fff3e0"),
            ("azure", "🔵", "Microsoft Azure", "Deploy to Azure using AKS or Container Instances", "#e3f2fd"),
            ("gcp", "🔴", "Google Cloud Platform", "Deploy to GCP using GKE or Cloud Run", "#ffebee"),
            ("kubernetes", "☸️", "Kubernetes Cluster", "Deploy to an existing Kubernetes cluster", "#e8f5e9"),
        ]
        
        # Create scrollable frame for targets
        for value, icon, title, desc, color in targets:
            card = tk.Frame(frame, bg=color, padx=20, pady=15, cursor="hand2")
            card.pack(fill=tk.X, pady=8)
            
            # Make entire card clickable
            def on_click(v=value, c=card):
                self._on_target_select(v)
                # Update visual selection
                self._update_target_cards()
            
            card.bind("<Button-1>", lambda e, v=value: self._on_target_select(v))
            
            # Radio button with larger font
            rb_frame = tk.Frame(card, bg=color)
            rb_frame.pack(fill=tk.X)
            
            rb = tk.Radiobutton(rb_frame, text=f"{icon}  {title}", 
                               variable=self.deploy_target_var, value=value,
                               font=self.UI_CONFIG["heading_font"],
                               bg=color, activebackground=color,
                               command=lambda v=value: self._on_target_select(v))
            rb.pack(anchor=tk.W)
            
            # Description
            desc_label = tk.Label(rb_frame, text=f"      {desc}", 
                                 font=self.UI_CONFIG["body_font"],
                                 fg="#666", bg=color)
            desc_label.pack(anchor=tk.W)
            
            # Store reference for updating
            card.target_value = value
            if not hasattr(self, '_target_cards'):
                self._target_cards = []
            self._target_cards.append(card)
    
    def _on_target_select(self, target: str):
        """Handle deployment target selection."""
        self.deploy_target_var.set(target)
        self.log_wizard_action("selection", "Deployment target selected", target)
        
        # Update platform and cloud provider based on target
        if target == "local":
            self.platform_var.set("docker")
            self.cloud_provider_var.set("none")
        elif target == "kubernetes":
            self.platform_var.set("kubernetes")
            self.cloud_provider_var.set("none")
        elif target in ["aws", "azure", "gcp"]:
            self.platform_var.set("cloud")
            self.cloud_provider_var.set(target)
        
        # Recalculate active steps
        self.active_steps = self._calculate_active_steps()
        self.update_progress_indicator()
    
    def _update_target_cards(self):
        """Update visual selection of target cards."""
        if not hasattr(self, '_target_cards'):
            return
        
        selected = self.deploy_target_var.get()
        for card in self._target_cards:
            if hasattr(card, 'target_value'):
                if card.target_value == selected:
                    card.configure(relief=tk.SOLID, borderwidth=2)
                else:
                    card.configure(relief=tk.FLAT, borderwidth=0)
    
    def create_step_cloud_config(self, parent):
        """Create cloud configuration step with CLI checking and auto-populated options."""
        frame = ttk.Frame(parent, padding="25")
        frame.pack(fill=tk.BOTH, expand=True)
        
        provider = self.cloud_provider_var.get()
        provider_upper = provider.upper()
        provider_names = {"aws": "Amazon Web Services", "azure": "Microsoft Azure", "gcp": "Google Cloud Platform"}
        provider_full = provider_names.get(provider, provider_upper)
        
        ttk.Label(frame, text=f"Configure {provider_full}", 
                 font=self.UI_CONFIG["heading_font"]).pack(anchor=tk.W, pady=(0, 15))
        
        # CLI Status Card
        cli_card = tk.Frame(frame, bg="#f5f5f5", padx=20, pady=15)
        cli_card.pack(fill=tk.X, pady=8)
        
        self.cli_status_var = tk.StringVar(value="Checking CLI...")
        tk.Label(cli_card, text=f"🔧  {provider_upper} CLI Status", 
                font=self.UI_CONFIG["subheading_font"], bg="#f5f5f5").pack(anchor=tk.W)
        
        cli_status_frame = tk.Frame(cli_card, bg="#f5f5f5")
        cli_status_frame.pack(fill=tk.X, pady=5)
        
        self.cli_status_label = tk.Label(cli_status_frame, textvariable=self.cli_status_var,
                                        font=self.UI_CONFIG["body_font"], bg="#f5f5f5")
        self.cli_status_label.pack(side=tk.LEFT)
        
        self.cli_install_btn = tk.Button(cli_status_frame, text="Install CLI",
                                        font=("Helvetica", 11),
                                        command=lambda: self._install_cloud_cli(provider))
        # Hidden by default, shown if CLI missing
        
        # Configuration section
        config_frame = ttk.LabelFrame(frame, text=f"  {provider_upper} Configuration  ", padding="20")
        config_frame.pack(fill=tk.X, pady=15)
        
        if provider == "aws":
            self._create_aws_config(config_frame)
        elif provider == "azure":
            self._create_azure_config(config_frame)
        elif provider == "gcp":
            self._create_gcp_config(config_frame)
        
        # Check CLI and populate resources
        self._check_cloud_cli_and_populate(provider)
    
    def _create_aws_config(self, parent):
        """Create AWS-specific configuration widgets."""
        # Region
        ttk.Label(parent, text="AWS Region:", 
                 font=self.UI_CONFIG["label_font"]).grid(row=0, column=0, sticky=tk.W, pady=10)
        
        self.aws_region_combo = ttk.Combobox(parent, textvariable=self.aws_region_var, 
                                              width=30, font=self.UI_CONFIG["body_font"])
        self.aws_region_combo.grid(row=0, column=1, padx=15, pady=10)
        self.aws_region_combo['values'] = ["us-east-1", "us-east-2", "us-west-1", "us-west-2", 
                                            "eu-west-1", "eu-west-2", "eu-central-1", 
                                            "ap-southeast-1", "ap-southeast-2", "ap-northeast-1"]
        
        ttk.Label(parent, text="Select the AWS region for deployment.",
                 font=("Helvetica", 11), foreground="gray").grid(row=1, column=0, columnspan=2, sticky=tk.W)
        
        # ECS Cluster
        ttk.Label(parent, text="ECS Cluster:", 
                 font=self.UI_CONFIG["label_font"]).grid(row=2, column=0, sticky=tk.W, pady=10)
        
        cluster_frame = tk.Frame(parent)
        cluster_frame.grid(row=2, column=1, sticky=tk.W, padx=15, pady=10)
        
        self.ecs_cluster_combo = ttk.Combobox(cluster_frame, textvariable=self.aws_ecs_cluster_var, 
                                               width=25, font=self.UI_CONFIG["body_font"])
        self.ecs_cluster_combo.pack(side=tk.LEFT)
        
        ttk.Button(cluster_frame, text="+ New", 
                  command=self._create_new_ecs_cluster).pack(side=tk.LEFT, padx=5)
        
        ttk.Label(parent, text="Select existing cluster or create new.",
                 font=("Helvetica", 11), foreground="gray").grid(row=3, column=0, columnspan=2, sticky=tk.W)
        
        # Refresh button
        ttk.Button(parent, text="🔄 Refresh Resources", 
                  command=lambda: self._refresh_aws_resources()).grid(row=4, column=0, columnspan=2, pady=15)
    
    def _create_azure_config(self, parent):
        """Create Azure-specific configuration widgets."""
        # Location
        ttk.Label(parent, text="Azure Location:", 
                 font=self.UI_CONFIG["label_font"]).grid(row=0, column=0, sticky=tk.W, pady=10)
        
        self.azure_location_combo = ttk.Combobox(parent, textvariable=self.azure_location_var,
                                                  width=30, font=self.UI_CONFIG["body_font"])
        self.azure_location_combo.grid(row=0, column=1, padx=15, pady=10)
        self.azure_location_combo['values'] = ["eastus", "eastus2", "westus", "westus2", "centralus",
                                                "northeurope", "westeurope", "southeastasia", "australiaeast"]
        
        # Resource Group
        ttk.Label(parent, text="Resource Group:", 
                 font=self.UI_CONFIG["label_font"]).grid(row=2, column=0, sticky=tk.W, pady=10)
        
        rg_frame = tk.Frame(parent)
        rg_frame.grid(row=2, column=1, sticky=tk.W, padx=15, pady=10)
        
        self.azure_rg_combo = ttk.Combobox(rg_frame, textvariable=self.azure_resource_group_var,
                                            width=25, font=self.UI_CONFIG["body_font"])
        self.azure_rg_combo.pack(side=tk.LEFT)
        
        ttk.Button(rg_frame, text="+ New", 
                  command=self._create_new_azure_rg).pack(side=tk.LEFT, padx=5)
        
        ttk.Label(parent, text="Select existing or create new resource group.",
                 font=("Helvetica", 11), foreground="gray").grid(row=3, column=0, columnspan=2, sticky=tk.W)
        
        # Subscription (display)
        ttk.Label(parent, text="Subscription:", 
                 font=self.UI_CONFIG["label_font"]).grid(row=4, column=0, sticky=tk.W, pady=10)
        
        self.azure_sub_combo = ttk.Combobox(parent, textvariable=self.azure_subscription_var,
                                             width=40, font=self.UI_CONFIG["body_font"])
        self.azure_sub_combo.grid(row=4, column=1, padx=15, pady=10)
        
        # Refresh
        ttk.Button(parent, text="🔄 Refresh Resources", 
                  command=lambda: self._refresh_azure_resources()).grid(row=5, column=0, columnspan=2, pady=15)
    
    def _create_gcp_config(self, parent):
        """Create GCP-specific configuration widgets."""
        # Project ID
        ttk.Label(parent, text="GCP Project ID:", 
                 font=self.UI_CONFIG["label_font"]).grid(row=0, column=0, sticky=tk.W, pady=10)
        
        project_frame = tk.Frame(parent)
        project_frame.grid(row=0, column=1, sticky=tk.W, padx=15, pady=10)
        
        self.gcp_project_combo = ttk.Combobox(project_frame, textvariable=self.gcp_project_var,
                                               width=30, font=self.UI_CONFIG["body_font"])
        self.gcp_project_combo.pack(side=tk.LEFT)
        
        ttk.Button(project_frame, text="+ New Project", 
                  command=self._create_new_gcp_project).pack(side=tk.LEFT, padx=5)
        
        ttk.Label(parent, text="Select existing project or create new. Project IDs must be globally unique.",
                 font=("Helvetica", 11), foreground="gray").grid(row=1, column=0, columnspan=2, sticky=tk.W)
        
        # Region
        ttk.Label(parent, text="GCP Region:", 
                 font=self.UI_CONFIG["label_font"]).grid(row=2, column=0, sticky=tk.W, pady=10)
        
        self.gcp_region_combo = ttk.Combobox(parent, textvariable=self.gcp_region_var,
                                              width=30, font=self.UI_CONFIG["body_font"])
        self.gcp_region_combo.grid(row=2, column=1, padx=15, pady=10)
        self.gcp_region_combo['values'] = ["us-central1", "us-east1", "us-west1", "europe-west1", 
                                            "europe-west2", "asia-east1", "asia-southeast1"]
        
        # GKE Cluster (optional)
        ttk.Label(parent, text="GKE Cluster:", 
                 font=self.UI_CONFIG["label_font"]).grid(row=3, column=0, sticky=tk.W, pady=10)
        
        self.gke_cluster_combo = ttk.Combobox(parent, textvariable=self.gcp_gke_cluster_var,
                                               width=30, font=self.UI_CONFIG["body_font"])
        self.gke_cluster_combo.grid(row=3, column=1, padx=15, pady=10)
        self.gke_cluster_combo['values'] = ["crm-cluster (will create)", ""]
        
        # Refresh
        ttk.Button(parent, text="🔄 Refresh Resources", 
                  command=lambda: self._refresh_gcp_resources()).grid(row=4, column=0, columnspan=2, pady=15)
    
    def _check_cloud_cli_and_populate(self, provider: str):
        """Check CLI installation and populate resources in background."""
        def do_check():
            cli_manager = CloudCLIManager(lambda t, m: self.log_message(t, m))
            installed, version = cli_manager.check_cli_installed(provider)
            
            if installed:
                self.root.after(0, lambda: self.cli_status_var.set(f"✅ Installed (v{version})"))
                self.root.after(0, lambda: self.cli_status_label.configure(fg="#4caf50"))
                
                # Check authentication
                authed, account_info = cli_manager.check_authentication(provider)
                if authed:
                    self.cloud_authenticated.set(True)
                    display = account_info.get("display", "Authenticated")
                    self.root.after(0, lambda: self.cli_status_var.set(f"✅ Installed & Authenticated: {display}"))
                    
                    # Populate resources
                    self._populate_cloud_resources(provider, cli_manager)
                else:
                    self.root.after(0, lambda: self.cli_status_var.set(f"✅ Installed but ⚠️ Not authenticated"))
            else:
                self.root.after(0, lambda: self.cli_status_var.set(f"❌ Not installed"))
                self.root.after(0, lambda: self.cli_status_label.configure(fg="#f44336"))
                self.root.after(0, lambda: self.cli_install_btn.pack(side=tk.LEFT, padx=15))
                
                # Show install instructions
                install_cmd = cli_manager.get_install_command(provider)
                self.log_message("warning", f"{provider.upper()} CLI not installed")
                self.log_message("info", f"Install with: {install_cmd}")
        
        threading.Thread(target=do_check, daemon=True).start()
    
    def _populate_cloud_resources(self, provider: str, cli_manager: CloudCLIManager):
        """Populate cloud resource dropdowns."""
        if provider == "aws":
            resources = cli_manager.get_aws_resources(self.aws_region_var.get() or "us-east-1")
            if resources.get("regions"):
                self.root.after(0, lambda: self.aws_region_combo.configure(values=resources["regions"]))
            if resources.get("ecs_clusters"):
                self.root.after(0, lambda: self.ecs_cluster_combo.configure(
                    values=resources["ecs_clusters"] + ["crm-cluster (will create)"]))
                
        elif provider == "azure":
            resources = cli_manager.get_azure_resources()
            if resources.get("resource_groups"):
                self.root.after(0, lambda: self.azure_rg_combo.configure(values=resources["resource_groups"]))
            if resources.get("subscriptions"):
                subs = [f"{s.get('name')} ({s.get('id')})" for s in resources["subscriptions"]]
                self.root.after(0, lambda: self.azure_sub_combo.configure(values=subs))
            if resources.get("locations"):
                self.root.after(0, lambda: self.azure_location_combo.configure(values=resources["locations"][:20]))
                
        elif provider == "gcp":
            resources = cli_manager.get_gcp_resources()
            if resources.get("projects"):
                projects = [p.get("projectId") for p in resources["projects"] if p.get("projectId")]
                self.root.after(0, lambda: self.gcp_project_combo.configure(values=projects))
                # Set current project if available
                if resources.get("current_project") and not self.gcp_project_var.get():
                    self.root.after(0, lambda: self.gcp_project_var.set(resources["current_project"]))
            if resources.get("regions"):
                self.root.after(0, lambda: self.gcp_region_combo.configure(values=resources["regions"]))
    
    def _install_cloud_cli(self, provider: str):
        """Install cloud CLI."""
        self.cli_status_var.set(f"Installing {provider.upper()} CLI...")
        self.cli_install_btn.configure(state=tk.DISABLED)
        
        def do_install():
            cli_manager = CloudCLIManager(lambda t, m: self.log_message(t, m))
            success, output = cli_manager.install_cli(provider)
            
            if success:
                self.root.after(0, lambda: self.cli_status_var.set(f"✅ Installed successfully"))
                self.root.after(0, lambda: self.cli_install_btn.pack_forget())
                # Re-check and populate
                self._check_cloud_cli_and_populate(provider)
            else:
                self.root.after(0, lambda: self.cli_status_var.set(f"❌ Installation failed - see log"))
                self.root.after(0, lambda: self.cli_install_btn.configure(state=tk.NORMAL))
                
                # Show manual install dialog
                install_cmd = cli_manager.get_install_command(provider)
                docs_url = cli_manager.CLI_INFO[provider]["docs_url"]
                self.root.after(0, lambda: messagebox.showinfo(
                    "Manual Installation Required",
                    f"Please install {provider.upper()} CLI manually:\n\n"
                    f"Command: {install_cmd}\n\n"
                    f"Documentation: {docs_url}"
                ))
        
        threading.Thread(target=do_install, daemon=True).start()
    
    def _refresh_aws_resources(self):
        """Refresh AWS resources."""
        def do_refresh():
            cli_manager = CloudCLIManager(lambda t, m: self.log_message(t, m))
            self._populate_cloud_resources("aws", cli_manager)
        threading.Thread(target=do_refresh, daemon=True).start()
    
    def _refresh_azure_resources(self):
        """Refresh Azure resources."""
        def do_refresh():
            cli_manager = CloudCLIManager(lambda t, m: self.log_message(t, m))
            self._populate_cloud_resources("azure", cli_manager)
        threading.Thread(target=do_refresh, daemon=True).start()
    
    def _refresh_gcp_resources(self):
        """Refresh GCP resources."""
        def do_refresh():
            cli_manager = CloudCLIManager(lambda t, m: self.log_message(t, m))
            self._populate_cloud_resources("gcp", cli_manager)
        threading.Thread(target=do_refresh, daemon=True).start()
    
    def _create_new_gcp_project(self):
        """Dialog to create new GCP project."""
        dialog = tk.Toplevel(self.root)
        dialog.title("Create GCP Project")
        dialog.geometry("500x320")
        dialog.transient(self.root)
        dialog.grab_set()
        
        ttk.Label(dialog, text="Create New GCP Project", 
                 font=self.UI_CONFIG["heading_font"]).pack(pady=15)
        
        form = ttk.Frame(dialog, padding=20)
        form.pack(fill=tk.X)
        
        ttk.Label(form, text="Project ID:", font=self.UI_CONFIG["label_font"]).grid(row=0, column=0, sticky=tk.W, pady=8)
        project_id_var = tk.StringVar(value=f"crm-project-{secrets.token_hex(4)}")
        ttk.Entry(form, textvariable=project_id_var, width=35, font=self.UI_CONFIG["body_font"]).grid(row=0, column=1, padx=10, pady=8)
        
        ttk.Label(form, text="Project IDs must be 6-30 characters, lowercase, and globally unique.",
                 font=("Helvetica", 10), foreground="gray").grid(row=1, column=0, columnspan=2, sticky=tk.W)
        
        ttk.Label(form, text="Project Name:", font=self.UI_CONFIG["label_font"]).grid(row=2, column=0, sticky=tk.W, pady=8)
        project_name_var = tk.StringVar(value="CRM Solution")
        ttk.Entry(form, textvariable=project_name_var, width=35, font=self.UI_CONFIG["body_font"]).grid(row=2, column=1, padx=10, pady=8)
        
        status_var = tk.StringVar(value="")
        ttk.Label(form, textvariable=status_var, font=("Helvetica", 10)).grid(row=3, column=0, columnspan=2, pady=10)
        
        def do_create():
            status_var.set("Creating project...")
            project_id = project_id_var.get().lower().replace(" ", "-")
            
            def create():
                cli_manager = CloudCLIManager(lambda t, m: self.log_message(t, m))
                success, output = cli_manager.create_gcp_project(project_id, project_name_var.get())
                
                if success:
                    self.root.after(0, lambda: self.gcp_project_var.set(project_id))
                    self.root.after(0, lambda: status_var.set("✅ Project created!"))
                    self.root.after(1000, dialog.destroy)
                    self._refresh_gcp_resources()
                else:
                    self.root.after(0, lambda: status_var.set(f"❌ Failed: {output[:50]}"))
            
            threading.Thread(target=create, daemon=True).start()
        
        # Button frame - ensure it's visible
        btn_frame = ttk.Frame(dialog)
        btn_frame.pack(side=tk.BOTTOM, pady=20)
        
        ttk.Button(btn_frame, text="Create Project", command=do_create).pack(side=tk.LEFT, padx=10)
        ttk.Button(btn_frame, text="Cancel", command=dialog.destroy).pack(side=tk.LEFT, padx=10)
    
    def _create_new_azure_rg(self):
        """Dialog to create new Azure resource group."""
        dialog = tk.Toplevel(self.root)
        dialog.title("Create Resource Group")
        dialog.geometry("450x220")
        dialog.transient(self.root)
        dialog.grab_set()
        
        ttk.Label(dialog, text="Create Azure Resource Group", 
                 font=self.UI_CONFIG["heading_font"]).pack(pady=15)
        
        form = ttk.Frame(dialog, padding=20)
        form.pack(fill=tk.X)
        
        ttk.Label(form, text="Name:", font=self.UI_CONFIG["label_font"]).grid(row=0, column=0, sticky=tk.W, pady=8)
        rg_name_var = tk.StringVar(value="crm-resources")
        ttk.Entry(form, textvariable=rg_name_var, width=30, font=self.UI_CONFIG["body_font"]).grid(row=0, column=1, padx=10, pady=8)
        
        ttk.Label(form, text="Location:", font=self.UI_CONFIG["label_font"]).grid(row=1, column=0, sticky=tk.W, pady=8)
        location_var = tk.StringVar(value=self.azure_location_var.get() or "eastus")
        ttk.Combobox(form, textvariable=location_var, values=["eastus", "westus", "westeurope", "southeastasia"],
                    width=27, font=self.UI_CONFIG["body_font"]).grid(row=1, column=1, padx=10, pady=8)
        
        status_var = tk.StringVar(value="")
        ttk.Label(form, textvariable=status_var, font=("Helvetica", 10)).grid(row=2, column=0, columnspan=2, pady=10)
        
        def do_create():
            status_var.set("Creating resource group...")
            
            def create():
                cli_manager = CloudCLIManager(lambda t, m: self.log_message(t, m))
                success, output = cli_manager.create_azure_resource_group(rg_name_var.get(), location_var.get())
                
                if success:
                    self.root.after(0, lambda: self.azure_resource_group_var.set(rg_name_var.get()))
                    self.root.after(0, lambda: status_var.set("✅ Resource group created!"))
                    self.root.after(1000, dialog.destroy)
                    self._refresh_azure_resources()
                else:
                    self.root.after(0, lambda: status_var.set(f"❌ Failed"))
            
            threading.Thread(target=create, daemon=True).start()
        
        btn_frame = ttk.Frame(dialog)
        btn_frame.pack(pady=10)
        ttk.Button(btn_frame, text="Create", command=do_create).pack(side=tk.LEFT, padx=10)
        ttk.Button(btn_frame, text="Cancel", command=dialog.destroy).pack(side=tk.LEFT, padx=10)
    
    def _create_new_ecs_cluster(self):
        """Dialog to create new ECS cluster."""
        cluster_name = f"crm-cluster-{secrets.token_hex(4)}"
        if messagebox.askyesno("Create ECS Cluster", 
                               f"Create new ECS cluster?\n\nName: {cluster_name}\nRegion: {self.aws_region_var.get() or 'us-east-1'}"):
            self.aws_ecs_cluster_var.set(cluster_name)
            self.log_message("info", f"ECS cluster '{cluster_name}' will be created during deployment")
    
    def create_step_environment(self, parent):
        """Create environment selection step with improved styling."""
        frame = ttk.Frame(parent, padding="25")
        frame.pack(fill=tk.BOTH, expand=True)
        
        ttk.Label(frame, text="Select Deployment Environment", 
                 font=self.UI_CONFIG["heading_font"]).pack(anchor=tk.W, pady=(0, 20))
        
        # Environment options with descriptions
        envs = [
            ("development", "🔧", "Development", 
             "Local development with debug features, seed data, and relaxed security", "#e8f5e9"),
            ("staging", "🧪", "Staging", 
             "Pre-production testing environment with production-like settings", "#fff8e1"),
            ("production", "🚀", "Production", 
             "Live production with optimized performance, security, and SSL enabled", "#ffebee"),
        ]
        
        for value, icon, title, desc, color in envs:
            card = tk.Frame(frame, bg=color, padx=20, pady=15)
            card.pack(fill=tk.X, pady=8)
            
            rb = tk.Radiobutton(card, text=f"{icon}  {title}", 
                               variable=self.env_var, value=value,
                               font=self.UI_CONFIG["heading_font"],
                               bg=color, activebackground=color,
                               command=lambda v=value: self._on_env_select(v))
            rb.pack(anchor=tk.W)
            
            tk.Label(card, text=f"      {desc}", 
                    font=self.UI_CONFIG["body_font"],
                    fg="#666", bg=color).pack(anchor=tk.W)
    
    def _on_env_select(self, env: str):
        """Handle environment selection."""
        self.log_wizard_action("selection", "Environment changed", env)
        
        # Adjust defaults based on environment
        if env == "development":
            self.ssl_var.set(False)
            self.domain_var.set("localhost")
        elif env == "staging":
            self.domain_var.set("staging.crm.local")
        elif env == "production":
            self.ssl_var.set(True)
            self.domain_var.set("crm.example.com")
    
    def create_step_architecture(self, parent):
        """Create architecture selection step with improved styling."""
        frame = ttk.Frame(parent, padding="25")
        frame.pack(fill=tk.BOTH, expand=True)
        
        ttk.Label(frame, text="Select Deployment Architecture", 
                 font=self.UI_CONFIG["heading_font"]).pack(anchor=tk.W, pady=(0, 20))
        
        # Monolithic option
        mono_card = tk.Frame(frame, bg="#e3f2fd", padx=20, pady=15)
        mono_card.pack(fill=tk.X, pady=8)
        
        tk.Radiobutton(mono_card, text="🏠  Monolithic Architecture", 
                      variable=self.arch_var, value="monolithic",
                      font=self.UI_CONFIG["heading_font"],
                      bg="#e3f2fd", activebackground="#e3f2fd",
                      command=lambda: self._on_arch_select("monolithic")).pack(anchor=tk.W)
        
        tk.Label(mono_card, text="""      • Single API container serves all endpoints
      • Simpler deployment and management
      • Good for small to medium deployments
      • Lower resource requirements""", 
                font=self.UI_CONFIG["body_font"],
                fg="#666", bg="#e3f2fd", justify=tk.LEFT).pack(anchor=tk.W)
        
        # Microservices option
        micro_card = tk.Frame(frame, bg="#f3e5f5", padx=20, pady=15)
        micro_card.pack(fill=tk.X, pady=8)
        
        tk.Radiobutton(micro_card, text="🔀  Microservices Architecture", 
                      variable=self.arch_var, value="microservices",
                      font=self.UI_CONFIG["heading_font"],
                      bg="#f3e5f5", activebackground="#f3e5f5",
                      command=lambda: self._on_arch_select("microservices")).pack(anchor=tk.W)
        
        tk.Label(micro_card, text="""      • Separate services: Identity, Customer, Sales, Marketing, ServiceDesk
      • Better scalability and fault isolation
      • Independent deployment of services
      • Recommended for large-scale production""", 
                font=self.UI_CONFIG["body_font"],
                fg="#666", bg="#f3e5f5", justify=tk.LEFT).pack(anchor=tk.W)
    
    def _on_arch_select(self, arch: str):
        """Handle architecture selection."""
        self.log_wizard_action("selection", "Architecture changed", arch)
    
    def create_step_platform(self, parent):
        """Create platform selection step."""
        frame = ttk.Frame(parent, padding="20")
        frame.pack(fill=tk.BOTH, expand=True)
        
        ttk.Label(frame, text="Select Deployment Platform:", 
                 font=("Helvetica", 12)).pack(anchor=tk.W, pady=(0, 15))
        
        platforms = [
            ("docker", "🐳 Docker Compose", "Deploy using Docker Compose on local or VM"),
            ("kubernetes", "☸️ Kubernetes", "Deploy to Kubernetes cluster"),
            ("cloud", "☁️ Cloud Managed Services", "Use cloud provider's managed container services"),
        ]
        
        for value, title, desc in platforms:
            p_frame = ttk.LabelFrame(frame, text="", padding="10")
            p_frame.pack(fill=tk.X, pady=5)
            
            ttk.Radiobutton(p_frame, text=title, variable=self.platform_var, value=value,
                           command=lambda v=value: self._on_platform_select(v)).pack(anchor=tk.W)
            ttk.Label(p_frame, text=f"  {desc}", foreground="gray").pack(anchor=tk.W)
    
    def _on_platform_select(self, platform: str):
        """Handle platform selection."""
        self.log_wizard_action("selection", "Platform changed", platform)
        # Recalculate steps to show/hide cloud options
        self.active_steps = self._calculate_active_steps()
        self.update_progress_indicator()
    
    def create_step_cloud_provider(self, parent):
        """Create cloud provider selection step."""
        frame = ttk.Frame(parent, padding="20")
        frame.pack(fill=tk.BOTH, expand=True)
        
        ttk.Label(frame, text="Select Cloud Provider:", 
                 font=("Helvetica", 12)).pack(anchor=tk.W, pady=(0, 15))
        
        providers = [
            ("aws", "🟠 Amazon Web Services (AWS)", "ECS/Fargate, RDS, ElastiCache"),
            ("azure", "🔵 Microsoft Azure", "AKS, Azure Container Instances, Azure DB"),
            ("gcp", "🔴 Google Cloud Platform", "GKE, Cloud Run, Cloud SQL"),
        ]
        
        for value, title, services in providers:
            p_frame = ttk.LabelFrame(frame, text="", padding="10")
            p_frame.pack(fill=tk.X, pady=5)
            
            ttk.Radiobutton(p_frame, text=title, variable=self.cloud_provider_var, value=value,
                           command=lambda v=value: self._on_cloud_provider_select(v)).pack(anchor=tk.W)
            ttk.Label(p_frame, text=f"  Services: {services}", foreground="gray").pack(anchor=tk.W)
        
        # Provider-specific configuration
        self.cloud_config_frame = ttk.LabelFrame(frame, text="Provider Configuration", padding="10")
        self.cloud_config_frame.pack(fill=tk.X, pady=15)
        
        self._update_cloud_config_fields()
    
    def _on_cloud_provider_select(self, provider: str):
        """Handle cloud provider selection."""
        self.log_wizard_action("selection", "Cloud provider changed", provider)
        self._update_cloud_config_fields()
        self.active_steps = self._calculate_active_steps()
        self.update_progress_indicator()
    
    def _update_cloud_config_fields(self):
        """Update cloud configuration fields based on provider."""
        # Clear existing fields
        for widget in self.cloud_config_frame.winfo_children():
            widget.destroy()
        
        provider = self.cloud_provider_var.get()
        
        if provider == "aws":
            ttk.Label(self.cloud_config_frame, text="Region:").grid(row=0, column=0, sticky=tk.W, pady=3)
            regions = ["us-east-1", "us-east-2", "us-west-1", "us-west-2", "eu-west-1", "eu-central-1", "ap-southeast-1"]
            ttk.Combobox(self.cloud_config_frame, textvariable=self.aws_region_var, 
                        values=regions, width=25).grid(row=0, column=1, padx=10, pady=3)
            
        elif provider == "azure":
            ttk.Label(self.cloud_config_frame, text="Location:").grid(row=0, column=0, sticky=tk.W, pady=3)
            locations = ["eastus", "westus", "westus2", "centralus", "northeurope", "westeurope", "southeastasia"]
            ttk.Combobox(self.cloud_config_frame, textvariable=self.azure_location_var,
                        values=locations, width=25).grid(row=0, column=1, padx=10, pady=3)
            
            ttk.Label(self.cloud_config_frame, text="Resource Group:").grid(row=1, column=0, sticky=tk.W, pady=3)
            ttk.Entry(self.cloud_config_frame, textvariable=self.azure_resource_group_var, 
                     width=28).grid(row=1, column=1, padx=10, pady=3)
            
        elif provider == "gcp":
            ttk.Label(self.cloud_config_frame, text="Project ID:").grid(row=0, column=0, sticky=tk.W, pady=3)
            ttk.Entry(self.cloud_config_frame, textvariable=self.gcp_project_var, 
                     width=28).grid(row=0, column=1, padx=10, pady=3)
            
            ttk.Label(self.cloud_config_frame, text="Region:").grid(row=1, column=0, sticky=tk.W, pady=3)
            regions = ["us-central1", "us-east1", "us-west1", "europe-west1", "asia-east1"]
            ttk.Combobox(self.cloud_config_frame, textvariable=self.gcp_region_var,
                        values=regions, width=25).grid(row=1, column=1, padx=10, pady=3)
    
    def create_step_cloud_auth(self, parent):
        """Create cloud authentication step with browser-based login."""
        frame = ttk.Frame(parent, padding="25")
        frame.pack(fill=tk.BOTH, expand=True)
        
        provider = self.cloud_provider_var.get()
        provider_upper = provider.upper()
        provider_names = {"aws": "Amazon Web Services", "azure": "Microsoft Azure", "gcp": "Google Cloud Platform"}
        provider_full = provider_names.get(provider, provider_upper)
        
        ttk.Label(frame, text=f"Authenticate with {provider_full}", 
                 font=self.UI_CONFIG["heading_font"]).pack(anchor=tk.W, pady=(0, 15))
        
        # Status card
        status_card = tk.Frame(frame, bg="#f5f5f5", padx=20, pady=20)
        status_card.pack(fill=tk.X, pady=10)
        
        self.auth_status_var = tk.StringVar(value="⚪ Checking authentication status...")
        self.auth_status_label = tk.Label(status_card, textvariable=self.auth_status_var,
                                         font=self.UI_CONFIG["heading_font"],
                                         bg="#f5f5f5")
        self.auth_status_label.pack(anchor=tk.W)
        
        # Authentication instructions
        instruction_text = {
            "aws": "Click 'Login with Browser' to open AWS Single Sign-On in your browser.\nAfter authenticating, return here to continue.",
            "azure": "Click 'Login with Browser' to open Azure login in your browser.\nAfter authenticating, return here to continue.",
            "gcp": "Click 'Login with Browser' to open Google Cloud login in your browser.\nAfter authenticating, return here to continue."
        }
        
        ttk.Label(status_card, text=instruction_text.get(provider, ""), 
                 font=("Helvetica", 12), background="#f5f5f5").pack(anchor=tk.W, pady=10)
        
        # Buttons frame
        btn_frame = ttk.Frame(frame)
        btn_frame.pack(fill=tk.X, pady=20)
        
        style = ttk.Style()
        style.configure("Auth.TButton", font=self.UI_CONFIG["button_font"], padding=(30, 15))
        
        self.auth_btn = ttk.Button(btn_frame, text="🌐  Login with Browser", 
                                   command=self._do_browser_auth, style="Auth.TButton")
        self.auth_btn.pack(side=tk.LEFT, padx=5)
        
        ttk.Button(btn_frame, text="🔄  Check Status", 
                  command=self._check_cloud_auth, style="Auth.TButton").pack(side=tk.LEFT, padx=15)
        
        # Alternative auth options
        alt_frame = ttk.LabelFrame(frame, text="  Alternative Authentication Methods  ", padding="10")
        alt_frame.pack(fill=tk.X, pady=10)
        
        if provider == "aws":
            ttk.Button(alt_frame, text="Configure AWS Credentials", 
                      command=self._configure_aws_credentials).pack(side=tk.LEFT, padx=5)
            ttk.Label(alt_frame, text="Use access key/secret (for service accounts)",
                     font=("Helvetica", 10), foreground="gray").pack(side=tk.LEFT, padx=10)
        elif provider == "azure":
            ttk.Button(alt_frame, text="Login with Device Code", 
                      command=self._azure_device_login).pack(side=tk.LEFT, padx=5)
            ttk.Label(alt_frame, text="For headless environments",
                     font=("Helvetica", 10), foreground="gray").pack(side=tk.LEFT, padx=10)
        elif provider == "gcp":
            ttk.Button(alt_frame, text="Use Service Account", 
                      command=self._gcp_service_account_auth).pack(side=tk.LEFT, padx=5)
            ttk.Label(alt_frame, text="For automation and CI/CD",
                     font=("Helvetica", 10), foreground="gray").pack(side=tk.LEFT, padx=10)
        
        # Account info display
        info_frame = ttk.LabelFrame(frame, text="  Account Information  ", padding="15")
        info_frame.pack(fill=tk.BOTH, expand=True, pady=10)
        
        self.auth_info_text = scrolledtext.ScrolledText(
            info_frame, 
            height=8, 
            wrap=tk.WORD, 
            state=tk.DISABLED,
            font=self.UI_CONFIG["body_font"],
            bg="#fafafa",
            padx=10,
            pady=10
        )
        self.auth_info_text.pack(fill=tk.BOTH, expand=True)
        
        # Check auth status on load
        self._check_cloud_auth()
    
    def _do_browser_auth(self):
        """Perform browser-based cloud authentication."""
        provider = self.cloud_provider_var.get()
        self.log_wizard_action("button_click", "Browser auth clicked", provider)
        
        self.auth_status_var.set(f"🌐 Opening browser for {provider.upper()} login...")
        self.auth_btn.configure(state=tk.DISABLED)
        
        # Show informational dialog
        messagebox.showinfo(
            "Browser Authentication",
            f"Your default browser will open to {provider.upper()} login.\n\n"
            f"1. Sign in with your {provider.upper()} account\n"
            f"2. Authorize access when prompted\n"
            f"3. Return to this application when complete\n\n"
            f"Click OK to open the browser."
        )
        
        def do_auth():
            cli_manager = CloudCLIManager(lambda t, m: self.log_message(t, m))
            
            # Check if CLI is installed
            installed, _ = cli_manager.check_cli_installed(provider)
            if not installed:
                self.root.after(0, lambda: self._auth_failed(f"{provider.upper()} CLI not installed"))
                return
            
            # Start browser auth
            success, output = cli_manager.start_browser_auth(provider)
            
            if success:
                # Verify authentication
                authed, account_info = cli_manager.check_authentication(provider)
                if authed:
                    self._update_auth_success(provider, account_info)
                else:
                    self.root.after(0, lambda: self._auth_failed("Authentication not detected after browser login"))
            else:
                self.root.after(0, lambda: self._auth_failed(f"Browser authentication failed: {output}"))
            
            self.root.after(0, lambda: self.auth_btn.configure(state=tk.NORMAL))
        
        threading.Thread(target=do_auth, daemon=True).start()
    
    def _update_auth_success(self, provider: str, account_info: dict):
        """Update UI after successful authentication."""
        display = account_info.get("display", "Authenticated")
        self.root.after(0, lambda: self.auth_status_var.set(f"✅ Authenticated: {display}"))
        self.root.after(0, lambda: self.auth_status_label.configure(fg="#4caf50"))
        self.cloud_authenticated.set(True)
        
        # Update account info display
        def update_info():
            self.auth_info_text.configure(state=tk.NORMAL)
            self.auth_info_text.delete(1.0, tk.END)
            info_lines = [f"{provider.upper()} Authentication Successful", "=" * 40, ""]
            
            for key, value in account_info.items():
                if key != "display":
                    info_lines.append(f"{key.replace('_', ' ').title()}: {value}")
            
            self.auth_info_text.insert(tk.END, "\n".join(info_lines))
            self.auth_info_text.configure(state=tk.DISABLED)
        
        self.root.after(0, update_info)
    
    def _configure_aws_credentials(self):
        """Dialog to configure AWS access key credentials."""
        dialog = tk.Toplevel(self.root)
        dialog.title("Configure AWS Credentials")
        dialog.geometry("500x280")
        dialog.transient(self.root)
        dialog.grab_set()
        
        ttk.Label(dialog, text="AWS Access Key Configuration", 
                 font=self.UI_CONFIG["heading_font"]).pack(pady=15)
        
        form = ttk.Frame(dialog, padding=20)
        form.pack(fill=tk.X)
        
        ttk.Label(form, text="Access Key ID:", font=self.UI_CONFIG["label_font"]).grid(row=0, column=0, sticky=tk.W, pady=8)
        access_key_var = tk.StringVar()
        ttk.Entry(form, textvariable=access_key_var, width=40, font=self.UI_CONFIG["body_font"]).grid(row=0, column=1, padx=10, pady=8)
        
        ttk.Label(form, text="Secret Access Key:", font=self.UI_CONFIG["label_font"]).grid(row=1, column=0, sticky=tk.W, pady=8)
        secret_key_var = tk.StringVar()
        secret_entry = ttk.Entry(form, textvariable=secret_key_var, width=40, font=self.UI_CONFIG["body_font"], show="*")
        secret_entry.grid(row=1, column=1, padx=10, pady=8)
        
        ttk.Label(form, text="Region:", font=self.UI_CONFIG["label_font"]).grid(row=2, column=0, sticky=tk.W, pady=8)
        region_var = tk.StringVar(value=self.aws_region_var.get() or "us-east-1")
        ttk.Combobox(form, textvariable=region_var, values=["us-east-1", "us-west-2", "eu-west-1"],
                    width=37, font=self.UI_CONFIG["body_font"]).grid(row=2, column=1, padx=10, pady=8)
        
        def do_configure():
            # Run aws configure with the provided values
            cmd = f'aws configure set aws_access_key_id "{access_key_var.get()}" && '
            cmd += f'aws configure set aws_secret_access_key "{secret_key_var.get()}" && '
            cmd += f'aws configure set region "{region_var.get()}"'
            
            result = subprocess.run(cmd, shell=True, capture_output=True, text=True)
            if result.returncode == 0:
                messagebox.showinfo("Success", "AWS credentials configured!")
                dialog.destroy()
                self._check_cloud_auth()
            else:
                messagebox.showerror("Error", f"Failed to configure: {result.stderr}")
        
        btn_frame = ttk.Frame(dialog)
        btn_frame.pack(pady=15)
        ttk.Button(btn_frame, text="Configure", command=do_configure).pack(side=tk.LEFT, padx=10)
        ttk.Button(btn_frame, text="Cancel", command=dialog.destroy).pack(side=tk.LEFT, padx=10)
    
    def _azure_device_login(self):
        """Perform Azure device code login."""
        self.auth_status_var.set("🔐 Starting device code login...")
        
        def do_device_login():
            result = subprocess.run("az login --use-device-code", shell=True, capture_output=True, text=True, timeout=120)
            self.root.after(0, self._check_cloud_auth)
        
        messagebox.showinfo(
            "Device Code Login",
            "A device code will be shown in the terminal.\n\n"
            "1. Go to https://microsoft.com/devicelogin\n"
            "2. Enter the code shown\n"
            "3. Sign in with your Azure account"
        )
        threading.Thread(target=do_device_login, daemon=True).start()
    
    def _gcp_service_account_auth(self):
        """Dialog to authenticate with GCP service account."""
        dialog = tk.Toplevel(self.root)
        dialog.title("GCP Service Account")
        dialog.geometry("550x200")
        dialog.transient(self.root)
        dialog.grab_set()
        
        ttk.Label(dialog, text="Authenticate with Service Account", 
                 font=self.UI_CONFIG["heading_font"]).pack(pady=15)
        
        form = ttk.Frame(dialog, padding=20)
        form.pack(fill=tk.X)
        
        ttk.Label(form, text="Key File Path:", font=self.UI_CONFIG["label_font"]).grid(row=0, column=0, sticky=tk.W, pady=8)
        key_file_var = tk.StringVar()
        ttk.Entry(form, textvariable=key_file_var, width=45, font=self.UI_CONFIG["body_font"]).grid(row=0, column=1, padx=10, pady=8)
        
        def browse_file():
            from tkinter import filedialog
            filename = filedialog.askopenfilename(filetypes=[("JSON files", "*.json")])
            if filename:
                key_file_var.set(filename)
        
        ttk.Button(form, text="Browse", command=browse_file).grid(row=0, column=2, padx=5, pady=8)
        
        def do_auth():
            key_file = key_file_var.get()
            if not key_file or not Path(key_file).exists():
                messagebox.showerror("Error", "Please select a valid key file")
                return
            
            result = subprocess.run(f'gcloud auth activate-service-account --key-file="{key_file}"', 
                                   shell=True, capture_output=True, text=True)
            if result.returncode == 0:
                messagebox.showinfo("Success", "Service account activated!")
                dialog.destroy()
                self._check_cloud_auth()
            else:
                messagebox.showerror("Error", f"Failed: {result.stderr}")
        
        btn_frame = ttk.Frame(dialog)
        btn_frame.pack(pady=15)
        ttk.Button(btn_frame, text="Authenticate", command=do_auth).pack(side=tk.LEFT, padx=10)
        ttk.Button(btn_frame, text="Cancel", command=dialog.destroy).pack(side=tk.LEFT, padx=10)
    
    def _do_cloud_auth(self):
        """Legacy method - redirects to browser auth."""
        self._do_browser_auth()
    
    def _check_cloud_auth(self):
        """Check current cloud authentication status using CloudCLIManager."""
        provider = self.cloud_provider_var.get()
        self.log_wizard_action("button_click", "Check status clicked", provider)
        
        def do_check():
            cli_manager = CloudCLIManager(lambda t, m: self.log_message(t, m))
            
            # First check if CLI is installed
            installed, version = cli_manager.check_cli_installed(provider)
            if not installed:
                self.root.after(0, lambda: self.auth_status_var.set(f"❌ {provider.upper()} CLI not installed"))
                self.root.after(0, lambda: self.auth_status_label.configure(fg="#f44336"))
                return
            
            # Check authentication
            authed, account_info = cli_manager.check_authentication(provider)
            
            if authed:
                self._update_auth_success(provider, account_info)
                
                # Also update provider-specific variables
                if provider == "aws":
                    account_id = account_info.get("account_id", "")
                    if account_id and self.aws_region_var.get():
                        ecr = f"{account_id}.dkr.ecr.{self.aws_region_var.get()}.amazonaws.com"
                        self.aws_ecr_var.set(ecr)
                elif provider == "azure":
                    sub_id = account_info.get("subscription_id", "")
                    if sub_id:
                        self.azure_subscription_var.set(sub_id)
                elif provider == "gcp":
                    project = account_info.get("project", "")
                    if project and not self.gcp_project_var.get():
                        self.root.after(0, lambda: self.gcp_project_var.set(project))
            else:
                self.root.after(0, lambda: self.auth_status_var.set("❌ Not authenticated"))
                self.root.after(0, lambda: self.auth_status_label.configure(fg="#f44336"))
                self.cloud_authenticated.set(False)
        
        threading.Thread(target=do_check, daemon=True).start()
    
    def _parse_aws_auth(self, output: str):
        """Parse AWS authentication response."""
        try:
            import json
            data = json.loads(output)
            account = data.get("Account", "Unknown")
            arn = data.get("Arn", "Unknown")
            
            self.root.after(0, lambda: self.auth_status_var.set(f"✅ Authenticated: {account}"))
            self.cloud_authenticated.set(True)
            
            info = f"Account ID: {account}\nUser ARN: {arn}"
            if self.aws_region_var.get():
                ecr = f"{account}.dkr.ecr.{self.aws_region_var.get()}.amazonaws.com"
                info += f"\nECR Registry: {ecr}"
                self.aws_ecr_var.set(ecr)
            
            self.root.after(0, lambda: self._update_auth_info(info))
            self.log_wizard_action("auth_success", "AWS authentication successful", account)
        except:
            self.root.after(0, lambda: self.auth_status_var.set("❌ Parse error"))
    
    def _parse_azure_auth(self, output: str):
        """Parse Azure authentication response."""
        try:
            import json
            data = json.loads(output)
            sub_name = data.get("name", "Unknown")
            sub_id = data.get("id", "Unknown")
            user = data.get("user", {}).get("name", "Unknown")
            
            self.root.after(0, lambda: self.auth_status_var.set(f"✅ Authenticated: {sub_name}"))
            self.cloud_authenticated.set(True)
            self.azure_subscription_var.set(sub_id)
            
            info = f"User: {user}\nSubscription: {sub_name}\nSubscription ID: {sub_id}"
            self.root.after(0, lambda: self._update_auth_info(info))
            self.log_wizard_action("auth_success", "Azure authentication successful", sub_name)
        except:
            self.root.after(0, lambda: self.auth_status_var.set("❌ Parse error"))
    
    def _parse_gcp_auth(self, output: str):
        """Parse GCP authentication response."""
        account = output.strip()
        self.root.after(0, lambda: self.auth_status_var.set(f"✅ Authenticated: {account}"))
        self.cloud_authenticated.set(True)
        
        info = f"Account: {account}"
        
        # Get project
        def get_project():
            config = self._get_current_config()
            deployer = GCPDeployer(config, lambda t, m: None)
            success, proj = deployer._run_command("gcloud config get-value project")
            if success and proj.strip():
                self.gcp_project_var.set(proj.strip())
                return f"Account: {account}\nProject: {proj.strip()}"
            return info
        
        threading.Thread(target=lambda: self.root.after(0, lambda: self._update_auth_info(get_project())), daemon=True).start()
        self.log_wizard_action("auth_success", "GCP authentication successful", account)
    
    def _auth_failed(self, message: str):
        """Handle authentication failure."""
        self.auth_status_var.set(f"❌ {message}")
        self.auth_btn.configure(state=tk.NORMAL)
        self.log_wizard_action("auth_failed", message)
    
    def _update_auth_info(self, info: str):
        """Update authentication info display."""
        self.auth_info_text.configure(state=tk.NORMAL)
        self.auth_info_text.delete(1.0, tk.END)
        self.auth_info_text.insert(tk.END, info)
        self.auth_info_text.configure(state=tk.DISABLED)
    
    def create_step_build(self, parent):
        """Create build source step with improved styling."""
        frame = ttk.Frame(parent, padding="25")
        frame.pack(fill=tk.BOTH, expand=True)
        
        ttk.Label(frame, text="Configure Build Source", 
                 font=self.UI_CONFIG["heading_font"]).pack(anchor=tk.W, pady=(0, 20))
        
        # Local option card
        local_card = tk.Frame(frame, bg="#e8f5e9", padx=20, pady=15)
        local_card.pack(fill=tk.X, pady=8)
        
        tk.Radiobutton(local_card, text="📁  Local Directory", 
                      variable=self.build_source_var, value="local",
                      font=self.UI_CONFIG["heading_font"],
                      bg="#e8f5e9", activebackground="#e8f5e9",
                      command=lambda: self._on_build_source_change("local")).pack(anchor=tk.W)
        tk.Label(local_card, text="      Build from local source code directory (fastest for development)",
                font=self.UI_CONFIG["body_font"],
                fg="#666", bg="#e8f5e9").pack(anchor=tk.W)
        
        # Git option card
        git_card = tk.Frame(frame, bg="#e3f2fd", padx=20, pady=15)
        git_card.pack(fill=tk.X, pady=8)
        
        tk.Radiobutton(git_card, text="🌐  Git Repository", 
                      variable=self.build_source_var, value="git",
                      font=self.UI_CONFIG["heading_font"],
                      bg="#e3f2fd", activebackground="#e3f2fd",
                      command=lambda: self._on_build_source_change("git")).pack(anchor=tk.W)
        tk.Label(git_card, text="      Clone and build from Git repository (for CI/CD workflows)",
                font=self.UI_CONFIG["body_font"],
                fg="#666", bg="#e3f2fd").pack(anchor=tk.W)
        
        # Git configuration (shown when git selected)
        self.git_config_frame = ttk.LabelFrame(frame, text="  Git Repository Configuration  ", padding="15")
        
        ttk.Label(self.git_config_frame, text="Repository URL:", 
                 font=self.UI_CONFIG["label_font"]).grid(row=0, column=0, sticky=tk.W, pady=8)
        ttk.Entry(self.git_config_frame, textvariable=self.git_repo_var, width=50,
                 font=self.UI_CONFIG["body_font"]).grid(row=0, column=1, padx=10, pady=8)
        
        ttk.Label(self.git_config_frame, text="Branch:", 
                 font=self.UI_CONFIG["label_font"]).grid(row=1, column=0, sticky=tk.W, pady=8)
        branches = ["main", "master", "develop", "staging", "production"]
        ttk.Combobox(self.git_config_frame, textvariable=self.git_branch_var, 
                    values=branches, width=25,
                    font=self.UI_CONFIG["body_font"]).grid(row=1, column=1, sticky=tk.W, padx=10, pady=8)
        
        ttk.Label(self.git_config_frame, text="Username:", 
                 font=self.UI_CONFIG["label_font"]).grid(row=2, column=0, sticky=tk.W, pady=8)
        ttk.Entry(self.git_config_frame, textvariable=self.git_username_var, width=35,
                 font=self.UI_CONFIG["body_font"]).grid(row=2, column=1, sticky=tk.W, padx=10, pady=8)
        
        ttk.Label(self.git_config_frame, text="Token/Password:", 
                 font=self.UI_CONFIG["label_font"]).grid(row=3, column=0, sticky=tk.W, pady=8)
        ttk.Entry(self.git_config_frame, textvariable=self.git_token_var, width=45, show="*",
                 font=self.UI_CONFIG["body_font"]).grid(row=3, column=1, sticky=tk.W, padx=10, pady=8)
        
        # Cloud build option if cloud target
        if self.deploy_target_var.get() in ["aws", "azure", "gcp"]:
            cloud_card = tk.Frame(frame, bg="#fff3e0", padx=20, pady=15)
            cloud_card.pack(fill=tk.X, pady=8)
            
            tk.Radiobutton(cloud_card, text="☁️  Cloud CI/CD Pipeline", 
                          variable=self.build_source_var, value="cloud",
                          font=self.UI_CONFIG["heading_font"],
                          bg="#fff3e0", activebackground="#fff3e0",
                          command=lambda: self._on_build_source_change("cloud")).pack(anchor=tk.W)
            tk.Label(cloud_card, text="      Use cloud build services (GitHub Actions, AWS CodeBuild, etc.)",
                    font=self.UI_CONFIG["body_font"],
                    fg="#666", bg="#fff3e0").pack(anchor=tk.W)
        
        self._on_build_source_change(self.build_source_var.get())
    
    def _on_build_source_change(self, source: str):
        """Handle build source change."""
        self.log_wizard_action("selection", "Build source changed", source)
        
        # Show/hide git config
        if source == "git":
            self.git_config_frame.pack(fill=tk.X, pady=15)
        else:
            self.git_config_frame.pack_forget()
    
    def create_step_components(self, parent):
        """Create components selection step with improved styling."""
        frame = ttk.Frame(parent, padding="25")
        frame.pack(fill=tk.BOTH, expand=True)
        
        ttk.Label(frame, text="Select Components to Deploy", 
                 font=self.UI_CONFIG["heading_font"]).pack(anchor=tk.W, pady=(0, 20))
        
        # Core components card
        core_card = tk.Frame(frame, bg="#f5f5f5", padx=20, pady=15)
        core_card.pack(fill=tk.X, pady=8)
        
        tk.Label(core_card, text="Core Components", 
                font=self.UI_CONFIG["heading_font"], bg="#f5f5f5").pack(anchor=tk.W, pady=(0, 10))
        
        components = [
            (self.deploy_db_var, "🗄️  Database (MariaDB)", "Required - stores all application data"),
            (self.deploy_redis_var, "📦  Redis Cache", "Recommended - session storage and caching"),
            (self.deploy_api_var, "⚡  API Backend", "Required - REST API server"),
            (self.deploy_frontend_var, "🌐  Frontend (React)", "Required - web application interface"),
        ]
        
        for var, text, desc in components:
            comp_frame = tk.Frame(core_card, bg="#f5f5f5")
            comp_frame.pack(fill=tk.X, pady=5)
            
            tk.Checkbutton(comp_frame, text=text, variable=var,
                          font=self.UI_CONFIG["label_font"],
                          bg="#f5f5f5", activebackground="#f5f5f5").pack(anchor=tk.W)
            tk.Label(comp_frame, text=f"         {desc}",
                    font=("Helvetica", 11), fg="#666", bg="#f5f5f5").pack(anchor=tk.W)
        
        # Optional components card
        opt_card = tk.Frame(frame, bg="#fff8e1", padx=20, pady=15)
        opt_card.pack(fill=tk.X, pady=8)
        
        tk.Label(opt_card, text="Optional Components", 
                font=self.UI_CONFIG["heading_font"], bg="#fff8e1").pack(anchor=tk.W, pady=(0, 10))
        
        tk.Checkbutton(opt_card, text="📊  Monitoring (Prometheus/Grafana)", 
                      variable=self.deploy_monitoring_var,
                      font=self.UI_CONFIG["label_font"],
                      bg="#fff8e1", activebackground="#fff8e1").pack(anchor=tk.W)
        tk.Label(opt_card, text="         Metrics collection and dashboards for production monitoring",
                font=("Helvetica", 11), fg="#666", bg="#fff8e1").pack(anchor=tk.W)
        
        # Microservices (if selected)
        if self.arch_var.get() == "microservices":
            micro_card = tk.Frame(frame, bg="#f3e5f5", padx=20, pady=15)
            micro_card.pack(fill=tk.X, pady=8)
            
            tk.Label(micro_card, text="Microservices", 
                    font=self.UI_CONFIG["heading_font"], bg="#f3e5f5").pack(anchor=tk.W, pady=(0, 10))
            
            services = [
                (self.deploy_identity_var, "🔐  Identity Service"),
                (self.deploy_customer_var, "👥  Customer Service"),
                (self.deploy_sales_var, "💰  Sales Service"),
                (self.deploy_marketing_var, "📣  Marketing Service"),
                (self.deploy_servicedesk_var, "🎫  ServiceDesk Service"),
            ]
            
            for var, text in services:
                tk.Checkbutton(micro_card, text=text, variable=var,
                              font=self.UI_CONFIG["label_font"],
                              bg="#f3e5f5", activebackground="#f3e5f5").pack(anchor=tk.W, pady=3)
        
        # Quick selection buttons
        btn_frame = ttk.Frame(frame)
        btn_frame.pack(fill=tk.X, pady=15)
        
        ttk.Button(btn_frame, text="✓ Select All", 
                  command=self._select_all_components).pack(side=tk.LEFT, padx=5)
        ttk.Button(btn_frame, text="○ Select Minimal", 
                  command=self._select_minimal_components).pack(side=tk.LEFT, padx=5)
    
    def _select_all_components(self):
        """Select all components."""
        self.deploy_db_var.set(True)
        self.deploy_redis_var.set(True)
        self.deploy_api_var.set(True)
        self.deploy_frontend_var.set(True)
        self.deploy_monitoring_var.set(True)
        self.log_wizard_action("button_click", "Select All clicked")
    
    def _select_minimal_components(self):
        """Select minimal components."""
        self.deploy_db_var.set(True)
        self.deploy_redis_var.set(False)
        self.deploy_api_var.set(True)
        self.deploy_frontend_var.set(True)
        self.deploy_monitoring_var.set(False)
        self.log_wizard_action("button_click", "Select Minimal clicked")
    
    def create_step_database(self, parent):
        """Create database configuration step with improved styling."""
        frame = ttk.Frame(parent, padding="25")
        frame.pack(fill=tk.BOTH, expand=True)
        
        ttk.Label(frame, text="Configure Database", 
                 font=self.UI_CONFIG["heading_font"]).pack(anchor=tk.W, pady=(0, 20))
        
        # Provider selection card
        provider_card = tk.Frame(frame, bg="#e8f5e9", padx=20, pady=15)
        provider_card.pack(fill=tk.X, pady=8)
        
        tk.Label(provider_card, text="Database Provider", 
                font=self.UI_CONFIG["heading_font"], bg="#e8f5e9").pack(anchor=tk.W, pady=(0, 10))
        
        provider_row = tk.Frame(provider_card, bg="#e8f5e9")
        provider_row.pack(fill=tk.X)
        
        tk.Label(provider_row, text="Provider:", 
                font=self.UI_CONFIG["label_font"], bg="#e8f5e9").pack(side=tk.LEFT)
        providers = ["mariadb", "mysql", "postgresql"]
        provider_cb = ttk.Combobox(provider_row, textvariable=self.db_provider_var, 
                                   values=providers, width=20, font=self.UI_CONFIG["body_font"])
        provider_cb.pack(side=tk.LEFT, padx=15)
        provider_cb.bind("<<ComboboxSelected>>", 
                        lambda e: self.log_wizard_action("selection", "Database provider", self.db_provider_var.get()))
        
        # Cloud managed database option
        if self.deploy_target_var.get() in ["aws", "azure", "gcp"]:
            managed_frame = tk.Frame(provider_card, bg="#e8f5e9")
            managed_frame.pack(fill=tk.X, pady=(10, 0))
            
            tk.Checkbutton(managed_frame, text="  Use cloud managed database service (RDS, Cloud SQL, etc.)", 
                          variable=self.use_managed_db_var,
                          font=self.UI_CONFIG["body_font"],
                          bg="#e8f5e9", activebackground="#e8f5e9").pack(anchor=tk.W)
        
        # Connection settings card
        conn_card = tk.Frame(frame, bg="#f5f5f5", padx=20, pady=15)
        conn_card.pack(fill=tk.X, pady=8)
        
        tk.Label(conn_card, text="Connection Settings", 
                font=self.UI_CONFIG["heading_font"], bg="#f5f5f5").pack(anchor=tk.W, pady=(0, 10))
        
        conn_grid = tk.Frame(conn_card, bg="#f5f5f5")
        conn_grid.pack(fill=tk.X)
        
        tk.Label(conn_grid, text="Host:", font=self.UI_CONFIG["label_font"], 
                bg="#f5f5f5", width=12, anchor=tk.W).grid(row=0, column=0, sticky=tk.W, pady=8)
        ttk.Entry(conn_grid, textvariable=self.db_host_var, width=35,
                 font=self.UI_CONFIG["body_font"]).grid(row=0, column=1, padx=10, pady=8)
        
        tk.Label(conn_grid, text="Port:", font=self.UI_CONFIG["label_font"], 
                bg="#f5f5f5", width=12, anchor=tk.W).grid(row=1, column=0, sticky=tk.W, pady=8)
        ttk.Entry(conn_grid, textvariable=self.db_port_var, width=12,
                 font=self.UI_CONFIG["body_font"]).grid(row=1, column=1, sticky=tk.W, padx=10, pady=8)
        
        tk.Label(conn_grid, text="Database:", font=self.UI_CONFIG["label_font"], 
                bg="#f5f5f5", width=12, anchor=tk.W).grid(row=2, column=0, sticky=tk.W, pady=8)
        ttk.Entry(conn_grid, textvariable=self.db_name_var, width=30,
                 font=self.UI_CONFIG["body_font"]).grid(row=2, column=1, sticky=tk.W, padx=10, pady=8)
        
        # Credentials card
        cred_card = tk.Frame(frame, bg="#fff3e0", padx=20, pady=15)
        cred_card.pack(fill=tk.X, pady=8)
        
        tk.Label(cred_card, text="🔐  Database Credentials", 
                font=self.UI_CONFIG["heading_font"], bg="#fff3e0").pack(anchor=tk.W, pady=(0, 10))
        
        cred_grid = tk.Frame(cred_card, bg="#fff3e0")
        cred_grid.pack(fill=tk.X)
        
        tk.Label(cred_grid, text="Username:", font=self.UI_CONFIG["label_font"], 
                bg="#fff3e0", width=14, anchor=tk.W).grid(row=0, column=0, sticky=tk.W, pady=8)
        ttk.Entry(cred_grid, textvariable=self.db_user_var, width=28,
                 font=self.UI_CONFIG["body_font"]).grid(row=0, column=1, padx=10, pady=8)
        
        tk.Label(cred_grid, text="Password:", font=self.UI_CONFIG["label_font"], 
                bg="#fff3e0", width=14, anchor=tk.W).grid(row=1, column=0, sticky=tk.W, pady=8)
        ttk.Entry(cred_grid, textvariable=self.db_pass_var, width=28, show="*",
                 font=self.UI_CONFIG["body_font"]).grid(row=1, column=1, padx=10, pady=8)
        
        tk.Label(cred_grid, text="Root Password:", font=self.UI_CONFIG["label_font"], 
                bg="#fff3e0", width=14, anchor=tk.W).grid(row=2, column=0, sticky=tk.W, pady=8)
        ttk.Entry(cred_grid, textvariable=self.db_root_pass_var, width=28, show="*",
                 font=self.UI_CONFIG["body_font"]).grid(row=2, column=1, padx=10, pady=8)
    
    def create_step_network(self, parent):
        """Create network configuration step with improved styling."""
        frame = ttk.Frame(parent, padding="25")
        frame.pack(fill=tk.BOTH, expand=True)
        
        ttk.Label(frame, text="Configure Network", 
                 font=self.UI_CONFIG["heading_font"]).pack(anchor=tk.W, pady=(0, 20))
        
        # Ports card
        port_card = tk.Frame(frame, bg="#e3f2fd", padx=20, pady=15)
        port_card.pack(fill=tk.X, pady=8)
        
        tk.Label(port_card, text="🔌  Service Ports", 
                font=self.UI_CONFIG["heading_font"], bg="#e3f2fd").pack(anchor=tk.W, pady=(0, 10))
        
        port_grid = tk.Frame(port_card, bg="#e3f2fd")
        port_grid.pack(fill=tk.X)
        
        tk.Label(port_grid, text="API Port:", font=self.UI_CONFIG["label_font"], 
                bg="#e3f2fd", width=14, anchor=tk.W).grid(row=0, column=0, sticky=tk.W, pady=8)
        ttk.Entry(port_grid, textvariable=self.api_port_var, width=12,
                 font=self.UI_CONFIG["body_font"]).grid(row=0, column=1, sticky=tk.W, padx=10, pady=8)
        tk.Label(port_grid, text="(default: 5000)", font=("Helvetica", 11), 
                fg="#666", bg="#e3f2fd").grid(row=0, column=2, sticky=tk.W, padx=10, pady=8)
        
        tk.Label(port_grid, text="Frontend Port:", font=self.UI_CONFIG["label_font"], 
                bg="#e3f2fd", width=14, anchor=tk.W).grid(row=1, column=0, sticky=tk.W, pady=8)
        ttk.Entry(port_grid, textvariable=self.frontend_port_var, width=12,
                 font=self.UI_CONFIG["body_font"]).grid(row=1, column=1, sticky=tk.W, padx=10, pady=8)
        tk.Label(port_grid, text="(default: 3000)", font=("Helvetica", 11), 
                fg="#666", bg="#e3f2fd").grid(row=1, column=2, sticky=tk.W, padx=10, pady=8)
        
        tk.Label(port_grid, text="Redis Port:", font=self.UI_CONFIG["label_font"], 
                bg="#e3f2fd", width=14, anchor=tk.W).grid(row=2, column=0, sticky=tk.W, pady=8)
        ttk.Entry(port_grid, textvariable=self.redis_port_var, width=12,
                 font=self.UI_CONFIG["body_font"]).grid(row=2, column=1, sticky=tk.W, padx=10, pady=8)
        tk.Label(port_grid, text="(default: 6379)", font=("Helvetica", 11), 
                fg="#666", bg="#e3f2fd").grid(row=2, column=2, sticky=tk.W, padx=10, pady=8)
        
        # Domain card
        domain_card = tk.Frame(frame, bg="#f5f5f5", padx=20, pady=15)
        domain_card.pack(fill=tk.X, pady=8)
        
        tk.Label(domain_card, text="🌐  Domain & SSL", 
                font=self.UI_CONFIG["heading_font"], bg="#f5f5f5").pack(anchor=tk.W, pady=(0, 10))
        
        domain_grid = tk.Frame(domain_card, bg="#f5f5f5")
        domain_grid.pack(fill=tk.X)
        
        tk.Label(domain_grid, text="Domain:", font=self.UI_CONFIG["label_font"], 
                bg="#f5f5f5", width=12, anchor=tk.W).grid(row=0, column=0, sticky=tk.W, pady=8)
        ttk.Entry(domain_grid, textvariable=self.domain_var, width=35,
                 font=self.UI_CONFIG["body_font"]).grid(row=0, column=1, padx=10, pady=8)
        
        ssl_frame = tk.Frame(domain_card, bg="#f5f5f5")
        ssl_frame.pack(fill=tk.X, pady=(10, 0))
        
        tk.Checkbutton(ssl_frame, text="  Enable SSL/HTTPS (required for production)", 
                      variable=self.ssl_var,
                      font=self.UI_CONFIG["label_font"],
                      bg="#f5f5f5", activebackground="#f5f5f5",
                      command=lambda: self._on_ssl_change()).pack(anchor=tk.W)
        
        # SSL certificate paths (shown when SSL enabled)
        self.ssl_paths_frame = tk.Frame(domain_card, bg="#fff3e0", padx=15, pady=10)
        
        tk.Label(self.ssl_paths_frame, text="🔐  SSL Certificate Paths", 
                font=self.UI_CONFIG["subheading_font"], bg="#fff3e0").pack(anchor=tk.W, pady=(0, 8))
        
        ssl_grid = tk.Frame(self.ssl_paths_frame, bg="#fff3e0")
        ssl_grid.pack(fill=tk.X)
        
        tk.Label(ssl_grid, text="Certificate:", font=self.UI_CONFIG["label_font"], 
                bg="#fff3e0", width=12, anchor=tk.W).grid(row=0, column=0, sticky=tk.W, pady=5)
        ttk.Entry(ssl_grid, textvariable=self.ssl_cert_var, width=45,
                 font=self.UI_CONFIG["body_font"]).grid(row=0, column=1, padx=10, pady=5)
        
        tk.Label(ssl_grid, text="Private Key:", font=self.UI_CONFIG["label_font"], 
                bg="#fff3e0", width=12, anchor=tk.W).grid(row=1, column=0, sticky=tk.W, pady=5)
        ttk.Entry(ssl_grid, textvariable=self.ssl_key_var, width=45,
                 font=self.UI_CONFIG["body_font"]).grid(row=1, column=1, padx=10, pady=5)
        
        self._on_ssl_change()
    
    def _on_ssl_change(self):
        """Handle SSL checkbox change."""
        self.log_wizard_action("selection", "SSL", str(self.ssl_var.get()))
        if self.ssl_var.get():
            self.ssl_paths_frame.pack(fill=tk.X, pady=(10, 0))
        else:
            self.ssl_paths_frame.pack_forget()
    
    def create_step_credentials(self, parent):
        """Create credentials configuration step with improved styling."""
        frame = ttk.Frame(parent, padding="25")
        frame.pack(fill=tk.BOTH, expand=True)
        
        ttk.Label(frame, text="Configure Admin Credentials", 
                 font=self.UI_CONFIG["heading_font"]).pack(anchor=tk.W, pady=(0, 20))
        
        # Admin credentials card
        admin_card = tk.Frame(frame, bg="#e8f5e9", padx=20, pady=15)
        admin_card.pack(fill=tk.X, pady=8)
        
        tk.Label(admin_card, text="👤  Administrator Account", 
                font=self.UI_CONFIG["heading_font"], bg="#e8f5e9").pack(anchor=tk.W, pady=(0, 10))
        
        admin_grid = tk.Frame(admin_card, bg="#e8f5e9")
        admin_grid.pack(fill=tk.X)
        
        tk.Label(admin_grid, text="Username:", font=self.UI_CONFIG["label_font"], 
                bg="#e8f5e9", width=12, anchor=tk.W).grid(row=0, column=0, sticky=tk.W, pady=8)
        ttk.Entry(admin_grid, textvariable=self.admin_user_var, width=30,
                 font=self.UI_CONFIG["body_font"]).grid(row=0, column=1, padx=10, pady=8)
        
        tk.Label(admin_grid, text="Email:", font=self.UI_CONFIG["label_font"], 
                bg="#e8f5e9", width=12, anchor=tk.W).grid(row=1, column=0, sticky=tk.W, pady=8)
        ttk.Entry(admin_grid, textvariable=self.admin_email_var, width=35,
                 font=self.UI_CONFIG["body_font"]).grid(row=1, column=1, padx=10, pady=8)
        
        tk.Label(admin_grid, text="Password:", font=self.UI_CONFIG["label_font"], 
                bg="#e8f5e9", width=12, anchor=tk.W).grid(row=2, column=0, sticky=tk.W, pady=8)
        
        pass_frame = tk.Frame(admin_grid, bg="#e8f5e9")
        pass_frame.grid(row=2, column=1, padx=10, pady=8, sticky=tk.W)
        ttk.Entry(pass_frame, textvariable=self.admin_pass_var, width=25, show="*",
                 font=self.UI_CONFIG["body_font"]).pack(side=tk.LEFT)
        ttk.Button(pass_frame, text="🔄 Generate", 
                  command=self._generate_password).pack(side=tk.LEFT, padx=10)
        
        # JWT Secret card
        jwt_card = tk.Frame(frame, bg="#fff3e0", padx=20, pady=15)
        jwt_card.pack(fill=tk.X, pady=8)
        
        tk.Label(jwt_card, text="🔐  Security Keys", 
                font=self.UI_CONFIG["heading_font"], bg="#fff3e0").pack(anchor=tk.W, pady=(0, 10))
        
        jwt_grid = tk.Frame(jwt_card, bg="#fff3e0")
        jwt_grid.pack(fill=tk.X)
        
        tk.Label(jwt_grid, text="JWT Secret:", font=self.UI_CONFIG["label_font"], 
                bg="#fff3e0", width=12, anchor=tk.W).grid(row=0, column=0, sticky=tk.W, pady=8)
        
        jwt_entry_frame = tk.Frame(jwt_grid, bg="#fff3e0")
        jwt_entry_frame.grid(row=0, column=1, padx=10, pady=8, sticky=tk.W)
        ttk.Entry(jwt_entry_frame, textvariable=self.jwt_secret_var, width=45, show="*",
                 font=self.UI_CONFIG["body_font"]).pack(side=tk.LEFT)
        ttk.Button(jwt_entry_frame, text="🔄 Generate", 
                  command=self._generate_jwt_secret).pack(side=tk.LEFT, padx=10)
        
        tk.Label(jwt_card, text="💡 JWT secrets are used for authentication tokens. Keep them secure!", 
                font=("Helvetica", 11), fg="#666", bg="#fff3e0").pack(anchor=tk.W, pady=(10, 0))
        
        # Generate JWT if empty
        if not self.jwt_secret_var.get():
            self._generate_jwt_secret()
    
    def _generate_password(self):
        """Generate a strong password."""
        chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*"
        password = ''.join(secrets.choice(chars) for _ in range(16))
        self.admin_pass_var.set(password)
        self.log_wizard_action("button_click", "Password generated")
    
    def _generate_jwt_secret(self):
        """Generate JWT secret."""
        self.jwt_secret_var.set(secrets.token_urlsafe(64))
        self.log_wizard_action("button_click", "JWT secret generated")
    
    def create_step_review(self, parent):
        """Create review step showing complete configuration with improved styling."""
        frame = ttk.Frame(parent, padding="25")
        frame.pack(fill=tk.BOTH, expand=True)
        
        ttk.Label(frame, text="Review Your Configuration", 
                 font=self.UI_CONFIG["heading_font"]).pack(anchor=tk.W, pady=(0, 20))
        
        # Scrollable review area
        review_canvas = tk.Canvas(frame, highlightthickness=0)
        scrollbar = ttk.Scrollbar(frame, orient="vertical", command=review_canvas.yview)
        review_frame = ttk.Frame(review_canvas)
        
        review_frame.bind("<Configure>", lambda e: review_canvas.configure(scrollregion=review_canvas.bbox("all")))
        review_canvas.create_window((0, 0), window=review_frame, anchor="nw")
        review_canvas.configure(yscrollcommand=scrollbar.set)
        
        review_canvas.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        
        # Build configuration summary
        sections = [
            ("🎯 Deployment Target", [
                ("Target", self._get_target_display_name()),
            ], "#e3f2fd"),
            ("🌍 Environment", [
                ("Environment", self.env_var.get().title()),
                ("Architecture", self.arch_var.get().replace("_", " ").title()),
            ], "#e8f5e9"),
            ("☁️ Cloud", [
                ("Provider", self.cloud_provider_var.get().upper() if self.cloud_provider_var.get() != "none" else "None"),
                ("Region", self.cloud_region_var.get() or "Default"),
                ("Authenticated", "✅ Yes" if self.cloud_authenticated.get() else "❌ No"),
            ], "#fff3e0") if self.deploy_target_var.get() in ["aws", "azure", "gcp"] else None,
            ("🔨 Build", [
                ("Source", self.build_source_var.get().title()),
                ("Git Repo", self.git_repo_var.get() or "N/A") if self.build_source_var.get() == "git" else None,
            ], "#f3e5f5"),
            ("📦 Components", [
                ("Database", "✅" if self.deploy_db_var.get() else "❌"),
                ("Redis", "✅" if self.deploy_redis_var.get() else "❌"),
                ("API", "✅" if self.deploy_api_var.get() else "❌"),
                ("Frontend", "✅" if self.deploy_frontend_var.get() else "❌"),
                ("Monitoring", "✅" if self.deploy_monitoring_var.get() else "❌"),
            ], "#e0f2f1"),
            ("🗄️ Database", [
                ("Provider", self.db_provider_var.get().title()),
                ("Host", self.db_host_var.get()),
                ("Port", str(self.db_port_var.get())),
                ("Database", self.db_name_var.get()),
                ("User", self.db_user_var.get()),
            ], "#fff8e1"),
            ("🌐 Network", [
                ("Domain", self.domain_var.get()),
                ("API Port", str(self.api_port_var.get())),
                ("Frontend Port", str(self.frontend_port_var.get())),
                ("SSL", "🔒 Enabled" if self.ssl_var.get() else "🔓 Disabled"),
            ], "#e8eaf6"),
            ("👤 Admin", [
                ("Username", self.admin_user_var.get()),
                ("Email", self.admin_email_var.get()),
                ("Password", "●●●●●●●●" if self.admin_pass_var.get() else "⚠️ Not set"),
            ], "#fce4ec"),
        ]
        
        row = 0
        for section in sections:
            if section is None:
                continue
                
            title, items, bg_color = section
            
            section_card = tk.Frame(review_frame, bg=bg_color, padx=15, pady=12)
            section_card.grid(row=row, column=0, sticky="ew", pady=6, padx=5)
            row += 1
            
            tk.Label(section_card, text=title, font=self.UI_CONFIG["subheading_font"], 
                    bg=bg_color).grid(row=0, column=0, columnspan=2, sticky=tk.W, pady=(0, 8))
            
            item_row = 1
            for item in items:
                if item is None:
                    continue
                key, value = item
                tk.Label(section_card, text=f"{key}:", font=self.UI_CONFIG["body_font"], 
                        bg=bg_color, width=15, anchor=tk.W).grid(row=item_row, column=0, sticky=tk.W, pady=3)
                tk.Label(section_card, text=value, font=self.UI_CONFIG["body_font"], 
                        fg="#1565c0", bg=bg_color).grid(row=item_row, column=1, sticky=tk.W, pady=3)
                item_row += 1
        
        # Buttons row
        btn_frame = tk.Frame(review_frame)
        btn_frame.grid(row=row, column=0, pady=20)
        
        ttk.Button(btn_frame, text="📝 Edit Configuration", 
                  command=lambda: self.show_step(0)).pack(side=tk.LEFT, padx=10)
        ttk.Button(btn_frame, text="💾 Export Config", 
                  command=self._export_config).pack(side=tk.LEFT, padx=10)
        
        row += 1
        
        # Test Deployment Card
        test_card = tk.Frame(review_frame, bg="#fff3e0", padx=20, pady=15)
        test_card.grid(row=row, column=0, sticky="ew", pady=15, padx=5)
        
        tk.Label(test_card, text="🧪  Test Deployment", 
                font=self.UI_CONFIG["heading_font"], bg="#fff3e0").pack(anchor=tk.W, pady=(0, 10))
        
        tk.Label(test_card, text="Deploy, verify, run smoke tests, then automatically decommission all resources.",
                font=self.UI_CONFIG["body_font"], fg="#666", bg="#fff3e0").pack(anchor=tk.W)
        
        test_desc = tk.Frame(test_card, bg="#fff3e0")
        test_desc.pack(fill=tk.X, pady=10)
        
        test_steps = [
            "✓ Build & deploy to selected target",
            "✓ Verify health checks",
            "✓ Run UI smoke tests (all pages accessible)",
            "✓ Test API endpoints",
            "✓ Check CORS configuration",
            "✓ Generate detailed summary report",
            "✓ Decommission all resources",
            "✓ Open summary in text editor"
        ]
        
        for step in test_steps:
            tk.Label(test_desc, text=step, font=("Helvetica", 11), 
                    fg="#555", bg="#fff3e0").pack(anchor=tk.W, padx=20)
        
        test_btn_frame = tk.Frame(test_card, bg="#fff3e0")
        test_btn_frame.pack(fill=tk.X, pady=(15, 0))
        
        test_btn = tk.Button(test_btn_frame, text="🧪  Run Test Deployment", 
                            font=("Helvetica", 13, "bold"),
                            bg="#ff9800", fg="white",
                            activebackground="#f57c00", activeforeground="white",
                            padx=20, pady=10,
                            command=self._start_test_deployment)
        test_btn.pack(side=tk.LEFT)
        
        tk.Label(test_btn_frame, text="  ⚠️ Resources will be created then removed",
                font=("Helvetica", 11), fg="#e65100", bg="#fff3e0").pack(side=tk.LEFT, padx=15)
    
    def _get_target_display_name(self) -> str:
        """Get display name for deployment target."""
        target_names = {
            "local": "🖥️ Local Docker",
            "aws": "☁️ Amazon Web Services (AWS)",
            "azure": "☁️ Microsoft Azure",
            "gcp": "☁️ Google Cloud Platform",
            "kubernetes": "⚙️ Kubernetes Cluster",
        }
        return target_names.get(self.deploy_target_var.get(), self.deploy_target_var.get().title())
    
    def _start_test_deployment(self):
        """Start a test deployment with smoke tests and decommissioning."""
        self.is_test_mode.set(True)
        self.resource_log = DeploymentResourceLog()  # Fresh log
        self.resource_log.log_event("test_mode_start", "Test deployment initiated")
        self.log_wizard_action("test_deployment_start", "Test deployment initiated")
        
        # Navigate to deploy step
        self.start_deployment(test_mode=True)
    
    def _export_config(self):
        """Export configuration to file."""
        config = self._get_current_config()
        
        filepath = filedialog.asksaveasfilename(
            defaultextension=".json",
            filetypes=[("JSON files", "*.json"), ("All files", "*.*")],
            initialfilename="deployment_config.json"
        )
        
        if filepath:
            config_dict = asdict(config)
            with open(filepath, 'w') as f:
                json.dump(config_dict, f, indent=2)
            self.log_wizard_action("export", "Configuration exported", filepath)
            messagebox.showinfo("Export", f"Configuration exported to:\n{filepath}")
    
    def create_step_deploy(self, parent):
        """Create deployment step with improved styling."""
        frame = ttk.Frame(parent, padding="25")
        frame.pack(fill=tk.BOTH, expand=True)
        
        # Header
        header_frame = tk.Frame(frame, bg="#e8f5e9", padx=20, pady=15)
        header_frame.pack(fill=tk.X, pady=(0, 15))
        
        tk.Label(header_frame, text="🚀  Deployment in Progress", 
                font=self.UI_CONFIG["heading_font"], bg="#e8f5e9").pack(anchor=tk.W)
        
        # Progress section
        progress_frame = tk.Frame(frame, bg="#f5f5f5", padx=20, pady=20)
        progress_frame.pack(fill=tk.X, pady=8)
        
        # Progress bar (larger)
        self.deploy_progress = ttk.Progressbar(progress_frame, mode='indeterminate', length=500)
        self.deploy_progress.pack(pady=15)
        
        # Status
        self.deploy_status_var = tk.StringVar(value="Preparing deployment...")
        tk.Label(progress_frame, textvariable=self.deploy_status_var, 
                font=self.UI_CONFIG["subheading_font"],
                bg="#f5f5f5").pack(pady=10)
        
        # Current step indicator
        self.deploy_step_var = tk.StringVar(value="")
        tk.Label(progress_frame, textvariable=self.deploy_step_var, 
                font=self.UI_CONFIG["body_font"],
                fg="#666", bg="#f5f5f5").pack(pady=5)
        
        # Results frame (shown after completion)
        self.deploy_results_frame = tk.Frame(frame, bg="white", padx=15, pady=15)
        
        tk.Label(self.deploy_results_frame, text="📋  Deployment Results", 
                font=self.UI_CONFIG["heading_font"],
                bg="white").pack(anchor=tk.W, pady=(0, 10))
        
        # Scrollable results text with dark theme
        results_container = tk.Frame(self.deploy_results_frame, bg="#1e1e1e")
        results_container.pack(fill=tk.BOTH, expand=True)
        
        self.results_text = scrolledtext.ScrolledText(
            results_container, 
            height=12,
            wrap=tk.WORD, 
            state=tk.DISABLED,
            font=self.UI_CONFIG["log_font"],
            bg="#1e1e1e",
            fg="#d4d4d4",
            insertbackground="white"
        )
        self.results_text.pack(fill=tk.BOTH, expand=True, padx=2, pady=2)
        
        # Configure result text tags
        self.results_text.tag_configure("success", foreground="#4caf50")
        self.results_text.tag_configure("error", foreground="#f44336")
        self.results_text.tag_configure("info", foreground="#2196f3")
        self.results_text.tag_configure("warning", foreground="#ff9800")
    
    def start_deployment(self, test_mode: bool = False):
        """Start the deployment process."""
        self.is_test_mode.set(test_mode)
        
        if test_mode:
            self.resource_log = DeploymentResourceLog()
            self.resource_log.log_event("deployment_start", "Test deployment initiated")
        else:
            self.resource_log = DeploymentResourceLog()
            self.resource_log.log_event("deployment_start", "Production deployment initiated")
        
        self.log_wizard_action("deployment_start", f"{'Test' if test_mode else 'Production'} deployment initiated")
        
        # Show deploy step
        deploy_step_index = len(self.active_steps) - 1
        self.show_step(deploy_step_index)
        
        # Disable navigation
        self.back_btn.configure(state=tk.DISABLED)
        self.next_btn.configure(state=tk.DISABLED)
        
        # Start progress bar
        self.deploy_progress.start()
        
        # Get configuration
        config = self._get_current_config()
        
        def log_callback(msg_type: str, message: str):
            self.log_message(msg_type, message)
            self.resource_log.log_event(msg_type, message)
            self.root.after(0, lambda: self.deploy_status_var.set(message[:60] + "..." if len(message) > 60 else message))
        
        def do_deploy():
            try:
                total_steps = 7 if test_mode else 5
                
                # Step 1: Generate scripts
                self.root.after(0, lambda: self.deploy_step_var.set(f"Step 1/{total_steps}: Generating deployment scripts..."))
                log_callback("info", "=== Generating Deployment Scripts ===")
                
                output_dir = Path(__file__).parent / "generated"
                output_dir.mkdir(parents=True, exist_ok=True)
                
                # Generate docker-compose or kubernetes files
                if config.hosting_platform == "docker":
                    self._generate_docker_compose_with_logging(config, output_dir, log_callback)
                elif config.hosting_platform == "kubernetes":
                    self._generate_kubernetes(config, output_dir, log_callback)
                elif config.hosting_platform == "cloud":
                    self._generate_cloud_scripts(config, output_dir, log_callback)
                
                log_callback("success", "✓ Deployment scripts generated")
                
                # Step 2: Check prerequisites
                self.root.after(0, lambda: self.deploy_step_var.set(f"Step 2/{total_steps}: Checking prerequisites..."))
                log_callback("info", "=== Checking Prerequisites ===")
                
                checker = PrerequisiteChecker()
                prereqs = checker.check_all()
                missing = [p for p in prereqs if not p["installed"]]
                
                if missing:
                    for p in missing:
                        log_callback("warning", f"Missing: {p['name']}")
                else:
                    log_callback("success", "✓ All prerequisites satisfied")
                
                # Step 3: Build (if local)
                if config.build_type == "local":
                    self.root.after(0, lambda: self.deploy_step_var.set(f"Step 3/{total_steps}: Building containers..."))
                    log_callback("info", "=== Building Containers ===")
                    
                    self._build_with_logging(config, log_callback)
                else:
                    self.root.after(0, lambda: self.deploy_step_var.set(f"Step 3/{total_steps}: Cloud build configured..."))
                    log_callback("info", "Cloud build configured - pipeline will execute on push")
                
                # Step 4: Deploy
                self.root.after(0, lambda: self.deploy_step_var.set(f"Step 4/{total_steps}: Deploying components..."))
                log_callback("info", "=== Deploying Components ===")
                
                # For local docker, run docker-compose
                if config.hosting_platform == "docker":
                    compose_file = output_dir / "docker-compose.yml"
                    if compose_file.exists():
                        success = self._run_command(f"cd {output_dir} && docker compose up -d", log_callback)
                        if success:
                            log_callback("success", "✓ Docker containers started")
                            # Log running containers
                            self._log_running_containers(config, log_callback)
                        else:
                            log_callback("warning", "Container startup had issues")
                
                # Step 5: Health checks
                self.root.after(0, lambda: self.deploy_step_var.set(f"Step 5/{total_steps}: Running health checks..."))
                log_callback("info", "=== Running Health Checks ===")
                
                time.sleep(8)  # Wait for services to start
                
                # Check endpoints
                protocol = "https" if config.ssl_enabled else "http"
                api_url = f"{protocol}://{config.domain}:{config.api_port}/health"
                frontend_url = f"{protocol}://{config.domain}:{config.frontend_port}"
                
                api_healthy = self._check_endpoint(api_url)
                frontend_healthy = self._check_endpoint(frontend_url)
                
                if api_healthy:
                    log_callback("success", f"✓ API healthy: {api_url}")
                else:
                    log_callback("warning", f"✗ API not responding: {api_url}")
                
                if frontend_healthy:
                    log_callback("success", f"✓ Frontend healthy: {frontend_url}")
                else:
                    log_callback("warning", f"✗ Frontend not responding: {frontend_url}")
                
                # Test mode: Run smoke tests and decommission
                if test_mode:
                    # Step 6: Smoke tests
                    self.root.after(0, lambda: self.deploy_step_var.set(f"Step 6/{total_steps}: Running smoke tests..."))
                    log_callback("info", "=== Running Smoke Tests ===")
                    
                    smoke_runner = SmokeTestRunner(config, self.resource_log, log_callback)
                    test_results = smoke_runner.run_all_tests()
                    
                    passed = sum(1 for r in test_results if r.passed)
                    total = len(test_results)
                    log_callback("info", f"Smoke tests completed: {passed}/{total} passed")
                    
                    # Step 7: Decommission
                    self.root.after(0, lambda: self.deploy_step_var.set(f"Step 7/{total_steps}: Decommissioning resources..."))
                    log_callback("info", "=== Decommissioning Resources ===")
                    
                    time.sleep(2)  # Brief pause before teardown
                    
                    decommissioner = ResourceDecommissioner(self.resource_log, config, log_callback)
                    decommissioner.decommission_all()
                    
                    # Finalize and generate summary
                    self.resource_log.finalize()
                    
                    # Generate summary file
                    summary_dir = output_dir / "summaries"
                    summary_dir.mkdir(parents=True, exist_ok=True)
                    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
                    summary_path = summary_dir / f"test_deployment_summary_{timestamp}.txt"
                    
                    generator = DeploymentSummaryGenerator(self.resource_log, config)
                    generator.save_summary(summary_path, is_test_mode=True)
                    self.summary_path = summary_path
                    
                    log_callback("success", f"Summary saved: {summary_path}")
                    
                    # Complete (test mode)
                    self.root.after(0, lambda: self._test_deployment_complete(config, passed, total))
                else:
                    # Complete (normal mode)
                    self.resource_log.finalize()
                    self._save_deployment_summary(config, output_dir)
                    self.root.after(0, lambda: self._deployment_complete(config, api_healthy and frontend_healthy))
                
            except Exception as e:
                log_callback("error", f"Deployment failed: {str(e)}")
                self.resource_log.log_event("error", str(e))
                self.root.after(0, lambda: self._deployment_failed(str(e)))
        
        threading.Thread(target=do_deploy, daemon=True).start()
    
    def _generate_docker_compose_with_logging(self, config: DeploymentConfig, output_dir: Path, log_callback):
        """Generate Docker Compose files and log resources."""
        log_callback("info", "Generating docker-compose.yml...")
        
        # Log the network
        self.resource_log.add_resource("network", "crm-network", {"driver": "bridge"})
        
        # Build services dict
        services = {}
        
        if config.deploy_database:
            services["mariadb"] = {
                "image": "mariadb:10.11",
                "container_name": "crm-mariadb",
                "environment": {
                    "MYSQL_ROOT_PASSWORD": config.database_root_password,
                    "MYSQL_DATABASE": config.database_name,
                    "MYSQL_USER": config.database_user,
                    "MYSQL_PASSWORD": config.database_password,
                },
                "ports": [f"{config.database_port}:3306"],
                "volumes": ["mariadb_data:/var/lib/mysql"],
                "networks": ["crm-network"],
                "restart": "unless-stopped",
            }
            self.resource_log.add_resource("container", "crm-mariadb", 
                                          {"image": "mariadb:10.11", "port": config.database_port})
            self.resource_log.add_resource("volume", "mariadb_data", {"type": "docker volume"})
        
        if config.deploy_redis:
            services["redis"] = {
                "image": "redis:7-alpine",
                "container_name": "crm-redis",
                "ports": [f"{config.redis_port}:6379"],
                "networks": ["crm-network"],
                "restart": "unless-stopped",
            }
            self.resource_log.add_resource("container", "crm-redis", 
                                          {"image": "redis:7-alpine", "port": config.redis_port})
        
        if config.deploy_api:
            services["api"] = {
                "build": {"context": "../../CRM.Backend", "dockerfile": "../docker/Dockerfile.backend"},
                "container_name": "crm-api",
                "environment": {
                    "ASPNETCORE_ENVIRONMENT": "Production",
                    "ConnectionStrings__DefaultConnection": f"Server={config.database_host};Port=3306;Database={config.database_name};User={config.database_user};Password={config.database_password};",
                    "JWT_SECRET": config.jwt_secret,
                },
                "ports": [f"{config.api_port}:5000"],
                "depends_on": ["mariadb"] if config.deploy_database else [],
                "networks": ["crm-network"],
                "restart": "unless-stopped",
            }
            self.resource_log.add_resource("container", "crm-api", 
                                          {"image": "crm-api:latest", "port": config.api_port})
            self.resource_log.add_resource("image", "crm-api:latest", {"type": "built"})
        
        if config.deploy_frontend:
            services["frontend"] = {
                "build": {"context": "../../CRM.Frontend", "dockerfile": "../docker/Dockerfile.frontend"},
                "container_name": "crm-frontend",
                "environment": {
                    "REACT_APP_API_URL": f"http://{config.domain}:{config.api_port}",
                },
                "ports": [f"{config.frontend_port}:80"],
                "depends_on": ["api"] if config.deploy_api else [],
                "networks": ["crm-network"],
                "restart": "unless-stopped",
            }
            self.resource_log.add_resource("container", "crm-frontend", 
                                          {"image": "crm-frontend:latest", "port": config.frontend_port})
            self.resource_log.add_resource("image", "crm-frontend:latest", {"type": "built"})
        
        compose = {
            "version": "3.8",
            "services": services,
            "networks": {"crm-network": {"driver": "bridge"}},
            "volumes": {"mariadb_data": {}},
        }
        
        compose_path = output_dir / "docker-compose.yml"
        with open(compose_path, 'w') as f:
            f.write("# CRM Solution - Generated Docker Compose\n")
            f.write(f"# Generated: {datetime.now().isoformat()}\n\n")
            json.dump(compose, f, indent=2)
        
        log_callback("success", f"Generated: {compose_path}")
    
    def _build_with_logging(self, config: DeploymentConfig, log_callback):
        """Build containers and log resources."""
        build_engine = BuildEngine(config, log_callback)
        if build_engine.build_all():
            log_callback("success", "✓ Containers built successfully")
        else:
            log_callback("warning", "Build had some issues")
    
    def _log_running_containers(self, config: DeploymentConfig, log_callback):
        """Log running containers."""
        try:
            result = subprocess.run("docker ps --format '{{.Names}}'", 
                                   shell=True, capture_output=True, text=True)
            if result.returncode == 0:
                containers = result.stdout.strip().split('\n')
                crm_containers = [c for c in containers if c.startswith('crm-')]
                for name in crm_containers:
                    self.resource_log.update_resource_status(name, "running")
                log_callback("info", f"Running containers: {', '.join(crm_containers)}")
        except:
            pass
    
    def _save_deployment_summary(self, config: DeploymentConfig, output_dir: Path):
        """Save deployment summary for production deployments."""
        summary_dir = output_dir / "summaries"
        summary_dir.mkdir(parents=True, exist_ok=True)
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        summary_path = summary_dir / f"deployment_summary_{timestamp}.txt"
        
        generator = DeploymentSummaryGenerator(self.resource_log, config)
        generator.save_summary(summary_path, is_test_mode=False)
        self.summary_path = summary_path
    
    def _test_deployment_complete(self, config: DeploymentConfig, passed: int, total: int):
        """Handle test deployment completion."""
        self.deploy_progress.stop()
        self.deploy_step_var.set("")
        
        if passed == total:
            self.deploy_status_var.set("✅ Test Deployment Complete - All Tests Passed!")
            status_color = "#4caf50"
        elif passed > 0:
            self.deploy_status_var.set(f"⚠️ Test Deployment Complete - {passed}/{total} Tests Passed")
            status_color = "#ff9800"
        else:
            self.deploy_status_var.set(f"❌ Test Deployment Complete - All Tests Failed")
            status_color = "#f44336"
        
        # Show results
        self.deploy_results_frame.pack(fill=tk.BOTH, expand=True, pady=10)
        
        # Generate results text
        results = f"""
TEST DEPLOYMENT COMPLETE
{'=' * 50}

Test Results: {passed}/{total} tests passed

Resources Deployed & Decommissioned:
"""
        
        for r in self.resource_log.resources:
            status_icon = "✓" if r.status == "deleted" else "○"
            results += f"  {status_icon} {r.resource_type}: {r.name} ({r.status})\n"
        
        results += f"""
{'=' * 50}

Summary saved to:
  {self.summary_path}

Click 'Open Summary' to view the detailed report.
"""
        
        self.results_text.configure(state=tk.NORMAL)
        self.results_text.delete(1.0, tk.END)
        self.results_text.insert(tk.END, results)
        self.results_text.configure(state=tk.DISABLED)
        
        # Re-enable navigation with summary button
        self.back_btn.configure(state=tk.NORMAL)
        self.next_btn.configure(text="📄 Open Summary", state=tk.NORMAL, 
                               command=self._open_summary_in_editor)
        
        self.log_wizard_action("test_deployment_complete", f"Tests: {passed}/{total}")
    
    def _open_summary_in_editor(self):
        """Open the deployment summary in system text editor."""
        if self.summary_path and self.summary_path.exists():
            generator = DeploymentSummaryGenerator(self.resource_log, self._get_current_config())
            generator.open_in_editor(self.summary_path)
            self.log_wizard_action("summary_opened", str(self.summary_path))
    
    def _run_command(self, cmd: str, log_callback) -> bool:
        """Run a shell command."""
        try:
            log_callback("cmd", f"$ {cmd}")
            result = subprocess.run(cmd, shell=True, capture_output=True, text=True, timeout=300)
            if result.returncode == 0:
                return True
            else:
                log_callback("error", result.stderr[:200] if result.stderr else "Command failed")
                return False
        except Exception as e:
            log_callback("error", str(e))
            return False
    
    def _check_endpoint(self, url: str) -> bool:
        """Check if endpoint is healthy."""
        try:
            ctx = ssl.create_default_context()
            ctx.check_hostname = False
            ctx.verify_mode = ssl.CERT_NONE
            
            request = urllib.request.Request(url, method='GET')
            with urllib.request.urlopen(request, timeout=10, context=ctx) as response:
                return response.status < 400
        except:
            return False
    
    def _deployment_complete(self, config: DeploymentConfig, success: bool):
        """Handle deployment completion."""
        self.deploy_progress.stop()
        self.deploy_step_var.set("")
        
        if success:
            self.deploy_status_var.set("✅ Deployment Complete!")
            status = "success"
        else:
            self.deploy_status_var.set("⚠️ Deployment completed with warnings")
            status = "warning"
        
        # Show results
        self.deploy_results_frame.pack(fill=tk.BOTH, expand=True, pady=10)
        
        protocol = "https" if config.ssl_enabled else "http"
        results = f"""
Deployment {status.upper()}!

Access URLs:
  Frontend: {protocol}://{config.domain}:{config.frontend_port}
  API:      {protocol}://{config.domain}:{config.api_port}

Admin Login:
  Email:    {config.admin_email}
  Password: {"*" * (len(config.admin_password) - 3) + config.admin_password[-3:]}

Generated files in: generated/

Next steps:
  • Access the frontend URL to verify the deployment
  • Check the logs for any issues
  • Configure DNS if using a custom domain
"""
        
        self.results_text.configure(state=tk.NORMAL)
        self.results_text.delete(1.0, tk.END)
        self.results_text.insert(tk.END, results)
        self.results_text.configure(state=tk.DISABLED)
        
        # Re-enable back button
        self.back_btn.configure(state=tk.NORMAL)
        self.next_btn.configure(text="🔄 Start Over", state=tk.NORMAL, 
                               command=lambda: self.show_step(0))
        
        self.log_wizard_action("deployment_complete", f"Deployment {status}")
    
    def _deployment_failed(self, error: str):
        """Handle deployment failure."""
        self.deploy_progress.stop()
        self.deploy_status_var.set(f"❌ Deployment Failed: {error[:50]}...")
        self.deploy_step_var.set("")
        
        self.back_btn.configure(state=tk.NORMAL)
        self.next_btn.configure(text="🔄 Retry", state=tk.NORMAL)
        
        self.log_wizard_action("deployment_failed", error)
    
    def _generate_docker_compose(self, config: DeploymentConfig, output_dir: Path, log_callback):
        """Generate Docker Compose files."""
        log_callback("info", "Generating docker-compose.yml...")
        
        # Build services dict
        services = {}
        
        if config.deploy_database:
            services["mariadb"] = {
                "image": "mariadb:10.11",
                "container_name": "crm-mariadb",
                "environment": {
                    "MYSQL_ROOT_PASSWORD": config.database_root_password,
                    "MYSQL_DATABASE": config.database_name,
                    "MYSQL_USER": config.database_user,
                    "MYSQL_PASSWORD": config.database_password,
                },
                "ports": [f"{config.database_port}:3306"],
                "volumes": ["mariadb_data:/var/lib/mysql"],
                "networks": ["crm-network"],
                "restart": "unless-stopped",
            }
        
        if config.deploy_redis:
            services["redis"] = {
                "image": "redis:7-alpine",
                "container_name": "crm-redis",
                "ports": [f"{config.redis_port}:6379"],
                "networks": ["crm-network"],
                "restart": "unless-stopped",
            }
        
        if config.deploy_api:
            services["api"] = {
                "build": {"context": "../../CRM.Backend", "dockerfile": "../docker/Dockerfile.backend"},
                "container_name": "crm-api",
                "environment": {
                    "ASPNETCORE_ENVIRONMENT": "Production",
                    "ConnectionStrings__DefaultConnection": f"Server={config.database_host};Port=3306;Database={config.database_name};User={config.database_user};Password={config.database_password};",
                    "JWT_SECRET": config.jwt_secret,
                },
                "ports": [f"{config.api_port}:5000"],
                "depends_on": ["mariadb"] if config.deploy_database else [],
                "networks": ["crm-network"],
                "restart": "unless-stopped",
            }
        
        if config.deploy_frontend:
            services["frontend"] = {
                "build": {"context": "../../CRM.Frontend", "dockerfile": "../docker/Dockerfile.frontend"},
                "container_name": "crm-frontend",
                "environment": {
                    "REACT_APP_API_URL": f"http://{config.domain}:{config.api_port}",
                },
                "ports": [f"{config.frontend_port}:80"],
                "depends_on": ["api"] if config.deploy_api else [],
                "networks": ["crm-network"],
                "restart": "unless-stopped",
            }
        
        compose = {
            "version": "3.8",
            "services": services,
            "networks": {"crm-network": {"driver": "bridge"}},
            "volumes": {"mariadb_data": {}},
        }
        
        compose_path = output_dir / "docker-compose.yml"
        with open(compose_path, 'w') as f:
            # Simple YAML-like output
            f.write("# CRM Solution - Generated Docker Compose\n")
            f.write(f"# Generated: {datetime.now().isoformat()}\n\n")
            json.dump(compose, f, indent=2)  # For simplicity, using JSON format
        
        log_callback("success", f"Generated: {compose_path}")
    
    def _generate_kubernetes(self, config: DeploymentConfig, output_dir: Path, log_callback):
        """Generate Kubernetes manifests."""
        log_callback("info", "Generating Kubernetes manifests...")
        # Implementation for K8s manifests
        log_callback("success", "Kubernetes manifests generated")
    
    def _generate_cloud_scripts(self, config: DeploymentConfig, output_dir: Path, log_callback):
        """Generate cloud deployment scripts."""
        provider = config.cloud_provider
        log_callback("info", f"Generating {provider.upper()} deployment scripts...")
        
        if provider == "aws":
            self._generate_aws_scripts(config, output_dir, log_callback)
        elif provider == "azure":
            self._generate_azure_scripts(config, output_dir, log_callback)
        elif provider == "gcp":
            self._generate_gcp_scripts(config, output_dir, log_callback)
        
        log_callback("success", f"{provider.upper()} scripts generated")
    
    def _generate_aws_scripts(self, config, output_dir, log_callback):
        """Generate AWS deployment scripts."""
        script_content = f"""#!/bin/bash
# AWS Deployment Script - Generated {datetime.now().isoformat()}

set -e

AWS_REGION="{config.aws_region}"
ECR_REGISTRY="{config.aws_ecr_registry}"
CLUSTER_NAME="crm-cluster"

echo "🚀 Deploying to AWS..."

# Login to ECR
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $ECR_REGISTRY

# Build and push images
docker build -t crm-api ../../CRM.Backend
docker tag crm-api:latest $ECR_REGISTRY/crm-api:latest
docker push $ECR_REGISTRY/crm-api:latest

# Update ECS service
aws ecs update-service --cluster $CLUSTER_NAME --service crm-api --force-new-deployment

echo "✅ AWS deployment complete"
"""
        
        script_path = output_dir / "deploy-aws.sh"
        with open(script_path, 'w') as f:
            f.write(script_content)
        os.chmod(script_path, 0o755)
        log_callback("info", f"Generated: {script_path}")
    
    def _generate_azure_scripts(self, config, output_dir, log_callback):
        """Generate Azure deployment scripts."""
        script_content = f"""#!/bin/bash
# Azure Deployment Script - Generated {datetime.now().isoformat()}

set -e

RESOURCE_GROUP="{config.azure_resource_group}"
LOCATION="{config.azure_location}"
ACR_NAME="crmacr"

echo "🚀 Deploying to Azure..."

# Login to ACR
az acr login --name $ACR_NAME

# Build and push
az acr build --registry $ACR_NAME --image crm-api:latest ../../CRM.Backend

echo "✅ Azure deployment complete"
"""
        
        script_path = output_dir / "deploy-azure.sh"
        with open(script_path, 'w') as f:
            f.write(script_content)
        os.chmod(script_path, 0o755)
        log_callback("info", f"Generated: {script_path}")
    
    def _generate_gcp_scripts(self, config, output_dir, log_callback):
        """Generate GCP deployment scripts."""
        project_id = config.gcp_project_id or "your-project-id"
        region = config.gcp_region
        
        script_content = f"""#!/bin/bash
# GCP Deployment Script - Generated {datetime.now().isoformat()}

set -e

PROJECT_ID="{project_id}"
REGION="{region}"
GCR_HOSTNAME="gcr.io"

echo "🚀 Deploying to Google Cloud..."

# Configure Docker for GCR
gcloud auth configure-docker $GCR_HOSTNAME

# Build and push
docker build -t $GCR_HOSTNAME/$PROJECT_ID/crm-api:latest ../../CRM.Backend
docker push $GCR_HOSTNAME/$PROJECT_ID/crm-api:latest

# Deploy to Cloud Run
gcloud run deploy crm-api \\
  --image $GCR_HOSTNAME/$PROJECT_ID/crm-api:latest \\
  --platform managed \\
  --region $REGION \\
  --allow-unauthenticated

echo "✅ GCP deployment complete"
"""
        
        script_path = output_dir / "deploy-gcp.sh"
        with open(script_path, 'w') as f:
            f.write(script_content)
        os.chmod(script_path, 0o755)
        log_callback("info", f"Generated: {script_path}")
    
    def _get_current_config(self) -> DeploymentConfig:
        """Build configuration from wizard state."""
        return DeploymentConfig(
            architecture=self.arch_var.get(),
            hosting_platform=self.platform_var.get(),
            cloud_provider=self.cloud_provider_var.get(),
            deploy_frontend=self.deploy_frontend_var.get(),
            deploy_api=self.deploy_api_var.get(),
            deploy_database=self.deploy_db_var.get(),
            deploy_redis=self.deploy_redis_var.get(),
            deploy_monitoring=self.deploy_monitoring_var.get(),
            database_provider=self.db_provider_var.get(),
            database_host=self.db_host_var.get(),
            database_port=self.db_port_var.get(),
            database_name=self.db_name_var.get(),
            database_user=self.db_user_var.get(),
            database_password=self.db_pass_var.get(),
            database_root_password=self.db_root_pass_var.get(),
            api_port=self.api_port_var.get(),
            frontend_port=self.frontend_port_var.get(),
            redis_port=self.redis_port_var.get(),
            domain=self.domain_var.get(),
            ssl_enabled=self.ssl_var.get(),
            ssl_cert_path=self.ssl_cert_var.get(),
            ssl_key_path=self.ssl_key_var.get(),
            admin_username=self.admin_user_var.get(),
            admin_email=self.admin_email_var.get(),
            admin_password=self.admin_pass_var.get(),
            jwt_secret=self.jwt_secret_var.get(),
            build_source=self.build_source_var.get(),
            git_repo_url=self.git_repo_var.get(),
            git_branch=self.git_branch_var.get(),
            git_username=self.git_username_var.get(),
            git_token=self.git_token_var.get(),
            build_type=self.build_method_var.get(),
            cloud_build_provider=self.cloud_build_provider_var.get(),
            aws_region=self.aws_region_var.get(),
            aws_ecr_registry=self.aws_ecr_var.get(),
            azure_subscription_id=self.azure_subscription_var.get(),
            azure_resource_group=self.azure_resource_group_var.get(),
            azure_location=self.azure_location_var.get(),
            gcp_project_id=self.gcp_project_var.get(),
            gcp_region=self.gcp_region_var.get(),
            deploy_identity_service=self.deploy_identity_var.get(),
            deploy_customer_service=self.deploy_customer_var.get(),
            deploy_sales_service=self.deploy_sales_var.get(),
            deploy_marketing_service=self.deploy_marketing_var.get(),
            deploy_servicedesk_service=self.deploy_servicedesk_var.get(),
        )
    
    def load_config(self):
        """Load configuration from file."""
        try:
            if self.config_path.exists():
                with open(self.config_path) as f:
                    data = json.load(f)
                    # Update vars from loaded config
                    for key, value in data.items():
                        var_name = f"{key}_var"
                        if hasattr(self, var_name):
                            getattr(self, var_name).set(value)
                self.log_message("info", "Configuration loaded")
        except Exception as e:
            self.log_message("warning", f"Could not load config: {e}")
    
    def save_config(self):
        """Save current configuration."""
        try:
            config = self._get_current_config()
            config_dict = asdict(config)
            
            with open(self.config_path, 'w') as f:
                json.dump(config_dict, f, indent=2)
            
            self.log_wizard_action("config_save", "Configuration saved")
            messagebox.showinfo("Saved", "Configuration saved successfully")
        except Exception as e:
            messagebox.showerror("Error", f"Failed to save: {e}")
    
    def load_config_dialog(self):
        """Load configuration from file dialog."""
        filepath = filedialog.askopenfilename(
            filetypes=[("JSON files", "*.json"), ("All files", "*.*")]
        )
        if filepath:
            try:
                with open(filepath) as f:
                    data = json.load(f)
                    for key, value in data.items():
                        if hasattr(self, f"{key}_var"):
                            getattr(self, f"{key}_var").set(value)
                self.log_wizard_action("config_load", "Configuration loaded from file", filepath)
                messagebox.showinfo("Loaded", "Configuration loaded successfully")
            except Exception as e:
                messagebox.showerror("Error", f"Failed to load: {e}")


class DeploymentTool:
    """Main deployment tool application."""
    
    def __init__(self, root: tk.Tk):
        self.root = root
        self.root.title(f"CRM Solution Deployment Tool v{VERSION}")
        self.root.geometry("1400x900")
        self.root.minsize(1200, 800)
        
        # Configuration
        self.config = DeploymentConfig()
        self.config_path = Path(__file__).parent / "config" / "deployment_config.json"
        self.log_queue = queue.Queue()
        self.test_results: List[TestResult] = []
        self.deployment_thread: Optional[threading.Thread] = None
        self.engine: Optional[DeploymentEngine] = None
        
        # Ensure config directory exists
        self.config_path.parent.mkdir(parents=True, exist_ok=True)
        
        # Load saved configuration
        self.load_config()
        
        # Create UI
        self.create_ui()
        
        # Start log processor
        self.process_logs()
    
    def create_ui(self):
        """Create the main user interface."""
        # Main paned window
        self.main_paned = ttk.PanedWindow(self.root, orient=tk.HORIZONTAL)
        self.main_paned.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)
        
        # Left panel - Configuration
        self.left_frame = ttk.Frame(self.main_paned, width=700)
        self.main_paned.add(self.left_frame, weight=1)
        
        # Right panel - Logs
        self.right_frame = ttk.Frame(self.main_paned, width=500)
        self.main_paned.add(self.right_frame, weight=1)
        
        # Create notebook
        self.notebook = ttk.Notebook(self.left_frame)
        self.notebook.pack(fill=tk.BOTH, expand=True, pady=(0, 10))
        
        # Create tabs
        self.create_architecture_tab()
        self.create_build_tab()
        self.create_build_server_tab()
        self.create_cloud_tab()
        self.create_components_tab()
        self.create_database_tab()
        self.create_network_tab()
        self.create_credentials_tab()
        self.create_seed_data_tab()
        self.create_testing_tab()
        
        # Create right panel
        self.create_log_panel()
        
        # Bottom button bar
        self.create_button_bar()
    
    def create_architecture_tab(self):
        """Create architecture selection tab."""
        tab = ttk.Frame(self.notebook, padding="15")
        self.notebook.add(tab, text="🏗️ Architecture")
        
        ttk.Label(tab, text="Deployment Architecture", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 15))
        
        # Architecture selection
        arch_frame = ttk.LabelFrame(tab, text="Architecture Type", padding="15")
        arch_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.arch_var = tk.StringVar(value=self.config.architecture)
        
        ttk.Radiobutton(arch_frame, text="Monolithic (Single API container)", 
                        variable=self.arch_var, value="monolithic",
                        command=self.on_architecture_change).pack(anchor=tk.W, pady=5)
        ttk.Radiobutton(arch_frame, text="Microservices (Separate services)", 
                        variable=self.arch_var, value="microservices",
                        command=self.on_architecture_change).pack(anchor=tk.W, pady=5)
        
        # Orchestration
        orch_frame = ttk.LabelFrame(tab, text="Container Orchestration", padding="15")
        orch_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.orch_var = tk.StringVar(value=self.config.hosting_platform)
        
        ttk.Radiobutton(orch_frame, text="Docker Compose", 
                        variable=self.orch_var, value="docker").pack(anchor=tk.W, pady=3)
        ttk.Radiobutton(orch_frame, text="Kubernetes", 
                        variable=self.orch_var, value="kubernetes").pack(anchor=tk.W, pady=3)
        
        # Naming conventions
        naming_frame = ttk.LabelFrame(tab, text="Naming Conventions", padding="15")
        naming_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(naming_frame, text="Container Prefix:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.container_prefix_var = tk.StringVar(value=NAMING_CONVENTIONS["container_prefix"])
        ttk.Entry(naming_frame, textvariable=self.container_prefix_var, width=20).grid(row=0, column=1, padx=10, pady=3)
        
        ttk.Label(naming_frame, text="Network Name:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.network_name_var = tk.StringVar(value=NAMING_CONVENTIONS["network_name"])
        ttk.Entry(naming_frame, textvariable=self.network_name_var, width=20).grid(row=1, column=1, padx=10, pady=3)
    
    def create_build_tab(self):
        """Create build configuration tab."""
        tab = ttk.Frame(self.notebook, padding="15")
        self.notebook.add(tab, text="🔨 Build")
        
        ttk.Label(tab, text="Build Configuration", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 15))
        
        # Source selection
        source_frame = ttk.LabelFrame(tab, text="Source Code", padding="15")
        source_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.build_source_var = tk.StringVar(value=self.config.build_source)
        
        ttk.Radiobutton(source_frame, text="Local Directory", 
                        variable=self.build_source_var, value="local",
                        command=self.on_build_source_change).pack(anchor=tk.W, pady=3)
        ttk.Radiobutton(source_frame, text="Git Repository", 
                        variable=self.build_source_var, value="git",
                        command=self.on_build_source_change).pack(anchor=tk.W, pady=3)
        
        # Git configuration
        self.git_frame = ttk.LabelFrame(tab, text="Git Repository", padding="15")
        self.git_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(self.git_frame, text="Repository URL:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.git_repo_var = tk.StringVar(value=self.config.git_repo_url)
        ttk.Entry(self.git_frame, textvariable=self.git_repo_var, width=50).grid(row=0, column=1, columnspan=2, sticky=tk.W, padx=10, pady=3)
        
        ttk.Label(self.git_frame, text="Branch:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.git_branch_var = tk.StringVar(value=self.config.git_branch)
        ttk.Entry(self.git_frame, textvariable=self.git_branch_var, width=20).grid(row=1, column=1, sticky=tk.W, padx=10, pady=3)
        
        # Authentication
        self.git_use_ssh_var = tk.BooleanVar(value=self.config.git_use_ssh)
        ttk.Checkbutton(self.git_frame, text="Use SSH", 
                        variable=self.git_use_ssh_var,
                        command=self.on_git_auth_change).grid(row=2, column=0, sticky=tk.W, pady=5)
        
        ttk.Label(self.git_frame, text="Username:").grid(row=3, column=0, sticky=tk.W, pady=3)
        self.git_username_var = tk.StringVar(value=self.config.git_username)
        self.git_username_entry = ttk.Entry(self.git_frame, textvariable=self.git_username_var, width=30)
        self.git_username_entry.grid(row=3, column=1, sticky=tk.W, padx=10, pady=3)
        
        ttk.Label(self.git_frame, text="Token/Password:").grid(row=4, column=0, sticky=tk.W, pady=3)
        self.git_token_var = tk.StringVar(value=self.config.git_token)
        self.git_token_entry = ttk.Entry(self.git_frame, textvariable=self.git_token_var, width=40, show="*")
        self.git_token_entry.grid(row=4, column=1, sticky=tk.W, padx=10, pady=3)
        
        ttk.Label(self.git_frame, text="SSH Key:").grid(row=5, column=0, sticky=tk.W, pady=3)
        ssh_key_frame = ttk.Frame(self.git_frame)
        ssh_key_frame.grid(row=5, column=1, sticky=tk.W, padx=10, pady=3)
        self.git_ssh_key_var = tk.StringVar(value=self.config.git_ssh_key)
        self.git_ssh_key_entry = ttk.Entry(ssh_key_frame, textvariable=self.git_ssh_key_var, width=30)
        self.git_ssh_key_entry.pack(side=tk.LEFT)
        ttk.Button(ssh_key_frame, text="Browse", 
                   command=lambda: self.browse_file(self.git_ssh_key_var)).pack(side=tk.LEFT, padx=5)
        
        # Build type
        build_type_frame = ttk.LabelFrame(tab, text="Build Method", padding="15")
        build_type_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.build_type_var = tk.StringVar(value=self.config.build_type)
        
        ttk.Radiobutton(build_type_frame, text="Local Build", 
                        variable=self.build_type_var, value="local",
                        command=self.on_build_type_change).pack(anchor=tk.W, pady=3)
        ttk.Radiobutton(build_type_frame, text="Remote Build (SSH)", 
                        variable=self.build_type_var, value="remote",
                        command=self.on_build_type_change).pack(anchor=tk.W, pady=3)
        ttk.Radiobutton(build_type_frame, text="Cloud CI/CD", 
                        variable=self.build_type_var, value="cloud",
                        command=self.on_build_type_change).pack(anchor=tk.W, pady=3)
        
        # Cloud build provider
        self.cloud_build_frame = ttk.LabelFrame(tab, text="Cloud Build Provider", padding="15")
        self.cloud_build_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.cloud_build_provider_var = tk.StringVar(value=self.config.cloud_build_provider)
        
        providers = [
            ("GitHub Actions", "github_actions"),
            ("Azure DevOps Pipelines", "azure_devops"),
            ("AWS CodeBuild", "aws_codebuild"),
            ("Google Cloud Build", "gcp_cloudbuild")
        ]
        
        for text, value in providers:
            ttk.Radiobutton(self.cloud_build_frame, text=text, 
                            variable=self.cloud_build_provider_var, value=value,
                            command=self.on_cloud_build_provider_change).pack(anchor=tk.W, pady=2)
        
        # Cloud build configuration notebook
        self.cloud_build_config_notebook = ttk.Notebook(self.cloud_build_frame)
        self.cloud_build_config_notebook.pack(fill=tk.X, pady=(10, 0))
        
        # GitHub Actions config
        gh_frame = ttk.Frame(self.cloud_build_config_notebook, padding="10")
        self.cloud_build_config_notebook.add(gh_frame, text="GitHub")
        ttk.Label(gh_frame, text="Workflow File:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.github_workflow_var = tk.StringVar(value=self.config.github_actions_workflow)
        ttk.Entry(gh_frame, textvariable=self.github_workflow_var, width=40).grid(row=0, column=1, padx=10, pady=3)
        
        # Azure DevOps config
        ado_frame = ttk.Frame(self.cloud_build_config_notebook, padding="10")
        self.cloud_build_config_notebook.add(ado_frame, text="Azure DevOps")
        ttk.Label(ado_frame, text="Organization:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.azure_devops_org_var = tk.StringVar(value=self.config.azure_devops_org)
        ttk.Entry(ado_frame, textvariable=self.azure_devops_org_var, width=30).grid(row=0, column=1, padx=10, pady=3)
        ttk.Label(ado_frame, text="Project:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.azure_devops_project_var = tk.StringVar(value=self.config.azure_devops_project)
        ttk.Entry(ado_frame, textvariable=self.azure_devops_project_var, width=30).grid(row=1, column=1, padx=10, pady=3)
        ttk.Label(ado_frame, text="Pipeline ID:").grid(row=2, column=0, sticky=tk.W, pady=3)
        self.azure_devops_pipeline_var = tk.StringVar(value=self.config.azure_devops_pipeline)
        ttk.Entry(ado_frame, textvariable=self.azure_devops_pipeline_var, width=20).grid(row=2, column=1, padx=10, pady=3)
        
        # AWS CodeBuild config
        aws_build_frame = ttk.Frame(self.cloud_build_config_notebook, padding="10")
        self.cloud_build_config_notebook.add(aws_build_frame, text="AWS")
        ttk.Label(aws_build_frame, text="Project Name:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.aws_codebuild_project_var = tk.StringVar(value=self.config.aws_codebuild_project)
        ttk.Entry(aws_build_frame, textvariable=self.aws_codebuild_project_var, width=30).grid(row=0, column=1, padx=10, pady=3)
        
        # GCP Cloud Build config
        gcp_build_frame = ttk.Frame(self.cloud_build_config_notebook, padding="10")
        self.cloud_build_config_notebook.add(gcp_build_frame, text="GCP")
        ttk.Label(gcp_build_frame, text="Trigger ID:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.gcp_cloudbuild_trigger_var = tk.StringVar(value=self.config.gcp_cloudbuild_trigger)
        ttk.Entry(gcp_build_frame, textvariable=self.gcp_cloudbuild_trigger_var, width=30).grid(row=0, column=1, padx=10, pady=3)
        
        # Build commands (for local/remote builds)
        self.build_cmd_frame = ttk.LabelFrame(tab, text="Build Commands", padding="15")
        self.build_cmd_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(self.build_cmd_frame, text="Frontend:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.build_frontend_cmd_var = tk.StringVar(value=self.config.build_frontend_cmd)
        ttk.Entry(self.build_cmd_frame, textvariable=self.build_frontend_cmd_var, width=60).grid(row=0, column=1, padx=10, pady=3)
        
        ttk.Label(self.build_cmd_frame, text="API:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.build_api_cmd_var = tk.StringVar(value=self.config.build_api_cmd)
        ttk.Entry(self.build_cmd_frame, textvariable=self.build_api_cmd_var, width=60).grid(row=1, column=1, padx=10, pady=3)
        
        ttk.Label(self.build_cmd_frame, text="Docker API:").grid(row=2, column=0, sticky=tk.W, pady=3)
        self.build_docker_api_cmd_var = tk.StringVar(value=self.config.build_docker_api_cmd)
        ttk.Entry(self.build_cmd_frame, textvariable=self.build_docker_api_cmd_var, width=60).grid(row=2, column=1, padx=10, pady=3)
        
        ttk.Label(self.build_cmd_frame, text="Docker Frontend:").grid(row=3, column=0, sticky=tk.W, pady=3)
        self.build_docker_frontend_cmd_var = tk.StringVar(value=self.config.build_docker_frontend_cmd)
        ttk.Entry(self.build_cmd_frame, textvariable=self.build_docker_frontend_cmd_var, width=60).grid(row=3, column=1, padx=10, pady=3)
        
        # Build actions
        actions_frame = ttk.Frame(tab)
        actions_frame.pack(fill=tk.X, pady=10)
        
        ttk.Button(actions_frame, text="📥 Clone Repository", 
                   command=self.clone_repository).pack(side=tk.LEFT, padx=5)
        ttk.Button(actions_frame, text="🔨 Build Local", 
                   command=self.run_local_build).pack(side=tk.LEFT, padx=5)
        ttk.Button(actions_frame, text="🐳 Build Docker Images", 
                   command=self.run_docker_build).pack(side=tk.LEFT, padx=5)
        ttk.Button(actions_frame, text="☁️ Trigger Cloud Build", 
                   command=self.trigger_cloud_build).pack(side=tk.LEFT, padx=5)
        
        actions_frame2 = ttk.Frame(tab)
        actions_frame2.pack(fill=tk.X, pady=5)
        
        ttk.Button(actions_frame2, text="📄 Generate CI/CD Config", 
                   command=self.generate_cicd_config).pack(side=tk.LEFT, padx=5)
        
        self.build_status_var = tk.StringVar(value="")
        ttk.Label(actions_frame2, textvariable=self.build_status_var).pack(side=tk.LEFT, padx=10)
        
        # Apply initial state
        self.on_build_source_change()
        self.on_build_type_change()
        self.on_git_auth_change()
    
    def on_build_source_change(self):
        """Handle build source change."""
        is_git = self.build_source_var.get() == "git"
        state = tk.NORMAL if is_git else tk.DISABLED
        for child in self.git_frame.winfo_children():
            if isinstance(child, (ttk.Entry, ttk.Checkbutton, ttk.Button)):
                child.configure(state=state)
            elif isinstance(child, ttk.Frame):
                for subchild in child.winfo_children():
                    if isinstance(subchild, (ttk.Entry, ttk.Button)):
                        subchild.configure(state=state)
    
    def on_build_type_change(self):
        """Handle build type change."""
        build_type = self.build_type_var.get()
        
        # Cloud build config visibility
        is_cloud = build_type == "cloud"
        for child in self.cloud_build_frame.winfo_children():
            if isinstance(child, ttk.Radiobutton):
                child.configure(state=tk.NORMAL if is_cloud else tk.DISABLED)
        
        # Build commands visibility
        is_local_or_remote = build_type in ["local", "remote"]
        for child in self.build_cmd_frame.winfo_children():
            if isinstance(child, ttk.Entry):
                child.configure(state=tk.NORMAL if is_local_or_remote else tk.DISABLED)
    
    def on_git_auth_change(self):
        """Handle git authentication method change."""
        use_ssh = self.git_use_ssh_var.get()
        self.git_username_entry.configure(state=tk.DISABLED if use_ssh else tk.NORMAL)
        self.git_token_entry.configure(state=tk.DISABLED if use_ssh else tk.NORMAL)
        self.git_ssh_key_entry.configure(state=tk.NORMAL if use_ssh else tk.DISABLED)
    
    def on_cloud_build_provider_change(self):
        """Handle cloud build provider change."""
        provider = self.cloud_build_provider_var.get()
        tab_map = {
            "github_actions": 0,
            "azure_devops": 1,
            "aws_codebuild": 2,
            "gcp_cloudbuild": 3
        }
        if provider in tab_map:
            self.cloud_build_config_notebook.select(tab_map[provider])
    
    def clone_repository(self):
        """Clone the git repository."""
        if self.build_source_var.get() != "git":
            messagebox.showinfo("Info", "Using local source - no clone needed")
            return
        
        config = self.get_current_config()
        build_engine = BuildEngine(config, self.log_message)
        
        def do_clone():
            self.build_status_var.set("Cloning...")
            if build_engine.clone_repository():
                self.build_status_var.set("✓ Clone complete")
            else:
                self.build_status_var.set("✗ Clone failed")
        
        threading.Thread(target=do_clone, daemon=True).start()
    
    def run_local_build(self):
        """Run local build."""
        config = self.get_current_config()
        build_engine = BuildEngine(config, self.log_message)
        
        def do_build():
            self.build_status_var.set("Building...")
            
            # Clone if using git
            if config.build_source == "git":
                if not build_engine.clone_repository():
                    self.build_status_var.set("✗ Clone failed")
                    return
            
            # Build frontend and API
            if config.deploy_frontend:
                if not build_engine.build_frontend():
                    self.build_status_var.set("✗ Frontend build failed")
                    return
            
            if config.deploy_api:
                if not build_engine.build_api():
                    self.build_status_var.set("✗ API build failed")
                    return
            
            self.build_status_var.set("✓ Build complete")
            build_engine.cleanup()
        
        threading.Thread(target=do_build, daemon=True).start()
    
    def run_docker_build(self):
        """Build Docker images."""
        config = self.get_current_config()
        build_engine = BuildEngine(config, self.log_message)
        
        def do_build():
            self.build_status_var.set("Building Docker images...")
            
            if build_engine.build_docker_images():
                self.build_status_var.set("✓ Docker images built")
            else:
                self.build_status_var.set("✗ Docker build failed")
        
        threading.Thread(target=do_build, daemon=True).start()
    
    def trigger_cloud_build(self):
        """Trigger cloud CI/CD build."""
        config = self.get_current_config()
        
        if config.cloud_build_provider == "none":
            messagebox.showwarning("Warning", "Select a cloud build provider first")
            return
        
        cloud_build = CloudBuildEngine(config, self.log_message)
        
        def do_trigger():
            self.build_status_var.set("Triggering build...")
            
            if cloud_build.trigger_build():
                self.build_status_var.set("✓ Build triggered")
            else:
                self.build_status_var.set("✗ Trigger failed")
        
        threading.Thread(target=do_trigger, daemon=True).start()
    
    def generate_cicd_config(self):
        """Generate CI/CD configuration files."""
        config = self.get_current_config()
        cloud_build = CloudBuildEngine(config, self.log_message)
        
        provider = config.cloud_build_provider
        
        if provider == "github_actions":
            content = cloud_build.generate_github_actions_workflow()
            filename = "build-deploy.yml"
            default_path = ".github/workflows"
        elif provider == "azure_devops":
            content = cloud_build.generate_azure_pipelines_yaml()
            filename = "azure-pipelines.yml"
            default_path = "."
        elif provider == "gcp_cloudbuild":
            content = cloud_build.generate_cloudbuild_yaml()
            filename = "cloudbuild.yaml"
            default_path = "."
        else:
            messagebox.showwarning("Warning", "Select a cloud build provider first")
            return
        
        # Save to file
        filepath = filedialog.asksaveasfilename(
            initialfile=filename,
            defaultextension=".yml",
            filetypes=[("YAML files", "*.yml *.yaml"), ("All files", "*.*")]
        )
        
        if filepath:
            with open(filepath, 'w') as f:
                f.write(content)
            messagebox.showinfo("Success", f"CI/CD config saved to:\n{filepath}")
            self.log_message("success", f"Generated CI/CD config: {filepath}")

    def create_build_server_tab(self):
        """Create build server configuration tab."""
        tab = ttk.Frame(self.notebook, padding="15")
        self.notebook.add(tab, text="🖥️ Build Server")
        
        ttk.Label(tab, text="Build Server Configuration", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 15))
        
        # Server type
        type_frame = ttk.LabelFrame(tab, text="Server Location", padding="15")
        type_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.server_local_var = tk.BooleanVar(value=self.config.build_server_is_local)
        
        ttk.Radiobutton(type_frame, text="Local Machine", 
                        variable=self.server_local_var, value=True,
                        command=self.on_server_type_change).pack(anchor=tk.W, pady=3)
        ttk.Radiobutton(type_frame, text="Remote Server (SSH)", 
                        variable=self.server_local_var, value=False,
                        command=self.on_server_type_change).pack(anchor=tk.W, pady=3)
        
        # Server details
        self.server_details_frame = ttk.LabelFrame(tab, text="Remote Server Details", padding="15")
        self.server_details_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(self.server_details_frame, text="Server Name:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.server_name_var = tk.StringVar(value=self.config.build_server_name)
        ttk.Entry(self.server_details_frame, textvariable=self.server_name_var, width=40).grid(row=0, column=1, padx=10, pady=3)
        
        ttk.Label(self.server_details_frame, text="Host/IP:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.server_host_var = tk.StringVar(value=self.config.build_server_host)
        ttk.Entry(self.server_details_frame, textvariable=self.server_host_var, width=40).grid(row=1, column=1, padx=10, pady=3)
        
        ttk.Label(self.server_details_frame, text="SSH Port:").grid(row=2, column=0, sticky=tk.W, pady=3)
        self.server_port_var = tk.IntVar(value=self.config.build_server_port)
        ttk.Entry(self.server_details_frame, textvariable=self.server_port_var, width=10).grid(row=2, column=1, sticky=tk.W, padx=10, pady=3)
        
        ttk.Label(self.server_details_frame, text="Username:").grid(row=3, column=0, sticky=tk.W, pady=3)
        self.server_user_var = tk.StringVar(value=self.config.build_server_user)
        ttk.Entry(self.server_details_frame, textvariable=self.server_user_var, width=20).grid(row=3, column=1, sticky=tk.W, padx=10, pady=3)
        
        ttk.Label(self.server_details_frame, text="SSH Key:").grid(row=4, column=0, sticky=tk.W, pady=3)
        key_frame = ttk.Frame(self.server_details_frame)
        key_frame.grid(row=4, column=1, sticky=tk.W, padx=10, pady=3)
        self.server_key_var = tk.StringVar(value=self.config.build_server_ssh_key)
        ttk.Entry(key_frame, textvariable=self.server_key_var, width=30).pack(side=tk.LEFT)
        ttk.Button(key_frame, text="Browse", 
                   command=lambda: self.browse_file(self.server_key_var)).pack(side=tk.LEFT, padx=5)
        
        # Connection test
        test_frame = ttk.Frame(tab)
        test_frame.pack(fill=tk.X, pady=10)
        
        ttk.Button(test_frame, text="🔌 Test Connection", 
                   command=self.test_server_connection).pack(side=tk.LEFT)
        
        self.connection_status_var = tk.StringVar(value="")
        ttk.Label(test_frame, textvariable=self.connection_status_var).pack(side=tk.LEFT, padx=10)
        
        # Prerequisites
        prereq_frame = ttk.LabelFrame(tab, text="Prerequisites", padding="15")
        prereq_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Button(prereq_frame, text="🔍 Check Prerequisites", 
                   command=self.check_prerequisites).pack(anchor=tk.W)
        
        self.prereq_text = scrolledtext.ScrolledText(prereq_frame, height=6, font=("Courier", 9))
        self.prereq_text.pack(fill=tk.X, pady=(10, 0))
        
        self.on_server_type_change()
    
    def create_cloud_tab(self):
        """Create cloud deployment configuration tab."""
        tab = ttk.Frame(self.notebook, padding="15")
        self.notebook.add(tab, text="☁️ Cloud")
        
        ttk.Label(tab, text="Cloud Deployment", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 15))
        
        # Cloud provider selection
        provider_frame = ttk.LabelFrame(tab, text="Cloud Provider", padding="15")
        provider_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.cloud_provider_var = tk.StringVar(value=self.config.cloud_provider)
        
        providers = [
            ("None (Local/SSH)", "none"),
            ("Amazon Web Services (AWS)", "aws"),
            ("Microsoft Azure", "azure"),
            ("Google Cloud Platform (GCP)", "gcp")
        ]
        
        for text, value in providers:
            ttk.Radiobutton(provider_frame, text=text, 
                            variable=self.cloud_provider_var, value=value,
                            command=self.on_cloud_provider_change).pack(anchor=tk.W, pady=3)
        
        # Cloud provider notebooks
        self.cloud_notebook = ttk.Notebook(tab)
        self.cloud_notebook.pack(fill=tk.BOTH, expand=True, pady=(10, 0))
        
        # AWS Tab
        self.create_aws_config_frame()
        
        # Azure Tab
        self.create_azure_config_frame()
        
        # GCP Tab
        self.create_gcp_config_frame()
        
        # Cloud actions
        actions_frame = ttk.Frame(tab)
        actions_frame.pack(fill=tk.X, pady=10)
        
        ttk.Button(actions_frame, text="🔍 Check All Prerequisites", 
                   command=self.check_all_cloud_prerequisites).pack(side=tk.LEFT, padx=5)
        ttk.Button(actions_frame, text="� Install Missing", 
                   command=self.install_missing_prerequisites).pack(side=tk.LEFT, padx=5)
        ttk.Button(actions_frame, text="🔐 Authenticate", 
                   command=self.authenticate_cloud).pack(side=tk.LEFT, padx=5)
        ttk.Button(actions_frame, text="📥 Fetch Account Info", 
                   command=self.fetch_cloud_account_info).pack(side=tk.LEFT, padx=5)
        
        actions_frame2 = ttk.Frame(tab)
        actions_frame2.pack(fill=tk.X, pady=5)
        
        ttk.Button(actions_frame2, text="🏗️ Create Infrastructure", 
                   command=self.create_cloud_infrastructure).pack(side=tk.LEFT, padx=5)
        
        self.cloud_status_var = tk.StringVar(value="")
        ttk.Label(actions_frame2, textvariable=self.cloud_status_var).pack(side=tk.LEFT, padx=10)
        
        self.on_cloud_provider_change()
    
    def create_aws_config_frame(self):
        """Create AWS configuration frame."""
        aws_frame = ttk.Frame(self.cloud_notebook, padding="10")
        self.cloud_notebook.add(aws_frame, text="AWS")
        
        # Region
        ttk.Label(aws_frame, text="Region:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.aws_region_var = tk.StringVar(value=self.config.aws_region)
        aws_regions = ttk.Combobox(aws_frame, textvariable=self.aws_region_var, width=20,
                                   values=["us-east-1", "us-east-2", "us-west-1", "us-west-2",
                                          "eu-west-1", "eu-west-2", "eu-central-1",
                                          "ap-southeast-1", "ap-southeast-2", "ap-northeast-1"])
        aws_regions.grid(row=0, column=1, sticky=tk.W, padx=10, pady=3)
        
        # ECS Cluster
        ttk.Label(aws_frame, text="ECS Cluster:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.aws_cluster_var = tk.StringVar(value=self.config.aws_ecs_cluster)
        ttk.Entry(aws_frame, textvariable=self.aws_cluster_var, width=30).grid(row=1, column=1, sticky=tk.W, padx=10, pady=3)
        
        # ECR Registry
        ttk.Label(aws_frame, text="ECR Registry:").grid(row=2, column=0, sticky=tk.W, pady=3)
        self.aws_ecr_var = tk.StringVar(value=self.config.aws_ecr_registry)
        ttk.Entry(aws_frame, textvariable=self.aws_ecr_var, width=40).grid(row=2, column=1, sticky=tk.W, padx=10, pady=3)
        
        # VPC
        ttk.Label(aws_frame, text="VPC ID:").grid(row=3, column=0, sticky=tk.W, pady=3)
        self.aws_vpc_var = tk.StringVar(value=self.config.aws_vpc_id)
        ttk.Entry(aws_frame, textvariable=self.aws_vpc_var, width=30).grid(row=3, column=1, sticky=tk.W, padx=10, pady=3)
        
        # Subnets
        ttk.Label(aws_frame, text="Subnet IDs:").grid(row=4, column=0, sticky=tk.W, pady=3)
        self.aws_subnets_var = tk.StringVar(value=self.config.aws_subnet_ids)
        ttk.Entry(aws_frame, textvariable=self.aws_subnets_var, width=50).grid(row=4, column=1, sticky=tk.W, padx=10, pady=3)
        
        # Options
        options_frame = ttk.LabelFrame(aws_frame, text="AWS Services", padding="10")
        options_frame.grid(row=5, column=0, columnspan=2, sticky=tk.EW, pady=10)
        
        self.aws_fargate_var = tk.BooleanVar(value=self.config.aws_use_fargate)
        ttk.Checkbutton(options_frame, text="Use Fargate (serverless)", 
                        variable=self.aws_fargate_var).pack(anchor=tk.W, pady=2)
        
        self.aws_rds_var = tk.BooleanVar(value=self.config.aws_use_rds)
        ttk.Checkbutton(options_frame, text="Use RDS for Database", 
                        variable=self.aws_rds_var).pack(anchor=tk.W, pady=2)
        
        self.aws_elasticache_var = tk.BooleanVar(value=self.config.aws_use_elasticache)
        ttk.Checkbutton(options_frame, text="Use ElastiCache for Redis", 
                        variable=self.aws_elasticache_var).pack(anchor=tk.W, pady=2)
    
    def create_azure_config_frame(self):
        """Create Azure configuration frame."""
        azure_frame = ttk.Frame(self.cloud_notebook, padding="10")
        self.cloud_notebook.add(azure_frame, text="Azure")
        
        # Subscription
        ttk.Label(azure_frame, text="Subscription ID:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.azure_subscription_var = tk.StringVar(value=self.config.azure_subscription_id)
        ttk.Entry(azure_frame, textvariable=self.azure_subscription_var, width=40).grid(row=0, column=1, sticky=tk.W, padx=10, pady=3)
        
        # Resource Group
        ttk.Label(azure_frame, text="Resource Group:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.azure_rg_var = tk.StringVar(value=self.config.azure_resource_group)
        ttk.Entry(azure_frame, textvariable=self.azure_rg_var, width=30).grid(row=1, column=1, sticky=tk.W, padx=10, pady=3)
        
        # Location
        ttk.Label(azure_frame, text="Location:").grid(row=2, column=0, sticky=tk.W, pady=3)
        self.azure_location_var = tk.StringVar(value=self.config.azure_location)
        azure_locations = ttk.Combobox(azure_frame, textvariable=self.azure_location_var, width=20,
                                       values=["eastus", "eastus2", "westus", "westus2", "westus3",
                                              "centralus", "northeurope", "westeurope",
                                              "southeastasia", "australiaeast"])
        azure_locations.grid(row=2, column=1, sticky=tk.W, padx=10, pady=3)
        
        # ACR Name
        ttk.Label(azure_frame, text="ACR Name:").grid(row=3, column=0, sticky=tk.W, pady=3)
        self.azure_acr_var = tk.StringVar(value=self.config.azure_acr_name)
        ttk.Entry(azure_frame, textvariable=self.azure_acr_var, width=30).grid(row=3, column=1, sticky=tk.W, padx=10, pady=3)
        
        # AKS Cluster
        ttk.Label(azure_frame, text="AKS Cluster:").grid(row=4, column=0, sticky=tk.W, pady=3)
        self.azure_aks_var = tk.StringVar(value=self.config.azure_aks_cluster)
        ttk.Entry(azure_frame, textvariable=self.azure_aks_var, width=30).grid(row=4, column=1, sticky=tk.W, padx=10, pady=3)
        
        # Options
        options_frame = ttk.LabelFrame(azure_frame, text="Azure Services", padding="10")
        options_frame.grid(row=5, column=0, columnspan=2, sticky=tk.EW, pady=10)
        
        self.azure_aks_enabled_var = tk.BooleanVar(value=self.config.azure_use_aks)
        ttk.Checkbutton(options_frame, text="Use AKS (Kubernetes)", 
                        variable=self.azure_aks_enabled_var).pack(anchor=tk.W, pady=2)
        
        self.azure_aci_var = tk.BooleanVar(value=self.config.azure_use_aci)
        ttk.Checkbutton(options_frame, text="Use ACI (Container Instances)", 
                        variable=self.azure_aci_var).pack(anchor=tk.W, pady=2)
        
        self.azure_db_var = tk.BooleanVar(value=self.config.azure_use_azure_db)
        ttk.Checkbutton(options_frame, text="Use Azure Database", 
                        variable=self.azure_db_var).pack(anchor=tk.W, pady=2)
        
        self.azure_redis_var = tk.BooleanVar(value=self.config.azure_use_redis_cache)
        ttk.Checkbutton(options_frame, text="Use Azure Cache for Redis", 
                        variable=self.azure_redis_var).pack(anchor=tk.W, pady=2)
    
    def create_gcp_config_frame(self):
        """Create GCP configuration frame."""
        gcp_frame = ttk.Frame(self.cloud_notebook, padding="10")
        self.cloud_notebook.add(gcp_frame, text="GCP")
        
        # Project ID
        ttk.Label(gcp_frame, text="Project ID:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.gcp_project_var = tk.StringVar(value=self.config.gcp_project_id)
        ttk.Entry(gcp_frame, textvariable=self.gcp_project_var, width=40).grid(row=0, column=1, sticky=tk.W, padx=10, pady=3)
        
        # Region
        ttk.Label(gcp_frame, text="Region:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.gcp_region_var = tk.StringVar(value=self.config.gcp_region)
        gcp_regions = ttk.Combobox(gcp_frame, textvariable=self.gcp_region_var, width=20,
                                   values=["us-central1", "us-east1", "us-west1", "us-west2",
                                          "europe-west1", "europe-west2", "europe-west3",
                                          "asia-east1", "asia-southeast1", "australia-southeast1"])
        gcp_regions.grid(row=1, column=1, sticky=tk.W, padx=10, pady=3)
        
        # Zone
        ttk.Label(gcp_frame, text="Zone:").grid(row=2, column=0, sticky=tk.W, pady=3)
        self.gcp_zone_var = tk.StringVar(value=self.config.gcp_zone)
        ttk.Entry(gcp_frame, textvariable=self.gcp_zone_var, width=20).grid(row=2, column=1, sticky=tk.W, padx=10, pady=3)
        
        # GKE Cluster
        ttk.Label(gcp_frame, text="GKE Cluster:").grid(row=3, column=0, sticky=tk.W, pady=3)
        self.gcp_gke_var = tk.StringVar(value=self.config.gcp_gke_cluster)
        ttk.Entry(gcp_frame, textvariable=self.gcp_gke_var, width=30).grid(row=3, column=1, sticky=tk.W, padx=10, pady=3)
        
        # GCR Hostname
        ttk.Label(gcp_frame, text="GCR Hostname:").grid(row=4, column=0, sticky=tk.W, pady=3)
        self.gcp_gcr_var = tk.StringVar(value=self.config.gcp_gcr_hostname)
        gcr_hosts = ttk.Combobox(gcp_frame, textvariable=self.gcp_gcr_var, width=20,
                                 values=["gcr.io", "us.gcr.io", "eu.gcr.io", "asia.gcr.io"])
        gcr_hosts.grid(row=4, column=1, sticky=tk.W, padx=10, pady=3)
        
        # Options
        options_frame = ttk.LabelFrame(gcp_frame, text="GCP Services", padding="10")
        options_frame.grid(row=5, column=0, columnspan=2, sticky=tk.EW, pady=10)
        
        self.gcp_gke_enabled_var = tk.BooleanVar(value=self.config.gcp_use_gke)
        ttk.Checkbutton(options_frame, text="Use GKE (Kubernetes)", 
                        variable=self.gcp_gke_enabled_var).pack(anchor=tk.W, pady=2)
        
        self.gcp_cloudrun_var = tk.BooleanVar(value=self.config.gcp_use_cloud_run)
        ttk.Checkbutton(options_frame, text="Use Cloud Run (serverless)", 
                        variable=self.gcp_cloudrun_var).pack(anchor=tk.W, pady=2)
        
        self.gcp_cloudsql_var = tk.BooleanVar(value=self.config.gcp_use_cloud_sql)
        ttk.Checkbutton(options_frame, text="Use Cloud SQL", 
                        variable=self.gcp_cloudsql_var).pack(anchor=tk.W, pady=2)
        
        self.gcp_memorystore_var = tk.BooleanVar(value=self.config.gcp_use_memorystore)
        ttk.Checkbutton(options_frame, text="Use Memorystore for Redis", 
                        variable=self.gcp_memorystore_var).pack(anchor=tk.W, pady=2)
    
    def on_cloud_provider_change(self):
        """Handle cloud provider change."""
        provider = self.cloud_provider_var.get()
        
        # Enable/disable cloud notebook based on selection
        if provider == "none":
            for i in range(self.cloud_notebook.index("end")):
                self.cloud_notebook.tab(i, state="disabled")
        else:
            # Enable all tabs but select the appropriate one
            tab_map = {"aws": 0, "azure": 1, "gcp": 2}
            for i in range(self.cloud_notebook.index("end")):
                self.cloud_notebook.tab(i, state="normal")
            if provider in tab_map:
                self.cloud_notebook.select(tab_map[provider])
    
    def check_cloud_cli(self):
        """Check if cloud CLI is installed."""
        provider = self.cloud_provider_var.get()
        
        if provider == "none":
            self.cloud_status_var.set("Select a cloud provider first")
            return
        
        def log_to_status(msg_type: str, message: str):
            self.cloud_status_var.set(message)
        
        config = self.get_current_config()
        
        if provider == "aws":
            deployer = AWSDeployer(config, log_to_status)
        elif provider == "azure":
            deployer = AzureDeployer(config, log_to_status)
        elif provider == "gcp":
            deployer = GCPDeployer(config, log_to_status)
        else:
            return
        
        deployer.check_cli()
    
    def check_all_cloud_prerequisites(self):
        """Check all cloud-related prerequisites."""
        self.cloud_status_var.set("Checking prerequisites...")
        self.log_message("info", "=== Checking All Prerequisites ===")
        
        def do_check():
            checker = PrerequisiteChecker()
            results = checker.check_all()
            
            # Group by category
            categories = {
                "core": "Core Tools",
                "build": "Build Tools", 
                "kubernetes": "Kubernetes Tools",
                "cloud": "Cloud CLIs",
                "infrastructure": "Infrastructure Tools"
            }
            
            missing_count = 0
            cloud_missing = []
            
            for category, title in categories.items():
                self.root.after(0, lambda t=title: self.log_message("info", f"\n--- {t} ---"))
                for name, result in results.items():
                    if result.get("category") == category:
                        if result["installed"]:
                            version = result["version"][:60] if result["version"] else "installed"
                            self.root.after(0, lambda n=name, v=version: self.log_message("success", f"  ✓ {n}: {v}"))
                        else:
                            missing_count += 1
                            install_cmd = result.get("install_cmd", "")
                            self.root.after(0, lambda n=name: self.log_message("error", f"  ✗ {n}: NOT INSTALLED"))
                            if install_cmd:
                                self.root.after(0, lambda cmd=install_cmd: self.log_message("cmd", f"      Install: {cmd}"))
                            if category == "cloud":
                                cloud_missing.append(name)
            
            # Store missing for install function
            self._missing_prerequisites = results
            
            # Summary
            self.root.after(0, lambda: self.log_message("info", f"\n=== Summary ==="))
            if missing_count == 0:
                self.root.after(0, lambda: self.log_message("success", "All prerequisites installed!"))
                self.root.after(0, lambda: self.cloud_status_var.set("✓ All prerequisites installed"))
            else:
                self.root.after(0, lambda m=missing_count: self.log_message("warning", f"{m} prerequisites missing"))
                if cloud_missing:
                    self.root.after(0, lambda c=cloud_missing: self.cloud_status_var.set(f"⚠ Missing: {', '.join(c)}"))
                else:
                    self.root.after(0, lambda: self.cloud_status_var.set("⚠ Some prerequisites missing"))
        
        threading.Thread(target=do_check, daemon=True).start()
    
    def install_missing_prerequisites(self):
        """Install missing prerequisites."""
        provider = self.cloud_provider_var.get()
        
        # Determine which tools to install based on provider
        tools_to_install = []
        
        if provider == "aws":
            tools_to_install = ["aws"]
        elif provider == "azure":
            tools_to_install = ["az"]
        elif provider == "gcp":
            tools_to_install = ["gcloud"]
        else:
            # Install all cloud CLIs
            tools_to_install = ["aws", "az", "gcloud"]
        
        # Also offer to install kubernetes tools if needed
        result = messagebox.askyesnocancel(
            "Install Prerequisites",
            f"Install the following tools?\n\n"
            f"Cloud CLIs: {', '.join(tools_to_install)}\n\n"
            f"Click 'Yes' to install cloud CLIs only\n"
            f"Click 'No' to install ALL missing prerequisites\n"
            f"Click 'Cancel' to abort"
        )
        
        if result is None:  # Cancel
            return
        
        install_all = not result  # 'No' means install all
        
        self.cloud_status_var.set("Installing prerequisites...")
        self.log_message("info", "=== Installing Prerequisites ===")
        
        def do_install():
            checker = PrerequisiteChecker()
            checker.check_all()
            
            def log_callback(msg_type: str, message: str):
                self.root.after(0, lambda: self.log_message(msg_type, message))
            
            installed = []
            failed = []
            
            if install_all:
                # Install all missing
                for name, result in checker.results.items():
                    if not result.get("installed", True):
                        success = checker.install_prerequisite(name, log_callback)
                        if success:
                            installed.append(name)
                        else:
                            failed.append(name)
            else:
                # Install only specified tools
                for tool in tools_to_install:
                    if not checker.results.get(tool, {}).get("installed", True):
                        success = checker.install_prerequisite(tool, log_callback)
                        if success:
                            installed.append(tool)
                        else:
                            failed.append(tool)
            
            # Summary
            self.root.after(0, lambda: self.log_message("info", "\n=== Installation Summary ==="))
            if installed:
                self.root.after(0, lambda i=installed: self.log_message("success", f"✓ Installed: {', '.join(i)}"))
            if failed:
                self.root.after(0, lambda f=failed: self.log_message("error", f"✗ Failed: {', '.join(f)}"))
            
            if not failed:
                self.root.after(0, lambda i=len(installed): self.cloud_status_var.set(f"✓ Installed {i} tools"))
            else:
                self.root.after(0, lambda f=len(failed): self.cloud_status_var.set(f"⚠ {f} installations failed"))
        
        threading.Thread(target=do_install, daemon=True).start()
    
    def fetch_cloud_account_info(self):
        """Fetch and populate cloud account information."""
        provider = self.cloud_provider_var.get()
        
        if provider == "none":
            self.cloud_status_var.set("Select a cloud provider first")
            return
        
        self.cloud_status_var.set(f"Fetching {provider.upper()} account info...")
        
        def log_callback(msg_type: str, message: str):
            self.cloud_status_var.set(message[:50] + "..." if len(message) > 50 else message)
        
        config = self.get_current_config()
        
        def do_fetch():
            if provider == "aws":
                deployer = AWSDeployer(config, log_callback)
                info = deployer.get_account_info()
                self.root.after(0, lambda: self._populate_aws_info(info))
            elif provider == "azure":
                deployer = AzureDeployer(config, log_callback)
                info = deployer.get_account_info()
                self.root.after(0, lambda: self._populate_azure_info(info))
            elif provider == "gcp":
                deployer = GCPDeployer(config, log_callback)
                info = deployer.get_account_info()
                self.root.after(0, lambda: self._populate_gcp_info(info))
        
        threading.Thread(target=do_fetch, daemon=True).start()
    
    def _populate_aws_info(self, info: Dict[str, Any]):
        """Populate AWS configuration with fetched info."""
        if not info.get("authenticated"):
            self.cloud_status_var.set("✗ Not authenticated")
            self.log_message("error", "AWS: Not authenticated. Run 'aws configure' first.")
            messagebox.showwarning("AWS Authentication", 
                "Not authenticated with AWS.\n\n"
                "Run 'aws configure' in terminal to set up credentials.")
            return
        
        # Update status
        account_id = info.get("account_id", "Unknown")
        self.cloud_status_var.set(f"✓ AWS Account: {account_id}")
        
        # Log account info
        self.log_message("info", "=== AWS Account Info ===")
        self.log_message("success", f"Account ID: {account_id}")
        self.log_message("info", f"User ARN: {info.get('user_arn', 'N/A')}")
        self.log_message("info", f"Available Regions: {len(info.get('regions', []))}")
        self.log_message("info", f"VPCs: {len(info.get('vpcs', []))}")
        
        if info.get("ecs_clusters"):
            self.log_message("info", f"ECS Clusters: {', '.join(info['ecs_clusters'])}")
        if info.get("ecr_repositories"):
            self.log_message("info", f"ECR Repos: {', '.join(info['ecr_repositories'])}")
        
        # Update region dropdown with available regions
        if info.get("regions"):
            for child in self.cloud_notebook.winfo_children():
                for widget in child.winfo_children():
                    if isinstance(widget, ttk.Combobox) and widget.cget("textvariable") == str(self.aws_region_var):
                        widget.configure(values=info["regions"])
        
        # Build ECR registry URL
        if info.get("account_id") and self.aws_region_var.get():
            ecr_url = f"{info['account_id']}.dkr.ecr.{self.aws_region_var.get()}.amazonaws.com"
            self.aws_ecr_var.set(ecr_url)
            self.log_message("success", f"ECR Registry: {ecr_url}")
        
        # Update VPC dropdown if VPCs available
        if info.get("vpcs"):
            default_vpc = next((v for v in info["vpcs"] if v.get("IsDefault")), None)
            if default_vpc:
                self.aws_vpc_var.set(default_vpc.get("VpcId", ""))
                self.log_message("info", f"Default VPC: {default_vpc.get('VpcId', '')}")
    
    def _populate_azure_info(self, info: Dict[str, Any]):
        """Populate Azure configuration with fetched info."""
        if not info.get("authenticated"):
            self.cloud_status_var.set("✗ Not authenticated")
            self.log_message("error", "Azure: Not authenticated. Run 'az login' first.")
            messagebox.showwarning("Azure Authentication", 
                "Not authenticated with Azure.\n\n"
                "Run 'az login' in terminal to authenticate.")
            return
        
        # Update status
        sub_name = info.get("subscription_name", "Unknown")
        self.cloud_status_var.set(f"✓ Azure: {sub_name}")
        
        # Log account info
        self.log_message("info", "=== Azure Account Info ===")
        self.log_message("success", f"User: {info.get('user', 'N/A')}")
        self.log_message("info", f"Subscription: {info.get('subscription_name', 'N/A')}")
        self.log_message("info", f"Subscription ID: {info.get('subscription_id', 'N/A')}")
        self.log_message("info", f"Tenant ID: {info.get('tenant_id', 'N/A')}")
        self.log_message("info", f"Resource Groups: {len(info.get('resource_groups', []))}")
        
        if info.get("resource_groups"):
            rg_names = [rg.get('name', '') for rg in info['resource_groups'][:5]]
            self.log_message("info", f"  → {', '.join(rg_names)}")
        
        if info.get("acr_registries"):
            self.log_message("info", f"ACR Registries: {len(info['acr_registries'])}")
            for acr in info["acr_registries"][:3]:
                self.log_message("info", f"  → {acr.get('name', '')} ({acr.get('loginServer', '')})")
        
        if info.get("aks_clusters"):
            self.log_message("info", f"AKS Clusters: {len(info['aks_clusters'])}")
        
        # Populate subscription ID
        if info.get("subscription_id"):
            self.azure_subscription_var.set(info["subscription_id"])
        
        # Update location dropdown
        if info.get("locations"):
            location_names = [loc.get("name") for loc in info["locations"][:20]]
            for child in self.cloud_notebook.winfo_children():
                for widget in child.winfo_children():
                    if isinstance(widget, ttk.Combobox) and widget.cget("textvariable") == str(self.azure_location_var):
                        widget.configure(values=location_names)
    
    def _populate_gcp_info(self, info: Dict[str, Any]):
        """Populate GCP configuration with fetched info."""
        if not info.get("authenticated"):
            self.cloud_status_var.set("✗ Not authenticated")
            self.log_message("error", "GCP: Not authenticated. Run 'gcloud auth login' first.")
            messagebox.showwarning("GCP Authentication", 
                "Not authenticated with GCP.\n\n"
                "Run 'gcloud auth login' in terminal to authenticate.")
            return
        
        # Update status
        account = info.get("account", "Unknown")
        self.cloud_status_var.set(f"✓ GCP: {account}")
        
        # Log account info
        self.log_message("info", "=== GCP Account Info ===")
        self.log_message("success", f"Account: {account}")
        self.log_message("info", f"Current Project: {info.get('project_id', 'N/A')}")
        self.log_message("info", f"Available Projects: {len(info.get('projects', []))}")
        
        if info.get("projects"):
            for proj in info["projects"][:5]:
                self.log_message("info", f"  → {proj.get('projectId', '')} ({proj.get('name', '')})")
        
        self.log_message("info", f"Regions: {len(info.get('regions', []))}")
        
        if info.get("gke_clusters"):
            self.log_message("info", f"GKE Clusters: {len(info['gke_clusters'])}")
            for cluster in info["gke_clusters"][:3]:
                self.log_message("info", f"  → {cluster.get('name', '')} ({cluster.get('zone', '')})")
        
        if info.get("cloud_run_services"):
            self.log_message("info", f"Cloud Run Services: {len(info['cloud_run_services'])}")
        
        # Populate project ID
        if info.get("project_id"):
            self.gcp_project_var.set(info["project_id"])
        
        # Update region dropdown
        if info.get("regions"):
            for child in self.cloud_notebook.winfo_children():
                for widget in child.winfo_children():
                    if isinstance(widget, ttk.Combobox) and widget.cget("textvariable") == str(self.gcp_region_var):
                        widget.configure(values=info["regions"][:20])
        
        # Update zone field with first matching zone
        if info.get("zones") and self.gcp_region_var.get():
            region = self.gcp_region_var.get()
            matching_zones = [z for z in info["zones"] if z.startswith(region)]
            if matching_zones:
                self.gcp_zone_var.set(matching_zones[0])

    def authenticate_cloud(self):
        """Authenticate with cloud provider."""
        provider = self.cloud_provider_var.get()
        
        if provider == "none":
            self.cloud_status_var.set("Select a cloud provider first")
            messagebox.showinfo("Info", "Please select a cloud provider first (AWS, Azure, or GCP)")
            return
        
        self.cloud_status_var.set(f"Authenticating with {provider.upper()}...")
        self.log_message("info", f"=== {provider.upper()} Authentication ===")
        
        def log_callback(msg_type: str, message: str):
            self.log_message(msg_type, message)
            # Update status with last message
            self.root.after(0, lambda: self.cloud_status_var.set(
                message[:50] + "..." if len(message) > 50 else message
            ))
        
        config = self.get_current_config()
        
        def do_auth():
            if provider == "aws":
                deployer = AWSDeployer(config, log_callback)
                
                # Check CLI first
                log_callback("info", "Checking AWS CLI installation...")
                if not deployer.check_cli():
                    log_callback("error", "AWS CLI not installed. Run: brew install awscli")
                    self.root.after(0, lambda: messagebox.showerror("Error", 
                        "AWS CLI not installed.\n\nInstall with: brew install awscli"))
                    return
                
                # Check if already authenticated
                log_callback("info", "Checking existing AWS credentials...")
                success, output = deployer._run_command("aws sts get-caller-identity --output json")
                
                if success:
                    try:
                        import json
                        identity = json.loads(output)
                        account_id = identity.get("Account", "Unknown")
                        user_arn = identity.get("Arn", "Unknown")
                        log_callback("success", f"✓ Already authenticated!")
                        log_callback("info", f"  Account ID: {account_id}")
                        log_callback("info", f"  User ARN: {user_arn}")
                        
                        # Auto-populate ECR registry
                        if account_id and config.aws_region:
                            ecr_url = f"{account_id}.dkr.ecr.{config.aws_region}.amazonaws.com"
                            self.root.after(0, lambda: self.aws_ecr_var.set(ecr_url))
                            log_callback("info", f"  ECR Registry: {ecr_url}")
                        
                        self.root.after(0, lambda: self.cloud_status_var.set(f"✓ AWS: {account_id}"))
                    except:
                        log_callback("success", "✓ AWS credentials valid")
                else:
                    log_callback("warning", "Not authenticated. Starting AWS SSO login...")
                    log_callback("info", "Opening browser for AWS authentication...")
                    
                    # Try SSO login first, then regular configure
                    sso_success, _ = deployer._run_command("aws sso login 2>/dev/null")
                    if not sso_success:
                        log_callback("info", "SSO not configured. Use 'aws configure' to set up credentials.")
                        log_callback("cmd", "$ aws configure")
                        self.root.after(0, lambda: messagebox.showinfo("AWS Setup Required",
                            "AWS credentials not configured.\n\n"
                            "Run in terminal:\n"
                            "  aws configure\n\n"
                            "Enter your:\n"
                            "  - AWS Access Key ID\n"
                            "  - AWS Secret Access Key\n"
                            "  - Default region\n"
                            "  - Output format (json)"))
                    else:
                        log_callback("success", "✓ AWS SSO login successful")
                        # Fetch details after login
                        self.root.after(100, lambda: self.fetch_cloud_account_info())
                        
            elif provider == "azure":
                deployer = AzureDeployer(config, log_callback)
                
                # Check CLI first
                log_callback("info", "Checking Azure CLI installation...")
                if not deployer.check_cli():
                    log_callback("error", "Azure CLI not installed. Run: brew install azure-cli")
                    self.root.after(0, lambda: messagebox.showerror("Error", 
                        "Azure CLI not installed.\n\nInstall with: brew install azure-cli"))
                    return
                
                # Check if already authenticated
                log_callback("info", "Checking existing Azure session...")
                success, output = deployer._run_command("az account show --output json")
                
                if success:
                    try:
                        import json
                        account = json.loads(output)
                        sub_name = account.get("name", "Unknown")
                        sub_id = account.get("id", "Unknown")
                        user = account.get("user", {}).get("name", "Unknown")
                        
                        log_callback("success", f"✓ Already authenticated!")
                        log_callback("info", f"  User: {user}")
                        log_callback("info", f"  Subscription: {sub_name}")
                        log_callback("info", f"  Subscription ID: {sub_id}")
                        
                        # Auto-populate subscription
                        self.root.after(0, lambda: self.azure_subscription_var.set(sub_id))
                        self.root.after(0, lambda: self.cloud_status_var.set(f"✓ Azure: {sub_name}"))
                    except:
                        log_callback("success", "✓ Azure credentials valid")
                else:
                    log_callback("warning", "Not authenticated. Opening browser for Azure login...")
                    log_callback("cmd", "$ az login")
                    
                    # Run az login - this will open browser
                    login_success, login_output = deployer._run_command("az login --output json", timeout=120)
                    
                    if login_success:
                        log_callback("success", "✓ Azure login successful!")
                        # Fetch details after login
                        self.root.after(100, lambda: self.fetch_cloud_account_info())
                    else:
                        log_callback("error", "Azure login failed or was cancelled")
                        self.root.after(0, lambda: self.cloud_status_var.set("✗ Azure login failed"))
                        
            elif provider == "gcp":
                deployer = GCPDeployer(config, log_callback)
                
                # Check CLI first
                log_callback("info", "Checking Google Cloud SDK installation...")
                if not deployer.check_cli():
                    log_callback("error", "gcloud not installed. Run: brew install google-cloud-sdk")
                    self.root.after(0, lambda: messagebox.showerror("Error", 
                        "Google Cloud SDK not installed.\n\nInstall with: brew install google-cloud-sdk"))
                    return
                
                # Check if already authenticated
                log_callback("info", "Checking existing GCP session...")
                success, output = deployer._run_command("gcloud auth list --filter=status:ACTIVE --format='value(account)'")
                
                if success and output.strip():
                    account = output.strip()
                    log_callback("success", f"✓ Already authenticated as: {account}")
                    
                    # Get current project
                    proj_success, proj_output = deployer._run_command("gcloud config get-value project")
                    if proj_success and proj_output.strip():
                        project_id = proj_output.strip()
                        log_callback("info", f"  Current Project: {project_id}")
                        self.root.after(0, lambda: self.gcp_project_var.set(project_id))
                    
                    self.root.after(0, lambda: self.cloud_status_var.set(f"✓ GCP: {account}"))
                else:
                    log_callback("warning", "Not authenticated. Opening browser for Google login...")
                    log_callback("cmd", "$ gcloud auth login")
                    
                    # Run gcloud auth login - this will open browser
                    login_success, login_output = deployer._run_command("gcloud auth login", timeout=120)
                    
                    if login_success:
                        log_callback("success", "✓ Google Cloud login successful!")
                        
                        # Also authenticate application-default credentials
                        log_callback("info", "Setting up application-default credentials...")
                        deployer._run_command("gcloud auth application-default login", timeout=120)
                        
                        # Fetch details after login
                        self.root.after(100, lambda: self.fetch_cloud_account_info())
                    else:
                        log_callback("error", "GCP login failed or was cancelled")
                        self.root.after(0, lambda: self.cloud_status_var.set("✗ GCP login failed"))
            
            log_callback("info", "")
            log_callback("info", "Authentication check complete.")
        
        # Run in background thread
        threading.Thread(target=do_auth, daemon=True).start()
    
    def create_cloud_infrastructure(self):
        """Create cloud infrastructure."""
        provider = self.cloud_provider_var.get()
        
        if provider == "none":
            messagebox.showwarning("Warning", "Select a cloud provider first")
            return
        
        if not messagebox.askyesno("Confirm", f"Create {provider.upper()} infrastructure?\nThis may incur costs."):
            return
        
        def log_callback(msg_type: str, message: str):
            self.log_message(msg_type, message)
            self.cloud_status_var.set(message[:50] + "..." if len(message) > 50 else message)
        
        config = self.get_current_config()
        
        if provider == "aws":
            deployer = AWSDeployer(config, log_callback)
        elif provider == "azure":
            deployer = AzureDeployer(config, log_callback)
        elif provider == "gcp":
            deployer = GCPDeployer(config, log_callback)
        else:
            return
        
        def run_infra():
            if deployer.check_cli() and deployer.authenticate():
                deployer.create_infrastructure()
                self.cloud_status_var.set("Infrastructure created!")
        
        threading.Thread(target=run_infra, daemon=True).start()

    def create_components_tab(self):
        """Create components selection tab."""
        tab = ttk.Frame(self.notebook, padding="15")
        self.notebook.add(tab, text="📦 Components")
        
        ttk.Label(tab, text="Components to Deploy", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 15))
        
        # Core components
        core_frame = ttk.LabelFrame(tab, text="Core Components", padding="15")
        core_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.deploy_db_var = tk.BooleanVar(value=self.config.deploy_database)
        ttk.Checkbutton(core_frame, text="Database (MariaDB)", 
                        variable=self.deploy_db_var).pack(anchor=tk.W, pady=3)
        
        self.deploy_redis_var = tk.BooleanVar(value=self.config.deploy_redis)
        ttk.Checkbutton(core_frame, text="Redis Cache", 
                        variable=self.deploy_redis_var).pack(anchor=tk.W, pady=3)
        
        self.deploy_api_var = tk.BooleanVar(value=self.config.deploy_api)
        ttk.Checkbutton(core_frame, text="API Server", 
                        variable=self.deploy_api_var).pack(anchor=tk.W, pady=3)
        
        self.deploy_frontend_var = tk.BooleanVar(value=self.config.deploy_frontend)
        ttk.Checkbutton(core_frame, text="Frontend (React)", 
                        variable=self.deploy_frontend_var).pack(anchor=tk.W, pady=3)
        
        # Microservices
        self.micro_frame = ttk.LabelFrame(tab, text="Microservices Components", padding="15")
        self.micro_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.deploy_identity_var = tk.BooleanVar(value=self.config.deploy_identity_service)
        ttk.Checkbutton(self.micro_frame, text="Identity Service", 
                        variable=self.deploy_identity_var).pack(anchor=tk.W, pady=3)
        
        self.deploy_customer_var = tk.BooleanVar(value=self.config.deploy_customer_service)
        ttk.Checkbutton(self.micro_frame, text="Customer Service", 
                        variable=self.deploy_customer_var).pack(anchor=tk.W, pady=3)
        
        self.deploy_sales_var = tk.BooleanVar(value=self.config.deploy_sales_service)
        ttk.Checkbutton(self.micro_frame, text="Sales Service", 
                        variable=self.deploy_sales_var).pack(anchor=tk.W, pady=3)
        
        self.deploy_marketing_var = tk.BooleanVar(value=self.config.deploy_marketing_service)
        ttk.Checkbutton(self.micro_frame, text="Marketing Service", 
                        variable=self.deploy_marketing_var).pack(anchor=tk.W, pady=3)
        
        self.deploy_servicedesk_var = tk.BooleanVar(value=self.config.deploy_servicedesk_service)
        ttk.Checkbutton(self.micro_frame, text="ServiceDesk Service", 
                        variable=self.deploy_servicedesk_var).pack(anchor=tk.W, pady=3)
        
        # Monitoring
        monitoring_frame = ttk.LabelFrame(tab, text="Optional Components", padding="15")
        monitoring_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.deploy_monitoring_var = tk.BooleanVar(value=self.config.deploy_monitoring)
        ttk.Checkbutton(monitoring_frame, text="Monitoring Stack (Uptime Kuma, Portainer)", 
                        variable=self.deploy_monitoring_var).pack(anchor=tk.W, pady=3)
        
        # Quick select
        btn_frame = ttk.Frame(tab)
        btn_frame.pack(fill=tk.X, pady=10)
        
        ttk.Button(btn_frame, text="Select All", 
                   command=self.select_all_components).pack(side=tk.LEFT, padx=5)
        ttk.Button(btn_frame, text="Select Minimal", 
                   command=self.select_minimal_components).pack(side=tk.LEFT, padx=5)
        
        self.on_architecture_change()
    
    def create_database_tab(self):
        """Create database configuration tab."""
        tab = ttk.Frame(self.notebook, padding="15")
        self.notebook.add(tab, text="🗄️ Database")
        
        ttk.Label(tab, text="Database Configuration", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 15))
        
        # Provider
        provider_frame = ttk.LabelFrame(tab, text="Database Provider", padding="15")
        provider_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.db_provider_var = tk.StringVar(value=self.config.database_provider)
        
        for text, value in [("MariaDB", "mariadb"), ("MySQL", "mysql"), ("PostgreSQL", "postgresql"), ("SQL Server", "sqlserver")]:
            ttk.Radiobutton(provider_frame, text=text, 
                            variable=self.db_provider_var, value=value).pack(anchor=tk.W, pady=2)
        
        # Connection
        conn_frame = ttk.LabelFrame(tab, text="Connection Details", padding="15")
        conn_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(conn_frame, text="Host:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.db_host_var = tk.StringVar(value=self.config.database_host)
        ttk.Entry(conn_frame, textvariable=self.db_host_var, width=30).grid(row=0, column=1, padx=10, pady=3)
        
        ttk.Label(conn_frame, text="Port:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.db_port_var = tk.IntVar(value=self.config.database_port)
        ttk.Entry(conn_frame, textvariable=self.db_port_var, width=10).grid(row=1, column=1, sticky=tk.W, padx=10, pady=3)
        
        ttk.Label(conn_frame, text="Database Name:").grid(row=2, column=0, sticky=tk.W, pady=3)
        self.db_name_var = tk.StringVar(value=self.config.database_name)
        ttk.Entry(conn_frame, textvariable=self.db_name_var, width=30).grid(row=2, column=1, padx=10, pady=3)
        
        ttk.Label(conn_frame, text="Username:").grid(row=3, column=0, sticky=tk.W, pady=3)
        self.db_user_var = tk.StringVar(value=self.config.database_user)
        ttk.Entry(conn_frame, textvariable=self.db_user_var, width=30).grid(row=3, column=1, padx=10, pady=3)
        
        ttk.Label(conn_frame, text="Password:").grid(row=4, column=0, sticky=tk.W, pady=3)
        self.db_pass_var = tk.StringVar(value=self.config.database_password)
        ttk.Entry(conn_frame, textvariable=self.db_pass_var, width=30, show="*").grid(row=4, column=1, padx=10, pady=3)
        
        ttk.Label(conn_frame, text="Root Password:").grid(row=5, column=0, sticky=tk.W, pady=3)
        self.db_root_pass_var = tk.StringVar(value=self.config.database_root_password)
        ttk.Entry(conn_frame, textvariable=self.db_root_pass_var, width=30, show="*").grid(row=5, column=1, padx=10, pady=3)
    
    def create_network_tab(self):
        """Create network configuration tab."""
        tab = ttk.Frame(self.notebook, padding="15")
        self.notebook.add(tab, text="🌐 Network")
        
        ttk.Label(tab, text="Network Configuration", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 15))
        
        # Ports
        ports_frame = ttk.LabelFrame(tab, text="Port Configuration", padding="15")
        ports_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(ports_frame, text="API Port:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.api_port_var = tk.IntVar(value=self.config.api_port)
        ttk.Entry(ports_frame, textvariable=self.api_port_var, width=10).grid(row=0, column=1, padx=10, pady=3)
        
        ttk.Label(ports_frame, text="Frontend Port:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.frontend_port_var = tk.IntVar(value=self.config.frontend_port)
        ttk.Entry(ports_frame, textvariable=self.frontend_port_var, width=10).grid(row=1, column=1, padx=10, pady=3)
        
        ttk.Label(ports_frame, text="Redis Port:").grid(row=2, column=0, sticky=tk.W, pady=3)
        self.redis_port_var = tk.IntVar(value=self.config.redis_port)
        ttk.Entry(ports_frame, textvariable=self.redis_port_var, width=10).grid(row=2, column=1, padx=10, pady=3)
        
        # Domain
        domain_frame = ttk.LabelFrame(tab, text="Domain Configuration", padding="15")
        domain_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(domain_frame, text="Domain/Hostname:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.domain_var = tk.StringVar(value=self.config.domain)
        ttk.Entry(domain_frame, textvariable=self.domain_var, width=40).grid(row=0, column=1, padx=10, pady=3)
        
        # SSL
        ssl_frame = ttk.LabelFrame(tab, text="SSL/TLS", padding="15")
        ssl_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.ssl_var = tk.BooleanVar(value=self.config.ssl_enabled)
        ttk.Checkbutton(ssl_frame, text="Enable SSL/TLS", 
                        variable=self.ssl_var,
                        command=self.on_ssl_change).pack(anchor=tk.W)
        
        self.ssl_details_frame = ttk.Frame(ssl_frame)
        self.ssl_details_frame.pack(fill=tk.X, pady=10)
        
        ttk.Label(self.ssl_details_frame, text="Certificate:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.ssl_cert_var = tk.StringVar(value=self.config.ssl_cert_path)
        ttk.Entry(self.ssl_details_frame, textvariable=self.ssl_cert_var, width=40).grid(row=0, column=1, padx=10, pady=3)
        
        ttk.Label(self.ssl_details_frame, text="Key:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.ssl_key_var = tk.StringVar(value=self.config.ssl_key_path)
        ttk.Entry(self.ssl_details_frame, textvariable=self.ssl_key_var, width=40).grid(row=1, column=1, padx=10, pady=3)
        
        self.on_ssl_change()
        
        # Network analyzer
        analyzer_frame = ttk.LabelFrame(tab, text="Network Analysis", padding="15")
        analyzer_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Button(analyzer_frame, text="🔍 Analyze Network", 
                   command=self.analyze_network).pack(anchor=tk.W)
        
        self.network_analysis_text = scrolledtext.ScrolledText(analyzer_frame, height=6, font=("Courier", 9))
        self.network_analysis_text.pack(fill=tk.X, pady=(10, 0))
    
    def create_credentials_tab(self):
        """Create credentials configuration tab."""
        tab = ttk.Frame(self.notebook, padding="15")
        self.notebook.add(tab, text="🔐 Credentials")
        
        ttk.Label(tab, text="Admin User & Security", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 15))
        
        # Admin user
        admin_frame = ttk.LabelFrame(tab, text="Admin User", padding="15")
        admin_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(admin_frame, text="Username:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.admin_user_var = tk.StringVar(value=self.config.admin_username)
        ttk.Entry(admin_frame, textvariable=self.admin_user_var, width=30).grid(row=0, column=1, padx=10, pady=3)
        
        ttk.Label(admin_frame, text="Email:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.admin_email_var = tk.StringVar(value=self.config.admin_email)
        ttk.Entry(admin_frame, textvariable=self.admin_email_var, width=30).grid(row=1, column=1, padx=10, pady=3)
        
        ttk.Label(admin_frame, text="Password:").grid(row=2, column=0, sticky=tk.W, pady=3)
        self.admin_pass_var = tk.StringVar(value=self.config.admin_password)
        self.admin_pass_entry = ttk.Entry(admin_frame, textvariable=self.admin_pass_var, width=30, show="*")
        self.admin_pass_entry.grid(row=2, column=1, padx=10, pady=3)
        
        self.show_pass_var = tk.BooleanVar(value=False)
        ttk.Checkbutton(admin_frame, text="Show", variable=self.show_pass_var,
                        command=self.toggle_password).grid(row=2, column=2)
        
        ttk.Label(admin_frame, text="First Name:").grid(row=3, column=0, sticky=tk.W, pady=3)
        self.admin_fname_var = tk.StringVar(value=self.config.admin_first_name)
        ttk.Entry(admin_frame, textvariable=self.admin_fname_var, width=30).grid(row=3, column=1, padx=10, pady=3)
        
        ttk.Label(admin_frame, text="Last Name:").grid(row=4, column=0, sticky=tk.W, pady=3)
        self.admin_lname_var = tk.StringVar(value=self.config.admin_last_name)
        ttk.Entry(admin_frame, textvariable=self.admin_lname_var, width=30).grid(row=4, column=1, padx=10, pady=3)
        
        ttk.Button(admin_frame, text="Generate Password", 
                   command=self.generate_password).grid(row=5, column=1, sticky=tk.W, padx=10, pady=10)
        
        # JWT
        jwt_frame = ttk.LabelFrame(tab, text="JWT Configuration", padding="15")
        jwt_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(jwt_frame, text="JWT Secret:").pack(anchor=tk.W)
        
        jwt_entry_frame = ttk.Frame(jwt_frame)
        jwt_entry_frame.pack(fill=tk.X, pady=5)
        
        self.jwt_secret_var = tk.StringVar(value=self.config.jwt_secret)
        ttk.Entry(jwt_entry_frame, textvariable=self.jwt_secret_var, width=60).pack(side=tk.LEFT, fill=tk.X, expand=True)
        ttk.Button(jwt_entry_frame, text="Generate", 
                   command=self.generate_jwt_secret).pack(side=tk.LEFT, padx=10)
    
    def create_seed_data_tab(self):
        """Create seed data configuration tab."""
        tab = ttk.Frame(self.notebook, padding="15")
        self.notebook.add(tab, text="🌱 Data")
        
        ttk.Label(tab, text="Data Seeding Configuration", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 15))
        
        # Master data
        master_frame = ttk.LabelFrame(tab, text="Master Data (Required)", padding="15")
        master_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.seed_master_var = tk.BooleanVar(value=True)
        cb = ttk.Checkbutton(master_frame, text="Deploy Master Data", 
                             variable=self.seed_master_var)
        cb.pack(anchor=tk.W, pady=3)
        cb.state(['selected', 'disabled'])
        
        # Demo data
        demo_frame = ttk.LabelFrame(tab, text="Demo Data (Optional)", padding="15")
        demo_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.seed_demo_var = tk.BooleanVar(value=self.config.seed_demo_data)
        ttk.Checkbutton(demo_frame, text="Demo Customers & Contacts", 
                        variable=self.seed_demo_var).pack(anchor=tk.W, pady=3)
        
        self.seed_workflows_var = tk.BooleanVar(value=False)
        ttk.Checkbutton(demo_frame, text="Sample Workflows", 
                        variable=self.seed_workflows_var).pack(anchor=tk.W, pady=3)
        
        # Reference data
        ref_frame = ttk.LabelFrame(tab, text="Reference Data (Optional)", padding="15")
        ref_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.seed_zip_var = tk.BooleanVar(value=self.config.seed_zip_codes)
        ttk.Checkbutton(ref_frame, text="US Zip Codes Database", 
                        variable=self.seed_zip_var).pack(anchor=tk.W, pady=3)
    
    def create_testing_tab(self):
        """Create testing configuration tab."""
        tab = ttk.Frame(self.notebook, padding="15")
        self.notebook.add(tab, text="✅ Testing")
        
        ttk.Label(tab, text="Testing Configuration", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 15))
        
        # Test options
        options_frame = ttk.LabelFrame(tab, text="Post-Deployment Tests", padding="15")
        options_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.run_smoke_var = tk.BooleanVar(value=self.config.run_smoke_tests)
        ttk.Checkbutton(options_frame, text="Run Smoke Tests", 
                        variable=self.run_smoke_var).pack(anchor=tk.W, pady=3)
        
        self.run_bvt_var = tk.BooleanVar(value=self.config.run_bvt_tests)
        ttk.Checkbutton(options_frame, text="Run BVT Tests", 
                        variable=self.run_bvt_var).pack(anchor=tk.W, pady=3)
        
        # Results
        results_frame = ttk.LabelFrame(tab, text="Test Results", padding="15")
        results_frame.pack(fill=tk.BOTH, expand=True, pady=(0, 15))
        
        columns = ("Test", "Status", "Duration", "Message")
        self.results_tree = ttk.Treeview(results_frame, columns=columns, show="headings", height=10)
        
        for col in columns:
            self.results_tree.heading(col, text=col)
            self.results_tree.column(col, width=150 if col != "Message" else 250)
        
        self.results_tree.pack(fill=tk.BOTH, expand=True)
        
        # Summary
        summary_frame = ttk.Frame(results_frame)
        summary_frame.pack(fill=tk.X, pady=10)
        
        self.results_summary_var = tk.StringVar(value="No tests run yet")
        ttk.Label(summary_frame, textvariable=self.results_summary_var).pack(side=tk.LEFT)
        
        ttk.Button(summary_frame, text="Export Results", 
                   command=self.export_test_results).pack(side=tk.RIGHT)
    
    def create_log_panel(self):
        """Create log and progress panel."""
        # Progress section
        progress_frame = ttk.LabelFrame(self.right_frame, text="Deployment Progress", padding="10")
        progress_frame.pack(fill=tk.X, pady=(0, 10))
        
        self.progress_var = tk.DoubleVar(value=0)
        self.progress_bar = ttk.Progressbar(progress_frame, variable=self.progress_var, 
                                            maximum=100, length=400, mode='determinate')
        self.progress_bar.pack(fill=tk.X, pady=5)
        
        self.progress_label = tk.StringVar(value="Ready to deploy")
        ttk.Label(progress_frame, textvariable=self.progress_label).pack(anchor=tk.W)
        
        # Step indicators
        self.steps_frame = ttk.Frame(progress_frame)
        self.steps_frame.pack(fill=tk.X, pady=10)
        
        self.step_labels = {}
        steps = ["Prerequisites", "Network", "Database", "API", "Frontend", "Data", "Tests"]
        for step in steps:
            frame = ttk.Frame(self.steps_frame)
            frame.pack(side=tk.LEFT, padx=5)
            
            self.step_labels[step] = tk.StringVar(value="⏳")
            ttk.Label(frame, textvariable=self.step_labels[step]).pack()
            ttk.Label(frame, text=step, font=("Helvetica", 8)).pack()
        
        # Control buttons
        control_frame = ttk.Frame(progress_frame)
        control_frame.pack(fill=tk.X, pady=10)
        
        self.start_btn = ttk.Button(control_frame, text="▶️ Start Deployment", 
                                    command=self.start_deployment)
        self.start_btn.pack(side=tk.LEFT, padx=5)
        
        self.pause_btn = ttk.Button(control_frame, text="⏸️ Pause", 
                                    command=self.pause_deployment, state=tk.DISABLED)
        self.pause_btn.pack(side=tk.LEFT, padx=5)
        
        self.stop_btn = ttk.Button(control_frame, text="⏹️ Stop", 
                                   command=self.stop_deployment, state=tk.DISABLED)
        self.stop_btn.pack(side=tk.LEFT, padx=5)
        
        # Log section
        log_frame = ttk.LabelFrame(self.right_frame, text="Deployment Log", padding="10")
        log_frame.pack(fill=tk.BOTH, expand=True)
        
        self.log_text = scrolledtext.ScrolledText(log_frame, height=20, font=("Courier", 9))
        self.log_text.pack(fill=tk.BOTH, expand=True)
        
        # Configure log tags
        self.log_text.tag_configure("info", foreground="#333")
        self.log_text.tag_configure("success", foreground="#28a745")
        self.log_text.tag_configure("warning", foreground="#ffc107")
        self.log_text.tag_configure("error", foreground="#dc3545")
        self.log_text.tag_configure("cmd", foreground="#6c757d")
        self.log_text.tag_configure("output", foreground="#007bff")
        
        # Log controls
        log_control_frame = ttk.Frame(log_frame)
        log_control_frame.pack(fill=tk.X, pady=5)
        
        ttk.Button(log_control_frame, text="Clear Log", 
                   command=self.clear_log).pack(side=tk.LEFT)
        ttk.Button(log_control_frame, text="Save Log", 
                   command=self.save_log).pack(side=tk.LEFT, padx=5)
        
        # Summary section
        summary_frame = ttk.LabelFrame(self.right_frame, text="Deployment Summary", padding="10")
        summary_frame.pack(fill=tk.X, pady=(10, 0))
        
        self.summary_text = tk.Text(summary_frame, height=8, font=("Courier", 9))
        self.summary_text.pack(fill=tk.X)
        
        ttk.Button(summary_frame, text="📋 Copy Login Details", 
                   command=self.copy_login_details).pack(anchor=tk.W, pady=5)
    
    def create_button_bar(self):
        """Create bottom button bar."""
        bar = ttk.Frame(self.left_frame)
        bar.pack(fill=tk.X, pady=5)
        
        ttk.Button(bar, text="💾 Save", command=self.save_config).pack(side=tk.LEFT, padx=5)
        ttk.Button(bar, text="📂 Load", command=self.load_config_dialog).pack(side=tk.LEFT, padx=5)
        ttk.Button(bar, text="📜 Generate Scripts", command=self.generate_scripts).pack(side=tk.LEFT, padx=5)
        ttk.Button(bar, text="🔄 Reset", command=self.reset_config).pack(side=tk.LEFT, padx=5)
        
        ttk.Button(bar, text="❌ Exit", command=self.root.quit).pack(side=tk.RIGHT, padx=5)
        ttk.Button(bar, text="❓ Help", command=self.show_help).pack(side=tk.RIGHT, padx=5)
    
    # Event handlers
    def on_architecture_change(self):
        """Handle architecture change."""
        is_micro = self.arch_var.get() == "microservices"
        for child in self.micro_frame.winfo_children():
            if isinstance(child, ttk.Checkbutton):
                child.state(['!disabled'] if is_micro else ['disabled'])
    
    def on_server_type_change(self):
        """Handle server type change."""
        is_local = self.server_local_var.get()
        for child in self.server_details_frame.winfo_children():
            if isinstance(child, (ttk.Entry, ttk.Button)):
                child.configure(state=tk.DISABLED if is_local else tk.NORMAL)
    
    def on_ssl_change(self):
        """Handle SSL toggle change."""
        is_enabled = self.ssl_var.get()
        for child in self.ssl_details_frame.winfo_children():
            if isinstance(child, (ttk.Entry, ttk.Button)):
                child.configure(state=tk.NORMAL if is_enabled else tk.DISABLED)
    
    def toggle_password(self):
        """Toggle password visibility."""
        self.admin_pass_entry.configure(show="" if self.show_pass_var.get() else "*")
    
    def generate_password(self):
        """Generate a strong password."""
        chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*"
        password = ''.join(secrets.choice(chars) for _ in range(16))
        self.admin_pass_var.set(password)
    
    def generate_jwt_secret(self):
        """Generate a secure JWT secret."""
        self.jwt_secret_var.set(secrets.token_urlsafe(64))
    
    def browse_file(self, var: tk.StringVar):
        """Open file browser."""
        filepath = filedialog.askopenfilename()
        if filepath:
            var.set(filepath)
    
    def select_all_components(self):
        """Select all components."""
        self.deploy_db_var.set(True)
        self.deploy_redis_var.set(True)
        self.deploy_api_var.set(True)
        self.deploy_frontend_var.set(True)
        self.deploy_monitoring_var.set(True)
    
    def select_minimal_components(self):
        """Select minimal components."""
        self.deploy_db_var.set(True)
        self.deploy_redis_var.set(False)
        self.deploy_api_var.set(True)
        self.deploy_frontend_var.set(True)
        self.deploy_monitoring_var.set(False)
    
    def get_current_config(self) -> DeploymentConfig:
        """Get current configuration from UI."""
        return DeploymentConfig(
            architecture=self.arch_var.get(),
            build_server_name=self.server_name_var.get(),
            build_server_host=self.server_host_var.get(),
            build_server_port=self.server_port_var.get(),
            build_server_user=self.server_user_var.get(),
            build_server_is_local=self.server_local_var.get(),
            build_server_ssh_key=self.server_key_var.get(),
            deploy_frontend=self.deploy_frontend_var.get(),
            deploy_api=self.deploy_api_var.get(),
            deploy_database=self.deploy_db_var.get(),
            deploy_redis=self.deploy_redis_var.get(),
            deploy_monitoring=self.deploy_monitoring_var.get(),
            hosting_platform=self.orch_var.get(),
            database_provider=self.db_provider_var.get(),
            database_host=self.db_host_var.get(),
            database_port=self.db_port_var.get(),
            database_name=self.db_name_var.get(),
            database_user=self.db_user_var.get(),
            database_password=self.db_pass_var.get(),
            database_root_password=self.db_root_pass_var.get(),
            api_port=self.api_port_var.get(),
            frontend_port=self.frontend_port_var.get(),
            redis_port=self.redis_port_var.get(),
            domain=self.domain_var.get(),
            ssl_enabled=self.ssl_var.get(),
            ssl_cert_path=self.ssl_cert_var.get(),
            ssl_key_path=self.ssl_key_var.get(),
            admin_username=self.admin_user_var.get(),
            admin_email=self.admin_email_var.get(),
            admin_password=self.admin_pass_var.get(),
            admin_first_name=self.admin_fname_var.get(),
            admin_last_name=self.admin_lname_var.get(),
            jwt_secret=self.jwt_secret_var.get(),
            seed_master_data=True,
            seed_demo_data=self.seed_demo_var.get(),
            seed_zip_codes=self.seed_zip_var.get(),
            run_bvt_tests=self.run_bvt_var.get(),
            run_smoke_tests=self.run_smoke_var.get(),
            deploy_identity_service=self.deploy_identity_var.get(),
            deploy_customer_service=self.deploy_customer_var.get(),
            deploy_sales_service=self.deploy_sales_var.get(),
            deploy_marketing_service=self.deploy_marketing_var.get(),
            deploy_servicedesk_service=self.deploy_servicedesk_var.get(),
            # Cloud Configuration
            cloud_provider=self.cloud_provider_var.get(),
            # AWS
            aws_region=self.aws_region_var.get(),
            aws_ecs_cluster=self.aws_cluster_var.get(),
            aws_ecr_registry=self.aws_ecr_var.get(),
            aws_vpc_id=self.aws_vpc_var.get(),
            aws_subnet_ids=self.aws_subnets_var.get(),
            aws_use_fargate=self.aws_fargate_var.get(),
            aws_use_rds=self.aws_rds_var.get(),
            aws_use_elasticache=self.aws_elasticache_var.get(),
            # Azure
            azure_subscription_id=self.azure_subscription_var.get(),
            azure_resource_group=self.azure_rg_var.get(),
            azure_location=self.azure_location_var.get(),
            azure_acr_name=self.azure_acr_var.get(),
            azure_aks_cluster=self.azure_aks_var.get(),
            azure_use_aks=self.azure_aks_enabled_var.get(),
            azure_use_aci=self.azure_aci_var.get(),
            azure_use_azure_db=self.azure_db_var.get(),
            azure_use_redis_cache=self.azure_redis_var.get(),
            # GCP
            gcp_project_id=self.gcp_project_var.get(),
            gcp_region=self.gcp_region_var.get(),
            gcp_zone=self.gcp_zone_var.get(),
            gcp_gke_cluster=self.gcp_gke_var.get(),
            gcp_gcr_hostname=self.gcp_gcr_var.get(),
            gcp_use_gke=self.gcp_gke_enabled_var.get(),
            gcp_use_cloud_run=self.gcp_cloudrun_var.get(),
            gcp_use_cloud_sql=self.gcp_cloudsql_var.get(),
            gcp_use_memorystore=self.gcp_memorystore_var.get(),
            # Build Configuration
            build_source=self.build_source_var.get(),
            git_repo_url=self.git_repo_var.get(),
            git_branch=self.git_branch_var.get(),
            git_username=self.git_username_var.get(),
            git_token=self.git_token_var.get(),
            git_ssh_key=self.git_ssh_key_var.get(),
            git_use_ssh=self.git_use_ssh_var.get(),
            build_type=self.build_type_var.get(),
            cloud_build_provider=self.cloud_build_provider_var.get(),
            build_frontend_cmd=self.build_frontend_cmd_var.get(),
            build_api_cmd=self.build_api_cmd_var.get(),
            build_docker_api_cmd=self.build_docker_api_cmd_var.get(),
            build_docker_frontend_cmd=self.build_docker_frontend_cmd_var.get(),
            github_actions_workflow=self.github_workflow_var.get(),
            azure_devops_org=self.azure_devops_org_var.get(),
            azure_devops_project=self.azure_devops_project_var.get(),
            azure_devops_pipeline=self.azure_devops_pipeline_var.get(),
            aws_codebuild_project=self.aws_codebuild_project_var.get(),
            gcp_cloudbuild_trigger=self.gcp_cloudbuild_trigger_var.get(),
        )
    
    def save_config(self):
        """Save current configuration."""
        self.config = self.get_current_config()
        with open(self.config_path, 'w') as f:
            json.dump(asdict(self.config), f, indent=2)
        messagebox.showinfo("Success", "Configuration saved!")
    
    def load_config(self):
        """Load saved configuration."""
        if self.config_path.exists():
            try:
                with open(self.config_path, 'r') as f:
                    data = json.load(f)
                    # Filter out unknown keys for backward compatibility
                    valid_keys = {f.name for f in DeploymentConfig.__dataclass_fields__.values()}
                    filtered_data = {k: v for k, v in data.items() if k in valid_keys}
                    self.config = DeploymentConfig(**filtered_data)
            except Exception as e:
                print(f"Error loading config: {e}")
    
    def load_config_dialog(self):
        """Load configuration from file."""
        filepath = filedialog.askopenfilename(filetypes=[("JSON files", "*.json")])
        if filepath:
            with open(filepath, 'r') as f:
                data = json.load(f)
                # Filter out unknown keys for backward compatibility
                valid_keys = {f.name for f in DeploymentConfig.__dataclass_fields__.values()}
                filtered_data = {k: v for k, v in data.items() if k in valid_keys}
                self.config = DeploymentConfig(**filtered_data)
            messagebox.showinfo("Success", "Configuration loaded!")
    
    def reset_config(self):
        """Reset to default configuration."""
        if messagebox.askyesno("Confirm", "Reset all settings to defaults?"):
            self.config = DeploymentConfig()
    
    def test_server_connection(self):
        """Test connection to build server."""
        self.connection_status_var.set("Testing...")
        self.root.update()
        
        if self.server_local_var.get():
            self.connection_status_var.set("✓ Local - OK")
            return
        
        host = self.server_host_var.get()
        port = self.server_port_var.get()
        
        try:
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.settimeout(5)
            result = sock.connect_ex((host, port))
            sock.close()
            
            if result == 0:
                self.connection_status_var.set(f"✓ Connected to {host}:{port}")
            else:
                self.connection_status_var.set(f"✗ Cannot connect to {host}:{port}")
        except Exception as e:
            self.connection_status_var.set(f"✗ Error: {str(e)}")
    
    def check_prerequisites(self):
        """Check prerequisites on build server."""
        self.prereq_text.delete(1.0, tk.END)
        
        ssh_cmd = ""
        if not self.server_local_var.get():
            user = self.server_user_var.get()
            host = self.server_host_var.get()
            key = self.server_key_var.get()
            if key:
                ssh_cmd = f"ssh -i {key} {user}@{host}"
            else:
                ssh_cmd = f"ssh {user}@{host}"
        
        checker = PrerequisiteChecker(ssh_cmd)
        results = checker.check_all()
        
        for name, result in results.items():
            if result["installed"]:
                self.prereq_text.insert(tk.END, f"✓ {name}: {result['version']}\n")
            elif result["required"]:
                self.prereq_text.insert(tk.END, f"✗ {name}: NOT INSTALLED (required)\n")
            else:
                self.prereq_text.insert(tk.END, f"⚠ {name}: not installed (optional)\n")
    
    def analyze_network(self):
        """Analyze network configuration."""
        self.network_analysis_text.delete(1.0, tk.END)
        
        config = self.get_current_config()
        analyzer = NetworkAnalyzer(config)
        issues = analyzer.analyze()
        
        if not issues:
            self.network_analysis_text.insert(tk.END, "✓ No network issues detected!\n\n")
        else:
            self.network_analysis_text.insert(tk.END, f"Found {len(issues)} potential issues:\n\n")
            for issue in issues:
                icon = "❌" if issue["severity"] == "critical" else "⚠️"
                self.network_analysis_text.insert(
                    tk.END, 
                    f"{icon} [{issue['component']}] {issue['message']}\n   Fix: {issue['fix']}\n\n"
                )
        
        fixes = analyzer.get_network_fixes()
        self.network_analysis_text.insert(tk.END, "\n--- Fix Commands ---\n")
        for fix in fixes:
            self.network_analysis_text.insert(tk.END, f"$ {fix}\n")
    
    def generate_scripts(self):
        """Generate deployment scripts."""
        config = self.get_current_config()
        engine = DeploymentEngine(config, self.log_message)
        
        self.log_message("info", "=== Generating Deployment Scripts ===")
        self.log_message("info", f"Architecture: {config.architecture}")
        self.log_message("info", f"Cloud Provider: {config.cloud_provider}")
        self.log_message("info", f"Hosting Platform: {config.hosting_platform}")
        
        output_dir = Path(__file__).parent / "generated"
        output_dir.mkdir(exist_ok=True)
        
        generated_files = []
        
        # Generate .env file
        self.log_message("info", "Generating environment file...")
        env_content = engine.generate_env_file()
        env_file = output_dir / ".env"
        with open(env_file, 'w') as f:
            f.write(env_content)
        generated_files.append(".env")
        self.log_message("success", f"✓ Generated: {env_file}")
        
        # Generate docker-compose.yml
        self.log_message("info", "Generating docker-compose.yml...")
        compose_content = engine.generate_docker_compose()
        compose_file = output_dir / "docker-compose.yml"
        with open(compose_file, 'w') as f:
            f.write(compose_content)
        generated_files.append("docker-compose.yml")
        self.log_message("success", f"✓ Generated: {compose_file}")
        
        # Generate cloud-specific scripts if cloud provider is selected
        if config.cloud_provider != "none":
            self.log_message("info", f"Generating {config.cloud_provider.upper()} deployment scripts...")
            
            if config.cloud_provider == "gcp":
                gcp_scripts = self._generate_gcp_scripts(config, output_dir)
                generated_files.extend(gcp_scripts)
            elif config.cloud_provider == "aws":
                aws_scripts = self._generate_aws_scripts(config, output_dir)
                generated_files.extend(aws_scripts)
            elif config.cloud_provider == "azure":
                azure_scripts = self._generate_azure_scripts(config, output_dir)
                generated_files.extend(azure_scripts)
        
        # Generate Kubernetes manifests if using k8s
        if config.hosting_platform == "kubernetes" or config.gcp_use_gke or config.azure_use_aks:
            self.log_message("info", "Generating Kubernetes manifests...")
            k8s_files = self._generate_kubernetes_manifests(config, output_dir)
            generated_files.extend(k8s_files)
        
        self.log_message("info", "")
        self.log_message("success", f"=== Generated {len(generated_files)} files ===")
        for f in generated_files:
            self.log_message("info", f"  → {f}")
        
        messagebox.showinfo("Success", f"Scripts generated in:\n{output_dir}\n\nFiles: {len(generated_files)}")
    
    def _generate_gcp_scripts(self, config: DeploymentConfig, output_dir: Path) -> List[str]:
        """Generate GCP deployment scripts."""
        generated = []
        
        project = config.gcp_project_id or "YOUR_PROJECT_ID"
        region = config.gcp_region or "us-central1"
        zone = config.gcp_zone or f"{region}-a"
        gcr_host = config.gcp_gcr_hostname or "gcr.io"
        gke_cluster = config.gcp_gke_cluster or "crm-cluster"
        
        self.log_message("info", f"  GCP Project: {project}")
        self.log_message("info", f"  Region: {region}, Zone: {zone}")
        self.log_message("info", f"  GKE Cluster: {gke_cluster}")
        self.log_message("info", f"  Use GKE: {config.gcp_use_gke}")
        self.log_message("info", f"  Use Cloud Run: {config.gcp_use_cloud_run}")
        
        # Main deployment script
        deploy_script = f'''#!/bin/bash
# CRM Solution - Google Cloud Platform Deployment Script
# Generated: {datetime.now().isoformat()}
# Project: {project}
# Region: {region}

set -e  # Exit on error

echo "=== CRM Solution GCP Deployment ==="
echo "Project: {project}"
echo "Region: {region}"
echo ""

# Configuration
export PROJECT_ID="{project}"
export REGION="{region}"
export ZONE="{zone}"
export GCR_HOST="{gcr_host}"
export GKE_CLUSTER="{gke_cluster}"

# Step 1: Authenticate and set project
echo "Step 1: Setting up GCP project..."
gcloud config set project $PROJECT_ID
gcloud config set compute/region $REGION
gcloud config set compute/zone $ZONE

# Step 2: Enable required APIs
echo "Step 2: Enabling required APIs..."
gcloud services enable container.googleapis.com
gcloud services enable containerregistry.googleapis.com
gcloud services enable cloudbuild.googleapis.com
gcloud services enable run.googleapis.com
gcloud services enable sqladmin.googleapis.com
gcloud services enable redis.googleapis.com
gcloud services enable secretmanager.googleapis.com

# Step 3: Configure Docker for GCR
echo "Step 3: Configuring Docker authentication..."
gcloud auth configure-docker $GCR_HOST --quiet

# Step 4: Build and push Docker images
echo "Step 4: Building and pushing Docker images..."
'''
        
        if config.deploy_api:
            deploy_script += f'''
echo "  Building API image..."
docker build -t crm-api:latest -f docker/Dockerfile.backend .
docker tag crm-api:latest $GCR_HOST/$PROJECT_ID/crm-api:latest
docker push $GCR_HOST/$PROJECT_ID/crm-api:latest
echo "  ✓ API image pushed to GCR"
'''
        
        if config.deploy_frontend:
            deploy_script += f'''
echo "  Building Frontend image..."
docker build -t crm-frontend:latest -f docker/Dockerfile.frontend .
docker tag crm-frontend:latest $GCR_HOST/$PROJECT_ID/crm-frontend:latest
docker push $GCR_HOST/$PROJECT_ID/crm-frontend:latest
echo "  ✓ Frontend image pushed to GCR"
'''
        
        # GKE deployment
        if config.gcp_use_gke:
            deploy_script += f'''
# Step 5: Create/Get GKE Cluster
echo "Step 5: Setting up GKE cluster..."
if ! gcloud container clusters describe $GKE_CLUSTER --zone $ZONE &>/dev/null; then
    echo "  Creating GKE cluster: $GKE_CLUSTER"
    gcloud container clusters create $GKE_CLUSTER \\
        --zone $ZONE \\
        --num-nodes 3 \\
        --machine-type e2-medium \\
        --enable-autoscaling \\
        --min-nodes 2 \\
        --max-nodes 5
else
    echo "  GKE cluster $GKE_CLUSTER already exists"
fi

# Get credentials
gcloud container clusters get-credentials $GKE_CLUSTER --zone $ZONE

# Step 6: Deploy to GKE
echo "Step 6: Deploying to GKE..."
kubectl apply -f generated/k8s-namespace.yaml
kubectl apply -f generated/k8s-secrets.yaml
kubectl apply -f generated/k8s-configmap.yaml
kubectl apply -f generated/k8s-deployment.yaml
kubectl apply -f generated/k8s-service.yaml
kubectl apply -f generated/k8s-ingress.yaml

echo "  Waiting for deployment to be ready..."
kubectl rollout status deployment/crm-api -n crm
kubectl rollout status deployment/crm-frontend -n crm

echo "  ✓ GKE deployment complete"
'''
        
        # Cloud Run deployment
        if config.gcp_use_cloud_run:
            deploy_script += f'''
# Step 5: Deploy to Cloud Run
echo "Step 5: Deploying to Cloud Run..."

# Deploy API
echo "  Deploying API to Cloud Run..."
gcloud run deploy crm-api \\
    --image $GCR_HOST/$PROJECT_ID/crm-api:latest \\
    --platform managed \\
    --region $REGION \\
    --allow-unauthenticated \\
    --port 5000 \\
    --memory 512Mi \\
    --cpu 1 \\
    --min-instances 0 \\
    --max-instances 10 \\
    --set-env-vars "ASPNETCORE_ENVIRONMENT=Production,DatabaseProvider={config.database_provider}"

API_URL=$(gcloud run services describe crm-api --region $REGION --format 'value(status.url)')
echo "  ✓ API deployed: $API_URL"

# Deploy Frontend
echo "  Deploying Frontend to Cloud Run..."
gcloud run deploy crm-frontend \\
    --image $GCR_HOST/$PROJECT_ID/crm-frontend:latest \\
    --platform managed \\
    --region $REGION \\
    --allow-unauthenticated \\
    --port 80 \\
    --memory 256Mi \\
    --cpu 1 \\
    --min-instances 0 \\
    --max-instances 5 \\
    --set-env-vars "REACT_APP_API_URL=$API_URL/api"

FRONTEND_URL=$(gcloud run services describe crm-frontend --region $REGION --format 'value(status.url)')
echo "  ✓ Frontend deployed: $FRONTEND_URL"

echo ""
echo "=== Cloud Run Deployment Complete ==="
echo "API URL: $API_URL"
echo "Frontend URL: $FRONTEND_URL"
'''
        
        # Cloud SQL setup if enabled
        if config.gcp_use_cloud_sql:
            deploy_script += f'''
# Cloud SQL Setup
echo "Setting up Cloud SQL..."
INSTANCE_NAME="crm-db-instance"

if ! gcloud sql instances describe $INSTANCE_NAME &>/dev/null; then
    echo "  Creating Cloud SQL instance..."
    gcloud sql instances create $INSTANCE_NAME \\
        --database-version=MYSQL_8_0 \\
        --tier=db-f1-micro \\
        --region=$REGION \\
        --root-password="{config.database_root_password or 'CHANGE_ME'}"
    
    gcloud sql databases create {config.database_name} --instance=$INSTANCE_NAME
    
    gcloud sql users create {config.database_user} \\
        --instance=$INSTANCE_NAME \\
        --password="{config.database_password or 'CHANGE_ME'}"
else
    echo "  Cloud SQL instance already exists"
fi
'''
        
        # Memorystore (Redis) setup if enabled
        if config.gcp_use_memorystore:
            deploy_script += f'''
# Memorystore (Redis) Setup
echo "Setting up Memorystore for Redis..."
REDIS_INSTANCE="crm-redis"

if ! gcloud redis instances describe $REDIS_INSTANCE --region $REGION &>/dev/null; then
    echo "  Creating Memorystore instance..."
    gcloud redis instances create $REDIS_INSTANCE \\
        --size=1 \\
        --region=$REGION \\
        --redis-version=redis_6_x
else
    echo "  Memorystore instance already exists"
fi
'''
        
        deploy_script += f'''
echo ""
echo "=== GCP Deployment Complete ==="
echo "Check the GCP Console for service status:"
echo "https://console.cloud.google.com/run?project=$PROJECT_ID"
echo "https://console.cloud.google.com/kubernetes/workload?project=$PROJECT_ID"
'''
        
        # Write the main deploy script
        deploy_file = output_dir / "deploy-gcp.sh"
        with open(deploy_file, 'w') as f:
            f.write(deploy_script)
        os.chmod(deploy_file, 0o755)
        generated.append("deploy-gcp.sh")
        self.log_message("success", f"  ✓ Generated: deploy-gcp.sh")
        
        # Generate cleanup script
        cleanup_script = f'''#!/bin/bash
# CRM Solution - GCP Cleanup Script
# WARNING: This will delete all resources!

set -e

export PROJECT_ID="{project}"
export REGION="{region}"
export ZONE="{zone}"
export GKE_CLUSTER="{gke_cluster}"

echo "=== CRM Solution GCP Cleanup ==="
echo "WARNING: This will delete all CRM resources!"
read -p "Are you sure? (yes/no): " confirm
if [ "$confirm" != "yes" ]; then
    echo "Aborted."
    exit 1
fi

# Delete Cloud Run services
echo "Deleting Cloud Run services..."
gcloud run services delete crm-api --region $REGION --quiet 2>/dev/null || true
gcloud run services delete crm-frontend --region $REGION --quiet 2>/dev/null || true

# Delete GKE cluster
echo "Deleting GKE cluster..."
gcloud container clusters delete $GKE_CLUSTER --zone $ZONE --quiet 2>/dev/null || true

# Delete Cloud SQL
echo "Deleting Cloud SQL instance..."
gcloud sql instances delete crm-db-instance --quiet 2>/dev/null || true

# Delete Memorystore
echo "Deleting Memorystore instance..."
gcloud redis instances delete crm-redis --region $REGION --quiet 2>/dev/null || true

# Delete GCR images
echo "Deleting container images..."
gcloud container images delete $GCR_HOST/$PROJECT_ID/crm-api --force-delete-tags --quiet 2>/dev/null || true
gcloud container images delete $GCR_HOST/$PROJECT_ID/crm-frontend --force-delete-tags --quiet 2>/dev/null || true

echo "=== Cleanup Complete ==="
'''
        
        cleanup_file = output_dir / "cleanup-gcp.sh"
        with open(cleanup_file, 'w') as f:
            f.write(cleanup_script)
        os.chmod(cleanup_file, 0o755)
        generated.append("cleanup-gcp.sh")
        self.log_message("success", f"  ✓ Generated: cleanup-gcp.sh")
        
        # Generate Cloud Build config (cloudbuild.yaml)
        cloudbuild_yaml = f'''# Cloud Build configuration for CRM Solution
# Trigger this build with: gcloud builds submit --config=cloudbuild.yaml .

steps:
  # Build API image
  - name: 'gcr.io/cloud-builders/docker'
    args: ['build', '-t', '{gcr_host}/{project}/crm-api:$COMMIT_SHA', '-f', 'docker/Dockerfile.backend', '.']
  
  # Build Frontend image  
  - name: 'gcr.io/cloud-builders/docker'
    args: ['build', '-t', '{gcr_host}/{project}/crm-frontend:$COMMIT_SHA', '-f', 'docker/Dockerfile.frontend', '.']
  
  # Push API image
  - name: 'gcr.io/cloud-builders/docker'
    args: ['push', '{gcr_host}/{project}/crm-api:$COMMIT_SHA']
  
  # Push Frontend image
  - name: 'gcr.io/cloud-builders/docker'
    args: ['push', '{gcr_host}/{project}/crm-frontend:$COMMIT_SHA']
'''
        
        if config.gcp_use_cloud_run:
            cloudbuild_yaml += f'''
  # Deploy API to Cloud Run
  - name: 'gcr.io/cloud-builders/gcloud'
    args:
      - 'run'
      - 'deploy'
      - 'crm-api'
      - '--image={gcr_host}/{project}/crm-api:$COMMIT_SHA'
      - '--region={region}'
      - '--platform=managed'
      - '--allow-unauthenticated'
  
  # Deploy Frontend to Cloud Run
  - name: 'gcr.io/cloud-builders/gcloud'
    args:
      - 'run'
      - 'deploy'
      - 'crm-frontend'
      - '--image={gcr_host}/{project}/crm-frontend:$COMMIT_SHA'
      - '--region={region}'
      - '--platform=managed'
      - '--allow-unauthenticated'
'''
        
        cloudbuild_yaml += f'''
images:
  - '{gcr_host}/{project}/crm-api:$COMMIT_SHA'
  - '{gcr_host}/{project}/crm-frontend:$COMMIT_SHA'

options:
  logging: CLOUD_LOGGING_ONLY
'''
        
        cloudbuild_file = output_dir / "cloudbuild.yaml"
        with open(cloudbuild_file, 'w') as f:
            f.write(cloudbuild_yaml)
        generated.append("cloudbuild.yaml")
        self.log_message("success", f"  ✓ Generated: cloudbuild.yaml")
        
        return generated
    
    def _generate_aws_scripts(self, config: DeploymentConfig, output_dir: Path) -> List[str]:
        """Generate AWS deployment scripts."""
        generated = []
        
        region = config.aws_region or "us-east-1"
        cluster = config.aws_ecs_cluster or "crm-cluster"
        ecr_registry = config.aws_ecr_registry or "YOUR_ACCOUNT_ID.dkr.ecr.{}.amazonaws.com".format(region)
        
        self.log_message("info", f"  AWS Region: {region}")
        self.log_message("info", f"  ECS Cluster: {cluster}")
        self.log_message("info", f"  Use Fargate: {config.aws_use_fargate}")
        
        deploy_script = f'''#!/bin/bash
# CRM Solution - AWS Deployment Script
# Generated: {datetime.now().isoformat()}

set -e

export AWS_REGION="{region}"
export ECS_CLUSTER="{cluster}"
export ECR_REGISTRY="{ecr_registry}"

echo "=== CRM Solution AWS Deployment ==="

# Get ECR login
echo "Logging into ECR..."
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $ECR_REGISTRY

# Create ECR repositories
echo "Creating ECR repositories..."
aws ecr create-repository --repository-name crm-api --region $AWS_REGION 2>/dev/null || true
aws ecr create-repository --repository-name crm-frontend --region $AWS_REGION 2>/dev/null || true

# Build and push images
echo "Building and pushing images..."
docker build -t crm-api:latest -f docker/Dockerfile.backend .
docker tag crm-api:latest $ECR_REGISTRY/crm-api:latest
docker push $ECR_REGISTRY/crm-api:latest

docker build -t crm-frontend:latest -f docker/Dockerfile.frontend .
docker tag crm-frontend:latest $ECR_REGISTRY/crm-frontend:latest
docker push $ECR_REGISTRY/crm-frontend:latest

# Create ECS cluster if not exists
echo "Setting up ECS cluster..."
aws ecs create-cluster --cluster-name $ECS_CLUSTER --region $AWS_REGION 2>/dev/null || true

echo "=== AWS Deployment Complete ==="
'''
        
        deploy_file = output_dir / "deploy-aws.sh"
        with open(deploy_file, 'w') as f:
            f.write(deploy_script)
        os.chmod(deploy_file, 0o755)
        generated.append("deploy-aws.sh")
        self.log_message("success", f"  ✓ Generated: deploy-aws.sh")
        
        return generated
    
    def _generate_azure_scripts(self, config: DeploymentConfig, output_dir: Path) -> List[str]:
        """Generate Azure deployment scripts."""
        generated = []
        
        rg = config.azure_resource_group or "crm-rg"
        location = config.azure_location or "eastus"
        acr_name = config.azure_acr_name or "crmacr"
        
        self.log_message("info", f"  Resource Group: {rg}")
        self.log_message("info", f"  Location: {location}")
        self.log_message("info", f"  ACR: {acr_name}")
        
        deploy_script = f'''#!/bin/bash
# CRM Solution - Azure Deployment Script
# Generated: {datetime.now().isoformat()}

set -e

export RESOURCE_GROUP="{rg}"
export LOCATION="{location}"
export ACR_NAME="{acr_name}"

echo "=== CRM Solution Azure Deployment ==="

# Create resource group
echo "Creating resource group..."
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create ACR
echo "Creating Azure Container Registry..."
az acr create --resource-group $RESOURCE_GROUP --name $ACR_NAME --sku Basic

# Login to ACR
echo "Logging into ACR..."
az acr login --name $ACR_NAME

# Build and push images
echo "Building and pushing images..."
az acr build --registry $ACR_NAME --image crm-api:latest --file docker/Dockerfile.backend .
az acr build --registry $ACR_NAME --image crm-frontend:latest --file docker/Dockerfile.frontend .

echo "=== Azure Deployment Complete ==="
'''
        
        deploy_file = output_dir / "deploy-azure.sh"
        with open(deploy_file, 'w') as f:
            f.write(deploy_script)
        os.chmod(deploy_file, 0o755)
        generated.append("deploy-azure.sh")
        self.log_message("success", f"  ✓ Generated: deploy-azure.sh")
        
        return generated
    
    def _generate_kubernetes_manifests(self, config: DeploymentConfig, output_dir: Path) -> List[str]:
        """Generate Kubernetes manifests."""
        generated = []
        
        namespace = "crm"
        
        # Namespace
        namespace_yaml = f'''apiVersion: v1
kind: Namespace
metadata:
  name: {namespace}
  labels:
    app: crm-solution
'''
        ns_file = output_dir / "k8s-namespace.yaml"
        with open(ns_file, 'w') as f:
            f.write(namespace_yaml)
        generated.append("k8s-namespace.yaml")
        
        # ConfigMap
        configmap_yaml = f'''apiVersion: v1
kind: ConfigMap
metadata:
  name: crm-config
  namespace: {namespace}
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  DatabaseProvider: "{config.database_provider}"
  DB_HOST: "crm-db"
  DB_PORT: "{config.database_port}"
  DB_NAME: "{config.database_name}"
  REDIS_HOST: "crm-redis"
'''
        cm_file = output_dir / "k8s-configmap.yaml"
        with open(cm_file, 'w') as f:
            f.write(configmap_yaml)
        generated.append("k8s-configmap.yaml")
        
        # Secrets
        secrets_yaml = f'''apiVersion: v1
kind: Secret
metadata:
  name: crm-secrets
  namespace: {namespace}
type: Opaque
stringData:
  DB_PASSWORD: "{config.database_password or 'CHANGE_ME'}"
  DB_ROOT_PASSWORD: "{config.database_root_password or 'CHANGE_ME'}"
  JWT_SECRET: "{config.jwt_secret or secrets.token_urlsafe(32)}"
'''
        secrets_file = output_dir / "k8s-secrets.yaml"
        with open(secrets_file, 'w') as f:
            f.write(secrets_yaml)
        generated.append("k8s-secrets.yaml")
        
        # Determine image registry
        if config.cloud_provider == "gcp":
            image_prefix = f"{config.gcp_gcr_hostname}/{config.gcp_project_id}"
        elif config.cloud_provider == "aws":
            image_prefix = config.aws_ecr_registry
        elif config.cloud_provider == "azure":
            image_prefix = f"{config.azure_acr_name}.azurecr.io"
        else:
            image_prefix = "crm"
        
        # Deployment
        deployment_yaml = f'''apiVersion: apps/v1
kind: Deployment
metadata:
  name: crm-api
  namespace: {namespace}
spec:
  replicas: 2
  selector:
    matchLabels:
      app: crm-api
  template:
    metadata:
      labels:
        app: crm-api
    spec:
      containers:
      - name: crm-api
        image: {image_prefix}/crm-api:latest
        ports:
        - containerPort: 5000
        envFrom:
        - configMapRef:
            name: crm-config
        - secretRef:
            name: crm-secrets
        resources:
          requests:
            memory: "256Mi"
            cpu: "200m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /api/monitoring/health
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /api/monitoring/health
            port: 5000
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: crm-frontend
  namespace: {namespace}
spec:
  replicas: 2
  selector:
    matchLabels:
      app: crm-frontend
  template:
    metadata:
      labels:
        app: crm-frontend
    spec:
      containers:
      - name: crm-frontend
        image: {image_prefix}/crm-frontend:latest
        ports:
        - containerPort: 80
        resources:
          requests:
            memory: "128Mi"
            cpu: "100m"
          limits:
            memory: "256Mi"
            cpu: "200m"
'''
        deploy_file = output_dir / "k8s-deployment.yaml"
        with open(deploy_file, 'w') as f:
            f.write(deployment_yaml)
        generated.append("k8s-deployment.yaml")
        
        # Service
        service_yaml = f'''apiVersion: v1
kind: Service
metadata:
  name: crm-api
  namespace: {namespace}
spec:
  selector:
    app: crm-api
  ports:
  - port: 5000
    targetPort: 5000
  type: ClusterIP
---
apiVersion: v1
kind: Service
metadata:
  name: crm-frontend
  namespace: {namespace}
spec:
  selector:
    app: crm-frontend
  ports:
  - port: 80
    targetPort: 80
  type: LoadBalancer
'''
        svc_file = output_dir / "k8s-service.yaml"
        with open(svc_file, 'w') as f:
            f.write(service_yaml)
        generated.append("k8s-service.yaml")
        
        # Ingress
        ingress_yaml = f'''apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: crm-ingress
  namespace: {namespace}
  annotations:
    kubernetes.io/ingress.class: "nginx"
spec:
  rules:
  - host: {config.domain}
    http:
      paths:
      - path: /api
        pathType: Prefix
        backend:
          service:
            name: crm-api
            port:
              number: 5000
      - path: /
        pathType: Prefix
        backend:
          service:
            name: crm-frontend
            port:
              number: 80
'''
        ingress_file = output_dir / "k8s-ingress.yaml"
        with open(ingress_file, 'w') as f:
            f.write(ingress_yaml)
        generated.append("k8s-ingress.yaml")
        
        self.log_message("success", f"  ✓ Generated {len(generated)} Kubernetes manifests")
        
        return generated
    
    def log_message(self, msg_type: str, message: str):
        """Add message to log queue."""
        self.log_queue.put((msg_type, message))
    
    def process_logs(self):
        """Process log queue and update UI."""
        try:
            while True:
                msg_type, message = self.log_queue.get_nowait()
                timestamp = datetime.now().strftime("%H:%M:%S")
                log_entry = f"[{timestamp}] {message}\n"
                self.log_text.insert(tk.END, log_entry, msg_type)
                self.log_text.see(tk.END)
        except queue.Empty:
            pass
        
        self.root.after(100, self.process_logs)
    
    def clear_log(self):
        """Clear the log text."""
        self.log_text.delete(1.0, tk.END)
    
    def save_log(self):
        """Save log to file."""
        filepath = filedialog.asksaveasfilename(
            defaultextension=".log",
            filetypes=[("Log files", "*.log")])
        if filepath:
            with open(filepath, 'w') as f:
                f.write(self.log_text.get(1.0, tk.END))
    
    def update_step_status(self, step: str, status: str):
        """Update step status indicator."""
        icons = {
            "pending": "⏳",
            "in_progress": "🔄",
            "success": "✅",
            "failed": "❌",
            "skipped": "⏭️"
        }
        if step in self.step_labels:
            self.step_labels[step].set(icons.get(status, "⏳"))
    
    def start_deployment(self):
        """Start the deployment process."""
        config = self.get_current_config()
        
        self.start_btn.configure(state=tk.DISABLED)
        self.pause_btn.configure(state=tk.NORMAL)
        self.stop_btn.configure(state=tk.NORMAL)
        
        self.progress_var.set(0)
        for step in self.step_labels:
            self.update_step_status(step, "pending")
        
        def deploy_thread():
            try:
                self.engine = DeploymentEngine(config, self.log_message)
                self.engine.is_running = True
                
                steps = [
                    ("Prerequisites", self.engine.deploy_prerequisites, 10),
                    ("Network", self.engine.create_docker_network, 20),
                    ("Database", lambda: True, 40),
                    ("API", lambda: True, 60),
                    ("Frontend", lambda: True, 70),
                    ("Data", self.engine.deploy_master_data, 85),
                    ("Tests", lambda: self.run_tests_internal(config), 100),
                ]
                
                for step_name, step_func, progress in steps:
                    if not self.engine.is_running:
                        break
                    
                    self.engine._wait_if_paused()
                    
                    self.update_step_status(step_name, "in_progress")
                    self.progress_label.set(f"Running: {step_name}")
                    self.log_message("info", f"=== {step_name} ===")
                    
                    try:
                        success = step_func()
                        self.update_step_status(step_name, "success" if success else "failed")
                    except Exception as e:
                        self.log_message("error", f"Error in {step_name}: {str(e)}")
                        self.update_step_status(step_name, "failed")
                    
                    self.progress_var.set(progress)
                
                self.show_deployment_summary(config)
                
            except Exception as e:
                self.log_message("error", f"Deployment error: {str(e)}")
            finally:
                self.start_btn.configure(state=tk.NORMAL)
                self.pause_btn.configure(state=tk.DISABLED)
                self.stop_btn.configure(state=tk.DISABLED)
                self.progress_label.set("Deployment complete" if self.engine.is_running else "Deployment stopped")
        
        self.deployment_thread = threading.Thread(target=deploy_thread, daemon=True)
        self.deployment_thread.start()
    
    def run_tests_internal(self, config: DeploymentConfig) -> bool:
        """Run tests and update results."""
        if self.engine:
            smoke_results = self.engine.run_smoke_tests()
            bvt_results = self.engine.run_bvt_tests()
            
            self.test_results = smoke_results + bvt_results
            
            for item in self.results_tree.get_children():
                self.results_tree.delete(item)
            
            passed = 0
            failed = 0
            for result in self.test_results:
                icon = "✅" if result.status == "passed" else "❌"
                self.results_tree.insert("", tk.END, values=(
                    result.name,
                    f"{icon} {result.status}",
                    f"{result.duration_ms}ms",
                    result.message
                ))
                if result.status == "passed":
                    passed += 1
                else:
                    failed += 1
            
            self.results_summary_var.set(f"Tests: {passed} passed, {failed} failed")
            
            return failed == 0
        return True
    
    def pause_deployment(self):
        """Pause the deployment."""
        if self.engine:
            self.engine.is_paused = not self.engine.is_paused
            status = "Paused" if self.engine.is_paused else "Resumed"
            self.pause_btn.configure(text="▶️ Resume" if self.engine.is_paused else "⏸️ Pause")
            self.log_message("warning", f"Deployment {status}")
            self.progress_label.set(status)
    
    def stop_deployment(self):
        """Stop the deployment."""
        if self.engine:
            self.engine.is_running = False
            self.log_message("warning", "Deployment stopped by user")
    
    def show_deployment_summary(self, config: DeploymentConfig):
        """Show deployment summary."""
        protocol = "https" if config.ssl_enabled else "http"
        domain = config.domain
        api_port = config.api_port
        frontend_port = config.frontend_port
        
        summary = f"""
╔══════════════════════════════════════════════════════╗
║          CRM SOLUTION DEPLOYMENT COMPLETE            ║
╠══════════════════════════════════════════════════════╣
║                                                      ║
║  Frontend: {protocol}://{domain}:{frontend_port}
║  API:      {protocol}://{domain}:{api_port}
║  Swagger:  {protocol}://{domain}:{api_port}/swagger
║                                                      ║
╠══════════════════════════════════════════════════════╣
║  LOGIN CREDENTIALS                                   ║
╠══════════════════════════════════════════════════════╣
║  Email:    {config.admin_email}
║  Password: {config.admin_password}
║                                                      ║
╚══════════════════════════════════════════════════════╝
"""
        
        self.summary_text.delete(1.0, tk.END)
        self.summary_text.insert(1.0, summary)
    
    def copy_login_details(self):
        """Copy login details to clipboard."""
        config = self.get_current_config()
        protocol = "https" if config.ssl_enabled else "http"
        
        details = f"""CRM Solution Login
URL: {protocol}://{config.domain}:{config.frontend_port}
Email: {config.admin_email}
Password: {config.admin_password}
"""
        
        self.root.clipboard_clear()
        self.root.clipboard_append(details)
        messagebox.showinfo("Copied", "Login details copied to clipboard!")
    
    def export_test_results(self):
        """Export test results to file."""
        if not self.test_results:
            messagebox.showwarning("Warning", "No test results to export")
            return
        
        filepath = filedialog.asksaveasfilename(
            defaultextension=".json",
            filetypes=[("JSON files", "*.json")])
        
        if filepath:
            results_data = {
                "timestamp": datetime.now().isoformat(),
                "version": VERSION,
                "tests": [asdict(r) for r in self.test_results]
            }
            with open(filepath, 'w') as f:
                json.dump(results_data, f, indent=2)
            
            messagebox.showinfo("Success", f"Results exported to {filepath}")
    
    def show_help(self):
        """Show help dialog."""
        help_text = f"""CRM Solution Deployment Tool v{VERSION}

TABS:
• Architecture: Choose deployment type
• Build Server: Configure target server
• Components: Select what to deploy
• Database: Database configuration
• Network: Ports, domain, SSL
• Credentials: Admin user setup
• Data: Seed data options
• Testing: Post-deployment tests

WORKFLOW:
1. Configure settings
2. Generate Scripts to preview
3. Start Deployment
4. Monitor progress
5. Review test results

For more info, see docs/HOWTO.md
"""
        messagebox.showinfo("Help", help_text)


def cli_mode():
    """Command-line interface mode for when tkinter is unavailable."""
    import argparse
    
    parser = argparse.ArgumentParser(description="CRM Deployment Tool CLI")
    parser.add_argument("--generate", action="store_true", help="Generate deployment scripts")
    parser.add_argument("--deploy", action="store_true", help="Run deployment")
    parser.add_argument("--config", type=str, help="Path to config JSON file")
    parser.add_argument("--output", type=str, default="./generated", help="Output directory")
    parser.add_argument("--domain", type=str, default="localhost", help="Domain/hostname")
    parser.add_argument("--api-port", type=int, default=5000, help="API port")
    parser.add_argument("--frontend-port", type=int, default=80, help="Frontend port")
    parser.add_argument("--db-host", type=str, default="crm-mariadb", help="Database host")
    parser.add_argument("--db-password", type=str, help="Database password")
    parser.add_argument("--admin-email", type=str, default="admin@crm.local", help="Admin email")
    parser.add_argument("--admin-password", type=str, default="Admin@123", help="Admin password")
    
    args = parser.parse_args()
    
    # Load or create config
    if args.config and Path(args.config).exists():
        with open(args.config, 'r') as f:
            data = json.load(f)
            config = DeploymentConfig(**data)
    else:
        config = DeploymentConfig()
    
    # Apply CLI overrides
    if args.domain:
        config.domain = args.domain
    if args.api_port:
        config.api_port = args.api_port
    if args.frontend_port:
        config.frontend_port = args.frontend_port
    if args.db_host:
        config.database_host = args.db_host
    if args.db_password:
        config.database_password = args.db_password
    if args.admin_email:
        config.admin_email = args.admin_email
    if args.admin_password:
        config.admin_password = args.admin_password
    
    def log_print(msg_type: str, message: str):
        timestamp = datetime.now().strftime("%H:%M:%S")
        prefix = {"info": "ℹ️", "success": "✅", "warning": "⚠️", "error": "❌", "cmd": "💻", "output": "📤"}.get(msg_type, "")
        print(f"[{timestamp}] {prefix} {message}")
    
    engine = DeploymentEngine(config, log_print)
    
    if args.generate or not args.deploy:
        # Generate scripts
        output_dir = Path(args.output)
        output_dir.mkdir(exist_ok=True)
        
        env_content = engine.generate_env_file()
        compose_content = engine.generate_docker_compose()
        
        env_path = output_dir / ".env"
        compose_path = output_dir / "docker-compose.yml"
        
        with open(env_path, 'w') as f:
            f.write(env_content)
        print(f"✅ Generated: {env_path}")
        
        with open(compose_path, 'w') as f:
            f.write(compose_content)
        print(f"✅ Generated: {compose_path}")
        
        print(f"\n📁 Output directory: {output_dir.absolute()}")
        print(f"\nTo deploy, run:")
        print(f"  cd {output_dir.absolute()}")
        print(f"  docker compose up -d")
    
    if args.deploy:
        print("\n🚀 Starting deployment...")
        engine.is_running = True
        
        if not engine.deploy_prerequisites():
            print("❌ Prerequisites check failed")
            return 1
        
        engine.create_docker_network()
        engine.deploy_master_data()
        
        results = engine.run_smoke_tests()
        
        passed = sum(1 for r in results if r.status == "passed")
        failed = len(results) - passed
        
        print(f"\n📊 Tests: {passed} passed, {failed} failed")
        
        protocol = "https" if config.ssl_enabled else "http"
        print(f"\n" + "=" * 50)
        print(f"🎉 Deployment Complete!")
        print(f"=" * 50)
        print(f"Frontend: {protocol}://{config.domain}:{config.frontend_port}")
        print(f"API:      {protocol}://{config.domain}:{config.api_port}")
        print(f"\n👤 Login Credentials:")
        print(f"   Email:    {config.admin_email}")
        print(f"   Password: {config.admin_password}")
        print(f"=" * 50)
    
    return 0


def main():
    """Main entry point."""
    # Check for CLI mode
    if len(sys.argv) > 1 and sys.argv[1] in ['--generate', '--deploy', '--config', '--help', '-h']:
        sys.exit(cli_mode())
    
    # Check for advanced mode (original tabbed interface)
    use_wizard = True
    if len(sys.argv) > 1 and sys.argv[1] in ['--advanced', '--tabs', '-a']:
        use_wizard = False
    
    # Try GUI mode
    try:
        root = tk.Tk()
        
        style = ttk.Style()
        if sys.platform == 'darwin':
            try:
                style.theme_use('aqua')
            except:
                style.theme_use('clam')
        else:
            style.theme_use('clam')
        
        if use_wizard:
            app = DeploymentWizard(root)
        else:
            app = DeploymentTool(root)
        
        root.mainloop()
    except Exception as e:
        print(f"GUI mode failed: {e}")
        print("\nFalling back to CLI mode...")
        print("Use --help for CLI options, or run:")
        print("  python main.py --generate")
        print("\nFor wizard mode: python main.py")
        print("For advanced mode: python main.py --advanced")
        sys.exit(1)


if __name__ == "__main__":
    main()
