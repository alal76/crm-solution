// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

#if ITSM_ADVANCED
// This file is part of the CRM Solution.
// Copyright (c) 2025 CRM Solution Contributors
// Licensed under the AGPL-3.0 license.

using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Interface for Knowledge-Centered Service (KCS) workflow management.
/// </summary>
public interface IKCSWorkflowService
{
    /// <summary>
    /// Start the KCS workflow for an incident (capture in context).
    /// </summary>
    Task<KCSSession> StartCaptureSessionAsync(int incidentId, int agentId);

    /// <summary>
    /// Create a draft article from incident resolution.
    /// </summary>
    Task<KCSDraftArticle> CreateDraftFromIncidentAsync(int incidentId, int authorId);

    /// <summary>
    /// Submit article for review (evolve loop).
    /// </summary>
    Task<KCSReviewRequest> SubmitForReviewAsync(int articleId, int submittedById);

    /// <summary>
    /// Review and approve/reject an article.
    /// </summary>
    Task<KCSReviewResult> ReviewArticleAsync(int reviewRequestId, int reviewerId, KCSReviewDecision decision, string? feedback = null);

    /// <summary>
    /// Publish an approved article.
    /// </summary>
    Task<KCSPublishResult> PublishArticleAsync(int articleId, int publishedById, PublishAudience audience);

    /// <summary>
    /// Get KCS metrics for an agent.
    /// </summary>
    Task<KCSAgentMetrics> GetAgentMetricsAsync(int agentId, DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Get KCS article lifecycle status.
    /// </summary>
    Task<KCSArticleLifecycle> GetArticleLifecycleAsync(int articleId);

    /// <summary>
    /// Flag an article for review or retirement.
    /// </summary>
    Task<bool> FlagArticleAsync(int articleId, KCSFlag flag, int flaggedById, string reason);

    /// <summary>
    /// Get the KCS coaching queue for reviewers.
    /// </summary>
    Task<List<KCSCoachingItem>> GetCoachingQueueAsync(int reviewerId);

    /// <summary>
    /// Record reuse of an article to solve an incident.
    /// </summary>
    Task RecordArticleReuseAsync(int articleId, int incidentId, ReuseOutcome outcome);
}

/// <summary>
/// A KCS capture session for an incident.
/// </summary>
public class KCSSession
{
    public int SessionId { get; set; }
    public int IncidentId { get; set; }
    public string IncidentNumber { get; set; } = string.Empty;
    public int AgentId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public KCSSessionState State { get; set; }
    public List<int> SearchedArticleIds { get; set; } = new();
    public int? UsedArticleId { get; set; }
    public int? CreatedArticleId { get; set; }
    public bool WasKnowledgeGap { get; set; }
}

public enum KCSSessionState
{
    Active,
    ArticleReused,
    ArticleCreated,
    NoActionTaken,
    Abandoned
}

/// <summary>
/// A draft KCS article created from incident.
/// </summary>
public class KCSDraftArticle
{
    public int DraftId { get; set; }
    public int? SourceIncidentId { get; set; }
    public string? SourceIncidentNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Symptom { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty;
    public string? Cause { get; set; }
    public List<string> Keywords { get; set; } = new();
    public string? Category { get; set; }
    public int AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public DateTime CreatedAt { get; set; }
    public KCSArticleQuality QualityAssessment { get; set; } = new();
}

/// <summary>
/// Quality assessment for a KCS article.
/// </summary>
public class KCSArticleQuality
{
    public int OverallScore { get; set; }
    public bool HasClearTitle { get; set; }
    public bool HasSymptom { get; set; }
    public bool HasEnvironment { get; set; }
    public bool HasResolution { get; set; }
    public bool HasKeywords { get; set; }
    public List<string> ImprovementSuggestions { get; set; } = new();
}

/// <summary>
/// Request for article review.
/// </summary>
public class KCSReviewRequest
{
    public int ReviewRequestId { get; set; }
    public int ArticleId { get; set; }
    public string ArticleNumber { get; set; } = string.Empty;
    public string ArticleTitle { get; set; } = string.Empty;
    public int SubmittedById { get; set; }
    public string? SubmittedByName { get; set; }
    public DateTime SubmittedAt { get; set; }
    public ReviewType Type { get; set; }
    public KCSReviewState State { get; set; }
    public int? AssignedReviewerId { get; set; }
    public string? AssignedReviewerName { get; set; }
    public DateTime? DueAt { get; set; }
}

public enum ReviewType
{
    NewArticle,
    UpdatedArticle,
    FlaggedForReview,
    PeriodicReview,
    CoachingReview
}

public enum KCSReviewState
{
    Pending,
    InReview,
    Approved,
    Rejected,
    NeedsWork,
    Escalated
}

/// <summary>
/// Result of an article review.
/// </summary>
public class KCSReviewResult
{
    public int ReviewRequestId { get; set; }
    public int ArticleId { get; set; }
    public KCSReviewDecision Decision { get; set; }
    public int ReviewerId { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime ReviewedAt { get; set; }
    public string? Feedback { get; set; }
    public KCSArticleQuality QualityAssessment { get; set; } = new();
    public List<string> RequiredChanges { get; set; } = new();
}

public enum KCSReviewDecision
{
    Approve,
    ApproveWithChanges,
    Reject,
    NeedsWork,
    Escalate
}

/// <summary>
/// Result of publishing an article.
/// </summary>
public class KCSPublishResult
{
    public int ArticleId { get; set; }
    public string ArticleNumber { get; set; } = string.Empty;
    public bool Success { get; set; }
    public PublishAudience Audience { get; set; }
    public DateTime PublishedAt { get; set; }
    public int PublishedById { get; set; }
    public string? Message { get; set; }
}

public enum PublishAudience
{
    Internal, // IT staff only
    AllEmployees, // Self-service portal
    Public // Public knowledge base
}

/// <summary>
/// KCS metrics for an agent.
/// </summary>
public class KCSAgentMetrics
{
    public int AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    // Article creation
    public int ArticlesCreated { get; set; }
    public int ArticlesPublished { get; set; }
    public int ArticlesRejected { get; set; }
    public double ArticleApprovalRate { get; set; }

    // Article reuse
    public int ArticlesReused { get; set; }
    public int IncidentsSolvedWithArticle { get; set; }
    public double ReuseSuccessRate { get; set; }

    // Quality metrics
    public double AverageArticleQualityScore { get; set; }
    public int CoachingSessionsReceived { get; set; }

    // KCS maturity
    public KCSMaturityLevel MaturityLevel { get; set; }
    public List<KCSBadge> EarnedBadges { get; set; } = new();
}

public enum KCSMaturityLevel
{
    Learner,
    Contributor,
    Publisher,
    Coach,
    Expert
}

public class KCSBadge
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EarnedAt { get; set; }
}

/// <summary>
/// Article lifecycle information.
/// </summary>
public class KCSArticleLifecycle
{
    public int ArticleId { get; set; }
    public string ArticleNumber { get; set; } = string.Empty;
    public KCSLifecycleStage CurrentStage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FirstPublishedAt { get; set; }
    public DateTime? LastReviewedAt { get; set; }
    public DateTime? NextReviewDue { get; set; }
    public int TotalViews { get; set; }
    public int TotalReuses { get; set; }
    public int RevisionCount { get; set; }
    public List<LifecycleEvent> Events { get; set; } = new();
}

public enum KCSLifecycleStage
{
    Draft,
    InReview,
    Published,
    NeedsUpdate,
    Deprecated,
    Archived
}

public class LifecycleEvent
{
    public string Event { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string? PerformedBy { get; set; }
    public string? Details { get; set; }
}

/// <summary>
/// Flag types for articles.
/// </summary>
public enum KCSFlag
{
    NeedsUpdate,
    Inaccurate,
    Outdated,
    Duplicate,
    SuggestRetirement,
    NeedsExpansion,
    Feedback
}

/// <summary>
/// Coaching queue item for reviewers.
/// </summary>
public class KCSCoachingItem
{
    public int ItemId { get; set; }
    public int ArticleId { get; set; }
    public string ArticleTitle { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public CoachingReason Reason { get; set; }
    public DateTime IdentifiedAt { get; set; }
    public int Priority { get; set; }
    public string? Notes { get; set; }
}

public enum CoachingReason
{
    NewAuthor,
    LowQualityScore,
    FrequentRejections,
    FlaggedByUsers,
    RandomSample
}

/// <summary>
/// Outcome of article reuse.
/// </summary>
public enum ReuseOutcome
{
    SolvedIncident,
    PartiallyHelped,
    NotHelpful,
    NeedUpdateRequired
}

/// <summary>
/// Service for Knowledge-Centered Service workflow management.
/// </summary>
public class KCSWorkflowService : IKCSWorkflowService
{
    private readonly IDbContextResolver _dbContextResolver;
    private readonly ILogger<KCSWorkflowService> _logger;

    // In-memory storage (would be database in production)
    private static readonly List<KCSSession> _sessions = new();
    private static readonly List<KCSDraftArticle> _drafts = new();
    private static readonly List<KCSReviewRequest> _reviewRequests = new();
    private static readonly List<ArticleReuseRecord> _reuseRecords = new();
    private static readonly List<ArticleFlagRecord> _flagRecords = new();
    private static int _nextSessionId = 1;
    private static int _nextDraftId = 1;
    private static int _nextReviewId = 1;

    public KCSWorkflowService(
        IDbContextResolver dbContextResolver,
        ILogger<KCSWorkflowService> logger)
    {
        _dbContextResolver = dbContextResolver;
        _logger = logger;
    }

    public async Task<KCSSession> StartCaptureSessionAsync(int incidentId, int agentId)
    {
        var context = _dbContextResolver.ResolveContext();

        var incident = await context.Incidents
            .FirstOrDefaultAsync(i => i.IncidentId == incidentId);

        if (incident == null)
        {
            throw new ArgumentException($"Incident {incidentId} not found");
        }

        // Check for existing active session
        var existingSession = _sessions.FirstOrDefault(s =>
            s.IncidentId == incidentId && s.State == KCSSessionState.Active);

        if (existingSession != null)
        {
            return existingSession;
        }

        var session = new KCSSession
        {
            SessionId = _nextSessionId++,
            IncidentId = incidentId,
            IncidentNumber = incident.Number,
            AgentId = agentId,
            StartedAt = DateTime.UtcNow,
            State = KCSSessionState.Active
        };

        _sessions.Add(session);

        _logger.LogInformation(
            "Started KCS capture session {SessionId} for incident {IncidentNumber}",
            session.SessionId, incident.Number);

        return session;
    }

    public async Task<KCSDraftArticle> CreateDraftFromIncidentAsync(int incidentId, int authorId)
    {
        var context = _dbContextResolver.ResolveContext();

        var incident = await context.Incidents
            .Include(i => i.Category)
            .Include(i => i.Subcategory)
            .FirstOrDefaultAsync(i => i.IncidentId == incidentId);

        if (incident == null)
        {
            throw new ArgumentException($"Incident {incidentId} not found");
        }

        var author = await context.Users.FindAsync(authorId);

        // Generate draft from incident data
        var draft = new KCSDraftArticle
        {
            DraftId = _nextDraftId++,
            SourceIncidentId = incidentId,
            SourceIncidentNumber = incident.Number,
            Title = GenerateArticleTitle(incident),
            Symptom = incident.Description ?? "User reports issue",
            Environment = GenerateEnvironmentSection(incident),
            Resolution = incident.ResolutionNotes ?? "Resolution steps to be documented",
            Cause = null,
            Keywords = ExtractKeywords(incident),
            Category = incident.Category?.Name,
            AuthorId = authorId,
            AuthorName = author != null ? $"{author.FirstName} {author.LastName}".Trim() : "Unknown",
            CreatedAt = DateTime.UtcNow
        };

        // Assess quality
        draft.QualityAssessment = AssessArticleQuality(draft);

        _drafts.Add(draft);

        // Update session if exists
        var session = _sessions.FirstOrDefault(s =>
            s.IncidentId == incidentId && s.State == KCSSessionState.Active);

        if (session != null)
        {
            session.State = KCSSessionState.ArticleCreated;
            session.CreatedArticleId = draft.DraftId;
            session.EndedAt = DateTime.UtcNow;
            session.WasKnowledgeGap = true;
        }

        _logger.LogInformation(
            "Created KCS draft article {DraftId} from incident {IncidentNumber}",
            draft.DraftId, incident.Number);

        return draft;
    }

    public async Task<KCSReviewRequest> SubmitForReviewAsync(int articleId, int submittedById)
    {
        var context = _dbContextResolver.ResolveContext();

        var article = await context.ITSMKnowledgeArticles
            .FirstOrDefaultAsync(a => a.ArticleId == articleId);

        if (article == null)
        {
            throw new ArgumentException($"Article {articleId} not found");
        }

        var submitter = await context.Users.FindAsync(submittedById);

        var reviewRequest = new KCSReviewRequest
        {
            ReviewRequestId = _nextReviewId++,
            ArticleId = articleId,
            ArticleNumber = article.Number,
            ArticleTitle = article.Title,
            SubmittedById = submittedById,
            SubmittedByName = submitter != null ? $"{submitter.FirstName} {submitter.LastName}".Trim() : "Unknown",
            SubmittedAt = DateTime.UtcNow,
            Type = article.PublishingState == PublishingState.Draft ? ReviewType.NewArticle : ReviewType.UpdatedArticle,
            State = KCSReviewState.Pending,
            DueAt = DateTime.UtcNow.AddDays(3)
        };

        _reviewRequests.Add(reviewRequest);

        // Update article status
        article.PublishingState = PublishingState.Review;
        article.ModifiedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        _logger.LogInformation(
            "Article {ArticleNumber} submitted for review by {Submitter}",
            article.Number, submitter != null ? $"{submitter.FirstName} {submitter.LastName}".Trim() : "Unknown");

        return reviewRequest;
    }

    public async Task<KCSReviewResult> ReviewArticleAsync(
        int reviewRequestId,
        int reviewerId,
        KCSReviewDecision decision,
        string? feedback = null)
    {
        var request = _reviewRequests.FirstOrDefault(r => r.ReviewRequestId == reviewRequestId);
        if (request == null)
        {
            throw new ArgumentException($"Review request {reviewRequestId} not found");
        }

        var context = _dbContextResolver.ResolveContext();
        var reviewer = await context.Users.FindAsync(reviewerId);
        var article = await context.ITSMKnowledgeArticles.FindAsync(request.ArticleId);

        var result = new KCSReviewResult
        {
            ReviewRequestId = reviewRequestId,
            ArticleId = request.ArticleId,
            Decision = decision,
            ReviewerId = reviewerId,
            ReviewerName = reviewer != null ? $"{reviewer.FirstName} {reviewer.LastName}".Trim() : "Unknown",
            ReviewedAt = DateTime.UtcNow,
            Feedback = feedback
        };

        // Update request state
        request.State = decision switch
        {
            KCSReviewDecision.Approve => KCSReviewState.Approved,
            KCSReviewDecision.ApproveWithChanges => KCSReviewState.Approved,
            KCSReviewDecision.Reject => KCSReviewState.Rejected,
            KCSReviewDecision.NeedsWork => KCSReviewState.NeedsWork,
            KCSReviewDecision.Escalate => KCSReviewState.Escalated,
            _ => KCSReviewState.Pending
        };
        request.AssignedReviewerId = reviewerId;
        request.AssignedReviewerName = reviewer != null ? $"{reviewer.FirstName} {reviewer.LastName}".Trim() : "Unknown";

        // Update article status
        if (article != null)
        {
            article.PublishingState = decision switch
            {
                KCSReviewDecision.Approve or KCSReviewDecision.ApproveWithChanges => PublishingState.Approved,
                KCSReviewDecision.Reject => PublishingState.Draft,
                KCSReviewDecision.NeedsWork => PublishingState.Draft,
                _ => article.PublishingState
            };
            article.ModifiedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }

        _logger.LogInformation(
            "Article {ArticleId} reviewed: {Decision} by {Reviewer}",
            request.ArticleId, decision, reviewer != null ? $"{reviewer.FirstName} {reviewer.LastName}".Trim() : "Unknown");

        return result;
    }

    public async Task<KCSPublishResult> PublishArticleAsync(
        int articleId,
        int publishedById,
        PublishAudience audience)
    {
        var context = _dbContextResolver.ResolveContext();

        var article = await context.ITSMKnowledgeArticles
            .FirstOrDefaultAsync(a => a.ArticleId == articleId);

        if (article == null)
        {
            return new KCSPublishResult
            {
                ArticleId = articleId,
                Success = false,
                Message = "Article not found"
            };
        }

        if (article.PublishingState != PublishingState.Approved)
        {
            return new KCSPublishResult
            {
                ArticleId = articleId,
                ArticleNumber = article.Number,
                Success = false,
                Message = "Article must be approved before publishing"
            };
        }

        article.PublishingState = PublishingState.Published;
        article.PublishedDate = DateTime.UtcNow;
        article.PublishedById = publishedById;
        article.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        _logger.LogInformation(
            "Article {ArticleNumber} published to {Audience}",
            article.Number, audience);

        return new KCSPublishResult
        {
            ArticleId = articleId,
            ArticleNumber = article.Number,
            Success = true,
            Audience = audience,
            PublishedAt = DateTime.UtcNow,
            PublishedById = publishedById,
            Message = $"Article published to {audience}"
        };
    }

    public async Task<KCSAgentMetrics> GetAgentMetricsAsync(
        int agentId,
        DateTime fromDate,
        DateTime toDate)
    {
        var context = _dbContextResolver.ResolveContext();
        var agent = await context.Users.FindAsync(agentId);

        // Get articles created by agent
        var articles = await context.ITSMKnowledgeArticles
            .Where(a => a.AuthorId == agentId)
            .Where(a => a.CreatedAt >= fromDate && a.CreatedAt <= toDate)
            .ToListAsync();

        var reuses = _reuseRecords
            .Where(r => r.AgentId == agentId)
            .Where(r => r.RecordedAt >= fromDate && r.RecordedAt <= toDate)
            .ToList();

        var reviews = _reviewRequests
            .Where(r => r.SubmittedById == agentId)
            .Where(r => r.SubmittedAt >= fromDate && r.SubmittedAt <= toDate)
            .ToList();

        var metrics = new KCSAgentMetrics
        {
            AgentId = agentId,
            AgentName = agent != null ? $"{agent.FirstName} {agent.LastName}".Trim() : "Unknown",
            FromDate = fromDate,
            ToDate = toDate,
            ArticlesCreated = articles.Count,
            ArticlesPublished = articles.Count(a => a.PublishingState == PublishingState.Published),
            ArticlesRejected = reviews.Count(r => r.State == KCSReviewState.Rejected),
            ArticleApprovalRate = reviews.Count > 0
                ? (double)reviews.Count(r => r.State == KCSReviewState.Approved) / reviews.Count * 100
                : 100,
            ArticlesReused = reuses.Count,
            IncidentsSolvedWithArticle = reuses.Count(r => r.Outcome == ReuseOutcome.SolvedIncident),
            ReuseSuccessRate = reuses.Count > 0
                ? (double)reuses.Count(r => r.Outcome == ReuseOutcome.SolvedIncident) / reuses.Count * 100
                : 0,
            AverageArticleQualityScore = 75, // Would be calculated from quality assessments
            CoachingSessionsReceived = 0,
            MaturityLevel = DetermineMaturityLevel(articles.Count, reuses.Count)
        };

        // Add earned badges
        if (articles.Count >= 10)
        {
            metrics.EarnedBadges.Add(new KCSBadge
            {
                Name = "Article Author",
                Description = "Created 10 or more knowledge articles",
                EarnedAt = DateTime.UtcNow
            });
        }

        if (reuses.Count(r => r.Outcome == ReuseOutcome.SolvedIncident) >= 20)
        {
            metrics.EarnedBadges.Add(new KCSBadge
            {
                Name = "Knowledge Champion",
                Description = "Solved 20 incidents using knowledge articles",
                EarnedAt = DateTime.UtcNow
            });
        }

        return metrics;
    }

    public async Task<KCSArticleLifecycle> GetArticleLifecycleAsync(int articleId)
    {
        var context = _dbContextResolver.ResolveContext();

        var article = await context.ITSMKnowledgeArticles
            .FirstOrDefaultAsync(a => a.ArticleId == articleId);

        if (article == null)
        {
            throw new ArgumentException($"Article {articleId} not found");
        }

        var reviews = _reviewRequests
            .Where(r => r.ArticleId == articleId)
            .OrderByDescending(r => r.SubmittedAt)
            .ToList();

        var reuses = _reuseRecords
            .Where(r => r.ArticleId == articleId)
            .ToList();

        var lifecycle = new KCSArticleLifecycle
        {
            ArticleId = articleId,
            ArticleNumber = article.Number,
            CurrentStage = MapStatusToLifecycleStage(article.PublishingState),
            CreatedAt = article.CreatedAt,
            FirstPublishedAt = article.PublishedDate,
            LastReviewedAt = reviews.FirstOrDefault()?.SubmittedAt,
            NextReviewDue = article.PublishedDate?.AddMonths(6),
            TotalViews = article.ViewCount,
            TotalReuses = reuses.Count,
            RevisionCount = article.Version,
            Events = new List<LifecycleEvent>()
        };

        // Build event timeline
        lifecycle.Events.Add(new LifecycleEvent
        {
            Event = "Created",
            OccurredAt = article.CreatedAt,
            Details = "Article draft created"
        });

        foreach (var review in reviews.OrderBy(r => r.SubmittedAt))
        {
            lifecycle.Events.Add(new LifecycleEvent
            {
                Event = $"Review: {review.State}",
                OccurredAt = review.SubmittedAt,
                PerformedBy = review.AssignedReviewerName
            });
        }

        if (article.PublishedDate.HasValue)
        {
            lifecycle.Events.Add(new LifecycleEvent
            {
                Event = "Published",
                OccurredAt = article.PublishedDate.Value
            });
        }

        return lifecycle;
    }

    public async Task<bool> FlagArticleAsync(
        int articleId,
        KCSFlag flag,
        int flaggedById,
        string reason)
    {
        var context = _dbContextResolver.ResolveContext();

        var article = await context.ITSMKnowledgeArticles.FindAsync(articleId);
        if (article == null) return false;

        _flagRecords.Add(new ArticleFlagRecord
        {
            ArticleId = articleId,
            Flag = flag,
            FlaggedById = flaggedById,
            Reason = reason,
            FlaggedAt = DateTime.UtcNow
        });

        // Create review request for flagged articles
        if (flag is KCSFlag.Inaccurate or KCSFlag.Outdated or KCSFlag.NeedsUpdate)
        {
            var reviewRequest = new KCSReviewRequest
            {
                ReviewRequestId = _nextReviewId++,
                ArticleId = articleId,
                ArticleNumber = article.Number,
                ArticleTitle = article.Title,
                SubmittedById = flaggedById,
                SubmittedAt = DateTime.UtcNow,
                Type = ReviewType.FlaggedForReview,
                State = KCSReviewState.Pending,
                DueAt = DateTime.UtcNow.AddDays(7)
            };
            _reviewRequests.Add(reviewRequest);
        }

        _logger.LogInformation(
            "Article {ArticleNumber} flagged as {Flag}: {Reason}",
            article.Number, flag, reason);

        return await Task.FromResult(true);
    }

    public async Task<List<KCSCoachingItem>> GetCoachingQueueAsync(int reviewerId)
    {
        var context = _dbContextResolver.ResolveContext();
        var coachingItems = new List<KCSCoachingItem>();

        // Find articles from new authors
        var newAuthorArticles = await context.ITSMKnowledgeArticles
            .Include(a => a.Author)
            .Where(a => a.PublishingState == PublishingState.Draft || a.PublishingState == PublishingState.Review)
            .GroupBy(a => a.AuthorId)
            .Where(g => g.Count() <= 3) // New authors with 3 or fewer articles
            .SelectMany(g => g)
            .Take(5)
            .ToListAsync();

        foreach (var article in newAuthorArticles)
        {
            coachingItems.Add(new KCSCoachingItem
            {
                ItemId = article.ArticleId,
                ArticleId = article.ArticleId,
                ArticleTitle = article.Title,
                AuthorId = article.AuthorId,
                AuthorName = article.Author != null ? $"{article.Author.FirstName} {article.Author.LastName}".Trim() : "Unknown",
                Reason = CoachingReason.NewAuthor,
                IdentifiedAt = DateTime.UtcNow,
                Priority = 1,
                Notes = "New author - provide coaching feedback"
            });
        }

        // Add flagged articles
        foreach (var flag in _flagRecords.Where(f => f.Flag == KCSFlag.Inaccurate))
        {
            var article = await context.ITSMKnowledgeArticles
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.ArticleId == flag.ArticleId);

            if (article != null)
            {
                coachingItems.Add(new KCSCoachingItem
                {
                    ItemId = flag.ArticleId * 100,
                    ArticleId = flag.ArticleId,
                    ArticleTitle = article.Title,
                    AuthorId = article.AuthorId,
                    AuthorName = article.Author != null ? $"{article.Author.FirstName} {article.Author.LastName}".Trim() : "Unknown",
                    Reason = CoachingReason.FlaggedByUsers,
                    IdentifiedAt = flag.FlaggedAt,
                    Priority = 2,
                    Notes = flag.Reason
                });
            }
        }

        return coachingItems.OrderBy(c => c.Priority).ToList();
    }

    public async Task RecordArticleReuseAsync(
        int articleId,
        int incidentId,
        ReuseOutcome outcome)
    {
        var context = _dbContextResolver.ResolveContext();

        var incident = await context.Incidents.FindAsync(incidentId);
        var agentId = incident?.AssignedToId ?? 0;

        _reuseRecords.Add(new ArticleReuseRecord
        {
            ArticleId = articleId,
            IncidentId = incidentId,
            AgentId = agentId,
            Outcome = outcome,
            RecordedAt = DateTime.UtcNow
        });

        // Update article view count
        var article = await context.ITSMKnowledgeArticles.FindAsync(articleId);
        if (article != null)
        {
            article.ViewCount++;
            if (outcome == ReuseOutcome.SolvedIncident)
            {
                article.HelpfulCount++;
            }
            await context.SaveChangesAsync();
        }

        // Update session if exists
        var session = _sessions.FirstOrDefault(s =>
            s.IncidentId == incidentId && s.State == KCSSessionState.Active);

        if (session != null)
        {
            session.State = outcome == ReuseOutcome.SolvedIncident
                ? KCSSessionState.ArticleReused
                : KCSSessionState.Active;
            session.UsedArticleId = articleId;

            if (outcome == ReuseOutcome.SolvedIncident)
            {
                session.EndedAt = DateTime.UtcNow;
            }
        }

        _logger.LogInformation(
            "Recorded article {ArticleId} reuse for incident {IncidentId}: {Outcome}",
            articleId, incidentId, outcome);
    }

    private static string GenerateArticleTitle(Incident incident)
    {
        // Generate a searchable title from incident
        var category = incident.Category?.Name ?? "General";
        var keyword = incident.Subcategory?.Name ?? "Issue";

        return $"{category} - {keyword}: How to Resolve";
    }

    private static string GenerateEnvironmentSection(Incident incident)
    {
        var parts = new List<string>();

        if (incident.ConfigurationItemId.HasValue)
        {
            parts.Add($"CI ID: {incident.ConfigurationItemId.Value}");
        }

        if (incident.Category != null)
            parts.Add($"Category: {incident.Category.Name}");

        return parts.Any() ? string.Join("\n", parts) : "Environment details to be documented";
    }

    private static List<string> ExtractKeywords(Incident incident)
    {
        var keywords = new List<string>();

        if (incident.Category != null && !string.IsNullOrEmpty(incident.Category.Name))
            keywords.Add(incident.Category.Name);

        if (incident.Subcategory != null && !string.IsNullOrEmpty(incident.Subcategory.Name))
            keywords.Add(incident.Subcategory.Name);

        // Extract from title
        var titleWords = (incident.ShortDescription ?? "")
            .Split(' ')
            .Where(w => w.Length > 3)
            .Take(3);
        keywords.AddRange(titleWords);

        return keywords.Distinct().ToList();
    }

    private static KCSArticleQuality AssessArticleQuality(KCSDraftArticle draft)
    {
        var quality = new KCSArticleQuality
        {
            HasClearTitle = !string.IsNullOrEmpty(draft.Title) && draft.Title.Length >= 10,
            HasSymptom = !string.IsNullOrEmpty(draft.Symptom) && draft.Symptom.Length >= 20,
            HasEnvironment = !string.IsNullOrEmpty(draft.Environment),
            HasResolution = !string.IsNullOrEmpty(draft.Resolution) && draft.Resolution.Length >= 20,
            HasKeywords = draft.Keywords.Count >= 2,
            ImprovementSuggestions = new List<string>()
        };

        int score = 0;

        if (quality.HasClearTitle) score += 20;
        else quality.ImprovementSuggestions.Add("Add a clear, searchable title");

        if (quality.HasSymptom) score += 25;
        else quality.ImprovementSuggestions.Add("Describe the symptom in detail");

        if (quality.HasEnvironment) score += 15;
        else quality.ImprovementSuggestions.Add("Specify the environment where this applies");

        if (quality.HasResolution) score += 30;
        else quality.ImprovementSuggestions.Add("Document the resolution steps");

        if (quality.HasKeywords) score += 10;
        else quality.ImprovementSuggestions.Add("Add relevant keywords for searchability");

        quality.OverallScore = score;

        return quality;
    }

    private static KCSLifecycleStage MapStatusToLifecycleStage(PublishingState status)
    {
        return status switch
        {
            PublishingState.Draft => KCSLifecycleStage.Draft,
            PublishingState.Review => KCSLifecycleStage.InReview,
            PublishingState.Approved => KCSLifecycleStage.InReview,
            PublishingState.Published => KCSLifecycleStage.Published,
            PublishingState.Retired => KCSLifecycleStage.Archived,
            _ => KCSLifecycleStage.Draft
        };
    }

    private static KCSMaturityLevel DetermineMaturityLevel(int articlesCreated, int articlesReused)
    {
        var total = articlesCreated + articlesReused;

        return total switch
        {
            >= 100 => KCSMaturityLevel.Expert,
            >= 50 => KCSMaturityLevel.Coach,
            >= 20 => KCSMaturityLevel.Publisher,
            >= 5 => KCSMaturityLevel.Contributor,
            _ => KCSMaturityLevel.Learner
        };
    }

    // Helper classes for in-memory storage
    private class ArticleReuseRecord
    {
        public int ArticleId { get; set; }
        public int IncidentId { get; set; }
        public int AgentId { get; set; }
        public ReuseOutcome Outcome { get; set; }
        public DateTime RecordedAt { get; set; }
    }

    private class ArticleFlagRecord
    {
        public int ArticleId { get; set; }
        public KCSFlag Flag { get; set; }
        public int FlaggedById { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime FlaggedAt { get; set; }
    }
}


#endif
