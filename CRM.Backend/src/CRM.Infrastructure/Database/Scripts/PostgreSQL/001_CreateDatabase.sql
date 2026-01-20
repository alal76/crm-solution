-- ============================================
-- CRM Database Schema for PostgreSQL
-- Version: 1.0.0
-- Generated: 2026-01-20
-- ============================================

-- Create database (run this as superuser if needed)
-- CREATE DATABASE crm_database;
-- \c crm_database

-- ============================================
-- Core Tables
-- ============================================

-- Departments Table
CREATE TABLE IF NOT EXISTS "Departments" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Description" TEXT DEFAULT '',
    "DepartmentCode" VARCHAR(20),
    "IsActive" BOOLEAN DEFAULT TRUE,
    "ParentDepartmentId" INTEGER,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "FK_Departments_Parent" FOREIGN KEY ("ParentDepartmentId") REFERENCES "Departments"("Id") ON DELETE SET NULL
);

-- User Profiles Table
CREATE TABLE IF NOT EXISTS "UserProfiles" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Description" TEXT DEFAULT '',
    "DepartmentId" INTEGER NOT NULL,
    "IsActive" BOOLEAN DEFAULT TRUE,
    "AccessiblePages" TEXT DEFAULT '[]',
    "CanCreateCustomers" BOOLEAN DEFAULT FALSE,
    "CanEditCustomers" BOOLEAN DEFAULT FALSE,
    "CanDeleteCustomers" BOOLEAN DEFAULT FALSE,
    "CanCreateOpportunities" BOOLEAN DEFAULT FALSE,
    "CanEditOpportunities" BOOLEAN DEFAULT FALSE,
    "CanDeleteOpportunities" BOOLEAN DEFAULT FALSE,
    "CanCreateProducts" BOOLEAN DEFAULT FALSE,
    "CanEditProducts" BOOLEAN DEFAULT FALSE,
    "CanDeleteProducts" BOOLEAN DEFAULT FALSE,
    "CanManageCampaigns" BOOLEAN DEFAULT FALSE,
    "CanViewReports" BOOLEAN DEFAULT FALSE,
    "CanManageUsers" BOOLEAN DEFAULT FALSE,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "FK_UserProfiles_Department" FOREIGN KEY ("DepartmentId") REFERENCES "Departments"("Id") ON DELETE CASCADE
);

-- Users Table
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Username" VARCHAR(100) NOT NULL UNIQUE,
    "Email" VARCHAR(255) NOT NULL UNIQUE,
    "FirstName" VARCHAR(100) DEFAULT '',
    "LastName" VARCHAR(100) DEFAULT '',
    "PasswordHash" VARCHAR(500) NOT NULL,
    "Role" INTEGER DEFAULT 0,
    "IsActive" BOOLEAN DEFAULT TRUE,
    "LastLoginDate" TIMESTAMP,
    "TwoFactorEnabled" BOOLEAN DEFAULT FALSE,
    "TwoFactorSecret" VARCHAR(500),
    "BackupCodes" TEXT,
    "PasswordResetToken" VARCHAR(500),
    "PasswordResetTokenExpiry" TIMESTAMP,
    "EmailVerified" BOOLEAN DEFAULT FALSE,
    "EmailVerificationToken" VARCHAR(500),
    "DepartmentId" INTEGER,
    "UserProfileId" INTEGER,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "FK_Users_Department" FOREIGN KEY ("DepartmentId") REFERENCES "Departments"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Users_UserProfile" FOREIGN KEY ("UserProfileId") REFERENCES "UserProfiles"("Id") ON DELETE SET NULL
);

-- User Groups Table
CREATE TABLE IF NOT EXISTS "UserGroups" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Description" TEXT DEFAULT '',
    "IsActive" BOOLEAN DEFAULT TRUE,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE
);

-- User Group Members Table
CREATE TABLE IF NOT EXISTS "UserGroupMembers" (
    "Id" SERIAL PRIMARY KEY,
    "UserGroupId" INTEGER NOT NULL,
    "UserId" INTEGER NOT NULL,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "FK_UserGroupMembers_Group" FOREIGN KEY ("UserGroupId") REFERENCES "UserGroups"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserGroupMembers_User" FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE
);

-- User Approval Requests Table
CREATE TABLE IF NOT EXISTS "UserApprovalRequests" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL,
    "RequestedRole" INTEGER NOT NULL,
    "RequestedDepartmentId" INTEGER,
    "Status" INTEGER DEFAULT 0,
    "RequestedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "ReviewedAt" TIMESTAMP,
    "ReviewedByUserId" INTEGER,
    "Notes" TEXT,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "FK_ApprovalRequests_User" FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ApprovalRequests_Department" FOREIGN KEY ("RequestedDepartmentId") REFERENCES "Departments"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_ApprovalRequests_Reviewer" FOREIGN KEY ("ReviewedByUserId") REFERENCES "Users"("Id") ON DELETE SET NULL
);

-- OAuth Tokens Table
CREATE TABLE IF NOT EXISTS "OAuthTokens" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL,
    "Provider" VARCHAR(50) NOT NULL,
    "AccessToken" TEXT NOT NULL,
    "RefreshToken" TEXT,
    "ExpiresAt" TIMESTAMP,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "FK_OAuthTokens_User" FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE
);

-- Database Backups Table
CREATE TABLE IF NOT EXISTS "DatabaseBackups" (
    "Id" SERIAL PRIMARY KEY,
    "FileName" VARCHAR(255) NOT NULL,
    "FilePath" VARCHAR(500) NOT NULL,
    "FileSize" BIGINT,
    "BackupType" VARCHAR(50) DEFAULT 'Full',
    "Status" VARCHAR(50) DEFAULT 'Completed',
    "CreatedByUserId" INTEGER,
    "Notes" TEXT,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "FK_DatabaseBackups_User" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users"("Id") ON DELETE SET NULL
);

-- ============================================
-- CRM Entity Tables
-- ============================================

-- Customers Table
CREATE TABLE IF NOT EXISTS "Customers" (
    "Id" SERIAL PRIMARY KEY,
    -- Basic Information
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "Email" VARCHAR(255) NOT NULL UNIQUE,
    "SecondaryEmail" VARCHAR(255),
    "Phone" VARCHAR(20) DEFAULT '',
    "MobilePhone" VARCHAR(20),
    "Company" VARCHAR(255) DEFAULT '',
    "JobTitle" VARCHAR(100),
    "Website" VARCHAR(500),
    -- Address Information
    "Address" VARCHAR(500) DEFAULT '',
    "Address2" VARCHAR(500),
    "City" VARCHAR(100) DEFAULT '',
    "State" VARCHAR(100) DEFAULT '',
    "ZipCode" VARCHAR(20) DEFAULT '',
    "Country" VARCHAR(100) DEFAULT '',
    -- Business Information
    "Industry" VARCHAR(100),
    "NumberOfEmployees" INTEGER,
    "AnnualRevenue" DECIMAL(18,2) DEFAULT 0,
    "CustomerType" INTEGER DEFAULT 0,
    "Priority" INTEGER DEFAULT 1,
    -- Lifecycle & Status
    "LifecycleStage" INTEGER DEFAULT 0,
    "LeadSource" VARCHAR(100),
    "FirstContactDate" TIMESTAMP,
    "ConversionDate" TIMESTAMP,
    "LastActivityDate" TIMESTAMP,
    "NextFollowUpDate" TIMESTAMP,
    -- Financial
    "TotalPurchases" DECIMAL(18,2) DEFAULT 0,
    "AccountBalance" DECIMAL(18,2) DEFAULT 0,
    "CreditLimit" DECIMAL(18,2) DEFAULT 0,
    "PaymentTerms" VARCHAR(50),
    "PreferredPaymentMethod" VARCHAR(50),
    -- Scoring & Rating
    "LeadScore" INTEGER DEFAULT 0,
    "CustomerHealthScore" INTEGER DEFAULT 50,
    "NpsScore" INTEGER DEFAULT 0,
    "SatisfactionRating" DOUBLE PRECISION DEFAULT 0,
    -- Social & Communication
    "LinkedInUrl" VARCHAR(500),
    "TwitterHandle" VARCHAR(100),
    "FacebookUrl" VARCHAR(500),
    "OptInEmail" BOOLEAN DEFAULT TRUE,
    "OptInSms" BOOLEAN DEFAULT FALSE,
    "OptInPhone" BOOLEAN DEFAULT TRUE,
    "PreferredContactMethod" VARCHAR(50),
    "PreferredContactTime" VARCHAR(50),
    "Timezone" VARCHAR(50),
    -- Assignment & Ownership
    "AssignedToUserId" INTEGER,
    "AccountManagerId" INTEGER,
    "Territory" VARCHAR(100),
    -- Classification
    "Tags" TEXT,
    "Segment" VARCHAR(100),
    "ReferralSource" VARCHAR(100),
    "ReferredByCustomerId" INTEGER,
    -- Documentation
    "Notes" TEXT DEFAULT '',
    "InternalNotes" TEXT,
    "Description" TEXT,
    "CustomFields" TEXT,
    -- Audit
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "FK_Customers_AssignedTo" FOREIGN KEY ("AssignedToUserId") REFERENCES "Users"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Customers_AccountManager" FOREIGN KEY ("AccountManagerId") REFERENCES "Users"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Customers_ReferredBy" FOREIGN KEY ("ReferredByCustomerId") REFERENCES "Customers"("Id") ON DELETE SET NULL
);

-- Products Table
CREATE TABLE IF NOT EXISTS "Products" (
    "Id" SERIAL PRIMARY KEY,
    -- Basic Information
    "Name" VARCHAR(255) NOT NULL,
    "Description" TEXT DEFAULT '',
    "ShortDescription" VARCHAR(500),
    "SKU" VARCHAR(100) NOT NULL UNIQUE,
    "Barcode" VARCHAR(100),
    "ExternalId" VARCHAR(100),
    -- Classification
    "ProductType" INTEGER DEFAULT 0,
    "Status" INTEGER DEFAULT 1,
    "Category" VARCHAR(100) DEFAULT '',
    "SubCategory" VARCHAR(100),
    "Brand" VARCHAR(100),
    "Manufacturer" VARCHAR(100),
    "Tags" TEXT,
    -- Pricing
    "Price" DECIMAL(18,2) DEFAULT 0,
    "ListPrice" DECIMAL(18,2),
    "Cost" DECIMAL(18,2) DEFAULT 0,
    "MinimumPrice" DECIMAL(18,2),
    "WholesalePrice" DECIMAL(18,2),
    "Margin" DECIMAL(18,2) DEFAULT 0,
    "CurrencyCode" VARCHAR(3) DEFAULT 'USD',
    "IsTaxable" BOOLEAN DEFAULT TRUE,
    "TaxRate" DECIMAL(5,2),
    -- Subscription Pricing
    "BillingFrequency" INTEGER DEFAULT 0,
    "RecurringPrice" DECIMAL(18,2),
    "SetupFee" DECIMAL(18,2),
    "TrialPeriodDays" INTEGER,
    "ContractLengthMonths" INTEGER,
    -- Inventory
    "Quantity" INTEGER DEFAULT 0,
    "ReorderLevel" INTEGER,
    "ReorderQuantity" INTEGER,
    "MaxQuantity" INTEGER,
    "ReservedQuantity" INTEGER,
    "AvailableQuantity" INTEGER,
    "WarehouseLocation" VARCHAR(100),
    "TrackInventory" BOOLEAN DEFAULT TRUE,
    "AllowBackorder" BOOLEAN DEFAULT FALSE,
    -- Physical Attributes
    "Weight" DECIMAL(10,2),
    "WeightUnit" VARCHAR(10) DEFAULT 'kg',
    "Length" DECIMAL(10,2),
    "Width" DECIMAL(10,2),
    "Height" DECIMAL(10,2),
    "DimensionUnit" VARCHAR(10) DEFAULT 'cm',
    -- Media
    "ImageUrl" VARCHAR(500) DEFAULT '',
    "ThumbnailUrl" VARCHAR(500),
    "AdditionalImages" TEXT,
    "VideoUrl" VARCHAR(500),
    "DocumentUrls" TEXT,
    -- SEO & Marketing
    "MetaTitle" VARCHAR(100),
    "MetaDescription" VARCHAR(500),
    "MetaKeywords" VARCHAR(500),
    "Slug" VARCHAR(200),
    -- Relationships
    "ParentProductId" INTEGER,
    "VendorId" INTEGER,
    "ProductFamilyId" INTEGER,
    -- Features & Specifications
    "Features" TEXT,
    "Specifications" TEXT,
    "Warranty" VARCHAR(500),
    "SupportInfo" TEXT,
    -- Sales Information
    "TotalSold" INTEGER DEFAULT 0,
    "TotalRevenue" DECIMAL(18,2) DEFAULT 0,
    "AverageRating" DOUBLE PRECISION DEFAULT 0,
    "ReviewCount" INTEGER DEFAULT 0,
    "IsFeatured" BOOLEAN DEFAULT FALSE,
    "IsBestSeller" BOOLEAN DEFAULT FALSE,
    -- Dates
    "AvailableFrom" TIMESTAMP,
    "AvailableTo" TIMESTAMP,
    "DiscontinuedDate" TIMESTAMP,
    -- Legacy
    "IsActive" BOOLEAN DEFAULT TRUE,
    "CustomFields" TEXT,
    -- Audit
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "FK_Products_Parent" FOREIGN KEY ("ParentProductId") REFERENCES "Products"("Id") ON DELETE SET NULL
);

-- Marketing Campaigns Table
CREATE TABLE IF NOT EXISTS "MarketingCampaigns" (
    "Id" SERIAL PRIMARY KEY,
    -- Basic Information
    "Name" VARCHAR(255) NOT NULL,
    "Description" TEXT DEFAULT '',
    "Objective" TEXT,
    "CampaignType" INTEGER DEFAULT 0,
    "Status" INTEGER DEFAULT 0,
    "Priority" INTEGER DEFAULT 1,
    "Type" VARCHAR(50) DEFAULT '',
    -- Dates
    "StartDate" TIMESTAMP,
    "EndDate" TIMESTAMP,
    "ActualStartDate" TIMESTAMP,
    "ActualEndDate" TIMESTAMP,
    -- Budget & Costs
    "Budget" DECIMAL(18,2) DEFAULT 0,
    "ActualCost" DECIMAL(18,2) DEFAULT 0,
    "ExpectedRevenue" DECIMAL(18,2),
    "ActualRevenue" DECIMAL(18,2),
    "CostPerLead" DECIMAL(18,2),
    "CostPerAcquisition" DECIMAL(18,2),
    "CurrencyCode" VARCHAR(3) DEFAULT 'USD',
    -- Target Audience
    "TargetAudience" INTEGER DEFAULT 0,
    "TargetAudienceDescription" TEXT,
    "TargetDemographics" TEXT,
    "TargetGeography" VARCHAR(500),
    "TargetIndustries" VARCHAR(500),
    "TargetSegments" VARCHAR(500),
    "ExclusionCriteria" TEXT,
    -- Performance Metrics
    "ConversionRate" DOUBLE PRECISION DEFAULT 0,
    "Impressions" INTEGER DEFAULT 0,
    "Clicks" INTEGER DEFAULT 0,
    "ClickThroughRate" DOUBLE PRECISION DEFAULT 0,
    "LeadsGenerated" INTEGER DEFAULT 0,
    "OpportunitiesCreated" INTEGER DEFAULT 0,
    "CustomersAcquired" INTEGER DEFAULT 0,
    "ROI" DOUBLE PRECISION DEFAULT 0,
    "EmailsSent" INTEGER DEFAULT 0,
    "EmailsOpened" INTEGER DEFAULT 0,
    "OpenRate" DOUBLE PRECISION DEFAULT 0,
    "Bounces" INTEGER DEFAULT 0,
    "Unsubscribes" INTEGER DEFAULT 0,
    -- Social Media Metrics
    "SocialReach" INTEGER DEFAULT 0,
    "SocialEngagement" INTEGER DEFAULT 0,
    "SocialShares" INTEGER DEFAULT 0,
    "SocialComments" INTEGER DEFAULT 0,
    "SocialLikes" INTEGER DEFAULT 0,
    "NewFollowers" INTEGER DEFAULT 0,
    -- Content
    "MessageSubject" VARCHAR(500),
    "MessageBody" TEXT,
    "CallToAction" VARCHAR(500),
    "LandingPageUrl" VARCHAR(500),
    "TrackingUrl" VARCHAR(500),
    "UtmSource" VARCHAR(100),
    "UtmMedium" VARCHAR(100),
    "UtmCampaign" VARCHAR(100),
    "UtmContent" VARCHAR(100),
    -- A/B Testing
    "IsABTest" BOOLEAN DEFAULT FALSE,
    "ABTestVariants" TEXT,
    "WinningVariant" VARCHAR(100),
    -- Channels & Platforms
    "Channels" TEXT,
    "Platforms" TEXT,
    -- Assignment
    "OwnerId" INTEGER,
    "AssignedToUserId" INTEGER,
    "TeamMembers" TEXT,
    -- Related
    "ParentCampaignId" INTEGER,
    "RelatedCampaigns" TEXT,
    -- Classification
    "Tags" TEXT,
    "Category" VARCHAR(100),
    -- Documentation
    "Notes" TEXT,
    "InternalNotes" TEXT,
    "Attachments" TEXT,
    "CustomFields" TEXT,
    -- Audit
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "FK_Campaigns_Owner" FOREIGN KEY ("OwnerId") REFERENCES "Users"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Campaigns_AssignedTo" FOREIGN KEY ("AssignedToUserId") REFERENCES "Users"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Campaigns_Parent" FOREIGN KEY ("ParentCampaignId") REFERENCES "MarketingCampaigns"("Id") ON DELETE SET NULL
);

-- Opportunities Table
CREATE TABLE IF NOT EXISTS "Opportunities" (
    "Id" SERIAL PRIMARY KEY,
    -- Basic Information
    "Name" VARCHAR(255) NOT NULL,
    "Description" TEXT DEFAULT '',
    "OpportunityType" INTEGER DEFAULT 0,
    "Priority" INTEGER DEFAULT 1,
    -- Financial
    "Amount" DECIMAL(18,2) DEFAULT 0,
    "ExpectedRevenue" DECIMAL(18,2) DEFAULT 0,
    "RecurringRevenue" DECIMAL(18,2) DEFAULT 0,
    "OneTimeRevenue" DECIMAL(18,2) DEFAULT 0,
    "Discount" DECIMAL(18,2) DEFAULT 0,
    "DiscountPercent" DECIMAL(5,2) DEFAULT 0,
    "CurrencyCode" VARCHAR(3) DEFAULT 'USD',
    -- Stage & Probability
    "Stage" INTEGER DEFAULT 0,
    "Probability" DOUBLE PRECISION DEFAULT 0,
    "ForecastCategory" INTEGER DEFAULT 0,
    -- Dates
    "CloseDate" TIMESTAMP,
    "ExpectedCloseDate" TIMESTAMP,
    "ActualCloseDate" TIMESTAMP,
    "NextStepDate" TIMESTAMP,
    "LastActivityDate" TIMESTAMP,
    "DaysInCurrentStage" INTEGER,
    "TotalDaysOpen" INTEGER,
    -- Sales Process
    "NextStep" VARCHAR(500),
    "LossReason" VARCHAR(500),
    "WinReason" VARCHAR(500),
    "CompetitorName" VARCHAR(200),
    "CompetitorStrengths" TEXT,
    "CompetitorWeaknesses" TEXT,
    -- Decision Making
    "DecisionMakers" TEXT,
    "BudgetStatus" VARCHAR(50),
    "DecisionProcess" TEXT,
    "PainPoints" TEXT,
    "ProposedSolution" TEXT,
    -- Source & Campaign
    "LeadSource" VARCHAR(100),
    "CampaignId" INTEGER,
    "OriginalLeadId" INTEGER,
    -- Relationships
    "CustomerId" INTEGER NOT NULL,
    "PrimaryContactId" INTEGER,
    "AssignedToUserId" INTEGER,
    "ProductId" INTEGER,
    "QuoteId" INTEGER,
    -- Classification
    "Tags" TEXT,
    "Territory" VARCHAR(100),
    "Region" VARCHAR(100),
    -- Documentation
    "Notes" TEXT,
    "InternalNotes" TEXT,
    "CustomFields" TEXT,
    -- Audit
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "FK_Opportunities_Customer" FOREIGN KEY ("CustomerId") REFERENCES "Customers"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Opportunities_Product" FOREIGN KEY ("ProductId") REFERENCES "Products"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Opportunities_AssignedTo" FOREIGN KEY ("AssignedToUserId") REFERENCES "Users"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Opportunities_Campaign" FOREIGN KEY ("CampaignId") REFERENCES "MarketingCampaigns"("Id") ON DELETE SET NULL
);

-- Campaign Metrics Table
CREATE TABLE IF NOT EXISTS "CampaignMetrics" (
    "Id" SERIAL PRIMARY KEY,
    "CampaignId" INTEGER NOT NULL,
    "MetricName" VARCHAR(100) NOT NULL,
    "MetricValue" DOUBLE PRECISION DEFAULT 0,
    "RecordedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "FK_CampaignMetrics_Campaign" FOREIGN KEY ("CampaignId") REFERENCES "MarketingCampaigns"("Id") ON DELETE CASCADE
);

-- Contacts Table
CREATE TABLE IF NOT EXISTS "Contacts" (
    "Id" SERIAL PRIMARY KEY,
    -- Type & Status
    "ContactType" INTEGER DEFAULT 0,
    "Status" INTEGER DEFAULT 0,
    "LeadStatus" INTEGER,
    -- Personal Information
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "MiddleName" VARCHAR(100),
    "Salutation" VARCHAR(20),
    "Suffix" VARCHAR(20),
    "Nickname" VARCHAR(50),
    "Gender" VARCHAR(20),
    "DateOfBirth" TIMESTAMP,
    -- Contact Information
    "EmailPrimary" VARCHAR(255),
    "EmailSecondary" VARCHAR(255),
    "EmailWork" VARCHAR(255),
    "PhonePrimary" VARCHAR(20),
    "PhoneSecondary" VARCHAR(20),
    "PhoneMobile" VARCHAR(20),
    "PhoneWork" VARCHAR(20),
    "PhoneFax" VARCHAR(20),
    -- Address - Primary
    "Address" VARCHAR(500),
    "Address2" VARCHAR(500),
    "City" VARCHAR(100),
    "State" VARCHAR(100),
    "Country" VARCHAR(100),
    "ZipCode" VARCHAR(20),
    -- Address - Mailing
    "MailingAddress" VARCHAR(500),
    "MailingCity" VARCHAR(100),
    "MailingState" VARCHAR(100),
    "MailingCountry" VARCHAR(100),
    "MailingZipCode" VARCHAR(20),
    -- Professional Information
    "JobTitle" VARCHAR(100),
    "Department" VARCHAR(100),
    "Company" VARCHAR(200),
    "ReportsTo" VARCHAR(100),
    "AssistantName" VARCHAR(100),
    "AssistantPhone" VARCHAR(20),
    -- Lead Information
    "LeadScore" INTEGER DEFAULT 0,
    "LeadSource" VARCHAR(100),
    "LeadRating" VARCHAR(20),
    "ConvertedDate" TIMESTAMP,
    "ConvertedToCustomerId" INTEGER,
    -- Communication Preferences
    "PreferredContactMethod" INTEGER DEFAULT 5,
    "PreferredLanguage" VARCHAR(10) DEFAULT 'en',
    "DoNotCall" BOOLEAN DEFAULT FALSE,
    "DoNotEmail" BOOLEAN DEFAULT FALSE,
    "DoNotMail" BOOLEAN DEFAULT FALSE,
    "DoNotSMS" BOOLEAN DEFAULT FALSE,
    -- Activity Tracking
    "LastContactDate" TIMESTAMP,
    "LastActivityDate" TIMESTAMP,
    "NextFollowUpDate" TIMESTAMP,
    "TotalInteractions" INTEGER DEFAULT 0,
    -- Relationships
    "CustomerId" INTEGER,
    "AccountId" INTEGER,
    "OwnerUserId" INTEGER,
    "CreatedByUserId" INTEGER,
    -- Classification
    "Tags" TEXT,
    "Categories" TEXT,
    -- Notes
    "Notes" TEXT,
    "InternalNotes" TEXT,
    "CustomFields" TEXT,
    -- Audit
    "DateAdded" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "LastModified" TIMESTAMP,
    "ModifiedBy" VARCHAR(100)
);

-- Social Media Links Table
CREATE TABLE IF NOT EXISTS "SocialMediaLinks" (
    "Id" SERIAL PRIMARY KEY,
    "ContactId" INTEGER NOT NULL,
    "Platform" INTEGER DEFAULT 0,
    "Url" VARCHAR(500) NOT NULL,
    "Handle" VARCHAR(100),
    "DateAdded" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_SocialMediaLinks_Contact" FOREIGN KEY ("ContactId") REFERENCES "Contacts"("Id") ON DELETE CASCADE
);

-- Interactions Table
CREATE TABLE IF NOT EXISTS "Interactions" (
    "Id" SERIAL PRIMARY KEY,
    -- Basic Information
    "InteractionType" INTEGER DEFAULT 9,
    "Type" VARCHAR(50) DEFAULT '',
    "Direction" INTEGER DEFAULT 1,
    "Subject" VARCHAR(500) DEFAULT '',
    "Description" TEXT DEFAULT '',
    -- Timing
    "InteractionDate" TIMESTAMP NOT NULL,
    "EndTime" TIMESTAMP,
    "DurationMinutes" INTEGER,
    "ScheduledDate" TIMESTAMP,
    "CompletedDate" TIMESTAMP,
    -- Status & Outcome
    "Outcome" INTEGER DEFAULT 0,
    "Sentiment" INTEGER DEFAULT 2,
    "IsCompleted" BOOLEAN DEFAULT FALSE,
    "IsPrivate" BOOLEAN DEFAULT FALSE,
    "Priority" INTEGER DEFAULT 1,
    -- Communication Details
    "PhoneNumber" VARCHAR(20),
    "EmailAddress" VARCHAR(255),
    "Location" VARCHAR(500),
    "MeetingLink" VARCHAR(500),
    "RecordingUrl" VARCHAR(500),
    -- Email Specific
    "EmailCc" TEXT,
    "EmailBcc" TEXT,
    "EmailOpened" BOOLEAN,
    "EmailOpenedDate" TIMESTAMP,
    "EmailClicked" BOOLEAN,
    "EmailClickedDate" TIMESTAMP,
    -- Relationships
    "CustomerId" INTEGER NOT NULL,
    "ContactId" INTEGER,
    "OpportunityId" INTEGER,
    "CampaignId" INTEGER,
    "AssignedToUserId" INTEGER,
    "CreatedByUserId" INTEGER,
    -- Follow-up
    "FollowUpRequired" BOOLEAN DEFAULT FALSE,
    "FollowUpDate" TIMESTAMP,
    "FollowUpNotes" TEXT,
    -- Classification
    "Tags" TEXT,
    "Category" VARCHAR(100),
    -- Documentation
    "Notes" TEXT,
    "InternalNotes" TEXT,
    "Attachments" TEXT,
    "CustomFields" TEXT,
    -- Audit
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "FK_Interactions_Customer" FOREIGN KEY ("CustomerId") REFERENCES "Customers"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Interactions_Contact" FOREIGN KEY ("ContactId") REFERENCES "Contacts"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Interactions_Opportunity" FOREIGN KEY ("OpportunityId") REFERENCES "Opportunities"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Interactions_Campaign" FOREIGN KEY ("CampaignId") REFERENCES "MarketingCampaigns"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Interactions_AssignedTo" FOREIGN KEY ("AssignedToUserId") REFERENCES "Users"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Interactions_CreatedBy" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users"("Id") ON DELETE SET NULL
);

-- CRM Tasks Table
CREATE TABLE IF NOT EXISTS "CrmTasks" (
    "Id" SERIAL PRIMARY KEY,
    -- Basic Information
    "Subject" VARCHAR(255) NOT NULL,
    "Description" TEXT,
    "TaskType" INTEGER DEFAULT 8,
    "Status" INTEGER DEFAULT 0,
    "Priority" INTEGER DEFAULT 1,
    -- Dates
    "DueDate" TIMESTAMP,
    "StartDate" TIMESTAMP,
    "CompletedDate" TIMESTAMP,
    "ReminderDate" TIMESTAMP,
    "HasReminder" BOOLEAN DEFAULT FALSE,
    -- Progress
    "PercentComplete" INTEGER DEFAULT 0,
    "EstimatedMinutes" INTEGER,
    "ActualMinutes" INTEGER,
    -- Recurrence
    "IsRecurring" BOOLEAN DEFAULT FALSE,
    "RecurrencePattern" TEXT,
    "RecurrenceEndDate" TIMESTAMP,
    "ParentTaskId" INTEGER,
    -- Relationships
    "CustomerId" INTEGER,
    "ContactId" INTEGER,
    "OpportunityId" INTEGER,
    "CampaignId" INTEGER,
    "AssignedToUserId" INTEGER,
    "CreatedByUserId" INTEGER,
    -- Classification
    "Tags" TEXT,
    "Category" VARCHAR(100),
    -- Attachments
    "Attachments" TEXT,
    "CustomFields" TEXT,
    -- Audit
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "FK_CrmTasks_Customer" FOREIGN KEY ("CustomerId") REFERENCES "Customers"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_CrmTasks_Opportunity" FOREIGN KEY ("OpportunityId") REFERENCES "Opportunities"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_CrmTasks_Campaign" FOREIGN KEY ("CampaignId") REFERENCES "MarketingCampaigns"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_CrmTasks_AssignedTo" FOREIGN KEY ("AssignedToUserId") REFERENCES "Users"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_CrmTasks_CreatedBy" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_CrmTasks_Parent" FOREIGN KEY ("ParentTaskId") REFERENCES "CrmTasks"("Id") ON DELETE SET NULL
);

-- Notes Table
CREATE TABLE IF NOT EXISTS "Notes" (
    "Id" SERIAL PRIMARY KEY,
    -- Content
    "Title" VARCHAR(255) NOT NULL,
    "Content" TEXT NOT NULL,
    "Summary" VARCHAR(500),
    -- Classification
    "NoteType" INTEGER DEFAULT 0,
    "Visibility" INTEGER DEFAULT 1,
    "IsPinned" BOOLEAN DEFAULT FALSE,
    "IsImportant" BOOLEAN DEFAULT FALSE,
    -- Relationships
    "CustomerId" INTEGER,
    "ContactId" INTEGER,
    "OpportunityId" INTEGER,
    "CampaignId" INTEGER,
    "ProductId" INTEGER,
    "TaskId" INTEGER,
    "InteractionId" INTEGER,
    -- Authorship
    "CreatedByUserId" INTEGER,
    "LastModifiedByUserId" INTEGER,
    -- Classification
    "Tags" TEXT,
    "Category" VARCHAR(100),
    -- Attachments
    "Attachments" TEXT,
    "MentionedUsers" TEXT,
    "RelatedNotes" TEXT,
    "CustomFields" TEXT,
    -- Audit
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "FK_Notes_Customer" FOREIGN KEY ("CustomerId") REFERENCES "Customers"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Notes_Opportunity" FOREIGN KEY ("OpportunityId") REFERENCES "Opportunities"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Notes_Campaign" FOREIGN KEY ("CampaignId") REFERENCES "MarketingCampaigns"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Notes_Product" FOREIGN KEY ("ProductId") REFERENCES "Products"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Notes_CreatedBy" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Notes_ModifiedBy" FOREIGN KEY ("LastModifiedByUserId") REFERENCES "Users"("Id") ON DELETE SET NULL
);

-- Quotes Table
CREATE TABLE IF NOT EXISTS "Quotes" (
    "Id" SERIAL PRIMARY KEY,
    -- Identification
    "QuoteNumber" VARCHAR(50) NOT NULL UNIQUE,
    "ExternalQuoteId" VARCHAR(100),
    "Version" INTEGER DEFAULT 1,
    -- Basic Information
    "Name" VARCHAR(255) NOT NULL,
    "Description" TEXT,
    "Status" INTEGER DEFAULT 0,
    -- Dates
    "QuoteDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "ExpirationDate" TIMESTAMP,
    "SentDate" TIMESTAMP,
    "ViewedDate" TIMESTAMP,
    "AcceptedDate" TIMESTAMP,
    "RejectedDate" TIMESTAMP,
    -- Pricing
    "Subtotal" DECIMAL(18,2) DEFAULT 0,
    "Discount" DECIMAL(18,2) DEFAULT 0,
    "DiscountPercent" DECIMAL(5,2) DEFAULT 0,
    "DiscountReason" VARCHAR(500),
    "Tax" DECIMAL(18,2) DEFAULT 0,
    "TaxRate" DECIMAL(5,2) DEFAULT 0,
    "ShippingCost" DECIMAL(18,2) DEFAULT 0,
    "Total" DECIMAL(18,2) DEFAULT 0,
    "CurrencyCode" VARCHAR(3) DEFAULT 'USD',
    -- Terms
    "PaymentTerms" VARCHAR(100),
    "DeliveryTerms" VARCHAR(100),
    "TermsAndConditions" TEXT,
    "Warranty" VARCHAR(500),
    "ValidityDays" INTEGER DEFAULT 30,
    -- Line Items
    "LineItems" TEXT,
    -- Billing Address
    "BillingName" VARCHAR(200),
    "BillingAddress" VARCHAR(500),
    "BillingCity" VARCHAR(100),
    "BillingState" VARCHAR(100),
    "BillingZipCode" VARCHAR(20),
    "BillingCountry" VARCHAR(100),
    -- Shipping Address
    "ShippingName" VARCHAR(200),
    "ShippingAddress" VARCHAR(500),
    "ShippingCity" VARCHAR(100),
    "ShippingState" VARCHAR(100),
    "ShippingZipCode" VARCHAR(20),
    "ShippingCountry" VARCHAR(100),
    -- Contact Information
    "ContactName" VARCHAR(200),
    "ContactEmail" VARCHAR(255),
    "ContactPhone" VARCHAR(20),
    -- Relationships
    "CustomerId" INTEGER,
    "ContactId" INTEGER,
    "OpportunityId" INTEGER,
    "AssignedToUserId" INTEGER,
    "CreatedByUserId" INTEGER,
    "ApprovedByUserId" INTEGER,
    "ParentQuoteId" INTEGER,
    -- Approval
    "RequiresApproval" BOOLEAN DEFAULT FALSE,
    "IsApproved" BOOLEAN DEFAULT FALSE,
    "ApprovalDate" TIMESTAMP,
    "ApprovalNotes" TEXT,
    -- Signature
    "IsSigned" BOOLEAN DEFAULT FALSE,
    "SignedDate" TIMESTAMP,
    "SignedBy" VARCHAR(200),
    "SignatureUrl" VARCHAR(500),
    -- Documentation
    "Notes" TEXT,
    "InternalNotes" TEXT,
    "Attachments" TEXT,
    "QuotePdfUrl" VARCHAR(500),
    -- Classification
    "Tags" TEXT,
    "Category" VARCHAR(100),
    "CustomFields" TEXT,
    -- Audit
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "FK_Quotes_Customer" FOREIGN KEY ("CustomerId") REFERENCES "Customers"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Quotes_Opportunity" FOREIGN KEY ("OpportunityId") REFERENCES "Opportunities"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Quotes_AssignedTo" FOREIGN KEY ("AssignedToUserId") REFERENCES "Users"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Quotes_CreatedBy" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Quotes_ApprovedBy" FOREIGN KEY ("ApprovedByUserId") REFERENCES "Users"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Quotes_Parent" FOREIGN KEY ("ParentQuoteId") REFERENCES "Quotes"("Id") ON DELETE SET NULL
);

-- Activities Table
CREATE TABLE IF NOT EXISTS "Activities" (
    "Id" SERIAL PRIMARY KEY,
    -- Activity Information
    "ActivityType" INTEGER DEFAULT 99,
    "Title" VARCHAR(255) NOT NULL,
    "Description" TEXT,
    "Details" TEXT,
    -- Timing
    "ActivityDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "DurationMinutes" INTEGER,
    -- Actor
    "UserId" INTEGER,
    "UserName" VARCHAR(200),
    "UserEmail" VARCHAR(255),
    -- Related Entity
    "EntityType" VARCHAR(50),
    "EntityId" INTEGER,
    "EntityName" VARCHAR(255),
    -- Related Records
    "CustomerId" INTEGER,
    "ContactId" INTEGER,
    "OpportunityId" INTEGER,
    "CampaignId" INTEGER,
    "ProductId" INTEGER,
    -- Metadata
    "IpAddress" VARCHAR(50),
    "UserAgent" VARCHAR(500),
    "DeviceType" VARCHAR(50),
    "Source" VARCHAR(100),
    -- Classification
    "Tags" TEXT,
    "Category" VARCHAR(100),
    "IsSystemGenerated" BOOLEAN DEFAULT FALSE,
    "IsVisible" BOOLEAN DEFAULT TRUE,
    "CustomFields" TEXT,
    -- Audit
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "FK_Activities_User" FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Activities_Customer" FOREIGN KEY ("CustomerId") REFERENCES "Customers"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Activities_Opportunity" FOREIGN KEY ("OpportunityId") REFERENCES "Opportunities"("Id") ON DELETE SET NULL
);

-- Workflows Table
CREATE TABLE IF NOT EXISTS "Workflows" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Description" TEXT DEFAULT '',
    "EntityType" VARCHAR(50) NOT NULL,
    "IsActive" BOOLEAN DEFAULT TRUE,
    "Priority" INTEGER DEFAULT 100,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE
);

-- Workflow Rules Table
CREATE TABLE IF NOT EXISTS "WorkflowRules" (
    "Id" SERIAL PRIMARY KEY,
    "WorkflowId" INTEGER NOT NULL,
    "Name" VARCHAR(100) NOT NULL,
    "Description" TEXT DEFAULT '',
    "ConditionLogic" VARCHAR(10) DEFAULT 'AND',
    "ActionType" VARCHAR(50) NOT NULL,
    "ActionConfig" TEXT,
    "TargetUserGroupId" INTEGER,
    "IsActive" BOOLEAN DEFAULT TRUE,
    "Priority" INTEGER DEFAULT 100,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "FK_WorkflowRules_Workflow" FOREIGN KEY ("WorkflowId") REFERENCES "Workflows"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_WorkflowRules_TargetGroup" FOREIGN KEY ("TargetUserGroupId") REFERENCES "UserGroups"("Id") ON DELETE SET NULL
);

-- Workflow Rule Conditions Table
CREATE TABLE IF NOT EXISTS "WorkflowRuleConditions" (
    "Id" SERIAL PRIMARY KEY,
    "WorkflowRuleId" INTEGER NOT NULL,
    "FieldName" VARCHAR(100) NOT NULL,
    "Operator" VARCHAR(20) NOT NULL,
    "Value" TEXT,
    "ValueType" VARCHAR(50) DEFAULT 'String',
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "FK_WorkflowRuleConditions_Rule" FOREIGN KEY ("WorkflowRuleId") REFERENCES "WorkflowRules"("Id") ON DELETE CASCADE
);

-- Workflow Executions Table
CREATE TABLE IF NOT EXISTS "WorkflowExecutions" (
    "Id" SERIAL PRIMARY KEY,
    "WorkflowId" INTEGER NOT NULL,
    "WorkflowRuleId" INTEGER,
    "EntityType" VARCHAR(50) NOT NULL,
    "EntityId" INTEGER NOT NULL,
    "Status" VARCHAR(20) DEFAULT 'Success',
    "Message" TEXT,
    "SourceUserGroupId" INTEGER,
    "TargetUserGroupId" INTEGER,
    "ExecutedByUserId" INTEGER,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    CONSTRAINT "FK_WorkflowExecutions_Workflow" FOREIGN KEY ("WorkflowId") REFERENCES "Workflows"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_WorkflowExecutions_Rule" FOREIGN KEY ("WorkflowRuleId") REFERENCES "WorkflowRules"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_WorkflowExecutions_SourceGroup" FOREIGN KEY ("SourceUserGroupId") REFERENCES "UserGroups"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_WorkflowExecutions_TargetGroup" FOREIGN KEY ("TargetUserGroupId") REFERENCES "UserGroups"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_WorkflowExecutions_User" FOREIGN KEY ("ExecutedByUserId") REFERENCES "Users"("Id") ON DELETE SET NULL
);

-- ============================================
-- Indexes for Performance
-- ============================================

CREATE INDEX IF NOT EXISTS idx_customers_email ON "Customers"("Email");
CREATE INDEX IF NOT EXISTS idx_customers_company ON "Customers"("Company");
CREATE INDEX IF NOT EXISTS idx_customers_lifecycle ON "Customers"("LifecycleStage");
CREATE INDEX IF NOT EXISTS idx_customers_assigned ON "Customers"("AssignedToUserId");

CREATE INDEX IF NOT EXISTS idx_products_sku ON "Products"("SKU");
CREATE INDEX IF NOT EXISTS idx_products_category ON "Products"("Category");
CREATE INDEX IF NOT EXISTS idx_products_status ON "Products"("Status");

CREATE INDEX IF NOT EXISTS idx_opportunities_customer ON "Opportunities"("CustomerId");
CREATE INDEX IF NOT EXISTS idx_opportunities_stage ON "Opportunities"("Stage");
CREATE INDEX IF NOT EXISTS idx_opportunities_close_date ON "Opportunities"("CloseDate");
CREATE INDEX IF NOT EXISTS idx_opportunities_assigned ON "Opportunities"("AssignedToUserId");

CREATE INDEX IF NOT EXISTS idx_campaigns_status ON "MarketingCampaigns"("Status");
CREATE INDEX IF NOT EXISTS idx_campaigns_type ON "MarketingCampaigns"("CampaignType");
CREATE INDEX IF NOT EXISTS idx_campaigns_dates ON "MarketingCampaigns"("StartDate", "EndDate");

CREATE INDEX IF NOT EXISTS idx_interactions_customer ON "Interactions"("CustomerId");
CREATE INDEX IF NOT EXISTS idx_interactions_date ON "Interactions"("InteractionDate");
CREATE INDEX IF NOT EXISTS idx_interactions_type ON "Interactions"("InteractionType");

CREATE INDEX IF NOT EXISTS idx_contacts_type ON "Contacts"("ContactType");
CREATE INDEX IF NOT EXISTS idx_contacts_company ON "Contacts"("Company");
CREATE INDEX IF NOT EXISTS idx_contacts_email ON "Contacts"("EmailPrimary");

CREATE INDEX IF NOT EXISTS idx_tasks_due_date ON "CrmTasks"("DueDate");
CREATE INDEX IF NOT EXISTS idx_tasks_status ON "CrmTasks"("Status");
CREATE INDEX IF NOT EXISTS idx_tasks_assigned ON "CrmTasks"("AssignedToUserId");

CREATE INDEX IF NOT EXISTS idx_notes_pinned ON "Notes"("IsPinned");
CREATE INDEX IF NOT EXISTS idx_notes_customer ON "Notes"("CustomerId");

CREATE INDEX IF NOT EXISTS idx_quotes_number ON "Quotes"("QuoteNumber");
CREATE INDEX IF NOT EXISTS idx_quotes_status ON "Quotes"("Status");
CREATE INDEX IF NOT EXISTS idx_quotes_customer ON "Quotes"("CustomerId");

CREATE INDEX IF NOT EXISTS idx_activities_date ON "Activities"("ActivityDate");
CREATE INDEX IF NOT EXISTS idx_activities_type ON "Activities"("ActivityType");
CREATE INDEX IF NOT EXISTS idx_activities_entity ON "Activities"("EntityType", "EntityId");

CREATE INDEX IF NOT EXISTS idx_users_username ON "Users"("Username");
CREATE INDEX IF NOT EXISTS idx_users_email ON "Users"("Email");
CREATE INDEX IF NOT EXISTS idx_users_department ON "Users"("DepartmentId");

CREATE INDEX IF NOT EXISTS idx_workflow_executions_workflow ON "WorkflowExecutions"("WorkflowId", "CreatedAt");
CREATE INDEX IF NOT EXISTS idx_workflow_executions_entity ON "WorkflowExecutions"("EntityType", "EntityId");

-- ============================================
-- Schema complete
-- ============================================
