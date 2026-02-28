-- =============================================================================
-- SYS-009: ServiceRequest Category & FK Migration Fix
-- =============================================================================
-- Purpose: Create missing ServiceRequestStatus and ServiceRequestPriority
--          LookupCategories and their items, then migrate FK values
-- Applied to: crm_db (192.168.0.9) - 2026-02-27
-- =============================================================================

-- =============================================================================
-- CREATE ServiceRequestStatus CATEGORY (if not exists)
-- =============================================================================

SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;
SET time_zone = '+00:00';

SELECT 'Creating ServiceRequestStatus category...' AS Step;

INSERT IGNORE INTO LookupCategories
    (Name, Description, EntityType, PropertyName, IsSystemManaged, AllowCustomValues, CreatedAt, UpdatedAt, IsDeleted)
VALUES
    ('ServiceRequestStatus', 'Status values for Service Requests', 'ServiceRequest', 'Status', 1, 0, NOW(), NOW(), 0);

-- COALESCE: if INSERT IGNORE matched a duplicate, LAST_INSERT_ID() returns 0;
-- fall back to a SELECT to retrieve the existing row's Id safely.
SET @status_cat_id = COALESCE(
    NULLIF(LAST_INSERT_ID(), 0),
    (SELECT Id FROM LookupCategories WHERE Name = 'ServiceRequestStatus' LIMIT 1)
);

-- Insert Status items
INSERT IGNORE INTO LookupItems
    (LookupCategoryId, `Key`, Value, Description, SortOrder, IsActive, IsDefault, Metadata, CreatedAt, UpdatedAt, IsDeleted)
VALUES
    (@status_cat_id, 'NEW',         'New',         'Newly created service request',                        1, 1, 1, NULL, NOW(), NOW(), 0),
    (@status_cat_id, 'OPEN',        'Open',        'Service request is open and being worked on',          2, 1, 0, NULL, NOW(), NOW(), 0),
    (@status_cat_id, 'IN_PROGRESS', 'In Progress', 'Service request is actively being worked on',          3, 1, 0, NULL, NOW(), NOW(), 0),
    (@status_cat_id, 'PENDING',     'Pending',     'Waiting for customer or third-party response',         4, 1, 0, NULL, NOW(), NOW(), 0),
    (@status_cat_id, 'ON_HOLD',     'On Hold',     'Service request is temporarily on hold',               5, 1, 0, NULL, NOW(), NOW(), 0),
    (@status_cat_id, 'RESOLVED',    'Resolved',    'Issue has been resolved, pending confirmation',        6, 1, 0, NULL, NOW(), NOW(), 0),
    (@status_cat_id, 'CLOSED',      'Closed',      'Service request is fully closed and verified',         7, 1, 0, NULL, NOW(), NOW(), 0),
    (@status_cat_id, 'CANCELLED',   'Cancelled',   'Service request was cancelled',                        8, 1, 0, NULL, NOW(), NOW(), 0);

-- =============================================================================
-- CREATE ServiceRequestPriority CATEGORY (if not exists)
-- =============================================================================
SELECT 'Creating ServiceRequestPriority category...' AS Step;

INSERT IGNORE INTO LookupCategories
    (Name, Description, EntityType, PropertyName, IsSystemManaged, AllowCustomValues, CreatedAt, UpdatedAt, IsDeleted)
VALUES
    ('ServiceRequestPriority', 'Priority levels for Service Requests', 'ServiceRequest', 'Priority', 1, 0, NOW(), NOW(), 0);

SET @priority_cat_id = COALESCE(
    NULLIF(LAST_INSERT_ID(), 0),
    (SELECT Id FROM LookupCategories WHERE Name = 'ServiceRequestPriority' LIMIT 1)
);

-- Insert Priority items (with SLA metadata)
INSERT IGNORE INTO LookupItems
    (LookupCategoryId, `Key`, Value, Description, SortOrder, IsActive, IsDefault, Metadata, CreatedAt, UpdatedAt, IsDeleted)
VALUES
    (@priority_cat_id, 'LOW',      'Low',      'Low priority - response within 5 business days',        1, 1, 0, '{"slaHours":120,"color":"#4CAF50"}', NOW(), NOW(), 0),
    (@priority_cat_id, 'MEDIUM',   'Medium',   'Medium priority - response within 2 business days',     2, 1, 1, '{"slaHours":48,"color":"#FF9800"}',  NOW(), NOW(), 0),
    (@priority_cat_id, 'HIGH',     'High',     'High priority - response within 4 hours',               3, 1, 0, '{"slaHours":4,"color":"#F44336"}',   NOW(), NOW(), 0),
    (@priority_cat_id, 'CRITICAL', 'Critical', 'Critical priority - immediate response required',        4, 1, 0, '{"slaHours":1,"color":"#9C27B0"}',   NOW(), NOW(), 0);

-- =============================================================================
-- MIGRATE ServiceRequests.StatusId
-- Maps ServiceRequest.Status (0=NEW, 1=OPEN, 2=IN_PROGRESS, 3=PENDING, 4=ON_HOLD, 5=RESOLVED, 6=CLOSED, 7=CANCELLED)
-- =============================================================================
SELECT 'MIGRATING: ServiceRequests.StatusId' AS Step;

-- Use @status_cat_id resolved above to avoid a per-row LookupCategories scan.
-- CASE in the JOIN ON clause means unmatched ordinals produce no join hit
-- (NULL = anything is UNKNOWN → row excluded), so no explicit WHERE guard needed.
START TRANSACTION;

UPDATE ServiceRequests sr
INNER JOIN LookupItems li
    ON  li.LookupCategoryId = @status_cat_id
    AND li.`Key` = CASE sr.`Status`
                       WHEN 0 THEN 'NEW'
                       WHEN 1 THEN 'OPEN'
                       WHEN 2 THEN 'IN_PROGRESS'
                       WHEN 3 THEN 'PENDING'
                       WHEN 4 THEN 'ON_HOLD'
                       WHEN 5 THEN 'RESOLVED'
                       WHEN 6 THEN 'CLOSED'
                       WHEN 7 THEN 'CANCELLED'
                       ELSE NULL
                   END
SET sr.StatusId = li.Id;

-- =============================================================================
-- MIGRATE ServiceRequests.PriorityId
-- Maps ServiceRequest.Priority (0=LOW, 1=MEDIUM, 2=HIGH, 3=CRITICAL)
-- =============================================================================
SELECT 'MIGRATING: ServiceRequests.PriorityId' AS Step;

UPDATE ServiceRequests sr
INNER JOIN LookupItems li
    ON  li.LookupCategoryId = @priority_cat_id
    AND li.`Key` = CASE sr.`Priority`
                       WHEN 0 THEN 'LOW'
                       WHEN 1 THEN 'MEDIUM'
                       WHEN 2 THEN 'HIGH'
                       WHEN 3 THEN 'CRITICAL'
                       ELSE NULL
                   END
SET sr.PriorityId = li.Id;

COMMIT;

-- =============================================================================
-- POST-CHECK
-- =============================================================================
SELECT 'RESULT: ServiceRequest NULL check' AS Step;
SELECT
    COUNT(CASE WHEN StatusId IS NULL THEN 1 END) AS StatusNulls,
    COUNT(CASE WHEN PriorityId IS NULL THEN 1 END) AS PriorityNulls,
    COUNT(*) AS Total
FROM ServiceRequests;

SELECT 'RESULT: ServiceRequest Status Distribution' AS Step;
SELECT li.Value AS Status, COUNT(*) AS Count
FROM ServiceRequests sr
INNER JOIN LookupItems li ON sr.StatusId = li.Id
GROUP BY li.Value ORDER BY Count DESC;

SELECT 'RESULT: ServiceRequest Priority Distribution' AS Step;
SELECT li.Value AS Priority, COUNT(*) AS Count
FROM ServiceRequests sr
INNER JOIN LookupItems li ON sr.PriorityId = li.Id
GROUP BY li.Value ORDER BY Count DESC;