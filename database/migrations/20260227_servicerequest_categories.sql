-- ============================================================================
-- Migration: ServiceRequest Status/Priority Categories
-- Date: 2026-02-27
-- Description: Add ServiceRequestStatus and ServiceRequestPriority categories
--              to support FK migration (these were missing from seed data)
-- Status: ✅ Applied to dev server (192.168.0.9:3306/crm_db) on 2026-02-27
-- ============================================================================

-- ============================================================================
-- Create ServiceRequestStatus Category and Items
-- ============================================================================

INSERT INTO LookupCategories (Name, Description, IsActive, CreatedAt, IsDeleted, EntityType, PropertyName, IsSystemManaged, AllowCustomValues)
VALUES ('ServiceRequestStatus', 'Service Request lifecycle statuses', 1, NOW(), 0, 'ServiceRequest', 'Status', 1, 1)
ON DUPLICATE KEY UPDATE 
    Description = VALUES(Description),
    EntityType = VALUES(EntityType),
    PropertyName = VALUES(PropertyName);

-- Get the category ID
SET @srStatusCatId = (SELECT Id FROM LookupCategories WHERE Name = 'ServiceRequestStatus');

-- Create ServiceRequestStatus items
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted, IsDefault, IsSystemValue, Color) VALUES
(@srStatusCatId, 'NEW', 'New', '{"default":true}', 1, 1, NOW(), 0, 1, 1, '#9e9e9e'),
(@srStatusCatId, 'OPEN', 'Open', NULL, 2, 1, NOW(), 0, 0, 1, '#2196f3'),
(@srStatusCatId, 'IN_PROGRESS', 'In Progress', NULL, 3, 1, NOW(), 0, 0, 1, '#03a9f4'),
(@srStatusCatId, 'PENDING', 'Pending', NULL, 4, 1, NOW(), 0, 0, 1, '#ff9800'),
(@srStatusCatId, 'ON_HOLD', 'On Hold', NULL, 5, 1, NOW(), 0, 0, 1, '#ff5722'),
(@srStatusCatId, 'RESOLVED', 'Resolved', NULL, 6, 1, NOW(), 0, 0, 1, '#8bc34a'),
(@srStatusCatId, 'CLOSED', 'Closed', NULL, 7, 1, NOW(), 0, 0, 1, '#4caf50'),
(@srStatusCatId, 'CANCELLED', 'Cancelled', NULL, 8, 1, NOW(), 0, 0, 1, '#f44336')
ON DUPLICATE KEY UPDATE 
    Value = VALUES(Value),
    Color = VALUES(Color);

-- ============================================================================
-- Create ServiceRequestPriority Category and Items
-- ============================================================================

INSERT INTO LookupCategories (Name, Description, IsActive, CreatedAt, IsDeleted, EntityType, PropertyName, IsSystemManaged, AllowCustomValues)
VALUES ('ServiceRequestPriority', 'Service Request priority levels', 1, NOW(), 0, 'ServiceRequest', 'Priority', 1, 1)
ON DUPLICATE KEY UPDATE 
    Description = VALUES(Description),
    EntityType = VALUES(EntityType),
    PropertyName = VALUES(PropertyName);

-- Get the category ID
SET @srPriorityCatId = (SELECT Id FROM LookupCategories WHERE Name = 'ServiceRequestPriority');

-- Create ServiceRequestPriority items
INSERT INTO LookupItems (LookupCategoryId, `Key`, Value, Meta, SortOrder, IsActive, CreatedAt, IsDeleted, IsDefault, IsSystemValue, Color) VALUES
(@srPriorityCatId, 'LOW', 'Low', NULL, 1, 1, NOW(), 0, 0, 1, '#4caf50'),
(@srPriorityCatId, 'MEDIUM', 'Medium', '{"default":true,"slaHours":48}', 2, 1, NOW(), 0, 1, 1, '#ffeb3b'),
(@srPriorityCatId, 'HIGH', 'High', '{"slaHours":24}', 3, 1, NOW(), 0, 0, 1, '#ff9800'),
(@srPriorityCatId, 'CRITICAL', 'Critical', '{"slaHours":4}', 4, 1, NOW(), 0, 0, 1, '#f44336')
ON DUPLICATE KEY UPDATE 
    Value = VALUES(Value),
    Color = VALUES(Color),
    Meta = VALUES(Meta);

-- Verification
SELECT 'ServiceRequestStatus Category' AS Info;
SELECT lc.Id, lc.Name, lc.EntityType, lc.PropertyName, COUNT(li.Id) as ItemCount
FROM LookupCategories lc
LEFT JOIN LookupItems li ON li.LookupCategoryId = lc.Id
WHERE lc.Name = 'ServiceRequestStatus'
GROUP BY lc.Id, lc.Name, lc.EntityType, lc.PropertyName;

SELECT 'ServiceRequestPriority Category' AS Info;
SELECT lc.Id, lc.Name, lc.EntityType, lc.PropertyName, COUNT(li.Id) as ItemCount
FROM LookupCategories lc
LEFT JOIN LookupItems li ON li.LookupCategoryId = lc.Id
WHERE lc.Name = 'ServiceRequestPriority'
GROUP BY lc.Id, lc.Name, lc.EntityType, lc.PropertyName;

SELECT 'ServiceRequestStatus Items' AS Info;
SELECT li.Id, li.`Key`, li.Value, li.Color, li.IsDefault, li.IsSystemValue
FROM LookupItems li
INNER JOIN LookupCategories lc ON li.LookupCategoryId = lc.Id
WHERE lc.Name = 'ServiceRequestStatus'
ORDER BY li.SortOrder;

SELECT 'ServiceRequestPriority Items' AS Info;
SELECT li.Id, li.`Key`, li.Value, li.Color, li.IsDefault, li.IsSystemValue
FROM LookupItems li
INNER JOIN LookupCategories lc ON li.LookupCategoryId = lc.Id
WHERE lc.Name = 'ServiceRequestPriority'
ORDER BY li.SortOrder;
