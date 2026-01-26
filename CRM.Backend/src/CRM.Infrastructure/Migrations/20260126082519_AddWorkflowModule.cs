using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Leads_LeadId",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactInfoLinks_Leads_LeadId",
                table: "ContactInfoLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_Interactions_Leads_LeadId",
                table: "Interactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Customers_ConvertedCustomerId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Customers_ReferredByCustomerId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Leads_MasterLeadId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_MarketingCampaigns_ConvertingCampaignId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_MarketingCampaigns_LastCampaignId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_MarketingCampaigns_PrimaryCampaignId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Opportunities_ConvertedOpportunityId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Products_PrimaryProductInterestId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Users_ConvertedByUserId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Users_DisqualifiedByUserId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Accounts_AccountId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Customers_CustomerId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Leads_OriginalLeadId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_MarketingCampaigns_CampaignId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Products_ProductId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Users_AssignedToUserId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Users_SalesEngineerId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Users_SalesManagerId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_WorkflowExecutions_WorkflowExecutionId",
                table: "ServiceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Workflows_WorkflowId",
                table: "ServiceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequestSubcategories_Workflows_DefaultWorkflowId",
                table: "ServiceRequestSubcategories");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowDefinitions_Users_CreatedByUserId",
                table: "WorkflowDefinitions");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowDefinitions_Users_LastModifiedByUserId",
                table: "WorkflowDefinitions");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstances_Users_StartedByUserId",
                table: "WorkflowInstances");

            migrationBuilder.DropTable(
                name: "WorkflowApiCredentials");

            migrationBuilder.DropTable(
                name: "WorkflowContextVariables");

            migrationBuilder.DropTable(
                name: "WorkflowDefinitionVersions");

            migrationBuilder.DropTable(
                name: "WorkflowEngineEvents");

            migrationBuilder.DropTable(
                name: "WorkflowExecutions");

            migrationBuilder.DropTable(
                name: "WorkflowJobs");

            migrationBuilder.DropTable(
                name: "WorkflowRuleConditions");

            migrationBuilder.DropTable(
                name: "WorkflowSchedules");

            migrationBuilder.DropTable(
                name: "WorkflowSteps");

            migrationBuilder.DropTable(
                name: "WorkflowEngineTasks");

            migrationBuilder.DropTable(
                name: "WorkflowRules");

            migrationBuilder.DropTable(
                name: "Workflows");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowInstances_Priority",
                table: "WorkflowInstances");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowInstances_Status_CurrentStepKey_Priority",
                table: "WorkflowInstances");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowDefinitions_CreatedByUserId",
                table: "WorkflowDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowDefinitions_TriggerEntityType_Status",
                table: "WorkflowDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowDefinitions_TriggerType",
                table: "WorkflowDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequestSubcategories_DefaultWorkflowId",
                table: "ServiceRequestSubcategories");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_WorkflowExecutionId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_WorkflowId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_AssignedToUserId",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_CampaignId",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_OriginalLeadId",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_SalesEngineerId",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Leads_ConvertedByUserId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_ConvertedCustomerId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_ConvertedOpportunityId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_ConvertingCampaignId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_DisqualifiedByUserId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_LastCampaignId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_LeadScore",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Interactions_LeadId",
                table: "Interactions");

            migrationBuilder.DropIndex(
                name: "IX_ContactInfoLinks_LeadId",
                table: "ContactInfoLinks");

            migrationBuilder.DropIndex(
                name: "IX_Activities_LeadId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ActiveStepKeys",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "CurrentStepKey",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "EntityReference",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "WorkflowVersion",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "ErrorHandlingConfig",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "NotificationConfig",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "ScheduleCron",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "DefaultWorkflowId",
                table: "ServiceRequestSubcategories");

            migrationBuilder.DropColumn(
                name: "CurrentWorkflowStep",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "WorkflowExecutionId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "WorkflowId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ActualCloseDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "AiScoreFactors",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "AiWinScore",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "AmountInBaseCurrency",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "AnnualRecurringRevenue",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "AtRiskReason",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Attachments",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "AuthorityConfirmed",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "BantScore",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Blockers",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "BudgetAmount",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "BudgetFiscalYear",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "BudgetStatus",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "BusinessCase",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CallCount",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Champion",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ChampionEngagement",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ChampionTitle",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ChangeHistory",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CloseDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CloseDatePushCount",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CompetitiveSituation",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CompetitorPrice",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CompetitorStrengths",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CompetitorWeaknesses",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Competitors",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ContractEndDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ContractLengthMonths",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ContractStartDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ContractType",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CostAmount",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CurrentStageEnteredDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CustomFields",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CustomerBuyingStage",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CustomerTargetDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "DaysInCurrentStage",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "DaysSinceLastContact",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "DecisionCriteria",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "DecisionDeadline",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "DecisionMakers",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "DecisionProcess",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "DemoCount",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "DiscountReason",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "DiscountRequiresApproval",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "EconomicBuyer",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "EmailCount",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "EngagementLevel",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ExecutiveSummary",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ExpectedRevenue",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ExpectedSalesCycleDays",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ExternalIds",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ExternalOpportunityId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "FiscalQuarter",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "FiscalYear",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ForecastCategory",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "GrossMarginPercent",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "HasProofOfConcept",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Health",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ImplementationRequirements",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Industry",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "InternalNotes",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "IsAtRisk",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "IsStalled",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "LastActivityDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "LastContactDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "LastMeetingDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "LastSignificantUpdate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "LastSyncDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "LeadSource",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "LeadSourceDetail",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "LossReason",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "LossReasonCategory",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "MarketingTouchpoints",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "MaxAllowedDiscount",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "MeddicCriteria",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "MeddicScore",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "MeetingCount",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "MetricsIdentified",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "NeedConfirmed",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "NextMeetingDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "NextStep",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "NextStepDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "OneTimeRevenue",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "OpportunityNumber",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "OpportunityType",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "OriginalCloseDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "OriginalLeadId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "PainPoints",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ParentOpportunityId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "PocEndDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "PocNotes",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "PocStartDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "PocStatus",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "PocSuccessCriteria",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "PreviousForecastCategory",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "PreviousNextStep",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "PreviousStage",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "PrimaryCompetitor",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ProbabilityOverridden",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ProductCount",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ProductFamily",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Products",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ProposedSolution",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "QuoteId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "RecurringRevenue",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ReferralPartner",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ReferralPartnerId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "RelatedOpportunityIds",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ResponseRate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "RiskFactors",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "RiskMitigationPlan",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "RiskScore",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "SalesEngineerId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "SalesEngineerUserId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Segment",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ServicesRevenue",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "SolutionType",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "SpecialTerms",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "StageHistory",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "StalledDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "StalledReason",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "SyncStatus",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "TechnicalRequirements",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Territory",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "TimelineConfirmed",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "TotalActivities",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "TotalContractValue",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "WinReason",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Address2",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "AffiliateCode",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "AnnualRevenue",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "AssignedDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "AssignmentMethod",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "AuthorityLevel",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "BantScore",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "BehaviorScore",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "BudgetAmount",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "BudgetApproved",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "BudgetRange",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "CallsConnected",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "CallsMade",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "CampaignHistory",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "CampaignTouchCount",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Company",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "CompanyDescription",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "CompanySize",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "CompetitorChosen",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ContentDownloads",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ConversionType",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ConvertedByUserId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ConvertedContactId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ConvertedCustomerId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ConvertedDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ConvertedOpportunityId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ConvertedRevenue",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ConvertingCampaignId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "CurrentSolution",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "CustomFields",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "DaysSinceLastContact",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "DaysToConvert",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "DemoCompleted",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "DemoCompletedDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "DemoRequestDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "DisqualificationNotes",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "DisqualificationReason",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "DisqualifiedByUserId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "DisqualifiedDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "DoNotCall",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "DoNotEmail",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "DownloadedContent",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "DuplicateCheckPerformed",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "EconomicBuyer",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "EmailBounceStatus",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "EmailClicks",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "EmailsOpened",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "EmailsSent",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "EnrichedCompanyData",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "EnrichedDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "EnrichedPersonData",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "EnrichmentSource",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "EstimatedValue",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "EventsAttended",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ExpectedPurchaseDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "FaxNumber",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Fbclid",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "FormData",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Gclid",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "HasAuthority",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "HasBudget",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "HasNeed",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "HasTimeline",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Industry",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "IndustryOther",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "InternalNotes",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "IsConverted",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "IsDisqualified",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "IsDuplicate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "IsEnriched",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "IsMql",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "IsSal",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "IsSql",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "IsStale",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LandingPageUrl",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LastActivityDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LastCallDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LastCampaignId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LastEmailClickDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LastEmailOpenDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LastMeetingDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LastRecycledDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LastSyncDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LastWebsiteVisit",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LeadQueue",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LeadScore",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LinkedInUrl",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "MeetingsCompleted",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "MeetingsScheduled",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "MergedDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "MobilePhone",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "NextAction",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "NextFollowUpDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "OptInEmail",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "OptInEmailDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "OptInPhone",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "OptInSms",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "PageViews",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "PotentialDuplicates",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "PreferredContactMethod",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "PreferredContactTime",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "PreferredLanguage",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "PrimaryPainPoint",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ProductInterests",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "RecycleCount",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ReferrerName",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ReferrerUrl",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "RequestedDemo",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "RevenueRange",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "SalDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Salutation",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "SecondaryEmail",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "SourceDescription",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "StartedTrial",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Suffix",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "SyncStatus",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Territory",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "TimelineDescription",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Timezone",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "TotalTouchpoints",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "TrialEndDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "TrialStartDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "TrialStatus",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "TwitterHandle",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "UseCase",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "UtmCampaign",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "UtmContent",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "UtmMedium",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "UtmSource",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "UtmTerm",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LeadId",
                table: "Interactions");

            migrationBuilder.DropColumn(
                name: "LeadId",
                table: "ContactInfoLinks");

            migrationBuilder.DropColumn(
                name: "LeadId",
                table: "Activities");

            migrationBuilder.RenameColumn(
                name: "StartedByUserId",
                table: "WorkflowInstances",
                newName: "TriggeredById");

            migrationBuilder.RenameColumn(
                name: "ProcessingWorkerId",
                table: "WorkflowInstances",
                newName: "TriggerEvent");

            migrationBuilder.RenameColumn(
                name: "ProcessingStartedAt",
                table: "WorkflowInstances",
                newName: "TimeoutAt");

            migrationBuilder.RenameColumn(
                name: "LockVersion",
                table: "WorkflowInstances",
                newName: "WorkflowVersionId");

            migrationBuilder.RenameColumn(
                name: "DueAt",
                table: "WorkflowInstances",
                newName: "ScheduledAt");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowInstances_StartedByUserId",
                table: "WorkflowInstances",
                newName: "IX_WorkflowInstances_TriggeredById");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowInstances_DueAt",
                table: "WorkflowInstances",
                newName: "IX_WorkflowInstances_ScheduledAt");

            migrationBuilder.RenameColumn(
                name: "VersionNumber",
                table: "WorkflowDefinitions",
                newName: "MaxConcurrentInstances");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "WorkflowDefinitions",
                newName: "Color");

            migrationBuilder.RenameColumn(
                name: "TriggerType",
                table: "WorkflowDefinitions",
                newName: "IconName");

            migrationBuilder.RenameColumn(
                name: "TriggerEvents",
                table: "WorkflowDefinitions",
                newName: "Tags");

            migrationBuilder.RenameColumn(
                name: "TriggerEntityType",
                table: "WorkflowDefinitions",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "WorkflowDefinitions",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowDefinitions_LastModifiedByUserId",
                table: "WorkflowDefinitions",
                newName: "IX_WorkflowDefinitions_OwnerId");

            migrationBuilder.RenameColumn(
                name: "WinReasonCategory",
                table: "Opportunities",
                newName: "TermLengthMonths");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "Opportunities",
                newName: "PricingModel");

            migrationBuilder.RenameColumn(
                name: "TotalDaysOpen",
                table: "Opportunities",
                newName: "SalesOwnerId");

            migrationBuilder.RenameColumn(
                name: "StakeholderCount",
                table: "Opportunities",
                newName: "QualificationReason");

            migrationBuilder.RenameColumn(
                name: "SalesManagerUserId",
                table: "Opportunities",
                newName: "LeadId");

            migrationBuilder.RenameColumn(
                name: "SalesManagerId",
                table: "Opportunities",
                newName: "AccountId1");

            migrationBuilder.RenameIndex(
                name: "IX_Opportunities_SalesManagerId",
                table: "Opportunities",
                newName: "IX_Opportunities_AccountId1");

            migrationBuilder.RenameColumn(
                name: "WebsiteVisits",
                table: "Leads",
                newName: "Score");

            migrationBuilder.RenameColumn(
                name: "WebinarsAttended",
                table: "Leads",
                newName: "EngagementScore");

            migrationBuilder.RenameColumn(
                name: "ReferredByCustomerId",
                table: "Leads",
                newName: "MarketingCampaignId2");

            migrationBuilder.RenameColumn(
                name: "PrimaryProductInterestId",
                table: "Leads",
                newName: "MarketingCampaignId1");

            migrationBuilder.RenameColumn(
                name: "PrimaryCampaignId",
                table: "Leads",
                newName: "MarketingCampaignId");

            migrationBuilder.RenameColumn(
                name: "PartnerId",
                table: "Leads",
                newName: "ContactId");

            migrationBuilder.RenameColumn(
                name: "NumberOfEmployees",
                table: "Leads",
                newName: "CampaignId");

            migrationBuilder.RenameColumn(
                name: "MasterLeadId",
                table: "Leads",
                newName: "AccountId");

            migrationBuilder.RenameIndex(
                name: "IX_Leads_ReferredByCustomerId",
                table: "Leads",
                newName: "IX_Leads_MarketingCampaignId2");

            migrationBuilder.RenameIndex(
                name: "IX_Leads_PrimaryProductInterestId",
                table: "Leads",
                newName: "IX_Leads_MarketingCampaignId1");

            migrationBuilder.RenameIndex(
                name: "IX_Leads_PrimaryCampaignId",
                table: "Leads",
                newName: "IX_Leads_MarketingCampaignId");

            migrationBuilder.RenameIndex(
                name: "IX_Leads_MasterLeadId",
                table: "Leads",
                newName: "IX_Leads_AccountId");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "WorkflowInstances",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "Priority",
                table: "WorkflowInstances",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ErrorMessage",
                table: "WorkflowInstances",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "WorkflowInstances",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "WorkflowInstances",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "WorkflowInstances",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CurrentNodeId",
                table: "WorkflowInstances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorStackTrace",
                table: "WorkflowInstances",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "InputData",
                table: "WorkflowInstances",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "WorkflowInstances",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxRetries",
                table: "WorkflowInstances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OutputData",
                table: "WorkflowInstances",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ParentInstanceId",
                table: "WorkflowInstances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StateData",
                table: "WorkflowInstances",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "WorkflowDefinitions",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WorkflowDefinitions",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CurrentVersion",
                table: "WorkflowDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DefaultTimeoutHours",
                table: "WorkflowDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "WorkflowDefinitions",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "WorkflowDefinitions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "WorkflowDefinitions",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "WorkflowKey",
                table: "WorkflowDefinitions",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Region",
                table: "Opportunities",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "Probability",
                table: "Opportunities",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Opportunities",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Opportunities",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<int>(
                name: "AccountId",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Opportunities",
                type: "varchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "QualificationNotes",
                table: "Opportunities",
                type: "varchar(4000)",
                maxLength: 4000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SolutionNotes",
                table: "Opportunities",
                type: "varchar(4000)",
                maxLength: 4000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Website",
                table: "Leads",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Tags",
                table: "Leads",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Region",
                table: "Leads",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "QualificationNotes",
                table: "Leads",
                type: "varchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Leads",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Leads",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Leads",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Leads",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Leads",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Leads",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Dashboards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsSystem = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IconName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ColumnCount = table.Column<int>(type: "int", nullable: false),
                    RefreshIntervalSeconds = table.Column<int>(type: "int", nullable: false),
                    LayoutConfig = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerId = table.Column<int>(type: "int", nullable: true),
                    Visibility = table.Column<int>(type: "int", nullable: false),
                    AllowedRoles = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dashboards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dashboards_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LeadProductInterests",
                columns: table => new
                {
                    LeadId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    InterestLevel = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadProductInterests", x => new { x.LeadId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_LeadProductInterests_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeadProductInterests_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OpportunityProducts",
                columns: table => new
                {
                    OpportunityId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityProducts", x => new { x.OpportunityId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_OpportunityProducts_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkflowVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkflowDefinitionId = table.Column<int>(type: "int", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChangeLog = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PublishedById = table.Column<int>(type: "int", nullable: true),
                    DeprecatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CanvasLayout = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowVersions_Users_PublishedById",
                        column: x => x.PublishedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkflowVersions_WorkflowDefinitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DashboardWidgets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DashboardId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subtitle = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WidgetType = table.Column<int>(type: "int", nullable: false),
                    DataSource = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RowIndex = table.Column<int>(type: "int", nullable: false),
                    ColumnIndex = table.Column<int>(type: "int", nullable: false),
                    ColumnSpan = table.Column<int>(type: "int", nullable: false),
                    RowSpan = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsVisible = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IconName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BackgroundColor = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NavigationLink = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConfigJson = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShowTrend = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TrendPeriodDays = table.Column<int>(type: "int", nullable: false),
                    RefreshIntervalSeconds = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardWidgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DashboardWidgets_Dashboards_DashboardId",
                        column: x => x.DashboardId,
                        principalTable: "Dashboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkflowNodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkflowVersionId = table.Column<int>(type: "int", nullable: false),
                    NodeKey = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NodeType = table.Column<int>(type: "int", nullable: false),
                    NodeSubType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PositionX = table.Column<double>(type: "double", precision: 10, scale: 2, nullable: false),
                    PositionY = table.Column<double>(type: "double", precision: 10, scale: 2, nullable: false),
                    Width = table.Column<double>(type: "double", precision: 10, scale: 2, nullable: false),
                    Height = table.Column<double>(type: "double", precision: 10, scale: 2, nullable: false),
                    IconName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsStartNode = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsEndNode = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Configuration = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TimeoutMinutes = table.Column<int>(type: "int", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    RetryDelaySeconds = table.Column<int>(type: "int", nullable: false),
                    UseExponentialBackoff = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ExecutionOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowNodes_WorkflowVersions_WorkflowVersionId",
                        column: x => x.WorkflowVersionId,
                        principalTable: "WorkflowVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkflowTransitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkflowVersionId = table.Column<int>(type: "int", nullable: false),
                    SourceNodeId = table.Column<int>(type: "int", nullable: false),
                    TargetNodeId = table.Column<int>(type: "int", nullable: false),
                    TransitionKey = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Label = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConditionType = table.Column<int>(type: "int", nullable: false),
                    ConditionExpression = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    SourceHandle = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetHandle = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LineStyle = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AnimationStyle = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitions_WorkflowNodes_SourceNodeId",
                        column: x => x.SourceNodeId,
                        principalTable: "WorkflowNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitions_WorkflowNodes_TargetNodeId",
                        column: x => x.TargetNodeId,
                        principalTable: "WorkflowNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitions_WorkflowVersions_WorkflowVersionId",
                        column: x => x.WorkflowVersionId,
                        principalTable: "WorkflowVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkflowNodeInstances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkflowInstanceId = table.Column<int>(type: "int", nullable: false),
                    WorkflowNodeId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    InputData = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OutputData = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorStackTrace = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsSkipped = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SkipReason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExecutionSequence = table.Column<int>(type: "int", nullable: false),
                    WorkerId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransitionTakenId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowNodeInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowNodeInstances_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowNodeInstances_WorkflowNodes_WorkflowNodeId",
                        column: x => x.WorkflowNodeId,
                        principalTable: "WorkflowNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowNodeInstances_WorkflowTransitions_TransitionTakenId",
                        column: x => x.TransitionTakenId,
                        principalTable: "WorkflowTransitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkflowLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkflowInstanceId = table.Column<int>(type: "int", nullable: false),
                    WorkflowNodeId = table.Column<int>(type: "int", nullable: true),
                    NodeInstanceId = table.Column<int>(type: "int", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Message = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Details = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    WorkerId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    ExceptionType = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StackTrace = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkflowLogs_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowLogs_WorkflowNodeInstances_NodeInstanceId",
                        column: x => x.NodeInstanceId,
                        principalTable: "WorkflowNodeInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkflowLogs_WorkflowNodes_WorkflowNodeId",
                        column: x => x.WorkflowNodeId,
                        principalTable: "WorkflowNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkflowTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkflowInstanceId = table.Column<int>(type: "int", nullable: false),
                    WorkflowNodeId = table.Column<int>(type: "int", nullable: false),
                    NodeInstanceId = table.Column<int>(type: "int", nullable: true),
                    TaskType = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    QueueName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ScheduledAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PickedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DueAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TimeoutAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LockedByWorkerId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LockExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AssignedToId = table.Column<int>(type: "int", nullable: true),
                    AssignedToRole = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InputData = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OutputData = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FormSchema = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FormData = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorStackTrace = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    MaxRetries = table.Column<int>(type: "int", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeadLetter = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeadLetterReason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeadLetterAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_Users_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_WorkflowNodeInstances_NodeInstanceId",
                        column: x => x.NodeInstanceId,
                        principalTable: "WorkflowNodeInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_WorkflowNodes_WorkflowNodeId",
                        column: x => x.WorkflowNodeId,
                        principalTable: "WorkflowNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_CorrelationId",
                table: "WorkflowInstances",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_CurrentNodeId",
                table: "WorkflowInstances",
                column: "CurrentNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_NextRetryAt",
                table: "WorkflowInstances",
                column: "NextRetryAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_ParentInstanceId",
                table: "WorkflowInstances",
                column: "ParentInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_WorkflowVersionId",
                table: "WorkflowInstances",
                column: "WorkflowVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_Category",
                table: "WorkflowDefinitions",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_EntityType",
                table: "WorkflowDefinitions",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_WorkflowKey",
                table: "WorkflowDefinitions",
                column: "WorkflowKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_ExpectedCloseDate",
                table: "Opportunities",
                column: "ExpectedCloseDate");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_LeadId",
                table: "Opportunities",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_PrimaryContactId",
                table: "Opportunities",
                column: "PrimaryContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_SalesOwnerId",
                table: "Opportunities",
                column: "SalesOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_Stage",
                table: "Opportunities",
                column: "Stage");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CampaignId",
                table: "Leads",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ContactId",
                table: "Leads",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_Score",
                table: "Leads",
                column: "Score");

            migrationBuilder.CreateIndex(
                name: "IX_Dashboards_IsActive",
                table: "Dashboards",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Dashboards_IsDefault",
                table: "Dashboards",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_Dashboards_Name",
                table: "Dashboards",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Dashboards_OwnerId",
                table: "Dashboards",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardWidgets_DashboardId",
                table: "DashboardWidgets",
                column: "DashboardId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardWidgets_DisplayOrder",
                table: "DashboardWidgets",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardWidgets_WidgetType",
                table: "DashboardWidgets",
                column: "WidgetType");

            migrationBuilder.CreateIndex(
                name: "IX_LeadProductInterests_ProductId",
                table: "LeadProductInterests",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityProducts_ProductId",
                table: "OpportunityProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowLogs_Category",
                table: "WorkflowLogs",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowLogs_Level",
                table: "WorkflowLogs",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowLogs_NodeInstanceId",
                table: "WorkflowLogs",
                column: "NodeInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowLogs_Timestamp",
                table: "WorkflowLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowLogs_UserId",
                table: "WorkflowLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowLogs_WorkflowInstanceId",
                table: "WorkflowLogs",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowLogs_WorkflowNodeId",
                table: "WorkflowLogs",
                column: "WorkflowNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowNodeInstances_NextRetryAt",
                table: "WorkflowNodeInstances",
                column: "NextRetryAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowNodeInstances_Status",
                table: "WorkflowNodeInstances",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowNodeInstances_TransitionTakenId",
                table: "WorkflowNodeInstances",
                column: "TransitionTakenId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowNodeInstances_WorkflowInstanceId",
                table: "WorkflowNodeInstances",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowNodeInstances_WorkflowNodeId",
                table: "WorkflowNodeInstances",
                column: "WorkflowNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowNodes_IsEndNode",
                table: "WorkflowNodes",
                column: "IsEndNode");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowNodes_IsStartNode",
                table: "WorkflowNodes",
                column: "IsStartNode");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowNodes_NodeType",
                table: "WorkflowNodes",
                column: "NodeType");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowNodes_WorkflowVersionId_NodeKey",
                table: "WorkflowNodes",
                columns: new[] { "WorkflowVersionId", "NodeKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_AssignedToId",
                table: "WorkflowTasks",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_IsDeadLetter",
                table: "WorkflowTasks",
                column: "IsDeadLetter");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_LockExpiresAt",
                table: "WorkflowTasks",
                column: "LockExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_NodeInstanceId",
                table: "WorkflowTasks",
                column: "NodeInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_Priority",
                table: "WorkflowTasks",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_QueueName",
                table: "WorkflowTasks",
                column: "QueueName");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_ScheduledAt",
                table: "WorkflowTasks",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_Status",
                table: "WorkflowTasks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_WorkflowInstanceId",
                table: "WorkflowTasks",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_WorkflowNodeId",
                table: "WorkflowTasks",
                column: "WorkflowNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_SourceNodeId",
                table: "WorkflowTransitions",
                column: "SourceNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_TargetNodeId",
                table: "WorkflowTransitions",
                column: "TargetNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_WorkflowVersionId",
                table: "WorkflowTransitions",
                column: "WorkflowVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowVersions_PublishedById",
                table: "WorkflowVersions",
                column: "PublishedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowVersions_Status",
                table: "WorkflowVersions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowVersions_WorkflowDefinitionId_VersionNumber",
                table: "WorkflowVersions",
                columns: new[] { "WorkflowDefinitionId", "VersionNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Accounts_AccountId",
                table: "Leads",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Contacts_ContactId",
                table: "Leads",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_MarketingCampaigns_CampaignId",
                table: "Leads",
                column: "CampaignId",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Contacts_PrimaryContactId",
                table: "Opportunities",
                column: "PrimaryContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Customers_CustomerId",
                table: "Opportunities",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Leads_LeadId",
                table: "Opportunities",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Products_ProductId",
                table: "Opportunities",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Users_SalesOwnerId",
                table: "Opportunities",
                column: "SalesOwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowDefinitions_Users_OwnerId",
                table: "WorkflowDefinitions",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstances_Users_TriggeredById",
                table: "WorkflowInstances",
                column: "TriggeredById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstances_WorkflowInstances_ParentInstanceId",
                table: "WorkflowInstances",
                column: "ParentInstanceId",
                principalTable: "WorkflowInstances",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstances_WorkflowNodes_CurrentNodeId",
                table: "WorkflowInstances",
                column: "CurrentNodeId",
                principalTable: "WorkflowNodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstances_WorkflowVersions_WorkflowVersionId",
                table: "WorkflowInstances",
                column: "WorkflowVersionId",
                principalTable: "WorkflowVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Accounts_AccountId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Contacts_ContactId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_MarketingCampaigns_CampaignId",
                table: "Leads");

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

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Contacts_PrimaryContactId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Customers_CustomerId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Leads_LeadId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Products_ProductId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Users_SalesOwnerId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowDefinitions_Users_OwnerId",
                table: "WorkflowDefinitions");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstances_Users_TriggeredById",
                table: "WorkflowInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstances_WorkflowInstances_ParentInstanceId",
                table: "WorkflowInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstances_WorkflowNodes_CurrentNodeId",
                table: "WorkflowInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstances_WorkflowVersions_WorkflowVersionId",
                table: "WorkflowInstances");

            migrationBuilder.DropTable(
                name: "DashboardWidgets");

            migrationBuilder.DropTable(
                name: "LeadProductInterests");

            migrationBuilder.DropTable(
                name: "OpportunityProducts");

            migrationBuilder.DropTable(
                name: "WorkflowLogs");

            migrationBuilder.DropTable(
                name: "WorkflowTasks");

            migrationBuilder.DropTable(
                name: "Dashboards");

            migrationBuilder.DropTable(
                name: "WorkflowNodeInstances");

            migrationBuilder.DropTable(
                name: "WorkflowTransitions");

            migrationBuilder.DropTable(
                name: "WorkflowNodes");

            migrationBuilder.DropTable(
                name: "WorkflowVersions");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowInstances_CorrelationId",
                table: "WorkflowInstances");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowInstances_CurrentNodeId",
                table: "WorkflowInstances");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowInstances_NextRetryAt",
                table: "WorkflowInstances");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowInstances_ParentInstanceId",
                table: "WorkflowInstances");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowInstances_WorkflowVersionId",
                table: "WorkflowInstances");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowDefinitions_Category",
                table: "WorkflowDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowDefinitions_EntityType",
                table: "WorkflowDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowDefinitions_WorkflowKey",
                table: "WorkflowDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_ExpectedCloseDate",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_LeadId",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_PrimaryContactId",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_SalesOwnerId",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_Stage",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Leads_CampaignId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_ContactId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_Score",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "CurrentNodeId",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "ErrorStackTrace",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "InputData",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "MaxRetries",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "OutputData",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "ParentInstanceId",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "StateData",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "CurrentVersion",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "DefaultTimeoutHours",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "WorkflowKey",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "QualificationNotes",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "SolutionNotes",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Leads");

            migrationBuilder.RenameColumn(
                name: "WorkflowVersionId",
                table: "WorkflowInstances",
                newName: "LockVersion");

            migrationBuilder.RenameColumn(
                name: "TriggeredById",
                table: "WorkflowInstances",
                newName: "StartedByUserId");

            migrationBuilder.RenameColumn(
                name: "TriggerEvent",
                table: "WorkflowInstances",
                newName: "ProcessingWorkerId");

            migrationBuilder.RenameColumn(
                name: "TimeoutAt",
                table: "WorkflowInstances",
                newName: "ProcessingStartedAt");

            migrationBuilder.RenameColumn(
                name: "ScheduledAt",
                table: "WorkflowInstances",
                newName: "DueAt");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowInstances_TriggeredById",
                table: "WorkflowInstances",
                newName: "IX_WorkflowInstances_StartedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowInstances_ScheduledAt",
                table: "WorkflowInstances",
                newName: "IX_WorkflowInstances_DueAt");

            migrationBuilder.RenameColumn(
                name: "Tags",
                table: "WorkflowDefinitions",
                newName: "TriggerEvents");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "WorkflowDefinitions",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "MaxConcurrentInstances",
                table: "WorkflowDefinitions",
                newName: "VersionNumber");

            migrationBuilder.RenameColumn(
                name: "IconName",
                table: "WorkflowDefinitions",
                newName: "TriggerType");

            migrationBuilder.RenameColumn(
                name: "Color",
                table: "WorkflowDefinitions",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "WorkflowDefinitions",
                newName: "TriggerEntityType");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowDefinitions_OwnerId",
                table: "WorkflowDefinitions",
                newName: "IX_WorkflowDefinitions_LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "TermLengthMonths",
                table: "Opportunities",
                newName: "WinReasonCategory");

            migrationBuilder.RenameColumn(
                name: "SalesOwnerId",
                table: "Opportunities",
                newName: "TotalDaysOpen");

            migrationBuilder.RenameColumn(
                name: "QualificationReason",
                table: "Opportunities",
                newName: "StakeholderCount");

            migrationBuilder.RenameColumn(
                name: "PricingModel",
                table: "Opportunities",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "LeadId",
                table: "Opportunities",
                newName: "SalesManagerUserId");

            migrationBuilder.RenameColumn(
                name: "AccountId1",
                table: "Opportunities",
                newName: "SalesManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_Opportunities_AccountId1",
                table: "Opportunities",
                newName: "IX_Opportunities_SalesManagerId");

            migrationBuilder.RenameColumn(
                name: "Score",
                table: "Leads",
                newName: "WebsiteVisits");

            migrationBuilder.RenameColumn(
                name: "MarketingCampaignId2",
                table: "Leads",
                newName: "ReferredByCustomerId");

            migrationBuilder.RenameColumn(
                name: "MarketingCampaignId1",
                table: "Leads",
                newName: "PrimaryProductInterestId");

            migrationBuilder.RenameColumn(
                name: "MarketingCampaignId",
                table: "Leads",
                newName: "PrimaryCampaignId");

            migrationBuilder.RenameColumn(
                name: "EngagementScore",
                table: "Leads",
                newName: "WebinarsAttended");

            migrationBuilder.RenameColumn(
                name: "ContactId",
                table: "Leads",
                newName: "PartnerId");

            migrationBuilder.RenameColumn(
                name: "CampaignId",
                table: "Leads",
                newName: "NumberOfEmployees");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "Leads",
                newName: "MasterLeadId");

            migrationBuilder.RenameIndex(
                name: "IX_Leads_MarketingCampaignId2",
                table: "Leads",
                newName: "IX_Leads_ReferredByCustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Leads_MarketingCampaignId1",
                table: "Leads",
                newName: "IX_Leads_PrimaryProductInterestId");

            migrationBuilder.RenameIndex(
                name: "IX_Leads_MarketingCampaignId",
                table: "Leads",
                newName: "IX_Leads_PrimaryCampaignId");

            migrationBuilder.RenameIndex(
                name: "IX_Leads_AccountId",
                table: "Leads",
                newName: "IX_Leads_MasterLeadId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "WorkflowInstances",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "WorkflowInstances",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ErrorMessage",
                table: "WorkflowInstances",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "WorkflowInstances",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ActiveStepKeys",
                table: "WorkflowInstances",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CurrentStepKey",
                table: "WorkflowInstances",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EntityReference",
                table: "WorkflowInstances",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "WorkflowVersion",
                table: "WorkflowInstances",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "WorkflowDefinitions",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "WorkflowDefinitions",
                keyColumn: "Description",
                keyValue: null,
                column: "Description",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WorkflowDefinitions",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "WorkflowDefinitions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorHandlingConfig",
                table: "WorkflowDefinitions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NotificationConfig",
                table: "WorkflowDefinitions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "WorkflowDefinitions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScheduleCron",
                table: "WorkflowDefinitions",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DefaultWorkflowId",
                table: "ServiceRequestSubcategories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentWorkflowStep",
                table: "ServiceRequests",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "WorkflowExecutionId",
                table: "ServiceRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowId",
                table: "ServiceRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Region",
                table: "Opportunities",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<double>(
                name: "Probability",
                table: "Opportunities",
                type: "double",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Opportunities",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "AccountId",
                table: "Opportunities",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualCloseDate",
                table: "Opportunities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiScoreFactors",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "AiWinScore",
                table: "Opportunities",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountInBaseCurrency",
                table: "Opportunities",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualRecurringRevenue",
                table: "Opportunities",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "AssignedToUserId",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AtRiskReason",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Attachments",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "AuthorityConfirmed",
                table: "Opportunities",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "BantScore",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Blockers",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "BudgetAmount",
                table: "Opportunities",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BudgetFiscalYear",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "BudgetStatus",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCase",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CallCount",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CampaignId",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Champion",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ChampionEngagement",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ChampionTitle",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ChangeHistory",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CloseDate",
                table: "Opportunities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CloseDatePushCount",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompetitiveSituation",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CompetitorPrice",
                table: "Opportunities",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompetitorStrengths",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CompetitorWeaknesses",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Competitors",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractEndDate",
                table: "Opportunities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContractLengthMonths",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractStartDate",
                table: "Opportunities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContractType",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CostAmount",
                table: "Opportunities",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CurrentStageEnteredDate",
                table: "Opportunities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomFields",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CustomerBuyingStage",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CustomerTargetDate",
                table: "Opportunities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DaysInCurrentStage",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DaysSinceLastContact",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecisionCriteria",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "DecisionDeadline",
                table: "Opportunities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecisionMakers",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DecisionProcess",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DemoCount",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Opportunities",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "Opportunities",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "Opportunities",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DiscountReason",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "DiscountRequiresApproval",
                table: "Opportunities",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EconomicBuyer",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "EmailCount",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EngagementLevel",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "Opportunities",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ExecutiveSummary",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "ExpectedRevenue",
                table: "Opportunities",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ExpectedSalesCycleDays",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalIds",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ExternalOpportunityId",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FiscalQuarter",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "FiscalYear",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ForecastCategory",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossMarginPercent",
                table: "Opportunities",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "HasProofOfConcept",
                table: "Opportunities",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Health",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImplementationRequirements",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "InternalNotes",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsAtRisk",
                table: "Opportunities",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStalled",
                table: "Opportunities",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivityDate",
                table: "Opportunities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastContactDate",
                table: "Opportunities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMeetingDate",
                table: "Opportunities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedByUserId",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSignificantUpdate",
                table: "Opportunities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncDate",
                table: "Opportunities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeadSource",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LeadSourceDetail",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LossReason",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "LossReasonCategory",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MarketingTouchpoints",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "MaxAllowedDiscount",
                table: "Opportunities",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MeddicCriteria",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MeddicScore",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MeetingCount",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MetricsIdentified",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "NeedConfirmed",
                table: "Opportunities",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextMeetingDate",
                table: "Opportunities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NextStep",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "NextStepDate",
                table: "Opportunities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "OneTimeRevenue",
                table: "Opportunities",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "OpportunityNumber",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "OpportunityType",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "OriginalCloseDate",
                table: "Opportunities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginalLeadId",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PainPoints",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ParentOpportunityId",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PocEndDate",
                table: "Opportunities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PocNotes",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "PocStartDate",
                table: "Opportunities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PocStatus",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PocSuccessCriteria",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "PreviousForecastCategory",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousNextStep",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "PreviousStage",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryCompetitor",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ProbabilityOverridden",
                table: "Opportunities",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ProductCount",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProductFamily",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Products",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProposedSolution",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "QuoteId",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RecurringRevenue",
                table: "Opportunities",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ReferralPartner",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ReferralPartnerId",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedOpportunityIds",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "ResponseRate",
                table: "Opportunities",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiskFactors",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RiskMitigationPlan",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "RiskScore",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SalesEngineerId",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalesEngineerUserId",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Segment",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "ServicesRevenue",
                table: "Opportunities",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SolutionType",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SpecialTerms",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "StageHistory",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "StalledDate",
                table: "Opportunities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StalledReason",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SyncStatus",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TechnicalRequirements",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Territory",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "TimelineConfirmed",
                table: "Opportunities",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TotalActivities",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalContractValue",
                table: "Opportunities",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "WinReason",
                table: "Opportunities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Website",
                table: "Leads",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Tags",
                table: "Leads",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Region",
                table: "Leads",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "QualificationNotes",
                table: "Leads",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Leads",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Leads",
                keyColumn: "LastName",
                keyValue: null,
                column: "LastName",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Leads",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Leads",
                keyColumn: "FirstName",
                keyValue: null,
                column: "FirstName",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Leads",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Leads",
                keyColumn: "Email",
                keyValue: null,
                column: "Email",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Leads",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Address2",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AffiliateCode",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualRevenue",
                table: "Leads",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignmentMethod",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AuthorityLevel",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "BantScore",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BehaviorScore",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "BudgetAmount",
                table: "Leads",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BudgetApproved",
                table: "Leads",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BudgetRange",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CallsConnected",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CallsMade",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CampaignHistory",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CampaignTouchCount",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Company",
                table: "Leads",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CompanyDescription",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CompanySize",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CompetitorChosen",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ContentDownloads",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ConversionType",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ConvertedByUserId",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConvertedContactId",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConvertedCustomerId",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConvertedDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConvertedOpportunityId",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ConvertedRevenue",
                table: "Leads",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConvertingCampaignId",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CurrentSolution",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CustomFields",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DaysSinceLastContact",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DaysToConvert",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DemoCompleted",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DemoCompletedDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DemoRequestDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DisqualificationNotes",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DisqualificationReason",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DisqualifiedByUserId",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DisqualifiedDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DoNotCall",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoNotEmail",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DownloadedContent",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "DuplicateCheckPerformed",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EconomicBuyer",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EmailBounceStatus",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "EmailClicks",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EmailsOpened",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EmailsSent",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EnrichedCompanyData",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "EnrichedDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnrichedPersonData",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EnrichmentSource",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedValue",
                table: "Leads",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EventsAttended",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedPurchaseDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FaxNumber",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Fbclid",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FormData",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Gclid",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "HasAuthority",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasBudget",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasNeed",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasTimeline",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Industry",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IndustryOther",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "InternalNotes",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsConverted",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisqualified",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDuplicate",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnriched",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMql",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSal",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSql",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStale",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LandingPageUrl",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivityDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCallDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastCampaignId",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEmailClickDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEmailOpenDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMeetingDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRecycledDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWebsiteVisit",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeadQueue",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "LeadScore",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LinkedInUrl",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "MeetingsCompleted",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MeetingsScheduled",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "MergedDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobilePhone",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NextAction",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "NextFollowUpDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "OptInEmail",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OptInEmailDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OptInPhone",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OptInSms",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PageViews",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PotentialDuplicates",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PreferredContactMethod",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PreferredContactTime",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PreferredLanguage",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryPainPoint",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProductInterests",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RecycleCount",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReferrerName",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ReferrerUrl",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "RequestedDemo",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RevenueRange",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "SalDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Salutation",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SecondaryEmail",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SourceDescription",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "StartedTrial",
                table: "Leads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Suffix",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SyncStatus",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Territory",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TimelineDescription",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Timezone",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "TotalTouchpoints",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "TrialEndDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TrialStartDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrialStatus",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TwitterHandle",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UseCase",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UtmCampaign",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UtmContent",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UtmMedium",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UtmSource",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UtmTerm",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Leads",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "LeadId",
                table: "Interactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeadId",
                table: "ContactInfoLinks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeadId",
                table: "Activities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkflowApiCredentials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    AuthenticationType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BaseUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CredentialData = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultHeaders = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowApiCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowApiCredentials_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkflowContextVariables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkflowInstanceId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsEncrypted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsSystemVariable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Key = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SetByStepKey = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValueType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowContextVariables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowContextVariables_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkflowDefinitionVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    WorkflowDefinitionId = table.Column<int>(type: "int", nullable: false),
                    ChangeNotes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DefinitionSnapshot = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Version = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WasPublished = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowDefinitionVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowDefinitionVersions_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkflowDefinitionVersions_WorkflowDefinitions_WorkflowDefin~",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkflowEngineEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkflowInstanceId = table.Column<int>(type: "int", nullable: false),
                    ActorId = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActorName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActorType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CorrelationId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    ErrorDetails = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EventType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InputData = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Message = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Metadata = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OutputData = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Severity = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StepKey = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UserAgent = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowEngineEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowEngineEvents_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkflowEngineTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AssignedToGroupId = table.Column<int>(type: "int", nullable: true),
                    AssignedToUserId = table.Column<int>(type: "int", nullable: true),
                    ClaimedByUserId = table.Column<int>(type: "int", nullable: true),
                    CompletedByUserId = table.Column<int>(type: "int", nullable: true),
                    WorkflowInstanceId = table.Column<int>(type: "int", nullable: false),
                    ActionTaken = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssignedToRole = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssignmentType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AvailableActions = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Comments = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DueAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EscalationLevel = table.Column<int>(type: "int", nullable: false),
                    FormData = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FormSchema = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Instructions = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastReminderAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Priority = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReminderCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StepKey = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowEngineTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowEngineTasks_UserGroups_AssignedToGroupId",
                        column: x => x.AssignedToGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkflowEngineTasks_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkflowEngineTasks_Users_ClaimedByUserId",
                        column: x => x.ClaimedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkflowEngineTasks_Users_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkflowEngineTasks_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Workflows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EntityType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflows", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkflowSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkflowDefinitionId = table.Column<int>(type: "int", nullable: false),
                    ContextData = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CronExpression = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExecutionCount = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastTriggeredAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NextTriggerAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TimeZone = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowSchedules_WorkflowDefinitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkflowSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkflowDefinitionId = table.Column<int>(type: "int", nullable: false),
                    Configuration = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsEndStep = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsStartStep = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    PositionX = table.Column<int>(type: "int", nullable: true),
                    PositionY = table.Column<int>(type: "int", nullable: true),
                    RetryPolicy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StepKey = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StepType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TimeoutMinutes = table.Column<int>(type: "int", nullable: false),
                    Transitions = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowSteps_WorkflowDefinitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkflowJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkflowInstanceId = table.Column<int>(type: "int", nullable: true),
                    WorkflowTaskId = table.Column<int>(type: "int", nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CorrelationId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    JobType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaxAttempts = table.Column<int>(type: "int", nullable: false),
                    Payload = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    ProcessingWorkerId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResultData = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ScheduledAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StepKey = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    VisibilityTimeoutAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowJobs_WorkflowEngineTasks_WorkflowTaskId",
                        column: x => x.WorkflowTaskId,
                        principalTable: "WorkflowEngineTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkflowJobs_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkflowRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TargetUserGroupId = table.Column<int>(type: "int", nullable: false),
                    WorkflowId = table.Column<int>(type: "int", nullable: false),
                    ConditionLogic = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false, defaultValue: "AND")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowRules_UserGroups_TargetUserGroupId",
                        column: x => x.TargetUserGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowRules_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkflowExecutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SourceUserGroupId = table.Column<int>(type: "int", nullable: false),
                    TargetUserGroupId = table.Column<int>(type: "int", nullable: false),
                    WorkflowId = table.Column<int>(type: "int", nullable: false),
                    WorkflowRuleId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    EntitySnapshotJson = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EntityType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Success")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowExecutions_UserGroups_SourceUserGroupId",
                        column: x => x.SourceUserGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowExecutions_UserGroups_TargetUserGroupId",
                        column: x => x.TargetUserGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowExecutions_WorkflowRules_WorkflowRuleId",
                        column: x => x.WorkflowRuleId,
                        principalTable: "WorkflowRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkflowExecutions_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkflowRuleConditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkflowRuleId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FieldName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Operator = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Value = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValueTwo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowRuleConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowRuleConditions_WorkflowRules_WorkflowRuleId",
                        column: x => x.WorkflowRuleId,
                        principalTable: "WorkflowRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_Priority",
                table: "WorkflowInstances",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_Status_CurrentStepKey_Priority",
                table: "WorkflowInstances",
                columns: new[] { "Status", "CurrentStepKey", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_CreatedByUserId",
                table: "WorkflowDefinitions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_TriggerEntityType_Status",
                table: "WorkflowDefinitions",
                columns: new[] { "TriggerEntityType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_TriggerType",
                table: "WorkflowDefinitions",
                column: "TriggerType");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestSubcategories_DefaultWorkflowId",
                table: "ServiceRequestSubcategories",
                column: "DefaultWorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_WorkflowExecutionId",
                table: "ServiceRequests",
                column: "WorkflowExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_WorkflowId",
                table: "ServiceRequests",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_AssignedToUserId",
                table: "Opportunities",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_CampaignId",
                table: "Opportunities",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_OriginalLeadId",
                table: "Opportunities",
                column: "OriginalLeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_SalesEngineerId",
                table: "Opportunities",
                column: "SalesEngineerId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ConvertedByUserId",
                table: "Leads",
                column: "ConvertedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ConvertedCustomerId",
                table: "Leads",
                column: "ConvertedCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ConvertedOpportunityId",
                table: "Leads",
                column: "ConvertedOpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ConvertingCampaignId",
                table: "Leads",
                column: "ConvertingCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_DisqualifiedByUserId",
                table: "Leads",
                column: "DisqualifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_LastCampaignId",
                table: "Leads",
                column: "LastCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_LeadScore",
                table: "Leads",
                column: "LeadScore");

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_LeadId",
                table: "Interactions",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactInfoLinks_LeadId",
                table: "ContactInfoLinks",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_LeadId",
                table: "Activities",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowApiCredentials_CreatedByUserId",
                table: "WorkflowApiCredentials",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowApiCredentials_Name",
                table: "WorkflowApiCredentials",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowContextVariables_WorkflowInstanceId_Key",
                table: "WorkflowContextVariables",
                columns: new[] { "WorkflowInstanceId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitionVersions_CreatedByUserId",
                table: "WorkflowDefinitionVersions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitionVersions_WorkflowDefinitionId_Version",
                table: "WorkflowDefinitionVersions",
                columns: new[] { "WorkflowDefinitionId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowEngineEvents_CorrelationId",
                table: "WorkflowEngineEvents",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowEngineEvents_EventType",
                table: "WorkflowEngineEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowEngineEvents_WorkflowInstanceId_Timestamp",
                table: "WorkflowEngineEvents",
                columns: new[] { "WorkflowInstanceId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowEngineTasks_AssignedToGroupId_Status",
                table: "WorkflowEngineTasks",
                columns: new[] { "AssignedToGroupId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowEngineTasks_AssignedToUserId_Status",
                table: "WorkflowEngineTasks",
                columns: new[] { "AssignedToUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowEngineTasks_ClaimedByUserId",
                table: "WorkflowEngineTasks",
                column: "ClaimedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowEngineTasks_CompletedByUserId",
                table: "WorkflowEngineTasks",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowEngineTasks_Status_DueAt",
                table: "WorkflowEngineTasks",
                columns: new[] { "Status", "DueAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowEngineTasks_WorkflowInstanceId",
                table: "WorkflowEngineTasks",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowExecutions_EntityType_EntityId",
                table: "WorkflowExecutions",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowExecutions_SourceUserGroupId",
                table: "WorkflowExecutions",
                column: "SourceUserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowExecutions_TargetUserGroupId",
                table: "WorkflowExecutions",
                column: "TargetUserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowExecutions_WorkflowId_CreatedAt",
                table: "WorkflowExecutions",
                columns: new[] { "WorkflowId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowExecutions_WorkflowRuleId",
                table: "WorkflowExecutions",
                column: "WorkflowRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowJobs_CorrelationId",
                table: "WorkflowJobs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowJobs_Status_ScheduledAt_Priority",
                table: "WorkflowJobs",
                columns: new[] { "Status", "ScheduledAt", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowJobs_VisibilityTimeoutAt",
                table: "WorkflowJobs",
                column: "VisibilityTimeoutAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowJobs_WorkflowInstanceId",
                table: "WorkflowJobs",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowJobs_WorkflowTaskId",
                table: "WorkflowJobs",
                column: "WorkflowTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowRuleConditions_WorkflowRuleId",
                table: "WorkflowRuleConditions",
                column: "WorkflowRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowRules_TargetUserGroupId",
                table: "WorkflowRules",
                column: "TargetUserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowRules_WorkflowId",
                table: "WorkflowRules",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSchedules_IsEnabled_NextTriggerAt",
                table: "WorkflowSchedules",
                columns: new[] { "IsEnabled", "NextTriggerAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSchedules_WorkflowDefinitionId",
                table: "WorkflowSchedules",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_StepType",
                table: "WorkflowSteps",
                column: "StepType");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_WorkflowDefinitionId_StepKey",
                table: "WorkflowSteps",
                columns: new[] { "WorkflowDefinitionId", "StepKey" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Leads_LeadId",
                table: "Activities",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactInfoLinks_Leads_LeadId",
                table: "ContactInfoLinks",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Interactions_Leads_LeadId",
                table: "Interactions",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Customers_ConvertedCustomerId",
                table: "Leads",
                column: "ConvertedCustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Customers_ReferredByCustomerId",
                table: "Leads",
                column: "ReferredByCustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Leads_MasterLeadId",
                table: "Leads",
                column: "MasterLeadId",
                principalTable: "Leads",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_MarketingCampaigns_ConvertingCampaignId",
                table: "Leads",
                column: "ConvertingCampaignId",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_MarketingCampaigns_LastCampaignId",
                table: "Leads",
                column: "LastCampaignId",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_MarketingCampaigns_PrimaryCampaignId",
                table: "Leads",
                column: "PrimaryCampaignId",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Opportunities_ConvertedOpportunityId",
                table: "Leads",
                column: "ConvertedOpportunityId",
                principalTable: "Opportunities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Products_PrimaryProductInterestId",
                table: "Leads",
                column: "PrimaryProductInterestId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Users_ConvertedByUserId",
                table: "Leads",
                column: "ConvertedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Users_DisqualifiedByUserId",
                table: "Leads",
                column: "DisqualifiedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Accounts_AccountId",
                table: "Opportunities",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Customers_CustomerId",
                table: "Opportunities",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Leads_OriginalLeadId",
                table: "Opportunities",
                column: "OriginalLeadId",
                principalTable: "Leads",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_MarketingCampaigns_CampaignId",
                table: "Opportunities",
                column: "CampaignId",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Products_ProductId",
                table: "Opportunities",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Users_AssignedToUserId",
                table: "Opportunities",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Users_SalesEngineerId",
                table: "Opportunities",
                column: "SalesEngineerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Users_SalesManagerId",
                table: "Opportunities",
                column: "SalesManagerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_WorkflowExecutions_WorkflowExecutionId",
                table: "ServiceRequests",
                column: "WorkflowExecutionId",
                principalTable: "WorkflowExecutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Workflows_WorkflowId",
                table: "ServiceRequests",
                column: "WorkflowId",
                principalTable: "Workflows",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequestSubcategories_Workflows_DefaultWorkflowId",
                table: "ServiceRequestSubcategories",
                column: "DefaultWorkflowId",
                principalTable: "Workflows",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowDefinitions_Users_CreatedByUserId",
                table: "WorkflowDefinitions",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowDefinitions_Users_LastModifiedByUserId",
                table: "WorkflowDefinitions",
                column: "LastModifiedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstances_Users_StartedByUserId",
                table: "WorkflowInstances",
                column: "StartedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
