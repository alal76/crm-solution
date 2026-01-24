-- Migration: 014_update_customer_lifecycle_stages_v2.sql
-- Description: Update customer lifecycle stages with new values
-- Flow: Other (default) → Lead → Opportunity → Customer → CustomerAtRisk → Churned → WinBack

-- First, update the LookupCategory description
UPDATE LookupCategories 
SET Description = 'Customer lifecycle stages: Other, Lead, Opportunity, Customer, CustomerAtRisk, Churned, WinBack'
WHERE Name = 'LifecycleStage';

-- Get the LifecycleStage category ID
SET @lifecycle_cat_id = (SELECT Id FROM LookupCategories WHERE Name = 'LifecycleStage');

-- Delete existing lifecycle items to replace with new ones
DELETE FROM LookupItems WHERE LookupCategoryId = @lifecycle_cat_id;

-- Insert new lifecycle stages with proper ordering (using Meta column for description, including IsDeleted)
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, IsDeleted, CreatedAt)
VALUES 
    (@lifecycle_cat_id, '0', 'Other', '{"description": "Initial default value for new customers"}', 0, TRUE, FALSE, NOW(6)),
    (@lifecycle_cat_id, '1', 'Lead', '{"description": "A potential customer showing interest"}', 1, TRUE, FALSE, NOW(6)),
    (@lifecycle_cat_id, '2', 'Opportunity', '{"description": "A qualified lead with an active sales opportunity"}', 2, TRUE, FALSE, NOW(6)),
    (@lifecycle_cat_id, '3', 'Customer', '{"description": "An active paying customer"}', 3, TRUE, FALSE, NOW(6)),
    (@lifecycle_cat_id, '4', 'CustomerAtRisk', '{"description": "A customer at risk of churning"}', 4, TRUE, FALSE, NOW(6)),
    (@lifecycle_cat_id, '5', 'Churned', '{"description": "A former customer who has stopped doing business"}', 5, TRUE, FALSE, NOW(6)),
    (@lifecycle_cat_id, '6', 'WinBack', '{"description": "A churned customer being re-engaged"}', 6, TRUE, FALSE, NOW(6));

-- Update any existing customers with old enum values
-- Old values: Lead=0, Prospect=1, Opportunity=2, Customer=3, Churned=4, Reactivated=5
-- New values: Other=0, Lead=1, Opportunity=2, Customer=3, CustomerAtRisk=4, Churned=5, WinBack=6

-- Map old values to new values (in reverse order to avoid conflicts):
-- First, update Reactivated (5) to WinBack (6)
UPDATE Customers SET LifecycleStage = 6 WHERE LifecycleStage = 5;

-- Update Churned (4) to Churned (5)
UPDATE Customers SET LifecycleStage = 5 WHERE LifecycleStage = 4;

-- Update old Lead (0) to new Lead (1) - use temp value to avoid conflicts
UPDATE Customers SET LifecycleStage = 100 WHERE LifecycleStage = 0;
UPDATE Customers SET LifecycleStage = 1 WHERE LifecycleStage = 100;

-- Display the new lifecycle stages
SELECT `Key`, Value, Meta FROM LookupItems WHERE LookupCategoryId = @lifecycle_cat_id ORDER BY SortOrder;

-- Display customer count per lifecycle stage
SELECT LifecycleStage, COUNT(*) as Count FROM Customers GROUP BY LifecycleStage ORDER BY LifecycleStage;
