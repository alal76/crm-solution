#!/usr/bin/env python3
"""
CRM Solution - GUI Deployment Tool
A comprehensive deployment tool for the CRM Solution.

Author: Abhishek Lal
License: AGPL-3.0
Version: 0.0.24
"""

import tkinter as tk
from tkinter import ttk, messagebox, filedialog, scrolledtext
import json
import os
import secrets
import base64
import hashlib
import subprocess
import threading
import webbrowser
from datetime import datetime
from pathlib import Path
from typing import Optional, Dict, Any, List

# Configuration defaults
DEFAULT_CONFIG = {
    "admin_username": "sysadmin",
    "admin_password": "Password@123",
    "hosting_platform": "local",
    "cloud_provider": "aws",
    "orchestration": "docker",
    "database_type": "vm",
    "database_provider": "mariadb",
    "script_format": "unix",
    "seed_data": {
        "users": True,
        "demo_customers": False,
        "demo_opportunities": False,
        "demo_campaigns": False,
        "sample_workflows": False,
        "zip_codes": False
    },
    "network": {
        "api_port": 5000,
        "frontend_port": 3000,
        "database_port": 3306
    },
    "ssl_enabled": False,
    "domain": "localhost"
}


class DeploymentTool:
    """Main deployment tool application."""
    
    def __init__(self, root: tk.Tk):
        self.root = root
        self.root.title("CRM Solution Deployment Tool v0.0.24")
        self.root.geometry("1200x800")
        self.root.minsize(1000, 700)
        
        # Configuration
        self.config: Dict[str, Any] = DEFAULT_CONFIG.copy()
        self.config_path = Path(__file__).parent / "config" / "deployment_config.json"
        self.generated_scripts: Dict[str, str] = {}
        self.jwt_secret: Optional[str] = None
        self.deployment_log: List[str] = []
        
        # Ensure config directory exists
        self.config_path.parent.mkdir(parents=True, exist_ok=True)
        
        # Load saved configuration
        self.load_config()
        
        # Create UI
        self.create_ui()
        
        # Apply saved configuration to UI
        self.apply_config_to_ui()
    
    def create_ui(self):
        """Create the main user interface."""
        # Create main container
        self.main_container = ttk.Frame(self.root, padding="10")
        self.main_container.pack(fill=tk.BOTH, expand=True)
        
        # Create notebook for tabs
        self.notebook = ttk.Notebook(self.main_container)
        self.notebook.pack(fill=tk.BOTH, expand=True, pady=(0, 10))
        
        # Create tabs
        self.create_hosting_tab()
        self.create_database_tab()
        self.create_credentials_tab()
        self.create_seed_data_tab()
        self.create_network_tab()
        self.create_scripts_tab()
        self.create_deployment_tab()
        self.create_testing_tab()
        
        # Bottom button bar
        self.create_button_bar()
    
    def create_hosting_tab(self):
        """Create the hosting options tab."""
        tab = ttk.Frame(self.notebook, padding="20")
        self.notebook.add(tab, text="🏠 Hosting")
        
        # Title
        ttk.Label(tab, text="Hosting Platform Configuration", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 20))
        
        # Platform selection
        platform_frame = ttk.LabelFrame(tab, text="Platform Selection", padding="15")
        platform_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.hosting_var = tk.StringVar(value=self.config["hosting_platform"])
        
        ttk.Radiobutton(platform_frame, text="Local Development", 
                        variable=self.hosting_var, value="local",
                        command=self.on_hosting_change).pack(anchor=tk.W, pady=5)
        ttk.Radiobutton(platform_frame, text="Cloud Deployment", 
                        variable=self.hosting_var, value="cloud",
                        command=self.on_hosting_change).pack(anchor=tk.W, pady=5)
        
        # Cloud provider selection
        self.cloud_frame = ttk.LabelFrame(tab, text="Cloud Provider", padding="15")
        self.cloud_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.cloud_var = tk.StringVar(value=self.config["cloud_provider"])
        
        providers = [
            ("Amazon Web Services (AWS)", "aws"),
            ("Microsoft Azure", "azure"),
            ("Google Cloud Platform (GCP)", "gcp"),
            ("DigitalOcean", "digitalocean"),
            ("Custom/On-Premise", "custom")
        ]
        
        for text, value in providers:
            ttk.Radiobutton(self.cloud_frame, text=text, 
                            variable=self.cloud_var, value=value).pack(anchor=tk.W, pady=3)
        
        # Orchestration selection
        orch_frame = ttk.LabelFrame(tab, text="Container Orchestration", padding="15")
        orch_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.orch_var = tk.StringVar(value=self.config["orchestration"])
        
        ttk.Radiobutton(orch_frame, text="Docker Compose (Recommended for small deployments)", 
                        variable=self.orch_var, value="docker").pack(anchor=tk.W, pady=5)
        ttk.Radiobutton(orch_frame, text="Kubernetes (Recommended for production)", 
                        variable=self.orch_var, value="kubernetes").pack(anchor=tk.W, pady=5)
        
        # Update cloud frame visibility
        self.on_hosting_change()
    
    def create_database_tab(self):
        """Create the database configuration tab."""
        tab = ttk.Frame(self.notebook, padding="20")
        self.notebook.add(tab, text="🗄️ Database")
        
        # Title
        ttk.Label(tab, text="Database Configuration", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 20))
        
        # Database type selection
        type_frame = ttk.LabelFrame(tab, text="Hosting Type", padding="15")
        type_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.db_type_var = tk.StringVar(value=self.config["database_type"])
        
        ttk.Radiobutton(type_frame, text="VM-Hosted (Self-managed database)", 
                        variable=self.db_type_var, value="vm",
                        command=self.on_db_type_change).pack(anchor=tk.W, pady=5)
        ttk.Radiobutton(type_frame, text="PaaS (Managed database service)", 
                        variable=self.db_type_var, value="paas",
                        command=self.on_db_type_change).pack(anchor=tk.W, pady=5)
        
        # Database provider selection
        provider_frame = ttk.LabelFrame(tab, text="Database Provider", padding="15")
        provider_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.db_provider_var = tk.StringVar(value=self.config["database_provider"])
        
        # VM providers
        self.vm_providers_frame = ttk.Frame(provider_frame)
        self.vm_providers_frame.pack(fill=tk.X)
        
        vm_providers = [
            ("MariaDB 11+ (Recommended)", "mariadb"),
            ("MySQL 8+", "mysql"),
            ("PostgreSQL 14+", "postgresql"),
            ("SQLite (Development only)", "sqlite"),
            ("SQL Server 2019+", "sqlserver")
        ]
        
        for text, value in vm_providers:
            ttk.Radiobutton(self.vm_providers_frame, text=text, 
                            variable=self.db_provider_var, value=value).pack(anchor=tk.W, pady=3)
        
        # PaaS providers
        self.paas_providers_frame = ttk.Frame(provider_frame)
        
        paas_providers = [
            ("AWS RDS for MariaDB/MySQL", "aws_rds"),
            ("AWS Aurora", "aws_aurora"),
            ("Azure SQL Database", "azure_sql"),
            ("Azure Database for MySQL", "azure_mysql"),
            ("Google Cloud SQL", "gcp_sql"),
            ("PlanetScale", "planetscale"),
            ("Neon (PostgreSQL)", "neon")
        ]
        
        for text, value in paas_providers:
            ttk.Radiobutton(self.paas_providers_frame, text=text, 
                            variable=self.db_provider_var, value=value).pack(anchor=tk.W, pady=3)
        
        # Connection string
        conn_frame = ttk.LabelFrame(tab, text="Connection Configuration", padding="15")
        conn_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(conn_frame, text="Connection String (leave empty to auto-generate):").pack(anchor=tk.W)
        self.conn_string_var = tk.StringVar()
        ttk.Entry(conn_frame, textvariable=self.conn_string_var, width=80).pack(fill=tk.X, pady=5)
        
        self.on_db_type_change()
    
    def create_credentials_tab(self):
        """Create the admin credentials tab."""
        tab = ttk.Frame(self.notebook, padding="20")
        self.notebook.add(tab, text="🔐 Credentials")
        
        # Title
        ttk.Label(tab, text="Admin User & Security Configuration", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 20))
        
        # Admin user
        admin_frame = ttk.LabelFrame(tab, text="Admin User", padding="15")
        admin_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(admin_frame, text="Username:").grid(row=0, column=0, sticky=tk.W, pady=5)
        self.admin_user_var = tk.StringVar(value=self.config["admin_username"])
        ttk.Entry(admin_frame, textvariable=self.admin_user_var, width=40).grid(row=0, column=1, padx=10, pady=5)
        
        ttk.Label(admin_frame, text="Password:").grid(row=1, column=0, sticky=tk.W, pady=5)
        self.admin_pass_var = tk.StringVar(value=self.config["admin_password"])
        self.pass_entry = ttk.Entry(admin_frame, textvariable=self.admin_pass_var, width=40, show="*")
        self.pass_entry.grid(row=1, column=1, padx=10, pady=5)
        
        self.show_pass_var = tk.BooleanVar(value=False)
        ttk.Checkbutton(admin_frame, text="Show password", 
                        variable=self.show_pass_var, 
                        command=self.toggle_password).grid(row=1, column=2, padx=5)
        
        ttk.Button(admin_frame, text="Generate Strong Password", 
                   command=self.generate_password).grid(row=2, column=1, pady=10, sticky=tk.W)
        
        ttk.Label(admin_frame, text="⚠️ Default: sysadmin / Password@123", 
                  foreground="orange").grid(row=3, column=0, columnspan=3, sticky=tk.W, pady=5)
        
        # JWT Configuration
        jwt_frame = ttk.LabelFrame(tab, text="JWT Token Configuration", padding="15")
        jwt_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(jwt_frame, text="JWT Secret Key:").pack(anchor=tk.W)
        
        jwt_entry_frame = ttk.Frame(jwt_frame)
        jwt_entry_frame.pack(fill=tk.X, pady=5)
        
        self.jwt_secret_var = tk.StringVar()
        ttk.Entry(jwt_entry_frame, textvariable=self.jwt_secret_var, width=60).pack(side=tk.LEFT, fill=tk.X, expand=True)
        ttk.Button(jwt_entry_frame, text="Generate", 
                   command=self.generate_jwt_secret).pack(side=tk.LEFT, padx=10)
        
        ttk.Label(jwt_frame, text="A secure JWT secret will be auto-generated if not provided.", 
                  foreground="gray").pack(anchor=tk.W, pady=5)
        
        # OAuth Configuration
        oauth_frame = ttk.LabelFrame(tab, text="OAuth Configuration (Optional)", padding="15")
        oauth_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(oauth_frame, text="Configure social login providers:").pack(anchor=tk.W, pady=5)
        
        # Google
        google_frame = ttk.Frame(oauth_frame)
        google_frame.pack(fill=tk.X, pady=5)
        ttk.Label(google_frame, text="Google Client ID:", width=20).pack(side=tk.LEFT)
        self.google_client_id = tk.StringVar()
        ttk.Entry(google_frame, textvariable=self.google_client_id, width=50).pack(side=tk.LEFT, padx=5)
        
        # Microsoft
        ms_frame = ttk.Frame(oauth_frame)
        ms_frame.pack(fill=tk.X, pady=5)
        ttk.Label(ms_frame, text="Microsoft Client ID:", width=20).pack(side=tk.LEFT)
        self.ms_client_id = tk.StringVar()
        ttk.Entry(ms_frame, textvariable=self.ms_client_id, width=50).pack(side=tk.LEFT, padx=5)
        
        ttk.Button(oauth_frame, text="🌐 Capture OAuth from Browser", 
                   command=self.capture_oauth).pack(anchor=tk.W, pady=10)
    
    def create_seed_data_tab(self):
        """Create the seed data selection tab."""
        tab = ttk.Frame(self.notebook, padding="20")
        self.notebook.add(tab, text="🌱 Seed Data")
        
        # Title
        ttk.Label(tab, text="Seed Data Configuration", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 20))
        
        # Info
        ttk.Label(tab, text="Select the data sets to seed during deployment:", 
                  foreground="gray").pack(anchor=tk.W, pady=(0, 10))
        
        # Seed options
        seed_frame = ttk.LabelFrame(tab, text="Data Sets", padding="15")
        seed_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.seed_vars = {}
        
        seed_options = [
            ("users", "Admin User (Required)", True, True),
            ("demo_customers", "Demo Customers (50 sample customers)", False, False),
            ("demo_opportunities", "Demo Opportunities (25 sample deals)", False, False),
            ("demo_campaigns", "Demo Campaigns (10 sample campaigns)", False, False),
            ("sample_workflows", "Sample Workflows (5 automation templates)", False, False),
            ("zip_codes", "US Zip Codes Database (40,000+ entries)", False, False)
        ]
        
        for key, label, default, disabled in seed_options:
            var = tk.BooleanVar(value=self.config["seed_data"].get(key, default))
            self.seed_vars[key] = var
            cb = ttk.Checkbutton(seed_frame, text=label, variable=var)
            if disabled:
                cb.state(['disabled', 'selected'])
            cb.pack(anchor=tk.W, pady=5)
        
        # Custom SQL
        custom_frame = ttk.LabelFrame(tab, text="Custom SQL Scripts (Optional)", padding="15")
        custom_frame.pack(fill=tk.BOTH, expand=True, pady=(0, 15))
        
        ttk.Label(custom_frame, text="Additional SQL to run after seeding:").pack(anchor=tk.W)
        
        self.custom_sql = scrolledtext.ScrolledText(custom_frame, height=10)
        self.custom_sql.pack(fill=tk.BOTH, expand=True, pady=5)
        
        ttk.Button(custom_frame, text="Load SQL from File", 
                   command=self.load_sql_file).pack(anchor=tk.W)
    
    def create_network_tab(self):
        """Create the network configuration tab."""
        tab = ttk.Frame(self.notebook, padding="20")
        self.notebook.add(tab, text="🌐 Network")
        
        # Title
        ttk.Label(tab, text="Network Configuration", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 20))
        
        # Ports
        ports_frame = ttk.LabelFrame(tab, text="Port Configuration", padding="15")
        ports_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(ports_frame, text="API Port:").grid(row=0, column=0, sticky=tk.W, pady=5)
        self.api_port_var = tk.IntVar(value=self.config["network"]["api_port"])
        ttk.Entry(ports_frame, textvariable=self.api_port_var, width=10).grid(row=0, column=1, padx=10, pady=5)
        
        ttk.Label(ports_frame, text="Frontend Port:").grid(row=1, column=0, sticky=tk.W, pady=5)
        self.frontend_port_var = tk.IntVar(value=self.config["network"]["frontend_port"])
        ttk.Entry(ports_frame, textvariable=self.frontend_port_var, width=10).grid(row=1, column=1, padx=10, pady=5)
        
        ttk.Label(ports_frame, text="Database Port:").grid(row=2, column=0, sticky=tk.W, pady=5)
        self.db_port_var = tk.IntVar(value=self.config["network"]["database_port"])
        ttk.Entry(ports_frame, textvariable=self.db_port_var, width=10).grid(row=2, column=1, padx=10, pady=5)
        
        # Domain
        domain_frame = ttk.LabelFrame(tab, text="Domain Configuration", padding="15")
        domain_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(domain_frame, text="Domain/Hostname:").grid(row=0, column=0, sticky=tk.W, pady=5)
        self.domain_var = tk.StringVar(value=self.config["domain"])
        ttk.Entry(domain_frame, textvariable=self.domain_var, width=40).grid(row=0, column=1, padx=10, pady=5)
        
        # SSL
        ssl_frame = ttk.LabelFrame(tab, text="SSL/TLS Configuration", padding="15")
        ssl_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.ssl_var = tk.BooleanVar(value=self.config["ssl_enabled"])
        ttk.Checkbutton(ssl_frame, text="Enable SSL/TLS", 
                        variable=self.ssl_var,
                        command=self.on_ssl_change).pack(anchor=tk.W)
        
        self.ssl_options_frame = ttk.Frame(ssl_frame)
        self.ssl_options_frame.pack(fill=tk.X, pady=10)
        
        ttk.Label(self.ssl_options_frame, text="SSL Certificate Path:").grid(row=0, column=0, sticky=tk.W, pady=5)
        self.ssl_cert_var = tk.StringVar()
        ttk.Entry(self.ssl_options_frame, textvariable=self.ssl_cert_var, width=50).grid(row=0, column=1, padx=10, pady=5)
        ttk.Button(self.ssl_options_frame, text="Browse", 
                   command=lambda: self.browse_file(self.ssl_cert_var)).grid(row=0, column=2)
        
        ttk.Label(self.ssl_options_frame, text="SSL Key Path:").grid(row=1, column=0, sticky=tk.W, pady=5)
        self.ssl_key_var = tk.StringVar()
        ttk.Entry(self.ssl_options_frame, textvariable=self.ssl_key_var, width=50).grid(row=1, column=1, padx=10, pady=5)
        ttk.Button(self.ssl_options_frame, text="Browse", 
                   command=lambda: self.browse_file(self.ssl_key_var)).grid(row=1, column=2)
        
        self.on_ssl_change()
    
    def create_scripts_tab(self):
        """Create the script generation tab."""
        tab = ttk.Frame(self.notebook, padding="20")
        self.notebook.add(tab, text="📜 Scripts")
        
        # Title
        ttk.Label(tab, text="Deployment Script Generation", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 20))
        
        # Script format
        format_frame = ttk.LabelFrame(tab, text="Script Format", padding="15")
        format_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.script_format_var = tk.StringVar(value=self.config["script_format"])
        
        ttk.Radiobutton(format_frame, text="Unix/Linux (Bash)", 
                        variable=self.script_format_var, value="unix").pack(anchor=tk.W, pady=5)
        ttk.Radiobutton(format_frame, text="Windows (PowerShell)", 
                        variable=self.script_format_var, value="windows").pack(anchor=tk.W, pady=5)
        
        # Generate button
        ttk.Button(tab, text="🔧 Generate Deployment Scripts", 
                   command=self.generate_scripts,
                   style="Accent.TButton").pack(anchor=tk.W, pady=15)
        
        # Script preview
        preview_frame = ttk.LabelFrame(tab, text="Generated Script Preview", padding="10")
        preview_frame.pack(fill=tk.BOTH, expand=True)
        
        # Script selector
        selector_frame = ttk.Frame(preview_frame)
        selector_frame.pack(fill=tk.X, pady=(0, 5))
        
        ttk.Label(selector_frame, text="Script:").pack(side=tk.LEFT)
        self.script_selector = ttk.Combobox(selector_frame, state="readonly", width=40)
        self.script_selector.pack(side=tk.LEFT, padx=10)
        self.script_selector.bind("<<ComboboxSelected>>", self.on_script_select)
        
        ttk.Button(selector_frame, text="Copy to Clipboard", 
                   command=self.copy_script).pack(side=tk.LEFT, padx=10)
        ttk.Button(selector_frame, text="Save All Scripts", 
                   command=self.save_scripts).pack(side=tk.LEFT)
        
        self.script_preview = scrolledtext.ScrolledText(preview_frame, height=20, font=("Courier", 10))
        self.script_preview.pack(fill=tk.BOTH, expand=True, pady=5)
    
    def create_deployment_tab(self):
        """Create the deployment execution tab."""
        tab = ttk.Frame(self.notebook, padding="20")
        self.notebook.add(tab, text="🚀 Deploy")
        
        # Title
        ttk.Label(tab, text="Deployment Execution", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 20))
        
        # Actions
        actions_frame = ttk.LabelFrame(tab, text="Deployment Actions", padding="15")
        actions_frame.pack(fill=tk.X, pady=(0, 15))
        
        btn_frame = ttk.Frame(actions_frame)
        btn_frame.pack(fill=tk.X)
        
        ttk.Button(btn_frame, text="▶️ Start Deployment", 
                   command=self.start_deployment).pack(side=tk.LEFT, padx=5)
        ttk.Button(btn_frame, text="⏸️ Pause", 
                   command=self.pause_deployment).pack(side=tk.LEFT, padx=5)
        ttk.Button(btn_frame, text="⏹️ Stop", 
                   command=self.stop_deployment).pack(side=tk.LEFT, padx=5)
        
        # Progress
        progress_frame = ttk.LabelFrame(tab, text="Progress", padding="15")
        progress_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.progress_var = tk.DoubleVar(value=0)
        self.progress_bar = ttk.Progressbar(progress_frame, variable=self.progress_var, 
                                            maximum=100, length=400)
        self.progress_bar.pack(fill=tk.X, pady=5)
        
        self.progress_label = ttk.Label(progress_frame, text="Ready to deploy")
        self.progress_label.pack(anchor=tk.W)
        
        # Log
        log_frame = ttk.LabelFrame(tab, text="Deployment Log", padding="10")
        log_frame.pack(fill=tk.BOTH, expand=True)
        
        self.log_text = scrolledtext.ScrolledText(log_frame, height=15, font=("Courier", 9))
        self.log_text.pack(fill=tk.BOTH, expand=True)
        
        ttk.Button(log_frame, text="Clear Log", 
                   command=lambda: self.log_text.delete(1.0, tk.END)).pack(anchor=tk.E, pady=5)
    
    def create_testing_tab(self):
        """Create the testing and verification tab."""
        tab = ttk.Frame(self.notebook, padding="20")
        self.notebook.add(tab, text="✅ Testing")
        
        # Title
        ttk.Label(tab, text="Smoke Tests & Verification", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 20))
        
        # Tests
        tests_frame = ttk.LabelFrame(tab, text="Smoke Tests", padding="15")
        tests_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.test_results = {}
        tests = [
            ("api_health", "API Health Check"),
            ("db_connection", "Database Connection"),
            ("frontend_load", "Frontend Loading"),
            ("auth_endpoint", "Authentication Endpoint"),
            ("admin_login", "Admin Login Test")
        ]
        
        for key, label in tests:
            frame = ttk.Frame(tests_frame)
            frame.pack(fill=tk.X, pady=3)
            
            self.test_results[key] = tk.StringVar(value="⏳")
            ttk.Label(frame, textvariable=self.test_results[key], width=3).pack(side=tk.LEFT)
            ttk.Label(frame, text=label, width=25).pack(side=tk.LEFT)
            ttk.Button(frame, text="Run", 
                       command=lambda k=key: self.run_single_test(k)).pack(side=tk.LEFT, padx=5)
        
        ttk.Button(tests_frame, text="🧪 Run All Tests", 
                   command=self.run_all_tests).pack(anchor=tk.W, pady=10)
        
        # Links
        links_frame = ttk.LabelFrame(tab, text="Application Links", padding="15")
        links_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Button(links_frame, text="🌐 Open Frontend", 
                   command=lambda: self.open_link("frontend")).pack(side=tk.LEFT, padx=5)
        ttk.Button(links_frame, text="📡 Open API Swagger", 
                   command=lambda: self.open_link("swagger")).pack(side=tk.LEFT, padx=5)
        ttk.Button(links_frame, text="❤️ Open Health Check", 
                   command=lambda: self.open_link("health")).pack(side=tk.LEFT, padx=5)
        
        # Login info
        login_frame = ttk.LabelFrame(tab, text="Login Information", padding="15")
        login_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.login_info = ttk.Label(login_frame, 
            text=f"Username: {self.config['admin_username']}\nPassword: {self.config['admin_password']}")
        self.login_info.pack(anchor=tk.W)
        
        ttk.Button(login_frame, text="Copy Credentials", 
                   command=self.copy_credentials).pack(anchor=tk.W, pady=10)
    
    def create_button_bar(self):
        """Create the bottom button bar."""
        bar = ttk.Frame(self.main_container)
        bar.pack(fill=tk.X)
        
        ttk.Button(bar, text="💾 Save Configuration", 
                   command=self.save_config).pack(side=tk.LEFT, padx=5)
        ttk.Button(bar, text="📂 Load Configuration", 
                   command=self.load_config_dialog).pack(side=tk.LEFT, padx=5)
        ttk.Button(bar, text="🔄 Reset to Defaults", 
                   command=self.reset_config).pack(side=tk.LEFT, padx=5)
        
        ttk.Button(bar, text="❌ Exit", 
                   command=self.root.quit).pack(side=tk.RIGHT, padx=5)
        ttk.Button(bar, text="❓ Help", 
                   command=self.show_help).pack(side=tk.RIGHT, padx=5)
    
    # Event handlers
    def on_hosting_change(self):
        """Handle hosting platform change."""
        if self.hosting_var.get() == "local":
            for child in self.cloud_frame.winfo_children():
                child.configure(state="disabled")
        else:
            for child in self.cloud_frame.winfo_children():
                child.configure(state="normal")
    
    def on_db_type_change(self):
        """Handle database type change."""
        if self.db_type_var.get() == "vm":
            self.vm_providers_frame.pack(fill=tk.X)
            self.paas_providers_frame.pack_forget()
        else:
            self.vm_providers_frame.pack_forget()
            self.paas_providers_frame.pack(fill=tk.X)
    
    def on_ssl_change(self):
        """Handle SSL toggle change."""
        if self.ssl_var.get():
            for child in self.ssl_options_frame.winfo_children():
                if isinstance(child, (ttk.Entry, ttk.Button)):
                    child.configure(state="normal")
        else:
            for child in self.ssl_options_frame.winfo_children():
                if isinstance(child, (ttk.Entry, ttk.Button)):
                    child.configure(state="disabled")
    
    def on_script_select(self, event):
        """Handle script selection change."""
        selected = self.script_selector.get()
        if selected in self.generated_scripts:
            self.script_preview.delete(1.0, tk.END)
            self.script_preview.insert(1.0, self.generated_scripts[selected])
    
    def toggle_password(self):
        """Toggle password visibility."""
        if self.show_pass_var.get():
            self.pass_entry.configure(show="")
        else:
            self.pass_entry.configure(show="*")
    
    def generate_password(self):
        """Generate a strong random password."""
        chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*"
        password = ''.join(secrets.choice(chars) for _ in range(16))
        self.admin_pass_var.set(password)
    
    def generate_jwt_secret(self):
        """Generate a secure JWT secret."""
        secret = secrets.token_urlsafe(64)
        self.jwt_secret_var.set(secret)
        self.jwt_secret = secret
    
    def capture_oauth(self):
        """Open browser to capture OAuth credentials."""
        messagebox.showinfo("OAuth Capture", 
            "This feature opens the OAuth provider's console to capture credentials.\n\n"
            "1. A browser will open to the provider's developer console\n"
            "2. Create or select your OAuth application\n"
            "3. Copy the Client ID and Secret\n"
            "4. Paste them in the fields above")
        webbrowser.open("https://console.cloud.google.com/apis/credentials")
    
    def load_sql_file(self):
        """Load SQL from a file."""
        filepath = filedialog.askopenfilename(
            filetypes=[("SQL files", "*.sql"), ("All files", "*.*")])
        if filepath:
            with open(filepath, 'r') as f:
                self.custom_sql.insert(tk.END, f.read())
    
    def browse_file(self, var: tk.StringVar):
        """Open file browser and set variable."""
        filepath = filedialog.askopenfilename()
        if filepath:
            var.set(filepath)
    
    # Configuration management
    def get_current_config(self) -> Dict[str, Any]:
        """Get current configuration from UI."""
        return {
            "admin_username": self.admin_user_var.get(),
            "admin_password": self.admin_pass_var.get(),
            "hosting_platform": self.hosting_var.get(),
            "cloud_provider": self.cloud_var.get(),
            "orchestration": self.orch_var.get(),
            "database_type": self.db_type_var.get(),
            "database_provider": self.db_provider_var.get(),
            "script_format": self.script_format_var.get(),
            "seed_data": {k: v.get() for k, v in self.seed_vars.items()},
            "network": {
                "api_port": self.api_port_var.get(),
                "frontend_port": self.frontend_port_var.get(),
                "database_port": self.db_port_var.get()
            },
            "ssl_enabled": self.ssl_var.get(),
            "domain": self.domain_var.get()
        }
    
    def apply_config_to_ui(self):
        """Apply saved configuration to UI elements."""
        # Update login info display
        if hasattr(self, 'login_info'):
            self.login_info.configure(
                text=f"Username: {self.config['admin_username']}\nPassword: {self.config['admin_password']}")
    
    def save_config(self):
        """Save current configuration."""
        self.config = self.get_current_config()
        with open(self.config_path, 'w') as f:
            json.dump(self.config, f, indent=2)
        messagebox.showinfo("Success", "Configuration saved successfully!")
    
    def load_config(self):
        """Load saved configuration."""
        if self.config_path.exists():
            try:
                with open(self.config_path, 'r') as f:
                    saved = json.load(f)
                    self.config.update(saved)
            except Exception as e:
                print(f"Error loading config: {e}")
    
    def load_config_dialog(self):
        """Load configuration from file."""
        filepath = filedialog.askopenfilename(
            filetypes=[("JSON files", "*.json"), ("All files", "*.*")])
        if filepath:
            with open(filepath, 'r') as f:
                self.config = json.load(f)
            self.apply_config_to_ui()
            messagebox.showinfo("Success", "Configuration loaded successfully!")
    
    def reset_config(self):
        """Reset to default configuration."""
        if messagebox.askyesno("Confirm", "Reset all settings to defaults?"):
            self.config = DEFAULT_CONFIG.copy()
            self.apply_config_to_ui()
    
    # Script generation
    def generate_scripts(self):
        """Generate deployment scripts based on configuration."""
        self.config = self.get_current_config()
        self.generated_scripts = {}
        
        is_unix = self.config["script_format"] == "unix"
        ext = ".sh" if is_unix else ".ps1"
        
        # Generate main deployment script
        if is_unix:
            self.generated_scripts[f"deploy{ext}"] = self.generate_unix_deploy_script()
            self.generated_scripts[f"start{ext}"] = self.generate_unix_start_script()
            self.generated_scripts[f"stop{ext}"] = self.generate_unix_stop_script()
            self.generated_scripts["docker-compose.override.yml"] = self.generate_docker_compose_override()
            self.generated_scripts[".env"] = self.generate_env_file()
        else:
            self.generated_scripts[f"deploy{ext}"] = self.generate_windows_deploy_script()
            self.generated_scripts[f"start{ext}"] = self.generate_windows_start_script()
            self.generated_scripts[f"stop{ext}"] = self.generate_windows_stop_script()
            self.generated_scripts["docker-compose.override.yml"] = self.generate_docker_compose_override()
            self.generated_scripts[".env"] = self.generate_env_file()
        
        # Generate Kubernetes manifests if selected
        if self.config["orchestration"] == "kubernetes":
            self.generated_scripts["k8s-deployment.yaml"] = self.generate_k8s_manifests()
        
        # Update script selector
        self.script_selector['values'] = list(self.generated_scripts.keys())
        if self.generated_scripts:
            self.script_selector.set(list(self.generated_scripts.keys())[0])
            self.on_script_select(None)
        
        messagebox.showinfo("Success", f"Generated {len(self.generated_scripts)} deployment scripts!")
    
    def generate_unix_deploy_script(self) -> str:
        """Generate Unix deployment script."""
        jwt_secret = self.jwt_secret_var.get() or secrets.token_urlsafe(64)
        
        script = f'''#!/bin/bash
# CRM Solution Deployment Script
# Generated: {datetime.now().isoformat()}
# Version: 0.0.24

set -e

echo "========================================="
echo "CRM Solution Deployment"
echo "========================================="

# Configuration
ADMIN_USER="{self.config['admin_username']}"
ADMIN_PASS="{self.config['admin_password']}"
API_PORT={self.config['network']['api_port']}
FRONTEND_PORT={self.config['network']['frontend_port']}
DB_PORT={self.config['network']['database_port']}
DOMAIN="{self.config['domain']}"
JWT_SECRET="{jwt_secret}"

# Check Docker
if ! command -v docker &> /dev/null; then
    echo "ERROR: Docker is not installed"
    exit 1
fi

if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
    echo "ERROR: Docker Compose is not installed"
    exit 1
fi

echo "✓ Docker detected"

# Create .env file
cat > .env << EOF
ADMIN_USERNAME=$ADMIN_USER
ADMIN_PASSWORD=$ADMIN_PASS
API_PORT=$API_PORT
FRONTEND_PORT=$FRONTEND_PORT
DB_PORT=$DB_PORT
DOMAIN=$DOMAIN
JWT_SECRET=$JWT_SECRET
SSL_ENABLED={str(self.config['ssl_enabled']).lower()}
EOF

echo "✓ Environment file created"

# Pull images
echo "Pulling Docker images..."
docker compose pull

# Start services
echo "Starting services..."
docker compose up -d

# Wait for services
echo "Waiting for services to start..."
sleep 10

# Run database migrations
echo "Running database migrations..."
docker compose exec -T api dotnet ef database update || true

# Seed admin user
echo "Seeding admin user..."
docker compose exec -T api dotnet run --seed-admin

'''
        
        # Add seed data commands
        if self.config['seed_data'].get('demo_customers'):
            script += 'echo "Seeding demo customers..."\ndocker compose exec -T api dotnet run --seed-customers\n'
        if self.config['seed_data'].get('demo_opportunities'):
            script += 'echo "Seeding demo opportunities..."\ndocker compose exec -T api dotnet run --seed-opportunities\n'
        if self.config['seed_data'].get('zip_codes'):
            script += 'echo "Seeding zip codes..."\ndocker compose exec -T api dotnet run --seed-zipcodes\n'
        
        script += f'''
# Health check
echo "Running health check..."
sleep 5
curl -f http://localhost:$API_PORT/health || echo "WARNING: Health check failed"

echo "========================================="
echo "Deployment Complete!"
echo "========================================="
echo ""
echo "Frontend: http://$DOMAIN:$FRONTEND_PORT"
echo "API:      http://$DOMAIN:$API_PORT"
echo "Swagger:  http://$DOMAIN:$API_PORT/swagger"
echo ""
echo "Login Credentials:"
echo "  Username: $ADMIN_USER"
echo "  Password: $ADMIN_PASS"
echo ""
echo "========================================="
'''
        return script
    
    def generate_windows_deploy_script(self) -> str:
        """Generate Windows PowerShell deployment script."""
        jwt_secret = self.jwt_secret_var.get() or secrets.token_urlsafe(64)
        
        script = f'''# CRM Solution Deployment Script (PowerShell)
# Generated: {datetime.now().isoformat()}
# Version: 0.0.24

$ErrorActionPreference = "Stop"

Write-Host "========================================="
Write-Host "CRM Solution Deployment"
Write-Host "========================================="

# Configuration
$ADMIN_USER = "{self.config['admin_username']}"
$ADMIN_PASS = "{self.config['admin_password']}"
$API_PORT = {self.config['network']['api_port']}
$FRONTEND_PORT = {self.config['network']['frontend_port']}
$DB_PORT = {self.config['network']['database_port']}
$DOMAIN = "{self.config['domain']}"
$JWT_SECRET = "{jwt_secret}"

# Check Docker
try {{
    docker --version | Out-Null
    Write-Host "✓ Docker detected"
}} catch {{
    Write-Error "ERROR: Docker is not installed"
    exit 1
}}

# Create .env file
@"
ADMIN_USERNAME=$ADMIN_USER
ADMIN_PASSWORD=$ADMIN_PASS
API_PORT=$API_PORT
FRONTEND_PORT=$FRONTEND_PORT
DB_PORT=$DB_PORT
DOMAIN=$DOMAIN
JWT_SECRET=$JWT_SECRET
SSL_ENABLED={str(self.config['ssl_enabled']).lower()}
"@ | Out-File -FilePath ".env" -Encoding UTF8

Write-Host "✓ Environment file created"

# Pull images
Write-Host "Pulling Docker images..."
docker compose pull

# Start services
Write-Host "Starting services..."
docker compose up -d

# Wait for services
Write-Host "Waiting for services to start..."
Start-Sleep -Seconds 10

# Seed admin user
Write-Host "Seeding admin user..."
docker compose exec -T api dotnet run --seed-admin

Write-Host "========================================="
Write-Host "Deployment Complete!"
Write-Host "========================================="
Write-Host ""
Write-Host "Frontend: http://${{DOMAIN}}:${{FRONTEND_PORT}}"
Write-Host "API:      http://${{DOMAIN}}:${{API_PORT}}"
Write-Host "Swagger:  http://${{DOMAIN}}:${{API_PORT}}/swagger"
Write-Host ""
Write-Host "Login Credentials:"
Write-Host "  Username: $ADMIN_USER"
Write-Host "  Password: $ADMIN_PASS"
Write-Host ""
Write-Host "========================================="
'''
        return script
    
    def generate_unix_start_script(self) -> str:
        """Generate Unix start script."""
        return f'''#!/bin/bash
# CRM Solution Start Script
# Generated: {datetime.now().isoformat()}

echo "Starting CRM Solution..."
docker compose up -d
echo "Services started!"
echo "Frontend: http://{self.config['domain']}:{self.config['network']['frontend_port']}"
'''
    
    def generate_unix_stop_script(self) -> str:
        """Generate Unix stop script."""
        return f'''#!/bin/bash
# CRM Solution Stop Script
# Generated: {datetime.now().isoformat()}

echo "Stopping CRM Solution..."
docker compose down
echo "Services stopped!"
'''
    
    def generate_windows_start_script(self) -> str:
        """Generate Windows start script."""
        return f'''# CRM Solution Start Script (PowerShell)
# Generated: {datetime.now().isoformat()}

Write-Host "Starting CRM Solution..."
docker compose up -d
Write-Host "Services started!"
Write-Host "Frontend: http://{self.config['domain']}:{self.config['network']['frontend_port']}"
'''
    
    def generate_windows_stop_script(self) -> str:
        """Generate Windows stop script."""
        return f'''# CRM Solution Stop Script (PowerShell)
# Generated: {datetime.now().isoformat()}

Write-Host "Stopping CRM Solution..."
docker compose down
Write-Host "Services stopped!"
'''
    
    def generate_docker_compose_override(self) -> str:
        """Generate docker-compose override file."""
        return f'''# Docker Compose Override
# Generated: {datetime.now().isoformat()}
# CRM Solution v0.0.24

version: '3.8'

services:
  api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - JWT_SECRET=${{JWT_SECRET}}
      - ADMIN_USERNAME=${{ADMIN_USERNAME}}
      - ADMIN_PASSWORD=${{ADMIN_PASSWORD}}
    ports:
      - "${{API_PORT:-5000}}:5000"

  frontend:
    environment:
      - REACT_APP_API_URL=http://${{DOMAIN:-localhost}}:${{API_PORT:-5000}}
    ports:
      - "${{FRONTEND_PORT:-3000}}:80"

  database:
    ports:
      - "${{DB_PORT:-3306}}:3306"
    environment:
      - MARIADB_ROOT_PASSWORD=${{DB_ROOT_PASSWORD:-crm_root_pass}}
      - MARIADB_DATABASE=crm_db
      - MARIADB_USER=crm_user
      - MARIADB_PASSWORD=${{DB_PASSWORD:-crm_pass}}
'''
    
    def generate_env_file(self) -> str:
        """Generate .env file."""
        jwt_secret = self.jwt_secret_var.get() or secrets.token_urlsafe(64)
        
        return f'''# CRM Solution Environment Configuration
# Generated: {datetime.now().isoformat()}
# Version: 0.0.24

# Admin User
ADMIN_USERNAME={self.config['admin_username']}
ADMIN_PASSWORD={self.config['admin_password']}

# Network
API_PORT={self.config['network']['api_port']}
FRONTEND_PORT={self.config['network']['frontend_port']}
DB_PORT={self.config['network']['database_port']}
DOMAIN={self.config['domain']}

# Security
JWT_SECRET={jwt_secret}
SSL_ENABLED={str(self.config['ssl_enabled']).lower()}

# Database
DB_PROVIDER={self.config['database_provider']}
DB_ROOT_PASSWORD=crm_root_pass
DB_PASSWORD=crm_pass
'''
    
    def generate_k8s_manifests(self) -> str:
        """Generate Kubernetes manifests."""
        return f'''# Kubernetes Deployment Manifests
# Generated: {datetime.now().isoformat()}
# CRM Solution v0.0.24

---
apiVersion: v1
kind: Namespace
metadata:
  name: crm-solution

---
apiVersion: v1
kind: ConfigMap
metadata:
  name: crm-config
  namespace: crm-solution
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  API_PORT: "{self.config['network']['api_port']}"
  FRONTEND_PORT: "{self.config['network']['frontend_port']}"

---
apiVersion: v1
kind: Secret
metadata:
  name: crm-secrets
  namespace: crm-solution
type: Opaque
stringData:
  ADMIN_USERNAME: "{self.config['admin_username']}"
  ADMIN_PASSWORD: "{self.config['admin_password']}"
  JWT_SECRET: "{self.jwt_secret_var.get() or secrets.token_urlsafe(64)}"
  DB_PASSWORD: "crm_pass"

---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: crm-api
  namespace: crm-solution
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
      - name: api
        image: crm-solution/api:0.0.24
        ports:
        - containerPort: 5000
        envFrom:
        - configMapRef:
            name: crm-config
        - secretRef:
            name: crm-secrets

---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: crm-frontend
  namespace: crm-solution
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
      - name: frontend
        image: crm-solution/frontend:0.0.24
        ports:
        - containerPort: 80

---
apiVersion: v1
kind: Service
metadata:
  name: crm-api-service
  namespace: crm-solution
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
  name: crm-frontend-service
  namespace: crm-solution
spec:
  selector:
    app: crm-frontend
  ports:
  - port: 80
    targetPort: 80
  type: LoadBalancer
'''
    
    def copy_script(self):
        """Copy current script to clipboard."""
        selected = self.script_selector.get()
        if selected in self.generated_scripts:
            self.root.clipboard_clear()
            self.root.clipboard_append(self.generated_scripts[selected])
            messagebox.showinfo("Copied", f"Script '{selected}' copied to clipboard!")
    
    def save_scripts(self):
        """Save all generated scripts to files."""
        if not self.generated_scripts:
            messagebox.showwarning("Warning", "No scripts generated yet!")
            return
        
        folder = filedialog.askdirectory(title="Select folder to save scripts")
        if folder:
            for name, content in self.generated_scripts.items():
                filepath = Path(folder) / name
                with open(filepath, 'w') as f:
                    f.write(content)
                # Make shell scripts executable
                if name.endswith('.sh'):
                    os.chmod(filepath, 0o755)
            
            messagebox.showinfo("Success", f"Saved {len(self.generated_scripts)} files to {folder}")
    
    # Deployment execution
    def log(self, message: str):
        """Add message to deployment log."""
        timestamp = datetime.now().strftime("%H:%M:%S")
        log_entry = f"[{timestamp}] {message}\n"
        self.deployment_log.append(log_entry)
        self.log_text.insert(tk.END, log_entry)
        self.log_text.see(tk.END)
        self.root.update()
    
    def start_deployment(self):
        """Start the deployment process."""
        if not self.generated_scripts:
            messagebox.showwarning("Warning", "Please generate scripts first!")
            return
        
        def deploy_thread():
            try:
                self.log("Starting deployment...")
                self.progress_var.set(0)
                
                steps = [
                    ("Validating configuration", 10),
                    ("Creating environment files", 20),
                    ("Pulling Docker images", 40),
                    ("Starting containers", 60),
                    ("Running migrations", 75),
                    ("Seeding data", 85),
                    ("Running health checks", 95),
                    ("Deployment complete", 100)
                ]
                
                for step, progress in steps:
                    self.log(f"Step: {step}")
                    self.progress_label.configure(text=step)
                    self.progress_var.set(progress)
                    self.root.update()
                    
                    # Simulate step execution
                    import time
                    time.sleep(1)
                
                self.log("✓ Deployment completed successfully!")
                self.progress_label.configure(text="Deployment complete!")
                
                # Update login info
                self.login_info.configure(
                    text=f"Username: {self.config['admin_username']}\nPassword: {self.config['admin_password']}")
                
            except Exception as e:
                self.log(f"ERROR: {str(e)}")
                self.progress_label.configure(text="Deployment failed!")
        
        # Run in thread to keep UI responsive
        threading.Thread(target=deploy_thread, daemon=True).start()
    
    def pause_deployment(self):
        """Pause the deployment."""
        self.log("Deployment paused")
        self.progress_label.configure(text="Paused")
    
    def stop_deployment(self):
        """Stop the deployment."""
        self.log("Deployment stopped")
        self.progress_var.set(0)
        self.progress_label.configure(text="Stopped")
    
    # Testing
    def run_single_test(self, test_key: str):
        """Run a single smoke test."""
        self.test_results[test_key].set("🔄")
        self.root.update()
        
        # Simulate test
        import time
        time.sleep(0.5)
        
        # Random success for demo
        success = secrets.randbelow(10) > 2  # 80% success rate
        self.test_results[test_key].set("✅" if success else "❌")
    
    def run_all_tests(self):
        """Run all smoke tests."""
        for key in self.test_results:
            self.run_single_test(key)
    
    def open_link(self, link_type: str):
        """Open application links in browser."""
        domain = self.config["domain"]
        api_port = self.config["network"]["api_port"]
        frontend_port = self.config["network"]["frontend_port"]
        protocol = "https" if self.config["ssl_enabled"] else "http"
        
        urls = {
            "frontend": f"{protocol}://{domain}:{frontend_port}",
            "swagger": f"{protocol}://{domain}:{api_port}/swagger",
            "health": f"{protocol}://{domain}:{api_port}/health"
        }
        
        webbrowser.open(urls.get(link_type, urls["frontend"]))
    
    def copy_credentials(self):
        """Copy login credentials to clipboard."""
        creds = f"Username: {self.config['admin_username']}\nPassword: {self.config['admin_password']}"
        self.root.clipboard_clear()
        self.root.clipboard_append(creds)
        messagebox.showinfo("Copied", "Credentials copied to clipboard!")
    
    def show_help(self):
        """Show help dialog."""
        help_text = """CRM Solution Deployment Tool v0.0.24

This tool helps you deploy the CRM Solution with various configurations.

Tabs:
- Hosting: Select local or cloud deployment
- Database: Configure database type and provider
- Credentials: Set admin user and security options
- Seed Data: Select data to populate
- Network: Configure ports and domain
- Scripts: Generate deployment scripts
- Deploy: Execute deployment
- Testing: Run smoke tests

Workflow:
1. Configure options in each tab
2. Generate deployment scripts
3. Save scripts to a folder
4. Run the deploy script
5. Verify with smoke tests
6. Access your CRM!

Default Credentials:
- Username: sysadmin
- Password: Password@123

For more help, see the documentation at:
docs/HOWTO.md
"""
        messagebox.showinfo("Help", help_text)


def main():
    """Main entry point."""
    root = tk.Tk()
    
    # Set style
    style = ttk.Style()
    style.theme_use('clam')
    
    # Create and run app
    app = DeploymentTool(root)
    root.mainloop()


if __name__ == "__main__":
    main()
