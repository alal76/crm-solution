using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations.Auto
{
    /// <inheritdoc />
    public partial class NormalizeRelationships_Start : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_MarketingCampaigns_CampaignId",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Contacts_PreferredContactMethodLookupId",
                table: "Contacts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_LookupItems_PreferredContactMethodLookupId1",
                table: "Contacts");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmTasks_MarketingCampaigns_CampaignId",
                table: "CrmTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Interactions_MarketingCampaigns_CampaignId",
                table: "Interactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_MarketingCampaigns_CampaignId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_MarketingCampaigns_CampaignId",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_PreferredContactMethodLookupId1",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "PreferredContactMethodLookupId1",
                table: "Contacts");

            migrationBuilder.AddColumn<int>(
                name: "MarketingCampaignId",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_MarketingCampaignId",
                table: "Opportunities",
                column: "MarketingCampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_MarketingCampaigns_CampaignId",
                table: "Activities",
                column: "CampaignId",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_LookupItems_PreferredContactMethodLookupId",
                table: "Contacts",
                column: "PreferredContactMethodLookupId",
                principalTable: "LookupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmTasks_MarketingCampaigns_CampaignId",
                table: "CrmTasks",
                column: "CampaignId",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Interactions_MarketingCampaigns_CampaignId",
                table: "Interactions",
                column: "CampaignId",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_MarketingCampaigns_CampaignId",
                table: "Notes",
                column: "CampaignId",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_MarketingCampaigns_CampaignId",
                table: "Opportunities",
                column: "CampaignId",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_MarketingCampaigns_MarketingCampaignId",
                table: "Opportunities",
                column: "MarketingCampaignId",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_MarketingCampaigns_CampaignId",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_LookupItems_PreferredContactMethodLookupId",
                table: "Contacts");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmTasks_MarketingCampaigns_CampaignId",
                table: "CrmTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Interactions_MarketingCampaigns_CampaignId",
                table: "Interactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_MarketingCampaigns_CampaignId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_MarketingCampaigns_CampaignId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_MarketingCampaigns_MarketingCampaignId",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_MarketingCampaignId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "MarketingCampaignId",
                table: "Opportunities");

            migrationBuilder.AddColumn<int>(
                name: "PreferredContactMethodLookupId1",
                table: "Contacts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_PreferredContactMethodLookupId1",
                table: "Contacts",
                column: "PreferredContactMethodLookupId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_MarketingCampaigns_CampaignId",
                table: "Activities",
                column: "CampaignId",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id");

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
                name: "FK_CrmTasks_MarketingCampaigns_CampaignId",
                table: "CrmTasks",
                column: "CampaignId",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Interactions_MarketingCampaigns_CampaignId",
                table: "Interactions",
                column: "CampaignId",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_MarketingCampaigns_CampaignId",
                table: "Notes",
                column: "CampaignId",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_MarketingCampaigns_CampaignId",
                table: "Opportunities",
                column: "CampaignId",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id");
        }
    }
}
