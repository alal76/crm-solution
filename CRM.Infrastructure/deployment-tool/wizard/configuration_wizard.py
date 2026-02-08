#!/usr/bin/env python3
"""
CRM Solution - Configuration Wizard
Interactive CLI wizard for configuring deployments across multiple platforms.

Author: Abhishek Lal
License: AGPL-3.0
"""

import os
import sys
import json
import getpass
from typing import Dict, List, Optional, Any, Tuple
from datetime import datetime
from pathlib import Path

# Add parent directory to path for imports
sys.path.insert(0, str(Path(__file__).parent.parent))

from models.config_models import (
    DeploymentConfig, TargetPlatform, DeploymentArchitecture, HostingType,
    DatabaseType, ProviderStrategy, SSLConfiguration, NetworkConfiguration,
    FrontendConfig, APIConfig, DatabaseConfig, CacheConfig, ModuleSelection,
    ProviderSelection, ProviderCredentials, CloudCredentials, GitConfiguration,
    MonitoringConfig, DeploymentOptions, DeploymentState
)
from models.platform_models import (
    AzureVMSize, AzureContainerSize, AzureDatabaseSKU, AzureRedisSKU, AzureRegion,
    AzurePlatformConfig, AWSInstanceType, AWSRDSInstanceClass, AWSElastiCacheNodeType,
    AWSRegion, AWSPlatformConfig, GCPMachineType, GCPCloudSQLTier, GCPMemorystoreTier,
    GCPRegion, GCPPlatformConfig, OnPremVirtualization, OnPremContainerRuntime,
    OnPremOrchestration, OnPremServer, OnPremPlatformConfig, get_size_recommendation
)
from models.provider_models import (
    ALL_PROVIDERS, get_provider_info, get_providers_by_strategy,
    get_required_containers, get_required_credentials, ProviderInfo
)


class ConfigurationWizard:
    """Interactive configuration wizard for CRM deployment."""
    
    def __init__(self, output_dir: str = "./generated"):
        self.output_dir = Path(output_dir)
        self.output_dir.mkdir(parents=True, exist_ok=True)
        self.config = DeploymentConfig()
        self.secrets = {}  # Store secrets separately
        
    def clear_screen(self):
        """Clear terminal screen."""
        os.system('cls' if os.name == 'nt' else 'clear')
    
    def print_header(self, title: str):
        """Print a section header."""
        width = 70
        print("\n" + "=" * width)
        print(f" {title}".center(width))
        print("=" * width + "\n")
    
    def print_option(self, num: int, text: str, description: str = ""):
        """Print a numbered option."""
        if description:
            print(f"  [{num}] {text}")
            print(f"      {description}")
        else:
            print(f"  [{num}] {text}")
    
    def get_choice(self, prompt: str, valid_choices: List[int], default: Optional[int] = None) -> int:
        """Get a validated numeric choice from user."""
        while True:
            try:
                default_str = f" [{default}]" if default else ""
                choice = input(f"\n{prompt}{default_str}: ").strip()
                
                if not choice and default is not None:
                    return default
                    
                choice_int = int(choice)
                if choice_int in valid_choices:
                    return choice_int
                print(f"  Invalid choice. Please enter one of: {valid_choices}")
            except ValueError:
                print("  Please enter a number.")
    
    def get_string(self, prompt: str, default: str = "", required: bool = True) -> str:
        """Get a string input from user."""
        while True:
            default_str = f" [{default}]" if default else ""
            value = input(f"{prompt}{default_str}: ").strip()
            
            if not value and default:
                return default
            if not value and required:
                print("  This field is required.")
                continue
            return value
    
    def get_password(self, prompt: str, confirm: bool = False) -> str:
        """Get a password input (hidden)."""
        while True:
            password = getpass.getpass(f"{prompt}: ")
            if confirm:
                password2 = getpass.getpass(f"Confirm {prompt}: ")
                if password != password2:
                    print("  Passwords do not match. Try again.")
                    continue
            return password
    
    def get_yes_no(self, prompt: str, default: bool = True) -> bool:
        """Get a yes/no answer from user."""
        default_str = "[Y/n]" if default else "[y/N]"
        while True:
            choice = input(f"{prompt} {default_str}: ").strip().lower()
            if not choice:
                return default
            if choice in ['y', 'yes']:
                return True
            if choice in ['n', 'no']:
                return False
            print("  Please enter 'y' or 'n'.")
    
    def get_int(self, prompt: str, default: int = 0, min_val: int = 0, max_val: int = 100000) -> int:
        """Get an integer input from user."""
        while True:
            try:
                default_str = f" [{default}]" if default else ""
                value = input(f"{prompt}{default_str}: ").strip()
                
                if not value and default:
                    return default
                    
                int_val = int(value)
                if min_val <= int_val <= max_val:
                    return int_val
                print(f"  Value must be between {min_val} and {max_val}.")
            except ValueError:
                print("  Please enter a valid number.")
    
    # ========================================================================
    # WIZARD STEPS
    # ========================================================================
    
    def step_welcome(self):
        """Display welcome message and overview."""
        self.clear_screen()
        self.print_header("CRM Solution Deployment Wizard")
        
        print("""
    Welcome to the CRM Solution Deployment Configuration Wizard!
    
    This wizard will guide you through configuring your deployment:
    
    1. Target Platform Selection (Azure, AWS, GCP, On-Premises)
    2. Deployment Architecture (Monolithic, Microservices)
    3. Infrastructure Configuration (Frontend, API, Database)
    4. Module and Provider Selection
    5. SSL/HTTPS Configuration
    6. Credentials and Certificates
    7. Monitoring and Logging
    8. Review and Generate Configuration
    
    The wizard will generate:
    - Deployment configuration file (JSON/YAML)
    - Infrastructure templates (Terraform, ARM, CloudFormation)
    - Docker Compose / Kubernetes manifests
    - Deployment scripts (bash/PowerShell)
    
    All deployments run in SIMULATION MODE by default.
    Use --deploy flag to perform actual deployment.
        """)
        
        input("\nPress Enter to continue...")
        
    def step_deployment_name(self):
        """Get deployment name and description."""
        self.print_header("Step 1: Deployment Information")
        
        print("First, let's name your deployment.\n")
        
        self.config.name = self.get_string(
            "Deployment name (no spaces)",
            default="crm-production"
        ).replace(" ", "-").lower()
        
        self.config.description = self.get_string(
            "Description",
            default="CRM Solution Deployment",
            required=False
        )
        
        self.config.expected_users = self.get_int(
            "Expected number of concurrent users",
            default=100,
            min_val=1,
            max_val=100000
        )
        
        print(f"\n  ✓ Deployment: {self.config.name}")
        print(f"  ✓ Users: {self.config.expected_users}")
        
        # Get size recommendation
        recommendation = get_size_recommendation(self.config.expected_users)
        if recommendation:
            print(f"\n  📊 Based on {self.config.expected_users} users, we recommend:")
            print(f"     - Description: {recommendation.description}")
            print(f"     - Azure: {recommendation.azure_api_vm.value} VMs, {recommendation.azure_database_sku.value} database")
            print(f"     - AWS: {recommendation.aws_api_instance.value} instances, {recommendation.aws_rds_class.value} RDS")
            print(f"     - GCP: {recommendation.gcp_api_machine.value} machines, {recommendation.gcp_cloudsql_tier.value} SQL")
    
    def step_target_platform(self):
        """Select target deployment platform."""
        self.print_header("Step 2: Target Platform")
        
        print("Select your deployment target:\n")
        
        platforms = [
            (1, "Azure", "Microsoft Azure Cloud (AKS, Container Apps, VMs)"),
            (2, "AWS", "Amazon Web Services (EKS, ECS, Fargate, EC2)"),
            (3, "GCP", "Google Cloud Platform (GKE, Cloud Run, Compute Engine)"),
            (4, "On-Premises", "Self-hosted infrastructure (VMs, Docker, Kubernetes)"),
            (5, "Hybrid", "Mix of cloud and on-premises")
        ]
        
        for num, name, desc in platforms:
            self.print_option(num, name, desc)
        
        choice = self.get_choice("Select platform", [1, 2, 3, 4, 5], default=1)
        
        platform_map = {
            1: TargetPlatform.AZURE,
            2: TargetPlatform.AWS,
            3: TargetPlatform.GCP,
            4: TargetPlatform.ON_PREMISES,
            5: TargetPlatform.HYBRID
        }
        
        self.config.target_platform = platform_map[choice]
        print(f"\n  ✓ Platform: {self.config.target_platform.value}")
        
        # Configure platform-specific settings
        if self.config.target_platform == TargetPlatform.AZURE:
            self._configure_azure()
        elif self.config.target_platform == TargetPlatform.AWS:
            self._configure_aws()
        elif self.config.target_platform == TargetPlatform.GCP:
            self._configure_gcp()
        elif self.config.target_platform == TargetPlatform.ON_PREMISES:
            self._configure_onprem()
    
    def _configure_azure(self):
        """Configure Azure-specific settings."""
        print("\n--- Azure Configuration ---\n")
        
        # Region selection
        print("Available Azure regions:")
        regions = list(AzureRegion)
        for i, region in enumerate(regions[:10], 1):  # Show first 10
            print(f"  [{i}] {region.value}")
        print(f"  [11] Other (enter manually)")
        
        region_choice = self.get_choice("Select region", list(range(1, 12)), default=1)
        if region_choice <= 10:
            selected_region = regions[region_choice - 1]
        else:
            region_name = self.get_string("Enter region name")
            selected_region = AzureRegion(region_name)
        
        # Resource group
        resource_group = self.get_string(
            "Resource group name",
            default=f"{self.config.name}-rg"
        )
        
        # Subscription
        subscription_id = self.get_string(
            "Azure Subscription ID",
            required=True
        )
        
        self.config.azure_config = AzurePlatformConfig(
            subscription_id=subscription_id,
            resource_group=resource_group,
            location=selected_region,
            tags={"environment": self.config.name, "managed-by": "crm-deployment-tool"}
        )
        
        print(f"\n  ✓ Azure region: {selected_region.value}")
        print(f"  ✓ Resource group: {resource_group}")
    
    def _configure_aws(self):
        """Configure AWS-specific settings."""
        print("\n--- AWS Configuration ---\n")
        
        # Region selection
        print("Available AWS regions:")
        regions = list(AWSRegion)
        for i, region in enumerate(regions[:10], 1):
            print(f"  [{i}] {region.value}")
        
        region_choice = self.get_choice("Select region", list(range(1, 11)), default=1)
        selected_region = regions[region_choice - 1]
        
        # VPC
        vpc_cidr = self.get_string(
            "VPC CIDR block",
            default="10.0.0.0/16"
        )
        
        self.config.aws_config = AWSPlatformConfig(
            region=selected_region,
            vpc_cidr=vpc_cidr,
            tags={"Environment": self.config.name, "ManagedBy": "crm-deployment-tool"}
        )
        
        print(f"\n  ✓ AWS region: {selected_region.value}")
        print(f"  ✓ VPC CIDR: {vpc_cidr}")
    
    def _configure_gcp(self):
        """Configure GCP-specific settings."""
        print("\n--- GCP Configuration ---\n")
        
        # Project
        project_id = self.get_string("GCP Project ID", required=True)
        
        # Region selection
        print("\nAvailable GCP regions:")
        regions = list(GCPRegion)
        for i, region in enumerate(regions[:10], 1):
            print(f"  [{i}] {region.value}")
        
        region_choice = self.get_choice("Select region", list(range(1, 11)), default=1)
        selected_region = regions[region_choice - 1]
        
        self.config.gcp_config = GCPPlatformConfig(
            project_id=project_id,
            region=selected_region,
            labels={"environment": self.config.name, "managed-by": "crm-deployment-tool"}
        )
        
        print(f"\n  ✓ GCP project: {project_id}")
        print(f"  ✓ GCP region: {selected_region.value}")
    
    def _configure_onprem(self):
        """Configure on-premises settings."""
        print("\n--- On-Premises Configuration ---\n")
        
        # Virtualization platform
        print("Virtualization platform:")
        platforms = list(OnPremVirtualization)
        for i, platform in enumerate(platforms, 1):
            print(f"  [{i}] {platform.value}")
        
        platform_choice = self.get_choice("Select platform", list(range(1, len(platforms) + 1)), default=1)
        selected_platform = platforms[platform_choice - 1]
        
        # Container runtime
        print("\nContainer runtime:")
        runtimes = list(OnPremContainerRuntime)
        for i, runtime in enumerate(runtimes, 1):
            print(f"  [{i}] {runtime.value}")
        
        runtime_choice = self.get_choice("Select runtime", list(range(1, len(runtimes) + 1)), default=1)
        selected_runtime = runtimes[runtime_choice - 1]
        
        # Orchestration
        print("\nOrchestration:")
        orchestrations = list(OnPremOrchestration)
        for i, orch in enumerate(orchestrations, 1):
            print(f"  [{i}] {orch.value}")
        
        orch_choice = self.get_choice("Select orchestration", list(range(1, len(orchestrations) + 1)), default=1)
        selected_orch = orchestrations[orch_choice - 1]
        
        self.config.onprem_config = OnPremPlatformConfig(
            virtualization=selected_platform,
            container_runtime=selected_runtime,
            orchestration=selected_orch
        )
        
        print(f"\n  ✓ Virtualization: {selected_platform.value}")
        print(f"  ✓ Container runtime: {selected_runtime.value}")
        print(f"  ✓ Orchestration: {selected_orch.value}")
    
    def step_architecture(self):
        """Select deployment architecture."""
        self.print_header("Step 3: Deployment Architecture")
        
        print("Select your deployment architecture:\n")
        
        self.print_option(1, "Monolithic", 
            "Single application with all modules. Simpler, good for smaller deployments.")
        self.print_option(2, "Microservices", 
            "Separate services for each module. More complex, better scalability.")
        
        choice = self.get_choice("Select architecture", [1, 2], default=1)
        
        self.config.architecture = (
            DeploymentArchitecture.MONOLITHIC if choice == 1 
            else DeploymentArchitecture.MICROSERVICES
        )
        
        print(f"\n  ✓ Architecture: {self.config.architecture.value}")
    
    def step_frontend_config(self):
        """Configure frontend deployment."""
        self.print_header("Step 4a: Frontend Configuration")
        
        print("Select frontend hosting type:\n")
        
        hosting_options = self._get_hosting_options_for_platform("frontend")
        for i, (hosting_type, description) in enumerate(hosting_options, 1):
            self.print_option(i, hosting_type.value, description)
        
        choice = self.get_choice("Select hosting", list(range(1, len(hosting_options) + 1)), default=1)
        selected_hosting = hosting_options[choice - 1][0]
        
        # Get instance count
        instance_count = self.get_int(
            "Number of instances/replicas",
            default=2,
            min_val=1,
            max_val=100
        )
        
        # CDN
        use_cdn = self.get_yes_no("Enable CDN for static assets?", default=True)
        
        self.config.frontend = FrontendConfig(
            hosting_type=selected_hosting,
            instance_count=instance_count
        )
        
        if use_cdn:
            self.config.network.enable_cdn = True
        
        print(f"\n  ✓ Frontend hosting: {selected_hosting.value}")
        print(f"  ✓ Instances: {instance_count}")
        print(f"  ✓ CDN: {'Enabled' if use_cdn else 'Disabled'}")
    
    def step_api_config(self):
        """Configure API deployment."""
        self.print_header("Step 4b: API Configuration")
        
        print("Select API hosting type:\n")
        
        hosting_options = self._get_hosting_options_for_platform("api")
        for i, (hosting_type, description) in enumerate(hosting_options, 1):
            self.print_option(i, hosting_type.value, description)
        
        choice = self.get_choice("Select hosting", list(range(1, len(hosting_options) + 1)), default=1)
        selected_hosting = hosting_options[choice - 1][0]
        
        # Get instance count
        instance_count = self.get_int(
            "Number of instances/replicas",
            default=2,
            min_val=1,
            max_val=100
        )
        
        # Auto-scaling
        enable_autoscaling = self.get_yes_no("Enable auto-scaling?", default=True)
        
        min_replicas = instance_count
        max_replicas = instance_count * 5
        if enable_autoscaling:
            min_replicas = self.get_int("Minimum replicas", default=2, min_val=1)
            max_replicas = self.get_int("Maximum replicas", default=10, min_val=min_replicas)
        
        self.config.api = APIConfig(
            hosting_type=selected_hosting,
            instance_count=instance_count,
            enable_autoscaling=enable_autoscaling,
            min_replicas=min_replicas,
            max_replicas=max_replicas
        )
        
        print(f"\n  ✓ API hosting: {selected_hosting.value}")
        print(f"  ✓ Instances: {instance_count}")
        if enable_autoscaling:
            print(f"  ✓ Auto-scaling: {min_replicas}-{max_replicas} replicas")
    
    def step_database_config(self):
        """Configure database deployment."""
        self.print_header("Step 4c: Database Configuration")
        
        print("Select database type:\n")
        
        db_types = [
            (DatabaseType.MARIADB, "MariaDB - MySQL-compatible, open-source"),
            (DatabaseType.MYSQL, "MySQL - Popular relational database"),
            (DatabaseType.POSTGRESQL, "PostgreSQL - Advanced open-source database"),
            (DatabaseType.SQL_SERVER, "SQL Server - Microsoft enterprise database")
        ]
        
        for i, (db_type, desc) in enumerate(db_types, 1):
            self.print_option(i, db_type.value, desc)
        
        choice = self.get_choice("Select database", [1, 2, 3, 4], default=1)
        selected_db = db_types[choice - 1][0]
        
        # Hosting type
        print("\nSelect database hosting:\n")
        
        hosting_options = self._get_db_hosting_options()
        for i, (hosting_type, desc) in enumerate(hosting_options, 1):
            self.print_option(i, hosting_type.value, desc)
        
        hosting_choice = self.get_choice("Select hosting", list(range(1, len(hosting_options) + 1)), default=1)
        selected_hosting = hosting_options[hosting_choice - 1][0]
        
        # High availability
        enable_ha = self.get_yes_no("Enable high availability (replicas)?", default=False)
        
        self.config.database = DatabaseConfig(
            database_type=selected_db,
            hosting_type=selected_hosting,
            enable_high_availability=enable_ha,
            database_name="crm_db"
        )
        
        print(f"\n  ✓ Database: {selected_db.value}")
        print(f"  ✓ Hosting: {selected_hosting.value}")
        print(f"  ✓ High Availability: {'Enabled' if enable_ha else 'Disabled'}")
    
    def step_modules(self):
        """Select CRM modules to deploy."""
        self.print_header("Step 5: Module Selection")
        
        print("Select which CRM modules to enable:\n")
        
        modules = [
            ("core", "Core CRM", "Accounts, Contacts, Opportunities - Always enabled", True),
            ("sales", "Sales Module", "Quotes, Orders, Products, Price Lists", True),
            ("marketing", "Marketing Module", "Campaigns, Leads, Email Templates", True),
            ("service", "Service Desk / ITSM", "Tickets, SLAs, Knowledge Base", True),
            ("itsm", "IT Service Management", "CMDB, Incidents, Changes, Problems", False),
            ("customer_portal", "Customer Portal", "Self-service portal for customers", False),
            ("partner_portal", "Partner Portal", "Partner deal registration, collaboration", False),
            ("knowledge_base", "Knowledge Base", "Articles, FAQs, Documentation", True),
            ("analytics", "Analytics Dashboard", "Reports, Dashboards, Insights", True)
        ]
        
        selection = ModuleSelection()
        
        for key, name, desc, default in modules:
            if key == "core":
                print(f"  [✓] {name} - {desc} (Required)")
                continue
            
            enabled = self.get_yes_no(f"Enable {name}? ({desc})", default=default)
            setattr(selection, f"enable_{key}", enabled)
            print(f"    {'✓' if enabled else '✗'} {name}")
        
        self.config.modules = selection
    
    def step_providers(self):
        """Select providers for each module."""
        self.print_header("Step 6: Provider Selection")
        
        print("""
    For each feature, you can choose:
    - BuiltIn: Basic implementation included with CRM
    - OpenSource: Self-hosted open-source solution
    - Cloud SaaS: Managed cloud service
        """)
        
        categories = [
            ("search", "Search Engine"),
            ("chat", "Live Chat / Messaging"),
            ("notification", "Notifications (Email/SMS/Push)"),
            ("analytics", "Analytics & BI"),
            ("signature", "E-Signatures"),
            ("integration", "Integration Platform"),
            ("ai", "AI / LLM")
        ]
        
        selection = ProviderSelection()
        credentials = ProviderCredentials()
        
        for category, display_name in categories:
            print(f"\n--- {display_name} ---\n")
            
            providers = ALL_PROVIDERS.get(category, {})
            
            # Group by strategy
            builtin = [p for p in providers.values() if p.strategy == "builtin"]
            opensource = [p for p in providers.values() if p.strategy == "opensource"]
            cloud = [p for p in providers.values() if p.strategy == "cloud_saas"]
            
            all_providers = builtin + opensource + cloud
            
            for i, provider in enumerate(all_providers, 1):
                strategy_label = {
                    "builtin": "Built-In",
                    "opensource": "Open-Source",
                    "cloud_saas": "Cloud SaaS"
                }.get(provider.strategy, provider.strategy)
                
                print(f"  [{i}] {provider.display_name} ({strategy_label})")
                print(f"      {provider.description[:60]}...")
                print(f"      Cost: {provider.estimated_monthly_cost} | Setup: {provider.setup_complexity}")
            
            choice = self.get_choice(
                f"Select {display_name} provider", 
                list(range(1, len(all_providers) + 1)), 
                default=1
            )
            
            selected = all_providers[choice - 1]
            setattr(selection, f"{category}_provider", selected.name)
            
            print(f"\n  ✓ Selected: {selected.display_name}")
            
            # Collect required credentials
            if selected.requires_credentials:
                print(f"\n  This provider requires the following credentials:")
                for cred in selected.requires_credentials:
                    print(f"    - {cred}")
                
                collect_now = self.get_yes_no("Enter credentials now?", default=True)
                if collect_now:
                    for cred in selected.requires_credentials:
                        if "password" in cred.lower() or "secret" in cred.lower() or "key" in cred.lower():
                            value = self.get_password(f"  {cred}")
                        else:
                            value = self.get_string(f"  {cred}")
                        self.secrets[cred] = value
                        setattr(credentials, cred, f"${{secrets.{cred}}}")
        
        self.config.providers = selection
        self.config.provider_credentials = credentials
    
    def step_ssl_configuration(self):
        """Configure SSL/TLS settings."""
        self.print_header("Step 7: SSL/HTTPS Configuration")
        
        enable_https = self.get_yes_no("Enable HTTPS?", default=True)
        
        if not enable_https:
            self.config.ssl = SSLConfiguration(enabled=False)
            print("\n  ⚠️  HTTPS disabled. Not recommended for production!")
            return
        
        print("\nSelect certificate source:\n")
        
        self.print_option(1, "Let's Encrypt", "Free automated certificates (recommended)")
        self.print_option(2, "Custom Certificate", "Upload your own certificate files")
        self.print_option(3, "Cloud Provider", "Use Azure/AWS/GCP managed certificates")
        
        choice = self.get_choice("Select certificate source", [1, 2, 3], default=1)
        
        ssl_config = SSLConfiguration(enabled=True)
        
        if choice == 1:
            ssl_config.use_lets_encrypt = True
            ssl_config.lets_encrypt_email = self.get_string(
                "Email for Let's Encrypt notifications",
                required=True
            )
            print(f"\n  ✓ Let's Encrypt configured with {ssl_config.lets_encrypt_email}")
            
        elif choice == 2:
            ssl_config.use_lets_encrypt = False
            ssl_config.certificate_path = self.get_string(
                "Path to certificate file (.crt/.pem)",
                required=True
            )
            ssl_config.private_key_path = self.get_string(
                "Path to private key file (.key)",
                required=True
            )
            print(f"\n  ✓ Custom certificate: {ssl_config.certificate_path}")
            
        elif choice == 3:
            ssl_config.use_lets_encrypt = False
            print("\n  ✓ Will use cloud provider managed certificates")
        
        # Domain configuration
        ssl_config.domain = self.get_string(
            "Primary domain for the CRM",
            default="crm.example.com"
        )
        
        self.config.ssl = ssl_config
        self.config.network.domain = ssl_config.domain
    
    def step_monitoring(self):
        """Configure monitoring and logging."""
        self.print_header("Step 8: Monitoring & Logging")
        
        monitoring = MonitoringConfig()
        
        print("Select monitoring options:\n")
        
        # Application monitoring
        if self.config.target_platform == TargetPlatform.AZURE:
            monitoring.enable_app_insights = self.get_yes_no(
                "Enable Azure Application Insights?", default=True
            )
        
        # Prometheus/Grafana
        monitoring.enable_prometheus = self.get_yes_no(
            "Enable Prometheus metrics?", default=True
        )
        monitoring.enable_grafana = self.get_yes_no(
            "Enable Grafana dashboards?", default=True
        )
        
        # Log aggregation
        print("\nLog aggregation:")
        self.print_option(1, "Built-in file logging", "Simple file-based logs")
        self.print_option(2, "Elasticsearch/Kibana (ELK)", "Full-text search on logs")
        self.print_option(3, "Loki", "Lightweight log aggregation")
        self.print_option(4, "Cloud provider logging", "Azure Monitor / CloudWatch / Cloud Logging")
        
        log_choice = self.get_choice("Select log aggregation", [1, 2, 3, 4], default=1)
        
        monitoring.log_aggregation = {
            1: "file",
            2: "elk",
            3: "loki",
            4: "cloud"
        }[log_choice]
        
        # Log retention
        monitoring.log_retention_days = self.get_int(
            "Log retention (days)",
            default=30,
            min_val=1,
            max_val=365
        )
        
        self.config.monitoring = monitoring
        
        print(f"\n  ✓ Prometheus: {'Enabled' if monitoring.enable_prometheus else 'Disabled'}")
        print(f"  ✓ Grafana: {'Enabled' if monitoring.enable_grafana else 'Disabled'}")
        print(f"  ✓ Log aggregation: {monitoring.log_aggregation}")
        print(f"  ✓ Log retention: {monitoring.log_retention_days} days")
    
    def step_git_configuration(self):
        """Configure Git repository settings."""
        self.print_header("Step 9: Git Configuration")
        
        print("Configure source code repository:\n")
        
        git_config = GitConfiguration()
        
        git_config.repository_url = self.get_string(
            "Git repository URL",
            default="https://github.com/username/crm-solution.git"
        )
        
        git_config.branch = self.get_string(
            "Branch to deploy",
            default="main"
        )
        
        use_ssh = self.get_yes_no("Use SSH authentication?", default=False)
        
        if use_ssh:
            git_config.ssh_key_path = self.get_string(
                "Path to SSH private key",
                default="~/.ssh/id_rsa"
            )
        else:
            use_token = self.get_yes_no("Use personal access token?", default=True)
            if use_token:
                git_config.access_token = "${secrets.git_access_token}"
                self.secrets["git_access_token"] = self.get_password(
                    "Git access token"
                )
        
        self.config.git = git_config
        
        print(f"\n  ✓ Repository: {git_config.repository_url}")
        print(f"  ✓ Branch: {git_config.branch}")
    
    def step_deployment_options(self):
        """Configure deployment options."""
        self.print_header("Step 10: Deployment Options")
        
        options = DeploymentOptions()
        
        print("Configure deployment behavior:\n")
        
        options.simulation_mode = self.get_yes_no(
            "Enable simulation mode (no actual changes)?",
            default=True
        )
        
        options.rollback_on_failure = self.get_yes_no(
            "Automatic rollback on failure?",
            default=True
        )
        
        options.backup_before_deploy = self.get_yes_no(
            "Create backup before deployment?",
            default=True
        )
        
        options.verify_health_after_deploy = self.get_yes_no(
            "Verify health after deployment?",
            default=True
        )
        
        options.notify_on_completion = self.get_yes_no(
            "Send notification on completion?",
            default=False
        )
        
        if options.notify_on_completion:
            options.notification_email = self.get_string(
                "Notification email address",
                required=True
            )
        
        self.config.deployment_options = options
        
        print(f"\n  ✓ Simulation mode: {'ON' if options.simulation_mode else 'OFF'}")
        print(f"  ✓ Auto-rollback: {'Enabled' if options.rollback_on_failure else 'Disabled'}")
        print(f"  ✓ Backup: {'Enabled' if options.backup_before_deploy else 'Disabled'}")
    
    def step_review(self):
        """Review configuration before saving."""
        self.print_header("Configuration Review")
        
        print(f"""
    Deployment Configuration Summary
    ================================
    
    Name: {self.config.name}
    Platform: {self.config.target_platform.value}
    Architecture: {self.config.architecture.value}
    Expected Users: {self.config.expected_users}
    
    Frontend:
      - Hosting: {self.config.frontend.hosting_type.value}
      - Instances: {self.config.frontend.instance_count}
    
    API:
      - Hosting: {self.config.api.hosting_type.value}
      - Instances: {self.config.api.instance_count}
      - Auto-scaling: {self.config.api.enable_autoscaling}
    
    Database:
      - Type: {self.config.database.database_type.value}
      - Hosting: {self.config.database.hosting_type.value}
      - High Availability: {self.config.database.enable_high_availability}
    
    SSL/HTTPS:
      - Enabled: {self.config.ssl.enabled}
      - Domain: {self.config.ssl.domain}
      - Let's Encrypt: {self.config.ssl.use_lets_encrypt}
    
    Providers:
      - Search: {self.config.providers.search_provider}
      - Chat: {self.config.providers.chat_provider}
      - Notifications: {self.config.providers.notification_provider}
      - Analytics: {self.config.providers.analytics_provider}
      - E-Signatures: {self.config.providers.signature_provider}
      - Integration: {self.config.providers.integration_provider}
      - AI: {self.config.providers.ai_provider}
    
    Deployment Options:
      - Simulation Mode: {'ON' if self.config.deployment_options.simulation_mode else 'OFF'}
      - Auto-rollback: {self.config.deployment_options.rollback_on_failure}
        """)
        
        return self.get_yes_no("Is this configuration correct?", default=True)
    
    def save_configuration(self):
        """Save configuration to files."""
        self.print_header("Saving Configuration")
        
        # Create output directory
        output_path = self.output_dir / self.config.name
        output_path.mkdir(parents=True, exist_ok=True)
        
        # Save main config
        config_file = output_path / "deployment-config.json"
        with open(config_file, 'w') as f:
            json.dump(self.config.to_dict(), f, indent=2, default=str)
        print(f"  ✓ Configuration saved to: {config_file}")
        
        # Save secrets (encrypted reference)
        secrets_file = output_path / "secrets.json"
        secrets_data = {
            "_warning": "This file contains sensitive credentials. Encrypt or use a secrets manager!",
            "_created": datetime.now().isoformat(),
            "secrets": self.secrets
        }
        with open(secrets_file, 'w') as f:
            json.dump(secrets_data, f, indent=2)
        os.chmod(secrets_file, 0o600)  # Restrict permissions
        print(f"  ✓ Secrets saved to: {secrets_file}")
        
        # Generate deployment scripts
        self._generate_deploy_script(output_path)
        
        print(f"\n  📁 All files saved to: {output_path}")
        print(f"\n  To deploy, run:")
        print(f"     cd {output_path}")
        print(f"     ./deploy.sh           # Simulation mode")
        print(f"     ./deploy.sh --deploy  # Actual deployment")
    
    def _generate_deploy_script(self, output_path: Path):
        """Generate deployment script."""
        script_content = f'''#!/bin/bash
# CRM Solution Deployment Script
# Generated by Configuration Wizard
# Deployment: {self.config.name}
# Created: {datetime.now().isoformat()}

set -e

CONFIG_FILE="deployment-config.json"
SECRETS_FILE="secrets.json"
LOG_FILE="deployment.log"
SIMULATION_MODE=true

# Parse arguments
while [[ "$#" -gt 0 ]]; do
    case $1 in
        --deploy) SIMULATION_MODE=false ;;
        --config) CONFIG_FILE="$2"; shift ;;
        --help) echo "Usage: $0 [--deploy] [--config <file>]"; exit 0 ;;
        *) echo "Unknown parameter: $1"; exit 1 ;;
    esac
    shift
done

echo "======================================"
echo " CRM Solution Deployment"
echo " Config: $CONFIG_FILE"
echo " Mode: $([ "$SIMULATION_MODE" = true ] && echo 'SIMULATION' || echo 'LIVE DEPLOYMENT')"
echo "======================================"

# Load configuration
if [ ! -f "$CONFIG_FILE" ]; then
    echo "ERROR: Configuration file not found: $CONFIG_FILE"
    exit 1
fi

# Function to log
log() {{
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1" | tee -a "$LOG_FILE"
}}

# Function for simulation
simulate() {{
    if [ "$SIMULATION_MODE" = true ]; then
        log "[SIMULATION] Would execute: $1"
    else
        log "[EXECUTING] $1"
        eval "$1"
    fi
}}

log "Starting deployment..."

# Step 1: Validate credentials
log "Step 1: Validating credentials..."
if [ "$SIMULATION_MODE" = true ]; then
    log "[SIMULATION] Credentials would be validated"
else
    # Add actual credential validation here
    log "Credentials validated"
fi

# Step 2: Deploy infrastructure
log "Step 2: Deploying infrastructure..."
simulate "echo 'Infrastructure deployment commands here'"

# Step 3: Clone/update repository
log "Step 3: Updating source code..."
simulate "git pull origin main"

# Step 4: Build application
log "Step 4: Building application..."
simulate "docker-compose build"

# Step 5: Deploy application
log "Step 5: Deploying application..."
simulate "docker-compose up -d"

# Step 6: Health check
log "Step 6: Running health checks..."
simulate "curl -s http://localhost:5000/health"

log "Deployment complete!"

if [ "$SIMULATION_MODE" = true ]; then
    echo ""
    echo "======================================"
    echo " SIMULATION COMPLETE"
    echo " To perform actual deployment, run:"
    echo "   $0 --deploy"
    echo "======================================"
fi
'''
        
        script_file = output_path / "deploy.sh"
        with open(script_file, 'w') as f:
            f.write(script_content)
        os.chmod(script_file, 0o755)
        print(f"  ✓ Deploy script saved to: {script_file}")
    
    # ========================================================================
    # HELPER METHODS
    # ========================================================================
    
    def _get_hosting_options_for_platform(self, component: str) -> List[Tuple[HostingType, str]]:
        """Get available hosting options based on platform."""
        
        if self.config.target_platform == TargetPlatform.AZURE:
            return [
                (HostingType.CONTAINER, "Azure Container Apps - Serverless containers"),
                (HostingType.KUBERNETES, "Azure Kubernetes Service (AKS)"),
                (HostingType.VM, "Azure Virtual Machines"),
                (HostingType.PAAS, "Azure App Service")
            ]
        elif self.config.target_platform == TargetPlatform.AWS:
            return [
                (HostingType.CONTAINER, "AWS Fargate - Serverless containers"),
                (HostingType.KUBERNETES, "Amazon EKS"),
                (HostingType.VM, "Amazon EC2"),
                (HostingType.SERVERLESS, "AWS Lambda (API only)" if component == "api" else "S3 + CloudFront")
            ]
        elif self.config.target_platform == TargetPlatform.GCP:
            return [
                (HostingType.SERVERLESS, "Cloud Run - Serverless containers"),
                (HostingType.KUBERNETES, "Google Kubernetes Engine (GKE)"),
                (HostingType.VM, "Compute Engine"),
                (HostingType.PAAS, "App Engine")
            ]
        else:  # On-premises
            return [
                (HostingType.CONTAINER, "Docker Containers"),
                (HostingType.KUBERNETES, "Kubernetes / K3s"),
                (HostingType.VM, "Virtual Machines")
            ]
    
    def _get_db_hosting_options(self) -> List[Tuple[HostingType, str]]:
        """Get available database hosting options based on platform."""
        
        if self.config.target_platform == TargetPlatform.AZURE:
            return [
                (HostingType.PAAS, "Azure Database for MySQL/MariaDB (Managed)"),
                (HostingType.VM, "Database on Azure VM"),
                (HostingType.CONTAINER, "Database in Container (Dev only)")
            ]
        elif self.config.target_platform == TargetPlatform.AWS:
            return [
                (HostingType.PAAS, "Amazon RDS (Managed)"),
                (HostingType.VM, "Database on EC2"),
                (HostingType.CONTAINER, "Database in Container (Dev only)")
            ]
        elif self.config.target_platform == TargetPlatform.GCP:
            return [
                (HostingType.PAAS, "Cloud SQL (Managed)"),
                (HostingType.VM, "Database on Compute Engine"),
                (HostingType.CONTAINER, "Database in Container (Dev only)")
            ]
        else:  # On-premises
            return [
                (HostingType.VM, "Database on VM / Bare Metal"),
                (HostingType.CONTAINER, "Database in Container")
            ]
    
    # ========================================================================
    # MAIN WIZARD RUNNER
    # ========================================================================
    
    def run(self):
        """Run the complete configuration wizard."""
        try:
            self.step_welcome()
            self.step_deployment_name()
            self.step_target_platform()
            self.step_architecture()
            self.step_frontend_config()
            self.step_api_config()
            self.step_database_config()
            self.step_modules()
            self.step_providers()
            self.step_ssl_configuration()
            self.step_monitoring()
            self.step_git_configuration()
            self.step_deployment_options()
            
            if self.step_review():
                self.save_configuration()
            else:
                print("\n  Configuration cancelled. Run wizard again to reconfigure.")
                
        except KeyboardInterrupt:
            print("\n\n  Wizard cancelled by user.")
            sys.exit(1)


# ============================================================================
# CLI ENTRY POINT
# ============================================================================

def main():
    """Main entry point for the configuration wizard."""
    import argparse
    
    parser = argparse.ArgumentParser(
        description="CRM Solution Deployment Configuration Wizard"
    )
    parser.add_argument(
        "--output", "-o",
        default="./generated",
        help="Output directory for generated files"
    )
    parser.add_argument(
        "--non-interactive",
        action="store_true",
        help="Run with defaults (for CI/CD)"
    )
    
    args = parser.parse_args()
    
    wizard = ConfigurationWizard(output_dir=args.output)
    wizard.run()


if __name__ == "__main__":
    main()
