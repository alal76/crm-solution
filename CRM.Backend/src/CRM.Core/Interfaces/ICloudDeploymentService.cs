using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing cloud deployments, providers, and health checks
/// </summary>
public interface ICloudDeploymentService
{
    #region Cloud Provider Management
    
    /// <summary>
    /// Gets all configured cloud providers
    /// </summary>
    Task<IEnumerable<CloudProviderDto>> GetProvidersAsync();
    
    /// <summary>
    /// Gets a specific cloud provider by ID
    /// </summary>
    Task<CloudProviderDto?> GetProviderByIdAsync(int id);
    
    /// <summary>
    /// Creates a new cloud provider configuration
    /// </summary>
    Task<CloudProviderDto> CreateProviderAsync(CreateCloudProviderRequest request);
    
    /// <summary>
    /// Updates an existing cloud provider configuration
    /// </summary>
    Task<CloudProviderDto> UpdateProviderAsync(int id, UpdateCloudProviderRequest request);
    
    /// <summary>
    /// Deletes a cloud provider configuration
    /// </summary>
    Task<bool> DeleteProviderAsync(int id);
    
    /// <summary>
    /// Tests connection to a cloud provider
    /// </summary>
    Task<ProviderConnectionResult> TestProviderConnectionAsync(TestProviderConnectionRequest request);
    
    /// <summary>
    /// Gets available resources for a cloud provider (regions, instance types, etc.)
    /// </summary>
    Task<IEnumerable<ResourceOption>> GetProviderResourcesAsync(int providerId, string resourceType);
    
    #endregion
    
    #region Deployment Management
    
    /// <summary>
    /// Gets all deployments with optional filtering
    /// </summary>
    Task<IEnumerable<CloudDeploymentDto>> GetDeploymentsAsync(int? providerId = null, string? status = null);
    
    /// <summary>
    /// Gets a specific deployment by ID
    /// </summary>
    Task<CloudDeploymentDto?> GetDeploymentByIdAsync(int id);
    
    /// <summary>
    /// Creates a new deployment configuration
    /// </summary>
    Task<CloudDeploymentDto> CreateDeploymentAsync(CreateDeploymentRequest request);
    
    /// <summary>
    /// Updates an existing deployment configuration
    /// </summary>
    Task<CloudDeploymentDto> UpdateDeploymentAsync(int id, UpdateDeploymentRequest request);
    
    /// <summary>
    /// Deletes a deployment configuration
    /// </summary>
    Task<bool> DeleteDeploymentAsync(int id);
    
    /// <summary>
    /// Triggers a new deployment attempt
    /// </summary>
    Task<DeploymentResult> TriggerDeploymentAsync(int deploymentId, TriggerDeploymentRequest request);
    
    /// <summary>
    /// Stops a running deployment
    /// </summary>
    Task<DeploymentResult> StopDeploymentAsync(int deploymentId);
    
    /// <summary>
    /// Restarts a deployment
    /// </summary>
    Task<DeploymentResult> RestartDeploymentAsync(int deploymentId);
    
    /// <summary>
    /// Scales a deployment (updates replicas)
    /// </summary>
    Task<DeploymentResult> ScaleDeploymentAsync(int deploymentId, int replicas);
    
    #endregion
    
    #region Deployment Attempts
    
    /// <summary>
    /// Gets all deployment attempts for a deployment
    /// </summary>
    Task<IEnumerable<DeploymentAttemptDto>> GetDeploymentAttemptsAsync(int deploymentId);
    
    /// <summary>
    /// Gets a specific deployment attempt
    /// </summary>
    Task<DeploymentAttemptDto?> GetDeploymentAttemptByIdAsync(int attemptId);
    
    /// <summary>
    /// Gets the logs for a deployment attempt
    /// </summary>
    Task<string> GetDeploymentAttemptLogsAsync(int attemptId);
    
    #endregion
    
    #region Health Checks
    
    /// <summary>
    /// Runs a health check on a deployment
    /// </summary>
    Task<HealthCheckResult> RunHealthCheckAsync(int deploymentId, RunHealthCheckRequest? request = null);
    
    /// <summary>
    /// Gets the health check history for a deployment
    /// </summary>
    Task<IEnumerable<HealthCheckDto>> GetHealthCheckHistoryAsync(int deploymentId, int? limit = null);
    
    /// <summary>
    /// Gets the latest health status for all deployments
    /// </summary>
    Task<IEnumerable<HealthCheckDto>> GetAllDeploymentHealthAsync();
    
    #endregion
    
    #region Dashboard
    
    /// <summary>
    /// Gets the deployment dashboard summary
    /// </summary>
    Task<DeploymentDashboardDto> GetDashboardAsync();
    
    #endregion
}
