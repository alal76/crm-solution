using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupTables_EF : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BillingCycleLookupId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrencyLookupId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreferredContactMethodLookupId",
                table: "Contacts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreferredContactMethodLookupId1",
                table: "Contacts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrencyLookupId",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Line1 = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Line2 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    State = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PostalCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Country = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ContactDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DetailType = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Label = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactDetails", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ContactInfoLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OwnerType = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    InfoKind = table.Column<int>(type: "int", nullable: false),
                    InfoId = table.Column<int>(type: "int", nullable: false),
                    IsPrimaryForOwner = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountId = table.Column<int>(type: "int", nullable: true),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    LeadId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactInfoLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactInfoLinks_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContactInfoLinks_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContactInfoLinks_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContactInfoLinks_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupCategories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SocialAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Network = table.Column<int>(type: "int", nullable: false),
                    HandleOrUrl = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Label = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialAccounts", x => x.Id);
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
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
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
                name: "IX_Customers_BillingCycleLookupId",
                table: "Customers",
                column: "BillingCycleLookupId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CurrencyLookupId",
                table: "Customers",
                column: "CurrencyLookupId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_PreferredContactMethodLookupId",
                table: "Contacts",
                column: "PreferredContactMethodLookupId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_PreferredContactMethodLookupId1",
                table: "Contacts",
                column: "PreferredContactMethodLookupId1");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CurrencyLookupId",
                table: "Accounts",
                column: "CurrencyLookupId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactInfoLinks_AccountId",
                table: "ContactInfoLinks",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactInfoLinks_ContactId",
                table: "ContactInfoLinks",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactInfoLinks_CustomerId",
                table: "ContactInfoLinks",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactInfoLinks_InfoKind_InfoId",
                table: "ContactInfoLinks",
                columns: new[] { "InfoKind", "InfoId" });

            migrationBuilder.CreateIndex(
                name: "IX_ContactInfoLinks_LeadId",
                table: "ContactInfoLinks",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactInfoLinks_OwnerType_OwnerId",
                table: "ContactInfoLinks",
                columns: new[] { "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_LookupItems_LookupCategoryId_SortOrder",
                table: "LookupItems",
                columns: new[] { "LookupCategoryId", "SortOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_LookupItems_CurrencyLookupId",
                table: "Accounts",
                column: "CurrencyLookupId",
                principalTable: "LookupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Contacts_PreferredContactMethodLookupId",
                table: "Contacts",
                column: "PreferredContactMethodLookupId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_LookupItems_PreferredContactMethodLookupId1",
                table: "Contacts",
                column: "PreferredContactMethodLookupId1",
                principalTable: "LookupItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_LookupItems_BillingCycleLookupId",
                table: "Customers",
                column: "BillingCycleLookupId",
                principalTable: "LookupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_LookupItems_CurrencyLookupId",
                table: "Customers",
                column: "CurrencyLookupId",
                principalTable: "LookupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_LookupItems_CurrencyLookupId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Contacts_PreferredContactMethodLookupId",
                table: "Contacts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_LookupItems_PreferredContactMethodLookupId1",
                table: "Contacts");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_LookupItems_BillingCycleLookupId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_LookupItems_CurrencyLookupId",
                table: "Customers");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "ContactDetails");

            migrationBuilder.DropTable(
                name: "ContactInfoLinks");

            migrationBuilder.DropTable(
                name: "LookupItems");

            migrationBuilder.DropTable(
                name: "SocialAccounts");

            migrationBuilder.DropTable(
                name: "LookupCategories");

            migrationBuilder.DropIndex(
                name: "IX_Customers_BillingCycleLookupId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_CurrencyLookupId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_PreferredContactMethodLookupId",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_PreferredContactMethodLookupId1",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_CurrencyLookupId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "BillingCycleLookupId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CurrencyLookupId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PreferredContactMethodLookupId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "PreferredContactMethodLookupId1",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "CurrencyLookupId",
                table: "Accounts");
        }
    }
}
