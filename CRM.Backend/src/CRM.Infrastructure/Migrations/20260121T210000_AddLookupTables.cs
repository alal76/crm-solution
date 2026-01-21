using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    public partial class AddLookupTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LookupCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupCategories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LookupItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LookupCategoryId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Meta = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LookupItems_LookupCategories_LookupCategoryId",
                        column: x => x.LookupCategoryId,
                        principalTable: "LookupCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_LookupItems_LookupCategoryId",
                table: "LookupItems",
                column: "LookupCategoryId");

            // Add customer FK columns
            migrationBuilder.AddColumn<int>(
                name: "CurrencyLookupId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BillingCycleLookupId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrencyLookupId",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreferredContactMethodLookupId",
                table: "Contacts",
                type: "int",
                nullable: true);

            // Foreign keys
            migrationBuilder.CreateIndex(
                name: "IX_Customers_CurrencyLookupId",
                table: "Customers",
                column: "CurrencyLookupId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_BillingCycleLookupId",
                table: "Customers",
                column: "BillingCycleLookupId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CurrencyLookupId",
                table: "Accounts",
                column: "CurrencyLookupId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_PreferredContactMethodLookupId",
                table: "Contacts",
                column: "PreferredContactMethodLookupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_LookupItems_CurrencyLookupId",
                table: "Customers",
                column: "CurrencyLookupId",
                principalTable: "LookupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_LookupItems_BillingCycleLookupId",
                table: "Customers",
                column: "BillingCycleLookupId",
                principalTable: "LookupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_LookupItems_CurrencyLookupId",
                table: "Accounts",
                column: "CurrencyLookupId",
                principalTable: "LookupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_LookupItems_PreferredContactMethodLookupId",
                table: "Contacts",
                column: "PreferredContactMethodLookupId",
                principalTable: "LookupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Customers_LookupItems_CurrencyLookupId", table: "Customers");
            migrationBuilder.DropForeignKey(name: "FK_Customers_LookupItems_BillingCycleLookupId", table: "Customers");
            migrationBuilder.DropForeignKey(name: "FK_Accounts_LookupItems_CurrencyLookupId", table: "Accounts");
            migrationBuilder.DropForeignKey(name: "FK_Contacts_LookupItems_PreferredContactMethodLookupId", table: "Contacts");

            migrationBuilder.DropIndex(name: "IX_Customers_CurrencyLookupId", table: "Customers");
            migrationBuilder.DropIndex(name: "IX_Customers_BillingCycleLookupId", table: "Customers");
            migrationBuilder.DropIndex(name: "IX_Accounts_CurrencyLookupId", table: "Accounts");
            migrationBuilder.DropIndex(name: "IX_Contacts_PreferredContactMethodLookupId", table: "Contacts");

            migrationBuilder.DropColumn(name: "CurrencyLookupId", table: "Customers");
            migrationBuilder.DropColumn(name: "BillingCycleLookupId", table: "Customers");
            migrationBuilder.DropColumn(name: "CurrencyLookupId", table: "Accounts");
            migrationBuilder.DropColumn(name: "PreferredContactMethodLookupId", table: "Contacts");

            migrationBuilder.DropTable(name: "LookupItems");
            migrationBuilder.DropTable(name: "LookupCategories");
        }
    }
}
