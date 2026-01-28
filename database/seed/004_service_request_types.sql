-- ============================================================================
-- CRM Solution Database Seed Data - Service Request Types (Master Data)
-- Version: 2.0
-- Date: 2026-01-28
-- Description: Comprehensive service request categories, subcategories, and types
-- ============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- Clear existing service request data
DELETE FROM ServiceRequestTypes WHERE IsDeleted = 0;
DELETE FROM ServiceRequestSubcategories WHERE IsDeleted = 0;
DELETE FROM ServiceRequestCategories WHERE IsDeleted = 0;

-- ============================================================================
-- SERVICE REQUEST CATEGORIES
-- ============================================================================
INSERT INTO ServiceRequestCategories (Id, Name, Description, DisplayOrder, IsActive, IconName, ColorCode, DefaultResponseTimeHours, DefaultResolutionTimeHours, CreatedAt, IsDeleted) VALUES
(1, 'Hardware', 'Hardware repair, installation, upgrade, and maintenance services', 1, 1, 'Computer', '#1976D2', 4, 24, NOW(), 0),
(2, 'Software', 'Software installation, configuration, troubleshooting, and licensing', 2, 1, 'Apps', '#4CAF50', 2, 8, NOW(), 0),
(3, 'Cloud Services', 'Cloud deployment, migration, IaaS, PaaS, SaaS, and hybrid cloud services', 3, 1, 'Cloud', '#9C27B0', 4, 24, NOW(), 0),
(4, 'Consulting & Strategy', 'IT strategy, digital transformation, and technology consulting', 4, 1, 'Lightbulb', '#FF5722', 24, 120, NOW(), 0),
(5, 'Implementation & Integration', 'Software implementation, system integration, and deployment services', 5, 1, 'Build', '#3F51B5', 24, 120, NOW(), 0),
(6, 'Managed Services', 'Ongoing managed IT, security, and cloud services', 6, 1, 'ManageAccounts', '#FF9800', 4, 48, NOW(), 0),
(7, 'Support & Maintenance', 'Help desk, on-site, and remote support services', 7, 1, 'SupportAgent', '#00BCD4', 1, 4, NOW(), 0),
(8, 'Security Services', 'Security assessments, penetration testing, and incident response', 8, 1, 'Security', '#F44336', 2, 24, NOW(), 0),
(9, 'Cloud Services (Professional)', 'Professional cloud migration, management, and optimization services', 9, 1, 'CloudDone', '#673AB7', 8, 72, NOW(), 0),
(10, 'Data & Analytics', 'Business intelligence, data warehousing, and analytics services', 10, 1, 'Analytics', '#607D8B', 8, 72, NOW(), 0),
(11, 'Specialized Services', 'VoIP, unified communications, cabling, and specialized IT services', 11, 1, 'SettingsPhone', '#795548', 4, 48, NOW(), 0);

-- ============================================================================
-- SERVICE REQUEST SUBCATEGORIES
-- ============================================================================
INSERT INTO ServiceRequestSubcategories (Id, CategoryId, Name, Description, DisplayOrder, IsActive, ResponseTimeHours, ResolutionTimeHours, DefaultPriority, CreatedAt, IsDeleted) VALUES
-- Hardware Subcategories (Category 1)
(1, 1, 'End-User Computing', 'Desktop, laptop, and end-user device support', 1, 1, 4, 24, 1, NOW(), 0),
(2, 1, 'Networking Equipment', 'Switches, routers, access points, and network devices', 2, 1, 2, 16, 2, NOW(), 0),
(3, 1, 'Server Infrastructure', 'Physical and virtual server hardware', 3, 1, 1, 8, 2, NOW(), 0),
(4, 1, 'Data Center Equipment', 'Racks, power, cooling, and data center infrastructure', 4, 1, 1, 8, 2, NOW(), 0),
(5, 1, 'Mobile & Telecommunications', 'Mobile devices, phones, and telecom equipment', 5, 1, 4, 24, 1, NOW(), 0),
(6, 1, 'Printing & Imaging', 'Printers, MFPs, scanners, and imaging devices', 6, 1, 4, 24, 0, NOW(), 0),
(7, 1, 'Point of Sale & Specialized', 'POS systems, kiosks, and specialized equipment', 7, 1, 4, 24, 1, NOW(), 0),

-- Software Subcategories (Category 2)
(8, 2, 'Operating Systems', 'Windows, Linux, macOS installation and support', 1, 1, 2, 8, 1, NOW(), 0),
(9, 2, 'Productivity & Collaboration', 'Microsoft 365, Google Workspace, and collaboration tools', 2, 1, 2, 8, 1, NOW(), 0),
(10, 2, 'Security Software', 'Antivirus, EDR, and security applications', 3, 1, 1, 4, 2, NOW(), 0),
(11, 2, 'Business Applications', 'CRM, ERP, and line-of-business applications', 4, 1, 4, 24, 1, NOW(), 0),
(12, 2, 'Development & DevOps', 'Development tools, CI/CD, and DevOps platforms', 5, 1, 4, 24, 1, NOW(), 0),
(13, 2, 'Backup & Recovery', 'Backup software and data recovery solutions', 6, 1, 2, 8, 2, NOW(), 0),
(14, 2, 'Network & Systems Management', 'RMM, PSA, and systems management tools', 7, 1, 4, 16, 1, NOW(), 0),

-- Cloud Services Subcategories (Category 3)
(15, 3, 'Infrastructure as a Service (IaaS)', 'Virtual machines, storage, and cloud infrastructure', 1, 1, 4, 24, 1, NOW(), 0),
(16, 3, 'Platform as a Service (PaaS)', 'Databases, app services, and cloud platforms', 2, 1, 4, 24, 1, NOW(), 0),
(17, 3, 'Software as a Service (SaaS)', 'Cloud-hosted applications and services', 3, 1, 2, 8, 1, NOW(), 0),
(18, 3, 'Hybrid & Multi-Cloud', 'Hybrid connectivity and multi-cloud management', 4, 1, 4, 48, 1, NOW(), 0),
(19, 3, 'Specialized Cloud Services', 'AI, ML, IoT, and specialized cloud services', 5, 1, 8, 72, 0, NOW(), 0),

-- Consulting & Strategy Subcategories (Category 4)
(20, 4, 'IT Strategy & Roadmap Development', 'Strategic IT planning and roadmap development', 1, 1, 24, 120, 0, NOW(), 0),
(21, 4, 'Technology Assessment & Audits', 'IT infrastructure and technology assessments', 2, 1, 24, 120, 0, NOW(), 0),
(22, 4, 'Digital Transformation Consulting', 'Digital transformation strategy and planning', 3, 1, 24, 240, 0, NOW(), 0),
(23, 4, 'Cloud Migration Planning', 'Cloud migration assessment and planning', 4, 1, 24, 120, 0, NOW(), 0),
(24, 4, 'Cybersecurity Consulting', 'Security strategy and architecture consulting', 5, 1, 24, 120, 1, NOW(), 0),
(25, 4, 'Compliance & Regulatory Consulting', 'HIPAA, PCI, SOC2, and compliance consulting', 6, 1, 24, 240, 1, NOW(), 0),
(26, 4, 'Business Continuity Planning', 'BC/DR planning and strategy', 7, 1, 24, 120, 1, NOW(), 0),
(27, 4, 'Vendor Selection & Evaluation', 'Technology vendor evaluation and selection', 8, 1, 24, 72, 0, NOW(), 0),

-- Implementation & Integration Subcategories (Category 5)
(28, 5, 'Software Implementation & Deployment', 'Business software implementation projects', 1, 1, 24, 240, 1, NOW(), 0),
(29, 5, 'System Integration Services', 'Enterprise system integration', 2, 1, 24, 120, 1, NOW(), 0),
(30, 5, 'Data Migration Services', 'Data migration and transformation', 3, 1, 24, 120, 1, NOW(), 0),
(31, 5, 'Custom Application Development', 'Custom software development', 4, 1, 24, 240, 0, NOW(), 0),
(32, 5, 'API Integration', 'API development and integration', 5, 1, 24, 72, 0, NOW(), 0),
(33, 5, 'Legacy System Modernization', 'Legacy application modernization', 6, 1, 48, 480, 0, NOW(), 0),
(34, 5, 'Network Design & Implementation', 'Network infrastructure deployment', 7, 1, 24, 240, 1, NOW(), 0),
(35, 5, 'Infrastructure Deployment', 'IT infrastructure deployment projects', 8, 1, 24, 240, 1, NOW(), 0),

-- Managed Services Subcategories (Category 6)
(36, 6, 'Managed IT Services (MSP)', 'Full-service managed IT support', 1, 1, 4, 24, 1, NOW(), 0),
(37, 6, 'Managed Security Services (MSSP)', 'Managed security and SOC services', 2, 1, 1, 8, 2, NOW(), 0),
(38, 6, 'Managed Network Services', 'Managed network monitoring and support', 3, 1, 2, 16, 1, NOW(), 0),
(39, 6, 'Managed Cloud Services', 'Cloud management and optimization', 4, 1, 4, 24, 1, NOW(), 0),
(40, 6, 'Database Administration', 'Managed database services', 5, 1, 4, 24, 1, NOW(), 0),
(41, 6, 'Application Management', 'Application support and management', 6, 1, 4, 24, 1, NOW(), 0),
(42, 6, 'Desktop as a Service (DaaS)', 'Virtual desktop management', 7, 1, 2, 8, 1, NOW(), 0),
(43, 6, 'Monitoring & Alerting Services', 'Infrastructure monitoring', 8, 1, 1, 4, 2, NOW(), 0),

-- Support & Maintenance Subcategories (Category 7)
(44, 7, 'Help Desk & Technical Support', 'Tier 1/2/3 help desk support', 1, 1, 1, 4, 1, NOW(), 0),
(45, 7, 'On-Site Support Services', 'On-site technical support', 2, 1, 4, 8, 1, NOW(), 0),
(46, 7, 'Remote Support & Troubleshooting', 'Remote technical support', 3, 1, 1, 2, 1, NOW(), 0),
(47, 7, 'Hardware Maintenance & Repair', 'Hardware repair services', 4, 1, 4, 24, 1, NOW(), 0),
(48, 7, 'Software Maintenance & Updates', 'Software patching and maintenance', 5, 1, 4, 24, 0, NOW(), 0),
(49, 7, 'SLA Management', 'Service level agreement management', 6, 1, 8, 48, 0, NOW(), 0),
(50, 7, 'After-Hours & Emergency Support', 'Emergency and after-hours support', 7, 1, 1, 2, 3, NOW(), 0),
(51, 7, 'End-User Training & Documentation', 'User training and documentation', 8, 1, 24, 72, 0, NOW(), 0),

-- Security Services Subcategories (Category 8)
(52, 8, 'Security Assessments & Audits', 'Vulnerability assessments and security audits', 1, 1, 24, 120, 1, NOW(), 0),
(53, 8, 'Penetration Testing & Ethical Hacking', 'Penetration testing services', 2, 1, 48, 240, 0, NOW(), 0),
(54, 8, 'SOC Services', 'Security operations center services', 3, 1, 1, 4, 2, NOW(), 0),
(55, 8, 'Incident Response & Forensics', 'Security incident response and forensics', 4, 1, 1, 8, 3, NOW(), 0),
(56, 8, 'Vulnerability Management', 'Ongoing vulnerability management', 5, 1, 4, 24, 1, NOW(), 0),
(57, 8, 'Compliance Auditing & Remediation', 'Compliance audit and remediation', 6, 1, 24, 240, 1, NOW(), 0),
(58, 8, 'Security Awareness Training', 'Security training programs', 7, 1, 24, 72, 0, NOW(), 0),
(59, 8, 'Security Architecture Design', 'Security architecture consulting', 8, 1, 24, 240, 0, NOW(), 0),

-- Cloud Services (Professional) Subcategories (Category 9)
(60, 9, 'Cloud Migration & Deployment', 'Cloud migration project execution', 1, 1, 24, 240, 1, NOW(), 0),
(61, 9, 'Cloud Infrastructure Management', 'Cloud infrastructure management', 2, 1, 4, 24, 1, NOW(), 0),
(62, 9, 'Cloud Optimization & Cost Management', 'Cloud cost optimization (FinOps)', 3, 1, 8, 72, 0, NOW(), 0),
(63, 9, 'Cloud Security & Compliance', 'Cloud security and compliance', 4, 1, 8, 72, 1, NOW(), 0),
(64, 9, 'Hybrid Cloud Implementation', 'Hybrid cloud deployment', 5, 1, 24, 240, 1, NOW(), 0),
(65, 9, 'Multi-Cloud Strategy & Management', 'Multi-cloud management', 6, 1, 24, 120, 0, NOW(), 0),
(66, 9, 'Cloud Disaster Recovery', 'Cloud-based DR solutions', 7, 1, 8, 72, 1, NOW(), 0),
(67, 9, 'Cloud Application Development', 'Cloud-native application development', 8, 1, 24, 240, 0, NOW(), 0),

-- Data & Analytics Subcategories (Category 10)
(68, 10, 'Data Center Design & Build', 'Data center design and construction', 1, 1, 48, 480, 0, NOW(), 0),
(69, 10, 'Data Analytics & Business Intelligence', 'BI and analytics solutions', 2, 1, 24, 240, 0, NOW(), 0),
(70, 10, 'Data Warehousing Solutions', 'Data warehouse implementation', 3, 1, 24, 240, 0, NOW(), 0),
(71, 10, 'Big Data Platform Implementation', 'Big data platform deployment', 4, 1, 48, 480, 0, NOW(), 0),
(72, 10, 'AI & Machine Learning Implementation', 'AI/ML solution development', 5, 1, 48, 480, 0, NOW(), 0),
(73, 10, 'Data Governance Consulting', 'Data governance framework', 6, 1, 24, 240, 0, NOW(), 0),
(74, 10, 'Master Data Management', 'MDM implementation', 7, 1, 24, 240, 0, NOW(), 0),
(75, 10, 'Data Quality & Cleansing Services', 'Data quality improvement', 8, 1, 24, 120, 0, NOW(), 0),

-- Specialized Services Subcategories (Category 11)
(76, 11, 'VoIP & Unified Communications', 'VoIP and UC services', 1, 1, 4, 24, 1, NOW(), 0),
(77, 11, 'Video Conferencing Setup & Support', 'Video conferencing solutions', 2, 1, 8, 48, 0, NOW(), 0),
(78, 11, 'Wireless Site Surveys & Design', 'Wireless network design', 3, 1, 24, 72, 0, NOW(), 0),
(79, 11, 'Cabling & Infrastructure Services', 'Network cabling services', 4, 1, 24, 120, 0, NOW(), 0),
(80, 11, 'Asset Tracking & Management', 'IT asset management', 5, 1, 8, 48, 0, NOW(), 0),
(81, 11, 'IT Procurement Services', 'Technology procurement', 6, 1, 24, 72, 0, NOW(), 0),
(82, 11, 'Technology Lifecycle Management', 'Tech refresh and lifecycle', 7, 1, 8, 48, 0, NOW(), 0),
(83, 11, 'Green IT & Sustainability Consulting', 'Sustainability consulting', 8, 1, 48, 240, 0, NOW(), 0);

-- ============================================================================
-- SERVICE REQUEST TYPES
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Hardware - End-User Computing (Subcategory 1)
-- ----------------------------------------------------------------------------
INSERT INTO ServiceRequestTypes (Id, Name, CategoryId, SubcategoryId, RequestType, DetailedDescription, WorkflowName, PossibleResolutions, FinalCustomerResolutions, DisplayOrder, IsActive, DefaultPriority, ResponseTimeHours, ResolutionTimeHours, Tags, CreatedAt, IsDeleted) VALUES
(1, 'Device Not Powering On', 1, 1, 'Complaint', 'Customer reports desktop/laptop will not power on or boot up', 'Hardware Troubleshooting Workflow', 'Check power cable and outlet;Test with different power adapter;Replace power supply;Replace motherboard;Device replacement', 'Device repaired on-site;Replacement device shipped;Loaner device provided', 1, 1, 2, 4, 24, 'hardware,power,desktop,laptop', NOW(), 0),
(2, 'Performance Degradation', 1, 1, 'Complaint', 'Device running slower than expected or experiencing lag', 'Performance Optimization Workflow', 'Run diagnostics;Clean temporary files;Upgrade RAM;Replace HDD with SSD;Reinstall OS;Device replacement', 'Device optimized and performance restored;Hardware upgraded;Replacement issued', 2, 1, 1, 4, 24, 'performance,slow,hardware', NOW(), 0),
(3, 'Screen/Display Issues', 1, 1, 'Complaint', 'Broken screen cracked display flickering or no display output', 'Display Repair Workflow', 'Replace screen;Update graphics drivers;Replace graphics card;Test external monitor;Full device replacement', 'Screen replaced;Device replaced;Temporary workaround with external monitor', 3, 1, 1, 4, 24, 'display,screen,monitor', NOW(), 0),
(4, 'Peripheral Connection Issues', 1, 1, 'Request', 'Keyboard mouse or other peripheral not connecting or working properly', 'Peripheral Support Workflow', 'Update drivers;Test on different USB port;Replace peripheral;Clean connection ports;BIOS/firmware update', 'Peripheral replaced;Driver updated;Issue resolved through configuration', 4, 1, 0, 4, 8, 'peripheral,keyboard,mouse,usb', NOW(), 0),
(5, 'Hardware Upgrade Request', 1, 1, 'Request', 'Customer requests RAM storage or component upgrades', 'Hardware Upgrade Workflow', 'Verify compatibility;Order components;Schedule installation;Perform upgrade;Test and validate', 'Upgrade completed and tested;Alternative solution provided if incompatible', 5, 1, 0, 8, 48, 'upgrade,ram,storage,hardware', NOW(), 0),

-- Hardware - Networking Equipment (Subcategory 2)
(6, 'Network Connectivity Loss', 1, 2, 'Complaint', 'Network devices offline or intermittent connectivity issues', 'Network Outage Resolution Workflow', 'Reboot device;Check cable connections;Update firmware;Replace faulty hardware;Reconfigure network settings', 'Connectivity restored;Faulty device replaced;Configuration corrected', 1, 1, 3, 1, 4, 'network,connectivity,outage', NOW(), 0),
(7, 'Slow Network Performance', 1, 2, 'Complaint', 'Network speeds below expected throughput or high latency', 'Network Performance Workflow', 'Run speed tests;Check bandwidth utilization;Update firmware;Replace cables;Upgrade hardware;Optimize QoS settings', 'Performance optimized;Hardware upgraded;Configuration tuned', 2, 1, 2, 2, 16, 'network,speed,performance,latency', NOW(), 0),
(8, 'Configuration Change Request', 1, 2, 'Request', 'Customer needs firewall rules VLANs or routing changes', 'Network Configuration Workflow', 'Review request;Create change plan;Implement during maintenance window;Test and validate;Document changes', 'Configuration successfully updated;Rollback if issues detected', 3, 1, 1, 4, 8, 'network,configuration,firewall,vlan', NOW(), 0),
(9, 'Firmware Update Required', 1, 2, 'Request', 'Device firmware is outdated and needs updating', 'Firmware Update Workflow', 'Backup current config;Download latest firmware;Schedule maintenance window;Apply update;Verify functionality', 'Firmware updated successfully;Rollback to previous version if needed', 4, 1, 0, 8, 24, 'firmware,update,network,security', NOW(), 0),
(10, 'Port/Interface Failure', 1, 2, 'Complaint', 'Network port or interface not functioning', 'Hardware Port Repair Workflow', 'Test with different cable;Update drivers;Replace network module;Replace entire device;Reconfigure redundant path', 'Port repaired;Device replaced;Traffic rerouted to backup interface', 5, 1, 2, 2, 16, 'port,interface,network,hardware', NOW(), 0),

-- Hardware - Server Infrastructure (Subcategory 3)
(11, 'Server Crash/Downtime', 1, 3, 'Complaint', 'Server unexpectedly down or crashed requiring immediate attention', 'Critical Server Recovery Workflow', 'Check hardware status;Review system logs;Reboot server;Replace failed components;Restore from backup;Failover to secondary', 'Server restored to operation;Root cause identified and remediated', 1, 1, 4, 1, 4, 'server,crash,downtime,critical', NOW(), 0),
(12, 'High CPU/Memory Usage', 1, 3, 'Complaint', 'Server experiencing resource exhaustion impacting performance', 'Resource Optimization Workflow', 'Identify resource-heavy processes;Optimize applications;Add RAM/CPU;Migrate workloads;Implement load balancing', 'Resources optimized;Hardware upgraded;Workloads redistributed', 2, 1, 2, 2, 8, 'server,cpu,memory,performance', NOW(), 0),
(13, 'Storage Capacity Warning', 1, 3, 'Request', 'Server storage reaching capacity threshold', 'Storage Expansion Workflow', 'Clean old files;Archive data;Add storage drives;Implement tiered storage;Migrate to larger storage solution', 'Storage expanded;Data archived;Capacity monitoring implemented', 3, 1, 1, 4, 24, 'storage,capacity,server,disk', NOW(), 0),
(14, 'RAID Array Degraded', 1, 3, 'Complaint', 'RAID array showing degraded status or disk failure', 'RAID Recovery Workflow', 'Replace failed disk;Rebuild array;Verify data integrity;Update firmware;Replace RAID controller if needed', 'Array rebuilt;Data verified intact;Preventive maintenance scheduled', 4, 1, 3, 1, 8, 'raid,storage,disk,failure', NOW(), 0),
(15, 'Virtualization Issues', 1, 3, 'Complaint', 'VM performance problems migration failures or hypervisor errors', 'Virtualization Support Workflow', 'Check host resources;Optimize VM settings;Update hypervisor;Redistribute VMs;Add host capacity', 'VMs optimized;Resources balanced;Hypervisor updated', 5, 1, 2, 2, 16, 'vm,virtualization,hypervisor,vmware', NOW(), 0),

-- Hardware - Data Center Equipment (Subcategory 4)
(16, 'Cooling System Alert', 1, 4, 'Complaint', 'Temperature warnings or HVAC system failures in data center', 'Environmental Alert Workflow', 'Check HVAC status;Increase cooling capacity;Redistribute heat load;Emergency cooling deployment;Schedule maintenance', 'Temperature normalized;HVAC repaired;Monitoring enhanced', 1, 1, 4, 1, 4, 'cooling,temperature,hvac,datacenter', NOW(), 0),
(17, 'Power Distribution Issue', 1, 4, 'Complaint', 'PDU failure power redundancy loss or circuit overload', 'Power System Recovery Workflow', 'Switch to backup power;Balance power load;Replace PDU;Add power capacity;Update power monitoring', 'Power restored;Redundancy verified;Load balanced', 2, 1, 4, 1, 4, 'power,pdu,datacenter,electrical', NOW(), 0),
(18, 'UPS Battery Replacement', 1, 4, 'Request', 'UPS batteries reaching end of life or failed battery test', 'UPS Maintenance Workflow', 'Test battery capacity;Schedule replacement;Install new batteries;Test UPS functionality;Update maintenance logs', 'Batteries replaced;UPS tested and operational;Next service scheduled', 3, 1, 1, 24, 72, 'ups,battery,power,maintenance', NOW(), 0),
(19, 'Rack Space Request', 1, 4, 'Request', 'Customer needs additional rack space or reorganization', 'Rack Allocation Workflow', 'Survey available space;Plan layout;Schedule installation;Mount equipment;Cable management;Documentation', 'Equipment installed;Cabling completed;Documentation updated', 4, 1, 0, 24, 120, 'rack,datacenter,space,equipment', NOW(), 0),

-- Hardware - Mobile & Telecommunications (Subcategory 5)
(20, 'Device Malfunction', 1, 5, 'Complaint', 'Mobile device not functioning properly hardware defects', 'Mobile Device Support Workflow', 'Remote diagnostics;Factory reset;Replace device;Restore from backup;Provide loaner device', 'Device replaced under warranty;Issue resolved remotely', 1, 1, 1, 4, 24, 'mobile,device,smartphone,tablet', NOW(), 0),
(21, 'SIM/Connectivity Issues', 1, 5, 'Complaint', 'Cannot connect to cellular network or SIM not recognized', 'Mobile Connectivity Workflow', 'Check SIM card;Verify account status;Update carrier settings;Replace SIM;Reconfigure APN settings', 'SIM replaced;Network settings corrected;Connectivity restored', 2, 1, 2, 2, 8, 'sim,mobile,cellular,connectivity', NOW(), 0),
(22, 'MDM Enrollment Problem', 1, 5, 'Request', 'Issues enrolling device in mobile device management system', 'MDM Support Workflow', 'Verify credentials;Check MDM server;Factory reset device;Re-enroll device;Update MDM profile', 'Device successfully enrolled;Policies applied;User trained', 3, 1, 0, 4, 8, 'mdm,mobile,enrollment,intune', NOW(), 0),

-- Hardware - Printing & Imaging (Subcategory 6)
(23, 'Printer Offline/Not Printing', 1, 6, 'Complaint', 'Printer shows offline or jobs stuck in queue', 'Printer Troubleshooting Workflow', 'Restart printer;Check connections;Update drivers;Clear print queue;Reinstall printer;Network configuration', 'Printer back online;Jobs printing successfully', 1, 1, 1, 2, 4, 'printer,offline,queue,printing', NOW(), 0),
(24, 'Paper Jam/Feed Issues', 1, 6, 'Complaint', 'Repeated paper jams or paper feed mechanism failure', 'Printer Maintenance Workflow', 'Clear paper path;Clean rollers;Replace pickup assembly;Adjust paper tray;Replace feed mechanism', 'Mechanism cleaned/replaced;Jam issue resolved', 2, 1, 0, 4, 8, 'printer,jam,paper,feed', NOW(), 0),
(25, 'Print Quality Issues', 1, 6, 'Complaint', 'Poor print quality streaks fading or color problems', 'Print Quality Workflow', 'Clean print heads;Replace toner/ink;Calibrate printer;Replace drum;Replace imaging unit', 'Print quality restored;Consumables replaced;Calibration completed', 3, 1, 0, 4, 8, 'printer,quality,toner,print', NOW(), 0),
(26, 'Toner/Ink Replacement', 1, 6, 'Request', 'Customer needs consumables replacement or supplies order', 'Consumables Order Workflow', 'Verify model;Order supplies;Ship to customer;Provide installation instructions;Recycle old cartridges', 'Supplies delivered;Installation confirmed;Recycling arranged', 4, 1, 0, 8, 24, 'toner,ink,supplies,consumables', NOW(), 0),
(27, 'Scanner Not Working', 1, 6, 'Complaint', 'Scanner function not operating or producing errors', 'Scanner Support Workflow', 'Update scanner drivers;Test scanner software;Clean scanner glass;Replace scanner module;Reinstall software', 'Scanner operational;Software updated;Issue resolved', 5, 1, 0, 4, 8, 'scanner,imaging,document', NOW(), 0),

-- Software - Operating Systems (Subcategory 8)
(28, 'System Won''t Boot', 2, 8, 'Complaint', 'Operating system fails to start or stuck in boot loop', 'OS Recovery Workflow', 'Boot into safe mode;Run startup repair;Restore system;Reinstall OS;Recover data;Restore from backup', 'OS repaired;System restored;Data recovered', 1, 1, 2, 2, 8, 'os,boot,windows,startup', NOW(), 0),
(29, 'Blue Screen/Crash Errors', 2, 8, 'Complaint', 'Frequent system crashes BSOD or kernel panics', 'System Stability Workflow', 'Analyze crash dumps;Update drivers;Check hardware;Remove problematic software;Reinstall OS', 'Stability restored;Faulty driver/software removed;System optimized', 2, 1, 2, 2, 8, 'bsod,crash,kernel,windows', NOW(), 0),
(30, 'License Activation Issues', 2, 8, 'Complaint', 'OS not activated or license validation errors', 'License Resolution Workflow', 'Verify license key;Reactivate license;Contact vendor;Issue new license;Update activation server', 'License activated successfully;Documentation updated', 3, 1, 1, 4, 8, 'license,activation,windows,key', NOW(), 0),
(31, 'OS Update Request', 2, 8, 'Request', 'Customer needs OS upgraded to newer version', 'OS Upgrade Workflow', 'Backup system;Check compatibility;Download OS;Perform upgrade;Migrate settings;Test applications', 'OS successfully upgraded;Applications tested;User trained', 4, 1, 0, 8, 24, 'upgrade,os,windows,update', NOW(), 0),
(32, 'Patch/Update Failures', 2, 8, 'Complaint', 'Windows updates failing or causing system issues', 'Update Troubleshooting Workflow', 'Clear update cache;Run troubleshooter;Manual download;Repair Windows components;Rollback update', 'Updates installed successfully;System stable;Update schedule optimized', 5, 1, 1, 2, 8, 'patch,update,windows,wsus', NOW(), 0),

-- Software - Productivity & Collaboration (Subcategory 9)
(33, 'Email Access Issues', 2, 9, 'Complaint', 'Cannot access email account sync errors or login problems', 'Email Support Workflow', 'Verify credentials;Check email server;Reconfigure email client;Reset password;Clear cache;Update software', 'Email access restored;Sync working;Credentials updated', 1, 1, 2, 1, 4, 'email,outlook,access,sync', NOW(), 0),
(34, 'Office Application Crashes', 2, 9, 'Complaint', 'Word Excel PowerPoint crashing frequently or not opening', 'Office Repair Workflow', 'Repair Office installation;Disable add-ins;Clear temp files;Reinstall application;Check file corruption', 'Application stable;Add-ins managed;Files recovered', 2, 1, 1, 2, 8, 'office,crash,word,excel', NOW(), 0),
(35, 'License Addition Request', 2, 9, 'Request', 'Need to add more user licenses or upgrade subscription', 'License Management Workflow', 'Review current licenses;Process order;Assign licenses;Configure new users;Update billing', 'Licenses added;Users activated;Billing confirmed', 3, 1, 0, 8, 24, 'license,m365,subscription,user', NOW(), 0),
(36, 'OneDrive/SharePoint Sync Issues', 2, 9, 'Complaint', 'Files not syncing properly or sync errors occurring', 'Cloud Sync Resolution Workflow', 'Reset sync;Check permissions;Clear cache;Reinstall sync client;Verify network;Check storage quota', 'Sync restored;Permissions corrected;Files up to date', 4, 1, 1, 2, 8, 'onedrive,sharepoint,sync,files', NOW(), 0),
(37, 'Teams/Meeting Issues', 2, 9, 'Complaint', 'Video conferencing problems audio issues or meeting failures', 'Collaboration Tools Support Workflow', 'Check audio/video settings;Update application;Test network;Verify permissions;Reinstall client;Optimize bandwidth', 'Meeting tools working;Audio/video tested;Network optimized', 5, 1, 2, 1, 4, 'teams,meeting,audio,video', NOW(), 0),

-- Software - Security Software (Subcategory 10)
(38, 'Antivirus Not Updating', 2, 10, 'Complaint', 'Security definitions not updating or update failures', 'Security Update Workflow', 'Check internet connection;Verify subscription;Manual update;Reinstall software;Check firewall settings', 'Definitions updated;Subscription verified;Protection active', 1, 1, 2, 1, 4, 'antivirus,update,security,definitions', NOW(), 0),
(39, 'False Positive Detection', 2, 10, 'Complaint', 'Legitimate files flagged as threats or quarantined incorrectly', 'False Positive Workflow', 'Analyze file;Whitelist application;Submit to vendor;Adjust scan settings;Restore quarantined files', 'File restored;Whitelist updated;Settings adjusted', 2, 1, 1, 2, 4, 'antivirus,false positive,quarantine', NOW(), 0),
(40, 'Performance Impact', 2, 10, 'Complaint', 'Security software causing system slowdown or high resource usage', 'Performance Tuning Workflow', 'Adjust scan schedule;Configure exclusions;Update software;Reduce scan intensity;Consider alternative solution', 'Performance improved;Scans optimized;Settings configured', 3, 1, 1, 4, 8, 'antivirus,performance,slow', NOW(), 0),
(41, 'Threat Detection Alert', 2, 10, 'Complaint', 'Malware virus or security threat detected on system', 'Threat Remediation Workflow', 'Isolate system;Run full scan;Remove threats;Check for data breach;Restore clean backup;Strengthen security', 'Threat removed;System cleaned;Prevention measures implemented', 4, 1, 3, 1, 4, 'malware,virus,threat,infection', NOW(), 0),
(42, 'License Renewal Required', 2, 10, 'Request', 'Security subscription expiring or expired', 'License Renewal Workflow', 'Generate renewal quote;Process payment;Apply new license;Verify activation;Update records', 'License renewed;Protection active;Next renewal scheduled', 5, 1, 1, 8, 24, 'license,renewal,security,subscription', NOW(), 0),

-- Software - Business Applications (Subcategory 11)
(43, 'CRM/ERP Performance Issues', 2, 11, 'Complaint', 'Business application running slow or timing out', 'Application Performance Workflow', 'Check database;Optimize queries;Add resources;Clear cache;Update application;Index databases', 'Performance improved;Database optimized;Resources added', 1, 1, 2, 2, 8, 'crm,erp,performance,slow', NOW(), 0),
(44, 'Data Import/Export Errors', 2, 11, 'Complaint', 'Problems importing or exporting data format errors', 'Data Integration Support Workflow', 'Validate file format;Check field mapping;Fix data errors;Adjust import settings;Manual correction', 'Data successfully imported;Errors corrected;Process documented', 2, 1, 1, 4, 8, 'data,import,export,integration', NOW(), 0),
(45, 'User Access Request', 2, 11, 'Request', 'New user needs access or permission changes required', 'User Access Workflow', 'Verify authorization;Create account;Assign roles;Configure permissions;Provide training;Document access', 'User access granted;Permissions configured;Training completed', 3, 1, 0, 4, 8, 'access,user,permissions,roles', NOW(), 0),
(46, 'Custom Report Request', 2, 11, 'Request', 'Customer needs new report or dashboard created', 'Report Development Workflow', 'Gather requirements;Design report;Develop solution;Test output;Deploy to production;Train users', 'Report delivered;Users trained;Documentation provided', 4, 1, 0, 24, 72, 'report,custom,dashboard,analytics', NOW(), 0),
(47, 'Integration Failure', 2, 11, 'Complaint', 'Third-party integration not working or API errors', 'Integration Support Workflow', 'Check API credentials;Verify endpoints;Review error logs;Update integration;Reconnect services;Test functionality', 'Integration restored;Data flowing;Monitoring enabled', 5, 1, 2, 2, 8, 'integration,api,connection,sync', NOW(), 0),

-- Software - Backup & Recovery (Subcategory 13)
(48, 'Backup Job Failure', 2, 13, 'Complaint', 'Scheduled backup failed or incomplete', 'Backup Troubleshooting Workflow', 'Review error logs;Check storage space;Verify credentials;Adjust schedule;Manual backup;Fix configuration', 'Backup successful;Schedule optimized;Alerts configured', 1, 1, 2, 2, 4, 'backup,failure,job,schedule', NOW(), 0),
(49, 'Restore Request', 2, 13, 'Request', 'Customer needs files or system restored from backup', 'Data Restoration Workflow', 'Verify backup availability;Confirm restore point;Perform restoration;Validate data integrity;Test restored system', 'Data successfully restored;Integrity verified;User confirmed', 2, 1, 1, 2, 8, 'restore,backup,recovery,data', NOW(), 0),
(50, 'Slow Backup Performance', 2, 13, 'Complaint', 'Backups taking too long or impacting system performance', 'Backup Optimization Workflow', 'Optimize backup window;Implement incremental backups;Add bandwidth;Deduplicate data;Upgrade backup solution', 'Backup speed improved;Impact minimized;Deduplication enabled', 3, 1, 1, 4, 24, 'backup,performance,slow,optimization', NOW(), 0),
(51, 'Storage Quota Exceeded', 2, 13, 'Complaint', 'Backup storage full or approaching capacity limit', 'Storage Management Workflow', 'Review retention policy;Delete old backups;Add storage;Implement tiering;Compress backups', 'Storage expanded;Old backups archived;Policy updated', 4, 1, 1, 4, 24, 'storage,quota,backup,capacity', NOW(), 0),

-- Cloud Services - IaaS (Subcategory 15)
(52, 'Virtual Machine Not Starting', 3, 15, 'Complaint', 'VM fails to start or stuck in provisioning state', 'VM Recovery Workflow', 'Check resource availability;Review error messages;Restart VM service;Rebuild VM;Restore from snapshot', 'VM operational;Issue identified;Monitoring added', 1, 1, 2, 2, 8, 'vm,iaas,cloud,startup', NOW(), 0),
(53, 'High Cloud Costs', 3, 15, 'Complaint', 'Unexpected high billing or cost overruns', 'Cost Optimization Workflow', 'Analyze usage;Right-size resources;Implement auto-scaling;Remove unused resources;Reserved instances;Budget alerts', 'Costs reduced;Right-sizing completed;Alerts configured', 2, 1, 1, 8, 48, 'cloud,cost,billing,optimization', NOW(), 0),
(54, 'Resource Scaling Request', 3, 15, 'Request', 'Need to increase/decrease VM resources or storage', 'Resource Adjustment Workflow', 'Review current usage;Plan scaling;Schedule change;Apply new configuration;Test performance;Update documentation', 'Resources scaled;Performance validated;Documentation updated', 3, 1, 0, 4, 24, 'cloud,scaling,resources,resize', NOW(), 0),
(55, 'Snapshot/Backup Request', 3, 15, 'Request', 'Customer needs VM snapshot or backup created', 'Cloud Backup Workflow', 'Create snapshot;Verify snapshot;Test restoration;Schedule automated backups;Document retention policy', 'Snapshot created;Backup schedule configured;Policy documented', 4, 1, 0, 4, 8, 'snapshot,backup,vm,cloud', NOW(), 0),

-- Cloud Services - PaaS (Subcategory 16)
(56, 'Database Connection Errors', 3, 16, 'Complaint', 'Application cannot connect to cloud database', 'Database Connectivity Workflow', 'Check connection string;Verify firewall rules;Test credentials;Review network security;Restart database service', 'Connection restored;Firewall configured;Monitoring enabled', 1, 1, 2, 1, 4, 'database,connection,paas,cloud', NOW(), 0),
(57, 'Database Performance Issues', 3, 16, 'Complaint', 'Slow query performance or database timeouts', 'Database Optimization Workflow', 'Analyze slow queries;Add indexes;Optimize queries;Increase DTUs/resources;Implement caching;Partition data', 'Performance improved;Queries optimized;Resources scaled', 2, 1, 2, 2, 8, 'database,performance,slow,query', NOW(), 0),
(58, 'Database Migration Request', 3, 16, 'Request', 'Need to migrate database to different tier or region', 'Database Migration Workflow', 'Plan migration;Backup data;Create target database;Migrate data;Test applications;Update connections;Cutover', 'Migration completed;Applications tested;Zero downtime achieved', 3, 1, 0, 24, 72, 'database,migration,paas,cloud', NOW(), 0),

-- Cloud Services - SaaS (Subcategory 17)
(59, 'User Provisioning Issues', 3, 17, 'Complaint', 'New users not created or SSO not working', 'User Management Workflow', 'Verify identity provider;Check synchronization;Manually provision;Fix SSO configuration;Test authentication', 'Users provisioned;SSO working;Sync verified', 1, 1, 2, 2, 8, 'saas,user,provisioning,sso', NOW(), 0),
(60, 'Feature Not Available', 3, 17, 'Complaint', 'Expected feature missing or not working in SaaS application', 'Feature Support Workflow', 'Verify subscription tier;Check feature rollout;Enable feature flag;Upgrade subscription;Provide workaround', 'Feature enabled;Subscription upgraded;User trained on feature', 2, 1, 1, 4, 24, 'saas,feature,subscription', NOW(), 0),
(61, 'Data Export Request', 3, 17, 'Request', 'Customer needs to export data from SaaS platform', 'Data Export Workflow', 'Verify export permissions;Generate export;Format conversion;Deliver securely;Verify data completeness', 'Data exported;Format verified;Securely delivered', 3, 1, 0, 8, 24, 'saas,export,data', NOW(), 0),

-- Cloud Services - Hybrid & Multi-Cloud (Subcategory 18)
(62, 'VPN Connection Failure', 3, 18, 'Complaint', 'Site-to-site VPN down or intermittent connectivity', 'VPN Troubleshooting Workflow', 'Check VPN status;Verify credentials;Review firewall rules;Restart VPN service;Reconfigure tunnel;Update configuration', 'VPN restored;Redundancy verified;Monitoring enhanced', 1, 1, 3, 1, 4, 'vpn,connectivity,hybrid,tunnel', NOW(), 0),
(63, 'Cloud Sync Issues', 3, 18, 'Complaint', 'Data not syncing between on-premises and cloud', 'Hybrid Sync Workflow', 'Check sync service;Verify permissions;Review network;Clear sync cache;Manual sync;Update sync client', 'Sync operational;Data consistent;Monitoring configured', 2, 1, 2, 2, 8, 'sync,hybrid,cloud,data', NOW(), 0),

-- Managed Services - Managed IT Services (Subcategory 36)
(64, 'Routine Maintenance Request', 6, 36, 'Request', 'Customer requests scheduled maintenance or health check', 'Scheduled Maintenance Workflow', 'Schedule maintenance window;Perform updates;Run diagnostics;Generate report;Review with customer', 'Maintenance completed;Systems optimized;Report delivered', 1, 1, 0, 24, 72, 'maintenance,managed,health check', NOW(), 0),
(65, 'Add New Device to Management', 6, 36, 'Request', 'New device needs to be added to managed services', 'Device Onboarding Workflow', 'Register device;Install agents;Configure monitoring;Apply policies;Test connectivity;Update inventory', 'Device onboarded;Monitoring active;Documentation updated', 2, 1, 0, 4, 24, 'device,onboarding,managed,rmm', NOW(), 0),
(66, 'Alert Fatigue Complaint', 6, 36, 'Complaint', 'Too many alerts or false alarms from monitoring', 'Alert Tuning Workflow', 'Review alert thresholds;Adjust sensitivity;Group alerts;Implement smart alerting;Reduce noise;Customize escalation', 'Alerts optimized;False positives reduced;Escalation tuned', 3, 1, 1, 8, 48, 'alerts,monitoring,fatigue,tuning', NOW(), 0),

-- Managed Services - Managed Security Services (Subcategory 37)
(67, 'Security Incident Alert', 6, 37, 'Complaint', 'Security event detected requiring investigation', 'Security Incident Response Workflow', 'Assess threat;Contain incident;Investigate root cause;Remediate vulnerability;Restore systems;Post-incident review', 'Incident contained;Systems secured;Report provided', 1, 1, 4, 1, 4, 'security,incident,soc,alert', NOW(), 0),
(68, 'Firewall Rule Change', 6, 37, 'Request', 'Customer needs firewall rules modified', 'Firewall Change Workflow', 'Review request;Assess security impact;Create change plan;Implement rule;Test connectivity;Document change', 'Rule implemented;Access verified;Change documented', 2, 1, 1, 4, 8, 'firewall,rules,security,change', NOW(), 0),
(69, 'Security Report Request', 6, 37, 'Request', 'Customer requests security posture or compliance report', 'Security Reporting Workflow', 'Gather security data;Analyze vulnerabilities;Generate report;Review findings;Provide recommendations;Present to customer', 'Report delivered;Findings reviewed;Remediation plan created', 3, 1, 0, 24, 72, 'security,report,compliance,posture', NOW(), 0),

-- Managed Services - Managed Cloud Services (Subcategory 39)
(70, 'Cloud Cost Alert', 6, 39, 'Complaint', 'Unexpected cloud spending increase or budget exceeded', 'Cloud Cost Management Workflow', 'Analyze spending;Identify cost drivers;Right-size resources;Implement cost controls;Set up alerts;Optimize licensing', 'Costs controlled;Alerts configured;Optimization ongoing', 1, 1, 2, 4, 24, 'cloud,cost,budget,spending', NOW(), 0),
(71, 'Cloud Architecture Review', 6, 39, 'Request', 'Customer requests architecture assessment or optimization', 'Architecture Review Workflow', 'Assess current state;Identify improvements;Recommend changes;Create roadmap;Implement changes;Validate results', 'Review completed;Recommendations delivered;Roadmap created', 2, 1, 0, 24, 120, 'cloud,architecture,review,optimization', NOW(), 0),

-- Support & Maintenance - Help Desk Support (Subcategory 44)
(72, 'Password Reset Request', 7, 44, 'Request', 'User locked out or forgot password', 'Password Reset Workflow', 'Verify identity;Reset password;Send temporary credentials;Force password change on login;Update documentation', 'Password reset;Access restored;User notified', 1, 1, 0, 1, 2, 'password,reset,access,locked', NOW(), 0),
(73, 'Software Installation Request', 7, 44, 'Request', 'User needs new software or application installed', 'Software Installation Workflow', 'Verify license;Check compatibility;Download software;Install application;Configure settings;Train user;Update inventory', 'Software installed;User trained;License tracked', 2, 1, 0, 2, 8, 'software,installation,request', NOW(), 0),
(74, 'Slow Computer Complaint', 7, 44, 'Complaint', 'User reports computer running slowly', 'Performance Support Workflow', 'Run diagnostics;Check for malware;Clear temp files;Disable startup programs;Add RAM if needed;Optimize system', 'Performance improved;Malware removed;User satisfied', 3, 1, 1, 2, 4, 'slow,performance,computer', NOW(), 0),
(75, 'Email Signature Update', 7, 44, 'Request', 'User needs email signature created or modified', 'Email Configuration Workflow', 'Gather requirements;Create signature template;Apply to email client;Test rendering;Provide instructions;Update centrally if applicable', 'Signature created;Applied successfully;Documentation provided', 4, 1, 0, 4, 8, 'email,signature,configuration', NOW(), 0),
(76, 'General How-To Question', 7, 44, 'Request', 'User needs help with using software or feature', 'User Training Support Workflow', 'Understand requirement;Provide step-by-step guidance;Share documentation;Offer training resources;Follow up', 'User trained;Issue resolved;Documentation shared', 5, 1, 0, 2, 4, 'howto,training,support,question', NOW(), 0),

-- Support & Maintenance - On-Site Support (Subcategory 45)
(77, 'Equipment Setup', 7, 45, 'Request', 'New equipment needs on-site installation and configuration', 'On-Site Installation Workflow', 'Schedule visit;Prepare equipment;Travel to site;Install hardware;Configure system;Test functionality;Train user', 'Equipment installed;Testing complete;User trained on-site', 1, 1, 0, 24, 48, 'onsite,installation,equipment,setup', NOW(), 0),
(78, 'Network Troubleshooting', 7, 45, 'Complaint', 'On-site network issues requiring physical inspection', 'On-Site Network Workflow', 'Travel to location;Diagnose issue;Test cables;Check switches;Replace hardware;Verify connectivity;Document resolution', 'Issue resolved on-site;Hardware replaced;Network operational', 2, 1, 2, 4, 8, 'onsite,network,troubleshooting', NOW(), 0),

-- Support & Maintenance - Remote Support (Subcategory 46)
(79, 'Remote Desktop Issues', 7, 46, 'Complaint', 'Cannot connect remotely or remote tools not working', 'Remote Access Workflow', 'Check VPN status;Verify credentials;Test remote software;Configure firewall;Reinstall remote agent;Alternative access method', 'Remote access restored;Alternative provided;User connected', 1, 1, 2, 1, 4, 'remote,desktop,vpn,access', NOW(), 0),

-- Security Services - Security Assessments (Subcategory 52)
(80, 'Vulnerability Scan Request', 8, 52, 'Request', 'Customer requests security vulnerability assessment', 'Vulnerability Assessment Workflow', 'Schedule scan;Execute scan;Analyze results;Prioritize vulnerabilities;Generate report;Provide remediation guidance', 'Scan completed;Report delivered;Remediation plan provided', 1, 1, 0, 24, 120, 'vulnerability,scan,assessment,security', NOW(), 0),
(81, 'Compliance Audit Request', 8, 52, 'Request', 'Customer needs compliance audit (HIPAA PCI SOC2)', 'Compliance Audit Workflow', 'Review requirements;Gather evidence;Assess controls;Identify gaps;Generate compliance report;Remediation recommendations', 'Audit completed;Gaps identified;Remediation roadmap delivered', 2, 1, 0, 48, 240, 'compliance,audit,hipaa,pci,soc2', NOW(), 0),

-- Security Services - Penetration Testing (Subcategory 53)
(82, 'Pentest Scheduling Request', 8, 53, 'Request', 'Customer wants to schedule penetration testing', 'Pentest Scheduling Workflow', 'Define scope;Schedule test;Execute testing;Document findings;Provide executive summary;Retest after remediation', 'Pentest completed;Vulnerabilities documented;Retest scheduled', 1, 1, 0, 48, 240, 'pentest,security,testing,schedule', NOW(), 0),

-- Security Services - Incident Response (Subcategory 55)
(83, 'Security Breach Suspected', 8, 55, 'Complaint', 'Potential security breach or compromise detected', 'Breach Response Workflow', 'Activate incident response;Contain breach;Forensic analysis;Identify scope;Remediate;Notify stakeholders;Post-mortem', 'Breach contained;Forensics completed;Security hardened', 1, 1, 4, 1, 8, 'breach,security,incident,compromise', NOW(), 0),
(84, 'Ransomware Attack', 8, 55, 'Complaint', 'System infected with ransomware or encryption detected', 'Ransomware Response Workflow', 'Isolate infected systems;Identify ransomware variant;Attempt decryption;Restore from backup;Strengthen defenses;User training', 'Systems restored;Ransomware removed;Prevention enhanced', 2, 1, 4, 1, 4, 'ransomware,attack,encryption,incident', NOW(), 0),

-- Specialized Services - VoIP & UC (Subcategory 76)
(85, 'No Dial Tone', 11, 76, 'Complaint', 'Phone not working or cannot make/receive calls', 'VoIP Troubleshooting Workflow', 'Check phone registration;Verify network;Test handset;Reconfigure phone;Replace hardware;Check PBX status', 'Phone operational;Calls working;Configuration verified', 1, 1, 2, 2, 8, 'voip,phone,dial tone,calls', NOW(), 0),
(86, 'Call Quality Issues', 11, 76, 'Complaint', 'Poor audio quality echo jitter or dropped calls', 'Call Quality Workflow', 'Test network quality;Check bandwidth;Implement QoS;Update firmware;Adjust codec;Optimize network path', 'Call quality improved;QoS implemented;Network optimized', 2, 1, 1, 4, 24, 'voip,quality,audio,jitter', NOW(), 0),
(87, 'Extension Addition', 11, 76, 'Request', 'New user needs phone extension or voicemail setup', 'User Provisioning Workflow', 'Create extension;Configure voicemail;Assign phone;Program features;Test functionality;Train user', 'Extension active;Voicemail configured;User trained', 3, 1, 0, 4, 24, 'voip,extension,voicemail,setup', NOW(), 0),

-- Specialized Services - Cabling Services (Subcategory 79)
(88, 'Cable Run Request', 11, 79, 'Request', 'New cable installation or additional network drops needed', 'Cabling Installation Workflow', 'Site survey;Plan cable route;Pull cables;Terminate connections;Test certification;Label cables;Update documentation', 'Cables installed;Testing passed;Documentation updated', 1, 1, 0, 48, 168, 'cabling,network,drops,installation', NOW(), 0),
(89, 'Cable Certification', 11, 79, 'Request', 'Customer needs cable testing and certification report', 'Cable Testing Workflow', 'Test all runs;Generate test reports;Identify failures;Remediate issues;Provide certification documentation', 'Cables certified;Reports provided;Issues resolved', 2, 1, 0, 24, 72, 'cabling,certification,testing', NOW(), 0),

-- Data & Analytics - Business Intelligence (Subcategory 69)
(90, 'Report Not Loading', 10, 69, 'Complaint', 'Dashboard or report failing to load or showing errors', 'Report Troubleshooting Workflow', 'Check data source;Verify credentials;Refresh cache;Rebuild report;Optimize queries;Test data connection', 'Report operational;Data refreshing;Performance optimized', 1, 1, 2, 2, 8, 'report,dashboard,bi,loading', NOW(), 0),
(91, 'Custom Dashboard Request', 10, 69, 'Request', 'User needs new dashboard or report created', 'Dashboard Development Workflow', 'Gather requirements;Design layout;Build dashboard;Connect data sources;Test functionality;Deploy;Train users', 'Dashboard delivered;Users trained;Refresh scheduled', 2, 1, 0, 24, 120, 'dashboard,bi,custom,analytics', NOW(), 0),

-- Data & Analytics - Data Warehousing (Subcategory 70)
(92, 'ETL Job Failure', 10, 70, 'Complaint', 'Data extraction transformation or loading process failed', 'ETL Troubleshooting Workflow', 'Review error logs;Check source connection;Validate data;Fix transformation logic;Rerun job;Monitor completion', 'ETL job successful;Data loaded;Monitoring enhanced', 1, 1, 2, 2, 8, 'etl,data,warehouse,failure', NOW(), 0),
(93, 'Data Quality Issues', 10, 70, 'Complaint', 'Incorrect duplicate or missing data in warehouse', 'Data Quality Workflow', 'Identify data issues;Trace to source;Implement validation;Clean data;Add quality checks;Prevent recurrence', 'Data quality improved;Validation rules added;Issues resolved', 2, 1, 1, 4, 24, 'data,quality,warehouse,cleansing', NOW(), 0);

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================================
-- END OF SEED DATA
-- ============================================================================
