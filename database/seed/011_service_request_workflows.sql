-- =====================================================
-- Service Request Workflow Definitions Seed Data
-- =====================================================
-- Creates workflow definitions for all service request types
-- These workflows will be triggered when service requests are created/updated
-- =====================================================

-- Hardware Support Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('hardware-troubleshooting', 'Hardware Troubleshooting Workflow', 'Standard workflow for diagnosing and resolving hardware issues', 'Service Request', 'ServiceRequest', 1, 1, 'Computer', '#3b82f6', 0, 2, 10, 24, NOW(), 0),
('performance-optimization', 'Performance Optimization Workflow', 'Workflow for system performance analysis and optimization', 'Service Request', 'ServiceRequest', 1, 1, 'Gauge', '#10b981', 0, 2, 10, 48, NOW(), 0),
('display-repair', 'Display Repair Workflow', 'Workflow for display and monitor troubleshooting and repair', 'Service Request', 'ServiceRequest', 1, 1, 'Monitor', '#8b5cf6', 0, 2, 10, 24, NOW(), 0),
('peripheral-support', 'Peripheral Support Workflow', 'Workflow for peripheral device support and troubleshooting', 'Service Request', 'ServiceRequest', 1, 1, 'Usb', '#06b6d4', 0, 3, 10, 12, NOW(), 0),
('hardware-upgrade', 'Hardware Upgrade Workflow', 'Workflow for hardware upgrade requests and implementation', 'Service Request', 'ServiceRequest', 1, 1, 'ArrowUp', '#f59e0b', 0, 2, 5, 72, NOW(), 0);

-- Network Support Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('network-outage-resolution', 'Network Outage Resolution Workflow', 'Critical workflow for network outage response and resolution', 'Service Request', 'ServiceRequest', 1, 1, 'WifiOff', '#ef4444', 0, 1, 20, 4, NOW(), 0),
('network-performance', 'Network Performance Workflow', 'Workflow for network performance issues and optimization', 'Service Request', 'ServiceRequest', 1, 1, 'Activity', '#f97316', 0, 2, 10, 24, NOW(), 0),
('network-configuration', 'Network Configuration Workflow', 'Workflow for network configuration changes and requests', 'Service Request', 'ServiceRequest', 1, 1, 'Settings', '#6366f1', 0, 2, 10, 48, NOW(), 0),
('firmware-update', 'Firmware Update Workflow', 'Workflow for firmware update scheduling and deployment', 'Service Request', 'ServiceRequest', 1, 1, 'Download', '#8b5cf6', 0, 3, 10, 24, NOW(), 0),
('hardware-port-repair', 'Hardware Port Repair Workflow', 'Workflow for network hardware port repair and replacement', 'Service Request', 'ServiceRequest', 1, 1, 'Plug', '#64748b', 0, 2, 10, 24, NOW(), 0);

-- Server and Infrastructure Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('critical-server-recovery', 'Critical Server Recovery Workflow', 'Emergency workflow for critical server failure recovery', 'Service Request', 'ServiceRequest', 1, 1, 'Server', '#dc2626', 0, 1, 20, 2, NOW(), 0),
('resource-optimization', 'Resource Optimization Workflow', 'Workflow for server resource optimization and allocation', 'Service Request', 'ServiceRequest', 1, 1, 'Cpu', '#22c55e', 0, 2, 10, 48, NOW(), 0),
('storage-expansion', 'Storage Expansion Workflow', 'Workflow for storage capacity expansion requests', 'Service Request', 'ServiceRequest', 1, 1, 'Database', '#0ea5e9', 0, 2, 5, 72, NOW(), 0),
('raid-recovery', 'RAID Recovery Workflow', 'Workflow for RAID array recovery and reconstruction', 'Service Request', 'ServiceRequest', 1, 1, 'HardDrive', '#f43f5e', 0, 1, 5, 24, NOW(), 0),
('virtualization-support', 'Virtualization Support Workflow', 'Workflow for virtualization platform support issues', 'Service Request', 'ServiceRequest', 1, 1, 'Layers', '#a855f7', 0, 2, 10, 24, NOW(), 0);

-- Data Center Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('environmental-alert', 'Environmental Alert Workflow', 'Workflow for data center environmental alerts (temperature, humidity)', 'Service Request', 'ServiceRequest', 1, 1, 'Thermometer', '#ef4444', 0, 1, 20, 1, NOW(), 0),
('power-system-recovery', 'Power System Recovery Workflow', 'Workflow for power failure and recovery procedures', 'Service Request', 'ServiceRequest', 1, 1, 'Zap', '#dc2626', 0, 1, 20, 2, NOW(), 0),
('ups-maintenance', 'UPS Maintenance Workflow', 'Workflow for UPS maintenance and battery replacement', 'Service Request', 'ServiceRequest', 1, 1, 'Battery', '#f59e0b', 0, 2, 10, 48, NOW(), 0),
('rack-allocation', 'Rack Allocation Workflow', 'Workflow for rack space allocation and cabling requests', 'Service Request', 'ServiceRequest', 1, 1, 'LayoutGrid', '#6366f1', 0, 3, 10, 72, NOW(), 0);

-- Mobile Device Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('mobile-device-support', 'Mobile Device Support Workflow', 'Workflow for mobile device troubleshooting and support', 'Service Request', 'ServiceRequest', 1, 1, 'Smartphone', '#3b82f6', 0, 2, 10, 24, NOW(), 0),
('mobile-connectivity', 'Mobile Connectivity Workflow', 'Workflow for mobile connectivity and network issues', 'Service Request', 'ServiceRequest', 1, 1, 'Signal', '#f97316', 0, 2, 10, 12, NOW(), 0),
('mdm-support', 'MDM Support Workflow', 'Workflow for Mobile Device Management issues', 'Service Request', 'ServiceRequest', 1, 1, 'ShieldCheck', '#8b5cf6', 0, 2, 10, 24, NOW(), 0);

-- Printer and Scanner Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('printer-troubleshooting', 'Printer Troubleshooting Workflow', 'Workflow for printer troubleshooting and repair', 'Service Request', 'ServiceRequest', 1, 1, 'Printer', '#64748b', 0, 3, 10, 24, NOW(), 0),
('printer-maintenance', 'Printer Maintenance Workflow', 'Workflow for scheduled printer maintenance', 'Service Request', 'ServiceRequest', 1, 1, 'Wrench', '#22c55e', 0, 3, 10, 48, NOW(), 0),
('print-quality', 'Print Quality Workflow', 'Workflow for print quality issues', 'Service Request', 'ServiceRequest', 1, 1, 'FileText', '#f59e0b', 0, 3, 10, 24, NOW(), 0),
('consumables-order', 'Consumables Order Workflow', 'Workflow for printer consumables ordering and delivery', 'Service Request', 'ServiceRequest', 1, 1, 'Package', '#0ea5e9', 0, 4, 10, 72, NOW(), 0),
('scanner-support', 'Scanner Support Workflow', 'Workflow for scanner troubleshooting and support', 'Service Request', 'ServiceRequest', 1, 1, 'Scan', '#6366f1', 0, 3, 10, 24, NOW(), 0);

-- Operating System Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('os-recovery', 'OS Recovery Workflow', 'Workflow for operating system recovery and reinstallation', 'Service Request', 'ServiceRequest', 1, 1, 'RotateCcw', '#ef4444', 0, 1, 5, 8, NOW(), 0),
('system-stability', 'System Stability Workflow', 'Workflow for system stability and crash investigation', 'Service Request', 'ServiceRequest', 1, 1, 'AlertTriangle', '#f97316', 0, 2, 10, 24, NOW(), 0),
('license-resolution', 'License Resolution Workflow', 'Workflow for software licensing issues', 'Service Request', 'ServiceRequest', 1, 1, 'Key', '#8b5cf6', 0, 2, 10, 48, NOW(), 0),
('os-upgrade', 'OS Upgrade Workflow', 'Workflow for operating system upgrade projects', 'Service Request', 'ServiceRequest', 1, 1, 'ArrowUpCircle', '#22c55e', 0, 3, 5, 168, NOW(), 0),
('update-troubleshooting', 'Update Troubleshooting Workflow', 'Workflow for Windows/OS update issues', 'Service Request', 'ServiceRequest', 1, 1, 'RefreshCw', '#3b82f6', 0, 2, 10, 24, NOW(), 0);

-- Productivity Software Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('email-support', 'Email Support Workflow', 'Workflow for email client issues and support', 'Service Request', 'ServiceRequest', 1, 1, 'Mail', '#3b82f6', 0, 2, 10, 12, NOW(), 0),
('office-repair', 'Office Repair Workflow', 'Workflow for Microsoft Office repair and troubleshooting', 'Service Request', 'ServiceRequest', 1, 1, 'FileSpreadsheet', '#f97316', 0, 2, 10, 24, NOW(), 0),
('license-management', 'License Management Workflow', 'Workflow for software license management', 'Service Request', 'ServiceRequest', 1, 1, 'Shield', '#8b5cf6', 0, 2, 10, 48, NOW(), 0),
('cloud-sync-resolution', 'Cloud Sync Resolution Workflow', 'Workflow for cloud storage sync issues', 'Service Request', 'ServiceRequest', 1, 1, 'Cloud', '#0ea5e9', 0, 2, 10, 24, NOW(), 0),
('collaboration-tools-support', 'Collaboration Tools Support Workflow', 'Workflow for Teams/Slack/Zoom support', 'Service Request', 'ServiceRequest', 1, 1, 'Users', '#22c55e', 0, 2, 10, 12, NOW(), 0);

-- Security Software Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('security-update', 'Security Update Workflow', 'Workflow for antivirus and security updates', 'Service Request', 'ServiceRequest', 1, 1, 'Shield', '#22c55e', 0, 2, 10, 24, NOW(), 0),
('false-positive', 'False Positive Workflow', 'Workflow for investigating security false positives', 'Service Request', 'ServiceRequest', 1, 1, 'AlertCircle', '#f59e0b', 0, 2, 10, 24, NOW(), 0),
('performance-tuning', 'Performance Tuning Workflow', 'Workflow for security software performance tuning', 'Service Request', 'ServiceRequest', 1, 1, 'Sliders', '#6366f1', 0, 3, 10, 48, NOW(), 0),
('threat-remediation', 'Threat Remediation Workflow', 'Critical workflow for malware and threat remediation', 'Service Request', 'ServiceRequest', 1, 1, 'ShieldAlert', '#dc2626', 0, 1, 20, 4, NOW(), 0),
('license-renewal', 'License Renewal Workflow', 'Workflow for security software license renewal', 'Service Request', 'ServiceRequest', 1, 1, 'RefreshCcw', '#8b5cf6', 0, 3, 10, 168, NOW(), 0);

-- Business Application Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('application-performance', 'Application Performance Workflow', 'Workflow for business application performance issues', 'Service Request', 'ServiceRequest', 1, 1, 'Activity', '#f97316', 0, 2, 10, 24, NOW(), 0),
('data-integration-support', 'Data Integration Support Workflow', 'Workflow for data import/export and integration issues', 'Service Request', 'ServiceRequest', 1, 1, 'GitMerge', '#8b5cf6', 0, 2, 10, 48, NOW(), 0),
('user-access', 'User Access Workflow', 'Workflow for application access and permissions', 'Service Request', 'ServiceRequest', 1, 1, 'UserCheck', '#22c55e', 0, 2, 10, 12, NOW(), 0),
('report-development', 'Report Development Workflow', 'Workflow for custom report development requests', 'Service Request', 'ServiceRequest', 1, 1, 'BarChart2', '#3b82f6', 0, 3, 5, 168, NOW(), 0),
('integration-support', 'Integration Support Workflow', 'Workflow for third-party integration support', 'Service Request', 'ServiceRequest', 1, 1, 'Link', '#6366f1', 0, 2, 10, 48, NOW(), 0);

-- Backup and Recovery Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('backup-troubleshooting', 'Backup Troubleshooting Workflow', 'Workflow for backup failure investigation', 'Service Request', 'ServiceRequest', 1, 1, 'Archive', '#ef4444', 0, 2, 10, 12, NOW(), 0),
('data-restoration', 'Data Restoration Workflow', 'Workflow for data recovery and restoration requests', 'Service Request', 'ServiceRequest', 1, 1, 'RotateCcw', '#dc2626', 0, 1, 10, 8, NOW(), 0),
('backup-optimization', 'Backup Optimization Workflow', 'Workflow for backup performance optimization', 'Service Request', 'ServiceRequest', 1, 1, 'Gauge', '#22c55e', 0, 3, 10, 72, NOW(), 0),
('storage-management', 'Storage Management Workflow', 'Workflow for backup storage management', 'Service Request', 'ServiceRequest', 1, 1, 'Database', '#0ea5e9', 0, 3, 10, 48, NOW(), 0);

-- Cloud Services Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('vm-recovery', 'VM Recovery Workflow', 'Workflow for cloud VM recovery and restoration', 'Service Request', 'ServiceRequest', 1, 1, 'Cloud', '#ef4444', 0, 1, 10, 4, NOW(), 0),
('cost-optimization', 'Cost Optimization Workflow', 'Workflow for cloud cost analysis and optimization', 'Service Request', 'ServiceRequest', 1, 1, 'DollarSign', '#22c55e', 0, 3, 5, 168, NOW(), 0),
('resource-adjustment', 'Resource Adjustment Workflow', 'Workflow for cloud resource scaling and adjustment', 'Service Request', 'ServiceRequest', 1, 1, 'Sliders', '#8b5cf6', 0, 2, 10, 24, NOW(), 0),
('cloud-backup', 'Cloud Backup Workflow', 'Workflow for cloud backup configuration and issues', 'Service Request', 'ServiceRequest', 1, 1, 'CloudUpload', '#3b82f6', 0, 2, 10, 48, NOW(), 0);

-- Database Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('database-connectivity', 'Database Connectivity Workflow', 'Workflow for database connection issues', 'Service Request', 'ServiceRequest', 1, 1, 'Database', '#ef4444', 0, 1, 10, 4, NOW(), 0),
('database-optimization', 'Database Optimization Workflow', 'Workflow for database performance optimization', 'Service Request', 'ServiceRequest', 1, 1, 'Zap', '#22c55e', 0, 2, 5, 72, NOW(), 0),
('database-migration', 'Database Migration Workflow', 'Workflow for database migration projects', 'Service Request', 'ServiceRequest', 1, 1, 'ArrowRightLeft', '#8b5cf6', 0, 2, 3, 336, NOW(), 0);

-- SaaS Application Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('user-management', 'User Management Workflow', 'Workflow for SaaS user provisioning and management', 'Service Request', 'ServiceRequest', 1, 1, 'Users', '#3b82f6', 0, 2, 10, 24, NOW(), 0),
('feature-support', 'Feature Support Workflow', 'Workflow for SaaS feature questions and support', 'Service Request', 'ServiceRequest', 1, 1, 'HelpCircle', '#22c55e', 0, 3, 10, 48, NOW(), 0),
('data-export', 'Data Export Workflow', 'Workflow for SaaS data export requests', 'Service Request', 'ServiceRequest', 1, 1, 'Download', '#6366f1', 0, 3, 10, 72, NOW(), 0);

-- VPN and Remote Access Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('vpn-troubleshooting', 'VPN Troubleshooting Workflow', 'Workflow for VPN connection troubleshooting', 'Service Request', 'ServiceRequest', 1, 1, 'Lock', '#3b82f6', 0, 2, 10, 12, NOW(), 0),
('hybrid-sync', 'Hybrid Sync Workflow', 'Workflow for hybrid cloud sync issues', 'Service Request', 'ServiceRequest', 1, 1, 'RefreshCw', '#8b5cf6', 0, 2, 10, 24, NOW(), 0);

-- Monitoring and Management Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('scheduled-maintenance', 'Scheduled Maintenance Workflow', 'Workflow for planned maintenance activities', 'Service Request', 'ServiceRequest', 1, 1, 'Calendar', '#6366f1', 0, 3, 10, 168, NOW(), 0),
('device-onboarding', 'Device Onboarding Workflow', 'Workflow for new device monitoring setup', 'Service Request', 'ServiceRequest', 1, 1, 'Plus', '#22c55e', 0, 3, 10, 48, NOW(), 0),
('alert-tuning', 'Alert Tuning Workflow', 'Workflow for monitoring alert threshold adjustment', 'Service Request', 'ServiceRequest', 1, 1, 'Bell', '#f59e0b', 0, 3, 10, 72, NOW(), 0);

-- Security Services Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('security-incident-response', 'Security Incident Response Workflow', 'Critical workflow for security incident response', 'Service Request', 'ServiceRequest', 1, 1, 'AlertOctagon', '#dc2626', 0, 1, 20, 2, NOW(), 0),
('firewall-change', 'Firewall Change Workflow', 'Workflow for firewall rule change requests', 'Service Request', 'ServiceRequest', 1, 1, 'Shield', '#f97316', 0, 2, 10, 24, NOW(), 0),
('security-reporting', 'Security Reporting Workflow', 'Workflow for security report generation and analysis', 'Service Request', 'ServiceRequest', 1, 1, 'FileText', '#8b5cf6', 0, 3, 10, 72, NOW(), 0);

-- Cloud Management Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('cloud-cost-management', 'Cloud Cost Management Workflow', 'Workflow for cloud cost monitoring and optimization', 'Service Request', 'ServiceRequest', 1, 1, 'DollarSign', '#22c55e', 0, 3, 5, 168, NOW(), 0),
('architecture-review', 'Architecture Review Workflow', 'Workflow for cloud architecture review and recommendations', 'Service Request', 'ServiceRequest', 1, 1, 'Compass', '#6366f1', 0, 3, 3, 336, NOW(), 0);

-- End User Support Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('password-reset', 'Password Reset Workflow', 'Workflow for password reset requests', 'Service Request', 'ServiceRequest', 1, 1, 'Key', '#3b82f6', 0, 2, 50, 2, NOW(), 0),
('software-installation', 'Software Installation Workflow', 'Workflow for software installation requests', 'Service Request', 'ServiceRequest', 1, 1, 'Download', '#22c55e', 0, 3, 10, 24, NOW(), 0),
('performance-support', 'Performance Support Workflow', 'Workflow for workstation performance issues', 'Service Request', 'ServiceRequest', 1, 1, 'Activity', '#f59e0b', 0, 2, 10, 24, NOW(), 0),
('email-configuration', 'Email Configuration Workflow', 'Workflow for email account setup and configuration', 'Service Request', 'ServiceRequest', 1, 1, 'Mail', '#0ea5e9', 0, 3, 10, 12, NOW(), 0),
('user-training-support', 'User Training Support Workflow', 'Workflow for user training and onboarding', 'Service Request', 'ServiceRequest', 1, 1, 'GraduationCap', '#8b5cf6', 0, 4, 10, 168, NOW(), 0);

-- On-Site Support Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('on-site-installation', 'On-Site Installation Workflow', 'Workflow for on-site hardware installation', 'Service Request', 'ServiceRequest', 1, 1, 'MapPin', '#3b82f6', 0, 2, 5, 48, NOW(), 0),
('on-site-network', 'On-Site Network Workflow', 'Workflow for on-site network troubleshooting', 'Service Request', 'ServiceRequest', 1, 1, 'Network', '#f97316', 0, 2, 5, 24, NOW(), 0),
('remote-access', 'Remote Access Workflow', 'Workflow for setting up remote access', 'Service Request', 'ServiceRequest', 1, 1, 'ExternalLink', '#22c55e', 0, 3, 10, 24, NOW(), 0);

-- Security Assessment Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('vulnerability-assessment', 'Vulnerability Assessment Workflow', 'Workflow for vulnerability scan and assessment', 'Service Request', 'ServiceRequest', 1, 1, 'Search', '#f59e0b', 0, 2, 5, 168, NOW(), 0),
('compliance-audit', 'Compliance Audit Workflow', 'Workflow for compliance audit support', 'Service Request', 'ServiceRequest', 1, 1, 'ClipboardCheck', '#6366f1', 0, 2, 3, 336, NOW(), 0),
('pentest-scheduling', 'Pentest Scheduling Workflow', 'Workflow for penetration test scheduling', 'Service Request', 'ServiceRequest', 1, 1, 'Target', '#dc2626', 0, 2, 2, 504, NOW(), 0);

-- Incident Response Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('breach-response', 'Breach Response Workflow', 'Critical workflow for data breach response', 'Service Request', 'ServiceRequest', 1, 1, 'AlertOctagon', '#dc2626', 0, 1, 20, 2, NOW(), 0),
('ransomware-response', 'Ransomware Response Workflow', 'Critical workflow for ransomware attack response', 'Service Request', 'ServiceRequest', 1, 1, 'Lock', '#ef4444', 0, 1, 20, 2, NOW(), 0);

-- Telecommunications Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('voip-troubleshooting', 'VoIP Troubleshooting Workflow', 'Workflow for VoIP phone system issues', 'Service Request', 'ServiceRequest', 1, 1, 'Phone', '#3b82f6', 0, 2, 10, 12, NOW(), 0),
('call-quality', 'Call Quality Workflow', 'Workflow for call quality investigation', 'Service Request', 'ServiceRequest', 1, 1, 'Volume2', '#f59e0b', 0, 2, 10, 24, NOW(), 0),
('user-provisioning', 'User Provisioning Workflow', 'Workflow for phone user provisioning', 'Service Request', 'ServiceRequest', 1, 1, 'UserPlus', '#22c55e', 0, 3, 10, 24, NOW(), 0);

-- Cabling and Infrastructure Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('cabling-installation', 'Cabling Installation Workflow', 'Workflow for new cabling installation', 'Service Request', 'ServiceRequest', 1, 1, 'Cable', '#6366f1', 0, 3, 5, 168, NOW(), 0),
('cable-testing', 'Cable Testing Workflow', 'Workflow for cable testing and certification', 'Service Request', 'ServiceRequest', 1, 1, 'CheckCircle', '#22c55e', 0, 3, 10, 48, NOW(), 0);

-- Data and Analytics Workflows
INSERT INTO WorkflowDefinitions (WorkflowKey, Name, Description, Category, EntityType, Status, CurrentVersion, IconName, Color, IsSystem, Priority, MaxConcurrentInstances, DefaultTimeoutHours, CreatedAt, IsDeleted) VALUES
('report-troubleshooting', 'Report Troubleshooting Workflow', 'Workflow for BI report troubleshooting', 'Service Request', 'ServiceRequest', 1, 1, 'FileText', '#f97316', 0, 2, 10, 24, NOW(), 0),
('dashboard-development', 'Dashboard Development Workflow', 'Workflow for dashboard development requests', 'Service Request', 'ServiceRequest', 1, 1, 'LayoutDashboard', '#8b5cf6', 0, 3, 5, 168, NOW(), 0),
('etl-troubleshooting', 'ETL Troubleshooting Workflow', 'Workflow for ETL and data pipeline issues', 'Service Request', 'ServiceRequest', 1, 1, 'GitBranch', '#ef4444', 0, 2, 10, 24, NOW(), 0),
('data-quality', 'Data Quality Workflow', 'Workflow for data quality investigation', 'Service Request', 'ServiceRequest', 1, 1, 'CheckSquare', '#22c55e', 0, 2, 10, 48, NOW(), 0);

-- Note: ServiceRequestTypes.WorkflowName is a text field that references workflows by name
-- The workflows above can be looked up by name when processing service requests
