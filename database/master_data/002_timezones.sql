-- ============================================================================
-- CRM Solution Database Master Data - Timezones
-- Version: 1.0
-- Date: 2026-01-28
-- Description: Standard IANA timezone identifiers with offsets and metadata
-- Note: Uses LookupCategory 21 (Timezone) from seed data
-- ============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================================
-- Create Timezone table if needed for standalone timezone management
-- This is an alternative to using the Lookup tables
-- ============================================================================
CREATE TABLE IF NOT EXISTS Timezones (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    IanaId VARCHAR(50) NOT NULL UNIQUE COMMENT 'IANA timezone identifier (e.g., America/New_York)',
    DisplayName VARCHAR(100) NOT NULL COMMENT 'User-friendly display name',
    StandardAbbreviation VARCHAR(10) COMMENT 'Standard time abbreviation (e.g., EST)',
    DaylightAbbreviation VARCHAR(10) COMMENT 'Daylight saving time abbreviation (e.g., EDT)',
    UtcOffset VARCHAR(10) NOT NULL COMMENT 'UTC offset in format +/-HH:MM',
    SupportsDst TINYINT(1) DEFAULT 0 COMMENT 'Whether timezone observes daylight saving time',
    SortOrder INT DEFAULT 0 COMMENT 'Display order for UI dropdowns',
    Region VARCHAR(50) COMMENT 'Geographic region (e.g., North America, Europe)',
    IsActive TINYINT(1) DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsDeleted TINYINT(1) DEFAULT 0,
    INDEX idx_timezone_region (Region),
    INDEX idx_timezone_offset (UtcOffset)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- Insert Standard Timezones
-- ============================================================================
INSERT INTO Timezones (IanaId, DisplayName, StandardAbbreviation, DaylightAbbreviation, UtcOffset, SupportsDst, SortOrder, Region) VALUES
-- North America (UTC-10 to UTC-4)
('Pacific/Honolulu', 'Hawaii', 'HST', NULL, '-10:00', 0, 10, 'North America'),
('America/Anchorage', 'Alaska', 'AKST', 'AKDT', '-09:00', 1, 20, 'North America'),
('America/Los_Angeles', 'Pacific Time (US & Canada)', 'PST', 'PDT', '-08:00', 1, 30, 'North America'),
('America/Vancouver', 'Pacific Time (Canada)', 'PST', 'PDT', '-08:00', 1, 31, 'North America'),
('America/Tijuana', 'Tijuana, Baja California', 'PST', 'PDT', '-08:00', 1, 32, 'North America'),
('America/Phoenix', 'Arizona', 'MST', NULL, '-07:00', 0, 40, 'North America'),
('America/Denver', 'Mountain Time (US & Canada)', 'MST', 'MDT', '-07:00', 1, 41, 'North America'),
('America/Edmonton', 'Mountain Time (Canada)', 'MST', 'MDT', '-07:00', 1, 42, 'North America'),
('America/Chicago', 'Central Time (US & Canada)', 'CST', 'CDT', '-06:00', 1, 50, 'North America'),
('America/Winnipeg', 'Central Time (Canada)', 'CST', 'CDT', '-06:00', 1, 51, 'North America'),
('America/Mexico_City', 'Central Time (Mexico)', 'CST', 'CDT', '-06:00', 1, 52, 'North America'),
('America/New_York', 'Eastern Time (US & Canada)', 'EST', 'EDT', '-05:00', 1, 60, 'North America'),
('America/Toronto', 'Eastern Time (Canada)', 'EST', 'EDT', '-05:00', 1, 61, 'North America'),
('America/Detroit', 'Eastern Time (Michigan)', 'EST', 'EDT', '-05:00', 1, 62, 'North America'),
('America/Indiana/Indianapolis', 'Indiana (East)', 'EST', 'EDT', '-05:00', 1, 63, 'North America'),
('America/Halifax', 'Atlantic Time (Canada)', 'AST', 'ADT', '-04:00', 1, 70, 'North America'),
('America/St_Johns', 'Newfoundland', 'NST', 'NDT', '-03:30', 1, 75, 'North America'),

-- South America
('America/Sao_Paulo', 'Brasilia, Sao Paulo', 'BRT', NULL, '-03:00', 0, 80, 'South America'),
('America/Buenos_Aires', 'Buenos Aires', 'ART', NULL, '-03:00', 0, 81, 'South America'),
('America/Santiago', 'Santiago', 'CLT', 'CLST', '-04:00', 1, 82, 'South America'),
('America/Lima', 'Lima, Bogota, Quito', 'PET', NULL, '-05:00', 0, 83, 'South America'),
('America/Bogota', 'Bogota', 'COT', NULL, '-05:00', 0, 84, 'South America'),
('America/Caracas', 'Caracas', 'VET', NULL, '-04:00', 0, 85, 'South America'),

-- Europe
('Atlantic/Reykjavik', 'Reykjavik', 'GMT', NULL, '+00:00', 0, 100, 'Europe'),
('Europe/London', 'London, Edinburgh', 'GMT', 'BST', '+00:00', 1, 101, 'Europe'),
('Europe/Dublin', 'Dublin', 'GMT', 'IST', '+00:00', 1, 102, 'Europe'),
('Europe/Lisbon', 'Lisbon', 'WET', 'WEST', '+00:00', 1, 103, 'Europe'),
('Europe/Paris', 'Paris', 'CET', 'CEST', '+01:00', 1, 110, 'Europe'),
('Europe/Berlin', 'Berlin, Frankfurt', 'CET', 'CEST', '+01:00', 1, 111, 'Europe'),
('Europe/Amsterdam', 'Amsterdam', 'CET', 'CEST', '+01:00', 1, 112, 'Europe'),
('Europe/Brussels', 'Brussels', 'CET', 'CEST', '+01:00', 1, 113, 'Europe'),
('Europe/Madrid', 'Madrid', 'CET', 'CEST', '+01:00', 1, 114, 'Europe'),
('Europe/Rome', 'Rome, Milan', 'CET', 'CEST', '+01:00', 1, 115, 'Europe'),
('Europe/Vienna', 'Vienna', 'CET', 'CEST', '+01:00', 1, 116, 'Europe'),
('Europe/Zurich', 'Zurich, Bern', 'CET', 'CEST', '+01:00', 1, 117, 'Europe'),
('Europe/Stockholm', 'Stockholm', 'CET', 'CEST', '+01:00', 1, 118, 'Europe'),
('Europe/Copenhagen', 'Copenhagen', 'CET', 'CEST', '+01:00', 1, 119, 'Europe'),
('Europe/Oslo', 'Oslo', 'CET', 'CEST', '+01:00', 1, 120, 'Europe'),
('Europe/Warsaw', 'Warsaw', 'CET', 'CEST', '+01:00', 1, 121, 'Europe'),
('Europe/Prague', 'Prague', 'CET', 'CEST', '+01:00', 1, 122, 'Europe'),
('Europe/Budapest', 'Budapest', 'CET', 'CEST', '+01:00', 1, 123, 'Europe'),
('Europe/Athens', 'Athens', 'EET', 'EEST', '+02:00', 1, 130, 'Europe'),
('Europe/Helsinki', 'Helsinki', 'EET', 'EEST', '+02:00', 1, 131, 'Europe'),
('Europe/Bucharest', 'Bucharest', 'EET', 'EEST', '+02:00', 1, 132, 'Europe'),
('Europe/Sofia', 'Sofia', 'EET', 'EEST', '+02:00', 1, 133, 'Europe'),
('Europe/Kiev', 'Kyiv', 'EET', 'EEST', '+02:00', 1, 134, 'Europe'),
('Europe/Istanbul', 'Istanbul', 'TRT', NULL, '+03:00', 0, 140, 'Europe'),
('Europe/Moscow', 'Moscow, St. Petersburg', 'MSK', NULL, '+03:00', 0, 141, 'Europe'),

-- Middle East
('Asia/Jerusalem', 'Jerusalem', 'IST', 'IDT', '+02:00', 1, 150, 'Middle East'),
('Asia/Beirut', 'Beirut', 'EET', 'EEST', '+02:00', 1, 151, 'Middle East'),
('Asia/Riyadh', 'Riyadh, Kuwait', 'AST', NULL, '+03:00', 0, 152, 'Middle East'),
('Asia/Baghdad', 'Baghdad', 'AST', NULL, '+03:00', 0, 153, 'Middle East'),
('Asia/Tehran', 'Tehran', 'IRST', 'IRDT', '+03:30', 1, 154, 'Middle East'),
('Asia/Dubai', 'Dubai, Abu Dhabi', 'GST', NULL, '+04:00', 0, 155, 'Middle East'),
('Asia/Muscat', 'Muscat', 'GST', NULL, '+04:00', 0, 156, 'Middle East'),
('Asia/Karachi', 'Karachi, Islamabad', 'PKT', NULL, '+05:00', 0, 157, 'Middle East'),

-- Asia
('Asia/Kolkata', 'Mumbai, Kolkata, New Delhi', 'IST', NULL, '+05:30', 0, 160, 'Asia'),
('Asia/Colombo', 'Colombo', 'IST', NULL, '+05:30', 0, 161, 'Asia'),
('Asia/Kathmandu', 'Kathmandu', 'NPT', NULL, '+05:45', 0, 162, 'Asia'),
('Asia/Dhaka', 'Dhaka', 'BST', NULL, '+06:00', 0, 163, 'Asia'),
('Asia/Almaty', 'Almaty', 'ALMT', NULL, '+06:00', 0, 164, 'Asia'),
('Asia/Yangon', 'Yangon (Rangoon)', 'MMT', NULL, '+06:30', 0, 165, 'Asia'),
('Asia/Bangkok', 'Bangkok, Hanoi', 'ICT', NULL, '+07:00', 0, 170, 'Asia'),
('Asia/Jakarta', 'Jakarta', 'WIB', NULL, '+07:00', 0, 171, 'Asia'),
('Asia/Ho_Chi_Minh', 'Ho Chi Minh City', 'ICT', NULL, '+07:00', 0, 172, 'Asia'),
('Asia/Singapore', 'Singapore', 'SGT', NULL, '+08:00', 0, 180, 'Asia'),
('Asia/Kuala_Lumpur', 'Kuala Lumpur', 'MYT', NULL, '+08:00', 0, 181, 'Asia'),
('Asia/Hong_Kong', 'Hong Kong', 'HKT', NULL, '+08:00', 0, 182, 'Asia'),
('Asia/Taipei', 'Taipei', 'CST', NULL, '+08:00', 0, 183, 'Asia'),
('Asia/Shanghai', 'Beijing, Shanghai', 'CST', NULL, '+08:00', 0, 184, 'Asia'),
('Asia/Manila', 'Manila', 'PHT', NULL, '+08:00', 0, 185, 'Asia'),
('Asia/Seoul', 'Seoul', 'KST', NULL, '+09:00', 0, 190, 'Asia'),
('Asia/Tokyo', 'Tokyo, Osaka', 'JST', NULL, '+09:00', 0, 191, 'Asia'),

-- Oceania / Pacific
('Australia/Perth', 'Perth', 'AWST', NULL, '+08:00', 0, 200, 'Oceania'),
('Australia/Darwin', 'Darwin', 'ACST', NULL, '+09:30', 0, 201, 'Oceania'),
('Australia/Adelaide', 'Adelaide', 'ACST', 'ACDT', '+09:30', 1, 202, 'Oceania'),
('Australia/Brisbane', 'Brisbane', 'AEST', NULL, '+10:00', 0, 203, 'Oceania'),
('Australia/Sydney', 'Sydney, Melbourne', 'AEST', 'AEDT', '+10:00', 1, 204, 'Oceania'),
('Australia/Hobart', 'Hobart', 'AEST', 'AEDT', '+10:00', 1, 205, 'Oceania'),
('Pacific/Guam', 'Guam, Port Moresby', 'ChST', NULL, '+10:00', 0, 206, 'Oceania'),
('Pacific/Noumea', 'New Caledonia', 'NCT', NULL, '+11:00', 0, 207, 'Oceania'),
('Pacific/Fiji', 'Fiji', 'FJT', 'FJST', '+12:00', 1, 208, 'Oceania'),
('Pacific/Auckland', 'Auckland, Wellington', 'NZST', 'NZDT', '+12:00', 1, 209, 'Oceania'),
('Pacific/Tongatapu', 'Tonga', 'TOT', NULL, '+13:00', 0, 210, 'Oceania'),
('Pacific/Apia', 'Samoa', 'WST', 'WSDT', '+13:00', 1, 211, 'Oceania'),
('Pacific/Kiritimati', 'Kiritimati Island', 'LINT', NULL, '+14:00', 0, 212, 'Oceania'),

-- Africa
('Africa/Casablanca', 'Casablanca', 'WET', 'WEST', '+00:00', 1, 220, 'Africa'),
('Africa/Lagos', 'Lagos', 'WAT', NULL, '+01:00', 0, 221, 'Africa'),
('Africa/Johannesburg', 'Johannesburg, Pretoria', 'SAST', NULL, '+02:00', 0, 222, 'Africa'),
('Africa/Cairo', 'Cairo', 'EET', NULL, '+02:00', 0, 223, 'Africa'),
('Africa/Nairobi', 'Nairobi', 'EAT', NULL, '+03:00', 0, 224, 'Africa'),

-- UTC / Special
('UTC', 'Coordinated Universal Time', 'UTC', NULL, '+00:00', 0, 999, 'UTC')
ON DUPLICATE KEY UPDATE DisplayName = VALUES(DisplayName), UtcOffset = VALUES(UtcOffset);

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================================
-- View to get timezones by region for UI dropdowns
-- ============================================================================
-- CREATE OR REPLACE VIEW v_timezones_by_region AS
-- SELECT 
--     Region,
--     IanaId,
--     CONCAT(DisplayName, ' (', UtcOffset, ')') AS FullDisplayName,
--     UtcOffset,
--     SupportsDst
-- FROM Timezones
-- WHERE IsActive = 1 AND IsDeleted = 0
-- ORDER BY Region, SortOrder;
