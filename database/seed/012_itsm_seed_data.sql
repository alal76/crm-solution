-- =====================================================
-- ITSM Module Seed Data
-- This file populates default data for ITSM functionality:
-- - SLA Policies (P1-P4)
-- - Business Hours Schedules
-- - Priority Matrix
-- - Service Catalog Categories
-- - CI Types and Classifications
-- - Sample Knowledge Base Articles
-- - Change Blackout Periods
-- =====================================================

SET XACT_ABORT ON;
BEGIN TRANSACTION;

PRINT 'Starting ITSM seed data population...';

-- =====================================================
-- 1. SLA POLICIES (Priority-based)
-- =====================================================

PRINT 'Creating SLA Policies...';

-- Check if SLA policies table exists and is empty
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ITSMSLAPolicies')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ITSMSLAPolicies WHERE PolicyName = 'P1 - Critical')
    BEGIN
        INSERT INTO ITSMSLAPolicies (
            PolicyName, Description, Priority, 
            ResponseTimeMinutes, ResolutionTimeMinutes,
            EscalationEnabled, EscalationThresholdPercent,
            BusinessHoursOnly, IsActive, CreatedAt
        )
        VALUES
        -- P1 - Critical: System down, major business impact
        ('P1 - Critical', 'Critical priority - Major system outage affecting multiple users or business-critical functions', 1,
         15, 240, -- 15 min response, 4 hour resolution
         1, 75, -- Escalate at 75% of SLA
         0, -- 24x7
         1, GETUTCDATE()),
        
        -- P2 - High: Major feature impacted, workaround difficult
        ('P2 - High', 'High priority - Significant impact with difficult or no workaround', 2,
         30, 480, -- 30 min response, 8 hour resolution
         1, 80,
         0, -- 24x7
         1, GETUTCDATE()),
        
        -- P3 - Medium: Feature impacted, workaround available
        ('P3 - Medium', 'Medium priority - Moderate impact with workaround available', 3,
         120, 1440, -- 2 hour response, 24 hour resolution
         1, 85,
         1, -- Business hours only
         1, GETUTCDATE()),
        
        -- P4 - Low: Minor issue, no significant impact
        ('P4 - Low', 'Low priority - Minor issue with minimal business impact', 4,
         480, 2880, -- 8 hour response, 48 hour resolution
         0, 90,
         1, -- Business hours only
         1, GETUTCDATE()),
        
        -- P5 - Planning: Enhancement requests, no urgency
        ('P5 - Planning', 'Planning priority - Enhancement requests and non-urgent items', 5,
         1440, 10080, -- 24 hour response, 7 day resolution
         0, 95,
         1, -- Business hours only
         1, GETUTCDATE());
        
        PRINT '  - Created 5 SLA policies (P1-P5)';
    END
    ELSE
        PRINT '  - SLA policies already exist, skipping...';
END

-- =====================================================
-- 2. BUSINESS HOURS SCHEDULES
-- =====================================================

PRINT 'Creating Business Hours Schedules...';

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ITSMBusinessHours')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ITSMBusinessHours WHERE ScheduleName = 'Standard Business Hours')
    BEGIN
        INSERT INTO ITSMBusinessHours (
            ScheduleName, Description, TimeZone, IsDefault, IsActive, CreatedAt
        )
        VALUES
        ('Standard Business Hours', 'Monday-Friday 8:00 AM - 5:00 PM', 'America/New_York', 1, 1, GETUTCDATE()),
        ('Extended Support', 'Monday-Friday 7:00 AM - 9:00 PM', 'America/New_York', 0, 1, GETUTCDATE()),
        ('24x7 Operations', '24 hours, 7 days a week', 'UTC', 0, 1, GETUTCDATE()),
        ('EMEA Business Hours', 'Monday-Friday 9:00 AM - 6:00 PM CET', 'Europe/London', 0, 1, GETUTCDATE()),
        ('APAC Business Hours', 'Monday-Friday 9:00 AM - 6:00 PM JST', 'Asia/Tokyo', 0, 1, GETUTCDATE());
        
        PRINT '  - Created 5 business hours schedules';
        
        -- Insert daily schedules for Standard Business Hours
        DECLARE @StandardHoursId INT = (SELECT TOP 1 ScheduleId FROM ITSMBusinessHours WHERE ScheduleName = 'Standard Business Hours');
        
        IF @StandardHoursId IS NOT NULL
        BEGIN
            INSERT INTO ITSMBusinessHoursDetails (ScheduleId, DayOfWeek, StartTime, EndTime, IsWorkDay)
            VALUES
            (@StandardHoursId, 0, NULL, NULL, 0),           -- Sunday - not a workday
            (@StandardHoursId, 1, '08:00', '17:00', 1),     -- Monday
            (@StandardHoursId, 2, '08:00', '17:00', 1),     -- Tuesday
            (@StandardHoursId, 3, '08:00', '17:00', 1),     -- Wednesday
            (@StandardHoursId, 4, '08:00', '17:00', 1),     -- Thursday
            (@StandardHoursId, 5, '08:00', '17:00', 1),     -- Friday
            (@StandardHoursId, 6, NULL, NULL, 0);           -- Saturday - not a workday
            
            PRINT '  - Created daily schedule for Standard Business Hours';
        END
    END
    ELSE
        PRINT '  - Business hours already exist, skipping...';
END

-- =====================================================
-- 3. HOLIDAYS (US Federal Holidays 2026)
-- =====================================================

PRINT 'Creating Holiday Calendar...';

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ITSMHolidays')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ITSMHolidays WHERE HolidayDate = '2026-01-01')
    BEGIN
        INSERT INTO ITSMHolidays (HolidayName, HolidayDate, IsRecurring, Country, IsActive, CreatedAt)
        VALUES
        ('New Year''s Day', '2026-01-01', 0, 'US', 1, GETUTCDATE()),
        ('Martin Luther King Jr. Day', '2026-01-19', 0, 'US', 1, GETUTCDATE()),
        ('Presidents'' Day', '2026-02-16', 0, 'US', 1, GETUTCDATE()),
        ('Memorial Day', '2026-05-25', 0, 'US', 1, GETUTCDATE()),
        ('Independence Day (Observed)', '2026-07-03', 0, 'US', 1, GETUTCDATE()),
        ('Independence Day', '2026-07-04', 0, 'US', 1, GETUTCDATE()),
        ('Labor Day', '2026-09-07', 0, 'US', 1, GETUTCDATE()),
        ('Columbus Day', '2026-10-12', 0, 'US', 1, GETUTCDATE()),
        ('Veterans Day', '2026-11-11', 0, 'US', 1, GETUTCDATE()),
        ('Thanksgiving Day', '2026-11-26', 0, 'US', 1, GETUTCDATE()),
        ('Day After Thanksgiving', '2026-11-27', 0, 'US', 1, GETUTCDATE()),
        ('Christmas Eve', '2026-12-24', 0, 'US', 1, GETUTCDATE()),
        ('Christmas Day', '2026-12-25', 0, 'US', 1, GETUTCDATE());
        
        PRINT '  - Created 13 US holidays for 2026';
    END
    ELSE
        PRINT '  - Holidays already exist, skipping...';
END

-- =====================================================
-- 4. PRIORITY MATRIX (Impact x Urgency)
-- =====================================================

PRINT 'Creating Priority Matrix...';

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ITSMPriorityMatrix')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ITSMPriorityMatrix WHERE Impact = 1 AND Urgency = 1)
    BEGIN
        -- Impact: 1=High, 2=Medium, 3=Low
        -- Urgency: 1=High, 2=Medium, 3=Low
        -- Priority: 1=Critical, 2=High, 3=Medium, 4=Low, 5=Planning
        INSERT INTO ITSMPriorityMatrix (Impact, Urgency, Priority, CreatedAt)
        VALUES
        (1, 1, 1), -- High Impact + High Urgency = P1 Critical
        (1, 2, 2), -- High Impact + Medium Urgency = P2 High
        (1, 3, 3), -- High Impact + Low Urgency = P3 Medium
        (2, 1, 2), -- Medium Impact + High Urgency = P2 High
        (2, 2, 3), -- Medium Impact + Medium Urgency = P3 Medium
        (2, 3, 4), -- Medium Impact + Low Urgency = P4 Low
        (3, 1, 3), -- Low Impact + High Urgency = P3 Medium
        (3, 2, 4), -- Low Impact + Medium Urgency = P4 Low
        (3, 3, 5); -- Low Impact + Low Urgency = P5 Planning
        
        PRINT '  - Created 9-cell priority matrix';
    END
    ELSE
        PRINT '  - Priority matrix already exists, skipping...';
END

-- =====================================================
-- 5. CI TYPES AND CLASSIFICATIONS
-- =====================================================

PRINT 'Creating CI Types...';

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ITSMCITypes')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ITSMCITypes WHERE TypeName = 'Server')
    BEGIN
        INSERT INTO ITSMCITypes (
            TypeName, TypeCategory, Description, 
            IconName, Color, SortOrder, 
            IsActive, CreatedAt
        )
        VALUES
        -- Hardware
        ('Server', 'Hardware', 'Physical or virtual servers', 'server', '#2196F3', 1, 1, GETUTCDATE()),
        ('Workstation', 'Hardware', 'Desktop computers and workstations', 'computer', '#4CAF50', 2, 1, GETUTCDATE()),
        ('Laptop', 'Hardware', 'Laptop computers', 'laptop', '#8BC34A', 3, 1, GETUTCDATE()),
        ('Network Device', 'Hardware', 'Routers, switches, firewalls', 'router', '#FF9800', 4, 1, GETUTCDATE()),
        ('Storage', 'Hardware', 'SAN, NAS, storage arrays', 'storage', '#9C27B0', 5, 1, GETUTCDATE()),
        ('Printer', 'Hardware', 'Printers and MFPs', 'print', '#795548', 6, 1, GETUTCDATE()),
        ('Mobile Device', 'Hardware', 'Smartphones and tablets', 'phone_iphone', '#00BCD4', 7, 1, GETUTCDATE()),
        
        -- Software
        ('Application', 'Software', 'Business applications', 'apps', '#3F51B5', 10, 1, GETUTCDATE()),
        ('Database', 'Software', 'Database instances', 'database', '#E91E63', 11, 1, GETUTCDATE()),
        ('Operating System', 'Software', 'Server and workstation OS', 'desktop_windows', '#607D8B', 12, 1, GETUTCDATE()),
        ('Middleware', 'Software', 'Application servers, message queues', 'layers', '#009688', 13, 1, GETUTCDATE()),
        ('License', 'Software', 'Software licenses', 'key', '#FFC107', 14, 1, GETUTCDATE()),
        
        -- Services
        ('Business Service', 'Service', 'Business-facing IT services', 'business', '#F44336', 20, 1, GETUTCDATE()),
        ('IT Service', 'Service', 'IT infrastructure services', 'build', '#673AB7', 21, 1, GETUTCDATE()),
        ('Cloud Service', 'Service', 'Cloud-based services (SaaS, PaaS)', 'cloud', '#03A9F4', 22, 1, GETUTCDATE()),
        
        -- Facilities
        ('Data Center', 'Facility', 'Data center facilities', 'location_city', '#FF5722', 30, 1, GETUTCDATE()),
        ('Rack', 'Facility', 'Server racks', 'view_column', '#CDDC39', 31, 1, GETUTCDATE()),
        ('UPS', 'Facility', 'Uninterruptible power supplies', 'battery_charging_full', '#FFEB3B', 32, 1, GETUTCDATE());
        
        PRINT '  - Created 18 CI types across 4 categories';
    END
    ELSE
        PRINT '  - CI types already exist, skipping...';
END

-- =====================================================
-- 6. CI RELATIONSHIP TYPES
-- =====================================================

PRINT 'Creating CI Relationship Types...';

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ITSMCIRelationshipTypes')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ITSMCIRelationshipTypes WHERE RelationshipName = 'Depends On')
    BEGIN
        INSERT INTO ITSMCIRelationshipTypes (
            RelationshipName, InverseName, Description,
            ImpactDirection, IsActive, CreatedAt
        )
        VALUES
        ('Depends On', 'Dependency Of', 'Source CI depends on target CI to function', 'Upstream', 1, GETUTCDATE()),
        ('Hosts', 'Hosted On', 'Source CI hosts/runs target CI (e.g., server hosts VM)', 'Downstream', 1, GETUTCDATE()),
        ('Contains', 'Contained In', 'Source CI physically contains target CI', 'Downstream', 1, GETUTCDATE()),
        ('Connects To', 'Connected From', 'Source CI has network connection to target CI', 'Bidirectional', 1, GETUTCDATE()),
        ('Uses', 'Used By', 'Source CI uses target CI as a resource', 'Upstream', 1, GETUTCDATE()),
        ('Manages', 'Managed By', 'Source CI manages/controls target CI', 'Downstream', 1, GETUTCDATE()),
        ('Provides Data To', 'Receives Data From', 'Source CI sends data to target CI', 'Downstream', 1, GETUTCDATE()),
        ('Backs Up', 'Backed Up By', 'Source CI provides backup for target CI', 'Downstream', 1, GETUTCDATE()),
        ('Replicated To', 'Replicated From', 'Source CI is replicated to target CI', 'Downstream', 1, GETUTCDATE()),
        ('Licensed For', 'License Of', 'Source license is assigned to target CI', 'Downstream', 1, GETUTCDATE());
        
        PRINT '  - Created 10 CI relationship types';
    END
    ELSE
        PRINT '  - CI relationship types already exist, skipping...';
END

-- =====================================================
-- 7. SERVICE CATALOG CATEGORIES
-- =====================================================

PRINT 'Creating Service Catalog Categories...';

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ITSMCatalogCategories')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ITSMCatalogCategories WHERE CategoryName = 'Hardware Requests')
    BEGIN
        INSERT INTO ITSMCatalogCategories (
            CategoryName, Description, IconName, Color,
            ParentCategoryId, SortOrder, IsActive, CreatedAt
        )
        VALUES
        -- Top-level categories
        ('Hardware Requests', 'Request new hardware equipment', 'devices', '#2196F3', NULL, 1, 1, GETUTCDATE()),
        ('Software Requests', 'Request software installation or access', 'apps', '#4CAF50', NULL, 2, 1, GETUTCDATE()),
        ('Access & Accounts', 'User accounts, permissions, and access requests', 'vpn_key', '#FF9800', NULL, 3, 1, GETUTCDATE()),
        ('IT Support', 'General IT support and troubleshooting', 'support_agent', '#9C27B0', NULL, 4, 1, GETUTCDATE()),
        ('Network & Connectivity', 'Network, VPN, and connectivity services', 'wifi', '#00BCD4', NULL, 5, 1, GETUTCDATE()),
        ('Security Services', 'Security-related requests and services', 'security', '#F44336', NULL, 6, 1, GETUTCDATE()),
        ('Cloud Services', 'Cloud infrastructure and SaaS requests', 'cloud', '#3F51B5', NULL, 7, 1, GETUTCDATE()),
        ('Communication & Collaboration', 'Email, Teams, phone systems', 'forum', '#E91E63', NULL, 8, 1, GETUTCDATE()),
        ('Facilities & AV', 'Conference room equipment, facilities tech', 'meeting_room', '#795548', NULL, 9, 1, GETUTCDATE());
        
        PRINT '  - Created 9 top-level catalog categories';
        
        -- Sub-categories for Hardware
        DECLARE @HardwareCatId INT = (SELECT CategoryId FROM ITSMCatalogCategories WHERE CategoryName = 'Hardware Requests');
        IF @HardwareCatId IS NOT NULL
        BEGIN
            INSERT INTO ITSMCatalogCategories (CategoryName, Description, IconName, Color, ParentCategoryId, SortOrder, IsActive, CreatedAt)
            VALUES
            ('Laptop', 'Request a new laptop', 'laptop', '#2196F3', @HardwareCatId, 1, 1, GETUTCDATE()),
            ('Desktop', 'Request a new desktop computer', 'computer', '#2196F3', @HardwareCatId, 2, 1, GETUTCDATE()),
            ('Monitor', 'Request additional monitors', 'desktop_windows', '#2196F3', @HardwareCatId, 3, 1, GETUTCDATE()),
            ('Peripherals', 'Keyboards, mice, docking stations', 'keyboard', '#2196F3', @HardwareCatId, 4, 1, GETUTCDATE()),
            ('Mobile Device', 'Smartphones and tablets', 'phone_iphone', '#2196F3', @HardwareCatId, 5, 1, GETUTCDATE());
            
            PRINT '  - Created 5 hardware sub-categories';
        END
        
        -- Sub-categories for Access & Accounts
        DECLARE @AccessCatId INT = (SELECT CategoryId FROM ITSMCatalogCategories WHERE CategoryName = 'Access & Accounts');
        IF @AccessCatId IS NOT NULL
        BEGIN
            INSERT INTO ITSMCatalogCategories (CategoryName, Description, IconName, Color, ParentCategoryId, SortOrder, IsActive, CreatedAt)
            VALUES
            ('New User Setup', 'Onboard new employee accounts', 'person_add', '#FF9800', @AccessCatId, 1, 1, GETUTCDATE()),
            ('Password Reset', 'Reset forgotten passwords', 'lock_reset', '#FF9800', @AccessCatId, 2, 1, GETUTCDATE()),
            ('Application Access', 'Request access to business applications', 'app_registration', '#FF9800', @AccessCatId, 3, 1, GETUTCDATE()),
            ('File Share Access', 'Request access to network shares', 'folder_shared', '#FF9800', @AccessCatId, 4, 1, GETUTCDATE()),
            ('User Offboarding', 'Disable accounts for departing employees', 'person_remove', '#FF9800', @AccessCatId, 5, 1, GETUTCDATE());
            
            PRINT '  - Created 5 access sub-categories';
        END
    END
    ELSE
        PRINT '  - Catalog categories already exist, skipping...';
END

-- =====================================================
-- 8. INCIDENT CATEGORIES
-- =====================================================

PRINT 'Creating Incident Categories...';

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ITSMIncidentCategories')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ITSMIncidentCategories WHERE CategoryName = 'Hardware')
    BEGIN
        INSERT INTO ITSMIncidentCategories (
            CategoryName, SubCategory, Description, 
            DefaultPriority, SLAPolicyId, IsActive, CreatedAt
        )
        VALUES
        -- Hardware issues
        ('Hardware', 'Laptop', 'Laptop hardware issues', 3, NULL, 1, GETUTCDATE()),
        ('Hardware', 'Desktop', 'Desktop hardware issues', 3, NULL, 1, GETUTCDATE()),
        ('Hardware', 'Printer', 'Printer hardware issues', 4, NULL, 1, GETUTCDATE()),
        ('Hardware', 'Mobile Device', 'Mobile device issues', 4, NULL, 1, GETUTCDATE()),
        ('Hardware', 'Peripheral', 'Keyboard, mouse, monitor issues', 4, NULL, 1, GETUTCDATE()),
        
        -- Software issues
        ('Software', 'Operating System', 'Windows/macOS issues', 3, NULL, 1, GETUTCDATE()),
        ('Software', 'Office Applications', 'Microsoft Office, Google Workspace', 3, NULL, 1, GETUTCDATE()),
        ('Software', 'Business Application', 'Line of business applications', 2, NULL, 1, GETUTCDATE()),
        ('Software', 'Installation', 'Software installation requests', 4, NULL, 1, GETUTCDATE()),
        ('Software', 'Performance', 'Slow application performance', 3, NULL, 1, GETUTCDATE()),
        
        -- Network issues
        ('Network', 'Internet', 'Internet connectivity issues', 2, NULL, 1, GETUTCDATE()),
        ('Network', 'VPN', 'VPN connection issues', 2, NULL, 1, GETUTCDATE()),
        ('Network', 'Wireless', 'WiFi connectivity issues', 3, NULL, 1, GETUTCDATE()),
        ('Network', 'DNS', 'DNS resolution issues', 2, NULL, 1, GETUTCDATE()),
        
        -- Account & Access
        ('Access', 'Login Issues', 'Cannot log in to systems', 2, NULL, 1, GETUTCDATE()),
        ('Access', 'Permissions', 'Missing permissions or access', 3, NULL, 1, GETUTCDATE()),
        ('Access', 'Password', 'Password expired or locked out', 2, NULL, 1, GETUTCDATE()),
        ('Access', 'MFA', 'Multi-factor authentication issues', 2, NULL, 1, GETUTCDATE()),
        
        -- Email & Communication
        ('Email', 'Outlook', 'Outlook client issues', 3, NULL, 1, GETUTCDATE()),
        ('Email', 'Webmail', 'Web-based email issues', 3, NULL, 1, GETUTCDATE()),
        ('Email', 'Calendar', 'Calendar and scheduling issues', 3, NULL, 1, GETUTCDATE()),
        ('Communication', 'Teams', 'Microsoft Teams issues', 3, NULL, 1, GETUTCDATE()),
        ('Communication', 'Phone', 'VoIP and phone issues', 2, NULL, 1, GETUTCDATE()),
        
        -- Security
        ('Security', 'Phishing', 'Suspected phishing email', 1, NULL, 1, GETUTCDATE()),
        ('Security', 'Malware', 'Suspected malware infection', 1, NULL, 1, GETUTCDATE()),
        ('Security', 'Data Breach', 'Suspected data breach', 1, NULL, 1, GETUTCDATE());
        
        PRINT '  - Created 26 incident categories';
    END
    ELSE
        PRINT '  - Incident categories already exist, skipping...';
END

-- =====================================================
-- 9. CHANGE TYPES AND RISK LEVELS
-- =====================================================

PRINT 'Creating Change Types...';

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ITSMChangeTypes')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ITSMChangeTypes WHERE TypeName = 'Standard')
    BEGIN
        INSERT INTO ITSMChangeTypes (
            TypeName, Description, 
            RequiresCAB, RequiresApproval, DefaultRiskLevel,
            LeadTimeDays, IsActive, CreatedAt
        )
        VALUES
        ('Standard', 'Pre-approved, low-risk changes with established procedures', 0, 0, 'Low', 0, 1, GETUTCDATE()),
        ('Normal', 'Changes requiring standard approval process', 0, 1, 'Medium', 5, 1, GETUTCDATE()),
        ('Emergency', 'Urgent changes to restore service - expedited approval', 0, 1, 'High', 0, 1, GETUTCDATE()),
        ('Major', 'High-risk changes requiring CAB review', 1, 1, 'High', 14, 1, GETUTCDATE()),
        ('Expedited', 'Time-sensitive normal changes with accelerated review', 0, 1, 'Medium', 2, 1, GETUTCDATE());
        
        PRINT '  - Created 5 change types';
    END
    ELSE
        PRINT '  - Change types already exist, skipping...';
END

-- =====================================================
-- 10. CHANGE BLACKOUT PERIODS
-- =====================================================

PRINT 'Creating Change Blackout Periods...';

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ITSMChangeBlackouts')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ITSMChangeBlackouts WHERE BlackoutName = 'Year-End Freeze')
    BEGIN
        INSERT INTO ITSMChangeBlackouts (
            BlackoutName, Description, 
            StartDate, EndDate, 
            IsRecurring, RecurrencePattern,
            AllowEmergencyChanges, IsActive, CreatedAt
        )
        VALUES
        ('Year-End Freeze', 'No changes during year-end close period', '2026-12-15', '2027-01-05', 0, NULL, 1, 1, GETUTCDATE()),
        ('Q1 Close', 'Quarterly financial close - limited changes', '2026-03-28', '2026-04-05', 0, NULL, 1, 1, GETUTCDATE()),
        ('Q2 Close', 'Quarterly financial close - limited changes', '2026-06-27', '2026-07-05', 0, NULL, 1, 1, GETUTCDATE()),
        ('Q3 Close', 'Quarterly financial close - limited changes', '2026-09-26', '2026-10-04', 0, NULL, 1, 1, GETUTCDATE()),
        ('Thanksgiving Week', 'Limited IT staff availability', '2026-11-23', '2026-11-29', 0, NULL, 1, 1, GETUTCDATE()),
        ('Weekend Maintenance Window', 'Standard weekend maintenance - changes allowed', '2026-01-01', '2026-12-31', 1, 'Weekly:Saturday,Sunday', 0, 1, GETUTCDATE());
        
        PRINT '  - Created 6 change blackout periods';
    END
    ELSE
        PRINT '  - Change blackout periods already exist, skipping...';
END

-- =====================================================
-- 11. SAMPLE KNOWLEDGE BASE ARTICLES
-- =====================================================

PRINT 'Creating Sample Knowledge Base Articles...';

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ITSMKnowledgeArticles')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ITSMKnowledgeArticles WHERE ArticleNumber = 'KB0000001')
    BEGIN
        INSERT INTO ITSMKnowledgeArticles (
            ArticleNumber, Title, Category, Content,
            Keywords, Status, ViewCount, HelpfulCount, NotHelpfulCount,
            Version, CreatedAt, PublishedAt
        )
        VALUES
        ('KB0000001', 'How to Reset Your Password', 'Access',
         '## How to Reset Your Password

### Self-Service Password Reset

1. Go to https://passwordreset.company.com
2. Enter your email address
3. Click "Reset Password"
4. Check your email for the reset link
5. Click the link and enter a new password

### Password Requirements
- Minimum 12 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character

### If Self-Service Does Not Work
Contact the IT Service Desk at ext. 4357 or submit a ticket.',
         'password,reset,forgot,login,access', 'Published', 1523, 1245, 42, 1, GETUTCDATE(), GETUTCDATE()),
        
        ('KB0000002', 'VPN Connection Troubleshooting', 'Network',
         '## VPN Connection Troubleshooting

### Common Issues and Solutions

#### Cannot Connect to VPN
1. Verify your internet connection is working
2. Try disconnecting and reconnecting
3. Restart the VPN client application
4. Reboot your computer

#### VPN Connected But Cannot Access Resources
1. Check if the resource is available (not under maintenance)
2. Try accessing via IP address instead of hostname
3. Flush DNS cache: Open Command Prompt and run `ipconfig /flushdns`
4. Disconnect VPN, reconnect, and try again

#### VPN Keeps Disconnecting
1. Check your internet stability
2. Try a different network if possible
3. Update the VPN client to the latest version
4. Contact IT if issue persists',
         'vpn,connection,remote,access,network', 'Published', 892, 654, 31, 1, GETUTCDATE(), GETUTCDATE()),
        
        ('KB0000003', 'Setting Up Multi-Factor Authentication (MFA)', 'Security',
         '## Setting Up Multi-Factor Authentication

### Prerequisites
- Microsoft Authenticator app installed on your mobile device
- Access to your company email

### Setup Steps

1. Sign in to https://mysignins.microsoft.com
2. Click "Security Info"
3. Click "Add method"
4. Select "Authenticator app"
5. Follow the prompts to scan the QR code with your Authenticator app
6. Enter the verification code to confirm setup

### Backup Methods
We recommend setting up at least two authentication methods:
- Authenticator app (primary)
- Phone number for SMS codes (backup)

### Lost Your Phone?
Contact the IT Service Desk immediately to reset your MFA settings.',
         'mfa,2fa,authentication,security,login,microsoft', 'Published', 756, 623, 18, 1, GETUTCDATE(), GETUTCDATE()),
        
        ('KB0000004', 'How to Request Software Installation', 'Software',
         '## How to Request Software Installation

### Self-Service Software Center
Many common applications can be installed directly from Software Center:

1. Open **Software Center** from the Start menu
2. Browse available applications
3. Click **Install** on the desired software
4. Installation will begin automatically

### Requesting Unlisted Software
If the software you need is not in Software Center:

1. Go to the IT Service Portal
2. Navigate to **Software Requests** category
3. Select **Request New Software**
4. Complete the request form including:
   - Software name and version
   - Business justification
   - Approximate cost (if known)
   - Your manager''s name for approval

### Standard Processing Time
- Self-service: Immediate
- New software requests: 3-5 business days
- Software requiring purchase: 5-10 business days',
         'software,install,application,request,download', 'Published', 445, 387, 12, 1, GETUTCDATE(), GETUTCDATE()),
        
        ('KB0000005', 'Outlook Not Syncing - Troubleshooting Guide', 'Email',
         '## Outlook Not Syncing - Troubleshooting Guide

### Quick Fixes

1. **Check Internet Connection**
   - Verify you can access other websites
   - Check if Outlook shows "Working Offline" in the status bar

2. **Send/Receive All Folders**
   - Press F9 or click Send/Receive > Send/Receive All Folders

3. **Check Outlook Status**
   - Look at the bottom-right status bar
   - Should show "Connected to: Microsoft Exchange"

### Common Solutions

#### Repair Outlook Profile
1. Close Outlook
2. Go to Control Panel > Mail > Show Profiles
3. Select your profile and click "Repair"

#### Create New Outlook Profile
1. Control Panel > Mail > Show Profiles
2. Click "Add" to create a new profile
3. Enter your email address and follow prompts

#### Clear Outlook Cache
1. Close Outlook
2. Navigate to %localappdata%\Microsoft\Outlook
3. Delete the .ost file (it will be recreated)',
         'outlook,sync,email,exchange,not working,slow', 'Published', 687, 534, 28, 1, GETUTCDATE(), GETUTCDATE());
        
        PRINT '  - Created 5 sample knowledge base articles';
    END
    ELSE
        PRINT '  - Knowledge base articles already exist, skipping...';
END

-- =====================================================
-- 12. APPROVAL WORKFLOW TEMPLATES
-- =====================================================

PRINT 'Creating Approval Workflow Templates...';

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ITSMApprovalTemplates')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ITSMApprovalTemplates WHERE TemplateName = 'Manager Approval')
    BEGIN
        INSERT INTO ITSMApprovalTemplates (
            TemplateName, Description, WorkflowType,
            ApprovalLevels, AutoApproveAfterDays,
            IsActive, CreatedAt
        )
        VALUES
        ('Manager Approval', 'Single level manager approval', 'ServiceRequest', 1, 7, 1, GETUTCDATE()),
        ('Manager + IT Approval', 'Manager approval followed by IT approval', 'ServiceRequest', 2, 5, 1, GETUTCDATE()),
        ('CAB Approval', 'Change Advisory Board approval for major changes', 'Change', 1, NULL, 1, GETUTCDATE()),
        ('Emergency Change', 'Expedited approval for emergency changes', 'Change', 1, 0, 1, GETUTCDATE()),
        ('Security Review', 'Security team review and approval', 'ServiceRequest', 1, 3, 1, GETUTCDATE()),
        ('Executive Approval', 'VP-level approval for high-cost items', 'ServiceRequest', 2, NULL, 1, GETUTCDATE());
        
        PRINT '  - Created 6 approval workflow templates';
    END
    ELSE
        PRINT '  - Approval templates already exist, skipping...';
END

-- =====================================================
-- 13. SAMPLE CONFIGURATION ITEMS (CIs)
-- =====================================================

PRINT 'Creating Sample Configuration Items...';

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ITSMConfigurationItems')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ITSMConfigurationItems WHERE Name = 'PROD-SQL-01')
    BEGIN
        INSERT INTO ITSMConfigurationItems (
            Name, CIType, Status, Environment,
            Description, Manufacturer, Model,
            SerialNumber, Location, OwnerId,
            SupportGroupId, Criticality,
            IsActive, CreatedAt
        )
        VALUES
        -- Production Servers
        ('PROD-SQL-01', 'Server', 'Active', 'Production',
         'Primary production SQL Server', 'Dell', 'PowerEdge R750',
         'SVCTAG001', 'Data Center A - Rack 12', NULL, NULL, 'Critical',
         1, GETUTCDATE()),
        ('PROD-SQL-02', 'Server', 'Active', 'Production',
         'Secondary production SQL Server (AlwaysOn replica)', 'Dell', 'PowerEdge R750',
         'SVCTAG002', 'Data Center B - Rack 08', NULL, NULL, 'Critical',
         1, GETUTCDATE()),
        ('PROD-WEB-01', 'Server', 'Active', 'Production',
         'Production web server node 1', 'Dell', 'PowerEdge R650',
         'SVCTAG003', 'Data Center A - Rack 10', NULL, NULL, 'High',
         1, GETUTCDATE()),
        ('PROD-WEB-02', 'Server', 'Active', 'Production',
         'Production web server node 2', 'Dell', 'PowerEdge R650',
         'SVCTAG004', 'Data Center B - Rack 06', NULL, NULL, 'High',
         1, GETUTCDATE()),
        
        -- Development Servers
        ('DEV-APP-01', 'Server', 'Active', 'Development',
         'Development application server', 'VMware', 'Virtual Machine',
         NULL, 'Virtual - ESXi Cluster 1', NULL, NULL, 'Low',
         1, GETUTCDATE()),
        
        -- Network Devices
        ('CORE-SW-01', 'Network Device', 'Active', 'Production',
         'Core network switch - Data Center A', 'Cisco', 'Nexus 9300',
         'FCW2345G1AB', 'Data Center A - MDF', NULL, NULL, 'Critical',
         1, GETUTCDATE()),
        ('FW-EXT-01', 'Network Device', 'Active', 'Production',
         'External firewall', 'Palo Alto', 'PA-5220',
         'PA5220001234', 'Data Center A - Security Cage', NULL, NULL, 'Critical',
         1, GETUTCDATE()),
        
        -- Applications
        ('CRM-PROD', 'Application', 'Active', 'Production',
         'CRM Solution - Production Instance', 'Internal', 'CRM Solution v2.1',
         NULL, 'Azure Cloud', NULL, NULL, 'Critical',
         1, GETUTCDATE()),
        ('ERP-PROD', 'Application', 'Active', 'Production',
         'Enterprise Resource Planning System', 'SAP', 'S/4HANA',
         NULL, 'Data Center A', NULL, NULL, 'Critical',
         1, GETUTCDATE()),
        
        -- Business Services
        ('SVC-EMAIL', 'Business Service', 'Active', 'Production',
         'Corporate Email Service (Microsoft 365)', 'Microsoft', 'Exchange Online',
         NULL, 'Microsoft Cloud', NULL, NULL, 'Critical',
         1, GETUTCDATE()),
        ('SVC-INTRANET', 'Business Service', 'Active', 'Production',
         'Corporate Intranet Portal', 'Microsoft', 'SharePoint Online',
         NULL, 'Microsoft Cloud', NULL, NULL, 'High',
         1, GETUTCDATE());
        
        PRINT '  - Created 11 sample configuration items';
    END
    ELSE
        PRINT '  - Configuration items already exist, skipping...';
END

PRINT '';
PRINT '============================================';
PRINT 'ITSM Seed Data Population Complete!';
PRINT '============================================';
PRINT '';

COMMIT TRANSACTION;
GO
