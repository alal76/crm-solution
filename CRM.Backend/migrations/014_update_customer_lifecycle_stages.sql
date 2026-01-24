-- Migration: 014_update_customer_lifecycle_stages.sql
-- Description: Update customer lifecycle stages with new values
-- Flow: Other (default) → Lead → Opportunity → Customer → CustomerAtRisk → Churned → (Win-back) → Lead

-- First, update the LookupCategory description
UPDATE LookupCategories 
SET Description = 'Customer lifecycle stages: Other → Lead → Opportunity → Customer → CustomerAtRisk → Churned → WinBack'
WHERE Name = 'LifecycleStage';

-- Get the LifecycleStage category ID
SET @lifecycle_cat_id = (SELECT Id FROM LookupCategories WHERE Name = 'LifecycleStage');

-- Delete existing lifecycle items to replace with new ones
DELETE FROM LookupItems WHERE LookupCategoryId = @lifecycle_cat_id;

-- Insert new lifecycle stages with proper ordering
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Description, SortOrder, IsActive, CreatedAt)
VALUES 
    (@lifecycle_cat_id, '0', 'Other', 'Initial default value for new customers', 0, TRUE, NOW(6)),
    (@lifecycle_cat_id, '1', 'Lead', 'A potential customer showing interest', 1, TRUE, NOW(6)),
    (@lifecycle_cat_id, '2', 'Opportunity', 'A qualified lead with an active sales opportunity', 2, TRUE, NOW(6)),
    (@lifecycle_cat_id, '3', 'Customer', 'An active paying customer', 3, TRUE, NOW(6)),
    (@lifecycle_cat_id, '4', 'CustomerAtRisk', 'A customer at risk of churning', 4, TRUE, NOW(6)),
    (@lifecycle_cat_id, '5', 'Churned', 'A former customer who has stopped doing business', 5, TRUE, NOW(6)),
    (@lifecycle_cat_id, '6', 'WinBack', 'A churned customer being re-engaged (transitions back to Lead)', 6, TRUE, NOW(6));

-- Update any existing customers with old enum values
-- Old values: Lead=0, Prospect=1, Opportunity=2, Customer=3, Churned=4, Reactivated=5
-- New values: Other=0, Lead=1, Opportunity=2, Customer=3, CustomerAtRisk=4, Churned=5, WinBack=6

-- Map old values to new values:
-- Old Lead (0) stays as Lead but needs to shift to 1
-- Old Prospect (1) → Lead (1)
-- Old Opportunity (2) → Opportunity (2) [same]
-- Old Customer (3) → Customer (3) [same]
-- Old Churned (4) → Churned (5)
-- Old Reactivated (5) → WinBack (6)

-- First, update Reactivated (5) to WinBack (6)
UPDATE Customers SET LifecycleStage = 6 WHERE LifecycleStage = 5;

-- Update Churned (4) to Churned (5)
UPDATE Customers SET LifecycleStage = 5 WHERE LifecycleStage = 4;

-- Update old Lead (0) to new Lead (1) - must be done carefully
-- Temporarily use a high number to avoid conflicts
UPDATE Customers SET LifecycleStage = 100 WHERE LifecycleStage = 0;
UPDATE Customers SET LifecycleStage = 1 WHERE LifecycleStage = 100;

-- Update old Prospect (1) to Lead (1) - already handled by above if needed
-- If there are any Prospects left, map them to Lead
UPDATE Customers SET LifecycleStage = 1 WHERE LifecycleStage = 99; -- Just in case

-- Verify the update
SELECT LifecycleStage, COUNT(*) as Count 
FROM Customers 
GROUP BY LifecycleStage 
ORDER BY LifecycleStage;
