-- Migration 021: Drop orphan tables
-- These tables were created by EnsureCreated() or legacy migrations 
-- but are not mapped to any EF Core DbSet:
--   Accounts: Created before Account entity was fixed to [Table("Customers")]
--   ArticleFeedback: Legacy orphan (EF uses ArticleFeedbacks and ITSMArticleFeedback tables)

-- Verify tables are empty before dropping
SELECT 'Accounts' AS TableName, COUNT(*) AS RowCount FROM Accounts
UNION ALL
SELECT 'ArticleFeedback', COUNT(*) FROM ArticleFeedback;

-- Drop orphan tables
DROP TABLE IF EXISTS Accounts;
DROP TABLE IF EXISTS ArticleFeedback;
