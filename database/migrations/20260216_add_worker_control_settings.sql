-- Add worker control settings to SystemSettings

SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;
SET time_zone = '+00:00';

ALTER TABLE SystemSettings
    ADD COLUMN WorkerControlState VARCHAR(50) NOT NULL DEFAULT 'Running',
    ADD COLUMN WorkerMaxInstances INT NOT NULL DEFAULT 1;