-- ============================================================================
-- CRM Solution Database Seed Data - BASIC
-- Version: 2.0
-- Description: Sample entries for each table - minimal but complete set
-- Copyright (C) 2024-2026 Abhishek Lal
-- Licensed under the GNU Affero General Public License v3.0
-- ============================================================================
-- Prerequisites: Run 01_essential_data.sql first
-- This file adds sample entries to demonstrate system functionality
-- ============================================================================

SET client_encoding = 'UTF8';

-- ============================================================================
-- SECTION 1: SAMPLE USERS (3 users - different roles)
-- ============================================================================

-- Sales Manager
INSERT INTO "Users" ("Id", "Username", "Email", "PasswordHash", "FirstName", "LastName", 
    "IsActive", "UserGroupId", "DepartmentId", "CreatedAt", "IsDeleted", "EmailConfirmed", 
    "PhoneNumber", "Title", "MustChangePassword")
VALUES 
    (2, 'jsmith', 'john.smith@crm.local', '$2a$11$8K1p/a0dL1LXMIgoEDFrwOexJujLDl1CZa4GlX2NqwONNlZwGLGHC', 
     'John', 'Smith', true, 2, 1, NOW(), false, true, '+1-555-0101', 'Sales Manager', false)
ON CONFLICT ("Id") DO NOTHING;

-- Sales Representative
INSERT INTO "Users" ("Id", "Username", "Email", "PasswordHash", "FirstName", "LastName", 
    "IsActive", "UserGroupId", "DepartmentId", "CreatedAt", "IsDeleted", "EmailConfirmed",
    "PhoneNumber", "Title", "MustChangePassword")
VALUES 
    (3, 'mjohnson', 'mary.johnson@crm.local', '$2a$11$8K1p/a0dL1LXMIgoEDFrwOexJujLDl1CZa4GlX2NqwONNlZwGLGHC', 
     'Mary', 'Johnson', true, 3, 1, NOW(), false, true, '+1-555-0102', 'Sales Representative', false)
ON CONFLICT ("Id") DO NOTHING;

-- Support Agent
INSERT INTO "Users" ("Id", "Username", "Email", "PasswordHash", "FirstName", "LastName", 
    "IsActive", "UserGroupId", "DepartmentId", "CreatedAt", "IsDeleted", "EmailConfirmed",
    "PhoneNumber", "Title", "MustChangePassword")
VALUES 
    (4, 'rwilliams', 'robert.williams@crm.local', '$2a$11$8K1p/a0dL1LXMIgoEDFrwOexJujLDl1CZa4GlX2NqwONNlZwGLGHC', 
     'Robert', 'Williams', true, 7, 3, NOW(), false, true, '+1-555-0103', 'Support Agent', false)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"Users_Id_seq"', (SELECT MAX("Id") FROM "Users"));

-- ============================================================================
-- SECTION 2: SAMPLE CUSTOMERS (3 types)
-- ============================================================================

-- Individual Customer
INSERT INTO "Customers" ("Id", "Category", "FirstName", "LastName", "Email", "Phone",
    "CustomerType", "LifecycleStage", "Priority", "Address", "City", "State", "PostalCode", "Country",
    "IsActive", "CreatedAt", "IsDeleted", "OwnerId")
VALUES 
    (1, 0, 'Alice', 'Brown', 'alice.brown@email.com', '+1-555-0201',
     0, 3, 1, '123 Oak Street', 'Boston', 'MA', '02101', 'United States',
     true, NOW(), false, 2)
ON CONFLICT ("Id") DO NOTHING;

-- Small Business Customer
INSERT INTO "Customers" ("Id", "Category", "Company", "LegalName", "Email", "Phone",
    "CustomerType", "LifecycleStage", "Priority", "Industry", "Website", "AnnualRevenue",
    "Address", "City", "State", "PostalCode", "Country",
    "IsActive", "CreatedAt", "IsDeleted", "OwnerId")
VALUES 
    (2, 1, 'Acme Solutions LLC', 'Acme Solutions Limited Liability Company', 
     'info@acmesolutions.com', '+1-555-0202',
     1, 3, 2, 'Technology', 'https://acmesolutions.com', 1500000.00,
     '456 Tech Park Drive', 'San Jose', 'CA', '95101', 'United States',
     true, NOW(), false, 2)
ON CONFLICT ("Id") DO NOTHING;

-- Enterprise Customer
INSERT INTO "Customers" ("Id", "Category", "Company", "LegalName", "Email", "Phone",
    "CustomerType", "LifecycleStage", "Priority", "Industry", "Website", "AnnualRevenue",
    "Address", "City", "State", "PostalCode", "Country",
    "IsActive", "CreatedAt", "IsDeleted", "OwnerId")
VALUES 
    (3, 1, 'Global Industries Inc', 'Global Industries Incorporated', 
     'contact@globalindustries.com', '+1-555-0203',
     3, 3, 3, 'Manufacturing', 'https://globalindustries.com', 50000000.00,
     '789 Corporate Boulevard', 'Chicago', 'IL', '60601', 'United States',
     true, NOW(), false, 2)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"Customers_Id_seq"', (SELECT MAX("Id") FROM "Customers"));

-- ============================================================================
-- SECTION 3: SAMPLE CONTACTS (for business customers)
-- ============================================================================

-- Contact for Acme Solutions
INSERT INTO "Contacts" ("Id", "FirstName", "LastName", "Email", "Phone", "Title",
    "CustomerId", "IsPrimary", "IsActive", "CreatedAt", "IsDeleted")
VALUES 
    (1, 'David', 'Chen', 'david.chen@acmesolutions.com', '+1-555-0211',
     'CEO', 2, true, true, NOW(), false),
    (2, 'Sarah', 'Martinez', 'sarah.martinez@acmesolutions.com', '+1-555-0212',
     'CTO', 2, false, true, NOW(), false)
ON CONFLICT ("Id") DO NOTHING;

-- Contacts for Global Industries
INSERT INTO "Contacts" ("Id", "FirstName", "LastName", "Email", "Phone", "Title",
    "CustomerId", "IsPrimary", "IsActive", "CreatedAt", "IsDeleted")
VALUES 
    (3, 'Michael', 'Thompson', 'michael.thompson@globalindustries.com', '+1-555-0221',
     'VP of Operations', 3, true, true, NOW(), false),
    (4, 'Jennifer', 'Lee', 'jennifer.lee@globalindustries.com', '+1-555-0222',
     'Director of IT', 3, false, true, NOW(), false),
    (5, 'Robert', 'Garcia', 'robert.garcia@globalindustries.com', '+1-555-0223',
     'Procurement Manager', 3, false, true, NOW(), false)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"Contacts_Id_seq"', (SELECT MAX("Id") FROM "Contacts"));

-- ============================================================================
-- SECTION 4: SAMPLE PRODUCTS (4 types)
-- ============================================================================

-- One-time purchase product
INSERT INTO "Products" ("Id", "Name", "SKU", "Description", "Price", "Cost", "Category",
    "ProductType", "IsActive", "IsTaxable", "CreatedAt", "IsDeleted")
VALUES 
    (1, 'CRM Professional License', 'CRM-PRO-001', 
     'One-time perpetual license for CRM Professional Edition. Includes 1 year of updates.',
     999.00, 200.00, 'Software', 0, true, true, NOW(), false)
ON CONFLICT ("Id") DO NOTHING;

-- Subscription product - Monthly
INSERT INTO "Products" ("Id", "Name", "SKU", "Description", "Price", "Cost", "Category",
    "ProductType", "BillingFrequency", "IsActive", "IsTaxable", "CreatedAt", "IsDeleted")
VALUES 
    (2, 'CRM Cloud - Monthly', 'CRM-CLOUD-M', 
     'Monthly subscription for CRM Cloud service. Includes all features and support.',
     99.00, 15.00, 'Subscription', 1, 'Monthly', true, true, NOW(), false)
ON CONFLICT ("Id") DO NOTHING;

-- Subscription product - Annual
INSERT INTO "Products" ("Id", "Name", "SKU", "Description", "Price", "Cost", "Category",
    "ProductType", "BillingFrequency", "IsActive", "IsTaxable", "CreatedAt", "IsDeleted")
VALUES 
    (3, 'CRM Cloud - Annual', 'CRM-CLOUD-A', 
     'Annual subscription for CRM Cloud service. Save 20% compared to monthly.',
     948.00, 144.00, 'Subscription', 1, 'Yearly', true, true, NOW(), false)
ON CONFLICT ("Id") DO NOTHING;

-- Usage-based product
INSERT INTO "Products" ("Id", "Name", "SKU", "Description", "Price", "Cost", "Category",
    "ProductType", "IsActive", "IsTaxable", "CreatedAt", "IsDeleted")
VALUES 
    (4, 'API Calls Package - 10K', 'API-10K', 
     '10,000 API calls per month. Overage charged at $0.005 per call.',
     49.00, 5.00, 'Usage', 2, true, true, NOW(), false)
ON CONFLICT ("Id") DO NOTHING;

-- Professional Services
INSERT INTO "Products" ("Id", "Name", "SKU", "Description", "Price", "Cost", "Category",
    "ProductType", "IsActive", "IsTaxable", "CreatedAt", "IsDeleted")
VALUES 
    (5, 'Implementation Services', 'SVC-IMPL', 
     'Professional implementation and setup services. Includes data migration and training.',
     2500.00, 1000.00, 'Services', 0, true, false, NOW(), false)
ON CONFLICT ("Id") DO NOTHING;

-- Support Package
INSERT INTO "Products" ("Id", "Name", "SKU", "Description", "Price", "Cost", "Category",
    "ProductType", "BillingFrequency", "IsActive", "IsTaxable", "CreatedAt", "IsDeleted")
VALUES 
    (6, 'Premium Support', 'SUP-PREM', 
     'Premium support package with 24/7 phone support and 4-hour response SLA.',
     199.00, 50.00, 'Support', 1, 'Monthly', true, true, NOW(), false)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"Products_Id_seq"', (SELECT MAX("Id") FROM "Products"));

-- ============================================================================
-- SECTION 5: SAMPLE LEADS (3 stages)
-- ============================================================================

-- New Lead
INSERT INTO "Leads" ("Id", "FirstName", "LastName", "Email", "Phone", "Company",
    "Status", "Source", "Score", "Title", "Address", "City", "State", "PostalCode", "Country",
    "CreatedAt", "IsDeleted", "OwnerId")
VALUES 
    (1, 'Emily', 'Davis', 'emily.davis@techstartup.com', '+1-555-0301',
     'Tech Startup Inc', 0, 0, 45, 'CTO',
     '100 Innovation Way', 'Austin', 'TX', '78701', 'United States',
     NOW(), false, 3)
ON CONFLICT ("Id") DO NOTHING;

-- Qualified Lead
INSERT INTO "Leads" ("Id", "FirstName", "LastName", "Email", "Phone", "Company",
    "Status", "Source", "Score", "Title", "Address", "City", "State", "PostalCode", "Country",
    "CreatedAt", "IsDeleted", "OwnerId")
VALUES 
    (2, 'James', 'Wilson', 'james.wilson@bigcorp.com', '+1-555-0302',
     'BigCorp International', 2, 1, 78, 'VP of Operations',
     '500 Enterprise Drive', 'Seattle', 'WA', '98101', 'United States',
     NOW() - INTERVAL '7 days', false, 3)
ON CONFLICT ("Id") DO NOTHING;

-- Hot Lead (high score)
INSERT INTO "Leads" ("Id", "FirstName", "LastName", "Email", "Phone", "Company",
    "Status", "Source", "Score", "Title", "Address", "City", "State", "PostalCode", "Country",
    "CreatedAt", "IsDeleted", "OwnerId")
VALUES 
    (3, 'Amanda', 'Rodriguez', 'amanda.rodriguez@enterprise.com', '+1-555-0303',
     'Enterprise Solutions Corp', 3, 2, 92, 'Director of IT',
     '1000 Business Center', 'Denver', 'CO', '80201', 'United States',
     NOW() - INTERVAL '14 days', false, 2)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"Leads_Id_seq"', (SELECT MAX("Id") FROM "Leads"));

-- ============================================================================
-- SECTION 6: SAMPLE OPPORTUNITIES (3 stages)
-- ============================================================================

-- Discovery Stage
INSERT INTO "Opportunities" ("Id", "Name", "Stage", "Probability", "Amount", "Currency",
    "ExpectedCloseDate", "CustomerId", "CreatedAt", "IsDeleted", "OwnerId")
VALUES 
    (1, 'CRM Implementation - Acme Solutions', 0, 10, 25000.00, 'USD',
     NOW() + INTERVAL '90 days', 2, NOW(), false, 3)
ON CONFLICT ("Id") DO NOTHING;

-- Proposal Stage
INSERT INTO "Opportunities" ("Id", "Name", "Stage", "Probability", "Amount", "Currency",
    "ExpectedCloseDate", "CustomerId", "CreatedAt", "IsDeleted", "OwnerId")
VALUES 
    (2, 'Cloud Migration - Global Industries', 2, 50, 75000.00, 'USD',
     NOW() + INTERVAL '60 days', 3, NOW() - INTERVAL '30 days', false, 2)
ON CONFLICT ("Id") DO NOTHING;

-- Negotiation Stage
INSERT INTO "Opportunities" ("Id", "Name", "Stage", "Probability", "Amount", "Currency",
    "ExpectedCloseDate", "CustomerId", "CreatedAt", "IsDeleted", "OwnerId")
VALUES 
    (3, 'Enterprise License Deal - Global Industries', 3, 75, 150000.00, 'USD',
     NOW() + INTERVAL '30 days', 3, NOW() - INTERVAL '45 days', false, 2)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"Opportunities_Id_seq"', (SELECT MAX("Id") FROM "Opportunities"));

-- ============================================================================
-- SECTION 7: SAMPLE CAMPAIGNS (3 types)
-- ============================================================================

-- Email Campaign
INSERT INTO "Campaigns" ("Id", "Name", "Type", "Status", "Description",
    "StartDate", "EndDate", "Budget", "ActualCost", "CreatedAt", "IsDeleted", "OwnerId")
VALUES 
    (1, 'Q1 Newsletter Campaign', 0, 1, 
     'Quarterly newsletter to all customers highlighting new features and success stories.',
     NOW() - INTERVAL '30 days', NOW() + INTERVAL '30 days', 5000.00, 2500.00, 
     NOW() - INTERVAL '30 days', false, 2)
ON CONFLICT ("Id") DO NOTHING;

-- Event Campaign
INSERT INTO "Campaigns" ("Id", "Name", "Type", "Status", "Description",
    "StartDate", "EndDate", "Budget", "ExpectedRevenue", "CreatedAt", "IsDeleted", "OwnerId")
VALUES 
    (2, 'Annual User Conference', 1, 0, 
     'Annual customer conference and networking event. Features keynotes, workshops, and demos.',
     NOW() + INTERVAL '60 days', NOW() + INTERVAL '62 days', 50000.00, 100000.00,
     NOW(), false, 2)
ON CONFLICT ("Id") DO NOTHING;

-- Webinar Campaign
INSERT INTO "Campaigns" ("Id", "Name", "Type", "Status", "Description",
    "StartDate", "EndDate", "Budget", "ExpectedLeads", "CreatedAt", "IsDeleted", "OwnerId")
VALUES 
    (3, 'Product Demo Webinar Series', 2, 1, 
     'Weekly product demonstration webinars showcasing key features and use cases.',
     NOW() - INTERVAL '7 days', NOW() + INTERVAL '90 days', 2000.00, 100,
     NOW() - INTERVAL '7 days', false, 2)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"Campaigns_Id_seq"', (SELECT MAX("Id") FROM "Campaigns"));

-- ============================================================================
-- SECTION 8: SAMPLE ACCOUNTS (Billing Records)
-- ============================================================================

-- Account for Acme Solutions
INSERT INTO "Accounts" ("Id", "AccountNumber", "CustomerId", "ProductId",
    "Status", "BillingFrequency", "MRR", "ARR", "Currency",
    "ContractStartDate", "ContractEndDate", "CreatedAt", "IsDeleted")
VALUES 
    (1, 'ACC-2024-001', 2, 2, 0, 'Monthly', 99.00, 1188.00, 'USD',
     NOW(), NOW() + INTERVAL '1 year', NOW(), false)
ON CONFLICT ("Id") DO NOTHING;

-- Account for Global Industries
INSERT INTO "Accounts" ("Id", "AccountNumber", "CustomerId", "ProductId",
    "Status", "BillingFrequency", "MRR", "ARR", "Currency",
    "ContractStartDate", "ContractEndDate", "CreatedAt", "IsDeleted")
VALUES 
    (2, 'ACC-2024-002', 3, 3, 0, 'Yearly', 1245.67, 14948.00, 'USD',
     NOW() - INTERVAL '30 days', NOW() + INTERVAL '11 months', NOW() - INTERVAL '30 days', false)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"Accounts_Id_seq"', (SELECT MAX("Id") FROM "Accounts"));

-- ============================================================================
-- SECTION 9: SAMPLE TASKS
-- ============================================================================

INSERT INTO "Tasks" ("Id", "Title", "Description", "DueDate", "Priority", "Status",
    "TaskType", "CustomerId", "OwnerId", "CreatedAt", "IsDeleted")
VALUES 
    (1, 'Follow up on proposal', 'Follow up with Acme Solutions regarding the CRM implementation proposal.',
     NOW() + INTERVAL '3 days', 2, 0, 0, 2, 3, NOW(), false),
    (2, 'Schedule demo call', 'Schedule a demo call with Global Industries to show advanced features.',
     NOW() + INTERVAL '7 days', 1, 0, 3, 3, 2, NOW(), false),
    (3, 'Send pricing information', 'Send updated pricing sheet to Tech Startup Inc.',
     NOW() + INTERVAL '1 day', 2, 1, 1, NULL, 3, NOW(), false)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"Tasks_Id_seq"', (SELECT MAX("Id") FROM "Tasks"));

-- ============================================================================
-- SECTION 10: SAMPLE SERVICE REQUESTS
-- ============================================================================

INSERT INTO "ServiceRequests" ("Id", "Subject", "Description", "Priority", "Status",
    "Type", "CustomerId", "AssignedToId", "CreatedAt", "IsDeleted")
VALUES 
    (1, 'Login issue - password reset not working', 
     'Customer reports that password reset emails are not being received.',
     2, 1, 'SUPPORT', 2, 4, NOW() - INTERVAL '1 day', false),
    (2, 'Feature request - custom dashboard', 
     'Customer would like to have a customizable dashboard for their team.',
     0, 0, 'FEATURE', 3, NULL, NOW(), false),
    (3, 'Billing inquiry - invoice discrepancy', 
     'Customer has questions about charges on their latest invoice.',
     1, 2, 'BILLING', 3, 4, NOW() - INTERVAL '2 hours', false)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"ServiceRequests_Id_seq"', (SELECT MAX("Id") FROM "ServiceRequests"));

-- ============================================================================
-- VERIFICATION
-- ============================================================================

DO $$
BEGIN
    RAISE NOTICE 'Basic data verification:';
    RAISE NOTICE '  - Users: % rows (expected 4+)', (SELECT COUNT(*) FROM "Users");
    RAISE NOTICE '  - Customers: % rows (expected 3+)', (SELECT COUNT(*) FROM "Customers");
    RAISE NOTICE '  - Contacts: % rows (expected 5+)', (SELECT COUNT(*) FROM "Contacts");
    RAISE NOTICE '  - Products: % rows (expected 6+)', (SELECT COUNT(*) FROM "Products");
    RAISE NOTICE '  - Leads: % rows (expected 3+)', (SELECT COUNT(*) FROM "Leads");
    RAISE NOTICE '  - Opportunities: % rows (expected 3+)', (SELECT COUNT(*) FROM "Opportunities");
    RAISE NOTICE '  - Campaigns: % rows (expected 3+)', (SELECT COUNT(*) FROM "Campaigns");
    RAISE NOTICE '  - Accounts: % rows (expected 2+)', (SELECT COUNT(*) FROM "Accounts");
    RAISE NOTICE '  - Tasks: % rows (expected 3+)', (SELECT COUNT(*) FROM "Tasks");
    RAISE NOTICE '  - ServiceRequests: % rows (expected 3+)', (SELECT COUNT(*) FROM "ServiceRequests");
    RAISE NOTICE 'Basic data loading complete!';
END $$;
