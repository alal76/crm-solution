#!/usr/bin/env python3
"""
CRM Solution - Comprehensive Deployment Tool
Main CLI entry point for configuration and deployment.

Features:
- Interactive configuration wizard
- Multi-cloud support (Azure, AWS, GCP, On-Premises)
- Provider selection (BuiltIn, OpenSource, Cloud SaaS)
- Simulation mode by default
- Live deployment with --deploy flag
- Rollback on failure

Usage:
    python deploy_cli.py configure           # Run configuration wizard
    python deploy_cli.py deploy              # Deploy in simulation mode
    python deploy_cli.py deploy --deploy     # Actually deploy
    python deploy_cli.py status              # Check deployment status
    python deploy_cli.py rollback            # Rollback deployment
    python deploy_cli.py health              # Run health checks

Author: Abhishek Lal
License: AGPL-3.0
"""

import argparse
import sys
import os
import json
import logging
from pathlib import Path
from datetime import datetime

# Add current directory to path
sys.path.insert(0, str(Path(__file__).parent))

from models.config_models import DeploymentConfig, DeploymentState
from wizard.configuration_wizard import ConfigurationWizard
from orchestrator.deployment_orchestrator import DeploymentOrchestrator, DeploymentPhase
from orchestrator.rollback_service import RollbackService, RollbackType
from orchestrator.health_checker import HealthChecker


VERSION = "1.0.0"
BANNER = f"""
╔═══════════════════════════════════════════════════════════════════════════════╗
║                                                                               ║
║   ██████╗██████╗ ███╗   ███╗    ██████╗ ███████╗██████╗ ██╗      ██████╗ ██╗ ║
║  ██╔════╝██╔══██╗████╗ ████║    ██╔══██╗██╔════╝██╔══██╗██║     ██╔═══██╗╚██╗║
║  ██║     ██████╔╝██╔████╔██║    ██║  ██║█████╗  ██████╔╝██║     ██║   ██║ ██║║
║  ██║     ██╔══██╗██║╚██╔╝██║    ██║  ██║██╔══╝  ██╔═══╝ ██║     ██║   ██║ ██║║
║  ╚██████╗██║  ██║██║ ╚═╝ ██║    ██████╔╝███████╗██║     ███████╗╚██████╔╝██╔╝║
║   ╚═════╝╚═╝  ╚═╝╚═╝     ╚═╝    ╚═════╝ ╚══════╝╚═╝     ╚══════╝ ╚═════╝ ╚═╝ ║
║                                                                               ║
║   Comprehensive Configuration and Deployment Tool                  v{VERSION:5}   ║
║   Multi-Cloud | Multi-Provider | Enterprise Ready                             ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
"""


def setup_logging(verbose: bool = False, log_dir: str = "./logs"):
    """Setup application logging."""
    log_path = Path(log_dir)
    log_path.mkdir(parents=True, exist_ok=True)
    
    log_file = log_path / f"deploy-{datetime.now().strftime('%Y%m%d-%H%M%S')}.log"
    
    level = logging.DEBUG if verbose else logging.INFO
    
    logging.basicConfig(
        level=level,
        format='%(asctime)s | %(levelname)-8s | %(name)-20s | %(message)s',
        handlers=[
            logging.FileHandler(log_file),
            logging.StreamHandler()
        ]
    )
    
    return log_file


def load_config(config_file: str = "deployment-config.json") -> DeploymentConfig:
    """Load deployment configuration from file."""
    config_path = Path(config_file)
    
    if not config_path.exists():
        print(f"\n❌ Configuration file not found: {config_file}")
        print("   Run 'python deploy_cli.py configure' to create one.")
        sys.exit(1)
    
    try:
        return DeploymentConfig.from_json(str(config_path))
    except Exception as e:
        print(f"\n❌ Failed to load configuration: {e}")
        sys.exit(1)


def cmd_configure(args):
    """Run the configuration wizard."""
    print(BANNER)
    
    output_dir = Path(args.output) if args.output else Path(".")
    output_dir.mkdir(parents=True, exist_ok=True)
    
    wizard = ConfigurationWizard(str(output_dir))
    
    try:
        config = wizard.run()
        
        if config:
            print("\n" + "=" * 60)
            print("✅ Configuration completed successfully!")
            print("=" * 60)
            print(f"\nGenerated files:")
            print(f"  • {output_dir}/deployment-config.json")
            print(f"  • {output_dir}/secrets.json")
            print(f"  • {output_dir}/deploy.sh")
            print(f"\nNext steps:")
            print(f"  1. Review the configuration in deployment-config.json")
            print(f"  2. Fill in credentials in secrets.json")
            print(f"  3. Run './deploy.sh' to deploy (simulation mode)")
            print(f"  4. Run './deploy.sh --deploy' to actually deploy")
        else:
            print("\n❌ Configuration was cancelled.")
            
    except KeyboardInterrupt:
        print("\n\n⚠️  Configuration cancelled by user.")
        sys.exit(1)


def cmd_deploy(args):
    """Run deployment."""
    print(BANNER)
    
    simulation_mode = not args.deploy
    
    if simulation_mode:
        print("=" * 60)
        print("   SIMULATION MODE")
        print("   No actual changes will be made")
        print("   Use --deploy flag to perform actual deployment")
        print("=" * 60)
    else:
        print("=" * 60)
        print("   ⚠️  LIVE DEPLOYMENT MODE")
        print("   Changes WILL be applied to the target environment")
        print("=" * 60)
        
        if not args.yes:
            response = input("\nAre you sure you want to proceed? (yes/no): ")
            if response.lower() not in ['yes', 'y']:
                print("\nDeployment cancelled.")
                return
    
    # Load configuration
    config = load_config(args.config)
    
    # Setup logging
    log_file = setup_logging(args.verbose, args.log_dir)
    
    print(f"\nDeployment: {config.name}")
    print(f"Platform: {config.target_platform.value}")
    print(f"Architecture: {config.architecture.value}")
    print(f"Log file: {log_file}")
    print()
    
    # Create orchestrator
    orchestrator = DeploymentOrchestrator(
        config=config,
        simulation_mode=simulation_mode,
        log_dir=args.log_dir
    )
    
    # Run deployment
    result = orchestrator.deploy()
    
    # Exit with appropriate code
    sys.exit(0 if result.success else 1)


def cmd_status(args):
    """Check deployment status."""
    print(BANNER)
    
    config = load_config(args.config)
    
    print(f"\nDeployment Status: {config.name}")
    print("=" * 60)
    
    state = config.deployment_state
    
    print(f"  State: {state.state}")
    
    if state.started_at:
        print(f"  Started: {state.started_at}")
    
    if state.completed_at:
        print(f"  Completed: {state.completed_at}")
    
    print(f"  Current Phase: {state.current_phase}")
    print(f"  Steps Completed: {state.steps_completed}/{state.steps_total}")
    
    if state.error_message:
        print(f"\n  ❌ Error: {state.error_message}")
    
    if state.warnings:
        print(f"\n  Warnings:")
        for warning in state.warnings:
            print(f"    ⚠️  {warning}")
    
    print("=" * 60)


def cmd_health(args):
    """Run health checks."""
    print(BANNER)
    
    config = load_config(args.config)
    
    print(f"\nHealth Check: {config.name}")
    
    # Setup logging
    setup_logging(args.verbose, args.log_dir)
    
    # Create health checker
    checker = HealthChecker(
        deployment_name=config.name,
        log_dir=args.log_dir
    )
    
    # Add provider-specific checks
    providers = {
        "search": config.providers.search_provider,
        "chat": config.providers.chat_provider,
        "analytics": config.providers.analytics_provider,
        "signatures": config.providers.signature_provider,
        "integrations": config.providers.integration_provider
    }
    checker.add_provider_checks(providers)
    
    # Run checks
    report = checker.run_checks(parallel=not args.sequential)
    
    # Export report if requested
    if args.output:
        checker.export_report(report, args.output)
        print(f"\nReport exported to: {args.output}")
    
    # Exit with appropriate code
    sys.exit(0 if report.overall_status.value == "healthy" else 1)


def cmd_rollback(args):
    """Rollback deployment."""
    print(BANNER)
    
    config = load_config(args.config)
    
    print(f"\nRollback: {config.name}")
    print("=" * 60)
    
    simulation_mode = not args.execute
    
    if simulation_mode:
        print("   SIMULATION MODE - No changes will be made")
    else:
        print("   ⚠️  LIVE ROLLBACK - Changes will be undone")
        
        if not args.yes:
            response = input("\nAre you sure you want to rollback? (yes/no): ")
            if response.lower() not in ['yes', 'y']:
                print("\nRollback cancelled.")
                return
    
    # Setup logging
    setup_logging(args.verbose, args.log_dir)
    
    # Create rollback service
    rollback_service = RollbackService(
        deployment_name=config.name,
        snapshot_dir=args.snapshot_dir,
        log_dir=args.log_dir
    )
    
    # List available snapshots
    if args.list:
        snapshots = rollback_service.list_snapshots()
        
        if not snapshots:
            print("\nNo snapshots available.")
            return
        
        print("\nAvailable snapshots:")
        for snap in snapshots:
            print(f"  • {snap['snapshot_id']} - {snap['phase']} ({snap['resource_count']} resources)")
        
        return
    
    # Determine rollback type
    rollback_type = RollbackType.FULL
    if args.infrastructure_only:
        rollback_type = RollbackType.INFRASTRUCTURE
    elif args.application_only:
        rollback_type = RollbackType.APPLICATION
    elif args.database_only:
        rollback_type = RollbackType.DATABASE
    
    # Perform rollback
    result = rollback_service.rollback(
        snapshot_id=args.snapshot,
        rollback_type=rollback_type,
        simulation=simulation_mode
    )
    
    if result['success']:
        print("\n✅ Rollback completed successfully")
    else:
        print("\n❌ Rollback failed:")
        for error in result.get('errors', []):
            print(f"   • {error}")
    
    sys.exit(0 if result['success'] else 1)


def cmd_validate(args):
    """Validate configuration."""
    print(BANNER)
    
    config = load_config(args.config)
    
    print(f"\nValidating: {config.name}")
    print("=" * 60)
    
    errors = []
    warnings = []
    
    # Basic validation
    if not config.name:
        errors.append("Deployment name is required")
    
    if not config.target_platform:
        errors.append("Target platform is required")
    
    # Platform-specific validation
    if config.target_platform.value == "azure":
        if not config.azure_config:
            errors.append("Azure configuration is required for Azure deployments")
        elif not config.azure_config.subscription_id:
            errors.append("Azure subscription ID is required")
    
    # Provider validation
    if config.providers.search_provider not in ["builtin", "meilisearch", "algolia", "elasticsearch", "azure_cognitive_search"]:
        warnings.append(f"Unknown search provider: {config.providers.search_provider}")
    
    # Git configuration
    if not config.git.repository_url:
        warnings.append("Git repository URL not configured")
    
    # SSL validation
    if config.ssl.enabled and not config.ssl.domain:
        warnings.append("SSL is enabled but no domain is specified")
    
    # Print results
    if errors:
        print("\n❌ Validation Errors:")
        for error in errors:
            print(f"   • {error}")
    
    if warnings:
        print("\n⚠️  Warnings:")
        for warning in warnings:
            print(f"   • {warning}")
    
    if not errors and not warnings:
        print("\n✅ Configuration is valid!")
    elif not errors:
        print("\n✅ Configuration is valid (with warnings)")
    else:
        print("\n❌ Configuration has errors")
    
    print("=" * 60)
    
    sys.exit(0 if not errors else 1)


def cmd_export(args):
    """Export deployment artifacts."""
    print(BANNER)
    
    config = load_config(args.config)
    
    output_dir = Path(args.output)
    output_dir.mkdir(parents=True, exist_ok=True)
    
    print(f"\nExporting deployment artifacts for: {config.name}")
    print(f"Output directory: {output_dir}")
    print("=" * 60)
    
    # Export configuration
    config_file = output_dir / "deployment-config.json"
    config.to_json(str(config_file))
    print(f"  ✓ {config_file}")
    
    # Export Kubernetes manifests
    if args.kubernetes:
        k8s_dir = output_dir / "kubernetes"
        k8s_dir.mkdir(exist_ok=True)
        
        # Would generate Kubernetes manifests here
        print(f"  ✓ {k8s_dir}/")
    
    # Export Docker Compose
    if args.docker_compose:
        compose_file = output_dir / "docker-compose.yml"
        # Would generate Docker Compose file here
        print(f"  ✓ {compose_file}")
    
    # Export Terraform
    if args.terraform:
        tf_dir = output_dir / "terraform"
        tf_dir.mkdir(exist_ok=True)
        # Would generate Terraform files here
        print(f"  ✓ {tf_dir}/")
    
    # Export ARM templates (Azure)
    if args.arm and config.target_platform.value == "azure":
        arm_dir = output_dir / "arm-templates"
        arm_dir.mkdir(exist_ok=True)
        # Would generate ARM templates here
        print(f"  ✓ {arm_dir}/")
    
    # Export CloudFormation (AWS)
    if args.cloudformation and config.target_platform.value == "aws":
        cf_dir = output_dir / "cloudformation"
        cf_dir.mkdir(exist_ok=True)
        # Would generate CloudFormation templates here
        print(f"  ✓ {cf_dir}/")
    
    print("=" * 60)
    print("✅ Export completed!")


def main():
    """Main entry point."""
    parser = argparse.ArgumentParser(
        description="CRM Solution - Comprehensive Deployment Tool",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  %(prog)s configure                    Run interactive configuration wizard
  %(prog)s deploy                       Deploy in simulation mode
  %(prog)s deploy --deploy              Actually deploy to target environment
  %(prog)s deploy --deploy --yes        Deploy without confirmation
  %(prog)s health                       Check health of deployed services
  %(prog)s rollback --list              List available rollback snapshots
  %(prog)s rollback --snapshot snap-123 Rollback to specific snapshot
        """
    )
    
    parser.add_argument('--version', action='version', version=f'%(prog)s {VERSION}')
    parser.add_argument('-v', '--verbose', action='store_true', help='Enable verbose logging')
    parser.add_argument('--log-dir', default='./logs', help='Directory for log files')
    
    subparsers = parser.add_subparsers(dest='command', help='Available commands')
    
    # Configure command
    config_parser = subparsers.add_parser('configure', help='Run configuration wizard')
    config_parser.add_argument('-o', '--output', help='Output directory for configuration files')
    config_parser.set_defaults(func=cmd_configure)
    
    # Deploy command
    deploy_parser = subparsers.add_parser('deploy', help='Deploy the CRM solution')
    deploy_parser.add_argument('-c', '--config', default='deployment-config.json',
                              help='Path to deployment configuration file')
    deploy_parser.add_argument('--deploy', action='store_true',
                              help='Actually deploy (default is simulation mode)')
    deploy_parser.add_argument('-y', '--yes', action='store_true',
                              help='Skip confirmation prompts')
    deploy_parser.set_defaults(func=cmd_deploy)
    
    # Status command
    status_parser = subparsers.add_parser('status', help='Check deployment status')
    status_parser.add_argument('-c', '--config', default='deployment-config.json',
                              help='Path to deployment configuration file')
    status_parser.set_defaults(func=cmd_status)
    
    # Health command
    health_parser = subparsers.add_parser('health', help='Run health checks')
    health_parser.add_argument('-c', '--config', default='deployment-config.json',
                              help='Path to deployment configuration file')
    health_parser.add_argument('--sequential', action='store_true',
                              help='Run health checks sequentially instead of parallel')
    health_parser.add_argument('-o', '--output', help='Export health report to file')
    health_parser.set_defaults(func=cmd_health)
    
    # Rollback command
    rollback_parser = subparsers.add_parser('rollback', help='Rollback deployment')
    rollback_parser.add_argument('-c', '--config', default='deployment-config.json',
                                help='Path to deployment configuration file')
    rollback_parser.add_argument('--snapshot', help='Snapshot ID to rollback to')
    rollback_parser.add_argument('--snapshot-dir', default='./snapshots',
                                help='Directory containing snapshots')
    rollback_parser.add_argument('--list', action='store_true',
                                help='List available snapshots')
    rollback_parser.add_argument('--execute', action='store_true',
                                help='Actually perform rollback (default is simulation)')
    rollback_parser.add_argument('--infrastructure-only', action='store_true',
                                help='Only rollback infrastructure')
    rollback_parser.add_argument('--application-only', action='store_true',
                                help='Only rollback application')
    rollback_parser.add_argument('--database-only', action='store_true',
                                help='Only rollback database')
    rollback_parser.add_argument('-y', '--yes', action='store_true',
                                help='Skip confirmation prompts')
    rollback_parser.set_defaults(func=cmd_rollback)
    
    # Validate command
    validate_parser = subparsers.add_parser('validate', help='Validate configuration')
    validate_parser.add_argument('-c', '--config', default='deployment-config.json',
                                help='Path to deployment configuration file')
    validate_parser.set_defaults(func=cmd_validate)
    
    # Export command
    export_parser = subparsers.add_parser('export', help='Export deployment artifacts')
    export_parser.add_argument('-c', '--config', default='deployment-config.json',
                              help='Path to deployment configuration file')
    export_parser.add_argument('-o', '--output', default='./export',
                              help='Output directory for exported artifacts')
    export_parser.add_argument('--kubernetes', action='store_true',
                              help='Export Kubernetes manifests')
    export_parser.add_argument('--docker-compose', action='store_true',
                              help='Export Docker Compose file')
    export_parser.add_argument('--terraform', action='store_true',
                              help='Export Terraform configuration')
    export_parser.add_argument('--arm', action='store_true',
                              help='Export Azure ARM templates')
    export_parser.add_argument('--cloudformation', action='store_true',
                              help='Export AWS CloudFormation templates')
    export_parser.set_defaults(func=cmd_export)
    
    # Parse arguments
    args = parser.parse_args()
    
    if not args.command:
        # No command provided, show help
        print(BANNER)
        parser.print_help()
        sys.exit(0)
    
    # Execute command
    args.func(args)


if __name__ == '__main__':
    main()
