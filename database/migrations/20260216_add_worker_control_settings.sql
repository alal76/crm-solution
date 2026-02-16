-- Add worker control settings to SystemSettings
ALTER TABLE SystemSettings
    ADD COLUMN WorkerControlState VARCHAR(50) NOT NULL DEFAULT 'Running',
    ADD COLUMN WorkerMaxInstances INT NOT NULL DEFAULT 1;
