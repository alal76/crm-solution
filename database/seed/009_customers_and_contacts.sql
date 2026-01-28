-- =====================================================
-- 009_customers_and_contacts.sql
-- Seed data for Customers (Accounts) and CustomerContacts (linking table)
-- Created: 2026-01-28
-- =====================================================

-- =====================================================
-- CONTACTS
-- These are individual people who can be linked to accounts
-- =====================================================

INSERT INTO Contacts (
    Id, FirstName, LastName, Email, Phone, MobilePhone, JobTitle, Department,
    Address, City, State, ZipCode, Country, Company, LeadSource, Status,
    Notes, CreatedAt, IsDeleted
) VALUES 
-- Core contacts (already exist as 1-3, adding 4-7)
(4, 'Emily', 'Chen', 'emily.chen@email.com', '555-0204', '555-0214', 'CTO', 'Technology', 
 '456 Innovation Dr', 'San Francisco', 'California', '94102', 'USA', 'TechFlow Solutions', 'Referral', 0,
 'Technology leader with 15+ years experience', NOW(), 0),
 
(5, 'David', 'Anderson', 'david.anderson@email.com', '555-0205', '555-0215', 'VP Operations', 'Operations',
 '789 Business Park', 'Chicago', 'Illinois', '60601', 'USA', 'Acme Industries', 'Trade Show', 0,
 'Operations specialist focused on manufacturing efficiency', NOW(), 0),
 
(6, 'Lisa', 'Thompson', 'lisa.thompson@email.com', '555-0206', '555-0216', 'CFO', 'Finance',
 '321 Financial Way', 'Denver', 'Colorado', '80202', 'USA', 'Summit Financial Advisors', 'Website', 0,
 'Financial executive with private equity background', NOW(), 0),
 
(7, 'Abhishek', 'Lal', 'abhishek.lal@email.com', '555-0207', '555-0217', 'Director of Sales', 'Sales',
 '555 Commerce Blvd', 'Austin', 'Texas', '78701', 'USA', 'Creative Media Studios', 'LinkedIn', 0,
 'Sales leader with media industry expertise', NOW(), 0)
ON DUPLICATE KEY UPDATE 
    FirstName = VALUES(FirstName),
    LastName = VALUES(LastName),
    Email = VALUES(Email);

-- =====================================================
-- CUSTOMERS (ACCOUNTS)
-- Mix of organizations across various industries
-- =====================================================

INSERT INTO Customers (
    Id, Category, FirstName, LastName, Company, Email, Phone, Address, City, State, ZipCode, Country,
    Industry, AnnualRevenue, CustomerType, Priority, LifecycleStage, Notes,
    TotalPurchases, AccountBalance, CreditLimit, LeadScore, CustomerHealthScore, NpsScore, SatisfactionRating,
    OptInEmail, OptInSms, OptInPhone, CreatedAt, IsDeleted
) VALUES 
-- Customer 1: Tech Corp (already exists with linked contacts)
(1, 1, '', '', 'Tech Corp', 'info@techcorp.com', '555-0001', '100 Tech Plaza', 'San Jose', 'California', '95101', 'USA',
 'Technology', 10000000.00, 1, 2, 2, 'Enterprise technology company - flagship account',
 250000.00, 0.00, 100000.00, 85, 90, 9, 4.5,
 1, 0, 1, NOW(), 0),

-- Customer 2: Innovation Inc (existing, no linked contacts yet)
(2, 1, '', '', 'Innovation Inc', 'contact@innovation-inc.com', '555-0002', '200 Startup Way', 'Austin', 'Texas', '78701', 'USA',
 'Technology', 2500000.00, 0, 1, 1, 'Growing startup with high potential',
 50000.00, 0.00, 25000.00, 70, 75, 8, 4.0,
 1, 1, 1, NOW(), 0),

-- Customer 3: Acme Industries
(3, 1, '', '', 'Acme Industries', 'info@acme-ind.com', '555-0101', '100 Industrial Way', 'Chicago', 'Illinois', '60601', 'USA',
 'Manufacturing', 5000000.00, 1, 1, 2, 'Major manufacturing client with long-term contract',
 150000.00, 0.00, 75000.00, 80, 85, 8, 4.2,
 1, 0, 1, NOW(), 0),

-- Customer 4: TechFlow Solutions
(4, 1, '', '', 'TechFlow Solutions', 'contact@techflow.io', '555-0102', '200 Tech Park', 'San Francisco', 'California', '94102', 'USA',
 'Technology', 2500000.00, 0, 1, 1, 'SaaS company specializing in workflow automation',
 45000.00, 0.00, 30000.00, 75, 70, 7, 3.8,
 1, 1, 1, NOW(), 0),

-- Customer 5: Green Earth Consulting
(5, 1, '', '', 'Green Earth Consulting', 'hello@greenearth.com', '555-0103', '75 Sustainability Blvd', 'Portland', 'Oregon', '97201', 'USA',
 'Consulting', 1200000.00, 0, 1, 1, 'Environmental consulting firm focused on sustainability',
 25000.00, 0.00, 15000.00, 65, 80, 9, 4.5,
 1, 0, 0, NOW(), 0),

-- Customer 6: Midwest Healthcare Group
(6, 1, '', '', 'Midwest Healthcare Group', 'admin@midwesthc.org', '555-0104', '400 Medical Center Dr', 'Minneapolis', 'Minnesota', '55401', 'USA',
 'Healthcare', 8500000.00, 1, 2, 2, 'Regional healthcare network with multiple facilities',
 320000.00, 0.00, 150000.00, 90, 88, 8, 4.3,
 1, 1, 1, NOW(), 0),

-- Customer 7: Pacific Retail Partners
(7, 1, '', '', 'Pacific Retail Partners', 'sales@pacificretail.com', '555-0105', '888 Commerce St', 'Seattle', 'Washington', '98101', 'USA',
 'Retail', 3200000.00, 1, 1, 2, 'Multi-brand retail chain across Pacific Northwest',
 95000.00, 0.00, 50000.00, 72, 78, 7, 3.9,
 1, 1, 0, NOW(), 0),

-- Customer 8: Summit Financial Advisors
(8, 1, '', '', 'Summit Financial Advisors', 'info@summitfa.com', '555-0106', '1 Financial Plaza', 'Denver', 'Colorado', '80202', 'USA',
 'Financial Services', 4100000.00, 1, 2, 2, 'Wealth management and financial planning firm',
 180000.00, 0.00, 100000.00, 88, 92, 9, 4.7,
 1, 0, 1, NOW(), 0),

-- Customer 9: Atlantic Shipping Co
(9, 1, '', '', 'Atlantic Shipping Co', 'ops@atlanticship.com', '555-0107', '500 Harbor View', 'Boston', 'Massachusetts', '02101', 'USA',
 'Transportation', 12000000.00, 1, 2, 2, 'International shipping and logistics company',
 450000.00, 0.00, 200000.00, 85, 82, 8, 4.1,
 1, 1, 1, NOW(), 0),

-- Customer 10: Desert Sun Energy
(10, 1, '', '', 'Desert Sun Energy', 'contact@desertsun.energy', '555-0108', '1200 Solar Ave', 'Phoenix', 'Arizona', '85001', 'USA',
 'Energy', 6700000.00, 1, 1, 2, 'Solar energy provider for commercial installations',
 210000.00, 0.00, 100000.00, 78, 85, 8, 4.2,
 1, 0, 0, NOW(), 0),

-- Customer 11: Creative Media Studios
(11, 1, '', '', 'Creative Media Studios', 'hello@creativemedia.co', '555-0109', '42 Art District', 'Austin', 'Texas', '78701', 'USA',
 'Media & Entertainment', 900000.00, 0, 1, 1, 'Digital media production and marketing agency',
 35000.00, 0.00, 20000.00, 60, 75, 8, 4.0,
 1, 1, 1, NOW(), 0),

-- Customer 12: Precision Manufacturing Inc
(12, 1, '', '', 'Precision Manufacturing Inc', 'info@precisionmfg.com', '555-0110', '777 Factory Ln', 'Detroit', 'Michigan', '48201', 'USA',
 'Manufacturing', 15000000.00, 1, 3, 2, 'Precision engineering and manufacturing for aerospace industry',
 580000.00, 0.00, 250000.00, 95, 90, 9, 4.6,
 1, 0, 1, NOW(), 0)

ON DUPLICATE KEY UPDATE 
    Company = VALUES(Company),
    Email = VALUES(Email),
    Industry = VALUES(Industry),
    AnnualRevenue = VALUES(AnnualRevenue);

-- =====================================================
-- CUSTOMER CONTACTS (Many-to-Many Linking Table)
-- Links contacts to customer accounts with roles
-- =====================================================

-- Clear existing customer contacts to avoid duplicates
DELETE FROM CustomerContacts WHERE CustomerId IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12);

INSERT INTO CustomerContacts (
    CustomerId, ContactId, Role, IsPrimaryContact, IsDecisionMaker,
    ReceivesBillingNotifications, ReceivesMarketingEmails, ReceivesTechnicalUpdates,
    PositionAtCustomer, DepartmentAtCustomer, RelationshipStartDate, Notes, CreatedAt, IsDeleted
) VALUES 
-- Customer 1: Tech Corp (3 linked contacts - flagship account)
(1, 1, 0, 1, 1, 1, 1, 0, 'VP of Engineering', 'Engineering', NOW(), 'Primary technical contact', NOW(), 0),
(1, 2, 1, 0, 0, 0, 1, 0, 'Account Manager', 'Sales', NOW(), 'Day-to-day operations contact', NOW(), 0),
(1, 3, 2, 0, 1, 0, 1, 1, 'Technical Lead', 'IT', NOW(), 'Technical decision maker', NOW(), 0),

-- Customer 3: Acme Industries (1 linked contact)
(3, 4, 0, 1, 1, 1, 1, 0, 'VP Operations', 'Operations', NOW(), 'Main point of contact for all manufacturing projects', NOW(), 0),

-- Customer 4: TechFlow Solutions (2 linked contacts)
(4, 5, 0, 1, 1, 1, 1, 1, 'CTO', 'Technology', NOW(), 'Technical lead and key decision maker', NOW(), 0),
(4, 6, 1, 0, 0, 0, 1, 0, 'Business Analyst', 'Product', NOW(), 'Product requirements liaison', NOW(), 0),

-- Customer 5: Green Earth Consulting (1 linked contact)
(5, 7, 0, 1, 1, 1, 1, 1, 'Managing Partner', 'Leadership', NOW(), 'Senior partner handling key accounts', NOW(), 0),

-- Customer 6: Midwest Healthcare Group (2 linked contacts)
(6, 4, 0, 1, 0, 1, 1, 0, 'Operations Director', 'Operations', NOW(), 'Manages vendor relationships', NOW(), 0),
(6, 5, 1, 0, 1, 0, 1, 1, 'IT Director', 'Information Technology', NOW(), 'Technical decision maker for healthcare IT', NOW(), 0),

-- Customer 7: Pacific Retail Partners (1 linked contact)
(7, 6, 0, 1, 1, 1, 1, 0, 'VP of Finance', 'Finance', NOW(), 'Budget authority for retail operations', NOW(), 0),

-- Customer 8: Summit Financial Advisors (2 linked contacts)
(8, 4, 0, 1, 0, 1, 1, 0, 'Client Services Manager', 'Client Services', NOW(), 'Day-to-day relationship manager', NOW(), 0),
(8, 7, 1, 0, 1, 0, 0, 1, 'Technology Advisor', 'Advisory', NOW(), 'Advises on fintech solutions', NOW(), 0),

-- Customer 9: Atlantic Shipping Co (1 linked contact)
(9, 5, 0, 1, 1, 1, 1, 1, 'Logistics Director', 'Logistics', NOW(), 'Oversees all shipping operations', NOW(), 0),

-- Customer 10: Desert Sun Energy (2 linked contacts)
(10, 6, 0, 1, 1, 1, 1, 0, 'Project Manager', 'Projects', NOW(), 'Manages solar installation projects', NOW(), 0),
(10, 7, 1, 0, 0, 0, 1, 1, 'Technical Consultant', 'Engineering', NOW(), 'Solar technology specialist', NOW(), 0),

-- Customer 11: Creative Media Studios (1 linked contact)
(11, 4, 0, 1, 1, 1, 1, 0, 'Creative Director', 'Creative', NOW(), 'Lead creative and business contact', NOW(), 0),

-- Customer 12: Precision Manufacturing Inc (4 linked contacts - major account)
(12, 4, 0, 1, 0, 1, 1, 0, 'Quality Manager', 'Quality Assurance', NOW(), 'Primary quality contact', NOW(), 0),
(12, 5, 1, 0, 1, 0, 1, 1, 'Engineering Director', 'Engineering', NOW(), 'Technical decision maker', NOW(), 0),
(12, 6, 2, 0, 0, 1, 0, 0, 'Finance Manager', 'Finance', NOW(), 'Billing and contracts contact', NOW(), 0),
(12, 7, 3, 0, 1, 0, 1, 1, 'Plant Manager', 'Manufacturing', NOW(), 'Operations decision maker', NOW(), 0);

-- =====================================================
-- SUMMARY
-- =====================================================
-- Total Customers: 12
-- - Tech Corp: 3 linked contacts (enterprise flagship)
-- - Innovation Inc: 0 linked contacts (new account)
-- - Acme Industries: 1 linked contact
-- - TechFlow Solutions: 2 linked contacts
-- - Green Earth Consulting: 1 linked contact
-- - Midwest Healthcare Group: 2 linked contacts
-- - Pacific Retail Partners: 1 linked contact
-- - Summit Financial Advisors: 2 linked contacts
-- - Atlantic Shipping Co: 1 linked contact
-- - Desert Sun Energy: 2 linked contacts
-- - Creative Media Studios: 1 linked contact
-- - Precision Manufacturing Inc: 4 linked contacts (major account)
--
-- Industries represented:
-- - Technology (3)
-- - Manufacturing (2)
-- - Healthcare (1)
-- - Retail (1)
-- - Financial Services (1)
-- - Transportation (1)
-- - Energy (1)
-- - Media & Entertainment (1)
-- - Consulting (1)
-- =====================================================
