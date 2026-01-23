-- ============================================================================
-- CRM Solution Database Seed Data - Service Request Types (Master Data)
-- Version: 1.0
-- Date: 2026-01-23
-- Description: Service request categories, subcategories, and types
-- ============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- Clear existing service request data
DELETE FROM ServiceRequestTypes WHERE IsDeleted = 0;
DELETE FROM ServiceRequestSubcategories WHERE IsDeleted = 0;
DELETE FROM ServiceRequestCategories WHERE IsDeleted = 0;

-- ----------------------------------------------------------------------------
-- Service Request Categories
-- ----------------------------------------------------------------------------
INSERT INTO ServiceRequestCategories (Id, Name, Description, DisplayOrder, IsActive, IconName, ColorCode, DefaultResponseTimeHours, DefaultResolutionTimeHours, CreatedAt, IsDeleted) VALUES
(1, 'Hardware', 'Hardware repair, installation, upgrade, and maintenance services', 1, 1, 'Computer', '#1976D2', 4, 24, NOW(), 0),
(2, 'Software', 'Software installation, configuration, troubleshooting, and licensing', 2, 1, 'Apps', '#4CAF50', 2, 8, NOW(), 0),
(3, 'Cloud Services', 'Cloud deployment, migration, IaaS, PaaS, SaaS, and hybrid cloud services', 3, 1, 'Cloud', '#9C27B0', 4, 24, NOW(), 0),
(4, 'Managed Services', 'Ongoing managed IT, security, and cloud services', 4, 1, 'ManageAccounts', '#FF9800', 4, 48, NOW(), 0),
(5, 'Support & Maintenance', 'Help desk, on-site, and remote support services', 5, 1, 'SupportAgent', '#00BCD4', 1, 4, NOW(), 0),
(6, 'Security Services', 'Security assessments, penetration testing, and incident response', 6, 1, 'Security', '#F44336', 2, 24, NOW(), 0),
(7, 'Specialized Services', 'VoIP, unified communications, and cabling services', 7, 1, 'SettingsPhone', '#795548', 4, 48, NOW(), 0),
(8, 'Data & Analytics', 'Business intelligence, data warehousing, and analytics services', 8, 1, 'Analytics', '#607D8B', 4, 48, NOW(), 0);

-- ----------------------------------------------------------------------------
-- Service Request Subcategories
-- ----------------------------------------------------------------------------
INSERT INTO ServiceRequestSubcategories (Id, CategoryId, Name, Description, DisplayOrder, IsActive, DefaultResponseTimeHours, DefaultResolutionTimeHours, CreatedAt, IsDeleted) VALUES
-- Hardware Subcategories
(1, 1, 'End-User Computing', 'End-User Computing services under Hardware', 1, 1, 4, 24, NOW(), 0),
(2, 1, 'Networking Equipment', 'Networking Equipment services under Hardware', 2, 1, 2, 16, NOW(), 0),
(3, 1, 'Server Infrastructure', 'Server Infrastructure services under Hardware', 3, 1, 1, 8, NOW(), 0),
(4, 1, 'Data Center Equipment', 'Data Center Equipment services under Hardware', 4, 1, 1, 8, NOW(), 0),
(5, 1, 'Mobile & Telecommunications', 'Mobile & Telecommunications services under Hardware', 5, 1, 4, 24, NOW(), 0),
(6, 1, 'Printing & Imaging', 'Printing & Imaging services under Hardware', 6, 1, 4, 24, NOW(), 0),
-- Software Subcategories
(7, 2, 'Operating Systems', 'Operating Systems services under Software', 1, 1, 2, 8, NOW(), 0),
(8, 2, 'Productivity & Collaboration', 'Productivity & Collaboration services under Software', 2, 1, 2, 8, NOW(), 0),
(9, 2, 'Security Software', 'Security Software services under Software', 3, 1, 1, 4, NOW(), 0),
(10, 2, 'Business Applications', 'Business Applications services under Software', 4, 1, 4, 24, NOW(), 0),
(11, 2, 'Backup & Recovery', 'Backup & Recovery services under Software', 5, 1, 2, 8, NOW(), 0),
-- Cloud Services Subcategories
(12, 3, 'IaaS', 'IaaS services under Cloud Services', 1, 1, 4, 24, NOW(), 0),
(13, 3, 'PaaS', 'PaaS services under Cloud Services', 2, 1, 4, 24, NOW(), 0),
(14, 3, 'SaaS', 'SaaS services under Cloud Services', 3, 1, 2, 8, NOW(), 0),
(15, 3, 'Hybrid & Multi-Cloud', 'Hybrid & Multi-Cloud services under Cloud Services', 4, 1, 4, 48, NOW(), 0),
-- Managed Services Subcategories
(16, 4, 'Managed IT Services', 'Managed IT Services services under Managed Services', 1, 1, 4, 24, NOW(), 0),
(17, 4, 'Managed Security Services', 'Managed Security Services services under Managed Services', 2, 1, 1, 8, NOW(), 0),
(18, 4, 'Managed Cloud Services', 'Managed Cloud Services services under Managed Services', 3, 1, 4, 24, NOW(), 0),
-- Support & Maintenance Subcategories
(19, 5, 'Help Desk Support', 'Help Desk Support services under Support & Maintenance', 1, 1, 1, 4, NOW(), 0),
(20, 5, 'On-Site Support', 'On-Site Support services under Support & Maintenance', 2, 1, 4, 8, NOW(), 0),
(21, 5, 'Remote Support', 'Remote Support services under Support & Maintenance', 3, 1, 1, 2, NOW(), 0),
-- Security Services Subcategories
(22, 6, 'Security Assessments', 'Security Assessments services under Security Services', 1, 1, 24, 120, NOW(), 0),
(23, 6, 'Penetration Testing', 'Penetration Testing services under Security Services', 2, 1, 24, 120, NOW(), 0),
(24, 6, 'Incident Response', 'Incident Response services under Security Services', 3, 1, 1, 4, NOW(), 0),
-- Specialized Services Subcategories
(25, 7, 'VoIP & UC', 'VoIP & UC services under Specialized Services', 1, 1, 4, 24, NOW(), 0),
(26, 7, 'Cabling Services', 'Cabling Services services under Specialized Services', 2, 1, 24, 72, NOW(), 0),
-- Data & Analytics Subcategories
(27, 8, 'Business Intelligence', 'Business Intelligence services under Data & Analytics', 1, 1, 8, 48, NOW(), 0),
(28, 8, 'Data Warehousing', 'Data Warehousing services under Data & Analytics', 2, 1, 8, 48, NOW(), 0);

-- ----------------------------------------------------------------------------
-- Service Request Types - Hardware (Category 1)
-- ----------------------------------------------------------------------------
INSERT INTO ServiceRequestTypes (Id, Name, CategoryId, SubcategoryId, RequestType, DetailedDescription, WorkflowName, PossibleResolutions, FinalCustomerResolutions, DisplayOrder, IsActive, DefaultPriority, ResponseTimeHours, ResolutionTimeHours, Tags, CreatedAt, IsDeleted) VALUES
-- End-User Computing (Subcategory 1)
(1, 'Device Not Powering On', 1, 1, 'Complaint', 'Customer reports desktop/laptop will not power on or boot up', 'Hardware Troubleshooting Workflow', 'Check power supply;Replace power adapter;Hardware component failure;Send for repair', 'Device repaired;Device replaced;Temporary replacement provided', 1, 1, 2, 4, 24, 'hardware,power,desktop,laptop', NOW(), 0),
(2, 'Performance Degradation', 1, 1, 'Complaint', 'Device running slower than expected or experiencing lag', 'Performance Optimization Workflow', 'RAM upgrade;Storage upgrade;Malware removal;OS reinstall;Hardware replacement', 'Performance issue resolved;Hardware upgraded;Device replaced', 1, 1, 1, 4, 24, 'performance,slow,hardware', NOW(), 0),
(3, 'Screen/Display Issues', 1, 1, 'Complaint', 'Broken screen cracked display flickering or no display output', 'Display Repair Workflow', 'Screen replacement;Cable repair;Graphics card replacement;External monitor provided', 'Display issue resolved;Screen replaced', 1, 1, 1, 4, 24, 'display,screen,monitor', NOW(), 0),
(4, 'Peripheral Connection Issues', 1, 1, 'Request', 'Keyboard mouse or other peripheral not connecting or working properly', 'Peripheral Support Workflow', 'Driver update;Device replacement;Port repair;Configuration change', 'Peripheral issue resolved;Device replaced', 1, 1, 0, 4, 8, 'peripheral,keyboard,mouse', NOW(), 0),
(5, 'Hardware Upgrade Request', 1, 1, 'Request', 'Customer requests RAM storage or component upgrades', 'Hardware Upgrade Workflow', 'RAM upgrade installed;Storage upgrade completed;Component replaced', 'Upgrade completed successfully', 1, 1, 0, 8, 48, 'upgrade,ram,storage', NOW(), 0),
-- Networking Equipment (Subcategory 2)
(6, 'Network Connectivity Loss', 1, 2, 'Complaint', 'Network devices offline or intermittent connectivity issues', 'Network Outage Resolution Workflow', 'Device reboot;Cable replacement;Port configuration;Device replacement', 'Connectivity restored', 1, 1, 2, 2, 8, 'network,connectivity,outage', NOW(), 0),
(7, 'Slow Network Performance', 1, 2, 'Complaint', 'Network speeds below expected throughput or high latency', 'Network Performance Workflow', 'Bandwidth optimization;QoS configuration;Hardware upgrade;Cable replacement', 'Network performance optimized', 1, 1, 1, 4, 16, 'network,speed,performance', NOW(), 0),
(8, 'Configuration Change Request', 1, 2, 'Request', 'Customer needs firewall rules VLANs or routing changes', 'Network Configuration Workflow', 'Firewall rule updated;VLAN configured;Routing change applied', 'Configuration change completed', 1, 1, 0, 4, 8, 'network,configuration,firewall', NOW(), 0),
(9, 'Firmware Update Required', 1, 2, 'Request', 'Device firmware is outdated and needs updating', 'Firmware Update Workflow', 'Firmware updated;Device tested;Rollback if needed', 'Firmware update completed', 1, 1, 0, 8, 24, 'firmware,update,network', NOW(), 0),
(10, 'Port/Interface Failure', 1, 2, 'Complaint', 'Network port or interface not functioning', 'Hardware Port Repair Workflow', 'Port replacement;Module replacement;Device replacement', 'Port/interface repaired or replaced', 1, 1, 2, 2, 16, 'port,interface,hardware', NOW(), 0),
-- Server Infrastructure (Subcategory 3)
(11, 'Server Crash/Downtime', 1, 3, 'Complaint', 'Server unexpectedly down or crashed requiring immediate attention', 'Critical Server Recovery Workflow', 'Server restart;Failover activation;Hardware replacement;Data recovery', 'Server restored to operational state', 1, 1, 3, 1, 4, 'server,crash,downtime,critical', NOW(), 0),
(12, 'High CPU/Memory Usage', 1, 3, 'Complaint', 'Server experiencing resource exhaustion impacting performance', 'Resource Optimization Workflow', 'Process termination;Resource reallocation;Service optimization;Hardware upgrade', 'Resource usage normalized', 1, 1, 2, 2, 8, 'server,cpu,memory,performance', NOW(), 0),
(13, 'Storage Capacity Warning', 1, 3, 'Request', 'Server storage reaching capacity threshold', 'Storage Expansion Workflow', 'Cleanup old data;Add storage;Archive data;Compression enabled', 'Storage capacity issue resolved', 1, 1, 1, 4, 24, 'storage,capacity,server', NOW(), 0),
(14, 'RAID Array Degraded', 1, 3, 'Complaint', 'RAID array showing degraded status or disk failure', 'RAID Recovery Workflow', 'Disk replacement;RAID rebuild;Data verification;Backup restore', 'RAID array restored', 1, 1, 2, 2, 8, 'raid,storage,disk', NOW(), 0),
(15, 'Virtualization Issues', 1, 3, 'Complaint', 'VM performance problems migration failures or hypervisor errors', 'Virtualization Support Workflow', 'VM restart;Resource reallocation;Migration retry;Hypervisor patch', 'Virtualization issue resolved', 1, 1, 2, 2, 16, 'vm,virtualization,hypervisor', NOW(), 0),
-- Data Center Equipment (Subcategory 4)
(16, 'Cooling System Alert', 1, 4, 'Complaint', 'Temperature warnings or HVAC system failures in data center', 'Environmental Alert Workflow', 'HVAC repair;Fan replacement;Airflow optimization;Emergency cooling', 'Temperature issue resolved', 1, 1, 3, 1, 4, 'cooling,temperature,datacenter', NOW(), 0),
(17, 'Power Distribution Issue', 1, 4, 'Complaint', 'PDU failure power redundancy loss or circuit overload', 'Power System Recovery Workflow', 'PDU repair;Load balancing;Circuit replacement;Redundancy restoration', 'Power distribution restored', 1, 1, 3, 1, 4, 'power,pdu,datacenter', NOW(), 0),
(18, 'UPS Battery Replacement', 1, 4, 'Request', 'UPS batteries reaching end of life or failed battery test', 'UPS Maintenance Workflow', 'Battery replacement;Load test;Monitoring update', 'UPS batteries replaced', 1, 1, 1, 24, 72, 'ups,battery,power', NOW(), 0),
(19, 'Rack Space Request', 1, 4, 'Request', 'Customer needs additional rack space or reorganization', 'Rack Allocation Workflow', 'Rack space allocated;Equipment relocated;Documentation updated', 'Rack space request fulfilled', 1, 1, 0, 24, 120, 'rack,datacenter,space', NOW(), 0),
-- Mobile & Telecommunications (Subcategory 5)
(20, 'Device Malfunction', 1, 5, 'Complaint', 'Mobile device not functioning properly hardware defects', 'Mobile Device Support Workflow', 'Device repair;Device replacement;Warranty claim', 'Device issue resolved', 1, 1, 1, 4, 24, 'mobile,device,hardware', NOW(), 0),
(21, 'SIM/Connectivity Issues', 1, 5, 'Complaint', 'Cannot connect to cellular network or SIM not recognized', 'Mobile Connectivity Workflow', 'SIM replacement;Network reset;Carrier escalation', 'Connectivity restored', 1, 1, 1, 2, 8, 'sim,mobile,connectivity', NOW(), 0),
(22, 'MDM Enrollment Problem', 1, 5, 'Request', 'Issues enrolling device in mobile device management system', 'MDM Support Workflow', 'Device reset;MDM profile reinstall;Policy update', 'Device enrolled successfully', 1, 1, 0, 4, 8, 'mdm,mobile,enrollment', NOW(), 0),
-- Printing & Imaging (Subcategory 6)
(23, 'Printer Offline/Not Printing', 1, 6, 'Complaint', 'Printer shows offline or jobs stuck in queue', 'Printer Troubleshooting Workflow', 'Driver reinstall;Connection reset;Spooler restart;Device restart', 'Printer online and operational', 1, 1, 1, 2, 4, 'printer,offline,printing', NOW(), 0),
(24, 'Paper Jam/Feed Issues', 1, 6, 'Complaint', 'Repeated paper jams or paper feed mechanism failure', 'Printer Maintenance Workflow', 'Jam cleared;Rollers cleaned;Parts replaced', 'Paper feed issue resolved', 1, 1, 0, 4, 8, 'printer,jam,paper', NOW(), 0),
(25, 'Print Quality Issues', 1, 6, 'Complaint', 'Poor print quality streaks fading or color problems', 'Print Quality Workflow', 'Cleaning cycle;Toner replacement;Print head service', 'Print quality restored', 1, 1, 0, 4, 8, 'printer,quality,print', NOW(), 0),
(26, 'Toner/Ink Replacement', 1, 6, 'Request', 'Customer needs consumables replacement or supplies order', 'Consumables Order Workflow', 'Supplies ordered;Supplies delivered;Supplies installed', 'Consumables replaced', 1, 1, 0, 8, 24, 'toner,ink,consumables', NOW(), 0),
(27, 'Scanner Not Working', 1, 6, 'Complaint', 'Scanner function not operating or producing errors', 'Scanner Support Workflow', 'Driver update;Hardware check;Device replacement', 'Scanner issue resolved', 1, 1, 0, 4, 8, 'scanner,imaging', NOW(), 0);

-- ----------------------------------------------------------------------------
-- Service Request Types - Software (Category 2)
-- ----------------------------------------------------------------------------
INSERT INTO ServiceRequestTypes (Id, Name, CategoryId, SubcategoryId, RequestType, DetailedDescription, WorkflowName, PossibleResolutions, FinalCustomerResolutions, DisplayOrder, IsActive, DefaultPriority, ResponseTimeHours, ResolutionTimeHours, Tags, CreatedAt, IsDeleted) VALUES
-- Operating Systems (Subcategory 7)
(28, 'System Won''t Boot', 2, 7, 'Complaint', 'Operating system fails to start or stuck in boot loop', 'OS Recovery Workflow', 'Startup repair;System restore;OS reinstall;Data recovery', 'System restored to bootable state', 1, 1, 2, 2, 8, 'os,boot,windows,linux', NOW(), 0),
(29, 'Blue Screen/Crash Errors', 2, 7, 'Complaint', 'Frequent system crashes BSOD or kernel panics', 'System Stability Workflow', 'Driver update;Memory test;Disk repair;Clean install', 'System stability restored', 1, 1, 2, 2, 8, 'bsod,crash,windows', NOW(), 0),
(30, 'License Activation Issues', 2, 7, 'Complaint', 'OS not activated or license validation errors', 'License Resolution Workflow', 'License key reactivation;Hardware ID update;Microsoft support escalation', 'License activated', 1, 1, 1, 4, 8, 'license,activation,windows', NOW(), 0),
(31, 'OS Update Request', 2, 7, 'Request', 'Customer needs OS upgraded to newer version', 'OS Upgrade Workflow', 'Backup data;Upgrade OS;Verify applications;User training', 'OS upgrade completed', 1, 1, 0, 8, 24, 'upgrade,os,windows', NOW(), 0),
(32, 'Patch/Update Failures', 2, 7, 'Complaint', 'Windows updates failing or causing system issues', 'Update Troubleshooting Workflow', 'Update retry;Manual patch install;Component reset;Rollback', 'Updates installed successfully', 1, 1, 1, 2, 8, 'update,patch,windows', NOW(), 0),
-- Productivity & Collaboration (Subcategory 8)
(33, 'Email Access Issues', 2, 8, 'Complaint', 'Cannot access email account sync errors or login problems', 'Email Support Workflow', 'Password reset;Profile recreate;Server config update', 'Email access restored', 1, 1, 2, 1, 4, 'email,outlook,access', NOW(), 0),
(34, 'Office Application Crashes', 2, 8, 'Complaint', 'Word Excel PowerPoint crashing frequently or not opening', 'Office Repair Workflow', 'Office repair;Add-in disable;Clean reinstall', 'Office applications working', 1, 1, 1, 2, 8, 'office,crash,word,excel', NOW(), 0),
(35, 'License Addition Request', 2, 8, 'Request', 'Need to add more user licenses or upgrade subscription', 'License Management Workflow', 'License purchased;License assigned;User notified', 'License request fulfilled', 1, 1, 0, 8, 24, 'license,office365,microsoft', NOW(), 0),
(36, 'OneDrive/SharePoint Sync Issues', 2, 8, 'Complaint', 'Files not syncing properly or sync errors occurring', 'Cloud Sync Resolution Workflow', 'Sync reset;Cache clear;Selective sync;Account relink', 'File sync restored', 1, 1, 1, 2, 8, 'onedrive,sharepoint,sync', NOW(), 0),
(37, 'Teams/Meeting Issues', 2, 8, 'Complaint', 'Video conferencing problems audio issues or meeting failures', 'Collaboration Tools Support Workflow', 'App reinstall;Audio driver update;Network optimization', 'Teams/meeting issues resolved', 1, 1, 1, 1, 4, 'teams,meeting,audio,video', NOW(), 0),
-- Security Software (Subcategory 9)
(38, 'Antivirus Not Updating', 2, 9, 'Complaint', 'Security definitions not updating or update failures', 'Security Update Workflow', 'Manual update;Reinstall;Server check;License verify', 'Antivirus updating properly', 1, 1, 2, 1, 4, 'antivirus,update,security', NOW(), 0),
(39, 'False Positive Detection', 2, 9, 'Complaint', 'Legitimate files flagged as threats or quarantined incorrectly', 'False Positive Workflow', 'File exclusion added;Submit to vendor;Restore file', 'False positive resolved', 1, 1, 1, 2, 4, 'antivirus,false positive', NOW(), 0),
(40, 'Performance Impact', 2, 9, 'Complaint', 'Security software causing system slowdown or high resource usage', 'Performance Tuning Workflow', 'Scan schedule optimization;Exclusions added;Software replacement', 'Performance impact minimized', 1, 1, 1, 4, 8, 'antivirus,performance', NOW(), 0),
(41, 'Threat Detection Alert', 2, 9, 'Complaint', 'Malware virus or security threat detected on system', 'Threat Remediation Workflow', 'Malware removal;System scan;Password reset;Data recovery', 'Threat removed and system secured', 1, 1, 3, 1, 4, 'malware,virus,threat', NOW(), 0),
(42, 'License Renewal Required', 2, 9, 'Request', 'Security subscription expiring or expired', 'License Renewal Workflow', 'Renewal processed;License applied;Verification', 'Security license renewed', 1, 1, 1, 8, 24, 'license,renewal,security', NOW(), 0),
-- Business Applications (Subcategory 10)
(43, 'CRM/ERP Performance Issues', 2, 10, 'Complaint', 'Business application running slow or timing out', 'Application Performance Workflow', 'Cache clear;Database optimization;Resource increase', 'Application performance improved', 1, 1, 2, 2, 8, 'crm,erp,performance', NOW(), 0),
(44, 'Data Import/Export Errors', 2, 10, 'Complaint', 'Problems importing or exporting data format errors', 'Data Integration Support Workflow', 'Data format correction;Mapping update;Manual import', 'Data transfer completed', 1, 1, 1, 4, 8, 'data,import,export', NOW(), 0),
(45, 'User Access Request', 2, 10, 'Request', 'New user needs access or permission changes required', 'User Access Workflow', 'Account created;Permissions assigned;User trained', 'User access granted', 1, 1, 0, 4, 8, 'access,user,permissions', NOW(), 0),
(46, 'Custom Report Request', 2, 10, 'Request', 'Customer needs new report or dashboard created', 'Report Development Workflow', 'Requirements gathered;Report developed;Tested and deployed', 'Custom report delivered', 1, 1, 0, 24, 72, 'report,custom,analytics', NOW(), 0),
(47, 'Integration Failure', 2, 10, 'Complaint', 'Third-party integration not working or API errors', 'Integration Support Workflow', 'Connection reset;API key update;Vendor escalation', 'Integration restored', 1, 1, 2, 2, 8, 'integration,api,connection', NOW(), 0),
-- Backup & Recovery (Subcategory 11)
(48, 'Backup Job Failure', 2, 11, 'Complaint', 'Scheduled backup failed or incomplete', 'Backup Troubleshooting Workflow', 'Job restart;Storage check;Schedule update;Media replace', 'Backup job running successfully', 1, 1, 2, 2, 4, 'backup,failure,job', NOW(), 0),
(49, 'Restore Request', 2, 11, 'Request', 'Customer needs files or system restored from backup', 'Data Restoration Workflow', 'Backup identified;Restore executed;Verification', 'Data restored successfully', 1, 1, 1, 2, 8, 'restore,backup,recovery', NOW(), 0),
(50, 'Slow Backup Performance', 2, 11, 'Complaint', 'Backups taking too long or impacting system performance', 'Backup Optimization Workflow', 'Schedule optimization;Incremental setup;Bandwidth limit', 'Backup performance improved', 1, 1, 1, 4, 24, 'backup,performance', NOW(), 0),
(51, 'Storage Quota Exceeded', 2, 11, 'Complaint', 'Backup storage full or approaching capacity limit', 'Storage Management Workflow', 'Old backups purged;Retention update;Storage added', 'Storage capacity issue resolved', 1, 1, 1, 4, 24, 'storage,quota,backup', NOW(), 0);

-- ----------------------------------------------------------------------------
-- Service Request Types - Cloud Services (Category 3)
-- ----------------------------------------------------------------------------
INSERT INTO ServiceRequestTypes (Id, Name, CategoryId, SubcategoryId, RequestType, DetailedDescription, WorkflowName, PossibleResolutions, FinalCustomerResolutions, DisplayOrder, IsActive, DefaultPriority, ResponseTimeHours, ResolutionTimeHours, Tags, CreatedAt, IsDeleted) VALUES
-- IaaS (Subcategory 12)
(52, 'VM Provisioning Request', 3, 12, 'Request', 'Customer needs new virtual machine deployed', 'VM Provisioning Workflow', 'VM specs confirmed;VM deployed;Access provided', 'VM provisioned and ready', 1, 1, 0, 4, 24, 'vm,iaas,cloud', NOW(), 0),
(53, 'VM Performance Issue', 3, 12, 'Complaint', 'Virtual machine running slow or unresponsive', 'Cloud Performance Workflow', 'Resource increase;VM restart;Storage optimization', 'VM performance improved', 1, 1, 2, 2, 8, 'vm,performance,cloud', NOW(), 0),
(54, 'Cloud Storage Request', 3, 12, 'Request', 'Additional cloud storage needed', 'Cloud Storage Workflow', 'Storage provisioned;Access configured;Quotas set', 'Cloud storage allocated', 1, 1, 0, 4, 24, 'storage,cloud,iaas', NOW(), 0),
-- PaaS (Subcategory 13)
(55, 'Container Deployment Issue', 3, 13, 'Complaint', 'Container not starting or crashing', 'Container Support Workflow', 'Container restart;Image rebuild;Resource adjustment', 'Container running properly', 1, 1, 2, 2, 8, 'container,kubernetes,paas', NOW(), 0),
(56, 'Database Service Request', 3, 13, 'Request', 'New database instance needed', 'Database Provisioning Workflow', 'Database provisioned;Access granted;Backup configured', 'Database service ready', 1, 1, 0, 4, 24, 'database,paas,cloud', NOW(), 0),
-- SaaS (Subcategory 14)
(57, 'SaaS Access Issue', 3, 14, 'Complaint', 'Cannot access SaaS application', 'SaaS Support Workflow', 'Account reset;SSO check;Vendor escalation', 'SaaS access restored', 1, 1, 2, 1, 4, 'saas,access,cloud', NOW(), 0),
(58, 'SaaS License Request', 3, 14, 'Request', 'Additional SaaS licenses needed', 'SaaS License Workflow', 'License procured;Account created;User notified', 'SaaS license assigned', 1, 1, 0, 8, 24, 'saas,license,cloud', NOW(), 0),
-- Hybrid & Multi-Cloud (Subcategory 15)
(59, 'Hybrid Connectivity Issue', 3, 15, 'Complaint', 'Connection between on-prem and cloud not working', 'Hybrid Cloud Workflow', 'VPN check;Tunnel reset;Route update', 'Hybrid connectivity restored', 1, 1, 2, 2, 8, 'hybrid,vpn,connectivity', NOW(), 0),
(60, 'Cloud Migration Request', 3, 15, 'Request', 'Migrate workload to cloud', 'Cloud Migration Workflow', 'Assessment;Migration plan;Migration execution;Validation', 'Workload migrated successfully', 1, 1, 0, 24, 168, 'migration,cloud,hybrid', NOW(), 0);

-- ----------------------------------------------------------------------------
-- Service Request Types - Managed Services (Category 4)
-- ----------------------------------------------------------------------------
INSERT INTO ServiceRequestTypes (Id, Name, CategoryId, SubcategoryId, RequestType, DetailedDescription, WorkflowName, PossibleResolutions, FinalCustomerResolutions, DisplayOrder, IsActive, DefaultPriority, ResponseTimeHours, ResolutionTimeHours, Tags, CreatedAt, IsDeleted) VALUES
-- Managed IT Services (Subcategory 16)
(61, 'Proactive Maintenance Request', 4, 16, 'Request', 'Schedule preventive maintenance', 'Maintenance Scheduling Workflow', 'Maintenance scheduled;Work completed;Report provided', 'Maintenance completed', 1, 1, 0, 24, 72, 'maintenance,managed', NOW(), 0),
(62, 'Performance Review Request', 4, 16, 'Request', 'Monthly/quarterly infrastructure review', 'Performance Review Workflow', 'Review scheduled;Analysis completed;Report delivered', 'Review completed', 1, 1, 0, 24, 120, 'review,performance,managed', NOW(), 0),
-- Managed Security Services (Subcategory 17)
(63, 'Security Alert Escalation', 4, 17, 'Complaint', 'Security incident requiring immediate attention', 'Security Incident Workflow', 'Incident contained;Threat removed;Post-mortem', 'Security incident resolved', 1, 1, 3, 1, 4, 'security,incident,managed', NOW(), 0),
(64, 'Security Policy Update', 4, 17, 'Request', 'Update security policies or configurations', 'Security Policy Workflow', 'Policy reviewed;Changes applied;Documentation updated', 'Security policy updated', 1, 1, 0, 8, 24, 'security,policy,managed', NOW(), 0),
-- Managed Cloud Services (Subcategory 18)
(65, 'Cloud Cost Optimization', 4, 18, 'Request', 'Review and optimize cloud spending', 'Cost Optimization Workflow', 'Analysis completed;Recommendations provided;Changes implemented', 'Cost optimization implemented', 1, 1, 0, 24, 120, 'cloud,cost,optimization', NOW(), 0),
(66, 'Cloud Scaling Request', 4, 18, 'Request', 'Scale cloud resources up or down', 'Cloud Scaling Workflow', 'Requirements verified;Resources scaled;Monitoring updated', 'Cloud resources scaled', 1, 1, 0, 4, 24, 'cloud,scaling,managed', NOW(), 0);

-- ----------------------------------------------------------------------------
-- Service Request Types - Support & Maintenance (Category 5)
-- ----------------------------------------------------------------------------
INSERT INTO ServiceRequestTypes (Id, Name, CategoryId, SubcategoryId, RequestType, DetailedDescription, WorkflowName, PossibleResolutions, FinalCustomerResolutions, DisplayOrder, IsActive, DefaultPriority, ResponseTimeHours, ResolutionTimeHours, Tags, CreatedAt, IsDeleted) VALUES
-- Help Desk Support (Subcategory 19)
(67, 'Password Reset Request', 5, 19, 'Request', 'User forgot password or account locked', 'Password Reset Workflow', 'Password reset;Account unlocked;MFA reset if needed', 'Access restored', 1, 1, 0, 1, 2, 'password,reset,access', NOW(), 0),
(68, 'General IT Support Query', 5, 19, 'Request', 'General how-to or technical question', 'IT Support Workflow', 'Question answered;Documentation provided;Training scheduled', 'Query resolved', 1, 1, 0, 2, 4, 'support,query,general', NOW(), 0),
(69, 'New User Setup', 5, 19, 'Request', 'Onboard new employee with IT equipment and accounts', 'User Onboarding Workflow', 'Equipment provisioned;Accounts created;Training completed', 'New user setup complete', 1, 1, 0, 24, 48, 'onboarding,new user,setup', NOW(), 0),
(70, 'User Offboarding', 5, 19, 'Request', 'Disable accounts and retrieve equipment for departing employee', 'User Offboarding Workflow', 'Accounts disabled;Equipment retrieved;Data archived', 'Offboarding completed', 1, 1, 1, 4, 8, 'offboarding,termination', NOW(), 0),
-- On-Site Support (Subcategory 20)
(71, 'On-Site Visit Request', 5, 20, 'Request', 'Technician needs to visit customer location', 'On-Site Scheduling Workflow', 'Visit scheduled;Work completed;Documentation provided', 'On-site visit completed', 1, 1, 0, 24, 48, 'onsite,visit,support', NOW(), 0),
(72, 'Hardware Installation On-Site', 5, 20, 'Request', 'Install hardware at customer location', 'On-Site Installation Workflow', 'Hardware delivered;Installation completed;Testing done', 'Installation completed', 1, 1, 0, 24, 72, 'onsite,installation,hardware', NOW(), 0),
-- Remote Support (Subcategory 21)
(73, 'Remote Session Request', 5, 21, 'Request', 'User needs remote desktop support', 'Remote Support Workflow', 'Remote session initiated;Issue resolved;Session closed', 'Remote support completed', 1, 1, 0, 1, 2, 'remote,support,session', NOW(), 0),
(74, 'Quick Fix Request', 5, 21, 'Request', 'Simple issue that can be resolved quickly', 'Quick Fix Workflow', 'Issue diagnosed;Fix applied;Verification', 'Quick fix completed', 1, 1, 0, 1, 1, 'quick,fix,remote', NOW(), 0);

-- ----------------------------------------------------------------------------
-- Service Request Types - Security Services (Category 6)
-- ----------------------------------------------------------------------------
INSERT INTO ServiceRequestTypes (Id, Name, CategoryId, SubcategoryId, RequestType, DetailedDescription, WorkflowName, PossibleResolutions, FinalCustomerResolutions, DisplayOrder, IsActive, DefaultPriority, ResponseTimeHours, ResolutionTimeHours, Tags, CreatedAt, IsDeleted) VALUES
-- Security Assessments (Subcategory 22)
(75, 'Vulnerability Assessment Request', 6, 22, 'Request', 'Scan infrastructure for vulnerabilities', 'Vulnerability Assessment Workflow', 'Scans completed;Report generated;Remediation guidance', 'Assessment report delivered', 1, 1, 0, 24, 120, 'vulnerability,assessment,security', NOW(), 0),
(76, 'Security Audit Request', 6, 22, 'Request', 'Comprehensive security audit of systems', 'Security Audit Workflow', 'Audit planned;Audit executed;Report delivered', 'Audit completed', 1, 1, 0, 48, 240, 'audit,security,compliance', NOW(), 0),
-- Penetration Testing (Subcategory 23)
(77, 'Penetration Test Request', 6, 23, 'Request', 'Conduct penetration testing of systems', 'Penetration Testing Workflow', 'Scope defined;Testing executed;Report delivered', 'Pen test completed', 1, 1, 0, 48, 240, 'pentest,security,testing', NOW(), 0),
(78, 'Web Application Security Test', 6, 23, 'Request', 'Test web application for security vulnerabilities', 'Web App Security Workflow', 'Application scanned;Vulnerabilities identified;Report delivered', 'Web app test completed', 1, 1, 0, 24, 120, 'webapp,security,testing', NOW(), 0),
-- Incident Response (Subcategory 24)
(79, 'Security Breach Investigation', 6, 24, 'Complaint', 'Investigate potential security breach', 'Breach Investigation Workflow', 'Investigation initiated;Evidence collected;Report provided', 'Investigation completed', 1, 1, 3, 1, 8, 'breach,investigation,security', NOW(), 0),
(80, 'Ransomware Response', 6, 24, 'Complaint', 'System infected with ransomware', 'Ransomware Response Workflow', 'Containment;Eradication;Recovery;Prevention measures', 'Ransomware incident resolved', 1, 1, 4, 1, 4, 'ransomware,incident,critical', NOW(), 0),
(81, 'Phishing Incident Report', 6, 24, 'Complaint', 'User clicked phishing link or submitted credentials', 'Phishing Response Workflow', 'Credentials reset;Account secured;Email blocked', 'Phishing incident contained', 1, 1, 2, 1, 4, 'phishing,email,security', NOW(), 0);

-- ----------------------------------------------------------------------------
-- Service Request Types - Specialized Services (Category 7)
-- ----------------------------------------------------------------------------
INSERT INTO ServiceRequestTypes (Id, Name, CategoryId, SubcategoryId, RequestType, DetailedDescription, WorkflowName, PossibleResolutions, FinalCustomerResolutions, DisplayOrder, IsActive, DefaultPriority, ResponseTimeHours, ResolutionTimeHours, Tags, CreatedAt, IsDeleted) VALUES
-- VoIP & UC (Subcategory 25)
(82, 'Phone System Issue', 7, 25, 'Complaint', 'VoIP phone not working or call quality issues', 'VoIP Troubleshooting Workflow', 'Phone restart;Network check;Codec adjustment', 'Phone system working', 1, 1, 2, 2, 8, 'voip,phone,uc', NOW(), 0),
(83, 'Extension Change Request', 7, 25, 'Request', 'Add, remove, or modify phone extension', 'Extension Management Workflow', 'Extension configured;Testing completed;User notified', 'Extension change completed', 1, 1, 0, 4, 24, 'extension,voip,change', NOW(), 0),
(84, 'Call Routing Update', 7, 25, 'Request', 'Modify call routing or auto attendant', 'Call Routing Workflow', 'Routing updated;Testing completed;Documentation updated', 'Call routing updated', 1, 1, 0, 8, 24, 'routing,voip,attendant', NOW(), 0),
-- Cabling Services (Subcategory 26)
(85, 'Network Cable Installation', 7, 26, 'Request', 'Run new network cables', 'Cabling Installation Workflow', 'Site survey;Installation;Testing;Documentation', 'Cabling installed', 1, 1, 0, 48, 168, 'cabling,network,installation', NOW(), 0),
(86, 'Cable Certification Request', 7, 26, 'Request', 'Test and certify existing cabling', 'Cable Certification Workflow', 'Testing scheduled;Cables tested;Certification report', 'Cables certified', 1, 1, 0, 24, 72, 'cabling,certification,testing', NOW(), 0);

-- ----------------------------------------------------------------------------
-- Service Request Types - Data & Analytics (Category 8)
-- ----------------------------------------------------------------------------
INSERT INTO ServiceRequestTypes (Id, Name, CategoryId, SubcategoryId, RequestType, DetailedDescription, WorkflowName, PossibleResolutions, FinalCustomerResolutions, DisplayOrder, IsActive, DefaultPriority, ResponseTimeHours, ResolutionTimeHours, Tags, CreatedAt, IsDeleted) VALUES
-- Business Intelligence (Subcategory 27)
(87, 'Dashboard Development Request', 8, 27, 'Request', 'Create new BI dashboard', 'Dashboard Development Workflow', 'Requirements gathered;Dashboard built;Training provided', 'Dashboard delivered', 1, 1, 0, 24, 120, 'dashboard,bi,analytics', NOW(), 0),
(88, 'Report Modification Request', 8, 27, 'Request', 'Modify existing report or dashboard', 'Report Modification Workflow', 'Changes documented;Modifications applied;Testing', 'Report modified', 1, 1, 0, 8, 48, 'report,modification,bi', NOW(), 0),
(89, 'Data Visualization Issue', 8, 27, 'Complaint', 'Dashboard or report showing incorrect data', 'Data Validation Workflow', 'Data source checked;Calculations verified;Corrections applied', 'Data issue resolved', 1, 1, 1, 4, 24, 'data,visualization,issue', NOW(), 0),
-- Data Warehousing (Subcategory 28)
(90, 'ETL Job Failure', 8, 28, 'Complaint', 'Data load or transformation job failed', 'ETL Troubleshooting Workflow', 'Error identified;Job restarted;Data verified', 'ETL job successful', 1, 1, 2, 2, 8, 'etl,data,failure', NOW(), 0),
(91, 'New Data Source Integration', 8, 28, 'Request', 'Integrate new data source into warehouse', 'Data Integration Workflow', 'Source analyzed;Integration developed;Testing completed', 'Data source integrated', 1, 1, 0, 24, 120, 'integration,data source,warehouse', NOW(), 0),
(92, 'Data Quality Issue', 8, 28, 'Complaint', 'Data in warehouse is incorrect or inconsistent', 'Data Quality Workflow', 'Issue identified;Root cause analysis;Corrections applied', 'Data quality issue resolved', 1, 1, 1, 4, 24, 'data quality,warehouse', NOW(), 0),
(93, 'Historical Data Request', 8, 28, 'Request', 'Need access to historical data for analysis', 'Historical Data Workflow', 'Data identified;Access granted;Documentation provided', 'Historical data available', 1, 1, 0, 8, 48, 'historical,data,analytics', NOW(), 0);

SET FOREIGN_KEY_CHECKS = 1;
