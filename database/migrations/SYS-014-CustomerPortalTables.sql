-- =============================================================================
-- SYS-014: Customer Portal Tables
-- PORTAL-016: EF Core migration verification stub
-- Generated: 2026-02-28
-- Description: Creates PortalUsers and PortalConfigs tables if they are absent.
--              EF Core (CrmDbContext) is the source of truth; this script is
--              provided as a reference for environments where migrations cannot
--              be run directly.
-- =============================================================================
-- NOTE: The authoritative schema is managed by EF Core migrations.
--       Run:
--           dotnet ef migrations add AddCustomerPortal
--               --project src/CRM.Infrastructure --startup-project src/CRM.Api
--           dotnet ef database update
--               --project src/CRM.Infrastructure --startup-project src/CRM.Api
--       to generate and apply a proper migration instead of this script.
-- =============================================================================

-- PortalUsers: Customer self-service portal accounts ---

CREATE TABLE IF NOT EXISTS `PortalUsers` (
    `Id`                        INT             NOT NULL AUTO_INCREMENT,
    `Email`                     VARCHAR(255)    NOT NULL,
    `PasswordHash`              VARCHAR(512)    NOT NULL,
    `DisplayName`               VARCHAR(100)    NULL,
    `ContactId`                 INT             NULL,
    `AccountId`                 INT             NULL,
    `IsActive`                  TINYINT(1)      NOT NULL DEFAULT 1,
    `IsEmailVerified`           TINYINT(1)      NOT NULL DEFAULT 0,
    `EmailVerificationToken`    VARCHAR(128)    NULL,
    `EmailVerifiedAt`           DATETIME(6)     NULL,
    `PasswordResetToken`        VARCHAR(128)    NULL,
    `PasswordResetExpiry`       DATETIME(6)     NULL,
    `LastLoginAt`               DATETIME(6)     NULL,
    `CreatedAt`                 DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt`                 DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    `IsDeleted`                 TINYINT(1)      NOT NULL DEFAULT 0,
    `RowVersion`                LONGBLOB        NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_PortalUsers_Email` (`Email`),
    KEY `IX_PortalUsers_ContactId` (`ContactId`),
    KEY `IX_PortalUsers_AccountId` (`AccountId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- PortalConfigs: Single portal configuration row --------------------------

CREATE TABLE IF NOT EXISTS `PortalConfigs` (
    `Id`                    INT             NOT NULL AUTO_INCREMENT,
    `IsEnabled`             TINYINT(1)      NOT NULL DEFAULT 0,
    `AllowSelfRegistration` TINYINT(1)      NOT NULL DEFAULT 0,
    `PortalTitle`           VARCHAR(100)    NULL,
    `WelcomeMessage`        VARCHAR(1000)   NULL,
    `SupportEmail`          VARCHAR(200)    NULL,
    `LogoUrl`               VARCHAR(500)    NULL,
    `PrimaryColor`          VARCHAR(20)     NULL,
    `AllowedDomains`        VARCHAR(500)    NULL,
    `CreatedAt`             DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt`             DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    `IsDeleted`             TINYINT(1)      NOT NULL DEFAULT 0,
    `RowVersion`            LONGBLOB        NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
