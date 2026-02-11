-- Migration: 022_refresh_token_table.sql
-- Purpose: Move refresh tokens from Users table to dedicated RefreshTokens table
-- Supports: Token rotation, multi-device sessions, revocation tracking, reuse detection
-- Date: 2026-02-18
-- Related: P-27 in SOLUTION_GAPS_REMEDIATION_PLAN.md

-- Step 1: Create RefreshTokens table
CREATE TABLE IF NOT EXISTS RefreshTokens (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Token VARCHAR(128) NOT NULL,
    UserId INT NOT NULL,
    ExpiresAt DATETIME(6) NOT NULL,
    RevokedAt DATETIME(6) NULL,
    ReplacedByToken VARCHAR(128) NULL,
    RevokedReason VARCHAR(200) NULL,
    IpAddress VARCHAR(45) NULL,
    DeviceInfo VARCHAR(500) NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    RowVersion BINARY(8) NULL,

    CONSTRAINT FK_RefreshTokens_Users_UserId
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,

    CONSTRAINT UQ_RefreshTokens_Token UNIQUE (Token)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Step 2: Create indexes for common queries
CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
CREATE INDEX IX_RefreshTokens_ExpiresAt ON RefreshTokens(ExpiresAt);

-- Step 3: Migrate existing refresh tokens from Users table
-- Only migrate non-null, non-expired tokens that still have value
INSERT INTO RefreshTokens (Token, UserId, ExpiresAt, CreatedAt, IsDeleted)
SELECT
    u.RefreshToken,
    u.Id,
    COALESCE(u.RefreshTokenExpiryTime, DATE_ADD(NOW(), INTERVAL 7 DAY)),
    COALESCE(u.UpdatedAt, u.CreatedAt, NOW()),
    0
FROM Users u
WHERE u.RefreshToken IS NOT NULL
  AND u.RefreshToken != ''
  AND u.IsDeleted = 0
  AND (u.RefreshTokenExpiryTime IS NULL OR u.RefreshTokenExpiryTime > NOW());

-- Step 4: Drop legacy columns from Users table
-- These are now replaced by the RefreshTokens table
ALTER TABLE Users DROP COLUMN IF EXISTS RefreshToken;
ALTER TABLE Users DROP COLUMN IF EXISTS RefreshTokenExpiryTime;
