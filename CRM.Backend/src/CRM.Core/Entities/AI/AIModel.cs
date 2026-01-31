namespace CRM.Core.Entities.AI;

#region AI Model Enumerations

/// <summary>
/// Type of AI model.
/// </summary>
public enum AIModelType
{
    /// <summary>Lead scoring model</summary>
    LeadScoring = 0,
    
    /// <summary>Opportunity win prediction</summary>
    OpportunityWinPrediction = 1,
    
    /// <summary>Churn risk prediction</summary>
    ChurnPrediction = 2,
    
    /// <summary>Next best action recommendation</summary>
    NextBestAction = 3,
    
    /// <summary>Email response generation</summary>
    EmailAssistant = 4,
    
    /// <summary>Sentiment analysis</summary>
    SentimentAnalysis = 5,
    
    /// <summary>Entity extraction</summary>
    EntityExtraction = 6,
    
    /// <summary>Classification model</summary>
    Classification = 7,
    
    /// <summary>Regression model</summary>
    Regression = 8
}

/// <summary>
/// Status of the AI model.
/// </summary>
public enum AIModelStatus
{
    /// <summary>Model is being trained</summary>
    Training = 0,
    
    /// <summary>Model training completed</summary>
    Trained = 1,
    
    /// <summary>Model is active and serving predictions</summary>
    Active = 2,
    
    /// <summary>Model is deprecated</summary>
    Deprecated = 3,
    
    /// <summary>Model training failed</summary>
    Failed = 4,
    
    /// <summary>Model is archived</summary>
    Archived = 5
}

/// <summary>
/// AI provider for inference.
/// </summary>
public enum AIProvider
{
    /// <summary>Allen AI OLMo models (free, open-source)</summary>
    AllenAI_OLMo = 0,
    
    /// <summary>Allen AI Tulu models (instruction-tuned)</summary>
    AllenAI_Tulu = 1,
    
    /// <summary>Local ML.NET model</summary>
    MLNet = 2,
    
    /// <summary>Hugging Face models</summary>
    HuggingFace = 3,
    
    /// <summary>OpenAI (paid)</summary>
    OpenAI = 4,
    
    /// <summary>Anthropic Claude (paid)</summary>
    Anthropic = 5,
    
    /// <summary>Custom model</summary>
    Custom = 6
}

#endregion

/// <summary>
/// Registry of AI models used in the CRM.
/// </summary>
public class AIModel : BaseEntity
{
    #region Identification
    
    /// <summary>Model name</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Model version</summary>
    public string Version { get; set; } = "1.0.0";
    
    /// <summary>Model description</summary>
    public string? Description { get; set; }
    
    /// <summary>Model type</summary>
    public AIModelType ModelType { get; set; }
    
    /// <summary>Model status</summary>
    public AIModelStatus Status { get; set; } = AIModelStatus.Training;
    
    /// <summary>AI provider</summary>
    public AIProvider Provider { get; set; } = AIProvider.AllenAI_OLMo;
    
    #endregion
    
    #region Model Configuration
    
    /// <summary>Model identifier (HuggingFace repo, API endpoint, etc.)</summary>
    public string? ModelIdentifier { get; set; }
    
    /// <summary>Model configuration as JSON</summary>
    public string? ConfigurationJson { get; set; }
    
    /// <summary>Feature columns used by the model (JSON array)</summary>
    public string? FeatureColumnsJson { get; set; }
    
    /// <summary>Target column for predictions</summary>
    public string? TargetColumn { get; set; }
    
    /// <summary>Model hyperparameters (JSON)</summary>
    public string? HyperparametersJson { get; set; }
    
    #endregion
    
    #region Training Metrics
    
    /// <summary>Training accuracy</summary>
    public decimal? TrainingAccuracy { get; set; }
    
    /// <summary>Validation accuracy</summary>
    public decimal? ValidationAccuracy { get; set; }
    
    /// <summary>Test accuracy</summary>
    public decimal? TestAccuracy { get; set; }
    
    /// <summary>AUC-ROC score</summary>
    public decimal? AucRoc { get; set; }
    
    /// <summary>F1 score</summary>
    public decimal? F1Score { get; set; }
    
    /// <summary>Mean absolute error (for regression)</summary>
    public decimal? MeanAbsoluteError { get; set; }
    
    /// <summary>Training samples count</summary>
    public int? TrainingSamplesCount { get; set; }
    
    #endregion
    
    #region Timestamps
    
    /// <summary>When training started</summary>
    public DateTime? TrainingStartedAt { get; set; }
    
    /// <summary>When training completed</summary>
    public DateTime? TrainingCompletedAt { get; set; }
    
    /// <summary>When model was activated</summary>
    public DateTime? ActivatedAt { get; set; }
    
    /// <summary>Last prediction time</summary>
    public DateTime? LastPredictionAt { get; set; }
    
    #endregion
    
    #region Usage Statistics
    
    /// <summary>Total predictions made</summary>
    public long PredictionCount { get; set; } = 0;
    
    /// <summary>Average inference time in milliseconds</summary>
    public decimal? AvgInferenceTimeMs { get; set; }
    
    #endregion
    
    #region Relationships
    
    /// <summary>Predictions made by this model</summary>
    public ICollection<Prediction> Predictions { get; set; } = new List<Prediction>();
    
    #endregion
}

/// <summary>
/// Generic prediction record from any AI model.
/// </summary>
public class Prediction : BaseEntity
{
    #region Identification
    
    /// <summary>Unique prediction ID</summary>
    public string PredictionId { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>Entity type being scored (Lead, Opportunity, Customer)</summary>
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>Entity ID being scored</summary>
    public int EntityId { get; set; }
    
    #endregion
    
    #region Prediction Result
    
    /// <summary>Predicted value (score, probability, category)</summary>
    public decimal PredictedValue { get; set; }
    
    /// <summary>Predicted label for classification</summary>
    public string? PredictedLabel { get; set; }
    
    /// <summary>Confidence score (0-1)</summary>
    public decimal Confidence { get; set; }
    
    /// <summary>Probability distribution (JSON) for multi-class</summary>
    public string? ProbabilitiesJson { get; set; }
    
    #endregion
    
    #region Feature Importance
    
    /// <summary>Top contributing features (JSON)</summary>
    public string? FeatureImportanceJson { get; set; }
    
    /// <summary>Explanation text</summary>
    public string? Explanation { get; set; }
    
    #endregion
    
    #region Timing
    
    /// <summary>When prediction was made</summary>
    public DateTime PredictedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>Inference time in milliseconds</summary>
    public decimal InferenceTimeMs { get; set; }
    
    /// <summary>When prediction expires (needs refresh)</summary>
    public DateTime? ExpiresAt { get; set; }
    
    #endregion
    
    #region Feedback
    
    /// <summary>Actual outcome (for model retraining)</summary>
    public decimal? ActualValue { get; set; }
    
    /// <summary>Actual label</summary>
    public string? ActualLabel { get; set; }
    
    /// <summary>When actual outcome was recorded</summary>
    public DateTime? ActualRecordedAt { get; set; }
    
    /// <summary>Whether prediction was correct</summary>
    public bool? WasCorrect { get; set; }
    
    #endregion
    
    #region Relationships
    
    /// <summary>Model that made this prediction</summary>
    public int AIModelId { get; set; }
    
    /// <summary>Navigation to AI model</summary>
    public AIModel? AIModel { get; set; }
    
    #endregion
}
