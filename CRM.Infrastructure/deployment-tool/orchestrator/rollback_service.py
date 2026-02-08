#!/usr/bin/env python3
"""
CRM Solution - Rollback Service
Handles rollback operations during failed deployments.

Features:
- State snapshots
- Automatic rollback on failure
- Manual rollback commands
- Partial rollback support

Author: Abhishek Lal
License: AGPL-3.0
"""

import os
import json
import shutil
import logging
from typing import Dict, List, Optional, Any
from datetime import datetime
from pathlib import Path
from dataclasses import dataclass, field, asdict
from enum import Enum


class RollbackType(Enum):
    """Types of rollback operations."""
    FULL = "full"
    PARTIAL = "partial"
    INFRASTRUCTURE = "infrastructure"
    APPLICATION = "application"
    DATABASE = "database"


class ResourceType(Enum):
    """Types of resources that can be rolled back."""
    RESOURCE_GROUP = "resource_group"
    KUBERNETES_CLUSTER = "kubernetes_cluster"
    DATABASE = "database"
    CACHE = "cache"
    STORAGE = "storage"
    CONTAINER = "container"
    NETWORK = "network"
    SSL_CERTIFICATE = "ssl_certificate"
    CONFIGURATION = "configuration"
    FILE = "file"
    CUSTOM = "custom"


@dataclass
class ResourceState:
    """State of a deployed resource."""
    resource_id: str
    resource_type: ResourceType
    name: str
    platform: str
    created_at: datetime
    properties: Dict[str, Any] = field(default_factory=dict)
    rollback_command: Optional[str] = None
    dependencies: List[str] = field(default_factory=list)
    can_rollback: bool = True


@dataclass
class DeploymentSnapshot:
    """Snapshot of deployment state at a point in time."""
    snapshot_id: str
    deployment_name: str
    created_at: datetime
    phase: str
    resources: List[ResourceState] = field(default_factory=list)
    configurations: Dict[str, Any] = field(default_factory=dict)
    database_state: Optional[Dict[str, Any]] = None
    notes: str = ""


class RollbackService:
    """
    Service for managing deployment rollbacks.
    
    Features:
    - Create snapshots before deployment steps
    - Rollback to previous snapshots
    - Selective resource rollback
    - Dependency-aware rollback ordering
    """
    
    def __init__(
        self,
        deployment_name: str,
        snapshot_dir: str = "./snapshots",
        log_dir: str = "./logs"
    ):
        self.deployment_name = deployment_name
        self.snapshot_dir = Path(snapshot_dir)
        self.snapshot_dir.mkdir(parents=True, exist_ok=True)
        self.log_dir = Path(log_dir)
        
        # Setup logging
        self.logger = logging.getLogger(f"rollback.{deployment_name}")
        self.logger.setLevel(logging.DEBUG)
        
        # Current snapshot being built
        self.current_snapshot: Optional[DeploymentSnapshot] = None
        self.snapshots: List[DeploymentSnapshot] = []
        
        # Load existing snapshots
        self._load_snapshots()
    
    def _load_snapshots(self):
        """Load existing snapshots from disk."""
        snapshot_file = self.snapshot_dir / f"{self.deployment_name}-snapshots.json"
        
        if snapshot_file.exists():
            try:
                with open(snapshot_file, 'r') as f:
                    data = json.load(f)
                    # Would deserialize snapshots here
                    self.logger.info(f"Loaded {len(data.get('snapshots', []))} existing snapshots")
            except Exception as e:
                self.logger.warning(f"Could not load snapshots: {e}")
    
    def _save_snapshots(self):
        """Save snapshots to disk."""
        snapshot_file = self.snapshot_dir / f"{self.deployment_name}-snapshots.json"
        
        try:
            data = {
                "deployment_name": self.deployment_name,
                "updated_at": datetime.now().isoformat(),
                "snapshots": [self._serialize_snapshot(s) for s in self.snapshots]
            }
            
            with open(snapshot_file, 'w') as f:
                json.dump(data, f, indent=2, default=str)
                
        except Exception as e:
            self.logger.error(f"Could not save snapshots: {e}")
    
    def _serialize_snapshot(self, snapshot: DeploymentSnapshot) -> Dict:
        """Serialize a snapshot to a dictionary."""
        return {
            "snapshot_id": snapshot.snapshot_id,
            "deployment_name": snapshot.deployment_name,
            "created_at": snapshot.created_at.isoformat(),
            "phase": snapshot.phase,
            "resources": [
                {
                    "resource_id": r.resource_id,
                    "resource_type": r.resource_type.value,
                    "name": r.name,
                    "platform": r.platform,
                    "created_at": r.created_at.isoformat(),
                    "properties": r.properties,
                    "rollback_command": r.rollback_command,
                    "dependencies": r.dependencies,
                    "can_rollback": r.can_rollback
                }
                for r in snapshot.resources
            ],
            "configurations": snapshot.configurations,
            "database_state": snapshot.database_state,
            "notes": snapshot.notes
        }
    
    def create_snapshot(self, phase: str, notes: str = "") -> DeploymentSnapshot:
        """
        Create a new deployment snapshot.
        
        Args:
            phase: Current deployment phase
            notes: Optional notes about the snapshot
        
        Returns:
            New DeploymentSnapshot
        """
        snapshot_id = f"snap-{datetime.now().strftime('%Y%m%d-%H%M%S')}"
        
        snapshot = DeploymentSnapshot(
            snapshot_id=snapshot_id,
            deployment_name=self.deployment_name,
            created_at=datetime.now(),
            phase=phase,
            notes=notes
        )
        
        self.current_snapshot = snapshot
        self.snapshots.append(snapshot)
        self._save_snapshots()
        
        self.logger.info(f"Created snapshot: {snapshot_id} (phase: {phase})")
        return snapshot
    
    def add_resource(self, resource: ResourceState):
        """
        Add a resource to the current snapshot.
        
        Args:
            resource: Resource state to track
        """
        if not self.current_snapshot:
            self.logger.warning("No active snapshot - creating one")
            self.create_snapshot("unknown")
        
        self.current_snapshot.resources.append(resource)
        self._save_snapshots()
        
        self.logger.debug(f"Added resource: {resource.name} ({resource.resource_type.value})")
    
    def record_resource(
        self,
        resource_id: str,
        resource_type: ResourceType,
        name: str,
        platform: str,
        rollback_command: Optional[str] = None,
        properties: Optional[Dict] = None,
        dependencies: Optional[List[str]] = None
    ):
        """
        Convenience method to record a deployed resource.
        
        Args:
            resource_id: Unique identifier for the resource
            resource_type: Type of resource
            name: Human-readable name
            platform: Target platform (azure, aws, gcp, onprem)
            rollback_command: Command to rollback this resource
            properties: Additional resource properties
            dependencies: List of resource IDs this depends on
        """
        resource = ResourceState(
            resource_id=resource_id,
            resource_type=resource_type,
            name=name,
            platform=platform,
            created_at=datetime.now(),
            properties=properties or {},
            rollback_command=rollback_command,
            dependencies=dependencies or [],
            can_rollback=rollback_command is not None
        )
        
        self.add_resource(resource)
    
    def record_configuration(self, key: str, value: Any):
        """
        Record a configuration change.
        
        Args:
            key: Configuration key
            value: Configuration value
        """
        if not self.current_snapshot:
            self.create_snapshot("configuration")
        
        self.current_snapshot.configurations[key] = value
        self._save_snapshots()
    
    def record_database_state(self, state: Dict[str, Any]):
        """
        Record database state for potential rollback.
        
        Args:
            state: Database state information
        """
        if not self.current_snapshot:
            self.create_snapshot("database")
        
        self.current_snapshot.database_state = state
        self._save_snapshots()
    
    def get_latest_snapshot(self) -> Optional[DeploymentSnapshot]:
        """Get the most recent snapshot."""
        if self.snapshots:
            return self.snapshots[-1]
        return None
    
    def get_snapshot(self, snapshot_id: str) -> Optional[DeploymentSnapshot]:
        """Get a specific snapshot by ID."""
        for snapshot in self.snapshots:
            if snapshot.snapshot_id == snapshot_id:
                return snapshot
        return None
    
    def list_snapshots(self) -> List[Dict]:
        """List all available snapshots."""
        return [
            {
                "snapshot_id": s.snapshot_id,
                "phase": s.phase,
                "created_at": s.created_at.isoformat(),
                "resource_count": len(s.resources),
                "notes": s.notes
            }
            for s in self.snapshots
        ]
    
    def rollback(
        self,
        snapshot_id: Optional[str] = None,
        rollback_type: RollbackType = RollbackType.FULL,
        resource_types: Optional[List[ResourceType]] = None,
        simulation: bool = True
    ) -> Dict[str, Any]:
        """
        Perform a rollback operation.
        
        Args:
            snapshot_id: Snapshot to rollback to (defaults to latest)
            rollback_type: Type of rollback (full, partial, etc.)
            resource_types: Specific resource types to rollback (for partial)
            simulation: If True, simulate rollback without executing
        
        Returns:
            Rollback result dictionary
        """
        result = {
            "success": True,
            "simulation": simulation,
            "rollback_type": rollback_type.value,
            "resources_rolled_back": [],
            "resources_skipped": [],
            "errors": [],
            "started_at": datetime.now().isoformat()
        }
        
        # Get target snapshot
        if snapshot_id:
            snapshot = self.get_snapshot(snapshot_id)
        else:
            snapshot = self.get_latest_snapshot()
        
        if not snapshot:
            result["success"] = False
            result["errors"].append("No snapshot found to rollback to")
            return result
        
        result["snapshot_id"] = snapshot.snapshot_id
        
        self.logger.info("=" * 50)
        self.logger.info("ROLLBACK OPERATION")
        self.logger.info("=" * 50)
        self.logger.info(f"  Target Snapshot: {snapshot.snapshot_id}")
        self.logger.info(f"  Rollback Type: {rollback_type.value}")
        self.logger.info(f"  Simulation: {simulation}")
        
        # Get resources to rollback in correct order
        resources_to_rollback = self._get_rollback_order(
            snapshot.resources,
            resource_types
        )
        
        self.logger.info(f"  Resources to rollback: {len(resources_to_rollback)}")
        
        for resource in resources_to_rollback:
            if not resource.can_rollback:
                result["resources_skipped"].append({
                    "resource_id": resource.resource_id,
                    "name": resource.name,
                    "reason": "Cannot be rolled back"
                })
                self.logger.info(f"  [SKIP] {resource.name} - not rollbackable")
                continue
            
            if resource_types and resource.resource_type not in resource_types:
                result["resources_skipped"].append({
                    "resource_id": resource.resource_id,
                    "name": resource.name,
                    "reason": "Not in selected resource types"
                })
                continue
            
            try:
                self.logger.info(f"  [ROLLBACK] {resource.name}")
                
                if simulation:
                    self.logger.info(f"    Would execute: {resource.rollback_command}")
                else:
                    if resource.rollback_command:
                        self._execute_rollback_command(resource)
                
                result["resources_rolled_back"].append({
                    "resource_id": resource.resource_id,
                    "name": resource.name,
                    "type": resource.resource_type.value
                })
                
            except Exception as e:
                error_msg = f"Failed to rollback {resource.name}: {str(e)}"
                result["errors"].append(error_msg)
                self.logger.error(f"    [ERROR] {error_msg}")
        
        result["completed_at"] = datetime.now().isoformat()
        result["success"] = len(result["errors"]) == 0
        
        self.logger.info("")
        self.logger.info(f"Rollback completed: {len(result['resources_rolled_back'])} resources rolled back")
        
        return result
    
    def _get_rollback_order(
        self,
        resources: List[ResourceState],
        filter_types: Optional[List[ResourceType]] = None
    ) -> List[ResourceState]:
        """
        Get resources in correct rollback order (reverse dependency order).
        
        Args:
            resources: List of resources
            filter_types: Optional filter by resource types
        
        Returns:
            Resources in rollback order
        """
        # Filter by type if specified
        if filter_types:
            resources = [r for r in resources if r.resource_type in filter_types]
        
        # Sort by dependencies (dependents first)
        # Simple reverse order for now
        return list(reversed(resources))
    
    def _execute_rollback_command(self, resource: ResourceState):
        """Execute a rollback command for a resource."""
        import subprocess
        
        if not resource.rollback_command:
            return
        
        self.logger.info(f"    Executing: {resource.rollback_command}")
        
        result = subprocess.run(
            resource.rollback_command,
            shell=True,
            capture_output=True,
            text=True
        )
        
        if result.returncode != 0:
            raise Exception(f"Rollback command failed: {result.stderr}")
    
    def cleanup_old_snapshots(self, keep_count: int = 10):
        """
        Remove old snapshots, keeping only the most recent ones.
        
        Args:
            keep_count: Number of snapshots to keep
        """
        if len(self.snapshots) <= keep_count:
            return
        
        removed = self.snapshots[:-keep_count]
        self.snapshots = self.snapshots[-keep_count:]
        self._save_snapshots()
        
        self.logger.info(f"Cleaned up {len(removed)} old snapshots")
    
    def export_snapshot(self, snapshot_id: str, output_path: str):
        """
        Export a snapshot to a file.
        
        Args:
            snapshot_id: ID of snapshot to export
            output_path: Path to write export file
        """
        snapshot = self.get_snapshot(snapshot_id)
        if not snapshot:
            raise ValueError(f"Snapshot not found: {snapshot_id}")
        
        with open(output_path, 'w') as f:
            json.dump(self._serialize_snapshot(snapshot), f, indent=2)
        
        self.logger.info(f"Exported snapshot {snapshot_id} to {output_path}")


class RollbackPlan:
    """
    A plan for rolling back a deployment.
    Can be reviewed before execution.
    """
    
    def __init__(
        self,
        rollback_service: RollbackService,
        snapshot_id: str,
        rollback_type: RollbackType = RollbackType.FULL
    ):
        self.rollback_service = rollback_service
        self.snapshot_id = snapshot_id
        self.rollback_type = rollback_type
        self.steps: List[Dict] = []
        self.estimated_duration_seconds: int = 0
    
    def generate(self) -> 'RollbackPlan':
        """Generate the rollback plan."""
        snapshot = self.rollback_service.get_snapshot(self.snapshot_id)
        if not snapshot:
            raise ValueError(f"Snapshot not found: {self.snapshot_id}")
        
        resources = self.rollback_service._get_rollback_order(snapshot.resources, None)
        
        for i, resource in enumerate(resources, 1):
            step = {
                "step_number": i,
                "resource_id": resource.resource_id,
                "resource_name": resource.name,
                "resource_type": resource.resource_type.value,
                "action": "DELETE" if resource.rollback_command else "SKIP",
                "command": resource.rollback_command,
                "can_rollback": resource.can_rollback,
                "estimated_seconds": 30  # Default estimate
            }
            
            self.steps.append(step)
            self.estimated_duration_seconds += step["estimated_seconds"]
        
        return self
    
    def to_dict(self) -> Dict:
        """Convert plan to dictionary."""
        return {
            "snapshot_id": self.snapshot_id,
            "rollback_type": self.rollback_type.value,
            "steps": self.steps,
            "total_steps": len(self.steps),
            "estimated_duration_seconds": self.estimated_duration_seconds,
            "estimated_duration_minutes": self.estimated_duration_seconds / 60
        }
    
    def print_plan(self):
        """Print the rollback plan."""
        print("\n" + "=" * 60)
        print("ROLLBACK PLAN")
        print("=" * 60)
        print(f"Snapshot: {self.snapshot_id}")
        print(f"Type: {self.rollback_type.value}")
        print(f"Steps: {len(self.steps)}")
        print(f"Estimated Duration: {self.estimated_duration_seconds / 60:.1f} minutes")
        print("\nSteps:")
        
        for step in self.steps:
            status = "✓" if step["can_rollback"] else "⊘"
            print(f"  {status} {step['step_number']}. {step['resource_name']}")
            print(f"      Type: {step['resource_type']}")
            print(f"      Action: {step['action']}")
            if step["command"]:
                print(f"      Command: {step['command'][:60]}...")
        
        print("=" * 60)
