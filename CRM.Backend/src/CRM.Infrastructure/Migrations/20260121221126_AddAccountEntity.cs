using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MRR = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ARR = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    OneTimeFee = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Currency = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingCycle = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    BillingEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ContractReference = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContractStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ContractEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TermCategory = table.Column<int>(type: "int", nullable: true),
                    ServiceTier = table.Column<int>(type: "int", nullable: true),
                    SLA = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContractNotes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingCity = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingState = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingZip = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingCountry = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingContactName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingContactEmail = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingContactPhone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContractFileName = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContractFilePath = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContractContentType = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContractFileSize = table.Column<long>(type: "bigint", nullable: true),
                    IsAutoRenew = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RenewalDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AccountOwner = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountManagerId = table.Column<int>(type: "int", nullable: true),
                    Tags = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalReference = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Accounts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_AccountId",
                table: "Opportunities",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccountNumber",
                table: "Accounts",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CustomerId",
                table: "Accounts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ProductId",
                table: "Accounts",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Accounts_AccountId",
                table: "Opportunities",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Accounts_AccountId",
                table: "Opportunities");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_AccountId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Opportunities");
        }
    }
}
