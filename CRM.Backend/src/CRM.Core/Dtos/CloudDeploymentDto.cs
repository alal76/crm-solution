using CRM.Core.Entities;

namespace CRM.Core.Dtos;

#region Cloud Provider DTOs

public class CloudProviderDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProviderType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Region { get; set; }
    public string? Endpoint { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DeploymentCount { get; set; }
}

public class CreateCloudProviderRequest
{
    public string Name { get; set; } = string.Empty;
    public CloudProviderType ProviderType { get; set; }
    public string? Description { get; set; }
    public string? AccessKeyId { get; set; }
    public string? SecretAccessKey { get; set; }
    public string? TenantId { get; set; }
    public string? SubscriptionId { get; set; }
    public string? ProjectId { get; set; }
    public string? Region { get; set; }
    public string? Endpoint { get; set; }
    public Dictionary<string, string>? Configuration { get; set; }
    public bool IsDefault { get; set; }
}

public class UpdateCloudProviderRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? AccessKeyId { get; set; }
    public string? SecretAccessKey { get; set; }
    public string? TenantId { get; set; }
    public string? SubscriptionId { get; set; }
    public string? ProjectId { get; set; }
    public string? Region { get; set; }
    public string? Endpoint { get; set; }
    public Dictionary<string, string>? Configuration { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDefault { get; set; }
}

public class TestProviderConnectionRequest
{
    public int ProviderId { get; set; }
}

public class ProviderConnectionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> AvailableRegions { get; set; } = new();
    public List<ResourceOption> AvailableResources { get; set; } = new();
}

public class ResourceOption
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Region { get; set; }
    public string? Status { get; set; }
}

#endregion

#region Cloud Deployment DTOs

public class CloudDeploymentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Provider info
    public int CloudProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderType { get; set; } = string.Empty;
    
    // Target
    public string? ClusterName { get; set; }
    public string? Namespace { get; set; }
    public string? ResourceGroup { get; set; }
    
    // Versions
    public string? BackendVersion { get; set; }
    public string? FrontendVersion { get; set; }
    
    // Endpoints
    public string? FrontendUrl { get; set; }
    public string? ApiUrl { get; set; }
    public string? DomainName { get; set; }
    public bool SslEnabled { get; set; }
    
    // Resources
    public int CpuUnits { get; set; }
    public int MemoryMb { get; set; }
    public int Replicas { get; set; }
    
    // Status
    public string Status { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = string.Empty;
    public DateTime? LastHealthCheck { get; set; }
    public DateTime? DeployedAt { get; set; }
    public string? LastError { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public int AttemptCount { get; set; }
}

public class CreateDeploymentRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CloudProviderId { get; set; }
    
    // Target configuration
    public string? ClusterName { get; set; }
    public string? Namespace { get; set; }
    public string? ResourceGroup { get; set; }
    public string? VpcId { get; set; }
    public List<string>? SubnetIds { get; set; }
    
    // Images
    public string? BackendImage { get; set; }
    public string? FrontendImage { get; set; }
    public string? DatabaseImage { get; set; }
    
    // Domain and SSL
    public string? DomainName { get; set; }
    public bool SslEnabled { get; set; } = true;
    
    // Resources
    public int CpuUnits { get; set; } = 256;
    public int MemoryMb { get; set; } = 512;
    public int Replicas { get; set; } = 1;
    
    // Environment variables
    public Dictionary<string, string>? EnvironmentVariables { get; set; }
}

public class UpdateDeploymentRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ClusterName { get; set; }
    public string? Namespace { get; set; }
    public string? DomainName { get; set; }
    public bool? SslEnabled { get; set; }
    public int? CpuUnits { get; set; }
    public int? MemoryMb { get; set; }
    public int? Replicas { get; set; }
    public Dictionary<string, string>? EnvironmentVariables { get; set; }
}

public class TriggerDeploymentRequest
{
    public int DeploymentId { get; set; }
    public string? BackendVersion { get; set; }
    public string? FrontendVersion { get; set; }
    public string? GitBranch { get; set; }
    public string? GitCommitHash { get; set; }
    public bool ForceBuild { get; set; }
    public int? TriggeredByUserId { get; set; }
}

public class DeploymentResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? AttemptId { get; set; }
    public string? FrontendUrl { get; set; }
    public string? ApiUrl { get; set; }
    public string? BuildLog { get; set; }
    public string? DeployLog { get; set; }
}

#endregion

#region Deployment Attempt DTOs

public class DeploymentAttemptDto
{
    public int Id { get; set; }
    public int CloudDeploymentId { get; set; }
    public string DeploymentName { get; set; } = string.Empty;
    public string AttemptNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    
    // Build info
    public string? GitCommitHash { get; set; }
    public string? GitBranch { get; set; }
    public string? BuildNumber { get; set; }
    public string? BackendImageTag { get; set; }
    public string? FrontendImageTag { get; set; }
    
    // Timing
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? DurationSeconds { get; set; }
    
    // Logs
    public string? BuildLog { get; set; }
    public string? DeployLog { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Trigger
    public int? TriggeredByUserId { get; set; }
    public string? TriggeredByUser { get; set; }
    public string? TriggerType { get; set; }
}

public class DeploymentAttemptListDto
{
    public int Id { get; set; }
    public string AttemptNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? GitBranch { get; set; }
    public string? BackendImageTag { get; set; }
    public string? FrontendImageTag { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? DurationSeconds { get; set; }
    public string? TriggerType { get; set; }
    public string? ErrorMessage { get; set; }
}

#endregion

#region Health Check DTOs

public class HealthCheckDto
{
    public int Id { get; set; }
    public int CloudDeploymentId { get; set; }
    public string DeploymentName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; }
    
    // Component health
    public bool? ApiHealthy { get; set; }
    public bool? FrontendHealthy { get; set; }
    public bool? DatabaseHealthy { get; set; }
    
    // Response times
    public int? ApiResponseTimeMs { get; set; }
    public int? FrontendResponseTimeMs { get; set; }
    public int? DatabaseResponseTimeMs { get; set; }
    
    // Details
    public string? ApiResponse { get; set; }
    public string? FrontendResponse { get; set; }
    public string? ErrorDetails { get; set; }
}

public class RunHealthCheckRequest
{
    public int DeploymentId { get; set; }
}

public class HealthCheckResult
{
    public bool Success { get; set; }
    public string OverallStatus { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; }
    
    public ComponentHealth Api { get; set; } = new();
    public ComponentHealth Frontend { get; set; } = new();
    public ComponentHealth Database { get; set; } = new();
    
    public string? Message { get; set; }
}

public class ComponentHealth
{
    public bool Healthy { get; set; }
    public int ResponseTimeMs { get; set; }
    public string? Response { get; set; }
    public string? Error { get; set; }
}

#endregion

#region Dashboard DTOs

public class DeploymentDashboardDto
{
    public int TotalProviders { get; set; }
    public int ActiveProviders { get; set; }
    public int TotalDeployments { get; set; }
    public int RunningDeployments { get; set; }
    public int HealthyDeployments { get; set; }
    public int FailedDeployments { get; set; }
    
    public List<DeploymentSummaryDto> RecentDeployments { get; set; } = new();
    public List<DeploymentAttemptListDto> RecentAttempts { get; set; } = new();
    public List<HealthCheckDto> RecentHealthChecks { get; set; } = new();
}

public class DeploymentSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProviderType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = string.Empty;
    public string? FrontendUrl { get; set; }
    public DateTime? DeployedAt { get; set; }
}

#endregion
