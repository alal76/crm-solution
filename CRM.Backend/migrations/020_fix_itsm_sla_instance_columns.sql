-- Migration 020: Fix ITSMSLAInstances notification column names
-- The EF entity uses longer descriptive names but migration 019 created shorter names.
-- Rename 6 columns to match the ITSM.SLAInstance entity properties exactly.
-- Date: 2026-02-09

ALTER TABLE ITSMSLAInstances
    CHANGE COLUMN ResponseWarning50Sent Response50PercentNotificationSent TINYINT(1) NOT NULL DEFAULT 0,
    CHANGE COLUMN ResponseWarning75Sent Response75PercentNotificationSent TINYINT(1) NOT NULL DEFAULT 0,
    CHANGE COLUMN ResponseBreachedSent ResponseBreachNotificationSent TINYINT(1) NOT NULL DEFAULT 0,
    CHANGE COLUMN ResolutionWarning50Sent Resolution50PercentNotificationSent TINYINT(1) NOT NULL DEFAULT 0,
    CHANGE COLUMN ResolutionWarning75Sent Resolution75PercentNotificationSent TINYINT(1) NOT NULL DEFAULT 0,
    CHANGE COLUMN ResolutionBreachedSent ResolutionBreachNotificationSent TINYINT(1) NOT NULL DEFAULT 0;
