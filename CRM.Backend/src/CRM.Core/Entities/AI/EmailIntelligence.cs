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

using System.ComponentModel.DataAnnotations.Schema;
namespace CRM.Core.Entities.AI;

#region Email Intelligence Enumerations

/// <summary>
/// Email sentiment.
/// </summary>
public enum EmailSentiment
{
    /// <summary>Very negative sentiment</summary>
    VeryNegative = 0,

    /// <summary>Negative sentiment</summary>
    Negative = 1,

    /// <summary>Neutral sentiment</summary>
    Neutral = 2,

    /// <summary>Positive sentiment</summary>
    Positive = 3,

    /// <summary>Very positive sentiment</summary>
    VeryPositive = 4
}

/// <summary>
/// Email intent classification.
/// </summary>
public enum EmailIntent
{
    /// <summary>General inquiry</summary>
    Inquiry = 0,

    /// <summary>Purchase interest</summary>
    PurchaseIntent = 1,

    /// <summary>Support request</summary>
    SupportRequest = 2,

    /// <summary>Complaint</summary>
    Complaint = 3,

    /// <summary>Feedback</summary>
    Feedback = 4,

    /// <summary>Meeting request</summary>
    MeetingRequest = 5,

    /// <summary>Follow-up</summary>
    FollowUp = 6,

    /// <summary>Cancellation</summary>
    Cancellation = 7,

    /// <summary>Pricing question</summary>
    PricingQuestion = 8,

    /// <summary>Technical question</summary>
    TechnicalQuestion = 9,

    /// <summary>Referral</summary>
    Referral = 10,

    /// <summary>Out of office</summary>
    OutOfOffice = 11,

    /// <summary>Thank you</summary>
    ThankYou = 12,

    /// <summary>Other</summary>
    Other = 99
}

/// <summary>
/// Response urgency level.
/// </summary>
public enum ResponseUrgency
{
    /// <summary>Respond immediately (within 1 hour)</summary>
    Immediate = 0,

    /// <summary>High urgency (within 4 hours)</summary>
    High = 1,

    /// <summary>Normal urgency (within 24 hours)</summary>
    Normal = 2,

    /// <summary>Low urgency (within 48 hours)</summary>
    Low = 3,

    /// <summary>No response needed</summary>
    NoResponse = 4
}

#endregion

/// <summary>
/// AI-generated email intelligence and analysis.
/// </summary>
public class EmailIntelligence : BaseEntity
{
    #region Email Reference

    /// <summary>Original email message ID</summary>
    public string EmailMessageId { get; set; } = string.Empty;

    /// <summary>Communication message ID if stored</summary>
    public int? CommunicationMessageId { get; set; }

    /// <summary>Navigation to CommunicationMessage</summary>
    public CommunicationMessage? CommunicationMessage { get; set; }

    #endregion

    #region Sentiment Analysis

    /// <summary>Overall sentiment</summary>
    public EmailSentiment Sentiment { get; set; }

    /// <summary>Sentiment score (-1 to 1)</summary>
    public decimal SentimentScore { get; set; }

    /// <summary>Sentiment confidence (0-1)</summary>
    public decimal SentimentConfidence { get; set; }

    /// <summary>Emotion detected (JSON with emotions and scores)</summary>
    public string? EmotionsJson { get; set; }

    #endregion

    #region Intent Classification

    /// <summary>Primary intent</summary>
    public EmailIntent PrimaryIntent { get; set; }

    /// <summary>Intent confidence (0-1)</summary>
    public decimal IntentConfidence { get; set; }

    /// <summary>Secondary intents (JSON array)</summary>
    public string? SecondaryIntentsJson { get; set; }

    #endregion

    #region Urgency Assessment

    /// <summary>Response urgency</summary>
    public ResponseUrgency Urgency { get; set; }

    /// <summary>Urgency score (0-100)</summary>
    public decimal UrgencyScore { get; set; }

    /// <summary>Recommended response deadline</summary>
    public DateTime? ResponseDeadline { get; set; }

    #endregion

    #region Entity Extraction

    /// <summary>Extracted entities (JSON - names, companies, products, dates, amounts)</summary>
    public string? ExtractedEntitiesJson { get; set; }

    /// <summary>Mentioned products (JSON array)</summary>
    public string? MentionedProductsJson { get; set; }

    /// <summary>Mentioned competitors (JSON array)</summary>
    public string? MentionedCompetitorsJson { get; set; }

    /// <summary>Key topics discussed (JSON array)</summary>
    public string? TopicsJson { get; set; }

    /// <summary>Action items extracted (JSON array)</summary>
    public string? ActionItemsJson { get; set; }

    /// <summary>Questions asked (JSON array)</summary>
    public string? QuestionsJson { get; set; }

    #endregion

    #region Summary and Response

    /// <summary>AI-generated email summary</summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>Key points (JSON array)</summary>
    public string? KeyPointsJson { get; set; }

    /// <summary>Suggested response</summary>
    public string? SuggestedResponse { get; set; }

    /// <summary>Response talking points (JSON array)</summary>
    public string? ResponseTalkingPointsJson { get; set; }

    /// <summary>Response tone recommendation</summary>
    public string? RecommendedTone { get; set; }

    #endregion

    #region Thread Analysis

    /// <summary>Email thread ID</summary>
    public string? ThreadId { get; set; }

    /// <summary>Position in thread</summary>
    public int? ThreadPosition { get; set; }

    /// <summary>Thread sentiment trend</summary>
    public string? ThreadSentimentTrend { get; set; }

    /// <summary>Unresolved items from thread (JSON)</summary>
    public string? UnresolvedItemsJson { get; set; }

    #endregion

    #region CRM Context

    /// <summary>Related lead ID if identified</summary>
    public int? LeadId { get; set; }

    /// <summary>Related opportunity ID if identified</summary>
    public int? OpportunityId { get; set; }

    /// <summary>Related account ID if identified</summary>
    [Column("AccountId")]
    public int? AccountId { get; set; }

    /// <summary>Related support case ID if identified</summary>
    public int? SupportCaseId { get; set; }

    /// <summary>Impact on opportunity (positive/negative/neutral)</summary>
    public string? OpportunityImpact { get; set; }

    #endregion

    #region Analysis Metadata

    /// <summary>When email was analyzed</summary>
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Processing time in milliseconds</summary>
    public decimal ProcessingTimeMs { get; set; }

    /// <summary>Model version</summary>
    public string ModelVersion { get; set; } = "1.0";

    /// <summary>AI Model used</summary>
    public int? AIModelId { get; set; }

    /// <summary>Navigation to AI Model</summary>
    public AIModel? AIModel { get; set; }

    #endregion
}
