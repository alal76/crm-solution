using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContactTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContactType = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    EmailPrimary = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    EmailSecondary = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    PhonePrimary = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PhoneSecondary = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ZipCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    JobTitle = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Department = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Company = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ReportsTo = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SocialMediaLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContactId = table.Column<int>(type: "INTEGER", nullable: false),
                    Platform = table.Column<int>(type: "INTEGER", nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Handle = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DateAdded = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialMediaLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialMediaLinks_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_FirstName",
                table: "Contacts",
                column: "FirstName");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_LastName",
                table: "Contacts",
                column: "LastName");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_ContactType",
                table: "Contacts",
                column: "ContactType");

            migrationBuilder.CreateIndex(
                name: "IX_SocialMediaLinks_ContactId",
                table: "SocialMediaLinks",
                column: "ContactId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SocialMediaLinks");

            migrationBuilder.DropTable(
                name: "Contacts");
        }
    }
}
