-- ============================================================================
-- CRM Solution Database Seed Data - DEMO
-- Version: 2.0
-- Description: Full demonstration data with 3+ types of each option per entity
-- Copyright (C) 2024-2026 Abhishek Lal
-- Licensed under the GNU Affero General Public License v3.0
-- ============================================================================
-- Prerequisites: Run 01_essential_data.sql and 02_basic_data.sql first
-- This file adds comprehensive demo data for full system demonstration
-- ============================================================================

SET client_encoding = 'UTF8';

-- ============================================================================
-- SECTION 1: ADDITIONAL USERS (Various roles and departments)
-- ============================================================================

INSERT INTO "Users" ("Id", "Username", "Email", "PasswordHash", "FirstName", "LastName", 
    "IsActive", "UserGroupId", "DepartmentId", "CreatedAt", "IsDeleted", "EmailConfirmed",
    "PhoneNumber", "Title", "MustChangePassword")
VALUES 
    -- Marketing Team
    (5, 'lchen', 'lisa.chen@crm.local', '$2a$11$8K1p/a0dL1LXMIgoEDFrwOexJujLDl1CZa4GlX2NqwONNlZwGLGHC', 
     'Lisa', 'Chen', true, 4, 2, NOW(), false, true, '+1-555-0104', 'Marketing Manager', false),
    (6, 'kpatel', 'kevin.patel@crm.local', '$2a$11$8K1p/a0dL1LXMIgoEDFrwOexJujLDl1CZa4GlX2NqwONNlZwGLGHC', 
     'Kevin', 'Patel', true, 5, 2, NOW(), false, true, '+1-555-0105', 'Marketing Specialist', false),
    -- Support Team
    (7, 'agarcia', 'anna.garcia@crm.local', '$2a$11$8K1p/a0dL1LXMIgoEDFrwOexJujLDl1CZa4GlX2NqwONNlZwGLGHC', 
     'Anna', 'Garcia', true, 6, 3, NOW(), false, true, '+1-555-0106', 'Support Manager', false),
    (8, 'tbrown', 'tom.brown@crm.local', '$2a$11$8K1p/a0dL1LXMIgoEDFrwOexJujLDl1CZa4GlX2NqwONNlZwGLGHC', 
     'Tom', 'Brown', true, 7, 3, NOW(), false, true, '+1-555-0107', 'Support Agent', false),
    -- Additional Sales
    (9, 'swilson', 'steve.wilson@crm.local', '$2a$11$8K1p/a0dL1LXMIgoEDFrwOexJujLDl1CZa4GlX2NqwONNlZwGLGHC', 
     'Steve', 'Wilson', true, 3, 1, NOW(), false, true, '+1-555-0108', 'Sales Representative', false),
    (10, 'jlee', 'jessica.lee@crm.local', '$2a$11$8K1p/a0dL1LXMIgoEDFrwOexJujLDl1CZa4GlX2NqwONNlZwGLGHC', 
     'Jessica', 'Lee', true, 3, 1, NOW(), false, true, '+1-555-0109', 'Sales Representative', false)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"Users_Id_seq"', (SELECT MAX("Id") FROM "Users"));

-- ============================================================================
-- SECTION 2: ADDITIONAL CUSTOMERS (Various types and industries)
-- ============================================================================

INSERT INTO "Customers" ("Id", "Category", "Company", "LegalName", "Email", "Phone",
    "CustomerType", "LifecycleStage", "Priority", "Industry", "Website", "AnnualRevenue",
    "Address", "City", "State", "PostalCode", "Country",
    "IsActive", "CreatedAt", "IsDeleted", "OwnerId", "FirstName", "LastName")
VALUES 
    -- Technology companies
    (4, 1, 'CloudTech Solutions', 'CloudTech Solutions Inc', 
     'info@cloudtech.com', '+1-555-0401',
     2, 3, 2, 'Technology', 'https://cloudtech.com', 8500000.00,
     '200 Cloud Drive', 'San Francisco', 'CA', '94102', 'United States',
     true, NOW() - INTERVAL '90 days', false, 3, NULL, NULL),
    
    (5, 1, 'DataStream Analytics', 'DataStream Analytics LLC', 
     'contact@datastream.io', '+1-555-0402',
     1, 2, 1, 'Technology', 'https://datastream.io', 2200000.00,
     '500 Analytics Way', 'Seattle', 'WA', '98102', 'United States',
     true, NOW() - INTERVAL '60 days', false, 9, NULL, NULL),
    
    (6, 1, 'AI Innovations Corp', 'AI Innovations Corporation', 
     'hello@aiinnovations.ai', '+1-555-0403',
     3, 4, 3, 'Technology', 'https://aiinnovations.ai', 45000000.00,
     '1000 AI Boulevard', 'Palo Alto', 'CA', '94301', 'United States',
     true, NOW() - INTERVAL '180 days', false, 2, NULL, NULL),
    
    -- Healthcare companies
    (7, 1, 'MedCare Systems', 'MedCare Systems Inc', 
     'info@medcare.com', '+1-555-0404',
     2, 3, 2, 'Healthcare', 'https://medcare.com', 12000000.00,
     '800 Health Center', 'Boston', 'MA', '02102', 'United States',
     true, NOW() - INTERVAL '120 days', false, 3, NULL, NULL),
    
    (8, 1, 'BioTech Research Labs', 'BioTech Research Laboratories Inc', 
     'contact@biotechresearch.com', '+1-555-0405',
     3, 3, 3, 'Healthcare', 'https://biotechresearch.com', 75000000.00,
     '1500 Research Park', 'San Diego', 'CA', '92101', 'United States',
     true, NOW() - INTERVAL '200 days', false, 2, NULL, NULL),
    
    -- Finance companies
    (9, 1, 'SecureFinance Group', 'SecureFinance Group LLC', 
     'info@securefinance.com', '+1-555-0406',
     2, 3, 2, 'Finance & Banking', 'https://securefinance.com', 25000000.00,
     '600 Wall Street', 'New York', 'NY', '10005', 'United States',
     true, NOW() - INTERVAL '150 days', false, 3, NULL, NULL),
    
    (10, 1, 'FinTech Pioneers', 'FinTech Pioneers Inc', 
     'hello@fintechpioneers.io', '+1-555-0407',
     1, 2, 1, 'Finance & Banking', 'https://fintechpioneers.io', 3500000.00,
     '100 Fintech Lane', 'Austin', 'TX', '78702', 'United States',
     true, NOW() - INTERVAL '45 days', false, 10, NULL, NULL),
    
    -- Manufacturing companies
    (11, 1, 'Precision Manufacturing Co', 'Precision Manufacturing Company', 
     'sales@precisionmfg.com', '+1-555-0408',
     2, 3, 2, 'Manufacturing', 'https://precisionmfg.com', 18000000.00,
     '2000 Industrial Way', 'Detroit', 'MI', '48201', 'United States',
     true, NOW() - INTERVAL '100 days', false, 9, NULL, NULL),
    
    (12, 1, 'AutoParts Express', 'AutoParts Express Inc', 
     'info@autopartsexpress.com', '+1-555-0409',
     3, 4, 3, 'Manufacturing', 'https://autopartsexpress.com', 65000000.00,
     '3000 Parts Boulevard', 'Cleveland', 'OH', '44101', 'United States',
     true, NOW() - INTERVAL '250 days', false, 2, NULL, NULL),
    
    -- Retail companies
    (13, 1, 'Urban Retail Group', 'Urban Retail Group LLC', 
     'contact@urbanretail.com', '+1-555-0410',
     2, 3, 2, 'Retail', 'https://urbanretail.com', 9500000.00,
     '400 Retail Plaza', 'Los Angeles', 'CA', '90001', 'United States',
     true, NOW() - INTERVAL '80 days', false, 3, NULL, NULL),
    
    (14, 1, 'E-Commerce Masters', 'E-Commerce Masters Inc', 
     'sales@ecommercemasters.com', '+1-555-0411',
     1, 2, 1, 'Retail', 'https://ecommercemasters.com', 4200000.00,
     '150 Digital Drive', 'Portland', 'OR', '97201', 'United States',
     true, NOW() - INTERVAL '30 days', false, 10, NULL, NULL),
    
    -- Additional individuals
    (15, 0, NULL, NULL, 'mark.taylor@email.com', '+1-555-0412',
     0, 2, 0, NULL, NULL, NULL,
     '789 Home Street', 'Phoenix', 'AZ', '85001', 'United States',
     true, NOW() - INTERVAL '20 days', false, 9, 'Mark', 'Taylor'),
    
    (16, 0, NULL, NULL, 'susan.davis@email.com', '+1-555-0413',
     0, 3, 1, NULL, NULL, NULL,
     '456 Oak Avenue', 'Miami', 'FL', '33101', 'United States',
     true, NOW() - INTERVAL '60 days', false, 3, 'Susan', 'Davis'),
    
    (17, 0, NULL, NULL, 'richard.moore@email.com', '+1-555-0414',
     0, 4, 2, NULL, NULL, NULL,
     '123 Elm Street', 'Atlanta', 'GA', '30301', 'United States',
     true, NOW() - INTERVAL '90 days', false, 10, 'Richard', 'Moore'),
    
    -- International customers
    (18, 1, 'Euro Tech Partners', 'Euro Tech Partners GmbH', 
     'info@eurotech.de', '+49-30-555-0101',
     2, 3, 2, 'Technology', 'https://eurotech.de', 15000000.00,
     'Technologie Strasse 100', 'Berlin', 'BE', '10115', 'Germany',
     true, NOW() - INTERVAL '120 days', false, 2, NULL, NULL),
    
    (19, 1, 'Asia Pacific Solutions', 'Asia Pacific Solutions Pte Ltd', 
     'contact@apac-solutions.sg', '+65-6555-0101',
     3, 4, 3, 'Technology', 'https://apac-solutions.sg', 35000000.00,
     '50 Marina Bay', 'Singapore', 'SG', '018983', 'Singapore',
     true, NOW() - INTERVAL '180 days', false, 2, NULL, NULL),
    
    (20, 1, 'UK Enterprises Ltd', 'UK Enterprises Limited', 
     'info@ukenterprises.co.uk', '+44-20-5550-0101',
     2, 3, 2, 'Professional Services', 'https://ukenterprises.co.uk', 22000000.00,
     '10 London Bridge', 'London', 'LND', 'SE1 2UP', 'United Kingdom',
     true, NOW() - INTERVAL '100 days', false, 3, NULL, NULL)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"Customers_Id_seq"', (SELECT MAX("Id") FROM "Customers"));

-- ============================================================================
-- SECTION 3: ADDITIONAL CONTACTS
-- ============================================================================

-- Contacts for tech companies
INSERT INTO "Contacts" ("Id", "FirstName", "LastName", "Email", "Phone", "Title",
    "CustomerId", "IsPrimary", "IsActive", "CreatedAt", "IsDeleted")
VALUES 
    (6, 'Patricia', 'White', 'patricia.white@cloudtech.com', '+1-555-0501', 'CEO', 4, true, true, NOW(), false),
    (7, 'Jason', 'Kim', 'jason.kim@cloudtech.com', '+1-555-0502', 'CTO', 4, false, true, NOW(), false),
    
    (8, 'Daniel', 'Anderson', 'daniel.anderson@datastream.io', '+1-555-0503', 'Founder', 5, true, true, NOW(), false),
    
    (9, 'Michelle', 'Taylor', 'michelle.taylor@aiinnovations.ai', '+1-555-0504', 'VP Engineering', 6, true, true, NOW(), false),
    (10, 'Christopher', 'Harris', 'christopher.harris@aiinnovations.ai', '+1-555-0505', 'CFO', 6, false, true, NOW(), false),
    (11, 'Amanda', 'Clark', 'amanda.clark@aiinnovations.ai', '+1-555-0506', 'Head of Product', 6, false, true, NOW(), false),
    
    -- Contacts for healthcare companies
    (12, 'Nancy', 'Lewis', 'nancy.lewis@medcare.com', '+1-555-0507', 'Medical Director', 7, true, true, NOW(), false),
    (13, 'Steven', 'Walker', 'steven.walker@medcare.com', '+1-555-0508', 'IT Director', 7, false, true, NOW(), false),
    
    (14, 'Rebecca', 'Hall', 'rebecca.hall@biotechresearch.com', '+1-555-0509', 'CEO', 8, true, true, NOW(), false),
    (15, 'Brian', 'Young', 'brian.young@biotechresearch.com', '+1-555-0510', 'CTO', 8, false, true, NOW(), false),
    (16, 'Kimberly', 'King', 'kimberly.king@biotechresearch.com', '+1-555-0511', 'VP Operations', 8, false, true, NOW(), false),
    
    -- Contacts for finance companies
    (17, 'Gregory', 'Wright', 'gregory.wright@securefinance.com', '+1-555-0512', 'Managing Director', 9, true, true, NOW(), false),
    (18, 'Laura', 'Scott', 'laura.scott@securefinance.com', '+1-555-0513', 'CTO', 9, false, true, NOW(), false),
    
    (19, 'Jeffrey', 'Green', 'jeffrey.green@fintechpioneers.io', '+1-555-0514', 'Co-Founder', 10, true, true, NOW(), false),
    
    -- Contacts for manufacturing companies
    (20, 'Ronald', 'Adams', 'ronald.adams@precisionmfg.com', '+1-555-0515', 'Operations Manager', 11, true, true, NOW(), false),
    (21, 'Sandra', 'Nelson', 'sandra.nelson@precisionmfg.com', '+1-555-0516', 'Quality Director', 11, false, true, NOW(), false),
    
    (22, 'Kenneth', 'Hill', 'kenneth.hill@autopartsexpress.com', '+1-555-0517', 'CEO', 12, true, true, NOW(), false),
    (23, 'Sharon', 'Moore', 'sharon.moore@autopartsexpress.com', '+1-555-0518', 'VP Sales', 12, false, true, NOW(), false),
    (24, 'Dennis', 'Jackson', 'dennis.jackson@autopartsexpress.com', '+1-555-0519', 'IT Manager', 12, false, true, NOW(), false),
    
    -- Contacts for retail companies
    (25, 'Carol', 'Martin', 'carol.martin@urbanretail.com', '+1-555-0520', 'Director of Operations', 13, true, true, NOW(), false),
    
    (26, 'Frank', 'Thompson', 'frank.thompson@ecommercemasters.com', '+1-555-0521', 'Founder', 14, true, true, NOW(), false),
    
    -- Contacts for international companies
    (27, 'Hans', 'Mueller', 'hans.mueller@eurotech.de', '+49-30-555-0201', 'Geschäftsführer', 18, true, true, NOW(), false),
    (28, 'Anna', 'Schmidt', 'anna.schmidt@eurotech.de', '+49-30-555-0202', 'Technical Lead', 18, false, true, NOW(), false),
    
    (29, 'Wei', 'Chen', 'wei.chen@apac-solutions.sg', '+65-6555-0201', 'Managing Director', 19, true, true, NOW(), false),
    (30, 'Priya', 'Sharma', 'priya.sharma@apac-solutions.sg', '+65-6555-0202', 'VP Engineering', 19, false, true, NOW(), false)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"Contacts_Id_seq"', (SELECT MAX("Id") FROM "Contacts"));

-- ============================================================================
-- SECTION 4: ADDITIONAL PRODUCTS
-- ============================================================================

INSERT INTO "Products" ("Id", "Name", "SKU", "Description", "Price", "Cost", "Category",
    "ProductType", "BillingFrequency", "IsActive", "IsTaxable", "CreatedAt", "IsDeleted")
VALUES 
    -- More subscription tiers
    (7, 'CRM Cloud - Team', 'CRM-CLOUD-TEAM', 
     'Team subscription for up to 10 users. Includes collaboration features.',
     499.00, 75.00, 'Subscription', 1, 'Monthly', true, true, NOW(), false),
    
    (8, 'CRM Cloud - Enterprise', 'CRM-CLOUD-ENT', 
     'Enterprise subscription with unlimited users and advanced security.',
     1999.00, 300.00, 'Subscription', 1, 'Monthly', true, true, NOW(), false),
    
    -- Add-on products
    (9, 'Email Integration Add-on', 'ADDON-EMAIL', 
     'Email integration with Gmail and Outlook. Automatic sync and tracking.',
     29.00, 5.00, 'Subscription', 1, 'Monthly', true, true, NOW(), false),
    
    (10, 'Analytics Dashboard Add-on', 'ADDON-ANALYTICS', 
     'Advanced analytics and custom reporting dashboards.',
     49.00, 8.00, 'Subscription', 1, 'Monthly', true, true, NOW(), false),
    
    (11, 'Marketing Automation Add-on', 'ADDON-MARKETING', 
     'Marketing automation with email campaigns, landing pages, and lead scoring.',
     99.00, 15.00, 'Subscription', 1, 'Monthly', true, true, NOW(), false),
    
    -- Training products
    (12, 'Basic Training Package', 'TRN-BASIC', 
     '2-hour virtual training session covering CRM basics.',
     500.00, 200.00, 'Training', 0, NULL, true, false, NOW(), false),
    
    (13, 'Advanced Training Package', 'TRN-ADV', 
     'Full-day on-site training with hands-on exercises and certification.',
     2000.00, 800.00, 'Training', 0, NULL, true, false, NOW(), false),
    
    -- Consulting services
    (14, 'Custom Development', 'SVC-CUSTOM', 
     'Custom development services. Billed per hour.',
     175.00, 85.00, 'Consulting', 0, NULL, true, false, NOW(), false),
    
    (15, 'Data Migration Service', 'SVC-MIGRATE', 
     'Professional data migration from existing CRM systems.',
     3500.00, 1500.00, 'Services', 0, NULL, true, false, NOW(), false),
    
    -- Hardware (if applicable)
    (16, 'Dedicated Server Hosting', 'HW-SERVER', 
     'Dedicated server hosting for on-premise deployment.',
     999.00, 400.00, 'Hardware', 1, 'Monthly', true, true, NOW(), false)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"Products_Id_seq"', (SELECT MAX("Id") FROM "Products"));

-- ============================================================================
-- SECTION 5: ADDITIONAL LEADS (Various stages and sources)
-- ============================================================================

INSERT INTO "Leads" ("Id", "FirstName", "LastName", "Email", "Phone", "Company",
    "Status", "Source", "Score", "Title", "Address", "City", "State", "PostalCode", "Country",
    "CreatedAt", "IsDeleted", "OwnerId")
VALUES 
    -- New leads
    (4, 'William', 'Jackson', 'william.jackson@newtech.com', '+1-555-0601',
     'NewTech Ventures', 0, 3, 35, 'Co-Founder',
     '200 Startup Lane', 'San Jose', 'CA', '95102', 'United States',
     NOW() - INTERVAL '2 days', false, 9),
    
    (5, 'Elizabeth', 'Martin', 'elizabeth.martin@futuresoft.io', '+1-555-0602',
     'Future Software Inc', 0, 4, 42, 'Product Manager',
     '300 Innovation Drive', 'Austin', 'TX', '78703', 'United States',
     NOW() - INTERVAL '1 day', false, 10),
    
    -- Contacted leads
    (6, 'Thomas', 'Thompson', 'thomas.thompson@datacorp.com', '+1-555-0603',
     'DataCorp Industries', 1, 0, 55, 'VP Technology',
     '400 Data Center Way', 'Dallas', 'TX', '75202', 'United States',
     NOW() - INTERVAL '10 days', false, 3),
    
    (7, 'Margaret', 'Garcia', 'margaret.garcia@healthtech.org', '+1-555-0604',
     'HealthTech Solutions', 1, 1, 48, 'Director of Operations',
     '500 Medical Plaza', 'Houston', 'TX', '77002', 'United States',
     NOW() - INTERVAL '8 days', false, 9),
    
    -- Working leads
    (8, 'Christopher', 'Martinez', 'christopher.martinez@finservices.com', '+1-555-0605',
     'Financial Services Group', 2, 2, 68, 'CTO',
     '600 Finance Tower', 'Chicago', 'IL', '60602', 'United States',
     NOW() - INTERVAL '15 days', false, 2),
    
    (9, 'Dorothy', 'Robinson', 'dorothy.robinson@retailplus.com', '+1-555-0606',
     'Retail Plus Corp', 2, 5, 72, 'VP Digital',
     '700 Retail Avenue', 'New York', 'NY', '10006', 'United States',
     NOW() - INTERVAL '20 days', false, 3),
    
    -- Nurturing leads
    (10, 'Daniel', 'Clark', 'daniel.clark@mfgtech.com', '+1-555-0607',
     'Manufacturing Technologies', 3, 4, 60, 'Operations Director',
     '800 Industrial Park', 'Detroit', 'MI', '48202', 'United States',
     NOW() - INTERVAL '30 days', false, 9),
    
    -- Qualified leads
    (11, 'Nancy', 'Lewis', 'nancy.lewis@cloudservices.io', '+1-555-0608',
     'Cloud Services Ltd', 4, 0, 88, 'CEO',
     '900 Cloud Way', 'Seattle', 'WA', '98103', 'United States',
     NOW() - INTERVAL '25 days', false, 2),
    
    (12, 'Paul', 'Walker', 'paul.walker@enterprise.net', '+1-555-0609',
     'Enterprise Networks Inc', 4, 1, 85, 'CIO',
     '1000 Enterprise Boulevard', 'San Francisco', 'CA', '94103', 'United States',
     NOW() - INTERVAL '21 days', false, 3),
    
    -- Converted leads
    (13, 'Karen', 'Hall', 'karen.hall@converted.com', '+1-555-0610',
     'Now a Customer Inc', 6, 2, 95, 'CEO',
     '1100 Success Street', 'Boston', 'MA', '02103', 'United States',
     NOW() - INTERVAL '45 days', false, 2),
    
    -- Lost leads
    (14, 'Steven', 'Allen', 'steven.allen@lostdeal.com', '+1-555-0611',
     'Lost Opportunity Corp', 7, 3, 50, 'VP',
     '1200 Missed Lane', 'Denver', 'CO', '80202', 'United States',
     NOW() - INTERVAL '60 days', false, 9),
    
    -- International leads
    (15, 'Pierre', 'Dubois', 'pierre.dubois@eurotech.fr', '+33-1-5550-0101',
     'Euro Technologies SARL', 2, 4, 70, 'Directeur Technique',
     '100 Avenue des Champs', 'Paris', 'IDF', '75008', 'France',
     NOW() - INTERVAL '12 days', false, 2)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"Leads_Id_seq"', (SELECT MAX("Id") FROM "Leads"));

-- ============================================================================
-- SECTION 6: ADDITIONAL OPPORTUNITIES (All stages)
-- ============================================================================

INSERT INTO "Opportunities" ("Id", "Name", "Stage", "Probability", "Amount", "Currency",
    "ExpectedCloseDate", "CustomerId", "CreatedAt", "IsDeleted", "OwnerId", "Description")
VALUES 
    -- Prospecting
    (4, 'CloudTech - Initial Assessment', 0, 10, 35000.00, 'USD',
     NOW() + INTERVAL '120 days', 4, NOW() - INTERVAL '5 days', false, 3, 
     'Initial assessment of CRM needs'),
    
    (5, 'DataStream - Cloud Evaluation', 0, 10, 18000.00, 'USD',
     NOW() + INTERVAL '90 days', 5, NOW() - INTERVAL '3 days', false, 9,
     'Evaluating cloud CRM options'),
    
    -- Qualification
    (6, 'AI Innovations - Enterprise Solution', 1, 25, 250000.00, 'USD',
     NOW() + INTERVAL '75 days', 6, NOW() - INTERVAL '20 days', false, 2,
     'Enterprise CRM with custom AI integrations'),
    
    (7, 'MedCare - Healthcare CRM', 1, 20, 85000.00, 'USD',
     NOW() + INTERVAL '80 days', 7, NOW() - INTERVAL '15 days', false, 3,
     'HIPAA-compliant CRM solution'),
    
    -- Needs Analysis
    (8, 'BioTech Research - Lab Integration', 2, 40, 175000.00, 'USD',
     NOW() + INTERVAL '60 days', 8, NOW() - INTERVAL '35 days', false, 2,
     'CRM integration with lab management systems'),
    
    (9, 'SecureFinance - Compliance Package', 2, 45, 120000.00, 'USD',
     NOW() + INTERVAL '45 days', 9, NOW() - INTERVAL '40 days', false, 3,
     'Financial services compliance add-ons'),
    
    -- Proposal
    (10, 'FinTech Pioneers - Startup Bundle', 3, 60, 45000.00, 'USD',
     NOW() + INTERVAL '30 days', 10, NOW() - INTERVAL '50 days', false, 10,
     'Startup bundle with growth options'),
    
    (11, 'Precision Manufacturing - Shop Floor CRM', 3, 55, 95000.00, 'USD',
     NOW() + INTERVAL '35 days', 11, NOW() - INTERVAL '45 days', false, 9,
     'CRM with manufacturing integration'),
    
    -- Negotiation
    (12, 'AutoParts Express - Enterprise Deal', 4, 75, 280000.00, 'USD',
     NOW() + INTERVAL '15 days', 12, NOW() - INTERVAL '60 days', false, 2,
     'Full enterprise deployment with training'),
    
    (13, 'Urban Retail - Omnichannel CRM', 4, 80, 65000.00, 'USD',
     NOW() + INTERVAL '20 days', 13, NOW() - INTERVAL '55 days', false, 3,
     'Omnichannel retail CRM solution'),
    
    -- Closed Won (historical)
    (14, 'Euro Tech Partners - Success Story', 5, 100, 120000.00, 'EUR',
     NOW() - INTERVAL '10 days', 18, NOW() - INTERVAL '90 days', false, 2,
     'Successfully closed European deal'),
    
    (15, 'Asia Pacific Solutions - APAC Expansion', 5, 100, 180000.00, 'USD',
     NOW() - INTERVAL '30 days', 19, NOW() - INTERVAL '120 days', false, 2,
     'Major APAC customer win'),
    
    -- Closed Lost (historical)
    (16, 'E-Commerce Masters - Lost to Competitor', 6, 0, 55000.00, 'USD',
     NOW() - INTERVAL '15 days', 14, NOW() - INTERVAL '75 days', false, 10,
     'Lost to competitor - price sensitivity'),
    
    (17, 'UK Enterprises - Budget Constraints', 6, 0, 90000.00, 'GBP',
     NOW() - INTERVAL '20 days', 20, NOW() - INTERVAL '80 days', false, 3,
     'Deal postponed due to budget cuts')
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"Opportunities_Id_seq"', (SELECT MAX("Id") FROM "Opportunities"));

-- ============================================================================
-- SECTION 7: ADDITIONAL CAMPAIGNS
-- ============================================================================

INSERT INTO "Campaigns" ("Id", "Name", "Type", "Status", "Description",
    "StartDate", "EndDate", "Budget", "ActualCost", "ExpectedRevenue", "ExpectedLeads",
    "CreatedAt", "IsDeleted", "OwnerId")
VALUES 
    -- Planned campaigns
    (4, 'Summer Product Launch', 1, 0, 
     'Summer product launch event with demos and special offers.',
     NOW() + INTERVAL '45 days', NOW() + INTERVAL '47 days', 25000.00, NULL, 100000.00, 200,
     NOW(), false, 5),
    
    (5, 'Partner Summit 2026', 1, 0, 
     'Annual partner summit for channel partners and resellers.',
     NOW() + INTERVAL '90 days', NOW() + INTERVAL '92 days', 75000.00, NULL, 500000.00, 50,
     NOW(), false, 5),
    
    -- Active campaigns
    (6, 'LinkedIn Lead Generation', 3, 1, 
     'Targeted LinkedIn advertising campaign for B2B leads.',
     NOW() - INTERVAL '14 days', NOW() + INTERVAL '45 days', 10000.00, 4500.00, 50000.00, 150,
     NOW() - INTERVAL '14 days', false, 6),
    
    (7, 'Content Marketing Program', 5, 1, 
     'Blog posts, whitepapers, and case studies to drive organic traffic.',
     NOW() - INTERVAL '30 days', NOW() + INTERVAL '60 days', 8000.00, 3200.00, 30000.00, 100,
     NOW() - INTERVAL '30 days', false, 6),
    
    (8, 'Email Drip Campaign - Trial Users', 0, 1, 
     'Automated email sequence for trial users to drive conversions.',
     NOW() - INTERVAL '21 days', NOW() + INTERVAL '90 days', 2000.00, 500.00, 25000.00, 80,
     NOW() - INTERVAL '21 days', false, 6),
    
    -- Paused campaigns
    (9, 'Trade Show Circuit', 1, 2, 
     'Regional trade show appearances. Currently paused for budget review.',
     NOW() - INTERVAL '60 days', NOW() + INTERVAL '120 days', 50000.00, 15000.00, 200000.00, 300,
     NOW() - INTERVAL '60 days', false, 5),
    
    -- Completed campaigns
    (10, 'Q4 2025 Holiday Promotion', 0, 3, 
     'Holiday season discount promotion with email and social.',
     NOW() - INTERVAL '90 days', NOW() - INTERVAL '60 days', 15000.00, 14500.00, 75000.00, 250,
     NOW() - INTERVAL '90 days', false, 5),
    
    (11, 'CRM Webinar Series - Fall 2025', 2, 3, 
     'Fall webinar series covering CRM best practices.',
     NOW() - INTERVAL '120 days', NOW() - INTERVAL '90 days', 5000.00, 4800.00, 45000.00, 180,
     NOW() - INTERVAL '120 days', false, 6),
    
    -- Cancelled campaign
    (12, 'Cancelled: Industry Conference', 1, 4, 
     'Conference sponsorship cancelled due to event postponement.',
     NOW() - INTERVAL '30 days', NOW() + INTERVAL '30 days', 30000.00, 5000.00, 100000.00, 100,
     NOW() - INTERVAL '45 days', false, 5)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"Campaigns_Id_seq"', (SELECT MAX("Id") FROM "Campaigns"));

-- ============================================================================
-- SECTION 8: ADDITIONAL ACCOUNTS (Billing records)
-- ============================================================================

INSERT INTO "Accounts" ("Id", "AccountNumber", "CustomerId", "ProductId",
    "Status", "BillingFrequency", "MRR", "ARR", "Currency",
    "ContractStartDate", "ContractEndDate", "CreatedAt", "IsDeleted")
VALUES 
    -- Active accounts
    (3, 'ACC-2024-003', 4, 7, 0, 'Monthly', 499.00, 5988.00, 'USD',
     NOW() - INTERVAL '60 days', NOW() + INTERVAL '10 months', NOW() - INTERVAL '60 days', false),
    
    (4, 'ACC-2024-004', 6, 8, 0, 'Monthly', 1999.00, 23988.00, 'USD',
     NOW() - INTERVAL '120 days', NOW() + INTERVAL '8 months', NOW() - INTERVAL '120 days', false),
    
    (5, 'ACC-2024-005', 8, 8, 0, 'Yearly', 2165.75, 25989.00, 'USD',
     NOW() - INTERVAL '180 days', NOW() + INTERVAL '6 months', NOW() - INTERVAL '180 days', false),
    
    (6, 'ACC-2024-006', 12, 8, 0, 'Yearly', 2832.42, 33989.00, 'USD',
     NOW() - INTERVAL '200 days', NOW() + INTERVAL '5 months', NOW() - INTERVAL '200 days', false),
    
    (7, 'ACC-2025-001', 18, 7, 0, 'Monthly', 499.00, 5988.00, 'EUR',
     NOW() - INTERVAL '90 days', NOW() + INTERVAL '9 months', NOW() - INTERVAL '90 days', false),
    
    (8, 'ACC-2025-002', 19, 8, 0, 'Yearly', 2082.42, 24989.00, 'USD',
     NOW() - INTERVAL '100 days', NOW() + INTERVAL '8 months', NOW() - INTERVAL '100 days', false)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"Accounts_Id_seq"', (SELECT MAX("Id") FROM "Accounts"));

-- ============================================================================
-- SECTION 9: ADDITIONAL TASKS
-- ============================================================================

INSERT INTO "Tasks" ("Id", "Title", "Description", "DueDate", "Priority", "Status",
    "TaskType", "CustomerId", "OwnerId", "CreatedAt", "IsDeleted")
VALUES 
    -- Various priorities
    (4, 'Critical: Contract renewal discussion', 
     'Discuss contract renewal terms with AutoParts Express before expiration.',
     NOW() + INTERVAL '5 days', 3, 1, 2, 12, 2, NOW(), false),
    
    (5, 'High: Demo preparation for AI Innovations', 
     'Prepare custom demo showcasing AI integration capabilities.',
     NOW() + INTERVAL '10 days', 2, 0, 3, 6, 2, NOW(), false),
    
    (6, 'Medium: Follow up with MedCare', 
     'Follow up on HIPAA compliance questions.',
     NOW() + INTERVAL '7 days', 1, 0, 0, 7, 3, NOW(), false),
    
    (7, 'Low: Send thank you note', 
     'Send thank you note to Euro Tech for successful implementation.',
     NOW() + INTERVAL '14 days', 0, 0, 1, 18, 2, NOW(), false),
    
    -- Various statuses
    (8, 'Quarterly business review - BioTech', 
     'Conduct quarterly business review and discuss expansion opportunities.',
     NOW() + INTERVAL '21 days', 2, 0, 2, 8, 2, NOW(), false),
    
    (9, 'Training session preparation', 
     'Prepare materials for upcoming training session.',
     NOW() + INTERVAL '3 days', 1, 1, 4, NULL, 7, NOW(), false),
    
    (10, 'Update CRM documentation', 
     'Update internal documentation with new features.',
     NOW() - INTERVAL '2 days', 0, 2, 4, NULL, 4, NOW(), false),
    
    -- Overdue tasks
    (11, 'OVERDUE: Send proposal to SecureFinance', 
     'Proposal was due 3 days ago. Needs immediate attention.',
     NOW() - INTERVAL '3 days', 3, 1, 1, 9, 3, NOW() - INTERVAL '10 days', false),
    
    -- Completed tasks
    (12, 'Completed: Initial call with CloudTech', 
     'Initial discovery call completed successfully.',
     NOW() - INTERVAL '5 days', 1, 2, 0, 4, 3, NOW() - INTERVAL '10 days', false),
    
    (13, 'Completed: Send case study', 
     'Sent healthcare case study to MedCare team.',
     NOW() - INTERVAL '3 days', 0, 2, 1, 7, 3, NOW() - INTERVAL '5 days', false)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"Tasks_Id_seq"', (SELECT MAX("Id") FROM "Tasks"));

-- ============================================================================
-- SECTION 10: ADDITIONAL SERVICE REQUESTS
-- ============================================================================

INSERT INTO "ServiceRequests" ("Id", "Subject", "Description", "Priority", "Status",
    "Type", "CustomerId", "AssignedToId", "CreatedAt", "IsDeleted")
VALUES 
    -- Various priorities and statuses
    (4, 'Critical: System down - AI Innovations', 
     'Production system experiencing outages. Multiple users affected.',
     3, 2, 'INCIDENT', 6, 7, NOW() - INTERVAL '2 hours', false),
    
    (5, 'Integration not syncing data', 
     'Email integration stopped syncing after recent update.',
     2, 2, 'SUPPORT', 4, 4, NOW() - INTERVAL '1 day', false),
    
    (6, 'Request for API documentation', 
     'Need updated API documentation for custom integration project.',
     0, 1, 'QUESTION', 8, 8, NOW() - INTERVAL '3 days', false),
    
    (7, 'Bug: Dashboard showing incorrect totals', 
     'The opportunity totals on the dashboard are not matching the detailed view.',
     1, 2, 'BUG', 12, 4, NOW() - INTERVAL '2 days', false),
    
    (8, 'Change request: Custom field addition', 
     'Request to add custom fields for healthcare compliance tracking.',
     1, 0, 'CHANGE', 7, NULL, NOW() - INTERVAL '5 days', false),
    
    (9, 'Access request for new team member', 
     'New sales rep needs CRM access with Sales Representative permissions.',
     0, 3, 'ACCESS', 3, 4, NOW() - INTERVAL '4 hours', false),
    
    (10, 'Feature request: Mobile app offline mode', 
     'Would like to have offline capability in the mobile app for field sales.',
     0, 0, 'FEATURE', 11, NULL, NOW() - INTERVAL '1 week', false),
    
    -- Resolved and closed
    (11, 'Resolved: Password reset issue', 
     'User was able to reset password after cache clear.',
     1, 5, 'SUPPORT', 2, 4, NOW() - INTERVAL '2 days', false),
    
    (12, 'Closed: Billing clarification', 
     'Explained prorated billing for mid-cycle subscription changes.',
     0, 6, 'BILLING', 3, 7, NOW() - INTERVAL '5 days', false),
    
    -- Escalated
    (13, 'ESCALATED: Data export not working', 
     'Large data export failing consistently. Escalated to engineering.',
     2, 4, 'SUPPORT', 19, 7, NOW() - INTERVAL '3 days', false)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"ServiceRequests_Id_seq"', (SELECT MAX("Id") FROM "ServiceRequests"));

-- ============================================================================
-- SECTION 11: ACTIVITY/NOTES (If applicable)
-- ============================================================================

-- Note: If your schema has an Activities or Notes table, add sample data here

-- ============================================================================
-- VERIFICATION
-- ============================================================================

DO $$
BEGIN
    RAISE NOTICE 'Demo data verification:';
    RAISE NOTICE '  - Users: % rows', (SELECT COUNT(*) FROM "Users");
    RAISE NOTICE '  - Customers: % rows', (SELECT COUNT(*) FROM "Customers");
    RAISE NOTICE '  - Contacts: % rows', (SELECT COUNT(*) FROM "Contacts");
    RAISE NOTICE '  - Products: % rows', (SELECT COUNT(*) FROM "Products");
    RAISE NOTICE '  - Leads: % rows', (SELECT COUNT(*) FROM "Leads");
    RAISE NOTICE '  - Opportunities: % rows', (SELECT COUNT(*) FROM "Opportunities");
    RAISE NOTICE '  - Campaigns: % rows', (SELECT COUNT(*) FROM "Campaigns");
    RAISE NOTICE '  - Accounts: % rows', (SELECT COUNT(*) FROM "Accounts");
    RAISE NOTICE '  - Tasks: % rows', (SELECT COUNT(*) FROM "Tasks");
    RAISE NOTICE '  - ServiceRequests: % rows', (SELECT COUNT(*) FROM "ServiceRequests");
    RAISE NOTICE '';
    RAISE NOTICE 'Demo data loading complete!';
    RAISE NOTICE 'The system now has comprehensive test data for demonstration.';
END $$;
