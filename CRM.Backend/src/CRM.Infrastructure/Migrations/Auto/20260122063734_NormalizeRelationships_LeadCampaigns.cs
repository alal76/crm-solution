using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations.Auto
{
    /// <inheritdoc />
    public partial class NormalizeRelationships_LeadCampaigns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leads_MarketingCampaigns_MarketingCampaignId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_MarketingCampaigns_MarketingCampaignId1",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_MarketingCampaigns_MarketingCampaignId2",
                table: "Leads");

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
                name: "MarketingCampaignId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "MarketingCampaignId1",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "MarketingCampaignId2",
                table: "Leads");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
