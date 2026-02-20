// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;
using CRM.Core.Entities;

namespace CRM.Core.Mappers;

public static class CampaignMapper
{
    // ── Helper: DateTime? → ISO-8601 string? ─────────────────────────────────
    private static string? ToIso(DateTime? dt) =>
        dt.HasValue ? dt.Value.ToString("o") : null;

    // ── Helper: ISO-8601 string? → DateTime? ─────────────────────────────────
    private static DateTime? FromIso(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : DateTime.Parse(s, null, System.Globalization.DateTimeStyles.RoundtripKind);

    public static CampaignDto ToDto(MarketingCampaign entity)
    {
        return new CampaignDto
        {
            // Identity
            Id = entity.Id,
            Name = entity.Name,
            CampaignCode = entity.CampaignCode,
            Description = entity.Description,

            // Enums (int)
            Objective = (int)entity.ObjectiveType,
            CampaignType = (int)entity.CampaignType,
            Status = (int)entity.Status,
            Priority = (int)entity.Priority,

            // Basic Info
            ObjectiveText = entity.Objective,
            Type = entity.Type,
            PrimarySuccessMetric = (int)entity.PrimarySuccessMetric,
            Theme = entity.Theme,
            ValueProposition = entity.ValueProposition,

            // Timeline (ISO-8601 strings)
            StartDate = ToIso(entity.StartDate),
            EndDate = ToIso(entity.EndDate),
            ActualStartDate = ToIso(entity.ActualStartDate),
            ActualEndDate = ToIso(entity.ActualEndDate),
            DurationDays = entity.DurationDays,
            IsEvergreen = entity.IsEvergreen,
            Timezone = entity.Timezone,
            Schedule = entity.Schedule,

            // Budget & Financials
            Budget = entity.Budget,
            ActualCost = entity.ActualCost,
            DailyBudget = entity.DailyBudget,
            MonthlyBudget = entity.MonthlyBudget,
            ExpectedRevenue = entity.ExpectedRevenue,
            ActualRevenue = entity.ActualRevenue ?? 0,
            PipelineInfluenced = entity.PipelineInfluenced,
            PipelineCreated = entity.PipelineCreated,
            CostPerLead = entity.CostPerLead,
            CostPerMql = entity.CostPerMql,
            CostPerSql = entity.CostPerSql,
            CostPerOpportunity = entity.CostPerOpportunity,
            CostPerAcquisition = entity.CostPerAcquisition,
            CurrencyCode = entity.CurrencyCode,

            // Legacy computed aliases
            Roi = (decimal)entity.ROI,
            Cpl = entity.CostPerLead ?? 0,
            Cpa = entity.CostPerAcquisition ?? 0,

            // Target Audience
            TargetAudience = entity.TargetAudience,
            TargetAudienceDescription = entity.TargetAudienceDescription,
            AudienceType = (int)entity.AudienceType,
            TargetDemographics = entity.TargetDemographics,
            TargetFirmographics = entity.TargetFirmographics,
            TargetGeography = entity.TargetGeography,
            TargetIndustries = entity.TargetIndustries,
            TargetSegments = entity.TargetSegments,
            TargetPersonas = entity.TargetPersonas,
            TargetJobTitles = entity.TargetJobTitles,
            TargetSeniorityLevels = entity.TargetSeniorityLevels,
            TargetAccounts = entity.TargetAccounts,
            ExclusionCriteria = entity.ExclusionCriteria,
            SuppressionLists = entity.SuppressionLists,
            AudienceListId = entity.AudienceListId,

            // Lead Generation Metrics
            LeadsGenerated = entity.LeadsGenerated,
            MqlsGenerated = entity.MqlsGenerated,
            SqlsGenerated = entity.SqlsGenerated,
            SalsGenerated = entity.SalsGenerated,
            OpportunitiesCreated = entity.OpportunitiesCreated,
            OpportunitiesInfluenced = entity.OpportunitiesInfluenced,
            DealsWon = entity.DealsWon,
            AccountsAcquired = entity.AccountsAcquired,
            LeadToMqlRate = entity.LeadToMqlRate,
            MqlToSqlRate = entity.MqlToSqlRate,
            SqlToOpportunityRate = entity.SqlToOpportunityRate,
            OpportunityToWinRate = entity.OpportunityToWinRate,
            ConversionRate = entity.ConversionRate,
            AverageLeadScore = entity.AverageLeadScore,
            LeadQualityDistribution = entity.LeadQualityDistribution,

            // Reach & Engagement
            Impressions = entity.Impressions,
            Reach = entity.Reach,
            Frequency = entity.Frequency,
            Clicks = entity.Clicks,
            ClickThroughRate = entity.ClickThroughRate,
            LandingPageVisits = entity.LandingPageVisits,
            FormSubmissions = entity.FormSubmissions,
            FormConversionRate = entity.FormConversionRate,
            ContentDownloads = entity.ContentDownloads,
            VideoViews = entity.VideoViews,
            VideoCompletionRate = entity.VideoCompletionRate,
            DemoRequests = entity.DemoRequests,
            TrialSignups = entity.TrialSignups,

            // Email Campaign Metrics
            EmailsSent = entity.EmailsSent,
            EmailsDelivered = entity.EmailsDelivered,
            DeliveryRate = entity.DeliveryRate,
            EmailsOpened = entity.EmailsOpened,
            OpenRate = entity.OpenRate,
            EmailClicks = entity.EmailClicks,
            EmailClickRate = entity.EmailClickRate,
            ClickToOpenRate = entity.ClickToOpenRate,
            HardBounces = entity.HardBounces,
            SoftBounces = entity.SoftBounces,
            Bounces = entity.Bounces,
            BounceRate = entity.BounceRate,
            Unsubscribes = entity.Unsubscribes,
            UnsubscribeRate = entity.UnsubscribeRate,
            SpamComplaints = entity.SpamComplaints,
            ComplaintRate = entity.ComplaintRate,
            EmailForwards = entity.EmailForwards,
            ListGrowth = entity.ListGrowth,

            // Social Media Metrics
            SocialReach = entity.SocialReach,
            SocialEngagement = entity.SocialEngagement,
            SocialEngagementRate = entity.SocialEngagementRate,
            SocialShares = entity.SocialShares,
            SocialComments = entity.SocialComments,
            SocialLikes = entity.SocialLikes,
            SocialSaves = entity.SocialSaves,
            NewFollowers = entity.NewFollowers,
            ProfileVisits = entity.ProfileVisits,
            Mentions = entity.Mentions,
            SentimentScore = entity.SentimentScore,

            // Paid Advertising Metrics
            AdSpend = entity.AdSpend,
            CostPerClick = entity.CostPerClick,
            CostPerMille = entity.CostPerMille,
            Roas = entity.Roas,
            QualityScore = entity.QualityScore,
            AveragePosition = entity.AveragePosition,
            ImpressionShare = entity.ImpressionShare,
            Keywords = entity.Keywords,
            NegativeKeywords = entity.NegativeKeywords,

            // Event / Webinar Metrics
            Registrations = entity.Registrations,
            Attendance = entity.Attendance,
            AttendanceRate = entity.AttendanceRate,
            NoShows = entity.NoShows,
            EventCapacity = entity.EventCapacity,
            EventLocation = entity.EventLocation,
            EventDateTime = ToIso(entity.EventDateTime),
            WebinarPlatform = entity.WebinarPlatform,
            WebinarRecordingUrl = entity.WebinarRecordingUrl,
            OnDemandViews = entity.OnDemandViews,
            PollResponses = entity.PollResponses,
            QuestionsAsked = entity.QuestionsAsked,
            EventSatisfactionScore = entity.EventSatisfactionScore,

            // ROI & Performance
            ROI = entity.ROI,
            TargetRoi = entity.TargetRoi,
            TargetLeads = entity.TargetLeads,
            TargetConversions = entity.TargetConversions,
            GoalAchievementPercent = entity.GoalAchievementPercent,
            CampaignHealthScore = entity.CampaignHealthScore,
            BenchmarkComparison = entity.BenchmarkComparison,

            // Content & Creative
            MessageSubject = entity.MessageSubject,
            PreheaderText = entity.PreheaderText,
            MessageBody = entity.MessageBody,
            FromName = entity.FromName,
            FromEmail = entity.FromEmail,
            ReplyToEmail = entity.ReplyToEmail,
            CallToAction = entity.CallToAction,
            CtaUrl = entity.CtaUrl,
            LandingPageUrl = entity.LandingPageUrl,
            TrackingUrl = entity.TrackingUrl,
            CreativeAssets = entity.CreativeAssets,
            TemplateId = entity.TemplateId,

            // UTM Tracking
            UtmSource = entity.UtmSource,
            UtmMedium = entity.UtmMedium,
            UtmCampaign = entity.UtmCampaign,
            UtmContent = entity.UtmContent,
            UtmTerm = entity.UtmTerm,

            // A/B Testing
            IsABTest = entity.IsABTest,
            ABTestVariants = entity.ABTestVariants,
            ABTestMetric = entity.ABTestMetric,
            WinningVariant = entity.WinningVariant,
            StatisticalSignificance = entity.StatisticalSignificance,
            ABTestResults = entity.ABTestResults,

            // Channels & Platforms
            Channels = entity.Channels,
            Platforms = entity.Platforms,
            SocialNetworks = entity.SocialNetworks,
            AdPlatforms = entity.AdPlatforms,
            ExternalCampaignIds = entity.ExternalCampaignIds,

            // Assignment & Ownership
            OwnerId = entity.OwnerId,
            OwnerName = entity.Owner?.FullName,
            AssignedToUserId = entity.AssignedToUserId,
            TeamMembers = entity.TeamMembers,
            ApprovedByUserId = entity.ApprovedByUserId,
            ApprovedDate = ToIso(entity.ApprovedDate),
            Department = entity.Department,
            CostCenter = entity.CostCenter,

            // Campaign Hierarchy
            ParentCampaignId = entity.ParentCampaignId,
            RelatedCampaigns = entity.RelatedCampaigns,
            Program = entity.Program,
            Initiative = entity.Initiative,
            FiscalQuarter = entity.FiscalQuarter,
            FiscalYear = entity.FiscalYear,

            // Classification & Tags
            Tags = entity.Tags,
            Category = entity.Category,
            SubCategory = entity.SubCategory,
            Region = entity.Region,

            // Documentation & Notes
            Notes = entity.Notes,
            InternalNotes = entity.InternalNotes,
            SuccessCriteria = entity.SuccessCriteria,
            LessonsLearned = entity.LessonsLearned,
            Attachments = entity.Attachments,
            BriefUrl = entity.BriefUrl,
            ReportUrl = entity.ReportUrl,

            // Custom Fields & Integration
            CustomFields = entity.CustomFields,
            ExternalId = entity.ExternalId,
            SyncStatus = entity.SyncStatus,
            LastSyncDate = ToIso(entity.LastSyncDate),

            // Calculated / Audit
            IsActive = entity.IsActive,
            IsOverBudget = entity.IsOverBudget,
            BudgetRemaining = entity.BudgetRemaining,
            BudgetUtilization = entity.BudgetUtilization,
            DaysRemaining = entity.DaysRemaining,
            IsEndingSoon = entity.IsEndingSoon,
            OverallEngagementRate = entity.OverallEngagementRate,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
    }

    public static MarketingCampaign ToEntity(CreateCampaignDto dto)
    {
        return new MarketingCampaign
        {
            // Basic Info
            Name = dto.Name,
            CampaignCode = dto.CampaignCode,
            Description = dto.Description ?? string.Empty,
            Objective = dto.Objective,
            ObjectiveType = (CampaignObjective)(dto.ObjectiveType ?? 0),
            CampaignType = (CampaignType)dto.CampaignType,
            Priority = (CampaignPriority)dto.Priority,
            Status = dto.Status.HasValue ? (CampaignStatus)dto.Status.Value : CampaignStatus.Draft,
            Type = dto.Type ?? string.Empty,
            PrimarySuccessMetric = dto.PrimarySuccessMetric.HasValue
                ? (SuccessMetric)dto.PrimarySuccessMetric.Value
                : SuccessMetric.LeadsGenerated,
            Theme = dto.Theme,
            ValueProposition = dto.ValueProposition,

            // Timeline
            StartDate = FromIso(dto.StartDate),
            EndDate = FromIso(dto.EndDate),
            ActualStartDate = FromIso(dto.ActualStartDate),
            ActualEndDate = FromIso(dto.ActualEndDate),
            DurationDays = dto.DurationDays,
            IsEvergreen = dto.IsEvergreen,
            Timezone = dto.Timezone,
            Schedule = dto.Schedule,

            // Budget & Financials
            Budget = dto.Budget,
            ActualCost = dto.ActualCost,
            DailyBudget = dto.DailyBudget,
            MonthlyBudget = dto.MonthlyBudget,
            ExpectedRevenue = dto.ExpectedRevenue,
            ActualRevenue = dto.ActualRevenue,
            CurrencyCode = dto.CurrencyCode,

            // Target Audience
            TargetAudience = dto.TargetAudience,
            TargetAudienceDescription = dto.TargetAudienceDescription,
            AudienceType = dto.AudienceType.HasValue ? (AudienceType)dto.AudienceType.Value : AudienceType.Prospects,
            TargetDemographics = dto.TargetDemographics,
            TargetFirmographics = dto.TargetFirmographics,
            TargetGeography = dto.TargetGeography,
            TargetIndustries = dto.TargetIndustries,
            TargetSegments = dto.TargetSegments,
            TargetPersonas = dto.TargetPersonas,
            TargetJobTitles = dto.TargetJobTitles,
            TargetSeniorityLevels = dto.TargetSeniorityLevels,
            TargetAccounts = dto.TargetAccounts,
            ExclusionCriteria = dto.ExclusionCriteria,
            SuppressionLists = dto.SuppressionLists,
            AudienceListId = dto.AudienceListId,

            // Content & Creative
            MessageSubject = dto.MessageSubject,
            PreheaderText = dto.PreheaderText,
            MessageBody = dto.MessageBody,
            FromName = dto.FromName,
            FromEmail = dto.FromEmail,
            ReplyToEmail = dto.ReplyToEmail,
            CallToAction = dto.CallToAction,
            CtaUrl = dto.CtaUrl,
            LandingPageUrl = dto.LandingPageUrl,
            TrackingUrl = dto.TrackingUrl,
            CreativeAssets = dto.CreativeAssets,
            TemplateId = dto.TemplateId,

            // UTM Tracking
            UtmSource = dto.UtmSource,
            UtmMedium = dto.UtmMedium,
            UtmCampaign = dto.UtmCampaign,
            UtmContent = dto.UtmContent,
            UtmTerm = dto.UtmTerm,

            // A/B Testing
            IsABTest = dto.IsABTest,
            ABTestVariants = dto.ABTestVariants,
            ABTestMetric = dto.ABTestMetric,

            // Channels & Platforms
            Channels = dto.Channels,
            Platforms = dto.Platforms,
            SocialNetworks = dto.SocialNetworks,
            AdPlatforms = dto.AdPlatforms,
            ExternalCampaignIds = dto.ExternalCampaignIds,

            // Assignment & Ownership
            OwnerId = dto.OwnerId,
            AssignedToUserId = dto.AssignedToUserId,
            TeamMembers = dto.TeamMembers,
            Department = dto.Department,
            CostCenter = dto.CostCenter,

            // Campaign Hierarchy
            ParentCampaignId = dto.ParentCampaignId,
            RelatedCampaigns = dto.RelatedCampaigns,
            Program = dto.Program,
            Initiative = dto.Initiative,
            FiscalQuarter = dto.FiscalQuarter,
            FiscalYear = dto.FiscalYear,

            // Classification & Tags
            Tags = dto.Tags,
            Category = dto.Category,
            SubCategory = dto.SubCategory,
            Region = dto.Region,

            // ROI Targets
            TargetRoi = dto.TargetRoi,
            TargetLeads = dto.TargetLeads,
            TargetConversions = dto.TargetConversions,

            // Documentation
            Notes = dto.Notes,
            InternalNotes = dto.InternalNotes,
            SuccessCriteria = dto.SuccessCriteria,
            BriefUrl = dto.BriefUrl,

            // Custom Fields & Integration
            CustomFields = dto.CustomFields,
            ExternalId = dto.ExternalId,

            // Event / Webinar
            EventCapacity = dto.EventCapacity,
            EventLocation = dto.EventLocation,
            EventDateTime = FromIso(dto.EventDateTime),
            WebinarPlatform = dto.WebinarPlatform,
            WebinarRecordingUrl = dto.WebinarRecordingUrl,

            // Paid Advertising
            Keywords = dto.Keywords,
            NegativeKeywords = dto.NegativeKeywords,
        };
    }

    public static void UpdateEntity(MarketingCampaign entity, UpdateCampaignDto dto)
    {
        // Basic Info
        if (dto.Name != null) entity.Name = dto.Name;
        if (dto.CampaignCode != null) entity.CampaignCode = dto.CampaignCode;
        if (dto.Description != null) entity.Description = dto.Description;
        if (dto.Objective != null) entity.Objective = dto.Objective;
        if (dto.ObjectiveType.HasValue) entity.ObjectiveType = (CampaignObjective)dto.ObjectiveType.Value;
        if (dto.CampaignType.HasValue) entity.CampaignType = (CampaignType)dto.CampaignType.Value;
        if (dto.Priority.HasValue) entity.Priority = (CampaignPriority)dto.Priority.Value;
        if (dto.Status.HasValue) entity.Status = (CampaignStatus)dto.Status.Value;
        if (dto.Type != null) entity.Type = dto.Type;
        if (dto.PrimarySuccessMetric.HasValue) entity.PrimarySuccessMetric = (SuccessMetric)dto.PrimarySuccessMetric.Value;
        if (dto.Theme != null) entity.Theme = dto.Theme;
        if (dto.ValueProposition != null) entity.ValueProposition = dto.ValueProposition;

        // Timeline
        if (dto.StartDate != null) entity.StartDate = FromIso(dto.StartDate);
        if (dto.EndDate != null) entity.EndDate = FromIso(dto.EndDate);
        if (dto.ActualStartDate != null) entity.ActualStartDate = FromIso(dto.ActualStartDate);
        if (dto.ActualEndDate != null) entity.ActualEndDate = FromIso(dto.ActualEndDate);
        if (dto.DurationDays.HasValue) entity.DurationDays = dto.DurationDays;
        if (dto.IsEvergreen.HasValue) entity.IsEvergreen = dto.IsEvergreen.Value;
        if (dto.Timezone != null) entity.Timezone = dto.Timezone;
        if (dto.Schedule != null) entity.Schedule = dto.Schedule;

        // Budget & Financials
        if (dto.Budget.HasValue) entity.Budget = dto.Budget.Value;
        if (dto.ActualCost.HasValue) entity.ActualCost = dto.ActualCost.Value;
        if (dto.DailyBudget.HasValue) entity.DailyBudget = dto.DailyBudget;
        if (dto.MonthlyBudget.HasValue) entity.MonthlyBudget = dto.MonthlyBudget;
        if (dto.ExpectedRevenue.HasValue) entity.ExpectedRevenue = dto.ExpectedRevenue;
        if (dto.ActualRevenue.HasValue) entity.ActualRevenue = dto.ActualRevenue;
        if (dto.CurrencyCode != null) entity.CurrencyCode = dto.CurrencyCode;

        // Target Audience
        if (dto.TargetAudience.HasValue) entity.TargetAudience = dto.TargetAudience.Value;
        if (dto.TargetAudienceDescription != null) entity.TargetAudienceDescription = dto.TargetAudienceDescription;
        if (dto.AudienceType.HasValue) entity.AudienceType = (AudienceType)dto.AudienceType.Value;
        if (dto.TargetDemographics != null) entity.TargetDemographics = dto.TargetDemographics;
        if (dto.TargetFirmographics != null) entity.TargetFirmographics = dto.TargetFirmographics;
        if (dto.TargetGeography != null) entity.TargetGeography = dto.TargetGeography;
        if (dto.TargetIndustries != null) entity.TargetIndustries = dto.TargetIndustries;
        if (dto.TargetSegments != null) entity.TargetSegments = dto.TargetSegments;
        if (dto.TargetPersonas != null) entity.TargetPersonas = dto.TargetPersonas;
        if (dto.TargetJobTitles != null) entity.TargetJobTitles = dto.TargetJobTitles;
        if (dto.TargetSeniorityLevels != null) entity.TargetSeniorityLevels = dto.TargetSeniorityLevels;
        if (dto.TargetAccounts != null) entity.TargetAccounts = dto.TargetAccounts;
        if (dto.ExclusionCriteria != null) entity.ExclusionCriteria = dto.ExclusionCriteria;
        if (dto.SuppressionLists != null) entity.SuppressionLists = dto.SuppressionLists;
        if (dto.AudienceListId != null) entity.AudienceListId = dto.AudienceListId;

        // Content & Creative
        if (dto.MessageSubject != null) entity.MessageSubject = dto.MessageSubject;
        if (dto.PreheaderText != null) entity.PreheaderText = dto.PreheaderText;
        if (dto.MessageBody != null) entity.MessageBody = dto.MessageBody;
        if (dto.FromName != null) entity.FromName = dto.FromName;
        if (dto.FromEmail != null) entity.FromEmail = dto.FromEmail;
        if (dto.ReplyToEmail != null) entity.ReplyToEmail = dto.ReplyToEmail;
        if (dto.CallToAction != null) entity.CallToAction = dto.CallToAction;
        if (dto.CtaUrl != null) entity.CtaUrl = dto.CtaUrl;
        if (dto.LandingPageUrl != null) entity.LandingPageUrl = dto.LandingPageUrl;
        if (dto.TrackingUrl != null) entity.TrackingUrl = dto.TrackingUrl;
        if (dto.CreativeAssets != null) entity.CreativeAssets = dto.CreativeAssets;
        if (dto.TemplateId != null) entity.TemplateId = dto.TemplateId;

        // UTM Tracking
        if (dto.UtmSource != null) entity.UtmSource = dto.UtmSource;
        if (dto.UtmMedium != null) entity.UtmMedium = dto.UtmMedium;
        if (dto.UtmCampaign != null) entity.UtmCampaign = dto.UtmCampaign;
        if (dto.UtmContent != null) entity.UtmContent = dto.UtmContent;
        if (dto.UtmTerm != null) entity.UtmTerm = dto.UtmTerm;

        // A/B Testing
        if (dto.IsABTest.HasValue) entity.IsABTest = dto.IsABTest.Value;
        if (dto.ABTestVariants != null) entity.ABTestVariants = dto.ABTestVariants;
        if (dto.ABTestMetric != null) entity.ABTestMetric = dto.ABTestMetric;
        if (dto.WinningVariant != null) entity.WinningVariant = dto.WinningVariant;

        // Channels & Platforms
        if (dto.Channels != null) entity.Channels = dto.Channels;
        if (dto.Platforms != null) entity.Platforms = dto.Platforms;
        if (dto.SocialNetworks != null) entity.SocialNetworks = dto.SocialNetworks;
        if (dto.AdPlatforms != null) entity.AdPlatforms = dto.AdPlatforms;
        if (dto.ExternalCampaignIds != null) entity.ExternalCampaignIds = dto.ExternalCampaignIds;

        // Assignment & Ownership
        if (dto.OwnerId.HasValue) entity.OwnerId = dto.OwnerId;
        if (dto.AssignedToUserId.HasValue) entity.AssignedToUserId = dto.AssignedToUserId;
        if (dto.TeamMembers != null) entity.TeamMembers = dto.TeamMembers;
        if (dto.Department != null) entity.Department = dto.Department;
        if (dto.CostCenter != null) entity.CostCenter = dto.CostCenter;

        // Campaign Hierarchy
        if (dto.ParentCampaignId.HasValue) entity.ParentCampaignId = dto.ParentCampaignId;
        if (dto.RelatedCampaigns != null) entity.RelatedCampaigns = dto.RelatedCampaigns;
        if (dto.Program != null) entity.Program = dto.Program;
        if (dto.Initiative != null) entity.Initiative = dto.Initiative;
        if (dto.FiscalQuarter != null) entity.FiscalQuarter = dto.FiscalQuarter;
        if (dto.FiscalYear.HasValue) entity.FiscalYear = dto.FiscalYear;

        // Classification & Tags
        if (dto.Tags != null) entity.Tags = dto.Tags;
        if (dto.Category != null) entity.Category = dto.Category;
        if (dto.SubCategory != null) entity.SubCategory = dto.SubCategory;
        if (dto.Region != null) entity.Region = dto.Region;

        // ROI Targets
        if (dto.TargetRoi.HasValue) entity.TargetRoi = dto.TargetRoi;
        if (dto.TargetLeads.HasValue) entity.TargetLeads = dto.TargetLeads;
        if (dto.TargetConversions.HasValue) entity.TargetConversions = dto.TargetConversions;

        // Documentation & Notes
        if (dto.Notes != null) entity.Notes = dto.Notes;
        if (dto.InternalNotes != null) entity.InternalNotes = dto.InternalNotes;
        if (dto.SuccessCriteria != null) entity.SuccessCriteria = dto.SuccessCriteria;
        if (dto.LessonsLearned != null) entity.LessonsLearned = dto.LessonsLearned;
        if (dto.Attachments != null) entity.Attachments = dto.Attachments;
        if (dto.BriefUrl != null) entity.BriefUrl = dto.BriefUrl;
        if (dto.ReportUrl != null) entity.ReportUrl = dto.ReportUrl;

        // Custom Fields & Integration
        if (dto.CustomFields != null) entity.CustomFields = dto.CustomFields;
        if (dto.ExternalId != null) entity.ExternalId = dto.ExternalId;
        if (dto.SyncStatus != null) entity.SyncStatus = dto.SyncStatus;

        // Event / Webinar
        if (dto.EventCapacity.HasValue) entity.EventCapacity = dto.EventCapacity;
        if (dto.EventLocation != null) entity.EventLocation = dto.EventLocation;
        if (dto.EventDateTime != null) entity.EventDateTime = FromIso(dto.EventDateTime);
        if (dto.WebinarPlatform != null) entity.WebinarPlatform = dto.WebinarPlatform;
        if (dto.WebinarRecordingUrl != null) entity.WebinarRecordingUrl = dto.WebinarRecordingUrl;

        // Paid Advertising
        if (dto.Keywords != null) entity.Keywords = dto.Keywords;
        if (dto.NegativeKeywords != null) entity.NegativeKeywords = dto.NegativeKeywords;
        if (dto.AdSpend.HasValue) entity.AdSpend = dto.AdSpend;
        if (dto.CostPerClick.HasValue) entity.CostPerClick = dto.CostPerClick;
        if (dto.CostPerMille.HasValue) entity.CostPerMille = dto.CostPerMille;
        if (dto.Roas.HasValue) entity.Roas = dto.Roas;

        // Metrics that can be manually set
        if (dto.LeadsGenerated.HasValue) entity.LeadsGenerated = dto.LeadsGenerated.Value;
        if (dto.MqlsGenerated.HasValue) entity.MqlsGenerated = dto.MqlsGenerated.Value;
        if (dto.SqlsGenerated.HasValue) entity.SqlsGenerated = dto.SqlsGenerated.Value;
        if (dto.SalsGenerated.HasValue) entity.SalsGenerated = dto.SalsGenerated.Value;
        if (dto.OpportunitiesCreated.HasValue) entity.OpportunitiesCreated = dto.OpportunitiesCreated.Value;
        if (dto.OpportunitiesInfluenced.HasValue) entity.OpportunitiesInfluenced = dto.OpportunitiesInfluenced.Value;
        if (dto.DealsWon.HasValue) entity.DealsWon = dto.DealsWon.Value;
        if (dto.AccountsAcquired.HasValue) entity.AccountsAcquired = dto.AccountsAcquired.Value;
        if (dto.Impressions.HasValue) entity.Impressions = dto.Impressions.Value;
        if (dto.Reach.HasValue) entity.Reach = dto.Reach.Value;
        if (dto.Clicks.HasValue) entity.Clicks = dto.Clicks.Value;
        if (dto.EmailsSent.HasValue) entity.EmailsSent = dto.EmailsSent.Value;
        if (dto.EmailsDelivered.HasValue) entity.EmailsDelivered = dto.EmailsDelivered.Value;
        if (dto.EmailsOpened.HasValue) entity.EmailsOpened = dto.EmailsOpened.Value;
        if (dto.EmailClicks.HasValue) entity.EmailClicks = dto.EmailClicks.Value;
        if (dto.HardBounces.HasValue) entity.HardBounces = dto.HardBounces.Value;
        if (dto.SoftBounces.HasValue) entity.SoftBounces = dto.SoftBounces.Value;
        if (dto.Bounces.HasValue) entity.Bounces = dto.Bounces.Value;
        if (dto.Unsubscribes.HasValue) entity.Unsubscribes = dto.Unsubscribes.Value;
        if (dto.SpamComplaints.HasValue) entity.SpamComplaints = dto.SpamComplaints.Value;
        if (dto.Registrations.HasValue) entity.Registrations = dto.Registrations.Value;
        if (dto.Attendance.HasValue) entity.Attendance = dto.Attendance.Value;
        if (dto.NoShows.HasValue) entity.NoShows = dto.NoShows.Value;
        if (dto.PipelineInfluenced.HasValue) entity.PipelineInfluenced = dto.PipelineInfluenced;
        if (dto.PipelineCreated.HasValue) entity.PipelineCreated = dto.PipelineCreated;
    }
}
