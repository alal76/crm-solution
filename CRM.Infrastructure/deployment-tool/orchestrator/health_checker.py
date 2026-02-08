#!/usr/bin/env python3
"""
CRM Solution - Health Checker Service
Verifies deployment health and service availability.

Features:
- HTTP endpoint health checks
- Database connectivity tests
- Container/service status verification
- Provider service checks
- Custom health check plugins

Author: Abhishek Lal
License: AGPL-3.0
"""

import os
import sys
import json
import socket
import logging
import time
import urllib.request
import urllib.error
from typing import Dict, List, Optional, Any, Callable
from datetime import datetime
from dataclasses import dataclass, field
from enum import Enum
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor, as_completed


class HealthStatus(Enum):
    """Health check status values."""
    HEALTHY = "healthy"
    UNHEALTHY = "unhealthy"
    DEGRADED = "degraded"
    UNKNOWN = "unknown"
    TIMEOUT = "timeout"


class CheckType(Enum):
    """Types of health checks."""
    HTTP = "http"
    TCP = "tcp"
    DATABASE = "database"
    REDIS = "redis"
    CONTAINER = "container"
    DNS = "dns"
    CERTIFICATE = "certificate"
    CUSTOM = "custom"


@dataclass
class HealthCheckResult:
    """Result of a single health check."""
    name: str
    check_type: CheckType
    status: HealthStatus
    response_time_ms: float = 0
    message: str = ""
    details: Dict[str, Any] = field(default_factory=dict)
    checked_at: datetime = field(default_factory=datetime.now)


@dataclass
class HealthCheck:
    """Definition of a health check."""
    name: str
    check_type: CheckType
    endpoint: str
    timeout_seconds: int = 30
    expected_status: int = 200
    expected_content: Optional[str] = None
    headers: Dict[str, str] = field(default_factory=dict)
    critical: bool = True
    retry_count: int = 3
    retry_delay_seconds: int = 2


@dataclass 
class HealthReport:
    """Complete health report for a deployment."""
    deployment_name: str
    timestamp: datetime
    overall_status: HealthStatus
    checks: List[HealthCheckResult] = field(default_factory=list)
    healthy_count: int = 0
    unhealthy_count: int = 0
    degraded_count: int = 0
    total_duration_ms: float = 0


class HealthChecker:
    """
    Service for performing health checks on deployed services.
    
    Features:
    - Parallel health check execution
    - Multiple check types (HTTP, TCP, database, etc.)
    - Retry logic with configurable delays
    - Detailed reporting
    """
    
    def __init__(
        self,
        deployment_name: str,
        log_dir: str = "./logs"
    ):
        self.deployment_name = deployment_name
        self.log_dir = Path(log_dir)
        self.log_dir.mkdir(parents=True, exist_ok=True)
        
        # Setup logging
        self.logger = logging.getLogger(f"health.{deployment_name}")
        self.logger.setLevel(logging.DEBUG)
        
        # Health checks registry
        self.checks: List[HealthCheck] = []
        self.custom_checkers: Dict[str, Callable] = {}
        
        # Add default CRM health checks
        self._register_default_checks()
    
    def _register_default_checks(self):
        """Register default health checks for CRM services."""
        
        # API Gateway
        self.add_check(HealthCheck(
            name="api_gateway",
            check_type=CheckType.HTTP,
            endpoint="http://localhost:5000/health",
            timeout_seconds=10,
            critical=True
        ))
        
        # Identity Service
        self.add_check(HealthCheck(
            name="identity_service",
            check_type=CheckType.HTTP,
            endpoint="http://localhost:5001/health",
            timeout_seconds=10,
            critical=True
        ))
        
        # Customer Service
        self.add_check(HealthCheck(
            name="customer_service",
            check_type=CheckType.HTTP,
            endpoint="http://localhost:5002/health",
            timeout_seconds=10,
            critical=True
        ))
        
        # Sales Service
        self.add_check(HealthCheck(
            name="sales_service",
            check_type=CheckType.HTTP,
            endpoint="http://localhost:5003/health",
            timeout_seconds=10,
            critical=True
        ))
        
        # Marketing Service
        self.add_check(HealthCheck(
            name="marketing_service",
            check_type=CheckType.HTTP,
            endpoint="http://localhost:5004/health",
            timeout_seconds=10,
            critical=False
        ))
        
        # Service Desk
        self.add_check(HealthCheck(
            name="servicedesk_service",
            check_type=CheckType.HTTP,
            endpoint="http://localhost:5005/health",
            timeout_seconds=10,
            critical=False
        ))
        
        # Frontend
        self.add_check(HealthCheck(
            name="frontend",
            check_type=CheckType.HTTP,
            endpoint="http://localhost:3000",
            timeout_seconds=10,
            expected_status=200,
            critical=True
        ))
        
        # Database (TCP check)
        self.add_check(HealthCheck(
            name="database_mariadb",
            check_type=CheckType.TCP,
            endpoint="localhost:3306",
            timeout_seconds=5,
            critical=True
        ))
        
        # Redis (TCP check)
        self.add_check(HealthCheck(
            name="redis_cache",
            check_type=CheckType.TCP,
            endpoint="localhost:6379",
            timeout_seconds=5,
            critical=False
        ))
    
    def add_check(self, check: HealthCheck):
        """Add a health check to the registry."""
        self.checks.append(check)
        self.logger.debug(f"Registered health check: {check.name}")
    
    def add_custom_checker(self, name: str, checker: Callable):
        """
        Add a custom health check function.
        
        Args:
            name: Name of the custom checker
            checker: Function that returns HealthCheckResult
        """
        self.custom_checkers[name] = checker
    
    def add_provider_checks(self, providers: Dict[str, str]):
        """
        Add health checks for external providers.
        
        Args:
            providers: Dict of provider_type -> provider_name
        """
        # Search provider
        if providers.get("search") == "meilisearch":
            self.add_check(HealthCheck(
                name="meilisearch",
                check_type=CheckType.HTTP,
                endpoint="http://localhost:7700/health",
                timeout_seconds=10,
                critical=False
            ))
        elif providers.get("search") == "algolia":
            # Algolia is a SaaS, different check
            pass
        
        # Chat provider
        if providers.get("chat") == "chatwoot":
            self.add_check(HealthCheck(
                name="chatwoot",
                check_type=CheckType.HTTP,
                endpoint="http://localhost:3000/api",
                timeout_seconds=10,
                critical=False
            ))
        
        # Analytics provider
        if providers.get("analytics") == "superset":
            self.add_check(HealthCheck(
                name="superset",
                check_type=CheckType.HTTP,
                endpoint="http://localhost:8088/health",
                timeout_seconds=15,
                critical=False
            ))
        elif providers.get("analytics") == "metabase":
            self.add_check(HealthCheck(
                name="metabase",
                check_type=CheckType.HTTP,
                endpoint="http://localhost:3000/api/health",
                timeout_seconds=15,
                critical=False
            ))
        
        # Signature provider
        if providers.get("signatures") == "docuseal":
            self.add_check(HealthCheck(
                name="docuseal",
                check_type=CheckType.HTTP,
                endpoint="http://localhost:3000",
                timeout_seconds=10,
                critical=False
            ))
        
        # Integration provider
        if providers.get("integrations") == "n8n":
            self.add_check(HealthCheck(
                name="n8n",
                check_type=CheckType.HTTP,
                endpoint="http://localhost:5678/healthz",
                timeout_seconds=10,
                critical=False
            ))
    
    def run_checks(
        self,
        parallel: bool = True,
        max_workers: int = 10
    ) -> HealthReport:
        """
        Run all registered health checks.
        
        Args:
            parallel: Run checks in parallel
            max_workers: Maximum parallel workers
        
        Returns:
            HealthReport with all results
        """
        start_time = datetime.now()
        results: List[HealthCheckResult] = []
        
        self.logger.info("")
        self.logger.info("=" * 50)
        self.logger.info("HEALTH CHECK")
        self.logger.info("=" * 50)
        self.logger.info(f"Running {len(self.checks)} health checks...")
        
        if parallel:
            results = self._run_parallel(max_workers)
        else:
            results = self._run_sequential()
        
        # Calculate overall status
        healthy = sum(1 for r in results if r.status == HealthStatus.HEALTHY)
        unhealthy = sum(1 for r in results if r.status == HealthStatus.UNHEALTHY)
        degraded = sum(1 for r in results if r.status == HealthStatus.DEGRADED)
        
        # Critical checks determine overall status
        critical_checks = [c for c in self.checks if c.critical]
        critical_results = [r for r in results if any(c.name == r.name for c in critical_checks)]
        critical_unhealthy = sum(1 for r in critical_results if r.status == HealthStatus.UNHEALTHY)
        
        if critical_unhealthy > 0:
            overall = HealthStatus.UNHEALTHY
        elif unhealthy > 0 or degraded > 0:
            overall = HealthStatus.DEGRADED
        else:
            overall = HealthStatus.HEALTHY
        
        end_time = datetime.now()
        duration_ms = (end_time - start_time).total_seconds() * 1000
        
        report = HealthReport(
            deployment_name=self.deployment_name,
            timestamp=start_time,
            overall_status=overall,
            checks=results,
            healthy_count=healthy,
            unhealthy_count=unhealthy,
            degraded_count=degraded,
            total_duration_ms=duration_ms
        )
        
        self._print_report(report)
        
        return report
    
    def _run_parallel(self, max_workers: int) -> List[HealthCheckResult]:
        """Run health checks in parallel."""
        results = []
        
        with ThreadPoolExecutor(max_workers=max_workers) as executor:
            futures = {
                executor.submit(self._perform_check, check): check
                for check in self.checks
            }
            
            for future in as_completed(futures):
                check = futures[future]
                try:
                    result = future.result()
                    results.append(result)
                except Exception as e:
                    results.append(HealthCheckResult(
                        name=check.name,
                        check_type=check.check_type,
                        status=HealthStatus.UNHEALTHY,
                        message=f"Exception: {str(e)}"
                    ))
        
        return results
    
    def _run_sequential(self) -> List[HealthCheckResult]:
        """Run health checks sequentially."""
        results = []
        
        for check in self.checks:
            result = self._perform_check(check)
            results.append(result)
        
        return results
    
    def _perform_check(self, check: HealthCheck) -> HealthCheckResult:
        """
        Perform a single health check with retries.
        
        Args:
            check: Health check to perform
        
        Returns:
            HealthCheckResult
        """
        last_result = None
        
        for attempt in range(check.retry_count):
            start_time = datetime.now()
            
            try:
                if check.check_type == CheckType.HTTP:
                    result = self._check_http(check)
                elif check.check_type == CheckType.TCP:
                    result = self._check_tcp(check)
                elif check.check_type == CheckType.DATABASE:
                    result = self._check_database(check)
                elif check.check_type == CheckType.REDIS:
                    result = self._check_redis(check)
                elif check.check_type == CheckType.DNS:
                    result = self._check_dns(check)
                elif check.check_type == CheckType.CUSTOM:
                    if check.name in self.custom_checkers:
                        result = self.custom_checkers[check.name](check)
                    else:
                        result = HealthCheckResult(
                            name=check.name,
                            check_type=check.check_type,
                            status=HealthStatus.UNKNOWN,
                            message="No custom checker registered"
                        )
                else:
                    result = HealthCheckResult(
                        name=check.name,
                        check_type=check.check_type,
                        status=HealthStatus.UNKNOWN,
                        message=f"Unknown check type: {check.check_type}"
                    )
                
                end_time = datetime.now()
                result.response_time_ms = (end_time - start_time).total_seconds() * 1000
                result.checked_at = start_time
                
                if result.status == HealthStatus.HEALTHY:
                    return result
                
                last_result = result
                
                if attempt < check.retry_count - 1:
                    import time
                    time.sleep(check.retry_delay_seconds)
                    
            except Exception as e:
                last_result = HealthCheckResult(
                    name=check.name,
                    check_type=check.check_type,
                    status=HealthStatus.UNHEALTHY,
                    message=f"Exception: {str(e)}"
                )
        
        return last_result or HealthCheckResult(
            name=check.name,
            check_type=check.check_type,
            status=HealthStatus.UNKNOWN,
            message="No result"
        )
    
    def _check_http(self, check: HealthCheck) -> HealthCheckResult:
        """Perform HTTP health check."""
        import urllib.request
        import urllib.error
        
        try:
            req = urllib.request.Request(check.endpoint, headers=check.headers)
            
            with urllib.request.urlopen(req, timeout=check.timeout_seconds) as response:
                status_code = response.status
                content = response.read().decode('utf-8')
                
                if status_code != check.expected_status:
                    return HealthCheckResult(
                        name=check.name,
                        check_type=check.check_type,
                        status=HealthStatus.UNHEALTHY,
                        message=f"Expected status {check.expected_status}, got {status_code}",
                        details={"status_code": status_code}
                    )
                
                if check.expected_content and check.expected_content not in content:
                    return HealthCheckResult(
                        name=check.name,
                        check_type=check.check_type,
                        status=HealthStatus.DEGRADED,
                        message=f"Expected content not found",
                        details={"status_code": status_code}
                    )
                
                return HealthCheckResult(
                    name=check.name,
                    check_type=check.check_type,
                    status=HealthStatus.HEALTHY,
                    message="OK",
                    details={"status_code": status_code}
                )
                
        except urllib.error.HTTPError as e:
            return HealthCheckResult(
                name=check.name,
                check_type=check.check_type,
                status=HealthStatus.UNHEALTHY,
                message=f"HTTP Error: {e.code} {e.reason}",
                details={"status_code": e.code}
            )
        except urllib.error.URLError as e:
            return HealthCheckResult(
                name=check.name,
                check_type=check.check_type,
                status=HealthStatus.UNHEALTHY,
                message=f"Connection failed: {str(e.reason)}"
            )
        except Exception as e:
            return HealthCheckResult(
                name=check.name,
                check_type=check.check_type,
                status=HealthStatus.UNHEALTHY,
                message=f"Error: {str(e)}"
            )
    
    def _check_tcp(self, check: HealthCheck) -> HealthCheckResult:
        """Perform TCP port health check."""
        try:
            # Parse host:port
            parts = check.endpoint.split(':')
            host = parts[0]
            port = int(parts[1]) if len(parts) > 1 else 80
            
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.settimeout(check.timeout_seconds)
            
            result = sock.connect_ex((host, port))
            sock.close()
            
            if result == 0:
                return HealthCheckResult(
                    name=check.name,
                    check_type=check.check_type,
                    status=HealthStatus.HEALTHY,
                    message=f"Port {port} is open",
                    details={"host": host, "port": port}
                )
            else:
                return HealthCheckResult(
                    name=check.name,
                    check_type=check.check_type,
                    status=HealthStatus.UNHEALTHY,
                    message=f"Port {port} is closed or unreachable",
                    details={"host": host, "port": port, "error_code": result}
                )
                
        except socket.timeout:
            return HealthCheckResult(
                name=check.name,
                check_type=check.check_type,
                status=HealthStatus.TIMEOUT,
                message="Connection timed out"
            )
        except Exception as e:
            return HealthCheckResult(
                name=check.name,
                check_type=check.check_type,
                status=HealthStatus.UNHEALTHY,
                message=f"Error: {str(e)}"
            )
    
    def _check_database(self, check: HealthCheck) -> HealthCheckResult:
        """Perform database connectivity check."""
        # Would use appropriate database driver
        # For now, fall back to TCP check
        return self._check_tcp(check)
    
    def _check_redis(self, check: HealthCheck) -> HealthCheckResult:
        """Perform Redis connectivity check."""
        try:
            # Parse host:port
            parts = check.endpoint.split(':')
            host = parts[0]
            port = int(parts[1]) if len(parts) > 1 else 6379
            
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.settimeout(check.timeout_seconds)
            sock.connect((host, port))
            
            # Send PING command
            sock.send(b"*1\r\n$4\r\nPING\r\n")
            response = sock.recv(1024).decode()
            sock.close()
            
            if "+PONG" in response:
                return HealthCheckResult(
                    name=check.name,
                    check_type=check.check_type,
                    status=HealthStatus.HEALTHY,
                    message="Redis PONG received"
                )
            else:
                return HealthCheckResult(
                    name=check.name,
                    check_type=check.check_type,
                    status=HealthStatus.DEGRADED,
                    message=f"Unexpected response: {response}"
                )
                
        except Exception as e:
            return HealthCheckResult(
                name=check.name,
                check_type=check.check_type,
                status=HealthStatus.UNHEALTHY,
                message=f"Error: {str(e)}"
            )
    
    def _check_dns(self, check: HealthCheck) -> HealthCheckResult:
        """Perform DNS resolution check."""
        try:
            hostname = check.endpoint.replace("http://", "").replace("https://", "").split('/')[0].split(':')[0]
            
            ip_address = socket.gethostbyname(hostname)
            
            return HealthCheckResult(
                name=check.name,
                check_type=check.check_type,
                status=HealthStatus.HEALTHY,
                message=f"Resolved to {ip_address}",
                details={"hostname": hostname, "ip": ip_address}
            )
            
        except socket.gaierror as e:
            return HealthCheckResult(
                name=check.name,
                check_type=check.check_type,
                status=HealthStatus.UNHEALTHY,
                message=f"DNS resolution failed: {str(e)}"
            )
    
    def _print_report(self, report: HealthReport):
        """Print the health report."""
        self.logger.info("")
        self.logger.info("RESULTS:")
        
        # Print each check result
        for result in report.checks:
            status_icon = {
                HealthStatus.HEALTHY: "✓",
                HealthStatus.UNHEALTHY: "✗",
                HealthStatus.DEGRADED: "⚠",
                HealthStatus.TIMEOUT: "⏱",
                HealthStatus.UNKNOWN: "?"
            }.get(result.status, "?")
            
            status_color = {
                HealthStatus.HEALTHY: "",
                HealthStatus.UNHEALTHY: "",
                HealthStatus.DEGRADED: "",
            }.get(result.status, "")
            
            self.logger.info(
                f"  {status_icon} {result.name:25} "
                f"{result.status.value:12} "
                f"{result.response_time_ms:6.0f}ms  "
                f"{result.message}"
            )
        
        self.logger.info("")
        self.logger.info(f"Summary: {report.healthy_count} healthy, "
                        f"{report.unhealthy_count} unhealthy, "
                        f"{report.degraded_count} degraded")
        self.logger.info(f"Overall Status: {report.overall_status.value.upper()}")
        self.logger.info(f"Total Duration: {report.total_duration_ms:.0f}ms")
        self.logger.info("=" * 50)
    
    def export_report(self, report: HealthReport, output_path: str):
        """Export health report to JSON file."""
        data = {
            "deployment_name": report.deployment_name,
            "timestamp": report.timestamp.isoformat(),
            "overall_status": report.overall_status.value,
            "healthy_count": report.healthy_count,
            "unhealthy_count": report.unhealthy_count,
            "degraded_count": report.degraded_count,
            "total_duration_ms": report.total_duration_ms,
            "checks": [
                {
                    "name": r.name,
                    "check_type": r.check_type.value,
                    "status": r.status.value,
                    "response_time_ms": r.response_time_ms,
                    "message": r.message,
                    "details": r.details,
                    "checked_at": r.checked_at.isoformat()
                }
                for r in report.checks
            ]
        }
        
        with open(output_path, 'w') as f:
            json.dump(data, f, indent=2)
        
        self.logger.info(f"Health report exported to {output_path}")
