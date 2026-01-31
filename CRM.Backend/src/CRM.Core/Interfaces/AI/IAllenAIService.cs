// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using CRM.Core.Entities;
using CRM.Core.Entities.AI;

namespace CRM.Core.Interfaces.AI;

/// <summary>
/// Interface for AI prediction services using Allen AI models (OLMo, Tulu).
/// </summary>
public interface IAllenAIService
{
    #region Lead Scoring
    
    /// <summary>
    /// Calculate lead score using AI model.
    /// </summary>
    Task<LeadScore> ScoreLeadAsync(int leadId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Batch score multiple leads.
    /// </summary>
    Task<List<LeadScore>> BatchScoreLeadsAsync(IEnumerable<int> leadIds, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get leads ranked by conversion probability.
    /// </summary>
    Task<List<LeadScore>> GetTopLeadsAsync(int count = 10, CancellationToken cancellationToken = default);
    
    #endregion
    
    #region Opportunity Insights
    
    /// <summary>
    /// Generate insights for an opportunity.
    /// </summary>
    Task<OpportunityInsight> GenerateOpportunityInsightAsync(int opportunityId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Predict win probability for an opportunity.
    /// </summary>
    Task<decimal> PredictWinProbabilityAsync(int opportunityId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get at-risk opportunities.
    /// </summary>
    Task<List<OpportunityInsight>> GetAtRiskOpportunitiesAsync(CancellationToken cancellationToken = default);
    
    #endregion
    
    #region Churn Prediction
    
    /// <summary>
    /// Calculate churn risk for a customer.
    /// </summary>
    Task<ChurnRisk> CalculateChurnRiskAsync(int customerId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get high churn risk customers.
    /// </summary>
    Task<List<ChurnRisk>> GetHighChurnRiskCustomersAsync(int count = 10, CancellationToken cancellationToken = default);
    
    #endregion
    
    #region Next Best Action
    
    /// <summary>
    /// Get recommended actions for an entity.
    /// </summary>
    Task<List<ActionRecommendation>> GetRecommendedActionsAsync(
        ActionTargetType targetType, 
        int entityId, 
        int maxActions = 5,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all pending recommendations for a user.
    /// </summary>
    Task<List<ActionRecommendation>> GetUserRecommendationsAsync(
        int userId, 
        int maxActions = 20,
        CancellationToken cancellationToken = default);
    
    #endregion
    
    #region Email Intelligence
    
    /// <summary>
    /// Analyze email content for sentiment, intent, and entities.
    /// </summary>
    Task<EmailIntelligence> AnalyzeEmailAsync(string emailContent, string? subject = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generate suggested response for an email.
    /// </summary>
    Task<string> GenerateEmailResponseAsync(string emailContent, string? context = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Extract action items from email.
    /// </summary>
    Task<List<string>> ExtractActionItemsAsync(string emailContent, CancellationToken cancellationToken = default);
    
    #endregion
    
    #region Text Generation
    
    /// <summary>
    /// Generate text using Allen AI model.
    /// </summary>
    Task<string> GenerateTextAsync(string prompt, int maxTokens = 500, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Summarize text content.
    /// </summary>
    Task<string> SummarizeTextAsync(string content, int maxLength = 200, CancellationToken cancellationToken = default);
    
    #endregion
    
    #region Model Management
    
    /// <summary>
    /// Get available AI models.
    /// </summary>
    Task<List<AIModel>> GetAvailableModelsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get model health status.
    /// </summary>
    Task<bool> CheckModelHealthAsync(AIProvider provider, CancellationToken cancellationToken = default);
    
    #endregion
}

/// <summary>
/// Configuration for Allen AI service.
/// </summary>
public class AllenAIConfiguration
{
    /// <summary>Section name in configuration</summary>
    public const string SectionName = "AllenAI";
    
    /// <summary>Hugging Face API endpoint for OLMo</summary>
    public string OLMoEndpoint { get; set; } = "https://api-inference.huggingface.co/models/allenai/OLMo-7B";
    
    /// <summary>Hugging Face API endpoint for Tulu</summary>
    public string TuluEndpoint { get; set; } = "https://api-inference.huggingface.co/models/allenai/tulu-2-7b";
    
    /// <summary>Hugging Face API key (optional for public models)</summary>
    public string? HuggingFaceApiKey { get; set; }
    
    /// <summary>Default model to use</summary>
    public AIProvider DefaultProvider { get; set; } = AIProvider.AllenAI_OLMo;
    
    /// <summary>Request timeout in seconds</summary>
    public int TimeoutSeconds { get; set; } = 60;
    
    /// <summary>Maximum retries on failure</summary>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>Enable caching of predictions</summary>
    public bool EnableCaching { get; set; } = true;
    
    /// <summary>Cache expiration in minutes</summary>
    public int CacheExpirationMinutes { get; set; } = 60;
    
    /// <summary>Enable fallback to local ML.NET models</summary>
    public bool EnableLocalFallback { get; set; } = true;
    
    /// <summary>Batch size for bulk predictions</summary>
    public int BatchSize { get; set; } = 10;
}
