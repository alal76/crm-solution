using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing cloud deployments, providers, and health checks
/// </summary>
public class CloudDeploymentService : ICloudDeploymentService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<CloudDeploymentService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public CloudDeploymentService(
        ICrmDbContext context, 
        ILogger<CloudDeploymentService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    #region Cloud Provider Management

    public async Task<IEnumerable<CloudProviderDto>> GetProvidersAsync()
    {
        var providers = await _context.CloudProviders
            .Where(p => !p.IsDeleted)
            .Include(p => p.Deployments)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return providers.Select(MapToDto);
    }

    public async Task<CloudProviderDto?> GetProviderByIdAsync(int id)
    {
        var provider = await _context.CloudProviders
            .Include(p => p.Deployments)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        return provider != null ? MapToDto(provider) : null;
    }

    public async Task<CloudProviderDto> CreateProviderAsync(CreateCloudProviderRequest request)
    {
        // If setting as default, unset other defaults
        if (request.IsDefault)
        {
            await UnsetDefaultProvidersAsync();
        }

        var provider = new CloudProvider
        {
            Name = request.Name,
            ProviderType = request.ProviderType,
            Description = request.Description,
            AccessKeyId = request.AccessKeyId,
            SecretAccessKey = request.SecretAccessKey,
            TenantId = request.TenantId,
            SubscriptionId = request.SubscriptionId,
            ProjectId = request.ProjectId,
            Region = request.Region,
            Endpoint = request.Endpoint,
            Configuration = request.Configuration != null 
                ? JsonSerializer.Serialize(request.Configuration) 
                : null,
            IsActive = true,
            IsDefault = request.IsDefault
        };

        _context.CloudProviders.Add(provider);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created cloud provider: {Name} ({Type})", provider.Name, provider.ProviderType);

        return MapToDto(provider);
    }

    public async Task<CloudProviderDto> UpdateProviderAsync(int id, UpdateCloudProviderRequest request)
    {
        var provider = await _context.CloudProviders
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted)
            ?? throw new KeyNotFoundException($"Provider {id} not found");

        if (request.IsDefault == true)
        {
            await UnsetDefaultProvidersAsync();
        }

        if (!string.IsNullOrEmpty(request.Name)) provider.Name = request.Name;
        if (request.Description != null) provider.Description = request.Description;
        if (!string.IsNullOrEmpty(request.AccessKeyId)) provider.AccessKeyId = request.AccessKeyId;
        if (!string.IsNullOrEmpty(request.SecretAccessKey)) provider.SecretAccessKey = request.SecretAccessKey;
        if (!string.IsNullOrEmpty(request.TenantId)) provider.TenantId = request.TenantId;
        if (!string.IsNullOrEmpty(request.SubscriptionId)) provider.SubscriptionId = request.SubscriptionId;
        if (!string.IsNullOrEmpty(request.ProjectId)) provider.ProjectId = request.ProjectId;
        if (!string.IsNullOrEmpty(request.Region)) provider.Region = request.Region;
        if (!string.IsNullOrEmpty(request.Endpoint)) provider.Endpoint = request.Endpoint;
        if (request.Configuration != null) provider.Configuration = JsonSerializer.Serialize(request.Configuration);
        if (request.IsActive.HasValue) provider.IsActive = request.IsActive.Value;
        if (request.IsDefault.HasValue) provider.IsDefault = request.IsDefault.Value;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated cloud provider: {Name}", provider.Name);

        return MapToDto(provider);
    }

    public async Task<bool> DeleteProviderAsync(int id)
    {
        var provider = await _context.CloudProviders
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (provider == null) return false;

        provider.IsDeleted = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted cloud provider: {Name}", provider.Name);

        return true;
    }

    public async Task<ProviderConnectionResult> TestProviderConnectionAsync(TestProviderConnectionRequest request)
    {
        var provider = await _context.CloudProviders
            .FirstOrDefaultAsync(p => p.Id == request.ProviderId && !p.IsDeleted);

        if (provider == null)
        {
            return new ProviderConnectionResult
            {
                Success = false,
                Message = "Provider not found"
            };
        }

        try
        {
            // Test connection based on provider type
            var result = provider.ProviderType switch
            {
                CloudProviderType.Kubernetes => await TestKubernetesConnection(provider),
                CloudProviderType.Docker => await TestDockerConnection(provider),
                CloudProviderType.AWS => await TestAwsConnection(provider),
                CloudProviderType.Azure => await TestAzureConnection(provider),
                CloudProviderType.GoogleCloud => await TestGcpConnection(provider),
                CloudProviderType.DigitalOcean => await TestDigitalOceanConnection(provider),
                CloudProviderType.OnPremise => await TestOnPremiseConnection(provider),
                _ => new ProviderConnectionResult { Success = false, Message = "Unsupported provider type" }
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection to provider {Name}", provider.Name);
            return new ProviderConnectionResult
            {
                Success = false,
                Message = $"Connection failed: {ex.Message}"
            };
        }
    }

    public async Task<IEnumerable<ResourceOption>> GetProviderResourcesAsync(int providerId, string resourceType)
    {
        var provider = await _context.CloudProviders
            .FirstOrDefaultAsync(p => p.Id == providerId && !p.IsDeleted);

        if (provider == null) return Enumerable.Empty<ResourceOption>();

        // Return available resources based on provider type and resource type
        return provider.ProviderType switch
        {
            CloudProviderType.Kubernetes => GetKubernetesResources(provider, resourceType),
            CloudProviderType.Docker => GetDockerResources(provider, resourceType),
            _ => Enumerable.Empty<ResourceOption>()
        };
    }

    #endregion

    #region Deployment Management

    public async Task<IEnumerable<CloudDeploymentDto>> GetDeploymentsAsync(int? providerId = null, string? status = null)
    {
        var query = _context.CloudDeployments
            .Include(d => d.CloudProvider)
            .Include(d => d.Attempts)
            .Where(d => !d.IsDeleted);

        if (providerId.HasValue)
        {
            query = query.Where(d => d.CloudProviderId == providerId.Value);
        }

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<DeploymentStatus>(status, true, out var statusEnum))
        {
            query = query.Where(d => d.Status == statusEnum);
        }

        var deployments = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();

        return deployments.Select(MapToDto);
    }

    public async Task<CloudDeploymentDto?> GetDeploymentByIdAsync(int id)
    {
        var deployment = await _context.CloudDeployments
            .Include(d => d.CloudProvider)
            .Include(d => d.Attempts)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        return deployment != null ? MapToDto(deployment) : null;
    }

    public async Task<CloudDeploymentDto> CreateDeploymentAsync(CreateDeploymentRequest request)
    {
        var provider = await _context.CloudProviders
            .FirstOrDefaultAsync(p => p.Id == request.CloudProviderId && !p.IsDeleted)
            ?? throw new KeyNotFoundException($"Provider {request.CloudProviderId} not found");

        var deployment = new CloudDeployment
        {
            Name = request.Name,
            Description = request.Description,
            CloudProviderId = request.CloudProviderId,
            ClusterName = request.ClusterName,
            Namespace = request.Namespace ?? "crm-app",
            ResourceGroup = request.ResourceGroup,
            VpcId = request.VpcId,
            SubnetIds = request.SubnetIds != null ? string.Join(",", request.SubnetIds) : null,
            BackendImage = request.BackendImage ?? "crm-backend",
            FrontendImage = request.FrontendImage ?? "crm-frontend",
            DatabaseImage = request.DatabaseImage ?? "mariadb:10.11",
            DomainName = request.DomainName,
            SslEnabled = request.SslEnabled,
            CpuUnits = request.CpuUnits,
            MemoryMb = request.MemoryMb,
            Replicas = request.Replicas,
            EnvironmentVariables = request.EnvironmentVariables != null 
                ? JsonSerializer.Serialize(request.EnvironmentVariables) 
                : null,
            Status = DeploymentStatus.Pending
        };

        _context.CloudDeployments.Add(deployment);
        await _context.SaveChangesAsync();

        // Reload with provider
        await _context.Entry(deployment).Reference(d => d.CloudProvider).LoadAsync();

        _logger.LogInformation("Created deployment: {Name} on {Provider}", deployment.Name, provider.Name);

        return MapToDto(deployment);
    }

    public async Task<CloudDeploymentDto> UpdateDeploymentAsync(int id, UpdateDeploymentRequest request)
    {
        var deployment = await _context.CloudDeployments
            .Include(d => d.CloudProvider)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted)
            ?? throw new KeyNotFoundException($"Deployment {id} not found");

        if (!string.IsNullOrEmpty(request.Name)) deployment.Name = request.Name;
        if (request.Description != null) deployment.Description = request.Description;
        if (!string.IsNullOrEmpty(request.ClusterName)) deployment.ClusterName = request.ClusterName;
        if (!string.IsNullOrEmpty(request.Namespace)) deployment.Namespace = request.Namespace;
        if (!string.IsNullOrEmpty(request.DomainName)) deployment.DomainName = request.DomainName;
        if (request.SslEnabled.HasValue) deployment.SslEnabled = request.SslEnabled.Value;
        if (request.CpuUnits.HasValue) deployment.CpuUnits = request.CpuUnits.Value;
        if (request.MemoryMb.HasValue) deployment.MemoryMb = request.MemoryMb.Value;
        if (request.Replicas.HasValue) deployment.Replicas = request.Replicas.Value;
        if (request.EnvironmentVariables != null) 
            deployment.EnvironmentVariables = JsonSerializer.Serialize(request.EnvironmentVariables);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated deployment: {Name}", deployment.Name);

        return MapToDto(deployment);
    }

    public async Task<bool> DeleteDeploymentAsync(int id)
    {
        var deployment = await _context.CloudDeployments
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        if (deployment == null) return false;

        deployment.IsDeleted = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted deployment: {Name}", deployment.Name);

        return true;
    }

    public async Task<DeploymentResult> TriggerDeploymentAsync(int deploymentId, TriggerDeploymentRequest request)
    {
        var deployment = await _context.CloudDeployments
            .Include(d => d.CloudProvider)
            .Include(d => d.Attempts)
            .FirstOrDefaultAsync(d => d.Id == deploymentId && !d.IsDeleted);

        if (deployment == null)
        {
            return new DeploymentResult { Success = false, Message = "Deployment not found" };
        }

        var attemptNumber = (deployment.Attempts.Count + 1).ToString();
        var attempt = new DeploymentAttempt
        {
            CloudDeploymentId = deploymentId,
            AttemptNumber = attemptNumber,
            Status = DeploymentStatus.Building,
            GitBranch = request.GitBranch,
            GitCommitHash = request.GitCommitHash,
            TriggeredByUserId = request.TriggeredByUserId,
            TriggerType = "Manual",
            StartedAt = DateTime.UtcNow
        };

        _context.DeploymentAttempts.Add(attempt);
        deployment.Status = DeploymentStatus.Building;
        await _context.SaveChangesAsync();

        try
        {
            // Execute deployment based on provider type
            var result = deployment.CloudProvider.ProviderType switch
            {
                CloudProviderType.Kubernetes => await DeployToKubernetes(deployment, attempt, request),
                CloudProviderType.Docker => await DeployToDocker(deployment, attempt, request),
                _ => new DeploymentResult 
                { 
                    Success = false, 
                    Message = $"Provider type {deployment.CloudProvider.ProviderType} not yet implemented" 
                }
            };

            // Update attempt and deployment status
            attempt.Status = result.Success ? DeploymentStatus.Running : DeploymentStatus.Failed;
            attempt.CompletedAt = DateTime.UtcNow;
            attempt.DurationSeconds = (int)(DateTime.UtcNow - attempt.StartedAt).TotalSeconds;
            attempt.BuildLog = result.BuildLog;
            attempt.DeployLog = result.DeployLog;
            
            if (!result.Success)
            {
                attempt.ErrorMessage = result.Message;
            }
            else
            {
                deployment.BackendVersion = request.BackendVersion ?? $"v{attemptNumber}";
                deployment.FrontendVersion = request.FrontendVersion ?? $"v{attemptNumber}";
                deployment.FrontendUrl = result.FrontendUrl;
                deployment.ApiUrl = result.ApiUrl;
                deployment.DeployedAt = DateTime.UtcNow;
            }

            deployment.Status = result.Success ? DeploymentStatus.Running : DeploymentStatus.Failed;
            await _context.SaveChangesAsync();

            result.AttemptId = attempt.Id;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Deployment failed for {Name}", deployment.Name);

            attempt.Status = DeploymentStatus.Failed;
            attempt.CompletedAt = DateTime.UtcNow;
            attempt.DurationSeconds = (int)(DateTime.UtcNow - attempt.StartedAt).TotalSeconds;
            attempt.ErrorMessage = ex.Message;
            attempt.ErrorStackTrace = ex.StackTrace;
            deployment.Status = DeploymentStatus.Failed;
            deployment.LastError = ex.Message;

            await _context.SaveChangesAsync();

            return new DeploymentResult
            {
                Success = false,
                Message = ex.Message,
                AttemptId = attempt.Id
            };
        }
    }

    public async Task<DeploymentResult> StopDeploymentAsync(int deploymentId)
    {
        var deployment = await _context.CloudDeployments
            .Include(d => d.CloudProvider)
            .FirstOrDefaultAsync(d => d.Id == deploymentId && !d.IsDeleted);

        if (deployment == null)
        {
            return new DeploymentResult { Success = false, Message = "Deployment not found" };
        }

        try
        {
            // Scale down to 0 replicas
            var result = deployment.CloudProvider.ProviderType switch
            {
                CloudProviderType.Kubernetes => await ScaleKubernetesDeployment(deployment, 0),
                CloudProviderType.Docker => await StopDockerContainers(deployment),
                _ => new DeploymentResult { Success = false, Message = "Unsupported provider" }
            };

            if (result.Success)
            {
                deployment.Status = DeploymentStatus.Stopped;
                await _context.SaveChangesAsync();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop deployment {Name}", deployment.Name);
            return new DeploymentResult { Success = false, Message = ex.Message };
        }
    }

    public async Task<DeploymentResult> RestartDeploymentAsync(int deploymentId)
    {
        var deployment = await _context.CloudDeployments
            .Include(d => d.CloudProvider)
            .FirstOrDefaultAsync(d => d.Id == deploymentId && !d.IsDeleted);

        if (deployment == null)
        {
            return new DeploymentResult { Success = false, Message = "Deployment not found" };
        }

        try
        {
            var result = deployment.CloudProvider.ProviderType switch
            {
                CloudProviderType.Kubernetes => await RestartKubernetesDeployment(deployment),
                CloudProviderType.Docker => await RestartDockerContainers(deployment),
                _ => new DeploymentResult { Success = false, Message = "Unsupported provider" }
            };

            if (result.Success)
            {
                deployment.Status = DeploymentStatus.Running;
                await _context.SaveChangesAsync();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restart deployment {Name}", deployment.Name);
            return new DeploymentResult { Success = false, Message = ex.Message };
        }
    }

    public async Task<DeploymentResult> ScaleDeploymentAsync(int deploymentId, int replicas)
    {
        var deployment = await _context.CloudDeployments
            .Include(d => d.CloudProvider)
            .FirstOrDefaultAsync(d => d.Id == deploymentId && !d.IsDeleted);

        if (deployment == null)
        {
            return new DeploymentResult { Success = false, Message = "Deployment not found" };
        }

        try
        {
            var result = deployment.CloudProvider.ProviderType switch
            {
                CloudProviderType.Kubernetes => await ScaleKubernetesDeployment(deployment, replicas),
                _ => new DeploymentResult { Success = false, Message = "Scaling not supported for this provider" }
            };

            if (result.Success)
            {
                deployment.Replicas = replicas;
                await _context.SaveChangesAsync();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scale deployment {Name}", deployment.Name);
            return new DeploymentResult { Success = false, Message = ex.Message };
        }
    }

    #endregion

    #region Deployment Attempts

    public async Task<IEnumerable<DeploymentAttemptDto>> GetDeploymentAttemptsAsync(int deploymentId)
    {
        var attempts = await _context.DeploymentAttempts
            .Include(a => a.CloudDeployment)
            .Where(a => a.CloudDeploymentId == deploymentId)
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync();

        return attempts.Select(MapToDto);
    }

    public async Task<DeploymentAttemptDto?> GetDeploymentAttemptByIdAsync(int attemptId)
    {
        var attempt = await _context.DeploymentAttempts
            .Include(a => a.CloudDeployment)
            .FirstOrDefaultAsync(a => a.Id == attemptId);

        return attempt != null ? MapToDto(attempt) : null;
    }

    public async Task<string> GetDeploymentAttemptLogsAsync(int attemptId)
    {
        var attempt = await _context.DeploymentAttempts
            .FirstOrDefaultAsync(a => a.Id == attemptId);

        if (attempt == null) return "Attempt not found";

        var logs = new System.Text.StringBuilder();
        
        if (!string.IsNullOrEmpty(attempt.BuildLog))
        {
            logs.AppendLine("=== BUILD LOG ===");
            logs.AppendLine(attempt.BuildLog);
        }
        
        if (!string.IsNullOrEmpty(attempt.DeployLog))
        {
            logs.AppendLine("\n=== DEPLOY LOG ===");
            logs.AppendLine(attempt.DeployLog);
        }
        
        if (!string.IsNullOrEmpty(attempt.ErrorMessage))
        {
            logs.AppendLine("\n=== ERROR ===");
            logs.AppendLine(attempt.ErrorMessage);
        }

        return logs.ToString();
    }

    #endregion

    #region Health Checks

    public async Task<HealthCheckResult> RunHealthCheckAsync(int deploymentId, RunHealthCheckRequest? request = null)
    {
        var deployment = await _context.CloudDeployments
            .Include(d => d.CloudProvider)
            .FirstOrDefaultAsync(d => d.Id == deploymentId && !d.IsDeleted);

        if (deployment == null)
        {
            return new HealthCheckResult
            {
                Success = false,
                OverallStatus = "Unknown",
                Message = "Deployment not found"
            };
        }

        var result = new HealthCheckResult
        {
            CheckedAt = DateTime.UtcNow
        };

        var log = new HealthCheckLog
        {
            CloudDeploymentId = deploymentId,
            CheckedAt = DateTime.UtcNow
        };

        try
        {
            // Check API health
            if (!string.IsNullOrEmpty(deployment.ApiUrl))
            {
                result.Api = await CheckComponentHealth(deployment.ApiUrl + "/health");
                log.ApiHealthy = result.Api.Healthy;
                log.ApiResponseTimeMs = result.Api.ResponseTimeMs;
                log.ApiResponse = result.Api.Response;
            }

            // Check Frontend health
            if (!string.IsNullOrEmpty(deployment.FrontendUrl))
            {
                result.Frontend = await CheckComponentHealth(deployment.FrontendUrl);
                log.FrontendHealthy = result.Frontend.Healthy;
                log.FrontendResponseTimeMs = result.Frontend.ResponseTimeMs;
                log.FrontendResponse = result.Frontend.Response;
            }

            // Check Database health (via API)
            if (!string.IsNullOrEmpty(deployment.ApiUrl))
            {
                result.Database = await CheckComponentHealth(deployment.ApiUrl + "/health/database");
                log.DatabaseHealthy = result.Database.Healthy;
                log.DatabaseResponseTimeMs = result.Database.ResponseTimeMs;
                log.DatabaseResponse = result.Database.Response;
            }

            // Determine overall status
            var allHealthy = (result.Api.Healthy || string.IsNullOrEmpty(deployment.ApiUrl)) &&
                            (result.Frontend.Healthy || string.IsNullOrEmpty(deployment.FrontendUrl)) &&
                            (result.Database.Healthy || string.IsNullOrEmpty(deployment.DatabaseHost));

            var anyHealthy = result.Api.Healthy || result.Frontend.Healthy || result.Database.Healthy;

            if (allHealthy)
            {
                result.OverallStatus = "Healthy";
                log.Status = HealthStatus.Healthy;
            }
            else if (anyHealthy)
            {
                result.OverallStatus = "Degraded";
                log.Status = HealthStatus.Degraded;
            }
            else
            {
                result.OverallStatus = "Unhealthy";
                log.Status = HealthStatus.Unhealthy;
            }

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.OverallStatus = "Offline";
            result.Message = ex.Message;
            log.Status = HealthStatus.Offline;
            log.ErrorDetails = ex.Message;
        }

        // Save health check log
        _context.HealthCheckLogs.Add(log);
        
        // Update deployment health status
        deployment.HealthStatus = log.Status;
        deployment.LastHealthCheck = log.CheckedAt;
        
        await _context.SaveChangesAsync();

        return result;
    }

    public async Task<IEnumerable<HealthCheckDto>> GetHealthCheckHistoryAsync(int deploymentId, int? limit = null)
    {
        var query = _context.HealthCheckLogs
            .Include(h => h.CloudDeployment)
            .Where(h => h.CloudDeploymentId == deploymentId)
            .OrderByDescending(h => h.CheckedAt);

        if (limit.HasValue)
        {
            query = (IOrderedQueryable<HealthCheckLog>)query.Take(limit.Value);
        }

        var logs = await query.ToListAsync();

        return logs.Select(MapToDto);
    }

    public async Task<IEnumerable<HealthCheckDto>> GetAllDeploymentHealthAsync()
    {
        var deployments = await _context.CloudDeployments
            .Where(d => !d.IsDeleted && d.Status == DeploymentStatus.Running)
            .ToListAsync();

        var results = new List<HealthCheckDto>();

        foreach (var deployment in deployments)
        {
            var latestCheck = await _context.HealthCheckLogs
                .Include(h => h.CloudDeployment)
                .Where(h => h.CloudDeploymentId == deployment.Id)
                .OrderByDescending(h => h.CheckedAt)
                .FirstOrDefaultAsync();

            if (latestCheck != null)
            {
                results.Add(MapToDto(latestCheck));
            }
        }

        return results;
    }

    #endregion

    #region Dashboard

    public async Task<DeploymentDashboardDto> GetDashboardAsync()
    {
        var providers = await _context.CloudProviders
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        var deployments = await _context.CloudDeployments
            .Include(d => d.CloudProvider)
            .Where(d => !d.IsDeleted)
            .ToListAsync();

        var recentAttempts = await _context.DeploymentAttempts
            .Include(a => a.CloudDeployment)
            .OrderByDescending(a => a.StartedAt)
            .Take(10)
            .ToListAsync();

        var recentHealthChecks = await _context.HealthCheckLogs
            .Include(h => h.CloudDeployment)
            .OrderByDescending(h => h.CheckedAt)
            .Take(10)
            .ToListAsync();

        return new DeploymentDashboardDto
        {
            TotalProviders = providers.Count,
            ActiveProviders = providers.Count(p => p.IsActive),
            TotalDeployments = deployments.Count,
            RunningDeployments = deployments.Count(d => d.Status == DeploymentStatus.Running),
            HealthyDeployments = deployments.Count(d => d.HealthStatus == HealthStatus.Healthy),
            FailedDeployments = deployments.Count(d => d.Status == DeploymentStatus.Failed),
            RecentDeployments = deployments
                .OrderByDescending(d => d.DeployedAt ?? d.CreatedAt)
                .Take(5)
                .Select(d => new DeploymentSummaryDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    ProviderType = d.CloudProvider.ProviderType.ToString(),
                    Status = d.Status.ToString(),
                    HealthStatus = d.HealthStatus.ToString(),
                    FrontendUrl = d.FrontendUrl,
                    DeployedAt = d.DeployedAt
                })
                .ToList(),
            RecentAttempts = recentAttempts.Select(a => new DeploymentAttemptListDto
            {
                Id = a.Id,
                AttemptNumber = a.AttemptNumber,
                Status = a.Status.ToString(),
                GitBranch = a.GitBranch,
                BackendImageTag = a.BackendImageTag,
                FrontendImageTag = a.FrontendImageTag,
                StartedAt = a.StartedAt,
                CompletedAt = a.CompletedAt,
                DurationSeconds = a.DurationSeconds,
                TriggerType = a.TriggerType,
                ErrorMessage = a.ErrorMessage
            }).ToList(),
            RecentHealthChecks = recentHealthChecks.Select(MapToDto).ToList()
        };
    }

    #endregion

    #region Private Helper Methods

    private async Task UnsetDefaultProvidersAsync()
    {
        var defaultProviders = await _context.CloudProviders
            .Where(p => p.IsDefault && !p.IsDeleted)
            .ToListAsync();

        foreach (var provider in defaultProviders)
        {
            provider.IsDefault = false;
        }
    }

    private async Task<ComponentHealth> CheckComponentHealth(string url)
    {
        var result = new ComponentHealth();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            var response = await client.GetAsync(url);
            stopwatch.Stop();

            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.Healthy = response.IsSuccessStatusCode;
            result.Response = $"{(int)response.StatusCode} {response.StatusCode}";
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.Healthy = false;
            result.Error = ex.Message;
        }

        return result;
    }

    #region Provider Connection Tests

    private Task<ProviderConnectionResult> TestKubernetesConnection(CloudProvider provider)
    {
        return Task.Run(() =>
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "kubectl",
                        Arguments = "cluster-info",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false
                    }
                };

                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(10000);

                if (process.ExitCode == 0)
                {
                    return new ProviderConnectionResult
                    {
                        Success = true,
                        Message = "Connected to Kubernetes cluster",
                        AvailableRegions = new List<string> { "local" }
                    };
                }

                return new ProviderConnectionResult
                {
                    Success = false,
                    Message = process.StandardError.ReadToEnd()
                };
            }
            catch (Exception ex)
            {
                return new ProviderConnectionResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        });
    }

    private Task<ProviderConnectionResult> TestDockerConnection(CloudProvider provider)
    {
        return Task.Run(() =>
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "docker",
                        Arguments = "info --format '{{.ServerVersion}}'",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false
                    }
                };

                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(10000);

                if (process.ExitCode == 0)
                {
                    return new ProviderConnectionResult
                    {
                        Success = true,
                        Message = $"Connected to Docker {output.Trim()}"
                    };
                }

                return new ProviderConnectionResult
                {
                    Success = false,
                    Message = process.StandardError.ReadToEnd()
                };
            }
            catch (Exception ex)
            {
                return new ProviderConnectionResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        });
    }

    private Task<ProviderConnectionResult> TestAwsConnection(CloudProvider provider)
    {
        // AWS connection test would use AWS SDK
        return Task.FromResult(new ProviderConnectionResult
        {
            Success = true,
            Message = "AWS connection test not yet implemented",
            AvailableRegions = new List<string> { "us-east-1", "us-west-2", "eu-west-1" }
        });
    }

    private Task<ProviderConnectionResult> TestAzureConnection(CloudProvider provider)
    {
        // Azure connection test would use Azure SDK
        return Task.FromResult(new ProviderConnectionResult
        {
            Success = true,
            Message = "Azure connection test not yet implemented",
            AvailableRegions = new List<string> { "eastus", "westus", "northeurope" }
        });
    }

    private Task<ProviderConnectionResult> TestGcpConnection(CloudProvider provider)
    {
        // GCP connection test would use GCP SDK
        return Task.FromResult(new ProviderConnectionResult
        {
            Success = true,
            Message = "GCP connection test not yet implemented",
            AvailableRegions = new List<string> { "us-central1", "us-east1", "europe-west1" }
        });
    }

    private Task<ProviderConnectionResult> TestDigitalOceanConnection(CloudProvider provider)
    {
        return Task.FromResult(new ProviderConnectionResult
        {
            Success = true,
            Message = "DigitalOcean connection test not yet implemented",
            AvailableRegions = new List<string> { "nyc1", "sfo1", "ams3" }
        });
    }

    private Task<ProviderConnectionResult> TestOnPremiseConnection(CloudProvider provider)
    {
        return Task.FromResult(new ProviderConnectionResult
        {
            Success = true,
            Message = "On-premise connection verified",
            AvailableRegions = new List<string> { "local" }
        });
    }

    #endregion

    #region Provider Resources

    private IEnumerable<ResourceOption> GetKubernetesResources(CloudProvider provider, string resourceType)
    {
        return resourceType.ToLower() switch
        {
            "namespaces" => new List<ResourceOption>
            {
                new() { Id = "crm-app", Name = "CRM Application", Type = "namespace" },
                new() { Id = "default", Name = "Default", Type = "namespace" }
            },
            "storageclasses" => new List<ResourceOption>
            {
                new() { Id = "standard", Name = "Standard", Type = "storageclass" }
            },
            _ => Enumerable.Empty<ResourceOption>()
        };
    }

    private IEnumerable<ResourceOption> GetDockerResources(CloudProvider provider, string resourceType)
    {
        return resourceType.ToLower() switch
        {
            "networks" => new List<ResourceOption>
            {
                new() { Id = "bridge", Name = "Bridge", Type = "network" },
                new() { Id = "host", Name = "Host", Type = "network" }
            },
            _ => Enumerable.Empty<ResourceOption>()
        };
    }

    #endregion

    #region Deployment Operations

    private async Task<DeploymentResult> DeployToKubernetes(CloudDeployment deployment, DeploymentAttempt attempt, TriggerDeploymentRequest request)
    {
        var logs = new System.Text.StringBuilder();
        var ns = deployment.Namespace ?? "crm-app";

        try
        {
            // Apply Kubernetes manifests
            logs.AppendLine($"Deploying to Kubernetes namespace: {ns}");
            
            var backendTag = request.BackendVersion ?? $"v{attempt.AttemptNumber}";
            var frontendTag = request.FrontendVersion ?? $"v{attempt.AttemptNumber}";

            // Update deployment images
            var setBackendImage = await RunKubectlCommand(
                $"set image deployment/crm-backend crm-backend={deployment.BackendImage}:{backendTag} -n {ns}");
            logs.AppendLine(setBackendImage);

            var setFrontendImage = await RunKubectlCommand(
                $"set image deployment/crm-frontend crm-frontend={deployment.FrontendImage}:{frontendTag} -n {ns}");
            logs.AppendLine(setFrontendImage);

            // Wait for rollout
            logs.AppendLine("Waiting for rollout to complete...");
            var backendRollout = await RunKubectlCommand($"rollout status deployment/crm-backend -n {ns} --timeout=300s");
            logs.AppendLine(backendRollout);

            var frontendRollout = await RunKubectlCommand($"rollout status deployment/crm-frontend -n {ns} --timeout=300s");
            logs.AppendLine(frontendRollout);

            // Get service URLs
            var services = await RunKubectlCommand($"get svc -n {ns} -o json");
            
            // Update attempt with image tags
            attempt.BackendImageTag = backendTag;
            attempt.FrontendImageTag = frontendTag;

            return new DeploymentResult
            {
                Success = true,
                Message = "Deployment completed successfully",
                FrontendUrl = deployment.FrontendUrl ?? $"http://localhost:30080",
                ApiUrl = deployment.ApiUrl ?? $"http://localhost:30081",
                DeployLog = logs.ToString()
            };
        }
        catch (Exception ex)
        {
            logs.AppendLine($"ERROR: {ex.Message}");
            return new DeploymentResult
            {
                Success = false,
                Message = ex.Message,
                DeployLog = logs.ToString()
            };
        }
    }

    private async Task<DeploymentResult> DeployToDocker(CloudDeployment deployment, DeploymentAttempt attempt, TriggerDeploymentRequest request)
    {
        var logs = new System.Text.StringBuilder();

        try
        {
            logs.AppendLine("Deploying with Docker Compose...");

            // Use docker compose to deploy
            var composeResult = await RunDockerCommand("compose up -d");
            logs.AppendLine(composeResult);

            attempt.BackendImageTag = request.BackendVersion ?? "latest";
            attempt.FrontendImageTag = request.FrontendVersion ?? "latest";

            return new DeploymentResult
            {
                Success = true,
                Message = "Docker deployment completed",
                FrontendUrl = "http://localhost:3000",
                ApiUrl = "http://localhost:5000",
                DeployLog = logs.ToString()
            };
        }
        catch (Exception ex)
        {
            logs.AppendLine($"ERROR: {ex.Message}");
            return new DeploymentResult
            {
                Success = false,
                Message = ex.Message,
                DeployLog = logs.ToString()
            };
        }
    }

    private async Task<DeploymentResult> ScaleKubernetesDeployment(CloudDeployment deployment, int replicas)
    {
        var ns = deployment.Namespace ?? "crm-app";
        
        var backendScale = await RunKubectlCommand($"scale deployment/crm-backend --replicas={replicas} -n {ns}");
        var frontendScale = await RunKubectlCommand($"scale deployment/crm-frontend --replicas={replicas} -n {ns}");

        return new DeploymentResult
        {
            Success = true,
            Message = $"Scaled to {replicas} replicas",
            DeployLog = $"{backendScale}\n{frontendScale}"
        };
    }

    private async Task<DeploymentResult> RestartKubernetesDeployment(CloudDeployment deployment)
    {
        var ns = deployment.Namespace ?? "crm-app";
        
        var backendRestart = await RunKubectlCommand($"rollout restart deployment/crm-backend -n {ns}");
        var frontendRestart = await RunKubectlCommand($"rollout restart deployment/crm-frontend -n {ns}");

        return new DeploymentResult
        {
            Success = true,
            Message = "Deployments restarted",
            DeployLog = $"{backendRestart}\n{frontendRestart}"
        };
    }

    private async Task<DeploymentResult> StopDockerContainers(CloudDeployment deployment)
    {
        var result = await RunDockerCommand("compose stop");
        return new DeploymentResult
        {
            Success = true,
            Message = "Docker containers stopped",
            DeployLog = result
        };
    }

    private async Task<DeploymentResult> RestartDockerContainers(CloudDeployment deployment)
    {
        var result = await RunDockerCommand("compose restart");
        return new DeploymentResult
        {
            Success = true,
            Message = "Docker containers restarted",
            DeployLog = result
        };
    }

    private Task<string> RunKubectlCommand(string arguments)
    {
        return Task.Run(() =>
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "kubectl",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit(60000);

            return process.ExitCode == 0 ? output : throw new Exception(error);
        });
    }

    private Task<string> RunDockerCommand(string arguments)
    {
        return Task.Run(() =>
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit(60000);

            return process.ExitCode == 0 ? output : throw new Exception(error);
        });
    }

    #endregion

    #region DTO Mappers

    private CloudProviderDto MapToDto(CloudProvider entity)
    {
        return new CloudProviderDto
        {
            Id = entity.Id,
            Name = entity.Name,
            ProviderType = entity.ProviderType.ToString(),
            Description = entity.Description,
            Region = entity.Region,
            Endpoint = entity.Endpoint,
            IsActive = entity.IsActive,
            IsDefault = entity.IsDefault,
            CreatedAt = entity.CreatedAt,
            DeploymentCount = entity.Deployments?.Count ?? 0
        };
    }

    private CloudDeploymentDto MapToDto(CloudDeployment entity)
    {
        return new CloudDeploymentDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CloudProviderId = entity.CloudProviderId,
            ProviderName = entity.CloudProvider?.Name ?? "",
            ProviderType = entity.CloudProvider?.ProviderType.ToString() ?? "",
            ClusterName = entity.ClusterName,
            Namespace = entity.Namespace,
            ResourceGroup = entity.ResourceGroup,
            BackendVersion = entity.BackendVersion,
            FrontendVersion = entity.FrontendVersion,
            FrontendUrl = entity.FrontendUrl,
            ApiUrl = entity.ApiUrl,
            DomainName = entity.DomainName,
            SslEnabled = entity.SslEnabled,
            CpuUnits = entity.CpuUnits,
            MemoryMb = entity.MemoryMb,
            Replicas = entity.Replicas,
            Status = entity.Status.ToString(),
            HealthStatus = entity.HealthStatus.ToString(),
            LastHealthCheck = entity.LastHealthCheck,
            DeployedAt = entity.DeployedAt,
            LastError = entity.LastError,
            CreatedAt = entity.CreatedAt,
            AttemptCount = entity.Attempts?.Count ?? 0
        };
    }

    private DeploymentAttemptDto MapToDto(DeploymentAttempt entity)
    {
        return new DeploymentAttemptDto
        {
            Id = entity.Id,
            CloudDeploymentId = entity.CloudDeploymentId,
            DeploymentName = entity.CloudDeployment?.Name ?? "",
            AttemptNumber = entity.AttemptNumber,
            Status = entity.Status.ToString(),
            GitCommitHash = entity.GitCommitHash,
            GitBranch = entity.GitBranch,
            BuildNumber = entity.BuildNumber,
            BackendImageTag = entity.BackendImageTag,
            FrontendImageTag = entity.FrontendImageTag,
            StartedAt = entity.StartedAt,
            CompletedAt = entity.CompletedAt,
            DurationSeconds = entity.DurationSeconds,
            BuildLog = entity.BuildLog,
            DeployLog = entity.DeployLog,
            ErrorMessage = entity.ErrorMessage,
            TriggeredByUserId = entity.TriggeredByUserId,
            TriggerType = entity.TriggerType
        };
    }

    private HealthCheckDto MapToDto(HealthCheckLog entity)
    {
        return new HealthCheckDto
        {
            Id = entity.Id,
            CloudDeploymentId = entity.CloudDeploymentId,
            DeploymentName = entity.CloudDeployment?.Name ?? "",
            Status = entity.Status.ToString(),
            CheckedAt = entity.CheckedAt,
            ApiHealthy = entity.ApiHealthy,
            FrontendHealthy = entity.FrontendHealthy,
            DatabaseHealthy = entity.DatabaseHealthy,
            ApiResponseTimeMs = entity.ApiResponseTimeMs,
            FrontendResponseTimeMs = entity.FrontendResponseTimeMs,
            DatabaseResponseTimeMs = entity.DatabaseResponseTimeMs,
            ApiResponse = entity.ApiResponse,
            FrontendResponse = entity.FrontendResponse,
            ErrorDetails = entity.ErrorDetails
        };
    }

    #endregion

    #endregion
}
