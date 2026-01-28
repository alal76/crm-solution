// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using CRM.Core.Entities;
using CRM.Core.Entities.Workflow;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for campaign execution integrated with workflow engine
/// </summary>
public class CampaignExecutionService
{
    private readonly CrmDbContext _context;
    private readonly WorkflowService _workflowService;
    private readonly WorkflowInstanceService _workflowInstanceService;
    private readonly ILogger<CampaignExecutionService> _logger;

    public CampaignExecutionService(
        CrmDbContext context,
        WorkflowService workflowService,
        WorkflowInstanceService workflowInstanceService,
        ILogger<CampaignExecutionService> logger)
    {
        _context = context;
        _workflowService = workflowService;
        _workflowInstanceService = workflowInstanceService;
        _logger = logger;
    }

    #region Campaign Workflow Configuration

    /// <summary>
    /// Get all workflows linked to a campaign
    /// </summary>
    public async Task<List<CampaignWorkflow>> GetCampaignWorkflowsAsync(int campaignId)
    {
        return await _context.CampaignWorkflows
            .Include(cw => cw.WorkflowDefinition)
            .Where(cw => cw.CampaignId == campaignId && !cw.IsDeleted)
            .ToListAsync();
    }

    /// <summary>
    /// Link a workflow to a campaign
    /// </summary>
    public async Task<CampaignWorkflow> LinkWorkflowToCampaignAsync(
        int campaignId,
        int workflowDefinitionId,
        string workflowType,
        string triggerEvent,
        string? triggerConditions = null,
        int maxExecutionsPerContact = 1,
        int cooldownHours = 24,
        int? userId = null)
    {
        // Validate campaign and workflow exist
        var campaign = await _context.MarketingCampaigns.FindAsync(campaignId);
        var workflow = await _context.WorkflowDefinitions.FindAsync(workflowDefinitionId);

        if (campaign == null)
            throw new ArgumentException("Campaign not found");
        if (workflow == null)
            throw new ArgumentException("Workflow not found");

        // Check for existing link
        var existingLink = await _context.CampaignWorkflows
            .FirstOrDefaultAsync(cw =>
                cw.CampaignId == campaignId &&
                cw.WorkflowDefinitionId == workflowDefinitionId &&
                cw.TriggerEvent == triggerEvent &&
                !cw.IsDeleted);

        if (existingLink != null)
            throw new InvalidOperationException("This workflow is already linked to the campaign with the same trigger");

        var campaignWorkflow = new CampaignWorkflow
        {
            CampaignId = campaignId,
            WorkflowDefinitionId = workflowDefinitionId,
            WorkflowType = workflowType,
            TriggerEvent = triggerEvent,
            TriggerConditions = triggerConditions,
            IsActive = true,
            MaxExecutionsPerContact = maxExecutionsPerContact,
            CooldownHours = cooldownHours,
            CreatedAt = DateTime.UtcNow
        };

        _context.CampaignWorkflows.Add(campaignWorkflow);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Linked workflow {WorkflowId} to campaign {CampaignId} with trigger {Trigger}",
            workflowDefinitionId, campaignId, triggerEvent);

        return campaignWorkflow;
    }

    /// <summary>
    /// Update a campaign workflow configuration
    /// </summary>
    public async Task<CampaignWorkflow?> UpdateCampaignWorkflowAsync(
        int id,
        bool? isActive = null,
        string? triggerConditions = null,
        int? maxExecutionsPerContact = null,
        int? cooldownHours = null)
    {
        var campaignWorkflow = await _context.CampaignWorkflows.FindAsync(id);
        if (campaignWorkflow == null || campaignWorkflow.IsDeleted)
            return null;

        if (isActive.HasValue)
            campaignWorkflow.IsActive = isActive.Value;
        if (triggerConditions != null)
            campaignWorkflow.TriggerConditions = triggerConditions;
        if (maxExecutionsPerContact.HasValue)
            campaignWorkflow.MaxExecutionsPerContact = maxExecutionsPerContact.Value;
        if (cooldownHours.HasValue)
            campaignWorkflow.CooldownHours = cooldownHours.Value;

        campaignWorkflow.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return campaignWorkflow;
    }

    /// <summary>
    /// Remove a workflow from a campaign
    /// </summary>
    public async Task<bool> UnlinkWorkflowFromCampaignAsync(int campaignWorkflowId)
    {
        var campaignWorkflow = await _context.CampaignWorkflows.FindAsync(campaignWorkflowId);
        if (campaignWorkflow == null)
            return false;

        campaignWorkflow.IsDeleted = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Unlinked campaign workflow {Id}", campaignWorkflowId);
        return true;
    }

    #endregion

    #region Campaign Execution

    /// <summary>
    /// Start a campaign - triggers entry workflows
    /// </summary>
    public async Task<bool> StartCampaignAsync(int campaignId, int? userId = null)
    {
        var campaign = await _context.MarketingCampaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        if (campaign == null)
            throw new ArgumentException("Campaign not found");

        if (campaign.Status != CampaignStatus.Scheduled)
        {
            _logger.LogWarning("Cannot start campaign {CampaignId} - current status: {Status}",
                campaignId, campaign.Status);
            return false;
        }

        // Update campaign status to Active
        campaign.Status = CampaignStatus.Active;
        campaign.ActualStartDate = DateTime.UtcNow;
        campaign.UpdatedAt = DateTime.UtcNow;

        // Get entry workflows
        var entryWorkflows = await _context.CampaignWorkflows
            .Include(cw => cw.WorkflowDefinition)
            .Where(cw => cw.CampaignId == campaignId &&
                         cw.TriggerEvent == "campaign_start" &&
                         cw.IsActive &&
                         !cw.IsDeleted)
            .ToListAsync();

        await _context.SaveChangesAsync();

        // Trigger entry workflows
        foreach (var workflow in entryWorkflows)
        {
            await TriggerWorkflowForCampaignAsync(campaignId, workflow.WorkflowDefinitionId, "campaign_start");
        }

        _logger.LogInformation("Started campaign {CampaignId}", campaignId);

        return true;
    }

    /// <summary>
    /// Trigger a workflow for campaign recipients based on an event
    /// </summary>
    public async Task<int> TriggerWorkflowForCampaignAsync(
        int campaignId,
        int workflowDefinitionId,
        string triggerEvent,
        int? specificContactId = null)
    {
        var campaignWorkflow = await _context.CampaignWorkflows
            .FirstOrDefaultAsync(cw =>
                cw.CampaignId == campaignId &&
                cw.WorkflowDefinitionId == workflowDefinitionId &&
                cw.TriggerEvent == triggerEvent &&
                cw.IsActive &&
                !cw.IsDeleted);

        if (campaignWorkflow == null)
        {
            _logger.LogWarning("No active campaign workflow found for campaign {CampaignId}, workflow {WorkflowId}, trigger {Trigger}",
                campaignId, workflowDefinitionId, triggerEvent);
            return 0;
        }

        // Get active workflow version
        var activeVersion = await _workflowService.GetActiveVersionAsync(workflowDefinitionId);
        if (activeVersion == null)
        {
            _logger.LogWarning("No active version found for workflow {WorkflowId}", workflowDefinitionId);
            return 0;
        }

        // Get eligible recipients
        var query = _context.CampaignRecipients
            .Where(r => r.CampaignId == campaignId && !r.IsDeleted);

        if (specificContactId.HasValue)
            query = query.Where(r => r.ContactId == specificContactId.Value);

        var recipients = await query.ToListAsync();
        var triggeredCount = 0;

        foreach (var recipient in recipients)
        {
            if (!recipient.ContactId.HasValue)
                continue;

            // Check execution limits
            if (!await CanExecuteWorkflowForRecipientAsync(campaignWorkflow, recipient.ContactId.Value))
                continue;

            // Create workflow instance
            var contextData = new Dictionary<string, object>
            {
                { "campaignId", campaignId },
                { "contactId", recipient.ContactId.Value },
                { "customerId", recipient.CustomerId ?? 0 },
                { "email", recipient.Email ?? "" },
                { "triggerEvent", triggerEvent }
            };

            var instance = await _workflowInstanceService.StartWorkflowAsync(
                workflowDefinitionId,
                "Contact",
                recipient.ContactId.Value,
                triggerEvent,
                null,
                contextData);

            if (instance != null)
            {
                triggeredCount++;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Triggered workflow {WorkflowId} for {Count} recipients in campaign {CampaignId}",
            workflowDefinitionId, triggeredCount, campaignId);

        return triggeredCount;
    }

    /// <summary>
    /// Check if a workflow can be executed for a recipient based on limits
    /// </summary>
    private async Task<bool> CanExecuteWorkflowForRecipientAsync(CampaignWorkflow campaignWorkflow, int contactId)
    {
        // Get existing executions for this contact
        var executions = await _context.WorkflowInstances
            .Where(i => i.WorkflowDefinitionId == campaignWorkflow.WorkflowDefinitionId &&
                        i.EntityId == contactId &&
                        !i.IsDeleted)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        // Check max executions
        if (executions.Count >= campaignWorkflow.MaxExecutionsPerContact)
            return false;

        // Check cooldown
        var lastExecution = executions.FirstOrDefault();
        if (lastExecution != null && campaignWorkflow.CooldownHours > 0)
        {
            var cooldownEnd = lastExecution.CreatedAt.AddHours(campaignWorkflow.CooldownHours);
            if (DateTime.UtcNow < cooldownEnd)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Process a campaign event for a specific contact
    /// </summary>
    public async Task ProcessCampaignEventAsync(
        int campaignId,
        int contactId,
        string eventType,
        Dictionary<string, object>? eventData = null)
    {
        // Find workflows triggered by this event
        var workflows = await _context.CampaignWorkflows
            .Where(cw => cw.CampaignId == campaignId &&
                         cw.TriggerEvent == eventType &&
                         cw.IsActive &&
                         !cw.IsDeleted)
            .ToListAsync();

        foreach (var workflow in workflows)
        {
            // Check trigger conditions if specified
            if (!string.IsNullOrEmpty(workflow.TriggerConditions))
            {
                if (!EvaluateTriggerConditions(workflow.TriggerConditions, eventData))
                    continue;
            }

            await TriggerWorkflowForCampaignAsync(campaignId, workflow.WorkflowDefinitionId, eventType, contactId);
        }
    }

    /// <summary>
    /// Evaluate trigger conditions against event data
    /// </summary>
    private bool EvaluateTriggerConditions(string conditions, Dictionary<string, object>? eventData)
    {
        if (eventData == null) return false;

        try
        {
            var conditionObj = JsonSerializer.Deserialize<Dictionary<string, object>>(conditions);
            if (conditionObj == null) return true;

            foreach (var condition in conditionObj)
            {
                if (!eventData.TryGetValue(condition.Key, out var value))
                    return false;

                // Simple equality check - can be extended for complex conditions
                if (value?.ToString() != condition.Value?.ToString())
                    return false;
            }

            return true;
        }
        catch (JsonException)
        {
            _logger.LogWarning("Failed to parse trigger conditions: {Conditions}", conditions);
            return true; // Default to allowing if conditions are malformed
        }
    }

    #endregion

    #region Campaign Recipients

    /// <summary>
    /// Get campaign recipients with filtering
    /// </summary>
    public async Task<(List<CampaignRecipient> Items, int TotalCount)> GetCampaignRecipientsAsync(
        int campaignId,
        string? status = null,
        bool? hasOpened = null,
        bool? hasClicked = null,
        bool? hasConverted = null,
        int skip = 0,
        int take = 50)
    {
        var query = _context.CampaignRecipients
            .Include(r => r.Customer)
            .Where(r => r.CampaignId == campaignId && !r.IsDeleted);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status == status);

        if (hasOpened == true)
            query = query.Where(r => r.FirstOpenedAt != null);
        else if (hasOpened == false)
            query = query.Where(r => r.FirstOpenedAt == null);

        if (hasClicked == true)
            query = query.Where(r => r.FirstClickedAt != null);
        else if (hasClicked == false)
            query = query.Where(r => r.FirstClickedAt == null);

        if (hasConverted == true)
            query = query.Where(r => r.ConvertedAt != null);
        else if (hasConverted == false)
            query = query.Where(r => r.ConvertedAt == null);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// Record an email open for a recipient
    /// </summary>
    public async Task RecordEmailOpenAsync(int recipientId, string? ipAddress = null, string? userAgent = null)
    {
        var recipient = await _context.CampaignRecipients.FindAsync(recipientId);
        if (recipient == null || recipient.IsDeleted) return;

        recipient.OpenCount++;
        recipient.LastOpenedAt = DateTime.UtcNow;

        if (recipient.FirstOpenedAt == null)
        {
            recipient.FirstOpenedAt = DateTime.UtcNow;
            recipient.Status = "Opened";

            // Trigger open event workflows
            if (recipient.ContactId.HasValue)
            {
                await ProcessCampaignEventAsync(recipient.CampaignId, recipient.ContactId.Value, "email_opened");
            }
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Record a link click for a recipient
    /// </summary>
    public async Task<CampaignLinkClick> RecordLinkClickAsync(
        int recipientId,
        string linkUrl,
        string? deviceType = null,
        string? browser = null,
        string? ipAddress = null,
        string? locationData = null)
    {
        var recipient = await _context.CampaignRecipients.FindAsync(recipientId);
        if (recipient == null || recipient.IsDeleted)
            throw new ArgumentException("Recipient not found");

        recipient.ClickCount++;
        recipient.LastClickedAt = DateTime.UtcNow;

        if (recipient.FirstClickedAt == null)
        {
            recipient.FirstClickedAt = DateTime.UtcNow;
            recipient.Status = "Clicked";

            // Trigger click event workflows
            if (recipient.ContactId.HasValue)
            {
                await ProcessCampaignEventAsync(recipient.CampaignId, recipient.ContactId.Value, "link_clicked",
                    new Dictionary<string, object> { { "linkUrl", linkUrl } });
            }
        }

        var click = new CampaignLinkClick
        {
            CampaignRecipientId = recipientId,
            LinkUrl = linkUrl,
            ClickedAt = DateTime.UtcNow,
            DeviceType = deviceType,
            Browser = browser,
            IpAddress = ipAddress,
            LocationData = locationData,
            CreatedAt = DateTime.UtcNow
        };

        _context.CampaignLinkClicks.Add(click);
        await _context.SaveChangesAsync();

        return click;
    }

    /// <summary>
    /// Record a conversion for a recipient
    /// </summary>
    public async Task<CampaignConversion> RecordConversionAsync(
        int recipientId,
        string conversionType,
        decimal? conversionValue = null,
        int? orderId = null,
        int? opportunityId = null,
        string attributionModel = "LastTouch",
        decimal attributionPercentage = 100,
        int? userId = null)
    {
        var recipient = await _context.CampaignRecipients
            .Include(r => r.Campaign)
            .FirstOrDefaultAsync(r => r.Id == recipientId);

        if (recipient == null || recipient.IsDeleted)
            throw new ArgumentException("Recipient not found");

        if (recipient.ConvertedAt == null)
        {
            recipient.ConvertedAt = DateTime.UtcNow;
            recipient.ConversionValue = conversionValue;
            recipient.Status = "Converted";

            // Trigger conversion event workflows
            if (recipient.ContactId.HasValue)
            {
                await ProcessCampaignEventAsync(recipient.CampaignId, recipient.ContactId.Value, "converted",
                    new Dictionary<string, object>
                    {
                        { "conversionType", conversionType },
                        { "conversionValue", conversionValue ?? 0 }
                    });
            }
        }

        var conversion = new CampaignConversion
        {
            CampaignId = recipient.CampaignId,
            CampaignRecipientId = recipientId,
            ContactId = recipient.ContactId,
            CustomerId = recipient.CustomerId,
            ConversionType = conversionType,
            ConversionValue = conversionValue,
            AttributionModel = attributionModel,
            AttributionPercentage = attributionPercentage,
            CreatedAt = DateTime.UtcNow
        };

        _context.CampaignConversions.Add(conversion);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Recorded conversion for recipient {RecipientId} - Type: {Type}, Value: {Value}",
            recipientId, conversionType, conversionValue);

        return conversion;
    }

    #endregion

    #region A/B Testing

    /// <summary>
    /// Create an A/B test for a campaign
    /// </summary>
    public async Task<CampaignABTest> CreateABTestAsync(
        int campaignId,
        string testName,
        string testType,
        string testMetric,
        string variantAConfig,
        string variantBConfig,
        int trafficSplit = 50,
        int? minimumSampleSize = null,
        int? testDurationHours = null,
        bool autoSelectWinner = false,
        int? userId = null)
    {
        // Store variant configs as JSON
        var variantConfigs = JsonSerializer.Serialize(new Dictionary<string, string>
        {
            { "A", variantAConfig },
            { "B", variantBConfig }
        });

        var trafficSplitJson = JsonSerializer.Serialize(new Dictionary<string, int>
        {
            { "A", trafficSplit },
            { "B", 100 - trafficSplit }
        });

        var abTest = new CampaignABTest
        {
            CampaignId = campaignId,
            TestName = testName,
            TestType = testType,
            TestMetric = testMetric,
            VariantConfigs = variantConfigs,
            TrafficSplit = trafficSplitJson,
            SampleSize = minimumSampleSize,
            AutoWinnerAfterHours = testDurationHours,
            AutoSelectWinner = autoSelectWinner,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow
        };

        _context.CampaignABTests.Add(abTest);
        await _context.SaveChangesAsync();

        return abTest;
    }

    /// <summary>
    /// Start an A/B test
    /// </summary>
    public async Task<bool> StartABTestAsync(int testId)
    {
        var test = await _context.CampaignABTests.FindAsync(testId);
        if (test == null || test.IsDeleted)
            return false;

        if (test.Status != "Draft")
            return false;

        test.Status = "Running";
        test.TestStartedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Assign a recipient to an A/B test variant
    /// </summary>
    public async Task<string> AssignToABTestVariantAsync(int testId, int recipientId)
    {
        var test = await _context.CampaignABTests.FindAsync(testId);
        if (test == null || test.Status != "Running")
            throw new InvalidOperationException("A/B test is not running");

        var recipient = await _context.CampaignRecipients.FindAsync(recipientId);
        if (recipient == null)
            throw new ArgumentException("Recipient not found");

        // Already assigned?
        if (!string.IsNullOrEmpty(recipient.ABTestVariant))
            return recipient.ABTestVariant;

        // Randomly assign based on traffic split
        int splitA = 50;
        if (!string.IsNullOrEmpty(test.TrafficSplit))
        {
            try
            {
                var split = JsonSerializer.Deserialize<Dictionary<string, int>>(test.TrafficSplit);
                if (split != null && split.TryGetValue("A", out var aPercent))
                    splitA = aPercent;
            }
            catch { }
        }

        var random = new Random();
        var variant = random.Next(100) < splitA ? "A" : "B";

        recipient.ABTestVariant = variant;
        await _context.SaveChangesAsync();
        return variant;
    }

    #endregion

    #region Campaign Analytics

    /// <summary>
    /// Get campaign analytics summary
    /// </summary>
    public async Task<CampaignAnalytics> GetCampaignAnalyticsAsync(int campaignId)
    {
        var campaign = await _context.MarketingCampaigns.FindAsync(campaignId);
        if (campaign == null)
            throw new ArgumentException("Campaign not found");

        var recipients = await _context.CampaignRecipients
            .Where(r => r.CampaignId == campaignId && !r.IsDeleted)
            .ToListAsync();

        var conversions = await _context.CampaignConversions
            .Where(c => c.CampaignId == campaignId && !c.IsDeleted)
            .ToListAsync();

        var totalSent = recipients.Count(r => r.Status != "Pending");
        var delivered = recipients.Count(r => r.Status != "Bounced" && r.Status != "Pending" && r.Status != "Failed");
        var opened = recipients.Count(r => r.FirstOpenedAt != null);
        var clicked = recipients.Count(r => r.FirstClickedAt != null);
        var converted = recipients.Count(r => r.ConvertedAt != null);
        var bounced = recipients.Count(r => r.Status == "Bounced");
        var unsubscribed = recipients.Count(r => r.UnsubscribedAt != null);

        return new CampaignAnalytics
        {
            CampaignId = campaignId,
            CampaignName = campaign.Name,
            TotalRecipients = recipients.Count,
            TotalSent = totalSent,
            TotalDelivered = delivered,
            TotalOpened = opened,
            TotalClicked = clicked,
            TotalConverted = converted,
            TotalBounced = bounced,
            TotalUnsubscribed = unsubscribed,
            UniqueOpens = opened,
            UniqueClicks = clicked,
            TotalOpens = recipients.Sum(r => r.OpenCount),
            TotalClicks = recipients.Sum(r => r.ClickCount),
            DeliveryRate = totalSent > 0 ? (decimal)delivered / totalSent * 100 : 0,
            OpenRate = delivered > 0 ? (decimal)opened / delivered * 100 : 0,
            ClickRate = delivered > 0 ? (decimal)clicked / delivered * 100 : 0,
            ClickToOpenRate = opened > 0 ? (decimal)clicked / opened * 100 : 0,
            ConversionRate = delivered > 0 ? (decimal)converted / delivered * 100 : 0,
            BounceRate = totalSent > 0 ? (decimal)bounced / totalSent * 100 : 0,
            UnsubscribeRate = delivered > 0 ? (decimal)unsubscribed / delivered * 100 : 0,
            TotalRevenue = conversions.Sum(c => c.ConversionValue ?? 0),
            AverageOrderValue = conversions.Count > 0 ? conversions.Average(c => c.ConversionValue ?? 0) : 0,
            ROI = campaign.Budget > 0
                ? (conversions.Sum(c => c.ConversionValue ?? 0) - campaign.Budget) / campaign.Budget * 100
                : 0
        };
    }

    #endregion
}

/// <summary>
/// Campaign analytics summary DTO
/// </summary>
public class CampaignAnalytics
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    
    // Counts
    public int TotalRecipients { get; set; }
    public int TotalSent { get; set; }
    public int TotalDelivered { get; set; }
    public int TotalOpened { get; set; }
    public int TotalClicked { get; set; }
    public int TotalConverted { get; set; }
    public int TotalBounced { get; set; }
    public int TotalUnsubscribed { get; set; }
    public int UniqueOpens { get; set; }
    public int UniqueClicks { get; set; }
    public int TotalOpens { get; set; }
    public int TotalClicks { get; set; }

    // Rates
    public decimal DeliveryRate { get; set; }
    public decimal OpenRate { get; set; }
    public decimal ClickRate { get; set; }
    public decimal ClickToOpenRate { get; set; }
    public decimal ConversionRate { get; set; }
    public decimal BounceRate { get; set; }
    public decimal UnsubscribeRate { get; set; }

    // Revenue
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal ROI { get; set; }
}
