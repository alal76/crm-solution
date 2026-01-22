using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations.Auto
{
    public partial class MigrateInlineTagsToNormalized : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use an inline numbers table (1..50) to avoid CTE compatibility issues in older MariaDB
            var seq = @"(SELECT 1 AS n UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5
                UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9 UNION ALL SELECT 10
                UNION ALL SELECT 11 UNION ALL SELECT 12 UNION ALL SELECT 13 UNION ALL SELECT 14 UNION ALL SELECT 15
                UNION ALL SELECT 16 UNION ALL SELECT 17 UNION ALL SELECT 18 UNION ALL SELECT 19 UNION ALL SELECT 20
                UNION ALL SELECT 21 UNION ALL SELECT 22 UNION ALL SELECT 23 UNION ALL SELECT 24 UNION ALL SELECT 25
                UNION ALL SELECT 26 UNION ALL SELECT 27 UNION ALL SELECT 28 UNION ALL SELECT 29 UNION ALL SELECT 30
                UNION ALL SELECT 31 UNION ALL SELECT 32 UNION ALL SELECT 33 UNION ALL SELECT 34 UNION ALL SELECT 35
                UNION ALL SELECT 36 UNION ALL SELECT 37 UNION ALL SELECT 38 UNION ALL SELECT 39 UNION ALL SELECT 40
                UNION ALL SELECT 41 UNION ALL SELECT 42 UNION ALL SELECT 43 UNION ALL SELECT 44 UNION ALL SELECT 45
                UNION ALL SELECT 46 UNION ALL SELECT 47 UNION ALL SELECT 48 UNION ALL SELECT 49 UNION ALL SELECT 50)";

            // Products (singular and plural keys to be idempotent regardless of earlier attempts)
            migrationBuilder.Sql($@"
INSERT INTO EntityTags (EntityType, EntityId, Tag, CreatedAt)
SELECT 'Product', p.Id,
       TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(p.Tags, ',', seq.n), ',', -1)), NOW()
FROM Products p
JOIN {seq} AS seq ON seq.n <= 1 + LENGTH(p.Tags) - LENGTH(REPLACE(p.Tags, ',', ''))
WHERE p.Tags IS NOT NULL AND p.Tags <> ''
  AND TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(p.Tags, ',', seq.n), ',', -1)) <> ''
  AND NOT EXISTS(
    SELECT 1 FROM EntityTags et WHERE et.EntityType='Product' AND et.EntityId = p.Id AND et.Tag = TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(p.Tags, ',', seq.n), ',', -1))
  );

INSERT INTO EntityTags (EntityType, EntityId, Tag, CreatedAt)
SELECT 'Products', p.Id,
       TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(p.Tags, ',', seq.n), ',', -1)), NOW()
FROM Products p
JOIN {seq} AS seq ON seq.n <= 1 + LENGTH(p.Tags) - LENGTH(REPLACE(p.Tags, ',', ''))
WHERE p.Tags IS NOT NULL AND p.Tags <> ''
  AND TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(p.Tags, ',', seq.n), ',', -1)) <> ''
  AND NOT EXISTS(
    SELECT 1 FROM EntityTags et WHERE et.EntityType='Products' AND et.EntityId = p.Id AND et.Tag = TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(p.Tags, ',', seq.n), ',', -1))
  );

", suppressTransaction: false);

            // MarketingCampaigns
            migrationBuilder.Sql($@"
INSERT INTO EntityTags (EntityType, EntityId, Tag, CreatedAt)
SELECT 'MarketingCampaign', m.Id,
       TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(m.Tags, ',', seq.n), ',', -1)), NOW()
FROM MarketingCampaigns m
JOIN {seq} AS seq ON seq.n <= 1 + LENGTH(m.Tags) - LENGTH(REPLACE(m.Tags, ',', ''))
WHERE m.Tags IS NOT NULL AND m.Tags <> ''
  AND TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(m.Tags, ',', seq.n), ',', -1)) <> ''
  AND NOT EXISTS(
    SELECT 1 FROM EntityTags et WHERE et.EntityType='MarketingCampaign' AND et.EntityId = m.Id AND et.Tag = TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(m.Tags, ',', seq.n), ',', -1))
  );

INSERT INTO EntityTags (EntityType, EntityId, Tag, CreatedAt)
SELECT 'MarketingCampaigns', m.Id,
       TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(m.Tags, ',', seq.n), ',', -1)), NOW()
FROM MarketingCampaigns m
JOIN {seq} AS seq ON seq.n <= 1 + LENGTH(m.Tags) - LENGTH(REPLACE(m.Tags, ',', ''))
WHERE m.Tags IS NOT NULL AND m.Tags <> ''
  AND TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(m.Tags, ',', seq.n), ',', -1)) <> ''
  AND NOT EXISTS(
    SELECT 1 FROM EntityTags et WHERE et.EntityType='MarketingCampaigns' AND et.EntityId = m.Id AND et.Tag = TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(m.Tags, ',', seq.n), ',', -1))
  );
", suppressTransaction: false);

            // Notes
            migrationBuilder.Sql($@"
INSERT INTO EntityTags (EntityType, EntityId, Tag, CreatedAt)
SELECT 'Note', n.Id,
       TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(n.Tags, ',', seq.n), ',', -1)), NOW()
FROM Notes n
JOIN {seq} AS seq ON seq.n <= 1 + LENGTH(n.Tags) - LENGTH(REPLACE(n.Tags, ',', ''))
WHERE n.Tags IS NOT NULL AND n.Tags <> ''
  AND TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(n.Tags, ',', seq.n), ',', -1)) <> ''
  AND NOT EXISTS(
    SELECT 1 FROM EntityTags et WHERE et.EntityType='Note' AND et.EntityId = n.Id AND et.Tag = TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(n.Tags, ',', seq.n), ',', -1))
  );

INSERT INTO EntityTags (EntityType, EntityId, Tag, CreatedAt)
SELECT 'Notes', n.Id,
       TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(n.Tags, ',', seq.n), ',', -1)), NOW()
FROM Notes n
JOIN {seq} AS seq ON seq.n <= 1 + LENGTH(n.Tags) - LENGTH(REPLACE(n.Tags, ',', ''))
WHERE n.Tags IS NOT NULL AND n.Tags <> ''
  AND TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(n.Tags, ',', seq.n), ',', -1)) <> ''
  AND NOT EXISTS(
    SELECT 1 FROM EntityTags et WHERE et.EntityType='Notes' AND et.EntityId = n.Id AND et.Tag = TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(n.Tags, ',', seq.n), ',', -1))
  );
", suppressTransaction: false);

            // ServiceRequests
            migrationBuilder.Sql($@"
INSERT INTO EntityTags (EntityType, EntityId, Tag, CreatedAt)
SELECT 'ServiceRequest', s.Id,
       TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(s.Tags, ',', seq.n), ',', -1)), NOW()
FROM ServiceRequests s
JOIN {seq} AS seq ON seq.n <= 1 + LENGTH(s.Tags) - LENGTH(REPLACE(s.Tags, ',', ''))
WHERE s.Tags IS NOT NULL AND s.Tags <> ''
  AND TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(s.Tags, ',', seq.n), ',', -1)) <> ''
  AND NOT EXISTS(
    SELECT 1 FROM EntityTags et WHERE et.EntityType='ServiceRequest' AND et.EntityId = s.Id AND et.Tag = TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(s.Tags, ',', seq.n), ',', -1))
  );

INSERT INTO EntityTags (EntityType, EntityId, Tag, CreatedAt)
SELECT 'ServiceRequests', s.Id,
       TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(s.Tags, ',', seq.n), ',', -1)), NOW()
FROM ServiceRequests s
JOIN {seq} AS seq ON seq.n <= 1 + LENGTH(s.Tags) - LENGTH(REPLACE(s.Tags, ',', ''))
WHERE s.Tags IS NOT NULL AND s.Tags <> ''
  AND TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(s.Tags, ',', seq.n), ',', -1)) <> ''
  AND NOT EXISTS(
    SELECT 1 FROM EntityTags et WHERE et.EntityType='ServiceRequests' AND et.EntityId = s.Id AND et.Tag = TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(s.Tags, ',', seq.n), ',', -1))
  );
", suppressTransaction: false);

            // Customers (tags)
            migrationBuilder.Sql($@"
INSERT INTO EntityTags (EntityType, EntityId, Tag, CreatedAt)
SELECT 'Customer', c.Id,
       TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(c.Tags, ',', seq.n), ',', -1)), NOW()
FROM Customers c
JOIN {seq} AS seq ON seq.n <= 1 + LENGTH(c.Tags) - LENGTH(REPLACE(c.Tags, ',', ''))
WHERE c.Tags IS NOT NULL AND c.Tags <> ''
  AND TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(c.Tags, ',', seq.n), ',', -1)) <> ''
  AND NOT EXISTS(
    SELECT 1 FROM EntityTags et WHERE et.EntityType='Customer' AND et.EntityId = c.Id AND et.Tag = TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(c.Tags, ',', seq.n), ',', -1))
  );

INSERT INTO EntityTags (EntityType, EntityId, Tag, CreatedAt)
SELECT 'Customers', c.Id,
       TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(c.Tags, ',', seq.n), ',', -1)), NOW()
FROM Customers c
JOIN {seq} AS seq ON seq.n <= 1 + LENGTH(c.Tags) - LENGTH(REPLACE(c.Tags, ',', ''))
WHERE c.Tags IS NOT NULL AND c.Tags <> ''
  AND TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(c.Tags, ',', seq.n), ',', -1)) <> ''
  AND NOT EXISTS(
    SELECT 1 FROM EntityTags et WHERE et.EntityType='Customers' AND et.EntityId = c.Id AND et.Tag = TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(c.Tags, ',', seq.n), ',', -1))
  );
", suppressTransaction: false);

            // CustomFields: copy raw JSON/text where present
            migrationBuilder.Sql(@"
INSERT INTO CustomFields (EntityType, EntityId, `Key`, `Value`, CreatedAt)
SELECT 'Product', p.Id, NULL, p.CustomFields, NOW()
FROM Products p
WHERE p.CustomFields IS NOT NULL AND p.CustomFields <> ''
  AND NOT EXISTS(SELECT 1 FROM CustomFields cf WHERE cf.EntityType='Product' AND cf.EntityId = p.Id AND cf.Value = p.CustomFields);

INSERT INTO CustomFields (EntityType, EntityId, `Key`, `Value`, CreatedAt)
SELECT 'Products', p.Id, NULL, p.CustomFields, NOW()
FROM Products p
WHERE p.CustomFields IS NOT NULL AND p.CustomFields <> ''
  AND NOT EXISTS(SELECT 1 FROM CustomFields cf WHERE cf.EntityType='Products' AND cf.EntityId = p.Id AND cf.Value = p.CustomFields);

INSERT INTO CustomFields (EntityType, EntityId, `Key`, `Value`, CreatedAt)
SELECT 'MarketingCampaign', m.Id, NULL, m.CustomFields, NOW()
FROM MarketingCampaigns m
WHERE m.CustomFields IS NOT NULL AND m.CustomFields <> ''
  AND NOT EXISTS(SELECT 1 FROM CustomFields cf WHERE cf.EntityType='MarketingCampaign' AND cf.EntityId = m.Id AND cf.Value = m.CustomFields);

INSERT INTO CustomFields (EntityType, EntityId, `Key`, `Value`, CreatedAt)
SELECT 'MarketingCampaigns', m.Id, NULL, m.CustomFields, NOW()
FROM MarketingCampaigns m
WHERE m.CustomFields IS NOT NULL AND m.CustomFields <> ''
  AND NOT EXISTS(SELECT 1 FROM CustomFields cf WHERE cf.EntityType='MarketingCampaigns' AND cf.EntityId = m.Id AND cf.Value = m.CustomFields);

INSERT INTO CustomFields (EntityType, EntityId, `Key`, `Value`, CreatedAt)
SELECT 'Customer', c.Id, NULL, c.CustomFields, NOW()
FROM Customers c
WHERE c.CustomFields IS NOT NULL AND c.CustomFields <> ''
  AND NOT EXISTS(SELECT 1 FROM CustomFields cf WHERE cf.EntityType='Customer' AND cf.EntityId = c.Id AND cf.Value = c.CustomFields);

INSERT INTO CustomFields (EntityType, EntityId, `Key`, `Value`, CreatedAt)
SELECT 'Customers', c.Id, NULL, c.CustomFields, NOW()
FROM Customers c
WHERE c.CustomFields IS NOT NULL AND c.CustomFields <> ''
  AND NOT EXISTS(SELECT 1 FROM CustomFields cf WHERE cf.EntityType='Customers' AND cf.EntityId = c.Id AND cf.Value = c.CustomFields);
", suppressTransaction: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data migration is idempotent; no automatic rollback implemented here.
        }
    }
}
