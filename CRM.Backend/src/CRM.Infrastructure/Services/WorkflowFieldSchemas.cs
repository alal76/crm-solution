// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.Workflow;

namespace CRM.Infrastructure.Services;

/// <summary>
/// PRA-004: Centralised entity field and related-entity schema definitions for workflow
/// condition/action editors.  Previously these 12+ schemas were hardcoded inline inside
/// WorkflowController.GetEntityFieldsInternal() and GetRelatedEntitiesInternal().
/// Extracting them here makes them discoverable, testable, and easy to extend without
/// touching the controller.
/// </summary>
public static class WorkflowFieldSchemas
{
    private const string FieldTypeString = "string";
    private const string FieldTypeNumber = "number";
    private const string FieldTypeBoolean = "boolean";
    private const string FieldTypeDate = "date";

    /// <summary>
    /// Entity field schemas keyed by entity type name.
    /// Each entry describes the filterable/settable fields for workflow conditions and actions.
    /// </summary>
    public static readonly Dictionary<string, List<EntityFieldConfig>> EntityFields = new()
    {
        ["Lead"] = new()
        {
            new() { Name = "Status", Label = "Status", Type = "enum", Required = true, EnumValues = new List<string> { "New", "Working", "Nurturing", "Qualified", "Disqualified", "Converted" }, Group = "Status" },
            new() { Name = "Source", Label = "Source", Type = "enum", Required = false, EnumValues = new List<string> { "Web", "Campaign", "Referral", "Event", "Partner", "Manual" }, Group = "Status" },
            new() { Name = "LeadScore", Label = "Lead Score", Type = FieldTypeNumber, Required = false, Group = "Scoring" },
            new() { Name = "Title", Label = "Title", Type = FieldTypeString, Required = false, Group = "Contact" },
            new() { Name = "Company", Label = "Company", Type = FieldTypeString, Required = false, Group = "Company" },
            new() { Name = "Email", Label = "Email", Type = FieldTypeString, Required = false, Group = "Contact" },
            new() { Name = "Phone", Label = "Phone", Type = FieldTypeString, Required = false, Group = "Contact" },
            new() { Name = "OwnerId", Label = "Owner", Type = "reference", Required = false, ReferenceEntity = "User", Group = "Assignment" },
            new() { Name = "AccountId", Label = "Account", Type = "reference", Required = false, ReferenceEntity = "Account", Group = "Related" },
            new() { Name = "CampaignId", Label = "Campaign", Type = "reference", Required = false, ReferenceEntity = "Campaign", Group = "Marketing" },
            new() { Name = "CreatedAt", Label = "Created Date", Type = FieldTypeDate, Required = false, Group = "Audit" },
            new() { Name = "UpdatedAt", Label = "Updated Date", Type = FieldTypeDate, Required = false, Group = "Audit" },
        },
        ["Opportunity"] = new()
        {
            new() { Name = "Stage", Label = "Stage", Type = "enum", Required = true, EnumValues = new List<string> { "Prospecting", "Qualification", "Proposal", "Negotiation", "ClosedWon", "ClosedLost" }, Group = "Status" },
            new() { Name = "Name", Label = "Name", Type = FieldTypeString, Required = true, Group = "Basic" },
            new() { Name = "Amount", Label = "Amount", Type = FieldTypeNumber, Required = false, Group = "Value" },
            new() { Name = "Probability", Label = "Probability (%)", Type = FieldTypeNumber, Required = false, Group = "Value" },
            new() { Name = "ExpectedCloseDate", Label = "Expected Close Date", Type = FieldTypeDate, Required = false, Group = "Dates" },
            new() { Name = "Type", Label = "Type", Type = "enum", Required = false, EnumValues = new List<string> { "NewBusiness", "ExistingBusiness", "Renewal", "Upsell" }, Group = "Classification" },
            new() { Name = "OwnerId", Label = "Owner", Type = "reference", Required = false, ReferenceEntity = "User", Group = "Assignment" },
            new() { Name = "AccountId", Label = "Account", Type = "reference", Required = false, ReferenceEntity = "Account", Group = "Related" },
            new() { Name = "LeadId", Label = "Lead", Type = "reference", Required = false, ReferenceEntity = "Lead", Group = "Related" },
            new() { Name = "IsClosed", Label = "Is Closed", Type = FieldTypeBoolean, Required = false, Group = "Status" },
            new() { Name = "IsWon", Label = "Is Won", Type = FieldTypeBoolean, Required = false, Group = "Status" },
        },
        ["ServiceRequest"] = new()
        {
            new() { Name = "Status", Label = "Status", Type = "enum", Required = true, EnumValues = new List<string> { "New", "Open", "InProgress", "Pending", "Resolved", "Closed" }, Group = "Status" },
            new() { Name = "Priority", Label = "Priority", Type = "enum", Required = true, EnumValues = new List<string> { "Low", "Medium", "High", "Urgent", "Critical" }, Group = "Status" },
            new() { Name = "Subject", Label = "Subject", Type = FieldTypeString, Required = true, Group = "Basic" },
            new() { Name = "Description", Label = "Description", Type = FieldTypeString, Required = false, Group = "Basic" },
            new() { Name = "Type", Label = "Type", Type = "enum", Required = false, EnumValues = new List<string> { "Question", "Problem", "Incident", "Request", "Task" }, Group = "Classification" },
            new() { Name = "OwnerId", Label = "Owner", Type = "reference", Required = false, ReferenceEntity = "User", Group = "Assignment" },
            new() { Name = "AccountId", Label = "Account", Type = "reference", Required = false, ReferenceEntity = "Account", Group = "Related" },
            new() { Name = "DueDate", Label = "Due Date", Type = FieldTypeDate, Required = false, Group = "Dates" },
            new() { Name = "SLABreachedAt", Label = "SLA Breach Date", Type = FieldTypeDate, Required = false, Group = "SLA" },
        },
        ["Contact"] = new()
        {
            new() { Name = "FirstName", Label = "First Name", Type = FieldTypeString, Required = true, Group = "Basic" },
            new() { Name = "LastName", Label = "Last Name", Type = FieldTypeString, Required = true, Group = "Basic" },
            new() { Name = "Email", Label = "Email", Type = FieldTypeString, Required = false, Group = "Contact" },
            new() { Name = "Phone", Label = "Phone", Type = FieldTypeString, Required = false, Group = "Contact" },
            new() { Name = "Title", Label = "Job Title", Type = FieldTypeString, Required = false, Group = "Business" },
            new() { Name = "Department", Label = "Department", Type = FieldTypeString, Required = false, Group = "Business" },
            new() { Name = "AccountId", Label = "Account", Type = "reference", Required = false, ReferenceEntity = "Account", Group = "Related" },
            new() { Name = "OwnerId", Label = "Owner", Type = "reference", Required = false, ReferenceEntity = "User", Group = "Assignment" },
            new() { Name = "IsPrimary", Label = "Is Primary", Type = FieldTypeBoolean, Required = false, Group = "Status" },
        },
        ["Campaign"] = new()
        {
            new() { Name = "Status", Label = "Status", Type = "enum", Required = true, EnumValues = new List<string> { "Draft", "Scheduled", "Active", "Paused", "Completed", "Cancelled" }, Group = "Status" },
            new() { Name = "Name", Label = "Name", Type = FieldTypeString, Required = true, Group = "Basic" },
            new() { Name = "Type", Label = "Type", Type = "enum", Required = false, EnumValues = new List<string> { "Email", "Social", "Event", "Webinar", "Advertisement", "Direct" }, Group = "Classification" },
            new() { Name = "StartDate", Label = "Start Date", Type = FieldTypeDate, Required = false, Group = "Dates" },
            new() { Name = "EndDate", Label = "End Date", Type = FieldTypeDate, Required = false, Group = "Dates" },
            new() { Name = "Budget", Label = "Budget", Type = FieldTypeNumber, Required = false, Group = "Budget" },
            new() { Name = "ActualCost", Label = "Actual Cost", Type = FieldTypeNumber, Required = false, Group = "Budget" },
            new() { Name = "OwnerId", Label = "Owner", Type = "reference", Required = false, ReferenceEntity = "User", Group = "Assignment" },
        },
        ["Account"] = new()
        {
            new() { Name = "Status", Label = "Status", Type = "enum", Required = true, EnumValues = new List<string> { "Active", "Inactive", "Prospect", "Churned" }, Group = "Status" },
            new() { Name = "Name", Label = "Name", Type = FieldTypeString, Required = true, Group = "Basic" },
            new() { Name = "Email", Label = "Email", Type = FieldTypeString, Required = false, Group = "Contact" },
            new() { Name = "Phone", Label = "Phone", Type = FieldTypeString, Required = false, Group = "Contact" },
            new() { Name = "Type", Label = "Type", Type = "enum", Required = false, EnumValues = new List<string> { "Prospect", "Customer", "Partner", "Vendor", "Other" }, Group = "Classification" },
            new() { Name = "Industry", Label = "Industry", Type = FieldTypeString, Required = false, Group = "Business" },
            new() { Name = "Website", Label = "Website", Type = FieldTypeString, Required = false, Group = "Contact" },
            new() { Name = "AnnualRevenue", Label = "Annual Revenue", Type = FieldTypeNumber, Required = false, Group = "Business" },
            new() { Name = "EmployeeCount", Label = "Employee Count", Type = FieldTypeNumber, Required = false, Group = "Business" },
            new() { Name = "OwnerId", Label = "Owner", Type = "reference", Required = false, ReferenceEntity = "User", Group = "Assignment" },
        },
        ["Quote"] = new()
        {
            new() { Name = "Status", Label = "Status", Type = "enum", Required = true, EnumValues = new List<string> { "Draft", "Pending", "Approved", "Rejected", "Expired" }, Group = "Status" },
            new() { Name = "Name", Label = "Name", Type = FieldTypeString, Required = true, Group = "Basic" },
            new() { Name = "TotalAmount", Label = "Total Amount", Type = FieldTypeNumber, Required = false, Group = "Value" },
            new() { Name = "Discount", Label = "Discount (%)", Type = FieldTypeNumber, Required = false, Group = "Value" },
            new() { Name = "ExpirationDate", Label = "Expiration Date", Type = FieldTypeDate, Required = false, Group = "Dates" },
            new() { Name = "OpportunityId", Label = "Opportunity", Type = "reference", Required = false, ReferenceEntity = "Opportunity", Group = "Related" },
            new() { Name = "AccountId", Label = "Account", Type = "reference", Required = false, ReferenceEntity = "Account", Group = "Related" },
            new() { Name = "OwnerId", Label = "Owner", Type = "reference", Required = false, ReferenceEntity = "User", Group = "Assignment" },
        },
    };

    /// <summary>
    /// Related-entity schemas keyed by entity type name.
    /// Each entry describes navigation relationships exposed in workflow node configuration.
    /// </summary>
    public static readonly Dictionary<string, List<RelatedEntityConfig>> RelatedEntities = new()
    {
        ["Lead"] = new()
        {
            new() { Name = "Owner", Label = "Owner (User)", EntityType = "User", RelationType = "parent" },
            new() { Name = "Account", Label = "Account", EntityType = "Account", RelationType = "parent" },
            new() { Name = "Campaign", Label = "Campaign", EntityType = "Campaign", RelationType = "parent" },
        },
        ["Opportunity"] = new()
        {
            new() { Name = "Owner", Label = "Owner (User)", EntityType = "User", RelationType = "parent" },
            new() { Name = "Account", Label = "Account", EntityType = "Account", RelationType = "parent" },
            new() { Name = "Lead", Label = "Source Lead", EntityType = "Lead", RelationType = "parent" },
            new() { Name = "Quotes", Label = "Quotes", EntityType = "Quote", RelationType = "child" },
        },
        ["Account"] = new()
        {
            new() { Name = "Owner", Label = "Owner (User)", EntityType = "User", RelationType = "parent" },
            new() { Name = "Contacts", Label = "Contacts", EntityType = "Contact", RelationType = "child" },
            new() { Name = "Opportunities", Label = "Opportunities", EntityType = "Opportunity", RelationType = "child" },
            new() { Name = "ServiceRequests", Label = "Service Requests", EntityType = "ServiceRequest", RelationType = "child" },
            new() { Name = "Accounts", Label = "Accounts", EntityType = "Account", RelationType = "child" },
        },
        ["ServiceRequest"] = new()
        {
            new() { Name = "Owner", Label = "Owner (User)", EntityType = "User", RelationType = "parent" },
            new() { Name = "Account", Label = "Account", EntityType = "Account", RelationType = "parent" },
        },
        ["Contact"] = new()
        {
            new() { Name = "Owner", Label = "Owner (User)", EntityType = "User", RelationType = "parent" },
            new() { Name = "Account", Label = "Account", EntityType = "Account", RelationType = "parent" },
        },
        ["Campaign"] = new()
        {
            new() { Name = "Owner", Label = "Owner (User)", EntityType = "User", RelationType = "parent" },
            new() { Name = "Leads", Label = "Leads", EntityType = "Lead", RelationType = "child" },
        },
        ["Quote"] = new()
        {
            new() { Name = "Owner", Label = "Owner (User)", EntityType = "User", RelationType = "parent" },
            new() { Name = "Opportunity", Label = "Opportunity", EntityType = "Opportunity", RelationType = "parent" },
            new() { Name = "Account", Label = "Account", EntityType = "Account", RelationType = "parent" },
        },
    };
}
