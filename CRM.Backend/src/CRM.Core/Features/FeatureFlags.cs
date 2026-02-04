// CRM Solution - Pluggable Architecture
// Feature Flags for Provider Selection
//
// ARCHITECTURE NOTE:
// These feature flags control which provider implementation is used at runtime.
// Uses Microsoft.FeatureManagement for industry-standard feature flag support.
// Flags are evaluated at deployment time via appsettings.json configuration.
//
// IMPORTANT: Microsoft.FeatureManagement does NOT allow colons (:) in feature names.
// Use underscores or periods for hierarchical naming.
//
// Pattern: [Feature Flag] → [Provider Factory] → [Provider Implementation]

namespace CRM.Core.Features;

/// <summary>
/// Centralized feature flag definitions following Microsoft.FeatureManagement conventions.
/// These flags control provider selection at deployment time.
/// </summary>
public static class FeatureFlags
{
    #region Provider Selection Flags
    
    /// <summary>
    /// When true, uses external chat provider (Chatwoot/Intercom) instead of BuiltIn.
    /// Configuration key: FeatureManagement:UseExternalChat
    /// </summary>
    public const string UseExternalChat = "UseExternalChat";
    
    /// <summary>
    /// When true, uses external search provider (Meilisearch/Algolia) instead of BuiltIn.
    /// Configuration key: FeatureManagement:UseExternalSearch
    /// </summary>
    public const string UseExternalSearch = "UseExternalSearch";
    
    /// <summary>
    /// When true, uses external notification provider (Novu/Twilio) instead of BuiltIn.
    /// Configuration key: FeatureManagement:UseExternalNotifications
    /// </summary>
    public const string UseExternalNotifications = "UseExternalNotifications";
    
    /// <summary>
    /// When true, uses external analytics provider (Superset/PowerBI) instead of BuiltIn.
    /// Configuration key: FeatureManagement:UseExternalAnalytics
    /// </summary>
    public const string UseExternalAnalytics = "UseExternalAnalytics";
    
    /// <summary>
    /// When true, uses external e-signature provider (DocuSeal/DocuSign) instead of BuiltIn.
    /// Configuration key: FeatureManagement:UseExternalSignatures
    /// </summary>
    public const string UseExternalSignatures = "UseExternalSignatures";
    
    /// <summary>
    /// When true, uses external AI provider (OpenAI/Azure) instead of local Ollama.
    /// Configuration key: FeatureManagement:UseExternalAI
    /// </summary>
    public const string UseExternalAI = "UseExternalAI";
    
    /// <summary>
    /// When true, uses external integration platform (n8n/Zapier).
    /// Configuration key: FeatureManagement:UseExternalIntegrations
    /// </summary>
    public const string UseExternalIntegrations = "UseExternalIntegrations";
    
    #endregion
    
    #region Module Enablement Flags
    
    /// <summary>
    /// When true, ITSM module is enabled.
    /// </summary>
    public const string EnableITSM = "EnableITSM";
    
    /// <summary>
    /// When true, Marketing module is enabled.
    /// </summary>
    public const string EnableMarketing = "EnableMarketing";
    
    /// <summary>
    /// When true, Customer Portal is enabled.
    /// </summary>
    public const string EnableCustomerPortal = "EnableCustomerPortal";
    
    /// <summary>
    /// When true, Partner Portal is enabled.
    /// </summary>
    public const string EnablePartnerPortal = "EnablePartnerPortal";
    
    /// <summary>
    /// When true, Knowledge Base module is enabled.
    /// </summary>
    public const string EnableKnowledgeBase = "EnableKnowledgeBase";
    
    #endregion
    
    #region Feature Rollout Flags (for gradual deployments)
    
    /// <summary>
    /// When true, new search experience is enabled.
    /// </summary>
    public const string NewSearchExperience = "NewSearchExperience";
    
    /// <summary>
    /// When true, AI Assistant is enabled.
    /// </summary>
    public const string AIAssistant = "AIAssistant";
    
    /// <summary>
    /// When true, real-time notifications via SignalR are enabled.
    /// </summary>
    public const string RealTimeNotifications = "RealTimeNotifications";
    
    /// <summary>
    /// When true, advanced workflow automation is enabled.
    /// </summary>
    public const string AdvancedWorkflows = "AdvancedWorkflows";
    
    #endregion
}
