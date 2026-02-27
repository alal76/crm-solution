-- ============================================================================
-- Reset Admin Password SQL Script
-- ============================================================================
-- This script resets the admin user (admin@crm.local) to require a fresh
-- password setup on next login.
--
-- Usage:
--   # Via Docker
--   docker exec -i crm-mariadb mariadb -u root -pRootPass@Dev2024 crm_db < scripts/reset-admin-password.sql
--
--   # Via SSH to remote server
--   ssh root@192.168.0.9 "docker exec -i crm-mariadb mariadb -u root -pRootPass@Dev2024 crm_db" < scripts/reset-admin-password.sql
--
--   # Direct MariaDB connection
--   mysql -h localhost -u crm_user -p crm_db < scripts/reset-admin-password.sql
-- ============================================================================

-- Show current admin user status
SELECT 
    'BEFORE:' as Status,
    Id, 
    Username, 
    Email, 
    PasswordNeverSet, 
    MustResetPassword,
    IsActive,
    LEFT(PasswordHash, 20) as PasswordHashPrefix
FROM Users 
WHERE Email = 'admin@crm.local';

-- Reset admin password fields
UPDATE Users 
SET 
    PasswordHash = '',
    PasswordNeverSet = 1,
    MustResetPassword = 0,
    PasswordLastChangedAt = NULL, -- NOSONAR: assignment, not comparison
    UpdatedAt = NOW()
WHERE Email = 'admin@crm.local';

-- Show updated admin user status
SELECT 
    'AFTER:' as Status,
    Id, 
    Username, 
    Email, 
    PasswordNeverSet, 
    MustResetPassword,
    IsActive,
    LEFT(PasswordHash, 20) as PasswordHashPrefix
FROM Users 
WHERE Email = 'admin@crm.local';

-- Confirm the change
SELECT 
    CASE 
        WHEN PasswordNeverSet = 1 AND PasswordHash = '' 
        THEN '✓ SUCCESS: Admin password reset. User will be prompted to set password on next login.'
        ELSE '✗ ERROR: Password reset may have failed. Please check the Users table.'
    END as Result
FROM Users 
WHERE Email = 'admin@crm.local';
