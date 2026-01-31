using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteToCashAndSalesPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommunicationMessages_CommunicationChannels_ChannelId",
                table: "CommunicationMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_CommunicationMessages_CommunicationMessages_ParentMessageId",
                table: "CommunicationMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_CommunicationMessages_Conversations_ConversationId1",
                table: "CommunicationMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_MarketingCampaigns_MarketingCampaignId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_MarketingCampaigns_MarketingCampaignId1",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_MarketingCampaigns_MarketingCampaignId2",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Accounts_AccountId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Accounts_AccountId1",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_AccountId1",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Leads_MarketingCampaignId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_MarketingCampaignId1",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_MarketingCampaignId2",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "AccountId1",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "MarketingCampaignId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "MarketingCampaignId1",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "MarketingCampaignId2",
                table: "Leads");

            migrationBuilder.RenameColumn(
                name: "ConversationId1",
                table: "CommunicationMessages",
                newName: "CommunicationChannelId");

            migrationBuilder.RenameIndex(
                name: "IX_CommunicationMessages_ConversationId1",
                table: "CommunicationMessages",
                newName: "IX_CommunicationMessages_CommunicationChannelId");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WorkflowVersions",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WorkflowTransitions",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WorkflowTasks",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WorkflowNodes",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WorkflowNodeInstances",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WorkflowLogs",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WorkflowInstances",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WorkflowDefinitions",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<bool>(
                name: "CompactMode",
                table: "Users",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DateFormat",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "DesktopNotifications",
                table: "Users",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailNotifications",
                table: "Users",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Users",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<int>(
                name: "RowsPerPage",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThemePreference",
                table: "Users",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TimeFormat",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Timezone",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "UserProfiles",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "UserGroups",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "UserGroupMembers",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "UserApprovalRequests",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "UserApprovalRequests",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<string>(
                name: "ActiveDatabaseProvider",
                table: "SystemSettings",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CompanyAddresses",
                table: "SystemSettings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CompanyDescription",
                table: "SystemSettings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CompanyEmails",
                table: "SystemSettings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CompanyFullName",
                table: "SystemSettings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CompanyIndustry",
                table: "SystemSettings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CompanyLegalName",
                table: "SystemSettings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CompanyLoginLogoUrl",
                table: "SystemSettings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CompanyPhones",
                table: "SystemSettings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CompanyRegistrationNumber",
                table: "SystemSettings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CompanyTaxId",
                table: "SystemSettings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultTaxRate",
                table: "SystemSettings",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "MariaDbEnabled",
                table: "SystemSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MySqlEnabled",
                table: "SystemSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PostgreSqlEnabled",
                table: "SystemSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "QuoteNumberPrefix",
                table: "SystemSettings",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "QuoteNumberSequence",
                table: "SystemSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "QuoteTermsAndConditions",
                table: "SystemSettings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "QuoteValidityDays",
                table: "SystemSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SystemSettings",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<bool>(
                name: "SqlServerEnabled",
                table: "SystemSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SqliteEnabled",
                table: "SystemSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SocialMediaFollows",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SocialMediaAccounts",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SocialAccounts",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ServiceRequestTypes",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ServiceRequestSubcategories",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ServiceRequests",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ServiceRequestCustomFieldValues",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ServiceRequestCustomFieldDefinitions",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ServiceRequestCategories",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualDeliveryDate",
                table: "Quotes",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedDeliveryDate",
                table: "Quotes",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RelationshipManagerId",
                table: "Quotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Quotes",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "ServiceEndDate",
                table: "Quotes",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ServiceStartDate",
                table: "Quotes",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedForApprovalDate",
                table: "Quotes",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WarrantyEndDate",
                table: "Quotes",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarrantyMonths",
                table: "Quotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Products",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PhoneNumbers",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Opportunities",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "OAuthTokens",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<string>(
                name: "ContextPath",
                table: "Notes",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "Notes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "Notes",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "LeadId",
                table: "Notes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuoteId",
                table: "Notes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Notes",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ModuleUIConfigs",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ModuleFieldConfigurations",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "MarketingCampaigns",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "LookupItems",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "LookupCategories",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Localities",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Leads",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Interactions",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "HealthCheckLogs",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "FieldMasterDataLinks",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EntityTags",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EntitySocialMediaLinks",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EntityPhoneLinks",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EntityEmailLinks",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EntityAddressLinks",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EmailTemplates",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EmailAddresses",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "DeploymentAttempts",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Departments",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "DatabaseBackups",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "DashboardWidgets",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Dashboards",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CustomFields",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Customers",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CustomerContacts",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CrmTasks",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "Conversations",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ParticipantName",
                table: "Conversations",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ParticipantAddress",
                table: "Conversations",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "LastMessagePreview",
                table: "Conversations",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ConversationId",
                table: "Conversations",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Conversations",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ContactInfoLinks",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ContactDetails",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<string>(
                name: "ToName",
                table: "CommunicationMessages",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ToAddress",
                table: "CommunicationMessages",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "CommunicationMessages",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "FromName",
                table: "CommunicationMessages",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "FromAddress",
                table: "CommunicationMessages",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalMessageId",
                table: "CommunicationMessages",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ConversationId",
                table: "CommunicationMessages",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CommunicationMessages",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CommunicationChannels",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ColorPalettes",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CloudProviders",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CloudDeployments",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CampaignMetrics",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "BackupSchedules",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Addresses",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Activities",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<int>(
                name: "PriceBookId",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Accounts",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.CreateTable(
                name: "AccountHealthSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OverallHealthScore = table.Column<int>(type: "int", nullable: false),
                    EngagementScore = table.Column<int>(type: "int", nullable: false),
                    ProductAdoptionScore = table.Column<int>(type: "int", nullable: false),
                    SupportSatisfactionScore = table.Column<int>(type: "int", nullable: false),
                    FinancialHealthScore = table.Column<int>(type: "int", nullable: false),
                    RelationshipScore = table.Column<int>(type: "int", nullable: false),
                    ActiveUsersCount = table.Column<int>(type: "int", nullable: true),
                    FeatureAdoptionRate = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    SupportTicketsCount = table.Column<int>(type: "int", nullable: true),
                    SupportTicketsResolved = table.Column<int>(type: "int", nullable: true),
                    AverageResponseTimeHours = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    NPSScore = table.Column<int>(type: "int", nullable: true),
                    RiskFactors = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WarningSignals = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GrowthIndicators = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AnalystNotes = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PreviousHealthScore = table.Column<int>(type: "int", nullable: true),
                    HealthTrend = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountHealthSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountHealthSnapshots_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AccountTerritories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TerritoryName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TerritoryCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Countries = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Regions = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    States = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cities = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Industries = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerTypes = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RevenueRangeMin = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    RevenueRangeMax = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    PrimaryOwnerId = table.Column<int>(type: "int", nullable: true),
                    TeamMemberIds = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AnnualQuota = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    QuotaCurrency = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetAccountCount = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountTerritories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountTerritories_Users_PrimaryOwnerId",
                        column: x => x.PrimaryOwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ApprovalGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalGroups", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AttributionSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DefaultRevenueModel = table.Column<int>(type: "int", nullable: false),
                    DefaultConversionModel = table.Column<int>(type: "int", nullable: false),
                    AttributionWindowDays = table.Column<int>(type: "int", nullable: false),
                    TimeDecayHalfLifeDays = table.Column<int>(type: "int", nullable: false),
                    IncludeAnonymousTouchpoints = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CustomModelWeights = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChannelGroupingRules = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributionSettings", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CampaignABTests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    TestName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TestType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TestMetric = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrafficSplit = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SampleSize = table.Column<int>(type: "int", nullable: true),
                    SamplePercentage = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    VariantConfigs = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WinnerVariant = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WinningCriteria = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConfidenceLevel = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    TestStartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TestCompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    WinnerDeployedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AutoSelectWinner = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AutoWinnerAfterHours = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignABTests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignABTests_MarketingCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "MarketingCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CampaignAttributionSummaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PeriodStart = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    Model = table.Column<int>(type: "int", nullable: false),
                    TotalTouchpoints = table.Column<int>(type: "int", nullable: false),
                    UniquLeads = table.Column<int>(type: "int", nullable: false),
                    AttributedConversions = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    AttributedRevenue = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    AttributedPipeline = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CostPerConversion = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ROAS = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    CampaignCost = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    FirstTouchConversions = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    LastTouchConversions = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    AvgTouchpointsToConversion = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    AvgDaysToConversion = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignAttributionSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignAttributionSummaries_MarketingCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "MarketingCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CampaignRecipients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Company = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SendScheduledTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SendActualTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FirstOpenedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastOpenedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    OpenCount = table.Column<int>(type: "int", nullable: false),
                    FirstClickedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastClickedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ClickCount = table.Column<int>(type: "int", nullable: false),
                    ConvertedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ConversionValue = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    UnsubscribedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    BounceType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BounceReason = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PersonalizationData = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ABTestVariant = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignRecipients_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CampaignRecipients_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CampaignRecipients_MarketingCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "MarketingCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CampaignWorkflows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    WorkflowDefinitionId = table.Column<int>(type: "int", nullable: false),
                    WorkflowType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TriggerEvent = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TriggerConditions = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    MaxExecutionsPerContact = table.Column<int>(type: "int", nullable: false),
                    CooldownHours = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignWorkflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignWorkflows_MarketingCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "MarketingCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignWorkflows_WorkflowDefinitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CommissionPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Code = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EffectiveStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EffectiveEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FiscalYear = table.Column<int>(type: "int", nullable: true),
                    CommissionType = table.Column<int>(type: "int", nullable: false),
                    BaseRate = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Trigger = table.Column<int>(type: "int", nullable: false),
                    ClawbackPeriodDays = table.Column<int>(type: "int", nullable: true),
                    MinDealSize = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    MaxCommissionPerDeal = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    MaxCommissionPerPeriod = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    AllowSplits = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DefaultOverlayPercent = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ManagerOverridePercent = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    TierRates = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AppliesToAllProducts = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProductCategories = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductIds = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductRates = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionPlans", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CommissionStatements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StatementNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Period = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PeriodStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TotalEarned = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TotalAdjustments = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TotalClawbacks = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    NetPayout = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsPaid = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PaymentReference = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatementUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GeneratedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionStatements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissionStatements_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DiscountApprovalMatrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    AppliesToAllProducts = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProductCategories = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerSegments = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Regions = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequireAllLevels = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowParallelApproval = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AutoEscalateHours = table.Column<int>(type: "int", nullable: true),
                    ReminderHours = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountApprovalMatrices", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DuplicateRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EntityType = table.Column<int>(type: "int", nullable: false),
                    MatchThreshold = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    RunOnCreate = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RunOnUpdate = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RunOnImport = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    EnableBatchScan = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    BatchScanFrequency = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastBatchScanDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    NextBatchScanDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TotalDuplicatesFound = table.Column<int>(type: "int", nullable: false),
                    TotalDuplicatesMerged = table.Column<int>(type: "int", nullable: false),
                    TotalFalsePositives = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuplicateRules", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailSequences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FromEmail = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FromName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReplyToEmail = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SenderId = table.Column<int>(type: "int", nullable: true),
                    SendFromOwner = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Timezone = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SendingDays = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SendingStartHour = table.Column<int>(type: "int", nullable: false),
                    SendingEndHour = table.Column<int>(type: "int", nullable: false),
                    MaxEmailsPerDay = table.Column<int>(type: "int", nullable: true),
                    ThrottleMinutes = table.Column<int>(type: "int", nullable: true),
                    ExitConditions = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExitOnReply = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ExitOnMeetingBooked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ExitOnBounce = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ExitOnUnsubscribe = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TotalEnrolled = table.Column<int>(type: "int", nullable: false),
                    ActiveEnrollments = table.Column<int>(type: "int", nullable: false),
                    TotalCompleted = table.Column<int>(type: "int", nullable: false),
                    TotalEmailsSent = table.Column<int>(type: "int", nullable: false),
                    TotalOpens = table.Column<int>(type: "int", nullable: false),
                    TotalClicks = table.Column<int>(type: "int", nullable: false),
                    TotalReplies = table.Column<int>(type: "int", nullable: false),
                    TotalBounces = table.Column<int>(type: "int", nullable: false),
                    TotalUnsubscribes = table.Column<int>(type: "int", nullable: false),
                    TotalMeetingsBooked = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSequences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailSequences_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmailSequences_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ForecastHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SnapshotDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Period = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    TeamId = table.Column<int>(type: "int", nullable: true),
                    QuotaAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ClosedWonAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CommitAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    BestCaseAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    PipelineAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    WeeksRemaining = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastHistories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "llm_provider_settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    setting_key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    setting_value = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    value_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, defaultValue: "string")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, defaultValue: "general")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_encrypted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_llm_provider_settings", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrderNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalOrderId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerPONumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReferenceNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OrderType = table.Column<int>(type: "int", nullable: false),
                    FulfillmentMethod = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RequestedDeliveryDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PromisedDeliveryDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ShippedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeliveredDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ContractStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ContractEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DiscountReason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaxAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ShippingAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    HandlingAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExchangeRate = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    BaseCurrencyAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    MRR = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ARR = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    TCV = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ACV = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    OneTimeRevenue = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    RecurringRevenue = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    BillingName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingCompany = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingStreet = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingCity = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingState = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingPostalCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingCountry = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingPhone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingEmail = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShippingName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShippingCompany = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShippingStreet = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShippingCity = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShippingState = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShippingPostalCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShippingCountry = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShippingPhone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShippingEmail = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShippingInstructions = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShippingMethod = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrackingNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrackingUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShippingWeight = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    PackageCount = table.Column<int>(type: "int", nullable: true),
                    PaymentTerms = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentMethod = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AmountInvoiced = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    QuoteId = table.Column<int>(type: "int", nullable: true),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    OpportunityId = table.Column<int>(type: "int", nullable: true),
                    OwnerId = table.Column<int>(type: "int", nullable: true),
                    ApprovedById = table.Column<int>(type: "int", nullable: true),
                    ParentOrderId = table.Column<int>(type: "int", nullable: true),
                    InternalNotes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SpecialInstructions = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CancellationReason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TermsAndConditions = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Source = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceIpAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_Orders_ParentOrderId",
                        column: x => x.ParentOrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_Users_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PriceBooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Code = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsStandard = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Countries = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerSegment = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Channel = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EffectiveStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceBooks", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PricingRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RuleType = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    AppliesToAllProducts = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProductIds = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductCategories = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerIds = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerSegments = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DiscountMethod = table.Column<int>(type: "int", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    FixedPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    MinOrderAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    MinQuantity = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    MaxDiscountAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    VolumeTiers = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EffectiveStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UsageLimit = table.Column<int>(type: "int", nullable: true),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    Conditions = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CombineWithOtherRules = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingRules", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductBundles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SKU = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BundleCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShortDescription = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PricingType = table.Column<int>(type: "int", nullable: false),
                    FixedPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    MinimumPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    MaxDiscountPercent = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ListPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    CurrencyCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MinItems = table.Column<int>(type: "int", nullable: true),
                    MaxItems = table.Column<int>(type: "int", nullable: true),
                    AllowQuantityChange = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ShowComponentPrices = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowPartialConfiguration = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EffectiveStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ImageUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsFeatured = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Tags = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBundles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QuoteLineItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    QuoteId = table.Column<int>(type: "int", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    SKU = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ListPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CostPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DiscountType = table.Column<int>(type: "int", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountReason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DiscountRequiresApproval = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DiscountApproved = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IsTaxable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TaxCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalDiscount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Margin = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    BillingPeriod = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WarrantyMonths = table.Column<int>(type: "int", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ServiceStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ServiceEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsOptional = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsIncluded = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ParentLineItemId = table.Column<int>(type: "int", nullable: true),
                    IsBundle = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    InternalNotes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QuoteNotes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomFields = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteLineItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_QuoteLineItems_QuoteLineItems_ParentLineItemId",
                        column: x => x.ParentLineItemId,
                        principalTable: "QuoteLineItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuoteLineItems_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RelationshipMaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MapName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CentralCustomerId = table.Column<int>(type: "int", nullable: true),
                    RelationshipDepth = table.Column<int>(type: "int", nullable: false),
                    IncludeRelationshipTypeIds = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExcludeRelationshipTypeIds = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MinRelationshipStrength = table.Column<int>(type: "int", nullable: false),
                    IncludeStatuses = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateRangeStart = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateRangeEnd = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LayoutConfig = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ViewSettings = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsPublic = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SharedWithUserIds = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SharedWithGroupIds = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelationshipMaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelationshipMaps_Customers_CentralCustomerId",
                        column: x => x.CentralCustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RelationshipTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TypeName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TypeCategory = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsBidirectional = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReverseTypeName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icon = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsSystem = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelationshipTypes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Code = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ManagerId = table.Column<int>(type: "int", nullable: true),
                    ParentTeamId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Teams_ParentTeamId",
                        column: x => x.ParentTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Teams_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebVisitors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VisitorId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FingerprintId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsIdentified = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IdentificationSource = table.Column<int>(type: "int", nullable: false),
                    IdentifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Company = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Industry = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompanySize = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompanyDomain = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Country = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Region = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PostalCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Timezone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitude = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Browser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BrowserVersion = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OperatingSystem = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ScreenResolution = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserAgent = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Language = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstReferrer = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstLandingPage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstUtmSource = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstUtmMedium = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstUtmCampaign = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstUtmContent = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstUtmTerm = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastReferrer = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastLandingPage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastUtmSource = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastUtmMedium = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastUtmCampaign = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastUtmContent = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastUtmTerm = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstVisitAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastVisitAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TotalSessions = table.Column<int>(type: "int", nullable: false),
                    TotalPageViews = table.Column<int>(type: "int", nullable: false),
                    TotalTimeOnSite = table.Column<int>(type: "int", nullable: false),
                    FormsSubmitted = table.Column<int>(type: "int", nullable: false),
                    FilesDownloaded = table.Column<int>(type: "int", nullable: false),
                    VideosWatched = table.Column<int>(type: "int", nullable: false),
                    BehaviorScore = table.Column<int>(type: "int", nullable: false),
                    FitScore = table.Column<int>(type: "int", nullable: false),
                    TotalScore = table.Column<int>(type: "int", nullable: false),
                    InterestTopics = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BuyingStage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LeadId = table.Column<int>(type: "int", nullable: true),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    AccountId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebVisitors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebVisitors_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WebVisitors_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WebVisitors_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustomerTerritoryAssignments",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    TerritoryId = table.Column<int>(type: "int", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AssignedBy = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssignedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerTerritoryAssignments", x => new { x.CustomerId, x.TerritoryId });
                    table.ForeignKey(
                        name: "FK_CustomerTerritoryAssignments_AccountTerritories_TerritoryId",
                        column: x => x.TerritoryId,
                        principalTable: "AccountTerritories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerTerritoryAssignments_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerTerritoryAssignments_Users_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ApprovalGroupMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ApprovalGroupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalGroupMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalGroupMembers_ApprovalGroups_ApprovalGroupId",
                        column: x => x.ApprovalGroupId,
                        principalTable: "ApprovalGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApprovalGroupMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CampaignConversions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    CampaignRecipientId = table.Column<int>(type: "int", nullable: true),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    ConversionType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConversionValue = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ConversionCurrency = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AttributionModel = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AttributionPercentage = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ConversionData = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConvertedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExternalOrderId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalTransactionId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignConversions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignConversions_CampaignRecipients_CampaignRecipientId",
                        column: x => x.CampaignRecipientId,
                        principalTable: "CampaignRecipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CampaignConversions_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CampaignConversions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CampaignConversions_MarketingCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "MarketingCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CampaignLinkClicks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CampaignRecipientId = table.Column<int>(type: "int", nullable: false),
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    LinkUrl = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LinkLabel = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClickedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserAgent = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Browser = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OperatingSystem = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LocationData = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignLinkClicks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignLinkClicks_CampaignRecipients_CampaignRecipientId",
                        column: x => x.CampaignRecipientId,
                        principalTable: "CampaignRecipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignLinkClicks_MarketingCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "MarketingCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CommissionPlanAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CommissionPlanId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RateOverride = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionPlanAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissionPlanAssignments_CommissionPlans_CommissionPlanId",
                        column: x => x.CommissionPlanId,
                        principalTable: "CommissionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommissionPlanAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CommissionTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TierOrder = table.Column<int>(type: "int", nullable: false),
                    MinAttainmentPercent = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    MaxAttainmentPercent = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    CommissionRate = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    FixedAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Multiplier = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CommissionPlanId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionTiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissionTiers_CommissionPlans_CommissionPlanId",
                        column: x => x.CommissionPlanId,
                        principalTable: "CommissionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ApprovalLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LevelOrder = table.Column<int>(type: "int", nullable: false),
                    ThresholdType = table.Column<int>(type: "int", nullable: false),
                    MinValue = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    MaxValue = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ApproverUserId = table.Column<int>(type: "int", nullable: true),
                    ApproverRole = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UseSubmitterManager = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ManagerLevelsUp = table.Column<int>(type: "int", nullable: false),
                    ApprovalGroupId = table.Column<int>(type: "int", nullable: true),
                    RequireAllGroupMembers = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CanSkip = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AutoApproveIfSelf = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TimeoutHours = table.Column<int>(type: "int", nullable: true),
                    EscalationUserId = table.Column<int>(type: "int", nullable: true),
                    SendEmailOnPending = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NotificationTemplateId = table.Column<int>(type: "int", nullable: true),
                    IncludeQuoteDetails = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DiscountApprovalMatrixId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalLevels_DiscountApprovalMatrices_DiscountApprovalMatr~",
                        column: x => x.DiscountApprovalMatrixId,
                        principalTable: "DiscountApprovalMatrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApprovalLevels_Users_ApproverUserId",
                        column: x => x.ApproverUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApprovalLevels_Users_EscalationUserId",
                        column: x => x.EscalationUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ApprovalRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RequestNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DiscountApprovalMatrixId = table.Column<int>(type: "int", nullable: true),
                    QuoteId = table.Column<int>(type: "int", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DealAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    MarginPercent = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Justification = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrentLevel = table.Column<int>(type: "int", nullable: false),
                    MaxLevelRequired = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TimeToApprovalHours = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    SubmitterId = table.Column<int>(type: "int", nullable: false),
                    FinalNotes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalRequests_DiscountApprovalMatrices_DiscountApprovalMa~",
                        column: x => x.DiscountApprovalMatrixId,
                        principalTable: "DiscountApprovalMatrices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApprovalRequests_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApprovalRequests_Users_SubmitterId",
                        column: x => x.SubmitterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DuplicateCandidates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EntityType = table.Column<int>(type: "int", nullable: false),
                    SourceRecordId = table.Column<int>(type: "int", nullable: false),
                    SourceRecordType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetRecordId = table.Column<int>(type: "int", nullable: false),
                    TargetRecordType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchScore = table.Column<int>(type: "int", nullable: false),
                    MatchingFields = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ComparisonData = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MergedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DuplicateRuleId = table.Column<int>(type: "int", nullable: true),
                    ReviewedById = table.Column<int>(type: "int", nullable: true),
                    MergedById = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuplicateCandidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuplicateCandidates_DuplicateRules_DuplicateRuleId",
                        column: x => x.DuplicateRuleId,
                        principalTable: "DuplicateRules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DuplicateCandidates_Users_MergedById",
                        column: x => x.MergedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DuplicateCandidates_Users_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DuplicateMatchFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FieldName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchType = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FuzzyTolerance = table.Column<int>(type: "int", nullable: true),
                    IgnoreNullValues = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Transform = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Order = table.Column<int>(type: "int", nullable: false),
                    DuplicateRuleId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuplicateMatchFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuplicateMatchFields_DuplicateRules_DuplicateRuleId",
                        column: x => x.DuplicateRuleId,
                        principalTable: "DuplicateRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailSequenceEnrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EnrolledAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExitReason = table.Column<int>(type: "int", nullable: true),
                    ExitNotes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrentStepIndex = table.Column<int>(type: "int", nullable: false),
                    CurrentStepId = table.Column<int>(type: "int", nullable: true),
                    NextStepScheduledAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastStepExecutedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    StepsCompleted = table.Column<int>(type: "int", nullable: false),
                    EmailsSent = table.Column<int>(type: "int", nullable: false),
                    RecipientEmail = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientTimezone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalOpens = table.Column<int>(type: "int", nullable: false),
                    TotalClicks = table.Column<int>(type: "int", nullable: false),
                    HasReplied = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RepliedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    HasBounced = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    BouncedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    HasUnsubscribed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UnsubscribedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MeetingBooked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MeetingBookedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EmailSequenceId = table.Column<int>(type: "int", nullable: false),
                    LeadId = table.Column<int>(type: "int", nullable: true),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    EnrolledById = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSequenceEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailSequenceEnrollments_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmailSequenceEnrollments_EmailSequences_EmailSequenceId",
                        column: x => x.EmailSequenceId,
                        principalTable: "EmailSequences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailSequenceEnrollments_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmailSequenceEnrollments_Users_EnrolledById",
                        column: x => x.EnrolledById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailSequenceSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StepType = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TimingMode = table.Column<int>(type: "int", nullable: false),
                    DelayDays = table.Column<int>(type: "int", nullable: false),
                    DelayHours = table.Column<int>(type: "int", nullable: false),
                    DelayMinutes = table.Column<int>(type: "int", nullable: false),
                    SpecificTime = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subject = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Body = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BodyPlainText = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailTemplateId = table.Column<int>(type: "int", nullable: true),
                    IsReply = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReplyToStepId = table.Column<int>(type: "int", nullable: true),
                    TaskTitle = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaskDescription = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaskPriority = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaskDueDays = table.Column<int>(type: "int", nullable: false),
                    ConditionType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConditionValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrueStepId = table.Column<int>(type: "int", nullable: true),
                    FalseStepId = table.Column<int>(type: "int", nullable: true),
                    IsABTest = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ABVariant = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ABSplitPercent = table.Column<int>(type: "int", nullable: true),
                    ExecutionCount = table.Column<int>(type: "int", nullable: false),
                    EmailsSent = table.Column<int>(type: "int", nullable: false),
                    Opens = table.Column<int>(type: "int", nullable: false),
                    Clicks = table.Column<int>(type: "int", nullable: false),
                    Replies = table.Column<int>(type: "int", nullable: false),
                    Bounces = table.Column<int>(type: "int", nullable: false),
                    EmailSequenceId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSequenceSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailSequenceSteps_EmailSequences_EmailSequenceId",
                        column: x => x.EmailSequenceId,
                        principalTable: "EmailSequences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ESignatureRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RequestNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalEnvelopeId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Provider = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    EmailSubject = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SentDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    VoidedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeclinedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastStatusUpdate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExpirationDays = table.Column<int>(type: "int", nullable: false),
                    ReminderDays = table.Column<int>(type: "int", nullable: true),
                    RemindersSent = table.Column<int>(type: "int", nullable: false),
                    RequireSigningOrder = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowDecline = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowComments = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AuthenticationMethod = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceDocumentUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SignedDocumentUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CertificateUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AuditTrailUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeclineReason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VoidReason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QuoteId = table.Column<int>(type: "int", nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    AccountId = table.Column<int>(type: "int", nullable: true),
                    OpportunityId = table.Column<int>(type: "int", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    VoidedById = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ESignatureRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ESignatureRequests_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ESignatureRequests_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ESignatureRequests_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ESignatureRequests_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ESignatureRequests_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ESignatureRequests_Users_VoidedById",
                        column: x => x.VoidedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SubscriptionNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalSubscriptionId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GatewaySubscriptionId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    BillingFrequency = table.Column<int>(type: "int", nullable: false),
                    ProrationType = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TrialStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TrialEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CancellationEffectiveDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CancellationReason = table.Column<int>(type: "int", nullable: true),
                    CancellationNotes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TermLengthMonths = table.Column<int>(type: "int", nullable: true),
                    CurrentTerm = table.Column<int>(type: "int", nullable: false),
                    CurrentPeriodStart = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CurrentPeriodEnd = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NextBillingDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastBillingDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    BillingCycleCount = table.Column<int>(type: "int", nullable: false),
                    BillingDayOfMonth = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    RecurringAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    SetupFee = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MRR = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ARR = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TCV = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    LifetimeValue = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TotalInvoiced = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TotalPaid = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    AutoRenew = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RenewalDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RenewalReminderSent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RenewalReminderSentDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RenewalReminderDays = table.Column<int>(type: "int", nullable: false),
                    RenewalCount = table.Column<int>(type: "int", nullable: false),
                    RenewalPriceChange = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    RenewalPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    DaysPastDue = table.Column<int>(type: "int", nullable: false),
                    FailedPaymentAttempts = table.Column<int>(type: "int", nullable: false),
                    LastFailedPaymentDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    GracePeriodEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DefaultPaymentMethodId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PreviousSubscriptionId = table.Column<int>(type: "int", nullable: true),
                    ChangeEffectiveDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ProrationCredit = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ProrationCharge = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    OpportunityId = table.Column<int>(type: "int", nullable: true),
                    OwnerId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Subscriptions_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Subscriptions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Subscriptions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Subscriptions_PreviousSubscriptionId",
                        column: x => x.PreviousSubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PriceBookEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ListPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    MinPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    MaxPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    StandardDiscount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EffectiveStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PriceBookId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceBookEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceBookEntries_PriceBooks_PriceBookId",
                        column: x => x.PriceBookId,
                        principalTable: "PriceBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PriceBookEntries_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PricingRuleUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PricingRuleId = table.Column<int>(type: "int", nullable: false),
                    QuoteId = table.Column<int>(type: "int", nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AppliedById = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingRuleUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PricingRuleUsages_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PricingRuleUsages_PricingRules_PricingRuleId",
                        column: x => x.PricingRuleId,
                        principalTable: "PricingRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PricingRuleUsages_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PricingRuleUsages_Users_AppliedById",
                        column: x => x.AppliedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductBundleItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    DefaultQuantity = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    MinQuantity = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    MaxQuantity = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    OverridePrice = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    IsFree = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CustomPricing = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExclusiveGroup = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDefaultSelected = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowQuantityChange = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowRemoval = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProductBundleId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBundleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductBundleItems_ProductBundles_ProductBundleId",
                        column: x => x.ProductBundleId,
                        principalTable: "ProductBundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductBundleItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductBundleRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RuleType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceProductId = table.Column<int>(type: "int", nullable: true),
                    TargetProductId = table.Column<int>(type: "int", nullable: true),
                    ErrorMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Condition = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductBundleId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBundleRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductBundleRules_ProductBundles_ProductBundleId",
                        column: x => x.ProductBundleId,
                        principalTable: "ProductBundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductBundleRules_Products_SourceProductId",
                        column: x => x.SourceProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductBundleRules_Products_TargetProductId",
                        column: x => x.TargetProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderLineItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    ExternalLineId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SKU = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ExtendedAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    QuantityShipped = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    QuantityInvoiced = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    QuantityReturned = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    FulfillmentStatus = table.Column<int>(type: "int", nullable: false),
                    EstimatedShipDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ShippedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    BillingFrequency = table.Column<int>(type: "int", nullable: true),
                    ServiceStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ServiceEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TermLengthMonths = table.Column<int>(type: "int", nullable: true),
                    AutoRenew = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    QuoteLineItemId = table.Column<int>(type: "int", nullable: true),
                    ParentLineItemId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InternalNotes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderLineItems_OrderLineItems_ParentLineItemId",
                        column: x => x.ParentLineItemId,
                        principalTable: "OrderLineItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderLineItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderLineItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderLineItems_QuoteLineItems_QuoteLineItemId",
                        column: x => x.QuoteLineItemId,
                        principalTable: "QuoteLineItems",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AccountRelationships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SourceCustomerId = table.Column<int>(type: "int", nullable: false),
                    TargetCustomerId = table.Column<int>(type: "int", nullable: false),
                    RelationshipTypeId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StrengthScore = table.Column<int>(type: "int", nullable: false),
                    StrategicImportance = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RelationshipStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RelationshipEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastReviewedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    NextReviewDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AnnualRevenueImpact = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    CostSavings = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TermsConditions = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountRelationships_Customers_SourceCustomerId",
                        column: x => x.SourceCustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountRelationships_Customers_TargetCustomerId",
                        column: x => x.TargetCustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountRelationships_RelationshipTypes_RelationshipTypeId",
                        column: x => x.RelationshipTypeId,
                        principalTable: "RelationshipTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountRelationships_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LeadRoutingRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    AssignmentType = table.Column<int>(type: "int", nullable: false),
                    AssignToTeam = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: true),
                    FallbackOwnerId = table.Column<int>(type: "int", nullable: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    BusinessHoursOnly = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Timezone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoundRobinPosition = table.Column<int>(type: "int", nullable: false),
                    LastAssignmentDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TotalLeadsAssigned = table.Column<int>(type: "int", nullable: false),
                    SendNotification = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NotificationTemplateId = table.Column<int>(type: "int", nullable: true),
                    NotifyManager = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadRoutingRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadRoutingRules_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeadRoutingRules_Users_FallbackOwnerId",
                        column: x => x.FallbackOwnerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SalesQuotas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PeriodType = table.Column<int>(type: "int", nullable: false),
                    Metric = table.Column<int>(type: "int", nullable: false),
                    Period = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FiscalYear = table.Column<int>(type: "int", nullable: false),
                    FiscalQuarter = table.Column<int>(type: "int", nullable: true),
                    FiscalMonth = table.Column<int>(type: "int", nullable: true),
                    PeriodStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TargetAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StretchTargetAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    MinimumTargetAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ActualAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    NewBusinessAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    RenewalAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ExpansionAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    TeamId = table.Column<int>(type: "int", nullable: true),
                    ParentQuotaId = table.Column<int>(type: "int", nullable: true),
                    LastRefreshedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesQuotas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesQuotas_SalesQuotas_ParentQuotaId",
                        column: x => x.ParentQuotaId,
                        principalTable: "SalesQuotas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesQuotas_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesQuotas_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TeamMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsTeamLead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamMembers_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SessionId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    PageViewCount = table.Column<int>(type: "int", nullable: false),
                    LandingPage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExitPage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Referrer = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UtmParameters = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WebVisitorId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebSessions_WebVisitors_WebVisitorId",
                        column: x => x.WebVisitorId,
                        principalTable: "WebVisitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ApprovalSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    ApprovalLevelId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AssignedToId = table.Column<int>(type: "int", nullable: true),
                    ActedById = table.Column<int>(type: "int", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ActedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DueAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Comments = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApprovalRequestId = table.Column<int>(type: "int", nullable: false),
                    ReminderSent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReminderSentAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    WasEscalated = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EscalatedToId = table.Column<int>(type: "int", nullable: true),
                    EscalatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalSteps_ApprovalLevels_ApprovalLevelId",
                        column: x => x.ApprovalLevelId,
                        principalTable: "ApprovalLevels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApprovalSteps_ApprovalRequests_ApprovalRequestId",
                        column: x => x.ApprovalRequestId,
                        principalTable: "ApprovalRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApprovalSteps_Users_ActedById",
                        column: x => x.ActedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApprovalSteps_Users_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DuplicateMergeHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EntityType = table.Column<int>(type: "int", nullable: false),
                    SurvivingRecordId = table.Column<int>(type: "int", nullable: false),
                    MergedRecordId = table.Column<int>(type: "int", nullable: false),
                    MergedRecordData = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FieldsFromMergedRecord = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RelinkedRecords = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MergedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MergedById = table.Column<int>(type: "int", nullable: true),
                    DuplicateCandidateId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuplicateMergeHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuplicateMergeHistories_DuplicateCandidates_DuplicateCandida~",
                        column: x => x.DuplicateCandidateId,
                        principalTable: "DuplicateCandidates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DuplicateMergeHistories_Users_MergedById",
                        column: x => x.MergedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailSequenceStepExecutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmailSequenceStepId = table.Column<int>(type: "int", nullable: false),
                    EmailSequenceEnrollmentId = table.Column<int>(type: "int", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Success = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MessageId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Opens = table.Column<int>(type: "int", nullable: false),
                    Clicks = table.Column<int>(type: "int", nullable: false),
                    Replied = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RepliedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Bounced = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    BounceType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSequenceStepExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailSequenceStepExecutions_EmailSequenceEnrollments_EmailSe~",
                        column: x => x.EmailSequenceEnrollmentId,
                        principalTable: "EmailSequenceEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailSequenceStepExecutions_EmailSequenceSteps_EmailSequence~",
                        column: x => x.EmailSequenceStepId,
                        principalTable: "EmailSequenceSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ESignatureDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DocumentOrder = table.Column<int>(type: "int", nullable: false),
                    ExternalDocumentId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DocumentUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    PageCount = table.Column<int>(type: "int", nullable: true),
                    ESignatureRequestId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ESignatureDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ESignatureDocuments_ESignatureRequests_ESignatureRequestId",
                        column: x => x.ESignatureRequestId,
                        principalTable: "ESignatureRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ESignatureSigners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SigningOrder = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExternalRecipientId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Company = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SentDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeliveredDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ViewedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SignedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeclinedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SignatureImageUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SignedFromIp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SignedFromLocation = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeclineReason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrivateMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ESignatureRequestId = table.Column<int>(type: "int", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ESignatureSigners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ESignatureSigners_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ESignatureSigners_ESignatureRequests_ESignatureRequestId",
                        column: x => x.ESignatureRequestId,
                        principalTable: "ESignatureRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ESignatureSigners_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InvoiceNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalInvoiceId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReferenceNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BatchNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    InvoiceType = table.Column<int>(type: "int", nullable: false),
                    PaymentTerms = table.Column<int>(type: "int", nullable: false),
                    PaymentTermsDescription = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InvoiceDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SentDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ViewedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PaidDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    VoidedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ServicePeriodStart = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ServicePeriodEnd = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ShippingAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    FeesAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    AmountCredited = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExchangeRate = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    EarlyPaymentDiscountPercent = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    EarlyPaymentDiscountDays = table.Column<int>(type: "int", nullable: true),
                    EarlyPaymentDiscountAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    LateFeePercent = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    LateFeeAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    LateFeeTotal = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    BillingName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingCompany = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingStreet = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingCity = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingState = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingPostalCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingCountry = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingEmail = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingPhone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReminderCount = table.Column<int>(type: "int", nullable: false),
                    LastReminderDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    NextReminderDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    InCollections = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CollectionsDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CollectionsReference = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: true),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    VoidedById = table.Column<int>(type: "int", nullable: true),
                    OriginalInvoiceId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InternalNotes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Footer = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TermsAndConditions = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VoidReason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisputeReason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PdfUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Invoices_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Invoices_Invoices_OriginalInvoiceId",
                        column: x => x.OriginalInvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Invoices_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Invoices_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Invoices_Users_VoidedById",
                        column: x => x.VoidedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SubscriptionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    BillingFrequency = table.Column<int>(type: "int", nullable: false),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubscriptionItems_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RelationshipInteractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountRelationshipId = table.Column<int>(type: "int", nullable: false),
                    InteractionType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subject = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InteractionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    ParticipantContactIds = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParticipantUserIds = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Outcome = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActionItems = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NextSteps = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FollowUpDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SentimentScore = table.Column<int>(type: "int", nullable: false),
                    HealthImpact = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Location = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MeetingLink = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelationshipInteractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelationshipInteractions_AccountRelationships_AccountRelatio~",
                        column: x => x.AccountRelationshipId,
                        principalTable: "AccountRelationships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FormDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FormKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subtitle = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubmitButtonText = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Width = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CssClasses = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomCss = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomJs = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Theme = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubmitAction = table.Column<int>(type: "int", nullable: false),
                    ThankYouMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RedirectUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DoubleOptIn = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DoubleOptInTemplateId = table.Column<int>(type: "int", nullable: true),
                    SpamProtection = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CaptchaType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HoneypotFieldName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreateLead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LeadSource = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultLeadOwnerId = table.Column<int>(type: "int", nullable: true),
                    LeadRoutingRuleId = table.Column<int>(type: "int", nullable: true),
                    UpdateExistingLead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ExistingLeadMatchField = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CampaignId = table.Column<int>(type: "int", nullable: true),
                    CampaignMemberStatus = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NotifyOwner = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NotificationRecipients = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NotificationTemplateId = table.Column<int>(type: "int", nullable: true),
                    SendAutoresponder = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AutoresponderTemplateId = table.Column<int>(type: "int", nullable: true),
                    EmbedCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DirectUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AllowedDomains = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalViews = table.Column<int>(type: "int", nullable: false),
                    TotalSubmissions = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormDefinitions_LeadRoutingRules_LeadRoutingRuleId",
                        column: x => x.LeadRoutingRuleId,
                        principalTable: "LeadRoutingRules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormDefinitions_MarketingCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "MarketingCampaigns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormDefinitions_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LeadRoutingCriteria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CriteriaType = table.Column<int>(type: "int", nullable: false),
                    FieldName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Operator = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValueTo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LogicalOperator = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Order = table.Column<int>(type: "int", nullable: false),
                    LeadRoutingRuleId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadRoutingCriteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadRoutingCriteria_LeadRoutingRules_LeadRoutingRuleId",
                        column: x => x.LeadRoutingRuleId,
                        principalTable: "LeadRoutingRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LeadRoutingLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LeadId = table.Column<int>(type: "int", nullable: false),
                    LeadRoutingRuleId = table.Column<int>(type: "int", nullable: true),
                    AssignedToUserId = table.Column<int>(type: "int", nullable: true),
                    PreviousOwnerId = table.Column<int>(type: "int", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AssignmentType = table.Column<int>(type: "int", nullable: false),
                    Success = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FailureReason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResponseTimeSeconds = table.Column<int>(type: "int", nullable: true),
                    ContactedWithinSLA = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadRoutingLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadRoutingLogs_LeadRoutingRules_LeadRoutingRuleId",
                        column: x => x.LeadRoutingRuleId,
                        principalTable: "LeadRoutingRules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeadRoutingLogs_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeadRoutingLogs_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LeadRoutingTargets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<int>(type: "int", nullable: false),
                    MaxLeadsPerDay = table.Column<int>(type: "int", nullable: true),
                    MaxLeadsPerWeek = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LeadsAssignedToday = table.Column<int>(type: "int", nullable: false),
                    LeadsAssignedThisWeek = table.Column<int>(type: "int", nullable: false),
                    LastAssignmentDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TotalLeadsAssigned = table.Column<int>(type: "int", nullable: false),
                    LeadRoutingRuleId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadRoutingTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadRoutingTargets_LeadRoutingRules_LeadRoutingRuleId",
                        column: x => x.LeadRoutingRuleId,
                        principalTable: "LeadRoutingRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeadRoutingTargets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SalesForecasts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Period = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PeriodStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FiscalYear = table.Column<int>(type: "int", nullable: false),
                    FiscalQuarter = table.Column<int>(type: "int", nullable: true),
                    FiscalMonth = table.Column<int>(type: "int", nullable: true),
                    QuotaAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClosedWonAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CommitAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    BestCaseAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    PipelineAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    OmittedAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ClosedWonCount = table.Column<int>(type: "int", nullable: false),
                    CommitCount = table.Column<int>(type: "int", nullable: false),
                    BestCaseCount = table.Column<int>(type: "int", nullable: false),
                    PipelineCount = table.Column<int>(type: "int", nullable: false),
                    AdjustedCommitAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    AdjustedBestCaseAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    AdjustmentNotes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdjustedById = table.Column<int>(type: "int", nullable: true),
                    AdjustedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SnapshotDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsSubmitted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    TeamId = table.Column<int>(type: "int", nullable: true),
                    SalesQuotaId = table.Column<int>(type: "int", nullable: true),
                    ParentForecastId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesForecasts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesForecasts_SalesForecasts_ParentForecastId",
                        column: x => x.ParentForecastId,
                        principalTable: "SalesForecasts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesForecasts_SalesQuotas_SalesQuotaId",
                        column: x => x.SalesQuotaId,
                        principalTable: "SalesQuotas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesForecasts_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesForecasts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebPageViews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PageUrl = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PagePath = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PageTitle = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<int>(type: "int", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TimeOnPage = table.Column<int>(type: "int", nullable: false),
                    ScrollDepth = table.Column<int>(type: "int", nullable: true),
                    Referrer = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QueryParameters = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WebVisitorId = table.Column<int>(type: "int", nullable: false),
                    WebSessionId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebPageViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebPageViews_WebSessions_WebSessionId",
                        column: x => x.WebSessionId,
                        principalTable: "WebSessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WebPageViews_WebVisitors_WebVisitorId",
                        column: x => x.WebVisitorId,
                        principalTable: "WebVisitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ESignatureAuditEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EventType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EventDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserAgent = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Location = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ESignatureSignerId = table.Column<int>(type: "int", nullable: true),
                    ESignatureRequestId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ESignatureAuditEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ESignatureAuditEvents_ESignatureRequests_ESignatureRequestId",
                        column: x => x.ESignatureRequestId,
                        principalTable: "ESignatureRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ESignatureAuditEvents_ESignatureSigners_ESignatureSignerId",
                        column: x => x.ESignatureSignerId,
                        principalTable: "ESignatureSigners",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Commissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CommissionNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CommissionPeriod = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PeriodStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DealAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CommissionableAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CommissionRate = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    SplitPercent = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    FinalCommissionAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QuotaAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    AttainmentPercent = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    TierName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Multiplier = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    EarnedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PaidDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ClawbackEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ClawbackDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AdjustmentAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    AdjustmentReason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClawbackAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ClawbackReason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CommissionPlanId = table.Column<int>(type: "int", nullable: false),
                    OpportunityId = table.Column<int>(type: "int", nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: true),
                    OriginalCommissionId = table.Column<int>(type: "int", nullable: true),
                    ApprovedById = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Commissions_CommissionPlans_CommissionPlanId",
                        column: x => x.CommissionPlanId,
                        principalTable: "CommissionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Commissions_Commissions_OriginalCommissionId",
                        column: x => x.OriginalCommissionId,
                        principalTable: "Commissions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Commissions_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Commissions_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Commissions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Commissions_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Commissions_Users_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Commissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CreditMemos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreditMemoNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalCreditMemoId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    ReasonDetails = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreditMemoDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AppliedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RefundedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    AmountApplied = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    AmountRefunded = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    SourceInvoiceId = table.Column<int>(type: "int", nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ApprovedById = table.Column<int>(type: "int", nullable: true),
                    InternalNotes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerNotes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditMemos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditMemos_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreditMemos_Invoices_SourceInvoiceId",
                        column: x => x.SourceInvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CreditMemos_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CreditMemos_Users_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CreditMemos_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InvoiceLineItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    ExternalLineId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SKU = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ExtendedAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ServiceStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ServiceEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RevenueRecognitionStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RevenueRecognitionEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeferredRevenue = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    RecognizedRevenue = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    OrderLineItemId = table.Column<int>(type: "int", nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceLineItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceLineItems_OrderLineItems_OrderLineItemId",
                        column: x => x.OrderLineItemId,
                        principalTable: "OrderLineItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvoiceLineItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvoiceLineItems_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PaymentNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalPaymentId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GatewayTransactionId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GatewayReference = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AuthorizationCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CheckNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    PaymentType = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    AmountApplied = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ProcessingFee = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    RefundedAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExchangeRate = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SettledDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RefundDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DepositDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CardBrand = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CardLast4 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CardExpMonth = table.Column<int>(type: "int", nullable: true),
                    CardExpYear = table.Column<int>(type: "int", nullable: true),
                    CardholderName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BankName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountLast4 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoutingNumberLast4 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Gateway = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GatewayResponseCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GatewayResponseMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AvsResponseCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CvvResponseCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RiskScore = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    GatewayResponseRaw = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FraudFlagged = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FraudNotes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceFingerprint = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: true),
                    OriginalPaymentId = table.Column<int>(type: "int", nullable: true),
                    ProcessedById = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InternalNotes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FailureReason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RefundReason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payments_Payments_OriginalPaymentId",
                        column: x => x.OriginalPaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payments_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payments_Users_ProcessedById",
                        column: x => x.ProcessedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SubscriptionUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PeriodStart = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsageType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    IsInvoiced = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    SubscriptionItemId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionUsages_SubscriptionItems_SubscriptionItemId",
                        column: x => x.SubscriptionItemId,
                        principalTable: "SubscriptionItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubscriptionUsages_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FormFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FieldName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FieldType = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RequiredMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MinLength = table.Column<int>(type: "int", nullable: true),
                    MaxLength = table.Column<int>(type: "int", nullable: true),
                    MinValue = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ValidationPattern = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValidationMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Placeholder = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HelpText = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Width = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CssClasses = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsHidden = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsReadOnly = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Options = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OptionValueField = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OptionLabelField = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AllowOther = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CrmFieldMapping = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CrmEntityMapping = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HasConditionalLogic = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ConditionalLogic = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FormDefinitionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormFields_FormDefinitions_FormDefinitionId",
                        column: x => x.FormDefinitionId,
                        principalTable: "FormDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FormSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SubmissionNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubmittedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FormData = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RawData = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserAgent = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Referrer = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PageUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UtmSource = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UtmMedium = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UtmCampaign = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UtmContent = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UtmTerm = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProcessedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    OptInConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OptInConfirmedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SpamScore = table.Column<int>(type: "int", nullable: true),
                    IsSpam = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FormDefinitionId = table.Column<int>(type: "int", nullable: false),
                    LeadId = table.Column<int>(type: "int", nullable: true),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    WebVisitorId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormSubmissions_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormSubmissions_FormDefinitions_FormDefinitionId",
                        column: x => x.FormDefinitionId,
                        principalTable: "FormDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormSubmissions_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormSubmissions_WebVisitors_WebVisitorId",
                        column: x => x.WebVisitorId,
                        principalTable: "WebVisitors",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ForecastLineItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CloseDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Stage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Probability = table.Column<int>(type: "int", nullable: false),
                    OverrideCategory = table.Column<int>(type: "int", nullable: true),
                    OverrideAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    OverrideNotes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SalesForecastId = table.Column<int>(type: "int", nullable: false),
                    OpportunityId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForecastLineItems_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ForecastLineItems_SalesForecasts_SalesForecastId",
                        column: x => x.SalesForecastId,
                        principalTable: "SalesForecasts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CreditApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    AppliedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreditMemoId = table.Column<int>(type: "int", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    AppliedById = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditApplications_CreditMemos_CreditMemoId",
                        column: x => x.CreditMemoId,
                        principalTable: "CreditMemos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreditApplications_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreditApplications_Users_AppliedById",
                        column: x => x.AppliedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CreditMemoLineItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CreditMemoId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    InvoiceLineItemId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditMemoLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditMemoLineItems_CreditMemos_CreditMemoId",
                        column: x => x.CreditMemoId,
                        principalTable: "CreditMemos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreditMemoLineItems_InvoiceLineItems_InvoiceLineItemId",
                        column: x => x.InvoiceLineItemId,
                        principalTable: "InvoiceLineItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CreditMemoLineItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CampaignTouchpoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TouchpointId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TouchpointType = table.Column<int>(type: "int", nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    TouchpointDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Source = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Medium = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CampaignName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Term = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LandingPageUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReferrerUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssetName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssetType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FormName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailCampaignName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdCreativeId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdGroupName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstTouchCredit = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    LastTouchCredit = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    LinearCredit = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TimeDecayCredit = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    UShapeCredit = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CustomCredit = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    PositionInJourney = table.Column<int>(type: "int", nullable: false),
                    TotalTouchpointsInJourney = table.Column<int>(type: "int", nullable: false),
                    DaysToConversion = table.Column<int>(type: "int", nullable: true),
                    FirstTouchRevenue = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    LastTouchRevenue = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    LinearRevenue = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    CustomRevenue = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    AttributedPipeline = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    LeadId = table.Column<int>(type: "int", nullable: true),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    AccountId = table.Column<int>(type: "int", nullable: true),
                    OpportunityId = table.Column<int>(type: "int", nullable: true),
                    CampaignId = table.Column<int>(type: "int", nullable: true),
                    WebVisitorId = table.Column<int>(type: "int", nullable: true),
                    FormSubmissionId = table.Column<int>(type: "int", nullable: true),
                    DeviceType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Country = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Region = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignTouchpoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignTouchpoints_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CampaignTouchpoints_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CampaignTouchpoints_FormSubmissions_FormSubmissionId",
                        column: x => x.FormSubmissionId,
                        principalTable: "FormSubmissions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CampaignTouchpoints_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CampaignTouchpoints_MarketingCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "MarketingCampaigns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CampaignTouchpoints_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CampaignTouchpoints_WebVisitors_WebVisitorId",
                        column: x => x.WebVisitorId,
                        principalTable: "WebVisitors",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_ContactId",
                table: "Quotes",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_RelationshipManagerId",
                table: "Quotes",
                column: "RelationshipManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_ContactId",
                table: "Notes",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_LeadId",
                table: "Notes",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_QuoteId",
                table: "Notes",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ConversationId",
                table: "Conversations",
                column: "ConversationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_Status",
                table: "Conversations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationMessages_ConversationId",
                table: "CommunicationMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationMessages_Direction",
                table: "CommunicationMessages",
                column: "Direction");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationMessages_SentAt",
                table: "CommunicationMessages",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationMessages_Status",
                table: "CommunicationMessages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_PriceBookId",
                table: "Accounts",
                column: "PriceBookId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountHealthSnapshots_CustomerId",
                table: "AccountHealthSnapshots",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountHealthSnapshots_CustomerId_SnapshotDate",
                table: "AccountHealthSnapshots",
                columns: new[] { "CustomerId", "SnapshotDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountHealthSnapshots_SnapshotDate",
                table: "AccountHealthSnapshots",
                column: "SnapshotDate");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRelationships_RelationshipTypeId",
                table: "AccountRelationships",
                column: "RelationshipTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRelationships_SourceCustomerId",
                table: "AccountRelationships",
                column: "SourceCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRelationships_SourceCustomerId_TargetCustomerId_Relat~",
                table: "AccountRelationships",
                columns: new[] { "SourceCustomerId", "TargetCustomerId", "RelationshipTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountRelationships_Status",
                table: "AccountRelationships",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRelationships_TargetCustomerId",
                table: "AccountRelationships",
                column: "TargetCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRelationships_UpdatedByUserId",
                table: "AccountRelationships",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountTerritories_IsActive",
                table: "AccountTerritories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AccountTerritories_PrimaryOwnerId",
                table: "AccountTerritories",
                column: "PrimaryOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountTerritories_TerritoryCode",
                table: "AccountTerritories",
                column: "TerritoryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalGroupMembers_ApprovalGroupId",
                table: "ApprovalGroupMembers",
                column: "ApprovalGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalGroupMembers_UserId",
                table: "ApprovalGroupMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalLevels_ApproverUserId",
                table: "ApprovalLevels",
                column: "ApproverUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalLevels_DiscountApprovalMatrixId",
                table: "ApprovalLevels",
                column: "DiscountApprovalMatrixId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalLevels_EscalationUserId",
                table: "ApprovalLevels",
                column: "EscalationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_DiscountApprovalMatrixId",
                table: "ApprovalRequests",
                column: "DiscountApprovalMatrixId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_QuoteId",
                table: "ApprovalRequests",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_SubmitterId",
                table: "ApprovalRequests",
                column: "SubmitterId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSteps_ActedById",
                table: "ApprovalSteps",
                column: "ActedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSteps_ApprovalLevelId",
                table: "ApprovalSteps",
                column: "ApprovalLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSteps_ApprovalRequestId",
                table: "ApprovalSteps",
                column: "ApprovalRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSteps_AssignedToId",
                table: "ApprovalSteps",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignABTests_CampaignId",
                table: "CampaignABTests",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignABTests_Status",
                table: "CampaignABTests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignAttributionSummaries_CampaignId",
                table: "CampaignAttributionSummaries",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignConversions_CampaignId",
                table: "CampaignConversions",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignConversions_CampaignRecipientId",
                table: "CampaignConversions",
                column: "CampaignRecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignConversions_ContactId",
                table: "CampaignConversions",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignConversions_ConversionType",
                table: "CampaignConversions",
                column: "ConversionType");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignConversions_ConvertedAt",
                table: "CampaignConversions",
                column: "ConvertedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignConversions_CustomerId",
                table: "CampaignConversions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignLinkClicks_CampaignId",
                table: "CampaignLinkClicks",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignLinkClicks_CampaignRecipientId",
                table: "CampaignLinkClicks",
                column: "CampaignRecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignLinkClicks_ClickedAt",
                table: "CampaignLinkClicks",
                column: "ClickedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRecipients_CampaignId",
                table: "CampaignRecipients",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRecipients_ContactId",
                table: "CampaignRecipients",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRecipients_CustomerId",
                table: "CampaignRecipients",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRecipients_Email",
                table: "CampaignRecipients",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRecipients_Status",
                table: "CampaignRecipients",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTouchpoints_AccountId",
                table: "CampaignTouchpoints",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTouchpoints_CampaignId",
                table: "CampaignTouchpoints",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTouchpoints_ContactId",
                table: "CampaignTouchpoints",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTouchpoints_FormSubmissionId",
                table: "CampaignTouchpoints",
                column: "FormSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTouchpoints_LeadId",
                table: "CampaignTouchpoints",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTouchpoints_OpportunityId",
                table: "CampaignTouchpoints",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTouchpoints_WebVisitorId",
                table: "CampaignTouchpoints",
                column: "WebVisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignWorkflows_CampaignId",
                table: "CampaignWorkflows",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignWorkflows_IsActive",
                table: "CampaignWorkflows",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignWorkflows_WorkflowDefinitionId",
                table: "CampaignWorkflows",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionPlanAssignments_CommissionPlanId",
                table: "CommissionPlanAssignments",
                column: "CommissionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionPlanAssignments_UserId",
                table: "CommissionPlanAssignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_ApprovedById",
                table: "Commissions",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_CommissionPlanId",
                table: "Commissions",
                column: "CommissionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_InvoiceId",
                table: "Commissions",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_OpportunityId",
                table: "Commissions",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_OrderId",
                table: "Commissions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_OriginalCommissionId",
                table: "Commissions",
                column: "OriginalCommissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_SubscriptionId",
                table: "Commissions",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_UserId",
                table: "Commissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionStatements_UserId",
                table: "CommissionStatements",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionTiers_CommissionPlanId",
                table: "CommissionTiers",
                column: "CommissionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplications_AppliedById",
                table: "CreditApplications",
                column: "AppliedById");

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplications_CreditMemoId",
                table: "CreditApplications",
                column: "CreditMemoId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplications_InvoiceId",
                table: "CreditApplications",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditMemoLineItems_CreditMemoId",
                table: "CreditMemoLineItems",
                column: "CreditMemoId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditMemoLineItems_InvoiceLineItemId",
                table: "CreditMemoLineItems",
                column: "InvoiceLineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditMemoLineItems_ProductId",
                table: "CreditMemoLineItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditMemos_AccountId",
                table: "CreditMemos",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditMemos_ApprovedById",
                table: "CreditMemos",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_CreditMemos_CreatedById",
                table: "CreditMemos",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_CreditMemos_OrderId",
                table: "CreditMemos",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditMemos_SourceInvoiceId",
                table: "CreditMemos",
                column: "SourceInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTerritoryAssignments_AssignedByUserId",
                table: "CustomerTerritoryAssignments",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTerritoryAssignments_IsPrimary",
                table: "CustomerTerritoryAssignments",
                column: "IsPrimary");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTerritoryAssignments_TerritoryId",
                table: "CustomerTerritoryAssignments",
                column: "TerritoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateCandidates_DuplicateRuleId",
                table: "DuplicateCandidates",
                column: "DuplicateRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateCandidates_MergedById",
                table: "DuplicateCandidates",
                column: "MergedById");

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateCandidates_ReviewedById",
                table: "DuplicateCandidates",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateMatchFields_DuplicateRuleId",
                table: "DuplicateMatchFields",
                column: "DuplicateRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateMergeHistories_DuplicateCandidateId",
                table: "DuplicateMergeHistories",
                column: "DuplicateCandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateMergeHistories_MergedById",
                table: "DuplicateMergeHistories",
                column: "MergedById");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSequenceEnrollments_ContactId",
                table: "EmailSequenceEnrollments",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSequenceEnrollments_EmailSequenceId",
                table: "EmailSequenceEnrollments",
                column: "EmailSequenceId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSequenceEnrollments_EnrolledById",
                table: "EmailSequenceEnrollments",
                column: "EnrolledById");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSequenceEnrollments_LeadId",
                table: "EmailSequenceEnrollments",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSequences_OwnerId",
                table: "EmailSequences",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSequences_SenderId",
                table: "EmailSequences",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSequenceStepExecutions_EmailSequenceEnrollmentId",
                table: "EmailSequenceStepExecutions",
                column: "EmailSequenceEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSequenceStepExecutions_EmailSequenceStepId",
                table: "EmailSequenceStepExecutions",
                column: "EmailSequenceStepId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSequenceSteps_EmailSequenceId",
                table: "EmailSequenceSteps",
                column: "EmailSequenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ESignatureAuditEvents_ESignatureRequestId",
                table: "ESignatureAuditEvents",
                column: "ESignatureRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ESignatureAuditEvents_ESignatureSignerId",
                table: "ESignatureAuditEvents",
                column: "ESignatureSignerId");

            migrationBuilder.CreateIndex(
                name: "IX_ESignatureDocuments_ESignatureRequestId",
                table: "ESignatureDocuments",
                column: "ESignatureRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ESignatureRequests_AccountId",
                table: "ESignatureRequests",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ESignatureRequests_CreatedById",
                table: "ESignatureRequests",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ESignatureRequests_OpportunityId",
                table: "ESignatureRequests",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_ESignatureRequests_OrderId",
                table: "ESignatureRequests",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ESignatureRequests_QuoteId",
                table: "ESignatureRequests",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_ESignatureRequests_VoidedById",
                table: "ESignatureRequests",
                column: "VoidedById");

            migrationBuilder.CreateIndex(
                name: "IX_ESignatureSigners_ContactId",
                table: "ESignatureSigners",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ESignatureSigners_ESignatureRequestId",
                table: "ESignatureSigners",
                column: "ESignatureRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ESignatureSigners_UserId",
                table: "ESignatureSigners",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastLineItems_OpportunityId",
                table: "ForecastLineItems",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastLineItems_SalesForecastId",
                table: "ForecastLineItems",
                column: "SalesForecastId");

            migrationBuilder.CreateIndex(
                name: "IX_FormDefinitions_CampaignId",
                table: "FormDefinitions",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_FormDefinitions_LeadRoutingRuleId",
                table: "FormDefinitions",
                column: "LeadRoutingRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_FormDefinitions_OwnerId",
                table: "FormDefinitions",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_FormFields_FormDefinitionId",
                table: "FormFields",
                column: "FormDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissions_ContactId",
                table: "FormSubmissions",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissions_FormDefinitionId",
                table: "FormSubmissions",
                column: "FormDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissions_LeadId",
                table: "FormSubmissions",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissions_WebVisitorId",
                table: "FormSubmissions",
                column: "WebVisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLineItems_InvoiceId",
                table: "InvoiceLineItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLineItems_OrderLineItemId",
                table: "InvoiceLineItems",
                column: "OrderLineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLineItems_ProductId",
                table: "InvoiceLineItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLineItems_SubscriptionId",
                table: "InvoiceLineItems",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_AccountId",
                table: "Invoices",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ContactId",
                table: "Invoices",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_OrderId",
                table: "Invoices",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_OriginalInvoiceId",
                table: "Invoices",
                column: "OriginalInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SubscriptionId",
                table: "Invoices",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_VoidedById",
                table: "Invoices",
                column: "VoidedById");

            migrationBuilder.CreateIndex(
                name: "IX_LeadRoutingCriteria_LeadRoutingRuleId",
                table: "LeadRoutingCriteria",
                column: "LeadRoutingRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadRoutingLogs_AssignedToUserId",
                table: "LeadRoutingLogs",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadRoutingLogs_LeadId",
                table: "LeadRoutingLogs",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadRoutingLogs_LeadRoutingRuleId",
                table: "LeadRoutingLogs",
                column: "LeadRoutingRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadRoutingRules_FallbackOwnerId",
                table: "LeadRoutingRules",
                column: "FallbackOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadRoutingRules_TeamId",
                table: "LeadRoutingRules",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadRoutingTargets_LeadRoutingRuleId",
                table: "LeadRoutingTargets",
                column: "LeadRoutingRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadRoutingTargets_UserId",
                table: "LeadRoutingTargets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_llm_provider_settings_category",
                table: "llm_provider_settings",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "IX_llm_provider_settings_setting_key",
                table: "llm_provider_settings",
                column: "setting_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineItems_OrderId",
                table: "OrderLineItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineItems_ParentLineItemId",
                table: "OrderLineItems",
                column: "ParentLineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineItems_ProductId",
                table: "OrderLineItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineItems_QuoteLineItemId",
                table: "OrderLineItems",
                column: "QuoteLineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AccountId",
                table: "Orders",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ApprovedById",
                table: "Orders",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ContactId",
                table: "Orders",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OpportunityId",
                table: "Orders",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OwnerId",
                table: "Orders",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ParentOrderId",
                table: "Orders",
                column: "ParentOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_QuoteId",
                table: "Orders",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_AccountId",
                table: "Payments",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId",
                table: "Payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OriginalPaymentId",
                table: "Payments",
                column: "OriginalPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ProcessedById",
                table: "Payments",
                column: "ProcessedById");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SubscriptionId",
                table: "Payments",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceBookEntries_PriceBookId",
                table: "PriceBookEntries",
                column: "PriceBookId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceBookEntries_ProductId",
                table: "PriceBookEntries",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PricingRuleUsages_AppliedById",
                table: "PricingRuleUsages",
                column: "AppliedById");

            migrationBuilder.CreateIndex(
                name: "IX_PricingRuleUsages_OrderId",
                table: "PricingRuleUsages",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PricingRuleUsages_PricingRuleId",
                table: "PricingRuleUsages",
                column: "PricingRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_PricingRuleUsages_QuoteId",
                table: "PricingRuleUsages",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBundleItems_ProductBundleId",
                table: "ProductBundleItems",
                column: "ProductBundleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBundleItems_ProductId",
                table: "ProductBundleItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBundleRules_ProductBundleId",
                table: "ProductBundleRules",
                column: "ProductBundleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBundleRules_SourceProductId",
                table: "ProductBundleRules",
                column: "SourceProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBundleRules_TargetProductId",
                table: "ProductBundleRules",
                column: "TargetProductId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteLineItems_ParentLineItemId",
                table: "QuoteLineItems",
                column: "ParentLineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteLineItems_ProductId",
                table: "QuoteLineItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteLineItems_QuoteId_LineNumber",
                table: "QuoteLineItems",
                columns: new[] { "QuoteId", "LineNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteLineItems_SKU",
                table: "QuoteLineItems",
                column: "SKU");

            migrationBuilder.CreateIndex(
                name: "IX_RelationshipInteractions_AccountRelationshipId",
                table: "RelationshipInteractions",
                column: "AccountRelationshipId");

            migrationBuilder.CreateIndex(
                name: "IX_RelationshipInteractions_InteractionDate",
                table: "RelationshipInteractions",
                column: "InteractionDate");

            migrationBuilder.CreateIndex(
                name: "IX_RelationshipInteractions_InteractionType",
                table: "RelationshipInteractions",
                column: "InteractionType");

            migrationBuilder.CreateIndex(
                name: "IX_RelationshipMaps_CentralCustomerId",
                table: "RelationshipMaps",
                column: "CentralCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_RelationshipMaps_IsPublic",
                table: "RelationshipMaps",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_RelationshipTypes_IsActive",
                table: "RelationshipTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_RelationshipTypes_TypeCategory",
                table: "RelationshipTypes",
                column: "TypeCategory");

            migrationBuilder.CreateIndex(
                name: "IX_RelationshipTypes_TypeName",
                table: "RelationshipTypes",
                column: "TypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesForecasts_ParentForecastId",
                table: "SalesForecasts",
                column: "ParentForecastId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesForecasts_SalesQuotaId",
                table: "SalesForecasts",
                column: "SalesQuotaId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesForecasts_TeamId",
                table: "SalesForecasts",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesForecasts_UserId",
                table: "SalesForecasts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotas_ParentQuotaId",
                table: "SalesQuotas",
                column: "ParentQuotaId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotas_TeamId",
                table: "SalesQuotas",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotas_UserId",
                table: "SalesQuotas",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionItems_ProductId",
                table: "SubscriptionItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionItems_SubscriptionId",
                table: "SubscriptionItems",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_AccountId",
                table: "Subscriptions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ContactId",
                table: "Subscriptions",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_OpportunityId",
                table: "Subscriptions",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_OrderId",
                table: "Subscriptions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_OwnerId",
                table: "Subscriptions",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PreviousSubscriptionId",
                table: "Subscriptions",
                column: "PreviousSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ProductId",
                table: "Subscriptions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionUsages_SubscriptionId",
                table: "SubscriptionUsages",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionUsages_SubscriptionItemId",
                table: "SubscriptionUsages",
                column: "SubscriptionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_TeamId",
                table: "TeamMembers",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_UserId",
                table: "TeamMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_ManagerId",
                table: "Teams",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_ParentTeamId",
                table: "Teams",
                column: "ParentTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_WebPageViews_WebSessionId",
                table: "WebPageViews",
                column: "WebSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_WebPageViews_WebVisitorId",
                table: "WebPageViews",
                column: "WebVisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_WebSessions_WebVisitorId",
                table: "WebSessions",
                column: "WebVisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_WebVisitors_AccountId",
                table: "WebVisitors",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_WebVisitors_ContactId",
                table: "WebVisitors",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_WebVisitors_LeadId",
                table: "WebVisitors",
                column: "LeadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_PriceBooks_PriceBookId",
                table: "Accounts",
                column: "PriceBookId",
                principalTable: "PriceBooks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunicationMessages_CommunicationChannels_ChannelId",
                table: "CommunicationMessages",
                column: "ChannelId",
                principalTable: "CommunicationChannels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CommunicationMessages_CommunicationChannels_CommunicationCha~",
                table: "CommunicationMessages",
                column: "CommunicationChannelId",
                principalTable: "CommunicationChannels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunicationMessages_CommunicationMessages_ParentMessageId",
                table: "CommunicationMessages",
                column: "ParentMessageId",
                principalTable: "CommunicationMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Contacts_ContactId",
                table: "Notes",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Leads_LeadId",
                table: "Notes",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Quotes_QuoteId",
                table: "Notes",
                column: "QuoteId",
                principalTable: "Quotes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Accounts_AccountId",
                table: "Opportunities",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_Contacts_ContactId",
                table: "Quotes",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_Users_RelationshipManagerId",
                table: "Quotes",
                column: "RelationshipManagerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_PriceBooks_PriceBookId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_CommunicationMessages_CommunicationChannels_ChannelId",
                table: "CommunicationMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_CommunicationMessages_CommunicationChannels_CommunicationCha~",
                table: "CommunicationMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_CommunicationMessages_CommunicationMessages_ParentMessageId",
                table: "CommunicationMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Contacts_ContactId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Leads_LeadId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Quotes_QuoteId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Accounts_AccountId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_Contacts_ContactId",
                table: "Quotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_Users_RelationshipManagerId",
                table: "Quotes");

            migrationBuilder.DropTable(
                name: "AccountHealthSnapshots");

            migrationBuilder.DropTable(
                name: "ApprovalGroupMembers");

            migrationBuilder.DropTable(
                name: "ApprovalSteps");

            migrationBuilder.DropTable(
                name: "AttributionSettings");

            migrationBuilder.DropTable(
                name: "CampaignABTests");

            migrationBuilder.DropTable(
                name: "CampaignAttributionSummaries");

            migrationBuilder.DropTable(
                name: "CampaignConversions");

            migrationBuilder.DropTable(
                name: "CampaignLinkClicks");

            migrationBuilder.DropTable(
                name: "CampaignTouchpoints");

            migrationBuilder.DropTable(
                name: "CampaignWorkflows");

            migrationBuilder.DropTable(
                name: "CommissionPlanAssignments");

            migrationBuilder.DropTable(
                name: "Commissions");

            migrationBuilder.DropTable(
                name: "CommissionStatements");

            migrationBuilder.DropTable(
                name: "CommissionTiers");

            migrationBuilder.DropTable(
                name: "CreditApplications");

            migrationBuilder.DropTable(
                name: "CreditMemoLineItems");

            migrationBuilder.DropTable(
                name: "CustomerTerritoryAssignments");

            migrationBuilder.DropTable(
                name: "DuplicateMatchFields");

            migrationBuilder.DropTable(
                name: "DuplicateMergeHistories");

            migrationBuilder.DropTable(
                name: "EmailSequenceStepExecutions");

            migrationBuilder.DropTable(
                name: "ESignatureAuditEvents");

            migrationBuilder.DropTable(
                name: "ESignatureDocuments");

            migrationBuilder.DropTable(
                name: "ForecastHistories");

            migrationBuilder.DropTable(
                name: "ForecastLineItems");

            migrationBuilder.DropTable(
                name: "FormFields");

            migrationBuilder.DropTable(
                name: "LeadRoutingCriteria");

            migrationBuilder.DropTable(
                name: "LeadRoutingLogs");

            migrationBuilder.DropTable(
                name: "LeadRoutingTargets");

            migrationBuilder.DropTable(
                name: "llm_provider_settings");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PriceBookEntries");

            migrationBuilder.DropTable(
                name: "PricingRuleUsages");

            migrationBuilder.DropTable(
                name: "ProductBundleItems");

            migrationBuilder.DropTable(
                name: "ProductBundleRules");

            migrationBuilder.DropTable(
                name: "RelationshipInteractions");

            migrationBuilder.DropTable(
                name: "RelationshipMaps");

            migrationBuilder.DropTable(
                name: "SubscriptionUsages");

            migrationBuilder.DropTable(
                name: "TeamMembers");

            migrationBuilder.DropTable(
                name: "WebPageViews");

            migrationBuilder.DropTable(
                name: "ApprovalGroups");

            migrationBuilder.DropTable(
                name: "ApprovalLevels");

            migrationBuilder.DropTable(
                name: "ApprovalRequests");

            migrationBuilder.DropTable(
                name: "CampaignRecipients");

            migrationBuilder.DropTable(
                name: "FormSubmissions");

            migrationBuilder.DropTable(
                name: "CommissionPlans");

            migrationBuilder.DropTable(
                name: "CreditMemos");

            migrationBuilder.DropTable(
                name: "InvoiceLineItems");

            migrationBuilder.DropTable(
                name: "AccountTerritories");

            migrationBuilder.DropTable(
                name: "DuplicateCandidates");

            migrationBuilder.DropTable(
                name: "EmailSequenceEnrollments");

            migrationBuilder.DropTable(
                name: "EmailSequenceSteps");

            migrationBuilder.DropTable(
                name: "ESignatureSigners");

            migrationBuilder.DropTable(
                name: "SalesForecasts");

            migrationBuilder.DropTable(
                name: "PriceBooks");

            migrationBuilder.DropTable(
                name: "PricingRules");

            migrationBuilder.DropTable(
                name: "ProductBundles");

            migrationBuilder.DropTable(
                name: "AccountRelationships");

            migrationBuilder.DropTable(
                name: "SubscriptionItems");

            migrationBuilder.DropTable(
                name: "WebSessions");

            migrationBuilder.DropTable(
                name: "DiscountApprovalMatrices");

            migrationBuilder.DropTable(
                name: "FormDefinitions");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "OrderLineItems");

            migrationBuilder.DropTable(
                name: "DuplicateRules");

            migrationBuilder.DropTable(
                name: "EmailSequences");

            migrationBuilder.DropTable(
                name: "ESignatureRequests");

            migrationBuilder.DropTable(
                name: "SalesQuotas");

            migrationBuilder.DropTable(
                name: "RelationshipTypes");

            migrationBuilder.DropTable(
                name: "WebVisitors");

            migrationBuilder.DropTable(
                name: "LeadRoutingRules");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "QuoteLineItems");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_ContactId",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_RelationshipManagerId",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_ContactId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_LeadId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_QuoteId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_ConversationId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_Status",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_CommunicationMessages_ConversationId",
                table: "CommunicationMessages");

            migrationBuilder.DropIndex(
                name: "IX_CommunicationMessages_Direction",
                table: "CommunicationMessages");

            migrationBuilder.DropIndex(
                name: "IX_CommunicationMessages_SentAt",
                table: "CommunicationMessages");

            migrationBuilder.DropIndex(
                name: "IX_CommunicationMessages_Status",
                table: "CommunicationMessages");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_PriceBookId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WorkflowVersions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WorkflowTransitions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WorkflowTasks");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WorkflowNodes");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WorkflowNodeInstances");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WorkflowLogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "CompactMode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DateFormat",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DesktopNotifications",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailNotifications",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RowsPerPage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ThemePreference",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TimeFormat",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Timezone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UserGroups");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UserGroupMembers");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "UserApprovalRequests");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UserApprovalRequests");

            migrationBuilder.DropColumn(
                name: "ActiveDatabaseProvider",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "CompanyAddresses",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "CompanyDescription",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "CompanyEmails",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "CompanyFullName",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "CompanyIndustry",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "CompanyLegalName",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "CompanyLoginLogoUrl",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "CompanyPhones",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "CompanyRegistrationNumber",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "CompanyTaxId",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "DefaultTaxRate",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "MariaDbEnabled",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "MySqlEnabled",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "PostgreSqlEnabled",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "QuoteNumberPrefix",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "QuoteNumberSequence",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "QuoteTermsAndConditions",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "QuoteValidityDays",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "SqlServerEnabled",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "SqliteEnabled",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SocialMediaFollows");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SocialMediaAccounts");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SocialAccounts");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ServiceRequestTypes");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ServiceRequestSubcategories");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ServiceRequestCustomFieldValues");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ServiceRequestCustomFieldDefinitions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ServiceRequestCategories");

            migrationBuilder.DropColumn(
                name: "ActualDeliveryDate",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "ExpectedDeliveryDate",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "RelationshipManagerId",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "ServiceEndDate",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "ServiceStartDate",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "SubmittedForApprovalDate",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "WarrantyEndDate",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "WarrantyMonths",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PhoneNumbers");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "OAuthTokens");

            migrationBuilder.DropColumn(
                name: "ContextPath",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "LeadId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "QuoteId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ModuleUIConfigs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ModuleFieldConfigurations");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "MarketingCampaigns");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "LookupItems");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "LookupCategories");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Localities");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Interactions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "HealthCheckLogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "FieldMasterDataLinks");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EntityTags");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EntitySocialMediaLinks");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EntityPhoneLinks");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EntityEmailLinks");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EntityAddressLinks");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EmailTemplates");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EmailAddresses");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "DeploymentAttempts");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "DatabaseBackups");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "DashboardWidgets");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Dashboards");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CustomFields");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CustomerContacts");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CrmTasks");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ContactInfoLinks");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ContactDetails");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CommunicationMessages");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CommunicationChannels");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ColorPalettes");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CloudProviders");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CloudDeployments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CampaignMetrics");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "BackupSchedules");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "PriceBookId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "CommunicationChannelId",
                table: "CommunicationMessages",
                newName: "ConversationId1");

            migrationBuilder.RenameIndex(
                name: "IX_CommunicationMessages_CommunicationChannelId",
                table: "CommunicationMessages",
                newName: "IX_CommunicationMessages_ConversationId1");

            migrationBuilder.AddColumn<int>(
                name: "AccountId1",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MarketingCampaignId",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MarketingCampaignId1",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MarketingCampaignId2",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "Conversations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ParticipantName",
                table: "Conversations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ParticipantAddress",
                table: "Conversations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "LastMessagePreview",
                table: "Conversations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ConversationId",
                table: "Conversations",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ToName",
                table: "CommunicationMessages",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ToAddress",
                table: "CommunicationMessages",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "CommunicationMessages",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "FromName",
                table: "CommunicationMessages",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "FromAddress",
                table: "CommunicationMessages",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalMessageId",
                table: "CommunicationMessages",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ConversationId",
                table: "CommunicationMessages",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_AccountId1",
                table: "Opportunities",
                column: "AccountId1");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_MarketingCampaignId",
                table: "Leads",
                column: "MarketingCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_MarketingCampaignId1",
                table: "Leads",
                column: "MarketingCampaignId1");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_MarketingCampaignId2",
                table: "Leads",
                column: "MarketingCampaignId2");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunicationMessages_CommunicationChannels_ChannelId",
                table: "CommunicationMessages",
                column: "ChannelId",
                principalTable: "CommunicationChannels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommunicationMessages_CommunicationMessages_ParentMessageId",
                table: "CommunicationMessages",
                column: "ParentMessageId",
                principalTable: "CommunicationMessages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunicationMessages_Conversations_ConversationId1",
                table: "CommunicationMessages",
                column: "ConversationId1",
                principalTable: "Conversations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_MarketingCampaigns_MarketingCampaignId",
                table: "Leads",
                column: "MarketingCampaignId",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_MarketingCampaigns_MarketingCampaignId1",
                table: "Leads",
                column: "MarketingCampaignId1",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_MarketingCampaigns_MarketingCampaignId2",
                table: "Leads",
                column: "MarketingCampaignId2",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Accounts_AccountId",
                table: "Opportunities",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Accounts_AccountId1",
                table: "Opportunities",
                column: "AccountId1",
                principalTable: "Accounts",
                principalColumn: "Id");
        }
    }
}
