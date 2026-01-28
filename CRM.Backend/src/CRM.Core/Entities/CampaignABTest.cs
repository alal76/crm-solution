// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

/// <summary>
/// A/B test type
/// </summary>
public enum ABTestType
{
    SubjectLine = 0,
    FromName = 1,
    Content = 2,
    SendTime = 3,
    PreviewText = 4
}

/// <summary>
/// A/B test metric to measure
/// </summary>
public enum ABTestMetric
{
    OpenRate = 0,
    ClickRate = 1,
    ConversionRate = 2,
    Revenue = 3
}

/// <summary>
/// A/B test status
/// </summary>
public enum ABTestStatus
{
    Draft = 0,
    Running = 1,
    Completed = 2,
    Cancelled = 3
}

/// <summary>
/// Campaign A/B test configuration and results
/// </summary>
public class CampaignABTest : BaseEntity
{
    /// <summary>
    /// The campaign this test belongs to
    /// </summary>
    [Required]
    public int CampaignId { get; set; }
    
    /// <summary>
    /// Name of the test
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string TestName { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of test (subject line, content, etc.)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TestType { get; set; } = "SubjectLine";
    
    /// <summary>
    /// Metric to measure
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TestMetric { get; set; } = "OpenRate";
    
    /// <summary>
    /// Traffic split as JSON: {"A": 50, "B": 50}
    /// </summary>
    public string? TrafficSplit { get; set; }
    
    /// <summary>
    /// Sample size for the test
    /// </summary>
    public int? SampleSize { get; set; }
    
    /// <summary>
    /// Sample percentage of total audience
    /// </summary>
    public decimal? SamplePercentage { get; set; }
    
    /// <summary>
    /// Variant configurations as JSON
    /// </summary>
    public string? VariantConfigs { get; set; }
    
    /// <summary>
    /// The winning variant (A, B, C, etc.)
    /// </summary>
    [MaxLength(10)]
    public string? WinnerVariant { get; set; }
    
    /// <summary>
    /// Criteria for selecting winner as JSON
    /// </summary>
    public string? WinningCriteria { get; set; }
    
    /// <summary>
    /// Statistical confidence level percentage
    /// </summary>
    public decimal? ConfidenceLevel { get; set; }
    
    /// <summary>
    /// When the test started
    /// </summary>
    public DateTime? TestStartedAt { get; set; }
    
    /// <summary>
    /// When the test completed
    /// </summary>
    public DateTime? TestCompletedAt { get; set; }
    
    /// <summary>
    /// When the winner was deployed
    /// </summary>
    public DateTime? WinnerDeployedAt { get; set; }
    
    /// <summary>
    /// Automatically select winner
    /// </summary>
    public bool AutoSelectWinner { get; set; } = false;
    
    /// <summary>
    /// Hours after which to auto-select winner
    /// </summary>
    public int? AutoWinnerAfterHours { get; set; }
    
    /// <summary>
    /// Current status
    /// </summary>
    [MaxLength(50)]
    public string Status { get; set; } = "Draft";
    
    // Navigation property
    public virtual MarketingCampaign? Campaign { get; set; }
}
