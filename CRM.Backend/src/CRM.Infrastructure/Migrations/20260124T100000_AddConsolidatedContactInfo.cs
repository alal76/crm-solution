using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <summary>
    /// Migration to implement consolidated contact information tables
    /// Addresses, PhoneNumbers, EmailAddresses, and SocialMediaAccounts are now shared
    /// between Customers, Contacts, Leads, and Accounts via junction tables.
    /// </summary>
    public partial class AddConsolidatedContactInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================================
            // MASTER TABLES
            // ============================================================

            // Create PhoneNumbers table
            migrationBuilder.CreateTable(
                name: "PhoneNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    CountryCode = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false, defaultValue: "+1"),
                    AreaCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    Number = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    Extension = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    FormattedNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    CanSMS = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    CanWhatsApp = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    CanFax = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    IsVerified = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    VerifiedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    BestTimeToCall = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhoneNumbers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhoneNumbers_Number",
                table: "PhoneNumbers",
                column: "Number");

            migrationBuilder.CreateIndex(
                name: "IX_PhoneNumbers_IsDeleted",
                table: "PhoneNumbers",
                column: "IsDeleted");

            // Create EmailAddresses table
            migrationBuilder.CreateTable(
                name: "EmailAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    IsVerified = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    VerifiedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    BounceCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastBounceDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    HardBounce = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    LastEmailSent = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastEmailOpened = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EmailEngagementScore = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAddresses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailAddresses_Email",
                table: "EmailAddresses",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailAddresses_IsDeleted",
                table: "EmailAddresses",
                column: "IsDeleted");

            // Create SocialMediaAccounts table
            migrationBuilder.CreateTable(
                name: "SocialMediaAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Platform = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    PlatformOther = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    AccountType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, defaultValue: "Personal"),
                    HandleOrUsername = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    ProfileUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    DisplayName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    FollowerCount = table.Column<int>(type: "int", nullable: true),
                    FollowingCount = table.Column<int>(type: "int", nullable: true),
                    IsVerifiedAccount = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    LastActivityDate = table.Column<DateTime>(type: "date", nullable: true),
                    EngagementLevel = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialMediaAccounts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SocialMediaAccounts_Platform",
                table: "SocialMediaAccounts",
                column: "Platform");

            migrationBuilder.CreateIndex(
                name: "IX_SocialMediaAccounts_HandleOrUsername",
                table: "SocialMediaAccounts",
                column: "HandleOrUsername");

            // Add new columns to Addresses table
            migrationBuilder.AddColumn<string>(
                name: "Line3",
                table: "Addresses",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Addresses",
                type: "char(2)",
                maxLength: 2,
                nullable: true,
                defaultValue: "US");

            migrationBuilder.AddColumn<string>(
                name: "GeocodeAccuracy",
                table: "Addresses",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Addresses",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedDate",
                table: "Addresses",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationSource",
                table: "Addresses",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsResidential",
                table: "Addresses",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryInstructions",
                table: "Addresses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccessHours",
                table: "Addresses",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteContactName",
                table: "Addresses",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteContactPhone",
                table: "Addresses",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Addresses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Addresses",
                type: "int",
                nullable: true);

            // ============================================================
            // JUNCTION TABLES
            // ============================================================

            // Create EntityAddressLinks table
            migrationBuilder.CreateTable(
                name: "EntityAddressLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AddressId = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    AddressType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, defaultValue: "Primary"),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ValidFrom = table.Column<DateTime>(type: "date", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityAddressLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityAddressLinks_Addresses",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntityAddressLinks_AddressId",
                table: "EntityAddressLinks",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityAddressLinks_Entity",
                table: "EntityAddressLinks",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityAddressLinks_Unique",
                table: "EntityAddressLinks",
                columns: new[] { "EntityType", "EntityId", "AddressId", "AddressType" },
                unique: true);

            // Create EntityPhoneLinks table
            migrationBuilder.CreateTable(
                name: "EntityPhoneLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PhoneId = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    PhoneType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, defaultValue: "Office"),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DoNotCall = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ValidFrom = table.Column<DateTime>(type: "date", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityPhoneLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityPhoneLinks_PhoneNumbers",
                        column: x => x.PhoneId,
                        principalTable: "PhoneNumbers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntityPhoneLinks_PhoneId",
                table: "EntityPhoneLinks",
                column: "PhoneId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityPhoneLinks_Entity",
                table: "EntityPhoneLinks",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityPhoneLinks_Unique",
                table: "EntityPhoneLinks",
                columns: new[] { "EntityType", "EntityId", "PhoneId", "PhoneType" },
                unique: true);

            // Create EntityEmailLinks table
            migrationBuilder.CreateTable(
                name: "EntityEmailLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmailId = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    EmailType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, defaultValue: "General"),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DoNotEmail = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    UnsubscribedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MarketingOptIn = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    TransactionalOnly = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ValidFrom = table.Column<DateTime>(type: "date", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityEmailLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityEmailLinks_EmailAddresses",
                        column: x => x.EmailId,
                        principalTable: "EmailAddresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntityEmailLinks_EmailId",
                table: "EntityEmailLinks",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityEmailLinks_Entity",
                table: "EntityEmailLinks",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityEmailLinks_Unique",
                table: "EntityEmailLinks",
                columns: new[] { "EntityType", "EntityId", "EmailId", "EmailType" },
                unique: true);

            // Create EntitySocialMediaLinks table
            migrationBuilder.CreateTable(
                name: "EntitySocialMediaLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SocialMediaAccountId = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    PreferredForContact = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntitySocialMediaLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntitySocialMediaLinks_SocialMediaAccounts",
                        column: x => x.SocialMediaAccountId,
                        principalTable: "SocialMediaAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntitySocialMediaLinks_SocialMediaAccountId",
                table: "EntitySocialMediaLinks",
                column: "SocialMediaAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_EntitySocialMediaLinks_Entity",
                table: "EntitySocialMediaLinks",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_EntitySocialMediaLinks_Unique",
                table: "EntitySocialMediaLinks",
                columns: new[] { "EntityType", "EntityId", "SocialMediaAccountId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop junction tables
            migrationBuilder.DropTable(name: "EntitySocialMediaLinks");
            migrationBuilder.DropTable(name: "EntityEmailLinks");
            migrationBuilder.DropTable(name: "EntityPhoneLinks");
            migrationBuilder.DropTable(name: "EntityAddressLinks");

            // Drop master tables
            migrationBuilder.DropTable(name: "SocialMediaAccounts");
            migrationBuilder.DropTable(name: "EmailAddresses");
            migrationBuilder.DropTable(name: "PhoneNumbers");

            // Remove added columns from Addresses
            migrationBuilder.DropColumn(name: "Line3", table: "Addresses");
            migrationBuilder.DropColumn(name: "CountryCode", table: "Addresses");
            migrationBuilder.DropColumn(name: "GeocodeAccuracy", table: "Addresses");
            migrationBuilder.DropColumn(name: "IsVerified", table: "Addresses");
            migrationBuilder.DropColumn(name: "VerifiedDate", table: "Addresses");
            migrationBuilder.DropColumn(name: "VerificationSource", table: "Addresses");
            migrationBuilder.DropColumn(name: "IsResidential", table: "Addresses");
            migrationBuilder.DropColumn(name: "DeliveryInstructions", table: "Addresses");
            migrationBuilder.DropColumn(name: "AccessHours", table: "Addresses");
            migrationBuilder.DropColumn(name: "SiteContactName", table: "Addresses");
            migrationBuilder.DropColumn(name: "SiteContactPhone", table: "Addresses");
            migrationBuilder.DropColumn(name: "CreatedBy", table: "Addresses");
            migrationBuilder.DropColumn(name: "UpdatedBy", table: "Addresses");
        }
    }
}
