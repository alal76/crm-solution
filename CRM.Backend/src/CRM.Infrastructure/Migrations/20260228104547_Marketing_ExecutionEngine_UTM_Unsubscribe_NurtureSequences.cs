using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Marketing_ExecutionEngine_UTM_Unsubscribe_NurtureSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "MarketingCampaigns",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledAt",
                table: "MarketingCampaigns",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "MarketingCampaigns",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UnsubscribeHeaderEnabled",
                table: "MarketingCampaigns",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "GuardrailScriptId",
                table: "AIAgents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxCallsPerHour",
                table: "AIAgents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxCostPerDay",
                table: "AIAgents",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxTokensPerCall",
                table: "AIAgents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OnActivateScriptId",
                table: "AIAgents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OnAfterToolCallScriptId",
                table: "AIAgents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OnBeforeToolCallScriptId",
                table: "AIAgents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OnDeactivateScriptId",
                table: "AIAgents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OnErrorScriptId",
                table: "AIAgents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OnPlanScriptId",
                table: "AIAgents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OnResponseScriptId",
                table: "AIAgents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CampaignTrackingLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    OriginalUrl = table.Column<string>(type: "VARCHAR(2048)", maxLength: 2048, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrackedUrl = table.Column<string>(type: "VARCHAR(2048)", maxLength: 2048, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LinkAlias = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UtmSource = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UtmMedium = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UtmCampaign = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UtmContent = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrackingToken = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClickCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignTrackingLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignTrackingLinks_MarketingCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "MarketingCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NurtureEnrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SequenceId = table.Column<int>(type: "int", nullable: false),
                    LeadId = table.Column<int>(type: "int", nullable: true),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    EnrolleeEmail = table.Column<string>(type: "VARCHAR(320)", maxLength: 320, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EnrolleeName = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Trigger = table.Column<int>(type: "int", nullable: false),
                    CurrentStep = table.Column<int>(type: "int", nullable: false),
                    NextStepAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsUnsubscribed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NurtureEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NurtureEnrollments_EmailSequences_SequenceId",
                        column: x => x.SequenceId,
                        principalTable: "EmailSequences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UnsubscribeRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(type: "VARCHAR(320)", maxLength: 320, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    ReasonNote = table.Column<string>(type: "VARCHAR(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CampaignId = table.Column<int>(type: "int", nullable: true),
                    Token = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnsubscribedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReceiveProductUpdates = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReceiveTransactional = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnsubscribeRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnsubscribeRecords_MarketingCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "MarketingCampaigns",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UtmLinkClicks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UtmSource = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UtmMedium = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UtmCampaign = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UtmContent = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UtmTerm = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginalUrl = table.Column<string>(type: "VARCHAR(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LandingUrl = table.Column<string>(type: "VARCHAR(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VisitorIp = table.Column<string>(type: "VARCHAR(45)", maxLength: 45, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VisitorUserAgent = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LeadId = table.Column<int>(type: "int", nullable: true),
                    TrackingLinkId = table.Column<int>(type: "int", nullable: true),
                    ClickedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtmLinkClicks", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CampaignEmailTrackings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CampaignId = table.Column<int>(type: "int", nullable: true),
                    EnrollmentId = table.Column<int>(type: "int", nullable: true),
                    RecipientEmail = table.Column<string>(type: "VARCHAR(320)", maxLength: 320, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Event = table.Column<int>(type: "int", nullable: false),
                    ClickedUrl = table.Column<string>(type: "VARCHAR(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserAgent = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "VARCHAR(45)", maxLength: 45, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MessageId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EventAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignEmailTrackings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignEmailTrackings_MarketingCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "MarketingCampaigns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CampaignEmailTrackings_NurtureEnrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "NurtureEnrollments",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignEmailTracking_CampaignEvent",
                table: "CampaignEmailTrackings",
                columns: new[] { "CampaignId", "Event" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignEmailTrackings_EnrollmentId",
                table: "CampaignEmailTrackings",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTrackingLinks_CampaignId",
                table: "CampaignTrackingLinks",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTrackingLinks_Token",
                table: "CampaignTrackingLinks",
                column: "TrackingToken");

            migrationBuilder.CreateIndex(
                name: "IX_NurtureEnrollments_SeqEmail",
                table: "NurtureEnrollments",
                columns: new[] { "SequenceId", "EnrolleeEmail" });

            migrationBuilder.CreateIndex(
                name: "IX_UnsubscribeRecords_CampaignId",
                table: "UnsubscribeRecords",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_UnsubscribeRecords_Email",
                table: "UnsubscribeRecords",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_UnsubscribeRecords_Token",
                table: "UnsubscribeRecords",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_UtmLinkClicks_SourceCampaign",
                table: "UtmLinkClicks",
                columns: new[] { "UtmSource", "UtmCampaign" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignEmailTrackings");

            migrationBuilder.DropTable(
                name: "CampaignTrackingLinks");

            migrationBuilder.DropTable(
                name: "UnsubscribeRecords");

            migrationBuilder.DropTable(
                name: "UtmLinkClicks");

            migrationBuilder.DropTable(
                name: "NurtureEnrollments");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "MarketingCampaigns");

            migrationBuilder.DropColumn(
                name: "ScheduledAt",
                table: "MarketingCampaigns");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "MarketingCampaigns");

            migrationBuilder.DropColumn(
                name: "UnsubscribeHeaderEnabled",
                table: "MarketingCampaigns");

            migrationBuilder.DropColumn(
                name: "GuardrailScriptId",
                table: "AIAgents");

            migrationBuilder.DropColumn(
                name: "MaxCallsPerHour",
                table: "AIAgents");

            migrationBuilder.DropColumn(
                name: "MaxCostPerDay",
                table: "AIAgents");

            migrationBuilder.DropColumn(
                name: "MaxTokensPerCall",
                table: "AIAgents");

            migrationBuilder.DropColumn(
                name: "OnActivateScriptId",
                table: "AIAgents");

            migrationBuilder.DropColumn(
                name: "OnAfterToolCallScriptId",
                table: "AIAgents");

            migrationBuilder.DropColumn(
                name: "OnBeforeToolCallScriptId",
                table: "AIAgents");

            migrationBuilder.DropColumn(
                name: "OnDeactivateScriptId",
                table: "AIAgents");

            migrationBuilder.DropColumn(
                name: "OnErrorScriptId",
                table: "AIAgents");

            migrationBuilder.DropColumn(
                name: "OnPlanScriptId",
                table: "AIAgents");

            migrationBuilder.DropColumn(
                name: "OnResponseScriptId",
                table: "AIAgents");
        }
    }
}
