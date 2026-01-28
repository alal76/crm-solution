-- ============================================================================
-- CRM Solution Database Seed Data - Lookup Categories and Items
-- Version: 1.0
-- Date: 2026-01-28
-- Description: Lookup/dropdown values for all CRM modules
-- Note: Matches actual database schema structure
-- ============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================================
-- Lookup Categories
-- Columns: Id, Name, Description, IsActive, CreatedAt, UpdatedAt, IsDeleted
-- ============================================================================
INSERT INTO LookupCategories (Id, Name, Description, IsActive, CreatedAt, IsDeleted) VALUES
(1, 'LeadSource', 'Source channels for leads', 1, NOW(), 0),
(2, 'LeadStatus', 'Lead lifecycle statuses', 1, NOW(), 0),
(3, 'OpportunityStage', 'Sales opportunity pipeline stages', 1, NOW(), 0),
(4, 'ContactSource', 'How contacts were acquired', 1, NOW(), 0),
(5, 'AccountIndustry', 'Industry classification for accounts', 1, NOW(), 0),
(6, 'TaskType', 'Types of tasks/activities', 1, NOW(), 0),
(7, 'TaskStatus', 'Task completion statuses', 1, NOW(), 0),
(8, 'TaskPriority', 'Task priority levels', 1, NOW(), 0),
(9, 'QuoteStatus', 'Quote/proposal statuses', 1, NOW(), 0),
(10, 'CampaignType', 'Marketing campaign types', 1, NOW(), 0),
(11, 'CampaignStatus', 'Campaign lifecycle statuses', 1, NOW(), 0),
(12, 'ProductCategory', 'Product/service categories', 1, NOW(), 0),
(13, 'NoteType', 'Note/memo types', 1, NOW(), 0),
(14, 'Priority', 'General priority levels', 1, NOW(), 0),
(15, 'Currency', 'Supported currencies', 1, NOW(), 0),
(16, 'Country', 'Countries for addresses', 1, NOW(), 0),
(17, 'Region', 'Sales/geographic regions', 1, NOW(), 0),
(18, 'ContactRole', 'Role of contact within account', 1, NOW(), 0),
(19, 'PaymentTerms', 'Payment term options', 1, NOW(), 0),
(20, 'AccountLifecycleStage', 'Account lifecycle stages', 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Name = VALUES(Name), Description = VALUES(Description);

-- ============================================================================
-- LookupItems 
-- Columns: Id, LookupCategoryId, Key, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted
-- Key = code/identifier, Value = display text, Meta = JSON metadata
-- ============================================================================

-- Lead Source Items (Category 1)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
(1, 'WEB', 'Website', '{"default":true}', 1, 1, NOW(), 0),
(1, 'REF', 'Referral', NULL, 2, 1, NOW(), 0),
(1, 'SOC', 'Social Media', NULL, 3, 1, NOW(), 0),
(1, 'EMAIL', 'Email Campaign', NULL, 4, 1, NOW(), 0),
(1, 'TRADE', 'Trade Show', NULL, 5, 1, NOW(), 0),
(1, 'COLD', 'Cold Call', NULL, 6, 1, NOW(), 0),
(1, 'PARTNER', 'Partner', NULL, 7, 1, NOW(), 0),
(1, 'PPC', 'Paid Search', NULL, 8, 1, NOW(), 0),
(1, 'SEO', 'Organic Search', NULL, 9, 1, NOW(), 0),
(1, 'WEBINAR', 'Webinar', NULL, 10, 1, NOW(), 0),
(1, 'MAIL', 'Direct Mail', NULL, 11, 1, NOW(), 0),
(1, 'OTHER', 'Other', NULL, 99, 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Value = VALUES(Value);

-- Lead Status Items (Category 2)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
(2, 'NEW', 'New', '{"default":true,"color":"#9e9e9e"}', 1, 1, NOW(), 0),
(2, 'CONTACT', 'Contacted', '{"color":"#2196f3"}', 2, 1, NOW(), 0),
(2, 'WORK', 'Working', '{"color":"#03a9f4"}', 3, 1, NOW(), 0),
(2, 'NURTURE', 'Nurturing', '{"color":"#00bcd4"}', 4, 1, NOW(), 0),
(2, 'QUAL', 'Qualified', '{"color":"#4caf50"}', 5, 1, NOW(), 0),
(2, 'UNQUAL', 'Unqualified', '{"color":"#ff5722"}', 6, 1, NOW(), 0),
(2, 'CONV', 'Converted', '{"color":"#9c27b0"}', 7, 1, NOW(), 0),
(2, 'LOST', 'Lost', '{"color":"#f44336"}', 8, 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Value = VALUES(Value);

-- Opportunity Stage Items (Category 3)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
(3, 'PROSP', 'Prospecting', '{"default":true,"probability":10,"color":"#9e9e9e"}', 1, 1, NOW(), 0),
(3, 'QUAL', 'Qualification', '{"probability":20,"color":"#2196f3"}', 2, 1, NOW(), 0),
(3, 'NEEDS', 'Needs Analysis', '{"probability":40,"color":"#03a9f4"}', 3, 1, NOW(), 0),
(3, 'VALUE', 'Value Proposition', '{"probability":50,"color":"#00bcd4"}', 4, 1, NOW(), 0),
(3, 'PROP', 'Proposal', '{"probability":60,"color":"#ff9800"}', 5, 1, NOW(), 0),
(3, 'NEG', 'Negotiation', '{"probability":75,"color":"#ff5722"}', 6, 1, NOW(), 0),
(3, 'WON', 'Closed Won', '{"probability":100,"color":"#4caf50","isClosed":true}', 7, 1, NOW(), 0),
(3, 'LOST', 'Closed Lost', '{"probability":0,"color":"#f44336","isClosed":true}', 8, 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Value = VALUES(Value);

-- Contact Source Items (Category 4)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
(4, 'WEB', 'Website Form', '{"default":true}', 1, 1, NOW(), 0),
(4, 'IMPORT', 'Import', NULL, 2, 1, NOW(), 0),
(4, 'MANUAL', 'Manual Entry', NULL, 3, 1, NOW(), 0),
(4, 'API', 'API Integration', NULL, 4, 1, NOW(), 0),
(4, 'LEAD', 'Lead Conversion', NULL, 5, 1, NOW(), 0),
(4, 'REF', 'Referral', NULL, 6, 1, NOW(), 0),
(4, 'EVENT', 'Event', NULL, 7, 1, NOW(), 0),
(4, 'SOCIAL', 'Social Media', NULL, 8, 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Value = VALUES(Value);

-- Account Industry Items (Category 5)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
(5, 'TECH', 'Technology', NULL, 1, 1, NOW(), 0),
(5, 'HEALTH', 'Healthcare', NULL, 2, 1, NOW(), 0),
(5, 'FIN', 'Finance', NULL, 3, 1, NOW(), 0),
(5, 'MFG', 'Manufacturing', NULL, 4, 1, NOW(), 0),
(5, 'RETAIL', 'Retail', NULL, 5, 1, NOW(), 0),
(5, 'EDU', 'Education', NULL, 6, 1, NOW(), 0),
(5, 'GOV', 'Government', NULL, 7, 1, NOW(), 0),
(5, 'NPO', 'Non-Profit', NULL, 8, 1, NOW(), 0),
(5, 'PROF', 'Professional Services', NULL, 9, 1, NOW(), 0),
(5, 'RE', 'Real Estate', NULL, 10, 1, NOW(), 0),
(5, 'HOSP', 'Hospitality', NULL, 11, 1, NOW(), 0),
(5, 'TRANS', 'Transportation', NULL, 12, 1, NOW(), 0),
(5, 'ENERGY', 'Energy', NULL, 13, 1, NOW(), 0),
(5, 'TELECOM', 'Telecommunications', NULL, 14, 1, NOW(), 0),
(5, 'MEDIA', 'Media & Entertainment', NULL, 15, 1, NOW(), 0),
(5, 'OTHER', 'Other', '{"default":true}', 99, 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Value = VALUES(Value);

-- Task Type Items (Category 6)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
(6, 'CALL', 'Call', '{"default":true,"icon":"phone"}', 1, 1, NOW(), 0),
(6, 'EMAIL', 'Email', '{"icon":"email"}', 2, 1, NOW(), 0),
(6, 'MEET', 'Meeting', '{"icon":"event"}', 3, 1, NOW(), 0),
(6, 'FOLLOW', 'Follow-Up', '{"icon":"refresh"}', 4, 1, NOW(), 0),
(6, 'DEMO', 'Demo', '{"icon":"computer"}', 5, 1, NOW(), 0),
(6, 'PROP', 'Proposal', '{"icon":"description"}', 6, 1, NOW(), 0),
(6, 'RESEARCH', 'Research', '{"icon":"search"}', 7, 1, NOW(), 0),
(6, 'DOC', 'Documentation', '{"icon":"article"}', 8, 1, NOW(), 0),
(6, 'REVIEW', 'Review', '{"icon":"check_circle"}', 9, 1, NOW(), 0),
(6, 'APPROVE', 'Approval', '{"icon":"thumb_up"}', 10, 1, NOW(), 0),
(6, 'OTHER', 'Other', '{"icon":"task"}', 99, 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Value = VALUES(Value);

-- Task Status Items (Category 7)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
(7, 'NEW', 'Not Started', '{"default":true,"color":"#9e9e9e"}', 1, 1, NOW(), 0),
(7, 'PROG', 'In Progress', '{"color":"#2196f3"}', 2, 1, NOW(), 0),
(7, 'DONE', 'Completed', '{"color":"#4caf50","isComplete":true}', 3, 1, NOW(), 0),
(7, 'DEFER', 'Deferred', '{"color":"#ff9800"}', 4, 1, NOW(), 0),
(7, 'WAIT', 'Waiting', '{"color":"#607d8b"}', 5, 1, NOW(), 0),
(7, 'CANCEL', 'Cancelled', '{"color":"#f44336","isCancelled":true}', 6, 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Value = VALUES(Value);

-- Task Priority Items (Category 8)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
(8, 'LOW', 'Low', '{"color":"#9e9e9e","value":0}', 1, 1, NOW(), 0),
(8, 'NORMAL', 'Normal', '{"default":true,"color":"#2196f3","value":1}', 2, 1, NOW(), 0),
(8, 'HIGH', 'High', '{"color":"#ff9800","value":2}', 3, 1, NOW(), 0),
(8, 'URGENT', 'Urgent', '{"color":"#f44336","value":3}', 4, 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Value = VALUES(Value);

-- Quote Status Items (Category 9)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
(9, 'DRAFT', 'Draft', '{"default":true,"color":"#9e9e9e"}', 1, 1, NOW(), 0),
(9, 'PEND', 'Pending Approval', '{"color":"#ff9800"}', 2, 1, NOW(), 0),
(9, 'SENT', 'Sent', '{"color":"#2196f3"}', 3, 1, NOW(), 0),
(9, 'ACCEPT', 'Accepted', '{"color":"#4caf50"}', 4, 1, NOW(), 0),
(9, 'REJECT', 'Rejected', '{"color":"#f44336"}', 5, 1, NOW(), 0),
(9, 'EXPIRE', 'Expired', '{"color":"#795548"}', 6, 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Value = VALUES(Value);

-- Campaign Type Items (Category 10)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
(10, 'EMAIL', 'Email', '{"default":true,"icon":"email"}', 1, 1, NOW(), 0),
(10, 'SOCIAL', 'Social Media', '{"icon":"share"}', 2, 1, NOW(), 0),
(10, 'PPC', 'Paid Search', '{"icon":"search"}', 3, 1, NOW(), 0),
(10, 'DISPLAY', 'Display Ads', '{"icon":"image"}', 4, 1, NOW(), 0),
(10, 'CONTENT', 'Content Marketing', '{"icon":"article"}', 5, 1, NOW(), 0),
(10, 'SEO', 'SEO', '{"icon":"trending_up"}', 6, 1, NOW(), 0),
(10, 'EVENT', 'Events', '{"icon":"event"}', 7, 1, NOW(), 0),
(10, 'WEBINAR', 'Webinar', '{"icon":"videocam"}', 8, 1, NOW(), 0),
(10, 'TRADE', 'Trade Show', '{"icon":"business"}', 9, 1, NOW(), 0),
(10, 'MAIL', 'Direct Mail', '{"icon":"mail"}', 10, 1, NOW(), 0),
(10, 'REF', 'Referral', '{"icon":"people"}', 11, 1, NOW(), 0),
(10, 'PARTNER', 'Partner', '{"icon":"handshake"}', 12, 1, NOW(), 0),
(10, 'PR', 'PR', '{"icon":"newspaper"}', 13, 1, NOW(), 0),
(10, 'VIDEO', 'Video', '{"icon":"play_circle"}', 14, 1, NOW(), 0),
(10, 'PODCAST', 'Podcast', '{"icon":"mic"}', 15, 1, NOW(), 0),
(10, 'OTHER', 'Other', '{"icon":"category"}', 99, 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Value = VALUES(Value);

-- Campaign Status Items (Category 11)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
(11, 'DRAFT', 'Draft', '{"default":true,"color":"#9e9e9e"}', 1, 1, NOW(), 0),
(11, 'SCHED', 'Scheduled', '{"color":"#2196f3"}', 2, 1, NOW(), 0),
(11, 'ACTIVE', 'Active', '{"color":"#4caf50"}', 3, 1, NOW(), 0),
(11, 'PAUSE', 'Paused', '{"color":"#ff9800"}', 4, 1, NOW(), 0),
(11, 'DONE', 'Completed', '{"color":"#9c27b0"}', 5, 1, NOW(), 0),
(11, 'CANCEL', 'Cancelled', '{"color":"#f44336"}', 6, 1, NOW(), 0),
(11, 'ARCHIVE', 'Archived', '{"color":"#607d8b"}', 7, 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Value = VALUES(Value);

-- Product Category Items (Category 12)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
(12, 'HW', 'Hardware', NULL, 1, 1, NOW(), 0),
(12, 'SW', 'Software', '{"default":true}', 2, 1, NOW(), 0),
(12, 'SVC', 'Services', NULL, 3, 1, NOW(), 0),
(12, 'SUB', 'Subscription', NULL, 4, 1, NOW(), 0),
(12, 'SUP', 'Support', NULL, 5, 1, NOW(), 0),
(12, 'TRAIN', 'Training', NULL, 6, 1, NOW(), 0),
(12, 'CONS', 'Consulting', NULL, 7, 1, NOW(), 0),
(12, 'BUNDLE', 'Bundles', NULL, 8, 1, NOW(), 0),
(12, 'ACC', 'Accessories', NULL, 9, 1, NOW(), 0),
(12, 'OTHER', 'Other', NULL, 99, 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Value = VALUES(Value);

-- Note Type Items (Category 13)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
(13, 'GEN', 'General', '{"default":true}', 1, 1, NOW(), 0),
(13, 'CALL', 'Call Log', NULL, 2, 1, NOW(), 0),
(13, 'MEET', 'Meeting Notes', NULL, 3, 1, NOW(), 0),
(13, 'EMAIL', 'Email Summary', NULL, 4, 1, NOW(), 0),
(13, 'FOLLOW', 'Follow-Up', NULL, 5, 1, NOW(), 0),
(13, 'INT', 'Internal', NULL, 6, 1, NOW(), 0),
(13, 'REMIND', 'Reminder', NULL, 7, 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Value = VALUES(Value);

-- Priority Items (Category 14)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
(14, 'LOW', 'Low', '{"color":"#9e9e9e","value":0}', 1, 1, NOW(), 0),
(14, 'MED', 'Medium', '{"default":true,"color":"#2196f3","value":1}', 2, 1, NOW(), 0),
(14, 'HIGH', 'High', '{"color":"#ff9800","value":2}', 3, 1, NOW(), 0),
(14, 'CRIT', 'Critical', '{"color":"#f44336","value":3}', 4, 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Value = VALUES(Value);

-- Currency Items (Category 15)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
(15, 'USD', 'US Dollar', '{"default":true,"symbol":"$","decimalPlaces":2}', 1, 1, NOW(), 0),
(15, 'EUR', 'Euro', '{"symbol":"€","decimalPlaces":2}', 2, 1, NOW(), 0),
(15, 'GBP', 'British Pound', '{"symbol":"£","decimalPlaces":2}', 3, 1, NOW(), 0),
(15, 'INR', 'Indian Rupee', '{"symbol":"₹","decimalPlaces":2}', 4, 1, NOW(), 0),
(15, 'CAD', 'Canadian Dollar', '{"symbol":"C$","decimalPlaces":2}', 5, 1, NOW(), 0),
(15, 'AUD', 'Australian Dollar', '{"symbol":"A$","decimalPlaces":2}', 6, 1, NOW(), 0),
(15, 'JPY', 'Japanese Yen', '{"symbol":"¥","decimalPlaces":0}', 7, 1, NOW(), 0),
(15, 'CHF', 'Swiss Franc', '{"symbol":"CHF","decimalPlaces":2}', 8, 1, NOW(), 0),
(15, 'CNY', 'Chinese Yuan', '{"symbol":"¥","decimalPlaces":2}', 9, 1, NOW(), 0),
(15, 'SGD', 'Singapore Dollar', '{"symbol":"S$","decimalPlaces":2}', 10, 1, NOW(), 0),
(15, 'AED', 'UAE Dirham', '{"symbol":"د.إ","decimalPlaces":2}', 11, 1, NOW(), 0),
(15, 'BRL', 'Brazilian Real', '{"symbol":"R$","decimalPlaces":2}', 12, 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Value = VALUES(Value);

-- Country Items (Category 16)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
(16, 'US', 'United States', '{"default":true}', 1, 1, NOW(), 0),
(16, 'GB', 'United Kingdom', NULL, 2, 1, NOW(), 0),
(16, 'CA', 'Canada', NULL, 3, 1, NOW(), 0),
(16, 'AU', 'Australia', NULL, 4, 1, NOW(), 0),
(16, 'DE', 'Germany', NULL, 5, 1, NOW(), 0),
(16, 'FR', 'France', NULL, 6, 1, NOW(), 0),
(16, 'IN', 'India', NULL, 7, 1, NOW(), 0),
(16, 'JP', 'Japan', NULL, 8, 1, NOW(), 0),
(16, 'CN', 'China', NULL, 9, 1, NOW(), 0),
(16, 'BR', 'Brazil', NULL, 10, 1, NOW(), 0),
(16, 'MX', 'Mexico', NULL, 11, 1, NOW(), 0),
(16, 'IT', 'Italy', NULL, 12, 1, NOW(), 0),
(16, 'ES', 'Spain', NULL, 13, 1, NOW(), 0),
(16, 'NL', 'Netherlands', NULL, 14, 1, NOW(), 0),
(16, 'SG', 'Singapore', NULL, 15, 1, NOW(), 0),
(16, 'CH', 'Switzerland', NULL, 16, 1, NOW(), 0),
(16, 'AE', 'United Arab Emirates', NULL, 17, 1, NOW(), 0),
(16, 'KR', 'South Korea', NULL, 18, 1, NOW(), 0),
(16, 'IE', 'Ireland', NULL, 19, 1, NOW(), 0),
(16, 'NZ', 'New Zealand', NULL, 20, 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Value = VALUES(Value);

-- Region Items (Category 17)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
(17, 'NA', 'North America', '{"default":true}', 1, 1, NOW(), 0),
(17, 'EMEA', 'EMEA', NULL, 2, 1, NOW(), 0),
(17, 'APAC', 'APAC', NULL, 3, 1, NOW(), 0),
(17, 'LATAM', 'LATAM', NULL, 4, 1, NOW(), 0),
(17, 'ANZ', 'ANZ', NULL, 5, 1, NOW(), 0),
(17, 'INDIA', 'India', NULL, 6, 1, NOW(), 0),
(17, 'GLOBAL', 'Global', NULL, 7, 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Value = VALUES(Value);

-- Contact Role Items (Category 18)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
(18, 'PRIM', 'Primary', '{"default":true}', 1, 1, NOW(), 0),
(18, 'SEC', 'Secondary', NULL, 2, 1, NOW(), 0),
(18, 'BILL', 'Billing', NULL, 3, 1, NOW(), 0),
(18, 'TECH', 'Technical', NULL, 4, 1, NOW(), 0),
(18, 'DM', 'Decision Maker', NULL, 5, 1, NOW(), 0),
(18, 'INF', 'Influencer', NULL, 6, 1, NOW(), 0),
(18, 'USER', 'End User', NULL, 7, 1, NOW(), 0),
(18, 'EXEC', 'Executive', NULL, 8, 1, NOW(), 0),
(18, 'PROC', 'Procurement', NULL, 9, 1, NOW(), 0),
(18, 'OTHER', 'Other', NULL, 99, 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Value = VALUES(Value);

-- Payment Terms Items (Category 19)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
(19, 'NET15', 'Net 15', '{"days":15}', 1, 1, NOW(), 0),
(19, 'NET30', 'Net 30', '{"default":true,"days":30}', 2, 1, NOW(), 0),
(19, 'NET45', 'Net 45', '{"days":45}', 3, 1, NOW(), 0),
(19, 'NET60', 'Net 60', '{"days":60}', 4, 1, NOW(), 0),
(19, 'NET90', 'Net 90', '{"days":90}', 5, 1, NOW(), 0),
(19, 'DOR', 'Due on Receipt', '{"days":0}', 6, 1, NOW(), 0),
(19, '210NET30', '2/10 Net 30', '{"days":30,"discountDays":10,"discountPercent":2}', 7, 1, NOW(), 0),
(19, 'PREPAID', 'Prepaid', '{"days":-1}', 8, 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Value = VALUES(Value);

-- Account Lifecycle Stage Items (Category 20)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted) VALUES
(20, 'OTHER', 'Other', '{"default":true,"color":"#9e9e9e","value":0}', 1, 1, NOW(), 0),
(20, 'LEAD', 'Lead', '{"color":"#2196f3","value":1}', 2, 1, NOW(), 0),
(20, 'OPP', 'Opportunity', '{"color":"#ff9800","value":2}', 3, 1, NOW(), 0),
(20, 'ACTIVE', 'Active', '{"color":"#4caf50","value":3}', 4, 1, NOW(), 0),
(20, 'RISK', 'At Risk', '{"color":"#f44336","value":4}', 5, 1, NOW(), 0),
(20, 'CHURN', 'Churned', '{"color":"#607d8b","value":5}', 6, 1, NOW(), 0),
(20, 'WINBACK', 'Win-back', '{"color":"#9c27b0","value":6}', 7, 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Value = VALUES(Value);

SET FOREIGN_KEY_CHECKS = 1;
