using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations.Auto
{
  public partial class AddTagsAndCustomFields : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: "Tags",
        columns: table => new
        {
          Id = table.Column<int>(type: "int", nullable: false),
          Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
          CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
          UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
          IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
        },
        constraints: table => { table.PrimaryKey("PK_Tags", x => x.Id); });

      migrationBuilder.CreateTable(
        name: "EntityTags",
        columns: table => new
        {
          Id = table.Column<int>(type: "int", nullable: false),
          EntityType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
          EntityId = table.Column<int>(type: "int", nullable: false),
          TagId = table.Column<int>(type: "int", nullable: false),
          Tag = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
          CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
        },
        constraints: table => { table.PrimaryKey("PK_EntityTags", x => x.Id); });

      migrationBuilder.CreateTable(
        name: "CustomFields",
        columns: table => new
        {
          Id = table.Column<int>(type: "int", nullable: false),
          EntityType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
          EntityId = table.Column<int>(type: "int", nullable: false),
          Key = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
          Value = table.Column<string>(type: "TEXT", nullable: true),
          CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
        },
        constraints: table => { table.PrimaryKey("PK_CustomFields", x => x.Id); });

            // Idempotent data migration: split comma-separated Tags into EntityTags for a set of top entities
            var splitTagsSql = @"
            WITH RECURSIVE seq(n) AS (
              SELECT 1
              UNION ALL
              SELECT n + 1 FROM seq WHERE n < 200
            )

            -- Products
            INSERT INTO EntityTags (EntityType, EntityId, Tag, CreatedAt)
            SELECT 'Products', p.Id,
                   TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(p.Tags, ',', seq.n), ',', -1)), NOW()
            FROM Products p
            JOIN seq ON seq.n <= 1 + LENGTH(p.Tags) - LENGTH(REPLACE(p.Tags, ',', ''))
            WHERE p.Tags IS NOT NULL AND p.Tags <> ''
              AND TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(p.Tags, ',', seq.n), ',', -1)) <> ''
              AND NOT EXISTS(
                SELECT 1 FROM EntityTags et WHERE et.EntityType='Products' AND et.EntityId = p.Id AND et.Tag = TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(p.Tags, ',', seq.n), ',', -1))
              );

            -- MarketingCampaigns
            INSERT INTO EntityTags (EntityType, EntityId, Tag, CreatedAt)
            SELECT 'MarketingCampaigns', m.Id,
                   TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(m.Tags, ',', seq.n), ',', -1)), NOW()
            FROM MarketingCampaigns m
            JOIN seq ON seq.n <= 1 + LENGTH(m.Tags) - LENGTH(REPLACE(m.Tags, ',', ''))
            WHERE m.Tags IS NOT NULL AND m.Tags <> ''
              AND TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(m.Tags, ',', seq.n), ',', -1)) <> ''
              AND NOT EXISTS(
                SELECT 1 FROM EntityTags et WHERE et.EntityType='MarketingCampaigns' AND et.EntityId = m.Id AND et.Tag = TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(m.Tags, ',', seq.n), ',', -1))
              );

            -- Notes
            INSERT INTO EntityTags (EntityType, EntityId, Tag, CreatedAt)
            SELECT 'Notes', n.Id,
                   TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(n.Tags, ',', seq.n), ',', -1)), NOW()
            FROM Notes n
            JOIN seq ON seq.n <= 1 + LENGTH(n.Tags) - LENGTH(REPLACE(n.Tags, ',', ''))
            WHERE n.Tags IS NOT NULL AND n.Tags <> ''
              AND TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(n.Tags, ',', seq.n), ',', -1)) <> ''
              AND NOT EXISTS(
                SELECT 1 FROM EntityTags et WHERE et.EntityType='Notes' AND et.EntityId = n.Id AND et.Tag = TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(n.Tags, ',', seq.n), ',', -1))
              );

            -- ServiceRequests (Tags column may be small)
            INSERT INTO EntityTags (EntityType, EntityId, Tag, CreatedAt)
            SELECT 'ServiceRequests', s.Id,
                   TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(s.Tags, ',', seq.n), ',', -1)), NOW()
            FROM ServiceRequests s
            JOIN seq ON seq.n <= 1 + LENGTH(s.Tags) - LENGTH(REPLACE(s.Tags, ',', ''))
            WHERE s.Tags IS NOT NULL AND s.Tags <> ''
              AND TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(s.Tags, ',', seq.n), ',', -1)) <> ''
              AND NOT EXISTS(
                SELECT 1 FROM EntityTags et WHERE et.EntityType='ServiceRequests' AND et.EntityId = s.Id AND et.Tag = TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(s.Tags, ',', seq.n), ',', -1))
              );

            -- Generic: migrate CustomFields as raw JSON/text into CustomFields table for selected entities
            INSERT INTO CustomFields (EntityType, EntityId, `Key`, `Value`, CreatedAt)
            SELECT 'Products', p.Id, NULL, p.CustomFields, NOW()
            FROM Products p
            WHERE p.CustomFields IS NOT NULL AND p.CustomFields <> ''
              AND NOT EXISTS(SELECT 1 FROM CustomFields cf WHERE cf.EntityType='Products' AND cf.EntityId = p.Id AND cf.Value = p.CustomFields);

            INSERT INTO CustomFields (EntityType, EntityId, `Key`, `Value`, CreatedAt)
            SELECT 'MarketingCampaigns', m.Id, NULL, m.CustomFields, NOW()
            FROM MarketingCampaigns m
            WHERE m.CustomFields IS NOT NULL AND m.CustomFields <> ''
              AND NOT EXISTS(SELECT 1 FROM CustomFields cf WHERE cf.EntityType='MarketingCampaigns' AND cf.EntityId = m.Id AND cf.Value = m.CustomFields);

            INSERT INTO CustomFields (EntityType, EntityId, `Key`, `Value`, CreatedAt)
            SELECT 'Customers', c.Id, NULL, c.CustomFields, NOW()
            FROM Customers c
            WHERE c.CustomFields IS NOT NULL AND c.CustomFields <> ''
              AND NOT EXISTS(SELECT 1 FROM CustomFields cf WHERE cf.EntityType='Customers' AND cf.EntityId = c.Id AND cf.Value = c.CustomFields);
            ";

            migrationBuilder.Sql(splitTagsSql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CustomFields");
            migrationBuilder.DropTable(name: "EntityTags");
            migrationBuilder.DropTable(name: "Tags");
        }
    }
}
