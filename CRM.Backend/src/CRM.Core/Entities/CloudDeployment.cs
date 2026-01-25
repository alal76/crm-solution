namespace CRM.Core.Entities;

/// <summary>
/// Supported cloud providers for deployment
/// </summary>
public enum CloudProviderType
{
    AWS = 1,
    Azure = 2,
    GoogleCloud = 3,
    DigitalOcean = 4,
    Kubernetes = 5,
    Docker = 6,
    OnPremise = 7
}

/// <summary>
/// Deployment status enumeration
/// </summary>
public enum DeploymentStatus
{
    Pending = 0,
    Provisioning = 1,
    Building = 2,
    Deploying = 3,
    Running = 4,
    Stopped = 5,
    Failed = 6,
    Terminated = 7
}

/// <summary>
/// Health check status
/// </summary>
public enum HealthStatus
{
    Unknown = 0,
    Healthy = 1,
    Degraded = 2,
    Unhealthy = 3,
    Offline = 4
}

/// <summary>
/// Cloud provider configuration
/// </summary>
public class CloudProvider : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public CloudProviderType ProviderType { get; set; }
    public string? Description { get; set; }
    
    // Connection credentials (encrypted in DB)
    public string? AccessKeyId { get; set; }
    public string? SecretAccessKey { get; set; }
    public string? TenantId { get; set; }
    public string? SubscriptionId { get; set; }
    public string? ProjectId { get; set; }
    public string? Region { get; set; }
    public string? Endpoint { get; set; }
    
    // Additional configuration as JSON
    public string? Configuration { get; set; }
    
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    
    // Navigation
    public virtual ICollection<CloudDeployment> Deployments { get; set; } = new List<CloudDeployment>();
}

/// <summary>
/// Cloud deployment configuration and target
/// </summary>
public class CloudDeployment : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Provider reference
    public int CloudProviderId { get; set; }
    public virtual CloudProvider CloudProvider { get; set; } = null!;
    
    // Deployment target
    public string? ClusterName { get; set; }
    public string? Namespace { get; set; }
    public string? ResourceGroup { get; set; }
    public string? VpcId { get; set; }
    public string? SubnetIds { get; set; }
    
    // Container/Image details
    public string? BackendImage { get; set; }
    public string? FrontendImage { get; set; }
    public string? DatabaseImage { get; set; }
    public string? BackendVersion { get; set; }
    public string? FrontendVersion { get; set; }
    
    // Deployed endpoints
    public string? FrontendUrl { get; set; }
    public string? ApiUrl { get; set; }
    public string? DatabaseHost { get; set; }
    public int? DatabasePort { get; set; }
    
    // SSL/TLS
    public bool SslEnabled { get; set; } = true;
    public string? SslCertificateArn { get; set; }
    public string? DomainName { get; set; }
    
    // Resource allocation
    public int CpuUnits { get; set; } = 256;
    public int MemoryMb { get; set; } = 512;
    public int Replicas { get; set; } = 1;
    
    // Status
    public DeploymentStatus Status { get; set; } = DeploymentStatus.Pending;
    public HealthStatus HealthStatus { get; set; } = HealthStatus.Unknown;
    public DateTime? LastHealthCheck { get; set; }
    public DateTime? DeployedAt { get; set; }
    public string? LastError { get; set; }
    
    // Configuration as JSON
    public string? EnvironmentVariables { get; set; }
    public string? ResourceConfiguration { get; set; }
    
    // Navigation
    public virtual ICollection<DeploymentAttempt> Attempts { get; set; } = new List<DeploymentAttempt>();
    public virtual ICollection<HealthCheckLog> HealthChecks { get; set; } = new List<HealthCheckLog>();
}

/// <summary>
/// Build and deployment attempt tracking
/// </summary>
public class DeploymentAttempt : BaseEntity
{
    public int CloudDeploymentId { get; set; }
    public virtual CloudDeployment CloudDeployment { get; set; } = null!;
    
    public string AttemptNumber { get; set; } = string.Empty;
    public DeploymentStatus Status { get; set; } = DeploymentStatus.Pending;
    
    // Build info
    public string? GitCommitHash { get; set; }
    public string? GitBranch { get; set; }
    public string? BuildNumber { get; set; }
    
    // Images built
    public string? BackendImageTag { get; set; }
    public string? FrontendImageTag { get; set; }
    
    // Timing
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int? DurationSeconds { get; set; }
    
    // Logs and errors
    public string? BuildLog { get; set; }
    public string? DeployLog { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorStackTrace { get; set; }
    
    // Triggered by
    public int? TriggeredByUserId { get; set; }
    public string? TriggerType { get; set; } // Manual, Webhook, Scheduled
}

/// <summary>
/// Health check history log
/// </summary>
public class HealthCheckLog : BaseEntity
{
    public int CloudDeploymentId { get; set; }
    public virtual CloudDeployment CloudDeployment { get; set; } = null!;
    
    public HealthStatus Status { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    
    // Component health
    public bool? ApiHealthy { get; set; }
    public bool? FrontendHealthy { get; set; }
    public bool? DatabaseHealthy { get; set; }
    
    // Response times (ms)
    public int? ApiResponseTimeMs { get; set; }
    public int? FrontendResponseTimeMs { get; set; }
    public int? DatabaseResponseTimeMs { get; set; }
    
    // Details
    public string? ApiResponse { get; set; }
    public string? FrontendResponse { get; set; }
    public string? DatabaseResponse { get; set; }
    public string? ErrorDetails { get; set; }
}
