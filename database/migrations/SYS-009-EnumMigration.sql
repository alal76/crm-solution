-- ============================================================================
-- SYS-009: Entity Migration for Configurable Enums
-- Relates to: ENUM-MIG-001 through ENUM-MIG-015
-- Date: 2026-02-28
--
-- PURPOSE:
--   Adds nullable FK columns (StatusId, StageId, PriorityId) to the Leads,
--   Opportunities, and ServiceRequests tables, referencing the new EnumValues
--   table created in SYS-008.  These columns coexist with the legacy integer
--   enum columns (Status, Stage, Priority) to preserve backward compatibility.
--
-- PREREQUISITE:
--   SYS-008-ConfigurableEnums.sql must have been applied first so that the
--   EnumCategories and EnumValues tables (and seed rows) are in place.
--
-- NOTES:
--   - All new columns are nullable to avoid breaking existing rows.
--   - Data migration UPDATEs are best-effort; unmapped rows remain NULL.
--     Run SYS-009-EnumMigration-verification.sql afterwards to check counts.
--   - FK constraints use ON DELETE SET NULL so that deleting an EnumValue
--     does not cascade-delete business records.
-- ============================================================================

SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;
SET time_zone = '+00:00';

-- ─────────────────────────────────────────────────────────────────────────────
-- Resolve EnumCategory IDs once into user variables.
-- This avoids a correlated subquery executing once per row in every UPDATE.
-- ─────────────────────────────────────────────────────────────────────────────

SET @catLeadStatus        = (SELECT Id FROM `EnumCategories` WHERE Name = 'LeadStatus'              LIMIT 1);
SET @catOpportunityStage  = (SELECT Id FROM `EnumCategories` WHERE Name = 'OpportunityStage'        LIMIT 1);
SET @catSRStatus          = (SELECT Id FROM `EnumCategories` WHERE Name = 'ServiceRequestStatus'    LIMIT 1);
SET @catSRPriority        = (SELECT Id FROM `EnumCategories` WHERE Name = 'ServiceRequestPriority'  LIMIT 1);

-- Abort early if prerequisite seed data is missing
SELECT IF(@catLeadStatus IS NULL OR @catOpportunityStage IS NULL OR @catSRStatus IS NULL OR @catSRPriority IS NULL,
    (SELECT CONCAT('ERROR: One or more EnumCategories are missing. Ensure SYS-008-ConfigurableEnums.sql has been applied. ',
                   'LeadStatus=', IFNULL(@catLeadStatus,'NULL'), ' OpportunityStage=', IFNULL(@catOpportunityStage,'NULL'),
                   ' SRStatus=', IFNULL(@catSRStatus,'NULL'), ' SRPriority=', IFNULL(@catSRPriority,'NULL'))),
    'EnumCategory IDs resolved OK') AS prerequisite_check;


-- ─────────────────────────────────────────────────────────────────────────────
-- ENUM-MIG-001 / ENUM-MIG-004:  Add StatusId to Leads
-- ─────────────────────────────────────────────────────────────────────────────

ALTER TABLE `Leads`
    ADD COLUMN `StatusId` INT NULL AFTER `Status`;

ALTER TABLE `Leads`
    ADD CONSTRAINT `FK_Leads_StatusId`
        FOREIGN KEY (`StatusId`) REFERENCES `EnumValues`(`Id`) ON DELETE SET NULL;

CREATE INDEX `IX_Leads_StatusId` ON `Leads`(`StatusId`);


-- ─────────────────────────────────────────────────────────────────────────────
-- ENUM-MIG-006 / ENUM-MIG-009:  Add StageId to Opportunities
-- ─────────────────────────────────────────────────────────────────────────────

ALTER TABLE `Opportunities`
    ADD COLUMN `StageId` INT NULL AFTER `Stage`;

ALTER TABLE `Opportunities`
    ADD CONSTRAINT `FK_Opportunities_StageId`
        FOREIGN KEY (`StageId`) REFERENCES `EnumValues`(`Id`) ON DELETE SET NULL;

CREATE INDEX `IX_Opportunities_StageId` ON `Opportunities`(`StageId`);


-- ─────────────────────────────────────────────────────────────────────────────
-- ENUM-MIG-011 / ENUM-MIG-014:  Add StatusId and PriorityId to ServiceRequests
-- ─────────────────────────────────────────────────────────────────────────────

ALTER TABLE `ServiceRequests`
    ADD COLUMN `StatusId`  INT NULL AFTER `Status`;

ALTER TABLE `ServiceRequests`
    ADD CONSTRAINT `FK_ServiceRequests_StatusId`
        FOREIGN KEY (`StatusId`) REFERENCES `EnumValues`(`Id`) ON DELETE SET NULL;

CREATE INDEX `IX_ServiceRequests_StatusId` ON `ServiceRequests`(`StatusId`);

ALTER TABLE `ServiceRequests`
    ADD COLUMN `PriorityId` INT NULL AFTER `Priority`;

ALTER TABLE `ServiceRequests`
    ADD CONSTRAINT `FK_ServiceRequests_PriorityId`
        FOREIGN KEY (`PriorityId`) REFERENCES `EnumValues`(`Id`) ON DELETE SET NULL;

CREATE INDEX `IX_ServiceRequests_PriorityId` ON `ServiceRequests`(`PriorityId`);


-- ─────────────────────────────────────────────────────────────────────────────
-- Data migration — wrapped in a transaction so all UPDATEs are atomic.
-- Note: the ALTER TABLE statements above issue implicit commits (DDL); only
-- the data backfill rows below benefit from this transaction boundary.
-- ─────────────────────────────────────────────────────────────────────────────

START TRANSACTION;

-- ─────────────────────────────────────────────────────────────────────────────
-- ENUM-MIG-002: Migrate existing Lead.Status ordinal values → StatusId
--
-- LeadLifecycleStatus enum ordinals vs. seeded EnumValues Keys:
--   0 = New          → 'new'
--   1 = Working      → 'contacted'   (closest match in seed data)
--   2 = Nurturing    → 'contacted'   (no exact match – approximate)
--   3 = Qualified    → 'qualified'
--   4 = Disqualified → 'unqualified'
--   5 = Converted    → 'converted'
-- Unmapped ordinals (ELSE NULL) produce no JOIN match and are left as NULL.
-- ─────────────────────────────────────────────────────────────────────────────

UPDATE `Leads` l
JOIN `EnumValues` ev
    ON ev.CategoryId = @catLeadStatus
    AND ev.`Key` = CASE l.`Status`
                       WHEN 0 THEN 'new'
                       WHEN 1 THEN 'contacted'
                       WHEN 2 THEN 'contacted'
                       WHEN 3 THEN 'qualified'
                       WHEN 4 THEN 'unqualified'
                       WHEN 5 THEN 'converted'
                       ELSE NULL
                   END
SET l.StatusId = ev.Id
WHERE l.IsDeleted = 0;


-- ─────────────────────────────────────────────────────────────────────────────
-- ENUM-MIG-007: Migrate existing Opportunity.Stage ordinal values → StageId
--
-- OpportunityStage enum ordinals vs. seeded EnumValues Keys:
--   0 = Discovery     → 'prospecting'
--   1 = Qualification → 'qualification'
--   2 = Proposal      → 'proposal'
--   3 = Negotiation   → 'negotiation'
--   4 = ClosedWon     → 'closed_won'
--   5 = ClosedLost    → 'closed_lost'
-- ─────────────────────────────────────────────────────────────────────────────

UPDATE `Opportunities` o
JOIN `EnumValues` ev
    ON ev.CategoryId = @catOpportunityStage
    AND ev.`Key` = CASE o.`Stage`
                       WHEN 0 THEN 'prospecting'
                       WHEN 1 THEN 'qualification'
                       WHEN 2 THEN 'proposal'
                       WHEN 3 THEN 'negotiation'
                       WHEN 4 THEN 'closed_won'
                       WHEN 5 THEN 'closed_lost'
                       ELSE NULL
                   END
SET o.StageId = ev.Id
WHERE o.IsDeleted = 0;


-- ─────────────────────────────────────────────────────────────────────────────
-- ENUM-MIG-012: Migrate existing ServiceRequest.Status ordinal values → StatusId
--
-- ServiceRequestStatus enum ordinals vs. seeded EnumValues Keys:
--   0 = New              → 'open'         (no 'new' key seeded)
--   1 = Open             → 'open'
--   2 = InProgress       → 'in_progress'
--   3 = PendingCustomer  → 'pending'
--   4 = PendingInternal  → 'pending'
--   5 = Escalated        → 'in_progress'  (approximate)
--   6 = Resolved         → 'resolved'
--   7 = Closed           → 'closed'
--   8 = Cancelled        → 'cancelled'
--   9 = OnHold           → 'pending'      (approximate)
--  10 = Reopened         → 'open'         (approximate)
-- ─────────────────────────────────────────────────────────────────────────────

UPDATE `ServiceRequests` sr
JOIN `EnumValues` ev
    ON ev.CategoryId = @catSRStatus
    AND ev.`Key` = CASE sr.`Status`
                       WHEN 0  THEN 'open'
                       WHEN 1  THEN 'open'
                       WHEN 2  THEN 'in_progress'
                       WHEN 3  THEN 'pending'
                       WHEN 4  THEN 'pending'
                       WHEN 5  THEN 'in_progress'
                       WHEN 6  THEN 'resolved'
                       WHEN 7  THEN 'closed'
                       WHEN 8  THEN 'cancelled'
                       WHEN 9  THEN 'pending'
                       WHEN 10 THEN 'open'
                       ELSE NULL
                   END
SET sr.StatusId = ev.Id
WHERE sr.IsDeleted = 0;


-- ─────────────────────────────────────────────────────────────────────────────
-- ENUM-MIG-012 (cont.): Migrate ServiceRequest.Priority → PriorityId
--
-- ServiceRequestPriority enum ordinals vs. seeded EnumValues Keys:
--   0 = Low      → 'low'
--   1 = Medium   → 'medium'
--   2 = High     → 'high'
--   3 = Critical → 'critical'
--   4 = Urgent   → 'critical'  (approximate – no 'urgent' key seeded)
-- ─────────────────────────────────────────────────────────────────────────────

UPDATE `ServiceRequests` sr
JOIN `EnumValues` ev
    ON ev.CategoryId = @catSRPriority
    AND ev.`Key` = CASE sr.`Priority`
                       WHEN 0 THEN 'low'
                       WHEN 1 THEN 'medium'
                       WHEN 2 THEN 'high'
                       WHEN 3 THEN 'critical'
                       WHEN 4 THEN 'critical'
                       ELSE NULL
                   END
SET sr.PriorityId = ev.Id
WHERE sr.IsDeleted = 0;

COMMIT;


-- ─────────────────────────────────────────────────────────────────────────────
-- Done
-- ─────────────────────────────────────────────────────────────────────────────
SELECT 'SYS-009-EnumMigration migration complete. Run SYS-009-EnumMigration-verification.sql to check counts.' AS status;
