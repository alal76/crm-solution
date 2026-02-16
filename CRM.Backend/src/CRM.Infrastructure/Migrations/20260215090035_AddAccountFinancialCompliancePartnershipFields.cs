using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations.Auto
{
    /// <inheritdoc />
    public partial class AddAccountFinancialCompliancePartnershipFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Users_AccountManagerId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Users_AssignedToUserId",
                table: "Accounts");

            migrationBuilder.AddColumn<int>(
                name: "ActiveSubscriptionCount",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualRecurringRevenue",
                table: "Accounts",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageOrderValue",
                table: "Accounts",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessLicense",
                table: "Accounts",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CompetitorAccountId",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ComplianceCheckDate",
                table: "Accounts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplianceNotes",
                table: "Accounts",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "ContractValue",
                table: "Accounts",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "Accounts",
                type: "VARCHAR(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DunsNumber",
                table: "Accounts",
                type: "VARCHAR(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "IntegrationPartnerType",
                table: "Accounts",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsIntegrationPartner",
                table: "Accounts",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPartner",
                table: "Accounts",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReseller",
                table: "Accounts",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPaymentDate",
                table: "Accounts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LifetimeValue",
                table: "Accounts",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyRecurringRevenue",
                table: "Accounts",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NdaReferenceId",
                table: "Accounts",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "NdaSigned",
                table: "Accounts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NdaSignedDate",
                table: "Accounts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentResellerAccountId",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PartnerEnrolledDate",
                table: "Accounts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerStatus",
                table: "Accounts",
                type: "VARCHAR(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PartnerTier",
                table: "Accounts",
                type: "VARCHAR(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Accounts",
                type: "VARCHAR(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "RequiresNda",
                table: "Accounts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TechStack",
                table: "Accounts",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "TotalInvoiceCount",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationDate",
                table: "Accounts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationMethod",
                table: "Accounts",
                type: "VARCHAR(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "VerificationStatus",
                table: "Accounts",
                type: "VARCHAR(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "VerifiedByUserId",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CompetitorAccountId",
                table: "Accounts",
                column: "CompetitorAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ParentResellerAccountId",
                table: "Accounts",
                column: "ParentResellerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_VerifiedByUserId",
                table: "Accounts",
                column: "VerifiedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Accounts_CompetitorAccountId",
                table: "Accounts",
                column: "CompetitorAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Accounts_ParentResellerAccountId",
                table: "Accounts",
                column: "ParentResellerAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Users_AccountManagerId",
                table: "Accounts",
                column: "AccountManagerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Users_AssignedToUserId",
                table: "Accounts",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Users_VerifiedByUserId",
                table: "Accounts",
                column: "VerifiedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Accounts_CompetitorAccountId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Accounts_ParentResellerAccountId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Users_AccountManagerId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Users_AssignedToUserId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Users_VerifiedByUserId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_CompetitorAccountId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_ParentResellerAccountId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_VerifiedByUserId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ActiveSubscriptionCount",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "AnnualRecurringRevenue",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "AverageOrderValue",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "BusinessLicense",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "CompetitorAccountId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ComplianceCheckDate",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ComplianceNotes",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ContractValue",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "DunsNumber",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IntegrationPartnerType",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsIntegrationPartner",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsPartner",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsReseller",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "LastPaymentDate",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "LifetimeValue",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "MonthlyRecurringRevenue",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "NdaReferenceId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "NdaSigned",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "NdaSignedDate",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ParentResellerAccountId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PartnerEnrolledDate",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PartnerStatus",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PartnerTier",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "RequiresNda",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "TechStack",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "TotalInvoiceCount",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "VerificationDate",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "VerificationMethod",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "VerifiedByUserId",
                table: "Accounts");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Users_AccountManagerId",
                table: "Accounts",
                column: "AccountManagerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Users_AssignedToUserId",
                table: "Accounts",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
